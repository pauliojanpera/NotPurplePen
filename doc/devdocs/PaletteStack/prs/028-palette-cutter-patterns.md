## General description
Where areas of one configuration overlap or touch, the Skia encoding wrote the hatching or the clipped area pattern of
each area apart, so an antialiased drawing made the pattern darker where the areas overlap, and an unturned pattern
brush left a lighter seam where they touch. A palette cutter of a hatching or a clipped area pattern now gives its area
clusters, the areas whose bounding boxes overlap or touch, and the union of each cluster. The clusters and their unions
are derived values of the intermediate layer, the same for all channels. The Skia encoding writes the pattern of a
cluster of several areas once, in the union of its areas.

## Changes in detail
* `PaletteCutter.AreaClusters` gives the area clusters (`AreaCluster`) of a palette cutter of a hatching or a clipped
  area pattern. The bounding boxes of the areas are connected as `OverlappingAreas` of `fix/overlapping-patterns`
  connects them, and an area whose contours meet is a cluster of its own.
* `AreaCluster.Union` is one filled path with the nonzero rule, which holds the contours of all the areas of the
  cluster. `SymPathWithHoles.AddToUnionPathPartList` and `SymPathWithHoles.ContoursMeet` are the code of
  `fix/overlapping-patterns`, unchanged.
* `SymbolCoverage.ExpandPrototype(RectangleF)` expands the coverage prototype over a rectangle, the bounds of a cluster.
* `TransparencyGroup.GetAreaCluster` gives the area cluster of a palette color coverage. `SkiaEncoding` writes the
  pattern of a cluster of several areas once, in its union, with the transformation of its first palette color
  coverage, and the other palette color coverages as before.
* `AreaClusterTests` tests the clusters, the union and the drawing without the old drawing as the oracle, with the cases
  of `OverlappingPatternsTests` of `fix/overlapping-patterns`. `RenderingTests.OverlappingMarshes` and
  `OverlappingMarshes2` draw two maps of overlapping marsh areas.

## Issue ticket number
[028-palette-cutter-patterns](../tickets/028-palette-cutter-patterns.md), separated from
[010-palette-composition](../tickets/010-palette-composition.md)

## Missing functionality
* The PDF, GDI+ and WPF graphics targets draw with the old drawing code, in which each area draws its own pattern. The
  PDF and SVG encodings will write the same area clusters.
* The antialiased edges of the pattern fills in the Skia encoding.

## Changes to be logged
Päällekkäisten tai toisiaan koskettavien saman symbolin alueiden viivoitus tai aluekuvio piirtyy näytöllä, tulostuksen
esikatselussa ja Windowsin tulostuksessa kerran, eikä se enää tummu alueiden päällekkäisissä kohdissa.
