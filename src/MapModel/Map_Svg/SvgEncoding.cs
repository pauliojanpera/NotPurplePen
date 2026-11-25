using System;
using System.Collections.Generic;
using System.Text;
using PurplePen.Graphics2D;

using PointF = System.Drawing.PointF;
using RectangleF = System.Drawing.RectangleF;

namespace PurplePen.MapModel
{
    // The SVG encoding: it writes the transparency groups of the palette colors that pass 2 makes (Map.Draw) into the
    // document of an SVG graphics target (Svg_GraphicsTarget), within the part, the map and the groups that the target
    // has open, as the Skia encoding writes the SKCanvas of its target. The structure of the SVG reflects the structures
    // of Map:
    // - The transparency group of a palette color is a group that carries the position of the palette color in the color
    //   table of its map (data-c), its color C as the paint of the fills and the strokes of its elements, its opacity α
    //   when α is below 1, and its blend mode B when B is not Normal. It never merges with its neighbours.
    // - The consecutive palette color coverages of one symbol definition are a group that carries the number of the
    //   symbol definition in the symbol table of the map (data-s).
    // - A palette color coverage of a symbol is a group that carries the text form of the symbol key (data-k), and for a
    //   hatching or an area pattern the place of the coverage prototype in the symbol definition (data-g). A coverage of
    //   one element gives them to the element.
    // - The clipping objects at the resolution of the drawing are elements in the order of the coverage, each with its
    //   fill rule or its pen and inheriting the color of the group: a run of dashes is one path with the dash pattern of
    //   its pen, an offset path is its offset curve with its dashes expanded, the shared shapes of a glyph are defined once
    //   and a use places them at each placement, and a repeated placement is a group with a use at each pose.
    // - A hatching or a regular clipped area pattern is a pattern fill when the group uses pattern brushes: its coverage
    //   prototype is a pattern of one color, and each configuration refers to it with its transformation M. The pattern
    //   of an area cluster of several areas is written once, in the union of its areas: a clipping path that holds the
    //   path of each area. Otherwise the expansion of the coverage prototype is written within the clip of the area or
    //   of the union.
    // The setting for a reader that cannot fill an area with a pattern draws the pattern fills as expansions, each area
    // its own (TransparencyGroup.PatternsWithEachArea), and the runs of dashes dash by dash.
    // The encoding keeps nothing over the groups: the document holds the definitions, each once.
    public sealed class SvgEncoding : IPaletteStackEncoding
    {
        // The style of a filled element, which takes its fill from the transparency group and paints no stroke.
        private static readonly SvgStyle FillStyle = new SvgStyle("stroke:none");

        // The target, whose document the encoding writes.
        private readonly Svg_GraphicsTarget target;

        // Make the encoding of an SVG graphics target.
        // target: the graphics target, whose document the encoding writes.
        internal SvgEncoding(Svg_GraphicsTarget target)
        {
            this.target = target;
        }

        // Write the transparency group of a palette color into the document, within the innermost group that the target
        // has open, which is in a map: a group of its palette color coverages, in the order of the group, each within the
        // group of its symbol definition. The map's record gets the palette color and the symbol definitions of the
        // coverages written.
        // group: the transparency group.
        public void WriteTransparencyGroup(TransparencyGroup group)
        {
            if (group == null)
                throw new ArgumentNullException(nameof(group));

            SvgMapRecord map = target.BeginEncodedDrawing(nameof(WriteTransparencyGroup));
            SymColor color = group.Color;
            int position = PositionOf(color);
            SvgGroup transparencyGroup = new SvgGroup {
                Identity = "data-c=\"" + SvgNumbers.FormatInteger(position) + "\"",
                Style = GroupDeclarations(group),
                Blends = true,
            };
            target.AddEncodedNode(transparencyGroup);

            bool colorRecorded = false;
            SymDef definitionOfSymbolGroup = null;
            SvgGroup symbolGroup = null;
            HashSet<AreaCluster> writtenClusters = new HashSet<AreaCluster>();
            foreach (SymbolCoverage coverage in group.SymbolCoverages) {
                string[] keys;
                SvgGroup node = WriteCoverage(group, coverage, writtenClusters, out keys);
                if (node == null || node.Children.Count == 0)
                    continue;
                foreach (string key in keys)
                    map.RecordSymbolKey(key);

                if (!colorRecorded) {
                    map.RecordColor(color, position, color.Name, color.CmykColor, color.OverPrint);
                    colorRecorded = true;
                }

                SymDef definition = coverage.Symbol.Definition;
                if (symbolGroup == null || definition != definitionOfSymbolGroup) {
                    SvgSymbolRecord symbol = map.RecordSymbol(definition, definition.SymbolId, definition.Name);
                    symbol.AddColor(position);
                    symbolGroup = new SvgGroup { Identity = "data-s=\"" + SvgNumbers.FormatInteger(symbol.Number) + "\"" };
                    transparencyGroup.Children.Add(symbolGroup);
                    definitionOfSymbolGroup = definition;
                }
                symbolGroup.Children.Add(node);
            }
        }

