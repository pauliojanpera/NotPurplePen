using System;
using System.Collections.Generic;
using System.Drawing;

namespace PurplePen.MapModel
{
    using PurplePen.Graphics2D;
    using SkiaSharp;

    // The Skia encoding: it writes the transparency groups of the palette colors that pass 2 makes (Map.Draw) on the SKCanvas
    // of a Skia graphics target (Skia_GraphicsTarget). The encoding and the graphics target share the canvas: its
    // transformations and clips, and the conversion of the colors, the intensity and the antialiasing of the target.
    // The group of a palette color is composited with SKCanvas.SaveLayer, whose paint gives the opacity α and the blend
    // mode B, when α is below 1 or B is Multiply. When α is 1 and B is Normal or Darken, compositing the group gives the
    // same result as drawing its content directly with B: source over is associative, and Darken of one opaque color is
    // idempotent. So the encoding draws such a group as the old drawing draws a palette color (Map.DrawColor). Skia blends
    // in RGB: Darken takes the minimum of each component, and Multiply their product.
    // The content of a group is the palette color coverages of the symbols, each written apart, in the order of the group,
    // so the union of a palette separation stays implicit. The clipping objects at the resolution of the drawing are
    // written with the steps of their placements, the runs of dashes, the offset paths and the repeated placements
    // expanded into single dashes and single placements, as the old drawing draws them, with the same Skia calls as the
    // graphics target makes for the same shapes. A hatching or a clipped area pattern is cut by its clipping object, the
    // area: it is a pattern brush of its pattern tile where the old drawing used a pattern brush, and the expansion of its
    // coverage prototype inside the clip of the area otherwise. The encoding does not keep the results of its own
    // conversions over the groups: the bitmaps of the pattern brushes are made for each group.
    public sealed class SkiaEncoding : IPaletteStackEncoding, IDisposable
    {
        private readonly Skia_GraphicsTarget target;

        // The fonts of the text runs, by family, size and effects, which the encoding keeps as its graphics target keeps its
        // fonts, until it is disposed.
        private readonly Dictionary<(string, float, TextEffects), SkiaFont> fonts = new Dictionary<(string, float, TextEffects), SkiaFont>();

        // Make the encoding of a Skia graphics target.
        // target: the graphics target, whose canvas the encoding writes.
        internal SkiaEncoding(Skia_GraphicsTarget target)
        {
            this.target = target;
        }

        // Write the transparency group of a palette color on the canvas, over what is drawn there.
        // group: the transparency group.
        public void WriteTransparencyGroup(TransparencyGroup group)
        {
            if (group == null)
                throw new ArgumentNullException(nameof(group));

            SKCanvas canvas = target.Canvas;
            int saveCount = canvas.SaveCount;
            bool isLayer = group.Opacity < 1 || group.BlendMode == ColorBlendMode.Multiply;
            SKBlendMode contentBlendMode;
            if (isLayer) {
                using (SKPaint layerPaint = new SKPaint()) {
                    float opacity = Math.Max(0, Math.Min(1, group.Opacity));
                    layerPaint.Color = new SKColor(255, 255, 255, (byte)Math.Round(opacity * 255));
                    layerPaint.BlendMode = GetSkBlendMode(group.BlendMode);
                    canvas.SaveLayer(layerPaint);
                }
                contentBlendMode = SKBlendMode.SrcOver;
            }
            else {
                contentBlendMode = (group.BlendMode == ColorBlendMode.Darken) ? SKBlendMode.Darken : target.CurrentBlendMode;
            }

            List<IDisposable> brushes = new List<IDisposable>();
            Dictionary<(PatternTile, int, int), PatternBrush> brushPaints = new Dictionary<(PatternTile, int, int), PatternBrush>();
            try {
                using (ShapeWriter writer = new ShapeWriter(canvas, target.ConvertColor(group.Color.ColorValue), target.AntiAlias, contentBlendMode, this)) {
                    foreach (SymbolCoverage coverage in group.SymbolCoverages) {
                        if (coverage.Prototype == CoveragePrototype.FullCoverage)
                            writer.Write(coverage.GetClippingObjects(group.Resolution));
                        else
                            WritePatternFill(writer, group, coverage, brushPaints, brushes);
                    }
                }
            }
            finally {
                canvas.RestoreToCount(saveCount);
                foreach (IDisposable brush in brushes)
                    brush.Dispose();
            }
        }

