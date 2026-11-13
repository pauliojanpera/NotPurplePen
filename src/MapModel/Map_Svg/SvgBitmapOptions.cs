using System;
using PurplePen.Graphics2D;

namespace PurplePen.MapModel
{
    // How the bitmaps drawn into a document reach it (B-4).
    public enum SvgBitmapHandling
    {
        // Embedded in the document as data, the default.
        Embedded,

        // Linked as files beside the document, which the caller stores.
        Linked,

        // Left out of the document.
        LeftOut
    }

    // How the SVG target writes the bitmaps drawn into it, as the export sets it (B-3, B-4). A bitmap is written in
    // its own format when that is JPEG, at the quality given here, and as PNG otherwise.
    public sealed class SvgBitmapOptions
    {
        // How bitmaps reach the document.
        public readonly SvgBitmapHandling Handling;

        // The quality, from 0 to 100, at which a bitmap whose own format is JPEG is written.
        public readonly int JpegQuality;

        // For linked bitmaps: stores the written data of a bitmap, in the format given, beside the document, and returns
        // the reference by which the document links to it, such as a file name relative to the document. The target
        // stores each bitmap once, however often it is drawn.
        public readonly Func<byte[], GraphicsBitmapFormat, string> StoreLinkedBitmap;

        // The options when the export sets none: embedded, and JPEG at the quality of 90.
        public static readonly SvgBitmapOptions Default = new SvgBitmapOptions(SvgBitmapHandling.Embedded, 90, null);

        // Create options.
        // handling: how bitmaps reach the document.
        // jpegQuality: the quality, from 0 to 100, of bitmaps written as JPEG.
        // storeLinkedBitmap: for linked bitmaps, stores a bitmap's data and returns the reference to it; null otherwise.
        public SvgBitmapOptions(SvgBitmapHandling handling, int jpegQuality, Func<byte[], GraphicsBitmapFormat, string> storeLinkedBitmap)
        {
            if (jpegQuality < 0 || jpegQuality > 100)
                throw new ArgumentOutOfRangeException(nameof(jpegQuality), "The JPEG quality lies from 0 to 100.");
            if (handling == SvgBitmapHandling.Linked && storeLinkedBitmap == null)
                throw new ArgumentNullException(nameof(storeLinkedBitmap), "Linked bitmaps need a store for their files.");
            Handling = handling;
            JpegQuality = jpegQuality;
            StoreLinkedBitmap = storeLinkedBitmap;
        }
    }
}
