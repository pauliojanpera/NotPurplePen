using System;
using System.Collections.Generic;
using System.Drawing;
using PurplePen.Graphics2D;
using SkiaSharp;

namespace SkiaPathOpsProbe
{
    /// <summary>
    /// The marks of one colour that follow each other with no mark of another colour between them (Model.md).
    /// </summary>
    public sealed class Run
    {
        /// <summary>The place of the run in the drawing. The lowest run has index 0.</summary>
        public int Index;

        /// <summary>The colour of the brush or the pen of the marks.</summary>
        public CmykColor Colour;

        /// <summary>True when the marks were drawn between PushBlending and PopBlending.</summary>
        public bool IsInsideBlending;

        /// <summary>The names of the colours of the map that have the values of this run.</summary>
        public string ColourName = "";

        /// <summary>The marks, in the coordinates of the page, the lowest first.</summary>
        public List<SKPath> Marks = new List<SKPath>();
    }

    /// <summary>
    /// The counts of what the target met and did not keep as marks.
    /// </summary>
    public sealed class NotKeptCounts
    {
        /// <summary>The calls of DrawText and DrawTextOutline.</summary>
        public int TextCalls;

        /// <summary>The calls of DrawBitmap and DrawBitmapPart.</summary>
        public int BitmapCalls;

        /// <summary>The calls of CreatePatternBrush, and the fills with such a brush.</summary>
        public int PatternBrushes, FillsWithPatternBrush;

        /// <summary>The calls of PushClip, those of them inside another clip, and those of them whose intersection with the outer clip gave no result.</summary>
        public int Clips, ClipIntersections, ClipIntersectionFailures;

        /// <summary>The marks whose bounds are outside the clip, the marks that an intersection cut to the clip, and the marks whose intersection gave no result.</summary>
        public int MarksOutsideClip, MarksClipped, MarkClipFailures;

        /// <summary>The calls of PushAntiAliasing, and those of them that changed the antialiasing.</summary>
        public int AntiAliasingPushes, AntiAliasingSwitches;

        /// <summary>The calls of PushBlending.</summary>
        public int BlendingPushes;

        /// <summary>The strokes with a pen of no width, which have no outline.</summary>
        public int HairlineStrokes;

        /// <summary>The marks that are empty after the clip.</summary>
        public int EmptyMarks;

        /// <summary>The wall-clock seconds of the intersections of marks with a clip.</summary>
        public double ClipSeconds;
    }

    /// <summary>
    /// A graphics target that draws nothing. It keeps the marks of each run as Skia paths in the coordinates of the
    /// page: a filled area as it is, and a stroked line as its outline, with the transform and the clip of the moment
    /// applied. It counts the calls that it does not keep.
    /// </summary>
    public class MarkCollectingGraphicsTarget : IGraphicsTarget
    {
        /// <summary>A pen of the target: its colour and the paint that makes the outline of a stroke.</summary>
        private sealed class Pen
        {
            public CmykColor Colour;
            public SKPaint StrokePaint;
        }

        private readonly float outlineResolutionScale;
        private readonly Dictionary<object, CmykColor> solidBrushes = new Dictionary<object, CmykColor>(ReferenceEqualityComparer.Instance);
        private readonly HashSet<object> patternBrushes = new HashSet<object>(ReferenceEqualityComparer.Instance);
        private readonly Dictionary<object, Pen> pens = new Dictionary<object, Pen>(ReferenceEqualityComparer.Instance);
        private readonly Dictionary<object, SKPath> paths = new Dictionary<object, SKPath>(ReferenceEqualityComparer.Instance);
        private readonly HashSet<object> fonts = new HashSet<object>(ReferenceEqualityComparer.Instance);
        private readonly Stack<SKMatrix> outerTransforms = new Stack<SKMatrix>();
        private readonly Stack<SKPath> outerClips = new Stack<SKPath>();
        private readonly Stack<bool> outerAntiAliasing = new Stack<bool>();
        private SKMatrix transform = SKMatrix.CreateIdentity();
        private SKPath clip;                 // In the coordinates of the page. Null when nothing clips.
        private bool isAntiAliasing;
        private int blendingDepth;

        /// <summary>The runs of the drawing, the lowest first.</summary>
        public List<Run> Runs { get; } = new List<Run>();

        /// <summary>The counts of what the target met and did not keep.</summary>
        public NotKeptCounts NotKept { get; } = new NotKeptCounts();

