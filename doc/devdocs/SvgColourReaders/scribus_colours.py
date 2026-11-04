# What Scribus makes of the colours of SVG files. Run inside Scribus 1.6's scripter, as run-scribus.sh does:
#   xvfb-run -a scribus -g -ns -py scribus_colours.py <out folder> <file.svg>...
# Each file is placed, as File > Import > Get Vector File places it, on a page of a new document, and saved beside the
# results as <out folder>/<name>.sla. <out folder>/<name>.json then lists the document's colours, each with its colour
# space and values, and every imported item with its type, fill colour and line colour.
import json
import os
import sys
import xml.etree.ElementTree as ElementTree

import scribus


# The colour space and values of each colour of a saved document, by name, read from its SLA file.
# slaPath: the saved document.
def colours_of_sla(slaPath):
    colours = {}
    for element in ElementTree.parse(slaPath).iter("COLOR"):
        space = element.get("SPACE", "")
        if space == "CMYK":
            values = [float(element.get(channel, "0")) for channel in "CMYK"]
        elif space == "RGB":
            values = [float(element.get(channel, "0")) for channel in "RGB"]
        else:
            values = []
        colours[element.get("NAME")] = {"space": space, "values": values}
    return colours


# The drawn items of a saved document, members of groups included and groups left out, each with its type, its fill
# and line colours and its line width in points, read from its SLA file, where the copies a <use> makes share their names.
# slaPath: the saved document.
def items_of_sla(slaPath):
    itemTypes = {"2": "image", "4": "text frame", "5": "line", "6": "polygon", "7": "polyline", "8": "path text"}
    items = []
    for element in ElementTree.parse(slaPath).iter():
        if element.tag in ("PAGEOBJECT", "ITEM") and element.get("PTYPE") != "12":
            items.append({"type": itemTypes.get(element.get("PTYPE"), element.get("PTYPE")),
                          "fill": element.get("PCOLOR", ""), "line": element.get("PCOLOR2", ""),
                          "lineWidth": float(element.get("PWIDTH", "0"))})
    return items


# Place one file on a new page and write what Scribus made of its colours.
# svgPath: the file.
# outFolder: the folder to write <name>.sla and <name>.json into.
def read_file(svgPath, outFolder):
    name = os.path.splitext(os.path.basename(svgPath))[0]
    scribus.newDocument((1000, 1000), (0, 0, 0, 0), scribus.PORTRAIT, 1, scribus.UNIT_MILLIMETERS,
                        scribus.NOFACINGPAGES, scribus.FIRSTPAGERIGHT, 1)
    result = {"file": os.path.basename(svgPath)}
    try:
        scribus.placeSVG(svgPath, 0, 0)
        slaPath = os.path.join(outFolder, name + ".sla")
        scribus.saveDocAs(slaPath)
        result["colours"] = colours_of_sla(slaPath)
        result["items"] = items_of_sla(slaPath)
    except Exception as error:
        result["error"] = repr(error)
    finally:
        scribus.closeDoc()
    with open(os.path.join(outFolder, name + ".json"), "w") as out:
        json.dump(result, out, indent=1)


def main():
    outFolder = sys.argv[1]
    os.makedirs(outFolder, exist_ok=True)
    for svgPath in sys.argv[2:]:
        read_file(os.path.abspath(svgPath), outFolder)


main()
