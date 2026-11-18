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

The exports of the OCAD viewer are to come, one for each file, as for the first sample. Their names are
`ocad-viewer-<the name of the file>-600dpi.png`.
