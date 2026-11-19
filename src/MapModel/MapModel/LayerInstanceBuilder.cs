using System;
using System.Collections.Generic;

namespace PurplePen.MapModel
{
    using PurplePen.Graphics2D;

    // The layer instances that pass 1 of the drawing makes (Map.CreateLayerInstances), while the geometry methods of
    // the symbols (Symbol.AddClippingObjects) add their clipping objects, and that the collector (LayerInstanceCollector)
    // makes as the oracle of the geometry methods (Map.CollectLayerInstances).
    // Each clipping object goes to the instance of its palette entry, so a symbol makes all its palette entries in one
    // pass. Each palette entry that gets a clipping object has one instance with full coverage (G = 1), whose clipping
    // objects are in the order that they were added.
    internal sealed class LayerInstanceBuilder
    {
        private readonly List<SymColor> palette;
        private readonly Dictionary<SymColor, List<ClippingObject>> clippingObjectsOfPaletteEntries = new Dictionary<SymColor, List<ClippingObject>>();

        // Make the builder of the instances of a palette, which have no clipping objects yet.
        // palette: the palette entries, the bottom one first.
        public LayerInstanceBuilder(IEnumerable<SymColor> palette)
        {
            this.palette = new List<SymColor>(palette);
            foreach (SymColor paletteEntry in this.palette)
                clippingObjectsOfPaletteEntries.Add(paletteEntry, new List<ClippingObject>());
        }

        // Add a clipping object to the instance of a palette entry. A color that is no entry of the palette, such as the
        // color of the image layer, has no instance, as the drawing does not draw it among the palette entries.
        // paletteEntry: the palette entry.
        // clippingObject: the clipping object.
        public void Add(SymColor paletteEntry, ClippingObject clippingObject)
        {
            if (clippingObject == null)
                throw new ArgumentNullException(nameof(clippingObject));

            List<ClippingObject> clippingObjects;
            if (clippingObjectsOfPaletteEntries.TryGetValue(paletteEntry, out clippingObjects))
                clippingObjects.Add(clippingObject);
        }

        // Make the instances of the palette entries that have clipping objects, in the order of the palette.
        public IReadOnlyList<LayerInstance> MakeInstances()
        {
            List<LayerInstance> instances = new List<LayerInstance>();
            foreach (SymColor paletteEntry in palette) {
                List<ClippingObject> clippingObjects = clippingObjectsOfPaletteEntries[paletteEntry];
                if (clippingObjects.Count > 0)
                    instances.Add(new LayerInstance(paletteEntry, GeometryPrototype.FullCoverage, new Matrix(), clippingObjects));
            }

            return instances.AsReadOnly();
        }
    }
}
