using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Threading;

namespace PurplePen.MapModel
{
    using PurplePen.Graphics2D;

    // The forms of the clipping objects, which are the derived values of the palette color coverages of the symbols
    // (SymbolCoverage). A clipping object limits the extent over which the coverage prototype of its palette cutter
    // applies. It has a shape that is drawn, a fill or a stroke, and a placement. It gives no color and no alpha: the
    // palette color gives them. All the forms are immutable, so several readers can read them at the same time.

    // A clipping object: a shape, filled or stroked, at its placement in the coordinates of the map.
    public abstract class ClippingObject
    {
        private readonly Shape shape;
        private readonly Placement placement;

        private protected ClippingObject(Shape shape, Placement placement)
        {
            if (shape == null)
                throw new ArgumentNullException(nameof(shape));
            if (placement == null)
                throw new ArgumentNullException(nameof(placement));

            this.shape = shape;
            this.placement = placement;
        }

        // The shape that is drawn, in its own coordinates. For a filled shape, it is the outline of the area. For a
        // stroked shape, it is the path at the middle of its stroke shape.
        public Shape Shape
        {
            get { return shape; }
        }

        // The placement of the shape in the coordinates of the map.
        public Placement Placement
        {
            get { return placement; }
        }

        // The bounds of the area that the clipping object covers, in the coordinates of the map.
        public RectangleF Bounds
        {
            get { return placement.TransformBounds(CoveredBounds); }
        }

        // The bounds of the area that the clipping object covers, in the coordinates of the shape.
        private protected abstract RectangleF CoveredBounds { get; }
    }

    // A filled shape: a clipping object that covers the inside of its shape by its fill rule.
    public sealed class FilledShape : ClippingObject
    {
        private readonly AreaFillMode fillRule;

        // Make a filled shape.
        // shape: the shape. An arc is open, and only a stroke covers it.
        // fillRule: the rule that finds the points that a path encloses.
        // placement: the placement of the shape in the coordinates of the map.
        public FilledShape(Shape shape, AreaFillMode fillRule, Placement placement)
            : base(shape, placement)
        {
            if (shape is ArcShape)
                throw new ArgumentException("An arc is open, and only a stroke covers it.", nameof(shape));

            this.fillRule = fillRule;
        }

        // The rule that finds the points that a path encloses. An ellipse and a text run fill in the same way with
        // each rule.
        public AreaFillMode FillRule
        {
            get { return fillRule; }
        }

        private protected override RectangleF CoveredBounds
        {
            get { return Shape.Bounds; }
        }

        public override string ToString()
        {
            return "Filled " + Shape;
        }
    }

    // A stroked shape: a clipping object that covers the band that a pen draws along its shape, its stroke shape.
    public sealed class StrokedShape : ClippingObject
    {
        private readonly Stroke stroke;

        // Make a stroked shape.
        // shape: the shape.
        // stroke: the pen that draws along the shape.
        // placement: the placement of the shape in the coordinates of the map.
        public StrokedShape(Shape shape, Stroke stroke, Placement placement)
            : base(shape, placement)
        {
            if (stroke == null)
                throw new ArgumentNullException(nameof(stroke));

            this.stroke = stroke;
        }

        // The pen that draws along the shape.
        public Stroke Stroke
        {
            get { return stroke; }
        }

        private protected override RectangleF CoveredBounds
        {
            get
            {
                RectangleF shapeBounds = Shape.Bounds;
                shapeBounds.Inflate(stroke.Reach, stroke.Reach);
                return shapeBounds;
            }
        }

        public override string ToString()
        {
            return string.Format("Stroked {0}, {1}", Shape, stroke);
        }
    }

    // The pen of a stroked shape: its width, caps, join and miter limit, in the coordinates of the shape. It has no
    // color, as the palette color gives the color.
    public sealed class Stroke
    {
        private readonly float width;
        private readonly LineCapMode caps;
        private readonly LineJoinMode join;
        private readonly float miterLimit;

        // Make a pen.
        // width: the width of the pen.
        // caps: the caps at the ends of the shape.
        // join: the joins at the corners of the shape.
        // miterLimit: the longest miter join, as a multiple of half the width.
        public Stroke(float width, LineCapMode caps, LineJoinMode join, float miterLimit)
        {
            this.width = width;
            this.caps = caps;
            this.join = join;
            this.miterLimit = miterLimit;
        }

