using System.IO;
using PurplePen.MapModel;
using TestingUtils;

namespace RenderingTestCases
{
    // One check of the map rendering tests: draw the map area that a descriptor file in TestFiles/skia_render
    // names, with the settings below, and compare the image with the descriptor's baseline image.
    public sealed class MapRenderingCheck
    {
        // The file name of the descriptor in TestFiles/skia_render.
        public readonly string DescriptorFileName;

        // Whether area patterns are drawn with pattern brushes rather than element by element.
        public readonly bool UsePatternBitmaps;

        // Whether colors that overprint are blended with darken.
        public readonly bool BlendOverprintedColors;

        // Whether edges are anti-aliased.
        public readonly bool AntiAlias;

        // The colors drawn: those after ColorBeginDrawExclusive, up to and including ColorEndDrawInclusive, where
        // null stands for the bottom or the top of the color table.
        public readonly int? ColorBeginDrawExclusive, ColorEndDrawInclusive;

        // Whether the map is also drawn at intensity 0.4 and compared with the lightened baseline image.
        public readonly bool TestLightenedColor;

        // Whether the map is also written as OCAD files of versions MinOcadVersion to MaxOcadVersion, each read
        // back, drawn and compared again.
        public readonly bool RoundtripToOcadFile;
        public readonly int MinOcadVersion, MaxOcadVersion;

        // Whether the map is also drawn through a RecordingGraphicsTarget and compared again.
        public readonly bool AlsoTestRecording;

        // Create a check from its settings.
        private MapRenderingCheck(string descriptorFileName, bool usePatternBitmaps, bool blendOverprintedColors, bool antiAlias,
                                  int? colorBeginDrawExclusive, int? colorEndDrawInclusive, bool alsoTestRecording,
                                  bool testLightenedColor, bool roundtripToOcadFile, int minOcadVersion, int maxOcadVersion)
        {
            DescriptorFileName = descriptorFileName;
            UsePatternBitmaps = usePatternBitmaps;
            BlendOverprintedColors = blendOverprintedColors;
            AntiAlias = antiAlias;
            ColorBeginDrawExclusive = colorBeginDrawExclusive;
            ColorEndDrawInclusive = colorEndDrawInclusive;
            AlsoTestRecording = alsoTestRecording;
            TestLightenedColor = testLightenedColor;
            RoundtripToOcadFile = roundtripToOcadFile;
            MinOcadVersion = minOcadVersion;
            MaxOcadVersion = maxOcadVersion;
        }

        // A check that draws area patterns with pattern brushes, aliased and without blending, and also through a
        // recording.
        public static MapRenderingCheck Check(string descriptorFileName, bool testLightenedColor, bool roundtripToOcadFile, int minOcadVersion, int maxOcadVersion)
        {
            return new MapRenderingCheck(descriptorFileName, true, false, false, null, null, true, testLightenedColor, roundtripToOcadFile, minOcadVersion, maxOcadVersion);
        }

        // A check that draws as Check does, but not through a recording.
        public static MapRenderingCheck CheckWithoutRecording(string descriptorFileName, bool testLightenedColor, bool roundtripToOcadFile, int minOcadVersion, int maxOcadVersion)
        {
            return new MapRenderingCheck(descriptorFileName, true, false, false, null, null, false, testLightenedColor, roundtripToOcadFile, minOcadVersion, maxOcadVersion);
        }

        // A check that draws as Check does, but anti-aliased.
        public static MapRenderingCheck CheckAntiAliased(string descriptorFileName, bool testLightenedColor, bool roundtripToOcadFile, int minOcadVersion, int maxOcadVersion)
        {
            return new MapRenderingCheck(descriptorFileName, true, false, true, null, null, true, testLightenedColor, roundtripToOcadFile, minOcadVersion, maxOcadVersion);
        }

        // A check that draws as Check does, but area patterns element by element.
        public static MapRenderingCheck CheckWithoutPatternBitmaps(string descriptorFileName, bool testLightenedColor, bool roundtripToOcadFile, int minOcadVersion, int maxOcadVersion)
        {
            return new MapRenderingCheck(descriptorFileName, false, false, false, null, null, true, testLightenedColor, roundtripToOcadFile, minOcadVersion, maxOcadVersion);
        }

        // A check that draws area patterns element by element and blends colors that overprint.
        public static MapRenderingCheck CheckOverprinted(string descriptorFileName, bool testLightenedColor, bool roundtripToOcadFile, int minOcadVersion, int maxOcadVersion)
        {
            return new MapRenderingCheck(descriptorFileName, false, true, false, null, null, true, testLightenedColor, roundtripToOcadFile, minOcadVersion, maxOcadVersion);
        }

        // A check that draws area patterns element by element, and only the colors after colorBeginDrawExclusive up
        // to and including colorEndDrawInclusive.
        public static MapRenderingCheck CheckLayers(string descriptorFileName, int? colorBeginDrawExclusive, int? colorEndDrawInclusive,
                                                    bool testLightenedColor, bool roundtripToOcadFile, int minOcadVersion, int maxOcadVersion)
        {
            return new MapRenderingCheck(descriptorFileName, false, false, false, colorBeginDrawExclusive, colorEndDrawInclusive, true,
                                         testLightenedColor, roundtripToOcadFile, minOcadVersion, maxOcadVersion);
        }

        // The path of the descriptor file.
        public string DescriptorPath
        {
            get { return TestUtil.GetTestFile(Path.Combine("skia_render", DescriptorFileName)); }
        }

        // Create the render options that the check draws with, apart from those MapDrawing.DrawMapArea sets.
        public RenderOptions CreateRenderOptions()
        {
            RenderOptions renderOptions = new RenderOptions();
            renderOptions.colorBeginDrawExclusive = ColorBeginDrawExclusive;
            renderOptions.colorEndDrawInclusive = ColorEndDrawInclusive;
            return renderOptions;
        }
    }
}
