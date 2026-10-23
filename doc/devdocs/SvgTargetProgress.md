# SVG Graphics Target — Progress

Oct 23, 2026 · kept current by the sessions doing the work

Where the work on the SVG target stands against the phases of [its requirements](SvgTargetRequirements.md): what is done, what is under way and on which branch, what was decided and why, what waits on @Pauli, and the tasks that come next. It is written so that another executor can pick the work up from it and the branches alone: nothing a session keeps outside GitHub, such as its scratch folders, is needed. A session updates this file whenever a step ends, a task changes hands or a decision is taken.

## Where the work lives

| Branch | Holds |
| --- | --- |
| `docs/svg-target-requirements` | The requirements, this file, and the tools beside it: `SvgGallery/`, `SvgTolerance/` and `SvgGeoreference/` |
| `feature/svg-target` | The work, rooted at `upstream/master` (C-8), with the three `fix/` branches below merged in. Its tip, 734778e, holds phase 2's steps 1 to 3 and the target's side of step 4's G-1 and G-2. |
| `svg/svg-target` | `base` with `feature/svg-target` merged in, rebuilt for each loop run (DL-1) |
| `svg-results/svg-target` | The results of the latest loop run (DL-7) |
| `feature/svg-georeference`, `svg/svg-georeference`, `svg-results/svg-georeference` | The target's side of G-1 and G-2, rebased onto 828fd87 as 734778e and merged into `feature/svg-target`; nothing depends on them |
| `feature/svg-bitmaps` | Phase 4's bitmaps: one commit of unfinished work, b5caf74, rooted at 033d648, to be finished and rewritten before it merges. See the task list. |
| `svg-gallery` | The gallery served at <https://pauliojanpera.github.io/NotPurplePen/svg/>: its page, fonts and each entry's drawings, as one commit that each update replaces. Every publish of the site copies it into `svg/`. |
| `fix/backslashes-in-test-file-paths`, `fix/culture-independent-descriptor-parsing`, `fix/case-of-test-file-names` | Changes the tests need on Linux, each to be offered to Purple Pen on its own |
| `feature/svg-edge-tolerance`, `svg/svg-edge-tolerance`, `svg-results/svg-edge-tolerance` | The tolerance's edge criterion, merged into `feature/svg-target` as 033d648; nothing depends on them |
| `experiment/svg-target`, `experiment/machine-record`, `svg/diagnose-recording`, `svg-results/diagnose-recording`, `diag/segoe-ui-faces` | Throwaway runs; nothing depends on them |

The SVG loop, `.github/workflows/svg.yml`, and the machine check of the Windows tests are on `base`.

## How to pick the work up

- **Environment.** Linux with the .NET 10 SDK, PowerShell, Node with Playwright, and Python 3 with NumPy, SciPy and Pillow. In a cloud session, install the SDK with `dotnet-install.sh --channel 10.0 --azure-feed https://dotnetcli.blob.core.windows.net/dotnet`. Build `src/MapModel/Map_Svg.Tests`, then install Playwright's pinned Chromium with `bin/Debug/net10.0/playwright.ps1 install --only-shell chromium` (PowerShell from its GitHub release). Where GitHub's releases are out of reach, PowerShell installs as the .NET tool `PowerShell` from NuGet. The Playwright package's own driver also installs the browser without PowerShell: `bin/Debug/net10.0/.playwright/node/linux-x64/node bin/Debug/net10.0/.playwright/package/cli.js install --only-shell chromium`. The hosts this needs are api.nuget.org, dotnetcli.blob.core.windows.net, cdn.playwright.dev and storage.googleapis.com. A sparse checkout of `src/DotSpatialProjectionsLite`, `src/MapModel/{Graphics2D,MapModel,Map_Skia.Tests,Map_SkiaStd,Map_Svg,Map_Svg.Tests,RenderingTestCases}`, `src/MapModel/TestFiles/{pdfrender,rendering,skia_render,svg_reference}` and `src/Testing` builds and runs the SVG tests. The application's phases need `src/PurplePenCore`, `src/AvPurplePen`, `src/PurplePenViewModels` and their tests too.
- **Local run.** Run `dotnet test` on `src/MapModel/Map_Svg.Tests` with `SVG_LOOP_RESULTS` naming a results directory. The run reproduces the loop's measurements, documents and reference images byte for byte, and takes about two minutes.
- **Loop run.**
  - Rebuild `svg/svg-target` as one merge commit of `origin/base` and `origin/feature/svg-target`: `git merge-tree --write-tree`, then `git commit-tree`.
  - Push it with `--force-with-lease`, and follow the run through the GitHub API.
  - Read `summary.md`, `failures.md` and `pending.tsv` on `svg-results/svg-target`. Its `measurements/`, `references/`, `differences/` and `renderings/` hold the numbers and images of the comparison.
  - Work on a branch beside `feature/svg-target` runs the same way on `svg/<name>`, with its results on `svg-results/<name>`.
