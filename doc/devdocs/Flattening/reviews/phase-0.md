# Review of phase 0: Skia's path operations on the geometry of real maps

Nov 17, 2026. The step is accepted: its numbers repeat in a run of the session. Its main result is that the cut, as
[Session.md](../Session.md) planned it, cannot be built on Skia's path operations as it stands.

## The step

Phase 0 of [README.md](../README.md), from the brief [briefs/phase-0.md](../briefs/phase-0.md). It adds the probe in
[probe/](../probe/) and the report [Measurements.md](../Measurements.md). It adds no code of the stage, so the stage
declares no construct. `feature/flattening` stays at e6116a7b.

## What the session measured

The session did not check each number. It checked that the tables come from the raw files, and it ran the short
stages of one map again.

- **The tables.** `probe/tables.py <results>` makes `<results>/tables.md` again byte for byte from the raw files.
  Each of its 452 table rows is in Measurements.md.
- **A run of the session,** with the machine alone, into `<work>/flat-results/phase-0-session`:

  ```
  ONLY_MAPS=LordHill ONLY_STAGES='shapes inventory digests tiles-second-run' \
      doc/devdocs/Flattening/probe/run-all.sh <work>/flat <work>/flat-results/phase-0-session
  ```

  Exit code 0, 280 s. Against the files of the agent's run:

  | What | Result |
  | --- | --- |
  | `q4-shared-edges.tsv`, `q7-curves.tsv` | Equal byte for byte |
  | `LordHill-ocd/marks.bin`, `q1-runs.tsv` | Equal byte for byte |
  | The 87 digests of `LordHill.ocd` | Equal. One pass takes 31.5 s |
  | `q6-tiles-operations.tsv`, the check of each result in the tiles | Equal byte for byte |
  | The pieces of the tiles against the drawing (question 8) | Equal at 5, 10 and 25 mm |

- **The operations of the knockout in the tiles of `LordHill.ocd`,** from the session's run:

  | Tile, mm | Operations | With no result | With a wrong result | Share | Unions of a run that are wrong | Time with the machine alone, s |
  | --- | --- | --- | --- | --- | --- | --- |
  | 5 | 7 142 | 18 | 199 | 3.0 % | 1 of 3 571 | 16.1 |
  | 10 | 2 768 | 25 | 162 | 6.8 % | 1 of 1 384 | 16.0 |
  | 25 | 782 | 32 | 75 | 13.7 % | 1 of 391 | 43.3 |

- **The pieces of those tiles against the drawing,** in the window of 16 000 000 samples: 155 376 samples differ at
  5 mm (118 739 thick, largest group 14 777), 433 886 at 10 mm (364 130 thick, largest group 29 715), and 2 257 749
  at 25 mm (1 944 620 thick, largest group 87 513).
- **The wrong union is a real error.** In `LordHill-ocd/q2-union-ByPairs-run-10-difference.png`, 207 991 samples are
  in the union of the contours and in no mark, and 9 182 samples of the marks are not in the union. The union fills
  the land between contour lines up to a straight line that no mark has.
- **The check that calls a result wrong uses samples only.** `SampleGrid.CountThickSamplesOffExpectation` draws the
  operands and the result without antialiasing, and counts the samples that differ together with their eight
  neighbours. It uses no path operation.
- **`<work>/flat`** is at e6116a7b with no changed file. In `<clone>` the agent wrote the probe and the report only.

Not run again by the session: the stage `whole-map`, the three other maps, `tiles-uncut`, `tiles-long`, `builder`
and `loop-time.sh`.

## The numbers that the next steps rest on

All are in Measurements.md, with the conditions of each.

- **The cut of a whole map is out of reach.** Each run cut by the union of the runs above it: `LordHill.ocd`, the
  smallest map, takes 860 s with the machine alone. 9 of its 40 operations give no result, and 62 % of the inked
  samples of the window have no piece. No other map ended in its limit.
- **Tiles make the time practical and do not cure the failures.** See the table above. `McHugh Creek.omap` has 0.4 %
  to 4.3 % bad operations. A wrong result with no failure is 2 to 14 times as frequent as "no result".
- **The bad results are in one kind of operation:** the `Difference` and the `Union` of an area run at the bottom of
  the order (yellow, green) with the union of all runs above it. The union of one run by pairs is wrong in 1 to 5
  tiles of a map.
