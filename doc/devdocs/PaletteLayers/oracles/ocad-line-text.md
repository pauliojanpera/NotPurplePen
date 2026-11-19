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
is Purple Pen's drawing of the file before the fix. The file is on @Pauli's drive in `ocad-linetext-1`, for its PDF
export.
