using System.Collections.Generic;
using RectangleF = System.Drawing.RectangleF;
using SizeF = System.Drawing.SizeF;

namespace PurplePen.Graphics2D
{
    // A graphics target that keeps the structure of what it draws: the parts of drawings that a document defines once,
    // the labeled maps within them, the objects that each map draws beside its palette colors and its place in the real
    // world, and the pages that show the parts. The map code makes these calls only to a target that implements this
    // interface, so every other target is unaffected. The palette colors of a map are not drawn through this interface:
    // an encoding writes them (IPaletteStackEncoding of MapModel), and a target that keeps structure gives one.
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

        // Begin an object of the map that is open, which the map draws beside its palette colors: an object of the image
        // or the layout layer. Everything drawn until EndObject is the object's. Objects do not nest.
        // objectKey: the text form of the key of the object, its symbol key (Symbol.Key of MapModel), which identifies it
        //            in its map.
        // symbolKey: the key that identifies the object's symbol definition within its map.
        // symbolId: the symbol definition's id, such as "101.0".
        // symbolName: the symbol definition's name.
        void BeginObject(string objectKey, object symbolKey, string symbolId, string symbolName);

        // End the object begun last, whose drawing has closed every group it opened.
        void EndObject();

        // Add a page that shows parts within an area of the map, transformed onto its paper. Pages are added after the
        // parts they show, outside any part.
        // paperSize: the size of the paper in millimetres.
        // mapArea: the area of map coordinates that the page shows; the parts are clipped to it.
        // mapToPage: the transform from map coordinates to millimetres on the paper, whose y axis points down.
        // partKeys: the parts the page shows, from the bottom up.
        // name: what the page is called, such as the name of the course part it shows, or null for no name.
        void AddPage(SizeF paperSize, RectangleF mapArea, Matrix mapToPage, object[] partKeys, string name);

        // Record where a map lies in the real world, with the map; a later call for the same map replaces it.
        // mapKey: the key of a map that has begun.
        // georeference: the transform from the map's coordinates to real-world coordinates, and the identifications of
        //               the coordinate system.
        void SetGeoreference(object mapKey, MapGeoreference georeference);

        // Describe the course that a map draws, with the map (CD-1); a later call for the same map replaces it.
        // mapKey: the key of a map that has begun.
        // course: the course.
        void DescribeCourse(object mapKey, CourseRecord course);

        // Describe the course objects that a course added to a map, by the text forms of their keys (CourseObj.Key of
        // PurplePenCore), on which the keys of their symbols in the map build: the symbol key of a course object's symbol
        // is the key of the course object, a slash and the index of the symbol (CD-2, CD-3). A later call for the same map
        // replaces it. A course object whose symbols draw nothing in the map is not described.
        // mapKey: the key of a map that has begun.
        // objects: for the key of each course object, what the course object draws.
        void DescribeObjects(object mapKey, IReadOnlyDictionary<string, CourseObjectRecord> objects);

        // Whether the target's pattern brushes are vectors: a brush's tile keeps what is drawn into it as drawing, which
        // no size of the tile pixelates, rather than as a bitmap of the size CreatePatternBrush suggests. MapDisplay then
        // asks for pattern brushes for such a target even when printing, and the encoding of the target writes the
        // hatchings and the regular clipped area patterns as pattern fills of vectors however fine the resolution.
        bool PatternBrushesAreVectors { get; }
    }
}
