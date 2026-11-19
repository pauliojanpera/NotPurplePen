using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using PurplePen.Graphics2D;
using PurplePen.MapModel;

namespace Map_Skia.Tests
{
    // The comparison of two lists of layer instances as structures, with which the tests check the geometry methods of
    // the symbols against their oracle, the instances that the collector makes (Map.CollectLayerInstances). Two lists
    // have the same structure when they have the same palette entries and prototypes, with the same transformations, in
    // the same order, and each instance has the same clipping objects in the same order: the same kind, shape, line or
    // fill rule, and steps of the placement. The collector makes a shape of each drawing call, so the comparison compares
    // the shapes as values, each number exactly. It compares a matrix by its elements, as Matrix.Elements gives them.
    // A dashed stroke of the instances to check, a run of a composed dashed line, is first expanded to its dashes
    // (StrokedShape.ExpandDashes). The run measures its dashes from its own start, and not from the start of the line as
    // the drawing does, so the ends of its dashes are compared to within DashTolerance, and points nearer each other
    // than that count as one.
    static class LayerInstanceComparison
    {
        // The distance in mm within which the ends of the expanded dashes of a run agree with the drawing.
        public const float DashTolerance = 0.0001F;

        // Find the differences between the structures of two lists of instances.
        // expected: the instances of the oracle.
        // actual: the instances to check.
        public static List<string> FindDifferences(IReadOnlyList<LayerInstance> expected, IReadOnlyList<LayerInstance> actual)
        {
            List<string> differences = new List<string>();
            if (actual.Count != expected.Count)
                differences.Add(string.Format("{0} instances, expected {1}", actual.Count, expected.Count));

            for (int i = 0; i < Math.Min(expected.Count, actual.Count); ++i) {
                LayerInstance expectedInstance = expected[i], actualInstance = actual[i];
                if (actualInstance.PaletteEntry != expectedInstance.PaletteEntry || actualInstance.Prototype != expectedInstance.Prototype ||
                    Describe(actualInstance.Transformation) != Describe(expectedInstance.Transformation))
                {
                    differences.Add(string.Format("instance {0}: {1}, {2}, expected {3}, {4}", i, actualInstance, Describe(actualInstance.Transformation), expectedInstance, Describe(expectedInstance.Transformation)));
                    continue;
                }

                IReadOnlyList<ClippingObject> expectedObjects = expectedInstance.ClippingObjects;
                List<ClippingObject> actualObjects = new List<ClippingObject>();
                List<bool> isExpandedDash = new List<bool>();
                foreach (ClippingObject clippingObject in actualInstance.ClippingObjects) {
                    StrokedShape stroked = clippingObject as StrokedShape;
                    if (stroked != null && stroked.Stroke.DashPattern != null) {
                        foreach (StrokedShape dash in stroked.ExpandDashes()) {
                            actualObjects.Add(dash);
                            isExpandedDash.Add(true);
                        }
                    }
                    else {
                        actualObjects.Add(clippingObject);
                        isExpandedDash.Add(false);
                    }
                }

                for (int j = 0; j < Math.Max(expectedObjects.Count, actualObjects.Count); ++j) {
                    string expectedObject = (j < expectedObjects.Count) ? Describe(expectedObjects[j]) : "none";
                    string actualObject = (j < actualObjects.Count) ? Describe(actualObjects[j]) : "none";
                    bool isSame = (j < expectedObjects.Count && j < actualObjects.Count && isExpandedDash[j])
                        ? AreSameDashes((StrokedShape)actualObjects[j], expectedObjects[j] as StrokedShape)
                        : actualObject == expectedObject;
                    if (!isSame)
                        differences.Add(string.Format("{0} (id {1}), clipping object {2}: {3}, expected {4}", expectedInstance.PaletteEntry.Name, expectedInstance.PaletteEntry.OcadId, j, actualObject, expectedObject));
                }
            }

            return differences;
        }

        // The description of a clipping object that the comparison compares: its kind, its shape, its line or fill rule,
        // and the steps of its placement, each number exactly.
        // clippingObject: the clipping object.
        public static string Describe(ClippingObject clippingObject)
        {
            string placement = string.Join(" ", clippingObject.Placement.Steps.Select(Describe));
            if (clippingObject is FilledShape filled)
                return string.Format("filled {0}, {1}, at [{2}]", Describe(filled.Shape), filled.FillRule, placement);

            StrokedShape stroked = (StrokedShape)clippingObject;
            Stroke stroke = stroked.Stroke;
            string dashes = (stroke.DashPattern == null) ? "" : string.Format(", dashes {0} from {1}", string.Join(" ", stroke.DashPattern.Select(Describe)), Describe(stroke.DashPhase));
            return string.Format("stroked {0}, width {1}, {2} caps, {3} joins, miter limit {4}{5}, at [{6}]",
                                 Describe(stroked.Shape), Describe(stroke.Width), stroke.Caps, stroke.Join, Describe(stroke.MiterLimit), dashes, placement);
        }

        // Is an expanded dash of a run the same as a dash that the drawing draws: the same line and placement, and a
        // polyline whose points agree to within DashTolerance, points nearer each other than that counting as one?
        // dash: the expanded dash.
        // drawn: the clipping object of the drawing, or null.
        static bool AreSameDashes(StrokedShape dash, StrokedShape drawn)
        {
            if (drawn == null)
                return false;
            string line = Describe(dash).Substring(Describe(dash).IndexOf(", width ", StringComparison.Ordinal));
            string drawnLine = Describe(drawn).Substring(Describe(drawn).IndexOf(", width ", StringComparison.Ordinal));
            if (line != drawnLine)
                return false;

            List<PointF> points = DistinctPoints((PathShape)dash.Shape), drawnPoints = DistinctPoints(drawn.Shape as PathShape);
            if (drawnPoints == null || points.Count != drawnPoints.Count)
                return false;
            for (int i = 0; i < points.Count; ++i) {
                if (Math.Abs(points[i].X - drawnPoints[i].X) > DashTolerance || Math.Abs(points[i].Y - drawnPoints[i].Y) > DashTolerance)
                    return false;
            }
            return true;
        }

        // The points of an open polyline, without a point nearer to the one before it than DashTolerance, or null for a
        // shape that is no open polyline.
        static List<PointF> DistinctPoints(PathShape path)
        {
            if (path == null || path.Parts.Count != 2 || path.Parts[0].Kind != GraphicsPathPartKind.Start || path.Parts[1].Kind != GraphicsPathPartKind.Lines)
                return null;
            List<PointF> points = new List<PointF>();
            foreach (PointF point in path.Parts[0].Points.Concat(path.Parts[1].Points)) {
                if (points.Count == 0 || Math.Abs(point.X - points[points.Count - 1].X) > DashTolerance || Math.Abs(point.Y - points[points.Count - 1].Y) > DashTolerance)
                    points.Add(point);
            }
            return points;
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
