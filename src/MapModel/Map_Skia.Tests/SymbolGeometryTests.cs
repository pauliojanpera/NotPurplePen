using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using NUnit.Framework;
using PurplePen.Graphics2D;
using PurplePen.MapModel;
using TestingUtils;

namespace Map_Skia.Tests
{
    // The geometry methods of the symbols (Symbol.TryAddClippingObjects), with which pass 1 (Map.CreateLayerInstances)
    // makes the clipping objects of the kinds of symbols that have one, and the collector those of the other kinds. The
    // instances that the collector makes of all the symbols (Map.CollectLayerInstances) are the oracle: pass 1 makes
    // instances of the same structure (LayerInstanceComparison). The other tests check what the oracle does not show:
    // the point symbols of a definition share the shapes of its glyph.
    [TestFixture, Parallelizable(ParallelScope.Children)]
    public class SymbolGeometryTests
    {
        // Let RenderingTests add the Roboto fonts, which the text symbols use, before the tests of the fixture, so that
        // both passes of a comparison measure the texts in the same fonts.
        [OneTimeSetUp]
        public void AddFontsOfTheRenderingTests()
        {
            System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(RenderingTests).TypeHandle);
        }

        // Each map of the Skia rendering tests.
        [TestCaseSource(nameof(FindMapFilesOfTheRenderingTests))]
        public void PassOneMakesTheInstancesOfTheCollector(string mapFileName)
        {
            Map map = ReadMap(mapFileName);
            using (map.Read()) {
                IReadOnlyList<LayerInstance> expected = map.CollectLayerInstances();
                IReadOnlyList<LayerInstance> actual = map.CreateLayerInstances();

                TestContext.WriteLine("{0} point symbols, {1} instances, {2} clipping objects", map.AllSymbols.OfType<PointSymbol>().Count(), expected.Count, expected.Sum(instance => instance.ClippingObjects.Count));
                AssertSameStructure(expected, actual);
            }
        }

        [Test]
        public void PointSymbolsOfADefinitionShareTheShapesOfItsGlyph()
        {
            Map map = CreateMap();
            SymColor brown, blue, black;
            using (map.Write()) {
                brown = map.AddColor("Brown", 1, 0, 0.56F, 1, 0.18F, false);
                blue = map.AddColor("Blue", 2, 1, 0, 0, 0, false);
                black = map.AddColor("Black", 3, 0, 0, 0, 1, false);

                Glyph glyph = new Glyph();
                glyph.AddLine(brown, new SymPath(new PointF[] { new PointF(-1, 0), new PointF(1, 0) }), 0.2F, LineJoinMode.Round, LineCapMode.Round);
                glyph.AddArea(blue, new SymPathWithHoles(CreateSquarePath(-0.5F, -0.5F, 1), null));
                glyph.AddCircle(black, new PointF(0, 1), 0.1F, 0.8F);
                glyph.AddFilledCircle(black, new PointF(0, -1), 0.4F);
                glyph.ConstructionComplete();
                PointSymDef symdef = new PointSymDef("Knoll", "1", glyph, true);
                map.AddSymdef(symdef);
                map.AddSymbol(new PointSymbol(symdef, new PointF(5, 3), 0, null));
                map.AddSymbol(new PointSymbol(symdef, new PointF(-2, 8), 30, null));
                map.AddSymbol(new PointSymbol(symdef, new PointF(5, 3), 90, null));
            }

            IReadOnlyList<LayerInstance> instances = CreateInstances(map);

            CollectionAssert.AreEqual(new SymColor[] { brown, blue, black }, instances.Select(instance => instance.PaletteEntry));
            CollectionAssert.AreEqual(new int[] { 3, 3, 6 }, instances.Select(instance => instance.ClippingObjects.Count));
            // Each part of the glyph is one shape, which each symbol places with its placement.
            ClippingObject[][] partsOfSymbols = Enumerable.Range(0, 3).Select(symbol => new ClippingObject[] {
                instances[0].ClippingObjects[symbol], instances[1].ClippingObjects[symbol],
                instances[2].ClippingObjects[2 * symbol], instances[2].ClippingObjects[2 * symbol + 1] }).ToArray();
            for (int part = 0; part < 4; ++part) {
                Assert.AreSame(partsOfSymbols[0][part].Shape, partsOfSymbols[1][part].Shape, "part {0}", part);
                Assert.AreSame(partsOfSymbols[0][part].Shape, partsOfSymbols[2][part].Shape, "part {0}", part);
            }
            // The parts of a symbol share its placement, of one step, and the symbols have placements of their own.
            foreach (ClippingObject[] partsOfSymbol in partsOfSymbols) {
                Assert.AreEqual(1, partsOfSymbol[0].Placement.Steps.Count);
                foreach (ClippingObject part in partsOfSymbol)
                    Assert.AreSame(partsOfSymbol[0].Placement, part.Placement);
            }
            CollectionAssert.AreNotEqual(partsOfSymbols[0][0].Placement.Matrix.Elements, partsOfSymbols[2][0].Placement.Matrix.Elements);
            AssertBounds(RectangleF.FromLTRB(4.5F, 2.5F, 5.5F, 3.5F), partsOfSymbols[2][1].Bounds);

            // The collector makes a shape of each drawing call. Its instances have the same structure.
            IReadOnlyList<LayerInstance> collected = CollectInstances(map);
            Assert.AreNotSame(collected[0].ClippingObjects[0].Shape, collected[0].ClippingObjects[1].Shape);
            AssertSameStructure(collected, instances);
        }

