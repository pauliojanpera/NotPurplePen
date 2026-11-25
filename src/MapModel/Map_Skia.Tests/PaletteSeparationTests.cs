using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using PurplePen.Graphics2D;
using PurplePen.MapModel;

namespace Map_Skia.Tests
{
    // The palette separations that pass 1 makes (Map.PaletteSeparations), checked without a drawing: the palette
    // separations and their order, the palette color coverages of the symbols with their palette colors and coverage
    // prototypes, the palette cutters, and the number, the kinds and the bounds of the clipping objects. Each test makes
    // its map, so that it knows the shapes of the symbols. The tests of the derived values check that the map keeps the
    // palette separations until it changes, and that the readers at the same time get the same objects.
    [TestFixture]
    public class PaletteSeparationTests
    {
        const float Tolerance = 0.0001F;

        static PaletteSeparationTests()
        {
            // The Roboto fonts, which the text symbols use.
            RenderingTests.RegisterRobotoFonts();
        }

        [Test]
        public void EachPaletteColorHasOnePaletteSeparationInPaletteOrder()
        {
            Map map = CreateMap();
            SymColor brown, unused, blue, black;
            Symbol contour1, contour2, stream, track;
            using (map.Write()) {
                brown = map.AddColor("Brown", 1, 0, 0.56F, 1, 0.18F, false);
                unused = map.AddColor("Unused", 2, 0, 1, 0, 0, false);
                blue = map.AddColor("Blue", 3, 1, 0, 0, 0, false);
                black = map.AddColor("Black", 4, 0, 0, 0, 1, false);
                LineSymDef contourSymdef = AddLineSymdef(map, "Contour", brown, 0.14F);
                LineSymDef streamSymdef = AddLineSymdef(map, "Stream", blue, 0.18F);
                LineSymDef trackSymdef = AddLineSymdef(map, "Track", black, 0.25F);
                contour1 = AddLine(map, contourSymdef, new PointF(0, 0), new PointF(10, 0));
                contour2 = AddLine(map, contourSymdef, new PointF(0, 5), new PointF(10, 5));
                stream = AddLine(map, streamSymdef, new PointF(0, 2), new PointF(10, 3));
                track = AddLine(map, trackSymdef, new PointF(2, 0), new PointF(2, 10));
            }

            using (map.Read()) {
                IReadOnlyList<PaletteSeparation> separations = map.PaletteSeparations;

                // A palette color that no symbol draws has a palette separation too, with no palette color coverages
                // and no palette cutters.
                CollectionAssert.AreEqual(new SymColor[] { brown, unused, blue, black }, separations.Select(separation => separation.Color));
                CollectionAssert.AreEqual(new Symbol[] { contour1, contour2 }, separations[0].SymbolCoverages.Select(coverage => coverage.Symbol));
                CollectionAssert.IsEmpty(separations[1].SymbolCoverages);
                CollectionAssert.IsEmpty(separations[1].Cutters);
                CollectionAssert.AreEqual(new Symbol[] { stream }, separations[2].SymbolCoverages.Select(coverage => coverage.Symbol));
                CollectionAssert.AreEqual(new Symbol[] { track }, separations[3].SymbolCoverages.Select(coverage => coverage.Symbol));

                foreach (PaletteSeparation separation in separations) {
                    foreach (SymbolCoverage coverage in separation.SymbolCoverages) {
                        Assert.AreSame(separation.Color, coverage.Color);
                        Assert.AreSame(CoveragePrototype.FullCoverage, coverage.Prototype);
                        Assert.AreEqual(1, coverage.ClippingObjects.Count, "The clipping objects of {0}", coverage);
                    }

                    // The palette color coverages with the full coverage are one palette cutter, whose M is the
                    // identity.
                    if (separation.SymbolCoverages.Count > 0) {
                        PaletteCutter cutter = separation.Cutters.Single();
                        Assert.AreSame(CoveragePrototype.FullCoverage, cutter.Prototype);
                        Assert.IsTrue(cutter.Transformation.IsIdentity);
                        CollectionAssert.AreEqual(separation.SymbolCoverages, cutter.SymbolCoverages);
                    }
                }
            }

            Assert.IsTrue(CoveragePrototype.FullCoverage.Covers(new PointF(123, -45)));
        }

        [Test]
        public void PaletteColorCoveragesAreInTheOrderOfTheDrawing()
        {
            Map map = CreateMap();
            Symbol trackA, pathA, trackB;
            using (map.Write()) {
                SymColor black = map.AddColor("Black", 1, 0, 0, 0, 1, false);
                LineSymDef track = AddLineSymdef(map, "Track", black, 0.35F);
                LineSymDef path = AddLineSymdef(map, "Path", black, 0.25F);
                trackA = AddLine(map, track, new PointF(0, 0), new PointF(10, 0));
                pathA = AddLine(map, path, new PointF(0, 5), new PointF(10, 5));
                trackB = AddLine(map, track, new PointF(0, 10), new PointF(10, 10));
            }

            // DrawColor draws the symbols of each symbol definition in turn, in the order of the definitions, and the
            // symbols of a definition in the order in which the map got them.
            using (map.Read())
                CollectionAssert.AreEqual(new Symbol[] { trackA, trackB, pathA }, map.PaletteSeparations.Single().SymbolCoverages.Select(coverage => coverage.Symbol));
        }

