using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using PurplePen.Graphics2D;
using PurplePen.MapModel;
using SkiaSharp;

namespace Map_Skia.Tests
{
    // The structure of pass 2 and of the Skia encoding (ticket 010), checked without the old drawing. Pass 2 does not
    // call the drawing code of the symbols, it gives the encoding one transparency group for each palette color in
    // palette order, the color range and the rectangle of the drawing limit the groups and their content, and the
    // overprint mode gives each palette color its blend mode. The Skia encoding composites a group once with its
    // opacity and its blend mode, writes a dashed line solid when its gaps are shorter than the resolution of the
    // drawing, places a pattern brush by M, and draws the same in a recorded drawing as in a direct drawing. Each test
    // makes its map.
    [TestFixture]
    public class PaletteStackTests
    {
        const float Tolerance = 0.0001F;

        static PaletteStackTests()
        {
            // RenderingTests adds the Roboto fonts, which the text symbols use.
            System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(RenderingTests).TypeHandle);
        }

        [Test]
        public void Pass2DoesNotCallTheDrawingCodeOfTheSymbols()
        {
            Map map = CreateMapOfEachKindOfSymbol();
            RecordingEncoding encoding = new RecordingEncoding();
            IEncodingTarget target = CountingTarget.Create(encoding);
            using (map.Read())
                map.Draw(target, RectangleF.FromLTRB(-50, -50, 50, 50), CreateOptions(0.1F, OverprintMode.None), null);

            // The drawing code of a symbol draws with the calls of its graphics target. Pass 2 makes none of them: the
            // encoding writes the palette colors.
            CollectionAssert.IsEmpty(((CountingTarget)(object)target).Calls, "the calls of the graphics target");

            // The encoding gets one transparency group for each palette color, in palette order, with the palette color
            // coverages of its palette separation.
            using (map.Read()) {
                IReadOnlyList<PaletteSeparation> separations = map.PaletteSeparations;
                Assert.AreEqual(separations.Count, encoding.Groups.Count, "groups");
                for (int i = 0; i < separations.Count; ++i) {
                    Assert.AreSame(separations[i].Color, encoding.Groups[i].Color, "the palette color of group {0}", i);
                    CollectionAssert.AreEqual(separations[i].SymbolCoverages, encoding.Contents[i], "the content of group {0}", i);
                }
                CollectionAssert.AreEqual(map.AllColors, encoding.Groups.Select(group => group.Color), "palette order");
            }
        }

        [Test]
        public void ColorRangeAndRectangleLimitTheGroupsAndTheirContent()
        {
            Map map = CreateMap();
            Symbol inside, outside;
            using (map.Write()) {
                SymColor brown = map.AddColor("Brown", 1, 0, 0.56F, 1, 0.18F, false);
                SymColor blue = map.AddColor("Blue", 2, 1, 0, 0, 0, false);
                SymColor black = map.AddColor("Black", 3, 0, 0, 0, 1, false);
                AddLine(map, AddLineSymdef(map, "Contour", brown, 0.14F), new PointF(0, 0), new PointF(10, 0));
                LineSymDef stream = AddLineSymdef(map, "Stream", blue, 0.18F);
                inside = AddLine(map, stream, new PointF(0, 5), new PointF(10, 5));
                outside = AddLine(map, stream, new PointF(0, 50), new PointF(10, 50));
                AddLine(map, AddLineSymdef(map, "Path", black, 0.25F), new PointF(0, 10), new PointF(10, 10));
            }

            // The color range after the palette color 1 to the palette color 2 is the blue alone, and the rectangle
            // shows the stream inside it and not the stream outside it.
            RenderOptions options = CreateOptions(0.1F, OverprintMode.None);
            options.colorBeginDrawExclusive = 1;
            options.colorEndDrawInclusive = 2;
            RecordingEncoding encoding = new RecordingEncoding();
            using (map.Read())
                map.Draw(CountingTarget.Create(encoding), RectangleF.FromLTRB(-5, -5, 15, 20), options, null);

            TransparencyGroup group = encoding.Groups.Single();
            Assert.AreEqual("Blue", group.Color.Name);
            List<Symbol> shown = encoding.Contents[0].Select(coverage => coverage.Symbol).ToList();
            CollectionAssert.AreEqual(new Symbol[] { inside }, shown);
            CollectionAssert.DoesNotContain(shown, outside);
        }

