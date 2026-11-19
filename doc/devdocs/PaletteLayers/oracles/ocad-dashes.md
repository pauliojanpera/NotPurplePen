# OCAD's dashed lines

Nov 18, 2026. OCAD is the authority on how its own maps render (the preface of [Specification.md](../Specification.md)).
@Pauli decided on Nov 18 to check the dashed lines against a large sample that the OCAD viewer exports as images of an
exact resolution, as the baselines of Purple Pen are probably wrong in some places (ticket
[011](../tickets/011-line-geometry.md)). This file records the samples, the exports and what they tell.

## How the samples are measured

- Purple Pen writes each sample with `InputOutput.WriteFile`, from a temporary test that is not committed. Each cell of
  a sample holds one line. A file `*-cells.json` gives the symbols and the path of each cell, in mm on the paper, and
  the four registration marks, which are filled circles of 0.6 mm at the corners.
- @Pauli exports each file from the OCAD viewer as a PNG image of 600 dpi without antialiasing. Purple Pen draws each
  file with Skia at 600 dpi without antialiasing (`purple-pen-*.png`), with the code of upstream.
- [measure-dashes.py](ocad-dashes/measure-dashes.py) finds the registration marks in a drawing. They give its scale,
  23.62 pixels to the mm in all the drawings, and its position. Along the path of each cell, the script counts the
  dashes: the runs of pixels darker than 128 at the middle of the line. It compares the count with the rule of Purple
  Pen (`LineSymDef.ComputeDashDistances`), with the metric of the file. In each drawing of Purple Pen, each count is
  the count of the rule.
- [measure-corners.py](ocad-dashes/measure-corners.py) measures the fifth and the sixth sample, whose cells turn at a
  vertex near a boundary of a dash. It tells whether the dash takes the corner of the miter at the vertex, and where
  the boundary is in the drawing (see the sixth sample).

## The first sample

Three files: [OCAD 12 with the OCAD metric](ocad-dashes/dashes-ocad12-ocad-metric.ocd) (the parameter `p1` in ViewPar),
[OCAD 12 with the Euclidean metric](ocad-dashes/dashes-ocad12-euclidean.ocd) (`p0`) and
[OCAD 9](ocad-dashes/dashes-ocad9.ocd), which has no `p`, as `OcadExport` writes it only to OCAD 11+. The cells are in
[dashes-cells.json](ocad-dashes/dashes-cells.json). Six line symbols, one in each row, go along nine paths, one in each
column. The rows are 15 mm apart, R1 at y = 0. The columns are 30 mm apart, the first at x = 2.

| Row | Symbol: a line of 0.3 mm with flat ends, dashes of 2.0 mm and gaps of 0.5 mm |
| --- | --- |
| R1 | end dashes 2.0 |
| R2 | end dashes 1.0 |
| R3 | as R1, with a secondary gap of 0.3 in each dash between the end dashes |
| R4 | as R1, with a secondary gap of 0.3 in each end dash |
| R5 | as R2, with at least 2 gaps |
| R6 | a double line without a main line: a fill of 0.6 and borders of 0.15, all dashed |

The columns, from the origin of the cell: a straight line of 25 mm, a line of 25 mm at 30°, the line (0, 0), (12, 0),
(20, 6) with a corner point at (12, 0), the same line with a plain vertex, the Bezier curve (0, 0), (8, 8), (16, −8),
(24, 0), a closed rectangle of 12 × 6 mm that starts at (0, 0), a line of 24 mm with a dash point at its middle, and
lines of 3.0 and 1.5 mm.

@Pauli exported the files on Nov 18:
[OCAD metric](ocad-dashes/ocad-viewer-dashes-ocad12-ocad-metric-600dpi.png),
[Euclidean](ocad-dashes/ocad-viewer-dashes-ocad12-euclidean-600dpi.png) and
[OCAD 9](ocad-dashes/ocad-viewer-dashes-ocad9-600dpi.png). The names of his files say OCAD Viewer 2022. On the same
drive, a file with such a name is the screenshot of the OCAD 8 circles in [ocad-thick-circles.md](ocad-thick-circles.md),
and its title bar shows OCAD 2020.5.18 Viewer. Thus the exports are probably from that viewer. Purple Pen drew the
files on the same grid of pixels: [OCAD metric](ocad-dashes/purple-pen-dashes-ocad12-ocad-metric-600dpi.png) and
[Euclidean](ocad-dashes/purple-pen-dashes-ocad12-euclidean-600dpi.png). It draws the OCAD 9 file as the file with the
OCAD metric.

### The values

The export of the OCAD 9 file is the same, pixel by pixel, as the export of the Euclidean OCAD 12 file.

The counts of dashes that are not the same in the export of OCAD and the drawing of Purple Pen. In all the other cells,
the counts are the same.

| File | Cells | OCAD | Purple Pen | The computation of Purple Pen |
| --- | --- | --- | --- | --- |
| OCAD metric | R2, 1.5 mm | 1 | 2 | 1.5 ≥ 1.0 / 2 + 1.0 / 2 + 0.5, so one gap |
| | R6, 1.5 mm | 1 | 2 | a double line has at least one gap |
| Euclidean | R1, R3, R4 and R6, the Bezier curve | 10, 18, 12, 10 | 11, 20, 13, 11 | (26.198 + 2.0 − 2 × 2.0) / 2.5 = 9.68, so 10 gaps |
| | R2 and R6, 1.5 mm | 1 | 2 | as above |
| OCAD 9 | 18 cells: the line at 30°, the corner point and the Bezier curve where the metrics give different counts, and R2 and R6 of 1.5 mm | as the Euclidean file | as the file with the OCAD metric | the OCAD metric |

Where the counts are the same, the dashes are at the same places:

- On the straight lines, the ends of the dashes are at most about one pixel (0.04 to 0.05 mm) apart.
- On the Bezier curve with the OCAD metric, the dashes agree in about 0.1 mm.
- On the Euclidean Bezier curve of R1, OCAD puts its 10 dashes along the full curve, each 2.10 to 2.27 mm long, with
  a mean gap of 0.505 mm. The line of OCAD is less than 0.02 mm from the curve.
- A line of 0.3 mm covers 7 pixels across in both. The double line covers 21 pixels across in OCAD and 22 in Purple
  Pen, whose lower edge is 0.03 pixel past the center of a pixel.
- On the closed rectangle of the double line, at the corners where a dash goes around the corner and at the start
  point, OCAD leaves open a triangle of about 5 pixels between the fill and the outer border. Purple Pen fills it.

### What the first sample tells

