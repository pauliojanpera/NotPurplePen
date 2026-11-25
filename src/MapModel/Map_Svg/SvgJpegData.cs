using System;

namespace PurplePen.MapModel
{
    // Reads the headers of JPEG data, up to its first scan, to tell whether the browsers draw the data as the application
    // draws the bitmap it decoded from it, so that the SVG target can write the data itself rather than encode the bitmap
    // anew (B-3). The application draws the pixels the data codes and nothing else: it applies neither a colour profile
    // that the data carries nor a turn or mirroring that its Exif data declares, and Chromium and WebKit apply both.
    // Chromium, Firefox and WebKit draw the other JPEG data the application decodes, baseline and progressive, grey,
    // colour and CMYK, as it draws it, CMYK within a level. Codings that not every browser decodes, arithmetic, lossless,
    // hierarchical or of more than 8 bits, are left to the encoder too.
    public static class SvgJpegData
    {
        // The codes of the markers that the headers are read by, each following a byte 0xFF.
        private const int StartOfImage = 0xD8, StartOfScan = 0xDA;
        private const int BaselineFrame = 0xC0, ExtendedSequentialFrame = 0xC1, ProgressiveFrame = 0xC2;
        private const int HuffmanTables = 0xC4, QuantizationTables = 0xDB, RestartInterval = 0xDD, Comment = 0xFE;
        private const int FirstApplicationSegment = 0xE0, ExifSegment = 0xE1, ProfileSegment = 0xE2, LastApplicationSegment = 0xEF;

        // How an Exif segment and a profile segment begin.
        private static readonly byte[] ExifIdentifier = { (byte)'E', (byte)'x', (byte)'i', (byte)'f', 0, 0 };
        private static readonly byte[] ProfileIdentifier = { (byte)'I', (byte)'C', (byte)'C', (byte)'_', (byte)'P', (byte)'R', (byte)'O', (byte)'F', (byte)'I', (byte)'L', (byte)'E', 0 };

        // The tag of the orientation in an Exif image file directory, the type of its value, a SHORT, and the value that
        // leaves the image upright.
        private const int OrientationTag = 0x0112, ShortType = 3, Upright = 1;

        // Whether the browsers draw JPEG data as the application draws the bitmap it decoded from it: data of an image
        // of the bitmap's size, Huffman-coded, sequential or progressive, in 8 bits, of one, three or four channels,
        // with neither a colour profile nor an Exif orientation other than upright, and with nothing before its scan
        // but its frame and the tables, comments and application segments that go with it. Data cut short or otherwise
        // unreadable is not.
        // data: the JPEG data.
        // width, height: the size of the bitmap, in pixels.
        public static bool IsDrawnAsDecoded(byte[] data, int width, int height)
        {
            if (data == null || data.Length < 2 || data[0] != 0xFF || data[1] != StartOfImage)
                return false;

            bool frameRead = false;
            int offset = 2;
            for (;;) {
                // A marker is a byte 0xFF, which more of them may precede, and its code. All but the start of the scan
                // begin a segment of the length its next two bytes give, those two included.
                if (offset >= data.Length || data[offset] != 0xFF)
                    return false;
                while (offset < data.Length && data[offset] == 0xFF)
                    ++offset;
                if (offset >= data.Length)
                    return false;
                int marker = data[offset++];
                if (marker == StartOfScan)
                    return frameRead;
                if (offset + 2 > data.Length)
                    return false;
                int length = ReadUInt16(data, offset, true);
                if (length < 2 || offset + length > data.Length)
                    return false;
                int content = offset + 2, contentLength = length - 2;

                switch (marker) {
                case BaselineFrame:
                case ExtendedSequentialFrame:
                case ProgressiveFrame:
                    if (frameRead || !IsFrameDrawnAsDecoded(data, content, contentLength, width, height))
                        return false;
                    frameRead = true;
                    break;
                case HuffmanTables:
                case QuantizationTables:
                case RestartInterval:
                case Comment:
                    break;
                case ExifSegment:
                    if (!IsUpright(data, content, contentLength))
                        return false;
                    break;
                case ProfileSegment:
                    if (StartsWith(data, content, contentLength, ProfileIdentifier))
                        return false;
                    break;
                default:
                    if (marker < FirstApplicationSegment || marker > LastApplicationSegment)
                        return false;
                    break;
                }
                offset += length;
            }
        }