        [Test]
        public void GlyphOfOneCircleAtItsOriginIsAtTheLocationOfEachSymbol()
        {
            Map map = CreateMap();
            using (map.Write()) {
                SymColor black = map.AddColor("Black", 1, 0, 0, 0, 1, false);
                Glyph ring = new Glyph();
                ring.AddCircle(black, new PointF(0, 0), 0.1F, 2.0F);
                ring.ConstructionComplete();
                PointSymDef symdef = new PointSymDef("Ring", "1", ring, false);
                map.AddSymdef(symdef);
                map.AddSymbol(new PointSymbol(symdef, new PointF(5, 3), 0, null));
                map.AddSymbol(new PointSymbol(symdef, new PointF(-2, 8), 45, null));
            }

            IReadOnlyList<ClippingObject> rings = CreateInstances(map).Single().ClippingObjects;

            // Each symbol has its own circle, at its location, without a transformation.
            Assert.AreEqual(2, rings.Count);
            Assert.AreNotSame(rings[0].Shape, rings[1].Shape);
            CollectionAssert.AreEqual(new PointF[] { new PointF(5, 3), new PointF(-2, 8) }, rings.Select(ring => ((EllipseShape)ring.Shape).Center));
            foreach (ClippingObject ring in rings) {
                Assert.AreEqual(0, ring.Placement.Steps.Count);
                Assert.AreEqual(0.95F, ((EllipseShape)ring.Shape).RadiusX, Tolerance);
            }
            AssertSameStructure(CollectInstances(map), CreateInstances(map));
        }

        [Test]
        public void ArcsThatTheGapsOfASymbolLeaveAreItsOwnShapes()
        {
            Map map = CreateMap();
            using (map.Write()) {
                SymColor black = map.AddColor("Black", 1, 0, 0, 0, 1, false);
                Glyph glyph = new Glyph();
                glyph.AddCircle(black, new PointF(0, 0), 0.1F, 2.0F);
                glyph.AddFilledCircle(black, new PointF(0, 0), 0.5F);
                glyph.ConstructionComplete();
                PointSymDef symdef = new PointSymDef("Ring and dot", "1", glyph, false);
                map.AddSymdef(symdef);
                // Two gaps leave two arcs: from 90 to 180 degrees, and from 270 around to 0 degrees.
                map.AddSymbol(new PointSymbol(symdef, new PointF(5, 3), 0, new float[] { 0, 90, 180, 270 }));
                map.AddSymbol(new PointSymbol(symdef, new PointF(-2, 8), 0, new float[] { 0, 90, 180, 270 }));
                map.AddSymbol(new PointSymbol(symdef, new PointF(1, 1), 0, null));
            }

            IReadOnlyList<ClippingObject> clippingObjects = CreateInstances(map).Single().ClippingObjects;

            // The symbols with gaps have two arcs and the shared dot each, and the symbol without gaps the shared ring.
            Assert.AreEqual(8, clippingObjects.Count);
            ArcShape[] arcs = { (ArcShape)clippingObjects[0].Shape, (ArcShape)clippingObjects[1].Shape, (ArcShape)clippingObjects[3].Shape, (ArcShape)clippingObjects[4].Shape };
            CollectionAssert.AreEqual(new float[] { 90, 270, 90, 270 }, arcs.Select(arc => arc.StartAngle));
            CollectionAssert.AreEqual(new float[] { 90, 90, 90, 90 }, arcs.Select(arc => arc.SweepAngle));
            Assert.AreNotSame(arcs[0], arcs[2]);
            Assert.AreSame(clippingObjects[0].Placement, clippingObjects[2].Placement);
            Assert.AreSame(clippingObjects[2].Shape, clippingObjects[5].Shape);
            Assert.AreSame(clippingObjects[2].Shape, clippingObjects[7].Shape);
            Assert.IsInstanceOf<EllipseShape>(clippingObjects[6].Shape);
            // The arcs have the line of the shared ring.
            Assert.AreSame(((StrokedShape)clippingObjects[6]).Stroke, ((StrokedShape)clippingObjects[0]).Stroke);
            Assert.AreEqual(0.95F, arcs[0].Radius, Tolerance);
            AssertSameStructure(CollectInstances(map), CreateInstances(map));
        }

