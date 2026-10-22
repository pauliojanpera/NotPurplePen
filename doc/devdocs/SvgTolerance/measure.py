"""Measure, for every pixel that the current edge criterion rescues (outside the band, with an edge within reach), how
far the blurred images differ and how much contrast lies around it, in each real rendering of the 101 drawn cases.

Usage: python3 -I measure.py <data folder>
"""
import os
import sys
import numpy as np
from PIL import Image
from scipy import ndimage

SLACK = 4


def load(path):
    return np.asarray(Image.open(path).convert("RGB")).astype(np.float64)


def window(image, size, highest):
    filter_ = ndimage.maximum_filter if highest else ndimage.minimum_filter
    return filter_(image, size=(size, size, 1), mode="constant", cval=255)


def blur(image):
    return ndimage.gaussian_filter(image, sigma=(1, 1, 0), mode="constant", cval=255)


def analyse(e, r):
    """Return the rescued mask, the blurred difference per pixel (largest over channels) and contrast measures."""
    outside = np.zeros(e.shape[:2], dtype=bool)
    for one, other in ((e, r), (r, e)):
        low, high = window(one, 3, False) - SLACK, window(one, 3, True) + SLACK
        outside |= ((other < low) | (other > high)).any(axis=2)
    range3 = np.maximum(window(e, 3, True) - window(e, 3, False), window(r, 3, True) - window(r, 3, False))
    at_edge = (range3 > SLACK).any(axis=2)
    difference = np.abs(blur(e) - blur(r))
    range9 = np.maximum(window(e, 9, True) - window(e, 9, False), window(r, 9, True) - window(r, 9, False))
    return outside & at_edge, outside & ~at_edge, difference, range3, range9


def main():
    data = sys.argv[1]
    names = sorted(name[:-4] for name in os.listdir(os.path.join(data, "references")))
    worst = []
    for name in names:
        e = load(os.path.join(data, "references", name + ".png"))
        r = load(os.path.join(data, "renderings", name + ".png"))
        rescued, inside_fail, difference, range3, range9 = analyse(e, r)
        if inside_fail.any():
            print("INSIDE FAILURE", name, int(inside_fail.sum()))
        if not rescued.any():
            continue
        d = difference.max(axis=2)[rescued]
        # Per channel ratios: the blurred difference of a channel against that channel's contrast.
        ratio3 = (difference / np.maximum(range3, 1)).max(axis=2)[rescued]
        ratio9 = (difference / np.maximum(range9, 1)).max(axis=2)[rescued]
        c9 = range9.max(axis=2)[rescued]
        worst.append((name, int(rescued.sum()), float(d.max()), float(ratio3.max()), float(ratio9.max()), float(c9.min())))
    worst.sort(key=lambda row: -row[4])
    print("case                          rescued  maxD  max D/range3  max D/range9  min range9")
    for row in worst[:25]:
        print("%-28s %8d %6.1f %12.3f %12.3f %10.0f" % row)
    print("cases with rescued pixels:", len(worst))
    print("overall max D %.1f, max D/range3 %.3f, max D/range9 %.3f" % (max(r[2] for r in worst), max(r[3] for r in worst), max(r[4] for r in worst)))


if __name__ == "__main__":
    main()
