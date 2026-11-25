using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Map_SkiaStd;
using NUnit.Framework;
using PurplePen.Graphics2D;
using PurplePen.MapModel;
using TestingUtils;

namespace Map_Svg.Tests
{
    // Checks the structure that the SVG encoding writes of the transparency groups of pass 2, with the intermediate
    // layer of the map as the oracle: the transparency group of a palette color carries its opacity and the blend mode
    // that the overprint mode gives it; the palette color coverages are written in the order of the palette separation,
    // the consecutive coverages of one symbol definition in its group; a run of dashes is one path with the dash pattern
    // of its pen, and with the setting for separate shapes its dashes; the run of dashes of an offset path is its single
    // dashes; a repeated placement is a group with a use of the shared shapes of its glyph at each pose; and an object of
    // the layout layer, which the map draws beside its palette colors, carries its symbol key outside the transparency
    // groups. The structures of the pattern fills are checked by AreaPatternTests.
    [TestFixture]
    public class SvgEncodingTests
    {
        private static readonly XNamespace Svg = "http://www.w3.org/2000/svg";
        private static readonly XNamespace XLink = "http://www.w3.org/1999/xlink";
        private static readonly XNamespace Npp = "https://pauliojanpera.github.io/NotPurplePen/svg";

        // The resolution of the drawings, in millimetres of map: the step the target writes coordinates in, at which no
        // dashed line component of the test maps is solid.
        private const float Resolution = 0.001F;

        // The metrics the application lays text out with, and the font data of the faces drawn in, which the documents
        // embed (T-1, T-6).
        private static readonly ITextMetrics TextMetrics = new Skia_TextMetrics();
        private static readonly IFontSubsetProvider FontSubsets = new SkiaFontSubsetProvider();

        // Write the drawing of a map into the SVG target, in one part shown on one page, and return the document, which
        // the loop's results keep under its name.
        // name: the name of the document in the loop's results.
        // map: the map.
        // overprintMode: the overprint mode of the drawing.
        // separateShapes: whether area patterns and dashed lines are drawn as separate shapes.
        private static XDocument WriteMap(string name, Map map, OverprintMode overprintMode = OverprintMode.None, bool separateShapes = false)
        {
            RectangleF area;
            using (map.Read())
                area = map.Bounds;
            area.Inflate(1, 1);
            using (Svg_GraphicsTarget target = new Svg_GraphicsTarget(DefaultColorConverter.Instance, true, SvgBitmapOptions.Default, TextMetrics, FontSubsets))
            using (Matrix mapToPage = new Matrix()) {
                RenderOptions options = new RenderOptions();
                options.usePatternBitmaps = !separateShapes;
                options.patternsWithEachArea = separateShapes;
                options.minResolution = Resolution;
                options.overprintMode = overprintMode;

                object part = new object();
                target.BeginPart(part);
                target.BeginMap(map, "map");
                using (map.Read())
                    map.Draw(target, area, options, null);
                target.EndMap();
                target.EndPart();

                mapToPage.Translate(-area.Left, area.Bottom);
                mapToPage.Scale(1, -1);
                target.AddPage(area.Size, area, mapToPage, new object[] { part }, null);
                string document = target.FinishDocument();
                LoopResults.SaveSvg("encoding-" + name, document);
                return XDocument.Parse(document);
            }
        }

        // Load a map of the Skia rendering tests.
        // fileName: the name of its file in skia_render.
        private static Map LoadMap(string fileName)
        {
            return ComparisonCase.LoadMap(TestUtil.GetTestFile(Path.Combine("skia_render", fileName)));
        }

        // The style that an element paints with: the declarations of the groups around it, and then its own.
        // element: the element.
        private static string InheritedStyle(XElement element)
        {
            return string.Join(";", element.AncestorsAndSelf().Reverse().Select(carrier => (string)carrier.Attribute("style")).Where(style => style != null));
        }

