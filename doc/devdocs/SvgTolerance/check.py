"""Check one rule on the collected measurements: real failures, and catches per planted error against the current
rule. Usage: python3 -I check.py <collected folder> <share>"""
import os, sys
import numpy as np
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
cases = {}
for name in sorted(os.listdir(sys.argv[1])):
    with np.load(os.path.join(sys.argv[1], name)) as arrays:
        cases[name[:-4]] = {key: arrays[key] for key in arrays.files}
share = float(sys.argv[2])
def fails(case, variant, new):
    if int(case[variant + "_inside"]) > 0:
        return True
    d = case[variant + "_difference"]
    if d.size == 0:
        return False
    if not new:
        return bool((d > 40).any())
    contrast = case[variant + "_contrast9"].max(axis=1, keepdims=True)
    return bool((d > np.minimum(40, share * contrast)).any())
print("real failures:", [n for n, c in cases.items() if fails(c, "real", True)])
for variant in ("shift2", "wider", "removed", "redder"):
    tested = [n for n, c in cases.items() if variant + "_inside" in c]
    before = {n for n in tested if fails(cases[n], variant, False)}
    after = {n for n in tested if fails(cases[n], variant, True)}
    print("%-8s tested %3d  current %3d  new %3d  newly caught: %s  lost: %s" % (variant, len(tested), len(before), len(after), " ".join(sorted(after - before)), " ".join(sorted(before - after))))
    print("          still passing: %s" % " ".join(sorted(set(tested) - after)))
