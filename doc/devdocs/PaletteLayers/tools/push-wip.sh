#!/usr/bin/env bash
# push-wip.sh [--collect] <clone> <feature branch> [<wip name>]
#
# Rebuilds wip/linux/<name> as one merge commit of origin/basewithfixes and the feature branch, as the handover
# says for a feature's full run, and pushes it with a lease on what the remote holds. The push starts the Tests and
# Build workflows. The name defaults to the feature branch's last part, so feature/inks runs as wip/linux/inks.
#
# With --collect, the script also starts the Tests workflow by hand on the branch with the input
# collect_differences, so that one run writes the image of every comparison that the change moves, into the
# artifacts rendering-differences-<group>. The verdict of that run says nothing; the run of the push does.
# Prints the commit pushed and the ids of the runs, for wait-run.sh and fetch-artifact.sh in ../../SvgDevLoop/.
set -euo pipefail
collect=0
if [ "${1:-}" = --collect ]; then collect=1; shift; fi
[ $# -ge 2 ] || { echo "usage: push-wip.sh [--collect] <clone> <feature branch> [<wip name>]" >&2; exit 2; }
cd "$1"
feature=$2
name=${3:-${feature##*/}}
wip=wip/linux/$name
remote=${REMOTE:-origin}
# The owner and the name of the repository: the last two parts of the remote's URL, whatever host or alias it names.
repository=$(git remote get-url "$remote" | sed -E 's#\.git$##; s#^.*[:/]([^/:]+/[^/:]+)$#\1#')

git fetch -q "$remote" basewithfixes "$feature"
tree=$(git merge-tree --write-tree "$remote/basewithfixes" "$remote/$feature") \
    || { echo "basewithfixes and $feature do not merge cleanly" >&2; exit 1; }
commit=$(git commit-tree "$tree" -p "$remote/basewithfixes" -p "$remote/$feature" \
         -m "Merge $feature into basewithfixes, for a full run")

before=$(git ls-remote "$remote" "refs/heads/$wip" | cut -f1)
git push -q --force-with-lease="refs/heads/$wip:$before" "$remote" "$commit:refs/heads/$wip" 2>&1 \
    | grep -v "moved\|new location\|^remote: *$\|pull request\|pull/new" || true
echo "pushed $commit to $wip"

if [ $collect = 1 ]; then
    gh workflow run tests.yml --repo "$repository" --ref "$wip" -f collect_differences=true
fi

for attempt in $(seq 1 24); do
    runs=$(gh run list --repo "$repository" --branch "$wip" --limit 10 --json databaseId,headSha,event,workflowName \
           --jq ".[] | select(.headSha == \"$commit\") | \"\(.workflowName) \(.event) \(.databaseId)\"" 2>/dev/null || true)
    expected=$((2 + collect))
    if [ "$(printf '%s\n' "$runs" | grep -c .)" -ge $expected ]; then
        printf '%s\n' "$runs"
        exit 0
    fi
    sleep 5
done
printf '%s\n' "${runs:-}"
echo "not all runs of $wip for $commit have started yet; look with gh run list --branch $wip" >&2
