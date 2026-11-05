# What Scribus 1.6 draws of SVG files, as images, for the gallery. Run inside Scribus's scripter:
#   xvfb-run -a scribus -g -ns -py scribus_png.py <dpi> <out folder> <file.svg>...
# Each file is placed, as File > Import > Get Vector File places it, on the page of a new document of the size its
# root gives in millimetres, and the page is exported as <out folder>/<name>.png at the given resolution. Scribus
# places an import by the bounds of what it draws, so the file is first copied beside the images with a white
# rectangle the size of its page put before its first page, and the import then lies where the page does.
import os
import re
import sys

import scribus


# The width and height in millimetres that a file's root gives.
def page_size(text):
    root = re.search(r"<svg\b[^>]*>", text).group(0)
    return (float(re.search(r'\bwidth="([0-9.]+)mm"', root).group(1)),
            float(re.search(r'\bheight="([0-9.]+)mm"', root).group(1)))


def main():
    dpi, outFolder = int(sys.argv[1]), os.path.abspath(sys.argv[2])
    os.makedirs(outFolder, exist_ok=True)
    for svgPath in sys.argv[3:]:
        name = os.path.splitext(os.path.basename(svgPath))[0]
        with open(svgPath, encoding="utf-8") as file:
            text = file.read()
        width, height = page_size(text)
        end = text.index("</defs>\n") + len("</defs>\n")
        paged = text[:end] + '<path style="fill:#fff" d="M0 0h%gv%gH0z"/>\n' % (width, height) + text[end:]
        pagedPath = os.path.join(outFolder, name + ".paged.svg")
        with open(pagedPath, "w", encoding="utf-8") as file:
            file.write(paged)
        scribus.newDocument((width, height), (0, 0, 0, 0), scribus.PORTRAIT, 1, scribus.UNIT_MILLIMETERS,
                            scribus.NOFACINGPAGES, scribus.FIRSTPAGERIGHT, 1)
        try:
            scribus.placeSVG(pagedPath, 0, 0)
            image = scribus.ImageExport()
            image.type = "PNG"
            image.dpi = dpi
            image.scale = 100
            image.name = os.path.join(outFolder, name + ".png")
            image.save()
        finally:
            scribus.closeDoc()
            os.remove(pagedPath)


main()
