using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using NUnit.Framework;
using PurplePen.Graphics2D;
using PurplePen.MapModel;
using SkiaSharp;

namespace Map_Skia.Tests
{
    // Tests of the area clusters of ticket 028, without the old drawing as the oracle of pass 2: the palette color
    // coverages of one palette cutter of a hatching or a clipped area pattern whose areas overlap or touch are one area
    // cluster (PaletteCutter.AreaClusters), the union of a cluster holds what its areas hold (AreaCluster.Union), and the
    // Skia encoding writes the pattern of a cluster once, in its union, so two such areas draw as one area whose outline is
    // their union. The cases of the drawings are those of OverlappingPatternsTests of fix/overlapping-patterns (7d2699b0),
    // which tests the same in the old drawing.
    [TestFixture]
    public class AreaClusterTests
    {
        // The part of the map drawn, in millimetres, and the width of the bitmap it is drawn into.
        static readonly RectangleF mapArea = new RectangleF(0, 0, 16, 16);
        const int bitmapWidth = 400;

        // How far a component of a pixel may differ between two drawings that a test compares as the same.
        const int ComponentTolerance = 1;

        // An area that a test draws: its outline with its holes, and the angle of the area symbol.
        sealed class TestArea
        {
            public readonly SymPathWithHoles Path;
            public readonly float Angle;

            // Make an area.
            // path: the outline, with its holes.
            // angle: the angle of the area symbol, in degrees.
            public TestArea(SymPathWithHoles path, float angle)
            {
                Path = path;
                Angle = angle;
            }
        }

        // A closed path through corners.
        // coordinates: the corners, x and y in turn.
        static SymPath ClosedPath(params float[] coordinates)
        {
            PointF[] points = new PointF[coordinates.Length / 2 + 1];
            for (int i = 0; i < coordinates.Length / 2; ++i)
                points[i] = new PointF(coordinates[2 * i], coordinates[2 * i + 1]);
            points[points.Length - 1] = points[0];
            return new SymPath(points);
        }

        // An area whose outline is a closed polygon.
        // angle: the angle of the area symbol, in degrees.
        // coordinates: the corners, x and y in turn.
        static TestArea Polygon(float angle, params float[] coordinates)
        {
            return new TestArea(new SymPathWithHoles(ClosedPath(coordinates), null), angle);
        }

        // An area whose outline is a rectangle.
        // angle: the angle of the area symbol, in degrees.
        // left, bottom, right, top: the sides of the rectangle.
        // clockwise: whether the corners run clockwise rather than counterclockwise.
        static TestArea Rectangle(float angle, float left, float bottom, float right, float top, bool clockwise = false)
        {
            if (clockwise)
                return Polygon(angle, left, bottom, left, top, right, top, right, bottom);
            else
                return Polygon(angle, left, bottom, right, bottom, right, top, left, top);
        }

        // An area of the angle 0 whose outline is a polygon with holes.
        // outline: the corners of the outline, x and y in turn.
        // holes: the corners of each hole, x and y in turn.
        static TestArea WithHoles(float[] outline, params float[][] holes)
        {
            return new TestArea(new SymPathWithHoles(ClosedPath(outline), holes.Select(hole => ClosedPath(hole)).ToArray()), 0);
        }

        // Give an area symbol definition a hatching of black lines 0.12 mm wide and 0.5 mm apart.
        // hatchAngle: the angle of the lines, in degrees.
        // rotateMode: whether the hatching turns with its area.
        static Action<AreaSymDef, SymColor> Hatching(float hatchAngle, AreaSymDef.RotateMode rotateMode)
        {
            return (AreaSymDef symdef, SymColor color) => {
                AreaSymDef.HatchInfo hatchInfo = new AreaSymDef.HatchInfo();
                hatchInfo.hatchColor = color;
                hatchInfo.hatchWidth = 0.12F;
                hatchInfo.hatchSpacing = 0.5F;
                hatchInfo.hatchOffset = 0;
                hatchInfo.hatchAngle = hatchAngle;
                hatchInfo.hatchRotateMode = rotateMode;
                symdef.AddHatching(hatchInfo);
            };
        }

