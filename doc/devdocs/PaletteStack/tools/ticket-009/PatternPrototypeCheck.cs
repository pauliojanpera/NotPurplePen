// A check of the choices of the draft 009 before the ticket goes to @Pauli. It is not committed: it lives in the local
// integration worktree only, and the session removes it after the check.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using PurplePen.Graphics2D;
using PurplePen.MapModel;
using SkiaSharp;
using TestingUtils;

namespace Map_Skia.Tests
{
    [TestFixture, Parallelizable(ParallelScope.Children)]
    public class PatternPrototypeCheck
    {
        static PatternPrototypeCheck()
        {
            System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(RenderingTests).TypeHandle);
        }

        static readonly MethodInfo handleIrregularity = typeof(AreaSymDef).GetMethod("HandleIrregularity", BindingFlags.Instance | BindingFlags.NonPublic);
        static readonly FieldInfo partsField = typeof(Glyph).GetField("parts", BindingFlags.Instance | BindingFlags.NonPublic);
        static readonly MethodInfo sharedMethod = typeof(Glyph).GetMethod("GetSharedClippingObjects", BindingFlags.Instance | BindingFlags.NonPublic);
        static readonly MethodInfo circleBorderPoints = typeof(Glyph.GlyphPart).GetMethod("CircleBorderPoints", BindingFlags.Instance | BindingFlags.NonPublic);

