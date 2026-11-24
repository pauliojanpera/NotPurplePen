# OCAD's blend modes and opacity: an oracle

Nov 24, 2026. OCAD is the authority on how its own maps render (the preface of [Specification.md](../Specification.md)).
[Ticket 027](../tickets/027-ocad-blend-mode-tests.md) tests how OCAD draws the blend mode B (`b`: Normal, Darken and
Multiply) and the opacity α (`t`) of a palette color: in its own view, in its PNG export and in its PDF export. The
oracle [ocad-darken.md](ocad-darken.md) rests on the images of one page of the OCAD wiki, whose processing nobody knows.
This file records the sample, what OCAD's documentation tells, what @Pauli exports from the sample, and how the exports
are measured. The results come when the exports are measured.

## The sample

Purple Pen wrote [blend-modes-ocad12.ocd](ocad-blend-modes/blend-modes-ocad12.ocd) on Nov 24 with
`InputOutput.WriteFile`, from a temporary test that is not committed, with the code of `feature/palette-stack` at
0d2148b3. The export writes the opacity (`t`), the blend mode (`b`), the overprint flag (`o`) and the shares of the
spot color separations (`s` and `p`) of each palette color, and the import reads them back as they were made.
[blend-modes-cells.json](ocad-blend-modes/blend-modes-cells.json) gives the spot inks, the palette colors in the
palette order, the symbols, the registration marks and the 76 cells. For each cell it gives the shapes, as rectangles
in mm on the paper, and the regions: 336 points whose colors the measurement reads, each with the palette colors that
lie there, the bottom one first.

- The map scale is 1:10 000. The cells are squares of 12 mm, in a grid of 14 mm with 9 columns and 10 rows.
- The four registration marks are filled circles of 2 mm, 4 mm out from the corners of the grid. Their palette color
  is Registration: K100, and 100 % of each spot color separation, so that the marks are also in a spot color export.
- Each shape is an object of an area symbol, except in two cells of case 3. The area symbol of a palette color has the
  number of the color, for example 102.0 "Area Purple Darken".
- A cell of a palette color over another has the color below in its lower half and the color on top as a vertical bar
  of 4 mm across both halves. So the cell has four regions: the paper, the color below alone, the color on top over
  the paper, and the color on top over the color below.

The palette colors, from the bottom up. The CMYK values and the opacities are in percent.

| Number | Name | CMYK | `t` | `b` | `o` | Spot color separations |
| --- | --- | --- | --- | --- | --- | --- |
| 11 | Blue below | 87 18 0 0 | 100 | Normal | 0 | |
| 12 | Brown below | 0 56 100 18 | 100 | Normal | 0 | |
| 13 | Black 50 below | 0 0 0 50 | 100 | Normal | 0 | |
| 21 | Spot Blue below | 87 18 0 0 | 100 | Normal | 0 | Spot Blue 100 |
| 22 | Spot Brown below | 0 56 100 18 | 100 | Normal | 0 | Spot Brown 100 |
| 101 to 103 | Purple Normal, Purple Darken, Purple Multiply | 0 100 0 0 | 100 | as the name | 0 | |
| 104 to 106 | Purple Normal 50%, Purple Darken 50%, Purple Multiply 50% | 0 100 0 0 | 50 | as the name | 0 | |
| 121 to 123 | Black 50 Normal, Black 50 Darken, Black 50 Multiply | 0 0 0 50 | 100 | as the name | 0 | |
| 124 to 126 | Black 50 Normal 50%, Black 50 Darken 50%, Black 50 Multiply 50% | 0 0 0 50 | 50 | as the name | 0 | |
| 111 | Green Normal | 76 0 91 0 | 100 | Normal | 0 | |
| 151 | Yellow (5) | 0 25 80 0 | 100 | Normal | 0 | |
| 152 | Blue (5) | 87 18 0 0 | 100 | Normal | 0 | |
| 112 | Green Darken | 76 0 91 0 | 100 | Darken | 0 | |
| 153 | Brown (5) | 0 56 100 18 | 100 | Normal | 0 | |
| 113 | Green Multiply | 76 0 91 0 | 100 | Multiply | 0 | |
| 114 to 116 | Green Normal 50%, Green Darken 50%, Green Multiply 50% | 76 0 91 0 | 50 | as the name | 0 | |
| 131 | Purple overprint Normal | 0 100 0 0 | 100 | Normal | 1 | |
| 132 | Purple overprint Darken | 0 100 0 0 | 100 | Darken | 1 | |
| 141 | Spot Violet Normal | 0 100 0 0 | 100 | Normal | 0 | Spot Violet 100, Spot Blue 0, Spot Brown 0 |
| 142 | Spot Violet Darken | 0 100 0 0 | 100 | Darken | 0 | Spot Violet 100, Spot Blue 0, Spot Brown 0 |
| 143 | Spot Violet only Normal | 0 100 0 0 | 100 | Normal | 0 | Spot Violet 100 |
| 144 | Spot Violet only Darken | 0 100 0 0 | 100 | Darken | 0 | Spot Violet 100 |
| 1 | Registration | 0 0 0 100 | 100 | Normal | 0 | Spot Violet 100, Spot Blue 100, Spot Brown 100 |

