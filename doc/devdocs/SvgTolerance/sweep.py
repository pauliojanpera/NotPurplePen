"""Sweep edge rules of one form over collected measurements: a pixel outside the band at an edge passes when its
blurred difference is at most the floor, or at most the share of the contrast around it and at most the cap. For each
rule that fails at most two real renderings, print those, the planted errors it catches and the removals it passes.

Usage: python3 -I sweep.py <collected folder>
"""
import itertools
import os
import sys
import numpy as np

VARIANTS = ["shift2", "wider", "removed", "redder"]


def load(folder):
    cases = {}
    for name in sorted(os.listdir(folder)):
        with np.load(os.path.join(folder, name)) as arrays:
            cases[name[:-4]] = {key: arrays[key] for key in arrays.files}
    return cases


def fails(case, variant, share, cap, floor):
    if variant + "_inside" not in case:
        return None
    if int(case[variant + "_inside"]) > 0:
        return True
    difference = case[variant + "_difference"]
    if difference.size == 0:
        return False
    allowed = np.maximum(floor, np.minimum(cap, share * case[variant + "_contrast9"]))
    return bool((difference > allowed).any())


def main():
    cases = load(sys.argv[1])
    tested = {variant: sum(1 for case in cases.values() if variant + "_inside" in case) for variant in VARIANTS}
    print("share cap floor | real fails | " + " ".join("%s/%d" % (variant, tested[variant]) for variant in VARIANTS) +
          " | removals passed")
    for share, cap, floor in itertools.product((0.30, 0.32, 0.34, 0.36, 0.38, 0.40), (40, 76, 80, 90, 255), (0, 12, 20)):
        real = [name for name, case in cases.items() if fails(case, "real", share, cap, floor)]
        if len(real) > 2:
            continue
        caught = {variant: sum(1 for case in cases.values() if fails(case, variant, share, cap, floor))
                  for variant in VARIANTS}
        passed = [name for name, case in cases.items() if fails(case, "removed", share, cap, floor) is False]
        print("%.2f %3d %2d | %d %s | %s | %s" % (share, cap, floor, len(real), ",".join(real),
                                                 " ".join(str(caught[variant]) for variant in VARIANTS), " ".join(passed)))


if __name__ == "__main__":
    main()