        // Write the palette color coverage of a hatching or a clipped area pattern, which its clipping object, the area,
        // cuts: a pattern brush of the pattern tile of a regular area pattern, when the group uses pattern brushes and the
        // brush is not too coarse at the resolution, as the old drawing chooses (AreaSymDef.DrawPatterns), and otherwise the
        // expansion of the coverage prototype inside the clip of the area, as the old drawing clips a hatching
        // (DrawHatchings) and an area pattern that it draws glyph by glyph (DrawPattern).
        // writer: the writer of the content of the group.
        // group: the transparency group.
        // coverage: the palette color coverage.
        // brushPaints: the pattern brushes of the group, by tile and size of bitmap.
        // brushes: the objects of the pattern brushes, which are disposed after the group.
        private void WritePatternFill(ShapeWriter writer, TransparencyGroup group, SymbolCoverage coverage, Dictionary<(PatternTile, int, int), PatternBrush> brushPaints, List<IDisposable> brushes)
        {
            SKCanvas canvas = target.Canvas;
            foreach (ClippingObject clippingObject in coverage.ClippingObjects) {
                FilledShape area = clippingObject as FilledShape;
                PathShape areaPath = (area != null) ? area.Shape as PathShape : null;
                if (areaPath == null || area.Placement.Steps.Count != 0)
                    throw new InvalidOperationException("The clipping object of a pattern fill is the area, a filled path at the identity placement.");

                AreaPatternPrototype pattern = coverage.Prototype as AreaPatternPrototype;
                if (pattern != null && group.UsePatternBrushes && !pattern.Pattern.irregular && !pattern.Tile.IsTooPixelatedAt(group.Resolution)) {
                    WritePatternBrush(writer, pattern.Tile, coverage, area, areaPath, group.Resolution, brushPaints, brushes);
                }
                else {
                    canvas.Save();
                    using (SKPath clip = Skia_GraphicsTarget.GetPath(new List<GraphicsPathPart>(areaPath.Parts), area.FillRule))
                        canvas.ClipPath(clip);
                    writer.Write(coverage.ExpandPrototype());
                    canvas.Restore();
                }
            }
        }

