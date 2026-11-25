using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using PurplePen.Graphics2D;

namespace Map_Svg.Tests
{
    // Describes what each drawing call draws, as Graphics2D defines it, in the coordinates of a page (V-10): a graphics
    // target into which a case's recording is played back, as it is played back into the SVG target, and which keeps a
    // DrawnGeometry of each call that draws, in the order of the calls. A path is described by its parts as Skia takes them,
    // a segment with no point before it starting at the origin; an arc by its ends, its radius and whether it is the
    // larger and turns towards positive angles on the page; an ellipse by its center and the ends of its axes; a text
    // by its characters as a document holds them, the start of its baseline, below its upper left corner by the font's
    // ascent, and the point an em along it; a bitmap by the corners of its rectangle. A call that draws nothing, a path
    // with no point or a text with no character a document can hold, is left out. The target of a pattern brush
    // describes its tile likewise, in steps of the tile, whose middle is its origin, as the SVG target draws it (D-7).
    // It hears the structure calls that bear on geometry: an area's outline drawn as a ghost is an unpainted drawing, and
    // a clip to, or a fill of, a path described as the union of areas' outlines is a clip to the union of the outlines,
    // each under its own rule, and a fill of the rectangle around the path's points within that clip, as the SVG target
    // writes them (M-6, S-12).
    public sealed class CallGeometryRecorder : IBrushTarget, IStructuredGraphicsTarget
    {
        // The steps in a millimetre, which a pattern's tile is described in.
        private const double StepsPerMillimetre = 1000;

        // What a brush paints with: a color, or the tile of a pattern turned by an angle.
        private sealed class Paint
        {
            // A color, whatever it is: the geometry check compares where ink goes, not its color.
            public static readonly Paint Color = new Paint(null, 0, 0, 0);

            // The pattern's tile, or null for a color.
            public readonly GeometryTile Tile;

            // The size of the tile, in the units of what it paints, and the angle it is turned by, in degrees.
            public readonly float Width, Height, Angle;

            // Create a paint.
            // tile: the pattern's tile, or null for a color.
            // width, height: the size of the tile, in the units of what it paints.
            // angle: the angle the tile is turned by, in degrees.
            public Paint(GeometryTile tile, float width, float height, float angle)
            {
                Tile = tile;
                Width = width;
                Height = height;
                Angle = angle;
            }
        }

        // A pen: the width of its stroke and what it paints with.
        private sealed class PenRecord
        {
            // The width of the stroke, in the units of what it strokes.
            public readonly float Width;

            // What the stroke paints with.
            public readonly Paint Paint;

            // Create a pen.
            // width: the width of its stroke.
            // paint: what its stroke paints with.
            public PenRecord(float width, Paint paint)
            {
                Width = width;
                Paint = paint;
            }
        }

        // A path created under a key: its parts and whether it is filled and clipped under the even-odd rule.
        private sealed class StoredPath
        {
            // The parts of the path.
            public readonly List<GraphicsPathPart> Parts;

            // Whether the path is filled and clipped under the even-odd rule rather than the nonzero one.
            public readonly bool EvenOdd;

            // Create a stored path.
            // parts: its parts.
            // evenOdd: whether it is filled and clipped under the even-odd rule.
            public StoredPath(List<GraphicsPathPart> parts, bool evenOdd)
            {
                Parts = parts;
                EvenOdd = evenOdd;
            }
        }

        // A font: its size, and its ascent, which puts the start of a text's baseline below its upper left corner.
        private sealed class FontRecord
        {
            // The size of the font, its em, and the ascent of its face, in the units of what it draws.
            public readonly float EmHeight, Ascent;

            // Create a font.
            // emHeight: its size.
            // ascent: the ascent of its face.
            public FontRecord(float emHeight, float ascent)
            {
                EmHeight = emHeight;
                Ascent = ascent;
            }
        }

        // The metrics the application lays text out with.
        private readonly ITextMetrics textMetrics;

