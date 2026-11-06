using System;
using System.Collections.Generic;
using System.Drawing;
using NUnit.Framework;
using PurplePen.Graphics2D;
using PurplePen.MapModel;

namespace Map_Svg.Tests
{
    // Checks the documents that the SVG target writes, against expected text (V-1): the layout of maps and pages, the
    // calls it implements, where it puts styles, and the errors of calls out of turn (L-1 to L-5, D-12, S-4, S-8, F-2 to
    // F-4, COL-1 to COL-6, B-1 to B-6).
    [TestFixture]
    public class SvgDocumentTests
    {
        // The start tag of a document whose first page is 100 by 50 millimetres.
        private const string RootOf100By50 =
            "<svg xmlns=\"http://www.w3.org/2000/svg\" xmlns:xlink=\"http://www.w3.org/1999/xlink\" " +
            "xmlns:npp=\"https://pauliojanpera.github.io/NotPurplePen/svg\" " +
            "xmlns:inkscape=\"http://www.inkscape.org/namespaces/inkscape\" xmlns:sodipodi=\"http://sodipodi.sourceforge.net/DTD/sodipodi-0.dtd\" " +
            "width=\"100mm\" height=\"50mm\" viewBox=\"0 0 100 50\">";

        // The start tag of a document whose first page is 100 by 50 millimetres and which draws text (T-11).
        private const string RootOf100By50WithText =
            "<svg xmlns=\"http://www.w3.org/2000/svg\" xmlns:xlink=\"http://www.w3.org/1999/xlink\" " +
            "xmlns:npp=\"https://pauliojanpera.github.io/NotPurplePen/svg\" " +
            "xmlns:inkscape=\"http://www.inkscape.org/namespaces/inkscape\" xmlns:sodipodi=\"http://sodipodi.sourceforge.net/DTD/sodipodi-0.dtd\" " +
            "width=\"100mm\" height=\"50mm\" viewBox=\"0 0 100 50\" xml:lang=\"und\">";

        // The lines that mark the only page of a document, of 100 by 50 millimetres and with no name, as Inkscape's own page
        // (L-6), which follow the metadata.
        private static readonly string[] InkscapePageOf100By50 = {
            "<sodipodi:namedview>", "<inkscape:page x=\"0\" y=\"0\" width=\"100\" height=\"50\"/>", "</sodipodi:namedview>",
        };

        // The view of the only page of a document, of 100 by 50 millimetres and with no name (L-6), before its group.
        private const string ViewOf100By50 = "<view id=\"page\" viewBox=\"0 0 100 50\"/>";

        // The declaration of the CMYK profile that icc-color() names, with the device CMYK profile behind it (COL-3).
        private const string CmykProfileLine = "<color-profile id=\"cmyk\" name=\"cmyk\" xlink:href=\"" + SvgPaints.CmykProfileHref + "\"/>";

        // The style block of a document that draws no text, which has none (S-4).
        private static readonly string[] NoStyleBlock = new string[0];

        // The declarations of black fills.
        private const string BlackFill = "fill:#000;fill:#000 icc-color(cmyk,0,0,0,1)";

        // Draw within one part holding one map labeled "map", show the part on a page of 100 by 50 millimetres that
        // shows the map from (0, 0) to (100, 50), its y axis turned to point down, and return the document.
        // draw: draws within the map.
        // declaresCmykColors: whether colors carry their CMYK values.
        // bitmapOptions: how bitmaps reach the document, or null for the default.
        // textMetrics: the metrics that lay text out, or null to draw no text.
        // fontSubsets: what gives the font data of the faces drawn in, or null to embed no fonts.
        private static string WriteOnePage(Action<Svg_GraphicsTarget> draw, bool declaresCmykColors = true, SvgBitmapOptions bitmapOptions = null,
                                           ITextMetrics textMetrics = null, IFontSubsetProvider fontSubsets = null)
        {
            using (Svg_GraphicsTarget target = new Svg_GraphicsTarget(DefaultColorConverter.Instance, declaresCmykColors, bitmapOptions ?? SvgBitmapOptions.Default,
                                                                      textMetrics, fontSubsets))
            using (Matrix mapToPage = new Matrix()) {
                object part = new object();
                target.BeginPart(part);
                target.BeginMap(part, "map");
                draw(target);
                target.EndMap();
                target.EndPart();

                mapToPage.Translate(0, 50);
                mapToPage.Scale(1, -1);
                target.AddPage(new SizeF(100, 50), new RectangleF(0, 0, 100, 50), mapToPage, new object[] { part }, null);
                return target.FinishDocument();
            }
        }

        // The document that WriteOnePage writes, given the lines of its style block and the lines within its part.
        // styleLines: the rules of the style block, one to a line, which only a document that draws text has.
        // partLines: the lines within the part's definition.
        // refersToCmykProfile: whether a color refers to the CMYK profile, which the definitions then declare.
        // drawsText: whether the part draws text, which the root then names the language of.
        private static string OnePageDocument(string[] styleLines, string[] partLines, bool refersToCmykProfile = true, bool drawsText = false)
        {
            List<string> lines = new List<string> { drawsText ? RootOf100By50WithText : RootOf100By50 };
            if (styleLines.Length > 0) {
                lines.Add("<style>");
                lines.AddRange(styleLines);
                lines.Add("</style>");
            }
            lines.AddRange(new string[] {
                "<metadata>", "<npp:document version=\"1\">", "<npp:map n=\"1\" label=\"map\" views=\"page\"/>", "</npp:document>", "</metadata>",
                InkscapePageOf100By50[0], InkscapePageOf100By50[1], InkscapePageOf100By50[2],
                "<defs>"
            });
            if (refersToCmykProfile)
                lines.Add(CmykProfileLine);
            lines.AddRange(new string[] { "<clipPath id=\"b\">", "<path d=\"M0 0h100000v50000H0z\"/>", "</clipPath>", "<g id=\"a\">" });
            lines.AddRange(partLines);
            lines.AddRange(new string[] {
                "</g>", "</defs>",
                ViewOf100By50,
                "<g transform=\"matrix(.001 0 0-.001 0 50)\" clip-path=\"url(#b)\">", "<use xlink:href=\"#a\"/>", "</g>",
                "</svg>"
            });
            return string.Join("\n", lines) + "\n";
        }

        // The document that WriteOnePage writes when its part draws images, which the definitions hold after the part's,
        // so that the page's clip, named last, takes the id after theirs.
        // styleLines: the rules of the style block, one to a line, which only a document that draws text has.
        // partLines: the lines within the part's definition.
        // imageLines: the definitions of the images.
        // clipId: the id of the page's clip.
        private static string OnePageDocumentWithImages(string[] styleLines, string[] partLines, string[] imageLines, string clipId)
        {
            List<string> lines = new List<string> { RootOf100By50 };
            if (styleLines.Length > 0) {
                lines.Add("<style>");
                lines.AddRange(styleLines);
                lines.Add("</style>");
            }
            lines.AddRange(new string[] {
                "<metadata>", "<npp:document version=\"1\">", "<npp:map n=\"1\" label=\"map\" views=\"page\"/>", "</npp:document>", "</metadata>",
                InkscapePageOf100By50[0], InkscapePageOf100By50[1], InkscapePageOf100By50[2],
                "<defs>", "<clipPath id=\"" + clipId + "\">", "<path d=\"M0 0h100000v50000H0z\"/>", "</clipPath>", "<g id=\"a\">"
            });
            lines.AddRange(partLines);
            lines.Add("</g>");
            lines.AddRange(imageLines);
            lines.AddRange(new string[] {
                "</defs>",
                ViewOf100By50,
                "<g transform=\"matrix(.001 0 0-.001 0 50)\" clip-path=\"url(#" + clipId + ")\">", "<use xlink:href=\"#a\"/>", "</g>",
                "</svg>"
            });
            return string.Join("\n", lines) + "\n";
        }

        // The data URI of the data that a test bitmap writes.
        // name: the bitmap's name.
        // format: the format it is written in.
        // quality: the quality, which JPEG alone carries.
        private static string DataUri(string name, GraphicsBitmapFormat format, int quality)
        {
            byte[] data = System.Text.Encoding.ASCII.GetBytes(TestBitmap.WrittenText(name, format, quality));
            return "data:" + (format == GraphicsBitmapFormat.JPEG ? "image/jpeg" : "image/png") + ";base64," + Convert.ToBase64String(data);
        }

        // A point.
        private static PointF Point(float x, float y)
        {
            return new PointF(x, y);
        }

        [Test]
        public void APageShowsItsPartThroughItsClipAndTransform()
        {
            string expected = string.Join("\n", new string[] {
                RootOf100By50,
                "<metadata>",
                "<npp:document version=\"1\">",
                "<npp:map n=\"1\" label=\"map\" views=\"page\"/>",
                "</npp:document>",
                "</metadata>",
                InkscapePageOf100By50[0],
                InkscapePageOf100By50[1],
                InkscapePageOf100By50[2],
                "<defs>",
                "<clipPath id=\"b\">",
                "<path d=\"M0 0h100000v50000H0z\"/>",
                "</clipPath>",
                "<g id=\"a\">",
                "</g>",
                "</defs>",
                ViewOf100By50,
                "<g transform=\"matrix(.001 0 0-.001 0 50)\" clip-path=\"url(#b)\">",
                "<use xlink:href=\"#a\"/>",
                "</g>",
                "</svg>",
            }) + "\n";
            Assert.AreEqual(expected, WriteOnePage(target => { }));
        }

        [Test]
        public void FilledShapesOfTwoBrushesEachCarryTheStyleOfTheirBrush()
        {
            string document = WriteOnePage(target => {
                object red = new object(), green = new object();
                target.CreateSolidBrush(red, CmykColor.FromCmyk(0, 1, 1, 0));
                target.CreateSolidBrush(green, CmykColor.FromCmyk(1, 0, 1, 0));
                target.FillRectangle(red, new RectangleF(10, 10, 20, 5));
                target.FillPolygon(green, new PointF[] { Point(40, 10), Point(50, 10), Point(45, 18.5F) }, AreaFillMode.Winding);
                target.FillEllipse(red, Point(60, 25), 5, 5);
                target.FillEllipse(green, Point(80, 25), 8, 4.5F);
            });
            Assert.AreEqual(OnePageDocument(
                NoStyleBlock,
                new string[] {
                    "<g data-m=\"1\">",
                    "<path style=\"fill:#f00;fill:#f00 icc-color(cmyk,0,1,1,0)\" d=\"M10000 10000h20000v5000H10000z\"/>",
                    "<path style=\"fill:#0f0;fill:#0f0 icc-color(cmyk,1,0,1,0)\" d=\"M40000 10000h10000l-5000 8500z\"/>",
                    "<circle style=\"fill:#f00;fill:#f00 icc-color(cmyk,0,1,1,0)\" cx=\"60000\" cy=\"25000\" r=\"5000\"/>",
                    "<ellipse style=\"fill:#0f0;fill:#0f0 icc-color(cmyk,1,0,1,0)\" cx=\"80000\" cy=\"25000\" rx=\"8000\" ry=\"4500\"/>",
                    "</g>",
                }), document);
        }

        [Test]
        public void CentersAtZeroAreLeftOutAsTheDefault()
        {
            string document = WriteOnePage(target => {
                object black = new object();
                target.CreateSolidBrush(black, CmykColor.FromCmyk(0, 0, 0, 1));
                target.FillEllipse(black, Point(0, 0), 1, 1);
                target.FillEllipse(black, Point(0, 3), 2, 1);
                target.FillEllipse(black, Point(4, 0), 1, 1);
            });
            Assert.AreEqual(OnePageDocument(
                NoStyleBlock,
                new string[] {
                    "<g style=\"" + BlackFill + "\" data-m=\"1\">",
                    "<circle r=\"1000\"/>",
                    "<ellipse cy=\"3000\" rx=\"2000\" ry=\"1000\"/>",
                    "<circle cx=\"4000\" r=\"1000\"/>",
                    "</g>",
                }), document);
        }

        [Test]
        public void ATranslucentColorUnderTheAlternateModeCarriesItsOpacityAndTheEvenOddRule()
        {
            string document = WriteOnePage(target => {
                object brush = new object();
                target.CreateSolidBrush(brush, CmykColor.FromCmyka(0.455F, 0.2F, 0, 0.1F, 0.25F));
                target.FillPolygon(brush, new PointF[] { Point(0, 0), Point(10, 0), Point(0, 10), Point(10, 10) }, AreaFillMode.Alternate);
            });
            Assert.AreEqual(OnePageDocument(
                NoStyleBlock,
                new string[] {
                    "<g style=\"fill:#71b2e6;fill:#71b2e6 icc-color(cmyk,.455,.2,0,.1);fill-opacity:.25;fill-rule:evenodd\" data-m=\"1\">",
                    "<path d=\"M0 0h10000L0 10000h10000z\"/>",
                    "</g>",
                }), document);
        }

        [Test]
        public void UnderTheRgbColorModelColorsCarryNoCmyk()
        {
            string document = WriteOnePage(target => {
                object brush = new object();
                target.CreateSolidBrush(brush, CmykColor.FromCmyk(0, 0.5F, 1, 0));
                target.FillRectangle(brush, new RectangleF(1, 2, 3, 4));
            }, false);
            Assert.AreEqual(OnePageDocument(
                NoStyleBlock,
                new string[] { "<g style=\"fill:#ff8000\" data-m=\"1\">", "<path d=\"M1000 2000h3000v4000H1000z\"/>", "</g>" },
                false), document);
        }

