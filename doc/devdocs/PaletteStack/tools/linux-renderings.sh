#!/usr/bin/env bash
# linux-renderings.sh [--no-build] <integration worktree> <results folder> [<filter>]
#
# The local run of the Skia tests on Linux. It runs the tests of Map_Skia.Tests in an integration worktree that
# int-worktree.sh made, with the fonts of the Windows machine of the workflow, and keeps their renderings for
# compare-renderings.py. FONTCONFIG_FILE names the fontconfig file that windows-fonts.sh wrote, by default
# ~/windows-fonts-linux/fonts.conf. The filter selects the tests, as the option --filter of dotnet test does. Without a
# filter, all the tests of Map_Skia.Tests run but those that the run leaves out.
#
# Each test of the run passes. A test that fails is a fault, and the script lists it and exits with 1.
#
# The image comparisons compare with the baselines of the Windows machine, which the texts on Linux do not match
# ("Local tests" in ../Handover.md). So the run sets TEST_COLLECT_DIFFERENCES, and an image comparison does not fail:
# it writes its rendering beside the baseline as <baseline>_actual.png, and the differences as <baseline>_diff.png, when
# they differ. The script moves these files into <results folder>/images, at their paths below src/, and keeps the trx
# of the run in the results folder. It stops before the run if such files are already below src/, as they would mix
# with the renderings of the run. The run sets TEST_SILENTRUN too.
#
# The run leaves out the cases of FontFallbackTests.FallbackFindsAFontFor: they need fonts for scripts that the fonts of
# the branch linux of windows-fonts do not have, for example Han and Devanagari, and they fail on Linux.
#
# A change is judged by its renderings with the change and without it: run the script in the integration worktree of
# each, and give compare-renderings.py the two results folders.
set -euo pipefail
build=()
if [ "${1:-}" = --no-build ]; then build=(--no-build); shift; fi
usage="usage: linux-renderings.sh [--no-build] <integration worktree> <results folder> [<filter>]"
[ $# -ge 2 ] || { echo "$usage" >&2; exit 2; }
worktree=$(cd "$1" && pwd)
left_out='FullyQualifiedName!~FontFallbackTests.FallbackFindsAFontFor'
if [ -n "${3:-}" ]; then filter="($3)&$left_out"; else filter=$left_out; fi
fonts=${FONTCONFIG_FILE:-$HOME/windows-fonts-linux/fonts.conf}
[ -f "$fonts" ] || { echo "no fontconfig file $fonts; make it with windows-fonts.sh" >&2; exit 1; }
[ ! -e "$2/images" ] || { echo "$2/images exists; give a new results folder" >&2; exit 1; }
mkdir -p "$2"
results=$(cd "$2" && pwd)

# The renderings that a run wrote beside the baselines, below src/ and outside the build folders.
renderings() {
    (cd "$worktree/src" && find . \( -name bin -o -name obj \) -prune -o \
        \( -name '*_actual.png' -o -name '*_diff.png' \) -type f -print0)
}
if [ -n "$(renderings | tr -d '\0')" ]; then
    echo "renderings of an earlier run are below $worktree/src:" >&2
    renderings | tr '\0' '\n' | head -5 >&2
    exit 1
fi

options=(--logger "trx;LogFileName=renderings.trx" --results-directory "$results" --verbosity quiet --nologo
         --filter "$filter")
FONTCONFIG_FILE=$fonts TEST_SILENTRUN=True TEST_COLLECT_DIFFERENCES=True "${DOTNET:-$HOME/.dotnet/dotnet}" test \
    "${build[@]}" "$worktree/src/MapModel/Map_Skia.Tests/Map_Skia.Tests.csproj" "${options[@]}" \
    > "$results/renderings.log" 2>&1 || true
grep -E "error|Passed!|Failed!" "$results/renderings.log" | grep -v "warning CS" | tail -5 || true

while IFS= read -r -d '' file; do
    mkdir -p "$results/images/$(dirname "$file")"
    mv "$worktree/src/$file" "$results/images/$file"
done < <(renderings)
mkdir -p "$results/images"
echo "$(find "$results/images" -name '*_actual.png' | wc -l) renderings differ from their baselines, in $results/images"
[ -f "$results/renderings.trx" ] || { echo "the run wrote no trx; see $results/renderings.log" >&2; exit 1; }

# The tests that did not pass, each with the first line of its message.
python3 -I - "$results/renderings.trx" <<'EOF'
import sys
import xml.etree.ElementTree as ElementTree
namespace = "{http://microsoft.com/schemas/VisualStudio/TeamTest/2010}"
failed = []
for result in ElementTree.parse(sys.argv[1]).getroot().iter(namespace + "UnitTestResult"):
    if result.get("outcome") not in ("Passed", "NotExecuted"):
        message = result.find("%sOutput/%sErrorInfo/%sMessage" % (namespace, namespace, namespace))
        text = (message.text or "").strip().split("\n")[0][:200] if message is not None else ""
        failed.append("%s %s -- %s" % (result.get("outcome"), result.get("testName"), text))
for line in failed:
    print(line)
print("%d tests did not pass" % len(failed))
sys.exit(1 if failed else 0)
EOF
