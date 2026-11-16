# Flattening, and the separations behind it

Nov 16, 2026. The work of task 12 of the handover: an option that changes each colour drawn with blending or with
less than full opacity into opaque pieces, and the separations that compute it. This file tells what the work is,
why it sits on the ink of task 7, how it is driven, and what each step must prove. A session or an agent that does
the next case follows [Session.md](Session.md). A session that starts with no context reads
[../Introduction.md](../Introduction.md) first.

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

## The branches

| Branch | Rooted at | Holds |
| --- | --- | --- |
| `feature/flattening` | `feature/multiply-blending` (f6673fc5) | The stage, its tests and the setting in the dialogs |
| `fix/colour-transparency` | where `base` meets upstream (d48f445) | Reading OCAD's `t` and Mapper's `opacity`, with the baselines it moves. Merged into `feature/flattening` before phase 5 |
| `feature/svg-flattening` | `feature/svg-target` | The SVG's structured form (phase 7), if it is built |
| `wip/linux/flattening` | `basewithfixes`, with the feature merged in | The full run |
| `experiment/flattening-record-all` | the wip tip, with one commit more | One run that writes every image a change moves |

A push of `feature/flattening` starts no workflow. The inner loop runs on this machine. The Windows suite runs on
`wip/linux/flattening` at the end of each phase.

## The requirements

**FL — the flattening.**

| ID | Requirement | Level |
| --- | --- | --- |
| FL-1 | The stage sits in front of the targets, so PDF, SVG, the image export and printing share it. It takes the drawing calls of a map and gives back drawing calls that hold no `PushBlending` and no colour with alpha below 1. | Must |
| FL-2 | It computes with inks, not colours. An ink is every colour of the table with the same CMYK values and the same overprint flag, consecutive or not. | Must |
| FL-3 | Each overprinted ink keeps one coverage mask. A mark of the ink raises its coverage to the greatest its marks ask for. A mark never adds to it, because the marks of one plate share one screen and one layer of ink. | Must |
| FL-4 | A colour that is not overprinted knocks the masks of the inks below it out, over the area it covers. | Must |
| FL-5 | At the end the masks compose as the product of the inks' transmittances. The result does not depend on the order of the marks within an ink, nor on the order of the inks. | Must |
| FL-6 | The result is opaque pieces. Each piece is one filled shape with one CMYK value, mixed by FL-5, the piece over white included. | Must |
| FL-7 | The pieces are geometry, made with boolean path operations that keep curves. Nothing is turned into a polygon. | Must |
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

This feature is the one piece of work in the fork whose correctness is **self-checking**. The flattened drawing must
rasterize to the same pixels as the unflattened drawing. So nothing is recorded to find out whether the flattening
is right: the unflattened drawing is the reference. The SVG target needs recorded reference images; this does not.

There are three oracles in a chain. Each one localizes a different error.

```
the drawing as it is today
        │  GATE-1: same pixels
        ▼
the raster reference flattener  (FL-2 to FL-5, per pixel)
        │  GATE-2: same pixels
        ▼
the vector stage                (FL-6 to FL-9, boolean path operations)
```

GATE-1 alone says that something is wrong. The chain says **what**: a failure at the first arrow is the colour
model, a failure at the second is the geometry.

| ID | Gate | Level |
| --- | --- | --- |
| GATE-1 | **Equivalence.** A case drawn through the stage rasterizes to the same image as the same case drawn without it, within a pixel tolerance. | Must |
| GATE-2 | **The raster reference.** A raster implementation of FL-2 to FL-5 composes the masks per pixel. The vector stage equals it within the tolerance. | Must |
| GATE-3 | **Opacity.** The flattened call stream, recorded with `RecordingGraphicsTarget`, holds no `PushBlending` and no brush or pen whose colour has an alpha below 1 (FL-1). This is checked on the calls, not on pixels. | Must |
| GATE-4 | **Pending, not failed.** A case that reaches a construct the stage does not declare (FL-10) is reported pending, with the construct named. It is not a failure. A case passes when the stage declares every construct it uses and GATE-1, GATE-2, GATE-3, GATE-5 and GATE-6 hold. | Must |
| GATE-5 | **Determinism.** The same input flattens to the same geometry twice, on every run and machine. | Must |
| GATE-6 | **Knockout.** The pieces of one result do not overlap each other, and no piece has zero area. | Must |
| GATE-7 | **Set apart.** A case that cannot meet GATE-1 for a reason the requirements allow, such as the growth of FL-9 or the bitmap of LIM-1, is set apart with that reason, and is still compared, so that it shows if it ever matches. | Must |

GATE-4 is the mechanic that keeps every run green. An agent never has to judge whether a failure is expected at this
stage of the work: the stage's own declaration answers it. Progress is the count of cases that moved from pending to
passed, which only rises.

## The ladder

The unit of work is one case. Each case adds exactly one construct to the one before it. A case is a declarative
drawing, as `SimpleDrawingTests` holds, except the last two, which are maps.

