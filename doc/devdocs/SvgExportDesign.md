# SVG export in the application: design

How the export and the target's later phases are built: a section for each, written before its work began and brought
up to date as built. The first sections are phase 3's first stage: where the export lives, how it shares PDF export's
code (A-3), and how `MapDisplay` draws a map's parts and a course part's layers apart (A-4, L-2 to L-4), written on
Oct 25 from `feature/svg-target` at b091333.

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
  - `DescribeCourse(object mapKey, CourseRecord course)`, with the course's map: the course's id in the event's .ppen
    file, its name, part, variation and print scale, and the controls in order, each with its id in the file, its code,
    its kind as the file names it and its position in map coordinates (CD-1);
  - `DescribeObjects(object mapKey, IReadOnlyDictionary<object, CourseObjectRecord> objects)`: for each object key of
    the course's map, its kind and its controls, by their places in the course's list: one for a control circle,
    start, finish or control number, two for a leg, none for a description or a special object (CD-2).
- **What the document writes.** Elements keep only their object number (S-11). The course is written in the terms and
  ids of the event's .ppen file, as @Pauli chose on Nov 4, in the structure of the course map's record. That
  `<npp:map>` record gains `<npp:course id="..." name="..." part="..." variation="..." print-scale="...">`: `id` is the
  course's id in the .ppen file, left out for a page of all controls, which is no course of the file, and
  `print-scale` is the name the file gives the print scale. The course holds an `<npp:control n="..." id="..."
  code="..." kind="..." x="..." y="..."/>` for each control in the order the course visits it: its place, its id in the
  .ppen file, its code, its kind as the file names it, and its position in steps of map, as the drawing writes
  coordinates. After the course comes an `<npp:object n="..." kind="..." controls="..."/>` for each described object
  that drew an element, keyed by the number its elements carry and naming its controls by their places; the .ppen file
  records nothing of what is drawn, so the object has no counterpart there. Nesting the file's own course data would
  have carried every part and relay fork of a course into each runner's file.
- **The pages that show a course (CD-1).** Every `<npp:map>` record names in `views` the views of the pages that show
  it, in the order of the pages, as the target finds them from the parts each page shows and the maps each part
  holds: a course part's record names the pages of its course, and the base map's every page that shows it. A reader of
  the metadata alone follows the attribute to the pages, and CD-1 is answered where it asks, in the course part's own
  record (@Pauli, Nov 6).
- **Kinds.** Of a control, the kind as the .ppen file names it, by `ControlPoint.KindText`, which the file's writing
  uses too: normal, start, finish, crossing-point, map-exchange or map-issue. Of an object, from the `CourseObj`
  class: `ControlCourseObj` control, `StartCourseObj` start, `FinishCourseObj` finish, `ControlNumberCourseObj`
  control-number, `LegCourseObj`, `FlaggedLegCourseObj` and `TopologyLegCourseObj` leg, `DescriptionCourseObj`
  description, and each special its own kind; controls from `courseControlId`, and for a leg from both ends.
- **The format's version stays 1.** The course data's names stood under version 1 as a proposal until @Pauli settled
  them, and no release has written them: the export has reached `dev` and its trial packages alone. From the first
  release on, a reader of version 1 meets the settled names only.

**Settled by @Pauli on Nov 6: the pages that show a course.** Each map's record names in `views` the views of the
pages that show it, as built. A course opens in a browser as `file.svg#<view>`, through the view of its page (L-6):
Chromium shows `file.svg#Course_12`, opened by itself, as that course's page, and one file holds every course when the
export's file choice is all pages in one file.

## Baseline tests (V-2, V-4, V-5)

`CourseSvgBaselineTests` in `Map_Svg.Tests`, beside `CourseSvgExportTests`, so that the SVG loop runs them on Linux
(V-4): each exports a reference event's courses, compares each file's text with a baseline committed in
`src/TestFiles/svg_export`, and writes a missing or changed file to the loop's results, from where it is committed, as
the comparison does with its references. Each file is also exported a second time and compared byte for byte, and the
results record each file's size, plain and gzip-compressed, which the loop's summary reports (V-5). The renderings of
the comparison decide whether the drawing is right; the baselines show in review what changed in the text.

The set, from events whose maps load on a case-sensitive file system:

- `controller/mapexchange1.ppen`: courses with map exchanges, and no map, so its files are small; also its Course 5 in
  a file of its own, plain and compressed (S-10);
- `courseprinting/Overprint test.ppen`: an OCAD map with overprinted colors and the course's purple blended; also drawn
  in upper and lower purple, its map in two parts around the course's lower layer (L-2), a file per course and all
  pages in one file (L-7);
- `controller/Lincoln Park.ppen`: a bitmap map, which the files embed;
- `modes/printarea2.ppen`: a georeferenced OpenMapper map, `ShorelineCC_Dec2018.omap`;
- `controller/Madrona Permanent11.ppen`: an OCAD map with area patterns, multi-colored point symbols and text along
  lines; also with its area patterns drawn as separate shapes (S-7);
- `gpx/testproj2.ppen`: a georeferenced OCAD map, `testproj2.ocd`.

Several events name their maps with another case than the files have, such as `Marymoor.ocd` for `marymoor.ocd`,
and so load only on Windows; spelling them as the files are spelled belongs on `fix/case-of-test-file-names`.

## Phase 3's stages

All four were built on `feature/svg-export` and have been on `feature/svg-target` since d3561c1 (Oct 29).

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

Written on Nov 4 from `feature/svg-target` at 3c40d20, and built on `feature/svg-pages`. The dialog shows the three
file choices, all of them working.

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
  children of that element, of Inkscape's other namespace. Inkscape keeps its first page at the root's view box, as
  the document does.
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
  defines its two four times, 1,780 bytes more than once. S-12 is met, so phase 6 keeps symbol reuse and compression.
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

## Symbol reuse (S-5)

Written on Nov 6 from `feature/svg-target` at f8b60f9, and built on `feature/svg-symbol-reuse` (353887b). The change
is the target's alone, in `Map_Svg`.

