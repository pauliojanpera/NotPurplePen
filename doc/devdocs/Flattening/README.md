# Flattening, and the separations behind it

Nov 16, 2026. The work of task 12 of the handover: an option that changes each colour drawn with blending or with
less than full opacity into opaque pieces, and the separations that compute it. This file tells what the work is,
why it sits on the ink of task 7, what each step must prove, and how the work is driven. [Model.md](Model.md) gives
the rules of the calculation. The session that drives the work follows [Session.md](Session.md). An agent that does
one step follows its brief and [`.claude/agents/flatten-step.md`](../../../.claude/agents/flatten-step.md). A session
that starts with no context reads [../Introduction.md](../Introduction.md) first.

## What flattening is

A map holds inks that are not opaque. An overprinted ink lets the ink below it show through. A colour with a
transparency does the same. A reader that composes them must blend. Blending is where readers disagree: a print
driver, a CUPS filter, an old PostScript interpreter and an editor each handle it differently, and some rasterize
the whole page because of it.

Flattening removes the disagreement. It computes the composite itself, and writes the answer as opaque pieces: plain
filled shapes, each with one CMYK value, no blending and no alpha. Every reader draws the same thing, because there
is nothing left to interpret.

The separations are the same computation seen from the side: the coverage mask of each ink. They are what the
misregistration compensation of the handover's "Future directions" needs.

## Why it sits on the ink of task 7

Task 7 holds that one ink is one layer, and that the layer blends one time with what lies below it. It takes an ink
to be the **consecutive** colours of the table with the same CMYK values and the same overprint flag
(`Map.IsSameInk`), and encloses their drawing in one blending.

This work drops the word "consecutive".

| | Task 7, the ink's layer | This work |
| --- | --- | --- |
| An ink is | consecutive colours of equal values and flag | every colour of equal values and flag, wherever it sits |
| An ink's marks compose by | one layer, blended one time | one coverage mask, raised to the greatest coverage asked |
| A colour between an ink's colours | breaks the ink into two layers | knocks the ink's mask out, and the ink keeps one coverage |
| The output holds | blending | no blending and no alpha |

The handover names the case that task 7 cannot hold: an ink whose colours are not consecutive, with an opaque colour
between them. The course drawn on the map's own colour levels is that case, and task 14 waits for this work because
of it.

So the branch is rooted at `feature/multiply-blending`, which carries the ink. Both branches are offered upstream as
a stack.

## The branches and the folders

| Branch | Rooted at | Holds |
| --- | --- | --- |
| `feature/flattening` | `feature/multiply-blending` (e6116a7b) | The stage, its tests and the setting in the dialogs |
| `fix/colour-transparency` | where `base` meets upstream (d48f445) | Reading OCAD's `t` and Mapper's `opacity`, with the baselines it moves. Merged into `feature/flattening` before phase 5 |
| `feature/svg-flattening` | `feature/svg-target` | The SVG's structured form (phase 7), if it is built |
| `local/flattening-int` | `basewithfixes`, with the feature merged in | The check on this machine that nothing else moved. It is never pushed |
| `wip/linux/flattening` | `basewithfixes`, with the feature merged in | The full run |
| `experiment/flattening-record-all` | the wip tip, with one commit more | One run that writes every image a change moves |
| `flattening-log` | an orphan commit | The page of the work ("The page") |

A push of `feature/flattening` starts no workflow. The inner loop runs on this machine. The Windows suite runs on
`wip/linux/flattening` at the end of each phase.

The folders of this machine, which keeps them between sessions:

| Name | Folder | Holds |
| --- | --- | --- |
| `<clone>` | `/home/pauli/Documents/Projects/NotPurplePen` | The clone, on `docs/process-coordination`. The session works here |
| `<work>` | `/home/pauli/Documents/Projects/npp-worktrees` | The worktrees |
| `<work>/flat` | | The worktree of `feature/flattening`. An agent works here |
| `<work>/flat-int` | | The worktree of `local/flattening-int` |
| `<work>/flat-results` | | What the runs write. It is not in git |

