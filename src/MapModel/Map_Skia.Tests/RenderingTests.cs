using Map_SkiaStd;
using NUnit.Framework;
using PurplePen.MapModel;
using RenderingTestCases;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using TestingUtils;

namespace Map_Skia.Tests
{
    // Draws each test of the map rendering tests with Skia, and compares the images with their baselines.
    [TestFixture, Parallelizable(ParallelScope.Children)]
    public class RenderingTests
    {
        private const int MAX_PIXEL_DIFF = 0;


        static RenderingTests()
        {
            Uri uri = new Uri(typeof(RenderingTests).Assembly.Location);
            string executablePath = Path.GetDirectoryName(uri.LocalPath);
            string fontPath = Path.Combine(executablePath, "fonts");

            SkiaFontManager.AddFontFile("Roboto", SKFontStyleWeight.Normal, SKFontStyleWidth.Normal,    SKFontStyleSlant.Upright, Path.Combine(fontPath, "Roboto-Regular.ttf"));
            SkiaFontManager.AddFontFile("Roboto", SKFontStyleWeight.Bold,   SKFontStyleWidth.Normal,    SKFontStyleSlant.Upright, Path.Combine(fontPath, "Roboto-Bold.ttf"));
            SkiaFontManager.AddFontFile("Roboto", SKFontStyleWeight.Normal, SKFontStyleWidth.Normal,    SKFontStyleSlant.Italic,  Path.Combine(fontPath, "Roboto-Italic.ttf"));
            SkiaFontManager.AddFontFile("Roboto", SKFontStyleWeight.Bold,   SKFontStyleWidth.Normal,    SKFontStyleSlant.Italic,  Path.Combine(fontPath, "Roboto-BoldItalic.ttf"));
            SkiaFontManager.AddFontFile("Roboto", SKFontStyleWeight.Normal, SKFontStyleWidth.Condensed, SKFontStyleSlant.Upright, Path.Combine(fontPath, "RobotoCondensed-Regular.ttf"));
            SkiaFontManager.AddFontFile("Roboto", SKFontStyleWeight.Bold,   SKFontStyleWidth.Condensed, SKFontStyleSlant.Upright, Path.Combine(fontPath, "RobotoCondensed-Bold.ttf"));
            SkiaFontManager.AddFontFile("Roboto", SKFontStyleWeight.Normal, SKFontStyleWidth.Condensed, SKFontStyleSlant.Italic,  Path.Combine(fontPath, "RobotoCondensed-Italic.ttf"));
            SkiaFontManager.AddFontFile("Roboto", SKFontStyleWeight.Bold,   SKFontStyleWidth.Condensed, SKFontStyleSlant.Italic,  Path.Combine(fontPath, "RobotoCondensed-BoldItalic.ttf"));
        }

        // Make the tests that may run beside other tests.
        private static IEnumerable<TestCaseData> ParallelizableTests()
        {
            return MakeTestCases(MapRenderingTestCases.All.Where(test => !test.NonParallelizable));
        }

        // Make the tests that must not run beside other tests.
        private static IEnumerable<TestCaseData> NonParallelizableTests()
        {
            return MakeTestCases(MapRenderingTestCases.All.Where(test => test.NonParallelizable));
        }

        // Make NUnit test cases of map rendering tests, each named as the test is.
        private static IEnumerable<TestCaseData> MakeTestCases(IEnumerable<MapRenderingTestCase> tests)
        {
            return tests.Select(test => new TestCaseData(test).SetName(test.Name));
        }

        [TestCaseSource(nameof(ParallelizableTests))]
        public void Render(MapRenderingTestCase test)
        {
            VerifyChecks(test);
        }

        [TestCaseSource(nameof(NonParallelizableTests)), NonParallelizable]
        public void RenderAlone(MapRenderingTestCase test)
        {
            VerifyChecks(test);
        }

        // Verify the checks of a test, in order.
        private static void VerifyChecks(MapRenderingTestCase test)
        {
            foreach (MapRenderingCheck check in test.Checks) {
                bool ok = RenderingUtil.VerifyTestFile(check, MAX_PIXEL_DIFF);
                Assert.IsTrue(ok, string.Format("Rendering test {0} did not compare correctly.", check.DescriptorFileName));
            }
        }
    }
}
