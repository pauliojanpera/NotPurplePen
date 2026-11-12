using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using NUnit.Framework;
using PurplePen;
using PurplePen.MapModel;
using SkiaSharp;

namespace Map_Svg.Tests
{
    // Exports reference events into files of several pages and checks the layout of each file (V-8, L-2 to L-7): the
    // files are those PDF export writes with the same choice of which pages go in each; each map is defined once, the
    // base map in a part for each of its layers whatever the number of pages, and each course part likewise; every page
    // uses its base map's parts and its own course part's, bottom up, the pages of a course part that takes several
    // sharing them, and carries a view of its paper and an Inkscape page, both named after its course part, laid left to
    // right 10 mm apart; the document's own view box is the first
    // page's; and the structure walk of V-7 holds. Then Chromium draws each page through its view, as a link to the
    // file with the view's id draws it, and the image matches the page as the application draws it with the Skia
    // target, within the tolerance of the exported pages' comparison: each view shows its own page's print area, and
    // nothing of the pages beside it. The culture is fixed, as the baseline tests fix it.
    //
    // A file goes to the loop's results as svg/layout-<export>-<file>.svg, and the image of a page through its view is
    // compared as layout-<export>-<file>-<page>, its images kept as those of CourseSvgPageTests are: with the results only
    // when it falls outside the tolerance, and of every page in the directory of renderings, when there is one.
    [TestFixture, NonParallelizable, SetCulture("en-US"), SetUICulture("en-US")]
    public class CourseSvgLayoutTests
    {
        private static readonly XNamespace Svg = "http://www.w3.org/2000/svg";
        private static readonly XNamespace Npp = SvgDocument.MetadataNamespace;
        private static readonly XNamespace XLink = "http://www.w3.org/1999/xlink";
        private static readonly XNamespace Inkscape = SvgDocument.InkscapeNamespace;
        private static readonly XNamespace Sodipodi = SvgDocument.SodipodiNamespace;

        // The gap between pages laid side by side, in millimetres.
        private const double GapBetweenPages = 10;

        // How far a length in the document may lie from the one the layout gives, in millimetres: the rounding of the
        // shortest text of a single-precision number.
        private const double LengthTolerance = 0.001;

        // The reference exports, each with its name, its event's file, the kind of map the event is drawn on, the
        // courses exported, or none for every course and the page of all controls, whether the course is drawn in upper
        // and lower purple, which pages go in each file, and the course whose print area is made twice as wide and high,
        // so that it takes several pages, or 0 for none; and what it covers.
        internal static readonly TestCaseData[] Exports = {
            // An OCAD map whose courses print on pages of five paper sizes and orientations, all in one file.
            Export("LincolnParkPrintAreas2-OneFile", "courseprinting/Lincoln Park PrintAreas 2.ppen", MapType.OCAD, new int[0], false,
                   CoursePdfSettings.PdfFileCreation.SingleFile, 0),
            // The map in two parts, with the course's lower layer between them, shared by the pages of two courses in
            // one file, one of which takes several pages that share its course drawing.
            Export("OverprintTestLowerPurple-OneFile", "courseprinting/Overprint test.ppen", MapType.OCAD, new int[0], true,
                   CoursePdfSettings.PdfFileCreation.SingleFile, 1),
            // A file for each course, with no map: each course with map exchanges holds a page for each of its parts.
            Export("mapexchange1-FilePerCourse", "controller/mapexchange1.ppen", MapType.None, new int[0], false,
                   CoursePdfSettings.PdfFileCreation.FilePerCourse, 0),
        };

        // A reference export, as a test case named by its name.
        // name: the name.
        // eventFile: the event's file, among the application's test files.
        // mapType: the kind of map the event is drawn on.
        // courses: the ids of the courses to export, or none for every course of the event and its page of all controls.
        // lowerPurpleLayer: whether to draw the course in upper and lower purple.
        // fileCreation: which pages go in each file.
        // enlargedCourse: the id of the course whose print area is made twice as wide and high, or 0 for none.
        private static TestCaseData Export(string name, string eventFile, MapType mapType, int[] courses, bool lowerPurpleLayer,
                                           CoursePdfSettings.PdfFileCreation fileCreation, int enlargedCourse)
        {
            return new TestCaseData(name, eventFile, mapType, courses, lowerPurpleLayer, fileCreation, enlargedCourse).SetName(name);
        }

