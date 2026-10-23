using RectangleF = System.Drawing.RectangleF;
using SizeF = System.Drawing.SizeF;

namespace PurplePen.Graphics2D
{
    // A graphics target that keeps the structure of what it draws: the parts of drawings that a document defines once,
    // the labeled maps within them, each map's color runs, objects and place in the real world, and the pages that show
    // the parts. The map code makes these calls only to a target that implements this interface, so every other target
    // is unaffected.
    public interface IStructuredGraphicsTarget : IGraphicsTarget
    {
        // Begin a part of a drawing, which the document defines once for every page that shows it. Drawing happens
        // only within a part, and parts do not nest.
        // partKey: the key by which pages name the part.
        void BeginPart(object partKey);

        // End the part begun last, closing every group still open within it.
        void EndPart();

        // Begin a labeled map: a base map or a course drawing within a part, or a template within a map. A map begun in
        // several parts, such as the parts below and above a course, is one map in each.
        // mapKey: the key that identifies the map.
        // label: what the map is called, such as its file name.
        void BeginMap(object mapKey, string label);

        // End the map begun last, closing every group still open within it.
        void EndMap();

        // Begin a run of one color of the map that is open: what the map draws in that color, between two runs of the
        // colors below and above it. Runs do not nest.
        // colorKey: the key that identifies the color within its map.
        // position: the color's position in the map's color table, counting from 0 at the bottom.
        // name: the color's name.
        // color: the color's CMYK values, as the map stores them.
        // overprint: whether the color overprints the colors below it.
        void BeginColorRun(object colorKey, int position, string name, CmykColor color, bool overprint);

        // End the color run begun last, closing every group still open within it.
        void EndColorRun();

        // Begin an object of the map that is open, within a color run or, for an object of the image or layout layer,
        // outside any; everything drawn until EndObject is the object's. Objects do not nest.
        // objectKey: the key that identifies the object, the same in each of its runs.
        // symbolKey: the key that identifies the object's symbol within its map.
        // symbolId: the symbol's id, such as "101.0".
        // symbolName: the symbol's name.
        void BeginObject(object objectKey, object symbolKey, string symbolId, string symbolName);

        // End the object begun last, whose drawing has closed every group it opened.
        void EndObject();

        // Add a page that shows parts within an area of the map, transformed onto its paper. Pages are added after the
        // parts they show, outside any part.
        // paperSize: the size of the paper in millimetres.
        // mapArea: the area of map coordinates that the page shows; the parts are clipped to it.
        // mapToPage: the transform from map coordinates to millimetres on the paper, whose y axis points down.
        // partKeys: the parts the page shows, from the bottom up.
        void AddPage(SizeF paperSize, RectangleF mapArea, Matrix mapToPage, object[] partKeys);

        // Record where a map lies in the real world, with the map; a later call for the same map replaces it.
        // mapKey: the key of a map that has begun.
        // georeference: the transform from the map's coordinates to real-world coordinates, and the identifications of
        //               the coordinate system.
        void SetGeoreference(object mapKey, MapGeoreference georeference);
    }
}
