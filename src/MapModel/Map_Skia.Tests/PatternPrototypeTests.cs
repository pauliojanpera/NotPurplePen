using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using PurplePen.Graphics2D;
using PurplePen.MapModel;

namespace Map_Skia.Tests
{
    // The coverage prototypes of the hatchings and the area patterns (HatchingPrototype, AreaPatternPrototype), checked
    // without a drawing: the coverage prototypes of a definition and their coverage (CoveragePrototype.Covers), the
    // transformation M of the configurations with each rotate mode, the palette cutters of the configurations, the area
    // as the clipping object, the expansion of the coverage prototypes, the glyph parts that a fill mode selects, and the
    // hatchings and the area patterns of the component definitions of combined areas. Each test makes its map, for the
    // cases that the maps of the coverage comparison do not have (PaletteSeparationCoverageTests).
    [TestFixture, Parallelizable(ParallelScope.Children)]
    public class PatternPrototypeTests
    {
        const float Tolerance = 0.0001F;

        // A hatching and an area pattern of an area symbol definition are coverage prototypes of the definition: each
        // refers to the definition and to its index, the area pattern also to its palette color, and each area symbol of
        // the definition refers to the same object, also through a combined area whose component definition the
        // definition is. The coverage prototype reads the hatching and the area pattern from the definition.
        [Test]
        public void HatchingsAndAreaPatternsAreCoveragePrototypesOfTheirDefinition()
        {
            Map map = CreateMap();
            SymColor brown;
            AreaSymDef forest;
            AreaSymbol first, second, combined;
            using (map.Write()) {
                SymColor green = AddColor(map, "Green", 1);
                SymColor blue = AddColor(map, "Blue", 2);
                brown = AddColor(map, "Brown", 3);
                forest = new AreaSymDef("Forest", "1", green, null);
                forest.AddHatching(new AreaSymDef.HatchInfo() { hatchColor = blue, hatchWidth = 0.1F, hatchSpacing = 0.5F, hatchAngle = 45 });
                forest.AddPattern(new AreaSymDef.PatternInfo() { patternGlyph = CreateGlyph(glyph => glyph.AddFilledCircle(brown, new PointF(0, 0), 0.3F)), patternWidth = 1, patternHeight = 1 });
                map.AddSymdef(forest);
                AreaComboSymDef combo = new AreaComboSymDef("Combined", "2", new SymDef[] { forest });
                map.AddSymdef(combo);
                first = AddArea(map, forest, Square(0, 0, 4), 0, new PointF());
                second = AddArea(map, forest, Square(6, 0, 4), 30, new PointF(1, 1));
                combined = AddArea(map, combo, Square(12, 0, 4), 0, new PointF());
            }

            HatchingPrototype hatching;
            AreaPatternPrototype pattern;
            using (map.Read()) {
                IReadOnlyList<PaletteSeparation> separations = map.PaletteSeparations;
                IReadOnlyList<SymbolCoverage> hatchings = separations[1].SymbolCoverages;
                IReadOnlyList<SymbolCoverage> patterns = separations[2].SymbolCoverages;
                CollectionAssert.AreEqual(new Symbol[] { first, second, combined }, hatchings.Select(coverage => coverage.Symbol));
                CollectionAssert.AreEqual(new Symbol[] { first, second, combined }, patterns.Select(coverage => coverage.Symbol));

                hatching = (HatchingPrototype)hatchings[0].Prototype;
                pattern = (AreaPatternPrototype)patterns[0].Prototype;
                foreach (SymbolCoverage coverage in hatchings)
                    Assert.AreSame(hatching, coverage.Prototype);
                foreach (SymbolCoverage coverage in patterns)
                    Assert.AreSame(pattern, coverage.Prototype);

                Assert.AreSame(forest, hatching.Definition);
                Assert.AreEqual(0, hatching.Index);
                Assert.AreEqual(forest.GetHatchings().Single(), hatching.Hatching);
                Assert.AreSame(forest, pattern.Definition);
                Assert.AreEqual(0, pattern.Index);
                Assert.AreSame(brown, pattern.Color);
                Assert.AreEqual(forest.GetPatterns().Single(), pattern.Pattern);
            }

            // A change of the map makes the palette separations again, and the coverage prototypes, which depend on the
            // definition alone, stay.
            using (map.Write())
                AddArea(map, forest, Square(18, 0, 4), 0, new PointF());
            using (map.Read()) {
                Assert.AreEqual(4, map.PaletteSeparations[1].SymbolCoverages.Count);
                foreach (SymbolCoverage coverage in map.PaletteSeparations[1].SymbolCoverages)
                    Assert.AreSame(hatching, coverage.Prototype);
                foreach (SymbolCoverage coverage in map.PaletteSeparations[2].SymbolCoverages)
                    Assert.AreSame(pattern, coverage.Prototype);
            }
        }

        // The coverage of a hatching is stripes of its width about the lines y = k · spacing. Its offset and its angle are
        // in M, and not in G.
        [Test]
        public void CoverageOfAHatchingIsItsStripesAboutTheLinesOfItsSpacing()
        {
            Map map = CreateMap();
            using (map.Write()) {
                SymColor blue = AddColor(map, "Blue", 1);
                AreaSymDef marsh = new AreaSymDef("Marsh", "1", null, null);
                marsh.AddHatching(new AreaSymDef.HatchInfo() { hatchColor = blue, hatchWidth = 0.2F, hatchSpacing = 1, hatchOffset = 0.3F, hatchAngle = 20 });
                map.AddSymdef(marsh);
                AddArea(map, marsh, Square(0, 0, 4), 0, new PointF());
            }

            using (map.Read()) {
                CoveragePrototype hatching = map.PaletteSeparations[0].SymbolCoverages.Single().Prototype;
                foreach (PointF covered in new PointF[] { new PointF(5, 0), new PointF(-7, 0.09F), new PointF(3, 2.95F), new PointF(0, -4.08F), new PointF(1000, 17.01F) })
                    Assert.IsTrue(hatching.Covers(covered), "{0} is covered", covered);
                foreach (PointF uncovered in new PointF[] { new PointF(5, 0.11F), new PointF(5, 0.5F), new PointF(5, -1.2F), new PointF(0, 0.3F) })
                    Assert.IsFalse(hatching.Covers(uncovered), "{0} is not covered", uncovered);
            }
        }

