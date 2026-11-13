using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Text;
using NUnit.Framework;
using PurplePen.Graphics2D;
using PurplePen.MapModel;
using SkiaSharp;

namespace Map_Skia.Tests
{
    // Tests that the areas of one symbol whose bounding boxes overlap or touch draw their hatchings and clipped
    // patterns once, within the union of their outlines. Two such areas then draw exactly as one area whose outline is
    // their union, where drawing each area's pattern by itself draws the overlap twice, which a renderer that smooths
    // edges blends into a darker pattern. An area that meets no other of its symbol is drawn as before, call for call.
    [TestFixture]
    public class OverlappingPatternsTests
    {
        // The part of the map drawn, in millimetres, and the width of the bitmap it is drawn into.
        static readonly RectangleF mapArea = new RectangleF(0, 0, 16, 16);
        const int bitmapWidth = 400;

        // How far a channel of a pixel may differ between the two drawings compared. A line that a group draws over
        // the stretch its areas' bounding boxes give it, rather than over the union's bounding box, is smoothed a few
        // levels differently along its edges, where a pattern drawn twice darkens such pixels by up to a quarter.
        const int LineEdgeTolerance = 24;

        // An area that a test draws: its outline and the angle its patterns turn by about the map's origin.
        sealed class TestArea
        {
            public readonly SymPathWithHoles Path;
            public readonly float Angle;

            // Create an area.
            // path: the outline.
            // angle: the angle, in degrees.
            public TestArea(SymPathWithHoles path, float angle)
            {
                Path = path;
                Angle = angle;
            }
        }

        // An outline that is a closed polygon.
        // angle: the angle its area turns its patterns by, in degrees.
        // coordinates: the corners, x and y in turn.
        static TestArea Polygon(float angle, params float[] coordinates)
        {
            PointF[] points = new PointF[coordinates.Length / 2 + 1];
            for (int i = 0; i < coordinates.Length / 2; ++i)
                points[i] = new PointF(coordinates[2 * i], coordinates[2 * i + 1]);
            points[points.Length - 1] = points[0];

            return new TestArea(new SymPathWithHoles(new SymPath(points), null), angle);
        }

        // An outline that is a rectangle, its corners listed the way a polygon's are.
        // angle: the angle its area turns its patterns by, in degrees.
        // left, bottom, right, top: the rectangle's sides.
        // clockwise: whether the corners run clockwise rather than counterclockwise.
        static TestArea Rectangle(float angle, float left, float bottom, float right, float top, bool clockwise = false)
        {
            if (clockwise)
                return Polygon(angle, left, bottom, left, top, right, top, right, bottom);
            else
                return Polygon(angle, left, bottom, right, bottom, right, top, left, top);
        }

        // Give a symbol a hatching of black lines 0.12 mm wide and 0.5 mm apart.
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

        // Give a symbol a clipped pattern of black dots 0.3 mm across, in rows 0.8 mm apart, every other row offset.
        // irregular: whether the pattern moves each dot by an amount that depends on where it lies.
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

        // Create a map with one black color, and areas of symbols that addPatterns gives hatchings and patterns.
        // addPatterns: what each symbol draws.
        // symbolOfArea: for each area, the index of its symbol, or null for one symbol of every area.
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

        // Draw a map's area into a bitmap, smoothed, with y pointing up, as the screen draws it.
        // map: the map.
        // usePatternBitmaps: whether clipped patterns are drawn with pattern brushes rather than glyph by glyph.
        static SKBitmap Render(Map map, bool usePatternBitmaps)
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
                    using (map.Read())
                        map.Draw(grTarget, mapArea, renderOpts, null);

