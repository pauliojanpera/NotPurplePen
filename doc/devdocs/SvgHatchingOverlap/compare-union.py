"""Measure what browser-union.mjs rendered: each browser's drawing of a clip's union, of the ghosts' clip and of each
area drawn on its own, against its drawing of the same areas merged into one path.

Usage: python3 -I compare-union.py <output folder of browser-union.mjs>

For each layout and paint, and each browser, prints the pixels inside the areas' union that differ from the merged
drawing by more than 4 levels in a channel, the largest difference, and how many of those pixels are lighter and how
many darker: a seam where areas only touch draws lighter, a pattern drawn twice where areas overlap draws darker.
The pixels within 2.5 pixels of the union's outline are left out: there the browsers smooth a clip of one path and a
clip of several otherwise, by up to a third of a pixel's coverage, whatever the paths are; what is asked here is
what happens inside, along the edges the areas share and where they overlap.
"""
import os
import re
import sys

from PIL import Image, ImageChops, ImageDraw

BROWSERS = ["chromium", "firefox", "webkit"]
LAYOUTS = ["touching", "overlapping", "strip42", "strip43", "grid"]
PAINTS = ["fill", "hatch"]
WAYS = ["union", "ghosts", "each"]


def inside_mask(svg_path, size):
    """The pixels of the merged union's interior, at least 2.5 pixels from its outline, as an image: 255 inside."""
    text = open(svg_path, encoding="utf-8").read()
    data = re.search(r'<clipPath id="c"><path d="([^"]+)"', text).group(1)
    tx, ty, scale = (float(v) for v in re.search(r'translate\(([\d.]+) ([\d.]+)\) scale\(([\d.]+)\)', text).groups())
    mask = Image.new("L", size, 0)
    draw = ImageDraw.Draw(mask)
    for polygon in re.findall(r"M([^Z]+)Z", data):
        points = [tuple(float(v) for v in pair.split()) for pair in polygon.split("L")]
        points = [(tx + scale * x, ty + scale * y) for x, y in points]
        draw.polygon(points, fill=255)
        draw.line(points + [points[0]], fill=0, width=5)
    return mask


def measure(reference, image, mask):
    """Pixels inside the mask differing by more than 4 levels, the largest difference, and the lighter and darker
    among them."""
    a = reference.convert("RGB")
    b = image.convert("RGB")
    diff = ImageChops.difference(a, b)
    count = lighter = darker = 0
    largest = 0
    pa, pb, pd, pm = a.load(), b.load(), diff.load(), mask.load()
    for y in range(a.height):
        for x in range(a.width):
            if not pm[x, y]:
                continue
            d = max(pd[x, y])
            if d > 4:
                count += 1
                if sum(pb[x, y]) > sum(pa[x, y]):
                    lighter += 1
                else:
                    darker += 1
            largest = max(largest, d)
    return count, largest, lighter, darker


def main():
    folder = sys.argv[1]
    print("| Layout | Paint | Way | " + " | ".join(b.capitalize() for b in BROWSERS) + " |")
    print("| --- | --- | --- | " + " | ".join("---" for _ in BROWSERS) + " |")
    for layout in LAYOUTS:
        for paint in PAINTS:
            for way in WAYS:
                cells = []
                for browser in BROWSERS:
                    merged = Image.open(os.path.join(folder, browser, "%s-%s-merged.png" % (layout, paint)))
                    image = Image.open(os.path.join(folder, browser, "%s-%s-%s.png" % (layout, paint, way)))
                    mask = inside_mask(os.path.join(folder, "svg", "%s-%s-merged.svg" % (layout, paint)), merged.size)
                    count, largest, lighter, darker = measure(merged, image, mask)
                    cells.append("0" if count == 0 else "%d (%d lighter, %d darker; up to %d)" % (count, lighter, darker, largest))
                print("| %s | %s | %s | %s |" % (layout, paint, way, " | ".join(cells)))


if __name__ == "__main__":
    main()
