// ExportSettingsTests.cs
//
// Tests that the commands creating files from the event keep the settings chosen in their dialogs until another map
// loads.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using NUnit.Framework;
using PurplePen;
using PurplePen.Graphics2D;
using PurplePen.MapModel;
using PurplePen.ViewModels;

namespace PurplePenViewModels.Tests
{
    /// <summary>
    /// Tests that each command creating files from the event seeds its dialog with the settings last chosen in it until
    /// another map loads, or a bitmap map's resolution changes, and that the paper and margins chosen for the PDF of the
    /// descriptions or of the punch cards are kept even then. The main window's view model drives a controller of its
    /// own on an event created in a folder of the test's own. The maps load into the map display without a graphics
    /// library: a stand-in loader reads a bitmap map as a blank bitmap, and the OCAD maps, which the GPX and KML exports
    /// need for their real-world coordinates, are empty. A stand-in dialog service answers the dialogs, and the test
    /// updates the view model as the application does when it is idle.
    /// </summary>
    [TestFixture, NonParallelizable, SetCulture("en-US"), SetUICulture("en-US")]
    public class ExportSettingsTests
    {
        // The text chosen in a dialog that has one, which the next dialog is seeded with while the settings are kept.
        private const string ChosenText = "chosen-";

        // The box size chosen for the descriptions or the punch cards, in millimeters.
        private const decimal ChosenBoxSize = 7m;

        /// <summary>
        /// The exports whose settings belong to the event's map.
        /// </summary>
        public static readonly Export[] Exports = {
            new Export("OCAD", MapType.Bitmap, viewModel => viewModel.CreateOcadFilesCommand,
                       dialog => (dialog as CreateOcadFilesDialogViewModel)?.FilePrefix, ChosenText,
                       dialog => ((CreateOcadFilesDialogViewModel)dialog).FilePrefix = ChosenText),
            new Export("Image", MapType.Bitmap, viewModel => viewModel.CreateImageFilesCommand,
                       dialog => (dialog as CreateImageFilesDialogViewModel)?.FilePrefix, ChosenText,
                       dialog => ((CreateImageFilesDialogViewModel)dialog).FilePrefix = ChosenText),
            new Export("PDF", MapType.Bitmap, viewModel => viewModel.CreateCoursePdfCommand,
                       dialog => (dialog as CreatePdfCoursesDialogViewModel)?.FilePrefix, ChosenText,
                       dialog => ((CreatePdfCoursesDialogViewModel)dialog).FilePrefix = ChosenText),
            new Export("RouteGadget", MapType.Bitmap, viewModel => viewModel.CreateRouteGadgetFilesCommand,
                       dialog => (dialog as CreateRouteGadgetDialogViewModel)?.FileBaseName, ChosenText,
                       dialog => ((CreateRouteGadgetDialogViewModel)dialog).FileBaseName = ChosenText),
            new Export("GPX", MapType.OCAD, viewModel => viewModel.CreateGpxCommand,
                       dialog => (dialog as CreateGpxDialogViewModel)?.CodePrefix, ChosenText,
                       dialog => ((CreateGpxDialogViewModel)dialog).CodePrefix = ChosenText),
            new Export("KML", MapType.OCAD, viewModel => viewModel.CreateKmlFilesCommand,
                       dialog => (dialog as CreateKmlFilesDialogViewModel)?.FilePrefix, ChosenText,
                       dialog => ((CreateKmlFilesDialogViewModel)dialog).FilePrefix = ChosenText),
            new Export("DescriptionPDF", MapType.Bitmap, viewModel => viewModel.CreateDescriptionPdfCommand,
                       dialog => (dialog as PrintDescriptionsDialogViewModel)?.BoxSize, ChosenBoxSize,
                       dialog => {
                           PrintDescriptionsDialogViewModel descriptions = (PrintDescriptionsDialogViewModel)dialog;
                           descriptions.BoxSize = ChosenBoxSize;
                           descriptions.PaperSizeWithMargins = ChosenPaper();
                       },
                       dialog => (dialog as PrintDescriptionsDialogViewModel)?.PaperSizeWithMargins),
            new Export("PunchCardPDF", MapType.Bitmap, viewModel => viewModel.CreatePunchcardPdfCommand,
                       dialog => (dialog as PrintPunchesDialogViewModel)?.BoxSize, ChosenBoxSize,
                       dialog => {
                           PrintPunchesDialogViewModel punches = (PrintPunchesDialogViewModel)dialog;
                           punches.BoxSize = ChosenBoxSize;
                           punches.PaperSizeWithMargins = ChosenPaper();
                       },
                       dialog => (dialog as PrintPunchesDialogViewModel)?.PaperSizeWithMargins),
        };

