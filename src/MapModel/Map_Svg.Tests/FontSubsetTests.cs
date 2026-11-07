using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using HarfBuzzSharp;
using Map_SkiaStd;
using NUnit.Framework;
using PurplePen.Graphics2D;
using PurplePen.MapModel;
using RenderingTestCases;
using SkiaSharp;

namespace Map_Svg.Tests
{
    // Tests of the subsets that HarfBuzzSubsetter cuts (T-7): what they hold, that HarfBuzz, as a browser runs it,
    // shapes the text from a subset into the glyphs and positions the application's HarfBuzz shapes it into from the
    // whole face, the face's box under characters the face lacks, and the subsets of symbol fonts and variable fonts.
    [TestFixture]
    public class FontSubsetTests
    {
        // The features the application shapes without, and kerning, which a document turns off in the browser.
        private static readonly string[] FeaturesOff = { "liga", "clig", "dlig", "hlig", "calt", "kern" };

        // The faces and texts whose shaping from the subset is checked against the whole face's, each composed as
        // the application composes text before it shapes it: Latin with marks precomposed and not; characters the
        // face lacks and draws from their decompositions, a space it lacks, drawn with the glyph of U+0020, and
        // U+2011, drawn as U+2010; characters it lacks and draws as its box; the Indic reordering, conjuncts and reph,
        // the Khmer and Myanmar ones, Thai marks; a face of CFF outlines; and characters beyond the Basic Multilingual
        // Plane.
        private static readonly object[] ShapedTexts = {
            new object[] { "LiberationSans-Regular.ttf", "Map \u00C5bo, Gie\u00DFen" },
            new object[] { "LiberationSans-Regular.ttf", "\u00E9 x\u0301 \u00E4\u0301 fi AV" },
            new object[] { "LiberationSans-Regular.ttf", "\u219A \u2190" },
            new object[] { "LiberationSans-Regular.ttf", "a\u2011b\u205Fc" },
            new object[] { "LiberationSans-Regular.ttf", "a\u4E2D\U0002B823b" },
            new object[] { "Caladea-Regular.ttf", "\u0162a" },
            new object[] { "Roboto-Regular.ttf", "\u01D7 \u00DC" },
            new object[] { "fallback/NotoSansDevanagari-Regular.ttf", "\u0915\u093F\u0930\u094D\u0915\u0915\u094D\u0937\u091F\u0902" },
            new object[] { "fallback/NotoSansBengali-Regular.ttf", "\u09B8\u09BF\u09A8\u09CD\u09A4\u09B0\u09CD\u09AE" },
            new object[] { "fallback/NotoSansTamil-Regular.ttf", "\u0B95\u0BBF\u0BA4\u0BC7\u0B95\u0BCD" },
            new object[] { "fallback/NotoSansKhmer-Regular.ttf", "\u1780\u17D2\u1798\u179F\u17C2" },
            new object[] { "fallback/NotoSansMyanmar-Regular.ttf", "\u1000\u103C\u1019\u103E" },
            new object[] { "fallback/NotoSansThai-Regular.ttf", "\u0E01\u0E34\u0E40\u0E14" },
            new object[] { "fallback/NotoSansCJKjp-Regular.otf", "\u4E2D\u6587\u5B57" },
            new object[] { "fallback/NotoSansMath-Regular.ttf", "\U0001D434\U0001D467\u2234" },
        };

        [Test]
        public void ASubsetHoldsTheGlyphsOfItsCharactersTheLicenseAndNoHinting()
        {
            byte[] subset = Cut("LiberationSans-Regular.ttf", "Map");
            OpenTypeFont font = OpenTypeFont.Read(subset, 0);
            Assert.AreEqual(4, OpenTypeFont.ReadUInt16(font.Tables["maxp"], 4), ".notdef, M, a and p");
            foreach (string tag in new string[] { "fpgm", "prep", "cvt ", "gasp", "hdmx", "kern" })
                Assert.IsFalse(font.Tables.ContainsKey(tag), tag);
            Assert.Less(subset.Length, 2000);
            CollectionAssert.IsSubsetOf(new int[] { 0, 1, 2, 3, 4, 5, 6, 13, 14 }, NameIds(font.Tables["name"]));
            using (Shaper shaper = new Shaper(subset)) {
                Assert.IsTrue(shaper.Has('M') && shaper.Has('a') && shaper.Has('p'));
                Assert.IsFalse(shaper.Has('b'));
            }
        }

