# The SVG work's development loop

Scripts for the steps every change of the SVG target goes through, so that no executor has to work them out again.
They need bash, Python 3 with Pillow, Node, git and the GitHub CLI signed in to the repository, and for the tests the
.NET 10 SDK and the Chromium of the tests' Playwright package, which `../SvgTargetProgress.md` says how to install.

| Script | Does |
| --- | --- |
| `run-svg-tests.sh [--no-build] [--filter <expr>] <worktree> <results>` | Builds `Map_Svg.Tests` and runs it as the loop does, into a results folder, and sorts the outcome with `triage.py`. A full run takes about four minutes; a filter such as `"Name~simpletext"` runs a few cases in seconds. |
| `triage.py <trx> [--names]` | Sorts a run's outcome: cases passed, pending, without a recorded reference, outside the tolerance, with a character of a text misplaced, failed otherwise, and the other tests' failures. Exits 1 when anything failed. |
| `stage-except-experiments.sh <file>...` | Stages a file's changes but the hunks that hold the word `EXPERIMENT`, the lines a local run needs and no commit carries, such as the text calls added to DL-5's list. |
| `push-svg-loop.sh [--with-experiments] <worktree> [<name>]` | Rebuilds `svg/<name>` as a merge of `origin/base` and the worktree's branch, with the worktree's experiment hunks in one commit more if asked, pushes it with a lease, and prints the loop run's id. The results come to `svg-results/<name>`. |
| `wait-run.sh <run id>` | Waits for a GitHub Actions run and prints its conclusion and jobs. Start it in the background and be told when the run ends. |
| `render-cases.sh <worktree> <results> <references> <cases>` | Renders cases' SVGs in Chromium exactly as the tests do, the sentinel font and the flags included, for the gallery and the tolerance research, through `../SvgGallery/render-cases.mjs`. |
| `fetch-artifact.sh <run id> <artifact> <folder>` | Downloads and unpacks one artifact of a run into a folder of its own. |
| `record-baselines.py [--dry-run] <artifact folder> <fix worktree> <wip worktree>` | Writes the renderings a Tests run left of its failed bitmap comparisons over their baselines: on the fix branch where the baseline is upstream's, on the wip branch where base moved it. |

Set `ARCHIVE_TRAILERS` to the executor's attribution lines and `push-svg-loop.sh` ends its commits with them, as the
archive scripts on `windows-fonts`'s `sessions` branch do.

## A change of the SVG target

1. Work in a worktree of the feature branch, say `feature/svg-text` in `wt/text`.
2. Run the cases the change touches, then all: `run-svg-tests.sh wt/text <results>`. Nothing but cases waiting on a
   recording or pending may be left.
3. Commit: `stage-except-experiments.sh` for the files that carry experiment lines, `git add` for the rest, and push
   the feature branch, which runs nothing.
4. `push-svg-loop.sh --with-experiments wt/text`, then `wait-run.sh <run id>` in the background. The loop runs on
   GitHub's runner, which is where a result counts; read `summary.md` on `svg-results/<name>`.
5. For the gallery: `render-cases.sh` for the cases that passed, whose renderings the loop does not keep, then
   `../SvgGallery/make-gallery-step.py`, `look-at-gallery.mjs` and `publish-svg-gallery.sh`.

## A fix offered to Purple Pen

A `fix/` branch is rooted at `upstream/master` and checked on `wip/linux/<name>`, which is `base` with the fix merged
in. When the fix moves baselines, each Tests run leaves the renderings of the comparisons that failed:

1. `fetch-artifact.sh <run id> rendering-differences-purplepen <folder>` (and `-skia`, when that group failed too).
2. `record-baselines.py <folder> <fix worktree> <wip worktree>`; commit on the fix branch, merge it into the wip branch,
   and push the wip branch.

A test stops at its first image that differs, so each run finds one more image a test. To find them all at once, push
the wip branch's tip to an `experiment/` branch with one commit more, in which `TestUtil.GdiPlus`'s
`BitmapTestUtil.CompareBitmapBaseline` writes a line instead of failing; that run passes and leaves every rendering.
`experiment/subpixel-record-all` did so for the subpixel fix.

## Elsewhere

- `../SvgTolerance/` measures and chooses the pixel tolerance.
- `../SvgGallery/` makes and publishes the gallery.
- The `sessions` branch of `pauliojanpera/windows-fonts` keeps the sessions' transcripts, with its own scripts.
