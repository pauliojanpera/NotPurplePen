using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using PurplePen.Graphics2D;

namespace PurplePen.MapModel
{
    // A face that the document's text is drawn in: a family with its weight and style, and the characters drawn in it,
    // for which the document embeds the face (T-6).
    public sealed class SvgFontFace
    {
        // The family, as the map names it.
        public readonly string Family;

        // Whether the face is bold, and whether it is italic.
        public readonly bool Bold, Italic;

        // The characters drawn in the face, as Unicode code points.
        private readonly SortedSet<int> characters = new SortedSet<int>();

        // Create a face.
        // family: the family, as the map names it.
        // bold, italic: whether the face is bold, and whether it is italic.
        public SvgFontFace(string family, bool bold, bool italic)
        {
            Family = family;
            Bold = bold;
            Italic = italic;
        }

        // The effects that ask for the face, as CreateFont takes them.
        public TextEffects Effects
        {
            get { return (Bold ? TextEffects.Bold : TextEffects.Regular) | (Italic ? TextEffects.Italic : TextEffects.Regular); }
        }

        // The characters drawn in the face, as Unicode code points in ascending order.
        public IList<int> Characters
        {
            get { return new List<int>(characters); }
        }

        // Record the characters of text drawn in the face.
        // text: the text, whose surrogate pairs each make one character.
        public void AddCharacters(string text)
        {
            for (int index = 0; index < text.Length; index += char.IsSurrogatePair(text, index) ? 2 : 1)
                characters.Add(char.ConvertToUtf32(text, index));
        }

        // The family as CSS writes it: quoted, so that no family name reads as a keyword.
        public string QuotedFamily
        {
            get { return QuoteFamily(Family); }
        }

        // Quote a family name for CSS, escaping what would end or break the string.
        // family: the family name.
        public static string QuoteFamily(string family)
        {
            StringBuilder quoted = new StringBuilder("\"");
            foreach (char character in family) {
                if (character == '"' || character == '\\')
                    quoted.Append('\\').Append(character);
                else if (character < ' ')
                    quoted.Append('\\').Append(((int)character).ToString("x", CultureInfo.InvariantCulture)).Append(' ');
                else
                    quoted.Append(character);
            }
            return quoted.Append('"').ToString();
        }

        // Append the face's @font-face rule for one subset: the family, weight and style that text asks for, the font
        // data, and the characters the subset draws, unless it draws every character drawn in the face (T-6, T-8).
        // text: the text to append to.
        // subset: the subset.
        // onlySubset: whether the subset is the only one of the face, and so draws every character drawn in it.
        public void AppendFontFaceRule(StringBuilder text, FontFaceSubset subset, bool onlySubset)
        {
            text.Append("@font-face{font-family:").Append(QuotedFamily);
            if (Bold)
                text.Append(";font-weight:700");
            if (Italic)
                text.Append(";font-style:italic");
            text.Append(";src:url(data:").Append(IsCompactFontFormat(subset.FontData) ? "font/otf" : "font/ttf").Append(";base64,")
                .Append(Convert.ToBase64String(subset.FontData)).Append(')');
            if (!onlySubset)
                text.Append(";unicode-range:").Append(FormatUnicodeRange(subset.Characters));
            text.Append("}\n");
        }

        // Whether font data holds outlines in the Compact Font Format, which an OpenType file signs with "OTTO".
        // fontData: the font data.
        private static bool IsCompactFontFormat(byte[] fontData)
        {
            return fontData.Length >= 4 && fontData[0] == 'O' && fontData[1] == 'T' && fontData[2] == 'T' && fontData[3] == 'O';
        }

        // Format characters as a CSS unicode-range: runs of consecutive code points as ranges, each in hexadecimal.
        // characters: the characters, as Unicode code points.
        public static string FormatUnicodeRange(IList<int> characters)
        {
            List<int> sorted = new List<int>(characters);
            sorted.Sort();
            StringBuilder range = new StringBuilder();
            int index = 0;
            while (index < sorted.Count) {
                int first = sorted[index], last = first;
                while (index + 1 < sorted.Count && sorted[index + 1] <= last + 1)
                    last = sorted[++index];
                ++index;
                if (range.Length > 0)
                    range.Append(',');
                range.Append("U+").Append(first.ToString("X", CultureInfo.InvariantCulture));
                if (last != first)
                    range.Append('-').Append(last.ToString("X", CultureInfo.InvariantCulture));
            }
            return range.ToString();
        }
    }
}
