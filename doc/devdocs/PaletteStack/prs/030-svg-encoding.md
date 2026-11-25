## General description
The branch brings the SVG export of NotPurplePen from `feature/svg-target` (1088d1295) to the new dev, without its
history, and makes the SVG encoding of pass 2: `Svg_GraphicsTarget` gives an encoding that writes the transparency
group of each palette color into the `SvgDocument`, in the base form of the SVG. The transparency group of a palette
color carries its color, its opacity and its blend mode, and the palette color coverage of each symbol is its own
elements, with the references of the drawn objects to the symbol keys and the coverage prototypes of the map. The
structures of the SVG target that came before the palette stack give way to those of the palette stack. The tests
check the SVG in Chromium against the drawing of pass 2 by the Skia encoding, and the geometry of the SVG against the
intermediate layer.

## Changes in detail
* 36c21261d brings the parts of the SVG work that stay, as at 1088d1295: `Map_Svg` and its tests, the structured
  interface of Graphics2D with the course records and the georeference, the font work and the JPEG data of
  `Map_SkiaStd`, the EPSG codes of the OCAD grids, the export of `PurplePenCore` (`CourseSvg`, `CourseOutputPages`,
  `MapDisplayLayer`, `CourseSvgSettings`), and the Avalonia dialog and command with their translations. Its message
  lists the 86 commits of `feature/svg-target` whose work it brings, and how each conflict with `dev` was resolved. It
  leaves out the old path of the palette colors in the map code (`BeginColorRun`, the vector brushes of `AreaSymDef`,
  `DrawOutline`, `DescribeUnion`, the changes of `OverlappingAreas`), b217b9ddd, 711e089f9 and
  `feature/multiply-blending`.
* 943a951d7 writes the palette colors with the SVG encoding (`SvgEncoding`):
  * The transparency group of a palette color is a group with its position in the color table (`data-c`), its color C
    as the paint of the fills and the strokes, its opacity α below 1 (`opacity`) and its blend mode B other than
    Normal (`mix-blend-mode`). The consecutive palette color coverages of one symbol definition are in its group
    (`data-s`), and each coverage carries the text form of its symbol key (`data-k`) and, for a hatching or an area
    pattern, the place of its coverage prototype (`data-g`).
  * The clipping objects at the resolution of the drawing are elements with their fill rules or their pens. A run of
    dashes is one path with the dash pattern of its pen (`stroke-dasharray`, `stroke-dashoffset`), and the run of
    dashes of an offset path is its single dashes. The shared shapes of a glyph are defined once with a use at each
    single placement, and a repeated placement is a group with a use at each pose.
  * A hatching or a regular clipped area pattern is a pattern fill: its coverage prototype is a definition, and each
    palette cutter is a configuration that refers to it with M (`patternTransform`). The pattern fill of an area
    cluster is written once, within a clip with the path of each area under its own rule (@Pauli, Nov 25).
  * With the setting for separate shapes, each area writes the expansion of its own coverage prototypes within its own
    clip, and each run of dashes is written dash by dash.
  * The course data describes each course object by its key (`npp:object key`). The runs of the colors, the object
    numbers (`data-o`), the ghosts and the unions of overlapping areas and the repeated shapes of S-5 give way.
  * The geometry check (V-10) describes the palette colors from the intermediate layer (`CallGeometryRecorder` gives the
    encoding), and reads the dash patterns, the links of the patterns and the clips of several paths. The reference
    image of a comparison case is the Skia encoding's drawing of its recording, with anti-aliased clips
    (`Skia_GraphicsTarget.AntiAliasesClips`). `SvgEncodingTests` and `AreaPatternTests` check the structures of the
    encoding with the intermediate layer as the oracle, and `CourseSvgStructureTests` walks the course exports by the
    keys.
* e9d66b9e9 names the setting `DrawAreaPatternsAndDashesAsSeparateShapes`, with the text "Draw area patterns and dashed
  lines as separate shapes" in `UIText.resx` and its 27 translations.
* 013cbb70d records the comparison references of the Skia encoding in the new dev: 68 move, and 3 cases get their
  first. bd0c94c3f records the baselines of the SVG export. 6e5aaab61 records the numbers of the browser check that
  move. c04f1fb49 records the JPEG baselines of the image export that Skia's encoder moves.
* 1ca7a4ab3 lets the Skia target decide whether its encoding draws with pattern brushes
  (`Skia_GraphicsTarget.SupportsPatternBrushes`), so that pass 2 makes no call of the graphics target.
