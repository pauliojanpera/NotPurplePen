using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using PurplePen.Graphics2D;
using PurplePen.MapModel;
using SkiaSharp;

namespace SkiaPathOpsProbe
{
    /// <summary>The choices of the measurements of a map.</summary>
    public sealed class MapProbeOptions
    {
        /// <summary>The time limit, in seconds, of the union of one run with the builder or one after the other.</summary>
        public double SlowWayLimitSeconds = 10;

        /// <summary>The time limit, in seconds, of the union of one run by pairs.</summary>
        public double PairsLimitSeconds = 120;

        /// <summary>The time limits, in seconds, of the work on the tiles of each size. The last limit is for each later size too.</summary>
        public List<double> TileSizeLimitSeconds = new List<double> { 180 };

        /// <summary>The sides of the square tiles of question 6, in millimetres.</summary>
        public List<float> TileSides = new List<float> { 10, 25, 50 };

        /// <summary>The side of the tiles whose pieces the probe saves as images.</summary>
        public float TileSideOfImages = 25;

        /// <summary>True when the tiles keep a run that a difference could not cut, as it is. If false, such a run has no piece in its tile.</summary>
        public bool IsUncutKeptOnFailure;

        /// <summary>The largest count of segments of a run that the stage of the digests works on.</summary>
        public long LargestSegmentsOfRepeatedRun = 10000;
    }

    /// <summary>
    /// The measurements of one map, in stages that run in a process each: what the map holds (question 1), the union
    /// of each run and the knockout over the whole map (2, 3 and 8), the union with the builder (2), the tiles (6
    /// and 8), and the digests (5). Each table is written row by row, so a stage that is stopped from outside leaves
    /// the rows that it measured.
    /// </summary>
    public sealed class MapProbe
    {
        /// <summary>The samples for each millimetre at which the probe compares drawings.</summary>
        public const int SamplesPerMillimetre = 40;

        /// <summary>The side of the window of the map in which the probe compares drawings, in millimetres.</summary>
        public const float WindowSide = 100;

        private readonly string mapFile;
        private readonly string mapName;
        private readonly string folder;
        private readonly MapProbeOptions options;
        private List<Run> runs;
        private List<SKPath> allMarks;
        private List<int> runOfEachMark;
        private SKRect bounds;
        private SKRect window;

        /// <summary>Makes the probe of one map. The tables of the map go to a folder of its own in the results folder.</summary>
        public MapProbe(string mapFile, string resultsFolder, MapProbeOptions options)
        {
            this.mapFile = mapFile;
            this.options = options;
            mapName = Path.GetFileNameWithoutExtension(mapFile).Replace(' ', '_') + "-" + Path.GetExtension(mapFile).TrimStart('.');
            folder = Path.Combine(resultsFolder, mapName);
            Directory.CreateDirectory(folder);
        }

        /// <summary>Runs one stage: "inventory", "whole-map", "builder", "tiles" or "digests". It gives false for another name.</summary>
        public bool RunStage(string stage)
        {
            switch (stage) {
                case "inventory":
                    DrawMap(true);
                    return true;

                case "whole-map":
                    DrawMap(false);
                    SKPath[] unions = MeasureUnions(new UnionWay[] { UnionWay.OneAfterTheOther, UnionWay.ByPairs }, "q2-union.tsv");
                    Piece[] pieces = MeasureKnockoutOfEachRun(unions);
                    if (pieces != null)
                        ComparePiecesWithDrawing(pieces.Select(piece => piece.Path).ToList(), pieces.Select(piece => piece.BaseRun).ToList(), "whole-map", true);
                    MeasureKnockoutWithOverprintedRuns(unions);
                    return true;

                case "builder":
                    DrawMap(false);
                    MeasureUnions(new UnionWay[] { UnionWay.Builder }, "q2-union-builder.tsv");
                    return true;

                case "tiles":
                    DrawMap(false);
                    MeasureTiles();
                    return true;

                case "digests":
                    DrawMap(false);
                    WriteDigests();
                    return true;

                default:
                    return false;
            }
        }

        // ---- Question 1: what the map holds.

