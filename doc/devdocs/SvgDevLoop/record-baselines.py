"""Record the baselines that a fix moves, from the images a Tests run left of its failures.

Usage: python3 -I record-baselines.py [--dry-run] [--base <ref>] <artifact folder> <fix worktree> <wip worktree>

A Tests run of a wip/ branch uploads, for each bitmap comparison that failed, the rendering as <stem>_actual.png, in
the rendering-differences-<group> artifacts, whose root stands for src/ (fetch one with fetch-artifact.sh). Each
rendering is written over the baseline of the same stem, whatever the baseline's image format: on the fix branch when
the baseline is upstream's, as base keeps it, and on the wip branch otherwise, where base moved or added the image
itself, as a fix rooted at upstream cannot hold base's images. A JPEG baseline gets the rendering encoded at quality
100 with full chroma, keeping its resolution. The fix's root is where the fix meets base, the default base origin/base.

A test stops at the first image that differs, so a run finds one image a test at a time; a run of an experiment/
branch on which BitmapTestUtil goes on past a failed comparison finds them all at once. Commit what this writes on
the fix branch, merge the fix into the wip branch, and push the wip branch to run the tests again.
"""
import argparse
import collections
import os
import shutil
import subprocess
import sys
from PIL import Image

IMAGES = {".png", ".jpg", ".jpeg", ".gif", ".bmp", ".tif", ".tiff"}


def git(worktree, *arguments):
    return subprocess.run(["git", "-C", worktree] + list(arguments), check=True, capture_output=True, text=True).stdout


def images_by_stem(names):
    stems = collections.defaultdict(list)
    for name in names:
        if os.path.splitext(name)[1].lower() in IMAGES:
            stems[os.path.splitext(name)[0]].append(name)
    return stems


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--dry-run", action="store_true")
    parser.add_argument("--base", default="origin/base")
    parser.add_argument("artifact")
    parser.add_argument("fix")
    parser.add_argument("wip")
    options = parser.parse_args()

    root = git(options.fix, "merge-base", "HEAD", options.base).strip()
    upstream = images_by_stem(git(options.fix, "ls-tree", "-r", "--name-only", root, "--", "src").split("\n"))
    base = images_by_stem(git(options.fix, "ls-tree", "-r", "--name-only", options.base, "--", "src").split("\n"))
    moved = set(git(options.fix, "diff", "--name-only", root, options.base, "--", "src").split("\n"))

    counts = collections.Counter()
    for folder, _, files in os.walk(options.artifact):
        for file in sorted(files):
            if not file.endswith("_actual.png"):
                continue
            rendering = os.path.join(folder, file)
            stem = "src/" + os.path.relpath(rendering, options.artifact)[:-len("_actual.png")].replace(os.sep, "/")
            baselines = base.get(stem, [])
            if len(baselines) != 1:
                print("left out:", stem, "has", baselines or "no baseline in base")
                counts["left out"] += 1
                continue
            baseline = baselines[0]
            on_wip = baseline in moved or stem not in upstream
            target = os.path.join(options.wip if on_wip else options.fix, baseline)
            counts["wip" if on_wip else "fix"] += 1
            print("%s %s" % ("wip" if on_wip else "fix", baseline))
            if options.dry_run:
                continue
            if target.lower().endswith((".jpg", ".jpeg")):
                dpi = Image.open(target).info.get("dpi")
                Image.open(rendering).convert("RGB").save(target, "JPEG", quality=100, subsampling=0, **({"dpi": dpi} if dpi else {}))
            else:
                shutil.copyfile(rendering, target)
    print(dict(counts))


if __name__ == "__main__":
    sys.exit(main())
