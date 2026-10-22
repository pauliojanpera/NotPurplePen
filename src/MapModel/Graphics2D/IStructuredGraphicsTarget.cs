using RectangleF = System.Drawing.RectangleF;
using SizeF = System.Drawing.SizeF;

namespace PurplePen.Graphics2D
{
    // A graphics target that keeps the structure of what it draws: the parts of drawings that a document defines once,
    // the labeled maps within them, and the pages that show the parts. The map code makes these calls only to a target
    // that implements this interface, so every other target is unaffected.
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

        // Add a page that shows parts within an area of the map, transformed onto its paper. Pages are added after the
        // parts they show, outside any part.
        // paperSize: the size of the paper in millimetres.
        // mapArea: the area of map coordinates that the page shows; the parts are clipped to it.
        // mapToPage: the transform from map coordinates to millimetres on the paper, whose y axis points down.
        // partKeys: the parts the page shows, from the bottom up.
        void AddPage(SizeF paperSize, RectangleF mapArea, Matrix mapToPage, object[] partKeys);
    }
}
