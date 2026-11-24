using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using PurplePen.Graphics2D;
using PurplePen.MapModel;

namespace Map_Skia.Tests
{
    // Tests that a dash of an OCAD line leaves out the vertex next to its start or its end when the piece of the dash
    // between them is short, as the PDF export of the OCAD 2020 viewer draws the seventh sample of the dashed lines: on a
    // single line when the piece is shorter than 0.4 times the width of the line; on a double line when the piece is
    // shorter than 0.05 mm, and then on each border when its own piece is shorter than 0.4 times the width of the border.
    [TestFixture]
    public class OcadDashVertexTests
    {
        // A cell of the sample: a line that turns right at its second vertex, where a dash starts the piece before the
        // vertex or ends the piece after it, and whether OCAD keeps the vertex in that dash on the outer and the inner side
        // of the turn.
        public class VertexCase
        {
            public readonly string name;
            public readonly float width;            // the width of the line drawn, in mm
            public readonly float offset;           // the distance of a border from the path, or 0 for a line on the path
            public readonly PointF first, vertex, last;   // the points of the cell before, at and after its second vertex
            public readonly float piece;            // the distance of the boundary of the dash from the vertex, in mm
            public readonly bool isOuterKept;       // whether OCAD keeps the vertex: on the line, or on the outer border
            public readonly bool isInnerKept;       // whether OCAD keeps the vertex on the inner border

            public VertexCase(string name, float width, float offset, float x1, float y1, float x2, float y2, float x3, float y3, float piece, bool isOuterKept, bool isInnerKept)
            {
                this.name = name;
                this.width = width;
                this.offset = offset;
                first = new PointF(x1, y1);
                vertex = new PointF(x2, y2);
                last = new PointF(x3, y3);
                this.piece = piece;
                this.isOuterKept = isOuterKept;
                this.isInnerKept = isInnerKept;
            }

            public bool IsStart => name.Contains("-start-");

            public override string ToString() => name;
        }

