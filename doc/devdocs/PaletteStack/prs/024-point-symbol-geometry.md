## General description
The geometry methods of the point symbols. A symbol definition gives the clipping objects of the palette color
coverages of its symbols with a geometry method, without a drawing target and without render options. The point
symbol definitions have one: they give the parts of their glyph in the palette color, and the shapes of the parts are
a derived value of the glyph, which the symbols share. The other kinds of symbols get their clipping objects from the
collector, as in ticket 023, and the collector is the oracle of the tests. The drawing does not change.

## Changes in detail
* `SymDef.MakeClippingObjects(Symbol, SymColor, CoveragePrototype)` (internal) is the interface of the geometry
  methods. Its implementation in the base class gives the clipping objects of the collector, through
  `SymDef.CollectClippingObjects` (internal), which the tests use as the oracle. `SymbolCoverage.ClippingObjects`
  calls the symbol definition in place of the collector.
* `PointSymDef` overrides it with the geometry method of its glyph, `Glyph.MakeClippingObjects`. It follows
  `Glyph.Draw` and `DrawSimple`, with the filled circles of fix 015: the parts of the glyph in the palette color, in
  the order of the parts, placed with one step that turns the glyph about its origin by the angle of the symbol and
  moves the origin to the location of the symbol.
* The shared shapes: `Glyph.GetSharedClippingObjects` makes the clipping object of each part at the identity
  placement once, and `Interlocked.CompareExchange` publishes them. Each symbol places them with
  `ClippingObject.WithPlacement` (internal). There are two exceptions, as in the drawing. A glyph of one circle at
  its origin, without gaps, is at the location of the symbol without a transformation. The arcs that the gaps of a
  symbol leave of a circle are shapes of the symbol, with the pen of the shared circle.
* `GlyphPart.MakeClippingObject`, `MakeClippingObjectOfSimpleGlyph` and `AddArcs` make the shapes of the parts.
  `SymPath.MakeStrokedShape`, `SymPath.MakeFilledShape` and `SymPathWithHoles.MakeFilledShape` make the paths as
  `SymPath.Draw` and `FillWithHoles` draw them, and `GraphicsUtil.MakeSolidStroke` makes the pen of
  `CreateSolidPen`. The code is that of the old pass 1 of `feature/palette-layers`, for one palette color at a time.
* `SymbolGeometryTests` compares, on each of the 162 OCAD and Mapper maps of `skia_render`, the clipping objects of
  each palette color coverage with those of the collector, as structures (`SymbolCoverageComparison`): the same
  kind, shape, pen or fill rule and steps of the placement, each number exactly. The maps have 12,311 point symbols
  and 59,197 palette color coverages, of which 14,303 are of point symbols. No coverage differs. Its other tests check
  the shared shapes, also over a change of the map, the two exceptions, and each branch of the drawing of a glyph. The
  fixture lets `RenderingTests` add its fonts when it starts, as both sides of the comparison measure the texts.

## Issue ticket number
[024-point-symbol-geometry](../tickets/024-point-symbol-geometry.md)

## Missing functionality
* The geometry methods of the lines, the areas and the texts are the drafts 011 to 013. The collector stays the
  source of their clipping objects until @Pauli approves the drafts as tickets and they are made.
* The hatchings and the area patterns as coverage prototypes are a later ticket.
* Pass 2, the composition of the palette separations, is a later ticket. The drawing does not use them.

## Changes to be logged
Ei käyttäjälle näkyviä muutoksia.
