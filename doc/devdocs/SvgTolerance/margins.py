"""For an edge rule that passes a rescued pixel when each channel's blurred difference is at most
max(floor, min(40, share x contrast)), find the smallest share that passes every real rendering and the largest share
that still fails each faint case shifted, for several contrast measures and floors.

Usage: python3 -I margins.py <collected folder>
"""
import os
import sys
import numpy as np

FAINT = ("decreasesymbols", "decreasesymbols6")


def load(folder):
    cases = {}
    for name in sorted(os.listdir(folder)):
        with np.load(os.path.join(folder, name)) as arrays:
            cases[name[:-4]] = {key: arrays[key] for key in arrays.files}
    return cases


def ratios(case, variant, measure, floor):
    """The ratio of blurred difference to contrast at every channel whose difference exceeds the floor."""
    difference = case[variant + "_difference"]
    contrast = case[variant + "_contrast9" if measure.startswith("9") else variant + "_contrast5"]
    if measure.endswith("max"):
        contrast = np.repeat(contrast.max(axis=1, keepdims=True), 3, axis=1)
    over = difference > floor
    return (difference[over] / np.maximum(contrast[over], 1e-9)) if over.any() else np.zeros(0)


def main():
    cases = load(sys.argv[1])
    print("measure floor | smallest share passing every real rendering (and where) | largest share failing each faint shift")
    for measure in ("9", "9max", "5", "5max"):
        for floor in (0, 1, 2, 3, 4, 5, 6):
            need, where = 0.0, ""
            for name, case in cases.items():
                values = ratios(case, "real", measure, floor)
                if values.size and values.max() > need:
                    need, where = float(values.max()), name
            reach = []
            for name in FAINT:
                values = ratios(cases[name], "shift2", measure, floor)
                reach.append(float(values.max()) if values.size else 0.0)
            print("%-5s %d | %.3f (%s) | %s | window %s" % (measure, floor, need, where, " ".join("%.3f" % value for value in reach),
                  "%.2f" % (min(reach) / need) if need else "-"))


if __name__ == "__main__":
    main()
