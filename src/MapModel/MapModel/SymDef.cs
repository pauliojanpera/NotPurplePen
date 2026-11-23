/* Copyright (c) 2006-2008, Peter Golde
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms, with or without 
 * modification, are permitted provided that the following conditions are 
 * met:
 * 
 * 1. Redistributions of source code must retain the above copyright
 * notice, this list of conditions and the following disclaimer.
 * 
 * 2. Redistributions in binary form must reproduce the above copyright
 * notice, this list of conditions and the following disclaimer in the
 * documentation and/or other materials provided with the distribution.
 * 
 * 3. Neither the name of Peter Golde, nor "Purple Pen", nor the names
 * of its contributors may be used to endorse or promote products
 * derived from this software without specific prior written permission.
 * 
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND
 * CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES,
 * INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR
 * CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING,
 * BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
 * SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
 * INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
 * WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
 * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE
 * USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY
 * OF SUCH DAMAGE.
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Drawing;
using System.Threading;
using System.Collections.Concurrent;

namespace PurplePen.MapModel
{
    using System.Linq;
    using PurplePen.Graphics2D;

    public abstract class SymDef
    {
        string name;
        public string Name { get { return name; } }

        internal List<Symbol> symbols = new List<Symbol>();
        public ICollection<Symbol> Symbols
        {
            get
            {
                return symbols;
            }
        }

        // Symdefs that depends on this symdef (e.g., area symdefs this use this symdef as a bounding line).
        internal List<SymDef> dependentSymdefs = new List<SymDef>();
        public ICollection<SymDef> DependentSymdefs
        {
            get
            {
                return dependentSymdefs;
            }
        }

        protected SymDef(string name, string symbolId)
        {
            this.name = name;
            this.symbolId = symbolId;
        }

        // The containing map
        protected Map map;
        public Map ContainingMap { get { return map; } }

        private ToolboxIcon toolboxIcon;

        public ToolboxIcon ToolboxImage
        {
            get
            {
                if (toolboxIcon == null)
                    toolboxIcon = new ToolboxIcon();
                return toolboxIcon;
            }
            set
            {
                CheckModifiable();
                toolboxIcon = value;
            }
        }

        private string symbolId;

        public string SymbolId
        {
            get { return symbolId; }
        }

        public virtual void SetMap(Map newMap)
        {
            if (map != null && newMap != null && map != newMap)
                throw new MapUsageException("Cannot add SyMDef to a map; it is already part of another map.");
            map = newMap;
        }

        // A new symbol with this symdef is added to the map. Add to the list of symbols with this SymDef.
        internal void AddSymbol(Symbol sym)
        {
            Debug.Assert(sym.Definition == this);

            symbols.Add(sym);
        }

        // A symbol with the symdef is removed from the map. Remove from the list of symbosl with this symdef.
        internal void RemoveSymbol(Symbol sym)
        {
            Debug.Assert(sym.Definition == this);
            symbols.Remove(sym);
        }

        // A new dependent symdef is added.
        internal void AddDependentSymdef(SymDef symdef)
        {
            Debug.Assert(symdef.DependsOnSymdef == this);

            dependentSymdefs.Add(symdef);
        }

        // A dependent symdef is removed
        internal void RemoveDependentSymdef(SymDef symdef)
        {
            Debug.Assert(symdef.DependsOnSymdef == this);
            dependentSymdefs.Remove(symdef);
        }

        protected void CheckModifiable()
        {
            if (map != null)
                throw new MapUsageException("Cannot modify a SymDef after it has been added to a map");
        }

        protected void CheckColor(SymColor color)
        {
            if (color != null && color.ContainingMap != map)
                throw new MapUsageException("Color in SymDef is not part of the containing map");
        }

        // Does this symbol definition draw the given color. Used to determine
        // if this symbol definition draws into the current layer being draw.
        // If color is null, return if draws in the image layer (below all color layers).
        public abstract bool HasColor(SymColor color);

        // The coverage prototypes of a symbol of this definition in a palette color, for pass 1 of the drawing
        // (Map.PaletteSeparations): the full coverage, where the definition draws the palette color with it, or none. An
        // area symbol definition gives the coverage prototypes of its hatchings and its clipped area patterns after it
        // (AreaLikeSymDef). The symbol has a palette color coverage for each of them (SymbolCoverage), also when it draws
        // no shape in the palette color.
        // symbol: the symbol, which has this definition.
        // color: the palette color.
        public virtual IReadOnlyList<CoveragePrototype> GetCoveragePrototypes(Symbol symbol, SymColor color)
        {
            return HasFullCoverage(color) ? CoveragePrototype.FullCoverageOnly : CoveragePrototype.None;
        }

        // Does this symbol definition draw the given palette color with the full coverage: does it draw the color apart
        // from the hatchings and the clipped area patterns of areas? The image layer and the layout layer are not palette
        // colors.
        // color: the palette color.
        internal virtual bool HasFullCoverage(SymColor color)
        {
            return !color.IsSpecialLayer && HasColor(color);
        }

        // The geometry method of this symbol definition: the clipping objects of the palette color coverage of a symbol
        // of this definition (SymbolCoverage), without a drawing target and without render options. Each symbol
        // definition has a geometry method of its own, so pass 1 does not use the collector.
        // symbol: the symbol, which has this definition.
        // color: the palette color.
        // prototype: the coverage prototype.
        internal abstract IReadOnlyList<ClippingObject> MakeClippingObjects(Symbol symbol, SymColor color, CoveragePrototype prototype);

        // The clipping objects of the full coverage of a symbol of this definition in a palette color, which the collector
        // makes from the drawing code of the symbol (ClippingObjectCollector). They are the oracle of the geometry methods
        // in the tests, and pass 1 does not use them.
        // symbol: the symbol, which has this definition.
        // color: the palette color.
        internal IReadOnlyList<ClippingObject> CollectClippingObjects(Symbol symbol, SymColor color)
        {
            return ClippingObjectCollector.Collect(symbol, color, map.TextMetricsProvider);
        }


        // Does this symdef depend on another? null means no.
        public virtual SymDef DependsOnSymdef
        {
            get
            {
                return null;
            }
        }

        // Remove any caches brushes/pens/fonts.
        public abstract void FreeGdiObjects();

        // Get an array of "count" distinct key objects (for pens or brushes), stored in "field", creating it if
        // the field is null or has the wrong length. Several threads can draw a symdef at once (on different
        // graphics targets), so this is lock-free: the array is published whole with Interlocked.CompareExchange
        // and never modified afterward. If two threads race, both get the array that won, so every thread
        // creates pens for and draws with the same keys.
        // field: the field that caches the array; must only be written through this method.
        // count: the number of keys needed.
        protected static object[] GetKeyArray(ref object[] field, int count)
        {
            object[] keys = Volatile.Read(ref field);
            while (keys == null || keys.Length != count) {
                object[] newKeys = new object[count];
                for (int i = 0; i < count; ++i)
                    newKeys[i] = new object();

                object[] existing = Interlocked.CompareExchange(ref field, newKeys, keys);
                keys = (existing == keys) ? newKeys : existing;
            }

            return keys;
        }

        // Do the symbols need to be sorted before drawing them?
        public virtual bool SortSymbolsForDrawing
        {
            get { return false; }
        }

        public abstract SymDef CopyToMap(Map map);
    }

    public class PointSymDef: SymDef
    {
        private readonly Glyph glyph;

        public Glyph Glyph { get { return glyph; } }

        readonly bool allowRotation;  // Should this glyph rotate when the feature/map is rotates, or always remain in the same orientation.
        public bool AllowRotation { get { return allowRotation; } }

        public float Radius { get { return glyph.Radius; } }

        public PointSymDef(string name, string symbolId, Glyph glyph, bool allowRotation)
            : base(name, symbolId)
        {
            if (glyph == null)
                throw new ArgumentNullException(nameof(glyph));
            this.glyph = glyph;
            this.allowRotation = allowRotation;
        }

        public override void SetMap(Map newMap)
        {
            base.SetMap(newMap);

            if (newMap != null) {
                glyph.CheckColors(newMap);
            }
        }

        public override SymDef CopyToMap(Map map)
        {
            var newSymDef = new PointSymDef(Name, SymbolId, glyph.CopyToMap(map), allowRotation);
            map.AddSymdef(newSymDef);
            return newSymDef;
        }

        // Does this symbol definition draw the given color. Used to determine
        // if this symbol definition draws into the current layer being draw.
        public override bool HasColor(SymColor color)
        {
            Debug.Assert(color != null);
            if (color.IsSpecialLayer)
                return false;
            return glyph.HasColor(color);
        }

        // Draw this point symbol at point pt with angle ang in this graphics (given color only).
        internal void Draw(IGraphicsTarget g, PointF pt, float angle, float[] gaps, SymColor color, RenderOptions renderOpts)
        {
            glyph.Draw(g, pt, angle, null, gaps, color, null, renderOpts);
        }

        // The geometry method of the point symbols: the parts of the glyph in the palette color, at the location and the
        // angle of the symbol, with its gaps (Glyph.MakeClippingObjects).
        internal override IReadOnlyList<ClippingObject> MakeClippingObjects(Symbol symbol, SymColor color, CoveragePrototype prototype)
        {
            Debug.Assert(prototype == CoveragePrototype.FullCoverage, "A point symbol has the full coverage only.");
            PointSymbol pointSymbol = (PointSymbol)symbol;
            return glyph.MakeClippingObjects(pointSymbol.Location, pointSymbol.Rotation, pointSymbol.Gaps, color);
        }

        internal void DrawHighlight(IGraphicsTarget g, PointF location, float rotation, float[] gaps, HighlightOptions options)
        {
            if (options.style == HighlightStyle.HighFidelity && glyph.Radius < 4 * options.logicalPixelSize) {
                // Draw bubble.
                float radius = (float) (6 * options.logicalPixelSize);
                g.FillEllipse(map.GetDimHighlightBrush(g), location, radius, radius);
            }

            glyph.DrawHighlight(g, location, rotation, null, gaps, map.GetHighlightBrush(g));
        }

        internal RectangleF HighlightBounds(PointF location, HighlightOptions options)
        {
            float radius = glyph.Radius;
            if (options.style == HighlightStyle.HighFidelity && radius < 4 * options.logicalPixelSize) {
                // Draw bubble.
                radius = (float)(6 * options.logicalPixelSize);
            }

            return new RectangleF(location.X - radius, location.Y - radius, radius * 2, radius * 2);
        }

        // Calculate the bounding box
        internal RectangleF CalcBounds(PointF pt, float angle)
        {
            RectangleF box1 = glyph.BoundingBox;
            if (angle != 0)
                box1 = Geometry.BoundsOfRotatedRectangle(box1, new PointF(), angle);
            box1.Offset(pt);

            float radius = glyph.Radius;
            RectangleF box2 = new RectangleF(pt.X - radius, pt.Y - radius, radius * 2, radius * 2);
            box1.Intersect(box2);
            return box1;
        }

        public override void FreeGdiObjects()
        {
            glyph.FreeGdiObjects();
        }
    }

    public abstract class LineLikeSymDef: SymDef
    {
        public LineLikeSymDef(string name, string symbolId)
            :base(name, symbolId)
        {
        }

        public abstract float MaxThickness { get; } 
        public abstract float HighlightThickness { get; }

        // Used during OpenMapper import for how to import dash points.
        internal abstract bool HasDashes { get; }

        internal abstract RectangleF CalcBounds(SymPath path);

        internal abstract void Draw(IGraphicsTarget g, SymPath path, SymColor color, RenderOptions renderOpts);

        internal abstract void DrawHighlight(IGraphicsTarget g, SymPath path, HighlightOptions highlightOptions);

        // The geometry method of a line of this definition along any base path: the clipping objects of the line in a
        // palette color, without a drawing target, in the order in which Draw draws them. The area border and a
        // component definition of an AreaComboSymDef use it with the main path and with each hole of an area.
        // path: the base path.
        // color: the palette color.
        internal abstract IReadOnlyList<ClippingObject> MakeClippingObjects(SymPath path, SymColor color);
    }

    // A combination of multiple line symdefs. Acts like a line symdef. For OpenMapper support.
    public class LineComboSymDef: LineLikeSymDef
    {
        public LineSymDef[] components;

        public LineComboSymDef(string name, string symbolId, IEnumerable<LineSymDef> components)
            :base(name, symbolId)
        {
            this.components = components.ToArray();

        }

        public IEnumerable<LineSymDef> Components
        {
            get
            {
                return components.ToList().AsReadOnly();
            }
        }

        public override float HighlightThickness
        {
            get
            {
                float max = 0;
                foreach (LineSymDef component in components) {
                    max = Math.Max(max, component.HighlightThickness);
                }
                return max;
            }
        }

        public override float MaxThickness
        {
            get
            {
                float max = 0;
                foreach (LineSymDef component in components) {
                    max = Math.Max(max, component.MaxThickness);
                }
                return max;
            }
        }

        internal override bool HasDashes
        {
            get
            {
                foreach (LineSymDef component in components) {
                    if (component.HasDashes)
                        return true;
                }

                return false;
            }
        }

        public override SymDef CopyToMap(Map map)
        {
            throw new NotImplementedException();
        }

        public override void FreeGdiObjects()
        {
        }

        public override bool HasColor(SymColor color)
        {
            foreach (LineSymDef component in components) {
                if (component.HasColor(color))
                    return true;
            }

            return false;
        }

        internal override RectangleF CalcBounds(SymPath path)
        {
            RectangleF bounds = new RectangleF();
            bool first = true;
            foreach (LineSymDef component in components) {
                RectangleF componentBounds = component.CalcBounds(path);

                if (first)
                    bounds = componentBounds;
                else
                    bounds = RectangleF.Union(bounds, componentBounds);

                first = false;
            }

            return bounds;
        }

        internal override void Draw(IGraphicsTarget g, SymPath path, SymColor color, RenderOptions renderOpts)
        {
            foreach (LineSymDef component in components) {
                component.Draw(g, path, color, renderOpts);
            }
        }

        internal override void DrawHighlight(IGraphicsTarget g, SymPath path, HighlightOptions highlightOptions)
        {
            foreach (LineSymDef component in components) {
                component.DrawHighlight(g, path, highlightOptions);
            }
        }

        // The geometry method of the combined lines: the clipping objects of the palette color coverage of a line symbol
        // (MakeClippingObjects(SymPath, SymColor)).
        internal override IReadOnlyList<ClippingObject> MakeClippingObjects(Symbol symbol, SymColor color, CoveragePrototype prototype)
        {
            Debug.Assert(prototype == CoveragePrototype.FullCoverage, "A line symbol has the full coverage only.");
            return MakeClippingObjects(((LineSymbol)symbol).Path, color);
        }

        // The clipping objects of a combined line along a base path in a palette color: the clipping objects of each
        // component definition, in the order of the components, as Draw draws them.
        // path: the base path.
        // color: the palette color.
        internal override IReadOnlyList<ClippingObject> MakeClippingObjects(SymPath path, SymColor color)
        {
            List<ClippingObject> clippingObjects = new List<ClippingObject>();
            foreach (LineSymDef component in components)
                clippingObjects.AddRange(component.MakeClippingObjects(path, color));
            return clippingObjects.AsReadOnly();
        }
    }

    public class LineSymDef: LineLikeSymDef
    {
        public enum SpacingMethod { OCAD, OpenMapperDashes, OpenMapperMidSymbols}
        // describes dash information
        public struct DashInfo
        {
            public float dashLength;      // length of the dashes (solid part)
            public float firstDashLength; // length of the first dash 
            public float lastDashLength;  // length of the last dash
            public bool halfEndDashLengthWhenClosed; // make first/last dash half length on a closed curve (for OpenMapper compatibility).
            public float gapLength;       // length of the gaps (undrawn part)
            public int minGaps;           // minimum number of GAPS in the line
            public int secondaryMiddleGaps;     // number of secondary gaps in the main dash (0 for none)
            public float secondaryMiddleLength; // length of the secondary gaps
            public int secondaryEndGaps;     // number of secondary gaps in the first and last dashes (0 for none)
            public float secondaryEndLength; // length of the first and last secondary gaps
            public SpacingMethod spacingMethod;    // method of computing dash spacing.
        }

        // describes double line info
        public struct DoubleLineInfo
        {
            public SymColor doubleFillColor; // color between double lines (or null for none)
            public SymColor doubleLeftColor; // color of left double line
            public SymColor doubleRightColor; // color of right double lines
            public float doubleThick;		// distance between the double lines, and thickness of fill
            public float doubleLeftWidth; // thickness of left double line
            public float doubleRightWidth; // thickness of right double line
            public bool doubleFillDashed;          // Dash the fill part?
            public bool doubleLeftDashed;        // Dash the left part?
            public bool doubleRightDashed;      // Dash the right part?
            public DashInfo doubleDashes;  // Info on the dashes.

        }

        // describes shorting of the line ends
        public struct ShortenInfo
        {
            public float shortenBeginning;	// amount to shorten beginning of line
            public float shortenEnd;		// amount to shorten end of line
            public bool pointyEnds;			// draw points to regular end
        }

        // Describes where a glyph is located.
        public enum GlyphLocation { 
            Start,        // at the start of the path (without shortening)
            End,         // at the end of the path (without shortening)
            Corners,   // at corner points
            CornersIgnoreEnds,   // at corner points, but never at the ends
            DashPoints,     // at the explicit dash points
            DashPointsIgnoreEnds,  // at the explicit dash points, but never at ends
            DashCenters,           // at the center of the main dashes in the dash information
            GapCenters,     // at the gaps between main dashes
            Spaced,            // spaced at distances given
            SpacedOffset,       // spaced as distances given, then offset by a certain amount, and the last one removed.
            SpacedDecrease,       // spaced as distances given, but with decreasing size and spacing
        }

        public struct GlyphInfo
        {
            public Glyph glyph;           // the glyph
            public GlyphLocation location;// where on line to draw it
            public float distance;        // for GlyphLocation.Spaced
            public float firstDistance;   // for GlyphLocation.Spaced
            public float lastDistance;    // for GlyphLocation.Spaced
            public int minimum;           // for GlyphLocation.Spaced, GapCenters -- minimum number of symbols. For GapCenters, the Max of this and the minimum number of gaps is used.
            public int number;            // number of symbols at each location
            public float spacing;         // spacing of symbols if number > 1
            public float offset;            // offset for GlyphLocation.SpacedOffset
            public float decreaseLimit;  // value between 1 and 0 with decrease final amount (SpacedDecrease only)
            public bool decreaseBothEnds;  // If true, decrease to both ends, else decrease to one end (SpacedDecrease only)
            public bool noScaleAtCorners;  // If true, glyph is NOT scaled when its a corner glyph.
            public SpacingMethod spacingMethod; // Way to compute spacing
        }

        private class LineSymDefPens
        {
            public object mainPen = new object();
            public object secondPen = new object();
            public object pointyEndsBrush = new object();
            public object doubleFillPen = new object();
            public object doubleLeftPen = new object();
            public object doubleRightPen = new object();
        }

        const float CORNER_GLYPH_STRETCH_LIMIT = 3.0F;

        SymColor lineColor;
        float thickness;
        LineCapMode lineCap;
        LineJoinMode lineJoin;

        SymColor secondLineColor;
        float secondThickness;
        LineJoinMode secondLineJoin;
        LineCapMode secondLineCap;

        DashInfo dashInfo;
        bool isDashed;

        bool isDoubleLine;
        DoubleLineInfo doubleLines;

        ShortenInfo shortenInfo;

        GlyphInfo[] glyphs;

        LineSymDefPens drawingPens = new LineSymDefPens();      // Pens for regular drawing
        LineSymDefPens highlightPens = new LineSymDefPens();    // Pens for drawing the highlight

        float maxThickness, maxMiteredThickness, highlightThickness;

        public SymColor LineColor { get { return lineColor; } }
        public float LineThickness { get { return thickness; } }
        public LineCapMode MainLineCap { get { return lineCap; } }
        public LineJoinMode MainLineJoin { get { return lineJoin; } }
        public bool HasSecondLine { get { return secondLineColor != null && secondThickness > 0; } }
        public SymColor SecondLineColor { get { return secondLineColor; } }
        public float SecondThickness { get { return secondThickness; } }
        public LineJoinMode SecondLineJoin { get { return secondLineJoin; } }
        public LineCapMode SecondLineCap { get { return secondLineCap; } }
        public bool IsDashed { get { return isDashed; } }
        public DashInfo Dashes { get { return dashInfo; } }
        public bool IsDoubleLine { get { return isDoubleLine; } }
        public DoubleLineInfo DoubleLines { get { return doubleLines; } }
        public GlyphInfo[] Glyphs { get { return (glyphs == null) ? null : (GlyphInfo[]) glyphs.Clone(); } }
        public override float MaxThickness { get { return maxThickness; } }
        public override float HighlightThickness { get { return highlightThickness; } }
        public ShortenInfo Shortening { get { return shortenInfo; } }
        public bool IsShortened { get { return (shortenInfo.shortenBeginning > 0 || shortenInfo.shortenEnd > 0); } }

        public override void SetMap(Map newMap)
        {
            base.SetMap(newMap);

            if (newMap != null) {
                CheckColor(lineColor);
                CheckColor(secondLineColor);
                CheckColor(doubleLines.doubleFillColor);
                CheckColor(doubleLines.doubleLeftColor);
                CheckColor(doubleLines.doubleRightColor);

                // compute the max thickness of this line.
                maxThickness = 0.0F;
                if (lineColor != null && thickness > maxThickness)
                    maxThickness = thickness;
                if (lineColor != null && lineJoin == LineJoinMode.Miter && thickness > maxMiteredThickness)
                    maxMiteredThickness = thickness;

                if (secondLineColor != null && secondThickness > maxThickness)
                    maxThickness = secondThickness;
                if (secondLineColor != null && secondLineJoin == LineJoinMode.Miter && secondThickness > maxMiteredThickness)
                    maxMiteredThickness = secondThickness;

                if (isDoubleLine) {
                    float doubleThickness = (2 * Math.Max(doubleLines.doubleLeftWidth, doubleLines.doubleRightWidth) + doubleLines.doubleThick);
                    if (doubleThickness > maxThickness)
                        maxThickness = doubleThickness;
                    if (doubleThickness > maxMiteredThickness)
                        maxMiteredThickness = doubleThickness;
                }

                highlightThickness = maxThickness;  // don't include glyph size in the highlight thickness.

                if (glyphs != null) {
                    foreach (GlyphInfo glyphInfo in glyphs) {
                        glyphInfo.glyph.CheckColors(newMap);

                        float glyphRadius = glyphInfo.glyph.Radius;

                        if (glyphRadius * 2 > maxThickness)
                            maxThickness = glyphRadius * 2;
                        if (glyphInfo.location == GlyphLocation.Corners && glyphRadius * 2 > maxMiteredThickness)
                            maxMiteredThickness = glyphRadius * 2;
                    }
                }

                // If a line symbol has no thickness other than glyphs, then include glyphs (e.g., narrow marsh).
                if (highlightThickness == 0)
                    highlightThickness = maxThickness;
            }
        }

        public LineSymDef(string name, string symbolId, SymColor color, float thick, LineJoinMode lineJoin, LineCapMode lineCap)
            : base(name, symbolId)
        {
            lineColor = color;
            thickness = thick;
            this.lineJoin = lineJoin;
            this.lineCap = lineCap;
        }

        public void SetSecondLine(SymColor secondLineColor, float secondThickness, LineJoinMode lineJoin, LineCapMode lineCap)
        {
            CheckModifiable();
            this.secondLineColor = secondLineColor;
            this.secondThickness = secondThickness;
            this.secondLineJoin = lineJoin;
            this.secondLineCap = lineCap;
        }

        public void SetDashInfo(DashInfo dashInfo)
        {
            CheckModifiable();
            this.dashInfo = dashInfo;
            isDashed = (dashInfo.gapLength > 0 || dashInfo.secondaryMiddleLength > 0 || dashInfo.secondaryEndLength > 0);
        }

        public void SetGlyphs(GlyphInfo[] glyphs)
        {
            CheckModifiable();
            this.glyphs = glyphs;
        }

        public void SetDoubleLines(DoubleLineInfo doubleLineInfo)
        {
            CheckModifiable();
            isDoubleLine = true;
            this.doubleLines = doubleLineInfo;
        }

        public void SetShortening(ShortenInfo shortenInfo)
        {
            CheckModifiable();
            this.shortenInfo = shortenInfo;
        }

        internal override bool HasDashes
        {
            get
            {
                if (isDashed)
                    return true;
                if (isDoubleLine && (doubleLines.doubleFillDashed || doubleLines.doubleLeftDashed || doubleLines.doubleRightDashed))
                    return true;

                return false;
            }
        }

        public override SymDef CopyToMap(Map map)
        {
            var newSymDef = new LineSymDef(Name, SymbolId, map.SymColorFromSymColor(lineColor), thickness, lineJoin, lineCap);
            if (secondLineColor != null)
                newSymDef.SetSecondLine(map.SymColorFromSymColor(secondLineColor), secondThickness, secondLineJoin, secondLineCap);
            if (isDashed)
                newSymDef.SetDashInfo(dashInfo);
            if (isDoubleLine) {
                DoubleLineInfo doubleLineInfo = doubleLines;
                doubleLineInfo.doubleFillColor = map.SymColorFromSymColor(doubleLineInfo.doubleFillColor);
                doubleLineInfo.doubleLeftColor = map.SymColorFromSymColor(doubleLineInfo.doubleLeftColor);
                doubleLineInfo.doubleRightColor = map.SymColorFromSymColor(doubleLineInfo.doubleRightColor);
                newSymDef.SetDoubleLines(doubleLineInfo);
            }
            if (glyphs != null) {
                GlyphInfo[] newGlyphs = new GlyphInfo[glyphs.Length];
                for (int i = 0; i < glyphs.Length; ++i) {
                    newGlyphs[i] = glyphs[i];
                    newGlyphs[i].glyph = newGlyphs[i].glyph.CopyToMap(map);
                }
                newSymDef.SetGlyphs(newGlyphs);
            }
            if (IsShortened)
                newSymDef.SetShortening(shortenInfo);

            map.AddSymdef(newSymDef);
            return newSymDef;
        }

        void CreatePens(IGraphicsTarget g, LineSymDefPens pens, bool highlight)
        {
            if (thickness > 0.0F && lineColor != null) {
                CmykColor mainColorValue = highlight ? map.HighlightColor : lineColor.ColorValue;

                if (!g.HasPen(pens.mainPen)) {
                    if (shortenInfo.shortenBeginning > 0 && shortenInfo.pointyEnds) {
                        // Always use round line cap with pointy ends.
                        g.CreatePen(pens.mainPen, mainColorValue, thickness, LineCapMode.Round, lineJoin, GraphicsUtil.MITER_LIMIT);
                    }
                    else {
                        g.CreatePen(pens.mainPen, mainColorValue, thickness, lineCap, lineJoin, GraphicsUtil.MITER_LIMIT);
                    }
                }

                if (shortenInfo.pointyEnds) {
                    if (!g.HasBrush(pens.pointyEndsBrush)) {
                        g.CreateSolidBrush(pens.pointyEndsBrush, mainColorValue);
                    }
                }
            }

            if (secondLineColor != null && secondThickness > 0) {
                CmykColor secondLineColorValue = highlight ? map.HighlightColor : secondLineColor.ColorValue;

                if (!g.HasPen(pens.secondPen)) {
                    g.CreatePen(pens.secondPen, secondLineColorValue, secondThickness, secondLineCap, secondLineJoin, GraphicsUtil.MITER_LIMIT);
                }
            }

            if (isDoubleLine) {
                if (doubleLines.doubleFillColor != null) {
                    CmykColor doubleFillColorValue = highlight ? map.HighlightColor : doubleLines.doubleFillColor.ColorValue;

                    if (!g.HasPen(pens.doubleFillPen)) {
                        GraphicsUtil.CreateSolidPen(g, pens.doubleFillPen, doubleFillColorValue, doubleLines.doubleThick, LineStyle.Mitered);
                    }
                }
                if (doubleLines.doubleLeftColor != null && doubleLines.doubleLeftWidth > 0.0F) {
                    CmykColor doubleLeftColorValue = highlight ? map.HighlightColor : doubleLines.doubleLeftColor.ColorValue;

                    if (!g.HasPen(pens.doubleLeftPen)) {
                        GraphicsUtil.CreateSolidPen(g, pens.doubleLeftPen, doubleLeftColorValue, doubleLines.doubleLeftWidth, LineStyle.FlatRounded);
                    }
                }
                if (doubleLines.doubleRightColor != null && doubleLines.doubleRightWidth > 0.0F) {
                    CmykColor doubleRightColorValue = highlight ? map.HighlightColor : doubleLines.doubleRightColor.ColorValue;
                    if (!g.HasPen(pens.doubleRightPen)) {
                        GraphicsUtil.CreateSolidPen(g, pens.doubleRightPen, doubleRightColorValue, doubleLines.doubleRightWidth, LineStyle.FlatRounded);
                    }
                }
            }
        }

        public override void FreeGdiObjects()
        {
        }


        // Does this symbol definition draw the given color. Used to determine
        // if this symbol definition draws into the current layer being draw.
        public override bool HasColor(SymColor color)
        {
            Debug.Assert(color != null);
            if (color.IsSpecialLayer)
                return false;

            if ((lineColor != null && color == lineColor && thickness > 0.0F) ||
                (secondLineColor != null && color == secondLineColor && secondThickness > 0.0F) ||
                (isDoubleLine && doubleLines.doubleFillColor != null && doubleLines.doubleFillColor == color) ||
                (isDoubleLine && doubleLines.doubleLeftWidth > 0.0F && doubleLines.doubleLeftColor != null && doubleLines.doubleLeftColor == color) ||
                (isDoubleLine && doubleLines.doubleRightWidth > 0.0F && doubleLines.doubleRightColor != null && doubleLines.doubleRightColor == color)) {
                return true;
            }

            if (glyphs != null) {
                foreach (GlyphInfo glyphInfo in glyphs) {
                    if (glyphInfo.glyph.HasColor(color))
                        return true;
                }
            }

            return false;
        }

        internal override void Draw(IGraphicsTarget g, SymPath path, SymColor color, RenderOptions renderOpts)
        {
            DrawCore(g, path, color, renderOpts, highlight: false);
        }

        internal override void DrawHighlight(IGraphicsTarget g, SymPath path, HighlightOptions highlightOptions)
        {
            if (highlightOptions.style == HighlightStyle.LowFidelity || HighlightThickness < SymbolHelpers.MinimumLineWidth(highlightOptions)) {
                SymbolHelpers.DrawLineHighlight(g, map, path, HighlightThickness, MainLineJoin, MainLineCap, highlightOptions);
            }
            else {
                RenderOptions renderOpts = new RenderOptions();
                renderOpts.minResolution = (float)highlightOptions.minResolution;
                DrawCore(g, path, null, renderOpts, true);
            }
        }

        // Draw this line symbol in the graphics along the path provided, with
        // the given color only.
        private void DrawCore(IGraphicsTarget g, SymPath path, SymColor color, RenderOptions renderOpts, bool highlight)
        {
            Debug.Assert(map != null);
            Debug.Assert(highlight || color != null);

            LineSymDefPens pens = highlight ? highlightPens : drawingPens;

            if (path.IsZeroLength)
                return;             // Don't draw anything for a zero-length path.

            CreatePens(g, pens, highlight);

            SymPath[] pathsWithShortening, pathsWithoutShortening;
            GetPathParts(path, out pathsWithShortening, out pathsWithoutShortening);

            if (lineColor != null && (highlight || color == lineColor) && thickness > 0.0F) {
                if (!isDashed) {
                    // simple drawing.
                    foreach (SymPath p in pathsWithShortening)
                        p.Draw(g, pens.mainPen);
                }
                else {
                    // Draw the dashed line.
                    foreach (SymPath p in pathsWithShortening)
                        DrawDashed(g, p, pens.mainPen, dashInfo, 0, GetVertexPieceLimit(dashInfo, thickness), renderOpts, map.MapDistanceMetric);
                }
            }

            // Draw the pointy ends of the line. If shortening has removed the whole line, this is all the line!
            if (lineColor != null && (highlight || color == lineColor) && shortenInfo.pointyEnds && thickness > 0.0F && (shortenInfo.shortenBeginning > 0.0F || shortenInfo.shortenEnd > 0.0F)) 
                DrawPointyEnds(g, path, shortenInfo.shortenBeginning, shortenInfo.shortenEnd, thickness, pens.pointyEndsBrush);

            if (secondLineColor != null && (highlight || color == secondLineColor) && secondThickness > 0.0F && path != null) {
                // note that shortened path not used for secondary line, the full length path is.
                foreach (SymPath p in pathsWithoutShortening)
                    p.Draw(g, pens.secondPen);
            }

            // Double lines don't use the shortened path, but the full-length path.
            if (isDoubleLine) {
                if (doubleLines.doubleFillColor != null && (highlight || doubleLines.doubleFillColor == color)) {
                    if (doubleLines.doubleFillDashed) {
                        foreach (SymPath p in pathsWithoutShortening)
                            DrawDashed(g, p, pens.doubleFillPen, doubleLines.doubleDashes, GetDoubleLineVertexPieceLimit(doubleLines.doubleDashes), GetVertexPieceLimit(doubleLines.doubleDashes, doubleLines.doubleThick), renderOpts, map.MapDistanceMetric);
                    }
                    else {
                        foreach (SymPath p in pathsWithoutShortening)
                            p.Draw(g, pens.doubleFillPen);
                    }
                }

                if (doubleLines.doubleLeftColor != null && (highlight || doubleLines.doubleLeftColor == color) && doubleLines.doubleLeftWidth > 0.0F) {
                    foreach (SymPath subpath in path.GetSubpaths(SymPath.DOUBLE_LEFT_STARTSTOPFLAG)) {
                        float offsetRight = -(doubleLines.doubleThick + doubleLines.doubleLeftWidth) / 2F;
                        if (doubleLines.doubleLeftDashed) {
                            DrawDashedWithOffset(g, subpath, pens.doubleLeftPen, doubleLines.doubleDashes, offsetRight, GraphicsUtil.MITER_LIMIT, GetDoubleLineVertexPieceLimit(doubleLines.doubleDashes), GetVertexPieceLimit(doubleLines.doubleDashes, doubleLines.doubleLeftWidth), renderOpts, map.MapDistanceMetric);
                        }
                        else {  
                            SymPath leftPath = subpath.OffsetRight(offsetRight, GraphicsUtil.MITER_LIMIT);
                            leftPath.Draw(g, pens.doubleLeftPen);
                        }
                    }
                }

                if (doubleLines.doubleRightColor != null && (highlight || doubleLines.doubleRightColor == color) && doubleLines.doubleRightWidth > 0.0F) {
                    foreach (SymPath subpath in path.GetSubpaths(SymPath.DOUBLE_RIGHT_STARTSTOPFLAG)) {
                        float offsetRight = (doubleLines.doubleThick + doubleLines.doubleRightWidth) / 2F;
                        if (doubleLines.doubleRightDashed) {
                            DrawDashedWithOffset(g, subpath, pens.doubleRightPen, doubleLines.doubleDashes, offsetRight, GraphicsUtil.MITER_LIMIT, GetDoubleLineVertexPieceLimit(doubleLines.doubleDashes), GetVertexPieceLimit(doubleLines.doubleDashes, doubleLines.doubleRightWidth), renderOpts, map.MapDistanceMetric);
                        }
                        else {  
                            SymPath rightPath = subpath.OffsetRight(offsetRight, GraphicsUtil.MITER_LIMIT);
                            rightPath.Draw(g, pens.doubleRightPen);
                        }
                    }
                }
            }

            if (glyphs != null) {
                foreach (GlyphInfo glyphInfo in glyphs) {
                    if (highlight || glyphInfo.glyph.HasColor(color)) {
                        GlyphInfo gi = glyphInfo;

                        if ((gi.location == GlyphLocation.Start || gi.location == GlyphLocation.End) && pathsWithShortening.Length > 0) {
                            // start and end only apply to the full path, ignoring cut parts and shortening.
                            DrawGlyphs(g, gi, path, path, color, renderOpts, highlight);
                        }
                        else {
                            for (int i = 0; i < pathsWithShortening.Length; ++i) {
                                SymPath shortPath = pathsWithShortening[i];
                                SymPath longPath = (i < pathsWithoutShortening.Length ? pathsWithoutShortening[i] : path);
                                DrawGlyphs(g, gi, shortPath, longPath, color, renderOpts, highlight);
                            }
                        }
                    }
                }
            }
        }

        // The geometry method of the line symbols: the clipping objects of the palette color coverage of a line symbol
        // (MakeClippingObjects(SymPath, SymColor)).
        internal override IReadOnlyList<ClippingObject> MakeClippingObjects(Symbol symbol, SymColor color, CoveragePrototype prototype)
        {
            Debug.Assert(prototype == CoveragePrototype.FullCoverage, "A line symbol has the full coverage only.");
            return MakeClippingObjects(((LineSymbol)symbol).Path, color);
        }

        // The clipping objects of a line of this definition along a base path in a palette color, without a drawing
        // target, in the order in which DrawCore draws the line components of the color: the main line on the pieces
        // after the shortening, its pointy ends, the second line and the fill of a double line on the pieces without
        // the shortening, the left and the right border of a double line, and the glyphs along the line. A dashed line is
        // a composed dashed line (AddDashedLine), and a border is an offset path of the base path (OffsetPathShape).
        // path: the base path.
        // color: the palette color.
        internal override IReadOnlyList<ClippingObject> MakeClippingObjects(SymPath path, SymColor color)
        {
            List<ClippingObject> clippingObjects = new List<ClippingObject>();
            if (path.IsZeroLength)
                return clippingObjects;

            SymPath[] pathsWithShortening, pathsWithoutShortening;
            GetPathParts(path, out pathsWithShortening, out pathsWithoutShortening);

            if (lineColor != null && color == lineColor && thickness > 0.0F) {
                // CreatePens makes the main pen with round caps when the start of the line has a pointy end.
                LineCapMode mainCaps = (shortenInfo.shortenBeginning > 0 && shortenInfo.pointyEnds) ? LineCapMode.Round : lineCap;
                Stroke mainStroke = new Stroke(thickness, mainCaps, lineJoin, GraphicsUtil.MITER_LIMIT);
                foreach (SymPath p in pathsWithShortening) {
                    if (isDashed)
                        AddDashedLine(clippingObjects, p, dashInfo, mainStroke, 0, 1, 0, GetVertexPieceLimit(dashInfo, thickness));
                    else
                        clippingObjects.Add(p.MakeStrokedShape(mainStroke, Placement.Identity));
                }

                if (shortenInfo.pointyEnds && (shortenInfo.shortenBeginning > 0.0F || shortenInfo.shortenEnd > 0.0F)) {
                    foreach (PointF[] tip in GetPointyEnds(path, shortenInfo.shortenBeginning, shortenInfo.shortenEnd, thickness))
                        clippingObjects.Add(new FilledShape(PathShape.FromPoints(tip, true), AreaFillMode.Winding, Placement.Identity));
                }
            }

            if (secondLineColor != null && color == secondLineColor && secondThickness > 0.0F) {
                Stroke secondStroke = new Stroke(secondThickness, secondLineCap, secondLineJoin, GraphicsUtil.MITER_LIMIT);
                foreach (SymPath p in pathsWithoutShortening)
                    clippingObjects.Add(p.MakeStrokedShape(secondStroke, Placement.Identity));
            }

            if (isDoubleLine) {
                if (doubleLines.doubleFillColor != null && color == doubleLines.doubleFillColor) {
                    Stroke fillStroke = GraphicsUtil.MakeSolidStroke(doubleLines.doubleThick, LineStyle.Mitered);
                    foreach (SymPath p in pathsWithoutShortening) {
                        if (doubleLines.doubleFillDashed)
                            AddDashedLine(clippingObjects, p, doubleLines.doubleDashes, fillStroke, 0, 1, GetDoubleLineVertexPieceLimit(doubleLines.doubleDashes), GetVertexPieceLimit(doubleLines.doubleDashes, doubleLines.doubleThick));
                        else
                            clippingObjects.Add(p.MakeStrokedShape(fillStroke, Placement.Identity));
                    }
                }

                if (doubleLines.doubleLeftColor != null && color == doubleLines.doubleLeftColor && doubleLines.doubleLeftWidth > 0.0F)
                    AddBorder(clippingObjects, path, SymPath.DOUBLE_LEFT_STARTSTOPFLAG, doubleLines.doubleLeftWidth, doubleLines.doubleLeftDashed, -(doubleLines.doubleThick + doubleLines.doubleLeftWidth) / 2F);

                if (doubleLines.doubleRightColor != null && color == doubleLines.doubleRightColor && doubleLines.doubleRightWidth > 0.0F)
                    AddBorder(clippingObjects, path, SymPath.DOUBLE_RIGHT_STARTSTOPFLAG, doubleLines.doubleRightWidth, doubleLines.doubleRightDashed, (doubleLines.doubleThick + doubleLines.doubleRightWidth) / 2F);
            }

            if (glyphs != null) {
                foreach (GlyphInfo glyphInfo in glyphs) {
                    if (!glyphInfo.glyph.HasColor(color))
                        continue;

                    if ((glyphInfo.location == GlyphLocation.Start || glyphInfo.location == GlyphLocation.End) && pathsWithShortening.Length > 0) {
                        // As in DrawCore, the start and the end apply only to the full path.
                        AddGlyphs(clippingObjects, glyphInfo, path, path, color);
                    }
                    else {
                        for (int i = 0; i < pathsWithShortening.Length; ++i)
                            AddGlyphs(clippingObjects, glyphInfo, pathsWithShortening[i], (i < pathsWithoutShortening.Length) ? pathsWithoutShortening[i] : path, color);
                    }
                }
            }

            return clippingObjects.AsReadOnly();
        }

        // Add the clipping objects of a border of a double line, as DrawCore draws it: the offset path of each subpath
        // that the flag of the border leaves, solid or dashed.
        // clippingObjects: the clipping objects, which get those of the border.
        // path: the base path.
        // flag: the start/stop flag of the border.
        // width, isDashed: the width and the dashes of the border.
        // offsetRight: the distance of the border to the right of the base path.
        private void AddBorder(List<ClippingObject> clippingObjects, SymPath path, byte flag, float width, bool isDashed, float offsetRight)
        {
            Stroke stroke = GraphicsUtil.MakeSolidStroke(width, LineStyle.FlatRounded);
            foreach (SymPath subpath in path.GetSubpaths(flag)) {
                if (isDashed)
                    AddDashedLine(clippingObjects, subpath, doubleLines.doubleDashes, stroke, offsetRight, GraphicsUtil.MITER_LIMIT, GetDoubleLineVertexPieceLimit(doubleLines.doubleDashes), GetVertexPieceLimit(doubleLines.doubleDashes, width));
                else
                    clippingObjects.Add(new StrokedShape(new OffsetPathShape(subpath, offsetRight, GraphicsUtil.MITER_LIMIT), stroke, Placement.Identity));
            }
        }

        // The tolerance within which two lengths that the drawing walks are the same in a run of dashes or a repeated
        // placement: a few units in the last place of a float, which the conversion of the measure of the quarters
        // leaves (SymPath.OcadCurveMeasure).
        private static bool AreSameLengths(float a, float b)
        {
            return Math.Abs(a - b) <= 1E-6 * Math.Max(1, Math.Max(Math.Abs(a), Math.Abs(b)));
        }

        // The regions of the edges of the flattened path within which the drawing walks the distances of a spacing
        // method at one ratio to the euclidean length (SymPath.GetEdgeRegions), and whether that ratio is the ratio of
        // each edge. The euclidean metric walks the path at its length, and the measure of the quarters converts the
        // distances to euclidean lengths that are in proportion within each quarter and, with the OCAD metric, within
        // each straight segment. The OCAD metric walks a path without that measure at a ratio of each edge.
        // path: the path.
        // spacingMethod: the spacing method.
        // isRatioOfEdge: whether the ratio is the euclidean length of each edge to its length in the metric of the map.
        private int[] GetRegions(SymPath path, SpacingMethod spacingMethod, out bool isRatioOfEdge)
        {
            bool byQuarters = IsMeasuredByQuarters(path, spacingMethod);
            isRatioOfEdge = !map.UseEuclideanMetric && !byQuarters;
            if (isRatioOfEdge)
                return path.GetEdgeRegions(false, false, true);
            return path.GetEdgeRegions(byQuarters && !map.UseEuclideanMetric, byQuarters, false);
        }

        // The ratio of the euclidean length of an edge of the flattened path to the distance that the drawing walks along
        // it: 1 with the euclidean metric, and the euclidean length to the length in the metric of the map otherwise.
        // path: the path.
        // edge: the index of the edge.
        // isRatioOfEdge: whether the ratio is that of the edge (GetRegions), and not 1.
        private double GetRatioOfEdge(SymPath path, int edge, bool isRatioOfEdge)
        {
            if (!isRatioOfEdge)
                return 1;
            PointF[] flattened = path.FlattenedPoints;
            PointF a = flattened[edge], b = flattened[edge + 1];
            double walked = map.MapDistanceMetric(a, b);
            return (walked > 0) ? SymPath.EuclidDistance(a, b) / walked : 1;
        }

        // Add the clipping objects of a dashed line along a path, as DrawDashedWithOffset draws it with no limit of
        // resolution, as a composed dashed line in the euclidean measure: the middle dashes of a segment that repeat a
        // pattern along one region of the path (GetRegions) are one run of dashes, a stroked shape whose pen has the
        // pattern, and the other dashes are single dashes, in the order of the path. A run has no dash from which a rule
        // of the vertices left a vertex out, no end dash of the line and no dash over a corner point or a dash point. The
        // shape of a run is the piece of the path from the start of its first dash to the end of its last, or the offset
        // path of that piece. A line without dashes is solid.
        // clippingObjects: the clipping objects, which get those of the line.
        // path: the path.
        // dashes: the dashes of the line.
        // stroke: the pen of the line, without dashes.
        // offsetRight, miterLimit: the offset of the line from the path, and its miter limit.
        // pathVertexLimit, drawnVertexLimit: the limits of the rules of the vertices next to the ends of a dash.
        private void AddDashedLine(List<ClippingObject> clippingObjects, SymPath path, DashInfo dashes, Stroke stroke, float offsetRight, float miterLimit, float pathVertexLimit, float drawnVertexLimit)
        {
            SymPath.DistanceMetric metric = map.MapDistanceMetric;
            float[] distances = ComputeDashDistances(path, LocationKind.DashAndGapLengths, dashes.spacingMethod, dashes.dashLength, dashes.firstDashLength, dashes.lastDashLength, dashes.gapLength, dashes.halfEndDashLengthWhenClosed, dashes.minGaps, 0, dashes.secondaryEndGaps, dashes.secondaryEndLength, dashes.secondaryMiddleGaps, dashes.secondaryMiddleLength, 1.0F, false, null, metric);
            if (distances.Length == 0) {
                if (offsetRight != 0)
                    clippingObjects.Add(new StrokedShape(new OffsetPathShape(path, offsetRight, miterLimit), stroke, Placement.Identity));
                else
                    clippingObjects.Add(path.MakeStrokedShape(stroke, Placement.Identity));
                return;
            }

            List<SymPath.DashOnPath> onPath = path.GetDashes(distances, GetMetricAlongPath(path, dashes.spacingMethod, metric), offsetRight, miterLimit, pathVertexLimit, drawnVertexLimit);
            bool isRatioOfEdge;
            int[] regions = GetRegions(path, dashes.spacingMethod, out isRatioOfEdge);
            PointF[] flatPoints = path.FlattenedPoints;
            int longestPeriod = 1 + Math.Max(0, dashes.secondaryMiddleGaps);

            int first = 0;
            while (first < onPath.Count) {
                int period;
                int last = FindRunOfDashes(onPath, distances, regions, first, longestPeriod, out period);
                if (last == first) {
                    clippingObjects.Add(new StrokedShape(PathShape.FromPoints(onPath[first].Points, false), stroke, Placement.Identity));
                    ++first;
                    continue;
                }

                SymPath.DashOnPath start = onPath[first], end = onPath[last];
                List<PointF> points = new List<PointF>();
                points.Add(start.BaseStart);
                for (int k = start.FirstPoint; k < end.EndPoint; ++k)
                    points.Add(flatPoints[k]);
                points.Add(end.BaseEnd);

                double ratio = GetRatioOfEdge(path, start.StartEdge, isRatioOfEdge);
                float[] pattern = new float[2 * period];
                for (int k = 0; k < period; ++k) {
                    pattern[2 * k] = (float)(distances[onPath[first + k].DashIndex] * ratio);
                    pattern[2 * k + 1] = (float)(distances[onPath[first + k].DashIndex + 1] * ratio);
                }

                Stroke dashed = new Stroke(stroke.Width, stroke.Caps, stroke.Join, stroke.MiterLimit, pattern, 0);
                SymPath piece = new SymPath(points.ToArray());
                Shape shape = (offsetRight != 0) ? (Shape)new OffsetPathShape(piece, offsetRight, miterLimit) : PathShape.FromPoints(points.ToArray(), false);
                clippingObjects.Add(new StrokedShape(shape, dashed, Placement.Identity));
                first = last + 1;
            }
        }

        // The last dash of the run of dashes from a first dash, or the first dash itself when no run starts there. A run
        // repeats a period of one dash and its gap, or of more dashes and their gaps for the secondary gaps, at least
        // twice. Its dashes are consecutive and in one region, and none of them is an end dash of the line, crosses a
        // corner point or a dash point, or lost a vertex to a rule of the vertices. Of the periods, the one that makes the
        // longest run is taken, and of equal runs the shortest period, so that the period of a dash with secondary gaps
        // is the whole dash.
        // dashes: the dashes of the line.
        // distances: the lengths of the dashes and the gaps in turn, a dash first.
        // regions: the regions of the edges of the flattened path (GetRegions).
        // first: the index of the first dash.
        // longestPeriod: the largest number of dashes in a period.
        // period: the number of dashes of the period of the run.
        private static int FindRunOfDashes(List<SymPath.DashOnPath> dashes, float[] distances, int[] regions, int first, int longestPeriod, out int period)
        {
            int bestLast = first, bestPeriod = 1;
            if (CanBeInRun(dashes, regions, first)) {
                for (int tried = 1; tried <= longestPeriod; ++tried) {
                    int last = first;
                    while (last + 1 < dashes.Count && FitsRun(dashes, distances, regions, first, last + 1, tried))
                        ++last;
                    if (last - first + 1 >= 2 * tried && last > bestLast) {
                        bestLast = last;
                        bestPeriod = tried;
                    }
                }
            }

            period = bestPeriod;
            return bestLast;
        }

        // Can a dash be in a run of dashes: is it within one region, not an end dash of the line, and did no rule of the
        // vertices leave a vertex out of it?
        private static bool CanBeInRun(List<SymPath.DashOnPath> dashes, int[] regions, int index)
        {
            SymPath.DashOnPath dash = dashes[index];
            if (index == 0 || index == dashes.Count - 1 || dash.IsVertexLeftOut)
                return false;
            if (dash.StartEdge >= regions.Length || dash.EndEdge >= regions.Length)
                return false;
            return regions[dash.StartEdge] >= 0 && regions[dash.StartEdge] == regions[dash.EndEdge];
        }

        // Can a dash end a run of a period from a first dash?
        private static bool FitsRun(List<SymPath.DashOnPath> dashes, float[] distances, int[] regions, int first, int next, int period)
        {
            if (!CanBeInRun(dashes, regions, next))
                return false;
            SymPath.DashOnPath dash = dashes[next], previous = dashes[next - 1], firstDash = dashes[first];
            if (dash.DashIndex != previous.DashIndex + 2 || dash.DashIndex + 1 >= distances.Length)
                return false;
            if (regions[dash.StartEdge] != regions[firstDash.StartEdge])
                return false;

            // The dash and the gap before it repeat those of the period.
            SymPath.DashOnPath same = dashes[first + (next - first) % period];
            SymPath.DashOnPath sameBefore = dashes[first + (next - 1 - first) % period];
            return AreSameLengths(distances[dash.DashIndex], distances[same.DashIndex]) && AreSameLengths(distances[previous.DashIndex + 1], distances[sameBefore.DashIndex + 1]);
        }

        // Add the clipping objects of the glyphs along the path in a palette color, as DrawGlyphs draws them. "longPath"
        // is the same as path unless shortening of the ends has occurred, in which case path is the shortened path (used
        // for all glyphs except start and end), and longPath is used for the start and end.
        // clippingObjects: the clipping objects, which get those of the glyphs.
        // glyphInfo: the glyphs.
        // color: the palette color.
        private void AddGlyphs(List<ClippingObject> clippingObjects, GlyphInfo glyphInfo, SymPath path, SymPath longPath, SymColor color)
        {
            float[] distances;
            PointF[] points;
            float[] perpAngles, subtendedAngles;
            float firstDistance;
            bool ignoreDashCornersAtEnds = false;
            SpacingMethod spacingMethod = glyphInfo.spacingMethod;      // The method that fits the distances of the glyphs.

            switch (glyphInfo.location) {
            case GlyphLocation.CornersIgnoreEnds:
            case GlyphLocation.DashPointsIgnoreEnds:
                ignoreDashCornersAtEnds = true;
                goto case GlyphLocation.Corners;

            case GlyphLocation.Corners:
            case GlyphLocation.DashPoints:
                // As in DrawGlyphs, a glyph at a corner is stretched by the angle that the corner subtends.
                points = path.FindCornerOrDashPoints((glyphInfo.location == GlyphLocation.DashPoints || glyphInfo.location == GlyphLocation.DashPointsIgnoreEnds) ? PointKind.Dash : PointKind.Corner,
                                                     ignoreDashCornersAtEnds, out perpAngles, out subtendedAngles);

                if (points != null) {
                    for (int i = 0; i < points.Length; ++i) {
                        float subtendedAngle = subtendedAngles[i];
                        float stretch;
                        if (subtendedAngle != 0 && !glyphInfo.noScaleAtCorners && (glyphInfo.location == GlyphLocation.Corners || glyphInfo.location == GlyphLocation.CornersIgnoreEnds))
                            stretch = Geometry.MiterFactor(subtendedAngle);
                        else
                            stretch = 1.0F;
                        stretch = Math.Min(stretch, CORNER_GLYPH_STRETCH_LIMIT);

                        Matrix stretchMatrix = new Matrix();
                        stretchMatrix.Scale(1.0F, stretch);

                        clippingObjects.AddRange(glyphInfo.glyph.MakeClippingObjects(points[i], perpAngles[i] + 90.0F, null, color, stretchMatrix));
                    }
                }
                return;

            case GlyphLocation.Spaced:
                distances = ComputeDashDistances(path, LocationKind.GapCenters, glyphInfo.spacingMethod, glyphInfo.distance, glyphInfo.firstDistance, glyphInfo.lastDistance, 0, false, glyphInfo.minimum, 0, 0, 0, 0, 0, 1.0F, false, null, map.MapDistanceMetric);
                break;
            case GlyphLocation.SpacedOffset:
                distances = ComputeDashDistances(path, LocationKind.GapCentersOffset, glyphInfo.spacingMethod, glyphInfo.distance, glyphInfo.firstDistance, glyphInfo.lastDistance, 0, false, glyphInfo.minimum, glyphInfo.offset, 0, 0, 0, 0, 1.0F, false, null, map.MapDistanceMetric);
                break;
            case GlyphLocation.SpacedDecrease:
                // The glyphs decrease in size and in spacing, so each is a single placement, with shapes of its own.
                DecreaseRanges decreaseRanges = new DecreaseRanges();
                distances = ComputeDashDistances(path, LocationKind.GapCentersDecrease, glyphInfo.spacingMethod, glyphInfo.distance, glyphInfo.firstDistance, glyphInfo.lastDistance, 0, false, glyphInfo.minimum, 0, 0, 0, 0, 0, glyphInfo.decreaseLimit, glyphInfo.decreaseBothEnds, decreaseRanges, map.MapDistanceMetric);

                if (distances != null && distances.Length > 0) {
                    firstDistance = distances[0];

                    for (int n = 0; n < glyphInfo.number; ++n) {
                        distances[0] = firstDistance - ((glyphInfo.number - 1 - n * 2) * (glyphInfo.spacing / 2.0F));

                        points = path.FindPointsAlongLine(distances, out perpAngles, GetMetricAlongPath(path, spacingMethod, map.MapDistanceMetric));

                        for (int i = 0; i < points.Length; ++i) {
                            float decreaseFactor = ComputeDecreaseFactor(i, decreaseRanges, glyphInfo.decreaseLimit);

                            if (decreaseFactor >= 0.001F) {
                                Matrix matrixTransform = new Matrix();
                                matrixTransform.Scale(decreaseFactor, decreaseFactor);
                                clippingObjects.AddRange(glyphInfo.glyph.MakeClippingObjects(points[i], perpAngles[i], null, color, matrixTransform));
                            }
                        }
                    }
                }

                return;
            case GlyphLocation.DashCenters:
                spacingMethod = dashInfo.spacingMethod;
                distances = ComputeDashDistances(path, LocationKind.DashCenters, dashInfo.spacingMethod, dashInfo.dashLength, dashInfo.firstDashLength, dashInfo.lastDashLength, dashInfo.gapLength, dashInfo.halfEndDashLengthWhenClosed, dashInfo.minGaps, 0, 0, 0, 0, 0, 1.0F, false, null, map.MapDistanceMetric);
                break;
            case GlyphLocation.GapCenters:
                // OCAD doesn't respect the "0 minimum gaps" for the symbols, although it does for the gaps. Always have at least one symbol. This is handled on import by having glyphInfo.minimum be 1.
                spacingMethod = dashInfo.spacingMethod;
                distances = ComputeDashDistances(path, LocationKind.GapCenters, dashInfo.spacingMethod, dashInfo.dashLength, dashInfo.firstDashLength, dashInfo.lastDashLength, dashInfo.gapLength, dashInfo.halfEndDashLengthWhenClosed, Math.Max(glyphInfo.minimum, dashInfo.minGaps), 0, 0, 0, 0, 0, 1.0F, false, null, map.MapDistanceMetric);
                break;
            case GlyphLocation.Start:
                distances = new float[1] { 0 };
                break;
            case GlyphLocation.End:
                distances = new float[1] { map.UseEuclideanMetric ? longPath.Length : longPath.BizzarroLength };
                break;
            default:
                Debug.Fail("bad glyph location");
                return;
            }

            if (distances == null || distances.Length == 0)
                return;
            firstDistance = distances[0];

            for (int n = 0; n < glyphInfo.number; ++n) {
                distances[0] = firstDistance - ((glyphInfo.number - 1 - n * 2) * (glyphInfo.spacing / 2.0F));

                SymPath walkedPath;
                SymPath.DistanceMetric metric;
                SpacingMethod regionSpacing = spacingMethod;
                if (glyphInfo.location != GlyphLocation.Start && glyphInfo.location != GlyphLocation.End) {
                    bool isEuclideanOnOcadMetric = !map.UseEuclideanMetric && IsMeasuredByQuarters(path, spacingMethod);
                    if (longPath.FirstPoint != path.FirstPoint && shortenInfo.shortenBeginning > 0 && !isEuclideanOnOcadMetric) {
                        // This is a shortened path, convert distances back into the long path.
                        distances[0] += shortenInfo.shortenBeginning;
                        walkedPath = longPath;
                        metric = map.MapDistanceMetric;
                        regionSpacing = map.UseEuclideanMetric ? spacingMethod : SpacingMethod.OpenMapperDashes;
                    }
                    else {
                        walkedPath = path;
                        metric = GetMetricAlongPath(path, spacingMethod, map.MapDistanceMetric);
                    }
                }
                else {
                    walkedPath = longPath;
                    metric = map.MapDistanceMetric;
                    regionSpacing = SpacingMethod.OpenMapperDashes;
                }

                points = walkedPath.FindPointsAlongLine(distances, out perpAngles, metric);
                AddGlyphsAtPoints(clippingObjects, glyphInfo.glyph, color, walkedPath, metric, regionSpacing, distances, points, perpAngles);
            }
        }

        // Add the clipping objects of glyphs at their points along a path in a palette color. Each sequence of glyphs at
        // one step in one region of the path (GetRegions) is a repeated placement of the shared shapes of the glyph, and
        // the other glyphs are single placements: the glyphs at the ends of the parts between corner and dash points,
        // whose steps to the glyphs of the end dashes and to the next part differ, and a glyph whose step to the next
        // glyph differs from the step after it.
        // clippingObjects: the clipping objects, which get those of the glyphs.
        // glyph: the glyph.
        // color: the palette color.
        // walkedPath: the path along which FindPointsAlongLine found the points.
        // metric: the metric that walked the distances.
        // regionSpacing: the spacing method whose measure walked the distances, for the regions of the path.
        // distances: the distance of each glyph from the one before it, and of the first glyph from the start of the path.
        // points, angles: the points and the angles of the glyphs, as FindPointsAlongLine gives them.
        private void AddGlyphsAtPoints(List<ClippingObject> clippingObjects, Glyph glyph, SymColor color, SymPath walkedPath, SymPath.DistanceMetric metric, SpacingMethod regionSpacing, float[] distances, PointF[] points, float[] angles)
        {
            if (points == null)
                return;

            bool isRatioOfEdge;
            int[] regions = GetRegions(walkedPath, regionSpacing, out isRatioOfEdge);
            int[] edges = walkedPath.FindEdgesAlongLine(distances, metric);
            Func<int, int> regionOf = i => (edges[i] < regions.Length) ? regions[edges[i]] : -1;

            int first = 0;
            while (first < points.Length) {
                int last = first;
                if (regionOf(first) >= 0) {
                    while (last + 1 < points.Length && regionOf(last + 1) == regionOf(first) && (last == first || AreSameLengths(distances[last + 1], distances[first + 1])))
                        ++last;
                    // Of two glyphs whose next step differs, the first is single, and the second may start a sequence.
                    if (last == first + 1 && last + 1 < points.Length && regionOf(last + 1) == regionOf(first))
                        last = first;
                }

                if (last > first) {
                    double start = 0;
                    for (int i = 0; i <= first; ++i)
                        start += distances[i];
                    int firstEdge = edges[first], lastEdge = edges[last];
                    double ratio = GetRatioOfEdge(walkedPath, firstEdge, isRatioOfEdge);
                    float firstOnPath = (float)((start - walkedPath.LengthToFlattenedPoint(firstEdge, metric)) * ratio);
                    PointF[] flattened = walkedPath.FlattenedPoints;
                    PointF[] pathPoints = new PointF[lastEdge - firstEdge + 2];
                    Array.Copy(flattened, firstEdge, pathPoints, 0, pathPoints.Length);

                    RepeatedPlacement placement = new RepeatedPlacement(Placement.Identity, PathShape.FromPoints(pathPoints, false), firstOnPath, (float)(distances[first + 1] * ratio), last - first + 1, !glyph.IsSimple);
                    clippingObjects.AddRange(glyph.MakeRepeatedClippingObjects(placement, color));
                    first = last + 1;
                }
                else {
                    clippingObjects.AddRange(glyph.MakeClippingObjects(points[first], angles[first], null, color));
                    ++first;
                }
            }
        }

        // The polygons of the pointy ends of a line, as DrawPointyEnds fills them: five points each, the tip first, at the
        // start of the line when pointyLengthStart is more than 0 and then at its end when pointyLengthEnd is.
        // longPath: the path of the line, without the shortening.
        // pointyLengthStart, pointyLengthEnd: the lengths of the pointy ends.
        // lineWidth: the width of the line.
        private List<PointF[]> GetPointyEnds(SymPath longPath, float pointyLengthStart, float pointyLengthEnd, float lineWidth)
        {
            float length = map.UseEuclideanMetric ? longPath.Length : longPath.BizzarroLength;
            float[] distances, angles;
            if (length >= pointyLengthStart + pointyLengthEnd) {
                distances = new float[6] { 0, pointyLengthStart / 2, pointyLengthStart / 2, length - pointyLengthEnd - pointyLengthStart, pointyLengthEnd / 2, pointyLengthEnd / 2 };
            }
            else {
                float scaleFactor = length / (pointyLengthStart + pointyLengthEnd);
                distances = new float[6] { 0, (pointyLengthStart / 2) * scaleFactor, (pointyLengthStart / 2) * scaleFactor, 0, (pointyLengthEnd / 2) * scaleFactor, (pointyLengthEnd / 2) * scaleFactor };
            }
            PointF[] pointsAlongPath = longPath.FindPointsAlongLine(distances, out angles, map.MapDistanceMetric);
            float midpointWidth = lineWidth * 0.666F;

            List<PointF[]> tips = new List<PointF[]>(2);
            if (pointyLengthStart > 0) {
                PointF[] tip = new PointF[5];
                tip[0] = pointsAlongPath[0];
                tip[1] = Geometry.MoveDistance(pointsAlongPath[1], midpointWidth / 2, angles[1] - 90.0F);
                tip[4] = Geometry.MoveDistance(pointsAlongPath[1], midpointWidth / 2, angles[1] + 90.0F);
                tip[2] = Geometry.MoveDistance(pointsAlongPath[2], lineWidth / 2, angles[2] - 90.0F);
                tip[3] = Geometry.MoveDistance(pointsAlongPath[2], lineWidth / 2, angles[2] + 90.0F);
                tips.Add(tip);
            }

            if (pointyLengthEnd > 0) {
                PointF[] tip = new PointF[5];
                tip[0] = pointsAlongPath[5];
                tip[1] = Geometry.MoveDistance(pointsAlongPath[4], midpointWidth / 2, angles[4] - 90.0F);
                tip[4] = Geometry.MoveDistance(pointsAlongPath[4], midpointWidth / 2, angles[4] + 90.0F);
                tip[2] = Geometry.MoveDistance(pointsAlongPath[3], lineWidth / 2, angles[3] - 90.0F);
                tip[3] = Geometry.MoveDistance(pointsAlongPath[3], lineWidth / 2, angles[3] + 90.0F);
                tips.Add(tip);
            }

            return tips;
        }

        // Apply the shortening and the start/stop flags to get an array of the paths to draw. This might be an empty array.
        private void GetPathParts(SymPath path, out SymPath[] withShortening, out SymPath[] withoutShortening) {
            withoutShortening = path.GetSubpaths(SymPath.MAIN_STARTSTOPFLAG);

            if (shortenInfo.shortenBeginning > 0.0F || shortenInfo.shortenEnd > 0.0F) {
                SymPath shortenedPath = path.Shorten(shortenInfo.shortenBeginning, shortenInfo.shortenEnd, map.MapDistanceMetric);
                if (shortenedPath == null)
                    withShortening = new SymPath[0] { };
                else
                    withShortening = shortenedPath.GetSubpaths(SymPath.MAIN_STARTSTOPFLAG);
            }
            else {
                withShortening = withoutShortening;
            }
        }

        // Unbelievably, it appears as though OCAD actually uses Bizarro distance metric (see SymPath) to do layout of dashes. This is so completely insane that I can't
        // even fathom it. 

        // Compute the distances along the line between dashes on the line. The distance are all deltas from the previous
        // distance. Can compute the start and end of the dashes/gaps (i.e., the lengths of the dashes and gaps), the distances
        // to the centers of the dashes (possibly excluded the first/last dash), or the distances to the centers of the gaps.
        // When computing the dash/gap lengths, the array returned is always of odd length, as follows:
        //    one element -- indicates that it should be fully drawn -- it's all dash.
        //    three elements -- lengths of the dash, gap, and dash
        //    five elements -- dash, gap, dash, gap, dash length, etc.
        //   it's possible that dashes may be of length zero, indicating that they shouldn't be drawn. For example, 
        //   if a very short line has a minimum # of gaps > 0, the line may be "all gap", which would
        //   be returns as a three-element array: 0, full-length, 0
        //
        //  Note: parameter "offset" is only used for MiddleDashCentersOffset! It does not otherwise affect the dashes.
        enum LocationKind { 
            DashAndGapLengths,      // gives lengths of both the dashes and the gaps
            DashCenters,                   // gives locations (as delta distances) of dash centers
            GapCenters,                     // gives locations of the gap centers
            GapCentersOffset,   // gives locations of gap centers, but offset forward by "offset", and missing the last 
            GapCentersDecrease,    // gives locations of the gap centers, but with decreasing distances based on final decrease value and a decrease toward both ends boolean
        }

        private class DecreaseRanges
        {
            public int startSmallToLarge, countSmallToLarge;
            public int startLargeToSmall, countLargeToSmall;
            public int startBoth, countBoth;
        }

        private static float[] ComputeDashDistances(SymPath path, LocationKind kind, SpacingMethod spacingMethod, 
                                                    float dashLength, float firstDashLength, float lastDashLength, float gapLength,
                                                    bool halfEndDashLengthWhenClosed, int minGaps, float offset, 
                                                    int numEndSecGaps, float lengthEndSecGaps, int numMiddleSecGaps, float lengthMiddleSecGaps, 
                                                    float decreaseLimit, bool decreaseBothEnds, DecreaseRanges decreaseRanges, SymPath.DistanceMetric distanceMetric)
        {
            LocationKind originalKind = kind;   // save original kind.

            // Get length of each segment, deliniated by corner points. With each metric, OCAD places the dashes on Bezier
            // curves by their quarters, and the glyphs of the OCAD spacing are placed in the same measure: the distances
            // are fitted to the measure of the quarters, in the metric of the map, and converted at the end to euclidean
            // distances along the path, which the drawing walks with the euclidean metric (GetMetricAlongPath).
            PointKind[] pointkinds;
            float[] segmentLengths = path.GetCornerAndDashPointDistances(distanceMetric, out pointkinds);
            SymPath.OcadCurveMeasure curveMeasure = (spacingMethod == SpacingMethod.OCAD) ? path.GetOcadCurveMeasure(distanceMetric) : null;
            if (curveMeasure != null)
                segmentLengths = curveMeasure.PartMeasures;
            float[] countingLengths = (spacingMethod == SpacingMethod.OCAD) ? path.GetCornerAndDashPointLengthsForCounting(distanceMetric) : segmentLengths;
            float[] lengthAtEnd = new float[segmentLengths.Length];
            float[][] dashDistances = new float[segmentLengths.Length][];
            int segmentSmallToLarge = -1, segmentLargeToSmall = -1, segmentBothEnds = -1;  // segment numbers with different decrease kinds.

            // For dash centers, first compute the dashes, then find the centers.
            if (kind == LocationKind.DashCenters)
                kind = LocationKind.DashAndGapLengths;  

            // Respect the halfEndDashLengthWhenClosed flag.
            if (path.IsClosedCurve && halfEndDashLengthWhenClosed) {
                firstDashLength /= 2.0F;
                lastDashLength /= 2.0F;
            }

            // Determine longest segment. This is the one that minGaps applies to.
            int segmentLongest = 0;
            float longestSegmentLength = 0;
            for (int i = 0; i < dashDistances.Length; ++i) {
                if (segmentLengths[i] >= longestSegmentLength) {
                    longestSegmentLength = segmentLengths[i];
                    segmentLongest = i;
                }
            }

            // Compute dash distances for each segment of the path.
            int totalDistances = 0;
            int minGapsLeft = minGaps;
            for (int i = 0; i < dashDistances.Length; ++i) {
                bool firstPointIsDashPoint = (i != 0 && pointkinds[i] == PointKind.Dash);
                bool lastPointIsDashPoint = (i < dashDistances.Length - 1 && pointkinds[i + 1] == PointKind.Dash);

                // Open mapper doesn't half dash points if there are secondary gaps.
                if (spacingMethod != SpacingMethod.OCAD && numMiddleSecGaps > 0) {
                    firstPointIsDashPoint = lastPointIsDashPoint = false;
                }

                // Compute first and last lengths of the segment in question.
                float firstLength, lastLength;
                if (firstPointIsDashPoint)
                    firstLength = dashLength / 2;
                else
                    firstLength = firstDashLength;

                if (lastPointIsDashPoint)
                    lastLength = dashLength / 2;
                else
                    lastLength = lastDashLength;

                // Note that minGaps is only required on the LONGEST segment. This is because that the minGaps requirement is for
                // all segments together, not each segment. Except for Open Mapper mid symbols.
                int numGaps;

                // In a multi-segment line, set decreasing for first/last segment only.
                DecreaseKind decreaseKind = DecreaseKind.None;
                if (kind == LocationKind.GapCentersDecrease) {
                    if (decreaseBothEnds) {
                        if (dashDistances.Length == 1) {
                            decreaseKind = DecreaseKind.BothEnds;
                            segmentBothEnds = i;
                        }
                        else if (i == 0) {
                            decreaseKind = DecreaseKind.SmallToLarge;
                            segmentSmallToLarge = i;
                        }
                        else if (i == dashDistances.Length - 1) {
                            decreaseKind = DecreaseKind.LargeToSmall;
                            segmentLargeToSmall = i;
                        }
                    }
                    else {
                        if (i == dashDistances.Length - 1) {
                            decreaseKind = DecreaseKind.LargeToSmall;
                            segmentLargeToSmall = i;
                        }
                    }
                }

                int minGapsThisSegment = 0;
                if (i == segmentLongest || segmentLengths[i] >= segmentLengths[segmentLongest] - 0.1 || spacingMethod == SpacingMethod.OpenMapperMidSymbols)
                    minGapsThisSegment = minGaps;

                dashDistances[i] = ComputeDashDistances(segmentLengths[i], countingLengths[i], kind, spacingMethod, dashLength, firstLength, lastLength, gapLength,
                                                        minGapsThisSegment, offset, 
                                                        numEndSecGaps, lengthEndSecGaps, numMiddleSecGaps, lengthMiddleSecGaps, 
                                                        decreaseLimit, decreaseKind, 
                                                        out lengthAtEnd[i], out numGaps);

                totalDistances += dashDistances[i].Length;

                // The last dash and first dash of adjacent parts merge together.
                if (kind == LocationKind.DashAndGapLengths && i > 0)
                    --totalDistances;
            }

            float[] distances;

            if (dashDistances.Length == 1) {
                if (segmentSmallToLarge == 0) {
                    decreaseRanges.startSmallToLarge = 0; decreaseRanges.countSmallToLarge = dashDistances[0].Length;
                }
                else if (segmentLargeToSmall == 0) {
                    decreaseRanges.startLargeToSmall = 0; decreaseRanges.countLargeToSmall = dashDistances[0].Length;
                }
                else if (segmentBothEnds == 0) {
                    decreaseRanges.startBoth = 0; decreaseRanges.countBoth = dashDistances[0].Length;
                }

                distances = dashDistances[0];
            }
            else {
                // Combine the distances from each segment into a single array. For dash and gap lengths, combine the 
                // dash lengths around segment boundaries.
                distances = new float[totalDistances];

                int index = 0;
                for (int i = 0; i < dashDistances.Length; ++i) {
                    Array.Copy(dashDistances[i], 0, distances, index, dashDistances[i].Length);

                    if (decreaseRanges != null) {
                        if (segmentSmallToLarge == i) {
                            decreaseRanges.startSmallToLarge = index; decreaseRanges.countSmallToLarge = dashDistances[i].Length;
                        }
                        else if (segmentLargeToSmall == i) {
                            decreaseRanges.startLargeToSmall = index; decreaseRanges.countLargeToSmall = dashDistances[i].Length;
                        }
                        else if (segmentBothEnds == i) {
                            decreaseRanges.startBoth = index; decreaseRanges.countBoth = dashDistances[i].Length;
                        }
                    }

                    if (i > 0 && dashDistances[i].Length > 0) {
                        distances[index] += lengthAtEnd[i - 1];
                    }
                    if (i > 0 && (dashDistances[i].Length == 0 || (kind == LocationKind.DashAndGapLengths && dashDistances[i].Length == 1)))
                        lengthAtEnd[i] += lengthAtEnd[i - 1];

                    index += dashDistances[i].Length;
                    if (kind == LocationKind.DashAndGapLengths)
                        --index;
                }
            }

            if (kind == LocationKind.DashAndGapLengths)
                distances = RemoveZeroGaps(distances);

            if (originalKind == LocationKind.DashCenters) {
                distances = ComputeDashCenters(distances, path.IsClosedCurve);
            }

            if (curveMeasure != null && distances != null)
                distances = curveMeasure.ToDistancesAlongPath(distances);

            return distances;
        }

        private enum DecreaseKind { None, SmallToLarge, LargeToSmall, BothEnds }

        // Computes the dash distances for a single segment. Also returns the distance from the last location to the end of the segment.
        // If LocationKind is DashesAndGapLengths, this is a duplicate of the last array element.
        private static float[] ComputeDashDistances(float pathLength, float countingLength, LocationKind kind, SpacingMethod spacingMethod, 
                                                    float dashLength, float firstDashLength, float lastDashLength, float gapLength, int minGaps, float offset, 
                                                    int numEndSecGaps, float lengthEndSecGaps, int numMiddleSecGaps, float lengthMiddleSecGaps, float decreaseLimit, DecreaseKind decreaseKind, 
                                                    out float lengthAtEnd, out int actualGaps)
        {
            int numGaps;		         // actual number of gaps in the line
            float actualDashLength;      // actual length of each dash
            float actualFirstDashLength; // actual length of first dash
            float actualLastDashLength;  // actual last dash length.

            if (decreaseKind == DecreaseKind.None && kind == LocationKind.GapCentersDecrease)
                kind = LocationKind.GapCenters;

            if (kind == LocationKind.GapCentersDecrease) {
                // Computer number of dashes based on average dash length.
                dashLength = (dashLength + (dashLength * decreaseLimit)) / 2;
            }

            // DashCenters should be done at a higher level.
            if (kind == LocationKind.DashCenters)
                throw new ArgumentException("Should not pass DashCenters to this function");

            // Computer the number of gaps
            // The number of gaps is adjusted so that the gap lengths are always preserved exactly, and the dash lengths are as close as possible to the actual dash lengths.
            if (dashLength + gapLength > 0) {
                if (spacingMethod == SpacingMethod.OCAD) {
                    if (countingLength - firstDashLength - lastDashLength <= gapLength) {
                        numGaps = (minGaps >= 1 || countingLength > firstDashLength / 2 + lastDashLength / 2 + gapLength) ? 1 : 0;
                    }
                    else {
                        // OCAD rounds the number of dashes, one more than the number of gaps, to the even number at a half.
                        numGaps = (int)Math.Round((countingLength + dashLength - firstDashLength - lastDashLength) / (dashLength + gapLength) + 1, MidpointRounding.ToEven) - 1;
                    }
                    //if (firstDashLength == 0 && lastDashLength == 0 && pathLength < dashLength / 2) {
                    //    numGaps = 0;
                    //}
                }
                else if (spacingMethod == SpacingMethod.OpenMapperMidSymbols) {
                    // Taken roughly from LineSymbol::createMidSymbolRenderables in OpenMapper. 

                    int mid_symbol_num_gaps = 0; // mid_symbols_per_spot - 1;
                    double segment_length_f = dashLength;
                    double end_length_f = (firstDashLength + lastDashLength) / 2; 
                    double end_length_twice_f = firstDashLength + lastDashLength;
                    double mid_symbol_distance_f = 0;   // 0.001 * mid_symbol_distance;
                    double mid_symbols_length = mid_symbol_num_gaps * mid_symbol_distance_f;


                    // The total length of the current continuous part
                    double length = pathLength;
                    // The length which is available for placing mid symbols
                    double segmented_length = Math.Max(0.0, length - end_length_twice_f) - mid_symbols_length;
                    // The number of segments to be created by mid symbols
                    double segment_count_raw = Math.Max((end_length_f == 0) ? 1.0 : 0.0, (segmented_length / (segment_length_f + mid_symbols_length)));
                    int lower_segment_count = (int)Math.Floor(segment_count_raw);
                    int higher_segment_count = (int)Math.Ceiling(segment_count_raw);
                    int segment_count;

                    if (end_length_f > 0) {
                        double lower_abs_deviation = Math.Abs(length - lower_segment_count * segment_length_f - (lower_segment_count + 1) * mid_symbols_length - end_length_twice_f);
                        double higher_abs_deviation = Math.Abs(length - higher_segment_count * segment_length_f - (higher_segment_count + 1) * mid_symbols_length - end_length_twice_f);
                        segment_count = (lower_abs_deviation >= higher_abs_deviation) ? higher_segment_count : lower_segment_count;

                        if (higher_segment_count > 0 || length > end_length_twice_f - 0.5 * (segment_length_f + mid_symbols_length))
                            numGaps = segment_count + 1;
                        else
                            numGaps = 0;
                    }
                    else {
                        // end_length == 0
                        double lower_segment_deviation = Math.Abs(length - lower_segment_count * segment_length_f - (lower_segment_count + 1) * mid_symbols_length) / lower_segment_count;
                        double higher_segment_deviation = Math.Abs(length - higher_segment_count * segment_length_f - (higher_segment_count + 1) * mid_symbols_length) / higher_segment_count;
                        segment_count = (lower_segment_deviation > higher_segment_deviation) ? higher_segment_count : lower_segment_count;
                        numGaps = segment_count + 1;
                    }

                }
                else {
                    // Taken roughly from LineSymbol::createDashGroups in OpenMapper. 
                    int numShortDash = ((firstDashLength < 0.6F * dashLength) ? 1 : 0) + ((lastDashLength < 0.6F * dashLength) ? 1 : 0);
                    double numberGaps = (pathLength + dashLength - firstDashLength - lastDashLength + gapLength * numShortDash) / (dashLength + gapLength);
                    double floorNumberGaps = Math.Floor(numberGaps);
                    double ceilNumberGaps = Math.Ceiling(numberGaps);
                    double floorDeviation = (pathLength + gapLength - (floorNumberGaps + 1) * (dashLength + gapLength)) / (floorNumberGaps + 1);
                    double ceilDeviation = - (pathLength + gapLength - (ceilNumberGaps + 1) * (dashLength + gapLength)) / (ceilNumberGaps + 1);
                    numGaps = (int) ((floorDeviation > ceilDeviation) ? ceilNumberGaps : floorNumberGaps);

                    double minimumOptimumLength = (2 * (dashLength + gapLength));
                    double minimumOptimumNumDashes = ((numMiddleSecGaps + 1) * 2.0 - numShortDash * 0.5);
                    double switchDeviation = 0.2 * (dashLength + gapLength) / (numMiddleSecGaps + 1);
                    if (floorNumberGaps == 0 && pathLength <  minimumOptimumLength - minimumOptimumNumDashes * switchDeviation) {
                        numGaps = 0;
                    }

                    if (numGaps < minGaps)
                        numGaps = minGaps;
                }
            }
            else
                numGaps = 0;

            // Enforce minimum gaps.
            if (numGaps < minGaps)
                numGaps = minGaps;

            // OCAD draws a dashed line as solid when its one gap would leave end dashes of 0, which fit no dash.
            if (spacingMethod == SpacingMethod.OCAD && kind == LocationKind.DashAndGapLengths && numGaps == 1 && firstDashLength == 0 && lastDashLength == 0)
                numGaps = 0;

            // Given the number of gaps, compute the lengths of the dashes. Gaps are always at their nominal length.
            if (numGaps == 0)
                actualDashLength = actualFirstDashLength = actualLastDashLength = pathLength;
            else {
                if (numGaps == 1 && firstDashLength == 0 && lastDashLength == 0)
                    actualDashLength = actualFirstDashLength = actualLastDashLength = 0;
                else {
                    actualDashLength = (pathLength - gapLength * numGaps) / (numGaps - 1 + firstDashLength / dashLength + lastDashLength / dashLength);
                    actualFirstDashLength = actualDashLength * firstDashLength / dashLength;
                    actualLastDashLength = actualDashLength * lastDashLength / dashLength;
                }
            }
            actualGaps = numGaps;

            if (actualDashLength <= 0) {
                // Special case: the path is "all gap".
                if (kind == LocationKind.GapCenters) {
                    if (numGaps > 1) {
                        // multiple gaps. Put at either end.
                        lengthAtEnd = 0;
                        return new float[2] { 0, pathLength }; // in middle of single gap
                    }
                    //else if (firstDashLength == 0 && lastDashLength == 0) {
                    //    lengthAtEnd = pathLength;
                    //    return new float[1] { 0 };
                    //}
                    else {
                        lengthAtEnd = pathLength / 2;
                        return new float[1] { pathLength / 2 }; // in middle of single gap
                    }
                }
                else if (kind == LocationKind.DashCenters || kind == LocationKind.GapCentersOffset) {
                    lengthAtEnd = pathLength;
                    return new float[0];
                }
                else {
                    lengthAtEnd = 0.0F;
                    return new float[3] { 0.0F, pathLength, 0.0F };
                }
            }

            int index = 0;
            float[] locations;

            if (kind == LocationKind.DashCenters) {
                locations = new float[numGaps + 1];

                locations[index++] = actualFirstDashLength / 2;
                lengthAtEnd = actualLastDashLength / 2;
                if (numGaps == 0)
                    return locations;
                else if (numGaps == 1)
                    locations[index++] = actualLastDashLength + gapLength;
                else {
                    locations[index++] = actualFirstDashLength / 2 + gapLength + actualDashLength / 2;
                    for (int i = 2; i < numGaps; ++i)
                        locations[index++] = actualDashLength + gapLength;
                    locations[index++] = actualLastDashLength / 2 + gapLength + actualDashLength / 2;
                }
            }
            else if (kind == LocationKind.GapCenters) {
                locations = new float[numGaps];

                if (numGaps == 0)
                    lengthAtEnd = pathLength;
                else {
                    lengthAtEnd = actualLastDashLength + gapLength / 2;
                    locations[index++] = actualFirstDashLength + gapLength / 2;
                    for (int i = 1; i < numGaps; ++i)
                        locations[index++] = actualDashLength + gapLength;
                }
            }
            else if (kind == LocationKind.GapCentersDecrease) {
                locations = new float[numGaps];

                if (numGaps == 0)
                    lengthAtEnd = pathLength;
                else {
                    lengthAtEnd = actualLastDashLength + gapLength / 2;
                    locations[index++] = actualFirstDashLength + gapLength / 2;

                    if (numGaps > 1) {
                        if (decreaseKind == DecreaseKind.BothEnds) {
                            float currentDashLength = 2 * actualDashLength / (1 + decreaseLimit);
                            if (numGaps % 2 == 1) {
                                float dashDelta = (currentDashLength - decreaseLimit * currentDashLength) / (numGaps - 1);
                                currentDashLength = decreaseLimit * currentDashLength;

                                for (int i = 1; i < numGaps; ++i) {
                                    if (i == (numGaps + 1) / 2)
                                        dashDelta = -dashDelta;

                                    currentDashLength += dashDelta;
                                    locations[index++] = currentDashLength + gapLength;
                                    currentDashLength += dashDelta;
                                }
                            }
                            else {
                                float dashDelta;
                                if (numGaps == 2) 
                                    dashDelta = 0;
                                else {
                                    dashDelta = currentDashLength * (((1.0F - (1.0F - decreaseLimit) / numGaps) - decreaseLimit) / (numGaps - 2));
                                    currentDashLength = decreaseLimit * currentDashLength;
                                }

                                for (int i = 1; i < numGaps; ++i) {
                                    if (i != numGaps / 2)
                                        currentDashLength += dashDelta;
                                    locations[index++] = currentDashLength + gapLength;

                                    if (i == numGaps / 2)
                                        dashDelta = -dashDelta;
                                    else
                                        currentDashLength += dashDelta;
                                }
                            }
                        }
                        else if (decreaseKind == DecreaseKind.LargeToSmall) {
                            float currentDashLength = 2 * actualDashLength / (1 + decreaseLimit);
                            float dashDelta = -(currentDashLength - decreaseLimit * currentDashLength) / ((numGaps-1) * 2);

                            for (int i = 1; i < numGaps; ++i) {
                                currentDashLength += dashDelta;
                                locations[index++] = currentDashLength + gapLength;
                                currentDashLength += dashDelta;
                            }
                        }
                        else if (decreaseKind == DecreaseKind.SmallToLarge) {
                            float currentDashLength = 2 * actualDashLength / (1 + decreaseLimit);
                            float dashDelta = (currentDashLength - decreaseLimit * currentDashLength) / ((numGaps - 1) * 2);
                            currentDashLength -= (currentDashLength - decreaseLimit * currentDashLength);

                            for (int i = 1; i < numGaps; ++i) {
                                currentDashLength += dashDelta;
                                locations[index++] = currentDashLength + gapLength;
                                currentDashLength += dashDelta;
                            }
                        }
                    }
                }
            }
            else if (kind == LocationKind.GapCentersOffset) {
                if (numGaps <= 1) {
                    locations = new float[0];
                    lengthAtEnd = pathLength;
                }
                else {
                    locations = new float[numGaps - 1];
                    lengthAtEnd = actualLastDashLength + gapLength / 2 + actualDashLength + gapLength - offset;
                    locations[index++] = actualFirstDashLength + gapLength / 2 + offset;
                    for (int i = 1; i < numGaps - 1; ++i)
                        locations[index++] = actualDashLength + gapLength;
                }
            }
            else {
                Debug.Assert(kind == LocationKind.DashAndGapLengths);

                List<float> locationList = new List<float>(numGaps * 2 + 1);

                if (numGaps > 0) {
                    // Add first dash (with secondary gaps as appropriate).
                    if (numEndSecGaps > 0 && (actualFirstDashLength >= firstDashLength / 2))
                        locationList.AddRange(AddSecondaryGaps(actualFirstDashLength, numEndSecGaps, lengthEndSecGaps));  // add with secondary gaps.
                    else
                        locationList.Add(actualFirstDashLength); // no secondary gaps

                    // Add middle dashes and gaps to them (with secondary gaps as appropriate). OCAD puts secondary gaps only in a
                    // dash that is longer than half of the length of a dash.
                    bool hasRoomForSecondaryGaps = (spacingMethod == SpacingMethod.OCAD) ? actualDashLength > dashLength / 2 : actualDashLength >= dashLength / 2;
                    for (int i = 1; i < numGaps; ++i) {
                        locationList.Add(gapLength);

                        if (numMiddleSecGaps > 0 && hasRoomForSecondaryGaps)
                            locationList.AddRange(AddSecondaryGaps(actualDashLength, numMiddleSecGaps, lengthMiddleSecGaps));  // add with secondary gaps
                        else
                            locationList.Add(actualDashLength);
                    }

                    // Add gap to last dash.
                    locationList.Add(gapLength);
                }

                // Add last dash (with secondary gaps as appropriate).
                if (numEndSecGaps > 0 && (actualLastDashLength >= lastDashLength / 2) && numGaps > 0)
                    locationList.AddRange(AddSecondaryGaps(actualLastDashLength, numEndSecGaps, lengthEndSecGaps));   // add with secondary gaps.
                else
                    locationList.Add(actualLastDashLength);

                // Convert to array.
                locations = locationList.ToArray();
                lengthAtEnd = locationList[locationList.Count - 1];
            }

            return locations;
        }

        // Adds secondary gaps to a dash of length dashLength, and returns an array of {dash-gap-dash} lengths. Could be array of length 1 if gaps aren't added.
        internal static float[] AddSecondaryGaps(float dashLength, int countGaps, float gapLength)
        {
            if (dashLength > countGaps * gapLength) {
                // we can create the secondary gaps.
                float[] newDistances = new float[countGaps * 2 + 1];
                float newDashLength = (dashLength - countGaps * gapLength) / (countGaps + 1);
                newDistances[0] = newDashLength;
                for (int k = 0; k < countGaps; ++k) {
                    newDistances[k * 2 + 1] = gapLength;
                    newDistances[k * 2 + 2] = newDashLength;
                }
                return newDistances;
            }
            else {
                // not room for the new gaps.
                return new float[1] { dashLength };
            }
        }

        // Given an array of dash lengths and gap lengths, remove zero-length gaps. Returns the same parameter if none to remove.
        static float[] RemoveZeroGaps(float[] distancesAndGaps)
        {
            bool hasZeroLengthGaps = false;

            for (int i = 1; i < distancesAndGaps.Length - 2; i += 2)
                if (distancesAndGaps[i] == 0)
                    hasZeroLengthGaps = true;

            if (!hasZeroLengthGaps)
                return distancesAndGaps;       // no gaps to remove.

            // Remove gaps and coellesce distances.
            List<float> list = new List<float>(distancesAndGaps);
            for (int i = distancesAndGaps.Length - 2; i >= 1; i -= 2) {
                if (list[i] == 0) {
                    list[i - 1] = list[i - 1] + list[i + 1];
                    list.RemoveAt(i + 1);
                    list.RemoveAt(i);
                }
            }

            return list.ToArray();
        }

        // Given an array of dash lengths and gap lengths, compute an array of spacing of the dash centers.
        // If "closed" is true, it is assume that first and last dashes are combined and uses the combined centers.
        static float[] ComputeDashCenters(float[] distancesAndGaps, bool closed)
        {
            // Special case -- one dash.
            if (distancesAndGaps.Length == 1) {
                return new float[1] { distancesAndGaps[0] / 2 };
            }

            List<float> dashCenters = new List<float>(distancesAndGaps.Length / 2);
            float len = 0;

            // First dash is special.
            float? firstPos;
            if (closed) {
                if (distancesAndGaps[0] >= distancesAndGaps[distancesAndGaps.Length - 1])
                    firstPos = distancesAndGaps[0] - (distancesAndGaps[0] + distancesAndGaps[distancesAndGaps.Length - 1])  / 2;
                else
                    firstPos = null;
            }
            else {
                firstPos = distancesAndGaps[0] / 2;
            }

            if (firstPos.HasValue)
                dashCenters.Add(firstPos.Value);
            len = distancesAndGaps[0] - (firstPos ?? 0);

            // Do the other dashes. "len" is the amount of the previous dash still to be used.
            for (int i = 1; i < (distancesAndGaps.Length + 1) / 2; ++i) {
                int gapIndex = i * 2 - 1;
                float dashLength = distancesAndGaps[gapIndex + 1];
                float gapLength = distancesAndGaps[gapIndex];
                if (dashLength > 0) {
                    if (closed && i == ((distancesAndGaps.Length - 1) / 2)) {
                        if (distancesAndGaps[0] < distancesAndGaps[distancesAndGaps.Length - 1])
                            dashCenters.Add(len + gapLength + (distancesAndGaps[0] + distancesAndGaps[distancesAndGaps.Length - 1])  / 2);
                    }
                    else {
                        dashCenters.Add(len + gapLength + dashLength / 2);
                        len = dashLength / 2;
                    }
                }
                else {
                    len += dashLength + gapLength;
                }
            }

            return dashCenters.ToArray();
        }

        // Does ComputeDashDistances fit the distances along a path in the measure of the quarters of its Bezier curves
        // (SymPath.GetOcadCurveMeasure), and give them as euclidean distances along the path?
        // path: the path.
        // spacingMethod: the method that fits the distances.
        private static bool IsMeasuredByQuarters(SymPath path, SpacingMethod spacingMethod)
        {
            return spacingMethod == SpacingMethod.OCAD && path.HasBeziers;
        }

        // The metric with which the drawing walks the distances that ComputeDashDistances gives along a path: the
        // euclidean metric where it gives them in the measure of the quarters, and otherwise the metric that it fits them
        // with.
        // path: the path.
        // spacingMethod: the method that fits the distances.
        // distanceMetric: the metric that fits the distances, the metric of the map.
        private static SymPath.DistanceMetric GetMetricAlongPath(SymPath path, SpacingMethod spacingMethod, SymPath.DistanceMetric distanceMetric)
        {
            return IsMeasuredByQuarters(path, spacingMethod) ? (SymPath.DistanceMetric)SymPath.EuclidDistance : distanceMetric;
        }

        // OCAD leaves out of a dash the vertex next to its start, and the vertex next to its end, when the piece of the
        // dash between the vertex and the end is shorter than this part of the width of the line that the dash draws.
        const float OCAD_VERTEX_PIECE_PER_WIDTH = 0.4F;

        // On a double line, OCAD first leaves the vertex out of the dash of the middle when the piece is shorter than
        // this, in mm, whatever the widths of the double line.
        const float OCAD_DOUBLE_LINE_VERTEX_PIECE = 0.05F;

        // The length in mm below which a piece of a dash leaves out the vertex next to the end of the dash, for a line of
        // a width that OCAD's spacing draws, or 0 for the other spacings, which keep all the vertices.
        // dashes: the dashes of the line; width: the width of the line in mm.
        private static float GetVertexPieceLimit(DashInfo dashes, float width)
        {
            return (dashes.spacingMethod == SpacingMethod.OCAD) ? OCAD_VERTEX_PIECE_PER_WIDTH * width : 0;
        }

        // The length in mm below which a piece of the dash of the middle of a double line leaves out the vertex next to
        // the end of the dash, before the dash is moved aside to a border, or 0 for the spacings other than OCAD's.
        // dashes: the dashes of the double line.
        private static float GetDoubleLineVertexPieceLimit(DashInfo dashes)
        {
            return (dashes.spacingMethod == SpacingMethod.OCAD) ? OCAD_DOUBLE_LINE_VERTEX_PIECE : 0;
        }

        // Draw a dashed line along a path. pathVertexLimit and drawnVertexLimit are the lengths in mm below which a piece
        // of a dash leaves out the vertex next to the end of the dash, on the path and on the line drawn.
        private static void DrawDashed(IGraphicsTarget g, SymPath path, object penKey, DashInfo dashes, float pathVertexLimit, float drawnVertexLimit, RenderOptions renderOpts, SymPath.DistanceMetric distanceMetric)
        {
            DrawDashedWithOffset(g, path, penKey, dashes, 0, 1, pathVertexLimit, drawnVertexLimit, renderOpts, distanceMetric);
        }

        // Draw a dashed line along a path, offset to the right. pathVertexLimit applies to each dash on the path, before
        // the offset, and drawnVertexLimit to the dash drawn, after it.
        private static void DrawDashedWithOffset(IGraphicsTarget g, SymPath path, object penKey, DashInfo dashes, float offsetRight, float miterLimit, float pathVertexLimit, float drawnVertexLimit, RenderOptions renderOpts, SymPath.DistanceMetric distanceMetric)
        {
            float[] distances;

            distances = ComputeDashDistances(path, LocationKind.DashAndGapLengths, dashes.spacingMethod, dashes.dashLength, dashes.firstDashLength, dashes.lastDashLength, dashes.gapLength, dashes.halfEndDashLengthWhenClosed, dashes.minGaps, 0, dashes.secondaryEndGaps, dashes.secondaryEndLength, dashes.secondaryMiddleGaps, dashes.secondaryMiddleLength, 1.0F, false, null, distanceMetric);

            if (distances.Length == 0 || (dashes.gapLength < renderOpts.minResolution && (dashes.secondaryMiddleGaps == 0 || dashes.secondaryMiddleLength < renderOpts.minResolution) && (dashes.secondaryEndGaps == 0 || dashes.secondaryEndLength < renderOpts.minResolution))) {
                // No dashes, or the dashes are too small to be visible. Draw solid.
                if (offsetRight != 0) {
                    SymPath offsetPath = path.OffsetRight(offsetRight, miterLimit);
                    offsetPath.Draw(g, penKey);
                }
                else
                    path.Draw(g, penKey);
            }
            else {
                path.DrawDashedOffset(g, penKey, distances, 0, offsetRight, miterLimit, GetMetricAlongPath(path, dashes.spacingMethod, distanceMetric), pathVertexLimit, drawnVertexLimit);
            }
        }

        // Helper for drawing or highlighting a glyph.
        private void DrawGlyph(IGraphicsTarget g, Glyph glyph, PointF location, float angle, Matrix extraTransform, SymColor color, RenderOptions renderOpts, bool highlight)
        {
            if (highlight) {
                glyph.DrawHighlight(g, location, angle, extraTransform, null, map.GetHighlightBrush(g));
            }
            else {
                glyph.Draw(g, location, angle, extraTransform, null, color, null, renderOpts);
            }
        }

        // Draw the glyphs along the path. "longPath" is the same as path unless shortening of the ends has occurred, in which case
        // path is the shortened path (used for all glyphs except start and end), and longPath is used for the start and end.
        private void DrawGlyphs(IGraphicsTarget g, GlyphInfo glyphInfo, SymPath path, SymPath longPath, SymColor color, RenderOptions renderOpts, bool highlight)
        {
            float[] distances;
            PointF[] points;
            float[] perpAngles, subtendedAngles;
            float firstDistance;
            bool ignoreDashCornersAtEnds = false;
            SpacingMethod spacingMethod = glyphInfo.spacingMethod;      // The method that fits the distances of the glyphs.

            Debug.Assert(highlight || color != null);

            // Figure out the distances of the glyphs along the line.
            switch (glyphInfo.location) {
            case GlyphLocation.CornersIgnoreEnds:
            case GlyphLocation.DashPointsIgnoreEnds:
                ignoreDashCornersAtEnds = true;
                goto case GlyphLocation.Corners;

            case GlyphLocation.Corners:
            case GlyphLocation.DashPoints:
                // Corner points/dash points are done somewhat differently. Only can have 1 symbol.
                // There is an interesting feature in OCAD where the dimensions of corner glyphs are stretched a certain amount at
                // very acute angles. This is so that power line crossbars always extend beyond the power lines themselves.
                // This is handled by stretching the glyph based on the subtended angle at the corner.
                points = path.FindCornerOrDashPoints((glyphInfo.location == GlyphLocation.DashPoints || glyphInfo.location == GlyphLocation.DashPointsIgnoreEnds) ? PointKind.Dash : PointKind.Corner,
                                                     ignoreDashCornersAtEnds, out perpAngles, out subtendedAngles);

                if (points != null) {
                    for (int i = 0; i < points.Length; ++i) {
                        float subtendedAngle = subtendedAngles[i];
                        float stretch;
                        if (subtendedAngle != 0 && !glyphInfo.noScaleAtCorners && (glyphInfo.location == GlyphLocation.Corners || glyphInfo.location == GlyphLocation.CornersIgnoreEnds))
                            stretch = Geometry.MiterFactor(subtendedAngle);   
                        else
                            stretch = 1.0F;
                        stretch = Math.Min(stretch, CORNER_GLYPH_STRETCH_LIMIT);

                        Matrix stretchMatrix = new Matrix();
                        stretchMatrix.Scale(1.0F, stretch);

                        DrawGlyph(g, glyphInfo.glyph, points[i], perpAngles[i] + 90.0F, stretchMatrix, color, renderOpts, highlight);
                    }
                }
                return;

            case GlyphLocation.Spaced:
                distances = ComputeDashDistances(path, LocationKind.GapCenters, glyphInfo.spacingMethod, glyphInfo.distance, glyphInfo.firstDistance, glyphInfo.lastDistance, 0, false, glyphInfo.minimum, 0, 0, 0, 0, 0, 1.0F, false, null, map.MapDistanceMetric);
                break;
            case GlyphLocation.SpacedOffset:
                distances = ComputeDashDistances(path, LocationKind.GapCentersOffset, glyphInfo.spacingMethod, glyphInfo.distance, glyphInfo.firstDistance, glyphInfo.lastDistance, 0, false, glyphInfo.minimum, glyphInfo.offset, 0, 0, 0, 0, 1.0F, false, null, map.MapDistanceMetric);
                break;
            case GlyphLocation.SpacedDecrease:
                DecreaseRanges decreaseRanges = new DecreaseRanges();
                distances = ComputeDashDistances(path, LocationKind.GapCentersDecrease, glyphInfo.spacingMethod, glyphInfo.distance, glyphInfo.firstDistance, glyphInfo.lastDistance, 0, false, glyphInfo.minimum, 0, 0, 0, 0, 0, glyphInfo.decreaseLimit, glyphInfo.decreaseBothEnds, decreaseRanges, map.MapDistanceMetric);

                if (distances != null && distances.Length > 0) {
                    firstDistance = distances[0];

                    for (int n = 0; n < glyphInfo.number; ++n) {
                        distances[0] = firstDistance - ((glyphInfo.number - 1 - n * 2) * (glyphInfo.spacing / 2.0F));

                        points = path.FindPointsAlongLine(distances, out perpAngles, GetMetricAlongPath(path, spacingMethod, map.MapDistanceMetric));

                        for (int i = 0; i < points.Length; ++i) {
                            float decreaseFactor = ComputeDecreaseFactor(i, decreaseRanges, glyphInfo.decreaseLimit);

                            if (decreaseFactor >= 0.001F) {
                                Matrix matrixTransform = new Matrix();
                                matrixTransform.Scale(decreaseFactor, decreaseFactor);
                                DrawGlyph(g, glyphInfo.glyph, points[i], perpAngles[i], matrixTransform, color, renderOpts, highlight);
                            }
                        }
                    }
                }
                
                return;
            case GlyphLocation.DashCenters:
                spacingMethod = dashInfo.spacingMethod;
                distances = ComputeDashDistances(path, LocationKind.DashCenters, dashInfo.spacingMethod, dashInfo.dashLength, dashInfo.firstDashLength, dashInfo.lastDashLength, dashInfo.gapLength, dashInfo.halfEndDashLengthWhenClosed, dashInfo.minGaps, 0, 0, 0, 0, 0, 1.0F, false, null, map.MapDistanceMetric);
                break;
            case GlyphLocation.GapCenters:
                // OCAD doesn't respect the "0 minimum gaps" for the symbols, although it does for the gaps. Always have at least one symbol. This is handled on import by having glyphInfo.minimum be 1.
                spacingMethod = dashInfo.spacingMethod;
                distances = ComputeDashDistances(path, LocationKind.GapCenters, dashInfo.spacingMethod, dashInfo.dashLength, dashInfo.firstDashLength, dashInfo.lastDashLength, dashInfo.gapLength, dashInfo.halfEndDashLengthWhenClosed, Math.Max(glyphInfo.minimum, dashInfo.minGaps), 0, 0, 0, 0, 0, 1.0F, false, null, map.MapDistanceMetric);
                break;
            case GlyphLocation.Start:
                distances = new float[1] { 0 };
                break;
            case GlyphLocation.End:
                distances = new float[1] { map.UseEuclideanMetric ? longPath.Length : longPath.BizzarroLength };
                break;
            default:
                Debug.Fail("bad glyph location");
                return;
            }

            if (distances == null || distances.Length == 0)
                return;
            firstDistance = distances[0];

            for (int n = 0; n < glyphInfo.number; ++n) {
                distances[0] = firstDistance - ((glyphInfo.number - 1 - n * 2) * (glyphInfo.spacing / 2.0F));

                if (glyphInfo.location != GlyphLocation.Start && glyphInfo.location != GlyphLocation.End) {
                    // With the OCAD metric, the measure of the quarters gives euclidean distances along the shortened
                    // path, and the shortening is in the OCAD metric, so the glyphs are found along the shortened path.
                    bool isEuclideanOnOcadMetric = !map.UseEuclideanMetric && IsMeasuredByQuarters(path, spacingMethod);
                    if (longPath.FirstPoint != path.FirstPoint && shortenInfo.shortenBeginning > 0 && !isEuclideanOnOcadMetric) {
                        // This is a shortened path, convert distances back into the long path.
                        distances[0] += shortenInfo.shortenBeginning;
                        points = longPath.FindPointsAlongLine(distances, out perpAngles, map.MapDistanceMetric);
                    }
                    else {
                        points = path.FindPointsAlongLine(distances, out perpAngles, GetMetricAlongPath(path, spacingMethod, map.MapDistanceMetric));
                    }
                }
                else {
                    points = longPath.FindPointsAlongLine(distances, out perpAngles, map.MapDistanceMetric);
                }


                for (int i = 0; i < points.Length; ++i) {
                    DrawGlyph(g, glyphInfo.glyph, points[i], perpAngles[i], null, color, renderOpts, highlight);
                }
            }
        }

        // Compute the decrease factor to apply to a glyph at the given index, given the ranges of decreasing symbols and the decrease limit.
        private float ComputeDecreaseFactor(int index, DecreaseRanges decreaseRanges, float decreaseLimit)
        {
            int i;
            if (index >= decreaseRanges.startBoth && index - decreaseRanges.startBoth < decreaseRanges.countBoth) {
                i = index - decreaseRanges.startBoth;
                if (decreaseRanges.countBoth <= 2)
                    return decreaseLimit;
                else
                    return 1.0F - (Math.Abs(i - ((decreaseRanges.countBoth - 1) / 2F)) * (1 - decreaseLimit) / ((decreaseRanges.countBoth - 1) / 2F));
            }
            else if (index >= decreaseRanges.startLargeToSmall && index - decreaseRanges.startLargeToSmall < decreaseRanges.countLargeToSmall) {
                i = index - decreaseRanges.startLargeToSmall;
                if (i == 0)
                    return 1.0F;
                else
                    return 1.0F - (i * (1 - decreaseLimit) / (decreaseRanges.countLargeToSmall - 1));
            }
            else if (index >= decreaseRanges.startSmallToLarge && index - decreaseRanges.startSmallToLarge < decreaseRanges.countSmallToLarge) {
                i = index - decreaseRanges.startSmallToLarge;
                i = decreaseRanges.countSmallToLarge - 1 - i;
                if (i == 0)
                    return 1.0F;
                else
                    return 1.0F - (i * (1 - decreaseLimit) / (decreaseRanges.countSmallToLarge - 1));
            }
            else {
                return 1.0F;
            }
        }

        // Draw the pointy ends on a line.
        void DrawPointyEnds(IGraphicsTarget g, SymPath longPath, float pointyLengthStart, float pointyLengthEnd, float lineWidth, object brush)
        {
            // Get locations of points at the tip, half-way, and base of the pointy tips.
            float length = map.UseEuclideanMetric ? longPath.Length : longPath.BizzarroLength;
            float[] distances, angles;
            if (length >= pointyLengthStart + pointyLengthEnd) {
                distances = new float[6] { 0, pointyLengthStart / 2, pointyLengthStart / 2, length - pointyLengthEnd - pointyLengthStart, pointyLengthEnd / 2, pointyLengthEnd / 2 };
            }
            else {
                float scaleFactor = length / (pointyLengthStart + pointyLengthEnd);
                distances = new float[6] { 0, (pointyLengthStart / 2) * scaleFactor, (pointyLengthStart / 2) * scaleFactor, 0, (pointyLengthEnd / 2) * scaleFactor, (pointyLengthEnd / 2) * scaleFactor };
            }
            PointF[] pointsAlongPath = longPath.FindPointsAlongLine(distances, out angles, map.MapDistanceMetric);

            // Each pointy tip is composed of a polygon of 5 points.
            PointF[] tipCorners = new PointF[5];
            float midpointWidth = lineWidth * 0.666F;  // Makes a sort of curvy tip.

            if (pointyLengthStart > 0) {
                // Draw point at beginning.
                tipCorners[0] = pointsAlongPath[0];
                tipCorners[1] = Geometry.MoveDistance(pointsAlongPath[1], midpointWidth / 2, angles[1] - 90.0F);
                tipCorners[4] = Geometry.MoveDistance(pointsAlongPath[1], midpointWidth / 2, angles[1] + 90.0F);
                tipCorners[2] = Geometry.MoveDistance(pointsAlongPath[2], lineWidth / 2, angles[2] - 90.0F);
                tipCorners[3] = Geometry.MoveDistance(pointsAlongPath[2], lineWidth / 2, angles[2] + 90.0F);
                g.FillPolygon(brush, tipCorners, AreaFillMode.Winding);
            }

            if (pointyLengthEnd > 0) {
                // Draw point at end.
                tipCorners[0] = pointsAlongPath[5];
                tipCorners[1] = Geometry.MoveDistance(pointsAlongPath[4], midpointWidth / 2, angles[4] - 90.0F);
                tipCorners[4] = Geometry.MoveDistance(pointsAlongPath[4], midpointWidth / 2, angles[4] + 90.0F);
                tipCorners[2] = Geometry.MoveDistance(pointsAlongPath[3], lineWidth / 2, angles[3] - 90.0F);
                tipCorners[3] = Geometry.MoveDistance(pointsAlongPath[3], lineWidth / 2, angles[3] + 90.0F);
                g.FillPolygon(brush, tipCorners, AreaFillMode.Winding);
            }
        }

        // Calculate the bounding box
        internal override RectangleF CalcBounds(SymPath path)
        {
            float lineThickness = maxThickness;

            // If the path has sharp mitered corners, the thickness is increased.
            if (maxMiteredThickness > 0) {
                float miterThickness = Math.Min(path.MaxMiter, GraphicsUtil.MITER_LIMIT) * maxMiteredThickness;
                if (miterThickness > lineThickness)
                    lineThickness = miterThickness;
            }

            RectangleF box = path.BoundingBox;
            box.Inflate(lineThickness / 2, lineThickness / 2);
            return box;
        }
    }

    public abstract class AreaLikeSymDef: SymDef
    {
        public AreaLikeSymDef(string name, string symbolId)
            :base(name, symbolId)
        { }

        internal abstract float BorderHighlightThickness { get; }

        internal abstract bool HasDashes { get; }

        internal abstract RectangleF CalcBounds(SymPathWithHoles path);

        internal abstract void Draw(IGraphicsTarget g, SymPathWithHoles path, SymColor color, float angle, PointF rotationCenter, RenderOptions renderOpts);

        internal abstract void DrawHighlight(IGraphicsTarget g, SymPathWithHoles path, HighlightOptions options);

        internal abstract RectangleF HighlightBounds(SymPathWithHoles path, RectangleF boundingBox, HighlightOptions options);

        // The coverage prototypes of an area symbol in a palette color: first the full coverage, when the definition
        // draws the palette color with its area fill, its area border or the glyph parts that the fill mode of an area
        // pattern selects, and then the coverage prototypes of the hatchings and the clipped area patterns in the
        // palette color, in the order of the drawing, of the component definitions in their order. A cross hatching is
        // two hatchings, so it gives two coverage prototypes.
        public override IReadOnlyList<CoveragePrototype> GetCoveragePrototypes(Symbol symbol, SymColor color)
        {
            if (color.IsSpecialLayer)
                return CoveragePrototype.None;

            List<CoveragePrototype> prototypes = new List<CoveragePrototype>();
            if (HasFullCoverage(color))
                prototypes.Add(CoveragePrototype.FullCoverage);
            AddCoveragePrototypesOfPatternFills(prototypes, color);
            return prototypes.AsReadOnly();
        }

        // Add the coverage prototypes of the hatchings and the clipped area patterns of this definition in a palette
        // color, in the order of the drawing: the hatchings first, as Draw draws them before the area patterns.
        // prototypes: the list to which the coverage prototypes are added.
        // color: the palette color.
        internal abstract void AddCoveragePrototypesOfPatternFills(List<CoveragePrototype> prototypes, SymColor color);

        // The geometry method of the area symbols: the clipping objects of the palette color coverage of an area symbol.
        // With the full coverage, they come from the path with holes and the angle and the rotation center of the area
        // symbol (MakeClippingObjects(SymPathWithHoles, SymColor, float, PointF)). With a hatching or a clipped area
        // pattern, the clipping object is the area: a filled shape of the main path and the holes with the alternate fill
        // rule, the area with which the drawing clips the hatching and the area pattern.
        internal override IReadOnlyList<ClippingObject> MakeClippingObjects(Symbol symbol, SymColor color, CoveragePrototype prototype)
        {
            AreaSymbol area = (AreaSymbol)symbol;
            if (prototype == CoveragePrototype.FullCoverage)
                return MakeClippingObjects(area.Path, color, area.Angle, area.RotationCenter);

            Debug.Assert(prototype is HatchingPrototype || prototype is AreaPatternPrototype, "The coverage prototype of an area is the full coverage, a hatching or an area pattern.");
            return Array.AsReadOnly(new ClippingObject[] { area.Path.MakeFilledShape(Placement.Identity) });
        }

        // The clipping objects of an area of this definition with a path with holes in a palette color, with the full
        // coverage, without a drawing target, in the order in which Draw draws them. The hatchings and the clipped area
        // patterns are periodic coverages, and they make no clipping objects here. The glyph parts that the fill mode of
        // an area pattern selects are clipping objects of the full coverage.
        // path: the path with holes: the main path and its holes.
        // color: the palette color.
        // angle: the angle of the area symbol, which turns its area patterns.
        // rotationCenter: the rotation center of the area symbol.
        internal abstract IReadOnlyList<ClippingObject> MakeClippingObjects(SymPathWithHoles path, SymColor color, float angle, PointF rotationCenter);

        // Add the clipping objects of a line along the main path and then along each hole of a path with holes, as an
        // area draws its area border and a combined area its line component definitions.
        // clippingObjects: the list to which the clipping objects are added.
        // lineSymDef: the line symbol definition.
        // path: the path with holes.
        // color: the palette color.
        private protected static void AddClippingObjectsOfLine(List<ClippingObject> clippingObjects, LineLikeSymDef lineSymDef, SymPathWithHoles path, SymColor color)
        {
            clippingObjects.AddRange(lineSymDef.MakeClippingObjects(path.MainPath, color));
            if (path.Holes != null) {
                foreach (SymPath hole in path.Holes)
                    clippingObjects.AddRange(lineSymDef.MakeClippingObjects(hole, color));
            }
        }
    }

    public class AreaComboSymDef: AreaLikeSymDef
    {
        SymDef[] components;

        public AreaComboSymDef(string name, string symbolId, IEnumerable<SymDef> components)
            :base(name, symbolId)
        {
            this.components = components.ToArray();
        }

        public IEnumerable<SymDef> Components
        {
            get
            {
                return components.ToList().AsReadOnly();
            }
        }

        internal override float BorderHighlightThickness
        {
            get
            {
                float max = 0;
                foreach (SymDef component in components) {
                    LineLikeSymDef lineComponent = component as LineLikeSymDef;
                    max = Math.Max(max, lineComponent.HighlightThickness);
                }

                return max;
            }
        }

        internal override bool HasDashes
        {
            get
            {
                foreach (SymDef component in components) {
                    LineLikeSymDef lineComponent = component as LineLikeSymDef;
                    if (lineComponent != null && lineComponent.HasDashes)
                        return true;
                }

                return false;
            }
        }

        public override SymDef CopyToMap(Map map)
        {
            throw new NotImplementedException();
        }

        public override void FreeGdiObjects()
        {
        }

        public override bool HasColor(SymColor color)
        {
            foreach (SymDef component in components) {
                if (component.HasColor(color))
                    return true;
            }

            return false;
        }

        // Does a component draw the given palette color with the full coverage?
        internal override bool HasFullCoverage(SymColor color)
        {
            foreach (SymDef component in components) {
                if (component.HasFullCoverage(color))
                    return true;
            }

            return false;
        }

        internal override RectangleF CalcBounds(SymPathWithHoles path)
        {
            RectangleF bounds = new RectangleF();
            bool first = true;
            foreach (SymDef component in components) {
                RectangleF componentBounds;

                if (component is LineLikeSymDef)
                    componentBounds = ((LineLikeSymDef)component).CalcBounds(path.MainPath);
                else
                    componentBounds = ((AreaLikeSymDef)component).CalcBounds(path);

                if (first)
                    bounds = componentBounds;
                else
                    bounds = RectangleF.Union(bounds, componentBounds);

                first = false;
            }

            return bounds;
        }

        internal override void Draw(IGraphicsTarget g, SymPathWithHoles path, SymColor color, float angle, PointF rotationCenter, RenderOptions renderOpts)
        {
            foreach (SymDef component in components) {
                if (component is LineLikeSymDef) {
                    LineLikeSymDef lineSymDef = (LineLikeSymDef)component;
                    // Draw main part of border.
                    lineSymDef.Draw(g, path.MainPath, color, renderOpts);

                    // Draw the holes.
                    if (path.Holes != null)
                        foreach (SymPath hole in path.Holes)
                            lineSymDef.Draw(g, hole, color, renderOpts);
                }
                else {
                    ((AreaLikeSymDef)component).Draw(g, path, color, angle, rotationCenter, renderOpts);
                }
            }
        }

        // The clipping objects of a combined area: the clipping objects of each component definition, in the order of the
        // components, as Draw draws them. A line symbol definition goes along the main path and along each hole, and an
        // area symbol definition gives the clipping objects of its own geometry method for the same path with holes, the
        // same angle and the same rotation center.
        internal override IReadOnlyList<ClippingObject> MakeClippingObjects(SymPathWithHoles path, SymColor color, float angle, PointF rotationCenter)
        {
            List<ClippingObject> clippingObjects = new List<ClippingObject>();
            foreach (SymDef component in components) {
                if (component is LineLikeSymDef lineSymDef)
                    AddClippingObjectsOfLine(clippingObjects, lineSymDef, path, color);
                else
                    clippingObjects.AddRange(((AreaLikeSymDef)component).MakeClippingObjects(path, color, angle, rotationCenter));
            }
            return clippingObjects.AsReadOnly();
        }

        // The coverage prototypes of the hatchings and the clipped area patterns of the area symbol definitions among the
        // components, in the order of the components.
        internal override void AddCoveragePrototypesOfPatternFills(List<CoveragePrototype> prototypes, SymColor color)
        {
            foreach (SymDef component in components) {
                if (component is AreaLikeSymDef areaSymDef)
                    areaSymDef.AddCoveragePrototypesOfPatternFills(prototypes, color);
            }
        }

        internal override void DrawHighlight(IGraphicsTarget g, SymPathWithHoles path, HighlightOptions options)
        {
            if (options.style == HighlightStyle.HighFidelity) {
                foreach (SymDef component in components) {
                    bool areaDrawn = false;

                    if (component is LineLikeSymDef) {
                        LineLikeSymDef lineSymDef = (LineLikeSymDef)component;
                        // Draw main part of border.
                        lineSymDef.DrawHighlight(g, path.MainPath, options);

                        // Draw the holes.
                        if (path.Holes != null)
                            foreach (SymPath hole in path.Holes)
                                lineSymDef.DrawHighlight(g, hole, options);
                    }
                    else if (!areaDrawn) {
                        ((AreaLikeSymDef)component).DrawHighlight(g, path, options);
                        areaDrawn = true;  // Only draw area once when highlighting.
                    }
                }
            }
            else {
                SymbolHelpers.DrawAreaHighlight(g, map, path, options);
            }
        }

        internal override RectangleF HighlightBounds(SymPathWithHoles path, RectangleF boundingBox, HighlightOptions options)
        {
            if (options.style == HighlightStyle.HighFidelity && this.BorderHighlightThickness > SymbolHelpers.MinimumLineWidth(options)) {
                return boundingBox;
            }
            else {
                return SymbolHelpers.AreaHighlightBounds(path, options);
            }
        }
    }

    public class AreaSymDef: AreaLikeSymDef
    {
        public enum RotateMode {
            Always,             // Always rotate the pattern when rotating map/object.
            ManualOnly,         // Only allow manual setting of pattern direction, but don't rotate pattern when rotating object/map
            Never               // Never allow pattern direction to influence rendering of pattern (even if already set).
        };

        public enum PatternFillMode
        {
            Clip = 0,               // Clip at boundary of area (only option until OCAD 12)
            CompletelyInside = 1,   // Draw structure elements completely inside area
            CenterInside = 2,       // Draw structure elements with center inside area
            PartiallyInside = 3,    // Draw structure elements with any part inside area
        }

        public struct HatchInfo
        {
            public SymColor hatchColor;  // color of hatch lines
            public float hatchWidth;    // width of hatch lines
            public float hatchSpacing;  // spacing of hatch lines
            public float hatchOffset;   // offset of hatch lines from zero (for OpenMapper compatibility)
            public float hatchAngle;    // angle of hatching
            public RotateMode hatchRotateMode; // rotation mode.
        }

        public struct PatternInfo
        {
            public bool offsetRows;     // offset alternate rows by 1/2
            public float patternWidth;
            public float patternHeight; // size of pattern element
            public float patternAngle;  // angle of pattern
            public float patternOffsetX, patternOffsetY;  // offset patterns by this.
            public Glyph patternGlyph;  // pattern element
            public RotateMode patternRotateMode; // rotation mode.
            public PatternFillMode patternFillMode;  // fill mode for pattern
            public bool irregular;         // true: irregular pattern
            public float irregularVarX;    // 0-1: fraction of irregularity in X
            public float irregularVarY;    // 0-1: fraction of irregularity in Y
            public float irregularMinDist; // min distance between objects.
        }

        // Solid fill information
        SymColor fillColor;  // color to fill with, null to not fill.

        // Border information
        LineSymDef borderSymdef;          // if non-null, the border symbol to use.

        // Hatching information.
        List<HatchInfo> hatchings = new List<HatchInfo>();
        // Keys for the pens used for hatching, one per hatching. Only written by GetKeyArray (see GetHatchPens),
        // which keeps it safe for several threads drawing at once.
        object[] hatchPens;

        // Pattern information
        List<PatternInfo> patterns = new List<PatternInfo>();

        // Cached brushes for faster pattern drawing.
        // Keys for pattern brushes, indexed by pattern index and color. Concurrent because several threads can draw at once.
        readonly ConcurrentDictionary<Pair<int, SymColor>, object> patternBrushes = new ConcurrentDictionary<Pair<int, SymColor>, object>();

        // The coverage prototypes of the hatchings and the clipped area patterns, after the first read made them
        // (GetPatternFillPrototypes).
        PatternFillPrototypes patternFillPrototypes;

        public override void SetMap(Map newMap)
        {
            base.SetMap(newMap);

            if (newMap != null) {
                CheckColor(fillColor);
                foreach (HatchInfo hatchInfo in hatchings)
                    CheckColor(hatchInfo.hatchColor);
                foreach (PatternInfo patternInfo in patterns)
                    patternInfo.patternGlyph.CheckColors(map);
            }
        }

        public AreaSymDef(string name, string symbolId, SymColor color, LineSymDef borderSymdef)
            : base(name, symbolId)
        {
            fillColor = color;
            this.borderSymdef = borderSymdef;
        }

        public SymColor FillColor { get { return fillColor; } }

        public LineSymDef BorderSymdef { get { return borderSymdef; } }

        public override SymDef DependsOnSymdef
        {
            get
            {
                return borderSymdef;
            }
        }

        internal override float BorderHighlightThickness
        {
            get
            {
                if (borderSymdef != null)
                    return borderSymdef.HighlightThickness;
                else
                    return 0;
            }
        }

        public void AddHatching(HatchInfo hatchInfo)
        {
            CheckModifiable();
            hatchings.Add(hatchInfo);
        }

        public bool HasHatching { get { return hatchings.Count > 0; }}

        public ICollection<HatchInfo> GetHatchings()
        {
            return hatchings.AsReadOnly();
        }

        public void AddPattern(PatternInfo patternInfo)
        {
            CheckModifiable();
            patterns.Add(patternInfo);
        }

        public bool HasPattern { get { return patterns.Count > 0; }}

        internal override bool HasDashes
        {
            get
            {
                if (borderSymdef != null && borderSymdef.HasDashes)
                    return true;
                else
                    return false;
            }
        }

        public ICollection<PatternInfo> GetPatterns()
        {
            return patterns.AsReadOnly();
        }

        // The hatching at an index.
        // index: the index of the hatching.
        internal HatchInfo GetHatching(int index)
        {
            return hatchings[index];
        }

        // The area pattern at an index.
        // index: the index of the area pattern.
        internal PatternInfo GetPattern(int index)
        {
            return patterns[index];
        }

        public override SymDef CopyToMap(Map map)
        {
            var newBorderSymDef = borderSymdef == null ? null : (LineSymDef) map.SymdefFromSymbolId(borderSymdef.SymbolId);
            AreaSymDef newSymDef = new AreaSymDef(Name, SymbolId, map.SymColorFromSymColor(fillColor), newBorderSymDef);

            foreach (HatchInfo hatchInfo in hatchings) {
                HatchInfo newHatchInfo = hatchInfo;
                newHatchInfo.hatchColor = map.SymColorFromSymColor(hatchInfo.hatchColor);
                newSymDef.AddHatching(newHatchInfo);
            }

            foreach (PatternInfo patternInfo in patterns) {
                PatternInfo newPatternInfo = patternInfo;
                newPatternInfo.patternGlyph = patternInfo.patternGlyph.CopyToMap(map);
            }

            map.AddSymdef(newSymDef);
            return newSymDef;
        }

        void CreatePensAndBrushes(IGraphicsTarget g)
        {
            if (HasHatching) {
                object[] pens = GetHatchPens();
                for (int i = 0; i < hatchings.Count; ++i) {
                    if (!g.HasPen(pens[i])) {
                        g.CreatePen(pens[i], hatchings[i].hatchColor.GetBrushKey(g), hatchings[i].hatchWidth, LineCapMode.Flat, LineJoinMode.Miter, GraphicsUtil.MITER_LIMIT);
                    }
                }
            }
        }

        // Get the keys for the pens used for hatching, one per hatching, creating them if needed. Thread-safe:
        // the returned array always has hatchings.Count entries and is never modified afterward.
        private object[] GetHatchPens()
        {
            return GetKeyArray(ref hatchPens, hatchings.Count);
        }

        public override void FreeGdiObjects()
        {
        }



        // Does this symbol definition draw the given color. Used to determine
        // if this symbol definition draws into the current layer being draw.
        public override bool HasColor(SymColor color)
        {
            Debug.Assert(color != null);
            if (color.IsSpecialLayer)
                return false;

            if (color == fillColor)
                return true;

            if (borderSymdef != null && borderSymdef.HasColor(color))
                return true;
            
            foreach (HatchInfo hatchInfo in hatchings) {
                if (color == hatchInfo.hatchColor)
                    return true;
            }

            foreach (PatternInfo patternInfo in patterns) {
                if (patternInfo.patternGlyph.HasColor(color))
                    return true;
            }

            return false;
        }

        // Does this area draw the given palette color with the full coverage: does it fill the area with the color, does
        // its border draw it, or does it draw it in the glyph parts that the fill mode of an area pattern selects? The
        // hatchings and the clipped area patterns of the area are periodic coverages of their own.
        internal override bool HasFullCoverage(SymColor color)
        {
            Debug.Assert(color != null);
            if (color.IsSpecialLayer)
                return false;

            if (color == fillColor || (borderSymdef != null && borderSymdef.HasColor(color)))
                return true;

            foreach (PatternInfo patternInfo in patterns) {
                if (patternInfo.patternFillMode != PatternFillMode.Clip && patternInfo.patternGlyph.HasColor(color))
                    return true;
            }

            return false;
        }

        // The coverage prototypes of the hatchings in a palette color, and then of the clipped area patterns in it, in
        // their order. The area patterns whose fill mode selects glyph parts have no coverage prototypes: the selected
        // glyph parts are clipping objects of the full coverage.
        internal override void AddCoveragePrototypesOfPatternFills(List<CoveragePrototype> prototypes, SymColor color)
        {
            PatternFillPrototypes made = GetPatternFillPrototypes();
            for (int i = 0; i < hatchings.Count; ++i) {
                if (hatchings[i].hatchColor == color)
                    prototypes.Add(made.Hatchings[i]);
            }

            for (int i = 0; i < patterns.Count; ++i) {
                foreach (AreaPatternPrototype prototype in made.AreaPatterns[i]) {
                    if (prototype.Color == color)
                        prototypes.Add(prototype);
                }
            }
        }

        // The coverage prototypes of the hatchings and the clipped area patterns: a derived value of the definition,
        // which depends on the definition alone. The first read makes them, and each area symbol of the definition refers
        // to the same objects. Several threads can read at once, and all get the coverage prototypes that one of them
        // publishes. A definition does not change after the map gets it.
        private PatternFillPrototypes GetPatternFillPrototypes()
        {
            PatternFillPrototypes made = Volatile.Read(ref patternFillPrototypes);
            if (made == null) {
                made = new PatternFillPrototypes(this);
                made = Interlocked.CompareExchange(ref patternFillPrototypes, made, null) ?? made;
            }

            return made;
        }

        // The coverage prototypes of the hatchings and the clipped area patterns of an area symbol definition.
        private sealed class PatternFillPrototypes
        {
            // The coverage prototype of each hatching, by the index of the hatching.
            public readonly HatchingPrototype[] Hatchings;

            // The coverage prototypes of each area pattern, by the index of the area pattern: one in each palette color
            // of the glyph parts of a clipped area pattern, in the order of the glyph parts, and none for an area pattern
            // whose fill mode selects glyph parts.
            public readonly AreaPatternPrototype[][] AreaPatterns;

            // Make the coverage prototypes of a definition.
            // definition: the area symbol definition.
            public PatternFillPrototypes(AreaSymDef definition)
            {
                Hatchings = new HatchingPrototype[definition.hatchings.Count];
                for (int i = 0; i < Hatchings.Length; ++i)
                    Hatchings[i] = new HatchingPrototype(definition, i);

                AreaPatterns = new AreaPatternPrototype[definition.patterns.Count][];
                for (int i = 0; i < AreaPatterns.Length; ++i) {
                    PatternInfo patternInfo = definition.patterns[i];
                    if (patternInfo.patternFillMode == PatternFillMode.Clip)
                        AreaPatterns[i] = patternInfo.patternGlyph.Colors.Select(color => new AreaPatternPrototype(definition, i, color)).ToArray();
                    else
                        AreaPatterns[i] = Array.Empty<AreaPatternPrototype>();
                }
            }
        }

        // Highlight the symbol.
        internal override void DrawHighlight(IGraphicsTarget g, SymPathWithHoles path, HighlightOptions options)
        {
            if (options.style == HighlightStyle.HighFidelity && BorderSymdef != null &&
                BorderSymdef.HighlightThickness > SymbolHelpers.MinimumLineWidth(options)) {
                // Just fill the interior, and highlight the border.
                SymbolHelpers.FillAreaHighlight(g, map, path, options);

                BorderSymdef.DrawHighlight(g, path.MainPath, options);
                if (path.Holes != null) {
                    foreach (SymPath holePath in path.Holes)
                        BorderSymdef.DrawHighlight(g, holePath, options);
                }
            }
            else {
                SymbolHelpers.DrawAreaHighlight(g, map, path, options);
            }
        }

        internal override RectangleF HighlightBounds(SymPathWithHoles path, RectangleF boundingBox, HighlightOptions options)
        {
            if (options.style == HighlightStyle.HighFidelity && BorderSymdef != null &&
                BorderSymdef.HighlightThickness > SymbolHelpers.MinimumLineWidth(options)) {
                return boundingBox;
            }
            else {
                return SymbolHelpers.AreaHighlightBounds(path, options);
            }
        }

        // Draw this area symbol in the graphics inside/around the path provided, with
        // the given color only.
        internal override void Draw(IGraphicsTarget g, SymPathWithHoles path, SymColor color, float angle, PointF rotationCenter, RenderOptions renderOpts)
        {
            Debug.Assert(color != null);

            CreatePensAndBrushes(g);

            if (color == fillColor) {
                path.Fill(g, color.GetBrushKey(g));
            }

            if (HasHatching) {
                DrawHatchings(g, path, angle, rotationCenter, renderOpts, color);
            }

            if (HasPattern) {
                DrawPatterns(g, path, color, angle, rotationCenter, renderOpts);
            }

            // Draw the border. The Draw routine on LineSymDef automatically takes into account the subpaths defined by start/stop flags along the paths.
            if (borderSymdef != null && borderSymdef.HasColor(color)) {
                // Draw main part of border.
                borderSymdef.Draw(g, path.MainPath, color, renderOpts);

                // Draw the holes.
                if (path.Holes != null)
                    foreach (SymPath hole in path.Holes)
                        borderSymdef.Draw(g, hole, color, renderOpts);
            }
        }

        // The clipping objects of an area with the full coverage, in the order in which Draw draws them: the area fill in
        // its color, a filled shape of the main path and its holes with the alternate fill rule, as SymPathWithHoles.Fill
        // fills them; the glyph parts that the fill mode of each area pattern selects (AddSelectedGlyphParts); and the
        // area border along the main path and then along each hole. The hatchings and the clipped area patterns make no
        // clipping objects here.
        internal override IReadOnlyList<ClippingObject> MakeClippingObjects(SymPathWithHoles path, SymColor color, float angle, PointF rotationCenter)
        {
            List<ClippingObject> clippingObjects = new List<ClippingObject>();
            if (color == fillColor)
                clippingObjects.Add(path.MakeFilledShape(Placement.Identity));

            foreach (PatternInfo patternInfo in patterns) {
                if (patternInfo.patternFillMode != PatternFillMode.Clip && patternInfo.patternGlyph.HasColor(color))
                    AddSelectedGlyphParts(clippingObjects, patternInfo, path, color, angle, rotationCenter);
            }

            if (borderSymdef != null && borderSymdef.HasColor(color))
                AddClippingObjectsOfLine(clippingObjects, borderSymdef, path, color);

            return clippingObjects.AsReadOnly();
        }

        // Add the glyph parts in a palette color that the fill mode of an area pattern selects, as DrawPattern and
        // DrawPatternRows draw them: each glyph part of each element is selected by its own points and center
        // (Glyph.GlyphPart.IsSelectedBy). A selected glyph part is a shared shape of the glyph at a single placement whose
        // steps are the transformation of the area pattern, as DrawPattern sets it, and the pose of the element: its
        // location at its lattice point, with the irregularity, and the angle -patternAngle.
        // clippingObjects: the list to which the clipping objects are added.
        // patternInfo: the area pattern, whose fill mode selects glyph parts.
        // path: the path with holes of the area.
        // color: the palette color.
        // angle: the angle of the area symbol.
        // rotationCenter: the rotation center of the area symbol.
        void AddSelectedGlyphParts(List<ClippingObject> clippingObjects, PatternInfo patternInfo, SymPathWithHoles path, SymColor color, float angle, PointF rotationCenter)
        {
            Matrix matrix = MakePatternMatrix(patternInfo, angle, rotationCenter);
            SinglePlacement placement = Placement.Identity.Within(matrix);
            matrix.Invert();
            RectangleF bounding = Geometry.BoundsOfTransformedRectangle(path.BoundingBox, matrix);
            GlyphPartFilter filter = MakeGlyphPartFilter(patternInfo.patternFillMode, path.Transform(matrix));

            foreach (PointF location in GetPatternElementLocations(patternInfo, bounding))
                clippingObjects.AddRange(patternInfo.patternGlyph.MakeClippingObjectsOfSelectedParts(location, -patternInfo.patternAngle, color, placement, filter));
        }

        // The filter of the glyph parts that a fill mode selects, as DrawPatternRows makes it: CompletelyInside selects a
        // part whose each point is inside the area, CenterInside a part whose center is inside it, and PartiallyInside a
        // part one of whose points is inside it.
        // fillMode: the fill mode, which is not Clip.
        // transformedPath: the path with holes of the area in the coordinates of the area pattern.
        static GlyphPartFilter MakeGlyphPartFilter(PatternFillMode fillMode, SymPathWithHoles transformedPath)
        {
            switch (fillMode) {
                case PatternFillMode.CompletelyInside:
                    return (PointF[] points, PointF center) => points.All(pt => transformedPath.IsInside(pt));
                case PatternFillMode.CenterInside:
                    return (PointF[] points, PointF center) => transformedPath.IsInside(center);
                case PatternFillMode.PartiallyInside:
                    return (PointF[] points, PointF center) => points.Any(pt => transformedPath.IsInside(pt));
                default:
                    throw new ArgumentException("The fill mode Clip selects no glyph parts.", nameof(fillMode));
            }
        }

        // The transformation of an area pattern, as DrawPattern sets it: T(c) · R(a) · R(patternAngle) ·
        // T(patternOffsetX, patternOffsetY). The rotate mode Never leaves out the angle a and the rotation center c of the
        // area symbol, as DrawPatterns does.
        // patternInfo: the area pattern.
        // angle: the angle of the area symbol.
        // rotationCenter: the rotation center of the area symbol.
        static Matrix MakePatternMatrix(PatternInfo patternInfo, float angle, PointF rotationCenter)
        {
            bool ignoreRotation = (patternInfo.patternRotateMode == RotateMode.Never);
            if (ignoreRotation) {
                angle = 0;
                rotationCenter = new PointF();
            }

            Matrix matrix = new Matrix();
            matrix.Translate(rotationCenter.X, rotationCenter.Y);
            matrix.Rotate(angle);
            matrix.Rotate(patternInfo.patternAngle);
            matrix.Translate(patternInfo.patternOffsetX, patternInfo.patternOffsetY);
            return matrix;
        }

        // The transformation of a hatching, as DrawOneHatching sets it: T(c) · R(a) · R(hatchAngle). The rotate mode Never
        // leaves out the angle a and the rotation center c of the area symbol. The drawing adds the offset of the hatching
        // to the places of the hatch lines (SymbolHelpers.DrawHatchLines).
        // hatchInfo: the hatching.
        // angle: the angle of the area symbol.
        // rotationCenter: the rotation center of the area symbol.
        static Matrix MakeHatchingMatrix(HatchInfo hatchInfo, float angle, PointF rotationCenter)
        {
            if (hatchInfo.hatchRotateMode == RotateMode.Never) {
                angle = 0;
                rotationCenter = new PointF();
            }

            Matrix matrix = new Matrix();
            matrix.Translate(rotationCenter.X, rotationCenter.Y);
            matrix.Rotate(angle);
            matrix.Rotate(hatchInfo.hatchAngle);
            return matrix;
        }

        // The affine transformation M of the configuration of a hatching: T(c) · R(a) · R(hatchAngle) ·
        // T(0, hatchOffset), the transformation of the drawing with the offset of the hatching.
        // index: the index of the hatching.
        // angle: the angle of the area symbol.
        // rotationCenter: the rotation center of the area symbol.
        internal Matrix GetHatchingTransformation(int index, float angle, PointF rotationCenter)
        {
            HatchInfo hatchInfo = hatchings[index];
            Matrix matrix = MakeHatchingMatrix(hatchInfo, angle, rotationCenter);
            matrix.Translate(0, hatchInfo.hatchOffset);
            return matrix;
        }

        // The affine transformation M of the configuration of an area pattern: T(c) · R(a) · R(patternAngle) ·
        // T(patternOffsetX, patternOffsetY), the transformation of the drawing.
        // index: the index of the area pattern.
        // angle: the angle of the area symbol.
        // rotationCenter: the rotation center of the area symbol.
        internal Matrix GetPatternTransformation(int index, float angle, PointF rotationCenter)
        {
            return MakePatternMatrix(patterns[index], angle, rotationCenter);
        }

        // The expansion of a hatching over the bounds of an area: the hatch lines that DrawOneHatching draws, each a
        // stroked line of the pen of the hatching, at a single placement whose step is the transformation of the drawing,
        // in the order of the drawing.
        // index: the index of the hatching.
        // path: the path with holes of the area.
        // angle: the angle of the area symbol.
        // rotationCenter: the rotation center of the area symbol.
        internal IReadOnlyList<ClippingObject> ExpandHatching(int index, SymPathWithHoles path, float angle, PointF rotationCenter)
        {
            HatchInfo hatchInfo = hatchings[index];
            Matrix matrix = MakeHatchingMatrix(hatchInfo, angle, rotationCenter);
            SinglePlacement placement = Placement.Identity.Within(matrix);
            matrix.Invert();
            RectangleF bounding = Geometry.BoundsOfTransformedRectangle(path.BoundingBox, matrix);

            Stroke stroke = new Stroke(hatchInfo.hatchWidth, LineCapMode.Flat, LineJoinMode.Miter, GraphicsUtil.MITER_LIMIT);
            return SymbolHelpers.MakeHatchLines(stroke, hatchInfo.hatchSpacing, hatchInfo.hatchOffset, bounding, placement);
        }

        // The expansion of a clipped area pattern in a palette color over the bounds of an area: the elements that
        // DrawPattern draws, at their lattice points with the irregularity, in the order of DrawPatternRows. The glyph
        // parts of each element in the palette color are the shared shapes of the glyph at a single placement whose steps
        // are the transformation of the area pattern and the pose of the element, and a glyph of one circle at its origin
        // is a circle at the location of the element within the transformation of the area pattern, as Glyph.DrawSimple
        // draws it (Glyph.MakeClippingObjects).
        // index: the index of the area pattern.
        // color: the palette color.
        // path: the path with holes of the area.
        // angle: the angle of the area symbol.
        // rotationCenter: the rotation center of the area symbol.
        internal IReadOnlyList<ClippingObject> ExpandAreaPattern(int index, SymColor color, SymPathWithHoles path, float angle, PointF rotationCenter)
        {
            PatternInfo patternInfo = patterns[index];
            Matrix matrix = MakePatternMatrix(patternInfo, angle, rotationCenter);
            SinglePlacement placement = Placement.Identity.Within(matrix);
            matrix.Invert();
            RectangleF bounding = Geometry.BoundsOfTransformedRectangle(path.BoundingBox, matrix);

            List<ClippingObject> expansion = new List<ClippingObject>();
            foreach (PointF location in GetPatternElementLocations(patternInfo, bounding))
                expansion.AddRange(patternInfo.patternGlyph.MakeClippingObjects(location, -patternInfo.patternAngle, null, color, null, placement));
            return expansion.AsReadOnly();
        }

        // The locations of the elements of an area pattern over a rectangle of the coordinates of the area pattern, in the
        // order in which DrawPatternRows draws them: row by row, with the steps summed as the drawing sums them, and each
        // moved by the irregularity (HandleIrregularity).
        // patternInfo: the area pattern.
        // boundingRect: the rectangle.
        IEnumerable<PointF> GetPatternElementLocations(PatternInfo patternInfo, RectangleF boundingRect)
        {
            RectangleF patternGlyphBounds = Geometry.BoundsOfRotatedRectangle(patternInfo.patternGlyph.BoundingBox, new PointF(0, 0), -patternInfo.patternAngle);
            RectangleF patternBounds = new RectangleF(-patternInfo.patternWidth / 2, -patternInfo.patternHeight / 2, patternInfo.patternWidth, patternInfo.patternHeight);

            long topLineCount = (long)Math.Round((boundingRect.Top - patternGlyphBounds.Bottom + patternBounds.Bottom) / patternInfo.patternHeight);
            double topLine = topLineCount * patternInfo.patternHeight;
            double bottomLine = (Math.Round((boundingRect.Bottom - patternGlyphBounds.Top + patternBounds.Top) / patternInfo.patternHeight) + 0.5) * patternInfo.patternHeight;
            double leftLine = Math.Round((boundingRect.Left - patternGlyphBounds.Right + patternBounds.Right) / patternInfo.patternWidth) * patternInfo.patternWidth;
            double rightLine = (Math.Round((boundingRect.Right - patternGlyphBounds.Left + patternBounds.Left) / patternInfo.patternWidth) + 0.5) * patternInfo.patternWidth;
            double offsetLeftLine = leftLine - (patternInfo.patternWidth / 2);
            double offsetRightLine = rightLine + (patternInfo.patternWidth / 2);
            bool firstLineOffset = (topLineCount & 1) != 0;

            bool offsetThisLine = patternInfo.offsetRows && firstLineOffset;

            for (double y = topLine; y <= bottomLine; y += patternInfo.patternHeight) {
                if (offsetThisLine) {
                    for (double x = offsetLeftLine; x <= offsetRightLine; x += patternInfo.patternWidth)
                        yield return HandleIrregularity(patternInfo, x, y);
                }
                else {
                    for (double x = leftLine; x <= rightLine; x += patternInfo.patternWidth)
                        yield return HandleIrregularity(patternInfo, x, y);
                }

                if (patternInfo.offsetRows)
                    offsetThisLine = !offsetThisLine;
            }
        }

        // Does the coverage prototype of an area pattern in a palette color cover a point of its coordinates: does a glyph
        // part of an element in the palette color cover it? The elements are turned by -patternAngle at the points of the
        // lattice, x = i · patternWidth and y = j · patternHeight, the points of each odd row patternWidth / 2 aside when
        // the area pattern offsets its rows, and each is moved by the irregularity (HandleIrregularity).
        // index: the index of the area pattern.
        // color: the palette color.
        // point: the point, in the coordinates of the area pattern.
        internal bool AreaPatternCovers(int index, SymColor color, PointF point)
        {
            PatternInfo patternInfo = patterns[index];
            double width = patternInfo.patternWidth, height = patternInfo.patternHeight;
            if (!(width > 0) || !(height > 0))
                return false;

            // The elements whose glyph parts can reach the point: within the radius of the glyph, and the most that the
            // irregularity moves an element, half the size of the element.
            double reachX = patternInfo.patternGlyph.Radius + (patternInfo.irregular ? width / 2 : 0);
            double reachY = patternInfo.patternGlyph.Radius + (patternInfo.irregular ? height / 2 : 0);
            long firstRow = (long)Math.Floor((point.Y - reachY) / height), lastRow = (long)Math.Ceiling((point.Y + reachY) / height);
            for (long row = firstRow; row <= lastRow; ++row) {
                double rowOffset = (patternInfo.offsetRows && (row & 1) != 0) ? width / 2 : 0;
                long firstColumn = (long)Math.Floor((point.X - rowOffset - reachX) / width), lastColumn = (long)Math.Ceiling((point.X - rowOffset + reachX) / width);
                for (long column = firstColumn; column <= lastColumn; ++column) {
                    PointF location = HandleIrregularity(patternInfo, column * width + rowOffset, row * height);
                    Matrix inverse = new Matrix();
                    inverse.Rotate(patternInfo.patternAngle);
                    inverse.Translate(-location.X, -location.Y);
                    if (patternInfo.patternGlyph.Covers(Geometry.TransformPoint(point, inverse), color))
                        return true;
                }
            }

            return false;
        }

        // Draw the hatching into the interior of the SymPath.
        void DrawHatchings(IGraphicsTarget g, SymPathWithHoles path, float angle, PointF rotationCenter, RenderOptions renderOpts, SymColor color)
        {
            if (renderOpts.omitHatchingsAndPatterns)
                return;

            // Precheck if any hatchings have the color before doing the expensive clip operation.
            bool hasColor = false;
            for (int i = 0; i < hatchings.Count; ++i) {
                if (hatchings[i].hatchColor == color)
                    hasColor = true;
            }

            if (!hasColor)
                return;

            // Set the clipping region to draw only inside the area.
            g.PushClip(path.GetPathKey(g));

            try {
                // CreatePensAndBrushes has already created these pens on this target.
                object[] pens = GetHatchPens();
                for (int i = 0; i < hatchings.Count; ++i) {
                    if (hatchings[i].hatchColor == color)
                        DrawOneHatching(g, path, angle, rotationCenter, renderOpts, hatchings[i], pens[i]);
                }
            }
            finally {
                g.PopClip();
            }
        }

        void DrawOneHatching(IGraphicsTarget g, SymPathWithHoles path, float angle, PointF rotationCenter, RenderOptions renderOpts, HatchInfo hatchInfo, object hatchPen)
        {
            if (hatchInfo.hatchRotateMode == RotateMode.Never) {
                angle = 0;
                rotationCenter = new PointF();
            }

            // use a transform to rotate and then draw hatching.
            Matrix matrix = new Matrix();
            matrix.Translate(rotationCenter.X, rotationCenter.Y);
            matrix.Rotate(angle);
            matrix.Rotate(hatchInfo.hatchAngle);
            g.PushTransform(matrix);

            try {
                // Get the correct bounding rect.
                matrix.Invert();
                RectangleF bounding = Geometry.BoundsOfTransformedRectangle(path.BoundingBox, matrix);

                SymbolHelpers.DrawHatchLines(g, hatchPen, hatchInfo.hatchSpacing, hatchInfo.hatchOffset, bounding);
            }
            finally {
                // restore the transform
                g.PopTransform();
            }
        }

        // Draw all patterns
        void DrawPatterns(IGraphicsTarget g, SymPathWithHoles path, SymColor color, float angle, PointF rotationCenter, RenderOptions renderOpts)
        {
            if (renderOpts.omitHatchingsAndPatterns)
                return;

            for (int i = 0; i < patterns.Count; ++i) {
                PatternInfo patternInfo = patterns[i];
                bool ignoreRotation = (patternInfo.patternRotateMode == RotateMode.Never);

                if (patternInfo.patternGlyph.HasColor(color)) {
                    // Faster to draw the pattern with a texture brush that has a bitmap
                    // of the pattern in it. Better quality to do it all with glyph drawing.
                    // Choose based on the renderOptions.
                    if (renderOpts.usePatternBitmaps && g.SupportsPatternBrushes && 
                        patternInfo.patternFillMode == PatternFillMode.Clip && !patternInfo.irregular &&
                        !PatternBrushTooPixelated(patternInfo, renderOpts.minResolution)) 
                    {
                        CreatePatternBrushes(i, renderOpts.minResolution, g);
                        DrawPatternWithTexBrush(i, g, path, ignoreRotation ? 0 : angle, ignoreRotation ? new PointF() : rotationCenter, color, renderOpts);
                    }
                    else
                        DrawPattern(patternInfo, g, path, ignoreRotation ? 0: angle, ignoreRotation ? new PointF() : rotationCenter, color, renderOpts);

                }
            }
        }

        // Draw the pattern using the texture brush.
        void DrawPatternWithTexBrush(int iPattern, IGraphicsTarget g, SymPathWithHoles path, float angle, PointF rotationCenter, SymColor color, RenderOptions renderOpts)
        {
            Debug.Assert(color != null);

            // CreatePatternBrushes has already created this brush on this target.
            object brush = GetPatternBrushKey(iPattern, color);

            if (angle != 0.0F) {
                // Set the clipping region to draw only inside the area.
                g.PushClip(path.GetPathKey(g));

                // use a transform to rotate.
                Matrix matrix = new Matrix();
                matrix.Translate(rotationCenter.X, rotationCenter.Y);
                matrix.Rotate(angle);
                g.PushTransform(matrix);

                try
                {
                    // Get the correct bounding rect.
                    matrix.Invert();
                    RectangleF bounding = Geometry.BoundsOfTransformedRectangle(path.BoundingBox, matrix);

                    g.FillRectangle(brush, bounding);
                }
                finally {
                    // restore the clip region and the transform
                    g.PopTransform();
                    g.PopClip();
                }
            }
            else {
                path.Fill(g, brush);
            }
        }

#if false
        void CreatePatternBrush() 
        {
            patternBrushes = new Dictionary<SymColor, Brush>(2);

            foreach (SymColor color in map.colors) {
                if (!patternGlyph.HasColor(color))
                    continue;

                // Create a visual with the glyph to tile in it.
                DrawingVisual visual = new DrawingVisual();
                DrawingContext dc = visual.RenderOpen();
                GraphicsTarget grTarget = new GraphicsTarget(dc);

                RenderOptions renderOpts = new RenderOptions();
                renderOpts.minResolution = 0.01F;
                renderOpts.usePatternBitmaps = false;

                patternGlyph.Draw(grTarget, new PointF(0F, 0F), -patternAngle, null, null, color, renderOpts);
                if (offsetRows) {
                    patternGlyph.Draw(grTarget, new PointF(patternWidth / 2, patternHeight), -patternAngle, null, null, color, renderOpts);
                    patternGlyph.Draw(grTarget, new PointF(-patternWidth / 2, patternHeight), -patternAngle, null, null, color, renderOpts);
                }

                dc.Close();

                // Get a drawing from the drawingvisual
                Drawing drawing = visual.Drawing;
                drawing.Freeze();

                // Create a brush from the drawing.
                DrawingBrush brush = new DrawingBrush(drawing);
                brush.Stretch = Stretch.Fill;
                brush.TileMode = TileMode.Tile;
                brush.ViewboxUnits = BrushMappingMode.Absolute;
                brush.ViewportUnits = BrushMappingMode.Absolute;
                if (offsetRows)
                    brush.Viewbox = brush.Viewport = new Rect(-patternWidth / 2, -patternHeight / 2, patternWidth, patternHeight * 2);
                else
                    brush.Viewbox = brush.Viewport = new Rect(-patternWidth / 2, -patternHeight / 2, patternWidth, patternHeight);
                brush.Transform = new RotateTransform(patternAngle);

                // Set the minimum and maximum relative sizes for regenerating the tiled brush.
                // The tiled brush will be regenerated when the size is
                //   0.5x, 0.25x (and so forth)
                // and
                //   2x, 4x, 8x (and so forth)
                // of the original size.
                System.Windows.Media.RenderOptions.SetCacheInvalidationThresholdMinimum(brush, 0.5);
                System.Windows.Media.RenderOptions.SetCacheInvalidationThresholdMaximum(brush, 2.0);

                // Set the caching hint option for the brush.
                System.Windows.Media.RenderOptions.SetCachingHint(brush, CachingHint.Cache);

                // Freeze the brush.
                brush.Freeze();

                // Add it to the collection of brushes.
                patternBrushes.Add(color, brush);
            }
        }
#endif
        const float MAX_PATTERN_SIZE = 80;
        const float MIN_PATTERN_SIZE = 10;
        
        // return true if a patten brush would be too pixelated at this size.
        bool PatternBrushTooPixelated(PatternInfo patternInfo, float pixelSize)
        {
            return (pixelSize < patternInfo.patternWidth / MAX_PATTERN_SIZE) || (pixelSize < patternInfo.patternHeight / MAX_PATTERN_SIZE);
        }

        void CreatePatternBrushes(int iPattern, float pixelSize, IGraphicsTarget gt)
        {
            PatternInfo patternInfo = patterns[iPattern];
            //  Determine adjusted pixel size of the brush to create. 
            if (pixelSize < patternInfo.patternWidth / MAX_PATTERN_SIZE)
                pixelSize = patternInfo.patternWidth / MAX_PATTERN_SIZE;
            if (pixelSize < patternInfo.patternHeight / MAX_PATTERN_SIZE)
                pixelSize = patternInfo.patternHeight / MAX_PATTERN_SIZE;
            if (pixelSize > patternInfo.patternWidth / MIN_PATTERN_SIZE)
                pixelSize = patternInfo.patternWidth / MIN_PATTERN_SIZE;
            if (pixelSize > patternInfo.patternHeight / MIN_PATTERN_SIZE)
                pixelSize = patternInfo.patternHeight / MIN_PATTERN_SIZE;

            // Get size of bitmap to create with the image of the pattern.
            float width = (float) Math.Round(patternInfo.patternWidth / pixelSize);
            float height = (float) Math.Round(patternInfo.patternHeight / pixelSize);

            int bitmapWidth = (int) width;
            int bitmapHeight = (int) (patternInfo.offsetRows ? height * 2 : height);

            RenderOptions renderOpts = new RenderOptions();
            renderOpts.minResolution = pixelSize;
            renderOpts.usePatternBitmaps = false;

            foreach (SymColor color in map.colors) {
                if (!patternInfo.patternGlyph.HasColor(color))
                    continue;

                // Create a new pattern brush.
                object brushKey = GetPatternBrushKey(iPattern, color);

                if (gt.HasBrush(brushKey))
                    continue;

                // Calculate the brush box in rendering coordinates.
                RectangleF patternBox = new RectangleF(-patternInfo.patternWidth / 2, -patternInfo.patternHeight / 2, patternInfo.patternWidth, patternInfo.patternHeight);

                // Create the brush, with coordinates set up with (0,0) in center of the brush.
                IBrushTarget brushTarget = gt.CreatePatternBrush(new SizeF(patternInfo.patternWidth, (patternInfo.offsetRows ? patternInfo.patternHeight * 2 : patternInfo.patternHeight)), patternInfo.patternAngle, bitmapWidth, bitmapHeight);

                // If the glyphPattern bounding box extend outside the brush box, then we need to render glyphPattern more then once. This
                // is calculated in both the vertical and horizontal directions.
                RectangleF patternGlyphBounds = patternInfo.patternGlyph.BoundingBox;
                Matrix m = new Matrix();
                m.Translate(patternInfo.patternOffsetX, patternInfo.patternOffsetY);
                m.Rotate(-patternInfo.patternAngle);
                patternGlyphBounds = Geometry.BoundsOfTransformedRectangle(patternGlyphBounds, m);

                int rowStart = 0, rowEnd = 0, colStart = 0, colEnd = 0;
                if (patternBox.Top > patternGlyphBounds.Top)
                    rowStart = -(int)Math.Ceiling((patternBox.Top - patternGlyphBounds.Top) / patternInfo.patternHeight);
                if (patternBox.Bottom < patternGlyphBounds.Bottom)
                    rowEnd = (int)Math.Ceiling((patternGlyphBounds.Bottom - patternBox.Bottom) / patternInfo.patternHeight);
                if (patternBox.Left > patternGlyphBounds.Left)
                    colStart = -(int)Math.Ceiling((patternBox.Left - patternGlyphBounds.Left) / patternInfo.patternWidth);
                if (patternBox.Right < patternGlyphBounds.Right)
                    colEnd = (int)Math.Ceiling((patternGlyphBounds.Right - patternBox.Right) / patternInfo.patternWidth);

                if (patternInfo.offsetRows) {
                    rowStart -= 1;
                    rowEnd += 1;
                    colStart -= 1;
                }

                // Draw the pattern into the bitmap
                Matrix matrix = new Matrix();

                for (int col = colStart; col <= colEnd; ++col) {
                    for (int row = rowStart; row <= rowEnd; ++row) {
                        matrix = new Matrix();
                        matrix.Translate(-col * patternInfo.patternWidth, -row * patternInfo.patternHeight);
                        matrix.Translate(patternInfo.patternOffsetX, patternInfo.patternOffsetY);
                        if (patternInfo.offsetRows && (row % 2) != 0)
                            matrix.Translate(-patternInfo.patternWidth / 2, 0);
                        matrix.Rotate(-patternInfo.patternAngle);
                        brushTarget.PushTransform(matrix);

                        patternInfo.patternGlyph.Draw(brushTarget, new PointF(0F, 0F), 0, null, null, color, null, renderOpts);

                        brushTarget.PopTransform();
                    }
                }

                // Get the brush
                brushTarget.FinishBrush(brushKey);
            }
        }

        // Get the key for the pattern brush for the given pattern and color, creating the key if needed.
        // Thread-safe: if two threads race, GetOrAdd returns the same stored key to both.
        // iPattern: index of the pattern in patterns.
        // color: the color the brush draws.
        private object GetPatternBrushKey(int iPattern, SymColor color)
        {
            return patternBrushes.GetOrAdd(new Pair<int, SymColor>(iPattern, color), _ => new object());
        }

        // Draw the pattern (at the given angle) inside the path.
        void DrawPattern(PatternInfo patternInfo, IGraphicsTarget g, SymPathWithHoles path, float angle, PointF rotationCenter, SymColor color, RenderOptions renderOpts)
        {
            // Set the clipping region to draw only inside the area.
            if (patternInfo.patternFillMode == PatternFillMode.Clip) {
                g.PushClip(path.GetPathKey(g));
            }

            // use a transform to rotate 
            Matrix matrix = new Matrix();
            matrix.Translate(rotationCenter.X, rotationCenter.Y);
            matrix.Rotate(angle);
            matrix.Rotate(patternInfo.patternAngle);
            matrix.Translate(patternInfo.patternOffsetX, patternInfo.patternOffsetY);
            g.PushTransform(matrix);

            try
            {
                // Get the correct bounding rect.
                matrix.Invert();
                RectangleF bounding = Geometry.BoundsOfTransformedRectangle(path.BoundingBox, matrix);

                DrawPatternRows(patternInfo, path.Transform(matrix), g, bounding, color, renderOpts);
            }
            finally {
                // restore the clip region and the transform
                g.PopTransform();
                if (patternInfo.patternFillMode == PatternFillMode.Clip) {
                    g.PopClip();
                }
            }
        }

        // Draw a set of rows of the pattern with the given rectangle
        void DrawPatternRows(PatternInfo patternInfo, SymPathWithHoles transformedPath, IGraphicsTarget g, RectangleF boundingRect, SymColor color, RenderOptions renderOpts)
        {
            RectangleF patternGlyphBounds = Geometry.BoundsOfRotatedRectangle(patternInfo.patternGlyph.BoundingBox, new PointF(0, 0), -patternInfo.patternAngle);
            RectangleF patternBounds = new RectangleF(-patternInfo.patternWidth / 2, -patternInfo.patternHeight / 2, patternInfo.patternWidth, patternInfo.patternHeight);

            long topLineCount = (long)Math.Round((boundingRect.Top - patternGlyphBounds.Bottom + patternBounds.Bottom) / patternInfo.patternHeight);
            double topLine = topLineCount * patternInfo.patternHeight;
            double bottomLine = (Math.Round((boundingRect.Bottom - patternGlyphBounds.Top + patternBounds.Top) / patternInfo.patternHeight) + 0.5) * patternInfo.patternHeight;
            double leftLine = Math.Round((boundingRect.Left - patternGlyphBounds.Right + patternBounds.Right) / patternInfo.patternWidth) * patternInfo.patternWidth;
            double rightLine = (Math.Round((boundingRect.Right - patternGlyphBounds.Left + patternBounds.Left) / patternInfo.patternWidth) + 0.5) * patternInfo.patternWidth;
            double offsetLeftLine = leftLine - (patternInfo.patternWidth / 2);
            double offsetRightLine = rightLine + (patternInfo.patternWidth / 2);
            bool firstLineOffset = (topLineCount & 1) != 0;

            bool offsetThisLine = patternInfo.offsetRows && firstLineOffset;

            GlyphPartFilter filter;
            switch (patternInfo.patternFillMode) {
                case PatternFillMode.Clip:
                    filter = null;
                    break;
                case PatternFillMode.CompletelyInside:
                    filter = (PointF[] points, PointF center) => points.All(pt => transformedPath.IsInside(pt));
                    break;
                case PatternFillMode.CenterInside:
                    filter = (PointF[] points, PointF center) => transformedPath.IsInside(center);
                    break;
                case PatternFillMode.PartiallyInside:
                    filter = (PointF[] points, PointF center) => points.Any(pt => transformedPath.IsInside(pt));
                    break;
                default:
                    Debug.Fail("Unexpected PatternFillMode");
                    filter = null;
                    break;
            }

            for (double y = topLine; y <= bottomLine; y += patternInfo.patternHeight) {
                if (offsetThisLine) {
                    for (double x = offsetLeftLine; x <= offsetRightLine; x += patternInfo.patternWidth) {
                        PointF location = HandleIrregularity(patternInfo, x, y);
                        patternInfo.patternGlyph.Draw(g, location, -patternInfo.patternAngle, null, null, color, filter, renderOpts);
                    }
                }
                else {
                    for (double x = leftLine; x <= rightLine; x += patternInfo.patternWidth) {
                        PointF location = HandleIrregularity(patternInfo, x, y);
                        patternInfo.patternGlyph.Draw(g, location, -patternInfo.patternAngle, null, null, color, filter, renderOpts);
                    }
                }

                if (patternInfo.offsetRows)
                    offsetThisLine = !offsetThisLine;
            }
        }

        // Handle irregularity by offseting x and y by random amounts if irregularity was asked for.
        private PointF HandleIrregularity(PatternInfo patternInfo, double x, double y)
        {
            if (!patternInfo.irregular) {
                return new PointF((float)x, (float)y);
            }

            // Irregularity is desired. Figure out the maximum displacement (either positive or negative) in each direction.

            // First get max displacements based on percentage.
            double maxXDisplacement = patternInfo.patternWidth / 2.0F * patternInfo.irregularVarX;
            double maxYDisplacement = patternInfo.patternHeight / 2.0F * patternInfo.irregularVarY;

            // Then, if the max displacement based on radius of pattern is smaller, use that.
            maxXDisplacement = Math.Min(maxXDisplacement, patternInfo.patternWidth / 2.0F - patternInfo.patternGlyph.Radius);
            maxYDisplacement = Math.Min(maxYDisplacement, patternInfo.patternHeight / 2.0F - patternInfo.patternGlyph.Radius);

            // Then, if the max displacement based on nearest distance is smaller, use that.
            maxXDisplacement = Math.Min(maxXDisplacement, maxXDisplacement - patternInfo.irregularMinDist * 0.5F);
            if (patternInfo.offsetRows)
                maxYDisplacement = Math.Min(maxYDisplacement, (patternInfo.patternHeight - (patternInfo.patternGlyph.Radius * 2 + patternInfo.irregularMinDist) * 0.85F) * 0.5F);  
            else
                maxYDisplacement = Math.Min(maxYDisplacement, maxYDisplacement - patternInfo.irregularMinDist * 0.5F);

            // But max displacement can't be smaller than zero.
            maxXDisplacement = Math.Max(0, maxXDisplacement);
            maxYDisplacement = Math.Max(0, maxYDisplacement);

            // Now get random displacement in X and Y from the max displacement.
            // The randomness actually needs to be deterministic according to the x and y location, so that
            // if different areas overlap they achieve the same randomness. We create a seed based on x and y.
            int seed = 17;
            seed = seed * 23 + Math.Round(x, 2).GetHashCode();
            seed = seed * 23 + Math.Round(y, 2).GetHashCode();
            Random rand = new Random(seed);
            double xDisplacement = (rand.NextDouble() * 2.0 - 1.0) * maxXDisplacement;
            double yDisplacement = (rand.NextDouble() * 2.0 - 1.0) * maxYDisplacement;

            return new PointF((float)(x + xDisplacement), (float)(y + yDisplacement));
        }

        // Calculate the bounding box
        internal override RectangleF CalcBounds(SymPathWithHoles path)
        {
            if (borderSymdef != null)
                return borderSymdef.CalcBounds(path.MainPath);
            else
                return path.BoundingBox;
        }
    }

    // The horizontal alignment of text symbols.
    public enum TextSymDefHorizAlignment: byte
    {
        Default,       // Default as set in SymDef.
        Left,
        Right,
        Center,
        Justified
    }

    // The vertical alignment of text symbols.
    public enum TextSymDefVertAlignment: byte
    {
        Default,        // Default as set in SymDef.
        TopAscent,      // At the top of the ascent line
        Midpoint,       // Midpoint between ascent and baseline of first line
        Baseline,       // At the baseline of the first line
        BaselineLast,   // At the baseline of the last line
        MidpointAllLines, // Midpoint between ascent of top line and baseline of last line.
        Bottom,         // Baseline of last line.
    }

    // A coordinate (line/column) in text.
    public struct TextCoord
    {
        public readonly int Line;  // 0-based line.
        public readonly int Col;  // 0-base column.

        public override bool Equals(object obj)
        {
            if (obj is TextCoord) {
                TextCoord other = (TextCoord)obj;
                return (this.Line == other.Line && this.Col == other.Col);
            }
            else {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return Line.GetHashCode() + 1737 * Col.GetHashCode();
        }

        public TextCoord(int line, int col)
        { 
            Line = line;
            Col = col;
        }
    }

    // This class contains data that remembers the mapping between unwrapped and wrapped text coordinates.
    public class TextCoordMapper
    {
        // For each wrapped line, the unwrapped coordinates of the start of that line.
        List<TextCoord> unwrappedCoordOfWrappedLine = new List<TextCoord>();

        internal void AddUnwrappedCoord(TextCoord unwrappedTextCoord)
        {
            unwrappedCoordOfWrappedLine.Add(unwrappedTextCoord);
        }

        // Add all the coordinates from another coord mapper, applying a line offset.
        internal void AddCoordMapper(int lineOffset, TextCoordMapper coordMapperToAdd)
        {
            foreach (TextCoord unwrappedCoord in coordMapperToAdd.unwrappedCoordOfWrappedLine) {
                AddUnwrappedCoord(new TextCoord(unwrappedCoord.Line + lineOffset, unwrappedCoord.Col));
            }
        }

        public TextCoord UnwrappedFromWrapped(TextCoord wrappedTextCoord, string[] unwrappedText, string[] wrappedText)
        {
            Debug.Assert(wrappedText.Length == unwrappedCoordOfWrappedLine.Count);
            Debug.Assert(wrappedTextCoord.Line >= 0 && wrappedTextCoord.Line < unwrappedCoordOfWrappedLine.Count);
            Debug.Assert(wrappedTextCoord.Col >= 0 && wrappedTextCoord.Col <= wrappedText[wrappedTextCoord.Line].Length);

            int unwrappedLine = unwrappedCoordOfWrappedLine[wrappedTextCoord.Line].Line;
            int unwrappedCol = unwrappedCoordOfWrappedLine[wrappedTextCoord.Line].Col + wrappedTextCoord.Col;

            return new TextCoord(unwrappedLine, unwrappedCol);
        }

        public TextCoord WrappedFromUnwrapped(TextCoord unwrappedTextCoord, string[] unwrappedText, string[] wrappedText)
        {
            Debug.Assert(wrappedText.Length == unwrappedCoordOfWrappedLine.Count);
            Debug.Assert(unwrappedTextCoord.Line >= 0 && unwrappedTextCoord.Line < unwrappedText.Length);
            Debug.Assert(unwrappedTextCoord.Col >= 0 && unwrappedTextCoord.Col <= unwrappedText[unwrappedTextCoord.Line].Length);

            int wrappedLine = 0, wrappedCol = 0;

            for (int i = 0; i < unwrappedCoordOfWrappedLine.Count; ++i) {
                TextCoord unwrappedCoordAtLineStart = unwrappedCoordOfWrappedLine[i];
                if (unwrappedCoordAtLineStart.Line == unwrappedTextCoord.Line && unwrappedCoordAtLineStart.Col <= unwrappedTextCoord.Col) {
                    wrappedLine = i;
                    wrappedCol = unwrappedTextCoord.Col - unwrappedCoordAtLineStart.Col;

                    if (i < unwrappedCoordOfWrappedLine.Count - 1) {
                        TextCoord unwrappedCoordAtNextLineStart = unwrappedCoordOfWrappedLine[i];
                        if (unwrappedCoordAtLineStart.Line != unwrappedTextCoord.Line ||
                            unwrappedCoordAtLineStart.Col > unwrappedTextCoord.Col) {
                            break;
                        }
                    }
                    else {
                        break;
                    }
                }
            }

            return new TextCoord(wrappedLine, wrappedCol);
        }

    }

    public class TextSymDef: SymDef
    {
        public enum FramingStyle {None, Line, Shadow, Rectangle };

        public enum PreferredSymbolKind { NormalText, LineText, None }

        public struct Framing {
            public FramingStyle framingStyle;
            public SymColor framingColor;
            public float lineWidth;
            public LineStyle lineStyle;
            public float shadowX, shadowY;
            public float rectBorderLeft, rectBorderTop, rectBorderRight, rectBorderBottom;
        }

        public class Underlining: ICloneable
        {
            public bool underlineOn;
            public SymColor underlineColor;
            public float underlineWidth;
            public float underlineDistance;

            public object Clone()
            {
                return this.MemberwiseClone();
            }
        }

        // Struct for static methods involving wrapping text.
        public struct WrapTextProperties
        {
            public ITextFaceMetrics TextFaceMetrics;
            public float[] Tabs;
            public float CharSpacing;
            public float WordSpacing;
            public TextSymDefHorizAlignment FontAlign;
            public float FirstIndent;
            public float RestIndent;
            public bool AddParagraphMarks;
        }

        PreferredSymbolKind preferredSymbolKind;
        SymColor fontColor;
        float fontSize;
        string fontName;
        TextEffects effects;
        TextSymDefHorizAlignment defaultHorizAlign;
        TextSymDefVertAlignment defaultVertAlign;
        float lineSpacing;
        float paraSpacing;   
        float charSpacing, wordSpacing;
        float firstIndent, restIndent;
        float[] tabs;
        Framing framing;
        Underlining underline = new Underlining();
        PointSymDef centerPointSymdef;          // if non-null, the center point symdef to use.

        // GDI+ object correspoding to the above attributes.
        // Keys for the pens used to draw line framing, one per stroke width. Only written by GetKeyArray
        // (see GetFramingPens), which keeps it safe for several threads drawing at once.
        object[] framingPens;
        object font = new object();

        // How to draw underlining font effect.
        const float fontEffectUnderlineWidth = 0.05F;
        const float fontEffectUnderlineDistance = 0.02F;

        // Font metrics, created on demand by EnsureFontMetrics. Several threads can draw or measure this symdef
        // at once, so it is only written with Interlocked operations (see EnsureFontMetrics and FreeGdiObjects).
        ITextFaceMetrics textFaceMetrics;


        public const string ParagraphMark = "\x2029";  // string denoted a paragraph boundary (Unicode paragraph mark).

        public TextSymDef(string name, string symbolId, PreferredSymbolKind preferredSymbolKind, PointSymDef centerPointSymdef)
            : base(name, symbolId)
        {
            this.preferredSymbolKind = preferredSymbolKind;
            this.centerPointSymdef = centerPointSymdef;
        }

        public override void SetMap(Map newMap)
        {
            base.SetMap(newMap);
        }

        public PointSymDef CenterPointSymdef { get { return centerPointSymdef; } }

        public PreferredSymbolKind SymbolKind { get { return preferredSymbolKind; } }

        public override SymDef DependsOnSymdef {
            get {
                return centerPointSymdef;
            }
        }

        // Create the font metrics if they haven't been created yet. Thread-safe and lock-free: if two threads
        // race, the one that loses disposes the metrics it created and uses the winner's. After this returns,
        // textFaceMetrics is non-null.
        private void EnsureFontMetrics()
        {
            if (Volatile.Read(ref textFaceMetrics) == null) {
                ITextFaceMetrics newMetrics = map.TextMetricsProvider.GetTextFaceMetrics(fontName, fontSize, effects);
                if (Interlocked.CompareExchange(ref textFaceMetrics, newMetrics, null) != null)
                    newMetrics.Dispose();   // Another thread got there first.
            }
        }

        private void CreateObjects(IGraphicsTarget g)
        {
            EnsureFontMetrics();

            if (!g.HasFont(font))
                g.CreateFont(font, fontName, fontSize, (effects & ~TextEffects.Underline)); // Underlining done otherwise.

            if (framing.framingStyle == FramingStyle.Line /* && framing.framingColor != null */) {
                // We use multiple pens to avoid weird artifacts with overlapping parts.
                object[] pens = GetFramingPens();
                int iPen = 0;
                for (float width = fontSize * 0.33F; width < framing.lineWidth * 2; width += fontSize * 0.33F) {
                    if (! g.HasPen(pens[iPen]))
                        GraphicsUtil.CreateSolidPen(g, pens[iPen], framing.framingColor.ColorValue, width, LineStyle.Beveled);
                    ++iPen;
                }

                if (!g.HasPen(pens[iPen]))
                    GraphicsUtil.CreateSolidPen(g, pens[iPen], framing.framingColor.ColorValue, framing.lineWidth * 2, framing.lineStyle);
            }
        }

        // Get the number of pens used to draw line framing: one for each intermediate stroke width,
        // plus one for the full framing width. Must use the same loop as CreateObjects so the counts match.
        private int FramingPenCount()
        {
            int count = 1;
            for (float width = fontSize * 0.33F; width < framing.lineWidth * 2; width += fontSize * 0.33F)
                ++count;
            return count;
        }

        // Get the keys for the pens used to draw line framing, creating them if needed. Thread-safe: the
        // returned array always has FramingPenCount() entries and is never modified afterward.
        private object[] GetFramingPens()
        {
            return GetKeyArray(ref framingPens, FramingPenCount());
        }

        public override void FreeGdiObjects()
        {
            // Called with the map write lock held, so no other thread is drawing with these metrics.
            ITextFaceMetrics oldMetrics = Interlocked.Exchange(ref textFaceMetrics, null);
            if (oldMetrics != null)
                oldMetrics.Dispose();
        }


        // Does this symbol definition draw the given color. Used to determine
        // if this symbol definition draws into the current layer being draw.
        public override bool HasColor(SymColor color)
        {
            Debug.Assert(color != null);
            if (color.IsSpecialLayer)
                return false;

            return color == fontColor || 
                   (framing.framingStyle != FramingStyle.None && color == framing.framingColor) || 
                   (underline.underlineOn && color == underline.underlineColor) ||
                   (centerPointSymdef != null && centerPointSymdef.HasColor(color));
        }

        public void SetFont(string fontName, float fontSize, TextEffects effects, SymColor fontColor, float lineSpacing, float paraSpacing, 
                            float firstIndent, float restIndent, float[] tabs, float charSpacing, float wordSpacing, 
                            TextSymDefHorizAlignment defaulHorizAlign, TextSymDefVertAlignment defaultVertAlign)
        {
            CheckModifiable();
            this.fontName = fontName;
            this.fontSize = fontSize;
            this.effects = effects;
            this.fontColor = fontColor;
            this.lineSpacing = lineSpacing;
            this.paraSpacing = paraSpacing;
            this.firstIndent = firstIndent;
            this.restIndent = restIndent;
            this.charSpacing = charSpacing;
            this.wordSpacing = wordSpacing;
            this.defaultHorizAlign = defaulHorizAlign;
            this.defaultVertAlign = defaultVertAlign;
            this.tabs = tabs;
        }

        // Set tab stops to be the width of a particular string.
        public void SetRepeatingTabs(Map map, string tabTextWidth)
        {
            CheckModifiable();
            float tabWidth = map.TextMetricsProvider.GetTextFaceMetrics(fontName, fontSize, effects).GetTextWidth(tabTextWidth);
            this.tabs = new float[1] { tabWidth };
        }

        public void SetFraming(Framing framing)
        {
            CheckModifiable();
            this.framing = framing;
        }

        public void SetUnderline(Underlining underline)
        {
            CheckModifiable();
            this.underline = (Underlining) underline.Clone();
        }

        public override SymDef CopyToMap(Map map)
        {
            var newCenterPointSymdef = centerPointSymdef == null ? null : (PointSymDef) map.SymdefFromSymbolId(centerPointSymdef.SymbolId);
            var newSymDef = new TextSymDef(Name, SymbolId, preferredSymbolKind, newCenterPointSymdef);

            float[] newTabs = (tabs == null) ? null : (float[])tabs.Clone();
            newSymDef.SetFont(fontName, fontSize, effects, map.SymColorFromSymColor(fontColor), lineSpacing, paraSpacing, 
                              firstIndent, restIndent, newTabs, charSpacing, wordSpacing, defaultHorizAlign, defaultVertAlign);

            if (framing.framingStyle != FramingStyle.None) {
                Framing newFraming = framing;
                newFraming.framingColor = map.SymColorFromSymColor(newFraming.framingColor);
                newSymDef.SetFraming(newFraming);
            }

            if (underline.underlineOn) {
                Underlining newUnderlining = (Underlining) underline.Clone();
                newUnderlining.underlineColor = map.SymColorFromSymColor(newUnderlining.underlineColor);
                newSymDef.SetUnderline(newUnderlining);
            }

            map.AddSymdef(newSymDef);
            return newSymDef;
        }

        public TextSymDefHorizAlignment FontAlignment { get { return defaultHorizAlign; } }
        public TextSymDefVertAlignment VertAlignment { get { return defaultVertAlign; } }
        public string FontName { get { return fontName; } }
        public TextEffects TextEffects { get { return effects; } }
        public SymColor FontColor { get { return fontColor; } }
        public float LineSpacing { get { return lineSpacing; } }
        public float ParaSpacing { get { return paraSpacing; } }
        public float FirstIndent { get { return firstIndent; } }
        public float RestIndent { get { return restIndent; } }
        public float CharSpacing { get { return charSpacing; } }
        public float WordSpacing { get { return wordSpacing; } }
        public float FontEmHeight { get { return fontSize; } }
        public float[] Tabs { get { return tabs; } }
        public Framing FramingInfo { get { return framing; } }
        public Underlining Underline { get { return underline; } }

        public float FontAscent
        {
            get
            {
                EnsureFontMetrics();

                return textFaceMetrics.Ascent;
            }
        }

        // Get height of the "W" character.
        public float WHeight
        {
            get
            {
                EnsureFontMetrics();

                return textFaceMetrics.CapHeight; 
            }
        }

        public float FontDescent
        {
            get
            {
                EnsureFontMetrics();

                return textFaceMetrics.Descent;
            }
        }

        // Return the offset from the top of the ascent of various vertical alignment options.
        internal float VertOffset(TextSymDefVertAlignment vertAlignment, string[] text)
        {
            TextSymDefVertAlignment vertAlign = GetVertAlignment(vertAlignment);

            switch (vertAlign) {
                case TextSymDefVertAlignment.TopAscent:
                    return 0;
                case TextSymDefVertAlignment.Baseline:
                    return FontAscent;
                case TextSymDefVertAlignment.Bottom:
                    return FontAscent + FontDescent;
                case TextSymDefVertAlignment.Midpoint:
                    return FontAscent / 2.0F;
                case TextSymDefVertAlignment.MidpointAllLines:
                    return (TextHeight(text) - FontDescent) / 2.0F;
                case TextSymDefVertAlignment.BaselineLast:
                    return (TextHeight(text) - FontDescent);
                default:
                    throw new ArgumentException("Bad vertical alignment");
            }
        }

        // Return the offset from the top of the ascent of various vertical alignment options for line text.
        internal float LineTextVertOffset(TextSymDefVertAlignment vertAlignment)
        {
            TextSymDefVertAlignment vertAlign = GetVertAlignment(vertAlignment);

            switch (vertAlign) {
                case TextSymDefVertAlignment.TopAscent:
                    return FontAscent - WHeight;

                case TextSymDefVertAlignment.Baseline:
                    return FontAscent;

                case TextSymDefVertAlignment.Bottom:
                    return FontAscent + FontDescent;

                case TextSymDefVertAlignment.Midpoint:
                case TextSymDefVertAlignment.MidpointAllLines:
                    return FontAscent - WHeight / 2.0F;

                default:
                    throw new ArgumentException("Bad vertical alignment");
            }
        }

        // Draw a single line of text at the given point with the given brush.
        private void DrawSingleLineString(IGraphicsTarget g, string text, object brushKey, PointF pt)
        {
            g.DrawText(text, font, brushKey, pt);
        }

        // Get the horizontal alignment to use, taking into account both the object and symdef alignment. 
        // Never returns default.
        internal TextSymDefHorizAlignment GetHorizAlignment(TextSymDefHorizAlignment objectHorizAlignment)
        {
            TextSymDefHorizAlignment fontAlign = objectHorizAlignment;
            if (fontAlign == TextSymDefHorizAlignment.Default)
                fontAlign = defaultHorizAlign;
            if (fontAlign == TextSymDefHorizAlignment.Default)
                fontAlign = TextSymDefHorizAlignment.Left;

            return fontAlign;
        }

        internal TextSymDefVertAlignment GetVertAlignment(TextSymDefVertAlignment objectVertAlignment)
        {
            TextSymDefVertAlignment vertAlign = objectVertAlignment;
            if (vertAlign == TextSymDefVertAlignment.Default)
                vertAlign = defaultVertAlign;
            if (vertAlign == TextSymDefVertAlignment.Default)
                vertAlign = TextSymDefVertAlignment.TopAscent;

            return vertAlign;
        }

        // Get properties needed to wrap text for this symdef.
        private WrapTextProperties GetWrapProperties(TextSymDefHorizAlignment objectHorizAlignment)
        {
            EnsureFontMetrics();

            return new WrapTextProperties() { 
                TextFaceMetrics = textFaceMetrics,
                Tabs = tabs,
                CharSpacing = charSpacing,
                WordSpacing = wordSpacing,
                FontAlign = GetHorizAlignment(objectHorizAlignment),
                FirstIndent = firstIndent,
                RestIndent = restIndent,
                AddParagraphMarks = true
            };
        }

        // Measure the width of a single line of text.
        private float MeasureStringWidth(string text, TextSymDefHorizAlignment objectHorizAlignment)
        {
            return MeasureStringWidth(GetWrapProperties(objectHorizAlignment), text);
        }

        // Measure the width of a single line of text.
        private static float MeasureStringWidth(WrapTextProperties wrapProperties, string text)
        {
            return wrapProperties.TextFaceMetrics.GetTextWidth(text);
        }

        // Draw a string with shadow or line framing effects, if specified. The font from this symdef is used.
        private void DrawStringWithEffects(IGraphicsTarget g, SymColor color, string text, PointF pt)
        {
            Debug.Assert(color != null);

            if (fontColor != null && color == fontColor) {
                DrawSingleLineString(g, text, fontColor.GetBrushKey(g), pt);
            }

            if (framing.framingStyle != FramingStyle.None && framing.framingColor != null && color == framing.framingColor) {
                if (framing.framingStyle == FramingStyle.Line) {
                    DrawSingleLineString(g, text, framing.framingColor.GetBrushKey(g), pt);
                    // CreateObjects has already created these pens on this target.
                    foreach (object p in GetFramingPens())
                        g.DrawTextOutline(text, font, p, pt);
                }
                else if (framing.framingStyle == FramingStyle.Shadow) {
                    DrawSingleLineString(g, text, framing.framingColor.GetBrushKey(g), new PointF(pt.X + framing.shadowX, pt.Y - framing.shadowY));
                }
            }
        }

        // Draw the framing rectangle around some text. The top of the text is at 0, and the bottom baseline of text is at "bottomOfText".
        private void DrawFramingRectangle(IGraphicsTarget g, float[] lineWidths, float fullWidth, SymColor color, float topOfText, float bottomOfText, TextSymDefHorizAlignment objectHorizAlignment)
        {
            if (framing.framingStyle == FramingStyle.Rectangle && color == framing.framingColor) {
                RectangleF textRect = CalcTextRectangle(lineWidths, fullWidth, topOfText, bottomOfText, objectHorizAlignment);

                // Add padding.
                float t = textRect.Top - framing.rectBorderTop;
                float b = textRect.Bottom + framing.rectBorderBottom;
                float l = textRect.Left - framing.rectBorderLeft;
                float r = textRect.Right + framing.rectBorderRight;

                // Draw the rectangle
                g.FillRectangle(color.GetBrushKey(g), new RectangleF(l, t, r - l, b - t));
            }
        }

        // Calculate the text rectangle of a piece of text, as for framing.
        private RectangleF CalcTextRectangle(float[] lineWidths, float fullWidth, float topOfText, float bottomOfText, TextSymDefHorizAlignment objectHorizAlignment) {
            float l, t, r, b;  // The left, top, right, bottom of the rectangle.
            TextSymDefHorizAlignment fontAlign = GetHorizAlignment(objectHorizAlignment);

            // First, figure out the width of the rectangle. If fullWidth is zero, used the maximum line width.
            fullWidth = CalcFullWidth(lineWidths, fullWidth, fontAlign);

            // Next, figure out the rectangle, not counting padding.

            if (fontAlign == TextSymDefHorizAlignment.Right)
                l = -fullWidth;
            else if (fontAlign == TextSymDefHorizAlignment.Center)
                l = -(fullWidth / 2F);
            else
                l = 0;
            r = l + fullWidth;
            t = topOfText + FontAscent - WHeight;
            b = bottomOfText;
            return RectangleF.FromLTRB(l, t, r, b);
        }

        private float CalcFullWidth(float[] lineWidths, float fullWidth, TextSymDefHorizAlignment objectHorizAlignment)
        {
            TextSymDefHorizAlignment fontAlign = GetHorizAlignment(objectHorizAlignment);

            if (fullWidth == 0) {
                foreach (float w in lineWidths) {
                    if (w > fullWidth)
                        fullWidth = w;
                }
                if (fontAlign == TextSymDefHorizAlignment.Justified || fontAlign == TextSymDefHorizAlignment.Left)
                    fullWidth += firstIndent;   // if fullWidth is zero, this is unformatted text, and only the firstIndent is used.
            }
            return fullWidth;
        }

        // Helper for GetNextTextElement to handle empty/null case.
        static string GetNextTextElement(string s)
        {
            if (string.IsNullOrEmpty(s))
                return "";
            else
                return StringInfo.GetNextTextElement(s);
        }

        // Draw the center point symbol of the text, if applicable.
        private void DrawCenterPoint(IGraphicsTarget g, float[] lineWidths, float fullWidth, SymColor color, float topOfText, float bottomOfText, TextSymDefHorizAlignment objectHorizAlignment, RenderOptions renderOpts) 
        {
            TextSymDefHorizAlignment fontAlign = GetHorizAlignment(objectHorizAlignment);

            // OCAD only draws center point for unformatted text (fullWidth == 0).
            if (centerPointSymdef != null && centerPointSymdef.HasColor(color) && fullWidth == 0) {
                float l, r, t, b;  // bounding rectangle for calculating center point.

                // First, figure out the width of the rectangle. If fullWidth is zero, used the maximum line width.
                fullWidth = CalcFullWidth(lineWidths, fullWidth, fontAlign);

                // Next, figure out the rectangle, not counting padding.
                if (fontAlign == TextSymDefHorizAlignment.Right)
                    l = -fullWidth;
                else if (fontAlign == TextSymDefHorizAlignment.Center)
                    l = -(fullWidth / 2F);
                else
                    l = 0;
                r = l + fullWidth;
                t = topOfText - (FontAscent + FontDescent) + FontEmHeight;  // strange OCAD adjustment.
                b = bottomOfText + FontDescent;  // include the descender on the text.

                PointF centerPoint = new PointF((r + l) / 2, (t + b) / 2);

                // We're currently in a reversed coordinate space;  re-reverse it.
                Matrix matrix = new Matrix();
                matrix.Translate(centerPoint.X, centerPoint.Y);
                matrix.Scale(1, -1);      // Reverse Y so text is correct way aroun
                g.PushTransform(matrix);

                try {
                    centerPointSymdef.Draw(g, new PointF(0, 0), 0, null, color, renderOpts);
                }
                finally {
                    g.PopTransform();  // restore transform
                }
            }
        }

        // Draw an underline under the text, if applicable.
        private void DrawUnderline(IGraphicsTarget g, Underlining underlining, SymColor color, float baseline, float width, float indent, TextSymDefHorizAlignment objectHorizAlignment)
        {
            if (underlining.underlineOn && color != null && color == underlining.underlineColor) {
                TextSymDefHorizAlignment fontAlign = GetHorizAlignment(objectHorizAlignment);

                // Figure out the left and right sides of the underline.
                float l, r;
                if (fontAlign == TextSymDefHorizAlignment.Right)
                    l = -width;
                else if (fontAlign == TextSymDefHorizAlignment.Center)
                    l = -(width / 2F);
                else
                    l = indent;
                r = l + width;

                // figure out y coordinate of line.
                float y = baseline + underlining.underlineDistance + underlining.underlineWidth / 2;

                // Create pen (use the Underlining object for the pen)
                if (!g.HasPen(underlining))
                    GraphicsUtil.CreateSolidPen(g, underlining, underlining.underlineColor.ColorValue, underlining.underlineWidth, LineStyle.Mitered);

                // draw the line.
                g.DrawLine(underlining, new PointF(l, y), new PointF(r, y));
            }
        }


        // Draw this text symbol at point pt with angle ang in this graphics (given color only). 
        internal void Draw(IGraphicsTarget g, string[] text, float[] lineWidths, PointF location, float angle,  float fullWidth, SymColor color, 
                           TextSymDefHorizAlignment objectHorizAlignment, TextSymDefVertAlignment objectVertAlignment, RenderOptions renderOpts)
        {
            // WARNING: changes to this likely mean changes to FindInsertionPoint! Keep them synchronized.

            TextSymDefHorizAlignment fontAlign = GetHorizAlignment(objectHorizAlignment);
            TextSymDefVertAlignment vertAlign = GetVertAlignment(objectVertAlignment);

            Debug.Assert(color != null);

            if (fontColor != null && color != fontColor && 
                (framing.framingStyle == FramingStyle.None || color != framing.framingColor) && 
                (!underline.underlineOn || color != underline.underlineColor) &&
                (centerPointSymdef == null || !centerPointSymdef.HasColor(color)))
                return;

            CreateObjects(g);

            Underlining underliningFontEffect = new Underlining();
            if ((effects & TextEffects.Underline) != 0) {
                underliningFontEffect.underlineOn = true;
                underliningFontEffect.underlineColor = fontColor;
                underliningFontEffect.underlineDistance = FontEmHeight * fontEffectUnderlineDistance;
                underliningFontEffect.underlineWidth = FontEmHeight * fontEffectUnderlineWidth;
            }
            
            // Move location to draw at to the origin.
            Matrix matrix = new Matrix();
            matrix.Translate(location.X, location.Y);
            matrix.Scale(1, -1); // Reverse Y direction so text is correct way around.
            if (angle != 0)
                matrix.RotateAt(-angle, new PointF(0,0));
            g.PushTransform(matrix);

            try {
                // Draw all the lines of text.
                PointF pt = new PointF(0F, 0F);
                float baselineOfLine = 0;          // y coordinate of baseline of line.
                bool firstLineOfPara = true, lastLineOfPara;

                pt.Y -= VertOffset(vertAlign, text);

                float topOfFirstLine = pt.Y;

                for (int lineIndex = 0; lineIndex < text.Length; ++lineIndex) {
                    string line = text[lineIndex];
                    float lineWidth = lineWidths[lineIndex];

                    lastLineOfPara = (lineIndex == text.Length - 1 || text[lineIndex + 1] == ParagraphMark);        // are we on the last line of a paragraph?

                    if (line == ParagraphMark) {
                        pt.Y += paraSpacing;
                        firstLineOfPara = true;
                    }
                    else {
                        float indent = 0;
                        float leftEdge;          // tabs are relative to this X position.
                        if (fontAlign == TextSymDefHorizAlignment.Right)
                            pt.X = leftEdge = -lineWidth;
                        else if (fontAlign == TextSymDefHorizAlignment.Center)
                            pt.X = leftEdge = -(lineWidth / 2F);
                        else {
                            leftEdge = 0;
                            pt.X = indent = firstLineOfPara ? firstIndent : restIndent;     // indents only used for left align or justified
                        }

                        // Get the size of spaces. Justification is done by adjusting this.
                        float sizeOfSpace = wordSpacing * textFaceMetrics.SpaceWidth;            // basic width of spaces as set by the symdef
                        if (fontAlign == TextSymDefHorizAlignment.Justified && !lastLineOfPara && fullWidth > 0)
                            sizeOfSpace += JustifyText(line, lineWidth, fullWidth - indent);

                        // Draw all the text segments in the line. (A text segment is a word, unless charSpacing>0, in which case it is graphemes).
                        int index = 0;
                        for (; ; ) {
                            string textSegment;

                            if (charSpacing > 0)
                                textSegment = GetNextTextElement(line.Substring(index));
                            else
                                textSegment = GetNextTextSegment(line.Substring(index));
                            if (string.IsNullOrEmpty(textSegment))
                                break;

                            if (textSegment == " ")
                                pt.X += sizeOfSpace;
                            else if (textSegment == "\t")
                                pt.X += WidthOfTextSegment("\t", pt.X - leftEdge, fontAlign);
                            else {
                                DrawStringWithEffects(g, color, textSegment, pt);
                                pt.X += MeasureStringWidth(textSegment, fontAlign);

                                if (charSpacing > 0)
                                    pt.X += charSpacing * textFaceMetrics.SpaceWidth;
                            }

                            index += textSegment.Length;
                        }

                        baselineOfLine = pt.Y + FontAscent;        // Set the bottom of the text.

                        // Draw underlining. For the font underlining effect, we draw it line this instead of setting the effect
                        // in the font so that it works right with extra character and space widths.
                        DrawUnderline(g, underliningFontEffect, color, baselineOfLine, lineWidth, indent, fontAlign);
                        if (lastLineOfPara)
                            DrawUnderline(g, this.underline, color, baselineOfLine, Math.Max(fullWidth, lineWidth), (fullWidth == 0) ? indent : 0, fontAlign);

                        pt.Y += lineSpacing;
                        firstLineOfPara = false;
                    }
                }

                // Draw the center point, if any.
                DrawCenterPoint(g, lineWidths, fullWidth, color, topOfFirstLine, baselineOfLine, fontAlign, renderOpts);

                // Draw the framing rectangle, if any.
                if (underline.underlineOn)
                    baselineOfLine += underline.underlineDistance + underline.underlineWidth;
                DrawFramingRectangle(g, lineWidths, fullWidth, color, topOfFirstLine, baselineOfLine, fontAlign);
            }
            finally {
                g.PopTransform();
            }
        }

        // The geometry method of the text symbol definitions: the clipping objects of a text symbol (TextSymbol), as Draw
        // draws it, or of a text along a line (LineTextSymbol), as DrawTextOnPath draws it, in a palette color. Only
        // these two kinds of symbols take this definition.
        internal override IReadOnlyList<ClippingObject> MakeClippingObjects(Symbol symbol, SymColor color, CoveragePrototype prototype)
        {
            Debug.Assert(prototype == CoveragePrototype.FullCoverage, "A text has the full coverage only.");
            if (symbol is TextSymbol text)
                return MakeClippingObjects(text.WrappedText, text.WrappedLineWidths, text.Location, text.Rotation, text.Width, color, text.HorizontalAlignment, text.VerticalAlignment);
            if (symbol is LineTextSymbol lineText)
                return MakeClippingObjectsOfTextOnPath(lineText.Path, lineText.Text, color, lineText.HorizontalAlignment, lineText.VerticalAlignment);
            throw new ArgumentException("Only a TextSymbol and a LineTextSymbol take a TextSymDef.", nameof(symbol));
        }

        // The geometry method of a text symbol: the clipping objects that Draw draws in a palette color, in the same order.
        // Each word, or each grapheme where the character spacing is more than 0, is a text run at its own place, in the
        // font color, and in the color of the framing as a framing line or a shadow. The underlines of each line follow
        // its runs, and the center point and the framing rectangle come last. The innermost step of their placement is the
        // pose of the text: its location and its angle. Draw lays out the text with its Y axis down, and the shapes are in
        // the coordinates of the pose, whose Y axis points up as the axis of the map does: the y coordinate of each point
        // is the opposite of its y coordinate in Draw (ToPoseCoordinates).
        // text, lineWidths: the lines of the text, and their widths.
        // location, angle: the location of the text, and its angle in degrees.
        // fullWidth: the width of formatted text, or 0.
        // color: the palette color.
        // objectHorizAlignment, objectVertAlignment: the alignments of the text object.
        internal IReadOnlyList<ClippingObject> MakeClippingObjects(string[] text, float[] lineWidths, PointF location, float angle, float fullWidth, SymColor color,
                                                                   TextSymDefHorizAlignment objectHorizAlignment, TextSymDefVertAlignment objectVertAlignment)
        {
            TextSymDefHorizAlignment fontAlign = GetHorizAlignment(objectHorizAlignment);
            TextSymDefVertAlignment vertAlign = GetVertAlignment(objectVertAlignment);

            Debug.Assert(color != null);

            List<ClippingObject> clippingObjects = new List<ClippingObject>();
            if (fontColor != null && color != fontColor &&
                (framing.framingStyle == FramingStyle.None || color != framing.framingColor) &&
                (!underline.underlineOn || color != underline.underlineColor) &&
                (centerPointSymdef == null || !centerPointSymdef.HasColor(color)))
                return clippingObjects.AsReadOnly();

            EnsureFontMetrics();

            Underlining underliningFontEffect = new Underlining();
            if ((effects & TextEffects.Underline) != 0) {
                underliningFontEffect.underlineOn = true;
                underliningFontEffect.underlineColor = fontColor;
                underliningFontEffect.underlineDistance = FontEmHeight * fontEffectUnderlineDistance;
                underliningFontEffect.underlineWidth = FontEmHeight * fontEffectUnderlineWidth;
            }

            SinglePlacement placement = Placement.Identity.Within(new Pose(location, angle).ToMatrix());

            // The lines of the text, in the coordinates of Draw.
            PointF pt = new PointF(0F, 0F);
            float baselineOfLine = 0;          // y coordinate of baseline of line.
            bool firstLineOfPara = true, lastLineOfPara;

            pt.Y -= VertOffset(vertAlign, text);

            float topOfFirstLine = pt.Y;

            for (int lineIndex = 0; lineIndex < text.Length; ++lineIndex) {
                string line = text[lineIndex];
                float lineWidth = lineWidths[lineIndex];

                lastLineOfPara = (lineIndex == text.Length - 1 || text[lineIndex + 1] == ParagraphMark);        // are we on the last line of a paragraph?

                if (line == ParagraphMark) {
                    pt.Y += paraSpacing;
                    firstLineOfPara = true;
                }
                else {
                    float indent = 0;
                    float leftEdge;          // tabs are relative to this X position.
                    if (fontAlign == TextSymDefHorizAlignment.Right)
                        pt.X = leftEdge = -lineWidth;
                    else if (fontAlign == TextSymDefHorizAlignment.Center)
                        pt.X = leftEdge = -(lineWidth / 2F);
                    else {
                        leftEdge = 0;
                        pt.X = indent = firstLineOfPara ? firstIndent : restIndent;     // indents only used for left align or justified
                    }

                    // The size of the spaces. Justification adjusts it.
                    float sizeOfSpace = wordSpacing * textFaceMetrics.SpaceWidth;            // basic width of spaces as set by the symdef
                    if (fontAlign == TextSymDefHorizAlignment.Justified && !lastLineOfPara && fullWidth > 0)
                        sizeOfSpace += JustifyText(line, lineWidth, fullWidth - indent);

                    // The text segments of the line. A text segment is a word, or a grapheme where charSpacing > 0.
                    int index = 0;
                    for (; ; ) {
                        string textSegment;

                        if (charSpacing > 0)
                            textSegment = GetNextTextElement(line.Substring(index));
                        else
                            textSegment = GetNextTextSegment(line.Substring(index));
                        if (string.IsNullOrEmpty(textSegment))
                            break;

                        if (textSegment == " ")
                            pt.X += sizeOfSpace;
                        else if (textSegment == "\t")
                            pt.X += WidthOfTextSegment("\t", pt.X - leftEdge, fontAlign);
                        else {
                            AddStringWithEffects(clippingObjects, color, textSegment, pt, placement);
                            pt.X += MeasureStringWidth(textSegment, fontAlign);

                            if (charSpacing > 0)
                                pt.X += charSpacing * textFaceMetrics.SpaceWidth;
                        }

                        index += textSegment.Length;
                    }

                    baselineOfLine = pt.Y + FontAscent;        // Set the bottom of the text.

                    // The underlines. The underline of the font effect is a line, as Draw draws it, and not an effect of
                    // the font of the text runs.
                    AddUnderline(clippingObjects, underliningFontEffect, color, baselineOfLine, lineWidth, indent, fontAlign, placement);
                    if (lastLineOfPara)
                        AddUnderline(clippingObjects, this.underline, color, baselineOfLine, Math.Max(fullWidth, lineWidth), (fullWidth == 0) ? indent : 0, fontAlign, placement);

                    pt.Y += lineSpacing;
                    firstLineOfPara = false;
                }
            }

            AddCenterPoint(clippingObjects, lineWidths, fullWidth, color, topOfFirstLine, baselineOfLine, fontAlign, placement);

            if (underline.underlineOn)
                baselineOfLine += underline.underlineDistance + underline.underlineWidth;
            AddFramingRectangle(clippingObjects, lineWidths, fullWidth, color, topOfFirstLine, baselineOfLine, fontAlign, placement);

            return clippingObjects.AsReadOnly();
        }

        // The point of the coordinates of a pose of a text, whose Y axis points up as the axis of the map does, at a point
        // of the coordinates in which the drawing lays out the text with its Y axis down: the y coordinate is the opposite.
        // point: the point in the coordinates of the drawing.
        private static PointF ToPoseCoordinates(PointF point)
        {
            return new PointF(point.X, -point.Y);
        }

        // Add the clipping objects of a text run in a palette color, as DrawStringWithEffects draws them: the run in the
        // font color, and in the color of the framing, the run and its framing line, or the run moved by the shadow. The
        // framing line is one stroke of the outline of the run with the full width, two times the width of the framing,
        // and the line style of the framing. DrawStringWithEffects first strokes the outline with narrower pens, whose
        // lines are inside that one, and OCAD draws one line.
        // clippingObjects: the clipping objects, which get those of the run.
        // color: the palette color.
        // text: the text of the run.
        // pt: the top left corner of the run, in the coordinates of the drawing of the text.
        // placement: the placement of the text.
        private void AddStringWithEffects(List<ClippingObject> clippingObjects, SymColor color, string text, PointF pt, SinglePlacement placement)
        {
            Debug.Assert(color != null);

            if (fontColor != null && color == fontColor)
                AddTextRun(clippingObjects, text, pt, placement);

            if (framing.framingStyle != FramingStyle.None && framing.framingColor != null && color == framing.framingColor) {
                if (framing.framingStyle == FramingStyle.Line) {
                    AddTextRun(clippingObjects, text, pt, placement);
                    if (!string.IsNullOrEmpty(text))
                        clippingObjects.Add(new StrokedShape(MakeTextRun(text, pt), GraphicsUtil.MakeSolidStroke(framing.lineWidth * 2, framing.lineStyle), placement));
                }
                else if (framing.framingStyle == FramingStyle.Shadow) {
                    AddTextRun(clippingObjects, text, new PointF(pt.X + framing.shadowX, pt.Y - framing.shadowY), placement);
                }
            }
        }

        // Add a filled text run, as DrawSingleLineString draws it. A run without text covers nothing.
        // clippingObjects: the clipping objects, which get the run.
        // text: the text of the run.
        // upperLeft: the top left corner of the run, in the coordinates of the drawing of the text.
        // placement: the placement of the text.
        private void AddTextRun(List<ClippingObject> clippingObjects, string text, PointF upperLeft, SinglePlacement placement)
        {
            if (!string.IsNullOrEmpty(text))
                clippingObjects.Add(new FilledShape(MakeTextRun(text, upperLeft), AreaFillMode.Winding, placement));
        }

        // A text run in the font that CreateObjects creates, which has no underline effect: the drawing draws the
        // underlines as lines.
        // text: the text of the run.
        // upperLeft: the top left corner of the run, in the coordinates of the drawing of the text.
        private TextShape MakeTextRun(string text, PointF upperLeft)
        {
            return new TextShape(text, fontName, fontSize, effects & ~TextEffects.Underline, ToPoseCoordinates(upperLeft), map.TextMetricsProvider);
        }

        // Add the clipping object of an underline in a palette color, as DrawUnderline draws it: a line of the width of the
        // underline with mitered joins.
        // clippingObjects: the clipping objects, which get the underline.
        // underlining: the underline.
        // color: the palette color.
        // baseline: the y coordinate of the baseline of the line of text, in the coordinates of the drawing of the text.
        // width, indent: the width of the underline, and its indent where the text is aligned left.
        // objectHorizAlignment: the horizontal alignment of the text object.
        // placement: the placement of the text.
        private void AddUnderline(List<ClippingObject> clippingObjects, Underlining underlining, SymColor color, float baseline, float width, float indent,
                                  TextSymDefHorizAlignment objectHorizAlignment, SinglePlacement placement)
        {
            if (underlining.underlineOn && color != null && color == underlining.underlineColor) {
                TextSymDefHorizAlignment fontAlign = GetHorizAlignment(objectHorizAlignment);

                float l, r;
                if (fontAlign == TextSymDefHorizAlignment.Right)
                    l = -width;
                else if (fontAlign == TextSymDefHorizAlignment.Center)
                    l = -(width / 2F);
                else
                    l = indent;
                r = l + width;

                float y = baseline + underlining.underlineDistance + underlining.underlineWidth / 2;
                PointF[] points = { ToPoseCoordinates(new PointF(l, y)), ToPoseCoordinates(new PointF(r, y)) };
                clippingObjects.Add(new StrokedShape(PathShape.FromPoints(points, false), GraphicsUtil.MakeSolidStroke(underlining.underlineWidth, LineStyle.Mitered), placement));
            }
        }

        // Add the clipping objects of the center point of the text in a palette color, as DrawCenterPoint draws them: the
        // clipping objects of the glyph of the center point symbol at the angle 0, at the center point. OCAD has a
        // center point for unformatted text only (fullWidth == 0). The glyph is placed within the placement of the text and
        // a translation to the center point.
        // clippingObjects: the clipping objects, which get those of the center point.
        // lineWidths, fullWidth: the widths of the lines, and the width of formatted text, or 0.
        // color: the palette color.
        // topOfText, bottomOfText: the top of the text and the baseline of its last line, in the coordinates of the
        //   drawing of the text.
        // objectHorizAlignment: the horizontal alignment of the text object.
        // placement: the placement of the text.
        private void AddCenterPoint(List<ClippingObject> clippingObjects, float[] lineWidths, float fullWidth, SymColor color, float topOfText, float bottomOfText,
                                    TextSymDefHorizAlignment objectHorizAlignment, SinglePlacement placement)
        {
            TextSymDefHorizAlignment fontAlign = GetHorizAlignment(objectHorizAlignment);

            if (centerPointSymdef != null && centerPointSymdef.HasColor(color) && fullWidth == 0) {
                float l, r, t, b;

                fullWidth = CalcFullWidth(lineWidths, fullWidth, fontAlign);

                if (fontAlign == TextSymDefHorizAlignment.Right)
                    l = -fullWidth;
                else if (fontAlign == TextSymDefHorizAlignment.Center)
                    l = -(fullWidth / 2F);
                else
                    l = 0;
                r = l + fullWidth;
                t = topOfText - (FontAscent + FontDescent) + FontEmHeight;  // strange OCAD adjustment.
                b = bottomOfText + FontDescent;  // include the descender on the text.

                PointF centerPoint = ToPoseCoordinates(new PointF((r + l) / 2, (t + b) / 2));
                Matrix translation = new Matrix();
                translation.Translate(centerPoint.X, centerPoint.Y);
                clippingObjects.AddRange(centerPointSymdef.Glyph.MakeClippingObjects(new PointF(0, 0), 0, null, color, null, placement.Within(translation)));
            }
        }

        // Add the clipping object of the framing rectangle around the text in a palette color, as DrawFramingRectangle
        // fills it: the rectangle around the lines with the margins of the framing, filled.
        // clippingObjects: the clipping objects, which get the framing rectangle.
        // lineWidths, fullWidth: the widths of the lines, and the width of formatted text, or 0.
        // color: the palette color.
        // topOfText, bottomOfText: the top of the text and the bottom of its last line, in the coordinates of the drawing
        //   of the text.
        // objectHorizAlignment: the horizontal alignment of the text object.
        // placement: the placement of the text.
        private void AddFramingRectangle(List<ClippingObject> clippingObjects, float[] lineWidths, float fullWidth, SymColor color, float topOfText, float bottomOfText,
                                         TextSymDefHorizAlignment objectHorizAlignment, SinglePlacement placement)
        {
            if (framing.framingStyle == FramingStyle.Rectangle && color == framing.framingColor) {
                RectangleF textRect = CalcTextRectangle(lineWidths, fullWidth, topOfText, bottomOfText, objectHorizAlignment);

                float t = textRect.Top - framing.rectBorderTop;
                float b = textRect.Bottom + framing.rectBorderBottom;
                float l = textRect.Left - framing.rectBorderLeft;
                float r = textRect.Right + framing.rectBorderRight;

                PointF[] corners = Array.ConvertAll(PathShape.GetCorners(new RectangleF(l, t, r - l, b - t)), ToPoseCoordinates);
                clippingObjects.Add(new FilledShape(PathShape.FromPoints(corners, true), AreaFillMode.Winding, placement));
            }
        }

        public class InsertionPointLocation
        {
            public PointF Baseline, Ascent, Descent;
        }

        // Draw this text symbol at point pt with angle ang in this graphics (given color only). 
        internal InsertionPointLocation FindInsertionPoint(TextCoord textCoord, string[] text, float[] lineWidths, PointF location, float angle, float fullWidth,
                                                           TextSymDefHorizAlignment objectHorizAlignment, TextSymDefVertAlignment objectVertAlignment)
        {
            // WARNING: This function must be kept synchronized to changed to the Draw() function!
            TextSymDefHorizAlignment fontAlign = GetHorizAlignment(objectHorizAlignment);
            TextSymDefVertAlignment vertAlign = GetVertAlignment(objectVertAlignment);

            if (textCoord.Line < 0 || textCoord.Line >= text.Length || textCoord.Col < 0 || textCoord.Col > text[textCoord.Line].Length) {
                return null;
            }

            // Move location to draw at to the origin.
            Matrix matrix = new Matrix();
            matrix.Translate(location.X, location.Y);
            matrix.Scale(1, -1); // Reverse Y direction so text is correct way around.
            if (angle != 0)
                matrix.RotateAt(-angle, new PointF(0, 0));

            // Draw all the lines of text.
            PointF pt = new PointF(0F, 0F);
            bool firstLineOfPara = true, lastLineOfPara;

            pt.Y -= VertOffset(vertAlign, text);

            float topOfFirstLine = pt.Y;

            for (int lineIndex = 0; lineIndex < text.Length; ++lineIndex) {
                string line = text[lineIndex];
                float lineWidth = lineWidths[lineIndex];

                lastLineOfPara = (lineIndex == text.Length - 1 || text[lineIndex + 1] == ParagraphMark);        // are we on the last line of a paragraph?

                if (line == ParagraphMark) {
                    pt.Y += paraSpacing;
                    firstLineOfPara = true;
                }
                else {
                    if (lineIndex == textCoord.Line) {
                        float indent = 0;
                        float leftEdge;          // tabs are relative to this X position.
                        if (fontAlign == TextSymDefHorizAlignment.Right)
                            pt.X = leftEdge = -lineWidth;
                        else if (fontAlign == TextSymDefHorizAlignment.Center)
                            pt.X = leftEdge = -(lineWidth / 2F);
                        else {
                            leftEdge = 0;
                            pt.X = indent = firstLineOfPara ? firstIndent : restIndent;     // indents only used for left align or justified
                        }

                        // Get the size of spaces. Justification is done by adjusting this.
                        float sizeOfSpace = wordSpacing * textFaceMetrics.SpaceWidth;            // basic width of spaces as set by the symdef
                        if (fontAlign == TextSymDefHorizAlignment.Justified && !lastLineOfPara && fullWidth > 0)
                            sizeOfSpace += JustifyText(line, lineWidth, fullWidth - indent);

                        // Draw all the text segments in the line. (A text segment is a word, unless charSpacing>0, in which case it is graphemes).
                        int index = 0;
                        bool indexByGraphemes = charSpacing > 0;  // May change later.
                        for (; ; ) {
                            string textSegment;

                            if (index >= textCoord.Col) {
                                // Found the place!
                                InsertionPointLocation insertionPoint = new InsertionPointLocation();

                                // Find all three points along the insertion point, as transformed.
                                PointF insertPt = pt;
                                insertionPoint.Ascent = Geometry.TransformPoint(insertPt, matrix);
                                insertPt.Y += FontAscent;
                                insertionPoint.Baseline = Geometry.TransformPoint(insertPt, matrix);
                                insertPt.Y += FontDescent;
                                insertionPoint.Descent = Geometry.TransformPoint(insertPt, matrix);
                                return insertionPoint;
                            }

                            if (indexByGraphemes) {
                                textSegment = GetNextTextElement(line.Substring(index));
                            }
                            else {
                                textSegment = GetNextTextSegment(line.Substring(index));
                                if (!string.IsNullOrEmpty(textSegment) && textSegment.Length > textCoord.Col - index) {
                                    // The textCoord we are looking for in withing the next text segment. Start going by
                                    // graphemes instead.
                                    indexByGraphemes = true;
                                    textSegment = GetNextTextElement(line.Substring(index));
                                }
                            }

                            if (string.IsNullOrEmpty(textSegment))
                                break;

                            if (textSegment == " ")
                                pt.X += sizeOfSpace;
                            else if (textSegment == "\t")
                                pt.X += WidthOfTextSegment("\t", pt.X - leftEdge, fontAlign);
                            else {
                                pt.X += MeasureStringWidth(textSegment, fontAlign);

                                if (charSpacing > 0)
                                    pt.X += charSpacing * textFaceMetrics.SpaceWidth;
                            }

                            index += textSegment.Length;
                        }
                    }

                    pt.Y += lineSpacing;
                    firstLineOfPara = false;
                }
            }

            // Not sure how this happens!
            Debug.Fail("Couldn't find insertion point for this text coord.");
            return null;
        }

        
        // Draw this text symbol at point pt with angle ang in this graphics (given color only). 
        internal bool HitTest(string[] text, float[] lineWidths, PointF location, float angle, float fullWidth,
                              TextSymDefHorizAlignment objectHorizAlignment, TextSymDefVertAlignment objectVertAlignment, 
                              PointF pointTest, float distanceTest, out float actualDistance)
        {
            EnsureFontMetrics();

            TextSymDefHorizAlignment fontAlign = GetHorizAlignment(objectHorizAlignment);
            TextSymDefVertAlignment vertAlign = GetVertAlignment(objectVertAlignment);

            // Move location to draw at to the origin.
            Matrix matrix = new Matrix();
            matrix.Translate(location.X, location.Y);
            matrix.Scale(1, -1); // Reverse Y direction so text is correct way around.
            if (angle != 0)
                matrix.RotateAt(-angle, new PointF(0, 0));
            matrix.Invert();  // Invert to translate dest point to source coordinates.
            pointTest = matrix.Transform(pointTest);

            // Check all the lines of text.
            PointF pt = new PointF(0F, 0F);
            bool firstLineOfPara = true, lastLineOfPara;

            pt.Y -= VertOffset(vertAlign, text);

            float topOfFirstLine = pt.Y;

            float minDistanceFromText = float.MaxValue;

            // For each line of text, hit test against that line of text.

            for (int lineIndex = 0; lineIndex < text.Length; ++lineIndex) {
                string line = text[lineIndex];
                float lineWidth = lineWidths[lineIndex];

                lastLineOfPara = (lineIndex == text.Length - 1 || text[lineIndex + 1] == ParagraphMark);        // are we on the last line of a paragraph?

                if (line == ParagraphMark) {
                    pt.Y += paraSpacing;
                    firstLineOfPara = true;
                }
                else {
                    float indent = 0;
                    if (fontAlign == TextSymDefHorizAlignment.Right)
                        pt.X = -lineWidth;
                    else if (fontAlign == TextSymDefHorizAlignment.Center)
                        pt.X = -(lineWidth / 2F);
                    else {
                        pt.X = indent = firstLineOfPara ? firstIndent : restIndent;     // indents only used for left align or justified
                    }

                    // Adjust width for justification.
                    if (fontAlign == TextSymDefHorizAlignment.Justified && !lastLineOfPara && fullWidth > 0)
                        lineWidth = fullWidth - indent;

                    // Hit test against this line.
                    RectangleF textRectangle = new RectangleF(pt.X, pt.Y, lineWidth, lineSpacing);
                    float distanceFromText = Geometry.DistanceFromRectangle(textRectangle, pointTest);
                    if (distanceFromText < minDistanceFromText)
                        minDistanceFromText = distanceFromText;

                    pt.Y += lineSpacing;
                    firstLineOfPara = false;
                }
            }

            // Return minimum of distance from each line of text.
            actualDistance = minDistanceFromText;
            return (minDistanceFromText <= distanceTest);
        }

        // Calculate the additional width to add to spaces to justify a line of text. textWidth is the width of text without
        // adjustment. fullWidth is the size we want the text to be.
        private float JustifyText(string text, float textWidth, float fullWidth)
        {
            int spaceCount = 0;
            for (int i = 0; i < text.Length; ++i)
                if (text[i] == ' ')
                    ++spaceCount;

            if (spaceCount > 0)
                return (fullWidth - textWidth) / spaceCount;
            else
                return 0;
        }

        internal float TextHeight(string[] text)
        {
            // count number of lines, number of new paragraphs.
            int lineCount, newParaCount = 0;
            for (int i = 0; i < text.Length; ++i)
                if (text[i] == ParagraphMark)
                    ++newParaCount;
            lineCount = text.Length - newParaCount;

            // Calculate height of text.
            return (FontAscent + FontDescent + ((lineCount - 1) * lineSpacing) + (newParaCount * paraSpacing));
        }

        // Calculate the bounding box. 
        public RectangleF CalcBounds(string[] text, float[] lineWidths, PointF location, float angle, float fullWidth,
                                     TextSymDefHorizAlignment objectHorizAlignment, TextSymDefVertAlignment objectVertAlignment,
                                     out SizeF size)
        {
            EnsureFontMetrics();

            TextSymDefHorizAlignment fontAlign = GetHorizAlignment(objectHorizAlignment);
            TextSymDefVertAlignment vertAlign = GetVertAlignment(objectVertAlignment);

            // Calculate height of text.
            float height = TextHeight(text);

            // Calculate full width of text.
            fullWidth = CalcFullWidth(lineWidths, fullWidth, fontAlign);

            // Get the size.
            size = new SizeF(fullWidth, height);

            // The rectangle, unrotated.
            RectangleF rect;
            if (fontAlign == TextSymDefHorizAlignment.Left || fontAlign == TextSymDefHorizAlignment.Justified)
                rect = new RectangleF(location.X, location.Y - size.Height, size.Width, size.Height);  // indents only used for left aligned and justified text.
            else if (fontAlign == TextSymDefHorizAlignment.Right)
                rect = new RectangleF(location.X - size.Width, location.Y - size.Height, size.Width, size.Height);
            else {
                Debug.Assert(fontAlign == TextSymDefHorizAlignment.Center);
                rect = new RectangleF(location.X - size.Width / 2, location.Y - size.Height, size.Width, size.Height);
            }

            // Expand for framing and underlining.
            if (underline.underlineOn)
                rect = RectangleF.FromLTRB(rect.Left, rect.Top - (underline.underlineDistance + underline.underlineWidth), rect.Right, rect.Bottom);

            if (framing.framingStyle == FramingStyle.Line)
                rect.Inflate(framing.lineWidth, framing.lineWidth);
            else if (framing.framingStyle == FramingStyle.Shadow) {
                RectangleF shadow = rect;
                shadow.Offset(framing.shadowX, framing.shadowY);
                rect = RectangleF.Union(rect, shadow);
            }
            else if (framing.framingStyle == FramingStyle.Rectangle) 
                rect = RectangleF.FromLTRB(rect.Left - framing.rectBorderLeft, rect.Top - framing.rectBorderBottom, rect.Right + framing.rectBorderRight, rect.Bottom + framing.rectBorderTop);

            // Adjust for vertical alignment
            rect.Offset(0, VertOffset(vertAlign, text));

            // Rotate the rectangle.
            if (angle != 0)
                rect = Geometry.BoundsOfRotatedRectangle(rect, location, angle);

            return rect;
        }

        internal SymPath GetHighlightPath(string[] text, float[] lineWidths, PointF location,
                                          TextSymDefHorizAlignment objectHorizAlignment, TextSymDefVertAlignment objectVertAlignment,
                                          float rotation, float width)
        {
            EnsureFontMetrics();

            TextSymDefHorizAlignment fontAlign = GetHorizAlignment(objectHorizAlignment);
            TextSymDefVertAlignment vertAlign = GetVertAlignment(objectVertAlignment);

            // count number of lines, number of new paragraphs.
            int lineCount, newParaCount = 0;
            for (int i = 0; i < text.Length; ++i)
                if (text[i] == ParagraphMark)
                    ++newParaCount;
            lineCount = text.Length - newParaCount;

            // Calculate height of text.
            float height = FontAscent + FontDescent + ((lineCount - 1) * lineSpacing) + (newParaCount * paraSpacing);

            // Calculate full width of text.
            float fullWidth = CalcFullWidth(lineWidths, width, fontAlign);

            SizeF size = new SizeF(fullWidth, height);

            // The rectangle, unrotated. First adjust horizonal alignment.
            RectangleF rect;
            if (fontAlign == TextSymDefHorizAlignment.Left || fontAlign == TextSymDefHorizAlignment.Justified)
                rect = new RectangleF(location.X, location.Y - size.Height, size.Width, size.Height);  // indents only used for left aligned and justified text.
            else if (fontAlign == TextSymDefHorizAlignment.Right)
                rect = new RectangleF(location.X - size.Width, location.Y - size.Height, size.Width, size.Height);
            else {
                Debug.Assert(fontAlign == TextSymDefHorizAlignment.Center);
                rect = new RectangleF(location.X - size.Width / 2, location.Y - size.Height, size.Width, size.Height);
            }

            // Adjust for vertical alignment
            rect.Offset(0, VertOffset(vertAlign, text));

            // Create path from rectangle.
            SymPath path = SymPath.CreateRectanglePath(rect);

            // Rotate the rectangle.
            if (rotation != 0) {
                Matrix matrix = new Matrix();
                matrix.RotateAt(rotation, location);  // Correct for centered/right aligned text?
                path = path.Transform(matrix);
            }

            return path;
        }

        // Breaks unwrapped text into paragraphs. This just means adding a paragraph mark
        // between each line. The lineWidths array has the width of each line.
        //
        // Also retursn a TextCoordMapper object to remember the mapping.
        public string[] BreakUnwrappedLines(string[] text, TextSymDefHorizAlignment objectHorizAlignment, out TextCoordMapper coordMapper, out float[] lineWidths)
        {
            return BreakUnwrappedLines(GetWrapProperties(objectHorizAlignment), text, out coordMapper, out lineWidths);
        }

        // Breaks unwrapped text into paragraphs. This just means adding a paragraph mark
        // between each line. The lineWidths array has the width of each line.
        //
        // Also retursn a TextCoordMapper object to remember the mapping.
        internal static string[] BreakUnwrappedLines(WrapTextProperties wrapProperties, string[] text, out TextCoordMapper coordMapper, out float[] lineWidths)
        {
            coordMapper = new TextCoordMapper();

            int firstLine = 0;

            if (text.Length == firstLine) {
                lineWidths = new float[0];
                return new string[0];
            }

            if (wrapProperties.AddParagraphMarks) {
                string[] newLines = new string[(text.Length - firstLine) * 2 - 1];
                lineWidths = new float[newLines.Length];

                for (int i = 0; i < text.Length - firstLine; ++i) {
                    newLines[i * 2] = text[i + firstLine];
                    lineWidths[i * 2] = LineWidth(wrapProperties, text[i + firstLine]);
                    coordMapper.AddUnwrappedCoord(new TextCoord(i + firstLine, 0));

                    if (i + firstLine != text.Length - 1) {
                        newLines[i * 2 + 1] = ParagraphMark;
                        lineWidths[i * 2 + 1] = 0;
                        coordMapper.AddUnwrappedCoord(new TextCoord(-1, -1));  // Paragraph marks don't correspond to any unwrapped text.
                    }
                }

                return newLines;
            }
            else {
                string[] newLines = new string[(text.Length - firstLine)];
                lineWidths = new float[newLines.Length];

                for (int i = 0; i < text.Length - firstLine; ++i) {
                    newLines[i] = text[i + firstLine];
                    lineWidths[i] = LineWidth(wrapProperties, text[i + firstLine]);
                    coordMapper.AddUnwrappedCoord(new TextCoord(i + firstLine, 0));
                }

                return newLines;
            }
        }

        // Breaks the text into lines based on the given width. Returns a new string array with
        // line breaks made into it. A line break that already exists is turned into a paragraph break,
        // which is a line with just a unicode paragraph separator in it. The line widths array has the 
        // width of each line.
        internal string[] BreakLines(string[] text, float width, TextSymDefHorizAlignment objectHorizAlignment, out TextCoordMapper coordMapper, out float[] lineWidths, out bool didWrapAtLeastOneLine)
        {
            return BreakLines(GetWrapProperties(objectHorizAlignment), text, width, out coordMapper, out lineWidths, out didWrapAtLeastOneLine);
        }
        
        // Breaks the text into lines based on the given width. Returns a new string array with
        // line breaks made into it. A line break that already exists is turned into a paragraph break,
        // which is a line with just a unicode paragraph separator in it. (This can be turned off in WrapTextProperties).
        // The line widths array has the width of each line.
        //
        // Also retursn a TextCoordMapper object to remember the mapping.
        internal static string[] BreakLines(WrapTextProperties wrapProperties, string[] text, float width, out TextCoordMapper coordMapper, out float[] lineWidths, out bool didWrapAtLeastOneLine)
        {
            List<String> lineList = new List<String>();
            List<float> widthList = new List<float>();
            coordMapper = new TextCoordMapper();
            didWrapAtLeastOneLine = false;

            float widthFirstLine, widthRemainingLines;
            if (wrapProperties.FontAlign == TextSymDefHorizAlignment.Left || wrapProperties.FontAlign == TextSymDefHorizAlignment.Justified) {
                widthFirstLine = Math.Max(0F, width - wrapProperties.FirstIndent);
                widthRemainingLines = Math.Max(0F, width - wrapProperties.RestIndent);
            }
            else {
                // indents only used for left-aligned and justified text.
                widthFirstLine = widthRemainingLines = width;
            }

            int firstLine = 0;

            for (int i = firstLine; i < text.Length; ++i) {
                TextCoordMapper coordMapperForThisLine;

                int nLines = WrapParagraph(wrapProperties, text[i], widthFirstLine, widthRemainingLines, lineList, widthList, out coordMapperForThisLine);
                if (nLines > 1)
                    didWrapAtLeastOneLine = true;

                coordMapper.AddCoordMapper(i, coordMapperForThisLine);
                
                if (i < text.Length - 1 && wrapProperties.AddParagraphMarks) {
                    lineList.Add(ParagraphMark);
                    widthList.Add(0);
                    coordMapper.AddUnwrappedCoord(new TextCoord(-1, -1));  // Paragraph marks don't correspond to any unwrapped line.
                }
            }

            lineWidths = widthList.ToArray();
            return lineList.ToArray();
        }

        // Split text into lines of length width or less, and add those lines to the given ArrayList (and widths of the lines to the width list).
        // Return number of lines the paragraphs was wrapped onto. 
        // Creates a TextCoordMapper object to remember how the text was wrapped.
        private static int WrapParagraph(WrapTextProperties wrapProperties, string text, float widthFirstLine, float widthRemainingLines, List<String> lineList, List<float> widthList, out TextCoordMapper coordMapper)
        {
            int unwrappedCol = 0;
            int translateLine = 0;
            coordMapper = new TextCoordMapper();
            
            bool firstLine = true;
            while (text != null) {
                float lineWidth;
                int textLengthBeforeWrap = text.Length;

                string line = WrapOneLine(wrapProperties, ref text, firstLine ? widthFirstLine : widthRemainingLines, out lineWidth);
                lineList.Add(line);
                widthList.Add(lineWidth);
                firstLine = false;
                coordMapper.AddUnwrappedCoord(new TextCoord(0, unwrappedCol));

                unwrappedCol += (textLengthBeforeWrap - ((text == null) ? 0 : text.Length));
                translateLine += 1;
            }

            return translateLine;
        }

        // Figure out how much of the line will fit and return that. line is modified
        // to be the remaining text to fit on subsequent lines, or null if nothing left. The amount of width
        // actually consumed is returned in actualLineWidth.
        private static string WrapOneLine(WrapTextProperties wrapProperties, ref string line, float lineWidth, out float actualLineWidth)
        {
            StringBuilder lineSoFar = new StringBuilder();
            float widthUsed = 0F;
            bool useSingleLetters = false;

            while (!string.IsNullOrEmpty(line)) {
                // Get next segment of text to add.
                string nextSegment;
                if (useSingleLetters)
                    nextSegment = GetNextTextElement(line);
                else
                    nextSegment = GetNextTextSegment(line);
                if (string.IsNullOrEmpty(nextSegment))
                    break;

                // See if this segment will fit on the line.
                float segmentWidth = WidthOfTextSegment(wrapProperties, nextSegment, widthUsed);
                if (segmentWidth + widthUsed > lineWidth) {
                    // The segment won't fit. If we haven't placed any segments yet, we need to try placing single letters.
                    if (widthUsed == 0) {
                        if (!useSingleLetters) {
                            useSingleLetters = true;
                            continue;
                        }
                    }
                    else
                        break;  // we're done.
                }

                // Add nextSegment to the line, and remove from the line under consideration.
                lineSoFar.Append(nextSegment);
                line = line.Substring(nextSegment.Length);
                widthUsed += segmentWidth;
            }

            // If we're wrapping, the new line replaces spaces.
            if (line.Length > 0) {
                // Remove trailing spaces from current line.
                while (lineSoFar.Length > 0 && lineSoFar[lineSoFar.Length - 1] == ' ') {
                    lineSoFar.Remove(lineSoFar.Length - 1, 1);
                    widthUsed -= WidthOfTextSegment(wrapProperties, " ", widthUsed);
                }

                // Remove initial spaces from next line.
                int startNextLine = 0;
                while (startNextLine < line.Length && line[startNextLine] == ' ')
                    ++startNextLine;
                if (startNextLine > 0)
                    line = line.Substring(startNextLine);
            }

            if (line == "")
                line = null;

            actualLineWidth = widthUsed;
            return lineSoFar.ToString();
        }

        // Calculate width of one line, not limited by wrapping.
        private float LineWidth(string text, TextSymDefHorizAlignment objectHorizAlignment)
        {
            return LineWidth(GetWrapProperties(objectHorizAlignment), text);
        }
            
        // Calculate width of one line, not limited by wrapping.
        private static float LineWidth(WrapTextProperties wrapProperties, string text)
        {
            string nextSegment;
            float width = 0;

            while ((nextSegment = GetNextTextSegment(text)) != null) {
                width += WidthOfTextSegment(wrapProperties, nextSegment, width);
                text = text.Substring(nextSegment.Length);
            }

            return width;
        }

        // Get the next text segment needed. If empty, returns null.
        // Is the next is a space or tab, that is it.
        // Otherwise, the whole word to the next space or tab.
        private static string GetNextTextSegment(string text)
        {
            if (String.IsNullOrEmpty(text))
                return null;

            if (text[0] == ' ')
                return " ";
            if (text[0] == '\t')
                return "\t";

            int i = 0;
            while (i < text.Length && text[i] != ' ' && text[i] != '\t')
                ++i;

            return text.Substring(0, i);
        }

                // Get the width of a text segment. Handles tabs, spaces between characters, and space widths.
        private float WidthOfTextSegment(string text, float widthSoFar, TextSymDefHorizAlignment objectHorizAlignment)
        {
            return WidthOfTextSegment(GetWrapProperties(objectHorizAlignment), text, widthSoFar);
        }

        // Get the width of a text segment. Handles tabs, spaces between characters, and space widths.
        private static float WidthOfTextSegment(WrapTextProperties wrapProperties, string text, float widthSoFar)
        {
            if (text == " ") {
                return wrapProperties.TextFaceMetrics.SpaceWidth * wrapProperties.WordSpacing;
            }
            else if (text == "\t") {
                float[] tabs = wrapProperties.Tabs;

                if (tabs == null || tabs.Length == 0)
                    return 0;          // no tabs.
                else {
                    // Find first tab stop beyond the current location.
                    for (int i = 0; i < tabs.Length; ++i) {
                        if (tabs[i] > widthSoFar)
                            return tabs[i] - widthSoFar;
                    }

                    // Find repeating tabs and first tab stop beyond current location.
                    float tabStop, tabInc;
                    if (tabs.Length > 2 && tabs[tabs.Length - 1] > tabs[tabs.Length - 2]) {
                        tabStop = tabs[tabs.Length - 1];
                        tabInc = tabs[tabs.Length - 1] - tabs[tabs.Length - 2];
                    }
                    else {
                        tabStop = tabInc = tabs[tabs.Length - 1];
                    }

                    for (; ; ) {
                        if (tabStop > widthSoFar)
                            return tabStop - widthSoFar;
                        tabStop += tabInc;
                    }
                }
            }
            else if (wrapProperties.CharSpacing > 0) {
                float width = 0;
                TextElementEnumerator enumTextElements = StringInfo.GetTextElementEnumerator(text);
                while (enumTextElements.MoveNext()) 
                    width += MeasureStringWidth(wrapProperties, enumTextElements.GetTextElement()) + 
                             (wrapProperties.CharSpacing * wrapProperties.TextFaceMetrics.SpaceWidth);
                return width;
            }
            else {
                return MeasureStringWidth(wrapProperties, text);
            }
        }




        // The remaining functions all have to do with text along a path ("line text").

        // Structure to show where one character (which might be multiple Unicode codepoints), hence Grapheme, is placed
        // when laying out line text.
        struct GraphemePlacement
        {
            public string grapheme;       // The grapheme to draw.
            public int indexInText;       // Index of the grapheme in the text.
            public float width;                // The width of this grapheme.
            public PointF pointStart;      // The location the grapheme starts at.
            public float angle;                // The angle of the grapheme.

            public GraphemePlacement(string grapheme, int indexInText, float width, PointF pointStart, float angle)
            {
                this.grapheme = grapheme;
                this.indexInText = indexInText;
                this.width = width;
                this.pointStart = pointStart;
                this.angle = angle;
            }
        }

        // Draw this text symbol along a path.
        internal void DrawTextOnPath(IGraphicsTarget g, SymPath path, string text, SymColor color,
                                     TextSymDefHorizAlignment objectHorizAlignment, TextSymDefVertAlignment objectVertAlignment,
                                     RenderOptions renderOpts)
        {
            TextSymDefHorizAlignment fontAlign = GetHorizAlignment(objectHorizAlignment);
            TextSymDefVertAlignment vertAlign = GetVertAlignment(objectVertAlignment);

            if (color == null)
                return;
            if (color != fontColor && (framing.framingStyle == FramingStyle.None || color != framing.framingColor))
                return;

            CreateObjects(g);

            // Get the location of each grapheme to print.
            List<GraphemePlacement> graphemeList = GetLineTextGraphemePlacement(path, text, fontAlign);

            // Get location to draw to, relative to the line we're drawing along.
            PointF topAscentPoint = new PointF(0, -LineTextVertOffset(vertAlign));

            foreach (GraphemePlacement grapheme in graphemeList) {
                // Move location to draw at to the origin, set angle for drawing text.
                Matrix matrix = new Matrix();
                matrix.Translate(grapheme.pointStart.X, grapheme.pointStart.Y);
                matrix.Scale(1, -1);      // Reverse Y so text is correct way aroun
                matrix.RotateAt(-grapheme.angle, new PointF(0,0));
                g.PushTransform(matrix);

                try
                {
                    DrawStringWithEffects(g, color, grapheme.grapheme, topAscentPoint);
                }
                finally {
                    g.PopTransform();  // restore transform
                }
            }
        }

        // The geometry method of a text along a line: the clipping objects that DrawTextOnPath draws in a palette color,
        // in the same order. Each grapheme is a text run, in the font color, and in the color of the framing with its
        // framing line or its shadow, at a single placement of its own. Its pose is the start point of the grapheme on the
        // path, and the direction from that point to the end point of the grapheme, where the OCAD metric places the
        // graphemes along the path (GetLineTextGraphemePlacement). The top left corner of the run is in the coordinates of
        // the pose, as for a text symbol. A text along a line has no underline, center point or framing rectangle.
        // path: the path.
        // text: the text.
        // color: the palette color.
        // objectHorizAlignment, objectVertAlignment: the alignments of the text object.
        internal IReadOnlyList<ClippingObject> MakeClippingObjectsOfTextOnPath(SymPath path, string text, SymColor color,
                                                                              TextSymDefHorizAlignment objectHorizAlignment, TextSymDefVertAlignment objectVertAlignment)
        {
            TextSymDefHorizAlignment fontAlign = GetHorizAlignment(objectHorizAlignment);
            TextSymDefVertAlignment vertAlign = GetVertAlignment(objectVertAlignment);

            List<ClippingObject> clippingObjects = new List<ClippingObject>();
            if (color == null)
                return clippingObjects.AsReadOnly();
            if (color != fontColor && (framing.framingStyle == FramingStyle.None || color != framing.framingColor))
                return clippingObjects.AsReadOnly();

            EnsureFontMetrics();

            List<GraphemePlacement> graphemeList = GetLineTextGraphemePlacement(path, text, fontAlign);

            // The top left corner of the run, relative to the line, in the coordinates of the drawing of the text.
            PointF topAscentPoint = new PointF(0, -LineTextVertOffset(vertAlign));

            foreach (GraphemePlacement grapheme in graphemeList) {
                SinglePlacement placement = Placement.Identity.Within(new Pose(grapheme.pointStart, grapheme.angle).ToMatrix());
                AddStringWithEffects(clippingObjects, color, grapheme.grapheme, topAscentPoint, placement);
            }

            return clippingObjects.AsReadOnly();
        }

        internal InsertionPointLocation FindInsertionPointOnPath(SymPath path, string text, int textIndex,
                                        TextSymDefHorizAlignment objectHorizAlignment, TextSymDefVertAlignment objectVertAlignment)
        {
            TextSymDefHorizAlignment fontAlign = GetHorizAlignment(objectHorizAlignment);
            TextSymDefVertAlignment vertAlign = GetVertAlignment(objectVertAlignment);

            // Get the location of each grapheme to print.
            List<GraphemePlacement> graphemeList = GetLineTextGraphemePlacement(path, text, fontAlign);

            // Add one more zero-width space at the end to handle the last character.
            if (graphemeList.Count > 0) {
                GraphemePlacement lastGrapheme = graphemeList[graphemeList.Count - 1];
                graphemeList.Add(new GraphemePlacement(" ", text.Length, 0, Geometry.MoveDistance(lastGrapheme.pointStart, lastGrapheme.width, lastGrapheme.angle), lastGrapheme.angle));
            }
            else {
                PointF nearPoint = path.PointAtLength(0.01F, map.MapDistanceMetric);
                graphemeList.Add(new GraphemePlacement(" ", text.Length, 0, path.FirstPoint, (float) (Math.Atan2(nearPoint.Y - path.FirstPoint.Y, nearPoint.X - path.FirstPoint.X) * 180 / Math.PI)));
            }

            // Get location to draw to, relative to the line we're drawing along.
            PointF topAscentPoint;
            topAscentPoint = new PointF(0, -LineTextVertOffset(vertAlign));

            for (int i = 0; i < graphemeList.Count; ++i) {
                GraphemePlacement grapheme = graphemeList[i];

                // If we are at the desired location, or at the last grapheme (our zero-width space), use this one.
                // The text can be truncated, so we can easily go off the end.
                if ((textIndex >= grapheme.indexInText && textIndex < grapheme.indexInText + grapheme.grapheme.Length) ||
                    i == graphemeList.Count - 1) 
                {
                    // Move location to draw at to the origin, set angle for drawing text.
                    Matrix matrix = new Matrix();
                    matrix.Translate(grapheme.pointStart.X, grapheme.pointStart.Y);
                    matrix.Scale(1, -1);      // Reverse Y so text is correct way aroun
                    matrix.RotateAt(-grapheme.angle, new PointF(0, 0));

                    // Found the place!
                    InsertionPointLocation insertionPoint = new InsertionPointLocation();

                    // Find all three points along the insertion point, as transformed.
                    PointF insertPt = topAscentPoint;
                    insertionPoint.Ascent = Geometry.TransformPoint(insertPt, matrix);
                    insertPt.Y += FontAscent;
                    insertionPoint.Baseline = Geometry.TransformPoint(insertPt, matrix);
                    insertPt.Y += FontDescent;
                    insertionPoint.Descent = Geometry.TransformPoint(insertPt, matrix);
                    return insertionPoint;
                }
            }

            return null;
        }


        // Draw this text symbol along a path.
        internal bool HitTestTextOnPath(SymPath path, string text, PointF pointTest, float distanceTest,
                                        TextSymDefHorizAlignment objectHorizAlignment, TextSymDefVertAlignment objectVertAlignment,
                                        out float actualDistance)
        {
            EnsureFontMetrics();

            TextSymDefHorizAlignment fontAlign = GetHorizAlignment(objectHorizAlignment);
            TextSymDefVertAlignment vertAlign = GetVertAlignment(objectVertAlignment);

            // Much quicker test to rule out points -- test distance from the path.
            float distanceFromPath = path.DistanceFromPoint(pointTest);
            if (distanceFromPath > distanceTest + WHeight + FontDescent) {
                actualDistance = float.MaxValue;
                return false;
            }

            // Get the location of each grapheme to print.
            List<GraphemePlacement> graphemeList = GetLineTextGraphemePlacement(path, text, fontAlign);

            // Get location to draw to, relative to the line we're drawing along.
            PointF topAscentPoint = new PointF(0, -LineTextVertOffset(vertAlign));
            topAscentPoint.Y -= (WHeight - FontAscent);

            float minDistanceFromText = float.MaxValue;

            foreach (GraphemePlacement grapheme in graphemeList) {
                // Move location to draw at to the origin, set angle for drawing text.
                Matrix matrix = new Matrix();
                matrix.Translate(grapheme.pointStart.X, grapheme.pointStart.Y);
                matrix.Scale(1, -1);      // Reverse Y so text is correct way aroun
                matrix.RotateAt(-grapheme.angle, new PointF(0, 0));
                matrix.Invert();  // Because we're transforming a test point backward.
                PointF transformedTestPoint = matrix.Transform(pointTest);

                RectangleF textRectangle = new RectangleF(topAscentPoint.X, topAscentPoint.Y, grapheme.width, WHeight + FontDescent);
                float distanceFromText = Geometry.DistanceFromRectangle(textRectangle, transformedTestPoint);
                if (distanceFromText < minDistanceFromText)
                    minDistanceFromText = distanceFromText;
            }

            // Return minimum of distance from each line of text.
            actualDistance = minDistanceFromText;
            return (minDistanceFromText <= distanceTest);
        }


        // Calculate bounds of text along a path with this symbol.
        internal RectangleF CalcBounds(SymPath path, string text)
        {
            // This doesn't take into account the text at all -- just the line. It is probably good enought. We could do better 
            // with a bunch of work by calling GetLineTextGraphemePlacement() and unioning the bounds of each grapheme.
            // But not that necessary.

            RectangleF rect = path.BoundingBox;
            rect.Inflate(FontAscent + FontDescent, FontAscent + FontDescent);

            if (framing.framingStyle == FramingStyle.Line)
                rect.Inflate(framing.lineWidth, framing.lineWidth);
            else if (framing.framingStyle == FramingStyle.Shadow)
                rect.Inflate(Math.Max(framing.shadowX, framing.shadowY), Math.Max(framing.shadowX, framing.shadowY));

            return rect;
        }

        // Determine where each grapheme (essentially character) will go when drawing text along a path.
        // The algorithm for how text is laid out along a path makes sense, with one MAJOR weirdness.
        // The basic algorithm is that the width of each character to draw is determined. Starting where the last
        // character ends, the path is followed for that character width (around corners if needed), and the point along 
        // the path at that point is connected to the start point of the character with a line. The character is drawn along that
        // baseline, starting at the start point. The end point is used as the start of the next character. If there are bends, 
        // of course, the character won't end right at that end point, but we ignore that.
        //
        // THe weirdness is this: instead of measuring distance along the path correctly with the Pythagorian formula, a
        // strange alternate metric of dx + 1/2 dy is used, where dx is the larger ordinate delta and dy is the smaller ordinate
        // delta. Thus, text along diagonals is squished together more than it should be. I have no explanation as to why
        // this might work but it reproduces what OCAD does. See the function "BizzarroDistance" in SymPath.
        private List<GraphemePlacement> GetLineTextGraphemePlacement(SymPath path, string text, TextSymDefHorizAlignment objectHorizAlignment)
        {
            TextSymDefHorizAlignment fontAlign = GetHorizAlignment(objectHorizAlignment);
            float totalWidth = 0;
            List<GraphemePlacement> graphemeList = new List<GraphemePlacement>();
            float pathLength = path.BizzarroLength;
            if (pathLength == 0)
                return graphemeList;            // nothing to draw.

            // First, determine all the graphemes and their width
            if (!string.IsNullOrEmpty(text)) {
                TextElementEnumerator enumerator = StringInfo.GetTextElementEnumerator(text);
                while (enumerator.MoveNext()) {
                    string grapheme = enumerator.GetTextElement();
                    float graphemeWidth;

                    if (grapheme == " ")
                        graphemeWidth = wordSpacing * textFaceMetrics.SpaceWidth;
                    else {
                        float width = MeasureStringWidth(grapheme, fontAlign);
                        graphemeWidth = width + charSpacing * textFaceMetrics.SpaceWidth;
                    }

                    graphemeList.Add(new GraphemePlacement(grapheme, enumerator.ElementIndex, graphemeWidth, new PointF(), 0));
                    totalWidth += graphemeWidth;
                    if (totalWidth + 0.01F >= pathLength && fontAlign != TextSymDefHorizAlignment.Justified)
                        break;          // We don't have any room for more characters. (0.01 prevents a very small tail at the end.)
                }
            }

            // For OCAD compatibility, truncate right aligned text if too big to fit so the whole
            // string fits. (Note that left-aligned text will typically show one more character than this.)
            if (pathLength < totalWidth && fontAlign != TextSymDefHorizAlignment.Left && fontAlign != TextSymDefHorizAlignment.Justified) {
                totalWidth -= graphemeList[graphemeList.Count - 1].width;
                if (fontAlign == TextSymDefHorizAlignment.Right)
                    graphemeList.RemoveAt(graphemeList.Count - 1);
            }

            // Where does the text begin?
            float startingDistance = 0;
            if (fontAlign == TextSymDefHorizAlignment.Left || fontAlign == TextSymDefHorizAlignment.Justified)
                startingDistance = 0;
            else if (fontAlign == TextSymDefHorizAlignment.Right)
                startingDistance = pathLength - totalWidth;
            else if (fontAlign == TextSymDefHorizAlignment.Center)
                startingDistance = (pathLength - totalWidth) / 2;

            // For justified (all-line) text, adjust the widths of each character so they all fit.
            if (fontAlign == TextSymDefHorizAlignment.Justified && graphemeList.Count > 1) {
                if (charSpacing > 0) {
                    // last character doesn't have space added.
                    GraphemePlacement graphemePlacement = graphemeList[graphemeList.Count - 1];
                    graphemePlacement.width -= charSpacing * textFaceMetrics.SpaceWidth;
                    totalWidth -= charSpacing * textFaceMetrics.SpaceWidth;
                    graphemeList[graphemeList.Count - 1] = graphemePlacement;
                }

                float adjustment = (pathLength - totalWidth) / (graphemeList.Count - 1);
                for (int i = 0; i < graphemeList.Count - 1; ++i) {
                    GraphemePlacement graphemePlacement = graphemeList[i];
                    graphemePlacement.width += adjustment;
                    graphemeList[i] = graphemePlacement;
                }
            }

            // Find points along the path that are the start/end of each grapheme.
            PointF[] points = new PointF[graphemeList.Count + 1];
            float curDistance = startingDistance;
            for (int i = 0; i < graphemeList.Count; ++i) {
                points[i] = path.PointAtLengthBizzarro(curDistance);
                curDistance += graphemeList[i].width;
            }
            points[graphemeList.Count] = path.PointAtLengthBizzarro(Math.Min(curDistance, pathLength));

            // Fill in graphemeList with start points and angles
            for (int i = 0; i < graphemeList.Count; ++i) {
                GraphemePlacement graphemePlacement = graphemeList[i];
                graphemePlacement.pointStart = points[i];
                float distX = points[i + 1].X - points[i].X;
                float distY = points[i + 1].Y - points[i].Y;
                graphemePlacement.angle = (float) (Math.Atan2(distY, distX) * 360.0 / (Math.PI * 2));
                graphemeList[i] = graphemePlacement;
            }

            return graphemeList;
        }

        string[] emptyText = new string[1] { " " };

        // OCAD aligns text vertically a little differently than the map rendering. This function calculated
        // the OCAD vertical adjustment amount, for formatted or unformatted text.
        internal float GetOcadTopAdjustment(bool formatted, int version) {
            float topAdjust = 0;

            if (formatted) {
                // OCAD adds an extra internal leading (incorrectly).
                topAdjust = this.FontEmHeight - (this.FontAscent + this.FontDescent);

                // OCAD always aligns formatted text by the top, no matter when the alignment is.
                topAdjust -= VertOffset(this.VertAlignment, emptyText);
            }
            else {
                if (version >= 10) {
                    // OCAD version 10 and up support vertical alignment for unformatted text.
                    if (this.VertAlignment == TextSymDefVertAlignment.TopAscent) {
                        topAdjust = (this.FontAscent - this.WHeight);
                    }
                    else if (this.VertAlignment == TextSymDefVertAlignment.Midpoint) {
                        topAdjust = (this.FontAscent - this.WHeight) / 2;
                    }
                }
                else {
                    // OCAD version 9 and below do not support vertical alignment, but always align by baseline.
                    topAdjust = this.FontAscent - VertOffset(this.VertAlignment, emptyText);
                }
            }

            return topAdjust;
        }

        // OCAD alway stores location of formatted text by left edge, so this adjusts it to the correct
        // horizontal location.
        internal float GetOcadFormattedHorizAdjustment(float width)
        {
            if (this.FontAlignment == TextSymDefHorizAlignment.Right)
                return width;
            else if (this.FontAlignment == TextSymDefHorizAlignment.Center)
                return width / 2;
            else
                return 0;
        }
    }

    // The HoleSymDef is a dummy symdef used for holes -- There is a singleton HoleSymDef that
    // is used as the symdef for all of these objects, if there are any. It is never added to a map.
    public class HoleSymDef: SymDef
    {
        private HoleSymDef()
            : base("Hole", "hole")
        {
        }

        public static readonly HoleSymDef Singleton = new HoleSymDef();

        public override SymDef CopyToMap(Map map)
        {
            return this;
        }

        // Does this symbol definition draw the given color. Used to determine
        // if this symbol definition draws into the current layer being draw.
        public override bool HasColor(SymColor color)
        {
            return false;
        }

        // A hole draws no palette color, so it has no clipping objects.
        internal override IReadOnlyList<ClippingObject> MakeClippingObjects(Symbol symbol, SymColor color, CoveragePrototype prototype)
        {
            return Array.Empty<ClippingObject>();
        }

        public override void FreeGdiObjects()
        {
        }

    }



    // The GraphicsSymDef is a sort of dummy symdef used for graphics objects -- objects
    // created by doing a "To Graphics" operation in OCAD. These objects define their own color and shape,
    // so there really isn't any state in the symdef that is useful. There is a singleton GraphicsSymDef that
    // is used as the symdef for all of these objects, if there are any.
    public class GraphicsSymDef: SymDef
    {
        public GraphicsSymDef()
            : base("Graphics object", "-2")
        {
        }

        public override SymDef CopyToMap(Map map)
        {
            var newSymDef = new GraphicsSymDef();
            map.AddSymdef(newSymDef);
            return newSymDef;
        }

        // Does this symbol definition draw the given color. Used to determine
        // if this symbol definition draws into the current layer being draw.
        public override bool HasColor(SymColor color)
        {
            // Always return true, because graphics objects could be any color! (except image layer)
            if (color.IsSpecialLayer)
                return false;
            else
                return true;
        }

        // The full coverage, where the graphics object has the given palette color, or none. A graphics object has its
        // own colors (IGraphicsSymbol.HasColor).
        public override IReadOnlyList<CoveragePrototype> GetCoveragePrototypes(Symbol symbol, SymColor color)
        {
            IGraphicsSymbol graphicsSymbol = symbol as IGraphicsSymbol;
            if (graphicsSymbol != null && !color.IsSpecialLayer && graphicsSymbol.HasColor(color))
                return CoveragePrototype.FullCoverageOnly;
            else
                return CoveragePrototype.None;
        }

        // The geometry method of the graphics objects: in its color, the fill of a GraphicsAreaSymbol, a filled shape of
        // its path with holes as GraphicsAreaSymbol.Draw fills it, and the line of a GraphicsLineSymbol, a stroked shape
        // of its path with the pen that GraphicsLineSymbol.Draw creates: its width, caps and join and the miter limit
        // GraphicsUtil.MITER_LIMIT. Only these two kinds of symbols take this definition.
        internal override IReadOnlyList<ClippingObject> MakeClippingObjects(Symbol symbol, SymColor color, CoveragePrototype prototype)
        {
            Debug.Assert(prototype == CoveragePrototype.FullCoverage, "A graphics object has the full coverage only.");
            List<ClippingObject> clippingObjects = new List<ClippingObject>();
            if (symbol is GraphicsAreaSymbol area) {
                if (area.FillColor != null && color == area.FillColor)
                    clippingObjects.Add(area.Path.MakeFilledShape(Placement.Identity));
            }
            else if (symbol is GraphicsLineSymbol line) {
                if (line.LineColor != null && color == line.LineColor) {
                    Stroke stroke = new Stroke(line.Thickness, line.LineCapMode, line.LineJoinMode, GraphicsUtil.MITER_LIMIT);
                    clippingObjects.Add(line.Path.MakeStrokedShape(stroke, Placement.Identity));
                }
            }
            else {
                throw new ArgumentException("Only a GraphicsAreaSymbol and a GraphicsLineSymbol take a GraphicsSymDef.", nameof(symbol));
            }

            return clippingObjects.AsReadOnly();
        }

        public override void FreeGdiObjects()
        {
        }

    }

    // The ImageSymDef is a sort of dummy symdef used for image objects and layout objects -- 
    // created by doing a import image operation in OCAD, or layout object. These objects live in a layer below
    // all others or above all others. There is a singleton ImageSymDef that
    // is used as the symdef for all of these objects, if there are any.
    public class ImageSymDef: SymDef
    {
        private SymLayer layer;

        public ImageSymDef(SymLayer layer)
            : base(layer == SymLayer.Image ? "Image object" : "Layout object", 
                   layer == SymLayer.Image ? "-3" : "-4")
        {
            Debug.Assert(layer != SymLayer.Normal);
            this.layer = layer;
        }

        public override SymDef CopyToMap(Map map)
        {
            var newSymDef = new ImageSymDef(layer);
            map.AddSymdef(newSymDef);
            return newSymDef;
        }

        // Does this symbol definition draw the given color. Used to determine
        // if this symbol definition draws into the current layer being draw.
        public override bool HasColor(SymColor color)
        {
            return (color.Layer == layer);  // only draw in the layer we are associated with.
        }

        // The image layer and the layout layer are beside the palette stack, so an image or a layout object has no
        // clipping objects.
        internal override IReadOnlyList<ClippingObject> MakeClippingObjects(Symbol symbol, SymColor color, CoveragePrototype prototype)
        {
            return Array.Empty<ClippingObject>();
        }

        public override void FreeGdiObjects()
        {
        }

        public SymLayer Layer
        {
            get { return layer; }
        }

        // Image and layout objects have a sort order that needs to be preserved.
        public override bool SortSymbolsForDrawing
        {
            get { return true; }
        }

        // OCAD aligns text vertically a little differently than the map rendering. This function calculated
        // the OCAD vertical adjustment amount, for formatted or unformatted text.
        internal float GetOcadTopAdjustment(string fontName, float fontSize, bool formatted)
        {
            using (ITextFaceMetrics textFaceMetrics = map.TextMetricsProvider.GetTextFaceMetrics(fontName, fontSize, TextEffects.Regular)) {
                if (formatted) {
                    // OCAD adds an extra internal leading (incorrectly).
                    return textFaceMetrics.EmHeight - (textFaceMetrics.Ascent + textFaceMetrics.Descent);
                }
                else {
                    return textFaceMetrics.Ascent;
                }
            }
        }
    }

    public class RectangleSymDef : SymDef
    {
        const float gridWidth = 0.15F;
        const string fontName = "Arial";

        SymColor lineColor;
        float thickness;
        float cornerRadius;
        ushort gridFlags;
        float cellWidth, cellHeight;
        float textSize;
        int unnumberedCells;   // number of unnumbered cells
        string unnumberedText;  // text of unnumbered calls

        object mainPen = new object();
        object gridPen = new object();
        object font = new object();
        object brush = new object();
        float fontAscent;

        public SymColor LineColor { get { return lineColor; } }
        public float LineThickness { get { return thickness; } }

        public ushort GridFlags { get { return gridFlags; } }
        public float CellWidth { get { return cellWidth; }}
        public float CellHeight { get {return cellHeight; }}

        public float CornerRadius { get { return cornerRadius; } }

        public int UnnumberedCells { get { return unnumberedCells; }}

        public string UnnnumberedText { get { return unnumberedText; }}

        public float TextSize { get { return textSize; }}

        public RectangleSymDef(string name, string symbolId, SymColor lineColor, float thickness, float cornerRadius, ushort gridFlags, float cellWidth, float cellHeight, float textSize, int unnumberedCells, string unnumberedText)
            : base(name, symbolId)
        {
            this.lineColor = lineColor;
            this.thickness = thickness;
            this.cornerRadius = cornerRadius;
            this.gridFlags = gridFlags;
            this.cellWidth = cellWidth;
            this.cellHeight = cellHeight;
            this.textSize = textSize;
            this.unnumberedCells = unnumberedCells;
            this.unnumberedText = unnumberedText;
        }

        public override void SetMap(Map newMap)
        {
            base.SetMap(newMap);

            if (newMap != null) {
                // check colors.
                CheckColor(lineColor);
            }
        }

        public override SymDef CopyToMap(Map map)
        {
            var newSymDef = new RectangleSymDef(Name, SymbolId, map.SymColorFromSymColor(lineColor), thickness, cornerRadius, gridFlags, cellWidth, cellHeight, textSize, unnumberedCells, unnumberedText);
            map.AddSymdef(newSymDef);
            return newSymDef;
        }

        void CreateObjects(IGraphicsTarget g)
        {
            if (thickness > 0.0F && lineColor != null) {
                if (! g.HasPen(mainPen)) {
                    GraphicsUtil.CreateSolidPen(g, mainPen, lineColor.ColorValue, thickness, ((cornerRadius > 0) ? LineStyle.Rounded : LineStyle.Mitered));
                }
            }

            if (lineColor != null && (gridFlags & 1) != 0) {
                if (! g.HasPen(gridPen))
                    GraphicsUtil.CreateSolidPen(g, gridPen, lineColor.ColorValue, gridWidth, LineStyle.Beveled);
            }

            if (lineColor != null && (gridFlags & 1) != 0 && (gridFlags & 2) != 0) {
                if (!g.HasFont(font)) {
                    g.CreateFont(font, fontName, textSize, TextEffects.Bold);
                    using (ITextFaceMetrics textFaceMetrics = map.TextMetricsProvider.GetTextFaceMetrics(fontName, textSize, TextEffects.Bold))
                        fontAscent = textFaceMetrics.Ascent;

                }
                if (!g.HasBrush(brush)) {
                    g.CreateSolidBrush(brush, lineColor.ColorValue);
                }
            }
        }

        // Does this symbol definition draw the given color. Used to determine
        // if this symbol definition draws into the current layer being draw.
        public override bool HasColor(SymColor color)
        {
            Debug.Assert(color != null);
            if (color.IsSpecialLayer)
                return false;
            return (lineColor == color);
        }

        // Draw this point symbol at point pt with angle ang in this graphics (given color only).
        internal void Draw(IGraphicsTarget g, PointF location, SizeF size, float rotation, SymColor color, RenderOptions renderOpts)
        {
            if (size.Width == 0&& size.Height == 0)
                return;

            CreateObjects(g);

            // Draw the outside.
            if (lineColor != null && color == lineColor && thickness > 0.0F) {
                GetPath(location, size, rotation).Draw(g, mainPen);
            }

            if ((gridFlags & 1) != 0) {
                // We have a grid.
                Matrix matrix = new Matrix();
                matrix.Translate(location.X, location.Y);
                matrix.Rotate(rotation);

                g.PushTransform(matrix);
                try {
                    DrawGrid(g, size, color, renderOpts);
                }
                finally {
                    g.PopTransform();
                }
            }
        }

        // The geometry method of the rectangle symbols: the clipping objects that Draw draws in a palette color, in the
        // same order. In the color of the line, they are the line of the rectangle, a stroked path that keeps the Bezier
        // curves of its rounded corners, and then the lines and the texts of the grid, placed with the location and the
        // rotation of the rectangle. The pose of a text of the grid is its place in its cell at the angle 0, and the top
        // left corner of its text run is at the origin of the pose.
        internal override IReadOnlyList<ClippingObject> MakeClippingObjects(Symbol symbol, SymColor color, CoveragePrototype prototype)
        {
            Debug.Assert(prototype == CoveragePrototype.FullCoverage, "A rectangle has the full coverage only.");
            RectangleSymbol rectangle = (RectangleSymbol)symbol;
            PointF location = rectangle.Location;
            SizeF size = rectangle.Size;

            List<ClippingObject> clippingObjects = new List<ClippingObject>();
            if (size.Width == 0 && size.Height == 0)
                return clippingObjects.AsReadOnly();

            if (lineColor != null && color == lineColor && thickness > 0.0F) {
                Stroke stroke = GraphicsUtil.MakeSolidStroke(thickness, (cornerRadius > 0) ? LineStyle.Rounded : LineStyle.Mitered);
                clippingObjects.Add(GetPath(location, size, rectangle.Rotation).MakeStrokedShape(stroke, Placement.Identity));
            }

            if ((gridFlags & 1) != 0 && lineColor != null && color == lineColor && size.Width > 0 && size.Height > 0) {
                Matrix matrix = new Matrix();
                matrix.Translate(location.X, location.Y);
                matrix.Rotate(rectangle.Rotation);
                SinglePlacement placement = Placement.Identity.Within(matrix);

                int cxCells = (int)Math.Round(size.Width / cellWidth);
                if (cxCells < 1)
                    cxCells = 1;
                int cyCells = (int)Math.Round(size.Height / cellHeight);
                if (cyCells < 1)
                    cyCells = 1;

                float width = size.Width / cxCells;
                float height = size.Height / cyCells;

                Stroke gridStroke = GraphicsUtil.MakeSolidStroke(gridWidth, LineStyle.Beveled);
                for (int x = 1; x < cxCells; ++x) {
                    PointF[] points = { new PointF(x * width, 0), new PointF(x * width, size.Height) };
                    clippingObjects.Add(new StrokedShape(PathShape.FromPoints(points, false), gridStroke, placement));
                }
                for (int y = 1; y < cyCells; ++y) {
                    PointF[] points = { new PointF(0, y * height), new PointF(size.Width, y * height) };
                    clippingObjects.Add(new StrokedShape(PathShape.FromPoints(points, false), gridStroke, placement));
                }

                if ((gridFlags & 2) != 0)
                    AddGridTexts(clippingObjects, cxCells, cyCells, width, height, placement);
            }

            return clippingObjects.AsReadOnly();
        }

        // Add the clipping objects of the texts of the grid, as DrawGridText draws them: the number of each cell, or the
        // text of the unnumbered cells, in Arial bold, each at the pose of its place in its cell. A cell without text has
        // no clipping object.
        // clippingObjects: the clipping objects, which get those of the texts.
        // cxCells, cyCells: the numbers of the cells across and down.
        // width, height: the size of a cell.
        // placement: the placement of the grid.
        private void AddGridTexts(List<ClippingObject> clippingObjects, int cxCells, int cyCells, float width, float height, SinglePlacement placement)
        {
            for (int y = 0; y < cyCells; ++y)
                for (int x = 0; x < cxCells; ++x) {
                    int cellNum;
                    string cellText;

                    if ((gridFlags & 4) != 0) // number from bottom
                        cellNum = y * cxCells + x + 1;
                    else
                        cellNum = (cyCells - 1 - y) * cxCells + x + 1;

                    if (cellNum > cxCells * cyCells - unnumberedCells)
                        cellText = unnumberedText;
                    else
                        cellText = cellNum.ToString();

                    PointF pt = new PointF(x * width, (y + 1) * height);
                    pt.Y -= textSize * 0.17F;
                    pt.X += textSize * 0.28F;

                    if (!string.IsNullOrEmpty(cellText)) {
                        Matrix pose = new Matrix();
                        pose.Translate(pt.X, pt.Y);
                        TextShape run = new TextShape(cellText, fontName, textSize, TextEffects.Bold, new PointF(0, 0), map.TextMetricsProvider);
                        clippingObjects.Add(new FilledShape(run, AreaFillMode.Winding, placement.Within(pose)));
                    }
                }
        }

        void DrawGrid(IGraphicsTarget g, SizeF size, SymColor color, RenderOptions renderOpts)
        {
            if (lineColor != null && color == lineColor && size.Width > 0 && size.Height > 0) {
                int cxCells = (int)Math.Round(size.Width / cellWidth);
                if (cxCells < 1)
                    cxCells = 1;
                int cyCells = (int)Math.Round(size.Height / cellHeight);
                if (cyCells < 1)
                    cyCells = 1;

                float width = size.Width / cxCells;
                float height = size.Height / cyCells;

                DrawGridLines(g, size, cxCells, cyCells, width, height);
                if ((gridFlags & 2) != 0)
                    DrawGridText(g, size, cxCells, cyCells, width, height);
            }
        }

        private void DrawGridLines(IGraphicsTarget g, SizeF size, int cxCells, int cyCells, float width, float height)
        {
            for (int x = 1; x < cxCells; ++x) {
                PointF pt1 = new PointF(x * width, 0);
                PointF pt2 = new PointF(x * width, size.Height);
                g.DrawLine(gridPen, pt1, pt2);
            }

            for (int y = 1; y < cyCells; ++y) {
                PointF pt1 = new PointF(0, y * height);
                PointF pt2 = new PointF(size.Width, y * height);
                g.DrawLine(gridPen, pt1, pt2);
            }
        }

        private void DrawGridText(IGraphicsTarget g, SizeF size, int cxCells, int cyCells, float width, float height)
        {
            for (int y = 0; y < cyCells; ++y)
                for (int x = 0; x < cxCells; ++x) {
                    int cellNum;
                    string cellText;

                    if ((gridFlags & 4) != 0) // number from bottom
                        cellNum = y * cxCells + x + 1;
                    else
                        cellNum = (cyCells - 1 - y) * cxCells + x + 1;

                    if (cellNum > cxCells * cyCells - unnumberedCells)
                        cellText = unnumberedText;
                    else
                        cellText = cellNum.ToString();

                    PointF pt = new PointF(x * width, (y + 1) * height);
                    pt.Y -= textSize * 0.17F;
                    pt.X += textSize * 0.28F;

                    Matrix flipMatrix = new Matrix();
                    flipMatrix.Translate(pt.X, pt.Y);
                    flipMatrix.Scale(1, -1); // Reverse Y direction so text is correct way around.
                    g.PushTransform(flipMatrix);

                    g.DrawText(cellText, font, brush, new PointF());

                    g.PopTransform();
                }
        }

        // Get the path of the outer rectangle (including corners)
        internal SymPath GetPath(PointF location, SizeF size, float rotation)
        {
            SymPath path = SymPath.CreateRoundedRectangle(new RectangleF(location, size), cornerRadius);
            if (rotation != 0) {
                Matrix mat = new Matrix();
                mat.RotateAt(rotation, location);
                path = path.Transform(mat);
            }

            return path;
        }

        // Calculate the bounding box
        internal RectangleF CalcBounds(PointF location, SizeF size, float rotation)
        {
            RectangleF box = new RectangleF(location, size);
            box.Inflate(thickness / 2, thickness / 2);
            if (rotation != 0)
                box = Geometry.BoundsOfRotatedRectangle(box, location, rotation);

            return box;
        }

        public override void FreeGdiObjects()
        {
            lineColor.FreeGdiObjects();
        }
    }


    // Enum for the different kinds of line
    public enum LineStyle { 
        Rounded,                 // round ends, corners
        Mitered,                   // flat ends, mitered corners
        Beveled,                   // flat ends, bevel corners
        FlatRounded         // flat ends, round corners -- not used directly for symbols, but for parts of symbols.
    };
}

