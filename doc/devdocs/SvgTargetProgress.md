# SVG Graphics Target — Progress

Oct 24, 2026 · kept current by the sessions doing the work

Where the work on the SVG target stands against the phases of [its requirements](SvgTargetRequirements.md): what is done, what is under way and on which branch, what was decided and why, what waits on @Pauli, and the tasks that come next. It is written so that another executor can pick the work up from it and the branches alone: nothing a session keeps outside GitHub, such as its scratch folders, is needed. A session updates this file whenever a step ends, a task changes hands or a decision is taken.

## Where the work lives

| Branch | Holds |
| --- | --- |
| `docs/svg-target-requirements` | The requirements, this file, and the tools beside it: `SvgGallery/`, `SvgTolerance/` and `SvgGeoreference/` |
| `feature/svg-target` | The work, rooted at `upstream/master` (C-8), with the three `fix/` branches below merged in. Its tip, d58cb9e, holds phase 2's steps 1 to 4 and phase 4's step 1: step 4's record of a map's georeference (734778e), the EPSG codes of OCAD's grids (af9f9df) and the georeference's computation (43eb51e); then the base of every drawn element (0b0701b), the bitmaps (75bcf6b) and their five references (d58cb9e). |
| `feature/svg-text` | Phase 4's text, under way, rooted at `feature/svg-target`'s d58cb9e. Its tip, 975d23c, is one commit of work in progress, to be rewritten into the commits that finish the step; pushing it runs nothing. It leaves `CreateFont`, `DrawText` and `DrawTextOutline` out of DL-5's list, so its text cases stay pending; to compare them locally, add the three to the list without committing it. |
| `wip/linux/svg-target` | The full runs: `base` with `feature/svg-target` merged in (DL-10). Phase 2's was green in Tests run 39980577132; with the bitmaps, fbfcd6a was green in Tests run 39986756022. |
| `svg/svg-target` | `base` with `feature/svg-target` merged in, rebuilt for each loop run (DL-1) |
| `svg-results/svg-target` | The results of the latest loop run (DL-7) |
| `feature/svg-georeference`, `svg/svg-georeference`, `svg-results/svg-georeference` | The target's side of G-1 and G-2, rebased onto 828fd87 as 734778e and merged into `feature/svg-target`; nothing depends on them |
| `feature/svg-bitmaps`, `svg/svg-bitmaps`, `svg-results/svg-bitmaps` | Phase 4's bitmaps, finished from the unfinished b5caf74 as 0b0701b, 75bcf6b and d58cb9e, which `feature/svg-target` took as they stand; nothing depends on them |
| `svg-gallery` | The gallery served at <https://pauliojanpera.github.io/NotPurplePen/svg/>: its page, fonts and each entry's drawings, as one commit that each update replaces. Every publish of the site copies it into `svg/`. |
| `fix/backslashes-in-test-file-paths`, `fix/culture-independent-descriptor-parsing`, `fix/case-of-test-file-names` | Changes the tests need on Linux, each to be offered to Purple Pen on its own |
| `feature/svg-edge-tolerance`, `svg/svg-edge-tolerance`, `svg-results/svg-edge-tolerance` | The tolerance's edge criterion, merged into `feature/svg-target` as 033d648; nothing depends on them |
| `experiment/svg-target`, `experiment/machine-record`, `svg/diagnose-recording`, `svg-results/diagnose-recording`, `diag/segoe-ui-faces` | Throwaway runs; nothing depends on them |

The SVG loop, `.github/workflows/svg.yml`, and the machine check of the Windows tests are on `base`.

## How to pick the work up

