using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using NUnit.Framework;
using PurplePen;
using PurplePen.MapModel;

namespace Map_Svg.Tests
{
    // Walks the files the SVG export writes of the baseline tests' reference events (V-7): every element drawn in a
    // palette color traces through the transparency group of the palette color (data-c), the group of its symbol
    // definition (data-s) and its palette color coverage (data-k) to the palette color, the symbol definition and the
    // symbol keys in the tables of the metadata, and a symbol key of a map belongs to one symbol definition; ids are
    // unique and every reference resolves; every map names the views of the pages that show it; and the course and its
    // controls carry their ids and kinds in the event's .ppen file, and every course object traces to its course, its
    // controls and the elements that its symbols draw, whose keys are the course object's key and an index (L-5, CD-1,
    // CD-2). An object of the map's image or layout layer, which the map draws beside its palette colors, sits under the
    // group of its symbol definition outside any transparency group and carries its symbol key, and the record of its
    // symbol definition names no palette colors. A bitmap of the map carries no key.
    [TestFixture, NonParallelizable, SetCulture("en-US"), SetUICulture("en-US")]
    public class CourseSvgStructureTests
    {
        private static readonly XNamespace Svg = "http://www.w3.org/2000/svg";
        private static readonly XNamespace Npp = "https://pauliojanpera.github.io/NotPurplePen/svg";
        private static readonly XNamespace XLink = "http://www.w3.org/1999/xlink";

        // The elements that draw something.
        private static readonly HashSet<string> DrawingElements = new HashSet<string>(StringComparer.Ordinal) {
            "path", "rect", "circle", "ellipse", "line", "polyline", "polygon", "text", "image", "use",
        };

        // The elements whose content defines something rather than draws it.
        private static readonly HashSet<string> Definitions = new HashSet<string>(StringComparer.Ordinal) {
            "clipPath", "pattern", "mask", "marker", "symbol", "linearGradient", "radialGradient", "filter",
        };

        // The number of controls each kind of course object belongs to (CD-2): two for a leg, one for a control
        // circle, start, finish, crossing point, control number or, on a page of all controls, control code.
        private static readonly Dictionary<string, int> ControlsOfKind = new Dictionary<string, int>(StringComparer.Ordinal) {
            { "leg", 2 }, { "control", 1 }, { "start", 1 }, { "finish", 1 }, { "control-number", 1 }, { "code", 1 }, { "crossing", 1 },
        };

        // A reference to an id in an attribute's value, as url(#id).
        private static readonly Regex UrlReference = new Regex(@"url\(#([^)]+)\)", RegexOptions.CultureInvariant);

        // The kinds of control, as a .ppen file names them (CD-1).
        private static readonly HashSet<string> ControlKinds = new HashSet<string>(StringComparer.Ordinal) {
            "normal", "start", "finish", "crossing-point", "map-exchange", "map-issue",
        };

        // An id of a .ppen file: a whole number above zero.
        private static readonly Regex PpenId = new Regex("^[1-9][0-9]*$", RegexOptions.CultureInvariant);

        // Export courses of an event as the baseline tests do, and walk each file the export writes.
        // eventName: the name the event's baselines go under.
        // eventFile: the event's file, among the application's test files.
        // mapType: the kind of map the event is drawn on.
        // courses: the ids of the courses to export, or none for every course of the event and its page of all controls.
        // lowerPurpleLayer: whether to draw the course in upper and lower purple.
        // fileCreation: which pages go in each file.
        // drawAreaPatternsAndDashesAsSeparateShapes: whether every area pattern is drawn as separate shapes, element by element,
        // and every dashed line dash by dash, rather than as a pattern fill and a dash pattern.
        [TestCaseSource(typeof(CourseSvgBaselineTests), nameof(CourseSvgBaselineTests.Events))]
        public async Task EveryElementTracesToItsMapAndEveryCourseObjectToItsControls(string eventName, string eventFile, MapType mapType, int[] courses, bool lowerPurpleLayer,
                                                                                    CoursePdfSettings.PdfFileCreation fileCreation, bool drawAreaPatternsAndDashesAsSeparateShapes)
        {
            Id<Course>[] courseIds = courses.Select(id => new Id<Course>(id)).ToArray();
            string folder = await CourseExport.ExportAsync(eventFile, "structure/" + eventName, mapType, lowerPurpleLayer, courseIds, fileCreation,
                                                           drawAreaPatternsAndDashesAsSeparateShapes);

            List<string> problems = new List<string>();
            foreach (string file in CourseExport.SvgFilesIn(folder))
                problems.AddRange(Walk(XDocument.Load(Path.Combine(folder, file))).Select(problem => file + ": " + problem));
            Assert.IsEmpty(problems, "{0} problems, the first of them:\n{1}", problems.Count, string.Join("\n", problems.Take(40)));
        }

