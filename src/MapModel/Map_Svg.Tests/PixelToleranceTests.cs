using System.Linq;
using NUnit.Framework;
using SkiaSharp;

namespace Map_Svg.Tests
{
    // Checks the pixel tolerance of the rendering comparison: shades along an edge pass, and so does a mix where edges
    // meet that blurring evens out, while a recolored inside, a missing line, a gap or a shape out of place fails, both
    // ways round, with blank paper beyond the image (F-1).
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
            int[,] expected = new int[9, 9];
            int[,] rendering = new int[9, 9];
            Fill(expected, 100);
            Fill(rendering, 100);
            FillBlock(rendering, 2, 2, 5, 100 + PixelTolerance.Default.Slack);
            Assert.AreEqual(0, CountPixelsOutside(expected, rendering));

            // Inside the block neither image has an edge within reach: its middle three by three pixels fail.
            FillBlock(rendering, 2, 2, 5, 100 + PixelTolerance.Default.Slack + 1);
            Assert.AreEqual(9, CountPixelsOutside(expected, rendering));
        }

        [Test]
        public void ARecoloredInsideFails()
        {
            using (SKBitmap expected = MakeImage(new int[9, 9], new SKColor(204, 76, 0)))
            using (SKBitmap rendering = MakeImage(new int[9, 9], new SKColor(204, 76, 0))) {
                for (int y = 2; y < 7; ++y)
                    for (int x = 2; x < 7; ++x)
                        rendering.SetPixel(x, y, new SKColor(204, 76, 10));
                int[] excess = ImageComparison.MeasureExcess(expected, rendering, PixelTolerance.Default);
                Assert.AreEqual(9, excess.Count(pixelExcess => pixelExcess > 0));
                Assert.AreEqual(10 - PixelTolerance.Default.Slack, excess.Max());
            }
        }

        [Test]
        public void AShadeWhereEdgesMeetPassesWhenTheBlurredImagesAgree()
        {
            // Where black meets dark grey, one rasterizer lets the white beneath show through and the other does not.
            int[,] expected = new int[9, 9];
            int[,] rendering = new int[9, 9];
            foreach (int[,] levels in new int[][,] { expected, rendering }) {
                FillBlock(levels, 0, 0, 9, 60);
                for (int y = 0; y < 4; ++y)
                    for (int x = 0; x < 9; ++x)
                        levels[y, x] = 0;
            }
            for (int x = 0; x < 9; ++x) {
                expected[4, x] = 100;
                rendering[4, x] = 30;
            }
            Assert.AreEqual(0, CountPixelsOutside(expected, rendering));
        }

        [Test]
        public void AWhiteGapWhereEdgesMeetFails()
        {
            // The gap stays away from the image's sides, beyond which lies white paper.
            int[,] expected = new int[9, 13];
            int[,] rendering = new int[9, 13];
            foreach (int[,] levels in new int[][,] { expected, rendering }) {
                FillBlock(levels, 0, 0, 13, 60);
                for (int y = 0; y < 4; ++y)
                    for (int x = 0; x < 13; ++x)
                        levels[y, x] = 0;
                for (int x = 0; x < 13; ++x)
                    levels[4, x] = 30;
            }
            for (int x = 2; x < 11; ++x)
                rendering[4, x] = 255;
            Assert.AreEqual(9, CountPixelsOutside(expected, rendering));
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
            FillBlock(levels, 0, 0, System.Math.Max(levels.GetLength(0), levels.GetLength(1)), level);
        }

        // Set the levels of a square block of an image, as far as it lies within the image.
        // levels: the image's levels, row by row.
        // left, top: the block's corner.
        // side: the block's side.
        // level: the level to set.
        private static void FillBlock(int[,] levels, int left, int top, int side, int level)
        {
            for (int y = top; y < System.Math.Min(top + side, levels.GetLength(0)); ++y)
                for (int x = left; x < System.Math.Min(left + side, levels.GetLength(1)); ++x)
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
