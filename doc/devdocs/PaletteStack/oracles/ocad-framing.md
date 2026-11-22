# The framing line of texts

A text symbol with a framing line (`FramingStyle.Line`) draws each text run, and then the outline of the run with
several pens: pens of increasing width, in steps of 0.33 times the font size, with bevel joins, and last a pen of the
full width, two times the width of the framing, with the joins of the symbol (`TextSymDef.CreateObjects` and
`DrawStringWithEffects`). A comment of the code says that the pens "avoid weird artifacts with overlapping parts".
The comment and the pens come from the first version of the code (2008), which drew with GDI+.

The narrower lines are inside the line of the widest pen, so the framing is one line by its geometry. Ticket
[013](../tickets/013-text-geometry.md) asks whether the instances hold one line or the lines of all the pens. This
file records what the renderers and OCAD do.

## The renderers

On Nov 19 a test rendered the map `skia_render/frametext1.ocd` of the tests, which has 10 texts with a framing line,
twice: with all the pens, and with the widest pen only. A local change of `DrawStringWithEffects`, which was not
committed, drew the widest pen only.

| Renderer | Pixels that the widest pen alone leaves white |
| --- | --- |
| Skia, 1500 px wide, without anti-aliasing | 41 |
| Skia, 6000 px wide, without anti-aliasing | 647 |
| Skia, 1500 px wide, with anti-aliasing | 73 |
| Skia, 6000 px wide, with anti-aliasing | 811 |
| Cairo (`pdftocairo`), PDF at 1200 dpi | 264 |
| Cairo (Inkscape), SVG at 1200 dpi | 127 |
| Poppler's Splash (`pdftoppm`), PDF at 1200 dpi | 0 |
| Ghostscript, PDF at 1200 dpi | 0 |

The PDF and the SVG were written with the PDF and SVG backends of Skia, which keep the framing as a stroke of the
text, so the renderer of the file strokes it. The test also rendered `framingline`, `framingline9`, `frametext2`,
`frametext3` and `framing_ocad6`. They have no text with a framing line, and their renderings did not change.

The holes are where the wide line folds inside a letter, for example in the counter of an "o".
[ocad-framing/skia-framing-pens.png](ocad-framing/skia-framing-pens.png) shows four places of the Skia rendering at
1500 px with anti-aliasing, magnified five times: all the pens, the widest pen alone, and the differences in red.

So the holes are a fault of some renderers and not of the geometry. Skia and Cairo draw many viewers of PDF and SVG
files (for example Chrome, Evince and Inkscape), and a file does not know its viewer.

## OCAD

Whether the holes are faults at all depends on how OCAD draws the framing line. @Pauli exported `frametext1.ocd` from
OCAD on Nov 19, as a PDF ([ocad-viewer-frametext1.pdf](ocad-framing/ocad-viewer-frametext1.pdf)) and as a PNG at
1200 dpi.

- **The PDF.** OCAD fills each glyph in the color of the framing (`f*`) and strokes its outline with one pen of the
  full width, for example 5.67 pt (2.0 mm) with round joins and caps. It writes no intermediate pens.
- **The PNG.** OCAD's own drawing leaves white holes inside the framing, where the wide line folds inside a letter, as
  Skia and Cairo do with one pen: for example in "style", "This", "Centered" and "again".
  [ocad-framing/ocad-framing-holes.png](ocad-framing/ocad-framing-holes.png) shows six places of the PNG, magnified
  three times, each hole in a blue frame.

So the intermediate pens of Purple Pen fill holes that OCAD's own drawing has. @Pauli's decision (Nov 19): the
framing line of a text run is one line in the instances, as in OCAD: one stroke of the full width with the joins of
the symbol (question 2 of ticket [013](../tickets/013-text-geometry.md)).
