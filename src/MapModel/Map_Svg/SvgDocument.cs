using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using PurplePen.Graphics2D;

namespace PurplePen.MapModel
{
    // A part of a drawing, which the document defines once for every page that shows it (L-2, L-4).
    public sealed class SvgPart
    {
        // What the part holds; the group is written as the part's definition, with no attribute but its id.
        public readonly SvgGroup Content = new SvgGroup();
    }

    // A clip: a path in steps, filled under its rule, that a group or a page clips to (D-2).
    public sealed class SvgClip
    {
        // The data of the path.
        public readonly string PathData;

        // Whether the path is filled under the even-odd rule rather than the nonzero one.
        public readonly bool EvenOdd;

        // Create a clip.
        // pathData: the data of the path.
        // evenOdd: whether the path is filled under the even-odd rule.
        public SvgClip(string pathData, bool evenOdd)
        {
            PathData = pathData;
            EvenOdd = evenOdd;
        }
    }

    // A pattern: a tile repeated across the user space of what it paints, its middle on the origin and turned by its
    // angle, as the Skia target repeats the tile its brush target drew (D-7). The document defines it once, however
    // many styles paint with it.
    public sealed class SvgPattern
    {
        // What the tile holds, in a frame whose origin is the middle of the tile.
        public readonly SvgGroup Content = new SvgGroup();

        // The size of the tile, in steps.
        public readonly float Width, Height;

        // The angle the pattern is turned by, in degrees from the x axis towards the y axis.
        public readonly float Angle;

        // Create a pattern, its tile still to be drawn.
        // width, height: the size of the tile, in steps.
        // angle: the angle it is turned by, in degrees.
        public SvgPattern(float width, float height, float angle)
        {
            Width = width;
            Height = height;
            Angle = angle;
        }
    }

    // An image: a bitmap's written data, which the document holds once however often it is drawn, and the size in
    // pixels that the elements drawing it scale to their rectangles (B-3 to B-5).
    public sealed class SvgImage
    {
        // The size of the image, in pixels.
        public readonly int Width, Height;

        // The reference to the image's data: a data URI when embedded, or the reference to a linked file.
        public readonly string Href;

        // Create an image.
        // width, height: its size in pixels.
        // href: the reference to its data.
        public SvgImage(int width, int height, string href)
        {
            Width = width;
            Height = height;
            Href = href;
        }
    }

    // A color of a map's color table, as the metadata records it once a run of it begins (M-3, M-7).
    public sealed class SvgColorRecord
    {
        // The color's position in the map's color table, counting from 0 at the bottom, by which its runs refer to it.
        public readonly int Position;

        // The color's name.
        public readonly string Name;

        // The color's CMYK values, as the map stores them.
        public readonly CmykColor Color;

        // Whether the color overprints the colors below it.
        public readonly bool Overprint;

        // Create the record of a color.
        // position: its position in the color table.
        // name: its name.
        // color: its CMYK values.
        // overprint: whether it overprints.
        public SvgColorRecord(int position, string name, CmykColor color, bool overprint)
        {
            Position = position;
            Name = name;
            Color = color;
            Overprint = overprint;
        }
    }

    // A symbol of a map, as the metadata records it once an object of it begins: its number, by which the groups of
    // its objects refer to it, its id and name, and the colors its objects are drawn in (M-6, M-7).
    public sealed class SvgSymbolRecord
    {
        // The number by which the symbol's groups refer to it, counting from 1 in the order the map's symbols first
        // begin an object.
        public readonly int Number;

        // The symbol's id, such as "101.0".
        public readonly string Id;

        // The symbol's name.
        public readonly string Name;

        // The positions of the colors its objects are drawn in, from the bottom up.
        public readonly List<int> ColorPositions = new List<int>();

        // Create the record of a symbol.
        // number: its number within its map.
        // id: its id.
        // name: its name.
        public SvgSymbolRecord(int number, string id, string name)
        {
            Number = number;
            Id = id;
            Name = name;
        }

        // Record that an object of the symbol is drawn in a color.
        // position: the color's position in the color table.
        public void AddColor(int position)
        {
            int index = ColorPositions.BinarySearch(position);
            if (index < 0)
                ColorPositions.Insert(~index, position);
        }
    }

    // A labeled map, as the metadata records it: its number and label, and the tables of the colors and symbols
    // drawn in it, which its runs, symbol groups and objects refer to (M-5, M-7).
    public sealed class SvgMapRecord
    {
        // The number by which the map's groups refer to it, counting from 1 in the order maps first begin.
        public readonly int Number;