        // The value of the last declaration of a property in a style, or null when none declares it.
        // style: the style.
        // property: the name of the property.
        private static string Declared(string style, string property)
        {
            return style.Split(';').Where(declaration => declaration.StartsWith(property + ":", StringComparison.Ordinal))
                        .Select(declaration => declaration.Substring(property.Length + 1)).LastOrDefault();
        }

        // The symbol keys that an element carries, on itself or on a group around it.
        // element: the element.
        private static string[] KeysOf(XElement element)
        {
            XElement carrier = element.AncestorsAndSelf().FirstOrDefault(ancestor => ancestor.Attribute("data-k") != null);
            return carrier == null ? new string[0] : ((string)carrier.Attribute("data-k")).Split(' ');
        }

        // The elements that a palette color coverage is written as: those of the group or the element that carries the key
        // of its symbol in the transparency group of its palette color.
        // document: the document.
        // map: the map, open for reading.
        // coverage: the palette color coverage.
        private static List<XElement> ElementsOfCoverage(XDocument document, Map map, SymbolCoverage coverage)
        {
            string key = coverage.Symbol.Key.Text;
            string position = map.AllColors.ToList().IndexOf(coverage.Color).ToString(System.Globalization.CultureInfo.InvariantCulture);
            XElement transparencyGroup = document.Descendants(Svg + "g").Single(group => (string)group.Attribute("data-c") == position);
            List<XElement> elements = new List<XElement>();
            foreach (XElement carrier in transparencyGroup.Descendants().Where(element => element.Attribute("data-k") != null &&
                                                                                        ((string)element.Attribute("data-k")).Split(' ').Contains(key))) {
                elements.AddRange(carrier.DescendantsAndSelf().Where(element => element.Name == Svg + "path" || element.Name == Svg + "use" ||
                                                                                  element.Name == Svg + "circle" || element.Name == Svg + "ellipse" ||
                                                                                  element.Name == Svg + "text"));
            }
            return elements;
        }

        // The palette color coverages of the full coverage of a map, with their clipping objects at the resolution.
        // map: the map, open for reading.
        private static IEnumerable<(SymbolCoverage, IReadOnlyList<ClippingObject>)> FullCoverages(Map map)
        {
            foreach (PaletteSeparation separation in map.PaletteSeparations) {
                foreach (SymbolCoverage coverage in separation.SymbolCoverages) {
                    if (coverage.Prototype == CoveragePrototype.FullCoverage)
                        yield return (coverage, coverage.GetClippingObjects(Resolution));
                }
            }
        }

        // Add a square of an area symbol to a map.
        // map: the map, open for writing.
        // symdef: the area symbol definition.
        // left, bottom: the corner of the square.
        private static void AddSquare(Map map, AreaSymDef symdef, float left, float bottom)
        {
            SymPath outline = new SymPath(new PointF[] {
                new PointF(left, bottom), new PointF(left + 4, bottom), new PointF(left + 4, bottom + 4), new PointF(left, bottom + 4), new PointF(left, bottom)
            });
            map.AddSymbol(new AreaSymbol(symdef, new SymPathWithHoles(outline, null), 0, new PointF(0, 0)));
        }