* 969f20236 registers the Roboto faces of the rendering tests wherever the tests draw text in them, and again after
  `FixedFontSetTests` clears the private fonts.
* 595344c55 adds a structure test of the place of a coverage prototype in a nested combination (`data-g`).

## The measurements
* The local run of the SVG tests (Linux, Chromium of Playwright): the 196 cases of the rendering comparison are in
  the tolerance against their references, and the geometry check finds no point or length beyond its allowance. Of
  the browser check, this machine's WebKit timed out on LordHill, LordHill11 and punchboxbug, which the loop's runner
  rendered.
* The SVG loop of c04f1fb49 (run 45630366944): 881 passed, 0 failed, 2 pending on calls that the target never honors,
  16 set apart for a browser. The geometry check read 137 911 drawings of 196 cases, and none lay beyond its allowance
  (at most 0.993 of it, LordHill).
* The full run of c04f1fb49 passed (`wip/linux/svg-encoding` at 8a3c46688, with `--base dev`: Tests 45630399861,
  Build 45630399925). The log of each group shows that its tests ran and passed: Skia (`Map_Skia.Tests` 820, the
  `CourseSvgBaselineTests` of `Map_Svg.Tests` 12, Graphics2D 23, the view models 135, TestUtil 2), MapModel 423,
  PurplePen 1 290, PDF 134, WPF 76 and PdfConverter 7, and the projections 3 241 (189 skipped). The Windows build had
  no error, and the trial packages for Linux were published (`trial-svg-encoding`).
* The SVG loop of bd0c94c3f (run 45626960318): 881 passed, 2 failed, 2 pending on calls that the target never
  honors, 14 set apart for a browser. The failures were Firefox's images of teanwest and teanwest11, with 32 198
  pixels outside the tolerance where 32 165 and 32 167 were when they were set apart (6e5aaab61). The geometry check
  read 137 911 drawings of 196 cases, and none lay beyond its allowance (at most 0.993 of it, LordHill).
* The full run of bd0c94c3f (Tests 45627101205, Build 45627100898) failed in the groups Skia
  (`Pass2DoesNotCallTheDrawingCodeOfTheSymbols`, `Roboto`) and PurplePen (`BitmapCreation1`, 4 and 5). The collecting
  run 45627123574 gave the JPEG baselines. 1ca7a4ab3, 969f20236 and c04f1fb49 answer them.
* The comparison references that move (68 images, 407 377 pixels). A pixel can move in more than one step: from the
  references of 1088d1295 to the old drawing of the new dev, then to the Skia encoding with each area's own pattern,
  then to the area clusters. The old drawing of the new dev moves 383 343 of them; the last column names the commits
  of the new dev that moved the Skia baseline of the same map area. The encoding moves 22 907: 11 429 in each of
  LordHill and LordHill11, whose one palette color below full opacity, 201 'Purple' at 0.8, the old drawing does not
  apply (reason 1 of ticket 010), and the others on the edges of the expansions of lines and in the framing of
  frametext1 (reason 3), as enlarged crops show. The area clusters move 2 531, on the unions and their edges (ticket
  028). The vector brushes of the old path move none, as the references of 1088d1295 drew the patterns of a map
  element by element. The images of the steps were drawn locally with experiments of the drawing that are not
  committed: the old drawing of the new dev (`RenderOptions.drawColorsDirectly`, no pattern brushes), and the Skia
  encoding with each area's own pattern.

