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

            ReportDefaults(directory, size, mapArea);

            // PixelOffsetMode is left alone throughout: the bitmap target sets it
            // to HighQuality, and overriding it swamps everything else.
            foreach (TextRenderingHint hint in new TextRenderingHint[] {
                         TextRenderingHint.SystemDefault, TextRenderingHint.SingleBitPerPixel,
                         TextRenderingHint.SingleBitPerPixelGridFit, TextRenderingHint.AntiAliasGridFit,
                         TextRenderingHint.AntiAlias, TextRenderingHint.ClearTypeGridFit }) {
                Render(map, size, mapArea, directory, hint, -1);
            }

            foreach (int contrast in new int[] { 0, 1, 2, 3, 4, 5, 6, 8, 10, 12 }) {
                Render(map, size, mapArea, directory, TextRenderingHint.AntiAlias, contrast);
            }
        }

        // Writes down what GDI+ hands us before anything is changed. The initial
        // text contrast is taken from the machine, and is the first thing to
        // compare between two machines that render differently.
        private static void ReportDefaults(string directory, Size size, RectangleF mapArea)
        {
            using (GDIPlus_BitmapGraphicsTarget grTarget = new GDIPlus_BitmapGraphicsTarget(
                       size.Width, size.Height, true, CmykColor.FromCmyk(0, 0, 0, 0), mapArea, true, null, 1.0F)) {
                string report = string.Format(
                    "TextContrast={0}\r\nPixelOffsetMode={1}\r\nTextRenderingHint={2}\r\nSmoothingMode={3}\r\nCompositingQuality={4}\r\nDpiX={5}\r\nDpiY={6}\r\n",
                    grTarget.Graphics.TextContrast, grTarget.Graphics.PixelOffsetMode,
                    grTarget.Graphics.TextRenderingHint, grTarget.Graphics.SmoothingMode,
                    grTarget.Graphics.CompositingQuality, grTarget.Graphics.DpiX, grTarget.Graphics.DpiY);
                report += ReportFamilies();
                File.WriteAllText(Path.Combine(directory, "sweep_defaults_actual.txt"), report);
            }
        }

        // Names each family the test maps ask for and the metrics GDI+ reports
        // for it. The em height, ascent and descent identify which file is
        // actually installed, which a family name alone does not.
        private static string ReportFamilies()
        {
            string[] names = {
                "Arial", "Arial Black", "Arial Narrow", "Switzerland", "SwitzerlandBlack",
                "Forte", "Tekton", "Goudy Stout", "Franklin Gothic Book", "Verdana",
                "Calibri", "Segoe UI Black", "Microsoft Sans Serif"
            };

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append("\r\nperhe;asennettu;emHeight;cellAscent;cellDescent;lineSpacing\r\n");
            foreach (string name in names) {
                try {
                    using (FontFamily family = new FontFamily(name)) {
                        FontStyle style = family.IsStyleAvailable(FontStyle.Regular) ? FontStyle.Regular : FontStyle.Bold;
                        sb.AppendFormat("{0};kyllä;{1};{2};{3};{4}\r\n", name,
                            family.GetEmHeight(style), family.GetCellAscent(style),
                            family.GetCellDescent(style), family.GetLineSpacing(style));
                    }
                }
                catch (Exception e) {
                    sb.AppendFormat("{0};ei;{1}\r\n", name, e.GetType().Name);
                }
            }
            return sb.ToString();
        }

        // Renders the map once with the given settings and saves the result.
        //
        // Parameters:
        //   contrast - the text contrast to set, or -1 to leave it as it comes.
        private static void Render(Map map, Size size, RectangleF mapArea, string directory,
                                   TextRenderingHint hint, int contrast)
        {
            RenderOptions renderOpts = new RenderOptions();
            renderOpts.usePatternBitmaps = true;
            renderOpts.blendOverprintedColors = false;
            renderOpts.renderTemplates = RenderTemplateOption.MapAndTemplates;
            renderOpts.minResolution = mapArea.Width / (float) size.Width;

            string label = string.Format("hint{0}_contrast{1}", (int) hint, contrast);

            using (GDIPlus_BitmapGraphicsTarget grTarget = new GDIPlus_BitmapGraphicsTarget(
                       size.Width, size.Height, true, CmykColor.FromCmyk(0, 0, 0, 0), mapArea, true, null, 1.0F)) {
                grTarget.Graphics.TextRenderingHint = hint;
                if (contrast >= 0)
                    grTarget.Graphics.TextContrast = contrast;

                using (map.Read())
                    map.Draw(grTarget, mapArea, renderOpts, null);

                string target = Path.Combine(directory, "sweep_" + label + "_actual.png");
                grTarget.Bitmap.Save(target, ImageFormat.Png);
                TestContext.WriteLine("SWEEP wrote " + Path.GetFileName(target));
            }
        }
    }
}
