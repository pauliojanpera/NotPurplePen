# The palette-stack work: progress

Nov 18, 2026. This file is the handover of the palette-stack work. It tells where the work is, how to continue it,
what comes next, what waits on @Pauli, and the decisions. The branches are not in `basewithfixes` or `dev`, so
`../NotPurplePenProgress.md` does not track them (@Pauli, Nov 18). Update this file when a branch changes state and
when @Pauli makes a decision. It tells the state, not the events: the commits and the runs record those.

- [README.md](README.md) tells how the work is organized: its tickets, branches, pull request descriptions and
  tracking.
- [Specification.md](Specification.md) is the reference of the architecture, and the epic
  [000](tickets/000-palette-stack.md) gives the goal and the scope of the work.

## Where the work is

The parts 001 to 005, 007 and 008 are done and merged into `feature/palette-stack`, and its full run passed. Pass 1
makes the instances with full coverage (G = 1) of each palette entry ([005](tickets/005-layer-instances.md)) from the
map alone: it takes no rectangle and no resolution of a drawing, and it holds each visible symbol wherever it lies
([007](tickets/007-map-instances.md)). It goes through the symbols once: the point symbols make their clipping objects
with a geometry method and share the shapes of their glyph ([008](tickets/008-symbol-geometry.md)), and the collector
collects the other symbols. The instances that the collector makes of all the symbols are the oracle of the geometry
methods. The fixes [006](tickets/006-matrix-equals-offsets.md) and [014](tickets/014-basic-tests-private-fonts.md),
which the tests of 005 and the full run of 008 found, are done on branches of their own, and no part needs them. The
next parts make the geometry methods of the lines ([011](tickets/011-line-geometry.md)), of the areas, the
combinations and the graphics objects ([012](tickets/012-area-geometry.md)), and of the texts and the rectangles
([013](tickets/013-text-geometry.md)). The tickets 009 to 013 are drafts (see "Next"). Before ticket 011, the
dashed lines were checked against the OCAD viewer ([oracles/ocad-dashes.md](oracles/ocad-dashes.md)). Three samples
showed the rules of OCAD that the fixes [016](tickets/016-ocad-metric-parameter.md),
[017](tickets/017-ocad-dash-count.md), [018](tickets/018-double-line-dash-gaps.md) and
[019](tickets/019-ocad-minimum-gaps.md) follow. A fourth sample showed that OCAD composes the dashes at corner points
and on the borders of double lines as Purple Pen does, except where a dash starts or ends very near a vertex
([020](tickets/020-dash-boundary-at-vertex.md)). The fixes 015 to 020 are merged into `feature/palette-stack`
(@Pauli, Nov 19), and its full run passed with them. The PDF export of the sixth sample showed that OCAD leaves the
vertex out of a dash whose piece beyond the vertex is short: on a single line shorter than half its width, at a turn
of 90°. The seventh sample gave the rule: shorter than 0.4 times the width of a single line at any angle, and on a
double line about 0.05 mm for the dash of the middle and 0.4 times the width of each border for its own dash.

Ticket 011 is in progress on `feature/line-geometry`. The lines make their clipping objects with a geometry method,
and the collector collects no line. A dashed line is a composed dashed line, and the evenly spaced glyphs of each part
of a line are one repetition of the shared shapes. @Pauli decided the questions 5 to 8 of the ticket on Nov 19, and
the eighth dash sample confirmed the rule of question 9. The tickets 011 to 013 were made while they were drafts,
without decided criteria. From Nov 19, @Pauli reviews the points of their implementation that the drafts left open.
The first point led to the decision on the OCAD metric on curves, an extension of fix 021 that is not made yet. On a
curve, the glyphs of the OCAD spacing repeat by quarters (@Pauli, Nov 20). The criteria are not decided. Ticket 012 is in
progress on `feature/area-geometry`, which is rooted at `feature/line-geometry`: the areas, the combined areas and the
graphics objects make their clipping objects with geometry methods. Ticket 013 is in progress on
`feature/text-geometry`, which is rooted at `feature/area-geometry`: the texts, the texts along lines and the
rectangles have geometry methods too, so every kind of symbol has one, and pass 1 no longer uses the collector. The
line framing of a text run is one line, as OCAD draws it (question 2 of the ticket). The collector makes the oracle of
the tests.

## Branches

`feature/palette-stack` collects the parts that are done. It is rooted where `base` meets upstream (d48f4456), as
is each part that builds on no other part; a part that builds on earlier parts is rooted at `feature/palette-stack`.
Each branch runs in full on `wip/linux/<name>`, on `base` (see "Decisions"). A run before that decision names its
base. No branch is offered upstream yet.

