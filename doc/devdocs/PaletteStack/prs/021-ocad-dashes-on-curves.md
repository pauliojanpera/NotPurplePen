## General description
The OCAD 2020 viewer places the dashes of a line on a Bezier curve, with the euclidean metric, otherwise than
`LineSymDef.ComputeDashDistances` did. It fits the dashes to a length in which each quarter of the curve, between its
points at the parameters 0, 1/4, 1/2, 3/4 and 1, is its chord, and within each quarter it places them in proportion to
the length of the quarter. Purple Pen fitted them to the length of the flattened curve and placed them evenly along
it, up to 0.84 mm from OCAD's dashes. OCAD places the glyphs that it spaces along a curve in the same measure. Purple
Pen now places the dashes and the glyphs of the OCAD spacing by the quarters, with each metric.

## Changes in detail
* `SymPath.GetOcadCurveMeasure` gives the measure of a path with Bezier curves (`OcadCurveMeasure`): a straight
  segment has its length, and each quarter of a curve has its chord. It gives the measure of each part between corner
  and dash points, and converts distances of the measure to distances along the flattened path, linearly between the
  ends of the quarters. The path keeps the measure after its first use, as it keeps its flattened points.
* `ComputeDashDistances` fits the dashes and the glyphs of `SpacingMethod.OCAD` to the measure of each part when the
  metric is euclidean, and converts the distances at the end. So `DrawDashed` and `FindPointsAlongLine` walk the
  flattened path as before, and the glyphs at the centers of the dashes and the gaps stay at those centers. Mapper's
  spacing and the OCAD metric do not change.
* `Bezier.Flatten` can give the indexes of its points at the parameters 1/4, 1/2 and 3/4. The flattening does not
  change. Where a curve is flat enough to have no point there, the measure takes the nearest point of the segment that
  passes it.
* `SymPath.GetQuarterEnds` gives the points of the chords, which the count of the dashes of fix 017
  (`GetCornerAndDashPointLengthsForCounting`) uses too.
* `OcadDashesOnCurvesTests` compares the dashes of the 17 Bezier curves of the ninth dash sample, and the glyphs of the
  seven S curves of the third, with OCAD's PDF exports of them. Each end of a dash is within 0.06 mm of OCAD's end,
  within 0.11 mm on the high S curve and within 0.21 mm on the loop, as far as the model of the quarters fits OCAD, and
  each glyph within 0.04 mm of OCAD's glyph. Without the change, 10 of the 17 cases of the dashes fail, and all 7 of the
  glyphs. `OcadSampleDrawing.DrawPolylines` and `DrawFilledEllipseCenters` keep the polylines and the filled ellipses
  that the drawing draws.
* 11 baselines move: 5 of Skia (`clouds11`, `LordHill OCAD11`, `marymoor11`, `marymoor11_upperlayers` and
  `teanwest11`), 4 of the GDI+ renderings of `Test_MapModel` and 2 of PDF (collecting run 44532214497). They are OCAD
  11 maps, which measure the lines with the euclidean metric. No baseline of `base` moves beyond those of fix 017, so
  the full run takes `prs/017-ocad-dash-count-base/` with `push-wip.sh --base-baselines`. Full run passed at dc76f6c6
  on `base` (Tests 44535962486, Build 44535962339).

* The extension to the OCAD metric (e56a67c4, @Pauli, Nov 19): `SymPath.GetOcadCurveMeasure` takes the metric of the
  map, and measures each quarter and, with the OCAD metric, each straight segment by its chord in that metric. The
  distances are converted to euclidean lengths along the flattened path within each quarter and straight segment, so
  the drawing walks a path with Bezier curves with the euclidean metric (`LineSymDef.GetMetricAlongPath`). Paths
  without Bezier curves, the shortening and the pointy ends do not change. A shortened path with the OCAD metric finds
  its glyphs along itself.
* `OcadDashesOnCurvesTests` also compares the dashes of the 17 curves of the ninth sample, and the dashes and the glyphs
  of the paths of the tenth sample, in their OCAD metric files with OCAD's PDF exports: within 0.11 mm, 0.19 mm on the
  loop, 0.07 mm on the paths of the tenth sample and 0.06 mm for its glyphs. Without the extension, 25 of the 71 cases
  of the test fail. `oracles/ocad-dashes/export-ocad-ends.py` writes the data of the exports.
* The extension moves 137 baselines on the curves of the maps that the OCAD metric measures, OCAD 6 to 10 files among
  them (collecting run 45159058675): 18 of Skia, 25 of `Test_MapModel`, 20 of PDF, 9 of WPF and 65 of
  `PurplePen_Tests`. It moves three route gadget baselines of `base`, so the full run takes
  `prs/021-ocad-dashes-on-curves-base/`. Full run passed at a288eaf8 on `base` (Tests 45161013437, Build 45161013427).

## Issue ticket number
[021-ocad-dashes-on-curves](../tickets/021-ocad-dashes-on-curves.md)

## Missing functionality
* Within each quarter, OCAD's dash ends are a little before the model in the first half of the quarter, and a little
  after it in the second half. No tested rule gives that difference ([oracles/ocad-dashes.md](../oracles/ocad-dashes.md)).
* The branch is rooted at `fix/ocad-dash-count` ([017](../tickets/017-ocad-dash-count.md)), as it uses its measure of
  four chords, and is offered upstream after it (@Pauli, Nov 19).

## Changes to be logged
OCAD-viivan katkot ja viivan varren glyyfit sijoittuvat Bezier-käyrälle kuten OCADissa kummallakin metriikalla.