| # | Case | Adds | First meets |
| --- | --- | --- | --- |
| 1 | `identity-opaque` | Two opaque marks of one colour overlap | Nothing. The stage must be the identity here |
| 2 | `two-colours` | An opaque colour over another | FL-4, the cut |
| 3 | `one-ink-overlap` | Two marks of one overprint ink overlap | FL-3, one coverage and not two |
| 4 | `two-inks-cross` | Two overprint inks cross | FL-5, the product of the transmittances |
| 5 | `split-ink` | An overprint ink whose colours are not consecutive, an opaque colour between them | FL-2 and FL-4 together. **What task 7's layer cannot hold** |
| 6 | `alpha-colour` | A colour with an alpha below 1 | The opacity half of the work |
| 7 | `stroked-lines` | Lines of a blended colour, and lines below one | FL-8, the outline |
| 8 | `patterns` | Hatchings and area patterns, drawn one time for a group of areas (task 2) | The union geometry |
| 9 | `text` | Text of a blended colour, and text below one | Glyph outlines |
| 10 | `bitmap` | A bitmap below a blended colour | LIM-1, the reported limit |
| 11 | `course-purple` | The course's purple over a map, in both modes and both places | The case a user sees |
| 12 | `test-maps` | The maps of the rendering tests | All of it together |

[cases.json](cases.json) holds the ladder for the scripts. A case is done when `reviews/<case>.md` exists, as a key
group of the translation work is done when its review exists.

## The phases

| Phase | Work |
| --- | --- |
| 1 | **The loop.** The harness, the gates, the ladder and the scripts. No flattening code. |
| 2 | **The ink model.** FL-2 to FL-5 and the raster reference. Cases 1 to 5 pass GATE-2 against the raster reference. |
| 3 | **The cut.** FL-6 to FL-8, the boolean path operations. Cases 1 to 5 pass GATE-1, then cases 6 to 9. |
| 4 | **The limits.** Case 10, the growth of FL-9, and the report of FL-10. |
| 5 | **The application.** `fix/colour-transparency` merged in first, then cases 11 and 12, the setting in the dialogs, its strings, and the full run. |
| 6 | **The separations** as an output of their own (SEP-2). |
| 7 | **The SVG's structured form:** one group for each ink with its blend mode, and a mask where a colour knocks the ink out, instead of opaque pieces. |

Phase 1 comes before any flattening code, as DL-1 put the SVG loop before any code of the SVG target. The loop is
what makes every later step cheap to check.

## Where the work runs

The inner loop is local and takes seconds. @Pauli's machine runs Linux, builds with the .NET 10 SDK, and renders
with SkiaSharp, which the repository already carries. Nothing of GATE-1 to GATE-7 needs a browser, a Windows machine
or a workflow.

| Runs | Where | Takes |
| --- | --- | --- |
| One case | This machine, `run-flatten-tests.sh --case <id>` | seconds |
| The ladder | This machine, `run-flatten-tests.sh` | minutes |
| The Windows suite | GitHub Actions, `wip/linux/flattening` | at the end of each phase |

So an agent does not wait for a run, and does not read a results branch. It builds, runs, reads the numbers and goes
on. CI is used at the end of a phase, and when a change moves a baseline.

## The scripts

All take the path of a worktree. They need Python 3 with Pillow, the .NET 10 SDK and git. Phase 1 writes them.

| Script | Does |
| --- | --- |
| `next-case.py <worktree>` | Names the next case of the ladder that has no review, and prints its brief: what it adds, which requirements it first meets, which constructs the stage must declare, and where its fixture goes. |
| `run-flatten-tests.sh [--case <id>] [--no-build] <worktree> <results>` | Builds the flattening tests and runs them into a results folder, then sorts the outcome with `triage.py`. |
| `triage.py <trx> <results>` | Sorts a run: passed, pending with the construct each needs, set apart with its reason, outside the tolerance, failed otherwise. Exits 1 when anything failed. |
| `compare-images.py <a> <b> [--out <diff>]` | The number behind GATE-1 and GATE-2: the pixels outside the tolerance, the greatest level difference, and a difference image. |
| `status.py <worktree> <out folder>` | Writes `Status.md`: the ladder with each case's state, the constructs the stage declares, and the cases set apart. |

The agent's own tools are in [`../SvgDevLoop/`](../SvgDevLoop/README.md) and are not written again here:
`agent-cache.py` sets the one-hour prompt cache, `watch-agent.py` follows an agent's commits and pushes, and
`dump-titles.py` prints its steps.

## How the work is driven

One agent works at a time. It does one case at a time. The session writes the briefs, asks @Pauli the questions and
keeps these documents.

The loop is built so that an agent makes no judgement call:

- **The next piece of work is named by a script**, not chosen. `next-case.py` gives it.
- **Correct is a number**, not an opinion. GATE-1 and GATE-2 give pixels outside the tolerance and the greatest level
  difference.
- **Not yet done is not the same as wrong.** GATE-4 keeps every run green until the stage declares the construct.
- **A case ends with a review**, which is what marks it done, and which the next session reads.

A case that an agent cannot make pass is left pending with its reason, and the agent stops and says so. It does not
widen a tolerance, and it does not set a case apart: GATE-7 is @Pauli's to grant.

### The model

The work is done by `claude-fable-5-2` at effort `high`.

[Session.md](Session.md) names three things that a brief must settle whatever model reads it: that one agent works
at a time, which code the agent owns, and which steps need no approval.

## Status

Phase 1 has not started. The ladder has no reviews.
