## General description
Pass 1 of the palette-stack drawing makes the clipping objects of areas, of combined areas and of graphics objects
with geometry methods, in place of the collector. The border of an area is a line, and the geometry method of lines
makes it. The hatchings and the patterns are repetitive geometry, and pass 1 still leaves them out. The instances
that the collector makes of all the symbols stay the oracle of the tests. The drawing does not change.

## Changes in detail
* `AreaLikeSymDef.AddClippingObjects(LayerInstanceBuilder, SymPathWithHoles)` is the geometry method of the
  definitions of areas, and `AreaSymbol.TryAddClippingObjects` calls it.
* `AreaSymDef` adds the fill of the path with its holes, and the border along the path and along each of its holes,
  with the geometry method of lines (`LineLikeSymDef.AddClippingObjects`), as `Draw` draws them. It adds no hatching
  and no pattern, as pass 1 leaves them out (`RenderOptions.omitHatchingsAndPatterns`).
* `AreaComboSymDef` adds the clipping objects of its components, in their order. A line component goes along the
  path and along each of its holes, as `Draw` draws it.
* `GraphicsAreaSymbol` adds the fill of its path in its fill color, and `GraphicsLineSymbol` the line of its path
  with its own pen.
* The old drawing code does not change. The branch adds methods only.
* `SymbolGeometryTests` compares the instances of pass 1 with those of the collector on each OCAD and Mapper map of
  `skia_render`. Its new test checks each branch of the drawing of areas and graphics objects.

## Issue ticket number
[012-area-geometry](../tickets/012-area-geometry.md)

## Missing functionality
* The branch is rooted at `feature/line-geometry` ([011](../tickets/011-line-geometry.md)), as the borders of areas
  use the geometry method of lines, and ticket 011 is not in `feature/palette-layers` yet.
* The hatchings and the area patterns are not in the instances ([009](../tickets/009-pattern-instances.md)).
* The texts and the rectangles have no geometry methods yet ([013](../tickets/013-text-geometry.md)), and the
  collector collects them. Pass 2 is a later ticket ([010](../tickets/010-palette-composition.md)).

## Changes to be logged
Ei käyttäjälle näkyviä muutoksia.
