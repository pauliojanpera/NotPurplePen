using System;
using System.Collections.Generic;

namespace Map_SkiaStd
{
    // Puts a face's box, its .notdef glyph, under the characters that the face lacks and draws as that box (T-7). Where
    // the application draws such a character as the face's .notdef, a browser draws it from a font of the viewer's
    // system, or as a box of its own, as it draws any character that no embedded face maps. Mapped to a copy of
    // .notdef, the character is drawn from the face, as any other is.
    //
    // The copy is added to a subset of TrueType outlines, which nearly every font that a map names has, as a glyph
    // after the others: the glyph data and its location, its metrics, the count of glyphs, and the character map. A
    // subset of CFF outlines, one with a table that lists something for each glyph that the copy would have to follow,
    // such as the variations of a variable font, and one with a character map of a symbol font, is left as it is, and
    // its characters fall back to the viewer's fonts.
    internal static class MissingCharacterBox
    {
        // The tables a subset may hold for the copy to be added: those the copy changes, and those that list nothing
        // for each glyph.
        private static readonly HashSet<string> TablesAllowed = new HashSet<string>(StringComparer.Ordinal) {
            "cmap", "head", "hhea", "hmtx", "maxp", "loca", "glyf", "vhea", "vmtx", "OS/2", "name", "post",
            "GDEF", "GSUB", "GPOS", "BASE", "MATH", "COLR", "CPAL", "STAT", "meta", "kern", "cvt ", "fpgm", "prep", "gasp", "VDMX",
        };

        // The tables the copy changes, which a subset of TrueType outlines holds.
        private static readonly string[] TablesRequired = { "cmap", "head", "hhea", "hmtx", "maxp", "loca", "glyf", "post" };

        // The most code points the character map of a subset is read to: a subset maps few.
        private const int MostMappings = 1 << 20;

        // Add to a subset a copy of its .notdef glyph, and map characters to it. Return the subset with the copy, or the
        // subset as it is when its tables are not all ones the copy can be added to.
        // fontData: the subset.
        // characters: the characters, as Unicode code points, that the face lacks and draws as its box.
        public static byte[] Add(byte[] fontData, IList<int> characters)
        {
            OpenTypeFont font = OpenTypeFont.Read(fontData, 0);
            if (font.SfntVersion != 0x00010000 && font.SfntVersion != 0x74727565)   // TrueType outlines, or "true"
                return fontData;
            foreach (string tag in font.Tables.Keys) {
                if (!TablesAllowed.Contains(tag))
                    return fontData;
            }
            foreach (string tag in TablesRequired) {
                if (!font.Tables.ContainsKey(tag))
                    return fontData;
            }
            byte[] post = font.Tables["post"];
            if (post.Length < 4 || OpenTypeFont.ReadUInt32(post, 0) != 0x00030000)   // glyph names would need one more
                return fontData;

            SortedDictionary<int, int> mappings;
            byte[] variations;
            if (!ReadCharacterMap(font.Tables["cmap"], out mappings, out variations))
                return fontData;

            if (font.Tables["maxp"].Length < 6)
                return fontData;
            int glyphCount = OpenTypeFont.ReadUInt16(font.Tables["maxp"], 4);
            if (glyphCount == 0 || glyphCount >= 0xFFFF)
                return fontData;
            int[] locations = ReadLocations(font, glyphCount);
            if (locations == null)
                return fontData;
            int horizontalLongCount, verticalLongCount = 0;
            byte[] horizontalMetrics = AppendMetrics(font.Tables["hhea"], font.Tables["hmtx"], glyphCount, out horizontalLongCount);
            byte[] verticalMetrics = null;
            if (font.Tables.ContainsKey("vmtx")) {
                if (!font.Tables.ContainsKey("vhea"))
                    return fontData;
                verticalMetrics = AppendMetrics(font.Tables["vhea"], font.Tables["vmtx"], glyphCount, out verticalLongCount);
                if (verticalMetrics == null)
                    return fontData;
            }
            if (horizontalMetrics == null)
                return fontData;

            int box = glyphCount;
            foreach (int character in characters)
                mappings[character] = box;
            byte[] characterMap = WriteCharacterMap(mappings, variations);
            if (characterMap == null)
                return fontData;

            AppendGlyph(font, locations, glyphCount);
            ReplaceMetrics(font, "hhea", "hmtx", horizontalMetrics, horizontalLongCount);
            if (verticalMetrics != null)
                ReplaceMetrics(font, "vhea", "vmtx", verticalMetrics, verticalLongCount);
            byte[] maximums = (byte[])font.Tables["maxp"].Clone();
            OpenTypeFont.WriteUInt16(maximums, 4, glyphCount + 1);
            font.Tables["maxp"] = maximums;
            font.Tables["cmap"] = characterMap;
            WriteCharacterRange(font, mappings);
            return font.Write();
        }

