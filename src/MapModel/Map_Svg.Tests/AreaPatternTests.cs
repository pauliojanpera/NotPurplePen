using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Xml.Linq;
using NUnit.Framework;
using PurplePen.Graphics2D;
using PurplePen.MapModel;

namespace Map_Svg.Tests
{
    // Checks how the SVG encoding writes the pattern fills of the hatchings and the clipped area patterns of a map, as the
    // export draws them. The coverage prototype of a hatching or a regular area pattern is a definition: a pattern that
    // is bound to the origin of its own coordinates and painted in its palette color. The pattern fill of each palette
    // cutter is a configuration, which refers to the definition with the transformation M of the cutter
    // (patternTransform). The pattern fill of an area fills its outline, and that of an area cluster of several areas
    // fills a rectangle around them once, within a clip to the union of their areas, a path for each area under its own
    // rule. An irregular area pattern is the expansion of its coverage prototype within the clip of its area, and the
    // elements of an area pattern in another fill mode are a part of the full coverage of the area symbol. With the
    // setting for separate shapes, each area writes the expansion of its own coverage prototypes within the clip of its
    // area.
    [TestFixture]
    public class AreaPatternTests
    {
        private static readonly XNamespace Svg = "http://www.w3.org/2000/svg";
        private static readonly XNamespace XLink = "http://www.w3.org/1999/xlink";

        // The smallest distance worth drawing, in millimetres of map, which the export draws with: the step the target
        // writes coordinates in.
        private const float MinResolution = 0.001F;

        // The outline of the square of SquaresInEveryPlacement[0], in steps, as the document writes it.
        private const string OutlineOfFirstSquare = "M-18000 2000h16000v16000h-16000V2000z";

        // Two squares that overlap, unturned, laid out from the map's origin: one area cluster.
        private static readonly PatternedAreaMap.Square[] OverlappingSquares = {
            new PatternedAreaMap.Square(new PointF(-4, -4), 0, new PointF(0, 0)),
            new PatternedAreaMap.Square(new PointF(4, 4), 0, new PointF(0, 0)),
        };

        // Draw a map into the SVG target as the export draws it, in one part shown on one page, and return the document,
        // which the loop's results keep under its name.
        // name: the name of the document in the loop's results.
        // map: the map.
        // separateShapes: whether area patterns and dashed lines are drawn as separate shapes, as the export draws them
        //   when asked to, rather than as pattern fills.
        private static XDocument WriteMap(string name, Map map, bool separateShapes = false)
        {
            using (Svg_GraphicsTarget target = new Svg_GraphicsTarget(DefaultColorConverter.Instance, true))
            using (Matrix mapToPage = new Matrix()) {
                Assert.IsTrue(GraphicsUtil.PatternBrushesAreVectors(target));
                RenderOptions options = new RenderOptions();
                options.usePatternBitmaps = !separateShapes;
                options.patternsWithEachArea = separateShapes;
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
                string document = target.FinishDocument();
                LoopResults.SaveSvg("area-patterns-" + name, document);
                return XDocument.Parse(document);
            }
        }

        // The text forms of the symbol keys of a map's symbols, in the order the map holds them.
        // map: the map.
        private static string[] KeysOf(Map map)
        {
            using (map.Read())
                return map.AllSymbols.Select(symbol => symbol.Key.Text).ToArray();
        }

        // The definitions of a document: the patterns that hold a tile of their own.
        // document: the document.
        private static List<XElement> Definitions(XDocument document)
        {
            return document.Descendants(Svg + "pattern").Where(pattern => pattern.Attribute(XLink + "href") == null).ToList();
        }

        // The configurations of a document: the patterns that refer to a definition.
        // document: the document.
        private static List<XElement> Configurations(XDocument document)
        {
            return document.Descendants(Svg + "pattern").Where(pattern => pattern.Attribute(XLink + "href") != null).ToList();
        }

        // The drawn elements of the map, outside the definitions of the document.
        // document: the document.
        private static List<XElement> DrawnElements(XDocument document)
        {
            XElement map = document.Descendants(Svg + "g").Single(group => group.Attribute("data-m") != null);
            return map.Descendants().Where(element => element.Name == Svg + "path" || element.Name == Svg + "use" ||
                                                      element.Name == Svg + "circle" || element.Name == Svg + "ellipse").ToList();
        }

        // The style that an element paints with: the declarations of the groups around it, and then its own.
        // element: the element.
        private static string InheritedStyle(XElement element)
        {
            return string.Join(";", element.AncestorsAndSelf().Reverse().Select(carrier => (string)carrier.Attribute("style")).Where(style => style != null));
        }

        // The pattern that an element is filled with, as url(#id), by the last declaration of the fill that it inherits.
        // element: the element.
        private static string FillOf(XElement element)
        {
            return InheritedStyle(element).Split(';').Last(declaration => declaration.StartsWith("fill:", StringComparison.Ordinal)).Substring(5);
        }

