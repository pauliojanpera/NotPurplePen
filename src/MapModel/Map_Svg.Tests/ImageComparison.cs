using System;
using System.IO;
using SkiaSharp;

namespace Map_Svg.Tests
{
    // Compares Chromium's image of a document with the image it should match, within a pixel tolerance (F-1), and
    // measures how far the two differ.
    public static class ImageComparison
    {
        // The channel differences that the measurement counts pixels beyond.
        private static readonly int[] measuredDifferences = { 0, 8, 32, 64 };

        // Describe how Chromium's image falls outside the tolerance of the image it should match, or return null when no
        // pixel does. The differences are measured into the loop's results as measurements/<name>.tsv; beyond the
        // tolerance, Chromium's image and an image of the pixels outside it go there as renderings/<name>.png and
        // differences/<name>.png.
        // name: the name of the case, which names the saved files.
        // expected: the image to match.
        // rendering: Chromium's image.
        // tolerance: how far the images may differ.
        public static string DescribeDifference(string name, SKBitmap expected, SKBitmap rendering, PixelTolerance tolerance)
        {
            if (expected.Width != rendering.Width || expected.Height != rendering.Height) {
                LoopResults.SaveImage(Path.Combine("renderings", name + ".png"), rendering);
                return string.Format("Chromium's image of {0} is {1}x{2} pixels, the image it should match {3}x{4}.",
                                     name, rendering.Width, rendering.Height, expected.Width, expected.Height);
            }

            int[] excess = MeasureExcess(expected, rendering, tolerance);
            int pixelsOutside = 0, largestExcess = 0;
            foreach (int pixelExcess in excess) {
                if (pixelExcess > 0) {
                    ++pixelsOutside;
                    largestExcess = Math.Max(largestExcess, pixelExcess);
                }
            }
            LoopResults.SaveText(Path.Combine("measurements", name + ".tsv"),
                                 MeasureDifferences(expected, rendering, pixelsOutside, largestExcess));
            if (pixelsOutside == 0)
                return null;

            LoopResults.SaveImage(Path.Combine("renderings", name + ".png"), rendering);
            using (SKBitmap difference = ShowPixelsOutside(expected, excess))
                LoopResults.SaveImage(Path.Combine("differences", name + ".png"), difference);
            return string.Format("Chromium's image of {0} has {1} pixels outside the tolerance ({2}), by up to {3} levels; see differences/{0}.png.",
                                 name, pixelsOutside, tolerance, largestExcess);
        }

        // Measure, for each pixel, how many levels a color channel of either image lies beyond the range the tolerance
        // holds it to in the other image: zero where every channel lies within it, both ways round.
        // expected: the image to match.
        // rendering: Chromium's image, of the same size.
        // tolerance: how far the images may differ.
        public static int[] MeasureExcess(SKBitmap expected, SKBitmap rendering, PixelTolerance tolerance)
        {
            int width = expected.Width, height = expected.Height;
            byte[][] expectedChannels = SplitChannels(expected), renderedChannels = SplitChannels(rendering);
            int[] excess = new int[width * height];
            for (int channel = 0; channel < 3; ++channel) {
                RaiseExcess(excess, renderedChannels[channel], expectedChannels[channel], width, height, tolerance);
                RaiseExcess(excess, expectedChannels[channel], renderedChannels[channel], width, height, tolerance);
            }
            return excess;
        }

        // Raise each pixel's excess to how far a channel of one image lies beyond the range that channel takes in the
        // other image within the tolerance's reach, widened by its slack.
        // excess: the excess of each pixel so far.
        // values: the channel held to the range.
        // range: the channel whose neighbourhoods give the range.
        // width, height: the size of the images.
        // tolerance: how far the images may differ.
        private static void RaiseExcess(int[] excess, byte[] values, byte[] range, int width, int height, PixelTolerance tolerance)
        {
            byte[] lowest = FindWithinReach(range, width, height, tolerance.Reach, false);
            byte[] highest = FindWithinReach(range, width, height, tolerance.Reach, true);
            for (int index = 0; index < values.Length; ++index) {
                int beyond = Math.Max(lowest[index] - tolerance.Slack - values[index], values[index] - highest[index] - tolerance.Slack);
                if (beyond > excess[index])
                    excess[index] = beyond;
            }
        }