        /// <summary>
        /// Loads the map and draws it through the target that keeps the marks. It chooses the window in which the
        /// probe compares drawings: the square of WindowSide that holds the most marks, on a grid of 10 mm. When
        /// isTableWritten is set, it writes the tables of question 1.
        /// </summary>
        private void DrawMap(bool isTableWritten)
        {
            string cacheFile = Path.Combine(folder, "marks.bin");
            if (!isTableWritten && File.Exists(cacheFile)) {
                runs = MarkCache.Load(cacheFile, out bounds);
                allMarks = runs.SelectMany(run => run.Marks).ToList();
                runOfEachMark = runs.SelectMany(run => run.Marks.Select(mark => run.Index)).ToList();
                ChooseWindow();
                Console.WriteLine(mapName + ": " + runs.Count + " runs and " + allMarks.Count + " marks read from the file of the marks");
                return;
            }

            Stopwatch stopwatch = Stopwatch.StartNew();
            Map map = new Map(new Skia_TextMetrics(), new Skia_FileLoader(Path.GetDirectoryName(mapFile)));
            InputOutput.ReadFile(mapFile, map);
            double loadSeconds = stopwatch.Elapsed.TotalSeconds;

            // The options of a vector export: CoursePdf sets minResolution for 2400 dots per inch, MapDisplay sets
            // usePatternBitmaps to false for printing, and templates are drawn with the map.
            RenderOptions renderOptions = new RenderOptions();
            renderOptions.usePatternBitmaps = false;
            renderOptions.minResolution = 25.4F / 2400F;
            renderOptions.renderTemplates = RenderTemplateOption.MapAndTemplates;
            renderOptions.blendOverprintedColors = true;
            renderOptions.overprintBlendMode = BlendMode.Darken;

            stopwatch.Restart();
            MarkCollectingGraphicsTarget target = new MarkCollectingGraphicsTarget(SamplesPerMillimetre);
            System.Drawing.RectangleF mapBounds;
            List<SymColor> colours;
            using (map.Read()) {
                mapBounds = map.Bounds;
                target.PushAntiAliasing(false);
                map.Draw(target, mapBounds, renderOptions, null);
                target.PopAntiAliasing();
                colours = map.AllColors.ToList();
            }
            double drawSeconds = stopwatch.Elapsed.TotalSeconds;
            runs = target.Runs;
            allMarks = runs.SelectMany(run => run.Marks).ToList();
            runOfEachMark = runs.SelectMany(run => run.Marks.Select(mark => run.Index)).ToList();
            bounds = new SKRect(mapBounds.Left, mapBounds.Top, mapBounds.Right, mapBounds.Bottom);
            foreach (Run run in runs) {
                IEnumerable<string> names = colours.Where(colour => !colour.IsSpecialLayer && colour.CmykColor.Equals(run.Colour) && colour.OverPrint == run.IsInsideBlending).Select(colour => colour.Name).Distinct();
                run.ColourName = string.Join(" + ", names);
                if (run.ColourName.Length == 0)
                    run.ColourName = "(no colour of the table)";
            }
            ChooseWindow();
            Console.WriteLine(mapName + ": " + runs.Count + " runs, " + allMarks.Count + " marks, loaded in " + loadSeconds.ToString("F1") + " s, drawn in " + drawSeconds.ToString("F1") + " s");
            if (!isTableWritten)
                return;

            MarkCache.Save(cacheFile, bounds, runs);
            TsvTable runTable = new TsvTable(Path.Combine(folder, "q1-runs.tsv"), "map", "run", "colour", "cyan", "magenta", "yellow", "black", "alpha", "inside_blending", "marks", "contours", "segments", "lines", "quadratics", "conics", "cubics");
            SegmentCounts all = new SegmentCounts();
            foreach (Run run in runs) {
                SegmentCounts counts = PathMeasures.CountSegments(run.Marks);
                all.Add(counts);
                runTable.AddRow(mapName, run.Index, run.ColourName, run.Colour.Cyan, run.Colour.Magenta, run.Colour.Yellow, run.Colour.Black, run.Colour.Alpha, run.IsInsideBlending, run.Marks.Count,
                                counts.Contours, counts.Segments, counts.Lines, counts.Quadratics, counts.Conics, counts.Cubics);
            }

            NotKeptCounts notKept = target.NotKept;
            TsvTable mapTable = new TsvTable(Path.Combine(folder, "q1-map.tsv"), "map", "width_mm", "height_mm", "window_left_mm", "window_top_mm", "colours_of_table", "overprinted_colours_of_table", "load_seconds", "draw_seconds",
                "runs", "runs_inside_blending", "marks", "contours", "segments", "lines", "quadratics", "conics", "cubics", "text_calls", "bitmap_calls", "pattern_brushes", "fills_with_pattern_brush", "clips",
                "clips_inside_a_clip", "clip_failures", "marks_cut_by_clip", "marks_outside_clip", "mark_clip_failures", "clip_seconds", "antialiasing_pushes", "antialiasing_switches", "blending_pushes", "hairline_strokes", "empty_marks");
            mapTable.AddRow(mapName, bounds.Width, bounds.Height, window.Left, window.Top, colours.Count, colours.Count(colour => !colour.IsSpecialLayer && colour.OverPrint), loadSeconds, drawSeconds,
                runs.Count, runs.Count(run => run.IsInsideBlending), allMarks.Count, all.Contours, all.Segments, all.Lines, all.Quadratics, all.Conics, all.Cubics, notKept.TextCalls, notKept.BitmapCalls, notKept.PatternBrushes,
                notKept.FillsWithPatternBrush, notKept.Clips, notKept.ClipIntersections, notKept.ClipIntersectionFailures, notKept.MarksClipped, notKept.MarksOutsideClip, notKept.MarkClipFailures, notKept.ClipSeconds,
                notKept.AntiAliasingPushes, notKept.AntiAliasingSwitches, notKept.BlendingPushes, notKept.HairlineStrokes, notKept.EmptyMarks);
        }

        /// <summary>Chooses the window: the square of WindowSide that holds the most marks, on a grid of 10 mm.</summary>
        private void ChooseWindow()
        {
            const float cell = 10;
            int columns = Math.Max(1, (int)Math.Ceiling(bounds.Width / cell)), rows = Math.Max(1, (int)Math.Ceiling(bounds.Height / cell));
            int[,] marksInCell = new int[columns, rows];
            foreach (SKPath mark in allMarks) {
                SKRect markBounds = mark.Bounds;
                ++marksInCell[Math.Clamp((int)((markBounds.MidX - bounds.Left) / cell), 0, columns - 1), Math.Clamp((int)((markBounds.MidY - bounds.Top) / cell), 0, rows - 1)];
            }

            int cellsInWindow = (int)(WindowSide / cell), bestColumn = 0, bestRow = 0;
            long bestCount = -1;
            for (int column = 0; column <= Math.Max(0, columns - cellsInWindow); ++column) {
                for (int row = 0; row <= Math.Max(0, rows - cellsInWindow); ++row) {
                    long count = 0;
                    for (int i = column; i < Math.Min(columns, column + cellsInWindow); ++i)
                        for (int j = row; j < Math.Min(rows, row + cellsInWindow); ++j)
                            count += marksInCell[i, j];
                    if (count > bestCount) {
                        bestCount = count;
                        bestColumn = column;
                        bestRow = row;
                    }
                }
            }
            window = SKRect.Create(bounds.Left + bestColumn * cell, bounds.Top + bestRow * cell, Math.Min(WindowSide, bounds.Width), Math.Min(WindowSide, bounds.Height));
        }