        // The clip path that a group refers to.
        // document: the document.
        // group: the group.
        private static XElement ClipOf(XDocument document, XElement group)
        {
            string reference = ((string)group.Attribute("clip-path") ?? "").Replace("url(#", "").TrimEnd(')');
            return document.Descendants(Svg + "clipPath").Single(clip => (string)clip.Attribute("id") == reference);
        }

        // The id of a pattern as a fill refers to it.
        // pattern: the pattern.
        private static string UrlOf(XElement pattern)
        {
            return "url(#" + (string)pattern.Attribute("id") + ")";
        }

        [Test]
        public void ARegularClippedAreaPatternIsAConfigurationOfItsDefinitionThatFillsTheArea()
        {
            XDocument document;
            string[] keys;
            using (Map map = PatternedAreaMap.Create(false, AreaSymDef.PatternFillMode.Clip, PatternedAreaMap.SquaresInEveryPlacement[0])) {
                document = WriteMap("regular", map);
                keys = KeysOf(map);
            }

            // The definition is the pattern tile, two rows of the element as the rows are offset, its middle on the origin.
            XElement definition = Definitions(document).Single();
            Assert.AreEqual("-1000", (string)definition.Attribute("x"));
            Assert.AreEqual("-1600", (string)definition.Attribute("y"));
            Assert.AreEqual("2000", (string)definition.Attribute("width"));
            Assert.AreEqual("3200", (string)definition.Attribute("height"));
            Assert.AreEqual("userSpaceOnUse", (string)definition.Attribute("patternUnits"));
            Assert.IsNull(definition.Attribute("patternTransform"), "a definition is bound to the origin");

            // The configuration refers to it with M · T(-offset), as the tile includes the offset of the area pattern: the
            // square lays its pattern out from the map's origin unturned, so M turns by the angle of the pattern.
            XElement configuration = Configurations(document).Single();
            Assert.AreEqual("#" + (string)definition.Attribute("id"), (string)configuration.Attribute(XLink + "href"));
            Assert.AreEqual("rotate(30)", (string)configuration.Attribute("patternTransform"));

            // The area fills its outline with the configuration, under the even-odd rule of an area, and carries the key
            // of its symbol and the place of the area pattern in its symbol definition. It is all the map draws.
            XElement area = DrawnElements(document).Single();
            Assert.AreEqual(OutlineOfFirstSquare, (string)area.Attribute("d"));
            Assert.AreEqual(UrlOf(configuration), FillOf(area));
            StringAssert.Contains("fill-rule:evenodd", InheritedStyle(area));
            Assert.AreEqual(keys[0], (string)area.Attribute("data-k"));
            Assert.AreEqual("pattern.0", (string)area.Attribute("data-g"));
            Assert.AreEqual(1, document.Descendants(Svg + "clipPath").Count(), "the page's clip alone");
        }

        [Test]
        public void AnIrregularAreaPatternIsTheExpansionOfItsCoveragePrototypeWithinTheClipOfItsArea()
        {
            XDocument document;
            string[] keys;
            using (Map map = PatternedAreaMap.Create(true, AreaSymDef.PatternFillMode.Clip, PatternedAreaMap.SquaresInEveryPlacement[0])) {
                document = WriteMap("irregular", map);
                keys = KeysOf(map);
            }

            Assert.AreEqual(0, document.Descendants(Svg + "pattern").Count());
            XElement coverage = document.Descendants(Svg + "g").Single(group => group.Attribute("data-g") != null);
            Assert.AreEqual(keys[0], (string)coverage.Attribute("data-k"));
            Assert.AreEqual("pattern.0", (string)coverage.Attribute("data-g"));
            XElement clipGroup = coverage.DescendantsAndSelf(Svg + "g").Single(group => group.Attribute("clip-path") != null);
            Assert.AreEqual(OutlineOfFirstSquare, (string)ClipOf(document, clipGroup).Elements(Svg + "path").Single().Attribute("d"));
            Assert.Greater(clipGroup.Descendants(Svg + "use").Count(), 50, "the elements of the pattern, each a use of the shared shapes of its glyph");
        }

        [Test]
        public void TheElementsOfAnAreaPatternInAnotherFillModeAreAPartOfTheFullCoverage()
        {
            XDocument document;
            string[] keys;
            using (Map map = PatternedAreaMap.Create(false, AreaSymDef.PatternFillMode.CenterInside, PatternedAreaMap.SquaresInEveryPlacement[0])) {
                document = WriteMap("center-inside", map);
                keys = KeysOf(map);
            }

            Assert.AreEqual(0, document.Descendants(Svg + "pattern").Count());
            Assert.AreEqual(1, document.Descendants(Svg + "clipPath").Count(), "the page's clip alone");
            Assert.IsFalse(document.Descendants().Any(element => element.Attribute("data-g") != null), "no coverage prototype of the area pattern");
            List<XElement> drawn = DrawnElements(document);
            Assert.Greater(drawn.Count, 50);
            Assert.IsTrue(drawn.All(element => element.AncestorsAndSelf().Any(carrier => (string)carrier.Attribute("data-k") == keys[0])));
        }

