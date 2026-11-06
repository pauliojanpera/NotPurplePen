# SVG export in the application: design

Phase 3's first stage (see `SvgTargetProgress.md`, Tasks): where the export lives, how it shares PDF export's code
(A-3), and how `MapDisplay` draws a map's parts and a course part's layers apart (A-4, L-2 to L-4). Written on Oct 25
from `feature/svg-target` at b091333.

## Where the export lives

- `CourseSvg`, in `PurplePenCore` beside `CoursePdf`, writes the files. `CourseSvgSettings` (A-2) holds PDF export's
  course, variation, map exchange, base map, description, crop, color model, directory and prefix fields, with CMYK as
  the default color model, plus the bitmap handling, JPEG quality, compression and whether area patterns are drawn as
  separate shapes.
- `PurplePenCore` references `Map_Svg`, which depends on `Graphics2D` alone, so no platform code comes with it.
- Skia reaches the export only through `Services` (A-5): `Services.TextMetricsProvider`, registered in AvPurplePen's
  `Program.cs` as `Skia_TextMetrics`, lays the text out, and a new `Services.FontSubsetProvider`, registered there as
  `SkiaFontSubsetProvider`, gives the embedded faces. The tests register the same pair.
- Each map's georeference comes from `coordinateMapper.GetMapGeoreference()` through `SetGeoreference` once its first
  part has begun (G-1, G-2).

## What is shared with PDF export (A-3)

`CoursePdf` holds four things that are not about PDF. They move to shared code, and `CoursePdf` calls them, so its
output stays the same; its baseline tests check that.

1. **The files and what goes in each:** `GetFilesToCreate` and `CreateOutputFileName`, given the file choice, the
   courses, the variations, the map exchange choice, the prefix, the directory and the extension. Phase 3 writes one
   page per file, so a course part that takes several pages gets the page number as a suffix; the other choices of
   L-7 come in phase 5.
2. **The pages:** `CoursePageLayout.LayoutPages`, already shared, with the crop setting.
3. **The course drawn on a page:** the course view, the purple that `FindPurple.GetPurpleColor` finds, the
   `CourseLayout` with its layer colors, and `CourseFormatter.FormatCourseToLayout` with the description option, from
   `DrawPage` into one method that both exports call.
4. **The page's transform:** `Geometry.CreateInvertedRectangleTransform(mapRectangle, printRectangle)`, in
   hundredths of an inch. The SVG page is in millimetres, so its transform is that one scaled by 0.254, and its paper
   size is the page's in inches, turned for landscape, times 25.4.

## Drawing a map's parts and a course part's layers apart (A-4)

`MapDisplay.DrawHelper` draws one page in up to four steps. When the event has a lower purple layer
(`LowerPurpleMapLayer`), they are: the map up to that color, the course below the white-out color (its lower layer),
the map above that color, and the rest of the course; otherwise the whole map and then the whole course. Then the
print area's outline.

The four steps become one public method, `Draw(IGraphicsTarget, MapDisplayLayer, RectangleF visRect, float
minResolution)`, with `MapDisplayLayer` naming `MapBelowCourse`, `CourseLower`, `MapAboveCourse` and `CourseUpper`, and
`DrawHelper` calls it four times, so the screen, bitmap, PDF and print output stay as they are. Without a lower purple
layer, `MapBelowCourse` is the whole map and `CourseLower` the whole course, and the other two draw nothing, which makes
the map one part (L-2).

`CourseSvg` then writes, for each map (one per event today, L-1 allowing more):

- the part `MapBelowCourse` over the union of the map rectangles of every page that shows the map (L-4), and the part
  `MapAboveCourse` likewise, each as `BeginPart`, `BeginMap` and the drawing, at a minimum resolution of 0.001 mm, the
  step of S-1;
- for each page, the parts `CourseLower` and `CourseUpper` of its course part, over its map rectangle;
- `AddPage(paper size, map rectangle, transform, [MapBelowCourse, CourseLower, MapAboveCourse, CourseUpper])` (L-3).

