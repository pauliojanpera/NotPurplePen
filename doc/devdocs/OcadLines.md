# OCAD's lines

Nov 19, 2026. This document specifies how OCAD draws the line symbols: the metric of the lengths along a line, the
dashes, the double lines, the glyphs along a line and the text along a line. It collects what the samples of Nov 18
and 19 show, and what follows from them. A rule is "verified" only where a sample shows it, and the document names the
sample. The section "Not known" lists what no sample checked. The last section tells what Purple Pen does with each
rule.

OCAD is the authority on how its own maps render (the preface of
[PaletteStack/Specification.md](PaletteStack/Specification.md)).

## Sources

- **The samples.** Purple Pen wrote each sample file. @Pauli exported the files from the OCAD viewer as PNG images of
  600 or 1200 dpi, and as PDF. The samples and the measurements are in these documents:
  - [PaletteStack/oracles/ocad-dashes.md](PaletteStack/oracles/ocad-dashes.md): nine samples of dashed lines, double
    lines and spaced glyphs.
  - [PaletteStack/oracles/ocad-line-text.md](PaletteStack/oracles/ocad-line-text.md): one sample of text along lines.
  - [PaletteStack/oracles/ocad-thick-circles.md](PaletteStack/oracles/ocad-thick-circles.md): the circle elements of
    glyphs.
- **The viewer.** The exports are from the OCAD viewer OCAD 2020.5.18.3780, build date 2022-05-09, 32 bit (@Pauli,
  Nov 19).
- **The reference.** The PDF export gives OCAD's geometry: it writes each dash as a path, with its points to 0.01 pt
  (0.0035 mm). The bitmap exports follow other rules in some details (see "The vertex next to the end of a dash").
  So this document uses the PDF exports where they exist.
- **The scripts.** The scripts that measure the exports are in
  [PaletteStack/oracles/ocad-dashes/](PaletteStack/oracles/ocad-dashes/) (`measure-*.py`).
  [OcadLines/ocad-metric-arcs.py](OcadLines/ocad-metric-arcs.py) computes the tables of the section "The OCAD metric
  and a euclidean measure".

## Coordinates

- OCAD keeps its coordinates on a grid of 0.01 mm on the paper. This document calls 0.01 mm a unit.
- OCAD puts each point of a dash on this grid. Mostly it is the exact point rounded down, and in the other cells one
  unit beside it (the seventh sample). So near a limit, the rounding of OCAD decides, and a rule on exact lengths
  agrees with OCAD to one unit only.
- OCAD rounds the length of each straight segment to a unit when it counts the dashes: a line of 64 straight segments
  fits only with this rounding (the second sample).
- The directions in this document are directions on the paper, as angles to the x axis of the paper. OCAD draws in
  the coordinates of the paper. The georeference of a map (the offset, the angle and the grid of the real world
  coordinates) is not in them. The sample files had no georeference (`r0` and `a0` in the scale parameters), so a map
  with a georeference angle is not checked.

## The metric

### The choice of the metric

- The view parameter `p` of the map selects the metric of the lines: `p1` is the OCAD metric, and `p0` is the
  euclidean metric. Without `p`, OCAD uses the euclidean metric. Verified with OCAD 9, 10 and 12 files (the first and
  the second sample).
- OCAD 6 to 8 files have no view parameters. The viewer measures them with the euclidean metric, as Purple Pen does
  (@Pauli, Nov 18). Verified: the tenth sample, whose OCAD 7 and 8 files the viewer draws as the OCAD 12 file with the
  euclidean metric, to 0.01 mm. The OCAD 6 file agrees too, except at a dash point, which Purple Pen does not write to
  an OCAD 6 file.
- Newer files can have `p1`. In the test data of Purple Pen, the OCAD 11 and 12 versions of Lake Sammamish and the
  OCAD 2018 version of teanwest have it.
- The metric of the map measures the lengths along a line for the dashes and for the spaced glyphs. Purple Pen also
  uses it for the shortening of a line and for its pointy ends. No sample checked these two in OCAD.