        // The target that created this one as the target of a pattern brush, with the brush's size and angle; or null.
        private readonly CallGeometryRecorder creator;
        private readonly SizeF tileSize;
        private readonly float tileAngle;

        // The transform from the current frame to the page, and those of the frames around it.
        private Affine current;
        private readonly Stack<Affine> transforms = new Stack<Affine>();

        // The clips around what is drawn, outermost first, and those before each clip still open.
        private DrawnGeometry[] clips = Array.Empty<DrawnGeometry>();
        private readonly Stack<DrawnGeometry[]> clipsBefore = new Stack<DrawnGeometry[]>();

        // The paths, brushes, pens and fonts, by their keys.
        private readonly Dictionary<object, StoredPath> paths = new Dictionary<object, StoredPath>(ReferenceEqualityComparer.Instance);

        // The outlines that each path described as a union unites, by the union's key, in the order drawn.
        private readonly Dictionary<object, object[]> unions = new Dictionary<object, object[]>(ReferenceEqualityComparer.Instance);
        private readonly Dictionary<object, Paint> brushes = new Dictionary<object, Paint>(ReferenceEqualityComparer.Instance);
        private readonly Dictionary<object, PenRecord> pens = new Dictionary<object, PenRecord>(ReferenceEqualityComparer.Instance);
        private readonly Dictionary<object, FontRecord> fonts = new Dictionary<object, FontRecord>(ReferenceEqualityComparer.Instance);

        // The number of drawing calls made, which names each in messages.
        private int callsMade;

        // The intensity, which only colors depend on.
        private float intensity = 1;

        // The drawings, in the order of the calls.
        public readonly List<DrawnGeometry> Drawings = new List<DrawnGeometry>();

        // Create a recorder of the calls that draw a page.
        // textMetrics: the metrics the application lays text out with.
        // toPage: the transform from the drawing's coordinates to the page's.
        public CallGeometryRecorder(ITextMetrics textMetrics, Affine toPage)
        {
            this.textMetrics = textMetrics;
            current = toPage;
        }

        // Create the recorder of a pattern brush's tile, in steps of the tile.
        // creator: the recorder that creates the brush.
        // size: the size of the tile, in the units of what it paints.
        // angle: the angle it is turned by, in degrees.
        private CallGeometryRecorder(CallGeometryRecorder creator, SizeF size, float angle)
            : this(creator.textMetrics, new Affine(StepsPerMillimetre, 0, 0, StepsPerMillimetre, 0, 0))
        {
            this.creator = creator;
            tileSize = size;
            tileAngle = angle;
        }

        // The clip of a rectangle under a transform, as a page clips its parts to the area of the map it shows (L-3).
        // area: the rectangle.
        // toPage: the transform from its coordinates to the page's.
        // what: what the clip is, for messages.
        public static DrawnGeometry RectangleClip(RectangleF area, Affine toPage, string what)
        {
            DrawnGeometry clip = new DrawnGeometry(what);
            clip.Signature.Append("clip nonzero");
            AddRectangle(new PathGeometryBuilder(clip), area, toPage);
            return clip;
        }

        // The intensity at which colors are drawn; setting it discards the brushes and pens, as every target does.
        public float Intensity
        {
            get { return intensity; }
            set {
                brushes.Clear();
                pens.Clear();
                intensity = value;
            }
        }

        // Prepend a transform to the current one.
        public void PushTransform(Matrix matrix)
        {
            transforms.Push(current);
            current = current.After(Affine.FromMatrix(matrix));
        }

        // Go back to the transform before the last PushTransform.
        public void PopTransform()
        {
            current = transforms.Pop();
        }

        // Clip to a path created with CreatePath, under its rule, or to the union of the outlines it was described as.
        public void PushClip(object pathKey)
        {
            object[] outlines;
            if (unions.TryGetValue(pathKey, out outlines)) {
                OpenClip(UnionClip(outlines));
                return;
            }

            StoredPath path = paths[pathKey];
            DrawnGeometry clip = BeginClip(path.EvenOdd);
            AddParts(new PathGeometryBuilder(clip), path.Parts);
            OpenClip(clip);
        }

