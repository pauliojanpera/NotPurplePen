"""Read OCAD's PDF export of the tenth dash sample, and compare the dashes and the glyphs of its paths with two models.

Usage: measure-pdf-paths.py CELLS.json EXPORT.pdf euclid|ocad [plain-dash-points]

A path has straight segments and Bezier curves, and its corner points and dash points divide it into parts. Each
model fits the dashes, or the spaced glyphs, of each part with OCAD's rules of the count (fix 017), in a measure of the
part, and gives the positions along the euclidean length of the path:

- "quarters": a straight segment has its length in the metric of the map, and each quarter of a Bezier curve (t from
  0 to 1/4, and so on) the length of its chord in that metric. In a straight segment and in a quarter, the positions
  are in proportion to the euclidean length. This is the model of fix 021, with the metric of the map.
- "even": even along the length of the path in the metric of the map, as Purple Pen places the dashes without fix 021.

At a dash point, the end dash of each part is half a dash, so that one dash is centered on the point. The end dashes
of the parts join at the corner points and the dash points. The count of the dashes uses the measure of the quarters,
with each straight segment rounded to 0.01 mm. A spaced glyph is at the end of each "dash" whose length is the
distance of the glyphs, with end "dashes" of the first and the last distance and no gaps.

For each cell, the script finds OCAD's ends of the dashes, or the centers of the glyphs, along the path, and prints
OCAD's count, the count of the rules, and for each model the largest distance along the path from one of OCAD's ends
or centers to the nearest one of the model. "plain-dash-points" reads the dash points as plain points, as in an OCAD 6
file, into which Purple Pen writes no dash points. The reader of the PDF is the one of measure-pdf-vertices.py."""
import importlib.util
import json
import math
import os
import sys
import numpy as np

HERE = os.path.dirname(os.path.abspath(__file__))
spec = importlib.util.spec_from_file_location("mpv", os.path.join(HERE, "measure-pdf-vertices.py"))
mpv = importlib.util.module_from_spec(spec)
spec.loader.exec_module(mpv)

STEPS = 200


def ocad_distance(a, b):
    dx, dy = abs(b[0] - a[0]), abs(b[1] - a[1])
    return max(dx, dy) + min(dx, dy) / 2


def euclid_distance(a, b):
    return math.hypot(b[0] - a[0], b[1] - a[1])


def bezier(p, t):
    u = 1 - t
    return (u ** 3 * p[0][0] + 3 * u * u * t * p[1][0] + 3 * u * t * t * p[2][0] + t ** 3 * p[3][0],
            u ** 3 * p[0][1] + 3 * u * u * t * p[1][1] + 3 * u * t * t * p[2][1] + t ** 3 * p[3][1])


def pieces_of(points):
    """The pieces of a path, each (kind, control points, part), the kinds of the points between its parts, and whether
    it is closed."""
    pieces, part, i, joins = [], 0, 0, []
    while i < len(points) - 1:
        if points[i + 1][2] == "B":
            pieces.append(("bezier", [p[:2] for p in points[i:i + 4]], part))
            i += 3
        else:
            pieces.append(("line", [p[:2] for p in points[i:i + 2]], part))
            i += 1
        if i < len(points) - 1 and points[i][2] in "CD":
            part += 1
            joins.append(points[i][2])
    closed = points[0][:2] == points[-1][:2]
    return pieces, joins, closed


def fine_points(kind, ctrl, t0=0.0, t1=1.0, steps=STEPS):
    if kind == "line":
        a, b = ctrl
        return [(a[0] + (b[0] - a[0]) * t0, a[1] + (b[1] - a[1]) * t0), (a[0] + (b[0] - a[0]) * t1, a[1] + (b[1] - a[1]) * t1)]
    return [bezier(ctrl, t0 + (t1 - t0) * k / steps) for k in range(steps + 1)]


def length(pts, metric=euclid_distance):
    return sum(metric(pts[k], pts[k + 1]) for k in range(len(pts) - 1))


