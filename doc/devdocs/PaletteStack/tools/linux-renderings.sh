#!/usr/bin/env bash
# linux-renderings.sh [--no-build] <integration worktree> <results folder> [<filter>]
#
# Runs the tests of Map_Skia.Tests in an integration worktree that int-worktree.sh made, on Linux with the fonts of
# the Windows machine of the workflow, and keeps their renderings for compare-renderings.py. FONTCONFIG_FILE names the
# fontconfig file that windows-fonts.sh wrote, by default ~/windows-fonts-linux/fonts.conf. The filter selects the
# tests, as the option --filter of dotnet test does. Without a filter, all the tests of Map_Skia.Tests run.
#
# The run sets TEST_SILENTRUN and TEST_COLLECT_DIFFERENCES. So each comparison whose rendering differs from its
# baseline writes the rendering beside the baseline as <baseline>_actual.png, and the differences as
# <baseline>_diff.png, and does not fail. The script moves these files into <results folder>/images, at their paths
# below src/, and keeps the trx of the run in the results folder. It stops before the run if such files are already
# below src/, as they would mix with the renderings of the run.
#
# On Linux, the texts render differently from the baselines of the Windows machine, also with its fonts ("Local
# tests" in ../Progress.md). So a local run judges a change by its renderings with the change and without it: run the
# script in the integration worktree of each, and give compare-renderings.py the two results folders.
set -euo pipefail
build=()
if [ "${1:-}" = --no-build ]; then build=(--no-build); shift; fi
usage="usage: linux-renderings.sh [--no-build] <integration worktree> <results folder> [<filter>]"
[ $# -ge 2 ] || { echo "$usage" >&2; exit 2; }
worktree=$(cd "$1" && pwd)
filter=${3:-}
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

options=(--logger "trx;LogFileName=renderings.trx" --results-directory "$results" --verbosity quiet --nologo)
[ -z "$filter" ] || options+=(--filter "$filter")
status=0
FONTCONFIG_FILE=$fonts TEST_SILENTRUN=True TEST_COLLECT_DIFFERENCES=True "${DOTNET:-$HOME/.dotnet/dotnet}" test \
    "${build[@]}" "$worktree/src/MapModel/Map_Skia.Tests/Map_Skia.Tests.csproj" "${options[@]}" \
    > "$results/renderings.log" 2>&1 || status=$?
grep -E "error|Passed!|Failed!" "$results/renderings.log" | grep -v "warning CS" | tail -5 || true

while IFS= read -r -d '' file; do
    mkdir -p "$results/images/$(dirname "$file")"
    mv "$worktree/src/$file" "$results/images/$file"
done < <(renderings)
mkdir -p "$results/images"
echo "$(find "$results/images" -name '*_actual.png' | wc -l) renderings differ from their baselines, in $results/images"
[ -f "$results/renderings.trx" ] || { echo "the run wrote no trx; see $results/renderings.log" >&2; exit 1; }
exit 0
