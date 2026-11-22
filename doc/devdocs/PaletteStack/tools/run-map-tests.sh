#!/usr/bin/env bash
# run-map-tests.sh [--no-build] <integration worktree> <filter> [<results folder>]
#
# Builds LinuxMapModelTestRun in an integration worktree that int-worktree.sh made, and runs the tests of
# Test_MapModel that the filter selects, for example "FullyQualifiedName~InkTests". Prints the outcome of each test
# that did not pass and the totals, and keeps the trx in the results folder, by default results/ in the worktree.
# A test that asks for a rendering is skipped: GDI+ draws the renderings of Test_MapModel, on Windows only.
set -euo pipefail
build=()
if [ "${1:-}" = --no-build ]; then build=(--no-build); shift; fi
[ $# -ge 2 ] || { echo "usage: run-map-tests.sh [--no-build] <integration worktree> <filter> [<results folder>]" >&2; exit 2; }
worktree=$(cd "$1" && pwd) filter=$2
results=${3:-$worktree/results}
mkdir -p "$results"
trx=map-tests-$(date +%Y%m%d-%H%M%S).trx
status=0
"${DOTNET:-$HOME/.dotnet/dotnet}" test "${build[@]}" "$worktree/src/MapModel/LinuxMapModelTestRun/LinuxMapModelTestRun.csproj" \
    --filter "$filter" --logger "trx;LogFileName=$trx" --results-directory "$results" \
    --verbosity quiet --nologo > "$results/${trx%.trx}.log" 2>&1 || status=$?
grep -E "error|Failed |Passed!|Failed!|Skipped |Total tests|warning MSB" "$results/${trx%.trx}.log" | grep -v "warning CS" | tail -40 || true
[ -f "$results/$trx" ] && python3 -I - "$results/$trx" <<'EOF'
import sys
import xml.etree.ElementTree as ElementTree
namespace = "{http://microsoft.com/schemas/VisualStudio/TeamTest/2010}"
counts = {}
for result in ElementTree.parse(sys.argv[1]).getroot().iter(namespace + "UnitTestResult"):
    outcome = result.get("outcome")
    counts[outcome] = counts.get(outcome, 0) + 1
    if outcome not in ("Passed", "NotExecuted"):
        message = result.find("%sOutput/%sErrorInfo/%sMessage" % (namespace, namespace, namespace))
        print(outcome, result.get("testName"), "--", (message.text or "").strip().split("\n")[0][:300] if message is not None else "")
print(counts, sys.argv[1])
EOF
exit $status
