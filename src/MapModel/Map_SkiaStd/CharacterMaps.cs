using System;
using System.Collections.Generic;

namespace Map_SkiaStd
{
    // Reads and writes the subtables of a font's character map, the cmap table, which maps characters to glyphs.
    internal static class CharacterMaps
    {
        // The most code points a character map is read to: the subsets and symbol fonts it is read for map few.
        private const int MostMappings = 1 << 20;

        // Read a subtable of format 4, 6 or 12. Return false for one of another format, or one that does not fit in the
        // table.
        // cmap: the table.
        // offset: where the subtable starts.
        // mappings: receives the glyph of each character mapped.
        public static bool ReadSubtable(byte[] cmap, int offset, SortedDictionary<int, int> mappings)
        {
            if (offset < 0 || offset + 4 > cmap.Length)
                return false;
            switch (OpenTypeFont.ReadUInt16(cmap, offset)) {
                case 4:
                    return ReadFormat4(cmap, offset, mappings);
                case 6:
                    return ReadFormat6(cmap, offset, mappings);
                case 12:
                    return ReadFormat12(cmap, offset, mappings);
                default:
                    return false;
            }
        }

        // Read a subtable of format 4: segments of characters, each mapped to glyphs by a delta or through an array.
        // cmap: the table.
        // offset: where the subtable starts.
        // mappings: receives the glyph of each character mapped.
        private static bool ReadFormat4(byte[] cmap, int offset, SortedDictionary<int, int> mappings)
        {
            int length = OpenTypeFont.ReadUInt16(cmap, offset + 2), segmentsTimesTwo = OpenTypeFont.ReadUInt16(cmap, offset + 6);
            if (offset + length > cmap.Length || 16 + 4 * segmentsTimesTwo > length)
                return false;
            int ends = offset + 14, starts = ends + segmentsTimesTwo + 2, deltas = starts + segmentsTimesTwo, rangeOffsets = deltas + segmentsTimesTwo;
            for (int segment = 0; segment < segmentsTimesTwo / 2; ++segment) {
                int start = OpenTypeFont.ReadUInt16(cmap, starts + 2 * segment), end = OpenTypeFont.ReadUInt16(cmap, ends + 2 * segment);
                int delta = OpenTypeFont.ReadUInt16(cmap, deltas + 2 * segment);
                int rangeOffsetAt = rangeOffsets + 2 * segment, rangeOffset = OpenTypeFont.ReadUInt16(cmap, rangeOffsetAt);
                for (int character = start; character <= end && character < 0xFFFF; ++character) {
                    int glyph;
                    if (rangeOffset == 0) {
                        glyph = (character + delta) & 0xFFFF;
                    }
                    else {
                        int at = rangeOffsetAt + rangeOffset + 2 * (character - start);
                        if (at + 2 > offset + length)
                            return false;
                        glyph = OpenTypeFont.ReadUInt16(cmap, at);
                        if (glyph != 0)
                            glyph = (glyph + delta) & 0xFFFF;
                    }
                    if (glyph != 0)
                        mappings[character] = glyph;
                }
            }
            return true;
        }

        // Read a subtable of format 6: consecutive characters from a first one, each with its glyph.
        // cmap: the table.
        // offset: where the subtable starts.
        // mappings: receives the glyph of each character mapped.
        private static bool ReadFormat6(byte[] cmap, int offset, SortedDictionary<int, int> mappings)
        {
            if (offset + 10 > cmap.Length)
                return false;
            int first = OpenTypeFont.ReadUInt16(cmap, offset + 6), count = OpenTypeFont.ReadUInt16(cmap, offset + 8);
            if (offset + 10 + 2 * count > cmap.Length)
                return false;
            for (int index = 0; index < count; ++index) {
                int glyph = OpenTypeFont.ReadUInt16(cmap, offset + 10 + 2 * index);
                if (glyph != 0)
                    mappings[first + index] = glyph;
            }
            return true;
        }

        // Read a subtable of format 12: groups of consecutive characters mapped to consecutive glyphs.
        // cmap: the table.
        // offset: where the subtable starts.
        // mappings: receives the glyph of each character mapped.
        private static bool ReadFormat12(byte[] cmap, int offset, SortedDictionary<int, int> mappings)
        {
            if (offset + 16 > cmap.Length)
                return false;
            long groups = OpenTypeFont.ReadUInt32(cmap, offset + 12);
            if (offset + 16 + 12 * groups > cmap.Length)
                return false;
            for (int group = 0; group < groups; ++group) {
                int at = offset + 16 + 12 * group;
                long start = OpenTypeFont.ReadUInt32(cmap, at), end = OpenTypeFont.ReadUInt32(cmap, at + 4), glyph = OpenTypeFont.ReadUInt32(cmap, at + 8);
                if (end < start || end > 0x10FFFF || mappings.Count + (end - start) > MostMappings)
                    return false;
                for (long character = start; character <= end; ++character) {
                    if (glyph + (character - start) != 0)
                        mappings[(int)character] = (int)(glyph + (character - start));
                }
            }
            return true;
        }

