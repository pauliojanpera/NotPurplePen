using System.Drawing;
using NUnit.Framework;
using PurplePen.MapModel;

namespace Map_Skia.Tests
{
    // The dashes of the fill and the borders of an OCAD double line have no minimum number of gaps: a line too short for
    // a gap is solid, also beside a dashed main line with a minimum of its own. The OCAD 2020 viewer draws the samples of
    // the dashed lines as the tests expect.
    [TestFixture]
    public class DoubleLineDashTests
    {
        // The double lines of the second sample, at y = -14 mm: C has a dashed fill and dashed borders, and C3 dashed
        // borders and a solid fill. Their dashes are 2.0 mm and their gaps 0.5 mm.
        // startX: the start of the line, in mm on the paper.
        // length: the length of the line.
        // offset: the distance to the left from the middle of the line at which the dashes are counted.
        // dashes: the number of dashes that OCAD draws.
        [TestCase(2.0F, 1.0F, 0F, 1, TestName = "{m}(C, 1.0 mm)")]
        [TestCase(12.0F, 1.5F, 0F, 1, TestName = "{m}(C, 1.5 mm)")]
        [TestCase(22.0F, 2.0F, 0F, 1, TestName = "{m}(C, 2.0 mm)")]
        [TestCase(32.0F, 2.45F, 0F, 1, TestName = "{m}(C, 2.45 mm)")]
        [TestCase(52.0F, 2.55F, 0F, 2, TestName = "{m}(C, 2.55 mm)")]
        [TestCase(62.0F, 3.0F, 0F, 2, TestName = "{m}(C, 3.0 mm)")]
        [TestCase(112.0F, 1.5F, 0.375F, 1, TestName = "{m}(C3, 1.5 mm)")]
        [TestCase(122.0F, 3.0F, 0.375F, 2, TestName = "{m}(C3, 3.0 mm)")]
        public void DoubleLineHasTheDashesThatOcadDraws(float startX, float length, float offset, int dashes)
        {
            Map map = OcadSampleDrawing.ReadTestFile("dashes-2-ocad12-euclidean.ocd");
            Assert.AreEqual(dashes, OcadSampleDrawing.CountRunsAlong(map, new PointF(startX, -14 + offset), new PointF(startX + length, -14 + offset)));
        }

        // The borders of the third sample, at y = -42 mm, beside a dashed main line of 0.1 mm: DM0 with no minimum of the
        // main line, and DM3 with a minimum of 3 gaps (MinSym 2). The borders are counted 0.375 mm to the left.
        // startX: the start of the line, in mm on the paper.
        // length: the length of the line.
        // dashes: the number of dashes that OCAD draws on a border.
        [TestCase(2.0F, 1.0F, 1, TestName = "{m}(DM0, 1.0 mm)")]
        [TestCase(14.0F, 1.5F, 1, TestName = "{m}(DM0, 1.5 mm)")]
        [TestCase(26.0F, 2.0F, 1, TestName = "{m}(DM0, 2.0 mm)")]
        [TestCase(38.0F, 3.0F, 2, TestName = "{m}(DM0, 3.0 mm)")]
        [TestCase(74.0F, 1.0F, 1, TestName = "{m}(DM3, 1.0 mm)")]
        [TestCase(86.0F, 1.5F, 1, TestName = "{m}(DM3, 1.5 mm)")]
        [TestCase(98.0F, 2.0F, 1, TestName = "{m}(DM3, 2.0 mm)")]
        [TestCase(110.0F, 3.0F, 2, TestName = "{m}(DM3, 3.0 mm)")]
        public void BorderBesideDashedMainLineHasTheDashesThatOcadDraws(float startX, float length, int dashes)
        {
            Map map = OcadSampleDrawing.ReadTestFile("dashes-3-ocad12-euclidean.ocd");
            Assert.AreEqual(dashes, OcadSampleDrawing.CountRunsAlong(map, new PointF(startX, -42 + 0.375F), new PointF(startX + length, -42 + 0.375F)));
        }
    }
}
