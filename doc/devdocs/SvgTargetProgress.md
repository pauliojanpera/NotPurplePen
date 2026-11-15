# SVG Graphics Target — Progress

Nov 16, 2026. The sessions that do the work keep this file current.

This file is the status of the SVG target work: its phases, its own decisions, its loop, its gallery and its tools. The handover of the work, with the branches, the process, the tasks and what waits on @Pauli, is [NotPurplePenProgress.md](NotPurplePenProgress.md). The requirements are in [SvgTargetRequirements.md](SvgTargetRequirements.md). The design is in [SvgExportDesign.md](SvgExportDesign.md).

## Where the work lives

| Branch | Contents |
| --- | --- |
| `feature/svg-target` | The SVG work. Rooted at `upstream/master` (C-8). It contains `upstream/master` up to d48f445 (Nov 13), the `fix/` branches that the work needs, and `feature/skia-fixed-font-set`. Its tip is in `dev`. |
| `feature/svg-<name>` | One branch for each phase and task, rooted at `feature/svg-target` and merged into it. Nothing depends on the finished ones. |
| `svg/<name>`, `svg-results/<name>` | The SVG loop of a feature branch: `base` with the branch merged in, made again for each run (DL-1), and the results of the latest run (DL-7). `svg/svg-target` and `svg-results/svg-target` are for `feature/svg-target`. |
| `feature/svg-subsetter`, `svg/svg-subsetter`, `svg/svg-text`, `svg-results/svg-text` | Keep these. They are the only references to commits that the gallery and this file cite, for example 4ce1f7c, 89e5a07 and 174dbbe (the text's history before dec839c, Oct 24). |
| `svg-gallery` | The gallery at <https://pauliojanpera.github.io/NotPurplePen/svg/>: its page, its fonts and the drawings of each entry, as one commit that each update replaces. Each publish of the site copies it into `svg/`. |

## How to continue the work

- **Environment.** Linux with the .NET 10 SDK, PowerShell, Node with Playwright, and Python 3 with NumPy, SciPy and Pillow.
  - In a cloud session, install the SDK with `dotnet-install.sh --channel 10.0 --azure-feed https://dotnetcli.blob.core.windows.net/dotnet`.
  - Build `src/MapModel/Map_Svg.Tests`. Then install Playwright's pinned Chromium with `bin/Debug/net10.0/playwright.ps1 install --only-shell chromium` (PowerShell from its GitHub release). If GitHub's releases are not available, install PowerShell as the .NET tool `PowerShell` from NuGet.
  - Without PowerShell, the driver of the Playwright package installs the browser: `bin/Debug/net10.0/.playwright/node/linux-x64/node bin/Debug/net10.0/.playwright/package/cli.js install --only-shell chromium`. With `chromium firefox webkit`, it installs the browsers of the browser check (V-6). On a machine without WebKit's system libraries, run `install-deps webkit` one time first.
  - The necessary hosts are api.nuget.org, dotnetcli.blob.core.windows.net, cdn.playwright.dev and storage.googleapis.com.
  - A sparse checkout of these paths builds and runs the SVG tests: `src/DotSpatialProjectionsLite`, `src/MapModel/{Graphics2D,MapModel,Map_Skia.Tests,Map_SkiaStd,Map_Svg,Map_Svg.Tests,RenderingTestCases}`, `src/MapModel/TestFiles/{pdfrender,rendering,skia_render,svg_reference}` and `src/Testing`. Work on the application also needs `src/PurplePenCore`, `src/AvPurplePen`, `src/PurplePenViewModels` and their tests.
  - AvPurplePen does not build in a clone that has `extensions.worktreeConfig` set, because GitVersion 5.12 cannot open such a repository. `git sparse-checkout` sets it in the configuration that all worktrees of the clone share. `SvgDevLoop/app-checkout.sh` makes a separate clone without it. AvPurplePen builds there in less than 2 minutes.
- **Scripts.** `SvgDevLoop/` has a script for each step of the loop: a local run and its triage, staging without the experiment lines, the loop branch, waiting for a run, getting a run's artifacts and images, rendering cases as the tests do, recording from a run's artifact the baselines that a fix moves, and following an agent. Its README shows how to change the target and how to offer a fix to Purple Pen.
- **Local run.** Run `SvgDevLoop/run-svg-tests.sh`, or `dotnet test` on `src/MapModel/Map_Svg.Tests` with `SVG_LOOP_RESULTS` set to a results directory. The run makes the loop's measurements, documents and reference images byte for byte. It takes approximately four minutes. The browser check takes approximately four minutes more.
- **Loop run.**
  - `SvgDevLoop/push-svg-loop.sh [--with-experiments] <worktree>` makes `svg/<name>` again for a feature branch and pushes it. `SvgDevLoop/wait-run.sh` waits for the run.
  - To start the loop by hand on a feature branch: `gh workflow run svg.yml -R pauliojanpera/NotPurplePen --ref dev -f ref=feature/svg-target`. The runner merges `base` into the branch. The check run is attached to the branch's tip. `gh run list --workflow=svg.yml --event=workflow_dispatch --limit 1` gives the run's id. Wait for the run in the background. Do not ask the API again and again.
  - To run the loop with a change to the workflow or its scripts, make `svg/svg-target` again as one merge commit of the `ci/` branch (or `origin/base`) and `origin/feature/svg-target` (`git merge-tree --write-tree`, then `git commit-tree`). Push it with `--force-with-lease`.
  - Read first "Since the previous run" in the summary, and `changes.md`. They show what changed from the results that the branch had before.
  - On `svg-results/<name>`, read `summary.md`, `failures.md` and `pending.tsv`. `measurements/`, `references/`, `differences/` and `renderings/` hold the numbers and images of the comparison. `geometry/` holds the geometry check (V-10), `browsers/` the browser check (V-6), and `course-pages/` the Skia drawing of each exported page that is outside the tolerance. The artifact `svg-loop-renderings-<name>` keeps all images (DL-9).
  - A run can pass its tests and fail in "Publish the results", when GitHub answers the push with an Internal Server Error. The artifact `svg-loop-<name>` keeps the results. Run the failed job again to publish them.
- **New references.** A case that becomes drawable fails one time, because it has no reference. The run puts the reference in `references/`. Make sure that the case's comparison is within the tolerance. Copy the image into `src/MapModel/TestFiles/svg_reference` (`git add --sparse` in a sparse checkout). Commit it in its own commit.
- **Gallery.** Publish when there is something new to show: approximately every 30 minutes of work, and at the end of each step.
  - Clone `svg-gallery`. Check out `svg-results/<name>` of the run to show.
  - `SvgDevLoop/fetch-renderings.sh <run id> <checkout>` copies the run's images from its artifact into the checkout: Chromium's image of each comparison, each exported page as the application draws it (in `course-pages/`), and the browsers' images (in `browsers/`). For a run before Nov 7, which kept only the images of failures, `SvgDevLoop/render-cases.sh` renders the cases as the tests do.
  - Run `SvgGallery/make-gallery-step.py` on the checkout and on the `svg_reference` of the commit that the loop ran. Give the entry's name and the cases to show. For exported pages, give `course-pages` as the reference folder. The script writes the entry's folder and prints the figures and the census for `LOG` and `STANDING` in `index.html`. The census counts separately, under the applicable requirement, the drawings that wait only on a call that the target never honours, for example a line of zero width (D-5). A case that matches its reference exactly has no window of most difference. Choose its detail by hand.
  - Update the course's states. Look at the page with `SvgGallery/look-at-gallery.mjs`.
  - Publish with `SvgGallery/publish-svg-gallery.sh`. It replaces the commit of `svg-gallery` and puts the gallery under `svg/` on `gh-pages`.
- **Research scripts.** `SvgTolerance/` holds the scripts that chose the pixel tolerance. Run them again when new cases come. `SvgGeoreference/` holds the checks of the EPSG codes of OCAD's grids (G-3).

## Status

| Phase | Status |
| --- | --- |
| 1, the development loop | Done: `.github/workflows/svg.yml` on `base`. It reports what changed since the previous run. It runs on demand for any branch. |
| 2, drawing, structure, layout, colour and georeferencing | Done. |
| 3, the export in the application and the course data | Done (d3561c1, Oct 29). Gallery entry `p3-export`. |
| 4, text and bitmaps | Done: bitmaps (d58cb9e), text (b091333, Oct 24). |
| The first release | Phases 1 to 4 in `dev` (1dca01f, Oct 30). |
| 5, several pages in one file, and area patterns as pattern fills | Done: pages (a415dc1, Nov 4; gallery entry `p5-pages`), and area patterns and hatchings as pattern fills (4e17b34, Nov 6; own gallery entry). |
| 6, symbol reuse and compression | Done (4764b57, Nov 7): S-5 and S-10. |
| 7, Firefox and WebKit | Done (267efc8, Nov 12; gallery entry `p7-browsers`). Firefox draws 202 of 205 documents as Chromium does, WebKit 193. The others are set apart with their reasons (design: "Firefox and WebKit"). |
| 8, the TypeScript port | Not started. It will be in its own repository. |

Other work that is done. Each is in `feature/svg-target` and `dev`, if not stated differently. The numbered tasks are in [NotPurplePenProgress.md](NotPurplePenProgress.md), "Done".
- **Colours that Scribus and Inkscape read** (Nov 4 and 5; S-4, COL-1, COL-3). Each style is a `style` attribute on the outermost group whose elements share it. A device-CMYK profile is behind `cmyk` (9fe17bd, 010704a). `SvgColourReaders/` has the readers.
- **Course data in .ppen's terms and ids** (f684787, CD-1), and **the deprecated EPSG codes** of four OCAD grids (f8b60f9, G-3), Nov 6.
- **The loop's checks** (Nov 7): all renderings in the run's artifact, the calls that the target never honours set apart (b67748d), and the geometry check (83e53fa, V-10). The loop's report of them is on `ci/svg-loop-improvements` (5ffb4bb).
- **Nov 11's items** (1b0e705 to cc462aa): lines of zero width left out (D-5), F-3 without its list of elements, bitmaps below full intensity drawn through a filter (B-6), clipped miter joins drawn as miters, and the reasons for M-8, C-6, the compactness ranking and Chromium written into the requirements.

The last SVG loop of the work in `feature/svg-target` (run 43563468407, Nov 14): 868 passed, 0 failed, 2 pending, 16 documents set apart for a browser. The two pending cases, clouds and clouds11, draw lines of zero width. The target leaves such lines out (D-5), so these cases stay pending.

The target fully implements these calls (DL-5): `Intensity`, `PushTransform`, `PopTransform`, `PushClip`, `PopClip`, `PushAntiAliasing`, `PopAntiAliasing`, `PushBlending`, `PopBlending`, `CreatePath`, `CreateSolidBrush`, `SupportsPatternBrushes`, `CreatePatternBrush`, `CreatePen`, `CreateFont`, `HasPath`, `HasBrush`, `HasPen`, `HasFont`, `DrawLine`, `DrawArc`, `DrawEllipse`, `FillEllipse`, `DrawRectangle`, `FillRectangle`, `DrawPolygon`, `DrawPolyline`, `FillPolygon`, `DrawPath`, `FillPath`, `DrawText`, `DrawTextOutline`, `DrawBitmap`, `DrawBitmapPart`.

**Left open:**
- In the SVG tests, the families that the test maps name are drawn in Arial, except Arial, Times New Roman, Cambria, Calibri and Roboto: for example Georgia and Arial Narrow. Fonts with the same metrics, for example Gelasio and Liberation Sans Narrow, could replace them.
- The gallery's standing figures show 784 passed. The loop has 871 since task 7.

## Decisions

### The export's output

- **Course data in .ppen's terms** (Nov 4). The course data maps to the .ppen format and is not nested: .ppen's element names and control kinds, and the .ppen id of each course and control. `npp:object` stays, because .ppen records nothing of what is drawn. Nested .ppen course data would carry each part and relay fork of a course into the file of each runner.
- **Parts by reference** (Nov 4). The parts stay in `<defs>` and the pages show them through `<use>`, also a part that is used one time. Inkscape shows each `<use>` as a clone. Edit › Clone › Unlink Clone changes the clone into a group whose objects can be selected.
- **Phase 5's choices** (Nov 4). `AddPage` takes the page's name as its last parameter. F-3 names `sodipodi:namedview` with its `inkscape:page` elements. Inkscape reads pages only from them. The default file choice is that of PDF export: one file for each course.
- **Colours for Scribus and Inkscape** (Nov 4 and 5). Each style is a `style` attribute, not a class of the `<style>` sheet, which Scribus 1.6 ignores. A device-CMYK ICC profile of 516 bytes is embedded under the name `cmyk`. Without it, Inkscape shows no CMYK channels. A shared style goes on the outermost group whose elements share it. Inkscape then shows a colour on the group of a symbol. A paint keeps its two declarations, sRGB first, so Chromium draws as before. Map-sized exports became 2.5 to 14 % smaller. A profile sampled from Purple Pen's SWOP table would bring Inkscape's preview within approximately 4.5 levels of Purple Pen's colours, instead of approximately 35. But it comes from Adobe's profile, whose licence forbids changes.
- **Deprecated EPSG codes** (Nov 4). Data published with a code keeps the code. Deprecated codes are supported. Grids 50050, 66044, 66045 and 66098 name the deprecated codes whose parameters they follow: 26979, 2244, 2245 and 3454. Their Proj4 strings stay as they are.
- **Area patterns** (Nov 4 and 6). The export writes the regular patterns of area symbols as `<pattern>` fills, as Purple Pen draws them on screen. Hatchings are pattern fills too. This is a conscious difference from Purple Pen's PDF export, which draws each pattern element by element. Irregular patterns, and patterns not in clip mode, are drawn element by element, because a fill cannot draw them the same. An option draws all area patterns element by element, and hatchings as lines, for readers that cannot fill an area with a pattern. Pattern fills are the default.
- **Adaptations for readers** (Nov 6). If a reader needs special handling, the export gives it as a specific option. The option's name tells what it does, never the name of the reader.
- **Path merging** (Nov 6). To merge paths is an optimization for the map's design, not for the export, and never a hidden step. S-6 left the requirements. Phase 6 is S-5 and S-10. S-5 stays, because each object keeps its own element and only the drawing of a symbol is shared.
- **The pages that show a course** (Nov 6; CD-1). The record of each map names in `views` the views of the pages that show the map. A course opens in Chrome as `map.svg#A-rata`, through the view of its page (L-6). Chromium shows `file.svg#Course_12`, opened by itself, as that course's page. One file holds all courses if the file choice is all pages in one file.
- **The names of a course's alternatives** (Nov 6). The page of a variation is named with a space before its variation, "H21 AB", as eResults names a class's course alternative. To match the two is a task for the orienteering-tools side. NotPurplePen's names stay. Its segmentation, `splitERCourseName` in `pauliojanpera/orienteering-tools` (`src/lib/class-alternative-name-parts.ts`), splits a name at its one space before an uppercase `[A-Z]+` tag. It does not split the page name of a relay leg, "H21 3.2" (team and leg, from `VariationInfo.Name`), or the name of a variation's part, "H21 AB-2".
- **Lines of zero width** (Nov 7 and 11). The export ignores a line that is drawn with a pen of zero width. It does not stop with an error. ISOM 2017 has no such line. Skia draws it as a hairline of one device pixel, PDF export as the printer's thinnest line. The case punchboxbug stays compared: it makes a pen of zero width but draws no line with it. Only clouds and clouds11 stay apart.
- **F-3's list of elements** (Nov 7). The whole list is dropped. It came from the first draft of the requirements (22e63d2, Oct 21), not from a test of readers. F-3 keeps: well-formed XML, references through `xlink:href`, identity in `data-` attributes, and the tables in the namespace of the metadata.
- **Bitmaps drawn lighter** (Nov 7). A bitmap at an intensity below 1 is drawn through a filter on its image (`feComponentTransfer`). The file keeps the original bitmap. Browsers and Inkscape draw it lighter. Scribus can show it at full strength.
- **Clipped miter joins** (Nov 7). `MiterClipped` is drawn as a miter, as PDF export draws it. A corner sharper than the miter limit is bevelled, not clipped. Nothing in the map code or in Purple Pen uses this join now.
- **Editor metadata** (Nov 7). The phrase is dropped from S-9. It meant the saved state of an editor (window, zoom, guides, current layer, version). The export has none. Inkscape's page elements (L-6) are content.
- **Fixes for Firefox and WebKit** (Nov 11). Where Firefox or WebKit draw the export differently because of the SVG, the export changes only if the change does not make the data or Chromium's output worse. The file keeps all its contents, with nothing dropped or approximated. Chromium's drawing stays at least as near to Purple Pen's references. A difference that only such a cost removes is set apart with its reason, and the alternative goes to @Pauli.
- **Safari** (Nov 12). The browser check stays on Linux only, as all of the loop does. Safari's own drawing differences are not checked, as those of Chrome and Firefox on other systems are not. WebKit's SVG code, which Safari shares, is checked on Linux.
- **Patterns for each symbol and colour** (Nov 12). Each symbol and colour keeps its own pattern definition, as S-12 asks. Equal patterns are not defined one time for each file. The base map is drawn one time in each file, so its patterns are written one time. But each course part draws its course as a separate drawing, with new symbols. So a file of several course parts defines the course's patterns one time for each part. Example: in one file, mapexchange1's Course 5 defines the two hatchings of its dangerous area four times, 1,780 bytes more than one time. Equal patterns of different map symbols each keep a definition too.
- **PDF base maps in the TypeScript port** (Nov 12). The port does not need to support PDF base maps. P-6 does not apply to an event whose map is a PDF. PdfPig and the reader's drawing calls do not go into the port.
- **The requirements after the fix of overlapping hatchings** (Nov 13). @Pauli approved the wording that task 2's agent proposed: M-1, D-2, M-6, S-7 and S-12 tell how the pattern of overlapping areas is drawn one time under the clip to their ghosts.
- **Images and determinism (F-2)** (Nov 13). Lossless image data is held to its pixels, not its bytes. A PNG that is compressed to other bytes but decodes to the same pixels meets F-2 and P-6. Skia's bundled zlib can compress differently on a CPU without the CRC32C instruction. Lossy data, JPEG too, is held byte for byte.
- **JPEG data** (Nov 13 and 14). The export writes the original JPEG data of an image where it has the data. Otherwise Skia's encoder encodes the image, with the image's resolution in the JFIF header. B-3 states the rule (wording approved on Nov 14). The design tells when browsers draw the data as the application does. Skia's encoder writes all JPEGs that the application encodes, those of the image export and of the OCAD export too. Baselines that this moves go into the same branch.
- **Chromium and a huge bitmap** (Nov 13). Not fixed. Chromium's headless shell crashed at the screenshot of the A2 map's SVG whose PDF base map is a bitmap of 139 million pixels, in two of task 1's three runs, on a machine with 7 GB. It drew the map's vectors. Firefox and WebKit drew the bitmap. The cause was not examined. It is probably memory.

### Tests and CI

- **The loop's improvements** (Oct 23). @Pauli agreed to all of them, fonts in the repository too, in the order that the executor chooses. The comparison stays with Skia: a PDF rendered to pixels adds a third rasterizer and checks what the Skia references check. Vector checks go where pixels cannot see: where Chromium puts each character, the geometry of each SVG against its drawing calls (V-10), and the CMYK values that the exports write.
- **Upstream's glyph positions** (Oct 24). The SVG does not copy upstream's typesetting bugs (design: "Glyph positions in the Skia target").
- **Fonts in the repository** (Oct 24). For determinism, the tests use fonts in the repository. Make reusable scripts for the loop where reasonably possible (`SvgDevLoop/`).
- **Fonts and determinism** (Oct 24). A document whose fonts are embedded never uses a font of the viewer's system.
- **The fixed font set** (Oct 29). The fonts are fixed for testing. The application has no such preference: in typical use, users do not care.
- **F-6 without a check by hand** (Nov 13). F-6 needs no check by hand. Inkscape and Scribus run where a session works. The readers in `SvgColourReaders/` open the exports in both and read the CMYK values of each colour.

### The SVG dialog

- **Task 4's choices** (Nov 13).
  - Kurmanji's and Venetian's drafts ship as the agent wrote them.
  - The SVG strings come out of the five files that translate almost nothing (ca, he, id, ru, zh-HK), by reverting 60e28d5a.
  - The bitmaps combo has its own row, approximately 228 pixels wide, as the multi-page combo has. The translations have their full wording where it fits.
  - The folder picker of the SVG dialog has its own title, "Select a folder for the SVG files", translated as the title of each language's PDF picker.
  - The four settings that the SVG export adds (the bitmaps combo, JPEG quality, compress as .svgz, draw area patterns as separate shapes) are under one heading, "Special options". The Appearance group then agrees with that of the PDF dialog. Each label states its full meaning without the heading.
  - The dialog is 662 pixels tall. So the area-patterns check box keeps its full wording on two lines in bg, es, fr, ja and pt.
- **The older cut-offs of the export dialogs** (Nov 13). Two cut-offs that the SVG dialog shares with the PDF dialog stay. They are older than the SVG work:
  - the Files combo's "One per course" in bg, es and pl;
  - with "Other folder" chosen, the path box and the "Select folder..." button go below the Folder group in 22 of 33 languages. The button is fully hidden in bg, es, fr, ja and pt. The window can be made larger by hand.