The spot color separations are Spot Violet (C0 M100), Spot Blue (C87 M18) and Spot Brown (C0 M56 Y100 K18), without a
screen. In OCAD, a share of 0 erases the separation where the color lies, and a separation without a share is left as
it is (the OCAD wiki, "Colors").

The cells, by rows from the top:

| Rows | Case | The cells |
| --- | --- | --- |
| 1 to 3 | 1 | Purple, Green and Black 50, each in Normal, Darken and Multiply with the opacity 100 (the 9 columns), over Blue below, Brown below and Black 50 below (the rows). The upper half of each cell is the paper below the color on top. |
| 4 to 6 | 2 | The same with the opacity 50. |
| 7 | 3 | Two overlapping shapes of Green Normal 50% and of Green Multiply, over Blue below: two objects of one area symbol, objects of two area symbols of the color (614.0 and 613.0 "Area 2 ..."), and one object of a point symbol whose two area parts overlap (814.0 and 813.0 "Point ..."). The shapes are 5 mm wide and overlap by 2 mm. |
| 8 | 4 | Purple overprint Normal and Purple overprint Darken, over the three colors below. |
| 9 | 4 | The four spot colors, over Spot Blue below and Spot Brown below. |
| 10 | 5 | Green Darken over Blue (5), the palette color next below it, over Yellow (5), which is further below, and over both. Brown (5), the Normal palette color next above Green Darken, over Green Darken. |

The column Normal of cases 1 and 2 gives each color on top without a blend mode, beside the same color with Darken and
Multiply. Multiply with the opacity 100 in case 3 tells whether OCAD applies B to each shape or once to the palette
color: Multiply is not idempotent, so an overlap of two shapes gets darker when B applies to each shape. Darken is
idempotent, so it cannot tell this. The spot colors of case 4 that erase the other spot color separations and those
that leave them tell how Darken acts on a share of 0.

## What OCAD's documentation tells

