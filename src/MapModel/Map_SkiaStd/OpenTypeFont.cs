using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Map_SkiaStd
{
    // The tables of one face of an OpenType font, read from a font file or from a face of a TrueType collection, and
    // written back as a standalone font: the table directory sorted by tag, each table padded to four bytes, with its
    // checksum, and the font's checksum adjustment in the head table, as the OpenType specification lays a font out.
    public sealed class OpenTypeFont
    {
        // The tables, by their tags.
        public readonly SortedDictionary<string, byte[]> Tables = new SortedDictionary<string, byte[]>(StringComparer.Ordinal);

        // The version of the font's outlines: 0x00010000 for TrueType outlines, "OTTO" for the Compact Font Format.
        public uint SfntVersion;

        // Where in the design space of a variable font the application draws the face (VariableFont): the coordinate
        // of each axis named, by the axis's tag, the others at their defaults; null for the default location.
        public IDictionary<string, float> Location;

        // Read a face of a font file.
        // data: the font file, a single font or a TrueType collection.
        // collectionIndex: the face within a collection; 0 for a single font.
        public static OpenTypeFont Read(byte[] data, int collectionIndex)
        {
            int directory = 0;
            if (ReadUInt32(data, 0) == 0x74746366) {   // "ttcf"
                int faces = (int)ReadUInt32(data, 8);
                if (collectionIndex < 0 || collectionIndex >= faces)
                    throw new ArgumentOutOfRangeException(nameof(collectionIndex), "The collection holds " + faces + " faces.");
                directory = (int)ReadUInt32(data, 12 + 4 * collectionIndex);
            }

            OpenTypeFont font = new OpenTypeFont { SfntVersion = ReadUInt32(data, directory) };
            int tableCount = ReadUInt16(data, directory + 4);
            for (int index = 0; index < tableCount; ++index) {
                int record = directory + 12 + 16 * index;
                string tag = Encoding.ASCII.GetString(data, record, 4);
                int offset = (int)ReadUInt32(data, record + 8), length = (int)ReadUInt32(data, record + 12);
                byte[] table = new byte[length];
                Buffer.BlockCopy(data, offset, table, 0, length);
                font.Tables[tag] = table;
            }
            return font;
        }

        // The font's embedding permissions, the fsType field of its OS/2 table, or 0, installable, when it has none.
        public int EmbeddingPermissions
        {
            get {
                byte[] os2;
                return Tables.TryGetValue("OS/2", out os2) && os2.Length >= 10 ? ReadUInt16(os2, 8) : 0;
            }
        }

        // Whether the font's permissions forbid embedding it in a document: a restricted license with no less
        // restrictive permission beside it, or embedding allowed for bitmaps alone, which a document of outlines
        // cannot honor.
        public bool ForbidsEmbedding
        {
            get {
                int permissions = EmbeddingPermissions;
                return (permissions & 0x000E) == 0x0002 || (permissions & 0x0200) != 0;
            }
        }

        // Whether the font's permissions forbid embedding a subset of it, so that it is embedded whole.
        public bool ForbidsSubsetting
        {
            get { return (EmbeddingPermissions & 0x0100) != 0; }
        }

        // Write the font as a standalone file.
        public byte[] Write()
        {
            int tableCount = Tables.Count;
            int power = 1, log2 = 0;
            while (power * 2 <= tableCount) {
                power *= 2;
                ++log2;
            }

            using (MemoryStream stream = new MemoryStream()) {
                byte[] header = new byte[12 + 16 * tableCount];
                WriteUInt32(header, 0, SfntVersion);
                WriteUInt16(header, 4, tableCount);
                WriteUInt16(header, 6, power * 16);
                WriteUInt16(header, 8, log2);
                WriteUInt16(header, 10, tableCount * 16 - power * 16);

                int offset = header.Length;
                int headOffset = -1;
                List<byte[]> bodies = new List<byte[]>();
                int index = 0;
                foreach (KeyValuePair<string, byte[]> table in Tables) {
                    byte[] body = table.Value;
                    if (table.Key == "head") {
                        body = (byte[])body.Clone();
                        WriteUInt32(body, 8, 0);   // checksum adjustment, set once the whole font is known
                        headOffset = offset;
                    }
                    int record = 12 + 16 * index++;
                    Buffer.BlockCopy(Encoding.ASCII.GetBytes(table.Key), 0, header, record, 4);
                    WriteUInt32(header, record + 4, Checksum(body, 0, body.Length));
                    WriteUInt32(header, record + 8, (uint)offset);
                    WriteUInt32(header, record + 12, (uint)body.Length);
                    bodies.Add(body);
                    offset += (body.Length + 3) & ~3;
                }

                stream.Write(header, 0, header.Length);
                foreach (byte[] body in bodies) {
                    stream.Write(body, 0, body.Length);
                    stream.Write(new byte[3], 0, ((body.Length + 3) & ~3) - body.Length);
                }

                byte[] font = stream.ToArray();
                if (headOffset >= 0)
                    WriteUInt32(font, headOffset + 8, unchecked(0xB1B0AFBA - Checksum(font, 0, font.Length)));
                return font;
            }
        }

        // The checksum of data as OpenType computes it: the sum of its big-endian 32-bit words, the last padded with
        // zeros.
        // data: the data.
        // offset, length: the part of the data to sum.
        public static uint Checksum(byte[] data, int offset, int length)
        {
            uint sum = 0;
            for (int index = 0; index < length; index += 4) {
                uint word = 0;
                for (int shift = 0; shift < 4; ++shift)
                    word = (word << 8) | (index + shift < length ? data[offset + index + shift] : (byte)0);
                sum = unchecked(sum + word);
            }
            return sum;
        }

        // Read a big-endian unsigned 16-bit number.
        // data: the data.
        // offset: where the number starts.
        public static int ReadUInt16(byte[] data, int offset)
        {
            return (data[offset] << 8) | data[offset + 1];
        }

        // Read a big-endian unsigned 32-bit number.
        // data: the data.
        // offset: where the number starts.
        public static uint ReadUInt32(byte[] data, int offset)
        {
            return ((uint)data[offset] << 24) | ((uint)data[offset + 1] << 16) | ((uint)data[offset + 2] << 8) | data[offset + 3];
        }

        // Write a big-endian unsigned 16-bit number.
        // data: the data.
        // offset: where the number starts.
        // value: the number.
        public static void WriteUInt16(byte[] data, int offset, int value)
        {
            data[offset] = (byte)(value >> 8);
            data[offset + 1] = (byte)value;
        }

        // Write a big-endian unsigned 32-bit number.
        // data: the data.
        // offset: where the number starts.
        // value: the number.
        public static void WriteUInt32(byte[] data, int offset, uint value)
        {
            data[offset] = (byte)(value >> 24);
            data[offset + 1] = (byte)(value >> 16);
            data[offset + 2] = (byte)(value >> 8);
            data[offset + 3] = (byte)value;
        }
    }
}
