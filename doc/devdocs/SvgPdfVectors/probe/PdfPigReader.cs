using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using PurplePen;
using PurplePen.Graphics2D;
using PurplePen.MapModel;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.Core;
using UglyToad.PdfPig.Filters;
using UglyToad.PdfPig.Geometry;
using UglyToad.PdfPig.Graphics;
using UglyToad.PdfPig.Graphics.Colors;
using UglyToad.PdfPig.Graphics.Core;
using UglyToad.PdfPig.Graphics.Operations;
using UglyToad.PdfPig.Outline.Destinations;
using UglyToad.PdfPig.Parser;
using UglyToad.PdfPig.PdfFonts;
using UglyToad.PdfPig.Tokenization.Scanner;
using UglyToad.PdfPig.Tokens;
using UglyToad.PdfPig.XObjects;
using PigMatrix = UglyToad.PdfPig.Core.TransformationMatrix;

namespace PdfVectorsProbe
{
    // The vector way: PdfPig reads the page's content, and each painting operator becomes a call of IGraphicsTarget into
    // the SVG target, as the map code's drawing does: paths with their fill and stroke in CMYK, clips, glyphs as filled
    // paths, images through DrawBitmap, and tiling patterns as pattern brushes. The map's part holds the drawing under
    // the map's label, outside any color run (M-5). What the target cannot take is counted, not drawn.
    internal static class PdfPigReader
    {
        // Millimetres in a PDF point.
        public const double MillimetresPerPoint = 25.4 / 72.0;

        // What one reading wrote and counted.
        internal sealed class Result
        {
            // The document's text.
            public string Svg;

            // The seconds that reading and drawing the page took, before the document was written.
            public double ReadSeconds;

            // What was drawn, and what was not, by kind.
            public readonly SortedDictionary<string, int> Counts = new SortedDictionary<string, int>(StringComparer.Ordinal);

            // Count one more of a kind.
            // kind: the kind.
            public void Count(string kind)
            {
                int count;
                Counts.TryGetValue(kind, out count);
                Counts[kind] = count + 1;
            }

            // The counts as JSON.
            public string CountsJson()
            {
                return "{" + string.Join(",", Counts.Select(pair => Program.Json(pair.Key) + ":" + pair.Value.ToString(CultureInfo.InvariantCulture))) + "}";
            }
        }

        // What a page factory hands the processor it creates: set before a page is read, as PdfPig creates factories
        // through their constructors alone.
        internal sealed class Context
        {
            public IGraphicsTarget Target;
            public bool OverprintDarken;
            public Result Result;

            // The pattern brushes made of the document's target, by their cells (TargetStreamProcessor.PatternBrush).
            public readonly Dictionary<string, object> PatternBrushes = new Dictionary<string, object>(StringComparer.Ordinal);
        }

        [ThreadStatic]
        internal static Context Current;

        // Read the first page of the PDF of a map into the SVG target as vectors, and write the document.
        // drawing: the map as the application holds it, which gives the map's bounds and the page.
        // overprintDarken: whether what overprints is drawn with darken blending, as COL-7 shows a map's overprinting.
        // colorConverter: the converter of the colors to sRGB: SwopColorConverter under the CMYK model (COL-2).
        // declaresCmykColors: whether the styles carry the CMYK values too, as under the CMYK model (COL-1).
        public static Result WriteSvg(ApplicationDrawing drawing, bool overprintDarken, IColorConverter colorConverter, bool declaresCmykColors)
        {
            Result result = new Result();
            using (Svg_GraphicsTarget target = new Svg_GraphicsTarget(colorConverter, declaresCmykColors, SvgBitmapOptions.Default,
                                                                      Services.TextMetricsProvider, Services.FontSubsetProvider)) {
                object part = new object(), map = new object();
                Stopwatch watch = Stopwatch.StartNew();
                target.BeginPart(part);
                target.BeginMap(map, Path.GetFileName(drawing.PdfPath));
                target.PushAntiAliasing(true);
                ParsingOptions options = new ParsingOptions { ClipPaths = false, UseLenientParsing = true, SkipMissingFonts = true };
                using (PdfDocument document = PdfDocument.Open(drawing.PdfPath, options)) {
                    document.AddPageFactory<DrawnPage, TargetPageFactory>();
                    Current = new Context { Target = target, OverprintDarken = overprintDarken, Result = result };
                    try {
                        document.GetPage<DrawnPage>(1);
                    }
                    finally {
                        Current = null;
                    }
                }
                target.PopAntiAliasing();
                target.EndMap();
                target.EndPart();
                drawing.AddWholePage(target, part);
                watch.Stop();
                result.ReadSeconds = watch.Elapsed.TotalSeconds;
                result.Svg = target.FinishDocument();
            }
            return result;
        }
    }

