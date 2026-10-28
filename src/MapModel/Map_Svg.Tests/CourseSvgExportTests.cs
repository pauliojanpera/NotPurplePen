using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using PurplePen;
using PurplePen.MapModel;
using SkiaSharp;

namespace Map_Svg.Tests
{
    // Exports events through the application's SVG export, as its command does, on whatever platform the tests run on
    // (A-1 to A-6, V-4): the controller loads an event, CourseSvg writes a file for each page, and the files come out
    // the same each time and draw in Chromium. The culture is fixed, as the baseline tests fix it.
    [TestFixture, NonParallelizable, SetCulture("en-US"), SetUICulture("en-US")]
    public class CourseSvgExportTests
    {
        [Test]
        public async Task AnEventIsWrittenAsAFileForEachPageTheSameEachTime()
        {
            string first = await CourseExport.ExportAsync("courseprinting/Lord Hill Feb 2024 - Final.ppen", "first", MapType.OCAD, new Id<Course>(2), new Id<Course>(3));
            string second = await CourseExport.ExportAsync("courseprinting/Lord Hill Feb 2024 - Final.ppen", "second", MapType.OCAD, new Id<Course>(2), new Id<Course>(3));

            string[] files = CourseExport.SvgFilesIn(first);
            Assert.GreaterOrEqual(files.Length, 2, "a file for each course");
            CollectionAssert.AreEqual(files, CourseExport.SvgFilesIn(second));
            foreach (string file in files)
                CollectionAssert.AreEqual(File.ReadAllBytes(Path.Combine(first, file)), File.ReadAllBytes(Path.Combine(second, file)), "{0} the same each time", file);

            // A page draws in Chromium: its map, and its course in purple. The image has the
            // page's proportions, at 4 pixels to the millimetre.
            string document = File.ReadAllText(Path.Combine(first, files[0]));
            StringAssert.Contains("<npp:map n=\"1\" label=\"LordHill_ver16_2024Jan_scaled.omap\">", document);

            // The course map's record describes the course, its controls in order, and what each of its objects draws,
            // by the number its elements carry (CD-1, CD-2).
            StringAssert.IsMatch("<npp:course name=\"[^\"]+\" scale=\"7500\">\n<npp:control n=\"1\" kind=\"start\" x=\"-?[0-9]+\" y=\"-?[0-9]+\"/>\n<npp:control n=\"2\" code=\"[0-9]+\" kind=\"control\"", document);
            StringAssert.IsMatch("<npp:object n=\"[0-9]+\" kind=\"leg\" controls=\"1 2\"/>", document);
            StringAssert.IsMatch("<npp:object n=\"[0-9]+\" kind=\"control\" controls=\"2\"/>", document);
            StringAssert.IsMatch("<npp:object n=\"[0-9]+\" kind=\"control-number\" controls=\"2\"/>", document);
            StringAssert.IsMatch("<npp:object n=\"[0-9]+\" kind=\"description\"/>", document);
            string svgPath = LoopResults.SaveSvg("course-export-" + Path.GetFileNameWithoutExtension(files[0]), document);
            System.Text.RegularExpressions.Match size = System.Text.RegularExpressions.Regex.Match(document, "width=\"([0-9.]+)mm\" height=\"([0-9.]+)mm\"");
            Assert.IsTrue(size.Success, "the page's size");
            Size pixels = new Size((int)Math.Round(4 * double.Parse(size.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture)),
                                   (int)Math.Round(4 * double.Parse(size.Groups[2].Value, System.Globalization.CultureInfo.InvariantCulture)));
            using (SKBitmap rendering = await ChromiumRenderer.RenderAsync(svgPath, pixels)) {
                LoopResults.SaveImage(Path.Combine("course-export", Path.GetFileNameWithoutExtension(files[0]) + ".png"), rendering);
                int inked = 0, magenta = 0;
                for (int y = 0; y < rendering.Height; ++y) {
                    for (int x = 0; x < rendering.Width; ++x) {
                        SKColor pixel = rendering.GetPixel(x, y);
                        if (pixel.Red < 200 || pixel.Green < 200 || pixel.Blue < 200)
                            ++inked;
                        if (pixel.Red - pixel.Green > 80 && pixel.Blue - pixel.Green > 40)
                            ++magenta;
                    }
                }
                Assert.Greater(inked, 100000, "the map is drawn");
                Assert.Greater(magenta, 2000, "the course is drawn in purple");
            }
        }
    }
}