        // Write a pattern brush of a pattern tile cut by an area, as the old drawing writes the brush
        // (AreaSymDef.DrawPatternWithTexBrush). The shader of the brush repeats the bitmap of the tile and turns it by the
        // angle of the tile, within the placement of the brush for the area symbol, so the tile lies where M of the palette
        // color coverage places the coverage prototype. A placement that does not turn the brush, of an area symbol whose
        // angle is 0, is a step of the matrix of the shader, and the area is filled with the brush, as the old drawing fills
        // such an area. A placement that turns the brush is a step of the canvas, within the clip of the area, and the
        // rectangle that bounds the area in the coordinates of the placement is filled with the brush, as the old drawing
        // fills a turned area. So the pixels of the brush stay where the old brush was placed as M places it. The old brush
        // left out the rotation center of an area symbol whose angle is 0.
        // writer: the writer of the content of the group, whose paint the brush takes.
        // tile: the pattern tile.
        // coverage: the palette color coverage.
        // area: the clipping object of the coverage, the area.
        // areaPath: the path of the area.
        // resolution: the resolution of the drawing.
        // brushPaints: the pattern brushes of the group, by tile and size of bitmap.
        // brushes: the objects of the pattern brushes, which are disposed after the group.
        private void WritePatternBrush(ShapeWriter writer, PatternTile tile, SymbolCoverage coverage, FilledShape area, PathShape areaPath, float resolution,
                                       Dictionary<(PatternTile, int, int), PatternBrush> brushPaints, List<IDisposable> brushes)
        {
            Size bitmapSize = tile.GetBitmapSize(resolution);
            SKCanvas canvas = target.Canvas;
            Matrix placement = tile.GetBrushPlacement(coverage.Symbol);
            float[] elements = placement.Elements;
            bool turns = !(elements[0] == 1 && elements[1] == 0 && elements[2] == 0 && elements[3] == 1);

            if (!turns) {
                // The placement is a step of the matrix of the shader, and the area is filled with the brush.
                using (SKPaint brushPaint = new SKPaint()) {
                    brushPaint.Shader = MakePatternShader(tile, bitmapSize, placement, brushPaints, brushes);
                    brushPaint.IsAntialias = writer.AntiAlias;
                    brushPaint.BlendMode = writer.BlendMode;
                    using (SKPath path = Skia_GraphicsTarget.GetPath(new List<GraphicsPathPart>(areaPath.Parts), area.FillRule))
                        canvas.DrawPath(path, brushPaint);
                }
                return;
            }

            // The placement is a step of the canvas, within the clip of the area.
            using (SKPaint brushPaint = new SKPaint()) {
                brushPaint.Shader = MakePatternShader(tile, bitmapSize, null, brushPaints, brushes);
                brushPaint.IsAntialias = writer.AntiAlias;
                brushPaint.BlendMode = writer.BlendMode;

                canvas.Save();
                using (SKPath clip = Skia_GraphicsTarget.GetPath(new List<GraphicsPathPart>(areaPath.Parts), area.FillRule))
                    canvas.ClipPath(clip);
                canvas.Concat(Skia_GraphicsTarget.GetSkMatrix(placement));
                placement.Invert();
                RectangleF bounding = Geometry.BoundsOfTransformedRectangle(area.Shape.Bounds, placement);
                canvas.DrawRect(Skia_GraphicsTarget.GetSKRect(bounding), brushPaint);
                canvas.Restore();
            }
        }

        // The shader of a pattern brush of a pattern tile, as the graphics target makes it (FinishBrush): it repeats the
        // bitmap of the tile with cubic sampling and turns it by the angle of the tile, within an outer step. The bitmap is
        // made once for each tile and size in a group (MakePatternBrush), and the shaders within no outer step are kept
        // with it.
        // tile: the pattern tile.
        // bitmapSize: the size of the bitmap.
        // outer: the outer step of the matrix of the shader, or null for none.
        // brushPaints: the pattern brushes of the group, by tile and size of bitmap.
        // brushes: the objects of the pattern brushes, which are disposed after the group.
        private SKShader MakePatternShader(PatternTile tile, Size bitmapSize, Matrix outer, Dictionary<(PatternTile, int, int), PatternBrush> brushPaints, List<IDisposable> brushes)
        {
            PatternBrush brush;
            if (!brushPaints.TryGetValue((tile, bitmapSize.Width, bitmapSize.Height), out brush)) {
                brush = MakePatternBrush(tile, bitmapSize, brushes);
                brushPaints.Add((tile, bitmapSize.Width, bitmapSize.Height), brush);
            }
            if (outer == null)
                return brush.Shader;

            SKShader shader = SKShader.CreateImage(brush.Image, SKShaderTileMode.Repeat, SKShaderTileMode.Repeat, new SKSamplingOptions(new SKCubicResampler(1 / 3.0f, 1 / 3.0f)),
                                                   Skia_GraphicsTarget.GetSkMatrix(MakeShaderMatrix(tile, bitmapSize, outer)));
            brushes.Add(shader);
            return shader;
        }