class Path:
    def __init__(self, points):
        self.pieces, self.joins, self.closed = pieces_of(points)
        poly, self.piece_arc = [], []
        for kind, ctrl, part in self.pieces:
            pts = fine_points(kind, ctrl)
            start = length(poly) if poly else 0.0
            if poly:
                pts = pts[1:]
            poly.extend(pts)
            self.piece_arc.append(start)
        self.poly = np.array(poly)
        seg = self.poly[1:] - self.poly[:-1]
        self.seglen = np.hypot(seg[:, 0], seg[:, 1])
        self.cum = np.concatenate([[0], np.cumsum(self.seglen)])
        self.total = self.cum[-1]
        self.parts = max(p[2] for p in self.pieces) + 1

    def project(self, q):
        a, b = self.poly[:-1], self.poly[1:]
        ab = b - a
        t = np.clip(((q[0] - a[:, 0]) * ab[:, 0] + (q[1] - a[:, 1]) * ab[:, 1]) / np.maximum(self.seglen ** 2, 1e-18), 0, 1)
        px, py = a[:, 0] + t * ab[:, 0], a[:, 1] + t * ab[:, 1]
        d = np.hypot(px - q[0], py - q[1])
        i = int(np.argmin(d))
        return self.cum[i] + t[i] * self.seglen[i], float(d[i])

    def subpieces(self, model, metric):
        """For each part: a list of (measure, arc start, arc length), and the measure for counting."""
        out = [[] for _ in range(self.parts)]
        counting = [0.0] * self.parts
        for (kind, ctrl, part), arc0 in zip(self.pieces, self.piece_arc):
            if kind == "line":
                m = metric(ctrl[0], ctrl[1])
                a = euclid_distance(ctrl[0], ctrl[1])
                out[part].append((m, arc0, a))
                counting[part] += round(m * 100) / 100
                continue
            quarters = [fine_points(kind, ctrl, k / 4, (k + 1) / 4, STEPS // 4) for k in range(4)]
            counting[part] += sum(metric(q[0], q[-1]) for q in quarters)
            arc = arc0
            for q in quarters:
                a = length(q)
                if model == "quarters":
                    out[part].append((metric(q[0], q[-1]), arc, a))
                else:
                    sub = arc
                    for k in range(len(q) - 1):
                        e = euclid_distance(q[k], q[k + 1])
                        out[part].append((metric(q[k], q[k + 1]), sub, e))
                        sub += e
                arc += a
        return out, counting

    def model_positions(self, model, metric, sym):
        """The arc positions of the ends of the dashes, or of the glyphs, by a model, and the count of the dashes or
        the glyphs that OCAD's rules give."""
        parts, counting = self.subpieces(model, metric)
        positions, count = [], 0
        for index, (subs, mc) in enumerate(zip(parts, counting)):
            total = sum(s[0] for s in subs)

            def arc_at(s):
                for m, arc0, a in subs:
                    if s <= m or (m, arc0, a) == subs[-1]:
                        return arc0 + (min(s / m, 1.0) if m > 0 else 0) * a
                    s -= m

            if "glyph" in sym:
                d, e1, e2 = sym["distance"], sym["first"], sym["last"]
                dashes = round_even((mc + d - e1 - e2) / d + 1)
                glyphs = max(dashes - 1, 1)
                step = total / (glyphs - 1 + e1 / d + e2 / d)
                for k in range(glyphs):
                    positions.append(arc_at(step * e1 / d + k * step))
                count += glyphs
                continue
            d, e, g = sym["dash"], sym["end"], sym["gap"]
            # At a dash point, the end dash of each part is half a dash, so that one dash is centered on the point.
            e1 = d / 2 if index > 0 and self.joins[index - 1] == "D" else e
            e2 = d / 2 if index < len(self.joins) and self.joins[index] == "D" else e
            if mc - e1 - e2 <= g:
                gaps = 1 if mc > e1 / 2 + e2 / 2 + g else 0
            else:
                gaps = round_even((mc + d - e1 - e2) / (d + g) + 1) - 1
            count += gaps + 1
            if gaps == 0:
                continue
            dash = (total - gaps * g) / (gaps - 1 + e1 / d + e2 / d)
            s = dash * e1 / d
            for k in range(gaps):
                positions.append(arc_at(s))
                positions.append(arc_at(s + g))
                s += g + dash
        if "glyph" not in sym:
            count -= self.parts - 1
            # The ends of the path. On a closed path, OCAD also writes a piece of the dash over the start that starts
            # there.
            positions += [0.0, self.total]
        return positions, count


def round_even(x):
    n = math.floor(x)
    f = x - n
    if abs(f - 0.5) < 1e-9:
        return n if n % 2 == 0 else n + 1
    return n + 1 if f > 0.5 else n


def observed(cell, paths, path, sym):
    xs = [p[0] for p in cell["points"]]
    ys = [p[1] for p in cell["points"]]
    lo, hi = (min(xs) - 1, min(ys) - 1), (max(xs) + 1, max(ys) + 1)
    out, count = [], 0
    for p in mpv.symbol_paths(paths, cell["symbol"]):
        if not all(lo[0] <= q[0] <= hi[0] and lo[1] <= q[1] <= hi[1] for q in p["mm"]):
            continue
        if "glyph" in sym:
            if p["op"] not in ("f", "F", "f*", "b", "B"):
                continue
            qx, qy = [q[0] for q in p["mm"]], [q[1] for q in p["mm"]]
            centre = ((min(qx) + max(qx)) / 2, (min(qy) + max(qy)) / 2)
            arc, dist = path.project(centre)
            if dist < 0.1:
                out.append(arc)
                count += 1
        else:
            ends = [path.project(p["mm"][0]), path.project(p["mm"][-1])]
            if all(dist < 0.1 for arc, dist in ends):
                out += [arc for arc, dist in ends]
                count += 1
    return out, count


def main():
    cells = json.load(open(sys.argv[1]))
    paths, misfit = mpv.read_paths(sys.argv[2], cells["marks"])
    metric = ocad_distance if sys.argv[3] == "ocad" else euclid_distance
    plain_dash_points = sys.argv[4:5] == ["plain-dash-points"]
    print("The registration marks fit to %.4f mm. Metric: %s." % (misfit, sys.argv[3]))
    print("%-24s %6s %6s %9s %9s" % ("cell", "OCAD", "rules", "quarters", "even"))
    worst = {}
    for cell in cells["cells"]:
        sym = cells["symbols"][cell["symbol"]]
        points = [(x, y, "N" if plain_dash_points and k == "D" else k) for x, y, k in cell["points"]]
        path = Path(points)
        obs, count = observed(cell, paths, path, sym)
        row = []
        for model in ("quarters", "even"):
            pos, rule_count = path.model_positions(model, metric, sym)
            err = max(min(abs(o - p) for p in pos) for o in obs) if obs and pos else float("nan")
            worst[(cell["symbol"], model)] = max(worst.get((cell["symbol"], model), 0), err)
            row.append(err)
        print("%-24s %6d %6d %9.3f %9.3f" % (cell["id"], count, rule_count, row[0], row[1]))
    for key in sorted(worst):
        print("largest", key, "%.3f" % worst[key])


if __name__ == "__main__":
    main()
