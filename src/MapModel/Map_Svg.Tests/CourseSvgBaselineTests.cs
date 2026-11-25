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
        // the courses exported, or none for every course of the event and its page of all controls, whether its course
        // is drawn in upper and lower purple, which pages go in each file, and whether area patterns are drawn as
        // separate shapes; and what it covers.
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
            // A JPEG map and JPEG images, each embedded as its data where the browsers draw that as the application draws
            // the image, and encoded anew by Skia otherwise, alike on every platform (B-3, F-2).
            Event("JPEG data", "svg_jpeg/JPEG data.ppen", MapType.Bitmap, new int[0], false),
            // An OpenMapper map.
            Event("printarea2", "modes/printarea2.ppen", MapType.OCAD, new int[] { 1 }, false),
            // An OCAD map with area patterns, multi-colored point symbols and text along lines; one course, as each page
            // of it writes the whole map, of almost 2 MB.
            Event("Madrona Permanent11", "controller/Madrona Permanent11.ppen", MapType.OCAD, new int[] { 1 }, false),
            // A georeferenced OCAD map, in the British National Grid.
            Event("testproj2", "gpx/testproj2.ppen", MapType.OCAD, new int[0], false),
            // Both pages of the map in two parts in one file, which defines the map's parts once for both (L-4, L-6).
            Event("Overprint test lower purple in one file", "courseprinting/Overprint test.ppen", MapType.OCAD, new int[0], true,
                  CoursePdfSettings.PdfFileCreation.SingleFile),
            // A course with map exchanges in a file of its own, a page for each of its four parts (L-6, L-7).
            Event("mapexchange1 Course 5 in a file", "controller/mapexchange1.ppen", MapType.None, new int[] { 6 }, false,
                  CoursePdfSettings.PdfFileCreation.FilePerCourse),
            // The map with area patterns, drawn as separate shapes: each element written as PDF export draws it, as
            // the export wrote every pattern before it wrote pattern fills (S-7).
            Event("Madrona Permanent11 with area patterns as separate shapes", "controller/Madrona Permanent11.ppen", MapType.OCAD, new int[] { 1 }, false,
                  CoursePdfSettings.PdfFileCreation.FilePerCoursePart, true),
        };

        // A reference event, as a test case named by the name its baselines go under.
        // name: the name.
        // eventFile: the event's file, among the application's test files.
        // mapType: the kind of map the event is drawn on.
        // courses: the ids of the courses to export, or none for every course of the event and its page of all controls.
        // lowerPurpleLayer: whether to draw the course in upper and lower purple.
        // fileCreation: which pages go in each file.
        // drawAreaPatternsAsSeparateShapes: whether every area pattern is drawn as separate shapes, element by element,
        // rather than as a pattern fill.
        private static TestCaseData Event(string name, string eventFile, MapType mapType, int[] courses, bool lowerPurpleLayer,
                                          CoursePdfSettings.PdfFileCreation fileCreation = CoursePdfSettings.PdfFileCreation.FilePerCoursePart,
                                          bool drawAreaPatternsAsSeparateShapes = false)
        {
            return new TestCaseData(name, eventFile, mapType, courses, lowerPurpleLayer, fileCreation, drawAreaPatternsAsSeparateShapes).SetName(name);
        }

        // The data of an embedded font, bitmap or color profile, and the type it is named with.
        private static readonly Regex EmbeddedData = new Regex("data:([A-Za-z0-9.+/-]+);base64,([A-Za-z0-9+/=]+)", RegexOptions.CultureInvariant);

        // Export courses of an event twice, and compare each file with the other export's and with its baseline.
        // eventName: the name the event's baselines go under.
        // eventFile: the event's file, among the application's test files.
        // mapType: the kind of map the event is drawn on.
        // courses: the ids of the courses to export, or none for every course of the event, its page of all controls
        //          first.
        // lowerPurpleLayer: whether to draw the course in upper and lower purple.
        // fileCreation: which pages go in each file.
        // drawAreaPatternsAsSeparateShapes: whether every area pattern is drawn as separate shapes, element by element,
        // rather than as a pattern fill.
        [TestCaseSource(nameof(Events))]
        public async Task AnEventIsExportedAsItsBaselinesSay(string eventName, string eventFile, MapType mapType, int[] courses, bool lowerPurpleLayer,
                                                             CoursePdfSettings.PdfFileCreation fileCreation, bool drawAreaPatternsAsSeparateShapes)
        {
            Id<Course>[] courseIds = courses.Select(id => new Id<Course>(id)).ToArray();
            string first = await CourseExport.ExportAsync(eventFile, "baseline/" + eventName + "/first", mapType, lowerPurpleLayer, courseIds, fileCreation,
                                                          drawAreaPatternsAsSeparateShapes);
            string second = await CourseExport.ExportAsync(eventFile, "baseline/" + eventName + "/second", mapType, lowerPurpleLayer, courseIds, fileCreation,
                                                           drawAreaPatternsAsSeparateShapes);

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

        // Export mapexchange1's Course 5 into a file of its own, plain and twice compressed, and check that each compressed
        // file is the plain one's bytes in one gzip member whose header names no file name, no time and no operating
        // system, the same each time and on every platform: the SHA-256 of its bytes is its baseline's, which the runs on
        // Windows and on Linux both check (S-10, F-2, V-4).
        [Test]
        public async Task ACompressedFileIsThePlainOneInAGzipMemberTheSameOnEveryPlatform()
        {
            const string eventName = "mapexchange1 Course 5 compressed";
            byte[] fixedHeader = { 0x1f, 0x8b, 8, 0, 0, 0, 0, 0, 0, 255 };
            Id<Course>[] courseIds = { new Id<Course>(6) };
            string[] folders = new string[3];
            for (int export = 0; export < folders.Length; ++export) {
                folders[export] = await CourseExport.ExportAsync("controller/mapexchange1.ppen", "baseline/" + eventName + "/" + export.ToString(CultureInfo.InvariantCulture),
                                                                 MapType.None, false, courseIds, CoursePdfSettings.PdfFileCreation.FilePerCourse, false, export > 0);
            }

            string[] plainFiles = CourseExport.SvgFilesIn(folders[0]);
            string[] compressedFiles = Directory.GetFiles(folders[1], "*.svgz").Select(Path.GetFileName).OrderBy(file => file, StringComparer.Ordinal).ToArray();
            CollectionAssert.AreEqual(plainFiles.Select(file => file + "z"), compressedFiles, "the plain files' names, as .svgz");

            List<string> problems = new List<string>();
            foreach (string file in compressedFiles) {
                byte[] compressed = File.ReadAllBytes(Path.Combine(folders[1], file));
                if (!compressed.AsSpan(0, fixedHeader.Length).SequenceEqual(fixedHeader))
                    problems.Add(file + " begins " + Convert.ToHexString(compressed, 0, fixedHeader.Length) + ", not the fixed header");
                if (!compressed.AsSpan().SequenceEqual(File.ReadAllBytes(Path.Combine(folders[2], file))))
                    problems.Add(file + " is not the same each time it is exported");
                using (MemoryStream decompressed = new MemoryStream()) {
                    using (GZipStream gzip = new GZipStream(new MemoryStream(compressed), CompressionMode.Decompress))
                        gzip.CopyTo(decompressed);
                    if (!decompressed.ToArray().AsSpan().SequenceEqual(File.ReadAllBytes(Path.Combine(folders[0], file.Substring(0, file.Length - 1)))))
                        problems.Add(file + " does not hold the plain file's bytes");
                }

                string hash = Convert.ToHexString(SHA256.HashData(compressed)).ToLowerInvariant() + "\n";
                string baselinePath = Path.Combine(CourseExport.ApplicationTestFile("svg_export/" + eventName), file + ".sha256");
                string resultPath = Path.Combine("svg_export", eventName, file + ".sha256");
                if (!File.Exists(baselinePath) || File.ReadAllText(baselinePath, Encoding.UTF8) != hash) {
                    LoopResults.SaveText(resultPath, hash);
                    problems.Add(file + "'s SHA-256 is " + hash.Trim() + ", not its baseline's; the loop's results hold it as " + resultPath.Replace('\\', '/'));
                }
            }
            Assert.IsEmpty(problems, string.Join("\n", problems));
        }

        // The text a file's baseline holds: the file's text, with the data of each embedded font, bitmap and color
        // profile replaced by the SHA-256 hash of its base64 text.
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

        // The size of a file's bytes compressed with gzip, as the export writes them into an .svgz file.
        // bytes: the file's bytes.
        private static long GzipSize(byte[] bytes)
        {
            return CourseSvg.Compress(bytes).Length;
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
