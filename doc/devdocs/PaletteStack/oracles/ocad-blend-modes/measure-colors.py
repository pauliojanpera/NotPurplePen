"""Measure the colors of the cells of the blend-mode sample of ticket 027 in an image: a screenshot of OCAD's view, a
PNG export of OCAD, or a drawing of Purple Pen. Tell which way of compositing the palette colors gives the colors.

Usage: measure-colors.py CELLS.json IMAGE [--sample MM] [--tolerance LEVELS] [--cells]

The four registration marks, the round dark blobs at the corners of the drawing, give the scale and the position of the
image. The script reads the color of each region of each cell: the median of a square of MM on the paper (0.8 by
default, at least 3 pixels) at the point of the region.

The colors are compared only within the image, as the conversion of a color to RGB is not known. The paper is the color
of the regions without a palette color. The full color of a palette color is the color of the regions where a palette
color with its CMYK value lies alone over the paper with the opacity 100. From these, the script predicts the color of
each region by six ways of compositing the palette colors that lie there, from the bottom one up:
- the blend mode B: ignored (each palette color covers), or applied in RGB to the color below (Darken the smaller of each
  component, and Multiply the product, scaled by the paper);
- the opacity α: ignored, applied once to each palette color, or applied once to each shape, so that an overlap of two
  shapes of one palette color gets it twice.

The blend modes in CMYK, by ink, cannot be predicted without the conversion of the colors to RGB. A region fits a way
when no component of the prediction differs from the measured color by more than the tolerance (6 levels by default).
The script prints, for each way, the regions that fit it, for all the cells and by case, and with --cells the color and
the fitting ways of each region."""
import json
import sys
from itertools import product
from PIL import Image, ImageFilter, ImageStat

WAYS = [(b, a) for b, a in product(("ignored", "RGB"), ("ignored", "once per color", "once per shape"))]


def way_name(way):
    return "B %s, α %s" % way


def dark_components(image, threshold, erode):
    """The connected components of the pixels not lighter than the threshold, after an erosion: each as its bounding box
    (x0, y0, x1, y1) and its count of pixels."""
    mask = image.point(lambda v: 255 if v <= threshold else 0)
    if erode > 1:
        mask = mask.filter(ImageFilter.MinFilter(erode))
    width, height = mask.size
    data = mask.tobytes()
    parent = []

    def find(i):
        while parent[i] != i:
            parent[i] = parent[parent[i]]
            i = parent[i]
        return i

    runs_above, boxes = [], []
    for y in range(height):
        row = data[y * width:(y + 1) * width]
        runs, x = [], row.find(255)
        while x >= 0:
            end = row.find(0, x)
            end = width if end < 0 else end
            label = len(parent)
            parent.append(label)
            boxes.append([x, y, end - 1, y, end - x])
            for ax, aend, alabel in runs_above:
                if ax < end and aend > x:
                    a, b = find(alabel), find(label)
                    if a != b:
                        parent[b] = a
            runs.append((x, end, label))
            x = row.find(255, end)
        runs_above = runs
    merged = {}
    for label, box in enumerate(boxes):
        root = find(label)
        m = merged.setdefault(root, [box[0], box[1], box[2], box[3], 0])
        m[0], m[1], m[2], m[3] = min(m[0], box[0]), min(m[1], box[1]), max(m[2], box[2]), max(m[3], box[3])
        m[4] += box[4]
    return list(merged.values())