        // The clip to the union of outlines, as the SVG target writes it: a clip path of a use of each outline's ghost,
        // each under its own rule, in the order drawn.
        // outlines: the keys of the outlines' paths.
        private DrawnGeometry UnionClip(object[] outlines)
        {
            ++callsMade;
            DrawnGeometry clip = new DrawnGeometry("PushClip, call " + callsMade.ToString(CultureInfo.InvariantCulture) + ", to a union");
            clip.Signature.Append("clip union");
            foreach (object outline in outlines) {
                StoredPath path = paths[outline];
                clip.Signature.Append(path.EvenOdd ? "; even-odd" : "; nonzero");
                AddParts(new PathGeometryBuilder(clip), path.Parts);
            }
            return clip;
        }

        // Clip to a rectangle.
        public void PushClip(RectangleF rectangle)
        {
            DrawnGeometry clip = BeginClip(false);
            AddRectangle(new PathGeometryBuilder(clip), rectangle, current);
            OpenClip(clip);
        }

        // Clip to the union of rectangles, one subpath each.
        public void PushClip(RectangleF[] rectangles)
        {
            DrawnGeometry clip = BeginClip(false);
            PathGeometryBuilder path = new PathGeometryBuilder(clip);
            foreach (RectangleF rectangle in rectangles)
                AddRectangle(path, rectangle, current);
            OpenClip(clip);
        }

        // Clip to a path given by its parts, under the rule of its fill mode.
        public void PushClip(List<GraphicsPathPart> parts, AreaFillMode windingMode)
        {
            DrawnGeometry clip = BeginClip(windingMode == AreaFillMode.Alternate);
            AddParts(new PathGeometryBuilder(clip), parts);
            OpenClip(clip);
        }

        // Go back to the clips before the last PushClip.
        public void PopClip()
        {
            clips = clipsBefore.Pop();
        }

        // Anti-aliasing changes nothing that the check compares.
        public void PushAntiAliasing(bool antiAlias)
        {
        }

        // Anti-aliasing changes nothing that the check compares.
        public void PopAntiAliasing()
        {
        }

        // Blending changes nothing that the check compares; it is taken, as the SVG target takes darken.
        public bool PushBlending(BlendMode blendMode)
        {
            return true;
        }

        // Blending changes nothing that the check compares.
        public void PopBlending()
        {
        }

        // Create a path under a key.
        public void CreatePath(object key, List<GraphicsPathPart> parts, AreaFillMode windingMode)
        {
            paths.Add(key, new StoredPath(parts, windingMode == AreaFillMode.Alternate));
        }

        // Create a brush of a color under a key.
        public void CreateSolidBrush(object key, CmykColor color)
        {
            brushes.Add(key, Paint.Color);
        }

        // Pattern brushes are supported, as the SVG target supports them.
        public bool SupportsPatternBrushes
        {
            get { return true; }
        }

        // Create a pattern brush: return the recorder of its tile, which FinishBrush makes a brush of this one.
        public IBrushTarget CreatePatternBrush(SizeF size, float angle, int bitmapWidth, int bitmapHeight)
        {
            return new CallGeometryRecorder(this, size, angle);
        }

        // Make the tile a brush of the recorder that created it, under a key.
        public void FinishBrush(object brushKey)
        {
            List<DrawnGeometry> tileDrawings = Drawings;
            GeometryTile tile = new GeometryTile("the tile of a pattern brush", () => tileDrawings);
            creator.brushes.Add(brushKey, new Paint(tile, tileSize.Width, tileSize.Height, tileAngle));
        }

        // Create a pen that strokes with a brush's paint, under a key.
        public void CreatePen(object key, object brushKey, float width, LineCapMode caps, LineJoinMode join, float miterLimit)
        {
            pens.Add(key, new PenRecord(width, brushes[brushKey]));
        }