        // Walk a document and return what is wrong with its structure.
        // document: the document.
        public static List<string> Walk(XDocument document)
        {
            List<string> problems = new List<string>();
            XElement root = document.Root;

            // Ids are unique (L-5), and every reference resolves.
            List<string> ids = root.DescendantsAndSelf().Where(element => element.Name.Namespace == Svg)
                                   .Select(element => (string)element.Attribute("id")).Where(id => id != null).ToList();
            foreach (IGrouping<string, string> repeated in ids.GroupBy(id => id, StringComparer.Ordinal).Where(group => group.Count() > 1))
                problems.Add(string.Format(CultureInfo.InvariantCulture, "the id {0} is given {1} times", repeated.Key, repeated.Count()));
            HashSet<string> knownIds = new HashSet<string>(ids, StringComparer.Ordinal);
            foreach (XElement element in root.Descendants()) {
                string href = (string)element.Attribute(XLink + "href");
                if (href != null && href.StartsWith("#", StringComparison.Ordinal) && !knownIds.Contains(href.Substring(1)))
                    problems.Add("a " + element.Name.LocalName + " refers to " + href + ", which nothing is");
                foreach (XAttribute attribute in element.Attributes()) {
                    foreach (Match reference in UrlReference.Matches(attribute.Value)) {
                        if (!knownIds.Contains(reference.Groups[1].Value))
                            problems.Add("a " + element.Name.LocalName + "'s " + attribute.Name.LocalName + " refers to #" + reference.Groups[1].Value + ", which nothing is");
                    }
                }
            }

            // The tables of each map.
            Dictionary<string, MapRecord> maps = new Dictionary<string, MapRecord>(StringComparer.Ordinal);
            foreach (XElement mapElement in root.Descendants(Npp + "map")) {
                MapRecord map = new MapRecord(mapElement);
                if (maps.ContainsKey(map.Number))
                    problems.Add("map " + map.Number + " has two records");
                else
                    maps.Add(map.Number, map);
            }

            // Each map names the views of the pages that show it, and every page is shown by some map's view (CD-1).
            HashSet<string> views = new HashSet<string>(root.Elements(Svg + "view").Select(view => (string)view.Attribute("id")), StringComparer.Ordinal);
            HashSet<string> viewsNamed = new HashSet<string>(StringComparer.Ordinal);
            foreach (MapRecord map in maps.Values) {
                foreach (string view in map.Views) {
                    viewsNamed.Add(view);
                    if (!views.Contains(view))
                        problems.Add("map " + map.Number + " names the view " + view + ", which no page has");
                }
            }
            foreach (string view in views.Where(view => !viewsNamed.Contains(view)))
                problems.Add("the page of view " + view + " shows no map");

            // Every drawn element, outside the definitions of clips and the like. A symbol key is that of a symbol of its
            // map, so the same key can name a symbol of each of several maps, such as each part of a course.
            Dictionary<(string, string), string> symbolsByKey = new Dictionary<(string, string), string>();
            foreach (XElement element in root.Descendants()) {
                if (element.Name.Namespace != Svg || !DrawingElements.Contains(element.Name.LocalName))
                    continue;
                if (element.Ancestors().Any(ancestor => Definitions.Contains(ancestor.Name.LocalName)))
                    continue;

                XElement mapGroup = element.Ancestors(Svg + "g").FirstOrDefault(group => group.Attribute("data-m") != null);
                if (mapGroup == null) {
                    // Outside any map there are only the definitions that the maps refer to, such as the shared shapes of
                    // the glyphs and the images, and the pages' references to the parts of maps.
                    bool definition = element.Ancestors(Svg + "defs").Any();
                    bool pageReference = element.Name.LocalName == "use" && !definition;
                    if (element.Attribute("data-k") != null || !(definition || pageReference))
                        problems.Add("a " + element.Name.LocalName + " is drawn outside any map");
                    continue;
                }
                string mapNumber = (string)mapGroup.Attribute("data-m");
                MapRecord map;
                if (!maps.TryGetValue(mapNumber, out map)) {
                    problems.Add("map " + mapNumber + " is drawn but has no record");
                    continue;
                }

                // A bitmap base map or an image template sits under its map outside any transparency group, and so does
                // an object of the image or layout layer, under the group of its symbol definition.
                XElement transparencyGroup = element.Ancestors(Svg + "g").TakeWhile(group => group != mapGroup).FirstOrDefault(group => group.Attribute("data-c") != null);
                if (transparencyGroup == null) {
                    XElement ownGroup = element.Ancestors(Svg + "g").TakeWhile(group => group != mapGroup).FirstOrDefault(group => group.Attribute("data-s") != null);
                    string[] ownKeys = KeysOf(element, ownGroup ?? mapGroup);
                    if (ownKeys == null)
                        continue;
                    HashSet<string> ownColors;
                    if (ownGroup == null || !map.Symbols.TryGetValue((string)ownGroup.Attribute("data-s"), out ownColors) || ownColors.Count > 0)
                        problems.Add("symbol " + string.Join(" ", ownKeys) + " of map " + mapNumber + " is drawn outside any transparency group, though its symbol definition is drawn in palette colors");
                    else
                        NoteKeys(symbolsByKey, problems, ownKeys, mapNumber, (string)ownGroup.Attribute("data-s"));
                    continue;
                }
                string color = (string)transparencyGroup.Attribute("data-c");
                if (!map.Colors.Contains(color))
                    problems.Add("map " + mapNumber + " has a transparency group of palette color " + color + ", which its color table lacks");

                XElement symbolGroup = element.Ancestors(Svg + "g").TakeWhile(group => group != transparencyGroup).FirstOrDefault(group => group.Attribute("data-s") != null);
                if (symbolGroup == null) {
                    problems.Add("a " + element.Name.LocalName + " of palette color " + color + " of map " + mapNumber + " is in no group of a symbol definition");
                    continue;
                }
                string symbol = (string)symbolGroup.Attribute("data-s");
                HashSet<string> symbolColors;
                if (!map.Symbols.TryGetValue(symbol, out symbolColors))
                    problems.Add("map " + mapNumber + " draws symbol definition " + symbol + ", which its symbol table lacks");
                else if (!symbolColors.Contains(color))
                    problems.Add("symbol definition " + symbol + " of map " + mapNumber + " is drawn in palette color " + color + ", which its record does not list");

                // Each element is in a palette color coverage, which carries the keys of its symbols: one, or each of an
                // area cluster whose pattern fill is written once.
                string[] keys = KeysOf(element, symbolGroup);
                if (keys == null) {
                    problems.Add("a " + element.Name.LocalName + " of symbol definition " + symbol + " of map " + mapNumber + " carries no symbol key");
                    continue;
                }
                NoteKeys(symbolsByKey, problems, keys, mapNumber, symbol);
            }

            // Every course object traces to its course and controls (CD-1, CD-2), and to the elements its symbols draw;
            // and every symbol of a course's map is a symbol of a described course object.
            foreach (MapRecord map in maps.Values.Where(record => record.Course != null)) {
                // A course, unless it is a page of all controls, and each of its controls carry their ids in the event's
                // .ppen file, and each control its kind as the file names it (CD-1).
                string courseId = (string)map.Course.Attribute("id");
                if (courseId != null && !PpenId.IsMatch(courseId))
                    problems.Add("map " + map.Number + "'s course has the id " + courseId);
                List<XElement> controls = map.Course.Elements(Npp + "control").ToList();
                for (int i = 0; i < controls.Count; ++i) {
                    string place = (i + 1).ToString(CultureInfo.InvariantCulture);
                    if ((string)controls[i].Attribute("n") != place)
                        problems.Add("control " + place + " of map " + map.Number + "'s course is numbered " + (string)controls[i].Attribute("n"));
                    if (!PpenId.IsMatch((string)controls[i].Attribute("id") ?? ""))
                        problems.Add("control " + place + " of map " + map.Number + "'s course has the id " + ((string)controls[i].Attribute("id") ?? "none"));
                    if (!ControlKinds.Contains((string)controls[i].Attribute("kind") ?? ""))
                        problems.Add("control " + place + " of map " + map.Number + "'s course is of the kind " + ((string)controls[i].Attribute("kind") ?? "none"));
                }
                List<string> keysOfMap = symbolsByKey.Keys.Where(mapAndKey => mapAndKey.Item1 == map.Number).Select(mapAndKey => mapAndKey.Item2).ToList();
                foreach (KeyValuePair<string, XElement> described in map.Objects) {
                    if (!keysOfMap.Any(key => CourseObjectKeyOf(key) == described.Key))
                        problems.Add("course object " + described.Key + " of map " + map.Number + " draws nothing of that map");
                    string kind = (string)described.Value.Attribute("kind");
                    string[] places = ((string)described.Value.Attribute("controls") ?? "").Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    int expected;
                    if (string.IsNullOrEmpty(kind))
                        problems.Add("course object " + described.Key + " of map " + map.Number + " has no kind");
                    else if (ControlsOfKind.TryGetValue(kind, out expected) && places.Length != expected)
                        problems.Add(string.Format(CultureInfo.InvariantCulture, "course object {0} of map {1}, a {2}, belongs to {3} controls", described.Key, map.Number, kind, places.Length));
                    foreach (string place in places) {
                        int index;
                        if (!int.TryParse(place, NumberStyles.None, CultureInfo.InvariantCulture, out index) || index < 1 || index > controls.Count)
                            problems.Add("course object " + described.Key + " of map " + map.Number + " belongs to control " + place + ", which its course lacks");
                    }
                }
                foreach (string key in keysOfMap) {
                    string objectKey = CourseObjectKeyOf(key);
                    if (objectKey == null || !map.Objects.ContainsKey(objectKey))
                        problems.Add("the symbol " + key + " of map " + map.Number + ", which draws a course, is of no described course object");
                }
            }

            return problems;
        }