        [Test]
        public void EachPaletteColorIsATransparencyGroupWithItsOpacityAndTheBlendModeOfTheOverprintMode()
        {
            using (Map map = new Map(new Skia_TextMetrics(), new Skia_FileLoader(AppContext.BaseDirectory))) {
                SymColor translucent = new SymColor("Translucent", 1, 0, 1, 0, 0, false);
                translucent.Opacity = 0.5F;
                translucent.BlendMode = ColorBlendMode.Multiply;
                SymColor overprinted = new SymColor("Overprinted", 2, 1, 0, 0, 0, true);
                SymColor plain = new SymColor("Plain", 3, 0, 0, 1, 0, false);
                using (map.Write()) {
                    foreach (SymColor color in new[] { translucent, overprinted, plain }) {
                        map.AddColor(color);
                        AreaSymDef symdef = new AreaSymDef(color.Name, "40" + color.OcadId + ".0", color, null);
                        map.AddSymdef(symdef);
                        AddSquare(map, symdef, color.OcadId * 5, 0);
                    }
                }
                List<SymColor> table;
                using (map.Read())
                    table = map.AllColors.ToList();

                // Each mode, with the blend mode it gives each color: by the color's own in ByPaletteColor, and to the
                // overprinted color alone in Darken and Multiply. The opacity applies in each mode.
                Dictionary<OverprintMode, string[]> blendModes = new Dictionary<OverprintMode, string[]> {
                    { OverprintMode.None, new string[] { null, null, null } },
                    { OverprintMode.ByPaletteColor, new string[] { "multiply", null, null } },
                    { OverprintMode.Darken, new string[] { null, "darken", null } },
                    { OverprintMode.Multiply, new string[] { null, "multiply", null } },
                };
                foreach (KeyValuePair<OverprintMode, string[]> mode in blendModes) {
                    XDocument document = WriteMap("overprint-" + mode.Key, map, mode.Key);
                    SymColor[] colors = { translucent, overprinted, plain };
                    for (int index = 0; index < colors.Length; ++index) {
                        string position = table.IndexOf(colors[index]).ToString(System.Globalization.CultureInfo.InvariantCulture);
                        XElement group = document.Descendants(Svg + "g").Single(element => (string)element.Attribute("data-c") == position);
                        string style = (string)group.Attribute("style");
                        Assert.AreEqual(colors[index] == translucent ? ".5" : null, Declared(style, "opacity"), mode.Key + ", " + colors[index].Name);
                        Assert.AreEqual(mode.Value[index], Declared(style, "mix-blend-mode"), mode.Key + ", " + colors[index].Name);
                        StringAssert.StartsWith("fill:", style, "the group carries the paint of its color");
                    }
                }
            }
        }

        [Test]
        public void TheCoveragesOfAPaletteColorAreWrittenInTheOrderOfItsPaletteSeparation()
        {
            using (Map map = new Map(new Skia_TextMetrics(), new Skia_FileLoader(AppContext.BaseDirectory))) {
                SymColor color;
                using (map.Write()) {
                    color = map.AddColor("Green", 1, 1, 0, 1, 0, false);
                    AreaSymDef first = new AreaSymDef("First", "401.0", color, null), second = new AreaSymDef("Second", "402.0", color, null);
                    map.AddSymdef(first);
                    map.AddSymdef(second);
                    AddSquare(map, first, 0, 0);
                    AddSquare(map, second, 5, 0);
                    AddSquare(map, first, 10, 0);
                }
                XDocument document = WriteMap("order", map);

                List<string> expected;
                List<string> definitionsInOrder;
                using (map.Read()) {
                    PaletteSeparation separation = map.PaletteSeparations.Single(candidate => candidate.Color == color);
                    expected = separation.SymbolCoverages.Select(coverage => coverage.Symbol.Key.Text).ToList();
                    definitionsInOrder = separation.SymbolCoverages.Select(coverage => coverage.Symbol.Definition.Name).ToList();
                }
                XElement group = document.Descendants(Svg + "g").Single(element => element.Attribute("data-c") != null);
                CollectionAssert.AreEqual(expected, group.Descendants().Where(element => element.Attribute("data-k") != null).Select(element => (string)element.Attribute("data-k")));

                // The consecutive coverages of one symbol definition share its group.
                int runs = definitionsInOrder.Where((name, index) => index == 0 || name != definitionsInOrder[index - 1]).Count();
                Assert.AreEqual(runs, group.Elements(Svg + "g").Count(element => element.Attribute("data-s") != null));
            }
        }

