using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NUnit.Framework;
using PurplePen;
using PurplePen.MapModel;

namespace Map_Svg.Tests
{
    // Exports reference events through the application's SVG export and compares each file's text with a baseline
    // committed in src/TestFiles/svg_export (V-2), so that a change to the written SVG shows in review; the renderings
    // of the comparison decide whether the drawing is right. A baseline holds the file's text with the data of each
    // embedded font and bitmap replaced by a hash of it, which keeps it small and still changes when the data does.
    // Each event is exported twice and must come out byte for byte the same (V-4), and the baselines are the same for
    // every platform, so the runs on Windows and on Linux check that the export is the same on both. A missing or
    // changed file is written to the loop's results under svg_export/, from where it is committed; and each file's
    // size, plain and gzip-compressed, to export-sizes.tsv there, which the loop's summary reports (V-5). The culture
    // is fixed too, as an event without paper of its own prints on the paper its culture's region uses.
    [TestFixture, NonParallelizable, SetCulture("en-US"), SetUICulture("en-US")]
    public class CourseSvgBaselineTests
    {
        // The reference events, each with the name its baselines go under, its file, the kind of map it is drawn on,
        // the courses exported, or none for every course of the event and its page of all controls, and whether its
        // course is drawn in upper and lower purple; and what it covers.
        internal static readonly TestCaseData[] Events = {
            // Courses with map exchanges, and no map.
            Event("mapexchange1", "controller/mapexchange1.ppen", MapType.None, new int[0], false),
            // An OCAD map with overprinted colors, and the course's purple blended.
            Event("Overprint test", "courseprinting/Overprint test.ppen", MapType.OCAD, new int[0], false),
            // The same in upper and lower purple, so that each page draws the map in two parts, with the course's lower
            // layer between them (L-2, L-3).
            Event("Overprint test lower purple", "courseprinting/Overprint test.ppen", MapType.OCAD, new int[0], true),
            // A bitmap map, which the files embed.
            Event("Lincoln Park", "controller/Lincoln Park.ppen", MapType.Bitmap, new int[0], false),
            // An OpenMapper map.
            Event("printarea2", "modes/printarea2.ppen", MapType.OCAD, new int[] { 1 }, false),
            // An OCAD map with area patterns, multi-colored point symbols and text along lines; one course, as each page
            // of it writes the whole map, of almost 2 MB.
            Event("Madrona Permanent11", "controller/Madrona Permanent11.ppen", MapType.OCAD, new int[] { 1 }, false),
            // A georeferenced OCAD map, in the British National Grid.
            Event("testproj2", "gpx/testproj2.ppen", MapType.OCAD, new int[0], false),
        };

        // A reference event, as a test case named by the name its baselines go under.
        // name: the name.
        // eventFile: the event's file, among the application's test files.
        // mapType: the kind of map the event is drawn on.
        // courses: the ids of the courses to export, or none for every course of the event and its page of all controls.
        // lowerPurpleLayer: whether to draw the course in upper and lower purple.
        private static TestCaseData Event(string name, string eventFile, MapType mapType, int[] courses, bool lowerPurpleLayer)
        {
            return new TestCaseData(name, eventFile, mapType, courses, lowerPurpleLayer).SetName(name);
        }

        // The data of an embedded font or bitmap, and the type it is named with.
        private static readonly Regex EmbeddedData = new Regex("data:([A-Za-z0-9.+/-]+);base64,([A-Za-z0-9+/=]+)", RegexOptions.CultureInvariant);

