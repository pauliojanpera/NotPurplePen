using System;
using System.Collections.Generic;
using System.Globalization;
using System.Drawing;
using System.Linq;
using System.Text;
using PurplePen.Graphics2D;

namespace Map_Svg.Tests
{
    // Text metrics for the document tests, whose numbers are simple to follow: every character advances half an em,
    // the pairs AV and VA kern by -0.1 and -0.08 em, and the ascent and descent are 0.8 and 0.2 em.
    public sealed class TestTextMetrics : ITextMetrics
    {
        // The metrics of a font of any family and effects.
        // familyName: the family.
        // emHeight: the size of the font.
        // effects: the effects.
        public ITextFaceMetrics GetTextFaceMetrics(string familyName, float emHeight, TextEffects effects)
        {
            return new TestFaceMetrics(emHeight);
        }

        // Every family is installed.
        // familyName: the family.
        public bool TextFaceIsInstalled(string familyName)
        {
            return true;
        }

        // Release nothing.
        public void Dispose()
        {
        }
    }

    // The metrics of a font of TestTextMetrics, at a size.
    public sealed class TestFaceMetrics : ITextFaceMetrics
    {
        // The kerning of the pairs that kern, in ems.
        private static readonly Dictionary<string, float> kerningPairs = new Dictionary<string, float> { { "AV", -0.1F }, { "VA", -0.08F } };

        // Create the metrics of a font.
        // emHeight: the size of the font.
        public TestFaceMetrics(float emHeight)
        {
            EmHeight = emHeight;
        }

        // The size, ascent, descent and other heights of the font.
        public float EmHeight { get; private set; }
        public float Ascent { get { return 0.8F * EmHeight; } }
        public float Descent { get { return 0.2F * EmHeight; } }
        public float CapHeight { get { return 0.7F * EmHeight; } }
        public float SpaceWidth { get { return 0.5F * EmHeight; } }
        public float RecommendedLineSpacing { get { return 1.2F * EmHeight; } }

        // The width of text: half an em for each character, a surrogate pair making one, and the kerning of its pairs. A
        // combining mark takes no room after another character, but at the start of the text, as shaping draws it alone,
        // the room of the placeholder that it puts under the mark.
        // text: the text.
        public float GetTextWidth(string text)
        {
            float ems = 0;
            for (int index = 0; index < text.Length; index += char.IsSurrogatePair(text, index) ? 2 : 1) {
                if (index > 0 && CharUnicodeInfo.GetUnicodeCategory(text, index) == UnicodeCategory.NonSpacingMark)
                    continue;
                ems += 0.5F;
                float kerning;
                if (index + 1 < text.Length && kerningPairs.TryGetValue(text.Substring(index, 2), out kerning))
                    ems += kerning;
            }
            return ems * EmHeight;
        }

        // The width of text and the height of the font.
        // text: the text.
        public SizeF GetTextSize(string text)
        {
            return new SizeF(GetTextWidth(text), Ascent + Descent);
        }

        // The box of text from its start, as tall as the font.
        // startPoint: the upper left corner of the text.
        // text: the text.
        public RectangleF GetTightBoundingBox(PointF startPoint, string text)
        {
            return new RectangleF(startPoint.X, startPoint.Y, GetTextWidth(text), Ascent + Descent);
        }

        // Release nothing.
        public void Dispose()
        {
        }
    }

    // Font subsets for the document tests: the family "Forbidden" may not be embedded, characters beyond U+00FF fall
    // back to a second face, and each subset's data is the text "<family> <effects> <characters>", so a test can tell
    // what was asked for.
    public sealed class TestFontSubsets : IFontSubsetProvider
    {
        // The requests made, as "<family> <effects> <characters>", in order.
        public readonly List<string> Requests = new List<string>();

        // The subsets of the characters drawn in a font.
        // familyName: the family.
        // effects: the effects.
        // characters: the characters, as code points.
        public IList<FontFaceSubset> GetSubsets(string familyName, TextEffects effects, IList<int> characters)
        {
            Requests.Add(Describe(familyName, effects, characters));
            List<FontFaceSubset> subsets = new List<FontFaceSubset>();
            List<int> own = characters.Where(character => character <= 0xFF).ToList();
            List<int> fallback = characters.Where(character => character > 0xFF).ToList();
            if (own.Count > 0)
                subsets.Add(new FontFaceSubset(own, familyName == "Forbidden" ? null : Encoding.UTF8.GetBytes(Describe(familyName, effects, own))));
            if (fallback.Count > 0)
                subsets.Add(new FontFaceSubset(fallback, Encoding.UTF8.GetBytes(Describe("Fallback", effects, fallback))));
            return subsets;
        }

        // Describe characters drawn in a font as "<family> <effects> <characters>".
        // familyName: the family.
        // effects: the effects.
        // characters: the characters, as code points.
        public static string Describe(string familyName, TextEffects effects, IList<int> characters)
        {
            return familyName + " " + effects + " " + string.Concat(characters.Select(char.ConvertFromUtf32));
        }

        // The data URI of a subset's data, as the document embeds it.
        // familyName: the family.
        // effects: the effects.
        // characters: the characters.
        public static string DataUri(string familyName, TextEffects effects, string characters)
        {
            List<int> codePoints = new List<int>();
            for (int index = 0; index < characters.Length; index += char.IsSurrogatePair(characters, index) ? 2 : 1)
                codePoints.Add(char.ConvertToUtf32(characters, index));
            return "data:font/ttf;base64," + Convert.ToBase64String(Encoding.UTF8.GetBytes(Describe(familyName, effects, codePoints)));
        }
    }
}
