using System.Collections.Generic;
using PurplePen.Graphics2D;
using PurplePen.MapModel;
using SkiaSharp;

using PointF = System.Drawing.PointF;
using RectangleF = System.Drawing.RectangleF;
using SizeF = System.Drawing.SizeF;

namespace Map_Svg.Tests
{
    // The Skia target with its clips anti-aliased, as Chromium always draws a clip path; the Skia target itself clips
    // with SkiaSharp's default, aliased. Every other call goes to the Skia target unchanged.
    public sealed class AntiAliasedClipsTarget : IGraphicsTarget
    {
        // The Skia target drawn through.
        private readonly Skia_GraphicsTarget target;

        // The paths created, as Skia paths for clipping, by their keys.
        private readonly Dictionary<object, SKPath> clipPaths = new Dictionary<object, SKPath>(new IdentityComparer<object>());

        // Create the target.
        // target: the Skia target drawn through.
        public AntiAliasedClipsTarget(Skia_GraphicsTarget target)
        {
            this.target = target;
        }

        // The intensity and the transforms, which go to the Skia target unchanged.
        public float Intensity { get { return target.Intensity; } set { target.Intensity = value; } }
        public void PushTransform(Matrix matrix) { target.PushTransform(matrix); }
        public void PopTransform() { target.PopTransform(); }

        // Clip to a path created with CreatePath, anti-aliased.
        public void PushClip(object pathKey) { Clip(clipPaths[pathKey]); }

        // Clip to a rectangle, anti-aliased.
        public void PushClip(RectangleF rectangle) { PushClip(new RectangleF[] { rectangle }); }

        // Clip to the union of rectangles, anti-aliased: each clockwise, under the nonzero rule, as the Skia target
        // clips to them.
        public void PushClip(RectangleF[] rectangles)
        {
            using (SKPath path = new SKPath()) {
                foreach (RectangleF rectangle in rectangles)
                    path.AddRect(new SKRect(rectangle.Left, rectangle.Top, rectangle.Right, rectangle.Bottom), SKPathDirection.Clockwise);
                Clip(path);
            }
        }

        // Clip to a path given by its parts, under the rule of its fill mode, anti-aliased.
        public void PushClip(List<GraphicsPathPart> parts, AreaFillMode windingMode)
        {
            using (SKPath path = PathOf(parts, windingMode))
                Clip(path);
        }

        // End the clip of the last PushClip.
        public void PopClip() { target.Canvas.Restore(); }

        // The anti-aliasing and the blending, which go to the Skia target unchanged.
        public void PushAntiAliasing(bool antiAlias) { target.PushAntiAliasing(antiAlias); }
        public void PopAntiAliasing() { target.PopAntiAliasing(); }
        public bool PushBlending(BlendMode blendMode) { return target.PushBlending(blendMode); }
        public void PopBlending() { target.PopBlending(); }

        // Create a path under a key in the Skia target, and keep a Skia path of it to clip to.
        public void CreatePath(object key, List<GraphicsPathPart> parts, AreaFillMode windingMode)
        {
            target.CreatePath(key, parts, windingMode);
            clipPaths.Add(key, PathOf(parts, windingMode));
        }