        // The position of a palette color in the color table of its map, counting from 0 at the bottom.
        // color: the palette color.
        private static int PositionOf(SymColor color)
        {
            int position = 0;
            foreach (SymColor other in color.ContainingMap.AllColors) {
                if (other == color)
                    return position;
                ++position;
            }
            throw new InvalidOperationException("WriteTransparencyGroup: the palette color is not in the color table of its map.");
        }

        // The declarations of the style of a transparency group: the color C of the palette color as the paint of the fills
        // and the strokes, at the intensity of the target, its opacity α when it is below 1, and its blend mode B when it is
        // not Normal (COL-1, D-8).
        // group: the transparency group.
        private string GroupDeclarations(TransparencyGroup group)
        {
            StringBuilder declarations = new StringBuilder(PaintDeclarations(group.Color));
            float opacity = Math.Max(0, Math.Min(1, group.Opacity));
            if (opacity < 1)
                declarations.Append(";opacity:").Append(SvgNumbers.FormatShortest(opacity));
            switch (group.BlendMode) {
                case ColorBlendMode.Darken:
                    declarations.Append(";mix-blend-mode:darken");
                    break;
                case ColorBlendMode.Multiply:
                    declarations.Append(";mix-blend-mode:multiply");
                    break;
            }
            return declarations.ToString();
        }

        // The declarations that paint the fills and the strokes with the color C of a palette color, at the intensity of
        // the target (COL-1).
        // color: the palette color.
        private string PaintDeclarations(SymColor color)
        {
            CmykColor value = target.AtIntensity(color.ColorValue);
            SvgPaints paints = target.Document.Paints;
            return paints.Paint("fill", value) + ";" + paints.Paint("stroke", value);
        }

        // Write a palette color coverage of a symbol, and return its node: a group with the identity of the coverage, which
        // its element takes when it has one. The coverage of a hatching or a clipped area pattern is its pattern fill. The
        // pattern fill of an area cluster of several areas is written once, at its first coverage that the group gives,
        // unless each area writes its own: for the others, nothing is written, and null is returned.
        // group: the transparency group.
        // coverage: the palette color coverage.
        // writtenClusters: the area clusters whose pattern fills are written.
        // keys: receives the text forms of the symbol keys that the node carries.
        private SvgGroup WriteCoverage(TransparencyGroup group, SymbolCoverage coverage, HashSet<AreaCluster> writtenClusters, out string[] keys)
        {
            if (coverage.Prototype == CoveragePrototype.FullCoverage) {
                keys = new string[] { coverage.Symbol.Key.Text };
                SvgGroup coverageGroup = new SvgGroup { Identity = Svg_GraphicsTarget.KeyIdentity(keys), IsCoverage = true };
                WriteClippingObjects(coverageGroup.Children, coverage.GetClippingObjects(group.Resolution), group.PatternsWithEachArea);
                return coverageGroup;
            }

            AreaCluster cluster = group.GetAreaCluster(coverage);
            if (group.PatternsWithEachArea || cluster.SymbolCoverages.Count == 1)
                return WritePatternFill(group, coverage, cluster.PaletteCutter, new SymbolCoverage[] { coverage }, null, out keys);
            keys = null;
            if (!writtenClusters.Add(cluster))
                return null;
            return WritePatternFill(group, cluster.SymbolCoverages[0], cluster.PaletteCutter, cluster.SymbolCoverages, cluster.Bounds, out keys);
        }