        // What the map is called.
        public readonly string Label;

        // The colors of the map's runs, from the bottom of the color table up, and the same by their keys.
        public readonly List<SvgColorRecord> Colors = new List<SvgColorRecord>();
        private readonly Dictionary<object, SvgColorRecord> colorsByKey = new Dictionary<object, SvgColorRecord>(new IdentityComparer<object>());

        // The symbols of the map's objects, in the order of their numbers, and the same by their keys.
        public readonly List<SvgSymbolRecord> Symbols = new List<SvgSymbolRecord>();
        private readonly Dictionary<object, SvgSymbolRecord> symbolsByKey = new Dictionary<object, SvgSymbolRecord>(new IdentityComparer<object>());

        // Where the map lies in the real world, or null when that is not known (G-1, G-2).
        public MapGeoreference Georeference;

        // The course the map draws, as an export describes it, or null (CD-1).
        public CourseRecord Course;

        // What each object that a course added to the map draws, by the object's key, or null (CD-2).
        public IReadOnlyDictionary<object, CourseObjectRecord> ObjectDescriptions;

        // The keys of the objects begun within the map, in the order each first began.
        public readonly List<object> ObjectKeys = new List<object>();
        private readonly HashSet<object> objectKeysSeen = new HashSet<object>(new IdentityComparer<object>());

        // Whether the metadata holds anything within the map's record: its georeference, a table, its course or the
        // descriptions of its objects.
        public bool HasContent
        {
            get { return Georeference != null || Colors.Count > 0 || Symbols.Count > 0 || Course != null || ObjectDescriptions != null; }
        }

        // Record that an object has begun within the map.
        // objectKey: the key that identifies the object.
        public void RecordObject(object objectKey)
        {
            if (objectKeysSeen.Add(objectKey))
                ObjectKeys.Add(objectKey);
        }

        // Create the record of a map.
        // number: its number.
        // label: what it is called.
        public SvgMapRecord(int number, string label)
        {
            Number = number;
            Label = label;
        }

        // The record of a color of the map, made when a run of it first begins.
        // colorKey: the key that identifies the color within the map.
        // position: its position in the color table.
        // name: its name.
        // color: its CMYK values.
        // overprint: whether it overprints.
        public SvgColorRecord RecordColor(object colorKey, int position, string name, CmykColor color, bool overprint)
        {
            SvgColorRecord record;
            if (!colorsByKey.TryGetValue(colorKey, out record)) {
                record = new SvgColorRecord(position, name, color.Clone(), overprint);
                colorsByKey.Add(colorKey, record);
                int index = 0;
                while (index < Colors.Count && Colors[index].Position < position)
                    ++index;
                Colors.Insert(index, record);
            }
            return record;
        }

        // The record of a symbol of the map, numbered when an object of it first begins.
        // symbolKey: the key that identifies the symbol within the map.
        // id: its id.
        // name: its name.
        public SvgSymbolRecord RecordSymbol(object symbolKey, string id, string name)
        {
            SvgSymbolRecord record;
            if (!symbolsByKey.TryGetValue(symbolKey, out record)) {
                record = new SvgSymbolRecord(Symbols.Count + 1, id, name);
                symbolsByKey.Add(symbolKey, record);
                Symbols.Add(record);
            }
            return record;
        }
    }

    // A page: parts shown through a clip and a transform onto paper (L-3, F-4).
    public sealed class SvgPage
    {
        // The size of the paper in millimetres.
        public readonly double Width, Height;

        // Where the page lies on the document's canvas: the left edge, in millimetres.
        public readonly double Left;

        // The transform from the steps of the parts to the page's millimetres.
        public readonly SvgTransform StepsToPage;

        // The clip, in steps: the area of the map that the page shows.
        public readonly SvgClip Clip;

        // The bounds of the clip, in steps.
        public readonly SvgBounds ClipBounds;

        // The parts the page shows, from the bottom up.
        public readonly List<SvgPart> Parts;

        // Create a page.
        public SvgPage(double width, double height, double left, SvgTransform stepsToPage, SvgClip clip, SvgBounds clipBounds, List<SvgPart> parts)
        {
            Width = width;
            Height = height;
            Left = left;
            StepsToPage = stepsToPage;
            Clip = clip;
            ClipBounds = clipBounds;
            Parts = parts;
        }
    }

    // The document being written: the parts it defines, the maps within them, and the pages that show them. Its text is
    // written at the end, when everything in it is known.
    public sealed class SvgDocument
    {
        // The namespace of the metadata, and the version of its format, which grows without breaking its readers (F-3).
        public const string MetadataNamespace = "https://pauliojanpera.github.io/NotPurplePen/svg";
        public const string FormatVersion = "1";