        // The branches of the drawing of the glyphs of point symbols, each of which the geometry method follows: circles
        // whose lines are as wide as their diameters or wider, a glyph of one circle at its origin with an empty array of
        // gaps and with an odd number of gap angles, parts that cover nothing, a line with Bezier curves, a closed line,
        // and an area with a hole.
        [Test]
        public void GeometryMethodOfPointSymbolsFollowsEachBranchOfTheirDrawing()
        {
            Map map = CreateMap();
            using (map.Write()) {
                SymColor green = map.AddColor("Green", 1, 0.76F, 0, 0.91F, 0, false);
                SymColor brown = map.AddColor("Brown", 2, 0, 0.56F, 1, 0.18F, false);
                SymColor black = map.AddColor("Black", 3, 0, 0, 0, 1, false);

                AddPointSymbols(map, CreateGlyph(glyph => glyph.AddCircle(black, new PointF(0, 0), 0.5F, 0.3F)), null, new float[0]);
                AddPointSymbols(map, CreateGlyph(glyph => glyph.AddCircle(black, new PointF(0, 0), 0.3F, 0.3F)), (float[])null);
                AddPointSymbols(map, CreateGlyph(glyph => glyph.AddCircle(brown, new PointF(0, 0), 0.1F, 1.0F)), null, new float[0], new float[] { 10, 80, 200 }, new float[] { 300, 30 });
                AddPointSymbols(map, CreateGlyph(glyph => glyph.AddFilledCircle(green, new PointF(0, 0), 0.6F)), null, new float[] { 0, 90 });
                AddPointSymbols(map, CreateGlyph(glyph => glyph.AddFilledCircle(green, new PointF(0, 0), 0)), (float[])null);
                AddPointSymbols(map, CreateGlyph(glyph => {
                    glyph.AddCircle(black, new PointF(1, 0), 0.4F, 0.3F);
                    glyph.AddLine(black, new SymPath(new PointF[] { new PointF(0, 0), new PointF(1, 1) }), 0, LineJoinMode.Miter, LineCapMode.Flat);
                    glyph.AddLine(black, new SymPath(new PointF[] { new PointF(1, 1), new PointF(1, 1) }), 0.1F, LineJoinMode.Miter, LineCapMode.Flat);
                    glyph.AddFilledCircle(green, new PointF(0, 1), 0);
                    glyph.AddLine(brown, new SymPath(new PointF[] { new PointF(0, 0), new PointF(0.5F, 1), new PointF(1.5F, 1), new PointF(2, 0) },
                                                     new PointKind[] { PointKind.Normal, PointKind.BezierControl, PointKind.BezierControl, PointKind.Normal }), 0.15F, LineJoinMode.Round, LineCapMode.Round);
                    glyph.AddLine(black, CreateSquarePath(-1, -1, 0.5F), 0.1F, LineJoinMode.Bevel, LineCapMode.Square);
                    glyph.AddArea(green, new SymPathWithHoles(CreateSquarePath(-2, -2, 4), new SymPath[] { CreateSquarePath(-1, -1, 2) }));
                    glyph.AddCircle(brown, new PointF(-1, 0), 0.1F, 0.5F);
                }), null, new float[] { 45, 135 });
            }

            IReadOnlyList<LayerInstance> instances = CreateInstances(map);

            AssertSameStructure(CollectInstances(map), instances);
            // Green: two dots and two areas with a hole. Brown: two whole rings, an arc that three gap angles leave, an
            // arc around 0 degrees, and two lines with Bezier curves, one ring and one arc of the last glyph. Black: dots
            // of the circles whose line is at least as wide as their diameter, two of the first glyph, one of the second
            // and one of each of the two last glyphs, and two closed lines.
            CollectionAssert.AreEqual(new int[] { 4, 8, 7 }, instances.Select(instance => instance.ClippingObjects.Count));
        }