    // What reading a page gives back: nothing but the counts, which the context holds.
    internal sealed class DrawnPage
    {
    }

    // The page factory through which PdfPig hands a page's operations to TargetStreamProcessor.
    internal sealed class TargetPageFactory : BasePageFactory<DrawnPage>
    {
        // Create the factory, as PdfPig creates it.
        public TargetPageFactory(IPdfTokenScanner pdfScanner, IResourceStore resourceStore, ILookupFilterProvider filterProvider,
                                 IPageContentParser pageContentParser, ParsingOptions parsingOptions)
            : base(pdfScanner, resourceStore, filterProvider, pageContentParser, parsingOptions)
        {
        }

        // Draw a page's operations into the context's target.
        protected override DrawnPage ProcessPage(int pageNumber, DictionaryToken dictionary, NamedDestinations namedDestinations,
                                                 MediaBox mediaBox, CropBox cropBox, UserSpaceUnit userSpaceUnit,
                                                 PageRotationDegrees rotation, PigMatrix initialMatrix,
                                                 IReadOnlyList<IGraphicsStateOperation> operations)
        {
            PdfPigReader.Context context = PdfPigReader.Current;
            if (rotation.Value != 0)
                context.Result.Count("unsupported:page-rotation");
            // From the page's space, in points, to millimetres of map: the crop box's lower left corner is the map's
            // origin, as in PDFium's bitmap and in PDF export's copy of the page.
            PdfRectangle crop = cropBox.Bounds;
            double k = PdfPigReader.MillimetresPerPoint;
            PigMatrix pageToMap = PigMatrix.FromValues(k, 0, 0, k, -crop.Left * k, -crop.Bottom * k);
            TargetStreamProcessor processor = new TargetStreamProcessor(pageNumber, ResourceStore, PdfScanner, PageContentParser,
                                                                        FilterProvider, cropBox, userSpaceUnit, rotation,
                                                                        PigMatrix.Identity, ParsingOptions, context, context.Target, pageToMap);
            return processor.Process(pageNumber, operations);
        }
    }

    // Draws a content stream's operators into an IGraphicsTarget. Points are taken through the current transformation
    // matrix and then the matrix from the stream's space to the target's, and written in the target's own coordinates;
    // clips are pushed as the stream clips and popped as its graphics states are restored.
    internal sealed class TargetStreamProcessor : BaseStreamProcessor<DrawnPage>
    {
        private readonly PdfPigReader.Context context;
        private readonly IGraphicsTarget target;
        private readonly PigMatrix toTarget;

        // The path being built, in the target's coordinates.
        private readonly List<GraphicsPathPart> parts = new List<GraphicsPathPart>();
        private readonly List<PointF> run = new List<PointF>();
        private GraphicsPathPartKind runKind = GraphicsPathPartKind.Start;
        private PdfPoint subpathStart;
        private bool hasCurrentPoint;
        private FillingRule? pendingClip;

        // How many clips each saved graphics state level has pushed, the current level on top.
        private readonly Stack<int> clipsPushed = new Stack<int>();

        // The matrix of the content stream being read: the identity for the page, the form's for a form XObject, to which
        // a pattern's matrix is relative.
        private readonly Stack<PigMatrix> streamMatrices = new Stack<PigMatrix>();

        // The brushes and pens made, by their paint, so that one paint is one key.
        private readonly Dictionary<string, object> brushes = new Dictionary<string, object>(StringComparer.Ordinal);
        private readonly Dictionary<string, object> pens = new Dictionary<string, object>(StringComparer.Ordinal);

        // Create the processor of a content stream.
        // toTarget: the matrix from the stream's space, after the CTM, to the target's coordinates.
        public TargetStreamProcessor(int pageNumber, IResourceStore resourceStore, IPdfTokenScanner pdfScanner,
                                     IPageContentParser pageContentParser, ILookupFilterProvider filterProvider, CropBox cropBox,
                                     UserSpaceUnit userSpaceUnit, PageRotationDegrees rotation, PigMatrix initialMatrix,
                                     ParsingOptions parsingOptions, PdfPigReader.Context context, IGraphicsTarget target, PigMatrix toTarget)
            : base(pageNumber, resourceStore, pdfScanner, pageContentParser, filterProvider, cropBox, userSpaceUnit, rotation,
                   initialMatrix, parsingOptions)
        {
            this.context = context;
            this.target = target;
            this.toTarget = toTarget;
            clipsPushed.Push(0);
            streamMatrices.Push(PigMatrix.Identity);
        }

