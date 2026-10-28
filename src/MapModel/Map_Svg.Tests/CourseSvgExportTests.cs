using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Map_SkiaStd;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using PurplePen;
using PurplePen.Graphics2D;
using PurplePen.MapModel;
using SkiaSharp;
using TestingUtils;

namespace Map_Svg.Tests
{
    // Exports events through the application's SVG export, as its command does, on whatever platform the tests run on
    // (A-1 to A-6, V-4): the controller loads an event, CourseSvg writes a file for each page, and the files come out
    // the same each time and draw in Chromium.
    [TestFixture, NonParallelizable]
    public class CourseSvgExportTests
    {
        // Register the services the application's core draws and measures with, the Skia ones the application
        // registers, with the subsets of the faces the documents embed.
        [OneTimeSetUp]
        public void RegisterServices()
        {
            ServiceCollection services = new ServiceCollection();
            services.AddSingleton<IGraphicsBitmapLoader, SkiaBitmapGraphicsLoader>();
            services.AddSingleton<IBitmapGraphicsTargetProvider, SkiaBitmapGraphicsTargetProvider>();
            services.AddSingleton<IFontLoader>(SkiaFontLoader.Instance);
            services.AddSingleton<ITextMetrics, Skia_TextMetrics>();
            services.AddSingleton<IFileLoaderProvider, SkiaFileLoaderProvider>();
            services.AddSingleton<IFontSubsetProvider, SkiaFontSubsetProvider>();
            Services.RegisterServiceProvider(services.BuildServiceProvider());
        }

        [Test]
        public async Task AnEventIsWrittenAsAFileForEachPageTheSameEachTime()
        {
            string first = await ExportAsync("courseprinting/Lord Hill Feb 2024 - Final.ppen", "first", new Id<Course>(2), new Id<Course>(3));
            string second = await ExportAsync("courseprinting/Lord Hill Feb 2024 - Final.ppen", "second", new Id<Course>(2), new Id<Course>(3));

            string[] files = Directory.GetFiles(first, "*.svg").Select(Path.GetFileName).OrderBy(name => name, StringComparer.Ordinal).ToArray();
            Assert.GreaterOrEqual(files.Length, 2, "a file for each course");
            CollectionAssert.AreEqual(files, Directory.GetFiles(second, "*.svg").Select(Path.GetFileName).OrderBy(name => name, StringComparer.Ordinal).ToArray());
            foreach (string file in files)
                CollectionAssert.AreEqual(File.ReadAllBytes(Path.Combine(first, file)), File.ReadAllBytes(Path.Combine(second, file)), "{0} the same each time", file);

            // A page draws in Chromium: its map, and its course in purple. The image has the
            // page's proportions, at 4 pixels to the millimetre.
            string document = File.ReadAllText(Path.Combine(first, files[0]));
            StringAssert.Contains("<npp:map n=\"1\" label=\"LordHill_ver16_2024Jan_scaled.omap\">", document);
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

        // Load an event and export courses of it as SVG files into a folder of their own, and return the folder.
        // eventFile: the event's file, among the application's test files.
        // name: the name of the folder within this run's.
        // courseIds: the courses to export.
        private static async Task<string> ExportAsync(string eventFile, string name, params Id<Course>[] courseIds)
        {
            string directory = Path.Combine(Path.GetTempPath(), "svg-export-tests-" + Environment.ProcessId.ToString(System.Globalization.CultureInfo.InvariantCulture), name);
            if (Directory.Exists(directory))
                Directory.Delete(directory, true);
            Directory.CreateDirectory(directory);

            RecordingUserInterface ui = new RecordingUserInterface();
            Controller controller = new Controller(ui);
            Assert.IsTrue(await controller.LoadInitialFile(ApplicationTestFile(eventFile), false), "loads: " + string.Join(" ", ui.Messages));
            Assert.AreEqual(MapType.OCAD, controller.MapDisplay.MapType, "the map loads: " + controller.MapFileName + " " + string.Join(" ", ui.Messages));
            if (controller.LowerPurpleMapLayer != null)
                controller.MapDisplay.LowerPurpleMapLayer = controller.LowerPurpleMapLayer;

            CourseSvgSettings settings = new CourseSvgSettings {
                mapDirectory = false, fileDirectory = false, outputDirectory = directory,
                CourseIds = courseIds, AllCourses = false, ShowProgressDialog = false, CropLargePrintArea = false,
            };
            Assert.IsTrue(controller.CreateCourseSvgs(settings), "exports: " + string.Join(" ", ui.Messages));
            return directory;
        }

        // The path of one of the application's test files, in src/TestFiles, beside the folder of MapModel's own, which
        // TestUtil finds.
        // name: the file's path within the folder, its parts separated by slashes.
        private static string ApplicationTestFile(string name)
        {
            string mapModelFolder = Path.GetDirectoryName(TestUtil.GetTestFileDirectory());
            return Path.GetFullPath(Path.Combine(mapModelFolder, "..", "TestFiles", name.Replace('/', Path.DirectorySeparatorChar)));
        }

        // The user interface of a controller the tests drive, which records every message and answers every question
        // with its default.
        private sealed class RecordingUserInterface : IUserInterface
        {
            // The messages shown.
            public readonly List<string> Messages = new List<string>();

            public void Initialize(Controller controller, SymbolDB symbolDB) { }
            public void QueueIdleEvent() { }
            public void PostDelayedAction(Action action) { action(); }
            public bool GetCurrentLocation(out PointF location, out float pixelSize) { location = PointF.Empty; pixelSize = 0.1F; return false; }
            public Task ErrorMessage(string message) { Messages.Add(message); return Task.CompletedTask; }
            public Task WarningMessage(string message) { Messages.Add(message); return Task.CompletedTask; }
            public Task InfoMessage(string message) { Messages.Add(message); return Task.CompletedTask; }
            public Task<bool> OKCancelMessage(string message, bool okDefault) { Messages.Add(message); return Task.FromResult(okDefault); }
            public Task<bool> YesNoQuestion(string message, bool yesDefault) { Messages.Add(message); return Task.FromResult(yesDefault); }
            public Task<YesNoCancel> YesNoCancelQuestion(string message, bool yesDefault) { Messages.Add(message); return Task.FromResult(yesDefault ? YesNoCancel.Yes : YesNoCancel.No); }
            public Task<YesNoCancel> MovingSharedControl(string controlCode, string otherCourses) { return Task.FromResult(YesNoCancel.Cancel); }
            public Task<bool> FindMissingMapFile(string missingMapFile) { Messages.Add("missing map " + missingMapFile); return Task.FromResult(false); }
            public void ShowTopologyView() { }
            public void ShowProgressDialog(bool knownDuration, Action onCancelPressed = null) { }
            public bool UpdateProgressDialog(string info, double fractionDone) { return false; }
            public void EndProgressDialog() { }
        }
    }
}
