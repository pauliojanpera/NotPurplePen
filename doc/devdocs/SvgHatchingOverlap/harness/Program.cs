using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using PurplePen.Graphics2D;
using PurplePen.MapModel;
using SkiaSharp;

namespace OverlapHarness
{
    // Measures the map code's drawing of overlapping patterns: the areas of each symbol that overlap others in the
    // same frame of their patterns, a map drawn to a PNG with the Skia target, and the time the Skia target takes to
    // draw a map as the screen draws it.
    public static class Program
    {
        // Run one command: info <map>, render <map> <png> <pixels per mm> <pattern bitmaps 0|1> [left top right bottom],
        // or time <map> <pixels per mm> <repeats> <pattern bitmaps 0|1>.
        // args: the command and its arguments.
        public static int Main(string[] args)
        {
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
            switch (args[0]) {
                case "info":
                    Info(LoadMap(args[1]));
                    return 0;
                case "render":
                    Render(LoadMap(args[1]), args[2], float.Parse(args[3]), args[4] == "1",
                           args.Length > 8 ? RectangleF.FromLTRB(float.Parse(args[5]), float.Parse(args[6]), float.Parse(args[7]), float.Parse(args[8])) : (RectangleF?)null);
                    return 0;
                case "time":
                    Time(LoadMap(args[1]), float.Parse(args[2]), int.Parse(args[3]), args[4] == "1");
                    return 0;
                case "clips":
                    Clips(LoadMap(args[1]), float.Parse(args[2]));
                    return 0;
                default:
                    Console.Error.WriteLine("unknown command " + args[0]);
                    return 2;
            }
        }

        // Load a map with Skia's text metrics, its templates looked for beside it.
        // fileName: the map's file.
        static Map LoadMap(string fileName)
        {
            Map map = new Map(new Skia_TextMetrics(), new Skia_FileLoader(Path.GetDirectoryName(Path.GetFullPath(fileName))));
            InputOutput.ReadFile(fileName, map);
            return map;
        }

        // Print the map's objects, and for each area symbol with hatchings or patterns the areas, the frames and the
        // groups of areas whose bounding boxes overlap in one frame.
        // map: the map.
        static void Info(Map map)
        {
            using (map.Read()) {
                Console.WriteLine("objects\t" + map.AllSymbols.Count());
                Console.WriteLine("bounds\t" + map.Bounds);
                int symbolsWithGroups = 0, areasInGroups = 0, groups = 0;
                double unionBoxArea = 0, memberBoxArea = 0;
                foreach (SymDef symdef in map.AllSymdefs) {
                    AreaSymDef areaSymdef = symdef as AreaSymDef;
                    if (areaSymdef == null || (!areaSymdef.HasHatching && !areaSymdef.HasPattern))
                        continue;
                    List<AreaSymbol> areas = map.AllSymbols.OfType<AreaSymbol>().Where(area => area.Definition == symdef).ToList();
                    if (areas.Count == 0)
                        continue;
                    Dictionary<Tuple<float, float, float>, List<AreaSymbol>> byFrame = areas
                        .GroupBy(area => Tuple.Create(area.Angle, area.RotationCenter.X, area.RotationCenter.Y))
                        .ToDictionary(group => group.Key, group => group.ToList());
                    int symbolGroups = 0, symbolAreasInGroups = 0;
                    foreach (List<AreaSymbol> frameAreas in byFrame.Values) {
                        foreach (List<AreaSymbol> component in Components(frameAreas)) {
                            if (component.Count > 1) {
                                ++symbolGroups;
                                symbolAreasInGroups += component.Count;
                                RectangleF union = component[0].Path.BoundingBox;
                                foreach (AreaSymbol area in component) {
                                    union = RectangleF.Union(union, area.Path.BoundingBox);
                                    memberBoxArea += area.Path.BoundingBox.Width * area.Path.BoundingBox.Height;
                                }
                                unionBoxArea += union.Width * union.Height;
                            }
                        }
                    }
                    string kinds = (areaSymdef.HasHatching ? "hatching " : "") +
                                   string.Join(" ", areaSymdef.GetPatterns().Select(pattern => pattern.patternFillMode + (pattern.irregular ? "-irregular" : "")));
                    Console.WriteLine(string.Format(CultureInfo.InvariantCulture, "symbol\t{0}\t{1}\t{2}\tareas {3}\tframes {4}\tgroups {5}\tareas in groups {6}",
                                                    symdef.SymbolId, symdef.Name, kinds.Trim(), areas.Count, byFrame.Count, symbolGroups, symbolAreasInGroups));
                    if (symbolGroups > 0) {
                        ++symbolsWithGroups;
                        groups += symbolGroups;
                        areasInGroups += symbolAreasInGroups;
                    }
                }
                Console.WriteLine(string.Format(CultureInfo.InvariantCulture, "total\tsymbols with groups {0}\tgroups {1}\tareas in groups {2}", symbolsWithGroups, groups, areasInGroups));
                Console.WriteLine(string.Format(CultureInfo.InvariantCulture, "boxes\tgroups' boxes {0:F0} mm2\ttheir areas' boxes {1:F0} mm2\tratio {2:F2}", unionBoxArea, memberBoxArea, unionBoxArea / Math.Max(1, memberBoxArea)));
            }
        }