        [Test]
        public void IntensityScalesTheCmykBeforeConversion()
        {
            string document = WriteOnePage(target => {
                object brush = new object();
                target.Intensity = 0.4F;
                target.CreateSolidBrush(brush, CmykColor.FromCmyk(0.5F, 0, 1, 0.25F));
                target.FillRectangle(brush, new RectangleF(1, 2, 3, 4));
            });
            Assert.AreEqual(OnePageDocument(
                NoStyleBlock,
                new string[] {
                    "<g style=\"fill:#b2e680;fill:#b2e680 icc-color(cmyk,.2,0,.4,.1)\" data-m=\"1\">",
                    "<path d=\"M1000 2000h3000v4000H1000z\"/>",
                    "</g>",
                }), document);
        }

        [Test]
        public void SettingTheIntensityDiscardsTheBrushes()
        {
            using (Svg_GraphicsTarget target = new Svg_GraphicsTarget(DefaultColorConverter.Instance, true)) {
                object brush = new object();
                target.CreateSolidBrush(brush, CmykColor.FromCmyk(0, 0, 0, 1));
                Assert.IsTrue(target.HasBrush(brush));
                target.Intensity = 0.5F;
                Assert.IsFalse(target.HasBrush(brush));
            }
        }

        [Test]
        public void TransformsAreWrittenInTheirShortestFormAndTheIdentityNotAtAll()
        {
            string document = WriteOnePage(target => {
                object black = new object();
                target.CreateSolidBrush(black, CmykColor.FromCmyk(0, 0, 0, 1));
                using (Matrix translation = new Matrix()) {
                    translation.Translate(20, 30);
                    target.PushTransform(translation);
                    target.FillRectangle(black, new RectangleF(-1, -1, 2, 2));
                    target.PopTransform();
                }
                using (Matrix rotation = new Matrix()) {
                    rotation.Translate(20, 30);
                    rotation.Rotate(45);
                    target.PushTransform(rotation);
                    target.FillRectangle(black, new RectangleF(-1, -1, 2, 2));
                    target.PopTransform();
                }
                using (Matrix shear = new Matrix()) {
                    shear.Shear(0.5F, 0);
                    target.PushTransform(shear);
                    target.FillEllipse(black, Point(0, 0), 1, 1);
                    target.PopTransform();
                }
                using (Matrix identity = new Matrix()) {
                    target.PushTransform(identity);
                    target.FillEllipse(black, Point(5, 5), 1, 1);
                    target.PopTransform();
                }
            });
            Assert.AreEqual(OnePageDocument(
                NoStyleBlock,
                new string[] {
                    "<g style=\"" + BlackFill + "\" data-m=\"1\">",
                    "<g transform=\"translate(20000 30000)\">",
                    "<path d=\"M-1000-1000h2000v2000h-2000z\"/>",
                    "</g>",
                    "<g transform=\"translate(20000 30000)rotate(45)\">",
                    "<path d=\"M-1000-1000h2000v2000h-2000z\"/>",
                    "</g>",
                    "<g transform=\"matrix(1 0 .5 1 0 0)\">",
                    "<circle r=\"1000\"/>",
                    "</g>",
                    "<circle cx=\"5000\" cy=\"5000\" r=\"1000\"/>",
                    "</g>",
                }), document);
        }

        [Test]
        public void CrispEdgesHoldUntilAntiAliasingIsPushedAgain()
        {
            string document = WriteOnePage(target => {
                object black = new object();
                target.CreateSolidBrush(black, CmykColor.FromCmyk(0, 0, 0, 1));
                target.PushAntiAliasing(true);
                target.FillEllipse(black, Point(1, 1), 1, 1);
                target.PushAntiAliasing(false);
                target.FillEllipse(black, Point(3, 3), 1, 1);
                target.PushAntiAliasing(true);
                target.FillEllipse(black, Point(5, 5), 1, 1);
                target.PopAntiAliasing();
                target.PopAntiAliasing();
                target.PopAntiAliasing();
            });
            Assert.AreEqual(OnePageDocument(
                NoStyleBlock,
                new string[] {
                    "<g style=\"" + BlackFill + "\" data-m=\"1\">",
                    "<circle cx=\"1000\" cy=\"1000\" r=\"1000\"/>",
                    "<g style=\"shape-rendering:crispEdges\">",
                    "<circle cx=\"3000\" cy=\"3000\" r=\"1000\"/>",
                    "<g style=\"shape-rendering:auto\">",
                    "<circle cx=\"5000\" cy=\"5000\" r=\"1000\"/>",
                    "</g>",
                    "</g>",
                    "</g>",
                }), document);
        }

        [Test]
        public void AdjacentGroupsOfOneTransformMergeAndFoldIntoTheirParent()
        {
            string document = WriteOnePage(target => {
                object black = new object();
                target.CreateSolidBrush(black, CmykColor.FromCmyk(0, 0, 0, 1));
                foreach (float x in new float[] { 1, 2 }) {
                    using (Matrix translation = new Matrix()) {
                        translation.Translate(10, 10);
                        target.PushTransform(translation);
                        target.FillRectangle(black, new RectangleF(x, 0, 1, 1));
                        target.PopTransform();
                    }
                }
            });
            Assert.AreEqual(OnePageDocument(
                NoStyleBlock,
                new string[] {
                    "<g style=\"" + BlackFill + "\" transform=\"translate(10000 10000)\" data-m=\"1\">",
                    "<path d=\"M1000 0h1000v1000H1000z\"/>",
                    "<path d=\"M2000 0h1000v1000H2000z\"/>",
                    "</g>",
                }), document);
        }

        [Test]
        public void AGroupThatOnlyMovesOneEllipseGivesWayToTheEllipseMovedIntoPlace()
        {
            string document = WriteOnePage(target => {
                object black = new object();
                target.CreateSolidBrush(black, CmykColor.FromCmyk(0, 0, 0, 1));
                using (Matrix translation = new Matrix()) {
                    translation.Translate(20.0004F, 30.0006F);
                    target.PushTransform(translation);
                    target.FillEllipse(black, Point(1, 0), 2, 1);
                    target.PopTransform();
                    target.PushTransform(translation);
                    target.FillEllipse(black, Point(0, 0), 1, 1);
                    target.FillEllipse(black, Point(3, 0), 1, 1);
                    target.PopTransform();
                }
            });
            Assert.AreEqual(OnePageDocument(
                NoStyleBlock,
                new string[] {
                    "<g style=\"" + BlackFill + "\" data-m=\"1\">",
                    "<ellipse cx=\"21000\" cy=\"30001\" rx=\"2000\" ry=\"1000\"/>",
                    "<g transform=\"translate(20000 30001)\">",
                    "<circle r=\"1000\"/>",
                    "<circle cx=\"3000\" r=\"1000\"/>",
                    "</g>",
                    "</g>",
                }), document);
        }

        [Test]
        public void EndingAMapClosesTheGroupsStillOpenWithinIt()
        {
            string document = WriteOnePage(target => {
                object black = new object();
                target.CreateSolidBrush(black, CmykColor.FromCmyk(0, 0, 0, 1));
                using (Matrix translation = new Matrix()) {
                    translation.Translate(1, 2);
                    target.PushTransform(translation);
                    target.FillRectangle(black, new RectangleF(0, 0, 1, 1));
                }
            });
            StringAssert.Contains("<g style=\"" + BlackFill + "\" transform=\"translate(1000 2000)\" data-m=\"1\">\n<path d=\"M0 0h1000v1000H0z\"/>\n</g>\n", document);
        }

        [Test]
        public void PagesAfterTheFirstLieToItsRightAndEqualClipsShareADefinition()
        {
            using (Svg_GraphicsTarget target = new Svg_GraphicsTarget(DefaultColorConverter.Instance, true))
            using (Matrix mapToPage = new Matrix()) {
                object part = new object();
                target.BeginPart(part);
                target.EndPart();
                mapToPage.Translate(0, 50);
                mapToPage.Scale(1, -1);
                target.AddPage(new SizeF(100, 50), new RectangleF(0, 0, 100, 50), mapToPage, new object[] { part }, null);
                target.AddPage(new SizeF(100, 50), new RectangleF(0, 0, 100, 50), mapToPage, new object[] { part }, null);

                string document = target.FinishDocument();
                StringAssert.Contains(ViewOf100By50 + "\n<g transform=\"matrix(.001 0 0-.001 0 50)\" clip-path=\"url(#b)\">\n<use xlink:href=\"#a\"/>\n</g>\n", document);
                StringAssert.Contains("<view id=\"page_2\" viewBox=\"110 0 100 50\"/>\n" +
                                      "<g transform=\"matrix(.001 0 0-.001 110 50)\" clip-path=\"url(#b)\">\n<use xlink:href=\"#a\"/>\n</g>\n", document);
                Assert.AreEqual(1, document.Split(new[] { "<clipPath" }, StringSplitOptions.None).Length - 1);
                StringAssert.Contains(RootOf100By50, document);
            }
        }

        [Test]
        public void EachPageCarriesAViewOfItsPaperAndAnInkscapePageNamedAfterIt()
        {
            using (Svg_GraphicsTarget target = new Svg_GraphicsTarget(DefaultColorConverter.Instance, true))
            using (Matrix mapToPage = new Matrix()) {
                object part = new object();
                target.BeginPart(part);
                target.EndPart();
                target.AddPage(new SizeF(215.9F, 279.4F), new RectangleF(0, 0, 100, 50), mapToPage, new object[] { part }, "Course 5-2");
                target.AddPage(new SizeF(279.4F, 215.9F), new RectangleF(0, 0, 100, 50), mapToPage, new object[] { part }, "Course 5-2");
                target.AddPage(new SizeF(100, 50), new RectangleF(0, 0, 100, 50), mapToPage, new object[] { part }, "1 & <2>");

                string document = target.FinishDocument();
                StringAssert.Contains(" xmlns:inkscape=\"http://www.inkscape.org/namespaces/inkscape\" xmlns:sodipodi=\"http://sodipodi.sourceforge.net/DTD/sodipodi-0.dtd\"" +
                                      " width=\"215.9mm\" height=\"279.4mm\" viewBox=\"0 0 215.9 279.4\">\n", document);
                StringAssert.Contains("</metadata>\n<sodipodi:namedview>\n" +
                                      "<inkscape:page x=\"0\" y=\"0\" width=\"215.9\" height=\"279.4\" inkscape:label=\"Course 5-2\"/>\n" +
                                      "<inkscape:page x=\"225.9\" y=\"0\" width=\"279.4\" height=\"215.9\" inkscape:label=\"Course 5-2\"/>\n" +
                                      "<inkscape:page x=\"515.3\" y=\"0\" width=\"100\" height=\"50\" inkscape:label=\"1 &amp; &lt;2&gt;\"/>\n" +
                                      "</sodipodi:namedview>\n<defs>\n", document);
                StringAssert.Contains("<view id=\"Course_5-2\" viewBox=\"0 0 215.9 279.4\"/>\n<g ", document);
                StringAssert.Contains("<view id=\"Course_5-2_2\" viewBox=\"225.9 0 279.4 215.9\"/>\n<g ", document);
                StringAssert.Contains("<view id=\"_1_2_\" viewBox=\"515.3 0 100 50\"/>\n<g ", document);
            }
        }

        [Test]
        public void EachMapRecordNamesTheViewsOfThePagesThatShowIt()
        {
            using (Svg_GraphicsTarget target = new Svg_GraphicsTarget(DefaultColorConverter.Instance, true))
            using (Matrix mapToPage = new Matrix()) {
                // A base map in a part of its own, two courses' maps in parts of their own, and a map in no page's part.
                object[] parts = { new object(), new object(), new object(), new object() };
                string[] labels = { "base", "Course 1", "Course 2", "unshown" };
                for (int index = 0; index < parts.Length; ++index) {
                    target.BeginPart(parts[index]);
                    target.BeginMap(new object(), labels[index]);
                    target.EndMap();
                    target.EndPart();
                }
                target.AddPage(new SizeF(100, 50), new RectangleF(0, 0, 100, 50), mapToPage, new object[] { parts[0], parts[1] }, "Course 1");
                target.AddPage(new SizeF(100, 50), new RectangleF(0, 0, 100, 50), mapToPage, new object[] { parts[0], parts[2] }, "Course 2");
                target.AddPage(new SizeF(100, 50), new RectangleF(0, 0, 100, 50), mapToPage, new object[] { parts[0], parts[2] }, "Course 2");

                StringAssert.Contains("<npp:map n=\"1\" label=\"base\" views=\"Course_1 Course_2 Course_2_2\"/>\n" +
                                      "<npp:map n=\"2\" label=\"Course 1\" views=\"Course_1\"/>\n" +
                                      "<npp:map n=\"3\" label=\"Course 2\" views=\"Course_2 Course_2_2\"/>\n" +
                                      "<npp:map n=\"4\" label=\"unshown\"/>\n", target.FinishDocument());
            }
        }

