using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using NUnit.Framework;
using PurplePen.Graphics2D;
using PurplePen.MapModel;
using SkiaSharp;
using TestingUtils;

namespace Map_Skia.Tests
{
    // The coverage of the palette separations that pass 1 makes (Map.PaletteSeparations): for each palette color of a
    // test map, the coverage of the palette cutters of its palette separation covers the pixels that the drawing of the
    // palette color covers. The coverage of a palette cutter with the full coverage is the union of the clipping objects
    // of its palette color coverages. The coverage of a palette cutter of a hatching or a clipped area pattern is the
    // expansion of the coverage prototype of each of its palette color coverages (SymbolCoverage.ExpandPrototype) inside
    // the clipping objects of the palette color coverage. Both drawings are made in the same run, without antialiasing,
    // on a transparent bitmap whose alpha is the coverage. So a white color counts as the others do, and the comparison
    // needs no baseline. No difference is allowed. The oracle is the old drawing of the palette colors (Map.DrawColor),
    // which the internal option drawColorsDirectly selects also on the Skia graphics target, whose encoding pass 2 would
    // use otherwise.
    // The palette separations are those of the whole map, and the bitmap of the area of the test map limits their
    // drawing. The drawing of the palette color, which is the oracle, draws the area at the resolution 0, as pass 1 draws
    // the symbols, so that both keep the dashes of a dashed line whatever the size of a pixel. It draws the area patterns
    // glyph by glyph, without the bitmaps of the pattern brushes (RenderOptions.usePatternBitmaps).
    // The drawing of the bottom palette color draws the image layer too, so the comparison leaves out the bottom palette
    // color of a map with image objects. It compares all the other palette colors.
    [TestFixture, Parallelizable(ParallelScope.Children)]
    public class PaletteSeparationCoverageTests
    {
        static PaletteSeparationCoverageTests()
        {
            // The fonts of the rendering tests, which a drawing of a text then uses. They are added before both drawings.
            RenderingTests.RegisterRobotoFonts();
        }

        // The maps without hatchings, area patterns and image objects, and the maps with hatchings or area patterns.
        [TestCase("isomlines.txt")]
        [TestCase("isompoints.txt")]
        [TestCase("dashline.txt")]
        [TestCase("fences.txt")]
        [TestCase("doublelines9.txt")]
        [TestCase("pointyends9.txt")]
        [TestCase("glyphs9.txt")]
        [TestCase("cornerglyphs9.txt")]
        [TestCase("decreasesymbols.txt")]
        [TestCase("cutcircles.txt")]
        [TestCase("simpletext.txt")]
        [TestCase("framingline-test.txt")]
        [TestCase("underlinetext.txt")]
        [TestCase("textpoint10.txt")]
        [TestCase("linetext_9.txt")]
        [TestCase("rectanglesymbols.txt")]
        [TestCase("graphicobjects9.txt")]
        [TestCase("LordHill.txt")]
        [TestCase("lksamm1.txt")]
        [TestCase("marymoor.txt")]
        [TestCase("rotarea-test.txt")]
        [TestCase("rotarea-test9.txt")]
        [TestCase("offsetpattern_nopatbm.txt")]
        [TestCase("offsetpatternrot_nopatbm.txt")]
        [TestCase("offsetpatternrot2_nopatbm.txt")]
        [TestCase("wholestructure.txt")]
        [TestCase("wholestructure2.txt")]
        [TestCase("wholestructure3.txt")]
        [TestCase("wholestructure4.txt")]
        [TestCase("irregular1.txt")]
        [TestCase("irregular2.txt")]
        [TestCase("irregular3.txt")]
        [TestCase("irregular4.txt")]
        [TestCase("holes.txt")]
        [TestCase("holes9.txt")]
        [TestCase("isomarea_nopatbm.txt")]
        [TestCase("isomareabug_nopatbm.txt")]
        [TestCase("borderedarea9.txt")]
        [TestCase("glyphholes.txt")]
        [TestCase("glyphholes9.txt")]
        [TestCase("aiimport.txt")]
        [TestCase("FtCasey.txt")]
        [TestCase("PenistoneHill.txt")]
        [TestCase("punchboxbug.txt")]
        [TestCase("missingcolor.txt")]
        [TestCase("clouds.txt")]
        public void ClippingObjectsCoverWhatTheDrawingCovers(string testFileName)
        {
            int comparedColors, allColors;
            bool hasImageObjects;
            CompareCoverage(TestMap.Read(testFileName), out comparedColors, out allColors, out hasImageObjects);
            Assert.AreEqual(hasImageObjects ? allColors - 1 : allColors, comparedColors, "The palette colors that the comparison compares.");
        }

