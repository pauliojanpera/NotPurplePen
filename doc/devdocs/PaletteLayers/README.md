# The palette-layer work

Nov 18, 2026. The palette-layer work changes the architecture of upstream's map model, as
[Specification.md](Specification.md) describes it. This file tells how the work is organized: its tickets, its
branches, its pull request descriptions and its tracking. The practice is @Pauli's (Nov 18).

| File or folder | Holds |
| --- | --- |
| [Specification.md](Specification.md) | The reference of the architecture. Its preface tells when it binds, and which programs are the oracles. |
| [tickets/](tickets/) | The tickets, numbered in order. [000](tickets/000-palette-layers.md) is the epic. The templates: `epic-template.md`, `feature-template.md` and `bug-template.md`. |
| [prs/](prs/) | A pull request description of each branch, with the number of its ticket. The template: `template.md`. |
| [Progress.md](Progress.md) | The state of each branch, its full runs and the branches it is merged into, and the decisions of the work. |
| [tools/](tools/) | The scripts of the work. Its README tells each script in one sentence, and the loop of a ticket. |
| [figures/](figures/) | The figures of the specification. |

## Tickets

- The epic gives the goal of the work and its scope, by the chapters of the specification. It names its parts under
  "Erotetut alatehtävät".
- Each part of the work is a ticket: a feature from `feature-template.md`, and a fault from `bug-template.md`.
- A ticket tells the current state, not the events. When a criterion is met, mark it in the ticket.
- Work that is separated from a ticket, such as a fault found on the way, gets a ticket of its own, which tells where
  it came from. The first ticket names it under "Erotetut alatehtävät", with its branch, and whether that branch is
  merged into the first ticket's branch.
- The tickets are written in Finnish.

## Branches

- Each ticket has one branch: `feature/<name>` for a feature, and `fix/<name>` for a fault.
- A branch is rooted where `base` meets upstream (`git merge-base origin/base upstream/master`, d48f4456), so that it
  can be offered upstream as it stands. A part that builds on earlier parts is rooted at `feature/palette-layers`. A
  branch takes nothing from the other branches of the fork.
- A branch runs in full on `wip/linux/<name>`: a feature on `basewithfixes`, and a fix alone on `base`
  (`tools/push-wip.sh`). It records on itself only the baselines that it moves.
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
- `feature/palette-layers` runs in full on `wip/linux/palette-layers`, on `basewithfixes`.

## Tracking

- [Progress.md](Progress.md) records each branch: its ticket and description, its full runs, the baselines that it
  moves, and the branches it is merged into. It records the decisions of the work, with @Pauli's dates.
- The work stays out of `../NotPurplePenProgress.md` until `feature/palette-layers` is merged into `dev`. That merge,
  as all merges into `base`, `basewithfixes`, `dev` and `master`, needs @Pauli's word.
