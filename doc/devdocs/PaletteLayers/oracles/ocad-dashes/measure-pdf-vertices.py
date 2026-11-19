"""Read OCAD's PDF export of a dash sample whose cells turn at a vertex near a boundary of a dash (the sixth sample),
and tell for each cell whether OCAD keeps the vertex in the dash and how long the piece beyond the vertex is.

Usage: measure-pdf-vertices.py CELLS.json EXPORT.pdf

The PDF has each dash as a stroked path, on a layer for each symbol, whose title starts with the number and the id of
the symbol ("501.000 W ..."). The registration marks give the position of the paper. OCAD puts each point of a dash
on its grid of 0.01 mm, so the script gives the points in those units.

For a single line, the dash is one path. For a double line, the dash is a path on each border, offset from the dash of
the middle, and the boundary of the dash of the middle is the midpoint of the ends of the two borders. The piece is
the distance from the vertex to the boundary, which is on the grid and so not exactly on the part."""
import json
import math
import re
import sys
import zlib

PT = 25.4 / 72
NUMBER = rb"[-+]?(?:\d+\.?\d*|\.\d+)"
TOKEN = re.compile(rb"\((?:\\.|[^\\)])*\)|<<|>>|\[|\]|/[^\s/\[\]()<>]+|" + NUMBER + rb"|[A-Za-z'\"*]+")


def decoded_streams(data):
    """The decoded streams of a PDF file without object streams (OCAD writes PDF 1.4)."""
    for m in re.finditer(rb"(\d+) 0 obj\s*<<(.*?)>>\s*stream\r?\n", data, re.S):
        raw = data[m.end():data.index(b"endstream", m.end())]
        yield zlib.decompress(raw) if b"FlateDecode" in m.group(2) else raw


def painted_subpaths(stream):
    """Each painted subpath of a content stream, with its layer title, its operator and its line width in points."""
    out, subpaths, operands = [], [], []
    layer, title, width, widths = None, None, 1.0, []
    for tok in TOKEN.findall(stream):
        if tok.startswith(b"("):
            title = tok[1:-1].decode("latin-1")
        elif re.fullmatch(NUMBER, tok):
            operands.append(float(tok))
            continue
        elif tok in (b"<<", b">>", b"[", b"]") or tok.startswith(b"/"):
            continue
        else:
            op = tok.decode()
            if op == "BDC":
                layer = title
            elif op == "q":
                widths.append(width)
            elif op == "Q":
                width = widths.pop()
            elif op == "w":
                width = operands[-1]
            elif op == "cm" and operands[-6:] != [1, 0, 0, 1, 0, 0]:
                raise ValueError("a transformation (cm) is not handled")
            elif op == "m":
                subpaths.append([(operands[-2], operands[-1])])
            elif op == "l":
                subpaths[-1].append((operands[-2], operands[-1]))
            elif op == "c":
                subpaths[-1].extend([(operands[-6], operands[-5]), (operands[-4], operands[-3]),
                                     (operands[-2], operands[-1])])
            elif op == "h":
                subpaths[-1].append(subpaths[-1][0])
            elif op in ("S", "s", "f", "F", "f*", "B", "B*", "b", "b*", "n"):
                if op != "n":
                    out.extend(dict(layer=layer, op=op, width=width, points=sp) for sp in subpaths)
                subpaths = []
            operands = []
    return out


def read_paths(pdf_path, marks_mm):
    """The painted subpaths of an OCAD PDF export, with their points in mm on the paper of the cells file."""
    paths = [p for stream in decoded_streams(open(pdf_path, "rb").read()) for p in painted_subpaths(stream)]
    centres = []
    for p in paths:
        if p["layer"] and "Registration mark" in p["layer"] and p["op"] == "f":
            xs, ys = [x for x, y in p["points"]], [y for x, y in p["points"]]
            centres.append(((min(xs) + max(xs)) / 2, (min(ys) + max(ys)) / 2))
    order = lambda m: (-round(m[1], 1), round(m[0], 1))
    pairs = list(zip(sorted(marks_mm, key=order), sorted(centres, key=order)))
    ox = sum(mm[0] - pt[0] * PT for mm, pt in pairs) / len(pairs)
    oy = sum(mm[1] - pt[1] * PT for mm, pt in pairs) / len(pairs)
    misfit = max(math.hypot(mm[0] - pt[0] * PT - ox, mm[1] - pt[1] * PT - oy) for mm, pt in pairs)
    for p in paths:
        p["mm"] = [(x * PT + ox, y * PT + oy) for x, y in p["points"]]
    return paths, misfit


def measure_vertices(cells, paths):
    """For each cell: (cell, kept, piece in units, piece by exact arithmetic in units), or (cell, None, ...)."""
    for cell in cells["cells"]:
        sym = cells["symbols"][cell["symbol"]]
        v2 = tuple(cell["points"][2][:2])
        starts = "start" in cell["id"]
        reach = abs(cell["delta"]) + (sym["width"] / 2 + sym["border"] + 0.3 if "double" in sym else 0.3)
        found = []
        for p in paths:
            if not p["layer"] or p["layer"].split()[1:2] != [cell["symbol"]]:
                continue
            near, far = (p["mm"][0], p["mm"][-1]) if starts else (p["mm"][-1], p["mm"][0])
            if math.hypot(near[0] - v2[0], near[1] - v2[1]) <= reach and math.dist(near, far) > 1.0:
                found.append(p)
        if not found:
            yield cell, None, None, abs(cell["delta"]) * 100
            continue
        # The PDF gives the points to 0.01 pt, a third of a unit, so the nearest point of the grid is OCAD's point.
        ends = [[round(c * 100) / 100 for c in (p["mm"][0] if starts else p["mm"][-1])] for p in found]
        end = (sum(e[0] for e in ends) / len(ends), sum(e[1] for e in ends) / len(ends))
        kept = all(len(p["mm"]) >= 3 for p in found)
        yield cell, kept, math.dist(end, v2) * 100, abs(cell["delta"]) * 100


def main():
    cells = json.load(open(sys.argv[1]))
    paths, misfit = read_paths(sys.argv[2], cells["marks"])
    print("The registration marks fit to %.4f mm." % misfit)
    print("%-13s %-6s %8s %8s %8s" % ("cell", "kind", "vertex", "piece", "exact"))
    for cell, kept, piece, exact in measure_vertices(cells, paths):
        print("%-13s %-6s %8s %8s %8.2f" % (cell["id"], "start" if "start" in cell["id"] else "end",
                                           "-" if kept is None else ("kept" if kept else "left out"),
                                           "-" if piece is None else "%.2f" % piece, exact))


if __name__ == "__main__":
    main()