        // Create a pen of a color, under a key.
        public void CreatePen(object key, CmykColor color, float width, LineCapMode caps, LineJoinMode join, float miterLimit)
        {
            pens.Add(key, new PenRecord(width, Paint.Color));
        }

        // Create a font under a key, with the ascent of its face in the metrics the application lays text out with.
        public void CreateFont(object key, string familyName, float emHeight, TextEffects effects)
        {
            using (ITextFaceMetrics metrics = textMetrics.GetTextFaceMetrics(familyName, emHeight, effects))
                fonts.Add(key, new FontRecord(emHeight, metrics.Ascent));
        }

        // Whether a path is created under a key.
        public bool HasPath(object pathKey)
        {
            return paths.ContainsKey(pathKey);
        }

        // Whether a brush is created under a key.
        public bool HasBrush(object brushKey)
        {
            return brushes.ContainsKey(brushKey);
        }

        // Whether a pen is created under a key.
        public bool HasPen(object penKey)
        {
            return pens.ContainsKey(penKey);
        }

        // Whether a font is created under a key.
        public bool HasFont(object fontKey)
        {
            return fonts.ContainsKey(fontKey);
        }

        // A line.
        public void DrawLine(object penKey, PointF start, PointF finish)
        {
            DrawnGeometry drawing = BeginStroke(nameof(DrawLine), penKey);
            PathGeometryBuilder path = new PathGeometryBuilder(drawing);
            path.MoveTo(current.Apply(start.X, start.Y), 0);
            path.LineTo(current.Apply(finish.X, finish.Y), 0);
            Finish(drawing, pens[penKey].Paint);
        }

        // An arc, its angles in degrees from the x axis towards the y axis; a full turn or more is the whole circle.
        public void DrawArc(object penKey, PointF center, float radius, float startAngle, float sweepAngle)
        {
            DrawnGeometry drawing = BeginStroke(nameof(DrawArc), penKey);
            if (Math.Abs(sweepAngle) >= 360) {
                AddEllipse(drawing, center, radius, radius);
            }
            else {
                double start = startAngle * Math.PI / 180, end = (startAngle + sweepAngle) * Math.PI / 180;
                PathGeometryBuilder path = new PathGeometryBuilder(drawing);
                path.MoveTo(current.Apply(center.X + radius * Math.Cos(start), center.Y + radius * Math.Sin(start)), 0);
                path.ArcTo(Math.Abs(radius) * current.LengthScale, 0, Math.Abs(sweepAngle) > 180, (sweepAngle > 0) != current.Mirrors,
                           current.Apply(center.X + radius * Math.Cos(end), center.Y + radius * Math.Sin(end)), 0);
            }
            Finish(drawing, pens[penKey].Paint);
        }

        // The outline of an ellipse.
        public void DrawEllipse(object penKey, PointF center, float radiusX, float radiusY)
        {
            DrawnGeometry drawing = BeginStroke(nameof(DrawEllipse), penKey);
            AddEllipse(drawing, center, radiusX, radiusY);
            Finish(drawing, pens[penKey].Paint);
        }

        // A filled ellipse.
        public void FillEllipse(object brushKey, PointF center, float radiusX, float radiusY)
        {
            DrawnGeometry drawing = BeginFill(nameof(FillEllipse), false);
            AddEllipse(drawing, center, radiusX, radiusY);
            Finish(drawing, brushes[brushKey]);
        }

        // The outline of a rectangle.
        public void DrawRectangle(object penKey, RectangleF rect)
        {
            DrawnGeometry drawing = BeginStroke(nameof(DrawRectangle), penKey);
            AddRectangle(new PathGeometryBuilder(drawing), rect, current);
            Finish(drawing, pens[penKey].Paint);
        }

