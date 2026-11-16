using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SkiaSharp;

namespace SkiaPathOpsProbe
{
    /// <summary>
    /// The measurements of shapes that the probe makes: edges that two areas share (question 4), and what the
    /// operations do with curves (question 7).
    /// </summary>
    public static class ShapeProbe
    {
        /// <summary>The place of the shapes on the page, in millimetres. It is not a round number of binary fractions, as the coordinates of a map are not.</summary>
        private const float BaseX = 123.4F, BaseY = 87.6F;

        /// <summary>The ratio of the distance of a control point to the radius, for a quarter of a circle as one cubic.</summary>
        private const float Kappa = 0.5522848F;

        /// <summary>Two shapes with a name, and the exact area of their intersection as a function of their areas.</summary>
        private sealed class ShapePair
        {
            public string Name;
            public SKPath First;
            public SKPath Second;
            public Func<double, double, double> ExactIntersectionArea;
        }

        /// <summary>Does each measurement of the shapes and writes the tables to the results folder.</summary>
        public static void Measure(string resultsFolder)
        {
            Directory.CreateDirectory(resultsFolder);
            MeasureSharedEdges(resultsFolder);
            MeasureCurves(resultsFolder);
        }

        // ---- The shapes.

        /// <summary>Makes a closed polygon from pairs of coordinates, which are moved to the place of the shapes.</summary>
        private static SKPath MakePolygon(params float[] coordinates)
        {
            SKPath path = new SKPath();
            path.MoveTo(BaseX + coordinates[0], BaseY + coordinates[1]);
            for (int i = 2; i < coordinates.Length; i += 2)
                path.LineTo(BaseX + coordinates[i], BaseY + coordinates[i + 1]);
            path.Close();
            return path;
        }

        /// <summary>Makes the shape to the left of a cubic curve that goes from (10, 0) to (10, 10). The shape draws the curve from its start to its end.</summary>
        private static SKPath MakeLeftOfCurve()
        {
            SKPath path = new SKPath();
            path.MoveTo(BaseX, BaseY);
            path.LineTo(BaseX + 10, BaseY);
            path.CubicTo(BaseX + 14, BaseY + 3, BaseX + 6, BaseY + 7, BaseX + 10, BaseY + 10);
            path.LineTo(BaseX, BaseY + 10);
            path.Close();
            return path;
        }

        /// <summary>
        /// Makes the shape to the right of the same cubic curve, moved by shift in x. It draws the curve from its
        /// start to its end when isSameDirection is set, and from its end to its start if not.
        /// </summary>
        private static SKPath MakeRightOfCurve(bool isSameDirection, float shift)
        {
            SKPath path = new SKPath();
            if (isSameDirection) {
                path.MoveTo(BaseX + 10 + shift, BaseY);
                path.CubicTo(BaseX + 14 + shift, BaseY + 3, BaseX + 6 + shift, BaseY + 7, BaseX + 10 + shift, BaseY + 10);
                path.LineTo(BaseX + 20, BaseY + 10);
                path.LineTo(BaseX + 20, BaseY);
            }
            else {
                path.MoveTo(BaseX + 20, BaseY);
                path.LineTo(BaseX + 20, BaseY + 10);
                path.LineTo(BaseX + 10 + shift, BaseY + 10);
                path.CubicTo(BaseX + 6 + shift, BaseY + 7, BaseX + 14 + shift, BaseY + 3, BaseX + 10 + shift, BaseY);
            }
            path.Close();
            return path;
        }

        /// <summary>Makes a circle of four cubic curves.</summary>
        private static SKPath MakeCircleOfCubics(float centreX, float centreY, float radius)
        {
            float x = BaseX + centreX, y = BaseY + centreY, control = Kappa * radius;
            SKPath path = new SKPath();
            path.MoveTo(x + radius, y);
            path.CubicTo(x + radius, y + control, x + control, y + radius, x, y + radius);
            path.CubicTo(x - control, y + radius, x - radius, y + control, x - radius, y);
            path.CubicTo(x - radius, y - control, x - control, y - radius, x, y - radius);
            path.CubicTo(x + control, y - radius, x + radius, y - control, x + radius, y);
            path.Close();
            return path;
        }

