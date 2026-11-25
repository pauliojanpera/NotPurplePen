using System.Drawing;
using System.Globalization;
using System.IO;
using PurplePen.MapModel;

namespace RenderingTestCases
{
    // A descriptor file of the map rendering tests. Its four lines name a map file and a baseline image in the
    // descriptor's own directory, give the area of the map that the image shows as its left, bottom, right and
    // top, and give the width and height of the image in pixels. Numbers are written with periods.
    public sealed class RenderingDescriptor
    {
        // The directory that holds the descriptor, its map and its baseline image.
        public readonly string DirectoryName;

        // The file name of the map, as the descriptor writes it.
        public readonly string MapFileName;

        // The file name of the baseline image, as the descriptor writes it.
        public readonly string BaselineFileName;

        // The area of the map that the image shows.
        public readonly RectangleF MapArea;

        // The size of the image in pixels.
        public readonly Size PixelSize;

        // Create a descriptor from its parts.
        private RenderingDescriptor(string directoryName, string mapFileName, string baselineFileName, RectangleF mapArea, Size pixelSize)
        {
            DirectoryName = directoryName;
            MapFileName = mapFileName;
            BaselineFileName = baselineFileName;
            MapArea = mapArea;
            PixelSize = pixelSize;
        }

        // Read a descriptor file.
        // descriptorPath: the path of the descriptor file.
        public static RenderingDescriptor Read(string descriptorPath)
        {
            string[] lines = File.ReadAllLines(descriptorPath);
            string[] area = lines[2].Split(',');
            float left = float.Parse(area[0], CultureInfo.InvariantCulture);
            float bottom = float.Parse(area[1], CultureInfo.InvariantCulture);
            float right = float.Parse(area[2], CultureInfo.InvariantCulture);
            float top = float.Parse(area[3], CultureInfo.InvariantCulture);
            string[] size = lines[3].Split(',');

            return new RenderingDescriptor(Path.GetDirectoryName(descriptorPath), lines[0], lines[1],
                                           new RectangleF(left, top, right - left, bottom - top),
                                           new Size(int.Parse(size[0], CultureInfo.InvariantCulture), int.Parse(size[1], CultureInfo.InvariantCulture)));
        }

        // The path of the map file.
        public string MapFilePath
        {
            get { return Path.Combine(DirectoryName, MapFileName); }
        }

        // Load the map that the descriptor names, measuring its text with Skia and finding its templates beside it.
        public Map LoadMap()
        {
            Map map = new Map(new Skia_TextMetrics(), new Skia_FileLoader(DirectoryName));
            InputOutput.ReadFile(MapFilePath, map);
            return map;
        }
    }
}