        [Test]
        public void EachPaletteCutterIsAConfigurationOfTheOneDefinitionWithItsTransformation()
        {
            XDocument document;
            using (Map map = PatternedAreaMap.Create(false, AreaSymDef.PatternFillMode.Clip, PatternedAreaMap.SquaresInEveryPlacement))
                document = WriteMap("every-placement", map);

            // The squares lay their patterns out in four ways that no symmetry of the pattern makes one, so each is a
            // palette cutter of its own, with M = T(c) · R(a) · R(30) · T(offset) of its rotation center c and its angle a.
            XElement definition = Definitions(document).Single();
            List<XElement> configurations = Configurations(document);
            Assert.AreEqual(4, configurations.Count);
            Assert.IsTrue(configurations.All(configuration => (string)configuration.Attribute(XLink + "href") == "#" + (string)definition.Attribute("id")));
            CollectionAssert.AreEquivalent(new[] { "rotate(30)", "translate(10700 10450)rotate(30)", "rotate(55)", "translate(10700-9550)rotate(55)" },
                                           configurations.Select(configuration => (string)configuration.Attribute("patternTransform")));

            // Each area fills its own outline with the configuration of its cutter.
            List<XElement> areas = DrawnElements(document);
            Assert.AreEqual(4, areas.Count);
            CollectionAssert.AreEquivalent(configurations.Select(UrlOf), areas.Select(FillOf));
        }

        [Test]
        public void AHatchingIsADefinitionOfItsLineInASquareTileAsWideAsItsSpacing()
        {
            XDocument document;
            using (Map map = PatternedAreaMap.CreateHatched(PatternedAreaMap.SquaresInEveryPlacement[0]))
                document = WriteMap("hatched", map);

            // Each hatching's tile is a square as wide as its spacing, its middle on the origin, and holds its line y = 0,
            // which runs on past both sides of the tile; the lines a spacing away do not reach into it. The width of the
            // line is that of the hatching, in the color of its palette color.
            List<XElement> definitions = Definitions(document);
            Assert.AreEqual(2, definitions.Count);
            XElement greenLine = definitions.Single(definition => (string)definition.Attribute("width") == "600").Descendants(Svg + "path").Single();
            XElement blueLine = definitions.Single(definition => (string)definition.Attribute("width") == "700").Descendants(Svg + "path").Single();
            Assert.AreEqual("M-600 0H600", (string)greenLine.Attribute("d"));
            StringAssert.Contains("stroke-width:150", InheritedStyle(greenLine));
            StringAssert.Contains("stroke:#3dff17", InheritedStyle(greenLine));
            Assert.AreEqual("M-700 0H700", (string)blueLine.Attribute("d"));
            StringAssert.Contains("stroke-width:350", InheritedStyle(blueLine));
            StringAssert.Contains("stroke:#00e6ff", InheritedStyle(blueLine));

            // Each configuration places its line by M = R(angle) · T(0, offset) of the square at the map's origin: the blue
            // line 0.31 mm and the green one 1 mm across from the origin, in whole steps. The area fills its outline once
            // with the configuration of each hatching, in the transparency group of each palette color.
            List<XElement> configurations = Configurations(document);
            CollectionAssert.AreEquivalent(new[] { "translate(-305-54)rotate(100)", "translate(-500 866)rotate(30)" },
                                           configurations.Select(configuration => (string)configuration.Attribute("patternTransform")));
            List<XElement> areas = DrawnElements(document);
            Assert.AreEqual(2, areas.Count);
            Assert.IsTrue(areas.All(area => (string)area.Attribute("d") == OutlineOfFirstSquare));
            CollectionAssert.AreEquivalent(new[] { "hatching.0", "hatching.1" }, areas.Select(area => (string)area.Attribute("data-g")));
            CollectionAssert.AreEquivalent(configurations.Select(UrlOf), areas.Select(FillOf));
        }

