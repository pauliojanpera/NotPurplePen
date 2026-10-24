using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using PurplePen.Graphics2D;

using PointF = System.Drawing.PointF;
using RectangleF = System.Drawing.RectangleF;
using SizeF = System.Drawing.SizeF;

namespace PurplePen.MapModel
{
    // Writes what is drawn into it as an SVG document of maps and pages (L-1 to L-5). Drawing happens within parts,
    // which the document defines once, and pages show the parts through a clip and a transform onto their paper. The
    // coordinates and widths of a part are whole steps of 0.001 mm of map (S-1). A call that the target cannot honor
    // raises an Svg_UnsupportedCallException naming it, and a call out of turn an InvalidOperationException naming it
    // (D-12).
    public class Svg_GraphicsTarget : IStructuredGraphicsTarget
    {
        // The calls the target fully implements, each to its whole requirement (DL-5).
        private static readonly HashSet<string> fullyImplementedCalls = new HashSet<string>(StringComparer.Ordinal) {
            nameof(Intensity), nameof(PushTransform), nameof(PopTransform), nameof(PushClip), nameof(PopClip),
            nameof(PushAntiAliasing), nameof(PopAntiAliasing), nameof(PushBlending), nameof(PopBlending),
            nameof(CreatePath), nameof(CreateSolidBrush), nameof(SupportsPatternBrushes), nameof(CreatePatternBrush), nameof(CreatePen),
            nameof(HasPath), nameof(HasBrush), nameof(HasPen), nameof(HasFont),
            nameof(DrawLine), nameof(DrawArc), nameof(DrawEllipse), nameof(FillEllipse), nameof(DrawRectangle),
            nameof(FillRectangle), nameof(DrawPolygon), nameof(DrawPolyline), nameof(FillPolygon), nameof(DrawPath),
            nameof(FillPath), nameof(DrawBitmap), nameof(DrawBitmapPart),
        };

        // What opened a group that is still open, which the call that closes it must match. The bottom group is a
        // part's, or in the target of a pattern brush, the tile's. A symbol's group is opened by the first of its
        // consecutive objects rather than by a call of its own, and closes with the group that holds it.
        private enum Opener { BeginPart, CreatePatternBrush, BeginMap, BeginColorRun, BeginObject, PushTransform, PushClip, PushAntiAliasing, PushBlending }

        // A group still open: what opened it, the anti-aliasing in force before it, and the record of what it holds,
        // if any: the map of a map's group, the color of a run's, or the symbol of a symbol's.
        private sealed class OpenGroup
        {
            public readonly Opener Opener;
            public readonly SvgGroup Group;
            public readonly bool AntiAliasBefore;
            public readonly object Record;

            public OpenGroup(Opener opener, SvgGroup group, bool antiAliasBefore, object record)
            {
                Opener = opener;
                Group = group;
                AntiAliasBefore = antiAliasBefore;
                Record = record;
            }
        }

        // A path created under a key, with the rule it is filled and clipped under.
        private sealed class StoredPath
        {
            public readonly SvgPathData Data;
            public readonly bool EvenOdd;

            public StoredPath(SvgPathData data, bool evenOdd)
            {
                Data = data;
                EvenOdd = evenOdd;
            }
        }

        // A font created under a key: the face it draws in, its size, the application's metrics of it, and the
        // declarations that set it in a text's class (T-1).
        private sealed class FontRecord
        {
            // The face, which records the characters drawn in it.
            public readonly SvgFontFace Face;

            // The application's metrics of the font, which lay its text out.
            public readonly ITextFaceMetrics Metrics;

            // The declarations of the font: its size in steps, family, weight and style. They are written as longhands,
            // since the shorthand font would reset the kerning and ligatures that the rule of every text element sets.
            public readonly string Declarations;

            // The advance of each character drawn alone, in millimetres, by the character.
            private readonly Dictionary<string, float> advances = new Dictionary<string, float>(StringComparer.Ordinal);

            // Create the record of a font.
            // face: the face it draws in.
            // emHeight: its size, in millimetres.
            // metrics: the application's metrics of it.
            public FontRecord(SvgFontFace face, float emHeight, ITextFaceMetrics metrics)
            {
                Face = face;
                Metrics = metrics;
                Declarations = "font-size:" + SvgNumbers.FormatInteger(SvgNumbers.ToSteps(emHeight)) + "px;font-family:" + face.QuotedFamily +
                               (face.Bold ? ";font-weight:700" : "") + (face.Italic ? ";font-style:italic" : "");
            }

            // The kerning before each character of text after the first, in steps, without the zeros that end it: how far
            // the application's shaping moves the character from where the advances of the characters before it, each
            // drawn alone, would put it. That is the width of the text up to and with the character, less the width up
            // to the character before and the character's own advance.
            // text: the text, whose surrogate pairs each make one character.
            public long[] Kerning(string text)
            {
                List<long> kerning = new List<long>();
                int last = -1;
                int start = char.IsSurrogatePair(text, 0) ? 2 : 1;
                float widthBefore = Metrics.GetTextWidth(text.Substring(0, start));
                while (start < text.Length) {
                    int length = char.IsSurrogatePair(text, start) ? 2 : 1;
                    float width = Metrics.GetTextWidth(text.Substring(0, start + length));
                    long shift = SvgNumbers.ToSteps(width - widthBefore - Advance(text.Substring(start, length)));
                    kerning.Add(shift);
                    if (shift != 0)
                        last = kerning.Count - 1;
                    widthBefore = width;
                    start += length;
                }
                kerning.RemoveRange(last + 1, kerning.Count - last - 1);
                return kerning.ToArray();
            }

            // The advance of a character drawn alone, in millimetres.
            // character: the character, a surrogate pair for a character beyond the Basic Multilingual Plane.
            private float Advance(string character)
            {
                float advance;
                if (!advances.TryGetValue(character, out advance)) {
                    advance = Metrics.GetTextWidth(character);
                    advances.Add(character, advance);
                }
                return advance;
            }
        }

        // The target that draws a pattern brush's tile, as the target that created the brush draws, but with keys of its
        // own; FinishBrush makes the tile a brush of the target that created it (D-7).
        private sealed class PatternBrushTarget : Svg_GraphicsTarget, IBrushTarget
        {
            // The target that created the brush.
            private readonly Svg_GraphicsTarget creator;

            // The pattern whose tile is drawn.
            private readonly SvgPattern pattern;

            // Create the target of a pattern brush's tile.
            // creator: the target that creates the brush.
            // pattern: the pattern whose tile it draws.
            public PatternBrushTarget(Svg_GraphicsTarget creator, SvgPattern pattern)
                : base(creator, pattern)
            {
                this.creator = creator;
                this.pattern = pattern;
            }

