# Phase 0: Skia's path operations on the geometry of real maps

Nov 17, 2026. These are the measurements of phase 0 of [README.md](README.md): what the boolean path operations of
Skia do with the marks of four real maps, and with edges that two areas share. The probe in [probe/](probe/) made each
number. This file designs nothing. Its last section, "What the numbers suggest", is an opinion and says so.

## How to make the tables again

```
doc/devdocs/Flattening/probe/run-all.sh <flat> <results>
```

- `<flat>` is a checkout of `feature/flattening` at e6116a7b. The probe builds against its `src` folder and changes no
  tracked file. Question 9 removes the `bin` and `obj` folders of its projects and makes them again.
- `<results>` gets the tables as files of tab-separated values, the images, the digests, and `tables.md`, which holds
  each table of this file. `probe/tables.py <results>` prints the tables again from the files.
- It needs the .NET 10 SDK in `~/.dotnet` or on the `PATH`, the packages SkiaSharp 3.119.4 and
  SkiaSharp.NativeAssets.Linux 3.119.4 from NuGet, and Python 3 with its standard library.
- `MAPS_AT_ONE_TIME=4` measures the four maps at the same time, as the run of this file did. The stage times of the
  slowest map add up to about 50 minutes. Without it the maps go one after the other.
- `ONLY_MAPS` and `ONLY_STAGES` make a part of the tables again: the comment at the top of `run-all.sh` names the
  stages.

| File of the probe | Does |
| --- | --- |
| `run-all.sh` | Each step, in order: `loop-time.sh`, the build of the probe, the shapes, the stages of each map, `tables.py` |
| `loop-time.sh` | Question 9 |
| `SkiaPathOpsProbe.csproj`, `Program.cs` | The console program: `inventory`, `tiles`, `digests` and `whole-map` for a map, and `shapes` |
| `MarkCollectingGraphicsTarget.cs` | The target that keeps the marks of each run as Skia paths |
| `MarkCache.cs` | The file `marks.bin` of a map, so that only `inventory` draws the map |
| `PathOperations.cs`, `Knockout.cs` | The three ways of union, and the two cuts of question 3 |
| `PathMeasures.cs`, `SampleGrid.cs` | Counts of segments, digests, areas, and the comparison of drawings sample by sample |
| `MapProbe.cs`, `ShapeProbe.cs` | The measurements of a map, and of the shapes that the probe makes |
| `tables.py` | The tables of this file from the files of a run |

## The conditions

- **The machine.** Intel Core i3-2120: 2 cores with 2 threads each, 15 GB of memory, Debian 13 with kernel 6.12,
  .NET SDK 10.0.404, SkiaSharp 3.119.4.
- **One run.** Each number is of one run, unless a table says that a result was made two times.
- **Times are wall-clock seconds, and they are too long by a factor of 2 to 3.** The four maps were measured at the
  same time on 2 cores, and later stages ran beside them. Three measurements with the machine alone show the factor:
  the drawing of `teanaway.omap` took 198 s alone and 350 s in the run; one pass of the digests of `LordHill.ocd`
  took 29.8 s alone and 97.7 s in the run; and the union by pairs of the brown run of `LordHill.ocd` took 69 s alone
  and 144 s in the run. So compare the times of one table with each other, and take a time as an order of magnitude.
- **Limits.** A way or a stage that passes its time limit is stopped, and the table says "stopped". A stop is not a
  failure of Skia. "No result" means that Skia gave no path (`Op` gave null, or `Resolve` gave false).
- **The final `run-all.sh` did not run from its start to its end in one call.** The two hours of the step did not
  allow a second full run. What ran, in this order:
  1. `loop-time.sh`, with the machine alone (question 9).
  2. One call of an earlier form of `run-all.sh`, with `MAPS_AT_ONE_TIME=4`: the stages `inventory`, `tiles`,
     `digests`, `digests-second-process` and `whole-map` of the four maps. The code of these measurements is the code
     of this folder, with two differences: `tiles` did not check each result and did not cut with the overprinted
     runs, and `whole-map` held the builder at its end, which no map reached.
  3. `shapes`, in a call of its own while the maps were measured (questions 4 and 7).
  4. Calls of the final script for single stages, each into a folder of its own, such as
     `ONLY_MAPS='LordHill|McHugh' ONLY_STAGES='tiles-second-run' run-all.sh <flat> <results>/followup3`. The files
     of these stages were then copied into `<results>`: `tiles-uncut`, `builder`, `tiles-long` and `tiles-second-run`.
     `tiles-uncut` and `tiles-long` ran with a form of the probe that did not check each result.
  5. Two calls of the final script with the machine alone, as a check of the script:
     `ONLY_MAPS=LordHill ONLY_STAGES='shapes inventory digests digests-second-process builder' run-all.sh <flat>
     <results>/smoke`, and then the same with `ONLY_STAGES=whole-map`. Their files stay in `<results>/smoke` and are
     in no table of this file; questions 2, 3, 5 and 8 quote them. The first call gave the tables of questions 4
     and 7 again byte for byte, the same file of the marks, and the same 87 digests. The second call was stopped by
     hand after the cut of each run as a knockout colour.
  So the check of each result is from `tiles-second-run` only, and so is the cut with the overprinted runs in the
  tiles of `LordHill.ocd` and `McHugh Creek.omap`.
- **Two numbers are from a trial that left no files:** the times of the drawing of a large map alone, 198 s above
  and 196 s in question 1.

## How the probe draws a map

- **The render options** are those of a vector export: `usePatternBitmaps = false`, as `MapDisplay.DrawHelper` sets it
  for printing; `minResolution = 25.4 / 2400` mm, as `CoursePdf` sets it for 2400 dots for each inch;
  `renderTemplates = MapAndTemplates`; `blendOverprintedColors = true`; `overprintBlendMode = Darken`, which changes
  no geometry. The map is drawn over `Map.Bounds`.
- **A mark** is one drawing call with a solid brush or a pen, as a Skia path in the coordinates of the map, which are
  millimetres on paper. A fill keeps its path and its fill rule. A stroke becomes its outline by `SKPaint.GetFillPath`
  with a resolution scale of 40 for each millimetre (question 7 shows why 1, the default, is wrong). The transform of
  the moment is applied to the path.
- **The clip of the moment** is applied to each mark with `Op(Intersect)`. The hatchings and the area patterns reach
  the target as lines and shapes inside a clip, so most of these intersections are one pattern element against the
  outline of its area.
- **A run** is the marks that follow each other with the same CMYK values, the same alpha, and the same state of
  blending (inside `PushBlending` or not). The name of a run is the names of the colours of the table that have its
  values and its overprint flag. The target does not see which colour of the table draws.
- **Not kept:** text, bitmaps and pattern brushes are counted and not kept. A map of these four has no bitmap and
  asks for no pattern brush.
- **The file of the marks.** `inventory` writes the marks to `<map>/marks.bin`, bit for bit. The other stages read
  them from there and do not draw the map again.

## How the probe compares two drawings

- **The window** of a map is the square of 100 mm by 100 mm that holds the most marks, on a grid of 10 mm.
- **Samples.** A drawing is drawn without antialiasing at 40 samples for each millimetre, so the window has
  16 000 000 samples. Two drawings are compared sample by sample.
- **A thick differing sample** is a differing sample whose eight neighbours differ too. Two drawings of the same
  outline differ in single samples along the outline. A thick sample shows an area that one drawing has and the
  other has not.
- **The largest group** is the largest count of differing samples that hang together side to side.

## The answers in short

| # | Question | Answer |
| --- | --- | --- |
| 1 | What a map holds | 17 to 21 runs, 12 000 to 171 000 marks, 0.27 to 2.2 million segments of each kind. No bitmap and no pattern brush. Up to 315 text calls, which the probe does not keep. |
| 2 | The union of a run | Only "by pairs" unites a large run. It takes up to 151 s for one run. One union of a large run is wrong, and Skia reports no failure. |
| 3 | The knockout of a whole map | It did not end on any map in the run of the tables, in limits of 480 s to 1 800 s. With the machine alone the cut of `LordHill.ocd` ends in 860 s: 9 of its 40 operations give no result, and 62 % of the inked samples of the window have no piece. |
| 4 | Shared edges | Straight shared edges are exact. Curves 0.0001 mm apart are taken as one curve. No contour below 0.000001 mm2. At most 6 samples of paper at 40 on 11 mm of curved edge. |
| 5 | The same result two times | Yes, each time: in one process, in two processes, failures and wrong results included. Another machine is not measured. |
| 6 | Tiles | Tiles of 10 mm cut a map in 24 s to 315 s. But the knockout gives no result or a wrong result in 0.4 % to 13.7 % of its operations, at each tile size from 5 mm to 25 mm, so the pieces have holes and overlaps. |
| 7 | Curves | The operations keep each kind of curve, within 0.00001 mm. The outline of a stroke needs a resolution scale: at the default it is 0.16 mm wrong. |
| 8 | The pieces against the drawing | The pieces of the whole map of `LordHill.ocd` differ from the drawing at 48 % of the samples of the window. The pieces of its tiles of 10 mm differ at 2.7 %, in areas of up to 29 715 samples. |
| 9 | The loop | First build 34.5 s, build with nothing changed 6.3 s, one test 3.7 s. |

## Question 1: what a map holds

**Question 1: what each map holds.**

From `<map>/q1-map.tsv`.

| Map | Size, mm | Colours of the table (overprinted) | Runs (inside a blending) | Marks | Contours | Segments | Lines / quadratics / conics / cubics | Load, s | Draw, s |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| LordHill-ocd | 139 x 181 | 43 (8) | 20 (5) | 12 226 | 12 789 | 270 965 | 148 715 / 72 316 / 37 721 / 12 213 | 4.5 | 1.6 |
| McHugh_Creek-omap | 401 x 272 | 38 (11) | 17 (8) | 24 474 | 37 904 | 534 892 | 106 483 / 309 904 / 12 493 / 106 012 | 1.4 | 69.8 |
| TeanawayValley-ocd | 730 x 371 | 34 (0) | 21 (0) | 170 584 | 207 601 | 2 163 863 | 905 580 / 221 525 / 675 423 / 361 335 | 4.4 | 356.7 |
| teanaway-omap | 730 x 371 | 35 (34) | 21 (21) | 168 476 | 205 489 | 2 158 894 | 902 590 / 221 523 / 672 449 / 362 332 | 5.2 | 350.0 |

- A map has about 20 runs, and far fewer runs than colours: colours that follow each other with the same values are
  one run, and a colour with no visible symbol draws nothing.
- One ink is more than one run. In `LordHill.ocd` the overprinted black is runs 14 and 16, and the knockout colour
  "Brown 50% / Street infill" is run 15 between them. In `McHugh Creek.omap` the overprinted black is runs 8, 10, 12
  and 16. These are real cases of an ink whose colours are not consecutive (case 5 of the ladder).
- `teanaway.omap` has 34 overprinted colours of 35, so each of its 21 runs is inside a blending. `TeanawayValley.ocd`
  holds almost the same marks and has no overprinted colour.
- The marks hold each kind of segment. Fills give lines and cubics. The outlines of strokes give quadratics, and
  conics at round caps and joins. Circles give conics.
- A few runs hold most of the geometry: the brown run (the contours) holds 64 % of the segments of `LordHill.ocd`,
  and the last black run and the brown run hold 78 % of `McHugh Creek.omap`.

**Question 1: what the target met and did not keep.**

From `<map>/q1-map.tsv`.

| Map | Text calls | Bitmap calls | Pattern brushes | Clips | Switches of antialiasing (pushes) | Blendings | Marks cut by a clip | Marks outside their clip | Marks that a clip made empty or that were empty | Cuts by a clip with no result | Time of the cuts by a clip, s |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| LordHill-ocd | 231 | 0 | 0 | 161 | 0 (1) | 5 | 9 780 | 8 777 | 6 510 | 0 | 1.1 |
| McHugh_Creek-omap | 0 | 0 | 0 | 1 817 | 0 (1) | 8 | 8 399 | 1 412 | 1 040 | 2 | 69.2 |
| TeanawayValley-ocd | 315 | 0 | 0 | 2 597 | 0 (1) | 0 | 294 589 | 265 698 | 160 979 | 1 | 351.4 |
| teanaway-omap | 315 | 0 | 0 | 2 739 | 0 (1) | 21 | 294 580 | 264 858 | 160 958 | 2 | 344.6 |

- The one push of antialiasing is the probe's own, before it calls `Map.Draw`. The map switches nothing.
- The clips are those of hatchings and area patterns. No clip was inside another clip.
- **The cut of the marks by their clips is almost all of the drawing time**: 351 s of 357 s for `TeanawayValley.ocd`
  in this run, and 196 s of 198 s when the map was drawn alone. It is one `Op(Intersect)` for each pattern element.