        // Read the operations, and pop the clips still pushed at the end.
        public override DrawnPage Process(int pageNumberCurrent, IReadOnlyList<IGraphicsStateOperation> operations)
        {
            ProcessOperations(operations);
            while (clipsPushed.Count > 0) {
                for (int i = clipsPushed.Pop(); i > 0; --i)
                    target.PopClip();
            }
            clipsPushed.Push(0);
            return new DrawnPage();
        }

        // Read operations given from outside, as a pattern's cell is read.
        public void ProcessCell(IReadOnlyList<IGraphicsStateOperation> operations)
        {
            Process(PageNumber, operations);
        }

        private void Count(string kind)
        {
            context.Result.Count(kind);
        }

        // A point of the stream's space in the target's coordinates.
        private PointF ToTarget(double x, double y)
        {
            PdfPoint page = CurrentTransformationMatrix.Transform(new PdfPoint(x, y));
            PdfPoint mapped = toTarget.Transform(page);
            return new PointF((float)mapped.X, (float)mapped.Y);
        }

        // A named graphics state, as PdfPig sets it, but with PDF's rule that an absent op takes OP's value (ISO 32000-1,
        // table 58), which PdfPig 0.1.16 quotes and does not apply: its OCAD maps' fills would lose their overprinting.
        public override void SetNamedGraphicsState(NameToken stateName)
        {
            base.SetNamedGraphicsState(stateName);
            DictionaryToken state = ResourceStore.GetExtendedGraphicsStateDictionary(stateName);
            if (state != null && state.ContainsKey(NameToken.Op) && !state.ContainsKey(NameToken.OpNs)) {
                GetCurrentState().NonStrokingOverprint = GetCurrentState().Overprint;
                Count("overprint:op-from-OP");
            }
        }

        public override void PushState()
        {
            base.PushState();
            clipsPushed.Push(0);
        }

        public override void PopState()
        {
            for (int i = clipsPushed.Pop(); i > 0; --i)
                target.PopClip();
            base.PopState();
        }

        // The path.
        public override void BeginSubpath()
        {
        }

        public override void MoveTo(double x, double y)
        {
            FlushRun();
            parts.Add(new GraphicsPathPart(GraphicsPathPartKind.Start, new PointF[] { ToTarget(x, y) }));
            subpathStart = new PdfPoint(x, y);
            CurrentPosition = subpathStart;
            hasCurrentPoint = true;
        }

        public override void LineTo(double x, double y)
        {
            Append(GraphicsPathPartKind.Lines, ToTarget(x, y));
            CurrentPosition = new PdfPoint(x, y);
        }

        public override void BezierCurveTo(double x2, double y2, double x3, double y3)
        {
            PdfPoint current = CurrentPosition;
            BezierCurveTo(current.X, current.Y, x2, y2, x3, y3);
        }

        public override void BezierCurveTo(double x1, double y1, double x2, double y2, double x3, double y3)
        {
            Append(GraphicsPathPartKind.Beziers, ToTarget(x1, y1));
            run.Add(ToTarget(x2, y2));
            run.Add(ToTarget(x3, y3));
            CurrentPosition = new PdfPoint(x3, y3);
        }

        public override void Rectangle(double x, double y, double width, double height)
        {
            MoveTo(x, y);
            LineTo(x + width, y);
            LineTo(x + width, y + height);
            LineTo(x, y + height);
            ClosePath();
            CurrentPosition = new PdfPoint(x, y);
        }

        public override PdfPoint? CloseSubpath()
        {
            ClosePath();
            return subpathStart;
        }

        public override void ClosePath()
        {
            FlushRun();
            if (parts.Count > 0)
                parts.Add(new GraphicsPathPart(GraphicsPathPartKind.Close, new PointF[0]));
        }

        // Append a point to the run of lines or curves being built.
        private void Append(GraphicsPathPartKind kind, PointF point)
        {
            if (!hasCurrentPoint) {
                parts.Add(new GraphicsPathPart(GraphicsPathPartKind.Start, new PointF[] { ToTarget(0, 0) }));
                hasCurrentPoint = true;
            }
            if (run.Count > 0 && runKind != kind)
                FlushRun();
            runKind = kind;
            run.Add(point);
        }