        // ---- Question 2: the union of a run.

        /// <summary>
        /// Unites the marks of each run in each of the given ways and writes the table of the ways. A result that
        /// took less than 2 seconds is made a second time, and the table says if the two digests are equal. It gives
        /// the unions of the last way, one for each run.
        /// </summary>
        private SKPath[] MeasureUnions(UnionWay[] ways, string tableName)
        {
            SKPath[] unions = new SKPath[runs.Count];
            TsvTable table = new TsvTable(Path.Combine(folder, tableName), "map", "run", "colour", "marks", "segments_in", "way", "seconds", "gave_result", "failed_operations", "stopped_by_limit", "contours", "segments",
                "lines", "quadratics", "conics", "cubics", "differing_samples", "thick_differing_samples", "largest_group", "digest", "equal_when_repeated");
            foreach (UnionWay way in ways) {
                // The builder cannot be stopped. Its runs go from the smallest to the largest, and the series ends
                // at the first run that passes the limit.
                IEnumerable<Run> order = way == UnionWay.Builder ? runs.OrderBy(run => run.Marks.Sum(mark => (long)mark.VerbCount)).ThenBy(run => run.Index) : runs;
                bool isSeriesEnded = false;
                foreach (Run run in order) {
                    long segmentsIn = PathMeasures.CountSegments(run.Marks).Segments;
                    if (isSeriesEnded) {
                        table.AddRow(mapName, run.Index, run.ColourName, run.Marks.Count, segmentsIn, way, "", "not measured", "", "", "", "", "", "", "", "", "", "", "", "", "");
                        continue;
                    }
                    UnionResult result = UniteInWay(run, way);
                    isSeriesEnded = way == UnionWay.Builder && result.IsStoppedByLimit;

                    SegmentCounts counts = PathMeasures.CountSegments(result.Path);
                    string differing = "", thick = "", largestGroup = "", isEqualWhenRepeated = "";
                    string digest = result.IsStoppedByLimit ? "" : PathMeasures.ComputeDigest(result.Path);
                    if (result.Path != null && run.Marks.Any(mark => mark.Bounds.IntersectsWith(window))) {
                        SampleGrid.CompareCoverage(window, SamplesPerMillimetre, run.Marks, new SKPath[] { result.Path }, out long differingSamples, out long thickSamples, out int largest);
                        differing = differingSamples.ToString(CultureInfo.InvariantCulture);
                        thick = thickSamples.ToString(CultureInfo.InvariantCulture);
                        largestGroup = largest.ToString(CultureInfo.InvariantCulture);
                        if (thickSamples > 0)
                            SaveUnionDifference(run, result.Path, "q2-union-" + way + "-run-" + run.Index + "-difference.png");
                    }
                    if (!result.IsStoppedByLimit && result.Seconds < 2) {
                        UnionResult again = UniteInWay(run, way);
                        isEqualWhenRepeated = PathMeasures.ComputeDigest(again.Path) == digest ? "yes" : "no";
                        again.Path?.Dispose();
                    }
                    table.AddRow(mapName, run.Index, run.ColourName, run.Marks.Count, segmentsIn, way, result.Seconds, result.Path != null, result.Failures, result.IsStoppedByLimit,
                                 counts.Contours, counts.Segments, counts.Lines, counts.Quadratics, counts.Conics, counts.Cubics, differing, thick, largestGroup, digest, isEqualWhenRepeated);
                    unions[run.Index]?.Dispose();
                    unions[run.Index] = result.Path;
                }
                Console.WriteLine(mapName + ": union " + way + " done");
            }
            return unions;
        }

        /// <summary>
        /// Saves an image of the window that shows where the marks of a run and their union differ. A sample that
        /// both cover is grey. A sample of the marks that the union does not cover is red. A sample that the union
        /// covers and no mark covers is blue.
        /// </summary>
        private void SaveUnionDifference(Run run, SKPath union, string fileName)
        {
            using (SKBitmap ofMarks = SampleGrid.DrawCoverage(window, SamplesPerMillimetre, run.Marks))
            using (SKBitmap ofUnion = SampleGrid.DrawCoverage(window, SamplesPerMillimetre, new SKPath[] { union }))
            using (SKBitmap runIndexes = new SKBitmap(new SKImageInfo(ofMarks.Width, ofMarks.Height, SKColorType.Rgba8888, SKAlphaType.Premul))) {
                byte[] markSamples = ofMarks.GetPixelSpan().ToArray(), unionSamples = ofUnion.GetPixelSpan().ToArray();
                SaveImage(runIndexes, (sample, index) => markSamples[index] != 0 ? (unionSamples[index] != 0 ? new SKColor(225, 225, 225) : SKColors.Red) : (unionSamples[index] != 0 ? SKColors.Blue : SKColors.White), fileName);
            }
        }

        /// <summary>Unites the marks of a run in one way, within the limit of the way.</summary>
        private UnionResult UniteInWay(Run run, UnionWay way)
        {
            return way == UnionWay.Builder ? PathOperations.UniteWithBuilderWithinLimit(run.Marks, options.SlowWayLimitSeconds)
                : PathOperations.Unite(run.Marks, way, way == UnionWay.ByPairs ? options.PairsLimitSeconds : options.SlowWayLimitSeconds);
        }

        // ---- Question 3: the knockout.