        public float Width
        {
            get { return width; }
        }

        public LineCapMode Caps
        {
            get { return caps; }
        }

        public LineJoinMode Join
        {
            get { return join; }
        }

        public float MiterLimit
        {
            get { return miterLimit; }
        }

        // The longest distance from the shape to a point that the pen covers: half the width, and more at a square
        // cap and at a miter join.
        internal float Reach
        {
            get
            {
                float factor = 1;
                if (caps == LineCapMode.Square)
                    factor = (float)Math.Sqrt(2);
                if ((join == LineJoinMode.Miter || join == LineJoinMode.MiterClipped) && miterLimit > factor)
                    factor = miterLimit;
                return width / 2 * factor;
            }
        }

        public override string ToString()
        {
            return string.Format("width {0}, {1} caps, {2} joins, miter limit {3}", width, caps, join, miterLimit);
        }
    }

    // The affine placement of a shape in the coordinates of the map. The drawing code of a symbol places a shape with
    // nested transformations. A placement keeps them as its steps, the outermost first, in the order in which the
    // symbol set them, and it composes them into one matrix. A channel can apply the steps as the symbols applied them,
    // with the same rounding. The shapes that a symbol places with one transformation share their placement.
    public sealed class Placement
    {
        // The placement of the shapes that no transformation places.
        public static readonly Placement Identity = new Placement(null, null, new Matrix());

        private readonly Placement outer;       // The placement within which the step applies, or null.
        private readonly Matrix step;           // The transformation that this placement adds to the outer one.
        private readonly Matrix composition;    // The composition of the steps.

        private Placement(Placement outer, Matrix step, Matrix composition)
        {
            this.outer = outer;
            this.step = step;
            this.composition = composition;
        }

        // The placement of the shapes that a transformation places within this placement. The transformation applies
        // to a shape before this placement does, as IGraphicsTarget.PushTransform prepends a transformation.
        // step: the transformation. It is copied.
        public Placement Within(Matrix step)
        {
            if (step == null)
                throw new ArgumentNullException(nameof(step));

            Matrix composition = this.composition.Clone();
            composition.Multiply(step, MatrixOrder.Prepend);
            return new Placement(this, step.Clone(), composition);
        }

        // A copy of the composition of the steps: the affine transformation from the coordinates of the shape to those
        // of the map.
        public Matrix Matrix
        {
            get { return composition.Clone(); }
        }

        // Copies of the steps, the outermost first. The identity placement has no steps.
        public IReadOnlyList<Matrix> Steps
        {
            get
            {
                List<Matrix> steps = new List<Matrix>();
                for (Placement placement = this; placement.outer != null; placement = placement.outer)
                    steps.Add(placement.step.Clone());
                steps.Reverse();
                return steps.AsReadOnly();
            }
        }

        // The bounds of a rectangle of the coordinates of the shape, in the coordinates of the map.
        // rect: the rectangle.
        internal RectangleF TransformBounds(RectangleF rect)
        {
            return Geometry.BoundsOfTransformedRectangle(rect, composition);
        }
    }

    // The shape of a clipping object, in its own coordinates: a path, an ellipse, an arc or a text run.
    public abstract class Shape
    {
        private protected Shape()
        {
        }

        // The bounds of the shape, in its own coordinates.
        public abstract RectangleF Bounds { get; }

        // The bounds of points.
        // points: the points.
        private protected static RectangleF GetBoundsOfPoints(IEnumerable<PointF> points)
        {
            float left = float.MaxValue, top = float.MaxValue, right = float.MinValue, bottom = float.MinValue;
            foreach (PointF point in points) {
                left = Math.Min(left, point.X);
                top = Math.Min(top, point.Y);
                right = Math.Max(right, point.X);
                bottom = Math.Max(bottom, point.Y);
            }

            if (left > right)
                return RectangleF.Empty;
            else
                return RectangleF.FromLTRB(left, top, right, bottom);
        }
    }

    // A path: figures of lines and Bezier curves, each from a start point, and closed or open (GraphicsPathPart).
    public sealed class PathShape : Shape
    {
        private readonly ReadOnlyCollection<GraphicsPathPart> parts;
        private readonly RectangleF bounds;