        [Test]
        public void AHatchingThatNeverTurnsIsOneConfigurationForEveryAreaAndOneThatTurnsIsOneForEachCutter()
        {
            XDocument document;
            using (Map map = PatternedAreaMap.CreateHatched(PatternedAreaMap.SquaresInEveryPlacement))
                document = WriteMap("hatched-every-placement", map);

            // The blue hatching never turns, so its four areas are one palette cutter; the green one turns with each square.
            List<XElement> configurations = Configurations(document);
            XElement blueDefinition = Definitions(document).Single(definition => (string)definition.Attribute("width") == "700");
            XElement greenDefinition = Definitions(document).Single(definition => (string)definition.Attribute("width") == "600");
            Assert.AreEqual(1, configurations.Count(configuration => (string)configuration.Attribute(XLink + "href") == "#" + (string)blueDefinition.Attribute("id")));
            Assert.AreEqual(4, configurations.Count(configuration => (string)configuration.Attribute(XLink + "href") == "#" + (string)greenDefinition.Attribute("id")));
            Assert.AreEqual(8, DrawnElements(document).Count, "each area once in each palette color");
        }

        [Test]
        public void TheAreasOfAnAreaClusterAreFilledOnceWithinAClipToTheUnionOfTheirAreas()
        {
            XDocument document;
            string[] keys;
            using (Map map = PatternedAreaMap.Create(false, AreaSymDef.PatternFillMode.Clip, OverlappingSquares)) {
                document = WriteMap("cluster", map);
                keys = KeysOf(map);
            }

            // The pattern fill carries the keys of both areas, and the place of the area pattern.
            XElement coverage = document.Descendants().Single(element => element.Attribute("data-g") != null);
            Assert.AreEqual(string.Join(" ", keys), (string)coverage.Attribute("data-k"));
            Assert.AreEqual("pattern.0", (string)coverage.Attribute("data-g"));

            // It clips to a clip path with the outline of each area under the even-odd rule of an area, whose union SVG
            // clips to, and fills the rectangle around them once with the configuration.
            XElement clipGroup = coverage.DescendantsAndSelf(Svg + "g").Single(group => group.Attribute("clip-path") != null);
            List<XElement> outlines = ClipOf(document, clipGroup).Elements(Svg + "path").ToList();
            Assert.AreEqual(2, outlines.Count);
            Assert.IsTrue(outlines.All(outline => (string)outline.Attribute("style") == "clip-rule:evenodd"));
            StringAssert.StartsWith("M-12000-12000", (string)outlines[0].Attribute("d"));
            StringAssert.StartsWith("M-4000-4000", (string)outlines[1].Attribute("d"));
            XElement fill = DrawnElements(document).Single();
            Assert.AreEqual(UrlOf(Configurations(document).Single()), FillOf(fill));
            StringAssert.StartsWith("M-12000-12000", (string)fill.Attribute("d"));
            Assert.AreEqual(2, document.Descendants(Svg + "clipPath").Count(), "the page's clip and the union's");
        }

        [Test]
        public void WithTheSettingForSeparateShapesEachAreaWritesTheExpansionOfItsOwnPatternWithinItsOwnClip()
        {
            XDocument document;
            string[] keys;
            using (Map map = PatternedAreaMap.Create(false, AreaSymDef.PatternFillMode.Clip, OverlappingSquares)) {
                document = WriteMap("cluster-separate-shapes", map, true);
                keys = KeysOf(map);
            }

            // No pattern fill and no union: each square clips its own elements to its own outline, as a reader that
            // keeps one child of a clip path draws them (S-7).
            Assert.AreEqual(0, document.Descendants(Svg + "pattern").Count());
            List<XElement> coverages = document.Descendants().Where(element => element.Attribute("data-g") != null).ToList();
            CollectionAssert.AreEqual(keys, coverages.Select(coverage => (string)coverage.Attribute("data-k")));
            foreach (XElement coverage in coverages) {
                XElement clipGroup = coverage.DescendantsAndSelf(Svg + "g").Single(group => group.Attribute("clip-path") != null);
                Assert.AreEqual(1, ClipOf(document, clipGroup).Elements(Svg + "path").Count());
                Assert.Greater(clipGroup.Descendants(Svg + "use").Count(), 50);
            }
        }

        [Test]
        public void WithTheSettingForSeparateShapesAHatchingIsWrittenLineByLine()
        {
            XDocument document;
            using (Map map = PatternedAreaMap.CreateHatched(PatternedAreaMap.SquaresInEveryPlacement[0]))
                document = WriteMap("hatched-separate-shapes", map, true);

            Assert.AreEqual(0, document.Descendants(Svg + "pattern").Count());
            List<XElement> coverages = document.Descendants().Where(element => element.Attribute("data-g") != null).ToList();
            Assert.AreEqual(2, coverages.Count);
            foreach (XElement coverage in coverages) {
                XElement clipGroup = coverage.DescendantsAndSelf(Svg + "g").Single(group => group.Attribute("clip-path") != null);
                Assert.AreEqual(OutlineOfFirstSquare, (string)ClipOf(document, clipGroup).Elements(Svg + "path").Single().Attribute("d"));
                Assert.Greater(clipGroup.Descendants(Svg + "path").Count(), 20, "the lines of the hatching over the area, one by one");
            }
        }
    }
}
