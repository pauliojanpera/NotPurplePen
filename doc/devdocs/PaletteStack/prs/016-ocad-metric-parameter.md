## General description
OCAD measures the lines of a file with the metric that the parameter `p` of its view parameters gives: 1 is the OCAD
metric, and 0 or no parameter is the Euclidean metric, in a file of any version that has view parameters. Purple Pen
read `p` only from OCAD 11+ files, and measured the dashes, the shortening and the glyphs of the lines of OCAD 6 to 10
files with the OCAD metric. It now gives each OCAD file the Euclidean metric, reads `p` from the view parameters of
any version, and writes `p` to each file that has view parameters. @Pauli decided on Nov 18 that Purple Pen follows
the current OCAD in a file without `p`.

## Changes in detail
* `OcadImport` gives each OCAD file the Euclidean metric, and reads `p` from the view parameters of any version.
* `OcadExport` writes `p` to each file that has view parameters, from OCAD 9 on. OCAD 6 to 8 files have no view
  parameters, so a map with the OCAD metric loses it there.
* `OcadMetricTests` checks the metric of OCAD 6, 8, 9 and 10 files, the dashes of a line at 45° in OCAD 9 and 10 files
  of the dash sample against the drawing of the OCAD 2020 viewer, and that a written file keeps the metric of the
  map. Without the change, 7 of its 15 cases fail. `OcadSampleDrawing` reads the sample files and counts the dashes
  along a line.
* `UndoMetric` starts from the Euclidean metric, which its OCAD 9 file now has. The expected dumps of the OCAD 9 files
  that `MapTests` and the course creation tests of `ControllerTests` write get `p1`.
* The OCAD 11 and 12 versions of Lake Sammamish and the OCAD 2018 version of teanwest have `p1`. The older versions,
  with which they shared their baselines, have no `p`, and with the same metric all of them draw the same. The
  descriptions of the versions with `p1` name new baselines, `*_ocadmetric`, that hold upstream's drawing.
  `LakeSammMap12` writes its map round only through OCAD 9 to 12.
* 210 baselines move: 36 of Skia, 45 of the GDI+ renderings of `Test_MapModel`, 38 of PDF, 22 of WPF and 69 of
  `PurplePen_Tests`. They are the test maps of OCAD 6 to 10 whose dashes, shortening or glyphs lie along diagonal or
  curved lines (collecting run 44363913238).
* Three baselines that `base` added with `fix/lossless-export-comparison`,
  `src/TestFiles/routegadget/exportautodpi1_baseline.png` to `exportautodpi3_baseline.png`, move too. This branch is
  rooted at upstream and cannot hold them. They are in `prs/016-ocad-metric-parameter-base/` for the merge into
  `base`, and `push-wip.sh --base-baselines` writes them into the wip commit.

## Issue ticket number
[016-ocad-metric-parameter](../tickets/016-ocad-metric-parameter.md)

## Missing functionality
* Purple Pen measures text along a path always with the OCAD metric (`TextSymDef.GetLineTextGraphemePlacement`). The
  samples had no text.

## Changes to be logged
OCAD 6–10 -tiedoston viivat mitataan kuten nykyisessä OCADissa: tiedosto, jossa ei ole metriikan parametria p,
euklidisesti, ja p luetaan, kun se on tiedostossa.