`MapDisplay` is set up as `CoursePdf` sets it: map intensity 1, `Printing`, the chosen color model, and no print area,
so no outline. For the SVG target, whose pattern brushes are vectors, it still draws regular area patterns and hatchings
with pattern brushes, unless the settings ask for area patterns as separate shapes (S-7; see "Area patterns as pattern
fills" below). A PDF map, which PDF export copies as vectors, is drawn as the bitmap `MapDisplay` makes of it (B-1 to
B-6).

## Course data (stage 2, CD-1 to CD-3)

- **Which symbols each course object adds (CD-3).** `CourseLayout.RenderToMap` gets an overload that also fills a
  dictionary from each map `Symbol` to the `CourseObj` that added it, by the map's symbols before and after each
  `CourseObj.AddToMap`. The map code already opens each symbol as an object, with the `Symbol` as its key
  (`Map.Draw`'s `BeginObject(curSym, ...)`), so the key reaches the target unchanged.
- **What the target is told.** Two calls join `IStructuredGraphicsTarget`, with plain data only, as `Graphics2D`
  carries no course types (`Graphics2D/CourseRecords.cs`):
  - `DescribeCourse(object mapKey, CourseRecord course)`, with the course's map: the name, part, variation and print
    scale, and the controls in order, each with its code, kind and position in map coordinates (CD-1);
  - `DescribeObjects(object mapKey, IReadOnlyDictionary<object, CourseObjectRecord> objects)`: for each object key of
    the course's map, its kind and its controls, by their places in the course's list: one for a control circle,
    start, finish or control number, two for a leg, none for a description or a special object (CD-2).
- **What the document writes.** Elements keep only their object number (S-11). The course map's `<npp:map>` record
  gains `<npp:course name="..." part="..." variation="..." scale="...">` with an `<npp:control n="..." code="..."
  kind="..." x="..." y="..."/>` for each control, its position in steps of map as the drawing writes coordinates, and
  an `<npp:object n="..." kind="..." controls="..."/>` for each described object that drew an element, keyed by the
  number its elements carry. The names are a proposal, to settle with @Pauli before the format's version is raised;
  version 1 only gains elements.
- **Kinds.** From the `CourseObj` class: `ControlCourseObj` control, `StartCourseObj` start, `FinishCourseObj`
  finish, `ControlNumberCourseObj` control-number, `LegCourseObj`, `FlaggedLegCourseObj` and `TopologyLegCourseObj`
  leg, `DescriptionCourseObj` description, and each special its own kind; controls from `courseControlId`, and for a
  leg from both ends.

## Baseline tests (V-2, V-4, V-5)

`CourseSvgBaselineTests` in `Map_Svg.Tests`, beside `CourseSvgExportTests`, so that the SVG loop runs them on Linux
(V-4): each exports a reference event's courses, compares each file's text with a baseline committed in
`src/TestFiles/svg_export`, and writes a missing or changed file to the loop's results, from where it is committed, as
the comparison does with its references. Each file is also exported a second time and compared byte for byte, and the
results record each file's size, plain and gzip-compressed, which the loop's summary reports (V-5). The renderings of
the comparison decide whether the drawing is right; the baselines show in review what changed in the text.

The set, from events whose maps load on a case-sensitive file system, smallest maps first:

- `controller/mapexchange1.ppen`: courses with map exchanges, and no map, so its files are small;
- `courseprinting/Overprint test.ppen`: an OCAD map with overprinted colors and the course's purple blended;
- `controller/Lincoln Park.ppen`: a bitmap map, which the files embed;
- `findpurple/OOM_ISOM_Blank.ppen`: an OpenMapper map, its all-controls page;
- `controller/Madrona Permanent11.ppen`: an OCAD map with area patterns, multi-colored point symbols and text along
  lines;
- a georeferenced OCAD map among the test maps, to be found.

Several events name their maps with another case than the files have, such as `Marymoor.ocd` for `marymoor.ocd`,
and so load only on Windows; spelling them as the files are spelled belongs on `fix/case-of-test-file-names`.

## Stages after this one

1. **The refactor:** `MapDisplay.Draw` by layer and the code shared with `CoursePdf`, on `feature/svg-export` rooted at
   `feature/svg-target`; the PDF and bitmap baseline tests must not move.
2. **Course data (CD-1 to CD-3):** `CourseLayout` records the map symbols each course object adds (CD-3); the export
   passes the target a lookup from those objects to their course, kind and controls as plain strings, and the course
   metadata of CD-1 through one new `IStructuredGraphicsTarget` call per course.
3. **The core:** `CourseSvgSettings`, `CourseSvg` and the baseline tests: a set of reference events exported to SVG and
   compared with committed baselines (V-2), each exported twice for identical bytes and run on Linux too (V-4), with
   the sizes in the job summary (V-5), and the structure walk of V-7. They need a test project that runs on Linux,
   unlike `PurplePen_Tests`.
4. **The dialog and command (A-1, A-6, A-7):** `CreateSvgFilesDialog` and its view model, on the pattern of
   `CreatePdfCoursesDialog`; the command in `MainWindowViewModel_Commands.cs`, keeping the settings until another map
   loads; the overwrite confirmation through `OverwritingFilesDialog`; the strings in `UIText.resx`, after checking
   which exist.

## Phase 5: several pages per file (L-6, L-7, V-8)

Written on Nov 4 from `feature/svg-target` at 3c40d20; the work is on `feature/svg-pages`. It answers the first open
question below: the dialog shows the three choices, all of them working.

### What goes in a file (L-7)

- `CourseSvgSettings` gains PDF export's `FileCreation`, of PDF export's type `CoursePdfSettings.PdfFileCreation`:
  all pages in one file, a file per course, or a file per course part. Its default is PDF export's, a file per course,
  as @Pauli settled on Nov 4, though a browser shows only a file's first page.
- `CourseSvg` takes the files and their course designators from `CourseOutputPages.GetFilesToCreate` with that choice,
  as `CoursePdf` does, and every page that `CoursePageLayout` lays out for a file goes in that file. A course part that
  takes several pages, with the crop setting off, is one file of several pages, as in PDF export, so phase 3's
  numbered files (A-3) no longer arise.
- The dialog shows PDF export's "Files:" choice where it said that each page is a file, with the PDF dialog's strings,
  which are already translated: `CreatePdfCourses_filesLabel_Text` and `CreatePdfCourses_comboBoxFileFormat_Items` to
  `_Items2`. `CreateSvgCourses_filesInfoLabel_Text` goes. The view model's `FileFormatIndex` numbers the choices as
  `PdfFileCreation` does, as the PDF dialog's view model does.

### How a file is drawn (L-2 to L-4)

For each file, `CourseSvg` lays out its pages and then draws each map once: the base map's parts over the union of the
map rectangles of all the file's pages, and each course part's two layers, a course drawing being a labeled map of its
own (M-5), over the union of its own pages' rectangles. The parts are drawn in the order the pages first use them,
from the bottom up within a page: for the first page the map below the course, its course part's lower layer, the map
above it and the part's upper layer, as in phase 3; after them, the two layers of each further course part. A file of
one page is then written as before, apart from what L-6 adds. Each course part is described (CD-1 to CD-3) once, under
its own map key, while its layout is in the map display. Then the file gets one `AddPage` for each page, in the order
of the layout, each with the base map's parts and its own course part's (L-3).

### What a page carries (L-6)

- `AddPage` gains a last parameter, `name`, what the page is called; `CourseSvg` passes `CoursePage.description`, its
  course part's full name, such as "Course 5-2" or "All controls". This changes the interface @Pauli approved, and
  @Pauli accepted it on Nov 4.
- Each page's group follows a `<view>` whose box is the page's paper on the canvas, which holds the print area where
  the paper prints it: `viewBox="left 0 width height"`. The first page's is the root's view box (F-4), so `file.svg`
  and `file.svg#<first page>` show the same.
- The view's id is the page's name as an XML name: each run of characters that a name cannot hold becomes an
  underscore, and one goes before a name that does not begin with a letter or an underscore ("Course 5-2" becomes
  `Course_5-2`, "1" `_1`). A name an earlier page has takes the first free of `_2`, `_3` and so on, so the pages of a
  course part that takes several are `X`, `X_2`, `X_3`. A page without a name is called `page`. The ids the document
  names itself skip the views' ids, so every id stays unique (L-5).
- After the metadata, a `<sodipodi:namedview>` holds an `<inkscape:page x="..." y="..." width="..." height="..."
  inkscape:label="name"/>` for each page, in order, with the page's name as given. Inkscape reads its pages only as
  children of that element, of Inkscape's other namespace, which F-3's list of elements names since
  @Pauli settled it on Nov 4. Inkscape keeps its first page at the root's view box, as the document does.
- Nothing joins the metadata: a page's maps are found through its uses (part, `data-m`, `npp:map`), and its course
  through its course map's `npp:course`.

### The layout test (V-8)

`CourseSvgLayoutTests`, in `Map_Svg.Tests` beside the export's other tests, exports reference events with a file
choice and checks each file:

- it holds as many views, Inkscape pages and page groups as the layout gives the file pages, in the same order; each
  view's box and each Inkscape page's are the page's paper, the pages laid left to right 10 mm apart; and the root's
  view box, width and height are the first page's;
- each map is defined once: one record each, at most one part for each of its layers whatever the number of pages,
  and every part shown by some page;
- each page uses the base map's parts and its own course part's, in the order of L-3, and its view is named after
  that course part, the label of the course map it uses;
- the structure walk of V-7 holds;
- through its view, `<img src="file.svg#id">` sized to the page, Chromium draws each page as the application draws it
  with the Skia target, within the tolerance of `CourseSvgPageTests`, so the view shows the page's own print area and
  nothing of the pages beside it.

The events: `courseprinting/Lincoln Park PrintAreas 2.ppen`, an OCAD map whose courses print on five paper sizes and
orientations, all in one file; `Overprint test` in upper and lower purple, all in one file, its map in two parts
shared by its pages, with Course 12's print area made twice as wide and high, so that its nine pages share one course
drawing; and `mapexchange1` with a file per course, so that Course 2 and Course 5 hold their parts' pages. The baseline
tests (V-2) gain `Overprint test` in upper and lower purple in one file and `mapexchange1`'s Course 5 in a file of
its own, so that the text of a file of several pages shows in review.

A page's paper size is taken from whole hundredths of an inch in double precision, since the inches are held in
single precision and wrote a 9 inch side as 228.59999 mm, and pages lie at whole micrometres.

## Colours for Scribus and Inkscape (S-4, S-8, COL-1, COL-3, F-6)

Written on Nov 4 from `feature/svg-target` at 756543f; the work was on `feature/svg-colours`, and is 9fe17bd on
`feature/svg-target`. @Pauli chose the form that day from task 5's findings (`SvgColourReaders/README.md`, option 2
with the device CMYK profile), and on Nov 5 the outermost group for a shared style rather than the innermost
(`feature/svg-style-groups`). The change is the target's alone, in `Map_Svg`: the export and its settings stay as they
are.