        // Write the pattern fill of a hatching or a clipped area pattern in its areas: in each area of a palette color
        // coverage, or once in the union of the areas of an area cluster. A pattern fill of the coverage prototype, when the
        // group uses pattern brushes and the prototype is a hatching or a regular area pattern, fills the area, or a
        // rectangle that bounds the cluster within the clip of the union. Otherwise the expansion of the coverage
        // prototype is written within the clip of the area or of the union, as the Skia encoding writes it. The clip of the
        // union holds the path of each area of the cluster with the fill rule of the area, and SVG clips to the union of
        // the children of a clipping path: the region of AreaCluster.Union. The pattern fill carries the symbol keys of all
        // its areas and the place of the coverage prototype.
        // group: the transparency group.
        // coverage: the palette color coverage whose transformation M places the expansion: the coverage of the area, or
        //   the first of the cluster.
        // cutter: the palette cutter of the coverages, whose transformation M places the pattern fill.
        // coverages: the palette color coverages whose areas the pattern fills: the coverage, or those of the cluster.
        // clusterBounds: the bounds of the cluster, over which the coverage prototype is expanded, or null for the areas of
        //   one palette color coverage.
        // keys: receives the text forms of the symbol keys of the coverages, which the pattern fill carries.
        private SvgGroup WritePatternFill(TransparencyGroup group, SymbolCoverage coverage, PaletteCutter cutter, IReadOnlyList<SymbolCoverage> coverages,
                                          RectangleF? clusterBounds, out string[] keys)
        {
            keys = new string[coverages.Count];
            for (int index = 0; index < keys.Length; ++index)
                keys[index] = coverages[index].Symbol.Key.Text;
            SvgGroup coverageGroup = new SvgGroup {
                Identity = Svg_GraphicsTarget.KeyIdentity(keys) + " data-g=\"" + PlaceOfPrototype(coverage.Symbol.Definition, coverage.Prototype) + "\"",
                IsCoverage = true,
            };
            SvgPattern pattern = UsesPatternFill(group, coverage.Prototype) ? ConfigurationOf(group.Color, cutter, coverage) : null;

            if (clusterBounds.HasValue) {
                List<string> clipData = new List<string>();
                List<bool> clipRules = new List<bool>();
                foreach (SymbolCoverage clustered in coverages) {
                    foreach (ClippingObject clippingObject in clustered.ClippingObjects) {
                        bool evenOdd;
                        SvgPathData areaData = PathOfArea(clippingObject, out evenOdd);
                        if (!areaData.IsEmpty) {
                            clipData.Add(areaData.ToString());
                            clipRules.Add(evenOdd);
                        }
                    }
                }
                if (clipData.Count > 0)
                    coverageGroup.Children.Add(PatternInClip(group, coverage, pattern, target.Document.ClipOf(clipData, clipRules), clusterBounds));
                return coverageGroup;
            }

            foreach (ClippingObject clippingObject in coverage.ClippingObjects) {
                bool evenOdd;
                SvgPathData areaData = PathOfArea(clippingObject, out evenOdd);
                if (areaData.IsEmpty)
                    continue;
                if (pattern != null) {
                    SvgStyle fill = new SvgStyle("fill:" + SvgStyle.PatternReference + ";stroke:none", pattern);
                    coverageGroup.Children.Add(new SvgPath(Svg_GraphicsTarget.FillUnderRule(fill, evenOdd), areaData));
                    pattern.ExtendReachOver(areaData.Bounds);
                }
                else {
                    coverageGroup.Children.Add(PatternInClip(group, coverage, null, target.Document.ClipOf(areaData.ToString(), evenOdd), null));
                }
            }
            return coverageGroup;
        }

        // The group of a pattern fill within a clip: a rectangle that bounds an area cluster, filled with the pattern of a
        // configuration, or the expansion of the coverage prototype over the cluster or over the area.
        // group: the transparency group.
        // coverage: the palette color coverage whose transformation M places the expansion.
        // pattern: the pattern of the configuration, or null for the expansion.
        // clip: the clip of the area or of the union of the cluster.
        // clusterBounds: the bounds of the cluster, or null for an area.
        private SvgGroup PatternInClip(TransparencyGroup group, SymbolCoverage coverage, SvgPattern pattern, SvgClip clip, RectangleF? clusterBounds)
        {
            SvgGroup clipGroup = new SvgGroup { Clip = clip };
            if (pattern != null) {
                SvgPathData rectangle = RectanglePath(clusterBounds.Value);
                clipGroup.Children.Add(new SvgPath(new SvgStyle("fill:" + SvgStyle.PatternReference + ";stroke:none", pattern), rectangle));
                pattern.ExtendReachOver(rectangle.Bounds);
            }
            else {
                WriteClippingObjects(clipGroup.Children, clusterBounds.HasValue ? coverage.ExpandPrototype(clusterBounds.Value) : coverage.ExpandPrototype(),
                                     group.PatternsWithEachArea);
            }
            return clipGroup;
        }

        // The path of an area, the clipping object of a hatching or a clipped area pattern, in steps, and its fill rule.
        // clippingObject: the clipping object, a filled path at the identity placement.
        // evenOdd: receives whether the area is filled under the even-odd rule.
        private static SvgPathData PathOfArea(ClippingObject clippingObject, out bool evenOdd)
        {
            FilledShape area = clippingObject as FilledShape;
            PathShape areaPath = (area != null) ? area.Shape as PathShape : null;
            if (areaPath == null || area.Placement.Steps.Count != 0)
                throw new InvalidOperationException("The clipping object of a pattern fill is the area, a filled path at the identity placement.");
            evenOdd = area.FillRule == AreaFillMode.Alternate;
            return Svg_GraphicsTarget.PathOfParts(areaPath.Parts);
        }