        // The coverage of an area pattern in a palette color is the glyph parts of its element in the palette color at the
        // points of its lattice, x = i · patternWidth and y = j · patternHeight, the points of each odd row patternWidth /
        // 2 aside when the rows are offset. The element is turned by -patternAngle. An element of glyph parts in two
        // palette colors gives the area pattern a coverage prototype in each of them, and each covers its own glyph parts.
        [Test]
        public void CoverageOfAnAreaPatternIsItsGlyphPartsInThePaletteColorAtThePointsOfItsLattice()
        {
            Map map = CreateMap();
            AreaSymbol offsetRows, turned;
            SymColor brown, black;
            using (map.Write()) {
                brown = AddColor(map, "Brown", 1);
                black = AddColor(map, "Black", 2);
                Func<Glyph> createElement = () => CreateGlyph(glyph => {
                    glyph.AddFilledCircle(brown, new PointF(0, 0), 0.4F);
                    glyph.AddLine(black, new SymPath(new PointF[] { new PointF(0.25F, 0), new PointF(0.45F, 0) }), 0.1F, LineJoinMode.Miter, LineCapMode.Flat);
                });
                AreaSymDef rows = new AreaSymDef("Offset rows", "1", null, null);
                rows.AddPattern(new AreaSymDef.PatternInfo() { patternGlyph = createElement(), patternWidth = 2, patternHeight = 1, offsetRows = true });
                map.AddSymdef(rows);
                AreaSymDef turnedElements = new AreaSymDef("Turned", "2", null, null);
                turnedElements.AddPattern(new AreaSymDef.PatternInfo() { patternGlyph = createElement(), patternWidth = 2, patternHeight = 1, patternAngle = 90 });
                map.AddSymdef(turnedElements);
                offsetRows = AddArea(map, rows, Square(0, 0, 4), 0, new PointF());
                turned = AddArea(map, turnedElements, Square(6, 0, 4), 0, new PointF());
            }

            using (map.Read()) {
                IReadOnlyList<PaletteSeparation> separations = map.PaletteSeparations;
                CollectionAssert.AreEqual(new Symbol[] { offsetRows, turned }, separations[0].SymbolCoverages.Select(coverage => coverage.Symbol));
                CollectionAssert.AreEqual(new Symbol[] { offsetRows, turned }, separations[1].SymbolCoverages.Select(coverage => coverage.Symbol));
                AreaPatternPrototype brownElements = (AreaPatternPrototype)separations[0].SymbolCoverages[0].Prototype;
                AreaPatternPrototype blackElements = (AreaPatternPrototype)separations[1].SymbolCoverages[0].Prototype;
                AreaPatternPrototype turnedBlackElements = (AreaPatternPrototype)separations[1].SymbolCoverages[1].Prototype;
                Assert.AreNotSame(brownElements, blackElements);
                Assert.AreEqual(brownElements.Index, blackElements.Index);
                Assert.AreSame(brown, brownElements.Color);
                Assert.AreSame(black, blackElements.Color);

                // Brown: the circles at (2i, j) on the even rows and at (2i + 1, j) on the odd rows.
                foreach (PointF covered in new PointF[] { new PointF(0, 0), new PointF(4, -2), new PointF(1, 1), new PointF(-3, -1), new PointF(0.15F, 0.1F) })
                    Assert.IsTrue(brownElements.Covers(covered), "{0} is covered by the brown glyph parts", covered);
                foreach (PointF uncovered in new PointF[] { new PointF(1, 0), new PointF(0, 1), new PointF(0.35F, 0) })
                    Assert.IsFalse(brownElements.Covers(uncovered), "{0} is not covered by the brown glyph parts", uncovered);

                // Black: the lines with flat caps beside the circles.
                foreach (PointF covered in new PointF[] { new PointF(0.35F, 0), new PointF(1.35F, 1.04F), new PointF(2.26F, -2.03F) })
                    Assert.IsTrue(blackElements.Covers(covered), "{0} is covered by the black glyph parts", covered);
                foreach (PointF uncovered in new PointF[] { new PointF(0.35F, 0.06F), new PointF(0.2F, 0), new PointF(0, 0), new PointF(0.5F, 0) })
                    Assert.IsFalse(blackElements.Covers(uncovered), "{0} is not covered by the black glyph parts", uncovered);

                // The angle 90 turns the element by -90 degrees: the line goes down from its lattice point.
                foreach (PointF covered in new PointF[] { new PointF(0, -0.35F), new PointF(2, 0.65F) })
                    Assert.IsTrue(turnedBlackElements.Covers(covered), "{0} is covered by the turned black glyph parts", covered);
                Assert.IsFalse(turnedBlackElements.Covers(new PointF(0.35F, 0)));
            }
        }

