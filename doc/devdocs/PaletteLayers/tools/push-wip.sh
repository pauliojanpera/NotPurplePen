#!/usr/bin/env bash
# push-wip.sh [--base <branch>] [--collect] [--blank-baseline <test description>]... [--base-baselines <folder>]
#             <clone> <branch> [<wip name>]
#
# Rebuilds wip/linux/<name> as one merge commit of the base branch and the branch, and pushes it with a lease on
# what the remote holds. The push starts the Tests and Build workflows. As ../README.md says, the branches of the
# palette-layer work run on base, the default; --base names another base branch. The name defaults to the branch's
# last part, so feature/inks runs as wip/linux/inks.
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
# --base-baselines names a folder of baselines of base's own tests that the branch moves, at their paths in the
# repository below the folder. A fix rooted at upstream cannot hold them, as base added or changed them itself, so one
# more commit, on the wip branch alone, writes them in. They belong to the merge of the branch into base, which takes
# them from the same folder. Give the option to each push of the branch.
#
# The message of each commit that the script makes lists the branches that the push builds for the first time: the
# merges into the branch, along its first parents, since the commit of the branch that the last push of the wip branch
# with a passing Tests run built, or since base on the first push. Each line names a merged branch and its work, from
# the subject of its merge, "Merge <branch>, with <work>, into <branch>". The commit that the push starts the runs on
# holds the list, as the build of a branch shows its message (@Pauli, Nov 19).
#
# Prints the commit pushed and the ids of the runs, for wait-run.sh and fetch-artifact.sh in ../../SvgDevLoop/.
set -euo pipefail
collect=0
base=base
descriptions=()
base_baselines=
while [ $# -gt 0 ]; do
    case "$1" in
        --base) base=$2; shift 2 ;;
        --collect) collect=1; shift ;;
        --blank-baseline) descriptions+=("$2"); shift 2 ;;
        --base-baselines) base_baselines=$(cd "$2" && pwd); shift 2 ;;
        *) break ;;
    esac
done
[ $# -ge 2 ] || { echo "usage: push-wip.sh [--base <branch>] [--collect] [--blank-baseline <test description>]... [--base-baselines <folder>] <clone> <branch> [<wip name>]" >&2; exit 2; }
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

# The commit of the branch that the last push of the wip branch with a passing Tests run built: the second parent of
# the merge of that push, below the commits that the push adds on the wip branch alone. Nothing if there is none.
last_built() {
    local sha parents step
    sha=$(gh run list --repo "$repository" --branch "$wip" --workflow tests.yml --event push --status success --limit 1 \
          --json headSha --jq '.[0].headSha // empty' 2>/dev/null) || return 0
    for step in 1 2 3 4; do
        [ -n "$sha" ] || return 0
        parents=$(gh api "repos/$repository/commits/$sha" --jq '[.parents[].sha] | join(" ")' 2>/dev/null) || return 0
        set -- $parents
        if [ $# -ge 2 ]; then
            echo "$2"
            return 0
        fi
        sha=${1:-}
    done
}

since=$(last_built)
if [ -n "$since" ] && git merge-base --is-ancestor "$since" "$remote/$feature" 2>/dev/null; then
    since_text="the last passing run of $wip, which built $(git rev-parse --short "$since")"
else
    since=$(git merge-base "$remote/$base" "$remote/$feature")
    since_text=$base
fi
merged=$(git log --first-parent --merges --reverse --format=%s "$since..$remote/$feature" \
         | sed -E -e 's/^Merge ([^ ,]+), with (.*), into [^ ]+$/* \1, with \2/' -e t \
                  -e "s/^Merge branch '([^']+)'.*$/* \\1/" -e t \
                  -e 's/^Merge ([^ ]+) into .*$/* \1/' -e t \
                  -e 's/^/* /')
if [ -n "$merged" ]; then
    news=$(printf 'The branches merged into %s since %s:\n%s' "$feature" "$since_text" "$merged")
else
    news="No branch is merged into $feature since $since_text."
fi
echo "$news"

commit=$(git commit-tree "$tree" -p "$remote/$base" -p "$remote/$feature" \
         -m "Merge $feature into $base, for a full run" -m "$news")

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
             -m "The Skia comparison leaves its rendering only where a baseline exists." -m "$news")
fi

if [ -n "$base_baselines" ]; then
    index=$(mktemp)
    rm -f "$index"
    GIT_INDEX_FILE=$index git read-tree "$commit"
    while IFS= read -r -d '' file; do
        path=${file#"$base_baselines"/}
        git cat-file -e "$commit:$path" 2>/dev/null || { echo "$path is not in $base and $feature" >&2; rm -f "$index"; exit 1; }
        blob=$(git hash-object -w "$file")
        GIT_INDEX_FILE=$index git update-index --cacheinfo "100644,$blob,$path"
        echo "base baseline $path"
    done < <(find "$base_baselines" -type f -print0 | sort -z)
    base_tree=$(GIT_INDEX_FILE=$index git write-tree)
    rm -f "$index"
    commit=$(git commit-tree "$base_tree" -p "$commit" -m "Record the baselines of base's own tests that $feature moves, on this run only" \
             -m "A branch rooted at upstream cannot hold them; the merge into $base takes them." -m "$news")
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