        // The matrix of the shader of a pattern brush, as the graphics target makes it (FinishBrush): the bitmap moved so
        // that its center is the origin of the tile, scaled to the size of the tile and turned by the angle of the tile,
        // within an outer step.
        // tile: the pattern tile.
        // bitmapSize: the size of the bitmap.
        // outer: the outer step, or null for none.
        private static Matrix MakeShaderMatrix(PatternTile tile, Size bitmapSize, Matrix outer)
        {
            SizeF size = tile.Size;
            Matrix transform = (outer == null) ? new Matrix() : outer.Clone();
            transform.Rotate(tile.Angle);
            transform.Scale(size.Width / (float)bitmapSize.Width, size.Height / (float)bitmapSize.Height);
            transform.Translate(-bitmapSize.Width / 2F, -bitmapSize.Height / 2F);
            return transform;
        }

        // Make a pattern brush of a pattern tile, as the graphics target makes a pattern brush
        // (Skia_GraphicsTarget.CreatePatternBrush and FinishBrush): the tile rasterized into a bitmap of the size of the
        // brush, with antialiasing, the origin of the tile at the center of the bitmap, and an image shader that repeats
        // the bitmap with cubic sampling and turns it by the angle of the tile.
        // tile: the pattern tile.
        // bitmapSize: the size of the bitmap.
        // brushes: the list that gets the objects of the brush, which are disposed after the group.
        private PatternBrush MakePatternBrush(PatternTile tile, Size bitmapSize, List<IDisposable> brushes)
        {
            int bitmapWidth = bitmapSize.Width, bitmapHeight = bitmapSize.Height;
            SizeF size = tile.Size;

            // Create a new bitmap and fill it transparent.
            SKBitmap bitmap = new SKBitmap(bitmapWidth, bitmapHeight, SKImageInfo.PlatformColorType, SKAlphaType.Premul);
            brushes.Add(bitmap);
            bitmap.Erase(SKColors.Transparent);

            IntPtr length; // not needed.
            using (SKSurface surface = SKSurface.Create(new SKImageInfo(bitmapWidth, bitmapHeight, SKImageInfo.PlatformColorType, SKAlphaType.Premul), bitmap.GetPixels(out length), bitmap.RowBytes)) {
                SKCanvas tileCanvas = surface.Canvas;
                tileCanvas.Translate((float)bitmapWidth / 2F, (float)bitmapHeight / 2F);
                tileCanvas.Scale((float)bitmapWidth / size.Width, (float)bitmapHeight / size.Height);

                using (ShapeWriter tileWriter = new ShapeWriter(tileCanvas, target.ConvertColor(GetColorOfTile(tile)), true, SKBlendMode.SrcOver, this))
                    tileWriter.Write(tile.ClippingObjects);
            }

            // Scale and rotate.
            Matrix transform = MakeShaderMatrix(tile, bitmapSize, null);

            // Create an SKShader around this texture, using high quality sampling.
            SKImage image = SKImage.FromBitmap(bitmap);
            brushes.Add(image);
            SKShader shader = SKShader.CreateImage(image, SKShaderTileMode.Repeat, SKShaderTileMode.Repeat, new SKSamplingOptions(new SKCubicResampler(1 / 3.0f, 1 / 3.0f)),
                                                   Skia_GraphicsTarget.GetSkMatrix(transform));
            brushes.Add(shader);

            return new PatternBrush(image, shader);
        }

        // A pattern brush of a group: the image of the bitmap of a pattern tile, and its shader within no outer step.
        private sealed class PatternBrush
        {
            public readonly SKImage Image;
            public readonly SKShader Shader;

            // Make a pattern brush.
            // image: the image of the bitmap.
            // shader: the shader.
            public PatternBrush(SKImage image, SKShader shader)
            {
                Image = image;
                Shader = shader;
            }
        }

        // The color of the glyph parts of a pattern tile: the color C of the palette color of its area pattern.
        // tile: the pattern tile.
        private static CmykColor GetColorOfTile(PatternTile tile)
        {
            return tile.Color.ColorValue;
        }