        // Give an area symbol definition a clipped area pattern of black dots 0.3 mm across, in rows 0.8 mm apart, every
        // other row offset.
        // irregular: whether the area pattern moves each dot by an amount that depends on its lattice point.
        static Action<AreaSymDef, SymColor> Dots(bool irregular)
        {
            return (AreaSymDef symdef, SymColor color) => {
                Glyph glyph = new Glyph();
                glyph.AddFilledCircle(color, new PointF(0, 0), 0.3F);
                glyph.ConstructionComplete();

                AreaSymDef.PatternInfo patternInfo = new AreaSymDef.PatternInfo();
                patternInfo.offsetRows = true;
                patternInfo.patternWidth = 0.9F;
                patternInfo.patternHeight = 0.8F;
                patternInfo.patternAngle = 0;
                patternInfo.patternGlyph = glyph;
                patternInfo.patternRotateMode = AreaSymDef.RotateMode.Always;
                patternInfo.patternFillMode = AreaSymDef.PatternFillMode.Clip;
                patternInfo.irregular = irregular;
                patternInfo.irregularVarX = 0.5F;
                patternInfo.irregularVarY = 0.5F;
                symdef.AddPattern(patternInfo);
            };
        }

        // Make a map with one black palette color, and areas of area symbol definitions to which addPatterns gives their
        // hatchings and area patterns.
        // addPatterns: what each area symbol definition draws.
        // symbolOfArea: for each area, the index of its area symbol definition, or null for one definition of all the areas.
        // areas: the areas.
        static Map CreateMap(Action<AreaSymDef, SymColor> addPatterns, int[] symbolOfArea, params TestArea[] areas)
        {
            Map map = new Map(new Skia_TextMetrics(), null);

            using (map.Write()) {
                SymColor black = map.AddColor("Black", 1, 0, 0, 0, 1, false);
                List<AreaSymDef> symdefs = new List<AreaSymDef>();
                for (int i = 0; i < areas.Length; ++i) {
                    int symbol = (symbolOfArea == null) ? 0 : symbolOfArea[i];
                    while (symdefs.Count <= symbol) {
                        AreaSymDef symdef = new AreaSymDef("Area " + symdefs.Count, (301 + symdefs.Count).ToString(CultureInfo.InvariantCulture) + ".0", null, null);
                        addPatterns(symdef, black);
                        map.AddSymdef(symdef);
                        symdefs.Add(symdef);
                    }

                    map.AddSymbol(new AreaSymbol(symdefs[symbol], areas[i].Path, areas[i].Angle, new PointF()));
                }
            }

            return map;
        }

        // Draw the map area of a map into a bitmap, antialiased, with y pointing up, as the screen draws it.
        // map: the map.
        // usePatternBitmaps: whether the clipped area patterns are filled with pattern brushes rather than glyph by glyph.
        // drawColorsDirectly: whether the palette colors are drawn with the old drawing code rather than with pass 2.
        static SKBitmap Render(Map map, bool usePatternBitmaps, bool drawColorsDirectly = false)
        {
            int bitmapHeight = (int)Math.Round(bitmapWidth * mapArea.Height / mapArea.Width);
            SKBitmap bitmap = new SKBitmap(bitmapWidth, bitmapHeight);

            using (SKCanvas canvas = new SKCanvas(bitmap)) {
                canvas.Clear(SKColors.White);

                using (Skia_GraphicsTarget grTarget = new Skia_GraphicsTarget(canvas, false)) {
                    Matrix matrix = new Matrix();
                    matrix.Translate(0, bitmapHeight);
                    matrix.Scale(bitmapWidth / mapArea.Width, -bitmapHeight / mapArea.Height);
                    matrix.Translate(-mapArea.Left, -mapArea.Top);
                    grTarget.PushTransform(matrix);
                    grTarget.PushAntiAliasing(true);

                    RenderOptions renderOpts = new RenderOptions();
                    renderOpts.minResolution = mapArea.Width / bitmapWidth;
                    renderOpts.usePatternBitmaps = usePatternBitmaps;
                    renderOpts.renderTemplates = RenderTemplateOption.MapOnly;
                    renderOpts.drawColorsDirectly = drawColorsDirectly;
                    using (map.Read())
                        map.Draw(grTarget, mapArea, renderOpts, null);

                    grTarget.PopAntiAliasing();
                    grTarget.PopTransform();
                }
            }

            return bitmap;
        }