- **What repeats.** A glyph draws its parts of one color under one `PushTransform`, which moves to the glyph's point and
  turns by its angle (`Glyph.DrawCore`): a point symbol's object, and the glyphs that a line draws along itself and an
  area drawn as separate shapes draws in its pattern. Each such draw is a group of the target carrying the transform
  and holding the parts' shapes, and the same shapes come again in every object of the symbol, or every glyph of a
  line, under transforms of their own. After the parts' groups are tidied (S-8), a group that carries a transform and
  no clip, style or blending, and holds nothing but shapes (paths, circles and ellipses) of one object, holds such a
  fragment. A group that also carries a symbol's identity, having taken on the transform of its only child, holds one
  too. A glyph of one circle at its point is drawn without a transform, as a circle in place, and stays as it is.
- **What is compared (S-5).** The fragment's text as written: each shape without its object number. When the shapes
  share one style, the text leaves the style out; otherwise each shape keeps its own, and such a fragment that paints
  with a pattern stays as drawn, since its text would name the pattern before the pattern's id is known.
- **Defined once.** A text found in two or more groups of the document's parts is defined once in `<defs>`, after the
  patterns and before the parts that use it: a single shape carries the id itself, as `<path id="c" d="..."/>`, and
  several are held in `<g id="c">`. A text found once stays where it is drawn, since its definition and reference would
  be longer than the group. Pattern tiles are not searched: they hold no objects, and are written as they are first
  used.
- **Placed by reference.** Each group of a repeated fragment gives way to `<use data-o="..." xlink:href="#c"
  transform="..."/>`: each object keeps an element of its own, which carries its number, and the transform is written
  as the group's was, with the digits the fragment's reach needs (S-2, S-8). A group that carries a symbol's identity
  keeps it and holds the use. The use carries the style its shapes share, as an element carries its style: written on
  the use unless a group around it carries it (S-4). The shapes inherit it through the use, so one definition serves
  every style its geometry is drawn in, and a symbol's or a run's shared style stays on its group.
- **Ids.** A definition's id is named when it is first used, as every id is (F-2, L-5).
- **Tests.** `SvgDocumentTests` define a glyph that three objects of two symbols draw once, the symbols' groups carrying
  the colours their uses share, and the group of a symbol of one object keeping its identity; keep the styles of shapes
  that differ in style in their definition; and leave glyphs drawn once as they are. The structure walk (V-7) takes the
  shapes of a definition's group as resources, as it takes a definition of one shape, and the layout test (V-8) tells
  the parts, which pages use, from the definitions that the parts use. `AreaPatternTests` count each dot of a pattern
  drawn element by element, a circle or a use. Seven export baselines (V-2) move by the references, and no rendering of
  the comparison or of the course pages moves: the 227 measurements of a local run are byte for byte those of loop run
  42223178438 on f8b60f9.

The 23 files of the baseline tests come to 6,426,906 bytes, against 6,485,149 at f8b60f9, and to 3,207,077
gzip-compressed, against 3,208,769. mapexchange1's page of all controls falls 12.6 %, Madrona Permanent11's Course 1
2.7 %, mapexchange1's Course 5 in a file 2.6 %, printarea2's Course 1 2.5 % and testproj2's page of all controls 2.3 %.
Madrona Permanent11 drawn as separate shapes falls only 0.6 %: its 20,732 dots are circles in place, which a use would
lengthen. Overprint test lower purple in one file grows by 12 bytes: each of its two course parts draws the start and
the finish once, and a definition of a small fragment with two uses is a few bytes longer than the two groups. The
212 SVGs of the comparison come to 29,499,213 bytes, against 30,091,506, and to 11,589,732 gzip-compressed, against
11,597,812: wholestructure2 and 4 fall 70 %, max2glyph7 and 9 53 %, decreasesymbols 29 %, and five small ones grow by 2
to 12 bytes. Compression finds the repeats by itself, so S-5 matters for the plain document.

**Inkscape and Scribus (F-6).** Read with `SvgColourReaders/` on the exports of the new code:

- Scribus 1.6.1 turns a `<use>` into a group holding a copy of what it refers to, with the use's style and transform.
  Of Overprint test lower purple in one file, mapexchange1's Course 5 in a file and its page of all controls, Madrona
  Permanent11's Course 1 and printarea2's Course 1, which hold 4, 6, 77, 256 and 701 uses, it made 32, 92, 869, 1,745
  and 8,095 items, each use's copy among them, with the fills and strokes that `expected_paints.py` counts, within Qt's
  steps of 1/255, and printarea2's 18 elements of its patterns' tiles besides. The copies take the stroke widths of
  the groups around their uses: every item of Course 5 is stroked 0.992 pt, the 0.35 mm of its groups.
- Inkscape 1.2.2 shows a use as a clone. It showed the profile `cmyk` and C, M, Y and K, the values written, on a
  symbol's group that carries its uses' style and on a use that carries its own (0, 0.56, 1 and 0.18 for printarea2's
  brown), and as unset a use whose group carries its style and a definition's shape, as it shows any element whose
  style a group carries.

**Settled by @Pauli on Nov 12: a pattern for each symbol and colour.** Each course part draws its course with symbols
of its own, so a file of several parts defines the patterns and hatchings of its course once a part: mapexchange1's
Course 5 in one file defines its two hatchings four times, 1,780 bytes more than once. The proposal was to define equal
patterns once a file, found as S-5 finds repeated glyphs, so that equal patterns of different symbols would share a
definition too. @Pauli kept a definition for each symbol and colour, as S-12 asks, and the repeats with it.

## Compressed files (S-10)

Written on Nov 6 from `feature/svg-target` with S-5 (353887b); the work is on `feature/svg-svgz`.

- **What is there.** Since phase 3, `CourseSvgSettings.Compress` (A-2), off by default, and the dialog's check box
  "Compress the files (.svgz)" in its Appearance group have the export write each file gzip-compressed, named `.svgz`,
  through `GZipStream` at `CompressionLevel.Optimal`.
- **What differs between platforms.** A gzip member's header (RFC 1952) holds a modification time and the operating
  system that wrote it. .NET writes the time as 0 and no file name, but takes the system from the zlib it is built
  with: 3, Unix, on Linux, and in zlib's own codes 10 on Windows and 7 or 19 on macOS. So the same document came out a
  byte apart on each platform, against F-2.
