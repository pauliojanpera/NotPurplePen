using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace PurplePen.MapModel
{
    using PurplePen.Graphics2D;

    // The intermediate layer, which pass 1 makes from the map alone (Map.PaletteSeparations). The map has one palette
    // separation for each palette color, in palette order. A palette separation has the palette color coverages of the
    // symbols in its palette color, and its palette cutters group them by their configuration. The clipping objects of
    // a palette color coverage of a symbol limit where its coverage prototype applies.
    // The intermediate layer is not source data. Its units are derived values of the map: the map makes each of them
    // when it is necessary the first time, and a change of the map makes them invalid. A read of the contents of an
    // invalid unit throws a MapUsageException, and the next read of Map.PaletteSeparations makes the units again. The
    // units are read within Map.Read or Map.Write, as the map is. Several readers can read them at the same time, and
    // all of them get the same objects.

    // A palette color coverage of a symbol: the coverage of one symbol in one palette color with one coverage prototype
    // G, the triple (Symbol, SymColor, G). Its coverage is G under the transformation M of its configuration, inside the
    // union of its clipping objects. It refers to its symbol, its palette color and its coverage prototype, and copies
    // nothing. It keeps its own derived values.
    public sealed class SymbolCoverage
    {
        private readonly Map map;
        private readonly long madeAt;           // The change count of the map when the map made the coverage.
        private readonly Symbol symbol;
        private readonly SymColor color;
        private readonly CoveragePrototype prototype;
        private IReadOnlyList<ClippingObject> clippingObjects;     // The clipping objects, after the first read made them.

        // Make a palette color coverage of a symbol.
        // map: the map that makes the coverage.
        // madeAt: the change count of the map.
        // symbol: the symbol.
        // color: the palette color.
        // prototype: the coverage prototype.
        internal SymbolCoverage(Map map, long madeAt, Symbol symbol, SymColor color, CoveragePrototype prototype)
        {
            this.map = map;
            this.madeAt = madeAt;
            this.symbol = symbol;
            this.color = color;
            this.prototype = prototype;
        }

        // The symbol.
        public Symbol Symbol
        {
            get { return symbol; }
        }

        // The palette color.
        public SymColor Color
        {
            get { return color; }
        }

        // The coverage prototype G.
        public CoveragePrototype Prototype
        {
            get { return prototype; }
        }

        // The clipping objects, in the order in which the drawing code of the symbol draws their shapes. They are a
        // derived value of the coverage, which the first read makes. The symbol definition makes them with its geometry
        // method, or with the collector from the drawing code of the symbol (SymDef.MakeClippingObjects). A coverage
        // can have no clipping objects.
        public IReadOnlyList<ClippingObject> ClippingObjects
        {
            get
            {
                map.CheckDerivedValue(madeAt);

                IReadOnlyList<ClippingObject> made = Volatile.Read(ref clippingObjects);
                if (made == null) {
                    made = symbol.Definition.MakeClippingObjects(symbol, color, prototype);
                    made = Interlocked.CompareExchange(ref clippingObjects, made, null) ?? made;
                }

                return made;
            }
        }

        public override string ToString()
        {
            return string.Format("{0} in {1}, {2}", symbol.Definition.Name, color.Name, prototype);
        }
    }

    // A palette separation: the palette color coverages of the symbols in one palette color, in the order in which the
    // drawing of the map draws the symbols (Map.DrawColor). It keeps its palette cutters as a derived value.
    public sealed class PaletteSeparation
    {
        private readonly Map map;
        private readonly long madeAt;           // The change count of the map when the map made the separation.
        private readonly SymColor color;
        private readonly ReadOnlyCollection<SymbolCoverage> symbolCoverages;
        private IReadOnlyList<PaletteCutter> cutters;      // The palette cutters, after the first read made them.

        // Make a palette separation.
        // map: the map that makes the separation.
        // madeAt: the change count of the map.
        // color: the palette color.
        // symbolCoverages: the palette color coverages of the symbols in the palette color, in the order of the drawing.
        internal PaletteSeparation(Map map, long madeAt, SymColor color, IEnumerable<SymbolCoverage> symbolCoverages)
        {
            this.map = map;
            this.madeAt = madeAt;
            this.color = color;
            this.symbolCoverages = symbolCoverages.ToList().AsReadOnly();
        }

        // The palette color.
        public SymColor Color
        {
            get { return color; }
        }

        // The palette color coverages of the symbols, in the order in which the drawing of the map draws the symbols.
        public IReadOnlyList<SymbolCoverage> SymbolCoverages
        {
            get
            {
                map.CheckDerivedValue(madeAt);
                return symbolCoverages;
            }
        }

        // The palette cutters, in the order of their first palette color coverages. Each palette color coverage of a
        // symbol is in one of them. They are a derived value of the separation, which the first read makes.
        public IReadOnlyList<PaletteCutter> Cutters
        {
            get
            {
                map.CheckDerivedValue(madeAt);

                IReadOnlyList<PaletteCutter> made = Volatile.Read(ref cutters);
                if (made == null) {
                    made = MakeCutters();
                    made = Interlocked.CompareExchange(ref cutters, made, null) ?? made;
                }

                return made;
            }
        }

        // Make the palette cutters: one for each configuration of the palette color coverages. A configuration is the
        // palette color, the coverage prototype G and the transformation M. All the affine maps are symmetries of the
        // full coverage, so the palette color coverages with the full coverage have one configuration, whose M is the
        // identity. Pass 1 makes no other coverage prototype (SymDef.GetCoveragePrototypes).
        private IReadOnlyList<PaletteCutter> MakeCutters()
        {
            if (symbolCoverages.Count == 0)
                return Array.Empty<PaletteCutter>();

            Debug.Assert(symbolCoverages.All(coverage => coverage.Prototype == CoveragePrototype.FullCoverage), "Pass 1 makes the full coverage only.");
            return Array.AsReadOnly(new PaletteCutter[] { new PaletteCutter(map, madeAt, CoveragePrototype.FullCoverage, new Matrix(), symbolCoverages) });
        }

        public override string ToString()
        {
            return string.Format("{0}, {1} palette color coverages", color.Name, symbolCoverages.Count);
        }
    }

    // A palette cutter: the palette color coverages of the symbols in one palette separation that have the same
    // configuration. It cuts the uniform color of the palette color to the coverage that its coverage prototype G under
    // its transformation M gives inside the union of the clipping objects of its palette color coverages.
    public sealed class PaletteCutter
    {
        private readonly Map map;
        private readonly long madeAt;           // The change count of the map when the map made the cutter.
        private readonly CoveragePrototype prototype;
        private readonly Matrix transformation;
        private readonly IReadOnlyList<SymbolCoverage> symbolCoverages;

        // Make a palette cutter.
        // map: the map that makes the cutter.
        // madeAt: the change count of the map.
        // prototype: the coverage prototype G of the configuration.
        // transformation: the transformation M of the configuration, in its normal form. It is not copied.
        // symbolCoverages: the palette color coverages of the symbols with the configuration.
        internal PaletteCutter(Map map, long madeAt, CoveragePrototype prototype, Matrix transformation, IReadOnlyList<SymbolCoverage> symbolCoverages)
        {
            this.map = map;
            this.madeAt = madeAt;
            this.prototype = prototype;
            this.transformation = transformation;
            this.symbolCoverages = symbolCoverages;
        }

        // The coverage prototype G.
        public CoveragePrototype Prototype
        {
            get { return prototype; }
        }

        // A copy of the transformation M, from the coordinates of the coverage prototype to those of the map.
        public Matrix Transformation
        {
            get { return transformation.Clone(); }
        }

        // The palette color coverages of the symbols with the configuration, in the order of the palette separation.
        public IReadOnlyList<SymbolCoverage> SymbolCoverages
        {
            get
            {
                map.CheckDerivedValue(madeAt);
                return symbolCoverages;
            }
        }

        public override string ToString()
        {
            return string.Format("{0}, {1} palette color coverages", prototype, symbolCoverages.Count);
        }
    }
}
