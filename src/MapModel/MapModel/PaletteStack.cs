using System;
using System.Collections.Generic;

namespace PurplePen.MapModel
{
    using PurplePen.Graphics2D;

    // Pass 2, layer composition, and the encodings with which it writes the palette colors. Pass 2 takes the palette
    // separations of a map in palette order (Map.PaletteSeparations). For each palette separation that a drawing draws,
    // it makes the transparency group of its palette color (TransparencyGroup), and an encoding writes the group on the
    // surface of its channel (IPaletteStackEncoding). A graphics target that shares its surface with an encoding gives the
    // encoding to the drawing (IPaletteStackEncodingProvider). Pass 2 does not call the drawing code of the symbols.

    // The overprint mode: the choice of the course appearance that tells which blend mode B the drawing applies to the
    // palette colors, in all the channels. The opacity α of a palette color applies in each mode.
    public enum OverprintMode
    {
        None,               // No blend mode: each palette color composites with Normal.
        ByPaletteColor,     // The blend mode of each palette color (SymColor.BlendMode).
        Darken,             // Darken for the overprint colors (SymColor.OverPrint), and Normal for the other palette colors.
        Multiply,           // Multiply for the overprint colors, and Normal for the other palette colors.
    }

    // An encoding: it writes the palette colors of a drawing for one channel, one transparency group of a palette color at
    // a time, on the surface of its channel.
    public interface IPaletteStackEncoding
    {
        // Write the transparency group of a palette color on the surface of the channel, over what is drawn there.
        // group: the transparency group.
        void WriteTransparencyGroup(TransparencyGroup group);
    }

    // A graphics target that shares its surface with an encoding, and gives the encoding to the drawing. The drawing of a
    // map writes its palette colors with the encoding (Map.Draw), and draws the templates, the image layer and the layout
    // layer on the graphics target.
    public interface IPaletteStackEncodingProvider
    {
        // The encoding that writes on the surface of the graphics target, or null when the target has none.
        IPaletteStackEncoding PaletteStackEncoding { get; }
    }

    // The transparency group of a palette color in a drawing, which pass 2 makes and an encoding writes. Its content is the
    // color C of the palette color on the coverage of the palette color coverages of the symbols that the rectangle of the
    // drawing can show, in the order of the palette separation. The group is composited once onto the backdrop with the
    // opacity α of the palette color and the blend mode B that the overprint mode gives, so the overlaps of the palette
    // color do not get α twice. The encoding writes the clipping objects of the palette color coverages at the resolution
    // of the drawing (SymbolCoverage.GetClippingObjects(float)).
    public sealed class TransparencyGroup
    {
        private readonly SymColor color;
        private readonly float opacity;
        private readonly ColorBlendMode blendMode;
        private readonly float resolution;
        private readonly bool usePatternBrushes;
        private readonly IEnumerable<SymbolCoverage> symbolCoverages;
        private readonly PaletteSeparation separation;

        // Make the transparency group of a palette color.
        // color: the palette color.
        // opacity: the opacity α of the palette color.
        // blendMode: the blend mode B that the overprint mode gives to the palette color.
        // resolution: the resolution of the drawing: the size of a pixel, in paper coordinates.
        // usePatternBrushes: whether the encoding fills a regular clipped area pattern with a pattern brush where the
        //   brush is not too coarse (RenderOptions.usePatternBitmaps).
        // symbolCoverages: the palette color coverages of the symbols in the group, in the order of the palette separation.
        //   Pass 2 gives them as the encoding reads them, and calls the cancellation of the drawing between them.
        // separation: the palette separation of the palette color, whose palette cutters give the area clusters of the
        //   palette color coverages.
        internal TransparencyGroup(SymColor color, float opacity, ColorBlendMode blendMode, float resolution, bool usePatternBrushes,
                                   IEnumerable<SymbolCoverage> symbolCoverages, PaletteSeparation separation)
        {
            if (color == null)
                throw new ArgumentNullException(nameof(color));
            if (symbolCoverages == null)
                throw new ArgumentNullException(nameof(symbolCoverages));
            if (separation == null)
                throw new ArgumentNullException(nameof(separation));

            this.color = color;
            this.opacity = opacity;
            this.blendMode = blendMode;
            this.resolution = resolution;
            this.usePatternBrushes = usePatternBrushes;
            this.symbolCoverages = symbolCoverages;
            this.separation = separation;
        }

        // The palette color. Its color C is SymColor.ColorValue, with the color matrix of the map, which an encoding
        // converts as its graphics target converts the colors of the drawing.
        public SymColor Color
        {
            get { return color; }
        }

        // The opacity α of the palette color (SymColor.Opacity), from 0 to 1.
        public float Opacity
        {
            get { return opacity; }
        }

        // The blend mode B, which the overprint mode of the drawing gives to the palette color (RenderOptions.overprintMode).
        public ColorBlendMode BlendMode
        {
            get { return blendMode; }
        }

        // The resolution of the drawing: the size of a pixel, in paper coordinates (RenderOptions.minResolution).
        public float Resolution
        {
            get { return resolution; }
        }

        // Whether the encoding fills a regular clipped area pattern with a pattern brush of its pattern tile where the brush
        // is not too coarse at the resolution, as the old drawing does (RenderOptions.usePatternBitmaps).
        public bool UsePatternBrushes
        {
            get { return usePatternBrushes; }
        }

        // The palette color coverages of the symbols that the rectangle of the drawing can show, in the order of the palette
        // separation. Pass 2 gives them as the encoding reads them, so an encoding reads them once.
        public IEnumerable<SymbolCoverage> SymbolCoverages
        {
            get { return symbolCoverages; }
        }

        // The area cluster of a palette color coverage of a hatching or a clipped area pattern (PaletteCutter.AreaClusters),
        // or null for a palette color coverage of the full coverage. An encoding writes the pattern of a cluster of several
        // areas once, in the union of its areas (AreaCluster.Union), when it reads the first of its palette color coverages
        // that the group gives, and the cluster gives all its palette color coverages, also those that the rectangle of the
        // drawing does not show.
        // coverage: a palette color coverage of the group.
        public AreaCluster GetAreaCluster(SymbolCoverage coverage)
        {
            return separation.GetAreaCluster(coverage);
        }

        // The blend mode B of a palette color in an overprint mode: Normal in the mode None, the blend mode of the palette
        // color in the mode ByPaletteColor, and Darken or Multiply for an overprint color in the modes Darken and Multiply.
        // color: the palette color.
        // overprintMode: the overprint mode.
        internal static ColorBlendMode GetBlendMode(SymColor color, OverprintMode overprintMode)
        {
            switch (overprintMode) {
                case OverprintMode.ByPaletteColor:
                    return color.BlendMode;
                case OverprintMode.Darken:
                    return color.OverPrint ? ColorBlendMode.Darken : ColorBlendMode.Normal;
                case OverprintMode.Multiply:
                    return color.OverPrint ? ColorBlendMode.Multiply : ColorBlendMode.Normal;
                default:
                    return ColorBlendMode.Normal;
            }
        }

        public override string ToString()
        {
            return string.Format("{0}, opacity {1}, {2}", color.Name, opacity, blendMode);
        }
    }
}
