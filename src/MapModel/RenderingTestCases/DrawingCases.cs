using System.Collections.Generic;
using System.Drawing;
using System.IO;
using PurplePen.Graphics2D;
using PurplePen.MapModel;
using SkiaSharp;
using TestingUtils;

namespace RenderingTestCases
{
    // The drawings that the rendering tests of the graphics targets draw: lines and shapes with pens of each cap
    // and join, fills, paths, clips, text, bitmaps and pattern brushes.
    public static class DrawingCases
    {
        // Every drawing case.
        public static readonly DrawingCase[] All = {
            new DrawingCase("SimpleLines", 500, RectangleF.FromLTRB(0, 0, 800, 1100), "simplelines.png", DrawSimpleLines),
            new DrawingCase("ComplexLines", 500, RectangleF.FromLTRB(0, 0, 800, 1100), "complexlines.png", DrawComplexLines),
            new DrawingCase("FillAreas", 500, RectangleF.FromLTRB(0, 0, 800, 1100), "fillareas.png", DrawFillAreas),
            new DrawingCase("Paths", 500, RectangleF.FromLTRB(0, 0, 800, 1100), "paths.png", DrawPaths),
            new DrawingCase("Clipping", 500, RectangleF.FromLTRB(0, 0, 800, 1100), "clipping.png", DrawClipping),
            new DrawingCase("Text", 500, RectangleF.FromLTRB(0, 0, 800, 1100), "text.png", DrawTextAndOutline),
            new DrawingCase("Bitmap", 1000, RectangleF.FromLTRB(0, 0, 800, 1100), "bitmap.png", DrawPhotographAtTwoSizes),
            new DrawingCase("BitmapPart", 1000, RectangleF.FromLTRB(0, 0, 800, 1100), "bitmappart.png", DrawPartOfPhotograph),
            new DrawingCase("PatternBrush", 800, new RectangleF(-103, -117, 200, 200), "patternbrush.png", DrawPatternBrush),
            new DrawingCase("RotatedPatternBrush", 800, new RectangleF(-103, -117, 200, 200), "rotatedpatternbrush.png", DrawRotatedPatternBrush),
        };

        // Draw lines, polylines and polygons with pens of three widths, caps and joins, anti-aliased.
        private static void DrawSimpleLines(IGraphicsTarget grTarget)
        {
            grTarget.PushAntiAliasing(true);

            object penKey1 = new object();
            grTarget.CreatePen(penKey1, CmykColor.FromColor(Color.DarkBlue), 20, LineCapMode.Flat, LineJoinMode.Bevel, 1);
            grTarget.DrawLine(penKey1, new PointF(100, 100), new PointF(600, 150));

            object penKey2 = new object();
            grTarget.CreatePen(penKey2, CmykColor.FromColor(Color.LightBlue), 40, LineCapMode.Round, LineJoinMode.Round, 1);
            grTarget.DrawLine(penKey2, new PointF(100, 200), new PointF(600, 250));

            object penKey3 = new object();
            grTarget.CreatePen(penKey3, CmykColor.FromColor(Color.IndianRed), 80, LineCapMode.Square, LineJoinMode.Miter, 2);
            grTarget.DrawLine(penKey3, new PointF(100, 300), new PointF(600, 350));

            grTarget.DrawPolyline(penKey1, new PointF[]
                 { new PointF(150, 400), new PointF(150, 450), new PointF(400, 400), new PointF(300, 500) });

            grTarget.DrawPolyline(penKey2, new PointF[]
                { new PointF(150, 500), new PointF(150, 550), new PointF(400, 500), new PointF(300, 600) });

            grTarget.DrawPolyline(penKey3, new PointF[]
                { new PointF(120, 700), new PointF(150, 750), new PointF(400, 700), new PointF(300, 800) });

            grTarget.DrawPolygon(penKey1, new PointF[]
                { new PointF(500, 400), new PointF(700, 500), new PointF(650, 600), new PointF(535, 520) });

            grTarget.DrawPolygon(penKey2, new PointF[]
                { new PointF(500, 600), new PointF(700, 700), new PointF(650, 800), new PointF(535, 720) });

            grTarget.DrawPolygon(penKey3, new PointF[]
                { new PointF(500, 800), new PointF(700, 900), new PointF(650, 1000), new PointF(535, 920) });

            grTarget.PopAntiAliasing();
        }