        [Test]
        public void ASubsetOfCffOutlinesIsCutToo()
        {
            byte[] whole = File.ReadAllBytes(FontPath("fallback/NotoSansCJKjp-Regular.otf"));
            byte[] subset = Cut("fallback/NotoSansCJKjp-Regular.otf", "\u4E2D\u6587");
            Assert.AreEqual(0x4F54544F, OpenTypeFont.Read(subset, 0).SfntVersion, "OTTO");
            Assert.Less(subset.Length, whole.Length / 4);
            using (Shaper shaper = new Shaper(subset)) {
                Assert.IsTrue(shaper.Has('\u4E2D') && shaper.Has('\u6587'));
                Assert.IsFalse(shaper.Has('\u5B57'));
            }
        }

        [TestCaseSource(nameof(ShapedTexts))]
        public void TheSubsetShapesTheTextAsTheWholeFaceDoes(string fontName, string text)
        {
            byte[] whole = File.ReadAllBytes(FontPath(fontName));
            AssertShapesAlike(whole, HarfBuzzSubsetter.Cut(OpenTypeFont.Read(whole, 0), CodePoints(text)), text);
        }

        [Test]
        public void ASymbolFontIsCutWithACharacterMapThatMapsTheTextAsHarfBuzzMapsItInTheWholeFont()
        {
            byte[] whole = SymbolFontFrom("LiberationSans-Regular.ttf");
            const string text = "Map \uF041\uF042";
            using (Shaper shaper = new Shaper(whole))
                Assert.IsTrue(shaper.Has('M') && shaper.Has(0xF04D), "HarfBuzz maps M through the symbol subtable, as U+F04D");
            byte[] subset = HarfBuzzSubsetter.Cut(OpenTypeFont.Read(whole, 0), CodePoints(text));
            Assert.IsTrue(OpenTypeFont.Read(subset, 0).Tables.ContainsKey("cmap"), "the subset has a character map");
            AssertShapesAlike(whole, subset, text);
        }

        [Test]
        public async Task ChromiumDrawsTheTextOfASymbolFontFromItsSubsetAsFromTheWholeFont()
        {
            byte[] whole = SymbolFontFrom("LiberationSans-Regular.ttf");
            const string text = "Map";
            byte[] subset = HarfBuzzSubsetter.Cut(OpenTypeFont.Read(whole, 0), CodePoints(text));
            using (SKBitmap fromWhole = await RenderText("symbol-whole", whole, text))
            using (SKBitmap fromSubset = await RenderText("symbol-subset", subset, text))
            using (SKBitmap fromSentinel = await RenderText("symbol-sentinel", null, text)) {
                string difference = ImageComparison.DescribeDifference("symbol-subset", fromWhole, fromSubset, PixelTolerance.Exact);
                Assert.IsNull(difference, difference);
                Assert.IsNotNull(ImageComparison.DescribeDifference("symbol-sentinel", fromWhole, fromSentinel, PixelTolerance.Exact),
                                 "the text is drawn from the font, not from the sentinel");
            }
        }

        [Test]
        public void CharactersTheFaceLacksAndDrawsAsItsBoxMapToACopyOfItsBox()
        {
            byte[] whole = File.ReadAllBytes(FontPath("LiberationSans-Regular.ttf"));
            byte[] subset = Cut("LiberationSans-Regular.ttf", "a\u4E2D\U0002B823");
            Assert.AreEqual(3, OpenTypeFont.ReadUInt16(OpenTypeFont.Read(subset, 0).Tables["maxp"], 4), ".notdef, a and the box");
            using (Shaper fromWhole = new Shaper(whole))
            using (Shaper fromSubset = new Shaper(subset)) {
                uint box = fromSubset.Glyph('\u4E2D');
                Assert.AreNotEqual(0, box);
                Assert.AreEqual(box, fromSubset.Glyph(0x2B823));
                Assert.AreEqual(fromWhole.Outline(0), fromSubset.Outline(box));
                StringAssert.StartsWith("M", fromSubset.Outline(box), "the box has an outline");
            }
        }

