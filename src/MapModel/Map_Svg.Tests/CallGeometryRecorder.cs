using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using PurplePen.Graphics2D;
using PurplePen.MapModel;

namespace Map_Svg.Tests
{
    // Describes what each drawing call draws, as Graphics2D defines it, and what each palette color of a map covers, as
    // the intermediate layer defines it, in the coordinates of a page (V-10): a graphics target into which a case's
    // recording is played back, as it is played back into the SVG target, and which keeps a DrawnGeometry of each
    // drawing, in the order drawn. A path is described by its parts as Skia takes them, a segment with no point before it
    // starting at the origin; an arc by its ends, its radius and whether it is the larger and turns towards positive
    // angles on the page; an ellipse by its center and the ends of its axes; a text by its characters as a document holds
    // them, the start of its baseline, below its upper left corner by the font's ascent, and the point an em along it; a
    // bitmap by the corners of its rectangle. A drawing of nothing, a path with no point or a text with no character a
    // document can hold, is left out. The target of a pattern brush describes its tile likewise, in steps of the tile,
    // whose middle is its origin, as the SVG target draws it (D-7).
    //
    // The recorder gives the drawing of a map its own encoding (IPaletteStackEncodingProvider), which describes the
    // transparency group of each palette color as the document draws it (SvgEncoding): the clipping objects of its
    // palette color coverages at the resolution of the drawing, in their order, the consecutive clipping objects of a
    // repeated placement one pose at a time, each within the steps of its placement and a stroke with its dash pattern;
    // the run of dashes of an offset path, and with the setting for separate shapes each run of dashes, as its single
    // dashes (StrokedShape.ExpandDashes); and a hatching or a clipped area pattern as its pattern fill. A pattern fill is
    // the area, or a rectangle that bounds an area cluster within the clip of the union of its areas, filled with the
    // configuration of its palette cutter, when the group uses pattern brushes, and otherwise the expansion of its
    // coverage prototype within that clip. The clip of the union is a path for each area, each under its own rule. A configuration lies where the transformation M of the first palette color coverage of its
    // palette cutter that the page fills places it, as the document defines each configuration once, and its tile is the
    // definition of its coverage prototype: the clipping objects of the pattern tile of an area pattern, or the lines of
    // a hatching whose stripes reach into the square tile as wide as their spacing, each from a spacing before the middle
    // of the tile to a spacing after it, so that the tiles join without a seam. A stroke of no width is left out, as the
    // document leaves it out (D-5).
    public sealed class CallGeometryRecorder : IBrushTarget, IStructuredGraphicsTarget, IPaletteStackEncodingProvider, IPaletteStackEncoding
    {
        // The steps in a millimetre, which a pattern's tile is described in.
        private const double StepsPerMillimetre = 1000;

        // The mirror of the Y axis, which a text run lays out its text with (TextShape).
        private static readonly Affine MirrorOfY = new Affine(1, 0, 0, -1, 0, 0);

        // What a brush paints with: a color, or the tile of a pattern placed by a transform.
        private sealed class Paint
        {
            // A color, whatever it is: the geometry check compares where ink goes, not its color.
            public static readonly Paint Color = new Paint(null, 0, 0, Affine.Identity);

            // The pattern's tile, or null for a color.
            public readonly GeometryTile Tile;

            // The size of the tile, in the units of its own coordinates, whose origin is its middle.
            public readonly double Width, Height;

            // The transform from the tile's coordinates to those of what it paints.
            public readonly Affine TileToPaint;

            // Create a paint.
            // tile: the pattern's tile, or null for a color.
            // width, height: the size of the tile, in the units of its own coordinates.
            // tileToPaint: the transform from the tile's coordinates to those of what it paints.
            public Paint(GeometryTile tile, double width, double height, Affine tileToPaint)
            {
                Tile = tile;
                Width = width;
                Height = height;
                TileToPaint = tileToPaint;
            }
        }

        // A pen: the width of its stroke and what it paints with.
        private sealed class PenRecord
        {
            // The width of the stroke, in the units of what it strokes.
            public readonly float Width;

            // What the stroke paints with.
            public readonly Paint Paint;

            // Create a pen.
            // width: the width of its stroke.
            // paint: what its stroke paints with.
            public PenRecord(float width, Paint paint)
            {
                Width = width;
                Paint = paint;
            }
        }

        // A path created under a key: its parts and whether it is filled and clipped under the even-odd rule.
        private sealed class StoredPath
        {
            // The parts of the path.
            public readonly List<GraphicsPathPart> Parts;

            // Whether the path is filled and clipped under the even-odd rule rather than the nonzero one.
            public readonly bool EvenOdd;

            // Create a stored path.
            // parts: its parts.
            // evenOdd: whether it is filled and clipped under the even-odd rule.
            public StoredPath(List<GraphicsPathPart> parts, bool evenOdd)
            {
                Parts = parts;
                EvenOdd = evenOdd;
            }
        }

        // A font: its size, and its ascent, which puts the start of a text's baseline below its upper left corner.
        private sealed class FontRecord
        {
            // The size of the font, its em, and the ascent of its face, in the units of what it draws.
            public readonly float EmHeight, Ascent;

            // Create a font.
            // emHeight: its size.
            // ascent: the ascent of its face.
            public FontRecord(float emHeight, float ascent)
            {
                EmHeight = emHeight;
                Ascent = ascent;
            }
        }

        // The metrics the application lays text out with.
        private readonly ITextMetrics textMetrics;

        // The target that created this one as the target of a pattern brush, with the brush's size and angle; or null.
        private readonly CallGeometryRecorder creator;
        private readonly SizeF tileSize;
        private readonly float tileAngle;

        // The transform from the current frame to the page, and those of the frames around it.
        private Affine current;
        private readonly Stack<Affine> transforms = new Stack<Affine>();

        // The clips around what is drawn, outermost first, and those before each clip still open.
        private DrawnGeometry[] clips = Array.Empty<DrawnGeometry>();
        private readonly Stack<DrawnGeometry[]> clipsBefore = new Stack<DrawnGeometry[]>();

