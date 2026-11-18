## General description
Pass 1 of the palette-layer drawing makes the clipping objects of point symbols with a geometry method, in place of
the collector. The geometry method of a kind of symbol adds the clipping objects of a symbol to each of its palette
entries, without a drawing target and without render options. The point symbols of a definition share the shapes of
its glyph, each at its own placement. The collector collects the other kinds of symbols, and the instances that it
makes of all the symbols are the oracle of the tests. The drawing does not change.

## Changes in detail
* `Symbol.TryAddClippingObjects(LayerInstanceBuilder)` is the geometry method of a kind of symbol. The base
  implementation adds nothing and returns false, and pass 1 then collects the drawing calls of the symbol.
* `LayerInstanceBuilder` holds the clipping objects of pass 1 by palette entry. It makes one instance with full
  coverage (G = 1) for each palette entry that has clipping objects, in the order of the palette.
* `Map.CreateLayerInstances` goes through the symbols once, in the order that `Map.DrawColor` draws them. A symbol
  with a geometry method adds all its palette entries in one call. The collector collects each palette entry of the
  other symbols into the same builder, so the clipping objects of a palette entry stay in their order.
* `Map.CollectLayerInstances` (internal) makes the instances with the collector alone, one palette entry at a time.
  It is the oracle of the geometry methods. The collector adds to the builder, and gives the drawing code of the
  symbols its own render options (`LayerInstanceCollector.Collect`).
* `PointSymbol`, `PointSymDef` and `Glyph` have the geometry method of point symbols. It follows the conditions of
  `Glyph.Draw`: lines, areas, circles and filled circles, the arcs that the gaps of a symbol leave of a circle, and a
  glyph of one circle at its origin, which is at the location of the symbol without a transformation.
* A glyph makes the clipping objects of its parts once, in its own coordinates, and each symbol places them in one
  step (`ClippingObject.WithPlacement`). The arcs and the circle of a glyph of one circle at its origin are shapes of
  their symbol.
* `SymPath.MakeStrokedShape`, `SymPath.MakeFilledShape`, `SymPathWithHoles.MakeFilledShape` and
  `GraphicsUtil.MakeSolidStroke` give the shapes and the lines that the drawing calls make. The drawing code stays as
  it is.
* The `InternalsVisibleTo` of MapModel names `Map_Skia.Tests`.
* `LayerInstanceComparison`, in `Map_Skia.Tests`, compares two lists of instances as structures: the palette entries,
  the prototypes and the transformations, and the kind, the shape, the line or the fill rule and the steps of the
  placement of each clipping object, in order, each number exactly.
* `SymbolGeometryTests` compares the instances of pass 1 with those of the collector on each OCAD and Mapper map of
  `skia_render`, 162 maps, and on a map of the branches of `Glyph.Draw`. No difference is allowed, and there is none.
  Its other tests check that the point symbols of a definition share the shapes of its glyph, and the two exceptions.

## Issue ticket number
[008-symbol-geometry](../tickets/008-symbol-geometry.md)

## Missing functionality
* The lines, the areas, the combinations, the graphics objects, the texts and the rectangles have no geometry methods
  yet, and the collector collects them. They are later tickets: [011](../tickets/011-line-geometry.md),
  [012](../tickets/012-area-geometry.md) and [013](../tickets/013-text-geometry.md).
* The hatchings and the area patterns are not in the instances ([009](../tickets/009-pattern-instances.md)).
* Pass 2, the composition of the instances by palette entry, is a later ticket. The drawing does not use the
  instances.

## Changes to be logged
Ei käyttäjälle näkyviä muutoksia.