        /// <summary>
        /// The exports that create files from a bitmap map.
        /// </summary>
        public static readonly Export[] BitmapExports = Exports.Where(export => export.MapType == MapType.Bitmap).ToArray();

        /// <summary>
        /// The exports whose dialogs choose the paper and margins of a PDF, which belong to no map.
        /// </summary>
        public static readonly Export[] PaperExports = Exports.Where(export => export.Paper != null).ToArray();

        private string folder = null!;
        private StandInDialogService dialogs = null!;
        private MainWindowViewModel viewModel = null!;
        private Controller controller = null!;

        /// <summary>
        /// Creates a folder of the test's own, holding two bitmap maps, a.png and b.png, and two empty OCAD maps with
        /// real-world coordinates, a.ocd and b.ocd.
        /// </summary>
        [SetUp]
        public void Initialize()
        {
            folder = Path.Combine(Path.GetTempPath(), "PurplePenViewModels.Tests-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(folder);
            File.WriteAllBytes(Path.Combine(folder, "a.png"), Array.Empty<byte>());
            File.WriteAllBytes(Path.Combine(folder, "b.png"), Array.Empty<byte>());
            WriteGeoreferencedMap(Path.Combine(folder, "a.ocd"));
            WriteGeoreferencedMap(Path.Combine(folder, "b.ocd"));

            // The view model reads the user's settings. At full intensity the map display does not dim the bitmap,
            // which would need a graphics library.
            if (UserSettings.Current == null)
                UserSettings.Initialize(Path.Combine(TestContext.CurrentContext.WorkDirectory, "PurplePenSettings.json"));
            UserSettings.Current!.MapIntensity = 1;

            dialogs = new StandInDialogService();
            Services.RegisterServiceProvider(new StandInServices(dialogs));
        }

        /// <summary>
        /// Deletes the test's folder.
        /// </summary>
        [TearDown]
        public void CleanUp()
        {
            controller?.Dispose();
            Directory.Delete(folder, true);
        }

        /// <summary>The settings chosen seed the next dialog while the map stays.</summary>
        /// <param name="export">The export whose dialog is shown.</param>
        [TestCaseSource(nameof(Exports))]
        public async Task SettingsAreKeptWhileTheMapStays(Export export)
        {
            await CreateEventAsync(export, "a");
            await ChooseAsync(export);
            controller.ForceChangeUpdate(true);
            viewModel.UpdateStateOnIdle();
            Assert.That(export.Value(await ShowAsync(export)), Is.EqualTo(export.Chosen));
        }

        /// <summary>The settings chosen are forgotten when the event's map changes to another, which loads.</summary>
        /// <param name="export">The export whose dialog is shown.</param>
        [TestCaseSource(nameof(Exports))]
        public async Task SettingsAreForgottenWhenTheMapChanges(Export export)
        {
            await CreateEventAsync(export, "a");
            await ChooseAsync(export);
            controller.ChangeMapFile(export.MapType, MapFile(export, "b"), 10000, 96);
            AssertMapLoaded(export, "b");
            viewModel.UpdateStateOnIdle();
            Assert.That(export.Value(await ShowAsync(export)), Is.Not.EqualTo(export.Chosen));
        }

        /// <summary>The settings chosen are forgotten when another event loads, on another map.</summary>
        /// <param name="export">The export whose dialog is shown.</param>
        [TestCaseSource(nameof(Exports))]
        public async Task SettingsAreForgottenWhenAnotherEventLoadsOnAnotherMap(Export export)
        {
            await CreateEventAsync(export, "a");
            await ChooseAsync(export);
            Assert.That(await controller.NewEvent(EventOn(export.MapType, MapFile(export, "b"), "other.ppen")), Is.True);
            AssertMapLoaded(export, "b");
            viewModel.UpdateStateOnIdle();
            Assert.That(export.Value(await ShowAsync(export)), Is.Not.EqualTo(export.Chosen));
        }

        /// <summary>The settings chosen are forgotten when the resolution of the event's bitmap map changes.</summary>
        /// <param name="export">The export whose dialog is shown.</param>
        [TestCaseSource(nameof(BitmapExports))]
        public async Task SettingsAreForgottenWhenTheBitmapResolutionChanges(Export export)
        {
            await CreateEventAsync(export, "a");
            await ChooseAsync(export);
            controller.ChangeMapFile(MapType.Bitmap, MapFile(export, "a"), 10000, 150);
            Assert.That(controller.MapDisplay.Dpi, Is.EqualTo(150));
            viewModel.UpdateStateOnIdle();
            Assert.That(export.Value(await ShowAsync(export)), Is.Not.EqualTo(export.Chosen));
        }

        /// <summary>
        /// The paper and margins chosen for the PDF of the descriptions or of the punch cards are kept when another
        /// event loads on another map, which forgets the other settings of the dialog.
        /// </summary>
        /// <param name="export">The export whose dialog is shown.</param>
        [TestCaseSource(nameof(PaperExports))]
        public async Task PaperIsKeptWhenAnotherEventLoadsOnAnotherMap(Export export)
        {
            await CreateEventAsync(export, "a");
            PrintingPaperSizeWithMargins chosenPaper = export.Paper!(await ChooseAsync(export))!;
            Assert.That(await controller.NewEvent(EventOn(export.MapType, MapFile(export, "b"), "other.ppen")), Is.True);
            viewModel.UpdateStateOnIdle();
            object dialog = await ShowAsync(export);
            Assert.That(export.Value(dialog), Is.Not.EqualTo(export.Chosen));
            Assert.That(export.Paper(dialog), Is.SameAs(chosenPaper));
        }

        // Create the view model, and a controller of its own on an event in the test's folder, on a map there that
        // the export can create files from, and update the view model.
        // export: the export, which names the kind of map.
        // map: the name of the map's file, without its extension.
        private async Task CreateEventAsync(Export export, string map)
        {
            viewModel = new MainWindowViewModel();
            controller = new Controller(viewModel);
            Assert.That(await controller.InitialNewEvent(EventOn(export.MapType, MapFile(export, map), "event.ppen")), Is.True);
            viewModel.UpdateStateOnIdle();
        }

        // Run the export's command, and confirm its dialog with the export's chosen values. Return the dialog's view
        // model. A new dialog must not be seeded with the value chosen, or the tests could not tell it from one seeded
        // with the settings kept.
        // export: the export.
        private async Task<object> ChooseAsync(Export export)
        {
            object? seeded = null;
            object dialog = await RunExportAsync(export, shown => {
                seeded = export.Value(shown);
                export.Choose(shown);
                return true;
            });
            Assert.That(seeded, Is.Not.EqualTo(export.Chosen), "a new dialog is seeded with another value");
            return dialog;
        }

        // Run the export's command, and cancel its dialog. Return the dialog's view model, as it was seeded.
        // export: the export.
        private Task<object> ShowAsync(Export export)
        {
            return RunExportAsync(export, shown => false);
        }

        // Run the export's command, answering its dialog with the function given. A file the command asks for is named
        // in the test's folder, and any other dialog, such as a message, is cancelled. Return the dialog's view model.
        // export: the export.
        // answer: answers the export's dialog, and returns whether it is confirmed.
        private async Task<object> RunExportAsync(Export export, Func<object, bool> answer)
        {
            object? exportDialog = null;
            dialogs.Answer = dialog => {
                if (dialog is FileSaveViewModel save) {
                    save.SelectedFile = Path.Combine(folder, Path.ChangeExtension("export", save.DefaultExtension));
                    return true;
                }
                if (export.Value(dialog) == null)
                    return false;
                exportDialog = dialog;
                return answer(dialog);
            };
            await export.Command(viewModel).ExecuteAsync(null);
            Assert.That(exportDialog, Is.Not.Null, "the export's dialog is shown");
            return exportDialog!;
        }

        // Check that the map display has loaded a map of the test's folder, as the controller loads the event's map
        // into it before the view model updates.
        // export: the export, which names the kind of map.
        // map: the name of the map's file, without its extension.
        private void AssertMapLoaded(Export export, string map)
        {
            Assert.That(controller.MapDisplay.MapType, Is.EqualTo(export.MapType));
            Assert.That(controller.MapDisplay.FileName, Is.EqualTo(MapFile(export, map)));
        }

        // The path of a map of the test's folder, of the kind the export creates files from.
        // export: the export.
        // map: the name of the map's file, without its extension.
        private string MapFile(Export export, string map)
        {
            return Path.Combine(folder, map + (export.MapType == MapType.OCAD ? ".ocd" : ".png"));
        }

        // The information that creates an event in the test's folder, on a map there.
        // mapType: the kind of map.
        // mapFile: the path of the map's file.
        // eventFile: the name of the event's file.
        private Controller.CreateEventInfo EventOn(MapType mapType, string mapFile, string eventFile)
        {
            return new Controller.CreateEventInfo {
                title = "Export settings",
                eventFileName = Path.Combine(folder, eventFile),
                mapType = mapType,
                mapFileName = mapFile,
                scale = 10000,
                allControlsPrintScale = 10000,
                dpi = 96,
                firstCode = 31,
                descriptionStandard = "2018",
                mapStandard = "2017",
                blend = PurpleColorBlend.Blend,
                printArea = new PrintArea { autoPrintArea = true, restrictToPageSize = true, pageWidth = 827, pageHeight = 1169 },
            };
        }

        // Write an empty OCAD map at 1:10000, whose real-world coordinates lie in UTM zone 10.
        // path: the map's file.
        private static void WriteGeoreferencedMap(string path)
        {
            Map map = new Map(new NoTextMetrics(), new NoFiles());
            using (map.Write()) {
                map.MapScale = 10000;
                map.RealWorldCoords = new RealWorldCoords {
                    RealWorldOn = true,
                    RealWorldOffsetX = 550000,
                    RealWorldOffsetY = 5270000,
                    RealWorldGridAndZone = 2010,
                };
            }
            InputOutput.WriteFile(path, map, new MapFileFormat(MapFileFormatKind.OCAD, 12));
        }

        // A paper and margins unlike the default ones, chosen for the PDF of the descriptions or of the punch cards.
        private static PrintingPaperSizeWithMargins ChosenPaper()
        {
            return new PrintingPaperSizeWithMargins(new PrintingPaperSize("A5", 583, 827), new PrintingMarginSize(33));
        }

        /// <summary>
        /// An export whose dialog the tests answer: its command, the kind of map it creates files from in the tests, and
        /// a value of its dialog, which tells whether the dialog was seeded with the settings chosen before.
        /// </summary>
        public sealed class Export
        {
            /// <summary>The export's name, which names its test cases.</summary>
            public string Name { get; }

            /// <summary>The kind of map the export creates files from in the tests.</summary>
            public MapType MapType { get; }

            /// <summary>The export's command on the main window's view model.</summary>
            public Func<MainWindowViewModel, IAsyncRelayCommand> Command { get; }

            /// <summary>The value of a dialog, or null when the dialog is not the export's.</summary>
            public Func<object, object?> Value { get; }

            /// <summary>The value chosen in the export's dialog.</summary>
            public object Chosen { get; }

            /// <summary>Choose the value, and any paper and margins, in the export's dialog.</summary>
            public Action<object> Choose { get; }

            /// <summary>
            /// The paper and margins of the export's dialog, or null when the export does not choose them.
            /// </summary>
            public Func<object, PrintingPaperSizeWithMargins?>? Paper { get; }

            /// <summary>
            /// Describes an export.
            /// </summary>
            /// <param name="name">The export's name.</param>
            /// <param name="mapType">The kind of map the export creates files from in the tests.</param>
            /// <param name="command">The export's command on the main window's view model.</param>
            /// <param name="value">The value of a dialog, or null when the dialog is not the export's.</param>
            /// <param name="chosen">The value chosen in the export's dialog.</param>
            /// <param name="choose">Choose the value, and any paper and margins, in the export's dialog.</param>
            /// <param name="paper">The paper and margins of the export's dialog, when it chooses them.</param>
            public Export(string name, MapType mapType, Func<MainWindowViewModel, IAsyncRelayCommand> command,
                          Func<object, object?> value, object chosen, Action<object> choose,
                          Func<object, PrintingPaperSizeWithMargins?>? paper = null)
            {
                Name = name;
                MapType = mapType;
                Command = command;
                Value = value;
                Chosen = chosen;
                Choose = choose;
                Paper = paper;
            }

            /// <summary>The export's name, which NUnit gives its test cases.</summary>
            public override string ToString()
            {
                return Name;
            }
        }

        // The services the controller and the view model ask for, each a stand-in.
        private sealed class StandInServices : IServiceProvider
        {
            private readonly Dictionary<Type, object> services;

            // Create the services.
            // dialogs: the dialog service.
            public StandInServices(IDialogService dialogs)
            {
                services = new Dictionary<Type, object> {
                    { typeof(IDialogService), dialogs },
                    { typeof(IGraphicsBitmapLoader), new BlankBitmapLoader() },
                    { typeof(ITextMetrics), new NoTextMetrics() },
                    { typeof(IFileLoaderProvider), new NoFiles() },
                    { typeof(IApplicationIdleService), new NoIdleEvents() },
                    { typeof(IEventDispatcherService), new NoEventDispatching() },
                };
            }

            // Get a service, or null when there is no stand-in for it.
            // serviceType: the type of the service.
            public object? GetService(Type serviceType)
            {
                return services.TryGetValue(serviceType, out object? service) ? service : null;
            }
        }

        // A dialog service that answers each dialog with the test's function, without showing it.
        private sealed class StandInDialogService : IDialogService
        {
            // How a dialog is answered: the function may change the dialog's view model, and returns whether the
            // dialog is confirmed. Without one, every dialog is cancelled.
            public Func<object, bool>? Answer;

            // Answer a dialog.
            // viewModel: the dialog's view model.
            public Task<bool> ShowDialogAsync<TViewModel>(TViewModel viewModel) where TViewModel : class
            {
                return Task.FromResult(Answer != null && Answer(viewModel));
            }

            // Show a dialog that stays open until it is closed, such as the progress of creating files.
            // viewModel: the dialog's view model.
            // disableOwner: not used.
            public INonModalDialog<TViewModel> ShowOwnedDialog<TViewModel>(TViewModel viewModel, bool disableOwner) where TViewModel : class
            {
                return new OpenDialog<TViewModel>(viewModel);
            }
        }

        // A dialog shown by ShowOwnedDialog, open until it is closed.
        private sealed class OpenDialog<TViewModel> : INonModalDialog<TViewModel> where TViewModel : class
        {
            private readonly TaskCompletionSource closed = new TaskCompletionSource();

            // Open the dialog.
            // viewModel: the dialog's view model.
            public OpenDialog(TViewModel viewModel)
            {
                ViewModel = viewModel;
            }

            public TViewModel ViewModel { get; }
            public Task ClosedTask => closed.Task;
            public bool ClosedProgrammatically { get; private set; }

            // Close the dialog, if it is open.
            public void Close()
            {
                if (closed.TrySetResult())
                    ClosedProgrammatically = true;
            }
        }

        // A bitmap loader that reads every file as a blank bitmap.
        private sealed class BlankBitmapLoader : IGraphicsBitmapLoader
        {
            // Read a file as a blank bitmap.
            // stream: the file's content, which is not read.
            public IGraphicsBitmap ReadBitmapFromStream(Stream stream)
            {
                return new BlankBitmap();
            }

            // Create a blank bitmap.
            // width, height, initialColor: not used; the bitmap is the stand-in's own size.
            public IGraphicsBitmap CreateEmptyBitmap(int width, int height, Color? initialColor = null)
            {
                return new BlankBitmap();
            }

            // Nothing to dispose of.
            public void Dispose()
            {
            }
        }

        // A white bitmap of 100 by 100 pixels at 96 dots per inch, which cannot be drawn on, cropped or written.
        private sealed class BlankBitmap : IGraphicsBitmap
        {
            public bool Disposed { get; private set; }
            public int PixelWidth => 100;
            public int PixelHeight => 100;
            public bool MustCopyBitsForGraphicsTarget => false;
            public double HorizontalResolution { get; set; } = 96;
            public double VerticalResolution { get; set; } = 96;

            // The format the bitmap was read from.
            public GraphicsBitmapFormat GetOriginalFormat()
            {
                return GraphicsBitmapFormat.PNG;
            }

            // Get a pixel, which is white.
            // x, y: the pixel.
            public Color GetPixel(int x, int y)
            {
                return Color.White;
            }

            // Not used by the tests.
            public IGraphicsBitmap Crop(int x, int y, int width, int height)
            {
                throw new NotSupportedException();
            }

            // Not used by the tests.
            public bool WriteToStream(GraphicsBitmapFormat format, Stream stream, int quality)
            {
                throw new NotSupportedException();
            }

            // Not used by the tests.
            public IBitmapGraphicsTarget GetGraphicsTarget(bool copyBits, IColorConverter? colorConverter = null)
            {
                throw new NotSupportedException();
            }

            // Mark the bitmap disposed of.
            public void Dispose()
            {
                Disposed = true;
            }
        }

        // Text metrics that know no face: the events lay out no text, and the OCAD maps have none.
        private sealed class NoTextMetrics : ITextMetrics
        {
            // Not used by the tests.
            public ITextFaceMetrics GetTextFaceMetrics(string familyName, float emHeight, TextEffects effects)
            {
                throw new NotSupportedException();
            }

            // No face is installed.
            // familyName: the face's family.
            public bool TextFaceIsInstalled(string familyName)
            {
                return false;
            }

            // Nothing to dispose of.
            public void Dispose()
            {
            }
        }

        // A file loader for maps that reference no other file, as the OCAD maps of the tests.
        private sealed class NoFiles : IFileLoaderProvider, IFileLoader
        {
            // The loader for any folder.
            // path: the folder.
            public IFileLoader GetFileLoaderForDirectory(string path)
            {
                return this;
            }

            // Not used by the tests.
            public FileKind CheckFileKind(string path)
            {
                throw new NotSupportedException();
            }

            // Not used by the tests.
            public IGraphicsBitmap LoadBitmap(string path, bool isTemplate)
            {
                throw new NotSupportedException();
            }

            // Not used by the tests.
            public IGraphicsBitmap LoadBitmapFromData(byte[] data)
            {
                throw new NotSupportedException();
            }

            // Not used by the tests.
            public Map LoadMap(string path, Map referencingMap)
            {
                throw new NotSupportedException();
            }

            // Not used by the tests.
            public string SearchForFile(string path)
            {
                throw new NotSupportedException();
            }
        }

        // An idle service that queues no idle event: the tests update the view model themselves.
        private sealed class NoIdleEvents : IApplicationIdleService
        {
            // Queue nothing.
            public void QueueIdleEvent()
            {
            }
        }

        // An event dispatcher that runs nothing posted to it, such as a delayed error message.
        private sealed class NoEventDispatching : IEventDispatcherService
        {
            // Drop an action.
            // action: the action.
            public void PostMessage(Action action)
            {
            }

            // There is nothing pending.
            public void ProcessPendingMessages()
            {
            }
        }
    }
}
