using System.Drawing;
using PurplePen.Graphics2D;
using PurplePen.MapModel;

namespace Map_Svg.Tests
{
    // Maps built for the tests of area patterns: squares of one area symbol, either patterned or hatched, its pattern
    // or its lines turned and moved off the middle of their tile, so that a pattern laid out from the wrong point or at
    // the wrong angle shows. Each square turns its pattern by an angle about a point of its own, as an OpenMapper
    // object stores a pattern origin and a rotation, where an OCAD object turns its pattern about the map's origin
    // (S-13).
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

        // Create a map of squares of one area symbol filled with a pattern of a dot with a line beside it, in offset
        // rows, turned by 30 degrees and moved off the middle of its tile.
        // irregular: whether the pattern is irregular, as OCAD's random structures are.
        // fillMode: how the pattern meets the edge of an area.
        // squares: where the squares lie and how each lays out its pattern.
        public static Map Create(bool irregular, AreaSymDef.PatternFillMode fillMode, params Square[] squares)
        {
            Map map = CreateEmptyMap();
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
                AddSquares(map, symdef, squares);
            }
            return map;
        }

        // Create a map of squares of one area symbol hatched twice: with thin green lines turned by 30 degrees, their
        // offset more than their spacing, which turn with each square; and with wide blue lines turned by 100 degrees,
        // wider than half their spacing and moved near its edge, so that the tile of a pattern fill holds two of them,
        // which never turn.
        // squares: where the squares lie and how each lays out its hatching.
        public static Map CreateHatched(params Square[] squares)
        {
            Map map = CreateEmptyMap();
            using (map.Write()) {
                SymColor blue = map.AddColor("Blue", 2, 1, 0.1F, 0, 0, false);
                SymColor green = map.AddColor("Green", 1, 0.76F, 0, 0.91F, 0, false);

                AreaSymDef symdef = new AreaSymDef("Hatched area", "402.0", null, null);
                symdef.AddHatching(Hatching(green, 0.15F, 0.6F, 30, 1.0F, AreaSymDef.RotateMode.Always));
                symdef.AddHatching(Hatching(blue, 0.35F, 0.7F, 100, 0.31F, AreaSymDef.RotateMode.Never));
                map.AddSymdef(symdef);
                AddSquares(map, symdef, squares);
            }
            return map;
        }

        // A hatching of an area symbol.
        // color: the color of its lines.
        // width: the width of its lines.
        // spacing: the distance between the middles of its lines.
        // angle: the angle its lines are turned by, in degrees.
        // offset: how far its lines are moved across from the point they are laid out from.
        // rotateMode: whether its lines turn with an area.
        private static AreaSymDef.HatchInfo Hatching(SymColor color, float width, float spacing, float angle, float offset, AreaSymDef.RotateMode rotateMode)
        {
            AreaSymDef.HatchInfo hatching = new AreaSymDef.HatchInfo();
            hatching.hatchColor = color;
            hatching.hatchWidth = width;
            hatching.hatchSpacing = spacing;
            hatching.hatchAngle = angle;
            hatching.hatchOffset = offset;
            hatching.hatchRotateMode = rotateMode;
            return hatching;
        }

        // Create a map without colors, symbols or objects, which measures text with Skia.
        private static Map CreateEmptyMap()
        {
            return new Map(new Skia_TextMetrics(), new Skia_FileLoader(System.AppContext.BaseDirectory));
        }

        // Add squares of an area symbol to a map.
        // map: the map, open for writing.
        // symdef: the area symbol.
        // squares: where the squares lie and how each lays out its pattern.
        private static void AddSquares(Map map, AreaSymDef symdef, Square[] squares)
        {
            foreach (Square square in squares) {
                float half = SideOfSquare / 2;
                float left = square.Middle.X - half, right = square.Middle.X + half, bottom = square.Middle.Y - half, top = square.Middle.Y + half;
                SymPath outline = new SymPath(new PointF[] {
                    new PointF(left, bottom), new PointF(right, bottom), new PointF(right, top), new PointF(left, top), new PointF(left, bottom)
                });
                map.AddSymbol(new AreaSymbol(symdef, new SymPathWithHoles(outline, null), square.Angle, square.PatternOrigin));
            }
        }
    }
}