        private void FlushRun()
        {
            if (run.Count > 0) {
                parts.Add(new GraphicsPathPart(runKind, run.ToArray()));
                run.Clear();
            }
        }

        // Take the path built, and start a new one.
        private List<GraphicsPathPart> TakePath()
        {
            FlushRun();
            List<GraphicsPathPart> path = new List<GraphicsPathPart>(parts);
            parts.Clear();
            hasCurrentPoint = false;
            return path;
        }

        // Painting.
        public override void StrokePath(bool close)
        {
            if (close)
                ClosePath();
            List<GraphicsPathPart> path = TakePath();
            Paint(path, null, true);
        }

        public override void FillPath(FillingRule fillingRule, bool close)
        {
            if (close)
                ClosePath();
            List<GraphicsPathPart> path = TakePath();
            Paint(path, fillingRule, false);
        }

        public override void FillStrokePath(FillingRule fillingRule, bool close)
        {
            if (close)
                ClosePath();
            List<GraphicsPathPart> path = TakePath();
            Paint(path, fillingRule, true);
        }

        public override void EndPath()
        {
            List<GraphicsPathPart> path = TakePath();
            ApplyPendingClip(path);
        }

        public override void ModifyClippingIntersect(FillingRule clippingRule)
        {
            pendingClip = clippingRule;
        }

        // Clip to the path just painted or ended, if a clip operator came before its painting operator.
        private void ApplyPendingClip(List<GraphicsPathPart> path)
        {
            if (pendingClip == null)
                return;
            AreaFillMode mode = pendingClip == FillingRule.EvenOdd ? AreaFillMode.Alternate : AreaFillMode.Winding;
            pendingClip = null;
            if (path.Count == 0) {
                Count("clip:empty-path");
                return;
            }
            object key = new object();
            target.CreatePath(key, path, mode);
            target.PushClip(key);
            clipsPushed.Push(clipsPushed.Pop() + 1);
            Count("clip");
        }

        protected override void ClipToRectangle(PdfRectangle rectangle, FillingRule clippingRule)
        {
            PointF[] corners = new PointF[] {
                ToTarget(rectangle.Left, rectangle.Bottom), ToTarget(rectangle.Right, rectangle.Bottom),
                ToTarget(rectangle.Right, rectangle.Top), ToTarget(rectangle.Left, rectangle.Top) };
            List<GraphicsPathPart> path = new List<GraphicsPathPart> {
                new GraphicsPathPart(GraphicsPathPartKind.Start, new PointF[] { corners[0] }),
                new GraphicsPathPart(GraphicsPathPartKind.Lines, new PointF[] { corners[1], corners[2], corners[3] }),
                new GraphicsPathPart(GraphicsPathPartKind.Close, new PointF[0]) };
            object key = new object();
            target.CreatePath(key, path, AreaFillMode.Winding);
            target.PushClip(key);
            clipsPushed.Push(clipsPushed.Pop() + 1);
            Count("clip:form-bbox");
        }

        // Fill and stroke a path, if it is to be filled (fillingRule given) or stroked, then clip to it if asked.
        private void Paint(List<GraphicsPathPart> path, FillingRule? fillingRule, bool stroke)
        {
            CurrentGraphicsState state = GetCurrentState();
            if (path.Count > 0) {
                if (fillingRule != null) {
                    AreaFillMode mode = fillingRule == FillingRule.EvenOdd ? AreaFillMode.Alternate : AreaFillMode.Winding;
                    PaintWith(state, false, () => {
                        IColor color = state.CurrentNonStrokingColor;
                        if (color is TilingPatternColor tiling)
                            FillWithPattern(path, mode, tiling, state);
                        else {
                            object brush = Brush(color, state.AlphaConstantNonStroking);
                            if (brush != null) {
                                target.FillPath(brush, path, mode);
                                Count("fill");
                            }
                        }
                    });
                }
                if (stroke) {
                    PaintWith(state, true, () => {
                        object pen = Pen(state);
                        if (pen != null) {
                            target.DrawPath(pen, path);
                            Count("stroke");
                        }
                    });
                }
            }
            ApplyPendingClip(path);
        }

