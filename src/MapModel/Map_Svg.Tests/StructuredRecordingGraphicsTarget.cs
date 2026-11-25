using System;
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
    // the keys of parts, maps and symbol definitions are replaced by objects of the recorder's own, which stand for them
    // with the same identity, so no key of the caller's is held once recording ends. The palette colors of a map are a
    // deferred drawing, which the encoding of the target that plays the recording back writes (IDeferredDrawingTarget).
    public sealed class StructuredRecordingGraphicsTarget : RecordingGraphicsTarget, IStructuredGraphicsTarget
    {
        // The objects that stand for the caller's keys, by those keys.
        private readonly Dictionary<object, object> standIns = new Dictionary<object, object>(new IdentityComparer<object>());

        // Create a recorder, which starts in recording mode.
        // bitmapLoader: loads the recorded bitmaps when they are played back, as bitmaps the playback target can draw.
        public StructuredRecordingGraphicsTarget(IGraphicsBitmapLoader bitmapLoader)
            : base(bitmapLoader)
        {
        }

        // What the deferred drawings of the recording read at its playback, such as the map whose palette colors they
        // write, which the recorder disposes with itself.
        private readonly List<IDisposable> readAtPlayback = new List<IDisposable>();

        // End recording, and let go of the caller's keys.
        public override void EndRecording()
        {
            base.EndRecording();
            standIns.Clear();
        }

        // Keep what the deferred drawings of the recording read at its playback until the recorder is disposed.
        // read: what they read, such as the map whose palette colors they write.
        public void KeepUntilDisposed(IDisposable read)
        {
            readAtPlayback.Add(read);
        }

        // Dispose the recording, and what its deferred drawings read.
        public override void Dispose()
        {
            base.Dispose();
            foreach (IDisposable read in readAtPlayback)
                read.Dispose();
            readAtPlayback.Clear();
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

        // Record beginning an object, with the text form of its key.
        public void BeginObject(string objectKey, object symbolKey, string symbolId, string symbolName)
        {
            object symbol = StandInFor(symbolKey);
            RecordStructure(target => target.BeginObject(objectKey, symbol, symbolId, symbolName));
        }

        // Record ending an object.
        public void EndObject()
        {
            RecordStructure(target => target.EndObject());
        }

        // Record adding a page; the transform and the list of parts are copied.
        public void AddPage(SizeF paperSize, RectangleF mapArea, Matrix mapToPage, object[] partKeys, string name)
        {
            Matrix transform = mapToPage.Clone();
            object[] parts = new object[partKeys.Length];
            for (int index = 0; index < partKeys.Length; ++index)
                parts[index] = StandInFor(partKeys[index]);
            RecordStructure(target => target.AddPage(paperSize, mapArea, transform, parts, name));
        }

        // Record where a map lies in the real world; the georeference is a value that cannot change, so it is kept.
        public void SetGeoreference(object mapKey, MapGeoreference georeference)
        {
            object map = StandInFor(mapKey);
            RecordStructure(target => target.SetGeoreference(map, georeference));
        }

        // Record describing the course a map draws.
        public void DescribeCourse(object mapKey, CourseRecord course)
        {
            object map = StandInFor(mapKey);
            RecordStructure(target => target.DescribeCourse(map, course));
        }

        // Record describing the course objects of a map, by the text forms of their keys; the lookup is copied.
        public void DescribeObjects(object mapKey, IReadOnlyDictionary<string, CourseObjectRecord> objects)
        {
            object map = StandInFor(mapKey);
            Dictionary<string, CourseObjectRecord> copy = new Dictionary<string, CourseObjectRecord>(StringComparer.Ordinal);
            foreach (KeyValuePair<string, CourseObjectRecord> pair in objects)
                copy[pair.Key] = pair.Value;
            RecordStructure(target => target.DescribeObjects(map, copy));
        }

        // Pattern brushes are vectors, as the SVG target's are, so that the map code draws into the recorder as it draws
        // into the SVG target: its hatchings and regular area patterns with pattern brushes, however fine the tile (S-7).
        public bool PatternBrushesAreVectors
        {
            get { return true; }
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
