using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using NUnit.Framework;
using PurplePen;
using PurplePen.Graphics2D;
using PurplePen.MapModel;
using SkiaSharp;
using Map_SkiaStd;

namespace Map_Svg.Tests
{
    // The browser check (V-6): renders documents of the SVG target in Firefox and in WebKit, and compares each image
    // with Chromium's image of the same document within the pixel tolerance of the rendering comparison (F-1), as
    // Firefox and WebKit should display the output as Chromium does (F-5). Its documents are those of the rendering
    // comparison's cases, at the case's size, as the comparison wrote and Chromium rendered them, when it ran first in
    // this run, and otherwise written and rendered anew as it writes and renders them; BrowserPageComparisonTests takes
    // pages of the export.
    //
    // A document that a browser draws otherwise because of the browser itself, where no form the requirements allow
    // draws alike in all three without costing the file data or Chromium's drawing, is set apart for that browser in
    // the table below, with its reason and the number of pixels of the browser's image that lay outside the tolerance
    // when it was set apart. It is still rendered and compared: while it differs by no more pixels, its test ends as
    // ignored, with the reason, which goes to the loop's results as browsers/apart/<browser>/<name>.txt; once it
    // matches, or differs by more, its test fails, so that the table holds only what still differs, and a document set
    // apart is still held to what it was set apart for.
    [TestFixture, Parallelizable(ParallelScope.Children)]
    public class BrowserComparisonTests
    {
        // Why WebKit's text differs: it hints glyphs.
        internal const string WebKitHints =
            "F-5: WebKit hints the glyphs of the faces the document embeds, as its Linux build does whatever fontconfig, text-rendering " +
            "or its own settings say, while Chromium draws them unhinted";

        // Why WebKit's bitmaps differ: it does not smooth them as it scales them down.
        internal const string WebKitScalesDown =
            "F-5: WebKit draws a bitmap scaled down to a fraction of its size without smoothing it, whatever its image-rendering";

        // Why WebKit's fine patterns differ: it shades their tiles otherwise.
        internal const string WebKitFineTiles =
            "F-5: WebKit shades a pattern whose tile is a few pixels across darker than Chromium and with a grain, from the one tile " +
            "the document gives, and otherwise as the pattern's phase falls on the pixels";

        // Why Firefox's fine patterns differ: it shades their tiles lighter.
        internal const string FirefoxFineTiles =
            "F-5: Firefox shades a pattern whose tile is a pixel or two across a few levels lighter than Chromium, from the one tile the " +
            "document gives; each lies within F-1's tolerance of the reference";

        // The documents set apart, each for a browser, with why the browser draws it otherwise than Chromium, and how
        // many pixels of the browser's image lay outside the tolerance when it was set apart.
        internal static readonly Dictionary<(Browser, string), SetApartDocument> SetApart = new Dictionary<(Browser, string), SetApartDocument> {
            { (Browser.Firefox, "teanwest"), new SetApartDocument(FirefoxFineTiles, 32165) },
            { (Browser.Firefox, "teanwest11"), new SetApartDocument(FirefoxFineTiles, 32167) },
            { (Browser.Firefox, "fontfallback"),
              new SetApartDocument("F-5: Firefox draws the lower part of a cluster of Bengali, below the headline, a pixel lower than Chromium", 1) },
            { (Browser.WebKit, "teanwest"), new SetApartDocument(WebKitFineTiles + "; and " + WebKitHints.Substring("F-5: ".Length), 133) },
            { (Browser.WebKit, "teanwest11"), new SetApartDocument(WebKitFineTiles + "; and " + WebKitHints.Substring("F-5: ".Length), 133) },
            { (Browser.WebKit, "ocad11templatealign"), new SetApartDocument(WebKitFineTiles, 95575) },
            { (Browser.WebKit, "marymoor11_lowerlayers"), new SetApartDocument(WebKitFineTiles, 3) },
            { (Browser.WebKit, "Bitmap"), new SetApartDocument(WebKitScalesDown, 4) },
            { (Browser.WebKit, "page-LincolnPark-Short"), new SetApartDocument(WebKitScalesDown, 6) },
            { (Browser.WebKit, "fontfallback"),
              new SetApartDocument("F-5: WebKit draws nothing of a text whose characters are all Runic, in any face and in SVG alone; and " +
                                   WebKitHints.Substring("F-5: ".Length), 829) },
            { (Browser.WebKit, "frametext1"), new SetApartDocument(WebKitHints, 44) },
            { (Browser.WebKit, "frametext3"), new SetApartDocument(WebKitHints, 35) },
            { (Browser.WebKit, "hiddensymbols"), new SetApartDocument(WebKitHints, 7) },
            { (Browser.WebKit, "hiddensymbols9"), new SetApartDocument(WebKitHints, 7) },
            { (Browser.WebKit, "PenistoneHill"), new SetApartDocument(WebKitHints, 13) },
            { (Browser.WebKit, "page-printarea2-Course1"), new SetApartDocument(WebKitHints, 2) },
        };

