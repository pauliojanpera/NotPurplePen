using System.Drawing;
using NUnit.Framework;
using PurplePen.MapModel;

namespace Map_Skia.Tests
{
    // OCAD reads the minimum number of gaps of a line symbol, or of its spaced glyphs, from MinSym: -1 is no minimum, 0 is
    // one, 1 is two, and 2 and 3 are one again. The third sample of the dashed lines has rows of lines of 1.0 to 6.0 mm,
    // each 12 mm to the right of the one before, from x = 2 mm. The OCAD 2020 viewer draws them as the tests expect.
    [TestFixture]
    public class OcadMinimumGapsTests
    {
        static readonly float[] LengthsOfLinesInRow = { 1.0F, 1.5F, 2.0F, 2.6F, 3.0F, 4.0F, 6.0F };

        // A row of dashed lines with dashes of 2.0 mm, end dashes of 1.0 mm and gaps of 0.5 mm.
        // y: the row, in mm on the paper.
        // dashes: the number of dashes that OCAD draws on each line of the row.
        [TestCase(-14F, new int[] { 2, 2, 2, 2, 2, 3, 3 }, TestName = "{m}(M3, MinSym 2)")]
        [TestCase(-18F, new int[] { 2, 2, 2, 2, 2, 3, 3 }, TestName = "{m}(M4, MinSym 3)")]
        [TestCase(-34F, new int[] { 2, 2, 2, 2, 2, 4, 4 }, TestName = "{m}(MS3, MinSym 2 and a secondary gap)")]
        [TestCase(-38F, new int[] { 2, 2, 2, 2, 2, 4, 4 }, TestName = "{m}(MS4, MinSym 3 and a secondary gap)")]
        public void LinesHaveTheDashesThatOcadDraws(float y, int[] dashes)
        {
            Map map = OcadSampleDrawing.ReadTestFile("dashes-3-ocad12-euclidean.ocd");
            for (int i = 0; i < LengthsOfLinesInRow.Length; ++i) {
                float x = 2 + 12 * i;
                Assert.AreEqual(dashes[i], OcadSampleDrawing.CountRunsAlong(map, new PointF(x, y), new PointF(x + LengthsOfLinesInRow[i], y)), "the line of {0} mm", LengthsOfLinesInRow[i]);
            }
        }

        // Filled circles of 0.3 mm every 2.0 mm, the first and the last 1.0 mm from the ends, with MinSym 2, at y = -50 mm.
        // startX: the start of the line, in mm on the paper.
        // length: the length of the line.
        // glyphs: the number of glyphs that OCAD draws.
        [TestCase(74F, 1.0F, 1, TestName = "{m}(GS3, 1.0 mm)")]
        [TestCase(86F, 2.0F, 1, TestName = "{m}(GS3, 2.0 mm)")]
        [TestCase(110F, 4.0F, 2, TestName = "{m}(GS3, 4.0 mm)")]
        public void LineHasTheGlyphsThatOcadDraws(float startX, float length, int glyphs)
        {
            Map map = OcadSampleDrawing.ReadTestFile("dashes-3-ocad12-euclidean.ocd");
            Assert.AreEqual(glyphs, OcadSampleDrawing.CountRunsAlong(map, new PointF(startX, -50), new PointF(startX + length, -50)));
        }
    }
}
