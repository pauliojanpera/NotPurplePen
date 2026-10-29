// CreateCourseSvgSettingsTests.cs
//
// Tests that the Create SVG Files command keeps the settings chosen in its
// dialog until another map loads (A-6).

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;
using PurplePen;
using PurplePen.Graphics2D;
using PurplePen.ViewModels;

namespace PurplePenViewModels.Tests
{
    /// <summary>
    /// Tests that the Create SVG Files command seeds its dialog with the settings last chosen in it until another map
    /// loads. The main window's view model drives a controller of its own on an event created in a folder of the
    /// test's own, on a bitmap map that a stand-in loader reads as a blank bitmap, so that the map loads into the map
    /// display without a graphics library. A stand-in dialog service answers the dialogs, and the test updates the view
    /// model as the application does when it is idle.
    /// </summary>
    [TestFixture, NonParallelizable, SetCulture("en-US"), SetUICulture("en-US")]
    public class CreateCourseSvgSettingsTests
    {
        // The file prefix chosen in the dialog, which the next dialog is seeded with while the settings are kept.
        private const string ChosenPrefix = "chosen-";

        private string folder = null!;
        private StandInDialogService dialogs = null!;
        private MainWindowViewModel viewModel = null!;
        private Controller controller = null!;

        /// <summary>
        /// Creates an event on the bitmap map a.png, in a folder of the test's own that holds a second map, b.png, and
        /// updates the view model.
        /// </summary>
        [SetUp]
        public async Task Initialize()
        {
            folder = Path.Combine(Path.GetTempPath(), "PurplePenViewModels.Tests-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(folder);
            File.WriteAllBytes(Path.Combine(folder, "a.png"), Array.Empty<byte>());
            File.WriteAllBytes(Path.Combine(folder, "b.png"), Array.Empty<byte>());

            // The view model reads the user's settings. At full intensity the map display does not dim the bitmap,
            // which would need a graphics library.
            if (UserSettings.Current == null)
                UserSettings.Initialize(Path.Combine(TestContext.CurrentContext.WorkDirectory, "PurplePenSettings.json"));
            UserSettings.Current!.MapIntensity = 1;

            dialogs = new StandInDialogService();
            Services.RegisterServiceProvider(new StandInServices(dialogs));
            viewModel = new MainWindowViewModel();
            controller = new Controller(viewModel);
            Assert.That(await controller.InitialNewEvent(EventOn("a.png", "event.ppen")), Is.True);
            viewModel.UpdateStateOnIdle();
        }

        /// <summary>
        /// Deletes the test's folder.
        /// </summary>
        [TearDown]
        public void CleanUp()
        {
            Directory.Delete(folder, true);
        }

        /// <summary>The settings chosen seed the next dialog while the map stays.</summary>
        [Test]
        public async Task SettingsAreKeptWhileTheMapStays()
        {
            Assert.That(await RunCreateSvgFilesAsync(true), Is.EqualTo(""));
            controller.ForceChangeUpdate(true);
            viewModel.UpdateStateOnIdle();
            Assert.That(await RunCreateSvgFilesAsync(false), Is.EqualTo(ChosenPrefix));
        }

        /// <summary>The settings chosen are forgotten when the event's map changes to another, which loads.</summary>
        [Test]
        public async Task SettingsAreForgottenWhenTheMapChanges()
        {
            Assert.That(await RunCreateSvgFilesAsync(true), Is.EqualTo(""));
            controller.ChangeMapFile(MapType.Bitmap, Path.Combine(folder, "b.png"), 10000, 96);
            AssertMapLoaded("b.png");
            viewModel.UpdateStateOnIdle();
            Assert.That(await RunCreateSvgFilesAsync(false), Is.EqualTo(""));
        }

        /// <summary>The settings chosen are forgotten when another event loads, on another map.</summary>
        [Test]
        public async Task SettingsAreForgottenWhenAnotherEventLoadsOnAnotherMap()
        {
            Assert.That(await RunCreateSvgFilesAsync(true), Is.EqualTo(""));
            Assert.That(await controller.NewEvent(EventOn("b.png", "other.ppen")), Is.True);
            AssertMapLoaded("b.png");
            viewModel.UpdateStateOnIdle();
            Assert.That(await RunCreateSvgFilesAsync(false), Is.EqualTo(""));
        }

        // Run the Create SVG Files command, and return the file prefix its dialog was seeded with. When choose is
        // true, the dialog is confirmed with the prefix ChosenPrefix, writing into a folder of the test's own; else it
        // is cancelled.
        // choose: whether to confirm the dialog.
        private async Task<string> RunCreateSvgFilesAsync(bool choose)
        {
            string? seededPrefix = null;
            dialogs.Answer = dialog => {
                if (dialog is not CreateSvgCoursesDialogViewModel svgDialog)
                    return false;
                seededPrefix = svgDialog.FilePrefix;
                if (choose) {
                    svgDialog.FilePrefix = ChosenPrefix;
                    svgDialog.UseFileDirectory = false;
                    svgDialog.UseMapDirectory = false;
                    svgDialog.UseOtherDirectory = true;
                    svgDialog.OutputDirectory = Path.Combine(folder, "svg");
                }
                return choose;
            };
            await viewModel.CreateCourseSvgCommand.ExecuteAsync(null);
            Assert.That(seededPrefix, Is.Not.Null, "the Create SVG Files dialog is shown");
            return seededPrefix!;
        }

        // Check that the map display has loaded a map of the test's folder, as the controller loads the event's map
        // into it before the view model updates.
        // mapFile: the name of the map's file.
        private void AssertMapLoaded(string mapFile)
        {
            Assert.That(controller.MapDisplay.MapType, Is.EqualTo(MapType.Bitmap));
            Assert.That(controller.MapDisplay.FileName, Is.EqualTo(Path.Combine(folder, mapFile)));
        }

        // The information that creates an event in the test's folder, on a bitmap map there.
        // mapFile: the name of the map's file.
        // eventFile: the name of the event's file.
        private Controller.CreateEventInfo EventOn(string mapFile, string eventFile)
        {
            return new Controller.CreateEventInfo {
                title = "SVG settings",
                eventFileName = Path.Combine(folder, eventFile),
                mapType = MapType.Bitmap,
                mapFileName = Path.Combine(folder, mapFile),
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

            // Not used by the tests.
            public INonModalDialog<TViewModel> ShowOwnedDialog<TViewModel>(TViewModel viewModel, bool disableOwner) where TViewModel : class
            {
                throw new NotSupportedException();
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

        // Text metrics that know no face: an event without controls lays out no text.
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
