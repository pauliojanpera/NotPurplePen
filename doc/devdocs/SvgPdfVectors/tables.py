#!/usr/bin/env python3
"""tables.py <run-all out folder>

Prints, from what run-all.sh measured (measurements.jsonl and summary.tsv), the tables of the design's section "A PDF
base map as vectors": for each PDF, the size of each way's SVG plain and gzip -9 compressed and the seconds it took;
whether each way's bytes repeated; and the pixels of each browser's rendering outside F-1's tolerance of the
application's drawing, with their share of the image. Run with python3 -I.
"""
import collections
import csv
import json
import os
import sys

WAYS = ["bitmap", "vector", "vector-darken", "cairo", "mupdf"]
BROWSERS = ["chromium", "firefox", "webkit"]


def short(pdf):
    """The name a PDF goes by in the tables: its file name without the extension."""
    return os.path.basename(pdf).rsplit(".", 1)[0]


def megabytes(n):
    """Bytes as megabytes with two decimals."""
    return "%.2f MB" % (n / 1e6)


def main(folder):
    """Print the tables."""
    rows = [json.loads(line) for line in open(os.path.join(folder, "measurements.jsonl")) if line.startswith("{")]
    seconds = collections.defaultdict(dict)
    repeats = collections.defaultdict(dict)
    for row in rows:
        name = short(row.get("pdf", row.get("svg", "")))
        if row.get("command") == "bitmap-svg":
            seconds[name]["bitmap"] = "%.1f s (+ %.1f s converting)" % (row["write_seconds"], row["conversion_seconds"])
        elif row.get("command") == "vector-svg":
            # The first is the plain vector way's; the second, with darkened overprinting, is left out.
            seconds[name].setdefault("vector", "%.1f s" % row["seconds"])
        elif row.get("command") == "repeat":
            repeats[name][row["way"]] = row["identical_runs"]
        elif "converter" in row:
            seconds[name][row["converter"]] = "%.1f s" % row["seconds"]
            repeats[name][row["converter"]] = row["identical_runs"]

    sizes = collections.defaultdict(dict)
    outside = collections.defaultdict(dict)
    with open(os.path.join(folder, "summary.tsv")) as f:
        for row in csv.DictReader(f, delimiter="\t"):
            pdf, way, browser = row["pdf"], row["way"], row["browser"]
            sizes[pdf][way] = (int(row["bytes"]), int(row["gzip_bytes"]))
            if row["outside"] not in ("None", ""):
                outside[(pdf, way)][browser] = (int(row["outside"]), float(row["outside_share"]))

    print("| PDF | " + " | ".join(WAYS) + " |")
    print("| --- |" + " --- |" * len(WAYS))
    for pdf in sizes:
        cells = []
        for way in WAYS:
            if way in sizes[pdf]:
                plain, packed = sizes[pdf][way]
                cells.append("%s, %s gzipped" % (megabytes(plain), megabytes(packed)))
            else:
                cells.append("")
        print("| %s | %s |" % (pdf, " | ".join(cells)))
    print()
    print("| PDF | " + " | ".join(["bitmap", "vector", "cairo", "mupdf"]) + " |")
    print("| --- |" + " --- |" * 4)
    for pdf in sizes:
        key = next((k for k in seconds if k.replace(" ", "_").replace(".", "_") == pdf), pdf)
        print("| %s | %s |" % (pdf, " | ".join(seconds[key].get(way, "") for way in ["bitmap", "vector", "cairo", "mupdf"])))
    print()
    print("Bytes repeated:", {pdf: values for pdf, values in repeats.items()})
    print()
    print("| PDF | Way | " + " | ".join(BROWSERS) + " |")
    print("| --- | --- |" + " --- |" * len(BROWSERS))
    for (pdf, way), values in outside.items():
        cells = ["%d (%.2f %%)" % (values[b][0], 100 * values[b][1]) if b in values else "" for b in BROWSERS]
        print("| %s | %s | %s |" % (pdf, way, " | ".join(cells)))


if __name__ == "__main__":
    main(sys.argv[1])