- A cut by a clip gave no result 5 times in 607 353 cuts. Those marks are not in the runs.
- About three of four pattern elements end outside their clip or empty: the map draws the elements over the
  bounding box of an area.

**Question 1: the runs.**

From `<map>/q1-runs.tsv`.

| Map | Run | Colour | C, M, Y, K | Inside a blending | Marks | Segments | Lines / quadratics / conics / cubics |
| --- | --- | --- | --- | --- | --- | --- | --- |
| LordHill-ocd | 0 | Yellow 50% | 0, 0.1, 0.5, 0 | no | 109 | 1 417 | 254 / 0 / 0 / 1 163 |
| LordHill-ocd | 1 | Yellow | 0, 0.2, 1, 0 | no | 26 | 298 | 70 / 0 / 0 / 228 |
| LordHill-ocd | 2 | White for yellow + White for green + White for road under construct. + White background | 0, 0, 0, 0 | no | 687 | 3 628 | 121 / 0 / 3 147 / 360 |
| LordHill-ocd | 3 | Black 30% | 0, 0, 0, 0.3 | no | 10 | 51 | 20 / 0 / 0 / 31 |
| LordHill-ocd | 4 | Green 30% | 0.12, 0, 0.24, 0 | no | 142 | 1 725 | 331 / 0 / 0 / 1 394 |
| LordHill-ocd | 5 | Green 60% | 0.24, 0, 0.48, 0 | no | 144 | 4 196 | 766 / 0 / 0 / 3 430 |
| LordHill-ocd | 6 | Green | 0.4, 0, 0.8, 0 | no | 238 | 3 509 | 749 / 0 / 8 / 2 752 |
| LordHill-ocd | 7 | Dark green | 0.5, 0, 1, 0 | no | 712 | 3 364 | 1 682 / 0 / 0 / 1 682 |
| LordHill-ocd | 8 | White for yellow + White for green + White for road under construct. + White background | 0, 0, 0, 0 | no | 135 | 2 160 | 0 / 2 160 / 0 / 0 |
| LordHill-ocd | 9 | Green above white | 0.5, 0, 1, 0.25 | no | 159 | 2 448 | 288 / 2 160 / 0 / 0 |
| LordHill-ocd | 10 | Brown | 0, 0.56, 1, 0.18 | yes | 2 411 | 172 376 | 93 452 / 65 427 / 13 429 / 68 |
| LordHill-ocd | 11 | Brown 50% + Street infill | 0, 0.28, 0.5, 0.09 | no | 2 | 10 | 8 / 0 / 0 / 2 |
| LordHill-ocd | 12 | Blue 50% | 0.44, 0.09, 0, 0 | no | 3 | 160 | 15 / 0 / 0 / 145 |
| LordHill-ocd | 13 | Blue | 0.88, 0.18, 0, 0 | yes | 1 129 | 8 099 | 5 303 / 186 / 1 900 / 710 |
| LordHill-ocd | 14 | Street borderlines + Black | 0, 0, 0, 1 | yes | 12 | 1 126 | 944 / 0 / 182 / 0 |
| LordHill-ocd | 15 | Brown 50% + Street infill | 0, 0.28, 0.5, 0.09 | no | 6 | 88 | 20 / 68 / 0 / 0 |
| LordHill-ocd | 16 | Street borderlines + Black | 0, 0, 0, 1 | yes | 6 273 | 66 065 | 44 465 / 2 315 / 19 055 / 230 |
| LordHill-ocd | 17 | Red 75% | 0, 0.75, 0.75, 0 | no | 1 | 13 | 13 / 0 / 0 / 0 |
| LordHill-ocd | 18 | Black above whiteout | 0, 0, 0, 1 | no | 17 | 191 | 191 / 0 / 0 / 0 |
| LordHill-ocd | 19 | Purple transparent + Purple | 0.3, 1, 0, 0 | yes | 10 | 41 | 23 / 0 / 0 / 18 |
| McHugh_Creek-omap | 0 | Purple + Purple for track symbols | 0.35, 0.85, 0, 0 | yes | 1 | 4 | 0 / 0 / 4 / 0 |
| McHugh_Creek-omap | 1 | Yellow 50% | 0, 0.135, 0.395, 0 | no | 988 | 21 686 | 0 / 0 / 0 / 21 686 |
| McHugh_Creek-omap | 2 | Green 100% for undergrowth + Green 100% for area features | 0.76, 0, 0.91, 0 | no | 7 184 | 61 759 | 25 844 / 0 / 0 / 35 915 |
| McHugh_Creek-omap | 3 | Green 30% + Green 30% for narrow ride | 0.228, 0, 0.273, 0 | no | 1 016 | 13 777 | 0 / 0 / 0 / 13 777 |
| McHugh_Creek-omap | 4 | Green 60% + Green 60% for narrow ride | 0.456, 0, 0.546, 0 | no | 926 | 11 482 | 0 / 0 / 0 / 11 482 |
| McHugh_Creek-omap | 5 | Green 100% for undergrowth + Green 100% for area features | 0.76, 0, 0.91, 0 | no | 200 | 1 193 | 0 / 0 / 0 / 1 193 |
| McHugh_Creek-omap | 6 | White over yellow + White over green + White for railway + White for course overprint | 0, 0, 0, 0 | no | 2 | 8 | 8 / 0 / 0 / 0 |
| McHugh_Creek-omap | 7 | Blue 100% for area features | 1, 0, 0, 0 | no | 1 | 14 | 0 / 0 / 0 / 14 |
| McHugh_Creek-omap | 8 | Black for open land + Black below lower brown 50% + Black below upper brown 50% + Black below purple for track symbols + Black 100% | 0, 0, 0, 1 | yes | 2 | 2 184 | 2 084 / 0 / 100 / 0 |
| McHugh_Creek-omap | 9 | Lower brown 50% + Upper brown 50% | 0, 0.28, 0.5, 0.09 | no | 5 | 421 | 303 / 118 / 0 / 0 |
| McHugh_Creek-omap | 10 | Black for open land + Black below lower brown 50% + Black below upper brown 50% + Black below purple for track symbols + Black 100% | 0, 0, 0, 1 | yes | 12 | 1 306 | 1 188 / 118 / 0 / 0 |
| McHugh_Creek-omap | 11 | Lower brown 50% + Upper brown 50% | 0, 0.28, 0.5, 0.09 | no | 2 | 2 184 | 2 084 / 0 / 100 / 0 |
| McHugh_Creek-omap | 12 | Black for open land + Black below lower brown 50% + Black below upper brown 50% + Black below purple for track symbols + Black 100% | 0, 0, 0, 1 | yes | 8 | 32 | 32 / 0 / 0 / 0 |
| McHugh_Creek-omap | 13 | Brown 100% | 0, 0.56, 1, 0.18 | yes | 1 827 | 160 484 | 23 603 / 126 194 / 10 687 / 0 |
| McHugh_Creek-omap | 14 | Blue 100% | 1, 0, 0, 0 | yes | 46 | 1 745 | 647 / 1 098 / 0 / 0 |
| McHugh_Creek-omap | 15 | Green 100% | 0.76, 0, 0.91, 0 | yes | 2 | 8 | 8 / 0 / 0 / 0 |
| McHugh_Creek-omap | 16 | Black for open land + Black below lower brown 50% + Black below upper brown 50% + Black below purple for track symbols + Black 100% | 0, 0, 0, 1 | yes | 12 252 | 256 605 | 50 682 / 182 376 / 1 602 / 21 945 |
| TeanawayValley-ocd | 0 | Yellow 50% | 0, 0.18, 0.52, 0 | no | 5 854 | 123 416 | 79 328 / 0 / 0 / 44 088 |
| TeanawayValley-ocd | 1 | White for yellow + White for green + White mask | 0, 0, 0, 0 | no | 98 295 | 561 298 | 44 549 / 0 / 484 671 / 32 078 |
| TeanawayValley-ocd | 2 | Yellow + Line yellow | 0, 0.265, 0.76, 0 | no | 13 855 | 91 085 | 23 310 / 124 / 62 110 / 5 541 |
| TeanawayValley-ocd | 3 | Grey 100% + Black 30% | 0, 0, 0, 0.3 | no | 160 | 1 800 | 1 254 / 0 / 0 / 546 |
| TeanawayValley-ocd | 4 | Green 20% | 0.14, 0, 0.18, 0.02 | no | 2 430 | 58 620 | 36 308 / 0 / 0 / 22 312 |
| TeanawayValley-ocd | 5 | Green 30% | 0.21, 0, 0.27, 0.03 | no | 6 674 | 217 193 | 16 096 / 0 / 0 / 201 097 |
| TeanawayValley-ocd | 6 | Green 50% | 0.28, 0, 0.45, 0.05 | no | 2 562 | 62 009 | 15 623 / 0 / 351 / 46 035 |
| TeanawayValley-ocd | 7 | White for yellow + White for green + White mask | 0, 0, 0, 0 | no | 146 | 584 | 0 / 0 / 0 / 584 |
| TeanawayValley-ocd | 8 | Green | 0.7, 0, 0.9, 0.1 | no | 4 932 | 28 931 | 20 304 / 2 829 / 439 / 5 359 |
| TeanawayValley-ocd | 9 | Yellow + Line yellow | 0, 0.265, 0.76, 0 | no | 8 | 253 | 33 / 174 / 46 / 0 |
| TeanawayValley-ocd | 10 | Brown 50% + Brown 50% Street infill | 0, 0.19, 0.31, 0.16 | no | 7 | 63 | 56 / 0 / 0 / 7 |
| TeanawayValley-ocd | 11 | Blue 50% | 0.44, 0.12, 0, 0.06 | no | 3 | 13 | 13 / 0 / 0 / 0 |
| TeanawayValley-ocd | 12 | Yellow/Green 100/50% | 0.28, 0.265, 1, 0.17 | no | 3 | 26 | 22 / 0 / 0 / 4 |
| TeanawayValley-ocd | 13 | Brown | 0, 0.375, 0.62, 0.32 | no | 12 171 | 747 223 | 449 696 / 211 663 / 84 736 / 1 128 |
| TeanawayValley-ocd | 14 | Blue | 0.875, 0.245, 0, 0.05 | no | 4 269 | 44 405 | 32 687 / 2 897 / 6 620 / 2 201 |
| TeanawayValley-ocd | 15 | Red 100% | 0, 1, 1, 0 | no | 1 | 13 | 13 / 0 / 0 / 0 |
| TeanawayValley-ocd | 16 | Purple | 0.18, 0.9, 0.07, 0.03 | no | 5 | 20 | 20 / 0 / 0 / 0 |
| TeanawayValley-ocd | 17 | Black 100%Road borders + Black + All color separations | 0, 0, 0, 1 | no | 39 | 4 455 | 3 916 / 0 / 535 / 4 |
| TeanawayValley-ocd | 18 | Black 40% Road infill | 0, 0, 0, 0.4 | no | 36 | 152 | 16 / 0 / 0 / 136 |
| TeanawayValley-ocd | 19 | Brown 50% + Brown 50% Street infill | 0, 0.19, 0.31, 0.16 | no | 19 | 830 | 572 / 258 / 0 / 0 |
| TeanawayValley-ocd | 20 | Black 100%Road borders + Black + All color separations | 0, 0, 0, 1 | no | 19 115 | 221 474 | 181 764 / 3 580 / 35 915 / 215 |
| teanaway-omap | 0 | Yellow 50% | 0, 0.18, 0.52, 0 | yes | 5 854 | 123 413 | 79 331 / 0 / 0 / 44 082 |
| teanaway-omap | 1 | White for yellow + White for green + White mask | 0, 0, 0, 0 | yes | 98 284 | 560 093 | 44 623 / 0 / 482 653 / 32 817 |
| teanaway-omap | 2 | Yellow + Line yellow | 0, 0.265, 0.76, 0 | yes | 13 852 | 91 421 | 23 325 / 124 / 62 366 / 5 606 |
| teanaway-omap | 3 | Grey 100% + Black 30% | 0, 0, 0, 0.3 | yes | 160 | 1 800 | 1 254 / 0 / 0 / 546 |
| teanaway-omap | 4 | Green 20% | 0.14, 0, 0.18, 0.02 | yes | 2 430 | 58 620 | 36 308 / 0 / 0 / 22 312 |
| teanaway-omap | 5 | Green 30% | 0.21, 0, 0.27, 0.03 | yes | 6 673 | 217 398 | 16 122 / 0 / 0 / 201 276 |
| teanaway-omap | 6 | Green 50% | 0.28, 0, 0.45, 0.05 | yes | 2 561 | 62 017 | 15 625 / 0 / 351 / 46 041 |
| teanaway-omap | 7 | White for yellow + White for green + White mask | 0, 0, 0, 0 | yes | 146 | 584 | 0 / 0 / 0 / 584 |
| teanaway-omap | 8 | Green | 0.7, 0, 0.9, 0.1 | yes | 4 930 | 29 697 | 21 017 / 2 829 / 493 / 5 358 |
| teanaway-omap | 9 | Yellow + Line yellow | 0, 0.265, 0.76, 0 | yes | 8 | 253 | 33 / 174 / 46 / 0 |
| teanaway-omap | 10 | Brown 50% + Brown 50% Street infill | 0, 0.19, 0.31, 0.16 | yes | 7 | 63 | 56 / 0 / 0 / 7 |
| teanaway-omap | 11 | Blue 50% | 0.44, 0.12, 0, 0.06 | yes | 3 | 13 | 13 / 0 / 0 / 0 |
| teanaway-omap | 12 | Yellow/Green 100/50% | 0.28, 0.265, 1, 0.17 | yes | 3 | 26 | 22 / 0 / 0 / 4 |
| teanaway-omap | 13 | Brown | 0, 0.375, 0.62, 0.32 | yes | 11 602 | 744 835 | 447 535 / 211 661 / 84 511 / 1 128 |
| teanaway-omap | 14 | Blue | 0.875, 0.245, 0, 0.05 | yes | 4 089 | 43 742 | 32 512 / 2 897 / 6 132 / 2 201 |
| teanaway-omap | 15 | Red 100% | 0, 1, 1, 0 | yes | 1 | 13 | 13 / 0 / 0 / 0 |
| teanaway-omap | 16 | Purple | 0.18, 0.9, 0.07, 0.03 | yes | 5 | 20 | 20 / 0 / 0 / 0 |
| teanaway-omap | 17 | Black 100%Road borders + Black + All color separations | 0, 0, 0, 1 | yes | 39 | 4 455 | 3 916 / 0 / 535 / 4 |
| teanaway-omap | 18 | Black 40% Road infill | 0, 0, 0, 0.4 | yes | 36 | 152 | 16 / 0 / 0 / 136 |
| teanaway-omap | 19 | Brown 50% + Brown 50% Street infill | 0, 0.19, 0.31, 0.16 | yes | 19 | 830 | 572 / 258 / 0 / 0 |
| teanaway-omap | 20 | Black 100%Road borders + Black + All color separations | 0, 0, 0, 1 | yes | 17 774 | 219 449 | 180 277 / 3 580 / 35 362 / 230 |

