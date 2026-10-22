# SVG Graphics Target — Progress

Oct 22, 2026 · kept current by the sessions doing the work

Where the work on the SVG target stands against the phases of [its requirements](SvgTargetRequirements.md): what is done, what was decided and why, what waits on @Pauli, and how a session picks the work up. A session updates this file whenever a step ends or a decision is taken.

## Where the work lives

| Branch | Holds |
| --- | --- |
| `docs/svg-target-requirements` | The requirements and this file |
| `feature/svg-target` | The work, rooted at `upstream/master` (C-8), with the three `fix/` branches below merged in |
| `svg/svg-target` | `base` with `feature/svg-target` merged in, rebuilt for each loop run (DL-1) |
| `svg-results/svg-target` | The results of the latest loop run (DL-7) |
| `svg-gallery` | The gallery served at <https://pauliojanpera.github.io/NotPurplePen/svg/>: its page, fonts and each step's drawings, as one commit that each update replaces. Every publish of the site copies it into `svg/`. |
| `fix/backslashes-in-test-file-paths`, `fix/culture-independent-descriptor-parsing`, `fix/case-of-test-file-names` | Changes the tests need on Linux, each to be offered to Purple Pen on its own |
| `experiment/svg-target`, `experiment/machine-record`, `svg/diagnose-recording`, `svg-results/diagnose-recording`, `diag/segoe-ui-faces` | Throwaway runs; nothing depends on them |

The SVG loop, `.github/workflows/svg.yml`, and the machine check of the Windows tests are on `base`.

## How to pick the work up

- **Loop run.** Rebuild `svg/svg-target` as one merge commit of `origin/base` and `origin/feature/svg-target` (`git merge-tree --write-tree`, then `git commit-tree`), push it with `--force-with-lease`, follow the run through the GitHub API, and read `summary.md`, `failures.md` and `pending.tsv` on `svg-results/svg-target`. Its `measurements/`, `references/`, `differences/` and `renderings/` hold the numbers and images of the comparison.
- **Local run.** In a cloud session, install the .NET 10 SDK with `dotnet-install.sh --channel 10.0 --azure-feed https://dotnetcli.blob.core.windows.net/dotnet`, build `src/MapModel/Map_Svg.Tests`, install Playwright's pinned Chromium with `bin/Debug/net10.0/playwright.ps1 install --only-shell chromium` (PowerShell from its GitHub release), and run `dotnet test` with `SVG_LOOP_RESULTS` naming a results directory. The run reproduces the loop's measurements and reference images byte for byte. The environment's proxy lets through the hosts this needs: api.nuget.org, dotnetcli.blob.core.windows.net, cdn.playwright.dev and storage.googleapis.com.
- **Gallery.** When a step ends, add it to the gallery. Clone `svg-gallery`, and run `SvgGallery/make-gallery-step.py` beside this file on a checkout of `svg-results/svg-target`, naming the step and the cases to show: it writes the step's folder and prints the figures and census that `index.html` takes into `LOG` and `STANDING`. Move the course's states on, look at the page with `SvgGallery/look-at-gallery.mjs`, and publish it with `SvgGallery/publish-svg-gallery.sh`, which replaces `svg-gallery`'s commit and puts the gallery under `svg/` on `gh-pages`. A `wip/` branch made before `build.yml` copied `svg-gallery` into the site publishes without it, until the next publish from `base`, `dev` or a newer branch.
- **Full run.** A change is finished only after a `wip/` branch rooted at `base`, with the feature merged in, has run the Windows suite (DL-10). That suite stops on a machine whose Windows version or system fonts differ from `.github/ci/environment.json`; a run started by hand with `machine_check` set to `report` tests anyway, and the stopped job's log prints the record that machine would need.

## Status

| Phase | Status |
| --- | --- |
| 1, the development loop | Done. The dispatch mode of `svg.yml` is untried until the workflow reaches `dev`, the default branch. |
| 2, drawing, structure, layout, color and georeferencing | In progress, in the four steps below |
| 3 to 8 | Not started |

| Step of phase 2 | Status |
| --- | --- |
| 1. The document, its layout and its writers (L-1 to L-5, F-2 to F-4, COL-1 to COL-6, S-1 to S-4, S-8, S-9, S-11) | Code on `feature/svg-target` (efa6995). Loop run 39744433244: 74 passed, 0 failed, 188 pending. The first comparable cases, FillAreas, irregular1, irregular2 and irregular4, match their recorded references within the pixel tolerance. |
| 2. The drawing operations (D-1 to D-12) | Next |
| 3. The map structure (M-1 to M-8) | To come |
| 4. Georeferencing (G-1 to G-3), then the full `wip/` run, with F-6 checked by hand | To come |