        // A filled rectangle.
        public void FillRectangle(object brushKey, RectangleF rect)
        {
            DrawnGeometry drawing = BeginFill(nameof(FillRectangle), false);
            AddRectangle(new PathGeometryBuilder(drawing), rect, current);
            Finish(drawing, brushes[brushKey]);
        }

        // The outline of a closed polygon.
        public void DrawPolygon(object penKey, PointF[] pts)
        {
            if (pts.Length == 0)
                return;
            DrawnGeometry drawing = BeginStroke(nameof(DrawPolygon), penKey);
            AddPolygon(drawing, pts, true);
            Finish(drawing, pens[penKey].Paint);
        }

        // An open polyline.
        public void DrawPolyline(object penKey, PointF[] pts)
        {
            if (pts.Length == 0)
                return;
            DrawnGeometry drawing = BeginStroke(nameof(DrawPolyline), penKey);
            AddPolygon(drawing, pts, false);
            Finish(drawing, pens[penKey].Paint);
        }

        // A filled polygon, under the rule of its fill mode.
        public void FillPolygon(object brushKey, PointF[] pts, AreaFillMode windingMode)
        {
            if (pts.Length == 0)
                return;
            DrawnGeometry drawing = BeginFill(nameof(FillPolygon), windingMode == AreaFillMode.Alternate);
            AddPolygon(drawing, pts, true);
            Finish(drawing, brushes[brushKey]);
        }

        // The outline of a path created with CreatePath.
        public void DrawPath(object penKey, object pathKey)
        {
            AddPath(BeginStroke(nameof(DrawPath), penKey), paths[pathKey].Parts, pens[penKey].Paint);
        }

        // The outline of a path given by its parts.
        public void DrawPath(object penKey, List<GraphicsPathPart> parts)
        {
            AddPath(BeginStroke(nameof(DrawPath), penKey), parts, pens[penKey].Paint);
        }

        // A filled path created with CreatePath, under its rule; or, for a path described as the union of outlines, the
        // rectangle around the path's points, control points included, filled within the clip to their union.
        public void FillPath(object brushKey, object pathKey)
        {
            StoredPath path = paths[pathKey];
            object[] outlines;
            if (unions.TryGetValue(pathKey, out outlines)) {
                RectangleF bounds = BoundsOf(path.Parts);
                OpenClip(UnionClip(outlines));
                DrawnGeometry drawing = BeginFill(nameof(FillPath), false);
                AddRectangle(new PathGeometryBuilder(drawing), bounds, current);
                Finish(drawing, brushes[brushKey]);
                PopClip();
                return;
            }

            AddPath(BeginFill(nameof(FillPath), path.EvenOdd), path.Parts, brushes[brushKey]);
        }

        // The rectangle around the points of a path's parts, control points included.
        // parts: the parts.
        private static RectangleF BoundsOf(List<GraphicsPathPart> parts)
        {
            float left = float.MaxValue, top = float.MaxValue, right = float.MinValue, bottom = float.MinValue;
            foreach (GraphicsPathPart part in parts) {
                if (part.Points == null)
                    continue;
                foreach (PointF point in part.Points) {
                    left = Math.Min(left, point.X);
                    top = Math.Min(top, point.Y);
                    right = Math.Max(right, point.X);
                    bottom = Math.Max(bottom, point.Y);
                }
            }
            return left > right ? RectangleF.Empty : RectangleF.FromLTRB(left, top, right, bottom);
        }

        // Beginning a part, which draws nothing, changes nothing that the check compares.
        public void BeginPart(object partKey)
        {
        }

        // Ending a part changes nothing that the check compares.
        public void EndPart()
        {
        }

        // Beginning a map changes nothing that the check compares.
        public void BeginMap(object mapKey, string label)
        {
        }

        // Ending a map changes nothing that the check compares.
        public void EndMap()
        {
        }

        // Beginning a color run changes nothing that the check compares.
        public void BeginColorRun(object colorKey, int position, string name, CmykColor color, bool overprint)
        {
        }

        // Ending a color run changes nothing that the check compares.
        public void EndColorRun()
        {
        }