- With the OCAD metric, OCAD gives each line the same number of gaps as Purple Pen and puts the dashes at the same
  places, except on two lines of 1.5 mm. The OCAD metric of Purple Pen (`SymPath.BizzarroDistance`: the larger
  difference of the coordinates, plus half of the smaller) agrees with OCAD on the line at 30°, the corner point, the
  plain vertex and the Bezier curve. The secondary gaps, the secondary gaps of the end dashes, the minimum number of
  gaps, the corner points, the dash points and the closed line agree.
- The viewer draws an OCAD 9 file that has no `p` with the Euclidean metric. Purple Pen draws all OCAD 6 to 10 files
  with the OCAD metric, as `OcadImport` reads `p` only from OCAD 11+.
- A short line: the viewer draws a line of 1.5 mm with end dashes of 1.0 mm and gaps of 0.5 mm without a gap. That
  length is half of each end dash plus a gap, the length from which Purple Pen gives a short line one gap
  (`pathLength >= firstDashLength / 2 + lastDashLength / 2 + gapLength`).
- A double line: the viewer draws a double line of 1.5 mm with dashes of 2.0 mm and gaps of 0.5 mm without a gap.
  Purple Pen gives the dashes of a double line at least one gap (`OcadImport`: `doubleDashes.minGaps = 1`).
- The Euclidean Bezier curve: in the other cells, OCAD rounds the computed number of gaps up from a fraction of 0.6 or
  more (3.6 in the second part of the corner line with the OCAD metric, 10.65 on the Bezier curve with the OCAD metric,
  8.8 and 4.8 in R2). But it gives the Euclidean Bezier curve 9 gaps for 9.68. Thus OCAD does not count the gaps of a
  Euclidean Bezier curve from its length, but it puts the dashes along the full curve. The second sample looks for
  the length that OCAD uses.

## The second sample

The second sample examines the limits of the rules that the first sample found, and the metric of OCAD 9 and 10
files. Purple Pen wrote it on Nov 18. The cells are in [dashes-2-cells.json](ocad-dashes/dashes-2-cells.json): 124
cells in 11 rows, from the top. The symbols are lines of 0.3 mm with flat ends, dashes of 2.0 mm and gaps of 0.5 mm,
or double lines as R6.

| Row | Symbols | Cells |
| --- | --- | --- |
| A1, A2 | A: as R1 | straight lines for which (L − 2.0) / 2.5 is 8.5, 9.0, 9.2, 9.4, 9.44, 9.48, 9.5, 9.52, 9.56, 9.6 and 9.8, for the rounding of the number of gaps |
| A2 | H3: end dashes 3.0; H0: end dashes 0 | 25, 10 and 5 mm; 25, 3 and 1 mm |
| B1 | B: as R2; A | 1.0 to 3.0 mm; 2.0 to 5.0 mm, for the limit of a short line |
| B2 | C: as R6; C3: as R6, with a fill that is not dashed; G1: as R4; G2: as R3, with at least 3 gaps; E: as R5 | 1.0 to 5.0 mm; 1.5 and 3.0 mm; 2.45 to 5.0 mm; 4, 6 and 8 mm; 1.0, 1.5 and 3.0 mm |
| C | C and C3; SM, SB and SR: double lines that are not dashed, whose main line has a miter, a bevel and a round join | 25 mm; a line with a right angle and an angle of 45°, and the closed rectangle of R6 |
| D S, D gentle, D arch | A | three Bezier curves, each in 9 sizes for which the length of the curve gives 9.3 to 10.6 |
| D4 F | A and E | the S curve of R1 as a line of 64 straight parts, in the sizes of 9.5, 9.6 and 9.7; A with a corner point at 1 mm and at 24 mm of 25 mm, and at 12 mm of a straight line of 24 mm; E with a corner point at 1.5 mm of 3 mm and at 5 mm of 10 mm; A with a dash point at 5 mm of 25 mm |
| E 4.4, E 4.6 | A | lines at 0°, 15°, ..., 90°, whose lengths give 4.4 and 4.6 with the OCAD metric |

The files:

| File | Version | `p` in ViewPar |
| --- | --- | --- |
| [dashes-2-ocad12-euclidean.ocd](ocad-dashes/dashes-2-ocad12-euclidean.ocd) | OCAD 12 | 0 |
| [dashes-2-ocad12-ocad-metric.ocd](ocad-dashes/dashes-2-ocad12-ocad-metric.ocd) | OCAD 12 | 1 |
| [dashes-2-ocad10.ocd](ocad-dashes/dashes-2-ocad10.ocd) | OCAD 10 | none |
| [dashes-2-ocad10-ocad-metric.ocd](ocad-dashes/dashes-2-ocad10-ocad-metric.ocd) | OCAD 10 | 1, from a temporary change of `OcadExport` |
| [dashes-2-ocad9-ocad-metric.ocd](ocad-dashes/dashes-2-ocad9-ocad-metric.ocd) | OCAD 9 | 1, from a temporary change of `OcadExport` |

Purple Pen drew the files at 600 dpi: [Euclidean](ocad-dashes/purple-pen-dashes-2-ocad12-euclidean-600dpi.png) and
[OCAD metric](ocad-dashes/purple-pen-dashes-2-ocad12-ocad-metric-600dpi.png). It draws the OCAD 9 and 10 files as the
OCAD 12 file with the OCAD metric, as it reads no `p` from them.

@Pauli exported the five files on Nov 18, as for the first sample:
[Euclidean](ocad-dashes/ocad-viewer-dashes-2-ocad12-euclidean-600dpi.png),
[OCAD metric](ocad-dashes/ocad-viewer-dashes-2-ocad12-ocad-metric-600dpi.png),
[OCAD 10](ocad-dashes/ocad-viewer-dashes-2-ocad10-600dpi.png),
[OCAD 10 with `p1`](ocad-dashes/ocad-viewer-dashes-2-ocad10-ocad-metric-600dpi.png) and
[OCAD 9 with `p1`](ocad-dashes/ocad-viewer-dashes-2-ocad9-ocad-metric-600dpi.png).

### The values

The export of the OCAD 10 file without `p` is the same, pixel by pixel, as the export of the Euclidean OCAD 12 file.
The exports of the OCAD 9 and 10 files with `p1` are the same as the export of the OCAD 12 file with `p1`.

The counts of dashes that are not the same in the export of OCAD and the drawing of Purple Pen. In all the other cells,
the counts are the same. "Both" is both metrics.

