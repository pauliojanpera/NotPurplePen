using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;

namespace PurplePen.MapModel
{
    using PurplePen.Graphics2D;

    // The data structure of the palette layers, which is the interface between the two passes of the drawing. Pass 1
    // makes the layer instances from the symbols and the objects of the map (Map.CreateLayerInstances), and pass 2
    // groups the instances by palette entry and composites each group once. Each instance has a palette entry, the
    // geometry prototype that it places with its transformation M, and its clipping objects. The palette entry gives
    // the color, the opacity and the blend mode. The prototype, the transformation and the clipping objects are
    // geometry only.

    // The geometry G of a layer instance: a binary selector of the points that the instance covers, which is bound to
    // its origin. The instance places it with its transformation M, and the clipping objects of the instance limit
    // where it applies.
    public abstract class GeometryPrototype
    {
        // Full coverage, G = 1: the constant case of repetitive geometry. A transformation does not change it.
        public static readonly GeometryPrototype FullCoverage = new FullCoveragePrototype();

        private protected GeometryPrototype()
        {
        }

        // Does the geometry cover a point of its own coordinates, G(x, y) = 1?
        // point: the point.
        public abstract bool Covers(PointF point);

        // The geometry that covers every point.
        private sealed class FullCoveragePrototype : GeometryPrototype
        {
            public override bool Covers(PointF point)
            {
                return true;
            }

            public override string ToString()
            {
                return "G = 1";
            }
        }
    }

    // A layer instance: a palette entry, the geometry prototype that the instance places with its transformation M,
    // and the clipping objects that limit where the geometry applies. The union of the clipping objects is the extent
    // of the instance.
    public sealed class LayerInstance
    {
        private readonly SymColor paletteEntry;
        private readonly GeometryPrototype prototype;
        private readonly Matrix transformation;
        private readonly ReadOnlyCollection<ClippingObject> clippingObjects;

        // Make a layer instance.
        // paletteEntry: the palette entry of the instance.
        // prototype: the geometry that the instance places.
        // transformation: the transformation M, from the coordinates of the prototype to those of the map. It is copied.
        // clippingObjects: the clipping objects of the instance.
        public LayerInstance(SymColor paletteEntry, GeometryPrototype prototype, Matrix transformation, IEnumerable<ClippingObject> clippingObjects)
        {
            if (paletteEntry == null)
                throw new ArgumentNullException(nameof(paletteEntry));
            if (prototype == null)
                throw new ArgumentNullException(nameof(prototype));
            if (transformation == null)
                throw new ArgumentNullException(nameof(transformation));
            if (clippingObjects == null)
                throw new ArgumentNullException(nameof(clippingObjects));

            this.paletteEntry = paletteEntry;
            this.prototype = prototype;
            this.transformation = transformation.Clone();
            this.clippingObjects = new List<ClippingObject>(clippingObjects).AsReadOnly();
        }

        // The palette entry of the instance.
        public SymColor PaletteEntry
        {
            get { return paletteEntry; }
        }

        // The geometry that the instance places.
        public GeometryPrototype Prototype
        {
            get { return prototype; }
        }

        // A copy of the transformation M, from the coordinates of the prototype to those of the map.
        public Matrix Transformation
        {
            get { return transformation.Clone(); }
        }

        // The clipping objects of the instance.
        public IReadOnlyList<ClippingObject> ClippingObjects
        {
            get { return clippingObjects; }
        }

        public override string ToString()
        {
            return string.Format("{0}, {1}, {2} clipping objects", paletteEntry, prototype, clippingObjects.Count);
        }
    }

    // A clipping object of a layer instance: a shape, filled or stroked, at its placement in the coordinates of the
    // map. It limits the extent over which the geometry of its instance applies. It has no color and no opacity of
    // its own.
    public abstract class ClippingObject
    {
        private readonly Shape shape;
        private readonly Placement placement;
        private RectangleF? bounds;

        private protected ClippingObject(Shape shape, Placement placement)
        {
            if (shape == null)
                throw new ArgumentNullException(nameof(shape));
            if (placement == null)
                throw new ArgumentNullException(nameof(placement));

            this.shape = shape;
            this.placement = placement;
        }

        // The shape, in its own coordinates.
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
            get
            {
                if (!bounds.HasValue)
                    bounds = placement.TransformBounds(CoveredBounds);
                return bounds.Value;
            }
        }

        // The bounds of the area that the clipping object covers, in the coordinates of the shape.
        private protected abstract RectangleF CoveredBounds { get; }

