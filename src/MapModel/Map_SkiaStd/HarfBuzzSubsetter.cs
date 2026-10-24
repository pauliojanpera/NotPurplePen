using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using HarfBuzzSharp;

namespace Map_SkiaStd
{
    // Cuts a face's font data down to the characters a document draws in it, with the subsetter of HarfBuzz (T-7). The
    // native library that HarfBuzzSharp ships for shaping carries it on every platform it is built for, the
    // WebAssembly one among them, and harfbuzzjs carries it for a browser: the subsetter and the shaping are the same
    // HarfBuzz the application draws text with.
    //
    // A browser that draws the subset must draw each character as the application draws it from the whole face, so
    // the subset keeps:
    // - the glyphs of the characters, and every glyph that the face's substitutions reach from them under the features
    //   the application shapes with, with the substitutions and positionings among those glyphs, so that marks, Indic
    //   conjuncts and Arabic forms come out as they do in the application. The features the application turns off,
    //   ligatures and contextual alternates, and kerning, which the document writes into its positions, are left out;
    //   a document turns all of them off in the browser too;
    // - the characters that HarfBuzz looks up while it draws a character from others: those of its canonical
    //   decomposition, which HarfBuzz draws a character from when the face lacks it, and which the shapers of the
    //   complex scripts decompose a character into even when the face has it; U+0020, whose glyph HarfBuzz draws a
    //   space the face lacks with; U+2010 for U+2011; and the jamo of a Hangul syllable. A browser's HarfBuzz, finding
    //   in the subset the same characters the application's found in the whole face, makes the same choices;
    // - the face's box, its .notdef glyph, under each character the face lacks and draws as that box, which a browser
    //   would draw from a font of the viewer's system instead (MissingCharacterBox).
    //
    // A symbol font, whose character map the subsetter would drop, is first given a character map for Unicode that maps
    // each character as HarfBuzz maps it through the symbol encoding (SymbolFont).
    //
    // It drops the hinting, which the application does not draw with, and the gasp table, which says how to smooth
    // hinted glyphs; and it keeps, beside the names the subsetter keeps by default, the font's license and the address
    // of the license, which licenses such as the SIL Open Font License ask a copy of the font to carry.
    public static class HarfBuzzSubsetter
    {
        // The native library that HarfBuzzSharp loads, which carries HarfBuzz's subsetter as well as its shaping.
        private const string HarfBuzz = "libHarfBuzzSharp";

        // hb_subset_flags_t: drop the hinting instructions and the tables of hinting; keep the outline of .notdef.
        private const uint NoHinting = 0x00000001;
        private const uint NotdefOutline = 0x00000040;

        // hb_subset_sets_t: the sets of a subset input.
        private const int UnicodeSet = 1;
        private const int DropTableTagSet = 3;
        private const int NameIdSet = 4;
        private const int LayoutFeatureTagSet = 6;

        // The features the application shapes without (SkiaFont), and kerning, which the document writes into the
        // positions of its text and turns off in the browser.
        private static readonly string[] FeaturesLeftOut = { "liga", "clig", "dlig", "hlig", "calt", "kern" };

        // The names kept beside the subsetter's default ones, 0 to 6: the license and the address of the license.
        private static readonly uint[] LicenseNameIds = { 13, 14 };

        // The scripts whose shapers draw a character the face has from its own glyph, without looking its
        // decomposition up: HarfBuzz's default shaper and those of Arabic, Hebrew, Thai and Hangul. The shapers of the
        // other scripts, such as the Indic ones, may decompose a character the face has.
        private static readonly Script[] ScriptsNotDecomposed = {
            Script.Common, Script.Inherited, Script.Latin, Script.Greek, Script.Cyrillic, Script.Armenian, Script.Georgian,
            Script.Han, Script.Hiragana, Script.Katakana, Script.Bopomofo, Script.Hangul, Script.Arabic, Script.Hebrew,
            Script.Thai, Script.Lao,
        };

