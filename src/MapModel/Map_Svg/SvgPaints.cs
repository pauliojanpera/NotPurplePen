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
        // The name of the device CMYK profile that icc-color() refers to, which is also the id of its declaration: the
        // document declares it once, with an ICC profile behind it, since Inkscape shows a color's CMYK values only
        // through a profile that has an id and a CMYK ICC profile (COL-3, F-6).
        public const string CmykProfileName = "cmyk";

        // The ICC profile behind the name, as a data URI: 516 bytes of version 2.1, a CMYK output profile whose one
        // table, A2B0, takes CMYK to CIELAB as R = (1 - C)(1 - K), G = (1 - M)(1 - K) and B = (1 - Y)(1 - K) do, on a
        // grid of two points a channel, which holds that exactly. The values stay the map's own uncalibrated CMYK; the
        // profile serves the CMYK views and conversions of the tools that read it. The bytes are those of
        // doc/devdocs/SvgColourReaders/device-cmyk.icc, which make_profile.py there writes with "naive 2".
        public const string CmykProfileHref =
            "data:application/vnd.iccprofile;base64," +
            "AAACBAAAAAACEAAAcHJ0ckNNWUtMYWIgB+oACwAEAAAAAAAAYWNzcAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAPbWAAEAAAAA0y0AAAAAAAAAAAAAAAAAAAAA" +
            "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAEZGVzYwAAALQAAABmY3BydAAAARwAAAAhd3RwdAAAAUAAAAAUQTJCMAAAAVQAAACwZGVzYwAAAAAAAAAM" +
            "RGV2aWNlIENNWUsAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAB0ZXh0" +
            "AAAAAE5vIGNvcHlyaWdodCwgdXNlIGZyZWVseQAAAABYWVogAAAAAAAA9tYAAQAAAADTLW1mdDIAAAAABAMCAAABAAAAAAAAAAAAAAAAAAAAAQAAAAAAAAAAAAAAAAAA" +
            "AAEAAAACAAIAAP//AAD//wAA//8AAP///wCAAX/6AACAAIAA+OZwQN1gAACAAIAAmW/djkN6AACAAIAAinLQ0cXhAACAAIAA5zJNVnEDAACAAIAA3/Awt9D6AACAAIAA" +
            "S2bETQ/yAACAAIAAAACAAIAAAACAAIAAAAD//wAA//8AAP//";

        // Converts colors to sRGB.
        private readonly IColorConverter colorConverter;

        // Whether colors carry their CMYK values, as under the CMYK color model.
        private readonly bool declaresCmykColors;

        // Whether a declaration has referred to the CMYK profile, which the document then declares.
        public bool RefersToCmykProfile { get; private set; }

        // Whether colors carry their CMYK values, as under the CMYK color model; the profile's id is then the document's
        // to give, and no other id takes it (L-5).
        public bool DeclaresCmykColors
        {
            get { return declaresCmykColors; }
        }

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
