# Palette layers: progress

Nov 18, 2026. This file is the handover of the palette-layer work. It tells where the work is, how to continue it,
what comes next, what waits on @Pauli, and the decisions. The branches are not in `basewithfixes` or `dev`, so
`../NotPurplePenProgress.md` does not track them (@Pauli, Nov 18). Update this file when a branch changes state and
when @Pauli makes a decision. It tells the state, not the events: the commits and the runs record those.

- [README.md](README.md) tells how the work is organized: its tickets, branches, pull request descriptions and
  tracking.
- [Specification.md](Specification.md) is the reference of the architecture, and the epic
  [000](tickets/000-palette-layers.md) gives the goal and the scope of the work.

## Where the work is

The parts 001 to 005 are done and merged into `feature/palette-layers`, and its full run passed. Pass 1 makes the
instances with full coverage (G = 1) of each palette entry ([005](tickets/005-layer-instances.md)). The fix
[006](tickets/006-matrix-equals-offsets.md), which the tests of 005 found, is done on a branch of its own, and no part
needs it. The next part, the patterns of pass 1, has no ticket yet (see "Next").

## Branches

`feature/palette-layers` collects the parts that are done. It is rooted where `base` meets upstream (d48f4456), as
is each part that builds on no other part; a part that builds on earlier parts is rooted at `feature/palette-layers`.
Each branch runs in full on `wip/linux/<name>`: a feature on `basewithfixes`, and a fix alone on `base`. No branch
is offered upstream yet.

| Branch | Ticket and description | State |
| --- | --- | --- |
| `feature/palette-layers` | [000](tickets/000-palette-layers.md) | The branch of the epic. It merges `feature/inks` (a0f0ed6f), `fix/ocad11-utf8-strings` (e51e395d), `feature/opacity-and-blend-mode` (10dd7511, e1101c42) and `feature/layer-instances` (c98d380e). Full run passed at c98d380e (`wip/linux/palette-layers`: Tests 44259000556, Build 44259000493). Its merge into `dev` conflicts in `Map_Skia.Tests/RenderingTests.cs`, and the changes of `feature/layer-instances` conflict with `dev` in `Map.cs` and `SymDef.cs`. |
| `feature/layer-instances` | [005](tickets/005-layer-instances.md), [description](prs/005-layer-instances.md) | All criteria met. Full run passed at fb451340 (`wip/linux/layer-instances`: Tests 44257645995, Build 44257646116). No image or text baseline moves (collecting run 44256257076, of 2e770ad4, wrote no image). In `feature/palette-layers` (c98d380e). |
| `fix/matrix-equals-offsets` | [006](tickets/006-matrix-equals-offsets.md), [description](prs/006-matrix-equals-offsets.md) | Full run passed at 5e827f47 (`wip/linux/matrix-equals-offsets`, on `base`: Tests 44256623574, Build 44256623572). Its test `MatrixTests.EqualsItsCopy` fails without the fix. Separated from 005, whose tests compare matrices element by element. No part needs it, and it is in no other branch. The work of 005 does not fix the fault, so the rule of the separated fixes does not cover the branch, and it is kept as it is (@Pauli, Nov 18). |
| `feature/opacity-and-blend-mode` | [004](tickets/004-opacity-and-blend-mode.md), [description](prs/004-opacity-and-blend-mode.md) | All criteria met. Full run passed at 7882bca8 (`wip/linux/opacity-and-blend-mode`: Tests 44222985536, Build 44222985519). No image baseline moves (collecting run 44223000991). The 30 text dumps of the OCAD 9 and 11 course files of `ControllerTests` get `t100` in each color (@Pauli, Nov 18). Its Mapper test map `openmapper/LordHill.omap` is `LordHill.ocd` as Mapper saved it (@Pauli, Nov 18). In `feature/palette-layers` (e1101c42). |
| `feature/inks` | [001](tickets/001-inks.md), [description](prs/001-inks.md) | All criteria met. Full run passed at a75c49aa (`wip/linux/inks`: Tests 44171873489, Build 44171873441). No existing baseline moves (collecting run 44155029726); the new Skia test `MapperOverprinting` has the baseline `skia_render/hakolaoverprinting.png`, from Tests run 44158232814. It merges `fix/mapper-export-color-numbers`. In `feature/palette-layers` (a0f0ed6f). |
| `fix/mapper-export-color-numbers` | [002](tickets/002-mapper-export-color-numbers.md), [description](prs/002-mapper-export-color-numbers.md) | Full run passed (`wip/linux/mapper-export-color-numbers`, on `base`: Tests 44171502072, Build 44171502166). Its test `ExportNumbersColorsByPlace` fails without the fix. In `feature/inks` (a75c49aa), which had the same fix as 91994366. |
| `fix/ocad11-utf8-strings` | [003](tickets/003-ocad11-utf8-strings.md), [description](prs/003-ocad11-utf8-strings.md) | Full run passed (`wip/linux/ocad11-utf8-strings`, on `base`: Tests 44196794627, Build 44196794547). No baseline moves (collecting run 44176064791, of an earlier form of the fix). Its test file is the ISSprOM 2019 symbol set of the Finnish Orienteering Federation, which the federation gives out with no statement about redistribution (@Pauli, Nov 18: little harm in it). In `feature/palette-layers` (e51e395d). |

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
  section 05 of [Specification.md](Specification.md), @Pauli, Nov 18).
- **Where the work runs.** This is @Pauli's machine; no Windows machine exists here. A build or a test counts only
  when it ran in GitHub Actions, on a `wip/` branch. The .NET SDK is `~/.dotnet/dotnet`, outside `PATH`.