        // The font of a text run, which the encoding makes once for each family, size and effects.
        // text: the text run.
        private SkiaFont GetFont(TextShape text)
        {
            SkiaFont font;
            if (!fonts.TryGetValue((text.FontFamily, text.EmHeight, text.Effects), out font)) {
                font = new SkiaFont(text.FontFamily, text.EmHeight, text.Effects);
                fonts.Add((text.FontFamily, text.EmHeight, text.Effects), font);
            }
            return font;
        }

        // The blend mode of Skia for a blend mode of a palette color.
        // blendMode: the blend mode.
        private static SKBlendMode GetSkBlendMode(ColorBlendMode blendMode)
        {
            switch (blendMode) {
                case ColorBlendMode.Darken:
                    return SKBlendMode.Darken;
                case ColorBlendMode.Multiply:
                    return SKBlendMode.Multiply;
                default:
                    return SKBlendMode.SrcOver;
            }
        }

        // Dispose the fonts that the encoding made.
        public void Dispose()
        {
            foreach (SkiaFont font in fonts.Values)
                font.Dispose();
            fonts.Clear();
        }

        // A writer of clipping objects in one color on a canvas: each shape filled or stroked at its placement, with the
        // paints that the graphics target makes for the same shapes (Skia_GraphicsTarget). It makes a paint for each pen,
        // which it disposes when it is disposed.
        private sealed class ShapeWriter : IDisposable
        {
            private readonly SKCanvas canvas;
            private readonly SKColor color;
            private readonly bool antiAlias;
            private readonly SKBlendMode blendMode;
            private readonly SkiaEncoding encoding;
            private readonly SKPaint fillPaint;
            private readonly Dictionary<Stroke, SKPaint> strokePaints = new Dictionary<Stroke, SKPaint>();

            // Make a writer.
            // canvas: the canvas.
            // color: the color of the shapes.
            // antiAlias: whether the shapes are antialiased.
            // blendMode: the blend mode with which the shapes are drawn.
            // encoding: the encoding, which keeps the fonts.
            public ShapeWriter(SKCanvas canvas, SKColor color, bool antiAlias, SKBlendMode blendMode, SkiaEncoding encoding)
            {
                this.canvas = canvas;
                this.color = color;
                this.antiAlias = antiAlias;
                this.blendMode = blendMode;
                this.encoding = encoding;

                fillPaint = new SKPaint();
                fillPaint.Color = color;
                fillPaint.IsStroke = false;
                fillPaint.IsAntialias = antiAlias;
                fillPaint.BlendMode = blendMode;
            }

            // Whether the shapes are antialiased.
            public bool AntiAlias
            {
                get { return antiAlias; }
            }

            // The blend mode with which the shapes are drawn.
            public SKBlendMode BlendMode
            {
                get { return blendMode; }
            }

            // Write clipping objects in their order. The consecutive clipping objects of one repeated placement are written
            // one pose at a time, in their order at each pose, as the old drawing draws a glyph at a time. The runs of
            // dashes are expanded into single dashes (StrokedShape.ExpandDashes), and an offset path is drawn as its
            // offset curve.
            // clippingObjects: the clipping objects.
            public void Write(IReadOnlyList<ClippingObject> clippingObjects)
            {
                int i = 0;
                while (i < clippingObjects.Count) {
                    RepeatedPlacement repeated = clippingObjects[i].Placement as RepeatedPlacement;
                    if (repeated != null) {
                        int last = i;
                        while (last + 1 < clippingObjects.Count && clippingObjects[last + 1].Placement == repeated)
                            ++last;
                        foreach (SinglePlacement placement in repeated.Expand()) {
                            for (int k = i; k <= last; ++k)
                                WriteExpanded(clippingObjects[k], placement);
                        }
                        i = last + 1;
                    }
                    else {
                        WriteExpanded(clippingObjects[i], (SinglePlacement)clippingObjects[i].Placement);
                        ++i;
                    }
                }
            }