        /// <summary>
        /// Takes each run as a knockout colour and cuts it by the runs above it. It writes each operation as it ends,
        /// and the totals at the end. It gives the pieces, or null when a run has no union.
        /// </summary>
        private Piece[] MeasureKnockoutOfEachRun(SKPath[] unions)
        {
            const string part = "each run a knockout colour";
            int runsWithoutUnion = unions.Count(union => union == null);
            OperationRecorder recorder = new OperationRecorder(folder, mapName, false);
            Stopwatch stopwatch = Stopwatch.StartNew();
            SKPath[] visible = Knockout.CutEachRunByTheRunsAbove(unions, part, recorder);
            double seconds = stopwatch.Elapsed.TotalSeconds;
            Piece[] pieces = visible.Select((path, run) => path == null || path.IsEmpty ? null : new Piece { Path = path, BaseRun = run }).Where(piece => piece != null).ToArray();
            WriteKnockoutTotals("q3-knockout.tsv", part, recorder.Records, seconds, false, runsWithoutUnion, pieces.Select(piece => piece.Path).ToList(), false);
            Console.WriteLine(mapName + ": knockout of each run done in " + seconds.ToString("F1") + " s");
            return runsWithoutUnion == 0 ? pieces : null;
        }

        /// <summary>Takes the overprinted runs as they are, and intersects each with the pieces below it.</summary>
        private void MeasureKnockoutWithOverprintedRuns(SKPath[] unions)
        {
            const string part = "overprinted runs as they are";
            OperationRecorder recorder = new OperationRecorder(folder, mapName, true);
            Stopwatch stopwatch = Stopwatch.StartNew();
            List<Piece> pieces = Knockout.CutWithOverprintedRuns(runs, unions, part, recorder, options.PairsLimitSeconds, out bool isStopped);
            double seconds = stopwatch.Elapsed.TotalSeconds;
            WriteKnockoutTotals("q3-knockout.tsv", part, recorder.Records, seconds, isStopped, unions.Count(union => union == null), pieces.Select(piece => piece.Path).ToList(), true);
            Console.WriteLine(mapName + ": knockout with overprinted runs done in " + seconds.ToString("F1") + " s");
        }

        /// <summary>
        /// Writes the row of one part of the knockout to a table of totals, with the samples of the window that two
        /// pieces cover, that a mark covers and no piece covers, and that a piece covers and no mark covers.
        /// </summary>
        private void WriteKnockoutTotals(string tableName, string part, List<OperationRecord> records, double seconds, bool isStopped, int runsWithoutUnion, List<SKPath> pieces, bool isAppended)
        {
            TsvTable totals = new TsvTable(Path.Combine(folder, tableName), isAppended, "map", "part", "runs_without_union", "operations", "failed_operations", "stopped_by_limit", "seconds", "longest_operation_seconds",
                                           "pieces", "contours", "segments", "samples_covered_by_two_pieces", "samples_of_marks_without_piece", "samples_of_piece_without_mark");
            SegmentCounts counts = PathMeasures.CountSegments(pieces);
            CompareCoverageOfPieces(pieces, out long coveredByTwo, out long marksWithoutPiece, out long pieceWithoutMark);
            totals.AddRow(mapName, part, runsWithoutUnion, records.Count, records.Count(record => !record.HasResult), isStopped, seconds, records.Count == 0 ? 0 : records.Max(record => record.Seconds),
                          pieces.Count, counts.Contours, counts.Segments, coveredByTwo, marksWithoutPiece, pieceWithoutMark);
        }

        /// <summary>
        /// Draws the pieces and the marks over the window. It counts the samples that two pieces cover, the samples
        /// that a mark covers and no piece covers, and the samples that a piece covers and no mark covers.
        /// </summary>
        private void CompareCoverageOfPieces(IReadOnlyList<SKPath> pieces, out long coveredByTwo, out long marksWithoutPiece, out long pieceWithoutMark)
        {
            coveredByTwo = marksWithoutPiece = pieceWithoutMark = 0;
            using (SKBitmap count = SampleGrid.CreateBitmap(window, SamplesPerMillimetre))
            using (SKBitmap ofMarks = SampleGrid.DrawCoverage(window, SamplesPerMillimetre, allMarks)) {
                // Each piece adds 1 to its samples, so a sample of two pieces gets 2.
                using (SKCanvas canvas = new SKCanvas(count))
                using (SKPaint paint = new SKPaint()) {
                    paint.IsAntialias = false;
                    paint.BlendMode = SKBlendMode.Plus;
                    paint.Color = new SKColor(0, 0, 0, 1);
                    canvas.Scale(SamplesPerMillimetre);
                    canvas.Translate(-window.Left, -window.Top);
                    foreach (SKPath piece in pieces)
                        if (piece.Bounds.IntersectsWith(window))
                            canvas.DrawPath(piece, paint);
                }
                ReadOnlySpan<byte> countSamples = count.GetPixelSpan(), markSamples = ofMarks.GetPixelSpan();
                for (int i = 0; i < countSamples.Length; ++i) {
                    if (countSamples[i] > 1) ++coveredByTwo;
                    if (countSamples[i] == 0 && markSamples[i] != 0) ++marksWithoutPiece;
                    if (countSamples[i] != 0 && markSamples[i] == 0) ++pieceWithoutMark;
                }
            }
        }

        // ---- Question 5: the digests.