        [Test]
        public void AViewIsNamedAfterItsPageAsAnXmlName()
        {
            Assert.AreEqual("Course_5-2", SvgDocument.XmlNameOf("Course 5-2"));
            Assert.AreEqual("All_controls", SvgDocument.XmlNameOf("All controls"));
            Assert.AreEqual("_1", SvgDocument.XmlNameOf("1"));
            Assert.AreEqual("_-a", SvgDocument.XmlNameOf("-a"));
            Assert.AreEqual("A_B", SvgDocument.XmlNameOf("A & B"));
            Assert.AreEqual("Bl\u00e5_2.v", SvgDocument.XmlNameOf("Bl\u00e5 2.v"));
            Assert.AreEqual("_", SvgDocument.XmlNameOf(":"));
            Assert.AreEqual("a\U0001D49C", SvgDocument.XmlNameOf("a\U0001D49C"));
            Assert.AreEqual("a_", SvgDocument.XmlNameOf("a\uD835"));
        }

        [Test]
        public void TheIdsADocumentNamesSkipItsViews()
        {
            using (Svg_GraphicsTarget target = new Svg_GraphicsTarget(DefaultColorConverter.Instance, true))
            using (Matrix mapToPage = new Matrix()) {
                object part = new object();
                target.BeginPart(part);
                target.EndPart();
                target.AddPage(new SizeF(100, 50), new RectangleF(0, 0, 100, 50), mapToPage, new object[] { part }, "a");

                string document = target.FinishDocument();
                StringAssert.Contains("<g id=\"b\">\n</g>\n", document);
                StringAssert.Contains("<clipPath id=\"c\">", document);
                StringAssert.Contains("<view id=\"a\" viewBox=\"0 0 100 50\"/>\n<g transform=\"scale(.001)\" clip-path=\"url(#c)\">\n<use xlink:href=\"#b\"/>\n</g>\n", document);
            }
        }

        [Test]
        public void AViewTakesAnotherIdThanTheColorProfilesUnderTheCmykColorModel()
        {
            foreach (bool declaresCmykColors in new bool[] { true, false }) {
                using (Svg_GraphicsTarget target = new Svg_GraphicsTarget(DefaultColorConverter.Instance, declaresCmykColors))
                using (Matrix mapToPage = new Matrix()) {
                    object part = new object();
                    target.BeginPart(part);
                    target.EndPart();
                    target.AddPage(new SizeF(100, 50), new RectangleF(0, 0, 100, 50), mapToPage, new object[] { part }, "cmyk");
                    string viewId = declaresCmykColors ? "cmyk_2" : "cmyk";
                    StringAssert.Contains("<view id=\"" + viewId + "\" viewBox=\"0 0 100 50\"/>\n", target.FinishDocument());
                }
            }
        }

        [Test]
        public void TheColorProfileIsADeviceCmykOutputProfileDeclaredWithAnIdAndItsName()
        {
            string document = WriteOnePage(target => {
                object black = new object();
                target.CreateSolidBrush(black, CmykColor.FromCmyk(0, 0, 0, 1));
                target.FillRectangle(black, new RectangleF(0, 0, 10, 10));
            });
            StringAssert.Contains("<defs>\n<color-profile id=\"cmyk\" name=\"cmyk\" xlink:href=\"data:application/vnd.iccprofile;base64,", document);

            const string prefix = "data:application/vnd.iccprofile;base64,";
            StringAssert.StartsWith(prefix, SvgPaints.CmykProfileHref);
            byte[] profile = Convert.FromBase64String(SvgPaints.CmykProfileHref.Substring(prefix.Length));
            Assert.AreEqual(516, profile.Length);
            Assert.AreEqual(516, BigEndian(profile, 0, 4), "the size in the header");
            Assert.AreEqual(0x02100000, BigEndian(profile, 8, 4), "version 2.1");
            Assert.AreEqual("prtrCMYKLab ", System.Text.Encoding.ASCII.GetString(profile, 12, 12), "an output profile from CMYK to CIELAB");
            Assert.AreEqual("acsp", System.Text.Encoding.ASCII.GetString(profile, 36, 4));

            List<string> tags = new List<string>();
            int table = 0;
            for (int index = 0; index < BigEndian(profile, 128, 4); ++index) {
                string signature = System.Text.Encoding.ASCII.GetString(profile, 132 + 12 * index, 4);
                tags.Add(signature);
                if (signature == "A2B0")
                    table = BigEndian(profile, 136 + 12 * index, 4);
            }
            CollectionAssert.AreEqual(new string[] { "desc", "cprt", "wtpt", "A2B0" }, tags);
            Assert.AreEqual("mft2", System.Text.Encoding.ASCII.GetString(profile, table, 4));
            Assert.AreEqual(2, profile[table + 10], "two grid points a channel");
            // The grid follows the table's header, its matrix, its counts and its input curves; its first point is no
            // ink and its second black alone, whose lightness is the paper's white and none.
            int grid = table + 68;
            Assert.AreEqual(0xFF00, BigEndian(profile, grid, 2), "no ink");
            Assert.AreEqual(0, BigEndian(profile, grid + 6, 2), "black alone");
        }

        // A number written big-endian, as ICC profiles write them.
        // data: the bytes.
        // offset: where the number begins.
        // length: its length in bytes.
        private static int BigEndian(byte[] data, int offset, int length)
        {
            int number = 0;
            for (int index = 0; index < length; ++index)
                number = (number << 8) | data[offset + index];
            return number;
        }

        // The declarations of a black stroke, which every style of a black pen begins with.
        private const string BlackStroke = "fill:none;stroke:#000;stroke:#000 icc-color(cmyk,0,0,0,1)";

        // Make a path part.
        // kind: the kind of part.
        // points: its points.
        private static GraphicsPathPart Part(GraphicsPathPartKind kind, params PointF[] points)
        {
            return new GraphicsPathPart(kind, points);
        }

        [Test]
        public void PensWriteOnlyWhatDiffersFromTheDefaults()
        {
            string document = WriteOnePage(target => {
                CmykColor black = CmykColor.FromCmyk(0, 0, 0, 1);
                object plain = new object(), round = new object(), square = new object(), longMiters = new object(), noMiters = new object();
                target.CreatePen(plain, black, 0.35F, LineCapMode.Flat, LineJoinMode.Miter, 4);
                target.CreatePen(round, black, 0.35F, LineCapMode.Round, LineJoinMode.Round, 4);
                target.CreatePen(square, black, 0.35F, LineCapMode.Square, LineJoinMode.Bevel, 4);
                target.CreatePen(longMiters, black, 0.35F, LineCapMode.Flat, LineJoinMode.Miter, 2.5F);
                target.CreatePen(noMiters, black, 0.35F, LineCapMode.Flat, LineJoinMode.Miter, 1);
                object[] pens = { plain, round, square, longMiters, noMiters };
                for (int index = 0; index < pens.Length; ++index)
                    target.DrawLine(pens[index], Point(0, index), Point(10, index));
            });
            Assert.AreEqual(OnePageDocument(
                NoStyleBlock,
                new string[] {
                    "<g data-m=\"1\">",
                    "<path style=\"" + BlackStroke + ";stroke-width:350\" d=\"M0 0h10000\"/>",
                    "<path style=\"" + BlackStroke + ";stroke-width:350;stroke-linecap:round;stroke-linejoin:round\" d=\"M0 1000h10000\"/>",
                    "<path style=\"" + BlackStroke + ";stroke-width:350;stroke-linecap:square;stroke-linejoin:bevel\" d=\"M0 2000h10000\"/>",
                    "<path style=\"" + BlackStroke + ";stroke-width:350;stroke-miterlimit:2.5\" d=\"M0 3000h10000\"/>",
                    "<path style=\"" + BlackStroke + ";stroke-width:350;stroke-linejoin:bevel\" d=\"M0 4000h10000\"/>",
                    "</g>",
                }), document);
        }

        [Test]
        public void ShapesAreStrokedAsPathsCirclesAndEllipses()
        {
            string document = WriteOnePage(target => {
                object pen = new object();
                target.CreatePen(pen, CmykColor.FromCmyk(0, 0, 0, 1), 0.5F, LineCapMode.Round, LineJoinMode.Round, 4);
                target.DrawPolyline(pen, new PointF[] { Point(0, 0), Point(10, 0), Point(10, 10) });
                target.DrawPolygon(pen, new PointF[] { Point(20, 0), Point(30, 0), Point(25, 8) });
                target.DrawRectangle(pen, new RectangleF(40, 0, 10, 5));
                target.DrawEllipse(pen, Point(60, 5), 3, 3);
                target.DrawEllipse(pen, Point(70, 5), 4, 2);
            });
            Assert.AreEqual(OnePageDocument(
                NoStyleBlock,
                new string[] {
                    "<g style=\"" + BlackStroke + ";stroke-width:500;stroke-linecap:round;stroke-linejoin:round\" data-m=\"1\">",
                    "<path d=\"M0 0h10000v10000\"/>",
                    "<path d=\"M20000 0h10000l-5000 8000z\"/>",
                    "<path d=\"M40000 0h10000v5000H40000z\"/>",
                    "<circle cx=\"60000\" cy=\"5000\" r=\"3000\"/>",
                    "<ellipse cx=\"70000\" cy=\"5000\" rx=\"4000\" ry=\"2000\"/>",
                    "</g>",
                }), document);
        }

        [Test]
        public void ArcsTurnFromTheXAxisTowardsTheYAxisAndAFullTurnIsACircle()
        {
            string document = WriteOnePage(target => {
                object pen = new object();
                target.CreatePen(pen, CmykColor.FromCmyk(0, 0, 0, 1), 0.5F, LineCapMode.Flat, LineJoinMode.Miter, 4);
                target.DrawArc(pen, Point(0, 0), 10, 0, 90);
                target.DrawArc(pen, Point(0, 0), 10, 0, -270);
                target.DrawArc(pen, Point(0, 0), 10, 45, 360);
            });
            Assert.AreEqual(OnePageDocument(
                NoStyleBlock,
                new string[] {
                    "<g style=\"" + BlackStroke + ";stroke-width:500\" data-m=\"1\">",
                    "<path d=\"M10000 0A10000 10000 0 0 1 0 10000\"/>",
                    "<path d=\"M10000 0A10000 10000 0 1 0 0 10000\"/>",
                    "<circle r=\"10000\"/>",
                    "</g>",
                }), document);
        }

        [Test]
        public void APenOfABrushStrokesWithTheBrushsPaint()
        {
            string document = WriteOnePage(target => {
                object brush = new object(), pen = new object();
                target.CreateSolidBrush(brush, CmykColor.FromCmyk(0, 1, 1, 0));
                target.CreatePen(pen, brush, 1, LineCapMode.Flat, LineJoinMode.Miter, 4);
                target.DrawLine(pen, Point(0, 0), Point(5, 5));
            });
            Assert.AreEqual(OnePageDocument(
                NoStyleBlock,
                new string[] {
                    "<g style=\"fill:none;stroke:#f00;stroke:#f00 icc-color(cmyk,0,1,1,0);stroke-width:1000\" data-m=\"1\">",
                    "<path d=\"M0 0l5000 5000\"/>",
                    "</g>",
                }), document);
        }

        [Test]
        public void ClippedJoinsAndHairlinesAreNotHonored()
        {
            using (Svg_GraphicsTarget target = new Svg_GraphicsTarget(DefaultColorConverter.Instance, true)) {
                CmykColor black = CmykColor.FromCmyk(0, 0, 0, 1);
                Svg_UnsupportedCallException error = Assert.Throws<Svg_UnsupportedCallException>(
                    () => target.CreatePen(new object(), black, 1, LineCapMode.Flat, LineJoinMode.MiterClipped, 4));
                Assert.AreEqual("CreatePen", error.CallName);
                error = Assert.Throws<Svg_UnsupportedCallException>(() => target.CreatePen(new object(), black, 0, LineCapMode.Flat, LineJoinMode.Miter, 4));
                Assert.AreEqual("CreatePen", error.CallName);
            }
        }

        [Test]
        public void PathsTakeTheirPartsAndFillUnderTheirRule()
        {
            List<GraphicsPathPart> parts = new List<GraphicsPathPart> {
                Part(GraphicsPathPartKind.Start, Point(0, 0)),
                Part(GraphicsPathPartKind.Lines, Point(10, 0), Point(10, 10)),
                Part(GraphicsPathPartKind.Beziers, Point(10, 15), Point(5, 20), Point(0, 20)),
                Part(GraphicsPathPartKind.Close),
            };
            string document = WriteOnePage(target => {
                object path = new object(), brush = new object(), pen = new object();
                target.CreatePath(path, parts, AreaFillMode.Alternate);
                target.CreateSolidBrush(brush, CmykColor.FromCmyk(0, 0, 0, 1));
                target.CreatePen(pen, CmykColor.FromCmyk(0, 0, 0, 1), 0.1F, LineCapMode.Flat, LineJoinMode.Miter, 4);
                target.FillPath(brush, path);
                target.DrawPath(pen, path);
                target.FillPath(brush, parts, AreaFillMode.Winding);
                target.DrawPath(pen, parts);
                Assert.IsTrue(target.HasPath(path));
            });
            const string data = "M0 0h10000v10000c0 5000-5000 10000-10000 10000z";
            Assert.AreEqual(OnePageDocument(
                NoStyleBlock,
                new string[] {
                    "<g data-m=\"1\">",
                    "<path style=\"" + BlackFill + ";fill-rule:evenodd\" d=\"" + data + "\"/>",
                    "<path style=\"" + BlackStroke + ";stroke-width:100\" d=\"" + data + "\"/>",
                    "<path style=\"" + BlackFill + "\" d=\"" + data + "\"/>",
                    "<path style=\"" + BlackStroke + ";stroke-width:100\" d=\"" + data + "\"/>",
                    "</g>",
                }), document);
        }

