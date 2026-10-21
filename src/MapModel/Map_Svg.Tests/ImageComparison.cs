using System.Drawing;
using System.IO;
using NUnit.Framework;
using SkiaSharp;
using TestingUtils;

namespace Map_Svg.Tests
{
    // Compares Chromium's image of a document with the image it should match.
    public static class ImageComparison
    {
        // Fail the test unless the images are the same size and no color channel of any pixel differs by more than
        // maxPixelDifference. On failure, Chromium's image and the difference image go to the loop's results as
        // renderings/<name>.png and differences/<name>.png.
        // name: the name of the case, which names the saved images.
        // expected: the image to match.
        // rendering: Chromium's image.
        // maxPixelDifference: how much a color channel of a pixel may differ.
        public static void AssertImagesMatch(string name, SKBitmap expected, SKBitmap rendering, int maxPixelDifference)
        {
            using (SKBitmap difference = BitmapTestUtil.CompareBitmaps(expected, rendering, Color.LightPink, Color.Transparent, maxPixelDifference)) {
                if (difference == null)
                    return;

                LoopResults.SaveImage(Path.Combine("renderings", name + ".png"), rendering);
                LoopResults.SaveImage(Path.Combine("differences", name + ".png"), difference);

                if (expected.Width != rendering.Width || expected.Height != rendering.Height)
                    Assert.Fail(string.Format("Chromium's image of {0} is {1}x{2} pixels, the image it should match {3}x{4}.",
                                              name, rendering.Width, rendering.Height, expected.Width, expected.Height));
                Assert.Fail(string.Format("Chromium's image of {0} differs from the image it should match by more than {1} in a color channel; see differences/{0}.png.",
                                          name, maxPixelDifference));
            }
        }
    }
}
