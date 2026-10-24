#!/usr/bin/env python3
"""check-embedded-fonts.py [--ots <ots-sanitize>] [--quiet] <SVG file or folder>...

Lists the fonts that SVG documents embed in their @font-face rules, with each font's size and glyph count, and checks
each with the OpenType Sanitizer, which Chromium and Firefox run on every web font they load: a font it rejects is
one a browser draws no text in, falling back to the viewer's own fonts. Prints a line for each font, then the totals,
and exits 1 when the sanitizer rejected a font.

The sanitizer is the `ots-sanitize` of the `opentype-sanitizer` package: --ots names it, or it is found on the PATH or
beside an importable `ots` module, as after

    pip install --break-system-packages --target <folder> opentype-sanitizer

with <folder> on PYTHONPATH. Without it the fonts are listed unchecked. fontTools, when it is installed, gives the glyph
counts. Run it with `python3 -I`: it reads the documents as data.
"""
import argparse
import base64
import io
import os
import re
import shutil
import subprocess
import sys
import tempfile

FONT_FACE = re.compile(r'@font-face\{font-family:("(?:[^"\\]|\\.)*"|[^;]*)([^}]*?);src:url\(data:(font/[a-z]+);base64,([A-Za-z0-9+/=]+)\)\}')


def find_ots(named):
    """The path of ots-sanitize, or None."""
    if named:
        return named
    found = shutil.which("ots-sanitize")
    if found:
        return found
    for entry in os.environ.get("PYTHONPATH", "").split(os.pathsep):
        candidate = os.path.join(entry, "ots", "ots-sanitize")
        if entry and os.access(candidate, os.X_OK):
            return candidate
    return None


def glyph_count(data):
    """The count of glyphs in a font, or None without fontTools."""
    try:
        from fontTools.ttLib import TTFont
    except ImportError:
        return None
    try:
        return TTFont(io.BytesIO(data), lazy=True)["maxp"].numGlyphs
    except Exception:
        return None


def sanitize(ots, data):
    """None when the sanitizer accepts a font, else its complaint."""
    with tempfile.TemporaryDirectory() as folder:
        path = os.path.join(folder, "font")
        with open(path, "wb") as file:
            file.write(data)
        result = subprocess.run([ots, path, os.devnull], capture_output=True, text=True)
        if result.returncode == 0:
            return None
        return (result.stderr or result.stdout).strip().replace("\n", " | ")[:300]


def documents(paths):
    """The SVG files among the paths, folders searched."""
    for path in paths:
        if os.path.isdir(path):
            for root, _, names in os.walk(path):
                for name in sorted(names):
                    if name.endswith(".svg"):
                        yield os.path.join(root, name)
        else:
            yield path


def main():
    parser = argparse.ArgumentParser(description=__doc__.split("\n\n")[1], usage=__doc__.split("\n")[0])
    parser.add_argument("--ots")
    parser.add_argument("--quiet", action="store_true", help="print only the rejected fonts and the totals")
    parser.add_argument("paths", nargs="+")
    arguments = parser.parse_args()
    ots = find_ots(arguments.ots)
    if not ots:
        print("no ots-sanitize: the fonts are listed unchecked", file=sys.stderr)

    fonts = total = rejected = 0
    seen = {}
    for document in documents(arguments.paths):
        with open(document, encoding="utf-8") as file:
            text = file.read()
        for match in FONT_FACE.finditer(text):
            family, kind, encoded = match.group(1), match.group(3), match.group(4)
            data = base64.b64decode(encoded)
            fonts += 1
            total += len(data)
            key = (len(data), encoded[:64], encoded[-64:])
            if key not in seen:
                seen[key] = sanitize(ots, data) if ots else None
            complaint = seen[key]
            if complaint:
                rejected += 1
            if complaint or not arguments.quiet:
                glyphs = glyph_count(data)
                print("%-40s %-28s %-8s %8d bytes %6s glyphs  %s" % (
                    os.path.basename(document)[:40], family[:28], kind, len(data),
                    "?" if glyphs is None else glyphs, ("REJECTED: " + complaint) if complaint else ("ok" if ots else "unchecked")))
    print("%d fonts, %d bytes in all, %d rejected%s" % (fonts, total, rejected, "" if ots else " (not checked)"))
    return 1 if rejected else 0


if __name__ == "__main__":
    sys.exit(main())
