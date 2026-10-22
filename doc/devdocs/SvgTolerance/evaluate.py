"""Evaluate edge rules on the collected measurements: real renderings must all pass, and each rule's catches of the
planted errors are compared with the current rule's.

Usage: python3 -I evaluate.py <collected folder>
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


def fails(case, variant, rule):
    if variant + "_inside" not in case:
        return None
    if int(case[variant + "_inside"]) > 0:
        return True
    difference = case[variant + "_difference"]
    if difference.size == 0:
        return False
    return bool(rule(difference, case[variant + "_contrast9"], case[variant + "_contrast5"]).any())


def current(difference, contrast9, contrast5):
    return (difference > 40).any(axis=1)


def relative(share, floor, cap, which):
    def rule(difference, contrast9, contrast5):
        contrast = contrast9 if which == 9 else contrast5
        threshold = np.maximum(floor, np.minimum(cap, share * contrast))
        return (difference > threshold).any(axis=1)
    return rule


def summarize(cases, rule):
    real_failures = [name for name, case in cases.items() if fails(case, "real", rule)]
    caught = {}
    for variant in VARIANTS:
        caught[variant] = {name for name, case in cases.items() if fails(case, variant, rule)}
    return real_failures, caught


def main():
    cases = load(sys.argv[1])
    tested = {variant: {name for name, case in cases.items() if variant + "_inside" in case} for variant in VARIANTS}
    base_real, base_caught = summarize(cases, current)
    print("current: real failures", base_real, "| " + ", ".join(
        "%s %d/%d" % (variant, len(base_caught[variant]), len(tested[variant])) for variant in VARIANTS))
    for variant in VARIANTS:
        print("   missed by current, %s: %s" % (variant, " ".join(sorted(tested[variant] - base_caught[variant]))))

    rows = []
    for which, share, floor, cap in itertools.product((9, 5), (0.36, 0.38, 0.4, 0.42, 0.45, 0.5, 0.6, 0.75, 1.0),
                                                      (0, 2, 4, 6, 8), (40,)):
        real_failures, caught = summarize(cases, relative(share, floor, cap, which))
        superset = all(base_caught[variant] <= caught[variant] for variant in VARIANTS)
        faint = all(name in caught["shift2"] for name in ("decreasesymbols", "decreasesymbols6"))
        rows.append((which, share, floor, cap, len(real_failures), superset, faint,
                     tuple(len(caught[variant]) for variant in VARIANTS), real_failures[:4]))
    print("window share floor cap | real fails | superset | faint shift caught | shift2 wider removed redder")
    for row in rows:
        print("%d %.2f %d %d | %d %s | %s | %s | %s %s" % (row[0], row[1], row[2], row[3], row[4], "", row[5], row[6], row[7], row[8]))


if __name__ == "__main__":
    main()