        // Maps of dashed lines, drawn with pixels that are larger than the gaps of the dashes. A drawing at the size of
        // these pixels draws the lines solid, and the drawing at the resolution 0 and the clipping objects keep the
        // dashes.
        // pixelSize: the size of a pixel, in the coordinates of the map.
        [TestCase("dashline.txt", 0.3F)]       // gaps of 0.25 mm
        [TestCase("isomlines.txt", 0.4F)]      // gaps and secondary gaps of 0.38 mm
        [TestCase("doublelines9.txt", 0.8F)]   // gaps of 0.75 mm in the dashes of double lines
        public void ClippingObjectsCoverTheDashesOfGapsShorterThanAPixel(string testFileName, float pixelSize)
        {
            TestMap testMap = TestMap.Read(testFileName).WithPixelSize(pixelSize);
            int pixelsThatTheResolutionChanges = CountPixelsThatTheResolutionChanges(testMap);
            TestContext.WriteLine("{0} x {1} pixels of {2} mm, {3} of which the resolution 0 changes", testMap.Size.Width, testMap.Size.Height, testMap.PixelSize, pixelsThatTheResolutionChanges);
            Assert.Greater(pixelsThatTheResolutionChanges, 0, "The drawing at the size of a pixel draws each dashed line of the map as the drawing at the resolution 0 does.");

            int comparedColors, allColors;
            bool hasImageObjects;
            CompareCoverage(testMap, out comparedColors, out allColors, out hasImageObjects);
            Assert.AreEqual(allColors, comparedColors, "The test map has image objects.");
        }

        // Count the pixels in which the drawing of a test map at the size of its pixel differs from the drawing at the
        // resolution 0: the pixels of the gaps that the drawing at the size of the pixel leaves out.
        // testMap: the test map.
        static int CountPixelsThatTheResolutionChanges(TestMap testMap)
        {
            Map map = testMap.Map;
            using (map.Read()) {
                bool[] atPixelSize = testMap.DrawCoverage(target => map.Draw(target, testMap.Area, CreateMapOptions(testMap.PixelSize), null));
                bool[] atResolution0 = testMap.DrawCoverage(target => map.Draw(target, testMap.Area, CreateMapOptions(0), null));
                return Enumerable.Range(0, atResolution0.Length).Count(i => atPixelSize[i] != atResolution0[i]);
            }
        }

        // Compare the coverage of the palette separation of each palette color of a test map with the drawing of the
        // palette color. Check also that each palette color coverage of a symbol is in one palette cutter, whose coverage
        // prototype and normal form of M it has.
        // testMap: the test map.
        // comparedColors: the number of palette colors that the comparison compares.
        // allColors: the number of palette colors of the map.
        // hasImageObjects: whether the map has visible image objects, so that the comparison leaves out the bottom palette
        //   color.
        static void CompareCoverage(TestMap testMap, out int comparedColors, out int allColors, out bool hasImageObjects)
        {
            Map map = testMap.Map;
            List<string> differences = new List<string>();
            bool hasAnyCoverage = false;
            comparedColors = 0;

            using (map.Read()) {
                List<SymColor> colors = map.AllColors.ToList();
                allColors = colors.Count;
                Assert.AreEqual(colors.Count, colors.Select(color => color.OcadId).Distinct().Count(), "The drawing of one palette color needs unique color numbers.");

                hasImageObjects = HasImageObjects(map);
                IReadOnlyList<PaletteSeparation> separations = map.PaletteSeparations;
                CollectionAssert.AreEqual(colors, separations.Select(separation => separation.Color), "The palette separations are in palette order.");

                for (int index = 0; index < colors.Count; ++index) {
                    SymColor color = colors[index];
                    PaletteSeparation separation = separations[index];
                    CollectionAssert.AreEquivalent(separation.SymbolCoverages, separation.Cutters.SelectMany(cutter => cutter.SymbolCoverages), "Each palette color coverage of {0} is in one palette cutter.", color.Name);
                    foreach (PaletteCutter cutter in separation.Cutters) {
                        foreach (SymbolCoverage coverage in cutter.SymbolCoverages) {
                            Assert.AreSame(cutter.Prototype, coverage.Prototype);
                            CollectionAssert.AreEqual(cutter.Transformation.Elements, coverage.Prototype.GetNormalForm(coverage.Transformation).Elements, "The normal form of M of {0}", coverage);
                        }
                    }
                    if (index == 0 && hasImageObjects)
                        continue;

                    int clippingObjectCount = 0, expandedCount = 0;
                    bool[] drawing = testMap.DrawCoverage(target => map.Draw(target, testMap.Area, CreateOneColorOptions(colors, index), null));
                    bool[] union = testMap.DrawCoverage(target => DrawCoverage(target, separation, ref clippingObjectCount, ref expandedCount));

                    ++comparedColors;
                    TestContext.WriteLine("{0} (id {1}): {2} pixels, {3} palette color coverages, {4} palette cutters, {5} clipping objects, {6} clipping objects of the expansions",
                                          color.Name, color.OcadId, drawing.Count(covered => covered), separation.SymbolCoverages.Count, separation.Cutters.Count, clippingObjectCount, expandedCount);
                    hasAnyCoverage |= drawing.Contains(true);
                    int differing = Enumerable.Range(0, drawing.Length).Count(i => drawing[i] != union[i]);
                    if (differing > 0) {
                        string image = testMap.WriteDifference(color, drawing, union);
                        differences.Add(string.Format("{0} (id {1}): {2} pixels differ, see {3}", color.Name, color.OcadId, differing, image));
                    }
                }
            }

            Assert.IsTrue(hasAnyCoverage, "The palette colors that the comparison compares draw nothing.");
            Assert.IsEmpty(differences, "The coverage of the clipping objects differs from the drawing:\n" + string.Join("\n", differences));
        }

