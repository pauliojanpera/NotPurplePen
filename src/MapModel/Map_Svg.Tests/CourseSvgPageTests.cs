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
    // Draws pages of the SVG export of the reference events in Chromium and compares each with the page as the
    // application draws it with the Skia target, within the pixel tolerance of the rendering comparison (F-1): each
    // exported page draws as the application draws it. A page is drawn whole, its paper filling the image, at a
    // resolution at which every page of the reference events, on Letter or Tabloid paper, is a whole number of pixels
    // across and down. Each page's SVG goes to the loop's results under svg/ and its measurements under measurements/,
    // named page-<name>, as the comparison names its cases; the page as the application draws it and as Chromium draws
    // it go under course-pages/, as <name>-skia.png and <name>-chromium.png. The culture is fixed, as the baseline
    // tests fix it.
    [TestFixture, NonParallelizable, SetCulture("en-US"), SetUICulture("en-US")]
    public class CourseSvgPageTests
    {
        // The resolution pages are drawn at, in pixels to the inch. At 100, the smallest text of the reference pages,
        // the 2.3 mm Arial of printarea2's legend, which the tests draw in Liberation Sans, has stems 0.8 pixels wide,
        // and its i and l stand 2 pixels apart. A stem across two columns of pixels shades both alike, Chromium at 153
        // to 157 and Skia at 183 to 186, as Chromium shades glyphs darker (T-10), so two such stems side by side, as in
        // "il" and "ll", make a band four pixels wide of one shade, with no edge within the tolerance's reach, which the
        // tolerance holds as the inside of a shape: 34 pixels lie outside it, by up to 26 levels. At 150 and at 200,
        // each of the 20 pages of the seven reference events draws within it.
        private const float PixelsPerInch = 150;

        // The pages compared, each with its name, its event's file, the kind of map the event is drawn on, the courses
        // exported, whether the course is drawn in upper and lower purple, and the file the page is written in; and
        // what it covers.
        internal static readonly TestCaseData[] Pages = {
            // An OpenMapper map.
            Page("printarea2-Course1", "modes/printarea2.ppen", MapType.OCAD, new int[] { 1 }, false, "Course 1.svg"),
            // An OCAD map with area patterns, multi-colored point symbols and text along lines.
            Page("MadronaPermanent11-Course1", "controller/Madrona Permanent11.ppen", MapType.OCAD, new int[] { 1 }, false, "Course 1.svg"),
            // A bitmap map.
            Page("LincolnPark-Short", "controller/Lincoln Park.ppen", MapType.Bitmap, new int[] { 1 }, false, "Short.svg"),
            // The map in two parts, with the course's lower layer between them.
            Page("OverprintTestLowerPurple-Course12", "courseprinting/Overprint test.ppen", MapType.OCAD, new int[] { 1 }, true, "Course 12.svg"),
            // A part of a course with map exchanges, with no map.
            Page("mapexchange1-Course5-2", "controller/mapexchange1.ppen", MapType.None, new int[] { 6 }, false, "Course 5-2.svg"),
            // A georeferenced map, its page of all controls.
            Page("testproj2-AllControls", "gpx/testproj2.ppen", MapType.OCAD, new int[] { 0 }, false, "All controls.svg"),
        };

        // A page to compare, as a test case named by its name.
        // name: the name.
        // eventFile: the event's file, among the application's test files.
        // mapType: the kind of map the event is drawn on.
        // courses: the ids of the courses to export, 0 for the page of all controls.
        // lowerPurpleLayer: whether to draw the course in upper and lower purple.
        // file: the name of the file the page is written in.
        private static TestCaseData Page(string name, string eventFile, MapType mapType, int[] courses, bool lowerPurpleLayer, string file)
        {
            return new TestCaseData(name, eventFile, mapType, courses, lowerPurpleLayer, file).SetName(name);
        }

        // Export a page of an event, render its file in Chromium, and compare the image with the page as the
        // application draws it with the Skia target.
        // name: the name of the page.
        // eventFile: the event's file, among the application's test files.
        // mapType: the kind of map the event is drawn on.
        // courses: the ids of the courses to export, 0 for the page of all controls.
        // lowerPurpleLayer: whether to draw the course in upper and lower purple.
        // file: the name of the file the page is written in.
        [TestCaseSource(nameof(Pages))]
        public async Task AnExportedPageDrawsAsTheApplicationDrawsIt(string name, string eventFile, MapType mapType, int[] courses, bool lowerPurpleLayer, string file)
        {
            CourseExport.LoadedEvent loadedEvent = await CourseExport.LoadAsync(eventFile, mapType, lowerPurpleLayer);
            CourseSvgSettings settings = CourseExport.Export(loadedEvent, "pages/" + name, courses.Select(id => new Id<Course>(id)).ToArray());
            ExportedPage page = ExportedPage.LayOut(loadedEvent, settings).Single(candidate => Path.GetFileName(candidate.FileName) == file);

            string caseName = "page-" + name;
            string document = File.ReadAllText(page.FileName);
            string svgPath = LoopResults.SaveSvg(caseName, document);
            Size pixelSize = page.PixelSizeAt(PixelsPerInch);
            using (SKBitmap reference = page.RenderReference(loadedEvent, settings, pixelSize))
            using (SKBitmap rendering = await ChromiumRenderer.RenderAsync(svgPath, pixelSize)) {
                LoopResults.SaveImage(Path.Combine("course-pages", name + "-skia.png"), reference);
                LoopResults.SaveImage(Path.Combine("course-pages", name + "-chromium.png"), rendering);
                string difference = ImageComparison.DescribeDifference(caseName, reference, rendering, DrawsText(document) ? PixelTolerance.Text : PixelTolerance.Default);
                if (difference != null)
                    Assert.Fail(difference);
            }
        }

        // Whether a document draws text, and is compared within the tolerance for text, as Chromium shades glyphs
        // otherwise than Skia does: every page with a control number does.
        // document: the text of the document.
        private static bool DrawsText(string document)
        {
            return document.Contains("<text", System.StringComparison.Ordinal);
        }
    }
}