        // The cells of dashes-7-ocad12-euclidean.ocd, from oracles/ocad-dashes.md of the palette-layer work: the name of
        // the cell, the width and the offset of its line, its points, its piece, and the outcome of OCAD's PDF export.
        static readonly VertexCase[] cases = {
            new VertexCase("L100-30-start-7", 1F, 0F, 91F, -13F, 93.35F, -8.93F, 96.86F, -6.91F, 0.350038F, false, false),
            new VertexCase("L100-30-start-9", 1F, 0F, 119F, -13F, 121.76F, -8.21F, 125.6F, -5.99F, 0.449983F, true, true),
            new VertexCase("L100-30-end-7", 1F, 0F, 91F, -26F, 93.12F, -22.32F, 98.1F, -19.45F, 0.349895F, false, false),
            new VertexCase("L100-30-end-9", 1F, 0F, 119F, -26F, 121.09F, -22.39F, 126.17F, -19.46F, 0.450104F, true, true),
            new VertexCase("L100-60-start-7", 1F, 0F, 91F, -39F, 93.65F, -34.41F, 98.1F, -34.41F, 0.350023F, false, false),
            new VertexCase("L100-60-start-9", 1F, 0F, 119F, -39F, 121.42F, -34.82F, 127.85F, -34.82F, 0.449995F, true, true),
            new VertexCase("L100-60-end-7", 1F, 0F, 91F, -52F, 93.98F, -46.85F, 99.23F, -46.85F, 0.349989F, false, false),
            new VertexCase("L100-60-end-9", 1F, 0F, 119F, -52F, 121.09F, -48.38F, 126.96F, -48.38F, 0.449995F, true, true),
            new VertexCase("L100-90-start-7", 1F, 0F, 91F, -65F, 93.35F, -60.93F, 96.86F, -62.95F, 0.350038F, false, false),
            new VertexCase("L100-90-start-9", 1F, 0F, 119F, -65F, 121.76F, -60.21F, 125.6F, -62.43F, 0.449983F, true, true),
            new VertexCase("L100-90-end-7", 1F, 0F, 91F, -78F, 93.12F, -74.32F, 98.1F, -77.19F, 0.349895F, false, false),
            new VertexCase("L100-90-end-9", 1F, 0F, 119F, -78F, 121.09F, -74.39F, 126.17F, -77.32F, 0.450104F, true, true),
            new VertexCase("L100-120-start-7", 1F, 0F, 91F, -91F, 93.35F, -86.93F, 95.37F, -90.44F, 0.350038F, false, false),
            new VertexCase("L100-120-start-9", 1F, 0F, 119F, -91F, 121.76F, -86.21F, 123.98F, -90.05F, 0.449983F, true, true),
            new VertexCase("L100-120-end-7", 1F, 0F, 91F, -104F, 93.12F, -100.32F, 95.99F, -105.3F, 0.349895F, false, false),
            new VertexCase("L100-120-end-9", 1F, 0F, 119F, -104F, 121.09F, -100.39F, 124.02F, -105.47F, 0.450104F, true, true),
            new VertexCase("L100-150-start-7", 1F, 0F, 91F, -117F, 93.65F, -112.41F, 93.65F, -116.86F, 0.350023F, false, false),
            new VertexCase("L100-150-start-9", 1F, 0F, 119F, -117F, 121.42F, -112.82F, 121.42F, -119.25F, 0.449995F, true, true),
            new VertexCase("L100-150-end-7", 1F, 0F, 91F, -130F, 93.98F, -124.85F, 93.98F, -130.1F, 0.349989F, false, false),
            new VertexCase("L100-150-end-9", 1F, 0F, 119F, -130F, 121.09F, -126.38F, 121.09F, -132.25F, 0.449995F, true, true),
            new VertexCase("L030-30-start-7", 0.3F, 0F, 91F, -143F, 93.08F, -139.41F, 96.63F, -137.36F, 0.0999803F, false, false),
            new VertexCase("L030-30-start-9", 0.3F, 0F, 119F, -143F, 121.46F, -138.73F, 125.4F, -136.45F, 0.139887F, true, true),
            new VertexCase("L030-30-end-7", 0.3F, 0F, 91F, -156F, 93.2F, -152.18F, 97.91F, -149.46F, 0.1001F, false, false),
            new VertexCase("L030-30-end-9", 0.3F, 0F, 119F, -156F, 121.06F, -152.44F, 125.66F, -149.79F, 0.140011F, true, true),
            new VertexCase("L030-90-start-7", 0.3F, 0F, 91F, -195F, 93.08F, -191.41F, 96.63F, -193.46F, 0.0999803F, false, false),
            new VertexCase("L030-90-start-9", 0.3F, 0F, 119F, -195F, 121.46F, -190.73F, 125.4F, -193.01F, 0.139887F, true, true),
            new VertexCase("L030-90-end-7", 0.3F, 0F, 91F, -208F, 93.2F, -204.18F, 97.91F, -206.9F, 0.1001F, false, false),
            new VertexCase("L030-90-end-9", 0.3F, 0F, 119F, -208F, 121.06F, -204.44F, 125.66F, -207.09F, 0.140011F, true, true),
            new VertexCase("L030-150-start-7", 0.3F, 0F, 91F, -247F, 93.09F, -243.38F, 93.09F, -247.5F, 0.100005F, false, false),
            new VertexCase("L030-150-start-9", 0.3F, 0F, 119F, -247F, 121.09F, -243.38F, 121.09F, -249.78F, 0.140006F, true, true),
            new VertexCase("L030-150-end-7", 0.3F, 0F, 91F, -260F, 94.06F, -254.7F, 94.06F, -259.66F, 0.100022F, false, false),
            new VertexCase("L030-150-end-9", 0.3F, 0F, 119F, -260F, 122.06F, -254.7F, 122.06F, -259.72F, 0.140022F, true, true),
            new VertexCase("L006-90-start-2", 0.06F, 0F, 21F, -273F, 24.21F, -267.44F, 27.83F, -269.53F, 0.0200258F, false, false),
            new VertexCase("L006-90-start-4", 0.06F, 0F, 49F, -273F, 51.38F, -268.87F, 55.39F, -271.18F, 0.0300162F, true, true),
            new VertexCase("L006-90-end-2", 0.06F, 0F, 21F, -286F, 23.09F, -282.39F, 27.55F, -284.96F, 0.0199432F, false, false),
            new VertexCase("L006-90-end-4", 0.06F, 0F, 49F, -286F, 51.3F, -282.02F, 56.02F, -284.74F, 0.0298724F, true, true),
            new VertexCase("L010-90-start-2", 0.1F, 0F, 21F, -299F, 23.38F, -294.87F, 27.39F, -297.18F, 0.0300162F, false, false),
            new VertexCase("L010-90-start-5", 0.1F, 0F, 63F, -299F, 65.38F, -294.89F, 69.35F, -297.18F, 0.0498746F, true, true),
            new VertexCase("L010-90-end-2", 0.1F, 0F, 21F, -312F, 23.3F, -308.02F, 28.02F, -310.74F, 0.0298724F, false, false),
            new VertexCase("L010-90-end-5", 0.1F, 0F, 63F, -312F, 65.14F, -308.28F, 69.71F, -310.92F, 0.0499939F, true, true),
            new VertexCase("L020-90-start-2", 0.2F, 0F, 21F, -325F, 24.23F, -319.41F, 29.85F, -322.65F, 0.0599987F, false, false),
            new VertexCase("L020-90-start-7", 0.2F, 0F, 91F, -325F, 94.2F, -319.46F, 97.68F, -321.47F, 0.120081F, true, true),
            new VertexCase("L020-90-end-2", 0.2F, 0F, 21F, -338F, 23.2F, -334.18F, 27.85F, -336.87F, 0.0599242F, false, false),
            new VertexCase("L020-90-end-7", 0.2F, 0F, 91F, -338F, 93.14F, -334.28F, 97.81F, -336.98F, 0.119956F, true, true),
            new VertexCase("L050-90-start-2", 0.5F, 0F, 21F, -351F, 23.2F, -347.18F, 26.83F, -349.27F, 0.150083F, false, false),
            new VertexCase("L050-90-start-9", 0.5F, 0F, 119F, -351F, 121.38F, -346.89F, 124.99F, -348.97F, 0.299935F, true, true),
            new VertexCase("L050-90-end-2", 0.5F, 0F, 21F, -364F, 23.15F, -360.28F, 27.87F, -363F, 0.149939F, false, false),
            new VertexCase("L050-90-end-9", 0.5F, 0F, 119F, -364F, 121.14F, -360.28F, 126.07F, -363.13F, 0.300056F, true, true),
            new VertexCase("DBM-90-start-12", 0.5F, 0F, 161F, -455F, 163.2F, -451.18F, 166.83F, -453.27F, 0.150083F, false, false),
            new VertexCase("DBM-90-start-15", 0.5F, 0F, 203F, -455F, 205.38F, -450.89F, 208.99F, -452.97F, 0.299935F, true, true),
            new VertexCase("DBM-90-end-12", 0.5F, 0F, 161F, -468F, 163.15F, -464.28F, 167.87F, -467F, 0.149939F, false, false),
            new VertexCase("DBM-90-end-15", 0.5F, 0F, 203F, -468F, 205.14F, -464.28F, 210.07F, -467.13F, 0.300056F, true, true),
            new VertexCase("DB10-90-start-1", 0.1F, 0.35F, 7F, -377F, 10.21F, -371.44F, 13.83F, -373.53F, 0.0200258F, false, false),
            new VertexCase("DB10-90-start-7", 0.1F, 0.35F, 91F, -377F, 94.23F, -371.41F, 99.85F, -374.65F, 0.0599987F, true, true),
            new VertexCase("DB10-90-end-1", 0.1F, 0.35F, 7F, -390F, 9.09F, -386.39F, 13.55F, -388.96F, 0.0199432F, false, false),
            new VertexCase("DB10-90-end-7", 0.1F, 0.35F, 91F, -390F, 93.2F, -386.18F, 97.85F, -388.87F, 0.0599242F, true, true),
            new VertexCase("DB15-90-start-1", 0.15F, 0.375F, 7F, -403F, 10.21F, -397.44F, 13.83F, -399.53F, 0.0200258F, false, false),
            new VertexCase("DB15-90-start-7", 0.15F, 0.375F, 91F, -403F, 94.23F, -397.41F, 99.85F, -400.65F, 0.0599987F, true, true),
            new VertexCase("DB15-90-end-1", 0.15F, 0.375F, 7F, -416F, 9.09F, -412.39F, 13.55F, -414.96F, 0.0199432F, false, false),
            new VertexCase("DB15-90-end-7", 0.15F, 0.375F, 91F, -416F, 93.2F, -412.18F, 97.85F, -414.87F, 0.0599242F, true, true),
            new VertexCase("DB30-90-start-1", 0.3F, 0.45F, 7F, -429F, 10.21F, -423.44F, 13.83F, -425.53F, 0.0200258F, false, false),
            new VertexCase("DB30-90-start-7", 0.3F, 0.45F, 91F, -429F, 94.23F, -423.41F, 99.85F, -426.65F, 0.0599987F, true, true),
            new VertexCase("DB30-90-end-3", 0.3F, 0.45F, 35F, -442F, 37.26F, -438.08F, 41.95F, -440.79F, 0.0400675F, false, false),
            new VertexCase("DB30-90-end-7", 0.3F, 0.45F, 91F, -442F, 93.2F, -438.18F, 97.85F, -440.87F, 0.0599242F, true, true),
            new VertexCase("DB15-150-start-1", 0.15F, 0.375F, 7F, -507F, 9.18F, -503.23F, 9.18F, -507.6F, 0.0199667F, false, false),
            new VertexCase("DB15-150-start-7", 0.15F, 0.375F, 91F, -507F, 93.42F, -502.82F, 93.42F, -507.44F, 0.0599959F, true, true),
            new VertexCase("DB15-150-end-1", 0.15F, 0.375F, 7F, -520F, 10.06F, -514.7F, 10.06F, -519.54F, 0.0200218F, false, false),
            new VertexCase("DB15-150-end-7", 0.15F, 0.375F, 91F, -520F, 93.09F, -516.38F, 93.09F, -521.6F, 0.0599952F, true, true),
            new VertexCase("DB15-30-start-1", 0.15F, 0.375F, 7F, -481F, 10.21F, -475.44F, 13.83F, -473.35F, 0.0200258F, false, false),
            new VertexCase("DB15-30-start-12", 0.15F, 0.375F, 161F, -481F, 163.2F, -477.18F, 166.83F, -475.09F, 0.150083F, true, false),
            new VertexCase("DB15-30-start-14", 0.15F, 0.375F, 189F, -481F, 191.7F, -476.31F, 195.76F, -473.96F, 0.250026F, true, true),
            new VertexCase("DB15-30-end-1", 0.15F, 0.375F, 7F, -494F, 9.09F, -490.39F, 13.55F, -487.82F, 0.0199432F, false, false),
            new VertexCase("DB15-30-end-12", 0.15F, 0.375F, 161F, -494F, 163.15F, -490.28F, 167.87F, -487.56F, 0.149939F, true, false),
            new VertexCase("DB15-30-end-14", 0.15F, 0.375F, 189F, -494F, 192.05F, -488.73F, 196.53F, -486.15F, 0.250212F, true, true),
        };

