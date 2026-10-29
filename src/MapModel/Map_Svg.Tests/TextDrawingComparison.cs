using System;
using System.Drawing;
using System.Threading.Tasks;
using Map_SkiaStd;
using PurplePen.Graphics2D;
using PurplePen.MapModel;
using RenderingTestCases;
using SkiaSharp;

namespace Map_Svg.Tests
{
    // Compares a drawing of text through the Skia target with the same drawing through the SVG target, its document
    // rendered in Chromium, under the tolerance for text (T-10).
    public static class TextDrawingComparison
    {
        // Draw text through the Skia target and through the SVG target, render the document in Chromium, and describe
        // how far its image lies from Skia's, or return null if within the tolerance for text.
        // name: the name of the drawing among the run's SVGs.
        // area: the area of drawing coordinates the image shows.
        // pixels: the size of the image.
        // draw: draws the text.
        // locale: the locale of the browser that renders the document, a language tag, or null for the browser of no
        // locale of its own.
        public static async Task<string> CompareAsync(string name, RectangleF area, Size pixels, Action<IGraphicsTarget> draw, string locale = null)
        {
            using (SKBitmap reference = new SKBitmap(pixels.Width, pixels.Height)) {
                using (SKCanvas canvas = new SKCanvas(reference)) {
                    canvas.Clear(SKColors.White);
                    using (Skia_GraphicsTarget target = new Skia_GraphicsTarget(canvas))
                    using (Matrix areaToImage = AreaInImage.CreateTransform(area, pixels, false)) {
                        target.PushTransform(areaToImage);
                        target.PushAntiAliasing(true);
                        draw(target);
                    }
                }

                string document;
                using (Svg_GraphicsTarget target = new Svg_GraphicsTarget(DefaultColorConverter.Instance, true, SvgBitmapOptions.Default, new Skia_TextMetrics(), new SkiaFontSubsetProvider()))
                using (Matrix areaToImage = AreaInImage.CreateTransform(area, pixels, false)) {
                    object part = new object(), map = new object();
                    target.BeginPart(part);
                    target.BeginMap(map, name);
                    draw(target);
                    target.EndMap();
                    target.EndPart();
                    target.AddPage(new SizeF(pixels.Width, pixels.Height), area, areaToImage, new object[] { part });
                    document = target.FinishDocument();
                }
                using (SKBitmap rendering = await ChromiumRenderer.RenderAsync(LoopResults.SaveSvg(name, document), pixels, locale))
                    return ImageComparison.DescribeDifference(name, reference, rendering, PixelTolerance.Text);
            }
        }
    }
}
