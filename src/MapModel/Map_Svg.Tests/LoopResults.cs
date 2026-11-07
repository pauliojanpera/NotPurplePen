using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using SkiaSharp;

namespace Map_Svg.Tests
{
    // The directory where the SVG loop collects what a run produced besides its test report: every SVG document
    // written, under svg/; the calls each pending case still needs, under pending/; how far each compared image
    // differs, under measurements/; the reference image of a case that has none recorded yet, under references/; for
    // each failed comparison Chromium's image and the difference image, under renderings/ and differences/; and for a
    // failed page of the export, whose reference is drawn anew in every run, that reference too, under course-pages/.
    // The SVG_LOOP_RESULTS environment variable names it; without it, it is svg-loop-results in the test run's work
    // directory. A directory of renderings beside it may keep the images of every comparison.
    public static class LoopResults
    {
        // The directory that keeps Chromium's image of every comparison, passed or failed, as renderings/<name>.png, and
        // every exported page as the application draws it, as course-pages/<name>.png, for the gallery and the tolerance's
        // checks, which then need no Chromium of their own. The SVG_LOOP_RENDERINGS environment variable names it, which
        // may be the results directory itself; the loop keeps it as an artifact of its own, as its images are too many
        // for the results branch. Without it, null, and only the images of failures are kept, with the results.
        public static readonly string RenderingsDirectory = string.IsNullOrEmpty(Environment.GetEnvironmentVariable("SVG_LOOP_RENDERINGS"))
            ? null
            : Path.GetFullPath(Environment.GetEnvironmentVariable("SVG_LOOP_RENDERINGS"));

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

        // Save the calls that a pending case still needs as pending/<name>.txt, one call to a line, followed, after a tab,
        // by why the target will never honor it, when it never will.
        // name: the name of the case.
        // callNames: the names of the calls.
        // whyNeverHonored: why the target will never honor the calls as the case makes them, or null when it does not
        //                  honor them yet.
        public static void SavePendingCase(string name, IEnumerable<string> callNames, string whyNeverHonored)
        {
            string path = Path.Combine(ResultsDirectory, "pending", name + ".txt");
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllLines(path, callNames.Select(callName => whyNeverHonored == null ? callName : callName + "\t" + whyNeverHonored), new UTF8Encoding(false));
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
            SaveImageIn(ResultsDirectory, relativePath, image);
        }

        // Keep an image in the directory of renderings, when there is one.
        // relativePath: the path of the file within the directory.
        // image: the image to keep.
        public static void KeepRendering(string relativePath, SKBitmap image)
        {
            if (RenderingsDirectory != null)
                SaveImageIn(RenderingsDirectory, relativePath, image);
        }

        // Save an image as a PNG file in a directory.
        // directory: the directory.
        // relativePath: the path of the file within the directory.
        // image: the image to save.
        private static void SaveImageIn(string directory, string relativePath, SKBitmap image)
        {
            string path = Path.Combine(directory, relativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            using (SKImage skImage = SKImage.FromBitmap(image))
            using (SKData png = skImage.Encode(SKEncodedImageFormat.Png, 100))
            using (FileStream stream = File.Create(path)) {
                png.SaveTo(stream);
            }
        }
    }
}