| Branch | Ticket and description | State |
| --- | --- | --- |
| `feature/palette-stack` | [000](tickets/000-palette-stack.md) | The branch of the epic. It merges `feature/inks` (a0f0ed6f), `fix/ocad11-utf8-strings` (e51e395d), `feature/opacity-and-blend-mode` (10dd7511, e1101c42), `feature/layer-instances` (c98d380e), `feature/map-instances` (dd6f9ee9, eaa9e446) and `feature/symbol-geometry` (3a459fdc), and before ticket 011 the fixes `fix/thick-glyph-circles` (636d5f95), `fix/ocad-metric-parameter` (bdec06f0), `fix/ocad-dash-count` (e00e50ff), `fix/double-line-dash-gaps` (ab0e0982), `fix/ocad-minimum-gaps` (3ea75c6c) and `fix/dash-boundary-at-vertex` (581e7588) (@Pauli, Nov 19), and then `fix/ocad-dashes-on-curves` (c53c9bebf, @Pauli, Nov 19). The 10 image baselines that the merge of `fix/ocad-dashes-on-curves` and the earlier fixes both moved take the fix's version. The collecting run 44538544336 drew 145 baselines of all the fixes together, which d00c5c4e records (23 of Skia, 28 of the GDI+ renderings of `Test_MapModel`, 22 of PDF, 9 of WPF and 63 of `PurplePen_Tests`), and `prs/000-palette-stack-base/` has its three route gadget baselines. Full run passed at d00c5c4e on `base` (`wip/linux/palette-layers` at 5b3123f2: Tests 44543160008, Build 44543159900). 6bc52ba1 makes the geometry method of the point symbols fill the glyph circles as the fixed drawing does, and 47578e98 gives `SymbolGeometryTests` the count of clipping objects that follows. Each merge of a fix recorded the baselines that the fix moves; the collecting run 44426327169 drew them with all the fixes together, and 400d4369 records them. Full run passed at 400d4369 on `base` (`wip/linux/palette-layers`, with `--base-baselines prs/000-palette-stack-base`: Tests 44429290119, Build 44429290123). `prs/000-palette-stack-base/` keeps the three route gadget baselines of `base` that the fixes move together. After the merge of `fix/dash-boundary-at-vertex` (581e7588), the collecting run 44445217631 drew the baselines of all the fixes together, de72040a records them, and the full run passed at de72040a (Tests 44447019786, Build 44447019560); `prs/000-palette-stack-base/` has the route gadget baselines of that run. Full run passed at 3a459fdc on `base` (`wip/linux/palette-layers`: Tests 44326121310, Build 44326121256), without a collecting run (@Pauli, Nov 18): its tree on `base` is the tree that `wip/linux/symbol-geometry` tested (41830ddb, `git merge-tree`), and no image baseline moves (see `feature/symbol-geometry`). The runs at eaa9e446 on `base` (Tests 44297317622, Build 44297317568) and on `basewithfixes` (Tests 44259000556, Build 44259000493) passed before. Its merge into `dev` conflicts in `Map.cs`, `SymDef.cs`, `GraphicsUtil.cs` and `Map_Skia.Tests/RenderingTests.cs`. `GraphicsUtil.cs` conflicts since `feature/symbol-geometry`, and the changes of `feature/layer-instances` conflict with `dev` in `Map.cs` and `SymDef.cs`. |
| `feature/text-geometry` | [013](tickets/013-text-geometry.md), [description](prs/013-text-geometry.md) | In progress, a draft ticket. The texts, the texts along lines and the rectangles have geometry methods, every kind of symbol has one (`Symbol.AddClippingObjects`), and pass 1 no longer uses the collector. The line framing of a text run is one line of the full width, as OCAD draws it (question 2 of the ticket), and the comparison keeps the last of the collector's consecutive strokes of a run. It merges the repetition of `feature/area-geometry` (40da5d78), and fix 021 through it (2ad8c6f39). The old drawing code does not change. Locally, `SymbolGeometryTests`, the `LayerInstance` tests and the tests of the OCAD dashes pass (334 tests). Full run passed at f4c2a23a, with fix 021 (Tests 44547297516, Build 44547297192), at eaa0a45a, with the framing line and the repetition (`wip/linux/text-geometry`, on `base`, with `--base-baselines prs/000-palette-stack-base`: Tests 44522031521, Build 44522031420), and before them at a20a13ac (Tests 44504633617, Build 44504633614). Rooted at `feature/area-geometry` (ca753598). |
| `feature/area-geometry` | [012](tickets/012-area-geometry.md), [description](prs/012-area-geometry.md) | In progress, a draft ticket. The geometry methods of areas (`AreaLikeSymDef.AddClippingObjects`) make the fills and the borders of the areas and the combined areas, without the hatchings and the patterns, and the graphics objects have geometry methods of their own. The old drawing code does not change. It merges the repetition of `feature/line-geometry` (7350f74d), and fix 021 through it (778009948). Locally, `SymbolGeometryTests`, the `LayerInstance` tests and the tests of the OCAD dashes pass (333 tests). Full run passed at b4ba3c9a, with fix 021 (`wip/linux/area-geometry`, on `base`, with `--base-baselines prs/000-palette-stack-base`: Tests 44547247036, Build 44547247229), and at c530643c (Tests 44520933949, Build 44520933988). Rooted at `feature/line-geometry` (159335a0), as the borders use the geometry method of lines. |
| `feature/line-geometry` | [011](tickets/011-line-geometry.md), [description](prs/011-line-geometry.md) | In progress, a draft ticket. The geometry methods of lines (`LineLikeSymDef.AddClippingObjects`) make all the lines: solid lines, composed dashed lines, the glyphs along lines and the combined lines of Mapper maps. The old drawing code does not change. Locally, `SymbolGeometryTests` and the `LayerInstance` tests pass. Full runs passed on `wip/linux/line-geometry`, on `base`, with `--base-baselines prs/000-palette-stack-base`: at 6e1113f1, with the composed dashed lines (Tests 44450938043, Build 44450938048), and at 6fda1e93, with the glyphs (Tests 44453540710, Build 44453540720). Full run passed at 4b0a7b88, with the combined lines (Tests 44454861516, Build 44454861561). The evenly spaced glyphs of each part are one repetition (`RepeatedShapes`, question 6 of the ticket), which does not turn a glyph of one circle at its origin, as the drawing draws that glyph without a transformation. Full run passed at aa3e9311, with the repetition (Tests 44519243388, Build 44519243371). It merges `feature/palette-stack` with fix 021 (465502f2), so the runs on a Bezier curve of a euclidean map are by quarters, and a test checks them (6c764860). Locally, `SymbolGeometryTests`, the `LayerInstance` tests and the tests of the OCAD dashes pass (332 tests). Full run passed at 507a2938, with fix 021 (Tests 44546671610, Build 44546671573). With the OCAD metric, the dashes on curves are mostly single, and a repetition crosses the vertices with its steps in that metric. The decisions of Nov 19 need runs and repetitions by segment and by quarter there, which come with the extension of fix 021. Rooted at `feature/palette-stack` (de72040a). |
| `feature/symbol-geometry` | [008](tickets/008-symbol-geometry.md), [description](prs/008-symbol-geometry.md) | All criteria met. Full run passed at 22069b9d (`wip/linux/symbol-geometry`, on `base`: Tests 44323843784, Build 44323843797), without a collecting run: the drawing code of the symbols does not change, and no image or text baseline moves. The run at 82cbbe57 failed in `RenderingTests.Roboto`, which drew Arial in place of Roboto (Tests 44318693192): the font tests of `BasicTests` clear the private fonts, and NUnit called the source of the test cases of `SymbolGeometryTests` while it made the tests, so the static constructor of `RenderingTests` added the fonts before `BasicTests` ran ([014](tickets/014-basic-tests-private-fonts.md)). 22069b9d adds the fonts when the fixture starts. In `feature/palette-stack` (3a459fdc). |
| `feature/map-instances` | [007](tickets/007-map-instances.md), [description](prs/007-map-instances.md) | All criteria met. Full run passed at bb2ead55 (`wip/linux/map-instances`, on `base`: Tests 44295161752, Build 44295161522). No image or text baseline moves (collecting run 44286210380, of 1a7b8051, wrote no image; bb2ead55 adds a test only). In `feature/palette-stack` (dd6f9ee9, eaa9e446). |
| `feature/layer-instances` | [005](tickets/005-layer-instances.md), [description](prs/005-layer-instances.md) | All criteria met. Full run passed at fb451340 (`wip/linux/layer-instances`, on `basewithfixes`: Tests 44257645995, Build 44257646116). No image or text baseline moves (collecting run 44256257076, of 2e770ad4, wrote no image). In `feature/palette-stack` (c98d380e). |
| `fix/ocad-dashes-on-curves` | [021](tickets/021-ocad-dashes-on-curves.md), [description](prs/021-ocad-dashes-on-curves.md) | With the euclidean metric, the dashes and the glyphs of the OCAD spacing on Bezier curves are placed by the quarters of each curve, as OCAD's PDF exports of the ninth and the third dash samples show (df4e9975, 0c6ec007). Its test `OcadDashesOnCurvesTests` fails without the fix in 10 of the 17 cases of the dashes and in all 7 of the glyphs. The collecting run 44532214497 drew 11 baselines that the fix moves, which the branch records (998cf61f): 5 of Skia, 4 of the GDI+ renderings of `Test_MapModel` and 2 of PDF, all of OCAD 11 maps. No baseline of `base` moves beyond those of fix 017, so its runs take `--base-baselines prs/017-ocad-dash-count-base`. Full run passed at dc76f6c6 (`wip/linux/ocad-dashes-on-curves`, on `base`: Tests 44535962486, Build 44535962339). Rooted at `fix/ocad-dash-count` (b99067b5), as the fix uses its measure of four chords (@Pauli, Nov 19). In `feature/palette-stack` (c53c9bebf, @Pauli, Nov 19). The extension to the OCAD metric, with the chords of the quarters in that metric, is decided for this branch (@Pauli, Nov 19) and not made yet. |
| `fix/dash-boundary-at-vertex` | [020](tickets/020-dash-boundary-at-vertex.md), [description](prs/020-dash-boundary-at-vertex.md) | The rule of OCAD's PDF export (@Pauli, Nov 19), at 4b0b22a7. Its test `OcadDashVertexTests` compares 74 cells of the seventh dash sample with OCAD's PDF export, and fails without the fix in 38 of them. The collecting run 44441352883 wrote 127 images: 124 baselines of upstream, which the branch records (363615c7), and three route gadget baselines that `base` added, which `prs/020-dash-boundary-at-vertex-base/` keeps for the merge into `base`. Full run passed at 363615c7 (`wip/linux/dash-boundary-at-vertex`, on `base`, with `--base-baselines prs/020-dash-boundary-at-vertex-base`: Tests 44443705594, Build 44443705749). Rooted at d48f4456. In `feature/palette-stack` (581e7588). |
| `fix/ocad-metric-parameter` | [016](tickets/016-ocad-metric-parameter.md), [description](prs/016-ocad-metric-parameter.md) | Full run passed at 99ce8e7c (`wip/linux/ocad-metric-parameter`, on `base`, with `--base-baselines prs/016-ocad-metric-parameter-base`: Tests 44415489933, Build 44415489926). The collecting run 44363913238 wrote 213 images: 210 baselines of upstream, which the branch records, and three route gadget baselines that `base` added, which `prs/016-ocad-metric-parameter-base/` keeps for the merge into `base`. The OCAD 11, 12 and 2018 versions of Lake Sammamish and teanwest have `p1` and get baselines of their own (`*_ocadmetric`). Its test `OcadMetricTests` fails without the fix in 7 of its 15 cases. Rooted where `base` meets upstream (d48f4456), as the other fixes. In `feature/palette-stack` (bdec06f0). |
| `fix/ocad-dash-count` | [017](tickets/017-ocad-dash-count.md), [description](prs/017-ocad-dash-count.md) | Full run passed at b99067b5 (`wip/linux/ocad-dash-count`, on `base`, with `--base-baselines prs/017-ocad-dash-count-base`: Tests 44417098812, Build 44417098893). The collecting run 44414319636 wrote 109 images: 107 baselines of upstream, which the branch records, and two route gadget baselines that `base` added, which `prs/017-ocad-dash-count-base/` keeps for the merge into `base`. Its test `OcadDashCountTests` fails without the fix in 21 of its 28 cases. Rooted at d48f4456. In `feature/palette-stack` (e00e50ff). |
| `fix/double-line-dash-gaps` | [018](tickets/018-double-line-dash-gaps.md), [description](prs/018-double-line-dash-gaps.md) | Full run passed at 61406887 (`wip/linux/double-line-dash-gaps`, on `base`: Tests 44415551500, Build 44415551330). No baseline moves (collecting run 44412734699). Its test `DoubleLineDashTests` fails without the fix in 11 of its 16 cases. Rooted at d48f4456. In `feature/palette-stack` (ab0e0982). |
| `fix/ocad-minimum-gaps` | [019](tickets/019-ocad-minimum-gaps.md), [description](prs/019-ocad-minimum-gaps.md) | Full run passed at 6805bf5c (`wip/linux/ocad-minimum-gaps`, on `base`: Tests 44415599153, Build 44415599108). No baseline moves (collecting run 44412813635). Its test `OcadMinimumGapsTests` fails without the fix in all its 7 cases. Rooted at d48f4456. In `feature/palette-stack` (3ea75c6c). |
| `fix/thick-glyph-circles` | [015](tickets/015-thick-glyph-circles.md), [description](prs/015-thick-glyph-circles.md) | Full run passed at 3376cb04 (`wip/linux/thick-glyph-circles`, on `base`: Tests 44346994914, Build 44346994936). The first attempt of that Tests run stopped in its build, where Chocolatey could not fetch `htmlhelp.exe` (HTTP 503). The collecting run 44344592098 wrote four images, which the branch records: `skia_render/multisymonedash.png`, `rendering/multisymonedash.png`, `skia_render/LordHill.png` and `skia_render/LordHill OCAD11.png`. Its test `GlyphCircleTests` fails without the fix: three glyphs without the change of the drawing, and five circles of an OCAD 8 file without the change of the import. The oracle is [oracles/ocad-thick-circles.md](oracles/ocad-thick-circles.md). Separated from 008. In `feature/palette-stack` (636d5f95), where 6bc52ba1 gives the geometry method of the point symbols the same circles. |
| `fix/basic-tests-private-fonts` | [014](tickets/014-basic-tests-private-fonts.md), [description](prs/014-basic-tests-private-fonts.md) | Full run passed at 5638588f (`wip/linux/basic-tests-private-fonts`, on `base`: Tests 44325347234, Build 44325347956). In a run of dbe7de2a, `base` with the fix and `feature/symbol-geometry` at c38217a6, `RenderingTests.Roboto` passed (Tests 44325323415, group Skia), which failed without the fix (Tests 44318693192). Its branch `experiment/basic-tests-private-fonts` is deleted (@Pauli, Nov 18). The group projections of that run could not fetch the build (HTTP 503, egress over the account limit) and ran no test. Separated from 008, whose full run found the fault. No part needs it, and it is in no other branch. |
| `fix/matrix-equals-offsets` | [006](tickets/006-matrix-equals-offsets.md), [description](prs/006-matrix-equals-offsets.md) | Full run passed at 5e827f47 (`wip/linux/matrix-equals-offsets`, on `base`: Tests 44256623574, Build 44256623572). Its test `MatrixTests.EqualsItsCopy` fails without the fix. Separated from 005, whose tests compare matrices element by element. No part needs it, and it is in no other branch. The work of 005 does not fix the fault, so the rule of the separated fixes does not cover the branch, and it is kept as it is (@Pauli, Nov 18). |
| `feature/opacity-and-blend-mode` | [004](tickets/004-opacity-and-blend-mode.md), [description](prs/004-opacity-and-blend-mode.md) | All criteria met. Full run passed at 7882bca8 (`wip/linux/opacity-and-blend-mode`, on `basewithfixes`: Tests 44222985536, Build 44222985519). No image baseline moves (collecting run 44223000991). The 30 text dumps of the OCAD 9 and 11 course files of `ControllerTests` get `t100` in each color (@Pauli, Nov 18). Its Mapper test map `openmapper/LordHill.omap` is `LordHill.ocd` as Mapper saved it (@Pauli, Nov 18). In `feature/palette-stack` (e1101c42). |
| `feature/inks` | [001](tickets/001-inks.md), [description](prs/001-inks.md) | All criteria met. Full run passed at a75c49aa (`wip/linux/inks`, on `basewithfixes`: Tests 44171873489, Build 44171873441). No existing baseline moves (collecting run 44155029726); the new Skia test `MapperOverprinting` has the baseline `skia_render/hakolaoverprinting.png`, from Tests run 44158232814. It merges `fix/mapper-export-color-numbers`. In `feature/palette-stack` (a0f0ed6f). |
| `fix/mapper-export-color-numbers` | [002](tickets/002-mapper-export-color-numbers.md), [description](prs/002-mapper-export-color-numbers.md) | Full run passed (`wip/linux/mapper-export-color-numbers`, on `base`: Tests 44171502072, Build 44171502166). Its test `ExportNumbersColorsByPlace` fails without the fix. In `feature/inks` (a75c49aa), which had the same fix as 91994366. |
| `fix/ocad11-utf8-strings` | [003](tickets/003-ocad11-utf8-strings.md), [description](prs/003-ocad11-utf8-strings.md) | Full run passed (`wip/linux/ocad11-utf8-strings`, on `base`: Tests 44196794627, Build 44196794547). No baseline moves (collecting run 44176064791, of an earlier form of the fix). Its test file is the ISSprOM 2019 symbol set of the Finnish Orienteering Federation, which the federation gives out with no statement about redistribution (@Pauli, Nov 18: little harm in it). In `feature/palette-stack` (e51e395d). |