## The requirements

**FL — the flattening.**

| ID | Requirement | Level |
| --- | --- | --- |
| FL-1 | The stage sits in front of the targets, so PDF, SVG, the image export and printing share it. It takes the drawing calls of a map and gives back drawing calls that hold no `PushBlending` and no colour with alpha below 1. | Must |
| FL-2 | It computes with inks, not colours. An ink is every colour of the table with the same CMYK values and the same overprint flag, consecutive or not. | Must |
| FL-3 | Each overprinted ink keeps one coverage mask. A mark of the ink raises its coverage to the greatest its marks ask for. A mark never adds to it, because the marks of one plate share one screen and one layer of ink. | Must |
| FL-4 | A colour that is not overprinted knocks the masks of the inks below it out, over the area it covers. | Must |
| FL-5 | The inks compose over the colour below them by the blend mode of the page, each channel of CMYK alone: by multiply as the product of the inks' transmittances, by darken as the darkest. The result does not depend on the order of the marks within an ink. By multiply it does not depend on the order of the inks either. [Model.md](Model.md) gives the rules of FL-2 to FL-5. | Must |
| FL-6 | The result is opaque pieces. Each piece is one filled shape with one CMYK value, mixed by FL-5, the piece over white included. | Must |
| FL-7 | The pieces are geometry, made with the boolean path operations of Skia, which keep curves. Nothing is turned into a polygon. | Must |
| FL-8 | A stroked line becomes its outline before it cuts or is cut. | Must |
| FL-9 | A setting grows each cutting shape by a set distance, 0.02 to 0.05 mm, against hairline gaps between pieces. A piece then overlaps its neighbour along a cut edge. The outer edge of a piece stays at the edge of its object. | Must |
| FL-10 | The stage declares the constructs it flattens. A drawing that reaches a construct it does not declare is reported, with the construct named, and is not flattened. | Must |
| FL-11 | Every class and method has a header comment, and types are explicit instead of `var`, as the repository requires. | Must |

**SEP — the separations.**

| ID | Requirement | Level |
| --- | --- | --- |
| SEP-1 | The coverage mask of each ink (FL-3, FL-4) is the separation of that ink. One computation gives both the flattening and the separations. | Must |
| SEP-2 | The separations are an output of their own, for the misregistration compensation of "Future directions". | Should |

**LIM — the limits.**

| ID | Requirement | Level |
| --- | --- | --- |
| LIM-1 | A bitmap below a blended colour is not cut as vectors. The stage reports it (FL-10) and leaves the drawing as it is. | Must |
| LIM-2 | The stage flattens solid colours with opacity and the standard blend modes, and nothing else. | Must |
| LIM-3 | **No colour of an export has an alpha below 1 today.** The application does not read the transparency of a colour, and the two colours that do carry an alpha are drawn on screen only: the print area outline (`MapDisplay.DrawPrintAreaOutline`, alpha 0.12) and the dimming of a bitmap base map (`mapIntensity`), which the export draws at full strength (B-6). So the opacity half of this work has no map case until `fix/colour-transparency` is merged (see "The transparency of a colour"). Case 6 sets the alpha in code. | Must |

## The transparency of a colour

@Pauli asked on Nov 16 whether this alpha is the same as OpenOrienteering Mapper's knockout, which the application
already draws as overprint. **It is not.** They are two fields, and one colour can carry both.

| Format | The overprint field | The transparency field |
| --- | --- | --- |
| Mapper `.omap` | `knockout`, a boolean. `OpenMapperImport` passes `!knockout` to `AddColor` ("Knockout in OOM is the opposite of overprint in OCAD"). | `opacity`, a scalar. `OpenMapperFileFormat` reads it into the struct; `AddColor` never gets it. |
| OCAD | `o` of the colour's parameter string. `OcadImport.CreateColors` reads it. | `t` of the same string, an opacity percentage where 100 is opaque. `OcadImport` says it: "we don't preserve the spot color information, or the transparency". |

