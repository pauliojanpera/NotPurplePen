#!/usr/bin/env bash
# fetch-renderings.sh <loop run id> <results copy> [<repository>]
#
# Puts the images an SVG loop run kept of every comparison into a copy of its results: Chromium's image of each case
# and exported page under renderings/, and each exported page as the application draws it under course-pages/. The
# run's artifact svg-loop-renderings-<name> is downloaded into a folder of its own beside the copy, as fetch-artifact.sh
# downloads one, and its two folders are copied into the copy, beside the images of failures that the results branch
# holds already, which are the same images. The gallery and the tolerance checks then need no Chromium of their own.
# A run from before the artifact (Nov 7) kept only the images of failures; render-cases.sh renders the others.
set -euo pipefail
[ $# -ge 2 ] || { echo "usage: fetch-renderings.sh <loop run id> <results copy> [<repository>]" >&2; exit 2; }
here=$(cd "$(dirname "$0")" && pwd)
run=$1 repository=${3:-pauliojanpera/NotPurplePen}
results=$(cd "$2" && pwd)
name=$(gh api "repos/$repository/actions/runs/$run/artifacts" \
       --jq '.artifacts[] | select(.name | startswith("svg-loop-renderings-")) | .name' | head -1)
[ -n "$name" ] || { echo "run $run kept no renderings artifact; render-cases.sh renders the cases" >&2; exit 1; }
folder="$results.renderings-$run"
bash "$here/fetch-artifact.sh" "$run" "$name" "$folder" "$repository"
for kind in renderings course-pages; do
    if [ -d "$folder/$kind" ]; then
        mkdir -p "$results/$kind"
        cp -R "$folder/$kind/." "$results/$kind/"
        echo "$kind: $(find "$results/$kind" -name '*.png' | wc -l) images in $results/$kind"
    fi
done
