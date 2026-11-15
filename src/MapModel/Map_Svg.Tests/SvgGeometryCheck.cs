using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using PurplePen.Graphics2D;

namespace Map_Svg.Tests
{
    // Compares the geometry of drawings read back from a document with what the calls that wrote it draw (V-10): in
    // order, the n-th drawing of the page, its uses expanded, with the n-th call that draws, as the SVG target writes an
    // element for each such call in the order of the calls, and the characters of a line text one by one. Two drawings
    // agree when their signatures are the same, each point and length lies within its allowance of the call's, their
    // clips agree likewise, and so do the tiles of the patterns they paint with, each compared once.
    public sealed class GeometryComparison
    {
        // The most problems kept for a message.
        private const int ProblemsKept = 5;

        // The first problems found.
        public readonly List<string> Problems = new List<string>();

        // How many problems were found.
        public int ProblemCount { get; private set; }

        // How many drawings were compared, those of clips and patterns' tiles among them.
        public int DrawingsCompared { get; private set; }

        // The largest distance of a point or length from the call's, in steps of the map.
        public double LargestDeviation { get; private set; }

        // The largest share of its allowance that a point or length takes.
        public double LargestShareOfAllowance { get; private set; }

        // The clips and the tiles compared already.
        private readonly HashSet<(DrawnGeometry, DrawnGeometry)> clipsCompared = new HashSet<(DrawnGeometry, DrawnGeometry)>();
        private readonly HashSet<(GeometryTile, GeometryTile)> tilesCompared = new HashSet<(GeometryTile, GeometryTile)>();

        // Compare the drawings of calls with those read back, in order. The drawings after the first whose signatures
        // differ are not compared, since one left out or added would set every one after it apart.
        // where: where the drawings are, for messages.
        // calls: the calls' drawings.
        // read: the drawings read back.
        // stepLength: how long a step of the map is in their coordinates.
        public void CompareDrawings(string where, IReadOnlyList<DrawnGeometry> calls, IReadOnlyList<DrawnGeometry> read, double stepLength)
        {
            int count = Math.Min(calls.Count, read.Count);
            for (int index = 0; index < count; ++index) {
                if (!Compare(where + ", drawing " + (index + 1).ToString(CultureInfo.InvariantCulture), calls[index], read[index], stepLength)) {
                    AddProblem(string.Format(CultureInfo.InvariantCulture, "{0}: the {1} drawings after drawing {2} are not compared", where, calls.Count - index - 1, index + 1));
                    return;
                }
            }
            if (calls.Count != read.Count) {
                AddProblem(string.Format(CultureInfo.InvariantCulture, "{0}: the calls draw {1} drawings, the document {2}; the first unmatched is {3}", where,
                                         calls.Count, read.Count, calls.Count > read.Count ? calls[count].What : read[count].What));
            }
        }

        // Compare a call's drawing with one read back; return whether their signatures are the same.
        // where: what the drawing is, for messages.
        // call: the call's drawing.
        // read: the drawing read back.
        // stepLength: how long a step of the map is in their coordinates.
        public bool Compare(string where, DrawnGeometry call, DrawnGeometry read, double stepLength)
        {
            ++DrawingsCompared;
            string callSignature = call.Signature.ToString(), readSignature = read.Signature.ToString();
            if (callSignature != readSignature) {
                AddProblem(string.Format(CultureInfo.InvariantCulture, "{0}: {1} draws \"{2}\" where the document's {3} draws \"{4}\"", where, call.What,
                                         Shorten(callSignature), read.What, Shorten(readSignature)));
                return false;
            }

            for (int index = 0; index < call.Points.Count; ++index) {
                CheckDeviation(where, call, read, "point " + (index + 1).ToString(CultureInfo.InvariantCulture) + " " + read.Points[index],
                               call.Points[index].DistanceTo(read.Points[index]), read.PointAllowances[index], stepLength);
            }
            for (int index = 0; index < call.Lengths.Count; ++index) {
                CheckDeviation(where, call, read, "length " + (index + 1).ToString(CultureInfo.InvariantCulture),
                               Math.Abs(call.Lengths[index] - read.Lengths[index]), read.LengthAllowances[index], stepLength);
            }

            if (call.Clips.Count != read.Clips.Count) {
                AddProblem(string.Format(CultureInfo.InvariantCulture, "{0}: {1} lies in {2} clips, the document's {3} in {4}", where, call.What, call.Clips.Count,
                                         read.What, read.Clips.Count));
            }
            else {
                for (int index = 0; index < call.Clips.Count; ++index) {
                    if (clipsCompared.Add((call.Clips[index], read.Clips[index])))
                        Compare(where + ", clip " + (index + 1).ToString(CultureInfo.InvariantCulture), call.Clips[index], read.Clips[index], stepLength);
                }
            }

            if (call.Pattern != null && read.Pattern != null && tilesCompared.Add((call.Pattern, read.Pattern)))
                CompareDrawings(where + ", " + read.Pattern.What, call.Pattern.Drawings, read.Pattern.Drawings, 1);
            return true;
        }

