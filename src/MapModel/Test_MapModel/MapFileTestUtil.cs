#if TEST
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TestingUtils;

namespace PurplePen.MapModel.Tests
{
    // Reads the test maps, writes maps in a format and reads them again, and finds the colors of a map.
    static class MapFileTestUtil
    {
        // Read the map file at a path.
        public static Map ReadMap(string path)
        {
            Map map = new Map(new GDIPlus_TextMetrics(), new GDIPlus_FileLoader(Path.GetDirectoryName(path)));
            InputOutput.ReadFile(path, map);
            return map;
        }

        // Read a test map.
        public static Map LoadMap(string fileName)
        {
            return ReadMap(TestUtil.GetTestFile(fileName));
        }

        // Write a map in a format, and return the path of the file: the test file's, with a suffix that names the
        // format and ends in _temp.
        public static string WriteMap(Map map, string fileName, MapFileFormat format)
        {
            string extension = (format.kind == MapFileFormatKind.OCAD) ? ".ocd" : ".omap";
            string path = Path.ChangeExtension(TestUtil.GetTestFile(fileName), null) + string.Format(" {0}{1}_temp{2}", format.kind, format.version, extension);
            InputOutput.WriteFile(path, map, format);
            return path;
        }

        // Write a map in a format, and read the file again into a new map.
        public static Map WriteAndRead(Map map, string fileName, MapFileFormat format)
        {
            return ReadMap(WriteMap(map, fileName, format));
        }

        // The file format of a kind and a version.
        public static MapFileFormat Format(MapFileFormatKind kind, int version)
        {
            if (kind == MapFileFormatKind.OpenMapper)
                return new MapFileFormat(MapFileFormatKind.OpenMapper, OpenMapperSubKind.OMap, version);
            else
                return new MapFileFormat(kind, version);
        }

        // The colors of a map, the top one first.
        public static List<SymColor> ColorsTopFirst(Map map)
        {
            using (map.Read())
                return map.AllColors.Reverse().ToList();
        }

        // The one color of a map that has a name.
        public static SymColor ColorNamed(Map map, string name)
        {
            return ColorsTopFirst(map).Single(color => color.Name == name);
        }

        // A map with the colors and the inks of a map, and no symbols. The OCAD export does not write all kinds of
        // symbols that the Mapper import makes.
        public static Map ColorsOnly(Map map)
        {
            using (map.Read())
                return map.Clone(new Map.CloneOptions() { CloneColors = true });
        }
    }
}
#endif