- **Environment.** Linux with the .NET 10 SDK, PowerShell, Node with Playwright, and Python 3 with NumPy, SciPy and Pillow. In a cloud session, install the SDK with `dotnet-install.sh --channel 10.0 --azure-feed https://dotnetcli.blob.core.windows.net/dotnet`. Build `src/MapModel/Map_Svg.Tests`, then install Playwright's pinned Chromium with `bin/Debug/net10.0/playwright.ps1 install --only-shell chromium` (PowerShell from its GitHub release). Where GitHub's releases are out of reach, PowerShell installs as the .NET tool `PowerShell` from NuGet. The Playwright package's own driver also installs the browser without PowerShell: `bin/Debug/net10.0/.playwright/node/linux-x64/node bin/Debug/net10.0/.playwright/package/cli.js install --only-shell chromium`. The hosts this needs are api.nuget.org, dotnetcli.blob.core.windows.net, cdn.playwright.dev and storage.googleapis.com. A sparse checkout of `src/DotSpatialProjectionsLite`, `src/MapModel/{Graphics2D,MapModel,Map_Skia.Tests,Map_SkiaStd,Map_Svg,Map_Svg.Tests,RenderingTestCases}`, `src/MapModel/TestFiles/{pdfrender,rendering,skia_render,svg_reference}` and `src/Testing` builds and runs the SVG tests. The application's phases need `src/PurplePenCore`, `src/AvPurplePen`, `src/PurplePenViewModels` and their tests too.
- **Local run.** Run `dotnet test` on `src/MapModel/Map_Svg.Tests` with `SVG_LOOP_RESULTS` naming a results directory. The run reproduces the loop's measurements, documents and reference images byte for byte, and takes about two minutes.
- **Loop run.**
  - Start it by hand on the feature branch: `gh workflow run svg.yml -R pauliojanpera/NotPurplePen --ref dev -f ref=feature/svg-target`. The runner merges `base` into the branch, and the check run is attached to the branch's tip. `gh run list --workflow=svg.yml --event=workflow_dispatch --limit 1` gives the run's id; wait for it in the background, not by asking the API again and again.
  - To run the loop with a change to the workflow or its scripts, rebuild `svg/svg-target` instead, as one merge commit of the `ci/` branch, or `origin/base`, and `origin/feature/svg-target` (`git merge-tree --write-tree`, then `git commit-tree`), and push it with `--force-with-lease`.
  - The summary's "Since the previous run" says what changed from the results the branch held before, and `changes.md` lists each change; read them first.
  - Read `summary.md`, `failures.md` and `pending.tsv` on `svg-results/svg-target`. Its `measurements/`, `references/`, `differences/` and `renderings/` hold the numbers and images of the comparison.
  - A run whose tests pass can still fail in "Publish the results", when GitHub answers the push of the results with an Internal Server Error, as loop run 39985693403 first did. Its results are kept as the artifact `svg-loop-<name>` all the same; re-run its failed job to publish them.
  - Work on a branch beside `feature/svg-target` runs the same way on `svg/<name>`, with its results on `svg-results/<name>`.
- **New references.** A case that becomes drawable fails once, for want of a recorded reference, and the run holds the reference under `references/`. Check that the case's comparison is within the tolerance, copy the image into `src/MapModel/TestFiles/svg_reference` (`git add --sparse` in a sparse checkout), and commit it in its own commit.
- **Gallery.** Keep it current: publish whenever there is something new to show, about every 30 minutes of work, and at the end of each step.
  - Clone `svg-gallery`, and check out `svg-results/svg-target`.
  - The loop keeps Chromium's image of a case only when it falls outside the tolerance, so render the cases to show with `SvgGallery/render-cases.mjs` beside this file. `CHROMIUM_EXECUTABLE` names the headless shell of the tests' Playwright package, and its images match the loop's pixel for pixel.
  - Run `SvgGallery/make-gallery-step.py` on the checkout and the `svg_reference` of the commit the loop ran, naming the entry and the cases to show. It writes the entry's folder and prints the figures and census that `index.html` takes into `LOG` and `STANDING`. The census counts apart the drawings that wait only because they draw a bitmap lighter, which the target never will (B-6). A case that matches its reference exactly has no window of most difference, so choose its detail by hand.
  - Move the course's states on, and look at the page with `SvgGallery/look-at-gallery.mjs`.
  - Publish it with `SvgGallery/publish-svg-gallery.sh`, which replaces `svg-gallery`'s commit and puts the gallery under `svg/` on `gh-pages`.
  - A `wip/` branch made before `build.yml` copied `svg-gallery` into the site publishes without it, until the next publish from `base`, `dev` or a newer branch.
