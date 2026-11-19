# The tools of the palette-layer work

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
- `crlf.py` gives files CRLF line endings again after a tool wrote them with LF.
- `int-worktree.sh` makes or updates the local integration worktree of a branch: `base` with the branch and
  `LinuxMapModelTestRun` merged in. `--base` names another base branch.
- `run-map-tests.sh` builds `LinuxMapModelTestRun` in an integration worktree and runs the tests that a filter
  selects.
- `push-wip.sh` rebuilds and pushes the `wip/linux/` branch of a branch for a full run on `base`, or on another base
  branch that `--base` names, and can collect the moved images or give a new Skia test a blank baseline. Its commits
  list the branches merged into the branch since the last passing run of the wip branch, which the build shows.
- `LinuxMapModelTestRun/` is a project that runs test files of `Test_MapModel` on Linux, as `src/Testing/LinuxTestRun`
  runs those of `PurplePen_Tests`.

## A ticket's loop

`../README.md` tells the workflow of the tickets and the branches. These are its steps with the scripts.

1. The feature branch is rooted where `base` meets upstream (`git merge-base origin/base upstream/master`). Work in
   its worktree. Commit often and push the feature branch: its push runs nothing.
2. `int-worktree.sh` after each commit that the local tests need, then `run-map-tests.sh` with a filter for the
   ticket's own tests and a few existing tests that go through the changed code. A test file of `Test_MapModel`
   joins `LinuxMapModelTestRun/LinuxMapModelTestRun.csproj` when a ticket needs it. A test that asks GDI+ for a
   rendering is skipped there; `Map_Skia.Tests` runs on Linux as it is.
   After a rewrite of the feature branch's history (an amend or a rebase), `REBUILD=1 int-worktree.sh`.
3. `push-wip.sh --collect` when the change is complete and can change the drawing. Wait for the collecting run with
   `wait-run.sh`, then fetch each group's images: `fetch-artifact.sh <run id> rendering-differences-<group> <folder>`.
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

The renderings of a local run are not the suite's, since the fonts of this machine are not the ones that the
workflow installs. They tell which images a change moves, and not what the images must hold.
