"""Evaluate edge rules that allow a rendering more where it is darker than its reference than where it is lighter, on
measurements collect.py took with signed differences: every real rendering must pass, and the planted errors each
rule catches are set against those of the rule that allows both directions alike.

Usage: python3 -I direction.py <collected folder> [cases...]
"""
import itertools
import os
import sys
import numpy as np

VARIANTS = ["shift2", "wider", "removed", "redder"]


def load(folder, names):
    cases = {}
    for name in sorted(os.listdir(folder)):
        if names and name[:-4] not in names:
            continue
        with np.load(os.path.join(folder, name)) as arrays:
            if "real_signed" not in arrays.files:
                raise SystemExit(name + " was collected without signed differences")
            cases[name[:-4]] = {key: arrays[key] for key in arrays.files}
    return cases


def rule(share, cap, dark_share, dark_cap, floor=0):
    """Fails a pixel lighter than its reference by more than share of the contrast around it, at most cap, and one
    darker by more than dark_share of it, at most dark_cap; never one within floor levels."""
    def fails(signed, contrast):
        light = np.maximum(floor, np.minimum(cap, share * contrast))
        dark = np.maximum(floor, np.minimum(dark_cap, dark_share * contrast))
        return ((-signed > light) | (signed > dark)).any(axis=1)
    return fails


def fails(case, variant, test):
    if variant + "_inside" not in case:
        return None
    if int(case[variant + "_inside"]) > 0:
        return True
    signed = case[variant + "_signed"]
    return signed.size > 0 and bool(test(signed, case[variant + "_contrast9"]).any())


def summarize(cases, test):
    real = sorted(name for name, case in cases.items() if fails(case, "real", test))
    caught = {variant: {name for name, case in cases.items() if fails(case, variant, test)} for variant in VARIANTS}
    return real, caught


def extremes(cases):
    """Per case, how much darker and how much lighter the real rendering is at its rescued pixels, in levels and as a
    share of the contrast around them."""
    print("case | outside with no edge near | darkest: levels share | lightest: levels share")
    for name, case in cases.items():
        signed, contrast = case["real_signed"], np.maximum(case["real_contrast9"], 1)
        if signed.size == 0 and int(case["real_inside"]) == 0:
            continue
        darkest = (signed.max(), (signed / contrast).max()) if signed.size else (0, 0)
        lightest = (-signed.min(), (-signed / contrast).max()) if signed.size else (0, 0)
        print("%-28s | %4d | %6.1f %5.2f | %6.1f %5.2f" % ((name, int(case["real_inside"])) + darkest + lightest))


def main():
    cases = load(sys.argv[1], set(sys.argv[2:]))
    tested = {variant: {name for name, case in cases.items() if variant + "_inside" in case} for variant in VARIANTS}
    extremes(cases)
    print()
    print("light: share cap | dark: share cap | real fails | " + " ".join(VARIANTS))
    for share, cap, dark_share, dark_cap in itertools.chain(
            [(0.4, 40, 0.4, 40), (0.4, 76, 0.4, 76), (0.4, 80, 0.4, 80)],
            itertools.product((0.4,), (40,), (0.4, 0.5, 0.6, 0.75, 1.0), (40, 60, 76, 80, 100, 128))):
        real, caught = summarize(cases, rule(share, cap, dark_share, dark_cap))
        print("%.2f %3d | %.2f %3d | %2d | %s | %s" % (
            share, cap, dark_share, dark_cap, len(real),
            " ".join("%d/%d" % (len(caught[variant]), len(tested[variant])) for variant in VARIANTS), " ".join(real[:5])))


if __name__ == "__main__":
    main()