            // Write a clipping object at a single placement, with its runs of dashes expanded into single dashes and its
            // offset path as its offset curve.
            // clippingObject: the clipping object.
            // placement: the single placement.
            private void WriteExpanded(ClippingObject clippingObject, SinglePlacement placement)
            {
                StrokedShape stroked = clippingObject as StrokedShape;
                if (stroked != null) {
                    if (stroked.Stroke.DashPattern != null) {
                        foreach (StrokedShape dash in stroked.ExpandDashes())
                            WriteShape(dash, placement);
                        return;
                    }

                    OffsetPathShape offsetPath = stroked.Shape as OffsetPathShape;
                    if (offsetPath != null) {
                        WriteShape(new StrokedShape(offsetPath.OffsetCurve, stroked.Stroke, placement), placement);
                        return;
                    }
                }

                WriteShape(clippingObject, placement);
            }

            // Write the shape of a clipping object, filled or stroked, at a single placement: within the steps of the
            // placement, each a transformation of the canvas, as the drawing code of the symbols pushes them on the
            // graphics target. The innermost step of a text run takes the mirror of the Y axis of the run, so the canvas
            // gets the transformation with which the drawing lays out the text, and the run is drawn at its top left corner
            // with the opposite y coordinate.
            // clippingObject: the clipping object, whose shape is a path, an ellipse, an arc or a text run.
            // placement: the single placement.
            private void WriteShape(ClippingObject clippingObject, SinglePlacement placement)
            {
                Shape shape = clippingObject.Shape;
                TextShape text = shape as TextShape;
                IReadOnlyList<Matrix> steps = placement.Steps;

                int saveCount = canvas.Save();
                for (int i = 0; i < steps.Count; ++i) {
                    Matrix step = steps[i];
                    if (text != null && i == steps.Count - 1)
                        step.Scale(1, -1);
                    canvas.Concat(Skia_GraphicsTarget.GetSkMatrix(step));
                }
                if (text != null && steps.Count == 0) {
                    Matrix mirror = new Matrix();
                    mirror.Scale(1, -1);
                    canvas.Concat(Skia_GraphicsTarget.GetSkMatrix(mirror));
                }

                FilledShape filled = clippingObject as FilledShape;
                if (filled != null)
                    Fill(shape, filled.FillRule);
                else
                    Stroke(shape, GetStrokePaint(((StrokedShape)clippingObject).Stroke));

                canvas.RestoreToCount(saveCount);
            }

            // Fill a shape, as the graphics target fills a path (FillPath and FillPolygon), a rectangle (FillRectangle), an
            // ellipse (FillEllipse) and a text (DrawText).
            // shape: the shape.
            // fillRule: the fill rule of a path.
            private void Fill(Shape shape, AreaFillMode fillRule)
            {
                PathShape path = shape as PathShape;
                if (path != null && path.Rectangle.HasValue) {
                    canvas.DrawRect(Skia_GraphicsTarget.GetSKRect(path.Rectangle.Value), fillPaint);
                    return;
                }
                if (path != null) {
                    using (SKPath skPath = Skia_GraphicsTarget.GetPath(new List<GraphicsPathPart>(path.Parts), fillRule))
                        canvas.DrawPath(skPath, fillPaint);
                    return;
                }

                EllipseShape ellipse = shape as EllipseShape;
                if (ellipse != null) {
                    canvas.DrawOval(GetEllipseRect(ellipse), fillPaint);
                    return;
                }

                TextShape text = shape as TextShape;
                if (text != null) {
                    SkiaFont font = encoding.GetFont(text);
                    using (SKPaint paint = new SKPaint()) {
                        paint.Color = color;
                        paint.IsAntialias = antiAlias;
                        paint.BlendMode = blendMode;
                        font.EnhancedTypeface.DrawText(canvas, text.Text, new SKPoint(text.UpperLeft.X, -text.UpperLeft.Y), font.EmHeight, paint);
                    }
                    return;
                }

                throw new ArgumentException("Only a path, an ellipse and a text run are filled.", nameof(shape));
            }