## Question 2: the union of a run

The three ways: **one after the other** is `Op(Union)` of each mark with the result so far. **By pairs** unites the
marks two by two, then the results two by two. **Builder** is `SKPath.OpBuilder` with each mark added by union, and one
`Resolve`. The limit of one run is 10 s for one after the other and for the builder, and 600 s (`LordHill.ocd`) or
240 s (`McHugh Creek.omap`) for by pairs. On the two large maps the limits are 5 s and 60 s. The builder cannot be
stopped from outside, so its runs go from the smallest to the largest, and its series ends at the first run that
passes the limit: the larger runs are "not measured".

**Question 2: the union of each run, the totals of each way.**

From `<map>/q2-union.tsv and <map>/q2-union-builder.tsv`.

| Map | Way | Runs | Runs measured | Gave a result | Stopped by the limit | Operations with no result | Time, s | Largest run with a result, segments | Differing samples | Thick differing samples | Largest group | Equal when made again |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| LordHill-ocd | OneAfterTheOther | 20 | 20 | 16 | 4 | 0 | 61.6 | 4 196 | 2 912 | 0 | 6 | 13 of 13 |
| LordHill-ocd | ByPairs | 20 | 20 | 20 | 0 | 0 | 166.6 | 172 376 | 221 152 | 198 914 | 38 496 | 18 of 18 |
| LordHill-ocd | Builder | 20 | 17 | 16 | 1 | 0 | 30.2 | 4 196 | 2 912 | 0 | 6 | 13 of 13 |
| McHugh_Creek-omap | OneAfterTheOther | 17 | 17 | 11 | 6 | 0 | 68.8 | 2 184 | 317 | 0 | 3 | 11 of 11 |
| McHugh_Creek-omap | ByPairs | 17 | 17 | 17 | 0 | 2 | 322.7 | 256 605 | 10 587 | 252 | 324 | 11 of 11 |
| McHugh_Creek-omap | Builder | 17 | 12 | 11 | 1 | 0 | 11.4 | 2 184 | 317 | 0 | 3 | 11 of 11 |
| TeanawayValley-ocd | OneAfterTheOther | 21 | 21 | 11 | 10 | 0 | 53.3 | 4 455 | 68 | 0 | 3 | 11 of 11 |
| TeanawayValley-ocd | ByPairs | 20 | 20 | 15 | 5 | 3 | 408.9 | 62 009 | 3 644 | 0 | 8 | 11 of 11 |
| TeanawayValley-ocd | Builder | 21 | 12 | 11 | 1 | 0 | 6.8 | 4 455 | 68 | 0 | 3 | 11 of 11 |
| teanaway-omap | OneAfterTheOther | 21 | 21 | 11 | 10 | 0 | 55.5 | 4 455 | 68 | 0 | 3 | 11 of 11 |
| teanaway-omap | ByPairs | 20 | 20 | 15 | 5 | 4 | 399.4 | 62 017 | 3 703 | 0 | 8 | 11 of 11 |
| teanaway-omap | Builder | 21 | 12 | 11 | 1 | 0 | 5.8 | 4 455 | 68 | 0 | 3 | 11 of 11 |

- **By pairs is the only way that unites a large run.** It gave a result for each run of `LordHill.ocd` and of
  `McHugh Creek.omap`. One after the other, and the builder, pass 10 s at a run of 687 marks and 3 628 segments,
  which by pairs unites in 0.16 s. For run 7 of `LordHill.ocd` (712 marks) the times are 8.5 s one after the other,
  9.7 s with the builder and 0.10 s by pairs. The largest run that the builder united within its limit has 4 455
  segments. With the machine alone (`<results>/smoke`) the builder unites the run of 687 marks in 6.5 s, and passes
  10 s at the next larger run, of 1 129 marks and 8 099 segments.
- **By pairs is slow for the largest runs.** The brown runs (the contours) take 144 s in `LordHill.ocd` (2 411 marks,
  172 376 segments) and 151 s in `McHugh Creek.omap`. The time does not follow the size only: the "Yellow 50%" run of
  `McHugh Creek.omap` has 21 686 segments and takes 77 s. On each of the two large maps, 5 runs passed the limit of
  60 s, and the stage was stopped at its own limit of 480 s before the last run.
- **One union is wrong, and Skia does not say so.** The union by pairs of the brown run of `LordHill.ocd` differs from
  its marks at 198 914 thick samples of the window (124 mm2), with a largest group of 38 496 samples. No operation
  gave "no result". `<results>/LordHill-ocd/q2-union-ByPairs-run-10-difference.png` shows the union: it fills the
  land between contour lines, up to a straight line that no mark has. In that image 207 991 samples are in the union
  and in no mark, and 9 182 samples of the marks are not in the union. A second run and a third run of the stage
  gave the same digest for this union, so the wrong result repeats.
- **`Op(Union)` gave no result 2 times** in the last black run of `McHugh Creek.omap` (12 252 marks), and 7 times in
  four runs of the two large maps, which the limit stopped later. The probe then keeps the first path of the pair,
  so the marks of the second path are lost: the union of that black run differs at 252 thick samples.
- **Each other union differs from its marks in single samples only:** at most 2 274 samples of a run in the window
  of 16 000 000, none of them thick, in groups of at most 8. The outline of a union is not, bit for bit, the outline
  of its marks.
- The count of segments falls where marks overlap along their length (the brown run of `LordHill.ocd`: 172 376 in,
  121 939 out) and rises where area marks cross (run 1 of `McHugh Creek.omap`: 21 686 in, 37 356 out).

**Question 2: the union of each run. Each cell gives the time, and then the samples of the window that differ (thick ones in brackets).**

From `<map>/q2-union.tsv and <map>/q2-union-builder.tsv`.