The calls the target fully implements, in the sense of DL-5: `Intensity`, `PushTransform`, `PopTransform`, `PushAntiAliasing`, `PopAntiAliasing`, `CreateSolidBrush`, `FillEllipse`, `FillPolygon`, `FillRectangle`, `HasPath`, `HasBrush`, `HasPen`, `HasFont`.

## Decisions

- **The structured interface.** `IStructuredGraphicsTarget`, in `Graphics2D`, extends `IGraphicsTarget`. Its layout calls exist: `BeginPart` and `EndPart` around a part of a drawing, which the document defines once; `BeginMap` and `EndMap` around a labeled map, nested for a template; and `AddPage(paperSize, mapArea, mapToPage, partKeys)`, made outside parts. Step 3 adds `BeginColorRun(colorKey, position, name, color, overprint)` with `EndColorRun`, and `BeginObject(objectKey, symbolKey, symbolId, symbolName)` with `EndObject`; step 4 adds `SetGeoreference(mapKey, MapGeoreference)`. @Pauli approved these calls.
- **Shared code.** `IdentityComparer` lives in `Graphics2D`, from where every graphics target, the SVG target too, keys its resources by identity (C-4, C-6).
- **The document.** Parts are groups with ids in `<defs>`. A page is a group carrying the page's transform and a clip of its map area, with a `<use>` for each part it shows. Pages after the first lie to its right, 10 mm apart, and the root has the first page's size in millimetres. The metadata's root is `npp:document version="1"`, holding an `npp:map` with the number and label of each map, whose groups carry `data-m`.
- **Numbers.** Coordinates are whole steps of 0.001 mm, rounded half up as `floor(x + 0.5)`, which every runtime does alike (P-6). A transform carries the digits that the reach of its content needs: within half a step for its translation, and within 0.45 of a step for its linear part, shared between angle (0.2), scale (0.2) and shear (0.05).
- **The text.** Each path command takes its shortest form; arc flags keep their separators, which SVG 1.1 readers need, and transform functions follow each other without one. Classes and ids are named in the order they are first used, the shortest names first. Groups are tidied before writing: empty ones dropped, those carrying nothing dissolved, an only child folded into its parent where the order of transform and clip allows, and adjacent groups with equal attributes merged, unless they blend.
- **Colors.** `icc-color()` names the profile `cmyk`, declared as `<color-profile name="cmyk"/>` with no ICC profile behind it. The sRGB comes from the converter passed in; the comparison passes `DefaultColorConverter`, which converts as the Skia references do.
- **Anti-aliasing.** Browsers anti-alias by default, so a group carries `shape-rendering` only where the mode changes.
- **Errors.** A call the target does not implement yet raises `Svg_UnsupportedCallException`, which makes a comparison case pending; a call out of turn raises an `InvalidOperationException` that names it.
- **The comparison.** Each case is one map in one part on one page whose millimetres are the image's pixels, and Chromium shows the SVG through an HTML page that sizes it to the image. Reference images are rendered by the Skia target from the case's own drawing, anti-aliased on white (V-9); a missing one goes to the results' `references/`, from where it is committed into `src/MapModel/TestFiles/svg_reference`.
- **The pixel tolerance (F-1).** A pixel of Chromium's image passes when each color channel lies within the range that channel takes in the reference within one pixel, give or take 4 levels, and the same holds the other way round; beyond the image both count as white paper, which keeps shapes cut by the image's edge fair. Inside a shape this asks for a match within 4 levels; along an edge it lets any shade between the colors meeting there pass, which is where Chromium's Skia and SkiaSharp's differ, by up to 108 levels in the first cases while every shape lies within 0.15 pixel of its place. Those cases pass with no pixel beyond a slack of 2, and fail it shifted by two pixels, with a feature removed, with features two pixels wider or with an ink six levels redder. `PixelTolerance` holds the rule, so that a case can carry a tolerance of its own, as V-3 foresees for fine evenly spaced lines; the check of Chromium's rendering on whole pixels keeps an exact one.

## Waiting on @Pauli

- **F-6**, opening the output in Inkscape and Scribus, by hand at the end of phase 2.

## Next

1. Leave default-valued attributes out (S-9): every dot of the irregular maps is written `<circle class="a" cx="0" cy="0" r="100"/>`, under a group that only translates it.
2. Step 2, the drawing operations, in the order of the census on `svg-results/svg-target`.
3. Add each step to the gallery when it ends.