        // ---- Question 4: edges that two areas share.

        /// <summary>
        /// For each pair of shapes and each operation, writes the contours of the result, the contours with an area
        /// below 0.000001 mm2, the error of the area, and the samples of paper that show between the pieces at 40 and
        /// at 400 samples for each millimetre.
        /// </summary>
        private static void MeasureSharedEdges(string resultsFolder)
        {
            Func<double, double, double> none = (first, second) => 0;
            Func<double, double, double> allOfFirst = (first, second) => first;
            // The two shapes together cover the rectangle of 20 mm by 10 mm, so the overlap is what the areas have too much.
            double rectangleArea = PathMeasures.ComputeArea(MakePolygon(0, 0, 20, 0, 20, 10, 0, 10));
            Func<double, double, double> overlapInRectangle = (first, second) => first + second - rectangleArea;

            List<ShapePair> pairs = new List<ShapePair> {
                new ShapePair { Name = "rectangles, one whole edge", First = MakePolygon(0, 0, 10, 0, 10, 10, 0, 10), Second = MakePolygon(10, 0, 20, 0, 20, 10, 10, 10), ExactIntersectionArea = none },
                new ShapePair { Name = "rectangles, a part of an edge", First = MakePolygon(0, 0, 10, 0, 10, 10, 0, 10), Second = MakePolygon(10, 3, 20, 3, 20, 7, 10, 7), ExactIntersectionArea = none },
                new ShapePair { Name = "slanted edge, one side has one vertex more", First = MakePolygon(0, 0, 10, 0, 13, 10, 0, 10), Second = MakePolygon(10, 0, 20, 0, 20, 10, 13, 10, 11.11F, 3.7F), ExactIntersectionArea = none },
                new ShapePair { Name = "cubic curve, same direction", First = MakeLeftOfCurve(), Second = MakeRightOfCurve(true, 0), ExactIntersectionArea = none },
                new ShapePair { Name = "cubic curve, opposite direction", First = MakeLeftOfCurve(), Second = MakeRightOfCurve(false, 0), ExactIntersectionArea = none },
                new ShapePair { Name = "a shape and its copy", First = MakeCircleOfCubics(5, 5, 5), Second = MakeCircleOfCubics(5, 5, 5), ExactIntersectionArea = allOfFirst },
                new ShapePair { Name = "straight edges 0.0001 mm apart, a gap", First = MakePolygon(0, 0, 10, 0, 10, 10, 0, 10), Second = MakePolygon(10.0001F, 0, 20, 0, 20, 10, 10.0001F, 10), ExactIntersectionArea = none },
                new ShapePair { Name = "straight edges 0.0001 mm apart, an overlap", First = MakePolygon(0, 0, 10, 0, 10, 10, 0, 10), Second = MakePolygon(9.9999F, 0, 20, 0, 20, 10, 9.9999F, 10), ExactIntersectionArea = overlapInRectangle },
                new ShapePair { Name = "cubic curves 0.0001 mm apart, a gap", First = MakeLeftOfCurve(), Second = MakeRightOfCurve(false, 0.0001F), ExactIntersectionArea = none },
                new ShapePair { Name = "cubic curves 0.0001 mm apart, an overlap", First = MakeLeftOfCurve(), Second = MakeRightOfCurve(false, -0.0001F), ExactIntersectionArea = overlapInRectangle },
            };

            TsvTable table = new TsvTable(Path.Combine(resultsFolder, "q4-shared-edges.tsv"), "shapes", "operation", "gave_result", "contours", "contours_below_0.000001_mm2", "segments_lines/quadratics/conics/cubics",
                "area_mm2", "exact_area_mm2", "area_error_mm2", "paper_samples_at_40", "extra_samples_at_40", "paper_samples_at_400", "extra_samples_at_400");
            foreach (ShapePair pair in pairs) {
                double firstArea = PathMeasures.ComputeArea(pair.First), secondArea = PathMeasures.ComputeArea(pair.Second);
                double intersectionArea = pair.ExactIntersectionArea(firstArea, secondArea);
                SKPath union = pair.First.Op(pair.Second, SKPathOp.Union), difference = pair.First.Op(pair.Second, SKPathOp.Difference);
                SKPath intersection = pair.First.Op(pair.Second, SKPathOp.Intersect), reverseDifference = pair.Second.Op(pair.First, SKPathOp.Difference);
                SKPath[] marks = new SKPath[] { pair.First, pair.Second };

                AddSharedEdgeRow(table, pair.Name, "union", union, firstArea + secondArea - intersectionArea, marks, new SKPath[] { union });
                AddSharedEdgeRow(table, pair.Name, "difference", difference, firstArea - intersectionArea, marks, new SKPath[] { difference, pair.Second });
                AddSharedEdgeRow(table, pair.Name, "intersection", intersection, intersectionArea, marks, new SKPath[] { difference, intersection, reverseDifference });
            }
        }

