using System;
using System.Collections.Generic;
using System.Drawing;

namespace PurplePen.MapModel
{
    using PurplePen.Graphics2D;

    // A coverage prototype: the coverage G in its origin-bound form, before the affine transformation M of a
    // configuration places it. G is binary: for each point of the plane, it tells if the point is in the coverage. On the
    // map side, G is a part of a symbol definition: the full coverage, one hatching (HatchingPrototype) or one area
    // pattern in one palette color (AreaPatternPrototype). A palette color coverage of a symbol refers to its coverage
    // prototype (SymbolCoverage), and M comes from the symbol and from G.
    public abstract class CoveragePrototype
    {
        // The full coverage, G = 1: the whole plane. It is the trivial case of a periodic coverage, and all the affine
        // maps are its symmetries, so no M changes it. The G of the points, the lines, the texts and the area fills is
        // the full coverage.
        public static readonly CoveragePrototype FullCoverage = new FullCoveragePrototype();

        // The coverage prototypes of a symbol that draws a palette color with the full coverage only.
        internal static readonly IReadOnlyList<CoveragePrototype> FullCoverageOnly = Array.AsReadOnly(new CoveragePrototype[] { FullCoverage });

        // The coverage prototypes of a symbol that does not draw a palette color.
        internal static readonly IReadOnlyList<CoveragePrototype> None = Array.Empty<CoveragePrototype>();

        // The precision of the normal form of M: of the elements of its linear part, and of its translation, in mm.
        internal const double LinearPrecision = 1E-6;
        internal const double TranslationPrecision = 0.0001;

        private protected CoveragePrototype()
        {
        }

        // Does the coverage cover a point of its own coordinates: is G(x, y) = 1?
        // point: the point.
        public abstract bool Covers(PointF point);

        // The affine transformation M of the configuration of the palette color coverage of a symbol with this coverage
        // prototype: from the coordinates of G to those of the map.
        // symbol: the symbol.
        internal abstract Matrix GetTransformation(Symbol symbol);

        // The normal form of a transformation M of this coverage prototype, which decides the sameness of the
        // configurations: two configurations of the coverage prototype in one palette color are the same when the normal
        // forms of their transformations are the same, as M₁⁻¹M₂ is then a symmetry of G. The normal form reduces the
        // translation of M into a fundamental domain of the translation symmetries of G, in the coordinates of G, and it
        // rounds the elements of the linear part of M to LinearPrecision and the translation to TranslationPrecision.
        // transformation: the transformation M.
        internal abstract Matrix GetNormalForm(Matrix transformation);

        // The expansion of the coverage prototype over the bounds of the area of a symbol (SymbolCoverage.ExpandPrototype):
        // the lines of a hatching or the elements of an area pattern, as single clipping objects at the places and with
        // the steps of the drawing.
        // symbol: the area symbol.
        internal abstract IReadOnlyList<ClippingObject> Expand(Symbol symbol);

        // The normal form of a transformation M: the linear part A rounded to LinearPrecision, and the translation t of M
        // in the coordinates of G, M = A · T(t), reduced by a function, at the translation A · t.
        // transformation: the transformation M.
        // reduce: the function that reduces the translation t into a fundamental domain and rounds it.
        private protected static Matrix MakeNormalForm(Matrix transformation, TranslationReduction reduce)
        {
            float[] elements = transformation.Elements;
            double m11 = elements[0], m12 = elements[1], m21 = elements[2], m22 = elements[3];
            double dx = elements[4], dy = elements[5];

            // M maps a point (x, y) to (m11 x + m21 y + dx, m12 x + m22 y + dy), so t = A⁻¹ (dx, dy).
            double determinant = m11 * m22 - m12 * m21;
            double tx = (m22 * dx - m21 * dy) / determinant;
            double ty = (m11 * dy - m12 * dx) / determinant;
            reduce(ref tx, ref ty);

            double r11 = RoundTo(m11, LinearPrecision), r12 = RoundTo(m12, LinearPrecision);
            double r21 = RoundTo(m21, LinearPrecision), r22 = RoundTo(m22, LinearPrecision);
            return new Matrix(ToElement(r11), ToElement(r12), ToElement(r21), ToElement(r22),
                              ToElement(r11 * tx + r21 * ty), ToElement(r12 * tx + r22 * ty));
        }

        // A function that reduces a translation in the coordinates of G into a fundamental domain of the translation
        // symmetries of G, and rounds it.
        private protected delegate void TranslationReduction(ref double tx, ref double ty);

