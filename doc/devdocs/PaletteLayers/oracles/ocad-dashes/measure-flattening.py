# Measure how far Purple Pen's flattened Bezier curves are from the true curves of the dash samples.
#
# Purple Pen flattens a curve with Bezier.Flatten(0.01): it divides the curve at the middle of its parameter until the
# sum of the absolute second differences of its control points is at most 2.5 times 0.01. The drawing of the dashes,
# the glyphs along a line and the borders of double lines measure and offset this polyline.
#
# The script prints, for the Bezier curves of the given cell files:
# - the number of points of each flattened curve,
# - the largest distance of a flattened curve from the true curve,
# - the largest amount by which the true length of a quarter of a curve (parameters k/4 to (k+1)/4) is longer than
#   its flattened length,
# - for each dashed path of four or more curves, the amount by which the true length of its curves is longer than the
#   flattened length, and the number of points of the path flattened and as Bezier curves.
#
# Usage: python3 measure-flattening.py dashes-8-cells.json dashes-9-cells.json dashes-10-cells.json

import json
import math
import struct
import sys


def f32(x):
    return struct.unpack('f', struct.pack('f', x))[0]


def flatten(p, error=0.01):
    """The points of a curve as Bezier.Flatten gives them, each with its parameter."""
    out = [(p[0][0], p[0][1], 0.0)]
    tolerance = f32(error * 2.5)

    def divide(x1, y1, x2, y2, x3, y3, x4, y4, level, t0, t1):
        d = abs(x1 + x3 - x2 - x2) + abs(y1 + y3 - y2 - y2) + abs(x2 + x4 - x3 - x3) + abs(y2 + y4 - y3 - y3)
        if d <= tolerance or level > 20:
            return
        x12, y12 = (x1 + x2) / 2, (y1 + y2) / 2
        x23, y23 = (x2 + x3) / 2, (y2 + y3) / 2
        x34, y34 = (x3 + x4) / 2, (y3 + y4) / 2
        x123, y123 = (x12 + x23) / 2, (y12 + y23) / 2
        x234, y234 = (x23 + x34) / 2, (y23 + y34) / 2
        x1234, y1234 = (x123 + x234) / 2, (y123 + y234) / 2
        tm = (t0 + t1) / 2
        divide(x1, y1, x12, y12, x123, y123, x1234, y1234, level + 1, t0, tm)
        out.append((f32(x1234), f32(y1234), tm))
        divide(x1234, y1234, x234, y234, x34, y34, x4, y4, level + 1, tm, t1)

    a, b, c, d = [(f32(x), f32(y)) for x, y in p]
    divide(a[0], a[1], b[0], b[1], c[0], c[1], d[0], d[1], 1, 0.0, 1.0)
    out.append((p[3][0], p[3][1], 1.0))
    return out


def bezier(p, t):
    s = 1 - t
    return (s ** 3 * p[0][0] + 3 * s * s * t * p[1][0] + 3 * s * t * t * p[2][0] + t ** 3 * p[3][0],
            s ** 3 * p[0][1] + 3 * s * s * t * p[1][1] + 3 * s * t * t * p[2][1] + t ** 3 * p[3][1])


def arc_length(p, t0, t1, steps=4000):
    q = [bezier(p, t0 + (t1 - t0) * k / steps) for k in range(steps + 1)]
    return sum(math.dist(q[k], q[k + 1]) for k in range(steps))


def distance_to_segment(q, a, b):
    dx, dy = b[0] - a[0], b[1] - a[1]
    l2 = dx * dx + dy * dy
    u = 0 if l2 == 0 else max(0.0, min(1.0, ((q[0] - a[0]) * dx + (q[1] - a[1]) * dy) / l2))
    return math.dist(q, (a[0] + u * dx, a[1] + u * dy))


def curves_of(cell):
    """The Bezier curves of a cell, and the count of its straight segments."""
    pts, curves, straight, i = cell['points'], [], 0, 0
    while i + 1 < len(pts):
        if i + 3 < len(pts) and pts[i + 1][2] == 'B' and pts[i + 2][2] == 'B':
            curves.append([tuple(q[:2]) for q in pts[i:i + 4]])
            i += 3
        else:
            straight += 1
            i += 1
    return curves, straight


def main(paths):
    seen, counts = set(), []
    worst_distance, worst_quarter = (0.0, ''), (0.0, '', 0, 0.0)
    for path in paths:
        sample = path.split('/')[-1].split('-cells')[0]
        for cell in json.load(open(path))['cells']:
            for p in curves_of(cell)[0]:
                key = tuple(round(c, 3) for xy in p for c in (xy[0] - p[0][0], xy[1] - p[0][1]))
                if key in seen:
                    continue
                seen.add(key)
                flat = flatten(p)
                counts.append(len(flat))
                poly = [(x, y) for x, y, t in flat]
                distance = 0.0
                for k in range(2001):
                    t = k / 2000
                    j = max(i for i in range(len(flat) - 1) if flat[i][2] <= t)
                    nearby = range(max(0, j - 1), min(len(poly) - 1, j + 2))
                    distance = max(distance, min(distance_to_segment(bezier(p, t), poly[i], poly[i + 1]) for i in nearby))
                if distance > worst_distance[0]:
                    worst_distance = (distance, sample + ':' + cell['id'])
                for quarter in range(4):
                    t0, t1 = quarter / 4, (quarter + 1) / 4
                    indexes = [i for i, (x, y, t) in enumerate(flat) if t0 <= t <= t1]
                    if len(indexes) < 2 or flat[indexes[0]][2] != t0 or flat[indexes[-1]][2] != t1:
                        continue
                    flattened = sum(math.dist(poly[i], poly[i + 1]) for i in range(indexes[0], indexes[-1]))
                    true = arc_length(p, t0, t1)
                    if true - flattened > worst_quarter[0]:
                        worst_quarter = (true - flattened, sample + ':' + cell['id'], quarter, true)

    counts.sort()
    print('%d curves; points of a flattened curve: %d to %d, median %d' % (len(seen), counts[0], counts[-1], counts[len(counts) // 2]))
    print('largest distance of a flattened curve from the true curve: %.4f mm (%s)' % worst_distance)
    print('largest amount by which a quarter is longer than flattened: %.4f mm (%s, quarter %d of %.2f mm)' % worst_quarter)

    print('dashed paths of four or more curves:')
    for path in paths:
        sample = path.split('/')[-1].split('-cells')[0]
        for cell in json.load(open(path))['cells']:
            curves, straight = curves_of(cell)
            if len(curves) < 4 or not cell['symbol'].startswith('D'):
                continue
            true = sum(arc_length(p, 0, 1) for p in curves)
            flattened = sum(sum(math.dist(f[k][:2], f[k + 1][:2]) for k in range(len(f) - 1)) for f in map(flatten, curves))
            flat_points = 1 + straight + sum(len(flatten(p)) - 1 for p in curves)
            bezier_points = 1 + straight + 3 * len(curves)
            print('  %s:%s, %d curves, %.1f mm of curves: %.4f mm longer than flattened; %d points flattened, %d as Bezier curves'
                  % (sample, cell['id'], len(curves), true, true - flattened, flat_points, bezier_points))


if __name__ == '__main__':
    main(sys.argv[1:])