        [Test]
        public void PaletteColorCoverageExistsWithoutClippingObjects()
        {
            Map map = CreateMap();
            TextSymbol emptyLabel;
            using (map.Write()) {
                SymColor black = map.AddColor("Black", 1, 0, 0, 0, 1, false);
                TextSymDef labelSymdef = new TextSymDef("Label", "1", TextSymDef.PreferredSymbolKind.NormalText, null);
                labelSymdef.SetFont("Roboto", 4, TextEffects.Regular, black, 4, 0, 0, 0, null, 0, 1, TextSymDefHorizAlignment.Left, TextSymDefVertAlignment.TopAscent);
                map.AddSymdef(labelSymdef);
                emptyLabel = new TextSymbol(labelSymdef, new string[] { "" }, new PointF(0, 0), 0, 0, TextSymDefHorizAlignment.Default, TextSymDefVertAlignment.Default);
                map.AddSymbol(emptyLabel);
            }

            using (map.Read()) {
                SymbolCoverage coverage = map.PaletteSeparations.Single().SymbolCoverages.Single();
                Assert.AreSame(emptyLabel, coverage.Symbol);
                CollectionAssert.IsEmpty(coverage.ClippingObjects);
            }
        }

        [Test]
        public void LineIsAStrokedPathWithTheLineOfItsSymbol()
        {
            Map map = CreateMap();
            using (map.Write()) {
                SymColor black = map.AddColor("Black", 1, 0, 0, 0, 1, false);
                LineSymDef track = new LineSymDef("Track", "1", black, 0.4F, LineJoinMode.Miter, LineCapMode.Flat);
                map.AddSymdef(track);
                AddLine(map, track, new PointF(0, 0), new PointF(10, 0), new PointF(10, 5));
            }

            StrokedShape line = (StrokedShape)GetClippingObjectsBySeparation(map).Single().Single();

            PathShape path = (PathShape)line.Shape;
            CollectionAssert.AreEqual(new GraphicsPathPartKind[] { GraphicsPathPartKind.Start, GraphicsPathPartKind.Lines }, path.Parts.Select(part => part.Kind));
            CollectionAssert.AreEqual(new PointF[] { new PointF(10, 0), new PointF(10, 5) }, path.Parts[1].Points);
            Assert.AreEqual(0.4F, line.Stroke.Width);
            Assert.AreEqual(LineCapMode.Flat, line.Stroke.Caps);
            Assert.AreEqual(LineJoinMode.Miter, line.Stroke.Join);
            Assert.AreEqual(GraphicsUtil.MITER_LIMIT, line.Stroke.MiterLimit);
            Assert.AreEqual(0, line.Placement.Steps.Count);
            // The miter joins reach the miter limit times half the width from the path.
            AssertBounds(RectangleF.FromLTRB(-1, -1, 11, 6), line.Bounds);
        }

        [Test]
        public void EachPartOfALineIsInItsPaletteColor()
        {
            Map map = CreateMap();
            SymColor yellow, brown, blue, black;
            Symbol roadSymbol;
            using (map.Write()) {
                yellow = map.AddColor("Yellow", 1, 0, 0.27F, 0.79F, 0, false);
                brown = map.AddColor("Brown", 2, 0, 0.56F, 1, 0.18F, false);
                blue = map.AddColor("Blue", 3, 1, 0, 0, 0, false);
                black = map.AddColor("Black", 4, 0, 0, 0, 1, false);

                LineSymDef road = new LineSymDef("Road", "1", black, 0.35F, LineJoinMode.Round, LineCapMode.Round);
                road.SetSecondLine(brown, 0.5F, LineJoinMode.Round, LineCapMode.Round);
                LineSymDef.DoubleLineInfo doubleLines = new LineSymDef.DoubleLineInfo();
                doubleLines.doubleFillColor = yellow;
                doubleLines.doubleThick = 1.0F;
                doubleLines.doubleLeftColor = blue;
                doubleLines.doubleLeftWidth = 0.1F;
                doubleLines.doubleRightColor = blue;
                doubleLines.doubleRightWidth = 0.1F;
                road.SetDoubleLines(doubleLines);
                map.AddSymdef(road);
                roadSymbol = AddLine(map, road, new PointF(0, 0), new PointF(10, 0));
            }

            // The symbol has a palette color coverage in each palette color of its parts.
            using (map.Read()) {
                foreach (PaletteSeparation separation in map.PaletteSeparations)
                    Assert.AreSame(roadSymbol, separation.SymbolCoverages.Single().Symbol);
            }

            List<ClippingObject>[] clippingObjects = GetClippingObjectsBySeparation(map);
            CollectionAssert.AreEqual(new float[] { 1.0F }, GetStrokeWidths(clippingObjects[0]));
            CollectionAssert.AreEqual(new float[] { 0.5F }, GetStrokeWidths(clippingObjects[1]));
            CollectionAssert.AreEqual(new float[] { 0.1F, 0.1F }, GetStrokeWidths(clippingObjects[2]));
            CollectionAssert.AreEqual(new float[] { 0.35F }, GetStrokeWidths(clippingObjects[3]));

            // The left and the right line are offset paths of the path (ticket 011), half the fill and half their width
            // away from it.
            CollectionAssert.AreEqual(new float[] { -0.55F, 0.55F }, clippingObjects[2].Select(border => ((OffsetPathShape)border.Shape).Offset));
            float[] borderOffsets = clippingObjects[2].Select(border => Math.Abs((border.Bounds.Top + border.Bounds.Bottom) / 2)).ToArray();
            Assert.AreEqual(0.55F, borderOffsets[0], Tolerance);
            Assert.AreEqual(0.55F, borderOffsets[1], Tolerance);
            Assert.AreNotEqual(clippingObjects[2][0].Bounds.Top, clippingObjects[2][1].Bounds.Top);
        }

