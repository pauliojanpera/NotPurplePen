using System.Collections.Generic;
using System.Drawing;
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
        // reference image, or end the test as pending. A case with no reference image recorded yet is compared with
        // one rendered now, which goes to the loop's results for recording, and fails.
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
            string referencePath = TestUtil.GetTestFile(Path.Combine("svg_reference", comparisonCase.Name + ".png"));
            bool referenceRecorded = File.Exists(referencePath);
            using (SKBitmap reference = referenceRecorded ? SKBitmap.Decode(referencePath) : RenderReferenceForRecording(comparisonCase))
            using (SKBitmap rendering = await ChromiumRenderer.RenderAsync(svgPath, comparisonCase.PixelSize)) {
                string difference = ImageComparison.DescribeDifference(comparisonCase.Name, reference, rendering, MaxPixelDifference);
                if (!referenceRecorded) {
                    Assert.Fail(string.Format("No reference image of {0} is recorded in svg_reference; the loop's results hold one under references/. Compared with it: {1}",
                                              comparisonCase.Name, difference ?? "Chromium's image matches."));
                }
                if (difference != null)
                    Assert.Fail(difference);
            }
        }

        // Write the SVG document of a case's recording: one map in one part, shown on a page the size of the image,
        // whose millimetres the renderer makes pixels. A call that the target cannot honor ends the test as pending.
        private static string WriteDocument(ComparisonCase comparisonCase, RecordingGraphicsTarget recording)
        {
            try {
                using (Svg_GraphicsTarget target = new Svg_GraphicsTarget(DefaultColorConverter.Instance, true))
                using (Matrix areaToImage = comparisonCase.CreateAreaToImageTransform()) {
                    object part = new object();
                    target.BeginPart(part);
                    target.BeginMap(comparisonCase, comparisonCase.Name);
                    recording.Playback(target);
                    target.EndMap();
                    target.EndPart();
                    target.AddPage(new SizeF(comparisonCase.PixelSize.Width, comparisonCase.PixelSize.Height), comparisonCase.Area, areaToImage, new object[] { part });
                    return target.FinishDocument();
                }
            }
            catch (Svg_UnsupportedCallException unsupportedCall) {
                throw Pending(comparisonCase, new[] { unsupportedCall.CallName });
            }
        }

        // Render a case's reference image, and save it in the loop's results as references/<name>.png, from where it
        // is recorded into TestFiles/svg_reference.
        private static SKBitmap RenderReferenceForRecording(ComparisonCase comparisonCase)
        {
            SKBitmap reference = comparisonCase.RenderReference();
            LoopResults.SaveImage(Path.Combine("references", comparisonCase.Name + ".png"), reference);
            return reference;
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
