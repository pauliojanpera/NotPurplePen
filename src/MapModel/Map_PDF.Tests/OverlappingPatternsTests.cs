using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using NUnit.Framework;
using PurplePen.Graphics2D;
using PurplePen.MapModel;
using SkiaSharp;

namespace Map_PDF.Tests
{
    // Tests that PDF export draws the hatchings and clipped patterns of areas of one symbol that overlap once, within
    // the union of their outlines, as the map code asks every target to: two such areas draw as one area whose outline
    // is their union, as the PDF renders, where the same areas of two symbols draw their overlap twice.
    [TestFixture]
    public class OverlappingPatternsTests
    {
        // The part of the map drawn, in millimetres, and the size of the page it is drawn on, in pixels at 100 dpi.
        static readonly RectangleF mapArea = new RectangleF(0, 0, 16, 16);
        const int pixelSize = 400;

        // How far a channel of a pixel may differ between the two renderings compared: a line drawn over another
        // stretch is smoothed a little differently along its edges, where a pattern drawn twice darkens much more.
        const int LineEdgeTolerance = 24;

        // A closed polygon as an area's outline.
        // coordinates: the corners, x and y in turn.
        static SymPathWithHoles Polygon(params float[] coordinates)
        {
            PointF[] points = new PointF[coordinates.Length / 2 + 1];
            for (int i = 0; i < coordinates.Length / 2; ++i)
                points[i] = new PointF(coordinates[2 * i], coordinates[2 * i + 1]);
            points[points.Length - 1] = points[0];

            return new SymPathWithHoles(new SymPath(points), null);
        }

        // Create a map with one black color and areas of symbols that draw a hatching, or a clipped pattern of dots.
        // dots: whether the symbols draw dots rather than a hatching.
        // symbolOfArea: for each area, the index of its symbol.
        // areas: the areas' outlines.
        static Map CreateMap(bool dots, int[] symbolOfArea, params SymPathWithHoles[] areas)
        {
            Map map = new Map(new Skia_TextMetrics(), null);

            using (map.Write()) {
                SymColor black = map.AddColor("Black", 1, 0, 0, 0, 1, false);
                AreaSymDef[] symdefs = new AreaSymDef[areas.Length];
                for (int i = 0; i < areas.Length; ++i) {
                    int symbol = symbolOfArea[i];
                    if (symdefs[symbol] == null) {
                        AreaSymDef symdef = new AreaSymDef("Area " + symbol, (301 + symbol).ToString(CultureInfo.InvariantCulture) + ".0", null, null);
                        if (dots) {
                            Glyph glyph = new Glyph();
                            glyph.AddFilledCircle(black, new PointF(0, 0), 0.3F);
                            glyph.ConstructionComplete();

                            AreaSymDef.PatternInfo patternInfo = new AreaSymDef.PatternInfo();
                            patternInfo.offsetRows = true;
                            patternInfo.patternWidth = 0.9F;
                            patternInfo.patternHeight = 0.8F;
                            patternInfo.patternGlyph = glyph;
                            patternInfo.patternRotateMode = AreaSymDef.RotateMode.Always;
                            patternInfo.patternFillMode = AreaSymDef.PatternFillMode.Clip;
                            symdef.AddPattern(patternInfo);
                        }
                        else {
                            AreaSymDef.HatchInfo hatchInfo = new AreaSymDef.HatchInfo();
                            hatchInfo.hatchColor = black;
                            hatchInfo.hatchWidth = 0.12F;
                            hatchInfo.hatchSpacing = 0.5F;
                            hatchInfo.hatchAngle = 30;
                            hatchInfo.hatchRotateMode = AreaSymDef.RotateMode.Always;
                            symdef.AddHatching(hatchInfo);
                        }

                        map.AddSymdef(symdef);
                        symdefs[symbol] = symdef;
                    }

                    map.AddSymbol(new AreaSymbol(symdefs[symbol], areas[i], 0, new PointF()));
                }
            }

            return map;
        }