- The text along a line uses the OCAD metric always, also in a map with the euclidean metric (see "The text along
  lines").

### The OCAD metric

The length of a step (dx, dy) in the OCAD metric is max(|dx|, |dy|) + min(|dx|, |dy|) / 2. Purple Pen calls it
`SymPath.BizzarroDistance`.

- It is a norm: half of the sum of the L1 norm, |dx| + |dy|, and the maximum norm, max(|dx|, |dy|).
- Its unit circle is an octagon. The corners of the octagon are at (±1, 0), (0, ±1) and (±2/3, ±2/3).
- Along a direction at the angle a, a step of euclidean length 1 has the length k(a) in the OCAD metric:

| Angle a | k(a) | Euclidean step for a step of 1 in the OCAD metric |
| --- | --- | --- |
| 0° and 90° | 1.0000 | 1.000 |
| 15° and 75° | 1.0953 | 0.913 |
| 26.57° and 63.43° (tan a = 1/2): the largest | 1.1180 | 0.894 |
| 30° and 60° | 1.1160 | 0.896 |
| 45° | 1.0607 | 0.943 |
| The mean of all directions | 1.0868 | |

k(a) repeats every 90°, and it is symmetric about each axis and each diagonal. The sample of the text along lines
shows these factors in OCAD's export: 0.896 at 30°, 0.943 at 45°, and 0.901 on a zigzag at 33.7°, where the table
gives 0.901.

### What the OCAD metric is

- The formula is a known approximation of the euclidean length without a square root. Its name is "alpha max plus
  beta min", here with alpha 1 and beta 1/2. It needs one comparison, one halving and one addition.
- Its error against the euclidean length is 0 along the axes, +6.07 % at 45°, and +11.80 % at 26.57°, the largest.
  The mean error is +8.68 %.
- OCAD does not tell why it uses the formula. The probable reason is speed. The first versions of OCAD ran on PCs of
  the late 1980s, many of which had no floating-point unit, and OCAD keeps its coordinates as integers. This is a
  conclusion from the formula. It is not a statement of OCAD. The comment of `SymPath.BizzarroDistance` in Purple Pen
  says that the reason is not known.

### The OCAD metric and a euclidean measure

The dash patterns of SVG and PDF, the dash effect of Skia and the 1D path effect of Skia measure the euclidean length
along a path.

- **A straight segment.** On a straight segment, the length in the OCAD metric is k(a) times the euclidean length,
  with one angle a for the segment. So an even euclidean step of s / k(a) gives the same points as a step of s in the
  OCAD metric, exactly.
- **A curve.** On a curve, the direction changes, and k(a) changes with it. So no even euclidean step gives the points
  of the OCAD metric. No transformation of the path makes the euclidean length equal to the length in the OCAD metric:
  a linear map changes the octagon into a polygon, and never into a circle or an ellipse. So a measure of a channel
  gives the points of the OCAD metric exactly on straight segments only.
- **OCAD's own curves.** OCAD does not place the dashes on a curve along its length in the OCAD metric. In a quarter of
  a Bezier curve, it places the points in proportion to the euclidean length of the arc with each metric, and the OCAD
  metric changes only the chords of the quarters (see "The dashes on Bezier curves"). So an even euclidean step in
  each quarter gives OCAD's points on a curve, within the fit of that model.
- **A sector of 45°.** Where the direction of a path stays in one sector between an axis and a diagonal, the length
  in the OCAD metric along the path is a linear function of its chord. For example, where 0 ≤ dy ≤ dx at each point of
  the path, the length is Δx + Δy / 2. So the points at equal steps are where the path crosses parallel lines at equal
  distances. A solid line that stripes of that direction cut has its dashes at the points of the OCAD metric. But the
  ends of its dashes turn to the direction of the stripes: at the edges of the line, an end moves by up to a quarter of
  the width of the line. So the stripes do not give OCAD's dashes either.
- **The error of an even euclidean step on an arc.** The tables compare an even euclidean step with the steps of the
  OCAD metric on an arc of a circle. The euclidean step makes the first and the last point the same in both. The error
  is the largest distance between two points of the same step, along the arc. "Worst" is the worst start direction,
  and "mean" is the mean of all start directions, at intervals of 0.5°. R is the radius, and L is the length of the
  arc.

| Turn of the arc | Worst error / R | Mean error / R | Worst error / L |
| --- | --- | --- | --- |
| 5° | 0.00043 | 0.00019 | 0.0049 |
| 10° | 0.00153 | 0.00070 | 0.0088 |
| 15° | 0.00307 | 0.00147 | 0.0117 |
| 22.5° | 0.00578 | 0.00295 | 0.0147 |
| 30° | 0.00851 | 0.00457 | 0.0163 |
| 45° | 0.01243 | 0.00710 | 0.0158 |
| 60° | 0.01277 | 0.00889 | 0.0122 |
| 90° | 0.01622 | 0.01210 | 0.0103 |
| 180° | 0.01622 | 0.01210 | 0.0052 |

| Arc | The whole arc: worst, mean | A quarter of the arc: worst, mean |
| --- | --- | --- |
| R 5 mm, 90°, L 7.9 mm | 0.081 mm, 0.060 mm | 0.029 mm, 0.015 mm |
| R 10 mm, 90°, L 15.7 mm | 0.162 mm, 0.121 mm | 0.058 mm, 0.029 mm |
| R 20 mm, 90°, L 31.4 mm | 0.324 mm, 0.242 mm | 0.116 mm, 0.059 mm |
| R 40 mm, 22.5°, L 15.7 mm | 0.231 mm, 0.118 mm | 0.021 mm, 0.009 mm |
| R 100 mm, 10°, L 17.5 mm | 0.153 mm, 0.070 mm | 0.011 mm, 0.005 mm |

On a short arc, the error is about L × Δk / 8, where Δk is the change of k(a) along the arc. At one curvature, it grows
as the square of the length of the arc. It does not grow past a turn of 90°, which is the period of k(a). A quarter of
a Bezier curve that is near an arc turns by about a quarter of the angle of the curve. Purple Pen's present placement
with the OCAD metric steps along the arc in the OCAD metric, as these tables do.

## The dashes

The symbols of the samples are lines with flat ends and miter joins. In this section, L is the length of a part, d is
the length of a dash, g the length of a gap, and e1 and e2 the lengths of the end dashes.

### The parts of a line

- The corner points and the dash points of a line divide it into parts. OCAD fits the dashes to each part. It joins
  the end dashes of two parts into one dash at the point between them. Verified: the fourth sample, 25 cells, the ends
  of the dashes within 0.055 mm in the bitmap export, and the tenth sample on a path of Bezier curves.
- At a dash point, the end dash of each part is half a dash, so that one dash is centered on the point. At a corner
  point, the end dashes have their full length. Verified: the tenth sample.
- On a closed line, the end dashes join over the start point into one dash. OCAD also writes a piece of that dash
  that starts at the start point (the tenth sample). The closed rectangles of the earlier samples agree with Purple
  Pen's drawing.

### The length of a part

- **The length of a straight segment** is its length in the metric of the map, rounded to a unit for the count of
  the dashes (see "Coordinates").
- **The length of a Bezier curve** is the sum of four chords: between the points of the curve at the parameters 0,
  1/4, 1/2, 3/4 and 1, in the metric of the map. It is not the true length of the curve. This length gives the count
  of the dashes of all 28 Bezier curves of the first two samples, with each metric. No other number of chords from 2
  to 16 fits all of them with both metrics. On the curves of the ninth sample, the mean lengths of the dashes and the
  gaps agree with this length within 0.2 mm, the rounding of the grid. Only a curve with a loop differs, by 0.4 mm.

### The number of gaps

- **The rule.** When L − e1 − e2 > g, the number of dashes is the whole number nearest to
  (L + d − e1 − e2) / (d + g) + 1. At exactly a half, it is the even number. The number of gaps is one less. Verified:
  in the second sample, the quotients 8.5 and 9.5 both give 10 dashes.
- **All the counts.** In the PDF exports of the first five samples, each of the 1011 lines has the count of these
  rules, in the metric of its file. The only exception is the minimum of 2 gaps on a short line, below. In the tenth
  sample, each of the 150 lines has the count of the rules, except a path of 30 curves whose quotient is 23.516,
  0.016 above a half: OCAD gives it 23 dashes, and the rules 24. Its measure of four chords is 0.04 mm above the limit,
  so OCAD's own rounding of its measure decides there, and the rule of that rounding is not known.
- **A short line.** When L − e1 − e2 ≤ g, the line has one gap when L > e1 / 2 + e2 / 2 + g. Otherwise it has no gap.
  Verified: the first two samples.
- **End dashes of 0.** A line that the rule gives one gap and end dashes of 0 is solid (the second sample).
- **The minimum number of gaps.** The symbol gives it as `MinSym`. OCAD reads −1 as no minimum, 0 as a minimum of
  1 gap and 1 as a minimum of 2 gaps. It reads 2 and 3 as a minimum of 1 gap, as 0. Verified: the third sample, with
  and without secondary gaps. No sample checked values above 3.
- **The minimum of 2 gaps on a short line.** A line of 1 mm that a minimum of 2 gaps leaves without dashes gets one
  dash, from the end of the first gap to the end of the line (0.49 to 1.00 mm and 0.48 to 0.98 mm, the second and the
  third sample). The rule of this is not known.

### The lengths of the dashes

- The gaps keep their length g.
- The dashes between the end dashes get one length D. The end dashes get D × e1 / d and D × e2 / d. Together the
  dashes and the gaps fill the part: D = (L − n × g) / (n − 1 + e1 / d + e2 / d), where n is the number of gaps.
- Verified: in the PDF exports of the first five samples, OCAD's ends of the dashes on the straight parts are within
  0.021 mm of this rule: the rounding to the grid, and a little more next to a corner point.

### The secondary gaps

- A secondary gap divides a dash between the end dashes, or an end dash, into two pieces (the first sample, rows R3
  and R4).
- OCAD puts a secondary gap only in a dash that is longer than half of d. A dash of exactly half of d gets no
  secondary gap (the third sample, MS2-5).

### The vertex next to the end of a dash

- **A single line.** OCAD leaves out the vertex next to the start or the end of a dash when the piece of the dash
  beyond the vertex is shorter than 0.4 × w, where w is the width of the line. The dash then goes straight from its end
  to the next point. The end of the dash does not move. Verified in the PDF export of the seventh sample: widths of
  0.06 to 1.0 mm and turns of 30° to 150°. OCAD keeps the vertex for a piece of exactly 0.4 × w (40.00 and 12.00
  units).
- **The scope.** The rule does not repeat: after OCAD leaves out a vertex, it keeps the next vertex, also where the
  new first segment is shorter than the limit. The rule does not change the vertices inside a dash, or the vertices
  of a solid line.
- **The join of a straight segment and a curve** is a vertex for the rule. A line of 1.0 mm leaves the join out for a
  piece of 0.1 to 0.3 mm, and keeps it for 0.5 mm. Where a dash leaves out the join at its start, OCAD writes the dash
  as one Bezier piece from its start along the curve (the eighth sample).
- **A double line.** The dash of the middle leaves out the vertex next to its end for a piece up to about 0.05 mm
  (the limit is between 4.47 and 5.83 units). Then each border applies the rule of a single line to its own dash, with
  the width of the border. The piece of the inner border, from its end to the corner of its miter, is
  |piece − h × tan(turn / 2)|, where h is the distance of the border from the path. Verified: the seventh sample.
- **The dashed fill of a double line** follows the rule of a single line with the width of the fill. A fill of
  0.6 mm leaves the vertex out for a piece of 0.20 mm, and keeps it for 0.25 mm. The limit 0.4 × 0.6 mm is 0.24 mm
  (the eighth sample).
- **The bitmap exports** follow another rule on single lines, which depends on the resolution. At 1200 dpi, OCAD draws
  the corner of a line of 1.0 mm from a piece of 0.20 mm, and of a line of 0.3 mm from 0.08 mm. The PDF export leaves
  the vertex out up to 0.30 and 0.11 mm. On the double lines, the bitmaps and the PDF agree (the sixth sample).

### The dashes on Bezier curves

- **The measure.** Each quarter of the curve, from the parameter 0 to 1/4 and so on, has the length of its chord in
  the metric of the map. OCAD fits the dashes and the gaps in this measure.
- **The points in a quarter.** In a quarter, OCAD places the points almost in proportion to the euclidean length of
  the arc. This is so with each metric.
- **The fit.** The table gives the largest distance from OCAD's ends of the dashes, in the PDF exports. "Even" is an
  even placement along the length of the curve in the metric of the map: Purple Pen's placement before fix 021, and
  its present placement with the OCAD metric.

| File | The model | Even |
| --- | --- | --- |
| Ninth sample, euclidean metric | Within 0.05 mm on most curves; 0.1 to 0.2 mm on the high S curve and on the curve with a loop; root mean square 0.02 mm | Up to 0.13 mm on the hooks, 0.28 mm on the high S curve, 0.84 mm on the curve with a loop |
| Ninth sample, OCAD metric | 0.013 to 0.096 mm on 16 curves; 0.175 mm on the curve with a loop | 0.019 to 0.252 mm; 0.794 mm on the curve with a loop |
| Eighth sample, OCAD metric | 0.008 to 0.034 mm | Up to 0.131 mm |
| Tenth sample, euclidean metric: long paths, real paths, straight segments, corner and dash points | Up to 0.045 mm | Up to 0.152 mm |
| Tenth sample, OCAD metric: the same paths | Up to 0.059 mm | Up to 0.269 mm |

[measure-pdf-curves.py](PaletteStack/oracles/ocad-dashes/measure-pdf-curves.py),
[measure-pdf-curves-metric.py](PaletteStack/oracles/ocad-dashes/measure-pdf-curves-metric.py) and
[measure-pdf-paths.py](PaletteStack/oracles/ocad-dashes/measure-pdf-paths.py) give the values. With the OCAD metric,
two other models do not fit the ninth sample: the quarters with their chords and their lengths in the OCAD metric are
0.019 to 0.190 mm from OCAD, and the quarters with their lengths in the OCAD metric and the points in proportion to the
euclidean arc up to 0.219 mm, and 0.813 mm on the curve with a loop. In the tenth sample, the one count that differs
by one dash near a half is left out (see "The number of gaps").

- **What is left.** In each quarter of the curves of the euclidean file, OCAD's points are a little before the model
  in the first half of the quarter, and a little after it in the second half. They are the same at the ends and at the
  middle of the quarter. The difference grows as the square of the turn of the quarter: 0.0025 of the length of the
  quarter at 37.5°, and 0.0035 at 45°. No tested rule gives it: a proportion to the parameter, to the distance from
  the start of the quarter, to the projection on its chord, or to two chords of its halves.
- **A straight Bezier curve.** OCAD follows the true length, whatever the control points (the eighth sample).
- **The output.** OCAD writes a dash on a curve as Bezier pieces of the curve. It splits a dash at the parameters 1/4,
  1/2 and 3/4.
- **The scope.** The eighth and the ninth sample had one Bezier curve in each cell. The tenth sample has paths of
  several curves, curves with straight segments, a closed path, a path with a corner point and a dash point, and three
  real paths of a test map, each with dense dashes, common dashes and spaced glyphs.

## The double lines

- **A solid border** is the offset of the path, with miter joins. Verified: the fourth sample, where OCAD and Purple
  Pen differ only in single pixels at the edges.
- **A dashed border** has the dashes of the middle of the line, moved aside with miter joins. OCAD does not fit
  dashes to the length of each border. Verified: the fourth sample, on half circles, on a closed circle and at plain
  vertices. On the S curve, both borders get the count of the middle.
- **In the PDF export**, each dash of a border is a path of its own: the offset of the dash of the middle, with the
  corner of the miter as a point, stroked with bevel joins and flat ends (the sixth sample).
- **The minimum of gaps.** The dashes of a double line have no minimum of gaps. The minimum of a dashed main line
  does not apply to the dashes of the borders (the second and the third sample).
- **The corners of a closed double line.** On a dashed closed rectangle, OCAD leaves a triangle of about 5 pixels at
  600 dpi open between the fill and the outer border: at the corners where a dash goes around the corner, and at the
  start point (the first sample). On a solid closed rectangle, OCAD leaves the outer corner at the start point open,
  about 0.25 mm, where two flat ends meet (the second sample).

## The glyphs along lines

- **Spaced glyphs** follow the rules of the dashes (the third sample):
  - A Bezier curve is four chords.
  - At exactly a half, the number of glyphs plus one is even. With a glyph every 2.0 mm and 1.0 mm at the ends, lines
    of 9 and 11 mm both get 5 glyphs, and a line of 3 mm gets 1 glyph, at its middle.
  - OCAD reads `MinSym` 2 and 3 as a minimum of 1 glyph.
- **Spaced glyphs on a Bezier curve.** OCAD places them in the measure of the dashes, by the quarters, with each
  metric. On seven S curves of the third sample, this model is within 0.032 mm of OCAD's glyphs (root mean square
  0.013 mm), and even glyphs along the true length within 0.090 mm (root mean square 0.050 mm). On the paths of the
  tenth sample, the model is within 0.039 mm with the euclidean metric and 0.050 mm with the OCAD metric.