        // The gap between pages laid side by side, in millimetres.
        private const double GapBetweenPages = 10;

        // How far from its origin a map's coordinates reach, in steps: the 8 m within which the map model's 32-bit
        // floats still resolve a step (S-1), and over which a map's georeference keeps its digits (S-2).
        private const double MapReachInSteps = 8000 * SvgNumbers.StepsPerMillimetre;

        // The paints of the document's styles.
        public readonly SvgPaints Paints;

        // The parts, in the order they began.
        public readonly List<SvgPart> Parts = new List<SvgPart>();

        // The maps, in the order they first began.
        public readonly List<SvgMapRecord> Maps = new List<SvgMapRecord>();

        // The pages, in the order they were added.
        private readonly List<SvgPage> pages = new List<SvgPage>();

        // The clips, by their rule and path data, so that equal clips share one definition.
        private readonly Dictionary<string, SvgClip> clips = new Dictionary<string, SvgClip>(StringComparer.Ordinal);

        // The numbers of the objects drawn, by their keys.
        private readonly Dictionary<object, int> objectNumbers = new Dictionary<object, int>(new IdentityComparer<object>());

        // The images, by their written data, so that a bitmap drawn again, or its same part, is held once (B-5).
        private readonly Dictionary<string, SvgImage> images = new Dictionary<string, SvgImage>(StringComparer.Ordinal);

        // The faces that text is drawn in, in the order they are first drawn in, and by their family, weight and style.
        private readonly List<SvgFontFace> fontFaces = new List<SvgFontFace>();
        private readonly Dictionary<string, SvgFontFace> fontFacesByName = new Dictionary<string, SvgFontFace>(StringComparer.Ordinal);

        // What gives the font data the document embeds for its faces, or null when it embeds none (T-6, T-7).
        private readonly IFontSubsetProvider fontSubsets;

        // Whether the faces that draw the fonts' characters have been asked for and named.
        private bool fontFacesResolved;

        // Where the next page goes: the left edge, in millimetres.
        private double nextPageLeft;

        // Create a document that embeds no fonts.
        // paints: the paints of its styles.
        public SvgDocument(SvgPaints paints)
            : this(paints, null)
        {
        }

        // Create a document.
        // paints: the paints of its styles.
        // fontSubsets: what gives the font data it embeds for the faces its text is drawn in, or null to embed none.
        public SvgDocument(SvgPaints paints, IFontSubsetProvider fontSubsets)
        {
            Paints = paints;
            this.fontSubsets = fontSubsets;
        }

        // The face of a family, weight and style that text is drawn in, made when text is first drawn in it.
        // family: the family, as the map names it.
        // effects: the effects that ask for the weight and style.
        public SvgFontFace FontFaceOf(string family, TextEffects effects)
        {
            bool bold = (effects & TextEffects.Bold) != 0, italic = (effects & TextEffects.Italic) != 0;
            string name = (bold ? "b" : "-") + (italic ? "i" : "-") + family;
            SvgFontFace face;
            if (!fontFacesByName.TryGetValue(name, out face)) {
                face = new SvgFontFace(family, bold, italic);
                fontFacesByName.Add(name, face);
                fontFaces.Add(face);
            }
            return face;
        }

        // The clip of a path filled under a rule. Equal clips are one, whose definition every group that clips to it
        // shares: a clip path is read in the user space of what refers to it, so the same data serves every frame.
        // pathData: the data of the path, in steps.
        // evenOdd: whether the path is filled under the even-odd rule.
        public SvgClip ClipOf(string pathData, bool evenOdd)
        {
            string key = (evenOdd ? "e" : "n") + pathData;
            SvgClip clip;
            if (!clips.TryGetValue(key, out clip)) {
                clip = new SvgClip(pathData, evenOdd);
                clips.Add(key, clip);
            }
            return clip;
        }

        // The number of an object that has drawn an element, if it has.
        // objectKey: the key that identifies the object.
        // number: receives the object's number.
        public bool TryGetNumber(object objectKey, out int number)
        {
            return objectNumbers.TryGetValue(objectKey, out number);
        }

        // The number of an object, which its elements carry: objects are numbered across the document from 1, in the
        // order they are first drawn, so an object drawn in several runs has the same number in each (M-6, L-5).
        // objectKey: the key that identifies the object.
        public int NumberOf(object objectKey)
        {
            int number;
            if (!objectNumbers.TryGetValue(objectKey, out number)) {
                number = objectNumbers.Count + 1;
                objectNumbers.Add(objectKey, number);
            }
            return number;
        }

