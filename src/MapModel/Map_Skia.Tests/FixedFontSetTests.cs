using System.IO;
using System.Linq;
using Map_SkiaStd;
using NUnit.Framework;
using PurplePen.Graphics2D;
using SkiaSharp;

namespace Map_Skia.Tests
{
    // Tests of the fixed font set, SkiaFontManager.RegisteredFontsOnly: with it on, a family is looked up among the
    // registered font files alone, and so is a character that a text's font lacks, so that no font of the machine's
    // is ever drawn with. Each test registers fonts under names no other test uses, and puts the font manager back as
    // it found it.
    [TestFixture, NonParallelizable]
    public class FixedFontSetTests
    {
        private const string SetFamily = "Fixed Set Sans";
        private const string NarrowFamily = "Fixed Set Narrow";
        private const string RunicFamily = "Fixed Set Runic";

        // Register Roboto, regular and bold, as the set's default family, Roboto Condensed and a runic face beside it,
        // and make them the only fonts there are.
        [SetUp]
        public void UseOnlyTheSet()
        {
            ClearFontState();
            SkiaFontManager.AddFontFile(SetFamily, SKFontStyleWeight.Normal, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright, FontPath("Roboto-Regular.ttf"));
            SkiaFontManager.AddFontFile(SetFamily, SKFontStyleWeight.Bold, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright, FontPath("Roboto-Bold.ttf"));
            SkiaFontManager.AddFontFile(NarrowFamily, SKFontStyleWeight.Normal, SKFontStyleWidth.Condensed, SKFontStyleSlant.Upright, FontPath("RobotoCondensed-Regular.ttf"));
            SkiaFontManager.AddFontFile(RunicFamily, SKFontStyleWeight.Normal, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright, FontPath("NotoSansRunic-Regular.ttf"));
            SkiaFontManager.SetDefaultFontFamilyName(SetFamily);
            SkiaFontManager.RegisteredFontsOnly = true;
        }

        // Let the system's fonts back in, and forget the set.
        [TearDown]
        public void PutTheFontManagerBack()
        {
            SkiaFontManager.RegisteredFontsOnly = false;
            SkiaFontManager.SetDefaultFontFamilyName("Arial");
            ClearFontState();
        }

        // A family the set lacks, even one that most machines have, is drawn in the set's default family, in the style
        // asked for; a family of the set, in its own face.
        [Test]
        public void AFamilyTheSetLacksIsDrawnInTheDefaultFamily()
        {
            foreach (string family in new string[] { "Arial", "Times New Roman", "Segoe UI", "DejaVu Sans", "Arial Narrow Bold", "No Such Family" }) {
                using (SKTypeface typeface = SkiaFontManager.CreateTypeface(family, SKFontStyleWeight.Normal, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright)) {
                    Assert.IsTrue(IsFromFile(typeface, "Roboto-Regular.ttf"), "{0} is drawn in the default family", family);
                }
            }

            using (SKTypeface typeface = SkiaFontManager.CreateTypeface("Arial", SKFontStyleWeight.Bold, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright)) {
                Assert.IsTrue(IsFromFile(typeface, "Roboto-Bold.ttf"), "bold Arial in the default family's bold");
            }

            using (SKTypeface typeface = SkiaFontManager.CreateTypeface(NarrowFamily, SKFontStyleWeight.Normal, SKFontStyleWidth.Condensed, SKFontStyleSlant.Upright)) {
                Assert.IsTrue(IsFromFile(typeface, "RobotoCondensed-Regular.ttf"), "a family of the set in its own face");
            }
        }

        // Only the set's families are installed, and only they are listed.
        [Test]
        public void OnlyTheSetsFamiliesAreInstalled()
        {
            Assert.IsTrue(SkiaFontManager.FontFamilyIsInstalled(SetFamily));
            Assert.IsFalse(SkiaFontManager.FontFamilyIsInstalled("Arial"));
            Assert.IsFalse(SkiaFontManager.FontFamilyIsInstalled("Times New Roman"));
            Assert.IsFalse(SkiaFontManager.FontVariantIsInstalled("Arial", TextEffects.Regular, false));
            Assert.IsFalse(SkiaFontManager.FontVariantIsInstalled(SetFamily, TextEffects.Regular, true), "no font of the system's");
            CollectionAssert.AreEquivalent(new string[] { SetFamily, NarrowFamily, RunicFamily }, SkiaFontManager.GetFontFamilies());
        }

        // A character that the text's font lacks is drawn in the first font of the set that has it, and one that no
        // font of the set has is drawn in none, however many fonts of the machine's have it.
        [Test]
        public void ACharacterIsLookedForInTheSetAlone()
        {
            ShapedTypeface runic = FontFallbackResolver.Resolve(0x16A0, SetFamily, SKFontStyleWeight.Normal, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright);
            Assert.IsNotNull(runic, "a runic letter, which the set's runic face has");
            Assert.IsTrue(IsFromFile(runic.Typeface, "NotoSansRunic-Regular.ttf"));

            Assert.IsNull(FontFallbackResolver.Resolve(0x4E2D, SetFamily, SKFontStyleWeight.Normal, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright),
                          "a Chinese character, which no font of the set has");
        }

        // Of the family whose font has the character first, the face nearest the style the text asked for draws it: a
        // Greek letter in bold runic text, which the runic face lacks, is drawn in Roboto Bold, though Roboto Regular,
        // registered before it, has the letter too.
        [Test]
        public void ACharacterIsDrawnInTheFacesNearestTheStyleAskedFor()
        {
            ShapedTypeface alpha = FontFallbackResolver.Resolve(0x03B1, RunicFamily, SKFontStyleWeight.Bold, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright);
            Assert.IsNotNull(alpha);
            Assert.IsTrue(IsFromFile(alpha.Typeface, "Roboto-Bold.ttf"));
        }

        // The path of one of the test fonts.
        // fileName: the font's file name.
        private static string FontPath(string fileName)
        {
            return Path.Combine(Path.GetDirectoryName(typeof(FixedFontSetTests).Assembly.Location), "fonts", fileName);
        }

        // Whether a typeface was read from one of the test fonts, told by its head table, which holds the font's
        // checksum and dates and so tells two files apart whatever family name the platform reads from them.
        // typeface: the typeface.
        // fileName: the test font's file name.
        private static bool IsFromFile(SKTypeface typeface, string fileName)
        {
            const uint headTable = 0x68656164;
            using (SKTypeface expected = SKTypeface.FromFile(FontPath(fileName))) {
                return typeface.GetTableData(headTable).SequenceEqual(expected.GetTableData(headTable));
            }
        }

        // Forget every private font, and every face resolved from them.
        private static void ClearFontState()
        {
            SkiaFontManager.ClearPrivateFonts();
            ShapedTypeface.ClearCache();
        }
    }
}