        [Test]
        public void DashedLineIsItsDashesAlsoWhenItsGapsAreShorterThanAPixel()
        {
            Map map = CreateMap();
            using (map.Write()) {
                SymColor black = map.AddColor("Black", 1, 0, 0, 0, 1, false);
                LineSymDef footpath = new LineSymDef("Footpath", "1", black, 0.18F, LineJoinMode.Miter, LineCapMode.Flat);
                // The gaps of 0.01 mm are shorter than a pixel of a print at 1200 dpi, 0.021 mm. A drawing at that
                // resolution draws the line solid.
                footpath.SetDashInfo(new LineSymDef.DashInfo() { dashLength = 1, firstDashLength = 1, lastDashLength = 1, gapLength = 0.01F });
                map.AddSymdef(footpath);
                AddLine(map, footpath, new PointF(0, 0), new PointF(10, 0));
            }

            List<ClippingObject> clippingObjects = GetClippingObjectsBySeparation(map).Single();

            // The line is a composed dashed line (ticket 011): the end dashes, and between them a run of the 8 middle
            // dashes, whose pen has the pattern of a dash and a gap.
            Assert.AreEqual(3, clippingObjects.Count);
            float dashLength = (10 - 9 * 0.01F) / 10;
            IReadOnlyList<float> pattern = ((StrokedShape)clippingObjects[1]).Stroke.DashPattern;
            Assert.AreEqual(2, pattern.Count);
            Assert.AreEqual(dashLength, pattern[0], Tolerance);
            Assert.AreEqual(0.01F, pattern[1], Tolerance);
            Assert.IsNull(((StrokedShape)clippingObjects[0]).Stroke.DashPattern);
            Assert.IsNull(((StrokedShape)clippingObjects[2]).Stroke.DashPattern);

            // The line of 10 mm has 9 gaps of 0.01 mm, and 10 dashes of the same length in the rest of the line.
            List<ClippingObject> dashes = SymbolCoverageComparison.Expand(clippingObjects).Select(expanded => expanded.clippingObject).ToList();
            Assert.AreEqual(10, dashes.Count);
            float previousEnd = 0;
            for (int i = 0; i < dashes.Count; ++i) {
                StrokedShape dash = (StrokedShape)dashes[i];
                Assert.AreEqual(0.18F, dash.Stroke.Width, Tolerance);
                PathShape path = (PathShape)dash.Shape;
                PointF start = path.Parts[0].Points[0];
                PointF end = path.Parts[path.Parts.Count - 1].Points.Last();
                Assert.AreEqual((i == 0) ? 0 : 0.01F, start.X - previousEnd, Tolerance, "gap before dash {0}", i);
                Assert.AreEqual(dashLength, end.X - start.X, Tolerance, "length of dash {0}", i);
                Assert.AreEqual(0, start.Y);
                Assert.AreEqual(0, end.Y);
                previousEnd = end.X;
            }
            Assert.AreEqual(10, previousEnd, Tolerance);
        }

        [Test]
        public void GlyphOfAPointSymbolIsPlacedWithTheSymbol()
        {
            Map map = CreateMap();
            using (map.Write()) {
                SymColor brown = map.AddColor("Brown", 1, 0, 0.56F, 1, 0.18F, false);
                SymColor black = map.AddColor("Black", 2, 0, 0, 0, 1, false);

                Glyph glyph = new Glyph();
                glyph.AddLine(brown, new SymPath(new PointF[] { new PointF(-1, 0), new PointF(1, 0) }), 0.2F, LineJoinMode.Round, LineCapMode.Round);
                glyph.AddFilledCircle(black, new PointF(0, 0.5F), 0.4F);
                glyph.ConstructionComplete();
                PointSymDef knoll = new PointSymDef("Knoll", "1", glyph, true);
                map.AddSymdef(knoll);
                map.AddSymbol(new PointSymbol(knoll, new PointF(5, 3), 90, null));
            }

            List<ClippingObject>[] clippingObjects = GetClippingObjectsBySeparation(map);
            StrokedShape line = (StrokedShape)clippingObjects[0].Single();
            FilledShape dot = (FilledShape)clippingObjects[1].Single();

            // One transformation places both parts: it moves the glyph to the location and turns it by 90 degrees.
            AssertEqualPlacements(line.Placement, dot.Placement);
            Assert.AreEqual(1, line.Placement.Steps.Count);
            PointF[] placed = { new PointF(0, 0), new PointF(1, 0) };
            ((SinglePlacement)line.Placement).Matrix.TransformPoints(placed);
            Assert.AreEqual(5, placed[0].X, Tolerance);
            Assert.AreEqual(3, placed[0].Y, Tolerance);
            Assert.AreEqual(5, placed[1].X, Tolerance);
            Assert.AreEqual(4, placed[1].Y, Tolerance);

            Assert.IsInstanceOf<PathShape>(line.Shape);
            AssertBounds(RectangleF.FromLTRB(4.9F, 1.9F, 5.1F, 4.1F), line.Bounds);
            EllipseShape ellipse = (EllipseShape)dot.Shape;
            Assert.AreEqual(new PointF(0, 0.5F), ellipse.Center);
            Assert.AreEqual(0.2F, ellipse.RadiusX, Tolerance);
            AssertBounds(RectangleF.FromLTRB(4.3F, 2.8F, 4.7F, 3.2F), dot.Bounds);
        }