        // The paths, brushes, pens and fonts, by their keys.
        private readonly Dictionary<object, StoredPath> paths = new Dictionary<object, StoredPath>(ReferenceEqualityComparer.Instance);

        private readonly Dictionary<object, Paint> brushes = new Dictionary<object, Paint>(ReferenceEqualityComparer.Instance);
        private readonly Dictionary<object, PenRecord> pens = new Dictionary<object, PenRecord>(ReferenceEqualityComparer.Instance);
        private readonly Dictionary<object, FontRecord> fonts = new Dictionary<object, FontRecord>(ReferenceEqualityComparer.Instance);

        // The configurations of the palette cutters that the page fills, by palette cutter, and the tiles of the
        // definitions of their coverage prototypes, by coverage prototype, each described once, as the document defines
        // each once.
        private readonly Dictionary<PaletteCutter, Paint> configurations = new Dictionary<PaletteCutter, Paint>();
        private readonly Dictionary<CoveragePrototype, GeometryTile> definitionTiles = new Dictionary<CoveragePrototype, GeometryTile>();

        // The symbol keys of the palette color coverage being described, which name its drawings in messages, or null.
        private string describedCoverage;

        // The number of drawings made, which names each in messages.
        private int callsMade;

        // The intensity, which only colors depend on.
        private float intensity = 1;

        // The drawings, in the order of the calls.
        public readonly List<DrawnGeometry> Drawings = new List<DrawnGeometry>();

        // Create a recorder of the calls that draw a page.
        // textMetrics: the metrics the application lays text out with.
        // toPage: the transform from the drawing's coordinates to the page's.
        public CallGeometryRecorder(ITextMetrics textMetrics, Affine toPage)
        {
            this.textMetrics = textMetrics;
            current = toPage;
        }

        // Create the recorder of a pattern brush's tile, in steps of the tile.
        // creator: the recorder that creates the brush.
        // size: the size of the tile, in the units of what it paints.
        // angle: the angle it is turned by, in degrees.
        private CallGeometryRecorder(CallGeometryRecorder creator, SizeF size, float angle)
            : this(creator.textMetrics, new Affine(StepsPerMillimetre, 0, 0, StepsPerMillimetre, 0, 0))
        {
            this.creator = creator;
            tileSize = size;
            tileAngle = angle;
        }

        // The clip of a rectangle under a transform, as a page clips its parts to the area of the map it shows (L-3).
        // area: the rectangle.
        // toPage: the transform from its coordinates to the page's.
        // what: what the clip is, for messages.
        public static DrawnGeometry RectangleClip(RectangleF area, Affine toPage, string what)
        {
            DrawnGeometry clip = new DrawnGeometry(what);
            clip.Signature.Append("clip nonzero");
            AddRectangle(new PathGeometryBuilder(clip), area, toPage);
            return clip;
        }

        // The intensity at which colors are drawn; setting it discards the brushes and pens, as every target does.
        public float Intensity
        {
            get { return intensity; }
            set {
                brushes.Clear();
                pens.Clear();
                intensity = value;
            }
        }

        // Prepend a transform to the current one.
        public void PushTransform(Matrix matrix)
        {
            transforms.Push(current);
            current = current.After(Affine.FromMatrix(matrix));
        }

        // Go back to the transform before the last PushTransform.
        public void PopTransform()
        {
            current = transforms.Pop();
        }

        // Clip to a path created with CreatePath, under its rule.
        public void PushClip(object pathKey)
        {
            StoredPath path = paths[pathKey];
            DrawnGeometry clip = BeginClip(path.EvenOdd);
            AddParts(new PathGeometryBuilder(clip), path.Parts);
            OpenClip(clip);
        }

        // Clip to a rectangle.
        public void PushClip(RectangleF rectangle)
        {
            DrawnGeometry clip = BeginClip(false);
            AddRectangle(new PathGeometryBuilder(clip), rectangle, current);
            OpenClip(clip);
        }

        // Clip to the union of rectangles, one subpath each.
        public void PushClip(RectangleF[] rectangles)
        {
            DrawnGeometry clip = BeginClip(false);
            PathGeometryBuilder path = new PathGeometryBuilder(clip);
            foreach (RectangleF rectangle in rectangles)
                AddRectangle(path, rectangle, current);
            OpenClip(clip);
        }

        // Clip to a path given by its parts, under the rule of its fill mode.
        public void PushClip(List<GraphicsPathPart> parts, AreaFillMode windingMode)
        {
            DrawnGeometry clip = BeginClip(windingMode == AreaFillMode.Alternate);
            AddParts(new PathGeometryBuilder(clip), parts);
            OpenClip(clip);
        }

        // Go back to the clips before the last PushClip.
        public void PopClip()
        {
            clips = clipsBefore.Pop();
        }

        // Anti-aliasing changes nothing that the check compares.
        public void PushAntiAliasing(bool antiAlias)
        {
        }

        // Anti-aliasing changes nothing that the check compares.
        public void PopAntiAliasing()
        {
        }

        // Blending changes nothing that the check compares; it is taken, as the SVG target takes darken.
        public bool PushBlending(BlendMode blendMode)
        {
            return true;
        }

        // Blending changes nothing that the check compares.
        public void PopBlending()
        {
        }

        // Create a path under a key.
        public void CreatePath(object key, List<GraphicsPathPart> parts, AreaFillMode windingMode)
        {
            paths.Add(key, new StoredPath(parts, windingMode == AreaFillMode.Alternate));
        }

        // Create a brush of a color under a key.
        public void CreateSolidBrush(object key, CmykColor color)
        {
            brushes.Add(key, Paint.Color);
        }

        // Pattern brushes are supported, as the SVG target supports them.
        public bool SupportsPatternBrushes
        {
            get { return true; }
        }

        // Create a pattern brush: return the recorder of its tile, which FinishBrush makes a brush of this one.
        public IBrushTarget CreatePatternBrush(SizeF size, float angle, int bitmapWidth, int bitmapHeight)
        {
            return new CallGeometryRecorder(this, size, angle);
        }