        // A translation along a period reduced into [0, period) and rounded: as a multiple of the period, rounded to a
        // step of about TranslationPrecision, which a whole number of steps fills. A multiple that rounds to the period is
        // 0, as the period is a symmetry. A period that is not positive reduces nothing, and the translation is rounded to
        // TranslationPrecision.
        // translation: the translation along the period.
        // period: the length of the period.
        private protected static double ReduceAlong(double translation, double period)
        {
            if (!(period > 0))
                return RoundTo(translation, TranslationPrecision);

            return ReduceCoefficient(translation / period, period) * period;
        }

        // A coefficient of a period reduced into [0, 1) and rounded to the step of ReduceAlong.
        // coefficient: the coefficient.
        // period: the length of the period.
        private protected static double ReduceCoefficient(double coefficient, double period)
        {
            long steps = Math.Max(1, (long)Math.Round(period / TranslationPrecision));
            double fraction = coefficient - Math.Floor(coefficient);
            return ((long)Math.Round(fraction * steps) % steps) / (double)steps;
        }

        // A value rounded to a precision.
        private protected static double RoundTo(double value, double precision)
        {
            return Math.Round(value / precision) * precision;
        }

        // An element of a matrix, in which -0 is 0.
        private static float ToElement(double value)
        {
            float element = (float)value;
            return (element == 0) ? 0F : element;
        }

        // The coverage of every point.
        private sealed class FullCoveragePrototype : CoveragePrototype
        {
            public override bool Covers(PointF point)
            {
                return true;
            }

            // No M changes the full coverage, so the configurations of a palette separation with the full coverage are
            // one, whose M is the identity.
            internal override Matrix GetTransformation(Symbol symbol)
            {
                return new Matrix();
            }

            internal override Matrix GetNormalForm(Matrix transformation)
            {
                return new Matrix();
            }

            internal override IReadOnlyList<ClippingObject> Expand(Symbol symbol)
            {
                throw new InvalidOperationException("The full coverage is not a pattern fill, and it has no expansion.");
            }

            public override string ToString()
            {
                return "G = 1";
            }
        }
    }

    // The coverage prototype of a hatching (AreaSymDef.HatchInfo) of an area symbol definition: stripes of the width
    // hatchWidth about the lines y = k · hatchSpacing, for each integer k. Its symmetries include all the translations
    // along the lines, and the translations across them by multiples of the spacing. M places it with the angle and the
    // rotation center of the area symbol and with the angle and the offset of the hatching. The coverage prototype refers
    // to its area symbol definition and to the index of the hatching, and copies nothing. The area symbol definition
    // makes it once, and each area symbol of the definition refers to the same object.
    public sealed class HatchingPrototype : CoveragePrototype
    {
        private readonly AreaSymDef definition;
        private readonly int index;

        // Make the coverage prototype of a hatching.
        // definition: the area symbol definition.
        // index: the index of the hatching in the definition.
        internal HatchingPrototype(AreaSymDef definition, int index)
        {
            this.definition = definition;
            this.index = index;
        }

        // The area symbol definition of the hatching.
        public AreaSymDef Definition
        {
            get { return definition; }
        }

        // The index of the hatching in the area symbol definition.
        public int Index
        {
            get { return index; }
        }

        // The hatching, as the area symbol definition keeps it.
        public AreaSymDef.HatchInfo Hatching
        {
            get { return definition.GetHatching(index); }
        }

        // A point is in the coverage when it is at most half the width of the hatch lines from the nearest line
        // y = k · hatchSpacing.
        public override bool Covers(PointF point)
        {
            AreaSymDef.HatchInfo hatching = Hatching;
            double spacing = hatching.hatchSpacing;
            double distance = (spacing > 0) ? Math.Abs(point.Y - spacing * Math.Round(point.Y / spacing)) : Math.Abs(point.Y);
            return distance <= hatching.hatchWidth / 2.0;
        }

        // M = T(c) · R(a) · R(hatchAngle) · T(0, hatchOffset), where c is the rotation center of the area symbol and a its
        // angle. The rotate mode Never leaves out c and a.
        internal override Matrix GetTransformation(Symbol symbol)
        {
            AreaSymbol area = (AreaSymbol)symbol;
            return definition.GetHatchingTransformation(index, area.Angle, area.RotationCenter);
        }

        // The translation along the lines is 0, and the translation across them is in [0, hatchSpacing).
        internal override Matrix GetNormalForm(Matrix transformation)
        {
            double spacing = Hatching.hatchSpacing;
            return MakeNormalForm(transformation, (ref double tx, ref double ty) => {
                tx = 0;
                ty = ReduceAlong(ty, spacing);
            });
        }

