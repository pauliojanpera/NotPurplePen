#!/usr/bin/env bash
# fetch-artifact.sh <run id> <artifact name> <folder> [<repository>]
#
# Downloads one artifact of a GitHub Actions run and unpacks it into a folder that must not exist yet: what a run
# wrote is data, and goes into a folder of its own. Such as the rendering-differences-purplepen artifact of a Tests
# run, whose <stem>_actual.png files record-baselines.py writes over the baselines. The repository defaults to
# pauliojanpera/NotPurplePen.
set -euo pipefail
[ $# -ge 3 ] || { echo "usage: fetch-artifact.sh <run id> <artifact name> <folder> [<repository>]" >&2; exit 2; }
run=$1 name=$2 folder=$3 repository=${4:-pauliojanpera/NotPurplePen}
[ ! -e "$folder" ] || { echo "$folder exists already" >&2; exit 1; }
id=$(gh api "repos/$repository/actions/runs/$run/artifacts" --jq ".artifacts[] | select(.name == \"$name\") | .id")
[ -n "$id" ] || { echo "run $run has no artifact named $name" >&2; exit 1; }
mkdir -p "$folder"
gh api "repos/$repository/actions/artifacts/$id/zip" > "$folder/.artifact.zip"
(cd "$folder" && unzip -q .artifact.zip && rm .artifact.zip)
echo "$name of run $run: $(find "$folder" -type f | wc -l) files in $folder"