| Image | Moved | Old drawing of the new dev | Encoding (ticket 010) | Area clusters (ticket 028) | Commits that moved the Skia baseline of the same map area |
| --- | ---: | ---: | ---: | ---: | --- |
| `FtCasey` | 2 525 | 2 525 | 2 | 0 | a777b4aab, 17c15521e, aa422a6d7 |
| `LordHill` | 31 168 | 19 889 | 11 429 | 8 | a777b4aab, 1530dabda, 1b2820469, 17c15521e, aa422a6d7, b99067b53 |
| `LordHill11` | 16 807 | 5 399 | 11 429 | 3 | a777b4aab, 1530dabda, 1b2820469, 8127117f2, 998cf61f7, b99067b53 |
| `PenistoneHill` | 10 788 | 10 782 | 2 | 6 | a777b4aab, 17c15521e, aa422a6d7, b99067b53 |
| `angledashes9` | 3 458 | 3 458 | 9 | 0 | 1b2820469, 47471041c |
| `badpointyends` | 272 | 272 | 0 | 0 | 47471041c |
| `borderedarea9` | 3 172 | 3 172 | 0 | 0 | 47471041c |
| `corner_ends` | 2 486 | 2 486 | 0 | 0 | 17c15521e, aa422a6d7 |
| `corner_ends9` | 2 486 | 2 486 | 0 | 0 | 17c15521e, aa422a6d7 |
| `cutareaborder9` | 3 211 | 3 211 | 0 | 0 | 17c15521e, aa422a6d7 |
| `cutdoublesides6` | 121 | 121 | 0 | 0 | ca8bd2184, 363615c74 |
| `cutdoublesides9` | 121 | 121 | 0 | 0 | ca8bd2184, 363615c74 |
| `dashlengths6` | 210 | 210 | 0 | 0 | 47471041c |
| `dashlengths9` | 210 | 210 | 0 | 0 | 47471041c |
| `dashline` | 611 | 611 | 0 | 0 | 3baf74a0f, 47471041c |
| `dashline9` | 611 | 611 | 0 | 0 | 3baf74a0f, 47471041c |
| `dashmin` | 1 253 | 1 253 | 0 | 0 | 8127117f2, 47471041c |
| `dbldashlengths7` | 160 | 160 | 0 | 0 | 47471041c |
| `dbldashlengths9` | 160 | 160 | 0 | 0 | 47471041c |
| `decrease` | 34 038 | 34 038 | 0 | 0 | 17c15521e, aa422a6d7, b99067b53 |
| `differentnewlinetypes` | 202 | 202 | 0 | 0 | a777b4aab, 47471041c |
| `doublelines6` | 1 899 | 1 899 | 0 | 0 | 47471041c |
| `doublelines9` | 1 899 | 1 899 | 0 | 0 | 47471041c |
| `fences` | 1 260 | 1 260 | 0 | 0 | a777b4aab, 3baf74a0f, 47471041c |
| `fences9` | 1 260 | 1 260 | 0 | 0 | a777b4aab, 3baf74a0f, 47471041c |
| `frametext1` | 1 | 0 | 1 | 0 | a777b4aab, 1b2820469 |
| `framingline-test` | 3 496 | 3 496 | 0 | 0 | 47471041c |
| `framingline-test9` | 3 496 | 3 496 | 0 | 0 | 47471041c |
| `isomlines` | 81 | 81 | 0 | 0 | none |
| `isomlines9` | 81 | 81 | 0 | 0 | none |
| `isomlines9_light` | 81 | 81 | 0 | 0 | none |
| `isomlines_light` | 81 | 81 | 0 | 0 | none |
| `linegaps10` | 858 | 858 | 0 | 0 | 17c15521e, aa422a6d7 |
| `lksamm1` | 6 419 | 6 419 | 0 | 0 | a777b4aab, 17c15521e, aa422a6d7, b99067b53 |
| `lksamm11_1` | 1 328 | 1 328 | 0 | 0 | a777b4aab, 17c15521e, ca8bd2184, 3baf74a0f, 3ab5d9edb |
| `lksamm11_3` | 3 724 | 3 723 | 1 | 2 | a777b4aab, 1530dabda, 17c15521e, ca8bd2184, 3ab5d9edb |
| `lksamm11_4` | 507 | 507 | 0 | 0 | a777b4aab, 17c15521e, 3ab5d9edb |
| `lksamm12_1` | 1 328 | 1 328 | 0 | 0 | a777b4aab, 17c15521e, ca8bd2184, 3baf74a0f, 3ab5d9edb |
| `lksamm12_3` | 3 724 | 3 723 | 1 | 2 | a777b4aab, 1530dabda, 17c15521e, ca8bd2184, 3ab5d9edb |
| `lksamm12_4` | 507 | 507 | 0 | 0 | a777b4aab, 17c15521e, 3ab5d9edb |
| `lksamm3` | 11 963 | 11 963 | 0 | 3 | a777b4aab, 1530dabda, 17c15521e, aa422a6d7 |
| `lksamm4` | 3 660 | 3 660 | 0 | 0 | a777b4aab, 17c15521e, aa422a6d7 |
| `lksamm9_1` | 6 419 | 6 419 | 0 | 0 | a777b4aab, 17c15521e, aa422a6d7, b99067b53 |
| `lksamm9_3` | 11 963 | 11 963 | 0 | 3 | a777b4aab, 1530dabda, 17c15521e, aa422a6d7 |
| `lksamm9_4` | 3 660 | 3 660 | 0 | 0 | a777b4aab, 17c15521e, aa422a6d7 |
| `mainglyph7` | 28 158 | 28 158 | 0 | 0 | 17c15521e, aa422a6d7 |
| `mainglyph9` | 28 158 | 28 158 | 0 | 0 | 17c15521e, aa422a6d7 |
| `marymoor` | 27 273 | 27 250 | 0 | 119 | a777b4aab, 17c15521e, aa422a6d7, b99067b53 |
| `marymoor11` | 4 319 | 4 292 | 0 | 31 | a777b4aab, 8127117f2, 998cf61f7, b99067b53 |
| `marymoor11_lowerlayers` | 1 | 1 | 0 | 0 | none |
| `marymoor11_upperlayers` | 4 315 | 4 290 | 0 | 29 | a777b4aab, 8127117f2, 998cf61f7, b99067b53 |
| `max2glyph7` | 1 357 | 1 357 | 0 | 0 | 3baf74a0f, b99067b53 |
| `max2glyph9` | 1 357 | 1 357 | 0 | 0 | 3baf74a0f, b99067b53 |
| `missingcolor` | 6 223 | 6 222 | 0 | 20 | 17c15521e, aa422a6d7 |
| `multimainglyph7` | 9 708 | 9 708 | 0 | 0 | 17c15521e, aa422a6d7 |
| `multimainglyph9` | 9 708 | 9 708 | 0 | 0 | 17c15521e, aa422a6d7 |
| `multisymonedash` | 53 | 53 | 0 | 0 | 3376cb041 |
| `pointyends6` | 2 723 | 2 723 | 0 | 0 | 47471041c |
| `pointyends9` | 2 723 | 2 723 | 0 | 0 | 47471041c |
| `punchboxbug` | 6 646 | 6 645 | 0 | 2 | a777b4aab, 17c15521e, aa422a6d7, b99067b53 |
| `secglyph7` | 5 477 | 5 477 | 0 | 0 | 47471041c |
| `secglyph9` | 5 477 | 5 477 | 0 | 0 | 47471041c |
| `startendglyph7` | 359 | 359 | 0 | 0 | 47471041c |
| `startendglyph9` | 359 | 359 | 0 | 0 | 47471041c |
| `teanwest` | 68 335 | 67 719 | 16 | 1 390 | a777b4aab, 1530dabda, 1b2820469, 17c15521e, aa422a6d7, b99067b53 |
| `teanwest11` | 8 934 | 8 289 | 17 | 913 | a777b4aab, 1530dabda, 1b2820469, 8127117f2, 998cf61f7, b99067b53 |
| `template` | 803 | 803 | 0 | 0 | 47471041c |
| `template2` | 1 146 | 1 146 | 0 | 0 | 682285053 |

  suo, suo2 and hakolaoverprinting_overprinted get their first references.
