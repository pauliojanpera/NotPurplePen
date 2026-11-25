using System;
using System.Collections.Generic;
using System.Linq;

namespace Map_Svg.Tests
{
    // Builds JPEG data for the tests of how the SVG target reads its headers: the start of the image, the segments
    // given, and a scan, whose coded data the target never reads, then the end of the image.
    internal static class TestJpegData
    {
        // The codes of the markers the tests build segments of.
        public const int BaselineFrame = 0xC0, ExtendedSequentialFrame = 0xC1, ProgressiveFrame = 0xC2, LosslessFrame = 0xC3;
        public const int HierarchicalFrame = 0xC5, ArithmeticFrame = 0xC9, ArithmeticProgressiveFrame = 0xCA;
        public const int HuffmanTables = 0xC4, ArithmeticConditioning = 0xCC, QuantizationTables = 0xDB, RestartInterval = 0xDD;
        public const int HierarchicalProgression = 0xDE, Comment = 0xFE, JfifSegment = 0xE0, ExifSegment = 0xE1, ProfileSegment = 0xE2;
        public const int AdobeSegment = 0xEE;

        // The tag of the orientation in an Exif image file directory, and the types of a SHORT and a LONG.
        public const int OrientationTag = 0x0112, ShortType = 3, LongType = 4;

        // JPEG data of the segments given: the start of the image, the segments, and a scan of three channels.
        // segments: the segments, each with its marker.
        public static byte[] Data(params byte[][] segments)
        {
            byte[] scan = { 0xFF, 0xDA, 0, 12, 3, 1, 0, 2, 0x11, 3, 0x11, 0, 63, 0, 0x12, 0x34, 0xFF, 0xD9 };
            return new byte[] { 0xFF, 0xD8 }.Concat(segments.SelectMany(segment => segment)).Concat(scan).ToArray();
        }

        // JPEG data as most writers write it: a JFIF segment, the tables, and a baseline frame of three channels.
        // width, height: the size of the image, in pixels.
        public static byte[] Plain(int width, int height)
        {
            return Data(Jfif(), Segment(QuantizationTables, new byte[65]), Frame(BaselineFrame, 8, width, height, 3), Segment(HuffmanTables, new byte[29]));
        }

        // A segment: its marker, its length and its content.
        // marker: the code of its marker.
        // content: what it holds.
        public static byte[] Segment(int marker, byte[] content)
        {
            int length = content.Length + 2;
            return new byte[] { 0xFF, (byte)marker, (byte)(length >> 8), (byte)length }.Concat(content).ToArray();
        }

        // A JFIF segment, version 1.1, that says only that the pixels are square.
        public static byte[] Jfif()
        {
            return Segment(JfifSegment, new byte[] { (byte)'J', (byte)'F', (byte)'I', (byte)'F', 0, 1, 1, 0, 0, 1, 0, 1, 0, 0 });
        }

        // A frame header.
        // marker: the code of its marker, which says how the frame is coded.
        // precision: the bits of each sample.
        // width, height: the size of the image, in pixels.
        // channels: the number of channels.
        public static byte[] Frame(int marker, int precision, int width, int height, int channels)
        {
            List<byte> content = new List<byte> { (byte)precision, (byte)(height >> 8), (byte)height, (byte)(width >> 8), (byte)width, (byte)channels };
            for (int channel = 1; channel <= channels; ++channel)
                content.AddRange(new byte[] { (byte)channel, channel == 1 ? (byte)0x22 : (byte)0x11, 0 });
            return Segment(marker, content.ToArray());
        }

        // An Exif segment whose first image file directory holds an entry for each tag given, the orientation among
        // them, each with a value of one item.
        // bigEndian: whether its numbers are written high byte first.
        // entries: each entry's tag, type and value.
        public static byte[] Exif(bool bigEndian, params (int Tag, int Type, int Value)[] entries)
        {
            List<byte> tiff = new List<byte>();
            tiff.AddRange(bigEndian ? new byte[] { (byte)'M', (byte)'M' } : new byte[] { (byte)'I', (byte)'I' });
            tiff.AddRange(Number(42, 2, bigEndian));
            tiff.AddRange(Number(8, 4, bigEndian));
            tiff.AddRange(Number(entries.Length, 2, bigEndian));
            foreach ((int tag, int type, int value) in entries) {
                tiff.AddRange(Number(tag, 2, bigEndian));
                tiff.AddRange(Number(type, 2, bigEndian));
                tiff.AddRange(Number(1, 4, bigEndian));
                tiff.AddRange(type == ShortType ? Number(value, 2, bigEndian).Concat(new byte[2]) : Number(value, 4, bigEndian));
            }
            tiff.AddRange(Number(0, 4, bigEndian));
            return Segment(ExifSegment, new byte[] { (byte)'E', (byte)'x', (byte)'i', (byte)'f', 0, 0 }.Concat(tiff).ToArray());
        }

        // An Exif segment whose orientation is a SHORT of the value given, its numbers written low byte first.
        // orientation: the orientation, 1 for upright.
        public static byte[] ExifOrientation(int orientation)
        {
            return Exif(false, (OrientationTag, ShortType, orientation));
        }

        // A segment that holds a colour profile, or the first part of one.
        public static byte[] Profile()
        {
            byte[] identifier = { (byte)'I', (byte)'C', (byte)'C', (byte)'_', (byte)'P', (byte)'R', (byte)'O', (byte)'F', (byte)'I', (byte)'L', (byte)'E', 0 };
            return Segment(ProfileSegment, identifier.Concat(new byte[] { 1, 1 }).Concat(new byte[128]).ToArray());
        }

        // A number of a length, in a byte order.
        // value: the number.
        // length: its length in bytes.
        // bigEndian: whether its high byte comes first.
        private static IEnumerable<byte> Number(int value, int length, bool bigEndian)
        {
            byte[] bytes = new byte[length];
            for (int index = 0; index < length; ++index)
                bytes[bigEndian ? length - 1 - index : index] = (byte)(value >> (8 * index));
            return bytes;
        }
    }
}
