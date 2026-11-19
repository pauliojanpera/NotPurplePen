"""Read OCAD's PDF export of the eighth or the ninth dash sample with the OCAD metric, and compare the dashes on its
Bezier curves with models of the placement.

Usage: measure-pdf-curves-metric.py CELLS.json EXPORT.pdf

The script reads each cell of one Bezier curve. Its symbol has end dashes as long as its dashes. Each model fits
OCAD's count of dashes, with the gap of the symbol, in a measure of the four quarters of the curve (t from 0 to 1/4,
and so on), and puts a point of a quarter at the same part of a length of the quarter:

- "now": the quarters measured by their length in the OCAD metric, and the points in proportion to that length. This
  is an even placement along the length of the curve in the OCAD metric, as Purple Pen places the dashes.
- "arc-euclid": the quarters measured by their length in the OCAD metric, and the points in proportion to the
  euclidean length of the arc.
- "chord-euclid": the quarters measured by their chords in the OCAD metric, and the points in proportion to the
  euclidean length of the arc. This is the model of fix 021 with the chords in the metric of the map.
- "chord-ocad": the quarters measured by their chords in the OCAD metric, and the points in proportion to the length
  of the arc in the OCAD metric.
- "euclid": the model of fix 021, with euclidean chords.

For each cell, the script prints the largest distance along the curve from an end of a dash of OCAD to the end that
each model gives. The reader of the PDF is the one of measure-pdf-vertices.py."""
import importlib.util
import json
import math
import os
import sys

HERE = os.path.dirname(os.path.abspath(__file__))
spec = importlib.util.spec_from_file_location("measure_pdf_curves", os.path.join(HERE, "measure-pdf-curves.py"))
mpc = importlib.util.module_from_spec(spec)
spec.loader.exec_module(mpc)

MODELS = ("now", "arc-euclid", "chord-euclid", "chord-ocad", "euclid")


def ocad_distance(a, b):
    """The distance of two points in the OCAD metric."""
    dx, dy = abs(b[0] - a[0]), abs(b[1] - a[1])
    return max(dx, dy) + min(dx, dy) / 2


def ocad_length(points, t0, t1, steps=200):
    """The length in the OCAD metric of the curve from the parameter t0 to t1."""
    q = [mpc.bezier(points, t0 + (t1 - t0) * k / steps) for k in range(steps + 1)]
    return sum(ocad_distance(q[k], q[k + 1]) for k in range(steps))


def euclid_length(points, t0, t1, steps=200):
    """The euclidean length of the curve from the parameter t0 to t1."""
    return mpc.length(points, t0, t1, steps)


def parameter_at(points, length_of, target, t0, t1):
    """The parameter after t0 at which the length from t0 is the target."""
    low, high = t0, t1
    for _ in range(40):
        middle = (low + high) / 2
        if length_of(points, t0, middle) < target:
            low = middle
        else:
            high = middle
    return (low + high) / 2


def place(points, count, gap, measures, length_of):
    """The parameters of the ends of the dashes: count dashes and their gaps fitted in the measures of the quarters, and
    each point of a quarter at the same part of its length by length_of."""
    dash = (sum(measures) - gap * (count - 1)) / count
    ends = []
    for k in range(count):
        for s in (k * (dash + gap), k * (dash + gap) + dash):
            quarter = 0
            while quarter < 3 and s > measures[quarter]:
                s -= measures[quarter]
                quarter += 1
            whole = length_of(points, quarter / 4, (quarter + 1) / 4)
            ends.append(parameter_at(points, length_of, min(s / measures[quarter], 1.0) * whole, quarter / 4, (quarter + 1) / 4))
    return ends


def models(points, count, gap):
    """The ends of the dashes by each model."""
    arcs = [ocad_length(points, k / 4, (k + 1) / 4) for k in range(4)]
    chords = [ocad_distance(mpc.bezier(points, k / 4), mpc.bezier(points, (k + 1) / 4)) for k in range(4)]
    euclid_chords = [math.dist(mpc.bezier(points, k / 4), mpc.bezier(points, (k + 1) / 4)) for k in range(4)]
    return {
        "now": place(points, count, gap, arcs, ocad_length),
        "arc-euclid": place(points, count, gap, arcs, euclid_length),
        "chord-euclid": place(points, count, gap, chords, euclid_length),
        "chord-ocad": place(points, count, gap, chords, ocad_length),
        "euclid": place(points, count, gap, euclid_chords, euclid_length),
    }


def main():
    cells = json.load(open(sys.argv[1]))
    paths, misfit = mpc.mpv.read_paths(sys.argv[2], cells["marks"])
    print("%-16s %6s  %s" % ("cell", "dashes", "  ".join("%12s" % name for name in MODELS)))
    largest = dict((name, 0.0) for name in MODELS)
    for cell in cells["cells"]:
        if [p[2] for p in cell["points"]] != ["N", "B", "B", "N"]:
            continue
        points = [tuple(p[:2]) for p in cell["points"]]
        gap = cells["symbols"][cell["symbol"]]["gap"]
        observed = [t for ends in mpc.dash_ends(cell, paths) for t in ends]
        count = len(observed) // 2
        if count == 0:
            continue
        row = []
        for name, ends in models(points, count, gap).items():
            error = max(euclid_length(points, min(a, b), max(a, b), 100) for a, b in zip(observed, ends))
            largest[name] = max(largest[name], error)
            row.append("%12.3f" % error)
        print("%-16s %6d  %s" % (cell["id"], count, "  ".join(row)))
    print("%-16s %6s  %s" % ("largest", "", "  ".join("%12.3f" % largest[name] for name in MODELS)))


if __name__ == "__main__":
    main()
