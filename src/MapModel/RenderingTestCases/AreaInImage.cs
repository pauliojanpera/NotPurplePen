using System.Drawing;
using PurplePen.Graphics2D;

namespace RenderingTestCases
{
    // Places an area of drawing or map coordinates in an image, as the Skia bitmap target places the area it is
    // given: the center of the area at the center of the image, and the width of the area across the image.
    public static class AreaInImage
    {
        // Create the transform from coordinates in an area to the pixels of the image that shows it.
        // area: the area that the image shows.
        // pixelSize: the size of the image in pixels.
        // yAxisUp: true for map coordinates, whose y axis points up; false for drawing coordinates, whose y axis
        //          points down as the image's does.
        public static Matrix CreateTransform(RectangleF area, Size pixelSize, bool yAxisUp)
        {
            PointF midpoint = new PointF(pixelSize.Width / 2.0F, pixelSize.Height / 2.0F);
            float scaleFactor = (float)pixelSize.Width / area.Width;
            PointF centerPoint = new PointF((area.Left + area.Right) / 2, (area.Top + area.Bottom) / 2);
            Matrix matrix = new Matrix();
            matrix.Translate(midpoint.X, midpoint.Y);
            matrix.Scale(scaleFactor, yAxisUp ? -scaleFactor : scaleFactor);
            matrix.Translate(-centerPoint.X, -centerPoint.Y);
            return matrix;
        }
    }
}
