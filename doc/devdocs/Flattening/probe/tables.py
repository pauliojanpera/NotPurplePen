#!/usr/bin/env python3
"""tables.py <results>

Prints the tables of doc/devdocs/Flattening/Measurements.md as Markdown, from the files that run-all.sh wrote to
<results>. Each table names the files that it reads.
"""
import csv
import glob
import os
import sys


def read_table(path):
    """Gives the rows of a file of tab-separated values, as dictionaries. A file that is not there has no rows."""
    if not os.path.exists(path):
        return []
    with open(path, newline="") as file:
        return list(csv.DictReader(file, delimiter="\t", quoting=csv.QUOTE_NONE))


def read_tables(results, name):
    """Gives the rows of the table of one name of each map, in the order of the folders of the maps."""
    rows = []
    for path in sorted(glob.glob(os.path.join(results, "*", name))):
        rows.extend(read_table(path))
    return rows


def number(text, decimals=1):
    """Gives a number of a table with a set count of decimals. An empty field stays empty."""
    if text is None or text == "":
        return ""
    value = float(text)
    return f"{value:,.{decimals}f}".replace(",", " ") if decimals else f"{int(round(value)):,}".replace(",", " ")


def count(text):
    """Gives a count with spaces between the thousands."""
    return number(text, 0)


def print_table(title, source, header, rows):
    """Prints one Markdown table with its title and the files that it comes from."""
    print(f"### {title}\n")
    print(f"From `{source}`.\n")
    print("| " + " | ".join(header) + " |")
    print("| " + " | ".join("---" for _ in header) + " |")
    for row in rows:
        print("| " + " | ".join(str(cell) for cell in row) + " |")
    print()


def kinds(row):
    """Gives the counts of the segments of a row by kind: lines / quadratics / conics / cubics."""
    return " / ".join(count(row[kind]) for kind in ("lines", "quadratics", "conics", "cubics"))


def sum_of(rows, column):
    """Gives the sum of a column over the rows that have a number in it."""
    return sum(float(row[column]) for row in rows if row.get(column) not in (None, ""))