        // Export a map's area as a PDF page and render the page.
        // map: the map.
        // name: the name of the files written, in the folder for temporary files.
        static SKBitmap RenderPdf(Map map, string name)
        {
            string directory = Path.Combine(Path.GetTempPath(), "OverlappingPatternsTests");
            Directory.CreateDirectory(directory);
            string pdfFileName = Path.Combine(directory, name + ".pdf"), pngFileName = Path.Combine(directory, name + ".png");

            Matrix matrix = new Matrix();
            matrix.Translate(0, pixelSize);
            matrix.Scale(pixelSize / mapArea.Width, -pixelSize / mapArea.Height);
            matrix.Translate(-mapArea.Left, -mapArea.Top);

            RenderOptions renderOpts = new RenderOptions();
            renderOpts.minResolution = mapArea.Width / pixelSize;
            renderOpts.usePatternBitmaps = false;
            renderOpts.renderTemplates = RenderTemplateOption.MapOnly;

            PdfCreation.CreatePdfAndPng(pdfFileName, pngFileName, pixelSize, pixelSize, false, (IGraphicsTarget grTarget) => {
                grTarget.PushTransform(matrix);
                using (map.Read())
                    map.Draw(grTarget, mapArea, renderOpts, null);
                grTarget.PopTransform();
            });

            return SKBitmap.Decode(pngFileName);
        }

        // Count the pixels of two bitmaps of one size that differ by more than the tolerance in a channel.
        // a, b: the bitmaps.
        static int CountDifferingPixels(SKBitmap a, SKBitmap b)
        {
            Assert.That(b.Width, Is.EqualTo(a.Width));
            Assert.That(b.Height, Is.EqualTo(a.Height));

            int count = 0;
            for (int y = 0; y < a.Height; ++y) {
                for (int x = 0; x < a.Width; ++x) {
                    SKColor ca = a.GetPixel(x, y), cb = b.GetPixel(x, y);
                    if (Math.Abs(ca.Red - cb.Red) > LineEdgeTolerance || Math.Abs(ca.Green - cb.Green) > LineEdgeTolerance ||
                        Math.Abs(ca.Blue - cb.Blue) > LineEdgeTolerance)
                        ++count;
                }
            }

            return count;
        }

        // Check that two overlapping areas of one symbol export as their union does, and that the same areas of two
        // symbols export differently, as their overlap is drawn twice.
        // dots: whether the symbols draw dots rather than a hatching.
        // name: the name of the test, for its files.
        static void CheckExportedAsUnion(bool dots, string name)
        {
            SymPathWithHoles lower = Polygon(2, 2, 10, 2, 10, 10, 2, 10);
            SymPathWithHoles upper = Polygon(6, 6, 6, 14, 14, 14, 14, 6);
            SymPathWithHoles union = Polygon(2, 2, 10, 2, 10, 6, 14, 6, 14, 14, 6, 14, 6, 10, 2, 10);

            using (SKBitmap expected = RenderPdf(CreateMap(dots, new int[] { 0 }, union), name + "_union"))
            using (SKBitmap drawn = RenderPdf(CreateMap(dots, new int[] { 0, 0 }, lower, upper), name + "_one_symbol"))
            using (SKBitmap separately = RenderPdf(CreateMap(dots, new int[] { 0, 1 }, lower, upper), name + "_two_symbols")) {
                Assert.That(CountDifferingPixels(expected, drawn), Is.EqualTo(0), "the areas of one symbol export as their union");
                Assert.That(CountDifferingPixels(expected, separately), Is.GreaterThan(100), "areas of two symbols draw each pattern by itself");
            }
        }

        [Test]
        public void HatchingOfOverlappingAreasIsExportedOnce()
        {
            CheckExportedAsUnion(false, "hatching");
        }

        [Test]
        public void PatternOfOverlappingAreasIsExportedOnce()
        {
            CheckExportedAsUnion(true, "dots");
        }
    }
}
