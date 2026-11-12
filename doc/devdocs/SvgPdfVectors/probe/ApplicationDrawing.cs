using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using Map_SkiaStd;
using PurplePen;
using PurplePen.Graphics2D;
using PurplePen.MapModel;
using PurplePen.PdfConverter;
using SkiaSharp;

namespace PdfVectorsProbe
{
    // A PDF map as the application holds it: MapDisplay with the map set as a PDF, which PdfMapFile converts through
    // PdfConverter's PDFium into a PNG of 600 dots per inch and draws as a bitmap (MapDisplay.DrawBitmapMap). Each
    // drawing here goes through MapDisplay.Draw, set up as CourseSvg sets it up: full intensity, printing, anti-aliased,
    // in the CMYK color model.
    internal sealed class ApplicationDrawing : IDisposable
    {
        // The step of the SVG's coordinates, in millimetres of map, which CourseSvg draws its parts at (S-1, A-4).
        private const float MinResolution = 0.001F;

        // The map display that holds the PDF map.
        public readonly MapDisplay MapDisplay;

        // The PDF file.
        public readonly string PdfPath;

        // Load a PDF map as the application loads it.
        // pdfPath: the PDF file.
        public ApplicationDrawing(string pdfPath)
        {
            ProbeServices.Register();
            PdfPath = Path.GetFullPath(pdfPath);
            MapDisplay = new MapDisplay();
            MapDisplay.SetMapFile(MapType.PDF, PdfPath);
            if (MapDisplay.MapType != MapType.PDF)
                throw new InvalidOperationException("The PDF does not load as a map: " + PdfPath);
            MapDisplay.MapIntensity = 1.0F;
            MapDisplay.AntiAlias = true;
            MapDisplay.Printing = true;
            MapDisplay.AntiAliasWhenPrinting = true;
            MapDisplay.ColorModel = ColorModel.CMYK;
        }

        // The map's bounds in millimetres of map, as MapDisplay gives them: the bitmap's pixels at 600 dots per inch,
        // with the origin at the page's lower left corner and y upward.
        public RectangleF MapBounds
        {
            get { return MapDisplay.MapBounds; }
        }

        // Time PdfConverter's conversion of the page at the application's resolution, into a file of its own, which is
        // deleted: what loading the map costs when the converted PNG is not cached.
        // Returns the seconds taken and the size of the PNG in bytes.
        public static Tuple<double, long> TimeConversion(string pdfPath)
        {
            string png = Path.Combine(Path.GetTempPath(), "pdf-vectors-probe-" + Environment.ProcessId.ToString(System.Globalization.CultureInfo.InvariantCulture) + ".png");
            Stopwatch watch = Stopwatch.StartNew();
            PdfConversion.Convert(600, pdfPath, png);
            watch.Stop();
            long size = new FileInfo(png).Length;
            File.Delete(png);
            return Tuple.Create(watch.Elapsed.TotalSeconds, size);
        }

        // The transform from millimetres of map to the pixels of an image of the whole page at a resolution, y downward.
        // pixelsPerInch: the image's resolution.
        public Matrix MapToImage(float pixelsPerInch)
        {
            RectangleF bounds = MapBounds;
            float scale = pixelsPerInch / 25.4F;
            return new Matrix(scale, 0, 0, -scale, -bounds.Left * scale, bounds.Bottom * scale);
        }

        // The size in pixels of an image of the whole page at a resolution.
        // pixelsPerInch: the image's resolution.
        public Size ImageSize(float pixelsPerInch)
        {
            RectangleF bounds = MapBounds;
            return new Size((int)Math.Round(bounds.Width / 25.4F * pixelsPerInch), (int)Math.Round(bounds.Height / 25.4F * pixelsPerInch));
        }

        // Draw the page as the application draws it with the Skia target, on white: the reference that F-1 holds the
        // browsers' drawings of the SVG to. The map is drawn as its part of an exported page is, by
        // MapDisplay.Draw(MapBelowCourse), anti-aliased.
        // pixelsPerInch: the image's resolution.
        public SKBitmap RenderReference(float pixelsPerInch)
        {
            Size size = ImageSize(pixelsPerInch);
            SKBitmap reference = new SKBitmap(size.Width, size.Height);
            using (SKCanvas canvas = new SKCanvas(reference)) {
                canvas.Clear(SKColors.White);
                using (Skia_GraphicsTarget target = new Skia_GraphicsTarget(canvas, false, SwopColorConverter.Instance))
                using (Matrix mapToImage = MapToImage(pixelsPerInch)) {
                    target.PushTransform(mapToImage);
                    target.PushClip(MapBounds);
                    target.PushAntiAliasing(true);
                    MapDisplay.Draw(target, MapDisplayLayer.MapBelowCourse, MapBounds, MinResolution, null);
                    target.PopAntiAliasing();
                    target.PopClip();
                    target.PopTransform();
                }
            }
            return reference;
        }

        // Write the page as the SVG export writes a PDF map today: the map's part is MapDisplay's drawing of it, the
        // bitmap, under the map's label (M-5), and one page shows the whole map. Returns the document's text.
        public string WriteBitmapSvg()
        {
            using (Svg_GraphicsTarget target = new Svg_GraphicsTarget(SwopColorConverter.Instance, true, SvgBitmapOptions.Default,
                                                                      Services.TextMetricsProvider, Services.FontSubsetProvider)) {
                object part = new object(), map = new object();
                target.BeginPart(part);
                target.BeginMap(map, Path.GetFileName(PdfPath));
                MapDisplay.Draw(target, MapDisplayLayer.MapBelowCourse, MapBounds, MinResolution, null);
                target.EndMap();
                target.EndPart();
                AddWholePage(target, part);
                return target.FinishDocument();
            }
        }

        // Add the page that shows the whole map: its paper is the map's bounds, the map's lower left corner at the
        // paper's, y downward.
        // target: the SVG target.
        // part: the part the page shows.
        public void AddWholePage(Svg_GraphicsTarget target, object part)
        {
            RectangleF bounds = MapBounds;
            using (Matrix mapToPage = new Matrix(1, 0, 0, -1, -bounds.Left, bounds.Bottom))
                target.AddPage(bounds.Size, bounds, mapToPage, new object[] { part }, "page");
        }

        // Release the map display.
        public void Dispose()
        {
            MapDisplay.Dispose();
        }
    }
}
