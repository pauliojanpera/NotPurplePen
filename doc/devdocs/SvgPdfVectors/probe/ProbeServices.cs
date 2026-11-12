using System;
using System.Threading;
using Map_SkiaStd;
using Microsoft.Extensions.DependencyInjection;
using PurplePen;
using PurplePen.Graphics2D;
using PurplePen.MapModel;
using PurplePen.PdfConverter;

namespace PdfVectorsProbe
{
    // The services the application's core draws, measures and converts PDF maps with, registered as AvPurplePen's
    // Program.cs registers them on Linux: Skia's bitmaps, text metrics and font subsets, and PdfConverter's PDFium,
    // with a loading status that waits for each conversion instead of showing a dialog.
    internal static class ProbeServices
    {
        private static bool registered;

        // Register the services, once.
        public static void Register()
        {
            if (registered)
                return;
            ServiceCollection services = new ServiceCollection();
            services.AddSingleton<IGraphicsBitmapLoader, SkiaBitmapGraphicsLoader>();
            services.AddSingleton<IBitmapGraphicsTargetProvider, SkiaBitmapGraphicsTargetProvider>();
            services.AddSingleton<IFontLoader>(SkiaFontLoader.Instance);
            services.AddSingleton<ITextMetrics, Skia_TextMetrics>();
            services.AddSingleton<IFileLoaderProvider, SkiaFileLoaderProvider>();
            services.AddSingleton<IFontSubsetProvider, SkiaFontSubsetProvider>();
            services.AddSingleton<IPdfConverter, PdfiumConverter>();
            services.AddSingleton<IPdfLoadingStatus, WaitingLoadingStatus>();
            Services.RegisterServiceProvider(services.BuildServiceProvider());
            registered = true;
        }

        // Converts a PDF into a PNG as the application's PdfConverterService does: through PdfConverter's PDFium.
        private sealed class PdfiumConverter : IPdfConverter
        {
            // Convert a PDF file into a PNG file at a resolution, as PdfConversion.Convert does.
            // dpi: the resolution, in dots per inch.
            // sourcePdfFileName: the PDF file.
            // destinationPngFileName: the PNG file to write.
            public void Convert(float dpi, string sourcePdfFileName, string destinationPngFileName)
            {
                PdfConversion.Convert(dpi, sourcePdfFileName, destinationPngFileName);
            }
        }

        // The loading status of a PDF map: instead of a modal dialog, it waits for the background conversion to end.
        private sealed class WaitingLoadingStatus : IPdfLoadingStatus
        {
            private readonly ManualResetEventSlim done = new ManualResetEventSlim(false);
            private bool succeeded;

            // Wait until the conversion of a file has completed, and say whether it succeeded.
            // fileName: the PDF file being converted.
            public bool ShowLoadingStatus(string fileName)
            {
                done.Wait();
                done.Reset();
                return succeeded;
            }

            // Record that the conversion has completed.
            // success: whether it succeeded.
            // errorMessage: why it failed, if it did.
            public void LoadingComplete(bool success, string errorMessage)
            {
                succeeded = success;
                if (!success)
                    Console.Error.WriteLine("PDF conversion failed: " + errorMessage);
                done.Set();
            }
        }
    }
}
