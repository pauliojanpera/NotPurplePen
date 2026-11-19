# The text along lines

Purple Pen places the graphemes of a text along a line (`LineTextSymbol`) by measuring the line with the OCAD metric
always, whatever the metric of the map (`TextSymDef.GetLineTextGraphemePlacement`). Current OCAD measures the lines
with the euclidean metric unless the map has the view parameter `p1`. Fix
[022](../tickets/022-line-text-metric.md) makes the text use the metric of the map, and this sample checks it against
OCAD.

## The first sample

Purple Pen wrote the sample on Nov 19 as
[linetext-1-ocad12-euclidean.ocd](ocad-line-text/linetext-1-ocad12-euclidean.ocd), an OCAD 12 map with the euclidean
metric, from the cells in [linetext-1-cells.json](ocad-line-text/linetext-1-cells.json). Each cell is the text
"ABCDEFGHIJKLMNOP" in Arial of 2.5 mm along a path, with a text symbol of its alignment.

| Cells | Paths and alignments |
| --- | --- |
| horizontal-left, diag30-left, diag45-left | straight lines of 45 mm along x, at 30 degrees and at 45 degrees, left aligned |
| diag45-center, diag45-right, diag45-justified | straight lines of 45 mm at 45 degrees, centered, right aligned and justified |
| zigzag-left, arc-left, S-left | a zigzag of four segments, a quarter circle of radius 30 mm and an S curve, left aligned |

With the OCAD metric, a line at 45 degrees is about 6 % longer than with the euclidean metric, so the text along it is
denser, and a centered or right aligned text starts at another place.
[purple-pen-linetext-1-ocad12-euclidean-150dpi.png](ocad-line-text/purple-pen-linetext-1-ocad12-euclidean-150dpi.png)
is Purple Pen's drawing of the file.

### OCAD's PDF export

@Pauli exported the file as PDF on Nov 19
([ocad-viewer-linetext-1-ocad12-euclidean.pdf](ocad-line-text/ocad-viewer-linetext-1-ocad12-euclidean.pdf)). OCAD
writes each glyph as a filled outline. The middle of each outline along the direction of the path gives the place of
the glyph along the path, and the steps between the glyphs on each slanted path, against those on the line along x,
are the factors of the OCAD metric:

| Path | Steps against the line along x | The OCAD metric | The euclidean metric |
| --- | --- | --- | --- |
| at 30 degrees | 0.896 | 0.896 | 1.000 |
| at 45 degrees | 0.943 | 0.943 | 1.000 |
| the zigzag, 33.7 degrees | 0.901 | 0.902 | 1.000 |

The centered, the right aligned and the justified text agree with the same measure. So current OCAD measures the text
along a line with the OCAD metric also in a map with the euclidean metric, as Purple Pen does, and as the comment of
`GetLineTextGraphemePlacement` says. Ticket [022](../tickets/022-line-text-metric.md) is closed without a fix
(@Pauli, Nov 19).

Mapper maps have no text along lines in Purple Pen: the import of Mapper files (`OpenMapperImport`) makes point and box
texts only.
