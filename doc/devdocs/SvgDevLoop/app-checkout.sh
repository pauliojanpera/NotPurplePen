#!/usr/bin/env bash
# app-checkout.sh <clone> <commit> <folder>
#
# Makes a checkout in which AvPurplePen builds: a clone of its own of <clone>, which shares its objects and fetches
# what it lacks from the same remote, at <commit>, without the test files, the documents and the WinForms project.
# The PdfSharp subtree builds with GitVersion 5.12, whose libgit2 refuses to open a repository whose config sets
# extensions.worktreeConfig ("unsupported extension name extensions.worktreeconfig"). `git sparse-checkout` sets it in
# the config that a clone shares with its worktrees, so once any worktree of a clone is sparse, none of them builds the
# application. This checkout keeps its sparse patterns in .git/info/sparse-checkout and switches them on in its own
# .git/config, without the extension. PurplePenCore takes its images from src/PurplePen/Images, and AvPurplePen copies
# the sample event from doc/userdocs/Sample, so those stay in. Build with
#   dotnet build <folder>/src/AvPurplePen/AvPurplePen.csproj
# and remove the folder when done; it holds no work of its own.
set -euo pipefail
[ $# -eq 3 ] || { echo "usage: app-checkout.sh <clone> <commit> <folder>" >&2; exit 2; }
clone=$(cd "$1" && git rev-parse --show-toplevel)
commit=$(git -C "$clone" rev-parse --verify "$2^{commit}")
folder=$3
remote=$(git -C "$clone" remote get-url origin)
git clone --quiet --shared --no-checkout "$clone" "$folder"
cd "$folder"
git remote set-url origin "$remote"
git config remote.origin.promisor true
git config remote.origin.partialclonefilter blob:none
git config core.sparseCheckout true
git config core.sparseCheckoutCone false
cat > .git/info/sparse-checkout <<'PATTERNS'
/*
!/doc/*
/doc/userdocs/Sample/
!/loc/
!/src/PurplePen/*
/src/PurplePen/Images/
!/src/TestFiles/
!/src/MapModel/TestFiles/
PATTERNS
git checkout --quiet --detach "$commit"
echo "$folder: $(git log --oneline -1)"