        static Map map;

        [OneTimeSetUp]
        public void ReadSample()
        {
            map = OcadSampleDrawing.ReadTestFile("dashes-7-ocad12-euclidean.ocd");
        }

        [TestCaseSource(nameof(cases))]
        public void DashKeepsOrLeavesOutTheVertexAsOcad(VertexCase cell)
        {
            List<PointF[]> dashes = DashesNearVertex(cell);
            if (cell.offset == 0) {
                Assert.AreEqual(1, dashes.Count, "dashes near the vertex");
                Assert.AreEqual(cell.isOuterKept, dashes[0].Length >= 3, "the vertex in the dash");
            }
            else {
                Assert.AreEqual(2, dashes.Count, "dashes of the borders near the vertex");
                PointF along = cell.IsStart ? Direction(cell.first, cell.vertex) : Direction(cell.vertex, cell.last);
                PointF outerSide = new PointF(-along.Y, along.X);   // The line turns right, so its left side is outer.
                PointF[] outer = dashes.Single(points => Dot(Sub(NearEnd(cell, points), cell.vertex), outerSide) > 0);
                PointF[] inner = dashes.Single(points => Dot(Sub(NearEnd(cell, points), cell.vertex), outerSide) < 0);
                Assert.AreEqual(cell.isOuterKept, outer.Length >= 3, "the corner in the dash of the outer border");
                Assert.AreEqual(cell.isInnerKept, inner.Length >= 3, "the corner in the dash of the inner border");
            }
        }