        // Paint with the state's blending: darken where the state blends so, and where it overprints when overprinting
        // is shown so (COL-7, D-8).
        private void PaintWith(CurrentGraphicsState state, bool stroking, Action paint)
        {
            bool overprints = stroking ? state.Overprint : state.NonStrokingOverprint;
            if (overprints)
                Count(stroking ? "overprint:stroke" : "overprint:fill");
            bool darken = state.BlendMode == UglyToad.PdfPig.Graphics.Core.BlendMode.Darken || (overprints && context.OverprintDarken);
            if (state.BlendMode != UglyToad.PdfPig.Graphics.Core.BlendMode.Normal && state.BlendMode != UglyToad.PdfPig.Graphics.Core.BlendMode.Darken)
                Count("unsupported:blend-mode:" + state.BlendMode);
            if (state.SoftMask != null)
                Count("unsupported:soft-mask");
            if (darken && target.PushBlending(PurplePen.Graphics2D.BlendMode.Darken)) {
                paint();
                target.PopBlending();
            }
            else
                paint();
        }

        // The brush of a color at an opacity, made once; null for a color the target cannot paint with.
        private object Brush(IColor color, double alpha)
        {
            CmykColor cmyk = ToCmyk(color, alpha);
            if (cmyk == null)
                return null;
            string key = PaintKey(cmyk);
            object brush;
            if (!brushes.TryGetValue(key, out brush)) {
                brush = new object();
                target.CreateSolidBrush(brush, cmyk);
                brushes[key] = brush;
            }
            return brush;
        }

        // The pen of the state's stroke, made once; null for a paint the target cannot stroke with.
        private object Pen(CurrentGraphicsState state)
        {
            CmykColor cmyk = ToCmyk(state.CurrentStrokingColor, state.AlphaConstantStroking);
            if (cmyk == null)
                return null;
            // The line width is in the stream's space; the target's is taken along the transformed x axis.
            PointF origin = ToTarget(0, 0), unit = ToTarget(state.LineWidth, 0), unitY = ToTarget(0, state.LineWidth);
            float width = (float)Math.Sqrt(Math.Abs((unit.X - origin.X) * (unitY.Y - origin.Y) - (unit.Y - origin.Y) * (unitY.X - origin.X)));
            if (state.LineWidth == 0)
                Count("stroke:zero-width");
            if (state.LineDashPattern.Array.Count > 0)
                Count("unsupported:dash");
            LineCapMode cap = state.CapStyle == LineCapStyle.Round ? LineCapMode.Round : state.CapStyle == LineCapStyle.ProjectingSquare ? LineCapMode.Square : LineCapMode.Flat;
            LineJoinMode join = state.JoinStyle == LineJoinStyle.Round ? LineJoinMode.Round : state.JoinStyle == LineJoinStyle.Bevel ? LineJoinMode.Bevel : LineJoinMode.Miter;
            string key = PaintKey(cmyk) + "|" + width.ToString("R", CultureInfo.InvariantCulture) + "|" + cap + "|" + join + "|" +
                         state.MiterLimit.ToString("R", CultureInfo.InvariantCulture);
            object pen;
            if (!pens.TryGetValue(key, out pen)) {
                pen = new object();
                target.CreatePen(pen, cmyk, width, cap, join, (float)state.MiterLimit);
                pens[key] = pen;
            }
            return pen;
        }

        // The key of a paint.
        private static string PaintKey(CmykColor color)
        {
            return string.Join(",", new float[] { color.Cyan, color.Magenta, color.Yellow, color.Black, color.Alpha }
                                    .Select(v => v.ToString("R", CultureInfo.InvariantCulture)));
        }

        // A PDF color as the target's: DeviceCMYK kept as it is, gray as black ink, RGB through the naive conversion
        // that CmykColor.FromRgb makes, which the target then writes as CMYK it was never given.
        private CmykColor ToCmyk(IColor color, double alpha)
        {
            ColorSpaceDetails space = null;
            if (ColorSpaceContext != null)
                space = ColorSpaceContext.CurrentNonStrokingColorSpace;
            switch (color) {
            case null:
                return CmykColor.FromCmyka(0, 0, 0, 1, (float)alpha);
            case CMYKColor cmyk:
                Count("color:cmyk");
                return CmykColor.FromCmyka((float)cmyk.C, (float)cmyk.M, (float)cmyk.Y, (float)cmyk.K, (float)alpha);
            case GrayColor gray:
                Count("color:gray");
                return CmykColor.FromCmyka(0, 0, 0, 1 - (float)gray.Gray, (float)alpha);
            case RGBColor rgb:
                Count("color:rgb");
                return CmykColor.FromRgba((float)rgb.R, (float)rgb.G, (float)rgb.B, (float)alpha);
            case PatternColor pattern:
                Count("unsupported:pattern-type-" + pattern.PatternType);
                return null;
            default:
                Count("unsupported:color:" + color.GetType().Name);
                double r, g, b;
                (r, g, b) = color.ToRGBValues();
                return CmykColor.FromRgba((float)r, (float)g, (float)b, (float)alpha);
            }
        }

