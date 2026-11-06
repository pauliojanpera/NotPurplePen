using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using PurplePen.Graphics2D;
using PurplePen.MapModel;
using RenderingTestCases;
using SkiaSharp;
using TestingUtils;

namespace Map_Svg.Tests
{
    // A case of the rendering comparison: a drawing case, a map area of the map rendering tests, or a map area that
    // checks where pattern fills put an area's pattern, drawn as its reference image is recorded. A map area is drawn
    // anti-aliased, with the blending, layers and intensity that the map rendering tests check it with, and with its
    // regular area patterns drawn as the export draws them: with pattern brushes into a target whose brushes are
    // vectors, as are the SVG target's and those of the recorder that stands in for it, and element by element into the
    // Skia target that draws the reference image (V-9). The comparison thus checks where the brushes put the patterns
    // (S-13).
    public sealed class ComparisonCase
    {
        // The intensity at which the map rendering tests check lightened colors.
        private const float LightenedIntensity = 0.4F;

        // The name of the case, which names its test, its SVG document and its reference image.
        public readonly string Name;

        // The area of drawing or map coordinates that the image shows.
        public readonly RectangleF Area;

        // The size of the image in pixels.
        public readonly Size PixelSize;

        // Whether the area is in map coordinates, whose y axis points up.
        public readonly bool YAxisUp;

        // Draws the case into a graphics target.
        private readonly Action<IGraphicsTarget> draw;

        // Create a case.
        private ComparisonCase(string name, RectangleF area, Size pixelSize, bool yAxisUp, Action<IGraphicsTarget> draw)
        {
            Name = name;
            Area = area;
            PixelSize = pixelSize;
            YAxisUp = yAxisUp;
            this.draw = draw;
        }

        // Make a case of each drawing case.
        public static IEnumerable<ComparisonCase> MakeDrawingCases()
        {
            return DrawingCases.All.Select(drawingCase => new ComparisonCase(
                drawingCase.Name, drawingCase.Area, new Size(drawingCase.PixelWidth, drawingCase.PixelHeight), false, drawingCase.Draw));
        }

        // Make a case of each map area that the map rendering tests check with distinct blending or layers, and one
        // more at lightened intensity where a check tests lightened colors. A case is named after its descriptor,
        // with "_overprinted" when it blends colors that overprint and "_light" when it is lightened.
        public static IEnumerable<ComparisonCase> MakeMapAreaCases()
        {
            List<ComparisonCase> cases = new List<ComparisonCase>();
            Dictionary<string, string> settingsByName = new Dictionary<string, string>(StringComparer.Ordinal);

            foreach (MapRenderingCheck check in MapRenderingTestCases.All.SelectMany(test => test.Checks)) {
                string name = Path.GetFileNameWithoutExtension(check.DescriptorFileName) + (check.BlendOverprintedColors ? "_overprinted" : "");
                string settings = check.DescriptorFileName + "|" + check.BlendOverprintedColors + "|" + check.ColorBeginDrawExclusive + "|" + check.ColorEndDrawInclusive;

                string settingsOfName;
                if (settingsByName.TryGetValue(name, out settingsOfName)) {
                    if (settingsOfName != settings)
                        throw new InvalidOperationException(string.Format("Two map areas of the rendering comparison would both be named {0}.", name));
                    continue;
                }
                settingsByName.Add(name, settings);

                cases.Add(MakeMapAreaCase(name, check, 1.0F));
                if (check.TestLightenedColor)
                    cases.Add(MakeMapAreaCase(name + "_light", check, LightenedIntensity));
            }

            return cases;
        }

        // Make the cases that check where pattern fills put an area's pattern and its hatchings (S-13): squares of a
        // regular pattern, and squares of two hatchings, turned or not, about the map's origin or about an origin of
        // their own, built for the purpose; and the turned areas of an OpenMapper map, patterned and hatched, each with
        // its own pattern origin, as its import tests draw them.
        public static IEnumerable<ComparisonCase> MakeAreaPatternCases()
        {
            return new ComparisonCase[] {
                MakeMapCase("patternorigins", PatternedAreaMap.AreaOfEveryPlacement, new Size(800, 800),
                            () => PatternedAreaMap.Create(false, AreaSymDef.PatternFillMode.Clip, PatternedAreaMap.SquaresInEveryPlacement)),
                MakeMapCase("hatchingorigins", PatternedAreaMap.AreaOfEveryPlacement, new Size(800, 800),
                            () => PatternedAreaMap.CreateHatched(PatternedAreaMap.SquaresInEveryPlacement)),
                MakeMapCase("rotarea_omap", RectangleF.FromLTRB(-37, -13, -10, 2), new Size(1000, 556),
                            () => LoadMap(TestUtil.GetTestFile(Path.Combine("openmapper", "rotarea.omap")))),
            };
        }

