using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using PurplePen.Graphics2D;
using PurplePen.MapModel;
using SkiaSharp;

namespace Map_Svg.Tests
{
    // Draws each JPEG variant of jpeg/ through the SVG target as the application loads it, and checks that the target
    // writes the variant's data as it is where the browsers draw that data as the application draws the bitmap, and
    // encodes the bitmap otherwise (B-3), and that Chromium, Firefox and WebKit then draw the document as the application
    // draws the bitmap, within the pixel tolerance of the rendering comparison (F-1, F-5). make-jpeg-variants.py beside
    // the variants writes them and says what each is.
    //
    // A variant's document goes to the loop's results as svg/jpeg-<variant>.svg, and each browser's image of it, when it
    // falls outside the tolerance, as renderings/jpeg-<variant>-<browser>.png beside the image of the pixels outside it,
    // differences/jpeg-<variant>-<browser>.png.
    [TestFixture, Parallelizable(ParallelScope.Children)]
    public class JpegDataTests
    {
        // The variants whose data the browsers draw as the application decodes it, which the target writes as it is.
        private static readonly string[] VariantsDrawnAsDecoded = { "baseline", "progressive", "gray", "rgb", "cmyk", "upright" };

        // The variants whose data a browser draws otherwise, whose bitmap the target encodes anew: the profile's colours
        // are applied by Chromium and WebKit, and the turn and the mirroring by all three, but by the application by none.
        private static readonly string[] VariantsDrawnOtherwise = { "turned", "mirrored", "profile" };

        // Each variant, with whether the browsers draw its data as the application decodes it.
        private static IEnumerable<TestCaseData> Variants()
        {
            return VariantsDrawnAsDecoded.Select(variant => new TestCaseData(variant, true).SetName("IsWritten(" + variant + ")"))
                .Concat(VariantsDrawnOtherwise.Select(variant => new TestCaseData(variant, false).SetName("IsWritten(" + variant + ")")));
        }

        // Each variant in each browser.
        private static IEnumerable<TestCaseData> VariantsInBrowsers()
        {
            foreach (Browser browser in new[] { Browser.Chromium, Browser.Firefox, Browser.WebKit }) {
                foreach (string variant in VariantsDrawnAsDecoded.Concat(VariantsDrawnOtherwise))
                    yield return new TestCaseData(variant, browser).SetName("In" + browser + "(" + variant + ")");
            }
        }

        [TestCaseSource(nameof(Variants))]
        public void AVariantIsWrittenAsItsDataOrEncodedAsTheBrowsersDrawIt(string variant, bool drawnAsDecoded)
        {
            byte[] data = File.ReadAllBytes(VariantPath(variant));
            using (IGraphicsBitmap bitmap = LoadVariant(variant)) {
                CollectionAssert.AreEqual(data, bitmap.GetOriginalData(), "the bitmap keeps the data it was decoded from");
                Assert.AreEqual(drawnAsDecoded, SvgJpegData.IsDrawnAsDecoded(data, bitmap.PixelWidth, bitmap.PixelHeight));

                byte[] written = WrittenImage(WriteDocument(bitmap));
                if (drawnAsDecoded) {
                    CollectionAssert.AreEqual(data, written, "the document holds the variant's data");
                }
                else {
                    byte[] encoded;
                    using (MemoryStream stream = new MemoryStream()) {
                        Assert.IsTrue(bitmap.WriteToStream(GraphicsBitmapFormat.JPEG, stream, SvgBitmapOptions.Default.JpegQuality));
                        encoded = stream.ToArray();
                    }
                    CollectionAssert.AreEqual(encoded, written, "the document holds the bitmap encoded as JPEG at the export's quality");
                }
            }
        }

        [TestCaseSource(nameof(VariantsInBrowsers))]
        public async Task AVariantsDocumentDrawsAsTheApplicationDrawsTheVariant(string variant, Browser browser)
        {
            using (IGraphicsBitmap bitmap = LoadVariant(variant)) {
                string svgPath = LoopResults.SaveSvg("jpeg-" + variant, WriteDocument(bitmap));
                Size pixelSize = new Size(bitmap.PixelWidth, bitmap.PixelHeight);
                using (SKBitmap application = ApplicationDrawing((Skia_Bitmap)bitmap))
                using (SKBitmap rendering = SKBitmap.Decode(await BrowserRenderer.RenderPngAsync(browser, svgPath, pixelSize))) {
                    int[] excess = ImageComparison.MeasureExcess(application, rendering, PixelTolerance.Default);
                    int pixelsOutside = excess.Count(pixelExcess => pixelExcess > 0);
                    if (pixelsOutside > 0) {
                        string name = "jpeg-" + variant + "-" + browser.ToString().ToLowerInvariant();
                        LoopResults.SaveImage(Path.Combine("renderings", name + ".png"), rendering);
                        Assert.Fail(browser + "'s image of the " + variant + " variant has " + pixelsOutside + " pixels outside the tolerance (" +
                                    PixelTolerance.Default + "), by up to " + excess.Max() + " levels; see renderings/" + name + ".png.");
                    }
                }
            }
        }

        // The path of a variant.
        // variant: the variant's name.
        private static string VariantPath(string variant)
        {
            return Path.Combine(AppContext.BaseDirectory, "jpeg", variant + ".jpg");
        }

        // Load a variant as the application loads a bitmap.
        // variant: the variant's name.
        private static IGraphicsBitmap LoadVariant(string variant)
        {
            using (FileStream stream = File.OpenRead(VariantPath(variant)))
                return new SkiaBitmapGraphicsLoader().ReadBitmapFromStream(stream);
        }

        // Write a document of one page, a millimetre to each of a bitmap's pixels, that draws the bitmap over all of it,
        // upright.
        // bitmap: the bitmap.
        private static string WriteDocument(IGraphicsBitmap bitmap)
        {
            using (Svg_GraphicsTarget target = new Svg_GraphicsTarget(DefaultColorConverter.Instance, true, SvgBitmapOptions.Default))
            using (Matrix mapToPage = new Matrix()) {
                RectangleF area = new RectangleF(0, 0, bitmap.PixelWidth, bitmap.PixelHeight);
                object part = new object();
                target.BeginPart(part);
                target.BeginMap(part, "variant");
                target.DrawBitmap(bitmap, area, BitmapScaling.MediumQuality);
                target.EndMap();
                target.EndPart();
                target.AddPage(area.Size, area, mapToPage, new object[] { part }, null);
                return target.FinishDocument();
            }
        }

        // The data of the one image a document holds.
        // document: the text of the document.
        private static byte[] WrittenImage(string document)
        {
            const string prefix = "xlink:href=\"data:image/jpeg;base64,";
            int start = document.IndexOf(prefix, StringComparison.Ordinal);
            Assert.GreaterOrEqual(start, 0, "the document holds a JPEG image");
            start += prefix.Length;
            return Convert.FromBase64String(document.Substring(start, document.IndexOf('"', start) - start));
        }

        // The bitmap as the application draws it at one pixel to each of its own, on white.
        // bitmap: the bitmap.
        private static SKBitmap ApplicationDrawing(Skia_Bitmap bitmap)
        {
            SKBitmap drawing = new SKBitmap(bitmap.PixelWidth, bitmap.PixelHeight);
            using (SKCanvas canvas = new SKCanvas(drawing)) {
                canvas.Clear(SKColors.White);
                canvas.DrawBitmap(bitmap.Bitmap, 0, 0);
            }
            return drawing;
        }
    }
}