        // The ids that uses within the definitions refer to: the definitions of the images and of the shared shapes that
        // the parts draw, which the definitions of the parts use. The pages' uses of parts lie outside the definitions.
        // root: the document's root.
        public static HashSet<string> IdsUsedWithinDefinitions(XElement root)
        {
            HashSet<string> ids = new HashSet<string>(StringComparer.Ordinal);
            foreach (XElement use in root.Descendants(Svg + "use").Where(use => use.Ancestors(Svg + "defs").Any())) {
                string href = (string)use.Attribute(XLink + "href");
                if (href != null && href.StartsWith("#", StringComparison.Ordinal))
                    ids.Add(href.Substring(1));
            }
            return ids;
        }

        // The symbol keys that an element carries, on itself or on a group around it within a group, or null when none
        // does (data-k).
        // element: the element.
        // within: the group within which the keys are looked for.
        private static string[] KeysOf(XElement element, XElement within)
        {
            foreach (XElement carrier in element.AncestorsAndSelf().TakeWhile(ancestor => ancestor != within)) {
                string keys = (string)carrier.Attribute("data-k");
                if (keys != null)
                    return keys.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            }
            return null;
        }

        // The key of the course object whose symbol a symbol key of a course's map names: the symbol key before its index,
        // or null when it has none.
        // symbolKey: the text form of the symbol key.
        private static string CourseObjectKeyOf(string symbolKey)
        {
            int slash = symbolKey.LastIndexOf('/');
            return slash > 0 ? symbolKey.Substring(0, slash) : null;
        }

