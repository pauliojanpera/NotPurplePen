#!/usr/bin/env bash
# render-cases.sh <worktree> <results folder> <reference folder> <case,case,...>
#
# Renders cases' SVG documents in Chromium as the tests do, for the gallery and the tolerance research, by
# ../SvgGallery/render-cases.mjs: each <results folder>/svg/<case>.svg into <results folder>/renderings/<case>.png,
# at the size of the case's reference image, from <results folder>/references/ or else <reference folder>. It drives
# the headless shell of the worktree's Playwright package, with the flags ChromiumRenderer passes, and on Linux under a
# fontconfig that offers the sentinel font alone, as ChromiumRenderer does: without it, a text case's rendering here
# would not match the loop's. Build Map_Svg.Tests in the worktree first.
set -euo pipefail
[ $# -eq 4 ] || { echo "usage: render-cases.sh <worktree> <results folder> <reference folder> <case,case,...>" >&2; exit 2; }
here=$(cd "$(dirname "$0")" && pwd)
worktree=$(cd "$1" && pwd)
tests="$worktree/src/MapModel/Map_Svg.Tests"
package="$tests/bin/Debug/net10.0/.playwright/package/index.js"
[ -f "$package" ] || { echo "no Playwright package under $tests/bin; build Map_Svg.Tests first" >&2; exit 1; }

browsers=${PLAYWRIGHT_BROWSERS_PATH:-}
if [ -z "$browsers" ]; then
    if [ -d /opt/pw-browsers ]; then browsers=/opt/pw-browsers; else browsers="$HOME/.cache/ms-playwright"; fi
fi
shell=$(ls -d "$browsers"/chromium_headless_shell-*/chrome-headless-shell-linux64/chrome-headless-shell 2>/dev/null | sort -V | tail -1)
[ -n "$shell" ] || { echo "no headless shell under $browsers" >&2; exit 1; }

work=$(mktemp -d)
trap 'rm -rf "$work"' EXIT
printf 'import playwright from "%s";\nexport const chromium = playwright.chromium;\n' "$package" > "$work/playwright.mjs"
mkdir -p "$work/fonts"
cp "$tests/fonts/Sentinel.ttf" "$work/fonts/"
printf '<?xml version="1.0"?>\n<!DOCTYPE fontconfig SYSTEM "fonts.dtd">\n<fontconfig>\n  <dir>%s</dir>\n  <cachedir>%s</cachedir>\n</fontconfig>\n' \
    "$work/fonts" "$work/cache" > "$work/fonts.conf"

FONTCONFIG_FILE="$work/fonts.conf" PLAYWRIGHT_MODULE="$work/playwright.mjs" CHROMIUM_EXECUTABLE="$shell" \
    node "$here/../SvgGallery/render-cases.mjs" "$2" "$3" "$4"