        // The dashes of the line of a cell whose boundary is near the second vertex: the polylines of the width of the
        // line, longer than 1 mm, whose start (for a dash that starts before the vertex) or end is within reach of it. The
        // palette colors are drawn with the old drawing code (RenderOptions.drawColorsDirectly), whose calls the recorder
        // keeps.
        static List<PointF[]> DashesNearVertex(VertexCase cell)
        {
            RectangleF area = new RectangleF(cell.vertex.X - 3, cell.vertex.Y - 3, 6, 6);
            PolylineRecorder recorder = new PolylineRecorder();
            using (map.Read())
                map.Draw(recorder, area, new RenderOptions() { minResolution = 0.01F, renderTemplates = RenderTemplateOption.MapOnly, drawColorsDirectly = true }, null);
            recorder.EndRecording();
            List<PointF[]> dashes = recorder.polylines
                .Where(polyline => Math.Abs(polyline.Key - cell.width) < 0.001F && Length(polyline.Value) > 1.0F)
                .Select(polyline => polyline.Value)
                .Where(points => Distance(NearEnd(cell, points), cell.vertex) <= cell.piece + cell.offset + 0.3F)
                .ToList();
            recorder.Dispose();
            return dashes;
        }

        // The end of a dash that is near the vertex of a cell: its start for a dash that starts before the vertex.
        static PointF NearEnd(VertexCase cell, PointF[] points) => cell.IsStart ? points[0] : points[points.Length - 1];

