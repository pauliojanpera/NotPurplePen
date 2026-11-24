using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using PurplePen.Graphics2D;
using PurplePen.MapModel;

namespace Map_Skia.Tests
{
    // The expansion of the clipping objects of palette color coverages of symbols into the clipping objects that the
    // drawing makes, with which the tests draw the clipping objects (PaletteSeparationCoverageTests), and the description
    // of a clipping object for the messages of the tests. The geometry methods of the lines make forms that the drawing
    // does not draw as they are: runs of dashes, offset paths and repeated placements. The expansion makes single
    // clipping objects of them.
    static class SymbolCoverageComparison
    {
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

        // The description of a clipping object: its kind, its shape, its pen or fill rule, and the steps of its
        // placement, each number exactly.
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
            if (shape is OffsetPathShape offsetPath)
                return string.Format("offset path of {0}, offset {1}", Describe(offsetPath.BasePath), Describe(offsetPath.Offset));
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

        // The description of a number, in which 0 and -0 are the same.
        static string Describe(float value)
        {
            return ((value == 0) ? 0F : value).ToString("R", CultureInfo.InvariantCulture);
        }
    }
}
