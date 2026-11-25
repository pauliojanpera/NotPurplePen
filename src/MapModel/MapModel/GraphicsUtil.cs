using System;
using System.Collections.Generic;
using System.Text;


namespace PurplePen.MapModel
{
    using PurplePen.Graphics2D;

    public static class GraphicsUtil
    {
        public static float MITER_LIMIT = 5.0F;

        // Create a solid pen
        public static void CreateSolidPen(IGraphicsTarget g, object penKey, CmykColor color, float thickness, LineStyle style)
        {
            switch (style)
            {
                case LineStyle.Rounded:
                    g.CreatePen(penKey, color, thickness, LineCapMode.Round, LineJoinMode.Round, MITER_LIMIT); break;
                case LineStyle.Beveled:
                    g.CreatePen(penKey, color, thickness, LineCapMode.Flat, LineJoinMode.Bevel, MITER_LIMIT); break;
                case LineStyle.Mitered:
                    g.CreatePen(penKey, color, thickness, LineCapMode.Flat, LineJoinMode.Miter, MITER_LIMIT); break;
                case LineStyle.FlatRounded:
                    g.CreatePen(penKey, color, thickness, LineCapMode.Flat, LineJoinMode.Round, MITER_LIMIT); break;
                default:
                    throw new ArgumentException();
            }
        }

        // Create a solid pen
        public static void CreateSolidPen(IGraphicsTarget g, object penKey, object brushKey, float thickness, LineStyle style)
        {
            switch (style) {
                case LineStyle.Rounded:
                    g.CreatePen(penKey, brushKey, thickness, LineCapMode.Round, LineJoinMode.Round, MITER_LIMIT); break;
                case LineStyle.Beveled:
                    g.CreatePen(penKey, brushKey, thickness, LineCapMode.Flat, LineJoinMode.Bevel, MITER_LIMIT); break;
                case LineStyle.Mitered:
                    g.CreatePen(penKey, brushKey, thickness, LineCapMode.Flat, LineJoinMode.Miter, MITER_LIMIT); break;
                case LineStyle.FlatRounded:
                    g.CreatePen(penKey, brushKey, thickness, LineCapMode.Flat, LineJoinMode.Round, MITER_LIMIT); break;
                default:
                    throw new ArgumentException();
            }
        }

        // The pen of a clipping object that a solid pen draws, as CreateSolidPen makes the pen.
        // thickness: the width of the line.
        // style: the style of the line, which gives its caps and joins.
        internal static Stroke MakeSolidStroke(float thickness, LineStyle style)
        {
            switch (style) {
                case LineStyle.Rounded:
                    return new Stroke(thickness, LineCapMode.Round, LineJoinMode.Round, MITER_LIMIT);
                case LineStyle.Beveled:
                    return new Stroke(thickness, LineCapMode.Flat, LineJoinMode.Bevel, MITER_LIMIT);
                case LineStyle.Mitered:
                    return new Stroke(thickness, LineCapMode.Flat, LineJoinMode.Miter, MITER_LIMIT);
                case LineStyle.FlatRounded:
                    return new Stroke(thickness, LineCapMode.Flat, LineJoinMode.Round, MITER_LIMIT);
                default:
                    throw new ArgumentException();
            }
        }

