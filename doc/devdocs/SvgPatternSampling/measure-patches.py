"""Measure, in patches of a real document, what the Skia reference and the browsers made of a screen, against the
same patch resolved tenfold.

Usage: python3 -I measure-patches.py <work folder> <patches file> [--font <ttf>]

The work folder holds renderings/<browser>.png, the document at its own size; reference.png, the Skia rendering the
rendering comparison holds it to (V-9); and patches/<id>/chromium.png, the patch rendered ten times larger. The
patches file is what patches.py printed: the pattern's id, the patch's left, top and side.

In the resolved patch, the screen's two colours are found: the paper and the ink the pattern's tile paints with. The
pixels taken are those whose hundred sub-pixels all lie on the line between the two, so that nothing else is in them,
and of them the measurement prints the mean of the channel that parts the two colours most, the spread of that channel,
how much of the area is paper, and the share of pixels that stayed paper white or full ink. It also prints how far each browser's image lies from
the Skia reference once both are blurred by a pixel, which is the difference F-1 judges at an edge. Writes the
measurements as measurements.tsv and the renderings beside each other as patches.png.
"""
import os
import sys

import numpy as np
from PIL import Image, ImageDraw, ImageFilter, ImageFont

BROWSERS = ["chromium", "firefox", "webkit"]
PAPER = np.array([255.0, 255.0, 255.0])


def read(path):
    """One rendering, as rows of red, green and blue."""
    return np.asarray(Image.open(path).convert("RGB")).astype(np.float64)


def ink_of(subpixels):
    """The colour the screen's tile paints with: the commonest colour well away from the paper."""
    colours, counts = np.unique(subpixels.reshape(-1, 3).astype(np.int16), axis=0, return_counts=True)
    away = np.abs(colours - PAPER).max(axis=1) > 60
    if not away.any():
        return colours[np.argmax(counts)].astype(np.float64)
    return colours[away][np.argmax(counts[away])].astype(np.float64)


def on_the_line(colours, ink, reach=8.0):
    """Whether each colour lies within reach of the line from the ink to the paper."""
    along = PAPER - ink
    share = np.clip(((colours - ink) @ along) / (along @ along), 0.0, 1.0)
    nearest = ink + share[..., None] * along
    return np.abs(colours - nearest).max(axis=-1) <= reach


def screen_alone(resolved_cells, ink):
    """The pixels of a patch that hold the screen and nothing else."""
    return on_the_line(resolved_cells, ink).all(axis=(1, 3))


def along_the_line(colours, ink):
    """How much of each colour is paper, from 0 at the ink to 1 at the paper."""
    along = PAPER - ink
    return np.clip(((colours - ink) @ along) / (along @ along), 0.0, 1.0)


def describe(name, colours, ink, channel, truth_level, rows, pattern):
    """Print and record one image's measurement of a patch."""
    level = colours[:, channel].mean()
    spread = colours[:, channel].std()
    paper = along_the_line(colours, ink).mean() * 100
    white = ((colours[:, 0] >= 250) & (colours[:, 1] >= 250) & (colours[:, 2] >= 250)).mean() * 100
    full = (np.abs(colours - ink).max(axis=1) <= 8).mean() * 100
    off = "" if truth_level is None else "%+8.1f" % (level - truth_level)
    print("%-10s %-9s %8.1f %8.1f %7.1f%% %7.1f%% %7.1f%% %s" % ("", name, level, spread, paper, white, full, off))
    rows.append((pattern, name, level, spread, paper, white, full, 0.0 if truth_level is None else level - truth_level))
    return level


def blurred_apart(work, x, y, side):
    """How far each browser's image lies from the Skia reference in a patch, once both are blurred by a pixel."""
    def blurred(path):
        image = Image.open(path).convert("RGB").filter(ImageFilter.GaussianBlur(1.0))
        return np.asarray(image).astype(np.float64)[y:y + side, x:x + side]
    reference = blurred(os.path.join(work, "reference.png"))
    apart = {}
    for name in BROWSERS:
        difference = np.abs(reference - blurred(os.path.join(work, "renderings", name + ".png"))).max(axis=2)
        apart[name] = (difference.mean(), difference.max())
    return apart


