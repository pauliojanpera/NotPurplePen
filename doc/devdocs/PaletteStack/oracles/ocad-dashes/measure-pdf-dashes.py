"""Compare the dashes of OCAD's PDF export of a dash sample with the rules that the fixes 016 to 019 give Purple Pen.

Usage: measure-pdf-dashes.py CELLS.json EXPORT.pdf euclidean|ocad

For each cell and each dashed line of its symbol (the main line, the borders, or the fill of a double line), the
script computes the dashes by the rules: the count from the length of each part between corner and dash points, with
each Bezier curve as four chords and each part rounded to 0.01 mm; half way to an even number of dashes; a short part
gets a gap only when it is longer than half its end dashes and a gap; one gap with end dashes of 0 is no gap; a
secondary gap only in a dash longer than half a dash; OCAD's minimum gaps; no minimum for the dashes of a double line.
The fitted lengths come from the flattened length of each part, with the metric of the file. The end dashes of the
parts join at a corner or a dash point. A dash of a border is the dash of the middle moved aside by the distance of
the border.

It prints, for each line of each cell, the count of the rules and OCAD's count, and the largest distance between an
end of a dash of OCAD's and the nearest end of the same kind (start or end) by the rules. A symbol of glyphs gets its
count of glyphs only. The cells without dashes (solid lines) and the solid fill of a double line are left out.
measure-pdf-vertices.py reads the PDF."""
import importlib.util
import json
import math
import os
import sys

HERE = os.path.dirname(os.path.abspath(__file__))


def load(name):
    spec = importlib.util.spec_from_file_location(name.replace("-", "_"), os.path.join(HERE, name + ".py"))
    module = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(module)
    return module


dashes = load("measure-dashes")
pdf = load("measure-pdf-vertices")
OCAD_MINIMUM_GAPS = {0: 0, 1: 1, 2: 2, 3: 1, 4: 1}   # Purple Pen's minimum, as OCAD reads it back (ticket 019).


def round_half_even(x):
    return int(round(x))   # Python rounds half to even.


def counting_length(points, metric):
    """The length of a part as OCAD counts it: each Bezier curve as four chords, the part rounded to 0.01 mm."""
    total, i = 0.0, 0
    while i < len(points) - 1:
        if points[i + 1][2] == "B":
            ctrl = [points[i][:2], points[i + 1][:2], points[i + 2][:2], points[i + 3][:2]]
            chords = [dashes.bezier(ctrl, k / 4) for k in range(5)]
            total += sum(metric(chords[k], chords[k + 1]) for k in range(4))
            i += 3
        else:
            total += metric(points[i][:2], points[i + 1][:2])
            i += 1
    return round(total * 100) / 100


def split_points(points):
    """The points of each part between corner and dash points, with their kinds."""
    result, current = [], [points[0]]
    for i in range(1, len(points)):
        current.append(points[i])
        if i < len(points) - 1 and points[i][2] in "CD":
            result.append(current)
            current = [points[i]]
    result.append(current)
    return result


def part_dashes(L, Lc, f, l, line, minimum):
    """The dashes of one part as (start, end) along it, by the rules, and the count of glyph positions."""
    dash, gap = line["dash"], line["gap"]
    sec, endsec = line.get("sec", 0), line.get("endsec", 0)
    if Lc - f - l <= gap:
        n = 1 if (minimum >= 1 or Lc > f / 2 + l / 2 + gap) else 0
    else:
        n = round_half_even((Lc + dash - f - l) / (dash + gap) + 1) - 1
    n = max(n, minimum)
    if n == 1 and f == 0 and l == 0:
        n = 0
    if n == 0:
        return [(0.0, L)], n
    d = (L - gap * n) / (n - 1 + f / dash + l / dash)
    fd, ld = d * f / dash, d * l / dash
    spans, s = [], 0.0
    for k in range(n + 1):
        length = fd if k == 0 else ld if k == n else d
        if length > 0:
            pieces = [(s, s + length)]
            limit, secondary = (f, endsec) if k == 0 else (l, endsec) if k == n else (dash, sec)
            if secondary > 0 and length > limit / 2 and length > secondary:
                mid = s + length / 2
                pieces = [(s, mid - secondary / 2), (mid + secondary / 2, s + length)]
            spans.extend(pieces)
        s += length + gap
    return spans, n


def model(cell, line, metric, is_double):
    """The dashes of a line of a cell as (start, end) along the whole path, the flattened path, and the glyph count."""
    parts = split_points(cell["points"])
    flats, breaks = dashes.parts(cell["points"])
    lengths = [dashes.length(p, metric) for p in flats]
    longest = max(lengths)
    minimum = 0 if is_double else OCAD_MINIMUM_GAPS.get(line.get("min", 0), 0)
    spans, offset, glyphs = [], 0.0, 0
    for i, (pts, L) in enumerate(zip(parts, lengths)):
        f = line["dash"] / 2 if i > 0 and breaks[i - 1] == "D" else line["end"]
        l = line["dash"] / 2 if i < len(parts) - 1 and breaks[i] == "D" else line["end"]
        mg = minimum if L >= longest - 0.1 else 0
        part, n = part_dashes(L, counting_length(pts, metric), f, l, line, mg)
        glyphs += n
        part = [(offset + a, offset + b) for a, b in part]
        if spans and part and abs(spans[-1][1] - part[0][0]) < 1e-9:
            spans[-1] = (spans[-1][0], part[0][1])   # The end dashes join at the point.
            part = part[1:]
        spans.extend(part)
        offset += L
    flat = [flats[0][0]] + [p for f in flats for p in f[1:]]
    return spans, flat, glyphs