        // Does the stroke shape of a polyline cover a point: the band that a pen covers along the polyline, half its width
        // to each side, with its caps at the ends of an open polyline and its joins at the vertices, as the channels
        // stroke a path. A miter join is beveled where its miter would reach farther from the vertex than the miter limit
        // times half the width, as in PDF, SVG and Skia.
        // points: the points of the polyline. A closed polyline can repeat its first point at its end.
        // isClosed: whether the polyline is closed, as a polygon.
        // width: the width of the pen.
        // caps: the caps at the ends of an open polyline.
        // join: the joins at the vertices.
        // miterLimit: the miter limit, as a multiple of half the width.
        // point: the point.
        internal static bool StrokeShapeCovers(System.Drawing.PointF[] points, bool isClosed, float width, LineCapMode caps, LineJoinMode join, float miterLimit, System.Drawing.PointF point)
        {
            double half = width / 2.0;
            if (!(half > 0))
                return false;

            // The vertices, without a point that repeats the point before it.
            List<System.Drawing.PointF> vertices = new List<System.Drawing.PointF>(points.Length);
            foreach (System.Drawing.PointF vertex in points) {
                if (vertices.Count == 0 || vertices[vertices.Count - 1] != vertex)
                    vertices.Add(vertex);
            }
            if (isClosed && vertices.Count > 1 && vertices[0] == vertices[vertices.Count - 1])
                vertices.RemoveAt(vertices.Count - 1);

            int count = vertices.Count;
            if (count == 0)
                return false;
            if (count == 1) {
                // A path of no length: a round cap is a dot, and a square cap a square along the axes.
                double dx = point.X - vertices[0].X, dy = point.Y - vertices[0].Y;
                if (caps == LineCapMode.Round)
                    return dx * dx + dy * dy <= half * half;
                return caps == LineCapMode.Square && Math.Abs(dx) <= half && Math.Abs(dy) <= half;
            }

            int segmentCount = isClosed ? count : count - 1;
            for (int i = 0; i < segmentCount; ++i) {
                if (BandOfSegmentCovers(vertices[i], vertices[(i + 1) % count], half, point))
                    return true;
            }

            if (!isClosed && (CapCovers(vertices[1], vertices[0], half, caps, point) || CapCovers(vertices[count - 2], vertices[count - 1], half, caps, point)))
                return true;

            int firstJoin = isClosed ? 0 : 1, lastJoin = isClosed ? count - 1 : count - 2;
            for (int i = firstJoin; i <= lastJoin; ++i) {
                if (JoinCovers(vertices[(i + count - 1) % count], vertices[i], vertices[(i + 1) % count], half, join, miterLimit, point))
                    return true;
            }

            return false;
        }

        // Does the band of a segment cover a point: the rectangle along the segment, half a width to each side of it.
        // start, end: the ends of the segment, which differ.
        // half: half the width of the pen.
        // point: the point.
        private static bool BandOfSegmentCovers(System.Drawing.PointF start, System.Drawing.PointF end, double half, System.Drawing.PointF point)
        {
            double dx = end.X - start.X, dy = end.Y - start.Y;
            double length = Math.Sqrt(dx * dx + dy * dy);
            double px = point.X - start.X, py = point.Y - start.Y;
            double along = (px * dx + py * dy) / length;
            double across = (dx * py - dy * px) / length;
            return along >= 0 && along <= length && Math.Abs(across) <= half;
        }

        // Does the cap at an end of a polyline cover a point beyond the end: a disk for a round cap, and half a square for
        // a square cap. A flat cap covers nothing beyond the end.
        // before: the vertex before the end, which gives the direction of the end.
        // end: the end.
        // half: half the width of the pen.
        // caps: the caps.
        // point: the point.
        private static bool CapCovers(System.Drawing.PointF before, System.Drawing.PointF end, double half, LineCapMode caps, System.Drawing.PointF point)
        {
            double px = point.X - end.X, py = point.Y - end.Y;
            switch (caps) {
                case LineCapMode.Round:
                    return px * px + py * py <= half * half;

                case LineCapMode.Square:
                    double dx = end.X - before.X, dy = end.Y - before.Y;
                    double length = Math.Sqrt(dx * dx + dy * dy);
                    double along = (px * dx + py * dy) / length;
                    double across = (dx * py - dy * px) / length;
                    return along >= 0 && along <= half && Math.Abs(across) <= half;

                default:
                    return false;
            }
        }