        // The color space context, which PdfPig keeps in the graphics state.
        private IColorSpaceContext ColorSpaceContext
        {
            get { return GetCurrentState().ColorSpaceContext; }
        }

        // Fill a path with a tiling pattern: the target's pattern brush, whose tile is the pattern's cell, drawn in the
        // pattern's space, under a transform from that space to the target's (D-7). The tile's middle is the space's
        // origin, so the cell, which begins at the origin, is drawn four times, a step apart, for the tile to cut.
        private void FillWithPattern(List<GraphicsPathPart> path, AreaFillMode mode, TilingPatternColor tiling, CurrentGraphicsState state)
        {
            if (tiling.PaintType != PatternPaintType.Coloured) {
                Count("unsupported:uncoloured-pattern");
                return;
            }
            if (!target.SupportsPatternBrushes) {
                Count("unsupported:pattern-brushes");
                return;
            }
            // The brush's space is the pattern's, moved by half a cell, since the target's tile has its middle at the
            // origin where a PDF cell has its corner: so the fill is drawn under the transform from the brush's space,
            // through the pattern's matrix, the stream's own matrix and the stream's space, to the target's.
            float width = (float)Math.Abs(tiling.XStep), height = (float)Math.Abs(tiling.YStep);
            PigMatrix brushToTarget = PigMatrix.FromValues(1, 0, 0, 1, width / 2, height / 2)
                .Multiply(tiling.Matrix).Multiply(streamMatrices.Peek()).Multiply(toTarget);
            Matrix brushToTargetMatrix = new Matrix((float)brushToTarget.A, (float)brushToTarget.B, (float)brushToTarget.C,
                                                    (float)brushToTarget.D, (float)brushToTarget.E, (float)brushToTarget.F);
            Matrix targetToBrush = brushToTargetMatrix.Clone();
            targetToBrush.Invert();
            List<GraphicsPathPart> inBrush = path.Select(part => {
                PointF[] points = (PointF[])part.Points.Clone();
                if (points.Length > 0)
                    targetToBrush.TransformPoints(points);
                return new GraphicsPathPart(part.Kind, points);
            }).ToList();

            object brush = PatternBrush(tiling, width, height);
            target.PushTransform(brushToTargetMatrix);
            target.FillPath(brush, inBrush, mode);
            target.PopTransform();
            Count("fill:tiling-pattern");
        }

        // The pattern brush of a tiling pattern's cell, made once for each cell of the same content, steps and box: its
        // tile is the cell drawn once, its corner moved from the origin to the tile's corner.
        private object PatternBrush(TilingPatternColor tiling, float width, float height)
        {
            string key = Convert.ToBase64String(System.Security.Cryptography.SHA256.HashData(tiling.Data.Span)) + "|" +
                         width.ToString("R", CultureInfo.InvariantCulture) + "|" + height.ToString("R", CultureInfo.InvariantCulture) + "|" +
                         tiling.BBox + "|" + (tiling.Resources == null ? "" : tiling.Resources.ToString());
            object brush;
            // Brushes are keys of the target that made them, so only the document's own are shared.
            bool shared = ReferenceEquals(target, context.Target);
            if (shared && context.PatternBrushes.TryGetValue(key, out brush)) {
                Count("pattern:reused");
                return brush;
            }
            IBrushTarget tile = target.CreatePatternBrush(new SizeF(width, height), 0, 16, 16);
            IReadOnlyList<IGraphicsStateOperation> operations = GetFormOperations(tiling.PatternStream);
            bool resourcesLoaded = false;
            if (tiling.Resources != null) {
                ResourceStore.LoadResourceDictionary(tiling.Resources);
                resourcesLoaded = true;
            }
            try {
                PigMatrix cellToTile = PigMatrix.FromValues(1, 0, 0, 1, -width / 2, -height / 2);
                TargetStreamProcessor cell = new TargetStreamProcessor(PageNumber, ResourceStore, PdfScanner, PageContentParser,
                                                                       FilterProvider, new CropBox(new PdfRectangle(-1e6, -1e6, 1e6, 1e6)),
                                                                       UserSpaceUnit, new PageRotationDegrees(0), PigMatrix.Identity,
                                                                       ParsingOptions, context, tile, cellToTile);
                cell.ProcessCell(operations);
            }
            finally {
                if (resourcesLoaded)
                    ResourceStore.UnloadResourceDictionary();
            }
            brush = new object();
            tile.FinishBrush(brush);
            if (shared)
                context.PatternBrushes[key] = brush;
            Count("pattern:made");
            return brush;
        }

