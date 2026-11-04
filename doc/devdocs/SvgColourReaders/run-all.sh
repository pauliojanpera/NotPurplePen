#!/bin/bash
# Runs the colour readers over known-answers.svg and over variants of one export.
#   run-all.sh <work folder> <export.svg> <probe>...
# The probes name the export's elements for Inkscape, as inkscape_colours.py takes them; for
# OverprintTestLowerPurple-Course12.svg of the gallery they are
#   o1:fill o1^:fill o9:fill o9^:fill o12:fill o12^:fill o8:fill o8^:fill o2:stroke o2^:stroke o10:stroke o10^:stroke
# Needs inkscape, scribus, xvfb, xdotool, ImageMagick and tesseract-ocr from Ubuntu's packages, and Python 3 with
# Pillow and Playwright's Chromium. Purple Pen's SWOP profile and lookup table are read from feature/svg-target.
set -e
work=$(realpath -m "$1"); export_svg=$(realpath "$2"); shift 2
here=$(cd "$(dirname "$0")" && pwd)
mkdir -p "$work/profiles"
git -C "$here" show origin/feature/svg-target:src/PurplePen/USWebCoatedSWOP.icc > "$work/profiles/USWebCoatedSWOP.icc"
git -C "$here" show origin/feature/svg-target:src/PurplePenCore/Resources/swopsamples.dat > "$work/profiles/swopsamples.dat"
python3 -I "$here/make_profile.py" "$work/profiles/swopsamples.dat" 5 "$work/profiles/swop5.icc"
python3 -I "$here/make_profile.py" naive 2 "$work/profiles/naive2.icc"

python3 -I "$here/make_variants.py" "$here/known-answers.svg" "$work/known" "$work/profiles/USWebCoatedSWOP.icc" \
    "$work/profiles/swop5.icc" > "$work/known-sizes.txt"
python3 -I "$here/make_variants.py" "$export_svg" "$work/export" "$work/profiles/USWebCoatedSWOP.icc" \
    "$work/profiles/swop5.icc" > "$work/export-sizes.txt"

for set in known export; do
    xvfb-run -a scribus -g -ns -py "$here/scribus_colours.py" "$work/scribus-$set" "$work/$set"/*.svg > /dev/null 2>&1
    python3 -I "$here/summarise_scribus.py" "$work/scribus-$set" > "$work/scribus-$set.txt"
done

(cd "$work/export" && python3 -I "$here/render_chromium.py" "$work/chromium" as-now.svg \
    $(ls *.svg | grep -v '^as-now\.svg$')) > "$work/chromium.txt"

: > "$work/inkscape-known.txt"
: > "$work/inkscape-export.txt"
for variant in as-now profile-id profile-linked-no-id profile-linked profile-embedded profile-small style-all attr-paint \
        inkscape-form no-fallback; do
    dbus-run-session -- xvfb-run -a -s "-screen 0 1400x900x24" python3 -I "$here/inkscape_colours.py" \
        "$work/inkscape-known" "$work/known/$variant.svg" a1:fill a2:fill s3:fill s4:fill k3:fill k4:stroke g1:fill \
        d1:fill 2>/dev/null >> "$work/inkscape-known.txt"
done
for variant in as-now profile-id profile-linked profile-embedded profile-small style-all-profile-small \
        style-groups-profile-small class-and-attr-profile-small inkscape-form; do
    dbus-run-session -- xvfb-run -a -s "-screen 0 1400x900x24" python3 -I "$here/inkscape_colours.py" \
        "$work/inkscape-export" "$work/export/$variant.svg" "$@" 2>/dev/null >> "$work/inkscape-export.txt"
done