- **New references.** A case that becomes drawable fails once, for want of a recorded reference, and the run holds the reference under `references/`. Check that the case's comparison is within the tolerance, copy the image into `src/MapModel/TestFiles/svg_reference` (`git add --sparse` in a sparse checkout), and commit it in its own commit.
- **Gallery.** Keep it current: publish whenever there is something new to show, about every 30 minutes of work, and at the end of each step.
  - Clone `svg-gallery`, and check out `svg-results/svg-target`.
  - The loop keeps Chromium's image of a case only when it falls outside the tolerance, so render the cases to show with `SvgGallery/render-cases.mjs` beside this file. `CHROMIUM_EXECUTABLE` names the headless shell of the tests' Playwright package, and its images match the loop's pixel for pixel.
  - Run `SvgGallery/make-gallery-step.py` on the checkout and the `svg_reference` of the commit the loop ran, naming the entry and the cases to show. It writes the entry's folder and prints the figures and census that `index.html` takes into `LOG` and `STANDING`.
  - Move the course's states on, and look at the page with `SvgGallery/look-at-gallery.mjs`.
  - Publish it with `SvgGallery/publish-svg-gallery.sh`, which replaces `svg-gallery`'s commit and puts the gallery under `svg/` on `gh-pages`.
  - A `wip/` branch made before `build.yml` copied `svg-gallery` into the site publishes without it, until the next publish from `base`, `dev` or a newer branch.
- **Full run.** A change is finished only after a `wip/` branch rooted at `base`, with the feature merged in, has run the Windows suite (DL-10). Name it `wip/linux/<name>` to package Linux alone. The suite stops on a machine whose Windows version or system fonts differ from `.github/ci/environment.json`; a run started by hand with `machine_check` set to `report` tests anyway, and the stopped job's log prints the record that machine would need.
- **Working in parallel.** On Oct 22 the work ran in up to three agents at once. Each had its own worktree and its own branch, rooted at `feature/svg-target`, and its own loop on `svg/<name>`. One session merged their branches, rebased onto the latest `feature/svg-target`, and alone published the gallery and edited this file. That split keeps the agents out of each other's way, and suits a next executor as well as working serially does. From Oct 23 @Pauli asked for the work to go on without parallel agents, and one session carries it serially.
- **Tolerance and georeferencing research.** `SvgTolerance/` holds the scripts that chose the pixel tolerance, to run again whenever new cases arrive. `SvgGeoreference/` holds the checks of the EPSG codes of OCAD's grids for G-3.

## Status

| Phase | Status |
| --- | --- |
| 1, the development loop | Done. The dispatch mode of `svg.yml` is untried until the workflow reaches `dev`, the default branch. |
| 2, drawing, structure, layout, color and georeferencing | Steps 1 to 3 done; step 4 under way. Then the full `wip/` run, with F-6 checked by hand. |
| 3, the export in the application and course data | Not started. @Pauli left the order of phases 3 and 4 to the executor; both may go on beside phase 2's end. |
| 4, text and bitmaps | Bitmaps under way on `feature/svg-bitmaps`; text not started |
| 5 to 8 | Not started |

