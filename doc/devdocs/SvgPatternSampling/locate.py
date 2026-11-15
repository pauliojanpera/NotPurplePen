"""Write copies of a document in which one pattern is painted a colour of its own, so that rendering them beside the
document itself shows where each pattern is drawn.

Usage: python3 -I locate.py <svg> <out folder> [<pattern id>...]

Writes <out folder>/base.svg, the document as it is, and <out folder>/<id>.svg for each pattern, all of them by
default. In a copy, every paint of that pattern's tile becomes magenta and its icc-color fallbacks are dropped, so
the pixels that differ from the base rendering are the pixels that pattern draws. Prints each copy's id.
"""
import os
import re
import sys


def patterns(text):
    """The ids of the patterns a document defines, in the order it defines them."""
    return re.findall(r'<pattern id="([^"]+)"', text)


def recoloured(text, identifier):
    """The document with one pattern's tile painted magenta."""
    found = re.search(r'<pattern id="%s".*?</pattern>' % re.escape(identifier), text, re.S)
    if found is None:
        raise ValueError("no pattern " + identifier)
    tile = found.group(0)
    tile = re.sub(r"fill:#[0-9a-fA-F]{3,6}", "fill:#f0f", tile)
    tile = re.sub(r"stroke:#[0-9a-fA-F]{3,6}", "stroke:#f0f", tile)
    tile = re.sub(r"icc-color\([^)]*\)", "", tile)
    return text[:found.start()] + tile + text[found.end():]


def main(arguments):
    if len(arguments) < 2:
        sys.stderr.write(__doc__)
        return 2
    with open(arguments[0], encoding="utf-8") as read:
        text = read.read()
    out = arguments[1]
    os.makedirs(out, exist_ok=True)
    with open(os.path.join(out, "base.svg"), "w", encoding="utf-8") as written:
        written.write(text)
    for identifier in arguments[2:] or patterns(text):
        with open(os.path.join(out, identifier + ".svg"), "w", encoding="utf-8") as written:
            written.write(recoloured(text, identifier))
        print(identifier)
    return 0


if __name__ == "__main__":
    sys.exit(main(sys.argv[1:]))
