using System;
using System.Drawing;
using System.Text.RegularExpressions;
using NUnit.Framework;
using PurplePen.Graphics2D;
using PurplePen.MapModel;

namespace Map_Svg.Tests
{
    // Checks how the map writes its area patterns into the SVG target, as the export draws them (S-7, S-12, S-13): a
    // regular pattern in clip mode as a pattern fill, however fine its tile against the steps the target writes, defined
    // once for its color and turned with each area about the area's pattern origin by a transform on the area's fill;
    // a hatching as a pattern fill of one of its lines, turned likewise; an irregular pattern, or one in another fill
    // mode, element by element; and, when area patterns are drawn as separate shapes, every pattern element by
    // element and every hatching line by line.
    [TestFixture]
    public class AreaPatternTests
    {
        // The smallest distance worth drawing, in millimetres of map, which the export draws with: the step the target
        // writes coordinates in, at which the map code finds a pattern bitmap of any tile wider than 0.08 mm too
        // pixelated.
        private const float MinResolution = 0.001F;

        // Draw a map into the SVG target as the export draws it, in one part shown on one page, and return the document.
        // map: the map.
        // separateShapes: whether area patterns are drawn as separate shapes, element by element, as the export draws
        // them when asked to, rather than with pattern brushes, as it draws them for the target, whose pattern brushes
        // are vectors.
        private static string WriteMap(Map map, bool separateShapes = false)
        {
            using (Svg_GraphicsTarget target = new Svg_GraphicsTarget(DefaultColorConverter.Instance, true))
            using (Matrix mapToPage = new Matrix()) {
                Assert.IsTrue(GraphicsUtil.PatternBrushesAreVectors(target));
                RenderOptions options = new RenderOptions();
                options.usePatternBitmaps = !separateShapes;
                options.minResolution = MinResolution;

                object part = new object();
                target.BeginPart(part);
                target.BeginMap(map, "map");
                using (map.Read())
                    map.Draw(target, PatternedAreaMap.AreaOfEveryPlacement, options, null);
                target.EndMap();
                target.EndPart();

                mapToPage.Translate(20, 20);
                mapToPage.Scale(1, -1);
                target.AddPage(new SizeF(40, 40), PatternedAreaMap.AreaOfEveryPlacement, mapToPage, new object[] { part }, null);
                return target.FinishDocument();
            }
        }

        // The number of times a text occurs in a document.
        // document: the document.
        // text: the text.
        private static int Occurrences(string document, string text)
        {
            return document.Split(new[] { text }, StringSplitOptions.None).Length - 1;
        }

        [Test]
        public void ARegularPatternInClipModeIsAPatternFillHoweverFineItsTile()
        {
            string document;
            using (Map map = PatternedAreaMap.Create(false, AreaSymDef.PatternFillMode.Clip, PatternedAreaMap.SquaresInEveryPlacement[0]))
                document = WriteMap(map);

            Assert.AreEqual(1, Occurrences(document, "<pattern "));
            StringAssert.Contains("<pattern id=\"b\" patternUnits=\"userSpaceOnUse\" x=\"-1000\" y=\"-1600\" width=\"2000\" height=\"3200\" " +
                                  "viewBox=\"-1000 -1600 2000 3200\" patternTransform=\"rotate(30)\">", document);
            // The area fills its outline, under the even-odd rule of an area's outline and holes, and the fill sits on
            // the map's group, as every element of the map shares it.
            StringAssert.Contains("<g style=\"fill:url(#b);fill-rule:evenodd\" data-m=\"1\">\n<g data-c=\"0\">\n<g data-s=\"1\">\n" +
                                  "<path data-o=\"1\" d=\"M-18000 2000h16000v16000h-16000V2000z\"/>\n", document);
            // The dots of the pattern lie in its tile alone, and no clip path but the page's is written.
            Assert.AreEqual(Occurrences(document, "<circle"), Occurrences(Regex.Match(document, "<pattern .*</pattern>", RegexOptions.Singleline).Value, "<circle"));
            Assert.AreEqual(1, Occurrences(document, "<clipPath"));
        }

        [Test]
        public void AnIrregularPatternAndOneOutsideClipModeAreDrawnElementByElement()
        {
            foreach (bool irregular in new[] { true, false }) {
                AreaSymDef.PatternFillMode fillMode = irregular ? AreaSymDef.PatternFillMode.Clip : AreaSymDef.PatternFillMode.CenterInside;
                string document;
                using (Map map = PatternedAreaMap.Create(irregular, fillMode, PatternedAreaMap.SquaresInEveryPlacement[0]))
                    document = WriteMap(map);

                Assert.AreEqual(0, Occurrences(document, "<pattern "), irregular ? "irregular" : "center inside");
                Assert.Greater(Occurrences(document, "<circle"), 50, irregular ? "irregular" : "center inside");
            }
        }