        // Form XObjects: the stream's own matrix, to which patterns inside are relative, is the CTM with the form's
        // matrix; transparency groups, which PdfPig reads with normal blending and full opacity, are counted.
        protected override void ProcessFormXObject(StreamToken formStream, NameToken xObjectName)
        {
            PigMatrix formMatrix = PigMatrix.Identity;
            if (formStream.StreamDictionary.TryGet<ArrayToken>(NameToken.Matrix, PdfScanner, out ArrayToken matrixToken))
                formMatrix = PigMatrix.FromArray(matrixToken.Data.OfType<NumericToken>().Select(x => x.Double).ToArray());
            if (formStream.StreamDictionary.ContainsKey(NameToken.Group))
                Count("form:transparency-group");
            else
                Count("form");
            streamMatrices.Push(formMatrix.Multiply(CurrentTransformationMatrix));
            try {
                base.ProcessFormXObject(formStream, xObjectName);
            }
            finally {
                streamMatrices.Pop();
            }
        }

        // Text: each glyph's outline, filled or stroked as the text rendering mode says.
        public override void RenderGlyph(IFont font, CurrentGraphicsState currentState, double fontSize, double pointSize, int code,
                                         string unicode, long currentOffset, in PigMatrix renderingMatrix, in PigMatrix textMatrix,
                                         in PigMatrix transformationMatrix, CharacterBoundingBox characterBoundingBox)
        {
            TextRenderingMode mode = currentState.FontState.TextRenderingMode;
            if (mode == TextRenderingMode.Neither) {
                Count("glyph:invisible");
                return;
            }
            IReadOnlyList<PdfSubpath> outline;
            if (!font.TryGetNormalisedPath(code, out outline)) {
                Count("unsupported:glyph-without-outline");
                return;
            }
            PigMatrix glyphToPage = renderingMatrix.Multiply(textMatrix).Multiply(transformationMatrix);
            List<GraphicsPathPart> path = new List<GraphicsPathPart>();
            foreach (PdfSubpath subpath in outline)
                AddSubpath(path, subpath, glyphToPage);
            if (path.Count == 0) {
                Count("glyph:empty");
                return;
            }
            Count("glyph");
            bool fill = mode == TextRenderingMode.Fill || mode == TextRenderingMode.FillThenStroke || mode == TextRenderingMode.FillClip || mode == TextRenderingMode.FillThenStrokeClip;
            bool stroke = mode == TextRenderingMode.Stroke || mode == TextRenderingMode.FillThenStroke || mode == TextRenderingMode.StrokeClip || mode == TextRenderingMode.FillThenStrokeClip;
            if (mode >= TextRenderingMode.FillClip)
                Count("unsupported:text-clip");
            if (fill) {
                PaintWith(currentState, false, () => {
                    object brush = Brush(currentState.CurrentNonStrokingColor, currentState.AlphaConstantNonStroking);
                    if (brush != null)
                        target.FillPath(brush, path, AreaFillMode.Winding);
                });
            }
            if (stroke) {
                PaintWith(currentState, true, () => {
                    object pen = Pen(currentState);
                    if (pen != null)
                        target.DrawPath(pen, path);
                });
            }
        }