        // Whether a map has visible image objects, which the drawing of the bottom palette color draws too.
        // map: the map.
        static bool HasImageObjects(Map map)
        {
            foreach (Symbol symbol in map.AllSymbols) {
                if (map.IsSymdefVisible(symbol.Definition) && symbol.IsVisible) {
                    ImageSymDef imageSymdef = symbol.Definition as ImageSymDef;
                    if (imageSymdef != null && imageSymdef.Layer == SymLayer.Image)
                        return true;
                }
            }

            return false;
        }

        // Draw the coverage of a palette separation with an opaque brush: the clipping objects of the palette color
        // coverages of each palette cutter with the full coverage, and inside the clipping objects of each palette color
        // coverage of a hatching or a clipped area pattern, the expansion of its coverage prototype. The clipping object of
        // a hatching or a clipped area pattern is the area, a filled path at the identity placement, which the drawing
        // clips with. It is the test's own drawing of the clipping objects, and not pass 2.
        // g: the target.
        // separation: the palette separation.
        // clippingObjectCount: the number of the clipping objects that were drawn.
        // expandedCount: the number of the clipping objects of the expansions that were drawn.
        static void DrawCoverage(IGraphicsTarget g, PaletteSeparation separation, ref int clippingObjectCount, ref int expandedCount)
        {
            foreach (PaletteCutter cutter in separation.Cutters) {
                foreach (SymbolCoverage coverage in cutter.SymbolCoverages) {
                    IReadOnlyList<ClippingObject> clippingObjects = coverage.ClippingObjects;
                    clippingObjectCount += clippingObjects.Count;
                    if (cutter.Prototype == CoveragePrototype.FullCoverage) {
                        DrawClippingObjects(g, clippingObjects);
                    }
                    else {
                        FilledShape area = (FilledShape)clippingObjects.Single();
                        Assert.AreEqual(0, area.Placement.Steps.Count, "the steps of the area of {0}", coverage);
                        IReadOnlyList<ClippingObject> expansion = coverage.ExpandPrototype();
                        expandedCount += expansion.Count;
                        g.PushClip(((PathShape)area.Shape).Parts.ToList(), area.FillRule);
                        DrawClippingObjects(g, expansion);
                        g.PopClip();
                    }
                }
            }
        }

        // The options that draw one palette color of a map, as the current drawing does: from the palette color below
        // it, which is not drawn, to the palette color itself, at the resolution 0 of pass 1. The drawing of the bottom
        // palette color starts at the bottom, with the image layer.
        // colors: the palette colors of the map, the bottom one first.
        // index: the index of the palette color.
        static RenderOptions CreateOneColorOptions(List<SymColor> colors, int index)
        {
            RenderOptions renderOpts = CreateMapOptions(0);
            renderOpts.colorBeginDrawExclusive = (index == 0) ? (int?)null : colors[index - 1].OcadId;
            renderOpts.colorEndDrawInclusive = colors[index].OcadId;
            return renderOpts;
        }

        // The options that draw the palette colors of a map without the templates and without the effect of
        // overprinting, with the old drawing code of the palette colors, the oracle of the comparison.
        // minResolution: the size of the smallest feature that the drawing draws.
        static RenderOptions CreateMapOptions(float minResolution)
        {
            RenderOptions renderOpts = new RenderOptions();
            renderOpts.minResolution = minResolution;
            renderOpts.renderTemplates = RenderTemplateOption.MapOnly;
            renderOpts.overprintMode = OverprintMode.None;
            renderOpts.drawColorsDirectly = true;
            return renderOpts;
        }