        // The branches of the drawing of solid lines, each of which the geometry method of lines follows: a polyline, a
        // closed line, a line with a Bezier curve and a line of zero length; pointy ends, also of a line that the
        // shortening takes whole; a second line; and the fill and the borders of a double line, also of a line whose
        // left border stops along it.
        [Test]
        public void GeometryMethodOfSolidLinesFollowsEachBranchOfTheirDrawing()
        {
            Map map = CreateMap();
            List<LineSymbol> lines = new List<LineSymbol>();
            using (map.Write()) {
                SymColor green = map.AddColor("Green", 1, 0.76F, 0, 0.91F, 0, false);
                SymColor brown = map.AddColor("Brown", 2, 0, 0.56F, 1, 0.18F, false);
                SymColor black = map.AddColor("Black", 3, 0, 0, 0, 1, false);

                LineSymDef main = AddLineSymDef(map, new LineSymDef("Main", "1", black, 0.35F, LineJoinMode.Miter, LineCapMode.Flat));
                lines.Add(AddLine(map, main, new SymPath(new PointF[] { new PointF(0, 0), new PointF(4, 1), new PointF(6, -1) })));
                lines.Add(AddLine(map, main, CreateSquarePath(8, 0, 2)));
                lines.Add(AddLine(map, main, new SymPath(new PointF[] { new PointF(12, 0), new PointF(13, 2), new PointF(15, 2), new PointF(16, 0) },
                                                         new PointKind[] { PointKind.Normal, PointKind.BezierControl, PointKind.BezierControl, PointKind.Normal })));
                lines.Add(AddLine(map, main, new SymPath(new PointF[] { new PointF(18, 0), new PointF(18, 0) })));

                LineSymDef pointy = new LineSymDef("Pointy", "2", black, 0.5F, LineJoinMode.Round, LineCapMode.Round);
                pointy.SetShortening(new LineSymDef.ShortenInfo() { shortenBeginning = 1.0F, shortenEnd = 0.5F, pointyEnds = true });
                AddLineSymDef(map, pointy);
                lines.Add(AddLine(map, pointy, new SymPath(new PointF[] { new PointF(0, 5), new PointF(3, 6), new PointF(6, 5) })));
                lines.Add(AddLine(map, pointy, new SymPath(new PointF[] { new PointF(8, 5), new PointF(9, 5) })));

                LineSymDef second = new LineSymDef("Second", "3", brown, 0.6F, LineJoinMode.Bevel, LineCapMode.Round);
                second.SetSecondLine(black, 0.2F, LineJoinMode.Bevel, LineCapMode.Square);
                AddLineSymDef(map, second);
                lines.Add(AddLine(map, second, new SymPath(new PointF[] { new PointF(0, 10), new PointF(3, 12), new PointF(6, 10) })));

                LineSymDef doubled = new LineSymDef("Double", "4", null, 0, LineJoinMode.Miter, LineCapMode.Flat);
                doubled.SetDoubleLines(new LineSymDef.DoubleLineInfo() {
                    doubleThick = 0.8F, doubleFillColor = green, doubleLeftColor = black, doubleLeftWidth = 0.1F, doubleRightColor = black, doubleRightWidth = 0.15F });
                AddLineSymDef(map, doubled);
                lines.Add(AddLine(map, doubled, new SymPath(new PointF[] { new PointF(0, 15), new PointF(3, 18), new PointF(6, 15) })));
                lines.Add(AddLine(map, doubled, new SymPath(new PointF[] { new PointF(8, 15), new PointF(10, 16), new PointF(12, 16), new PointF(14, 15) },
                                                            new PointKind[] { PointKind.Normal, PointKind.Normal, PointKind.Normal, PointKind.Normal },
                                                            new byte[] { 0, SymPath.DOUBLE_LEFT_STARTSTOPFLAG, 0 })));
            }

            using (map.Read()) {
                foreach (LineSymbol line in lines)
                    Assert.IsTrue(line.TryAddClippingObjects(new LayerInstanceBuilder(map.AllColors)), "{0}", line.Definition.Name);
            }
            IReadOnlyList<LayerInstance> instances = CreateInstances(map);

            AssertSameStructure(CollectInstances(map), instances);
            // Green: the fills of the two double lines. Brown: the main line of the line with a second line. Black: the
            // polyline, the closed line and the Bezier curve, but not the line of zero length; the shortened line and the
            // two pointy ends of the long line, and the two pointy ends alone of the short one; the second line; and the
            // borders of the double lines, the left border of the last one in two parts.
            CollectionAssert.AreEqual(new int[] { 2, 1, 14 }, instances.Select(instance => instance.ClippingObjects.Count));
        }

