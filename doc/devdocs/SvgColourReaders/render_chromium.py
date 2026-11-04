# Draws SVG files in Chromium and compares each drawing with the first file's, pixel for pixel.
#   python3 -I render_chromium.py <out folder> <reference.svg> <file.svg>...
# Needs Python's Playwright and its Chromium, which it starts with the flags ChromiumRenderer passes (text unhinted, in
# grayscale and in sRGB), and Pillow. Each file is opened as a document of its own, in a viewport of the root's size
# at 96 pixels to the inch, and its drawing written to <out folder>/<name>.png. A line a file says how many pixels
# differ from the reference's drawing and by how many levels at most.
import os
import re
import sys

from PIL import Image, ImageChops
from playwright.sync_api import sync_playwright


# The size in pixels of a document whose root gives its width and height in millimetres.
def viewport_of(svgPath):
    with open(svgPath, encoding="utf-8") as file:
        root = re.search(r"<svg\b[^>]*>", file.read(4096)).group(0)
    width = float(re.search(r'\bwidth="([0-9.]+)mm"', root).group(1))
    height = float(re.search(r'\bheight="([0-9.]+)mm"', root).group(1))
    return {"width": round(width / 25.4 * 96), "height": round(height / 25.4 * 96)}


def main():
    outFolder = sys.argv[1]
    paths = [os.path.abspath(path) for path in sys.argv[2:]]
    os.makedirs(outFolder, exist_ok=True)
    flags = ["--font-render-hinting=none", "--disable-lcd-text", "--force-color-profile=srgb"]
    reference = None
    with sync_playwright() as playwright:
        browser = playwright.chromium.launch(args=flags)
        for path in paths:
            page = browser.new_page(viewport=viewport_of(path), device_scale_factor=1)
            page.goto("file://" + path)
            pngPath = os.path.join(outFolder, os.path.splitext(os.path.basename(path))[0] + ".png")
            page.screenshot(path=pngPath)
            page.close()
            image = Image.open(pngPath).convert("RGB")
            if reference is None:
                reference = image
                print("%s: reference" % os.path.basename(path))
                continue
            if image.size != reference.size:
                print("%s: size %s, reference %s" % (os.path.basename(path), image.size, reference.size))
                continue
            difference = ImageChops.difference(image, reference)
            pixels = difference.size[0] * difference.size[1] - difference.convert("L").histogram()[0]
            largest = max(high for low, high in difference.getextrema())
            print("%s: %d pixels differ, by up to %d levels" % (os.path.basename(path), pixels, largest))
        browser.close()


main()