| Cells | File | OCAD | Purple Pen |
| --- | --- | --- | --- |
| A1-1: A, (L − 2.0) / 2.5 = 8.5 | both | 10 | 9 |
| A1-7: A, 9.5 | both | 10 | 11 |
| A2-8: H0, 1 mm | both | 1, no gap | 0: the rule gives one gap and end dashes of 0 |
| B1-4: B, 1.5 mm; B1-16: A, 2.5 mm; B2-15: G1, 2.5 mm | both | 1, no gap | 2, 2 and 4 |
| B2-1 to B2-5: C, 1.0 to 2.5 mm; B2-12: C3, 1.5 mm | both | 1, no gap | 2 |
| B2-20 to B2-22: G2, 4, 6 and 8 mm | both | 2, 4 and 4: 1, 2 and 2 gaps | 4, 6 and 6: 3 gaps |
| B2-23: E, 1 mm | both | 1, from 0.49 to 1.00 mm | 0: the rule gives 2 gaps and dashes of 0 |
| DS-3, DS-4 and DS-9, Darch-3, Darch-4 and Darch-9: the S curve and the arch | Euclidean | one dash less | |
| D4F-1: the S curve as a line of 64 straight parts, 9.5009 | Euclidean | 10 | 11 |
| Darch-4 and Darch-9: the arch | OCAD metric | one dash less | |

Where the counts are the same, the dashes are at the same places: on the straight lines and the lines with corner and
dash points, the ends of the dashes are at most 0.05 mm apart. On the S curve and the arch, they are up to 0.17 mm
apart, and on the gentle curve 0.05 mm.

The solid double lines join at the right angle and at the angle of 45° as in Purple Pen, in each of the three line
styles. At the start point of the closed rectangle, OCAD leaves the outer corner open, about 0.25 mm, as two flat ends
meet there. Purple Pen fills it.

### What the second sample tells

- **The metric.** The viewer reads `p` from OCAD 9 and 10 files too: with `p1` it uses the OCAD metric, and without `p`
  the Euclidean metric. Purple Pen reads `p` only from OCAD 11+, and uses the OCAD metric for OCAD 6 to 10 files.
- **The length of a Bezier curve.** OCAD counts the gaps from a length in which each Bezier curve is four chords
  between the points of the parameters 0, ¼, ½, ¾ and 1, measured with the metric of the map. This length fits all
  the 28 Bezier curves of the two samples with each metric, and no other number of chords from 2 to 16 fits all of
  them with both metrics. The true length of the curve fits 21 of them with the Euclidean metric and 26 with the OCAD
  metric. Purple Pen measures the curve flattened into fine parts (`SymPath.GetFlattenedPoints`). OCAD puts the
  dashes along the full curve (the first sample).
- **A half.** With end dashes as long as the dashes, OCAD gives a line the whole number of dashes nearest to
  (L + gap + 2 × dash − 2 × end dash) / (dash + gap), and at exactly a half the even number: 9.5 and 10.5 both give 10
  dashes. Purple Pen rounds the number of gaps to the even number at a half (`Math.Round`), so it gives one dash less
  at 9.5 and one more at 10.5.
- **The length of a part.** The line of 64 straight parts fits only when OCAD rounds the length of each part to
  0.01 mm: its exact length gives 9.5009, and its rounded parts give 9.5 exactly. The rounded parts fit all the other
  cells too. No other cell decides this.
- **A short line.** When the line is not longer than its end dashes and a gap, OCAD gives it a gap only when it is
  longer than half of each end dash and a gap. Purple Pen gives it a gap from that length on
  (`pathLength >= firstDashLength / 2 + lastDashLength / 2 + gapLength`).
- **A double line.** OCAD gives the dashes of a double line no minimum gap. Purple Pen gives them at least one
  (`OcadImport`: `doubleDashes.minGaps = 1`).
- **End dashes of 0.** OCAD draws a line of 1 mm with end dashes of 0, which the rule gives one gap, as a solid line.
  Purple Pen draws nothing, as all of the line is the gap.
- **Not yet known.** With a secondary gap, OCAD did not give the lines of G2 the minimum of 3 gaps: they have the
  counts of the rule without a minimum. Without a secondary gap, OCAD gives the minimum as Purple Pen does (E, also
  with corner points). A line of E of 1 mm, which the minimum of 2 gaps leaves without dashes, gets one dash from
  0.49 mm to its end. The places of the dashes on strongly curved Bezier curves are up to 0.17 mm from those of
  Purple Pen. These need more samples before a fix.

## The third sample

The third sample examines what the fixes of tickets [017](../tickets/017-ocad-dash-count.md) and
[018](../tickets/018-double-line-dash-gaps.md) need: the minimum number of gaps (`MinSym`, which Purple Pen reads as
`MinSym + 1` gaps and writes as the minimum less one), the dashes of a double line beside a dashed main line, and the
glyphs spaced along a line, which Purple Pen counts with the same rule as the dashes. Purple Pen wrote it on Nov 18
as [dashes-3-ocad12-euclidean.ocd](ocad-dashes/dashes-3-ocad12-euclidean.ocd), from
[dashes-3-cells.json](ocad-dashes/dashes-3-cells.json): 117 cells in 14 rows, from the top.

| Rows | Symbols | Cells |
| --- | --- | --- |
| M0 to M4 | dashes 2.0 mm, end dashes 1.0, gaps 0.5, and a minimum of 0 to 4 gaps (`MinSym` −1 to 3) | 1.0, 1.5, 2.0, 2.6, 3.0, 4.0, 6.0 and 10.0 mm |
| MS0 to MS4 | as M0 to M4, with a secondary gap of 0.3 | as above |
| DM | DM0 and DM3: a main line of 0.1 mm dashed as M0 and M3, beside dashed borders of 0.15 mm without a fill (dashes 2.0, gaps 0.5) | 1.0, 1.5, 2.0, 3.0, 4.0 and 6.0 mm of each |
| GS straight | GS1: filled circles of 0.3 mm every 2.0 mm, the first and the last 1.0 mm from the ends, at least 1, without a main line | 8.0, 8.98, 9.0, 9.02, 10.0, 11.0 and 12.0 mm: L / 2 is a half at 9 and 11 mm |
| GS short | GS1, and GS3 with at least 3 | 0.8 to 3.0 mm; 1.0 to 6.0 mm |
| GS curve | GS1 | the S curve of the first sample in 7 sizes for which half of the length of the curve is 9.3 to 10.1 |