        // Count the pixels of two bitmaps of one size that differ by more than a tolerance in a component.
        // a, b: the bitmaps.
        // tolerance: the largest difference of a component that counts as none.
        static int CountDifferingPixels(SKBitmap a, SKBitmap b, int tolerance)
        {
            Assert.That(b.Width, Is.EqualTo(a.Width));
            Assert.That(b.Height, Is.EqualTo(a.Height));

            int count = 0;
            for (int y = 0; y < a.Height; ++y) {
                for (int x = 0; x < a.Width; ++x) {
                    SKColor ca = a.GetPixel(x, y), cb = b.GetPixel(x, y);
                    if (Math.Abs(ca.Red - cb.Red) > tolerance || Math.Abs(ca.Green - cb.Green) > tolerance ||
                        Math.Abs(ca.Blue - cb.Blue) > tolerance || Math.Abs(ca.Alpha - cb.Alpha) > tolerance)
                        ++count;
                }
            }

            return count;
        }

        // Count the pixels of a bitmap that are not white.
        // bitmap: the bitmap.
        static int CountInkedPixels(SKBitmap bitmap)
        {
            int count = 0;
            for (int y = 0; y < bitmap.Height; ++y) {
                for (int x = 0; x < bitmap.Width; ++x) {
                    if (bitmap.GetPixel(x, y) != SKColors.White)
                        ++count;
                }
            }

            return count;
        }

        // The area clusters of all the palette cutters of a map, in the order of the palette separations and their palette
        // cutters, as a check reads them under the read lock of the map.
        // map: the map, read.
        static List<AreaCluster> AreaClustersOf(Map map)
        {
            return map.PaletteSeparations.SelectMany(separation => separation.Cutters).SelectMany(cutter => cutter.AreaClusters).ToList();
        }

        // Check that the areas of one area symbol definition draw as one area whose outline is their union, and that the
        // same areas, each of a definition of its own that draws the same, draw differently: the palette cutters of the
        // definitions are apart, so each writes its pattern itself, twice where the areas overlap, and with a seam where
        // they touch and a pattern brush fills them.
        // addPatterns: what the area symbol definitions draw.
        // usePatternBitmaps: whether the clipped area patterns are filled with pattern brushes.
        // union: the area whose outline is the union of the areas.
        // differencesWhenSeparate: how many pixels at least differ when each area has a definition of its own: none where
        //   the areas only touch and a clip without antialiasing cuts their patterns.
        // areas: the areas, which overlap or touch.
        static void CheckDrawnAsUnion(Action<AreaSymDef, SymColor> addPatterns, bool usePatternBitmaps, TestArea union, int differencesWhenSeparate, params TestArea[] areas)
        {
            Map map = CreateMap(addPatterns, null, areas);
            using (map.Read()) {
                List<AreaCluster> clusters = AreaClustersOf(map);
                Assert.That(clusters.Count, Is.EqualTo(1), "the areas are one area cluster");
                Assert.That(clusters[0].SymbolCoverages.Count, Is.EqualTo(areas.Length));
            }

            using (SKBitmap expected = Render(CreateMap(addPatterns, null, union), usePatternBitmaps))
            using (SKBitmap drawn = Render(map, usePatternBitmaps)) {
                Assert.That(CountInkedPixels(expected), Is.GreaterThan(1000), "the union draws its pattern");
                Assert.That(CountDifferingPixels(expected, drawn, ComponentTolerance), Is.EqualTo(0), "the areas of one area symbol definition draw as their union");
            }

            int[] symbolOfArea = Enumerable.Range(0, areas.Length).ToArray();
            using (SKBitmap expected = Render(CreateMap(addPatterns, null, union), usePatternBitmaps))
            using (SKBitmap separately = Render(CreateMap(addPatterns, symbolOfArea, areas), usePatternBitmaps)) {
                Assert.That(CountDifferingPixels(expected, separately, ComponentTolerance), Is.GreaterThanOrEqualTo(differencesWhenSeparate),
                            "the areas of separate area symbol definitions write each pattern itself");
            }
        }

        // Check that areas draw with pass 2 exactly as the old drawing draws them (RenderOptions.drawColorsDirectly), each
        // area its own pattern: with a hatching and with dots, drawn glyph by glyph and with pattern brushes.
        // areas: the areas.
        static void CheckDrawnAsTheOldDrawing(params TestArea[] areas)
        {
            foreach (bool usePatternBitmaps in new bool[] { false, true }) {
                foreach (Action<AreaSymDef, SymColor> addPatterns in new Action<AreaSymDef, SymColor>[] { Hatching(30, AreaSymDef.RotateMode.Always), Dots(false) }) {
                    Map map = CreateMap(addPatterns, null, areas);
                    using (SKBitmap passTwo = Render(map, usePatternBitmaps))
                    using (SKBitmap old = Render(map, usePatternBitmaps, true)) {
                        Assert.That(CountInkedPixels(old), Is.GreaterThan(1000), "the old drawing draws the patterns");
                        Assert.That(CountDifferingPixels(old, passTwo, 0), Is.EqualTo(0), $"pass 2 draws as the old drawing, pattern bitmaps {usePatternBitmaps}");
                    }
                }
            }
        }

