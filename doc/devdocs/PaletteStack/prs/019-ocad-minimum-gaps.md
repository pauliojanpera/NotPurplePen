## General description
`OcadImport` read the field `MinSym` of a line symbol as `MinSym + 1` gaps of its dashes and as at least as many
spaced glyphs. The OCAD 2020 viewer reads -1 as no minimum, 0 as one and 1 as two, as Purple Pen did, but 2 and 3 as
one, with or without a secondary gap, and for spaced glyphs too. `OcadImport` now reads 2 and the larger values as
one.

## Changes in detail
* `OcadImport.MinimumNumberOfGaps` reads `MinSym`. The minimum gaps of the dashes and the minimum number of spaced
  glyphs use it.
* `OcadMinimumGapsTests` counts the dashes and the glyphs of the lines of the third dash sample whose `MinSym` is 2 or
  3, and compares them with the drawing of the OCAD viewer. Without the change, all its 7 cases fail.
  `OcadSampleDrawing` reads the sample files and counts the dashes along a line.
* No baseline moves (collecting run 44412813635).

## Issue ticket number
[019-ocad-minimum-gaps](../tickets/019-ocad-minimum-gaps.md)

## Missing functionality
* Values of `MinSym` above 3 are not checked against OCAD. `OcadExport` writes a minimum of 3 gaps or more as
  `MinSym` 2 or more, which OCAD reads as one gap.

## Changes to be logged
OCAD-viivasymbolin vähimmäisvälit 2 ja 3 luetaan yhdeksi väliksi kuten OCADissa.
