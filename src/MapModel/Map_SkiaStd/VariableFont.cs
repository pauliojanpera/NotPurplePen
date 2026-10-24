using System;
using System.Collections.Generic;
using System.Text;

namespace Map_SkiaStd
{
    // Finds where in the design space of a variable font the application draws a face, so that the subset made of it
    // draws there too (T-7). A browser sets a variable font's axes from the text's style, its weight from the font's
    // weight among them, while the Skia target draws what the platform's font manager gives it. SkiaSharp can neither
    // set nor read where a typeface lies in the design space: a typeface loaded from a file, and on Linux any typeface,
    // draws the default instance whatever weight was asked for. A font manager that draws named instances, as
    // DirectWrite does, names each typeface after its instance, so the PostScript name of the typeface tells which:
    // the name the font's fvar table gives the instance, or, for an instance without one, the name the OpenType
    // specification makes for it (Adobe Technical Note 5902): the prefix of the PostScript names of the font's
    // variations, name 25, or else its family name, then a hyphen and the instance's subfamily name, each without the
    // characters a PostScript name cannot hold.
    public static class VariableFont
    {
        // The location of the named instance whose PostScript name a typeface carries, the coordinate of each axis by
        // its tag; or null for the default location: for a typeface of another name, and for a font that does not vary.
        // font: the font.
        // postScriptName: the PostScript name of the typeface the application draws the font with.
        public static IDictionary<string, float> LocationDrawn(OpenTypeFont font, string postScriptName)
        {
            byte[] fvar, name;
            if (string.IsNullOrEmpty(postScriptName) || !font.Tables.TryGetValue("fvar", out fvar) || fvar.Length < 16)
                return null;
            font.Tables.TryGetValue("name", out name);

            int axesOffset = OpenTypeFont.ReadUInt16(fvar, 4), axisCount = OpenTypeFont.ReadUInt16(fvar, 8), axisSize = OpenTypeFont.ReadUInt16(fvar, 10);
            int instanceCount = OpenTypeFont.ReadUInt16(fvar, 12), instanceSize = OpenTypeFont.ReadUInt16(fvar, 14);
            int instancesOffset = axesOffset + axisCount * axisSize;
            if (axisSize < 20 || instanceSize < 4 + 4 * axisCount || instancesOffset + instanceCount * instanceSize > fvar.Length)
                return null;
            bool namesInstances = instanceSize >= 6 + 4 * axisCount;

            string prefix = NameOf(name, 25) ?? NameOf(name, 16) ?? NameOf(name, 1);
            for (int instance = 0; instance < instanceCount; ++instance) {
                int record = instancesOffset + instance * instanceSize;
                string instanceName = null;
                if (namesInstances) {
                    int postScriptNameId = OpenTypeFont.ReadUInt16(fvar, record + 4 + 4 * axisCount);
                    if (postScriptNameId != 0xFFFF)
                        instanceName = NameOf(name, postScriptNameId);
                }
                if (instanceName == null) {
                    string subfamily = NameOf(name, OpenTypeFont.ReadUInt16(fvar, record));
                    if (prefix == null || subfamily == null)
                        continue;
                    instanceName = PostScriptCharacters(prefix) + "-" + PostScriptCharacters(subfamily);
                }
                if (instanceName != postScriptName)
                    continue;

                Dictionary<string, float> location = new Dictionary<string, float>(StringComparer.Ordinal);
                for (int axis = 0; axis < axisCount; ++axis) {
                    string tag = Encoding.ASCII.GetString(fvar, axesOffset + axis * axisSize, 4);
                    location[tag] = (int)OpenTypeFont.ReadUInt32(fvar, record + 4 + 4 * axis) / 65536F;
                }
                return location;
            }
            return null;
        }

        // A name of the font's name table: the one for Windows in English, or else for Windows in another language, or
        // else for the Macintosh in Roman; or null if it has none of them.
        // name: the name table, or null.
        // id: the name's ID.
        private static string NameOf(byte[] name, int id)
        {
            if (name == null || name.Length < 6)
                return null;
            int count = OpenTypeFont.ReadUInt16(name, 2), storage = OpenTypeFont.ReadUInt16(name, 4);
            string windows = null, english = null, macintosh = null;
            for (int index = 0; index < count && 6 + 12 * index + 12 <= name.Length; ++index) {
                int record = 6 + 12 * index;
                int platform = OpenTypeFont.ReadUInt16(name, record), encoding = OpenTypeFont.ReadUInt16(name, record + 2);
                int language = OpenTypeFont.ReadUInt16(name, record + 4), nameId = OpenTypeFont.ReadUInt16(name, record + 6);
                int length = OpenTypeFont.ReadUInt16(name, record + 8), offset = storage + OpenTypeFont.ReadUInt16(name, record + 10);
                if (nameId != id || offset + length > name.Length)
                    continue;
                if (platform == 3 && (encoding == 1 || encoding == 10)) {
                    string text = Encoding.BigEndianUnicode.GetString(name, offset, length);
                    if (language == 0x0409)
                        english = text;
                    else if (windows == null)
                        windows = text;
                }
                else if (platform == 1 && encoding == 0 && macintosh == null) {
                    macintosh = Encoding.Latin1.GetString(name, offset, length);
                }
            }
            return english ?? windows ?? macintosh;
        }

        // A name with only the characters a PostScript name of an instance may hold: the ASCII letters and digits.
        // text: the name.
        private static string PostScriptCharacters(string text)
        {
            StringBuilder kept = new StringBuilder(text.Length);
            foreach (char character in text) {
                if ((character >= 'A' && character <= 'Z') || (character >= 'a' && character <= 'z') || (character >= '0' && character <= '9'))
                    kept.Append(character);
            }
            return kept.ToString();
        }
    }
}