        // The metrics the Skia target lays text out with, which the SVG target lays it out with too (T-1).
        private static readonly ITextMetrics textMetrics = new Skia_TextMetrics();

        // The font data of the faces the Skia target draws text with, cut to the characters each document draws.
        private static readonly IFontSubsetProvider fontSubsets = new SkiaFontSubsetProvider();

        // The documents of the cases, each written once and rendered once in Chromium, by the case's name.
        private static readonly ConcurrentDictionary<string, Lazy<Task<BrowserCheckDocument>>> documents =
            new ConcurrentDictionary<string, Lazy<Task<BrowserCheckDocument>>>(StringComparer.Ordinal);

        // Take a document that the rendering comparison wrote and Chromium's image of it, for the browser check to
        // compare the browsers' images with, unless the check has written the document itself.
        // name: the name of the case.
        // svgPath: the path of the document, which is not written again.
        // pixelSize: the size of its image in pixels.
        // drawsText: whether it draws text.
        // chromium: Chromium's image of it, as PNG data.
        public static void Offer(string name, string svgPath, Size pixelSize, bool drawsText, byte[] chromium)
        {
            BrowserCheckDocument document = BrowserCheckDocument.Rendered(name, svgPath, pixelSize, drawsText ? PixelTolerance.Text : PixelTolerance.Default, chromium);
            documents.TryAdd(name, new Lazy<Task<BrowserCheckDocument>>(() => Task.FromResult(document)));
        }

        // Make the cases of the rendering comparison: its drawing, map-area and area-pattern cases.
        private static IEnumerable<ComparisonCase> Cases()
        {
            return ComparisonCase.MakeDrawingCases().Concat(ComparisonCase.MakeMapAreaCases()).Concat(ComparisonCase.MakeAreaPatternCases());
        }

        [TestCaseSource(nameof(Cases))]
        public async Task InFirefox(ComparisonCase comparisonCase)
        {
            await CompareWithChromiumAsync(Browser.Firefox, comparisonCase);
        }

        [TestCaseSource(nameof(Cases))]
        public async Task InWebKit(ComparisonCase comparisonCase)
        {
            await CompareWithChromiumAsync(Browser.WebKit, comparisonCase);
        }

        // Render a case's document in a browser and compare the image with Chromium's.
        // browser: Firefox or WebKit.
        // comparisonCase: the case.
        private static async Task CompareWithChromiumAsync(Browser browser, ComparisonCase comparisonCase)
        {
            BrowserCheckDocument document = await documents.GetOrAdd(comparisonCase.Name,
                                                                      name => new Lazy<Task<BrowserCheckDocument>>(() => WriteAsync(comparisonCase))).Value;
            await document.CompareAsync(browser);
        }