        // Make a path from parts. The path keeps a copy of the parts and their points.
        // parts: the parts of the path.
        public PathShape(IEnumerable<GraphicsPathPart> parts)
        {
            if (parts == null)
                throw new ArgumentNullException(nameof(parts));

            List<GraphicsPathPart> copies = new List<GraphicsPathPart>();
            foreach (GraphicsPathPart part in parts)
                copies.Add(new GraphicsPathPart(part.Kind, (part.Points == null) ? null : (PointF[])part.Points.Clone()));
            this.parts = copies.AsReadOnly();
            this.bounds = GetBoundsOfPoints(EnumerateAllPoints());
        }

        // Make the path of one figure of lines, through points from the first to the last, as a polyline or a polygon.
        // points: the points. At least one.
        // isClosed: whether the figure is closed, as a polygon.
        public static PathShape FromPoints(PointF[] points, bool isClosed)
        {
            if (points == null)
                throw new ArgumentNullException(nameof(points));
            if (points.Length == 0)
                throw new ArgumentException("A figure has a start point.", nameof(points));

            List<GraphicsPathPart> parts = new List<GraphicsPathPart>(3);
            parts.Add(new GraphicsPathPart(GraphicsPathPartKind.Start, new PointF[] { points[0] }));
            if (points.Length > 1) {
                PointF[] linePoints = new PointF[points.Length - 1];
                Array.Copy(points, 1, linePoints, 0, linePoints.Length);
                parts.Add(new GraphicsPathPart(GraphicsPathPartKind.Lines, linePoints));
            }
            if (isClosed)
                parts.Add(new GraphicsPathPart(GraphicsPathPartKind.Close, null));

            return new PathShape(parts);
        }

        // The parts of the path. Their points must not change.
        public IReadOnlyList<GraphicsPathPart> Parts
        {
            get { return parts; }
        }

        // The bounds of the points of the path, its Bezier control points too.
        public override RectangleF Bounds
        {
            get { return bounds; }
        }

        // All points of the parts.
        private IEnumerable<PointF> EnumerateAllPoints()
        {
            foreach (GraphicsPathPart part in parts) {
                if (part.Points != null) {
                    foreach (PointF point in part.Points)
                        yield return point;
                }
            }
        }

        public override string ToString()
        {
            return string.Format("path of {0} parts", parts.Count);
        }
    }

    // An ellipse, whose axes are the axes of its coordinates.
    public sealed class EllipseShape : Shape
    {
        private readonly PointF center;
        private readonly float radiusX, radiusY;

        // Make an ellipse.
        // center: the center.
        // radiusX, radiusY: the radii along the axes.
        public EllipseShape(PointF center, float radiusX, float radiusY)
        {
            this.center = center;
            this.radiusX = radiusX;
            this.radiusY = radiusY;
        }

        public PointF Center
        {
            get { return center; }
        }

        public float RadiusX
        {
            get { return radiusX; }
        }

        public float RadiusY
        {
            get { return radiusY; }
        }

        public override RectangleF Bounds
        {
            get { return RectangleF.FromLTRB(center.X - radiusX, center.Y - radiusY, center.X + radiusX, center.Y + radiusY); }
        }

        public override string ToString()
        {
            return string.Format("ellipse at {0}, radii {1} and {2}", center, radiusX, radiusY);
        }
    }

    // An arc of a circle, as IGraphicsTarget.DrawArc draws it: from a start angle, by a sweep angle, in degrees. An
    // angle turns from the X axis toward the Y axis.
    public sealed class ArcShape : Shape
    {
        private readonly PointF center;
        private readonly float radius;
        private readonly float startAngle, sweepAngle;

        // Make an arc.
        // center: the center of the circle.
        // radius: the radius of the circle.
        // startAngle: the angle of the start of the arc.
        // sweepAngle: the angle from the start to the end of the arc.
        public ArcShape(PointF center, float radius, float startAngle, float sweepAngle)
        {
            this.center = center;
            this.radius = radius;
            this.startAngle = startAngle;
            this.sweepAngle = sweepAngle;
        }