- **Full run.** A change is finished only after a `wip/` branch rooted at `base`, with the feature merged in, has run the Windows suite (DL-10). Name it `wip/linux/<name>` to package Linux alone. A machine whose Windows version or system fonts differ from `.github/ci/environment.json` is named in a warning and tested anyway, and its job's log prints the record that machine has, which replaces the file when baselines are recorded on it. A run started by hand with `machine_check` set to `stop` stops there instead.
- **Windows-only tests on Linux.** `Test_MapModel` and `PurplePen_Tests` target Windows, but one test file of theirs runs on Linux in a stand-in project of its own:
  - name it as the suite's assembly, so that `Test_MapModel`'s internals stay visible, and compile only that file with `TEST` defined;
  - reference the projects it needs, with the Linux native packages `SkiaSharp.NativeAssets.Linux` and `HarfBuzzSharp.NativeAssets.Linux`;
  - register Skia's services as `GlobalTestSetup` does, and put a `TestFiles` link beside it for `TestUtil` to find.

  It checks a test before the Windows suite runs it.
- **Working in parallel.** On Oct 22 the work ran in up to three agents at once. Each had its own worktree and its own branch, rooted at `feature/svg-target`, and its own loop on `svg/<name>`. One session merged their branches, rebased onto the latest `feature/svg-target`, and alone published the gallery and edited this file. That split keeps the agents out of each other's way, and suits a next executor as well as working serially does. From Oct 23 @Pauli asked for the work to go on without parallel agents, and one session carries it serially.
- **Session transcripts.** A session can end at any moment, when its tokens run out or its machine is restarted, so archive the transcripts at least every half an hour of work, as often as the gallery is published, and before a long break: on the branch `sessions` of the private repository `pauliojanpera/windows-fonts`, with `archive-session.sh <executor> <transcript>...` there (attach the repository with push access; its README says how to search the transcripts). Its `archive-if-due.sh`, run in the background by a post-commit hook of your clone, archives every transcript of the machine whenever the last archive is 10 minutes old. An archived file is replaced only by a transcript that extends it, and a transcript rewritten with less in it goes beside it as a revision. Claude Code keeps the transcript in `~/.claude/projects/<project>/<session id>.jsonl`; an executor that cannot read its own writes a journal of the session instead. The transcripts are searched when the question is what an executor did or tried; they are not read through, and this file stays the handover.
- **After a restart.** A restart of the machine can take its clones, worktrees, scratchpad and tools, and with them everything not pushed: on Oct 24 one did, and the next kept the disk. The `sessions` README lists what to do, in order: attach and clone again, put the hook back, archive at once, find what was lost against the restored transcript, and rebuild the work that was not committed with its `replay-edits.py`, which replays the transcript's edits onto the commit the work began from. Push work in progress at each step to a branch whose push runs nothing, such as `feature/svg-text`, and little needs rebuilding.
- **Tolerance and georeferencing research.** `SvgTolerance/` holds the scripts that chose the pixel tolerance, to run again whenever new cases arrive. `SvgGeoreference/` holds the checks of the EPSG codes of OCAD's grids for G-3.

## Status

| Phase | Status |
| --- | --- |
| 1, the development loop | Done. Since Oct 23 the results push is retried and the summary reports what changed since the previous run (`ci/svg-loop-report`, on `base` as 05873a9 and on `dev` as eb4d05d); the dispatch mode works, first run 40024278778. |
| 2, drawing, structure, layout, color and georeferencing | Done: steps 1 to 4, and the full run green. F-6, checked by hand, waits on @Pauli. |
| 3, the export in the application and course data | Not started. @Pauli left the order of phases 3 and 4 to the executor, and the task list takes phase 4 first. |
| 4, text and bitmaps | Step 1, bitmaps, done; step 2, text, under way on `feature/svg-text` |
| 5 to 8 | Not started |

| Step of phase 2 | Status |
| --- | --- |
| 1. The document, its layout and its writers (L-1 to L-5, F-2 to F-4, COL-1 to COL-6, S-1 to S-4, S-8, S-9, S-11) | Done (efa6995, loop run 39744433244) |
| 2. The drawing operations (D-1 to D-12) | Done (639332d, loop run 39783503957) |
| 3. The map structure (M-1 to M-8) | Done (f075a0c, loop run 39817113317) |
| 4. Georeferencing (G-1 to G-3), then the full `wip/` run, with F-6 checked by hand | Done: the record (734778e, loop run 39975400945), the EPSG codes of OCAD's grids (af9f9df, loop run 39979057271) and the computation (43eb51e, loop run 39980571564). The full run, Tests run 39980577132 on `wip/linux/svg-target`, passed, G-3's and the georeference's tests included |

