using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using PurplePen;
using PurplePen.MapModel;
using SkiaSharp;

using DefaultColorConverter = PurplePen.Graphics2D.DefaultColorConverter;
using Geometry = PurplePen.Graphics2D.Geometry;
using IColorConverter = PurplePen.Graphics2D.IColorConverter;
using Matrix = PurplePen.Graphics2D.Matrix;
using MatrixOrder = PurplePen.Graphics2D.MatrixOrder;

namespace Map_Svg.Tests
{
    // A page that the SVG export writes, with the file it writes the page in, and the page drawn as the application
    // draws it with the Skia target: the layers that CourseSvg writes as the parts of the page, the map below the
    // course, the course's lower layer, the map above it and the course's upper layer, each drawn by MapDisplay.Draw
    // over the page's map rectangle, with the map display set up as CourseSvg sets it up; under the page's transform
    // from map coordinates to millimetres of paper and its clip of the map rectangle, on white. Like the rendering
    // comparison's references, it is drawn anti-aliased, its clips too, as Chromium draws every clip path (V-9). The Skia
    // target's pattern brushes are bitmaps, so the map display draws area patterns element by element for it, as for
    // printing, where the export writes the regular ones as pattern brushes: the comparison checks where the brushes put
    // the patterns (S-13).
    internal sealed class ExportedPage
    {
        // The smallest distance worth drawing, in millimetres of map, which CourseSvg draws with.
        private const float MinResolution = 0.001F;

        // Millimetres in an inch, and in a hundredth of an inch, which page layouts are measured in.
        private const float MillimetresPerInch = 25.4F;
        private const float MillimetresPerHundredthOfInch = 0.254F;

        // The full path of the file the export writes the page in.
        public readonly string FileName;

        // The page, as the export lays it out.
        public readonly CoursePage Page;

        // Where the page lies among the pages of its file, from 0.
        public readonly int PlaceInFile;

        // Create a page of the export.
        // fileName: the full path of the file the export writes the page in.
        // page: the page.
        // placeInFile: where the page lies among the pages of its file, from 0.
        private ExportedPage(string fileName, CoursePage page, int placeInFile)
        {
            FileName = fileName;
            Page = page;
            PlaceInFile = placeInFile;
        }

        // Lay out the pages that the export writes of an event with the given settings, each with its file, as CourseSvg
        // lays them out: the files that PDF export would write with the settings' choice of which pages go in each, each
        // holding every page laid out for the courses or course parts that go in it, in the order they print (L-7).
        // loadedEvent: the event, loaded.
        // settings: the settings the event is exported with.
        public static List<ExportedPage> LayOut(CourseExport.LoadedEvent loadedEvent, CourseSvgSettings settings)
        {
            EventDB eventDB = loadedEvent.Controller.GetEventDB();
            List<Pair<string, IEnumerable<CourseDesignator>>> files = CourseOutputPages.GetFilesToCreate(
                eventDB, settings.FileCreation, settings.CourseIds, settings.VariationChoicesPerCourse,
                settings.PrintMapExchangesOnOneMap, settings.filePrefix, settings.outputDirectory, settings.Compress ? ".svgz" : ".svg");
            CoursePageLayout pageLayout = new CoursePageLayout(eventDB, loadedEvent.SymbolDB, loadedEvent.Controller,
                                                               loadedEvent.Controller.GetCourseAppearance(), settings.CropLargePrintArea);
            List<ExportedPage> pages = new List<ExportedPage>();
            foreach (Pair<string, IEnumerable<CourseDesignator>> file in files) {
                List<CoursePage> filePages = pageLayout.LayoutPages(file.Second);
                for (int index = 0; index < filePages.Count; ++index)
                    pages.Add(new ExportedPage(file.First, filePages[index], index));
            }
            return pages;
        }

        // The size of the page's paper in millimetres, turned for a landscape page.
        public SizeF PaperSize
        {
            get
            {
                double width = Math.Round(Page.paperSize.SizeInInches.Width * 100.0) * 254 / 1000;
                double height = Math.Round(Page.paperSize.SizeInInches.Height * 100.0) * 254 / 1000;
                return Page.landscape ? new SizeF((float)height, (float)width) : new SizeF((float)width, (float)height);
            }
        }

        // The size in pixels of an image of the whole page at a resolution.
        // pixelsPerInch: the resolution, in pixels to the inch.
        public Size PixelSizeAt(float pixelsPerInch)
        {
            SizeF paper = PaperSize;
            return new Size((int)Math.Round(paper.Width / MillimetresPerInch * pixelsPerInch), (int)Math.Round(paper.Height / MillimetresPerInch * pixelsPerInch));
        }