        // Make the case of a map's area, drawn as a map area of the map rendering tests is drawn, without blending.
        // name: the name of the case.
        // area: the area of the map that the image shows.
        // pixelSize: the size of the image in pixels.
        // createMap: creates the map, which the drawing disposes of.
        private static ComparisonCase MakeMapCase(string name, RectangleF area, Size pixelSize, Func<Map> createMap)
        {
            return new ComparisonCase(name, area, pixelSize, true, target => {
                using (Map map = createMap()) {
                    MapDrawing.DrawMapArea(target, map, area, pixelSize, new RenderOptions(),
                                           GraphicsUtil.PatternBrushesAreVectors(target), false, true);
                }
            });
        }

        // Load a map file, measuring its text with Skia and finding its templates beside it.
        // path: the path of the map file.
        private static Map LoadMap(string path)
        {
            Map map = new Map(new Skia_TextMetrics(), new Skia_FileLoader(Path.GetDirectoryName(path)));
            InputOutput.ReadFile(path, map);
            return map;
        }

        // Make the case of a check's map area at an intensity.
        // name: the name of the case.
        // check: the check whose descriptor, blending and layers the case takes.
        // intensity: the intensity to draw at, which the drawing sets when it is below 1.
        private static ComparisonCase MakeMapAreaCase(string name, MapRenderingCheck check, float intensity)
        {
            RenderingDescriptor descriptor = RenderingDescriptor.Read(check.DescriptorPath);
            return new ComparisonCase(name, descriptor.MapArea, descriptor.PixelSize, true, target => {
                if (intensity != 1.0F)
                    target.Intensity = intensity;
                using (Map map = descriptor.LoadMap()) {
                    MapDrawing.DrawMapArea(target, map, descriptor.MapArea, descriptor.PixelSize, check.CreateRenderOptions(),
                                           GraphicsUtil.PatternBrushesAreVectors(target), check.BlendOverprintedColors, true);
                }
            });
        }

        // Record the case's drawing, with the structure calls that the map code makes to a target that keeps them.
        public RecordingGraphicsTarget Record()
        {
            RecordingGraphicsTarget recording = new StructuredRecordingGraphicsTarget(new SkiaBitmapGraphicsLoader());
            try {
                draw(recording);
                recording.EndRecording();
                return recording;
            }
            catch {
                recording.Dispose();
                throw;
            }
        }

        // Render the case's reference image with the Skia target: anti-aliased, its clips too, on white, with the case's
        // area filling the image (V-9). The Skia target clips with SkiaSharp's default, aliased, while Chromium
        // anti-aliases every clip path whatever its shape-rendering, so an aliased clip edge could not be matched.
        public SKBitmap RenderReference()
        {
            SKBitmap reference = new SKBitmap(PixelSize.Width, PixelSize.Height);
            using (SKCanvas canvas = new SKCanvas(reference)) {
                canvas.Clear(SKColors.White);
                using (Skia_GraphicsTarget skiaTarget = new Skia_GraphicsTarget(canvas, false))
                using (AntiAliasedClipsTarget target = new AntiAliasedClipsTarget(skiaTarget))
                using (Matrix areaToImage = CreateAreaToImageTransform()) {
                    target.PushTransform(areaToImage);
                    target.PushAntiAliasing(true);
                    draw(target);
                }
            }
            return reference;
        }

        // Create the transform from the case's area to the pixels of its image.
        public Matrix CreateAreaToImageTransform()
        {
            return AreaInImage.CreateTransform(Area, PixelSize, YAxisUp);
        }

        // Return the name of the case, which names its test.
        public override string ToString()
        {
            return Name;
        }
    }
}