        // Make the tile a brush of the recorder that created it, under a key.
        public void FinishBrush(object brushKey)
        {
            List<DrawnGeometry> tileDrawings = Drawings;
            GeometryTile tile = new GeometryTile("the tile of a pattern brush", () => tileDrawings);
            creator.brushes.Add(brushKey, new Paint(tile, tileSize.Width, tileSize.Height, Affine.Rotation(tileAngle)));
        }

        // Create a pen that strokes with a brush's paint, under a key.
        public void CreatePen(object key, object brushKey, float width, LineCapMode caps, LineJoinMode join, float miterLimit)
        {
            pens.Add(key, new PenRecord(width, brushes[brushKey]));
        }

        // Create a pen of a color, under a key.
        public void CreatePen(object key, CmykColor color, float width, LineCapMode caps, LineJoinMode join, float miterLimit)
        {
            pens.Add(key, new PenRecord(width, Paint.Color));
        }

        // Create a font under a key, with the ascent of its face in the metrics the application lays text out with.
        public void CreateFont(object key, string familyName, float emHeight, TextEffects effects)
        {
            using (ITextFaceMetrics metrics = textMetrics.GetTextFaceMetrics(familyName, emHeight, effects))
                fonts.Add(key, new FontRecord(emHeight, metrics.Ascent));
        }

        // Whether a path is created under a key.
        public bool HasPath(object pathKey)
        {
            return paths.ContainsKey(pathKey);
        }

        // Whether a brush is created under a key.
        public bool HasBrush(object brushKey)
        {
            return brushes.ContainsKey(brushKey);
        }

        // Whether a pen is created under a key.
        public bool HasPen(object penKey)
        {
            return pens.ContainsKey(penKey);
        }

        // Whether a font is created under a key.
        public bool HasFont(object fontKey)
        {
            return fonts.ContainsKey(fontKey);
        }

        // The encoding that describes the palette colors of a map drawn into the recorder: the recorder itself.
        public IPaletteStackEncoding PaletteStackEncoding
        {
            get { return this; }
        }

        // Describe the transparency group of a palette color as the document draws it, in the frame and the clips around
        // the drawing of the map: its palette color coverages in their order, the full coverage as its clipping objects at
        // the resolution of the drawing, and a hatching or a clipped area pattern as its pattern fill, of each area, or of
        // an area cluster of several areas once, at the first of its palette color coverages that the group gives, unless
        // each area writes its own.
        // group: the transparency group.
        public void WriteTransparencyGroup(TransparencyGroup group)
        {
            HashSet<AreaCluster> filledClusters = new HashSet<AreaCluster>();
            foreach (SymbolCoverage coverage in group.SymbolCoverages) {
                describedCoverage = coverage.Symbol.Key.Text;
                if (coverage.Prototype == CoveragePrototype.FullCoverage) {
                    DescribeClippingObjects(coverage.GetClippingObjects(group.Resolution), group.PatternsWithEachArea);
                    continue;
                }

                AreaCluster cluster = group.GetAreaCluster(coverage);
                if (group.PatternsWithEachArea || cluster.SymbolCoverages.Count == 1) {
                    foreach (ClippingObject area in coverage.ClippingObjects)
                        DescribePatternFill(group, coverage, cluster.PaletteCutter, new ClippingObject[] { area }, null);
                }
                else if (filledClusters.Add(cluster)) {
                    describedCoverage = "the area cluster of " + coverage.Symbol.Key.Text;
                    List<ClippingObject> areas = new List<ClippingObject>();
                    foreach (SymbolCoverage clustered in cluster.SymbolCoverages)
                        areas.AddRange(clustered.ClippingObjects);
                    DescribePatternFill(group, cluster.SymbolCoverages[0], cluster.PaletteCutter, areas, cluster.Bounds);
                }
            }
            describedCoverage = null;
        }

        // Describe the pattern fill of a hatching or a clipped area pattern in an area, or in the union of the areas of an
        // area cluster. When the group uses pattern brushes and the coverage prototype is a hatching or a regular area
        // pattern, it is the area filled with the configuration of the palette cutter, or a rectangle that bounds the
        // cluster filled with it within the clip of the union; otherwise it is the expansion of the coverage prototype
        // within the clip of the area or of the union. An area with no point fills nothing.
        // group: the transparency group.
        // coverage: the palette color coverage whose transformation M places the pattern: the coverage of the area, or
        //   the first of the cluster.
        // cutter: the palette cutter of the coverage.
        // areas: the area, or the areas of the cluster.
        // clusterBounds: the bounds of the cluster, over which the coverage prototype is expanded, or null for an area.
        private void DescribePatternFill(TransparencyGroup group, SymbolCoverage coverage, PaletteCutter cutter, IReadOnlyList<ClippingObject> areas,
                                         RectangleF? clusterBounds)
        {
            Paint configuration = UsesPatternFill(group, coverage.Prototype) ? ConfigurationOf(cutter, coverage) : null;
            if (configuration != null && clusterBounds == null) {
                FilledShape area = AreaOf(areas[0]);
                AddPath(BeginFill("PatternFill", area.FillRule == AreaFillMode.Alternate), ((PathShape)area.Shape).Parts, configuration);
                return;
            }

            DrawnGeometry clip = ClipOfAreas(areas);
            if (clip == null)
                return;
            OpenClip(clip);
            if (configuration != null) {
                DrawnGeometry drawing = BeginFill("PatternFill", false);
                AddRectangle(new PathGeometryBuilder(drawing), clusterBounds.Value, current);
                Finish(drawing, configuration);
            }
            else {
                DescribeClippingObjects(clusterBounds.HasValue ? coverage.ExpandPrototype(clusterBounds.Value) : coverage.ExpandPrototype(),
                                        group.PatternsWithEachArea);
            }
            PopClip();
        }