        // The image of a bitmap's written data, made when the data is first seen: embedded as a data URI, or stored
        // through the options as a linked file, once whatever the number of bitmaps that write the same data (B-4, B-5).
        // data: the written data.
        // format: its format, PNG or JPEG.
        // width, height: the bitmap's size in pixels.
        // options: how bitmaps reach the document.
        public SvgImage ImageOf(byte[] data, GraphicsBitmapFormat format, int width, int height, SvgBitmapOptions options)
        {
            string base64 = Convert.ToBase64String(data);
            SvgImage image;
            if (!images.TryGetValue(base64, out image)) {
                string href = options.Handling == SvgBitmapHandling.Linked
                    ? options.StoreLinkedBitmap(data, format)
                    : "data:" + (format == GraphicsBitmapFormat.JPEG ? "image/jpeg" : "image/png") + ";base64," + base64;
                image = new SvgImage(width, height, href);
                images.Add(base64, image);
            }
            return image;
        }

        // Add a page, to the right of the pages before it; the first lies at the document's own view box.
        // width, height: the size of the paper in millimetres.
        // stepsToPage: the transform from the steps of the parts to the page's millimetres.
        // clip: the clip, in steps.
        // clipBounds: the bounds of the clip.
        // parts: the parts the page shows, from the bottom up.
        public void AddPage(double width, double height, SvgTransform stepsToPage, SvgClip clip, SvgBounds clipBounds, List<SvgPart> parts)
        {
            pages.Add(new SvgPage(width, height, nextPageLeft, stepsToPage, clip, clipBounds, parts));
            nextPageLeft += width + GapBetweenPages;
        }

        // Write the document's text: the style block, the metadata, the definitions of clips, patterns, parts and images,
        // and then the pages. Classes and ids are named in the order they are first used (F-2); the images, whose data
        // dwarfs the rest, come last among the definitions.
        public string Write()
        {
            foreach (SvgPart part in Parts)
                part.Content.TidyChildren();
            ResolveFontFaces();

            SvgWriter writer = new SvgWriter();
            foreach (SvgPart part in Parts) {
                writer.Text.Append("<g id=\"").Append(writer.IdOf(part)).Append("\">\n");
                part.Content.WriteChildren(writer);
                writer.Text.Append("</g>\n");
            }
            string partsText = writer.TakeText();

            foreach (SvgPage page in pages)
                WritePage(writer, page);
            string pagesText = writer.TakeText();

            // A tile may paint with further patterns, which join the list as they are first referred to.
            for (int index = 0; index < writer.PatternsReferredTo.Count; ++index)
                WritePattern(writer, writer.PatternsReferredTo[index]);
            string patternsText = writer.TakeText();

            foreach (SvgImage image in writer.ImagesReferredTo) {
                writer.Text.Append("<image id=\"").Append(writer.IdOfImage(image)).Append("\" width=\"").Append(SvgNumbers.FormatInteger(image.Width))
                           .Append("\" height=\"").Append(SvgNumbers.FormatInteger(image.Height)).Append("\" xlink:href=\"")
                           .Append(EscapeAttribute(image.Href)).Append("\"/>\n");
            }
            string imagesText = writer.TakeText();

            foreach (SvgClip clip in writer.ClipsReferredTo) {
                writer.Text.Append("<clipPath id=\"").Append(writer.IdOfClip(clip)).Append("\">\n<path ");
                if (clip.EvenOdd)
                    writer.Text.Append("class=\"").Append(writer.ClassOf("clip-rule:evenodd")).Append("\" ");
                writer.Text.Append("d=\"").Append(clip.PathData).Append("\"/>\n</clipPath>\n");
            }
            string clipsText = writer.TakeText();

            StringBuilder text = new StringBuilder();
            WriteRootStartTag(text);
            WriteStyleBlock(text, writer, WriteFontFaces());
            WriteMetadata(text);
            if (Paints.RefersToCmykProfile || clipsText.Length > 0 || patternsText.Length > 0 || partsText.Length > 0 || imagesText.Length > 0) {
                text.Append("<defs>\n");
                if (Paints.RefersToCmykProfile)
                    text.Append("<color-profile name=\"").Append(SvgPaints.CmykProfileName).Append("\"/>\n");
                text.Append(clipsText).Append(patternsText).Append(partsText).Append(imagesText).Append("</defs>\n");
            }
            text.Append(pagesText).Append("</svg>\n");
            return text.ToString();
        }

