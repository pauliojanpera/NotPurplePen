using System.Drawing;
using PurplePen.Graphics2D;
using PurplePen.MapModel;

namespace Map_Svg.Tests
{
    // Maps built for the tests of area patterns: squares of one area symbol whose pattern is a dot with a line beside
    // it, in offset rows, turned by 30 degrees and moved off the middle of its tile, so that a pattern laid out from the
    // wrong point or at the wrong angle shows. Each square turns its pattern by an angle about a point of its own, as
    // an OpenMapper object stores a pattern origin and a rotation, where an OCAD object turns its pattern about the
    // map's origin (S-13).
    internal static class PatternedAreaMap
    {
        // Where a square lies and how it lays out its pattern.
        public struct Square
        {
            // The middle of the square, in map coordinates.
            public readonly PointF Middle;

            // The angle the square turns its pattern by, in degrees.
            public readonly float Angle;

            // The point the square lays its pattern out from, in map coordinates.
            public readonly PointF PatternOrigin;

            // Create the placement of a square.
            // middle: the middle of the square.
            // angle: the angle it turns its pattern by.
            // patternOrigin: the point it lays its pattern out from.
            public Square(PointF middle, float angle, PointF patternOrigin)
            {
                Middle = middle;
                Angle = angle;
                PatternOrigin = patternOrigin;
            }
        }

        // The length of a square's side, in millimetres.
        public const float SideOfSquare = 16;

        // The squares of the comparison's case of pattern origins, in the quarters of the area around the map's origin:
        // unturned with the map's origin as its pattern's, unturned with a moved origin, turned about the map's origin,
        // and turned about a moved origin.
        public static readonly Square[] SquaresInEveryPlacement = {
            new Square(new PointF(-10, 10), 0, new PointF(0, 0)),
            new Square(new PointF(10, 10), 0, new PointF(10.7F, 10.45F)),
            new Square(new PointF(-10, -10), 25, new PointF(0, 0)),
            new Square(new PointF(10, -10), 25, new PointF(10.7F, -9.55F)),
        };

        // The area that holds the squares of SquaresInEveryPlacement, with a margin around them.
        public static readonly RectangleF AreaOfEveryPlacement = new RectangleF(-20, -20, 40, 40);

        // Create a map of squares of one area symbol filled with the pattern.
        // irregular: whether the pattern is irregular, as OCAD's random structures are.
        // fillMode: how the pattern meets the edge of an area.
        // squares: where the squares lie and how each lays out its pattern.
        public static Map Create(bool irregular, AreaSymDef.PatternFillMode fillMode, params Square[] squares)
        {
            Map map = new Map(new Skia_TextMetrics(), new Skia_FileLoader(System.AppContext.BaseDirectory));
            using (map.Write()) {
                SymColor color = map.AddColor("Brown", 1, 0, 0.56F, 1, 0.18F, false);

                Glyph glyph = new Glyph();
                glyph.AddFilledCircle(color, new PointF(0, 0), 0.6F);
                glyph.AddLine(color, new SymPath(new PointF[] { new PointF(0.2F, 0), new PointF(0.8F, 0.3F) }), 0.12F, LineJoinMode.Miter, LineCapMode.Flat);
                glyph.ConstructionComplete();

                AreaSymDef.PatternInfo pattern = new AreaSymDef.PatternInfo();
                pattern.patternWidth = 2;
                pattern.patternHeight = 1.6F;
                pattern.patternAngle = 30;
                pattern.patternOffsetX = 0.3F;
                pattern.patternOffsetY = -0.2F;
                pattern.offsetRows = true;
                pattern.patternGlyph = glyph;
                pattern.patternRotateMode = AreaSymDef.RotateMode.Always;
                pattern.patternFillMode = fillMode;
                pattern.irregular = irregular;
                if (irregular) {
                    pattern.irregularVarX = 0.5F;
                    pattern.irregularVarY = 0.5F;
                }

                AreaSymDef symdef = new AreaSymDef("Dotted area", "401.0", null, null);
                symdef.AddPattern(pattern);
                map.AddSymdef(symdef);

                foreach (Square square in squares) {
                    float half = SideOfSquare / 2;
                    float left = square.Middle.X - half, right = square.Middle.X + half, bottom = square.Middle.Y - half, top = square.Middle.Y + half;
                    SymPath outline = new SymPath(new PointF[] {
                        new PointF(left, bottom), new PointF(right, bottom), new PointF(right, top), new PointF(left, top), new PointF(left, bottom)
                    });
                    map.AddSymbol(new AreaSymbol(symdef, new SymPathWithHoles(outline, null), square.Angle, square.PatternOrigin));
                }
            }
            return map;
        }
    }
}