| Map | Run | Colour | Marks | Segments in | One after the other | By pairs | Builder | By pairs: contours, segments out |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| LordHill-ocd | 0 | Yellow 50% | 109 | 1 417 | 0.738 s, 358 (0) | 0.182 s, 358 (0) | 0.690 s, 358 (0) | 87, 1 520 |
| LordHill-ocd | 1 | Yellow | 26 | 298 | 0.020 s, 92 (0) | 0.019 s, 91 (0) | 0.020 s, 92 (0) | 25, 370 |
| LordHill-ocd | 2 | White for yellow + White for green + White for road under construct. + White background | 687 | 3 628 | stopped at 10.0 s | 0.161 s, 38 (0) | stopped at 10.0 s | 683, 3 669 |
| LordHill-ocd | 3 | Black 30% | 10 | 51 | 0.001 s, 12 (0) | 0.001 s, 12 (0) | 0.000 s, 12 (0) | 10, 59 |
| LordHill-ocd | 4 | Green 30% | 142 | 1 725 | 0.818 s, 498 (0) | 0.085 s, 498 (0) | 0.688 s, 498 (0) | 156, 2 096 |
| LordHill-ocd | 5 | Green 60% | 144 | 4 196 | 3.599 s, 971 (0) | 0.294 s, 970 (0) | 3.300 s, 971 (0) | 240, 4 428 |
| LordHill-ocd | 6 | Green | 238 | 3 509 | 5.419 s, 897 (0) | 0.170 s, 897 (0) | 4.318 s, 897 (0) | 226, 3 815 |
| LordHill-ocd | 7 | Dark green | 712 | 3 364 | 8.538 s, 20 (0) | 0.099 s, 20 (0) | 9.669 s, 20 (0) | 713, 3 232 |
| LordHill-ocd | 8 | White for yellow + White for green + White for road under construct. + White background | 135 | 2 160 | 0.733 s, 13 (0) | 0.049 s, 13 (0) | 0.487 s, 13 (0) | 263, 2 152 |
| LordHill-ocd | 9 | Green above white | 159 | 2 448 | 1.014 s, 13 (0) | 0.067 s, 13 (0) | 0.979 s, 13 (0) | 287, 2 440 |
| LordHill-ocd | 10 | Brown | 2 411 | 172 376 | stopped at 10.6 s | 143.875 s, 217 173 (198 914) | not measured | 2 012, 121 939 |
| LordHill-ocd | 11 | Brown 50% + Street infill | 2 | 10 | 0.000 s | 0.000 s | 0.006 s | 2, 10 |
| LordHill-ocd | 12 | Blue 50% | 3 | 160 | 0.001 s, 12 (0) | 0.001 s, 12 (0) | 0.010 s, 12 (0) | 5, 191 |
| LordHill-ocd | 13 | Blue | 1 129 | 8 099 | stopped at 10.0 s | 0.440 s, 115 (0) | not measured | 1 084, 7 083 |
| LordHill-ocd | 14 | Street borderlines + Black | 12 | 1 126 | 0.008 s, 9 (0) | 0.009 s, 9 (0) | 0.009 s, 9 (0) | 12, 814 |
| LordHill-ocd | 15 | Brown 50% + Street infill | 6 | 88 | 0.001 s, 12 (0) | 0.001 s, 12 (0) | 0.001 s, 12 (0) | 6, 86 |
| LordHill-ocd | 16 | Street borderlines + Black | 6 273 | 66 065 | stopped at 10.0 s | 21.171 s, 916 (0) | not measured | 4 904, 51 941 |
| LordHill-ocd | 17 | Red 75% | 1 | 13 | 0.000 s | 0.000 s | 0.000 s | 1, 12 |
| LordHill-ocd | 18 | Black above whiteout | 17 | 191 | 0.014 s, 0 (0) | 0.001 s, 0 (0) | 0.002 s, 0 (0) | 10, 147 |
| LordHill-ocd | 19 | Purple transparent + Purple | 10 | 41 | 0.001 s, 5 (0) | 0.000 s, 5 (0) | 0.001 s, 5 (0) | 11, 43 |
| McHugh_Creek-omap | 0 | Purple + Purple for track symbols | 1 | 4 | 0.001 s | 0.001 s | 0.001 s | 1, 4 |
| McHugh_Creek-omap | 1 | Yellow 50% | 988 | 21 686 | stopped at 16.9 s | 77.091 s, 2 014 (0) | not measured | 1 611, 37 356 |
| McHugh_Creek-omap | 2 | Green 100% for undergrowth + Green 100% for area features | 7 184 | 61 759 | stopped at 10.1 s | 34.474 s, 288 (0) | not measured | 9 121, 69 626 |
| McHugh_Creek-omap | 3 | Green 30% + Green 30% for narrow ride | 1 016 | 13 777 | stopped at 10.1 s | 7.478 s, 2 274 (0) | not measured | 1 231, 23 965 |
| McHugh_Creek-omap | 4 | Green 60% + Green 60% for narrow ride | 926 | 11 482 | stopped at 10.0 s | 3.419 s, 2 004 (0) | stopped at 10.0 s | 1 066, 19 852 |
| McHugh_Creek-omap | 5 | Green 100% for undergrowth + Green 100% for area features | 200 | 1 193 | 1.147 s, 317 (0) | 0.067 s, 317 (0) | 1.147 s, 317 (0) | 203, 2 022 |
| McHugh_Creek-omap | 6 | White over yellow + White over green + White for railway + White for course overprint | 2 | 8 | 0.000 s | 0.000 s | 0.000 s | 1, 12 |
| McHugh_Creek-omap | 7 | Blue 100% for area features | 1 | 14 | 0.000 s | 0.000 s | 0.000 s | 1, 21 |
| McHugh_Creek-omap | 8 | Black for open land + Black below lower brown 50% + Black below upper brown 50% + Black below purple for track symbols + Black 100% | 2 | 2 184 | 0.033 s, 0 (0) | 0.077 s, 0 (0) | 0.047 s, 0 (0) | 2, 2 084 |
| McHugh_Creek-omap | 9 | Lower brown 50% + Upper brown 50% | 5 | 421 | 0.003 s, 0 (0) | 0.007 s, 0 (0) | 0.003 s, 0 (0) | 10, 411 |
| McHugh_Creek-omap | 10 | Black for open land + Black below lower brown 50% + Black below upper brown 50% + Black below purple for track symbols + Black 100% | 12 | 1 306 | 0.044 s, 0 (0) | 0.019 s, 0 (0) | 0.020 s, 0 (0) | 20, 746 |
| McHugh_Creek-omap | 11 | Lower brown 50% + Upper brown 50% | 2 | 2 184 | 0.044 s, 0 (0) | 0.034 s, 0 (0) | 0.051 s, 0 (0) | 2, 2 082 |
| McHugh_Creek-omap | 12 | Black for open land + Black below lower brown 50% + Black below upper brown 50% + Black below purple for track symbols + Black 100% | 8 | 32 | 0.000 s | 0.000 s | 0.000 s | 8, 32 |
| McHugh_Creek-omap | 13 | Brown 100% | 1 827 | 160 484 | stopped at 10.3 s | 150.775 s, 1 151 (0) | not measured | 1 377, 146 442 |
| McHugh_Creek-omap | 14 | Blue 100% | 46 | 1 745 | 0.176 s | 0.062 s | 0.163 s | 43, 1 546 |
| McHugh_Creek-omap | 15 | Green 100% | 2 | 8 | 0.000 s | 0.000 s | 0.000 s | 1, 12 |
| McHugh_Creek-omap | 16 | Black for open land + Black below lower brown 50% + Black below upper brown 50% + Black below purple for track symbols + Black 100% | 12 252 | 256 605 | stopped at 10.0 s | 49.166 s, 2 539 (252) | not measured | 5 362, 97 465 |
| TeanawayValley-ocd | 0 | Yellow 50% | 5 854 | 123 416 | stopped at 5.0 s | stopped at 65.8 s | not measured |  |
| TeanawayValley-ocd | 1 | White for yellow + White for green + White mask | 98 295 | 561 298 | stopped at 5.0 s | stopped at 60.0 s | not measured |  |
| TeanawayValley-ocd | 2 | Yellow + Line yellow | 13 855 | 91 085 | stopped at 5.0 s | stopped at 63.8 s | not measured |  |
| TeanawayValley-ocd | 3 | Grey 100% + Black 30% | 160 | 1 800 | 1.156 s, 68 (0) | 0.066 s, 68 (0) | 0.638 s, 68 (0) | 173, 1 927 |
| TeanawayValley-ocd | 4 | Green 20% | 2 430 | 58 620 | stopped at 5.0 s | 19.491 s, 1 450 (0) | not measured | 2 404, 61 102 |
| TeanawayValley-ocd | 5 | Green 30% | 6 674 | 217 193 | stopped at 5.0 s | stopped at 60.3 s | not measured |  |
| TeanawayValley-ocd | 6 | Green 50% | 2 562 | 62 009 | stopped at 5.0 s | 29.791 s, 726 (0) | not measured | 4 727, 65 950 |
| TeanawayValley-ocd | 7 | White for yellow + White for green + White mask | 146 | 584 | 0.446 s | 0.024 s | 0.414 s | 126, 576 |
| TeanawayValley-ocd | 8 | Green | 4 932 | 28 931 | stopped at 5.0 s | 11.475 s, 1 375 (0) | stopped at 5.0 s | 4 680, 27 394 |
| TeanawayValley-ocd | 9 | Yellow + Line yellow | 8 | 253 | 0.008 s | 0.011 s | 0.007 s | 6, 250 |
| TeanawayValley-ocd | 10 | Brown 50% + Brown 50% Street infill | 7 | 63 | 0.000 s | 0.012 s | 0.000 s | 6, 60 |
| TeanawayValley-ocd | 11 | Blue 50% | 3 | 13 | 0.000 s | 0.000 s | 0.000 s | 3, 12 |
| TeanawayValley-ocd | 12 | Yellow/Green 100/50% | 3 | 26 | 0.006 s | 0.000 s | 0.000 s | 3, 24 |
| TeanawayValley-ocd | 13 | Brown | 12 171 | 747 223 | stopped at 5.4 s | stopped at 73.5 s | not measured |  |
| TeanawayValley-ocd | 14 | Blue | 4 269 | 44 405 | stopped at 5.0 s | 24.393 s, 25 (0) | not measured | 4 129, 36 970 |
| TeanawayValley-ocd | 15 | Red 100% | 1 | 13 | 0.000 s | 0.000 s | 0.002 s | 1, 13 |
| TeanawayValley-ocd | 16 | Purple | 5 | 20 | 0.000 s | 0.000 s | 0.000 s | 3, 28 |
| TeanawayValley-ocd | 17 | Black 100%Road borders + Black + All color separations | 39 | 4 455 | 1.113 s | 0.173 s | 0.676 s | 18, 3 771 |
| TeanawayValley-ocd | 18 | Black 40% Road infill | 36 | 152 | 0.023 s | 0.009 s | 0.010 s | 32, 148 |
| TeanawayValley-ocd | 19 | Brown 50% + Brown 50% Street infill | 19 | 830 | 0.037 s | 0.011 s | 0.026 s | 9, 625 |
| TeanawayValley-ocd | 20 | Black 100%Road borders + Black + All color separations | 19 115 | 221 474 | stopped at 5.0 s | no row | not measured |  |
| teanaway-omap | 0 | Yellow 50% | 5 854 | 123 413 | stopped at 5.0 s | stopped at 62.6 s | not measured |  |
| teanaway-omap | 1 | White for yellow + White for green + White mask | 98 284 | 560 093 | stopped at 5.0 s | stopped at 60.0 s | not measured |  |
| teanaway-omap | 2 | Yellow + Line yellow | 13 852 | 91 421 | stopped at 5.1 s | stopped at 62.8 s | not measured |  |
| teanaway-omap | 3 | Grey 100% + Black 30% | 160 | 1 800 | 1.050 s, 68 (0) | 0.093 s, 68 (0) | 0.291 s, 68 (0) | 173, 1 927 |
| teanaway-omap | 4 | Green 20% | 2 430 | 58 620 | stopped at 5.0 s | 19.711 s, 1 450 (0) | not measured | 2 404, 61 102 |
| teanaway-omap | 5 | Green 30% | 6 673 | 217 398 | stopped at 5.1 s | stopped at 63.3 s | not measured |  |
| teanaway-omap | 6 | Green 50% | 2 561 | 62 017 | stopped at 5.1 s | 31.352 s, 725 (0) | not measured | 4 749, 65 908 |
| teanaway-omap | 7 | White for yellow + White for green + White mask | 146 | 584 | 0.317 s | 0.051 s | 0.123 s | 126, 576 |
| teanaway-omap | 8 | Green | 4 930 | 29 697 | stopped at 5.0 s | 14.632 s, 1 435 (0) | stopped at 5.0 s | 4 680, 28 110 |
| teanaway-omap | 9 | Yellow + Line yellow | 8 | 253 | 0.004 s | 0.003 s | 0.002 s | 6, 250 |
| teanaway-omap | 10 | Brown 50% + Brown 50% Street infill | 7 | 63 | 0.000 s | 0.000 s | 0.000 s | 6, 60 |
| teanaway-omap | 11 | Blue 50% | 3 | 13 | 0.000 s | 0.000 s | 0.000 s | 3, 12 |
| teanaway-omap | 12 | Yellow/Green 100/50% | 3 | 26 | 0.000 s | 0.000 s | 0.000 s | 3, 24 |
| teanaway-omap | 13 | Brown | 11 602 | 744 835 | stopped at 8.0 s | stopped at 60.2 s | not measured |  |
| teanaway-omap | 14 | Blue | 4 089 | 43 742 | stopped at 5.2 s | 24.192 s, 25 (0) | not measured | 3 956, 36 379 |
| teanaway-omap | 15 | Red 100% | 1 | 13 | 0.000 s | 0.000 s | 0.001 s | 1, 13 |
| teanaway-omap | 16 | Purple | 5 | 20 | 0.000 s | 0.000 s | 0.000 s | 3, 28 |
| teanaway-omap | 17 | Black 100%Road borders + Black + All color separations | 39 | 4 455 | 0.548 s | 0.339 s | 0.322 s | 18, 3 771 |
| teanaway-omap | 18 | Black 40% Road infill | 36 | 152 | 0.010 s | 0.007 s | 0.006 s | 32, 148 |
| teanaway-omap | 19 | Brown 50% + Brown 50% Street infill | 19 | 830 | 0.015 s | 0.016 s | 0.009 s | 9, 625 |
| teanaway-omap | 20 | Black 100%Road borders + Black + All color separations | 17 774 | 219 449 | stopped at 5.0 s | no row | not measured |  |

## Question 3: the knockout at the size of a map

**Each run a knockout colour:** from the top run down, each run is cut by the union of the runs above it
(`Difference`), and then added to that union (`Union`). **Overprinted runs as they are:** a knockout run cuts each
run below it. Then, from the lowest run up, each overprinted run is intersected with each piece below it whose
bounds it meets: `Intersect` gives the part of the piece under the ink, `Difference` gives the rest of the piece,
and a second `Difference` takes the part away from what the ink covers on paper. A piece that already holds the
same ink is not divided. The unions of the runs are those of the way by pairs.

**The stages of each map: 124 as the exit code says that the time limit stopped the stage.**

From `<map>/stages.tsv`.

| Map | Stage | Exit code | Wall-clock seconds |
| --- | --- | --- | --- |
| LordHill-ocd | inventory | 0 | 6.5 |
| LordHill-ocd | tiles | 0 | 290.7 |
| LordHill-ocd | digests | 0 | 150.1 |
| LordHill-ocd | digests-second-process | 0 | 120.8 |
| LordHill-ocd | whole-map | 124 | 1800.0 |
| LordHill-ocd | tiles-uncut | 0 | 173.2 |
| LordHill-ocd | tiles-second-run | 0 | 349.2 |
| LordHill-ocd | builder | 0 | 36.4 |
| McHugh_Creek-omap | inventory | 0 | 71.8 |
| McHugh_Creek-omap | tiles | 0 | 332.3 |
| McHugh_Creek-omap | digests | 0 | 17.9 |
| McHugh_Creek-omap | digests-second-process | 0 | 19.4 |
| McHugh_Creek-omap | whole-map | 124 | 900.1 |
| McHugh_Creek-omap | tiles-uncut | 0 | 172.5 |
| McHugh_Creek-omap | tiles-second-run | 0 | 585.9 |
| McHugh_Creek-omap | builder | 0 | 14.9 |
| TeanawayValley-ocd | inventory | 0 | 363.3 |
| TeanawayValley-ocd | tiles | 0 | 579.0 |
| TeanawayValley-ocd | digests | 0 | 13.3 |
| TeanawayValley-ocd | digests-second-process | 0 | 15.3 |
| TeanawayValley-ocd | whole-map | 124 | 480.1 |
| TeanawayValley-ocd | builder | 0 | 12.1 |
| teanaway-omap | inventory | 0 | 357.0 |
| teanaway-omap | tiles | 0 | 565.3 |
| teanaway-omap | digests | 0 | 12.0 |
| teanaway-omap | digests-second-process | 0 | 14.1 |
| teanaway-omap | whole-map | 124 | 480.1 |
| teanaway-omap | builder | 0 | 7.8 |
| teanaway-omap | tiles-long | 0 | 767.9 |

**Question 3: the knockout over the whole map, the parts that ended.**

From `<map>/q3-knockout.tsv`.

| Map | Part | Runs with no union | Operations | With no result | Stopped by the limit | Time, s | Longest operation, s | Pieces | Contours | Segments | Samples that two pieces cover | Samples of marks with no piece | Samples of a piece with no mark |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |

**Question 3: the operations that ended, also of the parts that the limit stopped.**

From `<map>/q3-operations.tsv`.