        [TestCase("rotarea-test.txt")]
        [TestCase("rotarea-test9.txt")]
        [TestCase("offsetpattern_nopatbm.txt")]
        [TestCase("offsetpatternrot_nopatbm.txt")]
        [TestCase("offsetpatternrot2_nopatbm.txt")]
        [TestCase("wholestructure.txt")]
        [TestCase("wholestructure2.txt")]
        [TestCase("wholestructure3.txt")]
        [TestCase("wholestructure4.txt")]
        [TestCase("irregular1.txt")]
        [TestCase("irregular2.txt")]
        [TestCase("irregular3.txt")]
        [TestCase("irregular4.txt")]
        [TestCase("holes.txt")]
        [TestCase("holes9.txt")]
        [TestCase("isomarea_nopatbm.txt")]
        [TestCase("isomareabug_nopatbm.txt")]
        [TestCase("borderedarea9.txt")]
        [TestCase("glyphholes.txt")]
        [TestCase("glyphholes9.txt")]
        [TestCase("graphicobjects9.txt")]
        [TestCase("LordHill.txt")]
        [TestCase("lksamm1.txt")]
        [TestCase("marymoor.txt")]
        [TestCase("hakolaoverprinting.txt")]
        [TestCase("aiimport.txt")]
        [TestCase("FtCasey.txt")]
        [TestCase("PenistoneHill.txt")]
        [TestCase("punchboxbug.txt")]
        [TestCase("missingcolor.txt")]
        [TestCase("clouds.txt")]
        [TestCase("teanwest.txt")]
        public void CoverageOfTheChoicesIsTheDrawing(string testFileName)
        {
            TestMap testMap = TestMap.Read(testFileName);
            Map map = testMap.Map;
            List<string> differences = new List<string>();
            int compared = 0, withPrototypes = 0, hatchLines = 0, elements = 0, selectedParts = 0;

            using (map.Read()) {
                List<SymColor> colors = map.AllColors.ToList();
                IReadOnlyList<PaletteSeparation> separations = map.PaletteSeparations;
                bool hasImages = map.AllSymbols.Any(s => s.Definition is ImageSymDef d && d.Layer == SymLayer.Image && map.IsSymdefVisible(s.Definition) && s.IsVisible);

                for (int index = 0; index < colors.Count; ++index) {
                    if (index == 0 && hasImages)
                        continue;
                    SymColor color = colors[index];
                    List<ClippingObject> fullCoverage = separations[index].SymbolCoverages.SelectMany(c => c.ClippingObjects).ToList();
                    List<AreaSymbol> areas = map.AllSymbols.OfType<AreaSymbol>()
                        .Where(a => a.IsVisible && map.IsSymdefVisible(a.Definition) && AreaDefs(a.Definition).Any(def => HasPrototypeIn(def, color)))
                        .ToList();

                    bool[] drawing = testMap.DrawCoverage(target => map.Draw(target, testMap.Area, CreateOneColorOptions(colors, index), null));
                    int lines = 0, elts = 0, sel = 0;
                    bool[] union = testMap.DrawCoverage(target => {
                        DrawClippingObjects(target, fullCoverage);
                        foreach (AreaSymbol area in areas)
                            foreach (AreaSymDef def in AreaDefs(area.Definition))
                                DrawPrototypes(target, area, def, color, ref lines, ref elts, ref sel);
                    });
                    hatchLines += lines; elements += elts; selectedParts += sel;

                    ++compared;
                    if (areas.Count > 0) ++withPrototypes;
                    int differing = Enumerable.Range(0, drawing.Length).Count(i => drawing[i] != union[i]);
                    TestContext.WriteLine("{0} (id {1}): {2} pixels, {3} areas with prototypes, {4} hatch lines, {5} elements, {6} selected parts, {7} differ",
                                          color.Name, color.OcadId, drawing.Count(c => c), areas.Count, lines, elts, sel, differing);
                    if (differing > 0) {
                        int l2 = 0, e2 = 0, s2 = 0;
                        bool[] ofFull = testMap.DrawCoverage(target => DrawClippingObjects(target, fullCoverage));
                        bool[] ofPrototypes = testMap.DrawCoverage(target => {
                            foreach (AreaSymbol area in areas)
                                foreach (AreaSymDef def in AreaDefs(area.Definition))
                                    DrawPrototypes(target, area, def, color, ref l2, ref e2, ref s2);
                        });
                        foreach (int i in Enumerable.Range(0, drawing.Length).Where(i => drawing[i] != union[i])) {
                            TestContext.WriteLine("  pixel ({0}, {1}): drawing {2}, full coverage {3}, prototypes {4}", i % testMap.Size.Width, i / testMap.Size.Width, drawing[i], ofFull[i], ofPrototypes[i]);
                            float size = testMap.PixelSize;
                            PointF point = new PointF(testMap.Area.Left + (i % testMap.Size.Width + 0.5F) * size, testMap.Area.Bottom - (i / testMap.Size.Width + 0.5F) * size);
                            foreach (Symbol s in map.AllSymbols.Where(s => s.IsVisible && map.IsSymdefVisible(s.Definition) && s.Definition.HasColor(color))) {
                                RectangleF box = s.BoundingBox;
                                box.Inflate(size, size);
                                if (box.Contains(point))
                                    TestContext.WriteLine("    near {0} ({1}): {2} {3}", point, s.Definition.SymbolId, s.Definition.Name, s.GetType().Name);
                            }
                        }
                        string image = testMap.WriteDifference(color, drawing, union);
                        differences.Add(string.Format("{0} (id {1}): {2} pixels differ, see {3}", color.Name, color.OcadId, differing, image));
                    }
                }
            }

            TestContext.WriteLine("{0} colors compared, {1} with prototypes, {2} hatch lines, {3} elements, {4} selected parts", compared, withPrototypes, hatchLines, elements, selectedParts);
            Assert.IsEmpty(differences, "The coverage of the choices differs from the drawing:\n" + string.Join("\n", differences));
        }

        static IEnumerable<AreaSymDef> AreaDefs(SymDef def)
        {
            if (def is AreaSymDef a)
                yield return a;
            else if (def is AreaComboSymDef c)
                foreach (SymDef comp in c.Components)
                    foreach (AreaSymDef x in AreaDefs(comp))
                        yield return x;
        }

        static bool HasPrototypeIn(AreaSymDef def, SymColor color)
        {
            return def.GetHatchings().Any(h => h.hatchColor == color) || def.GetPatterns().Any(p => p.patternGlyph.HasColor(color));
        }