        /// <summary>
        /// Makes a target. outlineResolutionScale is the resolution, in samples for each unit of the page, for which
        /// SKPaint.GetFillPath makes the outline of a stroke.
        /// </summary>
        public MarkCollectingGraphicsTarget(float outlineResolutionScale)
        {
            this.outlineResolutionScale = outlineResolutionScale;
        }

        /// <summary>The intensity of the colours. The target keeps it and does not use it.</summary>
        public float Intensity { get; set; } = 1.0F;

        /// <summary>True, so that the target can count a pattern brush that the map asks for.</summary>
        public bool SupportsPatternBrushes { get { return true; } }

        // ---- Transforms, clips, antialiasing and blending.

        /// <summary>Prepends a transform, as SKCanvas.Concat does.</summary>
        public void PushTransform(Matrix matrix)
        {
            outerTransforms.Push(transform);
            transform = SKMatrix.Concat(transform, ConvertMatrix(matrix));
        }

        /// <summary>Goes back to the transform before the last PushTransform.</summary>
        public void PopTransform()
        {
            transform = outerTransforms.Pop();
        }

        /// <summary>Makes the clip smaller by a path that CreatePath made.</summary>
        public void PushClip(object pathKey)
        {
            PushClipPath(paths[pathKey]);
        }

        /// <summary>Makes the clip smaller by a rectangle.</summary>
        public void PushClip(RectangleF rectangle)
        {
            using (SKPath path = new SKPath()) {
                path.AddRect(ConvertRectangle(rectangle));
                PushClipPath(path);
            }
        }

        /// <summary>Makes the clip smaller by the area that a set of rectangles covers.</summary>
        public void PushClip(RectangleF[] rectangles)
        {
            using (SKPath path = new SKPath()) {
                foreach (RectangleF rectangle in rectangles)
                    path.AddRect(ConvertRectangle(rectangle), SKPathDirection.Clockwise);
                PushClipPath(path);
            }
        }

        /// <summary>Makes the clip smaller by a path.</summary>
        public void PushClip(List<GraphicsPathPart> parts, AreaFillMode windingMode)
        {
            using (SKPath path = ConvertPath(parts, windingMode))
                PushClipPath(path);
        }

        /// <summary>Goes back to the clip before the last PushClip.</summary>
        public void PopClip()
        {
            clip = outerClips.Pop();
        }

        /// <summary>Counts the call, and counts it as a switch when it changes the antialiasing.</summary>
        public void PushAntiAliasing(bool antiAlias)
        {
            ++NotKept.AntiAliasingPushes;
            if (antiAlias != isAntiAliasing)
                ++NotKept.AntiAliasingSwitches;
            outerAntiAliasing.Push(isAntiAliasing);
            isAntiAliasing = antiAlias;
        }

        /// <summary>Goes back to the antialiasing before the last PushAntiAliasing.</summary>
        public void PopAntiAliasing()
        {
            isAntiAliasing = outerAntiAliasing.Pop();
        }

        /// <summary>Starts a blending. The marks until PopBlending are marks of an overprinted colour.</summary>
        public bool PushBlending(BlendMode blendMode)
        {
            ++NotKept.BlendingPushes;
            ++blendingDepth;
            return true;
        }

        /// <summary>Ends the blending that the last PushBlending started.</summary>
        public void PopBlending()
        {
            --blendingDepth;
        }

        // ---- Paths, brushes, pens and fonts.

        /// <summary>Keeps a path under a key, in the coordinates in which it is given.</summary>
        public void CreatePath(object key, List<GraphicsPathPart> parts, AreaFillMode windingMode)
        {
            paths.Add(key, ConvertPath(parts, windingMode));
        }

        /// <summary>Keeps the colour of a brush under a key.</summary>
        public void CreateSolidBrush(object key, CmykColor color)
        {
            solidBrushes.Add(key, color);
        }

        /// <summary>Counts a pattern brush and gives a target that throws the drawing of the pattern away.</summary>
        public IBrushTarget CreatePatternBrush(SizeF size, float angle, int bitmapWidth, int bitmapHeight)
        {
            ++NotKept.PatternBrushes;
            return new PatternBrushTarget(this);
        }

        /// <summary>Keeps a pen with the colour of a brush under a key.</summary>
        public void CreatePen(object key, object brushKey, float width, LineCapMode caps, LineJoinMode join, float miterLimit)
        {
            CreatePen(key, solidBrushes[brushKey], width, caps, join, miterLimit);
        }