        // A path of Skia of path parts, with a fill type.
        // parts: the path parts.
        // fillType: the fill type.
        static SKPath ToSkPath(IEnumerable<GraphicsPathPart> parts, SKPathFillType fillType)
        {
            SKPath path = new SKPath();
            path.FillType = fillType;
            foreach (GraphicsPathPart part in parts) {
                switch (part.Kind) {
                    case GraphicsPathPartKind.Start:
                        path.MoveTo(part.Points[0].X, part.Points[0].Y);
                        break;
                    case GraphicsPathPartKind.Lines:
                        foreach (PointF point in part.Points)
                            path.LineTo(point.X, point.Y);
                        break;
                    case GraphicsPathPartKind.Beziers:
                        for (int i = 0; i + 2 < part.Points.Length; i += 3)
                            path.CubicTo(part.Points[i].X, part.Points[i].Y, part.Points[i + 1].X, part.Points[i + 1].Y, part.Points[i + 2].X, part.Points[i + 2].Y);
                        break;
                    case GraphicsPathPartKind.Close:
                        path.Close();
                        break;
                }
            }

            return path;
        }

        // Check, without a drawing, that a point is in the union of the areas of their area cluster by the nonzero rule
        // exactly when it is in one of the areas by the even-odd rule, at the points of a grid over the map area that lie on
        // no edge of the areas.
        // areas: the areas, which overlap or touch.
        static void CheckUnion(params TestArea[] areas)
        {
            Map map = CreateMap(Hatching(30, AreaSymDef.RotateMode.Always), null, areas);
            using (map.Read()) {
                List<AreaCluster> clusters = AreaClustersOf(map);
                Assert.That(clusters.Count, Is.EqualTo(1), "the areas are one area cluster");

                FilledShape union = (FilledShape)clusters[0].Union;
                Assert.That(union.FillRule, Is.EqualTo(AreaFillMode.Winding));
                Assert.That(union.Placement.Steps.Count, Is.EqualTo(0));

                List<SKPath> areaPaths = areas.Select(area => ToSkPath(((PathShape)area.Path.MakeFilledShape(Placement.Identity).Shape).Parts, SKPathFillType.EvenOdd)).ToList();
                using (SKPath unionPath = ToSkPath(((PathShape)union.Shape).Parts, SKPathFillType.Winding)) {
                    int inside = 0;
                    for (float x = 0.05F; x < mapArea.Right; x += 0.1F) {
                        for (float y = 0.05F; y < mapArea.Bottom; y += 0.1F) {
                            bool isInArea = areaPaths.Any(path => path.Contains(x, y));
                            Assert.That(unionPath.Contains(x, y), Is.EqualTo(isInArea), $"the union holds ({x}, {y}) as the areas hold it");
                            if (isInArea)
                                ++inside;
                        }
                    }
                    Assert.That(inside, Is.GreaterThan(1000), "the grid has points in the areas");
                }

                foreach (SKPath path in areaPaths)
                    path.Dispose();
            }
        }

        // Two areas that overlap, and their union.
        static readonly TestArea overlapLower = Rectangle(0, 2, 2, 10, 10);
        static readonly TestArea overlapUpper = Rectangle(0, 6, 6, 14, 14, true);
        static readonly TestArea overlapUnion = Polygon(0, 2, 2, 10, 2, 10, 6, 14, 6, 14, 14, 6, 14, 6, 10, 2, 10);

        // An area with a hole at its middle, which overlapUpper covers in part, and the union of the two.
        static readonly TestArea lowerWithHole = WithHoles(new float[] { 2, 2, 10, 2, 10, 10, 2, 10 }, new float[] { 4, 4, 4, 8, 8, 8, 8, 4 });
        static readonly TestArea lowerWithHoleUnion = WithHoles(new float[] { 2, 2, 10, 2, 10, 6, 14, 6, 14, 14, 6, 14, 6, 10, 2, 10 },
                                                                new float[] { 4, 4, 4, 8, 6, 8, 6, 6, 8, 6, 8, 4 });