- **The file.** The header names the operating system 255, unknown, wherever the file is written, and nothing else
  changes: deflate, no flags and so no name, comment or extra field, the time 0, no extra flags at this level, then the
  document's UTF-8 bytes deflated, their CRC-32 and their length. Any reader of `.svgz` takes it as before.
- **Tests (V-4).** `CourseSvgBaselineTests` export mapexchange1's Course 5 into a file of its own, plain and twice
  compressed. Each compressed file is named as the plain one with `.svgz`, begins with the header `1f 8b 08 00 00 00 00
  00 00 ff`, decompresses to the plain file's bytes, comes out the same both times, and its SHA-256 is its baseline's.
  The Windows job runs the baseline tests too, so the deflated bytes, which .NET's own zlib-ng makes on every platform,
  are checked to be the same there as on Linux. The view model's tests take the choice through the settings both ways.

## Bitmaps drawn lighter (B-6)

The target draws a bitmap at an intensity below 1 as it draws any bitmap, through a `<use>` of the image that holds
its data once (B-5), and the use's style draws it through a filter: `filter:url(#…)`, after its `image-rendering`
where it is pixelated (B-2). The document defines one filter for each intensity, among the definitions after the
clips, named when it is first used, as patterns are (F-2):

```xml
<filter id="b" style="color-interpolation-filters:sRGB">
<feComponentTransfer>
<feFuncR type="linear" slope=".4" intercept=".6"/>
<feFuncG type="linear" slope=".4" intercept=".6"/>
<feFuncB type="linear" slope=".4" intercept=".6"/>
</feComponentTransfer>
</filter>
```

- **The transfer.** The Skia target lightens a bitmap with a lighting color filter that multiplies each channel by the
  intensity and adds 1 less it, on the encoded sRGB values; alpha stays. `feComponentTransfer` does the same on
  unpremultiplied values, once `color-interpolation-filters` is sRGB, since a filter works in linear RGB by default.
  The slope and intercept are written with the fewest decimals that give the single-precision values back, the
  intercept computed in single precision, as the map's own values are written (COL-4), so the TypeScript port writes
  the same bytes (P-6).
- **Shared by groups.** The filter is part of the use's style, so the uses of images drawn at one intensity share it
  through their group as any style is shared (S-4). Lightening is the same per-pixel affine map of every color
  channel with alpha kept, which commutes with drawing one image over another, so a group drawn through the filter
  looks as its images drawn through it one by one.
- **Readers.** Chromium and Inkscape draw the image lighter. Scribus may show it at full strength (@Pauli, Nov 7).
- **The loop.** template_light and template2_light, which draw their templates at 0.4, are compared like any case.

## The geometry check (V-10)

Written on Nov 7 from `feature/svg-target` at 4764b57, with the loop's later improvements (b67748d); the work is on
`feature/svg-loop-improvements`. Chromium's pixels show no error smaller than a pixel, and a case's image holds a few
pixels to a millimetre, while the document writes its coordinates to a thousandth of one (S-1). The rendering
comparison therefore also reads each document back into page coordinates and compares its geometry with the case's
drawing calls. The check is the tests' alone, in `Map_Svg.Tests`.

- **What the calls draw.** `CallGeometryRecorder` is a graphics target into which the case's recording is played back,
  as it is into the SVG target, and keeps for each call that draws a description in page coordinates: a path by its
  parts as Skia takes them, a line or curve with no start before it beginning at the origin; a line, polygon or
  rectangle as such a path; an arc by its ends, its radius, and whether it is the larger arc and turns towards positive
  angles on the page, a full turn or more as a circle; an ellipse by its center and the ends of its axes; a text by its
  characters as a document holds them, the start of its baseline below its upper left corner by the face's ascent, and
  the point an em along it; a bitmap by the corners of its rectangle; a stroke by its width; and a pattern brush's
  paint by its lattice, with its tile described likewise in steps of the tile, whose middle is its origin (D-7). Each
  carries the clips around it. A call that draws nothing, a path with no point or a text with no character a document
  can hold, is left out, as the target leaves it out.
- **What the document draws.** `SvgGeometryReader` reads the first page of the document as SVG has it: each group's
  transform, clip and style, the uses of parts, of repeated shapes (S-5) and of images, a line text (T-5) character by
  character, and the fill, stroke, stroke width, fill rule and font size that a drawing inherits from its groups (S-4),
  into descriptions of the same form.
- **Matching.** The target writes an element for each call that draws, in the order of the calls (M-1), so the n-th
  drawing of the page is compared with the n-th call: their kinds, the rule they fill by, whether they paint with a
  pattern, the commands of their paths with an arc's flags and their characters must agree, and then each point and
  length must lie within its allowance of the call's, each clip around them must agree in the same way, and so must
  the tile of a pattern they paint with. Each clip and each tile is compared once. After the first drawing whose kind
  or commands differ, those after it are not compared, since one drawing left out or added sets every one after it
  apart.
- **Allowances.** The document writes the numbers of a frame in whole units of it, the steps of the calls' own
  coordinates there, and a text the positions of its characters, in its own millimetres, to a step of the frame it lies
  in. A point lies within half a unit on each axis of where the call puts it, √½ of a unit; a point rounded twice, a
  character of a line text or an ellipse moved into place (S-8), within √2; and a point a rounded radius or em from
  another, half a unit more. Each transform the document writes moves a point within its reach less than a unit of the
  frame it writes into (S-2), and that unit, on the page, adds to the allowance of every point below it; the page's
  transform adds a step of the map. A stroke's width may lie half a unit from the call's, and an arc's radius a unit,
  as SVG enlarges a radius too short for its arc's ends. Each allowance is measured on the page.
- **Results.** Each case's measurements go to `geometry/<case>.tsv` in the loop's results: the drawings compared, the
  largest distance of a point or length from the call's, in steps of the map, and the largest share of its allowance
  any takes. The loop's summary gives the largest of each across the cases, and its changes since the previous run
  take them in with a drawing's other measurements. A case whose geometry differs fails, with the first differences in
  its message.
