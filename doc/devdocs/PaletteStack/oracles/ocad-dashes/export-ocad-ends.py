"""Write the dashes or the glyphs of OCAD's PDF export of a dash sample as the test data of Map_Skia.Tests.

Usage: export-ocad-ends.py CELLS.json EXPORT.pdf OUT.json dashes|glyphs [single-curves]

For each cell, the script writes the points of its path and what OCAD draws along it, in mm on the paper of the
cells file, as OcadDashesOnCurvesTests reads them:

- "dashes": each dash as its start and its end, [x0, y0, x1, y1], in the order along the path. These are the first and
  the last point of the subpath that OCAD paints for the dash.
- "glyphs": the center of each glyph, [x, y], in the order along the path: the center of the bounds of the filled
  subpath that OCAD paints for it.

A dash or a glyph belongs to a cell when its ends, or its center, are within 0.1 mm of the path of the cell, as
measure-pdf-paths.py finds them. The cells are those of the symbols with dashes, or with glyphs. "single-curves" takes
only the cells of one Bezier curve, as measure-pdf-curves.py does for the ninth sample. The reader of the PDF is the
one of measure-pdf-vertices.py."""
import importlib.util
import json
import os
import sys

HERE = os.path.dirname(os.path.abspath(__file__))
spec = importlib.util.spec_from_file_location("mpp", os.path.join(HERE, "measure-pdf-paths.py"))
mpp = importlib.util.module_from_spec(spec)
spec.loader.exec_module(mpp)
mpv = mpp.mpv


def ends_along(cell, paths, kind):
    """The dashes or the glyphs of OCAD along the path of a cell, in order along the path."""
    path = mpp.Path([tuple(p) for p in cell["points"]])
    xs = [p[0] for p in cell["points"]]
    ys = [p[1] for p in cell["points"]]
    lo, hi = (min(xs) - 1, min(ys) - 1), (max(xs) + 1, max(ys) + 1)
    found = []
    for p in mpv.symbol_paths(paths, cell["symbol"]):
        if not all(lo[0] <= q[0] <= hi[0] and lo[1] <= q[1] <= hi[1] for q in p["mm"]):
            continue
        if kind == "glyphs":
            if p["op"] not in ("f", "F", "f*", "b", "B"):
                continue
            qx, qy = [q[0] for q in p["mm"]], [q[1] for q in p["mm"]]
            centre = ((min(qx) + max(qx)) / 2, (min(qy) + max(qy)) / 2)
            arc, dist = path.project(centre)
            if dist < 0.1:
                found.append((arc, [round(centre[0], 4), round(centre[1], 4)]))
        else:
            first, last = p["mm"][0], p["mm"][-1]
            (a, da), (b, db) = path.project(first), path.project(last)
            if da < 0.1 and db < 0.1:
                if b < a:
                    first, last, a = last, first, b
                found.append((a, [round(first[0], 4), round(first[1], 4), round(last[0], 4), round(last[1], 4)]))
    return [item for arc, item in sorted(found, key=lambda pair: pair[0])]


def main():
    cells = json.load(open(sys.argv[1]))
    paths, misfit = mpv.read_paths(sys.argv[2], cells["marks"])
    kind = sys.argv[4]
    single_curves = sys.argv[5:6] == ["single-curves"]
    source = "OCAD 2020 viewer, PDF export %s of the dash sample (oracles/ocad-dashes.md of the palette-stack work)" % (
        os.path.basename(sys.argv[2]))
    out = {"source": source, "symbols": {}, "cells": []}
    for cell in cells["cells"]:
        sym = cells["symbols"][cell["symbol"]]
        if ("glyph" in sym) != (kind == "glyphs"):
            continue
        if single_curves and [p[2] for p in cell["points"]] != ["N", "B", "B", "N"]:
            continue
        out["symbols"][cell["symbol"]] = sym
        out["cells"].append({
            "id": cell["id"],
            "symbol": cell["symbol"],
            "points": [[p[0], p[1]] for p in cell["points"]],
            kind: ends_along(cell, paths, kind),
        })
    json.dump(out, open(sys.argv[3], "w"), indent=1)
    print("The registration marks fit to %.4f mm. %d cells." % (misfit, len(out["cells"])))


if __name__ == "__main__":
    main()
