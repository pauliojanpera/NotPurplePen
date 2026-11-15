using System.Drawing;
using PurplePen.Graphics2D;
using PurplePen.MapModel;

namespace RenderingTestCases
{
    // Draws maps as the map rendering tests draw them.
    public static class MapDrawing
    {
        // Draw the area of a map that an image shows into a graphics target, with the map's templates, leaving out
        // detail smaller than the image's pixels.
        // target: the target to draw into.
        // map: the map to draw.
        // mapArea: the area of the map that the image shows.
        // pixelSize: the size of the image in pixels.
        // renderOptions: the options to draw with, apart from those this sets; left unchanged.
        // usePatternBitmaps: whether area patterns are drawn with pattern brushes rather than element by element.
        // blendOverprintedColors: whether colors that overprint are blended with darken.
        // antiAlias: whether edges are anti-aliased.
        public static void DrawMapArea(IGraphicsTarget target, Map map, RectangleF mapArea, Size pixelSize, RenderOptions renderOptions,
                                       bool usePatternBitmaps, bool blendOverprintedColors, bool antiAlias)
        {
            RenderOptions options = renderOptions.Clone();
            options.usePatternBitmaps = usePatternBitmaps;
            options.renderTemplates = RenderTemplateOption.MapAndTemplates;
            options.blendOverprintedColors = blendOverprintedColors;
            options.minResolution = mapArea.Width / (float)pixelSize.Width;

            target.PushAntiAliasing(antiAlias);
            using (map.Read())
                map.Draw(target, mapArea, options, null);
            target.PopAntiAliasing();
        }
    }
}