        // The groups of areas connected by overlapping or touching bounding boxes.
        // areas: the areas of one symbol and frame.
        static List<List<AreaSymbol>> Components(List<AreaSymbol> areas)
        {
            int count = areas.Count;
            int[] parent = Enumerable.Range(0, count).ToArray();
            RectangleF[] boxes = areas.Select(area => area.Path.BoundingBox).ToArray();
            int[] order = Enumerable.Range(0, count).OrderBy(index => boxes[index].Left).ToArray();
            List<int> open = new List<int>();
            foreach (int index in order) {
                open.RemoveAll(other => boxes[other].Right < boxes[index].Left);
                foreach (int other in open) {
                    if (boxes[other].Top <= boxes[index].Bottom && boxes[index].Top <= boxes[other].Bottom) {
                        int a = Root(parent, index), b = Root(parent, other);
                        if (a != b)
                            parent[Math.Max(a, b)] = Math.Min(a, b);
                    }
                }
                open.Add(index);
            }
            return Enumerable.Range(0, count).GroupBy(index => Root(parent, index)).Select(group => group.Select(index => areas[index]).ToList()).ToList();
        }

        // The root of an element's set.
        // parent: each element's parent.
        // element: the element.
        static int Root(int[] parent, int element)
        {
            while (parent[element] != element)
                element = parent[element];
            return element;
        }

        // Draw the map, or a rectangle of it, to a PNG with the Skia target, anti-aliased on white, as the PNG export
        // draws it.
        // map: the map.
        // pngFile: the file to write.
        // pixelsPerMm: pixels to a millimetre of map.
        // patternBitmaps: whether area patterns are drawn with pattern brushes.
        // area: the rectangle, or null for the map's bounds.
        static void Render(Map map, string pngFile, float pixelsPerMm, bool patternBitmaps, RectangleF? area)
        {
            RectangleF mapArea;
            using (map.Read())
                mapArea = area ?? map.Bounds;
            Size size = new Size((int)Math.Ceiling(mapArea.Width * pixelsPerMm), (int)Math.Ceiling(mapArea.Height * pixelsPerMm));
            using (Skia_BitmapGraphicsTarget target = Draw(map, mapArea, size, patternBitmaps))
            using (Skia_Bitmap bitmap = (Skia_Bitmap)target.FinishBitmap())
            using (FileStream stream = File.Create(pngFile))
                bitmap.Bitmap.Encode(stream, SKEncodedImageFormat.Png, 100);
            Console.WriteLine(pngFile + "\t" + size.Width + "x" + size.Height);
        }

        // Draw the map's bounds at a resolution again and again, as the screen draws it, and print the times.
        // map: the map.
        // pixelsPerMm: pixels to a millimetre of map.
        // repeats: how many times to draw it.
        // patternBitmaps: whether area patterns are drawn with pattern brushes, as the screen draws them.
        static void Time(Map map, float pixelsPerMm, int repeats, bool patternBitmaps)
        {
            RectangleF mapArea;
            using (map.Read())
                mapArea = map.Bounds;
            Size size = new Size((int)Math.Ceiling(mapArea.Width * pixelsPerMm), (int)Math.Ceiling(mapArea.Height * pixelsPerMm));
            List<double> times = new List<double>();
            for (int i = 0; i < repeats; ++i) {
                Stopwatch stopwatch = Stopwatch.StartNew();
                using (Skia_BitmapGraphicsTarget target = Draw(map, mapArea, size, patternBitmaps)) {
                    target.FinishBitmap().Dispose();
                }
                times.Add(stopwatch.Elapsed.TotalMilliseconds);
            }
            times.Sort();
            Console.WriteLine(string.Format(CultureInfo.InvariantCulture, "{0}x{1}\tmin {2:F0} ms\tmedian {3:F0} ms\tall {4}", size.Width, size.Height,
                                            times[0], times[times.Count / 2], string.Join(" ", times.Select(t => t.ToString("F0", CultureInfo.InvariantCulture)))));
        }