Three colours of the test maps carry a transparency. All 700 OCAD files and 112 Mapper files of the repository were
searched on Nov 16; of 6,734 OCAD colours, one has a `t` other than 100.

| File | Colour | Value |
| --- | --- | --- |
| `MapModel/TestFiles/*/LordHill.ocd`, `LordHill OCAD11.ocd` | "Purple", cmyk(30,100,0,0) | `o1` **and** `t80` |
| `TestFiles/courseprinting/LordHill_ver16_2024Jan_scaled.omap` | "Red" | `opacity="0.75"` |
| `TestFiles/courseprinting/McHugh Creek.omap` | "Purple 40%" | `opacity="0.5"` |

LordHill's Purple settles it: the colour is overprinted **and** 80 % opaque at the same time. McHugh Creek's
"Purple 40%" shows what the scalar is used for: a 40 % purple made by an opacity of 0.5 on a full purple, and not by
halving the CMYK values.

**The decision** (@Pauli, Nov 16). Reading the transparency goes on its own branch, `fix/colour-transparency`,
rooted where `base` meets upstream, with the baselines that it moves. It is merged into `feature/flattening` before
phase 5, so that the real maps give the opacity half of this work its input. Two reasons for a moved baseline stay
in two branches, and the fix can be offered upstream by itself.

Each such colour becomes one layer at its opacity, as the handover's decision "A colour is a layer" requires. Then
the objects of the colour do not darken each other where they overlap. Mapper darkens them, because it applies the
opacity to each object. What OCAD does is not known.

## The gates

The flattened drawing must look as the unflattened drawing looks, **except where the model differs on purpose**.
[Model.md](Model.md) names the two differences, D1 and D2: they are cases 5 and 6, the cases that the work exists
for. So today's drawing is not the reference of the whole work. There are three oracles, and each finds a
different error.

```
the probe values of a case          (calculated by hand from Model.md)
        │  GATE-M: the same CMYK values
        ▼
the raster reference flattener      (Model.md, sample by sample)   ◄── GATE-1 ── the drawing as it is today
        │  GATE-2: the same samples
        ▼
the vector stage                    (FL-6 to FL-9, boolean path operations)
```

A failure of GATE-M is the model in the code. A failure of GATE-1 is a change that nobody meant. A failure of
GATE-2 is the geometry.

**How the gates compare images.** The first plan compared antialiased pixels in RGB. Both were wrong, and Nov 16
corrected them:

- **Each process channel alone.** Skia blends in RGB after the conversion of the colours, and the application
  converts through the SWOP table (`MapDisplay`, the CMYK colour model). The model blends in CMYK. So a gate draws a
  case four times, each time through a colour converter that gives one channel as grey: the tone of
  [Model.md](Model.md). Skia's multiply and darken on that grey are the ISO 32000 result for the channel. The four
  images are also the process separations of the drawing.
- **No antialiasing, at a high resolution.** Two antialiased pieces that share a cut edge let the paper show
  through, by up to a quarter of the difference between the paper and the lower colour. Without antialiasing each
  sample belongs to one piece. A difference is then measured as the samples that differ and as the largest area of
  them that hangs together. Phase 1 measures what the path operations themselves move, and the session sets the
  tolerance from it in [cases.json](cases.json).