def main(results):
    maps = read_tables(results, "q1-map.tsv")
    names = [row["map"] for row in maps]
    # A results folder that holds only some stages can have a map with no inventory. Such a map comes last.
    for path in sorted(glob.glob(os.path.join(results, "*", "stages.tsv"))):
        name = os.path.basename(os.path.dirname(path))
        if name not in names:
            names.append(name)

    # Question 1.
    print_table("Question 1: what each map holds", "<map>/q1-map.tsv",
                ["Map", "Size, mm", "Colours of the table (overprinted)", "Runs (inside a blending)", "Marks", "Contours", "Segments", "Lines / quadratics / conics / cubics", "Load, s", "Draw, s"],
                [[row["map"], f'{number(row["width_mm"], 0)} x {number(row["height_mm"], 0)}', f'{row["colours_of_table"]} ({row["overprinted_colours_of_table"]})',
                  f'{row["runs"]} ({row["runs_inside_blending"]})', count(row["marks"]), count(row["contours"]), count(row["segments"]), kinds(row), number(row["load_seconds"]), number(row["draw_seconds"])]
                 for row in maps])
    print_table("Question 1: what the target met and did not keep", "<map>/q1-map.tsv",
                ["Map", "Text calls", "Bitmap calls", "Pattern brushes", "Clips", "Switches of antialiasing (pushes)", "Blendings", "Marks cut by a clip", "Marks outside their clip", "Marks that a clip made empty or that were empty",
                 "Cuts by a clip with no result", "Time of the cuts by a clip, s"],
                [[row["map"], count(row["text_calls"]), row["bitmap_calls"], row["pattern_brushes"], count(row["clips"]), f'{row["antialiasing_switches"]} ({row["antialiasing_pushes"]})', row["blending_pushes"],
                  count(row["marks_cut_by_clip"]), count(row["marks_outside_clip"]), count(row["empty_marks"]), row["mark_clip_failures"], number(row["clip_seconds"])] for row in maps])
    print_table("Question 1: the runs", "<map>/q1-runs.tsv",
                ["Map", "Run", "Colour", "C, M, Y, K", "Inside a blending", "Marks", "Segments", "Lines / quadratics / conics / cubics"],
                [[row["map"], row["run"], row["colour"], ", ".join(f'{float(row[c]):.3g}' for c in ("cyan", "magenta", "yellow", "black")) + ("" if row["alpha"] == "1" else f', alpha {row["alpha"]}'),
                  row["inside_blending"], count(row["marks"]), count(row["segments"]), kinds(row)] for row in read_tables(results, "q1-runs.tsv")])

    # Question 2.
    unions = read_tables(results, "q2-union.tsv") + read_tables(results, "q2-union-builder.tsv")
    summary = []
    for name in names:
        for way in ("OneAfterTheOther", "ByPairs", "Builder"):
            rows = [row for row in unions if row["map"] == name and row["way"] == way]
            measured = [row for row in rows if row["gave_result"] != "not measured"]
            done = [row for row in measured if row["gave_result"] == "yes"]
            summary.append([name, way, len(rows), len(measured), len(done), sum(1 for row in measured if row["stopped_by_limit"] == "yes"), count(str(sum_of(measured, "failed_operations"))),
                            number(str(sum_of(measured, "seconds"))), count(str(max([float(row["segments_in"]) for row in done] or [0]))),
                            count(str(sum_of(done, "differing_samples"))), count(str(sum_of(done, "thick_differing_samples"))), count(str(max([float(row["largest_group"] or 0) for row in done] or [0]))),
                            f'{sum(1 for row in measured if row["equal_when_repeated"] == "yes")} of {sum(1 for row in measured if row["equal_when_repeated"] != "")}'])
    print_table("Question 2: the union of each run, the totals of each way", "<map>/q2-union.tsv and <map>/q2-union-builder.tsv",
                ["Map", "Way", "Runs", "Runs measured", "Gave a result", "Stopped by the limit", "Operations with no result", "Time, s", "Largest run with a result, segments", "Differing samples", "Thick differing samples",
                 "Largest group", "Equal when made again"], summary)
    by_run = {}
    for row in unions:
        by_run.setdefault((row["map"], int(row["run"])), {})[row["way"]] = row

    def cell(row):
        if row is None:
            return "no row"
        if row["gave_result"] == "not measured":
            return "not measured"
        if row["stopped_by_limit"] == "yes":
            return f'stopped at {number(row["seconds"])} s'
        text = f'{number(row["seconds"], 3)} s' if row["gave_result"] == "yes" else f'no result in {number(row["seconds"], 3)} s'
        return text + (f', {count(row["differing_samples"])} ({count(row["thick_differing_samples"])})' if row["differing_samples"] != "" else "")

    print_table("Question 2: the union of each run. Each cell gives the time, and then the samples of the window that differ (thick ones in brackets)", "<map>/q2-union.tsv and <map>/q2-union-builder.tsv",
                ["Map", "Run", "Colour", "Marks", "Segments in", "One after the other", "By pairs", "Builder", "By pairs: contours, segments out"],
                [[key[0], key[1], ways.get("ByPairs", ways.get("Builder", {})).get("colour", ""), count(next(iter(ways.values()))["marks"]), count(next(iter(ways.values()))["segments_in"]),
                  cell(ways.get("OneAfterTheOther")), cell(ways.get("ByPairs")), cell(ways.get("Builder")),
                  (f'{count(ways["ByPairs"]["contours"])}, {count(ways["ByPairs"]["segments"])}' if ways.get("ByPairs", {}).get("gave_result") == "yes" else "")]
                 for key, ways in sorted(by_run.items(), key=lambda item: (names.index(item[0][0]), item[0][1]))])

    # Question 3.
    stages = []
    for path in sorted(glob.glob(os.path.join(results, "*", "stages.tsv"))):
        # A stage that ran more than one time has more than one line. The last line is the run that the tables come from.
        last = {}
        with open(path) as file:
            for line in file:
                if line.strip():
                    fields = line.rstrip("\n").split("\t")
                    last[fields[1]] = fields
        stages.extend(last.values())
    print_table("The stages of each map: 124 as the exit code says that the time limit stopped the stage", "<map>/stages.tsv",
                ["Map", "Stage", "Exit code", "Wall-clock seconds"], stages)
    totals = read_tables(results, "q3-knockout.tsv")
    print_table("Question 3: the knockout over the whole map, the parts that ended", "<map>/q3-knockout.tsv",
                ["Map", "Part", "Runs with no union", "Operations", "With no result", "Stopped by the limit", "Time, s", "Longest operation, s", "Pieces", "Contours", "Segments", "Samples that two pieces cover",
                 "Samples of marks with no piece", "Samples of a piece with no mark"],
                [[row["map"], row["part"], row["runs_without_union"], count(row["operations"]), row["failed_operations"], row["stopped_by_limit"], number(row["seconds"]), number(row["longest_operation_seconds"]), count(row["pieces"]),
                  count(row["contours"]), count(row["segments"]), count(row["samples_covered_by_two_pieces"]), count(row["samples_of_marks_without_piece"]), count(row["samples_of_piece_without_mark"])] for row in totals])
    operations = read_tables(results, "q3-operations.tsv")
    rows = []
    for name in names:
        for part in ("each run a knockout colour", "overprinted runs as they are"):
            ours = [row for row in operations if row["map"] == name and row["part"] == part]
            if not ours:
                rows.append([name, part, 0, "", "", "", "", ""])
                continue
            slowest = max(ours, key=lambda row: float(row["seconds"]))
            rows.append([name, part, len(ours), sum(1 for row in ours if row["gave_result"] == "no"), number(str(sum_of(ours, "seconds"))),
                         f'{number(slowest["seconds"])} s: {slowest["operation"]} of {count(slowest["left_verbs"])} and {count(slowest["right_verbs"])} verbs',
                         number(str(sorted(float(row["seconds"]) for row in ours)[len(ours) // 2]), 4),
                         f'{sum(1 for row in ours if float(row["seconds"]) > 1)}'])
    print_table("Question 3: the operations that ended, also of the parts that the limit stopped", "<map>/q3-operations.tsv",
                ["Map", "Part", "Operations that ended", "With no result", "Their time, s", "The longest", "Median, s", "Longer than 1 s"], rows)
    running = []
    for path in sorted(glob.glob(os.path.join(results, "*", "q3-running-operation.tsv"))):
        with open(path) as file:
            fields = file.read().strip().split("\t")
        running.append([fields[0], fields[1], fields[2], fields[3], count(fields[4]), count(fields[5])])
    print_table("Question 3: the operation that ran when the time limit stopped the stage", "<map>/q3-running-operation.tsv",
                ["Map", "Part", "Run", "Operation", "Verbs of the left path", "Verbs of the right path"], running)

    # Question 4.
    print_table("Question 4: edges that two areas share", "q4-shared-edges.tsv",
                ["Shapes", "Operation", "Result", "Contours", "Contours below 0.000001 mm2", "Lines / quadratics / conics / cubics", "Area, mm2", "Exact area, mm2", "Error of the area, mm2",
                 "Paper samples at 40", "Extra samples at 40", "Paper samples at 400", "Extra samples at 400"],
                [list(row.values()) for row in read_table(os.path.join(results, "q4-shared-edges.tsv"))])

    # Question 5.
    first = {}
    second = {}
    for target, path in ((first, os.path.join(results, "digests.txt")), (second, os.path.join(results, "second-process", "digests.txt"))):
        if os.path.exists(path):
            with open(path) as file:
                for line in file:
                    fields = line.rstrip("\n").split("\t")
                    target[tuple(fields[:-1])] = fields[-1]
    passes = {row["map"]: row for row in read_tables(results, "q5-two-passes.tsv")}
    print_table("Question 5: the same result two times", "<map>/q5-two-passes.tsv, digests.txt and second-process/digests.txt",
                ["Map", "Runs", "Runs of at most 10 000 segments", "Results", "Results with no path", "Equal the second time in one process", "Equal in the second process", "Time of one pass, s"],
                [[name, passes[name]["runs"], passes[name]["runs_with_few_segments"], passes[name]["results"], passes[name]["results_without_digest"], passes[name]["equal_in_the_second_pass"],
                  f'{sum(1 for key, digest in first.items() if key[0] == name and second.get(key) == digest)} of {sum(1 for key in first if key[0] == name)}', number(passes[name]["seconds_of_one_pass"])]
                 for name in names if name in passes])

    # Question 6.
    tile_header = ["Map", "Tile, mm", "Tiles (with marks)", "Stopped by the limit", "Cut, s", "Union, s", "Knockout, s", "All, s", "Cuts (no result)", "Unions with no result", "Knockout operations (no result)",
                   "Longest knockout operation, s", "Pieces", "Segments", "Paper samples in the window at 40", "On the borders at 40", "On the borders at 400 (of samples)"]

    def tile_rows(name, folder=results):
        return [[row["map"], row["tile_mm"], f'{count(row["tiles"])} ({count(row["tiles_with_marks"])})', row["stopped_by_limit"], number(row["cut_seconds"]), number(row["union_seconds"]), number(row["knockout_seconds"]),
                 number(row["total_seconds"]), f'{count(row["cut_operations"])} ({row["failed_cuts"]})', row["failed_unions"], f'{count(row["knockout_operations"])} ({row["failed_knockout_operations"]})',
                 number(row["longest_operation_seconds"], 3), count(row["pieces"]), count(row["segments"]), count(row["paper_samples_at_40"]), count(row["paper_samples_on_borders_at_40"]),
                 (f'{count(row["paper_samples_on_borders_at_400"])} (of {count(row["border_samples_at_400"])})' if row["border_samples_at_400"] else "")] for row in read_tables(folder, name)]

    print_table("Question 6: tiles. A run that a difference could not cut has no piece in its tile", "<map>/q6-tiles.tsv", tile_header, tile_rows("q6-tiles.tsv"))
    print_table("Question 6: tiles of the second run, in a process of its own", "tiles-second-run/<map>/q6-tiles.tsv", tile_header, tile_rows("q6-tiles.tsv", os.path.join(results, "tiles-second-run")))
    print_table("Question 6: tiles of 10 mm with a limit of 900 s", "tiles-long/<map>/q6-tiles.tsv", tile_header, tile_rows("q6-tiles.tsv", os.path.join(results, "tiles-long")))
    print_table("Question 6: tiles. A run that a difference could not cut stays as it is, not cut", "<map>/q6-tiles-uncut.tsv", tile_header, tile_rows("q6-tiles-uncut.tsv"))
    rows = []
    for source in ("", "tiles-second-run", "tiles-long"):
        for row in read_tables(os.path.join(results, source), "q6-tiles-knockout.tsv"):
            rows.append([row["map"], row["part"], source or "tiles", count(row["operations"]), row["failed_operations"], row["stopped_by_limit"], number(row["seconds"]), number(row["longest_operation_seconds"], 3), count(row["pieces"]),
                         count(row["contours"]), count(row["segments"]), count(row["samples_covered_by_two_pieces"]), count(row["samples_of_marks_without_piece"]), count(row["samples_of_piece_without_mark"])])
    print_table("Question 6: the two cuts of question 3 in the tiles, with the samples of the window", "<map>/q6-tiles-knockout.tsv, and the same file under tiles-second-run/ and tiles-long/",
                ["Map", "Part", "Stage", "Operations", "With no result", "Stopped by the limit", "Time, s", "Longest operation, s", "Pieces", "Contours", "Segments", "Samples that two pieces cover",
                 "Samples of marks with no piece", "Samples of a piece with no mark"], rows)
    operations = read_tables(results, "q6-tiles-operations.tsv") or read_tables(os.path.join(results, "tiles-second-run"), "q6-tiles-operations.tsv")
    totals = {}
    for row in operations:
        key = (row["map"], row["tile_mm"], row["operation"])
        total = totals.setdefault(key, [0, 0, 0])
        total[0] += int(row["operations"])
        total[1] += int(row["with_no_result"])
        total[2] += int(row.get("wrong_results") or 0)
    print_table("Question 6: the operations in the tiles, each result checked against the samples of its operands", "<map>/q6-tiles-operations.tsv, or tiles-second-run/<map>/q6-tiles-operations.tsv",
                ["Map", "Tile, mm", "Operation", "Operations", "With no result", "With a wrong result"], [[key[0], key[1], key[2], count(str(total[0])), total[1], total[2]] for key, total in totals.items()])
    print_table("Question 6: the operations in the tiles that gave no result or a wrong result, by run", "<map>/q6-tiles-operations.tsv, or tiles-second-run/<map>/q6-tiles-operations.tsv",
                ["Map", "Tile, mm", "Run", "Colour", "Inside a blending", "Operation", "Operations", "With no result", "With a wrong result"],
                [[row["map"], row["tile_mm"], row["run"], row["colour"], row["inside_blending"], row["operation"], count(row["operations"]), row["with_no_result"], row.get("wrong_results") or 0]
                 for row in operations if row["with_no_result"] != "0" or (row.get("wrong_results") or "0") != "0"])
    again = {(row["map"], row["tile_mm"]): row for row in read_tables(os.path.join(results, "tiles-second-run"), "q6-tiles.tsv")}
    rows = []
    for row in read_tables(results, "q6-tiles.tsv"):
        other = again.get((row["map"], row["tile_mm"]))
        if other and row["digest_of_pieces"] and other["digest_of_pieces"]:
            rows.append([row["map"], row["tile_mm"], count(row["pieces"]), "yes" if row["digest_of_pieces"] == other["digest_of_pieces"] else "no", number(row["total_seconds"]), number(other["total_seconds"])])
    print_table("Questions 5 and 6: the pieces of the tiles, made again in a second process", "<map>/q6-tiles.tsv and tiles-second-run/<map>/q6-tiles.tsv",
                ["Map", "Tile, mm", "Pieces", "The digest of all pieces is equal", "Time of the first run, s", "Time of the second run, s"], rows)

    # Question 7.
    print_table("Question 7: curves", "q7-curves.tsv",
                ["Shapes", "Operation", "In: lines / quadratics / conics / cubics", "Result", "Contours out", "Out: lines / quadratics / conics / cubics", "Largest distance, mm"],
                [list(row.values()) for row in read_table(os.path.join(results, "q7-curves.tsv"))])

    # Question 8.
    pieces = []
    for path in sorted(glob.glob(os.path.join(results, "*", "q8-pieces-*.tsv"))):
        pieces.extend(read_table(path))
    print_table("Question 8: the pieces against the drawing, over the window", "<map>/q8-pieces-<source>.tsv",
                ["Map", "Pieces of", "Window: left, top, width, height, mm", "Samples", "Samples with ink", "Differing samples", "Thick differing samples", "Largest group"],
                [[row["map"], row["pieces_of"], ", ".join(number(row[c], 1) for c in ("window_left_mm", "window_top_mm", "window_width_mm", "window_height_mm")), count(row["samples"]), count(row["samples_with_ink"]),
                  count(row["differing_samples"]), count(row["thick_differing_samples"]), count(row["largest_group"])] for row in pieces])

    # Question 9.
    print_table("Question 9: the time of the loop", "q9-loop-time.tsv", ["Step", "Command", "Wall-clock seconds", "Exit code"],
                [list(row.values()) for row in read_table(os.path.join(results, "q9-loop-time.tsv"))])


if __name__ == "__main__":
    if len(sys.argv) != 2:
        sys.exit("usage: tables.py <results>")
    main(sys.argv[1])