        [Test]
        public void ARunOfDashesIsOnePathWithTheDashPatternOfItsPenAndWithTheSettingForSeparateShapesItsDashes()
        {
            using (Map map = LoadMap("dashline.ocd")) {
                XDocument document = WriteMap("dashes", map);
                XDocument separate = WriteMap("dashes-separate-shapes", map, OverprintMode.None, true);

                int runsSeen = 0;
                using (map.Read()) {
                    foreach ((SymbolCoverage coverage, IReadOnlyList<ClippingObject> clippingObjects) in FullCoverages(map)) {
                        List<Stroke> pens = clippingObjects.OfType<StrokedShape>().Where(stroked => stroked.Shape is PathShape && stroked.Stroke.DashPattern != null &&
                                                                                                    stroked.Stroke.Width > 0)
                                                           .Select(stroked => stroked.Stroke).ToList();
                        if (pens.Count == 0)
                            continue;
                        runsSeen += pens.Count;
                        string key = coverage.Symbol.Key.Text;
                        List<string> dashArrays = ElementsOfCoverage(document, map, coverage).Select(element => Declared(InheritedStyle(element), "stroke-dasharray"))
                                                                                   .Where(dashArray => dashArray != null).ToList();
                        foreach (Stroke pen in pens) {
                            string dashArray = string.Join(" ", pen.DashPattern.Select(length => SvgNumbers.FormatDecimal(length * SvgNumbers.StepsPerMillimetre, 3)));
                            CollectionAssert.Contains(dashArrays, dashArray, "the run of symbol " + key);
                        }
                    }
                }
                Assert.Greater(runsSeen, 0, "the map has runs of dashes");
                Assert.IsFalse(separate.Descendants().Any(element => Declared((string)element.Attribute("style") ?? "", "stroke-dasharray") != null),
                               "with the setting for separate shapes, each dash is a path of its own");
                Assert.Greater(separate.Descendants(Svg + "path").Count(), document.Descendants(Svg + "path").Count());
            }
        }

        [Test]
        public void TheRunOfDashesOfAnOffsetPathIsItsSingleDashes()
        {
            using (Map map = LoadMap("cutdoublesides9.ocd")) {
                XDocument document = WriteMap("offset-dashes", map);

                // A dash pattern of SVG would be measured along the offset curve, and not along the base path, so each
                // dash of a border is a path of its own, without a dash pattern; the runs of dashes of the base paths keep
                // theirs.
                int coveragesSeen = 0;
                using (map.Read()) {
                    foreach ((SymbolCoverage coverage, IReadOnlyList<ClippingObject> clippingObjects) in FullCoverages(map)) {
                        List<StrokedShape> dashedOffsets = clippingObjects.OfType<StrokedShape>()
                                                                          .Where(stroked => stroked.Shape is OffsetPathShape && stroked.Stroke.DashPattern != null && stroked.Stroke.Width > 0)
                                                                          .ToList();
                        if (dashedOffsets.Count == 0)
                            continue;
                        ++coveragesSeen;
                        List<XElement> elements = ElementsOfCoverage(document, map, coverage);
                        int dashedPlainPaths = clippingObjects.OfType<StrokedShape>().Count(stroked => stroked.Shape is PathShape && stroked.Stroke.DashPattern != null &&
                                                                                                       stroked.Stroke.Width > 0);
                        Assert.AreEqual(dashedPlainPaths, elements.Count(element => Declared(InheritedStyle(element), "stroke-dasharray") != null),
                                        "the elements of symbol " + coverage.Symbol.Key.Text + " with a dash pattern");
                        Assert.GreaterOrEqual(elements.Count(element => element.Name == Svg + "path" && Declared(InheritedStyle(element), "stroke-dasharray") == null),
                                              dashedOffsets.Sum(stroked => stroked.ExpandDashes().Count));
                    }
                }
                Assert.Greater(coveragesSeen, 0, "the map has dashed offset paths");
            }
        }