        /// <summary>Adds the row of one operation on one pair of shapes. pieces are the paths that are drawn in place of the two shapes.</summary>
        private static void AddSharedEdgeRow(TsvTable table, string shapes, string operation, SKPath result, double exactArea, SKPath[] marks, SKPath[] pieces)
        {
            if (result == null) {
                table.AddRow(shapes, operation, false, "", "", "", "", exactArea.ToString("0.#######"), "", "", "", "", "");
                return;
            }
            List<double> contourAreas = PathMeasures.ComputeContourAreas(result);
            double area = ComputeAreaOfResult(result);
            SKRect window = SKRect.Create(BaseX - 0.5F, BaseY - 0.5F, 21, 11);
            long[] paper = new long[2], extra = new long[2];
            int[] resolutions = new int[] { 40, 400 };
            for (int i = 0; i < resolutions.Length; ++i) {
                foreach (SKRect part in SampleGrid.DivideWindow(window, resolutions[i])) {
                    using (SKBitmap ofMarks = SampleGrid.DrawCoverage(part, resolutions[i], marks))
                    using (SKBitmap ofPieces = SampleGrid.DrawCoverage(part, resolutions[i], pieces.Where(piece => piece != null).ToList())) {
                        SampleGrid.CompareSamples(ofMarks, ofPieces, out long differing, out long onlyInMarks);
                        paper[i] += onlyInMarks;
                        extra[i] += differing - onlyInMarks;
                    }
                }
            }
            table.AddRow(shapes, operation, true, contourAreas.Count, contourAreas.Count(contourArea => Math.Abs(contourArea) < 0.000001), PathMeasures.CountSegments(result).KindsText,
                         area.ToString("0.#######"), exactArea.ToString("0.#######"), (area - exactArea).ToString("0.#######"), paper[0], extra[0], paper[1], extra[1]);
        }

        /// <summary>
        /// Gives the area that a result of a path operation covers. The contours of such a result do not cross, so the
        /// area is the sum of the signed areas when each hole goes around in the direction opposite to its outline.
        /// SKPath.ToWinding gives that form.
        /// </summary>
        private static double ComputeAreaOfResult(SKPath result)
        {
            using (SKPath winding = result.ToWinding())
                return PathMeasures.ComputeArea(winding ?? result);
        }

        // ---- Question 7: curves.