        // An irregular area pattern moves each element by a random but deterministic distance from its lattice point, as
        // the drawing moves it. The expansion of the area pattern places the elements where the drawing draws them, and
        // the coverage prototype covers the center of each of them, also where the element is moved away from its lattice
        // point.
        [Test]
        public void CoverageOfAnIrregularAreaPatternHasItsElementsWhereTheDrawingMovesThem()
        {
            Map map = CreateMap();
            using (map.Write()) {
                SymColor brown = AddColor(map, "Brown", 1);
                AreaSymDef stony = new AreaSymDef("Stony", "1", null, null);
                stony.AddPattern(new AreaSymDef.PatternInfo() {
                    patternGlyph = CreateGlyph(glyph => glyph.AddFilledCircle(brown, new PointF(0, 0), 0.2F)), patternWidth = 2, patternHeight = 2,
                    irregular = true, irregularVarX = 1, irregularVarY = 1 });
                map.AddSymdef(stony);
                AddArea(map, stony, Square(-5, -5, 10), 0, new PointF());
            }

            using (map.Read()) {
                SymbolCoverage coverage = map.PaletteSeparations[0].SymbolCoverages.Single();
                List<PointF> centers = coverage.ExpandPrototype().Select(element => ((EllipseShape)element.Shape).Center).ToList();
                Assert.Greater(centers.Count, 20, "elements");
                foreach (PointF center in centers)
                    Assert.IsTrue(coverage.Prototype.Covers(center), "the element at {0}", center);

                int moved = centers.Count(center => Distance(center, new PointF((float)Math.Round(center.X / 2) * 2, (float)Math.Round(center.Y / 2) * 2)) > 0.1);
                TestContext.WriteLine("{0} elements, {1} of them moved farther than their radius", centers.Count, moved);
                Assert.Greater(moved, centers.Count / 2, "the elements that move away from their lattice points");
            }
        }

        // A cross hatching is two hatchings of one palette color, so an area of it has two palette color coverages in the
        // palette color, one for each hatching, in the order of the hatchings.
        [Test]
        public void CrossHatchingIsTwoCoveragePrototypes()
        {
            Map map = CreateMap();
            AreaSymbol area;
            using (map.Write()) {
                SymColor green = AddColor(map, "Green", 1);
                SymColor purple = AddColor(map, "Purple", 2);
                AreaSymDef dangerous = new AreaSymDef("Dangerous area", "1", green, null);
                dangerous.AddHatching(new AreaSymDef.HatchInfo() { hatchColor = purple, hatchWidth = 0.38F, hatchSpacing = 1.2F, hatchAngle = -45 });
                dangerous.AddHatching(new AreaSymDef.HatchInfo() { hatchColor = purple, hatchWidth = 0.38F, hatchSpacing = 1.2F, hatchAngle = 45 });
                map.AddSymdef(dangerous);
                area = AddArea(map, dangerous, Square(0, 0, 10), 0, new PointF());
            }

            using (map.Read()) {
                IReadOnlyList<SymbolCoverage> coverages = map.PaletteSeparations[1].SymbolCoverages;
                Assert.AreEqual(2, coverages.Count);
                CollectionAssert.AreEqual(new int[] { 0, 1 }, coverages.Select(coverage => ((HatchingPrototype)coverage.Prototype).Index));
                foreach (SymbolCoverage coverage in coverages)
                    Assert.AreSame(area, coverage.Symbol);
                AssertTransformation(Rotation(-45), coverages[0].Transformation);
                AssertTransformation(Rotation(45), coverages[1].Transformation);
                Assert.AreEqual(2, map.PaletteSeparations[1].Cutters.Count, "the palette cutters of the two hatchings");
            }
        }

        // M of a hatching is T(c) · R(a) · R(hatchAngle) · T(0, hatchOffset), and M of an area pattern is T(c) · R(a) ·
        // R(patternAngle) · T(patternOffsetX, patternOffsetY), where c is the rotation center and a the angle of the area
        // symbol. ManualOnly is the same as Always, and Never leaves out c and a. M of the full coverage is the identity.
        [TestCase(AreaSymDef.RotateMode.Always)]
        [TestCase(AreaSymDef.RotateMode.ManualOnly)]
        [TestCase(AreaSymDef.RotateMode.Never)]
        public void TransformationIsTheAngleAndTheRotationCenterOfTheAreaAndTheAngleAndTheOffsetOfThePatternFill(AreaSymDef.RotateMode rotateMode)
        {
            Map map = CreateMap();
            using (map.Write()) {
                SymColor green = AddColor(map, "Green", 1);
                SymColor blue = AddColor(map, "Blue", 2);
                SymColor brown = AddColor(map, "Brown", 3);
                AreaSymDef forest = new AreaSymDef("Forest", "1", green, null);
                forest.AddHatching(new AreaSymDef.HatchInfo() { hatchColor = blue, hatchWidth = 0.1F, hatchSpacing = 0.5F, hatchOffset = 0.25F, hatchAngle = 30, hatchRotateMode = rotateMode });
                forest.AddPattern(new AreaSymDef.PatternInfo() {
                    patternGlyph = CreateGlyph(glyph => glyph.AddFilledCircle(brown, new PointF(0, 0), 0.3F)), patternWidth = 1, patternHeight = 1,
                    patternAngle = 30, patternOffsetX = 0.3F, patternOffsetY = 0.4F, patternRotateMode = rotateMode });
                map.AddSymdef(forest);
                AddArea(map, forest, Square(0, 0, 10), 20, new PointF(5, 7));
            }

            bool turns = rotateMode != AreaSymDef.RotateMode.Never;
            Matrix hatching = new Matrix();
            if (turns) {
                hatching.Translate(5, 7);
                hatching.Rotate(20);
            }
            hatching.Rotate(30);
            Matrix pattern = hatching.Clone();
            hatching.Translate(0, 0.25F);
            pattern.Translate(0.3F, 0.4F);

            using (map.Read()) {
                IReadOnlyList<PaletteSeparation> separations = map.PaletteSeparations;
                Assert.IsTrue(separations[0].SymbolCoverages.Single().Transformation.IsIdentity, "M of the full coverage");
                AssertTransformation(hatching, separations[1].SymbolCoverages.Single().Transformation);
                AssertTransformation(pattern, separations[2].SymbolCoverages.Single().Transformation);
            }
        }

