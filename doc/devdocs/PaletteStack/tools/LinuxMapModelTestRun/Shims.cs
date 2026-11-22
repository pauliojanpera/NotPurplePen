using NUnit.Framework;
using PurplePen.MapModel;
using System;
using System.Drawing;

// Stands in for what only Windows has, for the test files of Test_MapModel that LinuxMapModelTestRun compiles.

namespace PurplePen.MapModel
{
    // Measures text with Skia, in place of GDI+.
    public class GDIPlus_TextMetrics : Skia_TextMetrics
    {
    }

    // Loads files with Skia, in place of GDI+.
    public class GDIPlus_FileLoader : Skia_FileLoader
    {
        public GDIPlus_FileLoader(string basePath) : base(basePath)
        {
        }
    }
}

namespace System.Drawing
{
    // A rendering that GDI+ makes on Windows. No rendering is made here.
    public sealed class Bitmap : IDisposable
    {
        public void Dispose()
        {
        }
    }
}

namespace PurplePen.MapModel.Tests
{
    // The renderings of the suite's comparisons, which GDI+ draws. A test that asks for one is skipped.
    public static class Rendering
    {
        public static Bitmap RenderBitmap(Map map, Size bitmapSize, RectangleF mapArea, RenderOptions renderOpts, float intensity)
        {
            Assert.Ignore("GDI+ draws the renderings of Test_MapModel, on Windows only.");
            return null;
        }

        public static Bitmap RenderAntiAliasBitmap(Map map, Size bitmapSize, RectangleF mapArea, RenderOptions renderOpts, float intensity)
        {
            return RenderBitmap(map, bitmapSize, mapArea, renderOpts, intensity);
        }
    }
}

namespace TestingUtils
{
    // Compares a rendering with its baseline. Never reached, since no rendering is made here.
    public static class BitmapTestUtil
    {
        public static void CompareBitmapBaseline(Bitmap bitmap, string baseline, int maxPixelDiff)
        {
            Assert.Ignore("GDI+ draws the renderings of Test_MapModel, on Windows only.");
        }
    }
}
