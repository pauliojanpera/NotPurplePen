## General description
The OCAD 2020 viewer counts the dashes of a line, and the glyphs spaced along it, otherwise than
`LineSymDef.ComputeDashDistances` did. It counts them from a length in which each Bezier curve is four chords and
each part is rounded to 0.01 mm, gives the even number of dashes at a half, gives a short line a gap only past its
limit, draws a line whose one gap would leave end dashes of 0 as solid, and puts a secondary gap only in a dash longer
than half of a dash. Purple Pen now counts them in the same way, for the OCAD spacing of dashes and glyphs.

## Changes in detail
* `SymPath.GetCornerAndDashPointLengthsForCounting` gives the length of each part between corner and dash points as
  OCAD measures it to count the dashes: each straight segment, and each Bezier curve as four chords between its
  points at the parameters 0, 1/4, 1/2, 3/4 and 1, measured with the metric of the map and rounded to 0.01 mm.
* `ComputeDashDistances` counts the gaps of each part from that length, and still fits the dashes to the length of
  the flattened path. It rounds the number of dashes, one more than the number of gaps, to the even number at a half
  (`MidpointRounding.ToEven`). It gives a short line a gap only when the line is longer than half of each end dash
  and a gap. It draws a dashed line whose one gap would leave end dashes of 0 as solid. It puts a secondary gap only
  in a dash longer than half of a dash. These apply to `SpacingMethod.OCAD`, also for spaced glyphs, and not to
  Mapper's spacing.
* `OcadDashCountTests` counts the dashes and the glyphs of 28 cells of the dash samples and compares them with the
  drawing of the OCAD viewer. Without the change, 21 of its cases fail, and its 7 cases of comparison pass.
  `OcadSampleDrawing` reads the sample files and counts the dashes along a line.
* 107 baselines move: 15 of Skia, 18 of the GDI+ renderings of `Test_MapModel`, 14 of PDF, 4 of WPF and 56 of
  `PurplePen_Tests` (collecting run 44414319636). Two baselines that `base` added with
  `fix/lossless-export-comparison`, `src/TestFiles/routegadget/exportautodpi1_baseline.png` and
  `exportautodpi2_baseline.png`, move too. This branch is rooted at upstream and cannot hold them. They are in
  `prs/017-ocad-dash-count-base/` for the merge into `base`, and `push-wip.sh --base-baselines` writes them into the
  wip commit.

## Issue ticket number
[017-ocad-dash-count](../tickets/017-ocad-dash-count.md)

## Missing functionality
* A line that a minimum of 2 gaps leaves without dashes gets one dash of 0.5 mm in OCAD and nothing in Purple Pen.
  The samples do not tell OCAD's rule.
* On strongly curved Bezier curves, OCAD's dashes are up to 0.17 mm from Purple Pen's, where their numbers are the
  same.

## Changes to be logged
OCAD-viivan katkojen ja viivan varren glyyfien määrä lasketaan kuten OCADissa, myös Bezier-käyrillä.