            // Make the tile a brush of the target that created it, under a key, closing every group still open in the
            // tile; the tile then takes no more drawing.
            public void FinishBrush(object brushKey)
            {
                CloseThrough(Opener.CreatePatternBrush, nameof(FinishBrush));
                creator.AddBrush(brushKey, new SvgBrush(pattern), nameof(FinishBrush));
            }
        }

        // The document being written.
        private readonly SvgDocument document;

        // Whether the target draws a pattern brush's tile, which holds drawing alone, rather than a document of parts,
        // maps and pages.
        private readonly bool drawsPatternTile;

        // The groups still open, the innermost on top; the bottom one is the part's while a part is open, or the tile's
        // until a pattern brush is finished.
        private readonly Stack<OpenGroup> openGroups = new Stack<OpenGroup>();

        // The key of the object begun and not yet ended, or null for none.
        private object openObjectKey;

        // The parts and the maps, by their keys.
        private readonly Dictionary<object, SvgPart> partsByKey = new Dictionary<object, SvgPart>(new IdentityComparer<object>());
        private readonly Dictionary<object, SvgMapRecord> mapsByKey = new Dictionary<object, SvgMapRecord>(new IdentityComparer<object>());

        // The brushes, the style each pen strokes with, the paths and the fonts, by their keys (D-10).
        private readonly Dictionary<object, SvgBrush> brushes = new Dictionary<object, SvgBrush>(new IdentityComparer<object>());
        private readonly Dictionary<object, SvgStyle> pens = new Dictionary<object, SvgStyle>(new IdentityComparer<object>());
        private readonly Dictionary<object, StoredPath> paths = new Dictionary<object, StoredPath>(new IdentityComparer<object>());
        private readonly Dictionary<object, FontRecord> fonts = new Dictionary<object, FontRecord>(new IdentityComparer<object>());

        // The metrics the application lays text out with, or null when the target draws no text (T-1).
        private readonly ITextMetrics textMetrics;

        // How the bitmaps drawn into the document reach it (B-3, B-4).
        private readonly SvgBitmapOptions bitmapOptions;

        // The intensity that colors are drawn at.
        private float intensity = 1;

        // Whether drawing is anti-aliased, as a browser draws it unless told otherwise.
        private bool antiAlias = true;

        // Create a target that writes a document.
        // colorConverter: converts colors to sRGB, as the chosen color model does (COL-2).
        // declaresCmykColors: whether colors also carry their CMYK values, as under the CMYK color model (COL-1).
        public Svg_GraphicsTarget(IColorConverter colorConverter, bool declaresCmykColors)
            : this(colorConverter, declaresCmykColors, SvgBitmapOptions.Default)
        {
        }

        // Create a target that writes a document, with the bitmaps drawn into it written as the options say (B-3, B-4),
        // and draws no text.
        // colorConverter: converts colors to sRGB, as the chosen color model does (COL-2).
        // declaresCmykColors: whether colors also carry their CMYK values, as under the CMYK color model (COL-1).
        // bitmapOptions: how bitmaps reach the document.
        public Svg_GraphicsTarget(IColorConverter colorConverter, bool declaresCmykColors, SvgBitmapOptions bitmapOptions)
            : this(colorConverter, declaresCmykColors, bitmapOptions, null, null)
        {
        }

        // Create a target that writes a document, with the bitmaps drawn into it written as the options say (B-3, B-4),
        // and its text laid out with the application's metrics, in the faces the font data embeds (T-1, T-6, T-7).
        // colorConverter: converts colors to sRGB, as the chosen color model does (COL-2).
        // declaresCmykColors: whether colors also carry their CMYK values, as under the CMYK color model (COL-1).
        // bitmapOptions: how bitmaps reach the document.
        // textMetrics: the metrics the application lays text out with, or null to draw no text.
        // fontSubsets: what gives the font data of the faces text is drawn in, or null to embed no fonts.
        public Svg_GraphicsTarget(IColorConverter colorConverter, bool declaresCmykColors, SvgBitmapOptions bitmapOptions,
                                  ITextMetrics textMetrics, IFontSubsetProvider fontSubsets)
        {
            document = new SvgDocument(new SvgPaints(colorConverter, declaresCmykColors), fontSubsets);
            this.bitmapOptions = bitmapOptions;
            this.textMetrics = textMetrics;
        }

        // Create the target of a pattern brush's tile, which writes into the same document at the intensity of the target
        // that creates the brush, with its bitmap options, and draws anti-aliased until told otherwise.
        // creator: the target that creates the brush.
        // pattern: the pattern whose tile it draws.
        private Svg_GraphicsTarget(Svg_GraphicsTarget creator, SvgPattern pattern)
        {
            document = creator.document;
            bitmapOptions = creator.bitmapOptions;
            textMetrics = creator.textMetrics;
            intensity = creator.intensity;
            drawsPatternTile = true;
            openGroups.Push(new OpenGroup(Opener.CreatePatternBrush, pattern.Content, antiAlias, null));
        }

        // Whether the target fully implements an IGraphicsTarget call, so that a drawing which makes it can be
        // compared with its reference image.
        // callName: the name of the IGraphicsTarget member.
        public static bool FullyImplements(string callName)
        {
            return fullyImplementedCalls.Contains(callName);
        }

        // Finish the document, closing every group still open, and return its text.
        public string FinishDocument()
        {
            RequireDocumentTarget(nameof(FinishDocument));
            openGroups.Clear();
            openObjectKey = null;
            return document.Write();
        }

        // Begin a part of a drawing; parts do not nest.
        public void BeginPart(object partKey)
        {
            RequireDocumentTarget(nameof(BeginPart));
            if (openGroups.Count > 0)
                throw new InvalidOperationException("BeginPart: a part is open already; parts do not nest.");
            if (partsByKey.ContainsKey(partKey))
                throw new InvalidOperationException("BeginPart: a part with this key has begun already.");

            SvgPart part = new SvgPart();
            partsByKey.Add(partKey, part);
            document.Parts.Add(part);
            antiAlias = true;
            openGroups.Push(new OpenGroup(Opener.BeginPart, part.Content, antiAlias, null));
        }

        // End the part begun last, closing every group still open within it.
        public void EndPart()
        {
            RequireDocumentTarget(nameof(EndPart));
            CloseThrough(Opener.BeginPart, nameof(EndPart));
        }