* The baselines of the SVG export change with the structure of the encoding and the course object keys. The record
  of a symbol definition lists the palette colors in which the encoding writes it: symbol 23 of Madrona Permanent11
  no longer lists the color 160, in which the export drew nothing of it. The color Red of printarea2 carries no
  overprint, as the new dev reads the overprinting of a Mapper color without a spot definition as a knockout.
* The browser check: Firefox's numbers of teanwest and teanwest11 move from 32 165 and 32 167 to 32 198. The 14 other
  documents set apart keep their numbers. The loop of 1088d1295's time (43563468407, svg-results/svg-jpeg-data) had
  868 passed, 0 failed, 2 pending and 16 set apart. The pending cases are the same, clouds and clouds11, which now wait
  on a `StrokedShape` of no width where they waited on a `DrawPolyline`, and so are the 16 documents set apart. Each
  browser compares 218 documents where it compared 215: suo, suo2 and hakolaoverprinting_overprinted are new.

## Issue ticket number
[030-svg-encoding](../tickets/030-svg-encoding.md)

## Missing functionality
* The union form of the SVG and the stroke shape, a ticket of their own after this one.
* The common structure of the vector files with the content of `Map`, and the PDF encoding.

## Changes to be logged
SVG-vienti on Tiedosto-valikossa. Kartan palettivärit kirjoitetaan SVG:hen palettivärin läpinäkyvyysryhminä, ja
kunkin symbolin elementit kantavat symbolinsa avaimen. Asetus "Draw area patterns and dashed lines as separate
shapes" piirtää aluekuviot ja katkoviivat erillisinä muotoina lukijalle, joka ei osaa täyttää aluetta kuviolla eikä
piirtää katkokuviota.
