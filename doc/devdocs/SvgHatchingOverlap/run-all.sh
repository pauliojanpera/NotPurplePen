#!/usr/bin/env bash
# run-all.sh <clone> <work folder>
#
# Repeats the measurements of the design's "Overlapping patterns, and a colour's layer" (../SvgExportDesign.md) on
# Linux, into a work folder that must not exist yet:
#   1. a checkout of the prototype, proto/overlapping-patterns at dbaa918, made by ../SvgDevLoop/app-checkout.sh from
#      the clone, and harness/ built against it;
#   2. `harness info` on the case's maps and on teanaway, printarea2 and Madrona.11, taken from the clone's origin/base:
#      the groups of overlapping areas each symbol has; and `harness clips` on the case's maps and printarea2: the union
#      of one area's region against the Skia target's own clip to it;
#   3. suo and suo2 drawn by the Skia target before the change and with it (PURPLEPEN_PATTERNS_ONCE=0 and 1) at
#      100 dpi at the print scale, and suo2 at 100 to 1200 dpi, measured by overlap-ink.py;
#   4. teanaway drawn whole five times each way and timed, with the union of regions and of path operations
#      (PURPLEPEN_UNION_CLIP);
#   5. browser-union.mjs in the Chromium, Firefox and WebKit of the tests' Playwright package, measured by
#      compare-union.py, and readers-union.sh in Inkscape and Scribus, measured by compare-union.py --readers;
#   6. ocad-colour-opacity.py over the clone's OCAD test maps, and the OpenMapper test maps' colours below full opacity.
# The baselines a Tests run moved are measured apart, from its artifacts, by moved-baselines.py.
# Needs the .NET 10 SDK, Python 3 with Pillow, Node, Microsoft.Playwright 1.63.0 in the NuGet cache with its browsers
# under PLAYWRIGHT_BROWSERS_PATH (default /opt/pw-browsers), and for step 5 Ubuntu's inkscape, scribus and xvfb.
set -euo pipefail
[ $# -eq 2 ] || { echo "usage: run-all.sh <clone> <work folder>" >&2; exit 2; }
clone=$(cd "$1" && pwd)
[ ! -e "$2" ] || { echo "$2 exists already" >&2; exit 1; }
mkdir -p "$2"
work=$(cd "$2" && pwd)
here=$(cd "$(dirname "$0")" && pwd)
export DOTNET_ROOT=${DOTNET_ROOT:-/root/.dotnet} DOTNET_CLI_TELEMETRY_OPTOUT=1 DOTNET_NOLOGO=1
dotnet=$DOTNET_ROOT/dotnet

# 1. The prototype and the harness.
git -C "$clone" fetch -q origin proto/overlapping-patterns
bash "$here/../SvgDevLoop/app-checkout.sh" "$clone" dbaa918 "$work/prototype"
mkdir -p "$work/harness"
cp "$here/harness/Program.cs" "$here/harness/Harness.csproj" "$work/harness/"
"$dotnet" build "$work/harness/Harness.csproj" -c Release -o "$work/harness/out" -p:CheckoutRoot="$work/prototype" > "$work/harness/build.log"
harness="$work/harness/out/Harness"

# 2. The groups of overlapping areas.
mkdir -p "$work/maps"
for path in src/MapModel/TestFiles/openmapper/teanaway.omap src/TestFiles/modes/printarea2.omap src/TestFiles/controller/Madrona.11.ocd; do
    git -C "$clone" show "origin/base:$path" > "$work/maps/$(basename "$path")"
done
for map in "$here/case/suo.ocd" "$here/case/suo2.ocd" "$work/maps/teanaway.omap" "$work/maps/printarea2.omap" "$work/maps/Madrona.11.ocd"; do
    echo "== $(basename "$map")"
    "$harness" info "$map"
done > "$work/groups.txt"
for map in "$here/case/suo.ocd" "$here/case/suo2.ocd"; do
    "$harness" clips "$map" 20
done > "$work/clips.txt"
"$harness" clips "$work/maps/printarea2.omap" 3 >> "$work/clips.txt"

# 3. The case drawn before and with the change, and suo2 at several resolutions: the map is 1:15,000 and its page
# prints at 1:10,000, so a millimetre of map is 1.5 mm of paper.
mkdir -p "$work/renders"
for once in 0 1; do
    PURPLEPEN_PATTERNS_ONCE=$once "$harness" render "$here/case/suo.ocd" "$work/renders/suo-once$once.png" 5.9055 0 202.5 198.1 222.7 218.05
    for dpi in 100 150 200 300 600 720 1200; do
        PURPLEPEN_PATTERNS_ONCE=$once "$harness" render "$here/case/suo2.ocd" "$work/renders/res-$dpi-once$once.png" \
            "$(python3 -c "print($dpi / 25.4 * 1.5)")" 0
    done
done > /dev/null
python3 -I "$here/overlap-ink.py" "$work/renders" 0.05905512 100 150 200 300 600 720 1200 > "$work/overlap-ink.md"

# 4. The time of the screen's drawing.
for pixels_per_mm in 2 4; do
    for brushes in 1 0; do
        for way in "0 region" "1 region" "1 pathops"; do
            set -- $way
            echo "pixels/mm $pixels_per_mm, pattern brushes $brushes, once $1, union $2: $(PURPLEPEN_PATTERNS_ONCE=$1 PURPLEPEN_UNION_CLIP=$2 \
                "$harness" time "$work/maps/teanaway.omap" $pixels_per_mm 5 $brushes)"
        done
    done
done > "$work/time-teanaway.txt"

# 5. The browsers, Inkscape and Scribus.
playwright=$HOME/.nuget/packages/microsoft.playwright/1.63.0/.playwright
PLAYWRIGHT_BROWSERS_PATH=${PLAYWRIGHT_BROWSERS_PATH:-/opt/pw-browsers} PLAYWRIGHT_MODULE=$playwright/package/index.js \
    "$playwright/node/linux-x64/node" "$here/browser-union.mjs" "$work/union"
python3 -I "$here/compare-union.py" "$work/union" > "$work/union.md"
"$here/readers-union.sh" "$work/union"
python3 -I "$here/compare-union.py" "$work/union" --readers > "$work/readers.md"

# 6. The colours below full opacity.
(cd "$clone" && find src -iname '*.ocd' -print0 | xargs -0 python3 -I "$here/ocad-colour-opacity.py") > "$work/ocad-opacity.txt"
(cd "$clone" && find src \( -iname '*.omap' -o -iname '*.xmap' \) -print0 | xargs -0 grep -o -E '<color [^>]*opacity="[^"]*"' \
    | grep -v -E 'opacity="1(\.0*)?"' || true) > "$work/openmapper-opacity.txt"

echo "results in $work: groups.txt, clips.txt, overlap-ink.md, time-teanaway.txt, union.md, readers.md, ocad-opacity.txt, openmapper-opacity.txt"
