#!/usr/bin/env bash
# publish-logbook.sh <page folder> <commit message> [remote]
#
# Publishes the Translation Logbook that logbook.py wrote into <page folder>, at
# https://pauliojanpera.github.io/NotPurplePen/translations/. Makes the branch
# translation-log hold the folder, with the fonts of the SVG Logbook, as its one
# commit, then puts the same files under translations/ on gh-pages, beside what
# the site's own publishing put there. build.yml's "Publish the site" copies the
# branch translation-log into translations/ at each publish, so the page stays
# whichever branch publishes the site.
#
# Both branches are pushed as orphan commits with a lease, as
# ../SvgGallery/publish-svg-gallery.sh pushes them: neither keeps history, and a
# push that would drop another publish's work is refused and assembled again. A
# branch that already holds exactly these files is left as it is. The remote is
# https://github.com/pauliojanpera/NotPurplePen unless a third argument names
# another.
set -euo pipefail

page_folder=$(cd "$1" && pwd)
message=$2
remote=${3:-https://github.com/pauliojanpera/NotPurplePen}
work=$(mktemp -d)
trap 'rm -rf "$work"' EXIT

# Make a folder a repository whose one commit, on the given branch, holds it.
commit_as_orphan() {
    local folder=$1 branch=$2
    git -C "$folder" init --quiet --initial-branch "$branch"
    git -C "$folder" config user.name Claude
    git -C "$folder" config user.email noreply@anthropic.com
    git -C "$folder" add -A
    git -C "$folder" commit --quiet -m "$message"
}

# Say whether a remote commit holds the same files as a folder's own commit.
holds_same_files() {
    local folder=$1 commit=$2
    [ -n "$commit" ] || return 1
    git -C "$folder" fetch --quiet --depth 1 "$remote" "$commit"
    [ "$(git -C "$folder" rev-parse 'FETCH_HEAD^{tree}')" = "$(git -C "$folder" rev-parse 'HEAD^{tree}')" ]
}

# The fonts of the SVG Logbook, from the branch svg-gallery. Without them the
# page falls back to the system's fonts.
mkdir "$work/fonts"
if [ -n "$(git ls-remote "$remote" svg-gallery | cut -f1)" ]; then
    git clone --quiet --depth 1 --filter=blob:none --no-checkout --branch svg-gallery "$remote" "$work/svg"
    git -C "$work/svg" archive HEAD fonts | tar -x -C "$work" || echo "no fonts in svg-gallery"
fi

for attempt in 1 2 3 4 5; do
    rm -rf "$work/log"
    mkdir "$work/log"
    cp -R "$page_folder/." "$work/log/"
    rm -rf "$work/log/.git"
    if [ -n "$(ls -A "$work/fonts")" ] && [ ! -d "$work/log/fonts" ]; then
        cp -R "$work/fonts" "$work/log/fonts"
    fi
    commit_as_orphan "$work/log" translation-log
    log_before=$(git ls-remote "$remote" translation-log | cut -f1)
    if holds_same_files "$work/log" "$log_before"; then
        echo "translation-log already holds this page"
        break
    fi
    if git -C "$work/log" push --quiet --force-with-lease="translation-log:$log_before" "$remote" translation-log; then
        echo "translation-log published on attempt $attempt"
        break
    fi
    if [ "$attempt" = 5 ]; then echo "translation-log: every attempt lost a race" >&2; exit 1; fi
    sleep $((5 + RANDOM % 10))
done

for attempt in 1 2 3 4 5; do
    pages_before=$(git ls-remote "$remote" gh-pages | cut -f1)
    rm -rf "$work/published" "$work/site"
    git clone --quiet --depth 1 --branch gh-pages "$remote" "$work/published"
    if [ ! -f "$work/published/catalog.json" ]; then
        echo "gh-pages holds no catalog.json; publishing over it would drop every download it lists." >&2
        exit 1
    fi
    mkdir "$work/site"
    git -C "$work/published" archive HEAD | tar -x -C "$work/site"
    rm -rf "$work/site/translations"
    mkdir "$work/site/translations"
    git -C "$work/log" archive HEAD | tar -x -C "$work/site/translations"
    commit_as_orphan "$work/site" gh-pages
    if holds_same_files "$work/site" "$pages_before"; then
        echo "gh-pages already serves this page"
        exit 0
    fi
    if git -C "$work/site" push --quiet --force-with-lease="gh-pages:$pages_before" "$remote" gh-pages; then
        echo "gh-pages published on attempt $attempt"
        exit 0
    fi
    sleep $((5 + RANDOM % 10))
done
echo "gh-pages: every attempt lost a race" >&2
exit 1