        // The choices: a hatching or a clipped area pattern covers G under M inside the area, and the drawing of G is
        // the lines or the elements that the drawing draws over the bounds of the area; a fill mode that selects the
        // parts of the elements gives them as shapes of the full coverage.
        static void DrawPrototypes(IGraphicsTarget g, AreaSymbol area, AreaSymDef def, SymColor color, ref int lines, ref int elts, ref int sel)
        {
            SymPathWithHoles path = area.Path;
            FilledShape areaShape = path.MakeFilledShape(Placement.Identity);
            object brush = new object();
            g.CreateSolidBrush(brush, CmykColor.FromCmyk(0, 0, 0, 1));

            foreach (AreaSymDef.HatchInfo h in def.GetHatchings()) {
                if (h.hatchColor != color)
                    continue;
                float angle = (h.hatchRotateMode == AreaSymDef.RotateMode.Never) ? 0 : area.Angle;
                PointF rc = (h.hatchRotateMode == AreaSymDef.RotateMode.Never) ? new PointF() : area.RotationCenter;
                Matrix matrix = new Matrix();
                matrix.Translate(rc.X, rc.Y);
                matrix.Rotate(angle);
                matrix.Rotate(h.hatchAngle);

                g.PushClip(((PathShape)areaShape.Shape).Parts.ToList(), areaShape.FillRule);
                g.PushTransform(matrix);
                Matrix inverse = matrix.Clone();
                inverse.Invert();
                RectangleF bounding = Geometry.BoundsOfTransformedRectangle(path.BoundingBox, inverse);
                object pen = new object();
                g.CreatePen(pen, brush, h.hatchWidth, LineCapMode.Flat, LineJoinMode.Miter, GraphicsUtil.MITER_LIMIT);
                float spacing = h.hatchSpacing, offset = h.hatchOffset;
                double firstLine = Math.Round((bounding.Top - offset) / spacing) * spacing + offset;
                double lastLine = (Math.Round((bounding.Bottom - offset) / spacing) + 0.5) * spacing + offset;
                for (double y = firstLine; y <= lastLine; y += spacing) {
                    PathShape line = PathShape.FromPoints(new PointF[] { new PointF(bounding.Left, (float)y), new PointF(bounding.Right, (float)y) }, false);
                    g.DrawPath(pen, line.Parts.ToList());
                    ++lines;
                }
                g.PopTransform();
                g.PopClip();
            }

            foreach (AreaSymDef.PatternInfo p in def.GetPatterns()) {
                if (!p.patternGlyph.HasColor(color))
                    continue;
                bool ignoreRotation = p.patternRotateMode == AreaSymDef.RotateMode.Never;
                float angle = ignoreRotation ? 0 : area.Angle;
                PointF rc = ignoreRotation ? new PointF() : area.RotationCenter;
                bool clip = p.patternFillMode == AreaSymDef.PatternFillMode.Clip;
                if (clip)
                    g.PushClip(((PathShape)areaShape.Shape).Parts.ToList(), areaShape.FillRule);
                Matrix matrix = new Matrix();
                matrix.Translate(rc.X, rc.Y);
                matrix.Rotate(angle);
                matrix.Rotate(p.patternAngle);
                matrix.Translate(p.patternOffsetX, p.patternOffsetY);
                g.PushTransform(matrix);
                Matrix inverse = matrix.Clone();
                inverse.Invert();
                RectangleF bounding = Geometry.BoundsOfTransformedRectangle(path.BoundingBox, inverse);
                SymPathWithHoles transformedPath = path.Transform(inverse);

                foreach (PointF location in Lattice(def, p, bounding)) {
                    if (clip) {
                        DrawClippingObjects(g, p.patternGlyph.MakeClippingObjects(location, -p.patternAngle, null, color));
                        ++elts;
                    }
                    else {
                        Matrix pose = new Matrix();
                        pose.Translate(location.X, location.Y);
                        pose.RotateAt(-p.patternAngle, new PointF(0, 0));
                        Array parts = (Array)partsField.GetValue(p.patternGlyph);
                        ClippingObject[] shared = (ClippingObject[])sharedMethod.Invoke(p.patternGlyph, null);
                        List<ClippingObject> selected = new List<ClippingObject>();
                        for (int i = 0; i < parts.Length; ++i) {
                            Glyph.GlyphPart part = (Glyph.GlyphPart)parts.GetValue(i);
                            if (part.color != color || shared[i] == null)
                                continue;
                            if (IsSelected(p.patternFillMode, part, pose, transformedPath))
                                selected.Add(shared[i].WithPlacement(Placement.Identity.Within(pose)));
                        }
                        DrawClippingObjects(g, selected);
                        sel += selected.Count;
                    }
                }

                g.PopTransform();
                if (clip)
                    g.PopClip();
            }
        }