        // A dashed line is a composed dashed line: each run of dashes that repeats a pattern is one dashed stroke, and
        // a dash that the rule of the vertices changes is a single dash. The line of 12 mm of a euclidean map has dashes
        // of 2 mm with gaps of 0.5 mm; its third dash starts 0.05 mm before a vertex, nearer than 0.4 times the width of
        // the line of 0.35 mm, so it leaves the vertex out and goes straight to its end. Its first two dashes and its
        // last two are runs.
        [Test]
        public void DashedLineIsAComposedDashedLine()
        {
            Map map = CreateMap();
            LineSymbol line;
            using (map.Write()) {
                map.UseEuclideanMetric = true;
                SymColor black = map.AddColor("Black", 1, 0, 0, 0, 1, false);
                LineSymDef dashed = new LineSymDef("Dashed", "1", black, 0.35F, LineJoinMode.Miter, LineCapMode.Flat);
                dashed.SetDashInfo(new LineSymDef.DashInfo() { dashLength = 2, firstDashLength = 2, lastDashLength = 2, gapLength = 0.5F });
                AddLineSymDef(map, dashed);
                double angle = Math.PI / 3;
                line = AddLine(map, dashed, new SymPath(new PointF[] {
                    new PointF(0, 0), new PointF(5.05F, 0), new PointF(5.05F + (float)(6.95 * Math.Cos(angle)), (float)(6.95 * Math.Sin(angle))) }));
            }

            IReadOnlyList<LayerInstance> instances = CreateInstances(map);

            AssertSameStructure(CollectInstances(map), instances);
            IReadOnlyList<ClippingObject> clippingObjects = instances.Single().ClippingObjects;
            Assert.AreEqual(3, clippingObjects.Count);
            StrokedShape[] shapes = clippingObjects.Cast<StrokedShape>().ToArray();
            CollectionAssert.AreEqual(new int[] { 2, 0, 2 }, shapes.Select(shape => (shape.Stroke.DashPattern == null) ? 0 : shape.Stroke.DashPattern.Count));
            CollectionAssert.AreEqual(new int[] { 2, 1, 2 }, shapes.Select(shape => shape.ExpandDashes().Count));
            Assert.AreEqual(2, ((PathShape)shapes[1].Shape).Parts.Sum(part => part.Points.Length), "the single dash leaves its vertex out");
        }

        // A line whose definition has glyphs is left to the collector: the geometry method of lines adds nothing for it.
        [Test]
        public void LinesWithGlyphsAreLeftToTheCollector()
        {
            Map map = CreateMap();
            LineSymbol line;
            using (map.Write()) {
                SymColor black = map.AddColor("Black", 1, 0, 0, 0, 1, false);
                LineSymDef glyphs = new LineSymDef("Glyphs", "1", black, 0.35F, LineJoinMode.Miter, LineCapMode.Flat);
                glyphs.SetGlyphs(new LineSymDef.GlyphInfo[] {
                    new LineSymDef.GlyphInfo() { glyph = CreateGlyph(glyph => glyph.AddFilledCircle(black, new PointF(0, 0), 0.5F)), location = LineSymDef.GlyphLocation.Start, number = 1 } });
                AddLineSymDef(map, glyphs);
                line = AddLine(map, glyphs, new SymPath(new PointF[] { new PointF(0, 5), new PointF(10, 5) }));
            }

            using (map.Read())
                Assert.IsFalse(line.TryAddClippingObjects(new LayerInstanceBuilder(map.AllColors)));
            AssertSameStructure(CollectInstances(map), CreateInstances(map));
        }

