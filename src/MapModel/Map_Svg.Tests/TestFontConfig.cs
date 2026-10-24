using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;

namespace Map_Svg.Tests
{
    // Gives the tests' own process, on Linux, a fontconfig of its own before any font is looked up: one that offers
    // the fallback faces kept in fonts/fallback and no font of the machine's. The Skia target draws a character that a
    // text's face lacks in a face the platform's font fallback finds, among the fonts fontconfig offers, so without
    // this a reference drew such characters in whatever the machine had, and fontfallback's differed from one
    // machine to another. A family the tests do not register is not found either, and falls back to Arial on every
    // machine. The variable is set through the C library, as .NET keeps its own copy of the environment, which the
    // native fontconfig does not read; and from a module initializer, so that it is set before the first test case
    // source or fixture of this assembly runs.
    internal static class TestFontConfig
    {
        // Point FONTCONFIG_FILE at a configuration offering the tests' fallback faces alone.
        [ModuleInitializer]
        internal static void OfferOnlyTheTestsFallbackFaces()
        {
            if (!OperatingSystem.IsLinux())
                return;

            string folder = Path.Combine(AppContext.BaseDirectory, "fonts", "fallback");
            string configuration = Path.Combine(Path.GetTempPath(), "svg-tests-fallback-" + Environment.ProcessId + ".conf");
            File.WriteAllText(configuration, "<?xml version=\"1.0\"?>\n<!DOCTYPE fontconfig SYSTEM \"fonts.dtd\">\n<fontconfig>\n" +
                                              "  <dir>" + SecurityElement.Escape(folder) + "</dir>\n" +
                                              "  <cachedir>" + SecurityElement.Escape(Path.Combine(Path.GetTempPath(), "svg-tests-fallback-cache")) + "</cachedir>\n" +
                                              "</fontconfig>\n");
            if (setenv("FONTCONFIG_FILE", configuration, 1) != 0)
                throw new InvalidOperationException("FONTCONFIG_FILE could not be set: error " + Marshal.GetLastPInvokeError() + ".");
        }

        // Set a variable of the process's environment, as the C library and the native code that reads it see it.
        // name: the variable's name.
        // value: its value.
        // overwrite: non-zero to replace a value it has.
        [DllImport("libc", SetLastError = true)]
        private static extern int setenv(string name, string value, int overwrite);
    }
}