        /// <summary>
        /// Does the operations of questions 2 and 3 on the runs that have at most LargestSegmentsOfRepeatedRun
        /// segments, two times, and writes the digests of the first time to digests.txt. The table says how many
        /// results have the same digest the second time.
        /// </summary>
        private void WriteDigests()
        {
            List<Run> smallRuns = runs.Where(run => PathMeasures.CountSegments(run.Marks).Segments <= options.LargestSegmentsOfRepeatedRun).ToList();
            List<SortedDictionary<string, string>> passes = new List<SortedDictionary<string, string>>();
            double seconds = 0;
            for (int pass = 0; pass < 2; ++pass) {
                Stopwatch stopwatch = Stopwatch.StartNew();
                SortedDictionary<string, string> digests = new SortedDictionary<string, string>(StringComparer.Ordinal);
                SKPath[] unions = new SKPath[smallRuns.Count];
                for (int i = 0; i < smallRuns.Count; ++i) {
                    unions[i] = PathOperations.Unite(smallRuns[i].Marks, UnionWay.ByPairs, double.MaxValue).Path;
                    digests["union by pairs\trun " + smallRuns[i].Index.ToString("D3")] = PathMeasures.ComputeDigest(unions[i]);
                }
                OperationRecorder recorder = new OperationRecorder();
                SKPath[] visible = Knockout.CutEachRunByTheRunsAbove(unions, "", recorder);
                for (int i = 0; i < smallRuns.Count; ++i)
                    digests["each run a knockout colour\trun " + smallRuns[i].Index.ToString("D3")] = PathMeasures.ComputeDigest(visible[i]);
                List<Piece> pieces = Knockout.CutWithOverprintedRuns(smallRuns, unions, "", recorder, double.MaxValue, out _);
                for (int i = 0; i < pieces.Count; ++i)
                    digests["overprinted runs as they are\tpiece " + i.ToString("D5")] = PathMeasures.ComputeDigest(pieces[i].Path);
                passes.Add(digests);
                seconds = stopwatch.Elapsed.TotalSeconds;
            }

            File.WriteAllLines(Path.Combine(folder, "digests.txt"), passes[0].Select(digest => mapName + "\t" + digest.Key + "\t" + digest.Value));
            TsvTable table = new TsvTable(Path.Combine(folder, "q5-two-passes.tsv"), "map", "runs", "runs_with_few_segments", "results", "results_without_digest", "equal_in_the_second_pass", "seconds_of_one_pass");
            table.AddRow(mapName, runs.Count, smallRuns.Count, passes[0].Count, passes[0].Count(digest => digest.Value == "none"),
                         passes[0].Count(digest => passes[1].TryGetValue(digest.Key, out string second) && second == digest.Value), seconds);
            Console.WriteLine(mapName + ": digests done");
        }

        // ---- Question 6: tiles.

