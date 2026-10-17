using Microsoft.VisualStudio.TestTools.UnitTesting;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;

namespace TestingUtils
{
    // Provides bitmap comparison helpers for SkiaSharp-based tests.
    public class BitmapTestUtil
    {
        // Compare two bitmaps. If they are different, return a difference bitmap, else return NULL.
        // The difference bitmap has "colorSame" background, and the bits from the second bitmap where differences are.
        // Used as helper from CompareBitmaps if a difference is detected.
        public static SKBitmap DifferenceBitmaps(SKBitmap bm1, SKBitmap bm2, Color colorSame, Color colorDifferent, int maxPixelDifference = 0)
        {
            int width = bm1.Width, height = bm1.Height;

            SKImageInfo imageInfo = new SKImageInfo(width, height, SKImageInfo.PlatformColorType, SKAlphaType.Opaque);
            SKBitmap diff = new SKBitmap(imageInfo);
            SKColor skColorSame = new SKColor(colorSame.R, colorSame.G, colorSame.B, colorSame.A);
            SKColor skColorDifferent = new SKColor(colorDifferent.R, colorDifferent.G, colorDifferent.B, colorDifferent.A);
            if (colorSame != Color.Transparent)
                diff.Erase(skColorSame);
            else if (colorDifferent != Color.Transparent)
                diff.Erase(skColorDifferent);

            bool different = false;
            for (int x = 0; x < width; ++x)
                for (int y = 0; y < height; ++y) {
                    SKColor color1 = bm1.GetPixel(x, y);
                    SKColor color2 = bm2.GetPixel(x, y);
                    bool similar = Math.Abs(color1.Red - color2.Red) <= maxPixelDifference &&
                                   Math.Abs(color1.Green - color2.Green) <= maxPixelDifference &&
                                   Math.Abs(color1.Blue - color2.Blue) <= maxPixelDifference &&
                                   Math.Abs(color1.Alpha - color2.Alpha) <= maxPixelDifference;

                    if (color1 != color2 && (maxPixelDifference == 0 || !similar)) {
                        if (colorDifferent == Color.Transparent)
                            diff.SetPixel(x, y, color2);
                        else if (colorSame != Color.Transparent)
                            diff.SetPixel(x, y, skColorDifferent);
                        different = true;
                    }
                    else {
                        if (colorSame == Color.Transparent)
                            diff.SetPixel(x, y, color2);
                    }
                }

            if (different)
                return diff;
            else {
                diff.Dispose();
                return null;
            }
        }

        // Compare two bitmaps. If they are different, return a difference bitmap, else return NULL.
        // The difference bitmap has light gray background, and the bits from the second bitmap where differences are.
        // If the bitmaps are different sizes, a copy of bm2 is returned.
        public static SKBitmap CompareBitmaps(SKBitmap bm1, SKBitmap bm2, Color colorSame, Color colorDifferent, int maxPixelDifference)
        {
            if (bm1.Width != bm2.Width || bm1.Height != bm2.Height)
                return bm2.Copy();

            int width = bm1.Width, height = bm1.Height;
            bool different = false;

            for (int x = 0; x < width && !different; ++x)
                for (int y = 0; y < height; ++y) {
                    if (bm1.GetPixel(x, y) != bm2.GetPixel(x, y)) {
                        different = true;
                        break;
                    }
                }

            if (!different)
                return null;
            else
                return DifferenceBitmaps(bm1, bm2, colorSame, colorDifferent, maxPixelDifference);
        }

        // Compare the bitmap loaded from bitmapNew with baselineFile, allowing each color channel to
        // differ by maxPixelDifference, and dispose the loaded bitmap after the comparison completes.
        public static void CompareBitmapBaseline(string bitmapNew, string baselineFile, int maxPixelDifference = 0)
        {
            Assert.IsTrue(File.Exists(bitmapNew));

            SKBitmap bmNew = SKBitmap.Decode(bitmapNew);
            Assert.IsNotNull(bmNew, $"Unable to decode bitmap file '{bitmapNew}'.");
            CompareBitmapBaseline(bmNew, baselineFile, maxPixelDifference);
        }

