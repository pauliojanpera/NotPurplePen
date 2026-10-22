using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NUnit.Framework;
using SkiaSharp;

namespace Map_Svg.Tests
{
    // The directory where the SVG loop collects what a run produced besides its test report: every SVG document
    // written, under svg/; the calls each pending case still needs, under pending/; how far each compared image
    // differs, under measurements/; the reference image of a case that has none recorded yet, under references/; and
    // for each failed comparison Chromium's image and the difference image, under renderings/ and differences/. The
    // SVG_LOOP_RESULTS environment variable names it; without it, it is svg-loop-results in the test run's work
    // directory.
    public static class LoopResults
    {
        // The full path of the directory.
        public static readonly string ResultsDirectory = Path.GetFullPath(
            string.IsNullOrEmpty(Environment.GetEnvironmentVariable("SVG_LOOP_RESULTS"))
                ? Path.Combine(TestContext.CurrentContext.WorkDirectory, "svg-loop-results")
                : Environment.GetEnvironmentVariable("SVG_LOOP_RESULTS"));

        // Save an SVG document as svg/<name>.svg, and return its path.
        // name: the name of the document.
        // document: the text of the document.
        public static string SaveSvg(string name, string document)
        {
            string path = Path.Combine(ResultsDirectory, "svg", name + ".svg");
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, document, new UTF8Encoding(false));
            return path;
        }

        // Save the calls that a pending case still needs as pending/<name>.txt, one call to a line.
        // name: the name of the case.
        // callNames: the names of the calls.
        public static void SavePendingCase(string name, IEnumerable<string> callNames)
        {
            string path = Path.Combine(ResultsDirectory, "pending", name + ".txt");
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllLines(path, callNames, new UTF8Encoding(false));
        }

        // Save text in a file.
        // relativePath: the path of the file within the directory.
        // text: the text.
        public static void SaveText(string relativePath, string text)
        {
            string path = Path.Combine(ResultsDirectory, relativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, text, new UTF8Encoding(false));
        }

        // Save an image as a PNG file.
        // relativePath: the path of the file within the directory.
        // image: the image to save.
        public static void SaveImage(string relativePath, SKBitmap image)
        {
            string path = Path.Combine(ResultsDirectory, relativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            using (SKImage skImage = SKImage.FromBitmap(image))
            using (SKData png = skImage.Encode(SKEncodedImageFormat.Png, 100))
            using (FileStream stream = File.Create(path)) {
                png.SaveTo(stream);
            }
        }
    }
}
