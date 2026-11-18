using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using PurplePen.Graphics2D;
using PurplePen.MapModel;

namespace Map_Skia.Tests
{
    // The layer instances that pass 1 makes (Map.CreateLayerInstances), checked without a drawing: the number of the
    // instances, their palette entries and prototype, and the number, the kinds and the bounds of their clipping
    // objects. Each test makes its map, so that it knows the shapes of the symbols.
    [TestFixture]
    public class LayerInstanceTests
    {
        const float Tolerance = 0.0001F;

        static LayerInstanceTests()
        {
            // RenderingTests adds the Roboto fonts, which the text symbols use.
            System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(RenderingTests).TypeHandle);
        }

        [Test]
        public void EachPaletteEntryWithShapesHasOneInstanceOfFullCoverage()
        {
            Map map = CreateMap();
            SymColor brown, unused, blue, black;
            using (map.Write()) {
                brown = map.AddColor("Brown", 1, 0, 0.56F, 1, 0.18F, false);
                unused = map.AddColor("Unused", 2, 0, 1, 0, 0, false);
                blue = map.AddColor("Blue", 3, 1, 0, 0, 0, false);
                black = map.AddColor("Black", 4, 0, 0, 0, 1, false);
                LineSymDef contour = AddLineSymdef(map, "Contour", brown, 0.14F);
                LineSymDef stream = AddLineSymdef(map, "Stream", blue, 0.18F);
                LineSymDef track = AddLineSymdef(map, "Track", black, 0.25F);
                map.AddSymbol(new LineSymbol(contour, CreateLinePath(new PointF(0, 0), new PointF(10, 0))));
                map.AddSymbol(new LineSymbol(contour, CreateLinePath(new PointF(0, 5), new PointF(10, 5))));
                map.AddSymbol(new LineSymbol(stream, CreateLinePath(new PointF(0, 2), new PointF(10, 3))));
                map.AddSymbol(new LineSymbol(track, CreateLinePath(new PointF(2, 0), new PointF(2, 10))));
            }

            IReadOnlyList<LayerInstance> instances = CreateInstances(map);

            CollectionAssert.AreEqual(new SymColor[] { brown, blue, black }, instances.Select(instance => instance.PaletteEntry));
            CollectionAssert.AreEqual(new int[] { 2, 1, 1 }, instances.Select(instance => instance.ClippingObjects.Count));
            foreach (LayerInstance instance in instances) {
                Assert.AreSame(GeometryPrototype.FullCoverage, instance.Prototype);
                Assert.IsTrue(instance.Transformation.IsIdentity);
            }
            Assert.IsTrue(GeometryPrototype.FullCoverage.Covers(new PointF(123, -45)));
        }

        [Test]
        public void LineIsAStrokedPathWithTheLineOfItsSymbol()
        {
            Map map = CreateMap();
            using (map.Write()) {
                SymColor black = map.AddColor("Black", 1, 0, 0, 0, 1, false);
                LineSymDef track = new LineSymDef("Track", "1", black, 0.4F, LineJoinMode.Miter, LineCapMode.Flat);
                map.AddSymdef(track);
                map.AddSymbol(new LineSymbol(track, CreateLinePath(new PointF(0, 0), new PointF(10, 0), new PointF(10, 5))));
            }

            StrokedShape line = (StrokedShape)GetSingleClippingObject(CreateInstances(map).Single());

            PathShape path = (PathShape)line.Shape;
            CollectionAssert.AreEqual(new GraphicsPathPartKind[] { GraphicsPathPartKind.Start, GraphicsPathPartKind.Lines }, path.Parts.Select(part => part.Kind));
            CollectionAssert.AreEqual(new PointF[] { new PointF(10, 0), new PointF(10, 5) }, path.Parts[1].Points);
            Assert.AreEqual(0.4F, line.Stroke.Width);
            Assert.AreEqual(LineCapMode.Flat, line.Stroke.Caps);
            Assert.AreEqual(LineJoinMode.Miter, line.Stroke.Join);
            Assert.AreEqual(GraphicsUtil.MITER_LIMIT, line.Stroke.MiterLimit);
            Assert.AreEqual(0, line.Placement.Steps.Count);
            // The miter joins reach the miter limit times half the width from the path.
            AssertBounds(RectangleF.FromLTRB(-1, -1, 11, 6), line.Bounds);
        }

