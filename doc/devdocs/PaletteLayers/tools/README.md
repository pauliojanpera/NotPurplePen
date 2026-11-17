# The tools of the palette-layer work

Scripts for the steps that each ticket of [the specification](../Specification.md) repeats. They need bash,
Python 3, git, the GitHub CLI signed in to the repository, and the .NET 10 SDK in `~/.dotnet/dotnet` (or the
`DOTNET` variable). `../../SvgDevLoop/` has the scripts that this folder uses as they are: `wait-run.sh`,
`fetch-artifact.sh` and `record-baselines.py`.

| Script | Does |
| --- | --- |
| `map-inks.py <map file>...` | Prints the inks of an OCAD 6 to 8, OCAD 9 or later, or Mapper file, and the ink components of each palette entry, in one form for all three. Compare a file and the file that an export made of it with `diff`. |
| `int-worktree.sh <clone> <feature branch> <integration worktree>` | Makes or updates the integration worktree `local/<name>-int`: `origin/basewithfixes` with the feature merged in, and `LinuxMapModelTestRun/` of this folder in `src/MapModel/`. The branch is local: it is never pushed. |
| `run-map-tests.sh [--no-build] <integration worktree> <filter> [<results>]` | Builds `LinuxMapModelTestRun` and runs the tests of `Test_MapModel` that the filter selects. Prints the tests that failed and the totals. |
| `push-wip.sh [--collect] [--blank-baseline <test description>]... <clone> <feature branch> [<wip name>]` | Rebuilds `wip/linux/<name>` as a merge of `origin/basewithfixes` and the feature branch, pushes it, and prints the ids of its runs. With `--collect`, it also starts the Tests workflow with `collect_differences`, whose artifacts hold every image that the change moves. With `--blank-baseline`, one more commit, on the wip branch alone, gives a new Skia rendering test a white baseline of its size, because the Skia comparison leaves its rendering only where a baseline exists. |
| `LinuxMapModelTestRun/` | A project that compiles test files of `Test_MapModel` where they are, with the assembly name `Test_MapModel`, for `net10.0`, as `src/Testing/LinuxTestRun` does for `PurplePen_Tests`. `Shims.cs` stands in for GDI+: a test that asks for a rendering is skipped, because GDI+ draws the renderings of `Test_MapModel`, on Windows only. Add a test file to the project when a ticket needs it. |

## A ticket's loop

1. The feature branch is rooted where `base` meets upstream (`git merge-base origin/base upstream/master`). Work in
   its worktree. Commit often and push the feature branch: its push runs nothing.
2. `int-worktree.sh` after each commit that the local tests need, then `run-map-tests.sh` with a filter for the
   ticket's own tests and a few existing tests that go through the changed code.
3. `push-wip.sh --collect` when the change is complete. Wait for the collecting run with `wait-run.sh`, then fetch
   each group's images: `fetch-artifact.sh <run id> rendering-differences-<group> <folder>`.
4. `record-baselines.py --dry-run --base origin/basewithfixes <folder> <feature worktree> <wip worktree>` tells which
   images the change moves. Record on the feature branch only the baselines that the change itself moves. An image
   that it writes to the wip side is a baseline that `basewithfixes` moved too: it needs a decision of its own.
   A new Skia rendering test has no baseline to move: push with `--blank-baseline` for its description, and copy
   `<baseline>_actual.png` from the artifact `rendering-differences-skia` of the push's Tests run to the baseline.
   Skia's comparisons allow no difference, so a rendering made on this machine is no baseline.
5. Commit the baselines on the feature branch, push it, and `push-wip.sh` again. The ticket is done when that push's
   Tests run passes.

The renderings of a local run are not the suite's, since the fonts of this machine are not the ones that the
workflow installs. They tell which images a change moves, and not what the images must hold.