        // Write a case's document as the rendering comparison writes it, and render it in Chromium. A case that makes
        // a call the target does not fully implement, or cannot honor, has no document, and is pending as the
        // comparison holds it. A document that leaves out the drawing of a call, such as a line of zero width (D-5),
        // is drawn like any other, as the browsers draw it alike.
        // comparisonCase: the case.
        private static async Task<BrowserCheckDocument> WriteAsync(ComparisonCase comparisonCase)
        {
            string text;
            bool drawsText;
            using (RecordingGraphicsTarget recording = comparisonCase.Record()) {
                List<string> missingCalls = recording.RecordedCallNames.Where(callName => !Svg_GraphicsTarget.FullyImplements(callName)).ToList();
                if (missingCalls.Count > 0)
                    return BrowserCheckDocument.Pending("Pending until the SVG target implements " + string.Join(", ", missingCalls) + ".");
                try {
                    text = RenderingComparisonTests.WriteDocument(comparisonCase, recording, textMetrics, fontSubsets, out IReadOnlyList<string> callsLeftOut);
                }
                catch (Svg_UnsupportedCallException unsupportedCall) {
                    return BrowserCheckDocument.Pending("Pending as the rendering comparison holds it: the SVG target does not honor " + unsupportedCall.CallName + ".");
                }
                drawsText = RenderingComparisonTests.DrawsText(recording.RecordedCallNames);
            }
            return await BrowserCheckDocument.RenderAsync(comparisonCase.Name, BrowserCheckDocument.Save(comparisonCase.Name, text), comparisonCase.PixelSize, null,
                                                          drawsText ? PixelTolerance.Text : PixelTolerance.Default);
        }
    }

    // The browser check on pages of the SVG export (V-6): the pages of the reference events that CourseSvgPageTests
    // draws, each exported anew as it exports them, and the pages of one file of several, mapexchange1's Course 5 as
    // the baseline tests export it, each shown through its view (L-6), rendered at the resolution the page tests draw
    // at, in Firefox and in WebKit, and compared with Chromium's image as BrowserComparisonTests compares its documents.
    // The culture is fixed, as the page tests fix it.
    [TestFixture, NonParallelizable, SetCulture("en-US"), SetUICulture("en-US")]
    public class BrowserPageComparisonTests
    {
        // The number of pages of the file of several pages, one for each part of mapexchange1's Course 5.
        private const int ViewedPages = 4;

        // The documents of the pages, each written once and rendered once in Chromium, by the page's name.
        private static readonly ConcurrentDictionary<string, Lazy<Task<BrowserCheckDocument>>> documents =
            new ConcurrentDictionary<string, Lazy<Task<BrowserCheckDocument>>>(StringComparer.Ordinal);

        // The pages that CourseSvgPageTests draws, each as a test case for a browser, named by the browser and the name
        // the page goes under in the loop's results.
        // browser: the browser.
        private static IEnumerable<TestCaseData> Pages(Browser browser)
        {
            return CourseSvgPageTests.Pages.Select(page => new TestCaseData(page.Arguments).SetName("In" + browser + "(page-" + page.Arguments[0] + ")"));
        }

        // The pages of the file of several pages, each as a test case for a browser: the page's place in its file.
        // browser: the browser.
        private static IEnumerable<TestCaseData> Views(Browser browser)
        {
            return Enumerable.Range(1, ViewedPages).Select(place => new TestCaseData(place).SetName("In" + browser + "(" + ViewName(place) + ")"));
        }

        private static IEnumerable<TestCaseData> PagesInFirefox() { return Pages(Browser.Firefox); }
        private static IEnumerable<TestCaseData> PagesInWebKit() { return Pages(Browser.WebKit); }
        private static IEnumerable<TestCaseData> ViewsInFirefox() { return Views(Browser.Firefox); }
        private static IEnumerable<TestCaseData> ViewsInWebKit() { return Views(Browser.WebKit); }

