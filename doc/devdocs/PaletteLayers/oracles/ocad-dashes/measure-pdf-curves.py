"""Read OCAD's PDF export of the eighth dash sample, and compare the dashes on its Bezier curves with two models.

Usage: measure-pdf-curves.py CELLS.json EXPORT.pdf

For each cell of one Bezier curve (the rows "straight" and "arcs"), the script finds the parameter of each end of
each dash of OCAD, and prints the distance along the curve from each end to the end that a model gives:

- "even": the dashes are even along the true length of the curve, with the count of dashes of OCAD.
- "quarters": the length of each quarter of the curve (t from 0 to 1/4, and so on) is its chord. The dashes are even
  in that measure, and a point of a quarter is at the same part of the true length of the quarter.

A positive distance is a model end after OCAD's end. The reader of the PDF is the one of measure-pdf-vertices.py."""
import importlib.util
import json
import math
import os
import sys

HERE = os.path.dirname(os.path.abspath(__file__))
spec = importlib.util.spec_from_file_location("mpv", os.path.join(HERE, "measure-pdf-vertices.py"))
mpv = importlib.util.module_from_spec(spec)
spec.loader.exec_module(mpv)
STEPS = 400


def bezier(ps, t):
    """The point of a cubic Bezier curve at a parameter."""
    u = 1 - t
    return tuple(u ** 3 * ps[0][i] + 3 * u * u * t * ps[1][i] + 3 * u * t * t * ps[2][i] + t ** 3 * ps[3][i]
                 for i in (0, 1))


def length(ps, ta, tb, steps=STEPS):
    """The length of the curve between two parameters, along a polyline of the given number of chords."""
    qs = [bezier(ps, ta + (tb - ta) * k / steps) for k in range(steps + 1)]
    return sum(math.dist(qs[k], qs[k + 1]) for k in range(steps))


def parameter_at(ps, s, t0=0.0, t1=1.0):
    """The parameter at a length along the curve from t0."""
    lo, hi = t0, t1
    for _ in range(50):
        mid = (lo + hi) / 2
        if length(ps, t0, mid, 200) < s:
            lo = mid
        else:
            hi = mid
    return (lo + hi) / 2


def nearest_parameter(ps, q):
    """The parameter of the point of the curve nearest a point, and the distance to it."""
    t = min((k / 2000 for k in range(2001)), key=lambda c: math.dist(bezier(ps, c), q))
    step = 1 / 2000
    for _ in range(30):
        step /= 2
        t = min((c for c in (t - step, t, t + step) if 0 <= c <= 1), key=lambda c: math.dist(bezier(ps, c), q))
    return t, math.dist(bezier(ps, t), q)


def dash_ends(cell, paths):
    """The parameters of the ends of OCAD's dashes on the curve of a cell, in order."""
    ps = [tuple(p[:2]) for p in cell["points"]]
    lo = [min(q[i] for q in ps) - 0.3 for i in (0, 1)]
    hi = [max(q[i] for q in ps) + 0.3 for i in (0, 1)]
    ends = []
    for p in mpv.symbol_paths(paths, cell["symbol"]):
        if not all(lo[0] <= q[0] <= hi[0] and lo[1] <= q[1] <= hi[1] for q in p["mm"]):
            continue
        (a, da), (b, db) = nearest_parameter(ps, p["mm"][0]), nearest_parameter(ps, p["mm"][-1])
        if da < 0.05 and db < 0.05:
            ends.append((min(a, b), max(a, b)))
    return sorted(ends)


def model_ends(ps, count, gap, model):
    """The parameters of the ends of the dashes of a model, for a count of dashes."""
    if model == "even":
        total = length(ps, 0, 1)
    else:
        chords = [math.dist(bezier(ps, k / 4), bezier(ps, (k + 1) / 4)) for k in range(4)]
        total = sum(chords)
    dash = (total - gap * (count - 1)) / count
    out = []
    for k in range(count):
        for s in (k * (dash + gap), k * (dash + gap) + dash):
            if model == "even":
                out.append(parameter_at(ps, s))
            else:
                quarter = 0
                while quarter < 3 and s > chords[quarter]:
                    s -= chords[quarter]
                    quarter += 1
                part = min(s / chords[quarter], 1.0) * length(ps, quarter / 4, (quarter + 1) / 4)
                out.append(parameter_at(ps, part, quarter / 4, (quarter + 1) / 4))
    return out


def main():
    cells = json.load(open(sys.argv[1]))
    paths, misfit = mpv.read_paths(sys.argv[2], cells["marks"])
    print("The registration marks fit to %.4f mm." % misfit)
    for cell in cells["cells"]:
        if cell["row"] not in ("straight", "arcs") or len(cell["points"]) != 4:
            continue
        ps = [tuple(p[:2]) for p in cell["points"]]
        gap = cells["symbols"][cell["symbol"]]["gap"]
        observed = [t for ends in dash_ends(cell, paths) for t in ends]
        print("%s: %s, %d dashes" % (cell["id"], cell["note"], len(observed) // 2))
        for model in ("even", "quarters"):
            ends = model_ends(ps, len(observed) // 2, gap, model)
            errors = [length(ps, min(a, b), max(a, b)) * (1 if b > a else -1) for a, b in zip(observed, ends)]
            print("   %-8s largest %.3f mm: %s" % (model, max(abs(e) for e in errors),
                                                 " ".join("%+.3f" % e for e in errors)))


if __name__ == "__main__":
    main()
