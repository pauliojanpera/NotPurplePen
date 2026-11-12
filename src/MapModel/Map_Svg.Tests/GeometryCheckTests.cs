using System.Collections.Generic;
using System.Drawing;
using Map_SkiaStd;
using NUnit.Framework;
using PurplePen.Graphics2D;
using PurplePen.MapModel;
using SkiaSharp;

namespace Map_Svg.Tests
{
    // Checks the geometry check of the rendering comparison (V-10): the document the SVG target writes of a drawing
    // agrees with the drawing's calls, and calls that differ from the document by a little more than its rounding allows
    // are told apart from it, whatever they draw: a line's end, a stroke's width, an arc's direction, an ellipse's
    // radius, a curve's control point, a drawing left out, a clip, a pattern's tile, a text's place or characters, and a
    // bitmap's place.
    [TestFixture]
    public class GeometryCheckTests
    {
        // The errors planted in a drawing's calls, each moving one thing a little further than the document's rounding
        // allows, or changing what is drawn.
        public enum PlantedError
        {
            None, LineEndMoved, PenWider, ArcTurnedTheOtherWay, EllipseWider, CurveBent, PolygonLeftOut, ClipMoved,
            TileDotMoved, TextMoved, TextChanged, BitmapMoved,
        }

        // How far a planted error moves a point: five steps of 0.001 mm, above the allowance of any point of the
        // drawing, under three steps.
        private const float PlantedDistance = 0.005F;

        // The area of the drawing's coordinates that the page shows, which are the page's millimetres.
        private static readonly RectangleF Area = new RectangleF(0, 0, 100, 50);

        // The metrics the Skia target lays text out with, which the SVG target lays it out with too (T-1).
        private static readonly ITextMetrics textMetrics = new Skia_TextMetrics();

        // The font data of the faces drawn in, cut to the characters drawn, which the document embeds (T-6).
        private static readonly IFontSubsetProvider fontSubsets = new SkiaFontSubsetProvider();

        [Test]
        public void DocumentAgreesWithItsCalls()
        {
            GeometryComparison comparison = CompareDocumentWithCalls(PlantedError.None);
            Assert.IsNull(comparison.Describe("the drawing"));
            Assert.AreEqual(11, comparison.DrawingsCompared, "The page's clip, the eight drawings, the polygon's clip and the tile's dot.");
        }

        [TestCase(PlantedError.LineEndMoved, "DrawLine, call 1")]
        [TestCase(PlantedError.PenWider, "length 1")]
        [TestCase(PlantedError.ArcTurnedTheOtherWay, "DrawArc, call 2")]
        [TestCase(PlantedError.EllipseWider, "DrawEllipse, call 3")]
        [TestCase(PlantedError.CurveBent, "FillPath, call 4")]
        [TestCase(PlantedError.PolygonLeftOut, "FillRectangle, call 6 draws")]
        [TestCase(PlantedError.ClipMoved, "PushClip")]
        [TestCase(PlantedError.TileDotMoved, "of a pattern's tile")]
        [TestCase(PlantedError.TextMoved, "DrawText")]
        [TestCase(PlantedError.TextChanged, "'Geometrie'")]
        [TestCase(PlantedError.BitmapMoved, "DrawBitmap")]
        public void PlantedErrorIsFound(PlantedError error, string expectedInProblem)
        {
            string problems = CompareDocumentWithCalls(error).Describe("the drawing");
            Assert.IsNotNull(problems, "The geometry check finds no difference between the document and the calls with " + error + ".");
            StringAssert.Contains(expectedInProblem, problems);
        }

        // Write the document of the drawing without any planted error, and compare it with the calls of the drawing
        // with an error planted in them.
        // error: the error planted in the calls.
        private static GeometryComparison CompareDocumentWithCalls(PlantedError error)
        {
            using (SKBitmap pixels = new SKBitmap(4, 2))
            using (Skia_Bitmap bitmap = new Skia_Bitmap(pixels))
            using (RecordingGraphicsTarget recording = new RecordingGraphicsTarget(new SkiaBitmapGraphicsLoader()))
            using (Matrix drawingToPage = new Matrix()) {
                Draw(recording, bitmap, error);
                recording.EndRecording();
                return SvgGeometryCheck.Compare(Affine.FromMatrix(drawingToPage), Area, recording, WriteDocument(bitmap, drawingToPage), textMetrics);
            }
        }

