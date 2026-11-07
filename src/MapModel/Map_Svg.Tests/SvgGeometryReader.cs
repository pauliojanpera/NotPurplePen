using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

namespace Map_Svg.Tests
{
    // Reads the first page of an SVG document back into page coordinates (V-10): every drawing within it, in the order
    // drawn, with the transforms the document writes, the styles drawings inherit and the uses of parts, repeated shapes
    // and images applied, as a DrawnGeometry of the same form as CallGeometryRecorder gives a call's; the clip of the page;
    // and, as the drawings that paint with them are compared, the tiles of their patterns, in steps of the tile.
    //
    // Each point carries how far it may lie from where the call puts it, which the document's rounding allows. The
    // document writes the coordinates of a frame in whole units of it, the steps of the calls' own coordinates there,
    // and a text the positions of its characters to a step of the frame it lies in. A point written so lies within
    // half a unit on each axis, √½ of a unit; a point rounded twice, as a character of a line text (T-5) or an ellipse
    // moved into place (S-8), within √2; a point a rounded radius or em from another, half a unit more. Each transform
    // the document writes moves a point within its reach less than a unit of the frame it writes into (S-2), which adds
    // that unit, on the page, to every point below it. A radius may lie a unit from the call's, as SVG enlarges a radius
    // too short for its arc's ends, and a width half a unit.
    public sealed class SvgGeometryReader
    {
        // The namespaces of SVG and of its links.
        private static readonly XNamespace Svg = "http://www.w3.org/2000/svg";
        private static readonly XNamespace XLink = "http://www.w3.org/1999/xlink";

        // How far a point written in whole units may lie from the exact point: half a unit on each axis.
        private static readonly double RoundedOnce = Math.Sqrt(0.5);

        // How far a point rounded twice may lie from the exact point: a unit on each axis.
        private static readonly double RoundedTwice = Math.Sqrt(2);

        // A number of SVG, starting at a position.
        private static readonly Regex Number = new Regex(@"\G[+-]?(?:\d+\.?\d*|\.\d+)(?:[eE][+-]?\d+)?", RegexOptions.CultureInvariant);

        // A function of a transform attribute, starting at a position.
        private static readonly Regex TransformFunction = new Regex(@"\G[\s,]*([A-Za-z]+)\s*\(([^)]*)\)", RegexOptions.CultureInvariant);

        // A reference to an id, as url(#id).
        private static readonly Regex UrlReference = new Regex(@"^\s*url\(#([^)]+)\)\s*$", RegexOptions.CultureInvariant);

        // A frame that drawings are written in: the transform from it to the page, the unit its numbers are rounded to,
        // in its own units, and how far the transforms above it may move a point, on the page.
        private sealed class Frame
        {
            // The transform from the frame's coordinates to the page's.
            public readonly Affine ToPage;

            // The unit the document rounds the frame's numbers to, in the frame's own units.
            public readonly double RoundingUnit;

            // How far, on the page, the transforms above the frame may move a point of it.
            public readonly double Budget;

            // Create a frame.
            // toPage: the transform from its coordinates to the page's.
            // roundingUnit: the unit its numbers are rounded to, in its own units.
            // budget: how far, on the page, the transforms above it may move a point of it.
            public Frame(Affine toPage, double roundingUnit, double budget)
            {
                ToPage = toPage;
                RoundingUnit = roundingUnit;
                Budget = budget;
            }

            // The frame within this one under a transform the document writes in it, as a group or a use carries one,
            // whose content the document writes in whole units of its own: the steps of the calls' coordinates within
            // it. The transform moves a point within its reach less than a unit of this frame (S-2).
            // transform: the transform, from the inner frame to this one.
            public Frame Within(Affine transform)
            {
                return new Frame(ToPage.After(transform), 1, Budget + RoundingUnit * ToPage.MaxStretch);
            }

            // The frame of a text's own coordinates under its transform, which sets the text in millimetres and places
            // its characters to a step of this frame, as the text's transform scales a step of this frame to.
            // transform: the text's transform, from its coordinates to this frame's.
            public Frame WithinText(Affine transform)
            {
                return new Frame(ToPage.After(transform), RoundingUnit / transform.LengthScale, Budget + RoundingUnit * ToPage.MaxStretch);
            }

