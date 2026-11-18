using System;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using PurplePen.Graphics2D;
using PurplePen.MapModel;

namespace Map_Skia.Tests
{
    // The circles of glyphs as OCAD draws them: a circle whose line is at least as wide as its diameter is a filled
    // circle of its diameter, whether it is alone at the origin of its glyph or not. The tests draw the circle without
    // antialiasing on a transparent bitmap and look at the pixels on the row through its center.
    [TestFixture]
    public class GlyphCircleTests
    {
        const float Diameter = 1.0F;
        const float PixelSize = 0.05F;

        [TestCase(1.5F, false, 0F, TestName = "{m}(line wider than the diameter, alone at the origin)")]
        [TestCase(1.0F, false, 0F, TestName = "{m}(line as wide as the diameter, alone at the origin)")]
        [TestCase(1.5F, true, 0F, TestName = "{m}(line wider than the diameter, with another part)")]
        [TestCase(1.5F, false, 1F, TestName = "{m}(line wider than the diameter, away from the origin)")]
        public void CircleIsFilledCircleOfItsDiameter(float lineWidth, bool withAnotherPart, float centerX)
        {
            Map map = new Map(new Skia_TextMetrics(), null);
            using (map.Write()) {
                SymColor black = map.AddColor("Black", 1, 0, 0, 0, 1, false);
                Glyph glyph = new Glyph();
                glyph.AddCircle(black, new PointF(centerX, 0), lineWidth, Diameter);
                if (withAnotherPart)
                    glyph.AddFilledCircle(black, new PointF(centerX, 3), 0.2F);
                glyph.ConstructionComplete();
                PointSymDef symdef = new PointSymDef("Circle", "1", glyph, false);
                map.AddSymdef(symdef);
                map.AddSymbol(new PointSymbol(symdef, new PointF(0, 0), 0, null));
            }

            // 40 pixels of 0.05 mm across the circle, the center between pixels 19 and 20.
            RectangleF area = new RectangleF(centerX - 1, -1, 2, 2);
            bool[] row = DrawRowThroughCenter(map, area, 40);
            for (int i = 0; i < row.Length; ++i) {
                float distance = Math.Abs((i + 0.5F) * PixelSize - 1);
                if (distance < Diameter / 2 - PixelSize)
                    Assert.IsTrue(row[i], "pixel {0}, {1} mm from the center, is not covered", i, distance);
                else if (distance > Diameter / 2 + PixelSize)
                    Assert.IsFalse(row[i], "pixel {0}, {1} mm from the center, is covered", i, distance);
            }
        }

        // Draw a map without antialiasing on a transparent square bitmap of an area, and tell whether the drawing covers
        // each pixel of the row through the middle of the area.
        // map: the map.
        // area: the area, a square.
        // size: the width and the height of the bitmap, in pixels.
        static bool[] DrawRowThroughCenter(Map map, RectangleF area, int size)
        {
            using (map.Read())
            using (Skia_BitmapGraphicsTarget target = new Skia_BitmapGraphicsTarget(size, size, true, CmykColor.FromCmyka(0, 0, 0, 0, 0), area, true)) {
                target.PushAntiAliasing(false);
                map.Draw(target, area, new RenderOptions() { minResolution = PixelSize, renderTemplates = RenderTemplateOption.MapOnly }, null);
                target.PopAntiAliasing();
                using (Skia_Bitmap bitmap = (Skia_Bitmap)target.FinishBitmap())
                    return Enumerable.Range(0, size).Select(x => bitmap.Bitmap.GetPixel(x, size / 2).Alpha != 0).ToArray();
            }
        }
    }
}