        [TestCaseSource(nameof(PagesInFirefox))]
        public async Task PageInFirefox(string name, string eventFile, MapType mapType, int[] courses, bool lowerPurpleLayer, string file)
        {
            await (await PageAsync(name, eventFile, mapType, courses, lowerPurpleLayer, file)).CompareAsync(Browser.Firefox);
        }

        [TestCaseSource(nameof(PagesInWebKit))]
        public async Task PageInWebKit(string name, string eventFile, MapType mapType, int[] courses, bool lowerPurpleLayer, string file)
        {
            await (await PageAsync(name, eventFile, mapType, courses, lowerPurpleLayer, file)).CompareAsync(Browser.WebKit);
        }

        [TestCaseSource(nameof(ViewsInFirefox))]
        public async Task ViewInFirefox(int place)
        {
            await (await ViewAsync(place)).CompareAsync(Browser.Firefox);
        }

        [TestCaseSource(nameof(ViewsInWebKit))]
        public async Task ViewInWebKit(int place)
        {
            await (await ViewAsync(place)).CompareAsync(Browser.WebKit);
        }

        // The name a page of the file of several pages goes under in the loop's results.
        // place: the page's place in its file, from 1.
        private static string ViewName(int place)
        {
            return "view-mapexchange1-Course5-" + place.ToString(CultureInfo.InvariantCulture);
        }

        // Export a page of an event as CourseSvgPageTests exports it, and render it in Chromium, once.
        // name: the name of the page.
        // eventFile: the event's file, among the application's test files.
        // mapType: the kind of map the event is drawn on.
        // courses: the ids of the courses to export, 0 for the page of all controls.
        // lowerPurpleLayer: whether to draw the course in upper and lower purple.
        // file: the name of the file the page is written in.
        private static Task<BrowserCheckDocument> PageAsync(string name, string eventFile, MapType mapType, int[] courses, bool lowerPurpleLayer, string file)
        {
            return documents.GetOrAdd("page-" + name, documentName => new Lazy<Task<BrowserCheckDocument>>(async () => {
                CourseExport.LoadedEvent loadedEvent = await CourseExport.LoadAsync(eventFile, mapType, lowerPurpleLayer);
                CourseSvgSettings settings = CourseExport.Export(loadedEvent, "browsers/pages/" + name, courses.Select(id => new Id<Course>(id)).ToArray());
                ExportedPage page = ExportedPage.LayOut(loadedEvent, settings).Single(candidate => Path.GetFileName(candidate.FileName) == file);
                string text = File.ReadAllText(page.FileName);
                return await BrowserCheckDocument.RenderAsync(documentName, BrowserCheckDocument.Save(documentName, text), page.PixelSizeAt(CourseSvgPageTests.PixelsPerInch),
                                                              null, CourseSvgPageTests.DrawsText(text) ? PixelTolerance.Text : PixelTolerance.Default);
            })).Value;
        }

        // Export mapexchange1's Course 5 into a file of a page for each of its parts, as the baseline tests export it, and
        // render a page through its view in Chromium, once.
        // place: the page's place in its file, from 1.
        private static Task<BrowserCheckDocument> ViewAsync(int place)
        {
            return documents.GetOrAdd(ViewName(place), documentName => new Lazy<Task<BrowserCheckDocument>>(async () => {
                (string svgPath, List<ExportedPage> pages, List<string> viewIds, bool drawsText) = await ExportViewedFileAsync();
                Assert.AreEqual(ViewedPages, pages.Count, "the file holds a page for each part of the course");
                Assert.AreEqual(ViewedPages, viewIds.Count, "the file holds a view for each page");
                ExportedPage page = pages[place - 1];
                return await BrowserCheckDocument.RenderAsync(documentName, svgPath, page.PixelSizeAt(CourseSvgPageTests.PixelsPerInch), viewIds[place - 1],
                                                              drawsText ? PixelTolerance.Text : PixelTolerance.Default);
            })).Value;
        }