            // How far a point of the frame may lie from where the call puts it, on the page, when its coordinates lie
            // within a number of rounding units of the exact ones.
            // units: the number of rounding units.
            public double Allowance(double units)
            {
                return units * RoundingUnit * ToPage.MaxStretch + Budget;
            }

            // How far a length of the frame may lie from the call's, on the page, when it lies within a number of
            // rounding units of the exact one.
            // units: the number of rounding units.
            public double LengthAllowance(double units)
            {
                return units * RoundingUnit * ToPage.LengthScale;
            }
        }

        // The style a drawing inherits: its fill, its stroke and the stroke's width, its fill rule and its font size.
        private sealed class Style
        {
            // The style of the root, by SVG's initial values.
            public static readonly Style Initial = new Style("#000", "none", 1, false, 16);

            // The fill and the stroke, as the last declaration of each gives them.
            public readonly string Fill, Stroke;

            // The stroke's width and the font's size, in the user units of the element that draws.
            public readonly double StrokeWidth, FontSize;

            // Whether the fill follows the even-odd rule.
            public readonly bool EvenOdd;

            // Create a style.
            // fill, stroke: the fill and the stroke.
            // strokeWidth: the stroke's width.
            // evenOdd: whether the fill follows the even-odd rule.
            // fontSize: the font's size.
            private Style(string fill, string stroke, double strokeWidth, bool evenOdd, double fontSize)
            {
                Fill = fill;
                Stroke = stroke;
                StrokeWidth = strokeWidth;
                EvenOdd = evenOdd;
                FontSize = fontSize;
            }

            // The style with an element's style attribute applied, its declarations in order, each later one winning.
            // element: the element.
            public Style With(XElement element)
            {
                string attribute = (string)element.Attribute("style");
                if (attribute == null)
                    return this;
                string fill = Fill, stroke = Stroke;
                double strokeWidth = StrokeWidth, fontSize = FontSize;
                bool evenOdd = EvenOdd;
                foreach (string declaration in attribute.Split(';')) {
                    int colon = declaration.IndexOf(':');
                    if (colon < 0)
                        continue;
                    string name = declaration.Substring(0, colon).Trim(), value = declaration.Substring(colon + 1).Trim();
                    switch (name) {
                        case "fill":
                            fill = value;
                            break;
                        case "stroke":
                            stroke = value;
                            break;
                        case "stroke-width":
                            strokeWidth = ParseLength(value);
                            break;
                        case "fill-rule":
                            evenOdd = value == "evenodd";
                            break;
                        case "font-size":
                            fontSize = ParseLength(value);
                            break;
                    }
                }
                return new Style(fill, stroke, strokeWidth, evenOdd, fontSize);
            }
        }

        // The document's elements, by their ids.
        private readonly Dictionary<string, XElement> elementsById = new Dictionary<string, XElement>(StringComparer.Ordinal);

        // The tiles of the patterns, by their ids, made when first painted with.
        private readonly Dictionary<string, GeometryTile> tilesById = new Dictionary<string, GeometryTile>(StringComparer.Ordinal);

        // The drawings of the page, in the order drawn.
        public readonly List<DrawnGeometry> Drawings = new List<DrawnGeometry>();

        // The clip of the page: the area of the map it shows.
        public DrawnGeometry PageClip;

        // How long a step of the map is on the page.
        public double StepOnPage;

        // Read the first page of a document: the first group of the root, whose transform takes the steps of the map to
        // the page, and the parts it uses.
        // document: the document's text.
        public static SvgGeometryReader ReadFirstPage(string document)
        {
            SvgGeometryReader reader = new SvgGeometryReader();
            XDocument parsed = XDocument.Parse(document, LoadOptions.PreserveWhitespace | LoadOptions.SetLineInfo);
            foreach (XElement element in parsed.Root.Descendants()) {
                string id = (string)element.Attribute("id");
                if (id != null)
                    reader.elementsById[id] = element;
            }

            XElement page = parsed.Root.Elements(Svg + "g").First();
            Affine pageTransform = ParseTransform((string)page.Attribute("transform"));
            reader.StepOnPage = pageTransform.LengthScale;
            // The page's transform moves a point less than a step of the map on the page.
            Frame frame = new Frame(Affine.Identity, reader.StepOnPage, 0).Within(pageTransform);
            reader.PageClip = reader.ReadClip(page, frame);
            foreach (XElement child in page.Elements())
                reader.Descend(child, frame, Style.Initial, Array.Empty<DrawnGeometry>(), reader.Drawings);
            return reader;
        }