        // Does the join at a vertex of a polyline cover a point beyond the bands of its two segments: a disk for a round
        // join; on the outer side of the turn, the triangle of the vertex and the outer corners of the two bands for a
        // bevel join, and with it the triangle of those corners and the tip where the outer edges of the bands meet for
        // a miter join whose miter is within the miter limit.
        // before, vertex, after: the vertex and the vertices before and after it, which differ from it.
        // half: half the width of the pen.
        // join: the join.
        // miterLimit: the miter limit, as a multiple of half the width.
        // point: the point.
        private static bool JoinCovers(System.Drawing.PointF before, System.Drawing.PointF vertex, System.Drawing.PointF after, double half, LineJoinMode join, float miterLimit, System.Drawing.PointF point)
        {
            if (join == LineJoinMode.Round) {
                double px = point.X - vertex.X, py = point.Y - vertex.Y;
                return px * px + py * py <= half * half;
            }

            // The directions of the two segments, as unit vectors, and the side of the turn.
            double length1 = Math.Sqrt((vertex.X - before.X) * (double)(vertex.X - before.X) + (vertex.Y - before.Y) * (double)(vertex.Y - before.Y));
            double length2 = Math.Sqrt((after.X - vertex.X) * (double)(after.X - vertex.X) + (after.Y - vertex.Y) * (double)(after.Y - vertex.Y));
            double d1x = (vertex.X - before.X) / length1, d1y = (vertex.Y - before.Y) / length1;
            double d2x = (after.X - vertex.X) / length2, d2y = (after.Y - vertex.Y) / length2;
            double cross = d1x * d2y - d1y * d2x;
            if (cross == 0)
                return false;   // The segments go on straight, or back along the same line, and the bands meet without a join.

            // The outer corners of the two bands, on the side opposite to the turn. The left normal of (dx, dy) is
            // (-dy, dx).
            double side = (cross > 0) ? -1 : 1;
            double ax = vertex.X - side * half * d1y, ay = vertex.Y + side * half * d1x;
            double bx = vertex.X - side * half * d2y, by = vertex.Y + side * half * d2x;
            if (TriangleCovers(vertex.X, vertex.Y, ax, ay, bx, by, point))
                return true;
            if (join == LineJoinMode.Bevel)
                return false;

            // The miter reaches from the vertex half the width divided by the cosine of half the turn.
            double cosineOfHalfTurn = Math.Sqrt(Math.Max(0, (1 + d1x * d2x + d1y * d2y) / 2));
            if (!(cosineOfHalfTurn > 0) || 1 / cosineOfHalfTurn > miterLimit)
                return false;
            double nx = -(d1y + d2y), ny = d1x + d2x;
            double normalLength = Math.Sqrt(nx * nx + ny * ny);
            double reach = half / cosineOfHalfTurn;
            double tipX = vertex.X + side * reach * nx / normalLength, tipY = vertex.Y + side * reach * ny / normalLength;
            return TriangleCovers(ax, ay, tipX, tipY, bx, by, point);
        }

        // Does a triangle cover a point, its edges included?
        private static bool TriangleCovers(double x1, double y1, double x2, double y2, double x3, double y3, System.Drawing.PointF point)
        {
            double c1 = (x2 - x1) * (point.Y - y1) - (y2 - y1) * (point.X - x1);
            double c2 = (x3 - x2) * (point.Y - y2) - (y3 - y2) * (point.X - x2);
            double c3 = (x1 - x3) * (point.Y - y3) - (y1 - y3) * (point.X - x3);
            return (c1 >= 0 && c2 >= 0 && c3 >= 0) || (c1 <= 0 && c2 <= 0 && c3 <= 0);
        }

        // Whether a target's pattern brushes are vectors, which a tile of any size draws without loss: a target that keeps
        // the structure of what it draws, and says so, as the SVG target does.
        // g: the target.
        public static bool PatternBrushesAreVectors(IGraphicsTarget g)
        {
            IStructuredGraphicsTarget structuredTarget = g as IStructuredGraphicsTarget;
            return structuredTarget != null && structuredTarget.PatternBrushesAreVectors;
        }
    }
}