        // Whether the coverage prototype of a palette color coverage is written as a pattern fill: when the group uses
        // pattern brushes, a hatching whose lines have a spacing, and a regular area pattern whose element has a size. A
        // pattern of vectors is never too coarse, so the resolution of the drawing does not count
        // (PatternTile.IsTooPixelatedAt).
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

        // The place of a coverage prototype in the symbol definition of its palette color coverage (data-g): the index of
        // the hatching or of the area pattern in its area symbol definition, as hatching.<index> or pattern.<index>, after
        // the index of the component definition at each level of a combination that holds that definition, each followed
        // by a period.
        // definition: the symbol definition of the palette color coverage.
        // prototype: the coverage prototype of a hatching or an area pattern.
        private static string PlaceOfPrototype(SymDef definition, CoveragePrototype prototype)
        {
            HatchingPrototype hatching = prototype as HatchingPrototype;
            AreaPatternPrototype areaPattern = prototype as AreaPatternPrototype;
            AreaSymDef owner = (hatching != null) ? hatching.Definition : areaPattern.Definition;
            string place = (hatching != null) ? "hatching." + SvgNumbers.FormatInteger(hatching.Index) : "pattern." + SvgNumbers.FormatInteger(areaPattern.Index);
            List<int> components = new List<int>();
            if (!FindComponent(definition, owner, components))
                throw new InvalidOperationException("The area symbol definition of the coverage prototype is not in the symbol definition of its palette color coverage.");
            StringBuilder text = new StringBuilder();
            foreach (int component in components)
                text.Append(SvgNumbers.FormatInteger(component)).Append('.');
            return text.Append(place).ToString();
        }

        // Find the indices of the component definitions, at each level of the combinations, through which a symbol
        // definition holds another, and return whether it holds it, or is it.
        // definition: the symbol definition.
        // owner: the symbol definition to find.
        // components: receives the indices, the outermost first.
        private static bool FindComponent(SymDef definition, SymDef owner, List<int> components)
        {
            if (definition == owner)
                return true;
            AreaComboSymDef combination = definition as AreaComboSymDef;
            if (combination == null)
                return false;
            int index = 0;
            foreach (SymDef component in combination.Components) {
                components.Add(index);
                if (FindComponent(component, owner, components))
                    return true;
                components.RemoveAt(components.Count - 1);
                ++index;
            }
            return false;
        }

        // The configuration of a palette cutter as a pattern: the definition of its coverage prototype, referred to with
        // the transformation M of the first palette color coverage of the palette cutter that refers to it. The
        // transformations of the palette color coverages of a palette cutter differ by symmetries of the coverage
        // prototype, so each places the pattern alike, within the precision of the normal form of M, and the rounding of
        // the normal form does not scale the pattern. The definition of an area pattern is its pattern tile, which
        // includes the offset of the area pattern, so it is referred to with M · T(-offset). The document defines each
        // configuration once.
        // color: the palette color.
        // cutter: the palette cutter.
        // coverage: the palette color coverage of the palette cutter that refers to the configuration.
        private SvgPattern ConfigurationOf(SymColor color, PaletteCutter cutter, SymbolCoverage coverage)
        {
            CoveragePrototype prototype = cutter.Prototype;
            SvgPattern definition = DefinitionOf(color, prototype);
            return target.Document.PatternOf(new ConfigurationKey(definition, cutter), () => {
                Matrix transformation = coverage.Transformation;
                AreaPatternPrototype areaPattern = prototype as AreaPatternPrototype;
                if (areaPattern != null)
                    transformation.Translate(-areaPattern.Pattern.patternOffsetX, -areaPattern.Pattern.patternOffsetY);
                return new SvgPattern(definition, SvgTransform.FromMatrix(transformation, SvgNumbers.StepsPerMillimetre));
            });
        }

