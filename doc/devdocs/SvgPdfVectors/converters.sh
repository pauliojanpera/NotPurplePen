#!/usr/bin/env bash
# converters.sh <out folder> <pdf>...
#
# The converter way: each candidate converter writes the first page of each PDF as SVG, twice, once in the C locale
# and UTC and once in a Finnish locale, another time zone and from another folder, so that a difference between the
# two runs shows what the output depends on beyond its input (F-2). For each run it prints one line of JSON: the
# converter and its version, the seconds taken, the size plain and gzip -9 compressed, and whether the two runs'
# files are byte-identical.
#   cairo: Poppler's pdftocairo -svg (Cairo's SVG surface), from poppler-utils
#   mupdf: MuPDF's mutool convert -F svg, from mupdf-tools
# Needs poppler-utils and mupdf-tools (apt), gzip and python3.
set -euo pipefail
[ $# -ge 2 ] || { echo "usage: converters.sh <out folder> <pdf>..." >&2; exit 2; }
out=$1; shift
mkdir -p "$out"
out=$(cd "$out" && pwd)
cairo_version=$(pdftocairo -v 2>&1 | head -1 | awk '{print $3}')
mupdf_version=$(mutool -v 2>&1 | head -1 | awk '{print $3}')

# Run one converter on one PDF into a file.
# $1: converter, $2: pdf, $3: output svg
convert_one() {
    case "$1" in
    cairo) pdftocairo -svg -f 1 -l 1 "$2" "$3" ;;
    mupdf)
        mutool convert -F svg -o "${3%.svg}%d.svg" "$2" 1 2>/dev/null
        mv "${3%.svg}1.svg" "$3" ;;
    esac
}

for pdf in "$@"; do
    name=$(basename "$pdf" .pdf | tr ' ' '_')
    pdf=$(cd "$(dirname "$pdf")" && pwd)/$(basename "$pdf")
    for converter in cairo mupdf; do
        first="$out/$name.$converter.svg"
        second="$out/$name.$converter.second.svg"
        start=$(date +%s.%N)
        LC_ALL=C TZ=UTC convert_one "$converter" "$pdf" "$first"
        end=$(date +%s.%N)
        other=$(mktemp -d)
        (cd "$other" && LC_ALL=fi_FI.UTF-8 LANG=fi_FI.UTF-8 TZ=Pacific/Auckland convert_one "$converter" "$pdf" "$second")
        rmdir "$other"
        identical=false
        cmp -s "$first" "$second" && identical=true
        rm -f "$second"
        version=$cairo_version
        [ "$converter" = mupdf ] && version=$mupdf_version
        python3 -I -c 'import json,sys; print(json.dumps({"converter": sys.argv[1], "version": sys.argv[2], "pdf": sys.argv[3], "svg": sys.argv[4], "seconds": round(float(sys.argv[5]) - float(sys.argv[6]), 3), "bytes": int(sys.argv[7]), "gzip_bytes": int(sys.argv[8]), "identical_runs": sys.argv[9] == "true"}))' \
            "$converter" "$version" "$pdf" "$first" "$end" "$start" "$(stat -c %s "$first")" "$(gzip -9 -c "$first" | wc -c)" "$identical"
    done
done
