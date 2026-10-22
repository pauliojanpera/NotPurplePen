using System.Linq;
using NUnit.Framework;
using SkiaSharp;

namespace Map_Svg.Tests
{
    // Checks the pixel tolerance of the rendering comparison: shades along an edge pass, while a recolored inside, a
    // missing line or a shape out of place fails, both ways round, with blank paper beyond the image (F-1).
    [TestFixture]
    public class PixelToleranceTests
    {
        [Test]
        public void IdenticalImagesMatch()
        {
            int[,] levels = { { 255, 128, 0 }, { 255, 128, 0 }, { 255, 128, 0 } };
            Assert.AreEqual(0, CountPixelsOutside(levels, levels));
        }

        [Test]
        public void AnEdgeShadedOtherwisePasses()
        {
            int[,] expected = { { 255, 255, 128, 0, 0 }, { 255, 255, 128, 0, 0 }, { 255, 255, 128, 0, 0 } };
            int[,] rendering = { { 255, 255, 60, 0, 0 }, { 255, 240, 90, 0, 0 }, { 255, 255, 60, 0, 0 } };
            Assert.AreEqual(0, CountPixelsOutside(expected, rendering));
        }

        [Test]
        public void AnInsideMayDifferByTheSlackOnly()
        {
            int[,] expected = new int[7, 7];
            int[,] rendering = new int[7, 7];
            Fill(expected, 100);
            Fill(rendering, 100);
            rendering[3, 3] = 100 + PixelTolerance.Default.Slack;
            Assert.AreEqual(0, CountPixelsOutside(expected, rendering));

            rendering[3, 3] = 100 + PixelTolerance.Default.Slack + 1;
            Assert.AreEqual(1, CountPixelsOutside(expected, rendering));
        }

        [Test]
        public void ARecoloredInsideFails()
        {
            using (SKBitmap expected = MakeImage(new int[7, 7], new SKColor(204, 76, 0)))
            using (SKBitmap rendering = MakeImage(new int[7, 7], new SKColor(204, 76, 0))) {
                rendering.SetPixel(3, 3, new SKColor(204, 76, 10));
                int[] excess = ImageComparison.MeasureExcess(expected, rendering, PixelTolerance.Default);
                Assert.AreEqual(1, excess.Count(pixelExcess => pixelExcess > 0));
                Assert.AreEqual(10 - PixelTolerance.Default.Slack, excess.Max());
            }
        }

        [Test]
        public void AMissingLineFails()
        {
            int[,] expected = new int[5, 7];
            int[,] rendering = new int[5, 7];
            Fill(expected, 255);
            Fill(rendering, 255);
            for (int y = 0; y < 5; ++y)
                expected[y, 3] = 0;
            Assert.AreEqual(5, CountPixelsOutside(expected, rendering));
            Assert.AreEqual(5, CountPixelsOutside(rendering, expected));
        }

        [Test]
        public void AShapeWithinReachOfItsPlacePasses()
        {
            Assert.AreEqual(0, CountPixelsOutside(MakeSquare(8, 2, 2, 3), MakeSquare(8, 3, 2, 3)));
        }

        [Test]
        public void AShapeBeyondReachOfItsPlaceFails()
        {
            Assert.Greater(CountPixelsOutside(MakeSquare(9, 2, 2, 3), MakeSquare(9, 4, 2, 3)), 0);
        }

        [Test]
        public void BeyondTheImageIsBlankPaper()
        {
            // A shape cut by the top of the image, whose cut row one rasterizer shades lighter than the other.
            int[,] expected = { { 255, 150, 150, 150, 255 }, { 255, 0, 0, 0, 255 }, { 255, 0, 0, 0, 255 } };
            int[,] rendering = { { 255, 100, 100, 100, 255 }, { 255, 0, 0, 0, 255 }, { 255, 0, 0, 0, 255 } };
            Assert.AreEqual(0, CountPixelsOutside(expected, rendering));
        }

        // Count the pixels of two grey images that lie outside the default tolerance.
        // expected, rendering: the grey levels of the images, row by row.
        private static int CountPixelsOutside(int[,] expected, int[,] rendering)
        {
            using (SKBitmap expectedImage = MakeImage(expected, null))
            using (SKBitmap renderedImage = MakeImage(rendering, null))
                return ImageComparison.MeasureExcess(expectedImage, renderedImage, PixelTolerance.Default).Count(pixelExcess => pixelExcess > 0);
        }

        // Make a white image of a black square.
        // size: the side of the image.
        // left, top: the square's corner.
        // side: the square's side.
        private static int[,] MakeSquare(int size, int left, int top, int side)
        {
            int[,] levels = new int[size, size];
            Fill(levels, 255);
            for (int y = top; y < top + side; ++y)
                for (int x = left; x < left + side; ++x)
                    levels[y, x] = 0;
            return levels;
        }

        // Set every level of an image.
        // levels: the image's levels, row by row.
        // level: the level to set.
        private static void Fill(int[,] levels, int level)
        {
            for (int y = 0; y < levels.GetLength(0); ++y)
                for (int x = 0; x < levels.GetLength(1); ++x)
                    levels[y, x] = level;
        }

        // Make an opaque image of grey levels, or of one color everywhere.
        // levels: the grey levels, row by row, which also give the image's size.
        // color: the color of every pixel, or null for the grey levels.
        private static SKBitmap MakeImage(int[,] levels, SKColor? color)
        {
            int height = levels.GetLength(0), width = levels.GetLength(1);
            SKBitmap image = new SKBitmap(width, height, SKColorType.Rgba8888, SKAlphaType.Opaque);
            for (int y = 0; y < height; ++y) {
                for (int x = 0; x < width; ++x) {
                    byte level = (byte) levels[y, x];
                    image.SetPixel(x, y, color ?? new SKColor(level, level, level));
                }
            }
            return image;
        }
    }
}
