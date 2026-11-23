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

        // Each map of the Skia rendering tests, and each sample of the dashed lines of OCAD.
        [TestCaseSource(nameof(FindMapFilesOfTheRenderingTests))]
        [TestCaseSource(nameof(FindDashSampleFiles))]
        public void GeometryMethodsMakeTheClippingObjectsOfTheCollector(string mapFileName)
        {
            Map map = ReadMap(mapFileName);
            using (map.Read()) {
                List<SymbolCoverage> coverages = map.PaletteSeparations.SelectMany(separation => separation.SymbolCoverages).ToList();
                TestContext.WriteLine("{0} point symbols, {1} palette color coverages, {2} of point symbols", map.AllSymbols.OfType<PointSymbol>().Count(), coverages.Count, coverages.Count(coverage => coverage.Symbol is PointSymbol));
                List<ClippingObject> ofLines = coverages.Where(coverage => coverage.Symbol is LineSymbol).SelectMany(coverage => coverage.ClippingObjects).ToList();
                TestContext.WriteLine("{0} line symbols, {1} palette color coverages of line symbols, {2} runs of dashes, {3} offset paths, {4} clipping objects at {5} repeated placements",
                                      map.AllSymbols.OfType<LineSymbol>().Count(), coverages.Count(coverage => coverage.Symbol is LineSymbol),
                                      ofLines.OfType<StrokedShape>().Count(shape => shape.Stroke.DashPattern != null), ofLines.Count(clippingObject => clippingObject.Shape is OffsetPathShape),
                                      ofLines.Count(clippingObject => clippingObject.Placement is RepeatedPlacement), ofLines.Select(clippingObject => clippingObject.Placement).OfType<RepeatedPlacement>().Distinct().Count());
                TestContext.WriteLine("{0} area symbols, {1} palette color coverages of area symbols, {2} graphics objects, {3} palette color coverages of graphics objects",
                                      map.AllSymbols.OfType<AreaSymbol>().Count(), coverages.Count(coverage => coverage.Symbol is AreaSymbol),
                                      map.AllSymbols.Count(symbol => symbol is IGraphicsSymbol), coverages.Count(coverage => coverage.Symbol is IGraphicsSymbol));
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
            CollectionAssert.AreNotEqual(((SinglePlacement)partsOfSymbols[0][0].Placement).Matrix.Elements, ((SinglePlacement)partsOfSymbols[2][0].Placement).Matrix.Elements);
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

        // The branches of the drawing of solid lines, each of which the geometry method of lines follows: a polyline, a
        // closed line, a Bezier curve and a line of zero length; the pointy ends of a shortened line, also of one that the
        // shortening takes whole; a second line; and the fill and the borders of a double line, also of a line whose
        // left border stops along it. A border is an offset path of the path.
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

            List<ClippingObject>[] clippingObjects = GetClippingObjectsBySeparation(map);

            AssertSameStructureAsTheCollector(map);
            // Green: the fills of the two double lines. Brown: the main line of the line with a second line. Black: the
            // polyline, the closed line and the Bezier curve, but not the line of zero length; the shortened line and the
            // two pointy ends of the long line, and the two pointy ends alone of the short one; the second line; and the
            // borders of the double lines, the left border of the last one in two parts.
            CollectionAssert.AreEqual(new int[] { 2, 1, 14 }, clippingObjects.Select(objects => objects.Count));
            List<OffsetPathShape> borders = clippingObjects[2].Select(clippingObject => clippingObject.Shape).OfType<OffsetPathShape>().ToList();
            float[] offsets = borders.Select(border => border.Offset).ToArray();
            Assert.AreEqual(5, offsets.Length);
            for (int i = 0; i < offsets.Length; ++i)
                Assert.AreEqual(new float[] { -0.45F, 0.475F, -0.45F, -0.45F, 0.475F }[i], offsets[i], Tolerance, "the offset of border {0}", i);
            Assert.IsTrue(((PathShape)clippingObjects[2][2].Shape).Parts.Any(part => part.Kind == GraphicsPathPartKind.Beziers), "the Bezier curve keeps its curve");
        }

        // A dashed line is a composed dashed line: the middle dashes that repeat a pattern are a run of dashes, a stroked
        // shape whose pen has the pattern, and the end dashes and a dash from which the rule of the vertices leaves the
        // vertex out are single dashes. The line of 29.5 mm of a euclidean map has 12 dashes of 2 mm with gaps of 0.5 mm.
        // Its seventh dash starts 0.05 mm before a vertex, nearer than 0.4 times the width of the line of 0.35 mm, so it
        // leaves the vertex out and goes straight to its end. The dashes 2 to 6 and 8 to 11 are runs.
        [Test]
        public void DashedLineIsAComposedDashedLine()
        {
            Map map = CreateMap();
            using (map.Write()) {
                map.UseEuclideanMetric = true;
                SymColor black = map.AddColor("Black", 1, 0, 0, 0, 1, false);
                LineSymDef dashed = new LineSymDef("Dashed", "1", black, 0.35F, LineJoinMode.Miter, LineCapMode.Flat);
                dashed.SetDashInfo(new LineSymDef.DashInfo() { dashLength = 2, firstDashLength = 2, lastDashLength = 2, gapLength = 0.5F });
                AddLineSymDef(map, dashed);
                double angle = Math.PI / 3;
                AddLine(map, dashed, new SymPath(new PointF[] {
                    new PointF(0, 0), new PointF(15.05F, 0), new PointF(15.05F + (float)(14.45 * Math.Cos(angle)), (float)(14.45 * Math.Sin(angle))) }));
            }

            AssertSameStructureAsTheCollector(map);
            StrokedShape[] shapes = GetClippingObjectsBySeparation(map).Single().Cast<StrokedShape>().ToArray();
            CollectionAssert.AreEqual(new int[] { 0, 2, 0, 2, 0 }, shapes.Select(shape => (shape.Stroke.DashPattern == null) ? 0 : shape.Stroke.DashPattern.Count));
            CollectionAssert.AreEqual(new int[] { 1, 5, 1, 4, 1 }, shapes.Select(shape => shape.ExpandDashes().Count));
            foreach (int run in new int[] { 1, 3 }) {
                Assert.AreEqual(2, shapes[run].Stroke.DashPattern[0], Tolerance, "the dash of run {0}", run);
                Assert.AreEqual(0.5F, shapes[run].Stroke.DashPattern[1], Tolerance, "the gap of run {0}", run);
                Assert.AreEqual(0, shapes[run].Stroke.DashPhase, "the phase of run {0}", run);
            }
            Assert.AreEqual(2, ((PathShape)shapes[2].Shape).Parts.Sum(part => part.Points.Length), "the single dash leaves its vertex out");
        }

        // The secondary gaps of the middle dashes make the period of a run: a dash, its secondary gap, the rest of the
        // dash and the gap, so the pattern of the run has four lengths. The end dashes with their secondary gaps are
        // single dashes.
        [Test]
        public void SecondaryGapsMakeThePeriodOfARun()
        {
            Map map = CreateMap();
            using (map.Write()) {
                map.UseEuclideanMetric = true;
                SymColor black = map.AddColor("Black", 1, 0, 0, 0, 1, false);
                LineSymDef dashed = new LineSymDef("Dashed", "1", black, 0.25F, LineJoinMode.Miter, LineCapMode.Flat);
                dashed.SetDashInfo(new LineSymDef.DashInfo() { dashLength = 3, firstDashLength = 2, lastDashLength = 2, gapLength = 0.5F,
                                                               secondaryMiddleGaps = 1, secondaryMiddleLength = 0.2F, secondaryEndGaps = 1, secondaryEndLength = 0.2F });
                AddLineSymDef(map, dashed);
                AddLine(map, dashed, new SymPath(new PointF[] { new PointF(0, 0), new PointF(30, 0) }));
            }

            AssertSameStructureAsTheCollector(map);
            StrokedShape[] shapes = GetClippingObjectsBySeparation(map).Single().Cast<StrokedShape>().ToArray();
            StrokedShape run = shapes.Single(shape => shape.Stroke.DashPattern != null);
            Assert.AreEqual(4, run.Stroke.DashPattern.Count);
            Assert.AreEqual(0.2F, run.Stroke.DashPattern[1], Tolerance, "the secondary gap");
            Assert.AreEqual(0.5F, run.Stroke.DashPattern[3], Tolerance, "the gap");
            Assert.AreEqual(run.Stroke.DashPattern[0] + run.Stroke.DashPattern[1] + run.Stroke.DashPattern[2], 3, 0.2F, "the dash with its secondary gap");
            Assert.IsNull(shapes[0].Stroke.DashPattern, "the first dash");
            Assert.IsNull(shapes[shapes.Length - 1].Stroke.DashPattern, "the last dash");
        }

        // On a Bezier curve, the composed dashed line has a run for each quarter of the curve, with each metric, as the
        // dashes are placed by quarters (fix 021): the curve of the ninth dash sample whose bend is near its end has four
        // runs. The dashes and the gaps of a run are those that the measure of the quarters fits, scaled by the length of
        // the flattened quarter against its chord in the metric of the map. So the ratio of a dash to its gap is the same
        // in each run, and with the euclidean metric the gaps get longer along the curve, as its quarters bend more. The
        // OCAD metric also measures the chords by their directions.
        [TestCase(true)]
        [TestCase(false)]
        public void DashedLineOnABezierCurveHasARunForEachQuarter(bool euclidean)
        {
            Map map = CreateMap();
            using (map.Write()) {
                map.UseEuclideanMetric = euclidean;
                SymColor black = map.AddColor("Black", 1, 0, 0, 0, 1, false);
                LineSymDef dashed = new LineSymDef("Dashed", "1", black, 0.05F, LineJoinMode.Miter, LineCapMode.Flat);
                dashed.SetDashInfo(new LineSymDef.DashInfo() { dashLength = 0.5F, firstDashLength = 0.5F, lastDashLength = 0.5F, gapLength = 0.25F });
                AddLineSymDef(map, dashed);
                AddLine(map, dashed, new SymPath(new PointF[] { new PointF(4, -82), new PointF(24, -82), new PointF(29, -82), new PointF(29, -77) },
                                                 new PointKind[] { PointKind.Normal, PointKind.BezierControl, PointKind.BezierControl, PointKind.Normal }));
            }

            AssertSameStructureAsTheCollector(map);
            List<IReadOnlyList<float>> patterns = GetClippingObjectsBySeparation(map).Single().Cast<StrokedShape>()
                                                                                    .Where(shape => shape.Stroke.DashPattern != null)
                                                                                    .Select(shape => shape.Stroke.DashPattern)
                                                                                    .ToList();
            Assert.AreEqual(4, patterns.Count, "runs");
            foreach (IReadOnlyList<float> pattern in patterns) {
                Assert.AreEqual(2, pattern.Count);
                Assert.AreEqual(patterns[0][0] / patterns[0][1], pattern[0] / pattern[1], Tolerance, "the ratio of a dash to its gap");
            }
            for (int i = 1; i < patterns.Count && euclidean; ++i)
                Assert.Greater(patterns[i][1], patterns[i - 1][1], "the gap of run {0}", i);
        }

        // With the OCAD metric, the runs of dashes are by straight segment, and the pattern of a run is the fitted pattern
        // scaled to the euclidean length by the direction of the segment: a line of a segment along the x axis and a
        // segment at 30 degrees, between corner points at its ends only, has one fitted pattern, which the second run
        // has divided by the OCAD metric of a euclidean length of 1 at 30 degrees, cos 30° + sin 30° / 2. A dash over the
        // vertex between the segments is a single dash.
        [Test]
        public void DashedLineOfTheOcadMetricHasARunForEachStraightSegment()
        {
            Map map = CreateMap();
            using (map.Write()) {
                map.UseEuclideanMetric = false;
                SymColor black = map.AddColor("Black", 1, 0, 0, 0, 1, false);
                LineSymDef dashed = new LineSymDef("Dashed", "1", black, 0.2F, LineJoinMode.Miter, LineCapMode.Flat);
                dashed.SetDashInfo(new LineSymDef.DashInfo() { dashLength = 2, firstDashLength = 2, lastDashLength = 2, gapLength = 0.5F });
                AddLineSymDef(map, dashed);
                double angle = Math.PI / 6;
                AddLine(map, dashed, new SymPath(new PointF[] { new PointF(0, 0), new PointF(16.2F, 0), new PointF(16.2F + (float)(16 * Math.Cos(angle)), (float)(16 * Math.Sin(angle))) }));
            }

            AssertSameStructureAsTheCollector(map);
            StrokedShape[] shapes = GetClippingObjectsBySeparation(map).Single().Cast<StrokedShape>().ToArray();
            StrokedShape[] runs = shapes.Where(shape => shape.Stroke.DashPattern != null).ToArray();
            Assert.AreEqual(2, runs.Length, "runs");
            double factor = Math.Cos(Math.PI / 6) + Math.Sin(Math.PI / 6) / 2;
            Assert.AreEqual(runs[0].Stroke.DashPattern[0] / factor, runs[1].Stroke.DashPattern[0], Tolerance, "the dash of the second run");
            Assert.AreEqual(runs[0].Stroke.DashPattern[1] / factor, runs[1].Stroke.DashPattern[1], Tolerance, "the gap of the second run");
            int between = Array.IndexOf(shapes, runs[1]) - Array.IndexOf(shapes, runs[0]) - 1;
            Assert.AreEqual(1, between, "the dash over the vertex");
            Assert.AreEqual(3, ((PathShape)shapes[Array.IndexOf(shapes, runs[0]) + 1].Shape).Parts.Sum(part => part.Points.Length), "the dash over the vertex goes through it");
        }

        // The runs of the dashed borders of a double line are the runs of the base path moved aside: offset paths of the
        // piece of the base path, whose pattern is measured along the base path. Each border has its own exceptions of
        // the rule of the vertices, from its own width.
        [Test]
        public void RunsOfADashedBorderAreOffsetPaths()
        {
            Map map = CreateMap();
            using (map.Write()) {
                map.UseEuclideanMetric = true;
                SymColor black = map.AddColor("Black", 1, 0, 0, 0, 1, false);
                LineSymDef doubled = new LineSymDef("Double", "1", null, 0, LineJoinMode.Miter, LineCapMode.Flat);
                doubled.SetDoubleLines(new LineSymDef.DoubleLineInfo() {
                    doubleThick = 0.6F, doubleLeftColor = black, doubleLeftWidth = 0.1F, doubleRightColor = black, doubleRightWidth = 0.1F,
                    doubleLeftDashed = true, doubleRightDashed = true,
                    doubleDashes = new LineSymDef.DashInfo() { dashLength = 1.5F, firstDashLength = 1.5F, lastDashLength = 1.5F, gapLength = 0.3F } });
                AddLineSymDef(map, doubled);
                AddLine(map, doubled, new SymPath(new PointF[] { new PointF(0, 0), new PointF(10, 0), new PointF(16, 6) }));
            }

            AssertSameStructureAsTheCollector(map);
            StrokedShape[] runs = GetClippingObjectsBySeparation(map).Single().Cast<StrokedShape>().Where(shape => shape.Stroke.DashPattern != null).ToArray();
            Assert.GreaterOrEqual(runs.Length, 2, "runs");
            foreach (StrokedShape run in runs) {
                OffsetPathShape offsetPath = (OffsetPathShape)run.Shape;
                Assert.AreEqual(0.35F, Math.Abs(offsetPath.Offset), Tolerance);
                Assert.AreEqual(1.5F, run.Stroke.DashPattern[0], 0.1F);
            }
            Assert.IsTrue(runs.Any(run => ((OffsetPathShape)run.Shape).Offset < 0) && runs.Any(run => ((OffsetPathShape)run.Shape).Offset > 0), "both borders have runs");
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

            AssertSameStructureAsTheCollector(map);
            // Each location places glyphs. The corners are the ends and the corner between them, and the dash point is
            // between the ends.
            List<ClippingObject>[] clippingObjects = GetClippingObjectsBySeparation(map);
            Assert.AreEqual(1 + locations.Length, clippingObjects.Length);
            Dictionary<string, int> glyphCounts = new Dictionary<string, int>();
            using (map.Read()) {
                for (int i = 1; i < clippingObjects.Length; ++i)
                    glyphCounts.Add(map.PaletteSeparations[i].Color.Name, SymbolCoverageComparison.Expand(clippingObjects[i]).Count);
            }
            TestContext.WriteLine(string.Join(", ", glyphCounts.Select(count => count.Key + " " + count.Value)));
            Assert.IsTrue(glyphCounts.Values.All(count => count > 0), "each location places glyphs");
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

            AssertSameStructureAsTheCollector(map);
            List<ClippingObject>[] clippingObjects = GetClippingObjectsBySeparation(map);
            Assert.AreEqual(5, clippingObjects.Length);

            // The corner between the ends turns the line by 45 degrees, and stretches its glyph by the miter factor of the
            // angle of 135 degrees, but not the width of its line. The glyphs at the ends share the shapes.
            List<ClippingObject> corners = clippingObjects[1];
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
            List<ClippingObject> spaced = SymbolCoverageComparison.Expand(clippingObjects[2]).Select(expanded => expanded.clippingObject).ToList();
            Assert.Greater(spaced.Count, 4);
            foreach (ClippingObject glyph in spaced)
                Assert.AreSame(spaced[0].Shape, glyph.Shape);

            // The decreasing glyphs of full size share the shapes, and each smaller glyph has shapes of its own.
            ClippingObject[] fullSize = clippingObjects[3].Where(glyph => Math.Abs(MeasureTick(glyph) - 0.8F) < Tolerance).ToArray();
            ClippingObject[] smaller = clippingObjects[3].Except(fullSize).ToArray();
            TestContext.WriteLine("{0} decreasing glyphs of full size, {1} smaller", fullSize.Length, smaller.Length);
            Assert.Greater(fullSize.Length, 1);
            Assert.Greater(smaller.Length, 1);
            foreach (ClippingObject glyph in fullSize)
                Assert.AreSame(fullSize[0].Shape, glyph.Shape);
            foreach (ClippingObject glyph in smaller)
                Assert.Less(MeasureTick(glyph), 0.8F - Tolerance);
            Assert.AreEqual(smaller.Length, smaller.Select(glyph => glyph.Shape).Concat(new Shape[] { fullSize[0].Shape }).Distinct().Count() - 1);

            // The glyph of one circle at its origin is placed with its parts at each dash point, and shares its shape.
            List<ClippingObject> dots = clippingObjects[4];
            Assert.AreEqual(2, dots.Count);
            Assert.AreSame(dots[0].Shape, dots[1].Shape);
            Assert.AreEqual(new PointF(0, 0), ((EllipseShape)dots[0].Shape).Center);
            foreach (ClippingObject dot in dots)
                Assert.AreEqual(1, dot.Placement.Steps.Count);
        }

        // The evenly spaced glyphs of a part of a line are one repeated placement of the shared shapes of the glyph, along
        // the part from the first glyph to the last, at a euclidean step: the glyphs spaced 2 mm along a straight line of
        // 40 mm turned by 30 degrees, 20 of them by the euclidean metric. The OCAD metric measures the line longer, and the
        // step of the repeated placement is the fitted distance divided by the OCAD metric of a euclidean length of 1 at
        // 30 degrees. The repeated placement turns the glyphs to the direction of the line, but not a glyph of one circle
        // at its origin, which the drawing draws without a transformation. At the centers of the dashes of a dashed line
        // whose end dashes are shorter, the glyphs of the end dashes are single placements, and the others repeat.
        [TestCase(true)]
        [TestCase(false)]
        public void EvenlySpacedGlyphsOfAPartAreOneRepeatedPlacement(bool euclidean)
        {
            Map map = CreateMap();
            using (map.Write()) {
                map.UseEuclideanMetric = euclidean;
                SymColor black = map.AddColor("Black", 1, 0, 0, 0, 1, false);
                SymColor brown = map.AddColor("Brown", 2, 0, 0.56F, 1, 0.18F, false);
                SymColor blue = map.AddColor("Blue", 3, 1, 0, 0, 0, false);
                SymColor green = map.AddColor("Green", 4, 0.76F, 0, 0.91F, 0, false);
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

            AssertSameStructureAsTheCollector(map);
            List<ClippingObject>[] clippingObjects = GetClippingObjectsBySeparation(map);
            ClippingObject ticks = clippingObjects[1].Single();
            RepeatedPlacement repeated = (RepeatedPlacement)ticks.Placement;
            int collected;
            using (map.Read())
                collected = map.PaletteSeparations[1].SymbolCoverages.Single().Symbol.Definition.CollectClippingObjects(map.PaletteSeparations[1].SymbolCoverages.Single().Symbol, map.PaletteSeparations[1].Color).Count;
            Assert.AreEqual(collected, repeated.Count);
            Assert.AreEqual(repeated.Count, repeated.Poses.Count);
            if (euclidean) {
                Assert.AreEqual(20, repeated.Count);
                Assert.AreEqual(2.0F, repeated.Step, Tolerance);
            }
            else {
                double factor = Math.Cos(Math.PI / 6) + Math.Sin(Math.PI / 6) / 2;
                double fitted = 40 * factor / repeated.Count;
                Assert.AreEqual(fitted / factor, repeated.Step, 0.01, "the euclidean step");
            }
            Assert.IsTrue(repeated.IsTurnedToPath);
            foreach (Pose pose in repeated.Poses)
                Assert.AreEqual(30, pose.Angle, Tolerance);

            RepeatedPlacement dots = (RepeatedPlacement)clippingObjects[3].Single().Placement;
            Assert.IsFalse(dots.IsTurnedToPath);
            foreach (Pose pose in dots.Poses)
                Assert.AreEqual(0, pose.Angle);

            List<ClippingObject> centers = clippingObjects[2];
            Assert.AreEqual(3, centers.Count);
            Assert.IsInstanceOf<SinglePlacement>(centers[0].Placement);
            Assert.IsInstanceOf<RepeatedPlacement>(centers[1].Placement);
            Assert.IsInstanceOf<SinglePlacement>(centers[2].Placement);
            Assert.AreSame(centers[0].Shape, centers[1].Shape);
        }

        // On a Bezier curve, the glyphs of the OCAD spacing repeat by the quarters of the curve, with each metric, as fix
        // 021 places them: the quarters have different steps, and a repeated placement is in one quarter.
        [TestCase(true)]
        [TestCase(false)]
        public void SpacedGlyphsOnABezierCurveRepeatByQuarters(bool euclidean)
        {
            Map map = CreateMap();
            using (map.Write()) {
                map.UseEuclideanMetric = euclidean;
                SymColor black = map.AddColor("Black", 1, 0, 0, 0, 1, false);
                SymColor brown = map.AddColor("Brown", 2, 0, 0.56F, 1, 0.18F, false);
                LineSymDef spaced = new LineSymDef("Spaced", "1", black, 0.1F, LineJoinMode.Miter, LineCapMode.Flat);
                spaced.SetGlyphs(new LineSymDef.GlyphInfo[] {
                    new LineSymDef.GlyphInfo() { glyph = CreateTick(brown), location = LineSymDef.GlyphLocation.Spaced, distance = 0.5F, firstDistance = 0.25F, lastDistance = 0.25F, number = 1 } });
                AddLineSymDef(map, spaced);
                AddLine(map, spaced, new SymPath(new PointF[] { new PointF(4, -82), new PointF(24, -82), new PointF(29, -82), new PointF(29, -77) },
                                                 new PointKind[] { PointKind.Normal, PointKind.BezierControl, PointKind.BezierControl, PointKind.Normal }));
            }

            AssertSameStructureAsTheCollector(map);
            List<RepeatedPlacement> repeated = GetClippingObjectsBySeparation(map)[1].Select(glyph => glyph.Placement).OfType<RepeatedPlacement>().ToList();
            TestContext.WriteLine(string.Join(", ", repeated.Select(placement => placement.Count + " by " + placement.Step)));
            Assert.AreEqual(4, repeated.Count, "repeated placements");
            for (int i = 1; i < repeated.Count; ++i)
                Assert.AreNotEqual(repeated[0].Step, repeated[i].Step, "the step of quarter {0}", i);
        }

        // With the OCAD metric, the evenly spaced glyphs repeat by straight segment, as the runs of dashes do: on a line
        // of a segment along the x axis and a segment at 30 degrees, between corner points at its ends only, the step of
        // the repeated placement of the second segment is the step of the first divided by the OCAD metric of a
        // euclidean length of 1 at 30 degrees, cos 30° + sin 30° / 2, and its poses turn by 30 degrees.
        [Test]
        public void SpacedGlyphsOfTheOcadMetricRepeatForEachStraightSegment()
        {
            Map map = CreateMap();
            using (map.Write()) {
                map.UseEuclideanMetric = false;
                SymColor black = map.AddColor("Black", 1, 0, 0, 0, 1, false);
                SymColor brown = map.AddColor("Brown", 2, 0, 0.56F, 1, 0.18F, false);
                LineSymDef spaced = new LineSymDef("Spaced", "1", black, 0.1F, LineJoinMode.Miter, LineCapMode.Flat);
                spaced.SetGlyphs(new LineSymDef.GlyphInfo[] {
                    new LineSymDef.GlyphInfo() { glyph = CreateTick(brown), location = LineSymDef.GlyphLocation.Spaced, distance = 0.5F, firstDistance = 0.25F, lastDistance = 0.25F, number = 1 } });
                AddLineSymDef(map, spaced);
                double angle = Math.PI / 6;
                AddLine(map, spaced, new SymPath(new PointF[] { new PointF(0, 0), new PointF(16.2F, 0), new PointF(16.2F + (float)(16 * Math.Cos(angle)), (float)(16 * Math.Sin(angle))) }));
            }

            AssertSameStructureAsTheCollector(map);
            List<RepeatedPlacement> repeated = GetClippingObjectsBySeparation(map)[1].Select(glyph => glyph.Placement).OfType<RepeatedPlacement>().ToList();
            TestContext.WriteLine(string.Join(", ", repeated.Select(placement => placement.Count + " by " + placement.Step)));
            Assert.AreEqual(2, repeated.Count, "repeated placements");
            double factor = Math.Cos(Math.PI / 6) + Math.Sin(Math.PI / 6) / 2;
            Assert.AreEqual(repeated[0].Step / factor, repeated[1].Step, Tolerance, "the step of the second segment");
            foreach (Pose pose in repeated[0].Poses)
                Assert.AreEqual(0, pose.Angle, Tolerance, "the angle on the first segment");
            foreach (Pose pose in repeated[1].Poses)
                Assert.AreEqual(30, pose.Angle, Tolerance, "the angle on the second segment");
        }

        // A combined line (LineComboSymDef, of a Mapper map) has the clipping objects of its components, in the order of
        // the components, as its drawing draws them: in brown the wide line of the first component, and then the
        // glyphs of the second component at the centers of its gaps; in black the dashes of the second component.
        [Test]
        public void CombinedLineHasTheClippingObjectsOfItsComponents()
        {
            Map map = CreateMap();
            using (map.Write()) {
                map.UseEuclideanMetric = true;
                SymColor brown = map.AddColor("Brown", 1, 0, 0.56F, 1, 0.18F, false);
                SymColor black = map.AddColor("Black", 2, 0, 0, 0, 1, false);
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

            AssertSameStructureAsTheCollector(map);
            List<ClippingObject>[] clippingObjects = GetClippingObjectsBySeparation(map);
            StrokedShape[] brownShapes = SymbolCoverageComparison.Expand(clippingObjects[0]).Select(expanded => expanded.clippingObject).Cast<StrokedShape>().ToArray();
            Assert.Greater(brownShapes.Length, 1);
            CollectionAssert.AreEqual(new float[] { 0.6F }.Concat(Enumerable.Repeat(0.1F, brownShapes.Length - 1)), brownShapes.Select(shape => shape.Stroke.Width));
            Assert.IsNotEmpty(clippingObjects[1]);
        }

        // The symbol definitions of the points, the lines, the areas and the graphics objects make the clipping objects of
        // the palette color coverages of their symbols with a geometry method of their own, and not with the collector
        // that SymDef.MakeClippingObjects uses. The texts and the rectangles still use the collector (draft 013). The
        // geometry methods of the areas and the graphics objects make the same clipping objects as the collector, so the
        // comparison cannot tell them apart: this test checks which class implements the interface.
        [Test]
        public void SymbolDefinitionsOfPointsLinesAreasAndGraphicsObjectsHaveAGeometryMethod()
        {
            Type[] parameters = new Type[] { typeof(Symbol), typeof(SymColor), typeof(CoveragePrototype) };
            Dictionary<Type, Type> expected = new Dictionary<Type, Type>() {
                { typeof(PointSymDef), typeof(PointSymDef) },
                { typeof(LineSymDef), typeof(LineSymDef) },
                { typeof(LineComboSymDef), typeof(LineComboSymDef) },
                { typeof(AreaSymDef), typeof(AreaLikeSymDef) },
                { typeof(AreaComboSymDef), typeof(AreaLikeSymDef) },
                { typeof(GraphicsSymDef), typeof(GraphicsSymDef) },
                { typeof(TextSymDef), typeof(SymDef) },
                { typeof(RectangleSymDef), typeof(SymDef) } };
            foreach (KeyValuePair<Type, Type> definition in expected) {
                System.Reflection.MethodInfo method = definition.Key.GetMethod("MakeClippingObjects", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic, null, parameters, null);
                Assert.IsNotNull(method, definition.Key.Name);
                Assert.AreEqual(definition.Value, method.DeclaringType, "the class that implements the geometry method of {0}", definition.Key.Name);
            }
        }

        // The branches of the area fill, each of which the geometry method of the areas follows as SymPath.FillCore fills
        // the path: a polygon of a main path without Bezier curves and holes, a path of a main path with a Bezier curve,
        // and a path of a main path and its two holes. An area whose hole the map removed is a polygon again, as a path
        // with holes keeps no empty array of holes (SymPathWithHoles). Each is a filled shape with the alternate fill
        // rule.
        [Test]
        public void GeometryMethodOfAreasFollowsEachBranchOfTheirFill()
        {
            Map map = CreateMap();
            AreaSymbol withRemovedHole;
            using (map.Write()) {
                SymColor green = map.AddColor("Green", 1, 0.76F, 0, 0.91F, 0, false);
                AreaSymDef fill = AddAreaSymDef(map, new AreaSymDef("Fill", "1", green, null));
                AddArea(map, fill, new SymPathWithHoles(CreateSquarePath(0, 0, 4), null));
                AddArea(map, fill, new SymPathWithHoles(CreateCurvedAreaPath(0, 10), null));
                AddArea(map, fill, new SymPathWithHoles(CreateSquarePath(6, 0, 4), new SymPath[] { CreateSquarePath(7, 1, 1), CreateSquarePath(8.5F, 2, 1) }));
                withRemovedHole = AddArea(map, fill, new SymPathWithHoles(CreateSquarePath(12, 0, 4), new SymPath[] { CreateSquarePath(13, 1, 1) }));
                map.RemoveSymbol(withRemovedHole.GetHole(0));
            }

            Assert.IsNull(withRemovedHole.Path.Holes, "the holes of the area whose hole the map removed");
            AssertSameStructureAsTheCollector(map);
            FilledShape[] fills = GetClippingObjectsBySeparation(map).Single().Cast<FilledShape>().ToArray();
            Assert.AreEqual(4, fills.Length);
            foreach (FilledShape fill in fills)
                Assert.AreEqual(AreaFillMode.Alternate, fill.FillRule);
            CollectionAssert.AreEqual(new int[] { 1, 1, 3, 1 }, fills.Select(fill => CountFigures((PathShape)fill.Shape)), "the figures of the fills");
            Assert.IsFalse(((PathShape)fills[0].Shape).Parts.Any(part => part.Kind == GraphicsPathPartKind.Beziers), "the polygon has no Bezier curve");
            Assert.IsTrue(((PathShape)fills[1].Shape).Parts.Any(part => part.Kind == GraphicsPathPartKind.Beziers), "the fill keeps the Bezier curve");
        }

        // The area border goes along the main path and then along each hole, as AreaSymDef.Draw draws it, with the clipping
        // objects of the geometry method of the lines: a solid border after the area fill, a dashed border with runs of
        // dashes, an area border that is a double line with offset paths, and an area border with glyphs at repeated
        // placements, each in a palette color of its own. Each area has a square hole.
        [Test]
        public void AreaBorderGoesAlongTheMainPathAndThenAlongEachHole()
        {
            Map map = CreateMap();
            using (map.Write()) {
                map.UseEuclideanMetric = true;
                SymColor green = map.AddColor("Green", 1, 0.76F, 0, 0.91F, 0, false);
                SymColor black = map.AddColor("Black", 2, 0, 0, 0, 1, false);
                SymColor brown = map.AddColor("Brown", 3, 0, 0.56F, 1, 0.18F, false);
                SymColor blue = map.AddColor("Blue", 4, 1, 0, 0, 0, false);
                SymColor purple = map.AddColor("Purple", 5, 0.35F, 0.85F, 0, 0, false);

                LineSymDef solid = AddLineSymDef(map, new LineSymDef("Solid", "1", black, 0.2F, LineJoinMode.Miter, LineCapMode.Flat));
                LineSymDef dashed = new LineSymDef("Dashed", "2", brown, 0.15F, LineJoinMode.Miter, LineCapMode.Flat);
                dashed.SetDashInfo(new LineSymDef.DashInfo() { dashLength = 1, firstDashLength = 1, lastDashLength = 1, gapLength = 0.3F });
                AddLineSymDef(map, dashed);
                LineSymDef doubled = new LineSymDef("Double", "3", null, 0, LineJoinMode.Miter, LineCapMode.Flat);
                doubled.SetDoubleLines(new LineSymDef.DoubleLineInfo() { doubleThick = 0.4F, doubleLeftColor = blue, doubleLeftWidth = 0.1F, doubleRightColor = blue, doubleRightWidth = 0.1F });
                AddLineSymDef(map, doubled);
                LineSymDef ticked = new LineSymDef("Ticked", "4", null, 0, LineJoinMode.Miter, LineCapMode.Flat);
                ticked.SetGlyphs(new LineSymDef.GlyphInfo[] {
                    new LineSymDef.GlyphInfo() { glyph = CreateTick(purple), location = LineSymDef.GlyphLocation.Spaced, distance = 1, firstDistance = 0.5F, lastDistance = 0.5F, number = 1 } });
                AddLineSymDef(map, ticked);

                AddArea(map, AddAreaSymDef(map, new AreaSymDef("Solid", "5", green, solid)), CreateSquareWithHole(0));
                AddArea(map, AddAreaSymDef(map, new AreaSymDef("Dashed", "6", null, dashed)), CreateSquareWithHole(20));
                AddArea(map, AddAreaSymDef(map, new AreaSymDef("Double", "7", null, doubled)), CreateSquareWithHole(40));
                AddArea(map, AddAreaSymDef(map, new AreaSymDef("Ticked", "8", null, ticked)), CreateSquareWithHole(60));
            }

            AssertSameStructureAsTheCollector(map);
            List<ClippingObject>[] clippingObjects = GetClippingObjectsBySeparation(map);
            // Green: the area fill. Black: the solid border along the main path and then along the hole.
            Assert.IsInstanceOf<FilledShape>(clippingObjects[0].Single());
            Assert.AreEqual(2, clippingObjects[1].Count);
            AssertBounds(RectangleF.FromLTRB(0, 0, 10, 10), clippingObjects[1][0].Shape.Bounds);
            AssertBounds(RectangleF.FromLTRB(3, 3, 7, 7), clippingObjects[1][1].Shape.Bounds);
            // Brown: the runs of dashes are along the main path and along the hole.
            StrokedShape[] runs = clippingObjects[2].Cast<StrokedShape>().Where(shape => shape.Stroke.DashPattern != null).ToArray();
            Assert.IsTrue(runs.Any(run => run.Shape.Bounds.Right > 27 + Tolerance), "a run along the main path");
            Assert.IsTrue(runs.Any(run => run.Shape.Bounds.Left > 23 - Tolerance && run.Shape.Bounds.Right < 27 + Tolerance && run.Shape.Bounds.Bottom < 7 + Tolerance), "a run along the hole");
            // Blue: the borders of the double line, offset paths of the main path and of the hole.
            List<OffsetPathShape> offsetPaths = clippingObjects[3].Select(clippingObject => clippingObject.Shape).OfType<OffsetPathShape>().ToList();
            Assert.AreEqual(4, offsetPaths.Count, "the borders of the double line along the main path and the hole");
            Assert.AreEqual(4, offsetPaths.Select(offsetPath => offsetPath.BasePath).Distinct().Count(), "the pieces of the base paths");
            // Purple: the ticks along the main path and along the hole repeat.
            Assert.GreaterOrEqual(clippingObjects[4].Select(clippingObject => clippingObject.Placement).OfType<RepeatedPlacement>().Distinct().Count(), 2, "repeated placements");
        }

        // A combined area (AreaComboSymDef, of a Mapper map) has the clipping objects of its component definitions, in
        // their order, as AreaComboSymDef.Draw draws them: a line along the main path and then along the hole, an area with
        // its area fill and its area border, a combined line (LineComboSymDef) whose components go along the main path and
        // then along the hole, and another combined area of a line. The Mapper import makes the last two when a part of a
        // combined symbol is a combined symbol (OpenMapperImport.DelayedComboSymDef.CreateSymDef). The lines have widths of
        // their own, which show the order.
        [Test]
        public void CombinedAreaHasTheClippingObjectsOfItsComponentDefinitionsInTheirOrder()
        {
            Map map = CreateMap();
            using (map.Write()) {
                SymColor green = map.AddColor("Green", 1, 0.76F, 0, 0.91F, 0, false);
                SymColor black = map.AddColor("Black", 2, 0, 0, 0, 1, false);
                LineSymDef[] lines = new LineSymDef[5];
                for (int i = 0; i < lines.Length; ++i)
                    lines[i] = AddLineSymDef(map, new LineSymDef("Line " + (i + 1), (i + 1).ToString(), black, 0.1F * (i + 1), LineJoinMode.Miter, LineCapMode.Flat));
                AreaSymDef area = AddAreaSymDef(map, new AreaSymDef("Area", "6", green, lines[1]));
                LineComboSymDef combinedLine = new LineComboSymDef("Combined line", "7", new LineSymDef[] { lines[2], lines[3] });
                map.AddSymdef(combinedLine);
                AreaComboSymDef inner = new AreaComboSymDef("Inner", "8", new SymDef[] { lines[4] });
                map.AddSymdef(inner);
                AreaComboSymDef outer = new AreaComboSymDef("Outer", "9", new SymDef[] { lines[0], area, combinedLine, inner });
                map.AddSymdef(outer);
                AddArea(map, outer, CreateSquareWithHole(0));
            }

            AssertSameStructureAsTheCollector(map);
            List<ClippingObject>[] clippingObjects = GetClippingObjectsBySeparation(map);
            Assert.IsInstanceOf<FilledShape>(clippingObjects[0].Single(), "the area fill of the area");
            float[] widths = clippingObjects[1].Cast<StrokedShape>().Select(shape => shape.Stroke.Width).ToArray();
            float[] expected = new float[] { 0.1F, 0.1F, 0.2F, 0.2F, 0.3F, 0.4F, 0.3F, 0.4F, 0.5F, 0.5F };
            Assert.AreEqual(expected.Length, widths.Length, "the lines along the main path and the hole");
            for (int i = 0; i < expected.Length; ++i)
                Assert.AreEqual(expected[i], widths[i], Tolerance, "the width of line {0}", i);
        }

        // An area symbol has no palette color coverage with the full coverage in a palette color that its area symbol
        // definition draws only with a hatching or an area pattern: the hatchings and the area patterns are periodic
        // coverages, and the geometry method of the areas makes no clipping objects of them. In the palette color of the
        // area fill, a hatching of the same palette color adds nothing to the area fill.
        [Test]
        public void AreaHasNoFullCoverageInAPaletteColorOfItsHatchingsAndAreaPatternsAlone()
        {
            Map map = CreateMap();
            AreaSymbol patterned, hatchedInItsFillColor;
            using (map.Write()) {
                SymColor green = map.AddColor("Green", 1, 0.76F, 0, 0.91F, 0, false);
                SymColor blue = map.AddColor("Blue", 2, 1, 0, 0, 0, false);
                SymColor brown = map.AddColor("Brown", 3, 0, 0.56F, 1, 0.18F, false);
                AreaSymDef withPatterns = new AreaSymDef("Patterns", "1", green, null);
                withPatterns.AddHatching(new AreaSymDef.HatchInfo() { hatchColor = blue, hatchWidth = 0.1F, hatchSpacing = 0.5F, hatchAngle = 45 });
                withPatterns.AddPattern(new AreaSymDef.PatternInfo() { patternGlyph = CreateGlyph(glyph => glyph.AddFilledCircle(brown, new PointF(0, 0), 0.3F)), patternWidth = 1, patternHeight = 1 });
                AddAreaSymDef(map, withPatterns);
                AreaSymDef hatchedInItsFill = new AreaSymDef("Hatched", "2", green, null);
                hatchedInItsFill.AddHatching(new AreaSymDef.HatchInfo() { hatchColor = green, hatchWidth = 0.1F, hatchSpacing = 0.5F, hatchAngle = 30 });
                AddAreaSymDef(map, hatchedInItsFill);
                patterned = AddArea(map, withPatterns, new SymPathWithHoles(CreateSquarePath(0, 0, 4), null));
                hatchedInItsFillColor = AddArea(map, hatchedInItsFill, new SymPathWithHoles(CreateSquarePath(6, 0, 4), null));
            }

            AssertSameStructureAsTheCollector(map);
            using (map.Read()) {
                IReadOnlyList<PaletteSeparation> separations = map.PaletteSeparations;
                CollectionAssert.AreEqual(new Symbol[] { patterned, hatchedInItsFillColor }, separations[0].SymbolCoverages.Select(coverage => coverage.Symbol));
                foreach (SymbolCoverage coverage in separations[0].SymbolCoverages)
                    Assert.IsInstanceOf<FilledShape>(coverage.ClippingObjects.Single(), "the area fill of {0}", coverage.Symbol.Definition.Name);
                CollectionAssert.IsEmpty(separations[1].SymbolCoverages, "the palette color of the hatching");
                CollectionAssert.IsEmpty(separations[2].SymbolCoverages, "the palette color of the area pattern");
            }
        }

        // The graphics objects have their own palette colors and pens: a GraphicsAreaSymbol is a filled shape of its path
        // with holes with the alternate fill rule, and a GraphicsLineSymbol is a stroked shape of its path, which keeps
        // its Bezier curve, with its width, caps and join and the miter limit GraphicsUtil.MITER_LIMIT, as their drawing
        // makes them.
        [Test]
        public void GraphicsObjectsAreAFilledShapeAndAStrokedShapeInTheirOwnPaletteColors()
        {
            Map map = CreateMap();
            using (map.Write()) {
                SymColor brown = map.AddColor("Brown", 1, 0, 0.56F, 1, 0.18F, false);
                SymColor blue = map.AddColor("Blue", 2, 1, 0, 0, 0, false);
                GraphicsSymDef graphics = new GraphicsSymDef();
                map.AddSymdef(graphics);
                map.AddSymbol(new GraphicsAreaSymbol(graphics, CreateSquareWithHole(0), brown));
                map.AddSymbol(new GraphicsLineSymbol(graphics, new SymPath(new PointF[] { new PointF(0, 15), new PointF(1, 17), new PointF(3, 17), new PointF(4, 15) },
                                                                           new PointKind[] { PointKind.Normal, PointKind.BezierControl, PointKind.BezierControl, PointKind.Normal }),
                                                     blue, 0.3F, LineJoinMode.Bevel, LineCapMode.Square));
            }

            AssertSameStructureAsTheCollector(map);
            List<ClippingObject>[] clippingObjects = GetClippingObjectsBySeparation(map);
            FilledShape fill = (FilledShape)clippingObjects[0].Single();
            Assert.AreEqual(AreaFillMode.Alternate, fill.FillRule);
            Assert.AreEqual(2, CountFigures((PathShape)fill.Shape), "the main path and the hole");
            StrokedShape line = (StrokedShape)clippingObjects[1].Single();
            Assert.AreEqual(0.3F, line.Stroke.Width);
            Assert.AreEqual(LineCapMode.Square, line.Stroke.Caps);
            Assert.AreEqual(LineJoinMode.Bevel, line.Stroke.Join);
            Assert.AreEqual(GraphicsUtil.MITER_LIMIT, line.Stroke.MiterLimit);
            Assert.IsNull(line.Stroke.DashPattern);
            Assert.IsTrue(((PathShape)line.Shape).Parts.Any(part => part.Kind == GraphicsPathPartKind.Beziers), "the line keeps its Bezier curve");
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

        // Add an area symbol definition to a map, and return it.
        static AreaSymDef AddAreaSymDef(Map map, AreaSymDef symdef)
        {
            map.AddSymdef(symdef);
            return symdef;
        }

        // Add an area symbol of a definition with a path with holes to a map, and return it.
        static AreaSymbol AddArea(Map map, AreaLikeSymDef symdef, SymPathWithHoles path)
        {
            AreaSymbol area = new AreaSymbol(symdef, path, 0, new PointF());
            map.AddSymbol(area);
            return area;
        }

        // A square of 10 mm with a square hole of 4 mm at its middle, at a distance along the x axis.
        static SymPathWithHoles CreateSquareWithHole(float left)
        {
            return new SymPathWithHoles(CreateSquarePath(left, 0, 10), new SymPath[] { CreateSquarePath(left + 3, 3, 4) });
        }

        // A closed path of an area with a Bezier curve at its top, at a distance along the y axis.
        static SymPath CreateCurvedAreaPath(float left, float bottom)
        {
            return new SymPath(new PointF[] { new PointF(left, bottom + 2), new PointF(left + 1, bottom + 4), new PointF(left + 3, bottom + 4), new PointF(left + 4, bottom + 2),
                                              new PointF(left + 4, bottom), new PointF(left, bottom), new PointF(left, bottom + 2) },
                               new PointKind[] { PointKind.Normal, PointKind.BezierControl, PointKind.BezierControl, PointKind.Normal, PointKind.Normal, PointKind.Normal, PointKind.Normal });
        }

        // The number of the figures of a path: of its start parts.
        static int CountFigures(PathShape path)
        {
            return path.Parts.Count(part => part.Kind == GraphicsPathPartKind.Start);
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

        // The names of the map files of the Skia rendering tests: each OCAD and Mapper file in skia_render.
        static IEnumerable<string> FindMapFilesOfTheRenderingTests()
        {
            return Directory.GetFiles(TestUtil.GetTestFile("skia_render"))
                            .Where(file => file.EndsWith(".ocd", StringComparison.OrdinalIgnoreCase) || file.EndsWith(".omap", StringComparison.OrdinalIgnoreCase))
                            .Select(Path.GetFileName)
                            .OrderBy(name => name, StringComparer.Ordinal);
        }

        // The names of the sample files of the dashed lines of OCAD in ocadimport, as paths from skia_render.
        static IEnumerable<string> FindDashSampleFiles()
        {
            return Directory.GetFiles(TestUtil.GetTestFile("ocadimport"), "dashes-*.ocd")
                            .Select(file => Path.Combine("..", "ocadimport", Path.GetFileName(file)))
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