        // Write a character map as HarfBuzz's subsetter writes one: a subtable of format 4 for the Basic Multilingual
        // Plane, for Unicode and Windows alike; one of format 12 for every plane when a character lies beyond it; and the
        // subtable of variation sequences, if any, as it was. Return null if the subtable of format 4 would be too long.
        // mappings: the glyph of each character mapped.
        // variations: the subtable of variation sequences, or null.
        public static byte[] Write(SortedDictionary<int, int> mappings, byte[] variations)
        {
            List<int[]> basicRuns = Runs(mappings, 0xFFFE), fullRuns = Runs(mappings, 0x10FFFF);
            bool beyondBasic = false;
            foreach (int character in mappings.Keys)
                beyondBasic |= character > 0xFFFF;

            // Format 4: the runs as segments mapped by a delta, and the segment that ends the table.
            int segments = basicRuns.Count + 1;
            int basicLength = 16 + 8 * segments;
            if (basicLength > 0xFFFF)
                return null;
            byte[] basic = new byte[basicLength];
            int power = 1, log2 = 0;
            while (power * 2 <= segments) {
                power *= 2;
                ++log2;
            }
            OpenTypeFont.WriteUInt16(basic, 0, 4);
            OpenTypeFont.WriteUInt16(basic, 2, basicLength);
            OpenTypeFont.WriteUInt16(basic, 6, 2 * segments);
            OpenTypeFont.WriteUInt16(basic, 8, 2 * power);
            OpenTypeFont.WriteUInt16(basic, 10, log2);
            OpenTypeFont.WriteUInt16(basic, 12, 2 * segments - 2 * power);
            int ends = 14, starts = ends + 2 * segments + 2, deltas = starts + 2 * segments;
            for (int segment = 0; segment < segments; ++segment) {
                int start = segment < basicRuns.Count ? basicRuns[segment][0] : 0xFFFF;
                int end = segment < basicRuns.Count ? basicRuns[segment][1] : 0xFFFF;
                int glyph = segment < basicRuns.Count ? basicRuns[segment][2] : 0;
                OpenTypeFont.WriteUInt16(basic, ends + 2 * segment, end);
                OpenTypeFont.WriteUInt16(basic, starts + 2 * segment, start);
                OpenTypeFont.WriteUInt16(basic, deltas + 2 * segment, (glyph - start) & 0xFFFF);
            }

            // Format 12: the runs as groups.
            byte[] full = null;
            if (beyondBasic) {
                full = new byte[16 + 12 * fullRuns.Count];
                OpenTypeFont.WriteUInt16(full, 0, 12);
                OpenTypeFont.WriteUInt32(full, 4, (uint)full.Length);
                OpenTypeFont.WriteUInt32(full, 12, (uint)fullRuns.Count);
                for (int group = 0; group < fullRuns.Count; ++group) {
                    OpenTypeFont.WriteUInt32(full, 16 + 12 * group, (uint)fullRuns[group][0]);
                    OpenTypeFont.WriteUInt32(full, 20 + 12 * group, (uint)fullRuns[group][1]);
                    OpenTypeFont.WriteUInt32(full, 24 + 12 * group, (uint)fullRuns[group][2]);
                }
            }

            // The encoding records, in the order of their platforms and encodings, and the subtables they point to.
            List<int[]> records = new List<int[]>();   // platform, encoding, subtable: 0 basic, 1 full, 2 variations
            records.Add(new int[] { 0, 3, 0 });
            if (full != null)
                records.Add(new int[] { 0, 4, 1 });
            if (variations != null)
                records.Add(new int[] { 0, 5, 2 });
            records.Add(new int[] { 3, 1, 0 });
            if (full != null)
                records.Add(new int[] { 3, 10, 1 });
            int headerLength = 4 + 8 * records.Count;
            int[] subtableOffsets = { headerLength, headerLength + basic.Length, headerLength + basic.Length + (full == null ? 0 : full.Length) };
            byte[] cmap = new byte[subtableOffsets[2] + (variations == null ? 0 : variations.Length)];
            OpenTypeFont.WriteUInt16(cmap, 2, records.Count);
            for (int index = 0; index < records.Count; ++index) {
                OpenTypeFont.WriteUInt16(cmap, 4 + 8 * index, records[index][0]);
                OpenTypeFont.WriteUInt16(cmap, 6 + 8 * index, records[index][1]);
                OpenTypeFont.WriteUInt32(cmap, 8 + 8 * index, (uint)subtableOffsets[records[index][2]]);
            }
            Buffer.BlockCopy(basic, 0, cmap, subtableOffsets[0], basic.Length);
            if (full != null)
                Buffer.BlockCopy(full, 0, cmap, subtableOffsets[1], full.Length);
            if (variations != null)
                Buffer.BlockCopy(variations, 0, cmap, subtableOffsets[2], variations.Length);
            return cmap;
        }

        // A part of data, or null if it does not lie within the data.
        // data: the data.
        // offset: where the part starts.
        // length: its length.
        public static byte[] Slice(byte[] data, int offset, long length)
        {
            if (offset < 0 || length < 0 || offset + length > data.Length)
                return null;
            byte[] part = new byte[length];
            Buffer.BlockCopy(data, offset, part, 0, (int)length);
            return part;
        }

        // The runs of characters up to a last one that map to consecutive glyphs: first character, last character and
        // the glyph of the first.
        // mappings: the glyph of each character mapped.
        // last: the last character the runs reach.
        private static List<int[]> Runs(SortedDictionary<int, int> mappings, int last)
        {
            List<int[]> runs = new List<int[]>();
            foreach (KeyValuePair<int, int> mapping in mappings) {
                if (mapping.Key > last)
                    break;
                int[] run = runs.Count > 0 ? runs[runs.Count - 1] : null;
                if (run != null && mapping.Key == run[1] + 1 && mapping.Value == run[2] + (mapping.Key - run[0]))
                    run[1] = mapping.Key;
                else
                    runs.Add(new int[] { mapping.Key, mapping.Key, mapping.Value });
            }
            return runs;
        }
    }
}
