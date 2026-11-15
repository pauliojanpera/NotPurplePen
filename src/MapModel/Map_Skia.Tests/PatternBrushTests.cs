using System;
using System.Drawing;
using NUnit.Framework;

namespace Map_Skia.Tests
{
    using Map_SkiaStd;
    using PurplePen.Graphics2D;
    using PurplePen.MapModel;
    using SkiaSharp;

    // Checks that an area pattern drawn with a pattern brush lands where the same pattern drawn element by element
    // lands, for an area whose pattern is turned or not, about the map's origin or about a pattern origin of its own, as
    // OpenMapper stores one for each object.
    [TestFixture]
    public class PatternBrushTests
    {
        // The area drawn, in map coordinates, and the size of the image it is drawn into: 0.05 mm a pixel, at which the
        // pattern's tile of 2 by 1.6 mm is drawn into a bitmap of the image's own resolution.
        private static readonly RectangleF area = new RectangleF(-10, -10, 20, 20);
        private const int ImageSize = 400;

        // The share of pixels that may differ by more than half the range of a channel. Drawn alike, the two drawings
        // differ only in how the edges of the pattern's dots and lines are shaded, and no pixel differs by that much;
        // with the pattern a fraction of its tile away, about an eighth of the pixels do.
        private const double AllowedShareOfDifferentPixels = 0.001;

        // Draw an area's pattern with a pattern brush and element by element, and require the two drawings to agree.
        // angle: the angle the object's pattern is turned by, in degrees.
        // originX, originY: the point the object's pattern is laid out from, in map coordinates.
        [TestCase(0F, 0F, 0F)]
        [TestCase(0F, 0.7F, 0.45F)]
        [TestCase(25F, 0.7F, 0.45F)]
        [TestCase(25F, 0F, 0F)]
        public void PatternBrushDrawsWhereThePatternDrawnElementByElementLies(float angle, float originX, float originY)
        {
            Map map = CreateMapOfOnePatternedArea(angle, new PointF(originX, originY));
            using (Skia_Bitmap withBrush = Draw(map, true))
            using (Skia_Bitmap elementByElement = Draw(map, false)) {
                double share = ShareOfPixelsDifferingByMoreThanHalf(withBrush.Bitmap, elementByElement.Bitmap);
                Assert.LessOrEqual(share, AllowedShareOfDifferentPixels,
                                   string.Format("{0:P2} of the pixels differ by more than half the range of a channel.", share));
            }
        }

        // Create a map holding one square area, whose symbol fills it with a regular pattern in clip mode: a dot with a
        // line beside it, in offset rows, turned by 30 degrees and moved off the middle of its tile, so that a pattern
        // laid out from the wrong point, or turned by the wrong angle, shows.
        // angle: the angle the object turns its pattern by, in degrees.
        // origin: the point the object's pattern is laid out from, in map coordinates.
        private static Map CreateMapOfOnePatternedArea(float angle, PointF origin)
        {
            Map map = new Map(new Skia_TextMetrics(), new Skia_FileLoader(TestContext.CurrentContext.TestDirectory));
            using (map.Write()) {
                SymColor black = map.AddColor("Black", 1, 0, 0, 0, 1, false);

                Glyph glyph = new Glyph();
                glyph.AddFilledCircle(black, new PointF(0, 0), 0.6F);
                glyph.AddLine(black, new SymPath(new PointF[] { new PointF(0.2F, 0), new PointF(0.8F, 0.3F) }), 0.12F, LineJoinMode.Miter, LineCapMode.Flat);
                glyph.ConstructionComplete();

                AreaSymDef.PatternInfo pattern = new AreaSymDef.PatternInfo();
                pattern.patternWidth = 2;
                pattern.patternHeight = 1.6F;
                pattern.patternAngle = 30;
                pattern.patternOffsetX = 0.3F;
                pattern.patternOffsetY = -0.2F;
                pattern.offsetRows = true;
                pattern.patternGlyph = glyph;
                pattern.patternRotateMode = AreaSymDef.RotateMode.Always;
                pattern.patternFillMode = AreaSymDef.PatternFillMode.Clip;

                AreaSymDef symdef = new AreaSymDef("Dots", "401.0", null, null);
                symdef.AddPattern(pattern);
                map.AddSymdef(symdef);

                SymPath outline = new SymPath(new PointF[] { new PointF(-8, -8), new PointF(8, -8), new PointF(8, 8), new PointF(-8, 8), new PointF(-8, -8) });
                map.AddSymbol(new AreaSymbol(symdef, new SymPathWithHoles(outline, null), angle, origin));
            }
            return map;
        }

        // Draw the area of the map anti-aliased, its patterns with pattern brushes or element by element.
        // map: the map.
        // usePatternBitmaps: whether to draw patterns with pattern brushes.
        private static Skia_Bitmap Draw(Map map, bool usePatternBitmaps)
        {
            using (Skia_BitmapGraphicsTarget target = new Skia_BitmapGraphicsTarget(ImageSize, ImageSize, false, CmykColor.FromCmyk(0, 0, 0, 0), area, true)) {
                RenderOptions options = new RenderOptions();
                options.usePatternBitmaps = usePatternBitmaps;
                options.minResolution = area.Width / ImageSize;
                target.PushAntiAliasing(true);
                using (map.Read())
                    map.Draw(target, area, options, null);
                target.PopAntiAliasing();
                return (Skia_Bitmap)target.FinishBitmap();
            }
        }

        // The share of the pixels of two images of one size in which a color channel differs by more than half its range.
        // first, second: the images.
        private static double ShareOfPixelsDifferingByMoreThanHalf(SKBitmap first, SKBitmap second)
        {
            int differing = 0;
            for (int y = 0; y < first.Height; ++y) {
                for (int x = 0; x < first.Width; ++x) {
                    SKColor a = first.GetPixel(x, y), b = second.GetPixel(x, y);
                    if (Math.Abs(a.Red - b.Red) > 128 || Math.Abs(a.Green - b.Green) > 128 || Math.Abs(a.Blue - b.Blue) > 128)
                        ++differing;
                }
            }
            return (double)differing / (first.Width * first.Height);
        }
    }
}
