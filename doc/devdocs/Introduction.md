# NotPurplePen: an introduction for a new session

Nov 16, 2026. Read this first. It tells what NotPurplePen is, what it consists of, where its parts are, and the rules
that all work keeps. Where the work stands is in the handover, [NotPurplePenProgress.md](NotPurplePenProgress.md).

## What NotPurplePen is

- **Purple Pen** is a program that sets orienteering courses. It draws the courses on a map, makes the control
  descriptions, and prints or exports the materials of an event. Peter Golde writes it, at
  <https://github.com/petergolde/PurplePen>. The sessions call it "upstream".
- **NotPurplePen**, <https://github.com/pauliojanpera/NotPurplePen>, is @Pauli's fork of it. The fork builds and
  tests upstream on GitHub Actions, offers its fixes of upstream's bugs to upstream as pull requests, and adds
  features of its own. The largest of them is the SVG export. Upstream may not be built under Purple Pen's name, so
  the fork has an identity of its own.
- **What ships.** The fork's packages for Windows, Linux and macOS are at <https://pauliojanpera.github.io/NotPurplePen/>.
  They hold the Avalonia application, AvPurplePen, and PdfConverter, and nothing else. The WinForms application in
  `src/PurplePen`, which upstream ships, is compiled but not shipped. So a feature must be in the Avalonia application
  to reach a user.

## What it consists of

The code is in `src`. `PPen.slnx` is the solution.

| Folder | Holds |
| --- | --- |
| `AvPurplePen` | The Avalonia application: its views (AXAML), its platform code, and its strings, `UIText.resx` and a satellite `UIText.<language>.resx` for each language. |
| `PurplePenViewModels` | The view models of the Avalonia application. They do not depend on the UI. |
| `PurplePenCore` | The logic of the application: `Controller`, `EventDB` (the course data, with undo), `CourseView`, `CourseLayout`, `MapDisplay`, printing and export, and the texts in `MiscText`, `CommandNameText`, `StatusBarText`, `SelectionDescriptionText` and `ReportText`. `AGENTS.md` still names `src/PurplePen` for these files; they are in `PurplePenCore` now. |
| `MapModel` | The map model, which reads OCAD and OpenOrienteering Mapper files, and the drawing targets behind `IGraphicsTarget`: Skia, PDF, GDI+, WPF, Direct2D, and the fork's SVG target (`Map_Svg`). |
| `PurplePen` | The WinForms application. Its form resources and satellites are in it too. |
| `PurplePen_Tests`, `PurplePenViewModels.Tests`, `PdfConverter.Tests`, `MapModel/*Tests*`, `Testing`, `TestFiles` | The tests and their files. |
| `Installer`, `Innosetup` and the other installer folders | The packaging. |

The languages of the application are the satellites of its six resources: `UIText` of AvPurplePen, and the five
resources of PurplePenCore. The application offers each language that has a satellite folder. Upstream's
translators work on Launchpad (<https://translations.launchpad.net/purple-pen>), and upstream brings their work into
the WinForms project with its TranslateTool. [Translations/README.md](Translations/README.md) tells more.

## Where the work runs

- **No Windows machine.** Nothing is built or tested on @Pauli's machine. Every build and test runs in GitHub Actions:
  `tests.yml` (Tests), `build.yml` (Build NotPurplePen, which makes the packages) and `svg.yml` (the SVG loop). A
  change that has not run there has not been checked.
- **The branches.**

  | Branch | Holds |
  | --- | --- |
  | `base` | Upstream, what it takes to build and test upstream here, and the fork's identity. |
  | `basewithfixes` | `base` and the fixes of upstream's bugs that the fork offers upstream. |
  | `dev` | `basewithfixes` and the fork's features. The default branch. |
  | `master` | The same as `dev`, as a release (`AGENTS.NotPurplePen.md`). It is not on GitHub on Nov 16. |
  | `fix/<name>` | A fix of upstream's code, rooted where `base` meets upstream, so that it can be offered as it stands. |
  | `feature/<name>` | A feature, rooted at upstream or at `dev`. |
  | `ci/<name>` | A change of the workflows, rooted at `base`. |
  | `wip/<platforms>/<name>` | Where a change runs in full: a push runs the tests and publishes a trial package for the platforms named (`win`, `linux`, `mac`, joined with `+`). |
  | `experiment/<name>` | A push runs the tests and makes no package. |
  | `svg/<name>` | The SVG loop of the SVG work. |
  | `docs/process-coordination` | The documents of the work and its tools, in `doc/devdocs`. |

  A branch rooted at upstream holds no workflow, so a push of it runs nothing. It runs in full from a `wip/` branch
  that merges it into `base` or `basewithfixes`. A change is finished when its package can be downloaded.
  The translation of the key groups is the exception: a session that translates neither starts nor waits for a build
  or a test run, and `fix/complete-translations` runs in full once, after the last group
  ([Translations/Session.md](Translations/Session.md)).
- **The documents.** `AGENTS.md` is upstream's guide to the code. `AGENTS.NotPurplePen.md` holds the fork's rules of
  building, testing and delivering, and wins where the two disagree. Both are in the code branches and load into a
  session that works in a checkout. On `docs/process-coordination`, `doc/devdocs` holds the handover
  (`NotPurplePenProgress.md`), the documents of the SVG target (`SvgTargetProgress.md`, `SvgTargetRequirements.md`,
  `SvgExportDesign.md`), the translation work (`Translations/`), the flattening work (`Flattening/`), and the tools of
  each piece of work.
- **The transcripts** of the sessions are archived in the private repository `pauliojanpera/windows-fonts`, on the
  branch `sessions`. Its README tells how.

## The rules of all work

These are @Pauli's rules. The handover, under "Decisions", gives where each came from.

- **When @Pauli writes**, answer first, and do nothing else until the answer is given. Then do only what he asked.
  This does not keep a session from telling him of a fault: a session tells of each fault that it finds at once,
  also when he did not ask about it (@Pauli, Nov 23).
- **A question for @Pauli** is asked one at a time, with choices and a recommendation, when he is present.
- **Documents** are written in Simplified Technical English: short sentences, one fact in each, exact terms.
- **A commit message** tells what the change does, and what in the code made it necessary.
- **A merge into a shared branch** (`base`, `basewithfixes`, `dev`, `master`, `feature/svg-target`) names the work
  that it brings in its subject, and lists the commits that it brings in its body, as `git merge --log` writes them.
- **A fix of upstream's code** goes on its own `fix/` branch, with the test baselines that it moves, and into "Ready to
  offer to Purple Pen" in the handover.

## Working in a cloud session

- Attach `pauliojanpera/NotPurplePen` with push access. Clone one branch, shallow, and give the clone up to ten
  minutes: `git clone --depth 1 -b <branch> https://github.com/pauliojanpera/NotPurplePen <folder>`. For the documents
  alone, a sparse checkout of `doc/devdocs` is faster.
- Push from a shallow clone with `git -c push.negotiate=true push origin <branch>`. The session cannot delete a
  branch: the git gateway refuses it.