- **Tests.** `GeometryCheckTests` draws a line, an arc, an ellipse and a curve under a turn, a polygon within a clip, a
  rectangle filled with a pattern, a text and a bitmap, and requires the document of the drawing to agree with its
  calls, and the calls to be told apart from it when a point of any of them lies five steps away, the pen is a step
  wider, the arc turns the other way, the polygon is left out or the text's characters differ.

Over the 190 cases that SVG loop run 42321275386 compares, the check reads 140,067 drawings back. No point or length
lies further from the call's than 1.362 steps (simpletext, a text's em, whose size is written to a thousandth of a
millimetre), and none takes more than 0.662 of its allowance (lksamm4 and its kin). No SVG changed. Where a template's frame shrinks the map, its allowances
shrink with its units: ocad11templatealign takes 0.193 of them and template2 0.262.

The check covers the rendering comparison's cases. Where Chromium puts each character of a text is checked apart,
against Skia's shaping, and the pages the export writes are compared by their images alone (`CourseSvgPageTests`,
`CourseSvgLayoutTests`).

## Firefox and WebKit (F-5, V-6)

Written on Nov 11 from `feature/svg-target` at cc462aa; the work is on `feature/svg-browsers`. The browser check
renders documents in Firefox and WebKit and compares each image with Chromium's image of the same document. It is the
tests' alone, in `Map_Svg.Tests` (`BrowserComparisonTests`).

- **The documents.** Every document the rendering comparison writes, at its case's size: the drawing, map-area and
  area-pattern cases of V-3, clouds and clouds11 among them, whose documents leave out the lines of zero width their
  references draw (D-5) but are drawn like any other. Then the six exported pages that `CourseSvgPageTests` draws, at
  150 pixels to the inch, which carry what only the application's export writes: course layers, descriptions, the
  metadata and its tables. And the four pages of one file, mapexchange1's Course 5 as the baseline tests export it,
  each shown through its view (L-6). The rendering comparison runs first, and hands the check each document it wrote
  and Chromium's image of it; a document the check does not get so, as when it runs alone, it writes and renders as
  its own test does, so that it needs no other test to have run.
- **The browsers.** Those the tests' Playwright package pins, as V-3 pins Chromium: with Microsoft.Playwright 1.63.0,
  Chromium 153.0.8010.12 (the headless shell), Firefox 155.0 and WebKit 26.6, Playwright's builds for Ubuntu 24.04.
  Its WebKit on Linux is the WPE port, drawing with Skia, not Safari's build.
- **How they are started.** As Chromium is: a page that shows the SVG as an image of the case's size, at one device
  pixel to each CSS pixel, and a fontconfig file that offers the sentinel font alone. The file also turns hinting off:
  Firefox takes its hinting from fontconfig, where Chromium takes it from `--font-render-hinting=none`, and hinted
  glyphs move by up to a pixel. Chromium draws every case byte for byte the same with it. WebKit takes its hinting from
  neither, nor from `text-rendering`, nor from MiniBrowser's settings file, which it does not read under Playwright: it
  hints the faces a document embeds. Firefox and WebKit draw the page for two frames before its screenshot is taken:
  WebKit draws a pattern of a few pixels a tile otherwise when the screenshot comes before its first frames, and not
  alike from one rendering to the next, which after two frames it does. Chromium is shot as soon as it has loaded the
  page, as the rendering comparison has always shot it, and draws alike either way. A screenshot that a browser says it
  is unable to capture, as Firefox and Chromium now and then do while many pages are open, is taken again on a page of
  its own, up to three times.
- **The comparison.** Each browser's image is held to Chromium's by F-1's rule, `PixelTolerance.Default`, or
  `PixelTolerance.Text` for a document that draws text: F-5 asks of two rasterizers drawing one document what F-1 asks
  of Chromium and Skia drawing one case. With `SvgTolerance/`'s planted errors, the text cases now held to 80 levels as
  the tests hold them, the rule fails no real rendering of the 205 documents but those set apart, and fails all 205
  shifted by two pixels or with features two pixels wider, 178 of the 194 with a solid feature removed in Firefox and
  180 in WebKit, and 165 of the 204 with their ink six levels redder in each: about as many in proportion as it
  catches in Chromium's images against the references, 164 of 182 removals and 161 of 192 recolorings. It is not
  loosened for any document.
- **Set apart.** A document that a browser draws otherwise because of the browser itself, where no form within the
  requirements draws alike in all three without costing the file data or Chromium's drawing (@Pauli, Nov 11), is set
  apart for that browser, with its reason and the number of pixels outside the tolerance it had, in a table of
  `BrowserComparisonTests`. It is still rendered and compared: while it differs by no more pixels, its test ends as
  ignored, with the reason, which goes to `browsers/apart/<browser>/<document>.txt`; once it matches, or differs by
  more, its test fails, so that the table holds only what still differs and nothing new hides behind it.
- **Where it runs.** In the SVG loop, on Linux, which installs Firefox and WebKit beside Chromium, with their system
  libraries. The Windows suite runs only `CourseSvgBaselineTests` of the assembly and installs no browser, so it
  neither changes nor slows.
- **The report.** The loop's results get `browsers/<browser>/`: each comparison's measurements as `measurements/` has
  them, and for a document outside the tolerance, set apart or not, the browser's image and the difference image, its
  pixels outside the tolerance in red over Chromium's image ghosted; and `browsers/apart/`. The summary's section
  "Firefox and WebKit" gives, for each browser, the documents compared, those that match Chromium's image, those set
  apart and those that fail, and each document set apart with its pixels outside the tolerance and why; the title counts
  the documents set apart, and "Since the previous run" names a change of a browser's measurements as
  `<browser>/<document>`. The artifact of renderings keeps every browser's image under `browsers/<browser>/`, and
  Chromium's of a document only the check rendered under `browsers/chromium/`.

What the browsers drew otherwise than Chromium, in the 205 documents, and what became of each:

