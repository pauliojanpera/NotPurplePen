## General description
The geometry methods of the texts and the rectangles. `TextSymDef` and `RectangleSymDef` give the clipping objects of
the palette color coverages of their symbols with a geometry method, without a drawing target and without render
options. `SymDef.MakeClippingObjects` becomes abstract, so each symbol definition has a geometry method, and pass 1
does not use the collector. The innermost step of the placement of a text is its pose, its location and its angle.
The drawing lays out a text with a mirror of the Y axis, and that mirror is a part of the text run. The collector
stays the oracle of the tests. The drawing does not change.

## Changes in detail
* `SymDef.MakeClippingObjects(Symbol, SymColor, CoveragePrototype)` is abstract. `HoleSymDef` and `ImageSymDef` give
  no clipping objects, as they draw no palette color. `GraphicsSymDef` throws `ArgumentException` for a symbol that
  is not a graphics object, and does not give the clipping objects of the collector.
* `TextSymDef` gives the clipping objects of a `TextSymbol` as `Draw` draws them, and of a `LineTextSymbol` as
  `DrawTextOnPath` draws them. Each word, or each grapheme where the character spacing is more than 0, is a text run
  in the font of the text without its underline effect: in the font color, and in the color of the framing as one
  framing line of the full width or as a shadow. The underline and the underline effect of the font are stroked
  lines. The center point is the glyph of its point symbol definition, within the pose of the text and a translation
  to the center point. The framing rectangle is a filled rectangle.
* The pose of a text: the innermost step of the placement of a text is `Translate(Location)` and
  `RotateAt(Rotation)`. For a grapheme of a text along a line, it is the start point of the grapheme on the path and
  the direction to its end point. The shapes are in the coordinates of the pose, whose Y axis points up. So the top
  left corner of each text run, and the points of the underlines and of the framing rectangle, have the opposite y
  coordinates of the drawing. `TextShape` measures its bounds below its top left corner.
* `RectangleSymDef` gives, as `Draw` draws them, the line of the rectangle and the lines and the texts of its grid.
  The line is a stroked path that keeps the Bezier curves of its rounded corners. The grid is within the location and
  the rotation of the rectangle. A text of the grid has the pose of its place in its cell, and the top left corner of
  its run is at the origin of the pose.
* `Glyph.MakeClippingObjects` takes an outer placement, within which the glyph of a center point is placed.
  `TextSymbol.WrappedLineWidths` gives the widths of the lines to the geometry method. `PathShape.GetCorners` gives the
  corners of a rectangle in the order of the targets, to the collector and to the framing rectangle.
* `SymbolGeometryTests`: the comparison with the collector moves the mirror of the steps of the collector into its
  shapes, and keeps the last of the consecutive strokes of a text run, the framing line of the full width. It covers
  the 162 maps of `skia_render` and the 10 dash samples, with 1,671 text symbols, 86 texts along lines and 175
  rectangle symbols, and no palette color coverage differs. Each placement gives its poses. New tests check the text
  runs and their places in the coordinates of the pose, the character spacing, the framing line, the shadow, the
  underlines, the framing rectangle, the center point, the graphemes of a text along a line with their poses, the
  rectangles, and the geometry method of each class of symbol definitions.
* `PaletteSeparationCoverageTests` draws a text run with the Y axis mirrored, as the drawing lays out the text.

## Issue ticket number
[013-text-geometry](../tickets/013-text-geometry.md)

## Missing functionality
* The hatchings and the area patterns as coverage prototypes, and the end of the use of the collector as the oracle,
  are the draft 009. Pass 2 is the draft 010.

## Changes to be logged
Ei käyttäjälle näkyviä muutoksia.