        // The selection of a part of an element, as Glyph.GlyphPart.Draw asks its filter and DrawPatternRows filters.
        static bool IsSelected(AreaSymDef.PatternFillMode mode, Glyph.GlyphPart part, Matrix pose, SymPathWithHoles transformedPath)
        {
            PointF[] points;
            PointF center;
            switch (part.kind.ToString()) {
                case "Line":
                    if (!(part.lineWidth > 0 && !part.path.IsZeroLength)) return false;
                    points = part.path.Points; center = part.path.AreaCentroid();
                    break;
                case "Area":
                    points = part.areaPath.MainPath.Points; center = part.areaPath.MainPath.AreaCentroid();
                    break;
                case "Circle":
                    if (!(part.lineWidth > 0 && part.circleDiam > 0)) return false;
                    points = (PointF[])circleBorderPoints.Invoke(part, null); center = part.point;
                    break;
                default:
                    if (!(part.circleDiam > 0)) return false;
                    points = (PointF[])circleBorderPoints.Invoke(part, null); center = part.point;
                    break;
            }
            points = Geometry.TransformPoints(points, pose);
            center = Geometry.TransformPoint(center, pose);
            switch (mode) {
                case AreaSymDef.PatternFillMode.CompletelyInside: return points.All(pt => transformedPath.IsInside(pt));
                case AreaSymDef.PatternFillMode.CenterInside: return transformedPath.IsInside(center);
                case AreaSymDef.PatternFillMode.PartiallyInside: return points.Any(pt => transformedPath.IsInside(pt));
                default: throw new InvalidOperationException();
            }
        }

