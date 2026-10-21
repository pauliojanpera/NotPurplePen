using System;
using System.Collections.Generic;

namespace Map_Skia.Tests
{
    using System.Diagnostics;
    using System.Drawing;
    using System.IO;
    using System.Text;
    using NUnit.Framework;
    using PurplePen.Graphics2D;
    using PurplePen.MapModel;
    using RenderingTestCases;
    using SkiaSharp;
    using TestingUtils;

    [TestFixture]
    public class SimpleDrawingTests
    {
        // Draw each drawing case with Skia, and compare the image with the case's baseline.
        [TestCaseSource(typeof(RenderingUtil), nameof(RenderingUtil.DrawingTestCases))]
        public void Draw(DrawingCase drawingCase)
        {
            RenderingUtil.RenderingTest(drawingCase);
        }

        [Test]
        public unsafe void TextOutline()
        {

            RenderingUtil.RenderingTest(500, RectangleF.FromLTRB(0, 0, 800, 1100), false, TestUtil.GetTestFile(Path.Combine("skia_render", "text_outline.png")),
                grTarget => {
                    object fontKey = new object(), brushKey = new object();

                    grTarget.CreateFont(fontKey, "Cambria", 200, TextEffects.Italic);
                    grTarget.CreateSolidBrush(brushKey, CmykColor.FromColor(Color.BlueViolet));
                    grTarget.DrawText("Hellö", fontKey, brushKey, new PointF(100, 100));

                    SKCanvas canvas = ((Skia_GraphicsTarget)grTarget).Canvas;
                    SKPaint paint = new SKPaint();
                    SKFont font = new SKFont(SKTypeface.FromFamilyName("Cambria", SKFontStyleWeight.Normal, SKFontStyleWidth.Normal, SKFontStyleSlant.Italic), 200);
                    paint.Color = SKColors.Chartreuse;
                    byte[] bytes = Encoding.UTF8.GetBytes("Hellö");
                    SKPath path;
                    fixed(byte *p = bytes)
                    {
                        path = font.GetTextPath((IntPtr)p, bytes.Length, SKTextEncoding.Utf8, new SKPoint(100, 100 - font.Metrics.Ascent));
                    }
                    paint.StrokeWidth = 3;
                    paint.IsStroke = true;
                    canvas.DrawPath(path, paint);

                }
            );
        }

        [Test]
        public unsafe void TextPosOutline()
        {

            RenderingUtil.RenderingTest(500, RectangleF.FromLTRB(0, 0, 800, 1100), false, TestUtil.GetTestFile(Path.Combine("skia_render", "text_pos_outline.png")),
                grTarget => {
                    SKCanvas canvas = ((Skia_GraphicsTarget)grTarget).Canvas;
                    SKPaint paint = new SKPaint();
                    SKFont font = new SKFont(SKTypeface.FromFamilyName("Cambria", SKFontStyleWeight.Bold, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright), 300);
                    SKPoint[] points = { new SKPoint(125, 600), new SKPoint(189, 214), new SKPoint(278, 303), new SKPoint(397, 245), new SKPoint(545, 299) };
                    paint.Color = SKColors.Red;
                    for (int i = 0; i < points.Length; ++i) {
                        canvas.DrawText("Hellö".Substring(i, 1), points[i], font, paint);
                    }

                    SKPath path = font.GetTextPath("Hellö", points);
                    paint.IsStroke = true;
                    paint.Color = SKColors.DarkBlue;
                    paint.StrokeWidth = 15;
                    canvas.DrawPath(path, paint);

                    byte[] bytes = Encoding.UTF8.GetBytes("Hellö");
                    fixed (byte* p = bytes)
                    {
                        path = font.GetTextPath((IntPtr)p, bytes.Length, SKTextEncoding.Utf8, points);
                    }
                    paint.IsStroke = true;
                    paint.Color = SKColors.Yellow;
                    paint.StrokeWidth = 5;
                    canvas.DrawPath(path, paint);

                }
            );
        }

        [Test]
        public void SkiaTimeTeanWest()
        {
            // Create and open the map file.
            Map map = new Map(new Skia_TextMetrics(), null);
            InputOutput.ReadFile(TestUtil.GetTestFile(Path.Combine("rendering", "teanwest.ocd")), map);

            RenderingUtil.TimeMapRender(map, new Size(1814, 1022), new RectangleF(-347.3F, -275F, 408.3F, 230F), "TeanWest full");
            RenderingUtil.TimeMapRender(map, new Size(1814, 1022), new RectangleF(-347.3F, -275F, 408.3F, 230F), "TeanWest full");
            RenderingUtil.TimeMapRender(map, new Size(1814, 1022), new RectangleF(-347.3F, -275F, 408.3F, 230F), "TeanWest full");
            RenderingUtil.TimeMapRender(map, new Size(1814, 1022), new RectangleF(-347.3F, -275F, 408.3F, 230F), "TeanWest full");
            RenderingUtil.TimeMapRender(map, new Size(1814, 1022), new RectangleF(-347.3F, -275F, 408.3F, 230F), "TeanWest full");
        }

        [Test]
        public void SkiaTimeSalmon()
        {
            // Create and open the map file.
            Map map = new Map(new Skia_TextMetrics(), null);
            InputOutput.ReadFile(TestUtil.GetTestFile(Path.Combine("rendering", "SalmonLaSac.ocd")), map);

            RenderingUtil.TimeMapRender(map, new Size(1462, 1564), RectangleF.FromLTRB(-115.4F, -93.3F, 114.6F, 152.75F), "Salmon La Sac");
            RenderingUtil.TimeMapRender(map, new Size(1462, 1564), RectangleF.FromLTRB(-115.4F, -93.3F, 114.6F, 152.75F), "Salmon La Sac");
            RenderingUtil.TimeMapRender(map, new Size(1462, 1564), RectangleF.FromLTRB(-115.4F, -93.3F, 114.6F, 152.75F), "Salmon La Sac");
        }


    }
}