        // Write the root's start tag, sized to the first page (F-4), and, when the document draws text, naming the
        // language of its text as undetermined (T-11). A browser shapes text that names no language in the language of
        // the reader's locale, with the forms a font holds for that language, its locl feature: Caladea draws a
        // Romanian ş with a comma below. The application shapes every text with no language, as the HarfBuzz that
        // HarfBuzzSharp ships is built without the C library's locale, so a font draws the document's text in its
        // default forms wherever it is read, as in the application.
        // text: the text to append to.
        private void WriteRootStartTag(StringBuilder text)
        {
            text.Append("<svg xmlns=\"http://www.w3.org/2000/svg\" xmlns:xlink=\"http://www.w3.org/1999/xlink\" xmlns:npp=\"")
                .Append(MetadataNamespace).Append('"');
            if (pages.Count > 0) {
                string width = SvgNumbers.FormatShortest((float)pages[0].Width);
                string height = SvgNumbers.FormatShortest((float)pages[0].Height);
                text.Append(" width=\"").Append(width).Append("mm\" height=\"").Append(height)
                    .Append("mm\" viewBox=\"0 0 ").Append(width).Append(' ').Append(height).Append('"');
            }
            if (fontFaces.Count > 0)
                text.Append(" xml:lang=\"und\"");
            text.Append(">\n");
        }

        // Ask for the faces that draw the characters of each font, and name the family of each fallback face: the family
        // the face names itself with, and a number after it when a face of that family, weight and style is embedded
        // already. Every face is then embedded under a family of its own, and a text names the family of each run that a
        // fallback face draws, so that a viewer draws each character in the face the application drew it in, and never
        // in a font of its own (T-8). A face that may not be embedded keeps its own family, which an installed font may
        // answer to (T-9). The faces are resolved once, before the text that names them is written.
        private void ResolveFontFaces()
        {
            if (fontSubsets == null || fontFacesResolved)
                return;
            fontFacesResolved = true;
            HashSet<string> taken = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (SvgFontFace face in fontFaces)
                taken.Add(FamilyKey(face.Family, face.Bold, face.Italic));
            foreach (SvgFontFace face in fontFaces) {
                face.SetSubsets(fontSubsets.GetSubsets(face.Family, face.Effects, face.Characters), subset => {
                    if (subset.FontData == null)
                        return subset.FaceName;
                    string family = subset.FaceName;
                    for (int number = 2; !taken.Add(FamilyKey(family, face.Bold, face.Italic)); ++number)
                        family = subset.FaceName + " " + number.ToString(CultureInfo.InvariantCulture);
                    return family;
                });
            }
        }

        // The key of a family with a weight and style, which no two embedded faces share.
        // family: the family.
        // bold, italic: whether the face is bold, and whether it is italic.
        private static string FamilyKey(string family, bool bold, bool italic)
        {
            return (bold ? "b" : "-") + (italic ? "i" : "-") + family;
        }

        // Write the @font-face rules of the faces text is drawn in, in the order they were first drawn in, each font with
        // a rule for each face that draws its characters and may be embedded (T-6 to T-9); and, when there is text, the rule that
        // every text element follows: no kerning or ligatures of the browser's own, since the kerning is written with the
        // text and the application draws no ligatures; emoji in the faces embedded, as the application draws them, where
        // a browser would take a color emoji font of its own (T-8); and spaces kept as they are (T-3). Return the rules.
        private string WriteFontFaces()
        {
            StringBuilder rules = new StringBuilder();
            foreach (SvgFontFace face in fontFaces)
                face.AppendFontFaceRules(rules);
            if (fontFaces.Count > 0)
                rules.Append("text{font-kerning:none;font-variant-ligatures:none;font-variant-emoji:text;white-space:pre}\n");
            return rules.ToString();
        }

        // Write the style block: the rules of the fonts and text, then one class to a line, in the order the classes were
        // named (S-4).
        // text: the text to append to.
        // writer: the writer that named the classes.
        // fontRules: the rules of the fonts and text, or nothing when there is no text.
        private static void WriteStyleBlock(StringBuilder text, SvgWriter writer, string fontRules)
        {
            if (writer.StylesInOrder.Count == 0 && fontRules.Length == 0)
                return;
            text.Append("<style>\n").Append(fontRules);
            foreach (string style in writer.StylesInOrder)
                text.Append('.').Append(writer.ClassOf(style)).Append('{').Append(style).Append("}\n");
            text.Append("</style>\n");
        }

