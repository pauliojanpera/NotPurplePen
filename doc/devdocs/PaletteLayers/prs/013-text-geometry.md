## General description
Pass 1 of the palette-layer drawing makes the clipping objects of texts, of texts along lines and of rectangles with
geometry methods. With them, every kind of symbol has a geometry method, and pass 1 no longer uses the collector. The
instances that the collector makes of all the symbols stay the oracle of the tests. The drawing does not change.

## Changes in detail
* `TextSymDef.AddClippingObjects` adds the clipping objects of a text as `Draw` draws them, beside it and with the
  same helpers. Each word, or each grapheme where the character spacing is more than 0, is a text run at its own
  position, with its shadow or its line framing. The underlines of each line, the center point and the framing
  rectangle follow in the order of `Draw`, at the placement that `Draw` pushes.
* The line framing of a text run has the stroke of each pen of the drawing, as the collector has them. Question 2 of
  ticket 013 decides whether the instances hold one line instead.
* `TextSymDef.AddClippingObjectsOfTextOnPath` adds each grapheme of a text along a line, at the placement of the
  grapheme, as `DrawTextOnPath` draws it.
* `RectangleSymDef.AddClippingObjects` adds the outline, the grid lines and the grid texts of a rectangle.
* `Symbol.AddClippingObjects` is abstract, and every kind of symbol has it. The image symbols draw in the image or the
  layout layer, which is no palette entry, and a hole draws nothing, so their methods add nothing.
  `Map.CreateLayerInstances` calls the method of each visible symbol, and no longer uses the collector.
* `PointSymDef.AddClippingObjects` and `Glyph.AddClippingObjects` take the placement within which a glyph is, as the
  center point of a text is within the placement of the text.
* `PathShape.FromRectangle` gives the corners of a rectangle in the order of the collector, which now uses it too.
* The old drawing code does not change. The branch adds methods, and the lines that it removes are the geometry code
  of tickets 008 and 011.
* `SymbolGeometryTests` compares the instances of pass 1 with those of the collector on each OCAD and Mapper map of
  `skia_render`, now for all the symbols.

## Issue ticket number
[013-text-geometry](../tickets/013-text-geometry.md)

## Missing functionality
* The branch is rooted at `feature/area-geometry` ([012](../tickets/012-area-geometry.md)), which is rooted at
  `feature/line-geometry` ([011](../tickets/011-line-geometry.md)), as neither is in `feature/palette-layers` yet.
* The hatchings and the area patterns are not in the instances ([009](../tickets/009-pattern-instances.md)), and pass 2
  is a later ticket ([010](../tickets/010-palette-composition.md)).

## Changes to be logged
Ei käyttäjälle näkyviä muutoksia.
