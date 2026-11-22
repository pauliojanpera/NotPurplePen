## General description
Pass 1 of the palette-stack drawing. The map model makes the layer instances of its symbols as a data structure, which
is the interface between pass 1 and pass 2. Each palette entry (`SymColor`) that the symbols draw a shape of has one
instance with full coverage (G = 1), and the shapes are the clipping objects of that instance. A collector, which
implements `IGraphicsTarget`, turns the drawing calls of the symbols into the clipping objects. The hatchings and the
area patterns are repetitive geometry, and the instances do not hold them yet. The drawing does not use the
instances, and it does not change.

## Changes in detail
* `LayerInstances.cs` holds the data structure:
  * `GeometryPrototype`, the geometry G that an instance places. `GeometryPrototype.FullCoverage` is G = 1.
  * `LayerInstance`: a palette entry, a prototype, the transformation M, and the clipping objects.
  * `ClippingObject`: a `FilledShape` with its fill rule, or a `StrokedShape` with its `Stroke` (width, caps, joins
    and miter limit). Each has a shape, a placement, and bounds in the coordinates of the map.
  * The shapes: `PathShape`, `EllipseShape`, `ArcShape`, and `TextShape`, a text run with its text, font family, size,
    effects and upper left corner.
  * `Placement`: the transformations that place a shape, in the order that the drawing applied them, and their
    composition. A drawing of the shapes can apply the steps as the symbols did, with the same rounding.
* `LayerInstanceCollector.cs` is the collector. It keeps the paths, pens, brushes and fonts that the symbols create,
  and turns each drawing call into a clipping object at the current placement. An ellipse and an arc stay an ellipse
  and an arc. A rectangle, a polygon, a polyline and a line become a path, as the drawing targets make them. The
  collector does not clip, and it has no pattern brushes and no bitmaps: only the hatchings, the area patterns, and the
  image and layout layers use them.
* `Map.CreateLayerInstances(rect, minResolution)` is pass 1. It draws the symbols of each palette entry on a collector,
  and returns the instances in the order of the palette. It selects the symbols as `Map.DrawColor` does: the visible
  symbols of the visible symbol definitions that may intersect the rectangle. The templates, the image layer and the
  layout layer have no instances.
* `RenderOptions.omitHatchingsAndPatterns` (internal) makes `AreaSymDef` leave out its hatchings and patterns. Pass 1
  sets it. A pattern that keeps whole glyphs draws them without a clip, as a point symbol draws its glyph, so the
  collector cannot tell them from the full coverage.
* `LayerInstanceTests` checks the instances of maps that the tests make, without a drawing: the number of the
  instances, their palette entries, prototype and transformation, and the number, the kinds and the bounds of their
  clipping objects. The maps have lines, double lines, point symbols with circles and arcs, an area with hatchings and
  a pattern, a text with an underline and a framing rectangle, a text along a line, graphics objects, image and layout
  objects, a hidden symbol definition, and a symbol outside the rectangle.
* `LayerInstanceCoverageTests` draws each color of 20 test maps of `skia_render` on a transparent bitmap without
  antialiasing, draws the clipping objects of the color's instance on another, and compares the alpha of each pixel.
  No pixel differs. The comparison leaves out each color of a symbol with hatchings or area patterns, and the bottom
  color of a map with image objects, whose drawing includes the image layer.

## Issue ticket number
[005-layer-instances](../tickets/005-layer-instances.md)

## Missing functionality
* The hatchings and the area patterns are not in the instances. As geometry prototypes with their lattice and element
  transformations, they are the next ticket.
* Pass 2, the composition of the instances by palette entry, is a later ticket. The drawing does not use the instances.

## Changes to be logged
Ei käyttäjälle näkyviä muutoksia.
