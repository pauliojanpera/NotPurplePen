using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using PurplePen;

namespace PdfVectorsProbe
{
    // The SVG export of an event whose map is a PDF, as the application's Create SVG Files command runs it: the
    // controller loads the event, and CourseSvg writes a file for each course part, with the export's default settings.
    internal static class ExportCommand
    {
        // Export every course of an event into a folder, and print the files' count, sizes and the seconds taken.
        // eventFile: the event's .ppen file.
        // folder: the folder to write into, emptied first.
        public static int Run(string eventFile, string folder)
        {
            ProbeServices.Register();
            if (Directory.Exists(folder))
                Directory.Delete(folder, true);
            Directory.CreateDirectory(folder);

            QuietUserInterface ui = new QuietUserInterface();
            Controller controller = new Controller(ui);
            Stopwatch loading = Stopwatch.StartNew();
            bool loaded = controller.LoadInitialFile(Path.GetFullPath(eventFile), false).GetAwaiter().GetResult();
            loading.Stop();
            if (!loaded || controller.MapDisplay.MapType != MapType.PDF) {
                Console.Error.WriteLine("The event does not load with a PDF map: " + string.Join(" ", ui.Messages));
                return 1;
            }
            Id<Course>[] courseIds = new Id<Course>[] { Id<Course>.None }.Concat(QueryEvent.SortedCourseIds(controller.GetEventDB(), true)).ToArray();
            CourseSvgSettings settings = new CourseSvgSettings {
                mapDirectory = false, fileDirectory = false, outputDirectory = Path.GetFullPath(folder),
                CourseIds = courseIds, AllCourses = false, ShowProgressDialog = false,
            };
            Stopwatch exporting = Stopwatch.StartNew();
            bool exported = controller.CreateCourseSvgs(settings);
            exporting.Stop();
            if (!exported) {
                Console.Error.WriteLine("The export failed: " + string.Join(" ", ui.Messages));
                return 1;
            }
            FileInfo[] files = new DirectoryInfo(folder).GetFiles().OrderBy(file => file.Name, StringComparer.Ordinal).ToArray();
            Console.WriteLine("{\"command\":\"export\",\"event\":" + Program.Json(eventFile) +
                              ",\"load_seconds\":" + loading.Elapsed.TotalSeconds.ToString("0.000", CultureInfo.InvariantCulture) +
                              ",\"export_seconds\":" + exporting.Elapsed.TotalSeconds.ToString("0.000", CultureInfo.InvariantCulture) +
                              ",\"files\":" + files.Length + ",\"bytes\":" + files.Sum(file => file.Length) +
                              ",\"largest\":" + (files.Length == 0 ? 0 : files.Max(file => file.Length)) +
                              ",\"names\":[" + string.Join(",", files.Select(file => Program.Json(file.Name))) + "]}");
            return 0;
        }

        // A user interface that answers every question with its default and keeps the messages, as the SVG tests' does.
        private sealed class QuietUserInterface : IUserInterface
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
