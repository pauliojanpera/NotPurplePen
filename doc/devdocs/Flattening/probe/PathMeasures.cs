using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using SkiaSharp;

namespace SkiaPathOpsProbe
{
    /// <summary>
    /// The counts of the contours and of the segments of paths, by the kind of the segment.
    /// </summary>
    public struct SegmentCounts
    {
        /// <summary>The count of the contours: the Move verbs.</summary>
        public long Contours;

        /// <summary>The counts of the segments of each kind.</summary>
        public long Lines, Quadratics, Conics, Cubics;

        /// <summary>The count of all segments.</summary>
        public long Segments { get { return Lines + Quadratics + Conics + Cubics; } }

        /// <summary>Adds the counts of another set of paths to these counts.</summary>
        public void Add(SegmentCounts other)
        {
            Contours += other.Contours;
            Lines += other.Lines;
            Quadratics += other.Quadratics;
            Conics += other.Conics;
            Cubics += other.Cubics;
        }

        /// <summary>The counts as "lines/quadratics/conics/cubics".</summary>
        public string KindsText { get { return Lines + "/" + Quadratics + "/" + Conics + "/" + Cubics; } }
    }

    /// <summary>
    /// Measures of a Skia path: the counts of its segments, the digest of its segments, and its area.
    /// </summary>
    public static class PathMeasures
    {
        /// <summary>Counts the contours and the segments of a path. A null path has none.</summary>
        public static SegmentCounts CountSegments(SKPath path)
        {
            SegmentCounts counts = new SegmentCounts();
            if (path == null)
                return counts;

            SKPoint[] points = new SKPoint[4];
            using (SKPath.RawIterator iterator = path.CreateRawIterator()) {
                SKPathVerb verb;
                while ((verb = iterator.Next(points)) != SKPathVerb.Done) {
                    switch (verb) {
                        case SKPathVerb.Move: ++counts.Contours; break;
                        case SKPathVerb.Line: ++counts.Lines; break;
                        case SKPathVerb.Quad: ++counts.Quadratics; break;
                        case SKPathVerb.Conic: ++counts.Conics; break;
                        case SKPathVerb.Cubic: ++counts.Cubics; break;
                    }
                }
            }
            return counts;
        }

        /// <summary>Counts the contours and the segments of a list of paths.</summary>
        public static SegmentCounts CountSegments(IEnumerable<SKPath> paths)
        {
            SegmentCounts counts = new SegmentCounts();
            foreach (SKPath path in paths)
                counts.Add(CountSegments(path));
            return counts;
        }

        /// <summary>
        /// Gives the SHA-256 digest of a path: its fill type, and the kind and the coordinates of each segment, bit
        /// for bit. A null path gives "none".
        /// </summary>
        public static string ComputeDigest(SKPath path)
        {
            if (path == null)
                return "none";

            using (MemoryStream bytes = new MemoryStream()) {
                Span<byte> number = stackalloc byte[4];
                bytes.WriteByte((byte)path.FillType);
                SKPoint[] points = new SKPoint[4];
                using (SKPath.RawIterator iterator = path.CreateRawIterator()) {
                    SKPathVerb verb;
                    while ((verb = iterator.Next(points)) != SKPathVerb.Done) {
                        bytes.WriteByte((byte)verb);
                        int first = verb == SKPathVerb.Move ? 0 : 1;
                        int last = verb == SKPathVerb.Move ? 0 : verb == SKPathVerb.Line ? 1 : verb == SKPathVerb.Cubic ? 3 : verb == SKPathVerb.Close ? 0 : 2;
                        for (int i = first; i <= last; ++i) {
                            BinaryPrimitives.WriteSingleLittleEndian(number, points[i].X);
                            bytes.Write(number);
                            BinaryPrimitives.WriteSingleLittleEndian(number, points[i].Y);
                            bytes.Write(number);
                        }
                        if (verb == SKPathVerb.Conic) {
                            BinaryPrimitives.WriteSingleLittleEndian(number, iterator.ConicWeight());
                            bytes.Write(number);
                        }
                    }
                }
                return Convert.ToHexString(SHA256.HashData(bytes.GetBuffer().AsSpan(0, (int)bytes.Length))).ToLowerInvariant();
            }
        }

