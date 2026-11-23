## General description
The geometry methods of the line symbols. `LineSymDef` and `LineComboSymDef` give the clipping objects of the
palette color coverages of their symbols with a geometry method, without a drawing target and without render
options, so pass 1 collects no line symbol. A dashed line is a composed dashed line: runs of dashes, whose pen has a
dash pattern measured as the channels measure it, and single dashes. A border of a double line is an offset path. The
evenly spaced glyphs along a line are a repeated placement of the shared shapes of the glyph. The collector stays the
oracle of the tests, and the source of the clipping objects of the areas, the graphics objects and the texts. The
drawing does not change.

## Changes in detail
* `Placement` is abstract. `SinglePlacement` holds nested affine transformations (`Placement.Identity`, `Within`,
  `Matrix`). `RepeatedPlacement` holds a path, the first position, the step and the count along it, and whether its
  poses turn to the path (`IsTurnedToPath`); `Expand` gives its single placements. `Placement.Poses` gives the poses,
  each a point and an angle (`Pose`), as `SymPath.FindPointsAlongLine` finds them.
* `Stroke` has a dash pattern and a dash phase (`DashPattern`, `DashPhase`), euclidean along the path of the shape,
  as SVG's `stroke-dasharray` and `stroke-dashoffset`, the dash pattern of PDF and the dash effect of Skia measure
  them. `StrokedShape.ExpandDashes` expands a run of dashes into single dashes.
* `OffsetPathShape` is an offset path: a piece of the base path (`BasePath`), the offset to the right (`Offset`) and
  the miter limit (`MiterLimit`). Its offset curve (`OffsetCurve`) is the offset of the drawing (`SymPath.OffsetRight`).
  The dash pattern of an offset path is measured along the base path, so the dashes of a border are the dashes of the
  base path moved aside.
* `LineSymDef.MakeClippingObjects(SymPath, SymColor)` follows `LineSymDef.DrawCore`: the main line on its pieces after
  the shortening, the pointy ends as filled shapes, the second line and the fill of a double line on the pieces
  without the shortening, the left and the right border on their sub-paths, and the glyphs along the line. It takes
  any base path, so that the borders of areas can use it.
* `AddDashedLine` takes the dashes of the drawing from `SymPath.GetDashes`, with the rule of fix 020, and
  `FindRunOfDashes` collects them into runs: consecutive dashes of one region (`SymPath.GetEdgeRegions`) whose
  pattern repeats at least twice, without an end dash, a dash that fix 020 changed, or a dash over a corner or dash
  point. A region ends at the corner and dash points, at the quarters of a Bezier curve with the OCAD spacing, and at
  the straight segments with the OCAD metric, where the pattern is scaled by the ratio of the euclidean length to the
  OCAD length of the segment (`GetRatioOfEdge`).
* `AddGlyphs` follows `DrawGlyphs`, and `AddGlyphsAtPoints` collects the glyphs of one region at one step into a
  repeated placement (`Glyph.MakeRepeatedClippingObjects`). The glyphs at the corners, the dash points, the ends, a
  glyph without a neighbour at the same step, and the glyphs of `SpacedDecrease` are single placements. The stretched
  and the decreasing glyphs get shapes of their own from the extra transformation of the glyph.
* `LineComboSymDef` gives the clipping objects of its component definitions in their order.
* `SymbolCoverageComparison` expands the runs of dashes, the offset paths and the repeated placements, and compares the
  expanded clipping objects in the coordinates of the map within 0.0001 mm, and those of an offset path within
  0.01 mm. `GeometryMethodsMakeTheClippingObjectsOfTheCollector` covers the 162 maps of `skia_render` and the 10 dash
  samples (`dashes-*.ocd`), and no coverage differs. `SymbolGeometryTests` checks the structure of the lines without
  the drawing, and the tests of ticket 023 check the new forms in the palette separations.

## Issue ticket number
[011-line-geometry](../tickets/011-line-geometry.md)

## Missing functionality
* The geometry methods of the areas, the graphics objects and the texts are the drafts 012 and 013. The collector
  stays the source of their clipping objects until @Pauli approves the drafts as tickets and they are made.
* The units of the structure of a line and the other channel conversions of the lines come with the encodings.
* The hatchings and the area patterns as coverage prototypes are a later ticket, and so is pass 2.

## Changes to be logged
Ei käyttäjälle näkyviä muutoksia.
