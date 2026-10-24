"""For each case, the real rendering and each planted error side by side: how many pixels lie outside the band with no
edge near (which every rule fails), and at the rescued pixel with the largest share of the contrast around it, that
share and the blurred difference there in levels. A planted error that comes no higher than real renderings do is one
that no edge rule can fail while it passes them.

Usage: python3 -I shares.py <collected folder>
"""
import os
import sys
import numpy as np

VARIANTS = ["real", "removed", "wider", "redder", "shift2"]


def describe(arrays, variant):
    if variant + "_inside" not in arrays.files:
        return "      -       "
    inside = int(arrays[variant + "_inside"])
    difference, contrast = arrays[variant + "_difference"], np.maximum(arrays[variant + "_contrast9"], 1)
    if difference.size == 0:
        return "%5d   -     - " % inside
    shares = (difference / contrast).max(axis=1)
    largest = int(np.argmax(shares))
    return "%5d %5.2f %4.0f" % (inside, shares[largest], difference[largest].max())


def main():
    folder = sys.argv[1]
    print("%-26s " % "case" + " | ".join("%-16s" % (variant + " out sh lv") for variant in VARIANTS))
    for name in sorted(os.listdir(folder)):
        with np.load(os.path.join(folder, name)) as arrays:
            print("%-26s " % name[:-4] + " | ".join(describe(arrays, variant) for variant in VARIANTS))


if __name__ == "__main__":
    main()