        [Test]
        public void EachPartOfALineIsInItsColor()
        {
            Map map = CreateMap();
            SymColor yellow, brown, blue, black;
            using (map.Write()) {
                yellow = map.AddColor("Yellow", 1, 0, 0.27F, 0.79F, 0, false);
                brown = map.AddColor("Brown", 2, 0, 0.56F, 1, 0.18F, false);
                blue = map.AddColor("Blue", 3, 1, 0, 0, 0, false);
                black = map.AddColor("Black", 4, 0, 0, 0, 1, false);

                LineSymDef road = new LineSymDef("Road", "1", black, 0.35F, LineJoinMode.Round, LineCapMode.Round);
                road.SetSecondLine(brown, 0.5F, LineJoinMode.Round, LineCapMode.Round);
                LineSymDef.DoubleLineInfo doubleLines = new LineSymDef.DoubleLineInfo();
                doubleLines.doubleFillColor = yellow;
                doubleLines.doubleThick = 1.0F;
                doubleLines.doubleLeftColor = blue;
                doubleLines.doubleLeftWidth = 0.1F;
                doubleLines.doubleRightColor = blue;
                doubleLines.doubleRightWidth = 0.1F;
                road.SetDoubleLines(doubleLines);
                map.AddSymdef(road);
                map.AddSymbol(new LineSymbol(road, CreateLinePath(new PointF(0, 0), new PointF(10, 0))));
            }

            IReadOnlyList<LayerInstance> instances = CreateInstances(map);

            CollectionAssert.AreEqual(new SymColor[] { yellow, brown, blue, black }, instances.Select(instance => instance.PaletteEntry));
            CollectionAssert.AreEqual(new float[] { 1.0F }, GetStrokeWidths(instances[0]));
            CollectionAssert.AreEqual(new float[] { 0.5F }, GetStrokeWidths(instances[1]));
            CollectionAssert.AreEqual(new float[] { 0.1F, 0.1F }, GetStrokeWidths(instances[2]));
            CollectionAssert.AreEqual(new float[] { 0.35F }, GetStrokeWidths(instances[3]));

            // The left and the right line lie half the fill and half their width away from the path.
            float[] borderOffsets = instances[2].ClippingObjects.Select(border => Math.Abs((border.Bounds.Top + border.Bounds.Bottom) / 2)).ToArray();
            Assert.AreEqual(0.55F, borderOffsets[0], Tolerance);
            Assert.AreEqual(0.55F, borderOffsets[1], Tolerance);
            Assert.AreNotEqual(instances[2].ClippingObjects[0].Bounds.Top, instances[2].ClippingObjects[1].Bounds.Top);
        }

        [Test]
        public void DashedLineIsItsDashesAlsoWhenItsGapsAreShorterThanAPixel()
        {
            Map map = CreateMap();
            using (map.Write()) {
                SymColor black = map.AddColor("Black", 1, 0, 0, 0, 1, false);
                LineSymDef footpath = new LineSymDef("Footpath", "1", black, 0.18F, LineJoinMode.Miter, LineCapMode.Flat);
                // The gaps of 0.01 mm are shorter than a pixel of a print at 1200 dpi, 0.021 mm. A drawing at that
                // resolution draws the line solid.
                footpath.SetDashInfo(new LineSymDef.DashInfo() { dashLength = 1, firstDashLength = 1, lastDashLength = 1, gapLength = 0.01F });
                map.AddSymdef(footpath);
                map.AddSymbol(new LineSymbol(footpath, CreateLinePath(new PointF(0, 0), new PointF(10, 0))));
            }

            IReadOnlyList<ClippingObject> dashes = CreateInstances(map).Single().ClippingObjects;

            // The line of 10 mm has 9 gaps of 0.01 mm, and 10 dashes of the same length in the rest of the line.
            Assert.AreEqual(10, dashes.Count);
            float dashLength = (10 - 9 * 0.01F) / 10;
            float previousEnd = 0;
            for (int i = 0; i < dashes.Count; ++i) {
                StrokedShape dash = (StrokedShape)dashes[i];
                Assert.AreEqual(0.18F, dash.Stroke.Width, Tolerance);
                PathShape path = (PathShape)dash.Shape;
                PointF start = path.Parts[0].Points[0];
                PointF end = path.Parts[path.Parts.Count - 1].Points.Last();
                Assert.AreEqual((i == 0) ? 0 : 0.01F, start.X - previousEnd, Tolerance, "gap before dash {0}", i);
                Assert.AreEqual(dashLength, end.X - start.X, Tolerance, "length of dash {0}", i);
                Assert.AreEqual(0, start.Y);
                Assert.AreEqual(0, end.Y);
                previousEnd = end.X;
            }
            Assert.AreEqual(10, previousEnd, Tolerance);
        }

