using System;
using System.Collections.Generic;
using PurplePen.Graphics2D;

using PointF = System.Drawing.PointF;
using RectangleF = System.Drawing.RectangleF;
using SizeF = System.Drawing.SizeF;

namespace PurplePen.MapModel
{
    // Writes what is drawn into it as an SVG document. A call that the target cannot honor raises an
    // Svg_UnsupportedCallException that names the call.
    public class Svg_GraphicsTarget : IGraphicsTarget
    {
        // The names of the IGraphicsTarget calls that the target fully implements, each to its whole requirement.
        private static readonly HashSet<string> fullyImplementedCalls = new HashSet<string>(StringComparer.Ordinal);

        // Whether the target fully implements an IGraphicsTarget call, so that a drawing which makes it can be
        // compared with its reference image.
        // callName: the name of the IGraphicsTarget member.
        public static bool FullyImplements(string callName)
        {
            return fullyImplementedCalls.Contains(callName);
        }

        // Finish the document, closing every group still open, and return its text.
        public string FinishDocument()
        {
            throw new Svg_UnsupportedCallException(nameof(FinishDocument));
        }

        // The intensity at which colors are drawn; setting it discards the brushes and pens.
        public float Intensity
        {
            get { throw new Svg_UnsupportedCallException(nameof(Intensity)); }
            set { throw new Svg_UnsupportedCallException(nameof(Intensity)); }
        }

        // Open a group whose transform is prepended to the current one.
        public void PushTransform(Matrix matrix)
        {
            throw new Svg_UnsupportedCallException(nameof(PushTransform));
        }

        // Close the group of the last PushTransform.
        public void PopTransform()
        {
            throw new Svg_UnsupportedCallException(nameof(PopTransform));
        }

        // Clip to a path created with CreatePath.
        public void PushClip(object pathKey)
        {
            throw new Svg_UnsupportedCallException(nameof(PushClip));
        }

        // Clip to a rectangle.
        public void PushClip(RectangleF rectangle)
        {
            throw new Svg_UnsupportedCallException(nameof(PushClip));
        }

        // Clip to the union of rectangles.
        public void PushClip(RectangleF[] rectangles)
        {
            throw new Svg_UnsupportedCallException(nameof(PushClip));
        }

        // Clip to a path given by its parts.
        public void PushClip(List<GraphicsPathPart> parts, AreaFillMode windingMode)
        {
            throw new Svg_UnsupportedCallException(nameof(PushClip));
        }

        // End the clip of the last PushClip.
        public void PopClip()
        {
            throw new Svg_UnsupportedCallException(nameof(PopClip));
        }

        // Draw anti-aliased or with crisp edges until the matching PopAntiAliasing.
        public void PushAntiAliasing(bool antiAlias)
        {
            throw new Svg_UnsupportedCallException(nameof(PushAntiAliasing));
        }

        // End the anti-aliasing mode of the last PushAntiAliasing.
        public void PopAntiAliasing()
        {
            throw new Svg_UnsupportedCallException(nameof(PopAntiAliasing));
        }

        // Blend what is drawn until the matching PopBlending; return whether the mode is supported.
        public bool PushBlending(BlendMode blendMode)
        {
            throw new Svg_UnsupportedCallException(nameof(PushBlending));
        }

        // End the blending of the last PushBlending.
        public void PopBlending()
        {
            throw new Svg_UnsupportedCallException(nameof(PopBlending));
        }

        // Create a path under a key.
        public void CreatePath(object key, List<GraphicsPathPart> parts, AreaFillMode windingMode)
        {
            throw new Svg_UnsupportedCallException(nameof(CreatePath));
        }

        // Create a solid brush under a key.
        public void CreateSolidBrush(object key, CmykColor color)
        {
            throw new Svg_UnsupportedCallException(nameof(CreateSolidBrush));
        }

        // Whether pattern brushes are supported.
        public bool SupportsPatternBrushes
        {
            get { throw new Svg_UnsupportedCallException(nameof(SupportsPatternBrushes)); }
        }

        // Create a target that draws one tile of a pattern brush.
        public IBrushTarget CreatePatternBrush(SizeF size, float angle, int bitmapWidth, int bitmapHeight)
        {
            throw new Svg_UnsupportedCallException(nameof(CreatePatternBrush));
        }

        // Create a pen that strokes with a brush, under a key.
        public void CreatePen(object key, object brushKey, float width, LineCapMode caps, LineJoinMode join, float miterLimit)
        {
            throw new Svg_UnsupportedCallException(nameof(CreatePen));
        }

        // Create a pen of a solid color, under a key.
        public void CreatePen(object key, CmykColor color, float width, LineCapMode caps, LineJoinMode join, float miterLimit)
        {
            throw new Svg_UnsupportedCallException(nameof(CreatePen));
        }