        /// <summary>Keeps a pen under a key: its colour, and the paint that makes the outline of its strokes.</summary>
        public void CreatePen(object key, CmykColor color, float width, LineCapMode caps, LineJoinMode join, float miterLimit)
        {
            SKPaint strokePaint = new SKPaint();
            strokePaint.IsStroke = true;
            strokePaint.StrokeWidth = width;
            strokePaint.StrokeCap = caps == LineCapMode.Round ? SKStrokeCap.Round : caps == LineCapMode.Square ? SKStrokeCap.Square : SKStrokeCap.Butt;
            strokePaint.StrokeJoin = join == LineJoinMode.Round ? SKStrokeJoin.Round : join == LineJoinMode.Bevel ? SKStrokeJoin.Bevel : SKStrokeJoin.Miter;
            if (join == LineJoinMode.Miter)
                strokePaint.StrokeMiter = miterLimit;
            pens.Add(key, new Pen { Colour = color, StrokePaint = strokePaint });
        }

        /// <summary>Keeps the key of a font. The target does not keep text.</summary>
        public void CreateFont(object key, string familyName, float emHeight, TextEffects effects)
        {
            fonts.Add(key);
        }

        /// <summary>Says if CreatePath made a path under the key.</summary>
        public bool HasPath(object pathKey) { return paths.ContainsKey(pathKey); }
        /// <summary>Says if a brush was made under the key.</summary>
        public bool HasBrush(object brushKey) { return solidBrushes.ContainsKey(brushKey) || patternBrushes.Contains(brushKey); }
        /// <summary>Says if a pen was made under the key.</summary>
        public bool HasPen(object penKey) { return pens.ContainsKey(penKey); }
        /// <summary>Says if a font was made under the key.</summary>
        public bool HasFont(object fontKey) { return fonts.Contains(fontKey); }

        // ---- Strokes.

        /// <summary>Keeps the outline of a stroked line.</summary>
        public void DrawLine(object penKey, PointF start, PointF finish)
        {
            using (SKPath path = new SKPath()) {
                path.MoveTo(start.X, start.Y);
                path.LineTo(finish.X, finish.Y);
                KeepStroke(penKey, path);
            }
        }

        /// <summary>Keeps the outline of a stroked arc.</summary>
        public void DrawArc(object penKey, PointF center, float radius, float startAngle, float sweepAngle)
        {
            using (SKPath path = new SKPath()) {
                path.AddArc(new SKRect(center.X - radius, center.Y - radius, center.X + radius, center.Y + radius), startAngle, sweepAngle);
                KeepStroke(penKey, path);
            }
        }

        /// <summary>Keeps the outline of a stroked ellipse.</summary>
        public void DrawEllipse(object penKey, PointF center, float radiusX, float radiusY)
        {
            using (SKPath path = new SKPath()) {
                path.AddOval(new SKRect(center.X - radiusX, center.Y - radiusY, center.X + radiusX, center.Y + radiusY));
                KeepStroke(penKey, path);
            }
        }

        /// <summary>Keeps the outline of a stroked rectangle.</summary>
        public void DrawRectangle(object penKey, RectangleF rect)
        {
            using (SKPath path = new SKPath()) {
                path.AddRect(ConvertRectangle(rect));
                KeepStroke(penKey, path);
            }
        }

        /// <summary>Keeps the outline of a stroked closed polygon.</summary>
        public void DrawPolygon(object penKey, PointF[] pts)
        {
            using (SKPath path = ConvertPolyline(pts, true))
                KeepStroke(penKey, path);
        }

        /// <summary>Keeps the outline of a stroked open polygon.</summary>
        public void DrawPolyline(object penKey, PointF[] pts)
        {
            using (SKPath path = ConvertPolyline(pts, false))
                KeepStroke(penKey, path);
        }

        /// <summary>Keeps the outline of a stroked path that CreatePath made.</summary>
        public void DrawPath(object penKey, object pathKey)
        {
            KeepStroke(penKey, paths[pathKey]);
        }

        /// <summary>Keeps the outline of a stroked path.</summary>
        public void DrawPath(object penKey, List<GraphicsPathPart> parts)
        {
            using (SKPath path = ConvertPath(parts, AreaFillMode.Winding))
                KeepStroke(penKey, path);
        }

        // ---- Fills.

        /// <summary>Keeps a filled ellipse.</summary>
        public void FillEllipse(object brushKey, PointF center, float radiusX, float radiusY)
        {
            using (SKPath path = new SKPath()) {
                path.AddOval(new SKRect(center.X - radiusX, center.Y - radiusY, center.X + radiusX, center.Y + radiusY));
                KeepFill(brushKey, path);
            }
        }

        /// <summary>Keeps a filled rectangle.</summary>
        public void FillRectangle(object brushKey, RectangleF rect)
        {
            using (SKPath path = new SKPath()) {
                path.AddRect(ConvertRectangle(rect));
                KeepFill(brushKey, path);
            }
        }

