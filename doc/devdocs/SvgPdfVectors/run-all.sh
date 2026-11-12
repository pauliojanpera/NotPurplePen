#!/usr/bin/env bash
# run-all.sh <checkout> <out folder> [<pdf>...]
#
# Measures every way of drawing a PDF base map into the SVG export, on the PDFs that the test events use as maps
# (or on the PDFs given, as paths within the checkout's src/TestFiles):
#   1. inventory.py: what each page holds that the SVG has to keep;
#   2. the probe: the application's own drawing of the page at 150 pixels to the inch (the reference), the SVG the
#      export writes today (PDFium's bitmap), and the SVG of the vector way (PdfPig into the SVG target's calls),
#      plain and with overprinting drawn darker, each written twice to check that the bytes repeat;
#   3. converters.sh: pdftocairo -svg and mutool convert -F svg;
#   4. render.mjs: every SVG in Chromium, Firefox and WebKit, as the tests render;
#   5. compare.py: every rendering held to the reference by F-1's rule;
# and writes <out>/summary.tsv with one row for each PDF, way and browser.
#
# <checkout> is a checkout of feature/svg-target (267efc8 or later) whose src/ the probe is built against. Needs the
# .NET 10 SDK, the tests' Playwright browsers (PLAYWRIGHT_BROWSERS_PATH, ../SvgTargetProgress.md says how to install
# them), poppler-utils, mupdf-tools, Python 3 with NumPy, SciPy, Pillow and pikepdf.
set -euo pipefail
[ $# -ge 2 ] || { echo "usage: run-all.sh <checkout> <out folder> [<pdf>...]" >&2; exit 2; }
here=$(cd "$(dirname "$0")" && pwd)
checkout=$(cd "$1" && pwd)
mkdir -p "$2"
out=$(cd "$2" && pwd)
shift 2
dotnet=${DOTNET:-$(command -v dotnet || echo /root/.dotnet/dotnet)}
ppi=150
tests=$checkout/src/TestFiles
if [ $# -eq 0 ]; then
    # The maps of the test events that name a PDF (pdfcourse, bitmapcreate, courseprinting), and the vector PDFs of
    # the PDF converter's own tests.
    set -- "pdfcourse/ST_Pauls_school_bald_hills_4000 Gordon v10.5.pdf" "bitmapcreate/St. Edward.pdf" \
           "pdfcourse/UWLetter.pdf" "pdfcourse/tengesdalslia.pdf" "pdfcourse/Lincoln-CMYK.pdf" "pdfcourse/Lincoln-RGB.pdf" \
           "pdfcourse/CleElumRidge.pdf" "pdfmaps/Potholes.pdf"
fi

echo "== building the probe against $checkout"
"$dotnet" build "$here/probe/PdfVectorsProbe.csproj" -p:AppSrc="$checkout/src" -o "$out/probe-bin" -v quiet -nologo 2>&1 | grep -E 'error|Warn|Build succeeded' | sort -u
probe() { "$dotnet" "$out/probe-bin/PdfVectorsProbe.dll" "$@"; }

: > "$out/measurements.jsonl"
jobs="$out/jobs.json"
echo "[" > "$jobs"
first_job=true
for relative in "$@"; do
    pdf="$tests/$relative"
    name=$(basename "$relative" .pdf | tr ' .' '__')
    folder="$out/$name"
    mkdir -p "$folder"
    echo "== $relative"
    python3 -I "$here/inventory.py" --json "$folder/inventory.json" "$pdf" > "$folder/inventory.txt"
    probe reference "$pdf" "$ppi" "$folder/reference.png" | tee -a "$out/measurements.jsonl"
    probe pdfium-objects "$pdf" >> "$out/measurements.jsonl"
    probe bitmap-svg "$pdf" "$folder/bitmap.svg" | tee -a "$out/measurements.jsonl"
    probe vector-svg "$pdf" "$folder/vector.svg" | tee -a "$out/measurements.jsonl"
    probe vector-svg "$pdf" "$folder/vector-again.svg" > /dev/null
    probe vector-svg "$pdf" "$folder/vector-darken.svg" --overprint-darken | tee -a "$out/measurements.jsonl"
    probe bitmap-svg "$pdf" "$folder/bitmap-again.svg" > /dev/null
    for way in vector bitmap; do
        same=false
        cmp -s "$folder/$way.svg" "$folder/$way-again.svg" && same=true
        echo "{\"command\":\"repeat\",\"pdf\":\"$pdf\",\"way\":\"$way\",\"identical_runs\":$same}" | tee -a "$out/measurements.jsonl"
        rm -f "$folder/$way-again.svg"
    done
    "$here/converters.sh" "$folder/converters" "$pdf" | tee -a "$out/measurements.jsonl"
    cp "$folder/converters/"*.cairo.svg "$folder/cairo.svg"
    cp "$folder/converters/"*.mupdf.svg "$folder/mupdf.svg"
    size=$(python3 -I -c 'from PIL import Image; import sys; print("%d %d" % Image.open(sys.argv[1]).size)' "$folder/reference.png")
    width=${size% *}; height=${size#* }
    for way in bitmap vector vector-darken cairo mupdf; do
        $first_job || echo "," >> "$jobs"
        first_job=false
        echo "{\"svg\":\"$folder/$way.svg\",\"width\":$width,\"height\":$height,\"out\":\"$folder/$way\"}" >> "$jobs"
    done
done
echo "]" >> "$jobs"

echo "== rendering"
driver=$(ls -d "${NUGET_PACKAGES:-$HOME/.nuget/packages}"/microsoft.playwright/*/.playwright/package | tail -1)
PLAYWRIGHT_MODULE="$driver/index.mjs" "$driver/../node/linux-x64/node" "$here/render.mjs" "$jobs" \
    "$checkout/src/MapModel/Map_Svg.Tests/fonts/Sentinel.ttf" | tee -a "$out/measurements.jsonl"

echo "== comparing"
printf 'pdf\tway\tbrowser\tbytes\tgzip_bytes\toutside\toutside_share\tmax_difference\tmean_difference\n' > "$out/summary.tsv"
for relative in "$@"; do
    name=$(basename "$relative" .pdf | tr ' .' '__')
    folder="$out/$name"
    for way in bitmap vector vector-darken cairo mupdf; do
        bytes=$(stat -c %s "$folder/$way.svg")
        gzip_bytes=$(gzip -9 -c "$folder/$way.svg" | wc -c)
        for browser in chromium firefox webkit; do
            result=$(python3 -I "$here/compare.py" --diff "$folder/$way.$browser.diff.png" "$folder/reference.png" "$folder/$way.$browser.png" || true)
            echo "$result" >> "$out/measurements.jsonl"
            python3 -I -c 'import json,sys; d=json.loads(sys.argv[1]); print("\t".join([sys.argv[2], sys.argv[3], sys.argv[4], sys.argv[5], sys.argv[6], str(d.get("outside")), str(d.get("outside_share")), str(d.get("max_difference")), str(d.get("mean_difference"))]))' \
                "$result" "$name" "$way" "$browser" "$bytes" "$gzip_bytes" >> "$out/summary.tsv"
        done
    done
done
column -t -s $'\t' "$out/summary.tsv"