        [Test]
        public void CirclesOfAPointSymbolAreEllipsesAndArcs()
        {
            Map map = CreateMap();
            using (map.Write()) {
                SymColor black = map.AddColor("Black", 1, 0, 0, 0, 1, false);

                Glyph dot = new Glyph();
                dot.AddFilledCircle(black, new PointF(0, 0), 1.0F);
                dot.ConstructionComplete();
                PointSymDef dotSymdef = new PointSymDef("Dot", "1", dot, false);
                map.AddSymdef(dotSymdef);
                map.AddSymbol(new PointSymbol(dotSymdef, new PointF(-5, 2), 0, null));

                Glyph ring = new Glyph();
                ring.AddCircle(black, new PointF(0, 0), 0.1F, 2.0F);
                ring.ConstructionComplete();
                PointSymDef ringSymdef = new PointSymDef("Ring", "2", ring, false);
                map.AddSymdef(ringSymdef);
                // A gap from 180 to 360 degrees leaves the half of the ring from 0 to 180 degrees.
                map.AddSymbol(new PointSymbol(ringSymdef, new PointF(5, 2), 0, new float[] { 180, 360 }));
            }

            List<ClippingObject> clippingObjects = GetClippingObjectsBySeparation(map).Single();
            Assert.AreEqual(2, clippingObjects.Count);

            // A glyph of one circle at its origin is drawn at the location of the symbol, with no transformation.
            FilledShape dotShape = (FilledShape)clippingObjects[0];
            Assert.AreEqual(0, dotShape.Placement.Steps.Count);
            Assert.AreEqual(new PointF(-5, 2), ((EllipseShape)dotShape.Shape).Center);
            AssertBounds(RectangleF.FromLTRB(-5.5F, 1.5F, -4.5F, 2.5F), dotShape.Bounds);

            StrokedShape ringShape = (StrokedShape)clippingObjects[1];
            ArcShape arc = (ArcShape)ringShape.Shape;
            Assert.AreEqual(0.95F, arc.Radius, Tolerance);
            Assert.AreEqual(180, arc.SweepAngle, Tolerance);
            Assert.AreEqual(0.1F, ringShape.Stroke.Width, Tolerance);
            AssertBounds(RectangleF.FromLTRB(-0.95F, 0, 0.95F, 0.95F), arc.Bounds);
        }

        [Test]
        public void AreaHasItsFillBorderHatchingsAndAreaPatternInItsPaletteColorCoverages()
        {
            Map map = CreateMap();
            AreaSymbol forestSymbol;
            SymColor brown;
            using (map.Write()) {
                SymColor green = map.AddColor("Green", 1, 0.76F, 0, 0.91F, 0, false);
                SymColor blue = map.AddColor("Blue", 2, 1, 0, 0, 0, false);
                brown = map.AddColor("Brown", 3, 0, 0.56F, 1, 0.18F, false);
                SymColor black = map.AddColor("Black", 4, 0, 0, 0, 1, false);

                LineSymDef border = AddLineSymdef(map, "Border", black, 0.2F);
                AreaSymDef forest = new AreaSymDef("Forest", "2", green, border);
                forest.AddHatching(new AreaSymDef.HatchInfo() { hatchColor = blue, hatchWidth = 0.1F, hatchSpacing = 0.5F, hatchAngle = 45 });
                forest.AddHatching(new AreaSymDef.HatchInfo() { hatchColor = green, hatchWidth = 0.1F, hatchSpacing = 0.5F, hatchAngle = -45 });
                Glyph element = new Glyph();
                element.AddFilledCircle(brown, new PointF(0, 0), 0.3F);
                element.ConstructionComplete();
                forest.AddPattern(new AreaSymDef.PatternInfo() { patternGlyph = element, patternWidth = 1, patternHeight = 1 });
                map.AddSymdef(forest);
                forestSymbol = new AreaSymbol(forest, new SymPathWithHoles(CreateSquarePath(0, 0, 10), null), 0, new PointF());
                map.AddSymbol(forestSymbol);
            }

            // The hatchings and the area pattern are periodic coverages, and the area has a palette color coverage of
            // each of them (ticket 009). The green hatching is in the palette color of the area fill, after its full
            // coverage.
            using (map.Read()) {
                IReadOnlyList<PaletteSeparation> separations = map.PaletteSeparations;
                CollectionAssert.AreEqual(new int[] { 2, 1, 1, 1 }, separations.Select(separation => separation.SymbolCoverages.Count));
                foreach (PaletteSeparation separation in separations) {
                    foreach (SymbolCoverage coverage in separation.SymbolCoverages)
                        Assert.AreSame(forestSymbol, coverage.Symbol);
                }
                Assert.AreSame(CoveragePrototype.FullCoverage, separations[0].SymbolCoverages[0].Prototype);
                Assert.AreEqual(1, ((HatchingPrototype)separations[0].SymbolCoverages[1].Prototype).Index);
                Assert.AreEqual(0, ((HatchingPrototype)separations[1].SymbolCoverages[0].Prototype).Index);
                Assert.AreSame(brown, ((AreaPatternPrototype)separations[2].SymbolCoverages[0].Prototype).Color);
                Assert.AreSame(CoveragePrototype.FullCoverage, separations[3].SymbolCoverages[0].Prototype);
            }

            List<ClippingObject>[] clippingObjects = GetClippingObjectsBySeparation(map);
            FilledShape fill = (FilledShape)clippingObjects[0][0];
            Assert.IsInstanceOf<PathShape>(fill.Shape);
            Assert.AreEqual(AreaFillMode.Alternate, fill.FillRule);
            AssertBounds(RectangleF.FromLTRB(0, 0, 10, 10), fill.Bounds);

            // The clipping object of each hatching and of the area pattern is the area, as the area fill is.
            foreach (ClippingObject area in new ClippingObject[] { clippingObjects[0][1], clippingObjects[1].Single(), clippingObjects[2].Single() }) {
                Assert.AreEqual(AreaFillMode.Alternate, ((FilledShape)area).FillRule);
                AssertBounds(fill.Bounds, area.Bounds);
            }

            StrokedShape outline = (StrokedShape)clippingObjects[3].Single();
            Assert.AreEqual(0.2F, outline.Stroke.Width, Tolerance);
            Assert.IsTrue(outline.Bounds.Contains(fill.Bounds));
        }

