using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using PurplePen.Graphics2D;
using PurplePen.MapModel;

namespace Map_Skia.Tests
{
    // The comparison of the clipping objects of palette color coverages of symbols as structures, with which the tests
    // check the geometry methods of the symbol definitions (SymDef.MakeClippingObjects) against their oracle, the
    // clipping objects that the collector makes (SymDef.CollectClippingObjects). Two lists of clipping objects have the
    // same structure when they have the same clipping objects in the same order: the same kind, shape, pen or fill rule,
    // and steps of the placement. The collector makes a shape of each drawing call, so the comparison compares the
    // shapes as values, each number exactly. It compares a matrix by its elements, as Matrix.Elements gives them.
    static class SymbolCoverageComparison
    {
        // Find the differences between the structures of two lists of the clipping objects of a palette color coverage.
        // coverage: the palette color coverage, which names the differences.
        // expected: the clipping objects of the oracle.
        // actual: the clipping objects to check.
        // The geometry methods of the lines make forms that the collector does not: runs of dashes, offset paths and
        // repeated placements. The comparison expands them first (Expand), and compares each expanded clipping object in
        // the coordinates of the map within ExpansionTolerance: a run measures its dashes from its own start, and the
        // drawing from the start of the line, and a repeated placement finds its poses along its own path, and the drawing
        // along the whole line. Points that are nearer each other than the tolerance are one point. The other clipping
        // objects are compared exactly.
        public static List<string> FindDifferences(SymbolCoverage coverage, IReadOnlyList<ClippingObject> expected, IReadOnlyList<ClippingObject> actual)
        {
            List<(ClippingObject clippingObject, bool isExpanded)> expanded = Expand(actual);
            List<string> differences = new List<string>();
            for (int i = 0; i < Math.Max(expected.Count, expanded.Count); ++i) {
                string expectedObject, actualObject;
                if (i < expanded.Count && expanded[i].isExpanded) {
                    expectedObject = (i < expected.Count) ? DescribeInMap(expected[i]) : "none";
                    actualObject = DescribeInMap(expanded[i].clippingObject);
                    if (i < expected.Count && AreSameInMap(expected[i], expanded[i].clippingObject))
                        continue;
                }
                else {
                    expectedObject = (i < expected.Count) ? Describe(expected[i]) : "none";
                    actualObject = (i < expanded.Count) ? Describe(expanded[i].clippingObject) : "none";
                    if (actualObject == expectedObject)
                        continue;
                }
                differences.Add(string.Format("{0} (id {1}), {2}, clipping object {3}: {4}, expected {5}", coverage.Color.Name, coverage.Color.OcadId, coverage.Symbol.Definition.Name, i, actualObject, expectedObject));
            }

            return differences;
        }

        // The distance in mm within which an expanded clipping object agrees with the one of the collector.
        public const float ExpansionTolerance = 0.0001F;

        // The expansion of clipping objects into the clipping objects that the drawing makes: a run of dashes into single
        // dashes (StrokedShape.ExpandDashes), an offset path into its offset curve, and the clipping objects that share a
        // repeated placement into the clipping objects at each pose, pose by pose, each in the order of the parts, as the
        // drawing draws a glyph at a time. Each item tells whether an expansion made it.
        // clippingObjects: the clipping objects.
        public static List<(ClippingObject clippingObject, bool isExpanded)> Expand(IReadOnlyList<ClippingObject> clippingObjects)
        {
            List<(ClippingObject, bool)> expanded = new List<(ClippingObject, bool)>();
            int i = 0;
            while (i < clippingObjects.Count) {
                if (clippingObjects[i].Placement is RepeatedPlacement repeated) {
                    int last = i;
                    while (last + 1 < clippingObjects.Count && clippingObjects[last + 1].Placement == repeated)
                        ++last;
                    foreach (SinglePlacement placement in repeated.Expand()) {
                        for (int k = i; k <= last; ++k) {
                            foreach (ClippingObject part in ExpandShape(clippingObjects[k].WithPlacement(placement)))
                                expanded.Add((part, true));
                        }
                    }
                    i = last + 1;
                }
                else {
                    ClippingObject clippingObject = clippingObjects[i];
                    StrokedShape stroked = clippingObject as StrokedShape;
                    bool isExpanded = stroked != null && (stroked.Stroke.DashPattern != null || stroked.Shape is OffsetPathShape);
                    foreach (ClippingObject part in ExpandShape(clippingObject))
                        expanded.Add((part, isExpanded));
                    ++i;
                }
            }

            return expanded;
        }

        // The expansion of the shape of a clipping object: the dashes of a run, or the offset curve of an offset path, or
        // the clipping object itself.
        static IEnumerable<ClippingObject> ExpandShape(ClippingObject clippingObject)
        {
            StrokedShape stroked = clippingObject as StrokedShape;
            if (stroked != null) {
                if (stroked.Stroke.DashPattern != null)
                    return stroked.ExpandDashes();
                if (stroked.Shape is OffsetPathShape offsetPath)
                    return new ClippingObject[] { new StrokedShape(offsetPath.OffsetCurve, stroked.Stroke, stroked.Placement) };
            }
            return new ClippingObject[] { clippingObject };
        }

