using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using PurplePen.Graphics2D;
using PurplePen.PdfConverter;
using Color = System.Drawing.Color;
using SixLabors.ImageSharp.PixelFormats;

namespace PdfVectorsProbe
{
    // A color converter that gives each CMYK color as PDFium draws it, as the application's bitmap of a PDF map shows
    // it, instead of through the SWOP table that COL-2 converts with. It learns the colors in two passes: asked for one
    // it has not sampled, it answers through the fallback and remembers it; Sample then draws every remembered color
    // as a patch of a PDF of its own, which PdfConverter's PDFium renders, and reads each patch back. With it, what of a
    // vector drawing lies outside F-1 of the application's drawing is the drawing, not the conversion of its colors.
    internal sealed class PdfiumColors : IColorConverter
    {
        private readonly IColorConverter fallback;
        private readonly Dictionary<string, Color> sampled = new Dictionary<string, Color>(StringComparer.Ordinal);
        private readonly SortedDictionary<string, float[]> asked = new SortedDictionary<string, float[]>(StringComparer.Ordinal);

        // Create the converter, which answers through a fallback until it has sampled a color.
        // fallback: the converter of colors not sampled yet.
        public PdfiumColors(IColorConverter fallback)
        {
            this.fallback = fallback;
        }

        // The colors sampled.
        public int Count
        {
            get { return sampled.Count; }
        }

        // A CMYK color as PDFium draws it, if sampled; else through the fallback, remembered for sampling.
        public Color ToColor(CmykColor cmykColor)
        {
            Color fallbackColor = fallback.ToColor(cmykColor);
            string key = Key(cmykColor.Cyan, cmykColor.Magenta, cmykColor.Yellow, cmykColor.Black);
            Color color;
            if (sampled.TryGetValue(key, out color))
                return Color.FromArgb(fallbackColor.A, color.R, color.G, color.B);
            asked[key] = new float[] { cmykColor.Cyan, cmykColor.Magenta, cmykColor.Yellow, cmykColor.Black };
            return fallbackColor;
        }

        // Sample every color asked for so far: draw each as a 10-point square of DeviceCMYK in a PDF, have PDFium render
        // it at 72 dots per inch, and read each square's middle.
        public void Sample()
        {
            List<KeyValuePair<string, float[]>> colors = asked.Where(pair => !sampled.ContainsKey(pair.Key)).ToList();
            if (colors.Count == 0)
                return;
            const int perRow = 32;
            int rows = (colors.Count + perRow - 1) / perRow;
            StringBuilder content = new StringBuilder();
            for (int index = 0; index < colors.Count; ++index) {
                float[] c = colors[index].Value;
                content.AppendFormat(CultureInfo.InvariantCulture, "{0} {1} {2} {3} k {4} {5} 10 10 re f\n",
                                     c[0], c[1], c[2], c[3], (index % perRow) * 10, (rows - 1 - index / perRow) * 10);
            }
            string folder = Path.Combine(Path.GetTempPath(), "pdf-vectors-colors-" + Environment.ProcessId.ToString(CultureInfo.InvariantCulture));
            Directory.CreateDirectory(folder);
            string pdf = Path.Combine(folder, "patches.pdf"), png = Path.Combine(folder, "patches.png");
            File.WriteAllBytes(pdf, MinimalPdf(perRow * 10, rows * 10, content.ToString()));
            PdfConversion.Convert(72, pdf, png);
            using (SixLabors.ImageSharp.Image<Rgb24> image = SixLabors.ImageSharp.Image.Load<Rgb24>(png)) {
                for (int index = 0; index < colors.Count; ++index) {
                    Rgb24 pixel = image[(index % perRow) * 10 + 5, (index / perRow) * 10 + 5];
                    sampled[colors[index].Key] = Color.FromArgb(pixel.R, pixel.G, pixel.B);
                }
            }
            Directory.Delete(folder, true);
        }

        // The key of a CMYK color.
        private static string Key(float c, float m, float y, float k)
        {
            return string.Join(",", new float[] { c, m, y, k }.Select(v => v.ToString("R", CultureInfo.InvariantCulture)));
        }

        // A PDF of one page of a size in points whose content is the given operators, with its cross-reference table.
        private static byte[] MinimalPdf(int width, int height, string content)
        {
            List<string> objects = new List<string> {
                "<< /Type /Catalog /Pages 2 0 R >>",
                "<< /Type /Pages /Kids [3 0 R] /Count 1 >>",
                string.Format(CultureInfo.InvariantCulture, "<< /Type /Page /Parent 2 0 R /MediaBox [0 0 {0} {1}] /Contents 4 0 R /Resources << >> >>", width, height),
                "<< /Length " + Encoding.ASCII.GetByteCount(content).ToString(CultureInfo.InvariantCulture) + " >>\nstream\n" + content + "endstream",
            };
            StringBuilder pdf = new StringBuilder("%PDF-1.4\n");
            List<int> offsets = new List<int>();
            for (int index = 0; index < objects.Count; ++index) {
                offsets.Add(Encoding.ASCII.GetByteCount(pdf.ToString()));
                pdf.Append((index + 1).ToString(CultureInfo.InvariantCulture)).Append(" 0 obj\n").Append(objects[index]).Append("\nendobj\n");
            }
            int xref = Encoding.ASCII.GetByteCount(pdf.ToString());
            pdf.Append("xref\n0 ").Append((objects.Count + 1).ToString(CultureInfo.InvariantCulture)).Append("\n0000000000 65535 f \n");
            foreach (int offset in offsets)
                pdf.Append(offset.ToString("0000000000", CultureInfo.InvariantCulture)).Append(" 00000 n \n");
            pdf.Append("trailer\n<< /Size ").Append((objects.Count + 1).ToString(CultureInfo.InvariantCulture))
               .Append(" /Root 1 0 R >>\nstartxref\n").Append(xref.ToString(CultureInfo.InvariantCulture)).Append("\n%%EOF\n");
            return Encoding.ASCII.GetBytes(pdf.ToString());
        }
    }
}