def find_marks(image, marks_mm):
    """The centers of the four registration marks in pixels, in the order of the cells file: top left, top right,
    bottom left and bottom right."""
    gray = image.convert("L")
    factor = max(1, max(gray.size) // 1600)
    small = gray.reduce(factor) if factor > 1 else gray
    darkest = next(level for level, count in enumerate(small.histogram()) if count)
    want = (marks_mm[1][0] - marks_mm[0][0]) / (marks_mm[0][1] - marks_mm[2][1])
    best = None
    # The marks are the darkest drawn color, but a view such as OCAD's draft mode draws them lighter.
    for threshold in sorted({min(darkest + 70, 128), 160, 200}):
        candidates = []
        for erode in (5, 3, 1):
            for x0, y0, x1, y1, count in dark_components(small, threshold, erode):
                w, h = x1 - x0 + 1, y1 - y0 + 1
                if w >= 3 and h >= 3 and 0.75 <= w / h <= 1.33 and 0.6 <= count / (w * h) <= 0.93:
                    candidates.append(((x0 + x1) / 2, (y0 + y1) / 2, (w + h) / 2))
            if len(candidates) >= 4:
                break
        for tl in candidates:
            for br in candidates:
                width, height = br[0] - tl[0], br[1] - tl[1]
                if width <= 10 * tl[2] or height <= 10 * tl[2] or abs(width / height - want) > 0.03 * want:
                    continue
                tolerance = max(2.0, 0.6 * tl[2])

                def near(x, y):
                    return [c for c in candidates if abs(c[0] - x) <= tolerance and abs(c[1] - y) <= tolerance
                            and 0.7 <= c[2] / tl[2] <= 1.4]
                tr, bl = near(br[0], tl[1]), near(tl[0], br[1])
                if tr and bl and 0.7 <= br[2] / tl[2] <= 1.4 and (best is None or width * height > best[0]):
                    best = (width * height, [tl, tr[0], bl[0], br])
        if best is not None:
            break
    if best is None:
        raise SystemExit("the four registration marks were not found")
    centers = []
    for cx, cy, size in best[1]:
        # The center of the dark pixels around the mark, at the full resolution.
        r = size * factor
        box = (int(cx * factor - r), int(cy * factor - r), int(cx * factor + r + factor), int(cy * factor + r + factor))
        crop = gray.crop(box)
        pixels = crop.load()
        sx = sy = n = 0
        for y in range(crop.size[1]):
            for x in range(crop.size[0]):
                if pixels[x, y] <= threshold:
                    sx, sy, n = sx + x, sy + y, n + 1
        centers.append((box[0] + sx / n + 0.5, box[1] + sy / n + 0.5))
    return centers


def fit(centers, marks_mm):
    """The scale and the offset of the pixels from the mm of the paper on each axis, and the largest residual in
    pixels."""
    fits = []
    for axis in (0, 1):
        pairs = [(m[axis], c[axis]) for m, c in zip(marks_mm, centers)]
        mean_m = sum(p[0] for p in pairs) / 4
        mean_c = sum(p[1] for p in pairs) / 4
        scale = sum((p[0] - mean_m) * (p[1] - mean_c) for p in pairs) / sum((p[0] - mean_m) ** 2 for p in pairs)
        offset = mean_c - scale * mean_m
        fits.append((scale, offset, max(abs(scale * p[0] + offset - p[1]) for p in pairs)))
    return fits


def sample(image, fits, point, size_mm):
    x = fits[0][0] * point[0] + fits[0][1]
    y = fits[1][0] * point[1] + fits[1][1]
    half = max(1, round(size_mm / 2 * abs(fits[0][0])))
    crop = image.crop((int(x) - half, int(y) - half, int(x) + half + 1, int(y) + half + 1))
    stat = ImageStat.Stat(crop)
    return tuple(stat.median), max(hi - lo for lo, hi in stat.extrema)


def median(values):
    values = sorted(values)
    n = len(values)
    return values[n // 2] if n % 2 else (values[n // 2 - 1] + values[n // 2]) / 2


def predict(stack, colors, full, paper, way):
    """The color of a region by a way of compositing its palette colors, or None if a full color is not known."""
    blend_way, alpha_way = way
    x, previous = list(paper), None
    for name in stack:
        color = colors[name]
        if alpha_way == "once per color" and name == previous:
            continue
        previous = name
        f = full.get(tuple(color["cmyk"]))
        if f is None:
            return None
        alpha = 1.0 if alpha_way == "ignored" else color["opacity"] / 100
        if blend_way == "RGB" and color["blend"] == "Darken":
            b = [min(xi, fi) for xi, fi in zip(x, f)]
        elif blend_way == "RGB" and color["blend"] == "Multiply":
            b = [xi * fi / max(pi, 1) for xi, fi, pi in zip(x, f, paper)]
        else:
            b = list(f)
        x = [(1 - alpha) * xi + alpha * bi for xi, bi in zip(x, b)]
    return x


def main():
    args = sys.argv[1:]
    options = {"--sample": 0.8, "--tolerance": 6.0}
    show_cells = "--cells" in args
    args = [a for a in args if a != "--cells"]
    for key in list(options):
        if key in args:
            i = args.index(key)
            options[key] = float(args[i + 1])
            del args[i:i + 2]
    if len(args) != 2:
        raise SystemExit(__doc__)
    cells = json.load(open(args[0]))
    image = Image.open(args[1]).convert("RGB")
    colors = {c["name"]: c for c in cells["colors"]}
    tolerance = options["--tolerance"]

    centers = find_marks(image, cells["marks"])
    fits = fit(centers, cells["marks"])
    print("%s: %d x %d pixels; the marks at %s; %.3f pixels to the mm across and %.3f down; the marks fit within %.2f "
          "pixels" % (args[1], image.size[0], image.size[1], ", ".join("(%.1f, %.1f)" % c for c in centers),
                      fits[0][0], -fits[1][0], max(f[2] for f in fits)))

    measured = []
    for cell in cells["cells"]:
        for region in cell["regions"]:
            color, spread = sample(image, fits, region["point"], options["--sample"])
            measured.append((cell, region, color, spread))
    paper = [median([m[2][i] for m in measured if not m[1]["stack"]]) for i in range(3)]
    full, sources = {}, {}
    for cell, region, color, spread in measured:
        stack = region["stack"]
        if len(stack) == 1 and colors[stack[0]]["opacity"] == 100:
            sources.setdefault(tuple(colors[stack[0]]["cmyk"]), []).append(color)
    for key, values in sources.items():
        full[key] = [median([v[i] for v in values]) for i in range(3)]
    print("Paper: %s" % " ".join("%.0f" % v for v in paper))
    print("Full colors, from the regions of a palette color alone with the opacity 100:")
    for key, values in sources.items():
        spread = max(max(v[i] for v in values) - min(v[i] for v in values) for i in range(3))
        print("  C%g M%g Y%g K%g: %s (%d regions, spread %.0f)" % (key + (" ".join("%.0f" % v for v in full[key]),
                                                                       len(values), spread)))

    fitting = {way: [] for way in WAYS}
    errors = {way: [] for way in WAYS}
    by_case = {}
    current = None
    for cell, region, color, spread in measured:
        stack = region["stack"]
        reference = not stack or (len(stack) == 1 and colors[stack[0]]["opacity"] == 100)
        results = []
        for way in WAYS:
            p = predict(stack, colors, full, paper, way)
            if p is None:
                continue
            error = max(abs(a - b) for a, b in zip(p, color))
            results.append((error, way))
            if not reference:
                errors[way].append(error)
                if error <= tolerance:
                    fitting[way].append(region)
                    by_case.setdefault((cell["case"], way), []).append(region)
        if not reference:
            by_case.setdefault((cell["case"], None), []).append(region)
        if show_cells:
            if cell is not current:
                print("%s" % cell["id"])
                current = cell
            fits_here = [way_name(w) for e, w in results if e <= tolerance]
            best = min(results)[0] if results else None
            print("    %-46s %3.0f %3.0f %3.0f%s | %s" % (
                region["name"], color[0], color[1], color[2], " (spread %.0f)" % spread if spread > 2 * tolerance else "",
                "reference" if reference else ("; ".join(fits_here) if fits_here else
                                               "no way fits (best error %.0f)" % best if best is not None else "-")))

    total = sum(1 for m in measured if m[1]["stack"] and not (len(m[1]["stack"]) == 1 and colors[m[1]["stack"][0]]["opacity"] == 100))
    print()
    print("The regions that each way fits within %g levels, of %d regions that are not references:" % (tolerance, total))
    for way in WAYS:
        e = errors[way]
        print("  %-36s %4d   largest error %3.0f, mean %5.1f" % (way_name(way), len(fitting[way]), max(e) if e else 0,
                                                                  sum(e) / len(e) if e else 0))
    print()
    print("By case: the regions that each way fits, of the regions of the case that are not references:")
    for case in sorted({c["case"] for c in cells["cells"]}):
        n = len(by_case.get((case, None), []))
        print("  case %d (%d regions): %s" % (case, n, "; ".join(
            "%s %d" % (way_name(w), len(by_case.get((case, w), []))) for w in WAYS)))


if __name__ == "__main__":
    main()
