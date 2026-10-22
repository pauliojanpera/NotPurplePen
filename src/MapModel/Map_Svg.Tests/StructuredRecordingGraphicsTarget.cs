using System.Collections.Generic;
using PurplePen.Graphics2D;
using PurplePen.MapModel;

using RectangleF = System.Drawing.RectangleF;
using SizeF = System.Drawing.SizeF;

namespace Map_Svg.Tests
{
    // A recorder that keeps the structure of what is drawn into it, so that the map code makes its structure calls to
    // it as it does to the SVG target, and plays them back in their places among the drawing calls (DL-4). Played back
    // onto a target that does not keep structure, it draws as the plain recorder does. Like the drawing calls' keys,
    // the keys of parts, maps, colors, objects and symbols are replaced by objects of the recorder's own, which stand
    // for them with the same identity, so no key of the caller's is held once recording ends.
    public sealed class StructuredRecordingGraphicsTarget : RecordingGraphicsTarget, IStructuredGraphicsTarget
    {
        // The objects that stand for the caller's keys, by those keys.
        private readonly Dictionary<object, object> standIns = new Dictionary<object, object>(new IdentityComparer<object>());

        // End recording, and let go of the caller's keys.
        public override void EndRecording()
        {
            base.EndRecording();
            standIns.Clear();
        }

        // Record beginning a part.
        public void BeginPart(object partKey)
        {
            object part = StandInFor(partKey);
            RecordStructure(target => target.BeginPart(part));
        }

        // Record ending a part.
        public void EndPart()
        {
            RecordStructure(target => target.EndPart());
        }

        // Record beginning a labeled map.
        public void BeginMap(object mapKey, string label)
        {
            object map = StandInFor(mapKey);
            RecordStructure(target => target.BeginMap(map, label));
        }

        // Record ending a map.
        public void EndMap()
        {
            RecordStructure(target => target.EndMap());
        }

        // Record beginning a color run; the color is copied.
        public void BeginColorRun(object colorKey, int position, string name, CmykColor color, bool overprint)
        {
            object colorStandIn = StandInFor(colorKey);
            CmykColor colorCopy = color.Clone();
            RecordStructure(target => target.BeginColorRun(colorStandIn, position, name, colorCopy, overprint));
        }

        // Record ending a color run.
        public void EndColorRun()
        {
            RecordStructure(target => target.EndColorRun());
        }

        // Record beginning an object.
        public void BeginObject(object objectKey, object symbolKey, string symbolId, string symbolName)
        {
            object objectStandIn = StandInFor(objectKey), symbol = StandInFor(symbolKey);
            RecordStructure(target => target.BeginObject(objectStandIn, symbol, symbolId, symbolName));
        }

        // Record ending an object.
        public void EndObject()
        {
            RecordStructure(target => target.EndObject());
        }

        // Record adding a page; the transform and the list of parts are copied.
        public void AddPage(SizeF paperSize, RectangleF mapArea, Matrix mapToPage, object[] partKeys)
        {
            Matrix transform = mapToPage.Clone();
            object[] parts = new object[partKeys.Length];
            for (int index = 0; index < partKeys.Length; ++index)
                parts[index] = StandInFor(partKeys[index]);
            RecordStructure(target => target.AddPage(paperSize, mapArea, transform, parts));
        }

        // Record where a map lies in the real world; the georeference is a value that cannot change, so it is kept.
        public void SetGeoreference(object mapKey, MapGeoreference georeference)
        {
            object map = StandInFor(mapKey);
            RecordStructure(target => target.SetGeoreference(map, georeference));
        }

        // The object that stands for a key of the caller's, made when the key is first seen.
        // key: the caller's key.
        private object StandInFor(object key)
        {
            object standIn;
            if (!standIns.TryGetValue(key, out standIn)) {
                standIn = new object();
                standIns.Add(key, standIn);
            }
            return standIn;
        }
    }
}