        // The palette cutters of a palette separation are its configurations: the palette color coverages with the same
        // coverage prototype and the same normal form of M are one palette cutter. Areas of one definition with the same
        // angle and rotation center are one configuration, and so are areas whose M differ by a vector of the lattice of
        // the area pattern, or for the hatching by any distance along its lines and by a multiple of its spacing across
        // them. A rotation center that moves the lattice by another vector, and another angle, give configurations of
        // their own. Of the translations of an irregular area pattern only the identity is certain to be a symmetry, so
        // its areas share a palette cutter only with the same M. The palette color coverages with the full coverage are
        // one palette cutter whose M is the identity.
        [Test]
        public void PaletteCuttersAreTheConfigurationsOfTheirNormalForms()
        {
            Map map = CreateMap();
            AreaSymbol[] areas = new AreaSymbol[7];
            AreaSymbol[] irregularAreas = new AreaSymbol[3];
            using (map.Write()) {
                SymColor green = AddColor(map, "Green", 1);
                SymColor blue = AddColor(map, "Blue", 2);
                SymColor brown = AddColor(map, "Brown", 3);
                AreaSymDef forest = new AreaSymDef("Forest", "1", green, null);
                forest.AddHatching(new AreaSymDef.HatchInfo() { hatchColor = blue, hatchWidth = 0.2F, hatchSpacing = 1, hatchRotateMode = AreaSymDef.RotateMode.Always });
                forest.AddPattern(new AreaSymDef.PatternInfo() {
                    patternGlyph = CreateGlyph(glyph => glyph.AddFilledCircle(brown, new PointF(0, 0), 0.3F)), patternWidth = 2, patternHeight = 1, offsetRows = true,
                    patternRotateMode = AreaSymDef.RotateMode.Always });
                map.AddSymdef(forest);
                AreaSymDef stony = new AreaSymDef("Stony", "2", null, null);
                stony.AddPattern(new AreaSymDef.PatternInfo() {
                    patternGlyph = CreateGlyph(glyph => glyph.AddFilledCircle(brown, new PointF(0, 0), 0.2F)), patternWidth = 2, patternHeight = 2,
                    irregular = true, irregularVarX = 1, irregularVarY = 1, patternRotateMode = AreaSymDef.RotateMode.Always });
                map.AddSymdef(stony);

                PointF[] rotationCenters = { new PointF(0, 0), new PointF(0, 0), new PointF(0, 3), new PointF(0.7F, 0), new PointF(4, 0), new PointF(1, 3), new PointF(0, 0) };
                for (int i = 0; i < areas.Length; ++i)
                    areas[i] = AddArea(map, forest, Square(10 * i, 0, 5), (i == 6) ? 45 : 0, rotationCenters[i]);
                PointF[] irregularCenters = { new PointF(0, 0), new PointF(2, 0), new PointF(0, 0) };
                for (int i = 0; i < irregularAreas.Length; ++i)
                    irregularAreas[i] = AddArea(map, stony, Square(10 * i, 20, 5), 0, irregularCenters[i]);
            }

            using (map.Read()) {
                IReadOnlyList<PaletteSeparation> separations = map.PaletteSeparations;
                // Green: the area fills are one palette cutter.
                PaletteCutter fills = separations[0].Cutters.Single();
                Assert.AreSame(CoveragePrototype.FullCoverage, fills.Prototype);
                Assert.IsTrue(fills.Transformation.IsIdentity);
                CollectionAssert.AreEqual(areas, fills.SymbolCoverages.Select(coverage => coverage.Symbol));

                // Blue: the hatching moves along its lines or by its spacing, apart from the turned area.
                AssertCutters(separations[1], new int[][] { new int[] { 0, 1, 2, 3, 4, 5 }, new int[] { 6 } }, areas);
                AssertTransformation(new Matrix(), separations[1].Cutters[0].Transformation);
                AssertTransformation(Rotation(45), separations[1].Cutters[1].Transformation);

                // Brown: the lattice of the area pattern has the basis (2, 0) and (1, 1), as its rows are offset. The
                // rotation centers (4, 0) and (1, 3) are vectors of the lattice, and (0, 3) and (0.7, 0) are not.
                IReadOnlyList<PaletteCutter> cutters = separations[2].Cutters;
                Assert.AreEqual(4 + 2, cutters.Count, "the palette cutters of the area patterns");
                List<Symbol>[] cuttersOfForest = cutters.Where(cutter => ((AreaPatternPrototype)cutter.Prototype).Definition.Name == "Forest")
                                                        .Select(cutter => cutter.SymbolCoverages.Select(coverage => coverage.Symbol).ToList()).ToArray();
                Assert.AreEqual(4, cuttersOfForest.Length);
                CollectionAssert.AreEqual(new Symbol[] { areas[0], areas[1], areas[4], areas[5] }, cuttersOfForest[0]);
                CollectionAssert.AreEqual(new Symbol[] { areas[2] }, cuttersOfForest[1]);
                CollectionAssert.AreEqual(new Symbol[] { areas[3] }, cuttersOfForest[2]);
                CollectionAssert.AreEqual(new Symbol[] { areas[6] }, cuttersOfForest[3]);
                AssertTransformation(new Matrix(), cutters[0].Transformation);
                // The rotation center (0, 3) is the translation 3 · (1, 1) - 1.5 · (2, 0), whose normal form is
                // 0.5 · (2, 0).
                AssertTransformation(new Matrix(1, 0, 0, 1, 1, 0), cutters[1].Transformation);

                // The irregular area pattern: the areas with the same M are one palette cutter, and the area whose M moves
                // its lattice by a lattice vector is another.
                PaletteCutter[] cuttersOfStony = cutters.Where(cutter => ((AreaPatternPrototype)cutter.Prototype).Definition.Name == "Stony").ToArray();
                Assert.AreEqual(2, cuttersOfStony.Length);
                CollectionAssert.AreEqual(new Symbol[] { irregularAreas[0], irregularAreas[2] }, cuttersOfStony[0].SymbolCoverages.Select(coverage => coverage.Symbol));
                CollectionAssert.AreEqual(new Symbol[] { irregularAreas[1] }, cuttersOfStony[1].SymbolCoverages.Select(coverage => coverage.Symbol));
                AssertTransformation(new Matrix(1, 0, 0, 1, 2, 0), cuttersOfStony[1].Transformation);
            }
        }