- **A circle element of a glyph.** OCAD takes the diameter of a circle from the middle of its line. An OCAD 6 to 8
  file gives the diameter from the outer edges, and OCAD subtracts the width of the line from it. With that diameter,
  a circle whose diameter is larger than the width of its line is a ring. A circle whose line is at least as wide as
  its diameter is a filled circle of that diameter. A circle whose diameter is 0 or less is not drawn. Verified with
  point symbols ([ocad-thick-circles.md](PaletteStack/oracles/ocad-thick-circles.md)). No sample checked the glyphs
  of a line.

## The text along lines

- OCAD measures the text along a line with the OCAD metric, also in a map with the euclidean metric. Verified: in the
  sample, an OCAD 12 file with the euclidean metric, the steps between the letters on lines at 30° and 45° and on a
  zigzag at 33.7°, against the steps on a line along the x axis, are 0.896, 0.943 and 0.901. These are the factors of
  the OCAD metric. The centered, the right aligned and the justified texts agree with the same measure.
- The sample also has a quarter circle and an S curve. OCAD's PDF export gives the places of the letters on them, but
  nobody compared them with Purple Pen's places.

## Not known

- The small difference in each quarter of a curve.
- The rule of the dash on a line that a minimum of 2 gaps leaves without dashes.
- `MinSym` above 3.
- A map with a georeference angle.
- The glyphs of a line at its corner points, its dash points, its start and its end, and at the middles of its dashes
  and its gaps. The glyphs that become smaller, the offset glyphs, and the stretch of a glyph at an acute corner,
  which a comment in Purple Pen's `LineSymDef.DrawGlyphs` describes.
