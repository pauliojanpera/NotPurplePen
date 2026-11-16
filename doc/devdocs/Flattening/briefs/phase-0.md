# Phase 0: measure Skia's path operations on the geometry of real maps

**The step.** Measure what Skia's boolean path operations do with the geometry of real orienteering maps and with
edges that two areas share, and write the numbers down. You write a probe and a report. You write no code of the
flattening stage, and you do not design it.

**Why.** The flattening (task 12) cuts the colours of a map into opaque pieces with these operations. Real maps are
the last case of its ladder, so nothing else measures the operations at that size before the stage is built on them.
The session uses your numbers to decide the structure of the stage.

**The premise.** SkiaSharp 3.119.4 gives the path operations on this machine (`SKPath.Op`, `SKPath.OpBuilder`,
`SKPath.Simplify`, `SKPaint.GetFillPath`), and the map model can load and draw the test maps on Linux. If one of the
two is false, report what fails and stop.

## Paths

| Name | Path |
| --- | --- |
| `<clone>` | `/home/pauli/Documents/Projects/NotPurplePen` (branch `docs/process-coordination`) |
| `<flat>` | `/home/pauli/Documents/Projects/npp-worktrees/flat` (branch `feature/flattening`, at e6116a7b) |
| `<results>` | `/home/pauli/Documents/Projects/npp-worktrees/flat-results/phase-0` |

- `<flat>` is the checkout that the probe builds against. **Do not change a tracked file in it, and do not commit in
  it.** Build outputs in its `bin` and `obj` folders are permitted.
- This step writes two things in `<clone>`: the probe in `<clone>/doc/devdocs/Flattening/probe/`, and the report
  `<clone>/doc/devdocs/Flattening/Measurements.md`. Give the probe a `.gitignore` for `bin/` and `obj/`. **Do not use
  git in `<clone>`:** the session commits.
- Raw data and images go to `<results>`.

## Read first

1. `<clone>/doc/devdocs/Flattening/Model.md`: the rules of the calculation. One page.
2. `<clone>/doc/devdocs/Flattening/README.md`, the sections "What flattening is", "The requirements" and "The gates".
3. `<clone>/doc/devdocs/SvgPdfVectors/README.md` and its `run-all.sh`: an earlier probe of this fork, as an example
   of the form.

## Where things are

- **How a test loads and draws a map with Skia:** `<flat>/src/MapModel/Map_Skia.Tests/RenderingUtil.cs`. Lines 164
  and 165 load a map (`new Map(new Skia_TextMetrics(), new Skia_FileLoader(directory))`, then
  `InputOutput.ReadFile`). `RenderBitmap`, from line 60, draws it with `map.Draw(grTarget, mapArea, renderOpts, null)`.
- **How a map draws its colours:** `<flat>/src/MapModel/MapModel/Map.cs`, `Draw` from line 2183. It draws one colour
  of the table at a time, the lowest first. It opens `PushBlending` around the consecutive overprinted colours of one
  ink (lines 2218 to 2247, and 2325 to 2329), when `renderOpts.blendOverprintedColors` is set.
- **The drawing calls:** `<flat>/src/MapModel/Graphics2D/IGraphicsTarget.cs`. A path is a list of
  `GraphicsPathPart`: `Start`, `Lines`, `Beziers` (cubic) and `Close`.
- **The Skia target,** as an example of each call in Skia: `<flat>/src/MapModel/Map_SkiaStd/SkiaGraphicsTarget.cs`.
- **The maps.** Lengths of a map are millimetres on paper.

  | Map | Why |
  | --- | --- |
  | `<flat>/src/MapModel/TestFiles/skia_render/LordHill.ocd` | OCAD, 2 MB. Its "Purple" is overprinted |
  | `<flat>/src/MapModel/TestFiles/openmapper/TeanawayValley.ocd` | OCAD, 7 MB: a large map |
  | `<flat>/src/TestFiles/courseprinting/McHugh Creek.omap` | Mapper, 7 MB |
  | `<flat>/src/MapModel/TestFiles/openmapper/teanaway.omap` | Mapper, 13 MB: the largest. Measure it last |

- **The .NET SDK** is in `~/.dotnet`. The machine has 4 cores and 15 GB of memory.

