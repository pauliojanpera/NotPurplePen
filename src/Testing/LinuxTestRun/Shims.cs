using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;

// The test files name these namespaces although they use nothing of them. The names make the using
// directives of the files resolve without the assemblies that only Windows has.
namespace PurplePen.MapView { }

namespace TestingUtils
{
    // Compares a text file with its baseline. The suite's version shows a dialog where they differ, which
    // an unattended run has no use for.
    public static class TextFileTestUtil
    {
        public static void CompareTextFileBaseline(string newFile, string baseline)
        {
            CompareTextFileBaseline(newFile, baseline, null);
        }

        public static void CompareTextFileBaseline(string newFile, string baseline, Dictionary<string, string> exceptionMap)
        {
            if (!File.ReadAllLines(newFile).SequenceEqual(File.ReadAllLines(baseline)))
                Assert.Fail($"{newFile} and {baseline} do not compare");
        }
    }
}

// Turns a PDF into a PNG with Ghostscript, in place of the converter of the Windows application.
public class GhostscriptPdfConverter : PurplePen.IPdfConverter
{
    public void Convert(float dpi, string sourcePdfFileName, string destinationPngFileName)
    {
        ProcessStartInfo info = new ProcessStartInfo("gs") { UseShellExecute = false };
        foreach (string argument in new string[] {
                "-q", "-dBATCH", "-dNOPAUSE", "-dSAFER", "-sDEVICE=png16m",
                "-r" + dpi.ToString(CultureInfo.InvariantCulture),
                "-sOutputFile=" + destinationPngFileName, sourcePdfFileName }) {
            info.ArgumentList.Add(argument);
        }

        if (!destinationPngFileName.Contains("%d"))
            info.ArgumentList.Insert(info.ArgumentList.Count - 2, "-dLastPage=1");

        using (Process process = Process.Start(info)) {
            process.WaitForExit();
            if (process.ExitCode != 0)
                throw new Exception($"Ghostscript could not convert '{sourcePdfFileName}'.");
        }
    }
}

// Says nothing while a PDF loads, in place of the dialog of the Windows application.
public class SilentPdfLoadingStatus : PurplePen.IPdfLoadingStatus
{
    public bool ShowLoadingStatus(string fileName) => true;
    public void LoadingComplete(bool success, string errorMessage) { }
}