        // The clip of areas, as the document writes it: one area under its rule, or a union of the paths of several areas,
        // each under its own rule; or null when no area has a point, as the document leaves out an area without one.
        // areas: the areas.
        private DrawnGeometry ClipOfAreas(IReadOnlyList<ClippingObject> areas)
        {
            List<FilledShape> drawn = new List<FilledShape>();
            foreach (ClippingObject clippingObject in areas) {
                FilledShape area = AreaOf(clippingObject);
                foreach (GraphicsPathPart part in ((PathShape)area.Shape).Parts) {
                    if (part.Kind != GraphicsPathPartKind.Close) {
                        drawn.Add(area);
                        break;
                    }
                }
            }
            if (drawn.Count == 0)
                return null;

            DrawnGeometry clip = BeginClip("the clip of the pattern fill", drawn.Count == 1 ? ClipKind(drawn[0].FillRule == AreaFillMode.Alternate) : "clip union");
            foreach (FilledShape area in drawn) {
                if (drawn.Count > 1)
                    clip.Signature.Append(area.FillRule == AreaFillMode.Alternate ? "; even-odd" : "; nonzero");
                AddParts(new PathGeometryBuilder(clip), ((PathShape)area.Shape).Parts);
            }
            return clip;
        }

        // The area of a hatching or a clipped area pattern: its clipping object, a filled path at the identity placement.
        // clippingObject: the clipping object.
        private static FilledShape AreaOf(ClippingObject clippingObject)
        {
            FilledShape area = clippingObject as FilledShape;
            if (area == null || !(area.Shape is PathShape) || area.Placement.Steps.Count != 0)
                throw new InvalidOperationException("The clipping object of a pattern fill is the area, a filled path at the identity placement.");
            return area;
        }

        // Whether the coverage prototype of a palette color coverage is a pattern fill of a configuration: when the group
        // uses pattern brushes, a hatching whose lines have a spacing, and a regular area pattern whose element has a size.
        // group: the transparency group.
        // prototype: the coverage prototype.
        private static bool UsesPatternFill(TransparencyGroup group, CoveragePrototype prototype)
        {
            if (!group.UsePatternBrushes)
                return false;
            HatchingPrototype hatching = prototype as HatchingPrototype;
            if (hatching != null)
                return hatching.Hatching.hatchSpacing > 0;
            AreaPatternPrototype areaPattern = prototype as AreaPatternPrototype;
            if (areaPattern != null) {
                AreaSymDef.PatternInfo pattern = areaPattern.Pattern;
                return !pattern.irregular && pattern.patternWidth > 0 && pattern.patternHeight > 0;
            }
            return false;
        }

        // The configuration of a palette cutter as a paint: the tile of the definition of its coverage prototype, placed
        // by the transformation M of the first palette color coverage of the palette cutter that the page fills, and for
        // an area pattern by M · T(-offset), as its pattern tile includes the offset of the area pattern (PatternTile).
        // cutter: the palette cutter.
        // coverage: the palette color coverage that the configuration fills.
        private Paint ConfigurationOf(PaletteCutter cutter, SymbolCoverage coverage)
        {
            Paint configuration;
            if (configurations.TryGetValue(cutter, out configuration))
                return configuration;

            Affine tileToArea;
            using (Matrix transformation = coverage.Transformation)
                tileToArea = Affine.FromMatrix(transformation);
            double width, height;
            AreaPatternPrototype areaPattern = cutter.Prototype as AreaPatternPrototype;
            if (areaPattern != null) {
                AreaSymDef.PatternInfo pattern = areaPattern.Pattern;
                tileToArea = tileToArea.After(new Affine(1, 0, 0, 1, -pattern.patternOffsetX, -pattern.patternOffsetY));
                width = areaPattern.Tile.Size.Width;
                height = areaPattern.Tile.Size.Height;
            }
            else {
                width = height = ((HatchingPrototype)cutter.Prototype).Hatching.hatchSpacing;
            }
            configuration = new Paint(TileOfDefinition(cutter.Prototype), width, height, tileToArea);
            configurations.Add(cutter, configuration);
            return configuration;
        }

        // The tile of the definition of a coverage prototype, in steps of the tile, whose middle is its origin: the
        // clipping objects of the pattern tile of an area pattern, or the lines y = k · spacing of a hatching whose stripes
        // reach into the square tile as wide as the spacing, each from x = -spacing to x = spacing.
        // prototype: the coverage prototype.
        private GeometryTile TileOfDefinition(CoveragePrototype prototype)
        {
            GeometryTile tile;
            if (definitionTiles.TryGetValue(prototype, out tile))
                return tile;

            CallGeometryRecorder tileRecorder = new CallGeometryRecorder(this, SizeF.Empty, 0);
            HatchingPrototype hatching = prototype as HatchingPrototype;
            if (hatching != null) {
                AreaSymDef.HatchInfo hatchInfo = hatching.Hatching;
                double spacing = hatchInfo.hatchSpacing, halfWidth = hatchInfo.hatchWidth / 2.0;
                int linesEachWay = (int)Math.Ceiling(halfWidth / spacing + 0.5);
                for (int line = -linesEachWay; line <= linesEachWay; ++line) {
                    double y = line * spacing;
                    if (y - halfWidth < spacing / 2 && y + halfWidth > -spacing / 2) {
                        DrawnGeometry drawing = tileRecorder.BeginStroke("a line of the hatching", hatchInfo.hatchWidth);
                        PathGeometryBuilder path = new PathGeometryBuilder(drawing);
                        path.MoveTo(tileRecorder.current.Apply(-spacing, y), 0);
                        path.LineTo(tileRecorder.current.Apply(spacing, y), 0);
                        tileRecorder.Finish(drawing, Paint.Color);
                    }
                }
            }
            else {
                tileRecorder.DescribeClippingObjects(((AreaPatternPrototype)prototype).Tile.ClippingObjects, false);
            }
            List<DrawnGeometry> drawings = tileRecorder.Drawings;
            tile = new GeometryTile("the tile of the " + prototype, () => drawings);
            definitionTiles.Add(prototype, tile);
            return tile;
        }