        [Test]
        public void TextHasItsRunsUnderlineAndFraming()
        {
            Map map = CreateMap();
            TextSymbol label;
            using (map.Write()) {
                SymColor yellow = map.AddColor("Yellow", 1, 0, 0.27F, 0.79F, 0, false);
                SymColor blue = map.AddColor("Blue", 2, 1, 0, 0, 0, false);
                SymColor black = map.AddColor("Black", 3, 0, 0, 0, 1, false);

                TextSymDef labelSymdef = new TextSymDef("Label", "1", TextSymDef.PreferredSymbolKind.NormalText, null);
                labelSymdef.SetFont("Roboto", 4, TextEffects.Regular, black, 4, 0, 0, 0, null, 0, 1, TextSymDefHorizAlignment.Left, TextSymDefVertAlignment.TopAscent);
                labelSymdef.SetFraming(new TextSymDef.Framing() { framingStyle = TextSymDef.FramingStyle.Rectangle, framingColor = yellow, rectBorderLeft = 0.5F, rectBorderTop = 0.5F, rectBorderRight = 0.5F, rectBorderBottom = 0.5F });
                labelSymdef.SetUnderline(new TextSymDef.Underlining() { underlineOn = true, underlineColor = blue, underlineWidth = 0.2F, underlineDistance = 0.3F });
                map.AddSymdef(labelSymdef);
                label = new TextSymbol(labelSymdef, new string[] { "Two words" }, new PointF(0, 0), 0, 0, TextSymDefHorizAlignment.Default, TextSymDefVertAlignment.Default);
                map.AddSymbol(label);

                TextSymDef lineLabelSymdef = new TextSymDef("Line label", "2", TextSymDef.PreferredSymbolKind.LineText, null);
                lineLabelSymdef.SetFont("Roboto", 4, TextEffects.Regular, black, 4, 0, 0, 0, null, 0, 1, TextSymDefHorizAlignment.Left, TextSymDefVertAlignment.TopAscent);
                map.AddSymdef(lineLabelSymdef);
                map.AddSymbol(new LineTextSymbol(lineLabelSymdef, new SymPath(new PointF[] { new PointF(0, -20), new PointF(20, -10) }), "AB", TextSymDefHorizAlignment.Default, TextSymDefVertAlignment.Default));
            }

            List<ClippingObject>[] clippingObjects = GetClippingObjectsBySeparation(map);

            // Each word of the label is a text run, and each character of the line label is one at its own placement.
            List<FilledShape> runs = clippingObjects[2].Cast<FilledShape>().ToList();
            CollectionAssert.AreEqual(new string[] { "Two", "words", "A", "B" }, runs.Select(run => ((TextShape)run.Shape).Text));
            foreach (FilledShape run in runs) {
                TextShape text = (TextShape)run.Shape;
                Assert.AreEqual("Roboto", text.FontFamily);
                Assert.AreEqual(4, text.EmHeight);
                Assert.AreEqual(TextEffects.Regular, text.Effects);
                Assert.AreEqual(1, run.Placement.Steps.Count);
            }
            Assert.AreSame(runs[0].Placement, runs[1].Placement);
            Assert.AreNotSame(runs[2].Placement, runs[3].Placement);
            using (map.Read()) {
                Assert.IsTrue(label.BoundingBox.Contains(runs[0].Bounds));
                Assert.IsTrue(label.BoundingBox.Contains(runs[1].Bounds));
            }

            // The underline and the framing rectangle of the label are placed with its text.
            StrokedShape underline = (StrokedShape)clippingObjects[1].Single();
            Assert.AreEqual(0.2F, underline.Stroke.Width, Tolerance);
            AssertEqualPlacements(runs[0].Placement, underline.Placement);
            FilledShape framing = (FilledShape)clippingObjects[0].Single();
            Assert.IsInstanceOf<PathShape>(framing.Shape);
            AssertEqualPlacements(runs[0].Placement, framing.Placement);
            Assert.IsTrue(framing.Bounds.Contains(runs[0].Bounds));
            Assert.IsTrue(framing.Bounds.Contains(runs[1].Bounds));
        }