        /// <summary>
        /// Gives the signed area of each contour of a path, calculated in double precision from the segments. The
        /// calculation is exact for lines and quadratics. For a cubic or a conic it divides the segment into 64
        /// intervals.
        /// </summary>
        public static List<double> ComputeContourAreas(SKPath path)
        {
            List<double> areas = new List<double>();
            SKPoint[] points = new SKPoint[4];
            SKPoint start = SKPoint.Empty, last = SKPoint.Empty;
            double area = 0;
            bool isOpen = false;

            using (SKPath.RawIterator iterator = path.CreateRawIterator()) {
                SKPathVerb verb;
                while ((verb = iterator.Next(points)) != SKPathVerb.Done) {
                    if (verb == SKPathVerb.Move) {
                        if (isOpen)
                            areas.Add(area + IntegrateSegment(SKPathVerb.Line, new SKPoint[] { last, start }, 1));
                        start = last = points[0];
                        area = 0;
                        isOpen = true;
                    }
                    else if (verb == SKPathVerb.Close) {
                        area += IntegrateSegment(SKPathVerb.Line, new SKPoint[] { last, start }, 1);
                        last = start;
                    }
                    else {
                        area += IntegrateSegment(verb, points, verb == SKPathVerb.Conic ? iterator.ConicWeight() : 1);
                        last = points[verb == SKPathVerb.Line ? 1 : verb == SKPathVerb.Cubic ? 3 : 2];
                    }
                }
            }
            if (isOpen)
                areas.Add(area + IntegrateSegment(SKPathVerb.Line, new SKPoint[] { last, start }, 1));
            return areas;
        }

        /// <summary>
        /// Gives the area that a path covers when its contours do not overlap: the sum of the signed areas of the
        /// contours, as a positive number. The results of the path operations are such paths.
        /// </summary>
        public static double ComputeArea(SKPath path)
        {
            double sum = 0;
            foreach (double area in ComputeContourAreas(path))
                sum += area;
            return Math.Abs(sum);
        }

        /// <summary>Gives the point of a segment at parameter t, in double precision.</summary>
        public static void EvaluateSegment(SKPathVerb verb, SKPoint[] points, float conicWeight, double t, out double x, out double y)
        {
            double u = 1 - t;
            switch (verb) {
                case SKPathVerb.Line:
                    x = u * points[0].X + t * points[1].X;
                    y = u * points[0].Y + t * points[1].Y;
                    break;
                case SKPathVerb.Quad:
                    x = u * u * points[0].X + 2 * u * t * points[1].X + t * t * points[2].X;
                    y = u * u * points[0].Y + 2 * u * t * points[1].Y + t * t * points[2].Y;
                    break;
                case SKPathVerb.Conic:
                    double weight = u * u + 2 * u * t * conicWeight + t * t;
                    x = (u * u * points[0].X + 2 * u * t * conicWeight * points[1].X + t * t * points[2].X) / weight;
                    y = (u * u * points[0].Y + 2 * u * t * conicWeight * points[1].Y + t * t * points[2].Y) / weight;
                    break;
                default:
                    x = u * u * u * points[0].X + 3 * u * u * t * points[1].X + 3 * u * t * t * points[2].X + t * t * t * points[3].X;
                    y = u * u * u * points[0].Y + 3 * u * u * t * points[1].Y + 3 * u * t * t * points[2].Y + t * t * t * points[3].Y;
                    break;
            }
        }

        /// <summary>
        /// Gives the part of the area integral, the half of the sum of x dy - y dx, of one segment. A curve is
        /// divided into 64 intervals, and each interval is integrated as the quadratic through its two ends and its
        /// middle point.
        /// </summary>
        private static double IntegrateSegment(SKPathVerb verb, SKPoint[] points, float conicWeight)
        {
            int intervals = verb == SKPathVerb.Line ? 1 : 64;
            double sum = 0;
            EvaluateSegment(verb, points, conicWeight, 0, out double previousX, out double previousY);
            for (int i = 1; i <= intervals; ++i) {
                double tMiddle = (i - 0.5) / intervals, tEnd = (double)i / intervals;
                EvaluateSegment(verb, points, conicWeight, tMiddle, out double middleX, out double middleY);
                EvaluateSegment(verb, points, conicWeight, tEnd, out double endX, out double endY);
                double controlX = 2 * middleX - 0.5 * (previousX + endX), controlY = 2 * middleY - 0.5 * (previousY + endY);
                sum += (previousX * controlY - controlX * previousY) / 3 + (controlX * endY - endX * controlY) / 3 + (previousX * endY - endX * previousY) / 6;
                previousX = endX;
                previousY = endY;
            }
            return sum;
        }
    }
}