        // The definition of a coverage prototype in a palette color, as a pattern: origin-bound, painted in the color with
        // its gaps unpainted. A hatching is a square tile as wide as its spacing, with its line y = 0 and the lines a
        // spacing from it whose width reaches into the tile, each running on past both sides of the tile, so that the
        // tiles join without a seam. An area pattern is its pattern tile (AreaPatternPrototype.Tile), its glyph parts the
        // shared shapes of the glyph of its element. The document defines each once for each paint.
        // color: the palette color.
        // prototype: the coverage prototype.
        private SvgPattern DefinitionOf(SymColor color, CoveragePrototype prototype)
        {
            string paint = PaintDeclarations(color);
            return target.Document.PatternOf(new DefinitionKey(prototype, paint), () => {
                SvgGroup content = new SvgGroup { Style = paint };
                SvgPattern definition;
                HatchingPrototype hatching = prototype as HatchingPrototype;
                if (hatching != null) {
                    AreaSymDef.HatchInfo hatchInfo = hatching.Hatching;
                    double spacing = hatchInfo.hatchSpacing, halfWidth = hatchInfo.hatchWidth / 2.0;
                    float size = (float)(spacing * SvgNumbers.StepsPerMillimetre);
                    definition = new SvgPattern(-size / 2, -size / 2, size, size);
                    SvgStyle pen = PenStyle(new Stroke(hatchInfo.hatchWidth, LineCapMode.Flat, LineJoinMode.Miter, GraphicsUtil.MITER_LIMIT), false);
                    int linesEachWay = (int)Math.Ceiling(halfWidth / spacing + 0.5);
                    for (int line = -linesEachWay; line <= linesEachWay; ++line) {
                        double y = line * spacing;
                        if (y - halfWidth < spacing / 2 && y + halfWidth > -spacing / 2) {
                            SvgPathData data = new SvgPathData();
                            data.MoveTo(SvgNumbers.ToSteps(-spacing), SvgNumbers.ToSteps(y));
                            data.LineTo(SvgNumbers.ToSteps(spacing), SvgNumbers.ToSteps(y));
                            content.Children.Add(new SvgPath(pen, data));
                        }
                    }
                }
                else {
                    PatternTile tile = ((AreaPatternPrototype)prototype).Tile;
                    float width = (float)(tile.Size.Width * SvgNumbers.StepsPerMillimetre), height = (float)(tile.Size.Height * SvgNumbers.StepsPerMillimetre);
                    definition = new SvgPattern(-width / 2, -height / 2, width, height);
                    WriteClippingObjects(content.Children, tile.ClippingObjects, false);
                }
                definition.Content.Children.Add(content);
                return definition;
            });
        }

        // Write clipping objects in their order. The consecutive clipping objects of one repeated placement are a group
        // that carries the steps of the placement of its path, with a use of their shared shapes at each pose; the
        // consecutive shared shapes of one single placement are a use of the shared shapes, with the steps of the
        // placement; and each other clipping object is its element within the steps of its placement.
        // nodes: the nodes that get the elements.
        // clippingObjects: the clipping objects.
        // dashesApart: whether the runs of dashes are written dash by dash.
        private void WriteClippingObjects(List<SvgNode> nodes, IReadOnlyList<ClippingObject> clippingObjects, bool dashesApart)
        {
            int index = 0;
            while (index < clippingObjects.Count) {
                ClippingObject clippingObject = clippingObjects[index];
                int last = index;
                while (last + 1 < clippingObjects.Count && clippingObjects[last + 1].Placement == clippingObject.Placement &&
                       clippingObjects[last + 1].IsSharedShape == clippingObject.IsSharedShape)
                    ++last;

                RepeatedPlacement repeated = clippingObject.Placement as RepeatedPlacement;
                if (repeated != null && clippingObject.IsSharedShape) {
                    SvgSharedShapes shapes = SharedShapesOf(clippingObjects, index, last);
                    SvgGroup placementGroup = new SvgGroup { IsStructure = true };
                    IReadOnlyList<SvgTransform> outerSteps = StepsOf(repeated.Steps);
                    if (outerSteps.Count > 0) {
                        placementGroup.TransformSteps = outerSteps;
                        placementGroup.Transform = SvgTransform.Compose(outerSteps);
                    }
                    foreach (Pose pose in repeated.Poses)
                        placementGroup.Children.Add(new SvgSharedShapesUse(shapes, new SvgTransform[] { StepOf(pose.ToMatrix()) }));
                    nodes.Add(placementGroup);
                }
                else if (repeated != null) {
                    foreach (SinglePlacement placement in repeated.Expand()) {
                        for (int k = index; k <= last; ++k)
                            WriteAt(nodes, clippingObjects[k], placement, dashesApart);
                    }
                }
                else if (clippingObject.IsSharedShape) {
                    nodes.Add(new SvgSharedShapesUse(SharedShapesOf(clippingObjects, index, last), StepsOf(clippingObject.Placement.Steps)));
                }
                else {
                    for (int k = index; k <= last; ++k)
                        WriteAt(nodes, clippingObjects[k], (SinglePlacement)clippingObjects[k].Placement, dashesApart);
                }
                index = last + 1;
            }
        }

