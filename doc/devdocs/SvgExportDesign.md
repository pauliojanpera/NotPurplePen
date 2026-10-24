# SVG export in the application: design

Phase 3's first stage (see `SvgTargetProgress.md`, Tasks): where the export lives, how it shares PDF export's code
(A-3), and how `MapDisplay` draws a map's parts and a course part's layers apart (A-4, L-2 to L-4). Written on Oct 25
from `feature/svg-target` at b091333.

## Where the export lives

- `CourseSvg`, in `PurplePenCore` beside `CoursePdf`, writes the files. `CourseSvgSettings` (A-2) holds PDF export's
  course, variation, map exchange, base map, description, crop, color model, directory and prefix fields, with CMYK as
  the default color model, plus the bitmap handling, JPEG quality and compression.
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

`MapDisplay` is set up as `CoursePdf` sets it: map intensity 1, `Printing`, so no pattern bitmaps, the chosen color
model, and no print area, so no outline. A PDF map, which PDF export copies as vectors, is drawn as the bitmap
`MapDisplay` makes of it (B-1 to B-6).

## Course data (stage 2, CD-1 to CD-3)

- **Which symbols each course object adds (CD-3).** `CourseLayout.RenderToMap` gets an overload that also fills a
  dictionary from each map `Symbol` to the `CourseObj` that added it, by the map's symbols before and after each
  `CourseObj.AddToMap`. The map code already opens each symbol as an object, with the `Symbol` as its key
  (`Map.Draw`'s `BeginObject(curSym, ...)`), so the key reaches the target unchanged.
- **What the target is told.** Two calls join `IStructuredGraphicsTarget`, with plain data only, as `Graphics2D`
  carries no course types:
  - `DescribeCourse(object courseKey, CourseRecord course)`: the name, part, variation and print scale, and the
    controls in order, each with its code, kind and position in map coordinates (CD-1), and the parts that draw it;
  - `DescribeObjects(object mapKey, IReadOnlyDictionary<object, CourseObjectRecord> objects)`: for each object key of
    the course's map, its course, its kind, and its controls: one for a control circle, start, finish or control
    number, two for a leg, none for a description or a special object (CD-2).
- **What the document writes.** Elements keep only their object number (S-11). The metadata gains, for each course,
  `<npp:course>` with its controls as `<npp:control>` children, and for each course object an `<npp:object n="..."
  course="..." kind="..." controls="..."/>` keyed by the object number its elements carry. The element and attribute
  names are a proposal, to settle with @Pauli before the format's version is raised.
- **Kinds.** From the `CourseObj` class: `ControlCourseObj` control, `StartCourseObj` start, `FinishCourseObj`
  finish, `ControlNumberCourseObj` control-number, `LegCourseObj`, `FlaggedLegCourseObj` and `TopologyLegCourseObj`
  leg, `DescriptionCourseObj` description, and each special its own kind; controls from `courseControlId`, and for a
  leg from both ends.

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

## Open questions

- Phase 3 writes one page per file. Should the dialog already show L-7's three file choices, with all but one
  disabled, or only the one?
- A PDF map is drawn as a bitmap. Is that acceptable for the first release, or should such events be refused?
