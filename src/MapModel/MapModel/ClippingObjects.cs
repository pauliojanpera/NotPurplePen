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

        // The clipping object of the same shape, filled or stroked in the same way, at another placement. The
        // placements of a glyph share the shapes of its parts in this way.
        // placement: the placement.
        internal abstract ClippingObject WithPlacement(Placement placement);
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

        internal override ClippingObject WithPlacement(Placement placement)
        {
            return new FilledShape(Shape, fillRule, placement);
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

        internal override ClippingObject WithPlacement(Placement placement)
        {
            return new StrokedShape(Shape, stroke, placement);
        }

        // The expansion of a run of dashes into single dashes: each a stroked polyline of the pen without dashes, from
        // its start through the points of the path to its end, at the placement of the shape. The dashes follow the
        // pattern along the path from the phase, euclidean in the coordinates of the shape. The path of an offset path is
        // its base path, and each dash is moved aside as the offset path moves its base path (OffsetPathShape). Where the
        // pattern and the path end together, within RoundingOfLengths, the last dash ends at the end of the path: the
        // pattern does not start a dash there, except a dash of length 0. A pen without dashes gives the shape itself,
        // and so does a shape that is not one open figure of lines or the offset path of one.
        public IReadOnlyList<StrokedShape> ExpandDashes()
        {
            OffsetPathShape offsetPath = Shape as OffsetPathShape;
            PathShape path = (offsetPath != null) ? offsetPath.BasePath : Shape as PathShape;
            PointF[] points = (path == null) ? null : path.GetPolyline();
            if (stroke.DashPattern == null || points == null)
                return new StrokedShape[] { this };

            IReadOnlyList<float> pattern = stroke.DashPattern;
            Stroke solid = stroke.WithoutDashes();

            // Find the dash or the gap of the pattern at the start of the path, and how much of it is left.
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

            List<PointF[]> dashes = new List<PointF[]>();
            List<PointF> dash = new List<PointF>();
            if (inDash)
                dash.Add(points[0]);
            for (int i = 1; i < points.Length; ++i) {
                PointF from = points[i - 1], to = points[i];
                double segment = MeasureSegment(from, to);
                double done = 0;
                while (left < segment - done) {
                    done += left;
                    double fraction = done / segment;
                    PointF boundary = new PointF((float)(from.X + (to.X - from.X) * fraction), (float)(from.Y + (to.Y - from.Y) * fraction));
                    dash.Add(boundary);
                    if (inDash) {
                        dashes.Add(dash.ToArray());
                        dash.Clear();
                    }
                    inDash = !inDash;
                    index = (index + 1) % pattern.Count;
                    left = pattern[index];
                }
                left -= segment - done;
                if (inDash)
                    dash.Add(to);
            }
            if (inDash && dash.Count > 1 && (pattern[index] == 0 || MeasurePolyline(dash) >= RoundingOfLengths)) {
                dashes.Add(dash.ToArray());
            }
            else if (!inDash && Math.Abs(left) < RoundingOfLengths && pattern[(index + 1) % pattern.Count] == 0) {
                // A gap ends at the end of the path, where a dash of length 0 follows it.
                PointF end = points[points.Length - 1];
                dashes.Add(new PointF[] { end, end });
            }

            List<StrokedShape> expanded = new List<StrokedShape>(dashes.Count);
            foreach (PointF[] dashPoints in dashes) {
                PointF[] drawn = (offsetPath != null) ? offsetPath.MoveAside(dashPoints) : dashPoints;
                expanded.Add(new StrokedShape(PathShape.FromPoints(drawn, false), solid, Placement));
            }
            return expanded;
        }

        // The difference, in mm, within which the lengths of a pattern and of a path that it dashes are equal: the
        // rounding of lengths that are measured in floats.
        private const double RoundingOfLengths = 0.00001;

        // The length of a polyline.
        // points: the points of the polyline.
        private static double MeasurePolyline(List<PointF> points)
        {
            double length = 0;
            for (int i = 1; i < points.Count; ++i)
                length += MeasureSegment(points[i - 1], points[i]);
            return length;
        }

        // The length of a line segment.
        // from, to: the ends of the segment.
        private static double MeasureSegment(PointF from, PointF to)
        {
            return Math.Sqrt((double)(to.X - from.X) * (to.X - from.X) + (double)(to.Y - from.Y) * (to.Y - from.Y));
        }

        public override string ToString()
        {
            return string.Format("Stroked {0}, {1}", Shape, stroke);
        }
    }

    // The pen of a stroked shape: its width, caps, join and miter limit, in the coordinates of the shape, and for a run
    // of dashes, its dash pattern and its phase. It has no color, as the palette color gives the color. The dash pattern
    // is the lengths of the dashes and the gaps in turn, a dash first, repeated along the shape, and the phase is the
    // distance that the pattern has gone at the start of the shape, as stroke-dasharray and stroke-dashoffset of SVG,
    // the dash pattern of PDF and the dash effect of Skia give them. They are in the measure of the channels: euclidean
    // along the own path of the shape, and along the base path of an offset path (OffsetPathShape). Each dash has the
    // caps of the pen.
    public sealed class Stroke
    {
        private readonly float width;
        private readonly LineCapMode caps;
        private readonly LineJoinMode join;
        private readonly float miterLimit;
        private readonly float[] dashPattern;
        private readonly float dashPhase;

        // Make a pen without dashes.
        // width: the width of the pen.
        // caps: the caps at the ends of the shape.
        // join: the joins at the corners of the shape.
        // miterLimit: the longest miter join, as a multiple of half the width.
        public Stroke(float width, LineCapMode caps, LineJoinMode join, float miterLimit)
            : this(width, caps, join, miterLimit, null, 0)
        {
        }

        // Make a pen with dashes.
        // width, caps, join, miterLimit: as for a pen without dashes.
        // dashPattern: the lengths of the dashes and the gaps in turn, a dash first, an even number of them, or null for
        //   a pen without dashes. It is copied.
        // dashPhase: the distance that the pattern has gone at the start of the shape.
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

        // The lengths of the dashes and the gaps in turn, a dash first, or null for a pen without dashes.
        public IReadOnlyList<float> DashPattern
        {
            get { return (dashPattern == null) ? null : Array.AsReadOnly(dashPattern); }
        }

        // The distance that the dash pattern has gone at the start of the shape.
        public float DashPhase
        {
            get { return dashPhase; }
        }

        // The pen of the same width, caps, join and miter limit without dashes.
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
            string pen = string.Format("width {0}, {1} caps, {2} joins, miter limit {3}", width, caps, join, miterLimit);
            return (dashPattern == null) ? pen : string.Format("{0}, dashes {1} from {2}", pen, string.Join(" ", dashPattern), dashPhase);
        }
    }

    // The placement of a shape in the coordinates of the map. A placement gives one or more poses, and each reader asks
    // a placement for its poses. A pose is a point and an angle. A placement keeps the affine transformations that place
    // it, as its steps, and the pose is the innermost step: a single placement (SinglePlacement) has one pose, and a
    // repeated placement (RepeatedPlacement) calculates its poses along a path. Only an encoding that gives a repeated
    // placement to an abstraction of its channel reads its path and its step.
    public abstract class Placement
    {
        // The placement of the shapes that no transformation places.
        public static readonly SinglePlacement Identity = SinglePlacement.MakeIdentity();

        private protected Placement()
        {
        }

        // Copies of the steps, the outermost first: the affine transformations in the order in which the symbol set them.
        // A single placement has its pose as its innermost step. The steps of a repeated placement place its path, and
        // each of its poses is a step within them.
        public abstract IReadOnlyList<Matrix> Steps { get; }

        // The poses of the placement.
        public abstract IReadOnlyList<Pose> Poses { get; }

        // The bounds of a rectangle of the coordinates of the shape, in the coordinates of the map, at each pose.
        // rect: the rectangle.
        internal abstract RectangleF TransformBounds(RectangleF rect);
    }

    // A pose: a point and an angle, the rotation and the translation that place a shape. The angle turns the shape about
    // its origin, from the X axis toward the Y axis, in degrees, and the point is where the origin goes.
    public struct Pose
    {
        // The pose of no rotation and no translation.
        public static readonly Pose Origin = new Pose(new PointF(0, 0), 0);

        private readonly PointF location;
        private readonly float angle;

        // Make a pose.
        // location: the point where the origin goes.
        // angle: the angle, in degrees.
        public Pose(PointF location, float angle)
        {
            this.location = location;
            this.angle = angle;
        }

        public PointF Location
        {
            get { return location; }
        }

        public float Angle
        {
            get { return angle; }
        }

        // The affine transformation of the pose: it turns about the origin by the angle and moves the origin to the
        // point, as a glyph is placed (Glyph.Draw).
        public Matrix ToMatrix()
        {
            Matrix matrix = new Matrix();
            matrix.Translate(location.X, location.Y);
            matrix.RotateAt(angle, new PointF(0, 0));
            return matrix;
        }

        // The pose of an affine transformation that is a rotation and a translation, or null if the transformation is
        // not one. The rounding of floats is allowed.
        // matrix: the transformation.
        internal static Pose? FromMatrix(Matrix matrix)
        {
            float[] e = matrix.Elements;
            const float tolerance = 1E-5F;
            if (Math.Abs(e[0] - e[3]) > tolerance || Math.Abs(e[1] + e[2]) > tolerance || Math.Abs(e[0] * e[3] - e[1] * e[2] - 1) > tolerance)
                return null;
            float angle = (float)(Math.Atan2(e[1], e[0]) * 180 / Math.PI);
            return new Pose(new PointF(e[4], e[5]), angle);
        }

        public override string ToString()
        {
            return string.Format("pose at {0}, angle {1}", location, angle);
        }
    }

    // A single placement: the affine placement of a shape in the coordinates of the map, with one pose. The drawing code
    // of a symbol places a shape with nested transformations. A single placement keeps them as its steps, the outermost
    // first, in the order in which the symbol set them, and it composes them into one matrix. A channel can apply the
    // steps as the symbols applied them, with the same rounding. The shapes that a symbol places with one transformation
    // share their placement. The pose is the innermost step, and a placement without steps has the pose Pose.Origin.
    public sealed class SinglePlacement : Placement
    {
        private readonly SinglePlacement outer;     // The placement within which the step applies, or null.
        private readonly Matrix step;               // The transformation that this placement adds to the outer one.
        private readonly Matrix composition;        // The composition of the steps.

        private SinglePlacement(SinglePlacement outer, Matrix step, Matrix composition)
        {
            this.outer = outer;
            this.step = step;
            this.composition = composition;
        }

        // The placement without steps, Placement.Identity.
        internal static SinglePlacement MakeIdentity()
        {
            return new SinglePlacement(null, null, new Matrix());
        }

        // The placement of the shapes that a transformation places within this placement. The transformation applies
        // to a shape before this placement does, as IGraphicsTarget.PushTransform prepends a transformation.
        // step: the transformation. It is copied.
        public SinglePlacement Within(Matrix step)
        {
            if (step == null)
                throw new ArgumentNullException(nameof(step));

            Matrix composition = this.composition.Clone();
            composition.Multiply(step, MatrixOrder.Prepend);
            return new SinglePlacement(this, step.Clone(), composition);
        }

        // A copy of the composition of the steps: the affine transformation from the coordinates of the shape to those
        // of the map.
        public Matrix Matrix
        {
            get { return composition.Clone(); }
        }

        public override IReadOnlyList<Matrix> Steps
        {
            get
            {
                List<Matrix> steps = new List<Matrix>();
                for (SinglePlacement placement = this; placement.outer != null; placement = placement.outer)
                    steps.Add(placement.step.Clone());
                steps.Reverse();
                return steps.AsReadOnly();
            }
        }

        // The one pose: the innermost step. The placements that the geometry methods of the symbols make have a
        // rotation and a translation as their innermost step: the mirror of the Y axis of a text is a part of its text
        // run (TextShape). The collector, the oracle of the tests, keeps the steps of the drawing, which mirror the Y
        // axis to lay out a text, and a placement whose innermost step is not a pose has no poses to give.
        public override IReadOnlyList<Pose> Poses
        {
            get
            {
                if (outer == null)
                    return new Pose[] { Pose.Origin };

                Pose? pose = Pose.FromMatrix(step);
                if (!pose.HasValue)
                    throw new InvalidOperationException("The innermost step of the placement is not a rotation and a translation.");
                return new Pose[] { pose.Value };
            }
        }

        internal override RectangleF TransformBounds(RectangleF rect)
        {
            return Geometry.BoundsOfTransformedRectangle(rect, composition);
        }
    }

    // A repeated placement: the evenly spaced poses along a path, as the evenly spaced glyphs of a part of a line. It
    // keeps its path, its first position along the path, its step and its count, and it calculates its poses from the
    // path with the method of the drawing (SymPath.FindPointsAlongLine): the point of the path at each position, and the
    // direction of the path there. The positions and the step are euclidean along the path, as the 1D path effect of
    // Skia and the markers of SVG measure a path. A single placement places the path, and each pose is a step within it.
    public sealed class RepeatedPlacement : Placement
    {
        private readonly SinglePlacement outer;
        private readonly PathShape path;
        private readonly float first;
        private readonly float step;
        private readonly int count;
        private readonly bool isTurnedToPath;
        private IReadOnlyList<Pose> poses;          // The poses, after the first read calculated them.

        // Make a repeated placement.
        // outer: the placement of the path.
        // path: the path, one figure of lines.
        // first: the position of the first pose along the path, from its start.
        // step: the distance along the path from one pose to the next.
        // count: the number of poses, at least two.
        // isTurnedToPath: whether each pose turns to the direction of the path there, or keeps the angle 0.
        public RepeatedPlacement(SinglePlacement outer, PathShape path, float first, float step, int count, bool isTurnedToPath)
        {
            if (outer == null)
                throw new ArgumentNullException(nameof(outer));
            if (path == null)
                throw new ArgumentNullException(nameof(path));
            if (count < 2)
                throw new ArgumentOutOfRangeException(nameof(count), "A repeated placement has at least two poses.");

            this.outer = outer;
            this.path = path;
            this.first = first;
            this.step = step;
            this.count = count;
            this.isTurnedToPath = isTurnedToPath;
        }

        // The placement of the path.
        public SinglePlacement Outer
        {
            get { return outer; }
        }

        // The path along which the poses are.
        public PathShape Path
        {
            get { return path; }
        }

        // The position of the first pose along the path, from its start.
        public float First
        {
            get { return first; }
        }

        // The distance along the path from one pose to the next.
        public float Step
        {
            get { return step; }
        }

        // The number of poses.
        public int Count
        {
            get { return count; }
        }

        // Whether each pose turns to the direction of the path there, as the rotation of the 1D path effect of Skia and
        // the automatic orientation of an SVG marker do. A line does not turn a glyph of one circle at its origin, as its
        // drawing draws that glyph at each point without a transformation (Glyph.DrawSimple).
        public bool IsTurnedToPath
        {
            get { return isTurnedToPath; }
        }

        public override IReadOnlyList<Matrix> Steps
        {
            get { return outer.Steps; }
        }

        // The poses, which the first read calculates, along the path from the first position at the step.
        public override IReadOnlyList<Pose> Poses
        {
            get
            {
                IReadOnlyList<Pose> calculated = Volatile.Read(ref poses);
                if (calculated == null) {
                    calculated = CalculatePoses();
                    calculated = Interlocked.CompareExchange(ref poses, calculated, null) ?? calculated;
                }

                return calculated;
            }
        }

        // The expansion of the repeated placement: the single placement of each pose, within the placement of the path.
        public IReadOnlyList<SinglePlacement> Expand()
        {
            IReadOnlyList<Pose> poses = Poses;
            SinglePlacement[] placements = new SinglePlacement[poses.Count];
            for (int i = 0; i < poses.Count; ++i)
                placements[i] = outer.Within(poses[i].ToMatrix());
            return placements;
        }

        // Calculate the poses along the path, with the method of the drawing.
        private IReadOnlyList<Pose> CalculatePoses()
        {
            List<PointF> points = new List<PointF>();
            foreach (GraphicsPathPart part in path.Parts) {
                if (part.Points != null)
                    points.AddRange(part.Points);
            }

            float[] distances = new float[count];
            distances[0] = first;
            for (int i = 1; i < count; ++i)
                distances[i] = step;
            float[] angles;
            PointF[] locations = new SymPath(points.ToArray()).FindPointsAlongLine(distances, out angles, SymPath.EuclidDistance);

            Pose[] calculated = new Pose[count];
            for (int i = 0; i < count; ++i)
                calculated[i] = new Pose(locations[i], isTurnedToPath ? angles[i] : 0);
            return Array.AsReadOnly(calculated);
        }

        internal override RectangleF TransformBounds(RectangleF rect)
        {
            RectangleF bounds = RectangleF.Empty;
            bool isFirst = true;
            foreach (SinglePlacement placement in Expand()) {
                RectangleF placed = placement.TransformBounds(rect);
                bounds = isFirst ? placed : RectangleF.Union(bounds, placed);
                isFirst = false;
            }
            return bounds;
        }

        public override string ToString()
        {
            return string.Format("{0} poses by {1} from {2} along {3}{4}", count, step, first, path, isTurnedToPath ? ", turned to the path" : "");
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

        // The corners of a rectangle, clockwise from the top left corner, as a target makes the path of a rectangle
        // (IGraphicsTarget.FillRectangle and DrawRectangle).
        // rect: the rectangle.
        internal static PointF[] GetCorners(RectangleF rect)
        {
            return new PointF[] {
                new PointF(rect.Left, rect.Top), new PointF(rect.Right, rect.Top),
                new PointF(rect.Right, rect.Bottom), new PointF(rect.Left, rect.Bottom) };
        }

        // The parts of the path. Their points must not change.
        public IReadOnlyList<GraphicsPathPart> Parts
        {
            get { return parts; }
        }

        // The points of the path when it is one open figure of lines, a polyline, from its start, or null.
        internal PointF[] GetPolyline()
        {
            if (parts.Count == 1 && parts[0].Kind == GraphicsPathPartKind.Start)
                return (PointF[])parts[0].Points.Clone();
            if (parts.Count != 2 || parts[0].Kind != GraphicsPathPartKind.Start || parts[1].Kind != GraphicsPathPartKind.Lines)
                return null;

            List<PointF> points = new List<PointF>(parts[0].Points);
            points.AddRange(parts[1].Points);
            return points.ToArray();
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

    // An offset path: the shape of a border of a double line in its ideal form, its base path moved aside by an offset,
    // with a miter limit as the rule of its corners. Its offset curve is a derived value, which the first read makes as
    // the drawing moves a path aside (SymPath.OffsetRight): the flattened base path, each point moved to the right by
    // the offset, times the miter factor of its corner up to the miter limit. The dash pattern of a stroked offset path
    // is measured along its base path, so its dashes are the dashes of the base path moved aside.
    public sealed class OffsetPathShape : Shape
    {
        private readonly SymPath basePath;
        private readonly PathShape basePathShape;
        private readonly float offset;
        private readonly float miterLimit;
        private PathShape offsetCurve;      // The offset curve, after the first read made it.

        // Make an offset path.
        // basePath: the base path. It does not change.
        // offset: the distance of the offset path to the right of the base path, negative to the left.
        // miterLimit: the longest offset of a corner, as a multiple of the offset.
        internal OffsetPathShape(SymPath basePath, float offset, float miterLimit)
        {
            if (basePath == null)
                throw new ArgumentNullException(nameof(basePath));

            this.basePath = basePath;
            this.basePathShape = basePath.MakePathShape();
            this.offset = offset;
            this.miterLimit = miterLimit;
        }

        // The base path, as the path that a pen draws along it.
        public PathShape BasePath
        {
            get { return basePathShape; }
        }

        // The distance of the offset path to the right of the base path, negative to the left.
        public float Offset
        {
            get { return offset; }
        }

        // The longest offset of a corner, as a multiple of the offset.
        public float MiterLimit
        {
            get { return miterLimit; }
        }

        // The offset curve: the base path moved aside, as a path of lines. The first read makes it.
        public PathShape OffsetCurve
        {
            get
            {
                PathShape made = Volatile.Read(ref offsetCurve);
                if (made == null) {
                    made = basePath.OffsetRight(offset, miterLimit).MakePathShape();
                    made = Interlocked.CompareExchange(ref offsetCurve, made, null) ?? made;
                }

                return made;
            }
        }

        // The bounds of the offset curve.
        public override RectangleF Bounds
        {
            get { return OffsetCurve.Bounds; }
        }

        // Move the points of a dash along the base path aside as the offset path moves its base path.
        // points: the points of the dash, from its start to its end.
        internal PointF[] MoveAside(PointF[] points)
        {
            return SymPath.OffsetPointsRight(points, offset, miterLimit);
        }

        public override string ToString()
        {
            return string.Format("offset path of {0}, offset {1}, miter limit {2}", basePathShape, offset, miterLimit);
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
    // corner of the text at a point. The point is in the coordinates of the placement of the run, whose Y axis points
    // up as the axis of the map does, and the glyphs of the run lie below the point, along the X axis. The drawing lays
    // out a text with its Y axis down (IGraphicsTarget.DrawText). So the run is the text that the drawing draws at the
    // point with the opposite y coordinate, with the Y axis mirrored. The mirror of the Y axis is a part of the run,
    // and not of its placement.
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
        // upperLeft: the top left corner of the text, in coordinates whose Y axis points up.
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

        // The top left corner of the text, in coordinates whose Y axis points up.
        public PointF UpperLeft
        {
            get { return upperLeft; }
        }

        // The bounds of the outlines of the glyphs of the text, as the metrics of its font measure them, below the top
        // left corner. A text without visible glyphs has empty bounds at its top left corner. The first read measures
        // them, and the readers that measure them at the same time get the same bounds.
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

        // Measure the bounds of the outlines of the glyphs of the text, and box them. The metrics measure them where the
        // drawing lays out the text, with its Y axis down, and the bounds are their mirror.
        private object MeasureBounds()
        {
            RectangleF glyphBounds;
            using (ITextFaceMetrics faceMetrics = textMetrics.GetTextFaceMetrics(fontFamily, emHeight, effects))
                glyphBounds = faceMetrics.GetTightBoundingBox(new PointF(upperLeft.X, -upperLeft.Y), text);
            if (glyphBounds.Width > 0 || glyphBounds.Height > 0)
                return RectangleF.FromLTRB(glyphBounds.Left, -glyphBounds.Bottom, glyphBounds.Right, -glyphBounds.Top);
            else
                return new RectangleF(upperLeft, SizeF.Empty);
        }

        public override string ToString()
        {
            return string.Format("text \"{0}\" in {1} {2} {3} at {4}", text, fontFamily, emHeight, effects, upperLeft);
        }
    }
}
