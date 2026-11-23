## General description
The coverage prototypes of the hatchings and the area patterns. Each hatching of an area symbol definition, and each
clipped area pattern in each palette color of the glyph parts of its element, is a coverage prototype of its own. An
area symbol has a palette color coverage of each of them, whose clipping object is the area, and the transformation M
of its configuration comes from the angle and the rotation center of the area symbol and from the angle and the
offset of the hatching or the area pattern. A palette separation groups its palette color coverages into palette
cutters by the normal form of M. The glyph parts that a fill mode selects are clipping objects of the full coverage.
The collector, the oracle of the geometry methods, is removed, and the coverage comparison compares each palette
color with the drawing. The drawing does not change.

## Changes in detail
* `HatchingPrototype` and `AreaPatternPrototype` are coverage prototypes (`CoveragePrototype`). They refer to their
  area symbol definition and to the index of the hatching or the area pattern, an area pattern also to its palette
  color, and they read the hatching and the area pattern from the definition. `AreaSymDef` makes them once, and each
  area symbol of the definition refers to the same objects, also through an `AreaComboSymDef`.
* `CoveragePrototype.Covers` of a hatching covers the stripes of its width about the lines y = k · `hatchSpacing`. Of
  an area pattern, it covers the glyph parts in its palette color at the points of the lattice, the odd rows offset
  when `offsetRows` is set, each element turned by -`patternAngle` and moved by the irregularity as the drawing moves
  it. A line part covers its stroke shape with its caps, joins and miter limit (`GraphicsUtil.StrokeShapeCovers`).
* `SymbolCoverage.Transformation` gives M: the identity for the full coverage, T(c) · R(a) · R(`hatchAngle`) ·
  T(0, `hatchOffset`) for a hatching, and T(c) · R(a) · R(`patternAngle`) · T(`patternOffsetX`, `patternOffsetY`)
  for an area pattern, where c and a are the rotation center and the angle of the area symbol. The rotate mode
  `Never` leaves out c and a.
* `CoveragePrototype.GetNormalForm` reduces the translation of M into a fundamental domain of the translation
  symmetries of G, in the coordinates of G: across the lines of a hatching in [0, `hatchSpacing`), and as the
  coefficients of the basis of the lattice of an area pattern in [0, 1), rounded to steps of about 0.0001 mm. The
  translation of an irregular area pattern stays. The linear part is rounded to 10⁻⁶. `PaletteSeparation` makes a
  palette cutter for each coverage prototype and normal form, and `PaletteCutter.Transformation` is the normal form.
* `SymDef.GetCoveragePrototypes` gives an area symbol first the full coverage, where its definition draws the palette
  color with its area fill, its area border or the glyph parts that a fill mode selects, and then its hatchings and
  its clipped area patterns in the palette color, in the order of the drawing, of the component definitions in their
  order.
* The clipping object of the palette color coverage of a hatching or a clipped area pattern is the area: a filled
  shape of the main path and the holes with the alternate fill rule, with which the drawing clips them.
* `SymbolCoverage.ExpandPrototype` gives the expansion: the hatch lines and the elements over the bounds of the area,
  as single clipping objects at the places, with the steps and in the order of the drawing. A glyph of one circle at
  its origin is a circle at its lattice point within M, as `Glyph.DrawSimple` draws it.
* The glyph parts that the fill modes `CompletelyInside`, `CenterInside` and `PartiallyInside` select, each by its own
  points and center as `GlyphPartFilter` selects it, are clipping objects of the full coverage between the area fill
  and the area border: the shared shapes of the glyph at a placement of M and the pose of the element. So the internal
  geometry method of the areas takes the angle and the rotation center of the area symbol.
* The collector (`ClippingObjectCollector`), `SymDef.CollectClippingObjects` and the internal render option
  `omitHatchingsAndPatterns` are removed. `SymbolCoverageComparison` keeps the expansion with which the tests draw the
  clipping objects.
* `PatternPrototypeTests` checks the coverage prototypes as structures: their definition, their coverage, a cross
  hatching, an area pattern in two palette colors, M with each rotate mode, the palette cutters, the area as the
  clipping object, the expansion, the selections of the fill modes and the combined areas.
* `SymbolGeometryTests` keeps the own checks of its structure tests, and checks that each placement of the clipping
  objects and of the expansions gives its poses in the 162 maps of `skia_render` and the 10 dash samples.
* `PaletteSeparationCoverageTests` compares each palette color of 46 maps with the drawing, also the palette colors of
  the hatchings and the area patterns, with the expansion of each of them inside its clipping object. No pixel
  differs.

## Issue ticket number
[009-pattern-instances](../tickets/009-pattern-instances.md)

## Missing functionality
* The pattern tile and the other channel conversions of the pattern fills come with the encodings. Pass 2 is the draft
  010.
* With the fill mode `CenterInside`, the drawing does not draw a glyph part that is a line whose path encloses no
  area, for example a straight line, as `SymPath.AreaCentroid` gives NaN for it. The selected glyph parts follow the
  drawing.

## Changes to be logged
Ei käyttäjälle näkyviä muutoksia.