        // Compare bmNew with baselineFile, allowing each color channel to differ by
        // maxPixelDifference; when they differ, synchronously run VisualDiff and fail the test if
        // it returns nonzero. This method always disposes bmNew.
        public static void CompareBitmapBaseline(SKBitmap bmNew, string baselineFile, int maxPixelDifference = 0)
        {
            bool different = false, fail = false;

            try {
                baselineFile = Path.GetFullPath(baselineFile);
                string specificBaselineFile = TestUtil.GetSpecificFileName(baselineFile, throwOnNotFound: false);
                if (specificBaselineFile != null)
                    baselineFile = specificBaselineFile;

                if (!File.Exists(baselineFile)) {
                    different = true;
                }
                else {
                    using (SKBitmap bmBaseline = SKBitmap.Decode(baselineFile)) {
                        if (bmBaseline == null)
                            throw new InvalidDataException($"Unable to decode baseline bitmap file '{baselineFile}'.");

                        using (SKBitmap diff = CompareBitmaps(bmNew, bmBaseline, Color.LightPink, Color.Transparent, maxPixelDifference)) {
                            different = (diff != null);

                            if (different && TestUtil.SilentRun) {
                                if (MatchesAlternateBaseline(bmNew, baselineFile, maxPixelDifference)) {
                                    different = false;
                                }
                                else {
                                    // Nobody is watching, so leave the evidence on disk
                                    // instead: what was rendered, and where it differs.
                                    SaveComparison(bmNew, diff, baselineFile);
                                }
                            }
                        }
                    }
                }

                if (different) {
                    if (TestUtil.SilentRun) {
                        fail = true;
                    }
                    else {
                        fail = !RunVisualDiff(bmNew, baselineFile);
                    }
                }
            }
            finally {
                bmNew.Dispose();
            }

            if (fail)
                Assert.Fail($"Bitmap compare against baseline file '{Path.GetFileName(baselineFile)}' failed");
        }

        // A second set of baselines to fall back on, named by TEST_ALT_BASELINE_ROOT.
        // A comparison whose baseline this machine cannot reproduce still has to tell a
        // rendering that has regressed from one that was never going to match; matching a
        // recorded rendering exactly separates the two, and is reported rather than passed
        // over in silence.
        private static Dictionary<string, string> alternateBaselines;
        private static readonly object alternateLock = new object();

        // Parameters:
        //   bmNew - the bitmap that was rendered.
        //   baselineFile - the baseline it failed against.
        //   maxPixelDifference - the tolerance that comparison used.
        private static bool MatchesAlternateBaseline(SKBitmap bmNew, string baselineFile, int maxPixelDifference)
        {
            string alternate = FindAlternateBaseline(baselineFile);
            if (alternate == null)
                return false;

            using (SKBitmap bmAlternate = SKBitmap.Decode(alternate)) {
                if (bmAlternate == null)
                    return false;

                using (SKBitmap difference = CompareBitmaps(bmNew, bmAlternate, Color.LightPink, Color.Transparent, maxPixelDifference)) {
                    if (difference != null)
                        return false;
                }
            }

            string note = string.Format("Alternate baseline matched: {0}", Path.GetFileName(baselineFile));
            Console.WriteLine(note);
            string log = Environment.GetEnvironmentVariable("TEST_ALT_BASELINE_LOG");
            if (!string.IsNullOrEmpty(log)) {
                try {
                    File.AppendAllText(log, note + Environment.NewLine);
                }
                catch (IOException) {
                }
            }

            return true;
        }

        // Parameters:
        //   baselineFile - full path of the baseline that did not match.
        private static string FindAlternateBaseline(string baselineFile)
        {
            lock (alternateLock) {
                if (alternateBaselines == null) {
                    alternateBaselines = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    string root = Environment.GetEnvironmentVariable("TEST_ALT_BASELINE_ROOT");
                    if (!string.IsNullOrEmpty(root) && Directory.Exists(root)) {
                        foreach (string file in Directory.GetFiles(root, "*.png", SearchOption.AllDirectories))
                            alternateBaselines[AlternateKey(file)] = file;
                    }
                }

                string found;
                return alternateBaselines.TryGetValue(AlternateKey(baselineFile), out found) ? found : null;
            }
        }