        // Draw a rectangle, an ellipse and an arc with a wide pen, and a rectangle with a thin one.
        private static void DrawComplexLines(IGraphicsTarget grTarget)
        {
            object penKey1 = new object();
            grTarget.CreatePen(penKey1, CmykColor.FromColor(Color.DarkBlue), 20, LineCapMode.Round, LineJoinMode.Round, 1);

            grTarget.DrawRectangle(penKey1, RectangleF.FromLTRB(100, 200, 600, 400));
            grTarget.DrawEllipse(penKey1, new PointF(350, 550), 250, 70);
            grTarget.DrawArc(penKey1, new PointF(500, 850), 150, 30, 200);

            object penKey2 = new object();
            grTarget.CreatePen(penKey2, CmykColor.FromColor(Color.IndianRed), 3, LineCapMode.Round, LineJoinMode.Round, 1);
            grTarget.DrawRectangle(penKey2, RectangleF.FromLTRB(100, 800, 600, 1000));
        }

        // Fill a rectangle and an ellipse, and a self-intersecting polygon with each fill rule.
        private static void DrawFillAreas(IGraphicsTarget grTarget)
        {
            object brushKey1 = new object();
            grTarget.CreateSolidBrush(brushKey1, CmykColor.FromRgb(0, 0, 1));

            grTarget.FillRectangle(brushKey1, RectangleF.FromLTRB(100, 200, 600, 400));
            grTarget.FillEllipse(brushKey1, new PointF(350, 550), 250, 70);

            object brushKey2 = new object();
            grTarget.CreateSolidBrush(brushKey2, CmykColor.FromRgb(0.5F, 0, 0));
            grTarget.FillPolygon(brushKey2, new PointF[]
                { new PointF(100,1000), new PointF(200, 800), new PointF(300, 1000), new PointF(100, 850), new PointF(300,850) },
                AreaFillMode.Alternate);

            grTarget.FillPolygon(brushKey2, new PointF[]
            { new PointF(500, 1000), new PointF(600, 800), new PointF(700, 1000), new PointF(500, 850), new PointF(700, 850) },
                AreaFillMode.Winding);
        }

        // Stroke and fill paths of lines and Bezier curves, under nested transforms.
        private static void DrawPaths(IGraphicsTarget grTarget)
        {
            Matrix mat = new Matrix();
            mat.Translate(425, 250);
            grTarget.PushTransform(mat);

            object penKey1 = new object(), pathKey1 = new object();
            grTarget.CreatePen(penKey1, CmykColor.FromColor(Color.Red), 7.0F, LineCapMode.Flat, LineJoinMode.Miter, 5F);
            grTarget.CreatePath(pathKey1, new List<GraphicsPathPart> {
                     new GraphicsPathPart(GraphicsPathPartKind.Start, new PointF[1] {new PointF(-70, -70)}),
                     new GraphicsPathPart(GraphicsPathPartKind.Lines,  new PointF[] { new PointF(-10, 50), new PointF(20, 50), new PointF(70, -90) }),
                     new GraphicsPathPart(GraphicsPathPartKind.Close, new PointF[0]),
                },
                AreaFillMode.Alternate);

            grTarget.DrawPath(penKey1, pathKey1);

            object penKey2 = new object(), pathKey2 = new object();
            grTarget.CreatePen(penKey2, CmykColor.FromColor(Color.Green), 3.0F, LineCapMode.Round, LineJoinMode.Round, 5F);
            grTarget.CreatePath(pathKey2, new List<GraphicsPathPart> {
                     new GraphicsPathPart(GraphicsPathPartKind.Start, new PointF[1] {new PointF(-70, -70)}),
                     new GraphicsPathPart(GraphicsPathPartKind.Beziers,  new PointF[] { new PointF(-10, 50), new PointF(20, 50), new PointF(70, -90) }),
                     new GraphicsPathPart(GraphicsPathPartKind.Start, new PointF[1] {new PointF(0, 0)}),
                     new GraphicsPathPart(GraphicsPathPartKind.Lines,  new PointF[] { new PointF(-50, 50), new PointF(50, 50) }),
                     new GraphicsPathPart(GraphicsPathPartKind.Close, new PointF[0] )},
                AreaFillMode.Alternate);
            grTarget.DrawPath(penKey2, pathKey2);

            mat = new Matrix();
            mat.Translate(0, 350);
            grTarget.PushTransform(mat);

            object brushKey3 = new object(), pathKey3 = new object();
            grTarget.CreateSolidBrush(brushKey3, CmykColor.FromColor(Color.IndianRed));
            grTarget.CreatePath(pathKey3, new List<GraphicsPathPart> {
                     new GraphicsPathPart(GraphicsPathPartKind.Start, new PointF[1] {new PointF(-70, -70)}),
                     new GraphicsPathPart(GraphicsPathPartKind.Beziers,  new PointF[] { new PointF(-10, 50), new PointF(20, 50), new PointF(70, -90) }),
                     new GraphicsPathPart(GraphicsPathPartKind.Close, new PointF[0] )},
                AreaFillMode.Alternate);
            grTarget.FillPath(brushKey3, pathKey3);
        }