        [TestCase(OverprintMode.None, ColorBlendMode.Normal, ColorBlendMode.Normal, ColorBlendMode.Normal)]
        [TestCase(OverprintMode.ByPaletteColor, ColorBlendMode.Multiply, ColorBlendMode.Normal, ColorBlendMode.Darken)]
        [TestCase(OverprintMode.Darken, ColorBlendMode.Normal, ColorBlendMode.Darken, ColorBlendMode.Darken)]
        [TestCase(OverprintMode.Multiply, ColorBlendMode.Normal, ColorBlendMode.Multiply, ColorBlendMode.Multiply)]
        public void OverprintModeGivesTheBlendModesOfThePaletteColors(OverprintMode mode, ColorBlendMode ofNormal, ColorBlendMode ofOverprint, ColorBlendMode ofOverprintDarken)
        {
            // A palette color without the overprint flag and with the blend mode Multiply, an overprint color with the
            // blend mode Normal and the opacity 0.5, and an overprint color with the blend mode Darken.
            Map map = CreateMap();
            using (map.Write()) {
                SymColor normal = map.AddColor(new SymColor("Green", 1, 0.76F, 0, 0.91F, 0, false) { BlendMode = ColorBlendMode.Multiply });
                SymColor overprint = map.AddColor(new SymColor("Blue", 2, 1, 0, 0, 0, true) { Opacity = 0.5F });
                SymColor overprintDarken = map.AddColor(new SymColor("Black", 3, 0, 0, 0, 1, true) { BlendMode = ColorBlendMode.Darken });
                foreach (SymColor color in new SymColor[] { normal, overprint, overprintDarken })
                    AddLine(map, AddLineSymdef(map, color.Name, color, 0.2F), new PointF(0, 0), new PointF(10, 0));
            }

            RecordingEncoding encoding = new RecordingEncoding();
            using (map.Read())
                map.Draw(CountingTarget.Create(encoding), RectangleF.FromLTRB(-5, -5, 15, 5), CreateOptions(0.1F, mode), null);

            CollectionAssert.AreEqual(new ColorBlendMode[] { ofNormal, ofOverprint, ofOverprintDarken }, encoding.Groups.Select(group => group.BlendMode));
            // The opacity α of a palette color applies in each mode.
            CollectionAssert.AreEqual(new float[] { 1, 0.5F, 1 }, encoding.Groups.Select(group => group.Opacity));
        }

        [Test]
        public void OverlapOfTwoCoveragesOfAPaletteColorGetsItsOpacityOnce()
        {
            // A palette color with the opacity 0.5 and two overlapping areas of it, and the same color with the opacity 1
            // apart from them.
            Map map = CreateMap();
            using (map.Write()) {
                SymColor gray = map.AddColor(new SymColor("Gray", 1, 0, 0, 0, 1, false) { Opacity = 0.5F });
                SymColor opaque = map.AddColor("Opaque", 2, 0, 0, 0, 1, false);
                AreaSymDef fill = AddArea(map, "Fill", gray, 0, 0);
                map.AddSymbol(new AreaSymbol(fill, new SymPathWithHoles(CreateSquarePath(2, 2, 4), null), 0, new PointF()));
                AddArea(map, "Opaque", opaque, 8, 0);
            }

            // Figure 6 of the specification: the area that both coverages cover gets α once, as the area that one of
            // them covers does, and α makes it lighter than the opaque color.
            Rendering rendering = Rendering.Of(map, RectangleF.FromLTRB(-1, -1, 13, 7), 0.1F, false, CreateOptions(0.1F, OverprintMode.None));
            SKColor alone = rendering.At(1, 1);
            Assert.AreEqual(alone, rendering.At(3, 3), "the overlap");
            Assert.Greater(alone.Red, rendering.At(10, 2).Red, "lighter than the opaque color");
            Assert.Less(alone.Red, rendering.At(7, 1).Red, "darker than the white");
        }

        [Test]
        public void PaletteColorsAreCompositedInPaletteOrder()
        {
            Map map = CreateMap();
            using (map.Write()) {
                SymColor blue = map.AddColor("Blue", 1, 1, 0, 0, 0, false);
                SymColor black = map.AddColor("Black", 2, 0, 0, 0, 1, false);
                AddArea(map, "Black", black, 2, 2);
                AddArea(map, "Blue", blue, 0, 0);
            }

            // The black is above the blue in palette order, so it covers the blue where both are, though the blue area
            // is the later symbol.
            Rendering rendering = Rendering.Of(map, RectangleF.FromLTRB(-1, -1, 7, 7), 0.1F, false, CreateOptions(0.1F, OverprintMode.None));
            Assert.AreEqual(rendering.At(5, 5), rendering.At(3, 3), "the black where both palette colors are");
            Assert.AreNotEqual(rendering.At(1, 1), rendering.At(3, 3), "the blue alone");
        }