- The shortening and the pointy ends of a line.
- The text along curves.
- Mapper. This document does not describe Mapper. Purple Pen measures Mapper maps with the euclidean metric, and fits
  their dashes as Mapper does (`SpacingMethod.OpenMapperDashes`).

## Purple Pen

Each fix is on a branch of its own, and it is merged into `feature/palette-stack`.

| Rule of OCAD | Purple Pen |
| --- | --- |
| The metric from `p`, also in OCAD 9 and 10 files | Fix [016](PaletteStack/tickets/016-ocad-metric-parameter.md) |
| A Bezier curve as four chords, the half, the short line, the end dashes of 0, the secondary gap | Fix [017](PaletteStack/tickets/017-ocad-dash-count.md) |
| No minimum of gaps for a double line | Fix [018](PaletteStack/tickets/018-double-line-dash-gaps.md) |
| `MinSym` 2 and 3 | Fix [019](PaletteStack/tickets/019-ocad-minimum-gaps.md) |
| The vertex next to the end of a dash | Fix [020](PaletteStack/tickets/020-dash-boundary-at-vertex.md) |
| The quarters of a Bezier curve with the euclidean metric, for the dashes and the spaced glyphs | Fix [021](PaletteStack/tickets/021-ocad-dashes-on-curves.md) |
| The quarters of a Bezier curve with the OCAD metric, with their chords in the OCAD metric | An extension of fix [021](PaletteStack/tickets/021-ocad-dashes-on-curves.md), decided on Nov 19, not done (see below) |
| The circle elements of glyphs | Fix [015](PaletteStack/tickets/015-thick-glyph-circles.md) |
| The text along lines with the OCAD metric | As OCAD already ([022](PaletteStack/tickets/022-line-text-metric.md), closed) |
| The dash on a line that a minimum of 2 gaps leaves without dashes | Not done |
| The open corners of a closed double line | Not done: Purple Pen fills them |

