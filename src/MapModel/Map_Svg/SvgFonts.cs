using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using PurplePen.Graphics2D;

namespace PurplePen.MapModel
{
    // A font face that the document's text is drawn in: a family with its weight and style, the characters drawn in it,
    // and the faces that draw them, the font's own and those its characters fall back to, which the document embeds
    // (T-6, T-8).
    public sealed class SvgFontFace
    {
        // The family, as the map names it.
        public readonly string Family;

        // Whether the face is bold, and whether it is italic.
        public readonly bool Bold, Italic;

        // The characters drawn in the face, as Unicode code points.
        private readonly SortedSet<int> characters = new SortedSet<int>();

        // The faces that draw the characters, and the family each is embedded under: the font's own under the map's
        // family, and each fallback face under a family of its own.
        private readonly List<FontFaceSubset> subsets = new List<FontFaceSubset>();
        private readonly List<string> subsetFamilies = new List<string>();

        // The family of the fallback face that draws each character the font's own face does not.
        private readonly Dictionary<int, string> fallbackFamilies = new Dictionary<int, string>();

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

        // Set the faces that draw the characters, and the families they are embedded under.
        // faceSubsets: the faces, each with the characters it draws.
        // familyOfFallback: the family a fallback face is embedded under.
        public void SetSubsets(IList<FontFaceSubset> faceSubsets, Func<FontFaceSubset, string> familyOfFallback)
        {
            foreach (FontFaceSubset subset in faceSubsets) {
                string family = subset.IsMainFace ? Family : familyOfFallback(subset);
                subsets.Add(subset);
                subsetFamilies.Add(family);
                if (!subset.IsMainFace) {
                    foreach (int character in subset.Characters)
                        fallbackFamilies[character] = family;
                }
            }
        }

        // The family of the fallback face that draws a character, or null when the font's own face draws it.
        // character: the character, as a Unicode code point.
        public string FallbackFamilyOf(int character)
        {
            string family;
            return fallbackFamilies.TryGetValue(character, out family) ? family : null;
        }

        // Append an @font-face rule for each face that draws the characters and may be embedded: its family, the weight
        // and style that text asks for, and its font data (T-6, T-8, T-9).
        // text: the text to append to.
        public void AppendFontFaceRules(StringBuilder text)
        {
            for (int index = 0; index < subsets.Count; ++index) {
                byte[] data = subsets[index].FontData;
                if (data == null)
                    continue;
                text.Append("@font-face{font-family:").Append(QuoteFamily(subsetFamilies[index]));
                if (Bold)
                    text.Append(";font-weight:700");
                if (Italic)
                    text.Append(";font-style:italic");
                text.Append(";src:url(data:").Append(IsCompactFontFormat(data) ? "font/otf" : "font/ttf").Append(";base64,")
                    .Append(Convert.ToBase64String(data)).Append(")}\n");
            }
        }

        // Whether font data holds outlines in the Compact Font Format, which an OpenType file signs with "OTTO".
        // fontData: the font data.
        private static bool IsCompactFontFormat(byte[] fontData)
        {
            return fontData.Length >= 4 && fontData[0] == 'O' && fontData[1] == 'T' && fontData[2] == 'T' && fontData[3] == 'O';
        }
    }
}