        // Begin a labeled map within the part or map that is open (M-5).
        public void BeginMap(object mapKey, string label)
        {
            RequireDocumentTarget(nameof(BeginMap));
            RequireOpenPart(nameof(BeginMap));

            SvgMapRecord map;
            if (!mapsByKey.TryGetValue(mapKey, out map)) {
                map = new SvgMapRecord(document.Maps.Count + 1, label);
                mapsByKey.Add(mapKey, map);
                document.Maps.Add(map);
            }
            Open(Opener.BeginMap, new SvgGroup { Identity = "data-m=\"" + SvgNumbers.FormatInteger(map.Number) + "\"" }, map);
        }

        // End the map begun last, closing every group still open within it.
        public void EndMap()
        {
            RequireDocumentTarget(nameof(EndMap));
            CloseThrough(Opener.BeginMap, nameof(EndMap));
        }

        // Begin a run of a color of the map that is open, outside any other run, and record the color in the map's
        // color table. The run's group carries the color's position in the table (M-3, M-7).
        public void BeginColorRun(object colorKey, int position, string name, CmykColor color, bool overprint)
        {
            RequireDocumentTarget(nameof(BeginColorRun));
            OpenGroup map = FindOpenMap(nameof(BeginColorRun));
            foreach (OpenGroup openGroup in openGroups) {
                if (openGroup.Opener == Opener.BeginColorRun)
                    throw new InvalidOperationException("BeginColorRun: a color run is open already; runs do not nest.");
            }
            CloseSymbolGroup(nameof(BeginColorRun));

            SvgColorRecord colorRecord = ((SvgMapRecord)map.Record).RecordColor(colorKey, position, name, color, overprint);
            Open(Opener.BeginColorRun, new SvgGroup { Identity = "data-c=\"" + SvgNumbers.FormatInteger(colorRecord.Position) + "\"" }, colorRecord);
        }

        // End the color run begun last, closing every group still open within it.
        public void EndColorRun()
        {
            RequireDocumentTarget(nameof(EndColorRun));
            CloseThrough(Opener.BeginColorRun, nameof(EndColorRun));
        }

        // Begin an object of the map that is open, and record its symbol in the map's symbol table, with the color of
        // the run it is drawn in, if any. Consecutive objects of one symbol share a group, which carries the symbol's
        // number, and every element drawn until EndObject carries the object's (M-4, M-6, M-7).
        public void BeginObject(object objectKey, object symbolKey, string symbolId, string symbolName)
        {
            RequireDocumentTarget(nameof(BeginObject));
            if (openObjectKey != null)
                throw new InvalidOperationException("BeginObject: an object is open already; objects do not nest.");
            OpenGroup map = FindOpenMap(nameof(BeginObject));

            SvgSymbolRecord symbol = ((SvgMapRecord)map.Record).RecordSymbol(symbolKey, symbolId, symbolName);
            foreach (OpenGroup openGroup in openGroups) {
                if (openGroup == map)
                    break;
                if (openGroup.Opener == Opener.BeginColorRun)
                    symbol.AddColor(((SvgColorRecord)openGroup.Record).Position);
            }

            if (openGroups.Peek().Record != symbol) {
                CloseSymbolGroup(nameof(BeginObject));
                Open(Opener.BeginObject, new SvgGroup { Identity = "data-s=\"" + SvgNumbers.FormatInteger(symbol.Number) + "\"" }, symbol);
            }
            openObjectKey = objectKey;
        }

        // End the object begun last, whose drawing must have closed every group it opened. Its symbol's group stays
        // open for the next object of the symbol.
        public void EndObject()
        {
            RequireDocumentTarget(nameof(EndObject));
            if (openObjectKey == null)
                throw new InvalidOperationException("EndObject: no object is open.");
            if (openGroups.Peek().Opener != Opener.BeginObject)
                throw new InvalidOperationException("EndObject: the object's drawing left a group opened by " + openGroups.Peek().Opener + " open.");
            openObjectKey = null;
        }

        // Add a page that shows parts within an area of the map, transformed onto its paper (L-3, F-4).
        public void AddPage(SizeF paperSize, RectangleF mapArea, Matrix mapToPage, object[] partKeys)
        {
            RequireDocumentTarget(nameof(AddPage));
            if (openGroups.Count > 0)
                throw new InvalidOperationException("AddPage: a part is open; pages are added outside parts.");

            List<SvgPart> parts = new List<SvgPart>();
            foreach (object partKey in partKeys) {
                SvgPart part;
                if (!partsByKey.TryGetValue(partKey, out part))
                    throw new InvalidOperationException("AddPage: no part has begun with one of the keys given.");
                parts.Add(part);
            }

            SvgPathData clipPath = RectanglePath(mapArea);
            SvgTransform stepsToMillimetres = new SvgTransform(1 / SvgNumbers.StepsPerMillimetre, 0, 0, 1 / SvgNumbers.StepsPerMillimetre, 0, 0);
            SvgTransform stepsToPage = SvgTransform.FromMatrix(mapToPage, 1).After(stepsToMillimetres);
            document.AddPage(paperSize.Width, paperSize.Height, stepsToPage, document.ClipOf(clipPath.ToString(), false), clipPath.Bounds, parts);
        }

        // Record where a map that has begun lies in the real world, which its record in the metadata then holds; a later
        // call for the same map replaces it (G-1, G-2).
        public void SetGeoreference(object mapKey, MapGeoreference georeference)
        {
            RequireDocumentTarget(nameof(SetGeoreference));
            SvgMapRecord map;
            if (!mapsByKey.TryGetValue(mapKey, out map))
                throw new InvalidOperationException("SetGeoreference: no map has begun with this key.");
            map.Georeference = georeference;
        }

        // The intensity at which colors are drawn; setting it discards the brushes and pens, whose colors it changed.
        public float Intensity
        {
            get { return intensity; }
            set {
                brushes.Clear();
                pens.Clear();
                intensity = value;
            }
        }

        // Open a group whose transform is prepended to the current one (D-1).
        public void PushTransform(Matrix matrix)
        {
            RequireOpenPart(nameof(PushTransform));
            Open(Opener.PushTransform, new SvgGroup { Transform = matrix.IsIdentity ? null : SvgTransform.FromMatrix(matrix, SvgNumbers.StepsPerMillimetre) });
        }

        // Close the group of the last PushTransform.
        public void PopTransform()
        {
            Close(Opener.PushTransform, nameof(PopTransform));
        }

        // Clip to a path created with CreatePath, under its rule; nested clips intersect (D-2).
        public void PushClip(object pathKey)
        {
            StoredPath path = GetPath(pathKey, nameof(PushClip));
            OpenClip(path.Data, path.EvenOdd, nameof(PushClip));
        }

