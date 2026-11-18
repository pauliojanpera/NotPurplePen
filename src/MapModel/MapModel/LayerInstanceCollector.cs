using System;
using System.Collections.Generic;
using System.Drawing;

namespace PurplePen.MapModel
{
    using PurplePen.Graphics2D;

    // The collector of pass 1 of the drawing: the adapter between the drawing code of the symbols and the layer
    // instances (LayerInstances.cs). The symbols draw the shapes of one palette entry on it as on any drawing target,
    // and it turns each drawing call into a clipping object of the entry's instance with full coverage (G = 1), at the
    // placement that the transformations of the call give. It draws nothing, and the colors of the brushes and the
    // pens do not matter to it.
    // The full coverage of a palette entry is not clipped, has no pattern brushes and draws no bitmaps: in the drawing
    // of the symbols, only the hatchings and the patterns of areas clip or use a pattern brush, and only the image
    // layer and the layout layer draw bitmaps. Pass 1 leaves all of them out, so the collector does not take them.
    internal sealed class LayerInstanceCollector : IGraphicsTarget
    {
        private readonly SymColor paletteEntry;
        private readonly ITextMetrics textMetrics;
        private readonly List<ClippingObject> clippingObjects = new List<ClippingObject>();

        // The placement of the shapes that the next drawing calls draw, and the placements that the transformations
        // pushed before it hold.
        private Placement placement = Placement.Identity;
        private readonly Stack<Placement> outerPlacements = new Stack<Placement>();

        // The paths, brushes, pens and fonts that the symbols created, by their keys.
        private readonly Dictionary<object, CreatedPath> paths = new Dictionary<object, CreatedPath>(ReferenceEqualityComparer.Instance);
        private readonly HashSet<object> brushes = new HashSet<object>(ReferenceEqualityComparer.Instance);
        private readonly Dictionary<object, Stroke> pens = new Dictionary<object, Stroke>(ReferenceEqualityComparer.Instance);
        private readonly Dictionary<object, CreatedFont> fonts = new Dictionary<object, CreatedFont>(ReferenceEqualityComparer.Instance);

        private float intensity = 1;

        // Make a collector of the shapes of a palette entry.
        // paletteEntry: the palette entry whose shapes the symbols draw.
        // textMetrics: the metrics of the fonts, which measure the bounds of the text runs.
        public LayerInstanceCollector(SymColor paletteEntry, ITextMetrics textMetrics)
        {
            this.paletteEntry = paletteEntry;
            this.textMetrics = textMetrics;
        }

        // Make the instances of the drawing calls: the instance of the palette entry with full coverage, if the symbols
        // drew a shape.
        public IReadOnlyList<LayerInstance> MakeInstances()
        {
            if (clippingObjects.Count == 0)
                return Array.Empty<LayerInstance>();
            else
                return new LayerInstance[] { new LayerInstance(paletteEntry, GeometryPrototype.FullCoverage, new Matrix(), clippingObjects) };
        }

        // The intensity changes the colors only. A change of it destroys the brushes and the pens, as on every target.
        public float Intensity
        {
            get { return intensity; }
            set
            {
                intensity = value;
                brushes.Clear();
                pens.Clear();
            }
        }

        public void PushTransform(Matrix matrix)
        {
            outerPlacements.Push(placement);
            placement = placement.Within(matrix);
        }

        public void PopTransform()
        {
            placement = outerPlacements.Pop();
        }

        public void PushClip(object pathKey)
        {
            throw CreateClipNotSupportedException();
        }

        public void PushClip(RectangleF rectangle)
        {
            throw CreateClipNotSupportedException();
        }

        public void PushClip(RectangleF[] rectangles)
        {
            throw CreateClipNotSupportedException();
        }

        public void PushClip(List<GraphicsPathPart> parts, AreaFillMode windingMode)
        {
            throw CreateClipNotSupportedException();
        }

        public void PopClip()
        {
            throw CreateClipNotSupportedException();
        }

        // The antialiasing is a concern of the rendering, and not of the geometry.
        public void PushAntiAliasing(bool antiAlias)
        {
        }

        public void PopAntiAliasing()
        {
        }

        // The collector does not blend: the compositing of the palette entries is pass 2.
        public bool PushBlending(BlendMode blendMode)
        {
            return false;
        }

        public void PopBlending()
        {
        }

