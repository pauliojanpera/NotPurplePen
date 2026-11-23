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

    }
}
