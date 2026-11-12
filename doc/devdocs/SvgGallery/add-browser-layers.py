"""Add Firefox's and WebKit's images of cases to a step of the SVG gallery, beside Chromium's.

Usage: python3 -I add-browser-layers.py <renderings folder> <gallery folder> <step id> <case,case,...>

<renderings folder> is a directory of renderings that a run of Map_Svg.Tests kept, as SVG_LOOP_RENDERINGS names it and
the loop's artifact svg-loop-renderings-<name> holds it: Chromium's image of each case under renderings/, or under
browsers/chromium/ for a document only the browser check rendered, and each browser's image under browsers/<browser>/.
For each case named, writes into <gallery folder>/<step id>/ <case>-firefox.png and <case>-webkit.png, and
<case>-firefox-difference.png and <case>-webkit-difference.png: Chromium's image ghosted, with every pixel the browser
draws otherwise painted in the blue of its bin, as make-gallery-step.py paints Chromium's image against Skia's.

Prints, as JSON, each case's layers for the page's LOG, which the page shows after its own four, and for each browser
the largest difference in a channel from Chromium's image and how many pixels differ by more than each bin's start.
"""
import importlib.util
import json
import os
import sys

import numpy as np
from PIL import Image

BROWSERS = [("firefox", "Firefox"), ("webkit", "WebKit")]


def load_gallery_step():
    """The functions of make-gallery-step.py beside this script, whose images these match."""
    path = os.path.join(os.path.dirname(os.path.abspath(__file__)), "make-gallery-step.py")
    spec = importlib.util.spec_from_file_location("make_gallery_step", path)
    module = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(module)
    return module


def find_chromium(renderings, name):
    """Return the path of Chromium's image of a case: the comparison's, else the browser check's."""
    for folder in (os.path.join(renderings, "renderings"), os.path.join(renderings, "browsers", "chromium")):
        candidate = os.path.join(folder, name + ".png")
        if os.path.exists(candidate):
            return candidate
    raise FileNotFoundError("no image of %s by Chromium in %s" % (name, renderings))


def main():
    step_module = load_gallery_step()
    renderings, gallery, step, case_list = sys.argv[1:5]
    out = os.path.join(gallery, step)
    os.makedirs(out, exist_ok=True)
    cases = []
    for name in case_list.split(","):
        chromium = step_module.read_rgb(find_chromium(renderings, name))
        layers, browsers = [], {}
        for key, label in BROWSERS:
            rendering = step_module.read_rgb(os.path.join(renderings, "browsers", key, name + ".png"))
            picture, difference = step_module.make_difference_image(chromium, rendering)
            Image.fromarray(rendering).save(os.path.join(out, name + "-" + key + ".png"), optimize=True)
            Image.fromarray(picture).save(os.path.join(out, name + "-" + key + "-difference.png"), optimize=True)
            layers.append({"key": key, "label": label, "caption": "In the browser check"})
            layers.append({"key": key + "-difference", "label": label + " against Chromium", "caption": "Darker blue, larger"})
            browsers[key] = {"largest difference": int(difference.max())}
            for low, _, _ in step_module.BINS:
                browsers[key]["beyond %d" % low] = int((difference > low).sum())
        cases.append({"name": name, "layers": layers, "browsers": browsers})
    print(json.dumps(cases, indent=1))


if __name__ == "__main__":
    main()
