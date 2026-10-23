using System.Drawing;
using System.Text;
using PurplePen.Graphics2D;

namespace PurplePen.MapModel
{
    // Writes the declarations that paint a fill or a stroke with a color (COL-1 to COL-6). The sRGB value comes from the
    // color converter of the chosen color model. Under the CMYK color model the color is declared a second time, as
    // the same sRGB followed by icc-color() with the map's CMYK values: a browser, which rejects that declaration,
    // keeps the first, and a print tool reads the second.
    public sealed class SvgPaints
    {
        // The name of the device CMYK profile that icc-color() refers to. The document declares it once and embeds or
        // references no ICC profile, since the values are the map's own uncalibrated CMYK (COL-3).
        public const string CmykProfileName = "cmyk";

        // Converts colors to sRGB.
        private readonly IColorConverter colorConverter;

        // Whether colors carry their CMYK values, as under the CMYK color model.
        private readonly bool declaresCmykColors;

        // Whether a declaration has referred to the CMYK profile, which the document then declares.
        public bool RefersToCmykProfile { get; private set; }

        // Create the paints of a document.
        // colorConverter: converts colors to sRGB.
        // declaresCmykColors: whether colors carry their CMYK values.
        public SvgPaints(IColorConverter colorConverter, bool declaresCmykColors)
        {
            this.colorConverter = colorConverter;
            this.declaresCmykColors = declaresCmykColors;
        }

        // The declarations that paint a property with a color, such as "fill:#f00;fill:#f00 icc-color(cmyk,0,1,1,0)",
        // followed by its opacity when the color is not opaque (COL-5).
        // property: "fill" or "stroke".
        // color: the color.
        public string Paint(string property, CmykColor color)
        {
            string rgb = FormatRgb(colorConverter.ToColor(color));
            StringBuilder declarations = new StringBuilder();
            declarations.Append(property).Append(':').Append(rgb);
            if (declaresCmykColors) {
                declarations.Append(';').Append(property).Append(':').Append(rgb).Append(" icc-color(").Append(CmykProfileName);
                foreach (float component in new float[] { color.Cyan, color.Magenta, color.Yellow, color.Black })
                    declarations.Append(',').Append(SvgNumbers.FormatShortest(component));
                declarations.Append(')');
                RefersToCmykProfile = true;
            }
            if (color.Alpha < 1)
                declarations.Append(';').Append(property).Append("-opacity:").Append(SvgNumbers.FormatShortest(color.Alpha));
            return declarations.ToString();
        }

        // Write an sRGB color in hex, with three digits when that is exact (COL-6).
        // color: the color; its alpha is written apart.
        public static string FormatRgb(Color color)
        {
            const string hexDigits = "0123456789abcdef";
            byte[] channels = { color.R, color.G, color.B };
            bool everyChannelRepeatsItsDigit = true;
            foreach (byte channel in channels) {
                if (channel >> 4 != (channel & 15))
                    everyChannelRepeatsItsDigit = false;
            }

            StringBuilder text = new StringBuilder("#");
            foreach (byte channel in channels) {
                text.Append(hexDigits[channel >> 4]);
                if (!everyChannelRepeatsItsDigit)
                    text.Append(hexDigits[channel & 15]);
            }
            return text.ToString();
        }
    }
}