| Step of phase 2 | Status |
| --- | --- |
| 1. The document, its layout and its writers (L-1 to L-5, F-2 to F-4, COL-1 to COL-6, S-1 to S-4, S-8, S-9, S-11) | Done (efa6995, loop run 39744433244) |
| 2. The drawing operations (D-1 to D-12) | Done (639332d, loop run 39783503957) |
| 3. The map structure (M-1 to M-8) | Done (f075a0c, loop run 39817113317) |
| 4. Georeferencing (G-1 to G-3), then the full `wip/` run, with F-6 checked by hand | Under way: the target's side of G-1 and G-2 is done (734778e, loop run 39975400945); G-3 and the computation of a map's georeference remain; see the task list |

On `feature/svg-target` at 734778e, loop run 39975400945 gives 179 passed, 0 failed, 91 pending. The 101 drawn cases all match their recorded references within the pixel tolerance. Of the 91 pending, 84 wait on `CreateFont` and 7 on `DrawBitmap` alone.

The calls the target fully implements, in the sense of DL-5: `Intensity`, `PushTransform`, `PopTransform`, `PushClip`, `PopClip`, `PushAntiAliasing`, `PopAntiAliasing`, `PushBlending`, `PopBlending`, `CreatePath`, `CreateSolidBrush`, `SupportsPatternBrushes`, `CreatePatternBrush`, `CreatePen`, `HasPath`, `HasBrush`, `HasPen`, `HasFont`, `DrawLine`, `DrawArc`, `DrawEllipse`, `FillEllipse`, `DrawRectangle`, `FillRectangle`, `DrawPolygon`, `DrawPolyline`, `FillPolygon`, `DrawPath`, `FillPath`.

## Tasks

In order of priority. A task ends with its commits on `feature/svg-target`, its loop green, the gallery and this file brought up to date.

1. **The EPSG codes of OCAD's grids (G-3).** Follow `SvgGeoreference/README.md`:
   - take the confirmed codes into `OcadToProj4Projection`'s table, with a lookup;
   - check the rest one by one, and leave out what cannot be confirmed;
   - test it in the map model's tests of `MapProjection`.
2. **The georeference's computation (G-1, A-5).** In `src/PurplePenCore`, beside `CoordinateMapper` (`GpxFile.cs`) and the world-file code of `ExportBitmap.cs`, add the function the export calls to build a map's `MapGeoreference`, with tests where PurplePenCore's coordinate code is tested. These tests run only in the Windows suite.
3. **Phase 2's full run.** Push `wip/linux/svg-target`, rooted at `base` with `feature/svg-target` merged in, and get the Windows suite green. Base's latest Windows tests passed on Oct 22, the machine check included. Hand @Pauli the run's SVGs for F-6. Then mark phase 2 done here and in the gallery.
4. **Bitmaps (B-1 to B-6).** b5caf74 on `feature/svg-bitmaps` is unfinished work. It holds:
   - `SvgBitmapOptions`: embedded, linked through a store the caller gives, or left out, and the JPEG quality (B-3, B-4);
   - `SvgImage` and `SvgDocument.ImageOf`, which hold each written bitmap once, keyed by its data, and write the images last among the definitions (B-5);
   - `SvgImageUse`, which draws an image over its rectangle through `<use>` and a transform;
   - `SvgElement`, a base for elements that carry an object number.

   Still to do:
   - Rebase it onto `feature/svg-target`, whose 828fd87 split `SvgShape` into `SvgPath` and `SvgEllipse`, by making `SvgShape` derive from `SvgElement`.
   - Write `DrawBitmap` and `DrawBitmapPart` through `IGraphicsBitmap.WriteToStream`, in its own format when that is PNG or JPEG and as PNG otherwise.
   - Render `NearestNeighbor` pixelated (B-2).
   - Raise the error of D-12 at an intensity below 1 (B-6).
   - Add document tests and the new references, and measure Chromium's smoothing against Skia's.
   - Add `DrawBitmap` to the fully implemented calls only when all of B-1 to B-6 hold.
   - Rewrite the commit into proper ones before merging.
