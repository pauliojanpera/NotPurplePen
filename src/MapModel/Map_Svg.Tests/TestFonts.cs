using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Map_SkiaStd;
using SkiaSharp;

namespace Map_Svg.Tests
{
    // Makes the fonts kept beside the tests the only fonts the Skia code draws text in, a fixed font set
    // (SkiaFontManager.RegisteredFontsOnly), so that a document or reference comes out the same on every machine,
    // Windows or Linux, whatever fonts the machine has: Liberation Sans as Arial, Liberation Serif as Times New Roman,
    // Caladea as Cambria, Carlito as Calibri, Roboto and Roboto Condensed under their own names, and Roboto's variable
    // face as Roboto Variable, regular and bold, which, loaded from its file, draws its default instance whichever is
    // asked for. Arial is the family every other family falls back to. A character a text's face lacks is drawn in the
    // first face that has it, in the order registered: the faces above, then the fallback faces, in the order of their
    // files' names. Done from a module initializer, before the first test case source or fixture of this assembly
    // runs, since a face resolved before the set is fixed would stay cached as the machine resolved it.
    internal static class TestFonts
    {
        // Register the fonts kept in the fonts folder beside the tests, and make them the only ones.
        [ModuleInitializer]
        internal static void FixTheFontSet()
        {
            string folder = Path.Combine(AppContext.BaseDirectory, "fonts");
            RegisterFamily(folder, "Arial", "LiberationSans");
            RegisterFamily(folder, "Times New Roman", "LiberationSerif");
            RegisterFamily(folder, "Cambria", "Caladea");
            RegisterFamily(folder, "Calibri", "Carlito");
            Register(folder, "Roboto", SKFontStyleWeight.Normal, SKFontStyleWidth.Normal, "Roboto-Regular");
            Register(folder, "Roboto", SKFontStyleWeight.Bold, SKFontStyleWidth.Normal, "Roboto-Bold");
            Register(folder, "Roboto", SKFontStyleWeight.Normal, SKFontStyleWidth.Condensed, "RobotoCondensed-Regular");
            Register(folder, "Roboto", SKFontStyleWeight.Bold, SKFontStyleWidth.Condensed, "RobotoCondensed-Bold");
            Register(folder, "Roboto Variable", SKFontStyleWeight.Normal, SKFontStyleSlant.Upright, "variable/RobotoVariable-Test");
            Register(folder, "Roboto Variable", SKFontStyleWeight.Bold, SKFontStyleSlant.Upright, "variable/RobotoVariable-Test");
            RegisterFallbackFaces(Path.Combine(folder, "fallback"));
            SkiaFontManager.SetDefaultFontFamilyName("Arial");
            SkiaFontManager.RegisteredFontsOnly = true;
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

        // Register the faces the text falls back to for characters no face above has, each under the name of its file,
        // which no map asks for, in the order of the files' names.
        // folder: the folder of the fallback faces.
        private static void RegisterFallbackFaces(string folder)
        {
            string[] files = Directory.GetFiles(folder)
                .Where(file => file.EndsWith(".ttf", StringComparison.OrdinalIgnoreCase) || file.EndsWith(".otf", StringComparison.OrdinalIgnoreCase))
                .OrderBy(file => Path.GetFileName(file), StringComparer.Ordinal)
                .ToArray();
            foreach (string file in files)
                SkiaFontManager.AddFontFile(Path.GetFileNameWithoutExtension(file), SKFontStyleWeight.Normal, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright, file);
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