## How to continue the work

- **Read first**, in this order: [../Introduction.md](../Introduction.md) (what NotPurplePen is, and the rules of all
  work), [README.md](README.md), this file, the preface of [Specification.md](Specification.md) (when it binds, and
  which programs are the oracles), and the ticket of the part at hand.
- **The rules that this work adds.** The tickets are in Finnish, and the other documents in Simplified Technical
  English. A question for @Pauli comes when the decision is his and the session's own choice would not be easy to
  correct later: one question at a time, with choices and a recommendation. Otherwise the session goes on. No push
  goes to `base`, `basewithfixes`, `dev` or `master`; a merge into them needs @Pauli's word. A session does not wait
  for a run whose result no next step needs.
- **The architecture comes first.** feature/multiply-blending is an oracle of the appearance only. The work does not
  read its code, and does not take its way, a layer around the drawing of the symbols, as the model of pass 2. The
  union of a palette entry can stay implicit in the rendering, but its instances cannot be left out (preface and
  section 04 of [Specification.md](Specification.md), @Pauli, Nov 18).
- **Screenshots and exports of the OCAD viewer.** @Pauli provides screenshots of the viewer of OCAD 2020, and its
  drawings exported as PNG images of 600 dpi, when the work needs to know how OCAD draws a file (@Pauli, Nov 18). The
  session gives him the file and tells what to report, as [oracles/ocad-thick-circles.md](oracles/ocad-thick-circles.md)
  and [oracles/ocad-dashes.md](oracles/ocad-dashes.md) show.
- **Where the work runs.** This is @Pauli's machine; no Windows machine exists here. A build or a test counts only
  when it ran in GitHub Actions, on a `wip/` branch. The .NET SDK is `~/.dotnet/dotnet`, outside `PATH`.
