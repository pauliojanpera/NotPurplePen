## General description
`OcadImport` gave the dashes of the fill and the borders of a double line at least one gap. The OCAD 2020 viewer
draws a dashed double line that is too short for a gap as solid, with a dashed fill or with only dashed borders, and
also beside a dashed main line with a minimum of its own. `OcadImport` now gives these dashes no minimum.

## Changes in detail
* `OcadImport` gives the dashes of a double line no minimum gap (`doubleDashes.minGaps = 0`).
* `DoubleLineDashTests` counts the dashes of the short double lines of the second and the third dash samples, and
  compares them with the drawing of the OCAD viewer. Without the change, 11 of its 16 cases fail.
  `OcadSampleDrawing` reads the sample files and counts the dashes along a line.
* No baseline moves (collecting run 44412734699).

## Issue ticket number
[018-double-line-dash-gaps](../tickets/018-double-line-dash-gaps.md)

## Missing functionality
None.

## Changes to be logged
Katkottu kaksoisviiva, joka on liian lyhyt välille, piirretään ehjänä kuten OCADissa.
