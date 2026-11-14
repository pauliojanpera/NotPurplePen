# SVG Graphics Target — Progress

Nov 14, 2026. The sessions that do the work keep this file current.

This file is the handover of the SVG target work. It tells where the work is, how to continue it, its status, the tasks, the decisions, and what waits on @Pauli. The requirements are in [SvgTargetRequirements.md](SvgTargetRequirements.md). The design is in [SvgExportDesign.md](SvgExportDesign.md). An executor can continue the work from this file and the branches only. Nothing that a session keeps outside GitHub is necessary.

Update this file when a step ends, when a task changes hands, and when @Pauli makes a decision. This file is a handover, not a journal. The commits, the runs and the session transcripts record how each step went (see "Session transcripts").

## Where the work lives

| Branch | Contents |
| --- | --- |
| `docs/svg-target-requirements` | The requirements, the design, this file, CLAUDE.md (the practice of the sessions that do the work), and the tools: `SvgDevLoop/`, `SvgGallery/`, `SvgTolerance/`, `SvgGeoreference/`, `SvgColourReaders/`, `SvgPdfVectors/`, `SvgHatchingOverlap/` and `SvgTranslations/`. |
| `feature/svg-target` | The SVG work. Rooted at `upstream/master` (C-8). It contains `upstream/master` up to d48f445 (Nov 13), the `fix/` branches that the work needs, and `feature/skia-fixed-font-set`. Its tip is in `dev`. |
| `feature/svg-<name>` | One branch for each phase and task, rooted at `feature/svg-target` and merged into it. Nothing depends on the finished ones. |
| `svg/<name>`, `svg-results/<name>` | The SVG loop of a feature branch: `base` with the branch merged in, made again for each run (DL-1), and the results of the latest run (DL-7). `svg/svg-target` and `svg-results/svg-target` are for `feature/svg-target`. |
| `wip/linux/<name>` | Full runs (DL-10). For a fix alone: `base` with the fix merged in. For the SVG work and all other work that has `fix/` branches: `basewithfixes` with the work merged in (@Pauli, Nov 13). `wip/linux/svg-target-into-dev` is the merge of `feature/svg-target` into `dev`. Make it again on top of `dev` for each merge. Run it in full before you fast-forward `dev` to it. |
| `base`, `basewithfixes`, `dev` | `base`: upstream, with what is necessary to build and test it here. `basewithfixes`: `base` with the `fix/` branches that change the application. It publishes the channel "base" ("Upstream with this fork's build and bug fixes"). `dev`: `basewithfixes` with the fork's features. The SVG loop (`.github/workflows/svg.yml`) and the machine check of the Windows tests are on `base`. |
| `ci/<name>` | Changes to the workflows. Each is rooted at `base` and merged into `base`, `basewithfixes` and `dev` through full runs. |
| `fix/<name>` | Fixes of upstream's code. Each is rooted where `base` meets upstream, and has only its own commits. See "Ready to offer to Purple Pen". |
| `feature/svg-subsetter`, `svg/svg-subsetter`, `svg/svg-text`, `svg-results/svg-text` | Keep these. They are the only references to commits that the gallery and this file cite, for example 4ce1f7c, 89e5a07 and 174dbbe (the text's history before dec839c, Oct 24). |
| `svg-gallery` | The gallery at <https://pauliojanpera.github.io/NotPurplePen/svg/>: its page, its fonts and the drawings of each entry, as one commit that each update replaces. Each publish of the site copies it into `svg/`. |
| `evidence/`, `experiment/`, `diag/`, `archive/`, `proto/` | Evidence and probes of finished investigations, throwaway runs, and old work-in-progress history. Nothing depends on them. |

This machine cannot delete branches. The git gateway refuses a push that deletes a branch (HTTP 403). The proxy refuses the API's deletion. "Waiting on @Pauli" gives the command for the branches to delete.

## How to continue the work

- **When @Pauli writes.** Answer first. Do nothing else until you give the answer: no tool call, no agent, no edit. Then do only what he asked (@Pauli, Oct 30).
- **Questions for @Pauli.** Do not leave a question or proposal that @Pauli must decide only in the documents. Ask him in an interview: one question at a time, with choices and a recommendation. If a question is too long for the dialog, ask it in the chat (@Pauli, Nov 13). Ask when he is present: when a step ends, or before the work that depends on the answer (@Pauli, Nov 4). Record the question under "Waiting on @Pauli" and the answer under "Decisions".
- **Language of the documents.** Write in Simplified Technical English: short sentences, one fact in each, exact terms (@Pauli, Nov 14).
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
- **Full run.** A change is finished only after a `wip/` branch with the change merged in has run the Windows suite (DL-10). Root the branch at `basewithfixes` for the SVG work and for all other work that has `fix/` branches. Root it at `base` for a fix alone (@Pauli, Nov 13). Name it `wip/linux/<name>` to make only the Linux package.
  - `.github/ci/environment.json` records the test machine's Windows version and system fonts, Segoe UI 5.72 among them. @Pauli settled that they must agree. GitHub's windows-11-arm runners use several images. A machine that does not agree with the record gets a warning and is tested. Its job's log prints that machine's record. That record replaces the file when baselines are recorded on that machine. A run started by hand with `machine_check` set to `stop` stops there.
  - A branch's packages are published when the newest Tests run of the commit has passed. After a failure, the publish waits up to 40 minutes for a new run.
- **Windows-only tests on Linux.** `Test_MapModel` and `PurplePen_Tests` target Windows. To run one test file of theirs on Linux, make a stand-in project:
  - Give it the suite's assembly name, so that the internals of `Test_MapModel` stay visible. Compile only that file, with `TEST` defined.
  - Reference the necessary projects, and the Linux native packages `SkiaSharp.NativeAssets.Linux` and `HarfBuzzSharp.NativeAssets.Linux`.
  - Register Skia's services as `GlobalTestSetup` does. Put a `TestFiles` link beside the project, for `TestUtil`.

  Use it to check a test before the Windows suite runs it.
- **Agents.** One session does the work. It gives each item to one agent at a time, at effort max (@Pauli, Oct 23 and 30). Each new task, or group of related tasks, gets a new agent, with an empty context and its own brief. Only the continuation of an agent's task, for example after a limit or a restart, goes back to the same agent (@Pauli, Nov 12 and 13). Agents also do the merges into the shared branches `base`, `basewithfixes`, `dev` and `feature/svg-target` (@Pauli, Nov 5). The session writes the briefs, asks @Pauli the questions and keeps this file.
  - Before you start an agent, run `SvgDevLoop/agent-cache.py set` (CLAUDE.md). Agents work without rests (@Pauli, Nov 13).
  - Follow the agent's commits, pushes and runs with `SvgDevLoop/watch-agent.py`, and its steps with `dump-titles.py`. Report them to @Pauli at headline level.
  - An agent in the session's background stops without notice when the session's turn is interrupted. An agent's 5-hour limit stops it until the limit resets. Then resume it.
- **Session transcripts.** A session can end at any time, when its tokens end or its machine restarts. So the machine's transcripts are archived on the branch `sessions` of the private repository `pauliojanpera/windows-fonts`. Attach it with push access. Its README tells how to search the transcripts.
  - The session keeps a background command (`run_in_background`) that runs `archive-if-due.sh` every minute. Start it again when it ends after two hours. The script archives all transcripts of the machine when the last archive is 10 minutes old. A git hook, or a process that a command leaves running, does not work: such a process ends with the command.
  - Only a transcript that extends an archived file replaces it. A transcript rewritten with less content goes beside it as a revision.
  - Claude Code keeps a session's transcript in `~/.claude/projects/<project>/<session id>.jsonl`. An executor that cannot read its own transcript writes a journal of the session instead.
  - If the permission check needs a rule for the push, use a wrapper script that sets `ARCHIVE_TRAILERS` and runs the script. Allow only the wrapper, with a Bash rule in `~/.claude/settings.json`.
  - Search the transcripts to find what an executor did or tried. Do not read them through. This file stays the handover.
- **After a restart.** A restart of the machine can remove its clones, worktrees, scratchpad and tools, and with them all work that was not pushed. It can also keep the disk. The `sessions` README gives the steps in order: attach and clone again, start the archiving task again, archive immediately, find what was lost against the restored transcript, and make the uncommitted work again with `replay-edits.py`, which replays the transcript's edits onto the commit where the work started. Then run `SvgDevLoop/agent-cache.py set` again before you start an agent. Push the work in progress at each step to a branch whose push runs nothing, for example a `feature/` branch. Then little needs to be made again.
- **Fixes for Purple Pen.** When the SVG target needs a fix of a bug in upstream's code, make the fix immediately on a `fix/` branch, rooted where `base` meets upstream (`git merge-base origin/base upstream/master`, with `upstream` fetched from petergolde/PurplePen), with the baselines that it moves. Run it on a `wip/linux/<name>` branch rooted at `base`. Merge it into the feature that needs it. Add it to "Ready to offer to Purple Pen". Rebase it onto the newest `upstream/master` only when you offer it.
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

Other work that is done. Each is in `feature/svg-target` and `dev`, if not stated differently:
- **Colours that Scribus and Inkscape read** (Nov 4 and 5; S-4, COL-1, COL-3). Each style is a `style` attribute on the outermost group whose elements share it. A device-CMYK profile is behind `cmyk` (9fe17bd, 010704a). `SvgColourReaders/` has the readers.
- **Course data in .ppen's terms and ids** (f684787, CD-1), and **the deprecated EPSG codes** of four OCAD grids (f8b60f9, G-3), Nov 6.
- **The loop's checks** (Nov 7): all renderings in the run's artifact, the calls that the target never honours set apart (b67748d), and the geometry check (83e53fa, V-10). The loop's report of them is on `ci/svg-loop-improvements` (5ffb4bb).
- **Nov 11's items** (1b0e705 to cc462aa): lines of zero width left out (D-5), F-3 without its list of elements, bitmaps below full intensity drawn through a filter (B-6), clipped miter joins drawn as miters, and the reasons for M-8, C-6, the compactness ranking and Chromium written into the requirements.
- **The fork's own work.** The colour model of the PDF dialog (`fix/pdf-dialog-color-model`, in `basewithfixes` and `dev`). The site's page of builds shows the commit of each build (`ci/site-build-commits`). The font install no longer lets the Skia text cases draw Arial Narrow as Arial (`ci/installed-fonts-visible`, Nov 5; see Decisions, "The fonts that the tests install").
- **Task 1, a PDF base map as vectors** (Nov 12): researched. Design: "A PDF base map as vectors". Probes: `SvgPdfVectors/`. Its `run-all.sh` makes all figures again in approximately 36 minutes (13dd320). @Pauli chose option (e). Building it waits under "Future directions".
- **Task 2, overlapping hatchings** (Nov 12 and 13). The map code draws a pattern one time for the areas of one symbol that overlap or touch, in all outputs. Design: "Overlapping patterns, and a colour's layer". @Pauli's case: `SvgHatchingOverlap/`.
  - `fix/overlapping-patterns` (c544ed87 to 7d2699b0; Tests 43494132427). The union comes from the outlines and holes of the areas. An area whose outline and holes cross or touch each other or themselves stays out of the groups, and draws its own pattern. 79 baselines moved: GDI+ 7, PDF 11, PurplePen_Tests 51, Skia 8, WPF 2. It is in `basewithfixes` (ef5cd9cd, Tests 43498069194). `IGraphicsTarget` did not change: the union is one path, filled with the nonzero rule.
  - `feature/svg-patterns-once` (f800a9ee to 6ae45334). The SVG writes the outline of each area as a ghost, and the pattern one time under the clip to the ghosts. With "Draw area patterns as separate shapes", each area keeps its own pattern.
  - In `feature/svg-target` (d1ceee4c) and `dev` (6e16e2f4; Tests 43498829019; Build 43500872876 published the packages).
- **Task 3, upstream into the fork** (Nov 13). `upstream/master` up to d48f445 is merged into `base` (68228505), `basewithfixes`, `feature/svg-target` and `dev`, each through its full run. No baseline was recorded. `fix/image-reencode` and `fix/invariant-culture-metric` gave way to upstream's d48f445 and bc4debb. `fix/headless-pdf-loading` keeps only `HeadlessPdfLoadingStatus` and its registration, on top of 1ed9e92 (see Decisions, "Upstream's fixes against ours").
- **Task 4, the strings of the SVG export in all languages** (Nov 13). The strings of the SVG dialog and command (A-7), and the error heading `CannotCreateSvgs`, in the terms of each language's own translations, mostly those of its PDF dialog. The strings are not in ca, he, id, ru and zh-HK, which translate almost nothing (@Pauli). The agent's report, with a review table for each language: [`SvgTranslations.md`](SvgTranslations.md). Then @Pauli's choices (see Decisions, "Task 4's choices"). In `dev` (d3f5d314; Build 43422337880 published the packages).

The last SVG loop of the work in `feature/svg-target` (run 43496141803, Nov 13): 784 passed, 0 failed, 2 pending, 16 documents set apart for a browser. The two pending cases, clouds and clouds11, draw lines of zero width. The target leaves such lines out (D-5), so these cases stay pending.

The target fully implements these calls (DL-5): `Intensity`, `PushTransform`, `PopTransform`, `PushClip`, `PopClip`, `PushAntiAliasing`, `PopAntiAliasing`, `PushBlending`, `PopBlending`, `CreatePath`, `CreateSolidBrush`, `SupportsPatternBrushes`, `CreatePatternBrush`, `CreatePen`, `CreateFont`, `HasPath`, `HasBrush`, `HasPen`, `HasFont`, `DrawLine`, `DrawArc`, `DrawEllipse`, `FillEllipse`, `DrawRectangle`, `FillRectangle`, `DrawPolygon`, `DrawPolyline`, `FillPolygon`, `DrawPath`, `FillPath`, `DrawText`, `DrawTextOutline`, `DrawBitmap`, `DrawBitmapPart`.

## Tasks

**In flight: task 5.** Agent a0aa24c77780ef5a5, since 01:12 Nov 14. It stopped at 02:18 for the end of the quota, and continued at 07:49. An executor without this agent starts a new agent from this note.
- `feature/svg-jpeg-data` (3b5c7b7e): four commits on d1ceee4c (79a17dd3, 03474258, 837da88c, 3b5c7b7e).
  - A Skia bitmap keeps the JPEG data that it was decoded from.
  - A JPEG bitmap that is drawn whole is written as that data, if browsers draw the data as the application does. They do not if the data has a colour profile, or an Exif orientation that is not upright.
  - All other JPEG bitmaps, parts too, are encoded by Skia's encoder, with the image's resolution in the JFIF header. This encoder now writes all JPEGs that the application encodes (@Pauli, Nov 14).
  - The loop's case is the event `src/TestFiles/svg_jpeg/JPEG data.ppen`.
- SVG loop 43563468407 on `svg/svg-jpeg-data`: 868 passed, 0 failed, 2 pending, 16 set apart.
- x64 against ARM64: run 43531515982 on `experiment/svg-jpeg-bytes` (7b1ad1ba). All 84 hashes of decoded pixels and of Skia's JPEG data are the same on seven machines.
- On this branch: the design section "JPEG data (B-3, F-2, P-6)" (00d43888), and B-3 as @Pauli approved it (85b3c291).
- Skia's encoder moved the JPEG baselines of the image export. They are recorded in 1088d129.
- Full run on `wip/linux/svg-jpeg-data` (100366a3): Build 43565924703 and Tests 43565924717 passed.
- `feature/svg-target` is at 1088d129. `dev` is at 2edef4d4, through `wip/linux/svg-target-into-dev` (Build 43566895811, Tests 43566895838). The task ends when the Build of `dev` (43567789260) has published the packages.

**Next, in this order: tasks 8, 9, 6, 7.** @Pauli gave the word for them on Nov 14. Each task gets its own agent (see "Agents"). A task ends with its commits on their branches, its loop green if it changes the target, and the gallery and this file up to date.

6. **The commits of a merge on the releases page** (@Pauli, Nov 13 and 14; see Decisions, "Merge messages").
   - The releases page shows the commit message of a build as its entry (`build.yml`, "Collect the packages"). The message of a merge tells little of what went in.
   - When the commit that a build publishes is a merge, add to its entry the commits that the merge brought, one line each: `git log --no-merges --format='%s (%h)' <commit>^1..<commit>`. Show up to 30 lines, then the count of the others. This applies to all merges, upstream's syncs too, whatever their message.
   - Entries published before the change stay as they are.
   - Make it on a `ci/` branch rooted at `base`. Merge it into `base`, `basewithfixes` and `dev` through full runs.
7. **Multiply as an alternative to darken** (@Pauli, Nov 13 and 14; after tasks 5 and 6; see Decisions, "Blend modes beyond darken" and "Multiply's default").
   - Multiply is an alternative to darken for the colours marked overprint, the course's purple too.
   - A setting chooses between the two. It is beside the overprint checkbox in Course Appearance. The event file saves it, as it saves the checkbox. A new event, and a file saved without the setting, use darken. Translate its strings as task 4 did.
   - `BlendMode` gets `Multiply`. The Skia target (`SKBlendMode.Multiply`) and the PDF target (`XBlendMode.Multiply`) set it on each shape, as they set darken. The SVG target writes `mix-blend-mode:multiply` on each shape, as the application draws it. Then the comparison with the application holds where the course's purple crosses itself. If it does not, allow for the difference.
   - WPF and GDI+ report it as not supported. They need no blend modes (@Pauli).
   - Add tests and baselines for multiply. None of upstream's baselines may move.
   - Root the branch as AGENTS.NotPurplePen.md says for work that could be offered upstream. Merge it into `feature/svg-target` and `dev` through full runs.
8. **Update the stale pull requests on petergolde/PurplePen** (@Pauli, Nov 14). Upstream's d48f445 makes four of them stale.
   - Close #71 (`fix/image-reencode`). Upstream's d48f445 replaces it.
   - Rebase #56 (`fix/headless-pdf-loading`), #57 (`fix/pdf-cache-race`) and #58 (`fix/pdfconverter-build`) onto the newest `upstream/master`.
     - #56: keep only its first commit. Its second commit, a6b3f32d, is in upstream since 1ed9e92.
     - #57: it conflicts with d48f445 in `PdfMapFile.cs`. First find out if upstream's 1ed9e92 covers it. If it does, ask @Pauli whether to close it.
     - #58: it conflicts with d48f445 in `AvPurplePen.csproj`.
   - Run each rebased branch in full on `wip/linux/<name>`, rooted at `base`. Then push it with `--force-with-lease`. The push updates the pull request.
   - If the proxy refuses the API calls to upstream's repository, give @Pauli the links. He closes the pull requests.
   - Update "Ready to offer to Purple Pen".
9. **Upstream's untranslated and wrong strings** (@Pauli, Nov 14). These are upstream's faults. Upstream (d48f445) has them too.
   - "Include:" and its items "Course and map" and "Course only" are English in all languages, in the PDF, SVG and image export dialogs. The keys `CreatePdfCourses_labelPrintBaseMap_Text`, `CreatePdfCourses_comboBoxPrintBaseMap_Items`, `CreatePdfCourses_comboBoxPrintBaseMap_Items1` and the same three `CreateImageFiles_*` keys are only in the English `UIText.resx`. No satellite has them, and no WinForms satellite has them either. Translate them into each language of `UIText.<language>.resx`, in the terms of the language's own translations, and in the room that the dialogs give, as task 4 did.
   - The Japanese `MiscText` messages `CannotCreatePdfs` and `CannotCreateOcadFiles` have a `{0}` that `HandleExceptions` formats with no arguments. A failed export then throws `FormatException` and does not tell why.
   - `CreatePdfCourses_Text` reads "Print Courses" in de, en-GB, et, fi, fr, nb-NO, nn-NO and ro.
   - zh-CN's `CreateMapFiles` changes the `&` of the access key into 和.
   - Make it on a `fix/` branch, rooted where `base` meets upstream, so that it can be offered upstream. Upstream ships the WinForms application, so also correct its satellites where they have the same faults. Run it in full on `wip/linux/<name>`, rooted at `base`. Merge it into `basewithfixes` and `dev` through full runs. Add it to "Ready to offer to Purple Pen".

**Left open:**
- Since the fix of overlapping hatchings, WPF draws the text of lksamm3 and marymoor_underline shifted (its 2 moved baselines). The cause is not known. Only tests draw with WPF (`Test_MapWPF`, `D2D_Tests`). @Pauli accepts it (Nov 13).
- PdfPig 0.1.16 does not give a missing `op` the value of `OP`, against the PDF specification. Task 1's probe corrects it. It could be reported to PdfPig.
- In the SVG tests, the families that the test maps name are drawn in Arial, except Arial, Times New Roman, Cambria, Calibri and Roboto: for example Georgia and Arial Narrow. Fonts with the same metrics, for example Gelasio and Liberation Sans Narrow, could replace them.
- Where a leg meets a control circle, the course drawing overlaps them a little. To move the end points of the leg a little would repair it sufficiently (@Pauli, Nov 13).
- The "Select folder..." button is cut by 13 pixels in the SVG dialog and by 23 pixels in the PDF dialog, in English too (found in task 4).
- Under upstream's bc4debb, the WinForms margin workaround in `PrintDescriptions.cs` and `PrintPunches.cs` follows the home location, but Windows' margins dialog follows the regional format. So the margins can be wrong by a factor of 2.54 where the two do not agree. This comes from reading the code. Nobody tried it. It is in the WinForms application, which the fork does not ship. It could be reported upstream.

## Future directions

Work for after the SVG work is done (@Pauli, Nov 6 and 12).

- **Flattening of blending and opacity.** An option of the export changes each colour that is drawn with blending or with less than full opacity into opaque pieces.
  - It applies to the course's purple and to the map's own colours, for example contours, whose opacity is set for each colour.
  - It is a stage in front of the targets, so that PDF, SVG and printing share it.
  - Each such colour is cut against each colour below it, in the stacking order of the map. Each piece is filled with the mixed CMYK, the piece over white too.
  - The pieces are real geometry, made with boolean path operations. SkiaSharp's operations keep curves. The PostScript and PDF interpreters of old printers accept plain filled shapes without transparency or very large clip paths. Editors work best with them.
  - Against hairline gaps between pieces, a setting makes each cutting shape larger by a set distance, approximately 0.02 to 0.05 mm. A piece then overlaps its neighbour along the cut edges. Its outer edge stays at the edge of the object.
  - Map lines are changed into outlines before they cut. Bitmaps below a blended colour cannot be cut as vectors.
  - It applies to solid colours with opacity and to the standard blend modes, and to nothing else.
  - It is designed so that Inkscape and Scribus can use it too:
    - a short specification of the mixing, in CMYK by the transparency rules of ISO 32000, and of the cutting;
    - only the geometry that all libraries have: outlines, booleans and growth;
    - SVG as the format that a standalone flattener reads and writes;
    - a shared test set that each port must pass;
    - a licence that GPL-2.0-or-later programs can include.
  - The geometry is Skia's path operations, which keep curves and do not change them into polygons. To run in the browser is a priority (@Pauli, Nov 6). The operations are available in C++, in C# through SkiaSharp, in Python as `skia-pathops`, and in the browser through CanvasKit (Skia compiled to WebAssembly). Edges that two areas share exactly are frequent in maps. Test the operations most on them.
  - The language is not chosen. There are two possibilities:
    - one implementation for each platform, on the same Skia algorithm: TypeScript on CanvasKit for the browser (beside the TypeScript port, phase 8), C# on SkiaSharp in Purple Pen, and Python on `skia-pathops` for Inkscape and Scribus;
    - one C++ core on Skia's path operations, compiled to WebAssembly for the browser and natively for the other platforms.
- **A PDF base map as vectors** (@Pauli, Nov 12; not current). The application reads a page of a PDF base map with PdfPig into drawing calls. The target draws them as it draws a map (design: "A PDF base map as vectors"; Decisions). It includes: a paint for RGB colours in `Graphics2D`, a JPEG written as its own data, loop cases of PDF maps, and the bitmap as the fallback for a page that the reader cannot draw. Its requirement replaces the line that puts it out of scope. The port's freedom from PDF maps in P-1 goes to @Pauli with the result.
- **The transparency of a colour** (@Pauli, Nov 13). Purple Pen does not read the transparency of a colour (OCAD's `t`, Mapper's `opacity`). It draws such a colour opaque. Three colours of the test maps have a transparency. If Purple Pen reads it, each such colour becomes one layer at its opacity, in all outputs (see Decisions, "A colour is a layer"). Then the colour's objects do not make each other darker where they overlap. In Mapper they do, because Mapper applies the opacity to each object. This changes how maps with transparent colours draw, and their baselines. It is not known whether OCAD applies the transparency to the layer as a whole. COL-5 would tell where the opacity goes. Flattening needs it.
- **Vector printing.** An option, not the default, at least at first. Printing stays rasterized if the option is not chosen.
  - It is built without WinForms.
  - On Linux and macOS, a PDF goes to CUPS: the PDF export's PDF, or the Skia target's drawing recorded into a PDF, which draws what the raster print draws.
  - On Windows, it goes through Direct2D's print support, or as XPS through Windows' print API.
  - With vectors, the driver or the CUPS filter controls the result. It converts CMYK. It can rasterize blend modes or handle them differently. Flattening before printing leaves no blending for drivers to handle incorrectly.

## Decisions

These are @Pauli's rulings and the practice. The design decisions of the target are in the design ("The SVG target's core design" and the sections after it).

### Process

- **The order of steps** (Nov 6). The session sets the order of steps. It does not ask @Pauli.
- **Offering to Purple Pen** (Oct 23 and 24). Keep separable work in mind as separate pull requests, but first find out if each piece is useful. A change that is useful to Purple Pen by itself goes on its own branch, rooted at `upstream/master`. Each bug in upstream's code that the SVG target needs fixed gets its `fix/` branch before the work continues. "Ready to offer to Purple Pen" lists them all. The EPSG codes of OCAD's grids stay on `feature/svg-target`, because only the SVG export reads them.
- **Offering the fixes upstream** (Nov 7 and 14). Do not offer more fixes now. Update the four stale pull requests now (task 8).
- **Upstream's string faults** (Nov 14). Translate "Include:" and its items, and correct the string faults that task 4 found, on a `fix/` branch that can be offered upstream (task 9).
- **Session transcripts** (Oct 24). Keep the full transcripts of the sessions on GitHub, in the private `windows-fonts`, on their own branch. Archive them unfiltered, as the executors record them: no token or secret gets into a transcript. Archive all transcripts of the machine, because a compaction moves a session to a new transcript. The interval is 10 minutes. It costs no bytes, because git stores each archive as a delta of the one before.
- **The agents' cache, and pacing** (Nov 12 and 13). The prompt cache of agents lasts 1 hour (`subagentPromptCacheTtl`, set by `agent-cache.py`). With the default of 5 minutes, an agent wrote its whole context again after each longer wait: 530,000 to 620,000 tokens. Pacing (rests between the agents' work) stopped on Nov 13: 20 minutes of the hatching agent's work used half of the 5-hour quota. The scripts and the practice stay in `SvgDevLoop/` and CLAUDE.md.
- **Where the SVG work runs in full** (Nov 13). On `basewithfixes`, for the SVG work and all other work that has `fix/` branches. On `base`, the baselines that its fixes move together fail.
- **Merge messages** (Nov 13). The releases page shows the commit message of a build. So a merge into a shared branch names the work that it brings and lists the commits that it brings, as `git merge --log` writes them (CLAUDE.md).
- **A merge's commits on the releases page** (Nov 14). An entry lists up to 30 of the commits that a merge brought, then the count of the others. Only builds published after the change list them (task 6).
- **The commits of builds already on the site** (Nov 7). All builds on the page show their commit. Each takes the head commit of the Build run that built it, found through the run number in its version. A release's tag cannot do this: `gh release create` makes the tag at the tip of the default branch, and a channel's release holds several builds.
- **The language of the documents** (Nov 14). Simplified Technical English.
- **The design decisions** (Nov 14). They are in the design, not in this file.
- **Finished branches** (Nov 14). This file gives @Pauli one command to delete them.

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
- **A PDF base map as vectors** (Nov 12). From task 1's research. The application reads a page of a PDF base map with PdfPig into drawing calls. The target draws them as it draws a map. The bitmap stays for a page that the reader cannot draw. Overprinting as the overprint effect of the map display gives it (`OcadOverprintEffect`). The sRGB of the colours through COL-2's `SwopColorConverter`. The page under the map's label, outside all colour runs for now (M-5). Vectors by default. The other ways (converters, PDFium's page objects, the bitmap) lose the CMYK values and the overprinting, or write files 5 to 25 times larger. Building it waits under "Future directions".
- **PDF base maps in the TypeScript port** (Nov 12). The port does not need to support PDF base maps. P-6 does not apply to an event whose map is a PDF. PdfPig and the reader's drawing calls do not go into the port.
- **A colour is a layer** (Nov 12). Each colour of the colour table is a layer. All that is drawn in it forms one drawing. Objects of the same colour have no effect on each other. The transparency of the colour applies to the layer as a whole. So a colour layer is the union of all that is drawn in it, put one time onto the layers below it, as its overprinting is (D-8). In the run of a colour, the order of its objects changes nothing that shows (M-1). The smoothed edges of shapes of one colour that are on top of each other or touch are not repaired, except the patterns of task 2: the IOF's rules keep objects of the same colour apart in the drawing.
- **Overlapping patterns** (Nov 12 and 13). A pattern drawn thicker where areas of one symbol overlap is not acceptable in any context. The fix is in the map code, for all outputs (screen, printing, the PNG and PDF exports, the SVG), with ghosts in the SVG. Each area is still drawn from its own outline. Only their union clips the pattern, so no object's geometry is merged (see "Path merging"). "Draw area patterns as separate shapes" draws the own pattern of each area, so Scribus loses nothing. Irregular patterns in clip mode are drawn one time too. A group is the areas whose bounding boxes overlap or touch, so areas that only touch lose their seam too. The transparency of a colour waits under "Future directions".
- **Invalid shapes** (Nov 13). The work makes good output possible from reasonable input. An invalid shape is not reasonable input. The map maker must resolve it. An area whose outline crosses itself, or whose hole crosses its outline, draws its own pattern, doubled where it overlaps another area. No polygon library is added for it.
- **The requirements after the fix of overlapping hatchings** (Nov 13). @Pauli approved the wording that task 2's agent proposed: M-1, D-2, M-6, S-7 and S-12 tell how the pattern of overlapping areas is drawn one time under the clip to their ghosts.
- **Blend modes beyond darken** (Nov 13). The map asks only for `BlendMode.Darken`, for each colour marked overprint. SVG, PDF and Skia share the 16 standard modes one to one (CSS `mix-blend-mode`; PDF `/BM`, all in the vendored PdfSharp's `XBlendMode`; `SKBlendMode`). Darken and lighten change nothing where a colour meets itself. The other modes build up. Multiply is the mode that has a meaning in print: it shows ink on ink. WPF and GDI+ need no blend modes. Objects of one colour that overlap on the map need no care, because the IOF's rules forbid them. The course's purple where it overlaps itself (legs that cross, and legs that meet circles and numbers) does not stop blend modes either: a leg drawn darker over the hatching of an out-of-bounds area can be easier to see.
- **Multiply's default** (Nov 14). A new event uses darken. Multiply is chosen beside the overprint checkbox in Course Appearance. The event file saves the choice, as it saves the checkbox (task 7).
- **Images and determinism (F-2)** (Nov 13). Lossless image data is held to its pixels, not its bytes. A PNG that is compressed to other bytes but decodes to the same pixels meets F-2 and P-6. Skia's bundled zlib can compress differently on a CPU without the CRC32C instruction. Lossy data, JPEG too, is held byte for byte.
- **JPEG data** (Nov 13 and 14). The export writes the original JPEG data of an image where it has the data. Otherwise Skia's encoder encodes the image, with the image's resolution in the JFIF header. B-3 states the rule (wording approved on Nov 14). The design tells when browsers draw the data as the application does. Skia's encoder writes all JPEGs that the application encodes, those of the image export and of the OCAD export too. Baselines that this moves go into the same branch.
- **Chromium and a huge bitmap** (Nov 13). Not fixed. Chromium's headless shell crashed at the screenshot of the A2 map's SVG whose PDF base map is a bitmap of 139 million pixels, in two of task 1's three runs, on a machine with 7 GB. It drew the map's vectors. Firefox and WebKit drew the bitmap. The cause was not examined. It is probably memory.

### Tests and CI

- **The loop's improvements** (Oct 23). @Pauli agreed to all of them, fonts in the repository too, in the order that the executor chooses. The comparison stays with Skia: a PDF rendered to pixels adds a third rasterizer and checks what the Skia references check. Vector checks go where pixels cannot see: where Chromium puts each character, the geometry of each SVG against its drawing calls (V-10), and the CMYK values that the exports write.
- **Upstream's glyph positions** (Oct 24). The SVG does not copy upstream's typesetting bugs (design: "Glyph positions in the Skia target").
- **Fonts in the repository** (Oct 24). For determinism, the tests use fonts in the repository. Make reusable scripts for the loop where reasonably possible (`SvgDevLoop/`).
- **Fonts and determinism** (Oct 24). A document whose fonts are embedded never uses a font of the viewer's system.
- **The fixed font set** (Oct 29). The fonts are fixed for testing. The application has no such preference: in typical use, users do not care.
- **The Skia tests' fonts** (Nov 5). Upstream's tests and baselines stay as they are. The Skia tests stay on the machine's fonts, which the workflow pins (`.github/ci/environment.json` and the fonts that the job installs). Only the SVG tests use the fixed font set. A text case that draws a family that the job installs in another face shows a fault of the font install or of the font lookup. Fix it in the workflow, or in the fork's own code if the evidence shows it there.
- **The fonts that the tests install** (Nov 5). Six Skia text cases (TestWest, TestWest11, ArialNarrow, FramingText1 to 3) sometimes drew Arial Narrow as Arial.
  - The install step stopped the font cache service. But to install a font starts the service again during the copy. A font set built from a half-installed store can get to a test process minutes later.
  - `ci/installed-fonts-visible` (71a29e0) keeps the service disabled until all files are in. Then it checks that DirectWrite has all faces that the registry names, and waits up to two minutes. The failure came in 1 of 16 copies of the job before the change, in none of 24 after it, and in no full run since.
  - Arial Narrow has two versions with the same names. Windows' registry kept 2.20, which DirectWrite and Skia drew. 2.40 got only to GDI. WPF's baselines need 2.40, and all other groups draw the same with either version. So `.github/ci/fonts.json` declares only 2.40 (`ARIALN*.TTF`).
  - Fonts that no test needs came out of windows-fonts: 14 files stay, 26 went, after a full run showed that no baseline moves.
- **F-6 without a check by hand** (Nov 13). F-6 needs no check by hand. Inkscape and Scribus run where a session works. The readers in `SvgColourReaders/` open the exports in both and read the CMYK values of each colour.

### Upstream

- **Upstream's fixes against ours** (Nov 13). Task 3 found upstream fixes that share their core with ours but are different in a detail.
  - Images saved as loaded: only upstream's d48f445. These left `base`: our re-encoding of a CMYK, progressive or turned JPEG for the OCAD export (`JpegDataProber`) with its tests; the event file's condition that writes the bytes of an image only in a format that the loader recognized; and the test helper `CompareExportedBitmap`.
  - The loading status of PDFs in tests: all of upstream's 1ed9e92, with our test-only `HeadlessPdfLoadingStatus` and its registration on top, so the tests stay without windows. `fix/headless-pdf-loading` contains only these.
  - The metric check: upstream's bc4debb, which reads Windows' "Country or region" setting, not the regional format. The two are different only on Windows where format and location do not agree. Our 98a85ad and its `UtilTests.IsCurrentCultureMetric` are removed.

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

## Waiting on @Pauli

- **Delete the finished branches** (Nov 14). Nothing depends on them. This machine cannot delete branches. Run this on your machine:

  ```
  git push origin --delete feature/svg-overlapping-patterns proto/overlapping-patterns experiment/svg-patterns-once-record-all experiment/svg-patterns-once-record-all-2 experiment/overlapping-patterns-record-all-2 experiment/basewithfixes-overlapping-record-all experiment/dev-overlapping-record-all
  ```

## Ready to offer to Purple Pen

These are the `fix/` branches. Each is made on upstream's history and has only its own commits. @Pauli reviews them before any becomes a pull request. Before a branch is offered, rebase it onto the newest `upstream/master` and check it again. Some change only what the fork's tests or packaging need. The review finds out if each is useful to Purple Pen.

Eighteen branches that are merged into `base` are offered as the pull requests #54 to #71 of petergolde/PurplePen. Each is on upstream's 2d58e982 (Oct 3). The session cannot read pull requests through GitHub's API. On Nov 13, a trial merge onto upstream's d48f445 was clean for fourteen of them. #56, #57, #58 and #71 had conflicts (task 8). Do not offer the others now (@Pauli, Nov 7). To update a pull request, rebase its branch and push it with force. This needs @Pauli's word.

| Branch | What it changes | In this fork |
| --- | --- | --- |
| `fix/pdf-dialog-color-model` | Map the colour model to the dialogs' choices without an offset | Ready. Full run passed (`wip/linux/pdf-dialog-color-model`, Tests 42279397558). In `basewithfixes` (3b28bfe) and `dev` (0048104), each through a full run. |
| `fix/pattern-brush-origin` | Fill a pattern brush from the pattern origin of an unturned area, where element-by-element drawing puts the pattern | Ready. Full run passed (Tests 42126631152). No baseline moves. `PatternBrushTests` draws areas in the two ways. In `feature/svg-area-patterns`. |
| `fix/skia-subpixel-glyph-positions` | Draw glyphs at their shaped subpixel positions in the Skia target | Ready. Full run passed (Tests 40084278496), with all the baselines that it moves. In `feature/svg-text`. |
| `fix/skia-null-system-typeface` | Take a missing system typeface as no match, and do not dereference it | Ready. Full run passed (Tests 40089461485). In `feature/svg-text`. |
| `fix/skia-font-data-race` | Read the font data of a face whole, under a lock, from any number of threads | Ready. Full run passed (Tests 40071740434). In `feature/svg-text`. |
| `fix/forget-settings-on-new-map` | Forget the export and print settings when another map loads, in both applications | Ready. Full run passed (Tests 41133887589). In `dev`, through `wip/linux/forget-settings-into-dev` (Tests 41133975402). |
| `feature/skia-fixed-font-set` | Let the private fonts be the only fonts, for families and for fallback (off by default) | Ready. Full run passed (Tests 40780464139). In `feature/svg-target`. |
| `fix/backslashes-in-test-file-paths` | Find test files named with backslashes on all platforms | In `feature/svg-target`. |
| `fix/case-of-test-file-names` | Spell four test file names as the files are spelled | In `feature/svg-target`. |
| `fix/culture-independent-descriptor-parsing` | Read rendering descriptors with periods as decimal points | In `feature/svg-target`. |
| `fix/bitmap-print-scale` | Size an exported bitmap by the scale at which the course prints | In `dev`. |
| `fix/compatibility-event-map` | Give the 1.01 compatibility event its map | In `base`. #54, clean on d48f445. |
| `fix/ui-tests-not-parallel` | Keep the tests that drive a window out of the parallel pool | In `base`. #55, clean on d48f445. |
| `fix/headless-pdf-loading` | Give each PDF conversion its own loading status | In `base`. #56. Conflicts with d48f445 in `GlobalTestSetup.cs` (task 8). |
| `fix/pdf-cache-race` | Publish a converted PDF under its cache name only when it is complete | In `base`. #57. Conflicts with d48f445 in `PdfMapFile.cs` (task 8). |
| `fix/pdfconverter-build` | Restore PdfConverter before it is built, and build it for the tests | In `base`. #58. Conflicts with d48f445 in `AvPurplePen.csproj` (task 8). |
| `fix/proj4-load-diagnostics` | Tell why proj.dll could not be loaded | In `base`. #59, clean on d48f445. |
| `fix/save-failed-comparison` | Leave something to look at when a bitmap comparison fails | In `base`. #60, clean on d48f445. |
| `fix/skia-decode-file-handle` | Decode a rendered PNG from memory, not from its path | In `base`. #61, clean on d48f445. |
| `fix/parallel-temp-file-names` | Give each rendering test its own scratch files | In `base`. #62, clean on d48f445. |
| `fix/print-area-output-directory` | Create the output directory that the print-area export writes into | In `base`. #63, clean on d48f445. |
| `fix/parallel-export-temp-names` | Give each relay export test its own scratch file | In `base`. #64, clean on d48f445. |
| `fix/record-missing-baseline` | Let a comparison without a baseline leave its rendering | In `base`. #65, clean on d48f445. |
| `fix/pdf-failure-message` | Tell why the creation of files failed | In `base`. #66, clean on d48f445. |
| `fix/lossless-export-comparison` | Compare what the image export draws, not a JPEG made of it | In `base`. #67, clean on d48f445. |
| `fix/font-fallback-coverage` | Ask the font fallback what it promises, not what one machine drew | In `base`. #68, clean on d48f445. |
| `fix/package-verify-pipefail` | Verify packages without a pipe into grep -q | In `base`. #69, clean on d48f445. |
| `fix/test-start-log` | Name each test when it starts | In `base` and `dev` as an equivalent commit. #70, clean on d48f445. |
| `fix/image-reencode` | Check the exported bitmaps as the files that they are | Replaced by upstream's d48f445. Removed from `base` (Nov 13; see Decisions, "Upstream's fixes against ours"). #71: close it (task 8). |
| `fix/invariant-culture-metric` | Tell whether a culture without a region is metric, so that an event without its own paper loads under the invariant culture | Replaced by upstream's bc4debb. Removed from `basewithfixes` and `dev` (Nov 13). Never offered. Retire it. |
| `fix/image-reencode-png-baselines` | Write a JPEG through only when all readers can take it | Not merged. Obsolete: an earlier form of `fix/image-reencode`. |
| `fix/jpeg-compare-tolerance` | Tolerate the encoder in the bitmaps of the OCAD export too | Not merged. Obsolete: `fix/lossless-export-comparison` made the comparisons of the JPEG export tests lossless, and upstream's d48f445 writes the images of the OCAD export unchanged. |
| `fix/pdf-temp-file-lock` | Give a locked render file time to be released | Not merged. Probably obsolete: the sharing violation did not come back in the full runs after the tests got their own scratch files. |
| `fix/pdf-font-without-os2` | Describe a font that has no OS/2 table | Not merged, because the fork's runs do not have such fonts. The bug is in upstream and `dev`: PDF export throws on a TrueType font without an OS/2 table, for example Corel's Switzerland, which the Marymoor test maps name. A candidate. |
| `fix/skia-font-tables` | Read optional font tables without an exception | Not merged, because the fork's runs install the fonts. The bug is in upstream and `dev`: SkiaSharp's `GetTableData` throws for a table that a font does not have, where `FontFallback` and `FontVerticalMetrics` expect null. On Oct 13, a map that names a font that is not installed did not load (marymoor.ocd without Tekton). A candidate. First check it in the Avalonia application. |
| `fix/xml-timestamp-timezone` | Accept all time zones in the baseline comparison of exported XML | Not merged, because the fork's runs set Pacific time (`tests.yml`). Test only: the comparison matches a timestamp only as Pacific time writes it, and fails in all other time zones. A candidate. |

## The executor

A session in Anthropic's cloud does the work. Since Nov 14, it is the session whose transcript on `sessions` is `2026-10-22-claude-046e7cdd-12d2-555a-877c-af697a5b8dae`. It took the work from this file and the branches on @Pauli's word. It archives the machine's transcripts every 10 minutes from a clone of the `sessions` branch at `/home/claude/windows-fonts`. No session has a scheduled task, so no other session wakes to work beside it.