| Browser | Documents | Why | What became of it |
| --- | --- | --- | --- |
| Firefox | 56 cases with text | Firefox hinted the glyphs, by fontconfig's defaults | Hinting turned off in the fontconfig file, as Chromium's flag turns it off |
| Firefox | fontfallback | A list of positions counts a character outside the Basic Multilingual Plane as two characters in Firefox, as SVG 2's addressable characters are UTF-16 code units, and as one in Chromium and WebKit, so every character after one stood a place further on | The target ends a list of positions at such a character, as at a cluster of several characters, and keeps such a character along a line out of a line text (T-5); Chromium draws every case as before |
| Firefox | fontfallback | Firefox draws the lower part of a Bengali cluster a pixel lower: one pixel outside the tolerance | Set apart |
| Firefox | teanwest, teanwest11 | Patterns whose tiles are a pixel or two across: Chromium shades them evenly and Firefox a few levels lighter, each within F-1 of the reference | Set apart |
| WebKit | teanwest, teanwest11, ocad11templatealign | Patterns whose tiles are a few pixels across: WebKit shades them darker and with a grain, and before its first frames otherwise again | Two frames drawn before the screenshot, which makes WebKit draw alike every time; set apart |
| WebKit | fontfallback | WebKit draws nothing of a text whose characters are all Runic, in any face, in SVG alone | Set apart |
| WebKit | Bitmap, page-LincolnPark-Short | WebKit scales a bitmap down without smoothing it, whatever `image-rendering` says | Set apart |
| WebKit | frametext1, frametext3, hiddensymbols, hiddensymbols9, PenistoneHill, page-printarea2-Course1, and fontfallback, teanwest and teanwest11 | WebKit hints the embedded faces, which nothing reachable turns off | Set apart |

Firefox matches Chromium in 202 of the 205 documents and WebKit in 193, SVG loop run 43064709418 among them; every
page shown through its view matches in both. The check takes the loop's test step from about 5 minutes to about 14, and
the browsers' install from 16 seconds to about 2 minutes.

## A PDF base map as vectors

Written on Nov 12 from `feature/svg-target` at 267efc8, for the task of the handover's plan (`SvgTargetProgress.md`,
Tasks, Nov 12). The requirements leave copying a PDF base map as vectors out of scope; this section finds out whether
the SVG could carry the page's vectors instead of the bitmap it carries now, and at what cost, and ends with the
choice put to @Pauli. Nothing of it is built. The probes are in [`SvgPdfVectors/`](SvgPdfVectors/README.md), whose
`run-all.sh` repeats every figure below on Linux.

### How the application handles a PDF map today

- **Loading.** `MapDisplay.SetMapFile(MapType.PDF, …)` has `CoreMapUtil.ValidatePdf` convert the PDF into a bitmap
  through `PdfMapFile`, which keeps the result as a PNG in `PurplePen` under the temporary folder, named after a SHA-1
  of the PDF's bytes, for 180 days. The conversion is `Services.PdfConverter`, in AvPurplePen `PdfConverterService`,
  which calls `PdfConversion.Convert` of the `PdfConverter` project: PDFium, through PDFiumCore 157.0.8100, renders
  page 0 alone at 600 dots per inch, at the size `FPDF_GetPageSizeByIndex` gives, the crop box's, onto white, with no
  render flags, so without annotations, into an 8-bit RGB PNG that records 600 dots per inch.
- **Drawing.** From then on the map is a bitmap map. `MapBounds` is the bitmap's pixels at 600 dots per inch, its
  origin at the page's lower left corner, and `DrawBitmapMap` draws the whole bitmap under `BitmapTransform`, smoothed
  (`MediumQuality`) when anti-aliased, and by nearest neighbour when not, or when the bitmap has more than 50 million
  pixels, as an A2 page has at 600 dots per inch (139 million). `HasLayersAboveCourse` holds only for an OCAD map, so
  the course is drawn above the whole of a PDF map.
- **PDF export.** `CoursePdf` does not draw the bitmap. When PdfSharp can import the page (`CanReadPdfPage`), each page
  of the output starts as the PDF's page 0: copied whole by `BeginCopiedPage` when the course is at the map's scale
  and its print area is the whole page, and otherwise drawn by `BeginCopiedPartialPage` as a form XObject (`XPdfForm`)
  under a clip to the print rectangle and a transform from the map rectangle, measured from the crop box's corner at
  72 points to the inch, onto it. The map display's map is then set to none, and the course is drawn over the copy.
  The page's content is never interpreted: its colour spaces, spot colours, overprinting, transparency, fonts and
  images reach the output byte for byte. A page PdfSharp cannot import falls back to the bitmap.
- **SVG export.** `CourseSvg` draws the map's part through `MapDisplay.Draw(MapBelowCourse)`, which for a PDF map is
  `DrawBitmapMap`: one `DrawBitmap` of the whole page's bitmap, whatever area the pages show, under the map's label
  and outside any colour run (M-5). The target writes it once a file (B-5), through the bitmap's `WriteToStream`, as
  PNG, the bitmap's own format (B-3), which ImageSharp's encoder writes with an alpha channel that the converter's PNG
  did not have, and embeds it as base64 (B-4). Above 50 million pixels the use is pixelated (B-2), as the application
  draws it.

### The test maps

The maps of the test events that name a PDF, and the PrimoPDF map of the map tests (`inventory.py`). None has spot
colours, shadings, soft masks, blend modes other than normal, dashes or annotations; Purple Pen's own exports draw
their course's purple with darken blending, which these maps of theirs do not have.