        [Test]
        public void GlyphOfAPointSymbolIsPlacedWithTheSymbol()
        {
            Map map = CreateMap();
            SymColor brown, black;
            using (map.Write()) {
                brown = map.AddColor("Brown", 1, 0, 0.56F, 1, 0.18F, false);
                black = map.AddColor("Black", 2, 0, 0, 0, 1, false);

                Glyph glyph = new Glyph();
                glyph.AddLine(brown, CreateLinePath(new PointF(-1, 0), new PointF(1, 0)), 0.2F, LineJoinMode.Round, LineCapMode.Round);
                glyph.AddFilledCircle(black, new PointF(0, 0.5F), 0.4F);
                glyph.ConstructionComplete();
                PointSymDef knoll = new PointSymDef("Knoll", "1", glyph, true);
                map.AddSymdef(knoll);
                map.AddSymbol(new PointSymbol(knoll, new PointF(5, 3), 90, null));
            }

            IReadOnlyList<LayerInstance> instances = CreateInstances(map);
            StrokedShape line = (StrokedShape)GetSingleClippingObject(instances[0]);
            FilledShape dot = (FilledShape)GetSingleClippingObject(instances[1]);

            // One transformation places both parts: it moves the glyph to the location and turns it by 90 degrees.
            AssertEqualPlacements(line.Placement, dot.Placement);
            Assert.AreEqual(1, line.Placement.Steps.Count);
            PointF[] placed = { new PointF(0, 0), new PointF(1, 0) };
            line.Placement.Matrix.TransformPoints(placed);
            Assert.AreEqual(5, placed[0].X, Tolerance);
            Assert.AreEqual(3, placed[0].Y, Tolerance);
            Assert.AreEqual(5, placed[1].X, Tolerance);
            Assert.AreEqual(4, placed[1].Y, Tolerance);

            Assert.IsInstanceOf<PathShape>(line.Shape);
            AssertBounds(RectangleF.FromLTRB(4.9F, 1.9F, 5.1F, 4.1F), line.Bounds);
            EllipseShape ellipse = (EllipseShape)dot.Shape;
            Assert.AreEqual(new PointF(0, 0.5F), ellipse.Center);
            Assert.AreEqual(0.2F, ellipse.RadiusX, Tolerance);
            AssertBounds(RectangleF.FromLTRB(4.3F, 2.8F, 4.7F, 3.2F), dot.Bounds);
        }

        [Test]
        public void CirclesOfAPointSymbolAreEllipsesAndArcs()
        {
            Map map = CreateMap();
            using (map.Write()) {
                SymColor black = map.AddColor("Black", 1, 0, 0, 0, 1, false);

                Glyph dot = new Glyph();
                dot.AddFilledCircle(black, new PointF(0, 0), 1.0F);
                dot.ConstructionComplete();
                PointSymDef dotSymdef = new PointSymDef("Dot", "1", dot, false);
                map.AddSymdef(dotSymdef);
                map.AddSymbol(new PointSymbol(dotSymdef, new PointF(-5, 2), 0, null));

                Glyph ring = new Glyph();
                ring.AddCircle(black, new PointF(0, 0), 0.1F, 2.0F);
                ring.ConstructionComplete();
                PointSymDef ringSymdef = new PointSymDef("Ring", "2", ring, false);
                map.AddSymdef(ringSymdef);
                // A gap from 180 to 360 degrees leaves the half of the ring from 0 to 180 degrees.
                map.AddSymbol(new PointSymbol(ringSymdef, new PointF(5, 2), 0, new float[] { 180, 360 }));
            }

            IReadOnlyList<ClippingObject> clippingObjects = CreateInstances(map).Single().ClippingObjects;
            Assert.AreEqual(2, clippingObjects.Count);

            // A glyph of one circle at its origin is drawn at the location of the symbol, with no transformation.
            FilledShape dotShape = (FilledShape)clippingObjects[0];
            Assert.AreEqual(0, dotShape.Placement.Steps.Count);
            Assert.AreEqual(new PointF(-5, 2), ((EllipseShape)dotShape.Shape).Center);
            AssertBounds(RectangleF.FromLTRB(-5.5F, 1.5F, -4.5F, 2.5F), dotShape.Bounds);

            StrokedShape ringShape = (StrokedShape)clippingObjects[1];
            ArcShape arc = (ArcShape)ringShape.Shape;
            Assert.AreEqual(0.95F, arc.Radius, Tolerance);
            Assert.AreEqual(180, arc.SweepAngle, Tolerance);
            Assert.AreEqual(0.1F, ringShape.Stroke.Width, Tolerance);
            AssertBounds(RectangleF.FromLTRB(-0.95F, 0, 0.95F, 0.95F), arc.Bounds);
        }

