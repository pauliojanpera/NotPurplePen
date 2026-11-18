using System;
using System.Drawing;
using System.IO;
using NUnit.Framework;
using PurplePen.Graphics2D;
using PurplePen.MapModel;
using TestingUtils;

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
            Map map = ReadTestFile(fileName);
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
            Map map = ReadTestFile(fileName);
            Assert.AreEqual(dashes, CountDashesAlong(map, new PointF(92.0F, -112.22F), new PointF(101.0F, -103.22F)));
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

        // Read a file of TestFiles/ocadimport into a new map.
        // fileName: the name of the file.
        static Map ReadTestFile(string fileName)
        {
            string path = TestUtil.GetTestFile(Path.Combine("ocadimport", fileName));
            Map map = new Map(new Skia_TextMetrics(), new Skia_FileLoader(Path.GetDirectoryName(path)));
            InputOutput.ReadFile(path, map);
            return map;
        }

        // Count the dashes that the drawing of a map has along a straight line: the runs of covered pixels at the middle
        // of the line, drawn without antialiasing at 0.02 mm to the pixel.
        // map: the map.
        // start: one end of the line, in mm on the paper.
        // end: the other end of the line.
        static int CountDashesAlong(Map map, PointF start, PointF end)
        {
            const float pixelSize = 0.02F, step = 0.01F;
            RectangleF area = RectangleF.FromLTRB(Math.Min(start.X, end.X) - 1, Math.Min(start.Y, end.Y) - 1, Math.Max(start.X, end.X) + 1, Math.Max(start.Y, end.Y) + 1);
            int width = (int)Math.Ceiling(area.Width / pixelSize), height = (int)Math.Ceiling(area.Height / pixelSize);
            float length = (float)Math.Sqrt((end.X - start.X) * (end.X - start.X) + (end.Y - start.Y) * (end.Y - start.Y));

            using (map.Read())
            using (Skia_BitmapGraphicsTarget target = new Skia_BitmapGraphicsTarget(width, height, true, CmykColor.FromCmyka(0, 0, 0, 0, 0), area, true)) {
                target.PushAntiAliasing(false);
                map.Draw(target, area, new RenderOptions() { minResolution = pixelSize, renderTemplates = RenderTemplateOption.MapOnly }, null);
                target.PopAntiAliasing();
                using (Skia_Bitmap bitmap = (Skia_Bitmap)target.FinishBitmap()) {
                    int dashes = 0;
                    bool wasCovered = false;
                    for (float distance = 0; distance <= length; distance += step) {
                        float x = start.X + (end.X - start.X) * distance / length;
                        float y = start.Y + (end.Y - start.Y) * distance / length;
                        bool isCovered = bitmap.Bitmap.GetPixel((int)((x - area.Left) / pixelSize), (int)((area.Bottom - y) / pixelSize)).Alpha != 0;
                        if (isCovered && !wasCovered)
                            ++dashes;
                        wasCovered = isCovered;
                    }
                    return dashes;
                }
            }
        }
    }
}
