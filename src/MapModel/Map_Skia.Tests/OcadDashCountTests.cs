using System;
using System.Drawing;
using NUnit.Framework;
using PurplePen.MapModel;

namespace Map_Skia.Tests
{
    // OCAD counts the dashes of a line, and the glyphs spaced along it, from a length in which each Bezier curve is four
    // chords and each part is rounded to 0.01 mm. At a half, it gives the line the even number of dashes. It gives a short
    // line a gap only when the line is longer than half of each end dash and a gap, draws a line whose one gap would leave
    // end dashes of 0 as solid, and puts a secondary gap only in a dash longer than half of a dash. The tests count the
    // dashes of cells of the samples of the dashed lines, which the OCAD 2020 viewer draws as they expect.
    [TestFixture]
    public class OcadDashCountTests
    {
        // fileName: a sample file in TestFiles/ocadimport.
        // startX: the start of the line of the cell, in mm on the paper.
        // startY: the start of the line of the cell.
        // runs: the number of dashes, or of glyphs, that OCAD draws.
        [TestCase("dashes-2-ocad12-euclidean.ocd", 2F, -2F, 10, TestName = "{m}(A1-1, a half, 9.5 dashes)")]
        [TestCase("dashes-2-ocad12-euclidean.ocd", 152F, -2F, 10, TestName = "{m}(A1-6, 10.48 dashes)")]
        [TestCase("dashes-2-ocad12-euclidean.ocd", 182F, -2F, 10, TestName = "{m}(A1-7, a half, 10.5 dashes)")]
        [TestCase("dashes-2-ocad12-euclidean.ocd", 212F, -2F, 11, TestName = "{m}(A1-8, 10.52 dashes)")]
        [TestCase("dashes-2-ocad12-euclidean.ocd", 212F, -6F, 1, TestName = "{m}(A2-8, one gap and end dashes of 0)")]
        [TestCase("dashes-2-ocad12-euclidean.ocd", 32F, -10F, 1, TestName = "{m}(B1-4, a short line at its limit)")]
        [TestCase("dashes-2-ocad12-euclidean.ocd", 42F, -10F, 2, TestName = "{m}(B1-5, a short line past its limit)")]
        [TestCase("dashes-2-ocad12-euclidean.ocd", 152F, -10F, 1, TestName = "{m}(B1-16, a short line at its limit)")]
        [TestCase("dashes-2-ocad12-euclidean.ocd", 162F, -10F, 2, TestName = "{m}(B1-17, a short line past its limit)")]
        [TestCase("dashes-2-ocad12-euclidean.ocd", 142F, -14F, 1, TestName = "{m}(B2-15, a short line at its limit, end secondary gaps)")]
        [TestCase("dashes-2-ocad12-euclidean.ocd", 62F, -38.7F, 10, TestName = "{m}(DS-3, S curve)")]
        [TestCase("dashes-2-ocad12-euclidean.ocd", 92F, -38.7F, 10, TestName = "{m}(DS-4, S curve)")]
        [TestCase("dashes-2-ocad12-euclidean.ocd", 242F, -38.7F, 11, TestName = "{m}(DS-9, S curve)")]
        [TestCase("dashes-2-ocad12-euclidean.ocd", 62F, -69.1F, 10, TestName = "{m}(Darch-3, arch)")]
        [TestCase("dashes-2-ocad12-euclidean.ocd", 92F, -69.1F, 10, TestName = "{m}(Darch-4, arch)")]
        [TestCase("dashes-2-ocad12-euclidean.ocd", 242F, -69.1F, 11, TestName = "{m}(Darch-9, arch)")]
        [TestCase("dashes-2-ocad12-euclidean.ocd", 62F, -54.88F, 11, TestName = "{m}(Dgentle-3, gentle curve)")]
        [TestCase("dashes-2-ocad12-euclidean.ocd", 2F, -75.41F, 10, TestName = "{m}(D4F-1, S curve as 64 straight parts)")]
        [TestCase("dashes-2-ocad12-ocad-metric.ocd", 92F, -69.1F, 11, TestName = "{m}(Darch-4, arch, OCAD metric)")]
        [TestCase("dashes-2-ocad12-ocad-metric.ocd", 242F, -69.1F, 12, TestName = "{m}(Darch-9, arch, OCAD metric)")]
        [TestCase("dashes-2-ocad12-ocad-metric.ocd", 242F, -38.7F, 13, TestName = "{m}(DS-9, S curve, OCAD metric)")]
        [TestCase("dashes-3-ocad12-euclidean.ocd", 50F, -30F, 3, TestName = "{m}(MS2-5, a middle dash of half a dash, no secondary gap)")]
        [TestCase("dashes-3-ocad12-euclidean.ocd", 62F, -30F, 4, TestName = "{m}(MS2-6, a middle dash longer than half a dash, a secondary gap)")]
        [TestCase("dashes-3-ocad12-euclidean.ocd", 62F, -46F, 5, TestName = "{m}(GSstraight-3, glyphs, a half)")]
        [TestCase("dashes-3-ocad12-euclidean.ocd", 152F, -46F, 5, TestName = "{m}(GSstraight-6, glyphs, a half)")]
        [TestCase("dashes-3-ocad12-euclidean.ocd", 62F, -50F, 1, TestName = "{m}(GSshort-6, glyphs, a half)")]
        [TestCase("dashes-3-ocad12-euclidean.ocd", 62F, -60.17F, 9, TestName = "{m}(GScurve-3, glyphs, S curve)")]
        [TestCase("dashes-3-ocad12-euclidean.ocd", 92F, -60.17F, 9, TestName = "{m}(GScurve-4, glyphs, S curve)")]
        public void LineHasTheDashesThatOcadDraws(string fileName, float startX, float startY, int runs)
        {
            Map map = OcadSampleDrawing.ReadTestFile(fileName);
            Assert.AreEqual(runs, OcadSampleDrawing.CountRunsAlong(map, FindPathStartingAt(map, new PointF(startX, startY)).FlattenedPoints));
        }

        // Find the path of the line of a map that starts at a point.
        // map: the map.
        // start: the point, in mm on the paper.
        static SymPath FindPathStartingAt(Map map, PointF start)
        {
            using (map.Read()) {
                foreach (Symbol symbol in map.AllSymbols) {
                    if (symbol is LineSymbol line) {
                        PointF first = line.Path.Points[0];
                        if (Math.Abs(first.X - start.X) < 0.005F && Math.Abs(first.Y - start.Y) < 0.005F)
                            return line.Path;
                    }
                }
            }
            throw new ArgumentException(string.Format("No line of the map starts at {0}.", start));
        }
    }
}
