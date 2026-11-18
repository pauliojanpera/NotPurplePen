## General description
Pass 1 of the palette-layer drawing makes the layer instances from the map alone. `Map.CreateLayerInstances` takes
no rectangle and no resolution of a drawing. The instances hold each visible symbol of the map, wherever it lies, and
a dashed line keeps its dashes whatever their lengths. The collector stays the source of the instances. The drawing
does not use the instances, and it does not change.

## Changes in detail
* `Map.CreateLayerInstances()` has no parameters. Its result depends on the colors, the symbol definitions, the
  symbols, the visibility and the text metrics of the map alone.
* `Map.SelectVisibleSymbolsOfColor` selects the visible symbols of the visible symbol definitions that have a color,
  in the order that `Map.DrawColor` draws them. It does not use `BoundingBox` or `MayIntersectRect`. `Map.DrawColor`
  keeps its own selection.
* Pass 1 gives the drawing code of the symbols the resolution 0. Of the parts that pass 1 draws, only
  `LineSymDef.DrawDashedWithOffset` reads the resolution: it draws a dashed line solid when its gaps are shorter than
  the resolution. At the resolution 0, it draws the dashes.
* `LayerInstanceCoverageTests` draws its oracle, the current drawing of one color, at the resolution 0, and compares
  it with the instances of the whole map. The bitmap of the area of the test map limits the drawing of the instances.
  The comparison is exact on the 20 maps of the earlier comparison. None of these maps has a dashed line whose gaps
  are all shorter than its pixel, so the resolution 0 does not change their drawing.
* `LayerInstanceCoverageTests.ClippingObjectsCoverTheDashesOfGapsShorterThanAPixel` renders dashline, isomlines and
  doublelines9 with pixels of 0.3, 0.4 and 0.8 mm, larger than their gaps: main gaps, secondary gaps and the gaps of
  double lines. Each case first checks that the drawing at the size of the pixel differs from the drawing at the
  resolution 0, and then compares the coverage of all colors. The three cases fail when pass 1 gets a resolution of
  1 mm.
* `LayerInstanceTests.InstancesHoldTheVisibleSymbolsWhereverTheyLie` checks that the instances hold a symbol 6.4 m
  away from the others on the paper of the map. `DashedLineIsItsDashesAlsoWhenItsGapsAreShorterThanAPixel` checks
  that a line of 10 mm with gaps of 0.01 mm is 10 dashes, with a gap between each two of them. A pixel of a print at
  1200 dpi is 0.021 mm.

## Issue ticket number
[007-map-instances](../tickets/007-map-instances.md)

## Missing functionality
* The collector makes the instances from the drawing calls of the symbols. Geometry methods of the symbols that make
  the same instances are a later ticket.
* The map does not keep its instances, and does not update them when it changes. These are for the ticket of pass 2.
* The hatchings and the area patterns are not in the instances.
* Pass 2, the composition of the instances by palette entry, is a later ticket. The drawing does not use the
  instances.

## Changes to be logged
Ei käyttäjälle näkyviä muutoksia.