        // Describe clipping objects in their order, each at its placement: the consecutive clipping objects of one
        // repeated placement one pose at a time, in their order at each pose, as the encodings write a glyph at a time.
        // clippingObjects: the clipping objects.
        // dashesApart: whether each run of dashes is described as its single dashes.
        private void DescribeClippingObjects(IReadOnlyList<ClippingObject> clippingObjects, bool dashesApart)
        {
            int index = 0;
            while (index < clippingObjects.Count) {
                RepeatedPlacement repeated = clippingObjects[index].Placement as RepeatedPlacement;
                int last = index;
                if (repeated == null) {
                    DescribeAt(clippingObjects[index], (SinglePlacement)clippingObjects[index].Placement, dashesApart);
                }
                else {
                    while (last + 1 < clippingObjects.Count && clippingObjects[last + 1].Placement == repeated)
                        ++last;
                    foreach (SinglePlacement placement in repeated.Expand()) {
                        for (int k = index; k <= last; ++k)
                            DescribeAt(clippingObjects[k], placement, dashesApart);
                    }
                }
                index = last + 1;
            }
        }

        // Describe a clipping object at a single placement: within the steps of the placement, and a text run with the
        // mirror of its Y axis within them (TextShape).
        // clippingObject: the clipping object.
        // placement: the single placement.
        // dashesApart: whether a run of dashes is described as its single dashes.
        private void DescribeAt(ClippingObject clippingObject, SinglePlacement placement, bool dashesApart)
        {
            Affine around = current;
            foreach (Matrix step in placement.Steps) {
                using (step)
                    current = current.After(Affine.FromMatrix(step));
            }
            if (clippingObject.Shape is TextShape)
                current = current.After(MirrorOfY);
            DescribeShape(clippingObject, dashesApart);
            current = around;
        }

        // Describe the shape of a clipping object in the current frame: filled by its fill rule, or stroked with its pen.
        // An offset path is its offset curve. The run of dashes of an offset path, and with dashesApart each run of
        // dashes, is its single dashes, as SVG measures a dash pattern along the element's own path, and the dash pattern
        // of an offset path is measured along its base path.
        // clippingObject: the clipping object.
        // dashesApart: whether a run of dashes is described as its single dashes.
        private void DescribeShape(ClippingObject clippingObject, bool dashesApart)
        {
            Shape shape = clippingObject.Shape;
            FilledShape filled = clippingObject as FilledShape;
            if (filled != null) {
                DescribeFill(shape, filled.FillRule == AreaFillMode.Alternate);
                return;
            }

            StrokedShape stroked = (StrokedShape)clippingObject;
            if (stroked.Stroke.Width == 0)
                return;
            OffsetPathShape offsetPath = shape as OffsetPathShape;
            if (stroked.Stroke.DashPattern != null && !(shape is TextShape) && (dashesApart || offsetPath != null)) {
                foreach (StrokedShape dash in stroked.ExpandDashes()) {
                    OffsetPathShape offsetDash = dash.Shape as OffsetPathShape;
                    DescribeStroke((offsetDash != null) ? offsetDash.OffsetCurve : dash.Shape, dash.Stroke);
                }
                return;
            }
            DescribeStroke((offsetPath != null) ? offsetPath.OffsetCurve : shape, stroked.Stroke);
        }

        // Describe a filled shape in the current frame: a path or an ellipse under a fill rule, or a text.
        // shape: the shape.
        // evenOdd: whether it is filled under the even-odd rule rather than the nonzero one.
        private void DescribeFill(Shape shape, bool evenOdd)
        {
            PathShape path = shape as PathShape;
            if (path != null) {
                AddPath(BeginFill(nameof(FilledShape), evenOdd), path.Parts, Paint.Color);
                return;
            }

            EllipseShape ellipse = shape as EllipseShape;
            if (ellipse != null) {
                DrawnGeometry drawing = BeginFill(nameof(FilledShape), evenOdd);
                AddEllipse(drawing, ellipse.Center, ellipse.RadiusX, ellipse.RadiusY);
                Finish(drawing, Paint.Color);
                return;
            }

            TextShape text = shape as TextShape;
            if (text != null) {
                DescribeText(text, "text", null);
                return;
            }
            throw new ArgumentException("A filled shape is a path, an ellipse or a text run.", nameof(shape));
        }

        // Describe a stroked shape in the current frame: a path, an ellipse or an arc with the width of the pen and its
        // dash pattern, in steps of the shape's coordinates, or the outline of a text.
        // shape: the shape.
        // stroke: the pen.
        private void DescribeStroke(Shape shape, Stroke stroke)
        {
            TextShape text = shape as TextShape;
            if (text != null) {
                DescribeText(text, "text outline", stroke.Width);
                return;
            }

            DrawnGeometry drawing = BeginStroke(nameof(StrokedShape), stroke.Width);
            if (stroke.DashPattern != null) {
                List<double> dashArray = new List<double>();
                foreach (float length in stroke.DashPattern)
                    dashArray.Add(length * StepsPerMillimetre);
                drawing.AddDashPattern(dashArray, stroke.DashPhase * StepsPerMillimetre, 0);
            }

            PathShape path = shape as PathShape;
            if (path != null) {
                AddPath(drawing, path.Parts, Paint.Color);
                return;
            }

            EllipseShape ellipse = shape as EllipseShape;
            if (ellipse != null) {
                AddEllipse(drawing, ellipse.Center, ellipse.RadiusX, ellipse.RadiusY);
                Finish(drawing, Paint.Color);
                return;
            }

            ArcShape arc = shape as ArcShape;
            if (arc != null) {
                AddArc(drawing, arc.Center, arc.Radius, arc.StartAngle, arc.SweepAngle);
                Finish(drawing, Paint.Color);
                return;
            }
            throw new ArgumentException("A stroked shape is a path, an ellipse, an arc or a text run.", nameof(shape));
        }

        // Describe a text run in the current frame, which has the mirror of its Y axis: the text that the drawing draws at
        // its top left corner with the opposite y coordinate, in its font.
        // text: the text run.
        // kind: the kind and paint of what it draws.
        // penWidth: the width of the pen of its outline, or null for a filled text.
        private void DescribeText(TextShape text, string kind, float? penWidth)
        {
            float ascent;
            using (ITextFaceMetrics metrics = textMetrics.GetTextFaceMetrics(text.FontFamily, text.EmHeight, text.Effects))
                ascent = metrics.Ascent;
            AddText(nameof(TextShape), kind, text.Text, text.EmHeight, ascent, new PointF(text.UpperLeft.X, -text.UpperLeft.Y), Paint.Color, penWidth);
        }