| Step of phase 4 | Status |
| --- | --- |
| 1. Bitmaps (B-1 to B-6) | Done (0b0701b, 75bcf6b, d58cb9e; loop run 39985693403). The full run, Tests run 39986756022 on `wip/linux/svg-target`, passed |
| 2. Text (T-1 to T-10) | Under way on `feature/svg-text` (975d23c). With the text calls in DL-5's list locally, 6 text cases are compared: MagNorthArrowBug matches its reference; frametext2, kern_text_outline, RobotoTest, differentnewlinetypes and PenistoneHill have 490, 379, 81, 73 and 60 pixels outside the pixel tolerance, mostly where master rounds its glyph positions (see Decisions) |

On `feature/svg-target` at d58cb9e, loop run 39985693403 gives 190 passed, 0 failed, 86 pending. The 106 drawn cases all match their recorded references within the pixel tolerance. Of the 86 pending, 84 wait on `CreateFont`. The other 2, template_light and template2_light, wait on `DrawBitmap` alone and always will: they draw their templates at an intensity below 1 (B-6).

The calls the target fully implements, in the sense of DL-5: `Intensity`, `PushTransform`, `PopTransform`, `PushClip`, `PopClip`, `PushAntiAliasing`, `PopAntiAliasing`, `PushBlending`, `PopBlending`, `CreatePath`, `CreateSolidBrush`, `SupportsPatternBrushes`, `CreatePatternBrush`, `CreatePen`, `HasPath`, `HasBrush`, `HasPen`, `HasFont`, `DrawLine`, `DrawArc`, `DrawEllipse`, `FillEllipse`, `DrawRectangle`, `FillRectangle`, `DrawPolygon`, `DrawPolyline`, `FillPolygon`, `DrawPath`, `FillPath`, `DrawBitmap`, `DrawBitmapPart`.

## Tasks

In order of priority. A task ends with its commits on `feature/svg-target`, its loop green, the gallery and this file brought up to date.

1. **Text (T-1 to T-10).** Under way on `feature/svg-text`; it unblocks 84 cases. Done there: Liberation Sans and Serif kept in the repository and registered by the SVG tests as Arial and Times New Roman, beside Roboto from `Map_Skia.Tests`; `DrawText` and `DrawTextOutline` with the injected `ITextMetrics`, each text at its baseline with its kerning as `dx`; `IFontSubsetProvider`, a new interface in `Graphics2D`, implemented for the tests through `Map_SkiaStd`'s font loading; the `@font-face` rules, the fallback faces and the `fsType` check, with whole faces embedded until the subsetter is chosen. Left:
   - the subsetter, once @Pauli has chosen (T-7, see Waiting on @Pauli);
   - the families the test maps name that the tests do not register yet, such as Georgia, Calibri and Arial Narrow, so that no case draws in a font of the runner's own, and a way for the fallback case's scripts and emoji;
   - a check of where Chromium puts each character, read through the SVG DOM in the test page and compared in millimetres with the glyph positions of Skia's shaping, which PDF export writes;
   - the merging of line text (T-5);
   - the text cases' own tolerance (T-10), which has to allow for master's rounding of glyph positions;
   - the 84 cases' references, the loop, the full run and the gallery's entry.

   Check the tolerance again with `SvgTolerance/` once the cases come.
