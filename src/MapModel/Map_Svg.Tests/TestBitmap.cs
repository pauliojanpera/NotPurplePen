using System;
using System.IO;
using System.Text;
using PurplePen.Graphics2D;

namespace Map_Svg.Tests
{
    // A bitmap for the document tests, whose written data names the bitmap, the part of it and the format and quality it
    // was written in, so that a test reads from a document what the target wrote.
    public sealed class TestBitmap : IGraphicsBitmap
    {
        // The bitmap's name, with the part of it that it is when it was cropped.
        private readonly string name;

        // The format the bitmap was read from.
        private readonly GraphicsBitmapFormat originalFormat;

        // How many times the bitmap's data has been written.
        public int Writes;

        // Create a bitmap.
        // name: its name, which its written data carries.
        // width, height: its size in pixels.
        // originalFormat: the format it was read from.
        public TestBitmap(string name, int width, int height, GraphicsBitmapFormat originalFormat)
        {
            this.name = name;
            PixelWidth = width;
            PixelHeight = height;
            this.originalFormat = originalFormat;
            HorizontalResolution = VerticalResolution = 96;
        }

        // Whether the bitmap has been disposed.
        public bool Disposed { get; private set; }

        // The bitmap's size in pixels.
        public int PixelWidth { get; private set; }
        public int PixelHeight { get; private set; }

        // A graphics target of the bitmap is never asked for.
        public bool MustCopyBitsForGraphicsTarget
        {
            get { return false; }
        }

        // The resolution, in dots per inch.
        public double HorizontalResolution { get; set; }
        public double VerticalResolution { get; set; }

        // The format the bitmap was read from.
        public GraphicsBitmapFormat GetOriginalFormat()
        {
            return originalFormat;
        }

        // The bitmap has no pixels to read.
        public System.Drawing.Color GetPixel(int x, int y)
        {
            throw new NotSupportedException("A test bitmap has no pixels.");
        }

        // The part of the bitmap, as a bitmap whose name says which part it is.
        public IGraphicsBitmap Crop(int x, int y, int width, int height)
        {
            return new TestBitmap(name + "[" + x + "," + y + "," + width + "," + height + "]", width, height, originalFormat);
        }

        // Write data naming the bitmap and the format, and for JPEG the quality, it is written in.
        public bool WriteToStream(GraphicsBitmapFormat format, Stream stream, int quality)
        {
            ++Writes;
            byte[] data = Encoding.ASCII.GetBytes(WrittenText(name, format, quality));
            stream.Write(data, 0, data.Length);
            return true;
        }

        // The text that a bitmap of a name writes as its data.
        // name: the bitmap's name.
        // format: the format it is written in.
        // quality: the quality, which JPEG alone carries.
        public static string WrittenText(string name, GraphicsBitmapFormat format, int quality)
        {
            return name + " as " + format + (format == GraphicsBitmapFormat.JPEG ? " at " + quality : "");
        }

        // A graphics target of the bitmap is never asked for.
        public IBitmapGraphicsTarget GetGraphicsTarget(bool copyBits, IColorConverter colorConverter = null)
        {
            throw new NotSupportedException("A test bitmap is not drawn into.");
        }

        // Mark the bitmap disposed.
        public void Dispose()
        {
            Disposed = true;
        }
    }
}