        // Clip to a rectangle (D-2).
        public void PushClip(RectangleF rectangle)
        {
            OpenClip(RectanglePath(rectangle), false, nameof(PushClip));
        }

        // Clip to the union of rectangles: one subpath each, all turning the same way, under the nonzero rule (D-2).
        public void PushClip(RectangleF[] rectangles)
        {
            SvgPathData path = new SvgPathData();
            foreach (RectangleF rectangle in rectangles)
                AddRectangle(path, rectangle);
            OpenClip(path, false, nameof(PushClip));
        }

        // Clip to a path given by its parts, under the rule of its fill mode (D-2).
        public void PushClip(List<GraphicsPathPart> parts, AreaFillMode windingMode)
        {
            OpenClip(PathOfParts(parts), windingMode == AreaFillMode.Alternate, nameof(PushClip));
        }

        // End the clip of the last PushClip.
        public void PopClip()
        {
            Close(Opener.PushClip, nameof(PopClip));
        }

        // Draw anti-aliased, or with crisp edges, until the matching PopAntiAliasing (D-9). A group carries the change
        // only where the mode changes, since the property is inherited.
        public void PushAntiAliasing(bool antiAlias)
        {
            RequireOpenPart(nameof(PushAntiAliasing));
            string style = null;
            if (antiAlias != this.antiAlias)
                style = antiAlias ? "shape-rendering:auto" : "shape-rendering:crispEdges";
            Open(Opener.PushAntiAliasing, new SvgGroup { Style = style });
            this.antiAlias = antiAlias;
        }

        // End the anti-aliasing mode of the last PushAntiAliasing.
        public void PopAntiAliasing()
        {
            Close(Opener.PushAntiAliasing, nameof(PopAntiAliasing));
        }

        // Blend what is drawn until the matching PopBlending with what lies below it, by darken, and return true (D-8).
        // The map opens the blending around the run of one overprinted color, so a group blends that run as a whole.
        public bool PushBlending(BlendMode blendMode)
        {
            RequireOpenPart(nameof(PushBlending));
            if (blendMode != BlendMode.Darken)
                throw new Svg_UnsupportedCallException(nameof(PushBlending));
            Open(Opener.PushBlending, new SvgGroup { Style = "mix-blend-mode:darken", Blends = true });
            return true;
        }

        // End the blending of the last PushBlending.
        public void PopBlending()
        {
            Close(Opener.PushBlending, nameof(PopBlending));
        }

        // Create a path under a key, filled and clipped under the rule of its fill mode (D-3, D-10).
        public void CreatePath(object key, List<GraphicsPathPart> parts, AreaFillMode windingMode)
        {
            if (paths.ContainsKey(key))
                throw new InvalidOperationException("CreatePath: the key has a path already.");
            paths.Add(key, new StoredPath(PathOfParts(parts), windingMode == AreaFillMode.Alternate));
        }

        // Create a solid brush under a key, its color at the current intensity (D-10, D-11).
        public void CreateSolidBrush(object key, CmykColor color)
        {
            AddBrush(key, new SvgBrush(AtIntensity(color)), nameof(CreateSolidBrush));
        }

        // Pattern brushes are supported, as patterns of vectors (D-7).
        public bool SupportsPatternBrushes
        {
            get { return true; }
        }

        // Create a pattern brush: return the target that draws its tile, whose middle is the origin of the tile's
        // coordinates, and whose FinishBrush makes it a brush of this target that repeats the tile across the user space
        // of what it paints, turned by an angle (D-7). The size of the bitmap, which a raster target draws the tile into,
        // is of no use to vectors.
        // size: the size of the tile.
        // angle: the angle the pattern is turned by, in degrees from the x axis towards the y axis.
        public IBrushTarget CreatePatternBrush(SizeF size, float angle, int bitmapWidth, int bitmapHeight)
        {
            float width = (float)(size.Width * SvgNumbers.StepsPerMillimetre), height = (float)(size.Height * SvgNumbers.StepsPerMillimetre);
            return new PatternBrushTarget(this, new SvgPattern(width, height, angle));
        }

        // Create a pen that strokes with a brush's paint, a color or a pattern, under a key (D-5, D-6).
        public void CreatePen(object key, object brushKey, float width, LineCapMode caps, LineJoinMode join, float miterLimit)
        {
            SvgBrush brush = GetBrush(brushKey, nameof(CreatePen));
            AddPen(key, brush.Paint(document.Paints, "stroke"), width, caps, join, miterLimit);
        }

        // Create a pen of a color at the current intensity, under a key (D-5, D-11).
        public void CreatePen(object key, CmykColor color, float width, LineCapMode caps, LineJoinMode join, float miterLimit)
        {
            AddPen(key, new SvgStyle(document.Paints.Paint("stroke", AtIntensity(color))), width, caps, join, miterLimit);
        }

        // Create a font under a key: its face, its size and its metrics, which lay out the text drawn in it (T-1).
        public void CreateFont(object key, string familyName, float emHeight, TextEffects effects)
        {
            if (textMetrics == null)
                throw new Svg_UnsupportedCallException(nameof(CreateFont));
            if (fonts.ContainsKey(key))
                throw new InvalidOperationException("CreateFont: the key has a font already.");
            fonts.Add(key, new FontRecord(document.FontFaceOf(familyName, effects), emHeight, textMetrics.GetTextFaceMetrics(familyName, emHeight, effects)));
        }

        // Whether a path exists under a key (D-10).
        public bool HasPath(object pathKey)
        {
            return paths.ContainsKey(pathKey);
        }

        // Whether a brush exists under a key (D-10).
        public bool HasBrush(object brushKey)
        {
            return brushes.ContainsKey(brushKey);
        }

        // Whether a pen exists under a key (D-10).
        public bool HasPen(object penKey)
        {
            return pens.ContainsKey(penKey);
        }

        // Whether a font exists under a key (D-10).
        public bool HasFont(object fontKey)
        {
            return fonts.ContainsKey(fontKey);
        }

        // Draw a line with a pen, as a path (D-4, F-3).
        public void DrawLine(object penKey, PointF start, PointF finish)
        {
            SvgStyle stroke = GetPen(penKey, nameof(DrawLine));
            SvgPathData path = new SvgPathData();
            path.MoveTo(SvgNumbers.ToSteps(start.X), SvgNumbers.ToSteps(start.Y));
            path.LineTo(SvgNumbers.ToSteps(finish.X), SvgNumbers.ToSteps(finish.Y));
            AddPath(stroke, path, nameof(DrawLine));
        }