        // The brushes, pens and fonts and every drawing call, which go to the Skia target unchanged.
        public void CreateSolidBrush(object key, CmykColor color) { target.CreateSolidBrush(key, color); }
        public bool SupportsPatternBrushes { get { return target.SupportsPatternBrushes; } }
        public IBrushTarget CreatePatternBrush(SizeF size, float angle, int bitmapWidth, int bitmapHeight) { return target.CreatePatternBrush(size, angle, bitmapWidth, bitmapHeight); }
        public void CreatePen(object key, object brushKey, float width, LineCapMode caps, LineJoinMode join, float miterLimit) { target.CreatePen(key, brushKey, width, caps, join, miterLimit); }
        public void CreatePen(object key, CmykColor color, float width, LineCapMode caps, LineJoinMode join, float miterLimit) { target.CreatePen(key, color, width, caps, join, miterLimit); }
        public void CreateFont(object key, string familyName, float emHeight, TextEffects effects) { target.CreateFont(key, familyName, emHeight, effects); }
        public bool HasPath(object pathKey) { return target.HasPath(pathKey); }
        public bool HasBrush(object brushKey) { return target.HasBrush(brushKey); }
        public bool HasPen(object penKey) { return target.HasPen(penKey); }
        public bool HasFont(object fontKey) { return target.HasFont(fontKey); }
        public void DrawLine(object penKey, PointF start, PointF finish) { target.DrawLine(penKey, start, finish); }
        public void DrawArc(object penKey, PointF center, float radius, float startAngle, float sweepAngle) { target.DrawArc(penKey, center, radius, startAngle, sweepAngle); }
        public void DrawEllipse(object penKey, PointF center, float radiusX, float radiusY) { target.DrawEllipse(penKey, center, radiusX, radiusY); }
        public void FillEllipse(object brushKey, PointF center, float radiusX, float radiusY) { target.FillEllipse(brushKey, center, radiusX, radiusY); }
        public void DrawRectangle(object penKey, RectangleF rect) { target.DrawRectangle(penKey, rect); }
        public void FillRectangle(object brushKey, RectangleF rect) { target.FillRectangle(brushKey, rect); }
        public void DrawPolygon(object penKey, PointF[] pts) { target.DrawPolygon(penKey, pts); }
        public void DrawPolyline(object penKey, PointF[] pts) { target.DrawPolyline(penKey, pts); }
        public void FillPolygon(object brushKey, PointF[] pts, AreaFillMode windingMode) { target.FillPolygon(brushKey, pts, windingMode); }
        public void DrawPath(object penKey, object pathKey) { target.DrawPath(penKey, pathKey); }
        public void DrawPath(object penKey, List<GraphicsPathPart> parts) { target.DrawPath(penKey, parts); }
        public void FillPath(object brushKey, object pathKey) { target.FillPath(brushKey, pathKey); }
        public void FillPath(object brushKey, List<GraphicsPathPart> parts, AreaFillMode windingMode) { target.FillPath(brushKey, parts, windingMode); }
        public void DrawText(string text, object fontKey, object brushKey, PointF upperLeft) { target.DrawText(text, fontKey, brushKey, upperLeft); }
        public void DrawTextOutline(string text, object fontKey, object penKey, PointF upperLeft) { target.DrawTextOutline(text, fontKey, penKey, upperLeft); }
        public void DrawBitmap(IGraphicsBitmap bm, RectangleF rectangle, BitmapScaling scalingMode) { target.DrawBitmap(bm, rectangle, scalingMode); }
        public void DrawBitmapPart(IGraphicsBitmap bm, int x, int y, int width, int height, RectangleF rectangle, BitmapScaling scalingMode) { target.DrawBitmapPart(bm, x, y, width, height, rectangle, scalingMode); }

        // Release the clip paths; the Skia target belongs to the caller.
        public void Dispose()
        {
            foreach (SKPath path in clipPaths.Values)
                path.Dispose();
        }

        // Save the canvas and clip it to a path, anti-aliased.
        // path: the path.
        private void Clip(SKPath path)
        {
            target.Canvas.Save();
            target.Canvas.ClipPath(path, SKClipOperation.Intersect, true);
        }

        // The Skia path of parts, built as the Skia target builds one.
        // parts: the parts.
        // windingMode: the fill mode.
        private static SKPath PathOf(List<GraphicsPathPart> parts, AreaFillMode windingMode)
        {
            SKPath path = new SKPath { FillType = windingMode == AreaFillMode.Alternate ? SKPathFillType.EvenOdd : SKPathFillType.Winding };
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
                        for (int index = 0; index + 2 < part.Points.Length; index += 3)
                            path.CubicTo(part.Points[index].X, part.Points[index].Y, part.Points[index + 1].X, part.Points[index + 1].Y, part.Points[index + 2].X, part.Points[index + 2].Y);
                        break;
                    case GraphicsPathPartKind.Close:
                        path.Close();
                        break;
                }
            }
            return path;
        }
    }
}