        [Test]
        public void EachAreaTurnsTheOnePatternOfItsColorAboutItsPatternOriginByATransformOnItsFill()
        {
            string document;
            using (Map map = PatternedAreaMap.Create(false, AreaSymDef.PatternFillMode.Clip, PatternedAreaMap.SquaresInEveryPlacement))
                document = WriteMap(map);

            // One definition serves the four areas.
            Assert.AreEqual(1, Occurrences(document, "<pattern "));
            Assert.AreEqual(4, Occurrences(document, "url(#b)"));
            // An area unturned with the map's origin as its pattern's fills its own outline. An area with a moved
            // origin, or turned, fills a rectangle around its outline in the frame of its pattern, which moves to the
            // origin and turns by the angle, clipped to its outline.
            StringAssert.Contains("<path style=\"fill:url(#b);fill-rule:evenodd\" data-o=\"1\" d=\"M-18000 2000h16000v16000h-16000V2000z\"/>\n" +
                                  "<g style=\"fill:url(#b)\" clip-path=\"url(#c)\">\n<g transform=\"translate(10700 10450)\">\n" +
                                  "<path data-o=\"2\" d=\"M-8700-8450H7300V7550H-8700z\"/>\n</g>\n</g>\n" +
                                  "<g style=\"fill:url(#b)\" clip-path=\"url(#d)\">\n<g transform=\"rotate(25)\">\n" +
                                  "<path data-o=\"3\" d=\"M-23921-15468h21263V5795h-21263z\"/>\n</g>\n</g>\n" +
                                  "<g style=\"fill:url(#b)\" clip-path=\"url(#e)\">\n<g transform=\"translate(10700-9550)rotate(25)\">\n" +
                                  "<path data-o=\"4\" d=\"M-11456-10743H9807v21262h-21263z\"/>\n</g>\n</g>\n", document);
        }

        [Test]
        public void AHatchingIsAPatternFillOfOneOfItsLinesInATileAsHighAsItsSpacing()
        {
            string document;
            using (Map map = PatternedAreaMap.CreateHatched(PatternedAreaMap.SquaresInEveryPlacement[0]))
                document = WriteMap(map);

            // Each hatching's tile is as high and as wide as its spacing, turned by its angle, and holds the line
            // through its offset nearest its middle: for the green one, whose offset of 1 mm is a spacing and 0.4 mm,
            // the line 0.2 mm before the middle; for the blue one, the line 0.31 mm after the middle, which reaches past
            // the tile's edge 0.04 mm beyond it, and the line a spacing before it, which reaches into the tile across
            // the other edge. Each line runs on past both sides of the tile.
            Assert.AreEqual(2, Occurrences(document, "<pattern "));
            StringAssert.Contains("<pattern id=\"b\" patternUnits=\"userSpaceOnUse\" x=\"-350\" y=\"-350\" width=\"700\" height=\"700\" " +
                                  "viewBox=\"-350 -350 700 700\" patternTransform=\"rotate(100)\">\n" +
                                  "<path style=\"fill:none;stroke:#00e6ff;stroke:#00e6ff icc-color(cmyk,1,.1,0,0);stroke-width:350;stroke-miterlimit:5\" " +
                                  "d=\"M-700-390H700\"/>\n" +
                                  "<path style=\"fill:none;stroke:#00e6ff;stroke:#00e6ff icc-color(cmyk,1,.1,0,0);stroke-width:350;stroke-miterlimit:5\" " +
                                  "d=\"M-700 310H700\"/>\n</pattern>\n", document);
            StringAssert.Contains("<pattern id=\"c\" patternUnits=\"userSpaceOnUse\" x=\"-300\" y=\"-300\" width=\"600\" height=\"600\" " +
                                  "viewBox=\"-300 -300 600 600\" patternTransform=\"rotate(30)\">\n" +
                                  "<path style=\"fill:none;stroke:#3dff17;stroke:#3dff17 icc-color(cmyk,.76,0,.91,0);stroke-width:150;stroke-miterlimit:5\" " +
                                  "d=\"M-600-200H600\"/>\n</pattern>\n", document);
            // The area fills its outline with each, and no line is drawn but in the tiles.
            StringAssert.Contains("<g style=\"fill:url(#b);fill-rule:evenodd\" data-c=\"0\">\n<g data-s=\"1\">\n" +
                                  "<path data-o=\"1\" d=\"M-18000 2000h16000v16000h-16000V2000z\"/>\n", document);
            StringAssert.Contains("<g style=\"fill:url(#c);fill-rule:evenodd\" data-c=\"1\">\n<g data-s=\"1\">\n" +
                                  "<path data-o=\"1\" d=\"M-18000 2000h16000v16000h-16000V2000z\"/>\n", document);
            Assert.AreEqual(3, Occurrences(document, "stroke-width"));
            Assert.AreEqual(1, Occurrences(document, "<clipPath"));
        }