        // Read the drawings of an element and of everything within it.
        // element: the element.
        // frame: the frame it is written in.
        // style: the style it inherits.
        // clips: the clips around it, outermost first.
        // drawings: receives the drawings, in the order drawn.
        private void Descend(XElement element, Frame frame, Style style, DrawnGeometry[] clips, List<DrawnGeometry> drawings)
        {
            if (element.Name.Namespace != Svg)
                return;
            switch (element.Name.LocalName) {
                case "g":
                    ReadGroup(element, frame, style, clips, drawings);
                    break;
                case "use":
                    ReadUse(element, frame, style, clips, drawings);
                    break;
                case "path":
                case "circle":
                case "ellipse":
                    drawings.Add(ReadShape(element, frame, style.With(element), clips, (string)element.Attribute("data-o"), null));
                    break;
                case "text":
                    ReadText(element, frame, style.With(element), clips, drawings);
                    break;
            }
        }

        // Read the drawings of a group, under its transform, its style and its clip.
        // group: the group.
        // frame: the frame it is written in.
        // style: the style it inherits.
        // clips: the clips around it, outermost first.
        // drawings: receives the drawings, in the order drawn.
        private void ReadGroup(XElement group, Frame frame, Style style, DrawnGeometry[] clips, List<DrawnGeometry> drawings)
        {
            Frame inner = WithinTransform(group, frame);
            Style innerStyle = style.With(group);
            DrawnGeometry clip = ReadClip(group, inner);
            DrawnGeometry[] innerClips = clip == null ? clips : clips.Append(clip).ToArray();
            foreach (XElement child in group.Elements())
                Descend(child, inner, innerStyle, innerClips, drawings);
        }

        // Read what a use draws: the image, the group or the shape it refers to, under the use's transform and style.
        // use: the use.
        // frame: the frame it is written in.
        // style: the style it inherits.
        // clips: the clips around it, outermost first.
        // drawings: receives the drawings, in the order drawn.
        private void ReadUse(XElement use, Frame frame, Style style, DrawnGeometry[] clips, List<DrawnGeometry> drawings)
        {
            XElement referred = Referred(use, (string)use.Attribute(XLink + "href"));
            Frame inner = WithinTransform(use, frame);
            Style innerStyle = style.With(use);
            switch (referred.Name.LocalName) {
                case "image":
                    drawings.Add(ReadImage(use, referred, frame, inner, clips));
                    break;
                case "g":
                    ReadGroup(referred, inner, innerStyle, clips, drawings);
                    break;
                default:
                    drawings.Add(ReadShape(referred, inner, innerStyle.With(referred), clips,
                                           (string)use.Attribute("data-o") ?? (string)referred.Attribute("data-o"), use));
                    break;
            }
        }