        // Note the symbol keys of an element: each belongs to one symbol definition of its map.
        // symbolsByKey: the symbol definitions of the symbols seen so far, by map and key.
        // problems: the problems found so far.
        // keys: the symbol keys.
        // map: the number of the element's map.
        // symbol: the number of the element's symbol definition in the map's table.
        private static void NoteKeys(Dictionary<(string, string), string> symbolsByKey, List<string> problems, string[] keys, string map, string symbol)
        {
            foreach (string key in keys) {
                string seen;
                if (!symbolsByKey.TryGetValue((map, key), out seen))
                    symbolsByKey.Add((map, key), symbol);
                else if (seen != symbol)
                    problems.Add("the symbol " + key + " of map " + map + " is drawn as symbol definition " + seen + " and as symbol definition " + symbol);
            }
        }

        // The record of a map in the metadata: its number, the views of the pages that show it, the positions of its
        // palette colors, its symbol definitions with the palette colors each is drawn in, its course if it draws one, and
        // the course objects it describes, by key.
        private sealed class MapRecord
        {
            public readonly string Number;
            public readonly string[] Views;
            public readonly HashSet<string> Colors = new HashSet<string>(StringComparer.Ordinal);
            public readonly Dictionary<string, HashSet<string>> Symbols = new Dictionary<string, HashSet<string>>(StringComparer.Ordinal);
            public readonly XElement Course;
            public readonly Dictionary<string, XElement> Objects = new Dictionary<string, XElement>(StringComparer.Ordinal);

            // Read a map's record.
            // element: the npp:map element.
            public MapRecord(XElement element)
            {
                Number = (string)element.Attribute("n");
                Views = ((string)element.Attribute("views") ?? "").Split(' ', StringSplitOptions.RemoveEmptyEntries);
                foreach (XElement color in element.Elements(Npp + "color"))
                    Colors.Add((string)color.Attribute("position"));
                foreach (XElement symbol in element.Elements(Npp + "symbol"))
                    Symbols[(string)symbol.Attribute("n")] = new HashSet<string>(((string)symbol.Attribute("colors") ?? "").Split(' ', StringSplitOptions.RemoveEmptyEntries), StringComparer.Ordinal);
                Course = element.Element(Npp + "course");
                foreach (XElement courseObject in element.Elements(Npp + "object"))
                    Objects[(string)courseObject.Attribute("key")] = courseObject;
            }
        }

    }
}