| ID | Gate | Level |
| --- | --- | --- |
| GATE-M | **The model.** At each probe point of a case, the piece that holds the point has the CMYK values that the case gives, and the raster reference gives the same. A probe on paper finds no piece. | Must |
| GATE-1 | **No change where none is meant.** Today's drawing equals the raster reference, channel by channel, outside the areas that the case names as departures (D1, D2). Inside such an area the two must differ, so that it shows if they ever agree. | Must |
| GATE-2 | **The geometry.** The pieces of the vector stage, drawn channel by channel, equal the raster reference. | Must |
| GATE-3 | **Opacity.** The calls that the stage gives hold no `PushBlending` and no brush or pen whose colour has an alpha below 1 (FL-1). This is checked on the calls, not on pixels. | Must |
| GATE-4 | **Pending, not failed.** A case that reaches a construct the stage does not declare (FL-10) is reported pending, with the construct named. It is not a failure. A case passes when the stage declares every construct it uses and the other gates hold. | Must |
| GATE-5 | **Determinism.** The same input flattens to the same geometry twice in each run. The machines of the full run are compared at the end of a phase; phase 0 measures whether they can be equal. | Must |
| GATE-6 | **Knockout.** The pieces of one result do not overlap each other, and no piece has zero area. | Must |
| GATE-7 | **Set apart.** A case that cannot meet a gate for a reason the requirements allow, such as the growth of FL-9 or the bitmap of LIM-1, is set apart with that reason, and is still compared, so that it shows if it ever matches. Only @Pauli sets a case apart. | Must |

GATE-4 keeps every run green. An agent never has to judge whether a failure is expected at this stage of the work:
the stage's own declaration answers it. Progress is the count of cases that moved from pending to passed, which only
rises.

**A gate must be seen to fail.** Phase 1 gives the stage a switch that makes it wrong on purpose: a piece moved, a
colour changed, a knockout left out, a blending passed through. Each gate must fail on its fault before it is
trusted.

**The gates are not the agent's to change.** The harness, the tolerances, the cases and their probe values are the
session's. An agent that finds one of them wrong stops and says so.

## The ladder

The unit of work is one case. Each case adds exactly one construct to the one before it. A case is a declarative
drawing in a file of its own, except the last two, which are maps. Cases 3 to 9 run by darken and by multiply.

| # | Case | Adds | First meets | Today's drawing |
| --- | --- | --- | --- | --- |
| 1 | `identity-opaque` | Two opaque marks of one colour overlap | Nothing. The stage must be the identity here | The same |
| 2 | `two-colours` | An opaque colour over another | FL-4, the cut | The same |
| 3 | `one-ink-overlap` | Two marks of one overprint ink overlap | FL-3, one coverage and not two | The same |
| 4 | `two-inks-cross` | Two overprint inks cross | FL-5 | The same |
| 5 | `split-ink` | An overprint ink whose colours are not consecutive, an opaque colour between them | FL-2 and FL-4 together. **What task 7's layer cannot hold** | Differs by multiply where the two colours of the ink overlap (D1) |
| 6 | `alpha-colour` | A colour with an alpha below 1 | The opacity half of the work | Differs where marks of one colour overlap (D2) |
| 7 | `stroked-lines` | Lines of a blended colour, and lines below one | FL-8, the outline | The same |
| 8 | `patterns` | Hatchings and area patterns, drawn one time for a group of areas (task 2) | The union geometry | The same |
| 9 | `text` | Text of a blended colour, and text below one | Glyph outlines | The same |
| 10 | `bitmap` | A bitmap below a blended colour | LIM-1, the reported limit | Set apart (GATE-7) |
| 11 | `course-purple` | The course's purple over a map, by darken and by multiply, and on the map's own colour levels | The case a user sees | To be found. The colour levels with overprint ink do not exist before task 14 |
| 12 | `test-maps` | The maps of the rendering tests | All of it together | To be found |

[cases.json](cases.json) holds the ladder for the scripts. A case is done when `reviews/<case>.md` exists, as a key
group of the translation work is done when its review exists. The session writes the review.

### The file of a case

The file of a declarative case is `src/TestFiles/flattening/<id>.json` on `feature/flattening`. The session writes
it, with probe values calculated from [Model.md](Model.md). The cases are the shared test set that the handover's
"Future directions" asks for: each port of the flattening must pass them.