2. **Phase 3: the export in the application (A-1 to A-7, CD-1 to CD-3, V-2, V-4, V-5, V-7).** Not started. Work in stages:
   - a short design of where the export lives, how it shares `CoursePdf`'s layout code (A-3) and how `MapDisplay` draws a map's parts and a course part's layers apart (A-4);
   - the few `IStructuredGraphicsTarget` calls the course data needs (CD-1 to CD-3);
   - the core: `CourseSvgSettings`, the writer and the baseline tests. The writer gives each map that has one its georeference, `SetGeoreference(mapKey, coordinateMapper.GetMapGeoreference())`, once the map has begun (G-1, G-2, A-5). It passes the bitmap handling the user chose as `SvgBitmapOptions`, storing a linked bitmap beside the SVG through `storeLinkedBitmap` (B-4), and draws at full intensity (B-6);
   - the Avalonia command and dialog, to `AGENTS.md`'s conventions (A-1, A-6, A-7).

   Each stage gets a `wip/linux/` run for the Windows suite. The baseline tests also export one course to PDF and to SVG and compare each color's CMYK values.
3. **The loop's later improvements:**
   - keep every Chromium rendering in the run's artifact, so that the gallery and the tolerance checks need no local Chromium;
   - mark the calls the target will never honor, such as a bitmap drawn lighter (B-6), apart in `pending.tsv` and the summary;
   - after text, read each SVG back into page coordinates and compare its geometry with the case's drawing calls, within a step.
4. **Keep the record.** Bring this file up to date whenever a task ends or changes hands, and keep the gallery current: about every 30 minutes of work.

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
  - `CoordinateMapper.GetMapGeoreference` computes it. The transform is where `GetRealWorld`, which world files are written with, puts the map's origin and unit vectors. It gives null for a map without real-world coordinates. OCAD's grid 1000, which says that a map has no coordinate system, identifies none.
  - `RealWorldCoords.CoordinateSystemEpsg` gives the EPSG code however the map names its system: a code it was given, or the code of its OCAD grid. `Epsg` keeps holding only a given code, which `OpenMapperExport` writes as such.
- **The EPSG codes of OCAD's grids (G-3).** A row of `OcadToProj4Projection`'s table carries an EPSG code only where PROJ's definition of the code and the row's Proj4 string have the same unit and put every point of the code's area of use within 1 mm of each other. A deprecated code gives way to the replacement EPSG names, where that one passes.
  - 624 of the 642 rows carry one. `SvgGeoreference/README.md` lists the eight left without one and why, and `check_with_pyproj.py` beside it repeats the check.
  - Four of the eight follow EPSG definitions withdrawn as wrong, and place points 29 m to 170 km from EPSG's corrected systems; see "Waiting on @Pauli".
- **The loop's improvements.** On Oct 23 @Pauli asked whether the loop should be improved, and agreed to all the improvements in the task list, fonts in the repository included, leaving their order to the executor.
  - He also asked whether the SVG should be measured against a vector PDF baseline. A PDF rendered to pixels would add a third rasterizer and check what the Skia references already check, so the comparison stays with Skia. Vector checks go where pixels are blind: where Chromium puts each character (text), the geometry of each SVG against its drawing calls (after text), and the CMYK values of PDF and SVG export (phase 3).