        // Read a path, circle or ellipse, as drawn by itself or through a use.
        // shape: the shape's element.
        // frame: the frame it is written in.
        // style: its style.
        // clips: the clips around it.
        // objectNumber: the number of its object, or null.
        // use: the use that draws it, or null.
        private DrawnGeometry ReadShape(XElement shape, Frame frame, Style style, DrawnGeometry[] clips, string objectNumber, XElement use)
        {
            DrawnGeometry drawing = new DrawnGeometry(Describe(use ?? shape, objectNumber) + (use == null ? "" : ", through " + Describe(shape, null)));
            drawing.Clips = clips;
            string paint = BeginPaint(drawing, style, frame, false);
            if (shape.Name.LocalName == "path") {
                ReadPathData((string)shape.Attribute("d") ?? "", frame, new PathGeometryBuilder(drawing));
            }
            else {
                double centerX = ReadLength(shape, "cx"), centerY = ReadLength(shape, "cy");
                double radiusX = shape.Name.LocalName == "circle" ? ReadLength(shape, "r") : ReadLength(shape, "rx");
                double radiusY = shape.Name.LocalName == "circle" ? radiusX : ReadLength(shape, "ry");
                drawing.Signature.Append(" ellipse");
                drawing.AddPoint(frame.ToPage.Apply(centerX, centerY), frame.Allowance(RoundedTwice));
                drawing.AddPoint(frame.ToPage.Apply(centerX + radiusX, centerY), frame.Allowance(RoundedTwice + 0.5));
                drawing.AddPoint(frame.ToPage.Apply(centerX, centerY + radiusY), frame.Allowance(RoundedTwice + 0.5));
            }
            FinishPaint(drawing, paint, frame);
            return drawing;
        }

        // Read a text: a line text, which carries each character's turn, character by character, and another as a
        // whole, by the start of its baseline, where its first character lies, and the point an em along it.
        // text: the text's element.
        // frame: the frame it is written in.
        // style: its style.
        // clips: the clips around it.
        // drawings: receives its drawings.
        private void ReadText(XElement text, Frame frame, Style style, DrawnGeometry[] clips, List<DrawnGeometry> drawings)
        {
            string transform = (string)text.Attribute("transform");
            Frame textFrame = transform == null ? frame : frame.WithinText(ParseTransform(transform));
            string characters = DocumentCharacters.Of(text.Value);
            string objectNumber = (string)text.Attribute("data-o");
            double[] xs = ReadList(text, "x"), ys = ReadList(text, "y"), turns = ReadList(text, "rotate");
            if (turns.Length == 0) {
                DrawnGeometry drawing = BeginText(text, style, textFrame, clips, characters, objectNumber);
                double x = xs.Length > 0 ? xs[0] : 0, y = ys.Length > 0 ? ys[0] : 0;
                drawing.AddPoint(textFrame.ToPage.Apply(x, y), textFrame.Allowance(RoundedOnce));
                drawing.AddPoint(textFrame.ToPage.Apply(x + style.FontSize, y), textFrame.Allowance(RoundedOnce + 0.5));
                FinishPaint(drawing, PaintOf(style), textFrame);
                drawings.Add(drawing);
                return;
            }

            int index = 0;
            for (int position = 0; position < characters.Length; position += char.IsSurrogatePair(characters, position) ? 2 : 1, ++index) {
                string character = characters.Substring(position, char.IsSurrogatePair(characters, position) ? 2 : 1);
                DrawnGeometry drawing = BeginText(text, style, textFrame, clips, character, objectNumber);
                double x = xs[Math.Min(index, xs.Length - 1)], y = ys[Math.Min(index, ys.Length - 1)];
                double turn = turns[Math.Min(index, turns.Length - 1)] * Math.PI / 180;
                drawing.AddPoint(textFrame.ToPage.Apply(x, y), textFrame.Allowance(RoundedTwice));
                drawing.AddPoint(textFrame.ToPage.Apply(x + style.FontSize * Math.Cos(turn), y + style.FontSize * Math.Sin(turn)),
                                 textFrame.Allowance(RoundedTwice + 0.5));
                FinishPaint(drawing, PaintOf(style), textFrame);
                drawings.Add(drawing);
            }
        }

        // Begin the geometry of a text's characters: its paint, its outline's width, and its characters.
        // text: the text's element.
        // style: its style.
        // textFrame: the frame of its own coordinates.
        // clips: the clips around it.
        // characters: the characters the geometry draws.
        // objectNumber: the number of its object, or null.
        private DrawnGeometry BeginText(XElement text, Style style, Frame textFrame, DrawnGeometry[] clips, string characters, string objectNumber)
        {
            DrawnGeometry drawing = new DrawnGeometry(Describe(text, objectNumber));
            drawing.Clips = clips;
            BeginPaint(drawing, style, textFrame, true);
            drawing.Signature.Append(" '").Append(characters).Append('\'');
            return drawing;
        }