- **The checkouts.** The clone `~/Documents/Projects/NotPurplePen` stays on `docs/process-coordination`: do not change
  its branch. Each branch has a worktree in `~/Documents/Projects/npp-worktrees/`: `palette-layers`, `inks`,
  `fix-mapper-colors`, `fix-ocad-utf8`, `opacity-and-blend-mode`, `layer-instances`, `fix-matrix-equals`,
  `map-instances`, `symbol-geometry`, `fix-private-fonts`, `fix-thick-circles`, `fix-ocad-metric`, `fix-dash-count`,
  `fix-double-dashes`, `fix-minimum-gaps`, `fix-dash-vertex`, `fix-dashes-on-curves`, `line-geometry`, `area-geometry` and `text-geometry`. The worktrees whose names end in `-int` hold the local integration
  branches `local/<name>-int`, which are never pushed. `tools/int-worktree.sh` makes them again from GitHub, on `base`.
  `palette-layers-int`, `map-instances-int`, `symbol-geometry-int`, `fix-thick-circles-int`, `fix-ocad-metric-int`,
  `fix-dash-count-int`, `fix-double-dashes-int`, `fix-minimum-gaps-int`, `fix-dash-vertex-int`,
  `fix-dashes-on-curves-int`, `line-geometry-int`, `area-geometry-int` and `text-geometry-int` are on `base`, and the others stay on
  `basewithfixes` until `REBUILD=1 tools/int-worktree.sh` makes them again.
- **Local tests.** `Test_MapModel` targets Windows; `tools/run-map-tests.sh` runs its files on Linux, and skips the
  renderings, which GDI+ draws. `Map_Skia.Tests` runs on Linux as it is: set `TEST_SILENTRUN=True`, or a failed
  comparison opens VisualDiff and waits. A local rendering tells which images a change moves. The fonts of this machine
  are not the workflow's, and a Skia comparison allows no difference, so each baseline comes from a run in GitHub
  Actions (`tools/push-wip.sh --collect`, or `--blank-baseline` for a new Skia test). Roboto is installed on this
  machine (`~/.local/share/fonts`), so a local run draws the Roboto maps in Roboto also where the private fonts of
  the tests are missing. The Windows machine of the workflow has no Roboto
  ([014](tickets/014-basic-tests-private-fonts.md)).
- **A collecting run and the run of the push.** A Tests run of a push waits for an earlier Tests run of the same commit
  and takes its result, and a collecting run does not fail on a moved image. So `tools/push-wip.sh --collect` starts
  the collecting run only after the runs of the push have started, and the Tests run of the push tests the commit
  itself. A collecting run alone passes only when it wrote no image: no artifact `rendering-differences-<group>`
  holds a `*_actual.png`.
- **Text baselines.** The `CreateOcadFiles` tests of `ControllerTests` compare an `OcadDump` of each course file that
  they write with an expected dump in `src/TestFiles/controller/ocad_create*`. A collecting run does not collect them,
  and `src/Testing/LinuxTestRun` does not run them. A change of the OCAD export that moves them is found by a full run.
- **Line endings.** The sources have CRLF line endings, and git does not change them. Check a changed file with
  `git ls-files --eol`; `tools/crlf.py` gives a file CRLF again.

## Next

The parts come in the order of the two passes of the specification (section 01), so that each part builds the
structure and not only the appearance. Each part builds on the parts before it, so its branch is rooted at
`feature/palette-stack`.

1. **Pass 1 from the geometry methods of the symbols**, in four tickets: the foundation and the point symbols
   ([008](tickets/008-symbol-geometry.md), done, branch `feature/symbol-geometry`), the lines
   ([011](tickets/011-line-geometry.md)), the areas, the combinations and the graphics objects
   ([012](tickets/012-area-geometry.md)), and the texts and the rectangles ([013](tickets/013-text-geometry.md)). The
   tickets 011 to 013 are drafts. The symbols make their clipping objects with geometry methods of their own
   (`Symbol.TryAddClippingObjects`), in place of the collector, and the instances that the collector makes of all the
   symbols (`Map.CollectLayerInstances`) are the oracle. `SymbolGeometryTests` compares the two as structures
   (`LayerInstanceComparison`) on each map of `skia_render`, and the later parts use the same comparison. The old
   drawing code stays as it is, so the drawing does not change. In ticket 011, the model keeps the structure of a
   line in its ideal form, and an encoding converts it for its channel. For a channel whose dash pattern measures the
   euclidean length, the encoding writes a composed dashed line, whose runs it makes from the measure of the model
   (@Pauli, Nov 20). The glyphs along a line repeat with shared shapes, on straight and curved parts alike (@Pauli, Nov 19). On a
   Bezier curve, the glyphs of the OCAD spacing repeat by quarters (@Pauli, Nov 20). Before that, the dashed lines
   were checked against OCAD: [oracles/ocad-dashes.md](oracles/ocad-dashes.md) records the
   samples and the rules of OCAD that they show. The fixes 015 to 020 are merged into `feature/palette-stack`
   before the work on the ticket starts.
2. **Pass 1, the patterns** (sections 02 and 03): [ticket 009](tickets/009-pattern-instances.md), a draft. The hatchings and the area patterns become prototypes
   with their lattice and element transformations, and the elements that a fill mode selects become clipping objects.
   The pass 1 of [ticket 005](tickets/005-layer-instances.md) leaves them out with the internal option
   `RenderOptions.omitHatchingsAndPatterns`. Ticket 009 removes the option and ends the use of the collector as the
   oracle of the tests, and the coverage comparison then compares the colors with hatchings and patterns too. A regular clipped pattern that the options draw with a pattern brush
   gives its lattice in `CreatePatternBrush`. The other patterns place each glyph with a transformation of its own,
   and a pattern that keeps whole glyphs draws them without a clip, as a point symbol draws its glyph. Their lattices
   and element transformations are in the definition of the area (`HatchInfo` and `PatternInfo` of `AreaSymDef`) and
   in the angle of the area object, and the drawing calls do not show them.
3. **Pass 2, the composition by palette entry** (section 04): [ticket 010](tickets/010-palette-composition.md), a draft. It reads the instances only, groups them by palette
   entry, and applies C, α and B once to each group, in the order of the palette. α and B are in the model
   (`SymColor.Opacity` and `SymColor.BlendMode`, [ticket 004](tickets/004-opacity-and-blend-mode.md)), and the
   drawing does not use them yet. For an OCAD map, B = Darken is the larger share of each ink, and not the RGB minimum
   ([oracles/ocad-darken.md](oracles/ocad-darken.md)). Mapper applies α to each shape, so it is not the oracle of α
   where the shapes of one color overlap ([oracles/mapper-opacity.md](oracles/mapper-opacity.md)). A clipping object
   keeps the steps of the transformations that placed it (`Placement`), so pass 2 can apply them as the drawing of the
   symbols did, with the same rounding.
4. **The rendering modes** (section 04): the normal view, the overprinting that the inks simulate, and the
   separations, as color configurations of pass 2. All modes use the same instances. The section "Rajaus" of
   [ticket 001](tickets/001-inks.md) describes Mapper's overprinting simulation, which is the oracle of the simulated
   overprinting of Mapper maps. OCAD applies B only in its PDF export, and not in its own view (the same oracle).
5. **One palette stack for all maps**: the course and the map composite in one stack.

A ticket of a part of the architecture has structural acceptance criteria, and not only criteria of the appearance.
For example: pass 2 does not call the drawing code of the symbols; the drawing target gets only composited groups; a
rendering mode does not change the instances; tests check the instances, their count and their groups, apart from
the rendering.

## Waiting on @Pauli

- **The scope of the epic** (Nov 18). [Ticket 000](tickets/000-palette-stack.md) is a draft. @Pauli confirmed that
  the flattening is outside it, and that the SVG encoding waits until pass 2 is done.
- **The SVG encoding**, when pass 2 is done (Nov 18). Its place: an encoding read from the instances, or the SVG
  target of NotPurplePen fitted to them in the integration. And whether the work can use the documented findings or
  the code of `feature/svg-target`. The notes of the epic give the reasons.