- **Session transcripts.** On Oct 24 @Pauli asked for the sessions' full transcripts to be kept on GitHub, in a private repository, and chose the private `windows-fonts`, on a branch of their own. They are archived as the executors record them, unfiltered, since no token or secret reaches an executor's transcript, and at least every half an hour, since a session's tokens can run out at any moment. Later that night a restart took the machine's files, among them the text work that was neither committed nor pushed, and the turn under way; the text work was rebuilt from the transcript. The archive turned out to lack the session's first two hours: a compaction had moved the session to a new transcript, and the archive took only the newest. Since then every transcript of the machine is archived, an archived file is never replaced by a transcript that does not extend it, and, as @Pauli suggested, the interval is 10 minutes; git stores each archive as a delta of the one before, so the shorter interval adds no bytes.
- **Master's glyph positions.** On Oct 24 @Pauli pointed out uneven kerning in master's image of frametext2, and asked that the SVG not replicate master's typesetting bugs. Master's Skia target shapes text with HarfBuzz to fractional positions but draws it with `SKFont.Subpixel` off, which rounds each glyph's origin to a whole device pixel, so that in rotated text the glyphs wander by up to half a pixel along and across the line. The SVG keeps the shaped positions. With subpixel positioning on in the references, the cases outside the tolerance fall from 490 pixels to 37 (frametext2), 379 to 85 (kern_text_outline), 81 to 21 (RobotoTest) and 73 to 48 (differentnewlinetypes), while PenistoneHill rises from 60 to 85, which is still to be understood. How the comparison allows for the rounding is part of T-10. Turning subpixel positioning on in master is a fix that could be offered to Purple Pen, but it moves the Skia baselines of every text case.
- **Offering to Purple Pen.** @Pauli asked on Oct 23 that easily separable work be kept in mind as pull requests of its own, weighing the use of each piece. A change useful to Purple Pen on its own therefore goes on a branch of its own rooted at `upstream/master`, as the three `fix/` branches are, and `feature/svg-target` merges it; the weighing comes before the piece is built. The EPSG codes of OCAD's grids are not useful on their own, as only the SVG export reads them, so they stay on `feature/svg-target`. A correction of the four grids under "Waiting on @Pauli" would be useful on its own, if OCAD defines them as EPSG's corrected systems.
- **Pattern brushes.** `CreatePatternBrush` returns a target that draws the tile as the target draws, with keys of its own and at its creator's intensity, and refuses structure calls. `FinishBrush` makes the tile a brush of its creator, and a pen made from it strokes with the pattern (D-6, D-7).
  - The document defines the pattern in the user space of what it paints, as the Skia target repeats its tile: `x`, `y` and a `viewBox` put the tile's middle on the origin, in the tile's own coordinates, and `patternTransform` turns it by the brush's angle.
  - The bitmap size, a hint to raster targets, is not used.
  - A style refers to its pattern as `url(#)` until the document is written, when the pattern's id is named in the order patterns are first used. Only patterns that a style paints with are written.
  - Equal patterns are separate definitions until S-12 shares them, in phase 6.
- **Bitmaps.** A bitmap is written through `IGraphicsBitmap.WriteToStream`: as JPEG at the options' quality, 90 by default, when JPEG is its own format, and as PNG otherwise (B-3). The document defines each distinct data once, as an `<image>` with an id after the parts, so the drawing comes before the long data (B-5).
  - Each draw is a `<use>` of the image carrying its object's number, under the transform from the image's pixels to the rectangle, with the digits the image's diagonal needs. A part is the bitmap cropped (B-1).
  - Nearest-neighbor scaling is a class with `image-rendering:optimizeSpeed;image-rendering:pixelated`; the other modes leave Chromium's smoothing (B-2). Through `<use>`, Chromium samples pixelated as Skia does: BitmapPart matches its reference exactly.
  - `SvgBitmapOptions` embed the data as a base64 data URI, the default; link it through `storeLinkedBitmap`, which stores the data and gives the href; or leave bitmaps out (B-4).
  - A bitmap drawn at an intensity below 1 raises `Svg_UnsupportedCallException` naming the call (B-6).
  - Smoothed, an upright bitmap differs from Skia's along a few rows and columns of pixels, by at most 10 levels in Bitmap and 22 in testlogo's logo; a template its map turns differs throughout, by up to 87 levels in single pixels. All five are within the tolerance, which needed no change.
  - Each draw encodes its bitmap anew, and the document keeps the first of equal data. Encoding once per bitmap would save time where one bitmap is drawn often; it is not done.
- **Blending.** Darken is the only mode, written as a group with `mix-blend-mode:darken` that never merges with its neighbours; the map opens it around the run of one overprinted color, so the group blends that run as a whole (D-8). Any other mode is an unsupported call.
- **The comparison.** Each case is one map in one part on one page whose millimetres are the image's pixels, and Chromium shows the SVG through an HTML page that sizes it to the image.
  - The census records each case with `StructuredRecordingGraphicsTarget`: a `RecordingGraphicsTarget` that also records, through `RecordStructure`, the structure calls the map code makes, and plays them back onto a target that keeps structure (DL-4). They are not drawing calls, so the census leaves them out.
  - The Skia target renders each reference image from the case's own drawing, anti-aliased on white (V-9), its clips anti-aliased too, as Chromium draws every clip path whatever its shape-rendering.
  - A missing reference goes to the results' `references/`, from where it is committed into `src/MapModel/TestFiles/svg_reference`.
