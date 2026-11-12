#!/usr/bin/env bash
# readers-union.sh <output folder of browser-union.mjs>
#
# Draws two of browser-union.mjs's layouts, two overlapping areas hatched and two touching areas filled, each way it
# writes them, with Inkscape (its command line's PNG export) and Scribus (../SvgColourReaders/scribus_png.py, under
# Xvfb), into <folder>/inkscape and <folder>/scribus, which `compare-union.py <folder> --readers` then measures. Scribus
# places a file by its size in millimetres, so it gets copies whose root says 63.5mm, the 240 pixels at 96 to the inch.
# Also lists how Inkscape sees the ghosts (--query-all) and writes them back (--export-plain-svg), and what items
# Scribus makes of them (../SvgColourReaders/scribus_colours.py), into <folder>/readers.txt.
# Needs Ubuntu's inkscape, scribus and xvfb packages.
set -euo pipefail
[ $# -eq 1 ] || { echo "usage: readers-union.sh <output folder of browser-union.mjs>" >&2; exit 2; }
folder=$(cd "$1" && pwd)
readers=$(cd "$(dirname "$0")/../SvgColourReaders" && pwd)
documents=()
for layout in overlapping-hatch touching-fill; do
    for way in merged union ghosts each; do
        documents+=("$layout-$way")
    done
done

mkdir -p "$folder/inkscape" "$folder/scribus" "$folder/scribus-in" "$folder/scribus-items"
for name in "${documents[@]}"; do
    inkscape --export-type=png --export-filename="$folder/inkscape/$name.png" --export-width=240 --export-height=240 \
        --export-background=white --export-background-opacity=1 "$folder/svg/$name.svg" > /dev/null 2>&1
    sed 's/width="240" height="240"/width="63.5mm" height="63.5mm"/' "$folder/svg/$name.svg" > "$folder/scribus-in/$name.svg"
done
xvfb-run -a scribus -g -ns -py "$readers/scribus_png.py" 96 "$folder/scribus" \
    $(for name in "${documents[@]}"; do echo "$folder/scribus-in/$name.svg"; done) > /dev/null 2>&1
xvfb-run -a scribus -g -ns -py "$readers/scribus_colours.py" "$folder/scribus-items" \
    "$folder/scribus-in/touching-fill-ghosts.svg" "$folder/scribus-in/touching-fill-union.svg" > /dev/null 2>&1

{
    echo "Inkscape's objects of overlapping-hatch-ghosts.svg:"
    inkscape --query-all "$folder/svg/overlapping-hatch-ghosts.svg" 2>/dev/null
    echo
    echo "Inkscape's plain SVG of it:"
    inkscape "$folder/svg/overlapping-hatch-ghosts.svg" --export-plain-svg --export-filename="$folder/inkscape/roundtrip.svg" > /dev/null 2>&1
    python3 -I -c 'import re, sys; print("\n".join(" ".join(tag.split()) for tag in re.findall(r"<(?:clipPath|use|path)\b[^>]*>", open(sys.argv[1]).read())))' \
        "$folder/inkscape/roundtrip.svg"
    echo
    echo "Scribus's items:"
    for file in "$folder/scribus-items"/*.json; do
        python3 -I -c 'import json, sys; d = json.load(open(sys.argv[1])); print(sys.argv[1].rsplit("/", 1)[1], d["items"])' "$file"
    done
} > "$folder/readers.txt"
echo "drawn into $folder/inkscape and $folder/scribus; see $folder/readers.txt"
