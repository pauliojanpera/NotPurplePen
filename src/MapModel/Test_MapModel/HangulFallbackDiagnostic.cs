using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.Text;

namespace PurplePen.MapModel.Tests
{
    // Temporary. Says which face GDI+ draws Hangul with when the text asks for a
    // family that has none, by measuring what the request costs and comparing it
    // against what each installed family costs on its own.
    [TestFixture]
    public class HangulFallbackDiagnostic
    {
        private const string Syllable = "한";   // 한

        [Test]
        public void WhichFaceDrawsHangul()
        {
            StringBuilder report = new StringBuilder();
            report.AppendLine();

            using (Bitmap bitmap = new Bitmap(16, 16))
            using (Graphics graphics = Graphics.FromImage(bitmap)) {
                graphics.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
                StringFormat format = StringFormat.GenericTypographic;

                foreach (float size in new float[] { 20F, 22F, 23F, 24F }) {
                    float linked = Measure(graphics, format, "Arial", size);
                    float malgun = Measure(graphics, format, "Malgun Gothic", size);
                    report.AppendFormat("size {0,4}: Arial+linking {1,6:F2}   Malgun Gothic {2,6:F2}",
                                        size, linked, malgun);
                    report.AppendLine();
                }

                // Which families cost the same as the request does. Whatever GDI+
                // linked to is among them; a family with no Hangul of its own links
                // the same way and lands there too, so the list is a shortlist.
                float reference = Measure(graphics, format, "Arial", 22F);
                List<string> alike = new List<string>();
                List<string> others = new List<string>();

                foreach (FontFamily family in FontFamily.Families) {
                    float width;
                    try { width = Measure(graphics, format, family.Name, 22F); }
                    catch (Exception) { continue; }

                    string entry = string.Format("{0} ({1:F2})", family.Name, width);
                    if (Math.Abs(width - reference) < 0.01F)
                        alike.Add(family.Name);
                    else
                        others.Add(entry);
                }

                report.AppendLine();
                report.AppendFormat("At 22 px the request costs {0:F2}.", reference);
                report.AppendLine();
                report.AppendFormat("{0} families cost the same (so they link rather than draw it).", alike.Count);
                report.AppendLine();
                report.AppendLine();
                report.AppendLine("Families that cost something else, and so have Hangul of their own:");
                foreach (string entry in others)
                    report.AppendLine("    " + entry);
            }

            Assert.Fail(report.ToString());
        }

        // The width one syllable costs in the given family at the given size.
        //
        // Parameters:
        //   graphics - what to measure with.
        //   format - the string format to measure under.
        //   familyName - the family to ask for.
        //   size - the em size in pixels.
        private static float Measure(Graphics graphics, StringFormat format, string familyName, float size)
        {
            using (Font font = new Font(familyName, size, GraphicsUnit.Pixel))
                return graphics.MeasureString(Syllable, font, PointF.Empty, format).Width;
        }
    }
}
