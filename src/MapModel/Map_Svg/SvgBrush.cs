using PurplePen.Graphics2D;

namespace PurplePen.MapModel
{
    // What a brush paints with: a color, at the intensity it was created at (D-11). A pen made from the brush strokes
    // with the same paint (D-6).
    public sealed class SvgBrush
    {
        // The color the brush paints with.
        private readonly CmykColor color;

        // Create a brush of a color.
        // color: the color, at the intensity it is drawn at.
        public SvgBrush(CmykColor color)
        {
            this.color = color;
        }

        // The declarations that paint a property with the brush, such as "fill:#f00" or "stroke:#f00".
        // paints: the paints of the document.
        // property: "fill" or "stroke".
        public string Paint(SvgPaints paints, string property)
        {
            return paints.Paint(property, color);
        }
    }
}
