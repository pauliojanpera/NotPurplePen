"""Measure the dashes of each cell of a dash sample in a drawing at 600 dpi (an export of OCAD or Purple Pen's), and
compare them with Purple Pen's rule for the counts of dashes.

Usage: measure-dashes.py CELLS.json DRAWING.png euclidean|ocad [OTHER-DRAWING.png]

The registration marks give the scale and the position of the drawing. A dash is a run of pixels darker than 128 along
the path of the cell (the middle of the line, or the left border of a double line whose fill is not dashed). The rule
is the one of LineSymDef.ComputeDashDistances, with the metric given; "strict short" gives a short line a gap only when
it is longer than half the end dashes and a gap."""
import json
import math
import sys
from PIL import Image

PP_DOUBLE_MIN_GAPS = 1   # OcadImport gives the dashes of double lines at least one gap.


def bezier(p, t):
    u = 1 - t
    return (u**3 * p[0][0] + 3 * u * u * t * p[1][0] + 3 * u * t * t * p[2][0] + t**3 * p[3][0],
            u**3 * p[0][1] + 3 * u * u * t * p[1][1] + 3 * u * t * t * p[2][1] + t**3 * p[3][1])


def euclid(a, b): return math.hypot(b[0] - a[0], b[1] - a[1])


def bizzarro(a, b):
    dx, dy = abs(b[0] - a[0]), abs(b[1] - a[1])
    return max(dx, dy) + min(dx, dy) / 2


def parts(points):
    """The parts of a path between corner and dash points, each a dense list of points, and the kinds of the breaks."""
    result, breaks, current = [], [], [tuple(points[0][:2])]
    i = 0
    while i < len(points) - 1:
        if points[i + 1][2] == "B":
            ctrl = [points[i][:2], points[i + 1][:2], points[i + 2][:2], points[i + 3][:2]]
            current += [bezier(ctrl, k / 2000) for k in range(1, 2001)]
            i += 3
        else:
            current.append(tuple(points[i + 1][:2]))
            i += 1
        if i < len(points) - 1 and points[i][2] in "CD":
            result.append(current); breaks.append(points[i][2]); current = [tuple(points[i][:2])]
    result.append(current)
    return result, breaks


def length(pts, metric): return sum(metric(pts[i], pts[i + 1]) for i in range(len(pts) - 1))


def model_runs(cell, sym, metric, strict_short=False, double_min_gaps=PP_DOUBLE_MIN_GAPS):
    dash, end, gap = sym["dash"], sym["end"], sym["gap"]
    min_gaps = sym.get("min", double_min_gaps if "double" in sym else 0)
    sec, endsec = sym.get("sec", 0), sym.get("endsec", 0)
    ps, breaks = parts(cell["points"])
    lengths = [length(p, metric) for p in ps]
    longest = max(lengths)
    closed = euclid(cell["points"][0], cell["points"][-1]) < 1e-6
    total = 0
    for i, L in enumerate(lengths):
        f = dash / 2 if i > 0 and breaks[i - 1] == "D" else end
        l = dash / 2 if i < len(lengths) - 1 and breaks[i] == "D" else end
        mg = min_gaps if L >= longest - 0.1 else 0
        if dash + gap <= 0:
            n = 0
        elif L - f - l <= gap:
            ok = L > f / 2 + l / 2 + gap if strict_short else L >= f / 2 + l / 2 + gap
            n = 1 if (mg >= 1 or ok) else 0
        else:
            n = int(round((L + dash - f - l) / (dash + gap)))
        n = max(n, mg)
        if n == 0:
            runs = 1
        else:
            if n == 1 and f == 0 and l == 0:
                d = fd = ld = 0
            else:
                d = (L - gap * n) / (n - 1 + f / dash + l / dash)
                fd, ld = d * f / dash, d * l / dash
            runs = (n - 1) * (d > 0) + (fd > 0) + (ld > 0)
            if sec > 0 and d >= dash / 2 and d > sec:
                runs += n - 1
            if endsec > 0:
                runs += (fd >= f / 2 and fd > endsec) + (ld >= l / 2 and ld > endsec)
        total += runs - (1 if i > 0 else 0)
    return total, lengths