- **Each result repeats:** 196 of 196 digests in one process and in two, the failures and the wrong results
  included. Another machine is not measured.
- **The operations keep curves,** within 0.00001 mm. The outline of a stroke from `SKPaint.GetFillPath` is wrong by
  0.158 mm for a line of 0.35 mm at the default resolution scale, by 0.0044 mm at 40, and by 0.0006 mm at 400.
- **The union of a run:** by pairs only. One after the other, and `OpBuilder`, pass 10 s at a run of 687 marks.
- **Shared edges:** straight ones are exact. Curves 0.0001 mm apart are taken as one curve: at most 6 samples of
  paper at 40 samples for each millimetre on 11 mm of edge.
- **A map as marks:** 17 to 21 runs, 12 000 to 171 000 marks, 0.27 to 2.2 million segments. The cut of each pattern
  element by its clip is 351 s of the 357 s that the largest map takes to draw. Text is 231 to 315 calls a map, and
  the probe does not keep it.
- **The loop:** first build 34.5 s, build with nothing changed 6.3 s, one test 3.7 s.

## What the step did not measure

- The final `run-all.sh` from its start to its end in one call. Measurements.md, "The conditions", lists the calls
  that made the tables.
- Another order of the cut: one upper run at a time, or only the runs that are blended.
- Tiles with a margin. The tiles of the probe put a straight edge of each run on the same line.
- Whether an `Intersect` gives a wrong result: the check of each result covers `Difference`, `Union` and the union
  of a run. The cuts of marks to a tile, and the 607 353 cuts by a clip, have no check.
- The totals of question 3 for three maps, and the cut with the overprinted runs at the size of a whole map.
- A second machine for the digests. `<results>/digests.txt` is not in git: `probe/run-all.sh` makes it again.
- The marks of the probe against the application's own drawing.

## The commits

- `docs/process-coordination`: the commit "Record the flattening step phase-0", which holds this file, the probe,
  Measurements.md, and the corrections of Model.md, Session.md, README.md and the handover.
- `feature/flattening`: none.

## The agent

`claude-fable-5-2` at effort `high`, as the agent type `flatten-step`. The usage limit of the account stopped it two
times, and it continued in the same context each time. Its cache had the one-hour lifetime. No request found the
cache expired: four requests came after a wait of more than five minutes (602 s, 467 s, 557 s and 405 s, each a run
of the probe), and each read its context from the cache.

## What the brief and the model had wrong

- `briefs/phase-0.md`, line 57: the machine has 2 cores with 2 threads each, not 4 cores.
- `briefs/phase-0.md`, question 8, takes it that the pieces of question 3 exist. They exist for one map only.
- `Model.md`: "A map draws each colour of its table as one run" was not exact. The session corrected the sentence.
  It changes no case.

## For the next steps, and the doubts

1. **The plan of the cut does not stand.** Session.md planned a division of the page: each knockout colour cuts all
   that is below it, and GATE-6 requires that pieces do not overlap. That division is made of the operations that
   fail. The handover asks for less: "Each such colour is cut against each colour below it", where "such" is a
   colour with blending or with less than full opacity. An opaque colour that stays whole, in its drawing order,
   covers what is below it with no cut. This structure needs `Intersect` only, and only for the blended runs. It is
   not measured, and it breaks GATE-6 as README.md gives it. Session.md, "What phase 0 changed", has this.
2. **The stage needs a check of each result,** and a way out when the check fails. Phase 0 shows that samples tell a
   right result from a wrong one. It does not show the way out.
3. **A run is defined by the values of the brush.** The stage cannot see which colour of the table draws. So two
   knockout colours of the table with equal values and an equal opacity below 1, one after the other, are one run by
   rule R3, and not two layers. No test map has this case.
4. **The agent found faults outside the step,** which the work does not correct on the way: `Skia_GraphicsTarget`
   does not dispose the `SKPath` of `DrawPath` and `FillPath` with a list of parts
   (`src/MapModel/Map_SkiaStd/SkiaGraphicsTarget.cs`, lines 482 to 485 and 498 to 501), and
   `SymDef.DrawPatternRows` draws pattern elements over the bounding box of an area, so three of four elements end
   outside their clip.
5. **`McHugh Creek.omap` gives the same count, 19 529, in two cells** of question 6 that measure different things. The
   agent believes it is a coincidence and did not prove it.