- **The pixel tolerance (F-1).** `PixelTolerance` holds the rule, so that a case can carry a tolerance of its own, as V-3 foresees for fine evenly spaced lines; the check of Chromium's rendering on whole pixels keeps an exact one. Beyond the image both images count as white paper, which keeps shapes cut by the image's edge fair.
  - A pixel of Chromium's image passes when each color channel lies within the range that channel takes in the reference within one pixel, give or take 4 levels, and the same holds the other way round.
  - Inside a shape this asks for a match within 4 levels; along an edge it lets any shade between the colors meeting there pass.
  - Where the edges of two shapes meet in one pixel, each rasterizer mixes their colors with the one beneath its own way, which can fall outside every neighbour's range. Such a pixel, with an edge within reach, passes when the two images, each blurred by a Gaussian of one pixel with white paper beyond them, differ there in every channel by at most 40 levels and by at most 0.4 of the contrast around it. The contrast is the widest range a channel takes in either image within the blur's reach of 4 pixels.

  In the 106 drawn cases, pixels differ by up to 191 levels along edges. In 57 of them, pixels where edges meet lie up to 102 levels outside the range; there the blurred images differ by at most 34.6 levels, and by at most 0.36 of the contrast.

  With errors planted in Chromium's images, the rule fails:
  - all 106 cases shifted by two pixels, the faintest included;
  - all 106 with features two pixels wider;
  - 96 of the 97 with a solid feature when their smallest one is removed;
  - 93 of the 105 with ink to change when their ink is six levels redder.

  The other 12 hold their ink in strokes and dots too thin to have an inside. The five bitmap cases fail with every planted error; checked again over loop run 39985693403.

## Waiting on @Pauli

- **F-6**, opening the output in Inkscape and Scribus, by hand at the end of phase 2. Handed over on Oct 23: the SVGs of loop run 39980571564, in its artifact `svg-loop-svg-target` (kept until Oct 30) and in `svg/` on `svg-results/svg-target`. Each color's CMYK values should read from its `icc-color()`.
- **The subsetter (T-7).** Asked on Oct 24. PDFsharp's subsetter, which T-7 names, cuts subsets that Chromium rejects, as they lack the OS/2, name and post tables. With those added, a subset of 13 glyphs of Liberation Sans takes 26 KB, since PDFsharp keeps a slot for each of the font's 2,620 glyphs, where a subsetter that renumbers the glyphs makes it in 1.5 KB. The recommendation is to write that one, about 400 lines beside `Map_SkiaStd`'s font code, and reword T-7; the alternative is PDFsharp's with a fix-up, some 15–25 KB more per face in every SVG. Until then the tests embed whole faces.
- **Four grids that follow withdrawn EPSG definitions.** Grids 50050 (Kentucky North), 66044 and 66045 (Indiana East and West) and 66098 (South Dakota North) follow 26979, 2244, 2245 and 3454, which EPSG withdrew as wrong. Correcting their Proj4 strings would move maps already georeferenced by them, so it is @Pauli's call, and probably upstream's. Until then they name no EPSG code. Raised on Oct 23.

## The test machine

`.github/ci/environment.json` records Segoe UI 5.72, on both `base` and `dev`, which @Pauli settled should match here. GitHub's windows-11-arm runners serve several images at once:
- 20261018.189.1 and 20261021.192.1 carry Segoe UI 5.72;
- 20261013.183.4 still carries 5.71, and the suite passes on both versions.

The machine check used to stop every job on a machine unlike the record. It stopped three jobs of `dev`'s Tests run 39861565207, and `base`'s run 39742515380 passed only on its fifth attempt.

`ci/mixed-runner-images`, landed on `base` as e0582c6 and on `dev` as 050ad37, changes two things:
- **The machine check.** It reports such a machine as a warning and tests anyway; `machine_check` set to `stop` in a run started by hand stops there.
- **Publishing.** It follows the commit's newest test run, and after a failure watches for a re-run until its 40-minute deadline.

`base`'s Tests run 39978811765 took the verdict of `wip/linux/mixed-runner-images`'s run 39976836532. `dev`'s Tests run 39978845567 passed, and both Builds published behind the new wait.

## The previous executor

The session that wrote this file ran in Anthropic's cloud. It disabled its scheduled check-ins when the work moved to another environment, so it does not wake again to work beside the next executor.
