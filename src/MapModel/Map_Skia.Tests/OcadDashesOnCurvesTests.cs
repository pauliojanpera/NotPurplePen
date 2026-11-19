using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.Json;
using NUnit.Framework;
using PurplePen.MapModel;
using TestingUtils;

namespace Map_Skia.Tests
{
    // Tests that the dashes of an OCAD line on a Bezier curve are where the PDF export of the OCAD 2020 viewer draws them
    // in the ninth sample of the dashed lines, with the euclidean metric. OCAD measures each quarter of a curve as its
    // chord, and places the dashes within a quarter in proportion to the length of the quarter. As far as that model
    // fits OCAD's drawing (oracles/ocad-dashes.md of the palette-layer work), each end of a dash is within 0.06 mm of
    // the end that OCAD draws, within 0.11 mm on the high S curve and within 0.21 mm on the loop. Dashes that are even
    // along the length of the curve are up to 0.84 mm off.
    [TestFixture]
    public class OcadDashesOnCurvesTests
    {
        // A cell of the sample: a line of one Bezier curve, and the ends of the dashes that OCAD draws along it.
        public class CurveCase
        {
            public readonly string name;
            public readonly PointF[] points;            // the points of the curve, in mm
            public readonly PointF[][] ocadDashes;      // the start and the end of each dash of OCAD, in order, in mm

            public CurveCase(string name, PointF[] points, PointF[][] ocadDashes)
            {
                this.name = name;
                this.points = points;
                this.ocadDashes = ocadDashes;
            }

            // The distance within which the ends of the dashes agree with OCAD's ends.
            public float Tolerance => (name == "loop") ? 0.21F : (name == "S-high") ? 0.11F : 0.06F;

            public override string ToString() => name;
        }

        const string sampleFile = "dashes-9-ocad12-euclidean.ocd";
        const float lineWidth = 0.05F;      // the width of the dashed line D050 of the sample

        static Map map;

        [OneTimeSetUp]
        public void ReadSample()
        {
            map = OcadSampleDrawing.ReadTestFile(sampleFile);
        }

        [TestCaseSource(nameof(ReadCases))]
        public void DashesOnACurveAreWhereOcadDrawsThem(CurveCase cell)
        {
            RectangleF area = RectangleF.FromLTRB(cell.points.Min(p => p.X) - 0.3F, cell.points.Min(p => p.Y) - 0.3F, cell.points.Max(p => p.X) + 0.3F, cell.points.Max(p => p.Y) + 0.3F);
            List<PointF[]> dashes = OcadSampleDrawing.DrawPolylines(map, area, lineWidth).Where(points => points.All(area.Contains)).ToList();
            Assert.AreEqual(cell.ocadDashes.Length, dashes.Count, "dashes");

            float largest = 0;
            string largestEnd = null;
            for (int i = 0; i < dashes.Count; ++i) {
                float start = Distance(dashes[i][0], cell.ocadDashes[i][0]), end = Distance(dashes[i][dashes[i].Length - 1], cell.ocadDashes[i][1]);
                if (Math.Max(start, end) > largest) {
                    largest = Math.Max(start, end);
                    largestEnd = string.Format("the {0} of dash {1}", (start >= end) ? "start" : "end", i);
                }
            }
            TestContext.WriteLine("{0}: {1} dashes, largest difference {2:0.000} mm, at {3}", cell.name, dashes.Count, largest, largestEnd);
            Assert.LessOrEqual(largest, cell.Tolerance, largestEnd);
        }

        // The cells of the sample with the dashes of OCAD's PDF export, from TestFiles/ocadimport.
        static IEnumerable<CurveCase> ReadCases()
        {
            string path = TestUtil.GetTestFile(Path.Combine("ocadimport", "dashes-9-ocad12-euclidean-ocad-dashes.json"));
            using (JsonDocument document = JsonDocument.Parse(File.ReadAllText(path))) {
                List<CurveCase> cases = new List<CurveCase>();
                foreach (JsonElement cell in document.RootElement.GetProperty("cells").EnumerateArray()) {
                    PointF[] points = cell.GetProperty("points").EnumerateArray().Select(point => new PointF(point[0].GetSingle(), point[1].GetSingle())).ToArray();
                    PointF[][] dashes = cell.GetProperty("dashes").EnumerateArray()
                                            .Select(dash => new PointF[] { new PointF(dash[0].GetSingle(), dash[1].GetSingle()), new PointF(dash[2].GetSingle(), dash[3].GetSingle()) })
                                            .ToArray();
                    cases.Add(new CurveCase(cell.GetProperty("id").GetString(), points, dashes));
                }
                return cases;
            }
        }

        static float Distance(PointF a, PointF b)
        {
            return (float)Math.Sqrt((a.X - b.X) * (a.X - b.X) + (a.Y - b.Y) * (a.Y - b.Y));
        }
    }
}