        // The shared shapes of consecutive clipping objects, which the document defines once for each sequence of shapes
        // with their fill rules or pens.
        // clippingObjects: the clipping objects.
        // first, last: the first and the last of the consecutive clipping objects.
        private SvgSharedShapes SharedShapesOf(IReadOnlyList<ClippingObject> clippingObjects, int first, int last)
        {
            ClippingObject[] shared = new ClippingObject[last - first + 1];
            for (int index = first; index <= last; ++index)
                shared[index - first] = clippingObjects[index];
            return target.Document.SharedShapesOf(new SharedShapesKey(shared), () => {
                List<SvgShape> shapes = new List<SvgShape>();
                foreach (ClippingObject clippingObject in shared) {
                    List<SvgNode> elements = new List<SvgNode>();
                    WriteShape(elements, clippingObject, false);
                    foreach (SvgNode element in elements) {
                        SvgShape shape = element as SvgShape;
                        if (shape == null)
                            throw new InvalidOperationException("A shared shape of a glyph is a path, an ellipse or an arc.");
                        shapes.Add(shape);
                    }
                }
                return new SvgSharedShapes(shapes);
            });
        }

        // The steps of a placement as transforms between steps of the map.
        // steps: the steps, the outermost first.
        private static IReadOnlyList<SvgTransform> StepsOf(IReadOnlyList<Matrix> steps)
        {
            SvgTransform[] transforms = new SvgTransform[steps.Count];
            for (int index = 0; index < steps.Count; ++index)
                transforms[index] = StepOf(steps[index]);
            return transforms;
        }

        // A step of a placement as a transform between steps of the map.
        // step: the step.
        private static SvgTransform StepOf(Matrix step)
        {
            return SvgTransform.FromMatrix(step, SvgNumbers.StepsPerMillimetre);
        }

        // Write a clipping object at a single placement: its element within a group for each step of the placement, as
        // the drawing code of the symbols pushes them on a graphics target. The innermost step of a text run takes the
        // mirror of the Y axis of the run, so the group gets the transformation with which the drawing lays out the text.
        // nodes: the nodes that get the element.
        // clippingObject: the clipping object.
        // placement: the single placement.
        // dashesApart: whether a run of dashes is written dash by dash.
        private void WriteAt(List<SvgNode> nodes, ClippingObject clippingObject, SinglePlacement placement, bool dashesApart)
        {
            bool isText = clippingObject.Shape is TextShape;
            IReadOnlyList<Matrix> steps = placement.Steps;
            List<SvgNode> inner = nodes;
            for (int index = 0; index < steps.Count; ++index) {
                Matrix step = steps[index];
                if (isText && index == steps.Count - 1)
                    step.Scale(1, -1);
                SvgGroup group = new SvgGroup { Transform = step.IsIdentity ? null : StepOf(step) };
                inner.Add(group);
                inner = group.Children;
            }
            if (isText && steps.Count == 0) {
                SvgGroup mirror = new SvgGroup { Transform = new SvgTransform(1, 0, 0, -1, 0, 0) };
                inner.Add(mirror);
                inner = mirror.Children;
            }
            WriteShape(inner, clippingObject, dashesApart);
        }

        // Write the elements of the shape of a clipping object in its own coordinates, filled by its fill rule or stroked
        // with its pen, as the target writes the same shapes of the old drawing: a path, an ellipse as a circle or an
        // ellipse, an arc as a path of an arc, or the whole circle for a full turn, and a text run as a text. A run of
        // dashes is one path with the dash pattern of its pen, or with dashesApart a path for each dash. An offset path is
        // its offset curve, its dashes each a path, as SVG measures a dash pattern along the element's own path, and the
        // dash pattern of an offset path is measured along its base path. A stroke of no width is left out (D-5).
        // nodes: the nodes that get the elements.
        // clippingObject: the clipping object.
        // dashesApart: whether a run of dashes is written dash by dash.
        private void WriteShape(List<SvgNode> nodes, ClippingObject clippingObject, bool dashesApart)
        {
            Shape shape = clippingObject.Shape;
            FilledShape filled = clippingObject as FilledShape;
            if (filled != null) {
                SvgStyle fill = Svg_GraphicsTarget.FillUnderRule(FillStyle, filled.FillRule == AreaFillMode.Alternate);
                if (shape is TextShape) {
                    AddText(nodes, (TextShape)shape, "stroke:none");
                    return;
                }
                AddShape(nodes, shape, fill);
                return;
            }

            StrokedShape stroked = (StrokedShape)clippingObject;
            Stroke stroke = stroked.Stroke;
            if (stroke.Width == 0) {
                target.Document.LeaveOut(nameof(StrokedShape));
                return;
            }
            if (shape is TextShape) {
                AddText(nodes, (TextShape)shape, PenStyle(stroke, true).Declarations);
                return;
            }
            OffsetPathShape offsetPath = shape as OffsetPathShape;
            if (stroke.DashPattern != null && (dashesApart || offsetPath != null)) {
                foreach (StrokedShape dash in stroked.ExpandDashes()) {
                    Shape dashShape = (dash.Shape is OffsetPathShape) ? ((OffsetPathShape)dash.Shape).OffsetCurve : dash.Shape;
                    AddShape(nodes, dashShape, PenStyle(dash.Stroke, false));
                }
                return;
            }
            AddShape(nodes, (offsetPath != null) ? offsetPath.OffsetCurve : shape, PenStyle(stroke, false));
        }