        /// <summary>
        /// For shapes of cubic curves, writes the kinds of the segments that go in and that come out, and the largest
        /// distance from the outline that comes out to the outline that is exact, at 1000 points or more of each contour.
        /// </summary>
        private static void MeasureCurves(string resultsFolder)
        {
            TsvTable table = new TsvTable(Path.Combine(resultsFolder, "q7-curves.tsv"), "shapes", "operation", "in_lines/quadratics/conics/cubics", "gave_result", "out_contours", "out_lines/quadratics/conics/cubics", "largest_distance_mm");

            SKPath firstCircle = MakeCircleOfCubics(5, 5, 5), secondCircle = MakeCircleOfCubics(11, 5, 5);
            foreach (SKPathOp operation in new SKPathOp[] { SKPathOp.Union, SKPathOp.Intersect, SKPathOp.Difference })
                AddCurveRow(table, "two circles of four cubics each, which cross", operation.ToString(), new SKPath[] { firstCircle, secondCircle }, firstCircle.Op(secondCircle, operation));

            SKPath left = MakeLeftOfCurve(), right = MakeRightOfCurve(false, 0);
            AddCurveRow(table, "two shapes that share a cubic curve", "Union", new SKPath[] { left, right }, left.Op(right, SKPathOp.Union));
            AddCurveRow(table, "two shapes that share a cubic curve", "Difference", new SKPath[] { left, right }, left.Op(right, SKPathOp.Difference));

            SKPath loop = new SKPath();
            loop.MoveTo(BaseX, BaseY);
            loop.CubicTo(BaseX + 15, BaseY + 10, BaseX - 5, BaseY + 10, BaseX + 10, BaseY);
            loop.Close();
            AddCurveRow(table, "one cubic that crosses itself, closed by a line", "Simplify", new SKPath[] { loop }, loop.Simplify());

            SKPath smallCircle = MakeCircleOfCubics(5, 5, 0.25F), rectangle = MakePolygon(5, 4, 6, 4, 6, 6, 5, 6);
            AddCurveRow(table, "a circle of 0.5 mm of four cubics, cut by a rectangle", "Intersect", new SKPath[] { smallCircle, rectangle }, smallCircle.Op(rectangle, SKPathOp.Intersect));

            // The outline of a stroked cubic, which SKPaint.GetFillPath makes for a resolution scale. Each point of
            // the exact outline is at half the width from the centre line, because the caps and the joins are round.
            SKPath centreLine = new SKPath();
            centreLine.MoveTo(BaseX, BaseY);
            centreLine.CubicTo(BaseX + 4, BaseY + 3, BaseX - 4, BaseY + 7, BaseX, BaseY + 10);
            const float width = 0.35F;
            foreach (float resolutionScale in new float[] { 1, 40, 400 }) {
                using (SKPaint paint = new SKPaint()) {
                    paint.IsStroke = true;
                    paint.StrokeWidth = width;
                    paint.StrokeCap = SKStrokeCap.Round;
                    paint.StrokeJoin = SKStrokeJoin.Round;
                    SKPath outline = new SKPath();
                    bool hasOutline = paint.GetFillPath(centreLine, outline, resolutionScale);
                    SegmentCounts counts = PathMeasures.CountSegments(outline);

                    // The outline that GetFillPath gives can cross itself. Simplify gives the edge of the area that it fills.
                    SKPath edge = outline.Simplify();
                    double largest = 0;
                    List<double[]> exact = FlattenOutlines(new SKPath[] { centreLine }, false);
                    foreach (double[] point in SamplePointsOfContours(edge ?? outline))
                        largest = Math.Max(largest, Math.Abs(DistanceToOutlines(point[0], point[1], exact) - width / 2.0));
                    table.AddRow("a stroked cubic of 0.35 mm with round caps", "GetFillPath at resolution scale " + resolutionScale + ", then Simplify", PathMeasures.CountSegments(centreLine).KindsText, hasOutline && edge != null,
                                 PathMeasures.CountSegments(edge).Contours, counts.KindsText + " (after Simplify: " + PathMeasures.CountSegments(edge).KindsText + ")", largest.ToString("0.#######"));
                }
            }
        }

        /// <summary>Adds the row of one result: its kinds of segments, and its largest distance from the outlines of the shapes that went in.</summary>
        private static void AddCurveRow(TsvTable table, string shapes, string operation, SKPath[] inputs, SKPath result)
        {
            string kindsIn = PathMeasures.CountSegments(inputs).KindsText;
            if (result == null) {
                table.AddRow(shapes, operation, kindsIn, false, "", "", "");
                return;
            }
            SegmentCounts counts = PathMeasures.CountSegments(result);
            List<double[]> exact = FlattenOutlines(inputs, true);
            double largest = 0;
            foreach (double[] point in SamplePointsOfContours(result))
                largest = Math.Max(largest, DistanceToOutlines(point[0], point[1], exact));
            table.AddRow(shapes, operation, kindsIn, true, counts.Contours, counts.KindsText, largest.ToString("0.#######"));
        }