        // Export an event with a choice of which pages go in each file, check the layout of each file, and draw each of
        // its pages through its view in Chromium.
        // name: the name of the export.
        // eventFile: the event's file, among the application's test files.
        // mapType: the kind of map the event is drawn on.
        // courses: the ids of the courses to export, or none for every course of the event and its page of all controls.
        // lowerPurpleLayer: whether to draw the course in upper and lower purple.
        // fileCreation: which pages go in each file.
        // enlargedCourse: the id of the course whose print area is made twice as wide and high, or 0 for none.
        [TestCaseSource(nameof(Exports))]
        public async Task EachFileDefinesItsMapsOnceAndShowsEachPageThroughItsView(string name, string eventFile, MapType mapType, int[] courses,
                                                                                   bool lowerPurpleLayer, CoursePdfSettings.PdfFileCreation fileCreation,
                                                                                   int enlargedCourse)
        {
            CourseExport.LoadedEvent loadedEvent = await CourseExport.LoadAsync(eventFile, mapType, lowerPurpleLayer);
            if (enlargedCourse != 0)
                EnlargePrintArea(loadedEvent.Controller, new CourseDesignator(new Id<Course>(enlargedCourse)));
            CourseSvgSettings settings = CourseExport.Export(loadedEvent, "layout/" + name, courses.Select(id => new Id<Course>(id)).ToArray(), fileCreation);
            List<ExportedPage> pages = ExportedPage.LayOut(loadedEvent, settings);
            List<string> fileNames = pages.Select(page => page.FileName).Distinct().ToList();
            Assert.Greater(pages.Count, fileNames.Count, "some file holds several pages");
            if (enlargedCourse != 0)
                Assert.Greater(pages.Count(page => page.Page.courseDesignator.CourseId == new Id<Course>(enlargedCourse)), 1, "the enlarged course takes several pages");
            CollectionAssert.AreEqual(fileNames.Select(Path.GetFileName).OrderBy(file => file, StringComparer.Ordinal),
                                      CourseExport.SvgFilesIn(settings.outputDirectory), "the files are those PDF export writes");

            MapDisplay mapDisplay = loadedEvent.Controller.MapDisplay;
            string mapLabel = mapType == MapType.None ? null : Path.GetFileName(mapDisplay.FileName);
            int layers = mapDisplay.HasLayersAboveCourse ? 2 : 1;

            List<string> problems = new List<string>();
            List<string> differences = new List<string>();
            foreach (string fileName in fileNames) {
                string fileStem = Path.GetFileNameWithoutExtension(fileName).Replace(" ", "");
                List<ExportedPage> filePages = pages.Where(page => page.FileName == fileName).OrderBy(page => page.PlaceInFile).ToList();
                string text = File.ReadAllText(fileName);
                XDocument document = XDocument.Parse(text);
                List<string> viewIds = new List<string>();
                problems.AddRange(CheckLayout(document, filePages, mapLabel, layers, viewIds).Select(problem => Path.GetFileName(fileName) + ": " + problem));
                problems.AddRange(CourseSvgStructureTests.Walk(document).Select(problem => Path.GetFileName(fileName) + ": " + problem));
                if (viewIds.Count != filePages.Count)
                    continue;

                // Draw each page through its view, and compare it with the page as the application draws it.
                string svgPath = LoopResults.SaveSvg("layout-" + name + "-" + fileStem, text);
                for (int index = 0; index < filePages.Count; ++index) {
                    ExportedPage page = filePages[index];
                    string caseName = "layout-" + name + "-" + fileStem + "-" + (index + 1).ToString(CultureInfo.InvariantCulture);
                    Size pixelSize = page.PixelSizeAt(CourseSvgPageTests.PixelsPerInch);
                    using (SKBitmap reference = page.RenderReference(loadedEvent, settings, pixelSize))
                    using (SKBitmap rendering = await BrowserRenderer.RenderAsync(svgPath, pixelSize, null, viewIds[index])) {
                        string difference = ImageComparison.DescribeDifference(caseName, reference, rendering,
                                                                               CourseSvgPageTests.DrawsText(text) ? PixelTolerance.Text : PixelTolerance.Default);
                        LoopResults.KeepRendering(Path.Combine("course-pages", caseName + ".png"), reference);
                        if (difference != null) {
                            LoopResults.SaveImage(Path.Combine("course-pages", caseName + ".png"), reference);
                            differences.Add(difference + " The page as the application draws it is course-pages/" + caseName + ".png.");
                        }
                    }
                }
            }

            Assert.IsEmpty(problems, "{0} problems, the first of them:\n{1}", problems.Count, string.Join("\n", problems.Take(40)));
            Assert.IsEmpty(differences, string.Join("\n", differences));
        }