        // Write the metadata: the version of its format and the record of each map, with where it lies in the real world
        // and the tables of its colors and symbols (F-3, G-1, G-2, M-7).
        // text: the text to append to.
        private void WriteMetadata(StringBuilder text)
        {
            text.Append("<metadata>\n<npp:document version=\"").Append(FormatVersion).Append("\">\n");
            foreach (SvgMapRecord map in Maps) {
                text.Append("<npp:map n=\"").Append(SvgNumbers.FormatInteger(map.Number)).Append("\" label=\"")
                    .Append(EscapeAttribute(map.Label)).Append('"');
                if (!map.HasContent) {
                    text.Append("/>\n");
                    continue;
                }

                text.Append(">\n");
                if (map.Georeference != null)
                    WriteGeoreferenceRecord(text, map.Georeference);
                foreach (SvgColorRecord color in map.Colors)
                    WriteColorRecord(text, color);
                foreach (SvgSymbolRecord symbol in map.Symbols)
                    WriteSymbolRecord(text, symbol);
                if (map.Course != null)
                    WriteCourseRecord(text, map.Course);
                if (map.ObjectDescriptions != null)
                    WriteObjectRecords(text, map);
                text.Append("</npp:map>\n");
            }
            text.Append("</npp:document>\n</metadata>\n");
        }

        // Write the record of where a map lies in the real world: the transform from the coordinates its drawing is
        // written in, steps of map, to real-world coordinates, written as every transform is and so left out when it
        // moves nothing (S-8, S-9), with the digits that keep any point within the map's reach less than a step of map
        // from where the exact transform takes it (S-2); and each identification of the coordinate system that the map
        // has (G-1, G-2).
        // text: the text to append to.
        // georeference: where the map lies in the real world.
        private static void WriteGeoreferenceRecord(StringBuilder text, MapGeoreference georeference)
        {
            SvgTransform stepsToRealWorld = new SvgTransform(
                georeference.A / SvgNumbers.StepsPerMillimetre, georeference.B / SvgNumbers.StepsPerMillimetre,
                georeference.C / SvgNumbers.StepsPerMillimetre, georeference.D / SvgNumbers.StepsPerMillimetre,
                georeference.E, georeference.F);
            string transform = stepsToRealWorld.Format(MapReachInSteps, stepsToRealWorld.LengthScale);

            text.Append("<npp:georeference");
            if (transform != "")
                text.Append(" transform=\"").Append(transform).Append('"');
            if (georeference.EpsgCode != 0)
                text.Append(" epsg=\"").Append(SvgNumbers.FormatInteger(georeference.EpsgCode)).Append('"');
            if (!string.IsNullOrWhiteSpace(georeference.Proj4String))
                text.Append(" proj4=\"").Append(EscapeAttribute(georeference.Proj4String.Trim())).Append('"');
            if (georeference.OcadGridAndZone != 0)
                text.Append(" ocad-grid-zone=\"").Append(SvgNumbers.FormatInteger(georeference.OcadGridAndZone)).Append('"');
            text.Append("/>\n");
        }

        // Write the record of a color: its position, name and CMYK values, which carry the fewest digits that give the
        // map's own values back, and whether it overprints, when it does (M-7, COL-4).
        // text: the text to append to.
        // color: the record of the color.
        private static void WriteColorRecord(StringBuilder text, SvgColorRecord color)
        {
            text.Append("<npp:color position=\"").Append(SvgNumbers.FormatInteger(color.Position)).Append('"');
            if (!string.IsNullOrEmpty(color.Name))
                text.Append(" name=\"").Append(EscapeAttribute(color.Name)).Append('"');
            text.Append(" cmyk=\"");
            float[] components = { color.Color.Cyan, color.Color.Magenta, color.Color.Yellow, color.Color.Black };
            for (int index = 0; index < components.Length; ++index) {
                if (index > 0)
                    text.Append(',');
                text.Append(SvgNumbers.FormatShortest(components[index]));
            }
            text.Append('"');
            if (color.Overprint)
                text.Append(" overprint=\"1\"");
            text.Append("/>\n");
        }