- **Offering upstream.** No branch is offered upstream yet.
- **The review of the new specification** (Nov 21). @Pauli let the session write [Specification.md](Specification.md)
  again on its own, and he reviews it afterwards. These are the choices that the session made on its own, and not his
  decisions:
  - Section 02 refers to [OcadLines.md](../OcadLines.md) for OCAD's rules of the lines, and does not repeat them.
  - Section 03 states the rules of the structure of a line that the decisions of Nov 19 to 21 give: the quarters and the
    runs by quarter with the OCAD spacing, the runs by straight edge with the OCAD metric, and the borders of the double
    lines as offset paths.
  - Section 04 keeps the SVG examples of the text of Nov 17, with comments in the terms of the glossary. The clipping
    paths of the examples are named after the palette cutters.
  - Section 04 says that the templates, the image layer and the layout layer are drawn beside the palette stack, below
    it or above it, and does not say how. The epic leaves this to the ticket of pass 2.
  - Section 05 has the paragraph of Nov 17 on the overlaps in one palette color, and section 04 the rule that the union
    can stay implicit in the rendering.
  - The section numbers of the other documents now refer to the new sections. Where an old number names the same content
    in the new text, it stays: "luvut 02 ja 03" and "luku 04" in the epic. The decisions on the dashed lines of Nov 18
    in Progress.md, the epic and ticket 011, which the decisions of Nov 20 replace, still name section 03. The new
    section 03 keeps a clipped pattern fill as a periodic coverage, but it gives the selected elements as derived
    values, and not as single clipping objects. The figure numbers stay.
  - In the done tickets, the session changed only the numbers, with two exceptions. In ticket 004, "luku 01 antaa
    palettikerroksen ominaisuudet C, α, B, G ja M" is now "luku 02 antaa palettimerkinnän ominaisuudet C, α ja B". In
    ticket 005, the sentence on the clipping objects starts with "Luvun 03 mukaan".
  - The reading list of [GlossaryHandover.md](GlossaryHandover.md) names section 01 of the specification, and its
    section "After the glossary" says that the specification is written again and that @Pauli reviews it.

## Decisions

- **The tracking of the branches** (Nov 18). The palette-stack branches are tracked here, apart from
  `../NotPurplePenProgress.md`, as long as they are not in `dev`.
- **Tickets and pull request descriptions** (Nov 18). Every branch goes through a ticket and a pull request
  description, also when no pull request is made. A ticket names the fixes and features separated from it under
  "Erotetut alatehtävät".
- **Pass 2 replaces the direct drawing of the palette entries** (Nov 18). When pass 2 is done, the palette entries of
  a map are drawn from the instances only. The normal view is a rendering mode of pass 2. The drawing code of the
  symbols is the source of pass 1, through the collector, until the geometry methods of the symbols replace the
  collector. The notes of the epic
  ([000](tickets/000-palette-stack.md)) tell what the ticket of pass 2 resolves: the compositing of a group, the
  drawing of the templates and the image and layout layers beside the palette stack, and a new base for the
  comparison of the coverage. Only the highlights stay open, and they do not hold back pass 2.
- **The drawing targets of the work** (Nov 18). The drawing targets that matter to this work are Skia, PDF, printing
  and the coming SVG implementation. GDI+ and WPF are legacy, and Direct2D and iOS are not in use: of the drawing
  targets, the application that ships refers to Skia (`Map_SkiaStd`) and PDF (`Map_PDF`) only. Its printing draws the
  pages with Skia on Windows and as PDF on Linux and macOS.
- **The drawing does not use the symbols** (Nov 18). Pass 2 reads the instances only, so the drawing does not select
  the symbols and does not call their code. Only pass 1 uses the symbols. The structure of the passes (Nov 20) changes
  this: pass 2 reads the palette separations, which refer to the symbols, and it does not call the drawing code of the
  symbols (`Draw`).
- **The instances belong to the map** (Nov 18). Pass 1 depends on the map alone, and not on a rectangle or a
  resolution of a drawing. To keep to a rectangle is pass 2's work. How the instances follow the changes of the map
  is for the ticket of pass 2.
- **Pass 1 before the patterns** (Nov 18). Two tickets come before the ticket of the patterns: one makes pass 1
  apart from the drawing, and one replaces the collector with geometry methods of the symbols, with the instances of
  the collector as its oracle.
- **The base of the full runs** (Nov 18). The palette-stack branches, the features and the fixes alike, run in full on
  `base`, which holds upstream and the workflows and test tools of the fork. The fixes of `basewithfixes` stay out of
  the runs: pass 2 would conflict with `fix/overlapping-patterns` in `Map.DrawColor`, and the fixes move baselines that
  the work does not touch. A fix that a part needs is merged into `feature/palette-stack`, as
  `fix/ocad11-utf8-strings` is. `tools/push-wip.sh` and `tools/int-worktree.sh` use `base` by default.
- **A full run without a collecting run** (Nov 18). When a change cannot change the drawing, for example because it
  does not touch the code that the drawing runs, its full run is `tools/push-wip.sh` without `--collect`, since the
  images stay as they are. The drafts 008 to 013 tell when this applies to each of them.
- **The geometry methods in four parts** (Nov 18). The replacement of the collector is four tickets, by the kinds of
  symbols: the foundation and the point symbols (008), the lines (011), the areas, the combinations and the graphics
  objects (012), and the texts and the rectangles (013).
- **The old drawing code during the geometry methods** (Nov 18). The old drawing code stays as it is during the
  four tickets, so that the collector collects its calls and stays an independent oracle. The geometry methods are
  written beside it, and the logic that composes a symbol may be in two places.
- **The fixes before ticket 011** (Nov 19). The fix branches 015 to 020 are merged into `feature/palette-stack`
  before the work on ticket 011 starts, so that the old drawing and the collector follow OCAD's rules and the
  comparison stays exact. This changes the decision above for these fixes. The collector stays an independent
  oracle, as the geometry methods are new code.
- **The limit of fix 020** (Nov 19). OCAD's limit for the piece of a dash beyond a vertex is found first from a PDF
  export of the sixth dash sample, whose dashes have coordinates and no pixels. The rule of the fix is chosen after
  that ([020](tickets/020-dash-boundary-at-vertex.md)).
- **The rule of fix 020** (Nov 19). Purple Pen follows OCAD's rule as its PDF exports show it: the vertex next to the
  start or the end of a dash is left out of the dash when the piece between them is shorter than 0.4 times the width of
  a single line; on a double line, the dash of the middle leaves it out for a piece shorter than 0.05 mm, and each
  border then applies the rule of a single line to its own dash with the width of the border
  ([020](tickets/020-dash-boundary-at-vertex.md), [oracles/ocad-dashes.md](oracles/ocad-dashes.md)).
- **The dashed lines** (Nov 18). A dashed line follows the principle that section 03 of
  [Specification.md](Specification.md) gives for the pattern fills: the basic model is a standard dashed line, and the
  exotic cases, such as the distance metric of OCAD, are split into parts, for a dashed line into single dashes. In
  section 03, a clipped pattern fill stays repetitive geometry, and the fill modes that select complete elements and
  the nonperiodic fills become single clipping objects. The decision on the structure of a line (Nov 20) replaces this.
- **The composed dashed lines** (Nov 19). A dashed line is a composed dashed line: each part between corner and dash
  points is one periodic dashed line, and the merged dashes at those points and the end dashes of the line are lines
  of their own. The geometry method makes single dashes only where an object cannot be a composed dashed line. This
  makes the decision above specific for the dashed lines (ticket 011). The decision on the structure of a line (Nov 20)
  moves this to the encodings: a composed dashed line is the way an encoding writes a line for a channel whose dash
  pattern measures the euclidean length, and the encoding makes the runs from the measure of the model. The readings
  of Nov 20 change this: the runs are derivations, which `Map` offers the means to make and to keep
  ([Glossary.md](Glossary.md), *malli*).
- **The glyphs along the lines** (Nov 19). The evenly spaced glyphs of each part between corner and dash points are
  one repetition, on straight and curved parts alike: the shared shapes of the glyph, the first position, the step
  and the count. Each glyph turns with the path at its position. The glyphs at the corners, the dash points, the
  start and the end are single placements of the shared shapes, and stretched and diminishing glyphs get shapes of
  their own (ticket 011).
- **The measure of the benefit** (Nov 19). A structure of the instances is of benefit when it lets the rendering
  channels solve the basic cases with an abstraction that they offer (tickets 011 and 013).
