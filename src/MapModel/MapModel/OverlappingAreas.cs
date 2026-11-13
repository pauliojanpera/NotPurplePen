/* Copyright (c) 2026, Peter Golde
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
using System.Drawing;
using PurplePen.Graphics2D;

namespace PurplePen.MapModel
{
    // The areas of one symbol, among those a color draws, whose bounding boxes overlap or touch others that lay their
    // patterns out in the same frame. Map.DrawColor draws the hatchings and the clipped patterns of each such group
    // once, after the last of its areas, within the union of the areas' outlines: where areas overlap, a pattern drawn
    // for each area lays the same lines and elements on each other, which a renderer that smooths edges blends into a
    // thicker, darker pattern. Each area is still drawn from its own outline, and only the pattern is drawn once. An
    // area that meets no other of its frame is in no group, and is drawn as before, as is an area whose main path and
    // holes cross or touch each other or themselves.
    internal sealed class OverlappingAreas
    {
        // A group of two or more areas, connected by bounding boxes that overlap or touch.
        internal sealed class Group
        {
            // The areas' paths and their bounding boxes, in the order the areas are drawn, and the path of each area.
            private readonly List<SymPathWithHoles> paths = new List<SymPathWithHoles>();
            private readonly List<RectangleF> pathBounds = new List<RectangleF>();
            private readonly Dictionary<Symbol, SymPathWithHoles> pathOfArea = new Dictionary<Symbol, SymPathWithHoles>();

            // The key of the path of the union of the areas, once it is created on the target drawn onto.
            private object unionPathKey;

            // The area drawn last, after which the group's patterns are drawn.
            public Symbol Last { get; private set; }

            // The union of the bounding boxes of the areas' paths.
            public RectangleF Bounds { get; private set; }

            // The angle, in degrees, and the rotation center that every area of the group lays its patterns out with,
            // unless the patterns never turn with their areas.
            public float Angle { get; private set; }
            public PointF RotationCenter { get; private set; }

            // The bounding boxes of the areas' paths, in the order the areas are drawn.
            public IList<RectangleF> PathBounds
            {
                get { return pathBounds; }
            }

            // Create a group with its first area.
            // area: the area.
            // pathBox: the bounding box of the area's path.
            public Group(AreaSymbol area, RectangleF pathBox)
            {
                Bounds = pathBox;
                Angle = area.Angle;
                RotationCenter = area.RotationCenter;
                Add(area, pathBox);
            }

            // Add an area drawn after those added before it.
            // area: the area.
            // pathBox: the bounding box of the area's path.
            public void Add(AreaSymbol area, RectangleF pathBox)
            {
                SymPathWithHoles path = area.Path;
                paths.Add(path);
                pathBounds.Add(pathBox);
                pathOfArea.Add(area, path);
                Bounds = RectangleF.Union(Bounds, pathBox);
                Last = area;
            }

            // The path of an area of the group, the one the union of the outlines is made of: an area makes a new path
            // object each time it is asked for its path, and a target that keeps the structure of what it draws knows
            // the outlines of a union by their paths' keys (S-12).
            // area: the area.
            public SymPathWithHoles PathOf(Symbol area)
            {
                return pathOfArea[area];
            }

            // Get the key of the path of the union of the areas' outlines on a target, creating the path the first
            // time. The path holds each area's main path winding one way and its holes the other, and fills by the
            // nonzero rule, so that every target fills and clips to the union however its areas overlap or touch. A
            // target that keeps the structure of what it draws is told which areas' paths the union unites, whose
            // outlines it has been given in their objects' places (S-12).
            // g: the target drawn onto, the only one a group is drawn onto.
            public object GetUnionPathKey(IGraphicsTarget g)
            {
                if (unionPathKey == null || !g.HasPath(unionPathKey)) {
                    List<GraphicsPathPart> parts = new List<GraphicsPathPart>();
                    foreach (SymPathWithHoles path in paths)
                        path.AddToUnionPathPartList(parts);

                    unionPathKey = new object();
                    g.CreatePath(unionPathKey, parts, AreaFillMode.Winding);

                    IStructuredGraphicsTarget structuredTarget = g as IStructuredGraphicsTarget;
                    if (structuredTarget != null) {
                        object[] pathKeys = new object[paths.Count];
                        for (int i = 0; i < paths.Count; ++i)
                            pathKeys[i] = paths[i].GetPathKey(g);
                        structuredTarget.DescribeUnion(unionPathKey, pathKeys);
                    }
                }

                return unionPathKey;
            }
        }

        // The group of each area that is in one.
        private readonly Dictionary<Symbol, Group> groupOfSymbol = new Dictionary<Symbol, Group>();

        // The group of an area, or null when it meets no other area of its symbol and frame.
        // symbol: the area.
        public Group GroupOf(Symbol symbol)
        {
            Group group;
            return groupOfSymbol.TryGetValue(symbol, out group) ? group : null;
        }

        // Find the groups among the objects of a symbol that Map.DrawColor draws in a rectangle: those visible and
        // possibly within it.
        // symbols: the objects of the symbol, in the order they are drawn.
        // rect: the rectangle drawn.
        // framesTurnWithAreas: whether the patterns grouped are laid out by each area's angle and rotation center, so
        //                      that only areas that share both share a frame; when false, the patterns never turn, and
        //                      every area of the symbol shares one frame.
        public static OverlappingAreas Find(List<Symbol> symbols, RectangleF rect, bool framesTurnWithAreas)
        {
            OverlappingAreas result = new OverlappingAreas();

            // The areas drawn, by the frame their patterns are laid out in.
            Dictionary<Tuple<float, float, float>, List<AreaSymbol>> byFrame = new Dictionary<Tuple<float, float, float>, List<AreaSymbol>>();
            foreach (Symbol symbol in symbols) {
                AreaSymbol area = symbol as AreaSymbol;
                if (area == null || !area.IsVisible || !area.BoundingBox.IntersectsWith(rect) || !area.MayIntersectRect(rect))
                    continue;

                Tuple<float, float, float> frame = framesTurnWithAreas ? Tuple.Create(area.Angle, area.RotationCenter.X, area.RotationCenter.Y)
                                                                       : Tuple.Create(0F, 0F, 0F);
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

        // Group the areas of one frame whose bounding boxes overlap or touch. An area whose contours meet each other or
        // themselves is left out, and draws its own patterns as an area in no group does: the path of a group's union
        // fills an area as the even-odd rule fills it by itself only when no two of its contours meet (see
        // SymPathWithHoles.AddToUnionPathPartList), and the other areas are grouped again without it. Only the areas
        // that meet another are looked at for that.
        // areas: the areas, in the order they are drawn.
        private void GroupAreas(List<AreaSymbol> areas)
        {
            RectangleF[] boxes = BoundingBoxes(areas);
            int[] roots = Connect(boxes);
            int[] members = CountMembers(roots);

            List<AreaSymbol> areasThatMeetOthers = new List<AreaSymbol>();
            bool anyLeftOut = false;
            for (int i = 0; i < areas.Count; ++i) {
                if (members[roots[i]] < 2)
                    continue;

                if (areas[i].ContoursMeet)
                    anyLeftOut = true;
                else
                    areasThatMeetOthers.Add(areas[i]);
            }

            if (anyLeftOut) {
                areas = areasThatMeetOthers;
                boxes = BoundingBoxes(areas);
                roots = Connect(boxes);
                members = CountMembers(roots);
            }

            Dictionary<int, Group> groupOfRoot = new Dictionary<int, Group>();
            for (int i = 0; i < areas.Count; ++i) {
                int root = roots[i];
                if (members[root] < 2)
                    continue;

                Group group;
                if (groupOfRoot.TryGetValue(root, out group)) {
                    group.Add(areas[i], boxes[i]);
                }
                else {
                    group = new Group(areas[i], boxes[i]);
                    groupOfRoot.Add(root, group);
                }
                groupOfSymbol.Add(areas[i], group);
            }
        }

        // The bounding boxes of areas' paths.
        // areas: the areas.
        private static RectangleF[] BoundingBoxes(List<AreaSymbol> areas)
        {
            RectangleF[] boxes = new RectangleF[areas.Count];
            for (int i = 0; i < areas.Count; ++i)
                boxes[i] = areas[i].Path.BoundingBox;
            return boxes;
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
}