def find_marks(image):
    """The centers of the four registration marks, in pixels: top left, top right, bottom left."""
    px = image.load()
    w, h = image.size
    dark = [(x, y) for y in range(0, h, 1) for x in list(range(0, min(w, 400))) + list(range(max(0, w - 400), w)) if px[x, y] < 128]
    xs = [p[0] for p in dark]; ys = [p[1] for p in dark]
    x0, x1, y0, y1 = min(xs), max(xs), min(ys), max(ys)

    def centroid(cx, cy):
        pts = [(x, y) for x, y in dark if abs(x - cx) < 40 and abs(y - cy) < 40]
        return (sum(p[0] for p in pts) / len(pts) + 0.5, sum(p[1] for p in pts) / len(pts) + 0.5)
    return centroid(x0, y0), centroid(x1, y0), centroid(x0, y1)


class Grid:
    def __init__(self, image, marks_mm):
        tl, tr, bl = find_marks(image)
        (mx0, my0), (mx1, _), (_, my1) = marks_mm[0], marks_mm[1], marks_mm[2]
        self.sx = (tr[0] - tl[0]) / (mx1 - mx0)
        self.sy = (bl[1] - tl[1]) / (my0 - my1)
        self.tl, self.m0 = tl, (mx0, my0)
        self.px = image.load()

    def covered(self, x, y):
        return self.px[int(self.tl[0] + (x - self.m0[0]) * self.sx), int(self.tl[1] + (self.m0[1] - y) * self.sy)] < 128


def measured_runs(grid, cell, offset=0.0):
    ps, _ = parts(cell["points"])
    pts = [p for part in ps for p in part]
    dense = []
    for i in range(len(pts) - 1):
        a, b = pts[i], pts[i + 1]
        seg = euclid(a, b)
        if seg == 0:
            continue
        nx, ny = -(b[1] - a[1]) / seg, (b[0] - a[0]) / seg
        n = max(1, int(seg / 0.005))
        for k in range(n):
            t = k / n
            dense.append((a[0] + (b[0] - a[0]) * t + nx * offset, a[1] + (b[1] - a[1]) * t + ny * offset))
    dense.append((pts[-1][0], pts[-1][1]))
    runs, start, s = [], None, 0.0
    for i, (x, y) in enumerate(dense):
        if i > 0:
            s += euclid(dense[i - 1], dense[i])
        c = grid.covered(x, y)
        if c and start is None:
            start = s
        elif not c and start is not None:
            runs.append((start, s)); start = None
    if start is not None:
        runs.append((start, s))
    return runs


def main():
    data = json.load(open(sys.argv[1]))
    metric = euclid if sys.argv[3] == "euclidean" else bizzarro
    grids = [Grid(Image.open(p).convert("L"), data["marks"]) for p in sys.argv[2:3] + sys.argv[4:5]]
    print("scale %s px/mm" % ", ".join("%.3f x %.3f" % (g.sx, g.sy) for g in grids))
    for cell in data["cells"]:
        sym = data["symbols"][cell["symbol"]]
        if sym.get("double", "").startswith("solid"):
            continue
        offset = 0.375 if sym.get("double") == "borders dashed" else 0.0
        counts = [len(measured_runs(g, cell, offset)) for g in grids]
        model, lengths = model_runs(cell, sym, metric)
        strict, _ = model_runs(cell, sym, metric, strict_short=True)
        no_minimum, _ = model_runs(cell, sym, metric, double_min_gaps=0)
        mark = "" if all(c == model for c in counts) else "  <-- differs from the rule"
        print("%-9s %-3s %-30s L %7.3f | dashes %-5s | rule %2d, strict short %2d, double lines without a minimum %2d%s" % (
            cell["id"], cell["symbol"], cell["note"], sum(lengths), "/".join(str(c) for c in counts), model, strict, no_minimum, mark))


if __name__ == "__main__":
    main()