        // An area whose hole lies outside its outline, where the even-odd rule fills it.
        static readonly TestArea leftWithHoleOutside = WithHoles(new float[] { 2, 2, 8, 2, 8, 14, 2, 14 }, new float[] { 10, 6, 12, 6, 12, 10, 10, 10 });

        [Test]
        public void HatchingOfOverlappingAreasIsDrawnOnce()
        {
            CheckDrawnAsUnion(Hatching(30, AreaSymDef.RotateMode.Always), false, overlapUnion, 100, overlapLower, overlapUpper);
        }

        [Test]
        public void HatchingOfTouchingAreasIsDrawnOnce()
        {
            CheckDrawnAsUnion(Hatching(60, AreaSymDef.RotateMode.Always), false, Rectangle(0, 2, 2, 14, 14), 0,
                              Rectangle(0, 2, 2, 8.02F, 14), Rectangle(0, 8.02F, 2, 14, 14, true));
        }

        [Test]
        public void HatchingTurnedWithItsAreasIsDrawnOnce()
        {
            CheckDrawnAsUnion(Hatching(0, AreaSymDef.RotateMode.Always), false, Polygon(35, 2, 2, 10, 2, 10, 6, 14, 6, 14, 14, 6, 14, 6, 10, 2, 10), 100,
                              Rectangle(35, 2, 2, 10, 10), Rectangle(35, 6, 6, 14, 14));
        }

        [Test]
        public void HatchingThatNeverTurnsIsDrawnOnceForAreasOfAnyAngle()
        {
            CheckDrawnAsUnion(Hatching(45, AreaSymDef.RotateMode.Never), false, overlapUnion, 100,
                              Rectangle(0, 2, 2, 10, 10), Rectangle(70, 6, 6, 14, 14));
        }

        [Test]
        public void PatternDrawnGlyphByGlyphIsDrawnOnce()
        {
            CheckDrawnAsUnion(Dots(false), false, overlapUnion, 100, overlapLower, overlapUpper);
        }

        [Test]
        public void IrregularPatternIsDrawnOnce()
        {
            CheckDrawnAsUnion(Dots(true), false, overlapUnion, 100, overlapLower, overlapUpper);
        }

        [Test]
        public void PatternFilledWithBrushIsDrawnOnce()
        {
            CheckDrawnAsUnion(Dots(false), true, overlapUnion, 100, overlapLower, overlapUpper);
        }

        [Test]
        public void PatternFilledWithBrushIsDrawnOnceWhereAreasTouch()
        {
            CheckDrawnAsUnion(Dots(false), true, Rectangle(0, 2, 2, 14, 14), 20, Rectangle(0, 2, 2, 8.02F, 14), Rectangle(0, 8.02F, 2, 14, 14));
        }

        [Test]
        public void AreaWithHoleOverlappingAnotherIsDrawnOnce()
        {
            CheckDrawnAsUnion(Hatching(30, AreaSymDef.RotateMode.Always), false, lowerWithHoleUnion, 100, lowerWithHole, overlapUpper);
        }

        [Test]
        public void AreaWithHoleOutsideItsOutlineIsDrawnOnce()
        {
            // The right area covers the hole of the left area.
            CheckDrawnAsUnion(Hatching(30, AreaSymDef.RotateMode.Always), false, Rectangle(0, 2, 2, 14, 14), 100, leftWithHoleOutside, Rectangle(0, 6, 2, 14, 14, true));
        }

        [Test]
        public void UnionHoldsWhatTheAreasHold()
        {
            CheckUnion(overlapLower, overlapUpper);
            CheckUnion(Rectangle(0, 2, 2, 8, 14), Rectangle(0, 8, 2, 14, 14, true));
            CheckUnion(lowerWithHole, overlapUpper);
            CheckUnion(leftWithHoleOutside, Rectangle(0, 9, 7, 13, 9));

            // An area whose hole holds an island, a second hole within the first, and an area that covers a part of the
            // hole and of the island.
            CheckUnion(WithHoles(new float[] { 1, 1, 13, 1, 13, 13, 1, 13 }, new float[] { 3, 3, 3, 11, 11, 11, 11, 3 }, new float[] { 5, 5, 9, 5, 9, 9, 5, 9 }),
                       Rectangle(0, 7, 7, 15, 15));
        }