        // Fill a rectangle and stroke an ellipse, clipped to a self-intersecting path under a scaling transform.
        private static void DrawClipping(IGraphicsTarget grTarget)
        {
            Matrix mat = new Matrix();
            mat.Translate(425, 250);
            mat.Scale(2, 2);
            grTarget.PushTransform(mat);

            object pathKey = new object(), brushKey = new object(), penKey = new object();

            grTarget.CreatePath(pathKey, new List<GraphicsPathPart> {
                    new GraphicsPathPart(GraphicsPathPartKind.Start, new PointF[1] {new PointF(-50, -30)}),
                    new GraphicsPathPart(GraphicsPathPartKind.Lines,  new PointF[] {new PointF(0, 80), new PointF(50, -30), new PointF(-50, 50), new PointF(50, 50)})},
                    AreaFillMode.Alternate);

            grTarget.PushClip(pathKey);

            grTarget.CreateSolidBrush(brushKey, CmykColor.FromColor(Color.Red));
            grTarget.FillRectangle(brushKey, new RectangleF(-100, -100, 200, 200));

            grTarget.CreatePen(penKey, CmykColor.FromColor(Color.Green), 15.0F, LineCapMode.Flat, LineJoinMode.Bevel, 5F);
            grTarget.DrawEllipse(penKey, new PointF(0, 0), 40, 50);

            grTarget.PopClip();
        }

        // Draw text, upright and rotated, and the outline of bold italic text.
        private static void DrawTextAndOutline(IGraphicsTarget grTarget)
        {
            grTarget.PushAntiAliasing(true);

            object fontKey = new object(), brushKey = new object();

            grTarget.CreateFont(fontKey, "Cambria", 90, TextEffects.Italic);
            grTarget.CreateSolidBrush(brushKey, CmykColor.FromColor(Color.BlueViolet));
            grTarget.DrawText("Hello", fontKey, brushKey, new PointF(100, 100));

            Matrix mat = new Matrix();
            mat.RotateAt(45, new PointF(100, 200));
            grTarget.PushTransform(mat);
            grTarget.DrawText("Hello", fontKey, brushKey, new PointF(100, 200));
            grTarget.PopTransform();

            object fontKey2 = new object(), penKey2 = new object();
            grTarget.CreateFont(fontKey2, "Times New Roman", 170, TextEffects.Bold | TextEffects.Italic);
            grTarget.CreatePen(penKey2, CmykColor.FromColor(Color.Crimson), 1, LineCapMode.Round, LineJoinMode.Round, 2);
            grTarget.DrawTextOutline("Hi There", fontKey2, penKey2, new PointF(100, 500));
        }

        // Draw a photograph twice, at two sizes.
        private static void DrawPhotographAtTwoSizes(IGraphicsTarget grTarget)
        {
            using (Skia_Bitmap penguins = LoadPenguins()) {
                grTarget.DrawBitmap(penguins, new RectangleF(100, 100, 600, 400), BitmapScaling.MediumQuality);
                grTarget.DrawBitmap(penguins, new RectangleF(200, 800, 150, 100), BitmapScaling.MediumQuality);
            }
        }

        // Draw the middle part of a photograph, scaled up with nearest-neighbor sampling.
        private static void DrawPartOfPhotograph(IGraphicsTarget grTarget)
        {
            using (Skia_Bitmap bitmap = LoadPenguins()) {
                grTarget.DrawBitmapPart(bitmap, bitmap.PixelWidth * 3 / 10, bitmap.PixelHeight * 2 / 10, bitmap.PixelWidth * 5 / 10, bitmap.PixelHeight * 4 / 10,
                                new RectangleF(100, 500, 600, 400), BitmapScaling.NearestNeighbor);
            }
        }

        // Fill a self-intersecting polygon with an upright pattern brush, and cross it with thin lines.
        private static void DrawPatternBrush(IGraphicsTarget grTarget)
        {
            DrawWithPatternBrush(grTarget, 0);
        }

        // Fill a self-intersecting polygon with a pattern brush turned by 147 degrees, and cross it with thin lines.
        private static void DrawRotatedPatternBrush(IGraphicsTarget grTarget)
        {
            DrawWithPatternBrush(grTarget, 147);
        }

        // Fill a self-intersecting polygon with a pattern brush of two lines and an ellipse, and cross it with thin
        // lines.
        // angle: the angle of the pattern, in degrees.
        private static void DrawWithPatternBrush(IGraphicsTarget grTarget, float angle)
        {
            IBrushTarget brushTarget = grTarget.CreatePatternBrush(new SizeF(30, 20), angle, 60, 60);

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
        }

        // Load the photograph that the bitmap cases draw.
        private static Skia_Bitmap LoadPenguins()
        {
            return new Skia_Bitmap(SKBitmap.Decode(TestUtil.GetTestFile(Path.Combine("pdfrender", "Penguins.jpg"))));
        }
    }
}