        [Test]
        public void EachAreaTurnsAHatchingThatTurnsAboutItsPatternOriginAndLeavesOneThatNeverTurns()
        {
            string document;
            using (Map map = PatternedAreaMap.CreateHatched(PatternedAreaMap.SquaresInEveryPlacement))
                document = WriteMap(map);

            // The blue hatching never turns, so each area fills its own outline with it, as the map's origin lays it out.
            StringAssert.Contains("<g style=\"fill:url(#b);fill-rule:evenodd\" data-c=\"0\">\n<g data-s=\"1\">\n" +
                                  "<path data-o=\"1\" d=\"M-18000 2000h16000v16000h-16000V2000z\"/>\n" +
                                  "<path data-o=\"2\" d=\"M2000 2000h16000v16000H2000V2000z\"/>\n" +
                                  "<path data-o=\"3\" d=\"M-18000-18000h16000v16000h-16000v-16000z\"/>\n" +
                                  "<path data-o=\"4\" d=\"M2000-18000h16000v16000H2000v-16000z\"/>\n</g>\n</g>\n", document);
            // The green one turns as a pattern does: an area with a moved origin, or turned, fills a rectangle around its
            // outline in the hatching's frame, clipped to its outline.
            StringAssert.Contains("<path style=\"fill:url(#c);fill-rule:evenodd\" data-o=\"1\" d=\"M-18000 2000h16000v16000h-16000V2000z\"/>\n" +
                                  "<g style=\"fill:url(#c)\" clip-path=\"url(#d)\">\n<g transform=\"translate(10700 10450)\">\n" +
                                  "<path data-o=\"2\" d=\"M-8700-8450H7300V7550H-8700z\"/>\n</g>\n</g>\n" +
                                  "<g style=\"fill:url(#c)\" clip-path=\"url(#e)\">\n<g transform=\"rotate(25)\">\n" +
                                  "<path data-o=\"3\" d=\"M-23921-15468h21263V5795h-21263z\"/>\n</g>\n</g>\n" +
                                  "<g style=\"fill:url(#c)\" clip-path=\"url(#f)\">\n<g transform=\"translate(10700-9550)rotate(25)\">\n" +
                                  "<path data-o=\"4\" d=\"M-11456-10743H9807v21262h-21263z\"/>\n</g>\n</g>\n", document);
            Assert.AreEqual(2, Occurrences(document, "<pattern "));
        }

        [Test]
        public void AsSeparateShapesEveryPatternIsDrawnElementByElementAndEveryHatchingLineByLine()
        {
            string patterned, hatched;
            using (Map map = PatternedAreaMap.Create(false, AreaSymDef.PatternFillMode.Clip, PatternedAreaMap.SquaresInEveryPlacement[0]))
                patterned = WriteMap(map, true);
            using (Map map = PatternedAreaMap.CreateHatched(PatternedAreaMap.SquaresInEveryPlacement[0]))
                hatched = WriteMap(map, true);

            Assert.AreEqual(0, Occurrences(patterned, "<pattern "));
            Assert.Greater(Occurrences(patterned, "<circle"), 50);
            Assert.AreEqual(0, Occurrences(hatched, "<pattern "));
            // The blue lines are drawn one by one in the hatching's frame, among them the two that the tile of its
            // pattern fill holds, and the others a whole number of spacings from them.
            StringAssert.Contains("<g transform=\"rotate(100)\">\n<path data-o=\"1\" d=\"M2317-1090h18535\"/>\n" +
                                  "<path data-o=\"1\" d=\"M2317-390h18535\"/>\n<path data-o=\"1\" d=\"M2317 310h18535\"/>\n" +
                                  "<path data-o=\"1\" d=\"M2317 1010h18535\"/>\n", hatched);
            Assert.AreEqual(27, Occurrences(Regex.Match(hatched, "<g transform=\"rotate\\(100\\)\">.*?</g>", RegexOptions.Singleline).Value, "<path "));
        }
    }
}