        /// <summary>
        /// Calls an action for each segment of a path, with the points of the segment and its conic weight. When
        /// isFilled is set, the line that closes each contour is a segment too, as it is for a filled path.
        /// </summary>
        private static void VisitSegments(SKPath path, bool isFilled, Action<SKPathVerb, SKPoint[], float> visit)
        {
            SKPoint[] points = new SKPoint[4];
            SKPoint start = SKPoint.Empty, last = SKPoint.Empty;
            using (SKPath.RawIterator iterator = path.CreateRawIterator()) {
                SKPathVerb verb;
                while ((verb = iterator.Next(points)) != SKPathVerb.Done) {
                    if (verb == SKPathVerb.Move) {
                        if (isFilled && last != start)
                            visit(SKPathVerb.Line, new SKPoint[] { last, start }, 1);
                        start = last = points[0];
                    }
                    else if (verb == SKPathVerb.Close) {
                        if (last != start)
                            visit(SKPathVerb.Line, new SKPoint[] { last, start }, 1);
                        last = start;
                    }
                    else {
                        visit(verb, (SKPoint[])points.Clone(), verb == SKPathVerb.Conic ? iterator.ConicWeight() : 1);
                        last = points[verb == SKPathVerb.Line ? 1 : verb == SKPathVerb.Cubic ? 3 : 2];
                    }
                }
            }
            if (isFilled && last != start)
                visit(SKPathVerb.Line, new SKPoint[] { last, start }, 1);
        }

        /// <summary>
        /// Gives the outlines of paths as short lines in double precision: 8192 lines for each curve. isFilled says
        /// if the paths are filled shapes, whose contours are closed, or lines.
        /// </summary>
        private static List<double[]> FlattenOutlines(IEnumerable<SKPath> paths, bool isFilled)
        {
            List<double[]> lines = new List<double[]>();
            foreach (SKPath path in paths) {
                VisitSegments(path, isFilled, (verb, points, weight) => {
                    int steps = verb == SKPathVerb.Line ? 1 : 8192;
                    PathMeasures.EvaluateSegment(verb, points, weight, 0, out double previousX, out double previousY);
                    for (int i = 1; i <= steps; ++i) {
                        PathMeasures.EvaluateSegment(verb, points, weight, (double)i / steps, out double x, out double y);
                        lines.Add(new double[] { previousX, previousY, x, y });
                        previousX = x;
                        previousY = y;
                    }
                });
            }
            return lines;
        }

        /// <summary>Gives points of the contours of a path: 1000 or more for each contour, the same count on each segment.</summary>
        private static List<double[]> SamplePointsOfContours(SKPath path)
        {
            List<double[]> samples = new List<double[]>();
            SegmentCounts counts = PathMeasures.CountSegments(path);
            if (counts.Contours == 0)
                return samples;
            int pointsOnSegment = (int)Math.Ceiling(1000.0 * counts.Contours / Math.Max(1, counts.Segments)) + 1;
            VisitSegments(path, true, (verb, points, weight) => {
                for (int i = 0; i < pointsOnSegment; ++i) {
                    PathMeasures.EvaluateSegment(verb, points, weight, (i + 0.5) / pointsOnSegment, out double x, out double y);
                    samples.Add(new double[] { x, y });
                }
            });
            return samples;
        }

        /// <summary>Gives the distance from a point to the nearest of a list of lines.</summary>
        private static double DistanceToOutlines(double x, double y, List<double[]> lines)
        {
            double smallestSquare = double.MaxValue;
            foreach (double[] line in lines) {
                double dx = line[2] - line[0], dy = line[3] - line[1];
                double lengthSquare = dx * dx + dy * dy;
                double t = lengthSquare == 0 ? 0 : Math.Clamp(((x - line[0]) * dx + (y - line[1]) * dy) / lengthSquare, 0, 1);
                double nearX = line[0] + t * dx - x, nearY = line[1] + t * dy - y;
                smallestSquare = Math.Min(smallestSquare, nearX * nearX + nearY * nearY);
            }
            return Math.Sqrt(smallestSquare);
        }
    }
}