        // Add a line symbol definition to a map, and return it.
        static LineSymDef AddLineSymDef(Map map, LineSymDef symdef)
        {
            map.AddSymdef(symdef);
            return symdef;
        }

        // Add a line of a definition along a path to a map, and return it.
        static LineSymbol AddLine(Map map, LineSymDef symdef, SymPath path)
        {
            LineSymbol line = new LineSymbol(symdef, path);
            map.AddSymbol(line);
            return line;
        }

        // The names of the map files of the Skia rendering tests: each OCAD and Mapper file in skia_render.
        static IEnumerable<string> FindMapFilesOfTheRenderingTests()
        {
            return Directory.GetFiles(TestUtil.GetTestFile("skia_render"))
                            .Where(file => file.EndsWith(".ocd", StringComparison.OrdinalIgnoreCase) || file.EndsWith(".omap", StringComparison.OrdinalIgnoreCase))
                            .Select(Path.GetFileName)
                            .OrderBy(name => name, StringComparer.Ordinal);
        }

        // An empty map that measures text with Skia.
        static Map CreateMap()
        {
            return new Map(new Skia_TextMetrics(), null);
        }

        // Read a map of the Skia rendering tests.
        // mapFileName: the name of the map file in skia_render.
        static Map ReadMap(string mapFileName)
        {
            string directory = Path.GetDirectoryName(TestUtil.GetTestFile(Path.Combine("skia_render", mapFileName)));
            Map map = new Map(new Skia_TextMetrics(), new Skia_FileLoader(directory));
            InputOutput.ReadFile(Path.Combine(directory, mapFileName), map);
            return map;
        }

        // The instances that pass 1 makes of a map.
        static IReadOnlyList<LayerInstance> CreateInstances(Map map)
        {
            using (map.Read())
                return map.CreateLayerInstances();
        }

        // The instances that the collector makes of a map, the oracle.
        static IReadOnlyList<LayerInstance> CollectInstances(Map map)
        {
            using (map.Read())
                return map.CollectLayerInstances();
        }

        // A glyph whose parts an action adds.
        static Glyph CreateGlyph(Action<Glyph> addParts)
        {
            Glyph glyph = new Glyph();
            addParts(glyph);
            glyph.ConstructionComplete();
            return glyph;
        }

        // Add a point symbol definition of a glyph to a map, and a symbol of it with each of the given gaps, at its own
        // location and angle.
        static void AddPointSymbols(Map map, Glyph glyph, params float[][] gapsOfSymbols)
        {
            PointSymDef symdef = new PointSymDef("Point " + (map.AllSymdefs.Count + 1), (map.AllSymdefs.Count + 1).ToString(), glyph, true);
            map.AddSymdef(symdef);
            for (int i = 0; i < gapsOfSymbols.Length; ++i)
                map.AddSymbol(new PointSymbol(symdef, new PointF(3 * i, 2 * map.AllSymdefs.Count), 20 * i, gapsOfSymbols[i]));
        }

        // The closed path of a square.
        static SymPath CreateSquarePath(float left, float bottom, float size)
        {
            return new SymPath(new PointF[] {
                new PointF(left, bottom), new PointF(left + size, bottom), new PointF(left + size, bottom + size),
                new PointF(left, bottom + size), new PointF(left, bottom) });
        }

        // Assert that two lists of instances have the same structure.
        static void AssertSameStructure(IReadOnlyList<LayerInstance> expected, IReadOnlyList<LayerInstance> actual)
        {
            List<string> differences = LayerInstanceComparison.FindDifferences(expected, actual);
            Assert.IsEmpty(differences, "{0} differences from the instances of the collector, the first ones:\n{1}", differences.Count, string.Join("\n", differences.Take(10)));
        }

        static void AssertBounds(RectangleF expected, RectangleF actual)
        {
            Assert.AreEqual(expected.Left, actual.Left, Tolerance, "left of {0}", actual);
            Assert.AreEqual(expected.Top, actual.Top, Tolerance, "top of {0}", actual);
            Assert.AreEqual(expected.Right, actual.Right, Tolerance, "right of {0}", actual);
            Assert.AreEqual(expected.Bottom, actual.Bottom, Tolerance, "bottom of {0}", actual);
        }

        const float Tolerance = 0.0001F;
    }
}