        // Draw the page as the application draws it with the Skia target, as an image of a size in pixels, with the
        // paper fitted into the image as Chromium fits a document's view box into an image of that size: scaled
        // alike in both directions, as far as the image allows, and centered.
        // loadedEvent: the event, loaded, as the export loaded it.
        // settings: the settings the event was exported with.
        // pixelSize: the size of the image.
        public SKBitmap RenderReference(CourseExport.LoadedEvent loadedEvent, CourseSvgSettings settings, Size pixelSize)
        {
            MapDisplay mapDisplay = CreateMapDisplay(loadedEvent, settings);
            IColorConverter colorConverter = settings.ColorModel == ColorModel.CMYK ? (IColorConverter)SwopColorConverter.Instance : (IColorConverter)DefaultColorConverter.Instance;

            SKBitmap reference = new SKBitmap(pixelSize.Width, pixelSize.Height);
            using (SKCanvas canvas = new SKCanvas(reference)) {
                canvas.Clear(SKColors.White);
                using (Skia_GraphicsTarget skiaTarget = new Skia_GraphicsTarget(canvas, false, colorConverter))
                using (AntiAliasedClipsTarget target = new AntiAliasedClipsTarget(skiaTarget))
                using (Matrix paperToImage = CreatePaperToImageTransform(pixelSize))
                using (Matrix mapToPaper = CreateMapToPaperTransform()) {
                    target.PushTransform(paperToImage);
                    target.PushTransform(mapToPaper);
                    target.PushClip(Page.mapRectangle);
                    target.PushAntiAliasing(true);
                    mapDisplay.Draw(target, MapDisplayLayer.MapBelowCourse, Page.mapRectangle, MinResolution, null);
                    mapDisplay.Draw(target, MapDisplayLayer.CourseLower, Page.mapRectangle, MinResolution, null);
                    if (mapDisplay.HasLayersAboveCourse) {
                        mapDisplay.Draw(target, MapDisplayLayer.MapAboveCourse, Page.mapRectangle, MinResolution, null);
                        mapDisplay.Draw(target, MapDisplayLayer.CourseUpper, Page.mapRectangle, MinResolution, null);
                    }
                    target.PopAntiAliasing();
                    target.PopClip();
                    target.PopTransform();
                    target.PopTransform();
                }
            }
            return reference;
        }

        // Create a map display that draws the page: a copy of the controller's at full intensity, set up as CourseSvg
        // sets up the map display it draws with, holding the page's course laid out as CourseSvg lays it out.
        // loadedEvent: the event, loaded, as the export loaded it.
        // settings: the settings the event was exported with.
        private MapDisplay CreateMapDisplay(CourseExport.LoadedEvent loadedEvent, CourseSvgSettings settings)
        {
            MapDisplay mapDisplay = loadedEvent.Controller.MapDisplay.CloneToFullIntensity();
            mapDisplay.MapIntensity = 1.0F;
            mapDisplay.AntiAlias = true;
            mapDisplay.Printing = true;
            mapDisplay.AntiAliasWhenPrinting = true;
            mapDisplay.AreaPatternsElementByElement = settings.DrawAreaPatternsAsSeparateShapes;
            mapDisplay.ColorModel = settings.ColorModel;
            mapDisplay.RecordsCourseObjects = true;
            if (settings.DontPrintBaseMap)
                mapDisplay.SetMapFile(MapType.None, null);

            CourseLayout layout = CourseOutputPages.CreateCourseLayout(loadedEvent.Controller.GetEventDB(), loadedEvent.SymbolDB, mapDisplay,
                                                                       loadedEvent.Controller.GetCourseAppearance(), Page.courseDesignator, settings.RenderControlDescriptions);
            mapDisplay.SetCourse(layout);
            mapDisplay.SetPrintArea(null);
            return mapDisplay;
        }

        // Create the transform from map coordinates to millimetres of the page's paper, with the y axis pointing down:
        // PDF export's transform from map coordinates to hundredths of an inch, scaled to millimetres.
        private Matrix CreateMapToPaperTransform()
        {
            Matrix transform = Geometry.CreateInvertedRectangleTransform(Page.mapRectangle, Page.printRectangle);
            transform.Scale(MillimetresPerHundredthOfInch, MillimetresPerHundredthOfInch, MatrixOrder.Append);
            return transform;
        }

        // Create the transform from millimetres of the page's paper to the pixels of an image of a size, which fits the
        // paper into the image as Chromium fits a document's view box into it: scaled alike in both directions, as far
        // as the image allows, and centered.
        // pixelSize: the size of the image.
        private Matrix CreatePaperToImageTransform(Size pixelSize)
        {
            SizeF paper = PaperSize;
            float scale = Math.Min(pixelSize.Width / paper.Width, pixelSize.Height / paper.Height);
            return new Matrix(scale, 0, 0, scale, (pixelSize.Width - paper.Width * scale) / 2, (pixelSize.Height - paper.Height * scale) / 2);
        }
    }
}