        /// <summary>
        /// Cuts the marks to square tiles, unites each run in each tile by pairs, and cuts the runs of each tile in
        /// the two ways of question 3. It checks each union of a run and each result of the knockout colours against
        /// the samples of its operands over the tile. Then it counts the samples of paper that show in the pieces of
        /// the knockout colours, and compares those pieces with the drawing.
        /// </summary>
        private void MeasureTiles()
        {
            string variant = options.IsUncutKeptOnFailure ? "tiles-uncut" : "tiles";
            TsvTable failures = new TsvTable(Path.Combine(folder, "q6-" + variant + "-operations.tsv"), "map", "tile_mm", "run", "colour", "inside_blending", "operation", "operations", "with_no_result", "wrong_results");
            TsvTable table = new TsvTable(Path.Combine(folder, "q6-" + variant + ".tsv"), "map", "tile_mm", "tiles", "tiles_with_marks", "stopped_by_limit", "cut_seconds", "union_seconds", "knockout_seconds", "total_seconds", "cut_operations",
                "failed_cuts", "failed_unions", "knockout_operations", "failed_knockout_operations", "longest_operation_seconds", "pieces", "contours", "segments", "paper_samples_at_40", "paper_samples_on_borders_at_40",
                "paper_samples_on_borders_at_400", "border_samples_at_400", "digest_of_pieces");
            for (int sizeIndex = 0; sizeIndex < options.TileSides.Count; ++sizeIndex) {
                float side = options.TileSides[sizeIndex];
                double limitSeconds = options.TileSizeLimitSeconds[Math.Min(sizeIndex, options.TileSizeLimitSeconds.Count - 1)];
                Stopwatch total = Stopwatch.StartNew();
                double cutSeconds = 0, unionSeconds = 0, checkSeconds = 0;
                int tiles = 0, tilesWithMarks = 0, cutOperations = 0, failedCuts = 0, failedUnions = 0;
                int[] unionsOfRun = new int[runs.Count], failedPairsOfRun = new int[runs.Count], wrongUnionsOfRun = new int[runs.Count];
                bool isStopped = false;
                OperationRecorder recorder = new OperationRecorder(), overprintRecorder = new OperationRecorder();
                List<OperationRecord> records = recorder.Records;
                List<SKPath> pieces = new List<SKPath>(), overprintPieces = new List<SKPath>();
                List<int> runOfEachPiece = new List<int>();
                bool hasOverprintedRuns = runs.Any(run => run.IsInsideBlending) && !options.IsUncutKeptOnFailure, isOverprintStopped = false;
                double overprintSeconds = 0;

                for (float top = (float)Math.Floor(bounds.Top / side) * side; top < bounds.Bottom && !isStopped; top += side) {
                    // The marks are cut to the row of tiles first, and the parts of the row are cut to each tile.
                    long rowStart = Stopwatch.GetTimestamp();
                    SKRect row = new SKRect((float)Math.Floor(bounds.Left / side) * side, top, bounds.Right + side, top + side);
                    List<SKPath>[] marksInRow = runs.Select(run => CutToRectangle(run.Marks, row, ref cutOperations, ref failedCuts)).ToArray();
                    cutSeconds += Stopwatch.GetElapsedTime(rowStart).TotalSeconds;

                    for (float left = row.Left; left < bounds.Right; left += side) {
                        if (total.Elapsed.TotalSeconds > limitSeconds) {
                            isStopped = true;
                            break;
                        }
                        ++tiles;
                        SKRect tile = new SKRect(left, top, left + side, top + side);
                        SKPath[] unions = new SKPath[runs.Count];
                        for (int run = 0; run < runs.Count; ++run) {
                            long cutStart = Stopwatch.GetTimestamp();
                            List<SKPath> marksInTile = CutToRectangle(marksInRow[run], tile, ref cutOperations, ref failedCuts);
                            cutSeconds += Stopwatch.GetElapsedTime(cutStart).TotalSeconds;
                            if (marksInTile.Count == 0)
                                continue;
                            UnionResult union = PathOperations.Unite(marksInTile, UnionWay.ByPairs, double.MaxValue);
                            unionSeconds += union.Seconds;
                            failedUnions += union.Failures;
                            unions[run] = union.Path;
                            ++unionsOfRun[run];
                            failedPairsOfRun[run] += union.Failures;
                            if (union.Path != null) {
                                long checkStart = Stopwatch.GetTimestamp();
                                if (SampleGrid.CountThickSamplesOffExpectation(tile, SamplesPerMillimetre, marksInTile, Array.Empty<SKPath>(), union.Path) > 0)
                                    ++wrongUnionsOfRun[run];
                                checkSeconds += Stopwatch.GetElapsedTime(checkStart).TotalSeconds;
                            }
                        }
                        if (unions.All(union => union == null))
                            continue;
                        ++tilesWithMarks;
                        recorder.CountThickSamplesOfWrongResult = (left, right, operation, result) => SampleGrid.CountThickSamplesOffExpectation(tile, SamplesPerMillimetre,
                            operation == SKPathOp.Difference ? new SKPath[] { left } : new SKPath[] { left, right }, operation == SKPathOp.Difference ? new SKPath[] { right } : Array.Empty<SKPath>(), result);
                        SKPath[] visible = Knockout.CutEachRunByTheRunsAbove(unions, "tile", recorder, options.IsUncutKeptOnFailure);
                        for (int run = 0; run < visible.Length; ++run) {
                            if (visible[run] != null && !visible[run].IsEmpty) {
                                pieces.Add(visible[run]);
                                runOfEachPiece.Add(run);
                            }
                        }
                        if (hasOverprintedRuns && !isOverprintStopped) {
                            long overprintStart = Stopwatch.GetTimestamp();
                            List<Piece> withInks = Knockout.CutWithOverprintedRuns(runs, unions, "tile", overprintRecorder, limitSeconds - total.Elapsed.TotalSeconds, out isOverprintStopped);
                            overprintSeconds += Stopwatch.GetElapsedTime(overprintStart).TotalSeconds;
                            overprintPieces.AddRange(withInks.Select(piece => piece.Path));
                        }
                        foreach (SKPath union in unions)
                            union?.Dispose();
                    }
                }
                double knockoutSeconds = records.Sum(record => record.Seconds);
                double totalSeconds = total.Elapsed.TotalSeconds - overprintSeconds - checkSeconds - recorder.CheckSeconds;

                SegmentCounts counts = PathMeasures.CountSegments(pieces);
                string paper = "", paperOnBorders = "", paperOnBordersFine = "", borderSamples = "", digest = "";
                if (!isStopped) {
                    paper = CountPaperSamples(SamplesPerMillimetre, pieces, 0, out _).ToString(CultureInfo.InvariantCulture);
                    paperOnBorders = CountPaperSamples(SamplesPerMillimetre, pieces, side, out _).ToString(CultureInfo.InvariantCulture);
                    paperOnBordersFine = CountPaperSamples(400, pieces, side, out long samplesInStrips).ToString(CultureInfo.InvariantCulture);
                    borderSamples = samplesInStrips.ToString(CultureInfo.InvariantCulture);
                    digest = Convert.ToHexString(SHA256.HashData(Encoding.ASCII.GetBytes(string.Concat(pieces.Select(PathMeasures.ComputeDigest))))).ToLowerInvariant();
                    ComparePiecesWithDrawing(pieces, runOfEachPiece, variant + "-" + side.ToString(CultureInfo.InvariantCulture) + "mm", side == options.TileSideOfImages && !options.IsUncutKeptOnFailure);
                    if (!options.IsUncutKeptOnFailure) {
                        string sizeOfTiles = "tiles of " + side.ToString(CultureInfo.InvariantCulture) + " mm, ";
                        WriteKnockoutTotals("q6-tiles-knockout.tsv", sizeOfTiles + "each run a knockout colour", records, knockoutSeconds, false, 0, pieces, sizeIndex > 0);
                        if (hasOverprintedRuns)
                            WriteKnockoutTotals("q6-tiles-knockout.tsv", sizeOfTiles + "overprinted runs as they are", overprintRecorder.Records, overprintSeconds, isOverprintStopped, 0, overprintPieces, true);
                    }
                }
                for (int run = 0; run < runs.Count; ++run)
                    if (unionsOfRun[run] > 0)
                        failures.AddRow(mapName, side, run, runs[run].ColourName, runs[run].IsInsideBlending, "union of the run by pairs", unionsOfRun[run], failedPairsOfRun[run], wrongUnionsOfRun[run]);
                foreach (IGrouping<(int Run, string Operation), OperationRecord> group in records.GroupBy(record => (record.Run, record.Operation)).OrderBy(group => group.Key.Run).ThenBy(group => group.Key.Operation))
                    failures.AddRow(mapName, side, group.Key.Run, runs[group.Key.Run].ColourName, runs[group.Key.Run].IsInsideBlending, group.Key.Operation, group.Count(), group.Count(record => !record.HasResult),
                                    group.Count(record => record.ThickSamplesOfWrongResult > 0));
                table.AddRow(mapName, side, tiles, tilesWithMarks, isStopped, cutSeconds, unionSeconds, knockoutSeconds, totalSeconds, cutOperations, failedCuts, failedUnions, records.Count,
                             records.Count(record => !record.HasResult), records.Count == 0 ? 0 : records.Max(record => record.Seconds), pieces.Count, counts.Contours, counts.Segments, paper, paperOnBorders,
                             paperOnBordersFine, borderSamples, digest);
                Console.WriteLine(mapName + ": tiles of " + side + " mm " + (isStopped ? "stopped after " : "done in ") + totalSeconds.ToString("F1") + " s");
                foreach (SKPath piece in pieces.Concat(overprintPieces))
                    piece.Dispose();
            }
        }