        // Beginning an object changes nothing that the check compares.
        public void BeginObject(object objectKey, object symbolKey, string symbolId, string symbolName)
        {
        }

        // Ending an object changes nothing that the check compares.
        public void EndObject()
        {
        }

        // An area's outline, drawn as a ghost: an unpainted drawing of its path.
        public void DrawOutline(object pathKey)
        {
            StoredPath path = paths[pathKey];
            DrawnGeometry drawing = Begin(nameof(DrawOutline), "unpainted");
            PathGeometryBuilder builder = new PathGeometryBuilder(drawing);
            AddParts(builder, path.Parts);
            if (builder.HasBegun)
                Drawings.Add(drawing);
        }

        // Note that a path is the union of the outlines of areas, which a clip to it and a fill of it then stand for.
        public void DescribeUnion(object unionPathKey, object[] pathKeys)
        {
            unions[unionPathKey] = (object[])pathKeys.Clone();
        }

        // Adding a page, which the pages' own clips are compared through, changes nothing that the check compares.
        public void AddPage(SizeF paperSize, RectangleF mapArea, Matrix mapToPage, object[] partKeys, string name)
        {
        }

        // Where a map lies in the real world changes nothing that the check compares.
        public void SetGeoreference(object mapKey, MapGeoreference georeference)
        {
        }

        // The course a map draws changes nothing that the check compares.
        public void DescribeCourse(object mapKey, CourseRecord course)
        {
        }

        // The course's objects change nothing that the check compares.
        public void DescribeObjects(object mapKey, IReadOnlyDictionary<object, CourseObjectRecord> objects)
        {
        }

        // Pattern brushes are vectors, as the SVG target's are, though the map code draws nothing into the recorder.
        public bool PatternBrushesAreVectors
        {
            get { return true; }
        }

        // A filled path given by its parts, under the rule of its fill mode.
        public void FillPath(object brushKey, List<GraphicsPathPart> parts, AreaFillMode windingMode)
        {
            AddPath(BeginFill(nameof(FillPath), windingMode == AreaFillMode.Alternate), parts, brushes[brushKey]);
        }

        // A filled text.
        public void DrawText(string text, object fontKey, object brushKey, PointF upperLeft)
        {
            AddText(nameof(DrawText), "text", text, fontKey, upperLeft, brushes[brushKey], null);
        }

        // The outline of a text.
        public void DrawTextOutline(string text, object fontKey, object penKey, PointF upperLeft)
        {
            PenRecord pen = pens[penKey];
            AddText(nameof(DrawTextOutline), "text outline", text, fontKey, upperLeft, pen.Paint, pen);
        }

        // A bitmap over a rectangle.
        public void DrawBitmap(IGraphicsBitmap bm, RectangleF rectangle, BitmapScaling scalingMode)
        {
            AddImage(nameof(DrawBitmap), rectangle);
        }

        // Part of a bitmap over a rectangle.
        public void DrawBitmapPart(IGraphicsBitmap bm, int x, int y, int width, int height, RectangleF rectangle, BitmapScaling scalingMode)
        {
            AddImage(nameof(DrawBitmapPart), rectangle);
        }

        // Nothing to release.
        public void Dispose()
        {
        }

        // Begin the geometry of a drawing call.
        // callName: the name of the call.
        // kind: the kind and paint of what it draws.
        private DrawnGeometry Begin(string callName, string kind)
        {
            ++callsMade;
            DrawnGeometry drawing = new DrawnGeometry(callName + ", call " + callsMade.ToString(CultureInfo.InvariantCulture) +
                                            (creator == null ? "" : " of a pattern's tile"));
            drawing.Signature.Append(kind);
            drawing.Clips = clips;
            return drawing;
        }

        // Begin the geometry of a call that strokes with a pen: its width, scaled to the page.
        // callName: the name of the call.
        // penKey: the key of the pen.
        private DrawnGeometry BeginStroke(string callName, object penKey)
        {
            DrawnGeometry drawing = Begin(callName, "stroke");
            drawing.AddLength(pens[penKey].Width * current.LengthScale, 0);
            return drawing;
        }

