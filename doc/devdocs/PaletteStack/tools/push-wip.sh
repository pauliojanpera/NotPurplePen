#!/usr/bin/env bash
# push-wip.sh [--base <branch>] [--collect] [--base-baselines <folder>]
#             <clone> <branch> [<wip name>]
#
# Rebuilds wip/linux/<name> as one merge commit of the base branch and the branch, and pushes it with a lease on
# what the remote holds. The push starts the Tests and Build workflows. As ../README.md says, the branches of the
# palette-stack work run on base, the default; --base names another base branch. The name defaults to the branch's
# last part, so feature/inks runs as wip/linux/inks.
#
# With --collect, the script also makes a collecting run, in which the Tests workflow writes the image of every
# comparison that the change moves, into the artifacts rendering-differences-<group>, and does not fail. The verdict
# of that run says nothing; the run of the wip push does. The collecting run needs a commit of its own: a Tests run of
# a push waits for an earlier Tests run of the same commit and takes its result, and a collecting run does not fail
# on a moved image. So one more commit, on top of the wip commit, changes only .github/workflows/tests.yml so that its
# tests collect, and the script pushes it to experiment/collect-<name>. A push runs the workflows of the commit pushed,
# and a push to experiment/** runs the Tests workflow and makes no package. The push needs no write of the GitHub API,
# which the proxy of a cloud session refuses (@Pauli, Nov 23). The tree of the commit is the tree of the wip commit
# apart from the workflow, so the images that it collects are the images of the wip commit.
#
# --base-baselines names a folder of baselines of base's own tests that the branch moves, at their paths in the
# repository below the folder. A fix rooted at upstream cannot hold them, as base added or changed them itself, so one
# more commit, on the wip branch alone, writes them in. They belong to the merge of the branch into base, which takes
# them from the same folder. Give the option to each push of the branch.
#
# The message of each commit that the script makes lists the ten branches merged into the branch last, along its first
# parents and not in the base branch, the newest first. Each line names a merged branch and its work, from the
# subject of its merge, "Merge <branch>, with <work>, into <branch>". So the trial build of feature/palette-stack
# tells on the site, under its commit, what it holds (@Pauli, Nov 19). A branch without such merges gets no list.
#
# Prints the commit pushed and the ids of the runs, for wait-run.sh and fetch-artifact.sh in ../../SvgDevLoop/.
set -euo pipefail
collect=0
base=base
base_baselines=
while [ $# -gt 0 ]; do
    case "$1" in
        --base) base=$2; shift 2 ;;
        --collect) collect=1; shift ;;
        --base-baselines) base_baselines=$(cd "$2" && pwd); shift 2 ;;
        *) break ;;
    esac
done
[ $# -ge 2 ] || { echo "usage: push-wip.sh [--base <branch>] [--collect] [--base-baselines <folder>] <clone> <branch> [<wip name>]" >&2; exit 2; }
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

# The ten branches merged into the branch last, which the message of each commit lists.
merged=$(git log --first-parent --merges --max-count=10 --format=%s "$remote/$base..$remote/$feature" \
         | sed -E -e 's/^Merge ([^ ,]+), with (.*), into [^ ]+$/* \1, with \2/' -e t \
                  -e "s/^Merge branch '([^']+)'.*$/* \\1/" -e t \
                  -e 's/^Merge ([^ ]+) into .*$/* \1/' -e t \
                  -e 's/^/* /')
news=()
if [ -n "$merged" ]; then
    news=(-m "$(printf 'The branches merged into %s last, the newest first:\n%s' "$feature" "$merged")")
    printf 'The branches merged into %s last, the newest first:\n%s\n' "$feature" "$merged"
fi

commit=$(git commit-tree "$tree" -p "$remote/$base" -p "$remote/$feature" \
         -m "Merge $feature into $base, for a full run" "${news[@]}")

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
             -m "A branch rooted at upstream cannot hold them; the merge into $base takes them." "${news[@]}")
fi

# The commit of the collecting run: the wip commit with a Tests workflow whose tests collect. The workflow sets the
# collection from the input of a run started by hand, which a push does not have, so the commit sets it on.
collect_commit=
experiment=experiment/collect-$name
if [ $collect = 1 ]; then
    workflow=.github/workflows/tests.yml
    blob=$(git cat-file blob "$commit:$workflow" | python3 -I -c '
import sys
data = sys.stdin.buffer.read()
setting = b"TEST_COLLECT_DIFFERENCES: ${{ inputs.collect_differences && \x27True\x27 || \x27False\x27 }}"
if data.count(setting) != 1:
    sys.exit("the Tests workflow does not set TEST_COLLECT_DIFFERENCES from the input collect_differences once")
sys.stdout.buffer.write(data.replace(setting, b"TEST_COLLECT_DIFFERENCES: \x27True\x27"))
' | git hash-object -w --stdin)
    index=$(mktemp)
    rm -f "$index"
    GIT_INDEX_FILE=$index git read-tree "$commit"
    GIT_INDEX_FILE=$index git update-index --cacheinfo "100644,$blob,$workflow"
    collect_tree=$(GIT_INDEX_FILE=$index git write-tree)
    rm -f "$index"
    collect_note="The Tests workflow of this commit writes the image of every comparison that differs and does not fail,"
    collect_note="$collect_note so its verdict says nothing. The tree is the tree of $commit apart from the workflow."
    collect_commit=$(git commit-tree "$collect_tree" -p "$commit" -m "Collect the images that $feature moves, on this run only" \
                     -m "$collect_note" "${news[@]}")
fi

# Push a commit to a branch with a lease on what the remote holds.
# commit: the commit.
# branch: the name of the branch.
push_branch() {
    local before
    before=$(git ls-remote "$remote" "refs/heads/$2" | cut -f1)
    git -c push.negotiate=true push -q --force-with-lease="refs/heads/$2:$before" "$remote" "$1:refs/heads/$2" 2>&1 \
        | grep -v "moved\|new location\|^remote: *$\|pull request\|pull/new" || true
    echo "pushed $1 to $2"
}

push_branch "$commit" "$wip"
[ -z "$collect_commit" ] || push_branch "$collect_commit" "$experiment"

# Wait up to two minutes until a commit has the given number of runs on a branch, and keep them in runs.
# count: the number of runs.
# branch: the name of the branch.
# commit: the commit.
runs=
wait_for_runs() {
    for attempt in $(seq 1 24); do
        runs=$(gh run list --repo "$repository" --branch "$2" --limit 10 --json databaseId,headSha,event,workflowName \
               --jq ".[] | select(.headSha == \"$3\") | \"\(.workflowName) \(.event) \(.databaseId)\"" 2>/dev/null || true)
        [ "$(printf '%s\n' "$runs" | grep -c .)" -ge "$1" ] && return 0
        sleep 5
    done
    return 1
}

wait_for_runs 2 "$wip" "$commit" \
    || echo "not all runs of $wip for $commit have started yet; look with gh run list --branch $wip" >&2
printf '%s\n' "$runs"
if [ -n "$collect_commit" ]; then
    echo "the collecting run, on $experiment:"
    wait_for_runs 1 "$experiment" "$collect_commit" \
        || echo "the collecting run of $collect_commit has not started yet; look with gh run list --branch $experiment" >&2
    printf '%s\n' "$runs"
fi