        [TestCase(1F)]
        [TestCase(0.5F)]
        public void DarkenTakesTheMinimumAndMultiplyTheProductOfEachComponent(float opacity)
        {
            // A lower palette color, and an overprint color above it that covers a part of it, each with two areas, so
            // that the overprint color overlaps itself where the opacity must apply once.
            Map map = CreateMap();
            using (map.Write()) {
                SymColor cyan = map.AddColor("Cyan", 1, 1, 0, 0, 0, false);
                SymColor magenta = map.AddColor(new SymColor("Magenta", 2, 0, 1, 0, 0.2F, true) { Opacity = opacity });
                AddArea(map, "Cyan", cyan, 0, 0);
                AreaSymDef overprint = AddArea(map, "Magenta", magenta, 2, 2);
                map.AddSymbol(new AreaSymbol(overprint, new SymPathWithHoles(CreateSquarePath(3, 3, 4), null), 0, new PointF()));
            }

            RectangleF area = RectangleF.FromLTRB(-1, -1, 8, 8);
            Rendering darken = Rendering.Of(map, area, 0.1F, false, CreateOptions(0.1F, OverprintMode.Darken));
            Rendering multiply = Rendering.Of(map, area, 0.1F, false, CreateOptions(0.1F, OverprintMode.Multiply));
            Rendering opaque = Rendering.Of(map, area, 0.1F, false, CreateOptions(0.1F, OverprintMode.None));

            // The cyan alone, and the magenta alone over the white, which each mode leaves as the magenta over the white,
            // also where the two areas of the magenta overlap.
            SKColor lower = darken.At(1, 1);
            // The blend modes round each component to within 1 of each other.
            SKColor upperOverWhite = darken.At(6.5F, 6.5F);
            AssertNear(upperOverWhite, multiply.At(6.5F, 6.5F), "Multiply");
            AssertNear(upperOverWhite, opaque.At(6.5F, 6.5F), "Normal");
            AssertNear(upperOverWhite, darken.At(5, 5), "Darken where the two areas of the magenta overlap");
            AssertNear(upperOverWhite, multiply.At(5, 5), "Multiply where the two areas of the magenta overlap");

            // The magenta at full opacity over the white, which gives the components of its color.
            float[] upper = Components(upperOverWhite).Select(component => 255 - (255 - component) / opacity).ToArray();

            for (int i = 0; i < 3; ++i) {
                float lowerComponent = Components(lower)[i];
                // Darken takes the minimum of each component, and Multiply their product. The opacity mixes the result
                // with the backdrop once, as SaveLayer composites the group.
                float darkened = (1 - opacity) * lowerComponent + opacity * Math.Min(lowerComponent, upper[i]);
                float multiplied = (1 - opacity) * lowerComponent + opacity * lowerComponent * upper[i] / 255F;
                Assert.AreEqual(darkened, Components(darken.At(2.5F, 2.5F))[i], 3, "Darken, component {0}", i);
                Assert.AreEqual(darkened, Components(darken.At(3.5F, 3.5F))[i], 3, "Darken over both areas of the magenta, component {0}", i);
                Assert.AreEqual(multiplied, Components(multiply.At(2.5F, 2.5F))[i], 3, "Multiply, component {0}", i);
                Assert.AreEqual(multiplied, Components(multiply.At(3.5F, 3.5F))[i], 3, "Multiply over both areas of the magenta, component {0}", i);
            }
        }

