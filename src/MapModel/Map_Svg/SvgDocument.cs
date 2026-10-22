using System;
using System.Collections.Generic;
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

    // A labeled map, as the metadata records it (M-5, M-7).
    public sealed class SvgMapRecord
    {
        // The number by which the map's groups refer to it, counting from 1 in the order maps first begin.
        public readonly int Number;

        // What the map is called.
        public readonly string Label;

        // Create the record of a map.
        // number: its number.
        // label: what it is called.
        public SvgMapRecord(int number, string label)
        {
            Number = number;
            Label = label;
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

        // Where the next page goes: the left edge, in millimetres.
        private double nextPageLeft;

        // Create a document.
        // paints: the paints of its styles.
        public SvgDocument(SvgPaints paints)
        {
            Paints = paints;
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

        // Write the document's text: the style block, the metadata, the definitions of clips, patterns and parts, and
        // then the pages. Classes and ids are named in the order they are first used (F-2).
        public string Write()
        {
            foreach (SvgPart part in Parts)
                part.Content.TidyChildren();

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

            foreach (SvgClip clip in writer.ClipsReferredTo) {
                writer.Text.Append("<clipPath id=\"").Append(writer.IdOfClip(clip)).Append("\">\n<path ");
                if (clip.EvenOdd)
                    writer.Text.Append("class=\"").Append(writer.ClassOf("clip-rule:evenodd")).Append("\" ");
                writer.Text.Append("d=\"").Append(clip.PathData).Append("\"/>\n</clipPath>\n");
            }
            string clipsText = writer.TakeText();

            StringBuilder text = new StringBuilder();
            WriteRootStartTag(text);
            WriteStyleBlock(text, writer);
            WriteMetadata(text);
            if (Paints.RefersToCmykProfile || clipsText.Length > 0 || patternsText.Length > 0 || partsText.Length > 0) {
                text.Append("<defs>\n");
                if (Paints.RefersToCmykProfile)
                    text.Append("<color-profile name=\"").Append(SvgPaints.CmykProfileName).Append("\"/>\n");
                text.Append(clipsText).Append(patternsText).Append(partsText).Append("</defs>\n");
            }
            text.Append(pagesText).Append("</svg>\n");
            return text.ToString();
        }

        // Write the root's start tag, sized to the first page (F-4).
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
            text.Append(">\n");
        }

        // Write the style block: one class to a line, in the order the classes were named (S-4).
        // text: the text to append to.
        // writer: the writer that named the classes.
        private static void WriteStyleBlock(StringBuilder text, SvgWriter writer)
        {
            if (writer.StylesInOrder.Count == 0)
                return;
            text.Append("<style>\n");
            foreach (string style in writer.StylesInOrder)
                text.Append('.').Append(writer.ClassOf(style)).Append('{').Append(style).Append("}\n");
            text.Append("</style>\n");
        }

        // Write the metadata: the version of its format and the record of each map (F-3, M-7).
        // text: the text to append to.
        private void WriteMetadata(StringBuilder text)
        {
            text.Append("<metadata>\n<npp:document version=\"").Append(FormatVersion).Append("\">\n");
            foreach (SvgMapRecord map in Maps) {
                text.Append("<npp:map n=\"").Append(SvgNumbers.FormatInteger(map.Number)).Append("\" label=\"")
                    .Append(EscapeAttribute(map.Label)).Append("\"/>\n");
            }
            text.Append("</npp:document>\n</metadata>\n");
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

        // The clips and the patterns referred to, each in the order they were first referred to, which the definitions
        // then hold.
        public readonly List<SvgClip> ClipsReferredTo = new List<SvgClip>();
        public readonly List<SvgPattern> PatternsReferredTo = new List<SvgPattern>();

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
    }
}