        // The lattice points of DrawPatternRows, in its order, with the irregularity of HandleIrregularity.
        static IEnumerable<PointF> Lattice(AreaSymDef def, AreaSymDef.PatternInfo patternInfo, RectangleF boundingRect)
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
                        yield return (PointF)handleIrregularity.Invoke(def, new object[] { patternInfo, x, y });
                }
                else {
                    for (double x = leftLine; x <= rightLine; x += patternInfo.patternWidth)
                        yield return (PointF)handleIrregularity.Invoke(def, new object[] { patternInfo, x, y });
                }
                if (patternInfo.offsetRows)
                    offsetThisLine = !offsetThisLine;
            }
        }

        static RenderOptions CreateOneColorOptions(List<SymColor> colors, int index)
        {
            RenderOptions renderOpts = new RenderOptions();
            renderOpts.minResolution = 0;
            renderOpts.renderTemplates = RenderTemplateOption.MapOnly;
            renderOpts.blendOverprintedColors = false;
            renderOpts.colorBeginDrawExclusive = (index == 0) ? (int?)null : colors[index - 1].OcadId;
            renderOpts.colorEndDrawInclusive = colors[index].OcadId;
            return renderOpts;
        }

        static void DrawClippingObjects(IGraphicsTarget g, IEnumerable<ClippingObject> clippingObjects)
        {
            object brush = new object();
            g.CreateSolidBrush(brush, CmykColor.FromCmyk(0, 0, 0, 1));
            Dictionary<Stroke, object> pens = new Dictionary<Stroke, object>();
            Dictionary<(string, float, TextEffects), object> fonts = new Dictionary<(string, float, TextEffects), object>();

            foreach ((ClippingObject clippingObject, bool isExpanded) in SymbolCoverageComparison.Expand(clippingObjects.ToList())) {
                IReadOnlyList<Matrix> steps = clippingObject.Placement.Steps;
                foreach (Matrix step in steps)
                    g.PushTransform(step);

                if (clippingObject is FilledShape filledShape) {
                    Shape shape = filledShape.Shape;
                    if (shape is PathShape path)
                        g.FillPath(brush, path.Parts.ToList(), filledShape.FillRule);
                    else if (shape is EllipseShape ellipse)
                        g.FillEllipse(brush, ellipse.Center, ellipse.RadiusX, ellipse.RadiusY);
                    else if (shape is TextShape text)
                        DrawTextRun(g, text, upperLeft => g.DrawText(text.Text, GetFont(g, text, fonts), brush, upperLeft));
                }
                else {
                    StrokedShape strokedShape = (StrokedShape)clippingObject;
                    object pen;
                    if (!pens.TryGetValue(strokedShape.Stroke, out pen)) {
                        pen = new object();
                        Stroke stroke = strokedShape.Stroke;
                        g.CreatePen(pen, brush, stroke.Width, stroke.Caps, stroke.Join, stroke.MiterLimit);
                        pens.Add(stroke, pen);
                    }
                    Shape shape = strokedShape.Shape;
                    if (shape is PathShape path)
                        g.DrawPath(pen, path.Parts.ToList());
                    else if (shape is EllipseShape ellipse)
                        g.DrawEllipse(pen, ellipse.Center, ellipse.RadiusX, ellipse.RadiusY);
                    else if (shape is ArcShape arc)
                        g.DrawArc(pen, arc.Center, arc.Radius, arc.StartAngle, arc.SweepAngle);
                    else if (shape is TextShape text)
                        DrawTextRun(g, text, upperLeft => g.DrawTextOutline(text.Text, GetFont(g, text, fonts), pen, upperLeft));
                }

                for (int i = 0; i < steps.Count; ++i)
                    g.PopTransform();
            }
        }

        static void DrawTextRun(IGraphicsTarget g, TextShape text, Action<PointF> draw)
        {
            Matrix mirror = new Matrix();
            mirror.Scale(1, -1);
            g.PushTransform(mirror);
            draw(new PointF(text.UpperLeft.X, -text.UpperLeft.Y));
            g.PopTransform();
        }

        static object GetFont(IGraphicsTarget g, TextShape text, Dictionary<(string, float, TextEffects), object> fonts)
        {
            object font;
            if (!fonts.TryGetValue((text.FontFamily, text.EmHeight, text.Effects), out font)) {
                font = new object();
                g.CreateFont(font, text.FontFamily, text.EmHeight, text.Effects);
                fonts.Add((text.FontFamily, text.EmHeight, text.Effects), font);
            }
            return font;
        }

        sealed class TestMap
        {
            public readonly string Name;
            public readonly Map Map;
            public readonly RectangleF Area;
            public readonly Size Size;

            private TestMap(string name, Map map, RectangleF area, Size size)
            {
                Name = name; Map = map; Area = area; Size = size;
            }

            public float PixelSize => Area.Width / Size.Width;

            public static TestMap Read(string fileName)
            {
                string description = TestUtil.GetTestFile(Path.Combine("skia_render", fileName));
                string[] lines = File.ReadAllLines(description);
                float[] area = lines[2].Split(',').Select(value => float.Parse(value, CultureInfo.InvariantCulture)).ToArray();
                int[] size = lines[3].Split(',').Select(value => int.Parse(value, CultureInfo.InvariantCulture)).ToArray();
                string directory = Path.GetDirectoryName(description);
                Map map = new Map(new Skia_TextMetrics(), new Skia_FileLoader(directory));
                InputOutput.ReadFile(Path.Combine(directory, lines[0].Trim()), map);
                return new TestMap(Path.GetFileNameWithoutExtension(fileName), map, new RectangleF(area[0], area[3], area[2] - area[0], area[1] - area[3]), new Size(size[0], size[1]));
            }

            public bool[] DrawCoverage(Action<IGraphicsTarget> draw)
            {
                using (Skia_BitmapGraphicsTarget target = new Skia_BitmapGraphicsTarget(Size.Width, Size.Height, true, CmykColor.FromCmyka(0, 0, 0, 0, 0), Area, true)) {
                    target.PushAntiAliasing(false);
                    draw(target);
                    target.PopAntiAliasing();
                    using (Skia_Bitmap bitmap = (Skia_Bitmap)target.FinishBitmap())
                        return bitmap.Bitmap.Pixels.Select(pixel => pixel.Alpha != 0).ToArray();
                }
            }

            public string WriteDifference(SymColor color, bool[] drawing, bool[] clippingObjects)
            {
                string path = Path.Combine(TestContext.CurrentContext.WorkDirectory, "pattern-prototype-check", string.Format("{0}_{1}.png", Name, color.OcadId));
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                using (SKBitmap image = new SKBitmap(Size.Width, Size.Height)) {
                    for (int i = 0; i < drawing.Length; ++i) {
                        SKColor pixel = drawing[i] ? (clippingObjects[i] ? new SKColor(160, 160, 160) : SKColors.Red) : (clippingObjects[i] ? SKColors.Blue : SKColors.White);
                        image.SetPixel(i % Size.Width, i / Size.Width, pixel);
                    }
                    using (FileStream stream = File.Create(path))
                        image.Encode(stream, SKEncodedImageFormat.Png, 100);
                }
                return path;
            }
        }
    }
}
