using System.IO;
using Map_SkiaStd;
using NUnit.Framework;
using SkiaSharp;

namespace Map_Svg.Tests
{
    // Registers the fonts that the tests draw text in, before any test runs, so that the Skia target finds them
    // whatever fonts the machine has: Liberation Sans as Arial, Liberation Serif as Times New Roman, Caladea as Cambria,
    // Carlito as Calibri, and Roboto and Roboto Condensed under their own names. Arial is the family every other family
    // falls back to: on Linux the tests' fontconfig offers no font of the machine's (TestFontConfig), where a machine's
    // own would map Cambria and Calibri to Caladea and Carlito where those are installed and to nothing where not.
    [SetUpFixture]
    public class TestFonts
    {
        // Register the fonts kept in the fonts folder beside the tests.
        [OneTimeSetUp]
        public void RegisterFonts()
        {
            string folder = Path.Combine(TestContext.CurrentContext.TestDirectory, "fonts");
            RegisterFamily(folder, "Arial", "LiberationSans");
            RegisterFamily(folder, "Times New Roman", "LiberationSerif");
            RegisterFamily(folder, "Cambria", "Caladea");
            RegisterFamily(folder, "Calibri", "Carlito");
            Register(folder, "Roboto", SKFontStyleWeight.Normal, SKFontStyleWidth.Normal, "Roboto-Regular");
            Register(folder, "Roboto", SKFontStyleWeight.Bold, SKFontStyleWidth.Normal, "Roboto-Bold");
            Register(folder, "Roboto", SKFontStyleWeight.Normal, SKFontStyleWidth.Condensed, "RobotoCondensed-Regular");
            Register(folder, "Roboto", SKFontStyleWeight.Bold, SKFontStyleWidth.Condensed, "RobotoCondensed-Bold");
            SkiaFontManager.SetDefaultFontFamilyName("Arial");
        }

        // Register a family's regular, bold, italic and bold italic faces, from files named <stem>-Regular.ttf and on.
        // folder: the fonts folder.
        // familyName: the family the faces stand for.
        // stem: the start of the files' names.
        private static void RegisterFamily(string folder, string familyName, string stem)
        {
            Register(folder, familyName, SKFontStyleWeight.Normal, SKFontStyleSlant.Upright, stem + "-Regular");
            Register(folder, familyName, SKFontStyleWeight.Bold, SKFontStyleSlant.Upright, stem + "-Bold");
            Register(folder, familyName, SKFontStyleWeight.Normal, SKFontStyleSlant.Italic, stem + "-Italic");
            Register(folder, familyName, SKFontStyleWeight.Bold, SKFontStyleSlant.Italic, stem + "-BoldItalic");
        }

        // Register an upright face of normal or condensed width.
        // folder: the fonts folder.
        // familyName: the family the face stands for.
        // weight, width: the face's weight and width.
        // fileName: the name of its file, without the extension.
        private static void Register(string folder, string familyName, SKFontStyleWeight weight, SKFontStyleWidth width, string fileName)
        {
            SkiaFontManager.AddFontFile(familyName, weight, width, SKFontStyleSlant.Upright, Path.Combine(folder, fileName + ".ttf"));
        }

        // Register a face of normal width.
        // folder: the fonts folder.
        // familyName: the family the face stands for.
        // weight, slant: the face's weight and slant.
        // fileName: the name of its file, without the extension.
        private static void Register(string folder, string familyName, SKFontStyleWeight weight, SKFontStyleSlant slant, string fileName)
        {
            SkiaFontManager.AddFontFile(familyName, weight, SKFontStyleWidth.Normal, slant, Path.Combine(folder, fileName + ".ttf"));
        }
    }
}
