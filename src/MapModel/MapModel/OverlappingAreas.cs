using System;
using System.Collections.Generic;
using System.Drawing;

namespace PurplePen.MapModel
{
    // The areas of one symbol, among those a color draws, whose bounding boxes overlap or touch others laid out in the
    // same frame of their patterns: the same angle and rotation center. Map.DrawColor draws the hatchings and regular
    // clipped patterns of each such group once, under the union of the areas' outlines, so that where the areas
    // overlap a pattern is drawn once rather than once for each area, which a renderer that smooths edges would blend
    // into a thicker, darker pattern. An area that overlaps no other of its frame is in no group, and is drawn as
    // before.
    internal sealed class OverlappingAreas
    {
        // A group of two or more areas, connected by overlapping bounding boxes, in the order they are drawn.
        internal sealed class Group
        {
            // The areas' paths, in the order the areas are drawn.
            public readonly List<SymPathWithHoles> PathList = new List<SymPathWithHoles>();

            // The area drawn last, after which the group's patterns are drawn.
            public Symbol Last;

            // The union of the bounding boxes of the areas' paths.
            public RectangleF Bounds;

            // The angle and rotation center that every area of the group lays its patterns out with.
            public float Angle;
            public PointF RotationCenter;

            // The areas' paths, in the order the areas are drawn.
            public SymPathWithHoles[] Paths
            {
                get { return PathList.ToArray(); }
            }
        }

        // The group of each area that is in one.
        private readonly Dictionary<Symbol, Group> groupOfSymbol = new Dictionary<Symbol, Group>();

        // The group of an area, or null when it overlaps no other area of its symbol and frame.
        // symbol: the area.
        public Group GroupOf(Symbol symbol)
        {
            Group group;
            return groupOfSymbol.TryGetValue(symbol, out group) ? group : null;
        }

        // Find the groups among the objects of a symbol that a drawing of a rectangle draws: those Map.DrawColor
        // draws, visible and possibly within the rectangle.
        // symbols: the objects of the symbol, in the order they are drawn.
        // rect: the rectangle drawn.
        public static OverlappingAreas Find(List<Symbol> symbols, RectangleF rect)
        {
            OverlappingAreas result = new OverlappingAreas();

            // The drawn areas, by the frame of their patterns.
            Dictionary<Tuple<float, float, float>, List<AreaSymbol>> byFrame = new Dictionary<Tuple<float, float, float>, List<AreaSymbol>>();
            foreach (Symbol symbol in symbols) {
                AreaSymbol area = symbol as AreaSymbol;
                if (area == null || !area.IsVisible || !area.BoundingBox.IntersectsWith(rect) || !area.MayIntersectRect(rect))
                    continue;
                Tuple<float, float, float> frame = Tuple.Create(area.Angle, area.RotationCenter.X, area.RotationCenter.Y);
                List<AreaSymbol> areas;
                if (!byFrame.TryGetValue(frame, out areas)) {
                    areas = new List<AreaSymbol>();
                    byFrame.Add(frame, areas);
                }
                areas.Add(area);
            }

            foreach (List<AreaSymbol> areas in byFrame.Values) {
                if (areas.Count > 1)
                    result.GroupAreas(areas);
            }
            return result;
        }

        // Group the areas of one frame whose bounding boxes overlap or touch, by a sweep across them from left to
        // right, joining each area to those still open whose boxes it meets.
        // areas: the areas, in the order they are drawn.
        private void GroupAreas(List<AreaSymbol> areas)
        {
            int count = areas.Count;
            RectangleF[] boxes = new RectangleF[count];
            int[] parent = new int[count];
            int[] order = new int[count];
            for (int i = 0; i < count; ++i) {
                boxes[i] = areas[i].Path.BoundingBox;
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

            Dictionary<int, Group> groupOfRoot = new Dictionary<int, Group>();
            int[] members = new int[count];
            for (int i = 0; i < count; ++i)
                members[Find(parent, i)]++;
            for (int i = 0; i < count; ++i) {
                int root = Find(parent, i);
                if (members[root] < 2)
                    continue;
                Group group;
                if (!groupOfRoot.TryGetValue(root, out group)) {
                    group = new Group { Bounds = boxes[i], Angle = areas[i].Angle, RotationCenter = areas[i].RotationCenter };
                    groupOfRoot.Add(root, group);
                }
                group.PathList.Add(areas[i].Path);
                group.Bounds = RectangleF.Union(group.Bounds, boxes[i]);
                group.Last = areas[i];
                groupOfSymbol.Add(areas[i], group);
            }
        }

        // The root of an element's set, halving the path to it.
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
}