        // The clipping object of the palette color coverage of a hatching and of a clipped area pattern is the area: one
        // filled shape of the main path and the holes with the alternate fill rule, at the identity placement, the same
        // shape as the area fill.
        [Test]
        public void ClippingObjectOfAHatchingAndOfAClippedAreaPatternIsTheAreaWithItsHoles()
        {
            Map map = CreateMap();
            using (map.Write()) {
                SymColor green = AddColor(map, "Green", 1);
                SymColor blue = AddColor(map, "Blue", 2);
                SymColor brown = AddColor(map, "Brown", 3);
                AreaSymDef forest = new AreaSymDef("Forest", "1", green, null);
                forest.AddHatching(new AreaSymDef.HatchInfo() { hatchColor = blue, hatchWidth = 0.1F, hatchSpacing = 0.5F, hatchAngle = 45 });
                forest.AddPattern(new AreaSymDef.PatternInfo() { patternGlyph = CreateGlyph(glyph => glyph.AddFilledCircle(brown, new PointF(0, 0), 0.3F)), patternWidth = 1, patternHeight = 1 });
                map.AddSymdef(forest);
                AddArea(map, forest, new SymPathWithHoles(Square(0, 0, 10), new SymPath[] { Square(3, 3, 4) }), 30, new PointF(2, 2));
            }

            using (map.Read()) {
                IReadOnlyList<PaletteSeparation> separations = map.PaletteSeparations;
                FilledShape fill = (FilledShape)separations[0].SymbolCoverages.Single().ClippingObjects.Single();
                foreach (PaletteSeparation separation in separations.Skip(1)) {
                    SymbolCoverage coverage = separation.SymbolCoverages.Single();
                    Assert.AreNotSame(CoveragePrototype.FullCoverage, coverage.Prototype);
                    FilledShape area = (FilledShape)coverage.ClippingObjects.Single();
                    Assert.AreEqual(AreaFillMode.Alternate, area.FillRule);
                    Assert.AreEqual(0, area.Placement.Steps.Count);
                    PathShape path = (PathShape)area.Shape;
                    Assert.AreEqual(2, path.Parts.Count(part => part.Kind == GraphicsPathPartKind.Start), "the figures of the main path and the hole");
                    CollectionAssert.AreEqual(((PathShape)fill.Shape).Parts.SelectMany(part => part.Points ?? new PointF[0]), path.Parts.SelectMany(part => part.Points ?? new PointF[0]));
                }
            }
        }

