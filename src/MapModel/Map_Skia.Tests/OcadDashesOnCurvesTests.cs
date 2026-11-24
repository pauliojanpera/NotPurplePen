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
    // 2020 viewer draws them, with each metric. OCAD measures each quarter of a curve as its chord in the metric of the
    // map, and places the dashes and the glyphs within a quarter in proportion to the euclidean length of the quarter.
    // With the euclidean metric, the tests take the dashes of the ninth sample of the dashed lines and the glyphs of the
    // third. As far as the model fits OCAD's drawing (oracles/ocad-dashes.md of the palette-stack work), each end of a
    // dash is within 0.06 mm of the end that OCAD draws, within 0.11 mm on the high S curve and within 0.21 mm on the
    // loop, and each glyph is within 0.04 mm of OCAD's. Dashes and glyphs that are even along the length of the curve are
    // up to 0.84 and 0.09 mm off. With the OCAD metric, the tests take the dashes of the ninth sample, and the dashes and
    // the glyphs of the paths of the tenth sample, with straight segments, corner points and dash points: each end of a
    // dash is within 0.11 mm of OCAD's end on the curves of the ninth sample, and within 0.19 mm on the loop, and each
    // end of a dash and each glyph is within 0.07 and 0.06 mm of OCAD's on the paths of the tenth. Dashes and glyphs that
    // are even along the length in the OCAD metric are up to 0.25, 0.79 and 0.27 mm off.
    [TestFixture]
    public class OcadDashesOnCurvesTests
    {
        // A cell of a sample: a line, and the ends of the dashes that OCAD draws along it, or the centers of its glyphs.
        public class CurveCase
        {
            public readonly string sample;              // the file of the sample, in TestFiles/ocadimport
            public readonly string name;
            public readonly PointF[] points;            // the points of the line, in mm
            public readonly PointF[][] ocadDashes;      // the start and the end of each dash of OCAD, in order, in mm, or null
            public readonly PointF[] ocadGlyphs;        // the center of each glyph of OCAD, in order, in mm, or null
            public readonly float lineWidth;            // the width of the dashed line, in mm
            public readonly float tolerance;            // the distance within which the dashes or the glyphs agree with OCAD's

            public CurveCase(string sample, string name, PointF[] points, PointF[][] ocadDashes, PointF[] ocadGlyphs, float lineWidth, float tolerance)
            {
                this.sample = sample;
                this.name = name;
                this.points = points;
                this.ocadDashes = ocadDashes;
                this.ocadGlyphs = ocadGlyphs;
                this.lineWidth = lineWidth;
                this.tolerance = tolerance;
            }

            public override string ToString() => Path.GetFileNameWithoutExtension(sample) + " " + name;
        }

        static readonly Dictionary<string, Map> samples = new Dictionary<string, Map>();

        // The map of a sample file, read once.
        static Map GetSample(string fileName)
        {
            lock (samples) {
                if (!samples.TryGetValue(fileName, out Map map)) {
                    map = OcadSampleDrawing.ReadTestFile(fileName);
                    samples.Add(fileName, map);
                }
                return map;
            }
        }

        [TestCaseSource(nameof(ReadCurveCases))]
        public void DashesOnACurveAreWhereOcadDrawsThem(CurveCase cell)
        {
            List<PointF[]> dashes = DrawDashes(cell);
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
            Assert.LessOrEqual(largest, cell.tolerance, largestEnd);
        }

        // On a path of several parts, the dashes of OCAD and of the drawing can start at different ends, so each end is
        // compared with the nearest end of the other side, both ways.
        [TestCaseSource(nameof(ReadPathCases))]
        public void DashesOnAPathAreWhereOcadDrawsThem(CurveCase cell)
        {
            List<PointF[]> dashes = DrawDashes(cell);
            Assert.AreEqual(cell.ocadDashes.Length, dashes.Count, "dashes");

            List<PointF> ends = dashes.SelectMany(dash => new PointF[] { dash[0], dash[dash.Length - 1] }).ToList();
            List<PointF> ocadEnds = cell.ocadDashes.SelectMany(dash => dash).ToList();
            AssertNearEachOther(cell, ends, ocadEnds, "end of a dash");
        }

        [TestCaseSource(nameof(ReadGlyphCases))]
        public void GlyphsOnACurveAreWhereOcadDrawsThem(CurveCase cell)
        {
            RectangleF area = AreaAround(cell.points);
            List<PointF> glyphs = OcadSampleDrawing.DrawFilledEllipseCenters(GetSample(cell.sample), area).Where(area.Contains).ToList();
            Assert.AreEqual(cell.ocadGlyphs.Length, glyphs.Count, "glyphs");
            AssertNearEachOther(cell, glyphs, cell.ocadGlyphs.ToList(), "glyph");
        }

        // The dashes that the drawing of the sample of a cell draws within the area of the cell, with the width of its
        // line.
        static List<PointF[]> DrawDashes(CurveCase cell)
        {
            RectangleF area = AreaAround(cell.points);
            return OcadSampleDrawing.DrawPolylines(GetSample(cell.sample), area, cell.lineWidth).Where(points => points.All(area.Contains)).ToList();
        }

        // Assert that each point of the drawing is within the tolerance of a cell from a point of OCAD, and each point of
        // OCAD from a point of the drawing.
        static void AssertNearEachOther(CurveCase cell, List<PointF> drawn, List<PointF> ocad, string what)
        {
            float largest = 0;
            string largestPoint = null;
            foreach (PointF point in drawn) {
                float difference = ocad.Min(other => Distance(point, other));
                if (difference > largest) {
                    largest = difference;
                    largestPoint = string.Format("the {0} of the drawing at {1}", what, point);
                }
            }
            foreach (PointF point in ocad) {
                float difference = drawn.Min(other => Distance(point, other));
                if (difference > largest) {
                    largest = difference;
                    largestPoint = string.Format("the {0} of OCAD at {1}", what, point);
                }
            }
            TestContext.WriteLine("{0}: {1} points, largest difference {2:0.000} mm, at {3}", cell.name, drawn.Count, largest, largestPoint);
            Assert.LessOrEqual(largest, cell.tolerance, largestPoint);
        }

        // The cells of one Bezier curve of the ninth sample with the dashes of OCAD's PDF exports, from
        // TestFiles/ocadimport, with each metric.
        static IEnumerable<CurveCase> ReadCurveCases()
        {
            foreach (CurveCase cell in ReadCells("dashes-9-ocad12-euclidean.ocd", "dashes-9-ocad12-euclidean-ocad-dashes.json",
                                                 name => (name == "loop") ? 0.21F : (name == "S-high") ? 0.11F : 0.06F))
                yield return cell;
            foreach (CurveCase cell in ReadCells("dashes-9-ocad12-ocad-metric.ocd", "dashes-9-ocad12-ocad-metric-ocad-dashes.json",
                                                 name => (name == "loop") ? 0.19F : 0.11F))
                yield return cell;
        }

        // The cells of the paths of the tenth sample with the dashes of OCAD's PDF export with the OCAD metric.
        static IEnumerable<CurveCase> ReadPathCases()
        {
            return ReadCells("dashes-10-ocad12-ocad-metric.ocd", "dashes-10-ocad12-ocad-metric-ocad-dashes.json", name => 0.07F);
        }

        // The cells of the S curves of the third sample, and of the paths of the tenth sample with the OCAD metric, with
        // the glyphs of OCAD's PDF exports.
        static IEnumerable<CurveCase> ReadGlyphCases()
        {
            foreach (CurveCase cell in ReadCells("dashes-3-ocad12-euclidean.ocd", "dashes-3-ocad12-euclidean-ocad-glyphs.json", name => 0.04F))
                yield return cell;
            foreach (CurveCase cell in ReadCells("dashes-10-ocad12-ocad-metric.ocd", "dashes-10-ocad12-ocad-metric-ocad-glyphs.json", name => 0.06F))
                yield return cell;
        }

        // Read the cells of a file of OCAD's dashes or glyphs: each cell's name and the points of its line, and what OCAD
        // draws along it.
        // sample: the file of the sample, in TestFiles/ocadimport.
        // fileName: the name of the file of OCAD's dashes or glyphs, in TestFiles/ocadimport.
        // getTolerance: the tolerance of a cell, by its name.
        static List<CurveCase> ReadCells(string sample, string fileName, Func<string, float> getTolerance)
        {
            string path = TestUtil.GetTestFile(Path.Combine("ocadimport", fileName));
            using (JsonDocument document = JsonDocument.Parse(File.ReadAllText(path))) {
                JsonElement root = document.RootElement;
                List<CurveCase> cases = new List<CurveCase>();
                foreach (JsonElement cell in root.GetProperty("cells").EnumerateArray()) {
                    string name = cell.GetProperty("id").GetString();
                    PointF[] points = cell.GetProperty("points").EnumerateArray().Select(point => new PointF(point[0].GetSingle(), point[1].GetSingle())).ToArray();
                    PointF[][] dashes = null;
                    PointF[] glyphs = null;
                    if (cell.TryGetProperty("dashes", out JsonElement dashArray)) {
                        dashes = dashArray.EnumerateArray()
                            .Select(dash => new PointF[] { new PointF(dash[0].GetSingle(), dash[1].GetSingle()), new PointF(dash[2].GetSingle(), dash[3].GetSingle()) })
                            .ToArray();
                    }
                    else {
                        glyphs = cell.GetProperty("glyphs").EnumerateArray().Select(glyph => new PointF(glyph[0].GetSingle(), glyph[1].GetSingle())).ToArray();
                    }
                    cases.Add(new CurveCase(sample, name, points, dashes, glyphs, GetLineWidth(root, cell), getTolerance(name)));
                }
                return cases;
            }
        }

        // The width of the dashed line of a cell: from the symbol of the cell, or from the one symbol of the file.
        static float GetLineWidth(JsonElement root, JsonElement cell)
        {
            JsonElement symbol;
            if (cell.TryGetProperty("symbol", out JsonElement name) && root.TryGetProperty("symbols", out JsonElement symbols))
                symbol = symbols.GetProperty(name.GetString());
            else if (!root.TryGetProperty("symbol", out symbol))
                return 0;

            if (symbol.TryGetProperty("linewidth", out JsonElement width) || symbol.TryGetProperty("lineWidth", out width))
                return width.GetSingle();
            return 0;
        }

        // The area of a cell: the bounding box of the points of its line, which holds the line, and 0.3 mm around it.
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