        [Test]
        public void ARepeatedPlacementIsAGroupWithAUseOfTheSharedShapesOfItsGlyphAtEachPose()
        {
            using (Map map = LoadMap("mainglyph9.ocd")) {
                XDocument document = WriteMap("repeated-placements", map);

                int placementsSeen = 0;
                using (map.Read()) {
                    foreach ((SymbolCoverage coverage, IReadOnlyList<ClippingObject> clippingObjects) in FullCoverages(map)) {
                        List<RepeatedPlacement> placements = clippingObjects.Select(clippingObject => clippingObject.Placement).OfType<RepeatedPlacement>().Distinct().ToList();
                        if (placements.Count == 0)
                            continue;
                        placementsSeen += placements.Count;

                        // Each repeated placement is a group of uses, one at each pose, of one definition of the shared
                        // shapes of the consecutive clipping objects at the placement.
                        List<XElement> carriers = document.Descendants().Where(element => (string)element.Attribute("data-k") == coverage.Symbol.Key.Text).ToList();
                        List<XElement> groupsOfUses = carriers.SelectMany(carrier => carrier.DescendantsAndSelf(Svg + "g"))
                                                              .Where(group => group.Elements().Any() && group.Elements().All(child => child.Name == Svg + "use"))
                                                              .ToList();
                        CollectionAssert.AreEquivalent(placements.Select(placement => placement.Poses.Count), groupsOfUses.Select(group => group.Elements().Count()),
                                                       "the poses of symbol " + coverage.Symbol.Key.Text);
                        foreach (XElement group in groupsOfUses) {
                            Assert.AreEqual(1, group.Elements().Select(use => (string)use.Attribute(XLink + "href")).Distinct().Count());
                            string id = ((string)group.Elements().First().Attribute(XLink + "href")).Substring(1);
                            XElement definition = document.Root.Element(Svg + "defs").Elements().Single(element => (string)element.Attribute("id") == id);
                            foreach (XElement shape in definition.DescendantsAndSelf().Where(element => element.Name != Svg + "g")) {
                                string style = (string)shape.Attribute("style") ?? "";
                                Assert.IsTrue(new[] { null, "none" }.Contains(Declared(style, "fill")) && new[] { null, "none" }.Contains(Declared(style, "stroke")),
                                              "the shared shapes carry their fill rules or pens, and take the paint of the transparency group: " + style);
                            }
                        }
                    }
                }
                Assert.Greater(placementsSeen, 0, "the map has repeated placements");
            }
        }

        [Test]
        public void AnObjectOfTheLayoutLayerCarriesItsSymbolKeyOutsideTheTransparencyGroups()
        {
            using (Map map = LoadMap("layout_objects11.ocd")) {
                XDocument document = WriteMap("layout-layer", map);
                HashSet<string> keys;
                using (map.Read())
                    keys = new HashSet<string>(map.AllSymbols.Select(symbol => symbol.Key.Text), StringComparer.Ordinal);

                XElement mapGroup = document.Descendants(Svg + "g").Single(group => group.Attribute("data-m") != null);
                List<XElement> outside = mapGroup.Descendants().Where(element => element.Attribute("data-k") != null &&
                                                                                  !element.Ancestors(Svg + "g").Any(group => group.Attribute("data-c") != null))
                                                 .ToList();
                Assert.IsNotEmpty(outside, "the layout layer draws objects beside the palette colors");
                foreach (XElement carrier in outside) {
                    Assert.IsTrue(keys.Contains((string)carrier.Attribute("data-k")), "the key of a symbol of the map");
                    XElement symbolGroup = carrier.AncestorsAndSelf(Svg + "g").FirstOrDefault(group => group.Attribute("data-s") != null);
                    Assert.IsNotNull(symbolGroup, "within the group of its symbol definition");
                    XElement record = document.Descendants(Npp + "symbol").Single(symbol => (string)symbol.Attribute("n") == (string)symbolGroup.Attribute("data-s"));
                    Assert.IsNull(record.Attribute("colors"), "a symbol definition drawn beside the palette colors lists none");
                }
            }
        }
    }
}
