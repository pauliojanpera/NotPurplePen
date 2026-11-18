#!/usr/bin/env bash
# push-wip.sh [--base <branch>] [--collect] [--blank-baseline <test description>]... <clone> <branch> [<wip name>]
#
# Rebuilds wip/linux/<name> as one merge commit of the base branch and the branch, and pushes it with a lease on
# what the remote holds. The push starts the Tests and Build workflows. As the handover says, a feature runs on
# basewithfixes, the default, and a fix alone runs on base: --base base. The name defaults to the branch's last
# part, so feature/inks runs as wip/linux/inks.
#
# With --collect, the script also starts the Tests workflow by hand on the branch with the input
# collect_differences, so that one run writes the image of every comparison that the change moves, into the
# artifacts rendering-differences-<group>. The verdict of that run says nothing; the run of the push does. So the
# script starts the collecting run only after the runs of the push have started: a Tests run of a push waits for an
# earlier Tests run of the same commit and takes its result, and a collecting run does not fail on a moved image.
#
# --blank-baseline names the description of a Skia rendering test (src/MapModel/TestFiles/skia_render/<name>.txt,
# as a path in the repository) whose baseline does not exist yet. The Skia comparison leaves its rendering only
# where a baseline exists, so one more commit, on the wip branch alone, gives the test a white baseline of the size
# that the description gives. The test fails, and the artifact rendering-differences-skia holds the rendering as
# <baseline>_actual.png, for record-baselines.py. Push again without the option for the run that counts.
#
# Prints the commit pushed and the ids of the runs, for wait-run.sh and fetch-artifact.sh in ../../SvgDevLoop/.
set -euo pipefail
collect=0
base=basewithfixes
descriptions=()
while [ $# -gt 0 ]; do
    case "$1" in
        --base) base=$2; shift 2 ;;
        --collect) collect=1; shift ;;
        --blank-baseline) descriptions+=("$2"); shift 2 ;;
        *) break ;;
    esac
done
[ $# -ge 2 ] || { echo "usage: push-wip.sh [--base <branch>] [--collect] [--blank-baseline <test description>]... <clone> <branch> [<wip name>]" >&2; exit 2; }
cd "$1"
feature=$2
name=${3:-${feature##*/}}
wip=wip/linux/$name
remote=${REMOTE:-origin}
# The owner and the name of the repository: the last two parts of the remote's URL, whatever host or alias it names.
repository=$(git remote get-url "$remote" | sed -E 's#\.git$##; s#^.*[:/]([^/:]+/[^/:]+)$#\1#')

git fetch -q "$remote" "$base" "$feature"
tree=$(git merge-tree --write-tree "$remote/$base" "$remote/$feature") \
    || { echo "$base and $feature do not merge cleanly" >&2; exit 1; }
commit=$(git commit-tree "$tree" -p "$remote/$base" -p "$remote/$feature" \
         -m "Merge $feature into $base, for a full run")

if [ ${#descriptions[@]} -gt 0 ]; then
    index=$(mktemp)
    rm -f "$index"
    GIT_INDEX_FILE=$index git read-tree "$commit"
    for description in "${descriptions[@]}"; do
        # Line 2 of the description names the baseline beside it, and line 4 gives the size, as width,height.
        baseline=$(dirname "$description")/$(git show "$commit:$description" | sed -n 2p | tr -d '\r')
        size=$(git show "$commit:$description" | sed -n 4p | tr -d '\r')
        blob=$(python3 -I -c '
import io, sys
from PIL import Image
width, height = (int(v) for v in sys.argv[1].split(","))
data = io.BytesIO()
Image.new("RGBA", (width, height), (255, 255, 255, 255)).save(data, "PNG")
sys.stdout.buffer.write(data.getvalue())
' "$size" | git hash-object -w --stdin)
        GIT_INDEX_FILE=$index git update-index --add --cacheinfo "100644,$blob,$baseline"
        echo "blank baseline $baseline ($size)"
    done
    blank_tree=$(GIT_INDEX_FILE=$index git write-tree)
    rm -f "$index"
    commit=$(git commit-tree "$blank_tree" -p "$commit" -m "Give the new Skia rendering tests blank baselines, on this run only" \
             -m "The Skia comparison leaves its rendering only where a baseline exists.")
fi

before=$(git ls-remote "$remote" "refs/heads/$wip" | cut -f1)
git push -q --force-with-lease="refs/heads/$wip:$before" "$remote" "$commit:refs/heads/$wip" 2>&1 \
    | grep -v "moved\|new location\|^remote: *$\|pull request\|pull/new" || true
echo "pushed $commit to $wip"

# Wait up to two minutes until the commit has the given number of runs, and keep them in runs.
runs=
wait_for_runs() {
    for attempt in $(seq 1 24); do
        runs=$(gh run list --repo "$repository" --branch "$wip" --limit 10 --json databaseId,headSha,event,workflowName \
               --jq ".[] | select(.headSha == \"$commit\") | \"\(.workflowName) \(.event) \(.databaseId)\"" 2>/dev/null || true)
        [ "$(printf '%s\n' "$runs" | grep -c .)" -ge "$1" ] && return 0
        sleep 5
    done
    return 1
}

if [ $collect = 1 ]; then
    if ! wait_for_runs 2; then
        printf '%s\n' "$runs"
        echo "the runs of the push have not started; start the collecting run after them:" \
             "gh workflow run tests.yml --repo $repository --ref $wip -f collect_differences=true" >&2
        exit 1
    fi
    gh workflow run tests.yml --repo "$repository" --ref "$wip" -f collect_differences=true
fi

if wait_for_runs $((2 + collect)); then
    printf '%s\n' "$runs"
    exit 0
fi
printf '%s\n' "$runs"
echo "not all runs of $wip for $commit have started yet; look with gh run list --branch $wip" >&2
