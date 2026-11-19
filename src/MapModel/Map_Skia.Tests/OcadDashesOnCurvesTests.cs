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
    // Tests that the dashes and the spaced glyphs of an OCAD line on a Bezier curve are where the PDF export of the OCAD
    // 2020 viewer draws them, with the euclidean metric: the dashes of the ninth sample of the dashed lines, and the
    // glyphs of the third. OCAD measures each quarter of a curve as its chord, and places the dashes and the glyphs within
    // a quarter in proportion to the length of the quarter. As far as that model fits OCAD's drawing
    // (oracles/ocad-dashes.md of the palette-layer work), each end of a dash is within 0.06 mm of the end that OCAD draws,
    // within 0.11 mm on the high S curve and within 0.21 mm on the loop, and each glyph is within 0.04 mm of OCAD's.
    // Dashes and glyphs that are even along the length of the curve are up to 0.84 and 0.09 mm off.
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

        // A cell of the third sample: a line of glyphs spaced along one Bezier curve, and the centers of the glyphs that
        // OCAD draws along it.
        public class GlyphCurveCase
        {
            public readonly string name;
            public readonly PointF[] points;            // the points of the curve, in mm
            public readonly PointF[] ocadGlyphs;        // the center of each glyph of OCAD, in order, in mm

            public GlyphCurveCase(string name, PointF[] points, PointF[] ocadGlyphs)
            {
                this.name = name;
                this.points = points;
                this.ocadGlyphs = ocadGlyphs;
            }

            public override string ToString() => name;
        }

        const float lineWidth = 0.05F;              // the width of the dashed line D050 of the ninth sample
        const float glyphTolerance = 0.04F;         // the distance within which the glyphs agree with OCAD's glyphs

        static Map dashSample, glyphSample;

        [OneTimeSetUp]
        public void ReadSamples()
        {
            dashSample = OcadSampleDrawing.ReadTestFile("dashes-9-ocad12-euclidean.ocd");
            glyphSample = OcadSampleDrawing.ReadTestFile("dashes-3-ocad12-euclidean.ocd");
        }

        [TestCaseSource(nameof(ReadCases))]
        public void DashesOnACurveAreWhereOcadDrawsThem(CurveCase cell)
        {
            RectangleF area = AreaAround(cell.points);
            List<PointF[]> dashes = OcadSampleDrawing.DrawPolylines(dashSample, area, lineWidth).Where(points => points.All(area.Contains)).ToList();
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

        [TestCaseSource(nameof(ReadGlyphCases))]
        public void GlyphsOnACurveAreWhereOcadDrawsThem(GlyphCurveCase cell)
        {
            RectangleF area = AreaAround(cell.points);
            List<PointF> glyphs = OcadSampleDrawing.DrawFilledEllipseCenters(glyphSample, area).Where(area.Contains).ToList();
            Assert.AreEqual(cell.ocadGlyphs.Length, glyphs.Count, "glyphs");

            float largest = 0;
            int largestGlyph = -1;
            for (int i = 0; i < glyphs.Count; ++i) {
                float difference = Distance(glyphs[i], cell.ocadGlyphs[i]);
                if (difference > largest) {
                    largest = difference;
                    largestGlyph = i;
                }
            }
            TestContext.WriteLine("{0}: {1} glyphs, largest difference {2:0.000} mm, at glyph {3}", cell.name, glyphs.Count, largest, largestGlyph);
            Assert.LessOrEqual(largest, glyphTolerance, "glyph {0}", largestGlyph);
        }

        // The cells of the ninth sample with the dashes of OCAD's PDF export, from TestFiles/ocadimport.
        static IEnumerable<CurveCase> ReadCases()
        {
            return ReadCells("dashes-9-ocad12-euclidean-ocad-dashes.json", (name, points, cell) => new CurveCase(name, points, cell.GetProperty("dashes").EnumerateArray()
                .Select(dash => new PointF[] { new PointF(dash[0].GetSingle(), dash[1].GetSingle()), new PointF(dash[2].GetSingle(), dash[3].GetSingle()) })
                .ToArray()));
        }

        // The cells of the third sample with the glyphs of OCAD's PDF export, from TestFiles/ocadimport.
        static IEnumerable<GlyphCurveCase> ReadGlyphCases()
        {
            return ReadCells("dashes-3-ocad12-euclidean-ocad-glyphs.json", (name, points, cell) => new GlyphCurveCase(name, points, cell.GetProperty("glyphs").EnumerateArray()
                .Select(glyph => new PointF(glyph[0].GetSingle(), glyph[1].GetSingle()))
                .ToArray()));
        }

        // Read the cells of a file of OCAD's dashes or glyphs: each cell's name and the points of its curve, and what
        // OCAD draws along it.
        // fileName: the name of the file in TestFiles/ocadimport.
        // makeCase: makes the case of a cell from its name, its points and its element of the file.
        static List<T> ReadCells<T>(string fileName, Func<string, PointF[], JsonElement, T> makeCase)
        {
            string path = TestUtil.GetTestFile(Path.Combine("ocadimport", fileName));
            using (JsonDocument document = JsonDocument.Parse(File.ReadAllText(path))) {
                List<T> cases = new List<T>();
                foreach (JsonElement cell in document.RootElement.GetProperty("cells").EnumerateArray()) {
                    PointF[] points = cell.GetProperty("points").EnumerateArray().Select(point => new PointF(point[0].GetSingle(), point[1].GetSingle())).ToArray();
                    cases.Add(makeCase(cell.GetProperty("id").GetString(), points, cell));
                }
                return cases;
            }
        }

        // The area of a cell: the bounding box of the points of its curve, which holds the curve, and 0.3 mm around it.
        static RectangleF AreaAround(PointF[] points)
        {
            return RectangleF.FromLTRB(points.Min(p => p.X) - 0.3F, points.Min(p => p.Y) - 0.3F, points.Max(p => p.X) + 0.3F, points.Max(p => p.Y) + 0.3F);
        }

        static float Distance(PointF a, PointF b)
        {
            return (float)Math.Sqrt((a.X - b.X) * (a.X - b.X) + (a.Y - b.Y) * (a.Y - b.Y));
        }
    }
}