        // Begin the geometry of a call that fills under a rule.
        // callName: the name of the call.
        // evenOdd: whether it fills under the even-odd rule rather than the nonzero one.
        private DrawnGeometry BeginFill(string callName, bool evenOdd)
        {
            return Begin(callName, evenOdd ? "fill even-odd" : "fill nonzero");
        }

        // Begin the geometry of a clip under a rule.
        // evenOdd: whether it clips under the even-odd rule rather than the nonzero one.
        private DrawnGeometry BeginClip(bool evenOdd)
        {
            ++callsMade;
            DrawnGeometry clip = new DrawnGeometry("PushClip, call " + callsMade.ToString(CultureInfo.InvariantCulture) +
                                         (creator == null ? "" : " of a pattern's tile"));
            clip.Signature.Append(evenOdd ? "clip even-odd" : "clip nonzero");
            return clip;
        }

        // Open a clip around what is drawn until the matching PopClip.
        // clip: the clip.
        private void OpenClip(DrawnGeometry clip)
        {
            clipsBefore.Push(clips);
            DrawnGeometry[] opened = new DrawnGeometry[clips.Length + 1];
            clips.CopyTo(opened, 0);
            opened[clips.Length] = clip;
            clips = opened;
        }

        // Finish a drawing that paints with a paint, and keep it: a pattern's paint adds its lattice, the tile's middle
        // and the ends of half its sides, turned by its angle, in the frame of what it paints.
        // drawing: the drawing.
        // paint: what it paints with.
        private void Finish(DrawnGeometry drawing, Paint paint)
        {
            if (paint.Tile != null) {
                drawing.Signature.Append(" pattern");
                Affine lattice = current.After(Affine.Rotation(paint.Angle));
                drawing.AddPoint(lattice.Apply(0, 0), 0);
                drawing.AddPoint(lattice.Apply(paint.Width / 2.0, 0), 0);
                drawing.AddPoint(lattice.Apply(0, paint.Height / 2.0), 0);
                drawing.Pattern = paint.Tile;
            }
            Drawings.Add(drawing);
        }

        // Add an ellipse: its center and the ends of its axes.
        // drawing: the drawing.
        // center: the center.
        // radiusX, radiusY: the radii.
        private void AddEllipse(DrawnGeometry drawing, PointF center, float radiusX, float radiusY)
        {
            drawing.Signature.Append(" ellipse");
            drawing.AddPoint(current.Apply(center.X, center.Y), 0);
            drawing.AddPoint(current.Apply(center.X + Math.Abs((double)radiusX), center.Y), 0);
            drawing.AddPoint(current.Apply(center.X, center.Y + Math.Abs((double)radiusY)), 0);
        }

        // Add a polygon or a polyline through points.
        // drawing: the drawing.
        // points: the points.
        // close: whether the path closes.
        private void AddPolygon(DrawnGeometry drawing, PointF[] points, bool close)
        {
            PathGeometryBuilder path = new PathGeometryBuilder(drawing);
            path.MoveTo(current.Apply(points[0].X, points[0].Y), 0);
            for (int index = 1; index < points.Length; ++index)
                path.LineTo(current.Apply(points[index].X, points[index].Y), 0);
            if (close)
                path.Close();
        }

        // Add a path given by its parts, and keep the drawing unless the path has no point.
        // drawing: the drawing.
        // parts: the parts.
        // paint: what the drawing paints with.
        private void AddPath(DrawnGeometry drawing, List<GraphicsPathPart> parts, Paint paint)
        {
            PathGeometryBuilder path = new PathGeometryBuilder(drawing);
            AddParts(path, parts);
            if (path.HasBegun)
                Finish(drawing, paint);
        }