        [Test]
        public void AreaHasItsFillAndBorderButNotItsHatchingsAndPatterns()
        {
            Map map = CreateMap();
            SymColor green, blue, brown, black;
            using (map.Write()) {
                green = map.AddColor("Green", 1, 0.76F, 0, 0.91F, 0, false);
                blue = map.AddColor("Blue", 2, 1, 0, 0, 0, false);
                brown = map.AddColor("Brown", 3, 0, 0.56F, 1, 0.18F, false);
                black = map.AddColor("Black", 4, 0, 0, 0, 1, false);

                LineSymDef border = AddLineSymdef(map, "Border", black, 0.2F);
                AreaSymDef forest = new AreaSymDef("Forest", "2", green, border);
                forest.AddHatching(new AreaSymDef.HatchInfo() { hatchColor = blue, hatchWidth = 0.1F, hatchSpacing = 0.5F, hatchAngle = 45 });
                forest.AddHatching(new AreaSymDef.HatchInfo() { hatchColor = green, hatchWidth = 0.1F, hatchSpacing = 0.5F, hatchAngle = -45 });
                Glyph element = new Glyph();
                element.AddFilledCircle(brown, new PointF(0, 0), 0.3F);
                element.ConstructionComplete();
                forest.AddPattern(new AreaSymDef.PatternInfo() { patternGlyph = element, patternWidth = 1, patternHeight = 1 });
                map.AddSymdef(forest);
                map.AddSymbol(new AreaSymbol(forest, new SymPathWithHoles(CreateSquarePath(0, 0, 10), null), 0, new PointF()));
            }

            IReadOnlyList<LayerInstance> instances = CreateInstances(map);

            // The hatchings and the pattern are repetitive geometry. Their colors have no other shapes.
            CollectionAssert.AreEqual(new SymColor[] { green, black }, instances.Select(instance => instance.PaletteEntry));

            FilledShape fill = (FilledShape)GetSingleClippingObject(instances[0]);
            Assert.IsInstanceOf<PathShape>(fill.Shape);
            Assert.AreEqual(AreaFillMode.Alternate, fill.FillRule);
            AssertBounds(RectangleF.FromLTRB(0, 0, 10, 10), fill.Bounds);

            StrokedShape outline = (StrokedShape)GetSingleClippingObject(instances[1]);
            Assert.AreEqual(0.2F, outline.Stroke.Width, Tolerance);
            Assert.IsTrue(outline.Bounds.Contains(fill.Bounds));
        }

