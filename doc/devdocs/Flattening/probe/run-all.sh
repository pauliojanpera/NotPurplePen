#!/usr/bin/env bash
# run-all.sh <flat> <results>
#
# Makes each table of doc/devdocs/Flattening/Measurements.md again.
#   <flat>     a checkout of feature/flattening (e6116a7b or later). The probe is built against its src folder and
#              changes no tracked file. Question 9 removes and makes the bin and obj folders of its projects.
#   <results>  the folder that gets the tables, the images and the digests.
#
# The steps:
#   1. loop-time.sh: the time of the build and test loop (question 9). Its name as a stage is "loop-time".
#   2. The build of the probe into <results>/probe-bin.
#   3. "shapes": the shapes that the probe makes (questions 4 and 7).
#   4. For each map, these stages, each in a process of its own:
#        inventory                what the map holds (1). It writes <map>/marks.bin, which the other stages read.
#        tiles                    the marks cut to tiles of 10, 25 and 50 mm (6, 8)
#        tiles-second-run         the tiles of 5, 10 and 25 mm in a second process (5, 6). Two maps only.
#        tiles-uncut              the tiles of 10 and 25 mm, with a run that could not be cut kept as it is (6). Two
#                                 maps only.
#        tiles-long               the tiles of 10 mm with a limit of 900 s (6). One map only.
#        digests                  results made two times in one process (5)
#        digests-second-process   the same in a second process (5)
#        builder                  the union of each run with SKPath.OpBuilder (2)
#        whole-map                the union of each run in two ways, and the knockout over the whole map (2, 3, 8)
#      A stage that passes its time limit is stopped, and its tables keep the rows that it measured.
#   5. tables.py: the tables of the report, to <results>/tables.md.
#
# Environment:
#   MAPS_AT_ONE_TIME   how many maps are measured at the same time (default 1). The measurements of Measurements.md
#                      ran with 4, on a machine of 2 cores.
#   ONLY_MAPS          an extended regular expression. Only the maps whose path matches it are measured.
#   ONLY_STAGES        names of stages, with spaces between them. Only these stages run, and each map must have its
#                      marks.bin already, unless "inventory" is one of them.
set -uo pipefail

# The script is one group, so that bash reads all of it before it runs a line. A change of this file then does not
# reach a run in progress.
{
[ $# -eq 2 ] || { echo "usage: run-all.sh <flat> <results>" >&2; exit 2; }
here=$(cd "$(dirname "$0")" && pwd)
flat=$(cd "$1" && pwd)
mkdir -p "$2"
results=$(cd "$2" && pwd)
export PATH="$HOME/.dotnet:$PATH" DOTNET_ROOT="${DOTNET_ROOT:-$HOME/.dotnet}" DOTNET_CLI_TELEMETRY_OPTOUT=1 DOTNET_NOLOGO=1

# is_stage_wanted <stage>: true when ONLY_STAGES is not set or names the stage.
is_stage_wanted() {
    [ -z "${ONLY_STAGES:-}" ] && return 0
    case " $ONLY_STAGES " in *" $1 "*) return 0 ;; *) return 1 ;; esac
}

is_stage_wanted loop-time && "$here/loop-time.sh" "$flat" "$results"

echo "== building the probe against $flat"
dotnet build "$here/SkiaPathOpsProbe.csproj" -c Release -p:AppSrc="$flat/src" -o "$results/probe-bin" -v quiet 2>&1 | grep -E " error |Build succeeded" | sort -u
probe="$results/probe-bin/SkiaPathOpsProbe.dll"

if is_stage_wanted shapes; then
    echo "== shapes"
    dotnet "$probe" shapes "$results"
fi