        /// <summary>Keeps a filled polygon.</summary>
        public void FillPolygon(object brushKey, PointF[] pts, AreaFillMode windingMode)
        {
            using (SKPath path = ConvertPolyline(pts, true)) {
                path.FillType = ConvertFillMode(windingMode);
                KeepFill(brushKey, path);
            }
        }

        /// <summary>Keeps a filled path that CreatePath made.</summary>
        public void FillPath(object brushKey, object pathKey)
        {
            KeepFill(brushKey, paths[pathKey]);
        }

        /// <summary>Keeps a filled path.</summary>
        public void FillPath(object brushKey, List<GraphicsPathPart> parts, AreaFillMode windingMode)
        {
            using (SKPath path = ConvertPath(parts, windingMode))
                KeepFill(brushKey, path);
        }

        // ---- What the target does not keep.

        /// <summary>Counts a call that draws text.</summary>
        public void DrawText(string text, object fontKey, object brushKey, PointF upperLeft) { ++NotKept.TextCalls; }
        /// <summary>Counts a call that draws the outline of text.</summary>
        public void DrawTextOutline(string text, object fontKey, object penKey, PointF upperLeft) { ++NotKept.TextCalls; }
        /// <summary>Counts a call that draws a bitmap.</summary>
        public void DrawBitmap(IGraphicsBitmap bm, RectangleF rectangle, BitmapScaling scalingMode) { ++NotKept.BitmapCalls; }
        /// <summary>Counts a call that draws a part of a bitmap.</summary>
        public void DrawBitmapPart(IGraphicsBitmap bm, int x, int y, int width, int height, RectangleF rectange, BitmapScaling scalingMode) { ++NotKept.BitmapCalls; }

        /// <summary>Frees the paints of the pens and the paths that CreatePath made. The marks stay.</summary>
        public virtual void Dispose()
        {
            foreach (Pen pen in pens.Values)
                pen.StrokePaint.Dispose();
            pens.Clear();
            foreach (SKPath path in paths.Values)
                path.Dispose();
            paths.Clear();
        }

        // ---- The marks.

        /// <summary>Keeps the outline of a stroke of a path, which is in the coordinates of the moment.</summary>
        private void KeepStroke(object penKey, SKPath path)
        {
            Pen pen = pens[penKey];
            if (pen.StrokePaint.StrokeWidth <= 0) {
                ++NotKept.HairlineStrokes;
                return;
            }

            using (SKPath outline = new SKPath()) {
                float scaleOfTransform = (float)Math.Sqrt(Math.Abs(transform.ScaleX * transform.ScaleY - transform.SkewX * transform.SkewY));
                pen.StrokePaint.GetFillPath(path, outline, outlineResolutionScale * scaleOfTransform);
                KeepMark(pen.Colour, outline);
            }
        }

        /// <summary>Keeps a filled path, which is in the coordinates of the moment.</summary>
        private void KeepFill(object brushKey, SKPath path)
        {
            if (patternBrushes.Contains(brushKey))
                ++NotKept.FillsWithPatternBrush;
            else
                KeepMark(solidBrushes[brushKey], path);
        }

        /// <summary>
        /// Moves a shape to the coordinates of the page, cuts it to the clip of the moment, and adds it to the run
        /// of its colour.
        /// </summary>
        private void KeepMark(CmykColor colour, SKPath shape)
        {
            SKPath mark = new SKPath();
            shape.Transform(in transform, mark);

            if (clip != null) {
                SKPath clipped = null;
                if (mark.Bounds.IntersectsWith(clip.Bounds)) {
                    long start = System.Diagnostics.Stopwatch.GetTimestamp();
                    clipped = mark.Op(clip, SKPathOp.Intersect);
                    NotKept.ClipSeconds += System.Diagnostics.Stopwatch.GetElapsedTime(start).TotalSeconds;
                    if (clipped == null)
                        ++NotKept.MarkClipFailures;
                    else
                        ++NotKept.MarksClipped;
                }
                else {
                    ++NotKept.MarksOutsideClip;
                }
                mark.Dispose();
                if (clipped == null)
                    return;
                mark = clipped;
            }

            if (mark.IsEmpty) {
                ++NotKept.EmptyMarks;
                mark.Dispose();
                return;
            }

            bool isInsideBlending = blendingDepth > 0;
            Run run = Runs.Count > 0 ? Runs[Runs.Count - 1] : null;
            if (run == null || run.IsInsideBlending != isInsideBlending || !run.Colour.Equals(colour)) {
                run = new Run { Index = Runs.Count, Colour = colour, IsInsideBlending = isInsideBlending };
                Runs.Add(run);
            }
            run.Marks.Add(mark);
        }

