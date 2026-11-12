using System;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text;
using SkiaSharp;

namespace PdfVectorsProbe
{
    // The probe's commands. Each writes its file and prints one line of JSON with what it measured.
    //   reference <pdf> <pixels per inch> <out.png>   the page as the application draws it (F-1's reference)
    //   bitmap-svg <pdf> <out.svg>                     the page as the SVG export writes it today: PDFium's bitmap
    //   vector-svg <pdf> <out.svg> [options]           the page as vectors, read by PdfPig into the SVG target's calls
    //       --overprint-darken   draw what overprints with darken blending, as COL-7 shows a map's overprinting
    //       --glyphs-as-text     (not built) see the design note
    //   pdfium-objects <pdf>                           what PDFium's page-object API reports of the page
    internal static class Program
    {
        // Run a command.
        // args: the command and its arguments.
        private static int Main(string[] args)
        {
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
            if (args.Length < 2) {
                Console.Error.WriteLine("usage: reference <pdf> <ppi> <out.png> | bitmap-svg <pdf> <out.svg> | vector-svg <pdf> <out.svg> [--overprint-darken] | pdfium-objects <pdf>");
                return 2;
            }
            switch (args[0]) {
            case "reference":
                return Reference(args[1], float.Parse(args[2], CultureInfo.InvariantCulture), args[3]);
            case "bitmap-svg":
                return BitmapSvg(args[1], args[2]);
            case "vector-svg":
                return VectorSvg(args[1], args[2], Array.IndexOf(args, "--overprint-darken") >= 0);
            case "pdfium-objects":
                return PdfiumObjects.Report(args[1]);
            default:
                Console.Error.WriteLine("unknown command " + args[0]);
                return 2;
            }
        }

        // Draw the page as the application draws it, at a resolution, into a PNG.
        // pdf: the PDF file.
        // pixelsPerInch: the image's resolution.
        // output: the PNG file to write.
        private static int Reference(string pdf, float pixelsPerInch, string output)
        {
            using (ApplicationDrawing drawing = new ApplicationDrawing(pdf)) {
                Stopwatch watch = Stopwatch.StartNew();
                using (SKBitmap reference = drawing.RenderReference(pixelsPerInch))
                using (FileStream stream = File.Create(output)) {
                    reference.Encode(stream, SKEncodedImageFormat.Png, 100);
                    watch.Stop();
                    Console.WriteLine("{\"command\":\"reference\",\"pdf\":" + Json(pdf) + ",\"width\":" + reference.Width +
                                      ",\"height\":" + reference.Height + ",\"seconds\":" + Seconds(watch) + "}");
                }
            }
            return 0;
        }

        // Write the page as the SVG export writes it today, and time the conversion and the writing apart.
        // pdf: the PDF file.
        // output: the SVG file to write.
        private static int BitmapSvg(string pdf, string output)
        {
            Tuple<double, long> conversion = ApplicationDrawing.TimeConversion(pdf);
            using (ApplicationDrawing drawing = new ApplicationDrawing(pdf)) {
                Stopwatch watch = Stopwatch.StartNew();
                string svg = drawing.WriteBitmapSvg();
                File.WriteAllText(output, svg, new UTF8Encoding(false));
                watch.Stop();
                Console.WriteLine("{\"command\":\"bitmap-svg\",\"pdf\":" + Json(pdf) + ",\"conversion_seconds\":" +
                                  conversion.Item1.ToString("0.000", CultureInfo.InvariantCulture) + ",\"png_bytes\":" + conversion.Item2 +
                                  ",\"write_seconds\":" + Seconds(watch) + ",\"bytes\":" + new FileInfo(output).Length +
                                  ",\"map_bounds\":" + Json(drawing.MapBounds) + "}");
            }
            return 0;
        }

        // Write the page as vectors, read by PdfPig into the SVG target's drawing calls.
        // pdf: the PDF file.
        // output: the SVG file to write.
        // overprintDarken: whether what overprints is drawn with darken blending.
        private static int VectorSvg(string pdf, string output, bool overprintDarken)
        {
            using (ApplicationDrawing drawing = new ApplicationDrawing(pdf)) {
                Stopwatch watch = Stopwatch.StartNew();
                PdfPigReader.Result result = PdfPigReader.WriteSvg(drawing, overprintDarken);
                File.WriteAllText(output, result.Svg, new UTF8Encoding(false));
                watch.Stop();
                Console.WriteLine("{\"command\":\"vector-svg\",\"pdf\":" + Json(pdf) + ",\"seconds\":" + Seconds(watch) +
                                  ",\"read_seconds\":" + result.ReadSeconds.ToString("0.000", CultureInfo.InvariantCulture) +
                                  ",\"bytes\":" + new FileInfo(output).Length + ",\"counts\":" + result.CountsJson() + "}");
            }
            return 0;
        }

        // A stopwatch's seconds, to the millisecond.
        private static string Seconds(Stopwatch watch)
        {
            return watch.Elapsed.TotalSeconds.ToString("0.000", CultureInfo.InvariantCulture);
        }

        // A string as JSON.
        internal static string Json(string text)
        {
            StringBuilder builder = new StringBuilder("\"");
            foreach (char c in text) {
                if (c == '"' || c == '\\')
                    builder.Append('\\').Append(c);
                else if (c < ' ')
                    builder.Append("\\u").Append(((int)c).ToString("x4", CultureInfo.InvariantCulture));
                else
                    builder.Append(c);
            }
            return builder.Append('"').ToString();
        }

        // A rectangle as JSON: left, bottom, width and height.
        private static string Json(RectangleF rectangle)
        {
            return string.Format(CultureInfo.InvariantCulture, "[{0},{1},{2},{3}]", rectangle.Left, rectangle.Top, rectangle.Width, rectangle.Height);
        }
    }
}