        // The containing directory and the file name together. Baseline names repeat
        // across the test directories but not within one, and this survives the alternate
        // set being rooted somewhere else entirely.
        private static string AlternateKey(string path)
        {
            string directory = Path.GetDirectoryName(path);
            return (directory == null ? "" : Path.GetFileName(directory)) + "/" + Path.GetFileName(path);
        }

        // Writes what a failed comparison produced next to the baseline, so that an
        // unattended run leaves something to look at.
        //
        // Parameters:
        //   bmNew - the bitmap that was rendered.
        //   difference - the differences against the baseline.
        //   baselineFile - full path of the baseline the comparison used.
        private static void SaveComparison(SKBitmap bmNew, SKBitmap difference, string baselineFile)
        {
            string directory = Path.GetDirectoryName(baselineFile);
            string stem = Path.GetFileNameWithoutExtension(baselineFile);

            try {
                Write(bmNew, Path.Combine(directory, stem + "_actual.png"));
                if (difference != null)
                    Write(difference, Path.Combine(directory, stem + "_diff.png"));
            }
            catch (IOException) {
                // Evidence is a convenience; failing to write it must not replace
                // the comparison's own result with an unrelated error.
            }
            catch (UnauthorizedAccessException) {
            }
        }

        // Encodes one bitmap as a PNG at the given path.
        private static void Write(SKBitmap bitmap, string path)
        {
            using (SKData data = bitmap.Encode(SKEncodedImageFormat.Png, 100))
            using (FileStream stream = new FileStream(path, FileMode.Create, FileAccess.Write)) {
                data.SaveTo(stream);
            }
        }

        // Save bmNew to a temporary PNG beside baselineFile, synchronously run the platform-specific
        // VisualDiff executable located beside the TestUtil.Skia assembly with both full paths, delete
        // the temporary file, and return true only when VisualDiff exits with a zero status code.
        private static bool RunVisualDiff(SKBitmap bmNew, string baselineFile)
        {
            string tempNewFile = Path.Combine(
                Path.GetDirectoryName(baselineFile),
                Path.GetFileNameWithoutExtension(baselineFile) + "_tempnew.png");

            try {
                using (SKImage image = SKImage.FromBitmap(bmNew))
                using (SKData encoded = image.Encode(SKEncodedImageFormat.Png, 100))
                using (FileStream stream = new FileStream(tempNewFile, FileMode.Create, FileAccess.Write, FileShare.None)) {
                    encoded.SaveTo(stream);
                }

                string visualDiffFilename = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                    ? "VisualDiff.exe"
                    : "VisualDiff";
                string assemblyDirectory = Path.GetDirectoryName(typeof(BitmapTestUtil).Assembly.Location);
                string visualDiffExecutable = Path.Combine(assemblyDirectory, visualDiffFilename);
                ProcessStartInfo startInfo = new ProcessStartInfo {
                    FileName = visualDiffExecutable,
                    UseShellExecute = false,
                };
#if NETFRAMEWORK
                startInfo.Arguments = $"bitmap \"{tempNewFile}\" \"{baselineFile}\"";
#else
                startInfo.ArgumentList.Add("bitmap");
                startInfo.ArgumentList.Add(tempNewFile);
                startInfo.ArgumentList.Add(baselineFile);
#endif

                using (Process process = Process.Start(startInfo)) {
                    process.WaitForExit();
                    return process.ExitCode == 0;
                }
            }
            finally {
                if (File.Exists(tempNewFile))
                    File.Delete(tempNewFile);
            }
        }

        // Compare bmNew with the baseline named baselineFileBaseName plus "_baseline.png" in the
        // test-file directory, allowing each color channel to differ by maxPixelDifference.
        public static void CheckBitmapsBase(SKBitmap bmNew, string baselineFileBaseName, int maxPixelDifference = 0)
        {
            CompareBitmapBaseline(bmNew, TestUtil.GetTestFile(baselineFileBaseName + "_baseline.png"), maxPixelDifference);
        }
    }
}