        [Test]
        public void DashedLineIsSolidWhenItsGapsAreShorterThanTheResolution()
        {
            // A dashed line, a double line whose borders are dashed, and an area whose border is the dashed line, each
            // with gaps of 0.01 mm.
            Map map = CreateMap();
            using (map.Write()) {
                SymColor black = map.AddColor("Black", 1, 0, 0, 0, 1, false);
                SymColor green = map.AddColor("Green", 2, 0.76F, 0, 0.91F, 0, false);
                LineSymDef.DashInfo dashes = new LineSymDef.DashInfo() { dashLength = 1, firstDashLength = 1, lastDashLength = 1, gapLength = 0.01F };
                LineSymDef footpath = new LineSymDef("Footpath", "1", black, 0.18F, LineJoinMode.Miter, LineCapMode.Flat);
                footpath.SetDashInfo(dashes);
                map.AddSymdef(footpath);
                AddLine(map, footpath, new PointF(0, 0), new PointF(10, 0));

                LineSymDef road = new LineSymDef("Road", "2", null, 0, LineJoinMode.Miter, LineCapMode.Flat);
                road.SetDoubleLines(new LineSymDef.DoubleLineInfo() {
                    doubleThick = 0.6F, doubleLeftColor = black, doubleLeftWidth = 0.1F, doubleRightColor = black, doubleRightWidth = 0.1F,
                    doubleLeftDashed = true, doubleRightDashed = true, doubleDashes = dashes });
                map.AddSymdef(road);
                AddLine(map, road, new PointF(0, 5), new PointF(10, 5));

                AreaSymDef garden = new AreaSymDef("Garden", "3", green, footpath);
                map.AddSymdef(garden);
                map.AddSymbol(new AreaSymbol(garden, new SymPathWithHoles(CreateSquarePath(0, 10, 10), null), 0, new PointF()));
            }

            using (map.Read()) {
                List<SymbolCoverage> coverages = map.PaletteSeparations.SelectMany(separation => separation.SymbolCoverages).ToList();
                List<SymbolCoverage> black = coverages.Where(coverage => coverage.Color.Name == "Black").ToList();
                Assert.AreEqual(3, black.Count, "the line, the double line and the area border");

                foreach (SymbolCoverage coverage in black) {
                    // At a resolution of 0.005 mm, the gaps are longer than a pixel, and the line components are dashed.
                    // At 0.05 mm, they are shorter, and each line component is one solid line, as the old drawing draws it.
                    List<StrokedShape> dashed = coverage.GetClippingObjects(0.005F).Cast<StrokedShape>().ToList();
                    List<StrokedShape> solid = coverage.GetClippingObjects(0.05F).Cast<StrokedShape>().ToList();
                    string symbol = coverage.Symbol.Definition.Name;
                    Assert.Greater(dashed.Count + dashed.Count(stroked => stroked.Stroke.DashPattern != null), solid.Count, "{0} is dashed at 0.005 mm", symbol);
                    Assert.IsTrue(solid.All(stroked => stroked.Stroke.DashPattern == null), "{0} has no run of dashes at 0.05 mm", symbol);
                    Assert.AreEqual((symbol == "Road") ? 2 : 1, solid.Count, "{0} is solid at 0.05 mm", symbol);
                }

                // The border of the double line stays an offset path of the base path.
                SymbolCoverage road = black.Single(coverage => coverage.Symbol.Definition.Name == "Road");
                Assert.IsTrue(road.GetClippingObjects(0.05F).All(clippingObject => clippingObject.Shape is OffsetPathShape), "the borders are offset paths");
                CollectionAssert.AreEqual(road.ClippingObjects, road.GetClippingObjects(0), "the resolution 0 gives the clipping objects of the coverage");
            }
        }