        // Draw an arc with a pen. Its angles are degrees turning from the x axis towards the y axis, as Skia's are, and
        // an arc of a full turn or more is the whole circle, which Skia then draws as a closed oval (D-4).
        public void DrawArc(object penKey, PointF center, float radius, float startAngle, float sweepAngle)
        {
            SvgStyle stroke = GetPen(penKey, nameof(DrawArc));
            if (Math.Abs(sweepAngle) >= 360) {
                AddElement(EllipseShape(stroke, center, radius, radius), nameof(DrawArc));
                return;
            }

            double start = startAngle * Math.PI / 180, end = (startAngle + sweepAngle) * Math.PI / 180;
            long radiusInSteps = Math.Abs(SvgNumbers.ToSteps(radius));
            SvgPathData path = new SvgPathData();
            path.MoveTo(SvgNumbers.ToSteps(center.X + radius * Math.Cos(start)), SvgNumbers.ToSteps(center.Y + radius * Math.Sin(start)));
            path.ArcTo(radiusInSteps, radiusInSteps, Math.Abs(sweepAngle) > 180, sweepAngle > 0,
                       SvgNumbers.ToSteps(center.X + radius * Math.Cos(end)), SvgNumbers.ToSteps(center.Y + radius * Math.Sin(end)));
            long centerX = SvgNumbers.ToSteps(center.X), centerY = SvgNumbers.ToSteps(center.Y);
            path.Bounds.Include(centerX - radiusInSteps, centerY - radiusInSteps);
            path.Bounds.Include(centerX + radiusInSteps, centerY + radiusInSteps);
            AddPath(stroke, path, nameof(DrawArc));
        }

        // Draw an ellipse with a pen, as a circle when its radii are equal (D-4).
        public void DrawEllipse(object penKey, PointF center, float radiusX, float radiusY)
        {
            SvgStyle stroke = GetPen(penKey, nameof(DrawEllipse));
            AddElement(EllipseShape(stroke, center, radiusX, radiusY), nameof(DrawEllipse));
        }

        // Fill an ellipse with a brush, as a circle when its radii are equal (D-4).
        public void FillEllipse(object brushKey, PointF center, float radiusX, float radiusY)
        {
            SvgStyle fill = GetFill(brushKey, nameof(FillEllipse));
            AddElement(EllipseShape(fill, center, radiusX, radiusY), nameof(FillEllipse));
        }

        // Draw a rectangle with a pen, as a closed path (D-4).
        public void DrawRectangle(object penKey, RectangleF rect)
        {
            SvgStyle stroke = GetPen(penKey, nameof(DrawRectangle));
            AddPath(stroke, RectanglePath(rect), nameof(DrawRectangle));
        }

        // Fill a rectangle with a brush, as a path (D-4, F-3).
        public void FillRectangle(object brushKey, RectangleF rect)
        {
            SvgStyle fill = GetFill(brushKey, nameof(FillRectangle));
            AddPath(fill, RectanglePath(rect), nameof(FillRectangle));
        }

        // Draw a closed polygon with a pen (D-4).
        public void DrawPolygon(object penKey, PointF[] pts)
        {
            SvgStyle stroke = GetPen(penKey, nameof(DrawPolygon));
            AddPath(stroke, PathThrough(pts, true), nameof(DrawPolygon));
        }

        // Draw an open polyline with a pen (D-4).
        public void DrawPolyline(object penKey, PointF[] pts)
        {
            SvgStyle stroke = GetPen(penKey, nameof(DrawPolyline));
            AddPath(stroke, PathThrough(pts, false), nameof(DrawPolyline));
        }

        // Fill a polygon with a brush, under the even-odd rule in the alternate mode and the nonzero rule in the winding
        // mode (D-3).
        public void FillPolygon(object brushKey, PointF[] pts, AreaFillMode windingMode)
        {
            SvgStyle fill = GetFill(brushKey, nameof(FillPolygon));
            AddPath(FillUnderRule(fill, windingMode == AreaFillMode.Alternate), PathThrough(pts, true), nameof(FillPolygon));
        }

        // Draw a path created with CreatePath with a pen (D-3).
        public void DrawPath(object penKey, object pathKey)
        {
            SvgStyle stroke = GetPen(penKey, nameof(DrawPath));
            AddPath(stroke, GetPath(pathKey, nameof(DrawPath)).Data, nameof(DrawPath));
        }

        // Draw a path given by its parts with a pen (D-3).
        public void DrawPath(object penKey, List<GraphicsPathPart> parts)
        {
            SvgStyle stroke = GetPen(penKey, nameof(DrawPath));
            AddPath(stroke, PathOfParts(parts), nameof(DrawPath));
        }

        // Fill a path created with CreatePath with a brush, under the path's rule (D-3).
        public void FillPath(object brushKey, object pathKey)
        {
            SvgStyle fill = GetFill(brushKey, nameof(FillPath));
            StoredPath path = GetPath(pathKey, nameof(FillPath));
            AddPath(FillUnderRule(fill, path.EvenOdd), path.Data, nameof(FillPath));
        }

        // Fill a path given by its parts with a brush, under the rule of its fill mode (D-3).
        public void FillPath(object brushKey, List<GraphicsPathPart> parts, AreaFillMode windingMode)
        {
            SvgStyle fill = GetFill(brushKey, nameof(FillPath));
            AddPath(FillUnderRule(fill, windingMode == AreaFillMode.Alternate), PathOfParts(parts), nameof(FillPath));
        }

        // Draw text with its upper left corner at a point, filled with a brush (T-1, T-3).
        public void DrawText(string text, object fontKey, object brushKey, PointF upperLeft)
        {
            if (textMetrics == null)
                throw new Svg_UnsupportedCallException(nameof(DrawText));
            AddText(GetFont(fontKey, nameof(DrawText)), GetFill(brushKey, nameof(DrawText)), text, upperLeft, nameof(DrawText));
        }

        // Draw the outline of text with its upper left corner at a point, stroked with a pen and not filled (T-2).
        public void DrawTextOutline(string text, object fontKey, object penKey, PointF upperLeft)
        {
            if (textMetrics == null)
                throw new Svg_UnsupportedCallException(nameof(DrawTextOutline));
            AddText(GetFont(fontKey, nameof(DrawTextOutline)), GetPen(penKey, nameof(DrawTextOutline)), text, upperLeft, nameof(DrawTextOutline));
        }

        // Draw a bitmap over a rectangle, its first pixel at the rectangle's left and top, as the Skia target draws it
        // (B-1); pixelated for nearest-neighbor scaling, and smooth otherwise (B-2).
        public void DrawBitmap(IGraphicsBitmap bm, RectangleF rectangle, BitmapScaling scalingMode)
        {
            AddImage(bm, rectangle, scalingMode, nameof(DrawBitmap));
        }

