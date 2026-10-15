using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;

using NUnit.Framework;
using PurplePen.Graphics2D;
using TestingUtils;

namespace PurplePen.MapModel.Tests
{
    // Renders marymoor once for each Switzerland file in the font store's
    // candidates folder, loading it as a private font so the installed copy is
    // not involved. Comparing each result against the committed baseline says
    // which file the baseline was made with, which no amount of comparing the
    // files to each other can settle. Asserts nothing and always passes.
    [TestFixture]
    public class SwitzerlandCandidates
    {
        [Test]
        public void RenderEachCandidate()
        {
            string descriptor = TestUtil.GetTestFile("rendering\\marymoor.txt");
            string directory = Path.GetDirectoryName(descriptor);

            string candidateDir = FindCandidateDirectory(directory);
            if (candidateDir == null) {
                File.WriteAllText(Path.Combine(directory, "switz_report_actual.txt"),
                                  "ci-fonts/candidates not found\r\n");
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

            System.Text.StringBuilder report = new System.Text.StringBuilder();
            report.Append("candidates: " + candidateDir + "\r\n");

            foreach (string candidate in Directory.GetFiles(candidateDir, "*.ttf")) {
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
                        grTarget.Bitmap.Save(Path.Combine(directory, "switz_" + label + "_actual.png"), ImageFormat.Png);
                    }

                    using (FontFamily family = new FontFamily("Switzerland")) {
                        report.AppendFormat("{0};em={1};asc={2};desc={3}\r\n", label,
                            family.GetEmHeight(FontStyle.Regular), family.GetCellAscent(FontStyle.Regular),
                            family.GetCellDescent(FontStyle.Regular));
                    }
                }
                catch (Exception e) {
                    report.AppendFormat("{0};VIRHE;{1};{2}\r\n", label, e.GetType().Name, e.Message);
                }
            }

            File.WriteAllText(Path.Combine(directory, "switz_report_actual.txt"), report.ToString());
        }

        // Walks up from the test files to the workspace, where the workflow puts
        // the font store.
        private static string FindCandidateDirectory(string start)
        {
            DirectoryInfo dir = new DirectoryInfo(start);
            while (dir != null) {
                string candidates = Path.Combine(dir.FullName, "ci-fonts", "candidates");
                if (Directory.Exists(candidates))
                    return candidates;
                dir = dir.Parent;
            }
            return null;
        }

        // The loader keeps one private collection per family and style and never
        // replaces it, so each candidate needs the previous one cleared away.
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