        // Check that the prototype's union of one area's region, PushClipToUnion, clips to the pixels that the Skia
        // target's own clip to the area's path does, as it clips without smoothing: for each area of the map, fill the
        // map's bounds under each, and count the pixels that differ.
        // map: the map.
        // pixelsPerMm: pixels to a millimetre of map.
        static void Clips(Map map, float pixelsPerMm)
        {
            List<AreaSymbol> areas;
            RectangleF mapArea;
            using (map.Read()) {
                areas = map.AllSymbols.OfType<AreaSymbol>().ToList();
                mapArea = map.Bounds;
            }
            Size size = new Size((int)Math.Ceiling(mapArea.Width * pixelsPerMm), (int)Math.Ceiling(mapArea.Height * pixelsPerMm));
            int areasDiffering = 0;
            long pixelsDiffering = 0;
            object brush = new object();
            foreach (AreaSymbol area in areas) {
                using (Skia_Bitmap byPath = ClipAndFill(area.Path, mapArea, size, brush, false))
                using (Skia_Bitmap byUnion = ClipAndFill(area.Path, mapArea, size, brush, true)) {
                    ReadOnlySpan<byte> a = byPath.Bitmap.GetPixelSpan(), b = byUnion.Bitmap.GetPixelSpan();
                    int differing = 0;
                    for (int i = 0; i < a.Length; i += 4) {
                        if (a[i] != b[i] || a[i + 1] != b[i + 1] || a[i + 2] != b[i + 2])
                            ++differing;
                    }
                    if (differing > 0) {
                        ++areasDiffering;
                        pixelsDiffering += differing;
                    }
                }
            }
            Console.WriteLine(string.Format(CultureInfo.InvariantCulture, "{0} areas at {1} pixels a mm ({2}x{3}): {4} clip otherwise through the union, {5} pixels in all",
                                            areas.Count, pixelsPerMm, size.Width, size.Height, areasDiffering, pixelsDiffering));
        }

        // Fill the map's bounds in black under a clip to an area, on white, with the Skia target anti-aliased.
        // path: the area's path.
        // mapArea: the map's bounds.
        // size: the bitmap's size.
        // brush: the key of the black brush.
        // union: whether to clip through PushClipToUnion with the one path, rather than PushClip.
        static Skia_Bitmap ClipAndFill(SymPathWithHoles path, RectangleF mapArea, Size size, object brush, bool union)
        {
            using (Skia_BitmapGraphicsTarget target = new Skia_BitmapGraphicsTarget(size.Width, size.Height, false, CmykColor.FromCmyk(0, 0, 0, 0), mapArea, true, null, 1.0F)) {
                target.PushAntiAliasing(true);
                target.CreateSolidBrush(brush, CmykColor.FromCmyk(0, 0, 0, 1));
                object pathKey = path.GetPathKey(target);
                if (union)
                    target.PushClipToUnion(new object[] { pathKey });
                else
                    target.PushClip(pathKey);
                target.FillRectangle(brush, mapArea);
                target.PopClip();
                target.PopAntiAliasing();
                return (Skia_Bitmap)target.FinishBitmap();
            }
        }

        // Draw the map's area into a new Skia bitmap target of a size, anti-aliased on white, with the options the
        // screen draws with.
        // map: the map.
        // mapArea: the area of the map.
        // size: the bitmap's size.
        // patternBitmaps: whether area patterns are drawn with pattern brushes.
        static Skia_BitmapGraphicsTarget Draw(Map map, RectangleF mapArea, Size size, bool patternBitmaps)
        {
            Skia_BitmapGraphicsTarget target = new Skia_BitmapGraphicsTarget(size.Width, size.Height, false, CmykColor.FromCmyk(0, 0, 0, 0), mapArea, true, null, 1.0F);
            RenderOptions renderOpts = new RenderOptions();
            renderOpts.usePatternBitmaps = patternBitmaps;
            renderOpts.renderTemplates = RenderTemplateOption.MapOnly;
            renderOpts.blendOverprintedColors = true;
            renderOpts.minResolution = mapArea.Width / size.Width;
            target.PushAntiAliasing(true);
            using (map.Read())
                map.Draw(target, mapArea, renderOpts, null);
            target.PopAntiAliasing();
            return target;
        }
    }
}
