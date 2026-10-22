"""Collect, for each drawn case's real rendering and for copies with errors planted in them, what any edge rule needs:
the pixels outside the band with no edge within reach (which fail under every rule), and for each pixel outside the
band with an edge within reach, the blurred difference and the contrast around it, per channel.

Usage: python3 -I collect.py <data folder> <out folder> [worker index] [worker count]
Writes <out folder>/<case>.npz holding, for each variant (real, shift2, removed, redder, wider), the arrays
<variant>_inside (count), <variant>_difference (N x 3) and <variant>_contrast9 (N x 3), <variant>_contrast5 (N x 3).
A variant whose planted image equals the rendering is left out.
"""
import os
import sys
import numpy as np
from PIL import Image
from scipy import ndimage

SLACK = 4


def load(path):
    return np.asarray(Image.open(path).convert("RGB")).astype(np.float64)


def window(image, size, highest):
    filter_ = ndimage.maximum_filter if highest else ndimage.minimum_filter
    return filter_(image, size=(size, size, 1), mode="constant", cval=255)


def blur(image):
    return ndimage.gaussian_filter(image, sigma=(1, 1, 0), mode="constant", cval=255)


def contrast(e, r, size):
    """The range each channel takes within a square window, in whichever image has the wider one."""
    return np.maximum(window(e, size, True) - window(e, size, False), window(r, size, True) - window(r, size, False))


def plant_errors(e, r):
    """Copies of Chromium's image with errors planted in them, as the earlier harness planted them."""
    ink = (255 - e).sum(axis=2)
    labels, count = ndimage.label(ink > 0)
    errors = {"shift2": np.roll(r, 2, axis=1)}
    if count:
        sizes = ndimage.sum(ink > 0, labels, np.arange(1, count + 1))
        solid = ndimage.maximum(ink, labels, np.arange(1, count + 1)) >= 300
        candidates = np.where((sizes >= 6) & solid, sizes, 1e9)
        if candidates.min() < 1e9:
            smallest = int(np.argmin(candidates)) + 1
            missing = r.copy()
            missing[ndimage.binary_dilation(labels == smallest, iterations=2)] = 255
            errors["removed"] = missing
    recolored = r.copy()
    interior = ndimage.binary_erosion(ink > 0, iterations=2)
    recolored[interior] = np.clip(recolored[interior] + np.array([6, 0, 0]), 0, 255)
    errors["redder"] = recolored
    errors["wider"] = ndimage.grey_erosion(r, size=(5, 5, 1))
    return {label: image for label, image in errors.items() if not np.array_equal(image, r)}


def measure(e, r):
    outside = np.zeros(e.shape[:2], dtype=bool)
    for one, other in ((e, r), (r, e)):
        low, high = window(one, 3, False) - SLACK, window(one, 3, True) + SLACK
        outside |= ((other < low) | (other > high)).any(axis=2)
    at_edge = (contrast(e, r, 3) > SLACK).any(axis=2)
    rescue = outside & at_edge
    difference = np.abs(blur(e) - blur(r))[rescue].astype(np.float32)
    return int((outside & ~at_edge).sum()), difference, contrast(e, r, 9)[rescue].astype(np.float32), contrast(e, r, 5)[rescue].astype(np.float32)


def main():
    data, out = sys.argv[1], sys.argv[2]
    worker, workers = (int(sys.argv[3]), int(sys.argv[4])) if len(sys.argv) > 4 else (0, 1)
    os.makedirs(out, exist_ok=True)
    names = sorted(name[:-4] for name in os.listdir(os.path.join(data, "references")))
    for index, name in enumerate(names):
        if index % workers != worker or os.path.exists(os.path.join(out, name + ".npz")):
            continue
        e = load(os.path.join(data, "references", name + ".png"))
        r = load(os.path.join(data, "renderings", name + ".png"))
        arrays = {}
        variants = {"real": r}
        variants.update(plant_errors(e, r))
        for label, image in variants.items():
            inside, difference, contrast9, contrast5 = measure(e, image)
            arrays[label + "_inside"] = np.array(inside)
            arrays[label + "_difference"] = difference
            arrays[label + "_contrast9"] = contrast9
            arrays[label + "_contrast5"] = contrast5
        np.savez_compressed(os.path.join(out, name + ".npz"), **arrays)
        print(name, flush=True)


if __name__ == "__main__":
    main()