        // Write the document of the drawing without any planted error: one map in one part, shown on a page whose
        // millimetres are the drawing's coordinates.
        // bitmap: the bitmap the drawing draws.
        // drawingToPage: the transform from the drawing's coordinates to the page's.
        private static string WriteDocument(IGraphicsBitmap bitmap, Matrix drawingToPage)
        {
            using (Svg_GraphicsTarget target = new Svg_GraphicsTarget(DefaultColorConverter.Instance, true, SvgBitmapOptions.Default, textMetrics, fontSubsets)) {
                object part = new object();
                target.BeginPart(part);
                target.BeginMap(new object(), "map");
                Draw(target, bitmap, PlantedError.None);
                target.EndMap();
                target.EndPart();
                target.AddPage(Area.Size, Area, drawingToPage, new object[] { part }, null);
                return target.FinishDocument();
            }
        }

        // Draw a line, an arc, an ellipse and a curve under a turn, a polygon within a clip, a rectangle filled with a
        // pattern of dots, a text and a bitmap, with an error planted in one of them.
        // target: the target to draw into.
        // bitmap: the bitmap to draw.
        // error: the error to plant.
        private static void Draw(IGraphicsTarget target, IGraphicsBitmap bitmap, PlantedError error)
        {
            CmykColor black = CmykColor.FromCmyk(0, 0, 0, 1);
            object pen = new object(), brush = new object(), pattern = new object(), font = new object();
            target.CreatePen(pen, black, error == PlantedError.PenWider ? 0.25F + 0.001F : 0.25F, LineCapMode.Flat, LineJoinMode.Miter, 4);
            target.CreateSolidBrush(brush, black);

            target.DrawLine(pen, new PointF(5, 5), new PointF(45, error == PlantedError.LineEndMoved ? 8 + PlantedDistance : 8));
            target.DrawArc(pen, new PointF(60, 10), 6, 30, error == PlantedError.ArcTurnedTheOtherWay ? -200 : 200);

            using (Matrix turn = new Matrix()) {
                turn.Translate(20, 25);
                turn.Rotate(30);
                target.PushTransform(turn);
                target.DrawEllipse(pen, new PointF(0, 0), error == PlantedError.EllipseWider ? 4 + PlantedDistance : 4, 2.5F);
                target.FillPath(brush, new List<GraphicsPathPart> {
                    new GraphicsPathPart(GraphicsPathPartKind.Start, new[] { new PointF(6, 0) }),
                    new GraphicsPathPart(GraphicsPathPartKind.Beziers, new[] {
                        new PointF(8, error == PlantedError.CurveBent ? 3 + PlantedDistance : 3), new PointF(12, 3), new PointF(14, 0) }),
                    new GraphicsPathPart(GraphicsPathPartKind.Close, new PointF[0]),
                }, AreaFillMode.Winding);
                target.PopTransform();
            }

            target.PushClip(new RectangleF(30, 20, error == PlantedError.ClipMoved ? 10 + PlantedDistance : 10, 10));
            if (error != PlantedError.PolygonLeftOut)
                target.FillPolygon(brush, new[] { new PointF(28, 18), new PointF(45, 22), new PointF(35, 35) }, AreaFillMode.Alternate);
            target.PopClip();

            IBrushTarget tile = target.CreatePatternBrush(new SizeF(2, 2), 30, 20, 20);
            object dot = new object();
            tile.CreateSolidBrush(dot, black);
            tile.FillEllipse(dot, new PointF(error == PlantedError.TileDotMoved ? PlantedDistance : 0, 0), 0.3F, 0.3F);
            tile.FinishBrush(pattern);
            target.FillRectangle(pattern, new RectangleF(50, 20, 15, 10));

            target.CreateFont(font, "Arial", 3, TextEffects.Regular);
            target.DrawText(error == PlantedError.TextChanged ? "Geometrie" : "Geometry", font, brush,
                            new PointF(70, error == PlantedError.TextMoved ? 35 + PlantedDistance : 35));
            target.DrawBitmap(bitmap, new RectangleF(error == PlantedError.BitmapMoved ? 5 + PlantedDistance : 5, 35, 8, 4), BitmapScaling.NearestNeighbor);
        }
    }
}
