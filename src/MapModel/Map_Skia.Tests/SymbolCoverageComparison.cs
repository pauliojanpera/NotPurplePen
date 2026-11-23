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
        public static List<string> FindDifferences(SymbolCoverage coverage, IReadOnlyList<ClippingObject> expected, IReadOnlyList<ClippingObject> actual)
        {
            List<string> differences = new List<string>();
            for (int i = 0; i < Math.Max(expected.Count, actual.Count); ++i) {
                string expectedObject = (i < expected.Count) ? Describe(expected[i]) : "none";
                string actualObject = (i < actual.Count) ? Describe(actual[i]) : "none";
                if (actualObject != expectedObject)
                    differences.Add(string.Format("{0} (id {1}), {2}, clipping object {3}: {4}, expected {5}", coverage.Color.Name, coverage.Color.OcadId, coverage.Symbol.Definition.Name, i, actualObject, expectedObject));
            }

            return differences;
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
