"""Make the variable face of the SVG tests: Roboto's variable face, with its axes of weight and width, cut down to
the printable characters of ASCII, its variations kept.

Usage: python3 -I make-variable-font.py [<cache folder>]

The source is fetched from GitHub at a pinned commit of google/fonts, into the cache folder (by default
~/.cache/svg-test-fonts), once, and written here as RobotoVariable-Test.ttf with its licence as OFL.txt. Roboto's
licence reserves no name, so the face keeps its own.

Needs Python 3 with fontTools, and curl.
"""
import os
import subprocess
import sys

from fontTools import subset
from fontTools.ttLib import TTFont

HERE = os.path.dirname(os.path.abspath(__file__))
SOURCE = "https://raw.githubusercontent.com/google/fonts/e7db6ca603fd4862010383ff17ffd6cc4ffac1c0/ofl/roboto/"
FONT = "Roboto%5Bwdth%2Cwght%5D.ttf"
LICENCE = "OFL.txt"


def fetch(url, cache):
    path = os.path.join(cache, "google-fonts-roboto-" + url.rsplit("/", 1)[1].replace("%5B", "[").replace("%5D", "]").replace("%2C", ","))
    if not os.path.exists(path):
        subprocess.run(["curl", "-sSfL", "--max-time", "600", "-o", path + ".part", url], check=True)
        os.replace(path + ".part", path)
    return path


def main():
    cache = sys.argv[1] if len(sys.argv) > 1 else os.path.expanduser("~/.cache/svg-test-fonts")
    os.makedirs(cache, exist_ok=True)
    font = TTFont(fetch(SOURCE + FONT, cache))
    options = subset.Options()
    options.layout_features = ["*"]
    options.name_IDs = ["*"]
    options.name_languages = ["*"]
    options.notdef_outline = True
    options.glyph_names = False
    options.hinting = False
    subsetter = subset.Subsetter(options)
    subsetter.populate(unicodes=range(0x20, 0x7F))
    subsetter.subset(font)
    target = os.path.join(HERE, "RobotoVariable-Test.ttf")
    font.save(target)
    with open(fetch(SOURCE + LICENCE, cache), encoding="utf-8") as source, open(os.path.join(HERE, "OFL.txt"), "w", encoding="utf-8") as copy:
        copy.write(source.read())
    print("RobotoVariable-Test.ttf: %d glyphs, %d bytes, axes %s" % (
        font["maxp"].numGlyphs, os.path.getsize(target), ", ".join(axis.axisTag for axis in font["fvar"].axes)))


if __name__ == "__main__":
    main()