        // Add the parts of a path, as Skia takes them: a start begins a subpath, lines and Beziers continue it, a line
        // or curve with no point before it starting at the origin, and a close closes it.
        // path: the builder of the path.
        // parts: the parts.
        private void AddParts(PathGeometryBuilder path, List<GraphicsPathPart> parts)
        {
            foreach (GraphicsPathPart part in parts) {
                switch (part.Kind) {
                    case GraphicsPathPartKind.Start:
                        path.MoveTo(current.Apply(part.Points[0].X, part.Points[0].Y), 0);
                        break;
                    case GraphicsPathPartKind.Lines:
                        if (!path.HasBegun)
                            path.MoveTo(current.Apply(0, 0), 0);
                        foreach (PointF point in part.Points)
                            path.LineTo(current.Apply(point.X, point.Y), 0);
                        break;
                    case GraphicsPathPartKind.Beziers:
                        if (!path.HasBegun)
                            path.MoveTo(current.Apply(0, 0), 0);
                        for (int index = 0; index + 2 < part.Points.Length; index += 3) {
                            path.CubicTo(current.Apply(part.Points[index].X, part.Points[index].Y),
                                         current.Apply(part.Points[index + 1].X, part.Points[index + 1].Y),
                                         current.Apply(part.Points[index + 2].X, part.Points[index + 2].Y), 0);
                        }
                        break;
                    case GraphicsPathPartKind.Close:
                        path.Close();
                        break;
                }
            }
        }

        // Add a rectangle as a closed subpath from its top left corner along its top first, as Skia adds one.
        // path: the builder of the path.
        // rectangle: the rectangle.
        // toPage: the transform from its coordinates to the page's.
        private static void AddRectangle(PathGeometryBuilder path, RectangleF rectangle, Affine toPage)
        {
            path.MoveTo(toPage.Apply(rectangle.Left, rectangle.Top), 0);
            path.LineTo(toPage.Apply(rectangle.Right, rectangle.Top), 0);
            path.LineTo(toPage.Apply(rectangle.Right, rectangle.Bottom), 0);
            path.LineTo(toPage.Apply(rectangle.Left, rectangle.Bottom), 0);
            path.Close();
        }

        // Add a text: its characters as a document holds them, the start of its baseline and the point an em along it.
        // callName: the name of the call.
        // kind: the kind and paint of what it draws.
        // text: the text.
        // fontKey: the key of its font.
        // upperLeft: its upper left corner.
        // paint: what it paints with.
        // pen: the pen of its outline, or null for a filled text.
        private void AddText(string callName, string kind, string text, object fontKey, PointF upperLeft, Paint paint, PenRecord pen)
        {
            string characters = DocumentCharacters.Of(text);
            if (characters.Length == 0)
                return;
            FontRecord font = fonts[fontKey];
            DrawnGeometry drawing = Begin(callName, kind);
            if (pen != null)
                drawing.AddLength(pen.Width * current.LengthScale, 0);
            drawing.Signature.Append(" '").Append(characters).Append('\'');
            double baseline = (double)upperLeft.Y + font.Ascent;
            drawing.AddPoint(current.Apply(upperLeft.X, baseline), 0);
            drawing.AddPoint(current.Apply((double)upperLeft.X + font.EmHeight, baseline), 0);
            Finish(drawing, paint);
        }

        // Add a bitmap: the corners of its rectangle, from its top left corner along its top first.
        // callName: the name of the call.
        // rectangle: the rectangle.
        private void AddImage(string callName, RectangleF rectangle)
        {
            DrawnGeometry drawing = Begin(callName, "image");
            drawing.AddPoint(current.Apply(rectangle.Left, rectangle.Top), 0);
            drawing.AddPoint(current.Apply(rectangle.Right, rectangle.Top), 0);
            drawing.AddPoint(current.Apply(rectangle.Right, rectangle.Bottom), 0);
            drawing.AddPoint(current.Apply(rectangle.Left, rectangle.Bottom), 0);
            Drawings.Add(drawing);
        }
    }
}
