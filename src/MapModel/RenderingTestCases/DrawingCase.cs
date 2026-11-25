using System;
using System.Drawing;
using System.IO;
using PurplePen.Graphics2D;
using TestingUtils;

namespace RenderingTestCases
{
    // A drawing made of IGraphicsTarget calls, which the rendering tests of the graphics targets draw and compare
    // with an image of it. The image shows the drawing's Area, scaled to PixelWidth pixels across.
    public sealed class DrawingCase
    {
        // The name of the case, which also names its tests.
        public readonly string Name;

        // The width of the image in pixels. Its height follows from the shape of the area.
        public readonly int PixelWidth;

        // The area of drawing coordinates that the image shows.
        public readonly RectangleF Area;

        // The file name, in TestFiles/skia_render, of the Skia rendering tests' baseline image of the drawing.
        public readonly string SkiaBaselineFileName;

        // Draws the drawing into a graphics target.
        public readonly Action<IGraphicsTarget> Draw;

        // Create a drawing case.
        // name: the name of the case.
        // pixelWidth: the width of its image in pixels.
        // area: the area of drawing coordinates that the image shows.
        // skiaBaselineFileName: the file name of the Skia baseline image in TestFiles/skia_render.
        // draw: draws the drawing into a graphics target.
        public DrawingCase(string name, int pixelWidth, RectangleF area, string skiaBaselineFileName, Action<IGraphicsTarget> draw)
        {
            Name = name;
            PixelWidth = pixelWidth;
            Area = area;
            SkiaBaselineFileName = skiaBaselineFileName;
            Draw = draw;
        }

        // The height of the image in pixels: the height of the area at the scale that fits its width, rounded up.
        public int PixelHeight
        {
            get { return (int)Math.Ceiling(PixelWidth * Area.Height / Area.Width); }
        }

        // The path of the Skia baseline image.
        public string SkiaBaselinePath
        {
            get { return TestUtil.GetTestFile(Path.Combine("skia_render", SkiaBaselineFileName)); }
        }

        // Return the name of the case, which names the tests made of it.
        public override string ToString()
        {
            return Name;
        }
    }
}