```json
{
  "id": "split-ink",
  "page": {"width": 60, "height": 40},
  "colours": {
    "purple": {"cmyk": [0.2, 0.8, 0, 0], "opacity": 1, "overprint": true},
    "green":  {"cmyk": [0.6, 0, 0.8, 0], "opacity": 1, "overprint": false}
  },
  "layers": [
    {"colour": "purple", "marks": [{"fill": "M10 10 L40 10 L40 30 L10 30 Z"}]},
    {"colour": "green",  "marks": [{"fill": "M25 5 L35 5 L35 35 L25 35 Z"}]},
    {"colour": "purple", "marks": [{"fill": "M20 15 L50 15 L50 25 L20 25 Z"}]}
  ],
  "departures": [
    {"modes": ["multiply"], "rule": "D1", "fill": "M20 15 L25 15 L25 25 L20 25 Z M35 15 L40 15 L40 25 L35 25 Z"}
  ],
  "probes": [
    {"at": [12, 12], "what": "the lower purple alone", "cmyk": [0.2, 0.8, 0, 0]},
    {"at": [22, 20], "what": "the two purples, nothing between them", "cmyk": [0.2, 0.8, 0, 0]},
    {"at": [30, 20], "what": "the upper purple over the green",
     "cmyk": {"multiply": [0.68, 0.8, 0.8, 0], "darken": [0.6, 0.8, 0.8, 0]}},
    {"at": [30, 8], "what": "the green alone", "cmyk": [0.6, 0, 0.8, 0]},
    {"at": [5, 5], "what": "paper", "cmyk": null}
  ]
}
```

- Lengths are millimetres on the page, with y down. Path data holds `M`, `L`, `C` and `Z` with absolute
  coordinates. A mark is a `fill` (with `"rule": "evenodd"` where it is not nonzero) or a `stroke` with its `width`,
  `cap`, `join` and `miterLimit`. Later cases add text, a pattern and a bitmap.
- The layers are the colours of a colour table, the lowest first. The harness draws them as `Map.Draw` draws a map:
  it opens a blending around the consecutive overprinted layers of one ink. So the unflattened drawing of a case is
  the drawing of task 7.
- A probe gives one CMYK value for all blend modes, or one for each. `null` is paper.

## The phases

| Phase | Work |
| --- | --- |
| 0 | **The measurements.** Skia's path operations on the geometry of real maps: time, failures, edges that two areas share, curves, and equal results on each run. No code of the stage. [Measurements.md](Measurements.md) holds the results. |
| 1 | **The harness.** The file of a case and the code that draws it, the gates, a stage that changes nothing, the switch that makes it wrong, the scripts, and the page. No flattening code. Case 1 passes. |
| 2 | **The model.** [Model.md](Model.md) as code, and the raster reference. The raster reference passes GATE-M and GATE-1 on cases 1 to 6. |
| 3 | **The cut.** FL-6 to FL-8, the boolean path operations. Cases 2 to 5 pass, then cases 6 to 9. |
| 4 | **The limits.** Case 10, the growth of FL-9, and the report of FL-10. |
| 5 | **The application.** `fix/colour-transparency` merged in first, then cases 11 and 12, the setting in the dialogs, its strings, and the full run. |
| 6 | **The separations** as an output of their own (SEP-2). |
| 7 | **The SVG's structured form:** one group for each ink with its blend mode, and a mask where a colour knocks the ink out, instead of opaque pieces. |

Phase 0 comes first because real maps are the last case of the ladder, and the path operations at the size of a map
are the largest risk of the work. If they fail there, the structure of the stage must allow for it from the start.
Phase 1 comes before any flattening code, as DL-1 put the SVG loop before any code of the SVG target.

## Where the work runs

The inner loop is local. @Pauli's machine runs Linux, builds with the .NET 10 SDK in `~/.dotnet`, which is not on the
`PATH` of a script, and renders with SkiaSharp, which the repository already carries. Nothing of the gates needs a
browser, a Windows machine or a workflow.

| Runs | Where | When |
| --- | --- | --- |
| One case, or the ladder | `<work>/flat` | In each step |
| The tests that the work must not move | `<work>/flat-int` | When a step is accepted |
| The Windows suite | GitHub Actions, `wip/linux/flattening` | At the end of each phase, on @Pauli's word |

