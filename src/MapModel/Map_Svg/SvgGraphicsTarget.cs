using System;
using System.Collections.Generic;
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
            nameof(CreatePath), nameof(CreateSolidBrush), nameof(CreatePen),
            nameof(HasPath), nameof(HasBrush), nameof(HasPen), nameof(HasFont),
            nameof(DrawLine), nameof(DrawArc), nameof(DrawEllipse), nameof(FillEllipse), nameof(DrawRectangle),
            nameof(FillRectangle), nameof(DrawPolygon), nameof(DrawPolyline), nameof(FillPolygon), nameof(DrawPath),
            nameof(FillPath),
        };

        // What opened a group that is still open, which the call that closes it must match.
        private enum Opener { BeginPart, BeginMap, PushTransform, PushClip, PushAntiAliasing, PushBlending }

        // A group still open, with what opened it and the anti-aliasing in force before it.
        private sealed class OpenGroup
        {
            public readonly Opener Opener;
            public readonly SvgGroup Group;
            public readonly bool AntiAliasBefore;

            public OpenGroup(Opener opener, SvgGroup group, bool antiAliasBefore)
            {
                Opener = opener;
                Group = group;
                AntiAliasBefore = antiAliasBefore;
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

        // The document being written.
        private readonly SvgDocument document;

        // The groups still open, the innermost on top; the bottom one is the part's, while a part is open.
        private readonly Stack<OpenGroup> openGroups = new Stack<OpenGroup>();

        // The parts and the maps, by their keys.
        private readonly Dictionary<object, SvgPart> partsByKey = new Dictionary<object, SvgPart>(new IdentityComparer<object>());
        private readonly Dictionary<object, SvgMapRecord> mapsByKey = new Dictionary<object, SvgMapRecord>(new IdentityComparer<object>());

        // The brushes, the declarations each pen strokes with, the paths and the fonts, by their keys (D-10).
        private readonly Dictionary<object, SvgBrush> brushes = new Dictionary<object, SvgBrush>(new IdentityComparer<object>());
        private readonly Dictionary<object, string> pens = new Dictionary<object, string>(new IdentityComparer<object>());
        private readonly Dictionary<object, StoredPath> paths = new Dictionary<object, StoredPath>(new IdentityComparer<object>());
        private readonly Dictionary<object, string> fonts = new Dictionary<object, string>(new IdentityComparer<object>());

        // The intensity that colors are drawn at.
        private float intensity = 1;

        // Whether drawing is anti-aliased, as a browser draws it unless told otherwise.
        private bool antiAlias = true;

        // Create a target that writes a document.
        // colorConverter: converts colors to sRGB, as the chosen color model does (COL-2).
        // declaresCmykColors: whether colors also carry their CMYK values, as under the CMYK color model (COL-1).
        public Svg_GraphicsTarget(IColorConverter colorConverter, bool declaresCmykColors)
        {
            document = new SvgDocument(new SvgPaints(colorConverter, declaresCmykColors));
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
            openGroups.Clear();
            return document.Write();
        }

        // Begin a part of a drawing; parts do not nest.
        public void BeginPart(object partKey)
        {
            if (openGroups.Count > 0)
                throw new InvalidOperationException("BeginPart: a part is open already; parts do not nest.");
            if (partsByKey.ContainsKey(partKey))
                throw new InvalidOperationException("BeginPart: a part with this key has begun already.");

            SvgPart part = new SvgPart();
            partsByKey.Add(partKey, part);
            document.Parts.Add(part);
            antiAlias = true;
            openGroups.Push(new OpenGroup(Opener.BeginPart, part.Content, antiAlias));
        }

        // End the part begun last, closing every group still open within it.
        public void EndPart()
        {
            CloseThrough(Opener.BeginPart, nameof(EndPart));
        }

        // Begin a labeled map within the part or map that is open (M-5).
        public void BeginMap(object mapKey, string label)
        {
            RequireOpenPart(nameof(BeginMap));

            SvgMapRecord map;
            if (!mapsByKey.TryGetValue(mapKey, out map)) {
                map = new SvgMapRecord(document.Maps.Count + 1, label);
                mapsByKey.Add(mapKey, map);
                document.Maps.Add(map);
            }
            Open(Opener.BeginMap, new SvgGroup { Identity = "data-m=\"" + SvgNumbers.FormatInteger(map.Number) + "\"" });
        }

        // End the map begun last, closing every group still open within it.
        public void EndMap()
        {
            CloseThrough(Opener.BeginMap, nameof(EndMap));
        }

        // Add a page that shows parts within an area of the map, transformed onto its paper (L-3, F-4).
        public void AddPage(SizeF paperSize, RectangleF mapArea, Matrix mapToPage, object[] partKeys)
        {
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
            if (brushes.ContainsKey(key))
                throw new InvalidOperationException("CreateSolidBrush: the key has a brush already.");
            brushes.Add(key, new SvgBrush(AtIntensity(color)));
        }

        // Whether pattern brushes are supported.
        public bool SupportsPatternBrushes
        {
            get { throw new Svg_UnsupportedCallException(nameof(SupportsPatternBrushes)); }
        }

        // Create a target that draws one tile of a pattern brush.
        public IBrushTarget CreatePatternBrush(SizeF size, float angle, int bitmapWidth, int bitmapHeight)
        {
            throw new Svg_UnsupportedCallException(nameof(CreatePatternBrush));
        }

        // Create a pen that strokes with a brush's paint, under a key (D-5, D-6).
        public void CreatePen(object key, object brushKey, float width, LineCapMode caps, LineJoinMode join, float miterLimit)
        {
            SvgBrush brush = GetBrush(brushKey, nameof(CreatePen));
            AddPen(key, brush.Paint(document.Paints, "stroke"), width, caps, join, miterLimit);
        }

        // Create a pen of a color at the current intensity, under a key (D-5, D-11).
        public void CreatePen(object key, CmykColor color, float width, LineCapMode caps, LineJoinMode join, float miterLimit)
        {
            AddPen(key, document.Paints.Paint("stroke", AtIntensity(color)), width, caps, join, miterLimit);
        }

        // Create a font under a key.
        public void CreateFont(object key, string familyName, float emHeight, TextEffects effects)
        {
            throw new Svg_UnsupportedCallException(nameof(CreateFont));
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
            string stroke = GetPen(penKey, nameof(DrawLine));
            SvgPathData path = new SvgPathData();
            path.MoveTo(SvgNumbers.ToSteps(start.X), SvgNumbers.ToSteps(start.Y));
            path.LineTo(SvgNumbers.ToSteps(finish.X), SvgNumbers.ToSteps(finish.Y));
            AddPath(stroke, path, nameof(DrawLine));
        }

        // Draw an arc with a pen. Its angles are degrees turning from the x axis towards the y axis, as Skia's are, and
        // an arc of a full turn or more is the whole circle, which Skia then draws as a closed oval (D-4).
        public void DrawArc(object penKey, PointF center, float radius, float startAngle, float sweepAngle)
        {
            string stroke = GetPen(penKey, nameof(DrawArc));
            if (Math.Abs(sweepAngle) >= 360) {
                AddShape(EllipseShape(stroke, center, radius, radius), nameof(DrawArc));
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
            string stroke = GetPen(penKey, nameof(DrawEllipse));
            AddShape(EllipseShape(stroke, center, radiusX, radiusY), nameof(DrawEllipse));
        }

        // Fill an ellipse with a brush, as a circle when its radii are equal (D-4).
        public void FillEllipse(object brushKey, PointF center, float radiusX, float radiusY)
        {
            string fill = GetFill(brushKey, nameof(FillEllipse));
            AddShape(EllipseShape(fill, center, radiusX, radiusY), nameof(FillEllipse));
        }

        // Draw a rectangle with a pen, as a closed path (D-4).
        public void DrawRectangle(object penKey, RectangleF rect)
        {
            string stroke = GetPen(penKey, nameof(DrawRectangle));
            AddPath(stroke, RectanglePath(rect), nameof(DrawRectangle));
        }

        // Fill a rectangle with a brush, as a path (D-4, F-3).
        public void FillRectangle(object brushKey, RectangleF rect)
        {
            string fill = GetFill(brushKey, nameof(FillRectangle));
            AddPath(fill, RectanglePath(rect), nameof(FillRectangle));
        }

        // Draw a closed polygon with a pen (D-4).
        public void DrawPolygon(object penKey, PointF[] pts)
        {
            string stroke = GetPen(penKey, nameof(DrawPolygon));
            AddPath(stroke, PathThrough(pts, true), nameof(DrawPolygon));
        }

        // Draw an open polyline with a pen (D-4).
        public void DrawPolyline(object penKey, PointF[] pts)
        {
            string stroke = GetPen(penKey, nameof(DrawPolyline));
            AddPath(stroke, PathThrough(pts, false), nameof(DrawPolyline));
        }

        // Fill a polygon with a brush, under the even-odd rule in the alternate mode and the nonzero rule in the winding
        // mode (D-3).
        public void FillPolygon(object brushKey, PointF[] pts, AreaFillMode windingMode)
        {
            string fill = GetFill(brushKey, nameof(FillPolygon));
            AddPath(FillUnderRule(fill, windingMode == AreaFillMode.Alternate), PathThrough(pts, true), nameof(FillPolygon));
        }

        // Draw a path created with CreatePath with a pen (D-3).
        public void DrawPath(object penKey, object pathKey)
        {
            string stroke = GetPen(penKey, nameof(DrawPath));
            AddPath(stroke, GetPath(pathKey, nameof(DrawPath)).Data, nameof(DrawPath));
        }

        // Draw a path given by its parts with a pen (D-3).
        public void DrawPath(object penKey, List<GraphicsPathPart> parts)
        {
            string stroke = GetPen(penKey, nameof(DrawPath));
            AddPath(stroke, PathOfParts(parts), nameof(DrawPath));
        }

        // Fill a path created with CreatePath with a brush, under the path's rule (D-3).
        public void FillPath(object brushKey, object pathKey)
        {
            string fill = GetFill(brushKey, nameof(FillPath));
            StoredPath path = GetPath(pathKey, nameof(FillPath));
            AddPath(FillUnderRule(fill, path.EvenOdd), path.Data, nameof(FillPath));
        }

        // Fill a path given by its parts with a brush, under the rule of its fill mode (D-3).
        public void FillPath(object brushKey, List<GraphicsPathPart> parts, AreaFillMode windingMode)
        {
            string fill = GetFill(brushKey, nameof(FillPath));
            AddPath(FillUnderRule(fill, windingMode == AreaFillMode.Alternate), PathOfParts(parts), nameof(FillPath));
        }

        // Draw text with its upper left corner at a point.
        public void DrawText(string text, object fontKey, object brushKey, PointF upperLeft)
        {
            throw new Svg_UnsupportedCallException(nameof(DrawText));
        }

        // Draw the outline of text with its upper left corner at a point.
        public void DrawTextOutline(string text, object fontKey, object penKey, PointF upperLeft)
        {
            throw new Svg_UnsupportedCallException(nameof(DrawTextOutline));
        }

        // Draw a bitmap over a rectangle.
        public void DrawBitmap(IGraphicsBitmap bm, RectangleF rectangle, BitmapScaling scalingMode)
        {
            throw new Svg_UnsupportedCallException(nameof(DrawBitmap));
        }

        // Draw part of a bitmap over a rectangle.
        public void DrawBitmapPart(IGraphicsBitmap bm, int x, int y, int width, int height, RectangleF rectangle, BitmapScaling scalingMode)
        {
            throw new Svg_UnsupportedCallException(nameof(DrawBitmapPart));
        }

        // Release nothing: the target holds no unmanaged resources.
        public void Dispose()
        {
        }

        // Fail a call unless a part is open, as drawing happens only within one.
        // callName: the name of the call.
        private void RequireOpenPart(string callName)
        {
            if (openGroups.Count == 0)
                throw new InvalidOperationException(callName + ": no part is open; drawing happens between BeginPart and EndPart.");
        }

        // Open a group within the innermost open one.
        // opener: what opens it.
        // group: the group.
        private void Open(Opener opener, SvgGroup group)
        {
            openGroups.Peek().Group.Children.Add(group);
            openGroups.Push(new OpenGroup(opener, group, antiAlias));
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

            OpenGroup closed;
            do {
                closed = openGroups.Pop();
                antiAlias = closed.AntiAliasBefore;
            } while (closed.Opener != opener);
        }

        // Add a shape within the innermost open group.
        // shape: the shape.
        // callName: the name of the call that draws it.
        private void AddShape(SvgShape shape, string callName)
        {
            RequireOpenPart(callName);
            openGroups.Peek().Group.Children.Add(shape);
        }

        // Add a path shape within the innermost open group, unless the path is empty and draws nothing.
        // style: the declarations of its style.
        // path: its data.
        // callName: the name of the call that draws it.
        private void AddPath(string style, SvgPathData path, string callName)
        {
            RequireOpenPart(callName);
            if (!path.IsEmpty)
                AddShape(new SvgShape("path", style, "d=\"" + path + "\"", path.Bounds), callName);
        }

        // Add a pen: its paint, then its width, cap and join, each left out where it is SVG's default (D-5, S-9).
        // key: the pen's key.
        // paint: the declarations that paint the stroke.
        // width: the width of the stroke, in millimetres.
        // caps: the caps of its ends.
        // join: the joins of its corners.
        // miterLimit: how far a mitered corner may reach, in widths of the stroke.
        private void AddPen(object key, string paint, float width, LineCapMode caps, LineJoinMode join, float miterLimit)
        {
            if (pens.ContainsKey(key))
                throw new InvalidOperationException("CreatePen: the key has a pen already.");
            // A width of zero is a hairline to Skia, one device pixel at any scale, which a document has no unit for.
            if (width == 0)
                throw new Svg_UnsupportedCallException(nameof(CreatePen));

            StringBuilder style = new StringBuilder("fill:none;").Append(paint);
            style.Append(";stroke-width:").Append(SvgNumbers.FormatInteger(SvgNumbers.ToSteps(width)));
            switch (caps) {
                case LineCapMode.Flat:
                    break;
                case LineCapMode.Square:
                    style.Append(";stroke-linecap:square");
                    break;
                case LineCapMode.Round:
                    style.Append(";stroke-linecap:round");
                    break;
                default:
                    throw new Svg_UnsupportedCallException(nameof(CreatePen));
            }
            switch (join) {
                case LineJoinMode.Miter:
                    // Skia bevels every corner under a limit of 1 or less, which SVG would take for an error.
                    if (miterLimit <= 1)
                        style.Append(";stroke-linejoin:bevel");
                    else if (miterLimit != 4)
                        style.Append(";stroke-miterlimit:").Append(SvgNumbers.FormatShortest(miterLimit));
                    break;
                case LineJoinMode.Bevel:
                    style.Append(";stroke-linejoin:bevel");
                    break;
                case LineJoinMode.Round:
                    style.Append(";stroke-linejoin:round");
                    break;
                default:
                    // MiterClipped, which the Skia target rejects too (D-5).
                    throw new Svg_UnsupportedCallException(nameof(CreatePen));
            }
            pens.Add(key, style.ToString());
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

        // The declarations of a brush's fill.
        // brushKey: the key of the brush.
        // callName: the name of the call that fills with it.
        private string GetFill(object brushKey, string callName)
        {
            return GetBrush(brushKey, callName).Paint(document.Paints, "fill");
        }

        // The declarations of a pen's stroke.
        // penKey: the key of the pen.
        // callName: the name of the call that strokes with it.
        private string GetPen(object penKey, string callName)
        {
            string stroke;
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

        // The declarations of a fill under its rule: nonzero, the default, or even-odd (D-3).
        // fill: the declarations of the fill.
        // evenOdd: whether the fill is under the even-odd rule.
        private static string FillUnderRule(string fill, bool evenOdd)
        {
            return evenOdd ? fill + ";fill-rule:evenodd" : fill;
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

        // An ellipse shape, a circle when its radii are equal in steps. A center coordinate of 0 is the default, which
        // is left out (S-9).
        // style: the declarations of its style.
        // center: its center, in millimetres.
        // radiusX, radiusY: its radii, in millimetres.
        private static SvgShape EllipseShape(string style, PointF center, float radiusX, float radiusY)
        {
            long cx = SvgNumbers.ToSteps(center.X), cy = SvgNumbers.ToSteps(center.Y);
            long rx = Math.Abs(SvgNumbers.ToSteps(radiusX)), ry = Math.Abs(SvgNumbers.ToSteps(radiusY));
            SvgBounds bounds = new SvgBounds();
            bounds.Include(cx - rx, cy - ry);
            bounds.Include(cx + rx, cy + ry);

            string position = (cx != 0 ? "cx=\"" + SvgNumbers.FormatInteger(cx) + "\" " : "")
                            + (cy != 0 ? "cy=\"" + SvgNumbers.FormatInteger(cy) + "\" " : "");
            if (rx == ry)
                return new SvgShape("circle", style, position + "r=\"" + SvgNumbers.FormatInteger(rx) + "\"", bounds);
            return new SvgShape("ellipse", style, position + "rx=\"" + SvgNumbers.FormatInteger(rx) + "\" ry=\"" + SvgNumbers.FormatInteger(ry) + "\"", bounds);
        }
    }
}