        [Test]
        public void PatternBrushIsPlacedByMAlsoWithTheRotationCenterAtTheAngle0()
        {
            // A regular clipped area pattern of dots, in an area whose angle is 0 and whose rotation center is not the
            // origin, so that M moves the pattern by the rotation center.
            Map map = CreateMap();
            AreaSymbol area;
            using (map.Write()) {
                SymColor black = map.AddColor("Black", 1, 0, 0, 0, 1, false);
                Glyph dot = new Glyph();
                dot.AddFilledCircle(black, new PointF(0, 0), 0.4F);
                dot.ConstructionComplete();
                AreaSymDef dots = new AreaSymDef("Dots", "1", null, null);
                dots.AddPattern(new AreaSymDef.PatternInfo() { patternGlyph = dot, patternWidth = 1, patternHeight = 1 });
                map.AddSymdef(dots);
                area = new AreaSymbol(dots, new SymPathWithHoles(CreateSquarePath(0, 0, 10), null), 0, new PointF(0.37F, 0.21F));
                map.AddSymbol(area);
            }

            using (map.Read()) {
                SymbolCoverage coverage = map.PaletteSeparations.Single().SymbolCoverages.Single();
                CollectionAssert.AreEqual(new float[] { 1, 0, 0, 1, 0.37F, 0.21F }, coverage.Transformation.Elements, "M");
            }

            // The brush and the drawing glyph by glyph, which places each element by M, cover the same dots: they differ
            // only in the antialiasing of the edges of the dots, by less than half the coverage of a pixel.
            RectangleF window = RectangleF.FromLTRB(2, 2, 8, 8);
            RenderOptions withBrush = CreateOptions(0.05F, OverprintMode.None);
            withBrush.usePatternBitmaps = true;
            RenderOptions glyphByGlyph = CreateOptions(0.05F, OverprintMode.None);
            glyphByGlyph.usePatternBitmaps = false;
            Rendering brush = Rendering.Of(map, window, 0.05F, true, withBrush);
            Rendering glyphs = Rendering.Of(map, window, 0.05F, true, glyphByGlyph);

            int covered = glyphs.Pixels.Count(pixel => pixel.Red < 128);
            int differing = Enumerable.Range(0, glyphs.Pixels.Length).Count(i => Math.Abs(brush.Pixels[i].Red - glyphs.Pixels[i].Red) > 128);
            Assert.Greater(covered, 1000, "dots in the window");
            Assert.Less(differing, covered / 100, "pixels that only one of the two covers");
        }

        [Test]
        public void RecordedDrawingPlaysBackAsTheDirectDrawing()
        {
            // A palette color with the opacity 0.5, which pass 2 applies, the same color with the opacity 1 apart from
            // it, and a line.
            Map map = CreateMap();
            using (map.Write()) {
                SymColor gray = map.AddColor(new SymColor("Gray", 1, 0, 0, 0, 1, false) { Opacity = 0.5F });
                SymColor opaque = map.AddColor("Opaque", 2, 0, 0, 0, 1, false);
                SymColor blue = map.AddColor("Blue", 3, 1, 0, 0, 0, false);
                AddArea(map, "Gray", gray, 0, 0);
                AddArea(map, "Opaque", opaque, 8, 0);
                AddLine(map, AddLineSymdef(map, "Stream", blue, 0.3F), new PointF(-1, 3), new PointF(13, 3));
            }

            RectangleF area = RectangleF.FromLTRB(-1, -1, 13, 7);
            RenderOptions options = CreateOptions(0.1F, OverprintMode.None);
            Rendering direct = Rendering.Of(map, area, 0.1F, true, options);

            RecordingGraphicsTarget recorder = new RecordingGraphicsTarget(new SkiaBitmapGraphicsLoader());
            using (recorder) {
                recorder.PushAntiAliasing(true);
                using (map.Read())
                    map.Draw(recorder, area, options, null);
                recorder.EndRecording();

                // The playback writes the palette colors with the encoding of the target that plays the recording back,
                // which applies the opacity.
                Rendering playedBack = Rendering.Of(area, 0.1F, target => recorder.Playback(target));
                CollectionAssert.AreEqual(direct.Pixels, playedBack.Pixels, "the recorded drawing");
                Assert.Greater(playedBack.At(1, 1).Red, playedBack.At(10, 1).Red, "the opacity of the gray");

                // The recorded drawing reads the map at the playback, so it shows a change made after the recording.
                using (map.Write())
                    AddLine(map, (LineSymDef)map.AllSymdefs.Single(symdef => symdef.Name == "Stream"), new PointF(-1, 5), new PointF(13, 5));
                Rendering changed = Rendering.Of(map, area, 0.1F, true, options);
                Rendering playedBackAfterTheChange = Rendering.Of(area, 0.1F, target => recorder.Playback(target));
                CollectionAssert.AreEqual(changed.Pixels, playedBackAfterTheChange.Pixels, "the recorded drawing after a change of the map");
                Assert.AreNotEqual(direct.At(3, 5), changed.At(3, 5), "the new stream");
            }
        }

        // An empty map that measures text with Skia.
        static Map CreateMap()
        {
            return new Map(new Skia_TextMetrics(), null);
        }

