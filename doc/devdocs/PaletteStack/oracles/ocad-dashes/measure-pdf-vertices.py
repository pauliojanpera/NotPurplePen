"""Read OCAD's PDF export of a dash sample whose cells turn at a vertex near a boundary of a dash (the sixth and the
seventh sample), and tell for each cell whether OCAD keeps the vertex in the dash and how long the piece beyond the
vertex is.

Usage: measure-pdf-vertices.py CELLS.json EXPORT.pdf

The PDF has each dash as a stroked path, on a layer for each symbol, whose title starts with the number and the id of
the symbol ("501.000 W ..."). The registration marks give the position of the paper. OCAD puts each point of a dash
on its grid of 0.01 mm, so the script gives the points in those units.

For a single line, the dash is one path. For a double line, the dash is a path on each border, offset from the dash of
the middle, and the boundary of the dash of the middle is the midpoint of the ends of the two borders. A dashed main
line of a double line has paths of its own, of its own width, and the script tells it apart. The piece is the
distance from the vertex to the boundary, which is on the grid and so not exactly on the part.

The script prints each cell, then the limits of each row: the longest piece whose vertex OCAD leaves out and the
shortest whose vertex it keeps. A cell without a boundary near a vertex (a scope cell of the seventh sample) gets the
paths of its symbol near its points, with each point that is a point of the cell named by its index."""
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


def symbol_paths(paths, symbol):
    """The paths on the layer of a symbol."""
    return [p for p in paths if p["layer"] and p["layer"].split()[1:2] == [symbol]]


def line_groups(sym):
    """The lines of a symbol that have dashes of their own, by name, with their widths in mm."""
    if "double" not in sym:
        return {"line": sym["linewidth"]}
    groups = {"borders": sym["border"]}
    if sym.get("linewidth"):
        groups["main"] = sym["linewidth"]
    return groups


def path_length(path):
    """The length of a path in mm: a dash is longer than 1 mm even where it turns back."""
    return sum(math.dist(path["mm"][i], path["mm"][i + 1]) for i in range(len(path["mm"]) - 1))


def on_grid(point):
    """The point of OCAD's grid of 0.01 mm nearest to a point of the PDF, which gives a third of a unit."""
    return (round(point[0] * 100) / 100, round(point[1] * 100) / 100)


def measure_vertices(cells, paths):
    """For each line of each vertex cell: (cell, line, kept, piece in units, piece by exact arithmetic in units).
    kept and piece are None when no dash of the line is near the vertex."""
    for cell in cells["cells"]:
        if "delta" not in cell:
            continue
        sym = cells["symbols"][cell["symbol"]]
        v2 = tuple(cell["points"][2][:2])
        starts = "start" in cell["id"]
        reach = abs(cell["delta"]) + (sym["width"] / 2 + sym["border"] + 0.3 if "double" in sym else 0.3)
        groups = line_groups(sym)
        found = {name: [] for name in groups}
        for p in symbol_paths(paths, cell["symbol"]):
            near = p["mm"][0] if starts else p["mm"][-1]
            if math.dist(near, v2) <= reach and path_length(p) > 1.0:
                name = min(groups, key=lambda g: abs(groups[g] - p["width"] * PT))
                found[name].append(p)
        for name in groups:
            if not found[name]:
                yield cell, name, None, None, abs(cell["delta"]) * 100
                continue
            ends = [on_grid(p["mm"][0] if starts else p["mm"][-1]) for p in found[name]]
            end = (sum(e[0] for e in ends) / len(ends), sum(e[1] for e in ends) / len(ends))
            kept = all(len(p["mm"]) >= 3 for p in found[name])
            yield cell, name, kept, math.dist(end, v2) * 100, abs(cell["delta"]) * 100


def scope_paths(cells, paths):
    """For each cell without a boundary near a vertex: (cell, the points of each path of its symbol near the cell, in
    units from the first point of the cell, with the index of each point that is a point of the cell)."""
    for cell in cells["cells"]:
        if "delta" in cell:
            continue
        points = [on_grid(p[:2]) for p in cell["points"]]
        found = []
        for p in symbol_paths(paths, cell["symbol"]):
            grid = [on_grid(q) for q in p["mm"]]
            if min(math.dist(q, c) for q in grid for c in points) < 0.6:
                found.append(["V%d" % points.index(q) if q in points else
                              "(%d, %d)" % (round((q[0] - points[0][0]) * 100), round((q[1] - points[0][1]) * 100))
                              for q in grid])
        yield cell, found


def main():
    cells = json.load(open(sys.argv[1]))
    paths, misfit = read_paths(sys.argv[2], cells["marks"])
    print("The registration marks fit to %.4f mm." % misfit)
    print("%-17s %-8s %8s %8s %8s" % ("cell", "line", "vertex", "piece", "exact"))
    limits = {}
    for cell, name, kept, piece, exact in measure_vertices(cells, paths):
        print("%-17s %-8s %8s %8s %8.2f" % (cell["id"], name, "-" if kept is None else ("kept" if kept else "left out"),
                                           "-" if piece is None else "%.2f" % piece, exact))
        if kept is not None:
            row = limits.setdefault((cell["row"], name), [None, None])
            if kept:
                row[1] = piece if row[1] is None else min(row[1], piece)
            else:
                row[0] = piece if row[0] is None else max(row[0], piece)
    print()
    print("%-22s %-8s %22s %22s" % ("row", "line", "left out: longest", "kept: shortest"))
    for (row, name), (left_out, kept) in limits.items():
        print("%-22s %-8s %22s %22s" % (row, name, "-" if left_out is None else "%.2f" % left_out,
                                         "-" if kept is None else "%.2f" % kept))
    for cell, found in scope_paths(cells, paths):
        print()
        print("%s (%s): %s" % (cell["id"], cell["symbol"], cell["note"]))
        for points in found:
            print("   " + " ".join(points))


if __name__ == "__main__":
    main()