        // Draw part of a bitmap over a rectangle, as the bitmap cropped to the part (B-1).
        public void DrawBitmapPart(IGraphicsBitmap bm, int x, int y, int width, int height, RectangleF rectangle, BitmapScaling scalingMode)
        {
            RequireImageDrawing(nameof(DrawBitmapPart));
            IGraphicsBitmap part = bm.Crop(x, y, width, height);
            try {
                AddImage(part, rectangle, scalingMode, nameof(DrawBitmapPart));
            }
            finally {
                if (!ReferenceEquals(part, bm))
                    part.Dispose();
            }
        }

        // Release the metrics of the fonts, which the application may hold font data for.
        public void Dispose()
        {
            foreach (FontRecord font in fonts.Values)
                font.Metrics.Dispose();
            fonts.Clear();
        }

        // Fail a call unless a part is open, as drawing happens only within one, or in the target of a pattern brush,
        // unless the brush is still being drawn.
        // callName: the name of the call.
        private void RequireOpenPart(string callName)
        {
            if (openGroups.Count == 0) {
                throw new InvalidOperationException(callName + (drawsPatternTile ? ": the pattern brush is finished, and its tile takes no more drawing."
                                                                                 : ": no part is open; drawing happens between BeginPart and EndPart."));
            }
        }

        // Fail a call that structures the document when the target draws a pattern brush's tile, which holds drawing
        // alone.
        // callName: the name of the call.
        private void RequireDocumentTarget(string callName)
        {
            if (drawsPatternTile)
                throw new InvalidOperationException(callName + ": a pattern brush's tile holds drawing alone, without parts, maps or pages.");
        }

        // Open a group within the innermost open one.
        // opener: what opens it.
        // group: the group.
        private void Open(Opener opener, SvgGroup group)
        {
            Open(opener, group, null);
        }

        // Open a group within the innermost open one, with the record of what it holds.
        // opener: what opens it.
        // group: the group.
        // record: the record of the map, color or symbol it holds, or null for none.
        private void Open(Opener opener, SvgGroup group, object record)
        {
            openGroups.Peek().Group.Children.Add(group);
            openGroups.Push(new OpenGroup(opener, group, antiAlias, record));
        }

        // The group of the innermost map that is open.
        // callName: the name of the call that needs it.
        private OpenGroup FindOpenMap(string callName)
        {
            foreach (OpenGroup openGroup in openGroups) {
                if (openGroup.Opener == Opener.BeginMap)
                    return openGroup;
            }
            throw new InvalidOperationException(callName + ": no map is open; runs and objects belong to a map.");
        }

        // Close the group of a symbol when it is the innermost open group: a call that closes or begins something
        // around it ends the symbol's consecutive objects, once the last of them has ended.
        // callName: the name of that call.
        private void CloseSymbolGroup(string callName)
        {
            if (openGroups.Count == 0 || openGroups.Peek().Opener != Opener.BeginObject)
                return;
            if (openObjectKey != null)
                throw new InvalidOperationException(callName + ": the object begun last has not ended.");
            openGroups.Pop();
        }

        // Open a group that clips what is drawn within it to a path, in the current frame (D-2).
        // path: the path.
        // evenOdd: whether the path clips under the even-odd rule rather than the nonzero one.
        // callName: the name of the call that clips.
        private void OpenClip(SvgPathData path, bool evenOdd, string callName)
        {
            RequireOpenPart(callName);
            Open(Opener.PushClip, new SvgGroup { Clip = document.ClipOf(path.ToString(), evenOdd) });
        }

        // Close the innermost open group, which the closing call must match.
        // opener: what the group must have been opened by.
        // callName: the name of the closing call.
        private void Close(Opener opener, string callName)
        {
            CloseSymbolGroup(callName);
            if (openGroups.Count == 0 || openGroups.Peek().Opener != opener) {
                string innermost = openGroups.Count == 0 ? "nothing" : "a group opened by " + openGroups.Peek().Opener;
                throw new InvalidOperationException(callName + " closes a group opened by " + opener + ", but the innermost open group is " + innermost + ".");
            }
            antiAlias = openGroups.Pop().AntiAliasBefore;
        }

        // Close the innermost group that a call opened, and every group still open within it.
        // opener: the call that opened it.
        // callName: the name of the closing call.
        private void CloseThrough(Opener opener, string callName)
        {
            bool open = false;
            foreach (OpenGroup openGroup in openGroups) {
                if (openGroup.Opener == opener)
                    open = true;
            }
            if (!open)
                throw new InvalidOperationException(callName + " closes a group opened by " + opener + ", but none is open.");
            if (openObjectKey != null)
                throw new InvalidOperationException(callName + ": the object begun last has not ended.");

            OpenGroup closed;
            do {
                closed = openGroups.Pop();
                antiAlias = closed.AntiAliasBefore;
            } while (closed.Opener != opener);
        }

        // Add an element within the innermost open group, as an element of the object begun last, if it has not ended.
        // element: the element.
        // callName: the name of the call that draws it.
        private void AddElement(SvgElement element, string callName)
        {
            RequireOpenPart(callName);
            if (openObjectKey != null)
                element.ObjectNumber = document.NumberOf(openObjectKey);
            openGroups.Peek().Group.Children.Add(element);
        }

        // Require that a bitmap can be drawn: within an open part, and at full intensity, since the elements a document
        // holds cannot lighten an image (B-6, D-12).
        // callName: the name of the call that draws the bitmap.
        private void RequireImageDrawing(string callName)
        {
            RequireOpenPart(callName);
            if (intensity < 1)
                throw new Svg_UnsupportedCallException(callName);
        }