        [Test]
        public void ADefaultIgnorableCharacterTheFaceLacksGetsNoBox()
        {
            byte[] subset = Cut("LiberationSans-Regular.ttf", "a\u2060\uFE0E\U000E0041");
            Assert.AreEqual(2, OpenTypeFont.ReadUInt16(OpenTypeFont.Read(subset, 0).Tables["maxp"], 4), ".notdef and a");
            using (Shaper shaper = new Shaper(subset)) {
                Assert.IsFalse(shaper.Has(0x2060));
                Assert.IsFalse(shaper.Has(0xFE0E));
            }
        }

        [Test]
        public void AFaceOfCffOutlinesKeepsItsCharactersOutsideTheBox()
        {
            byte[] subset = Cut("fallback/NotoSansCJKjp-Regular.otf", "\u4E2Da");
            using (Shaper shaper = new Shaper(subset)) {
                Assert.IsTrue(shaper.Has('\u4E2D'));
                Assert.IsFalse(shaper.Has('a'));
            }
        }

        // Assert that HarfBuzz shapes a text from a subset into the glyphs, clusters, advances and offsets it shapes it
        // into from the whole face, glyphs compared by their outlines.
        // whole: the whole face.
        // subset: the subset.
        // text: the text.
        private static void AssertShapesAlike(byte[] whole, byte[] subset, string text)
        {
            using (Shaper fromWhole = new Shaper(whole))
            using (Shaper fromSubset = new Shaper(subset)) {
                ShapedGlyph[] expected = fromWhole.Shape(text), shaped = fromSubset.Shape(text);
                Assert.AreEqual(expected.Length, shaped.Length, "the count of glyphs");
                for (int index = 0; index < expected.Length; ++index) {
                    string where = "glyph " + index + " of " + expected.Length;
                    Assert.AreEqual(expected[index].Cluster, shaped[index].Cluster, where + ": its cluster");
                    Assert.AreEqual(expected[index].Position, shaped[index].Position, where + ": its advance and offset");
                    Assert.AreEqual(fromWhole.Outline(expected[index].Glyph), fromSubset.Outline(shaped[index].Glyph), where + ": its outline");
                }
            }
        }

        // A symbol font made from a test font: the characters U+0020 to U+00FF it has, mapped under U+F020 to U+F0FF in a
        // character map whose one subtable is for the Windows symbol encoding, as a symbol font maps its characters.
        // name: the test font's file, within the fonts folder.
        private static byte[] SymbolFontFrom(string name)
        {
            byte[] data = File.ReadAllBytes(FontPath(name));
            List<int[]> mappings = new List<int[]>();
            using (Shaper shaper = new Shaper(data)) {
                for (int character = 0x20; character <= 0xFF; ++character) {
                    uint glyph = shaper.Glyph(character);
                    if (glyph != 0)
                        mappings.Add(new int[] { 0xF000 + character, (int)glyph });
                }
            }

            // One subtable of format 4, a segment for each code mapped by a delta, and the segment that ends the table.
            int segments = mappings.Count + 1, start = 12;
            byte[] cmap = new byte[start + 16 + 8 * segments];
            OpenTypeFont.WriteUInt16(cmap, 2, 1);
            OpenTypeFont.WriteUInt16(cmap, 4, 3);
            OpenTypeFont.WriteUInt32(cmap, 8, (uint)start);
            int power = 1, log2 = 0;
            while (power * 2 <= segments) {
                power *= 2;
                ++log2;
            }
            OpenTypeFont.WriteUInt16(cmap, start, 4);
            OpenTypeFont.WriteUInt16(cmap, start + 2, 16 + 8 * segments);
            OpenTypeFont.WriteUInt16(cmap, start + 6, 2 * segments);
            OpenTypeFont.WriteUInt16(cmap, start + 8, 2 * power);
            OpenTypeFont.WriteUInt16(cmap, start + 10, log2);
            OpenTypeFont.WriteUInt16(cmap, start + 12, 2 * segments - 2 * power);
            int ends = start + 14, starts = ends + 2 * segments + 2, deltas = starts + 2 * segments;
            for (int segment = 0; segment < segments; ++segment) {
                int code = segment < mappings.Count ? mappings[segment][0] : 0xFFFF;
                int glyph = segment < mappings.Count ? mappings[segment][1] : 0;
                OpenTypeFont.WriteUInt16(cmap, ends + 2 * segment, code);
                OpenTypeFont.WriteUInt16(cmap, starts + 2 * segment, code);
                OpenTypeFont.WriteUInt16(cmap, deltas + 2 * segment, (glyph - code) & 0xFFFF);
            }

            OpenTypeFont font = OpenTypeFont.Read(data, 0);
            font.Tables["cmap"] = cmap;
            return font.Write();
        }

