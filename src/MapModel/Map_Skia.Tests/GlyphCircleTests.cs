using System;
using System.Drawing;
using System.IO;
using System.Linq;
using NUnit.Framework;
using PurplePen.Graphics2D;
using PurplePen.MapModel;
using TestingUtils;

namespace Map_Skia.Tests
{
    // The circles of glyphs as OCAD 2020 draws them. With the diameter from the middle of the line, which OCAD 9+
    // writes, and OCAD 6-8 writes with the line added: a circle whose diameter is larger than its line is a ring, one
    // whose line is at least as wide as its diameter is a filled circle of that diameter, and one whose diameter is
    // 0 or less covers nothing, whether the circle is alone at the origin of its glyph or not. The tests draw a
    // circle without antialiasing on a transparent bitmap and look at the pixels on the row through its center.
    [TestFixture]
    public class GlyphCircleTests
    {
        const float PixelSize = 0.05F;

        // A circle of a diameter of 1 mm from the outer edges, which Purple Pen keeps for a ring.
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
                glyph.AddCircle(black, new PointF(centerX, 0), lineWidth, 1.0F);
                if (withAnotherPart)
                    glyph.AddFilledCircle(black, new PointF(centerX, 3), 0.2F);
                glyph.ConstructionComplete();
                PointSymDef symdef = new PointSymDef("Circle", "1", glyph, false);
                map.AddSymdef(symdef);
                map.AddSymbol(new PointSymbol(symdef, new PointF(0, 0), 0, null));
            }

            AssertRowThroughCenterCovers(map, centerX, 1.0F, 0);
        }

        // The point symbols of two files, in a row on the paper, each with one circle at a center. In the OCAD 12 file,
        // the diameters are from the middle of the line: A 1.0 with a line of 1.5, B the same and a dot 2.5 mm to the
        // right, C 1.0 with 1.0, D 2.0 with 0.5, and E as A. In the OCAD 8 file, the diameters are from the outer
        // edges: A8 1.0 with 1.5, B8 the same and a dot, C8 1.0 with 1.0, F8 1.0 with 0.6, D8 2.5 with 0.5, and E8 as A8.
        // outerDiameter: the diameter that OCAD covers, or 0 if OCAD draws nothing.
        // innerDiameter: the diameter of the hole of a ring, or 0.
        [TestCase("circle-line-wider-than-diameter-ocad12.ocd", 0F, 1.0F, 0F, TestName = "{m}(OCAD 12, A)")]
        [TestCase("circle-line-wider-than-diameter-ocad12.ocd", 10F, 1.0F, 0F, TestName = "{m}(OCAD 12, B)")]
        [TestCase("circle-line-wider-than-diameter-ocad12.ocd", 20F, 1.0F, 0F, TestName = "{m}(OCAD 12, C)")]
        [TestCase("circle-line-wider-than-diameter-ocad12.ocd", 30F, 2.5F, 1.5F, TestName = "{m}(OCAD 12, D)")]
        [TestCase("circle-line-wider-than-diameter-ocad12.ocd", 41F, 1.0F, 0F, TestName = "{m}(OCAD 12, E)")]
        [TestCase("circle-line-wide-ocad8.ocd", 0F, 0F, 0F, TestName = "{m}(OCAD 8, A8)")]
        [TestCase("circle-line-wide-ocad8.ocd", 10F, 0F, 0F, TestName = "{m}(OCAD 8, B8)")]
        [TestCase("circle-line-wide-ocad8.ocd", 20F, 0F, 0F, TestName = "{m}(OCAD 8, C8)")]
        [TestCase("circle-line-wide-ocad8.ocd", 30F, 0.4F, 0F, TestName = "{m}(OCAD 8, F8)")]
        [TestCase("circle-line-wide-ocad8.ocd", 40F, 2.5F, 1.5F, TestName = "{m}(OCAD 8, D8)")]
        [TestCase("circle-line-wide-ocad8.ocd", 51F, 0F, 0F, TestName = "{m}(OCAD 8, E8)")]
        public void CircleOfOcadFileIsDrawnAsOcadDrawsIt(string fileName, float centerX, float outerDiameter, float innerDiameter)
        {
            string path = TestUtil.GetTestFile(Path.Combine("ocadimport", fileName));
            Map map = new Map(new Skia_TextMetrics(), new Skia_FileLoader(Path.GetDirectoryName(path)));
            InputOutput.ReadFile(path, map);

            AssertRowThroughCenterCovers(map, centerX, outerDiameter, innerDiameter);
        }

        // Assert which pixels the drawing of a map covers on the row through a center: those that lie less than half an
        // outer diameter and more than half an inner diameter from the center, apart from the pixels at these edges.
        // map: the map.
        // centerX: the center, on the X axis.
        // outerDiameter: the outer diameter, or 0 if no pixel is covered.
        // innerDiameter: the diameter of the hole, or 0.
        static void AssertRowThroughCenterCovers(Map map, float centerX, float outerDiameter, float innerDiameter)
        {
            // 60 pixels of 0.05 mm across 3 mm around the center, which lies between pixels 29 and 30.
            const int size = 60;
            bool[] row = DrawRowThroughMiddle(map, new RectangleF(centerX - 1.5F, -1.5F, 3, 3), size);
            for (int i = 0; i < row.Length; ++i) {
                float distance = Math.Abs((i + 0.5F) * PixelSize - 1.5F);
                bool isInside = distance < outerDiameter / 2 - PixelSize && distance > innerDiameter / 2 + PixelSize;
                bool isOutside = distance > outerDiameter / 2 + PixelSize || (innerDiameter > 0 && distance < innerDiameter / 2 - PixelSize);
                if (isInside)
                    Assert.IsTrue(row[i], "pixel {0}, {1} mm from the center, is not covered", i, distance);
                else if (isOutside || outerDiameter == 0)
                    Assert.IsFalse(row[i], "pixel {0}, {1} mm from the center, is covered", i, distance);
            }
        }

        // Draw a map without antialiasing on a transparent square bitmap of an area, and tell whether the drawing covers
        // each pixel of the row through the middle of the area.
        // map: the map.
        // area: the area, a square.
        // size: the width and the height of the bitmap, in pixels.
        static bool[] DrawRowThroughMiddle(Map map, RectangleF area, int size)
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
