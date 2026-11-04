# Sums up what scribus_colours.py found, a line a file: how many items Scribus made, how they are filled and stroked,
# and the CMYK colours it defined from the file.
#   python3 -I summarise_scribus.py <folder of scribus_colours.py's .json files>
# "Black" counts items filled with Scribus's own Black, which an SVG fill Scribus did not read leaves them; a colour
# Scribus read whose values equal one of its own colours, such as Black or Magenta, takes that colour's name.
import collections
import glob
import json
import os
import sys


def kind_of(colourName, colours):
    if colourName in ("", "None"):
        return "none"
    colour = colours.get(colourName)
    if colour is None:
        return "?"
    return colour["space"] if colourName.startswith("FromSVG") else colourName


def main():
    for path in sorted(glob.glob(os.path.join(sys.argv[1], "*.json"))):
        result = json.load(open(path))
        name = os.path.splitext(os.path.basename(path))[0]
        if "error" in result:
            print("%s: %s" % (name, result["error"]))
            continue
        colours = result["colours"]
        fills = collections.Counter(kind_of(item["fill"], colours) for item in result["items"])
        lines = collections.Counter(kind_of(item["line"], colours) for item in result["items"])
        widths = sum(1 for item in result["items"] if item["line"] not in ("", "None") and item["lineWidth"] > 0.01)
        cmyk = sorted({tuple(round(value, 2) for value in colours[item[part]]["values"])
                       for item in result["items"] for part in ("fill", "line")
                       if colours.get(item[part], {}).get("space") == "CMYK"})
        print("%s: %d items; fill %s; line %s, %d with a width; %d CMYK colours: %s" % (
            name, len(result["items"]), dict(fills), dict(lines), widths, len(cmyk),
            " ".join("/".join("%g" % value for value in values) for values in cmyk)))


main()
