using System;
using System.Collections.Generic;
using System.Drawing;
using NUnit.Framework;
using PurplePen.Graphics2D;
using PurplePen.MapModel;

namespace Map_Svg.Tests
{
    // Checks the documents that the SVG target writes, against expected text (V-1): the layout of maps and pages, the
    // calls it implements, and the errors of calls out of turn (L-1 to L-5, D-12, F-2 to F-4, COL-1 to COL-6).
    [TestFixture]
    public class SvgDocumentTests
    {
        // The start tag of a document whose first page is 100 by 50 millimetres.
        private const string RootOf100By50 =
            "<svg xmlns=\"http://www.w3.org/2000/svg\" xmlns:xlink=\"http://www.w3.org/1999/xlink\" " +
            "xmlns:npp=\"https://pauliojanpera.github.io/NotPurplePen/svg\" width=\"100mm\" height=\"50mm\" viewBox=\"0 0 100 50\">";

        // Draw within one part holding one map labeled "map", show the part on a page of 100 by 50 millimetres that
        // shows the map from (0, 0) to (100, 50), its y axis turned to point down, and return the document.
        // draw: draws within the map.
        // declaresCmykColors: whether colors carry their CMYK values.
        private static string WriteOnePage(Action<Svg_GraphicsTarget> draw, bool declaresCmykColors = true)
        {
            using (Svg_GraphicsTarget target = new Svg_GraphicsTarget(DefaultColorConverter.Instance, declaresCmykColors))
            using (Matrix mapToPage = new Matrix()) {
                object part = new object();
                target.BeginPart(part);
                target.BeginMap(part, "map");
                draw(target);
                target.EndMap();
                target.EndPart();

                mapToPage.Translate(0, 50);
                mapToPage.Scale(1, -1);
                target.AddPage(new SizeF(100, 50), new RectangleF(0, 0, 100, 50), mapToPage, new object[] { part });
                return target.FinishDocument();
            }
        }

