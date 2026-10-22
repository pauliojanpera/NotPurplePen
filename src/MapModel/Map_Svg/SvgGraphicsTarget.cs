using System;
using System.Collections.Generic;
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
            nameof(Intensity), nameof(PushTransform), nameof(PopTransform), nameof(PushAntiAliasing), nameof(PopAntiAliasing),
            nameof(CreateSolidBrush), nameof(FillEllipse), nameof(FillPolygon), nameof(FillRectangle),
            nameof(HasPath), nameof(HasBrush), nameof(HasPen), nameof(HasFont),
        };

        // What opened a group that is still open, which the call that closes it must match.
        private enum Opener { BeginPart, BeginMap, PushTransform, PushAntiAliasing }

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

        // The document being written.
        private readonly SvgDocument document;

        // The groups still open, the innermost on top; the bottom one is the part's, while a part is open.
        private readonly Stack<OpenGroup> openGroups = new Stack<OpenGroup>();

        // The parts and the maps, by their keys.
        private readonly Dictionary<object, SvgPart> partsByKey = new Dictionary<object, SvgPart>(new IdentityComparer<object>());
        private readonly Dictionary<object, SvgMapRecord> mapsByKey = new Dictionary<object, SvgMapRecord>(new IdentityComparer<object>());

        // The declarations that paint a fill with each brush, by the brush's key.
        private readonly Dictionary<object, string> brushes = new Dictionary<object, string>(new IdentityComparer<object>());

        // The pens, paths and fonts, by their keys; the calls that create them are still to come.
        private readonly Dictionary<object, string> pens = new Dictionary<object, string>(new IdentityComparer<object>());
        private readonly Dictionary<object, string> paths = new Dictionary<object, string>(new IdentityComparer<object>());
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
            document.AddPage(paperSize.Width, paperSize.Height, stepsToPage, new SvgClip(clipPath.ToString(), false), clipPath.Bounds, parts);
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

        // Clip to a path created with CreatePath.
        public void PushClip(object pathKey)
        {
            throw new Svg_UnsupportedCallException(nameof(PushClip));
        }

        // Clip to a rectangle.
        public void PushClip(RectangleF rectangle)
        {
            throw new Svg_UnsupportedCallException(nameof(PushClip));
        }

        // Clip to the union of rectangles.
        public void PushClip(RectangleF[] rectangles)
        {
            throw new Svg_UnsupportedCallException(nameof(PushClip));
        }

        // Clip to a path given by its parts.
        public void PushClip(List<GraphicsPathPart> parts, AreaFillMode windingMode)
        {
            throw new Svg_UnsupportedCallException(nameof(PushClip));
        }

        // End the clip of the last PushClip.
        public void PopClip()
        {
            throw new Svg_UnsupportedCallException(nameof(PopClip));
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

        // Blend what is drawn until the matching PopBlending; return whether the mode is supported.
        public bool PushBlending(BlendMode blendMode)
        {
            throw new Svg_UnsupportedCallException(nameof(PushBlending));
        }

        // End the blending of the last PushBlending.
        public void PopBlending()
        {
            throw new Svg_UnsupportedCallException(nameof(PopBlending));
        }

        // Create a path under a key.
        public void CreatePath(object key, List<GraphicsPathPart> parts, AreaFillMode windingMode)
        {
            throw new Svg_UnsupportedCallException(nameof(CreatePath));
        }

        // Create a solid brush under a key, its color at the current intensity (D-10, D-11).
        public void CreateSolidBrush(object key, CmykColor color)
        {
            if (brushes.ContainsKey(key))
                throw new InvalidOperationException("CreateSolidBrush: the key has a brush already.");
            brushes.Add(key, document.Paints.Paint("fill", AtIntensity(color)));
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

        // Create a pen that strokes with a brush, under a key.
        public void CreatePen(object key, object brushKey, float width, LineCapMode caps, LineJoinMode join, float miterLimit)
        {
            throw new Svg_UnsupportedCallException(nameof(CreatePen));
        }

        // Create a pen of a solid color, under a key.
        public void CreatePen(object key, CmykColor color, float width, LineCapMode caps, LineJoinMode join, float miterLimit)
        {
            throw new Svg_UnsupportedCallException(nameof(CreatePen));
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

        // Draw a line with a pen.
        public void DrawLine(object penKey, PointF start, PointF finish)
        {
            throw new Svg_UnsupportedCallException(nameof(DrawLine));
        }

        // Draw an arc with a pen.
        public void DrawArc(object penKey, PointF center, float radius, float startAngle, float sweepAngle)
        {
            throw new Svg_UnsupportedCallException(nameof(DrawArc));
        }

        // Draw an ellipse with a pen.
        public void DrawEllipse(object penKey, PointF center, float radiusX, float radiusY)
        {
            throw new Svg_UnsupportedCallException(nameof(DrawEllipse));
        }

        // Fill an ellipse with a brush, as a circle when its radii are equal (D-4).
        public void FillEllipse(object brushKey, PointF center, float radiusX, float radiusY)
        {
            string fill = GetBrush(brushKey, nameof(FillEllipse));
            AddShape(EllipseShape(fill, center, radiusX, radiusY), nameof(FillEllipse));
        }

        // Draw a rectangle with a pen.
        public void DrawRectangle(object penKey, RectangleF rect)
        {
            throw new Svg_UnsupportedCallException(nameof(DrawRectangle));
        }

        // Fill a rectangle with a brush, as a path (D-4, F-3).
        public void FillRectangle(object brushKey, RectangleF rect)
        {
            string fill = GetBrush(brushKey, nameof(FillRectangle));
            AddShape(PathShape(fill, RectanglePath(rect)), nameof(FillRectangle));
        }

        // Draw a closed polygon with a pen.
        public void DrawPolygon(object penKey, PointF[] pts)
        {
            throw new Svg_UnsupportedCallException(nameof(DrawPolygon));
        }

        // Draw an open polyline with a pen.
        public void DrawPolyline(object penKey, PointF[] pts)
        {
            throw new Svg_UnsupportedCallException(nameof(DrawPolyline));
        }

        // Fill a polygon with a brush, under the even-odd rule in the alternate mode and the nonzero rule in the winding
        // mode (D-3).
        public void FillPolygon(object brushKey, PointF[] pts, AreaFillMode windingMode)
        {
            string fill = GetBrush(brushKey, nameof(FillPolygon));
            if (pts.Length == 0)
                return;

            SvgPathData path = new SvgPathData();
            path.MoveTo(SvgNumbers.ToSteps(pts[0].X), SvgNumbers.ToSteps(pts[0].Y));
            for (int index = 1; index < pts.Length; ++index)
                path.LineTo(SvgNumbers.ToSteps(pts[index].X), SvgNumbers.ToSteps(pts[index].Y));
            path.Close();
            AddShape(PathShape(FillUnderRule(fill, windingMode), path), nameof(FillPolygon));
        }

        // Draw a path created with CreatePath with a pen.
        public void DrawPath(object penKey, object pathKey)
        {
            throw new Svg_UnsupportedCallException(nameof(DrawPath));
        }

        // Draw a path given by its parts with a pen.
        public void DrawPath(object penKey, List<GraphicsPathPart> parts)
        {
            throw new Svg_UnsupportedCallException(nameof(DrawPath));
        }

        // Fill a path created with CreatePath with a brush.
        public void FillPath(object brushKey, object pathKey)
        {
            throw new Svg_UnsupportedCallException(nameof(FillPath));
        }

        // Fill a path given by its parts with a brush.
        public void FillPath(object brushKey, List<GraphicsPathPart> parts, AreaFillMode windingMode)
        {
            throw new Svg_UnsupportedCallException(nameof(FillPath));
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

        // The declarations of a brush's fill.
        // brushKey: the key of the brush.
        // callName: the name of the call that fills with it.
        private string GetBrush(object brushKey, string callName)
        {
            string fill;
            if (!brushes.TryGetValue(brushKey, out fill))
                throw new InvalidOperationException(callName + ": the key has no brush.");
            return fill;
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

        // The declarations of a fill under a fill mode's rule: nonzero, the default, for the winding mode, and even-odd
        // for the alternate one (D-3).
        // fill: the declarations of the fill.
        // windingMode: the fill mode.
        private static string FillUnderRule(string fill, AreaFillMode windingMode)
        {
            return windingMode == AreaFillMode.Alternate ? fill + ";fill-rule:evenodd" : fill;
        }

        // The path of a rectangle, in steps.
        // rectangle: the rectangle, in millimetres.
        private static SvgPathData RectanglePath(RectangleF rectangle)
        {
            long left = SvgNumbers.ToSteps(rectangle.Left), top = SvgNumbers.ToSteps(rectangle.Top);
            long right = SvgNumbers.ToSteps(rectangle.Right), bottom = SvgNumbers.ToSteps(rectangle.Bottom);
            SvgPathData path = new SvgPathData();
            path.MoveTo(left, top);
            path.LineTo(right, top);
            path.LineTo(right, bottom);
            path.LineTo(left, bottom);
            path.Close();
            return path;
        }

        // A path shape.
        // style: the declarations of its style.
        // path: its data.
        private static SvgShape PathShape(string style, SvgPathData path)
        {
            return new SvgShape("path", style, "d=\"" + path + "\"", path.Bounds);
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
