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
    // Walks the files the SVG export writes of the baseline tests' reference events (V-7): every drawn element traces
    // through its run and symbol group to its object, symbol, color and map in the tables of the metadata; an object
    // drawn in several colors carries one number in all its runs, numbers are given in the order the objects are first
    // drawn, and ids are unique and every reference resolves; and every course object traces to its course and its
    // controls (M-5 to M-7, L-5, CD-1, CD-2). An object of the map's image or layout layer, which the map draws in
    // colors of the object's own rather than the color table's, sits under its symbol's group outside any run, and its
    // symbol's record names no colors.
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

        // Export courses of an event as the baseline tests do, and walk each file the export writes.
        // eventName: the name the event's baselines go under.
        // eventFile: the event's file, among the application's test files.
        // mapType: the kind of map the event is drawn on.
        // courses: the ids of the courses to export, or none for every course of the event and its page of all controls.
        // lowerPurpleLayer: whether to draw the course in upper and lower purple.
        [TestCaseSource(typeof(CourseSvgBaselineTests), nameof(CourseSvgBaselineTests.Events))]
        public async Task EveryElementTracesToItsMapAndEveryCourseObjectToItsControls(string eventName, string eventFile, MapType mapType, int[] courses, bool lowerPurpleLayer)
        {
            Id<Course>[] courseIds = courses.Select(id => new Id<Course>(id)).ToArray();
            string folder = await CourseExport.ExportAsync(eventFile, "structure/" + eventName, mapType, lowerPurpleLayer, courseIds);

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

            // The tables of each map (M-7).
            Dictionary<string, MapRecord> maps = new Dictionary<string, MapRecord>(StringComparer.Ordinal);
            foreach (XElement mapElement in root.Descendants(Npp + "map")) {
                MapRecord map = new MapRecord(mapElement);
                if (maps.ContainsKey(map.Number))
                    problems.Add("map " + map.Number + " has two records");
                else
                    maps.Add(map.Number, map);
            }

            // Every drawn element, outside the definitions of clips and the like.
            Dictionary<string, DrawnObject> objects = new Dictionary<string, DrawnObject>(StringComparer.Ordinal);
            List<string> numbersInOrder = new List<string>();
            foreach (XElement element in root.Descendants()) {
                if (element.Name.Namespace != Svg || !DrawingElements.Contains(element.Name.LocalName))
                    continue;
                if (element.Ancestors().Any(ancestor => Definitions.Contains(ancestor.Name.LocalName)))
                    continue;
                string number = (string)element.Attribute("data-o");

                XElement mapGroup = element.Ancestors(Svg + "g").FirstOrDefault(group => group.Attribute("data-m") != null);
                if (mapGroup == null) {
                    // Outside any map there are only the resources the maps refer to, each directly in the definitions
                    // with an id, and the pages' references to the parts of maps.
                    bool resource = element.Parent != null && element.Parent.Name == Svg + "defs" && element.Attribute("id") != null;
                    bool pageReference = element.Name.LocalName == "use" && !element.Ancestors(Svg + "defs").Any();
                    if (number != null || !(resource || pageReference))
                        problems.Add("a " + element.Name.LocalName + (number == null ? "" : " of object " + number) + " is drawn outside any map");
                    continue;
                }
                string mapNumber = (string)mapGroup.Attribute("data-m");
                MapRecord map;
                if (!maps.TryGetValue(mapNumber, out map)) {
                    problems.Add("map " + mapNumber + " is drawn but has no record");
                    continue;
                }

                // A bitmap base map or an image template sits under its map outside any run (M-5), and so does an object
                // of the image or layout layer, under its symbol's group.
                XElement run = element.Ancestors(Svg + "g").TakeWhile(group => group != mapGroup).FirstOrDefault(group => group.Attribute("data-c") != null);
                if (run == null) {
                    if (number != null) {
                        XElement ownGroup = element.Ancestors(Svg + "g").TakeWhile(group => group != mapGroup).FirstOrDefault(group => group.Attribute("data-s") != null);
                        HashSet<string> ownColors;
                        if (ownGroup == null || !map.Symbols.TryGetValue((string)ownGroup.Attribute("data-s"), out ownColors) || ownColors.Count > 0)
                            problems.Add("object " + number + " of map " + mapNumber + " is drawn outside any run, though its symbol is drawn in the color table's colors");
                        else
                            NoteObject(objects, numbersInOrder, problems, number, mapNumber, (string)ownGroup.Attribute("data-s"), null);
                    }
                    continue;
                }
                string color = (string)run.Attribute("data-c");
                if (!map.Colors.Contains(color))
                    problems.Add("map " + mapNumber + " has a run of color " + color + ", which its color table lacks");

                XElement symbolGroup = element.Ancestors(Svg + "g").TakeWhile(group => group != run).FirstOrDefault(group => group.Attribute("data-s") != null);
                if (symbolGroup == null) {
                    problems.Add("a " + element.Name.LocalName + " in run " + color + " of map " + mapNumber + " is in no symbol's group");
                    continue;
                }
                string symbol = (string)symbolGroup.Attribute("data-s");
                HashSet<string> symbolColors;
                if (!map.Symbols.TryGetValue(symbol, out symbolColors))
                    problems.Add("map " + mapNumber + " draws symbol " + symbol + ", which its symbol table lacks");
                else if (!symbolColors.Contains(color))
                    problems.Add("symbol " + symbol + " of map " + mapNumber + " is drawn in color " + color + ", which its record does not list");

                if (number == null) {
                    problems.Add("a " + element.Name.LocalName + " of symbol " + symbol + " of map " + mapNumber + " carries no object number");
                    continue;
                }
                NoteObject(objects, numbersInOrder, problems, number, mapNumber, symbol, color);
            }

            // Objects are numbered in the order they are first drawn (M-6).
            int previous = 0;
            foreach (string number in numbersInOrder) {
                int value;
                if (!int.TryParse(number, NumberStyles.None, CultureInfo.InvariantCulture, out value))
                    problems.Add("an object is numbered " + number);
                else if (value <= previous)
                    problems.Add(string.Format(CultureInfo.InvariantCulture, "object {0} is first drawn after object {1}", value, previous));
                else
                    previous = value;
            }

            // Every course object traces to its course and controls (CD-1, CD-2), and every object of a course's map
            // is described.
            foreach (MapRecord map in maps.Values.Where(record => record.Course != null)) {
                List<XElement> controls = map.Course.Elements(Npp + "control").ToList();
                for (int i = 0; i < controls.Count; ++i) {
                    if ((string)controls[i].Attribute("n") != (i + 1).ToString(CultureInfo.InvariantCulture))
                        problems.Add("control " + (i + 1).ToString(CultureInfo.InvariantCulture) + " of map " + map.Number + "'s course is numbered " + (string)controls[i].Attribute("n"));
                }
                foreach (KeyValuePair<string, XElement> described in map.Objects) {
                    DrawnObject drawn;
                    if (!objects.TryGetValue(described.Key, out drawn) || drawn.Map != map.Number)
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
                foreach (KeyValuePair<string, DrawnObject> drawn in objects.Where(pair => pair.Value.Map == map.Number)) {
                    if (!map.Objects.ContainsKey(drawn.Key))
                        problems.Add("object " + drawn.Key + " of map " + map.Number + ", which draws a course, is not described");
                }
            }

            return problems;
        }

        // Note an element of an object: the object's map, symbol and color, which must be the same map and symbol for
        // every element of the object.
        // objects: the objects seen so far, by number.
        // numbersInOrder: the objects' numbers, in the order they were first seen.
        // problems: the problems found so far.
        // number: the object's number.
        // map: the number of the element's map.
        // symbol: the number of the element's symbol in the map's table.
        // color: the position of the element's color in the map's table, or null for an element outside any run.
        private static void NoteObject(Dictionary<string, DrawnObject> objects, List<string> numbersInOrder, List<string> problems,
                                       string number, string map, string symbol, string color)
        {
            DrawnObject drawn;
            if (!objects.TryGetValue(number, out drawn)) {
                drawn = new DrawnObject(map, symbol);
                objects.Add(number, drawn);
                numbersInOrder.Add(number);
            }
            else if (drawn.Map != map || drawn.Symbol != symbol) {
                problems.Add("object " + number + " is drawn as symbol " + drawn.Symbol + " of map " + drawn.Map + " and as symbol " + symbol + " of map " + map);
            }
            if (color != null)
                drawn.Colors.Add(color);
        }

        // The record of a map in the metadata: its number, the positions of its colors, its symbols with the colors
        // each is drawn in, its course if it draws one, and the course objects it describes, by number.
        private sealed class MapRecord
        {
            public readonly string Number;
            public readonly HashSet<string> Colors = new HashSet<string>(StringComparer.Ordinal);
            public readonly Dictionary<string, HashSet<string>> Symbols = new Dictionary<string, HashSet<string>>(StringComparer.Ordinal);
            public readonly XElement Course;
            public readonly Dictionary<string, XElement> Objects = new Dictionary<string, XElement>(StringComparer.Ordinal);

            // Read a map's record.
            // element: the npp:map element.
            public MapRecord(XElement element)
            {
                Number = (string)element.Attribute("n");
                foreach (XElement color in element.Elements(Npp + "color"))
                    Colors.Add((string)color.Attribute("position"));
                foreach (XElement symbol in element.Elements(Npp + "symbol"))
                    Symbols[(string)symbol.Attribute("n")] = new HashSet<string>(((string)symbol.Attribute("colors") ?? "").Split(' ', StringSplitOptions.RemoveEmptyEntries), StringComparer.Ordinal);
                Course = element.Element(Npp + "course");
                foreach (XElement courseObject in element.Elements(Npp + "object"))
                    Objects[(string)courseObject.Attribute("n")] = courseObject;
            }
        }

        // An object as the drawing shows it: the map and symbol it is drawn as, and the colors it is drawn in.
        private sealed class DrawnObject
        {
            public readonly string Map;
            public readonly string Symbol;
            public readonly HashSet<string> Colors = new HashSet<string>(StringComparer.Ordinal);

            // Start an object's record.
            // map: the number of its map.
            // symbol: the number of its symbol in the map's table.
            public DrawnObject(string map, string symbol)
            {
                Map = map;
                Symbol = symbol;
            }
        }
    }
}
