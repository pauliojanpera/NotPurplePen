using PurplePen.Graphics2D;

namespace PurplePen.MapModel
{
    // What a brush paints with: a color, at the intensity it was created at (D-11), or a pattern, whose tile the brush's
    // own target drew (D-7). A pen made from the brush strokes with the same paint (D-6).
    public sealed class SvgBrush
    {
        // The color the brush paints with, or null for a pattern brush.
        private readonly CmykColor color;

        // The pattern the brush paints with, or null for a color brush.
        private readonly SvgPattern pattern;

        // Create a brush of a color.
        // color: the color, at the intensity it is drawn at.
        public SvgBrush(CmykColor color)
        {
            this.color = color;
        }

        // Create a brush of a pattern.
        // pattern: the pattern.
        public SvgBrush(SvgPattern pattern)
        {
            this.pattern = pattern;
        }

        // The style that paints a property with the brush, such as "fill:#f00" or "stroke:url(#)".
        // paints: the paints of the document.
        // property: "fill" or "stroke".
        public SvgStyle Paint(SvgPaints paints, string property)
        {
            if (pattern == null)
                return new SvgStyle(paints.Paint(property, color));
            return new SvgStyle(property + ":" + SvgStyle.PatternReference, pattern);
        }
    }
}