        // Add a glyph's subpath, in glyph space, to a path in the target's coordinates.
        private void AddSubpath(List<GraphicsPathPart> path, PdfSubpath subpath, PigMatrix glyphToPage)
        {
            Func<PdfPoint, PointF> map = point => {
                PdfPoint page = glyphToPage.Transform(point);
                PdfPoint mapped = toTarget.Transform(page);
                return new PointF((float)mapped.X, (float)mapped.Y);
            };
            foreach (PdfSubpath.IPathCommand command in subpath.Commands) {
                switch (command) {
                case PdfSubpath.Move move:
                    path.Add(new GraphicsPathPart(GraphicsPathPartKind.Start, new PointF[] { map(move.Location) }));
                    break;
                case PdfSubpath.Line line:
                    path.Add(new GraphicsPathPart(GraphicsPathPartKind.Lines, new PointF[] { map(line.To) }));
                    break;
                case PdfSubpath.CubicBezierCurve cubic:
                    path.Add(new GraphicsPathPart(GraphicsPathPartKind.Beziers, new PointF[] { map(cubic.FirstControlPoint), map(cubic.SecondControlPoint), map(cubic.EndPoint) }));
                    break;
                case PdfSubpath.QuadraticBezierCurve quadratic: {
                    // Raise the quadratic to a cubic: the controls a third of the way from each end to the quadratic's.
                    PdfPoint start = quadratic.StartPoint, control = quadratic.ControlPoint, end = quadratic.EndPoint;
                    PdfPoint first = new PdfPoint(start.X + 2.0 / 3 * (control.X - start.X), start.Y + 2.0 / 3 * (control.Y - start.Y));
                    PdfPoint second = new PdfPoint(end.X + 2.0 / 3 * (control.X - end.X), end.Y + 2.0 / 3 * (control.Y - end.Y));
                    path.Add(new GraphicsPathPart(GraphicsPathPartKind.Beziers, new PointF[] { map(first), map(second), map(end) }));
                    break;
                }
                case PdfSubpath.Close:
                    path.Add(new GraphicsPathPart(GraphicsPathPartKind.Close, new PointF[0]));
                    break;
                }
            }
        }

        // Images: decoded by PdfPig, a JPEG kept as its own data, then drawn through DrawBitmap over the unit square that
        // the CTM places, row 0 at its top, as the application draws bitmaps, smoothed.
        protected override void RenderXObjectImage(XObjectContentRecord xObjectContentRecord)
        {
            XObjectImage image = XObjectFactory.ReadImage(xObjectContentRecord, PdfScanner, FilterProvider, ResourceStore);
            DrawImage(image, xObjectContentRecord.AppliedTransformation);
        }

        protected override void RenderInlineImage(InlineImage inlineImage)
        {
            DrawImage(inlineImage, CurrentTransformationMatrix);
        }

        // Draw an image of PdfPig's under a transformation from the unit square to the page.
        private void DrawImage(IPdfImage image, PigMatrix unitToPage)
        {
            if (image.IsImageMask) {
                Count("unsupported:image-mask");
                return;
            }
            byte[] data = null;
            string kind;
            bool isJpeg = image.ImageDictionary.TryGet(NameToken.Filter, out IToken filter) && filter.ToString().Contains("DCT");
            if (isJpeg && image.ColorSpaceDetails != null && image.ColorSpaceDetails.BaseType != ColorSpace.DeviceCMYK) {
                data = image.RawMemory.ToArray();
                kind = "image:jpeg";
            }
            else if (image.TryGetPng(out byte[] png)) {
                data = png;
                kind = "image:png";
            }
            else {
                Count("unsupported:image-undecodable");
                return;
            }
            if (image.ImageDictionary.ContainsKey(NameToken.Smask))
                Count("unsupported:image-soft-mask");
            IGraphicsBitmap bitmap;
            using (MemoryStream stream = new MemoryStream(data))
                bitmap = Services.BitmapLoader.ReadBitmapFromStream(stream);
            int w = bitmap.PixelWidth, h = bitmap.PixelHeight;
            // Pixels to the unit square, row 0 at y = 1, then the unit square to the page and the page to the target.
            PigMatrix pixelsToTarget = PigMatrix.FromValues(1.0 / w, 0, 0, -1.0 / h, 0, 1).Multiply(unitToPage).Multiply(toTarget);
            using (Matrix matrix = new Matrix((float)pixelsToTarget.A, (float)pixelsToTarget.B, (float)pixelsToTarget.C,
                                              (float)pixelsToTarget.D, (float)pixelsToTarget.E, (float)pixelsToTarget.F)) {
                target.PushTransform(matrix);
                target.DrawBitmap(bitmap, new RectangleF(0, 0, w, h), BitmapScaling.MediumQuality);
                target.PopTransform();
            }
            Count(kind);
        }

        public override void PaintShading(NameToken shadingName)
        {
            Count("unsupported:shading");
        }

        public override void BeginMarkedContent(NameToken name, NameToken propertyDictionaryName, DictionaryToken properties)
        {
            Count("marked-content:" + name.Data);
        }

        public override void EndMarkedContent()
        {
        }
    }
}
