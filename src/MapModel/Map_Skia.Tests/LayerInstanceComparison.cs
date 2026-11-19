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
    // the drawing does, so the ends of its dashes are compared to within ExpansionTolerance, and points nearer each
    // other than that count as one. A repetition of shapes (RepeatedShapes) is first expanded to its placements. It
    // finds its placements along its own path, and not along the line as the drawing does, so each placed shape is
    // compared in the coordinates of the map, its points to within ExpansionTolerance.
    // The drawing strokes the outline of a text run with a line framing with pens of increasing width, and last with the
    // pen of the full width (TextSymDef.DrawStringWithEffects). The narrower lines are inside the widest, and the
    // framing line of the instances is that one line, as OCAD draws it (question 2 of ticket 013). So of consecutive
    // strokes of the same text run at the same placement, the comparison keeps the last.
    static class LayerInstanceComparison
    {
        // The distance in mm within which the expanded dashes of a run and the expanded placements of a repetition agree
        // with the drawing.
        public const float ExpansionTolerance = 0.0001F;

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

                List<ClippingObject> expectedObjects = LeaveOutNarrowerPensOfFramings(expectedInstance.ClippingObjects);
                List<ClippingObject> actualObjects = new List<ClippingObject>();
                List<Expansion> expansions = new List<Expansion>();
                foreach (ClippingObject clippingObject in actualInstance.ClippingObjects) {
                    StrokedShape stroked = clippingObject as StrokedShape;
                    RepeatedShapes repeated = clippingObject as RepeatedShapes;
                    if (stroked != null && stroked.Stroke.DashPattern != null) {
                        foreach (StrokedShape dash in stroked.ExpandDashes()) {
                            actualObjects.Add(dash);
                            expansions.Add(Expansion.Dash);
                        }
                    }
                    else if (repeated != null) {
                        foreach (ClippingObject placed in repeated.Expand()) {
                            actualObjects.Add(placed);
                            expansions.Add(Expansion.Placement);
                        }
                    }
                    else {
                        actualObjects.Add(clippingObject);
                        expansions.Add(Expansion.None);
                    }
                }

                for (int j = 0; j < Math.Max(expectedObjects.Count, actualObjects.Count); ++j) {
                    string expectedObject = (j < expectedObjects.Count) ? Describe(expectedObjects[j]) : "none";
                    string actualObject = (j < actualObjects.Count) ? Describe(actualObjects[j]) : "none";
                    bool isSame;
                    if (j >= expectedObjects.Count || j >= actualObjects.Count || expansions[j] == Expansion.None)
                        isSame = actualObject == expectedObject;
                    else if (expansions[j] == Expansion.Dash)
                        isSame = AreSameDashes((StrokedShape)actualObjects[j], expectedObjects[j] as StrokedShape);
                    else
                        isSame = AreSameInPlace(actualObjects[j], expectedObjects[j]);
                    if (!isSame)
                        differences.Add(string.Format("{0} (id {1}), clipping object {2}: {3}, expected {4}", expectedInstance.PaletteEntry.Name, expectedInstance.PaletteEntry.OcadId, j, actualObject, expectedObject));
                }
            }

            return differences;
        }

        // How an object to check comes from an object of the instances: as it is, as a dash of a run, or as a placed
        // shape of a repetition.
        enum Expansion { None, Dash, Placement }

        // The clipping objects of the oracle without the narrower pens of the line framings of texts: of consecutive
        // strokes of the same text run at the same placement, the last.
        // clippingObjects: the clipping objects of an instance of the oracle.
        static List<ClippingObject> LeaveOutNarrowerPensOfFramings(IReadOnlyList<ClippingObject> clippingObjects)
        {
            List<ClippingObject> kept = new List<ClippingObject>();
            for (int i = 0; i < clippingObjects.Count; ++i) {
                if (i + 1 < clippingObjects.Count && AreStrokesOfTheSameTextRun(clippingObjects[i], clippingObjects[i + 1]))
                    continue;
                kept.Add(clippingObjects[i]);
            }
            return kept;
        }

        // Are two clipping objects strokes of the same text run at the same placement, whatever their lines?
        static bool AreStrokesOfTheSameTextRun(ClippingObject first, ClippingObject second)
        {
            return first is StrokedShape && second is StrokedShape && first.Shape is TextShape && second.Shape is TextShape &&
                   Describe(first.Shape) == Describe(second.Shape) && first.Placement.Steps.Select(Describe).SequenceEqual(second.Placement.Steps.Select(Describe));
        }

        // The description of a clipping object that the comparison compares: its kind, its shape, its line or fill rule,
        // and the steps of its placement, each number exactly.
        // clippingObject: the clipping object.
        public static string Describe(ClippingObject clippingObject)
        {
            string placement = string.Join(" ", clippingObject.Placement.Steps.Select(Describe));
            if (clippingObject is FilledShape filled)
                return string.Format("filled {0}, {1}, at [{2}]", Describe(filled.Shape), filled.FillRule, placement);
            if (clippingObject is RepeatedShapes repeated)
                return string.Format("repeated {0} {1} times by {2}{3} along {4}, at [{5}]", string.Join("; ", repeated.Elements.Select(Describe)),
                                     repeated.Count, Describe(repeated.Step), repeated.IsEuclidean ? "" : " of the OCAD metric", Describe(repeated.Shape), placement);

            StrokedShape stroked = (StrokedShape)clippingObject;
            return string.Format("stroked {0}, {1}, at [{2}]", Describe(stroked.Shape), Describe(stroked.Stroke), placement);
        }

        // The description of a line: its width, caps, joins and miter limit, and its dashes.
        static string Describe(Stroke stroke)
        {
            string dashes = (stroke.DashPattern == null) ? "" : string.Format(", dashes {0} from {1}", string.Join(" ", stroke.DashPattern.Select(Describe)), Describe(stroke.DashPhase));
            return string.Format("width {0}, {1} caps, {2} joins, miter limit {3}{4}", Describe(stroke.Width), stroke.Caps, stroke.Join, Describe(stroke.MiterLimit), dashes);
        }

        // Is a placed shape of a repetition the same as a clipping object of the drawing: the same kind, line or fill rule
        // and kind of shape, a circle of the same radius, and points that agree in the coordinates of the map to within
        // ExpansionTolerance? A glyph of one circle at its origin is drawn at its location without a placement, so only
        // the coordinates of the map compare it.
        // placed: the placed shape.
        // drawn: the clipping object of the drawing.
        static bool AreSameInPlace(ClippingObject placed, ClippingObject drawn)
        {
            if (placed.GetType() != drawn.GetType() || placed.Shape.GetType() != drawn.Shape.GetType())
                return false;
            if (placed is FilledShape filled && filled.FillRule != ((FilledShape)drawn).FillRule)
                return false;
            if (placed is StrokedShape stroked && Describe(stroked.Stroke) != Describe(((StrokedShape)drawn).Stroke))
                return false;

            if (placed.Shape is EllipseShape ellipse) {
                EllipseShape drawnEllipse = (EllipseShape)drawn.Shape;
                if (ellipse.RadiusX != ellipse.RadiusY || ellipse.RadiusX != drawnEllipse.RadiusX || ellipse.RadiusY != drawnEllipse.RadiusY)
                    return false;
                return AreNear(Geometry.TransformPoint(ellipse.Center, placed.Placement.Matrix), Geometry.TransformPoint(drawnEllipse.Center, drawn.Placement.Matrix));
            }

            PathShape path = placed.Shape as PathShape, drawnPath = drawn.Shape as PathShape;
            if (path == null || path.Parts.Count != drawnPath.Parts.Count)
                return false;
            for (int i = 0; i < path.Parts.Count; ++i) {
                PointF[] points = path.Parts[i].Points ?? new PointF[0], drawnPoints = drawnPath.Parts[i].Points ?? new PointF[0];
                if (path.Parts[i].Kind != drawnPath.Parts[i].Kind || points.Length != drawnPoints.Length)
                    return false;
                for (int k = 0; k < points.Length; ++k) {
                    if (!AreNear(Geometry.TransformPoint(points[k], placed.Placement.Matrix), Geometry.TransformPoint(drawnPoints[k], drawn.Placement.Matrix)))
                        return false;
                }
            }
            return true;
        }

        // Do two points agree to within ExpansionTolerance?
        static bool AreNear(PointF a, PointF b)
        {
            return Math.Abs(a.X - b.X) <= ExpansionTolerance && Math.Abs(a.Y - b.Y) <= ExpansionTolerance;
        }

        // Is an expanded dash of a run the same as a dash that the drawing draws: the same line and placement, and a
        // polyline whose points agree to within ExpansionTolerance, points nearer each other than that counting as one?
        // dash: the expanded dash.
        // drawn: the clipping object of the drawing, or null.
        static bool AreSameDashes(StrokedShape dash, StrokedShape drawn)
        {
            if (drawn == null)
                return false;
            if (Describe(dash.Stroke) != Describe(drawn.Stroke) || !dash.Placement.Steps.Select(Describe).SequenceEqual(drawn.Placement.Steps.Select(Describe)))
                return false;

            List<PointF> points = DistinctPoints((PathShape)dash.Shape), drawnPoints = DistinctPoints(drawn.Shape as PathShape);
            if (drawnPoints == null || points.Count != drawnPoints.Count)
                return false;
            for (int i = 0; i < points.Count; ++i) {
                if (Math.Abs(points[i].X - drawnPoints[i].X) > ExpansionTolerance || Math.Abs(points[i].Y - drawnPoints[i].Y) > ExpansionTolerance)
                    return false;
            }
            return true;
        }

        // The points of an open polyline, without a point nearer to the one before it than ExpansionTolerance, or null for a
        // shape that is no open polyline.
        static List<PointF> DistinctPoints(PathShape path)
        {
            if (path == null || path.Parts.Count != 2 || path.Parts[0].Kind != GraphicsPathPartKind.Start || path.Parts[1].Kind != GraphicsPathPartKind.Lines)
                return null;
            List<PointF> points = new List<PointF>();
            foreach (PointF point in path.Parts[0].Points.Concat(path.Parts[1].Points)) {
                if (points.Count == 0 || Math.Abs(point.X - points[points.Count - 1].X) > ExpansionTolerance || Math.Abs(point.Y - points[points.Count - 1].Y) > ExpansionTolerance)
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
