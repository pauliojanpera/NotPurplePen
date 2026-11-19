## General description
Pass 1 of the palette-layer drawing makes the clipping objects of lines with a geometry method, in place of the
collector. A dashed line is a composed dashed line: each run of dashes that repeats a pattern is one dashed stroke,
and the other dashes are single dashes. The glyphs along a line share the shapes of their glyph: the evenly spaced
glyphs of each part of a line are one repetition, and the other glyphs are single placements. The collector no longer collects any line, and the instances that it makes of all the symbols stay the
oracle of the tests. The drawing does not change.

## Changes in detail
* `LineLikeSymDef.AddClippingObjects(LayerInstanceBuilder, SymPath)` is the geometry method of the definitions of
  lines, and `LineSymbol.TryAddClippingObjects` calls it. `LineSymDef` adds the main line, its pointy ends, the
  second line, the fill and the borders of a double line, and the glyphs along the line, in the order that `DrawCore`
  draws the parts of a color. `LineComboSymDef` adds the clipping objects of its components, in their order.
* `Stroke` has a dash pattern and a phase: the lengths of the dashes and the gaps in turn, from a dash, and the
  distance that the pattern has gone at the start of the shape, as `stroke-dasharray` and `stroke-dashoffset` of SVG
  and the dash pattern of PDF. A solid stroke has no pattern.
* `SymPath.GetDashes` gives the dashes of the drawing of a dashed line without a drawing target, beside
  `DrawDashedCore` and `DrawDash`, with the vertices that the rule of fix 020 leaves out.
  `LineSymDef.AddDashedLine` composes them: a run of at least two periods of one pattern, of one or two dashes, is
  one stroked shape with the pattern. A dash whose vertex the rule leaves out is a single dash. A run crosses the
  vertices of the path only when the map measures with the euclidean metric and the line is on the path. With the
  OCAD metric, and on the borders of double lines, each run is within one segment.
* `StrokedShape.ExpandDashes` gives the dashes of a dashed stroke as single dashes. Where the pattern and the shape
  end together, within the rounding of lengths, it starts no dash at the end, except a dash of length 0.
* `LineSymDef.AddGlyphs` places the glyphs along a line as `DrawGlyphs` does, beside it. Each run of at least three
  glyphs at equal steps is one repetition (`AddGlyphsAtPoints`), and the other glyphs are single placements.
* `RepeatedShapes` is a clipping object that repeats the shared shapes of a glyph in one palette entry along a path.
  Its shape is its path: the segments of the flattened path from the segment of the first glyph to that of the last
  (`GetSegmentsOfGlyphs`). It has the distance of the first placement from the start of the path, the step, the
  count, the metric, and whether each placement turns the glyph to the direction of the path (`IsTurnedToPath`).
  `Expand` gives the placements, with `FindPointsAlongLine`, as the drawing finds them. `Glyph.AddRepetition` adds a
  repetition for each color of the glyph. A repetition does not turn a glyph of one circle at its origin, because
  `Glyph.DrawSimple` draws that glyph at its location without a transformation. A turned circle has the same
  geometry, but the coverage tests found the turned circles in pixels at their edges.
* `Glyph.AddClippingObjects` takes the extra transformation of a glyph: it moves the points of the parts and scales
  the diameters of the circles, but not the widths of the lines, as `GlyphPart.Draw` does. So a glyph that the line
  stretches at a corner or decreases has shapes of its own, and an extra transformation that is the identity keeps
  the shared shapes. `SymPath.MakeStrokedShape` and `MakeFilledShape` take the transformation of the points.
* The old drawing code does not change. The branch adds methods beside it, and the lines that it removes are the
  geometry code of ticket 008.
* `LayerInstanceComparison` expands each dashed stroke and each repetition of the instances to check before it
  compares them. The points of the expanded dashes agree with the dashes of the drawing to within 0.0001 mm
  (`ExpansionTolerance`), and points nearer each other than that count as one: a run measures its dashes from its own
  start, and the drawing from the start of the line. Each placed shape of a repetition agrees with the drawing in the
  coordinates of the map to within the same distance, because a repetition finds its placements along its own path,
  and the drawing along the line. All the other clipping objects, the single placements of the glyphs too, are
  compared exactly.
* `LayerInstanceCoverageTests` draws the dashes of each dashed stroke and the placed shapes of each repetition.
* `SymbolGeometryTests` compares the instances of pass 1 with those of the collector on each OCAD and Mapper map of
  `skia_render`. Its own tests check each branch of the drawing of solid lines, a composed dashed line, each location
  of the glyphs along a line, the shared and the own shapes of the glyphs, the repetition of the evenly spaced glyphs
  with each metric, the repetition of a glyph of one circle without a turn, and a combined line.

## Issue ticket number
[011-line-geometry](../tickets/011-line-geometry.md)

## Missing functionality
* The dashes on Bezier curves follow the drawing of Purple Pen. With the euclidean metric, OCAD places them by the
  quarters of each curve. Fix [021](../tickets/021-ocad-dashes-on-curves.md) does that, and the runs on curves by
  quarter come with it (question 8 of the ticket).
* The areas, the combinations, the graphics objects, the texts and the rectangles have no geometry methods yet
  ([012](../tickets/012-area-geometry.md) and [013](../tickets/013-text-geometry.md)). The hatchings and the area
  patterns are not in the instances ([009](../tickets/009-pattern-instances.md)), and pass 2 is a later ticket
  ([010](../tickets/010-palette-composition.md)).

## Changes to be logged
Ei käyttäjälle näkyviä muutoksia.