def locate(flat, metric, s):
    """The point at metric length s along a flattened path, and the unit normal to its left there."""
    for i in range(len(flat) - 1):
        seg = metric(flat[i], flat[i + 1])
        if s <= seg + 1e-9 or i == len(flat) - 2:
            t = 0 if seg == 0 else min(1.0, s / seg)
            a, b = flat[i], flat[i + 1]
            e = math.dist(a, b)
            return (a[0] + (b[0] - a[0]) * t, a[1] + (b[1] - a[1]) * t), (-(b[1] - a[1]) / e, (b[0] - a[0]) / e)
        s -= seg


def lines_of(sym):
    """The dashed lines of a symbol: (name, the line, its width in mm or None, the offsets of its paths)."""
    # The double lines of the samples are 0.6 mm wide with borders of 0.15 mm, unless the cells file says otherwise.
    # A symbol whose "border" is a dict (DM of the third sample) has a dashed main line of 0.1 mm beside its dashed
    # borders.
    beside = isinstance(sym.get("border"), dict)
    if sym.get("glyphs"):
        return [("glyphs", sym, None, [0.0])]
    if "double" not in sym and not beside:
        return [("main", sym, sym.get("linewidth"), [0.0])]
    if not beside and not sym["double"].endswith("dashed"):
        return []
    border_width = 0.15 if beside else sym.get("border", 0.15)
    h = sym.get("width", 0.6) / 2 + border_width / 2
    result = [("borders", sym["border"] if beside else sym, border_width, [h, -h])]
    if sym.get("double") == "all dashed":
        result.append(("fill", sym, sym.get("width", 0.6), [0.0]))
    elif "width" not in sym and not beside:
        # The double lines of the first three samples have a solid fill: its path is told apart and not measured.
        result.append(("fill", sym, 0.6, None))
    if beside:
        result.append(("main", sym, sym.get("linewidth", 0.1), [0.0]))
    return result


def main():
    cells = json.load(open(sys.argv[1]))
    metric = dashes.euclid if sys.argv[3] == "euclidean" else dashes.bizzarro
    paths, misfit = pdf.read_paths(sys.argv[2], cells["marks"])
    print("The registration marks fit to %.4f mm." % misfit)
    print("%-10s %-4s %-8s %5s %5s %9s  %s" % ("cell", "sym", "line", "rules", "OCAD", "largest", "note"))
    for cell in cells["cells"]:
        sym = cells["symbols"][cell["symbol"]]
        layer = pdf.symbol_paths(paths, cell["symbol"])
        xs = [p[0] for p in cell["points"]]
        ys = [p[1] for p in cell["points"]]
        box = (min(xs) - 1.5, min(ys) - 1.5, max(xs) + 1.5, max(ys) + 1.5)
        near = [p for p in layer if all(box[0] <= q[0] <= box[2] and box[1] <= q[1] <= box[3] for q in p["mm"])]
        groups = lines_of(sym)
        widths = [w for name, line, w, offsets in groups if w]
        for name, line, width, offsets in groups:
            if offsets is None:
                continue
            spans, flat, glyph_count = model(cell, line, metric, "double" in sym)
            if name == "glyphs":
                centres = {(round(sum(q[0] for q in p["mm"]) / len(p["mm"]), 1), round(sum(q[1] for q in p["mm"]) / len(p["mm"]), 1)) for p in near}
                print("%-10s %-4s %-8s %5d %5d %9s  %s" % (cell["id"], cell["symbol"], name, glyph_count, len(centres), "", cell["note"]))
                continue
            mine = [p for p in near if not widths or min(widths, key=lambda w: abs(w - p["width"] * pdf.PT)) == width]
            mine = [p for p in mine if pdf.path_length(p) > 0.05]
            expected = []
            for a, b in spans:
                for off in offsets:
                    for s, kind in ((a, "start"), (b, "end")):
                        point, normal = locate(flat, metric, s)
                        expected.append(((point[0] + normal[0] * off, point[1] + normal[1] * off), kind))
            worst = 0.0
            for p in mine:
                for q, kind in ((p["mm"][0], "start"), (p["mm"][-1], "end")):
                    options = [math.dist(q, e) for e, k in expected if k == kind] or [99.0]
                    other = [math.dist(q, e) for e, k in expected if k != kind] or [99.0]
                    worst = max(worst, min(min(options), min(other)))
            count = len(mine) // len(offsets)
            mark = "" if count == len(spans) and worst < 0.02 else "  <--"
            print("%-10s %-4s %-8s %5d %5d %9.4f  %s%s" % (cell["id"], cell["symbol"], name, len(spans), count, worst,
                                                       cell["note"], mark))


if __name__ == "__main__":
    main()
