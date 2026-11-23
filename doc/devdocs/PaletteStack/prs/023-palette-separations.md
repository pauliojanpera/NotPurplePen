## General description
The intermediate layer and pass 1 of the palette-stack drawing. The map makes from the map alone one palette
separation for each palette color (`SymColor`), in palette order. A palette separation has the palette color coverages
of the symbols in its palette color, in the order of the drawing, and its palette cutters group them by their
configuration. A palette color coverage of a symbol is the triple of the symbol, the palette color and the coverage
prototype G, and its clipping objects come from the drawing code of the symbol through a collector, which implements
`IGraphicsTarget`. The only coverage prototype is the full coverage (G = 1): the hatchings and the area patterns are
periodic coverages, and pass 1 leaves them out. The map keeps the palette separations as derived values until it
changes. The drawing does not use them, and it does not change.

## Changes in detail
* `CoveragePrototype.cs`: `CoveragePrototype`, the coverage G before M places it, and `CoveragePrototype.FullCoverage`,
  the whole plane.
* `PaletteSeparations.cs` holds the units of the intermediate layer:
  * `SymbolCoverage`: a symbol, a palette color and a coverage prototype, and its clipping objects as a derived value.
    It exists also when the symbol draws no shape in the palette color.
  * `PaletteSeparation`: the palette color coverages of the symbols in one palette color, in the order of
    `Map.DrawColor`, and its palette cutters as a derived value.
  * `PaletteCutter`: the palette color coverages of one configuration, with its prototype and its transformation M.
    All the affine maps are symmetries of the full coverage, so a palette separation has one palette cutter, whose M
    is the identity.
* `ClippingObjects.cs` holds the forms of the clipping objects, from the old pass 1 of `feature/palette-layers`:
  `FilledShape` with its fill rule, `StrokedShape` with its `Stroke` (width, caps, join and miter limit), the shapes
  `PathShape`, `EllipseShape`, `ArcShape` and `TextShape`, and `Placement`, which keeps the nested transformations of
  a shape in the order in which the symbol set them.
* `ClippingObjectCollector.cs` is the collector. It makes the clipping objects of one palette color coverage at a
  time: it draws the symbol on itself at the resolution 0, so that a dashed line keeps its dashes, and without the
  hatchings and the area patterns (`RenderOptions.omitHatchingsAndPatterns`, internal).
* `Map.PaletteSeparations` is pass 1. It takes the visible symbols of the visible symbol definitions that have a
  palette color, wherever the symbols lie, and no rectangle and no resolution of a drawing.
  `SymDef.GetCoveragePrototypes` gives the coverage prototypes of a symbol in a palette color: an area without its
  hatchings and patterns, and a graphics object by its own colors.
* The derived values: `Map` counts its changes (`NoteChange`), also the removal of a palette color and a change of the
  visibility of a symbol definition, which upstream does not mark. A unit keeps the count at which the map made it,
  and a read of its contents after a later change throws a `MapUsageException`. The first read makes each value,
  `Interlocked.CompareExchange` publishes it once, and the readers within `Map.Read` get the same objects.
* `PaletteSeparationTests` checks the palette separations of maps that the tests make, without a drawing: their order,
  the palette color coverages and their order, the palette cutters, the forms of the clipping objects, and the derived
  values with each kind of change and with eight readers at the same time.
* `PaletteSeparationCoverageTests` draws each palette color of 20 test maps of `skia_render`, and three of them with
  pixels larger than the gaps of their dashes, on a transparent bitmap without antialiasing, draws the union of the
  clipping objects of its palette separation on another, and compares the alpha of each pixel. No pixel differs. The
  comparison leaves out each palette color of a symbol with hatchings or area patterns, and the bottom palette color
  of a map with image objects.

## Issue ticket number
[023-palette-separations](../tickets/023-palette-separations.md)

## Missing functionality
* The hatchings and the area patterns as coverage prototypes, with the normal form of their configurations, are a
  later ticket.
* The geometry methods of the symbols, which make the clipping objects without the drawing code, are later tickets.
* Pass 2, the composition of the palette separations, is a later ticket. The drawing does not use them.

## Changes to be logged
Ei käyttäjälle näkyviä muutoksia.