            // Stroke a shape, as the graphics target draws a path (DrawPath, DrawPolyline, DrawPolygon and DrawLine), an
            // ellipse (DrawEllipse), an arc (DrawArc) and the outline of a text (DrawTextOutline).
            // shape: the shape.
            // strokePaint: the paint of the pen.
            private void Stroke(Shape shape, SKPaint strokePaint)
            {
                PathShape path = shape as PathShape;
                if (path != null) {
                    using (SKPath skPath = Skia_GraphicsTarget.GetPath(new List<GraphicsPathPart>(path.Parts), AreaFillMode.Winding))
                        canvas.DrawPath(skPath, strokePaint);
                    return;
                }

                EllipseShape ellipse = shape as EllipseShape;
                if (ellipse != null) {
                    canvas.DrawOval(GetEllipseRect(ellipse), strokePaint);
                    return;
                }

                ArcShape arc = shape as ArcShape;
                if (arc != null) {
                    SKRect oval = Skia_GraphicsTarget.GetSKRect(Geometry.RectangleFromCenterSize(arc.Center, new SizeF(arc.Radius * 2, arc.Radius * 2)));
                    using (SKPath skPath = new SKPath()) {
                        skPath.AddArc(oval, arc.StartAngle, arc.SweepAngle);
                        canvas.DrawPath(skPath, strokePaint);
                    }
                    return;
                }

                TextShape text = shape as TextShape;
                if (text != null) {
                    SkiaFont font = encoding.GetFont(text);
                    using (SKPaint paint = new SKPaint()) {
                        paint.IsAntialias = antiAlias;
                        paint.BlendMode = blendMode;
                        paint.IsStroke = true;
                        paint.Color = strokePaint.Color;
                        paint.StrokeWidth = strokePaint.StrokeWidth;
                        paint.StrokeJoin = strokePaint.StrokeJoin;
                        paint.StrokeCap = strokePaint.StrokeCap;
                        paint.StrokeMiter = strokePaint.StrokeMiter;
                        font.EnhancedTypeface.DrawText(canvas, text.Text, new SKPoint(text.UpperLeft.X, -text.UpperLeft.Y), font.EmHeight, paint);
                    }
                    return;
                }

                throw new ArgumentException("A stroked shape is a path, an ellipse, an arc or a text run.", nameof(shape));
            }

            // The rectangle of an ellipse, as the graphics target makes it for DrawOval.
            // ellipse: the ellipse.
            private static SKRect GetEllipseRect(EllipseShape ellipse)
            {
                return Skia_GraphicsTarget.GetSKRect(Geometry.RectangleFromCenterSize(ellipse.Center, new SizeF(ellipse.RadiusX * 2, ellipse.RadiusY * 2)));
            }

            // The paint of a pen, as the graphics target makes the paint of a pen (CreatePen): the color, the width, the
            // caps, the join, and the miter limit for a miter join.
            // stroke: the pen.
            private SKPaint GetStrokePaint(Stroke stroke)
            {
                SKPaint paint;
                if (!strokePaints.TryGetValue(stroke, out paint)) {
                    paint = Skia_GraphicsTarget.MakePenPaint(color, stroke.Width, stroke.Caps, stroke.Join, stroke.MiterLimit);
                    paint.IsAntialias = antiAlias;
                    paint.BlendMode = blendMode;
                    strokePaints.Add(stroke, paint);
                }
                return paint;
            }

            // Dispose the paints.
            public void Dispose()
            {
                fillPaint.Dispose();
                foreach (SKPaint paint in strokePaints.Values)
                    paint.Dispose();
                strokePaints.Clear();
            }
        }
    }
}