        // Make a course's print area twice as wide and high, around its middle, and free it from its paper's size, so that
        // the course takes several pages when large print areas are not cropped.
        // controller: the controller that loaded the event.
        // designator: the course.
        private static void EnlargePrintArea(Controller controller, CourseDesignator designator)
        {
            PrintArea printArea = (PrintArea)controller.GetCurrentPrintArea(designator).Clone();
            RectangleF area = controller.GetCurrentPrintAreaRectangle(designator);
            printArea.autoPrintArea = false;
            printArea.restrictToPageSize = false;
            printArea.printAreaRectangle = new RectangleF(area.X - area.Width / 2, area.Y - area.Height / 2, area.Width * 2, area.Height * 2);
            UndoMgr undoMgr = controller.GetUndoMgr();
            undoMgr.BeginCommand(1127, "Set print area");
            ChangeEvent.ChangePrintArea(controller.GetEventDB(), designator, true, printArea);
            undoMgr.EndCommand(1127);
        }

        // Check the layout of a file of pages, and return what is wrong with it.
        // document: the file's document.
        // pages: the pages the export lays out for the file, in order.
        // mapLabel: the label of the base map, or null for an event without one.
        // layers: how many layers the map and each course part are drawn in: 2 when the course goes between two parts of
        //         the map, otherwise 1.
        // viewIds: receives the id of each page's view, in order, when there is a view for each page.
        private static List<string> CheckLayout(XDocument document, List<ExportedPage> pages, string mapLabel, int layers, List<string> viewIds)
        {
            List<string> problems = new List<string>();
            XElement root = document.Root;

            // The views, the Inkscape pages and the page groups: one of each for each page, in order, each view right
            // before its page's group.
            List<XElement> views = root.Elements(Svg + "view").ToList();
            List<XElement> namedViews = root.Elements(Sodipodi + "namedview").ToList();
            List<XElement> inkscapePages = namedViews.Count == 1 ? namedViews[0].Elements(Inkscape + "page").ToList() : new List<XElement>();
            List<XElement> pageGroups = root.Elements(Svg + "g").ToList();
            if (root.Descendants(Svg + "view").Count() != views.Count || root.Descendants(Inkscape + "page").Count() != inkscapePages.Count)
                problems.Add("a view or an Inkscape page lies elsewhere than among the root's children and in the root's sodipodi:namedview");
            if (namedViews.Count != 1)
                problems.Add(namedViews.Count.ToString(CultureInfo.InvariantCulture) + " sodipodi:namedview elements, not 1");
            if (views.Count != pages.Count || inkscapePages.Count != pages.Count || pageGroups.Count != pages.Count) {
                problems.Add(string.Format(CultureInfo.InvariantCulture, "{0} pages laid out, but {1} views, {2} Inkscape pages and {3} page groups",
                                           pages.Count, views.Count, inkscapePages.Count, pageGroups.Count));
                return problems;
            }

            // The parts, each with the maps it holds, outside the maps nested within them, such as templates. A group of the
            // definitions that the parts themselves use holds repeated shapes, and is no part (S-5).
            XElement definitions = root.Element(Svg + "defs");
            HashSet<string> usedWithinDefinitions = CourseSvgStructureTests.IdsUsedWithinDefinitions(root);
            Dictionary<string, List<string>> mapsOfPart = new Dictionary<string, List<string>>(StringComparer.Ordinal);
            if (definitions != null) {
                foreach (XElement part in definitions.Elements(Svg + "g").Where(element => element.Attribute("id") != null &&
                                                                                           !usedWithinDefinitions.Contains((string)element.Attribute("id")))) {
                    mapsOfPart.Add((string)part.Attribute("id"),
                                   part.Descendants().Where(element => element.Attribute("data-m") != null &&
                                                                       !element.Ancestors().TakeWhile(ancestor => ancestor != part).Any(ancestor => ancestor.Attribute("data-m") != null))
                                       .Select(element => (string)element.Attribute("data-m")).Distinct().ToList());
                }
            }

            // The maps' records: the base map's, and a course map's for each course part, by its label.
            List<XElement> mapRecords = root.Descendants(Npp + "map").ToList();
            Dictionary<string, string> mapByLabel = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (XElement record in mapRecords) {
                string label = (string)record.Attribute("label");
                if (mapByLabel.ContainsKey(label))
                    problems.Add("two maps are labeled " + label);
                else
                    mapByLabel.Add(label, (string)record.Attribute("n"));
            }
            List<string> courseParts = pages.Select(page => page.Page.description).Distinct(StringComparer.Ordinal).ToList();
            int expectedMaps = (mapLabel == null ? 0 : 1) + courseParts.Count;
            if (mapRecords.Count != expectedMaps)
                problems.Add(string.Format(CultureInfo.InvariantCulture, "{0} maps are recorded, not the base map and the {1} course parts", mapRecords.Count, courseParts.Count));

            // Each map is defined once: a part for each layer it is drawn in, whatever the number of pages, and each part
            // holds one map, or nothing.
            int expectedParts = (mapLabel == null ? 0 : layers) + courseParts.Count * layers;
            if (mapsOfPart.Count != expectedParts)
                problems.Add(string.Format(CultureInfo.InvariantCulture, "{0} parts are defined, not {1}: {2} layers of the base map and of each of {3} course parts",
                                           mapsOfPart.Count, expectedParts, layers, courseParts.Count));
            foreach (KeyValuePair<string, List<string>> part in mapsOfPart.Where(pair => pair.Value.Count > 1))
                problems.Add("part " + part.Key + " holds the maps " + string.Join(" ", part.Value));
            foreach (IGrouping<string, string> map in mapsOfPart.Where(pair => pair.Value.Count == 1).GroupBy(pair => pair.Value[0], pair => pair.Key)) {
                if (map.Count() > layers)
                    problems.Add("map " + map.Key + " is drawn in the parts " + string.Join(" ", map) + ", more than its layers");
            }

            // The root is sized to the first page, and its view box is the first page's view's.
            SizeF firstPaper = pages[0].PaperSize;
            if (!LengthIs((string)root.Attribute("width"), firstPaper.Width, "mm") || !LengthIs((string)root.Attribute("height"), firstPaper.Height, "mm"))
                problems.Add("the root's size is " + (string)root.Attribute("width") + " by " + (string)root.Attribute("height") + ", not the first page's");
            if ((string)root.Attribute("viewBox") != (string)views[0].Attribute("viewBox"))
                problems.Add("the root's view box is " + (string)root.Attribute("viewBox") + ", the first page's view's " + (string)views[0].Attribute("viewBox"));

            HashSet<string> partsUsed = new HashSet<string>(StringComparer.Ordinal);
            HashSet<string> viewIdsGiven = new HashSet<string>(StringComparer.Ordinal);
            double left = 0;
            for (int index = 0; index < pages.Count; ++index) {
                ExportedPage page = pages[index];
                SizeF paper = page.PaperSize;
                string pageName = "page " + (index + 1).ToString(CultureInfo.InvariantCulture) + ", " + page.Page.description;

                // The view and the Inkscape page are the page's paper on the canvas, named after its course part.
                XElement view = views[index], inkscapePage = inkscapePages[index];
                double[] viewBox = Numbers((string)view.Attribute("viewBox"));
                if (viewBox.Length != 4 || !Near(viewBox[0], left) || !Near(viewBox[1], 0) || !Near(viewBox[2], paper.Width) || !Near(viewBox[3], paper.Height))
                    problems.Add(pageName + ": its view's box is " + (string)view.Attribute("viewBox") + ", not its paper at " + left.ToString("0.###", CultureInfo.InvariantCulture));
                if (!LengthIs((string)inkscapePage.Attribute("x"), left, "") || !LengthIs((string)inkscapePage.Attribute("y"), 0, "") ||
                    !LengthIs((string)inkscapePage.Attribute("width"), paper.Width, "") || !LengthIs((string)inkscapePage.Attribute("height"), paper.Height, ""))
                    problems.Add(pageName + ": its Inkscape page is not its paper");
                if ((string)inkscapePage.Attribute(Inkscape + "label") != page.Page.description)
                    problems.Add(pageName + ": its Inkscape page is labeled " + (string)inkscapePage.Attribute(Inkscape + "label"));
                string baseId = SvgDocument.XmlNameOf(page.Page.description), expectedId = baseId;
                for (int number = 2; viewIdsGiven.Contains(expectedId); ++number)
                    expectedId = baseId + "_" + number.ToString(CultureInfo.InvariantCulture);
                viewIdsGiven.Add(expectedId);
                if ((string)view.Attribute("id") != expectedId)
                    problems.Add(pageName + ": its view is named " + (string)view.Attribute("id") + ", not " + expectedId);
                if (pageGroups[index].ElementsBeforeSelf().LastOrDefault() != view)
                    problems.Add(pageName + ": its group does not follow its view");
                viewIds.Add((string)view.Attribute("id"));
                left += paper.Width + GapBetweenPages;

                // The page uses its base map's parts and its own course part's, from the bottom up: the map below the
                // course, the course's lower layer, the map above it and the course's upper layer. A part that draws
                // nothing holds no map.
                List<string> expectedMapsOfUses = new List<string>();
                string courseMap;
                mapByLabel.TryGetValue(page.Page.description, out courseMap);
                string baseMap = null;
                if (mapLabel != null)
                    mapByLabel.TryGetValue(mapLabel, out baseMap);
                for (int layer = 0; layer < layers; ++layer) {
                    if (mapLabel != null)
                        expectedMapsOfUses.Add(baseMap);
                    expectedMapsOfUses.Add(courseMap);
                }
                List<string> uses = pageGroups[index].Elements(Svg + "use").Select(use => ((string)use.Attribute(XLink + "href") ?? "").TrimStart('#')).ToList();
                if (uses.Count != expectedMapsOfUses.Count || pageGroups[index].Elements().Count() != uses.Count) {
                    problems.Add(pageName + ": its group holds " + pageGroups[index].Elements().Count().ToString(CultureInfo.InvariantCulture) +
                                 " elements, " + uses.Count.ToString(CultureInfo.InvariantCulture) + " of them uses, not " +
                                 expectedMapsOfUses.Count.ToString(CultureInfo.InvariantCulture) + " uses");
                    continue;
                }
                for (int place = 0; place < uses.Count; ++place) {
                    List<string> maps;
                    if (!mapsOfPart.TryGetValue(uses[place], out maps))
                        problems.Add(pageName + ": it uses " + uses[place] + ", which is no part");
                    else if (maps.Count > 0 && (expectedMapsOfUses[place] == null || maps[0] != expectedMapsOfUses[place]))
                        problems.Add(pageName + ": its use " + (place + 1).ToString(CultureInfo.InvariantCulture) + " shows map " + maps[0] +
                                     ", not map " + (expectedMapsOfUses[place] ?? "(none recorded)"));
                    partsUsed.Add(uses[place]);
                }
            }

            foreach (string part in mapsOfPart.Keys.Where(part => !partsUsed.Contains(part)))
                problems.Add("no page shows part " + part);
            if (problems.Count > 0)
                viewIds.Clear();
            return problems;
        }

        // Whether a length's text gives a value, within the tolerance, followed by a unit.
        // text: the text, or null.
        // value: the value.
        // unit: the unit that follows the number, or the empty string for none.
        private static bool LengthIs(string text, double value, string unit)
        {
            if (text == null || !text.EndsWith(unit, StringComparison.Ordinal))
                return false;
            double number;
            return double.TryParse(text.Substring(0, text.Length - unit.Length), NumberStyles.Float, CultureInfo.InvariantCulture, out number) && Near(number, value);
        }

        // The numbers of a list separated by spaces, or none when one of them is not a number.
        // text: the list, or null.
        private static double[] Numbers(string text)
        {
            string[] items = (text ?? "").Split(' ', StringSplitOptions.RemoveEmptyEntries);
            double[] numbers = new double[items.Length];
            for (int index = 0; index < items.Length; ++index) {
                if (!double.TryParse(items[index], NumberStyles.Float, CultureInfo.InvariantCulture, out numbers[index]))
                    return new double[0];
            }
            return numbers;
        }

        // Whether two lengths in millimetres are the same, within the tolerance.
        // a, b: the lengths.
        private static bool Near(double a, double b)
        {
            return Math.Abs(a - b) <= LengthTolerance;
        }
    }
}