- **Styles.** The target keeps each element's style as now, as declarations and the pattern they paint with. The
  writer no longer names classes; after `TidyChildren` has tidied a part's groups or a pattern's tile (S-8), a pass
  puts styles on groups: a group takes the style that every element within it, at any depth, has, unless an
  enclosing group carries one already, and those elements then carry none. A style thus sits on the outermost group
  whose elements share it: on a symbol's group rather than on each of its objects' groups, and on the group around a
  description's cells rather than on each cell. 9fe17bd put it instead on each group that holds elements of its own,
  the innermost. Every other element writes its style itself. A group's own declarations, anti-aliasing or blending, come first in
  its `style` attribute, before its elements' style. A part's group and a pattern's tile are written without
  attributes, so the elements directly within them carry their own styles.
- **Everything the classes styled.** A clip path under the even-odd rule carries `style="clip-rule:evenodd"`, the use
  of a pixelated image its `image-rendering`, and a run of a fallback face its `tspan`'s `font-family`. A family is
  quoted in single quotes, in the attributes and in the `@font-face` rules alike, so that no attribute needs
  `&quot;`, and every attribute's value is escaped for XML.
- **The style sheet** keeps the `@font-face` rules and the rule that all text follows, and is written only when the
  document draws text. Scribus reads neither: it draws text in installed fonts by their family, as Inkscape does, and
  knows none of the rule's four properties.