5. **Text (T-1 to T-10).** Not started; it unblocks 84 cases. It covers:
   - `DrawText` and `DrawTextOutline` with the injected `ITextMetrics`;
   - `IFontSubsetProvider`, a new interface in `Graphics2D`, implemented by the application, and by the tests through `Map_SkiaStd`'s font loading and PdfSharp's subsetter;
   - `@font-face` subsets in the style block, the fallback faces, and the `fsType` check;
   - the merging of line text (T-5);
   - the text cases' own tolerance (T-10).

   Check the tolerance again with `SvgTolerance/` once the cases come.
6. **Phase 3: the export in the application (A-1 to A-7, CD-1 to CD-3, V-2, V-4, V-5, V-7).** Not started. Work in stages:
   - a short design of where the export lives, how it shares `CoursePdf`'s layout code (A-3) and how `MapDisplay` draws a map's parts and a course part's layers apart (A-4);
   - the few `IStructuredGraphicsTarget` calls the course data needs (CD-1 to CD-3);
   - the core: `CourseSvgSettings`, the writer and the baseline tests;
   - the Avalonia command and dialog, to `AGENTS.md`'s conventions (A-1, A-6, A-7).

   Each stage gets a `wip/linux/` run for the Windows suite.
7. **Keep the record.** Bring this file up to date whenever a task ends or changes hands, and keep the gallery current: about every 30 minutes of work.

## Decisions

- **The structured interface.** `IStructuredGraphicsTarget`, in `Graphics2D`, extends `IGraphicsTarget`, and @Pauli approved its calls:
  - `BeginPart` and `EndPart` around a part of a drawing, which the document defines once;
  - `BeginMap` and `EndMap` around a labeled map, nested for a template;
  - `AddPage(paperSize, mapArea, mapToPage, partKeys)`, made outside parts;
  - `BeginColorRun(colorKey, position, name, color, overprint)` with `EndColorRun`;
  - `BeginObject(objectKey, symbolKey, symbolId, symbolName)` with `EndObject`;
  - `SetGeoreference(mapKey, MapGeoreference)`, which a later call for the same map replaces.
- **Shared code.** `IdentityComparer` lives in `Graphics2D`, from where every graphics target, the SVG target too, keys its resources by identity (C-4, C-6).
- **The document.** Parts are groups with ids in `<defs>`. A page is a group carrying the page's transform and a clip of its map area, with a `<use>` for each part it shows. Pages after the first lie to its right, 10 mm apart, and the root has the first page's size in millimetres. The metadata's root is `npp:document version="1"`, holding an `npp:map` with the number and label of each map, whose groups carry `data-m`.
- **Numbers.** Coordinates are whole steps of 0.001 mm, rounded half up as `floor(x + 0.5)`, which every runtime does alike (P-6). A transform carries the digits that the reach of its content needs: within half a step for its translation, and within 0.45 of a step for its linear part, shared between angle (0.2), scale (0.2) and shear (0.05).
- **The text.** Each path command takes its shortest form; arc flags keep their separators, which SVG 1.1 readers need, and transform functions follow each other without one. Classes and ids are named in the order they are first used, the shortest names first. Groups are tidied before writing:
  - empty ones are dropped, and those carrying nothing dissolved;
  - an only child is folded into its parent where the order of transform and clip allows;
  - a group carrying nothing but a translation and holding one ellipse gives way to the ellipse moved into place, its center rounded to whole steps;
  - adjacent groups with equal attributes are merged, unless they blend.

  @Pauli approved the folding of single ellipses, which takes irregular1's document from 298,736 to 220,661 bytes.