        // Cut a face to the characters a document draws in it.
        // font: the face.
        // characters: the characters, as Unicode code points.
        public static byte[] Cut(OpenTypeFont font, IList<int> characters)
        {
            OpenTypeFont unicodeFont = SymbolFont.WithUnicodeCharacterMap(font);
            if (unicodeFont == null)
                return font.Write();
            byte[] data = unicodeFont.Write();
            using (Blob blob = Blob.FromStream(new MemoryStream(data)))
            using (Face face = new Face(blob, 0))
            using (HarfBuzzSharp.Font shaping = new HarfBuzzSharp.Font(face)) {
                SortedSet<int> kept = new SortedSet<int>();
                List<int> boxed = new List<int>();
                foreach (int character in new SortedSet<int>(characters)) {
                    if (Has(shaping, character)) {
                        kept.Add(character);
                        if (!IsInScriptNotDecomposed(character))
                            KeepDecomposition(shaping, character, kept);
                    }
                    else {
                        KeepWhatReplaces(shaping, character, kept);
                        if (IsDrawnAsBox(shaping, character))
                            boxed.Add(character);
                    }
                }

                byte[] subset = Subset(face, kept, boxed.Count > 0);
                if (subset == null)
                    return data;
                return boxed.Count > 0 ? MissingCharacterBox.Add(subset, boxed) : subset;
            }
        }

        // Whether a face maps a character to a glyph of its own.
        // face: the face, as HarfBuzz shapes with it.
        // character: the character.
        private static bool Has(HarfBuzzSharp.Font face, int character)
        {
            uint glyph;
            return face.TryGetNominalGlyph(character, out glyph) && glyph != 0;
        }

        // Whether a character belongs to a script whose shaper never looks up the decomposition of a character the
        // face has.
        // character: the character.
        private static bool IsInScriptNotDecomposed(int character)
        {
            Script script = UnicodeFunctions.Default.GetScript(character);
            foreach (Script notDecomposed in ScriptsNotDecomposed) {
                if (script.Equals(notDecomposed))
                    return true;
            }
            return false;
        }

        // Keep each character of a character's canonical decomposition, at every level, that the face has: HarfBuzz
        // decomposes a character into a first part and a second, and the first part in turn.
        // face: the face.
        // character: the character.
        // kept: the characters kept, which receives the parts.
        private static void KeepDecomposition(HarfBuzzSharp.Font face, int character, SortedSet<int> kept)
        {
            int first, second;
            if (!UnicodeFunctions.Default.TryDecompose(character, out first, out second))
                return;
            if (second != 0 && Has(face, second))
                kept.Add(second);
            if (Has(face, first))
                kept.Add(first);
            KeepDecomposition(face, first, kept);
        }

        // Keep the characters whose glyphs HarfBuzz may draw in place of a character the face lacks: those of its
        // decomposition; U+0020, whose glyph it draws a space with; U+2010, which it draws U+2011 with; and the jamo
        // that a Hangul syllable is made of.
        // face: the face.
        // character: the character the face lacks.
        // kept: the characters kept, which receives those characters.
        private static void KeepWhatReplaces(HarfBuzzSharp.Font face, int character, SortedSet<int> kept)
        {
            KeepDecomposition(face, character, kept);
            if (UnicodeFunctions.Default.GetGeneralCategory(character) == UnicodeGeneralCategory.SpaceSeparator && Has(face, 0x0020))
                kept.Add(0x0020);
            if (character == 0x2011 && Has(face, 0x2010))
                kept.Add(0x2010);
            if (character >= 0xAC00 && character <= 0xD7A3) {
                int index = character - 0xAC00;
                foreach (int jamo in new int[] { 0x1100 + index / 588, 0x1161 + index % 588 / 28, index % 28 == 0 ? 0 : 0x11A7 + index % 28 }) {
                    if (jamo != 0 && Has(face, jamo))
                        kept.Add(jamo);
                }
            }
        }

