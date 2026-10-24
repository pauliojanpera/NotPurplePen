using System.Collections.Generic;

namespace Map_SkiaStd
{
    // Gives a symbol font a character map for Unicode before HarfBuzz's subsetter cuts it (T-7). A symbol font maps its
    // characters in a subtable for the Windows symbol encoding, platform 3 and encoding 0, usually under the codes
    // U+F020 to U+F0FF of the Private Use Area. HarfBuzz prefers that subtable to every other: it looks a character up
    // in it, and a character up to U+00FF that the subtable lacks under U+F000 more, as Windows does (the OpenType
    // recommendations, "Non-Standard (Symbol) Fonts"), so that text typed as Latin letters draws the symbols. Its
    // subsetter writes no subtable for the symbol encoding, and cut a symbol font to a subset with no character map at
    // all, which browsers reject and draw the text in fonts of the viewer's instead. Given a character map for Unicode
    // that maps each character to the glyph HarfBuzz maps it to through the symbol subtable, the subsetter writes a
    // subset that maps the text's characters as the application drew them.
    internal static class SymbolFont
    {
        // The pages of the old OS/2 tables, version 0, that HarfBuzz maps through tables of Arabic forms of its own:
        // the simplified and the traditional Arabic.
        private const int SimplifiedArabicPage = 0xB200, TraditionalArabicPage = 0xB300;

        // The font itself when its character map has no subtable for the symbol encoding; else a copy whose character
        // map maps for Unicode what HarfBuzz maps through the symbol subtable, keeping the subtable of variation
        // sequences, if any; or null, for a font HarfBuzz maps in a way this does not follow: a symbol subtable of a
        // format it cannot read, or an Arabic page named by an OS/2 table of version 0.
        // font: the font.
        public static OpenTypeFont WithUnicodeCharacterMap(OpenTypeFont font)
        {
            byte[] cmap;
            if (!font.Tables.TryGetValue("cmap", out cmap) || cmap.Length < 4)
                return font;
            int count = OpenTypeFont.ReadUInt16(cmap, 2);
            int symbol = -1;
            byte[] variations = null;
            for (int index = 0; index < count && 12 + 8 * index <= cmap.Length; ++index) {
                int platform = OpenTypeFont.ReadUInt16(cmap, 4 + 8 * index), encoding = OpenTypeFont.ReadUInt16(cmap, 6 + 8 * index);
                long offset = OpenTypeFont.ReadUInt32(cmap, 8 + 8 * index);
                if (platform == 3 && encoding == 0)
                    symbol = (int)offset;
                else if (platform == 0 && encoding == 5 && offset + 6 <= cmap.Length && OpenTypeFont.ReadUInt16(cmap, (int)offset) == 14)
                    variations = CharacterMaps.Slice(cmap, (int)offset, OpenTypeFont.ReadUInt32(cmap, (int)offset + 2));
            }
            if (symbol < 0)
                return font;

            int page = FontPage(font);
            if (page == SimplifiedArabicPage || page == TraditionalArabicPage)
                return null;
            SortedDictionary<int, int> symbols = new SortedDictionary<int, int>();
            if (symbol >= cmap.Length || !CharacterMaps.ReadSubtable(cmap, symbol, symbols))
                return null;
            SortedDictionary<int, int> mappings = new SortedDictionary<int, int>(symbols);
            if (page == 0) {
                for (int character = 0; character <= 0xFF; ++character) {
                    int glyph;
                    if (!symbols.ContainsKey(character) && symbols.TryGetValue(0xF000 + character, out glyph))
                        mappings[character] = glyph;
                }
            }
            byte[] unicode = CharacterMaps.Write(mappings, variations);
            if (unicode == null)
                return null;

            OpenTypeFont copy = new OpenTypeFont { SfntVersion = font.SfntVersion };
            foreach (KeyValuePair<string, byte[]> table in font.Tables)
                copy.Tables[table.Key] = table.Value;
            copy.Tables["cmap"] = unicode;
            return copy;
        }

        // The page of characters an OS/2 table of version 0 names in the high byte of its fsSelection, which HarfBuzz
        // reads to choose how to map a symbol font, or 0, for a later version and for a font without the table: then
        // HarfBuzz maps the characters up to U+00FF under U+F000 more. Under any other page it maps the symbol subtable
        // as it is.
        // font: the font.
        private static int FontPage(OpenTypeFont font)
        {
            byte[] os2;
            if (!font.Tables.TryGetValue("OS/2", out os2) || os2.Length < 64 || OpenTypeFont.ReadUInt16(os2, 0) != 0)
                return 0;
            return OpenTypeFont.ReadUInt16(os2, 62) & 0xFF00;
        }
    }
}