def sheet(work, measured, font_path):
    """Write the patches beside each other: a row for each patch, the resolved drawing and then each image."""
    try:
        font = ImageFont.truetype(font_path, 17) if font_path else ImageFont.load_default()
        small = ImageFont.truetype(font_path, 13) if font_path else ImageFont.load_default()
    except OSError:
        font = small = ImageFont.load_default()
    detail = min(side for _, _, _, side in measured)
    tile, gap, left, top, label = detail * 10, 12, 180, 28, 18
    columns = ["resolved", "skia"] + BROWSERS
    image = Image.new("RGB", (left + len(columns) * (tile + gap) - gap + 16,
                              top + len(measured) * (tile + gap + label) - gap + 8), "white")
    draw = ImageDraw.Draw(image)
    for column, name in enumerate(columns):
        draw.text((left + column * (tile + gap), 6), name, "black", font=font)
    for row, (pattern, x, y, side) in enumerate(measured):
        at = top + row * (tile + gap + label)
        draw.text((8, at + tile // 2 - 8), pattern, "black", font=small)
        for column, name in enumerate(columns):
            if column == 0:
                part = read(os.path.join(work, "patches", pattern, "chromium.png"))[:detail * 10, :detail * 10]
                piece = Image.fromarray(part.astype(np.uint8))
            else:
                path = os.path.join(work, "reference.png") if name == "skia" \
                    else os.path.join(work, "renderings", name + ".png")
                part = read(path)[y:y + detail, x:x + detail]
                piece = Image.fromarray(part.astype(np.uint8)).resize((tile, tile), Image.NEAREST)
            image.paste(piece, (left + column * (tile + gap), at))
            draw.rectangle([left + column * (tile + gap), at, left + column * (tile + gap) + tile - 1, at + tile - 1],
                           outline=(120, 120, 120))
    path = os.path.join(work, "patches.png")
    image.save(path)
    return path


def main(arguments):
    if len(arguments) < 2:
        sys.stderr.write(__doc__)
        return 2
    work = arguments[0]
    font_path = arguments[arguments.index("--font") + 1] if "--font" in arguments else None
    rows, measured = [], []
    with open(arguments[1], encoding="utf-8") as read_file:
        patches = [line.split("\t") for line in read_file.read().splitlines() if line.strip()]
    for columns in patches:
        pattern, x, y, side = columns[0], int(columns[1]), int(columns[2]), int(columns[3])
        resolved_path = os.path.join(work, "patches", pattern, "chromium.png")
        if not os.path.exists(resolved_path):
            continue
        cells = read(resolved_path).reshape(side, 10, side, 10, 3)
        ink = ink_of(cells)
        alone = screen_alone(cells, ink)
        if alone.sum() < 50:
            print("%s: too little of the patch holds the screen alone (%d pixels)" % (pattern, int(alone.sum())))
            continue
        channel = int(np.argmax(np.abs(PAPER - ink)))
        truth = cells.mean(axis=(1, 3))[alone]
        print("\n== %s, a patch of %d pixels a side at %d,%d: the screen alone in %d of them, "
              "ink (%d,%d,%d) ==" % (pattern, side, x, y, int(alone.sum()), ink[0], ink[1], ink[2]))
        print("%-10s %-9s %8s %8s %8s %8s %8s %8s"
              % ("", "image", "level", "spread", "paper", "white", "ink", "vs truth"))
        level = describe("resolved", truth, ink, channel, None, rows, pattern)
        describe("skia", read(os.path.join(work, "reference.png"))[y:y + side, x:x + side][alone], ink, channel,
                 level, rows, pattern)
        for name in BROWSERS:
            describe(name, read(os.path.join(work, "renderings", name + ".png"))[y:y + side, x:x + side][alone],
                     ink, channel, level, rows, pattern)
        apart = blurred_apart(work, x, y, side)
        print("%-10s blurred by a pixel, the browsers lie from the reference by %s levels"
              % ("", ", ".join("%s %.1f (at most %.0f)" % (name, mean, largest)
                               for name, (mean, largest) in apart.items())))
        measured.append((pattern, x, y, side))
    path = os.path.join(work, "measurements.tsv")
    with open(path, "w", encoding="utf-8") as written:
        written.write("pattern\timage\tlevel\tspread\tpaper\twhite\tink\tvs truth\n")
        for row in rows:
            written.write("%s\t%s\t%.1f\t%.1f\t%.1f\t%.1f\t%.1f\t%+.1f\n" % row)
    print("\n" + path)
    if measured:
        print(sheet(work, measured, font_path))
    return 0


if __name__ == "__main__":
    sys.exit(main(sys.argv[1:]))