        // The file of several pages, once exported: its path, its pages in their order in it, their views' ids, in the
        // order the root holds them, and whether it draws text.
        private static readonly Lazy<Task<(string, List<ExportedPage>, List<string>, bool)>> viewedFile =
            new Lazy<Task<(string, List<ExportedPage>, List<string>, bool)>>(async () => {
                CourseExport.LoadedEvent loadedEvent = await CourseExport.LoadAsync("controller/mapexchange1.ppen", MapType.None, false);
                CourseSvgSettings settings = CourseExport.Export(loadedEvent, "browsers/views/mapexchange1-Course5", new Id<Course>[] { new Id<Course>(6) },
                                                                 CoursePdfSettings.PdfFileCreation.FilePerCourse);
                List<ExportedPage> pages = ExportedPage.LayOut(loadedEvent, settings).OrderBy(page => page.PlaceInFile).ToList();
                Assert.AreEqual(1, pages.Select(page => page.FileName).Distinct().Count(), "the course is exported into one file");
                string text = File.ReadAllText(pages[0].FileName);
                XNamespace svg = "http://www.w3.org/2000/svg";
                List<string> viewIds = XDocument.Parse(text).Root.Elements(svg + "view").Select(view => (string)view.Attribute("id")).ToList();
                return (BrowserCheckDocument.Save("view-mapexchange1-Course5", text), pages, viewIds, CourseSvgPageTests.DrawsText(text));
            });

        // Export the file of several pages, once.
        private static Task<(string, List<ExportedPage>, List<string>, bool)> ExportViewedFileAsync()
        {
            return viewedFile.Value;
        }
    }

    // A document of the browser check, written and rendered in Chromium, which each browser's rendering is compared
    // with; or a case that has no document, and is pending.
    internal sealed class BrowserCheckDocument
    {
        // The name the document goes under in the loop's results.
        private readonly string name;

        // The path of its SVG file.
        private readonly string svgPath;

        // The size of its image in pixels.
        private readonly Size pixelSize;

        // The id of the view it is shown through, or null for its own view box.
        private readonly string view;

        // How far a browser's image may differ from Chromium's.
        private readonly PixelTolerance tolerance;

        // Chromium's image of it, as PNG data.
        private readonly byte[] chromium;

        // Why the case has no document, or null when it has one.
        private readonly string whyPending;

        // Create a document of the check.
        private BrowserCheckDocument(string name, string svgPath, Size pixelSize, string view, PixelTolerance tolerance, byte[] chromium, string whyPending)
        {
            this.name = name;
            this.svgPath = svgPath;
            this.pixelSize = pixelSize;
            this.view = view;
            this.tolerance = tolerance;
            this.chromium = chromium;
            this.whyPending = whyPending;
        }

        // A case with no document, pending for the reason given.
        // whyPending: why, in the words of a pending test.
        public static BrowserCheckDocument Pending(string whyPending)
        {
            return new BrowserCheckDocument(null, null, Size.Empty, null, PixelTolerance.Exact, null, whyPending);
        }

        // Save a document's text in a folder of the check's own, apart from the comparison's svg/, which may be read
        // while the check writes, and return its path.
        // name: the name of the document.
        // text: its text.
        public static string Save(string name, string text)
        {
            string path = Path.Combine(Path.GetTempPath(), "svg-browser-check-" + Environment.ProcessId.ToString(CultureInfo.InvariantCulture), name + ".svg");
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, text, new UTF8Encoding(false));
            return path;
        }

        // A document that Chromium has rendered.
        // name: the name the document goes under in the loop's results.
        // svgPath: the path of its SVG file.
        // pixelSize: the size of its image in pixels.
        // tolerance: how far a browser's image may differ from Chromium's.
        // chromium: Chromium's image of it, as PNG data.
        public static BrowserCheckDocument Rendered(string name, string svgPath, Size pixelSize, PixelTolerance tolerance, byte[] chromium)
        {
            return new BrowserCheckDocument(name, svgPath, pixelSize, null, tolerance, chromium, null);
        }

