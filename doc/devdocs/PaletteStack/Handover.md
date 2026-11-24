# The palette-stack work: handover

Nov 24, 2026. A new session reads this file to continue the work. It tells the present state, the next steps, what
waits on @Pauli, the decisions that bind the next steps, and how the work is done. [Record.md](Record.md) keeps the
rest: the full runs, the measurements, the history of the branches and all the decisions with their dates. Read the
record only when a question needs it, and then by search (`AGENTS.NotPurplePen.md`, "The handover of a piece of
work"). When a step is done, the same commit takes its notes out of this file.

## Read first

1. [../Introduction.md](../Introduction.md) and `AGENTS.NotPurplePen.md`: what NotPurplePen is, and the rules of all
   work, among them how to make a question.
2. [README.md](README.md): how the work is organized in tickets, branches and pull request descriptions.
3. This file.
4. The preface and sections 01 to 04 of [Specification.md](Specification.md), and [Glossary.md](Glossary.md) with
   [GlossaryHandover.md](GlossaryHandover.md).
5. The ticket of the step at hand, and the tickets that it names. [tools/README.md](tools/README.md) tells the loop of
   a ticket.

## The state

`feature/palette-stack` at 0d2148b3 holds pass 1 and pass 2 with the Skia encoding, and its full run passed
(`wip/linux/palette-stack` at 78770fb2: Tests 45405076724, Build 45405076683). The next steps build on it. It holds
these tickets, each with its branch and with its description in `prs/`:
- 001 the inks, with 002 the color numbers of the Mapper export; 003 the UTF-8 strings of OCAD 11; 004 the opacity
  and the blend mode.
- 015 to 021: the fixes of the glyph circles and of the dashes of OCAD's lines.
- 023 the palette separations; 024 the point symbols; 025 the symbol keys; 026 the course object keys.
- 011 the line symbols, 012 the area symbols and the graphics objects, 013 the texts and the rectangles: each symbol
  definition has a geometry method.
- 009 the coverage prototypes of the hatchings and the area patterns. The collector is removed.
- 010 pass 2 and the Skia encoding, with 028, the pattern fill of an area cluster once in the union of its areas.

Ticket [027](tickets/027-ocad-blend-mode-tests.md), the tests of OCAD's blend modes and opacity, is done. Its oracle
[oracles/ocad-blend-modes.md](oracles/ocad-blend-modes.md) gives how OCAD's view, its PNG export and its PDF export
apply B and α, and [Record.md](Record.md), "The results of ticket 027", what the PDF encoding and the rendering modes
need of it.

`basewithfixes` is at 78bc360b1, without `fix/overlapping-patterns`. `dev-next` at a777b4aab is the new dev: the
new `basewithfixes`, `base` and `feature/palette-stack`, with the baselines that their merges move, and its full run
passed (`wip/linux/dev-next`: Tests 45503251403, Build 45503251564). It replaces `dev` on @Pauli's word
([Record.md](Record.md), "Branches").

Ticket [029](tickets/029-ocad-float-strings.md), the CMYK shares and the font sizes of the layout texts that the OCAD
export writes with the error of the float computation, is approved. It is not made now, as it does not affect the
work that is under way (@Pauli, Nov 24).

The fixes 006 and 014 are on branches of their own, and no part needs them. The tickets 005, 007 and 008 are replaced
by 023 to 026, and the old branch of the epic, `feature/palette-layers`, keeps their pass 1.

## Next steps

1. The draft [030](tickets/030-svg-encoding.md), the SVG export in the new dev with the SVG encoding, waits for
   @Pauli's approval. Its branch `feature/svg-encoding` is rooted at `dev-next`. Its full runs go on
   `wip/linux/svg-encoding` with `--base dev-next`, and its SVG loop on `svg/svg-encoding`
   (`../SvgDevLoop/README.md`).
2. The ticket of the common structure of the vector files with the content of `Map`, and of the flag that leaves the
   content out, after ticket 030. Ask its questions early, and give it to @Pauli for approval. The PDF ticket uses the
   same structure.
3. The ticket of the PDF encoding, which ticket 010 separated: the same structure of the vector files. The oracle of
   ticket 027 gives OCAD's appearance of B and α in PDF.
4. The tickets of the Skia abstractions for the runs of dashes and the repeated placements, and of the anti-aliasing
   of the pattern fills in the Skia encoding, which ticket 010 separated, are not written.
5. The tickets of the rendering modes (the overprinting that the inks simulate, the ink separations and the blend mode
   by ink) and of one palette stack for all the maps are not written (ticket 010, "Rajaus").

A draft is implemented only after @Pauli approves it as a ticket.

## Waiting on @Pauli

- **The scope of the epic.** [Ticket 000](tickets/000-palette-stack.md) is a draft. The transparency flattening is
  outside it.
- **Offering upstream.** No branch is offered upstream yet.
- **The change of `dev`** to `dev-next`, with a merge of the old `dev` into it with the strategy `ours`.
- **The approval of the draft 030**, the SVG export in the new dev with the SVG encoding.

## The decisions that bind the next steps

The architecture is in [Specification.md](Specification.md), which @Pauli approved. A change of it is his. These
decisions of the process are in force. [Record.md](Record.md), "Decisions", gives each with its date and its reasons.
- **The architecture comes first.** `feature/multiply-blending` is an oracle of the appearance only: the work does not
  read its code, and does not take its way, a layer around the drawing of the symbols.
- **The session resolves matters** when the knock-on effects of its choice are not unreasonable, and records the choice
  as its own. A matter whose knock-on effects are large is @Pauli's: a change of the specification or of an approved
  ticket, the approval of a draft, a push or a merge into `base`, `basewithfixes`, `dev` or `master`, and the points
  under "Waiting on @Pauli".
- **Questions early.** When a session prepares a ticket, it asks its questions first, one at a time, and does not first
  test and investigate the whole ticket. After an answer, it can plan and ask again.
- **The tasks of a session.** A session does not investigate `dev` or other work that nobody gave it.
- **The base of the full runs** is `base`, with `--base-baselines prs/000-palette-stack-base` for the epic and its
  parts. A part that builds on earlier parts is rooted at `feature/palette-stack`.
- **A full run without a collecting run** when a change cannot change the drawing.
- **The OCAD metric is best effort.** The OCAD views and exports come from @Pauli when the work needs them.
- **The PDF of NotPurplePen** need not and should not be identical in structure with the PDF that OCAD exports.
- **The new dev** will be built strongly on the palette stack (@Pauli, Nov 24).
- **The SVG encoding** is in `Map_Svg` of NotPurplePen, which comes to the new dev: `Svg_GraphicsTarget` gives the
  encoding, which writes the transparency groups of pass 2 into its `SvgDocument`, and the structures of `Map_Svg`
  that come before the palette stack give way to those of the palette stack (@Pauli, Nov 24; [Record.md](Record.md),
  "The place of the SVG encoding"). The work uses the code of `Map_Svg` and the findings of `../SvgExportDesign.md`.
- **The vector files.** The PDF export and the SVG export have the same form, and both carry the content of `Map`. If
  it is needed, a flag leaves the content of `Map` out (@Pauli, Nov 25).
- **The SVG before the PDF.** The SVG ticket comes before the PDF ticket (@Pauli, Nov 25).
- **The two tickets of the SVG export.** The first brings `Map_Svg` to the new dev with the SVG encoding, and its
  tests check the SVG in Chromium against pass 2 with the Skia encoding (draft 030). The second gives the common
  structure of the vector files with the content of `Map`, after the first. The branches of both are rooted at
  `dev-next` (@Pauli, Nov 25).
- **The new basewithfixes and the new dev.** The new `basewithfixes` has no `fix/overlapping-patterns`, the new dev
  merges `feature/palette-stack` into it, `feature/multiply-blending` stays out, and `Map_Svg` comes in the SVG
  ticket without the history of `feature/svg-target` (@Pauli, Nov 25).
- **No users.** NotPurplePen has no users, and only trial releases have been built (@Pauli, Nov 25).
- **The baselines of Course 5 in base** stay upstream's. The epic has its own, and they are up to date.
- **The archiving of a cloud session** runs when @Pauli lets it, as "A cloud session" below tells.

## How the work is done

- **Language.** The tickets are in Finnish. The other documents are in Simplified Technical English, with lines of at
  most 120 characters and LF line endings. A word that the glossary does not have gets an entry before a ticket that
  uses it goes to @Pauli. A commit message tells what the change does and what in the code made it necessary, without
  a co-author line. The git identity is `Pauli Ojanperä <pauli.ojanpera@miettii.net>`.
- **No Windows machine.** A build or a test counts only when it ran in GitHub Actions, on a `wip/` branch.
- **A cloud session.**
  - Clone the repository with `git clone --filter=blob:none`, and make a worktree for each branch. Push with
    `git -c push.negotiate=true push origin <branch>`. A `git log -S` in a partial clone fetches the blobs of the
    whole history, and takes too long.
  - With `dot.net`, `builds.dotnet.microsoft.com` and `api.nuget.org` allowed, `dotnet-install.sh --channel 10.0
    --install-dir ~/.dotnet` installs the SDK. The SDK is `~/.dotnet/dotnet`, outside `PATH`.
  - The projects of PDFsharp run GitVersion, whose LibGit2Sharp cannot open a repository whose config names
    `extensions.worktreeConfig`. `git sparse-checkout` sets it, so do not use it in the clone.
  - The proxy refuses the writes of the GitHub API. `git push` and the reads of `gh` work.
  - When @Pauli lets the session archive, it runs `archive-if-due.sh claude` of the branch `sessions` of
    `pauliojanpera/windows-fonts` every minute in a background command, from a clone of that branch in
    `~/session-archive`. A background command ends after two hours, so the session starts the loop again.
- **On @Pauli's machine.** The clone `~/Documents/Projects/NotPurplePen` stays on `docs/process-coordination`. The
  worktrees are in `~/Documents/Projects/npp-worktrees/`, and `windows-fonts` is in
  `~/Documents/Projects/windows-fonts`.
- **Local tests.** They run in the integration worktree of the branch (`tools/int-worktree.sh <clone> <branch>
  <worktree>`): `base` with the branch, the fixes of the tests that Linux needs (`fix/case-of-test-file-names`) and
  `LinuxMapModelTestRun` merged in. A feature branch lacks the test tools of `base`, so its own worktree does not run
  the tests.
  - The local run of `Map_Skia.Tests` is `tools/linux-renderings.sh`, with the fonts of the workflow's Windows machine
    (`tools/windows-fonts.sh ~/windows-fonts-linux`; on @Pauli's machine with
    `FONTS_REMOTE=~/Documents/Projects/windows-fonts`). Each of its tests passes, and a test that fails is a fault of
    the change. It does not judge an image comparison by its baseline, and it leaves out the cases of
    `FallbackFindsAFontFor`. `tools/compare-renderings.py` compares its renderings and outcomes with those of a run
    without the change. Do not run `Map_Skia.Tests` locally in another way: on Linux, the texts render differently
    from the baselines of the Windows machine, and some tests need fonts that the branch `linux` lacks.
  - `tools/run-map-tests.sh` runs the files of `Test_MapModel` on Linux, without the renderings that GDI+ draws.
  - A local rendering is no baseline: each baseline comes from a run in GitHub Actions.
- **Full runs.** `tools/push-wip.sh [--collect] --base-baselines prs/000-palette-stack-base <clone> <branch>`, from
  `doc/devdocs/PaletteStack`, pushes `wip/linux/<name>`. With `--collect`, it also pushes a collecting run to
  `experiment/collect-<name>`, whose tests write the image of each comparison that differs beyond its threshold, in
  the artifacts `rendering-differences-<group>`. `record-baselines.py` of `../SvgDevLoop/` records them. A rendering
  that differs from its baseline below the threshold of its comparison passes, and a collecting run leaves no image of
  it. To record it, the commit of the collecting run removes the baseline: a comparison without a baseline leaves its
  rendering. A full run takes about ten minutes.
- **The result of a full run.** It has passed when its Tests run and its Build run are complete with the conclusion
  `success`, and the log of each group shows that its tests ran and passed (`gh run view <run> --json jobs`, `gh api
  repos/pauliojanpera/NotPurplePen/actions/jobs/<job>/logs`, the lines `Passed!` and `Failed!`). A group can end
  without its tests. A session states a result only after it has read it, and does not wait for a run whose result no
  next step needs.
- **Text baselines.** The `CreateOcadFiles` tests of `ControllerTests` compare an `OcadDump` of each course file with
  an expected dump in `src/TestFiles/controller/ocad_create*`. A collecting run does not collect them, so a full run
  finds a change of the OCAD export that moves them.
- **Line endings.** The sources have CRLF line endings. Check a changed file with `git ls-files --eol`;
  `tools/crlf.py` gives a file CRLF again. For a source that mixes CRLF with lone LFs, for example
  `src/PurplePenCore/CourseLayout.cs`, `tools/keep-eol.py <committed copy> <edited file>` keeps its line endings.