        /// <summary>
        /// Gives the parts of paths that are inside a rectangle. A path that is inside as a whole is given as it is.
        /// A path that is partly inside is cut with one intersection.
        /// </summary>
        private static List<SKPath> CutToRectangle(List<SKPath> paths, SKRect rectangle, ref int cutOperations, ref int failedCuts)
        {
            List<SKPath> inside = new List<SKPath>();
            using (SKPath rectanglePath = new SKPath()) {
                rectanglePath.AddRect(rectangle);
                foreach (SKPath path in paths) {
                    SKRect pathBounds = path.Bounds;
                    if (!pathBounds.IntersectsWith(rectangle))
                        continue;
                    if (rectangle.Contains(pathBounds)) {
                        inside.Add(path);
                        continue;
                    }
                    ++cutOperations;
                    SKPath cut = path.Op(rectanglePath, SKPathOp.Intersect);
                    if (cut == null)
                        ++failedCuts;
                    else if (!cut.IsEmpty)
                        inside.Add(cut);
                }
            }
            return inside;
        }

        /// <summary>
        /// Counts the samples of the window that a mark covers and no piece covers. When tileSide is 0, it counts
        /// over the whole window. If not, it counts in strips of 8 samples to each side of each border of the tiles,
        /// and samplesInStrips gets the count of the samples that the strips hold.
        /// </summary>
        private long CountPaperSamples(int samplesPerMillimetre, List<SKPath> pieces, float tileSide, out long samplesInStrips)
        {
            List<SKRect> parts = new List<SKRect>();
            if (tileSide == 0) {
                parts = SampleGrid.DivideWindow(window, samplesPerMillimetre);
            }
            else {
                float half = 8F / samplesPerMillimetre;
                for (float x = (float)Math.Ceiling(window.Left / tileSide) * tileSide; x < window.Right; x += tileSide)
                    parts.AddRange(SampleGrid.DivideWindow(new SKRect(x - half, window.Top, x + half, window.Bottom), samplesPerMillimetre));
                for (float y = (float)Math.Ceiling(window.Top / tileSide) * tileSide; y < window.Bottom; y += tileSide)
                    parts.AddRange(SampleGrid.DivideWindow(new SKRect(window.Left, y - half, window.Right, y + half), samplesPerMillimetre));
            }

            List<SKPath> marksInWindow = allMarks.Where(mark => mark.Bounds.IntersectsWith(window)).ToList();
            List<SKPath> piecesInWindow = pieces.Where(piece => piece.Bounds.IntersectsWith(window)).ToList();
            long paper = 0, samples = 0;
            Parallel.ForEach(parts, part => {
                using (SKBitmap ofMarks = SampleGrid.DrawCoverage(part, samplesPerMillimetre, marksInWindow))
                using (SKBitmap ofPieces = SampleGrid.DrawCoverage(part, samplesPerMillimetre, piecesInWindow)) {
                    SampleGrid.CompareSamples(ofMarks, ofPieces, out long differing, out long onlyInMarks);
                    System.Threading.Interlocked.Add(ref paper, onlyInMarks);
                    System.Threading.Interlocked.Add(ref samples, (long)ofMarks.Width * ofMarks.Height);
                }
            });
            samplesInStrips = samples;
            return paper;
        }

        // ---- Question 8: the pieces against the drawing.