        // Find, for each pixel, the lowest or highest value a channel takes within reach of it, counting what lies beyond
        // the image as white: the blank paper around the page.
        // values: the channel.
        // width, height: the size of the image.
        // reach: how many pixels away a value is found.
        // highest: whether to find the highest value rather than the lowest.
        private static byte[] FindWithinReach(byte[] values, int width, int height, int reach, bool highest)
        {
            // The square neighbourhood is the row neighbourhood of the column neighbourhood.
            byte[] alongRows = new byte[values.Length];
            for (int y = 0; y < height; ++y) {
                for (int x = 0; x < width; ++x) {
                    int found = highest ? 0 : 255;
                    for (int step = -reach; step <= reach; ++step) {
                        int column = x + step;
                        int value = (column < 0 || column >= width) ? 255 : values[y * width + column];
                        found = highest ? Math.Max(found, value) : Math.Min(found, value);
                    }
                    alongRows[y * width + x] = (byte) found;
                }
            }

            byte[] result = new byte[values.Length];
            for (int y = 0; y < height; ++y) {
                for (int x = 0; x < width; ++x) {
                    int found = highest ? 0 : 255;
                    for (int step = -reach; step <= reach; ++step) {
                        int row = y + step;
                        int value = (row < 0 || row >= height) ? 255 : alongRows[row * width + x];
                        found = highest ? Math.Max(found, value) : Math.Min(found, value);
                    }
                    result[y * width + x] = (byte) found;
                }
            }
            return result;
        }

        // Split an image into its red, green and blue channels.
        // image: the image.
        private static byte[][] SplitChannels(SKBitmap image)
        {
            SKColor[] pixels = image.Pixels;
            byte[][] channels = { new byte[pixels.Length], new byte[pixels.Length], new byte[pixels.Length] };
            for (int index = 0; index < pixels.Length; ++index) {
                channels[0][index] = pixels[index].Red;
                channels[1][index] = pixels[index].Green;
                channels[2][index] = pixels[index].Blue;
            }
            return channels;
        }

        // Make an image of the pixels outside the tolerance: red over a pale ghost of the image to match.
        // expected: the image to match.
        // excess: each pixel's excess beyond the tolerance.
        private static SKBitmap ShowPixelsOutside(SKBitmap expected, int[] excess)
        {
            SKColor[] pixels = expected.Pixels;
            SKColor[] shown = new SKColor[pixels.Length];
            for (int index = 0; index < pixels.Length; ++index) {
                if (excess[index] > 0) {
                    shown[index] = new SKColor(255, 0, 0);
                }
                else {
                    double luminance = 0.2126 * pixels[index].Red + 0.7152 * pixels[index].Green + 0.0722 * pixels[index].Blue;
                    byte ghost = (byte) Math.Round(255 - (255 - luminance) * 0.16);
                    shown[index] = new SKColor(ghost, ghost, ghost);
                }
            }
            SKBitmap image = new SKBitmap(expected.Width, expected.Height, SKColorType.Rgba8888, SKAlphaType.Opaque);
            image.Pixels = shown;
            return image;
        }

        // Measure how two images of one size differ, as a header line and a line of values, tab separated: the largest
        // difference in a color channel, how many pixels differ by more than each measured difference, and how many lie
        // outside the tolerance, by up to how many levels.
        // expected: the image to match.
        // rendering: Chromium's image.
        // pixelsOutside: how many pixels lie outside the tolerance.
        // largestExcess: by how many levels the farthest of them does.
        private static string MeasureDifferences(SKBitmap expected, SKBitmap rendering, int pixelsOutside, int largestExcess)
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
            header += "\toutside tolerance\tlargest excess";
            values += "\t" + pixelsOutside + "\t" + largestExcess;
            return header + "\n" + values + "\n";
        }
    }
}