- **The dash pattern** (Nov 19). The dash pattern and the phase of a run of a composed dashed line are properties of
  `Stroke`, as `stroke-dasharray` and `stroke-dashoffset` of SVG, the dash pattern of PDF and the dash effect of
  Skia (question 5 of ticket 011).
- **The repetition of the glyphs in the instances** (Nov 19). The repetition of the evenly spaced glyphs is in the
  instance: the path of the part, the shared shapes of the glyph, the first position, the step and the count. So an
  encoding can give it to an abstraction of its channel: the 1D path effect of Skia for any glyph, the dash pattern of
  SVG and PDF for dots and for ticks that are symmetric across the line, and the markers of SVG. Where no abstraction
  fits, the encoding expands it to placements (question 6 of ticket 011).
- **The runs with the OCAD metric** (Nov 19). With the OCAD metric and on the borders of double lines, the runs are by
  segment: the middle dashes of each segment are a run whose pattern is scaled to euclidean by the direction of the
  segment, and the dashes over a vertex are single. A straight segment is a basic case, which the dash pattern of the
  channels solves (question 7 of ticket 011).
- **The dashes on curves** (Nov 19, after the ninth sample). With the euclidean metric, the dashes on a Bezier curve
  are placed by its quarters, as OCAD places them: each quarter has its chord as its length in the measure, and within
  the quarter the dashes are in proportion to the true arc. It is fix [021](tickets/021-ocad-dashes-on-curves.md). The
  runs on a curve are by quarter, and the pattern of a run is scaled by the arc and the chord of its quarter. The
  curves with the OCAD metric stay as they are (question 8 of ticket 011,
  [oracles/ocad-dashes.md](oracles/ocad-dashes.md)).
- **The root of fix 021** (Nov 19). `fix/ocad-dashes-on-curves` is rooted at `fix/ocad-dash-count` (b99067b5), as the
  fix uses the measure of four chords of fix 017. It is offered upstream after fix 017.
- **The merge of fix 021** (Nov 19). `fix/ocad-dashes-on-curves` is merged into `feature/palette-stack` before the
  tickets 011 to 013, so that the composed dashed lines of ticket 011 get the placement by quarters.
- **What a trial build holds** (Nov 19). The trial builds on the site of NotPurplePen tell what is tried. For
  `feature/palette-stack`, the message of the commit that a push of `wip/linux/palette-stack` builds lists the ten
  work branches merged into it last, and the site shows the message under the commit of the build.
  `tools/push-wip.sh` writes the list.
- **The OCAD metric is best effort** (Nov 19). @Pauli: "vuonna 2026 tuntuu melko irrelevantilta käyttää paljon
  voimia käytännössä OCADin 8-version ja sitä vanhempien tukemiseen. OCADin metriikka voidaan siis toteuttaa 'best
  effort'-tyyliin." (In 2026, it seems quite irrelevant to put much effort into the support of OCAD 8 and older in
  practice, so the OCAD metric can be implemented as best effort.) Measurements of it can be made (@Pauli, Nov 19).
  Current OCAD measures the lines with the euclidean metric unless a map has the view parameter `p1`, and Mapper maps
  always do.
- **The text along lines** (Nov 19). Purple Pen measures the text along a line always with the OCAD metric, and so
  does current OCAD, also in a map with the euclidean metric: the PDF export of a sample shows it
  ([oracles/ocad-line-text.md](oracles/ocad-line-text.md)). Mapper maps have no text along lines in Purple Pen.
  Ticket [022](tickets/022-line-text-metric.md) is closed without a fix.
- **The OCAD metric on curves** (Nov 19). In a map with the OCAD metric, the drawing places the dashes, and the glyphs
  at lengths along the line, on a Bezier curve by the model of fix 021: each quarter of the curve has the length of
  its chord in the OCAD metric, and in a quarter the points are in proportion to the euclidean length of the arc. The
  straight segments do not change. So the drawing changes on the curves of the maps with `p1`, and their baselines
  move. This changes the decision on the curves with the OCAD metric (question 8 of ticket 011). The change is an
  extension of fix 021 (@Pauli, Nov 19), and it is not made yet.
  - The measurement: in the PDF exports with the OCAD metric, the model is within 0.013 to 0.096 mm of OCAD's dashes on
    16 curves of the ninth sample and 0.175 mm on its loop, and within 0.008 to 0.034 mm on the curves of the eighth
    sample. Fix 021 fits the euclidean files as well. Purple Pen's present placement is 0.019 to 0.252 mm from OCAD,
    and 0.794 mm on the loop
    ([oracles/ocad-dashes/measure-pdf-curves-metric.py](oracles/ocad-dashes/measure-pdf-curves-metric.py)).
    So OCAD probably places the dashes on a curve by one rule with each metric, and the metric changes only the chords
    of the quarters. The tenth sample confirms the model on long paths of several curves, with straight segments,
    corner and dash points, on a closed path and on three real paths of a test map, and for the spaced glyphs: with
    the OCAD metric, within 0.059 mm of OCAD, where the present placement is up to 0.269 mm off
    ([oracles/ocad-dashes/measure-pdf-paths.py](oracles/ocad-dashes/measure-pdf-paths.py)).
  - The observation: the OCAD metric is a norm whose unit circle is an octagon. Along a direction, it measures a
    euclidean length of 1 as 1.000 to 1.118. On a straight segment, an even euclidean step gives its points exactly.
    On a curve, no euclidean measure gives them, and no transformation of the path does.
  - The reasons: the model is OCAD's rule as far as the samples show it, and it is nearer to OCAD than the present
    placement. Each straight segment and each quarter has an even euclidean step, so each run and each repetition can
    be euclidean, and a channel can draw it with an abstraction of its own. A dash pattern with a table of lengths
    could keep the present placement, but that placement is farther from OCAD, and the 1D path effect of Skia takes
    no table.
  - In the questions 6 and 7 of ticket 011, a vertex was a point of the flattened path, so the dashes and the glyphs on
    a curve with the OCAD metric were single. @Pauli took a vertex to be a corner point that the user puts on the
    curve.
  - [OcadLines.md](../OcadLines.md) specifies OCAD's lines and gives the details of this decision.
- **The repetition of the glyphs on curves** (Nov 20). On a Bezier curve, the glyphs of the OCAD spacing repeat by
  quarters. Fix [021](tickets/021-ocad-dashes-on-curves.md) places them by quarters, as it places the dashes, so the
  quarters have different steps. This changes the decision on the glyphs along the lines (Nov 19) for the OCAD spacing
  (ticket 011).
- **The placements give positions** (Nov 20). Each clipping object has a shape that is drawn, a fill or a stroke, and
  a placement. A placement gives one or more positions, and each position is a point and an angle. A single placement
  keeps one point and an angle. A repetition keeps its path, its first position, its step and its count, and it
  calculates the angles of the positions from the direction of the path with the method of the drawing. Each reader
  asks a placement for its positions, and only an encoding that gives a repetition to an abstraction of its channel
  reads the path and the step. A placement keeps its sequence of steps, and the position is the innermost step. So the
  repetition of the glyphs is a placement, and its parts are usual filled and stroked shapes (ticket 011). In
  `feature/line-geometry`, the repetition is still a kind of clipping object (`RepeatedShapes`).
- **The ideal forms** (Nov 20). The map model keeps the ideal forms. On a curve, the dashes, the runs of dashes and
  the paths of the repetitions are Bezier pieces of the line, as a solid line is. An encoding adapts to its channel:
  where the implementation of the channel does not agree with the model, the encoding itself makes, for example, a
  polyline (ticket 011). In `feature/line-geometry`, they are still polylines of the flattened path. The readings of
  Nov 20 change this: `Map` keeps the ideal forms, and the derivations are practical adaptations of them. The
  derivations keep the dashes, the runs and the paths of the repetitions as Bezier pieces, as polylines or as both, as
  the implementations that the channels choose need ([Glossary.md](Glossary.md), *malli*).
