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
using TestingUtils;

namespace Map_Svg.Tests
{
    // Exports events through the application's SVG export, as its command does: the controller loads an event, and
    // CourseSvg writes a file for each page into a folder of its own. The services the application's core draws and
    // measures with are the Skia ones the application registers, with the subsets of the faces the documents embed.
    internal static class CourseExport
    {
        private static readonly object servicesLock = new object();
        private static bool servicesRegistered;

        // Register the services the application's core draws and measures with, once in a run.
        public static void RegisterServices()
        {
            lock (servicesLock) {
                if (servicesRegistered)
                    return;
                ServiceCollection services = new ServiceCollection();
                services.AddSingleton<IGraphicsBitmapLoader, SkiaBitmapGraphicsLoader>();
                services.AddSingleton<IBitmapGraphicsTargetProvider, SkiaBitmapGraphicsTargetProvider>();
                services.AddSingleton<IFontLoader>(SkiaFontLoader.Instance);
                services.AddSingleton<ITextMetrics, Skia_TextMetrics>();
                services.AddSingleton<IFileLoaderProvider, SkiaFileLoaderProvider>();
                services.AddSingleton<IFontSubsetProvider, SkiaFontSubsetProvider>();
                Services.RegisterServiceProvider(services.BuildServiceProvider());
                servicesRegistered = true;
            }
        }

        // Load an event and export courses of it as SVG files into a folder of their own, and return the folder.
        // eventFile: the event's file, among the application's test files.
        // name: the name of the folder within this run's.
        // mapType: the kind of map the event is drawn on, which must load, or MapType.None for an event without one.
        // courseIds: the courses to export, Id<Course>.None for the page of all controls; none for every course of the
        //            event, in the order the event sorts them, after its page of all controls.
        public static async Task<string> ExportAsync(string eventFile, string name, MapType mapType, params Id<Course>[] courseIds)
        {
            RegisterServices();
            string directory = Path.Combine(Path.GetTempPath(), "svg-export-tests-" + Environment.ProcessId.ToString(System.Globalization.CultureInfo.InvariantCulture), name);
            if (Directory.Exists(directory))
                Directory.Delete(directory, true);
            Directory.CreateDirectory(directory);

            RecordingUserInterface ui = new RecordingUserInterface();
            Controller controller = new Controller(ui);
            Assert.IsTrue(await controller.LoadInitialFile(ApplicationTestFile(eventFile), false), "loads: " + string.Join(" ", ui.Messages));
            Assert.AreEqual(mapType, controller.MapDisplay.MapType, "the map loads: " + controller.MapFileName + " " + string.Join(" ", ui.Messages));
            if (controller.LowerPurpleMapLayer != null)
                controller.MapDisplay.LowerPurpleMapLayer = controller.LowerPurpleMapLayer;

            if (courseIds.Length == 0)
                courseIds = new Id<Course>[] { Id<Course>.None }.Concat(QueryEvent.SortedCourseIds(controller.GetEventDB(), true)).ToArray();

            CourseSvgSettings settings = new CourseSvgSettings {
                mapDirectory = false, fileDirectory = false, outputDirectory = directory,
                CourseIds = courseIds, AllCourses = false, ShowProgressDialog = false, CropLargePrintArea = false,
            };
            Assert.IsTrue(controller.CreateCourseSvgs(settings), "exports: " + string.Join(" ", ui.Messages));
            return directory;
        }

        // The names of the SVG files in a folder, in ordinal order.
        // directory: the folder.
        public static string[] SvgFilesIn(string directory)
        {
            return Directory.GetFiles(directory, "*.svg").Select(Path.GetFileName).OrderBy(file => file, StringComparer.Ordinal).ToArray();
        }

        // The path of one of the application's test files, in src/TestFiles, beside the folder of MapModel's own, which
        // TestUtil finds.
        // name: the file's path within the folder, its parts separated by slashes.
        public static string ApplicationTestFile(string name)
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