The script measures the main line of DM at its middle and the borders 0.375 mm to the left, and counts the glyphs as
runs. In [Purple Pen's drawing](ocad-dashes/purple-pen-dashes-3-ocad12-euclidean-600dpi.png), each count is the count
of the rule, except GScurve-2: its curve gives 9.5015 by a fine measure, and Purple Pen's flattened curve is less
than a half, so it gets 9 glyphs.

@Pauli exported the file on Nov 18, at [600 dpi](ocad-dashes/ocad-viewer-dashes-3-ocad12-euclidean-600dpi.png), as the
other samples, and at [1200 dpi](ocad-dashes/ocad-viewer-dashes-3-ocad12-euclidean-1200dpi.png).

### The values

The counts that are not the same in the export of OCAD at 600 dpi and the drawing of Purple Pen. In all the other
cells, the counts are the same. The minimum is Purple Pen's, as it reads `MinSym`.

| Cells | OCAD | Purple Pen |
| --- | --- | --- |
| M0-2 and MS0-2: 1.5 mm, no minimum | 1 | 2: the limit of a short line |
| M2-1 and MS2-1: 1.0 mm, a minimum of 2 | 1, from 0.48 to 0.98 mm | 0: the minimum leaves dashes of 0 |
| M3 and M4, 1.0 to 6.0 mm: a minimum of 3 and 4 (`MinSym` 2 and 3) | 2, 2, 2, 2, 2, 3 and 3: the counts with a minimum of 1 | 0, 0, 4, 4, 4, 4 and 4, and 0, 0, 0, 5, 5, 5 and 5 |
| MS3 and MS4, as M3 and M4 with a secondary gap | 2, 2, 2, 2, 2, 4 and 4: as MS1 | 0, 0, 4, 4, 4, 4 and 6, and 0, 0, 0, 5, 5, 5 and 8 |
| MS2-5: 3.0 mm, a minimum of 2 and a secondary gap | 3: the middle dash of 1.0 mm has no secondary gap | 4 |
| DM0 and DM3, the main line | the counts of OCAD in M0 and M3 | the counts of Purple Pen in M0 and M3 |
| DM, the borders, 1.0 to 2.0 mm, beside a main line with no minimum or a minimum of 3 | 1, no gap | 2 |
| GSstraight-3 and GSstraight-6: 9 and 11 mm | 5 and 5 glyphs | 4 and 6 |
| GSshort-6: 3 mm | 1 glyph, at the middle | 2 |
| GSshort-7 to GSshort-10: a minimum of 3 glyphs (`MinSym` 2), 1.0 to 4.0 mm | 1, 1, 1 and 2 | 3 |
| GScurve-3 and GScurve-4: the S curve, half of its true length 9.6 and 9.7 | 9 glyphs | 10 |

### What the third sample tells

- **The minimum number of gaps.** OCAD reads `MinSym` −1 as no minimum, 0 as a minimum of 1 gap and 1 as a minimum of
  2 gaps, as Purple Pen does. But it reads `MinSym` 2 and 3 as a minimum of 1 gap, as 0, for the dashes with and
  without a secondary gap and for spaced glyphs. Purple Pen reads them as minimums of 3 and 4 gaps or glyphs.
- **A secondary gap.** OCAD puts no secondary gap in a dash that is exactly half of the length of a dash. Purple Pen
  puts one in a dash that is half of it or longer (`actualDashLength >= dashLength / 2`).
- **Spaced glyphs** follow the rules of the dashes: at exactly a half, the number of glyphs plus one is even (9 and
  11 mm both give 5 glyphs, and 3 mm gives 1), and a Bezier curve is measured as four chords.
- **A double line beside a dashed main line.** The minimum of the main line does not reach the dashes of the borders:
  OCAD gives them no minimum, as without a main line.
- **Not yet known.** A line that a minimum of 2 gaps leaves without dashes (M2-1, and E of the second sample) gets one
  dash of 0.5 mm, from the end of the first gap to the end of the line.

## The fourth sample

@Pauli proposed on Nov 19 to draw the dashed lines with corner points, and the line symbols whose borders lie beside
the path, as a shape composed of a few lines, and to find how OCAD composes them by exact measures (ticket
[011](../tickets/011-line-geometry.md), question 4). The fourth sample measures that. Purple Pen wrote it on Nov 19 as
[dashes-4-ocad12-euclidean.ocd](ocad-dashes/dashes-4-ocad12-euclidean.ocd), from
[dashes-4-cells.json](ocad-dashes/dashes-4-cells.json): 55 cells in 9 rows, from the top. A and B are the symbols of
the second sample. The double lines have borders of 0.15 mm and no fill, at 0.6 or 1.6 mm apart, and their dashes are
2.0 mm with gaps of 0.5 mm.

| Rows | Symbols | Cells |
| --- | --- | --- |
| K A, K B | A, B | a part of 6.3 mm, a corner point, and a part of 8.7 mm that turns 0°, 30°, 60°, 90°, 120° and 150° |
| V A | A | as K A, with a plain vertex in place of the corner point |
| K more | A | three corner points and three plain vertices on the same line; a corner point 1 mm from the start and 1 mm before the end of a line that turns 60°; a dash point on a straight line and on a line that turns 60°; a closed rectangle of 12 × 6 mm with corner points |
| DB6, DB16 | dashed borders, 0.6 and 1.6 mm apart | 25 mm straight; a line with plain vertices that turns 60°, −90° and 120°; the same line with corner points; half circles of radius 5 and 1; the S curve; a closed circle of radius 5 |
| S6, S16 | solid borders, 0.6 and 1.6 mm apart | as DB6 and DB16 |
| DF | a fill of 0.6 mm and borders, all dashed | the line with corner points; the half circle of radius 5 |

The dashes of the corner rows tell how OCAD fits the parts of a line and joins their end dashes at a corner. The
dashes of the borders tell whether OCAD puts the dash of a border beside the dash of the middle of the line, as
Purple Pen does (`OffsetPointsRight`), or fits the dashes to the length of each border. The solid borders tell how
OCAD makes the edges of a border at the vertices, along tight curves and where the inner border of a half circle of
radius 1 has no room. [Purple Pen's drawing](ocad-dashes/purple-pen-dashes-4-ocad12-euclidean-600dpi.png) is at 600
dpi.

@Pauli exported the file on Nov 19 at [600 dpi](ocad-dashes/ocad-viewer-dashes-4-ocad12-euclidean-600dpi.png) and at
[1200 dpi](ocad-dashes/ocad-viewer-dashes-4-ocad12-euclidean-1200dpi.png).

### The values

- **Corner and dash points.** In all the 25 cells of the rows K A, K B, V A and K more, OCAD draws the same number of
  dashes as Purple Pen, and the ends of the dashes are at most 0.055 mm apart: about one pixel of 600 dpi.
- **The dashes of the borders.** Counted on each border, an offset of the path with miter joins, the borders have
  the dashes of the middle of the line, in OCAD as in Purple Pen, and not dashes fitted to the length of each border:

  | Cell | Middle | Left border: length, fitted, OCAD, Purple Pen | Right border: length, fitted, OCAD, Purple Pen |
  | --- | --- | --- | --- |
  | DB16-4, half circle of radius 5 | 15.71 mm, 6 dashes | 18.46 mm, 8, 6, 6 | 12.97 mm, 5, 6, 6 |
  | DB16-7, closed circle of radius 5 | 31.42 mm, 13 dashes | 25.92 mm, 11, 13, 13 | 36.92 mm, 15, 13, 13 |
  | DB16-2, plain vertices | 27.00 mm, 11 dashes | 24.71 mm, 10, 10, 10 | 29.30 mm, 12, 10, 10 |
  | DB6-6 and DB16-6, the S curve | 26.20 mm, 11 dashes | 26.20 mm, 11, 10, 11 | 26.20 mm, 11, 10, 11 |

  On the S curve, OCAD gives the borders the 10 dashes of its count of the middle (ticket
  [017](../tickets/017-ocad-dash-count.md)). The ends of the dashes of the borders are at most 0.13 mm apart, except
  on the left border of DB6-2 and DB16-2, where one end is 0.76 and 1.74 mm apart. There a dash of the middle starts
  exactly at a plain vertex: Purple Pen starts it just before the vertex, so its dash on the outer border takes the
  corner of the miter, and OCAD starts it at the vertex. Where a dash of the middle goes over an outer vertex, as on
  the right border at the first vertex, OCAD's dash on the border takes the corner of the miter as Purple Pen's does.
- **Solid borders.** On the same grid of 600 dpi, OCAD's and Purple Pen's borders differ only in single pixels at
  their edges: at the turns of 60° and 120°, at the corner points, and along the half circles. The inner border of
  the half circle of radius 1 of S16 has no room, and both draw a small spot there.

### What the fourth sample tells

- OCAD fits the dashes of each part between corner and dash points, and joins the end dashes of the parts into one
  dash at the point, as Purple Pen does (`ComputeDashDistances`).
- OCAD puts the dash of a border beside the dash of the middle, as Purple Pen does: the offset of the dash of the
  middle, with miter joins (`OffsetPointsRight`).
- OCAD draws a solid border as the offset of the path with miter joins, as Purple Pen does.
- So the composition that ticket [011](../tickets/011-line-geometry.md) considers (question 4) can follow the
  drawing of Purple Pen and of OCAD alike, except at a dash that starts exactly at a vertex.

## The fifth sample

In the fourth sample, a dash of the middle of DB16-2 starts 0.004 mm before a plain vertex, by exact arithmetic and by
Purple Pen's. Purple Pen's dash takes the vertex, so on the outer border it takes the corner of the miter. OCAD's
dash on the outer border starts after the corner, as if the dash started at the vertex. The fifth sample finds how
far from a vertex OCAD takes a boundary of a dash as at the vertex. Purple Pen wrote it on Nov 19 as
[dashes-5-ocad12-euclidean.ocd](ocad-dashes/dashes-5-ocad12-euclidean.ocd), from
[dashes-5-cells.json](ocad-dashes/dashes-5-cells.json): 38 cells in 4 rows. Each cell is a line of three straight
parts with coordinates on the grid of 0.01 mm: 8 mm along x, a part at about 60°, and a part of about 9.6 mm that
turns about 90° to the right. The second vertex is placed so that a dash starts, or ends, at a given distance from it,
by the exact fit of the dashes: 0.020, 0.012, 0.008, 0.006, 0.004 and 0.002 mm before it, and up to 0.006 mm after it.

| Rows | Symbol | Boundary near the vertex |
| --- | --- | --- |
| DB16 start, DB16 end | DB16 of the fourth sample: dashed borders of 0.15 mm, 1.6 mm apart | a start of a dash; an end of a dash |
| W start, W end | W: a dashed line of 1.0 mm with miter joins and flat ends, dashes and end dashes of 2.0 mm, gaps of 0.5 mm | as above |

On the outer border of DB16, and on the outer side of the wide line W, the corner of the miter tells whether the dash
takes the vertex. [Purple Pen's drawing](ocad-dashes/purple-pen-dashes-5-ocad12-euclidean-600dpi.png) is at 600 dpi.

@Pauli exported the file on Nov 19 at [600 dpi](ocad-dashes/ocad-viewer-dashes-5-ocad12-euclidean-600dpi.png).

### The values and what they tell

In none of the 38 cells does OCAD's dash take the corner of the miter: not on the outer border of DB16, and not on the
outer side of W, also where the dash starts 0.020 mm before the vertex or ends 0.004 mm after it. Purple Pen's dash
takes the corner in each cell where it starts before the vertex or ends after it. In the fourth sample, OCAD's dash of
the right border of DB16-2, which goes 0.5 mm over a vertex, takes the corner. So OCAD takes a boundary that is at
least 0.02 mm from a vertex as at the vertex, and the limit is between 0.02 and 0.5 mm.

## The sixth sample

The sixth sample finds the limit. Purple Pen wrote it on Nov 19 as
[dashes-6-ocad12-euclidean.ocd](ocad-dashes/dashes-6-ocad12-euclidean.ocd), from
[dashes-6-cells.json](ocad-dashes/dashes-6-cells.json): 56 cells in 8 rows, made as the cells of the fifth sample, with
third parts of 8.7 to 10.2 mm. A dash starts 0.03, 0.05, 0.08, 0.10, 0.15, 0.20, 0.30 and 0.50 mm before the second
vertex, or ends 0.03, 0.05, 0.08, 0.10, 0.30 and 0.50 mm after it.

| Rows | Symbol |
| --- | --- |
| W start, W end | a dashed line of 1.0 mm with miter joins and flat ends, dashes and end dashes of 2.0 mm, gaps of 0.5 mm |
| W3 start, W3 end | as W, 0.3 mm wide |
| DB6 start, DB6 end | dashed borders of 0.15 mm, 0.6 mm apart, without a fill |
| DB16 start, DB16 end | as DB6, 1.6 mm apart |

Two widths of each kind tell whether the limit depends on the width of the line or on the distance of a border from
the middle. [Purple Pen's drawing](ocad-dashes/purple-pen-dashes-6-ocad12-euclidean-600dpi.png) is at 600 dpi.

@Pauli exported the file on Nov 19 at [600 dpi](ocad-dashes/ocad-viewer-dashes-6-ocad12-euclidean-600dpi.png) and at
[1200 dpi](ocad-dashes/ocad-viewer-dashes-6-ocad12-euclidean-1200dpi.png).

### The values

[measure-corners.py](ocad-dashes/measure-corners.py) measures each cell. It finds the corner of the miter from the
dark pixels along the outer bisector of the vertex. It finds the boundary of the dash from the flat end of the
neighbouring dash, one gap away: that end is oblique to the pixels, so the mean of where it crosses many parallel scan
lines gives its position to a fraction of a pixel. In the fifth sample, the script finds no corner in OCAD's drawing,
as above.

In each cell where OCAD's dash does not take the corner, OCAD leaves out the short piece beyond the vertex: the dash
starts or ends at the vertex, with a flat end across the part. On a double line, the dash of the outer border then
starts at the offset of the vertex on the next part, and the dash of the inner border crosses the inner border of the
other part. Where the dash takes the corner, OCAD draws the piece and the miter as Purple Pen does. Purple Pen's dash
takes the corner in all the 56 cells.

For each row, the table gives the largest distance of the boundary from the vertex at which OCAD leaves out the piece,
and the smallest at which it draws the piece and the corner. These are the distances of the cells, by exact
arithmetic.

| Row | Symbol | 600 dpi: no corner, corner | 1200 dpi: no corner, corner |
| --- | --- | --- | --- |
| W start | line of 1.0 mm | 0.08, 0.10 mm | 0.15, 0.20 mm |
| W end | line of 1.0 mm | 0.10, 0.30 mm | 0.10, 0.30 mm |
| W3 start, W3 end | line of 0.3 mm | 0.05, 0.08 mm | 0.05, 0.08 mm |
| DB6 start, DB16 start | borders of 0.15 mm, 0.6 and 1.6 mm apart | 0.03, 0.05 mm | 0.03, 0.05 mm |
| DB6 end, DB16 end | borders of 0.15 mm, 0.6 and 1.6 mm apart | 0.05, 0.08 mm | 0.05, 0.08 mm |

In the drawing of 1200 dpi, OCAD's boundaries near the second vertex are 0.01 to 0.02 mm earlier along the line than
by exact arithmetic: a start is that much farther before the vertex, and an end that much nearer after it. With the
boundaries as drawn, the starts and the ends of a row agree. OCAD leaves out a piece of the 0.3 mm line and of the
borders up to 0.055 mm, and draws it from 0.059 mm. It leaves out a piece of the 1.0 mm line up to 0.159 mm, and
draws it from 0.213 mm. In the drawing of 600 dpi, the script finds a boundary to about 0.03 mm only, which does not
make these limits narrower.

At 600 dpi, OCAD draws the piece of the cell W start 0.10 with vertical sides, though its part has a direction of 70°.
This agrees with a piece drawn from coordinates rounded to pixels.

### The PDF export

@Pauli exported the file to [PDF](ocad-dashes/ocad-viewer-dashes-6-ocad12-euclidean.pdf) on Nov 19. The PDF has each
dash as a stroked path, on a layer for each symbol, with the coordinates of its points to 0.01 pt (0.0035 mm). The
single lines have miter joins and flat ends. Each border of a double line is a path of its own: the offset of the
dash of the middle, with the corner of the miter as a point, stroked with bevel joins and flat ends.
[measure-pdf-vertices.py](ocad-dashes/measure-pdf-vertices.py) reads the paths. Below, a unit is 0.01 mm, the unit of
OCAD's coordinates.

- OCAD puts each point of a dash, on the path and on the borders, on its grid of units: no point is more than
  0.0023 mm from the grid, which is the rounding of the PDF. So a boundary of a dash is up to 0.7 units from its exact
  place.
- Where the piece of a dash beyond the vertex is short, the vertex is not a point of the dash: the dash goes straight
  from its boundary to the next point. OCAD does not move the boundary to the vertex. Where the piece is longer, the
  vertex is a point of the dash, with the corner of the miter.
- The two borders of a double line keep or leave out the vertex together.

| Symbol | Vertex left out: longest piece | Vertex kept: shortest piece |
| --- | --- | --- |
| W, line of 1.0 mm | 30.36 units | 50.01 units |
| W3, line of 0.3 mm | 10.77 units | 15.52 units |
| DB6, DB16, borders of 0.15 mm, main line of width 0 | 4.47 units | 5.39 units |

The piece is the distance between the vertex and OCAD's point of the boundary, which is on the grid. For a double
line, the boundary is the midpoint of the ends of the two borders.

On the single lines, half the width of the line is in the limits: 50 and 15 units. On the double lines, the limit is
between 4.47 and 5.39 units, for both distances of the borders. All the turns of the sample are 90°, where half the
width is also the extension of the miter along the part, (w/2) tan(turn/2). So the sample does not tell whether the
limit depends on the angle of the turn.

The bitmap exports do not agree with the PDF on the single lines. At 1200 dpi, OCAD draws the corner of W from a piece
of 0.20 mm and of W3 from 0.08 mm, where the PDF leaves the vertex out up to 0.30 and 0.11 mm. On the double lines
they agree. So OCAD draws its bitmaps by a rule of its own, which depends on the resolution.

### What the sixth sample tells

- OCAD leaves the vertex out of a dash whose piece beyond the vertex is short: the dash goes straight from its
  boundary to the next point.
- In the PDF, the limit on a single line is half its width, at a turn of 90°. On a double line without a main line,
  it is about 5 units (between 4.47 and 5.39).
- OCAD puts the points of the dashes on its grid of 0.01 mm.
- OCAD's bitmaps follow a different rule on the single lines, which depends on the resolution. The PDF gives OCAD's
  geometry.
- The sample does not tell how the limit depends on the angle of the turn, on the width of the borders of a double
  line, or on a main line beside the borders.

## The PDF exports of the first five samples

@Pauli exported the files of the first five samples to PDF on Nov 19 (`ocad-viewer-*.pdf` in
[ocad-dashes/](ocad-dashes/)). [measure-pdf-dashes.py](ocad-dashes/measure-pdf-dashes.py) computes the dashes of each
cell by the rules that the fixes 016 to 019 give Purple Pen, and compares them with OCAD's dashes: their count, and the
distance of each end of a dash from the nearest end by the rules. For the borders of a double line, the rules give the
dashes of the middle moved aside by the distance of the border.

- **The counts.** In all the eleven files, each line of each cell has the count of the rules, in the metric of each
  file: 1011 lines in all. The only exception is the case that the third sample left open: a line of 1 mm with a minimum
  of 2 gaps (B2-23, M2-1 and MS2-1), where OCAD draws one dash from the end of the first gap to the end of the line.
- **The ends on straight parts.** On straight lines, at corner and dash points, on the closed rectangles and on the
  short lines, OCAD's ends are at most 0.021 mm from the ends by the rules: the rounding to OCAD's grid, and a little
  more next to a corner point.
- **The ends on curves.** On Bezier curves, OCAD's ends are up to 0.16 mm from the ends along the true arc of the
  curve, in a pattern that follows the bends of the curve. OCAD writes each dash on a curve as a part of the curve, and
  its ends are on the curve. Neither a fit along the true arc, nor four chords with the parameter in proportion along
  them, nor a parameter in proportion to the length, nor a polyline of N equal steps of the parameter gives OCAD's
  ends. The nearest is a fit along four chords with each end moved to the nearest point of the curve, which still
  leaves 0.08 mm. How OCAD finds the point of a curve at a length is not known.
- **The vertices.** On DB6-2 and DB16-2 of the fourth sample, the ends of the borders are where the rule of the
  seventh sample leaves the vertex out of the dash.

## The seventh sample

The PDF export of the sixth sample showed OCAD's limit for the piece of a dash beyond a vertex at turns of 90° only.
The seventh sample finds how the limit depends on the angle of the turn, on the width of a single line, and on the
borders and the main line of a double line. It also finds whether OCAD leaves out a vertex inside a dash or in a solid
line. Purple Pen wrote it on Nov 19 as [dashes-7-ocad12-euclidean.ocd](ocad-dashes/dashes-7-ocad12-euclidean.ocd),
from [dashes-7-cells.json](ocad-dashes/dashes-7-cells.json): 614 cells in 41 rows. Each vertex cell is a line of three
straight parts with coordinates on the grid of 0.01 mm: 3 mm along x, a part at 60°, and a part that turns right by
the angle of its row. A dash starts the given piece before the second vertex, or ends the given piece after it, by
exact arithmetic. All the dashes are 2.0 mm, with end dashes of 2.0 mm and gaps of 0.5 mm, and all the lines have
miter joins and flat ends.

| Symbols | Lines | Turns | Pieces |
| --- | --- | --- | --- |
| L100, L030 | single lines of 1.0 and 0.3 mm | 30°, 60°, 90°, 120°, 150° | 0.05 to 1.00 mm, 0.02 to 0.80 mm |
| L006, L010, L020, L050 | single lines of 0.06, 0.10, 0.20 and 0.50 mm | 90° | near half the width and near 0.05 mm |
| DB10, DB15, DB30 | double lines without a main line: borders of 0.10, 0.15 and 0.30 mm, 0.6 mm apart | 90°, and 30° and 150° for DB15 | 0.02 to 0.30 mm |
| DBM | a double line with borders of 0.15 mm, 0.6 mm apart, and a dashed main line of 0.5 mm | 90° | 0.02 to 0.30 mm |

For each symbol and turn, there are two rows of cells: in one, a dash starts before the vertex, and in the other, a
dash ends after it. The last row has the scope cells: a part of 0.10 or 0.30 mm between two turns of 45° in the middle
of a dash of L100 and in S100, a solid line of 1.0 mm; a dash of L100 that starts 0.05 mm before a vertex, followed by
two parts of 0.10 mm; and a line of S100 whose first part is 0.10 mm and whose last part is 0.30 mm.
[Purple Pen's drawing](ocad-dashes/purple-pen-dashes-7-ocad12-euclidean-150dpi.png) at 150 dpi shows the cells. The
oracle of this sample is OCAD's PDF export.

@Pauli exported the file on Nov 19 to [PDF](ocad-dashes/ocad-viewer-dashes-7-ocad12-euclidean.pdf), and at 1200 dpi,
which this file does not use. `measure-pdf-vertices.py dashes-7-cells.json ocad-viewer-dashes-7-ocad12-euclidean.pdf`
gives the values.

### The values

On the single lines, the limit is 0.4 times the width of the line, at all the angles. The pieces are OCAD's, in units
of 0.01 mm:

| Symbol | Width | Vertex left out: longest piece | Vertex kept: shortest piece | 0.4 × width |
| --- | --- | --- | --- | --- |
| L006 | 0.06 mm | 2.24 | 2.83 | 2.4 |
| L010 | 0.10 mm | 3.61 | 4.47 | 4 |
| L020 | 0.20 mm | 7.21 | 8.60 | 8 |
| L030, turns of 30° to 150° | 0.30 mm | 11.66 | 12.00 | 12 |
| L050, and the main line of DBM | 0.50 mm | 19.72 | 20.59 | 20 |
| L100, turns of 30° to 150° | 1.00 mm | 39.82 | 40.00 | 40 |

OCAD keeps the vertex of a piece of exactly 40.00 and 12.00 units, so it leaves the vertex out when the piece is
shorter than the limit.

The scope cells show that the rule concerns only the vertex next to the start or the end of a dash:

- A part of 0.10 mm or 0.30 mm in the middle of a dash of L100 keeps both its vertices.
- Where a dash of L100 starts 0.05 mm before a vertex and two parts of 0.10 mm follow, OCAD leaves out only the first
  vertex. The second vertex stays, though the new first part is 0.14 mm, shorter than the limit. So the rule does not
  repeat.
- A solid line keeps all its vertices, also a first part of 0.10 mm and a last part of 0.30 mm.

On a double line, two rules act:

- The dash of the middle leaves out the vertex next to its end when the piece is up to about 0.045 mm, and keeps it
  from about 0.058 mm: on DB15, DB30 and the borders of DBM between 5.00 and 5.83 units, on DB10 between 4.47 and
  5.00 units, at 90° and 150°. This limit is the same for the borders of 0.15 and 0.30 mm, so it does not come from
  their width.
- Each border then leaves out the vertex next to the end of its own dash when its own piece, from its end to the corner
  of the miter, is shorter than about 0.4 times the width of the border. The piece of the inner border is
  |piece − h tan(turn/2)|, where h is the distance of the border from the path. So the inner border of DB15 leaves out
  its corner where a dash starts 0.305 mm before a vertex of 90° (its own piece is 0.066 mm), and at a turn of 30° up
  to a dash that starts 0.153 mm before the vertex. The outer border keeps its corner there. In the cells, the pieces
  of the inner borders agree with 0.4 times the width within a unit: 3.95 left out and 8.90 kept on DB10, 5.71 and
  6.59 left out and 10.17 kept on DB15, 12.69 kept on DB30.

OCAD puts the boundaries of the dashes on its grid: in most cells at the exact boundary rounded down, in the others
one unit beside it. The borders of a double line are the offsets of these rounded points, so where the piece is a
few units long, its direction on the grid turns the borders a little.

### What the seventh sample tells

- On a single line, OCAD leaves out the vertex next to the start or the end of a dash when the piece between them is
  shorter than 0.4 times the width of the line, whatever the angle of the turn. The dash then goes straight from its
  boundary to the next point.
- The rule does not repeat, and it does not touch the vertices inside a dash or the vertices of a solid line.
- On a double line, the dash of the middle leaves out the vertex next to its end for a piece shorter than about
  0.05 mm, and each border then applies the rule of a single line to its own dash, with the width of the border.
- The limits are on OCAD's grid of 0.01 mm, so near a limit OCAD's rounding decides, and a rule on exact pieces
  agrees with OCAD up to a unit.

## The eighth sample

Two questions stay open after the seventh sample: how OCAD finds the point of a Bezier curve at a length along it
(see the PDF exports of the first five samples), and the limit of the rule of the vertex for the dashed fill of a
double line and at a vertex where a straight part meets a curve. Purple Pen wrote the eighth sample on Nov 19 as
[dashes-8-ocad12-euclidean.ocd](ocad-dashes/dashes-8-ocad12-euclidean.ocd) and, with the OCAD metric,
[dashes-8-ocad12-ocad-metric.ocd](ocad-dashes/dashes-8-ocad12-ocad-metric.ocd), from
[dashes-8-cells.json](ocad-dashes/dashes-8-cells.json): 43 cells. The dashes are 2.0 mm, with end dashes of 2.0 mm and
gaps of 0.5 mm.

| Rows | Symbol | Cells |
| --- | --- | --- |
| straight | L010, a line of 0.1 mm | Bezier curves that are straight, 20 mm along x, with their control points at fractions of the length (1/3 and 2/3, 0 and 1, 0.5 and 0.5, 0.1 and 0.3, 0.7 and 0.9, 0.25 and 0.25, 0 and 0.5, 0.5 and 1), so that the parameter runs at an uneven speed along them |
| arcs | L010 | quarter circles of radius 3, 5, 10 and 20 mm, each one Bezier curve; half circles of radius 5 and 10 mm, each two curves; the S curve of the first sample |
| DF 90 start, DF 90 end | DF, a double line with a dashed fill of 0.6 mm and dashed borders of 0.15 mm | a turn of 90°, where a dash starts 0.02 to 0.30 mm before the vertex or ends that much after it |
| join start, join end | L100, a line of 1.0 mm | a straight part, then a quarter circle that continues it at the join without a turn; a dash starts 0.1, 0.2, 0.3 or 0.5 mm before the join or ends that much after it |

On a straight Bezier curve, the ends of OCAD's dashes along x show directly how OCAD turns a length into a parameter
of the curve: by the true arc, the dashes are even whatever the control points. The files are on @Pauli's drive in
`ocad-dashes-8`. The oracle of this sample is OCAD's PDF export of both files.

### The PDF exports of the eighth sample

@Pauli exported both files as PDF on Nov 19:
[ocad-viewer-dashes-8-ocad12-euclidean.pdf](ocad-dashes/ocad-viewer-dashes-8-ocad12-euclidean.pdf) and
[ocad-viewer-dashes-8-ocad12-ocad-metric.pdf](ocad-dashes/ocad-viewer-dashes-8-ocad12-ocad-metric.pdf). OCAD writes
a dash on a curve as Bezier pieces, and splits it where the curve has the parameters 1/4, 1/2 and 3/4.
[measure-pdf-curves.py](ocad-dashes/measure-pdf-curves.py) compares the dashes of the euclidean file with two models
(see the script):

| Cells | Even along the true length | Each quarter measured by its chord |
| --- | --- | --- |
| straight Bezier curves, 8 cells | at most 0.016 mm | at most 0.016 mm |
| quarter circles, radius 3 to 20 mm | at most 0.017 mm | at most 0.020 mm |
| the S curve | 0.130 mm | 0.041 mm |

- **The length of a curve.** On a straight curve, OCAD follows the true length whatever the control points, so it does
  not turn a length into a parameter in proportion. On circles its dashes are even along the true arc. On the S curve,
  which bends strongly, they are not: its second and ninth dashes are about 0.08 mm longer than the mean, and its gaps
  near the ends are up to 0.54 mm long. A measure in which each quarter of the curve is as long as its chord fits
  within 0.041 mm, but not exactly, so the rule of OCAD is not found. With the OCAD metric, no tested model fits the
  quarter circles of radius 10 and 20 mm and the S curve: the errors are 0.06 to 0.14 mm.
- **The dashed fill of a double line.** The fill of 0.6 mm leaves the vertex out where its piece is at most 0.20 mm,
  and keeps it from 0.25 mm. The limit of fix 020, 0.4 times the width of the fill or 0.24 mm, fits.
- **The borders of a double line.** Below 0.05 mm both borders leave the vertex out, by the rule of the middle. From
  0.06 mm they keep it, except the inner border where the dash starts 0.30 mm before the vertex: its piece is about
  0.07 mm, against the limit of 0.4 times 0.15 mm, 0.06 mm. That is within the grid of 0.01 mm.
- **The join of a straight part and a curve** is a vertex for the rule: the line of 1.0 mm leaves it out where the
  piece is 0.1 to 0.3 mm, and keeps it at 0.5 mm. Where a dash leaves out the join at its start, OCAD writes the dash
  as one Bezier piece from its start on the straight part along the curve.

@Pauli's decision on the curves (Nov 19): the even dashes along the true length, which Purple Pen draws, are
acceptable, but the rule of OCAD is still to be found, and the decision is then made again. The ninth sample is for
that.

## The ninth sample

The eighth sample did not find how OCAD places the dashes on a curve that bends strongly. Purple Pen wrote the ninth
sample on Nov 19 as [dashes-9-ocad12-euclidean.ocd](ocad-dashes/dashes-9-ocad12-euclidean.ocd) and, with the OCAD
metric, [dashes-9-ocad12-ocad-metric.ocd](ocad-dashes/dashes-9-ocad12-ocad-metric.ocd), from
[dashes-9-cells.json](ocad-dashes/dashes-9-cells.json): 17 cells, each one Bezier curve. The dashed line D050 has
dashes and end dashes of 0.5 mm and gaps of 0.25 mm, so that the boundaries of its dashes sample OCAD's placement
along each curve every 0.25 or 0.5 mm. From them, the measure in which OCAD's dashes are even can be found along
each curve.

| Rows | Cells |
| --- | --- |
| bend | circular arcs of radius 15 mm that turn 30, 60, 90, 120, 150 and 180 degrees, each one Bezier curve |
| hook | a curve whose bend is near its end, one whose bend is near its start, one whose control points meet at a corner, and one whose parameter starts at speed 0 |
| S | S curves 30 mm long whose control points are 5, 10 and 15 mm off the chord, and the S curve of the first sample |
| turned | the arc of 90 degrees and the middle S curve turned by 30 degrees, for the OCAD metric, and a curve with a loop |

The files are on @Pauli's drive in `ocad-dashes-9`, for their PDF export.
[purple-pen-dashes-9-ocad12-euclidean-150dpi.png](ocad-dashes/purple-pen-dashes-9-ocad12-euclidean-150dpi.png) is
Purple Pen's drawing of the euclidean file.
