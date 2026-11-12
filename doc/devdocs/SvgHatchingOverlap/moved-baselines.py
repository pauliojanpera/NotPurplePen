"""List the baselines a Tests run moved, with how far each rendering lies from its baseline.

Usage: python3 -I moved-baselines.py <clone> <ref> <artifact folder>...

Each artifact folder is a rendering-differences-<group> artifact of a Tests run, unpacked by
../SvgDevLoop/fetch-artifact.sh, whose root stands for src/: a comparison that failed left its rendering there as
<stem>_actual.png. Each is compared with its baseline as the commit <ref> of the clone holds it, the branch the run
merged its change into, such as origin/base. For each, prints the pixels that differ from the baseline by more than 2
levels in a channel, which leaves out the level or two a baseline recorded elsewhere may differ by throughout and the
tests tolerate, and by more than 32, the largest difference, and the bounds of the pixels that differ by more than
32 levels; then the counts by group.
"""
import io
import os
import subprocess
import sys

from PIL import Image, ImageChops

IMAGES = (".png", ".jpg", ".jpeg", ".gif", ".bmp")


def git(clone, *arguments):
    """The output of a git command in the clone, as bytes."""
    return subprocess.run(["git", "-C", clone] + list(arguments), check=True, capture_output=True).stdout


def baseline_of(files, stem):
    """The path of the baseline of a stem, src/<stem>.<image extension>, among the commit's files, or None."""
    for extension in IMAGES:
        for candidate in (stem + extension, stem + extension.upper()):
            if candidate in files:
                return candidate
    return None


def main():
    clone, ref = sys.argv[1], sys.argv[2]
    committed = set(git(clone, "ls-tree", "-r", "--name-only", ref, "--", "src").decode("utf-8").split("\n"))
    counts = {}
    for artifact in sys.argv[3:]:
        group = os.path.basename(os.path.normpath(artifact))
        for folder, _, files in os.walk(artifact):
            for file in sorted(files):
                if not file.endswith("_actual.png"):
                    continue
                stem = os.path.relpath(os.path.join(folder, file), artifact)[:-len("_actual.png")]
                baseline = baseline_of(committed, "src/" + stem.replace(os.sep, "/"))
                if baseline is None:
                    print("%s\t%s\tno baseline" % (group, stem))
                    continue
                actual = Image.open(os.path.join(folder, file)).convert("RGB")
                expected = Image.open(io.BytesIO(git(clone, "show", ref + ":" + baseline))).convert("RGB")
                if actual.size != expected.size:
                    print("%s\t%s\tsize %s against %s" % (group, stem, actual.size, expected.size))
                    continue
                difference = ImageChops.difference(actual, expected)
                channels = difference.split()
                largest = ImageChops.lighter(ImageChops.lighter(channels[0], channels[1]), channels[2])
                histogram = largest.histogram()
                any_count = sum(histogram[3:])
                big_count = sum(histogram[33:])
                top = max((level for level, count in enumerate(histogram) if count), default=0)
                bounds = largest.point(lambda v: 255 if v > 32 else 0).getbbox()
                pixels = actual.size[0] * actual.size[1]
                print("%s\t%s\t%dx%d\t%d differ (%.3f %%), %d by more than 32, up to %d\t%s" % (
                    group, stem.replace(os.sep, "/"), actual.size[0], actual.size[1], any_count, 100.0 * any_count / pixels,
                    big_count, top, bounds))
                counts[group] = counts.get(group, 0) + 1
    print("counts\t" + ", ".join("%s %d" % item for item in sorted(counts.items())))


if __name__ == "__main__":
    main()
