using System;
using System.Drawing;
using System.IO;
using SkiaSharp;
using TestingUtils;

namespace Map_Svg.Tests
{
    // Compares Chromium's image of a document with the image it should match, and measures how far they differ, which
    // is what the pixel tolerance of the comparison is chosen from (F-1).
    public static class ImageComparison
    {
        // The channel differences that the measurement counts pixels beyond.
        private static readonly int[] measuredDifferences = { 0, 8, 32, 64 };

        // Describe how Chromium's image differs from the image it should match, or return null when no color channel
        // of any pixel differs by more than maxPixelDifference. The differences are measured into the loop's results
        // as measurements/<name>.tsv; beyond the allowance, Chromium's image and the difference image go there as
        // renderings/<name>.png and differences/<name>.png.
        // name: the name of the case, which names the saved files.
        // expected: the image to match.
        // rendering: Chromium's image.
        // maxPixelDifference: how much a color channel of a pixel may differ.
        public static string DescribeDifference(string name, SKBitmap expected, SKBitmap rendering, int maxPixelDifference)
        {
            if (expected.Width == rendering.Width && expected.Height == rendering.Height)
                LoopResults.SaveText(Path.Combine("measurements", name + ".tsv"), MeasureDifferences(expected, rendering));

            using (SKBitmap difference = BitmapTestUtil.CompareBitmaps(expected, rendering, Color.LightPink, Color.Transparent, maxPixelDifference)) {
                if (difference == null)
                    return null;

                LoopResults.SaveImage(Path.Combine("renderings", name + ".png"), rendering);
                LoopResults.SaveImage(Path.Combine("differences", name + ".png"), difference);

                if (expected.Width != rendering.Width || expected.Height != rendering.Height)
                    return string.Format("Chromium's image of {0} is {1}x{2} pixels, the image it should match {3}x{4}.",
                                         name, rendering.Width, rendering.Height, expected.Width, expected.Height);
                return string.Format("Chromium's image of {0} differs from the image it should match by more than {1} in a color channel; see differences/{0}.png.",
                                     name, maxPixelDifference);
            }
        }

        // Measure how two images of one size differ: the largest difference in a color channel, and how many pixels
        // differ by more than each measured difference, as a header line and a line of values, tab separated.
        // expected: the image to match.
        // rendering: Chromium's image.
        private static string MeasureDifferences(SKBitmap expected, SKBitmap rendering)
        {
            SKColor[] expectedPixels = expected.Pixels;
            SKColor[] renderedPixels = rendering.Pixels;
            int largestDifference = 0;
            int[] pixelsBeyond = new int[measuredDifferences.Length];
            for (int index = 0; index < expectedPixels.Length; ++index) {
                SKColor a = expectedPixels[index], b = renderedPixels[index];
                int difference = Math.Max(Math.Abs(a.Red - b.Red), Math.Max(Math.Abs(a.Green - b.Green), Math.Abs(a.Blue - b.Blue)));
                largestDifference = Math.Max(largestDifference, difference);
                for (int level = 0; level < measuredDifferences.Length; ++level) {
                    if (difference > measuredDifferences[level])
                        ++pixelsBeyond[level];
                }
            }

            string header = "largest difference\tpixels";
            string values = largestDifference + "\t" + expectedPixels.Length;
            for (int level = 0; level < measuredDifferences.Length; ++level) {
                header += "\tbeyond " + measuredDifferences[level];
                values += "\t" + pixelsBeyond[level];
            }
            return header + "\n" + values + "\n";
        }
    }
}
