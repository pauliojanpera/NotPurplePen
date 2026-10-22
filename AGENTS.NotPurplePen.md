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

## A branch is verified from a wip branch

```
git checkout -b feature/<name> upstream/master     # or: dev
...
git checkout -b wip/<name> base                    # or: dev
git merge feature/<name>
git push origin wip/<name>
```

The work stays on the feature branch, rooted at what it is meant for, carrying
nothing else. The `wip/**` branch is where it is run: rooted at `base` for what
could go upstream, at `dev` for what could not, with the feature merged into
it. The push runs the tests and publishes an installable package for Windows,
Linux and macOS to <https://pauliojanpera.github.io/NotPurplePen/> under
`trial/`. The package does not wait for the tests: it says the branch builds,
and the test run says the rest.

A wip branch named `wip/<platforms>/<name>` is packaged only for the platforms
it names: `win`, `linux` or `mac`, or several joined with `+`, as in
`wip/linux+mac/<name>`. The name picks the packages and nothing else; the tests
run the same for every wip branch.

The trial is named after `<name>` alone and keeps the newest build of each
platform; each file's name says which platform it is for. Pushing a commit that
is up as `wip/linux/<name>` to `wip/mac/<name>` as well adds macOS packages
beside the Linux ones without running the tests again, since a commit is tested
once whichever branches it reaches.

It has to be that way round. A push runs only the workflows that are in the
branch pushed, and a branch rooted at `upstream/master` has none, so pushing
the feature branch itself runs nothing.

A push to `experiment/**` runs the tests and produces nothing to install.

A branch is finished when that package can be downloaded, not when it is pushed.

Work on the SVG target, and nothing else, is checked as it goes by its own
loop: a push to `svg/<name>`, rooted at `base` with `feature/svg-target`
merged in, runs `.github/workflows/svg.yml`, whose verdict comes back as the
check run "SVG loop report" and the branch `svg-results/<name>`; the work is
finished, as all other work is, only after a full run on a `wip/` branch. Its
requirements, and its progress, which says where the work stands and how to
pick it up, are `doc/devdocs/SvgTargetRequirements.md` and
`doc/devdocs/SvgTargetProgress.md` on the branch `docs/svg-target-requirements`.

## A feature has to be reachable in the application that ships

The installer carries `AvPurplePen`, the Avalonia application, and
`PdfConverter`, and nothing else; the WinForms project in `src/PurplePen/` is
still compiled but is not shipped. A feature added only to a WinForms dialog
cannot be tried by anyone, so a feature branch carries the core change and the
Avalonia dialog together.

## Commit messages

What the change does, and what in the code made it necessary. Not why someone
would want the feature.
