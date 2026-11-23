# The tools of the palette-stack work

Scripts for the steps that each ticket of [the specification](../Specification.md) repeats. They need bash,
Python 3 with Pillow, git, the GitHub CLI signed in to the repository, and the .NET 10 SDK in `~/.dotnet/dotnet`
(or the `DOTNET` variable). The comment at the top of each script tells its options and its output.
`../../SvgDevLoop/` has the scripts that this folder uses as they are: `wait-run.sh`, `fetch-artifact.sh` and
`record-baselines.py`.

- `map-inks.py` prints the inks of an OCAD or Mapper file and the ink components of each of its colors, in one form
  for all formats.
- `ocad-string-encodings.py` tells whether the parameter strings of OCAD files encode their non-ASCII characters as
  UTF-8 or as an 8-bit code page, and whether OCAD, Mapper or Purple Pen wrote each file.
- `compare-images.py` counts the pixels in which two renderings differ and can mark them in red on a copy.
- `windows-fonts.sh` fetches the fonts of the workflow's Windows machine from the branch `linux` of
  `pauliojanpera/windows-fonts`, and writes a fontconfig file that offers only these fonts. The local runs of the Skia
  tests set `FONTCONFIG_FILE` to it.
- `linux-renderings.sh` is the local run of `Map_Skia.Tests`: it runs the tests in an integration worktree with these
  fonts, without judging an image comparison by its baseline, and each of its tests passes. It keeps each rendering
  that differs from its baseline, and the trx of the run.
- `compare-renderings.py` compares two runs of `linux-renderings.sh`: it lists the images whose renderings differ
  and the tests whose outcome differs.
- `crlf.py` gives files CRLF line endings again after a tool wrote them with LF.
- `keep-eol.py` gives an edited file back the line endings of its committed version, and gives the new lines CRLF.
  It is for the sources that mix CRLF with lone LFs, whose lone LFs `crlf.py` would change.
- `int-worktree.sh` makes or updates the local integration worktree of a branch: `base` with the branch, the fixes of
  the tests that a run on Linux needs and `LinuxMapModelTestRun` merged in. `--base` names another base branch. The
  integration branch is never pushed.
- `run-map-tests.sh` builds `LinuxMapModelTestRun` in an integration worktree and runs the tests that a filter
  selects.
- `push-wip.sh` rebuilds and pushes the `wip/linux/` branch of a branch for a full run on `base`, or on another base
  branch that `--base` names, and can collect the moved images or give a new Skia test a blank baseline. It pushes
  the collecting run to `experiment/collect-<name>`, on a commit whose `tests.yml` makes its tests collect, so it
  needs no write of the GitHub API. Its commits list the ten branches merged into the branch last, which the site
  shows with a trial build.
- `LinuxMapModelTestRun/` is a project that runs test files of `Test_MapModel` on Linux, as `src/Testing/LinuxTestRun`
  runs those of `PurplePen_Tests`.
- `ticket-009/` holds the check of the choices of ticket 009, a test file for `Map_Skia.Tests` that compares their
  coverage with the drawing (`PatternPrototypeCheck.cs`), and the count of the hatchings and the area patterns of the
  test maps (`CountPatterns.cs`, with its output).

## A ticket's loop

`../README.md` tells the workflow of the tickets and the branches. These are its steps with the scripts.

1. The feature branch is rooted where `base` meets upstream (`git merge-base origin/base upstream/master`). Work in
   its worktree. Commit often and push the feature branch: its push runs nothing.
2. `int-worktree.sh` after each commit that the local tests need, then `run-map-tests.sh` with a filter for the
   ticket's own tests and a few existing tests that go through the changed code. A test file of `Test_MapModel`
   joins `LinuxMapModelTestRun/LinuxMapModelTestRun.csproj` when a ticket needs it. A test that asks GDI+ for a
   rendering is skipped there. `Map_Skia.Tests` runs on Linux with `linux-renderings.sh` in the integration worktree,
   and each of its tests passes. Do not run it in another way: its image comparisons fail on Linux
   (`../Progress.md`, "Local tests").
   After a rewrite of the feature branch's history (an amend or a rebase), `REBUILD=1 int-worktree.sh`.
   A change of the drawing code is compared on Linux with the drawing without it: `windows-fonts.sh
   ~/windows-fonts-linux` once, then `linux-renderings.sh` in the integration worktree of the base of the change, for
   example `feature/palette-stack`, and in the integration worktree of the change, each with a new results folder.
   Then `compare-renderings.py <results without> <results with>` lists the images that the change moves on Linux. A
   change that does not change the drawing moves none.
3. `push-wip.sh --collect` when the change is complete and can change the drawing. The script prints the id of the
   collecting run, on `experiment/collect-<name>`. Wait for it with `wait-run.sh`, then fetch each group's images:
   `fetch-artifact.sh <run id> rendering-differences-<group> <folder>`.
   A change that cannot change the drawing, for example because it does not touch the code that the drawing runs,
   keeps the images as they are. Its full run is `push-wip.sh` without `--collect`, and the ticket is done when that
   push's Tests run passes (@Pauli, Nov 18).
4. `record-baselines.py --dry-run --base origin/base <folder> <feature worktree> <wip worktree>` tells which
   images the change moves. Record on the feature branch only the baselines that the change itself moves. An image
   that it writes to the wip side is a baseline that `base` moved too: it needs a decision of its own. When `base`
   added or changed the baseline for a test of its own, and the change moves it, the image belongs to the merge of the
   branch into `base`: keep it in `../prs/<ticket>-base/`, at its path in the repository below that folder, and
   give `push-wip.sh` the option `--base-baselines` with that folder at each push.
   A new Skia rendering test has no baseline to move: push with `--blank-baseline` for its description, and copy
   `<baseline>_actual.png` from the artifact `rendering-differences-skia` of the push's Tests run to the baseline.
   Skia's comparisons allow no difference, so a rendering made on this machine is no baseline.
5. Commit the baselines on the feature branch, push it, and `push-wip.sh` again. The ticket is done when that push's
   Tests run passes.

The renderings of a local run are not the suite's. With the fonts of the Windows machine, the texts still render
differently, since FreeType rasterizes them on Linux and DirectWrite on Windows. So a local run tells which images a
change moves, and not what the images must hold.