# measure_map <map file> <seconds for whole-map> <slow limit> <pairs limit> <more stages>
# Runs the stages of one map. Each line of <map>/stages.tsv gives a stage, its exit code (124: stopped at the limit)
# and its wall-clock seconds. <more stages> names the stages that only some maps have.
measure_map() {
    local map="$1" whole_map_limit="$2" slow_limit="$3" pairs_limit="$4" more_stages="$5"
    local name folder
    name=$(basename "$map"); name="${name%.*}-${name##*.}"; name="${name// /_}"
    folder="$results/$name"
    mkdir -p "$folder"
    [ -z "${ONLY_STAGES:-}" ] && : > "$folder/stages.tsv" && : > "$folder/log.txt"

    # run_stage <stage> <limit in seconds> <arguments of the probe>
    run_stage() {
        local stage="$1" limit="$2" start code; shift 2
        is_stage_wanted "$stage" || return 0
        start=$(date +%s.%N)
        timeout "$limit" dotnet "$probe" "$@" >> "$folder/log.txt" 2>&1
        code=$?
        printf '%s\t%s\t%s\t%s\n' "$name" "$stage" "$code" "$(awk -v s="$start" -v e="$(date +%s.%N)" 'BEGIN { printf "%.1f", e - s }')" >> "$folder/stages.tsv"
    }

    # run_stage_in_folder <stage> <limit> <folder under results> <arguments of the probe>
    # Runs a stage whose results folder is a folder of its own. The folder gets a copy of the marks for the time of
    # the stage.
    run_stage_in_folder() {
        local stage="$1" limit="$2" own="$results/$3"; shift 3
        is_stage_wanted "$stage" || return 0
        mkdir -p "$own/$name"
        cp "$folder/marks.bin" "$own/$name/marks.bin"
        run_stage "$stage" "$limit" "$@"
        rm -f "${own:?}/${name:?}/marks.bin"
    }

    run_stage inventory 1200 inventory "$map" "$results"
    run_stage tiles 900 tiles "$map" "$results" --tile-limit 180
    case " $more_stages " in *" tiles-second-run "*)
        run_stage_in_folder tiles-second-run 1500 tiles-second-run tiles "$map" "$results/tiles-second-run" --tiles 5,10,25 --tile-limit 600 ;;
    esac
    case " $more_stages " in *" tiles-uncut "*)
        run_stage tiles-uncut 1500 tiles "$map" "$results" --tiles 10,25 --tile-limit 600 --keep-uncut ;;
    esac
    case " $more_stages " in *" tiles-long "*)
        run_stage_in_folder tiles-long 1500 tiles-long tiles "$map" "$results/tiles-long" --tiles 10 --tile-limit 900 ;;
    esac
    run_stage digests 600 digests "$map" "$results"
    run_stage_in_folder digests-second-process 600 second-process digests "$map" "$results/second-process"
    run_stage builder 600 builder "$map" "$results" --slow-limit "$slow_limit"
    run_stage whole-map "$whole_map_limit" whole-map "$map" "$results" --slow-limit "$slow_limit" --pairs-limit "$pairs_limit"
    echo "== $name done"
}

at_one_time=${MAPS_AT_ONE_TIME:-1}
running=0
while IFS='|' read -r map whole_map_limit slow_limit pairs_limit more_stages; do
    [[ "$map" =~ ${ONLY_MAPS:-.} ]] || continue
    measure_map "$flat/$map" "$whole_map_limit" "$slow_limit" "$pairs_limit" "$more_stages" &
    running=$((running + 1))
    if [ "$running" -ge "$at_one_time" ]; then wait -n; running=$((running - 1)); fi
done <<'MAPS'
src/MapModel/TestFiles/skia_render/LordHill.ocd|1800|10|600|tiles-second-run tiles-uncut
src/TestFiles/courseprinting/McHugh Creek.omap|900|10|240|tiles-second-run tiles-uncut
src/MapModel/TestFiles/openmapper/TeanawayValley.ocd|480|5|60|
src/MapModel/TestFiles/openmapper/teanaway.omap|480|5|60|tiles-long
MAPS
wait

cat "$results"/*/digests.txt > "$results/digests.txt" 2>/dev/null
[ -d "$results/second-process" ] && cat "$results"/second-process/*/digests.txt > "$results/second-process/digests.txt" 2>/dev/null
python3 -I "$here/tables.py" "$results" > "$results/tables.md"
echo "== the tables are in $results/tables.md"
exit
}