| PDF | Events | Made by | Page | What it holds |
| --- | --- | --- | --- | --- |
| `pdfcourse/ST_Pauls_school_bald_hills_4000 Gordon v10.5.pdf` | St Pauls Week 4 | OCAD 10.5 | A4 | 5,544 paths in DeviceCMYK; overprinting (`OP`, `OPM` 1) in 71 of its graphics states; 56 fills with tiling patterns, OCAD's screens, 8 mm cells of 361 dots turned 45°, whose resources repeat one key 38 times; 82 transparency groups, one for each colour of OCAD's table, each named in marked content (`/Layer` with its `/Title`); no fonts |
| `bitmapcreate/St. Edward.pdf` | StEd, Bad PDF Test | OCAD 9.0 | 258 × 188 mm | 7,929 paths in DeviceCMYK; overprinting; 62 tiling-pattern fills; 83 layers as above; no fonts |
| `pdfcourse/UWLetter.pdf` | UWLetter | OpenOrienteering Mapper, through Qt 5.12 | letter | 15,878 paths in DeviceRGB; no fonts |
| `pdfcourse/tengesdalslia.pdf` | Tengle | a scanner, through macOS Quartz | 384 × 243 mm, a crop box within an A3 media box | one JPEG of 4953 × 3497 pixels, 3.6 MB |
| `pdfcourse/Lincoln-CMYK.pdf` | Lincoln Park PDF, in four folders | Purple Pen's PDF export | letter | 75,094 paths in DeviceCMYK; 177 texts in four embedded TrueType subsets of Arial |
| `pdfcourse/Lincoln-RGB.pdf` | Lincoln Park PDF RGB | Purple Pen's PDF export | letter | the same in DeviceRGB |
| `pdfcourse/CleElumRidge.pdf` | PDF rescale test | Purple Pen's PDF export | A2 | 194,135 paths in DeviceCMYK; 340 texts in two Arial subsets |
| `pdfmaps/Potholes.pdf` | the map tests | PrimoPDF | letter | 12,056 paths in DeviceCMYK; overprinting; 50 strokes of zero width |

OCAD and OpenOrienteering Mapper write a map's text as outlines: only Purple Pen's own PDFs carry fonts. The OCAD 10.5
map is also malformed: the dictionary of its patterns repeats one key for each of 38 copies of a pattern, which qpdf
drops and PdfPig and PDFium read, and MuPDF and pdf.js find 56 of its streams cut short. A reader of map PDFs has to be
lenient.

### Two ways, and where the reading can live (C-2, C-3)

- **A converter writes the page as SVG**, and the export takes its document into the map's part. The target then
  holds content that none of its calls made: a new call of `IStructuredGraphicsTarget`, one that places an SVG
  fragment under a transform, would have to rename the fragment's ids to keep them unique (L-5), and could neither
  write its numbers in the target's steps (S-1 to S-3), nor give its colours their CMYK values (COL-1), nor be read
  back by the geometry check (V-10). C-2 and C-3 allow such a call, as the fragment is a plain string; the converter,
  a program of its own, runs in the application.
- **The application reads the page into drawing calls**, and the target draws them as it draws a map: paths filled and
  stroked with CMYK brushes and pens, clips, pattern brushes and bitmaps, all of them calls that the target fully
  implements today (DL-5). The target references `Graphics2D` alone (C-2) and takes data only through its interfaces
  and plain values (C-3), so the reader cannot live in it: it lives in the application, in `PurplePenCore` beside
  `MapDisplay` or in a project of its own that references `Graphics2D` and the reading library, as the map code lives
  beside the targets. `MapDisplay` would call it for a PDF map in place of `DrawBitmapMap`, for a target that asks for
  vectors, as `PatternBrushesAreVectors` asks for pattern fills (S-7); every other target could draw a PDF map so too.
  What C-3 does not carry today: a colour that is not CMYK (a `CmykColor` holds CMYK alone, and `FromRgb` invents
  CMYK values that COL-1 would then write), a font program of the PDF's own (`IFontSubsetProvider` gives the faces of
  a family the application can open), and an image's original JPEG data (`WriteToStream` encodes it anew).

### The candidates

| Candidate | Way | Licence | Purple Pen and the fork could ship it | Windows, Linux, macOS | Browser |
| --- | --- | --- | --- | --- | --- |
| Poppler 24.02 with Cairo 1.18, `pdftocairo -svg` | Converter | Poppler GPL-2 or GPL-3, Cairo LGPL-2.1 or MPL-1.1 | Not linked into the BSD application; only as a program beside it, under the GPL's terms for that program, with its source | Built for each by package managers, none of them NuGet | No official build |
| MuPDF 1.23 (`mutool convert -F svg`), or its device interface | Converter, or reader | AGPL-3.0-or-later, or Artifex's commercial licence | Not linked without the commercial licence; as a program beside it under the AGPL's terms | Yes | MuPDF.js, npm `mupdf` 1.28, AGPL |
| PDFium's page objects, through PDFiumCore 157.0.8100 | Reader | BSD-3-Clause; the bindings and the binaries' packages Apache-2.0 | Yes: the application ships them already, for PdfConverter | Yes | `bblanchon.PDFium.WebAssembly` on NuGet |
| PdfPig 0.1.16 | Reader | Apache-2.0 | Yes, with its notice | Yes: managed code alone, .NET Standard 2.0 and .NET 8 | Only within a .NET application compiled to WebAssembly; the TypeScript port would translate it |
| pdf.js 6.5 (`pdfjs-dist`) | Reader, for the TypeScript port | Apache-2.0 | The .NET application has no use for it | Node | Yes: it is Firefox's PDF viewer |

Each was run on the test maps: the two converters as programs (`converters.sh`), PDFium's page-object API through
PDFiumCore (`probe pdfium-objects`), PdfPig through a reader written for the probe, which draws into the SVG target
(`probe vector-svg`, described in the README), and pdf.js through its operator list (`pdfjs-ops.mjs`).

- **The converters** write every colour as sRGB, each converted its own way: MuPDF's DeviceCMYK by PDF's own formula,
  red 1 − min(1, C + K) and so on (ISO 32000-1, 10.3.5), which turns the green of (70 %, 0, 100 %, 10 %) into (51,
  230, 0), and Poppler's otherwise again, into (70, 172, 50). Neither writes overprinting. Both write text as the
  outlines of its glyphs, each glyph defined once and placed by `<use>`, keep a JPEG's data as it is, and draw a
  tiling pattern as an SVG pattern, Cairo one for each fill. Cairo writes numbers in points with six decimals, and a
  clip path of its own around each drawing.
- **PDFium's page objects** give each object's fill and stroke as 8-bit RGBA after PDFium's own conversion, the one
  the application's bitmap shows; no colour space, spot colour, overprinting, blend mode (it has a setter and no
  getter) or soft mask; and a tiling pattern only as a bitmap of it (`FPDFPageObj_GetRenderedFillPattern`,
  experimental). It does give paths, clip paths, glyph outlines, font programs, images with their filters, and marked
  content, OCAD's layer names among it.