### The OCAD metric on curves (decision, Nov 19)

**The decision.** In a map with the OCAD metric, Purple Pen places the dashes, and the glyphs at lengths along the
line, on a Bezier curve by the model of fix 021: each quarter of the curve has the length of its chord in the OCAD
metric, and in a quarter the points are in proportion to the euclidean length of the arc. The straight segments do not
change. So the drawing changes on the curves of the maps with `p1`, and their baselines move (@Pauli, Nov 19). The
change is an extension of fix 021 (@Pauli, Nov 19), and it is not made yet.

**The reasons:**

1. The model is OCAD's rule, as far as the samples show it (see "The dashes on Bezier curves"). It is nearer to OCAD
   than Purple Pen's present placement with the OCAD metric.
2. The channels of the drawing measure the euclidean length along a path. On a straight segment, an even euclidean
   step gives the points of the OCAD metric exactly, and in a quarter of a curve the model has an even euclidean step.
   So each straight segment and each quarter can be a run of dashes or a repetition of glyphs, which a channel draws
   with its dash pattern or its path effect. This is the measure of the benefit of the palette-stack work: a basic
   case uses an abstraction of the channels (@Pauli, Nov 19).
3. A dash pattern with a table of lengths could keep the present placement exactly, as the dash patterns of SVG, PDF
   and Skia take any list of lengths. But that placement is farther from OCAD, and the 1D path effect of Skia, which
   draws a glyph along a path, takes one step and no table.

**Verified.** The tenth sample confirms the model with the OCAD metric on long paths of several curves, on curves
with straight segments, corner points and dash points, on real paths, and for the spaced glyphs (see "The dashes on
Bezier curves").

[Ticket 011](PaletteStack/tickets/011-line-geometry.md) records the decision and what it changes in the instances.