        // Write the record of the course a map draws: its name, part, variation and print scale, and its controls in the
        // order the course visits them, each with its place, code, kind and position in steps of map, as the drawing
        // gives coordinates (CD-1).
        // text: the text to append to.
        // course: the course.
        private static void WriteCourseRecord(StringBuilder text, CourseRecord course)
        {
            text.Append("<npp:course");
            if (!string.IsNullOrEmpty(course.Name))
                text.Append(" name=\"").Append(EscapeAttribute(course.Name)).Append('"');
            if (course.Part > 0)
                text.Append(" part=\"").Append(SvgNumbers.FormatInteger(course.Part)).Append('"');
            if (!string.IsNullOrEmpty(course.Variation))
                text.Append(" variation=\"").Append(EscapeAttribute(course.Variation)).Append('"');
            if (course.PrintScale > 0)
                text.Append(" scale=\"").Append(SvgNumbers.FormatShortest(course.PrintScale)).Append('"');
            if (course.Controls.Count == 0) {
                text.Append("/>\n");
                return;
            }
            text.Append(">\n");
            for (int index = 0; index < course.Controls.Count; ++index) {
                CourseControlRecord control = course.Controls[index];
                text.Append("<npp:control n=\"").Append(SvgNumbers.FormatInteger(index + 1)).Append('"');
                if (!string.IsNullOrEmpty(control.Code))
                    text.Append(" code=\"").Append(EscapeAttribute(control.Code)).Append('"');
                if (!string.IsNullOrEmpty(control.Kind))
                    text.Append(" kind=\"").Append(EscapeAttribute(control.Kind)).Append('"');
                text.Append(" x=\"").Append(SvgNumbers.FormatInteger(SvgNumbers.ToSteps(control.Position.X)))
                    .Append("\" y=\"").Append(SvgNumbers.FormatInteger(SvgNumbers.ToSteps(control.Position.Y))).Append("\"/>\n");
            }
            text.Append("</npp:course>\n");
        }

        // Write the record of each object of a map that the course describes and that drew an element: its number, the
        // one its elements carry, its kind, and the places of the course's controls it belongs to (CD-2).
        // text: the text to append to.
        // map: the record of the map.
        private void WriteObjectRecords(StringBuilder text, SvgMapRecord map)
        {
            foreach (object objectKey in map.ObjectKeys) {
                CourseObjectRecord description;
                int number;
                if (!map.ObjectDescriptions.TryGetValue(objectKey, out description) || description == null || !TryGetNumber(objectKey, out number))
                    continue;
                text.Append("<npp:object n=\"").Append(SvgNumbers.FormatInteger(number)).Append('"');
                if (!string.IsNullOrEmpty(description.Kind))
                    text.Append(" kind=\"").Append(EscapeAttribute(description.Kind)).Append('"');
                if (description.Controls != null && description.Controls.Length > 0) {
                    text.Append(" controls=\"");
                    for (int index = 0; index < description.Controls.Length; ++index) {
                        if (index > 0)
                            text.Append(' ');
                        text.Append(SvgNumbers.FormatInteger(description.Controls[index]));
                    }
                    text.Append('"');
                }
                text.Append("/>\n");
            }
        }

        // Write the record of a symbol: its number, id and name, and the positions of the colors its objects are drawn
        // in, when they are drawn in any (M-7).
        // text: the text to append to.
        // symbol: the record of the symbol.
        private static void WriteSymbolRecord(StringBuilder text, SvgSymbolRecord symbol)
        {
            text.Append("<npp:symbol n=\"").Append(SvgNumbers.FormatInteger(symbol.Number)).Append('"');
            if (!string.IsNullOrEmpty(symbol.Id))
                text.Append(" id=\"").Append(EscapeAttribute(symbol.Id)).Append('"');
            if (!string.IsNullOrEmpty(symbol.Name))
                text.Append(" name=\"").Append(EscapeAttribute(symbol.Name)).Append('"');
            if (symbol.ColorPositions.Count > 0) {
                text.Append(" colors=\"");
                for (int index = 0; index < symbol.ColorPositions.Count; ++index) {
                    if (index > 0)
                        text.Append(' ');
                    text.Append(SvgNumbers.FormatInteger(symbol.ColorPositions[index]));
                }
                text.Append('"');
            }
            text.Append("/>\n");
        }

        // Write a page: a group that clips the parts to the area the page shows and transforms them onto its paper.
        // writer: the writer of the document.
        // page: the page.
        private static void WritePage(SvgWriter writer, SvgPage page)
        {
            SvgTransform stepsToCanvas = new SvgTransform(1, 0, 0, 1, page.Left, 0).After(page.StepsToPage);
            SvgBounds reached = new SvgBounds();
            reached.Include(page.ClipBounds);
            foreach (SvgPart part in page.Parts)
                reached.Include(part.Content.GetBoundsOfChildren());

            writer.Text.Append("<g transform=\"").Append(stepsToCanvas.Format(reached.ReachFromOrigin, stepsToCanvas.LengthScale))
                       .Append("\" clip-path=\"url(#").Append(writer.IdOfClip(page.Clip)).Append(")\">\n");
            foreach (SvgPart part in page.Parts)
                new SvgPartReference(part).Write(writer);
            writer.Text.Append("</g>\n");
        }

