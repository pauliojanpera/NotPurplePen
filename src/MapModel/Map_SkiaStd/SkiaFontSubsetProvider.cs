using System;
using System.Collections.Generic;
using PurplePen.Graphics2D;
using PurplePen.MapModel;
using SkiaSharp;

namespace Map_SkiaStd
{
    // Gives a target that embeds its fonts, such as the SVG target, the font data of the faces the Skia target draws
    // text with (T-7 to T-9). Each character drawn in a font goes to the face that EnhancedTypeface draws it with: the
    // font's own face, or the face the platform's font fallback offers for a character the font lacks. Each face's
    // data is cut to its characters, by HarfBuzz's subsetter unless the provider is given another, and unless the
    // face's permissions forbid subsetting; a face whose permissions forbid embedding gives no data, and is referred to
    // by name only.
    public sealed class SkiaFontSubsetProvider : IFontSubsetProvider
    {
        // Cuts a face's data to characters: the face, and its characters as Unicode code points.
        private readonly Func<OpenTypeFont, IList<int>, byte[]> cut;

        // Create a provider that cuts each face with HarfBuzz's subsetter (HarfBuzzSubsetter).
        public SkiaFontSubsetProvider()
            : this(HarfBuzzSubsetter.Cut)
        {
        }

        // Create a provider that cuts each face with a subsetter.
        // cut: cuts a face to characters, given as Unicode code points, and returns the font data.
        public SkiaFontSubsetProvider(Func<OpenTypeFont, IList<int>, byte[]> cut)
        {
            this.cut = cut;
        }

        // The faces that draw characters in a font, as the Skia target draws them, each with its data cut to the
        // characters it draws; the font's own face first, then the fallback faces in the order of their first
        // character.
        // familyName: the font's family.
        // effects: the font's effects.
        // characters: the characters drawn in the font, as Unicode code points.
        public IList<FontFaceSubset> GetSubsets(string familyName, TextEffects effects, IList<int> characters)
        {
            SKFontStyleWeight weight = SkiaFont.GetSKFontStyleWeight(effects);
            SKFontStyleSlant slant = SkiaFont.GetSKFontStyleSlant(effects);
            List<ShapedTypeface> faces = new List<ShapedTypeface>();
            Dictionary<ShapedTypeface, List<int>> charactersByFace = new Dictionary<ShapedTypeface, List<int>>(new IdentityComparer<ShapedTypeface>());

            using (ShapedTypeface mainFace = ShapedTypeface.Get(familyName, weight, SKFontStyleWidth.Normal, slant)) {
                EnhancedTypeface typeface = new EnhancedTypeface(mainFace, familyName, weight, SKFontStyleWidth.Normal, slant, null);
                faces.Add(mainFace);
                charactersByFace.Add(mainFace, new List<int>());
                foreach (int character in characters) {
                    ShapedTypeface face = typeface.TypefaceForCodepoint(character);
                    List<int> ofFace;
                    if (!charactersByFace.TryGetValue(face, out ofFace)) {
                        ofFace = new List<int>();
                        charactersByFace.Add(face, ofFace);
                        faces.Add(face);
                    }
                    ofFace.Add(character);
                }

                List<FontFaceSubset> subsets = new List<FontFaceSubset>();
                foreach (ShapedTypeface face in faces) {
                    List<int> ofFace = charactersByFace[face];
                    if (ofFace.Count == 0)
                        continue;
                    OpenTypeFont font = OpenTypeFont.Read(face.GetFontData(), face.FontDataCollectionIndex);
                    font.Location = VariableFont.LocationDrawn(font, face.Typeface.PostScriptName);
                    byte[] data = font.ForbidsEmbedding ? null : font.ForbidsSubsetting ? font.Write() : cut(font, ofFace);
                    subsets.Add(new FontFaceSubset(ofFace, data, face.Typeface.FamilyName, face == mainFace));
                }
                return subsets;
            }
        }
    }
}
