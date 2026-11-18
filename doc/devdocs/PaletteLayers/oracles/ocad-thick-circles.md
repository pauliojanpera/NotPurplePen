# OCAD's circle whose line is as wide as its diameter or wider

Nov 18, 2026. OCAD is the authority on how its own maps render (the preface of [Specification.md](../Specification.md)).
This file records how OCAD, Mapper and Purple Pen draw the circle element of a point symbol whose line is at least as
wide as its diameter. From OCAD 9 on, the diameter of a circle element is measured from the middle of its line.

## The source

Purple Pen wrote two files on Nov 18, with `InputOutput.WriteFile` in a temporary test, at the scale 1:10 000:
[ocad-thick-circles/circle-line-wider-than-diameter-ocad9.ocd](ocad-thick-circles/circle-line-wider-than-diameter-ocad9.ocd)
and
[ocad-thick-circles/circle-line-wider-than-diameter-ocad12.ocd](ocad-thick-circles/circle-line-wider-than-diameter-ocad12.ocd).
Each has five point symbols in a row, at x = 0, 10, 20, 30 and 40 mm on the paper. Purple Pen reads the circle of
each with the diameter and the line of the file, except D, whose diameter becomes 2.5 mm from the outer edges.

| Symbol | The circle element in the file |
| --- | --- |
| A | diameter 1.0 mm, line 1.5 mm, at the origin of the symbol, the only element |
| B | as A, and a blue filled circle of 0.5 mm at (2.5, 0) mm |
| C | diameter 1.0 mm, line 1.0 mm, the only element |
| D | diameter 2.0 mm, line 0.5 mm, the only element |
| E | as A, with its center 1 mm to the right of the origin |

@Pauli took the screenshots on Nov 18: OCAD 2020.5.18 Viewer
([ocad-viewer-ocad9.png](ocad-thick-circles/ocad-viewer-ocad9.png),
[ocad-viewer-ocad12.png](ocad-thick-circles/ocad-viewer-ocad12.png)) and OpenOrienteering Mapper 0.9.7
([mapper-ocad9.png](ocad-thick-circles/mapper-ocad9.png), [mapper-ocad12.png](ocad-thick-circles/mapper-ocad12.png)).
The view of Mapper shows the whole area that the drawing covers. The same temporary test drew
[purple-pen.png](ocad-thick-circles/purple-pen.png) with Skia, at 0.05 mm to the pixel.

![OCAD 2020 Viewer: A, B, C and E are filled circles, and D is a ring](ocad-thick-circles/ocad-viewer-ocad12.png)

## The values

In the screenshots of OCAD, at the zoom 3.7, the rulers put 0 and 30 mm 347 pixels apart: 11.6 pixels to the mm. The
two files draw the same in each program.

| Symbol | OCAD 2020 | Purple Pen, upstream d48f4456 | Mapper 0.9.7 |
| --- | --- | --- | --- |
| A | filled circle, 12 px, about 1.0 mm | filled circle, 1.0 mm | nothing |
| B, the circle | filled circle, 13 px, about 1.0 mm | nothing | nothing |
| C | filled circle, 12 px, about 1.0 mm | nothing | nothing |
| D | ring, 28 to 29 px, 2.5 mm across | ring, 2.5 mm across | ring, 2.5 mm across |
| E | filled circle, 11 to 12 px, about 1.0 mm, its center at 41 mm | nothing | nothing |

## What the oracle tells

- OCAD draws a circle element whose line is as wide as its diameter or wider as a filled circle of its diameter. The
  line does not change the size: C, with a line of 1.0 mm, is as large as A, with a line of 1.5 mm.
- Purple Pen's OCAD import keeps the diameter of such a circle, since it adds the line to the diameter only when the
  diameter is larger than the line. `Glyph.DrawSimple` fills a lone circle at the origin when its line is wider than
  its diameter, so A agrees with OCAD. `GlyphPart.Draw` draws a circle only when its diameter is larger than its line,
  so B and E are missing, and `DrawSimple` draws C as a ring of radius 0, which covers nothing. Ticket
  [015](../tickets/015-thick-glyph-circles.md) fixes both.
- Mapper draws none of these circles. For an OCAD file, OCAD is the authority, so Mapper is not the oracle of them.
- Purple Pen wrote the files. They show how OCAD reads these values, and not whether OCAD writes such circles.