        // Are two clipping objects the same in the coordinates of the map, within ExpansionTolerance: of the same kind,
        // with the same pen or fill rule, and with their shapes at the same points?
        static bool AreSameInMap(ClippingObject expected, ClippingObject actual)
        {
            if (expected.GetType() != actual.GetType())
                return false;
            if (expected is FilledShape filled) {
                if (filled.FillRule != ((FilledShape)actual).FillRule)
                    return false;
            }
            else if (DescribeStroke(((StrokedShape)expected).Stroke) != DescribeStroke(((StrokedShape)actual).Stroke)) {
                return false;
            }

            List<PointF> expectedPoints = PointsInMap(expected), actualPoints = PointsInMap(actual);
            if (expectedPoints == null || actualPoints == null || expectedPoints.Count != actualPoints.Count)
                return false;
            for (int i = 0; i < expectedPoints.Count; ++i) {
                if (Distance(expectedPoints[i], actualPoints[i]) > ExpansionTolerance)
                    return false;
            }
            return true;
        }

        // The points of the shape of a clipping object in the coordinates of the map, without the points that are nearer
        // the point before them than ExpansionTolerance: the points of a path, or the center of an ellipse and the points
        // at its radii, or null for another shape.
        static List<PointF> PointsInMap(ClippingObject clippingObject)
        {
            List<PointF> points = new List<PointF>();
            if (clippingObject.Shape is PathShape path) {
                foreach (GraphicsPathPart part in path.Parts) {
                    if (part.Points != null)
                        points.AddRange(part.Points);
                }
            }
            else if (clippingObject.Shape is EllipseShape ellipse) {
                points.Add(ellipse.Center);
                points.Add(new PointF(ellipse.Center.X + ellipse.RadiusX, ellipse.Center.Y));
                points.Add(new PointF(ellipse.Center.X, ellipse.Center.Y + ellipse.RadiusY));
            }
            else {
                return null;
            }

            PointF[] placed = points.ToArray();
            ((SinglePlacement)clippingObject.Placement).Matrix.TransformPoints(placed);
            List<PointF> kept = new List<PointF>();
            foreach (PointF point in placed) {
                if (kept.Count == 0 || Distance(kept[kept.Count - 1], point) >= ExpansionTolerance)
                    kept.Add(point);
            }
            return kept;
        }

        // The description of a clipping object in the coordinates of the map, for the message of a difference.
        static string DescribeInMap(ClippingObject clippingObject)
        {
            List<PointF> points = PointsInMap(clippingObject);
            string pen = (clippingObject is StrokedShape stroked) ? DescribeStroke(stroked.Stroke) : ((FilledShape)clippingObject).FillRule.ToString();
            return string.Format("{0} in the map at {1}, {2}", clippingObject.GetType().Name, (points == null) ? clippingObject.Shape.ToString() : string.Join(", ", points.Select(Describe)), pen);
        }

        static string DescribeStroke(Stroke stroke)
        {
            return string.Format("width {0}, {1} caps, {2} joins, miter limit {3}", Describe(stroke.Width), stroke.Caps, stroke.Join, Describe(stroke.MiterLimit));
        }

        static float Distance(PointF a, PointF b)
        {
            return (float)Math.Sqrt((a.X - b.X) * (a.X - b.X) + (a.Y - b.Y) * (a.Y - b.Y));
        }

        // The description of a clipping object that the comparison compares: its kind, its shape, its pen or fill rule,
        // and the steps of its placement, each number exactly.
        // clippingObject: the clipping object.
        public static string Describe(ClippingObject clippingObject)
        {
            string placement = string.Join(" ", clippingObject.Placement.Steps.Select(Describe));
            if (clippingObject is FilledShape filled)
                return string.Format("filled {0}, {1}, at [{2}]", Describe(filled.Shape), filled.FillRule, placement);

            StrokedShape stroked = (StrokedShape)clippingObject;
            Stroke stroke = stroked.Stroke;
            return string.Format("stroked {0}, width {1}, {2} caps, {3} joins, miter limit {4}, at [{5}]",
                                 Describe(stroked.Shape), Describe(stroke.Width), stroke.Caps, stroke.Join, Describe(stroke.MiterLimit), placement);
        }

        // The description of a shape: its kind and the values that make it.
        // shape: the shape.
        static string Describe(Shape shape)
        {
            if (shape is PathShape path)
                return "path " + string.Join(" ", path.Parts.Select(part => part.Kind + ((part.Points == null) ? "" : "(" + string.Join(", ", part.Points.Select(Describe)) + ")")));
            if (shape is EllipseShape ellipse)
                return string.Format("ellipse {0}, radii {1} {2}", Describe(ellipse.Center), Describe(ellipse.RadiusX), Describe(ellipse.RadiusY));
            if (shape is ArcShape arc)
                return string.Format("arc {0}, radius {1}, from {2} by {3}", Describe(arc.Center), Describe(arc.Radius), Describe(arc.StartAngle), Describe(arc.SweepAngle));

            TextShape text = (TextShape)shape;
            return string.Format("text \"{0}\" in {1} {2} {3} at {4}", text.Text, text.FontFamily, Describe(text.EmHeight), text.Effects, Describe(text.UpperLeft));
        }

        static string Describe(Matrix matrix)
        {
            return "(" + string.Join(" ", matrix.Elements.Select(Describe)) + ")";
        }

        static string Describe(PointF point)
        {
            return Describe(point.X) + " " + Describe(point.Y);
        }

        static string Describe(float value)
        {
            return value.ToString("R", CultureInfo.InvariantCulture);
        }
    }
}