@Pauli gave these pages on Nov 24: the OCAD wiki pages
[Colors](https://www.ocad.com/wiki/ocad/en/index.php?title=Colors),
[Export Files](https://www.ocad.com/wiki/ocad/en/index.php?title=Export_Files) and
[Course Setting Printing](https://www.ocad.com/wiki/ocad/en/index.php?title=Course_Setting_Printing), and the OCAD blog
post [Blend Modes for PDF
Export](https://ocad.com/blog/2016/08/blend-modes-for-pdf-export-print-your-maps-with-more-transparency/).

- OCAD's view does not show the overprint effect. The overprint flag affects the color AI, EPS and PDF files and the
  CMYK separations ("Colors").
- The blend modes are Darken and Multiply. They affect only the PDF export, and they are not visible in OCAD (the
  blog post, "Course Setting Printing").
- The opacity of a color is 0 to 100. The page shows black with the opacities 100, 50 and 0, the 50 as gray over the
  colors below ("Colors").
- The PDF export has two choices of colors. "CMYK (process) colors" ignores the spot colors. "Spot color separations"
  exports the selected spot color separations: a PDF file in grayscale for each, or one PDF file of all of them with
  "Combine". The PDF export is not available in the draft mode ("Export Files").
- The PNG export has a resolution, the anti-aliasing and the color correction of the screen ("Export Files").
- The spot color mode of the view simulates the printing with spot colors ("Export Files").

## The structure of OCAD's PDF export

The PDF exports of the dash samples ([ocad-dashes.md](ocad-dashes.md), OCAD 2020.5.18 Viewer) show how OCAD writes the
palette colors, for example
[ocad-viewer-dashes-6-ocad12-euclidean.pdf](ocad-dashes/ocad-viewer-dashes-6-ocad12-euclidean.pdf):

- The page paints each palette color, from the bottom up, as a form XObject on a layer with the name of the color.
- Each form is a transparency group: `/S /Transparency /I false /K false`, without a color space (`/CS`).
- The graphics state at the `Do` of the form gives the blend mode and the opacity. OCAD writes a graphics state for
  each blend mode and each opacity in percent: `GSNormal0` to `GSNormal100`, `GSDarken0` to `GSDarken100` and
  `GSMultiply0` to `GSMultiply100`, each with `/BM`, `/ca` and `/CA`. The two colors of that sample use `GSNormal100`.
- Inside the form, each symbol is a layer whose title is the number and the name of the symbol. The layer gives the
  color with `k` (DeviceCMYK), and the overprint with `GSOP0` (`/OP false`) or `GSOP1` (`/OP true /OPM 1`).
- The page has no transparency group, and the file has no output intent.

A transparency group starts with the blend mode Normal and the opacity 1, and the blend mode and the opacity at its
`Do` apply to the group as a whole (ISO 32000-1, 11.6.6). So if OCAD writes the sample in the same way, it applies B and
α once to each palette color, as the transparency group of a palette color does (section 04 of the specification). The
group has no color space and the page has no group, so the PDF does not give the blending color space: a viewer
chooses it. The measurement of the sample's exports confirms or corrects this.

## What @Pauli exports and reports

Open [blend-modes-ocad12.ocd](ocad-blend-modes/blend-modes-ocad12.ocd) in OCAD. Put the files on your drive in a folder
`ocad-blend-modes`, under any names, and tell which file is which. If a step is not possible, for example because the
OCAD Viewer does not have a dialog, tell which step.

1. **The program.** Its name and version from its About dialog, for example "OCAD 2020.5.18 Viewer".
2. **The colors.** A screenshot of the Colors dialog of the Map menu and one of its Blend mode dialog. They show how
   OCAD read the opacities, the blend modes, the overprint flags and the spot color shares.
3. **The view.** A screenshot of the normal view, not the draft mode, with all of the map and its four registration
   marks on the screen, at the largest zoom that shows all of it. Save it as PNG, without scaling.
4. **The spot color view.** If the View menu has the spot color mode, a screenshot of the same view in that mode.
5. **The PNG export.** 600 dpi, without anti-aliasing and without color correction, of the entire map.
6. **The PDF export of the process colors.** "CMYK (process) colors", the entire map, at the scale of the map
   (1:10 000).
7. **The PDF export of the spot colors.** "Spot color separations", with the three spot color separations selected
   and "Combine" on, the entire map, at the scale of the map.
8. **The settings.** For each export, a screenshot of its dialog before OK, or a list of all its options with their
   values.

## How the exports are measured

- [measure-pdf.py](ocad-blend-modes/measure-pdf.py) reads the objects of a PDF export and interprets the content
  stream of its page. It prints the page group with its blending color space (`/CS`), the output intents, each
  transparency group with its color space, its isolation (`/I`) and knockout (`/K`) flags, and the blend mode, the
  opacities (`/ca`, `/CA`) and the overprint flags (`/OP`, `/op`, `/OPM`) at its `Do`. For each cell, it prints each
  painted shape with its symbol layer, its color (DeviceCMYK, Separation or DeviceN, with the colorants), its graphics
  state inside the group and its group. For the regions of the cells in DeviceCMYK, it gives the inks by the PDF model
  in a CMYK blending space: Darken takes the larger share of each ink, and Multiply the product of the complements.
  It does not model a spot color, an isolated or knockout group, or a soft mask.
- [measure-colors.py](ocad-blend-modes/measure-colors.py) finds the registration marks in an image, reads the color of
  each region, and compares the colors only within the image. It takes the paper and the full color of each palette
  color from the regions where a color lies alone over the paper with the opacity 100, and predicts the other regions
  by six ways of compositing: B ignored or applied in RGB, and α ignored, applied once to each palette color, or
  applied once to each shape. It prints the regions that each way fits, by case. A blend mode by ink cannot be
  predicted in an image without the conversion of its colors.
- The scripts are checked as follows. measure-pdf.py reads the PDF of the sixth dash sample: it finds its two groups,
  and the marks give 2.8346 pt/mm, which is 72 / 25.4. It also reads a PDF that a script of the session wrote of the
  cells in the structure above, with Separation and DeviceN colors: it matches each shape with its cell, and its inks
  agree with the computation by hand, for example C87 M59 for Purple Darken 50% over Blue below, and C76 M25 Y91 for
  Green Darken over Yellow (5). measure-colors.py finds the marks in Purple Pen's drawings below and in a screenshot
  that a script of the session made of a drawing at 5.9 pixels to the mm, with dark menus, texts, round icons and a
  page border around it.

## Purple Pen's drawings

The two drawings
[in the mode ByPaletteColor](ocad-blend-modes/purple-pen-blend-modes-by-palette-color-600dpi.png) and
[in the mode None](ocad-blend-modes/purple-pen-blend-modes-none-600dpi.png) are Purple Pen's drawings of the sample
with Skia, at 600 dpi without anti-aliasing, by pass 2 of `feature/palette-stack` (0d2148b3), in the overprint modes
`ByPaletteColor` and `None` (`OverprintMode`). The temporary test drew the file as the import read it.

measure-colors.py gives these results. In `ByPaletteColor`, B applied in RGB and α applied once to each palette color
fit all 133 regions that are not references, within 2 levels. In `None`, B ignored and α applied once to each palette
color fit all 133. Case 3 tells the two ways of applying α apart: α applied once to each shape fits 18 of its 30
regions in `ByPaletteColor`, and 24 in `None`.

## Results

None yet. The exports are due from @Pauli when he is at OCAD.