- **Colors.** `icc-color()` names the profile `cmyk`, declared as `<color-profile name="cmyk"/>` with no ICC profile behind it. The sRGB comes from the converter passed in; the comparison passes `DefaultColorConverter`, which converts as the Skia references do.
- **Anti-aliasing.** Browsers anti-alias by default, so a group carries `shape-rendering` only where the mode changes.
- **Errors.** A call the target does not implement yet raises `Svg_UnsupportedCallException`, which makes a comparison case pending; a call out of turn raises an `InvalidOperationException` that names it.
- **Pens.** A pen is a class: `fill:none`, its paint, its width in steps, then its cap and join where they differ from SVG's defaults of butt, miter and a miter limit of 4 (D-5, S-9). Skia bevels every corner under a miter limit of 1 or less, which SVG would take for an error, so such a pen is written with a bevel join. A width of 0, Skia's hairline of one device pixel at any scale, has no unit in a document and is an unsupported call, as the clipped miter join is. A pen made from a brush strokes with the brush's paint (D-6).
- **Shapes.** Lines, polylines, polygons, rectangles and paths are written as paths (`SvgPath`), ellipses as circles or ellipses (`SvgEllipse`). An arc's angles are degrees turning from the x axis towards the y axis, as Skia's are; it is written as an arc command, and an arc of a full turn or more as a circle, as Skia draws it as a closed oval (D-4). A path's parts become its commands, a line or curve with no start before it beginning at the origin as in Skia, and a path with nothing in it draws nothing (D-3).
- **Clips.** A clip is a group with a `clip-path`. The document defines each clip path once for all equal clips, the page's among them, since a clip path is read in the user space of what refers to it. The union of rectangles is one subpath per rectangle, each turning clockwise, under the nonzero rule (D-2).
- **The map's structure.** To a target that implements `IStructuredGraphicsTarget`, `Map.DrawColor` opens a run of a color of the color table before its first object and closes it after its last, and brackets each object with its symbol. The image and layout layers, which are not colors of the table, open no run, so their objects sit in their map outside any run. `Map.DrawTemplates` labels each template with its file name (M-3 to M-5).
  - A run is a group carrying `data-c`, the color's position in its map's color table.
  - Consecutive objects of one symbol share a group carrying `data-s`: the symbol's number in its map's symbol table, counted from 1 in the order the map's symbols first begin an object. A call that closes or begins something around the group closes it, once its last object has ended.
  - Each element carries `data-o`, its object's number, counted across the document from 1 in the order objects are first drawn, so an object drawn in several runs has one number (M-6, S-11).
  - Each `npp:map` holds, in table order, an `npp:color` for each color its runs drew: `position`, `name`, `cmyk` with the digits of `icc-color()`, and `overprint="1"` when it overprints. Then comes an `npp:symbol` for each symbol its objects drew: `n`, `id`, `name`, and `colors`, the positions of the colors its objects were drawn in (M-7).
  - The structure adds about 21 % to the SVG of the 101 drawn cases, most of it the object numbers.
- **Georeferencing.** `MapGeoreference`, a plain value in `Graphics2D`, carries the affine transform from millimetres of map to real-world coordinates and the map's EPSG code, Proj4 string and OCAD grid and zone id (G-1, G-2).
  - A map's record in the metadata opens with `npp:georeference`, ahead of its tables, so a map with a georeference and no tables still has a record.
  - Its `transform` takes steps of map to the real world. It is written as every transform is (S-8), and left out when it moves nothing (S-9). It keeps the digits that hold a point within a step of map over the 8 m the map's coordinates reach (S-2).
  - `epsg`, `proj4` and `ocad-grid-zone` follow where the map has them.