        // Render a text in Chromium, in a font embedded in the document, or with no font, in the sentinel.
        // name: the name of the document among the run's SVGs.
        // fontData: the font, or null.
        // text: the text.
        private static async Task<SKBitmap> RenderText(string name, byte[] fontData, string text)
        {
            string fontFace = fontData == null ? "" : "@font-face{font-family:\"Test\";src:url(data:font/ttf;base64," + Convert.ToBase64String(fontData) + ")}";
            string document = "<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"160\" height=\"50\"><style>" + fontFace +
                              "text{font-family:\"Test\";font-size:32px;font-kerning:none}</style><text x=\"8\" y=\"38\">" + text + "</text></svg>";
            return await ChromiumRenderer.RenderAsync(LoopResults.SaveSvg(name, document), new Size(160, 50));
        }

        // The variable face of the tests, which TestFonts registers as Roboto Variable.
        private const string VariableFace = "variable/RobotoVariable-Test.ttf";

        [Test]
        public void AVariableFaceIsCutToAStaticFaceAtItsDefaultLocation()
        {
            byte[] whole = File.ReadAllBytes(FontPath(VariableFace));
            const string text = "Hamburgefonstiv 0123";
            byte[] subset = HarfBuzzSubsetter.Cut(OpenTypeFont.Read(whole, 0), CodePoints(text));
            OpenTypeFont font = OpenTypeFont.Read(subset, 0);
            foreach (string tag in new string[] { "fvar", "gvar", "avar", "HVAR" })
                Assert.IsFalse(font.Tables.ContainsKey(tag), tag);
            AssertShapesAlike(whole, subset, text);
        }

        [Test]
        public void AVariableFaceIsCutAtTheLocationOfTheInstanceItsTypefaceIsNamedAfter()
        {
            byte[] whole = File.ReadAllBytes(FontPath(VariableFace));
            OpenTypeFont font = OpenTypeFont.Read(whole, 0);
            font.Location = VariableFont.LocationDrawn(font, "Roboto-Bold");
            const string text = "Hamburgefonstiv";
            byte[] subset = HarfBuzzSubsetter.Cut(font, CodePoints(text));
            Assert.IsFalse(OpenTypeFont.Read(subset, 0).Tables.ContainsKey("fvar"));
            using (Shaper atBold = new Shaper(whole, font.Location))
            using (Shaper fromSubset = new Shaper(subset))
            using (Shaper atDefault = new Shaper(whole)) {
                foreach (char character in text) {
                    Assert.AreEqual(atBold.Metrics(atBold.Glyph(character)), fromSubset.Metrics(fromSubset.Glyph(character)), "{0}: its advance and extents", character);
                    Assert.AreNotEqual(atDefault.Metrics(atDefault.Glyph(character)), fromSubset.Metrics(fromSubset.Glyph(character)), "{0}: not the regular", character);
                }
            }
        }