        // The hatch lines over the bounds of the area, as the drawing draws them.
        internal override IReadOnlyList<ClippingObject> Expand(Symbol symbol)
        {
            AreaSymbol area = (AreaSymbol)symbol;
            return definition.ExpandHatching(index, area.Path, area.Angle, area.RotationCenter);
        }

        public override string ToString()
        {
            return string.Format("hatching {0} of {1}", index, definition.Name);
        }
    }

    // The coverage prototype of an area pattern (AreaSymDef.PatternInfo) of an area symbol definition in one palette
    // color: the glyph parts of its element in the palette color, turned by -patternAngle, at the points of its lattice,
    // x = i · patternWidth and y = j · patternHeight for integers i and j. When the area pattern offsets its rows, the
    // points of each odd row are patternWidth / 2 aside. An irregular area pattern moves each element as the drawing
    // moves it (AreaSymDef.HandleIrregularity), so of its translations only the identity is certain to be a symmetry. An
    // element whose glyph parts are in several palette colors gives the area pattern a coverage prototype in each of
    // them. M places it with the angle and the rotation center of the area symbol and with the angle and the offset of
    // the area pattern. The coverage prototype refers to its area symbol definition, to the index of the area pattern
    // and to the palette color, and copies nothing. The area symbol definition makes it once for each palette color of a
    // clipped area pattern, and each area symbol of the definition refers to the same object.
    public sealed class AreaPatternPrototype : CoveragePrototype
    {
        private readonly AreaSymDef definition;
        private readonly int index;
        private readonly SymColor color;

        // Make the coverage prototype of an area pattern in a palette color.
        // definition: the area symbol definition.
        // index: the index of the area pattern in the definition.
        // color: the palette color.
        internal AreaPatternPrototype(AreaSymDef definition, int index, SymColor color)
        {
            this.definition = definition;
            this.index = index;
            this.color = color;
        }

        // The area symbol definition of the area pattern.
        public AreaSymDef Definition
        {
            get { return definition; }
        }

        // The index of the area pattern in the area symbol definition.
        public int Index
        {
            get { return index; }
        }

        // The palette color of the glyph parts.
        public SymColor Color
        {
            get { return color; }
        }

        // The area pattern, as the area symbol definition keeps it.
        public AreaSymDef.PatternInfo Pattern
        {
            get { return definition.GetPattern(index); }
        }

        // A point is in the coverage when a glyph part of an element in the palette color covers it.
        public override bool Covers(PointF point)
        {
            return definition.AreaPatternCovers(index, color, point);
        }

        // M = T(c) · R(a) · R(patternAngle) · T(patternOffsetX, patternOffsetY), where c is the rotation center of the
        // area symbol and a its angle. The rotate mode Never leaves out c and a.
        internal override Matrix GetTransformation(Symbol symbol)
        {
            AreaSymbol area = (AreaSymbol)symbol;
            return definition.GetPatternTransformation(index, area.Angle, area.RotationCenter);
        }

        // The translation is in [0, 1) as the coefficients of the basis of the lattice: (patternWidth, 0), and
        // (patternWidth / 2, patternHeight) when the rows are offset or (0, patternHeight) when they are not. The
        // translation of an irregular area pattern stays, as only the identity is certain to be its translation
        // symmetry.
        internal override Matrix GetNormalForm(Matrix transformation)
        {
            AreaSymDef.PatternInfo pattern = Pattern;
            if (pattern.irregular || !(pattern.patternWidth > 0) || !(pattern.patternHeight > 0)) {
                return MakeNormalForm(transformation, (ref double tx, ref double ty) => {
                    tx = RoundTo(tx, TranslationPrecision);
                    ty = RoundTo(ty, TranslationPrecision);
                });
            }

            double width = pattern.patternWidth, height = pattern.patternHeight;
            double rowOffset = pattern.offsetRows ? width / 2 : 0;
            return MakeNormalForm(transformation, (ref double tx, ref double ty) => {
                double second = ReduceCoefficient(ty / height, Math.Sqrt(rowOffset * rowOffset + height * height));
                double first = ReduceCoefficient((tx - (ty / height) * rowOffset) / width, width);
                tx = first * width + second * rowOffset;
                ty = second * height;
            });
        }

        // The elements over the bounds of the area, as the drawing draws them.
        internal override IReadOnlyList<ClippingObject> Expand(Symbol symbol)
        {
            AreaSymbol area = (AreaSymbol)symbol;
            return definition.ExpandAreaPattern(index, color, area.Path, area.Angle, area.RotationCenter);
        }

        public override string ToString()
        {
            return string.Format("area pattern {0} of {1} in {2}", index, definition.Name, color.Name);
        }
    }
}