        // Whether HarfBuzz draws a character that the face lacks as the face's box: whether it shapes to .notdef,
        // rather than to glyphs it draws the character from, or to nothing, as a default ignorable character.
        // face: the face.
        // character: the character.
        private static bool IsDrawnAsBox(HarfBuzzSharp.Font face, int character)
        {
            using (HarfBuzzSharp.Buffer buffer = new HarfBuzzSharp.Buffer()) {
                buffer.AddUtf16(char.ConvertFromUtf32(character));
                buffer.GuessSegmentProperties();
                face.Shape(buffer, Array.Empty<Feature>());
                foreach (GlyphInfo glyph in buffer.GlyphInfos) {
                    if (glyph.Codepoint == 0)
                        return true;
                }
                return false;
            }
        }

        // Run HarfBuzz's subsetter on a face and return the subset, or null if it could not cut the face.
        // face: the face.
        // characters: the characters to keep.
        // keepNotdefOutline: whether to keep the outline of .notdef, which the box is copied from.
        private static byte[] Subset(Face face, SortedSet<int> characters, bool keepNotdefOutline)
        {
            IntPtr input = hb_subset_input_create_or_fail();
            if (input == IntPtr.Zero)
                throw new OutOfMemoryException("HarfBuzz could not create the input of its subsetter.");
            try {
                IntPtr unicodes = hb_subset_input_set(input, UnicodeSet);
                foreach (int character in characters)
                    hb_set_add(unicodes, (uint)character);
                IntPtr features = hb_subset_input_set(input, LayoutFeatureTagSet);
                foreach (string feature in FeaturesLeftOut)
                    hb_set_del(features, TagOf(feature));
                IntPtr names = hb_subset_input_set(input, NameIdSet);
                foreach (uint name in LicenseNameIds)
                    hb_set_add(names, name);
                hb_set_add(hb_subset_input_set(input, DropTableTagSet), TagOf("gasp"));
                hb_subset_input_set_flags(input, NoHinting | (keepNotdefOutline ? NotdefOutline : 0));

                IntPtr subset = hb_subset_or_fail(face.Handle, input);
                if (subset == IntPtr.Zero)
                    return null;
                try {
                    IntPtr blob = hb_face_reference_blob(subset);
                    try {
                        uint length;
                        IntPtr bytes = hb_blob_get_data(blob, out length);
                        byte[] result = new byte[length];
                        if (length > 0)
                            Marshal.Copy(bytes, result, 0, (int)length);
                        return result;
                    }
                    finally {
                        hb_blob_destroy(blob);
                    }
                }
                finally {
                    hb_face_destroy(subset);
                }
            }
            finally {
                hb_subset_input_destroy(input);
            }
        }

        // The OpenType tag of four characters, as HarfBuzz holds it.
        // tag: the tag.
        private static uint TagOf(string tag)
        {
            return ((uint)tag[0] << 24) | ((uint)tag[1] << 16) | ((uint)tag[2] << 8) | tag[3];
        }

        [DllImport(HarfBuzz, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr hb_subset_input_create_or_fail();

        [DllImport(HarfBuzz, CallingConvention = CallingConvention.Cdecl)]
        private static extern void hb_subset_input_destroy(IntPtr input);

        [DllImport(HarfBuzz, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr hb_subset_input_set(IntPtr input, int setType);

        [DllImport(HarfBuzz, CallingConvention = CallingConvention.Cdecl)]
        private static extern void hb_subset_input_set_flags(IntPtr input, uint flags);

        [DllImport(HarfBuzz, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr hb_subset_or_fail(IntPtr face, IntPtr input);

        [DllImport(HarfBuzz, CallingConvention = CallingConvention.Cdecl)]
        private static extern void hb_set_add(IntPtr set, uint codepoint);

        [DllImport(HarfBuzz, CallingConvention = CallingConvention.Cdecl)]
        private static extern void hb_set_del(IntPtr set, uint codepoint);

        [DllImport(HarfBuzz, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr hb_face_reference_blob(IntPtr face);

        [DllImport(HarfBuzz, CallingConvention = CallingConvention.Cdecl)]
        private static extern void hb_face_destroy(IntPtr face);

        [DllImport(HarfBuzz, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr hb_blob_get_data(IntPtr blob, out uint length);

        [DllImport(HarfBuzz, CallingConvention = CallingConvention.Cdecl)]
        private static extern void hb_blob_destroy(IntPtr blob);
    }
}