        // The expansion of a hatching is the hatch lines over the bounds of the area, as the drawing draws them: lines of
        // the pen of the hatching, the width with flat caps and miter joins, at the places y = hatchOffset + k ·
        // hatchSpacing, in the coordinates of the one step T(c) · R(a) · R(hatchAngle). The expansion of a clipped area
        // pattern is its elements: the shared shapes of the glyph parts in the palette color at a placement whose steps
        // are M and the pose of the element, at its lattice point with the angle -patternAngle. A glyph of one circle at
        // its origin is a circle at the lattice point within M alone, as the drawing draws it.
        [Test]
        public void ExpansionIsTheHatchLinesAndTheElementsAsTheDrawingDrawsThem()
        {
            Map map = CreateMap();
            using (map.Write()) {
                SymColor blue = AddColor(map, "Blue", 1);
                SymColor brown = AddColor(map, "Brown", 2);
                SymColor black = AddColor(map, "Black", 3);
                AreaSymDef marsh = new AreaSymDef("Marsh", "1", null, null);
                marsh.AddHatching(new AreaSymDef.HatchInfo() { hatchColor = blue, hatchWidth = 0.2F, hatchSpacing = 1, hatchOffset = 0.25F, hatchAngle = 30 });
                marsh.AddPattern(new AreaSymDef.PatternInfo() {
                    patternGlyph = CreateGlyph(glyph => {
                        glyph.AddFilledCircle(brown, new PointF(0.2F, 0), 0.2F);
                        glyph.AddLine(brown, new SymPath(new PointF[] { new PointF(-0.3F, -0.3F), new PointF(-0.1F, -0.3F) }), 0.05F, LineJoinMode.Miter, LineCapMode.Flat);
                    }), patternWidth = 1, patternHeight = 1, patternAngle = 40 });
                marsh.AddPattern(new AreaSymDef.PatternInfo() { patternGlyph = CreateGlyph(glyph => glyph.AddFilledCircle(black, new PointF(0, 0), 0.3F)), patternWidth = 1.5F, patternHeight = 1.5F });
                map.AddSymdef(marsh);
                AddArea(map, marsh, Square(0, 0, 10), 15, new PointF(2, 3));
            }

            Matrix lineStep = new Matrix();
            lineStep.Translate(2, 3);
            lineStep.Rotate(15);
            Matrix elementStep = lineStep.Clone();
            lineStep.Rotate(30);
            elementStep.Rotate(40);
            Matrix circleStep = new Matrix();
            circleStep.Translate(2, 3);
            circleStep.Rotate(15);

            using (map.Read()) {
                IReadOnlyList<PaletteSeparation> separations = map.PaletteSeparations;

                // The hatch lines.
                List<StrokedShape> lines = separations[0].SymbolCoverages.Single().ExpandPrototype().Cast<StrokedShape>().ToList();
                Assert.Greater(lines.Count, 10, "hatch lines");
                float previous = float.NaN;
                foreach (StrokedShape line in lines) {
                    Assert.AreEqual(0.2F, line.Stroke.Width);
                    Assert.AreEqual(LineCapMode.Flat, line.Stroke.Caps);
                    Assert.AreEqual(LineJoinMode.Miter, line.Stroke.Join);
                    Assert.AreEqual(GraphicsUtil.MITER_LIMIT, line.Stroke.MiterLimit);
                    Assert.IsNull(line.Stroke.DashPattern);
                    AssertTransformation(lineStep, line.Placement.Steps.Single());
                    PointF[] points = ((PathShape)line.Shape).Parts.SelectMany(part => part.Points).ToArray();
                    Assert.AreEqual(2, points.Length);
                    Assert.AreEqual(points[0].Y, points[1].Y);
                    Assert.AreEqual(0.25, points[0].Y - Math.Floor(points[0].Y), Tolerance, "the offset of the line at {0}", points[0].Y);
                    if (!float.IsNaN(previous))
                        Assert.AreEqual(1, points[0].Y - previous, Tolerance, "the spacing");
                    previous = points[0].Y;
                }
                // The lines go over the bounds of the area.
                Matrix inverse = lineStep.Clone();
                inverse.Invert();
                PointF[] corners = { new PointF(0, 0), new PointF(10, 0), new PointF(10, 10), new PointF(0, 10) };
                inverse.TransformPoints(corners);
                PointF[] firstLine = ((PathShape)lines[0].Shape).Parts.SelectMany(part => part.Points).ToArray();
                Assert.LessOrEqual(firstLine[0].X, corners.Min(corner => corner.X));
                Assert.GreaterOrEqual(firstLine[1].X, corners.Max(corner => corner.X));
                Assert.LessOrEqual(firstLine[0].Y, corners.Min(corner => corner.Y) + 0.5);
                Assert.GreaterOrEqual(previous, corners.Max(corner => corner.Y) - 0.5);

                // The elements of the area pattern of two glyph parts: the shared shapes, two at each pose.
                List<ClippingObject> elements = separations[1].SymbolCoverages.Single().ExpandPrototype().ToList();
                Assert.Greater(elements.Count, 100, "glyph parts of the elements");
                Assert.AreEqual(0, elements.Count % 2);
                for (int i = 0; i < elements.Count; i += 2) {
                    Assert.AreSame(elements[0].Shape, elements[i].Shape);
                    Assert.AreSame(elements[1].Shape, elements[i + 1].Shape);
                    Assert.AreSame(elements[i].Placement, elements[i + 1].Placement);
                    IReadOnlyList<Matrix> steps = elements[i].Placement.Steps;
                    Assert.AreEqual(2, steps.Count);
                    AssertTransformation(elementStep, steps[0]);
                    Pose pose = elements[i].Placement.Poses.Single();
                    Assert.AreEqual(-40, pose.Angle, Tolerance);
                    Assert.AreEqual(Math.Round(pose.Location.X), pose.Location.X, Tolerance, "the lattice point {0}", pose.Location);
                    Assert.AreEqual(Math.Round(pose.Location.Y), pose.Location.Y, Tolerance, "the lattice point {0}", pose.Location);
                }

                // The elements of one circle at the origin: the circle at its lattice point, within M alone.
                List<ClippingObject> circles = separations[2].SymbolCoverages.Single().ExpandPrototype().ToList();
                Assert.Greater(circles.Count, 40, "circles");
                foreach (ClippingObject circle in circles) {
                    EllipseShape ellipse = (EllipseShape)circle.Shape;
                    Assert.AreEqual(0.15F, ellipse.RadiusX, Tolerance);
                    AssertTransformation(circleStep, circle.Placement.Steps.Single());
                    Assert.AreEqual(Math.Round(ellipse.Center.X / 1.5) * 1.5, ellipse.Center.X, Tolerance, "the lattice point {0}", ellipse.Center);
                    Assert.AreEqual(Math.Round(ellipse.Center.Y / 1.5) * 1.5, ellipse.Center.Y, Tolerance, "the lattice point {0}", ellipse.Center);
                }
            }
        }

