#!/usr/bin/env bash
# int-worktree.sh [--base <branch>] <clone> <feature branch> <integration worktree>
#
# Makes or updates the integration worktree of a feature: the local branch local/<name>-int, which is the base
# branch from origin with the feature branch merged in, and with this folder's LinuxMapModelTestRun in
# src/MapModel/, committed on the local branch alone. The branch is never pushed, and no other branch gets the
# project. As ../README.md says, the branches of the palette-layer work run on base, the default base branch;
# --base names another.
#
# The first call makes the worktree from the base branch. A later call merges the feature branch in again, and
# commits the project again if this folder holds a newer one. Set UPDATE_BASE=1 to merge the newest base branch in
# too. Each merge stops on a conflict, for the conflict to be resolved by hand. Set REBUILD=1 to make the branch
# again from the base branch, after the feature branch's history changed (an amended or rebased commit) or to move
# the worktree to another base branch: the branch holds nothing that the script does not make again.
set -euo pipefail
base=base
if [ "${1:-}" = --base ]; then base=$2; shift 2; fi
[ $# -eq 3 ] || { echo "usage: int-worktree.sh [--base <branch>] <clone> <feature branch> <integration worktree>" >&2; exit 2; }
clone=$1 feature=$2 worktree=$3
tools=$(cd "$(dirname "$0")" && pwd)
name=${feature##*/}
branch=local/$name-int

git -C "$clone" fetch -q origin "$base"
if [ ! -e "$worktree" ]; then
    git -C "$clone" worktree add -q -b "$branch" "$worktree" "origin/$base"
fi
[ "$(git -C "$worktree" symbolic-ref --short HEAD)" = "$branch" ] || { echo "$worktree is not on $branch" >&2; exit 1; }
if [ "${REBUILD:-0}" = 1 ]; then
    git -C "$worktree" merge --abort 2>/dev/null || true
    git -C "$worktree" reset -q --hard "origin/$base"
elif [ "${UPDATE_BASE:-0}" = 1 ]; then
    git -C "$worktree" merge -q --no-edit "origin/$base"
fi

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