        [Test]
        public void TextHasItsRunsUnderlineAndFraming()
        {
            Map map = CreateMap();
            SymColor yellow, blue, black;
            TextSymbol label, lineLabel;
            using (map.Write()) {
                yellow = map.AddColor("Yellow", 1, 0, 0.27F, 0.79F, 0, false);
                blue = map.AddColor("Blue", 2, 1, 0, 0, 0, false);
                black = map.AddColor("Black", 3, 0, 0, 0, 1, false);

                TextSymDef labelSymdef = new TextSymDef("Label", "1", TextSymDef.PreferredSymbolKind.NormalText, null);
                labelSymdef.SetFont("Roboto", 4, TextEffects.Regular, black, 4, 0, 0, 0, null, 0, 1, TextSymDefHorizAlignment.Left, TextSymDefVertAlignment.TopAscent);
                labelSymdef.SetFraming(new TextSymDef.Framing() { framingStyle = TextSymDef.FramingStyle.Rectangle, framingColor = yellow, rectBorderLeft = 0.5F, rectBorderTop = 0.5F, rectBorderRight = 0.5F, rectBorderBottom = 0.5F });
                labelSymdef.SetUnderline(new TextSymDef.Underlining() { underlineOn = true, underlineColor = blue, underlineWidth = 0.2F, underlineDistance = 0.3F });
                map.AddSymdef(labelSymdef);
                label = new TextSymbol(labelSymdef, new string[] { "Two words" }, new PointF(0, 0), 0, 0, TextSymDefHorizAlignment.Default, TextSymDefVertAlignment.Default);
                map.AddSymbol(label);

                TextSymDef lineLabelSymdef = new TextSymDef("Line label", "2", TextSymDef.PreferredSymbolKind.LineText, null);
                lineLabelSymdef.SetFont("Roboto", 4, TextEffects.Regular, black, 4, 0, 0, 0, null, 0, 1, TextSymDefHorizAlignment.Left, TextSymDefVertAlignment.TopAscent);
                map.AddSymdef(lineLabelSymdef);
                map.AddSymbol(new LineTextSymbol(lineLabelSymdef, CreateLinePath(new PointF(0, -20), new PointF(20, -10)), "AB", TextSymDefHorizAlignment.Default, TextSymDefVertAlignment.Default));
            }

            IReadOnlyList<LayerInstance> instances = CreateInstances(map);
            CollectionAssert.AreEqual(new SymColor[] { yellow, blue, black }, instances.Select(instance => instance.PaletteEntry));

            // Each word of the label is a text run, and each character of the line label is one at its own placement.
            List<FilledShape> runs = instances[2].ClippingObjects.Cast<FilledShape>().ToList();
            CollectionAssert.AreEqual(new string[] { "Two", "words", "A", "B" }, runs.Select(run => ((TextShape)run.Shape).Text));
            foreach (FilledShape run in runs) {
                TextShape text = (TextShape)run.Shape;
                Assert.AreEqual("Roboto", text.FontFamily);
                Assert.AreEqual(4, text.EmHeight);
                Assert.AreEqual(TextEffects.Regular, text.Effects);
                Assert.AreEqual(1, run.Placement.Steps.Count);
            }
            Assert.AreSame(runs[0].Placement, runs[1].Placement);
            Assert.AreNotSame(runs[2].Placement, runs[3].Placement);
            using (map.Read()) {
                Assert.IsTrue(label.BoundingBox.Contains(runs[0].Bounds));
                Assert.IsTrue(label.BoundingBox.Contains(runs[1].Bounds));
            }

            // The underline and the framing rectangle of the label are placed with its text.
            StrokedShape underline = (StrokedShape)GetSingleClippingObject(instances[1]);
            Assert.AreEqual(0.2F, underline.Stroke.Width, Tolerance);
            AssertEqualPlacements(runs[0].Placement, underline.Placement);
            FilledShape framing = (FilledShape)GetSingleClippingObject(instances[0]);
            Assert.IsInstanceOf<PathShape>(framing.Shape);
            AssertEqualPlacements(runs[0].Placement, framing.Placement);
            Assert.IsTrue(framing.Bounds.Contains(runs[0].Bounds));
            Assert.IsTrue(framing.Bounds.Contains(runs[1].Bounds));
        }

        [Test]
        public void InstancesHoldTheVisibleSymbolsWhereverTheyLie()
        {
            Map map = CreateMap();
            SymColor brown, blue, red;
            using (map.Write()) {
                brown = map.AddColor("Brown", 1, 0, 0.56F, 1, 0.18F, false);
                blue = map.AddColor("Blue", 2, 1, 0, 0, 0, false);
                red = map.AddColor("Red", 3, 0, 1, 1, 0, false);

                LineSymDef contour = AddLineSymdef(map, "Contour", brown, 0.14F);
                map.AddSymbol(new LineSymbol(contour, CreateLinePath(new PointF(0, 0), new PointF(10, 0))));
                // A symbol far from the others: 6.4 m away on the paper of the map.
                map.AddSymbol(new LineSymbol(contour, CreateLinePath(new PointF(5000, -4000), new PointF(5010, -4000))));
                LineSymDef hiddenStream = AddLineSymdef(map, "Stream", blue, 0.18F);
                map.AddSymbol(new LineSymbol(hiddenStream, CreateLinePath(new PointF(0, 2), new PointF(10, 2))));
                map.SetSymdefVisible(hiddenStream, false);

                // Graphics objects have their colors, and image and layout objects are no palette entries.
                GraphicsSymDef graphics = new GraphicsSymDef();
                map.AddSymdef(graphics);
                map.AddSymbol(new GraphicsAreaSymbol(graphics, new SymPathWithHoles(CreateSquarePath(0, 0, 2), null), red));
                map.AddSymbol(new GraphicsLineSymbol(graphics, CreateLinePath(new PointF(0, 4), new PointF(4, 4)), red, 0.3F, LineJoinMode.Round, LineCapMode.Round));
                map.AddSymbol(new ImageAreaSymbol(map.GetImageSymDef(), new SymPathWithHoles(CreateSquarePath(3, 3, 2), null), CmykColor.FromCmyk(0, 0, 0, 0.5F), true, 0));
                map.AddSymbol(new ImageAreaSymbol(map.GetLayoutSymDef(), new SymPathWithHoles(CreateSquarePath(5, 5, 2), null), CmykColor.FromCmyk(0, 0, 0, 0.5F), true, 0));
            }

            IReadOnlyList<LayerInstance> instances = CreateInstances(map);

            CollectionAssert.AreEqual(new SymColor[] { brown, red }, instances.Select(instance => instance.PaletteEntry));
            Assert.AreEqual(2, instances[0].ClippingObjects.Count);
            RectangleF farContour = instances[0].ClippingObjects[1].Bounds;
            Assert.IsTrue(farContour.Contains(new PointF(5000, -4000)) && farContour.Contains(new PointF(5010, -4000)), "bounds of the far contour {0}", farContour);
            Assert.IsInstanceOf<FilledShape>(instances[1].ClippingObjects[0]);
            Assert.IsInstanceOf<StrokedShape>(instances[1].ClippingObjects[1]);
            Assert.AreEqual(2, instances[1].ClippingObjects.Count);
            AssertBounds(RectangleF.FromLTRB(0, 0, 2, 2), instances[1].ClippingObjects[0].Bounds);
            AssertBounds(RectangleF.FromLTRB(-0.15F, 3.85F, 4.15F, 4.15F), instances[1].ClippingObjects[1].Bounds);
        }