        // A line.
        public void DrawLine(object penKey, PointF start, PointF finish)
        {
            DrawnGeometry drawing = BeginStroke(nameof(DrawLine), penKey);
            PathGeometryBuilder path = new PathGeometryBuilder(drawing);
            path.MoveTo(current.Apply(start.X, start.Y), 0);
            path.LineTo(current.Apply(finish.X, finish.Y), 0);
            Finish(drawing, pens[penKey].Paint);
        }

        // An arc, its angles in degrees from the x axis towards the y axis; a full turn or more is the whole circle.
        public void DrawArc(object penKey, PointF center, float radius, float startAngle, float sweepAngle)
        {
            DrawnGeometry drawing = BeginStroke(nameof(DrawArc), penKey);
            AddArc(drawing, center, radius, startAngle, sweepAngle);
            Finish(drawing, pens[penKey].Paint);
        }

        // The outline of an ellipse.
        public void DrawEllipse(object penKey, PointF center, float radiusX, float radiusY)
        {
            DrawnGeometry drawing = BeginStroke(nameof(DrawEllipse), penKey);
            AddEllipse(drawing, center, radiusX, radiusY);
            Finish(drawing, pens[penKey].Paint);
        }

        // A filled ellipse.
        public void FillEllipse(object brushKey, PointF center, float radiusX, float radiusY)
        {
            DrawnGeometry drawing = BeginFill(nameof(FillEllipse), false);
            AddEllipse(drawing, center, radiusX, radiusY);
            Finish(drawing, brushes[brushKey]);
        }

        // The outline of a rectangle.
        public void DrawRectangle(object penKey, RectangleF rect)
        {
            DrawnGeometry drawing = BeginStroke(nameof(DrawRectangle), penKey);
            AddRectangle(new PathGeometryBuilder(drawing), rect, current);
            Finish(drawing, pens[penKey].Paint);
        }

        // A filled rectangle.
        public void FillRectangle(object brushKey, RectangleF rect)
        {
            DrawnGeometry drawing = BeginFill(nameof(FillRectangle), false);
            AddRectangle(new PathGeometryBuilder(drawing), rect, current);
            Finish(drawing, brushes[brushKey]);
        }

        // The outline of a closed polygon.
        public void DrawPolygon(object penKey, PointF[] pts)
        {
            if (pts.Length == 0)
                return;
            DrawnGeometry drawing = BeginStroke(nameof(DrawPolygon), penKey);
            AddPolygon(drawing, pts, true);
            Finish(drawing, pens[penKey].Paint);
        }

        // An open polyline.
        public void DrawPolyline(object penKey, PointF[] pts)
        {
            if (pts.Length == 0)
                return;
            DrawnGeometry drawing = BeginStroke(nameof(DrawPolyline), penKey);
            AddPolygon(drawing, pts, false);
            Finish(drawing, pens[penKey].Paint);
        }

        // A filled polygon, under the rule of its fill mode.
        public void FillPolygon(object brushKey, PointF[] pts, AreaFillMode windingMode)
        {
            if (pts.Length == 0)
                return;
            DrawnGeometry drawing = BeginFill(nameof(FillPolygon), windingMode == AreaFillMode.Alternate);
            AddPolygon(drawing, pts, true);
            Finish(drawing, brushes[brushKey]);
        }

        // The outline of a path created with CreatePath.
        public void DrawPath(object penKey, object pathKey)
        {
            AddPath(BeginStroke(nameof(DrawPath), penKey), paths[pathKey].Parts, pens[penKey].Paint);
        }

        // The outline of a path given by its parts.
        public void DrawPath(object penKey, List<GraphicsPathPart> parts)
        {
            AddPath(BeginStroke(nameof(DrawPath), penKey), parts, pens[penKey].Paint);
        }

        // A filled path created with CreatePath, under its rule.
        public void FillPath(object brushKey, object pathKey)
        {
            StoredPath path = paths[pathKey];
            AddPath(BeginFill(nameof(FillPath), path.EvenOdd), path.Parts, brushes[brushKey]);
        }

        // Beginning a part, which draws nothing, changes nothing that the check compares.
        public void BeginPart(object partKey)
        {
        }

        // Ending a part changes nothing that the check compares.
        public void EndPart()
        {
        }

        // Beginning a map changes nothing that the check compares.
        public void BeginMap(object mapKey, string label)
        {
        }

        // Ending a map changes nothing that the check compares.
        public void EndMap()
        {
        }

        // Beginning an object changes nothing that the check compares.
        public void BeginObject(string objectKey, object symbolKey, string symbolId, string symbolName)
        {
        }

        // Ending an object changes nothing that the check compares.
        public void EndObject()
        {
        }

        // Adding a page, which the pages' own clips are compared through, changes nothing that the check compares.
        public void AddPage(SizeF paperSize, RectangleF mapArea, Matrix mapToPage, object[] partKeys, string name)
        {
        }

        // Where a map lies in the real world changes nothing that the check compares.
        public void SetGeoreference(object mapKey, MapGeoreference georeference)
        {
        }

        // The course a map draws changes nothing that the check compares.
        public void DescribeCourse(object mapKey, CourseRecord course)
        {
        }

        // The course's objects change nothing that the check compares.
        public void DescribeObjects(object mapKey, IReadOnlyDictionary<string, CourseObjectRecord> objects)
        {
        }

        // Pattern brushes are vectors, as the SVG target's are, though the map code draws nothing into the recorder.
        public bool PatternBrushesAreVectors
        {
            get { return true; }
        }

        // A filled path given by its parts, under the rule of its fill mode.
        public void FillPath(object brushKey, List<GraphicsPathPart> parts, AreaFillMode windingMode)
        {
            AddPath(BeginFill(nameof(FillPath), windingMode == AreaFillMode.Alternate), parts, brushes[brushKey]);
        }