- **PdfPig** gives the content stream's operators with their graphics state: DeviceCMYK, gray and RGB colours as
  given, a separation's name and alternate, overprinting and its mode, blend mode, constant opacity and soft mask,
  the CTM and the clipping operators, glyph outlines of TrueType, Type 1 and CFF fonts, and images, a JPEG's data as it
  is. It reads; drawing is the reader's. Its 0.1.16 sets non-stroking overprinting only from `op`, where an absent `op`
  takes `OP`'s value (ISO 32000-1, table 58): the OCAD maps' fills would lose theirs, which the probe's reader
  corrects. Its reading part is about 89,000 lines of C#.
- **pdf.js** gives an operator list whose colours are already sRGB, `#rrggbb` strings that its evaluator converted
  from DeviceCMYK. Its graphics states keep opacity and blend mode, as the canvas names them (`darken`,
  `source-over`), and drop overprinting. It turns each embedded font into an OpenType font that a browser takes, 11 to
  71 KB for the Arial subsets of Lincoln-CMYK, and reads tiling patterns as patterns.

### What each keeps of what the SVG has to keep

| What | The bitmap, today | A converter | A reader on PDFium's page objects | A reader on PdfPig, as probed | A reader on pdf.js, for the port |
| --- | --- | --- | --- | --- | --- |
| CMYK values in `icc-color()` (COL-1) | No: pixels | No: sRGB alone | No: 8-bit RGB | Yes, for DeviceCMYK, which OCAD's, PrimoPDF's and Purple Pen's own PDFs use | No: sRGB alone |
| RGB colours, as OpenOrienteering Mapper and Purple Pen's RGB export write them | As pixels | Yes, as sRGB | Yes, in 8 bits | Only as CMYK that `CmykColor.FromRgb` invents, which COL-1 writes and the CMYK model converts back otherwise; kept only by a paint for RGB colours in `Graphics2D` | Yes, as sRGB |
| Spot colours | No | No | No | The name and alternate are read; the target has nowhere to write the name (none of the test maps has one) | No |
| Overprinting (COL-7) | No: PDFium's bitmap does not show it | No | No | Yes: read, and drawn with darken blending (D-8) if chosen | No: dropped |
| Opacity and blend modes | As pixels | As SVG opacity and blending | Opacity alone | A constant opacity as the colour's alpha (COL-5); darken alone among the blend modes (D-8); soft masks and a group's own opacity not drawn (none in the test maps) | Yes |
| Clipping | As pixels | Yes | Yes | Yes, as clip paths (D-2) | Yes |
| Tiling patterns | As pixels | Yes | As bitmaps alone | Yes, as pattern brushes (D-7), one for each distinct cell: 6 for St Pauls's 56 fills | Yes |
| Text (T-6, T-7) | As pixels | Outlines | Outlines, or the font programs | Outlines, as filled paths; the font programs are readable, but embedding them needs a converter to web fonts and a way past C-3 | Fonts converted to OpenType, which a browser takes |
| Images (B-3) | The page is one image | Kept, a JPEG as its data | Raw data and filters | Kept, but `WriteToStream` encodes a JPEG anew at the export's quality: the scan's 3.6 MB became 4.9 MB of JPEG | Decoded |
| A place in the map's structure (M-5) | Under the map's label, outside any run | A fragment under the map's label | Under the map's label, outside any run | Under the map's label, outside any run; OCAD's layers, which name the colours of its table, could become colour runs (M-3) | As the port draws it |

### What it costs

**Size.** Each way's map part, the whole page, plain and gzip -9 compressed; the bitmap's is what the export writes
today:

SIZE_TABLE

For the maps OCAD, OpenOrienteering Mapper and PrimoPDF wrote, the vectors are 5 to 6 times smaller than the bitmap
plain and 18 to 25 times compressed; for Purple Pen's own dense exports about as large plain and 12 times smaller
compressed; for the scan 15 times smaller, as the bitmap holds the page at 600 dots per inch where the vectors hold its
JPEG. The converters write 1.5 to 7 times the vectors' bytes for the vector maps, and Cairo 1.86 GB for the A2 map,
with a clip path of its own, 184,589 in all, around each drawing. The application's own export writes the bitmap into
each file, as each file draws the map once: Lincoln Park PDF's 5 files are 9.25 MB each, 46.2 MB in all, that of a
course whose print area is 75 × 46 mm of the letter page as large as the others, and Tengle's 3 files 95.5 MB each.

**Time.** Seconds to write the map's part, on this machine:

TIME_TABLE

The bitmap's conversion is done once a map is loaded and cached; the export then writes the bitmap anew into each
file: Lincoln Park PDF's 5 files take 12.5 s, Tengle's 3 take 68 s. The vector way reads and draws the page for each
file, 1 to 6 s, as the export now draws an OCAD map for each.

**Determinism (F-2, P-6).** Every way wrote the same bytes twice, and the converters the same bytes in another locale,
time zone and folder. The vector way kept its bytes in another locale and time zone, and its paths with the JIT
without tiering and without hardware intrinsics (`determinism.sh`); the scan's document did not without hardware
intrinsics, as ImageSharp's JPEG encoder, which re-encodes
its image (B-3), wrote other bytes, while everything outside the image's data stayed the same. Whether x64 and ARM64
encode a JPEG alike is so not settled; the export's baseline events draw a GIF map, and V-4 does not encode a JPEG.
Passing a JPEG through as its own data would take the encoder out of the bytes. For the TypeScript port (P-6) the
reader has to be the same, which pdf.js is not: the porter translates PdfPig with the map model, about 89,000 lines,
or the comparison replays the .NET reader's drawing calls as P-5 replays metrics, which `RecordingGraphicsTarget`
records already; the port's own reader for its users would then be pdf.js, without CMYK and overprinting. A converter
can be in the port only as MuPDF.js, under the AGPL.

