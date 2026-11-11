using System.Collections.Generic;
using System.Globalization;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using PurplePen.Graphics2D;
using PurplePen.MapModel;
using SkiaSharp;
using Map_SkiaStd;
using TestingUtils;

namespace Map_Svg.Tests
{
    // The rendering comparison: draws each case through the SVG target, renders the document in Chromium, and
    // compares the image with the case's reference image within the pixel tolerance; and reads the document back into
    // page coordinates and compares its geometry with the case's drawing calls, within what the rounding of its numbers
    // allows (V-10). A case that makes a call the target does not fully implement, or one it cannot honor, is pending, and
    // names the calls it still needs.
    [TestFixture, Parallelizable(ParallelScope.Children)]
    public class RenderingComparisonTests
    {
        // The metrics the Skia target lays text out with, which the SVG target lays it out with too (T-1).
        private static readonly ITextMetrics textMetrics = new Skia_TextMetrics();

        // The font data of the faces the Skia target draws text with, cut to the characters each document draws in
        // them, which the documents embed (T-6 to T-9).
        private static readonly IFontSubsetProvider fontSubsets = new SkiaFontSubsetProvider();

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

        // Make the cases that check where pattern fills put an area's pattern and its hatchings.
        private static IEnumerable<ComparisonCase> AreaPatternCases()
        {
            return ComparisonCase.MakeAreaPatternCases();
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

        [TestCaseSource(nameof(AreaPatternCases))]
        public async Task AreaPattern(ComparisonCase comparisonCase)
        {
            await CompareWithReferenceAsync(comparisonCase);
        }

        // Draw a case through the SVG target, render the document in Chromium and compare the image with the case's
        // reference image, and the document's geometry with the case's calls, or end the test as pending. A case with
        // no reference image recorded yet is compared with one rendered now, which goes to the loop's results for
        // recording, and fails.
        private static async Task CompareWithReferenceAsync(ComparisonCase comparisonCase)
        {
            string document;
            bool drawsText;
            string geometryDifference;
            using (RecordingGraphicsTarget recording = comparisonCase.Record()) {
                List<string> missingCalls = recording.RecordedCallNames.Where(callName => !Svg_GraphicsTarget.FullyImplements(callName)).ToList();
                if (missingCalls.Count > 0)
                    throw Pending(comparisonCase, missingCalls, null);

                document = WriteDocument(comparisonCase, recording);
                drawsText = DrawsText(recording.RecordedCallNames);
                geometryDifference = SvgGeometryCheck.Describe(comparisonCase, recording, document, textMetrics);
            }

            string svgPath = LoopResults.SaveSvg(comparisonCase.Name, document);
            string referencePath = TestUtil.GetTestFile(Path.Combine("svg_reference", comparisonCase.Name + ".png"));
            bool referenceRecorded = File.Exists(referencePath);
            using (SKBitmap reference = referenceRecorded ? SKBitmap.Decode(referencePath) : RenderReferenceForRecording(comparisonCase))
            using (SKBitmap rendering = await ChromiumRenderer.RenderAsync(svgPath, comparisonCase.PixelSize)) {
                string difference = ImageComparison.DescribeDifference(comparisonCase.Name, reference, rendering, drawsText ? PixelTolerance.Text : PixelTolerance.Default);
                if (drawsText) {
                    TextPositionMeasurement positions = await MeasureTextPositionsAsync(comparisonCase, svgPath);
                    if (positions.LargestOffset > TextPositionTolerance) {
                        string misplaced = string.Format(CultureInfo.InvariantCulture,
                                                         "Chromium puts a character of {0} {1:0.###} pixels from where Skia's shaping puts its glyph, more than {2} pixels: {3}.",
                                                         comparisonCase.Name, positions.LargestOffset, TextPositionTolerance, positions.Where);
                        difference = difference == null ? misplaced : difference + " " + misplaced;
                    }
                }
                if (geometryDifference != null)
                    difference = difference == null ? geometryDifference : difference + " " + geometryDifference;
                if (!referenceRecorded) {
                    Assert.Fail(string.Format("No reference image of {0} is recorded in svg_reference; the loop's results hold one under references/. Compared with it: {1}",
                                              comparisonCase.Name, difference ?? "Chromium's image matches within the tolerance."));
                }
                if (difference != null)
                    Assert.Fail(difference);
            }
        }

        // Whether a drawing draws text, which is compared within the tolerance for text, as Chromium shades glyphs
        // otherwise than Skia does, and whose characters' positions are measured too.
        // callNames: the names of the calls the drawing makes.
        private static bool DrawsText(IEnumerable<string> callNames)
        {
            return callNames.Any(callName => callName == nameof(Svg_GraphicsTarget.DrawText) || callName == nameof(Svg_GraphicsTarget.DrawTextOutline));
        }

        // How far, in pixels of the image, Chromium may put a character of a text from where Skia's shaping puts its
        // glyph, which is where the Skia target draws it and the PDF export writes it. The document places each cluster
        // of characters by a position written to a thousandth of a millimetre, so Chromium puts it there within the
        // rounding of that and of the widths the target is given in single precision: over the 78 text cases, within
        // 0.063 pixels. Without the positions it advances each glyph by its own rounding of the advance, which over a
        // line of 40 characters came to 0.6 pixels.
        private const double TextPositionTolerance = 0.1;

        // Measure where Chromium puts the characters of a drawing's texts against where Skia's shaping puts their
        // glyphs, and record it in the loop's results as text-positions/<name>.tsv, each cluster measured in
        // text-positions/<name>.clusters.tsv.
        // comparisonCase: the case.
        // svgPath: the path of its SVG document.
        private static async Task<TextPositionMeasurement> MeasureTextPositionsAsync(ComparisonCase comparisonCase, string svgPath)
        {
            List<DrawnText> texts = await ChromiumRenderer.ReadTextPositionsAsync(svgPath, comparisonCase.PixelSize);
            TextPositionMeasurement measurement = TextPositionMeasurement.Measure(texts, textMetrics);
            LoopResults.SaveText(Path.Combine("text-positions", comparisonCase.Name + ".tsv"), measurement.ToTsv());
            LoopResults.SaveText(Path.Combine("text-positions", comparisonCase.Name + ".clusters.tsv"), measurement.ClustersToTsv());
            return measurement;
        }

        // Write the SVG document of a case's recording: one map in one part, shown on a page the size of the image,
        // whose millimetres the renderer makes pixels. A call that the target cannot honor ends the test as pending, and
        // so, for good, does a line of zero width, which the document leaves out where the reference draws a hairline
        // (D-5, DL-6).
        private static string WriteDocument(ComparisonCase comparisonCase, RecordingGraphicsTarget recording)
        {
            try {
                using (Svg_GraphicsTarget target = new Svg_GraphicsTarget(DefaultColorConverter.Instance, true, SvgBitmapOptions.Default, textMetrics, fontSubsets))
                using (Matrix areaToImage = comparisonCase.CreateAreaToImageTransform()) {
                    object part = new object();
                    target.BeginPart(part);
                    target.BeginMap(comparisonCase, comparisonCase.Name);
                    recording.Playback(target);
                    target.EndMap();
                    target.EndPart();
                    target.AddPage(new SizeF(comparisonCase.PixelSize.Width, comparisonCase.PixelSize.Height), comparisonCase.Area, areaToImage, new object[] { part }, null);
                    string document = target.FinishDocument();
                    if (target.CallsLeftOut.Count > 0)
                        throw Pending(comparisonCase, target.CallsLeftOut, Svg_GraphicsTarget.WhyStrokesOfNoWidthAreLeftOut);
                    return document;
                }
            }
            catch (Svg_UnsupportedCallException unsupportedCall) {
                throw Pending(comparisonCase, new[] { unsupportedCall.CallName }, unsupportedCall.WhyNeverHonored);
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

        // Record in the loop's results that a case is pending on calls, until the target implements them or for good,
        // and create the exception that ends its test as pending.
        // comparisonCase: the case.
        // callNames: the names of the calls.
        // whyNeverHonored: why the target will never honor the calls as the case makes them, or null when it does not
        //                  honor them yet.
        private static IgnoreException Pending(ComparisonCase comparisonCase, IEnumerable<string> callNames, string whyNeverHonored)
        {
            List<string> calls = callNames.ToList();
            LoopResults.SavePendingCase(comparisonCase.Name, calls, whyNeverHonored);
            return new IgnoreException(whyNeverHonored == null
                ? "Pending until the SVG target implements " + string.Join(", ", calls) + "."
                : "Pending for good: the SVG target will never honor " + string.Join(", ", calls) + " as the case makes it: " + whyNeverHonored + ".");
        }
    }
}
