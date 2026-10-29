using System.Collections.Generic;

namespace PurplePen.Graphics2D
{
    // Gives a target that embeds its fonts the font data to embed: for the characters drawn in a font, the faces that
    // the application draws them with, each cut to the characters it draws. The application implements it, since only
    // it knows which face draws each character: the font's own face, or a fallback face for characters the font lacks.
    public interface IFontSubsetProvider
    {
        // Get the faces that draw characters in a font, as the application draws them, each with its font data cut to
        // the characters it draws.
        // familyName: the font's family, as CreateFont was given it.
        // effects: the font's effects, as CreateFont was given them.
        // characters: the characters drawn in the font, as Unicode code points, each once.
        IList<FontFaceSubset> GetSubsets(string familyName, TextEffects effects, IList<int> characters);
    }

    // A face that draws some of the characters drawn in a font, with the font data a target embeds for them.
    public sealed class FontFaceSubset
    {
        // The characters, as Unicode code points, that the face draws.
        public readonly IList<int> Characters;

        // The face's OpenType font data, cut to those characters, or null when the face may not be embedded.
        public readonly byte[] FontData;

        // The family the face names itself with.
        public readonly string FaceName;

        // Whether the face is the font's own, rather than one that characters the font lacks fall back to.
        public readonly bool IsMainFace;

        // Create the subset of a face.
        // characters: the characters, as Unicode code points, that the face draws.
        // fontData: the OpenType font data cut to them, or null when the face may not be embedded.
        // faceName: the family the face names itself with.
        // isMainFace: whether the face is the font's own.
        public FontFaceSubset(IList<int> characters, byte[] fontData, string faceName, bool isMainFace)
        {
            Characters = characters;
            FontData = fontData;
            FaceName = faceName;
            IsMainFace = isMainFace;
        }
    }
}
