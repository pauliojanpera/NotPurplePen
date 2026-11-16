using System;
using System.Collections.Generic;
using System.Diagnostics;
using PurplePen.Graphics2D;
using SkiaSharp;

namespace SkiaPathOpsProbe
{
    /// <summary>
    /// An area in which the result is one value: the part of one run that shows, with the inks that lie on it.
    /// </summary>
    public sealed class Piece
    {
        /// <summary>The area of the piece.</summary>
        public SKPath Path;

        /// <summary>The run of the knockout colour that shows in the piece. It is -1 when the piece lies on paper.</summary>
        public int BaseRun;

        /// <summary>The colours of the inks that lie on the piece.</summary>
        public List<CmykColor> Inks = new List<CmykColor>();
    }

    /// <summary>
    /// The cut of the runs of a drawing into pieces that do not overlap, with the boolean path operations of Skia.
    /// </summary>
    public static class Knockout
    {
        /// <summary>
        /// Takes each run as a knockout colour. From the top run down, it cuts each run by the union of the runs
        /// above it. The result has one path for each run: the part of the run that shows. An entry is null when
        /// the run has no union. When a difference gives no result, the entry is null too, or it is the run as it
        /// is, not cut, when isUncutKeptOnFailure is set.
        /// </summary>
        public static SKPath[] CutEachRunByTheRunsAbove(IReadOnlyList<SKPath> unions, string part, OperationRecorder recorder, bool isUncutKeptOnFailure = false)
        {
            SKPath[] pieces = new SKPath[unions.Count];
            SKPath above = new SKPath();
            for (int run = unions.Count - 1; run >= 0; --run) {
                if (unions[run] == null)
                    continue;
                pieces[run] = PathOperations.Operate(unions[run], above, SKPathOp.Difference, part, run, recorder) ?? (isUncutKeptOnFailure ? new SKPath(unions[run]) : null);
                SKPath larger = PathOperations.Operate(above, unions[run], SKPathOp.Union, part, run, recorder);
                if (larger != null) {
                    above.Dispose();
                    above = larger;
                }
            }
            above.Dispose();
            return pieces;
        }

        /// <summary>
        /// Takes the overprinted runs as they are. A knockout run cuts each run below it. Then, from the lowest run
        /// up, each overprinted run is intersected with the pieces below it: the part of a piece that the ink covers
        /// becomes a piece of its own, and the part of the ink that covers no piece becomes a piece on paper.
        /// isStopped is set when the time limit stopped the work.
        /// </summary>
        public static List<Piece> CutWithOverprintedRuns(IReadOnlyList<Run> runs, IReadOnlyList<SKPath> unions, string part, OperationRecorder recorder, double limitSeconds, out bool isStopped)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            isStopped = false;

            // The part of each run that the knockout runs above it leave.
            SKPath[] visible = new SKPath[runs.Count];
            SKPath knockoutAbove = new SKPath();
            for (int run = runs.Count - 1; run >= 0; --run) {
                if (unions[run] == null)
                    continue;
                visible[run] = PathOperations.Operate(unions[run], knockoutAbove, SKPathOp.Difference, part, run, recorder);
                if (!runs[run].IsInsideBlending) {
                    SKPath larger = PathOperations.Operate(knockoutAbove, unions[run], SKPathOp.Union, part, run, recorder);
                    if (larger != null) {
                        knockoutAbove.Dispose();
                        knockoutAbove = larger;
                    }
                }
            }
            knockoutAbove.Dispose();

            List<Piece> pieces = new List<Piece>();
            for (int run = 0; run < runs.Count && !isStopped; ++run) {
                SKPath shows = visible[run];
                if (shows == null || shows.IsEmpty)
                    continue;
                if (!runs[run].IsInsideBlending) {
                    pieces.Add(new Piece { Path = shows, BaseRun = run });
                    continue;
                }

                CmykColor ink = runs[run].Colour;
                SKPath onPaper = shows;
                int countBelow = pieces.Count;
                for (int i = 0; i < countBelow; ++i) {
                    if (stopwatch.Elapsed.TotalSeconds > limitSeconds) {
                        isStopped = true;
                        break;
                    }
                    Piece below = pieces[i];
                    if (below.Path.IsEmpty || !below.Path.Bounds.IntersectsWith(shows.Bounds))
                        continue;

                    SKPath covered = below.Path;
                    if (!below.Inks.Contains(ink)) {
                        covered = PathOperations.Operate(below.Path, shows, SKPathOp.Intersect, part, run, recorder);
                        if (covered == null || covered.IsEmpty)
                            continue;
                        SKPath rest = PathOperations.Operate(below.Path, shows, SKPathOp.Difference, part, run, recorder);
                        if (rest == null)
                            continue;
                        Piece withInk = new Piece { Path = covered, BaseRun = below.BaseRun };
                        withInk.Inks.AddRange(below.Inks);
                        withInk.Inks.Add(ink);
                        pieces.Add(withInk);
                        below.Path = rest;
                    }
                    SKPath smaller = PathOperations.Operate(onPaper, covered, SKPathOp.Difference, part, run, recorder);
                    if (smaller != null)
                        onPaper = smaller;
                }
                if (!onPaper.IsEmpty) {
                    Piece inkOnPaper = new Piece { Path = onPaper, BaseRun = -1 };
                    inkOnPaper.Inks.Add(ink);
                    pieces.Add(inkOnPaper);
                }
            }
            pieces.RemoveAll(piece => piece.Path.IsEmpty);
            return pieces;
        }
    }
}