        // An empty map that measures text with Skia.
        static Map CreateMap()
        {
            return new Map(new Skia_TextMetrics(), null);
        }

        // The instances of a map.
        static IReadOnlyList<LayerInstance> CreateInstances(Map map)
        {
            using (map.Read())
                return map.CreateLayerInstances();
        }

        // Add a solid line symbol with mitered joins and flat caps to a map.
        static LineSymDef AddLineSymdef(Map map, string name, SymColor color, float width)
        {
            LineSymDef symdef = new LineSymDef(name, (map.AllSymdefs.Count + 1).ToString(), color, width, LineJoinMode.Miter, LineCapMode.Flat);
            map.AddSymdef(symdef);
            return symdef;
        }

        // A path of lines through points.
        static SymPath CreateLinePath(params PointF[] points)
        {
            return new SymPath(points);
        }

        // The closed path of a square.
        static SymPath CreateSquarePath(float left, float bottom, float size)
        {
            return new SymPath(new PointF[] {
                new PointF(left, bottom), new PointF(left + size, bottom), new PointF(left + size, bottom + size),
                new PointF(left, bottom + size), new PointF(left, bottom) });
        }

        // The one clipping object of an instance.
        static ClippingObject GetSingleClippingObject(LayerInstance instance)
        {
            Assert.AreEqual(1, instance.ClippingObjects.Count, "The clipping objects of {0}", instance);
            return instance.ClippingObjects[0];
        }

        // The widths of the strokes of the clipping objects of an instance, which are all stroked paths.
        static float[] GetStrokeWidths(LayerInstance instance)
        {
            return instance.ClippingObjects.Select(clippingObject => {
                StrokedShape stroked = (StrokedShape)clippingObject;
                Assert.IsInstanceOf<PathShape>(stroked.Shape);
                return stroked.Stroke.Width;
            }).ToArray();
        }

        // Assert that two placements have the same steps. The shapes of two palette entries do not share a placement,
        // since the symbols draw each palette entry apart.
        static void AssertEqualPlacements(Placement expected, Placement actual)
        {
            Assert.AreEqual(expected.Steps.Count, actual.Steps.Count, "steps");
            for (int i = 0; i < expected.Steps.Count; ++i)
                CollectionAssert.AreEqual(expected.Steps[i].Elements, actual.Steps[i].Elements, "step {0}", i);
        }

        static void AssertBounds(RectangleF expected, RectangleF actual)
        {
            Assert.AreEqual(expected.Left, actual.Left, Tolerance, "left of {0}", actual);
            Assert.AreEqual(expected.Top, actual.Top, Tolerance, "top of {0}", actual);
            Assert.AreEqual(expected.Right, actual.Right, Tolerance, "right of {0}", actual);
            Assert.AreEqual(expected.Bottom, actual.Bottom, Tolerance, "bottom of {0}", actual);
        }
    }
}
