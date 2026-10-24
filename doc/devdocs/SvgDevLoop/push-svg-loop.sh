#!/usr/bin/env bash
# push-svg-loop.sh [--with-experiments] <worktree> [<name>]
#
# Runs the SVG loop on the branch a worktree has checked out: rebuilds svg/<name> as one merge commit of origin/base
# and that branch, as the handover describes, and pushes it with a lease on what the remote holds, which starts the
# loop. The name defaults to the branch's last part, so feature/svg-text runs as svg/svg-text, with its results on
# svg-results/svg-text. With --with-experiments, one commit more carries the worktree's uncommitted hunks that hold the
# word EXPERIMENT, such as the text calls added to DL-5's list for a local run; that commit is the loop branch's alone
# and is never merged anywhere. Prints the commit pushed and, once GitHub has it, the loop run's id, for wait-run.sh.
set -euo pipefail
experiments=0
if [ "${1:-}" = --with-experiments ]; then experiments=1; shift; fi
[ $# -ge 1 ] || { echo "usage: push-svg-loop.sh [--with-experiments] <worktree> [<name>]" >&2; exit 2; }
cd "$1"
branch=$(git symbolic-ref --short HEAD)
name=${2:-${branch##*/}}
remote=${REMOTE:-origin}
trailers=${ARCHIVE_TRAILERS:-}

git fetch -q "$remote" base 2>&1 | grep -v "moved\|new location" || true
tree=$(git merge-tree --write-tree "$remote/base" "$branch") || { echo "base and $branch do not merge cleanly" >&2; exit 1; }
message=$(printf 'Merge %s into base, for the SVG loop\n%s' "$branch" "${trailers:+$'\n'$trailers}")
commit=$(git commit-tree "$tree" -p "$remote/base" -p "$branch" -m "$message")

if [ $experiments = 1 ]; then
    patch=$(git diff -U0 | python3 -I -c '
import sys
text = sys.stdin.read()
out = []
for part in ("\n" + text).split("\ndiff --git ")[1:]:
    head, _, rest = part.partition("\n@@")
    hunks = ["@@" + h for h in rest.split("\n@@")] if rest else []
    kept = [h for h in hunks if "EXPERIMENT" in h]
    if kept:
        out.append("diff --git " + head + "\n" + "\n".join(kept))
sys.stdout.write("\n".join(out) + ("\n" if out else ""))
')
    if [ -n "$patch" ]; then
        index=$(mktemp)
        rm -f "$index"
        GIT_INDEX_FILE=$index git read-tree "$commit"
        printf '%s\n' "$patch" | GIT_INDEX_FILE=$index git apply --cached --unidiff-zero -
        experiment_tree=$(GIT_INDEX_FILE=$index git write-tree)
        rm -f "$index"
        message=$(printf 'Carry the experiments of %s, on this loop branch only\n\nThe uncommitted lines of the worktree that hold the word EXPERIMENT, which a local run uses and no branch\ncommits.\n%s' "$branch" "${trailers:+$'\n'$trailers}")
        commit=$(git commit-tree "$experiment_tree" -p "$commit" -m "$message")
        git diff --stat "$commit~1" "$commit" | tail -1
    else
        echo "no experiment hunks in the worktree"
    fi
fi

before=$(git ls-remote "$remote" "refs/heads/svg/$name" | cut -f1)
git push -q --force-with-lease="refs/heads/svg/$name:$before" "$remote" "$commit:refs/heads/svg/$name" 2>&1 | grep -v "moved\|new location\|^remote: *$\|pull request\|pull/new" || true
echo "pushed $commit to svg/$name"

repository=$(git remote get-url "$remote" | sed -E 's#^.*github\.com[:/]##; s#\.git$##')
for attempt in $(seq 1 24); do
    run=$(gh run list --repo "$repository" --branch "svg/$name" --limit 5 --json databaseId,headSha \
          --jq ".[] | select(.headSha == \"$commit\") | .databaseId" 2>/dev/null | head -1)
    if [ -n "$run" ]; then echo "SVG loop run $run"; exit 0; fi
    sleep 5
done
echo "no run of svg/$name for $commit yet; look with gh run list --branch svg/$name" >&2
