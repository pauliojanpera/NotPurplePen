using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Map_SkiaStd;
using PurplePen.Graphics2D;
using PurplePen.MapModel;
using SkiaSharp;

namespace Map_Svg.Tests
{
    // A text as Chromium draws it, as ChromiumRenderer.ReadTextPositionsAsync reads it: its characters, the face its
    // style names, and where each character starts along the baseline, in the text's own coordinates.
    public class DrawnText
    {
        // The characters of the text, its runs in fallback faces included.
        public string Text { get; set; }

        // The font-family of the text's style: the family the map names, quoted when it holds a space.
        public string Family { get; set; }

        // The font size, in units of the text's coordinates.
        public float Size { get; set; }

        // The font weight, 400 or 700.
        public int Weight { get; set; }

        // Whether the style is italic.
        public bool Italic { get; set; }

        // How many pixels of the image a unit of the text's coordinates takes.
        public double Scale { get; set; }

        // Where each UTF-16 unit of the text starts along the baseline, in units of the text's coordinates.
        public double[] Starts { get; set; }
    }

    // How far Chromium puts the characters of a drawing's texts from where Skia's shaping puts their glyphs, which is
    // where the Skia target draws them and the PDF export writes them.
    public class TextPositionMeasurement
    {
        // How many texts were measured.
        public int Texts;

        // How many texts were left out: those not in Unicode normalization form C, which shaping normalizes first,
        // so that their characters and glyphs are not counted alike.
        public int TextsLeftOut;

        // How many clusters of characters were measured: one for each place the shaping starts a cluster.
        public int Clusters;

        // The largest offset of a cluster from its glyph's shaped position, in pixels of the image.
        public double LargestOffset;

        // Where the largest offset lies, or null when there was nothing to measure.
        public string Where;

        // Each cluster measured, as rows of a table: the text's number, the cluster's first character's index and
        // its characters, where the shaping and Chromium put it, and the pixels to a unit.
        private readonly StringBuilder clusterRows = new StringBuilder();

        // Measure a drawing's texts.
        // texts: the texts as Chromium draws them.
        // metrics: the text metrics of the Skia target, whose faces shape the texts.
        public static TextPositionMeasurement Measure(IEnumerable<DrawnText> texts, ITextMetrics metrics)
        {
            TextPositionMeasurement measurement = new TextPositionMeasurement();
            foreach (DrawnText text in texts) {
                if (string.IsNullOrEmpty(text.Text))
                    continue;
                if (!text.Text.IsNormalized(NormalizationForm.FormC)) {
                    ++measurement.TextsLeftOut;
                    continue;
                }
                ++measurement.Texts;
                TextEffects effects = (text.Weight >= 600 ? TextEffects.Bold : TextEffects.Regular) |
                                      (text.Italic ? TextEffects.Italic : TextEffects.Regular);
                using (ITextFaceMetrics face = metrics.GetTextFaceMetrics(FirstFamily(text.Family), text.Size, effects)) {
                    GlyphPosition[] glyphs = ((SkiaFont)face).EnhancedTypeface.GetGlyphPositions(text.Text, new SKPoint(0, 0), face.EmHeight);
                    HashSet<int> clusterStarts = new HashSet<int>();
                    foreach (GlyphPosition glyph in glyphs) {
                        // The first glyph of a cluster stands where the cluster does.
                        if (!clusterStarts.Add(glyph.ClusterStart) || glyph.ClusterStart >= text.Starts.Length)
                            continue;
                        ++measurement.Clusters;
                        double offset = Math.Abs(text.Starts[glyph.ClusterStart] - glyph.Position.X) * text.Scale;
                        measurement.clusterRows.AppendFormat(CultureInfo.InvariantCulture, "{0}\t{1}\t{2}\t{3:0.#####}\t{4:0.#####}\t{5:0.####}\n",
                            measurement.Texts, glyph.ClusterStart, glyph.GlyphText.Replace('\t', ' ').Replace('\n', ' '), glyph.Position.X, text.Starts[glyph.ClusterStart], text.Scale);
                        if (offset > measurement.LargestOffset || measurement.Where == null) {
                            measurement.LargestOffset = Math.Max(offset, measurement.LargestOffset);
                            measurement.Where = string.Format(CultureInfo.InvariantCulture,
                                "\"{0}\" at character {1}, \"{2}\": shaped at {3:0.#####}, drawn at {4:0.#####}, {5:0.###} pixels to a unit",
                                Shorten(text.Text), glyph.ClusterStart, glyph.GlyphText, glyph.Position.X, text.Starts[glyph.ClusterStart], text.Scale);
                        }
                    }
                }
            }
            return measurement;
        }

        // The measurement as the rows of the loop's results: a header and one row of figures.
        public string ToTsv()
        {
            return "texts\ttexts left out\tclusters\tlargest offset\twhere\n" +
                   string.Format(CultureInfo.InvariantCulture, "{0}\t{1}\t{2}\t{3:0.####}\t{4}\n",
                                 Texts, TextsLeftOut, Clusters, LargestOffset, (Where ?? "").Replace('\t', ' ').Replace('\n', ' '));
        }

        // Each cluster measured, as a table with a header.
        public string ClustersToTsv()
        {
            return "text\tindex\tcharacters\tshaped\tdrawn\tpixels to a unit\n" + clusterRows;
        }

        // The first family of a CSS font-family list, without its quotes.
        // family: the font-family, as a computed style gives it.
        private static string FirstFamily(string family)
        {
            string first = family.Split(',')[0].Trim();
            return first.Length >= 2 && (first[0] == '"' || first[0] == '\'') ? first.Substring(1, first.Length - 2) : first;
        }

        // A text cut to its first 40 characters, for a description.
        // text: the text.
        private static string Shorten(string text)
        {
            return text.Length <= 40 ? text : text.Substring(0, 40) + "…";
        }
    }
}