        // A filled text.
        public void DrawText(string text, object fontKey, object brushKey, PointF upperLeft)
        {
            FontRecord font = fonts[fontKey];
            AddText(nameof(DrawText), "text", text, font.EmHeight, font.Ascent, upperLeft, brushes[brushKey], null);
        }

        // The outline of a text.
        public void DrawTextOutline(string text, object fontKey, object penKey, PointF upperLeft)
        {
            FontRecord font = fonts[fontKey];
            PenRecord pen = pens[penKey];
            AddText(nameof(DrawTextOutline), "text outline", text, font.EmHeight, font.Ascent, upperLeft, pen.Paint, pen.Width);
        }

        // A bitmap over a rectangle.
        public void DrawBitmap(IGraphicsBitmap bm, RectangleF rectangle, BitmapScaling scalingMode)
        {
            AddImage(nameof(DrawBitmap), rectangle);
        }

        // Part of a bitmap over a rectangle.
        public void DrawBitmapPart(IGraphicsBitmap bm, int x, int y, int width, int height, RectangleF rectangle, BitmapScaling scalingMode)
        {
            AddImage(nameof(DrawBitmapPart), rectangle);
        }

        // Nothing to release.
        public void Dispose()
        {
        }

        // Begin the geometry of a drawing: of a call, or of a clipping object of the palette color coverage being
        // described.
        // callName: the name of the call, or of the kind of clipping object.
        // kind: the kind and paint of what it draws.
        private DrawnGeometry Begin(string callName, string kind)
        {
            ++callsMade;
            DrawnGeometry drawing = new DrawnGeometry((describedCoverage == null ? callName : callName + " of " + describedCoverage) +
                                                      ", call " + callsMade.ToString(CultureInfo.InvariantCulture) +
                                                      (creator == null ? "" : " of a pattern's tile"));
            drawing.Signature.Append(kind);
            drawing.Clips = clips;
            return drawing;
        }

        // Begin the geometry of a call that strokes with a pen: its width, scaled to the page.
        // callName: the name of the call.
        // penKey: the key of the pen.
        private DrawnGeometry BeginStroke(string callName, object penKey)
        {
            return BeginStroke(callName, pens[penKey].Width);
        }

        // Begin the geometry of a drawing that strokes with a pen of a width: the width, scaled to the page.
        // callName: the name of the call, or of the kind of clipping object.
        // width: the width of the pen, in the units of what it strokes.
        private DrawnGeometry BeginStroke(string callName, float width)
        {
            DrawnGeometry drawing = Begin(callName, "stroke");
            drawing.AddLength(width * current.LengthScale, 0);
            return drawing;
        }

        // Begin the geometry of a call that fills under a rule.
        // callName: the name of the call.
        // evenOdd: whether it fills under the even-odd rule rather than the nonzero one.
        private DrawnGeometry BeginFill(string callName, bool evenOdd)
        {
            return Begin(callName, evenOdd ? "fill even-odd" : "fill nonzero");
        }

        // Begin the geometry of a clip under a rule.
        // evenOdd: whether it clips under the even-odd rule rather than the nonzero one.
        private DrawnGeometry BeginClip(bool evenOdd)
        {
            return BeginClip("PushClip", ClipKind(evenOdd));
        }

        // The kind of a clip of one path under a rule, which begins its signature.
        // evenOdd: whether it clips under the even-odd rule rather than the nonzero one.
        private static string ClipKind(bool evenOdd)
        {
            return evenOdd ? "clip even-odd" : "clip nonzero";
        }

        // Begin the geometry of a clip: of a call, or of a pattern fill of the palette color coverage being described.
        // callName: the name of the call, or what the clip is.
        // kind: the kind of the clip, which begins its signature.
        private DrawnGeometry BeginClip(string callName, string kind)
        {
            ++callsMade;
            DrawnGeometry clip = new DrawnGeometry((describedCoverage == null ? callName : callName + " of " + describedCoverage) +
                                                   ", call " + callsMade.ToString(CultureInfo.InvariantCulture) +
                                                   (creator == null ? "" : " of a pattern's tile"));
            clip.Signature.Append(kind);
            return clip;
        }

        // Open a clip around what is drawn until the matching PopClip.
        // clip: the clip.
        private void OpenClip(DrawnGeometry clip)
        {
            clipsBefore.Push(clips);
            DrawnGeometry[] opened = new DrawnGeometry[clips.Length + 1];
            clips.CopyTo(opened, 0);
            opened[clips.Length] = clip;
            clips = opened;
        }

        // Finish a drawing that paints with a paint, and keep it: a pattern's paint adds its lattice, the tile's middle
        // and the ends of half its sides, placed by the paint's transform in the frame of what it paints.
        // drawing: the drawing.
        // paint: what it paints with.
        private void Finish(DrawnGeometry drawing, Paint paint)
        {
            if (paint.Tile != null) {
                drawing.Signature.Append(" pattern");
                Affine lattice = current.After(paint.TileToPaint);
                drawing.AddPoint(lattice.Apply(0, 0), 0);
                drawing.AddPoint(lattice.Apply(paint.Width / 2.0, 0), 0);
                drawing.AddPoint(lattice.Apply(0, paint.Height / 2.0), 0);
                drawing.Pattern = paint.Tile;
            }
            Drawings.Add(drawing);
        }

        // Add an ellipse: its center and the ends of its axes.
        // drawing: the drawing.
        // center: the center.
        // radiusX, radiusY: the radii.
        private void AddEllipse(DrawnGeometry drawing, PointF center, float radiusX, float radiusY)
        {
            drawing.Signature.Append(" ellipse");
            drawing.AddPoint(current.Apply(center.X, center.Y), 0);
            drawing.AddPoint(current.Apply(center.X + Math.Abs((double)radiusX), center.Y), 0);
            drawing.AddPoint(current.Apply(center.X, center.Y + Math.Abs((double)radiusY)), 0);
        }

