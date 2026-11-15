using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PurplePen;
using PurplePen.Graphics2D;
using PurplePen.MapModel;
using System;
using System.IO;

// Registers what PurplePenCore needs, as GlobalTestSetup does in the Windows suite, with the services that
// run on Linux.
[TestClass]
public class LinuxTestRunSetup
{
    [AssemblyInitialize]
    public static void Initialize(TestContext context)
    {
        UserSettings.Initialize(Path.Combine(AppContext.BaseDirectory, "linux-test-run-settings.xml"));

        ServiceCollection services = new ServiceCollection();
        services.AddSingleton<IGraphicsBitmapLoader, SkiaBitmapGraphicsLoader>();
        services.AddSingleton<IBitmapGraphicsTargetProvider, SkiaBitmapGraphicsTargetProvider>();
        services.AddSingleton<IFontLoader>(SkiaFontLoader.Instance);
        services.AddSingleton<ITextMetrics, Skia_TextMetrics>();
        services.AddSingleton<IFileLoaderProvider, SkiaFileLoaderProvider>();
        services.AddSingleton<IPdfWriter, PdfWriter>();
        services.AddSingleton<IPdfConverter, GhostscriptPdfConverter>();
        services.AddTransient<IPdfLoadingStatus, SilentPdfLoadingStatus>();
        Services.RegisterServiceProvider(services.BuildServiceProvider());
    }
}
