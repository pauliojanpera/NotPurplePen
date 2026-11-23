#!/usr/bin/env bash
# int-worktree.sh [--base <branch>] <clone> <feature branch> <integration worktree>
#
# Makes or updates the integration worktree of a feature: the local branch local/<name>-int, which is the base branch
# from origin with the feature branch merged in (the branch of origin, or the local branch of the clone when that holds
# all of it), with the fixes of the tests that a run on Linux needs and the base branch does not have (local_fixes
# below), and with this folder's LinuxMapModelTestRun in src/MapModel/, committed on the local branch alone. The branch
# is never pushed, so the fixes and the project change no other branch and no run of the workflow. As ../README.md says,
# the branches of the palette-stack work run on base, the default base branch; --base names another.
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

# The feature branch: the branch of origin, fetched, and the local branch of the same name in the clone, if it has one.
remote=
if git -C "$clone" ls-remote --exit-code origin "refs/heads/$feature" >/dev/null; then
    git -C "$clone" fetch -q origin "+refs/heads/$feature:refs/remotes/origin/$feature"
    remote=origin/$feature
else
    status=$?
    [ $status = 2 ] || { echo "origin could not be read: git ls-remote ended with $status" >&2; exit 1; }
fi
local_branch=
if git -C "$clone" rev-parse -q --verify "refs/heads/$feature^{commit}" >/dev/null; then
    local_branch=refs/heads/$feature
fi
if [ -z "$remote$local_branch" ]; then
    echo "$feature is neither a branch of origin nor a local branch of $clone" >&2
    exit 1
fi

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

# The local branch is merged when it holds all the commits of the branch of origin, so that the local tests can run
# commits that are not pushed yet. A local branch that is behind origin gives way to origin. A local branch that
# diverged from origin, as after a rewrite of its history that is not pushed yet, is merged when the integration branch
# holds none of the commits that only origin has: on the first call, after REBUILD=1, and after such a merge.
source=${local_branch:-$remote}
if [ -n "$local_branch" ] && [ -n "$remote" ]; then
    if git -C "$clone" merge-base --is-ancestor "$remote" "$local_branch"; then
        ahead=$(git -C "$clone" rev-list --count "$remote..$local_branch")
        [ "$ahead" = 0 ] || echo "Merging the local branch $feature: $remote does not have $ahead of its commits."
    elif git -C "$clone" merge-base --is-ancestor "$local_branch" "$remote"; then
        echo "The local branch $feature is behind $remote. Merging $remote."
        source=$remote
    elif [ "$(git -C "$worktree" rev-list --count "$remote" "^$local_branch" ^HEAD)" = \
           "$(git -C "$worktree" rev-list --count "$remote" "^$local_branch")" ]; then
        echo "The local branch $feature and $remote diverged, as after a rewrite that is not pushed. Merging the"
        echo "local branch."
    else
        echo "The local branch $feature and $remote diverged, and $branch holds commits that only $remote has." >&2
        echo "After a rewrite of the local branch, run with REBUILD=1. Otherwise make one of the two hold the" >&2
        echo "other, or delete the local branch." >&2
        exit 1
    fi
elif [ -n "$local_branch" ]; then
    echo "$feature is not on origin. Merging the local branch."
fi
git -C "$worktree" merge -q --no-edit "$source"

# The fixes of the tests that a run on Linux needs, which are not in base. fix/case-of-test-file-names (1973c604)
# spells four test file names as the files are spelled: without it, seven tests of Map_Skia.Tests do not find their
# files on a file system that tells the letter case apart. Windows does not, so the workflow needs no such fix.
local_fixes=(fix/case-of-test-file-names)
for fix in "${local_fixes[@]}"; do
    git -C "$clone" fetch -q origin "$fix"
    if ! git -C "$worktree" merge-base --is-ancestor "origin/$fix" HEAD; then
        git -C "$worktree" merge -q --no-edit "origin/$fix"
    fi
done

mkdir -p "$worktree/src/MapModel/LinuxMapModelTestRun"
cp "$tools/LinuxMapModelTestRun/LinuxMapModelTestRun.csproj" "$tools/LinuxMapModelTestRun/Shims.cs" \
   "$worktree/src/MapModel/LinuxMapModelTestRun/"
git -C "$worktree" add src/MapModel/LinuxMapModelTestRun
if ! git -C "$worktree" diff --cached --quiet; then
    git -C "$worktree" commit -q -m "Add LinuxMapModelTestRun, on this local branch only" \
        -m "It runs test files of Test_MapModel on Linux. doc/devdocs/PaletteStack/tools/ on docs/process-coordination keeps it."
fi
git -C "$worktree" log --oneline -3
