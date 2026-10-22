"""Make one step of the SVG gallery from a copy of the SVG loop's results.

Usage: python3 -I make-gallery-step.py <results copy> <reference folder> <gallery folder> <step id> <case,case,...>

<results copy> is a checkout of svg-results/<name>, holding Chromium's image of each case named under renderings/:
the loop's own for a case outside the tolerance, else one render-cases.mjs made. A case's Skia reference is the
run's own under references/ when its reference was not recorded yet, else the recorded one in <reference folder>,
the svg_reference of the commit the loop ran. For each case named, writes into <gallery folder>/<step id>/ the
case's SVG, its Skia reference and its Chromium rendering as RGB PNGs, and a difference image: the reference
ghosted to a pale grey, with every pixel whose largest channel difference exceeds 0 painted in one of four blues,
binned at the thresholds the loop's measurements count (8, 32, 64). The page's BINS hold the same blues.

Prints the step's figures as JSON for the page's LOG and STANDING: per case its size, measurements, drawing
elements and the 24-pixel window holding the most difference, where the detail opens; and the pending census,
each call with the drawings that need it, those that need only it, and the work that adds it.
"""
import json
import os
import re
import shutil
import sys

import numpy as np
from PIL import Image

# Difference bins (exclusive lower bound, inclusive upper) and their blues, light to dark.
BINS = [(0, 8, (0x86, 0xb6, 0xef)), (8, 32, (0x39, 0x87, 0xe5)),
        (32, 64, (0x1c, 0x5c, 0xab)), (64, 255, (0x0d, 0x36, 0x6b))]
GHOST_INK = 0.16      # how much of the reference's darkness the ghost keeps
WINDOW = 24           # side of the detail window, in image pixels

# The work that adds each call, by the requirements' plan: drawing in phase 2's step 2, text and bitmaps in
# phase 4.
WORK_THAT_ADDS_CALL = {
    "CreatePen": "step2", "DrawPolyline": "step2", "DrawPath": "step2", "FillPath": "step2",
    "CreatePath": "step2", "PushClip": "step2", "PopClip": "step2", "DrawLine": "step2",
    "DrawEllipse": "step2", "DrawPolygon": "step2", "CreatePatternBrush": "step2", "DrawArc": "step2",
    "PushBlending": "step2", "PopBlending": "step2", "DrawRectangle": "step2",
    "CreateFont": "phase4", "DrawText": "phase4", "DrawTextOutline": "phase4", "DrawBitmap": "phase4",
}


def read_rgb(path):
    """Read a PNG as an RGB array, compositing any alpha onto white."""
    rgba = np.asarray(Image.open(path).convert("RGBA")).astype(np.float64)
    alpha = rgba[..., 3:4] / 255.0
    return np.rint(rgba[..., :3] * alpha + 255.0 * (1.0 - alpha)).astype(np.uint8)


def make_difference_image(reference, rendering):
    """Ghost the reference and paint each differing pixel in the blue of its bin."""
    difference = np.abs(reference.astype(np.int16) - rendering.astype(np.int16)).max(axis=2)
    luminance = reference.astype(np.float64) @ np.array([0.2126, 0.7152, 0.0722])
    ghost = 255.0 - (255.0 - luminance) * GHOST_INK
    picture = np.repeat(ghost[..., None], 3, axis=2)
    for low, high, color in BINS:
        picture[(difference > low) & (difference <= high)] = color
    return np.rint(picture).astype(np.uint8), difference


def find_window_of_most_difference(difference):
    """Return the left and top of the WINDOW-square window whose differences sum highest."""
    summed = np.pad(difference.astype(np.int64), ((1, 0), (1, 0))).cumsum(0).cumsum(1)
    sums = (summed[WINDOW:, WINDOW:] - summed[:-WINDOW, WINDOW:]
            - summed[WINDOW:, :-WINDOW] + summed[:-WINDOW, :-WINDOW])
    top, left = np.unravel_index(int(sums.argmax()), sums.shape)
    return int(left), int(top)


def count_drawing_elements(svg_text):
    """Count the drawing elements of an SVG by tag."""
    counts = {}
    for tag in re.findall(r"<(circle|ellipse|path|rect|polygon|polyline|line|text|image|use)\b", svg_text):
        counts[tag] = counts.get(tag, 0) + 1
    return counts


def read_measurements(path):
    """Read a case's measurement row as a dictionary of integers."""
    with open(path, encoding="utf-8") as file:
        header, values = [line.rstrip("\n").split("\t") for line in file.readlines()[:2]]
    return {name: int(value) for name, value in zip(header, values)}


def read_census(path):
    """Read pending.tsv into the calls each pending case still needs."""
    with open(path, encoding="utf-8") as file:
        rows = [line.rstrip("\r\n").split("\t") for line in file.readlines()[1:] if line.strip()]
    return {case: [call.strip() for call in calls.split(",")] for case, calls in rows}


def find_reference(results, reference_folder, name):
    """Return the path of a case's reference image: the run's own, else the recorded one."""
    run_reference = os.path.join(results, "references", name + ".png")
    return run_reference if os.path.exists(run_reference) else os.path.join(reference_folder, name + ".png")


def main():
    results, reference_folder, gallery, step, case_list = sys.argv[1:6]
    out = os.path.join(gallery, step)
    os.makedirs(out, exist_ok=True)
    cases = []
    for name in case_list.split(","):
        reference = read_rgb(find_reference(results, reference_folder, name))
        rendering = read_rgb(os.path.join(results, "renderings", name + ".png"))
        picture, difference = make_difference_image(reference, rendering)
        Image.fromarray(reference).save(os.path.join(out, name + "-skia.png"), optimize=True)
        Image.fromarray(rendering).save(os.path.join(out, name + "-chromium.png"), optimize=True)
        Image.fromarray(picture).save(os.path.join(out, name + "-difference.png"), optimize=True)
        svg_path = os.path.join(results, "svg", name + ".svg")
        shutil.copyfile(svg_path, os.path.join(out, name + ".svg"))
        with open(svg_path, encoding="utf-8") as file:
            svg_text = file.read()
        left, top = find_window_of_most_difference(difference)
        cases.append({
            "name": name,
            "width": int(reference.shape[1]), "height": int(reference.shape[0]),
            "window": {"left": left, "top": top, "size": WINDOW},
            "measurements": read_measurements(os.path.join(results, "measurements", name + ".tsv")),
            "elements": count_drawing_elements(svg_text),
            "svgBytes": len(svg_text.encode("utf-8")),
        })

    census = read_census(os.path.join(results, "pending.tsv"))
    calls = {}
    for case_calls in census.values():
        for call in case_calls:
            calls.setdefault(call, {"call": call, "cases": 0, "alone": 0, "comesIn": WORK_THAT_ADDS_CALL[call]})
            calls[call]["cases"] += 1
            if len(case_calls) == 1:
                calls[call]["alone"] += 1
    waiting_on_step2_only = sum(1 for case_calls in census.values()
                                if all(WORK_THAT_ADDS_CALL[call] == "step2" for call in case_calls))
    print(json.dumps({
        "cases": cases,
        "waiting": len(census),
        "waitingOnStep2Only": waiting_on_step2_only,
        "calls": sorted(calls.values(), key=lambda row: (-row["cases"], row["call"])),
    }, indent=1))


if __name__ == "__main__":
    main()