        // A message that names the problems found, or null when there is none.
        // name: the name of what was compared.
        public string Describe(string name)
        {
            if (ProblemCount == 0)
                return null;
            return string.Format(CultureInfo.InvariantCulture, "The geometry of {0} differs from its calls in {1} places, the first of them: {2}.", name,
                                 ProblemCount, string.Join("; ", Problems));
        }

        // The measurements of the comparison, as a table of one row: the drawings compared, the largest distance of a
        // point or length from the call's, in steps of the map, and the largest share of its allowance.
        public string ToTsv()
        {
            return "drawings compared\tlargest deviation in steps\tlargest share of an allowance\n" +
                   string.Format(CultureInfo.InvariantCulture, "{0}\t{1:0.###}\t{2:0.###}\n", DrawingsCompared, LargestDeviation, LargestShareOfAllowance);
        }

        // Check how far a point or length of a drawing read back lies from the call's against its allowance, and keep
        // the largest deviation and share of an allowance.
        // where: what the drawing is, for messages.
        // call: the call's drawing.
        // read: the drawing read back.
        // what: which point or length it is, for messages.
        // distance: how far it lies from the call's.
        // allowance: how far it may lie.
        // stepLength: how long a step of the map is in their coordinates.
        private void CheckDeviation(string where, DrawnGeometry call, DrawnGeometry read, string what, double distance, double allowance, double stepLength)
        {
            double slack = 1e-9 * stepLength;
            LargestDeviation = Math.Max(LargestDeviation, distance / stepLength);
            if (allowance > 0)
                LargestShareOfAllowance = Math.Max(LargestShareOfAllowance, distance / allowance);
            if (distance > allowance + slack) {
                AddProblem(string.Format(CultureInfo.InvariantCulture, "{0}: {1}, {2} of the document's {3}, lies {4:0.###} steps from the call's, more than the {5:0.###} its rounding allows",
                                         where, call.What, what, read.What, distance / stepLength, allowance / stepLength));
            }
        }

        // Record a problem, keeping the first few for the message.
        // problem: what the problem is.
        private void AddProblem(string problem)
        {
            ++ProblemCount;
            if (Problems.Count < ProblemsKept)
                Problems.Add(problem);
        }

        // A signature cut to its first 200 characters, for a message.
        // signature: the signature.
        private static string Shorten(string signature)
        {
            return signature.Length <= 200 ? signature : signature.Substring(0, 200) + "...";
        }
    }

    // The geometry check of the rendering comparison (V-10): reads a case's document back into page coordinates and
    // compares it with what the case's drawing calls draw, within what the rounding of the document's numbers allows.
    public static class SvgGeometryCheck
    {
        // Compare the geometry of a case's document with its calls, record the comparison in the loop's results as
        // geometry/<name>.tsv, and describe how they differ, or return null when they agree.
        // comparisonCase: the case.
        // recording: the recording of its drawing.
        // document: the document written of it.
        // textMetrics: the metrics the application lays text out with.
        public static string Describe(ComparisonCase comparisonCase, RecordingGraphicsTarget recording, string document, ITextMetrics textMetrics)
        {
            GeometryComparison comparison = Compare(comparisonCase, recording, document, textMetrics);
            LoopResults.SaveText(Path.Combine("geometry", comparisonCase.Name + ".tsv"), comparison.ToTsv());
            return comparison.Describe(comparisonCase.Name);
        }

        // Compare the geometry of a case's document with its calls.
        // comparisonCase: the case.
        // recording: the recording of its drawing.
        // document: the document written of it.
        // textMetrics: the metrics the application lays text out with.
        public static GeometryComparison Compare(ComparisonCase comparisonCase, RecordingGraphicsTarget recording, string document, ITextMetrics textMetrics)
        {
            using (Matrix areaToImage = comparisonCase.CreateAreaToImageTransform())
                return Compare(Affine.FromMatrix(areaToImage), comparisonCase.Area, recording, document, textMetrics);
        }

        // Compare the geometry of a document with the calls that drew its first page, which shows an area of the map
        // under a transform.
        // toPage: the transform from the map's coordinates to the page's.
        // area: the area of the map the page shows.
        // recording: the recording of the calls.
        // document: the document.
        // textMetrics: the metrics the application lays text out with.
        public static GeometryComparison Compare(Affine toPage, System.Drawing.RectangleF area, RecordingGraphicsTarget recording, string document, ITextMetrics textMetrics)
        {
            CallGeometryRecorder calls = new CallGeometryRecorder(textMetrics, toPage);
            recording.Playback(calls);
            SvgGeometryReader read = SvgGeometryReader.ReadFirstPage(document);

            GeometryComparison comparison = new GeometryComparison();
            comparison.Compare("the page's clip", CallGeometryRecorder.RectangleClip(area, toPage, "the area of the page"), read.PageClip, read.StepOnPage);
            comparison.CompareDrawings("the page", calls.Drawings, read.Drawings, read.StepOnPage);
            return comparison;
        }
    }
}