        // Render a saved document in Chromium, keep the image in the directory of renderings as
        // browsers/chromium/<name>.png, and return the document of the check.
        // name: the name the document goes under in the loop's results.
        // svgPath: the path of its SVG file.
        // pixelSize: the size of its image in pixels.
        // view: the id of the view it is shown through, or null for its own view box.
        // tolerance: how far a browser's image may differ from Chromium's.
        public static async Task<BrowserCheckDocument> RenderAsync(string name, string svgPath, Size pixelSize, string view, PixelTolerance tolerance)
        {
            byte[] chromium = await BrowserRenderer.RenderPngAsync(Browser.Chromium, svgPath, pixelSize, view);
            if (LoopResults.RenderingsDirectory != null) {
                string kept = Path.Combine(LoopResults.RenderingsDirectory, ImageComparison.BrowserFolder(Browser.Chromium), name + ".png");
                Directory.CreateDirectory(Path.GetDirectoryName(kept));
                File.WriteAllBytes(kept, chromium);
            }
            return new BrowserCheckDocument(name, svgPath, pixelSize, view, tolerance, chromium, null);
        }

        // Render the document in a browser and compare the image with Chromium's within the tolerance, or end the test
        // as pending when there is no document, and as ignored when the document is set apart for the browser and still
        // differs.
        // browser: Firefox or WebKit.
        public async Task CompareAsync(Browser browser)
        {
            if (whyPending != null)
                Assert.Ignore(whyPending);
            byte[] png = await BrowserRenderer.RenderPngAsync(browser, svgPath, pixelSize, view);
            string difference;
            int pixelsOutside;
            using (SKBitmap chromiumImage = SKBitmap.Decode(chromium))
            using (SKBitmap rendering = SKBitmap.Decode(png))
                difference = ImageComparison.DescribeBrowserDifference(browser, name, chromiumImage, rendering, tolerance, out pixelsOutside);
            if (BrowserComparisonTests.SetApart.TryGetValue((browser, name), out SetApartDocument setApart)) {
                if (difference == null) {
                    Assert.Fail(string.Format("{0} draws {1} as Chromium draws it now: take it off the documents set apart for {0}, which say \"{2}\".",
                                              browser, name, setApart.Why));
                }
                if (pixelsOutside < 0 || pixelsOutside > setApart.PixelsOutside) {
                    Assert.Fail(string.Format("{0} draws {1} otherwise than when it was set apart, with {2} pixels outside the tolerance: {3}",
                                              browser, name, setApart.PixelsOutside, difference));
                }
                LoopResults.SaveText(Path.Combine("browsers", "apart", browser.ToString().ToLowerInvariant(), name + ".txt"), setApart.Why + "\n");
                Assert.Ignore("Set apart for " + browser + ": " + setApart.Why + ". " + difference);
            }
            if (difference != null)
                Assert.Fail(difference);
        }
    }

    // A document set apart for a browser: why the browser draws it otherwise than Chromium, and how many pixels of its
    // image lay outside the tolerance when it was set apart, which it may not exceed.
    internal sealed class SetApartDocument
    {
        // Why the browser draws the document otherwise, beginning with the requirement it falls short of.
        public readonly string Why;

        // How many pixels of the browser's image lay outside the tolerance when the document was set apart.
        public readonly int PixelsOutside;

        // Create the record of a document set apart.
        // why: why the browser draws it otherwise.
        // pixelsOutside: how many pixels of the browser's image lay outside the tolerance.
        public SetApartDocument(string why, int pixelsOutside)
        {
            Why = why;
            PixelsOutside = pixelsOutside;
        }
    }
}