        [Test]
        public void PaletteSeparationsHoldTheVisibleSymbolsWhereverTheyLie()
        {
            Map map = CreateMap();
            SymColor brown, blue, red;
            Symbol nearContour, farContour, graphicsArea, graphicsLine;
            using (map.Write()) {
                brown = map.AddColor("Brown", 1, 0, 0.56F, 1, 0.18F, false);
                blue = map.AddColor("Blue", 2, 1, 0, 0, 0, false);
                red = map.AddColor("Red", 3, 0, 1, 1, 0, false);

                LineSymDef contour = AddLineSymdef(map, "Contour", brown, 0.14F);
                nearContour = AddLine(map, contour, new PointF(0, 0), new PointF(10, 0));
                // A symbol far from the others: 6.4 m away on the paper of the map.
                farContour = AddLine(map, contour, new PointF(5000, -4000), new PointF(5010, -4000));
                LineSymDef hiddenStream = AddLineSymdef(map, "Stream", blue, 0.18F);
                AddLine(map, hiddenStream, new PointF(0, 2), new PointF(10, 2));
                map.SetSymdefVisible(hiddenStream, false);

                // The graphics objects have their own palette colors, and the image and the layout objects are not
                // in a palette color.
                GraphicsSymDef graphics = new GraphicsSymDef();
                map.AddSymdef(graphics);
                graphicsArea = new GraphicsAreaSymbol(graphics, new SymPathWithHoles(CreateSquarePath(0, 0, 2), null), red);
                map.AddSymbol(graphicsArea);
                graphicsLine = new GraphicsLineSymbol(graphics, new SymPath(new PointF[] { new PointF(0, 4), new PointF(4, 4) }), red, 0.3F, LineJoinMode.Round, LineCapMode.Round);
                map.AddSymbol(graphicsLine);
                map.AddSymbol(new ImageAreaSymbol(map.GetImageSymDef(), new SymPathWithHoles(CreateSquarePath(3, 3, 2), null), CmykColor.FromCmyk(0, 0, 0, 0.5F), true, 0));
                map.AddSymbol(new ImageAreaSymbol(map.GetLayoutSymDef(), new SymPathWithHoles(CreateSquarePath(5, 5, 2), null), CmykColor.FromCmyk(0, 0, 0, 0.5F), true, 0));
            }

            using (map.Read()) {
                IReadOnlyList<PaletteSeparation> separations = map.PaletteSeparations;
                CollectionAssert.AreEqual(new SymColor[] { brown, blue, red }, separations.Select(separation => separation.Color));
                CollectionAssert.AreEqual(new Symbol[] { nearContour, farContour }, separations[0].SymbolCoverages.Select(coverage => coverage.Symbol));
                CollectionAssert.IsEmpty(separations[1].SymbolCoverages);
                CollectionAssert.AreEqual(new Symbol[] { graphicsArea, graphicsLine }, separations[2].SymbolCoverages.Select(coverage => coverage.Symbol));
            }

            List<ClippingObject>[] clippingObjects = GetClippingObjectsBySeparation(map);
            Assert.AreEqual(2, clippingObjects[0].Count);
            RectangleF farBounds = clippingObjects[0][1].Bounds;
            Assert.IsTrue(farBounds.Contains(new PointF(5000, -4000)) && farBounds.Contains(new PointF(5010, -4000)), "bounds of the far contour {0}", farBounds);
            Assert.AreEqual(2, clippingObjects[2].Count);
            Assert.IsInstanceOf<FilledShape>(clippingObjects[2][0]);
            Assert.IsInstanceOf<StrokedShape>(clippingObjects[2][1]);
            AssertBounds(RectangleF.FromLTRB(0, 0, 2, 2), clippingObjects[2][0].Bounds);
            AssertBounds(RectangleF.FromLTRB(-0.15F, 3.85F, 4.15F, 4.15F), clippingObjects[2][1].Bounds);
        }

        [Test]
        public void ReadsWithoutAChangeGetTheSameObjects()
        {
            LineMap lines = LineMap.Create();
            IReadOnlyList<PaletteSeparation> separations;
            IReadOnlyList<SymbolCoverage> coverages;
            IReadOnlyList<PaletteCutter> cutters;
            IReadOnlyList<ClippingObject> clippingObjects;
            using (lines.Map.Read()) {
                separations = lines.Map.PaletteSeparations;
                coverages = separations[0].SymbolCoverages;
                cutters = separations[0].Cutters;
                clippingObjects = coverages[0].ClippingObjects;
            }

            // A write that changes nothing keeps the derived values.
            using (lines.Map.Write())
                lines.Map.SetSymdefVisible(lines.Contour, true);

            using (lines.Map.Read()) {
                Assert.AreSame(separations, lines.Map.PaletteSeparations);
                Assert.AreSame(coverages, separations[0].SymbolCoverages);
                Assert.AreSame(cutters, separations[0].Cutters);
                Assert.AreSame(clippingObjects, coverages[0].ClippingObjects);
            }
        }