## What to deliver

**The probe:** a C# console project that makes each table of the report again with one command,
`run-all.sh <flat> <results>`. It draws a map through a target of its own, which keeps the marks of each run of one
colour as Skia paths in the coordinates of the page: a filled area as it is, and a stroked line as its outline
(`SKPaint.GetFillPath`), with the transform and the clip of the moment applied. A run is the marks of one colour
that follow each other (Model.md).

Draw each map as a vector export draws it: area patterns and hatchings as shapes and lines, not as pattern brushes or
bitmaps, and with `blendOverprintedColors` set. Find the render options that do this in `RenderOptions`, and say in
the report which you used.

**The report,** `Measurements.md`, in Simplified Technical English: a table for each question below, the command
that makes it again, and at the end a short section "What the numbers suggest", which is your opinion and is marked
as such. Give each time as wall-clock seconds, and say when a number is of one run only.

### The questions, the most important first

1. **What a map holds.** For each map: the runs, and for each run its colour, whether it is inside a blending, the
   count of its marks, and the count of their segments by kind (line, cubic). Also the counts of what the target met
   and did not keep: text, bitmaps, pattern brushes, clips, and switches of antialiasing.
2. **The union of a run.** The marks of each run united into one path, in three ways: `SKPath.OpBuilder`; one
   `Op(Union)` after the other; and by pairs, then pairs of the results. For each way: the time, whether it gave a
   result, and the contours and segments of the result. Check each result: draw the union and draw the marks, without
   antialiasing, at 40 samples for each millimetre over a window of the map, and count the samples that differ.
3. **The knockout at the size of a map.** Take each run as a knockout colour. From the top run down, cut each run by
   the union of the runs above it. Give the time of each operation, the failures, the total time for the map, and the
   count of pieces. Then take the overprinted runs as they are: intersect each with the pieces below it, and give the
   same numbers.
4. **Edges that two areas share.** Maps hold many. Make shapes for each of these: two rectangles that share one whole
   edge; two that share a part of an edge; two shapes that share a cubic curve, drawn in the same direction and in
   the opposite one; a shape and a copy of itself; and edges that are 0.0001 mm apart. For union, difference and
   intersection give: the contours of the result, the count of contours with an area below 0.000001 mm², the error of
   the area, and the samples of paper that show between the pieces when they are drawn without antialiasing at 40 and
   at 400 samples for each millimetre.
5. **The same result two times.** A digest of each result of questions 2 and 3: the kinds and the coordinates of its
   segments, bit for bit. Are the digests equal when the probe runs the operations two times in one process, and in
   two processes? Write the digests to `<results>/digests.txt`, so that a run on another machine can be compared
   with them.
6. **Tiles.** Questions 2 and 3 again, with the marks cut to square tiles first, of 10, 25 and 50 mm: the total
   time and the failures. Do the pieces of two tiles that touch meet without a gap? Count the samples of paper along
   the borders of the tiles, as in question 4.
7. **Curves.** Do the results keep curves? For shapes made of cubic curves, give the kinds of the segments that go in
   and that come out (line, quadratic, conic, cubic), and the largest distance between the outline that comes out and
   the outline that is exact, at 1000 points of each contour.
8. **The pieces against the drawing.** The pieces of question 3, drawn without antialiasing at 40 samples for each
   millimetre over a window of the map, against the runs drawn in their order with no blending: the samples that
   differ, and the largest group of them that hangs together. Save the two images and their difference to
   `<results>`, and one image of the pieces in false colours.
9. **The time of the loop on this machine.** In `<flat>`: `dotnet build` of `src/MapModel/Map_Skia.Tests`, the first
   time and again with nothing changed, and `dotnet test --no-build` with a filter for one test. The test can fail
   here, because this branch does not find test files that are named with backslashes on Linux: give the times, not
   the verdict.

## Stop when

- The premise is false.
- A question needs a change of a tracked file in `<flat>`. Say which, and go on with the next question.
- You have worked for about two hours. Say which questions have no numbers.

## The report to the session

Your last message, as your instructions give its order. For this step, "each gate" is each question: the command,
and the three or four numbers that matter most. Name the file of each table. Give the full paths of the files that
you wrote in `<clone>`.
