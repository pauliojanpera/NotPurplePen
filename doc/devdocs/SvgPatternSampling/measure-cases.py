"""Measure what the browsers made of each way of carrying a screen, against the same drawing resolved tenfold.

Usage: python3 -I measure-cases.py <out folder of the renderings> [--font <ttf>]

The folder holds, for each pitch and way that cases.py wrote, <pitch>/<way>/<browser>.png at the rendering's own size
and <pitch>/<way>-x10/chromium.png at ten times it, and may hold <pitch>/pattern-x10/<browser>.png for the other two
engines. A pixel of the resolved drawing, box-averaged ten by ten, is what a pixel of the rendering should hold; the
way's own resolved drawing is its truth, and the truths of the ways agree within half a level.

Prints, and writes as measurements.tsv, for each way and engine: the mean of the blue channel, which the white of the
screen raises and its yellow ink lowers; the spread of that channel, which is the grain of the screen; the share of
pixels that stayed paper white and the share that stayed full ink; and how far the mean lies from the truth. Writes
the renderings beside each other as cases.png, each magnified tenfold by whole pixels.
"""
import os
import sys

import numpy as np
from PIL import Image, ImageDraw, ImageFont

BROWSERS = ["chromium", "firefox", "webkit"]
MARGIN = 20      # left out at each edge of the rendering, where the page ends
DETAIL = 24      # the side, in pixels of the rendering, of the detail the sheet shows


def read(path):
    """One rendering, as rows of red, green and blue."""
    return np.asarray(Image.open(path).convert("RGB")).astype(np.float64)


def middle(image):
    """The pixels of a rendering away from its edges, as a list of colours."""
    return image[MARGIN:-MARGIN, MARGIN:-MARGIN].reshape(-1, 3)


def resolved(path, size):
    """The truth: the drawing rendered ten times larger, each pixel of the rendering the mean of its hundred."""
    image = read(path)
    return image.reshape(size, 10, size, 10, 3).mean(axis=(1, 3))


def shares(colours):
    """What share of the colours stayed paper white, and what share full ink."""
    white = ((colours[:, 0] >= 250) & (colours[:, 1] >= 250) & (colours[:, 2] >= 250)).mean() * 100
    ink = (colours[:, 2] <= 40).mean() * 100
    return white, ink


def ways_of(folder):
    """The ways a pitch's folder holds, in the order cases.py writes them."""
    order = ["pattern", "pattern-crisp", "pattern-ten", "pattern-scaled", "pattern-image", "circles", "uses"]
    held = {name for name in os.listdir(folder) if os.path.isdir(os.path.join(folder, name))}
    return [way for way in order if way in held]


def sheet(folder, pitch, ways, font_path):
    """Write the renderings beside each other: a row for each way, the truth and then each engine."""
    try:
        font = ImageFont.truetype(font_path, 17) if font_path else ImageFont.load_default()
        small = ImageFont.truetype(font_path, 13) if font_path else ImageFont.load_default()
    except OSError:
        font = small = ImageFont.load_default()
    tile, gap, left, top, label = DETAIL * 10, 12, 230, 28, 18
    columns = ["resolved"] + BROWSERS
    image = Image.new("RGB", (left + len(columns) * (tile + gap) - gap + 16,
                              top + len(ways) * (tile + gap + label) - gap + 8), "white")
    draw = ImageDraw.Draw(image)
    for column, name in enumerate(columns):
        draw.text((left + column * (tile + gap), 6), name, "black", font=font)
    for row, way in enumerate(ways):
        y = top + row * (tile + gap + label)
        draw.text((8, y + tile // 2 - 8), way, "black", font=small)
        for column, name in enumerate(columns):
            if column == 0:
                part = read(os.path.join(folder, way + "-x10", "chromium.png"))
                detail = Image.fromarray(part[MARGIN * 10:(MARGIN + DETAIL) * 10,
                                              MARGIN * 10:(MARGIN + DETAIL) * 10].astype(np.uint8))
            else:
                part = read(os.path.join(folder, way, name + ".png"))
                detail = Image.fromarray(part[MARGIN:MARGIN + DETAIL, MARGIN:MARGIN + DETAIL].astype(np.uint8))
                detail = detail.resize((tile, tile), Image.NEAREST)
            image.paste(detail, (left + column * (tile + gap), y))
            draw.rectangle([left + column * (tile + gap), y, left + column * (tile + gap) + tile - 1, y + tile - 1],
                           outline=(120, 120, 120))
    path = os.path.join(folder, "cases.png")
    image.save(path)
    return path


def measure(folder, pitch, rows):
    """Measure every way of one pitch, print its table and add its rows to the list."""
    ways = ways_of(folder)
    print("\n== a lattice of %s pixels ==" % pitch)
    print("%-15s %-9s %8s %8s %8s %8s %8s" % ("way", "engine", "blue", "spread", "white", "ink", "vs truth"))
    for way in ways:
        truth = middle(resolved(os.path.join(folder, way + "-x10", "chromium.png"),
                                read(os.path.join(folder, way, "chromium.png")).shape[0]))
        white, ink = shares(truth)
        print("%-15s %-9s %8.1f %8.1f %7.1f%% %7.1f%%" % (way, "resolved", truth[:, 2].mean(), truth[:, 2].std(), white, ink))
        rows.append((pitch, way, "resolved", truth[:, 2].mean(), truth[:, 2].std(), white, ink, 0.0))
        for name in BROWSERS:
            colours = middle(read(os.path.join(folder, way, name + ".png")))
            white, ink = shares(colours)
            off = colours[:, 2].mean() - truth[:, 2].mean()
            print("%-15s %-9s %8.1f %8.1f %7.1f%% %7.1f%% %+8.1f"
                  % ("", name, colours[:, 2].mean(), colours[:, 2].std(), white, ink, off))
            rows.append((pitch, way, name, colours[:, 2].mean(), colours[:, 2].std(), white, ink, off))
    return ways


def agreement(folder):
    """How far the engines' resolved drawings of the pattern lie apart, which says the truth is no engine's own."""
    held = [name for name in BROWSERS if os.path.exists(os.path.join(folder, "pattern-x10", name + ".png"))]
    if len(held) < 2:
        return
    means = {}
    for name in held:
        size = read(os.path.join(folder, "pattern", "chromium.png")).shape[0]
        means[name] = middle(resolved(os.path.join(folder, "pattern-x10", name + ".png"), size))[:, 2].mean()
    print("   resolved tenfold, the engines agree within %.1f levels: %s"
          % (max(means.values()) - min(means.values()), ", ".join("%s %.1f" % (n, m) for n, m in means.items())))


def main(arguments):
    if not arguments:
        sys.stderr.write(__doc__)
        return 2
    out = arguments[0]
    font_path = arguments[arguments.index("--font") + 1] if "--font" in arguments else None
    rows = []
    for pitch_folder in sorted(name for name in os.listdir(out) if name.startswith("pitch-")):
        folder = os.path.join(out, pitch_folder)
        pitch = pitch_folder[len("pitch-"):]
        ways = measure(folder, pitch, rows)
        agreement(folder)
        print("   sheet: " + sheet(folder, pitch, ways, font_path))
    path = os.path.join(out, "measurements.tsv")
    with open(path, "w", encoding="utf-8") as written:
        written.write("pitch\tway\tengine\tblue\tspread\twhite\tink\tvs truth\n")
        for row in rows:
            written.write("%s\t%s\t%s\t%.1f\t%.1f\t%.1f\t%.1f\t%+.1f\n" % row)
    print("\n" + path)
    return 0


if __name__ == "__main__":
    sys.exit(main(sys.argv[1:]))
