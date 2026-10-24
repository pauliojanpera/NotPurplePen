#!/usr/bin/env bash
# run-svg-tests.sh [--no-build] [--filter <expression>] <worktree> <results folder>
#
# Builds Map_Svg.Tests in a worktree of NotPurplePen and runs its tests as the SVG loop does: SVG_LOOP_RESULTS names
# the results folder, which gets each case's SVG, its measurements, the references not yet recorded and the images
# of the cases outside the tolerance, and the run's test results as all.trx. Then sorts the outcome with triage.py and
# exits with its status: 0 when nothing failed but what waits on a recording or is pending.
#
# The filter is a dotnet test filter, such as "Name~simpletext" or "FullyQualifiedName~RenderingComparisonTests";
# without one, every test of the project runs. The results folder is emptied first.
set -euo pipefail
here=$(cd "$(dirname "$0")" && pwd)
build=1
filter=
while [ $# -gt 0 ]; do
    case "$1" in
        --no-build) build=0; shift ;;
        --filter) filter=$2; shift 2 ;;
        *) break ;;
    esac
done
[ $# -eq 2 ] || { echo "usage: run-svg-tests.sh [--no-build] [--filter <expression>] <worktree> <results folder>" >&2; exit 2; }
worktree=$(cd "$1" && pwd)
mkdir -p "$2"
results=$(cd "$2" && pwd)
project="$worktree/src/MapModel/Map_Svg.Tests/Map_Svg.Tests.csproj"
[ -f "$project" ] || { echo "no Map_Svg.Tests in $worktree" >&2; exit 2; }

if [ -z "${DOTNET_ROOT:-}" ] && [ -x /root/.dotnet/dotnet ]; then export DOTNET_ROOT=/root/.dotnet; fi
dotnet=${DOTNET_ROOT:+$DOTNET_ROOT/}dotnet
export DOTNET_CLI_TELEMETRY_OPTOUT=1 DOTNET_NOLOGO=1

if [ $build = 1 ]; then
    if ! "$dotnet" build "$project" -c Debug > "$results.build.log" 2>&1; then
        grep -E " error " "$results.build.log" | sort -u | head -20 >&2
        echo "the build failed; see $results.build.log" >&2
        exit 1
    fi
fi

find "$results" -mindepth 1 -delete
started=$(date +%s)
SVG_LOOP_RESULTS="$results" "$dotnet" test "$project" -c Debug --no-build ${filter:+--filter "$filter"} \
    --logger "trx;LogFileName=all.trx" --results-directory "$results" > "$results/test.log" 2>&1 || true
grep -E "Passed!|Failed!" "$results/test.log" | sed 's/^ *//' || true
echo "($(( $(date +%s) - started )) s; results in $results)"
[ -f "$results/all.trx" ] || { echo "no test results; see $results/test.log" >&2; exit 1; }
python3 -I "$here/triage.py" "$results/all.trx"
