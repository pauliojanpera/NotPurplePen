using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
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
            return ExpandPrototype(((AreaSymbol)symbol).Path.BoundingBox);
        }

        // The expansion of the coverage prototype of a hatching or a clipped area pattern over a rectangle, with the
        // transformation M of this palette color coverage: the hatch lines or the elements of the area pattern that reach
        // the rectangle, as ExpandPrototype gives them over the bounds of the area. The pattern of an area cluster is the
        // expansion of its first palette color coverage over the bounds of the cluster (AreaCluster.Bounds). It is a
        // channel conversion, which each call makes again: the map does not keep it.
        // bounds: the rectangle, in the coordinates of the map.
        public IReadOnlyList<ClippingObject> ExpandPrototype(RectangleF bounds)
        {
            map.CheckDerivedValue(madeAt);
            return prototype.Expand(symbol, bounds);
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
        private Dictionary<SymbolCoverage, AreaCluster> areaClusterOfCoverage;     // After the first read made it.

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

        // The area cluster of a palette color coverage of a hatching or a clipped area pattern in the separation, or null
        // for a palette color coverage of the full coverage. The area clusters are the derived values of the palette
        // cutters (PaletteCutter.AreaClusters), and the first read makes those of all the palette cutters.
        // coverage: the palette color coverage.
        internal AreaCluster GetAreaCluster(SymbolCoverage coverage)
        {
            map.CheckDerivedValue(madeAt);

            Dictionary<SymbolCoverage, AreaCluster> made = Volatile.Read(ref areaClusterOfCoverage);
            if (made == null) {
                made = new Dictionary<SymbolCoverage, AreaCluster>();
                foreach (PaletteCutter cutter in Cutters) {
                    foreach (AreaCluster cluster in cutter.AreaClusters) {
                        foreach (SymbolCoverage member in cluster.SymbolCoverages)
                            made.Add(member, cluster);
                    }
                }
                made = Interlocked.CompareExchange(ref areaClusterOfCoverage, made, null) ?? made;
            }

            AreaCluster areaCluster;
            return made.TryGetValue(coverage, out areaCluster) ? areaCluster : null;
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
        private IReadOnlyList<AreaCluster> areaClusters;   // The area clusters, after the first read made them.

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

        // The area clusters of a palette cutter of a hatching or a clipped area pattern, in the order of their first
        // palette color coverages: the palette color coverages whose areas are connected by bounding boxes that overlap or
        // touch. An area whose contours meet (SymPathWithHoles.ContoursMeet), or that meets no other area of the palette
        // cutter, is a cluster of its own. A palette cutter of the full coverage has none. They are a derived value of the
        // palette cutter, which the first read makes.
        public IReadOnlyList<AreaCluster> AreaClusters
        {
            get
            {
                map.CheckDerivedValue(madeAt);

                IReadOnlyList<AreaCluster> made = Volatile.Read(ref areaClusters);
                if (made == null) {
                    made = MakeAreaClusters();
                    made = Interlocked.CompareExchange(ref areaClusters, made, null) ?? made;
                }

                return made;
            }
        }

        public override string ToString()
        {
            return string.Format("{0}, {1} palette color coverages", prototype, symbolCoverages.Count);
        }

        // Make the area clusters, as fix/overlapping-patterns groups the areas of a symbol (OverlappingAreas.GroupAreas):
        // the bounding boxes of the paths of the areas are connected where they overlap or touch. An area whose contours
        // meet is left out, as the path of a union fills an area as the even-odd rule fills it alone only when no two of
        // its contours meet (SymPathWithHoles.AddToUnionPathPartList), and the other areas are connected again without it.
        // Only the areas that meet another are looked at for that.
        private IReadOnlyList<AreaCluster> MakeAreaClusters()
        {
            if (prototype == CoveragePrototype.FullCoverage)
                return Array.Empty<AreaCluster>();

            int count = symbolCoverages.Count;
            SymPathWithHoles[] paths = new SymPathWithHoles[count];
            RectangleF[] boxes = new RectangleF[count];
            for (int i = 0; i < count; ++i) {
                paths[i] = ((AreaSymbol)symbolCoverages[i].Symbol).Path;
                boxes[i] = paths[i].BoundingBox;
            }

            int[] roots = Connect(boxes);
            int[] members = CountMembers(roots);
            List<int> connected = new List<int>(count);
            for (int i = 0; i < count; ++i) {
                if (members[roots[i]] < 2 || !paths[i].ContoursMeet())
                    connected.Add(i);
            }

            // The first area of the cluster of each area: the root of its set, which is the first of the set, or the area
            // itself when its contours meet.
            int[] firstOfCluster = new int[count];
            for (int i = 0; i < count; ++i)
                firstOfCluster[i] = i;
            if (connected.Count == count) {
                firstOfCluster = roots;
            }
            else {
                int[] connectedRoots = Connect(connected.Select(i => boxes[i]).ToArray());
                for (int k = 0; k < connected.Count; ++k)
                    firstOfCluster[connected[k]] = connected[connectedRoots[k]];
            }

            Dictionary<int, List<SymbolCoverage>> coveragesOfCluster = new Dictionary<int, List<SymbolCoverage>>();
            List<int> firsts = new List<int>();
            for (int i = 0; i < count; ++i) {
                List<SymbolCoverage> coverages;
                if (!coveragesOfCluster.TryGetValue(firstOfCluster[i], out coverages)) {
                    coverages = new List<SymbolCoverage>();
                    coveragesOfCluster.Add(firstOfCluster[i], coverages);
                    firsts.Add(firstOfCluster[i]);
                }
                coverages.Add(symbolCoverages[i]);
            }

            AreaCluster[] clusters = new AreaCluster[firsts.Count];
            for (int c = 0; c < clusters.Length; ++c) {
                List<SymbolCoverage> coverages = coveragesOfCluster[firsts[c]];
                RectangleF bounds = boxes[firsts[c]];
                foreach (SymbolCoverage coverage in coverages)
                    bounds = RectangleF.Union(bounds, ((AreaSymbol)coverage.Symbol).Path.BoundingBox);
                clusters[c] = new AreaCluster(map, madeAt, coverages.AsReadOnly(), bounds);
            }

            return Array.AsReadOnly(clusters);
        }

        // Connect the boxes that overlap or touch, by a sweep across them from left to right, joining each box to
        // those still open that it meets, and return the root of each box's set: boxes connected through others share
        // a root, the index of one of them.
        // boxes: the boxes.
        private static int[] Connect(RectangleF[] boxes)
        {
            int count = boxes.Length;
            int[] parent = new int[count];
            int[] order = new int[count];
            for (int i = 0; i < count; ++i) {
                parent[i] = i;
                order[i] = i;
            }
            Array.Sort(order, (a, b) => boxes[a].Left.CompareTo(boxes[b].Left));

            List<int> open = new List<int>();
            foreach (int index in order) {
                RectangleF box = boxes[index];
                open.RemoveAll(other => boxes[other].Right < box.Left);
                foreach (int other in open) {
                    if (boxes[other].Top <= box.Bottom && box.Top <= boxes[other].Bottom)
                        Join(parent, index, other);
                }
                open.Add(index);
            }

            int[] roots = new int[count];
            for (int i = 0; i < count; ++i)
                roots[i] = Find(parent, i);
            return roots;
        }

        // Count the members of each set: the count of the elements whose root is an element, at its index.
        // roots: the root of each element's set.
        private static int[] CountMembers(int[] roots)
        {
            int[] members = new int[roots.Length];
            foreach (int root in roots)
                members[root]++;
            return members;
        }

        // Find the root of an element's set, halving the path to it.
        // parent: each element's parent.
        // element: the element.
        private static int Find(int[] parent, int element)
        {
            while (parent[element] != element) {
                parent[element] = parent[parent[element]];
                element = parent[element];
            }
            return element;
        }

        // Join the sets of two elements.
        // parent: each element's parent.
        // a, b: the elements.
        private static void Join(int[] parent, int a, int b)
        {
            int rootA = Find(parent, a), rootB = Find(parent, b);
            if (rootA != rootB)
                parent[Math.Max(rootA, rootB)] = Math.Min(rootA, rootB);
        }
    }

    // An area cluster: the palette color coverages of one palette cutter of a hatching or a clipped area pattern whose
    // areas are connected by bounding boxes that overlap or touch, directly or through other areas of the cluster. The
    // pattern of an area cluster is written once, in the union of its areas: where the areas overlap, a pattern written
    // for each area lays the same lines and elements on each other, which a drawing that smooths the edges blends into a
    // darker pattern, and where they touch, the smoothed edges of a pattern brush leave a lighter seam. Each area is still
    // drawn from its own outline, and only the union cuts the pattern. The clusters and their unions are derived values of
    // the intermediate layer, the same for all channels.
    public sealed class AreaCluster
    {
        private readonly Map map;
        private readonly long madeAt;           // The change count of the map when the map made the cluster.
        private readonly IReadOnlyList<SymbolCoverage> symbolCoverages;
        private readonly RectangleF bounds;
        private ClippingObject union;            // The union of the areas, after the first read made it.

        // Make an area cluster.
        // map: the map that makes the cluster.
        // madeAt: the change count of the map.
        // symbolCoverages: the palette color coverages of the cluster, in the order of the palette separation.
        // bounds: the union of the bounding boxes of the paths of the areas.
        internal AreaCluster(Map map, long madeAt, IReadOnlyList<SymbolCoverage> symbolCoverages, RectangleF bounds)
        {
            this.map = map;
            this.madeAt = madeAt;
            this.symbolCoverages = symbolCoverages;
            this.bounds = bounds;
        }

        // The palette color coverages of the cluster, in the order of the palette separation.
        public IReadOnlyList<SymbolCoverage> SymbolCoverages
        {
            get
            {
                map.CheckDerivedValue(madeAt);
                return symbolCoverages;
            }
        }

        // The union of the bounding boxes of the paths of the areas, in the coordinates of the map. The pattern of the
        // cluster is the expansion of the coverage prototype of its first palette color coverage over it
        // (SymbolCoverage.ExpandPrototype(RectangleF)).
        public RectangleF Bounds
        {
            get
            {
                map.CheckDerivedValue(madeAt);
                return bounds;
            }
        }

        // The union of the areas: a clipping object, a filled path with the nonzero rule (AreaFillMode.Winding) that holds
        // all the contours of all the areas of the cluster. A contour winds positively when an even number of the other
        // contours of its area enclose it, and the other way when an odd number do (SymPathWithHoles.AddToUnionPathPartList),
        // so the nonzero rule fills of each area what the even-odd rule fills of it alone, and no area cancels another.
        // Each channel clips to it and fills it as any filled path. The union of a cluster of one area is the clipping
        // object of its palette color coverage, the area. It is a derived value of the cluster, which the first read makes.
        public ClippingObject Union
        {
            get
            {
                map.CheckDerivedValue(madeAt);

                ClippingObject made = Volatile.Read(ref union);
                if (made == null) {
                    made = MakeUnion();
                    made = Interlocked.CompareExchange(ref union, made, null) ?? made;
                }

                return made;
            }
        }

        public override string ToString()
        {
            return string.Format("area cluster of {0} palette color coverages", symbolCoverages.Count);
        }

        // Make the union of the areas.
        private ClippingObject MakeUnion()
        {
            if (symbolCoverages.Count == 1)
                return symbolCoverages[0].ClippingObjects[0];

            List<GraphicsPathPart> parts = new List<GraphicsPathPart>();
            foreach (SymbolCoverage coverage in symbolCoverages)
                ((AreaSymbol)coverage.Symbol).Path.AddToUnionPathPartList(parts);
            return new FilledShape(new PathShape(parts), AreaFillMode.Winding, Placement.Identity);
        }
    }
}