| Map | Part | Operations that ended | With no result | Their time, s | The longest | Median, s | Longer than 1 s |
| --- | --- | --- | --- | --- | --- | --- | --- |
| LordHill-ocd | each run a knockout colour | 39 | 8 | 1 532.4 | 145.7 s: Union of 69 869 and 125 963 verbs | 39.9727 | 33 |
| LordHill-ocd | overprinted runs as they are | 0 |  |  |  |  |  |
| McHugh_Creek-omap | each run a knockout colour | 6 | 0 | 134.8 | 32.4 s: Difference of 14 and 108 193 verbs | 26.3330 | 6 |
| McHugh_Creek-omap | overprinted runs as they are | 0 |  |  |  |  |  |
| TeanawayValley-ocd | each run a knockout colour | 0 |  |  |  |  |  |
| TeanawayValley-ocd | overprinted runs as they are | 0 |  |  |  |  |  |
| teanaway-omap | each run a knockout colour | 0 |  |  |  |  |  |
| teanaway-omap | overprinted runs as they are | 0 |  |  |  |  |  |

**Question 3: the operation that ran when the time limit stopped the stage.**

From `<map>/q3-running-operation.tsv`.

| Map | Part | Run | Operation | Verbs of the left path | Verbs of the right path |
| --- | --- | --- | --- | --- | --- |
| LordHill-ocd | each run a knockout colour | 0 | Union | 191 159 | 1 694 |
| McHugh_Creek-omap | each run a knockout colour | 13 | Difference | 149 196 | 109 679 |

- **No map ended this cut within its limit in the run of the tables.** The limits of the stage are 1 800 s
  (`LordHill.ocd`), 900 s (`McHugh Creek.omap`) and 480 s (the two large maps). The table of the parts that ended
  is empty.
- **The time of an operation follows the size of its two paths, not the area that they share.** In `LordHill.ocd` the
  `Difference` of a run of 14 verbs and the union of 69 879 verbs above it takes 17 s. With 190 000 verbs above, each
  operation takes 40 s to 112 s. In `McHugh Creek.omap` the `Difference` of 14 verbs and 108 193 verbs takes 32 s.
- **`McHugh Creek.omap`:** 6 operations ended. The seventh, the `Difference` of the brown run (149 196 verbs) and the
  union of the three runs above it (109 679 verbs), ran for about 370 s and had not ended when the limit stopped the
  stage. Its start time is in `q3-running-operation.tsv`.
- **`LordHill.ocd`:** 39 operations ended, in 1 532 s. 8 of them gave no result, after 30 s to 60 s each: the `Union`
  of run 7, the `Difference` and the `Union` of the runs 6, 5 and 4, and the `Difference` of run 0. So the green runs
  4, 5 and 6 and the lowest yellow run have no piece. The limit stopped the stage in its last operation.
- **The cut of `LordHill.ocd` ended in the check run with the machine alone** (`<results>/smoke/LordHill-ocd/`, step 5
  of "The conditions"): 40 operations in 860 s, the longest 51 s, and 9 with no result, which are the 8 above and
  the last `Union`. It gives 15 pieces. 7 492 376 samples of the window that a mark covers have no piece, which is
  62 % of the samples with ink, and 11 235 samples are in two pieces. The part with the overprinted runs was stopped
  by hand before it ended.
- **The two large maps did not reach the cut.** The unions by pairs took the 480 s of the stage.
- **The part "overprinted runs as they are" did not start on any map** at this size. Question 6 has it in tiles.
- These times are of the loaded machine. A machine with nothing else to do is 2 to 3 times as fast, which does not
  change the answer.

## Question 4: edges that two areas share

The shapes are at (123.4, 87.6) mm, so that their coordinates are not round binary numbers. Each rectangle is 10 mm
by 10 mm, and the shared cubic curve goes from (10, 0) through the control points (14, 3) and (6, 7) to (10, 10).
The exact areas come from the coordinates of the shapes in double precision. For a row of "difference" the pieces
that are drawn are the difference and the second shape. For "intersection" they are the two differences and the
intersection. "Paper samples" are covered when the two shapes are drawn, and not covered when the pieces are drawn.
"Extra samples" are the reverse.

**Question 4: edges that two areas share.**

From `q4-shared-edges.tsv`.

| Shapes | Operation | Result | Contours | Contours below 0.000001 mm2 | Lines / quadratics / conics / cubics | Area, mm2 | Exact area, mm2 | Error of the area, mm2 | Paper samples at 40 | Extra samples at 40 | Paper samples at 400 | Extra samples at 400 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| rectangles, one whole edge | union | yes | 1 | 0 | 5/0/0/0 | 199.9999237 | 199.9999237 | 0 | 0 | 0 | 0 | 0 |
| rectangles, one whole edge | difference | yes | 1 | 0 | 4/0/0/0 | 99.9999237 | 99.9999237 | 0 | 0 | 0 | 0 | 0 |
| rectangles, one whole edge | intersection | yes | 0 | 0 | 0/0/0/0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 |
| rectangles, a part of an edge | union | yes | 1 | 0 | 8/0/0/0 | 139.9999237 | 139.9999237 | 0 | 0 | 0 | 0 | 0 |
| rectangles, a part of an edge | difference | yes | 1 | 0 | 4/0/0/0 | 99.9999237 | 99.9999237 | 0 | 0 | 0 | 0 | 0 |
| rectangles, a part of an edge | intersection | yes | 0 | 0 | 0/0/0/0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 |
| slanted edge, one side has one vertex more | union | yes | 1 | 0 | 5/0/0/0 | 199.9999237 | 199.9999161 | 0.0000076 | 0 | 0 | 0 | 47 |
| slanted edge, one side has one vertex more | difference | yes | 1 | 0 | 5/0/0/0 | 114.9999313 | 114.9999237 | 0.0000076 | 0 | 0 | 0 | 47 |
| slanted edge, one side has one vertex more | intersection | yes | 0 | 0 | 0/0/0/0 | 0 | 0 | 0 | 0 | 0 | 0 | 47 |
| cubic curve, same direction | union | yes | 1 | 0 | 5/0/0/0 | 199.9999237 | 199.9999237 | 0 | 0 | 0 | 0 | 0 |
| cubic curve, same direction | difference | yes | 1 | 0 | 3/0/0/2 | 99.9999237 | 99.9999237 | 0 | 0 | 0 | 11 | 0 |
| cubic curve, same direction | intersection | yes | 0 | 0 | 0/0/0/0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 |
| cubic curve, opposite direction | union | yes | 1 | 0 | 5/0/0/0 | 199.9999237 | 199.9999237 | 0 | 0 | 0 | 0 | 0 |
| cubic curve, opposite direction | difference | yes | 1 | 0 | 3/0/0/2 | 99.9999237 | 99.9999237 | 0 | 0 | 0 | 11 | 0 |
| cubic curve, opposite direction | intersection | yes | 0 | 0 | 0/0/0/0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 |
| a shape and its copy | union | yes | 1 | 0 | 0/0/0/4 | 78.5618027 | 78.5618027 | -0 | 0 | 0 | 0 | 0 |
| a shape and its copy | difference | yes | 0 | 0 | 0/0/0/0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 |
| a shape and its copy | intersection | yes | 1 | 0 | 0/0/0/4 | 78.5618027 | 78.5618027 | -0 | 0 | 0 | 0 | 0 |
| straight edges 0.0001 mm apart, a gap | union | yes | 2 | 0 | 8/0/0/0 | 199.9988556 | 199.9988556 | 0 | 0 | 0 | 0 | 0 |
| straight edges 0.0001 mm apart, a gap | difference | yes | 1 | 0 | 4/0/0/0 | 99.9999237 | 99.9999237 | 0 | 0 | 0 | 0 | 0 |
| straight edges 0.0001 mm apart, a gap | intersection | yes | 0 | 0 | 0/0/0/0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 |
| straight edges 0.0001 mm apart, an overlap | union | yes | 1 | 0 | 5/0/0/0 | 199.9999237 | 199.9999237 | 0 | 0 | 0 | 0 | 0 |
| straight edges 0.0001 mm apart, an overlap | difference | yes | 1 | 0 | 4/0/0/0 | 99.9994659 | 99.9990082 | 0.0004578 | 0 | 0 | 0 | 0 |
| straight edges 0.0001 mm apart, an overlap | intersection | yes | 1 | 0 | 3/0/0/0 | 0.0009155 | 0.0009155 | 0 | 0 | 0 | 0 | 0 |
| cubic curves 0.0001 mm apart, a gap | union | yes | 1 | 0 | 5/0/0/0 | 199.9999237 | 199.9988556 | 0.0010681 | 0 | 0 | 0 | 183 |
| cubic curves 0.0001 mm apart, a gap | difference | yes | 1 | 0 | 3/0/0/3 | 99.9999237 | 99.9999237 | -0 | 6 | 0 | 118 | 23 |
| cubic curves 0.0001 mm apart, a gap | intersection | yes | 0 | 0 | 0/0/0/0 | 0 | 0 | 0 | 0 | 0 | 0 | 172 |
| cubic curves 0.0001 mm apart, an overlap | union | yes | 1 | 0 | 5/0/0/0 | 199.9999237 | 199.9999237 | 0 | 0 | 0 | 0 | 0 |
| cubic curves 0.0001 mm apart, an overlap | difference | yes | 1 | 0 | 3/0/0/3 | 99.9990041 | 99.9988556 | 0.0001485 | 6 | 0 | 45 | 0 |
| cubic curves 0.0001 mm apart, an overlap | intersection | yes | 0 | 0 | 0/0/0/0 | 0 | 0.0010681 | -0.0010681 | 0 | 0 | 1 | 0 |

- **Straight edges are exact.** Two rectangles that share an edge, or a part of one, unite into one contour. Their
  intersection is empty, the difference is the first rectangle, and no sample shows paper at 40 or at 400.
- **No operation made a contour below 0.000001 mm2.** The overlap of 0.0001 mm of two straight edges gives one
  contour of 0.0009 mm2, which is its exact area.
- **A shared cubic curve is taken as shared in both directions.** The union has no curve left, and the intersection
  is empty. The difference keeps the curve as two cubics in place of one, with the exact area. At 40 samples for each
  millimetre nothing shows. At 400, 11 samples show paper, on an edge of 11.3 mm.
- **Curves 0.0001 mm apart are taken as one curve.** The union closes the gap (the area grows by 0.0011 mm2), and the
  intersection of the two shapes that overlap by 0.0001 mm is empty. The difference moves the edge: 6 samples show
  paper at 40, on an edge of 11.3 mm, and 45 to 118 at 400.
- **A straight overlap of 0.0001 mm** leaves the difference 0.0005 mm2 too large: half of the overlap stays in it.

## Question 5: the same result two times

The stage `digests` does the union by pairs and the two cuts of question 3 on the runs of at most 10 000 segments,
two times in one process. It ran in two processes. A digest is the SHA-256 of the fill type of a path and of the
kind and the coordinates of each segment, bit for bit. `<results>/digests.txt` holds the digests of the first pass
of the first process, for a run on another machine. The tables of question 2 also give the digest of each union,
and say if a union that took less than 2 s gave the same digest when it was made again.

**Question 5: the same result two times.**

From `<map>/q5-two-passes.tsv, digests.txt and second-process/digests.txt`.

| Map | Runs | Runs of at most 10 000 segments | Results | Results with no path | Equal the second time in one process | Equal in the second process | Time of one pass, s |
| --- | --- | --- | --- | --- | --- | --- | --- |
| LordHill-ocd | 20 | 18 | 87 | 1 | 87 | 87 of 87 | 97.7 |
| McHugh_Creek-omap | 17 | 11 | 37 | 0 | 37 | 37 of 37 | 8.7 |
| TeanawayValley-ocd | 21 | 11 | 33 | 0 | 33 | 33 of 33 | 4.9 |
| teanaway-omap | 21 | 11 | 39 | 0 | 39 | 39 of 39 | 4.9 |

**Questions 5 and 6: the pieces of the tiles, made again in a second process.**

From `<map>/q6-tiles.tsv and tiles-second-run/<map>/q6-tiles.tsv`.

| Map | Tile, mm | Pieces | The digest of all pieces is equal | Time of the first run, s | Time of the second run, s |
| --- | --- | --- | --- | --- | --- |
| LordHill-ocd | 10 | 1 293 | yes | 24.0 | 31.6 |
| LordHill-ocd | 25 | 350 | yes | 74.6 | 77.5 |
| McHugh_Creek-omap | 10 | 4 216 | yes | 71.3 | 60.6 |
| McHugh_Creek-omap | 25 | 924 | yes | 84.7 | 47.4 |

- **Each result that was made two times has the same digest**, in one process and in two processes: the 196 results
  of the stage `digests`, and the 143 unions of question 2 that took less than 2 s.
- A failure repeats too. One of the 196 results is "no path": `Op(Difference)` gives no result for run 5 of
  `LordHill.ocd` each time.