        // Read the use of an image: the corners of the image placed by the use's transform, which puts the corners of
        // the call's rectangle, rounded to whole units of the frame the use is written in, where its pixels' corners go.
        // use: the use.
        // image: the image.
        // frame: the frame the use is written in.
        // imageFrame: the frame of the image's pixels.
        // clips: the clips around it.
        private DrawnGeometry ReadImage(XElement use, XElement image, Frame frame, Frame imageFrame, DrawnGeometry[] clips)
        {
            DrawnGeometry drawing = new DrawnGeometry(Describe(use, (string)use.Attribute("data-o")));
            drawing.Clips = clips;
            drawing.Signature.Append("image");
            double width = ReadLength(image, "width"), height = ReadLength(image, "height");
            double allowance = RoundedOnce * frame.RoundingUnit * frame.ToPage.MaxStretch + imageFrame.Budget;
            drawing.AddPoint(imageFrame.ToPage.Apply(0, 0), allowance);
            drawing.AddPoint(imageFrame.ToPage.Apply(width, 0), allowance);
            drawing.AddPoint(imageFrame.ToPage.Apply(width, height), allowance);
            drawing.AddPoint(imageFrame.ToPage.Apply(0, height), allowance);
            return drawing;
        }

        // Begin a drawing's signature with its kind and paint, and a stroke's width, and return the paint it paints
        // with, which a pattern's lattice is added for when the drawing is finished.
        // drawing: the drawing.
        // style: its style.
        // frame: the frame it is written in.
        // isText: whether it is a text.
        private static string BeginPaint(DrawnGeometry drawing, Style style, Frame frame, bool isText)
        {
            bool stroked = style.Stroke != "none", filled = style.Fill != "none";
            if (stroked && filled) {
                drawing.Signature.Append(isText ? "text filled and outlined" : "fill and stroke");
            }
            else if (stroked) {
                drawing.Signature.Append(isText ? "text outline" : "stroke");
                drawing.AddLength(style.StrokeWidth * frame.ToPage.LengthScale, frame.LengthAllowance(0.5 + 1e-3));
            }
            else if (filled) {
                drawing.Signature.Append(isText ? "text" : (style.EvenOdd ? "fill even-odd" : "fill nonzero"));
            }
            else {
                drawing.Signature.Append(isText ? "text unpainted" : "unpainted");
            }
            return PaintOf(style);
        }

        // What a drawing paints with: its stroke's paint when it is stroked, else its fill's.
        // style: its style.
        private static string PaintOf(Style style)
        {
            return style.Stroke != "none" ? style.Stroke : style.Fill;
        }

        // Finish a drawing that paints with a paint: a pattern adds its lattice, the tile's middle and the ends of half
        // its sides, turned by the pattern's transform, in the frame of what it paints.
        // drawing: the drawing.
        // paint: what it paints with.
        // frame: the frame it is written in.
        private void FinishPaint(DrawnGeometry drawing, string paint, Frame frame)
        {
            Match reference = UrlReference.Match(paint);
            if (!reference.Success)
                return;
            XElement pattern = Referred(null, "#" + reference.Groups[1].Value);
            double x = ReadLength(pattern, "x"), y = ReadLength(pattern, "y"), width = ReadLength(pattern, "width"), height = ReadLength(pattern, "height");
            Affine lattice = frame.ToPage.After(ParseTransform((string)pattern.Attribute("patternTransform")));
            drawing.Signature.Append(" pattern");
            double allowance = frame.Allowance(0.5);
            drawing.AddPoint(lattice.Apply(x + width / 2, y + height / 2), allowance);
            drawing.AddPoint(lattice.Apply(x + width, y + height / 2), allowance);
            drawing.AddPoint(lattice.Apply(x + width / 2, y + height), allowance);
            drawing.Pattern = TileOf(pattern, x, y, width, height);
        }