- **The profile.** `SvgPaints` holds the 516 bytes of `SvgColourReaders/device-cmyk.icc`, which `make_profile.py
  naive 2` writes, and the document embeds them as `data:application/vnd.iccprofile;base64,` in its
  `<color-profile id="cmyk" name="cmyk">` whenever a color refers to it. Under the CMYK color model the id `cmyk` is
  kept from every other id and from the pages' views (L-5).
- **Tests.** `SvgDocumentTests` expect the styles where the pass puts them; the renderings of the comparison and of
  the course pages must not move, since Chromium draws as before; the export baselines (V-2) move in the styles' form
  alone; and F-6 is read again with `SvgColourReaders/` on documents the new code writes.

The findings predicted the size, and SVG loop run 41895993797 of the change measured it: the 22 files of the baseline
tests come to 6,972,977 bytes, 3.2 % less than 7,204,808, and to 3,154,577 gzip-compressed, 0.1 % more. Small course
pages grow by about a kilobyte, 751 bytes of it the profile (Overprint test's Course 12, 13.9 %); maps shrink, as most
of their elements lose their class (printarea2 3.7 %, Madrona Permanent11 11.8 %, Lincoln Park PrintAreas 2 13.7 %, as
predicted); and a description sheet grows, as each of its cells holds its texts in a group of its own, which then
carries the whole text style (mapexchange1's all-controls page, 16.6 %).

On the outermost group, a style is written once for a symbol's objects rather than once for each object or description
cell, and SVG loop run 42064035635 measured it against 9fe17bd's run: the 22 files come to 6,863,687 bytes, 1.6 % less
than on the innermost group and 4.7 % less than with classes, and to 3,153,798 gzip-compressed, as before. Over the
classes' form, mapexchange1's all-controls page grows 5.9 % rather than 16.9 %, and printarea2's map shrinks 7.7 %
rather than 3.7 %, as a sketch on the baselines estimated; testproj2's all-controls page grows 9.8 % rather than
23.3 %, and Madrona Permanent11 shrinks 12.7 % rather than 11.8 %. Small course pages keep most of their growth, more
than half of it the profile (Overprint test's Course 12, 18.3 %). The SVGs of the comparison come to 102,291,992 bytes,
against 114,374,247; no measurement moved, and Chromium draws each export as before, pixel for pixel. Inkscape shows a
colour on the group that carries it, a symbol's or a whole run's, rather than nearer each object.

## Area patterns as pattern fills (S-7, S-12, S-13)

Written on Nov 6 from `feature/svg-target` at 010704a; the work is on `feature/svg-area-patterns`, with the map code's
fix on `fix/pattern-brush-origin`. The export writes the regular patterns of area symbols as pattern fills, as Purple
Pen draws them on screen (@Pauli, Nov 4), and the hatchings as pattern fills too, each of one of its lines: a conscious
deviation from PDF export, which draws every pattern element by element and every hatching line by line (@Pauli,
Nov 6). The export's setting "Draw area patterns as separate shapes" draws them all as PDF export does (@Pauli, Nov 6).

- **Which patterns become fills (S-7).** `IStructuredGraphicsTarget.PatternBrushesAreVectors` says whether a target
  keeps what is drawn into a pattern brush's tile as drawing, rather than as a bitmap of the size `CreatePatternBrush`
  suggests. The SVG target says so, and so does `StructuredRecordingGraphicsTarget`, which records the comparison's
  cases for it; `GraphicsUtil.PatternBrushesAreVectors` asks it of any target. For such a target
  `MapDisplay.CreateRenderOptions` turns pattern brushes on, for printing too; `AreaSymDef.DrawPatterns` skips
  `PatternBrushTooPixelated`, which at the export's step of 0.001 mm refuses a brush for any tile wider or taller than
  0.08 mm; and `AreaSymDef.DrawHatchings` fills each hatching with a brush of one of its lines
  (`HatchingIsFilledWithBrush`). Every other target draws as before, so the screen, print, PDF and bitmap output do not
  change. A pattern becomes a fill only when it is regular and in clip mode. An irregular pattern, such as an OCAD
  structure whose elements vary in place, and a pattern that draws only the elements lying wholly, partly or by their
  middles within the area, are drawn element by element, as on screen: a fill repeats one tile everywhere and cuts it at
  the area's edge, so it cannot draw them the same. A hatching whose spacing is not above zero draws no line, and is
  left to the line-by-line drawing, which draws none either.
- **Area patterns as separate shapes (S-7, A-2).** `CourseSvgSettings.DrawAreaPatternsAsSeparateShapes`, off by default,
  and its check box "Draw area patterns as separate shapes" in the dialog's Appearance group draw every area pattern
  element by element and every hatching line by line, each element and each line a shape of its own, as PDF export
  draws them. `CourseSvg` sets `MapDisplay.AreaPatternsElementByElement` from it, which turns pattern brushes off for
  every target. With it on, the export draws as PDF export does: Madrona Permanent11 exported so is byte for byte what
  010704a exported. The setting serves readers that cannot fill an area with a pattern. Scribus 1.6.1 is one: it makes
  each `<pattern>` a pattern of its document but fills no area with it, so it shows the map without its patterns and
  hatchings (F-6 below).
- **Where a fill puts the pattern (S-13).** Element by element, `DrawPattern` lays the pattern's grid out in a frame
  that moves to the area's rotation center, turns by the area's angle and then the pattern's, and moves by the pattern's
  offset, with every second row half a tile along when the rows are offset. The brush's tile holds the glyph at the
  pattern's offset, unturned within the turned tile, and two rows of it in a tile twice as high when the rows are
  offset; the target repeats the tile from the origin of the coordinates the fill is made in, turned by the pattern's
  angle (D-7). A fill made in the frame that moves to the rotation center and turns by the area's angle therefore puts
  every element where `DrawPattern` puts it. Line by line, `DrawOneHatching` draws a hatching's lines in a frame that
  moves to the area's rotation center and turns by the area's angle and then the hatching's, or for a hatching that
  never turns by the hatching's angle alone, at the hatching's offset and at every spacing from it. Its brush, which
  `CreateHatchBrush` makes, has a square tile as wide as the spacing, turned by the hatching's angle, holding the line
  through the offset nearest the middle of the tile and the lines a spacing from it whose width reaches into the tile;
  each line runs on past both sides of the tile, so that the tiles join without a seam. A fill in the area's frame
  therefore puts every line where `DrawOneHatching` draws it. `DrawOneHatching` draws every line whose middle lies
  within half a spacing of the area's bounds, and a line further out reaches into the area only when it is wider than
  its spacing, when the lines drawn cover the area already; so the fill draws what the lines draw, and no hatching was
  found that a fill cannot draw the same. `FillInPatternFrame` makes the fill of a pattern or a hatching in the area's
  frame, clipped to the area, whenever the angle is not zero or the rotation center is not the map's origin, and
  otherwise fills the area's outline in map coordinates. An OCAD object turns its pattern about the map's origin. An
  OpenMapper object carries a pattern origin of its own, which import makes its rotation center; until
  `fix/pattern-brush-origin` (b217b9d) the brush took the frame only for a turned area, so an unturned one with a moved
  origin had its pattern laid out from the map's origin, on screen as in the SVG, where printing lays it out from the
  area's origin. The fix is Purple Pen's own, rooted at `upstream/master` and offered with the other `fix/` branches;
  `PatternBrushTests` in `Map_Skia.Tests` draws areas both ways with Skia, turned and not, about the map's origin and
  about a moved one, and requires them to agree. No baseline moves with it, as no test map has an unturned OpenMapper
  area with a moved origin.
- **One definition for each pattern and color, and for each hatching (S-12).** The map code makes one brush for each
  pattern of an area symbol in each color it draws in, and one for each hatching, which has one color, keyed in the
  symbol definition, and asks the target whether it has it before drawing the tile; so a document defines each once,
  as a `<pattern>` in the user space of what it paints, which within a part is steps of map. An unturned area whose
  pattern origin is the map's fills its outline with it, so the grid or the lines run on across neighbouring areas from
  the map's origin, as OCAD draws them. A turned area, or one with an origin of its own, fills a rectangle around its
  outline, clipped to the outline, within a group that moves to its origin and turns by its angle: the transform is on
  the area's fill, and every area shares the definition. Equal patterns of different symbols stay separate definitions,
  which S-12 does not ask to share. So do a course's: each course part draws its course with symbols of its own, so a
  file of several parts defines the hatchings of a dangerous area once a part, as mapexchange1's Course 5 in one file
  defines its two four times, 1,780 bytes more than once. S-12 is met, so phase 6 keeps symbol reuse, path merging and
  compression.
- **What the document holds.** A `<pattern>` with its tile for each brush, as D-7 writes it; in the part, each area's
  path with `fill:url(#…)`, or its rectangle within the group of its frame. A pattern's tile holds each copy of the
  glyph that the map draws across the tile's edges, and the pattern's own bounds cut them off; a copy that lies wholly
  outside the tile draws nothing and is still written: the map draws such copies to cover a glyph that its offset moves
  beyond the tile, and they cost a few hundred bytes in the reference exports. A hatching's tile holds one or two
  stroked paths in the hatching's color and width.
- **How the comparisons check it.** The references stay element-by-element and line-by-line drawings (V-9, F-1): the
  Skia target's pattern brushes are bitmaps, so neither a comparison case's reference nor `ExportedPage`'s drawing of a
  page uses them. The rendering comparison draws each map area for the SVG target as the export draws it, with pattern
  brushes where the target's are vectors (`ComparisonCase`), so every case with a regular pattern in clip mode or a
  hatching compares Chromium's drawing of the fills with Skia's drawing of each element and each line, and so do the
  comparisons of exported pages. Three cases join the comparison: `patternorigins` and `hatchingorigins`, four squares
  of a pattern and four squares of two hatchings built for them (`PatternedAreaMap`), unturned and turned, about the
  map's origin and about origins of their own, one hatching wider than half its spacing and never turning; and
  `rotarea_omap`, OpenMapper's `rotarea.omap` over the area its import tests draw, whose turned areas carry their own
  origins and hatchings among their patterns. `AreaPatternTests` check what the map writes into the target: one
  definition, the transforms of turned and moved fills, the tiles of hatchings, a hatching that never turns filling
  each outline as it lies, an irregular pattern and one outside clip mode element by element, and every pattern and
  hatching drawn as separate shapes. The baseline tests export Madrona Permanent11 a second time, its area patterns as
  separate shapes.

The drawings of 50 cases changed, and the pages of 12 files of the page comparisons, 20 pages in all, Lincoln Park
PrintAreas 2's five pages of the layout test among them; all lie within the tolerance, as do the three new cases.
Chromium puts the patterns and the lines where Skia's elements and lines are: by phase correlation, its images of
hatchingorigins, rotarea_omap, marymoor, lksamm4, isomarea and four more hatched cases lie within 0.04 pixels of their
references, and of five pattern cases within 0.03. They are blurrier: Chromium draws a pattern's tile into a bitmap of
whole pixels and scales that, so in hatchingorigins 83,615 of the 640,000 pixels differ from the reference by more than
32 levels, and in offsetpattern the mean difference rose from 0.21 to 0.72 levels. Where a screen's dots are about a
pixel apart, as in teanwest, Chromium's tile averages them into a tint where Skia draws each dot, and 66,093 pixels
differ by more than 64 levels, against 306 at 010704a; each lies within the range its neighbours take in the reference,
which the tolerance allows, as it does for any edge.

Measured with `SvgTolerance/`'s scripts from local renderings, each case under the tolerance it is compared with (the
text tolerance for the 22 of them that draw text), every real rendering of the 53 cases changed or new passes, and the
rule fails all 53 shifted by two pixels, all 53 with features two pixels wider, 42 of the 50 with a solid feature
removed and all 53 with their ink six levels redder. The removals it passes are irregular3's, which every rule passes,
and seven of the cases that draw text, whose rule passes them as it did before: PenistoneHill, four of lksamm's and
teanwest's two.

The 22 files of the baseline tests come to 5,017,909 bytes, against 6,863,687 at 010704a, and to 3,032,188
gzip-compressed, against 3,153,798. Madrona Permanent11's Course 1 falls from 1,464,679 to 338,607 bytes (76.9 %, and
38.8 % gzip-compressed), printarea2's from 1,597,686 to 953,989 (40.3 %, and 13.6 %), mapexchange1's course pages,
which show its dangerous area, by 26 % to 46 % (8 % to 22 % gzip-compressed), and its page of all controls by 4.5 %;
the other events have neither area patterns nor hatchings. Of the two maps, the hatchings' share is small: 3.1 % of
Madrona Permanent11 and 2.3 % of printarea2. The SVGs of the comparison come to 30,070,181 bytes, against
102,291,992, most of it teanwest's 22.6 MB falling to 4.2 MB.

**Inkscape and Scribus (F-6).** Read with `SvgColourReaders/`:

- Inkscape 1.2.2 draws the fills, and shows on an area's group "Pattern fill" and the pattern's id. An element of a
  tile, selected, shows the profile `cmyk` and the C, M, Y and K written, for a fill and a stroke alike: 0, 0.27, 0.79,
  0 for rotarea's yellow dots, 0.76, 0, 0.91, 0 for a green line.
- Scribus 1.6.1 makes each `<pattern>` a pattern of its document, its elements in their CMYK colors, but fills no area
  with it, so the areas lose their patterns and hatchings. A fill that names a pattern without an `xlink:href` gets an
  empty pattern name, as `SVGPlug::parseStyle` takes the name from the pattern's reference rather than its id, and the
  item is drawn with no fill; a pattern reached through another's `xlink:href`, as Inkscape writes them, is linked, but
  its tile takes the numbers of its `width` and `height` as points, without the transform of the user space it is used
  in, so the export's tiles, in steps under the pages' scale of 0.001, would come out hundreds of times too large. Of
  Madrona Permanent11's page Scribus made 1,970 items, 11 of them its open sandy ground with no fill; of printarea2's
  8,539, 26 of them with no fill; of a square hatched as `hatchingorigins`'s are, both hatched areas with no fill.
  Drawn as separate shapes, the page of printarea2 gave it 20,442 items.

**Settled by @Pauli on Nov 6.**

1. Hatchings are pattern fills, as the regular patterns are: a conscious deviation from PDF export, which draws them
   line by line.
2. Pattern fills are the export's default for every reader, and the setting "Draw area patterns as separate shapes"
   draws every pattern element by element and every hatching line by line, for a reader that cannot fill an area with
   a pattern. A setting serves such a reader by what it does, and neither its name, its identifiers nor its text in
   the dialog name any reader.

## Open questions

- Phase 3 writes one page per file. Should the dialog already show L-7's three file choices, with all but one
  disabled, or only the one? (Phase 5 shows all three; see above.)
- A PDF map is drawn as a bitmap. Is that acceptable for the first release, or should such events be refused?