        // A map with a line, an area with a hatching and an area pattern, a point symbol and a text, in four palette
        // colors.
        static Map CreateMapOfEachKindOfSymbol()
        {
            Map map = CreateMap();
            using (map.Write()) {
                SymColor green = map.AddColor("Green", 1, 0.76F, 0, 0.91F, 0, false);
                SymColor blue = map.AddColor("Blue", 2, 1, 0, 0, 0, false);
                SymColor brown = map.AddColor("Brown", 3, 0, 0.56F, 1, 0.18F, false);
                SymColor black = map.AddColor("Black", 4, 0, 0, 0, 1, false);

                AddLine(map, AddLineSymdef(map, "Contour", brown, 0.14F), new PointF(0, 0), new PointF(10, 0));

                AreaSymDef forest = new AreaSymDef("Forest", "10", green, AddLineSymdef(map, "Border", black, 0.2F));
                forest.AddHatching(new AreaSymDef.HatchInfo() { hatchColor = blue, hatchWidth = 0.1F, hatchSpacing = 0.5F, hatchAngle = 45 });
                Glyph element = new Glyph();
                element.AddFilledCircle(brown, new PointF(0, 0), 0.3F);
                element.ConstructionComplete();
                forest.AddPattern(new AreaSymDef.PatternInfo() { patternGlyph = element, patternWidth = 1, patternHeight = 1 });
                map.AddSymdef(forest);
                map.AddSymbol(new AreaSymbol(forest, new SymPathWithHoles(CreateSquarePath(0, 5, 10), null), 0, new PointF()));

                Glyph glyph = new Glyph();
                glyph.AddLine(brown, new SymPath(new PointF[] { new PointF(-1, 0), new PointF(1, 0) }), 0.2F, LineJoinMode.Round, LineCapMode.Round);
                glyph.AddFilledCircle(black, new PointF(0, 0.5F), 0.4F);
                glyph.ConstructionComplete();
                PointSymDef knoll = new PointSymDef("Knoll", "11", glyph, true);
                map.AddSymdef(knoll);
                map.AddSymbol(new PointSymbol(knoll, new PointF(5, -3), 90, null));

                TextSymDef label = new TextSymDef("Label", "12", TextSymDef.PreferredSymbolKind.NormalText, null);
                label.SetFont("Roboto", 4, TextEffects.Regular, black, 4, 0, 0, 0, null, 0, 1, TextSymDefHorizAlignment.Left, TextSymDefVertAlignment.TopAscent);
                map.AddSymdef(label);
                map.AddSymbol(new TextSymbol(label, new string[] { "Two words" }, new PointF(0, -10), 0, 0, TextSymDefHorizAlignment.Default, TextSymDefVertAlignment.Default));
            }
            return map;
        }