        // Add the use of a bitmap's image over a rectangle, the bitmap's first pixel at the rectangle's left and top. The
        // image is written as the options say: in its own format when that is JPEG, at the options' quality, and as PNG
        // otherwise (B-3), embedded or linked, once however often the same data is drawn (B-4, B-5), or left out. A
        // nearest-neighbor scaling renders pixelated, the others smooth (B-2).
        // bitmap: the bitmap.
        // rectangle: the rectangle it covers.
        // scalingMode: how it is scaled.
        // callName: the name of the call that draws it.
        private void AddImage(IGraphicsBitmap bitmap, RectangleF rectangle, BitmapScaling scalingMode, string callName)
        {
            RequireImageDrawing(callName);
            if (bitmapOptions.Handling == SvgBitmapHandling.LeftOut)
                return;

            GraphicsBitmapFormat format = bitmap.GetOriginalFormat() == GraphicsBitmapFormat.JPEG ? GraphicsBitmapFormat.JPEG : GraphicsBitmapFormat.PNG;
            byte[] data;
            using (MemoryStream stream = new MemoryStream()) {
                if (!bitmap.WriteToStream(format, stream, bitmapOptions.JpegQuality))
                    throw new InvalidOperationException(callName + ": the bitmap cannot be written as " + format + ".");
                data = stream.ToArray();
            }
            SvgImage image = document.ImageOf(data, format, bitmap.PixelWidth, bitmap.PixelHeight, bitmapOptions);

            long left = SvgNumbers.ToSteps(rectangle.Left), top = SvgNumbers.ToSteps(rectangle.Top);
            long right = SvgNumbers.ToSteps(rectangle.Right), bottom = SvgNumbers.ToSteps(rectangle.Bottom);
            SvgTransform placement = new SvgTransform((right - left) / (double)bitmap.PixelWidth, 0, 0, (bottom - top) / (double)bitmap.PixelHeight, left, top);
            SvgBounds bounds = new SvgBounds();
            bounds.Include(left, top);
            bounds.Include(right, bottom);
            string style = scalingMode == BitmapScaling.NearestNeighbor ? "image-rendering:optimizeSpeed;image-rendering:pixelated" : null;
            AddElement(new SvgImageUse(image, placement, style, bounds), callName);
        }

        // Add text within the innermost open group: its baseline starts at the upper left corner lowered by the font's
        // ascent, as the application draws it, and each character after the first is moved by the kerning that the
        // application's shaping puts between it and the one before (T-1). The text is composed as the application
        // composes it before shaping, and its characters are recorded with its face, which the document embeds (T-6).
        // font: the font.
        // paint: the style of its fill or stroke.
        // text: the text.
        // upperLeft: its upper left corner.
        // callName: the name of the call that draws it.
        private void AddText(FontRecord font, SvgStyle paint, string text, PointF upperLeft, string callName)
        {
            RequireOpenPart(callName);
            string drawn = ComposeForXml(text);
            if (drawn.Length == 0)
                return;

            ITextFaceMetrics metrics = font.Metrics;
            long[] kerning = font.Kerning(drawn);
            SvgBounds bounds = new SvgBounds();
            bounds.Include(SvgNumbers.ToSteps(upperLeft.X), SvgNumbers.ToSteps(upperLeft.Y));
            bounds.Include(SvgNumbers.ToSteps(upperLeft.X + metrics.GetTextWidth(drawn)), SvgNumbers.ToSteps(upperLeft.Y + metrics.Ascent + metrics.Descent));
            font.Face.AddCharacters(drawn);
            AddElement(new SvgText(paint.Around(font.Declarations + ";", ""), SvgNumbers.ToSteps(upperLeft.X),
                                   SvgNumbers.ToSteps(upperLeft.Y + metrics.Ascent), kerning, drawn, bounds), callName);
        }

        // Text composed to the precomposed form, as the application composes it before shaping, and without the
        // characters that XML cannot hold: controls other than tab, line feed and carriage return, U+FFFE and U+FFFF,
        // and unpaired surrogates.
        // text: the text.
        private static string ComposeForXml(string text)
        {
            StringBuilder kept = new StringBuilder(text.Length);
            for (int index = 0; index < text.Length; ++index) {
                char character = text[index];
                if (char.IsSurrogatePair(text, index)) {
                    kept.Append(character).Append(text[++index]);
                }
                else if (!char.IsSurrogate(character) && character != '\uFFFE' && character != '\uFFFF' &&
                         (character >= ' ' || character == '\t' || character == '\n' || character == '\r')) {
                    kept.Append(character);
                }
            }
            string composed = kept.ToString();
            return composed.IsNormalized(NormalizationForm.FormC) ? composed : composed.Normalize(NormalizationForm.FormC);
        }

        // The font under a key.
        // fontKey: the key of the font.
        // callName: the name of the call that draws with it.
        private FontRecord GetFont(object fontKey, string callName)
        {
            FontRecord font;
            if (!fonts.TryGetValue(fontKey, out font))
                throw new InvalidOperationException(callName + ": the key has no font.");
            return font;
        }

        // Add a path shape within the innermost open group, unless the path is empty and draws nothing.
        // style: its style.
        // path: its data.
        // callName: the name of the call that draws it.
        private void AddPath(SvgStyle style, SvgPathData path, string callName)
        {
            RequireOpenPart(callName);
            if (!path.IsEmpty)
                AddElement(new SvgPath(style, path), callName);
        }

        // Add a brush under a key.
        // key: the brush's key.
        // brush: the brush.
        // callName: the name of the call that creates it.
        private void AddBrush(object key, SvgBrush brush, string callName)
        {
            if (brushes.ContainsKey(key))
                throw new InvalidOperationException(callName + ": the key has a brush already.");
            brushes.Add(key, brush);
        }

        // Add a pen: its paint, then its width, cap and join, each left out where it is SVG's default (D-5, S-9).
        // key: the pen's key.
        // paint: the style that paints the stroke.
        // width: the width of the stroke, in millimetres.
        // caps: the caps of its ends.
        // join: the joins of its corners.
        // miterLimit: how far a mitered corner may reach, in widths of the stroke.
        private void AddPen(object key, SvgStyle paint, float width, LineCapMode caps, LineJoinMode join, float miterLimit)
        {
            if (pens.ContainsKey(key))
                throw new InvalidOperationException("CreatePen: the key has a pen already.");
            // A width of zero is a hairline to Skia, one device pixel at any scale, which a document has no unit for.
            if (width == 0)
                throw new Svg_UnsupportedCallException(nameof(CreatePen));

            StringBuilder strokeShape = new StringBuilder(";stroke-width:").Append(SvgNumbers.FormatInteger(SvgNumbers.ToSteps(width)));
            switch (caps) {
                case LineCapMode.Flat:
                    break;
                case LineCapMode.Square:
                    strokeShape.Append(";stroke-linecap:square");
                    break;
                case LineCapMode.Round:
                    strokeShape.Append(";stroke-linecap:round");
                    break;
                default:
                    throw new Svg_UnsupportedCallException(nameof(CreatePen));
            }
            switch (join) {
                case LineJoinMode.Miter:
                    // Skia bevels every corner under a limit of 1 or less, which SVG would take for an error.
                    if (miterLimit <= 1)
                        strokeShape.Append(";stroke-linejoin:bevel");
                    else if (miterLimit != 4)
                        strokeShape.Append(";stroke-miterlimit:").Append(SvgNumbers.FormatShortest(miterLimit));
                    break;
                case LineJoinMode.Bevel:
                    strokeShape.Append(";stroke-linejoin:bevel");
                    break;
                case LineJoinMode.Round:
                    strokeShape.Append(";stroke-linejoin:round");
                    break;
                default:
                    // MiterClipped, which the Skia target rejects too (D-5).
                    throw new Svg_UnsupportedCallException(nameof(CreatePen));
            }
            pens.Add(key, paint.Around("fill:none;", strokeShape.ToString()));
        }

