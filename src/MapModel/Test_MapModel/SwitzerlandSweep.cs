using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;

using NUnit.Framework;
using PurplePen.Graphics2D;
using TestingUtils;

namespace PurplePen.MapModel.Tests
{
    // Draws marymoor once for every font the runner has, each one renamed to the
    // family the map asks for, and records how far each result lands from the
    // committed baseline. Comparing font files to each other cannot settle which
    // one the baseline was drawn with; putting each through the same drawing path
    // and measuring the picture can. Asserts nothing and always passes.
    [TestFixture]
    public class SwitzerlandSweep
    {
        [Test]
        public void SweepEveryFont()
        {
            string descriptor = TestUtil.GetTestFile("rendering\\marymoor.txt");
            string directory = Path.GetDirectoryName(descriptor);
            string fontDir = FindDirectory(directory, "font-candidates");
            string report = Path.Combine(directory, "sweep_fonts_actual.txt");

            if (fontDir == null) {
                File.WriteAllText(report, "font-candidates not found\r\n");
                return;
            }

            string mapFileName;
            RectangleF mapArea;
            Size size;
            using (StreamReader reader = new StreamReader(descriptor)) {
                mapFileName = Path.Combine(directory, reader.ReadLine());
                reader.ReadLine();
                string[] coords = reader.ReadLine().Split(',');
                float left = float.Parse(coords[0]), bottom = float.Parse(coords[1]);
                float right = float.Parse(coords[2]), top = float.Parse(coords[3]);
                mapArea = new RectangleF(left, top, right - left, bottom - top);
                string[] sizeParts = reader.ReadLine().Split(',');
                size = new Size(int.Parse(sizeParts[0]), int.Parse(sizeParts[1]));
            }

            string baselinePath = TestUtil.GetSpecificFileName(Path.Combine(directory, "marymoor.png"));
            List<string> lines = new List<string>();

            using (Bitmap baseline = (Bitmap) Image.FromFile(baselinePath)) {
                foreach (string candidate in Directory.GetFiles(fontDir, "*.ttf")) {
                    string label = Path.GetFileNameWithoutExtension(candidate);
                    try {
                        ForgetPrivateFonts();
                        GdiplusFontLoader.Instance.AddFontFile("Switzerland", TextEffects.Regular, candidate);

                        Map map = new Map(new GDIPlus_TextMetrics(), new GDIPlus_FileLoader(directory));
                        InputOutput.ReadFile(mapFileName, map);

                        RenderOptions renderOpts = new RenderOptions();
                        renderOpts.usePatternBitmaps = true;
                        renderOpts.blendOverprintedColors = false;
                        renderOpts.renderTemplates = RenderTemplateOption.MapAndTemplates;
                        renderOpts.minResolution = mapArea.Width / (float) size.Width;

                        using (GDIPlus_BitmapGraphicsTarget grTarget = new GDIPlus_BitmapGraphicsTarget(
                                   size.Width, size.Height, true, CmykColor.FromCmyk(0, 0, 0, 0), mapArea, true, null, 1.0F)) {
                            using (map.Read())
                                map.Draw(grTarget, mapArea, renderOpts, null);
                            lines.Add(string.Format("{0};{1}", CountDifferent(baseline, grTarget.Bitmap, 40), label));
                        }
                    }
                    catch (Exception e) {
                        lines.Add(string.Format("-1;{0};{1}", label, e.GetType().Name));
                    }
                }
            }

            lines.Sort(CompareByCount);
            File.WriteAllLines(report, lines);
        }

        private static int CompareByCount(string a, string b)
        {
            int ca = int.Parse(a.Split(';')[0]), cb = int.Parse(b.Split(';')[0]);
            return ca.CompareTo(cb);
        }

        // Pixels where any channel differs by more than the given amount.
        private static int CountDifferent(Bitmap a, Bitmap b, int tolerance)
        {
            if (a.Width != b.Width || a.Height != b.Height)
                return int.MaxValue;

            int count = 0;
            for (int y = 0; y < a.Height; ++y) {
                for (int x = 0; x < a.Width; ++x) {
                    Color ca = a.GetPixel(x, y), cb = b.GetPixel(x, y);
                    if (Math.Abs(ca.R - cb.R) > tolerance || Math.Abs(ca.G - cb.G) > tolerance ||
                        Math.Abs(ca.B - cb.B) > tolerance)
                        ++count;
                }
            }
            return count;
        }

        private static string FindDirectory(string start, string name)
        {
            DirectoryInfo dir = new DirectoryInfo(start);
            while (dir != null) {
                string candidate = Path.Combine(dir.FullName, name);
                if (Directory.Exists(candidate))
                    return candidate;
                dir = dir.Parent;
            }
            return null;
        }

        private static void ForgetPrivateFonts()
        {
            FieldInfo field = typeof(GdiplusFontLoader).GetField(
                "fontCollections", BindingFlags.Instance | BindingFlags.NonPublic);
            System.Collections.IDictionary collections =
                (System.Collections.IDictionary) field.GetValue(GdiplusFontLoader.Instance);
            collections.Clear();
        }
    }
}
