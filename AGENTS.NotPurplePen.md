# NotPurplePen

This fork's own instructions. `AGENTS.md` beside this one came from upstream and
describes the code; where the two disagree about how work is built, tested or
delivered, this file is the one that applies.

## There is no Windows machine here

Nothing is built or tested on the developer's own machine, because there is none
that can run it. `AGENTS.md` opens with Visual Studio 2022, `msbuild` and a local
`dotnet test`: none of that is available, and its "Build and Test Commands"
section describes a machine this project does not have.

Every build and every test run happens in GitHub Actions. A change that has not
run there has not been checked, and saying it "should work" is not a result.

## Branches

| branch | holds |
|---|---|
| `base` | upstream, what it takes to build and test upstream here, and this fork's identity -- upstream may not be built under Purple Pen's name |
| `dev` | `base`, plus the fork's features and the changes of behaviour that come with them |
| `master` | the same as `dev`, as a release |

A change is made on a branch of its own, rooted at the branch it is meant for:

- something that could be offered upstream is rooted at `upstream/master`, so
  that it carries nothing else and can be offered as it stands;
- something that is this fork's alone is rooted at `dev`.

A change to a test baseline belongs in the same branch as the change that moved
the output, and nowhere else. `base` therefore carries baselines moved by fixes
offered upstream, and never ones moved by a feature.

## A branch is verified by a pull request

```
git checkout -b feature/<name> upstream/master     # or: dev
...
gh pr create --base base --head feature/<name>     # or: --base dev
```

The pull request is opened **in this repository**, against the branch the work
is meant for -- `base` for what could go upstream, `dev` for what could not --
and not to upstream. Both workflows carry a `pull_request` trigger for those
branches, so the run tests the merge of the base branch and the branch, and
publishes an installable package for Windows, Linux and macOS to
<https://pauliojanpera.github.io/NotPurplePen/> under `trial/`.

A push to `wip/**` does what that pull request does, without one: the tests
run, and the packages are published under `trial/`. A branch rooted at upstream
carries no workflow, and a push cannot run one that is not in the branch it
pushes, so a `wip/**` branch is rooted at `base` (or `dev`) with the work
merged into it.

A push to `experiment/**` runs the tests and produces nothing to install.

A branch is finished when that package can be downloaded, not when it is pushed.

## A feature has to be reachable in the application that ships

The installer carries `AvPurplePen`, the Avalonia application, and
`PdfConverter`, and nothing else; the WinForms project in `src/PurplePen/` is
still compiled but is not shipped. A feature added only to a WinForms dialog
cannot be tried by anyone, so a feature branch carries the core change and the
Avalonia dialog together.

## Commit messages

What the change does, and what in the code made it necessary. Not why someone
would want the feature.