        // Add the element of a path, an ellipse or an arc with a style. An empty path draws nothing.
        // nodes: the nodes that get the element.
        // shape: the shape.
        // style: the style: the fill rule, or the pen.
        private static void AddShape(List<SvgNode> nodes, Shape shape, SvgStyle style)
        {
            PathShape path = shape as PathShape;
            if (path != null) {
                SvgPathData data = Svg_GraphicsTarget.PathOfParts(path.Parts);
                if (!data.IsEmpty)
                    nodes.Add(new SvgPath(style, data));
                return;
            }

            EllipseShape ellipse = shape as EllipseShape;
            if (ellipse != null) {
                nodes.Add(new SvgEllipse(style, SvgNumbers.ToSteps(ellipse.Center.X), SvgNumbers.ToSteps(ellipse.Center.Y),
                                         Math.Abs(SvgNumbers.ToSteps(ellipse.RadiusX)), Math.Abs(SvgNumbers.ToSteps(ellipse.RadiusY))));
                return;
            }

            ArcShape arc = shape as ArcShape;
            if (arc != null) {
                PointF center = arc.Center;
                float radius = arc.Radius;
                long radiusInSteps = Math.Abs(SvgNumbers.ToSteps(radius));
                if (Math.Abs(arc.SweepAngle) >= 360) {
                    nodes.Add(new SvgEllipse(style, SvgNumbers.ToSteps(center.X), SvgNumbers.ToSteps(center.Y), radiusInSteps, radiusInSteps));
                    return;
                }
                double start = arc.StartAngle * Math.PI / 180, end = (arc.StartAngle + arc.SweepAngle) * Math.PI / 180;
                SvgPathData data = new SvgPathData();
                data.MoveTo(SvgNumbers.ToSteps(center.X + radius * Math.Cos(start)), SvgNumbers.ToSteps(center.Y + radius * Math.Sin(start)));
                data.ArcTo(radiusInSteps, radiusInSteps, Math.Abs(arc.SweepAngle) > 180, arc.SweepAngle > 0,
                           SvgNumbers.ToSteps(center.X + radius * Math.Cos(end)), SvgNumbers.ToSteps(center.Y + radius * Math.Sin(end)));
                long centerX = SvgNumbers.ToSteps(center.X), centerY = SvgNumbers.ToSteps(center.Y);
                data.Bounds.Include(centerX - radiusInSteps, centerY - radiusInSteps);
                data.Bounds.Include(centerX + radiusInSteps, centerY + radiusInSteps);
                nodes.Add(new SvgPath(style, data));
                return;
            }

            throw new ArgumentException("A shape of an element is a path, an ellipse or an arc.", nameof(shape));
        }

        // Add the element of a text run: the text that the drawing draws at its top left corner with the opposite y
        // coordinate, in the mirrored frame around it, in its font, which the document embeds (T-1, T-6).
        // nodes: the nodes that get the element.
        // text: the text run.
        // paint: the declarations of the fill or the stroke of the text, after its font.
        private void AddText(List<SvgNode> nodes, TextShape text, string paint)
        {
            Svg_GraphicsTarget.FontRecord font = target.FontOfTextRun(text.FontFamily, text.EmHeight, text.Effects, nameof(WriteTransparencyGroup));
            SvgText element = Svg_GraphicsTarget.MakeText(font, new SvgStyle(font.Declarations + ";" + paint), text.Text,
                                                          new PointF(text.UpperLeft.X, -text.UpperLeft.Y));
            if (element != null)
                nodes.Add(element);
        }

