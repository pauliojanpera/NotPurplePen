using System.Drawing;
using System.Threading.Tasks;
using NUnit.Framework;
using SkiaSharp;

namespace Map_Svg.Tests
{
    // Checks the rendering step of the comparison on its own: Chromium, started through Playwright, renders an SVG
    // file at the size asked for, as Skia draws the same shapes.
    [TestFixture]
    public class ChromiumRenderingTests
    {
        [Test]
        public async Task ChromiumRendersAnSvgFileAsSkiaDrawsTheSameShapes()
        {
            const string document =
                "<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"40\" height=\"30\">" +
                "<rect x=\"5\" y=\"5\" width=\"20\" height=\"10\" fill=\"#f00\"/>" +
                "<rect x=\"15\" y=\"10\" width=\"20\" height=\"15\" fill=\"#00f\"/>" +
                "</svg>";

            using (SKBitmap expected = new SKBitmap(40, 30)) {
                using (SKCanvas canvas = new SKCanvas(expected))
                using (SKPaint red = new SKPaint { Color = new SKColor(255, 0, 0) })
                using (SKPaint blue = new SKPaint { Color = new SKColor(0, 0, 255) }) {
                    canvas.Clear(SKColors.White);
                    canvas.DrawRect(SKRect.Create(5, 5, 20, 10), red);
                    canvas.DrawRect(SKRect.Create(15, 10, 20, 15), blue);
                }

                string svgPath = LoopResults.SaveSvg("chromium-rendering-check", document);
                using (SKBitmap rendering = await ChromiumRenderer.RenderAsync(svgPath, new Size(40, 30))) {
                    string difference = ImageComparison.DescribeDifference("chromium-rendering-check", expected, rendering, PixelTolerance.Exact);
                    Assert.IsNull(difference, difference);
                }
            }
        }
    }
}