`feature/flattening` is rooted at upstream, so it does not hold what lets upstream's tests run on Linux: the fix that
finds test files named with backslashes, and `src/Testing/LinuxTestRun`, which runs test files of `PurplePen_Tests`
here. `basewithfixes` holds both. So the check that nothing else moved runs in `<work>/flat-int`, which is
`basewithfixes` with the feature merged in. The check compares the tests that pass there with the tests that passed
before the work: the fonts of this machine are not the fonts of the workflow, so some tests fail here with or
without the work.

## The scripts

All are in this folder. They need Python 3 with Pillow, the .NET 10 SDK and git. Phase 1 writes them.

| Script | Does |
| --- | --- |
| `flat.py run [--case <id>] [--no-build] <worktree> <results>` | Builds the flattening tests and runs them into a results folder. Then it sorts the outcome: passed, pending with the construct each case needs, set apart with its reason, outside the tolerance, failed otherwise. Exits 1 when anything failed. |
| `flat.py next <worktree>` | Names the next case of the ladder that has no review, with what it adds and the requirements it first meets. |
| `flat.py status <results> <out folder>` | Writes `Status.md`: the ladder with the state and the numbers of each case, and the constructs the stage declares. |
| `logbook.py <results> <out folder>` | Writes the page ("The page"). |
| `publish-logbook.sh` | Publishes the page. It is the script of the Translation Logbook, made to serve the two pages. |

The agent's own tools are in [`../SvgDevLoop/`](../SvgDevLoop/README.md) and are not written again here:
`agent-cache.py` sets the one-hour prompt cache.

## The page

<https://pauliojanpera.github.io/NotPurplePen/flattening/> is the page of this work, as the SVG Logbook (`svg/`) and
the Translation Logbook (`translations/`) are the pages of theirs (@Pauli, Nov 16).

- It shows the plan drawn as a course, and where the work stands.
- For each case it shows the drawing before the stage and after it, the difference of the two, the pieces in false
  colours, the separations, and the numbers of the gates.
- The harness writes these images in each run, so the page draws nothing itself.
- `logbook.py` makes the page. It shares the code that draws the plan and the page with
  `../Translations/logbook.py`, which is not copied.
- The page is published on the branch `flattening-log` and under `flattening/` on `gh-pages`.
- `build.yml` copies the branches `svg-gallery` and `translation-log` into the site at each publish. Until it copies
  `flattening-log` too, a publish of the site removes the page, and the next publish of the page puts it back. The
  change of `build.yml` is a `ci/` branch that goes into `base`, `basewithfixes` and `dev` through full runs, on
  @Pauli's word.

## How the work is driven

One session drives the work. It starts one agent at a time. An agent does one step and stops. [Session.md](Session.md)
gives the steps of the session.

- **One agent, one step.** A step is one case of the ladder, or one piece of a phase. The next step gets a new agent
  with an empty context. Only the continuation of the same step goes back to the same agent.
- **The session writes the brief of each step:** what to deliver, the probe values, the places in the code, the
  commands, and the condition on which to stop. An agent does not look for these again.
- **Correct is a number**, not an opinion: the probe values, the samples that differ, the count of pieces.
- **Not yet done is not the same as wrong.** GATE-4 keeps every run green until the stage declares the construct.
- **An agent does not approve its own work.** The session runs the gates again on the agent's commit, and makes sure
  that the harness, the tolerances and the cases did not change. Then the session writes the review, which marks the
  case done.
- **An agent works in `<work>/flat` and commits there.** It does not push, and it does not write these documents.
  The session pushes what it accepted.
- **Nothing watches an agent.** The session goes on when the agent ends. An agent commits often, so a stop loses
  little, and the work can end after any step.

A step that an agent cannot finish stays pending with its reason, and the agent stops and says so. It does not widen
a tolerance, and it does not set a case apart: GATE-7 is @Pauli's to grant.

### The model

The work is done by `claude-fable-5-2` at effort `high` (@Pauli, Nov 16). The agent type `flatten-step` sets both.
The agent type `plan-step` runs at effort `max`, so this work does not use it.

## Status

Phase 0 has not started. The ladder has no reviews.