        // Each change of the map makes the derived values that the map made before it invalid, and the next read makes
        // them again.
        // change: the change.
        // separationCount: the number of the palette separations after the change.
        // coverageCount: the number of the palette color coverages of the first palette separation after the change.
        [TestCase("AddSymbol", 3, 3)]
        [TestCase("RemoveSymbol", 3, 1)]
        [TestCase("HideSymbolDefinition", 3, 0)]
        [TestCase("AddColor", 4, 2)]
        [TestCase("RemoveColor", 2, 2)]
        [TestCase("ChangeColor", 3, 2)]
        [TestCase("AddSymbolDefinition", 3, 2)]
        [TestCase("ChangeMetric", 3, 2)]
        public void AChangeMakesTheDerivedValuesInvalid(string change, int separationCount, int coverageCount)
        {
            LineMap lines = LineMap.Create();
            IReadOnlyList<PaletteSeparation> separations;
            PaletteSeparation separation;
            SymbolCoverage coverage;
            PaletteCutter cutter;
            using (lines.Map.Read()) {
                separations = lines.Map.PaletteSeparations;
                separation = separations[0];
                coverage = separation.SymbolCoverages[0];
                cutter = separation.Cutters[0];
            }

            using (lines.Map.Write())
                lines.Change(change);

            using (lines.Map.Read()) {
                Assert.Throws<MapUsageException>(() => GC.KeepAlive(separation.SymbolCoverages));
                Assert.Throws<MapUsageException>(() => GC.KeepAlive(separation.Cutters));
                Assert.Throws<MapUsageException>(() => GC.KeepAlive(cutter.SymbolCoverages));
                Assert.Throws<MapUsageException>(() => GC.KeepAlive(coverage.ClippingObjects));

                IReadOnlyList<PaletteSeparation> madeAgain = lines.Map.PaletteSeparations;
                Assert.AreNotSame(separations, madeAgain);
                Assert.AreSame(madeAgain, lines.Map.PaletteSeparations);
                Assert.AreEqual(separationCount, madeAgain.Count);
                Assert.AreEqual(coverageCount, madeAgain[0].SymbolCoverages.Count);
            }
        }

        [Test]
        public void AChangeWithinAWriteMakesThePaletteSeparationsAgain()
        {
            LineMap lines = LineMap.Create();
            using (lines.Map.Write()) {
                IReadOnlyList<PaletteSeparation> before = lines.Map.PaletteSeparations;
                Assert.AreSame(before, lines.Map.PaletteSeparations);
                Assert.AreEqual(2, before[0].SymbolCoverages.Count);

                lines.Change("AddSymbol");

                IReadOnlyList<PaletteSeparation> after = lines.Map.PaletteSeparations;
                Assert.AreNotSame(before, after);
                Assert.AreEqual(3, after[0].SymbolCoverages.Count);
                Assert.Throws<MapUsageException>(() => GC.KeepAlive(before[0].SymbolCoverages));
            }
        }

        [Test]
        public void DerivedValuesAreReadWithinMapRead()
        {
            LineMap lines = LineMap.Create();
            Assert.Throws<MapUsageException>(() => GC.KeepAlive(lines.Map.PaletteSeparations));

            SymbolCoverage coverage;
            using (lines.Map.Read())
                coverage = lines.Map.PaletteSeparations[0].SymbolCoverages[0];
            Assert.Throws<MapUsageException>(() => GC.KeepAlive(coverage.ClippingObjects));
        }

        [Test]
        public void ReadersAtTheSameTimeGetTheSameObjects()
        {
            const int readerCount = 8;
            LineMap lines = LineMap.Create(200);
            Barrier start = new Barrier(readerCount);
            IReadOnlyList<PaletteSeparation>[] separationsOfReaders = new IReadOnlyList<PaletteSeparation>[readerCount];
            List<object>[] valuesOfReaders = new List<object>[readerCount];
            Exception[] failures = new Exception[readerCount];

            Thread[] readers = Enumerable.Range(0, readerCount).Select(index => new Thread(() => {
                try {
                    using (lines.Map.Read()) {
                        start.SignalAndWait();
                        IReadOnlyList<PaletteSeparation> separations = lines.Map.PaletteSeparations;
                        List<object> values = new List<object>();
                        foreach (PaletteSeparation separation in separations) {
                            values.Add(separation.Cutters);
                            foreach (SymbolCoverage coverage in separation.SymbolCoverages)
                                values.Add(coverage.ClippingObjects);
                        }
                        separationsOfReaders[index] = separations;
                        valuesOfReaders[index] = values;
                    }
                }
                catch (Exception e) {
                    failures[index] = e;
                    start.RemoveParticipant();
                }
            })).ToArray();

            foreach (Thread reader in readers)
                reader.Start();
            foreach (Thread reader in readers)
                reader.Join();

            Assert.IsEmpty(failures.Where(failure => failure != null), "The readers failed.");
            for (int index = 1; index < readerCount; ++index) {
                Assert.AreSame(separationsOfReaders[0], separationsOfReaders[index], "the palette separations of reader {0}", index);
                Assert.AreEqual(valuesOfReaders[0].Count, valuesOfReaders[index].Count);
                for (int i = 0; i < valuesOfReaders[0].Count; ++i)
                    Assert.AreSame(valuesOfReaders[0][i], valuesOfReaders[index][i], "derived value {0} of reader {1}", i, index);
            }
        }

