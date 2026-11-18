using System;
using System.Drawing;
using System.IO;
using PurplePen.Graphics2D;
using PurplePen.MapModel;
using TestingUtils;

namespace Map_Skia.Tests
{
    // Reads the sample files of the dashed lines of OCAD, and counts the dashes or the glyphs that their drawing has along
    // a straight line, for the tests that compare them with the drawing of the OCAD 2020 viewer.
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

        // Count the runs of covered pixels that the drawing of a map has along a straight line, drawn without antialiasing
        // at 0.02 mm to the pixel: the dashes of a dashed line, or the glyphs of a line of glyphs.
        // map: the map.
        // start: one end of the line, in mm on the paper.
        // end: the other end of the line.
        public static int CountRunsAlong(Map map, PointF start, PointF end)
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
                    int runs = 0;
                    bool wasCovered = false;
                    for (float distance = 0; distance <= length; distance += step) {
                        float x = start.X + (end.X - start.X) * distance / length;
                        float y = start.Y + (end.Y - start.Y) * distance / length;
                        bool isCovered = bitmap.Bitmap.GetPixel((int)((x - area.Left) / pixelSize), (int)((area.Bottom - y) / pixelSize)).Alpha != 0;
                        if (isCovered && !wasCovered)
                            ++runs;
                        wasCovered = isCovered;
                    }
                    return runs;
                }
            }
        }
    }
}