        // Read where each glyph's data starts in glyf, and where the last one ends, or return null if the locations do
        // not lie in order within the table.
        // font: the font.
        // glyphCount: the count of glyphs.
        private static int[] ReadLocations(OpenTypeFont font, int glyphCount)
        {
            byte[] head = font.Tables["head"], loca = font.Tables["loca"];
            if (head.Length < 54)
                return null;
            bool longOffsets = OpenTypeFont.ReadUInt16(head, 50) != 0;
            if (loca.Length < (glyphCount + 1) * (longOffsets ? 4 : 2))
                return null;
            int[] locations = new int[glyphCount + 1];
            for (int glyph = 0; glyph <= glyphCount; ++glyph) {
                long location = longOffsets ? OpenTypeFont.ReadUInt32(loca, 4 * glyph) : 2 * OpenTypeFont.ReadUInt16(loca, 2 * glyph);
                if (location > font.Tables["glyf"].Length || (glyph > 0 && location < locations[glyph - 1]))
                    return null;
                locations[glyph] = (int)location;
            }
            return locations;
        }

        // Append a copy of the .notdef glyph's data to glyf, and its location to loca, which takes long offsets when the
        // short ones no longer reach.
        // font: the font.
        // locations: where each glyph's data starts, and where the last one ends.
        // glyphCount: the count of glyphs before the copy.
        private static void AppendGlyph(OpenTypeFont font, int[] locations, int glyphCount)
        {
            byte[] head = font.Tables["head"], glyf = font.Tables["glyf"];
            bool longOffsets = OpenTypeFont.ReadUInt16(head, 50) != 0;
            int notdefLength = locations[1] - locations[0], end = locations[glyphCount];
            int paddedLength = (notdefLength + 3) & ~3;
            if (!longOffsets && (end + paddedLength) / 2 > 0xFFFF) {
                longOffsets = true;
                head = (byte[])head.Clone();
                OpenTypeFont.WriteUInt16(head, 50, 1);
                font.Tables["head"] = head;
            }

            byte[] glyphs = new byte[end + paddedLength];
            Buffer.BlockCopy(glyf, 0, glyphs, 0, end);
            Buffer.BlockCopy(glyf, locations[0], glyphs, end, notdefLength);
            font.Tables["glyf"] = glyphs;

            byte[] loca = new byte[(glyphCount + 2) * (longOffsets ? 4 : 2)];
            for (int glyph = 0; glyph <= glyphCount + 1; ++glyph) {
                int location = glyph <= glyphCount ? locations[glyph] : end + paddedLength;
                if (longOffsets)
                    OpenTypeFont.WriteUInt32(loca, 4 * glyph, (uint)location);
                else
                    OpenTypeFont.WriteUInt16(loca, 2 * glyph, location / 2);
            }
            font.Tables["loca"] = loca;
        }

        // The metrics of hmtx or vmtx with those of the .notdef glyph appended for its copy, or null if the table is
        // shorter than its header says. The glyphs after the last long metric share its advance and list only their
        // side bearings; the copy is listed so when its advance is that advance, and otherwise every glyph gets a long
        // metric.
        // header: hhea or vhea, whose last field counts the long metrics.
        // metrics: hmtx or vmtx.
        // glyphCount: the count of glyphs before the copy.
        // longCount: receives the count of long metrics with the copy.
        private static byte[] AppendMetrics(byte[] header, byte[] metrics, int glyphCount, out int longCount)
        {
            longCount = header.Length < 36 ? 0 : OpenTypeFont.ReadUInt16(header, 34);
            if (longCount < 1 || longCount > glyphCount || metrics.Length < 4 * longCount + 2 * (glyphCount - longCount))
                return null;

            int lastAdvance = OpenTypeFont.ReadUInt16(metrics, 4 * (longCount - 1));
            int notdefAdvance = OpenTypeFont.ReadUInt16(metrics, 0);
            int notdefBearing = OpenTypeFont.ReadUInt16(metrics, 2);
            if (notdefAdvance == lastAdvance) {
                byte[] appended = new byte[4 * longCount + 2 * (glyphCount + 1 - longCount)];
                Buffer.BlockCopy(metrics, 0, appended, 0, appended.Length - 2);
                OpenTypeFont.WriteUInt16(appended, appended.Length - 2, notdefBearing);
                return appended;
            }

            byte[] expanded = new byte[4 * (glyphCount + 1)];
            for (int glyph = 0; glyph <= glyphCount; ++glyph) {
                int source = glyph == glyphCount ? 0 : glyph;
                int advance = source < longCount ? OpenTypeFont.ReadUInt16(metrics, 4 * source) : lastAdvance;
                int bearing = source < longCount ? OpenTypeFont.ReadUInt16(metrics, 4 * source + 2)
                                                 : OpenTypeFont.ReadUInt16(metrics, 4 * longCount + 2 * (source - longCount));
                OpenTypeFont.WriteUInt16(expanded, 4 * glyph, advance);
                OpenTypeFont.WriteUInt16(expanded, 4 * glyph + 2, bearing);
            }
            longCount = glyphCount + 1;
            return expanded;
        }