        // A map of three palette colors, brown, blue and one that no symbol draws, with contours in brown and a stream
        // in blue, which the tests of the derived values change.
        sealed class LineMap
        {
            public readonly Map Map;
            public readonly SymColor Brown;
            public readonly SymColor Blue;
            public readonly LineSymDef Contour;
            public readonly Symbol FirstContour;

            private LineMap(Map map, SymColor brown, SymColor blue, LineSymDef contour, Symbol firstContour)
            {
                Map = map;
                Brown = brown;
                Blue = blue;
                Contour = contour;
                FirstContour = firstContour;
            }

            // Make the map, with the given number of contours.
            // contourCount: the number of contours, at least two.
            public static LineMap Create(int contourCount = 2)
            {
                Map map = CreateMap();
                using (map.Write()) {
                    SymColor brown = map.AddColor("Brown", 1, 0, 0.56F, 1, 0.18F, false);
                    SymColor blue = map.AddColor("Blue", 2, 1, 0, 0, 0, false);
                    map.AddColor("Unused", 3, 0, 1, 0, 0, false);
                    LineSymDef contour = AddLineSymdef(map, "Contour", brown, 0.14F);
                    LineSymDef stream = AddLineSymdef(map, "Stream", blue, 0.18F);
                    Symbol firstContour = AddLine(map, contour, new PointF(0, 0), new PointF(10, 0));
                    for (int i = 1; i < contourCount; ++i)
                        AddLine(map, contour, new PointF(0, i), new PointF(10, i));
                    AddLine(map, stream, new PointF(0, -2), new PointF(10, -3));
                    return new LineMap(map, brown, blue, contour, firstContour);
                }
            }

            // Make a change of the map, within Map.Write.
            // change: the name of the change.
            public void Change(string change)
            {
                switch (change) {
                    case "AddSymbol":
                        AddLine(Map, Contour, new PointF(0, 20), new PointF(10, 20));
                        break;
                    case "RemoveSymbol":
                        Map.RemoveSymbol(FirstContour);
                        break;
                    case "HideSymbolDefinition":
                        Map.SetSymdefVisible(Contour, false);
                        break;
                    case "AddColor":
                        Map.AddColor("Green", 4, 0.76F, 0, 0.91F, 0, false);
                        break;
                    case "RemoveColor":
                        Map.RemoveColorAtIndex(2);
                        break;
                    case "ChangeColor":
                        Map.ChangeColorAtIndex(0, "Dark brown", 1, 0, 0.6F, 1, 0.3F, false);
                        break;
                    case "AddSymbolDefinition":
                        AddLineSymdef(Map, "Path", Brown, 0.25F);
                        break;
                    case "ChangeMetric":
                        Map.UseEuclideanMetric = !Map.UseEuclideanMetric;
                        break;
                    default:
                        throw new ArgumentException("Unknown change " + change, nameof(change));
                }
            }
        }

        // An empty map that measures text with Skia.
        static Map CreateMap()
        {
            return new Map(new Skia_TextMetrics(), null);
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

        // Add a solid line symbol with mitered joins and flat caps to a map.
        static LineSymDef AddLineSymdef(Map map, string name, SymColor color, float width)
        {
            LineSymDef symdef = new LineSymDef(name, (map.AllSymdefs.Count + 1).ToString(), color, width, LineJoinMode.Miter, LineCapMode.Flat);
            map.AddSymdef(symdef);
            return symdef;
        }

        // Add a line through points to a map.
        static Symbol AddLine(Map map, LineSymDef symdef, params PointF[] points)
        {
            LineSymbol symbol = new LineSymbol(symdef, new SymPath(points));
            map.AddSymbol(symbol);
            return symbol;
        }

        // The closed path of a square.
        static SymPath CreateSquarePath(float left, float bottom, float size)
        {
            return new SymPath(new PointF[] {
                new PointF(left, bottom), new PointF(left + size, bottom), new PointF(left + size, bottom + size),
                new PointF(left, bottom + size), new PointF(left, bottom) });
        }

        // The widths of the strokes of clipping objects, which are all stroked paths.
        static float[] GetStrokeWidths(IEnumerable<ClippingObject> clippingObjects)
        {
            return clippingObjects.Select(clippingObject => {
                StrokedShape stroked = (StrokedShape)clippingObject;
                Assert.IsTrue(stroked.Shape is PathShape || stroked.Shape is OffsetPathShape, "a path or an offset path");
                return stroked.Stroke.Width;
            }).ToArray();
        }

        // Assert that two placements have the same steps. The shapes of two palette color coverages have placements of
        // their own, since the symbols draw each palette color apart.
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