        // The fill modes CompletelyInside, CenterInside and PartiallyInside select each glyph part of each element by its
        // own points and center, as the drawing does (GlyphPartFilter). The selected glyph parts are clipping objects of
        // the full coverage, between the area fill and the area border: the shared shapes of the glyph at a placement whose
        // steps are M and the pose of the element. The area pattern has no palette color coverage of its own. In the
        // elements at x = 9, the glyph parts are: a dot at 9, inside; a dot at 9.9 whose center is inside and whose right
        // edge is outside the area, which ends at x = 10; a ring at 10.15 whose center is outside and whose left edge is
        // inside; and an area square at 10.35 to 10.65, outside.
        [TestCase(AreaSymDef.PatternFillMode.CompletelyInside, 1)]
        [TestCase(AreaSymDef.PatternFillMode.CenterInside, 2)]
        [TestCase(AreaSymDef.PatternFillMode.PartiallyInside, 3)]
        public void FillModesSelectEachGlyphPartByItsOwnPointsAndCenter(AreaSymDef.PatternFillMode fillMode, int selectedAtTheEdge)
        {
            Map map = CreateMap();
            Glyph element;
            using (map.Write()) {
                SymColor brown = AddColor(map, "Brown", 1);
                element = CreateGlyph(glyph => {
                    glyph.AddFilledCircle(brown, new PointF(0, 0), 0.4F);
                    glyph.AddFilledCircle(brown, new PointF(0.9F, 0), 0.4F);
                    glyph.AddCircle(brown, new PointF(1.15F, 0), 0.05F, 0.4F);
                    glyph.AddArea(brown, new SymPathWithHoles(Square(1.35F, -0.15F, 0.3F), null));
                });
                LineSymDef border = new LineSymDef("Border", "1", brown, 0.1F, LineJoinMode.Miter, LineCapMode.Flat);
                map.AddSymdef(border);
                AreaSymDef boulders = new AreaSymDef("Boulders", "2", brown, border);
                boulders.AddPattern(new AreaSymDef.PatternInfo() { patternGlyph = element, patternWidth = 3, patternHeight = 3, patternFillMode = fillMode });
                map.AddSymdef(boulders);
                AddArea(map, boulders, Square(-0.5F, -0.5F, 10.5F), 0, new PointF());
            }

            using (map.Read()) {
                SymbolCoverage coverage = map.PaletteSeparations.Single().SymbolCoverages.Single();
                Assert.AreSame(CoveragePrototype.FullCoverage, coverage.Prototype, "the only palette color coverage of the area");
                IReadOnlyList<ClippingObject> clippingObjects = coverage.ClippingObjects;

                // The area fill first, and the area border along the main path last.
                Assert.IsInstanceOf<FilledShape>(clippingObjects[0]);
                Assert.AreEqual(0, clippingObjects[0].Placement.Steps.Count);
                Assert.IsInstanceOf<StrokedShape>(clippingObjects[clippingObjects.Count - 1]);
                Assert.AreEqual(0, clippingObjects[clippingObjects.Count - 1].Placement.Steps.Count);
                List<ClippingObject> selected = clippingObjects.Skip(1).Take(clippingObjects.Count - 2).ToList();

                // Four rows of elements at x = 0, 3 and 6, whose four glyph parts are all inside, and the glyph parts that
                // the fill mode selects at x = 9.
                Assert.AreEqual(4 * (3 * 4 + selectedAtTheEdge), selected.Count, "the selected glyph parts");
                List<ClippingObject> atTheEdge = selected.Where(part => Math.Abs(part.Placement.Poses.Single().Location.X - 9) < Tolerance).ToList();
                Assert.AreEqual(4 * selectedAtTheEdge, atTheEdge.Count, "the selected glyph parts at x = 9");
                foreach (ClippingObject part in selected) {
                    Assert.AreEqual(2, part.Placement.Steps.Count);
                    Assert.IsTrue(part.Placement.Steps[0].IsIdentity, "M of the area pattern");
                    Assert.AreEqual(0, part.Placement.Poses.Single().Angle, Tolerance);
                }

                // The selected glyph parts share the shapes of the glyph, in the order of the parts.
                List<ClippingObject> ofOneElement = selected.Take(4).ToList();
                foreach (ClippingObject part in selected)
                    Assert.IsTrue(ofOneElement.Any(shared => shared.Shape == part.Shape), "a shared shape");
                CollectionAssert.AreEqual(new float[] { 0, 0.9F }, ofOneElement.Take(2).Select(part => ((EllipseShape)part.Shape).Center.X));
                Assert.IsInstanceOf<StrokedShape>(ofOneElement[2]);
                Assert.IsInstanceOf<PathShape>(ofOneElement[3].Shape);
                CollectionAssert.AreEqual(ofOneElement.Take(selectedAtTheEdge).Select(part => part.Shape), atTheEdge.Take(selectedAtTheEdge).Select(part => part.Shape));
            }
        }