        // The brush under a key.
        // brushKey: the key of the brush.
        // callName: the name of the call that paints with it.
        private SvgBrush GetBrush(object brushKey, string callName)
        {
            SvgBrush brush;
            if (!brushes.TryGetValue(brushKey, out brush))
                throw new InvalidOperationException(callName + ": the key has no brush.");
            return brush;
        }

        // The style of a brush's fill.
        // brushKey: the key of the brush.
        // callName: the name of the call that fills with it.
        private SvgStyle GetFill(object brushKey, string callName)
        {
            return GetBrush(brushKey, callName).Paint(document.Paints, "fill");
        }

        // The style of a pen's stroke.
        // penKey: the key of the pen.
        // callName: the name of the call that strokes with it.
        private SvgStyle GetPen(object penKey, string callName)
        {
            SvgStyle stroke;
            if (!pens.TryGetValue(penKey, out stroke))
                throw new InvalidOperationException(callName + ": the key has no pen.");
            return stroke;
        }

        // The path under a key.
        // pathKey: the key of the path.
        // callName: the name of the call that uses it.
        private StoredPath GetPath(object pathKey, string callName)
        {
            StoredPath path;
            if (!paths.TryGetValue(pathKey, out path))
                throw new InvalidOperationException(callName + ": the key has no path.");
            return path;
        }

        // A color at the current intensity: below 1, its CMYK components scaled before conversion, as the Skia target
        // does (D-11).
        // color: the color at full intensity.
        private CmykColor AtIntensity(CmykColor color)
        {
            if (intensity >= 1)
                return color;
            return CmykColor.FromCmyka(color.Cyan * intensity, color.Magenta * intensity, color.Yellow * intensity, color.Black * intensity, color.Alpha);
        }

        // The style of a fill under its rule: nonzero, the default, or even-odd (D-3).
        // fill: the style of the fill.
        // evenOdd: whether the fill is under the even-odd rule.
        private static SvgStyle FillUnderRule(SvgStyle fill, bool evenOdd)
        {
            return evenOdd ? fill.Around("", ";fill-rule:evenodd") : fill;
        }

        // The path through points, in steps; closed for a polygon, open for a polyline.
        // points: the points, in millimetres.
        // close: whether the path closes.
        private static SvgPathData PathThrough(PointF[] points, bool close)
        {
            SvgPathData path = new SvgPathData();
            if (points.Length == 0)
                return path;
            path.MoveTo(SvgNumbers.ToSteps(points[0].X), SvgNumbers.ToSteps(points[0].Y));
            for (int index = 1; index < points.Length; ++index)
                path.LineTo(SvgNumbers.ToSteps(points[index].X), SvgNumbers.ToSteps(points[index].Y));
            if (close)
                path.Close();
            return path;
        }

        // The path of parts, in steps: a start moves, lines draw straight lines, Beziers draw cubic curves three points
        // at a time, and a close closes the subpath (D-3). A line or curve with no point before it starts at the origin,
        // as it does in Skia.
        // parts: the parts, in millimetres.
        private static SvgPathData PathOfParts(List<GraphicsPathPart> parts)
        {
            SvgPathData path = new SvgPathData();
            foreach (GraphicsPathPart part in parts) {
                switch (part.Kind) {
                    case GraphicsPathPartKind.Start:
                        path.MoveTo(SvgNumbers.ToSteps(part.Points[0].X), SvgNumbers.ToSteps(part.Points[0].Y));
                        break;
                    case GraphicsPathPartKind.Lines:
                        if (path.IsEmpty)
                            path.MoveTo(0, 0);
                        foreach (PointF point in part.Points)
                            path.LineTo(SvgNumbers.ToSteps(point.X), SvgNumbers.ToSteps(point.Y));
                        break;
                    case GraphicsPathPartKind.Beziers:
                        if (path.IsEmpty)
                            path.MoveTo(0, 0);
                        for (int index = 0; index + 2 < part.Points.Length; index += 3) {
                            path.CubicTo(SvgNumbers.ToSteps(part.Points[index].X), SvgNumbers.ToSteps(part.Points[index].Y),
                                         SvgNumbers.ToSteps(part.Points[index + 1].X), SvgNumbers.ToSteps(part.Points[index + 1].Y),
                                         SvgNumbers.ToSteps(part.Points[index + 2].X), SvgNumbers.ToSteps(part.Points[index + 2].Y));
                        }
                        break;
                    case GraphicsPathPartKind.Close:
                        if (!path.IsEmpty)
                            path.Close();
                        break;
                }
            }
            return path;
        }

        // The path of a rectangle, in steps.
        // rectangle: the rectangle, in millimetres.
        private static SvgPathData RectanglePath(RectangleF rectangle)
        {
            SvgPathData path = new SvgPathData();
            AddRectangle(path, rectangle);
            return path;
        }

        // Add a rectangle to a path as a closed subpath, from its top left corner along the top first, as Skia adds one.
        // path: the path.
        // rectangle: the rectangle, in millimetres.
        private static void AddRectangle(SvgPathData path, RectangleF rectangle)
        {
            long left = SvgNumbers.ToSteps(rectangle.Left), top = SvgNumbers.ToSteps(rectangle.Top);
            long right = SvgNumbers.ToSteps(rectangle.Right), bottom = SvgNumbers.ToSteps(rectangle.Bottom);
            path.MoveTo(left, top);
            path.LineTo(right, top);
            path.LineTo(right, bottom);
            path.LineTo(left, bottom);
            path.Close();
        }

        // An ellipse shape, in whole steps.
        // style: its style.
        // center: its center, in millimetres.
        // radiusX, radiusY: its radii, in millimetres.
        private static SvgShape EllipseShape(SvgStyle style, PointF center, float radiusX, float radiusY)
        {
            return new SvgEllipse(style, SvgNumbers.ToSteps(center.X), SvgNumbers.ToSteps(center.Y),
                                  Math.Abs(SvgNumbers.ToSteps(radiusX)), Math.Abs(SvgNumbers.ToSteps(radiusY)));
        }
    }
}
