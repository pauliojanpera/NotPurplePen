"""Compare two local runs of linux-renderings.sh: the renderings and the tests that fail.

Usage: python3 -I compare-renderings.py <results without the change> <results with the change>

A run keeps the rendering of each comparison that differs from its baseline, as <baseline>_actual.png. An image that
only one run kept matches its baseline in the other run, so the change moved it. An image that both runs kept moved
when the two renderings differ in a pixel. The script prints each image that moved, with the number of pixels that
differ, and the tests whose outcome differs between the two runs, from the trx of each run. Exits with 1 when an image
moved or an outcome differs, and with 0 otherwise.
"""
import os
import sys
import xml.etree.ElementTree as ElementTree
from functools import reduce

from PIL import Image, ImageChops

NAMESPACE = "{http://microsoft.com/schemas/VisualStudio/TeamTest/2010}"


def renderings(results):
    """Gives the paths of the renderings of a run, relative to its images folder."""
    images = os.path.join(results, "images")
    found = set()
    for folder, _, files in os.walk(images):
        for name in files:
            if name.endswith("_actual.png"):
                found.add(os.path.relpath(os.path.join(folder, name), images))
    return found


def differing_pixels(first, second):
    """Gives the number of pixels in which two images differ, or None when their sizes differ."""
    first = Image.open(first).convert("RGBA")
    second = Image.open(second).convert("RGBA")
    if first.size != second.size:
        return None
    largest = reduce(ImageChops.lighter, ImageChops.difference(first, second).split())
    histogram = largest.histogram()
    return sum(histogram[1:])


def outcomes(results):
    """Gives the outcome of each test of the trx files of a run, by the name of the test."""
    found = {}
    for name in sorted(os.listdir(results)):
        if name.endswith(".trx"):
            root = ElementTree.parse(os.path.join(results, name)).getroot()
            for result in root.iter(NAMESPACE + "UnitTestResult"):
                found[result.get("testName")] = result.get("outcome")
    return found


def main():
    if len(sys.argv) != 3:
        print(__doc__.strip().split("\n")[2], file=sys.stderr)
        return 2
    without, with_change = sys.argv[1], sys.argv[2]

    moved = []
    unchanged = 0
    before, after = renderings(without), renderings(with_change)
    for path in sorted(before | after):
        if path not in after:
            moved.append((path, "matches its baseline with the change"))
        elif path not in before:
            moved.append((path, "matches its baseline without the change"))
        else:
            count = differing_pixels(os.path.join(without, "images", path), os.path.join(with_change, "images", path))
            if count is None:
                moved.append((path, "the sizes differ"))
            elif count:
                moved.append((path, "%d pixels differ" % count))
            else:
                unchanged += 1
    for path, how in moved:
        print("moved: %s: %s" % (path[:-len("_actual.png")] + ".png", how))
    print("%d images moved; %d renderings differ from their baselines in both runs, and not from each other"
          % (len(moved), unchanged))

    first, second = outcomes(without), outcomes(with_change)
    changed = [name for name in sorted(set(first) | set(second)) if first.get(name) != second.get(name)]
    for name in changed:
        print("outcome: %s: %s without the change, %s with it"
              % (name, first.get(name, "absent"), second.get(name, "absent")))
    failed = sum(1 for outcome in second.values() if outcome == "Failed")
    print("%d tests differ in their outcome; %d of %d fail with the change" % (len(changed), failed, len(second)))
    return 1 if moved or changed else 0


if __name__ == "__main__":
    sys.exit(main())
