namespace PurplePen.Graphics2D
{
    // Where a map lies in the real world: the affine transform from the map's coordinates, in millimetres of map, to
    // real-world coordinates, and every identification of the real-world coordinate system that the map has (G-1, G-2).
    // A plain value, which the caller computes and a target that keeps the structure of what it draws records with
    // its map.
    public sealed class MapGeoreference
    {
        // The affine transform from map coordinates to real-world coordinates, whose elements follow Matrix's order:
        // a map point (x, y) lies at (A x + C y + E, B x + D y + F) in the real world.
        public readonly double A, B, C, D, E, F;

        // The EPSG code of the coordinate system, or 0 when the map has none.
        public readonly int EpsgCode;

        // The Proj4 string of the coordinate system, or null when the map has none.
        public readonly string Proj4String;

        // The OCAD grid and zone id of the coordinate system, or 0 when the map has none.
        public readonly int OcadGridAndZone;

        // Create a georeference.
        // a, b, c, d, e, f: the elements of the transform from map coordinates to real-world coordinates.
        // epsgCode: the EPSG code of the coordinate system, or 0 for none.
        // proj4String: the Proj4 string of the coordinate system, or null for none.
        // ocadGridAndZone: the OCAD grid and zone id of the coordinate system, or 0 for none.
        public MapGeoreference(double a, double b, double c, double d, double e, double f, int epsgCode, string proj4String, int ocadGridAndZone)
        {
            A = a;
            B = b;
            C = c;
            D = d;
            E = e;
            F = f;
            EpsgCode = epsgCode;
            Proj4String = proj4String;
            OcadGridAndZone = ocadGridAndZone;
        }
    }
}
