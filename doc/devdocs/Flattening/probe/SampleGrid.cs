using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SkiaSharp;

namespace SkiaPathOpsProbe
{
    /// <summary>
    /// Draws paths without antialiasing on a grid of samples, and compares the samples of two drawings. A window
    /// that is larger than one bitmap is drawn as a set of bitmaps.
    /// </summary>
    public static class SampleGrid
    {
        /// <summary>The largest width and height of one bitmap, in samples.</summary>
        public const int LargestBitmapSide = 4000;

        /// <summary>Divides a window, in millimetres, into the windows of single bitmaps.</summary>
        public static List<SKRect> DivideWindow(SKRect window, int samplesPerMillimetre)
        {
            float side = (float)LargestBitmapSide / samplesPerMillimetre;
            List<SKRect> parts = new List<SKRect>();
            for (float top = window.Top; top < window.Bottom; top += side)
                for (float left = window.Left; left < window.Right; left += side)
                    parts.Add(new SKRect(left, top, Math.Min(left + side, window.Right), Math.Min(top + side, window.Bottom)));
            return parts;
        }

        /// <summary>Makes an empty bitmap of one byte for each sample, for a window in millimetres.</summary>
        public static SKBitmap CreateBitmap(SKRect window, int samplesPerMillimetre)
        {
            int width = Math.Max(1, (int)Math.Ceiling(window.Width * samplesPerMillimetre));
            int height = Math.Max(1, (int)Math.Ceiling(window.Height * samplesPerMillimetre));
            SKBitmap bitmap = new SKBitmap(new SKImageInfo(width, height, SKColorType.Alpha8, SKAlphaType.Premul));
            bitmap.Erase(SKColors.Transparent);
            return bitmap;
        }

        /// <summary>
        /// Draws paths on a bitmap without antialiasing. Each path sets its samples to a value from 1 to 255, which
        /// valueOfPath gives for the index of the path. A later path replaces an earlier one.
        /// </summary>
        public static void DrawPaths(SKBitmap bitmap, SKRect window, int samplesPerMillimetre, IReadOnlyList<SKPath> paths, Func<int, byte> valueOfPath)
        {
            using (SKCanvas canvas = new SKCanvas(bitmap))
            using (SKPaint paint = new SKPaint()) {
                paint.IsAntialias = false;
                paint.BlendMode = SKBlendMode.Src;
                canvas.Scale(samplesPerMillimetre);
                canvas.Translate(-window.Left, -window.Top);
                for (int i = 0; i < paths.Count; ++i) {
                    if (paths[i] == null || !paths[i].Bounds.IntersectsWith(window))
                        continue;
                    paint.Color = new SKColor(0, 0, 0, valueOfPath(i));
                    canvas.DrawPath(paths[i], paint);
                }
            }
        }

        /// <summary>Draws the area that a set of paths covers: each covered sample gets the value 255.</summary>
        public static SKBitmap DrawCoverage(SKRect window, int samplesPerMillimetre, IReadOnlyList<SKPath> paths)
        {
            SKBitmap bitmap = CreateBitmap(window, samplesPerMillimetre);
            DrawPaths(bitmap, window, samplesPerMillimetre, paths, index => (byte)255);
            return bitmap;
        }

        /// <summary>
        /// Counts the samples of two bitmaps of the same size. differing gets the count of the samples that are not
        /// equal. onlyInFirst gets the count of the samples that are set in the first bitmap and empty in the second.
        /// </summary>
        public static void CompareSamples(SKBitmap first, SKBitmap second, out long differing, out long onlyInFirst)
        {
            ReadOnlySpan<byte> firstSamples = first.GetPixelSpan(), secondSamples = second.GetPixelSpan();
            differing = 0;
            onlyInFirst = 0;
            for (int i = 0; i < firstSamples.Length; ++i) {
                if (firstSamples[i] != secondSamples[i]) {
                    ++differing;
                    if (secondSamples[i] == 0)
                        ++onlyInFirst;
                }
            }
        }

