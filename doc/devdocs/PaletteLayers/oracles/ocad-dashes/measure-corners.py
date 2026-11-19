"""Measure, for each cell of the fifth or the sixth dash sample, whether the dash near the second vertex takes the corner
of the miter, and where the boundary of that dash is in the drawing.

Usage: measure-corners.py CELLS.json DRAWING.png

Each cell is a line of three straight parts that turns to the right at the second vertex. A dash starts before the
vertex, or ends after it, by the distance that the field "delta" of the cell gives (exact arithmetic, the rule of
LineSymDef.ComputeDashDistances). The registration marks give the scale and the position of the drawing, as in
measure-dashes.py. All the scan lines are on the outer side of the turn.

- corner: how far the dark pixels go out from the vertex along the outer bisector, and what that tells. A dash that
  takes the corner of the miter covers the bisector to the tip of the miter, (w/2) / cos(turn/2), on a line of width
  w. A dash that starts or ends at the vertex with a flat end does not cover it. On a double line, the measure is
  across the outer border, at h / cos(turn/2), where h is the distance of the middle of the border from the path.
- drawn: the boundary that the drawing implies. When a dash starts before the vertex, the previous dash ends one gap
  (0.5 mm) before that start. When a dash ends after the vertex, the next dash starts one gap after that end. The
  flat end of that neighbouring dash is straight and oblique to the pixels, so the mean of where it crosses many
  parallel scan lines gives its position to a fraction of a pixel."""
import importlib.util
import json
import math
import os
import sys
from PIL import Image

spec = importlib.util.spec_from_file_location(
    "measure_dashes", os.path.join(os.path.dirname(os.path.abspath(__file__)), "measure-dashes.py"))
measure = importlib.util.module_from_spec(spec)
spec.loader.exec_module(measure)

GAP = 0.5


def unit(v):
    n = math.hypot(v[0], v[1])
    return (v[0] / n, v[1] / n)


def dark_runs(grid, origin, along, t_from, t_to, step=0.002):
    """The dark runs along origin + t * along, for t from t_from to t_to, in mm."""
    runs, start, t = [], None, t_from
    while t <= t_to + 1e-9:
        dark = grid.covered(origin[0] + along[0] * t, origin[1] + along[1] * t)
        if dark and start is None:
            start = t
        elif not dark and start is not None:
            runs.append((start, t))
            start = None
        t += step
    if start is not None:
        runs.append((start, t_to))
    return runs


def crossing(grid, origin, along, t_from, t_to, to_dark, step=0.001):
    """The first t from t_from to t_to where the drawing turns dark (or light)."""
    t = t_from
    while t <= t_to:
        if grid.covered(origin[0] + along[0] * t, origin[1] + along[1] * t) == to_dark:
            return t
        t += step
    return None


def flat_end(grid, vertex, along, across, u_range, t_range, to_dark):
    """The mean position of a flat end along `along` from the vertex, over scan lines at u in u_range across it."""
    found = []
    u = u_range[0]
    while u <= u_range[1] + 1e-9:
        origin = (vertex[0] + across[0] * u, vertex[1] + across[1] * u)
        t = crossing(grid, origin, along, t_range[0], t_range[1], to_dark)
        if t is not None:
            found.append(t)
        u += 0.01
    return sum(found) / len(found) if found else None


def measure_cell(grid, cell, sym):
    """The corner verdict, the reach along the bisector and the drawn boundary of one cell."""
    v1, v2, v3 = [p[:2] for p in cell["points"][1:4]]
    d1, d2 = unit((v2[0] - v1[0], v2[1] - v1[1])), unit((v3[0] - v2[0], v3[1] - v2[1]))
    n1, n2 = (-d1[1], d1[0]), (-d2[1], d2[0])   # The line turns right, so the left normals are on the outer side.
    bisector = unit((n1[0] + n2[0], n1[1] + n2[1]))
    cos_half = bisector[0] * n1[0] + bisector[1] * n1[1]
    if "double" in sym:
        h = sym["width"] / 2 + sym["border"] / 2
        tip = h / cos_half
        runs = dark_runs(grid, v2, bisector, tip - 0.3, tip + 0.3)
        corner = any(s <= tip <= e for s, e in runs)
        reach = max([e for s, e in runs if s <= tip <= e] or [0.0])
        u_range = (h - sym["border"] / 2 + 0.025, h + sym["border"] / 2 - 0.025)
    else:
        tip = sym["linewidth"] / 2 / cos_half
        runs = dark_runs(grid, v2, bisector, 0.0, tip + 0.2)
        reach = max([e for s, e in runs if s <= 0.01] or [0.0])
        # A bevel reaches (w/2) cos(turn/2), which is cos^2(turn/2) of the tip. A corner reaches more than a third of
        # the way from the bevel to the tip: a pixel of a thin line is a large part of its tip.
        bevel = cos_half * cos_half
        corner = reach > (bevel + (1 - bevel) / 3) * tip
        u_range = (0.03, sym["linewidth"] / 2 - 0.03)
    exact = abs(cell["delta"])
    if "start" in cell["id"]:
        place = flat_end(grid, v2, d1, n1, u_range, (-(exact + GAP) - 0.15, -(exact + GAP) + 0.15), to_dark=False)
        drawn = None if place is None else -place - GAP
    else:
        place = flat_end(grid, v2, d2, n2, u_range, (exact + GAP - 0.15, exact + GAP + 0.15), to_dark=True)
        drawn = None if place is None else place - GAP
    return corner, reach, tip, exact, drawn


def main():
    data = json.load(open(sys.argv[1]))
    grid = measure.Grid(Image.open(sys.argv[2]).convert("L"), data["marks"])
    print("%-13s %-6s %8s %8s %8s %8s %8s" % ("cell", "kind", "exact", "drawn", "diff", "corner", "reach/tip"))
    for cell in data["cells"]:
        corner, reach, tip, exact, drawn = measure_cell(grid, cell, data["symbols"][cell["symbol"]])
        print("%-13s %-6s %8.4f %8s %8s %8s %8.2f" % (
            cell["id"], "start" if "start" in cell["id"] else "end", exact,
            "-" if drawn is None else "%.4f" % drawn, "-" if drawn is None else "%+.4f" % (drawn - exact),
            "yes" if corner else "no", reach / tip))


if __name__ == "__main__":
    main()
