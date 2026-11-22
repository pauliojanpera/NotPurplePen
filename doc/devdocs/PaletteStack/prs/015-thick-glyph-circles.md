## General description
OCAD takes the diameter of a circle element from the middle of its line, and an OCAD 6 to 8 file holds it from the
outer edges of the line. With that diameter, OCAD 2020 draws a circle whose line is at least as wide as its diameter as
a filled circle of the diameter, and a circle whose diameter is 0 or less not at all. Purple Pen drew such a circle
only when it was alone at the origin of its glyph, read an OCAD 6 to 8 circle with its outer diameter, and wrote such
a circle as a ring. It now reads, draws and writes these circles as OCAD draws them.

## Changes in detail
* `GlyphPart.Draw` draws a circle when its line and its diameter are larger than 0, and fills it as a circle of its
  diameter when the radius to the middle of its line is 0 or less. `Glyph.DrawSimple` fills a lone circle at the
  origin also when its line is as wide as its diameter. `GlyphPart.BoundingBox` holds the filled circle. The circles of
  the glyphs of lines and area patterns follow, since the same code draws them.
* `OcadImport` takes the diameter of an OCAD 6 to 8 circle from the middle of its line when the line is at least as
  wide as that diameter, and not below 0. OCAD 9+ circles and OCAD 6 to 8 rings read as before.
* `OcadExport` writes a circle whose line is at least as wide as its diameter with the diameter from the middle to
  OCAD 9+, and with the line added to OCAD 6 to 8, so that a round trip keeps the circle. Rings are written as before.
* `GlyphCircleTests` draws four glyphs of such circles, and the circles of an OCAD 12 and an OCAD 8 file (new files
  in `TestFiles/ocadimport`), and checks them against the drawing of OCAD 2020.
* Four rendering baselines move. The bar scale of `multisymonedash` gets a filled circle of 0.4 mm, in the Skia and
  the GDI+ renderings. The symbol 418.2 of `LordHill` and `LordHill OCAD11` gets circles of 0.01 mm, which move 7
  pixels by at most 16 in the Skia renderings.

## Issue ticket number
[015-thick-glyph-circles](../tickets/015-thick-glyph-circles.md)

## Missing functionality
* The Mapper export (`OpenMapperExport`) writes such a circle as a filled circle as wide as two lines.

## Changes to be logged
Purple Pen piirtää OCAD-kartan ympyrän, jonka viiva on vähintään halkaisijan levyinen, täytettynä ympyränä kuten OCAD.