        [Test]
        public void ClipsAreGroupsThatNestAndShareEqualDefinitions()
        {
            string document = WriteOnePage(target => {
                object brush = new object();
                target.CreateSolidBrush(brush, CmykColor.FromCmyk(0, 0, 0, 1));
                target.PushClip(new RectangleF(0, 0, 10, 10));
                target.FillRectangle(brush, new RectangleF(0, 0, 20, 20));
                target.PushClip(new RectangleF[] { new RectangleF(20, 0, 5, 5), new RectangleF(30, 0, 5, 5) });
                target.FillEllipse(brush, Point(5, 5), 1, 1);
                target.PopClip();
                target.PopClip();
                target.FillEllipse(brush, Point(50, 5), 1, 1);
                target.PushClip(new RectangleF(0, 0, 10, 10));
                target.FillEllipse(brush, Point(8, 8), 1, 1);
                target.PopClip();
                target.PushClip(new List<GraphicsPathPart> { Part(GraphicsPathPartKind.Start, Point(0, 0)), Part(GraphicsPathPartKind.Lines, Point(10, 0), Point(0, 10), Point(10, 10)), Part(GraphicsPathPartKind.Close) },
                                AreaFillMode.Alternate);
                target.FillEllipse(brush, Point(2, 2), 1, 1);
                target.PopClip();
            });
            StringAssert.Contains(string.Join("\n", new string[] {
                "<defs>", CmykProfileLine,
                "<clipPath id=\"b\">", "<path d=\"M0 0h10000v10000H0z\"/>", "</clipPath>",
                "<clipPath id=\"c\">", "<path d=\"M20000 0h5000v5000h-5000zM30000 0h5000v5000h-5000z\"/>", "</clipPath>",
                "<clipPath id=\"d\">", "<path style=\"clip-rule:evenodd\" d=\"M0 0h10000L0 10000h10000z\"/>", "</clipPath>",
                "<clipPath id=\"e\">", "<path d=\"M0 0h100000v50000H0z\"/>", "</clipPath>",
                "<g id=\"a\">",
                "<g style=\"" + BlackFill + "\" data-m=\"1\">",
                "<g clip-path=\"url(#b)\">",
                "<path d=\"M0 0h20000v20000H0z\"/>",
                "<g clip-path=\"url(#c)\">",
                "<circle cx=\"5000\" cy=\"5000\" r=\"1000\"/>",
                "</g>",
                "</g>",
                "<circle cx=\"50000\" cy=\"5000\" r=\"1000\"/>",
                "<g clip-path=\"url(#b)\">",
                "<circle cx=\"8000\" cy=\"8000\" r=\"1000\"/>",
                "</g>",
                "<g clip-path=\"url(#d)\">",
                "<circle cx=\"2000\" cy=\"2000\" r=\"1000\"/>",
                "</g>",
                "</g>",
                "</g>",
                "</defs>",
            }), document);
        }

        [Test]
        public void BlendingDarkensWhatLiesBelowAndBlendingGroupsNeverMerge()
        {
            bool supported = false;
            string document = WriteOnePage(target => {
                object brush = new object();
                target.CreateSolidBrush(brush, CmykColor.FromCmyk(0, 0, 0, 1));
                supported = target.PushBlending(BlendMode.Darken);
                target.FillEllipse(brush, Point(1, 1), 1, 1);
                target.PopBlending();
                target.PushBlending(BlendMode.Darken);
                target.FillEllipse(brush, Point(3, 3), 1, 1);
                target.PopBlending();
            });
            Assert.IsTrue(supported);
            Assert.AreEqual(OnePageDocument(
                NoStyleBlock,
                new string[] {
                    "<g style=\"" + BlackFill + "\" data-m=\"1\">",
                    "<g style=\"mix-blend-mode:darken\">",
                    "<circle cx=\"1000\" cy=\"1000\" r=\"1000\"/>",
                    "</g>",
                    "<g style=\"mix-blend-mode:darken\">",
                    "<circle cx=\"3000\" cy=\"3000\" r=\"1000\"/>",
                    "</g>",
                    "</g>",
                }), document);
        }

        // Create a pattern brush whose tile holds a black dot at its middle.
        // target: the target that creates the brush.
        // brushKey: the brush's key.
        // size: the size of the tile.
        // angle: the angle of the pattern.
        private static void CreateDottedBrush(IGraphicsTarget target, object brushKey, SizeF size, float angle)
        {
            using (IBrushTarget tile = target.CreatePatternBrush(size, angle, 16, 16)) {
                object dot = new object();
                tile.CreateSolidBrush(dot, CmykColor.FromCmyk(0, 0, 0, 1));
                tile.FillEllipse(dot, Point(0, 0), 0.25F, 0.25F);
                tile.FinishBrush(brushKey);
            }
        }

        [Test]
        public void APatternRepeatsItsTileWithTheTilesMiddleOnTheOriginTurnedByItsAngle()
        {
            string document = WriteOnePage(target => {
                Assert.IsTrue(target.SupportsPatternBrushes);
                Assert.IsTrue(target.PatternBrushesAreVectors);
                object brush = new object();
                using (IBrushTarget tile = target.CreatePatternBrush(new SizeF(4, 2), 30, 99, 99)) {
                    object pen = new object();
                    tile.CreatePen(pen, CmykColor.FromCmyk(0, 0, 0, 1), 0.5F, LineCapMode.Flat, LineJoinMode.Miter, 4);
                    tile.DrawLine(pen, Point(-2, 0), Point(2, 0));
                    tile.FinishBrush(brush);
                }
                target.FillRectangle(brush, new RectangleF(0, 0, 10, 10));
            });
            string expected = string.Join("\n", new string[] {
                RootOf100By50,
                "<metadata>", "<npp:document version=\"1\">", "<npp:map n=\"1\" label=\"map\" views=\"page\"/>", "</npp:document>", "</metadata>",
                InkscapePageOf100By50[0], InkscapePageOf100By50[1], InkscapePageOf100By50[2],
                "<defs>",
                CmykProfileLine,
                "<clipPath id=\"c\">", "<path d=\"M0 0h100000v50000H0z\"/>", "</clipPath>",
                "<pattern id=\"b\" patternUnits=\"userSpaceOnUse\" x=\"-2000\" y=\"-1000\" width=\"4000\" height=\"2000\" viewBox=\"-2000 -1000 4000 2000\" patternTransform=\"rotate(30)\">",
                "<path style=\"" + BlackStroke + ";stroke-width:500\" d=\"M-2000 0h4000\"/>",
                "</pattern>",
                "<g id=\"a\">", "<g style=\"fill:url(#b)\" data-m=\"1\">", "<path d=\"M0 0h10000v10000H0z\"/>", "</g>", "</g>",
                "</defs>",
                ViewOf100By50,
                "<g transform=\"matrix(.001 0 0-.001 0 50)\" clip-path=\"url(#c)\">", "<use xlink:href=\"#a\"/>", "</g>",
                "</svg>",
            }) + "\n";
            Assert.AreEqual(expected, document);
        }

        [Test]
        public void APenOfAPatternBrushStrokesWithThePattern()
        {
            string document = WriteOnePage(target => {
                object brush = new object(), pen = new object();
                CreateDottedBrush(target, brush, new SizeF(1, 1), 0);
                target.CreatePen(pen, brush, 1, LineCapMode.Round, LineJoinMode.Round, 4);
                target.DrawLine(pen, Point(0, 0), Point(5, 5));
                target.FillPolygon(brush, new PointF[] { Point(0, 0), Point(4, 0), Point(0, 4) }, AreaFillMode.Alternate);
            });
            StringAssert.Contains("<path style=\"fill:none;stroke:url(#b);stroke-width:1000;stroke-linecap:round;stroke-linejoin:round\" d=\"M0 0l5000 5000\"/>\n" +
                                  "<path style=\"fill:url(#b);fill-rule:evenodd\" d=\"M0 0h4000L0 4000z\"/>\n", document);
            Assert.AreEqual(1, document.Split(new[] { "<pattern" }, StringSplitOptions.None).Length - 1);
        }

        [Test]
        public void PatternsAreNamedInTheOrderTheyAreFirstUsedAndOnlyThoseUsedAreWritten()
        {
            string document = WriteOnePage(target => {
                object small = new object(), unused = new object(), large = new object();
                CreateDottedBrush(target, small, new SizeF(2, 2), 0);
                CreateDottedBrush(target, unused, new SizeF(4, 4), 0);
                CreateDottedBrush(target, large, new SizeF(6, 6), 0);
                target.FillRectangle(large, new RectangleF(0, 0, 10, 10));
                target.FillRectangle(small, new RectangleF(20, 0, 10, 10));
            });
            StringAssert.Contains("<pattern id=\"b\" patternUnits=\"userSpaceOnUse\" x=\"-3000\" y=\"-3000\" width=\"6000\" height=\"6000\" viewBox=\"-3000 -3000 6000 6000\">", document);
            StringAssert.Contains("<pattern id=\"c\" patternUnits=\"userSpaceOnUse\" x=\"-1000\" y=\"-1000\" width=\"2000\" height=\"2000\" viewBox=\"-1000 -1000 2000 2000\">", document);
            Assert.AreEqual(2, document.Split(new[] { "<pattern" }, StringSplitOptions.None).Length - 1);
            StringAssert.Contains("<path style=\"fill:url(#b)\" d=\"M0 0h10000v10000H0z\"/>\n<path style=\"fill:url(#c)\" d=\"M20000 0h10000v10000H20000z\"/>\n", document);
        }

        [Test]
        public void ATileIsDrawnAtTheIntensityOfTheTargetThatCreatesItWithKeysOfItsOwn()
        {
            string document = WriteOnePage(target => {
                target.Intensity = 0.5F;
                object brush = new object();
                using (IBrushTarget tile = target.CreatePatternBrush(new SizeF(2, 2), 0, 8, 8)) {
                    object dot = new object();
                    tile.CreateSolidBrush(dot, CmykColor.FromCmyk(0, 0, 0, 1));
                    tile.FillEllipse(dot, Point(0, 0), 0.25F, 0.25F);
                    Assert.IsFalse(target.HasBrush(dot));
                    tile.FinishBrush(brush);
                }
                Assert.IsTrue(target.HasBrush(brush));
                target.FillRectangle(brush, new RectangleF(0, 0, 10, 10));
            });
            StringAssert.Contains("icc-color(cmyk,0,0,0,.5)", document);
        }

        [Test]
        public void ATileHoldsDrawingAloneAndTakesNoneOnceItsBrushIsFinished()
        {
            using (Svg_GraphicsTarget target = new Svg_GraphicsTarget(DefaultColorConverter.Instance, true))
            using (Matrix matrix = new Matrix()) {
                IBrushTarget tile = target.CreatePatternBrush(new SizeF(2, 2), 0, 8, 8);
                IStructuredGraphicsTarget structuredTile = (IStructuredGraphicsTarget)tile;
                InvalidOperationException error = Assert.Throws<InvalidOperationException>(() => structuredTile.BeginPart(new object()));
                StringAssert.StartsWith("BeginPart: a pattern brush's tile holds drawing alone", error.Message);

                object brush = new object();
                tile.FinishBrush(brush);
                error = Assert.Throws<InvalidOperationException>(() => tile.PushTransform(matrix));
                StringAssert.StartsWith("PushTransform: the pattern brush is finished", error.Message);
                error = Assert.Throws<InvalidOperationException>(() => target.CreatePatternBrush(new SizeF(2, 2), 0, 8, 8).FinishBrush(brush));
                StringAssert.StartsWith("FinishBrush: the key has a brush already", error.Message);
            }
        }

        // Fill a dot at a point as the drawing of an object.
        // target: the target.
        // brush: the brush to fill with.
        // objectKey, symbolKey: the object's key and its symbol's.
        // symbolId, symbolName: the symbol's id and name.
        // x: where the dot lies along the x axis.
        private static void DrawObject(IStructuredGraphicsTarget target, object brush, object objectKey, object symbolKey, string symbolId, string symbolName, float x)
        {
            target.BeginObject(objectKey, symbolKey, symbolId, symbolName);
            target.FillEllipse(brush, Point(x, 0), 1, 1);
            target.EndObject();
        }

