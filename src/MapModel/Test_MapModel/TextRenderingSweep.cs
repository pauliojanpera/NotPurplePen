using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;

using NUnit.Framework;
using PurplePen.Graphics2D;
using TestingUtils;

namespace PurplePen.MapModel.Tests
{
    // Renders one map under a range of GDI+ text settings and leaves each result
    // beside the baseline. This measures which settings, if any, bring the output
    // closer to the committed baseline; it asserts nothing and always passes.
    [TestFixture]
    public class TextRenderingSweep
    {
        [Test]
        public void SweepTextSettings()
        {
            string descriptor = TestUtil.GetTestFile("rendering\\marymoor.txt");
            string directory = Path.GetDirectoryName(descriptor);

            string mapFileName;
            RectangleF mapArea;
            Size size;

            using (StreamReader reader = new StreamReader(descriptor)) {
                mapFileName = Path.Combine(directory, reader.ReadLine());
                reader.ReadLine();          // baseline png, not needed here
                string[] coords = reader.ReadLine().Split(',');
                float left = float.Parse(coords[0]), bottom = float.Parse(coords[1]);
                float right = float.Parse(coords[2]), top = float.Parse(coords[3]);
                mapArea = new RectangleF(left, top, right - left, bottom - top);
                string[] sizeParts = reader.ReadLine().Split(',');
                size = new Size(int.Parse(sizeParts[0]), int.Parse(sizeParts[1]));
            }

            Map map = new Map(new GDIPlus_TextMetrics(), new GDIPlus_FileLoader(directory));
            InputOutput.ReadFile(mapFileName, map);

            ReportDefaults(size, mapArea);

            foreach (TextRenderingHint hint in new TextRenderingHint[] {
                         TextRenderingHint.AntiAlias, TextRenderingHint.AntiAliasGridFit }) {
                foreach (int contrast in new int[] { 0, 2, 4, 6, 8, 10, 12 }) {
                    Render(map, size, mapArea, directory, hint, contrast, PixelOffsetMode.Default);
                }
            }

            foreach (PixelOffsetMode offset in new PixelOffsetMode[] {
                         PixelOffsetMode.None, PixelOffsetMode.Half, PixelOffsetMode.HighQuality }) {
                Render(map, size, mapArea, directory, TextRenderingHint.AntiAlias, -1, offset);
            }
        }

        // Writes down what GDI+ hands us before anything is changed. The initial
        // text contrast is taken from the machine, and is the first thing to
        // compare between two machines that render differently.
        private static void ReportDefaults(Size size, RectangleF mapArea)
        {
            using (GDIPlus_BitmapGraphicsTarget grTarget = new GDIPlus_BitmapGraphicsTarget(
                       size.Width, size.Height, true, CmykColor.FromCmyk(0, 0, 0, 0), mapArea, true, null, 1.0F)) {
                TestContext.WriteLine(string.Format(
                    "SWEEP defaults: TextContrast={0} PixelOffsetMode={1} TextRenderingHint={2} SmoothingMode={3} CompositingQuality={4}",
                    grTarget.Graphics.TextContrast, grTarget.Graphics.PixelOffsetMode,
                    grTarget.Graphics.TextRenderingHint, grTarget.Graphics.SmoothingMode,
                    grTarget.Graphics.CompositingQuality));
            }
        }

        // Renders the map once with the given settings and saves the result.
        //
        // Parameters:
        //   contrast - the text contrast to set, or -1 to leave it alone.
        private static void Render(Map map, Size size, RectangleF mapArea, string directory,
                                   TextRenderingHint hint, int contrast, PixelOffsetMode offset)
        {
            RenderOptions renderOpts = new RenderOptions();
            renderOpts.usePatternBitmaps = true;
            renderOpts.blendOverprintedColors = false;
            renderOpts.renderTemplates = RenderTemplateOption.MapAndTemplates;
            renderOpts.minResolution = mapArea.Width / (float) size.Width;

            string label = string.Format("hint{0}_contrast{1}_offset{2}", (int) hint, contrast, (int) offset);

            using (GDIPlus_BitmapGraphicsTarget grTarget = new GDIPlus_BitmapGraphicsTarget(
                       size.Width, size.Height, true, CmykColor.FromCmyk(0, 0, 0, 0), mapArea, true, null, 1.0F)) {
                grTarget.Graphics.TextRenderingHint = hint;
                if (contrast >= 0)
                    grTarget.Graphics.TextContrast = contrast;
                grTarget.Graphics.PixelOffsetMode = offset;

                using (map.Read())
                    map.Draw(grTarget, mapArea, renderOpts, null);

                string target = Path.Combine(directory, "sweep_" + label + "_actual.png");
                grTarget.Bitmap.Save(target, ImageFormat.Png);
                TestContext.WriteLine("SWEEP wrote " + Path.GetFileName(target));
            }
        }
    }
}