- **Large results repeat too.** The digest of all pieces of all tiles is equal in the second run, for each size and
  map of the table. The 20 unions by pairs of `LordHill.ocd`, the wrong union included, have the same digests in a
  second run of the stage `whole-map`; that run was stopped by hand after the unions and is in no table.
- **A later build of the same code repeats too.** The call of the final script in `<results>/smoke` gave the file
  of the marks of `LordHill.ocd` again bit for bit, the same 87 digests in two more processes, and the same digests
  for the 16 unions that the builder made in both runs.
- **Not measured:** another machine, another processor, and another build of Skia. `<results>/digests.txt` holds the
  196 digests for that comparison.

## Question 6: tiles

The marks are cut to the row of tiles and then to each tile, with `Op(Intersect)` of a mark and a rectangle. A mark
whose bounds are inside the tile is not cut. In each tile each run is united by pairs, and the runs are cut as
knockout colours, as in question 3. The limit of the tiles of one size is 180 s in the stage `tiles`, 600 s in
`tiles-second-run` and `tiles-uncut`, and 900 s in `tiles-long`. "Paper samples" are samples of the window that a
mark covers and no piece covers. The borders are counted in strips of 8 samples to each side of each border between
tiles, inside the window.

**Question 6: tiles. A run that a difference could not cut has no piece in its tile.**

From `<map>/q6-tiles.tsv`.

| Map | Tile, mm | Tiles (with marks) | Stopped by the limit | Cut, s | Union, s | Knockout, s | All, s | Cuts (no result) | Unions with no result | Knockout operations (no result) | Longest knockout operation, s | Pieces | Segments | Paper samples in the window at 40 | On the borders at 40 | On the borders at 400 (of samples) |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| LordHill-ocd | 10 | 285 (234) | no | 7.5 | 3.4 | 13.1 | 24.0 | 6 526 (0) | 0 | 2 768 (25) | 0.040 | 1 293 | 409 738 | 403 398 | 24 959 | 230 170 (of 13 280 000) |
| LordHill-ocd | 25 | 48 (47) | no | 4.2 | 8.7 | 61.7 | 74.6 | 2 581 (0) | 0 | 782 (32) | 0.597 | 350 | 365 238 | 2 192 604 | 60 647 | 603 853 (of 5 280 000) |
| LordHill-ocd | 50 | 11 (11) | yes | 2.2 | 18.3 | 159.9 | 180.5 | 981 (0) | 0 | 244 (19) | 6.048 | 105 | 240 695 |  |  |  |
| McHugh_Creek-omap | 10 | 1 148 (773) | no | 33.4 | 9.3 | 28.5 | 71.3 | 32 680 (0) | 0 | 8 554 (14) | 0.037 | 4 216 | 952 615 | 31 052 | 2 580 | 19 529 (of 13 280 000) |
| McHugh_Creek-omap | 25 | 204 (145) | no | 15.8 | 13.6 | 55.4 | 84.7 | 13 579 (0) | 1 | 1 886 (24) | 0.200 | 924 | 897 189 | 148 023 | 6 401 | 52 332 (of 5 280 000) |
| McHugh_Creek-omap | 50 | 54 (43) | no | 10.2 | 22.7 | 130.6 | 163.6 | 7 587 (0) | 0 | 672 (14) | 1.195 | 327 | 846 254 | 2 582 393 | 42 143 | 364 353 (of 2 640 000) |
| TeanawayValley-ocd | 10 | 1 154 (796) | yes | 77.8 | 28.4 | 74.2 | 180.5 | 37 791 (0) | 1 | 9 076 (19) | 0.256 | 4 519 | 1 824 609 |  |  |  |
| TeanawayValley-ocd | 25 | 144 (94) | yes | 19.1 | 23.9 | 139.1 | 182.0 | 7 772 (0) | 1 | 1 266 (6) | 2.907 | 630 | 853 309 |  |  |  |
| TeanawayValley-ocd | 50 | 25 (16) | yes | 7.1 | 15.5 | 191.0 | 213.6 | 1 787 (0) | 2 | 238 (3) | 8.258 | 118 | 297 921 |  |  |  |
| teanaway-omap | 10 | 1 184 (820) | yes | 84.3 | 28.6 | 70.5 | 183.4 | 40 506 (0) | 3 | 9 358 (17) | 0.186 | 4 644 | 1 767 708 |  |  |  |
| teanaway-omap | 25 | 147 (97) | yes | 19.5 | 26.6 | 135.7 | 181.8 | 8 199 (0) | 2 | 1 314 (6) | 2.066 | 653 | 862 783 |  |  |  |
| teanaway-omap | 50 | 25 (16) | yes | 7.6 | 16.1 | 173.8 | 197.5 | 1 805 (0) | 1 | 238 (4) | 6.441 | 117 | 278 070 |  |  |  |

**Question 6: tiles of the second run, in a process of its own.**

From `tiles-second-run/<map>/q6-tiles.tsv`.

| Map | Tile, mm | Tiles (with marks) | Stopped by the limit | Cut, s | Union, s | Knockout, s | All, s | Cuts (no result) | Unions with no result | Knockout operations (no result) | Longest knockout operation, s | Pieces | Segments | Paper samples in the window at 40 | On the borders at 40 | On the borders at 400 (of samples) |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| LordHill-ocd | 5 | 1 073 (768) | no | 22.2 | 3.4 | 11.4 | 37.2 | 13 905 (0) | 0 | 7 142 (18) | 0.041 | 3 415 | 435 974 | 138 963 | 21 222 | 209 970 (of 26 480 000) |
| LordHill-ocd | 10 | 285 (234) | no | 10.2 | 4.3 | 17.1 | 31.6 | 6 526 (0) | 0 | 2 768 (25) | 0.067 | 1 293 | 409 738 | 403 398 | 24 959 | 230 170 (of 13 280 000) |
| LordHill-ocd | 25 | 48 (47) | no | 4.4 | 9.4 | 63.7 | 77.5 | 2 581 (0) | 0 | 782 (32) | 0.681 | 350 | 365 238 | 2 192 604 | 60 647 | 603 853 (of 5 280 000) |
| McHugh_Creek-omap | 5 | 4 536 (2 857) | no | 72.8 | 8.3 | 25.0 | 106.4 | 66 293 (0) | 1 | 27 196 (7) | 0.023 | 13 385 | 1 046 325 | 27 610 | 3 556 | 27 360 (of 26 520 000) |
| McHugh_Creek-omap | 10 | 1 148 (773) | no | 29.0 | 7.7 | 23.9 | 60.6 | 32 680 (0) | 0 | 8 554 (14) | 0.046 | 4 216 | 952 615 | 31 052 | 2 580 | 19 529 (of 13 280 000) |
| McHugh_Creek-omap | 25 | 204 (145) | no | 8.4 | 8.0 | 31.0 | 47.4 | 13 579 (0) | 1 | 1 886 (24) | 0.123 | 924 | 897 189 | 148 023 | 6 401 | 52 332 (of 5 280 000) |

**Question 6: tiles of 10 mm with a limit of 900 s.**

From `tiles-long/<map>/q6-tiles.tsv`.

| Map | Tile, mm | Tiles (with marks) | Stopped by the limit | Cut, s | Union, s | Knockout, s | All, s | Cuts (no result) | Unions with no result | Knockout operations (no result) | Longest knockout operation, s | Pieces | Segments | Paper samples in the window at 40 | On the borders at 40 | On the borders at 400 (of samples) |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| teanaway-omap | 10 | 2 812 (1 760) | no | 132.6 | 52.2 | 130.0 | 314.9 | 78 690 (0) | 3 | 20 442 (28) | 0.196 | 10 164 | 3 532 434 | 62 145 | 6 104 | 53 014 (of 13 120 000) |

**Question 6: the two cuts of question 3 in the tiles, with the samples of the window.**

From `<map>/q6-tiles-knockout.tsv, and the same file under tiles-second-run/ and tiles-long/`.

| Map | Part | Stage | Operations | With no result | Stopped by the limit | Time, s | Longest operation, s | Pieces | Contours | Segments | Samples that two pieces cover | Samples of marks with no piece | Samples of a piece with no mark |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| LordHill-ocd | tiles of 5 mm, each run a knockout colour | tiles-second-run | 7 142 | 18 | no | 11.4 | 0.041 | 3 415 | 30 449 | 435 974 | 72 234 | 138 963 | 560 |
| LordHill-ocd | tiles of 5 mm, overprinted runs as they are | tiles-second-run | 23 033 | 9 | no | 31.6 | 0.079 | 8 200 | 54 542 | 571 519 | 148 970 | 27 098 | 587 |
| LordHill-ocd | tiles of 10 mm, each run a knockout colour | tiles-second-run | 2 768 | 25 | no | 17.1 | 0.067 | 1 293 | 25 213 | 409 738 | 316 189 | 403 398 | 905 |
| LordHill-ocd | tiles of 10 mm, overprinted runs as they are | tiles-second-run | 10 982 | 4 | no | 37.4 | 0.159 | 3 806 | 48 880 | 548 453 | 188 865 | 121 171 | 597 |
| LordHill-ocd | tiles of 25 mm, each run a knockout colour | tiles-second-run | 782 | 32 | no | 63.7 | 0.681 | 350 | 20 883 | 365 238 | 191 110 | 2 192 604 | 479 |
| LordHill-ocd | tiles of 25 mm, overprinted runs as they are | tiles-second-run | 4 109 | 11 | no | 92.4 | 3.263 | 1 370 | 45 714 | 553 158 | 564 181 | 113 931 | 582 |
| McHugh_Creek-omap | tiles of 5 mm, each run a knockout colour | tiles-second-run | 27 196 | 7 | no | 25.0 | 0.023 | 13 385 | 135 626 | 1 046 325 | 28 874 | 27 610 | 1 375 |
| McHugh_Creek-omap | tiles of 5 mm, overprinted runs as they are | tiles-second-run | 85 509 | 14 | no | 99.6 | 0.062 | 31 371 | 311 801 | 1 901 228 | 50 858 | 17 131 | 1 639 |
| McHugh_Creek-omap | tiles of 10 mm, each run a knockout colour | tiles-second-run | 8 554 | 14 | no | 23.9 | 0.046 | 4 216 | 112 709 | 952 615 | 79 339 | 31 052 | 4 115 |
| McHugh_Creek-omap | tiles of 10 mm, overprinted runs as they are | tiles-second-run | 32 029 | 15 | no | 91.1 | 0.151 | 11 502 | 281 122 | 1 778 359 | 50 656 | 12 178 | 1 968 |
| McHugh_Creek-omap | tiles of 25 mm, each run a knockout colour | tiles-second-run | 1 886 | 24 | no | 31.0 | 0.123 | 924 | 100 274 | 897 189 | 216 533 | 148 023 | 6 226 |
| McHugh_Creek-omap | tiles of 25 mm, overprinted runs as they are | tiles-second-run | 8 545 | 17 | no | 109.9 | 0.908 | 2 954 | 261 484 | 1 701 406 | 293 207 | 190 249 | 4 101 |
| teanaway-omap | tiles of 10 mm, each run a knockout colour | tiles-long | 20 442 | 28 | no | 130.0 | 0.196 | 10 164 | 327 719 | 3 532 434 | 33 183 | 62 145 | 2 370 |
| teanaway-omap | tiles of 10 mm, overprinted runs as they are | tiles-long | 126 651 | 44 | no | 447.1 | 0.991 | 39 526 | 640 021 | 5 266 294 | 229 830 | 7 673 | 13 187 |

The stage `tiles-second-run` also checks each result. It draws the result and its operands over the tile at 40
samples for each millimetre. A result is **wrong** when it differs from what the samples of its operands give at one
thick sample or more. A union of a run is checked against its marks, a `Difference` against the first path less the
second, and a `Union` against the two paths together.

**Question 6: the operations in the tiles, each result checked against the samples of its operands.**

From `<map>/q6-tiles-operations.tsv, or tiles-second-run/<map>/q6-tiles-operations.tsv`.

| Map | Tile, mm | Operation | Operations | With no result | With a wrong result |
| --- | --- | --- | --- | --- | --- |
| LordHill-ocd | 5 | union of the run by pairs | 3 571 | 0 | 1 |
| LordHill-ocd | 5 | Difference | 3 571 | 9 | 101 |
| LordHill-ocd | 5 | Union | 3 571 | 9 | 98 |
| LordHill-ocd | 10 | union of the run by pairs | 1 384 | 0 | 1 |
| LordHill-ocd | 10 | Difference | 1 384 | 9 | 83 |
| LordHill-ocd | 10 | Union | 1 384 | 16 | 79 |
| LordHill-ocd | 25 | union of the run by pairs | 391 | 0 | 1 |
| LordHill-ocd | 25 | Difference | 391 | 14 | 37 |
| LordHill-ocd | 25 | Union | 391 | 18 | 38 |
| McHugh_Creek-omap | 5 | union of the run by pairs | 13 598 | 1 | 3 |
| McHugh_Creek-omap | 5 | Difference | 13 598 | 3 | 55 |
| McHugh_Creek-omap | 5 | Union | 13 598 | 4 | 43 |
| McHugh_Creek-omap | 10 | union of the run by pairs | 4 277 | 0 | 2 |
| McHugh_Creek-omap | 10 | Difference | 4 277 | 4 | 37 |
| McHugh_Creek-omap | 10 | Union | 4 277 | 10 | 34 |
| McHugh_Creek-omap | 25 | union of the run by pairs | 943 | 1 | 5 |
| McHugh_Creek-omap | 25 | Difference | 943 | 10 | 31 |
| McHugh_Creek-omap | 25 | Union | 943 | 14 | 27 |