- **Pattern brushes.** `CreatePatternBrush` returns a target that draws the tile as the target draws, with keys of its own and at its creator's intensity, and refuses structure calls. `FinishBrush` makes the tile a brush of its creator, and a pen made from it strokes with the pattern (D-6, D-7).
  - The document defines the pattern in the user space of what it paints, as the Skia target repeats its tile: `x`, `y` and a `viewBox` put the tile's middle on the origin, in the tile's own coordinates, and `patternTransform` turns it by the brush's angle.
  - The bitmap size, a hint to raster targets, is not used.
  - A style refers to its pattern as `url(#)` until the document is written, when the pattern's id is named in the order patterns are first used. Only patterns that a style paints with are written.
  - Equal patterns are separate definitions until S-12 shares them, in phase 6.
- **Blending.** Darken is the only mode, written as a group with `mix-blend-mode:darken` that never merges with its neighbours; the map opens it around the run of one overprinted color, so the group blends that run as a whole (D-8). Any other mode is an unsupported call.
- **The comparison.** Each case is one map in one part on one page whose millimetres are the image's pixels, and Chromium shows the SVG through an HTML page that sizes it to the image.
  - The census records each case with `StructuredRecordingGraphicsTarget`: a `RecordingGraphicsTarget` that also records, through `RecordStructure`, the structure calls the map code makes, and plays them back onto a target that keeps structure (DL-4). They are not drawing calls, so the census leaves them out.
  - The Skia target renders each reference image from the case's own drawing, anti-aliased on white (V-9), its clips anti-aliased too, as Chromium draws every clip path whatever its shape-rendering.
  - A missing reference goes to the results' `references/`, from where it is committed into `src/MapModel/TestFiles/svg_reference`.
- **The pixel tolerance (F-1).** `PixelTolerance` holds the rule, so that a case can carry a tolerance of its own, as V-3 foresees for fine evenly spaced lines; the check of Chromium's rendering on whole pixels keeps an exact one. Beyond the image both images count as white paper, which keeps shapes cut by the image's edge fair.
  - A pixel of Chromium's image passes when each color channel lies within the range that channel takes in the reference within one pixel, give or take 4 levels, and the same holds the other way round.
  - Inside a shape this asks for a match within 4 levels; along an edge it lets any shade between the colors meeting there pass.
  - Where the edges of two shapes meet in one pixel, each rasterizer mixes their colors with the one beneath its own way, which can fall outside every neighbour's range. Such a pixel, with an edge within reach, passes when the two images, each blurred by a Gaussian of one pixel with white paper beyond them, differ there in every channel by at most 40 levels and by at most 0.4 of the contrast around it. The contrast is the widest range a channel takes in either image within the blur's reach of 4 pixels.

  In the 101 drawn cases, pixels differ by up to 191 levels along edges. In 55 of them, pixels where edges meet lie up to 102 levels outside the range; there the blurred images differ by at most 34.6 levels, and by at most a third of the contrast.

  With errors planted in Chromium's images, the rule fails:
  - all 101 cases shifted by two pixels, the faintest included;
  - all 101 with features two pixels wider;
  - 91 of the 92 with a solid feature when their smallest one is removed;
  - 88 of the 100 with ink to change when their ink is six levels redder.

  The other 12 hold their ink in strokes and dots too thin to have an inside.

## Waiting on @Pauli

- **F-6**, opening the output in Inkscape and Scribus, by hand at the end of phase 2.

## The test machine

`.github/ci/environment.json` records Segoe UI 5.72, which GitHub's Windows image 20261018.189.1 carries, on both `base` and `dev`. @Pauli settled that `dev` matches `base` here. `dev` took it up by merging `base` (7d1ce11), which also brought `dev` the SVG loop, the machine check that requires only the Windows version and the fonts, and the fork instructions' pointer to this file. Check that `dev`'s Tests run 39861565207, the first after the merge, is green.

## The previous executor

The session that wrote this file ran in Anthropic's cloud. It disabled its scheduled check-ins when the work moved to another environment, so it does not wake again to work beside the next executor.
