#!/usr/bin/env bash
# wait-run.sh <run id> [<repository>]
#
# Waits for a GitHub Actions run to finish, asking every 45 seconds, then prints its conclusion and its jobs'. Meant
# to be started in the background, so that the session is told when the run ends instead of asking again and again.
# The repository defaults to pauliojanpera/NotPurplePen.
set -euo pipefail
run=${1:?usage: wait-run.sh <run id> [<repository>]}
repository=${2:-pauliojanpera/NotPurplePen}
while :; do
    state=$(gh api "repos/$repository/actions/runs/$run" --jq '.status + " " + (.conclusion // "") + " " + .name + " " + .head_branch' 2>/dev/null || true)
    case "$state" in
        completed*) break ;;
    esac
    sleep 45
done
echo "run $run: $state"
gh api "repos/$repository/actions/runs/$run/jobs" --jq '.jobs[] | "  " + .name + ": " + (.conclusion // .status)'
