using System;
using System.Collections.Generic;
using System.Drawing;

namespace PurplePen.MapModel
{
    // A coverage prototype: the coverage G in its origin-bound form, before the affine transformation M of a
    // configuration places it. G is binary: for each point of the plane, it tells if the point is in the coverage. On the
    // map side, G is a part of a symbol definition: the full coverage, one hatching or one area pattern. A palette color
    // coverage of a symbol refers to its coverage prototype (SymbolCoverage).
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

        private protected CoveragePrototype()
        {
        }

        // Does the coverage cover a point of its own coordinates: is G(x, y) = 1?
        // point: the point.
        public abstract bool Covers(PointF point);

        // The coverage of every point.
        private sealed class FullCoveragePrototype : CoveragePrototype
        {
            public override bool Covers(PointF point)
            {
                return true;
            }

            public override string ToString()
            {
                return "G = 1";
            }
        }
    }
}