        // The hatchings and the area patterns of the area symbol definitions among the component definitions of a combined
        // area are its coverage prototypes, in the order of the components, also of a combined area that is a component.
        // They are the coverage prototypes of the component definitions, and M comes from the angle and the rotation
        // center of the combined area.
        [Test]
        public void CombinedAreaHasTheCoveragePrototypesOfItsComponentDefinitions()
        {
            Map map = CreateMap();
            AreaSymbol combined, alone;
            using (map.Write()) {
                SymColor blue = AddColor(map, "Blue", 1);
                SymColor brown = AddColor(map, "Brown", 2);
                SymColor black = AddColor(map, "Black", 3);
                LineSymDef line = new LineSymDef("Line", "1", black, 0.1F, LineJoinMode.Miter, LineCapMode.Flat);
                map.AddSymdef(line);
                AreaSymDef first = new AreaSymDef("First", "2", null, null);
                first.AddHatching(new AreaSymDef.HatchInfo() { hatchColor = blue, hatchWidth = 0.1F, hatchSpacing = 0.5F, hatchAngle = 10 });
                first.AddPattern(new AreaSymDef.PatternInfo() { patternGlyph = CreateGlyph(glyph => glyph.AddFilledCircle(brown, new PointF(0, 0), 0.3F)), patternWidth = 1, patternHeight = 1 });
                map.AddSymdef(first);
                AreaSymDef second = new AreaSymDef("Second", "3", null, null);
                second.AddHatching(new AreaSymDef.HatchInfo() { hatchColor = blue, hatchWidth = 0.1F, hatchSpacing = 0.7F, hatchAngle = 80 });
                map.AddSymdef(second);
                AreaComboSymDef inner = new AreaComboSymDef("Inner", "4", new SymDef[] { second });
                map.AddSymdef(inner);
                AreaComboSymDef outer = new AreaComboSymDef("Outer", "5", new SymDef[] { line, first, inner });
                map.AddSymdef(outer);
                alone = AddArea(map, first, Square(20, 0, 5), 0, new PointF());
                combined = AddArea(map, outer, Square(0, 0, 10), 30, new PointF(1, 2));
            }

            using (map.Read()) {
                IReadOnlyList<PaletteSeparation> separations = map.PaletteSeparations;
                List<SymbolCoverage> hatchings = separations[0].SymbolCoverages.Where(coverage => coverage.Symbol == combined).ToList();
                CollectionAssert.AreEqual(new string[] { "First", "Second" }, hatchings.Select(coverage => ((HatchingPrototype)coverage.Prototype).Definition.Name));
                SymbolCoverage ofFirst = separations[0].SymbolCoverages.Single(coverage => coverage.Symbol == alone);
                Assert.AreSame(ofFirst.Prototype, hatchings[0].Prototype);

                Matrix expected = new Matrix();
                expected.Translate(1, 2);
                expected.Rotate(30);
                expected.Rotate(80);
                AssertTransformation(expected, hatchings[1].Transformation);

                SymbolCoverage pattern = separations[1].SymbolCoverages.Single(coverage => coverage.Symbol == combined);
                Assert.AreSame(separations[1].SymbolCoverages.Single(coverage => coverage.Symbol == alone).Prototype, pattern.Prototype);
                Assert.IsInstanceOf<FilledShape>(pattern.ClippingObjects.Single());

                // Black: the line along the main path, with the full coverage.
                Assert.AreSame(CoveragePrototype.FullCoverage, separations[2].SymbolCoverages.Single().Prototype);
            }
        }

        // Assert that the palette cutters of a palette separation hold the palette color coverages of areas in groups.
        // separation: the palette separation.
        // groups: the indexes of the areas of each palette cutter, in order.
        // areas: the areas.
        static void AssertCutters(PaletteSeparation separation, int[][] groups, AreaSymbol[] areas)
        {
            Assert.AreEqual(groups.Length, separation.Cutters.Count, "the palette cutters of {0}", separation.Color.Name);
            for (int i = 0; i < groups.Length; ++i) {
                CollectionAssert.AreEqual(groups[i].Select(index => areas[index]), separation.Cutters[i].SymbolCoverages.Select(coverage => coverage.Symbol), "palette cutter {0}", i);
                foreach (SymbolCoverage coverage in separation.Cutters[i].SymbolCoverages)
                    Assert.AreSame(separation.Cutters[i].Prototype, coverage.Prototype);
            }
        }

        // Assert that two affine transformations map the points (0, 0), (1, 0) and (0, 1) to the same points.
        static void AssertTransformation(Matrix expected, Matrix actual)
        {
            PointF[] expectedPoints = { new PointF(0, 0), new PointF(1, 0), new PointF(0, 1) };
            PointF[] actualPoints = (PointF[])expectedPoints.Clone();
            expected.TransformPoints(expectedPoints);
            actual.TransformPoints(actualPoints);
            for (int i = 0; i < expectedPoints.Length; ++i) {
                Assert.AreEqual(expectedPoints[i].X, actualPoints[i].X, Tolerance, "the x of point {0}", i);
                Assert.AreEqual(expectedPoints[i].Y, actualPoints[i].Y, Tolerance, "the y of point {0}", i);
            }
        }

        // The rotation about the origin by an angle, in degrees.
        static Matrix Rotation(float angle)
        {
            Matrix matrix = new Matrix();
            matrix.Rotate(angle);
            return matrix;
        }

        static double Distance(PointF a, PointF b)
        {
            return Math.Sqrt((a.X - b.X) * (a.X - b.X) + (a.Y - b.Y) * (a.Y - b.Y));
        }

        // An empty map that measures text with Skia.
        static Map CreateMap()
        {
            return new Map(new Skia_TextMetrics(), null);
        }

        // Add a process color to a map, and return it.
        static SymColor AddColor(Map map, string name, short ocadId)
        {
            return map.AddColor(name, ocadId, 0.1F * ocadId, 0.5F, 0, 0, false);
        }

        // A glyph whose parts an action adds.
        static Glyph CreateGlyph(Action<Glyph> addParts)
        {
            Glyph glyph = new Glyph();
            addParts(glyph);
            glyph.ConstructionComplete();
            return glyph;
        }

        // Add an area of a definition to a map, and return it.
        static AreaSymbol AddArea(Map map, AreaLikeSymDef symdef, SymPath path, float angle, PointF rotationCenter)
        {
            return AddArea(map, symdef, new SymPathWithHoles(path, null), angle, rotationCenter);
        }

        // Add an area of a definition with a path with holes to a map, and return it.
        static AreaSymbol AddArea(Map map, AreaLikeSymDef symdef, SymPathWithHoles path, float angle, PointF rotationCenter)
        {
            AreaSymbol area = new AreaSymbol(symdef, path, angle, rotationCenter);
            map.AddSymbol(area);
            return area;
        }

        // The closed path of a square.
        static SymPath Square(float left, float bottom, float size)
        {
            return new SymPath(new PointF[] {
                new PointF(left, bottom), new PointF(left + size, bottom), new PointF(left + size, bottom + size),
                new PointF(left, bottom + size), new PointF(left, bottom) });
        }
    }
}