        static PointF Sub(PointF a, PointF b) => new PointF(a.X - b.X, a.Y - b.Y);

        static float Dot(PointF a, PointF b) => a.X * b.X + a.Y * b.Y;

        static float Distance(PointF a, PointF b) => (float)Math.Sqrt(Dot(Sub(a, b), Sub(a, b)));

        static PointF Direction(PointF from, PointF to)
        {
            float length = Distance(from, to);
            return new PointF((to.X - from.X) / length, (to.Y - from.Y) / length);
        }

        static float Length(PointF[] points)
        {
            float length = 0;
            for (int i = 0; i < points.Length - 1; ++i)
                length += Distance(points[i], points[i + 1]);
            return length;
        }

        // A drawing target that keeps the polylines drawn, with the widths of their pens, for the tests to read.
        class PolylineRecorder : RecordingGraphicsTarget, IGraphicsTarget
        {
            readonly Dictionary<object, float> penWidths = new Dictionary<object, float>();
            public readonly List<KeyValuePair<float, PointF[]>> polylines = new List<KeyValuePair<float, PointF[]>>();

            public PolylineRecorder() : base(new SkiaBitmapGraphicsLoader())
            {
            }

            void IGraphicsTarget.CreatePen(object key, CmykColor color, float width, LineCapMode caps, LineJoinMode join, float miterLimit)
            {
                penWidths[key] = width;
                CreatePen(key, color, width, caps, join, miterLimit);
            }

            void IGraphicsTarget.CreatePen(object key, object brushKey, float width, LineCapMode caps, LineJoinMode join, float miterLimit)
            {
                penWidths[key] = width;
                CreatePen(key, brushKey, width, caps, join, miterLimit);
            }

            void IGraphicsTarget.DrawPolyline(object penKey, PointF[] pts)
            {
                polylines.Add(new KeyValuePair<float, PointF[]>(penWidths.TryGetValue(penKey, out float width) ? width : 0, (PointF[])pts.Clone()));
                DrawPolyline(penKey, pts);
            }
        }
    }
}
