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
dpi. An export of 1200 dpi measures the edges more exactly.