        [Test]
        public void TheInstanceATypefaceIsNamedAfterIsFoundByItsOwnNameOrByTheNameMadeForIt()
        {
            OpenTypeFont font = OpenTypeFont.Read(File.ReadAllBytes(FontPath(VariableFace)), 0);
            CollectionAssert.AreEquivalent(new Dictionary<string, float> { { "wght", 700 }, { "wdth", 100 } }, VariableFont.LocationDrawn(font, "Roboto-Bold"));
            CollectionAssert.AreEquivalent(new Dictionary<string, float> { { "wght", 400 }, { "wdth", 75 } }, VariableFont.LocationDrawn(font, "Roboto-CondensedRegular"));
            Assert.IsNull(VariableFont.LocationDrawn(font, "Roboto"), "the default instance, as FreeType names it");
            Assert.IsNull(VariableFont.LocationDrawn(OpenTypeFont.Read(File.ReadAllBytes(FontPath("LiberationSans-Regular.ttf")), 0), "LiberationSans"));

            // Without the names of its instances, each instance is known by the name made of name 25 and its subfamily.
            byte[] fvar = font.Tables["fvar"];
            int axisCount = OpenTypeFont.ReadUInt16(fvar, 8), instanceCount = OpenTypeFont.ReadUInt16(fvar, 12), instanceSize = OpenTypeFont.ReadUInt16(fvar, 14);
            int instances = OpenTypeFont.ReadUInt16(fvar, 4) + axisCount * OpenTypeFont.ReadUInt16(fvar, 10);
            Assert.AreEqual(6 + 4 * axisCount, instanceSize, "the instances carry PostScript names");
            byte[] unnamed = new byte[instances + instanceCount * (instanceSize - 2)];
            System.Buffer.BlockCopy(fvar, 0, unnamed, 0, instances);
            for (int instance = 0; instance < instanceCount; ++instance)
                System.Buffer.BlockCopy(fvar, instances + instance * instanceSize, unnamed, instances + instance * (instanceSize - 2), instanceSize - 2);
            OpenTypeFont.WriteUInt16(unnamed, 14, instanceSize - 2);
            font.Tables["fvar"] = unnamed;
            CollectionAssert.AreEquivalent(new Dictionary<string, float> { { "wght", 700 }, { "wdth", 75 } }, VariableFont.LocationDrawn(font, "Roboto-CondensedBold"));
        }

        [Test]
        public async Task ChromiumDrawsAVariableFaceWhereTheApplicationDrawsIt()
        {
            // Loaded from its file, the face draws its default instance, the regular, for the bold too.
            RectangleF area = new RectangleF(0, 0, 120, 30);
            Size pixels = new Size(1200, 300);
            string difference = await TextDrawingComparison.CompareAsync("variable-face", area, pixels, target => {
                object regular = new object(), bold = new object(), black = new object();
                target.CreateFont(regular, "Roboto Variable", 8, TextEffects.Regular);
                target.CreateFont(bold, "Roboto Variable", 8, TextEffects.Bold);
                target.CreateSolidBrush(black, CmykColor.FromCmyk(0, 0, 0, 1));
                target.DrawText("Hamburgefonstiv 0123", regular, black, new PointF(2, 2));
                target.DrawText("Hamburgefonstiv 0123", bold, black, new PointF(2, 16));
            });
            Assert.IsNull(difference, difference);
        }

        // The path of a test font.
        // name: the font's file, within the fonts folder.
        private static string FontPath(string name)
        {
            return Path.Combine(TestContext.CurrentContext.TestDirectory, "fonts", name);
        }

        // Cut a test font to the characters of a text.
        // name: the font's file, within the fonts folder.
        // text: the text.
        private static byte[] Cut(string name, string text)
        {
            return HarfBuzzSubsetter.Cut(OpenTypeFont.Read(File.ReadAllBytes(FontPath(name)), 0), CodePoints(text));
        }

        // The code points of a text.
        // text: the text.
        private static List<int> CodePoints(string text)
        {
            List<int> codePoints = new List<int>();
            for (int index = 0; index < text.Length; index += char.IsSurrogatePair(text, index) ? 2 : 1)
                codePoints.Add(char.ConvertToUtf32(text, index));
            return codePoints;
        }

        // The IDs of the names a name table holds.
        // name: the table.
        private static List<int> NameIds(byte[] name)
        {
            List<int> ids = new List<int>();
            int count = OpenTypeFont.ReadUInt16(name, 2);
            for (int index = 0; index < count; ++index)
                ids.Add(OpenTypeFont.ReadUInt16(name, 6 + 12 * index + 6));
            return ids;
        }

        // A glyph that shaping put out: the glyph, its cluster, and its advance and offset.
        private struct ShapedGlyph
        {
            public uint Glyph;
            public uint Cluster;
            public string Position;
        }

        // A face, both as HarfBuzz shapes with it and as Skia outlines its glyphs.
        private sealed class Shaper : IDisposable
        {
            private readonly Blob blob;
            private readonly Face face;
            private readonly HarfBuzzSharp.Font font;
            private readonly SKTypeface typeface;
            private readonly SKFont outlines;