        public PointF Center
        {
            get { return center; }
        }

        public float Radius
        {
            get { return radius; }
        }

        public float StartAngle
        {
            get { return startAngle; }
        }

        public float SweepAngle
        {
            get { return sweepAngle; }
        }

        // The bounds of the ends of the arc and of the points where it crosses an axis through the center.
        public override RectangleF Bounds
        {
            get
            {
                double from = Math.Min(startAngle, startAngle + sweepAngle);
                double to = Math.Max(startAngle, startAngle + sweepAngle);
                List<PointF> points = new List<PointF>() { GetPointAt(from), GetPointAt(to) };
                for (double angle = Math.Ceiling(from / 90) * 90; angle < to && angle < from + 360; angle += 90)
                    points.Add(GetPointAt(angle));
                return GetBoundsOfPoints(points);
            }
        }

        // The point of the circle at an angle.
        // angle: the angle, in degrees.
        private PointF GetPointAt(double angle)
        {
            double radians = angle * Math.PI / 180;
            return new PointF((float)(center.X + radius * Math.Cos(radians)), (float)(center.Y + radius * Math.Sin(radians)));
        }

        public override string ToString()
        {
            return string.Format("arc at {0}, radius {1}, from {2} by {3} degrees", center, radius, startAngle, sweepAngle);
        }
    }

    // A text run: a word, or a grapheme where the character spacing is more than 0, in a font, with the top left
    // corner of the text at a point, as IGraphicsTarget.DrawText draws it.
    public sealed class TextShape : Shape
    {
        private readonly string text;
        private readonly string fontFamily;
        private readonly float emHeight;
        private readonly TextEffects effects;
        private readonly PointF upperLeft;
        private readonly ITextMetrics textMetrics;
        private object bounds;      // The boxed bounds, after the first read measured them.

        // Make a text run.
        // text: the text.
        // fontFamily: the family of the font.
        // emHeight: the size of the font: the height of its em square.
        // effects: the effects of the font.
        // upperLeft: the top left corner of the text.
        // textMetrics: the metrics of the fonts that measure the bounds of the text.
        public TextShape(string text, string fontFamily, float emHeight, TextEffects effects, PointF upperLeft, ITextMetrics textMetrics)
        {
            if (text == null)
                throw new ArgumentNullException(nameof(text));
            if (fontFamily == null)
                throw new ArgumentNullException(nameof(fontFamily));
            if (textMetrics == null)
                throw new ArgumentNullException(nameof(textMetrics));

            this.text = text;
            this.fontFamily = fontFamily;
            this.emHeight = emHeight;
            this.effects = effects;
            this.upperLeft = upperLeft;
            this.textMetrics = textMetrics;
        }

        public string Text
        {
            get { return text; }
        }

        public string FontFamily
        {
            get { return fontFamily; }
        }

        public float EmHeight
        {
            get { return emHeight; }
        }

        public TextEffects Effects
        {
            get { return effects; }
        }

        public PointF UpperLeft
        {
            get { return upperLeft; }
        }

        // The bounds of the outlines of the glyphs of the text, as the metrics of its font measure them. A text
        // without visible glyphs has empty bounds at its top left corner. The first read measures them, and the
        // readers that measure them at the same time get the same bounds.
        public override RectangleF Bounds
        {
            get
            {
                object measured = Volatile.Read(ref bounds);
                if (measured == null) {
                    measured = MeasureBounds();
                    Interlocked.CompareExchange(ref bounds, measured, null);
                }

                return (RectangleF)measured;
            }
        }

        // Measure the bounds of the outlines of the glyphs of the text, and box them.
        private object MeasureBounds()
        {
            RectangleF glyphBounds;
            using (ITextFaceMetrics faceMetrics = textMetrics.GetTextFaceMetrics(fontFamily, emHeight, effects))
                glyphBounds = faceMetrics.GetTightBoundingBox(upperLeft, text);
            return (glyphBounds.Width > 0 || glyphBounds.Height > 0) ? glyphBounds : new RectangleF(upperLeft, SizeF.Empty);
        }

        public override string ToString()
        {
            return string.Format("text \"{0}\" in {1} {2} {3} at {4}", text, fontFamily, emHeight, effects, upperLeft);
        }
    }
}