        // Write a pattern's definition: its tile, in the user space of what it paints, turned by its angle, with a view
        // box that keeps the tile's own coordinates, whose origin is the tile's middle (D-7).
        // writer: the writer of the document.
        // pattern: the pattern.
        private static void WritePattern(SvgWriter writer, SvgPattern pattern)
        {
            pattern.Content.TidyChildren();
            string left = SvgNumbers.FormatShortest(-pattern.Width / 2), top = SvgNumbers.FormatShortest(-pattern.Height / 2);
            string width = SvgNumbers.FormatShortest(pattern.Width), height = SvgNumbers.FormatShortest(pattern.Height);
            writer.Text.Append("<pattern id=\"").Append(writer.IdOfPattern(pattern)).Append("\" patternUnits=\"userSpaceOnUse\" x=\"")
                       .Append(left).Append("\" y=\"").Append(top).Append("\" width=\"").Append(width).Append("\" height=\"").Append(height)
                       .Append("\" viewBox=\"").Append(left).Append(' ').Append(top).Append(' ').Append(width).Append(' ').Append(height).Append('"');
            if (pattern.Angle != 0)
                writer.Text.Append(" patternTransform=\"rotate(").Append(SvgNumbers.FormatShortest(pattern.Angle)).Append(")\"");
            writer.Text.Append(">\n");
            pattern.Content.WriteChildren(writer);
            writer.Text.Append("</pattern>\n");
        }

        // Escape text for an attribute's value.
        // value: the text.
        public static string EscapeAttribute(string value)
        {
            return value.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;");
        }
    }

    // Writes a document's text, and names its classes and ids in the order they are first used (F-2, S-4, L-5).
    public sealed class SvgWriter
    {
        // The text being written.
        public StringBuilder Text = new StringBuilder();

        // The names of the classes, and the style each was named for.
        private readonly SvgNames classNames = new SvgNames();
        private readonly Dictionary<string, string> classOfStyle = new Dictionary<string, string>(StringComparer.Ordinal);

        // The styles, in the order their classes were named.
        public readonly List<string> StylesInOrder = new List<string>();

        // The names of the ids, and what each was named for.
        private readonly SvgNames ids = new SvgNames();
        private readonly Dictionary<object, string> idOfDefinition = new Dictionary<object, string>(new IdentityComparer<object>());

        // The clips, the patterns and the images referred to, each in the order they were first referred to, which the
        // definitions then hold.
        public readonly List<SvgClip> ClipsReferredTo = new List<SvgClip>();
        public readonly List<SvgPattern> PatternsReferredTo = new List<SvgPattern>();
        public readonly List<SvgImage> ImagesReferredTo = new List<SvgImage>();

        // Return the text written so far, and start anew.
        public string TakeText()
        {
            string text = Text.ToString();
            Text = new StringBuilder();
            return text;
        }

        // The class of a style, named when the style is first used; styles of the same declarations share it (S-4).
        // style: the declarations of the style.
        public string ClassOf(string style)
        {
            string name;
            if (!classOfStyle.TryGetValue(style, out name)) {
                name = classNames.Next();
                classOfStyle.Add(style, name);
                StylesInOrder.Add(style);
            }
            return name;
        }

        // The id of a definition, named when it is first used.
        // definition: what the id identifies.
        public string IdOf(object definition)
        {
            string id;
            if (!idOfDefinition.TryGetValue(definition, out id)) {
                id = ids.Next();
                idOfDefinition.Add(definition, id);
            }
            return id;
        }

        // The id of a clip, which the definitions then hold.
        // clip: the clip.
        public string IdOfClip(SvgClip clip)
        {
            if (!idOfDefinition.ContainsKey(clip))
                ClipsReferredTo.Add(clip);
            return IdOf(clip);
        }

        // The id of a pattern, which the definitions then hold.
        // pattern: the pattern.
        public string IdOfPattern(SvgPattern pattern)
        {
            if (!idOfDefinition.ContainsKey(pattern))
                PatternsReferredTo.Add(pattern);
            return IdOf(pattern);
        }

        // The id of an image, which the definitions then hold.
        // image: the image.
        public string IdOfImage(SvgImage image)
        {
            if (!idOfDefinition.ContainsKey(image))
                ImagesReferredTo.Add(image);
            return IdOf(image);
        }
    }
}
