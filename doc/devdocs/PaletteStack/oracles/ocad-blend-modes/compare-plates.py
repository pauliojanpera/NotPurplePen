"""Compare the plates of the inks that Ghostscript draws of a PDF export of the blend-mode sample of ticket 027 with the
inks that measure-pdf.py gives by the PDF model in a CMYK blending space.

Usage: compare-plates.py MEASURE-PDF.py CELLS.json EXPORT.pdf PLATE-FOLDER DPI

Draw the plates first, at the same DPI, with Ghostscript's tiffsep device, which writes one grayscale TIFF for each
process ink and each spot ink of the page, without a conversion of the colors:

    gs -q -dNOPAUSE -dBATCH -dSAFER -sDEVICE=tiffsep -r100 -sOutputFile=PLATE-FOLDER/sep.tif EXPORT.pdf

The script reads the ink of each plate at the point of each region of each cell (the median of 3 x 3 pixels; a
plate gives 255 where it has no ink), and prints the inks above 0.5 percent, the inks of the model where the model
applies, their largest difference, and the largest difference of all the regions."""
import importlib.util
import json
import os
import sys
from PIL import Image

spec = importlib.util.spec_from_file_location("mp", sys.argv[1])
mp = importlib.util.module_from_spec(spec)
spec.loader.exec_module(mp)
cells = json.load(open(sys.argv[2]))
pdf = mp.Pdf(sys.argv[3])
catalog = pdf.get(pdf.trailer.get("Root"))
page = pdf.get(pdf.get(pdf.get(catalog["Pages"])["Kids"])[0])
height_pt = pdf.get(page["MediaBox"])[3]
it = mp.Interpreter(pdf)
it.run(page)
fits = mp.fit_marks(it.painted, cells["marks"])
for shape in it.painted:
    shape["boxes_mm"] = []
    for sp in shape["subpaths"]:
        x0, y0, x1, y1 = mp.bbox(sp)
        (a, b), (c, d) = mp.to_mm(fits, (x0, y0)), mp.to_mm(fits, (x1, y1))
        shape["boxes_mm"].append((min(a, c), min(b, d), max(a, c), max(b, d)))
dpi = float(sys.argv[5])
plates = {}
for name in os.listdir(sys.argv[4]):
    if name.startswith("sep(") and name.endswith(").tif"):
        plates[name[4:-5]] = Image.open(os.path.join(sys.argv[4], name)).convert("L")

def ink(plate, x_mm, y_mm):
    x_pt = fits[0][0] * x_mm + fits[0][1]
    y_pt = fits[1][0] * y_mm + fits[1][1]
    px, py = int(x_pt * dpi / 72), int((height_pt - y_pt) * dpi / 72)
    values = sorted(plate.getpixel((px + dx, py + dy)) for dx in (-1, 0, 1) for dy in (-1, 0, 1))
    return 100 * (255 - values[4]) / 255

names = {"C": "Cyan", "M": "Magenta", "Y": "Yellow", "K": "Black"}
worst, count, rows = 0, 0, []
for cell in cells["cells"]:
    rect = cell["rect"]
    mine = [s for s in it.painted if mp.inside(rect, (s["boxes_mm"][0][0] + s["boxes_mm"][0][2]) / 2,
                                              (s["boxes_mm"][0][1] + s["boxes_mm"][0][3]) / 2, 0.5)]
    for region in cell["regions"]:
        x, y = region["point"]
        measured = {p: ink(plates[p], x, y) for p in plates}
        model, reason = mp.model_inks(mine, it.groups, x, y)
        line = "%s | %s | %s" % (cell["id"], region["name"],
                                 " ".join("%s%.0f" % (p, v) for p, v in sorted(measured.items()) if v > 0.5) or "no ink")
        if model:
            diff = max(abs(model[i] - measured[names[k]]) for i, k in enumerate("CMYK"))
            worst, count = max(worst, diff), count + 1
            line += " | model %s | difference %.1f" % (mp.ink_text(model), diff)
        rows.append(line)
print("\n".join(rows))
print("modelled regions: %d, largest difference from the plates: %.1f percentage points" % (count, worst))