        // The tile of a pattern: its drawings, in steps of the tile, whose middle is the origin, read through the
        // pattern's view box. The tile's content is written in whole steps of the tile.
        // pattern: the pattern's element.
        // x, y, width, height: the tile's rectangle, in the user space of what the pattern paints.
        private GeometryTile TileOf(XElement pattern, double x, double y, double width, double height)
        {
            string id = (string)pattern.Attribute("id");
            GeometryTile tile;
            if (tilesById.TryGetValue(id, out tile))
                return tile;
            double[] viewBox = ParseNumbers((string)pattern.Attribute("viewBox") ?? string.Format(CultureInfo.InvariantCulture, "{0} {1} {2} {3}", x, y, width, height));
            double scaleX = width / viewBox[2], scaleY = height / viewBox[3];
            Affine contentToTile = new Affine(scaleX, 0, 0, scaleY, x - viewBox[0] * scaleX - (x + width / 2), y - viewBox[1] * scaleY - (y + height / 2));
            tile = new GeometryTile("the tile of " + Describe(pattern, null), () => {
                List<DrawnGeometry> drawings = new List<DrawnGeometry>();
                Frame frame = new Frame(contentToTile, 1, 0);
                foreach (XElement child in pattern.Elements())
                    Descend(child, frame, Style.Initial, Array.Empty<DrawnGeometry>(), drawings);
                return drawings;
            });
            tilesById.Add(id, tile);
            return tile;
        }

        // Read the clip that a group clips to, in the frame within the group, or null when it clips to none.
        // group: the group.
        // frame: the frame within it.
        private DrawnGeometry ReadClip(XElement group, Frame frame)
        {
            Match reference = UrlReference.Match((string)group.Attribute("clip-path") ?? "");
            if (!reference.Success)
                return null;
            XElement clipPath = Referred(group, "#" + reference.Groups[1].Value);
            XElement path = clipPath.Elements(Svg + "path").Single();
            DrawnGeometry clip = new DrawnGeometry(Describe(clipPath, null) + ", which " + Describe(group, null) + " clips to");
            string style = (string)path.Attribute("style") ?? "";
            clip.Signature.Append(style.Contains("clip-rule:evenodd", StringComparison.Ordinal) ? "clip even-odd" : "clip nonzero");
            ReadPathData((string)path.Attribute("d") ?? "", frame, new PathGeometryBuilder(clip));
            return clip;
        }