        // Add an arc, its angles in degrees from the x axis towards the y axis: its start, then its end with its radius
        // and whether it is the larger and turns towards positive angles on the page; a full turn or more is the whole
        // circle.
        // drawing: the drawing.
        // center: the center.
        // radius: the radius.
        // startAngle: the angle of the start.
        // sweepAngle: the angle from the start to the end.
        private void AddArc(DrawnGeometry drawing, PointF center, float radius, float startAngle, float sweepAngle)
        {
            if (Math.Abs(sweepAngle) >= 360) {
                AddEllipse(drawing, center, radius, radius);
                return;
            }
            double start = startAngle * Math.PI / 180, end = (startAngle + sweepAngle) * Math.PI / 180;
            PathGeometryBuilder path = new PathGeometryBuilder(drawing);
            path.MoveTo(current.Apply(center.X + radius * Math.Cos(start), center.Y + radius * Math.Sin(start)), 0);
            path.ArcTo(Math.Abs(radius) * current.LengthScale, 0, Math.Abs(sweepAngle) > 180, (sweepAngle > 0) != current.Mirrors,
                       current.Apply(center.X + radius * Math.Cos(end), center.Y + radius * Math.Sin(end)), 0);
        }

        // Add a polygon or a polyline through points.
        // drawing: the drawing.
        // points: the points.
        // close: whether the path closes.
        private void AddPolygon(DrawnGeometry drawing, PointF[] points, bool close)
        {
            PathGeometryBuilder path = new PathGeometryBuilder(drawing);
            path.MoveTo(current.Apply(points[0].X, points[0].Y), 0);
            for (int index = 1; index < points.Length; ++index)
                path.LineTo(current.Apply(points[index].X, points[index].Y), 0);
            if (close)
                path.Close();
        }

        // Add a path given by its parts, and keep the drawing unless the path has no point.
        // drawing: the drawing.
        // parts: the parts.
        // paint: what the drawing paints with.
        private void AddPath(DrawnGeometry drawing, IEnumerable<GraphicsPathPart> parts, Paint paint)
        {
            PathGeometryBuilder path = new PathGeometryBuilder(drawing);
            AddParts(path, parts);
            if (path.HasBegun)
                Finish(drawing, paint);
        }

        // Add the parts of a path, as Skia takes them: a start begins a subpath, lines and Beziers continue it, a line
        // or curve with no point before it starting at the origin, and a close closes it.
        // path: the builder of the path.
        // parts: the parts.
        private void AddParts(PathGeometryBuilder path, IEnumerable<GraphicsPathPart> parts)
        {
            foreach (GraphicsPathPart part in parts) {
                switch (part.Kind) {
                    case GraphicsPathPartKind.Start:
                        path.MoveTo(current.Apply(part.Points[0].X, part.Points[0].Y), 0);
                        break;
                    case GraphicsPathPartKind.Lines:
                        if (!path.HasBegun)
                            path.MoveTo(current.Apply(0, 0), 0);
                        foreach (PointF point in part.Points)
                            path.LineTo(current.Apply(point.X, point.Y), 0);
                        break;
                    case GraphicsPathPartKind.Beziers:
                        if (!path.HasBegun)
                            path.MoveTo(current.Apply(0, 0), 0);
                        for (int index = 0; index + 2 < part.Points.Length; index += 3) {
                            path.CubicTo(current.Apply(part.Points[index].X, part.Points[index].Y),
                                         current.Apply(part.Points[index + 1].X, part.Points[index + 1].Y),
                                         current.Apply(part.Points[index + 2].X, part.Points[index + 2].Y), 0);
                        }
                        break;
                    case GraphicsPathPartKind.Close:
                        path.Close();
                        break;
                }
            }
        }

        // Add a rectangle as a closed subpath from its top left corner along its top first, as Skia adds one.
        // path: the builder of the path.
        // rectangle: the rectangle.
        // toPage: the transform from its coordinates to the page's.
        private static void AddRectangle(PathGeometryBuilder path, RectangleF rectangle, Affine toPage)
        {
            path.MoveTo(toPage.Apply(rectangle.Left, rectangle.Top), 0);
            path.LineTo(toPage.Apply(rectangle.Right, rectangle.Top), 0);
            path.LineTo(toPage.Apply(rectangle.Right, rectangle.Bottom), 0);
            path.LineTo(toPage.Apply(rectangle.Left, rectangle.Bottom), 0);
            path.Close();
        }

        // Add a text: its characters as a document holds them, the start of its baseline and the point an em along it.
        // callName: the name of the call, or of the kind of clipping object.
        // kind: the kind and paint of what it draws.
        // text: the text.
        // emHeight: the size of its font.
        // ascent: the ascent of its font's face, which puts the start of its baseline below its upper left corner.
        // upperLeft: its upper left corner.
        // paint: what it paints with.
        // penWidth: the width of the pen of its outline, or null for a filled text.
        private void AddText(string callName, string kind, string text, float emHeight, float ascent, PointF upperLeft, Paint paint, float? penWidth)
        {
            string characters = DocumentCharacters.Of(text);
            if (characters.Length == 0)
                return;
            DrawnGeometry drawing = Begin(callName, kind);
            if (penWidth.HasValue)
                drawing.AddLength(penWidth.Value * current.LengthScale, 0);
            drawing.Signature.Append(" '").Append(characters).Append('\'');
            double baseline = (double)upperLeft.Y + ascent;
            drawing.AddPoint(current.Apply(upperLeft.X, baseline), 0);
            drawing.AddPoint(current.Apply((double)upperLeft.X + emHeight, baseline), 0);
            Finish(drawing, paint);
        }

        // Add a bitmap: the corners of its rectangle, from its top left corner along its top first.
        // callName: the name of the call.
        // rectangle: the rectangle.
        private void AddImage(string callName, RectangleF rectangle)
        {
            DrawnGeometry drawing = Begin(callName, "image");
            drawing.AddPoint(current.Apply(rectangle.Left, rectangle.Top), 0);
            drawing.AddPoint(current.Apply(rectangle.Right, rectangle.Top), 0);
            drawing.AddPoint(current.Apply(rectangle.Right, rectangle.Bottom), 0);
            drawing.AddPoint(current.Apply(rectangle.Left, rectangle.Bottom), 0);
            Drawings.Add(drawing);
        }
    }
}