**Against the application's drawing (F-1, F-5).** Each SVG rendered at 150 pixels to the inch in Chromium, Firefox
and WebKit, as the tests render, and held to the application's own drawing of the page by F-1's rule
(`PixelTolerance.Default`): `MapDisplay` drawing PDFium's bitmap with the Skia target. The share of pixels outside the
tolerance, in Chromium, Firefox and WebKit:

TOLERANCE_TABLE

- **The bitmap** matches in Chromium and Firefox, as it is the application's own drawing. WebKit scales a bitmap down
  without smoothing, as for every bitmap (see "Firefox and WebKit"), which fails half the scan's photograph. Chromium's
  headless shell crashes on the A2 map's bitmap of 139 million pixels.
- **The vectors** differ for two reasons, which the variants tell apart. With each CMYK colour converted as PDFium
  converts it, `--pdfium-colors`, they lie within the tolerance but for at most 0.16 % of the pixels, along fine
  hatching that two rasterizers draw, and Lincoln-CMYK matches entirely in all three browsers: the drawing is the
  same. The rest is the conversion: COL-2 converts CMYK through `SwopColorConverter`, as for an OCAD map and the
  course, where PDFium converts it its own way, black (0, 0, 0, 100 %) as `#000` against (35, 31, 32), the green of
  (70 %, 0, 100 %, 10 %) as (72, 168, 66) against (67, 168, 66). An RGB map drawn through `CmykColor.FromRgb` and the
  CMYK model fails across a fifth to two fifths of the page; in the RGB model, which gives its colours back as they
  were, as a paint for RGB colours would, UWLetter and Lincoln-RGB match but for 1 and 5 pixels.
- **Overprinting drawn darker** changes Chromium's and Firefox's images by at most a few hundred pixels, as darkening a
  dark colour over a lighter one changes little on these maps; WebKit, which draws the thousands of blended groups
  otherwise, departs up to 0.75 % of the pixels.
- **The converters** fail across 4 to 40 % of the CMYK maps' pixels, by their colours, and match the RGB maps.

### Proposal

The choice is @Pauli's; nothing is built before it.

- **(a) Keep the bitmap.** The export stays as it is, and copying a PDF map as vectors stays out of scope. The SVG
  shows the page as the application draws it, as pixels without CMYK, written whole into every file at 600 dots per
  inch, 9 to 31 MB for the vector maps and 95.5 MB for the scan.
- **(b) Keep the bitmap, written smaller.** As (a), but each file writes only the part of the bitmap that its pages
  show (`DrawBitmapPart`), as RGB without the alpha channel ImageSharp adds. No requirement changes; a file of a small
  print area shrinks with it, and the rest by about a tenth; every other cost of (a) stays.
- **(c) A converter writes the page as SVG.** Not recommended: both lose the CMYK values (COL-1) and overprinting, put
  4 to 40 % of a CMYK map's pixels outside F-1 by their colours, run as programs under the GPL or the AGPL beside the
  BSD application, and bring into the document content that none of the target's rules governs; Cairo wrote 1.86 GB
  for the A2 map.
- **(d) The application reads the page with PDFium's page objects**, which it ships already. Not recommended: 8-bit
  RGB alone, no overprinting or blend mode, and tiling patterns as bitmaps, so OCAD's screens would be pixels again.
- **(e) The application reads the page with PdfPig** into drawing calls, which the target draws as it draws a map,
  and which every target could draw. It keeps a CMYK map's values, its overprinting, clips and patterns, writes far
  smaller files, and draws the page as the application does. It takes, beside the reader: a requirement of its own in
  place of the line that leaves it out of scope; a paint for RGB colours in `Graphics2D`; a JPEG passed through as its
  data; loop cases of PDF maps; and the bitmap as the fallback for a page the reader cannot draw or read, as PDF export
  falls back to it for a page PdfSharp cannot import.

Within (e), @Pauli would also settle:

1. **Overprinting:** drawn darker as the map display's overprint effect says, as an OCAD map's overprinted colours are
   (`OcadOverprintEffect`); or never, as the application's own drawing of a PDF map shows it.
2. **The colours' sRGB:** COL-2's, through `SwopColorConverter`, as for an OCAD map and the course; or PDFium's, as the
   application shows the PDF on screen. The loop then holds the SVG to the reader's calls drawn by the Skia target,
   as every map case is held, and the comparison with PDFium's bitmap above checks the reader.
3. **OCAD's layers:** the page under the map's label outside any run (M-5), as a bitmap map is now; or each layer as a
   colour run (M-3) named as OCAD names the colour, its CMYK taken from what the layer paints.
4. **The default:** vectors for every PDF map, the bitmap where the reader cannot draw a page; or vectors as a setting
   named for what it does, "Draw a PDF map as vectors" (A-2), the bitmap the default.

**Recommendation: (e)**, with overprinting as the overprint effect says, COL-2's colours, OCAD's layers outside any run
for now, and vectors as the default. The reasons:

- It is the only way that keeps what the SVG exists for (Purpose and scope): a map in its own right, whose colours
  carry their CMYK values for Inkscape and Scribus (COL-1, F-6) and whose overprinting travels with them (COL-7), in
  files 5 to 25 times smaller than the bitmap's for the maps OCAD, OpenOrienteering Mapper and PrimoPDF make.
- It draws the page as the application does: with PDFium's colours, within F-1 but for at most 0.16 % of the pixels.
  Its own colours are COL-2's, the conversion of every OCAD map's colours and of the course drawn over it.
- Its library fits: PdfPig is Apache-2.0 and managed code alone, which runs wherever the application runs; the target
  needs no new call, only a paint for RGB colours; and the bitmap remains for what the reader cannot draw.
- Colour runs from OCAD's layers would serve structure but rest on OCAD's private marked content, and can come later
  without changing anything else.
- Its main cost falls on phase 8: the port has no PdfPig, so P-6 for a PDF map needs the reader's calls replayed or
  PdfPig translated.

## Open questions

- A PDF map is drawn as the bitmap `MapDisplay` makes of it, where PDF export copies the page as vectors. Whether the
  SVG can carry the page's vectors instead, and at what cost, is "A PDF base map as vectors" above, which ends with
  the choice put to @Pauli.