        [Test]
        public void AreasWhoseBoundingBoxesOverlapOrTouchAreOneCluster()
        {
            // A chain of three areas, of which the first and the last do not meet, an area that touches the last of the
            // chain, and an area that meets none.
            Map map = CreateMap(Hatching(30, AreaSymDef.RotateMode.Always), null,
                                Rectangle(0, 1, 1, 5, 5), Rectangle(0, 1, 12, 3, 14), Rectangle(0, 4, 4, 8, 8), Rectangle(0, 7, 7, 11, 11),
                                Rectangle(0, 11, 7, 13, 9));
            using (map.Read()) {
                List<AreaCluster> clusters = AreaClustersOf(map);
                Assert.That(clusters.Count, Is.EqualTo(2));

                List<SymbolCoverage> coverages = map.PaletteSeparations.Single().SymbolCoverages.ToList();
                Assert.That(clusters[0].SymbolCoverages, Is.EqualTo(new SymbolCoverage[] { coverages[0], coverages[2], coverages[3], coverages[4] }),
                            "the cluster of the chain holds its palette color coverages in the order of the palette separation");
                Assert.That(clusters[1].SymbolCoverages, Is.EqualTo(new SymbolCoverage[] { coverages[1] }), "the area that meets none is a cluster of its own");
                Assert.That(clusters[0].Bounds, Is.EqualTo(RectangleF.FromLTRB(1, 1, 13, 11)));
                Assert.That(clusters[1].Union, Is.SameAs(coverages[1].ClippingObjects[0]), "the union of a cluster of one area is the area");
            }
        }

        [Test]
        public void AreaWhoseContoursMeetIsAClusterOfItsOwn()
        {
            // An area whose outline crosses itself, an area that covers a part of it, and an area that overlaps the second.
            Map map = CreateMap(Hatching(30, AreaSymDef.RotateMode.Always), null,
                                Polygon(0, 2, 2, 10, 10, 10, 2, 2, 10), Rectangle(0, 8, 2, 14, 10), Rectangle(0, 12, 8, 15, 14));
            using (map.Read()) {
                List<AreaCluster> clusters = AreaClustersOf(map);
                List<SymbolCoverage> coverages = map.PaletteSeparations.Single().SymbolCoverages.ToList();
                Assert.That(clusters.Count, Is.EqualTo(2));
                Assert.That(clusters[0].SymbolCoverages, Is.EqualTo(new SymbolCoverage[] { coverages[0] }));
                Assert.That(clusters[1].SymbolCoverages, Is.EqualTo(new SymbolCoverage[] { coverages[1], coverages[2] }));
            }
        }

        [Test]
        public void AreasThatMeetNoOtherAreDrawnAsTheOldDrawingDrawsThem()
        {
            CheckDrawnAsTheOldDrawing(Rectangle(0, 1, 1, 5, 5), Rectangle(0, 9, 9, 13, 13));
        }

        [Test]
        public void AreasWhoseContoursMeetAreDrawnAsTheOldDrawingDrawsThem()
        {
            // An area whose hole crosses its outline, and an area that covers the part of the hole outside the outline,
            // which the even-odd rule fills.
            CheckDrawnAsTheOldDrawing(WithHoles(new float[] { 2, 2, 10, 2, 10, 10, 2, 10 }, new float[] { 8, 4, 12, 4, 12, 8, 8, 8 }), Rectangle(0, 10, 2, 14, 10));

            // An area whose outline crosses itself, and an area that covers a part of it.
            CheckDrawnAsTheOldDrawing(Polygon(0, 2, 2, 10, 10, 10, 2, 2, 10), Rectangle(0, 8, 2, 14, 10));
        }

        [Test]
        public void AreasOfDifferentConfigurationsAreWrittenApart()
        {
            // Two overlapping areas whose hatching turns with them, at different angles: their transformations M differ by
            // a turn, which is no symmetry of the hatching, so they are in two palette cutters.
            TestArea[] areas = { Rectangle(0, 2, 2, 10, 10), Rectangle(70, 6, 6, 14, 14) };
            Map map = CreateMap(Hatching(30, AreaSymDef.RotateMode.Always), null, areas);
            using (map.Read())
                Assert.That(AreaClustersOf(map).Count, Is.EqualTo(2));

            CheckDrawnAsTheOldDrawing(areas);
        }
    }
}