- **The borders of double lines** (Nov 20). The shape of a border is an offset path in its ideal form: the base path
  and the offset. Its stroke is a usual stroke. The dash pattern of an offset path is measured along its base path. So
  the dashes of a border are the dashes of the base path moved aside, as in OCAD, and the runs of a border are the
  runs of the base path moved aside. The rule of the corners of the offset belongs
  to the shape, and the join of the stroke to the stroke. This changes the decision on the runs with the OCAD metric
  (question 7 of ticket 011) for the borders of double lines. In `feature/line-geometry`, a border is a polyline that
  offsets the flattened path.
- **The model keeps the information of the encodings** (Nov 20). When an encoding needs information, the model keeps
  it, and the encoding reads the structure from the model (ticket 011).
- **The structure of a line** (Nov 20). The model keeps the structure of a line in its ideal form, also what PDF and
  SVG cannot describe: the base path, the parts as its pieces and offsets, the measure of the line, the fitted
  pattern of a part, the end dashes and the exceptions of fix 020. An encoding converts the structure for its channel:
  in a basic case directly into a structure of the channel, and otherwise by splitting it. This replaces the decision
  of Nov 18 that the exotic cases are split into single dashes in the instances (ticket 011).
- **Self-supporting vector files** (Nov 20). The goal is that an SVG or a PDF file carries the model. The drawn
  objects are closely linked to the parts of the model, so that the parts of the model are reachable through the
  drawn objects. What the structure of the model lets an object refer to is not duplicated in the drawn objects: they
  use it through references. Then a file can one day be opened in Purple Pen again, and Purple Pen reads the model
  from the file and does not recognize it from the geometry of the drawing. The opening is not in the scope of the
  epic. The epic includes a model that makes it possible: the model keeps its structure in its ideal form, and its
  parts and its shared structures are objects to which an encoding can refer ([000](tickets/000-palette-stack.md)).
- **The conversions of the model** (Nov 20). The model offers the known conversions of its structure into forms that
  the channels can use, as calculations. It does not keep their results. An encoding chooses a conversion that its
  channel can use. The conversions concern the whole model, not only the lines. Conversions of the lines are, for
  example, the runs and the single dashes in the euclidean measure, a table of dashes, repetitions with a euclidean
  step, the pieces of the base path, offset curves, and the expansion into single dashes and placements
  ([000](tickets/000-palette-stack.md), ticket 011). Conversions of the pattern fills are, for example, the placements
  of the elements of an irregular pattern, whose randomness the model keeps as a definition, and a tile of a pattern,
  which the conversion composes from single elements. The structure of the passes and the readings of Nov 20 change
  this: `Map` offers the known channel conversions as means to make derivations from its ideal forms, and keeps their
  results where the shared use of the channels or a cache makes it sensible. An encoding chooses a channel conversion
  that its channel can use, and makes with channel conversions of its own only what no channel conversion of `Map`
  covers ([Glossary.md](Glossary.md), *malli*).
- **The structure of the passes** (Nov 20). `Map` is the source. Pass 1 makes from the map alone the palette
  separations (`PaletteSeparation`) of the symbol separations (`SymbolSeparation`), and their palette cutters
  (`PaletteCutter`). A symbol separation is the triple (`Symbol`, `SymColor`, G). The symbol separations of a palette
  separation that have the same configuration are one palette cutter, the layer instance of the specification. `Map`
  offers the means to make derivations, and keeps them where the shared use of the channels or a cache makes it
  sensible. Pass 2 writes each palette separation as one group. Section 01 of [Specification.md](Specification.md)
  gives the structure, and [Glossary.md](Glossary.md) gives the names.
- **Earlier decisions are negotiable** (Nov 20). The structure of the passes changes the concepts at the base of the
  work, so each earlier decision is negotiable when it is necessary. The structure touches at least the decisions "The
  source of the clipping objects of pass 1" (Nov 18) and "Pass 2 replaces the direct drawing of the palette entries"
  (Nov 18). [Glossary.md](Glossary.md) gives the readings of the decisions of Nov 20 that say "the model" (*malli*).
- **The names** (Nov 20). [Glossary.md](Glossary.md) gives the rules of the names and the names of the concepts.
- **The future of `Map` and the SVG channel** (Nov 21). `Map` and the SVG channel are ported to TypeScript, into a
  browser, where editing functions of the map may also be wanted. The courses also update in the browser as mutations
  of the SVG DOM.
- **The symbol key** (Nov 21). A `Symbol` has a symbol key (`Symbol.Key`), which is its identity. The maker of the
  `Symbol` gives it. In a `Map` that is read from a file or edited, `Map` gives it from a counter whose values it does
  not use again. When an edit replaces a `Symbol`, the replacing `Symbol` gets the same symbol key. The undo restores
  it, and a self-supporting vector file carries it. In the course map, `CourseLayout.RenderToMap` makes it from
  `EventDB`: from the identity of the `CourseObj` that made the `Symbol`, and from the index of the `Symbol` among the
  symbols of that `CourseObj`. So a course map that is made again gives the same symbol keys to the same `CourseObj`.
  The reference of a drawn object also gives the `Map`. The content of the identity of a `CourseObj` is open. The
  basis is the future of `Map` and the SVG channel: the mutations of the SVG DOM need keys that stay the same over the
  edits and over the course maps that are made again. [Glossary.md](Glossary.md) gives the terms.
- **The same configuration** (Nov 21). Two configurations are the same when they give the same coverage: the same
  palette color and coverage prototype, and M₁⁻¹M₂ is a symmetry of G. A normal form decides it: the translation of M
  is reduced into a fundamental domain of the translations that are symmetries of G, and rounded to a fixed precision.
  The union of the clipping objects of a palette cutter is a true union. This answers question 1 of ticket 009, whose
  proposal, the same parameters, is a sufficient condition only. [Glossary.md](Glossary.md) gives the terms.
- **The hatchings and `ManualOnly`** (Nov 21). A hatching is a coverage prototype of its own: stripes, whose
  symmetries include all the translations along the lines (question 3 of ticket 009). In the specification,
  `ManualOnly` is the same as `Always`: M takes the angle and the rotation center of the `AreaSymbol`, and only `Never`
  leaves them out, as the drawing of upstream does. The difference is in the editing (question 4 of ticket 009).
- **The measure of the dash pattern of a pen** (Nov 21). `Stroke.DashPattern` and `Stroke.DashPhase` are in the
  measure of the channels, euclidean along the own path of the stroked shape, and they belong to the runs of dashes,
  which are derived values. The dash structure in the measure of the line is the fitted pattern of a part of a line,
  and not a part of `Stroke`. This settles the dash pattern of question 5 of ticket 011 after the decisions of Nov 20.
- **The channel conversions of the encodings** (Nov 21). `Map` offers all the geometric conversions, and their results
  are forms of `Map`: shapes, strokes, placements and polylines, also the arcs as Bezier pieces. The own conversions of
  an encoding are only the conversions into the vocabulary of its channel and the adaptations to the deviations of its
  channel that no general conversion covers. [Glossary.md](Glossary.md), *kanavamuunnos*, gives the comparison with the
  legalization of a compiler.
- **The encodings and `IGraphicsTarget`** (Nov 21). An encoding writes its channel directly: the SVG encoding writes
  SVG, the PDF encoding the objects of PdfSharp, and the Skia encoding an `SKCanvas`. `IGraphicsTarget` stays beside
  the palette stack. It draws what does not belong to the palette stack: the templates, the highlights and the control
  descriptions, and the old drawing code until that code goes away. So the highlights stay a direct drawing. The
  encoding and the graphics target of one channel share the surface of the channel. The vocabulary of a channel belongs
  to its encoding, and a shared drawing interface would level it to the subset that all the channels share.
- **The identity of a `CourseObj`** (Nov 21). The identity of a `CourseObj`, from which the symbol key of the course
  map comes, is a key that `CourseFormatter` or `TopologyFormatter` gives to it at the place where it makes it: the
  role of the place in the data of `EventDB`, the identifiers of `EventDB` that the place uses, and the `CourseLayer`.
  Each place has a role of its own. The key is apart from the fields that the selection uses (`controlId`,
  `courseControlId`, `courseControlId2` and `specialId`). These fields are associations and not identities: a control
  circle and its control number, or the two parts of a leg that is flagged in part, have the same associations, and
  the map issue point of a leg has none on purpose. The class of the `CourseObj` is not a part of the key, so a whole
  leg keeps its key when it becomes flagged. Its name is `CourseObj.Key`. [Glossary.md](Glossary.md) gives the terms.
