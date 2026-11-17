#!/usr/bin/env bash
# int-worktree.sh <clone> <feature branch> <integration worktree>
#
# Makes or updates the integration worktree of a feature: the local branch local/<name>-int, which is
# origin/basewithfixes with the feature branch merged in, and with this folder's LinuxMapModelTestRun in
# src/MapModel/, committed on the local branch alone. The branch is never pushed, and no other branch gets the project.
#
# The first call makes the worktree from origin/basewithfixes. A later call merges the feature branch in again, and
# commits the project again if this folder holds a newer one. Set UPDATE_BASE=1 to merge the newest
# origin/basewithfixes in too. Each merge stops on a conflict, for the conflict to be resolved by hand.
set -euo pipefail
[ $# -eq 3 ] || { echo "usage: int-worktree.sh <clone> <feature branch> <integration worktree>" >&2; exit 2; }
clone=$1 feature=$2 worktree=$3
tools=$(cd "$(dirname "$0")" && pwd)
name=${feature##*/}
branch=local/$name-int

git -C "$clone" fetch -q origin basewithfixes
if [ ! -e "$worktree" ]; then
    git -C "$clone" worktree add -q -b "$branch" "$worktree" origin/basewithfixes
elif [ "${UPDATE_BASE:-0}" = 1 ]; then
    git -C "$worktree" merge -q --no-edit origin/basewithfixes
fi
[ "$(git -C "$worktree" symbolic-ref --short HEAD)" = "$branch" ] || { echo "$worktree is not on $branch" >&2; exit 1; }

git -C "$worktree" merge -q --no-edit "$feature"

mkdir -p "$worktree/src/MapModel/LinuxMapModelTestRun"
cp "$tools/LinuxMapModelTestRun/LinuxMapModelTestRun.csproj" "$tools/LinuxMapModelTestRun/Shims.cs" \
   "$worktree/src/MapModel/LinuxMapModelTestRun/"
git -C "$worktree" add src/MapModel/LinuxMapModelTestRun
if ! git -C "$worktree" diff --cached --quiet; then
    git -C "$worktree" commit -q -m "Add LinuxMapModelTestRun, on this local branch only" \
        -m "It runs test files of Test_MapModel on Linux. doc/devdocs/PaletteLayers/tools/ on docs/process-coordination keeps it."
fi
git -C "$worktree" log --oneline -3