        // Draw clipping objects with an opaque brush: each shape filled or stroked, at its placement, within the steps of
        // the placement. The runs of dashes, the offset paths and the repeated placements are drawn expanded
        // (SymbolCoverageComparison.Expand). A text run is drawn with the Y axis mirrored, at its top left corner with the
        // opposite y coordinate, so the target gets the same transformation and the same point as in the drawing of the
        // text. It is the test's own drawing of the clipping objects, and not pass 2.
        // g: the target.
        // clippingObjects: the clipping objects.
        static void DrawClippingObjects(IGraphicsTarget g, IEnumerable<ClippingObject> clippingObjects)
        {
            object brush = new object();
            g.CreateSolidBrush(brush, CmykColor.FromCmyk(0, 0, 0, 1));
            Dictionary<Stroke, object> pens = new Dictionary<Stroke, object>();
            Dictionary<(string, float, TextEffects), object> fonts = new Dictionary<(string, float, TextEffects), object>();

            foreach ((ClippingObject clippingObject, bool isExpanded) in SymbolCoverageComparison.Expand(clippingObjects.ToList())) {
                IReadOnlyList<Matrix> steps = clippingObject.Placement.Steps;
                foreach (Matrix step in steps)
                    g.PushTransform(step);

                FilledShape filledShape = clippingObject as FilledShape;
                if (filledShape != null) {
                    FillShape(g, filledShape.Shape, filledShape.FillRule, brush, fonts);
                }
                else {
                    StrokedShape strokedShape = (StrokedShape)clippingObject;
                    object pen;
                    if (!pens.TryGetValue(strokedShape.Stroke, out pen)) {
                        pen = new object();
                        Stroke stroke = strokedShape.Stroke;
                        g.CreatePen(pen, brush, stroke.Width, stroke.Caps, stroke.Join, stroke.MiterLimit);
                        pens.Add(stroke, pen);
                    }
                    StrokeShape(g, strokedShape.Shape, pen, fonts);
                }

                for (int i = 0; i < steps.Count; ++i)
                    g.PopTransform();
            }
        }

        // Fill a shape.
        static void FillShape(IGraphicsTarget g, Shape shape, AreaFillMode fillRule, object brush, Dictionary<(string, float, TextEffects), object> fonts)
        {
            if (shape is PathShape path)
                g.FillPath(brush, path.Parts.ToList(), fillRule);
            else if (shape is EllipseShape ellipse)
                g.FillEllipse(brush, ellipse.Center, ellipse.RadiusX, ellipse.RadiusY);
            else if (shape is TextShape text)
                DrawTextRun(g, text, upperLeft => g.DrawText(text.Text, GetFont(g, text, fonts), brush, upperLeft));
            else
                Assert.Fail("A filled {0}", shape);
        }

        // Stroke a shape.
        static void StrokeShape(IGraphicsTarget g, Shape shape, object pen, Dictionary<(string, float, TextEffects), object> fonts)
        {
            if (shape is PathShape path)
                g.DrawPath(pen, path.Parts.ToList());
            else if (shape is EllipseShape ellipse)
                g.DrawEllipse(pen, ellipse.Center, ellipse.RadiusX, ellipse.RadiusY);
            else if (shape is ArcShape arc)
                g.DrawArc(pen, arc.Center, arc.Radius, arc.StartAngle, arc.SweepAngle);
            else if (shape is TextShape text)
                DrawTextRun(g, text, upperLeft => g.DrawTextOutline(text.Text, GetFont(g, text, fonts), pen, upperLeft));
            else
                Assert.Fail("A stroked {0}", shape);
        }

        // Draw a text run as the drawing lays out a text, with the Y axis mirrored: the text run has the mirror as a part
        // of it (TextShape).
        // g: the target.
        // text: the text run.
        // draw: the call that draws the text at its top left corner in the mirrored coordinates.
        static void DrawTextRun(IGraphicsTarget g, TextShape text, Action<PointF> draw)
        {
            Matrix mirror = new Matrix();
            mirror.Scale(1, -1);
            g.PushTransform(mirror);
            draw(new PointF(text.UpperLeft.X, -text.UpperLeft.Y));
            g.PopTransform();
        }

