# The palette-layer work

Nov 18, 2026. The palette-layer work changes the architecture of upstream's `Map` and its consumers, as
[Specification.md](Specification.md) describes it. This file tells how the work is organized: its tickets, its branches,
its pull request descriptions and its tracking. The practice is @Pauli's (Nov 18).

| File or folder | Holds |
| --- | --- |
| [Specification.md](Specification.md) | The reference of the architecture. Its preface tells when it binds, and which programs are the oracles. |
| [Glossary.md](Glossary.md) | The names of the concepts of the architecture in the code, in Finnish and in English, and the rules of the names. |
| [GlossaryHandover.md](GlossaryHandover.md) | How a new session continues the glossary work: the rules of the work with @Pauli, what is decided and what is open. |
| [tickets/](tickets/) | The tickets, numbered in order. [000](tickets/000-palette-layers.md) is the epic. The templates: `epic-template.md`, `feature-template.md` and `bug-template.md`. |
| [prs/](prs/) | A pull request description of each branch, with the number of its ticket. The template: `template.md`. |
| [Progress.md](Progress.md) | The state of each branch, its full runs and the branches it is merged into, and the decisions of the work. |
| [tools/](tools/) | The scripts of the work. Its README tells each script in one sentence, and the loop of a ticket. |
| [oracles/](oracles/) | The oracles of the appearance: each with its source, the values measured from it, and where the work uses it. |
| [figures/](figures/) | The figures of the specification. |

## The architecture comes first

The goal of the work is the structure that the specification separates: pass 1 makes the instances, and pass 2
composites them. feature/multiply-blending is an oracle of the appearance only. The work does not read its code, and
does not take its way, a layer around the drawing of the symbols, as the model of pass 2. The union of a palette
entry can stay implicit in the rendering, but its instances cannot be left out (preface and section 04 of the
specification, @Pauli, Nov 18).

## Sessions

- A session of the work needs no cache of the agents, and it starts no agents (@Pauli, Nov 18). So it does not run
  `agent-cache.py`, although `CLAUDE.md` asks every session to run it.
- A session of the work needs no archiving of the session, and it does not start the script that archives it
  (@Pauli, Nov 18).

## Tickets

- The epic gives the goal of the work and its scope, by the chapters of the specification. It names its parts under
  "Erotetut alatehtävät".
- Each part of the work is a ticket: a feature from `feature-template.md`, and a fault from `bug-template.md`.
- A ticket tells the current state, not the events. When a criterion is met, mark it in the ticket.
- Work that is separated from a ticket gets a ticket of its own, which tells where it came from. When the work of a
  ticket fixes a fault of upstream, the fix gets a ticket from `bug-template.md` and a `fix/` branch (@Pauli,
  Nov 18). The first ticket names the separated work under "Erotetut alatehtävät", with its branch, and whether that
  branch is merged into the first ticket's branch.
- A ticket of a part of the architecture has structural acceptance criteria, and not only criteria of the
  appearance ([Progress.md](Progress.md), "Next").
- The tickets are written in Finnish.
- A draft ticket sets no requirements for the other work. A session changes a draft only when @Pauli asks (@Pauli,
  Nov 18). A draft is not implemented (@Pauli, Nov 21).

## Branches

- Each ticket has one branch: `feature/<name>` for a feature, and `fix/<name>` for a fault.
- A branch is rooted where `base` meets upstream (`git merge-base origin/base upstream/master`, d48f4456), so that it
  can be offered upstream as it stands. A part that builds on earlier parts is rooted at `feature/palette-layers`. A
  branch takes nothing from the other branches of the fork.
- A branch runs in full on `wip/linux/<name>`, on `base` (`tools/push-wip.sh`, @Pauli, Nov 18). `base` holds upstream,
  the workflows and the test tools of the fork, and not the fixes of `basewithfixes`. A fix that a part needs is merged
  into `feature/palette-layers`. A branch records on itself only the baselines that it moves.
- On top of d48f4456, `base` changes only these files in `src/MapModel` and `src/PurplePenCore` (`origin/base` at
  d256a9e8, Nov 21): the `AssemblyInfo.cs` files, the tests of `src/MapModel`, the strings of `MiscText`,
  `VersionNumber.cs` and the cache of the PDF conversions (`PdfMapFile.cs`). Its other changes are in other folders,
  for example `src/AvPurplePen`, the tests, the installer and the workflows. So the code of `Map`, of the symbols, of
  the graphics targets and of the courses is the same at d48f4456 and in `base`.
- A branch is done when its ticket's criteria are met and its full run passed.

## Pull request descriptions

- A done branch gets a description in `prs/<number>-<name>.md` from `prs/template.md`, also when no pull request is
  made. No pull request is made yet.
- A description is in English, as upstream reads it, and short and factual. "Changes to be logged" is in Finnish, and
  tells only what a user sees.

## The branch of the epic

- `feature/palette-layers` collects the parts that are done, as `feature/svg-target` collects the SVG work.
- A merge into it names the work that it brings in its subject, and lists the commits in its body, as a merge into a
  shared branch does. It tells how a conflict was resolved.
- The trial build of `feature/palette-layers` on the site of NotPurplePen tells what it holds: the message of the commit
  that a push of `wip/linux/palette-layers` builds lists the ten work branches merged into `feature/palette-layers`
  last (@Pauli, Nov 19). `tools/push-wip.sh` writes the list into each commit that it makes, and the site shows the
  message under the commit of the build.
- `feature/palette-layers` runs in full on `wip/linux/palette-layers`, on `base`.

## Tracking

- [Progress.md](Progress.md) records each branch: its ticket and description, its full runs, the baselines that it
  moves, and the branches it is merged into. It records the decisions of the work, with @Pauli's dates.
- A document records as @Pauli's decisions only his decisions, and otherwise the facts that were found. A session
  does not give its own reading, its proposal or the practice of another document as a decision or a rule (@Pauli,
  Nov 18).
- The work stays out of `../NotPurplePenProgress.md` until `feature/palette-layers` is merged into `dev`. That merge,
  as all merges into `base`, `basewithfixes`, `dev` and `master`, needs @Pauli's word.