        /// <summary>
        /// Draws pieces, and the runs in their order with no blending, over the window. Each sample gets the index of
        /// its run. It writes the count of the samples that differ, of the thick ones and of the largest group, and
        /// saves the two images, their difference and the pieces in false colours when isImageSaved is set.
        /// </summary>
        private void ComparePiecesWithDrawing(IReadOnlyList<SKPath> pieces, IReadOnlyList<int> runOfEachPiece, string source, bool isImageSaved)
        {
            using (SKBitmap ofPieces = DrawRunIndexes(pieces, runOfEachPiece))
            using (SKBitmap ofDrawing = DrawRunIndexes(allMarks, runOfEachMark)) {
                ReadOnlySpan<uint> pieceSamples = System.Runtime.InteropServices.MemoryMarshal.Cast<byte, uint>(ofPieces.GetPixelSpan());
                ReadOnlySpan<uint> drawingSamples = System.Runtime.InteropServices.MemoryMarshal.Cast<byte, uint>(ofDrawing.GetPixelSpan());
                bool[] isDiffering = new bool[pieceSamples.Length], isInked = new bool[pieceSamples.Length];
                long inked = 0;
                for (int i = 0; i < isDiffering.Length; ++i) {
                    isDiffering[i] = pieceSamples[i] != drawingSamples[i];
                    isInked[i] = drawingSamples[i] != 0;
                    if (isInked[i])
                        ++inked;
                }
                bool[] isShownAsDiffering = (bool[])isDiffering.Clone();
                SampleGrid.MeasureDifference(isDiffering, ofPieces.Width, ofPieces.Height, out long differing, out long thick, out int largestGroup);

                TsvTable table = new TsvTable(Path.Combine(folder, "q8-pieces-" + source + ".tsv"), "map", "pieces_of", "window_left_mm", "window_top_mm", "window_width_mm", "window_height_mm", "samples", "samples_with_ink",
                                              "differing_samples", "thick_differing_samples", "largest_group");
                table.AddRow(mapName, source, window.Left, window.Top, window.Width, window.Height, (long)ofPieces.Width * ofPieces.Height, inked, differing, thick, largestGroup);
                if (!isImageSaved)
                    return;

                SKColor[] trueColours = runs.Select(run => { System.Drawing.Color colour = ColorConverter.ToColor(run.Colour); return new SKColor(colour.R, colour.G, colour.B); }).ToArray();
                SKColor[] falseColours = runs.Select(run => SKColor.FromHsv(run.Index * 137.508F % 360, 55 + run.Index * 29 % 45, 60 + run.Index * 17 % 40)).ToArray();
                SaveImage(ofPieces, (sample, index) => sample == 0 ? SKColors.White : trueColours[sample - 1], "q8-" + source + "-pieces.png");
                SaveImage(ofDrawing, (sample, index) => sample == 0 ? SKColors.White : trueColours[sample - 1], "q8-" + source + "-drawing.png");
                SaveImage(ofPieces, (sample, index) => sample == 0 ? SKColors.White : falseColours[sample - 1], "q8-" + source + "-pieces-false-colours.png");
                SaveImage(ofPieces, (sample, index) => isShownAsDiffering[index] ? SKColors.Red : isInked[index] ? new SKColor(225, 225, 225) : SKColors.White, "q8-" + source + "-difference.png");
            }
            Console.WriteLine(mapName + ": pieces of " + source + " compared with the drawing");
        }

        /// <summary>
        /// Draws paths without antialiasing over the window. Each sample gets the index of the run of the last path
        /// that covers it, plus 1. A sample that no path covers gets 0.
        /// </summary>
        private SKBitmap DrawRunIndexes(IReadOnlyList<SKPath> paths, IReadOnlyList<int> runOfPath)
        {
            int width = Math.Max(1, (int)Math.Ceiling(window.Width * SamplesPerMillimetre)), height = Math.Max(1, (int)Math.Ceiling(window.Height * SamplesPerMillimetre));
            SKBitmap bitmap = new SKBitmap(new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Premul));
            bitmap.Erase(SKColors.Transparent);
            using (SKCanvas canvas = new SKCanvas(bitmap))
            using (SKPaint paint = new SKPaint()) {
                paint.IsAntialias = false;
                paint.BlendMode = SKBlendMode.Src;
                canvas.Scale(SamplesPerMillimetre);
                canvas.Translate(-window.Left, -window.Top);
                for (int i = 0; i < paths.Count; ++i) {
                    if (!paths[i].Bounds.IntersectsWith(window))
                        continue;
                    int value = runOfPath[i] + 1;
                    paint.Color = new SKColor((byte)(value & 255), (byte)((value >> 8) & 255), (byte)((value >> 16) & 255), 255);
                    canvas.DrawPath(paths[i], paint);
                }
            }
            return bitmap;
        }

        /// <summary>
        /// Saves an image of the size of a bitmap of run indexes. colourOfSample gives the colour of a sample from
        /// its run index and its place in the bitmap. The y axis of the map points up, so the rows are written from
        /// the last to the first.
        /// </summary>
        private void SaveImage(SKBitmap runIndexes, Func<int, int, SKColor> colourOfSample, string fileName)
        {
            int width = runIndexes.Width, height = runIndexes.Height;
            ReadOnlySpan<uint> samples = System.Runtime.InteropServices.MemoryMarshal.Cast<byte, uint>(runIndexes.GetPixelSpan());
            using (SKBitmap image = new SKBitmap(new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Opaque))) {
                Span<uint> pixels = System.Runtime.InteropServices.MemoryMarshal.Cast<byte, uint>(image.GetPixelSpan());
                for (int y = 0; y < height; ++y) {
                    for (int x = 0; x < width; ++x) {
                        int index = y * width + x;
                        SKColor colour = colourOfSample((int)(samples[index] & 0xFFFFFF), index);
                        pixels[(height - 1 - y) * width + x] = (uint)(colour.Red | (colour.Green << 8) | (colour.Blue << 16) | (255 << 24));
                    }
                }
                using (SKData data = image.Encode(SKEncodedImageFormat.Png, 100))
                using (FileStream stream = File.Create(Path.Combine(folder, fileName)))
                    data.SaveTo(stream);
            }
        }
    }

    /// <summary>A table that the probe writes as a file of tab-separated values, one row at a time.</summary>
    public sealed class TsvTable
    {
        private readonly string path;

        /// <summary>Starts a new table with the names of its columns.</summary>
        public TsvTable(string path, params string[] columns) : this(path, false, columns)
        {
        }

        /// <summary>Starts a table with the names of its columns. When isAppended is set and the file is there, the rows go to its end.</summary>
        public TsvTable(string path, bool isAppended, params string[] columns)
        {
            this.path = path;
            if (!isAppended || !File.Exists(path))
                File.WriteAllText(path, string.Join("\t", columns) + "\n");
        }

        /// <summary>Writes a row to the file. A number has a point as its decimal sign, and a time has 6 decimals.</summary>
        public void AddRow(params object[] values)
        {
            File.AppendAllText(path, string.Join("\t", values.Select(value => value is double seconds ? seconds.ToString("0.######", CultureInfo.InvariantCulture)
                : value is bool flag ? (flag ? "yes" : "no") : Convert.ToString(value, CultureInfo.InvariantCulture))) + "\n");
        }
    }
}
