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
    // The geometry methods of the symbol definitions (SymDef.MakeClippingObjects), with which pass 1 makes the clipping
    // objects of the palette color coverages of the symbols whose definition has one, and the collector those of the
    // others. The clipping objects that the collector makes of each palette color coverage (SymDef.CollectClippingObjects)
    // are the oracle: the geometry methods make clipping objects of the same structure (SymbolCoverageComparison). The
    // other tests check what the oracle does not show: the point symbols of a definition share the shapes of its glyph.
    [TestFixture, Parallelizable(ParallelScope.Children)]
    public class SymbolGeometryTests
    {
        const float Tolerance = 0.0001F;

        // Let RenderingTests add the Roboto fonts, which the text symbols use, before the tests of the fixture, so that
        // both sides of a comparison measure the texts in the same fonts.
        [OneTimeSetUp]
        public void AddFontsOfTheRenderingTests()
        {
            System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(RenderingTests).TypeHandle);
        }

        // Each map of the Skia rendering tests.
        [TestCaseSource(nameof(FindMapFilesOfTheRenderingTests))]
        public void GeometryMethodsMakeTheClippingObjectsOfTheCollector(string mapFileName)
        {
            Map map = ReadMap(mapFileName);
            using (map.Read()) {
                List<SymbolCoverage> coverages = map.PaletteSeparations.SelectMany(separation => separation.SymbolCoverages).ToList();
                TestContext.WriteLine("{0} point symbols, {1} palette color coverages, {2} of point symbols", map.AllSymbols.OfType<PointSymbol>().Count(), coverages.Count, coverages.Count(coverage => coverage.Symbol is PointSymbol));
                AssertSameStructureAsTheCollector(coverages);
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

            List<ClippingObject>[] clippingObjects = GetClippingObjectsBySeparation(map);

            CollectionAssert.AreEqual(new int[] { 3, 3, 6 }, clippingObjects.Select(objects => objects.Count));
            // Each part of the glyph is one shape, which each symbol places with its placement.
            ClippingObject[][] partsOfSymbols = Enumerable.Range(0, 3).Select(symbol => new ClippingObject[] {
                clippingObjects[0][symbol], clippingObjects[1][symbol],
                clippingObjects[2][2 * symbol], clippingObjects[2][2 * symbol + 1] }).ToArray();
            for (int part = 0; part < 4; ++part) {
                Assert.AreSame(partsOfSymbols[0][part].Shape, partsOfSymbols[1][part].Shape, "part {0}", part);
                Assert.AreSame(partsOfSymbols[0][part].Shape, partsOfSymbols[2][part].Shape, "part {0}", part);
            }
            // The parts of a symbol in one palette color share its placement, of one step, and the symbols have
            // placements of their own.
            foreach (ClippingObject[] partsOfSymbol in partsOfSymbols) {
                Assert.AreEqual(1, partsOfSymbol[0].Placement.Steps.Count);
                Assert.AreSame(partsOfSymbol[2].Placement, partsOfSymbol[3].Placement);
                AssertEqualPlacements(partsOfSymbol[0].Placement, partsOfSymbol[1].Placement);
                AssertEqualPlacements(partsOfSymbol[0].Placement, partsOfSymbol[2].Placement);
            }
            CollectionAssert.AreNotEqual(partsOfSymbols[0][0].Placement.Matrix.Elements, partsOfSymbols[2][0].Placement.Matrix.Elements);
            AssertBounds(RectangleF.FromLTRB(4.5F, 2.5F, 5.5F, 3.5F), partsOfSymbols[2][1].Bounds);

            // The collector makes a shape of each drawing call. Its clipping objects have the same structure.
            using (map.Read()) {
                IReadOnlyList<SymbolCoverage> lines = map.PaletteSeparations[0].SymbolCoverages;
                IReadOnlyList<ClippingObject> firstLine = lines[0].Symbol.Definition.CollectClippingObjects(lines[0].Symbol, brown);
                IReadOnlyList<ClippingObject> secondLine = lines[1].Symbol.Definition.CollectClippingObjects(lines[1].Symbol, brown);
                Assert.AreNotSame(firstLine.Single().Shape, secondLine.Single().Shape);
            }
            AssertSameStructureAsTheCollector(map);
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

            List<ClippingObject> rings = GetClippingObjectsBySeparation(map).Single();

            // Each symbol has its own circle, at its location, without a transformation.
            Assert.AreEqual(2, rings.Count);
            Assert.AreNotSame(rings[0].Shape, rings[1].Shape);
            CollectionAssert.AreEqual(new PointF[] { new PointF(5, 3), new PointF(-2, 8) }, rings.Select(ring => ((EllipseShape)ring.Shape).Center));
            foreach (ClippingObject ring in rings) {
                Assert.AreEqual(0, ring.Placement.Steps.Count);
                Assert.AreEqual(0.95F, ((EllipseShape)ring.Shape).RadiusX, Tolerance);
            }
            AssertSameStructureAsTheCollector(map);
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

            List<ClippingObject> clippingObjects = GetClippingObjectsBySeparation(map).Single();

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
            // The arcs have the pen of the shared ring.
            Assert.AreSame(((StrokedShape)clippingObjects[6]).Stroke, ((StrokedShape)clippingObjects[0]).Stroke);
            Assert.AreEqual(0.95F, arcs[0].Radius, Tolerance);
            AssertSameStructureAsTheCollector(map);
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

            AssertSameStructureAsTheCollector(map);
            // Green: two dots and two areas with a hole. Brown: two whole rings, an arc that three gap angles leave, an
            // arc around 0 degrees, and two lines with Bezier curves, one ring and one arc of the last glyph. Black: dots
            // of the circles whose line is at least as wide as their diameter, two of the first glyph, one of the second
            // and one of each of the two last glyphs, and two closed lines.
            CollectionAssert.AreEqual(new int[] { 4, 8, 7 }, GetClippingObjectsBySeparation(map).Select(objects => objects.Count));
        }

        [Test]
        public void SharedShapesOfAGlyphStayOverTheChangesOfTheMap()
        {
            Map map = CreateMap();
            PointSymDef symdef;
            using (map.Write()) {
                SymColor black = map.AddColor("Black", 1, 0, 0, 0, 1, false);
                Glyph glyph = new Glyph();
                glyph.AddLine(black, new SymPath(new PointF[] { new PointF(-1, 0), new PointF(1, 0) }), 0.2F, LineJoinMode.Round, LineCapMode.Round);
                glyph.ConstructionComplete();
                symdef = new PointSymDef("Cliff", "1", glyph, true);
                map.AddSymdef(symdef);
                map.AddSymbol(new PointSymbol(symdef, new PointF(5, 3), 0, null));
            }

            Shape shape = GetClippingObjectsBySeparation(map).Single().Single().Shape;

            // The shared shapes depend on the glyph alone, so a change of the map, which makes the palette color
            // coverages again, does not make them again.
            using (map.Write())
                map.AddSymbol(new PointSymbol(symdef, new PointF(-2, 8), 30, null));

            List<ClippingObject> clippingObjects = GetClippingObjectsBySeparation(map).Single();
            Assert.AreEqual(2, clippingObjects.Count);
            foreach (ClippingObject clippingObject in clippingObjects)
                Assert.AreSame(shape, clippingObject.Shape);
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

        // The clipping objects of the palette color coverages of each palette separation of a map, in palette order.
        static List<ClippingObject>[] GetClippingObjectsBySeparation(Map map)
        {
            using (map.Read()) {
                return map.PaletteSeparations
                    .Select(separation => separation.SymbolCoverages.SelectMany(coverage => coverage.ClippingObjects).ToList())
                    .ToArray();
            }
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

        // Assert that the clipping objects of each palette color coverage of a map have the structure of the clipping
        // objects that the collector makes of it.
        static void AssertSameStructureAsTheCollector(Map map)
        {
            using (map.Read())
                AssertSameStructureAsTheCollector(map.PaletteSeparations.SelectMany(separation => separation.SymbolCoverages).ToList());
        }

        // Assert that the clipping objects of palette color coverages have the structure of the clipping objects that the
        // collector makes of them. The map of the coverages is read.
        static void AssertSameStructureAsTheCollector(List<SymbolCoverage> coverages)
        {
            List<string> differences = new List<string>();
            foreach (SymbolCoverage coverage in coverages) {
                IReadOnlyList<ClippingObject> expected = coverage.Symbol.Definition.CollectClippingObjects(coverage.Symbol, coverage.Color);
                differences.AddRange(SymbolCoverageComparison.FindDifferences(coverage, expected, coverage.ClippingObjects));
            }

            Assert.IsEmpty(differences, "{0} differences from the clipping objects of the collector, the first ones:\n{1}", differences.Count, string.Join("\n", differences.Take(10)));
        }

        // Assert that two placements have the same steps.
        static void AssertEqualPlacements(Placement expected, Placement actual)
        {
            Assert.AreEqual(expected.Steps.Count, actual.Steps.Count, "steps");
            for (int i = 0; i < expected.Steps.Count; ++i)
                CollectionAssert.AreEqual(expected.Steps[i].Elements, actual.Steps[i].Elements, "step {0}", i);
        }

        static void AssertBounds(RectangleF expected, RectangleF actual)
        {
            Assert.AreEqual(expected.Left, actual.Left, Tolerance, "left of {0}", actual);
            Assert.AreEqual(expected.Top, actual.Top, Tolerance, "top of {0}", actual);
            Assert.AreEqual(expected.Right, actual.Right, Tolerance, "right of {0}", actual);
            Assert.AreEqual(expected.Bottom, actual.Bottom, Tolerance, "bottom of {0}", actual);
        }
    }
}