        [Test]
        public void RunsSymbolsAndObjectsCarryTheMapsStructureAndTheMetadataItsTables()
        {
            string document = WriteOnePage(target => {
                object brush = new object(), brown = new object(), black = new object();
                object contour = new object(), cliff = new object(), first = new object(), second = new object(), third = new object();
                target.CreateSolidBrush(brush, CmykColor.FromCmyk(0, 0, 0, 1));
                target.BeginColorRun(brown, 3, "Brown", CmykColor.FromCmyk(0, 0.56F, 1, 0.18F), false);
                DrawObject(target, brush, first, contour, "101.0", "Contour", 2);
                DrawObject(target, brush, second, contour, "101.0", "Contour", 4);
                DrawObject(target, brush, third, cliff, "201.0", "Cliff", 6);
                target.EndColorRun();
                target.BeginColorRun(black, 5, "Black & \"key\"", CmykColor.FromCmyk(0, 0, 0, 1), true);
                DrawObject(target, brush, first, contour, "101.0", "Contour", 2);
                target.EndColorRun();
            });
            StringAssert.Contains(string.Join("\n", new string[] {
                "<npp:map n=\"1\" label=\"map\" views=\"page\">",
                "<npp:color position=\"3\" name=\"Brown\" cmyk=\"0,.56,1,.18\"/>",
                "<npp:color position=\"5\" name=\"Black &amp; &quot;key&quot;\" cmyk=\"0,0,0,1\" overprint=\"1\"/>",
                "<npp:symbol n=\"1\" id=\"101.0\" name=\"Contour\" colors=\"3 5\"/>",
                "<npp:symbol n=\"2\" id=\"201.0\" name=\"Cliff\" colors=\"3\"/>",
                "</npp:map>",
            }) + "\n", document);
            StringAssert.Contains(string.Join("\n", new string[] {
                "<g style=\"" + BlackFill + "\" data-m=\"1\">",
                "<g data-c=\"3\">",
                "<g data-s=\"1\">",
                "<circle data-o=\"1\" cx=\"2000\" r=\"1000\"/>",
                "<circle data-o=\"2\" cx=\"4000\" r=\"1000\"/>",
                "</g>",
                "<g data-s=\"2\">",
                "<circle data-o=\"3\" cx=\"6000\" r=\"1000\"/>",
                "</g>",
                "</g>",
                "<g data-c=\"5\">",
                "<g data-s=\"1\">",
                "<circle data-o=\"1\" cx=\"2000\" r=\"1000\"/>",
                "</g>",
                "</g>",
                "</g>",
            }) + "\n", document);
        }

        [Test]
        public void ObjectsOutsideRunsSitInTheirMapAndASymbolGroupClosesWithWhatHoldsIt()
        {
            string document = WriteOnePage(target => {
                object brush = new object(), image = new object(), photo = new object(), purple = new object(), course = new object(), start = new object();
                target.CreateSolidBrush(brush, CmykColor.FromCmyk(0, 0, 0, 1));
                DrawObject(target, brush, photo, image, "", "Image", 2);
                target.BeginColorRun(purple, 0, "Purple", CmykColor.FromCmyk(0.35F, 0.85F, 0, 0), true);
                Assert.IsTrue(target.PushBlending(BlendMode.Darken));
                DrawObject(target, brush, start, course, "701.0", "Start", 4);
                target.PopBlending();
                target.EndColorRun();
            });
            StringAssert.Contains("<npp:symbol n=\"1\" name=\"Image\"/>\n<npp:symbol n=\"2\" id=\"701.0\" name=\"Start\" colors=\"0\"/>\n", document);
            StringAssert.Contains(string.Join("\n", new string[] {
                "<g style=\"" + BlackFill + "\" data-m=\"1\">",
                "<g data-s=\"1\">",
                "<circle data-o=\"1\" cx=\"2000\" r=\"1000\"/>",
                "</g>",
                "<g data-c=\"0\">",
                "<g style=\"mix-blend-mode:darken\" data-s=\"2\">",
                "<circle data-o=\"2\" cx=\"4000\" r=\"1000\"/>",
                "</g>",
                "</g>",
                "</g>",
            }) + "\n", document);
        }

        [Test]
        public void AStyleSitsOnTheOutermostGroupWhoseElementsAllShareIt()
        {
            string document = WriteOnePage(target => {
                object red = new object(), green = new object(), first = new object(), second = new object();
                object contour = new object(), cliff = new object(), knoll = new object();
                object one = new object(), two = new object(), three = new object(), four = new object();
                target.CreateSolidBrush(red, CmykColor.FromCmyk(0, 1, 1, 0));
                target.CreateSolidBrush(green, CmykColor.FromCmyk(1, 0, 1, 0));
                target.BeginColorRun(first, 1, "First", CmykColor.FromCmyk(0, 1, 1, 0), false);
                DrawObject(target, red, one, contour, "101.0", "Contour", 2);
                DrawObject(target, red, two, cliff, "201.0", "Cliff", 4);
                target.EndColorRun();
                target.BeginColorRun(second, 2, "Second", CmykColor.FromCmyk(1, 0, 1, 0), false);
                DrawObject(target, red, three, contour, "101.0", "Contour", 6);
                DrawObject(target, green, four, knoll, "301.0", "Knoll", 8);
                target.EndColorRun();
            });
            StringAssert.Contains(string.Join("\n", new string[] {
                "<g data-m=\"1\">",
                "<g style=\"fill:#f00;fill:#f00 icc-color(cmyk,0,1,1,0)\" data-c=\"1\">",
                "<g data-s=\"1\">",
                "<circle data-o=\"1\" cx=\"2000\" r=\"1000\"/>",
                "</g>",
                "<g data-s=\"2\">",
                "<circle data-o=\"2\" cx=\"4000\" r=\"1000\"/>",
                "</g>",
                "</g>",
                "<g data-c=\"2\">",
                "<g style=\"fill:#f00;fill:#f00 icc-color(cmyk,0,1,1,0)\" data-s=\"1\">",
                "<circle data-o=\"3\" cx=\"6000\" r=\"1000\"/>",
                "</g>",
                "<g style=\"fill:#0f0;fill:#0f0 icc-color(cmyk,1,0,1,0)\" data-s=\"3\">",
                "<circle data-o=\"4\" cx=\"8000\" r=\"1000\"/>",
                "</g>",
                "</g>",
                "</g>",
            }) + "\n", document);
        }

        [Test]
        public void StructureCallsOutOfTurnRaiseErrorsNamingThem()
        {
            using (Svg_GraphicsTarget target = new Svg_GraphicsTarget(DefaultColorConverter.Instance, true))
            using (Matrix matrix = new Matrix()) {
                CmykColor black = CmykColor.FromCmyk(0, 0, 0, 1);
                object part = new object(), map = new object(), run = new object(), item = new object(), symbol = new object();
                target.BeginPart(part);
                AssertError("BeginColorRun: no map is open", () => target.BeginColorRun(run, 0, "Black", black, false));
                AssertError("BeginObject: no map is open", () => target.BeginObject(item, symbol, "1", "One"));

                target.BeginMap(map, "map");
                target.BeginColorRun(run, 0, "Black", black, false);
                AssertError("BeginColorRun: a color run is open already", () => target.BeginColorRun(new object(), 1, "Brown", black, false));
                AssertError("EndObject: no object is open", () => target.EndObject());

                target.BeginObject(item, symbol, "1", "One");
                AssertError("BeginObject: an object is open already", () => target.BeginObject(new object(), symbol, "1", "One"));
                AssertError("EndColorRun: the object begun last has not ended", () => target.EndColorRun());
                target.PushTransform(matrix);
                AssertError("EndObject: the object's drawing left a group opened by PushTransform open", () => target.EndObject());
                target.PopTransform();
                target.EndObject();
                target.EndColorRun();

                IStructuredGraphicsTarget tile = (IStructuredGraphicsTarget)target.CreatePatternBrush(new SizeF(2, 2), 0, 8, 8);
                AssertError("BeginObject: a pattern brush's tile holds drawing alone", () => tile.BeginObject(item, symbol, "1", "One"));
            }
        }

        [Test]
        public void ARecordingPlaysItsStructureBackOntoATargetThatKeepsItAndIsLeftOutElsewhere()
        {
            string drawnDirectly = WriteOnePage(target => DrawRunOfTwoObjects(target));
            string playedBack;
            using (StructuredRecordingGraphicsTarget recording = new StructuredRecordingGraphicsTarget(new SkiaBitmapGraphicsLoader())) {
                DrawRunOfTwoObjects(recording);
                recording.EndRecording();
                CollectionAssert.AreEqual(new string[] { "CreateSolidBrush", "FillEllipse" }, recording.RecordedCallNames);
                playedBack = WriteOnePage(target => recording.Playback(target));
                using (RecordingGraphicsTarget plain = new RecordingGraphicsTarget(new SkiaBitmapGraphicsLoader())) {
                    recording.Playback(plain);
                    plain.EndRecording();
                    CollectionAssert.AreEqual(new string[] { "CreateSolidBrush", "FillEllipse" }, plain.RecordedCallNames);
                }
            }
            Assert.AreEqual(drawnDirectly, playedBack);
            StringAssert.Contains("data-o=\"2\"", playedBack);
        }

        // Draw a run of one color holding two objects of one symbol.
        // target: the target to draw into.
        private static void DrawRunOfTwoObjects(IStructuredGraphicsTarget target)
        {
            object brush = new object(), color = new object(), symbol = new object();
            target.CreateSolidBrush(brush, CmykColor.FromCmyk(0, 0, 0, 1));
            target.BeginColorRun(color, 2, "Black", CmykColor.FromCmyk(0, 0, 0, 1), false);
            DrawObject(target, brush, new object(), symbol, "101.0", "Contour", 2);
            DrawObject(target, brush, new object(), symbol, "101.0", "Contour", 4);
            target.EndColorRun();
        }

        // Assert that an action raises an InvalidOperationException whose message begins with the given text.
        // messageStart: how the message begins.
        // action: the action.
        private static void AssertError(string messageStart, TestDelegate action)
        {
            InvalidOperationException error = Assert.Throws<InvalidOperationException>(action);
            StringAssert.StartsWith(messageStart, error.Message);
        }

        // A map at 1:10,000 in ETRS-TM35FIN, unturned, its origin at (386250, 6671500), with every identification, and
        // the record the metadata holds of it: a step of map is 0.01 m of the real world.
        private static readonly MapGeoreference tm35finGeoreference = new MapGeoreference(10, 0, 0, 10, 386250, 6671500, 3067,
            "+proj=utm +zone=35 +ellps=GRS80 +towgs84=0,0,0,0,0,0,0 +units=m +no_defs", 6005);
        private const string Tm35finRecord =
            "<npp:georeference transform=\"matrix(.01 0 0 .01 386250 6671500)\" epsg=\"3067\" " +
            "proj4=\"+proj=utm +zone=35 +ellps=GRS80 +towgs84=0,0,0,0,0,0,0 +units=m +no_defs\" ocad-grid-zone=\"6005\"/>\n";

        // Structure a document as WriteOnePage does, through any structured target: one part holding one map labeled
        // "map", on a page of 100 by 50 millimetres. Once the part has ended, the map is given each georeference in turn.
        // target: the target to structure the document through.
        // draw: draws within the map.
        // georeferences: where the map lies in the real world, in the order they are given.
        private static void StructureGeoreferencedPage(IStructuredGraphicsTarget target, Action<IStructuredGraphicsTarget> draw,
                                                       params MapGeoreference[] georeferences)
        {
            using (Matrix mapToPage = new Matrix()) {
                object part = new object(), map = new object();
                target.BeginPart(part);
                target.BeginMap(map, "map");
                draw(target);
                target.EndMap();
                target.EndPart();
                foreach (MapGeoreference georeference in georeferences)
                    target.SetGeoreference(map, georeference);

                mapToPage.Translate(0, 50);
                mapToPage.Scale(1, -1);
                target.AddPage(new SizeF(100, 50), new RectangleF(0, 0, 100, 50), mapToPage, new object[] { part }, null);
            }
        }

        // Write the document that StructureGeoreferencedPage structures.
        // draw: draws within the map.
        // georeferences: where the map lies in the real world, in the order they are given.
        private static string WriteGeoreferencedPage(Action<IStructuredGraphicsTarget> draw, params MapGeoreference[] georeferences)
        {
            using (Svg_GraphicsTarget target = new Svg_GraphicsTarget(DefaultColorConverter.Instance, true)) {
                StructureGeoreferencedPage(target, draw, georeferences);
                return target.FinishDocument();
            }
        }

        [Test]
        public void AGeoreferencedMapRecordsTheTransformFromStepsToTheRealWorldAndEveryIdentification()
        {
            StringAssert.Contains("<npp:map n=\"1\" label=\"map\" views=\"page\">\n" + Tm35finRecord + "</npp:map>\n",
                                  WriteGeoreferencedPage(target => { }, tm35finGeoreference));
        }

        [Test]
        public void ATurnedMapKeepsTheDigitsThatHoldAPositionWithinAStepOverTheMapsReach()
        {
            // At 1:15,000 and a grid scale factor of 0.99961234567, a step of map is 0.015 m of the real world: the
            // translation keeps centimetres, and over the 8 m the map reaches the angle keeps six decimals and the scale ten.
            double scale = 15 * 0.99961234567, angle = 1.23456789 * Math.PI / 180;
            MapGeoreference turned = new MapGeoreference(scale * Math.Cos(angle), scale * Math.Sin(angle), -scale * Math.Sin(angle),
                                                         scale * Math.Cos(angle), 386250.123456, 6671500.987654, 0, null, 0);
            StringAssert.Contains("<npp:georeference transform=\"translate(386250.12 6671500.99)rotate(1.234568)scale(.0149941852)\"/>\n",
                                  WriteGeoreferencedPage(target => { }, turned));
        }

