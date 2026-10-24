#!/usr/bin/env bash
# stage-except-experiments.sh <file>...
#
# Stages each file's changes except the hunks that hold the word EXPERIMENT, so that a line changed for a local run
# only, such as the text calls added to DL-5's list of fully implemented calls, stays in the working tree and out of
# the commit. Hunks are taken without context, so an experiment line is left out alone; a change on a line next to it
# would be left out with it, and the script stops rather than do that silently.
set -euo pipefail
[ $# -gt 0 ] || { echo "usage: stage-except-experiments.sh <file>..." >&2; exit 2; }
for file in "$@"; do
    diff=$(git diff -U0 -- "$file")
    [ -n "$diff" ] || continue
    kept=$(printf '%s\n' "$diff" | python3 -I -c '
import sys
text = sys.stdin.read()
head, _, rest = text.partition("\n@@")
hunks = ["@@" + h for h in rest.split("\n@@")] if rest else []
kept = [h for h in hunks if "EXPERIMENT" not in h]
for h in hunks:
    if "EXPERIMENT" in h:
        changed = [l for l in h.split("\n")[1:] if l[:1] in "+-" and "EXPERIMENT" not in l]
        removed = [l for l in changed if l.startswith("-")]
        added = [l for l in changed if l.startswith("+")]
        if len(added) > 0 or len(removed) > 1:
            sys.exit("a hunk holds an experiment line and other changes:\n" + h)
sys.stdout.write(head + "\n" + "\n".join(kept) + ("\n" if kept else ""))
')
    if printf '%s\n' "$kept" | grep -q '^@@'; then
        printf '%s\n' "$kept" | git apply --cached --unidiff-zero -
        echo "staged $file$(printf '%s\n' "$diff" | grep -q EXPERIMENT && echo ', its experiment left out')"
    else
        echo "nothing to stage in $file but experiments"
    fi
done