        /// <summary>Makes the clip of the moment smaller by a path, which is in the coordinates of the moment.</summary>
        private void PushClipPath(SKPath path)
        {
            ++NotKept.Clips;
            outerClips.Push(clip);

            SKPath pageClip = new SKPath();
            path.Transform(in transform, pageClip);
            if (clip != null) {
                ++NotKept.ClipIntersections;
                SKPath both = pageClip.Op(clip, SKPathOp.Intersect);
                pageClip.Dispose();
                if (both == null) {
                    ++NotKept.ClipIntersectionFailures;
                    both = new SKPath();
                }
                pageClip = both;
            }
            clip = pageClip;
        }

        // ---- Conversions of the types of the drawing calls to the types of Skia.

        /// <summary>Gives the fill type of Skia for a fill mode of the drawing calls.</summary>
        private static SKPathFillType ConvertFillMode(AreaFillMode windingMode)
        {
            return windingMode == AreaFillMode.Alternate ? SKPathFillType.EvenOdd : SKPathFillType.Winding;
        }

        /// <summary>Gives the rectangle of Skia for a rectangle of the drawing calls.</summary>
        private static SKRect ConvertRectangle(RectangleF rectangle)
        {
            return new SKRect(rectangle.Left, rectangle.Top, rectangle.Right, rectangle.Bottom);
        }

        /// <summary>Gives the matrix of Skia for a matrix of the drawing calls.</summary>
        private static SKMatrix ConvertMatrix(Matrix matrix)
        {
            float[] elements = matrix.Elements;
            SKMatrix converted = SKMatrix.CreateIdentity();
            converted.ScaleX = elements[0];
            converted.SkewY = elements[1];
            converted.SkewX = elements[2];
            converted.ScaleY = elements[3];
            converted.TransX = elements[4];
            converted.TransY = elements[5];
            return converted;
        }

        /// <summary>Makes a path of Skia from the points of a polygon.</summary>
        private static SKPath ConvertPolyline(PointF[] points, bool isClosed)
        {
            SKPath path = new SKPath();
            path.MoveTo(points[0].X, points[0].Y);
            for (int i = 1; i < points.Length; ++i)
                path.LineTo(points[i].X, points[i].Y);
            if (isClosed)
                path.Close();
            return path;
        }

        /// <summary>Makes a path of Skia from the parts of a path of the drawing calls.</summary>
        private static SKPath ConvertPath(List<GraphicsPathPart> parts, AreaFillMode windingMode)
        {
            SKPath path = new SKPath();
            path.FillType = ConvertFillMode(windingMode);
            foreach (GraphicsPathPart part in parts) {
                switch (part.Kind) {
                    case GraphicsPathPartKind.Start:
                        path.MoveTo(part.Points[0].X, part.Points[0].Y);
                        break;
                    case GraphicsPathPartKind.Lines:
                        foreach (PointF point in part.Points)
                            path.LineTo(point.X, point.Y);
                        break;
                    case GraphicsPathPartKind.Beziers:
                        for (int i = 0; i + 2 < part.Points.Length; i += 3)
                            path.CubicTo(part.Points[i].X, part.Points[i].Y, part.Points[i + 1].X, part.Points[i + 1].Y, part.Points[i + 2].X, part.Points[i + 2].Y);
                        break;
                    case GraphicsPathPartKind.Close:
                        path.Close();
                        break;
                }
            }
            return path;
        }

        /// <summary>
        /// The target of a pattern brush. The probe draws patterns as shapes, so it counts such a brush and throws
        /// its drawing away.
        /// </summary>
        private sealed class PatternBrushTarget : MarkCollectingGraphicsTarget, IBrushTarget
        {
            private readonly MarkCollectingGraphicsTarget owner;

            /// <summary>Makes the target of a pattern brush of a target.</summary>
            public PatternBrushTarget(MarkCollectingGraphicsTarget owner) : base(owner.outlineResolutionScale)
            {
                this.owner = owner;
            }

            /// <summary>Notes the key as the key of a pattern brush, and throws the marks of the pattern away.</summary>
            public void FinishBrush(object brushKey)
            {
                owner.patternBrushes.Add(brushKey);
                foreach (Run run in Runs)
                    foreach (SKPath mark in run.Marks)
                        mark.Dispose();
                Runs.Clear();
            }
        }
    }
}