        public void CreatePath(object key, List<GraphicsPathPart> parts, AreaFillMode windingMode)
        {
            if (paths.ContainsKey(key))
                throw new InvalidOperationException("Key already has a path created for it");

            paths.Add(key, new CreatedPath(new PathShape(parts), windingMode));
        }

        public void CreateSolidBrush(object key, CmykColor color)
        {
            if (!brushes.Add(key))
                throw new InvalidOperationException("Key already has a brush created for it");
        }

        public bool SupportsPatternBrushes
        {
            get { return false; }
        }

        public IBrushTarget CreatePatternBrush(SizeF size, float angle, int bitmapWidth, int bitmapHeight)
        {
            throw new NotSupportedException("The full coverage of a palette entry has no pattern brushes: the patterns of areas are repetitive geometry.");
        }

        public void CreatePen(object key, object brushKey, float width, LineCapMode caps, LineJoinMode join, float miterLimit)
        {
            CheckBrush(brushKey);
            AddPen(key, new Stroke(width, caps, join, miterLimit));
        }

        public void CreatePen(object key, CmykColor color, float width, LineCapMode caps, LineJoinMode join, float miterLimit)
        {
            AddPen(key, new Stroke(width, caps, join, miterLimit));
        }

        public void CreateFont(object key, string familyName, float emHeight, TextEffects effects)
        {
            if (fonts.ContainsKey(key))
                throw new InvalidOperationException("Key already has a font created for it");

            fonts.Add(key, new CreatedFont(familyName, emHeight, effects));
        }

        public bool HasPath(object pathKey)
        {
            return paths.ContainsKey(pathKey);
        }

        public bool HasBrush(object brushKey)
        {
            return brushes.Contains(brushKey);
        }

        public bool HasPen(object penKey)
        {
            return pens.ContainsKey(penKey);
        }

        public bool HasFont(object fontKey)
        {
            return fonts.ContainsKey(fontKey);
        }

        public void DrawLine(object penKey, PointF start, PointF finish)
        {
            AddStroked(penKey, PathShape.FromPoints(new PointF[] { start, finish }, false));
        }

        public void DrawArc(object penKey, PointF center, float radius, float startAngle, float sweepAngle)
        {
            AddStroked(penKey, new ArcShape(center, radius, startAngle, sweepAngle));
        }

        public void DrawEllipse(object penKey, PointF center, float radiusX, float radiusY)
        {
            AddStroked(penKey, new EllipseShape(center, radiusX, radiusY));
        }

        public void FillEllipse(object brushKey, PointF center, float radiusX, float radiusY)
        {
            AddFilled(brushKey, new EllipseShape(center, radiusX, radiusY), AreaFillMode.Winding);
        }

        public void DrawRectangle(object penKey, RectangleF rect)
        {
            AddStroked(penKey, PathShape.FromPoints(GetCorners(rect), true));
        }

        public void FillRectangle(object brushKey, RectangleF rect)
        {
            AddFilled(brushKey, PathShape.FromPoints(GetCorners(rect), true), AreaFillMode.Winding);
        }

        public void DrawPolygon(object penKey, PointF[] pts)
        {
            AddStroked(penKey, PathShape.FromPoints(pts, true));
        }

        public void DrawPolyline(object penKey, PointF[] pts)
        {
            AddStroked(penKey, PathShape.FromPoints(pts, false));
        }

        public void FillPolygon(object brushKey, PointF[] pts, AreaFillMode windingMode)
        {
            AddFilled(brushKey, PathShape.FromPoints(pts, true), windingMode);
        }

        public void DrawPath(object penKey, object pathKey)
        {
            AddStroked(penKey, GetPath(pathKey).Shape);
        }

        public void DrawPath(object penKey, List<GraphicsPathPart> parts)
        {
            AddStroked(penKey, new PathShape(parts));
        }

        public void FillPath(object brushKey, object pathKey)
        {
            CreatedPath path = GetPath(pathKey);
            AddFilled(brushKey, path.Shape, path.FillRule);
        }

        public void FillPath(object brushKey, List<GraphicsPathPart> parts, AreaFillMode windingMode)
        {
            AddFilled(brushKey, new PathShape(parts), windingMode);
        }

        // A text without characters covers nothing, as no target draws it.
        public void DrawText(string text, object fontKey, object brushKey, PointF upperLeft)
        {
            CreatedFont font = GetFont(fontKey);
            if (!string.IsNullOrEmpty(text))
                AddFilled(brushKey, font.MakeTextRun(text, upperLeft, textMetrics), AreaFillMode.Winding);
            else
                CheckBrush(brushKey);
        }