        // The render options of a drawing with pixels of a size and an overprint mode, with the pattern brushes.
        static RenderOptions CreateOptions(float resolution, OverprintMode overprintMode)
        {
            return new RenderOptions() {
                minResolution = resolution,
                usePatternBitmaps = true,
                renderTemplates = RenderTemplateOption.MapAndTemplates,
                overprintMode = overprintMode,
            };
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

        // Add an area symbol definition of a fill without a border, and a square of 4 mm of it at a corner, to a map.
        static AreaSymDef AddArea(Map map, string name, SymColor color, float left, float bottom)
        {
            AreaSymDef symdef = new AreaSymDef(name, (map.AllSymdefs.Count + 1).ToString(), color, null);
            map.AddSymdef(symdef);
            map.AddSymbol(new AreaSymbol(symdef, new SymPathWithHoles(CreateSquarePath(left, bottom, 4), null), 0, new PointF()));
            return symdef;
        }

        // The closed path of a square.
        static SymPath CreateSquarePath(float left, float bottom, float size)
        {
            return new SymPath(new PointF[] {
                new PointF(left, bottom), new PointF(left + size, bottom), new PointF(left + size, bottom + size),
                new PointF(left, bottom + size), new PointF(left, bottom) });
        }

        // The red, the green and the blue component of a pixel.
        static byte[] Components(SKColor pixel)
        {
            return new byte[] { pixel.Red, pixel.Green, pixel.Blue };
        }

        // Assert that two pixels differ by at most 1 in each component.
        // expected: the expected pixel.
        // actual: the pixel.
        // message: what the pixel is.
        static void AssertNear(SKColor expected, SKColor actual, string message)
        {
            for (int i = 0; i < 3; ++i)
                Assert.AreEqual(Components(expected)[i], Components(actual)[i], 1, "{0}, component {1}", message, i);
        }

        // A rendering of an area of a map with Skia on white, the pixels of a bitmap whose Y axis points down.
        sealed class Rendering
        {
            private readonly RectangleF area;
            private readonly float pixelSize;
            private readonly int width;

            public readonly SKColor[] Pixels;

            // Make a rendering from its pixels.
            // area: the area of the map.
            // pixelSize: the size of a pixel, in the coordinates of the map.
            // width: the width of the bitmap.
            // pixels: the pixels, row by row.
            private Rendering(RectangleF area, float pixelSize, int width, SKColor[] pixels)
            {
                this.area = area;
                this.pixelSize = pixelSize;
                this.width = width;
                Pixels = pixels;
            }

            // Draw an area of a map, as the rendering tests draw it (RenderingUtil).
            // map: the map.
            // area: the area of the map.
            // pixelSize: the size of a pixel, in the coordinates of the map.
            // antiAlias: whether the drawing is antialiased.
            // options: the render options.
            public static Rendering Of(Map map, RectangleF area, float pixelSize, bool antiAlias, RenderOptions options)
            {
                return Of(area, pixelSize, target => {
                    target.PushAntiAliasing(antiAlias);
                    using (map.Read())
                        map.Draw(target, area, options, null);
                    target.PopAntiAliasing();
                });
            }

            // Make a rendering of an area with a drawing on a Skia bitmap.
            // area: the area of the map.
            // pixelSize: the size of a pixel, in the coordinates of the map.
            // draw: the drawing.
            public static Rendering Of(RectangleF area, float pixelSize, Action<IGraphicsTarget> draw)
            {
                int width = (int)Math.Round(area.Width / pixelSize), height = (int)Math.Round(area.Height / pixelSize);
                using (Skia_BitmapGraphicsTarget target = new Skia_BitmapGraphicsTarget(width, height, false, CmykColor.FromCmyk(0, 0, 0, 0), area, true)) {
                    draw(target);
                    using (Skia_Bitmap bitmap = (Skia_Bitmap)target.FinishBitmap())
                        return new Rendering(area, pixelSize, width, bitmap.Bitmap.Pixels);
                }
            }

            // The pixel at a point of the map.
            // x, y: the point, in the coordinates of the map.
            public SKColor At(float x, float y)
            {
                int column = (int)Math.Floor((x - area.Left) / pixelSize);
                int row = (int)Math.Floor((area.Bottom - y) / pixelSize);
                return Pixels[row * width + column];
            }
        }

        // An encoding that keeps the transparency groups that it gets, with their palette color coverages.
        sealed class RecordingEncoding : IPaletteStackEncoding
        {
            public readonly List<TransparencyGroup> Groups = new List<TransparencyGroup>();
            private readonly List<List<SymbolCoverage>> contents = new List<List<SymbolCoverage>>();

            // Keep a transparency group, and read its palette color coverages, which pass 2 gives as they are read.
            // group: the transparency group.
            public void WriteTransparencyGroup(TransparencyGroup group)
            {
                Groups.Add(group);
                contents.Add(group.SymbolCoverages.ToList());
            }

            // The palette color coverages of each group, in the order of the groups.
            public IReadOnlyList<List<SymbolCoverage>> Contents
            {
                get { return contents; }
            }
        }

        // A graphics target that gives an encoding: the interface of the counting target.
        public interface IEncodingTarget : IGraphicsTarget, IPaletteStackEncodingProvider
        {
        }

        // A graphics target that draws nothing, keeps the name of each of its calls, and gives an encoding.
        public class CountingTarget : DispatchProxy
        {
            private IPaletteStackEncoding encoding;

            public readonly List<string> Calls = new List<string>();

            // Make a counting target that gives an encoding.
            // encoding: the encoding.
            internal static IEncodingTarget Create(IPaletteStackEncoding encoding)
            {
                IEncodingTarget target = DispatchProxy.Create<IEncodingTarget, CountingTarget>();
                ((CountingTarget)(object)target).encoding = encoding;
                return target;
            }

            // Give the encoding, and keep the name of any other call, which returns the default value of its type.
            // method: the method that is called.
            // args: the arguments of the call.
            protected override object Invoke(MethodInfo method, object[] args)
            {
                if (method.Name == "get_PaletteStackEncoding")
                    return encoding;

                Calls.Add(method.Name);
                return (method.ReturnType == typeof(void) || !method.ReturnType.IsValueType) ? null : Activator.CreateInstance(method.ReturnType);
            }
        }
    }
}