        // Whether the header of a Huffman-coded frame is of 8 bits, of one, three or four channels, and of the bitmap's
        // size: its precision, its height and width, of two bytes each, and its channels, each of three bytes.
        // data: the JPEG data.
        // start, length: where the header's content lies in the data.
        // width, height: the size of the bitmap, in pixels.
        private static bool IsFrameDrawnAsDecoded(byte[] data, int start, int length, int width, int height)
        {
            if (length < 6)
                return false;
            int channels = data[start + 5];
            return data[start] == 8 && ReadUInt16(data, start + 1, true) == height && ReadUInt16(data, start + 3, true) == width &&
                   (channels == 1 || channels == 3 || channels == 4) && length == 6 + 3 * channels;
        }

        // Whether an application segment of the kind Exif data goes in leaves the image as it is coded: a segment that
        // holds no Exif data, or Exif data whose first image file directory holds no orientation, or the upright one, a
        // SHORT of value 1. Exif data that cannot be read is taken to turn the image, as a browser may read it otherwise.
        // data: the JPEG data.
        // start, length: where the segment's content lies in the data.
        private static bool IsUpright(byte[] data, int start, int length)
        {
            if (!StartsWith(data, start, length, ExifIdentifier))
                return true;

            // A TIFF header follows: the byte order, II or MM, the number 42, and where the first directory lies, which
            // is its number of entries and the entries, of twelve bytes each: a tag, a type, a count and a value.
            int tiff = start + ExifIdentifier.Length, tiffLength = length - ExifIdentifier.Length;
            if (tiffLength < 8)
                return false;
            bool bigEndian;
            if (data[tiff] == 'M' && data[tiff + 1] == 'M')
                bigEndian = true;
            else if (data[tiff] == 'I' && data[tiff + 1] == 'I')
                bigEndian = false;
            else
                return false;
            if (ReadUInt16(data, tiff + 2, bigEndian) != 42)
                return false;

            long directory = ReadUInt32(data, tiff + 4, bigEndian);
            if (directory < 8 || directory + 2 > tiffLength)
                return false;
            int entries = ReadUInt16(data, tiff + (int)directory, bigEndian);
            if (directory + 2 + 12L * entries > tiffLength)
                return false;
            for (int index = 0; index < entries; ++index) {
                int entry = tiff + (int)directory + 2 + 12 * index;
                if (ReadUInt16(data, entry, bigEndian) == OrientationTag) {
                    return ReadUInt16(data, entry + 2, bigEndian) == ShortType && ReadUInt32(data, entry + 4, bigEndian) == 1 &&
                           ReadUInt16(data, entry + 8, bigEndian) == Upright;
                }
            }
            return true;
        }

        // Whether a segment's content begins with an identifier.
        // data: the JPEG data.
        // start, length: where the content lies in the data.
        // identifier: the identifier.
        private static bool StartsWith(byte[] data, int start, int length, byte[] identifier)
        {
            return length >= identifier.Length && data.AsSpan(start, identifier.Length).SequenceEqual(identifier);
        }

        // Read a number of two bytes.
        // data: the data.
        // offset: where the number lies.
        // bigEndian: whether its high byte comes first.
        private static int ReadUInt16(byte[] data, int offset, bool bigEndian)
        {
            return bigEndian ? (data[offset] << 8) | data[offset + 1] : (data[offset + 1] << 8) | data[offset];
        }

        // Read a number of four bytes.
        // data: the data.
        // offset: where the number lies.
        // bigEndian: whether its high byte comes first.
        private static long ReadUInt32(byte[] data, int offset, bool bigEndian)
        {
            return bigEndian ? ((long)data[offset] << 24) | ((long)data[offset + 1] << 16) | ((long)data[offset + 2] << 8) | data[offset + 3]
                             : ((long)data[offset + 3] << 24) | ((long)data[offset + 2] << 16) | ((long)data[offset + 1] << 8) | data[offset];
        }
    }
}