        // Read path data, written in whole units of a frame, into a path's geometry.
        // data: the path data.
        // frame: the frame it is written in.
        // path: the builder of the path.
        private static void ReadPathData(string data, Frame frame, PathGeometryBuilder path)
        {
            double allowance = frame.Allowance(RoundedOnce);
            int position = 0;
            char command = '\0', previous = '\0';
            double x = 0, y = 0, startX = 0, startY = 0, controlX = 0, controlY = 0;
            while (true) {
                SkipSeparators(data, ref position);
                if (position >= data.Length)
                    break;
                char letter = data[position];
                if (char.IsLetter(letter)) {
                    command = letter;
                    ++position;
                    if (command == 'z' || command == 'Z') {
                        path.Close();
                        x = startX;
                        y = startY;
                        previous = 'Z';
                        continue;
                    }
                }
                else if (command == '\0' || command == 'z' || command == 'Z') {
                    throw new FormatException("path data without a command at " + position.ToString(CultureInfo.InvariantCulture) + ": " + data);
                }

                bool relative = char.IsLower(command);
                double originX = relative ? x : 0, originY = relative ? y : 0;
                switch (char.ToUpperInvariant(command)) {
                    case 'M':
                        x = originX + ReadNumber(data, ref position);
                        y = originY + ReadNumber(data, ref position);
                        path.MoveTo(frame.ToPage.Apply(x, y), allowance);
                        startX = x;
                        startY = y;
                        // Further pairs after a move are lines (SVG 1.1, 8.3.2).
                        command = relative ? 'l' : 'L';
                        break;
                    case 'L':
                        x = originX + ReadNumber(data, ref position);
                        y = originY + ReadNumber(data, ref position);
                        path.LineTo(frame.ToPage.Apply(x, y), allowance);
                        break;
                    case 'H':
                        x = originX + ReadNumber(data, ref position);
                        path.LineTo(frame.ToPage.Apply(x, y), allowance);
                        break;
                    case 'V':
                        y = originY + ReadNumber(data, ref position);
                        path.LineTo(frame.ToPage.Apply(x, y), allowance);
                        break;
                    case 'C':
                    case 'S': {
                        double x1, y1;
                        if (char.ToUpperInvariant(command) == 'C') {
                            x1 = originX + ReadNumber(data, ref position);
                            y1 = originY + ReadNumber(data, ref position);
                        }
                        else if (previous == 'C' || previous == 'S') {
                            // The first control point reflects the second of the curve before (SVG 1.1, 8.3.6).
                            x1 = 2 * x - controlX;
                            y1 = 2 * y - controlY;
                        }
                        else {
                            x1 = x;
                            y1 = y;
                        }
                        double x2 = originX + ReadNumber(data, ref position), y2 = originY + ReadNumber(data, ref position);
                        x = originX + ReadNumber(data, ref position);
                        y = originY + ReadNumber(data, ref position);
                        path.CubicTo(frame.ToPage.Apply(x1, y1), frame.ToPage.Apply(x2, y2), frame.ToPage.Apply(x, y), allowance);
                        controlX = x2;
                        controlY = y2;
                        break;
                    }
                    case 'A': {
                        double radiusX = Math.Abs(ReadNumber(data, ref position)), radiusY = Math.Abs(ReadNumber(data, ref position));
                        double rotation = ReadNumber(data, ref position) * Math.PI / 180;
                        bool large = ReadFlag(data, ref position), sweep = ReadFlag(data, ref position);
                        double endX = originX + ReadNumber(data, ref position), endY = originY + ReadNumber(data, ref position);
                        // A radius too short for the arc's ends is enlarged until it reaches them (SVG 1.1, F.6.6).
                        double cos = Math.Cos(rotation), sin = Math.Sin(rotation);
                        double halfX = cos * (x - endX) / 2 + sin * (y - endY) / 2, halfY = -sin * (x - endX) / 2 + cos * (y - endY) / 2;
                        double lambda = radiusX > 0 && radiusY > 0 ? halfX * halfX / (radiusX * radiusX) + halfY * halfY / (radiusY * radiusY) : 0;
                        double enlargement = lambda > 1 ? Math.Sqrt(lambda) : 1;
                        double radius = Math.Sqrt(radiusX * radiusY) * enlargement;
                        x = endX;
                        y = endY;
                        path.ArcTo(radius * frame.ToPage.LengthScale, frame.LengthAllowance(1), large, sweep != frame.ToPage.Mirrors,
                                   frame.ToPage.Apply(x, y), allowance);
                        break;
                    }
                    default:
                        throw new FormatException("path data with the command " + command + ": " + data);
                }
                previous = char.ToUpperInvariant(command);
            }
        }

        // Skip the separators of path data or a list: whitespace and commas.
        // data: the path data or list.
        // position: the position to skip from; receives the position after the separators.
        private static void SkipSeparators(string data, ref int position)
        {
            while (position < data.Length && (char.IsWhiteSpace(data[position]) || data[position] == ','))
                ++position;
        }

        // Read the number at a position of path data or a list, and move past it.
        // data: the path data or list.
        // position: the position of the number, or of separators before it; receives the position after it.
        private static double ReadNumber(string data, ref int position)
        {
            SkipSeparators(data, ref position);
            Match number = Number.Match(data, position);
            if (!number.Success)
                throw new FormatException("path data without a number at " + position.ToString(CultureInfo.InvariantCulture) + ": " + data);
            position += number.Length;
            return double.Parse(number.Value, NumberStyles.Float, CultureInfo.InvariantCulture);
        }

        // Read an arc's flag at a position of path data, one digit with or without separators around it, and move past it.
        // data: the path data.
        // position: the position of the flag, or of separators before it; receives the position after it.
        private static bool ReadFlag(string data, ref int position)
        {
            SkipSeparators(data, ref position);
            if (position >= data.Length || (data[position] != '0' && data[position] != '1'))
                throw new FormatException("path data without an arc flag at " + position.ToString(CultureInfo.InvariantCulture) + ": " + data);
            return data[position++] == '1';
        }

