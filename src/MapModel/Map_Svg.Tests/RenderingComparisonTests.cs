using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using PurplePen.Graphics2D;
using PurplePen.MapModel;
using SkiaSharp;
using TestingUtils;

namespace Map_Svg.Tests
{
    // The rendering comparison: draws each case through the SVG target, renders the document in Chromium, and
    // compares the image with the case's reference image. A case that makes a call the target does not fully
    // implement, or one it cannot honor, is pending, and names the calls it still needs.
    [TestFixture, Parallelizable(ParallelScope.Children)]
    public class RenderingComparisonTests
    {
        // How much a color channel of a pixel may differ from the reference image.
        private const int MaxPixelDifference = 0;

        // Make the cases of the drawing cases.
        private static IEnumerable<ComparisonCase> DrawingCases()
        {
            return ComparisonCase.MakeDrawingCases();
        }

        // Make the cases of the map rendering tests' map areas.
        private static IEnumerable<ComparisonCase> MapAreaCases()
        {
            return ComparisonCase.MakeMapAreaCases();
        }

        [TestCaseSource(nameof(DrawingCases))]
        public async Task Drawing(ComparisonCase comparisonCase)
        {
            await CompareWithReferenceAsync(comparisonCase);
        }

        [TestCaseSource(nameof(MapAreaCases))]
        public async Task MapArea(ComparisonCase comparisonCase)
        {
            await CompareWithReferenceAsync(comparisonCase);
        }

        // Draw a case through the SVG target, render the document in Chromium and compare the image with the case's
        // reference image, or end the test as pending.
        private static async Task CompareWithReferenceAsync(ComparisonCase comparisonCase)
        {
            string document;
            using (RecordingGraphicsTarget recording = comparisonCase.Record()) {
                List<string> missingCalls = recording.RecordedCallNames.Where(callName => !Svg_GraphicsTarget.FullyImplements(callName)).ToList();
                if (missingCalls.Count > 0)
                    throw Pending(comparisonCase, missingCalls);

                document = WriteDocument(comparisonCase, recording);
            }

            string svgPath = LoopResults.SaveSvg(comparisonCase.Name, document);
            using (SKBitmap reference = LoadReferenceImage(comparisonCase))
            using (SKBitmap rendering = await ChromiumRenderer.RenderAsync(svgPath, comparisonCase.PixelSize)) {
                ImageComparison.AssertImagesMatch(comparisonCase.Name, reference, rendering, MaxPixelDifference);
            }
        }

        // Write the SVG document of a case's recording, its area filling the image; a call that the target cannot
        // honor ends the test as pending.
        private static string WriteDocument(ComparisonCase comparisonCase, RecordingGraphicsTarget recording)
        {
            try {
                using (Svg_GraphicsTarget target = new Svg_GraphicsTarget())
                using (Matrix areaToImage = comparisonCase.CreateAreaToImageTransform()) {
                    target.PushTransform(areaToImage);
                    recording.Playback(target);
                    target.PopTransform();
                    return target.FinishDocument();
                }
            }
            catch (Svg_UnsupportedCallException unsupportedCall) {
                throw Pending(comparisonCase, new[] { unsupportedCall.CallName });
            }
        }

        // Load a case's reference image from TestFiles/svg_reference, failing the test if none is recorded.
        private static SKBitmap LoadReferenceImage(ComparisonCase comparisonCase)
        {
            string path = TestUtil.GetTestFile(Path.Combine("svg_reference", comparisonCase.Name + ".png"));
            if (!File.Exists(path))
                Assert.Fail(string.Format("No reference image of {0} has been recorded in svg_reference.", comparisonCase.Name));
            return SKBitmap.Decode(path);
        }

        // Record in the loop's results that a case is pending until the target implements the given calls, and
        // create the exception that ends its test as pending.
        private static IgnoreException Pending(ComparisonCase comparisonCase, IEnumerable<string> callNames)
        {
            List<string> calls = callNames.ToList();
            LoopResults.SavePendingCase(comparisonCase.Name, calls);
            return new IgnoreException("Pending until the SVG target implements " + string.Join(", ", calls) + ".");
        }
    }
}
