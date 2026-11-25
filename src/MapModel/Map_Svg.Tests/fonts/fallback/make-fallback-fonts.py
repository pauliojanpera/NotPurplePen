"""Make the fallback faces of the SVG tests: Noto faces cut down to the characters in characters.txt, the characters
that the test maps draw and no face the tests register has.

Usage: python3 -I make-fallback-fonts.py [--collect <SVG folder>] [<cache folder>]

Each source is fetched from GitHub at a pinned commit, into the cache folder (by default ~/.cache/svg-test-fonts),
once. Each character goes to the first source, in the order below, that has it, and each source with characters is
cut down to them, with the glyphs its layout reaches from them, and written here under its own name; a character no
source has is reported and left to be drawn as a box. The faces keep their names: none of Noto's licences reserves
one, and Noto Sans CJK reserves only "Source". OFL.txt collects their copyrights and the licence.

--collect first rewrites characters.txt from the SVG documents of a run of the tests (a results folder's svg/): every
character of their texts that no face in ../ has, except white space and controls. Run the tests once with the
characters to add, collect, make the faces, and run them again.

Needs Python 3 with fontTools, and curl.
"""
import glob
import os
import re
import subprocess
import sys
import unicodedata
from html import unescape

from fontTools import subset
from fontTools.ttLib import TTFont
from fontTools.varLib import instancer

HERE = os.path.dirname(os.path.abspath(__file__))
RAW = "https://raw.githubusercontent.com/"
NOTO = "notofonts/notofonts.github.io/42af67ccd12bc0e31ed85384bf73e39248265374/fonts/"

# The sources, in the order characters go to them: the faces of one script first, then the symbols, the emoji, and
# last the CJK face, which has many characters of other blocks too.
SOURCES = [(name, NOTO + name + "/unhinted/ttf/" + name + "-Regular.ttf") for name in (
    "NotoSansDevanagari", "NotoSansBengali", "NotoSansTamil", "NotoSansThai", "NotoSansMyanmar", "NotoSansKhmer",
    "NotoSansGeorgian", "NotoSansArmenian", "NotoSansEthiopic", "NotoSansRunic", "NotoSansMath", "NotoMusic",
    "NotoSansSymbols2", "NotoSansSymbols")] + [
    # A variable face, made static at the regular weight.
    ("NotoEmoji", "google/fonts/e7db6ca603fd4862010383ff17ffd6cc4ffac1c0/ofl/notoemoji/NotoEmoji%5Bwght%5D.ttf"),
    ("NotoSansCJKjp", "notofonts/noto-cjk/f8d157532fbfaeda587e826d4cd5b21a49186f7c/Sans/OTF/Japanese/NotoSansCJKjp-Regular.otf"),
]

LICENCE = RAW + "google/fonts/e7db6ca603fd4862010383ff17ffd6cc4ffac1c0/ofl/notoemoji/OFL.txt"


def fetch(url, cache):
    path = os.path.join(cache, os.path.basename(url).replace("%5B", "[").replace("%5D", "]"))
    if not os.path.exists(path):
        subprocess.run(["curl", "-sSfL", "--max-time", "600", "-o", path + ".part", url], check=True)
        os.replace(path + ".part", path)
    return path


def registered_characters():
    """The characters the faces the tests register have: those in ../, and Roboto's in Map_Skia.Tests."""
    paths = glob.glob(os.path.join(HERE, "..", "*.ttf"))
    paths += glob.glob(os.path.join(HERE, "..", "..", "..", "Map_Skia.Tests", "fonts", "Roboto*.ttf"))
    characters = set()
    for path in paths:
        if os.path.basename(path) != "Sentinel.ttf":
            characters |= set(TTFont(path).getBestCmap())
    return characters


def collect(folder):
    registered = registered_characters()
    wanted = set()
    for path in glob.glob(os.path.join(folder, "*.svg")):
        with open(path, encoding="utf-8") as file:
            document = re.sub(r"<style>.*?</style>", "", file.read(), flags=re.S)
        for text in re.findall(r">([^<]+)<", document):
            for character in unescape(text):
                if ord(character) not in registered and not character.isspace() and unicodedata.category(character) != "Cc":
                    wanted.add(ord(character))
    # A document writes the emoji presentation selector as the text one; the application has the first.
    if 0xFE0E in wanted:
        wanted.add(0xFE0F)
    with open(os.path.join(HERE, "characters.txt"), "w", encoding="utf-8") as file:
        file.write("".join(chr(code) for code in sorted(wanted)) + "\n")
    print("collected %d characters from %s" % (len(wanted), folder))


def main():
    arguments = sys.argv[1:]
    if arguments[:1] == ["--collect"]:
        collect(arguments[1])
        arguments = arguments[2:]
    cache = arguments[0] if arguments else os.path.expanduser("~/.cache/svg-test-fonts")
    os.makedirs(cache, exist_ok=True)
    with open(os.path.join(HERE, "characters.txt"), encoding="utf-8") as file:
        wanted = sorted(set(ord(character) for character in file.read().strip()))

    fonts, cmaps = {}, {}
    for name, path in SOURCES:
        fonts[name] = TTFont(fetch(RAW + path, cache))
        if "fvar" in fonts[name]:
            fonts[name] = instancer.instantiateVariableFont(fonts[name], {"wght": 400})
        cmaps[name] = set(fonts[name].getBestCmap())

    given = {name: [] for name, _ in SOURCES}
    for code in wanted:
        source = next((name for name, _ in SOURCES if code in cmaps[name]), None)
        if source is None:
            print("no source has U+%04X %s" % (code, unicodedata.name(chr(code), "")))
        else:
            given[source].append(code)

    for old in glob.glob(os.path.join(HERE, "*.ttf")) + glob.glob(os.path.join(HERE, "*.otf")):
        os.remove(old)
    copyrights = []
    for name, _ in SOURCES:
        if not given[name]:
            continue
        font = fonts[name]
        options = subset.Options()
        options.layout_features = ["*"]
        options.name_IDs = ["*"]
        options.name_languages = ["*"]
        options.notdef_outline = True
        options.glyph_names = False
        subsetter = subset.Subsetter(options)
        subsetter.populate(unicodes=given[name])
        subsetter.subset(font)
        extension = ".otf" if font.sfntVersion == "OTTO" else ".ttf"
        font.save(os.path.join(HERE, name + "-Regular" + extension))
        copyright = font["name"].getDebugName(0)
        if copyright and copyright not in copyrights:
            copyrights.append(copyright)
        print("%-20s %4d characters, %7d bytes" % (name, len(given[name]),
                                                 os.path.getsize(os.path.join(HERE, name + "-Regular" + extension))))

    licence = open(fetch(LICENCE, cache), encoding="utf-8").read()
    body = licence[licence.index("This Font Software is licensed"):]
    with open(os.path.join(HERE, "OFL.txt"), "w", encoding="utf-8") as file:
        file.write("The Noto faces in this folder, cut down to the characters the SVG tests draw.\n\n")
        file.write("\n".join(copyrights) + "\n\n" + body)


if __name__ == "__main__":
    main()