            // Load a face, at a location of its design space if it is variable.
            // data: the font data.
            // location: the coordinates of its axes, by tag, or null for the default location.
            public Shaper(byte[] data, IDictionary<string, float> location = null)
            {
                blob = Blob.FromStream(new MemoryStream(data));
                face = new Face(blob, 0);
                font = new HarfBuzzSharp.Font(face);
                if (location != null) {
                    List<HarfBuzzVariation> variations = new List<HarfBuzzVariation>();
                    foreach (KeyValuePair<string, float> axis in location)
                        variations.Add(new HarfBuzzVariation { Tag = ((uint)axis.Key[0] << 24) | ((uint)axis.Key[1] << 16) | ((uint)axis.Key[2] << 8) | axis.Key[3], Value = axis.Value });
                    hb_font_set_variations(font.Handle, variations.ToArray(), (uint)variations.Count);
                }
                typeface = SKTypeface.FromData(SKData.CreateCopy(data));
                Assert.IsNotNull(typeface, "Skia loads the face");
                outlines = new SKFont(typeface, 1000) { Hinting = SKFontHinting.None };
            }

            // Whether the face maps a character to a glyph.
            // character: the character.
            public bool Has(int character)
            {
                return Glyph(character) != 0;
            }

            // The glyph the face maps a character to, or 0.
            // character: the character.
            public uint Glyph(int character)
            {
                uint glyph;
                return font.TryGetNominalGlyph(character, out glyph) ? glyph : 0;
            }

            // Shape a text with the features the application shapes with, kerning aside.
            // text: the text.
            public ShapedGlyph[] Shape(string text)
            {
                using (HarfBuzzSharp.Buffer buffer = new HarfBuzzSharp.Buffer()) {
                    buffer.AddUtf16(text);
                    buffer.GuessSegmentProperties();
                    Feature[] features = new Feature[FeaturesOff.Length];
                    for (int index = 0; index < FeaturesOff.Length; ++index)
                        features[index] = new Feature(new Tag(FeaturesOff[index][0], FeaturesOff[index][1], FeaturesOff[index][2], FeaturesOff[index][3]), 0);
                    font.Shape(buffer, features);
                    GlyphInfo[] infos = buffer.GlyphInfos;
                    HarfBuzzSharp.GlyphPosition[] positions = buffer.GlyphPositions;
                    ShapedGlyph[] shaped = new ShapedGlyph[infos.Length];
                    for (int index = 0; index < infos.Length; ++index) {
                        shaped[index] = new ShapedGlyph {
                            Glyph = infos[index].Codepoint,
                            Cluster = infos[index].Cluster,
                            Position = positions[index].XAdvance + " " + positions[index].YAdvance + " " + positions[index].XOffset + " " + positions[index].YOffset,
                        };
                    }
                    return shaped;
                }
            }

            // A glyph's advance and extents in HarfBuzz, at the face's location.
            // glyph: the glyph.
            public string Metrics(uint glyph)
            {
                GlyphExtents extents;
                font.TryGetGlyphExtents(glyph, out extents);
                return font.GetHorizontalGlyphAdvance(glyph) + " " + extents.XBearing + " " + extents.YBearing + " " + extents.Width + " " + extents.Height;
            }

            // The outline of a glyph, unhinted, as SVG path data.
            // glyph: the glyph.
            public string Outline(uint glyph)
            {
                using (SKPath path = outlines.GetGlyphPath((ushort)glyph))
                    return path == null ? "" : path.ToSvgPathData();
            }

            // hb_variation_t: the coordinate of one axis.
            [StructLayout(LayoutKind.Sequential)]
            private struct HarfBuzzVariation
            {
                public uint Tag;
                public float Value;
            }

            [DllImport("libHarfBuzzSharp", CallingConvention = CallingConvention.Cdecl)]
            private static extern void hb_font_set_variations(IntPtr font, HarfBuzzVariation[] variations, uint count);

            // Release the face.
            public void Dispose()
            {
                outlines.Dispose();
                typeface.Dispose();
                font.Dispose();
                face.Dispose();
                blob.Dispose();
            }
        }
    }
}