        // The style of a pen: no fill, the width, then the cap and the join, each left out where it is SVG's default, and
        // the dash pattern and its phase, all in steps of the shape, or for a text in millimetres, the units a text is set
        // in (D-5, T-2). The pattern and the phase keep their thousandths of a step, so that a long run of dashes does not
        // drift.
        // stroke: the pen.
        // inTextUnits: whether the pen strokes a text.
        private static SvgStyle PenStyle(Stroke stroke, bool inTextUnits)
        {
            StringBuilder declarations = new StringBuilder("fill:none;stroke-width:");
            long width = SvgNumbers.ToSteps(stroke.Width);
            declarations.Append(inTextUnits ? SvgNumbers.FormatDecimal(width / SvgText.StepsPerTextUnit, SvgText.TextUnitDecimals) : SvgNumbers.FormatInteger(width));
            declarations.Append(Svg_GraphicsTarget.CapAndJoinDeclarations(stroke.Caps, stroke.Join, stroke.MiterLimit, nameof(WriteTransparencyGroup)));
            if (stroke.DashPattern != null && !inTextUnits) {
                declarations.Append(";stroke-dasharray:");
                for (int index = 0; index < stroke.DashPattern.Count; ++index) {
                    if (index > 0)
                        declarations.Append(' ');
                    declarations.Append(SvgNumbers.FormatDecimal(stroke.DashPattern[index] * SvgNumbers.StepsPerMillimetre, DashDecimals));
                }
                string phase = SvgNumbers.FormatDecimal(stroke.DashPhase * SvgNumbers.StepsPerMillimetre, DashDecimals);
                if (phase != "0")
                    declarations.Append(";stroke-dashoffset:").Append(phase);
            }
            return new SvgStyle(declarations.ToString());
        }

        // The decimals of a step that the lengths of a dash pattern keep.
        private const int DashDecimals = 3;

        // The path of a rectangle, in steps, from its top left corner along the top first.
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

        // The key of shared shapes: the shapes of the clipping objects, each with its fill rule or its pen. The placements
        // of a glyph share the shape and the pen objects, so they are compared by identity, and the fill rules by value.
        private sealed class SharedShapesKey : IEquatable<SharedShapesKey>
        {
            private readonly ClippingObject[] clippingObjects;

            public SharedShapesKey(ClippingObject[] clippingObjects)
            {
                this.clippingObjects = clippingObjects;
            }

            public bool Equals(SharedShapesKey other)
            {
                if (other == null || other.clippingObjects.Length != clippingObjects.Length)
                    return false;
                for (int index = 0; index < clippingObjects.Length; ++index) {
                    ClippingObject first = clippingObjects[index], second = other.clippingObjects[index];
                    if (first.Shape != second.Shape || first.GetType() != second.GetType())
                        return false;
                    if (first is StrokedShape && ((StrokedShape)first).Stroke != ((StrokedShape)second).Stroke)
                        return false;
                    if (first is FilledShape && ((FilledShape)first).FillRule != ((FilledShape)second).FillRule)
                        return false;
                }
                return true;
            }

            public override bool Equals(object obj)
            {
                return Equals(obj as SharedShapesKey);
            }

            public override int GetHashCode()
            {
                int hash = clippingObjects.Length;
                foreach (ClippingObject clippingObject in clippingObjects)
                    hash = hash * 31 + System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(clippingObject.Shape);
                return hash;
            }
        }

        // The key of the definition of a coverage prototype: the prototype, and the paint of its color.
        private sealed class DefinitionKey : IEquatable<DefinitionKey>
        {
            private readonly CoveragePrototype prototype;
            private readonly string paint;

            public DefinitionKey(CoveragePrototype prototype, string paint)
            {
                this.prototype = prototype;
                this.paint = paint;
            }

            public bool Equals(DefinitionKey other)
            {
                return other != null && other.prototype == prototype && string.Equals(other.paint, paint, StringComparison.Ordinal);
            }

            public override bool Equals(object obj)
            {
                return Equals(obj as DefinitionKey);
            }

            public override int GetHashCode()
            {
                return System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(prototype) * 31 + StringComparer.Ordinal.GetHashCode(paint);
            }
        }

        // The key of a configuration: the definition of its coverage prototype, and its palette cutter.
        private sealed class ConfigurationKey : IEquatable<ConfigurationKey>
        {
            private readonly SvgPattern definition;
            private readonly PaletteCutter cutter;

            public ConfigurationKey(SvgPattern definition, PaletteCutter cutter)
            {
                this.definition = definition;
                this.cutter = cutter;
            }

            public bool Equals(ConfigurationKey other)
            {
                return other != null && other.definition == definition && other.cutter == cutter;
            }

            public override bool Equals(object obj)
            {
                return Equals(obj as ConfigurationKey);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(definition), System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(cutter));
            }
        }
    }
}