**Question 6: the operations in the tiles that gave no result or a wrong result, by run.**

From `<map>/q6-tiles-operations.tsv, or tiles-second-run/<map>/q6-tiles-operations.tsv`.

| Map | Tile, mm | Run | Colour | Inside a blending | Operation | Operations | With no result | With a wrong result |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| LordHill-ocd | 5 | 10 | Brown | yes | union of the run by pairs | 596 | 0 | 1 |
| LordHill-ocd | 5 | 0 | Yellow 50% | no | Difference | 256 | 3 | 26 |
| LordHill-ocd | 5 | 0 | Yellow 50% | no | Union | 256 | 2 | 28 |
| LordHill-ocd | 5 | 1 | Yellow | no | Difference | 47 | 0 | 1 |
| LordHill-ocd | 5 | 1 | Yellow | no | Union | 47 | 0 | 1 |
| LordHill-ocd | 5 | 4 | Green 30% | no | Difference | 225 | 1 | 15 |
| LordHill-ocd | 5 | 4 | Green 30% | no | Union | 225 | 1 | 13 |
| LordHill-ocd | 5 | 5 | Green 60% | no | Difference | 447 | 3 | 38 |
| LordHill-ocd | 5 | 5 | Green 60% | no | Union | 447 | 4 | 31 |
| LordHill-ocd | 5 | 6 | Green | no | Difference | 396 | 1 | 20 |
| LordHill-ocd | 5 | 6 | Green | no | Union | 396 | 1 | 20 |
| LordHill-ocd | 5 | 7 | Dark green | no | Difference | 186 | 1 | 1 |
| LordHill-ocd | 5 | 7 | Dark green | no | Union | 186 | 1 | 5 |
| LordHill-ocd | 10 | 10 | Brown | yes | union of the run by pairs | 174 | 0 | 1 |
| LordHill-ocd | 10 | 0 | Yellow 50% | no | Difference | 101 | 3 | 24 |
| LordHill-ocd | 10 | 0 | Yellow 50% | no | Union | 101 | 4 | 20 |
| LordHill-ocd | 10 | 2 | White for yellow + White for green + White for road under construct. + White background | no | Difference | 36 | 0 | 1 |
| LordHill-ocd | 10 | 2 | White for yellow + White for green + White for road under construct. + White background | no | Union | 36 | 0 | 2 |
| LordHill-ocd | 10 | 4 | Green 30% | no | Difference | 90 | 0 | 15 |
| LordHill-ocd | 10 | 4 | Green 30% | no | Union | 90 | 1 | 14 |
| LordHill-ocd | 10 | 5 | Green 60% | no | Difference | 140 | 4 | 24 |
| LordHill-ocd | 10 | 5 | Green 60% | no | Union | 140 | 6 | 21 |
| LordHill-ocd | 10 | 6 | Green | no | Difference | 138 | 1 | 18 |
| LordHill-ocd | 10 | 6 | Green | no | Union | 138 | 2 | 17 |
| LordHill-ocd | 10 | 7 | Dark green | no | Difference | 78 | 1 | 1 |
| LordHill-ocd | 10 | 7 | Dark green | no | Union | 78 | 3 | 5 |
| LordHill-ocd | 25 | 10 | Brown | yes | union of the run by pairs | 34 | 0 | 1 |
| LordHill-ocd | 25 | 0 | Yellow 50% | no | Difference | 30 | 5 | 9 |
| LordHill-ocd | 25 | 0 | Yellow 50% | no | Union | 30 | 6 | 8 |
| LordHill-ocd | 25 | 1 | Yellow | no | Difference | 16 | 0 | 1 |
| LordHill-ocd | 25 | 1 | Yellow | no | Union | 16 | 0 | 1 |
| LordHill-ocd | 25 | 2 | White for yellow + White for green + White for road under construct. + White background | no | Union | 19 | 0 | 1 |
| LordHill-ocd | 25 | 4 | Green 30% | no | Difference | 23 | 2 | 6 |
| LordHill-ocd | 25 | 4 | Green 30% | no | Union | 23 | 2 | 5 |
| LordHill-ocd | 25 | 5 | Green 60% | no | Difference | 30 | 3 | 12 |
| LordHill-ocd | 25 | 5 | Green 60% | no | Union | 30 | 3 | 13 |
| LordHill-ocd | 25 | 6 | Green | no | Difference | 31 | 3 | 9 |
| LordHill-ocd | 25 | 6 | Green | no | Union | 31 | 4 | 8 |
| LordHill-ocd | 25 | 7 | Dark green | no | Difference | 25 | 1 | 0 |
| LordHill-ocd | 25 | 7 | Dark green | no | Union | 25 | 3 | 2 |
| McHugh_Creek-omap | 5 | 13 | Brown 100% | yes | union of the run by pairs | 2 676 | 0 | 1 |
| McHugh_Creek-omap | 5 | 16 | Black for open land + Black below lower brown 50% + Black below upper brown 50% + Black below purple for track symbols + Black 100% | yes | union of the run by pairs | 2 321 | 1 | 2 |
| McHugh_Creek-omap | 5 | 1 | Yellow 50% | no | Difference | 2 646 | 1 | 31 |
| McHugh_Creek-omap | 5 | 1 | Yellow 50% | no | Union | 2 646 | 4 | 24 |
| McHugh_Creek-omap | 5 | 2 | Green 100% for undergrowth + Green 100% for area features | no | Difference | 2 155 | 1 | 4 |
| McHugh_Creek-omap | 5 | 2 | Green 100% for undergrowth + Green 100% for area features | no | Union | 2 155 | 0 | 2 |
| McHugh_Creek-omap | 5 | 3 | Green 30% + Green 30% for narrow ride | no | Difference | 1 524 | 0 | 11 |
| McHugh_Creek-omap | 5 | 3 | Green 30% + Green 30% for narrow ride | no | Union | 1 524 | 0 | 9 |
| McHugh_Creek-omap | 5 | 4 | Green 60% + Green 60% for narrow ride | no | Difference | 1 303 | 1 | 7 |
| McHugh_Creek-omap | 5 | 4 | Green 60% + Green 60% for narrow ride | no | Union | 1 303 | 0 | 7 |
| McHugh_Creek-omap | 5 | 8 | Black for open land + Black below lower brown 50% + Black below upper brown 50% + Black below purple for track symbols + Black 100% | yes | Difference | 136 | 0 | 1 |
| McHugh_Creek-omap | 5 | 13 | Brown 100% | yes | Difference | 2 676 | 0 | 1 |
| McHugh_Creek-omap | 5 | 13 | Brown 100% | yes | Union | 2 676 | 0 | 1 |
| McHugh_Creek-omap | 10 | 13 | Brown 100% | yes | union of the run by pairs | 703 | 0 | 1 |
| McHugh_Creek-omap | 10 | 16 | Black for open land + Black below lower brown 50% + Black below upper brown 50% + Black below purple for track symbols + Black 100% | yes | union of the run by pairs | 747 | 0 | 1 |
| McHugh_Creek-omap | 10 | 1 | Yellow 50% | no | Difference | 708 | 1 | 24 |
| McHugh_Creek-omap | 10 | 1 | Yellow 50% | no | Union | 708 | 6 | 21 |
| McHugh_Creek-omap | 10 | 2 | Green 100% for undergrowth + Green 100% for area features | no | Difference | 637 | 2 | 3 |
| McHugh_Creek-omap | 10 | 2 | Green 100% for undergrowth + Green 100% for area features | no | Union | 637 | 2 | 3 |
| McHugh_Creek-omap | 10 | 3 | Green 30% + Green 30% for narrow ride | no | Difference | 524 | 0 | 5 |
| McHugh_Creek-omap | 10 | 3 | Green 30% + Green 30% for narrow ride | no | Union | 524 | 0 | 6 |
| McHugh_Creek-omap | 10 | 4 | Green 60% + Green 60% for narrow ride | no | Difference | 478 | 1 | 4 |
| McHugh_Creek-omap | 10 | 4 | Green 60% + Green 60% for narrow ride | no | Union | 478 | 2 | 3 |
| McHugh_Creek-omap | 10 | 8 | Black for open land + Black below lower brown 50% + Black below upper brown 50% + Black below purple for track symbols + Black 100% | yes | Difference | 59 | 0 | 1 |
| McHugh_Creek-omap | 10 | 8 | Black for open land + Black below lower brown 50% + Black below upper brown 50% + Black below purple for track symbols + Black 100% | yes | Union | 59 | 0 | 1 |
| McHugh_Creek-omap | 25 | 13 | Brown 100% | yes | union of the run by pairs | 128 | 0 | 2 |
| McHugh_Creek-omap | 25 | 16 | Black for open land + Black below lower brown 50% + Black below upper brown 50% + Black below purple for track symbols + Black 100% | yes | union of the run by pairs | 143 | 1 | 3 |
| McHugh_Creek-omap | 25 | 1 | Yellow 50% | no | Difference | 130 | 5 | 19 |
| McHugh_Creek-omap | 25 | 1 | Yellow 50% | no | Union | 130 | 8 | 11 |
| McHugh_Creek-omap | 25 | 2 | Green 100% for undergrowth + Green 100% for area features | no | Difference | 123 | 2 | 0 |
| McHugh_Creek-omap | 25 | 2 | Green 100% for undergrowth + Green 100% for area features | no | Union | 123 | 3 | 1 |
| McHugh_Creek-omap | 25 | 3 | Green 30% + Green 30% for narrow ride | no | Difference | 114 | 3 | 4 |
| McHugh_Creek-omap | 25 | 3 | Green 30% + Green 30% for narrow ride | no | Union | 114 | 1 | 9 |
| McHugh_Creek-omap | 25 | 4 | Green 60% + Green 60% for narrow ride | no | Difference | 111 | 0 | 7 |
| McHugh_Creek-omap | 25 | 4 | Green 60% + Green 60% for narrow ride | no | Union | 111 | 2 | 6 |
| McHugh_Creek-omap | 25 | 8 | Black for open land + Black below lower brown 50% + Black below upper brown 50% + Black below purple for track symbols + Black 100% | yes | Difference | 22 | 0 | 1 |

With `tiles-uncut`, a run whose `Difference` gave no result stays in its tile as it is, not cut.

**Question 6: tiles. A run that a difference could not cut stays as it is, not cut.**

From `<map>/q6-tiles-uncut.tsv`.

| Map | Tile, mm | Tiles (with marks) | Stopped by the limit | Cut, s | Union, s | Knockout, s | All, s | Cuts (no result) | Unions with no result | Knockout operations (no result) | Longest knockout operation, s | Pieces | Segments | Paper samples in the window at 40 | On the borders at 40 | On the borders at 400 (of samples) |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| LordHill-ocd | 10 | 285 (234) | no | 17.4 | 7.4 | 29.1 | 54.0 | 6 526 (0) | 0 | 2 768 (25) | 0.143 | 1 302 | 410 128 | 275 086 | 12 968 | 117 381 (of 13 280 000) |
| LordHill-ocd | 25 | 48 (47) | no | 5.7 | 12.2 | 95.7 | 113.6 | 2 581 (0) | 0 | 782 (32) | 1.200 | 364 | 367 690 | 239 334 | 7 485 | 72 694 (of 5 280 000) |
| McHugh_Creek-omap | 10 | 1 148 (773) | no | 43.4 | 10.4 | 33.0 | 86.8 | 32 680 (0) | 0 | 8 554 (14) | 0.062 | 4 220 | 953 150 | 31 052 | 2 580 | 19 529 (of 13 280 000) |
| McHugh_Creek-omap | 25 | 204 (145) | no | 14.0 | 12.7 | 54.3 | 81.0 | 13 579 (0) | 1 | 1 886 (24) | 0.288 | 934 | 902 366 | 19 529 | 1 670 | 7 407 (of 5 280 000) |

- **Tiles make the time practical.** `LordHill.ocd` in tiles of 10 mm takes 24 s for the cut to the tiles, the unions
  and the knockout colours, where the whole map did not end in 1 800 s. `McHugh Creek.omap` takes 71 s. The longest
  operation is 0.04 s. The time grows with the tile: at 25 mm the longest operation is 0.2 s to 2.9 s, and at 50 mm
  it is 1.2 s to 8.3 s and `LordHill.ocd` does not end in 180 s. The two large maps do not end in 180 s at any size.
  The cut to the tiles is a large part of the time at 10 mm and below.
