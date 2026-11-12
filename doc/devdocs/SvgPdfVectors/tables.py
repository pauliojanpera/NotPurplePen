#!/usr/bin/env python3
"""tables.py <run-all out folder>

Prints, from what run-all.sh measured (measurements.jsonl and summary.tsv), the tables of the design's section "A PDF
base map as vectors": for each PDF, each way's SVG size plain and gzip -9 compressed; the seconds each took; whether
each way's bytes repeated; the pixels of each browser's rendering outside F-1's tolerance of the application's
drawing, with their share of the image; and the application's own export of the test events. Run with python3 -I.
"""
import collections
import csv
import json
import os
import sys

SIZE_WAYS = ["bitmap", "vector", "cairo", "mupdf"]
TOLERANCE_WAYS = ["bitmap", "vector", "vector-darken", "vector-pdfium", "vector-rgb", "cairo", "mupdf"]
BROWSERS = ["chromium", "firefox", "webkit"]


def short(path):
    """The name a PDF or event goes by in the tables: its file name without the extension, as run-all.sh names it."""
    return os.path.basename(path).rsplit(".", 1)[0].replace(" ", "_").replace(".", "_")


def megabytes(n):
    """Bytes as megabytes, with two decimals below 10 and one above."""
    return ("%.2f MB" if n < 10e6 else "%.1f MB") % (n / 1e6)


def main(folder):
    """Print the tables."""
    rows = [json.loads(line) for line in open(os.path.join(folder, "measurements.jsonl")) if line.startswith("{")]
    seconds = collections.defaultdict(dict)
    repeats = collections.defaultdict(dict)
    exports = []
    not_rendered = []
    for row in rows:
        command = row.get("command")
        name = short(row.get("pdf", row.get("svg", row.get("event", ""))))
        if command == "bitmap-svg":
            seconds[name]["bitmap"] = "%.1f s, and %.1f s converting at loading" % (row["write_seconds"], row["conversion_seconds"])
        elif command == "vector-svg" and "variant" not in row:
            seconds[name]["vector"] = "%.1f s" % row["seconds"]
        elif command == "repeat":
            repeats[name][row["way"]] = row["identical_runs"]
        elif command == "export":
            exports.append(row)
        elif command == "not-rendered":
            not_rendered.append((name, row["way"], row["bytes"]))
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

    print("Sizes, plain and gzip -9 compressed:\n")
    print("| PDF | " + " | ".join(SIZE_WAYS) + " |")
    print("| --- |" + " --- |" * len(SIZE_WAYS))
    for pdf in sizes:
        cells = ["%s, %s" % (megabytes(sizes[pdf][way][0]), megabytes(sizes[pdf][way][1])) if way in sizes[pdf] else ""
                 for way in SIZE_WAYS]
        print("| %s | %s |" % (pdf, " | ".join(cells)))
    print("\nSeconds to write:\n")
    print("| PDF | " + " | ".join(SIZE_WAYS) + " |")
    print("| --- |" + " --- |" * len(SIZE_WAYS))
    for pdf in sizes:
        print("| %s | %s |" % (pdf, " | ".join(seconds[pdf].get(way, "") for way in SIZE_WAYS)))
    print("\nBytes repeated:", dict(repeats))
    print("\nNot rendered, being too large:", not_rendered)
    print("\nPixels outside F-1's tolerance of the application's drawing, Chromium, Firefox, WebKit:\n")
    print("| PDF | " + " | ".join(TOLERANCE_WAYS) + " |")
    print("| --- |" + " --- |" * len(TOLERANCE_WAYS))
    for pdf in sizes:
        cells = []
        for way in TOLERANCE_WAYS:
            values = outside.get((pdf, way), {})
            cells.append(", ".join("%.2f %%" % (100 * values[b][1]) if b in values else "-" for b in BROWSERS))
        print("| %s | %s |" % (pdf, " | ".join(cells)))
    print("\nThe application's own export:\n")
    for row in exports:
        print("| %s | %d files | %s | largest %s | %.1f s |" % (short(row["event"]), row["files"], megabytes(row["bytes"]),
                                                             megabytes(row["largest"]), row["export_seconds"]))


if __name__ == "__main__":
    main(sys.argv[1])
