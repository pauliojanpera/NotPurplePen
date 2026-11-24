## General description
Pass 2 draws the palette colors of a map from the intermediate layer. It takes the palette separations in palette
order, and for each palette color an encoding writes its transparency group: the color of the palette color on the
coverage of its palette separation, composited once with the opacity α of the palette color and the blend mode B that
the overprint mode gives. The Skia encoding writes the groups on the canvas of a Skia graphics target, so the screen,
the printing on Windows and the print preview draw the palette colors with pass 2. The PDF and the legacy graphics
targets draw them with the old drawing code until they get encodings. The overprint mode (None, ByPaletteColor,
Darken, Multiply) replaces the overprint effect of the course appearance.

## Changes in detail
* `PaletteStack.cs` gives the overprint mode (`OverprintMode`), the interfaces of an encoding
  (`IPaletteStackEncoding`, `IPaletteStackEncodingProvider`) and the transparency group of a palette color
  (`TransparencyGroup`).
* `Map.Draw` selects the palette colors of the color range (`SelectColorsToDraw`) and draws them
  (`DrawPaletteColors`): with pass 2 on a graphics target that gives an encoding (`WritePaletteStack`), at the
  playback on a graphics target that defers a drawing (`IDeferredDrawingTarget`), and with the old drawing code
  (`DrawColor`) on the other graphics targets. Pass 2 takes the palette color coverages of the symbols that can
  intersect the rectangle of the drawing, with the condition and the cancellation of `DrawColor`, and does not call
  the drawing code of the symbols.
* `SkiaEncoding` composites a group with `SKCanvas.SaveLayer` when α is below 1 or B is Multiply, and draws the other
  groups directly with Normal or Darken, which gives the same result. It writes each palette color coverage of a symbol
  apart, with the steps of its placements and the Skia calls of `Skia_GraphicsTarget`, and expands the runs of dashes,
  the offset paths and the repeated placements. A hatching or a clipped area pattern is a pattern brush of its pattern
  tile (`AreaPatternPrototype.Tile`, `PatternTile`) where the old drawing uses a brush, placed by M, and otherwise the
  expansion of its coverage prototype inside the clip of the area.
* `SymbolCoverage.GetClippingObjects(float)` gives the clipping objects at the resolution of a drawing: a dashed line
  component whose gaps are all shorter than the resolution is solid, as the old drawing draws it.
  `SymDef.MakeClippingObjects` takes the resolution.
* The path of the framing rectangle of a text keeps the rectangle (`PathShape.Rectangle`), which the Skia encoding
  draws as a rectangle, as the old drawing does.
* `RecordingGraphicsTarget` defers the drawing of the palette colors (`DrawDeferred`). At the playback, the drawing
  reads the map and writes the palette colors with the encoding of the target of the playback.
* `RenderOptions.overprintMode` replaces `blendOverprintedColors`, `CourseAppearance.overprintMode` replaces
  `useOcadOverprint`, and `MapDisplay.OverprintMode` and `Controller.OverprintMode` replace `OcadOverprintEffect`.
  The course file keeps the attribute `overprint-colors`, and writes the attribute `overprint-mode` only for the modes
  ByPaletteColor and Multiply. The Avalonia `CourseAppearanceDialog` chooses the mode from a menu, and the WinForms
  dialog keeps its check box for Darken and None.
* With ticket 028 (`feature/palette-cutter-patterns`, merged), the Skia encoding writes the pattern of an area
  cluster of several areas once, in the union of its areas (`PaletteCutter.AreaClusters`, `AreaCluster.Union`).
* The internal render option `drawColorsDirectly` draws the palette colors with the old drawing code, for the
  coverage comparison and for the tests of the OCAD dashes.
* `PaletteStackTests` tests pass 2 and the Skia encoding without the old drawing, and `EventDBTests` tests the
  overprint modes of the course file.

## Issue ticket number
[010-palette-composition](../tickets/010-palette-composition.md), with
[028-palette-cutter-patterns](../tickets/028-palette-cutter-patterns.md)

## Missing functionality
* The PDF encoding. Until it is done, the PDF export and the printing on Linux and macOS draw the palette colors with
  the old drawing code.
* The abstractions of Skia for the runs of dashes and the repeated placements, the antialiased edges of the pattern
  fills, and one palette stack for all the maps, the courses included.

## Changes to be logged
Kartan palettivärien peittävyys näkyy näytöllä, tulostuksen esikatselussa ja Windowsin tulostuksessa. Ratojen
ulkoasun päällepainatusvaikutus on valikko: ei käytössä, kunkin värin sekoitustilan mukaan, tummenna "overprint"-värit
tai kerro "overprint"-värit. Päällekkäisten saman symbolin alueiden viivoitus tai aluekuvio ei tummu niiden
päällekkäisissä kohdissa.
