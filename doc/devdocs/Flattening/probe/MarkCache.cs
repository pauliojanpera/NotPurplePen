using System.Collections.Generic;
using System.IO;
using PurplePen.Graphics2D;
using SkiaSharp;

namespace SkiaPathOpsProbe
{
    /// <summary>
    /// A file that holds the runs of a map as the target kept them: the fill type, the kind and the coordinates of
    /// each segment of each mark, bit for bit. The stage "inventory" writes it, and the other stages of the map read
    /// it, so that they do not draw the map again.
    /// </summary>
    public static class MarkCache
    {
        /// <summary>Writes the bounds of the map and its runs to a file.</summary>
        public static void Save(string file, SKRect bounds, List<Run> runs)
        {
            using (BinaryWriter writer = new BinaryWriter(File.Create(file))) {
                writer.Write(bounds.Left); writer.Write(bounds.Top); writer.Write(bounds.Right); writer.Write(bounds.Bottom);
                writer.Write(runs.Count);
                SKPoint[] points = new SKPoint[4];
                foreach (Run run in runs) {
                    writer.Write(run.Colour.Cyan); writer.Write(run.Colour.Magenta); writer.Write(run.Colour.Yellow); writer.Write(run.Colour.Black); writer.Write(run.Colour.Alpha);
                    writer.Write(run.IsInsideBlending);
                    writer.Write(run.ColourName);
                    writer.Write(run.Marks.Count);
                    foreach (SKPath mark in run.Marks) {
                        writer.Write((byte)mark.FillType);
                        writer.Write(mark.VerbCount);
                        using (SKPath.RawIterator iterator = mark.CreateRawIterator()) {
                            SKPathVerb verb;
                            while ((verb = iterator.Next(points)) != SKPathVerb.Done) {
                                writer.Write((byte)verb);
                                int first = verb == SKPathVerb.Move ? 0 : 1, last = LastPointIndex(verb);
                                for (int i = first; i <= last; ++i) {
                                    writer.Write(points[i].X);
                                    writer.Write(points[i].Y);
                                }
                                if (verb == SKPathVerb.Conic)
                                    writer.Write(iterator.ConicWeight());
                            }
                        }
                    }
                }
            }
        }

        /// <summary>Reads the bounds of a map and its runs from a file.</summary>
        public static List<Run> Load(string file, out SKRect bounds)
        {
            List<Run> runs = new List<Run>();
            using (BinaryReader reader = new BinaryReader(File.OpenRead(file))) {
                bounds = new SKRect(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                int runCount = reader.ReadInt32();
                float[] numbers = new float[7];
                for (int index = 0; index < runCount; ++index) {
                    Run run = new Run { Index = index };
                    run.Colour = CmykColor.FromCmyka(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                    run.IsInsideBlending = reader.ReadBoolean();
                    run.ColourName = reader.ReadString();
                    int markCount = reader.ReadInt32();
                    for (int markIndex = 0; markIndex < markCount; ++markIndex) {
                        SKPath mark = new SKPath();
                        mark.FillType = (SKPathFillType)reader.ReadByte();
                        int verbCount = reader.ReadInt32();
                        for (int verbIndex = 0; verbIndex < verbCount; ++verbIndex) {
                            SKPathVerb verb = (SKPathVerb)reader.ReadByte();
                            int numberCount = verb == SKPathVerb.Close ? 0 : verb == SKPathVerb.Conic ? 5 : 2 * (verb == SKPathVerb.Move ? 1 : LastPointIndex(verb));
                            for (int i = 0; i < numberCount; ++i)
                                numbers[i] = reader.ReadSingle();
                            switch (verb) {
                                case SKPathVerb.Move: mark.MoveTo(numbers[0], numbers[1]); break;
                                case SKPathVerb.Line: mark.LineTo(numbers[0], numbers[1]); break;
                                case SKPathVerb.Quad: mark.QuadTo(numbers[0], numbers[1], numbers[2], numbers[3]); break;
                                case SKPathVerb.Conic: mark.ConicTo(numbers[0], numbers[1], numbers[2], numbers[3], numbers[4]); break;
                                case SKPathVerb.Cubic: mark.CubicTo(numbers[0], numbers[1], numbers[2], numbers[3], numbers[4], numbers[5]); break;
                                case SKPathVerb.Close: mark.Close(); break;
                            }
                        }
                        run.Marks.Add(mark);
                    }
                    runs.Add(run);
                }
            }
            return runs;
        }

        /// <summary>Gives the index of the last point of a segment in the points that the iterator of a path gives.</summary>
        private static int LastPointIndex(SKPathVerb verb)
        {
            return verb == SKPathVerb.Line ? 1 : verb == SKPathVerb.Cubic ? 3 : verb == SKPathVerb.Quad || verb == SKPathVerb.Conic ? 2 : 0;
        }
    }
}
