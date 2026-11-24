using System;
using System.Drawing;
using System.IO;
using NUnit.Framework;
using PurplePen.Graphics2D;
using PurplePen.MapModel;

namespace Map_Skia.Tests
{
    // The metric with which OCAD measures the lines of a file, for the dashes along them. OCAD takes it from the
    // parameter p of the view parameters of a file of any version: 1 is the OCAD metric, and 0 or no parameter is the
    // Euclidean metric. OCAD 6 to 8 files have no view parameters. The files dashes-*.ocd are from the second sample of
    // the dashed lines, and the OCAD 2020 viewer draws them as the tests expect.
    [TestFixture]
    public class OcadMetricTests
    {
        // fileName: a file in TestFiles/ocadimport.
        // isEuclidean: whether OCAD measures the lines of the file with the Euclidean metric.
        [TestCase("dashes-ocad10.ocd", true, TestName = "{m}(OCAD 10 without p)")]
        [TestCase("dashes-ocad10-ocad-metric.ocd", false, TestName = "{m}(OCAD 10 with p1)")]
        [TestCase("dashes-ocad9-ocad-metric.ocd", false, TestName = "{m}(OCAD 9 with p1)")]
        [TestCase("fileinfo_9.ocd", true, TestName = "{m}(OCAD 9)")]
        [TestCase("fileinfo_8.ocd", true, TestName = "{m}(OCAD 8)")]
        [TestCase("nonrenderable6.ocd", true, TestName = "{m}(OCAD 6)")]
        public void MetricOfFileIsTheOneThatOcadUses(string fileName, bool isEuclidean)
        {
            Map map = OcadSampleDrawing.ReadTestFile(fileName);
            using (map.Read())
                Assert.AreEqual(isEuclidean, map.UseEuclideanMetric);
        }

        // The line at 45 degrees of the cell E4.6-4: 12.73 mm, with dashes of 2.0 mm, end dashes of 2.0 mm and gaps of
        // 0.5 mm. The Euclidean metric gives it 5 dashes, and the OCAD metric, by which it is 13.5 mm long, 6 dashes.
        // fileName: a file in TestFiles/ocadimport.
        // dashes: the number of dashes that OCAD draws.
        [TestCase("dashes-ocad10.ocd", 5, TestName = "{m}(OCAD 10 without p)")]
        [TestCase("dashes-ocad10-ocad-metric.ocd", 6, TestName = "{m}(OCAD 10 with p1)")]
        [TestCase("dashes-ocad9-ocad-metric.ocd", 6, TestName = "{m}(OCAD 9 with p1)")]
        public void LineHasTheDashesThatOcadDraws(string fileName, int dashes)
        {
            Map map = OcadSampleDrawing.ReadTestFile(fileName);
            Assert.AreEqual(dashes, OcadSampleDrawing.CountRunsAlong(map, new PointF(92.0F, -112.22F), new PointF(101.0F, -103.22F)));
        }

        // A file that Purple Pen writes keeps the metric of the map in each version that has view parameters.
        // version: the version of OCAD of the file.
        // isEuclidean: whether the map uses the Euclidean metric.
        [TestCase(9, true)]
        [TestCase(9, false)]
        [TestCase(10, true)]
        [TestCase(10, false)]
        [TestCase(12, true)]
        [TestCase(12, false)]
        public void FileKeepsTheMetricOfTheMap(int version, bool isEuclidean)
        {
            Map map = new Map(new Skia_TextMetrics(), null);
            using (map.Write()) {
                map.AddColor("Black", 1, 0, 0, 0, 1, false);
                map.UseEuclideanMetric = isEuclidean;
            }

            string path = Path.Combine(Path.GetTempPath(), "OcadMetricTest_" + Guid.NewGuid().ToString("N") + ".ocd");
            try {
                using (map.Read())
                    InputOutput.WriteFile(path, map, new MapFileFormat(MapFileFormatKind.OCAD, version));

                Map read = new Map(new Skia_TextMetrics(), null);
                InputOutput.ReadFile(path, read);
                using (read.Read())
                    Assert.AreEqual(isEuclidean, read.UseEuclideanMetric);
            }
            finally {
                File.Delete(path);
            }
        }
    }
}