- **The checkouts.** The clone `~/Documents/Projects/NotPurplePen` stays on `docs/process-coordination`: do not change
  its branch. Each branch has a worktree in `~/Documents/Projects/npp-worktrees/`: `palette-layers`, `inks`,
  `fix-mapper-colors`, `fix-ocad-utf8`, `opacity-and-blend-mode`, `layer-instances` and `fix-matrix-equals`. The worktrees whose names end in `-int` hold the
  local integration branches `local/<name>-int`, which are never pushed. `tools/int-worktree.sh` makes them again from
  GitHub.
- **Local tests.** `Test_MapModel` targets Windows; `tools/run-map-tests.sh` runs its files on Linux, and skips the
  renderings, which GDI+ draws. `Map_Skia.Tests` runs on Linux as it is: set `TEST_SILENTRUN=True`, or a failed
  comparison opens VisualDiff and waits. A local rendering tells which images a change moves. The fonts of this machine
  are not the workflow's, and a Skia comparison allows no difference, so each baseline comes from a run in GitHub
  Actions (`tools/push-wip.sh --collect`, or `--blank-baseline` for a new Skia test).
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

The parts come in the order of the two passes of the specification (section 02), so that each part builds the
structure and not only the appearance. Each part builds on the parts before it, so its branch is rooted at
`feature/palette-layers`.

1. **Pass 1, the patterns** (section 03). It has no ticket yet. The hatchings and the area patterns become prototypes
   with their lattice and element transformations, and the elements that a fill mode selects become clipping objects.
   The pass 1 of [ticket 005](tickets/005-layer-instances.md) leaves them out with the internal option
   `RenderOptions.omitHatchingsAndPatterns`. A regular clipped pattern that the options draw with a pattern brush
   gives its lattice in `CreatePatternBrush`. The other patterns place each glyph with a transformation of its own,
   and a pattern that keeps whole glyphs draws them without a clip, as a point symbol draws its glyph. Their lattices
   and element transformations are in the definition of the area (`HatchInfo` and `PatternInfo` of `AreaSymDef`) and
   in the angle of the area object, and the drawing calls do not show them.
2. **Pass 2, the composition by palette entry** (section 05). It reads the instances only, groups them by palette
   entry, and applies C, α and B once to each group, in the order of the palette. α and B are in the model
   (`SymColor.Opacity` and `SymColor.BlendMode`, [ticket 004](tickets/004-opacity-and-blend-mode.md)), and the
   drawing does not use them yet. For an OCAD map, B = Darken is the larger share of each ink, and not the RGB minimum
   ([oracles/ocad-darken.md](oracles/ocad-darken.md)). Mapper applies α to each shape, so it is not the oracle of α
   where the shapes of one color overlap ([oracles/mapper-opacity.md](oracles/mapper-opacity.md)). A clipping object
   keeps the steps of the transformations that placed it (`Placement`), so pass 2 can apply them as the drawing of the
   symbols did, with the same rounding.
3. **The rendering modes** (sections 06 and 07): the normal view, the overprinting that the inks simulate, and the
   separations, as color configurations of pass 2. All modes use the same instances. The section "Rajaus" of
   [ticket 001](tickets/001-inks.md) describes Mapper's overprinting simulation, which is the oracle of the simulated
   overprinting of Mapper maps. OCAD applies B only in its PDF export, and not in its own view (the same oracle).
4. **One palette stack for all maps**: the course and the map composite in one stack.

A ticket of a part of the architecture has structural acceptance criteria, and not only criteria of the appearance.
For example: pass 2 does not call the drawing code of the symbols; the drawing target gets only composited groups; a
rendering mode does not change the instances; tests check the instances, their count and their groups, apart from
the rendering.

## Waiting on @Pauli

- **The scope of the epic** (Nov 18). [Ticket 000](tickets/000-palette-layers.md) is a draft. @Pauli confirmed that
  the flattening is outside it, and that the SVG encoding waits until pass 2 is done.
- **The SVG encoding**, when pass 2 is done (Nov 18). Its place: an encoding read from the instances, or the SVG
  target of NotPurplePen fitted to them in the integration. And whether the work can use the documented findings or
  the code of `feature/svg-target`. The notes of the epic give the reasons.
- **Offering upstream.** No branch is offered upstream yet.

## Decisions

- **The tracking of the branches** (Nov 18). The palette-layer branches are tracked here, apart from
  `../NotPurplePenProgress.md`, as long as they are not in `dev`.
- **Tickets and pull request descriptions** (Nov 18). Every branch goes through a ticket and a pull request
  description, also when no pull request is made. A ticket names the fixes and features separated from it under
  "Erotetut alatehtävät".
- **Pass 2 replaces the direct drawing of the palette entries** (Nov 18). When pass 2 is done, the palette entries of
  a map are drawn from the instances only. The normal view is a rendering mode of pass 2. The drawing code of the
  symbols stays the source of pass 1, through the collector. The notes of the epic
  ([000](tickets/000-palette-layers.md)) tell the points of pass 2 that are open.
- **The conflicts with dev** (Nov 18). The palette-layer branches may conflict with `dev`. A comparison of their
  implementation with the one in `dev` is a later investigation.
- **The fixes of upstream that a ticket separates** (Nov 18). When the work of a ticket fixes a fault of upstream, the
  fix gets a ticket of its own, from `tickets/bug-template.md`, and a `fix/` branch, and the first ticket names it
  under "Erotetut alatehtävät".
- **The epic and its branch** (Nov 18). Ticket 000 is the epic of the work, from `tickets/epic-template.md`.
  `feature/palette-layers` collects the parts that are done, as `feature/svg-target` collects the SVG work. A merge
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