- **But Skia does not give the right pieces.** In the tiles of 10 mm of `LordHill.ocd`, 25 of the 2 768 operations
  of the knockout give no result, and 162 more give a wrong result with no failure: 6.8 % of the operations in all.
  A smaller tile lowers the share and does not cure it: 13.7 % at 25 mm (32 and 75 of 782), 6.8 % at 10 mm, and
  3.0 % at 5 mm (18 and 199 of 7 142). The count of bad results grows as the tile gets smaller.
  `McHugh Creek.omap` has fewer: 4.3 % at 25 mm (24 and 58 of 1 886), 1.0 % at 10 mm (14 and 71 of 8 554) and 0.4 %
  at 5 mm (7 and 98 of 27 196).
- **Most bad results are in the knockout, and few are in the union of a run.** Of the unions of a run in a tile of
  10 mm, 1 of 1 384 is wrong in `LordHill.ocd`, and 2 of 4 277 in `McHugh Creek.omap`. No cut to a tile gave "no
  result"; the probe did not check the cuts for wrong results. The wrong and the missing results are in the
  `Difference` and the `Union` of the knockout, and almost all are in the area runs at the bottom of the order
  (yellow and green). The other path of these operations is the union of each run above.
- **A large map in tiles of 10 mm:** `teanaway.omap` takes 315 s for 2 812 tiles (the cut to the tiles 133 s, the
  unions 52 s, the knockout 130 s). 28 of 20 442 operations of the knockout give no result. This stage ran without
  the check of each result.
- **So the pieces have holes and overlaps.** With tiles of 10 mm, 403 398 samples of the window of `LordHill.ocd`
  that a mark covers have no piece (3.3 % of the samples with ink), and 316 189 samples are in two pieces. When the
  runs that could not be cut stay whole (`tiles-uncut`), 275 086 samples still have no piece: these come from the
  wrong results.
- **Do the pieces of two tiles meet without a gap? The probe cannot say it apart from the holes.** The strips along
  the borders hold no more paper than the rest of the window: 24 959 of about 1 300 000 samples (1.9 %) at 40, and
  230 170 of 13 280 000 (1.7 %) at 400, where the whole window has 2.5 %. A row of paper of one sample along each
  border would show at 400 as a higher share, and it does not. Question 4 says the same for two rectangles: a
  straight shared edge is exact.
- **The overprinted runs as they are** give about three times as many pieces and four times as many operations
  (`LordHill.ocd`, 10 mm: 3 806 pieces and 10 982 operations, where the knockout colours give 1 293 and 2 768). 4 of
  these operations give no result. They were not checked one by one. Their pieces have holes and overlaps too.
  In `teanaway.omap`, where each run is overprinted, this cut takes 447 s and 126 651 operations for 39 526 pieces,
  and 44 operations give no result.
- **The same pieces come out the second time:** the digest of all pieces is equal in the second run, with the same
  failures.

## Question 7: curves

The largest distance is from 1000 or more points of each contour that comes out, to the outlines that went in, which
are the exact outlines. For the stroke it is the largest difference between half the width and the distance to the
centre line, on the outline after `Simplify`.

**Question 7: curves.**

From `q7-curves.tsv`.

| Shapes | Operation | In: lines / quadratics / conics / cubics | Result | Contours out | Out: lines / quadratics / conics / cubics | Largest distance, mm |
| --- | --- | --- | --- | --- | --- | --- |
| two circles of four cubics each, which cross | Union | 0/0/0/8 | yes | 1 | 0/0/0/8 | 0.0000014 |
| two circles of four cubics each, which cross | Intersect | 0/0/0/8 | yes | 1 | 0/0/0/4 | 0.0000011 |
| two circles of four cubics each, which cross | Difference | 0/0/0/8 | yes | 1 | 0/0/0/6 | 0.0000014 |
| two shapes that share a cubic curve | Union | 4/0/0/2 | yes | 1 | 5/0/0/0 | 0 |
| two shapes that share a cubic curve | Difference | 4/0/0/2 | yes | 1 | 3/0/0/2 | 0.0000001 |
| one cubic that crosses itself, closed by a line | Simplify | 0/0/0/1 | yes | 2 | 1/0/0/4 | 0.0000081 |
| a circle of 0.5 mm of four cubics, cut by a rectangle | Intersect | 3/0/0/4 | yes | 1 | 1/0/0/2 | 0 |
| a stroked cubic of 0.35 mm with round caps | GetFillPath at resolution scale 1, then Simplify | 0/0/0/1 | yes | 1 | 0/4/4/0 (after Simplify: 0/4/5/0) | 0.158177 |
| a stroked cubic of 0.35 mm with round caps | GetFillPath at resolution scale 40, then Simplify | 0/0/0/1 | yes | 1 | 0/14/4/0 (after Simplify: 0/14/5/0) | 0.0043535 |
| a stroked cubic of 0.35 mm with round caps | GetFillPath at resolution scale 400, then Simplify | 0/0/0/1 | yes | 1 | 0/24/4/0 (after Simplify: 0/24/5/0) | 0.0006121 |

- **The operations keep curves.** Cubics come out as cubics, at a distance of at most 0.000008 mm from the exact
  outline. The unions of question 2 show the same for the other kinds: the 3 147 conics of run 2 of `LordHill.ocd`
  come out as 3 147 conics, and the 2 160 quadratics of run 8 as 2 152 quadratics. Nothing is turned into a polygon.
- **An operation divides a curve where it cuts it,** so the count of segments can grow: the cubic that crosses itself
  comes out as 4 cubics.
- **The outline of a stroke is not exact, and it depends on the resolution scale of `SKPaint.GetFillPath`.** The
  outline is quadratics and conics, and no cubic. At the default scale of 1 the outline of a line of 0.35 mm is up to
  0.158 mm from its place. At a scale of 40 for each millimetre it is 0.0044 mm, and at 400 it is 0.0006 mm. The
  probe made the marks of the maps at 40.

## Question 8: the pieces against the drawing

Each sample of the window gets the index of its run two times: from the pieces, and from the marks drawn in their
order with no blending. The images are in `<results>/<map>/`: `q8-<source>-pieces.png`, `q8-<source>-drawing.png`,
`q8-<source>-difference.png` (differing samples in red, other ink in grey) and
`q8-<source>-pieces-false-colours.png`. The run of this file has them for the tiles of 25 mm of `LordHill.ocd` and
of `McHugh Creek.omap`.

**Question 8: the pieces against the drawing, over the window.**

From `<map>/q8-pieces-<source>.tsv`.

| Map | Pieces of | Window: left, top, width, height, mm | Samples | Samples with ink | Differing samples | Thick differing samples | Largest group |
| --- | --- | --- | --- | --- | --- | --- | --- |
| LordHill-ocd | tiles-10mm | -27.9, -129.7, 100.0, 100.0 | 16 000 000 | 12 054 503 | 433 886 | 364 130 | 29 715 |
| LordHill-ocd | tiles-25mm | -27.9, -129.7, 100.0, 100.0 | 16 000 000 | 12 054 503 | 2 257 749 | 1 944 620 | 87 513 |
| LordHill-ocd | tiles-uncut-10mm | -27.9, -129.7, 100.0, 100.0 | 16 000 000 | 12 054 503 | 294 101 | 244 835 | 29 715 |
| LordHill-ocd | tiles-uncut-25mm | -27.9, -129.7, 100.0, 100.0 | 16 000 000 | 12 054 503 | 284 148 | 233 445 | 64 655 |
| McHugh_Creek-omap | tiles-10mm | -16.7, 13.0, 100.0, 100.0 | 16 000 000 | 15 058 819 | 46 674 | 25 447 | 5 425 |
| McHugh_Creek-omap | tiles-25mm | -16.7, 13.0, 100.0, 100.0 | 16 000 000 | 15 058 819 | 162 510 | 118 639 | 4 437 |
| McHugh_Creek-omap | tiles-50mm | -16.7, 13.0, 100.0, 100.0 | 16 000 000 | 15 058 819 | 2 802 445 | 2 423 297 | 61 525 |
| McHugh_Creek-omap | tiles-uncut-10mm | -16.7, 13.0, 100.0, 100.0 | 16 000 000 | 15 058 819 | 46 674 | 25 447 | 5 425 |
| McHugh_Creek-omap | tiles-uncut-25mm | -16.7, 13.0, 100.0, 100.0 | 16 000 000 | 15 058 819 | 34 133 | 14 103 | 1 639 |

- **The pieces of a whole map exist for `LordHill.ocd` only, from the check run with the machine alone.**
  `<results>/smoke/LordHill-ocd/q8-pieces-whole-map.tsv` and the four images `q8-whole-map-*.png` beside it hold
  them: 7 719 777 of the 16 000 000 samples differ from the drawing, 6 772 305 of them thick, with a largest group of
  191 099 samples. The green runs and the lowest yellow run have no piece, because their `Difference` gave no result.
- The pieces of the table are the pieces of the tiles, of each run as a knockout colour.
- **The differences are areas, not outlines.** Most differing samples are thick, and the largest group has tens of
  thousands of samples: they are the pieces that are missing or wrong (question 6). In the images of
  `LordHill.ocd` with tiles of 25 mm, whole parts of a tile have no piece of a green or a yellow run.
- With `tiles-uncut` a run that could not be cut stays whole, and the upper pieces cover it when they are drawn. The
  thick samples that then stay (244 835 for `LordHill.ocd` at 10 mm) are the wrong results that Skia gave with no
  failure.

## Question 9: the time of the loop

`loop-time.sh` ran alone, before the other measurements. The first build is with no `bin` and `obj` folders, the
build servers stopped, and the NuGet packages in the cache. The test fails, as the brief says: the branch does not
find `skia_render\fences.txt` on Linux.

**Question 9: the time of the loop.**

From `q9-loop-time.tsv`.

| Step | Command | Wall-clock seconds | Exit code |
| --- | --- | --- | --- |
| first build | dotnet build | 34.5 | 0 |
| build with nothing changed | dotnet build | 6.3 | 0 |
| one test | dotnet test --no-build --filter FullyQualifiedName=Map_Skia.Tests.RenderingTests.Fences | 3.7 | 1 |

## What the numbers suggest

**This section is the opinion of the agent that measured. It is not a result.**

1. **The risk that the plan names is real: the path operations fail at the size of a map.** They give no result for
   some operations, and for more operations they give a wrong result and report nothing. This holds for the whole
   map and for tiles down to 5 mm. So Skia's path operations cannot be trusted on this geometry without a check. A
   stage built on them needs a check of each result that does not use the path operations, and another way to make
   the piece when the check fails. The numbers do not say what that other way is. The raster reference of phase 2
   can be the check. In these measurements a right result differs from its operands in single samples along its
   outline, and a wrong result differs in areas, so "no thick sample differs" is a test that can tell them apart.
2. **The stage must divide the page before it cuts.** An operation costs time by the size of its paths, even where
   they share nothing, so the cut of a whole map is out of reach. Tiles of 5 mm to 10 mm bring the work to seconds
   for a small map and to minutes for a large map.
3. **The order of the cut is the first thing to change and to measure again.** Almost all bad results are in
   operations with the union of all upper runs as one path. The probe did not try the other order: each run cut by
   one upper run at a time, or by the upper runs of its near surroundings only. The tiles also put a straight edge
   of each run on the same line. Question 4 found two such edges exact, but it did not try many edges at one place.
   Tiles with a margin, cut back at the end, are a second thing to measure. I would measure both before the stage
   is designed around tiles.
4. **Unite by pairs, never one after the other, and not with `OpBuilder`.** Check the union of a large run too.
5. **FL-8 needs a rule for the resolution scale of `SKPaint.GetFillPath`.** At the default the outline of a thin line
   is wrong by almost half its width. At 40 for each millimetre the error is 0.004 mm.
6. **The clip of pattern elements is the largest cost of drawing a large map as marks:** about 300 000 intersections,
   and three of four elements end empty. Case 8 of the ladder ("patterns") will meet this cost.
7. **FL-7 holds:** the operations keep curves. The count of segments grows where shapes cross.
8. **GATE-5 can hold on one machine.** Each repeat gave equal digests. Another machine is an open question, and
   `digests.txt` is the means to answer it.
9. **`teanaway.omap` is a hard case for the model, not only for Skia:** 34 of its 35 colours are overprinted, so each
   of its runs is an ink and no colour knocks out. The pieces are then the overlaps of inks: 39 526 pieces in tiles
   of 10 mm, where the knockout colours give 10 164.
10. **A tolerance for GATE-2** has to allow single samples along a cut curved edge (question 4: up to 6 at 40 on
    11 mm of edge) and can forbid thick samples.
