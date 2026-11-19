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
    // The geometry methods of the symbols (Symbol.AddClippingObjects), with which pass 1 (Map.CreateLayerInstances)
    // makes the clipping objects of all the symbols. The instances that the collector makes of all the symbols
    // (Map.CollectLayerInstances) are the oracle: pass 1 makes instances of the same structure
    // (LayerInstanceComparison). The other tests check what the oracle does not show: the point symbols of a definition
    // share the shapes of its glyph.
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
            using (map.Write()) {
                SymColor green = map.AddColor("Green", 1, 0.76F, 0, 0.91F, 0, false);
                SymColor brown = map.AddColor("Brown", 2, 0, 0.56F, 1, 0.18F, false);
                SymColor black = map.AddColor("Black", 3, 0, 0, 0, 1, false);

                LineSymDef main = AddLineSymDef(map, new LineSymDef("Main", "1", black, 0.35F, LineJoinMode.Miter, LineCapMode.Flat));
                AddLine(map, main, new SymPath(new PointF[] { new PointF(0, 0), new PointF(4, 1), new PointF(6, -1) }));
                AddLine(map, main, CreateSquarePath(8, 0, 2));
                AddLine(map, main, new SymPath(new PointF[] { new PointF(12, 0), new PointF(13, 2), new PointF(15, 2), new PointF(16, 0) },
                                               new PointKind[] { PointKind.Normal, PointKind.BezierControl, PointKind.BezierControl, PointKind.Normal }));
                AddLine(map, main, new SymPath(new PointF[] { new PointF(18, 0), new PointF(18, 0) }));

                LineSymDef pointy = new LineSymDef("Pointy", "2", black, 0.5F, LineJoinMode.Round, LineCapMode.Round);
                pointy.SetShortening(new LineSymDef.ShortenInfo() { shortenBeginning = 1.0F, shortenEnd = 0.5F, pointyEnds = true });
                AddLineSymDef(map, pointy);
                AddLine(map, pointy, new SymPath(new PointF[] { new PointF(0, 5), new PointF(3, 6), new PointF(6, 5) }));
                AddLine(map, pointy, new SymPath(new PointF[] { new PointF(8, 5), new PointF(9, 5) }));

                LineSymDef second = new LineSymDef("Second", "3", brown, 0.6F, LineJoinMode.Bevel, LineCapMode.Round);
                second.SetSecondLine(black, 0.2F, LineJoinMode.Bevel, LineCapMode.Square);
                AddLineSymDef(map, second);
                AddLine(map, second, new SymPath(new PointF[] { new PointF(0, 10), new PointF(3, 12), new PointF(6, 10) }));

                LineSymDef doubled = new LineSymDef("Double", "4", null, 0, LineJoinMode.Miter, LineCapMode.Flat);
                doubled.SetDoubleLines(new LineSymDef.DoubleLineInfo() {
                    doubleThick = 0.8F, doubleFillColor = green, doubleLeftColor = black, doubleLeftWidth = 0.1F, doubleRightColor = black, doubleRightWidth = 0.15F });
                AddLineSymDef(map, doubled);
                AddLine(map, doubled, new SymPath(new PointF[] { new PointF(0, 15), new PointF(3, 18), new PointF(6, 15) }));
                AddLine(map, doubled, new SymPath(new PointF[] { new PointF(8, 15), new PointF(10, 16), new PointF(12, 16), new PointF(14, 15) },
                                                  new PointKind[] { PointKind.Normal, PointKind.Normal, PointKind.Normal, PointKind.Normal },
                                                  new byte[] { 0, SymPath.DOUBLE_LEFT_STARTSTOPFLAG, 0 }));
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

        // On a Bezier curve of a euclidean map, the composed dashed line has a run for each quarter of the curve, as the
        // dashes are placed by quarters (fix 021): the curve of the ninth dash sample whose bend is near its end has four
        // runs. The dashes and the gaps of a run are those that the measure of the quarters fits, scaled by the length of
        // the flattened quarter against its chord. So the ratio of a dash to its gap is the same in each run, and the gaps
        // get longer along the curve, as its quarters bend more.
        [Test]
        public void DashedLineOnABezierCurveHasARunForEachQuarter()
        {
            Map map = CreateMap();
            using (map.Write()) {
                map.UseEuclideanMetric = true;
                SymColor black = map.AddColor("Black", 1, 0, 0, 0, 1, false);
                LineSymDef dashed = new LineSymDef("Dashed", "1", black, 0.05F, LineJoinMode.Miter, LineCapMode.Flat);
                dashed.SetDashInfo(new LineSymDef.DashInfo() { dashLength = 0.5F, firstDashLength = 0.5F, lastDashLength = 0.5F, gapLength = 0.25F });
                AddLineSymDef(map, dashed);
                AddLine(map, dashed, new SymPath(new PointF[] { new PointF(4, -82), new PointF(24, -82), new PointF(29, -82), new PointF(29, -77) },
                                                 new PointKind[] { PointKind.Normal, PointKind.BezierControl, PointKind.BezierControl, PointKind.Normal }));
            }

            IReadOnlyList<LayerInstance> instances = CreateInstances(map);

            AssertSameStructure(CollectInstances(map), instances);
            List<IReadOnlyList<float>> patterns = instances.Single().ClippingObjects.Cast<StrokedShape>()
                                                           .Where(shape => shape.Stroke.DashPattern != null)
                                                           .Select(shape => shape.Stroke.DashPattern)
                                                           .ToList();
            Assert.AreEqual(4, patterns.Count, "runs");
            foreach (IReadOnlyList<float> pattern in patterns) {
                Assert.AreEqual(2, pattern.Count);
                Assert.AreEqual(patterns[0][0] / patterns[0][1], pattern[0] / pattern[1], Tolerance, "the ratio of a dash to its gap");
            }
            for (int i = 1; i < patterns.Count; ++i)
                Assert.Greater(patterns[i][1], patterns[i - 1][1], "the gap of run {0}", i);
        }

        // The branches of the placement of the glyphs along a line, each of which the geometry method of lines follows:
        // each location of the glyphs, in a color of its own. The lines of the glyphs at the corners and the dash points
        // have the corners and the dash point of CreatePathWithCornersAndDashPoints. The other lines are shortened, so
        // that the glyphs at the start and the end are on the whole line and the others on the shortened one, and the
        // glyphs that are spaced along them are in pairs.
        [Test]
        public void GeometryMethodOfLinesFollowsEachBranchOfThePlacementOfTheirGlyphs()
        {
            Map map = CreateMap();
            LineSymDef.GlyphLocation[] locations = (LineSymDef.GlyphLocation[])Enum.GetValues(typeof(LineSymDef.GlyphLocation));
            using (map.Write()) {
                SymColor black = map.AddColor("Black", 1, 0, 0, 0, 1, false);
                foreach (LineSymDef.GlyphLocation location in locations) {
                    int index = map.AllSymdefs.Count;
                    SymColor color = map.AddColor(location.ToString(), (short)(index + 2), 0.05F * index, 0.5F, 0, 0, false);
                    bool isAtPointsOfThePath = location == LineSymDef.GlyphLocation.Corners || location == LineSymDef.GlyphLocation.CornersIgnoreEnds ||
                                               location == LineSymDef.GlyphLocation.DashPoints || location == LineSymDef.GlyphLocation.DashPointsIgnoreEnds;
                    bool isSpaced = location == LineSymDef.GlyphLocation.Spaced || location == LineSymDef.GlyphLocation.SpacedOffset || location == LineSymDef.GlyphLocation.SpacedDecrease;

                    LineSymDef symdef = new LineSymDef(location.ToString(), (index + 1).ToString(), black, 0.2F, LineJoinMode.Miter, LineCapMode.Flat);
                    if (!isAtPointsOfThePath)
                        symdef.SetShortening(new LineSymDef.ShortenInfo() { shortenBeginning = 0.5F, shortenEnd = 0.7F });
                    if (location == LineSymDef.GlyphLocation.DashCenters || location == LineSymDef.GlyphLocation.GapCenters)
                        symdef.SetDashInfo(new LineSymDef.DashInfo() { dashLength = 2, firstDashLength = 2, lastDashLength = 2, gapLength = 0.5F });
                    symdef.SetGlyphs(new LineSymDef.GlyphInfo[] { new LineSymDef.GlyphInfo() {
                        glyph = CreateTick(color), location = location, distance = 2, firstDistance = 1, lastDistance = 1, minimum = 1,
                        number = isSpaced ? 2 : 1, spacing = 0.3F, offset = 0.5F, decreaseLimit = 0.4F, decreaseBothEnds = true } });
                    AddLineSymDef(map, symdef);
                    AddLine(map, symdef, CreatePathWithCornersAndDashPoints(5 * index));
                }
            }

            IReadOnlyList<LayerInstance> instances = CreateInstances(map);

            AssertSameStructure(CollectInstances(map), instances);
            // Each location places glyphs. The corners are the ends and the corner between them, and the dash point is
            // between the ends.
            Assert.AreEqual(1 + locations.Length, instances.Count);
            Dictionary<string, int> glyphCounts = instances.Skip(1).ToDictionary(instance => instance.PaletteEntry.Name, instance => instance.ClippingObjects.Count);
            TestContext.WriteLine(string.Join(", ", glyphCounts.Select(count => count.Key + " " + count.Value)));
            CollectionAssert.AreEqual(new int[] { 1, 1, 3, 1, 1, 1 }, new string[] { "Start", "End", "Corners", "CornersIgnoreEnds", "DashPoints", "DashPointsIgnoreEnds" }.Select(name => glyphCounts[name]));
        }

        // The glyphs along a line share the shapes of their glyph, as the point symbols of a definition do, unless the
        // line stretches them at a corner or decreases them: those have shapes of their own, in which the transformation
        // moves the points but keeps the width of the line. A glyph that the line places with a transformation that is
        // the identity shares the shapes: the glyphs at the ends of a line of corners, the decreasing glyphs of full
        // size, and the glyph of one circle at its origin at the dash point, which the drawing then places with its
        // parts. Two lines of one definition share the shapes too.
        [Test]
        public void GlyphsAlongALineShareTheShapesOfTheirGlyphUnlessTheLineStretchesOrDecreasesThem()
        {
            Map map = CreateMap();
            SymColor black, brown, blue, green, purple;
            using (map.Write()) {
                black = map.AddColor("Black", 1, 0, 0, 0, 1, false);
                brown = map.AddColor("Brown", 2, 0, 0.56F, 1, 0.18F, false);
                blue = map.AddColor("Blue", 3, 1, 0, 0, 0, false);
                green = map.AddColor("Green", 4, 0.76F, 0, 0.91F, 0, false);
                purple = map.AddColor("Purple", 5, 0.35F, 0.85F, 0, 0, false);
                LineSymDef symdef = new LineSymDef("Glyphs", "1", black, 0.2F, LineJoinMode.Miter, LineCapMode.Flat);
                symdef.SetGlyphs(new LineSymDef.GlyphInfo[] {
                    new LineSymDef.GlyphInfo() { glyph = CreateTick(brown), location = LineSymDef.GlyphLocation.Corners, number = 1 },
                    new LineSymDef.GlyphInfo() { glyph = CreateTick(blue), location = LineSymDef.GlyphLocation.Spaced, distance = 2, firstDistance = 1, lastDistance = 1, number = 1 },
                    new LineSymDef.GlyphInfo() { glyph = CreateTick(green), location = LineSymDef.GlyphLocation.SpacedDecrease, distance = 2, firstDistance = 1, lastDistance = 1, number = 1, decreaseLimit = 0.4F },
                    new LineSymDef.GlyphInfo() { glyph = CreateGlyph(glyph => glyph.AddFilledCircle(purple, new PointF(0, 0), 0.5F)), location = LineSymDef.GlyphLocation.DashPoints, number = 1 } });
                AddLineSymDef(map, symdef);
                AddLine(map, symdef, CreatePathWithCornersAndDashPoints(0));
                AddLine(map, symdef, CreatePathWithCornersAndDashPoints(6));
            }

            IReadOnlyList<LayerInstance> instances = CreateInstances(map);

            AssertSameStructure(CollectInstances(map), instances);
            CollectionAssert.AreEqual(new SymColor[] { black, brown, blue, green, purple }, instances.Select(instance => instance.PaletteEntry));

            // The corner between the ends turns the line by 45 degrees, and stretches its glyph by the miter factor of the
            // angle of 135 degrees, but not the width of its line. The glyphs at the ends share the shapes.
            IReadOnlyList<ClippingObject> corners = instances[1].ClippingObjects;
            Assert.AreEqual(6, corners.Count);
            foreach (int end in new int[] { 2, 3, 5 })
                Assert.AreSame(corners[0].Shape, corners[end].Shape, "corner {0}", end);
            foreach (int stretched in new int[] { 1, 4 }) {
                Assert.AreNotSame(corners[0].Shape, corners[stretched].Shape, "corner {0}", stretched);
                Assert.AreEqual(0.8F / Math.Sin(67.5 * Math.PI / 180), MeasureTick(corners[stretched]), Tolerance, "corner {0}", stretched);
                Assert.AreEqual(0.1F, ((StrokedShape)corners[stretched]).Stroke.Width, "corner {0}", stretched);
            }
            Assert.AreNotSame(corners[1].Shape, corners[4].Shape);

            // The spaced glyphs share the shapes, also those that repeat along a part.
            List<ClippingObject> spaced = Expand(instances[2].ClippingObjects);
            Assert.Greater(spaced.Count, 4);
            foreach (ClippingObject glyph in spaced)
                Assert.AreSame(spaced[0].Shape, glyph.Shape);

            // The decreasing glyphs of full size share the shapes, and each smaller glyph has shapes of its own.
            ClippingObject[] fullSize = instances[3].ClippingObjects.Where(glyph => Math.Abs(MeasureTick(glyph) - 0.8F) < Tolerance).ToArray();
            ClippingObject[] smaller = instances[3].ClippingObjects.Except(fullSize).ToArray();
            TestContext.WriteLine("{0} decreasing glyphs of full size, {1} smaller", fullSize.Length, smaller.Length);
            Assert.Greater(fullSize.Length, 1);
            Assert.Greater(smaller.Length, 1);
            foreach (ClippingObject glyph in fullSize)
                Assert.AreSame(fullSize[0].Shape, glyph.Shape);
            foreach (ClippingObject glyph in smaller)
                Assert.Less(MeasureTick(glyph), 0.8F - Tolerance);
            Assert.AreEqual(smaller.Length, smaller.Select(glyph => glyph.Shape).Concat(new Shape[] { fullSize[0].Shape }).Distinct().Count() - 1);

            // The glyph of one circle at its origin is placed with its parts at each dash point, and shares its shape.
            IReadOnlyList<ClippingObject> dots = instances[4].ClippingObjects;
            Assert.AreEqual(2, dots.Count);
            Assert.AreSame(dots[0].Shape, dots[1].Shape);
            Assert.AreEqual(new PointF(0, 0), ((EllipseShape)dots[0].Shape).Center);
            foreach (ClippingObject dot in dots)
                Assert.AreEqual(1, dot.Placement.Steps.Count);
        }

        // The evenly spaced glyphs of a part of a line are one repetition of the shared shapes of the glyph, along the
        // part from the first glyph to the last, with the metric of the map: the glyphs spaced 2 mm along a straight line
        // of 40 mm, 20 of them by the euclidean metric, turned by 30 degrees so that the OCAD metric measures the line
        // longer. The repetition turns the glyphs to the direction of the line, but not a glyph of one circle at its
        // origin, which the drawing draws without a transformation. At the centers of the dashes of a dashed line whose
        // end dashes are shorter, the glyphs of the end dashes are single placements, and the others repeat.
        [TestCase(true)]
        [TestCase(false)]
        public void EvenlySpacedGlyphsOfAPartAreOneRepetition(bool euclidean)
        {
            Map map = CreateMap();
            SymColor black, brown, blue, green;
            using (map.Write()) {
                map.UseEuclideanMetric = euclidean;
                black = map.AddColor("Black", 1, 0, 0, 0, 1, false);
                brown = map.AddColor("Brown", 2, 0, 0.56F, 1, 0.18F, false);
                blue = map.AddColor("Blue", 3, 1, 0, 0, 0, false);
                green = map.AddColor("Green", 4, 0.76F, 0, 0.91F, 0, false);
                double angle = Math.PI / 6;
                PointF end = new PointF((float)(40 * Math.Cos(angle)), (float)(40 * Math.Sin(angle)));

                LineSymDef spaced = new LineSymDef("Spaced", "1", black, 0.1F, LineJoinMode.Miter, LineCapMode.Flat);
                spaced.SetGlyphs(new LineSymDef.GlyphInfo[] {
                    new LineSymDef.GlyphInfo() { glyph = CreateTick(brown), location = LineSymDef.GlyphLocation.Spaced, distance = 2, firstDistance = 1, lastDistance = 1, number = 1 } });
                AddLineSymDef(map, spaced);
                AddLine(map, spaced, new SymPath(new PointF[] { new PointF(0, 0), end }));

                LineSymDef dashed = new LineSymDef("Dashed", "2", black, 0.2F, LineJoinMode.Miter, LineCapMode.Flat);
                dashed.SetDashInfo(new LineSymDef.DashInfo() { dashLength = 2, firstDashLength = 1, lastDashLength = 1, gapLength = 0.5F });
                dashed.SetGlyphs(new LineSymDef.GlyphInfo[] {
                    new LineSymDef.GlyphInfo() { glyph = CreateTick(blue), location = LineSymDef.GlyphLocation.DashCenters, number = 1 } });
                AddLineSymDef(map, dashed);
                AddLine(map, dashed, new SymPath(new PointF[] { new PointF(0, 10), new PointF(end.X, end.Y + 10) }));

                LineSymDef dotted = new LineSymDef("Dotted", "3", black, 0.1F, LineJoinMode.Miter, LineCapMode.Flat);
                dotted.SetGlyphs(new LineSymDef.GlyphInfo[] {
                    new LineSymDef.GlyphInfo() { glyph = CreateGlyph(glyph => glyph.AddFilledCircle(green, new PointF(0, 0), 0.4F)), location = LineSymDef.GlyphLocation.Spaced, distance = 2, firstDistance = 1, lastDistance = 1, number = 1 } });
                AddLineSymDef(map, dotted);
                AddLine(map, dotted, new SymPath(new PointF[] { new PointF(0, 20), new PointF(end.X, end.Y + 20) }));
            }

            IReadOnlyList<LayerInstance> instances = CreateInstances(map);

            AssertSameStructure(CollectInstances(map), instances);
            CollectionAssert.AreEqual(new SymColor[] { black, brown, blue, green }, instances.Select(instance => instance.PaletteEntry));
            RepeatedShapes repetition = (RepeatedShapes)instances[1].ClippingObjects.Single();
            Assert.AreEqual(CollectInstances(map)[1].ClippingObjects.Count, repetition.Count);
            Assert.AreEqual(euclidean, repetition.IsEuclidean);
            if (euclidean) {
                Assert.AreEqual(20, repetition.Count);
                Assert.AreEqual(2.0F, repetition.Step, Tolerance);
            }
            Assert.IsTrue(repetition.IsTurnedToPath);
            ClippingObject element = repetition.Elements.Single();
            foreach (ClippingObject placed in repetition.Expand())
                Assert.AreSame(element.Shape, placed.Shape);

            RepeatedShapes dots = (RepeatedShapes)instances[3].ClippingObjects.Single();
            Assert.IsFalse(dots.IsTurnedToPath);
            foreach (ClippingObject placed in dots.Expand())
                CollectionAssert.AreEqual(new float[] { 1, 0, 0, 1 }, placed.Placement.Matrix.Elements.Take(4));

            IReadOnlyList<ClippingObject> centers = instances[2].ClippingObjects;
            Assert.AreEqual(3, centers.Count);
            Assert.IsNotInstanceOf<RepeatedShapes>(centers[0]);
            Assert.IsInstanceOf<RepeatedShapes>(centers[1]);
            Assert.IsNotInstanceOf<RepeatedShapes>(centers[2]);
            Assert.AreSame(centers[0].Shape, ((RepeatedShapes)centers[1]).Elements.Single().Shape);
        }

        // The clipping objects of a list, with each repetition expanded to its placed shapes.
        static List<ClippingObject> Expand(IReadOnlyList<ClippingObject> clippingObjects)
        {
            return clippingObjects.SelectMany(clippingObject => (clippingObject is RepeatedShapes repeated) ? repeated.Expand() : new ClippingObject[] { clippingObject }).ToList();
        }

        // A combined line (LineComboSymDef, of a Mapper map) has the clipping objects of its components, in the order of
        // the components, as its drawing draws them: in brown the wide line of the first component, and then the
        // glyphs of the second component at the centers of its gaps; in black the dashes of the second component.
        [Test]
        public void CombinedLineHasTheClippingObjectsOfItsComponents()
        {
            Map map = CreateMap();
            SymColor brown, black;
            using (map.Write()) {
                map.UseEuclideanMetric = true;
                brown = map.AddColor("Brown", 1, 0, 0.56F, 1, 0.18F, false);
                black = map.AddColor("Black", 2, 0, 0, 0, 1, false);
                LineSymDef wide = AddLineSymDef(map, new LineSymDef("Wide", "1", brown, 0.6F, LineJoinMode.Round, LineCapMode.Round));
                LineSymDef dashed = new LineSymDef("Dashed", "2", black, 0.2F, LineJoinMode.Miter, LineCapMode.Flat);
                dashed.SetDashInfo(new LineSymDef.DashInfo() { dashLength = 2, firstDashLength = 2, lastDashLength = 2, gapLength = 0.5F });
                dashed.SetGlyphs(new LineSymDef.GlyphInfo[] {
                    new LineSymDef.GlyphInfo() { glyph = CreateTick(brown), location = LineSymDef.GlyphLocation.GapCenters, minimum = 1, number = 1 } });
                AddLineSymDef(map, dashed);
                LineComboSymDef combined = new LineComboSymDef("Combined", "3", new LineSymDef[] { wide, dashed });
                map.AddSymdef(combined);
                map.AddSymbol(new LineSymbol(combined, new SymPath(new PointF[] { new PointF(0, 0), new PointF(6, 0), new PointF(10, 3) })));
            }

            IReadOnlyList<LayerInstance> instances = CreateInstances(map);

            AssertSameStructure(CollectInstances(map), instances);
            CollectionAssert.AreEqual(new SymColor[] { brown, black }, instances.Select(instance => instance.PaletteEntry));
            StrokedShape[] brownShapes = Expand(instances[0].ClippingObjects).Cast<StrokedShape>().ToArray();
            Assert.Greater(brownShapes.Length, 1);
            CollectionAssert.AreEqual(new float[] { 0.6F }.Concat(Enumerable.Repeat(0.1F, brownShapes.Length - 1)), brownShapes.Select(shape => shape.Stroke.Width));
            Assert.IsNotEmpty(instances[1].ClippingObjects);
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

        // The branches of the drawing of areas and graphics objects, each of which their geometry methods follow: an area
        // with a fill alone, with a border alone and with both, along a path with holes, along which the border goes
        // too, and along a path with a Bezier curve; a dashed border, and hatchings and a pattern, which pass 1 leaves
        // out; a combined area of a dashed line, an area and a line; and a graphics area and a graphics line.
        [Test]
        public void GeometryMethodOfAreasFollowsEachBranchOfTheirDrawing()
        {
            Map map = CreateMap();
            SymColor green, blue, brown, black;
            using (map.Write()) {
                green = map.AddColor("Green", 1, 0.76F, 0, 0.91F, 0, false);
                blue = map.AddColor("Blue", 2, 1, 0, 0, 0, false);
                brown = map.AddColor("Brown", 3, 0, 0.56F, 1, 0.18F, false);
                black = map.AddColor("Black", 4, 0, 0, 0, 1, false);

                LineSymDef border = AddLineSymDef(map, new LineSymDef("Border", "1", black, 0.2F, LineJoinMode.Miter, LineCapMode.Flat));
                LineSymDef dashedBorder = new LineSymDef("Dashed border", "2", brown, 0.15F, LineJoinMode.Miter, LineCapMode.Flat);
                dashedBorder.SetDashInfo(new LineSymDef.DashInfo() { dashLength = 1, firstDashLength = 1, lastDashLength = 1, gapLength = 0.3F });
                AddLineSymDef(map, dashedBorder);
                AreaSymDef fill = AddAreaSymDef(map, new AreaSymDef("Fill", "3", green, null));
                AreaSymDef outline = AddAreaSymDef(map, new AreaSymDef("Outline", "4", null, border));
                AreaSymDef filledAndOutlined = AddAreaSymDef(map, new AreaSymDef("Fill and outline", "5", green, border));
                AreaSymDef hatched = new AreaSymDef("Hatched", "6", blue, dashedBorder);
                hatched.AddHatching(new AreaSymDef.HatchInfo() { hatchColor = black, hatchWidth = 0.1F, hatchSpacing = 0.5F, hatchAngle = 45 });
                hatched.AddPattern(new AreaSymDef.PatternInfo() { patternGlyph = CreateGlyph(glyph => glyph.AddFilledCircle(brown, new PointF(0, 0), 0.3F)), patternWidth = 1, patternHeight = 1 });
                AddAreaSymDef(map, hatched);
                AreaComboSymDef combined = new AreaComboSymDef("Combined", "7", new SymDef[] { dashedBorder, fill, border });
                map.AddSymdef(combined);
                GraphicsSymDef graphics = new GraphicsSymDef();
                map.AddSymdef(graphics);

                SymPathWithHoles withHoles = new SymPathWithHoles(CreateSquarePath(6, 0, 4), new SymPath[] { CreateSquarePath(7, 1, 1), CreateSquarePath(8.5F, 2, 1) });
                SymPathWithHoles curved = new SymPathWithHoles(new SymPath(
                    new PointF[] { new PointF(0, 10), new PointF(1, 12), new PointF(3, 12), new PointF(4, 10), new PointF(4, 8), new PointF(0, 8), new PointF(0, 10) },
                    new PointKind[] { PointKind.Normal, PointKind.BezierControl, PointKind.BezierControl, PointKind.Normal, PointKind.Normal, PointKind.Normal, PointKind.Normal }), null);
                AddArea(map, fill, new SymPathWithHoles(CreateSquarePath(0, 0, 4), null));
                AddArea(map, outline, new SymPathWithHoles(CreateSquarePath(0, 5, 4), null));
                AddArea(map, filledAndOutlined, withHoles);
                AddArea(map, hatched, curved);
                AddArea(map, combined, new SymPathWithHoles(CreateSquarePath(12, 0, 4), null));
                map.AddSymbol(new GraphicsAreaSymbol(graphics, withHoles, brown));
                map.AddSymbol(new GraphicsLineSymbol(graphics, new SymPath(new PointF[] { new PointF(0, 15), new PointF(5, 16) }), blue, 0.3F, LineJoinMode.Round, LineCapMode.Round));
            }

            IReadOnlyList<LayerInstance> instances = CreateInstances(map);

            AssertSameStructure(CollectInstances(map), instances);
            // Green: the fills of the areas of a fill, and of the combined area. Blue: the fill of the hatched area, but
            // not its hatching, and the graphics line. Brown: the dashes of the border of the hatched area and of the
            // combined area, and the graphics area, but not the pattern. Black: the border of the outlined area, the
            // border of the area with holes along its path and its two holes, and the line of the combined area.
            CollectionAssert.AreEqual(new SymColor[] { green, blue, brown, black }, instances.Select(instance => instance.PaletteEntry));
            CollectionAssert.AreEqual(new int[] { 3, 2, 5 }, new int[] { 0, 1, 3 }.Select(index => instances[index].ClippingObjects.Count));
            Assert.Greater(instances[2].ClippingObjects.Count, 3);
            Assert.IsInstanceOf<FilledShape>(instances[2].ClippingObjects.Last());
        }

        // The line framing of a text run is one line, as OCAD draws it: in the color of the framing, the run filled, and
        // its outline stroked with the line of the full width, two times the width of the framing, with the joins of the
        // framing. The drawing strokes the outline with narrower pens first, of 0.33 times the font size and more, which
        // the collector has too (question 2 of ticket 013).
        [Test]
        public void LineFramingOfATextRunIsOneLineOfTheFullWidth()
        {
            Map map = CreateMap();
            SymColor yellow, black;
            using (map.Write()) {
                yellow = map.AddColor("Yellow", 1, 0, 0.27F, 0.79F, 0, false);
                black = map.AddColor("Black", 2, 0, 0, 0, 1, false);

                TextSymDef symdef = new TextSymDef("Framed", "1", TextSymDef.PreferredSymbolKind.NormalText, null);
                symdef.SetFont("Roboto", 2, TextEffects.Regular, black, 2, 0, 0, 0, null, 0, 1, TextSymDefHorizAlignment.Left, TextSymDefVertAlignment.TopAscent);
                symdef.SetFraming(new TextSymDef.Framing() { framingStyle = TextSymDef.FramingStyle.Line, framingColor = yellow, lineWidth = 1, lineStyle = LineStyle.Rounded });
                map.AddSymdef(symdef);
                map.AddSymbol(new TextSymbol(symdef, new string[] { "Two words" }, new PointF(0, 0), 0, 0, TextSymDefHorizAlignment.Default, TextSymDefVertAlignment.Default));
            }

            IReadOnlyList<LayerInstance> instances = CreateInstances(map);

            AssertSameStructure(CollectInstances(map), instances);
            CollectionAssert.AreEqual(new SymColor[] { yellow, black }, instances.Select(instance => instance.PaletteEntry));
            IReadOnlyList<ClippingObject> framing = instances[0].ClippingObjects;
            CollectionAssert.AreEqual(new string[] { "Two", "Two", "words", "words" }, framing.Select(clippingObject => ((TextShape)clippingObject.Shape).Text));
            for (int i = 0; i < framing.Count; i += 2) {
                Assert.IsInstanceOf<FilledShape>(framing[i]);
                Stroke line = ((StrokedShape)framing[i + 1]).Stroke;
                Assert.AreEqual(2, line.Width);
                Assert.AreEqual(LineJoinMode.Round, line.Join);
            }
            Assert.Greater(CollectInstances(map)[0].ClippingObjects.Count, framing.Count);
        }

        // Add an area symbol definition to a map, and return it.
        static AreaSymDef AddAreaSymDef(Map map, AreaSymDef symdef)
        {
            map.AddSymdef(symdef);
            return symdef;
        }

        // Add an area of a definition along a path with holes to a map, and return it.
        static AreaSymbol AddArea(Map map, AreaLikeSymDef symdef, SymPathWithHoles path)
        {
            AreaSymbol area = new AreaSymbol(symdef, path, 0, new PointF());
            map.AddSymbol(area);
            return area;
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

        // A glyph of a tick across a line: a line of 0.8 mm along the y axis of the glyph, 0.1 mm wide.
        static Glyph CreateTick(SymColor color)
        {
            return CreateGlyph(glyph => glyph.AddLine(color, new SymPath(new PointF[] { new PointF(0, -0.4F), new PointF(0, 0.4F) }), 0.1F, LineJoinMode.Miter, LineCapMode.Flat));
        }

        // The length of the line of a tick (CreateTick), as a clipping object gives it in the coordinates of its shape.
        static double MeasureTick(ClippingObject tick)
        {
            PointF[] points = ((PathShape)tick.Shape).Parts.SelectMany(part => part.Points).ToArray();
            Assert.AreEqual(2, points.Length);
            return Math.Sqrt((points[1].X - points[0].X) * (points[1].X - points[0].X) + (points[1].Y - points[0].Y) * (points[1].Y - points[0].Y));
        }

        // A path at a height y, with corners at its ends and at (3, y), where it turns up by 45 degrees, and a dash point
        // at (6, y + 3), where it turns back. After a normal point at (9, y + 3) it goes down to its end at (12, y).
        static SymPath CreatePathWithCornersAndDashPoints(float y)
        {
            return new SymPath(new PointF[] { new PointF(0, y), new PointF(3, y), new PointF(6, y + 3), new PointF(9, y + 3), new PointF(12, y) },
                               new PointKind[] { PointKind.Corner, PointKind.Corner, PointKind.Dash, PointKind.Normal, PointKind.Corner });
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