                    grTarget.PopAntiAliasing();
                    grTarget.PopTransform();
                }
            }

            return bitmap;
        }

        // Count the pixels of two bitmaps of one size that differ by more than a tolerance in a channel.
        // a, b: the bitmaps.
        // tolerance: the largest difference of a channel that counts as none.
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

        // Check that areas of one symbol draw as one area whose outline is their union, and that the same areas, each
        // of a symbol of its own that draws the same, draw differently, as each pattern drawn by itself draws an overlap
        // twice and leaves a seam where the areas touch.
        // addPatterns: what the symbols draw.
        // usePatternBitmaps: whether clipped patterns are drawn with pattern brushes.
        // union: the area whose outline is the union of the areas.
        // differencesWhenSeparate: how many pixels at least differ when each area has a symbol of its own; none where
        //                          the areas only touch and their patterns are cut by clips that are not smoothed.
        // areas: the areas, which overlap or touch.
        static void CheckDrawnAsUnion(Action<AreaSymDef, SymColor> addPatterns, bool usePatternBitmaps, TestArea union, int differencesWhenSeparate, params TestArea[] areas)
        {
            using (SKBitmap expected = Render(CreateMap(addPatterns, null, union), usePatternBitmaps))
            using (SKBitmap drawn = Render(CreateMap(addPatterns, null, areas), usePatternBitmaps)) {
                Assert.That(CountInkedPixels(expected), Is.GreaterThan(1000), "the union draws its pattern");
                Assert.That(CountDifferingPixels(expected, drawn, LineEdgeTolerance), Is.EqualTo(0), "the areas of one symbol draw as their union");
            }

            int[] symbolOfArea = new int[areas.Length];
            for (int i = 0; i < areas.Length; ++i)
                symbolOfArea[i] = i;

            using (SKBitmap expected = Render(CreateMap(addPatterns, null, union), usePatternBitmaps))
            using (SKBitmap separately = Render(CreateMap(addPatterns, symbolOfArea, areas), usePatternBitmaps)) {
                Assert.That(CountDifferingPixels(expected, separately, LineEdgeTolerance), Is.GreaterThanOrEqualTo(differencesWhenSeparate),
                            "areas of separate symbols draw each pattern by itself");
            }
        }

        // Two areas that overlap, and their union.
        static readonly TestArea overlapLower = Rectangle(0, 2, 2, 10, 10);
        static readonly TestArea overlapUpper = Rectangle(0, 6, 6, 14, 14, true);
        static readonly TestArea overlapUnion = Polygon(0, 2, 2, 10, 2, 10, 6, 14, 6, 14, 14, 6, 14, 6, 10, 2, 10);

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
            // The lower area has a hole at its middle, which the upper area covers in part.
            PointF[] outer = { new PointF(2, 2), new PointF(10, 2), new PointF(10, 10), new PointF(2, 10), new PointF(2, 2) };
            PointF[] hole = { new PointF(4, 4), new PointF(4, 8), new PointF(8, 8), new PointF(8, 4), new PointF(4, 4) };
            TestArea lowerWithHole = new TestArea(new SymPathWithHoles(new SymPath(outer), new SymPath[] { new SymPath(hole) }), 0);

            PointF[] unionOuter = { new PointF(2, 2), new PointF(10, 2), new PointF(10, 6), new PointF(14, 6), new PointF(14, 14),
                                    new PointF(6, 14), new PointF(6, 10), new PointF(2, 10), new PointF(2, 2) };
            PointF[] unionHole = { new PointF(4, 4), new PointF(4, 8), new PointF(6, 8), new PointF(6, 6), new PointF(8, 6), new PointF(8, 4), new PointF(4, 4) };
            TestArea union = new TestArea(new SymPathWithHoles(new SymPath(unionOuter), new SymPath[] { new SymPath(unionHole) }), 0);

            CheckDrawnAsUnion(Hatching(30, AreaSymDef.RotateMode.Always), false, union, 100, lowerWithHole, overlapUpper);
        }

        [Test]
        public void AreaWithHoleOutsideItsOutlineIsDrawnOnce()
        {
            // The left area's hole lies outside its outline, where the even-odd rule fills it, and the right area
            // covers the hole.
            PointF[] outline = { new PointF(2, 2), new PointF(8, 2), new PointF(8, 14), new PointF(2, 14), new PointF(2, 2) };
            PointF[] hole = { new PointF(10, 6), new PointF(12, 6), new PointF(12, 10), new PointF(10, 10), new PointF(10, 6) };
            TestArea leftWithHole = new TestArea(new SymPathWithHoles(new SymPath(outline), new SymPath[] { new SymPath(hole) }), 0);

            CheckDrawnAsUnion(Hatching(30, AreaSymDef.RotateMode.Always), false, Rectangle(0, 2, 2, 14, 14), 100, leftWithHole, Rectangle(0, 6, 2, 14, 14, true));
        }

        [Test]
        public void AreasThatMeetNoOtherAreDrawnAsBefore()
        {
            CheckDrawnEachByItself(Rectangle(0, 1, 1, 5, 5), Rectangle(0, 9, 9, 13, 13));
        }

        [Test]
        public void AreasWhoseContoursCrossDrawTheirOwnPatterns()
        {
            // An area whose hole crosses its outline, and an area that covers the part of the hole outside the
            // outline, which the even-odd rule fills.
            PointF[] outline = { new PointF(2, 2), new PointF(10, 2), new PointF(10, 10), new PointF(2, 10), new PointF(2, 2) };
            PointF[] hole = { new PointF(8, 4), new PointF(12, 4), new PointF(12, 8), new PointF(8, 8), new PointF(8, 4) };
            TestArea crossedByItsHole = new TestArea(new SymPathWithHoles(new SymPath(outline), new SymPath[] { new SymPath(hole) }), 0);
            CheckDrawnEachByItself(crossedByItsHole, Rectangle(0, 10, 2, 14, 10));

            // An area whose outline crosses itself, and an area that covers a part of it.
            CheckDrawnEachByItself(Polygon(0, 2, 2, 10, 10, 10, 2, 2, 10), Rectangle(0, 8, 2, 14, 10));
        }

        // Check that areas of one symbol draw call for call as each draws by itself, with no path of a union: with a
        // hatching and with dots, drawn glyph by glyph and with brushes.
        // areas: the areas.
        static void CheckDrawnEachByItself(params TestArea[] areas)
        {
            foreach (bool usePatternBitmaps in new bool[] { false, true }) {
                foreach (Action<AreaSymDef, SymColor> addPatterns in new Action<AreaSymDef, SymColor>[] { Hatching(30, AreaSymDef.RotateMode.Always), Dots(false) }) {
                    string all = LogDrawing(CreateMap(addPatterns, null, areas), usePatternBitmaps);
                    StringBuilder each = new StringBuilder();
                    foreach (TestArea area in areas)
                        each.Append(LogDrawing(CreateMap(addPatterns, null, area), usePatternBitmaps));

                    Assert.That(all, Is.EqualTo(each.ToString()));
                    Assert.That(all, Does.Not.Contain("Winding"));
                }
            }
        }

        [Test]
        public void OverlappingAreasDrawTheirPatternOnceWithinTheirUnion()
        {
            string log = LogDrawing(CreateMap(Hatching(30, AreaSymDef.RotateMode.Always), null, overlapLower, overlapUpper), false);
            Assert.That(CountOccurrences(log, "CreatePath Winding"), Is.EqualTo(1));
            Assert.That(CountOccurrences(log, "PushClip path"), Is.EqualTo(1));
        }

        // Count where a text occurs in another.
        // text: the text searched.
        // part: the text sought.
        static int CountOccurrences(string text, string part)
        {
            int count = 0;
            for (int index = text.IndexOf(part, StringComparison.Ordinal); index >= 0; index = text.IndexOf(part, index + part.Length, StringComparison.Ordinal))
                ++count;
            return count;
        }

        // Draw a map's area onto a target that writes down the drawing calls it is given, and return what it wrote.
        // map: the map.
        // usePatternBitmaps: whether clipped patterns are drawn with pattern brushes.
        static string LogDrawing(Map map, bool usePatternBitmaps)
        {
            using (LoggingGraphicsTarget grTarget = new LoggingGraphicsTarget()) {
                RenderOptions renderOpts = new RenderOptions();
                renderOpts.minResolution = mapArea.Width / bitmapWidth;
                renderOpts.usePatternBitmaps = usePatternBitmaps;
                renderOpts.renderTemplates = RenderTemplateOption.MapOnly;
                using (map.Read())
                    map.Draw(grTarget, mapArea, renderOpts, null);

                return grTarget.Log;
            }
        }

        // A target that draws nothing and writes down the drawing calls it is given, one a line, each with the
        // geometry it draws, its paths described by their parts, so that drawings of different maps can be compared.
        sealed class LoggingGraphicsTarget : IGraphicsTarget, IBrushTarget
        {
            // What the target has been given, the paths it has been given by their keys, and the keys of the
            // brushes, pens and fonts it has made.
            readonly StringBuilder log = new StringBuilder();
            readonly Dictionary<object, string> paths = new Dictionary<object, string>(ReferenceEqualityComparer.Instance);
            readonly HashSet<object> brushes = new HashSet<object>(ReferenceEqualityComparer.Instance), pens = new HashSet<object>(ReferenceEqualityComparer.Instance),
                                    fonts = new HashSet<object>(ReferenceEqualityComparer.Instance);

            // Whether what is drawn goes into the tile of a pattern brush, which the log leaves out, as a brush is made
            // once on a target, the first time it is needed.
            bool drawingBrush;

            // What the target was given, a call a line.
            public string Log
            {
                get { return log.ToString(); }
            }

            // Write a call down.
            // call: the call and its arguments.
            void Write(string call)
            {
                if (!drawingBrush)
                    log.Append(call).Append('\n');
            }

            // Describe a point.
            // point: the point.
            static string Describe(PointF point)
            {
                return string.Format(CultureInfo.InvariantCulture, "({0:0.###},{1:0.###})", point.X, point.Y);
            }

            // Describe a path by its parts.
            // parts: the parts.
            // windingMode: how the path fills.
            static string Describe(List<GraphicsPathPart> parts, AreaFillMode windingMode)
            {
                StringBuilder builder = new StringBuilder(windingMode.ToString());
                foreach (GraphicsPathPart part in parts) {
                    builder.Append(' ').Append(part.Kind);
                    if (part.Points != null) {
                        foreach (PointF point in part.Points)
                            builder.Append(Describe(point));
                    }
                }
                return builder.ToString();
            }

            // Describe a rectangle.
            // rect: the rectangle.
            static string Describe(RectangleF rect)
            {
                return string.Format(CultureInfo.InvariantCulture, "[{0:0.###},{1:0.###},{2:0.###},{3:0.###}]", rect.Left, rect.Top, rect.Width, rect.Height);
            }

            // The intensity drawn with, which the log leaves out.
            public float Intensity { get; set; } = 1.0F;

            // Release nothing, as the target holds nothing to release.
            public void Dispose()
            {
            }

            // Write a transform down by its elements.
            // matrix: the transform.
            public void PushTransform(Matrix matrix)
            {
                Write("PushTransform " + string.Join(",", Array.ConvertAll(matrix.Elements, (float e) => e.ToString("0.###", CultureInfo.InvariantCulture))));
            }

            // Write the end of a transform down.
            public void PopTransform()
            {
                Write("PopTransform");
            }

            // Write a clip to a path down, the path described by its parts.
            // pathKey: the key of the path.
            public void PushClip(object pathKey)
            {
                Write("PushClip path " + paths[pathKey]);
            }

            // Write a clip to a rectangle down.
            // rectangle: the rectangle.
            public void PushClip(RectangleF rectangle)
            {
                Write("PushClip rect " + Describe(rectangle));
            }

            // Write a clip to rectangles down.
            // rectangles: the rectangles.
            public void PushClip(RectangleF[] rectangles)
            {
                Write("PushClip rects " + string.Join(" ", Array.ConvertAll(rectangles, (RectangleF r) => Describe(r))));
            }

            // Write a clip to a path given by its parts down.
            // parts, windingMode: the path.
            public void PushClip(List<GraphicsPathPart> parts, AreaFillMode windingMode)
            {
                Write("PushClip parts " + Describe(parts, windingMode));
            }

            // Write the end of a clip down.
            public void PopClip()
            {
                Write("PopClip");
            }

            // Write a change of smoothing down.
            // antiAlias: whether edges are smoothed.
            public void PushAntiAliasing(bool antiAlias)
            {
                Write("PushAntiAliasing " + antiAlias);
            }

            // Write the end of a change of smoothing down.
            public void PopAntiAliasing()
            {
                Write("PopAntiAliasing");
            }

            // Write a blending down, and take it.
            // blendMode: the blending.
            public bool PushBlending(BlendMode blendMode)
            {
                Write("PushBlending " + blendMode);
                return true;
            }

            // Write the end of a blending down.
            public void PopBlending()
            {
                Write("PopBlending");
            }

            // Note a path by its parts, and write its creation down.
            // key, parts, windingMode: the path.
            public void CreatePath(object key, List<GraphicsPathPart> parts, AreaFillMode windingMode)
            {
                paths[key] = Describe(parts, windingMode);
                Write("CreatePath " + paths[key]);
            }

            // Note a brush.
            // key, color: the brush.
            public void CreateSolidBrush(object key, CmykColor color)
            {
                brushes.Add(key);
            }

            // Whether the target takes pattern brushes, which it does.
            public bool SupportsPatternBrushes
            {
                get { return true; }
            }

            // Begin a pattern brush, whose tile is drawn onto this target without being written down.
            // size, angle, bitmapWidth, bitmapHeight: the brush's tile.
            public IBrushTarget CreatePatternBrush(SizeF size, float angle, int bitmapWidth, int bitmapHeight)
            {
                drawingBrush = true;
                return this;
            }

            // Note a pattern brush, and end its tile.
            // brushKey: the key of the brush.
            public void FinishBrush(object brushKey)
            {
                brushes.Add(brushKey);
                drawingBrush = false;
            }

            // Note a pen.
            // key, brushKey, width, caps, join, miterLimit: the pen.
            public void CreatePen(object key, object brushKey, float width, LineCapMode caps, LineJoinMode join, float miterLimit)
            {
                pens.Add(key);
            }

            // Note a pen.
            // key, color, width, caps, join, miterLimit: the pen.
            public void CreatePen(object key, CmykColor color, float width, LineCapMode caps, LineJoinMode join, float miterLimit)
            {
                pens.Add(key);
            }

            // Note a font.
            // key, familyName, emHeight, effects: the font.
            public void CreateFont(object key, string familyName, float emHeight, TextEffects effects)
            {
                fonts.Add(key);
            }

            // Whether a path has been created.
            // pathKey: the key of the path.
            public bool HasPath(object pathKey)
            {
                return paths.ContainsKey(pathKey);
            }

            // Whether a brush has been created.
            // brushKey: the key of the brush.
            public bool HasBrush(object brushKey)
            {
                return brushes.Contains(brushKey);
            }

            // Whether a pen has been created.
            // penKey: the key of the pen.
            public bool HasPen(object penKey)
            {
                return pens.Contains(penKey);
            }

            // Whether a font has been created.
            // fontKey: the key of the font.
            public bool HasFont(object fontKey)
            {
                return fonts.Contains(fontKey);
            }

            // Write a line down.
            // penKey, start, finish: the line.
            public void DrawLine(object penKey, PointF start, PointF finish)
            {
                Write("DrawLine " + Describe(start) + Describe(finish));
            }

            // Write an arc down by its center.
            // penKey, center, radius, startAngle, sweepAngle: the arc.
            public void DrawArc(object penKey, PointF center, float radius, float startAngle, float sweepAngle)
            {
                Write("DrawArc " + Describe(center));
            }

            // Write an ellipse down by its center.
            // penKey, center, radiusX, radiusY: the ellipse.
            public void DrawEllipse(object penKey, PointF center, float radiusX, float radiusY)
            {
                Write("DrawEllipse " + Describe(center));
            }

            // Write a filled ellipse down by its center.
            // brushKey, center, radiusX, radiusY: the ellipse.
            public void FillEllipse(object brushKey, PointF center, float radiusX, float radiusY)
            {
                Write("FillEllipse " + Describe(center));
            }

            // Write a rectangle down.
            // penKey, rect: the rectangle.
            public void DrawRectangle(object penKey, RectangleF rect)
            {
                Write("DrawRectangle " + Describe(rect));
            }

            // Write a filled rectangle down.
            // brushKey, rect: the rectangle.
            public void FillRectangle(object brushKey, RectangleF rect)
            {
                Write("FillRectangle " + Describe(rect));
            }

            // Write a polygon down.
            // penKey, pts: the polygon.
            public void DrawPolygon(object penKey, PointF[] pts)
            {
                Write("DrawPolygon " + string.Concat(Array.ConvertAll(pts, (PointF p) => Describe(p))));
            }

            // Write a polyline down.
            // penKey, pts: the polyline.
            public void DrawPolyline(object penKey, PointF[] pts)
            {
                Write("DrawPolyline " + string.Concat(Array.ConvertAll(pts, (PointF p) => Describe(p))));
            }

            // Write a filled polygon down.
            // brushKey, pts, windingMode: the polygon.
            public void FillPolygon(object brushKey, PointF[] pts, AreaFillMode windingMode)
            {
                Write("FillPolygon " + windingMode + " " + string.Concat(Array.ConvertAll(pts, (PointF p) => Describe(p))));
            }

            // Write a path drawn down, described by its parts.
            // penKey, pathKey: the path.
            public void DrawPath(object penKey, object pathKey)
            {
                Write("DrawPath " + paths[pathKey]);
            }

            // Write a path given by its parts drawn down.
            // penKey, parts: the path.
            public void DrawPath(object penKey, List<GraphicsPathPart> parts)
            {
                Write("DrawPath parts " + Describe(parts, AreaFillMode.Winding));
            }

            // Write a path filled down, described by its parts.
            // brushKey, pathKey: the path.
            public void FillPath(object brushKey, object pathKey)
            {
                Write("FillPath " + paths[pathKey]);
            }

            // Write a path given by its parts filled down.
            // brushKey, parts, windingMode: the path.
            public void FillPath(object brushKey, List<GraphicsPathPart> parts, AreaFillMode windingMode)
            {
                Write("FillPath parts " + Describe(parts, windingMode));
            }

            // Write a text down.
            // text, fontKey, brushKey, upperLeft: the text.
            public void DrawText(string text, object fontKey, object brushKey, PointF upperLeft)
            {
                Write("DrawText " + text);
            }

            // Write an outlined text down.
            // text, fontKey, penKey, upperLeft: the text.
            public void DrawTextOutline(string text, object fontKey, object penKey, PointF upperLeft)
            {
                Write("DrawTextOutline " + text);
            }

            // Write a bitmap down by where it is drawn.
            // bm, rectangle, scalingMode: the bitmap.
            public void DrawBitmap(IGraphicsBitmap bm, RectangleF rectangle, BitmapScaling scalingMode)
            {
                Write("DrawBitmap " + Describe(rectangle));
            }

            // Write a part of a bitmap down by where it is drawn.
            // bm, x, y, width, height, rectangle, scalingMode: the part of the bitmap.
            public void DrawBitmapPart(IGraphicsBitmap bm, int x, int y, int width, int height, RectangleF rectangle, BitmapScaling scalingMode)
            {
                Write("DrawBitmapPart " + Describe(rectangle));
            }
        }
    }
}
