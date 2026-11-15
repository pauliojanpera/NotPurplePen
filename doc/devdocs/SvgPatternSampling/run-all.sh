#!/usr/bin/env bash
# run-all.sh <checkout> <document.svg> <work folder>
#
# Repeats the measurements of the design's "A fine pattern tile in the browsers" (../SvgExportDesign.md) on Linux,
# into a work folder that must not exist yet:
#   1. cases.py writes the ways of carrying one screen, at a lattice of 2.0 pixels, which teanwest's screen has at the
#      size the rendering comparison draws it, and of 2.169, which stands for any other scale; render.mjs draws each
#      way in Chromium, Firefox and WebKit at the rendering's own size and ten times larger, and measure-cases.py
#      measures them against the resolved drawing;
#   2. locate.py paints each pattern of the given document in turn, render.mjs draws the copies in Chromium, and
#      patches.py chooses a patch that each pattern alone draws on; render.mjs then draws the document itself in the
#      three browsers and each patch ten times larger, and measure-patches.py measures them and the Skia reference of
#      the same case against the resolved patch.
# The document is one the rendering comparison wrote, such as svg/teanwest.svg of a loop run; its reference image is
# the case's in the checkout's src/MapModel/TestFiles/svg_reference. About 10 minutes for a map of teanwest's size.
# Needs Python 3 with NumPy and Pillow, Node, and Microsoft.Playwright 1.63.0 with its browsers, named by
# PLAYWRIGHT_MODULE and PLAYWRIGHT_BROWSERS_PATH.
set -euo pipefail
[ $# -eq 3 ] || { echo "usage: run-all.sh <checkout> <document.svg> <work folder>" >&2; exit 2; }
checkout=$(cd "$1" && pwd)
document=$(cd "$(dirname "$2")" && pwd)/$(basename "$2")
[ ! -e "$3" ] || { echo "$3 exists already" >&2; exit 1; }
mkdir -p "$3"
work=$(cd "$3" && pwd)
here=$(cd "$(dirname "$0")" && pwd)
export PLAYWRIGHT_MODULE=${PLAYWRIGHT_MODULE:-$HOME/.nuget/packages/microsoft.playwright/1.63.0/.playwright/package/index.js}
export PLAYWRIGHT_BROWSERS_PATH=${PLAYWRIGHT_BROWSERS_PATH:-/opt/pw-browsers}
sentinel=$checkout/src/MapModel/Map_Svg.Tests/fonts/Sentinel.ttf
font=$checkout/src/MapModel/Map_Svg.Tests/fonts/LiberationSans-Regular.ttf
name=$(basename "$document" .svg)
reference=$checkout/src/MapModel/TestFiles/svg_reference/$name.png

echo "== the ways of carrying a screen =="
python3 -I "$here/cases.py" "$work/cases" 2.0 2.169
for pitch in "$work"/cases/pitch-*; do
    for case_file in "$pitch"/*.svg; do
        way=$(basename "$case_file" .svg)
        node "$here/render.mjs" "$case_file" 200x200 "$pitch/$way" --sentinel "$sentinel"
        node "$here/render.mjs" "$case_file" 200x200 "$pitch/$way-x10" --scale 10 --browsers chromium \
             --sentinel "$sentinel"
    done
    node "$here/render.mjs" "$pitch/pattern.svg" 200x200 "$pitch/pattern-x10" --scale 10 \
         --browsers firefox,webkit --sentinel "$sentinel"
done
python3 -I "$here/measure-cases.py" "$work/cases" --font "$font" | tee "$work/cases.txt"

echo
echo "== the screens of $name =="
size=$(python3 -I -c 'import re,sys
text=open(sys.argv[1],encoding="utf-8").read(2000)
width=re.search(r"\bwidth=\"([0-9.]+)", text).group(1)
height=re.search(r"\bheight=\"([0-9.]+)", text).group(1)
print("%dx%d" % (float(width), float(height)))' "$document")
mkdir -p "$work/map"
cp "$reference" "$work/map/reference.png"
python3 -I "$here/locate.py" "$document" "$work/map/probes" > "$work/map/patterns.txt"
for probe in "$work/map/probes"/*.svg; do
    node "$here/render.mjs" "$probe" "$size" "$work/map/located" --browsers chromium --sentinel "$sentinel"
    mv "$work/map/located/chromium.png" "$work/map/located/$(basename "$probe" .svg).png"
done
python3 -I "$here/patches.py" "$work/map/located" > "$work/map/patches.tsv"
node "$here/render.mjs" "$document" "$size" "$work/map/renderings" --sentinel "$sentinel"
while IFS=$'\t' read -r pattern x y side rest; do
    node "$here/render.mjs" "$document" "$size" "$work/map/patches/$pattern" --scale 10 --crop "$x,$y,$side,$side" \
         --browsers chromium --sentinel "$sentinel"
done < "$work/map/patches.tsv"
python3 -I "$here/measure-patches.py" "$work/map" "$work/map/patches.tsv" --font "$font" | tee "$work/patches.txt"
