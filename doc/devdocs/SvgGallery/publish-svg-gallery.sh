#!/usr/bin/env bash
# publish-svg-gallery.sh <gallery folder> <remote> <commit message file>
#
# Publishes the SVG gallery: makes the branch svg-gallery hold the given
# folder as its one commit, then puts the same files under svg/ on gh-pages,
# beside what the site's own publishing put there. Both branches are pushed as
# orphan commits with a lease, as build.yml's "Publish the site" pushes
# gh-pages: neither keeps history, and a push that would drop another
# publish's work is refused and assembled again. A branch that already holds
# exactly these files is left as it is.
set -euo pipefail

gallery_folder=$(cd "$1" && pwd)
remote=$2
message=$(cd "$(dirname "$3")" && pwd)/$(basename "$3")
work=$(mktemp -d)
trap 'rm -rf "$work"' EXIT

# Make a folder a repository whose one commit, on the given branch, holds it.
commit_as_orphan() {
    local folder=$1 branch=$2
    git -C "$folder" init --quiet --initial-branch "$branch"
    git -C "$folder" config user.name Claude
    git -C "$folder" config user.email noreply@anthropic.com
    git -C "$folder" add -A
    git -C "$folder" commit --quiet -F "$message"
}

# Say whether a remote commit holds the same files as a folder's own commit.
holds_same_files() {
    local folder=$1 commit=$2
    [ -n "$commit" ] || return 1
    git -C "$folder" fetch --quiet --depth 1 "$remote" "$commit"
    [ "$(git -C "$folder" rev-parse 'FETCH_HEAD^{tree}')" = "$(git -C "$folder" rev-parse 'HEAD^{tree}')" ]
}

for attempt in 1 2 3 4 5; do
    rm -rf "$work/gallery"
    mkdir "$work/gallery"
    cp -R "$gallery_folder/." "$work/gallery/"
    rm -rf "$work/gallery/.git"
    commit_as_orphan "$work/gallery" svg-gallery
    gallery_before=$(git ls-remote "$remote" svg-gallery | cut -f1)
    if holds_same_files "$work/gallery" "$gallery_before"; then
        echo "svg-gallery already holds this gallery"
        break
    fi
    if git -C "$work/gallery" push --quiet --force-with-lease="svg-gallery:$gallery_before" "$remote" svg-gallery; then
        echo "svg-gallery published on attempt $attempt"
        break
    fi
    if [ "$attempt" = 5 ]; then echo "svg-gallery: every attempt lost a race" >&2; exit 1; fi
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
    rm -rf "$work/site/svg"
    mkdir "$work/site/svg"
    git -C "$work/gallery" archive HEAD | tar -x -C "$work/site/svg"
    commit_as_orphan "$work/site" gh-pages
    if holds_same_files "$work/site" "$pages_before"; then
        echo "gh-pages already serves this gallery"
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
