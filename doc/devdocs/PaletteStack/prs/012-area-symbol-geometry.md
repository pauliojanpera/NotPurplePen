## General description
The geometry methods of the area symbols and the graphics objects. `AreaSymDef`, `AreaComboSymDef` and
`GraphicsSymDef` give the clipping objects of the palette color coverages of their symbols with a geometry method,
without a drawing target and without render options, so pass 1 collects no area symbol and no graphics object. An
area gives its area fill as a filled shape of its path with holes and its area border with the geometry method of the
lines, along the main path and then along each hole. A combined area gives the clipping objects of its component
definitions in their order. The hatchings and the area patterns are periodic coverages and make no clipping objects
here. The collector stays the oracle of the tests, and the source of the clipping objects of the texts and the
rectangles. The drawing does not change.

## Changes in detail
* `AreaLikeSymDef` overrides `SymDef.MakeClippingObjects(Symbol, SymColor, CoveragePrototype)` and gives the path
  with holes of the area symbol to the internal abstract `MakeClippingObjects(SymPathWithHoles, SymColor)`.
* `AreaSymDef` gives, as `AreaSymDef.Draw` draws them, the area fill in its color, a filled shape of the main path and
  its holes with the alternate fill rule (`SymPathWithHoles.MakeFilledShape`, as `SymPathWithHoles.Fill` fills it),
  and the area border, the clipping objects of its line symbol definition with the main path and then each hole as
  the base path.
* `AreaComboSymDef` gives the clipping objects of its component definitions in their order, as
  `AreaComboSymDef.Draw` draws them: a line symbol definition along the main path and then along each hole, and an
  area symbol definition with its own geometry method for the same path with holes. The Mapper import makes an
  `AreaComboSymDef` whose component definition is a `LineComboSymDef` or another `AreaComboSymDef`
  (`OpenMapperImport.DelayedComboSymDef.CreateSymDef`), so both kinds are covered.
* `LineLikeSymDef` has the internal abstract `MakeClippingObjects(SymPath, SymColor)`, the clipping objects of a line
  along any base path. `LineSymDef` implements it as before, and `LineComboSymDef` gives the clipping objects of its
  component definitions in their order.
* `GraphicsSymDef` gives the fill of a `GraphicsAreaSymbol` in its color as a filled shape of its path with holes, and
  the line of a `GraphicsLineSymbol` in its color as a stroked shape of its path with its width, caps and join and the
  miter limit `GraphicsUtil.MITER_LIMIT`, as `GraphicsLineSymbol.Draw` creates its pen.
* `SymbolGeometryTests`: the comparison with the collector covers the area symbols and the graphics objects of the
  162 maps of `skia_render` and the 10 dash samples, and no palette color coverage differs. New tests check the
  branches of the area fill, the area border along the main path and each hole (solid, dashed, as a double line and
  with glyphs), the order of the component definitions of a combined area with a `LineComboSymDef` and another
  `AreaComboSymDef`, the palette colors of the hatchings and the area patterns alone, the graphics objects, and which
  class implements the geometry method of each kind of symbol definition.

## Issue ticket number
[012-area-geometry](../tickets/012-area-geometry.md)

## Missing functionality
* The geometry methods of the texts, the texts along lines and the rectangles are the draft 013. The collector stays
  the source of their clipping objects until @Pauli approves the draft as a ticket and it is made.
* The hatchings and the area patterns as coverage prototypes are the draft 009, and pass 2 is the draft 010.

## Changes to be logged
Ei käyttäjälle näkyviä muutoksia.