- **The division of the specification** (Nov 21). [Specification.md](Specification.md) is written again in the terms of
  [Glossary.md](Glossary.md) and by the structure of the passes, and it is divided as the glossary is. The unnumbered
  opening sections come first. Section 01 gives the structure of the passes as an overview, with figure 1. Section 02
  gives the map side, section 03 the intermediate layer, section 04 the rendering side, and section 05 the transparency
  flattening. The references [1] to [16] come last and keep their numbers. The work has five parts: the opening sections
  with section 01, section 02, section 03, section 04, and section 05 with the references. The new text grows at the
  start of the file, and the old text stays below it until the last part. Until then, the section and figure numbers
  that the other documents give refer to the old text. The last part changes them to the new sections and figures.
- **The selected elements in the specification** (Nov 22). Section 03 of [Specification.md](Specification.md) gives the
  elements that a fill mode selects as derived values of a palette color coverage of a symbol. It does not say to which
  palette cutter they belong, which question 2 of ticket 009 asks. It leaves the composite representations of the
  selected elements to a later refinement, as the text of Nov 17 did.
- **The line breaks of the specification** (Nov 22). [Specification.md](Specification.md) wraps its lines at 120
  characters, as README.md, Progress.md, the glossary, its handover and [OcadLines.md](../OcadLines.md) do. Inside a
  paragraph, a line break is a soft line break, which the renderer shows as a space. So the line breaks show only in the
  source and in the diffs.
- **General words** (Nov 22). A general word, for example *model*, can have its general meaning in the documents, when
  it does not name a concept that the specification defines. Where a general word means a concept that the specification
  defines by another name, the documents use that name. So the preface keeps "not a model of the implementation", and
  the opening sections of [Specification.md](Specification.md) say "architecture" for the architecture that the
  specification defines.
- **The map model in the preface** (Nov 22). In the preface of [Specification.md](Specification.md) and in
  [README.md](README.md), "upstream's map model" is "upstream's `Map` and its consumers", and "the model" is "`Map` and
  its consumers". The consumers of `Map` are the code that draws it: in upstream `MapDisplay`, and through it the
  printing, the PDF and the bitmap export, and in the specification the rendering side.
- **No dates in the specification** (Nov 22). [Specification.md](Specification.md) gives no dates and no attributions,
  also in its preface, as nobody asked for them there. Progress.md gives the decisions with their dates. The citations
  [1] to [16] stay.
- **The name of the architecture and of the work** (Nov 22). The architecture is named after the palette stack, the
  transparency groups of the palette colors in palette order. The palette stack takes the place of *palette layer* in
  the names. The title of [Specification.md](Specification.md) is "Vector graphics architecture of the palette
  stack", and [Glossary.md](Glossary.md) is the glossary of the palette-stack architecture. The work is the
  palette-stack work, in Finnish *palettipinotyö*, its folder is `doc/devdocs/PaletteStack`, and the title of the
  epic is "Palettipino upstreamin karttamalliin".
- **The name of the branch of the epic** (Nov 22). The branch of the epic is `feature/palette-stack`. @Pauli
  renamed it on GitHub, which redirects the web addresses of the old name to the new one. The proxy of the cloud
  session refused the call of the GitHub API that renames a branch (HTTP 403). The ticket of the epic is
  [tickets/000-palette-stack.md](tickets/000-palette-stack.md), and the base baselines of its runs are in
  `prs/000-palette-stack-base/`. Its full runs go on `wip/linux/palette-stack`. The earlier runs are on
  `wip/linux/palette-layers`, and the tag `trial-palette-layers` holds the earlier trial build. The brief of the
  session of Nov 22 let it push only to `docs/process-coordination`, and @Pauli decided that this does not bind.
- **The order of the figures** (Nov 22). Section 01 shows the structure of the passes first, as figure 1, and then
  the coverage and the color, as figure 2, so the overview comes before the details. The other figures keep their
  places and their numbers 3 to 8. The labels and the captions of the figures use the terms of the glossary. @Pauli
  left the choice to the session.
- **The motive in the specification** (Nov 22). [Specification.md](Specification.md) states the motive of the work
  in its opening text: the future of `Map` and the SVG channel (Nov 21). The motive guides the implementation choices
  that the specification does not define. Section 02 gives the need of the keys that comes from it, the mutations of
  the SVG DOM in a browser.
- **The choices of the session that stay** (Nov 22). The opening text of the specification names the channels Skia,
  PDF and SVG. Section 01 gives the units in a table and the rules of the structure as a list. Section 02 gives the
  properties of a palette color in a table, and says that G and M do not belong to the palette color. These are
  choices of the session that @Pauli let stand.
- **The framing line of texts** (Nov 19). The framing of a text run is one line in the instances, a stroke of the full
  width with the joins of the symbol, as in OCAD's PDF export. OCAD's own drawing leaves the same holes as Skia and
  Cairo with one pen, so the holes that the intermediate pens of Purple Pen fill are no faults against OCAD (question
  2 of ticket 013, [oracles/ocad-framing.md](oracles/ocad-framing.md)).
- **The text runs** (Nov 19). Each word, or each grapheme where the character spacing is more than 0, is a text run
  at its own position, as the drawing and the collector make them. The channels take positioned runs as they are
  (question 1 of ticket 013).
- **The shared shapes of the glyphs** (Nov 18). The point symbols share the shapes of their glyph: each part of the
  glyph is one shape, which each symbol places with its own placement (ticket 008).
- **The collector as the oracle until ticket 009** (Nov 18). The collector is the oracle of the tests during the
  tickets 008 to 013. Ticket 009 removes the internal option `omitHatchingsAndPatterns` and ends the use of the
  collector as the oracle, and the coverage comparison then compares the colors with hatchings and patterns too.
- **The conflicts with dev** (Nov 18). The palette-stack branches may conflict with `dev`. A comparison of their
  implementation with the one in `dev` is a later investigation.
- **The fixes of upstream that a ticket separates** (Nov 18). When the work of a ticket fixes a fault of upstream, the
  fix gets a ticket of its own, from `tickets/bug-template.md`, and a `fix/` branch, and the first ticket names it
  under "Erotetut alatehtävät".
- **The epic and its branch** (Nov 18). Ticket 000 is the epic of the work, from `tickets/epic-template.md`.
  `feature/palette-stack` collects the parts that are done, as `feature/svg-target` collects the SVG work. A merge
  into it names the work that it brings and lists the commits, as a merge into a shared branch does.
- **The screens of the process inks** (Nov 18). Nobody prints from OCAD 6 to 8 files now. So the process inks are the
  same for all maps and have no halftone screen. OCAD 6 to 8 give screens to the process inks in their symbol header,
  and the screens stay in it through an OCAD 6 to 8 round trip.
- **No fallback for files that Purple Pen wrote** (Nov 18). Purple Pen does not read its course files back as maps.
  So `fix/ocad11-utf8-strings` reads an OCAD 11 or later file in UTF-8 only, also where an earlier Purple Pen wrote
  the file in the 8-bit code page.
- **The text dumps of the OCAD course files** (Nov 18). The OCAD export writes the transparency (`t`) in each color of
  OCAD 9 and later, as OCAD does. So the 30 expected text dumps of the OCAD 9 and 11 course files of `ControllerTests`
  get `t100` in each color. The criterion of ticket 004 that no baseline moves meant the image baselines, and the
  change of the dumps is a justified correction.
- **The source of the clipping objects of pass 1** (Nov 18). The data structure of the instances is the interface
  between pass 1 and pass 2: pass 1 makes it, and pass 2 and the tests of the instances read it. Its clipping objects
  are geometry: a filled shape with its fill rule, a stroked shape with the width, caps, joins and miter limit of its
  line, or a text run, each with an affine placement. The collector is the adapter of pass 1 between the drawing code
  of the symbols and this data structure: it implements `IGraphicsTarget` and turns the drawing calls of the symbols
  into instances. Geometry methods of the symbols can later make the same instances directly, in place of the
  adapter. If the collector becomes needlessly complex, the work makes the instances with geometry methods of the
  symbols at once.