        // Replace hmtx or vmtx with the metrics that AppendMetrics made, and set the count of long metrics in hhea or
        // vhea.
        // font: the font.
        // headerTag, metricsTag: hhea and hmtx, or vhea and vmtx.
        // metrics: the metrics.
        // longCount: the count of long metrics among them.
        private static void ReplaceMetrics(OpenTypeFont font, string headerTag, string metricsTag, byte[] metrics, int longCount)
        {
            byte[] header = (byte[])font.Tables[headerTag].Clone();
            OpenTypeFont.WriteUInt16(header, 34, longCount);
            font.Tables[headerTag] = header;
            font.Tables[metricsTag] = metrics;
        }

        // Read the character map that the subsetter writes: a subtable of format 4 for the Basic Multilingual Plane, one
        // of format 12 for every plane when a character lies beyond it, and one of format 14 for variation sequences.
        // Return false for any other, such as the character map of a symbol font.
        // cmap: the table.
        // mappings: receives the glyph of each character mapped.
        // variations: receives the subtable of variation sequences, or null.
        private static bool ReadCharacterMap(byte[] cmap, out SortedDictionary<int, int> mappings, out byte[] variations)
        {
            mappings = new SortedDictionary<int, int>();
            variations = null;
            if (cmap.Length < 4)
                return false;
            int count = OpenTypeFont.ReadUInt16(cmap, 2);
            if (cmap.Length < 4 + 8 * count)
                return false;
            int basic = -1, full = -1;
            for (int index = 0; index < count; ++index) {
                int platform = OpenTypeFont.ReadUInt16(cmap, 4 + 8 * index), encoding = OpenTypeFont.ReadUInt16(cmap, 6 + 8 * index);
                long offset = OpenTypeFont.ReadUInt32(cmap, 8 + 8 * index);
                if (offset + 8 > cmap.Length)
                    return false;
                int format = OpenTypeFont.ReadUInt16(cmap, (int)offset);
                if (format == 4 && ((platform == 0 && encoding == 3) || (platform == 3 && encoding == 1)))
                    basic = (int)offset;
                else if (format == 12 && ((platform == 0 && encoding == 4) || (platform == 3 && encoding == 10)))
                    full = (int)offset;
                else if (format == 14 && platform == 0 && encoding == 5 && (variations = Slice(cmap, (int)offset, OpenTypeFont.ReadUInt32(cmap, (int)offset + 2))) != null)
                    continue;
                else
                    return false;
            }
            if (full >= 0)
                return ReadFormat12(cmap, full, mappings);
            if (basic >= 0)
                return ReadFormat4(cmap, basic, mappings);
            return count == 0;
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

        // Write a character map as the subsetter writes one: a subtable of format 4 for the Basic Multilingual Plane,
        // for Unicode and Windows alike; one of format 12 for every plane when a character lies beyond it; and the
        // subtable of variation sequences, if any, as it was. Return null if the subtable of format 4 would be too long.
        // mappings: the glyph of each character mapped.
        // variations: the subtable of variation sequences, or null.
        private static byte[] WriteCharacterMap(SortedDictionary<int, int> mappings, byte[] variations)
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

        // Set the first and last characters the font maps in OS/2, if it has the table: the last is at most U+FFFF.
        // font: the font.
        // mappings: the glyph of each character mapped.
        private static void WriteCharacterRange(OpenTypeFont font, SortedDictionary<int, int> mappings)
        {
            byte[] os2;
            if (!font.Tables.TryGetValue("OS/2", out os2) || os2.Length < 68 || mappings.Count == 0)
                return;
            int first = int.MaxValue, last = 0;
            foreach (int character in mappings.Keys) {
                first = Math.Min(first, character);
                last = Math.Max(last, character);
            }
            os2 = (byte[])os2.Clone();
            OpenTypeFont.WriteUInt16(os2, 64, Math.Min(first, 0xFFFF));
            OpenTypeFont.WriteUInt16(os2, 66, Math.Min(last, 0xFFFF));
            font.Tables["OS/2"] = os2;
        }

        // A part of data.
        // data: the data.
        // offset: where the part starts.
        // length: its length.
        private static byte[] Slice(byte[] data, int offset, long length)
        {
            if (length < 0 || offset + length > data.Length)
                return null;
            byte[] part = new byte[length];
            Buffer.BlockCopy(data, offset, part, 0, (int)length);
            return part;
        }
    }
}
