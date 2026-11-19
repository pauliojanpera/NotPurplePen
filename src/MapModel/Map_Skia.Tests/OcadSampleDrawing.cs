using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using PurplePen.Graphics2D;
using PurplePen.MapModel;
using TestingUtils;

namespace Map_Skia.Tests
{
    // Reads the sample files of the dashed lines of OCAD, and counts the dashes or the glyphs that their drawing has along
    // a straight line, or keeps the polylines and the filled ellipses that it draws, for the tests that compare them with
    // the drawing of the OCAD 2020 viewer.
    static class OcadSampleDrawing
    {
        // Read a file of TestFiles/ocadimport into a new map.
        // fileName: the name of the file.
        public static Map ReadTestFile(string fileName)
        {
            string path = TestUtil.GetTestFile(Path.Combine("ocadimport", fileName));
            Map map = new Map(new Skia_TextMetrics(), new Skia_FileLoader(Path.GetDirectoryName(path)));
            InputOutput.ReadFile(path, map);
            return map;
        }

        // The polylines that the drawing of an area of a map draws with pens of a width, in the order of the drawing: the
        // dashes of the dashed lines of that width, in mm on the paper.
        // map: the map.
        // area: the area, in mm on the paper.
        // penWidth: the width of the pens, in mm.
        public static List<PointF[]> DrawPolylines(Map map, RectangleF area, float penWidth)
        {
            using (ShapeRecorder recorder = DrawArea(map, area))
                return recorder.polylines.Where(polyline => Math.Abs(polyline.Key - penWidth) < 0.001F).Select(polyline => polyline.Value).ToList();
        }

        // The centers of the filled ellipses that the drawing of an area of a map draws, in the order of the drawing: the
        // glyphs of one filled circle along the lines, in mm on the paper.
        // map: the map.
        // area: the area, in mm on the paper.
        public static List<PointF> DrawFilledEllipseCenters(Map map, RectangleF area)
        {
            using (ShapeRecorder recorder = DrawArea(map, area))
                return new List<PointF>(recorder.filledEllipseCenters);
        }

        // Draw an area of a map on a drawing target that keeps the polylines and the filled ellipses.
        static ShapeRecorder DrawArea(Map map, RectangleF area)
        {
            ShapeRecorder recorder = new ShapeRecorder();
            using (map.Read())
                map.Draw(recorder, area, new RenderOptions() { minResolution = 0.01F, renderTemplates = RenderTemplateOption.MapOnly }, null);
            recorder.EndRecording();
            return recorder;
        }

        // A drawing target that keeps the polylines drawn, with the widths of their pens, and the centers of the filled
        // ellipses.
        class ShapeRecorder : RecordingGraphicsTarget, IGraphicsTarget
        {
            readonly Dictionary<object, float> penWidths = new Dictionary<object, float>();
            public readonly List<KeyValuePair<float, PointF[]>> polylines = new List<KeyValuePair<float, PointF[]>>();
            public readonly List<PointF> filledEllipseCenters = new List<PointF>();

            public ShapeRecorder() : base(new SkiaBitmapGraphicsLoader())
            {
            }

            void IGraphicsTarget.FillEllipse(object brushKey, PointF center, float radiusX, float radiusY)
            {
                filledEllipseCenters.Add(center);
                FillEllipse(brushKey, center, radiusX, radiusY);
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

        // Count the runs of covered pixels that the drawing of a map has along a line through points, drawn without
        // antialiasing at 0.02 mm to the pixel: the dashes of a dashed line, or the glyphs of a line of glyphs.
        // map: the map.
        // points: the points of the line, in mm on the paper.
        public static int CountRunsAlong(Map map, params PointF[] points)
        {
            const float pixelSize = 0.02F, step = 0.01F;
            float left = points.Min(p => p.X) - 1, bottom = points.Min(p => p.Y) - 1, right = points.Max(p => p.X) + 1, top = points.Max(p => p.Y) + 1;
            RectangleF area = RectangleF.FromLTRB(left, bottom, right, top);
            int width = (int)Math.Ceiling(area.Width / pixelSize), height = (int)Math.Ceiling(area.Height / pixelSize);

            using (map.Read())
            using (Skia_BitmapGraphicsTarget target = new Skia_BitmapGraphicsTarget(width, height, true, CmykColor.FromCmyka(0, 0, 0, 0, 0), area, true)) {
                target.PushAntiAliasing(false);
                map.Draw(target, area, new RenderOptions() { minResolution = pixelSize, renderTemplates = RenderTemplateOption.MapOnly }, null);
                target.PopAntiAliasing();
                using (Skia_Bitmap bitmap = (Skia_Bitmap)target.FinishBitmap()) {
                    int runs = 0;
                    bool wasCovered = false;
                    for (int i = 0; i < points.Length - 1; ++i) {
                        PointF start = points[i], end = points[i + 1];
                        float length = (float)Math.Sqrt((end.X - start.X) * (end.X - start.X) + (end.Y - start.Y) * (end.Y - start.Y));
                        for (float distance = 0; distance < length; distance += step) {
                            float x = start.X + (end.X - start.X) * distance / length;
                            float y = start.Y + (end.Y - start.Y) * distance / length;
                            bool isCovered = bitmap.Bitmap.GetPixel((int)((x - area.Left) / pixelSize), (int)((area.Bottom - y) / pixelSize)).Alpha != 0;
                            if (isCovered && !wasCovered)
                                ++runs;
                            wasCovered = isCovered;
                        }
                    }
                    return runs;
                }
            }
        }
    }
}
