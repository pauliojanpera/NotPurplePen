## General description
A dash of a dashed line went through each vertex that it crosses. Where a dash started a little before a vertex, or
ended a little after it, the dash took the vertex and the corner of its miter, and a border of a double line took the
corner of the offset. The PDF export of the OCAD 2020 viewer leaves out of a dash the vertex next to its start or its
end when the piece of the dash between them is shorter than 0.4 times the width of the line, at any angle of the
turn. The dash then goes straight from its boundary to the next point. On a double line, OCAD first leaves the vertex
out of the dash of the middle when the piece is shorter than 0.05 mm, and each border then applies the rule of a
single line to its own dash, with the width of the border. Purple Pen now does the same for the lines that OCAD's
spacing draws.

## Changes in detail
* `SymPath.DrawDash` draws one dash. It leaves out the vertex next to each end of the dash when the piece is shorter
  than a limit: one limit for the dash on the path, before a border is offset, and one for the dash that is drawn.
  The points that the flattening of a curve makes stay, and the rule does not repeat. `SymPath.DrawDashedCore` draws
  each dash with it.
* `LineSymDef` gives the limits for OCAD's spacing (`SpacingMethod.OCAD`): 0.4 times the width for the main line;
  0.05 mm, and then 0.4 times the width of the fill or of the border, for the dashed fill and the dashed borders of a
  double line. The spacings of Mapper keep all the vertices.
* `OcadDashVertexTests` draws 74 cells of the seventh dash sample, records the polylines of the dashes, and checks
  whether the dash next to the vertex keeps it, as OCAD's PDF export does: single lines of 0.06 to 1.0 mm at turns of
  30° to 150°, the dashed main line of a double line, and the borders of double lines with borders of 0.10, 0.15 and
  0.30 mm at turns of 30°, 90° and 150°. Without the change, 38 of its cases fail. `OcadSampleDrawing` reads the
  sample file, as in the fixes of tickets 017 to 019.
* The change moves 124 image baselines (collecting run 44441352883): 18 of Skia, 18 of the GDI+ renderings of
  `Test_MapModel`, 18 of PDF, 3 of WPF and 67 of `PurplePen_Tests`, among them course printings and course
  symbols, whose dashes start or end near a vertex. It also moves three route gadget baselines that `base` added
  (`exportautodpi1` to `3`); `prs/020-dash-boundary-at-vertex-base/` keeps them for the merge into `base`.

## Issue ticket number
[020-dash-boundary-at-vertex](../tickets/020-dash-boundary-at-vertex.md)

## Missing functionality
* OCAD puts the points of its dashes on its grid of 0.01 mm, so near a limit its rounding decides. Purple Pen uses
  the exact pieces, and there it can differ from OCAD.
* The rule for a dashed fill of a double line is not checked against OCAD: the fill takes the limits of a border,
  with the width of the fill.
* The spacings of Mapper are not checked, and keep all the vertices.
* The bitmap exports of the OCAD viewer draw single lines by a rule of their own, which depends on the resolution.
  The change follows the PDF export.

## Changes to be logged
Katkoviivan katko jättää alkuaan tai loppuaan lähimmän taitepisteen pois, kun katkon pätkä taitepisteen yli on lyhyt,
kuten OCADissa. Katkon alkuun tai loppuun ei siksi tule viisteen kulmaa.