        // The key of the font of a text run, which is created once for each family, size and effects.
        static object GetFont(IGraphicsTarget g, TextShape text, Dictionary<(string, float, TextEffects), object> fonts)
        {
            object font;
            if (!fonts.TryGetValue((text.FontFamily, text.EmHeight, text.Effects), out font)) {
                font = new object();
                g.CreateFont(font, text.FontFamily, text.EmHeight, text.Effects);
                fonts.Add((text.FontFamily, text.EmHeight, text.Effects), font);
            }
            return font;
        }

        // A test map of the Skia rendering tests, with the area and the size of its rendering.
        sealed class TestMap
        {
            public readonly string Name;
            public readonly Map Map;
            public readonly RectangleF Area;
            public readonly Size Size;

            private TestMap(string name, Map map, RectangleF area, Size size)
            {
                Name = name;
                Map = map;
                Area = area;
                Size = size;
            }

            // The size of a pixel of the rendering, in the coordinates of the map. The rendering scales the width of the
            // area to the width of the bitmap, and the height by the same factor.
            public float PixelSize
            {
                get { return Area.Width / Size.Width; }
            }

            // The same map and area, rendered with pixels of about a given size: the bitmap has the number of whole
            // pixels nearest to the width and the height of the area in that size.
            // pixelSize: the size of a pixel, in the coordinates of the map.
            public TestMap WithPixelSize(float pixelSize)
            {
                Size size = new Size((int)Math.Round(Area.Width / pixelSize), (int)Math.Round(Area.Height / pixelSize));
                return new TestMap(string.Format(CultureInfo.InvariantCulture, "{0}_pixel{1}", Name, pixelSize), Map, Area, size);
            }

            // Read a test map from its description in skia_render: the map file, the baseline, which is not used, the
            // area as left, bottom, right and top, and the size as width and height.
            // fileName: the name of the description.
            public static TestMap Read(string fileName)
            {
                string description = TestUtil.GetTestFile(Path.Combine("skia_render", fileName));
                string[] lines = File.ReadAllLines(description);
                float[] area = lines[2].Split(',').Select(value => float.Parse(value, CultureInfo.InvariantCulture)).ToArray();
                int[] size = lines[3].Split(',').Select(value => int.Parse(value, CultureInfo.InvariantCulture)).ToArray();

                string directory = Path.GetDirectoryName(description);
                Map map = new Map(new Skia_TextMetrics(), new Skia_FileLoader(directory));
                InputOutput.ReadFile(Path.Combine(directory, lines[0].Trim()), map);

                return new TestMap(Path.GetFileNameWithoutExtension(fileName), map, new RectangleF(area[0], area[3], area[2] - area[0], area[1] - area[3]), new Size(size[0], size[1]));
            }

            // The coverage of a drawing of the area: whether it covers each pixel, row by row. The drawing is made on a
            // transparent bitmap, without antialiasing.
            // draw: the drawing.
            public bool[] DrawCoverage(Action<IGraphicsTarget> draw)
            {
                using (Skia_BitmapGraphicsTarget target = new Skia_BitmapGraphicsTarget(Size.Width, Size.Height, true, CmykColor.FromCmyka(0, 0, 0, 0, 0), Area, true)) {
                    target.PushAntiAliasing(false);
                    draw(target);
                    target.PopAntiAliasing();
                    using (Skia_Bitmap bitmap = (Skia_Bitmap)target.FinishBitmap())
                        return bitmap.Bitmap.Pixels.Select(pixel => pixel.Alpha != 0).ToArray();
                }
            }

            // Write an image of where two coverages of a palette color differ, and return its path: red where the
            // drawing alone covers a pixel, blue where the clipping objects alone cover it, and grey where both cover it.
            // color: the palette color.
            // drawing: the coverage of the drawing.
            // clippingObjects: the coverage of the clipping objects.
            public string WriteDifference(SymColor color, bool[] drawing, bool[] clippingObjects)
            {
                string path = Path.Combine(TestContext.CurrentContext.WorkDirectory, "palette-separation-coverage", string.Format("{0}_{1}.png", Name, color.OcadId));
                Directory.CreateDirectory(Path.GetDirectoryName(path));

                using (SKBitmap image = new SKBitmap(Size.Width, Size.Height)) {
                    for (int i = 0; i < drawing.Length; ++i) {
                        SKColor pixel = drawing[i] ? (clippingObjects[i] ? new SKColor(160, 160, 160) : SKColors.Red) : (clippingObjects[i] ? SKColors.Blue : SKColors.White);
                        image.SetPixel(i % Size.Width, i / Size.Width, pixel);
                    }

                    using (FileStream stream = File.Create(path))
                        image.Encode(stream, SKEncodedImageFormat.Png, 100);
                }

                return path;
            }
        }
    }
}