        [Test]
        public void IdentificationsAMapLacksAreLeftOutAndAMapWithoutAGeoreferenceRecordsNone()
        {
            MapGeoreference proj4Only = new MapGeoreference(10, 0, 0, 10, 0, 0, 0, " +proj=utm +zone=35 +datum=WGS84 +units=m +no_defs ", 0);
            StringAssert.Contains("<npp:georeference transform=\"scale(.01)\" proj4=\"+proj=utm +zone=35 +datum=WGS84 +units=m +no_defs\"/>\n",
                                  WriteGeoreferencedPage(target => { }, proj4Only));
            MapGeoreference epsgAndGrid = new MapGeoreference(10, 0, 0, 10, 0, 0, 3067, "  ", 6005);
            StringAssert.Contains("<npp:georeference transform=\"scale(.01)\" epsg=\"3067\" ocad-grid-zone=\"6005\"/>\n",
                                  WriteGeoreferencedPage(target => { }, epsgAndGrid));

            // A transform that takes a step of map to a unit of the real world moves nothing, so it is left out (S-9).
            MapGeoreference stepsAsTheyAre = new MapGeoreference(1000, 0, 0, 1000, 0, 0, 0, null, 0);
            StringAssert.Contains("<npp:map n=\"1\" label=\"map\" views=\"page\">\n<npp:georeference/>\n</npp:map>\n",
                                  WriteGeoreferencedPage(target => { }, stepsAsTheyAre));
            StringAssert.Contains("<npp:map n=\"1\" label=\"map\" views=\"page\"/>\n", WriteGeoreferencedPage(target => { }));
        }

        [Test]
        public void AGeoreferenceComesBeforeTheTablesOfItsMap()
        {
            StringAssert.Contains("<npp:map n=\"1\" label=\"map\" views=\"page\">\n" + Tm35finRecord +
                                  "<npp:color position=\"2\" name=\"Black\" cmyk=\"0,0,0,1\"/>\n" +
                                  "<npp:symbol n=\"1\" id=\"101.0\" name=\"Contour\" colors=\"2\"/>\n</npp:map>\n",
                                  WriteGeoreferencedPage(target => DrawRunOfTwoObjects(target), tm35finGeoreference));
        }

        [Test]
        public void AGeoreferenceGoesToTheMapItsKeyNamesAndReplacesAnEarlierOne()
        {
            StringAssert.Contains("<npp:map n=\"1\" label=\"map\" views=\"page\">\n" + Tm35finRecord + "</npp:map>\n",
                                  WriteGeoreferencedPage(target => { }, new MapGeoreference(10, 0, 0, 10, 0, 0, 3067, null, 0), tm35finGeoreference));

            using (Svg_GraphicsTarget target = new Svg_GraphicsTarget(DefaultColorConverter.Instance, true))
            using (Matrix mapToPage = new Matrix()) {
                object part = new object(), baseMap = new object(), courseMap = new object();
                target.BeginPart(part);
                target.BeginMap(baseMap, "base");
                target.EndMap();
                target.BeginMap(courseMap, "course");
                target.SetGeoreference(baseMap, tm35finGeoreference);
                target.EndMap();
                target.EndPart();
                target.AddPage(new SizeF(100, 50), new RectangleF(0, 0, 100, 50), mapToPage, new object[] { part }, null);
                StringAssert.Contains("<npp:map n=\"1\" label=\"base\" views=\"page\">\n" + Tm35finRecord + "</npp:map>\n<npp:map n=\"2\" label=\"course\" views=\"page\"/>\n",
                                      target.FinishDocument());
            }
        }

        [Test]
        public void ARecordingPlaysAGeoreferenceBackWithItsMap()
        {
            string drawnDirectly = WriteGeoreferencedPage(target => DrawRunOfTwoObjects(target), tm35finGeoreference);
            using (StructuredRecordingGraphicsTarget recording = new StructuredRecordingGraphicsTarget(new SkiaBitmapGraphicsLoader())) {
                StructureGeoreferencedPage(recording, target => DrawRunOfTwoObjects(target), tm35finGeoreference);
                recording.EndRecording();
                using (Svg_GraphicsTarget target = new Svg_GraphicsTarget(DefaultColorConverter.Instance, true)) {
                    recording.Playback(target);
                    Assert.AreEqual(drawnDirectly, target.FinishDocument());
                }
            }
        }

        [Test]
        public void AGeoreferenceForAKeyThatNamesNoMapOrOnATileRaisesAnErrorNamingIt()
        {
            using (Svg_GraphicsTarget target = new Svg_GraphicsTarget(DefaultColorConverter.Instance, true)) {
                object part = new object(), map = new object();
                target.BeginPart(part);
                AssertError("SetGeoreference: no map has begun with this key", () => target.SetGeoreference(map, tm35finGeoreference));
                target.BeginMap(map, "map");
                AssertError("SetGeoreference: no map has begun with this key", () => target.SetGeoreference(part, tm35finGeoreference));
                target.SetGeoreference(map, tm35finGeoreference);

                IStructuredGraphicsTarget tile = (IStructuredGraphicsTarget)target.CreatePatternBrush(new SizeF(2, 2), 0, 8, 8);
                AssertError("SetGeoreference: a pattern brush's tile holds drawing alone", () => tile.SetGeoreference(map, tm35finGeoreference));
            }
        }

        [Test]
        public void ABitmapIsDefinedOnceAsAnImageAndPlacedOverEachRectangleItCovers()
        {
            // 40 by 20 pixels over 20 by 10 millimetres is 500 steps a pixel, and over 40 by 40 millimetres 1000 by 2000.
            TestBitmap logo = new TestBitmap("logo", 40, 20, GraphicsBitmapFormat.PNG);
            string document = WriteOnePage(target => {
                target.DrawBitmap(logo, new RectangleF(10, 10, 20, 10), BitmapScaling.MediumQuality);
                target.DrawBitmap(logo, new RectangleF(50, 5, 40, 40), BitmapScaling.HighQuality);
            });
            Assert.AreEqual(OnePageDocumentWithImages(
                NoStyleBlock,
                new string[] {
                    "<g data-m=\"1\">",
                    "<use xlink:href=\"#b\" transform=\"matrix(500 0 0 500 10000 10000)\"/>",
                    "<use xlink:href=\"#b\" transform=\"matrix(1000 0 0 2000 50000 5000)\"/>",
                    "</g>",
                },
                new string[] { "<image id=\"b\" width=\"40\" height=\"20\" xlink:href=\"" + DataUri("logo", GraphicsBitmapFormat.PNG, 0) + "\"/>" },
                "c"), document);
        }

        [Test]
        public void APartOfABitmapIsItsCroppedImageWrittenOnceHoweverOftenItIsDrawn()
        {
            TestBitmap logo = new TestBitmap("logo", 40, 20, GraphicsBitmapFormat.PNG);
            string document = WriteOnePage(target => {
                target.DrawBitmapPart(logo, 5, 5, 10, 10, new RectangleF(1, 2, 3, 4), BitmapScaling.MediumQuality);
                target.DrawBitmapPart(logo, 5, 5, 10, 10, new RectangleF(10, 20, 10, 10), BitmapScaling.MediumQuality);
                target.DrawBitmapPart(logo, 0, 0, 20, 20, new RectangleF(30, 20, 10, 10), BitmapScaling.MediumQuality);
            });
            Assert.AreEqual(OnePageDocumentWithImages(
                NoStyleBlock,
                new string[] {
                    "<g data-m=\"1\">",
                    "<use xlink:href=\"#b\" transform=\"matrix(300 0 0 400 1000 2000)\"/>",
                    "<use xlink:href=\"#b\" transform=\"translate(10000 20000)scale(1000)\"/>",
                    "<use xlink:href=\"#c\" transform=\"matrix(500 0 0 500 30000 20000)\"/>",
                    "</g>",
                },
                new string[] {
                    "<image id=\"b\" width=\"10\" height=\"10\" xlink:href=\"" + DataUri("logo[5,5,10,10]", GraphicsBitmapFormat.PNG, 0) + "\"/>",
                    "<image id=\"c\" width=\"20\" height=\"20\" xlink:href=\"" + DataUri("logo[0,0,20,20]", GraphicsBitmapFormat.PNG, 0) + "\"/>",
                },
                "d"), document);
        }

        [Test]
        public void AJpegBitmapStaysJpegAtTheOptionsQualityAndBitmapsOfOtherFormatsBecomePng()
        {
            string document = WriteOnePage(target => {
                target.DrawBitmap(new TestBitmap("photo", 30, 30, GraphicsBitmapFormat.JPEG), new RectangleF(0, 0, 30, 30), BitmapScaling.HighQuality);
                target.DrawBitmap(new TestBitmap("scan", 30, 30, GraphicsBitmapFormat.TIFF), new RectangleF(30, 0, 30, 30), BitmapScaling.HighQuality);
                target.DrawBitmap(new TestBitmap("drawn", 30, 30, GraphicsBitmapFormat.None), new RectangleF(60, 0, 30, 30), BitmapScaling.HighQuality);
            }, true, new SvgBitmapOptions(SvgBitmapHandling.Embedded, 75, null));
            StringAssert.Contains("<image id=\"b\" width=\"30\" height=\"30\" xlink:href=\"" + DataUri("photo", GraphicsBitmapFormat.JPEG, 75) + "\"/>\n" +
                                  "<image id=\"c\" width=\"30\" height=\"30\" xlink:href=\"" + DataUri("scan", GraphicsBitmapFormat.PNG, 0) + "\"/>\n" +
                                  "<image id=\"d\" width=\"30\" height=\"30\" xlink:href=\"" + DataUri("drawn", GraphicsBitmapFormat.PNG, 0) + "\"/>\n",
                                  document);
            StringAssert.Contains("data:image/jpeg;base64," + Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes("photo as JPEG at 75")), document);
        }

        [Test]
        public void NearestNeighborScalingRendersPixelatedAndTheOtherScalingsSmooth()
        {
            TestBitmap logo = new TestBitmap("logo", 40, 20, GraphicsBitmapFormat.PNG);
            string document = WriteOnePage(target => {
                target.DrawBitmap(logo, new RectangleF(10, 10, 20, 10), BitmapScaling.NearestNeighbor);
                target.DrawBitmap(logo, new RectangleF(10, 30, 20, 10), BitmapScaling.MediumQuality);
                target.DrawBitmap(logo, new RectangleF(50, 30, 20, 10), BitmapScaling.NearestNeighbor);
            });
            Assert.AreEqual(OnePageDocumentWithImages(
                NoStyleBlock,
                new string[] {
                    "<g data-m=\"1\">",
                    "<use style=\"image-rendering:optimizeSpeed;image-rendering:pixelated\" xlink:href=\"#b\" transform=\"matrix(500 0 0 500 10000 10000)\"/>",
                    "<use xlink:href=\"#b\" transform=\"matrix(500 0 0 500 10000 30000)\"/>",
                    "<use style=\"image-rendering:optimizeSpeed;image-rendering:pixelated\" xlink:href=\"#b\" transform=\"matrix(500 0 0 500 50000 30000)\"/>",
                    "</g>",
                },
                new string[] { "<image id=\"b\" width=\"40\" height=\"20\" xlink:href=\"" + DataUri("logo", GraphicsBitmapFormat.PNG, 0) + "\"/>" },
                "c"), document);
        }

        [Test]
        public void ABitmapDrawnWithinAnObjectCarriesTheObjectsNumber()
        {
            string document = WriteOnePage(target => {
                object image = new object(), symbol = new object();
                target.BeginObject(image, symbol, "1.0", "Image");
                target.DrawBitmap(new TestBitmap("logo", 40, 20, GraphicsBitmapFormat.PNG), new RectangleF(10, 10, 20, 10), BitmapScaling.MediumQuality);
                target.EndObject();
            });
            StringAssert.Contains("<use data-o=\"1\" xlink:href=\"#b\" transform=\"matrix(500 0 0 500 10000 10000)\"/>\n", document);
        }

        [Test]
        public void LinkedBitmapsAreStoredOnceBesideTheDocumentAndLeftOutBitmapsAreNotWritten()
        {
            List<string> stored = new List<string>();
            SvgBitmapOptions linked = new SvgBitmapOptions(SvgBitmapHandling.Linked, 90, (data, format) => {
                stored.Add(System.Text.Encoding.ASCII.GetString(data) + " " + format);
                return "map-files/" + stored.Count + (format == GraphicsBitmapFormat.JPEG ? ".jpg" : ".png");
            });
            TestBitmap logo = new TestBitmap("logo", 40, 20, GraphicsBitmapFormat.PNG);
            Action<Svg_GraphicsTarget> drawTwice = target => {
                target.DrawBitmap(logo, new RectangleF(10, 10, 20, 10), BitmapScaling.MediumQuality);
                target.DrawBitmap(logo, new RectangleF(10, 30, 20, 10), BitmapScaling.MediumQuality);
            };

            string document = WriteOnePage(drawTwice, true, linked);
            CollectionAssert.AreEqual(new string[] { "logo as PNG PNG" }, stored);
            StringAssert.Contains("<image id=\"b\" width=\"40\" height=\"20\" xlink:href=\"map-files/1.png\"/>\n", document);

            Assert.AreEqual(WriteOnePage(target => { }),
                            WriteOnePage(drawTwice, true, new SvgBitmapOptions(SvgBitmapHandling.LeftOut, 90, null)));
        }

