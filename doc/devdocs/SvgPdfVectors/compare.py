#!/usr/bin/env python3
"""compare.py [--text] [--diff <out.png>] <expected.png> <rendering.png>

Holds a rendering to the image it should match by F-1's rule, as the SVG tests' ImageComparison.MeasureExcess does with
PixelTolerance.Default (or PixelTolerance.Text with --text), and prints one line of JSON: the pixels outside the
tolerance, those of them at an edge, the largest channel difference, and the share of pixels that differ by more than
8, 32 and 64 levels. With --diff, writes the pixels outside the tolerance in red over the expected image, ghosted.

The rule: each channel of a pixel lies within the range that channel takes in the other image within 1 pixel, give or
take 4 levels, both ways round; a pixel outside that range with an edge within reach passes when the two images, each
blurred by a Gaussian of 1 pixel with white paper beyond them, differ there in every channel by at most 40 levels (80
for text) and by at most 0.4 of the contrast around it, the widest range a channel takes in either image within 4
pixels. Run with python3 -I. Needs NumPy, SciPy and Pillow.
"""
import json
import sys

import numpy as np
from PIL import Image
from scipy import ndimage

REACH, SLACK, EDGE_SHARE = 1, 4, 0.4


def load(path):
    """An image as RGB levels, composited on white."""
    image = Image.open(path)
    if image.mode in ("RGBA", "LA", "P"):
        image = image.convert("RGBA")
        background = Image.new("RGBA", image.size, (255, 255, 255, 255))
        image = Image.alpha_composite(background, image)
    return np.asarray(image.convert("RGB")).astype(np.int32)


def window(image, size, highest):
    """The highest or lowest level of each channel within a square window, white beyond the image."""
    filter_ = ndimage.maximum_filter if highest else ndimage.minimum_filter
    return filter_(image, size=(size, size, 1), mode="constant", cval=255)


def blur(image):
    """The image blurred by a Gaussian of 1 pixel, reaching 4, white beyond it."""
    return ndimage.gaussian_filter(image.astype(np.float64), sigma=(1, 1, 0), mode="constant", cval=255, truncate=4.0)


def outside_tolerance(e, r, edge_levels):
    """The pixels of two images of one size that lie outside the tolerance, and those of them at an edge."""
    size = 2 * REACH + 1
    excess = np.zeros(e.shape[:2], dtype=np.int32)
    at_edge = np.zeros(e.shape[:2], dtype=bool)
    for values, ranged in ((r, e), (e, r)):
        low, high = window(ranged, size, False), window(ranged, size, True)
        beyond = np.maximum(low - SLACK - values, values - high - SLACK).max(axis=2)
        excess = np.maximum(excess, beyond)
        at_edge |= ((high - low) > SLACK).any(axis=2)
    judged = (excess > 0) & at_edge
    contrast = np.maximum(window(e, 9, True) - window(e, 9, False), window(r, 9, True) - window(r, 9, False)).max(axis=2)
    allowed = np.minimum(edge_levels, EDGE_SHARE * contrast)
    agree = (np.abs(blur(e) - blur(r)) <= allowed[..., None]).all(axis=2)
    failing = (excess > 0) & ~(judged & agree)
    return failing, failing & at_edge


def main(argv):
    """Compare two images and print what lies outside the tolerance."""
    text = "--text" in argv
    argv = [a for a in argv if a != "--text"]
    diff_out = None
    if "--diff" in argv:
        i = argv.index("--diff")
        diff_out = argv[i + 1]
        argv = argv[:i] + argv[i + 2:]
    expected_path, rendering_path = argv[1], argv[2]
    result = {"expected": expected_path, "rendering": rendering_path, "tolerance": "Text" if text else "Default"}
    try:
        e, r = load(expected_path), load(rendering_path)
    except FileNotFoundError as missing:
        # A document left unrendered, such as one too large for the machine, is reported, not compared.
        result.update({"error": "missing: " + str(missing.filename)})
        print(json.dumps(result))
        return 1
    if e.shape != r.shape:
        result.update({"error": "sizes differ", "expected_size": list(e.shape[:2]), "rendering_size": list(r.shape[:2])})
        print(json.dumps(result))
        return 1
    failing, at_edge = outside_tolerance(e, r, 80 if text else 40)
    difference = np.abs(e - r).max(axis=2)
    pixels = difference.size
    result.update({
        "width": int(e.shape[1]), "height": int(e.shape[0]),
        "outside": int(failing.sum()), "outside_at_edge": int(at_edge.sum()),
        "outside_share": round(float(failing.sum()) / pixels, 6),
        "max_difference": int(difference.max()),
        "over8": round(float((difference > 8).sum()) / pixels, 5), "over32": round(float((difference > 32).sum()) / pixels, 5),
        "over64": round(float((difference > 64).sum()) / pixels, 5),
        "mean_difference": round(float(np.abs(e - r).mean()), 3),
    })
    if diff_out:
        ghost = (255 - (255 - e) * 0.25).astype(np.uint8)
        ghost[failing] = [255, 0, 0]
        Image.fromarray(ghost).save(diff_out)
    print(json.dumps(result))
    return 0


if __name__ == "__main__":
    sys.exit(main(sys.argv))