        // Export courses of an event twice, and compare each file with the other export's and with its baseline.
        // eventName: the name the event's baselines go under.
        // eventFile: the event's file, among the application's test files.
        // mapType: the kind of map the event is drawn on.
        // courses: the ids of the courses to export, or none for every course of the event, its page of all controls
        //          first.
        // lowerPurpleLayer: whether to draw the course in upper and lower purple.
        [TestCaseSource(nameof(Events))]
        public async Task AnEventIsExportedAsItsBaselinesSay(string eventName, string eventFile, MapType mapType, int[] courses, bool lowerPurpleLayer)
        {
            Id<Course>[] courseIds = courses.Select(id => new Id<Course>(id)).ToArray();
            string first = await CourseExport.ExportAsync(eventFile, "baseline/" + eventName + "/first", mapType, lowerPurpleLayer, courseIds);
            string second = await CourseExport.ExportAsync(eventFile, "baseline/" + eventName + "/second", mapType, lowerPurpleLayer, courseIds);

            string[] files = CourseExport.SvgFilesIn(first);
            Assert.IsNotEmpty(files, "the export writes files");
            CollectionAssert.AreEqual(files, CourseExport.SvgFilesIn(second), "the same files each time");

            string baselineFolder = CourseExport.ApplicationTestFile("svg_export/" + eventName);
            List<string> problems = new List<string>();
            StringBuilder sizes = new StringBuilder();
            foreach (string file in files) {
                byte[] bytes = File.ReadAllBytes(Path.Combine(first, file));
                if (!bytes.AsSpan().SequenceEqual(File.ReadAllBytes(Path.Combine(second, file))))
                    problems.Add(file + " is not the same each time it is exported");
                sizes.Append(eventName).Append('\t').Append(file).Append('\t')
                     .Append(bytes.Length.ToString(CultureInfo.InvariantCulture)).Append('\t')
                     .Append(GzipSize(bytes).ToString(CultureInfo.InvariantCulture)).Append('\n');

                string baseline = BaselineText(Encoding.UTF8.GetString(bytes));
                string baselinePath = Path.Combine(baselineFolder, file + ".txt");
                string resultPath = Path.Combine("svg_export", eventName, file + ".txt");
                if (!File.Exists(baselinePath)) {
                    LoopResults.SaveText(resultPath, baseline);
                    problems.Add(file + " has no baseline; the loop's results hold one as " + resultPath.Replace('\\', '/'));
                }
                else {
                    string difference = FirstDifference(File.ReadAllText(baselinePath, Encoding.UTF8), baseline);
                    if (difference != null) {
                        LoopResults.SaveText(resultPath, baseline);
                        problems.Add(file + " differs from its baseline, " + difference + "; the loop's results hold the new text as " + resultPath.Replace('\\', '/'));
                    }
                }
            }

            if (Directory.Exists(baselineFolder)) {
                foreach (string baselinePath in Directory.GetFiles(baselineFolder, "*.svg.txt").OrderBy(path => path, StringComparer.Ordinal)) {
                    string file = Path.GetFileNameWithoutExtension(baselinePath);
                    if (!files.Contains(file))
                        problems.Add("the baseline of " + file + " is of a file the export no longer writes");
                }
            }

            AppendSizes(sizes.ToString());
            Assert.IsEmpty(problems, string.Join("\n", problems));
        }

        // The text a file's baseline holds: the file's text, with the data of each embedded font and bitmap replaced
        // by the SHA-256 hash of its base64 text.
        // text: the text of the file.
        public static string BaselineText(string text)
        {
            return EmbeddedData.Replace(text, match => {
                byte[] hash = SHA256.HashData(Encoding.ASCII.GetBytes(match.Groups[2].Value));
                return "data:" + match.Groups[1].Value + ";base64,sha256:" + Convert.ToHexString(hash).ToLowerInvariant();
            });
        }

        // Where two texts first differ, as the line and its two versions, cut short; or null when they are the same.
        // expected: the baseline's text.
        // actual: the text exported.
        private static string FirstDifference(string expected, string actual)
        {
            if (string.Equals(expected, actual, StringComparison.Ordinal))
                return null;
            string[] expectedLines = expected.Split('\n'), actualLines = actual.Split('\n');
            int line = 0;
            while (line < expectedLines.Length && line < actualLines.Length && string.Equals(expectedLines[line], actualLines[line], StringComparison.Ordinal))
                ++line;
            return string.Format(CultureInfo.InvariantCulture, "first at line {0}: \"{1}\" where the baseline has \"{2}\"", line + 1,
                                 Shorten(line < actualLines.Length ? actualLines[line] : "(the end)"), Shorten(line < expectedLines.Length ? expectedLines[line] : "(the end)"));
        }

        // A line cut to its first 120 characters.
        // line: the line.
        private static string Shorten(string line)
        {
            return line.Length <= 120 ? line : line.Substring(0, 120) + "...";
        }

        // The size of a file's bytes compressed with gzip, as an .svgz file holds them.
        // bytes: the file's bytes.
        private static long GzipSize(byte[] bytes)
        {
            using (MemoryStream compressed = new MemoryStream()) {
                using (GZipStream gzip = new GZipStream(compressed, CompressionLevel.Optimal, true))
                    gzip.Write(bytes, 0, bytes.Length);
                return compressed.Length;
            }
        }

        // Add lines to export-sizes.tsv in the loop's results, which gets its header when it is made.
        // lines: the lines, each the event, the file, its size and its size compressed, separated by tabs.
        private static void AppendSizes(string lines)
        {
            string path = Path.Combine(LoopResults.ResultsDirectory, "export-sizes.tsv");
            Directory.CreateDirectory(LoopResults.ResultsDirectory);
            if (!File.Exists(path))
                File.WriteAllText(path, "event\tfile\tbytes\tgzip bytes\n", new UTF8Encoding(false));
            File.AppendAllText(path, lines, new UTF8Encoding(false));
        }
    }
}
