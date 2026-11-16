#!/usr/bin/env bash
# loop-time.sh <flat> <results>
#
# Question 9 of phase 0: the time of the build and test loop on this machine. It writes <results>/q9-loop-time.tsv.
#   1. It removes the bin and obj folders of each project in <flat>/src, and stops the build servers. The NuGet
#      cache stays as it is.
#   2. It builds src/MapModel/Map_Skia.Tests: the first build.
#   3. It builds the same project again, with nothing changed.
#   4. It runs one test with "dotnet test --no-build". The verdict of the test is recorded and is not a result.
# Each time is wall-clock seconds of one run.
set -uo pipefail
[ $# -eq 2 ] || { echo "usage: loop-time.sh <flat> <results>" >&2; exit 2; }
flat=$(cd "$1" && pwd)
mkdir -p "$2"
results=$(cd "$2" && pwd)
export PATH="$HOME/.dotnet:$PATH" DOTNET_ROOT="${DOTNET_ROOT:-$HOME/.dotnet}" DOTNET_CLI_TELEMETRY_OPTOUT=1 DOTNET_NOLOGO=1
project="$flat/src/MapModel/Map_Skia.Tests"
test_filter="FullyQualifiedName=Map_Skia.Tests.RenderingTests.Fences"
table="$results/q9-loop-time.tsv"
log="$results/q9-loop-time.log"

# Print the wall-clock seconds that a command takes, and append its output to the log. Set $status to its exit code.
measure_seconds() {
    local start end
    start=$(date +%s.%N)
    "$@" >> "$log" 2>&1
    status=$?
    end=$(date +%s.%N)
    seconds=$(awk -v s="$start" -v e="$end" 'BEGIN { printf "%.1f", e - s }')
}

: > "$log"
find "$flat/src" -maxdepth 5 -type d \( -name bin -o -name obj \) -print0 | while IFS= read -r -d '' folder; do
    ls "$(dirname "$folder")"/*.csproj > /dev/null 2>&1 && rm -rf "${folder:?}"
done
dotnet build-server shutdown >> "$log" 2>&1

printf 'step\tcommand\tseconds\texit_code\n' > "$table"
cd "$project"
measure_seconds dotnet build
printf 'first build\tdotnet build\t%s\t%s\n' "$seconds" "$status" >> "$table"
measure_seconds dotnet build
printf 'build with nothing changed\tdotnet build\t%s\t%s\n' "$seconds" "$status" >> "$table"
measure_seconds dotnet test --no-build --filter "$test_filter"
printf 'one test\tdotnet test --no-build --filter %s\t%s\t%s\n' "$test_filter" "$seconds" "$status" >> "$table"
grep -E "Passed!|Failed!|error" "$log" | tail -3
cat "$table"