        // Create a font under a key.
        public void CreateFont(object key, string familyName, float emHeight, TextEffects effects)
        {
            throw new Svg_UnsupportedCallException(nameof(CreateFont));
        }

        // Whether a path exists under a key.
        public bool HasPath(object pathKey)
        {
            throw new Svg_UnsupportedCallException(nameof(HasPath));
        }

        // Whether a brush exists under a key.
        public bool HasBrush(object brushKey)
        {
            throw new Svg_UnsupportedCallException(nameof(HasBrush));
        }

        // Whether a pen exists under a key.
        public bool HasPen(object penKey)
        {
            throw new Svg_UnsupportedCallException(nameof(HasPen));
        }

        // Whether a font exists under a key.
        public bool HasFont(object fontKey)
        {
            throw new Svg_UnsupportedCallException(nameof(HasFont));
        }

        // Draw a line with a pen.
        public void DrawLine(object penKey, PointF start, PointF finish)
        {
            throw new Svg_UnsupportedCallException(nameof(DrawLine));
        }

        // Draw an arc with a pen.
        public void DrawArc(object penKey, PointF center, float radius, float startAngle, float sweepAngle)
        {
            throw new Svg_UnsupportedCallException(nameof(DrawArc));
        }

        // Draw an ellipse with a pen.
        public void DrawEllipse(object penKey, PointF center, float radiusX, float radiusY)
        {
            throw new Svg_UnsupportedCallException(nameof(DrawEllipse));
        }

        // Fill an ellipse with a brush.
        public void FillEllipse(object brushKey, PointF center, float radiusX, float radiusY)
        {
            throw new Svg_UnsupportedCallException(nameof(FillEllipse));
        }

        // Draw a rectangle with a pen.
        public void DrawRectangle(object penKey, RectangleF rect)
        {
            throw new Svg_UnsupportedCallException(nameof(DrawRectangle));
        }

        // Fill a rectangle with a brush.
        public void FillRectangle(object brushKey, RectangleF rect)
        {
            throw new Svg_UnsupportedCallException(nameof(FillRectangle));
        }

        // Draw a closed polygon with a pen.
        public void DrawPolygon(object penKey, PointF[] pts)
        {
            throw new Svg_UnsupportedCallException(nameof(DrawPolygon));
        }

        // Draw an open polyline with a pen.
        public void DrawPolyline(object penKey, PointF[] pts)
        {
            throw new Svg_UnsupportedCallException(nameof(DrawPolyline));
        }

        // Fill a polygon with a brush.
        public void FillPolygon(object brushKey, PointF[] pts, AreaFillMode windingMode)
        {
            throw new Svg_UnsupportedCallException(nameof(FillPolygon));
        }

        // Draw a path created with CreatePath with a pen.
        public void DrawPath(object penKey, object pathKey)
        {
            throw new Svg_UnsupportedCallException(nameof(DrawPath));
        }

        // Draw a path given by its parts with a pen.
        public void DrawPath(object penKey, List<GraphicsPathPart> parts)
        {
            throw new Svg_UnsupportedCallException(nameof(DrawPath));
        }

        // Fill a path created with CreatePath with a brush.
        public void FillPath(object brushKey, object pathKey)
        {
            throw new Svg_UnsupportedCallException(nameof(FillPath));
        }

        // Fill a path given by its parts with a brush.
        public void FillPath(object brushKey, List<GraphicsPathPart> parts, AreaFillMode windingMode)
        {
            throw new Svg_UnsupportedCallException(nameof(FillPath));
        }

        // Draw text with its upper left corner at a point.
        public void DrawText(string text, object fontKey, object brushKey, PointF upperLeft)
        {
            throw new Svg_UnsupportedCallException(nameof(DrawText));
        }

        // Draw the outline of text with its upper left corner at a point.
        public void DrawTextOutline(string text, object fontKey, object penKey, PointF upperLeft)
        {
            throw new Svg_UnsupportedCallException(nameof(DrawTextOutline));
        }

        // Draw a bitmap over a rectangle.
        public void DrawBitmap(IGraphicsBitmap bm, RectangleF rectangle, BitmapScaling scalingMode)
        {
            throw new Svg_UnsupportedCallException(nameof(DrawBitmap));
        }

        // Draw part of a bitmap over a rectangle.
        public void DrawBitmapPart(IGraphicsBitmap bm, int x, int y, int width, int height, RectangleF rectangle, BitmapScaling scalingMode)
        {
            throw new Svg_UnsupportedCallException(nameof(DrawBitmapPart));
        }

        // Release nothing: the target holds no unmanaged resources.
        public void Dispose()
        {
        }
    }
}