        // The numbers of a list, such as a view box or an attribute that places characters.
        // text: the list.
        private static double[] ParseNumbers(string text)
        {
            List<double> numbers = new List<double>();
            int position = 0;
            while (true) {
                SkipSeparators(text, ref position);
                if (position >= text.Length)
                    break;
                numbers.Add(ReadNumber(text, ref position));
            }
            return numbers.ToArray();
        }

        // The numbers of an attribute that lists them, or none when the element lacks it.
        // element: the element.
        // name: the attribute's name.
        private static double[] ReadList(XElement element, string name)
        {
            string text = (string)element.Attribute(name);
            return text == null ? Array.Empty<double>() : ParseNumbers(text);
        }

        // The length an attribute gives, or 0 when the element lacks it, which is SVG's default for the lengths read here.
        // element: the element.
        // name: the attribute's name.
        private static double ReadLength(XElement element, string name)
        {
            string text = (string)element.Attribute(name);
            return text == null ? 0 : ParseLength(text);
        }

        // A length, with or without a unit of px.
        // text: the length as written.
        private static double ParseLength(string text)
        {
            string number = text.Trim();
            if (number.EndsWith("px", StringComparison.Ordinal))
                number = number.Substring(0, number.Length - 2);
            return double.Parse(number, NumberStyles.Float, CultureInfo.InvariantCulture);
        }

        // A transform attribute's transform: its functions applied right to left, or the identity when there is none.
        // text: the attribute's value, or null.
        public static Affine ParseTransform(string text)
        {
            Affine transform = Affine.Identity;
            if (string.IsNullOrWhiteSpace(text))
                return transform;
            int position = 0;
            while (position < text.Length && text.Substring(position).Trim().Length > 0) {
                Match function = TransformFunction.Match(text, position);
                if (!function.Success)
                    throw new FormatException("a transform that cannot be read: " + text);
                position += function.Length;
                double[] arguments = ParseNumbers(function.Groups[2].Value);
                Affine step;
                switch (function.Groups[1].Value) {
                    case "matrix":
                        step = new Affine(arguments[0], arguments[1], arguments[2], arguments[3], arguments[4], arguments[5]);
                        break;
                    case "translate":
                        step = new Affine(1, 0, 0, 1, arguments[0], arguments.Length > 1 ? arguments[1] : 0);
                        break;
                    case "scale":
                        step = new Affine(arguments[0], 0, 0, arguments.Length > 1 ? arguments[1] : arguments[0], 0, 0);
                        break;
                    case "rotate":
                        step = Affine.Rotation(arguments[0]);
                        if (arguments.Length == 3)
                            step = new Affine(1, 0, 0, 1, arguments[1], arguments[2]).After(step).After(new Affine(1, 0, 0, 1, -arguments[1], -arguments[2]));
                        break;
                    default:
                        throw new FormatException("a transform function that cannot be read: " + text);
                }
                transform = transform.After(step);
            }
            return transform;
        }

        // The frame within an element under its transform attribute, if it has one, as a group or a use carries one.
        // element: the element.
        // frame: the frame the element is written in.
        private static Frame WithinTransform(XElement element, Frame frame)
        {
            string transform = (string)element.Attribute("transform");
            return transform == null ? frame : frame.Within(ParseTransform(transform));
        }

        // The element an attribute refers to by #id.
        // referrer: the element that refers, for messages, or null for a style.
        // reference: the reference.
        private XElement Referred(XElement referrer, string reference)
        {
            XElement element;
            if (reference == null || !reference.StartsWith("#", StringComparison.Ordinal) || !elementsById.TryGetValue(reference.Substring(1), out element))
                throw new FormatException((referrer == null ? "a style" : Describe(referrer, null)) + " refers to " + reference + ", which nothing is");
            return element;
        }

        // An element and its place in the document, for messages.
        // element: the element.
        // objectNumber: the number of its object, or null.
        private static string Describe(XElement element, string objectNumber)
        {
            IXmlLineInfo line = element;
            return element.Name.LocalName + (line.HasLineInfo() ? " at line " + line.LineNumber.ToString(CultureInfo.InvariantCulture) : "") +
                   (objectNumber == null ? "" : " of object " + objectNumber);
        }
    }
}