        /// <summary>
        /// Draws two sets of paths over a window and compares their coverage, with one task for each bitmap of the
        /// window. differing gets the count of the samples that differ. thick gets the count of the differing samples
        /// whose eight neighbours differ too: the two drawings of one outline differ in a line of single samples,
        /// so a thick sample shows an area that one drawing has and the other has not. largestGroup gets the size
        /// of the largest group of differing samples that hangs together in one bitmap.
        /// </summary>
        public static void CompareCoverage(SKRect window, int samplesPerMillimetre, IReadOnlyList<SKPath> first, IReadOnlyList<SKPath> second, out long differing, out long thick, out int largestGroup)
        {
            long allDiffering = 0, allThick = 0;
            int largest = 0;
            object gate = new object();
            Parallel.ForEach(DivideWindow(window, samplesPerMillimetre), part => {
                using (SKBitmap firstBitmap = DrawCoverage(part, samplesPerMillimetre, first))
                using (SKBitmap secondBitmap = DrawCoverage(part, samplesPerMillimetre, second)) {
                    ReadOnlySpan<byte> firstSamples = firstBitmap.GetPixelSpan(), secondSamples = secondBitmap.GetPixelSpan();
                    bool[] isDiffering = new bool[firstSamples.Length];
                    for (int i = 0; i < isDiffering.Length; ++i)
                        isDiffering[i] = firstSamples[i] != secondSamples[i];
                    MeasureDifference(isDiffering, firstBitmap.Width, firstBitmap.Height, out long partDiffering, out long partThick, out int partLargest);
                    lock (gate) {
                        allDiffering += partDiffering;
                        allThick += partThick;
                        largest = Math.Max(largest, partLargest);
                    }
                }
            });
            differing = allDiffering;
            thick = allThick;
            largestGroup = largest;
        }

        /// <summary>
        /// Measures the flags of the samples that differ, row by row: their count, the count of the thick ones, and
        /// the size of the largest group that hangs together. The flags are cleared.
        /// </summary>
        public static void MeasureDifference(bool[] isDiffering, int width, int height, out long differing, out long thick, out int largestGroup)
        {
            differing = 0;
            thick = 0;
            for (int y = 0; y < height; ++y) {
                for (int x = 0; x < width; ++x) {
                    int i = y * width + x;
                    if (!isDiffering[i])
                        continue;
                    ++differing;
                    if (x > 0 && y > 0 && x + 1 < width && y + 1 < height
                        && isDiffering[i - width - 1] && isDiffering[i - width] && isDiffering[i - width + 1] && isDiffering[i - 1] && isDiffering[i + 1]
                        && isDiffering[i + width - 1] && isDiffering[i + width] && isDiffering[i + width + 1])
                        ++thick;
                }
            }
            largestGroup = differing == 0 ? 0 : FindLargestGroup(isDiffering, width, height);
        }

        /// <summary>
        /// Counts the thick samples at which a result differs from what its operands give over an area. The operands
        /// give the samples that the paths of covered cover, less the samples that the paths of removed cover.
        /// </summary>
        public static long CountThickSamplesOffExpectation(SKRect area, int samplesPerMillimetre, IReadOnlyList<SKPath> covered, IReadOnlyList<SKPath> removed, SKPath result)
        {
            using (SKBitmap expected = DrawCoverage(area, samplesPerMillimetre, covered))
            using (SKBitmap actual = DrawCoverage(area, samplesPerMillimetre, new SKPath[] { result })) {
                if (removed.Count > 0)
                    DrawPaths(expected, area, samplesPerMillimetre, removed, index => (byte)0);
                ReadOnlySpan<byte> expectedSamples = expected.GetPixelSpan(), actualSamples = actual.GetPixelSpan();
                bool[] isDiffering = new bool[expectedSamples.Length];
                for (int i = 0; i < isDiffering.Length; ++i)
                    isDiffering[i] = expectedSamples[i] != actualSamples[i];
                MeasureDifference(isDiffering, expected.Width, expected.Height, out long differing, out long thick, out int largestGroup);
                return thick;
            }
        }

        /// <summary>
        /// Gives the size of the largest group of set samples that hangs together, where two samples hang together
        /// when they share a side. isSet is one flag for each sample, row by row. The flags are cleared.
        /// </summary>
        public static int FindLargestGroup(bool[] isSet, int width, int height)
        {
            int largest = 0;
            Stack<int> pending = new Stack<int>();
            for (int seed = 0; seed < isSet.Length; ++seed) {
                if (!isSet[seed])
                    continue;
                int size = 0;
                isSet[seed] = false;
                pending.Push(seed);
                while (pending.Count > 0) {
                    int sample = pending.Pop();
                    ++size;
                    int x = sample % width, y = sample / width;
                    if (x > 0 && isSet[sample - 1]) { isSet[sample - 1] = false; pending.Push(sample - 1); }
                    if (x + 1 < width && isSet[sample + 1]) { isSet[sample + 1] = false; pending.Push(sample + 1); }
                    if (y > 0 && isSet[sample - width]) { isSet[sample - width] = false; pending.Push(sample - width); }
                    if (y + 1 < height && isSet[sample + width]) { isSet[sample + width] = false; pending.Push(sample + width); }
                }
                largest = Math.Max(largest, size);
            }
            return largest;
        }
    }
}
