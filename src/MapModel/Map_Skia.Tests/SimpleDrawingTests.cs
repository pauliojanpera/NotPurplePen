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
        public void Bitmap()
        {
            RenderingUtil.RenderingTest(1000, RectangleF.FromLTRB(0, 0, 800, 1100), false, TestUtil.GetTestFile("skia_render\\bitmap.png"),
                grTarget => {
                    Skia_Bitmap penguins = new Skia_Bitmap(SKBitmap.Decode(TestUtil.GetTestFile("pdfrender\\penguins.jpg")));
                    grTarget.DrawBitmap(penguins, new RectangleF(100, 100, 600, 400), BitmapScaling.MediumQuality);
                    grTarget.DrawBitmap(penguins, new RectangleF(200, 800, 150, 100), BitmapScaling.MediumQuality);
                }
            );
        }

        [Test]
        public void BitmapPart()
        {
            RenderingUtil.RenderingTest(1000, RectangleF.FromLTRB(0, 0, 800, 1100), false, TestUtil.GetTestFile("skia_render\\bitmappart.png"),
                grTarget => {
                    Skia_Bitmap bitmap = new Skia_Bitmap(SKBitmap.Decode(TestUtil.GetTestFile("pdfrender\\penguins.jpg")));
                    grTarget.DrawBitmapPart(bitmap, bitmap.PixelWidth * 3 / 10, bitmap.PixelHeight * 2 / 10, bitmap.PixelWidth * 5 / 10, bitmap.PixelHeight * 4 / 10,
                                    new RectangleF(100, 500, 600, 400), BitmapScaling.NearestNeighbor);
                }
            );
        }

        // Draw a circle, and two lines that cross each other and the circle, in the course's purple, blended with a blend
        // mode over bars of color.
        // grTarget: the target to draw on.
        // blendMode: the blend mode.
        // centerX: the horizontal center of the drawing.
        static void DrawBlendedPurple(IGraphicsTarget grTarget, BlendMode blendMode, float centerX)
        {
            object penPurple = new object();
            grTarget.CreatePen(penPurple, CmykColor.FromCmyk(0.35F, 0.85F, 0, 0), 24, LineCapMode.Round, LineJoinMode.Round, 5);

            Assert.IsTrue(grTarget.PushBlending(blendMode));
            grTarget.DrawEllipse(penPurple, new PointF(centerX, 300), 120, 120);
            grTarget.DrawLine(penPurple, new PointF(centerX - 150, 120), new PointF(centerX + 150, 480));
            grTarget.DrawLine(penPurple, new PointF(centerX - 150, 480), new PointF(centerX + 150, 120));
            grTarget.PopBlending();
        }

        // Blend by darken on the left and by multiply on the right, over bars of the colors of a map: light green, yellow,
        // blue, brown and black. Multiply comes out darker than darken over each but black. Each shape is blended as it
        // is drawn: where the purple overlaps itself, darken leaves it as it is, and multiply builds it up.
        [Test]
        public void BlendModes()
        {
            RenderingUtil.RenderingTest(600, RectangleF.FromLTRB(0, 0, 800, 600), false, TestUtil.GetTestFile("skia_render\\blendmodes.png"),
                grTarget => {
                    grTarget.PushAntiAliasing(true);

                    CmykColor[] barColors = { CmykColor.FromCmyk(0.455F, 0, 0.545F, 0), CmykColor.FromCmyk(0, 0.27F, 0.79F, 0),
                                              CmykColor.FromCmyk(0.87F, 0.18F, 0, 0), CmykColor.FromCmyk(0, 0.56F, 1, 0.18F),
                                              CmykColor.FromCmyk(0, 0, 0, 1) };
                    for (int bar = 0; bar < barColors.Length; ++bar) {
                        object penBar = new object();
                        grTarget.CreatePen(penBar, barColors[bar], 40, LineCapMode.Flat, LineJoinMode.Miter, 5);
                        grTarget.DrawLine(penBar, new PointF(20, 140 + bar * 80), new PointF(780, 140 + bar * 80));
                    }

                    DrawBlendedPurple(grTarget, BlendMode.Darken, 200);
                    DrawBlendedPurple(grTarget, BlendMode.Multiply, 600);

                    grTarget.PopAntiAliasing();
                }
            );
        }

        [Test]
        public void PatternBrush()
        {
            RenderingUtil.RenderingTest(800, new RectangleF(-103, -117, 200, 200), false, TestUtil.GetTestFile("skia_render\\patternbrush.png"),
                grTarget => {
                    IBrushTarget brushTarget = grTarget.CreatePatternBrush(new SizeF(30, 20), 0, 60, 60);
                    
                    object pen = new object();
                    brushTarget.CreatePen(pen, CmykColor.FromRgb(1, 0, 0), 1.5F, LineCapMode.Round, LineJoinMode.Round, 5F);
                    brushTarget.DrawLine(pen, new PointF(-15, -10), new PointF(3, 10));
                    pen = new object();
                    brushTarget.CreatePen(pen, CmykColor.FromRgb(0, 1, 0), 3.0F, LineCapMode.Round, LineJoinMode.Round, 5F);
                    brushTarget.DrawLine(pen, new PointF(3, 10), new PointF(15, -10));

                    pen = new object();
                    brushTarget.CreatePen(pen, CmykColor.FromRgb(0, 0, 1), 3F, LineCapMode.Round, LineJoinMode.Round, 5F);
                    brushTarget.DrawEllipse(pen, new PointF(1, -2), 5, 4);

                    object brush = new object();
                    brushTarget.FinishBrush(brush);

                    grTarget.FillPolygon(brush, new PointF[] { new PointF(-50, -60), new PointF(0, 30), new PointF(50, -60), new PointF(-50, 20), new PointF(50, 20) }, AreaFillMode.Winding);

                    object pen2 = new object();
                    grTarget.CreatePen(pen2, CmykColor.FromRgb(0, 0, 0), 0.5F, LineCapMode.Flat, LineJoinMode.Round, 5F);
                    grTarget.DrawLine(pen2, new PointF(-30, -30), new PointF(30, 30));
                    grTarget.DrawLine(pen2, new PointF(30, -30), new PointF(-30, 30));
                });
        }

        [Test]
        public void RotatedPatternBrush()
        {
            RenderingUtil.RenderingTest(800, new RectangleF(-103, -117, 200, 200), false, TestUtil.GetTestFile("skia_render\\rotatedpatternbrush.png"),
                grTarget => {
                    IBrushTarget brushTarget = grTarget.CreatePatternBrush(new SizeF(30, 20), 147, 60, 60);

                    object pen = new object();
                    brushTarget.CreatePen(pen, CmykColor.FromRgb(1, 0, 0), 1.5F, LineCapMode.Round, LineJoinMode.Round, 5F);
                    brushTarget.DrawLine(pen, new PointF(-15, -10), new PointF(3, 10));
                    pen = new object();
                    brushTarget.CreatePen(pen, CmykColor.FromRgb(0, 1, 0), 3.0F, LineCapMode.Round, LineJoinMode.Round, 5F);
                    brushTarget.DrawLine(pen, new PointF(3, 10), new PointF(15, -10));

                    pen = new object();
                    brushTarget.CreatePen(pen, CmykColor.FromRgb(0, 0, 1), 3F, LineCapMode.Round, LineJoinMode.Round, 5F);
                    brushTarget.DrawEllipse(pen, new PointF(1, -2), 5, 4);

                    object brush = new object();
                    brushTarget.FinishBrush(brush);

                    grTarget.FillPolygon(brush, new PointF[] { new PointF(-50, -60), new PointF(0, 30), new PointF(50, -60), new PointF(-50, 20), new PointF(50, 20) }, AreaFillMode.Winding);

                    pen = new object();
                    grTarget.CreatePen(pen, CmykColor.FromRgb(0, 0, 0), 0.5F, LineCapMode.Flat, LineJoinMode.Round, 5F);
                    grTarget.DrawLine(pen, new PointF(-30, -30), new PointF(30, 30));
                    grTarget.DrawLine(pen, new PointF(30, -30), new PointF(-30, 30));
                });
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
