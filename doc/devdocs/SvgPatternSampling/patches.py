"""Choose, for each pattern of a document, the largest square patch of the rendering that pattern alone draws on.

Usage: python3 -I patches.py <folder of the probes' renderings> [--sizes 48,40,32,24] [--share 0.8]

The folder holds base.png and <id>.png, each Chromium's rendering of what locate.py wrote. A pixel belongs to a
pattern when the pattern's rendering differs there from the base by more than six levels in a channel. A patch is
taken when no other pattern draws on any of its pixels and that pattern draws on the share of them --share asks, so
that nothing but the screen and what lies over it is in the patch; the measurement then keeps the pixels that hold
the screen alone. Prints a row for each pattern that has one: its id, the patch's left, top and side, and
how many pixels of the rendering the pattern draws on in all.
"""
import os
import sys

import numpy as np
from PIL import Image


def mask_of(base, path):
    """The pixels one pattern draws on: those its rendering changed."""
    other = np.asarray(Image.open(path).convert("RGB")).astype(np.int16)
    return np.abs(base - other).max(axis=2) > 6


def boxes(mask, side):
    """How many pixels of the mask each square of the given side holds, by the square's left top corner."""
    sums = np.cumsum(np.cumsum(np.pad(mask.astype(np.int32), ((1, 0), (1, 0))), axis=0), axis=1)
    return sums[side:, side:] - sums[:-side, side:] - sums[side:, :-side] + sums[:-side, :-side]


def patch_of(mask, others, sides, share):
    """The largest square that pattern draws on and no other does, as left, top and side, or None."""
    for side in sides:
        alone = boxes(mask, side) >= share * side * side
        for other in others:
            alone &= boxes(other, side) == 0
        found = np.argwhere(alone)
        if len(found) > 0:
            top, left = found[len(found) // 2]
            return int(left), int(top), side
    return None


def main(arguments):
    if not arguments:
        sys.stderr.write(__doc__)
        return 2
    folder = arguments[0]
    sides = [int(side) for side in (arguments[arguments.index("--sizes") + 1] if "--sizes" in arguments
                                    else "48,40,32,24").split(",")]
    share = float(arguments[arguments.index("--share") + 1]) if "--share" in arguments else 0.8
    base = np.asarray(Image.open(os.path.join(folder, "base.png")).convert("RGB")).astype(np.int16)
    masks = {}
    for name in sorted(os.listdir(folder)):
        if name.endswith(".png") and name != "base.png":
            masks[name[:-len(".png")]] = mask_of(base, os.path.join(folder, name))
    for identifier, mask in masks.items():
        others = [other for key, other in masks.items() if key != identifier]
        patch = patch_of(mask, others, sides, share)
        if patch is not None:
            print("%s\t%d\t%d\t%d\t%d" % (identifier, patch[0], patch[1], patch[2], int(mask.sum())))
    return 0


if __name__ == "__main__":
    sys.exit(main(sys.argv[1:]))