        // The document that WriteOnePage writes, given the lines of its style block and the lines within its part.
        // styleLines: the classes, one to a line.
        // partLines: the lines within the part's definition.
        // refersToCmykProfile: whether a color refers to the CMYK profile, which the definitions then declare.
        private static string OnePageDocument(string[] styleLines, string[] partLines, bool refersToCmykProfile = true)
        {
            List<string> lines = new List<string> { RootOf100By50 };
            if (styleLines.Length > 0) {
                lines.Add("<style>");
                lines.AddRange(styleLines);
                lines.Add("</style>");
            }
            lines.AddRange(new string[] {
                "<metadata>", "<npp:document version=\"1\">", "<npp:map n=\"1\" label=\"map\"/>", "</npp:document>", "</metadata>",
                "<defs>"
            });
            if (refersToCmykProfile)
                lines.Add("<color-profile name=\"cmyk\"/>");
            lines.AddRange(new string[] { "<clipPath id=\"b\">", "<path d=\"M0 0h100000v50000H0z\"/>", "</clipPath>", "<g id=\"a\">" });
            lines.AddRange(partLines);
            lines.AddRange(new string[] {
                "</g>", "</defs>",
                "<g transform=\"matrix(.001 0 0-.001 0 50)\" clip-path=\"url(#b)\">", "<use xlink:href=\"#a\"/>", "</g>",
                "</svg>"
            });
            return string.Join("\n", lines) + "\n";
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
                "<npp:map n=\"1\" label=\"map\"/>",
                "</npp:document>",
                "</metadata>",
                "<defs>",
                "<clipPath id=\"b\">",
                "<path d=\"M0 0h100000v50000H0z\"/>",
                "</clipPath>",
                "<g id=\"a\">",
                "</g>",
                "</defs>",
                "<g transform=\"matrix(.001 0 0-.001 0 50)\" clip-path=\"url(#b)\">",
                "<use xlink:href=\"#a\"/>",
                "</g>",
                "</svg>",
            }) + "\n";
            Assert.AreEqual(expected, WriteOnePage(target => { }));
        }

        [Test]
        public void FilledShapesShareTheClassOfTheirBrush()
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
                new string[] {
                    ".a{fill:#f00;fill:#f00 icc-color(cmyk,0,1,1,0)}",
                    ".b{fill:#0f0;fill:#0f0 icc-color(cmyk,1,0,1,0)}",
                },
                new string[] {
                    "<g data-m=\"1\">",
                    "<path class=\"a\" d=\"M10000 10000h20000v5000H10000z\"/>",
                    "<path class=\"b\" d=\"M40000 10000h10000l-5000 8500z\"/>",
                    "<circle class=\"a\" cx=\"60000\" cy=\"25000\" r=\"5000\"/>",
                    "<ellipse class=\"b\" cx=\"80000\" cy=\"25000\" rx=\"8000\" ry=\"4500\"/>",
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
                new string[] { ".a{fill:#71b2e6;fill:#71b2e6 icc-color(cmyk,.455,.2,0,.1);fill-opacity:.25;fill-rule:evenodd}" },
                new string[] { "<g data-m=\"1\">", "<path class=\"a\" d=\"M0 0h10000L0 10000h10000z\"/>", "</g>" }), document);
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
                new string[] { ".a{fill:#ff8000}" },
                new string[] { "<g data-m=\"1\">", "<path class=\"a\" d=\"M1000 2000h3000v4000H1000z\"/>", "</g>" },
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
                new string[] { ".a{fill:#b2e680;fill:#b2e680 icc-color(cmyk,.2,0,.4,.1)}" },
                new string[] { "<g data-m=\"1\">", "<path class=\"a\" d=\"M1000 2000h3000v4000H1000z\"/>", "</g>" }), document);
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
                    target.FillEllipse(black, Point(0, 0), 1, 1);
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
                new string[] { ".a{fill:#000;fill:#000 icc-color(cmyk,0,0,0,1)}" },
                new string[] {
                    "<g data-m=\"1\">",
                    "<g transform=\"translate(20000 30000)\">",
                    "<circle class=\"a\" cx=\"0\" cy=\"0\" r=\"1000\"/>",
                    "</g>",
                    "<g transform=\"translate(20000 30000)rotate(45)\">",
                    "<path class=\"a\" d=\"M-1000-1000h2000v2000h-2000z\"/>",
                    "</g>",
                    "<g transform=\"matrix(1 0 .5 1 0 0)\">",
                    "<circle class=\"a\" cx=\"0\" cy=\"0\" r=\"1000\"/>",
                    "</g>",
                    "<circle class=\"a\" cx=\"5000\" cy=\"5000\" r=\"1000\"/>",
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
                new string[] {
                    ".a{fill:#000;fill:#000 icc-color(cmyk,0,0,0,1)}",
                    ".b{shape-rendering:crispEdges}",
                    ".c{shape-rendering:auto}",
                },
                new string[] {
                    "<g data-m=\"1\">",
                    "<circle class=\"a\" cx=\"1000\" cy=\"1000\" r=\"1000\"/>",
                    "<g class=\"b\">",
                    "<circle class=\"a\" cx=\"3000\" cy=\"3000\" r=\"1000\"/>",
                    "<g class=\"c\">",
                    "<circle class=\"a\" cx=\"5000\" cy=\"5000\" r=\"1000\"/>",
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
                        target.FillEllipse(black, Point(x, 0), 1, 1);
                        target.PopTransform();
                    }
                }
            });
            Assert.AreEqual(OnePageDocument(
                new string[] { ".a{fill:#000;fill:#000 icc-color(cmyk,0,0,0,1)}" },
                new string[] {
                    "<g transform=\"translate(10000 10000)\" data-m=\"1\">",
                    "<circle class=\"a\" cx=\"1000\" cy=\"0\" r=\"1000\"/>",
                    "<circle class=\"a\" cx=\"2000\" cy=\"0\" r=\"1000\"/>",
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
                    target.FillEllipse(black, Point(0, 0), 1, 1);
                }
            });
            StringAssert.Contains("<g transform=\"translate(1000 2000)\" data-m=\"1\">\n<circle class=\"a\" cx=\"0\" cy=\"0\" r=\"1000\"/>\n</g>\n", document);
        }

        [Test]
        public void PagesAfterTheFirstLieToItsRight()
        {
            using (Svg_GraphicsTarget target = new Svg_GraphicsTarget(DefaultColorConverter.Instance, true))
            using (Matrix mapToPage = new Matrix()) {
                object part = new object();
                target.BeginPart(part);
                target.EndPart();
                mapToPage.Translate(0, 50);
                mapToPage.Scale(1, -1);
                target.AddPage(new SizeF(100, 50), new RectangleF(0, 0, 100, 50), mapToPage, new object[] { part });
                target.AddPage(new SizeF(100, 50), new RectangleF(0, 0, 100, 50), mapToPage, new object[] { part });

                string document = target.FinishDocument();
                StringAssert.Contains("<g transform=\"matrix(.001 0 0-.001 0 50)\" clip-path=\"url(#b)\">\n<use xlink:href=\"#a\"/>\n</g>\n", document);
                StringAssert.Contains("<g transform=\"matrix(.001 0 0-.001 110 50)\" clip-path=\"url(#c)\">\n<use xlink:href=\"#a\"/>\n</g>\n", document);
                StringAssert.Contains(RootOf100By50, document);
            }
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
                    target.AddPage(new SizeF(10, 10), new RectangleF(0, 0, 10, 10), identity, new object[] { part }));
                StringAssert.StartsWith("AddPage: a part is open", error.Message);
            }
        }
    }
}