        // The clipping object of the same shape, covered in the same way, at another placement. The placements of a
        // glyph share the shapes of its parts in this way.
        // placement: the placement.
        internal abstract ClippingObject WithPlacement(Placement placement);
    }

    // A clipping object that a filled shape makes.
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

        internal override ClippingObject WithPlacement(Placement placement)
        {
            return new FilledShape(Shape, fillRule, placement);
        }

        public override string ToString()
        {
            return "Filled " + Shape;
        }
    }

    // A clipping object that a stroked shape makes.
    public sealed class StrokedShape : ClippingObject
    {
        private readonly Stroke stroke;

        // Make a stroked shape.
        // shape: the shape.
        // stroke: the line that strokes the shape.
        // placement: the placement of the shape in the coordinates of the map.
        public StrokedShape(Shape shape, Stroke stroke, Placement placement)
            : base(shape, placement)
        {
            if (stroke == null)
                throw new ArgumentNullException(nameof(stroke));

            this.stroke = stroke;
        }

        // The line that strokes the shape.
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

        internal override ClippingObject WithPlacement(Placement placement)
        {
            return new StrokedShape(Shape, stroke, placement);
        }

        // The dashes of a dashed stroke as single dashes: each a stroked polyline of the solid line, from its start
        // through the points of the shape to its end, at the placement of the shape. The dashes follow the pattern along
        // the shape, measured in the coordinates of the shape, from the phase. A solid stroke, or a shape that is not one
        // open figure of lines, gives itself.
        public IReadOnlyList<StrokedShape> ExpandDashes()
        {
            PathShape path = Shape as PathShape;
            if (stroke.DashPattern == null || path == null || path.Parts.Count != 2 ||
                path.Parts[0].Kind != GraphicsPathPartKind.Start || path.Parts[1].Kind != GraphicsPathPartKind.Lines)
            {
                return new StrokedShape[] { this };
            }

            List<PointF> points = new List<PointF>(path.Parts[0].Points);
            points.AddRange(path.Parts[1].Points);
            IReadOnlyList<float> pattern = stroke.DashPattern;
            Stroke solid = stroke.WithoutDashes();

            // Find the dash or the gap of the pattern at the start of the shape, and how much of it is left.
            double period = 0;
            foreach (float length in pattern)
                period += length;
            double phase = (period > 0) ? stroke.DashPhase % period : 0;
            if (phase < 0)
                phase += period;
            int index = 0;
            while (phase > 0 && phase >= pattern[index]) {
                phase -= pattern[index];
                index = (index + 1) % pattern.Count;
            }
            double left = pattern[index] - phase;
            bool inDash = (index % 2 == 0);

            List<StrokedShape> dashes = new List<StrokedShape>();
            List<PointF> dash = new List<PointF>();
            if (inDash)
                dash.Add(points[0]);
            for (int i = 1; i < points.Count; ++i) {
                PointF from = points[i - 1], to = points[i];
                double segment = Math.Sqrt((double)(to.X - from.X) * (to.X - from.X) + (double)(to.Y - from.Y) * (to.Y - from.Y));
                double done = 0;
                while (left < segment - done) {
                    done += left;
                    double fraction = done / segment;
                    PointF boundary = new PointF((float)(from.X + (to.X - from.X) * fraction), (float)(from.Y + (to.Y - from.Y) * fraction));
                    if (inDash) {
                        dash.Add(boundary);
                        dashes.Add(new StrokedShape(PathShape.FromPoints(dash.ToArray(), false), solid, Placement));
                        dash.Clear();
                    }
                    else {
                        dash.Add(boundary);
                    }
                    inDash = !inDash;
                    index = (index + 1) % pattern.Count;
                    left = pattern[index];
                }
                left -= segment - done;
                if (inDash)
                    dash.Add(to);
            }
            if (inDash && dash.Count > 1) {
                dashes.Add(new StrokedShape(PathShape.FromPoints(dash.ToArray(), false), solid, Placement));
            }
            else if (!inDash && Math.Abs(left) < 1e-6 && pattern[(index + 1) % pattern.Count] == 0) {
                // A gap ends at the end of the shape, where a dash of length 0 follows it.
                PointF end = points[points.Count - 1];
                dashes.Add(new StrokedShape(PathShape.FromPoints(new PointF[] { end, end }, false), solid, Placement));
            }

            return dashes;
        }

        public override string ToString()
        {
            return string.Format("Stroked {0}, {1}", Shape, stroke);
        }
    }

    // The line that strokes a shape: its width, caps, joins and miter limit, in the coordinates of the shape, and, for a
    // dashed line, its dash pattern and phase. The pattern is the lengths of the dashes and the gaps in turn, a dash
    // first, repeated along the shape; the phase is how far into the pattern the shape starts, as SVG's
    // stroke-dasharray and stroke-dashoffset and the dash pattern of PDF give them. Each dash has the caps of the line.
    public sealed class Stroke
    {
        private readonly float width;
        private readonly LineCapMode caps;
        private readonly LineJoinMode join;
        private readonly float miterLimit;
        private readonly float[] dashPattern;
        private readonly float dashPhase;

        // Make a stroke of a solid line.
        // width: the width of the line.
        // caps: the caps at the ends of the line.
        // join: the joins at the corners of the line.
        // miterLimit: the longest miter join, as a multiple of half the width.
        public Stroke(float width, LineCapMode caps, LineJoinMode join, float miterLimit)
            : this(width, caps, join, miterLimit, null, 0)
        {
        }

        // Make a stroke of a dashed line.
        // width, caps, join, miterLimit: as for a solid line.
        // dashPattern: the lengths of the dashes and the gaps in turn, a dash first, an even number of them, or null for
        //   a solid line. It is copied.
        // dashPhase: how far into the pattern the shape starts.
        public Stroke(float width, LineCapMode caps, LineJoinMode join, float miterLimit, float[] dashPattern, float dashPhase)
        {
            if (dashPattern != null && (dashPattern.Length == 0 || dashPattern.Length % 2 != 0))
                throw new ArgumentException("A dash pattern has a gap after each dash.", nameof(dashPattern));

            this.width = width;
            this.caps = caps;
            this.join = join;
            this.miterLimit = miterLimit;
            this.dashPattern = (dashPattern == null) ? null : (float[])dashPattern.Clone();
            this.dashPhase = dashPhase;
        }

        // The lengths of the dashes and the gaps in turn, a dash first, or null for a solid line.
        public IReadOnlyList<float> DashPattern
        {
            get { return dashPattern; }
        }

        // How far into the dash pattern the shape starts.
        public float DashPhase
        {
            get { return dashPhase; }
        }

        // The stroke of the same line without dashes.
        public Stroke WithoutDashes()
        {
            return (dashPattern == null) ? this : new Stroke(width, caps, join, miterLimit);
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

        // The longest distance from the shape to a point that the stroke covers: half the width, and more at a square
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
            string line = string.Format("width {0}, {1} caps, {2} joins, miter limit {3}", width, caps, join, miterLimit);
            return (dashPattern == null) ? line : string.Format("{0}, dashes {1} from {2}", line, string.Join(" ", dashPattern), dashPhase);
        }
    }

    // The affine placement of a shape in the coordinates of the map. The drawing places a shape with a sequence of
    // transformations. A placement keeps them, the outermost first, and their composition, which is the affine
    // placement. A drawing of the shapes can apply the steps as the symbols applied them, with the same rounding. The
    // shapes that a symbol places with one transformation share their placement, as the parts of a glyph do.
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

    // The geometry of a clipping object, in its own coordinates.
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
            get { return GetBoundsOfPoints(EnumerateAllPoints()); }
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

    // A text run: a string in a font, with the upper left corner of the text at a point, as IGraphicsTarget.DrawText
    // draws it.
    public sealed class TextShape : Shape
    {
        private readonly string text;
        private readonly string fontFamily;
        private readonly float emHeight;
        private readonly TextEffects effects;
        private readonly PointF upperLeft;
        private readonly ITextMetrics textMetrics;
        private RectangleF? bounds;

        // Make a text run.
        // text: the text.
        // fontFamily: the family of the font.
        // emHeight: the size of the font: the height of its em square.
        // effects: the effects of the font.
        // upperLeft: the upper left corner of the text.
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
        // without visible glyphs has empty bounds at its upper left corner.
        public override RectangleF Bounds
        {
            get
            {
                if (!bounds.HasValue) {
                    RectangleF glyphBounds;
                    using (ITextFaceMetrics faceMetrics = textMetrics.GetTextFaceMetrics(fontFamily, emHeight, effects))
                        glyphBounds = faceMetrics.GetTightBoundingBox(upperLeft, text);
                    bounds = (glyphBounds.Width > 0 || glyphBounds.Height > 0) ? glyphBounds : new RectangleF(upperLeft, SizeF.Empty);
                }

                return bounds.Value;
            }
        }

        public override string ToString()
        {
            return string.Format("text \"{0}\" in {1} {2} {3} at {4}", text, fontFamily, emHeight, effects, upperLeft);
        }
    }
}