        public void DrawTextOutline(string text, object fontKey, object penKey, PointF upperLeft)
        {
            CreatedFont font = GetFont(fontKey);
            if (!string.IsNullOrEmpty(text))
                AddStroked(penKey, font.MakeTextRun(text, upperLeft, textMetrics));
            else
                GetPen(penKey);
        }

        public void DrawBitmap(IGraphicsBitmap bm, RectangleF rectangle, BitmapScaling scalingMode)
        {
            throw CreateBitmapNotSupportedException();
        }

        public void DrawBitmapPart(IGraphicsBitmap bm, int x, int y, int width, int height, RectangleF rectangle, BitmapScaling scalingMode)
        {
            throw CreateBitmapNotSupportedException();
        }

        public void Dispose()
        {
        }

        // Add a shape that a brush fills, at the current placement.
        private void AddFilled(object brushKey, Shape shape, AreaFillMode fillRule)
        {
            CheckBrush(brushKey);
            clippingObjects.Add(new FilledShape(shape, fillRule, placement));
        }

        // Add a shape that a pen strokes, at the current placement.
        private void AddStroked(object penKey, Shape shape)
        {
            clippingObjects.Add(new StrokedShape(shape, GetPen(penKey), placement));
        }

        // Add a pen with its stroke.
        private void AddPen(object key, Stroke stroke)
        {
            if (pens.ContainsKey(key))
                throw new InvalidOperationException("Key already has a pen created for it");

            pens.Add(key, stroke);
        }

        // Check that a brush was created.
        private void CheckBrush(object brushKey)
        {
            if (!brushes.Contains(brushKey))
                throw new ArgumentException("Given key does not have a brush created for it", nameof(brushKey));
        }

        // The stroke of a pen that was created.
        private Stroke GetPen(object penKey)
        {
            Stroke stroke;
            if (pens.TryGetValue(penKey, out stroke))
                return stroke;
            throw new ArgumentException("Given key does not have a pen created for it", nameof(penKey));
        }

        // A path that was created.
        private CreatedPath GetPath(object pathKey)
        {
            CreatedPath path;
            if (paths.TryGetValue(pathKey, out path))
                return path;
            throw new ArgumentException("Given key does not have a path created for it", nameof(pathKey));
        }

        // A font that was created.
        private CreatedFont GetFont(object fontKey)
        {
            CreatedFont font;
            if (fonts.TryGetValue(fontKey, out font))
                return font;
            throw new ArgumentException("Given key does not have a font created for it", nameof(fontKey));
        }

        // The corners of a rectangle, clockwise from the upper left corner, as a target makes the path of a rectangle.
        private static PointF[] GetCorners(RectangleF rect)
        {
            return new PointF[] {
                new PointF(rect.Left, rect.Top), new PointF(rect.Right, rect.Top),
                new PointF(rect.Right, rect.Bottom), new PointF(rect.Left, rect.Bottom) };
        }

        private static NotSupportedException CreateClipNotSupportedException()
        {
            return new NotSupportedException("The full coverage of a palette entry is not clipped: only the hatchings and the patterns of areas clip, which are repetitive geometry.");
        }

        private static NotSupportedException CreateBitmapNotSupportedException()
        {
            return new NotSupportedException("A palette entry has no bitmaps: the image layer and the layout layer draw them.");
        }

        // A path that the symbols created: its shape, and the rule that fills it.
        private sealed class CreatedPath
        {
            public readonly PathShape Shape;
            public readonly AreaFillMode FillRule;

            public CreatedPath(PathShape shape, AreaFillMode fillRule)
            {
                Shape = shape;
                FillRule = fillRule;
            }
        }

        // A font that the symbols created.
        private sealed class CreatedFont
        {
            private readonly string familyName;
            private readonly float emHeight;
            private readonly TextEffects effects;

            public CreatedFont(string familyName, float emHeight, TextEffects effects)
            {
                this.familyName = familyName;
                this.emHeight = emHeight;
                this.effects = effects;
            }

            // A run of text in the font.
            // text: the text.
            // upperLeft: the upper left corner of the text.
            // textMetrics: the metrics of the fonts.
            public TextShape MakeTextRun(string text, PointF upperLeft, ITextMetrics textMetrics)
            {
                return new TextShape(text, familyName, emHeight, effects, upperLeft, textMetrics);
            }
        }
    }
}