        [Test]
        public void ABitmapDrawnBelowFullIntensityRaisesAnErrorNamingTheCall()
        {
            WriteOnePage(target => {
                TestBitmap logo = new TestBitmap("logo", 40, 20, GraphicsBitmapFormat.PNG);
                target.Intensity = 0.5F;
                Svg_UnsupportedCallException error = Assert.Throws<Svg_UnsupportedCallException>(
                    () => target.DrawBitmap(logo, new RectangleF(10, 10, 20, 10), BitmapScaling.MediumQuality));
                Assert.AreEqual("DrawBitmap", error.CallName);
                error = Assert.Throws<Svg_UnsupportedCallException>(
                    () => target.DrawBitmapPart(logo, 0, 0, 10, 10, new RectangleF(10, 10, 20, 10), BitmapScaling.MediumQuality));
                Assert.AreEqual("DrawBitmapPart", error.CallName);
                Assert.AreEqual(0, logo.Writes);
            });
        }

        [Test]
        public void BitmapOptionsRefuseAQualityOutOfRangeAndLinkingWithoutAStore()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new SvgBitmapOptions(SvgBitmapHandling.Embedded, 101, null));
            Assert.Throws<ArgumentOutOfRangeException>(() => new SvgBitmapOptions(SvgBitmapHandling.Embedded, -1, null));
            Assert.Throws<ArgumentNullException>(() => new SvgBitmapOptions(SvgBitmapHandling.Linked, 90, null));
        }

        [Test]
        public void TheSameDrawingWritesTheSameText()
        {
            Action<Svg_GraphicsTarget> draw = target => {
                object brush = new object();
                target.CreateSolidBrush(brush, CmykColor.FromCmyk(0.1F, 0.2F, 0.3F, 0.4F));
                target.FillEllipse(brush, Point(1, 2), 3, 4);
            };
            Assert.AreEqual(WriteOnePage(draw), WriteOnePage(draw));
        }

        [Test]
        public void APopThatClosesNoGroupOfItsPushRaisesAnErrorNamingIt()
        {
            InvalidOperationException error = Assert.Throws<InvalidOperationException>(() => WriteOnePage(target => target.PopTransform()));
            StringAssert.StartsWith("PopTransform closes a group opened by PushTransform", error.Message);

            error = Assert.Throws<InvalidOperationException>(() => WriteOnePage(target => {
                using (Matrix identity = new Matrix())
                    target.PushTransform(identity);
                target.PopAntiAliasing();
            }));
            StringAssert.StartsWith("PopAntiAliasing closes a group opened by PushAntiAliasing, but the innermost open group is a group opened by PushTransform", error.Message);
        }

        // Draw text with the test metrics and font subsets, and return the document.
        // draw: draws within the map.
        // fontSubsets: the font subsets, or null to embed no fonts.
        private static string WriteText(Action<Svg_GraphicsTarget> draw, IFontSubsetProvider fontSubsets)
        {
            return WriteOnePage(draw, true, null, new TestTextMetrics(), fontSubsets);
        }

        // The rule that every text element follows, as the style block writes it after the fonts.
        private const string TextRule = "text{font-kerning:none;font-variant-ligatures:none;font-variant-emoji:text;white-space:pre}";

        // The style of black text in a font.
        // font: the font's declarations.
        private static string BlackText(string font)
        {
            return font + ";" + BlackFill;
        }

        [Test]
        public void TextStartsItsBaselineAtTheUpperLeftCornerLoweredByTheAscent()
        {
            TestFontSubsets subsets = new TestFontSubsets();
            string document = WriteText(target => {
                object font = new object(), black = new object();
                target.CreateFont(font, "Arial", 2.5F, TextEffects.Regular);
                target.CreateSolidBrush(black, CmykColor.FromCmyk(0, 0, 0, 1));
                target.DrawText("Map", font, black, Point(10, 20));
            }, subsets);
            Assert.AreEqual(OnePageDocument(
                new string[] {
                    "@font-face{font-family:'Arial';src:url(" + TestFontSubsets.DataUri("Arial", TextEffects.Regular, "Map") + ")}",
                    TextRule,
                },
                new string[] {
                    "<g style=\"" + BlackText("font-size:2.5px;font-family:'Arial'") + "\" data-m=\"1\">",
                    "<text transform=\"translate(10000 22000)scale(1000)\" x=\"0 1.25 2.5\">Map</text>",
                    "</g>",
                }, drawsText: true), document);
            CollectionAssert.AreEqual(new string[] { "Arial Regular Map" }, subsets.Requests);
        }

        [Test]
        public void TextPlacesEachCharacterWhereTheApplicationsShapingStartsIt()
        {
            string document = WriteText(target => {
                object font = new object(), black = new object();
                target.CreateFont(font, "Arial", 2.5F, TextEffects.Regular);
                target.CreateSolidBrush(black, CmykColor.FromCmyk(0, 0, 0, 1));
                target.DrawText("AVA", font, black, Point(0, 0));
                target.DrawText("AVX", font, black, Point(0, 10));
                target.DrawText("XYZ", font, black, Point(0, 20));
            }, null);
            StringAssert.Contains("<text transform=\"translate(0 2000)scale(1000)\" x=\"0 1 2.05\">AVA</text>\n", document);
            StringAssert.Contains("<text transform=\"translate(0 12000)scale(1000)\" x=\"0 1 2.25\">AVX</text>\n", document);
            StringAssert.Contains("<text transform=\"translate(0 22000)scale(1000)\" x=\"0 1.25 2.5\">XYZ</text>\n", document);
        }

        [Test]
        public void AMarkKeepsItsPlaceByItsLetterAndTheClustersAfterItAreLaidOutByATspan()
        {
            string document = WriteText(target => {
                object font = new object(), black = new object();
                target.CreateFont(font, "Arial", 2.5F, TextEffects.Regular);
                target.CreateSolidBrush(black, CmykColor.FromCmyk(0, 0, 0, 1));
                target.DrawText("x\u0301AV", font, black, Point(0, 0));
            }, null);
            StringAssert.Contains("<text transform=\"translate(0 2000)scale(1000)\">x\u0301<tspan x=\"1.25 2.25\">AV</tspan></text>\n", document);
        }

        [Test]
        public void TextOverTenMillimetresKeepsItsSizeThoughBrowsersCapTheFontSize()
        {
            string document = WriteText(target => {
                object font = new object(), black = new object();
                target.CreateFont(font, "Arial", 20, TextEffects.Regular);
                target.CreateSolidBrush(black, CmykColor.FromCmyk(0, 0, 0, 1));
                target.DrawText("Big", font, black, Point(0, 0));
            }, null);
            StringAssert.Contains("<g style=\"" + BlackText("font-size:20px;font-family:'Arial'") + "\" data-m=\"1\">\n" +
                                  "<text transform=\"translate(0 16000)scale(1000)\" x=\"0 10 20\">Big</text>\n", document);
        }

        [Test]
        public void TextPaintedWithAPatternIsNotHonoredYet()
        {
            using (Svg_GraphicsTarget target = new Svg_GraphicsTarget(DefaultColorConverter.Instance, true, SvgBitmapOptions.Default, new TestTextMetrics(), null)) {
                object font = new object(), pattern = new object(), pen = new object();
                target.CreateFont(font, "Arial", 2.5F, TextEffects.Regular);
                using (IBrushTarget tile = target.CreatePatternBrush(new SizeF(2, 2), 0, 8, 8)) {
                    object dot = new object();
                    tile.CreateSolidBrush(dot, CmykColor.FromCmyk(0, 0, 0, 1));
                    tile.FillEllipse(dot, Point(0, 0), 0.25F, 0.25F);
                    tile.FinishBrush(pattern);
                }
                target.CreatePen(pen, pattern, 0.1F, LineCapMode.Flat, LineJoinMode.Miter, 4);
                Assert.AreEqual("DrawText", Assert.Throws<Svg_UnsupportedCallException>(() => target.DrawText("a", font, pattern, Point(0, 0))).CallName);
                Assert.AreEqual("DrawTextOutline", Assert.Throws<Svg_UnsupportedCallException>(() => target.DrawTextOutline("a", font, pen, Point(0, 0))).CallName);
            }
        }

        [Test]
        public void TextKeepsItsSpacesAndEscapesWhatXmlCannotHoldAsItIs()
        {
            string document = WriteText(target => {
                object font = new object(), black = new object();
                target.CreateFont(font, "Arial", 2, TextEffects.Regular);
                target.CreateSolidBrush(black, CmykColor.FromCmyk(0, 0, 0, 1));
                target.DrawText(" a < b & c > d  \u0001", font, black, Point(1, 1));
            }, null);
            StringAssert.Contains("<text transform=\"translate(1000 2600)scale(1000)\" x=\"0 1 2 3 4 5 6 7 8 9 10 11 12 13 14 15\"> a &lt; b &amp; c &gt; d  </text>\n", document);
        }

        [Test]
        public void TextIsComposedAsTheApplicationComposesItBeforeShaping()
        {
            TestFontSubsets subsets = new TestFontSubsets();
            string document = WriteText(target => {
                object font = new object(), black = new object();
                target.CreateFont(font, "Arial", 2, TextEffects.Regular);
                target.CreateSolidBrush(black, CmykColor.FromCmyk(0, 0, 0, 1));
                target.DrawText("Å", font, black, Point(0, 0));
            }, subsets);
            StringAssert.Contains(">Å</text>\n", document);
            CollectionAssert.AreEqual(new string[] { "Arial Regular Å" }, subsets.Requests);
        }

        [Test]
        public void BoldAndItalicFacesAreDeclaredWithTheirWeightAndStyle()
        {
            string document = WriteText(target => {
                object font = new object(), black = new object();
                target.CreateFont(font, "Times New Roman", 3, TextEffects.Bold | TextEffects.Italic);
                target.CreateSolidBrush(black, CmykColor.FromCmyk(0, 0, 0, 1));
                target.DrawText("T", font, black, Point(0, 0));
            }, new TestFontSubsets());
            StringAssert.Contains("@font-face{font-family:'Times New Roman';font-weight:700;font-style:italic;src:url(" +
                                  TestFontSubsets.DataUri("Times New Roman", TextEffects.Bold | TextEffects.Italic, "T") + ")}\n", document);
            StringAssert.Contains("<g style=\"" + BlackText("font-size:3px;font-family:'Times New Roman';font-weight:700;font-style:italic") + "\" data-m=\"1\">\n", document);
        }

        [Test]
        public void TextOutlinesAreStrokedWithThePenAndNotFilled()
        {
            string document = WriteText(target => {
                object font = new object(), pen = new object();
                target.CreateFont(font, "Arial", 2, TextEffects.Regular);
                target.CreatePen(pen, CmykColor.FromCmyk(0, 1, 1, 0), 0.1F, LineCapMode.Flat, LineJoinMode.Miter, 4);
                target.DrawTextOutline("O", font, pen, Point(0, 0));
            }, null);
            StringAssert.Contains("<g style=\"font-size:2px;font-family:'Arial';fill:none;stroke:#f00;stroke:#f00 icc-color(cmyk,0,1,1,0);stroke-width:.1\" data-m=\"1\">\n" +
                                  "<text transform=\"translate(0 1600)scale(1000)\">O</text>\n", document);
        }

        [Test]
        public void AFaceIsEmbeddedOnceForAllTheCharactersDrawnInIt()
        {
            TestFontSubsets subsets = new TestFontSubsets();
            string document = WriteText(target => {
                object small = new object(), large = new object(), bold = new object(), black = new object();
                target.CreateFont(small, "Arial", 2, TextEffects.Regular);
                target.CreateFont(large, "Arial", 4, TextEffects.Regular);
                target.CreateFont(bold, "Arial", 2, TextEffects.Bold);
                target.CreateSolidBrush(black, CmykColor.FromCmyk(0, 0, 0, 1));
                target.DrawText("bab", small, black, Point(0, 0));
                target.DrawText("cab", large, black, Point(0, 10));
                target.DrawText("ab", bold, black, Point(0, 20));
            }, subsets);
            CollectionAssert.AreEqual(new string[] { "Arial Regular abc", "Arial Bold ab" }, subsets.Requests);
            StringAssert.Contains("<text style=\"" + BlackText("font-size:2px;font-family:'Arial'") + "\" ", document);
            StringAssert.Contains("<text style=\"" + BlackText("font-size:4px;font-family:'Arial'") + "\" ", document);
            StringAssert.Contains("<text style=\"" + BlackText("font-size:2px;font-family:'Arial';font-weight:700") + "\" ", document);
        }

        [Test]
        public void AFallbackFaceIsEmbeddedUnderItsOwnFamilyWhichItsRunsName()
        {
            string document = WriteText(target => {
                object font = new object(), black = new object();
                target.CreateFont(font, "Arial", 2, TextEffects.Regular);
                target.CreateSolidBrush(black, CmykColor.FromCmyk(0, 0, 0, 1));
                target.DrawText("ab中文\U0001F600c", font, black, Point(0, 0));
            }, new TestFontSubsets());
            StringAssert.Contains("@font-face{font-family:'Arial';src:url(" + TestFontSubsets.DataUri("Arial", TextEffects.Regular, "abc") + ")}\n", document);
            StringAssert.Contains("@font-face{font-family:'Fallback';src:url(" + TestFontSubsets.DataUri("Fallback", TextEffects.Regular, "中文\U0001F600") + ")}\n", document);
            StringAssert.Contains(">ab<tspan style=\"font-family:'Fallback'\">中文\U0001F600</tspan>c</text>\n", document);
            StringAssert.DoesNotContain("unicode-range", document);
        }

        [Test]
        public void CharactersDrawnOneByOneAlongALineMergeIntoOneTextWithAPositionAndATurnEach()
        {
            string document = WriteText(target => {
                object font = new object(), black = new object();
                target.CreateFont(font, "Arial", 2, TextEffects.Regular);
                target.CreateSolidBrush(black, CmykColor.FromCmyk(0, 0, 0, 1));
                string[] characters = { "a", "b", "c" };
                for (int index = 0; index < characters.Length; ++index) {
                    using (Matrix placement = new Matrix()) {
                        placement.Translate(10 * index, 5);
                        placement.Rotate(30 * index);
                        target.PushTransform(placement);
                        target.DrawText(characters[index], font, black, Point(0, 0));
                        target.PopTransform();
                    }
                }
            }, null);
            StringAssert.Contains("<text transform=\"scale(1000)\" x=\"0 9.2 18.614\" y=\"6.6 6.386 5.8\" rotate=\"0 30 60\">abc</text>\n", document);
            Assert.AreEqual(1, CountOccurrences(document, "<text"));
        }

        [Test]
        public void CharactersScaledOrInAnotherFontAreNotMergedIntoALineText()
        {
            string document = WriteText(target => {
                object font = new object(), other = new object(), black = new object();
                target.CreateFont(font, "Arial", 2, TextEffects.Regular);
                target.CreateFont(other, "Arial", 3, TextEffects.Regular);
                target.CreateSolidBrush(black, CmykColor.FromCmyk(0, 0, 0, 1));
                using (Matrix placement = new Matrix()) {
                    placement.Translate(0, 5);
                    target.PushTransform(placement);
                    target.DrawText("a", font, black, Point(0, 0));
                    target.PopTransform();
                }
                using (Matrix placement = new Matrix()) {
                    placement.Translate(10, 5);
                    target.PushTransform(placement);
                    target.DrawText("b", other, black, Point(0, 0));
                    target.PopTransform();
                }
                using (Matrix placement = new Matrix()) {
                    placement.Translate(20, 5);
                    placement.Scale(2, 2);
                    target.PushTransform(placement);
                    target.DrawText("c", other, black, Point(0, 0));
                    target.PopTransform();
                }
            }, null);
            StringAssert.DoesNotContain("rotate=", document);
            Assert.AreEqual(3, CountOccurrences(document, "<text"));
        }

        [Test]
        public void CharactersThatShapingSetsByTheirNeighboursKeepTextsOfTheirOwnBetweenLineTexts()
        {
            // An Arabic letter, which joins its neighbours; the fraction slash, which makes the digits around it a
            // fraction; and a combining mark, which attaches to the letter before it.
            string[] characters = { "a", "b", "ب", "c", "d", "1", "⁄", "2", "e", "́", "f", "g" };
            string document = WriteText(target => {
                object font = new object(), black = new object();
                target.CreateFont(font, "Arial", 2, TextEffects.Regular);
                target.CreateSolidBrush(black, CmykColor.FromCmyk(0, 0, 0, 1));
                for (int index = 0; index < characters.Length; ++index) {
                    using (Matrix placement = new Matrix()) {
                        placement.Translate(10 * index, 5);
                        placement.Rotate(10 * index);
                        target.PushTransform(placement);
                        target.DrawText(characters[index], font, black, Point(0, 0));
                        target.PopTransform();
                    }
                }
            }, null);
            foreach (string merged in new string[] { "ab", "cd1", "2e", "fg" })
                StringAssert.IsMatch("<text [^>]* rotate=\"[^\"]*\">" + merged + "</text>\n", document);
            Assert.AreEqual(4, CountOccurrences(document, "rotate="));
            Assert.AreEqual(7, CountOccurrences(document, "<text"));
        }

        [Test]
        public void ShapingSetsLettersOfTheSimpleScriptsAndTheirPunctuationAloneButNotMarksJoinersOrJoiningScripts()
        {
            foreach (string character in new string[] { "a", " ", "é", "Ω", "Ж", "Ա", "ა", "–", "→", "中", "あ", "가", "\U00020000" })
                Assert.IsTrue(SvgLineText.IsShapedAlone(character), "U+{0:X4}", char.ConvertToUtf32(character, 0));
            foreach (string character in new string[] { "́", "‍", "‌", "️", "ب", "क", "ำ", "ᄀ", "א", "⁄", "∕", "\U0001F3FB", "\U0001F1EB", "", "\t" })
                Assert.IsFalse(SvgLineText.IsShapedAlone(character), "U+{0:X4}", char.ConvertToUtf32(character, 0));
        }

        [Test]
        public void ARunInAFallbackFaceThatCrossesTheEndOfAListOfPositionsIsClosedAndOpenedAgainAfterIt()
        {
            string document = WriteText(target => {
                object font = new object(), black = new object();
                target.CreateFont(font, "Arial", 2, TextEffects.Regular);
                target.CreateSolidBrush(black, CmykColor.FromCmyk(0, 0, 0, 1));
                target.DrawText("中x́文", font, black, Point(0, 0));
            }, new TestFontSubsets());
            StringAssert.Contains(" x=\"0 1\"><tspan style=\"font-family:'Fallback'\">中</tspan>x<tspan style=\"font-family:'Fallback'\">́</tspan>" +
                                  "<tspan x=\"2\"><tspan style=\"font-family:'Fallback'\">文</tspan></tspan></text>\n", document);
        }

        [Test]
        public void FallbackFacesOfTheSameFamilyAndStyleForTwoFontsGetFamiliesOfTheirOwn()
        {
            string document = WriteText(target => {
                object arial = new object(), tahoma = new object(), black = new object();
                target.CreateFont(arial, "Arial", 2, TextEffects.Regular);
                target.CreateFont(tahoma, "Tahoma", 2, TextEffects.Regular);
                target.CreateSolidBrush(black, CmykColor.FromCmyk(0, 0, 0, 1));
                target.DrawText("中", arial, black, Point(0, 0));
                target.DrawText("文", tahoma, black, Point(0, 10));
            }, new TestFontSubsets());
            StringAssert.Contains("@font-face{font-family:'Fallback';src:url(" + TestFontSubsets.DataUri("Fallback", TextEffects.Regular, "中") + ")}\n", document);
            StringAssert.Contains("@font-face{font-family:'Fallback 2';src:url(" + TestFontSubsets.DataUri("Fallback", TextEffects.Regular, "文") + ")}\n", document);
            StringAssert.Contains("><tspan style=\"font-family:'Fallback'\">中</tspan></text>\n", document);
            StringAssert.Contains("><tspan style=\"font-family:'Fallback 2'\">文</tspan></text>\n", document);
        }

        [Test]
        public void TheEmojiPresentationSelectorIsWrittenAsTheTextPresentationSelector()
        {
            string document = WriteText(target => {
                object font = new object(), black = new object();
                target.CreateFont(font, "Arial", 2, TextEffects.Regular);
                target.CreateSolidBrush(black, CmykColor.FromCmyk(0, 0, 0, 1));
                target.DrawText("a\u2764\uFE0F", font, black, Point(0, 0));
            }, null);
            // Ordinal, since a culture-sensitive search passes over variation selectors.
            Assert.IsTrue(document.Contains(">a\u2764\uFE0E</text>\n", StringComparison.Ordinal));
            Assert.IsFalse(document.Contains('\uFE0F'));
        }

        [Test]
        public void AFaceThatMayNotBeEmbeddedIsReferredToByNameOnly()
        {
            string document = WriteText(target => {
                object font = new object(), black = new object();
                target.CreateFont(font, "Forbidden", 2, TextEffects.Regular);
                target.CreateSolidBrush(black, CmykColor.FromCmyk(0, 0, 0, 1));
                target.DrawText("x", font, black, Point(0, 0));
            }, new TestFontSubsets());
            StringAssert.DoesNotContain("@font-face", document);
            StringAssert.Contains("<style>\n" + TextRule + "\n</style>\n", document);
            StringAssert.Contains("<g style=\"" + BlackText("font-size:2px;font-family:'Forbidden'") + "\" data-m=\"1\">\n", document);
        }

        [Test]
        public void ADocumentThatDrawsTextNamesItsLanguageAsUndetermined()
        {
            string withText = WriteText(target => {
                object font = new object(), black = new object();
                target.CreateFont(font, "Arial", 2, TextEffects.Regular);
                target.CreateSolidBrush(black, CmykColor.FromCmyk(0, 0, 0, 1));
                target.DrawText("x", font, black, Point(0, 0));
            }, null);
            StringAssert.StartsWith(RootOf100By50WithText + "\n", withText);
            string withoutText = WriteOnePage(target => {
                object black = new object();
                target.CreateSolidBrush(black, CmykColor.FromCmyk(0, 0, 0, 1));
                target.FillRectangle(black, new RectangleF(0, 0, 10, 10));
            });
            StringAssert.StartsWith(RootOf100By50 + "\n", withoutText);
        }

        [Test]
        public void TextCarriesTheNumberOfItsObject()
        {
            string document = WriteText(target => {
                object font = new object(), black = new object(), objectKey = new object(), symbolKey = new object();
                target.CreateFont(font, "Arial", 2, TextEffects.Regular);
                target.CreateSolidBrush(black, CmykColor.FromCmyk(0, 0, 0, 1));
                target.BeginObject(objectKey, symbolKey, "1.0", "Text");
                target.DrawText("a", font, black, Point(1, 0));
                target.EndObject();
            }, null);
            StringAssert.Contains("<text data-o=\"1\" transform=\"translate(1000 1600)scale(1000)\">a</text>\n", document);
        }

        [Test]
        public void FontsExistUnderTheirKeys()
        {
            WriteText(target => {
                object font = new object();
                Assert.IsFalse(target.HasFont(font));
                target.CreateFont(font, "Arial", 2, TextEffects.Regular);
                Assert.IsTrue(target.HasFont(font));
                InvalidOperationException error = Assert.Throws<InvalidOperationException>(() => target.CreateFont(font, "Arial", 2, TextEffects.Regular));
                StringAssert.StartsWith("CreateFont: the key has a font already", error.Message);
                error = Assert.Throws<InvalidOperationException>(() => target.DrawText("a", new object(), new object(), Point(0, 0)));
                StringAssert.StartsWith("DrawText: the key has no font", error.Message);
            }, null);
        }

        [Test]
        public void AFamilyIsEscapedForXmlInStyleAttributesAndInTheStyleBlock()
        {
            string document = WriteText(target => {
                object font = new object(), black = new object();
                target.CreateFont(font, "A & \"B\" <C>", 2, TextEffects.Regular);
                target.CreateSolidBrush(black, CmykColor.FromCmyk(0, 0, 0, 1));
                target.DrawText("x", font, black, Point(0, 0));
            }, new TestFontSubsets());
            StringAssert.Contains("@font-face{font-family:'A &amp; \"B\" &lt;C&gt;';src:url(", document);
            StringAssert.Contains("<g style=\"" + BlackText("font-size:2px;font-family:'A &amp; &quot;B&quot; &lt;C&gt;'") + "\" data-m=\"1\">\n", document);
            Assert.DoesNotThrow(() => System.Xml.Linq.XDocument.Parse(document));
        }

        [Test]
        public void FamilyNamesAreQuotedForCssInSingleQuotes()
        {
            Assert.AreEqual("'Arial'", SvgFontFace.QuoteFamily("Arial"));
            Assert.AreEqual("'a\\'b\\\\c\"d'", SvgFontFace.QuoteFamily("a'b\\c\"d"));
        }

        [Test]
        public void DrawingOutsideAPartRaisesAnErrorNamingTheCall()
        {
            using (Svg_GraphicsTarget target = new Svg_GraphicsTarget(DefaultColorConverter.Instance, true)) {
                object brush = new object();
                target.CreateSolidBrush(brush, CmykColor.FromCmyk(0, 0, 0, 1));
                InvalidOperationException error = Assert.Throws<InvalidOperationException>(() => target.FillEllipse(brush, Point(0, 0), 1, 1));
                StringAssert.StartsWith("FillEllipse: no part is open", error.Message);
            }
        }

        [Test]
        public void PagesAreAddedOutsideParts()
        {
            using (Svg_GraphicsTarget target = new Svg_GraphicsTarget(DefaultColorConverter.Instance, true))
            using (Matrix identity = new Matrix()) {
                object part = new object();
                target.BeginPart(part);
                InvalidOperationException error = Assert.Throws<InvalidOperationException>(() =>
                    target.AddPage(new SizeF(10, 10), new RectangleF(0, 0, 10, 10), identity, new object[] { part }, null));
                StringAssert.StartsWith("AddPage: a part is open", error.Message);
            }
        }
    
        // How many times a string occurs in a document.
        // document: the document.
        // part: the string.
        private static int CountOccurrences(string document, string part)
        {
            int count = 0;
            for (int index = document.IndexOf(part, StringComparison.Ordinal); index >= 0; index = document.IndexOf(part, index + part.Length, StringComparison.Ordinal))
                ++count;
            return count;
        }
    }
}
