using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
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
        private readonly Matrix transformation;  // The transformation M of the configuration.
        private IReadOnlyList<ClippingObject> clippingObjects;     // The clipping objects, after the first read made them.
        private float[] dashResolutions;        // The resolutions above which its dashed line components are solid, after the first read made them.
        private IReadOnlyList<ClippingObject>[] solidClippingObjects;   // The clipping objects at the resolutions of drawings, by the number of solid dashed line components.

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
            this.transformation = prototype.GetTransformation(symbol);
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

        // A copy of the transformation M of the configuration, from the coordinates of the coverage prototype to those of
        // the map. M of the full coverage is the identity. M of a hatching or an area pattern comes from the angle and the
        // rotation center of the area symbol and from the angle and the offset of the hatching or the area pattern
        // (CoveragePrototype.GetTransformation).
        public Matrix Transformation
        {
            get { return transformation.Clone(); }
        }

        // The clipping objects, in the order in which the drawing code of the symbol draws their shapes. They are a
        // derived value of the coverage, which the first read makes. The symbol definition makes them with its geometry
        // method (SymDef.MakeClippingObjects). A coverage can have no clipping objects. The clipping object of a
        // hatching or a clipped area pattern is the area that it fills.
        public IReadOnlyList<ClippingObject> ClippingObjects
        {
            get
            {
                map.CheckDerivedValue(madeAt);

                IReadOnlyList<ClippingObject> made = Volatile.Read(ref clippingObjects);
                if (made == null) {
                    made = symbol.Definition.MakeClippingObjects(symbol, color, prototype, 0);
                    made = Interlocked.CompareExchange(ref clippingObjects, made, null) ?? made;
                }

                return made;
            }
        }

        // The clipping objects at the resolution of a drawing, a channel conversion of the clipping objects: a dashed line
        // component whose gaps are all shorter than the resolution is solid, as the old drawing draws it
        // (LineSymDef.DrawDashedWithOffset). This applies to the line symbols, the area borders and the component
        // definitions of combinations. At the resolution 0, they are the clipping objects of the coverage (ClippingObjects).
        // The conversion depends on the resolution only through which dashed line components are solid, and the coverage
        // keeps its result for each set of solid dashed line components, as it keeps its clipping objects.
        // resolution: the resolution of the drawing: the size of a pixel, in paper coordinates.
        public IReadOnlyList<ClippingObject> GetClippingObjects(float resolution)
        {
            map.CheckDerivedValue(madeAt);

            // The number of dashed line components that are solid at the resolution.
            float[] resolutions = GetDashResolutions();
            int solid = 0;
            while (solid < resolutions.Length && resolutions[solid] < resolution)
                ++solid;
            if (solid == 0)
                return ClippingObjects;

            IReadOnlyList<ClippingObject>[] kept = Volatile.Read(ref solidClippingObjects);
            if (kept == null) {
                kept = new IReadOnlyList<ClippingObject>[resolutions.Length + 1];
                kept = Interlocked.CompareExchange(ref solidClippingObjects, kept, null) ?? kept;
            }

            IReadOnlyList<ClippingObject> made = Volatile.Read(ref kept[solid]);
            if (made == null) {
                made = symbol.Definition.MakeClippingObjects(symbol, color, prototype, resolution);
                made = Interlocked.CompareExchange(ref kept[solid], made, null) ?? made;
            }

            return made;
        }

        // The distinct resolutions above which the dashed line components of the coverage are solid, from the smallest
        // (SymDef.AddDashResolutions). Only the full coverage has line components. The first read makes them.
        private float[] GetDashResolutions()
        {
            float[] made = Volatile.Read(ref dashResolutions);
            if (made == null) {
                List<float> resolutions = new List<float>();
                if (prototype == CoveragePrototype.FullCoverage)
                    symbol.Definition.AddDashResolutions(resolutions, color);
                made = resolutions.Where(resolution => !float.IsNaN(resolution)).Distinct().OrderBy(resolution => resolution).ToArray();
                made = Interlocked.CompareExchange(ref dashResolutions, made, null) ?? made;
            }

            return made;
        }

        // The expansion of the coverage prototype of a hatching or a clipped area pattern over the bounds of the area:
        // the hatch lines or the elements of the area pattern in the palette color, as single clipping objects at the
        // places and with the steps of the drawing, in the order of the drawing. The coverage of the palette color coverage
        // is the expansion inside its clipping objects. The expansion of an irregular area pattern is the placements of
        // its elements. It is a channel conversion, which each call makes again: the map does not keep it.
        public IReadOnlyList<ClippingObject> ExpandPrototype()
        {
            map.CheckDerivedValue(madeAt);
            return prototype.Expand(symbol);
        }

        // The transformation M of the configuration, which is not copied.
        internal Matrix TransformationOfConfiguration
        {
            get { return transformation; }
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

        // Make the palette cutters: one for each configuration of the palette color coverages, in the order of their first
        // palette color coverages. A configuration is the palette color, the coverage prototype G and the transformation
        // M. Two configurations are the same when they have the same coverage prototype and the same normal form of M
        // (CoveragePrototype.GetNormalForm), and the palette cutter of a configuration has that normal form. All the
        // affine maps are symmetries of the full coverage, so the palette color coverages with the full coverage have one
        // configuration, whose M is the identity.
        private IReadOnlyList<PaletteCutter> MakeCutters()
        {
            if (symbolCoverages.Count == 0)
                return Array.Empty<PaletteCutter>();

            Dictionary<Configuration, List<SymbolCoverage>> coveragesOfConfigurations = new Dictionary<Configuration, List<SymbolCoverage>>();
            List<(Configuration configuration, Matrix normalForm)> configurations = new List<(Configuration, Matrix)>();
            foreach (SymbolCoverage coverage in symbolCoverages) {
                Matrix normalForm = coverage.Prototype.GetNormalForm(coverage.TransformationOfConfiguration);
                Configuration configuration = new Configuration(coverage.Prototype, normalForm.Elements);
                List<SymbolCoverage> coverages;
                if (!coveragesOfConfigurations.TryGetValue(configuration, out coverages)) {
                    coverages = new List<SymbolCoverage>();
                    coveragesOfConfigurations.Add(configuration, coverages);
                    configurations.Add((configuration, normalForm));
                }
                coverages.Add(coverage);
            }

            PaletteCutter[] cutters = new PaletteCutter[configurations.Count];
            for (int i = 0; i < cutters.Length; ++i) {
                (Configuration configuration, Matrix normalForm) = configurations[i];
                cutters[i] = new PaletteCutter(map, madeAt, configuration.Prototype, normalForm, coveragesOfConfigurations[configuration].AsReadOnly());
            }

            return Array.AsReadOnly(cutters);
        }

        // A configuration in a palette separation: the coverage prototype, and the elements of the normal form of M.
        private readonly struct Configuration : IEquatable<Configuration>
        {
            public readonly CoveragePrototype Prototype;
            private readonly float m11, m12, m21, m22, dx, dy;

            // Make a configuration.
            // prototype: the coverage prototype.
            // normalForm: the elements of the normal form of M, as Matrix.Elements gives them.
            public Configuration(CoveragePrototype prototype, float[] normalForm)
            {
                Prototype = prototype;
                m11 = normalForm[0];
                m12 = normalForm[1];
                m21 = normalForm[2];
                m22 = normalForm[3];
                dx = normalForm[4];
                dy = normalForm[5];
            }

            public bool Equals(Configuration other)
            {
                return Prototype == other.Prototype && m11 == other.m11 && m12 == other.m12 && m21 == other.m21 && m22 == other.m22 &&
                       dx == other.dx && dy == other.dy;
            }

            public override bool Equals(object obj)
            {
                return obj is Configuration other && Equals(other);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(RuntimeHelpers.GetHashCode(Prototype), m11, m12, m21, m22, dx, dy);
            }
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
        // transformation: the transformation M of the configuration, in its normal form (CoveragePrototype.GetNormalForm).
        //   It is not copied.
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

        // A copy of the transformation M, from the coordinates of the coverage prototype to those of the map, in its
        // normal form: its translation is in a fundamental domain of the translation symmetries of the coverage
        // prototype, and it is rounded to a fixed precision (CoveragePrototype.GetNormalForm). The transformations of the
        // palette color coverages of the palette cutter have this normal form.
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
