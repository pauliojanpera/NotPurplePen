using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace PurplePen
{
    using PurplePen.Graphics2D;
    using PurplePen.MapModel;

    // Writes courses as SVG files, one page to a file (A-1 to A-6). Each page is laid out as PDF export lays it out, and
    // drawn through the SVG target, which keeps the structure of what is drawn: the map in two parts, below and above
    // the course's lower layer, and the course in its two layers between and over them (L-2, L-3). A page that needs a
    // map without a lower purple layer, or no map, has fewer parts.
    public class CourseSvg
    {
        // The smallest distance worth drawing, in millimetres of map: the step the target writes coordinates in (S-1, A-4).
        private const float MinResolution = 0.001F;

        // Millimetres in an inch, and in a hundredth of an inch, which page layouts are measured in.
        private const float MillimetresPerInch = 25.4F;
        private const float MillimetresPerHundredthOfInch = 0.254F;

        private readonly CourseSvgSettings settings;
        private readonly EventDB eventDB;
        private readonly SymbolDB symbolDB;
        private readonly Controller controller;
        private readonly MapDisplay mapDisplay;
        private readonly CourseAppearance appearance;
        private int totalPages, currentPage;

        // Create the writer of a set of SVG files.
        // eventDB: the event.
        // symbolDB: the symbols of the control descriptions.
        // controller: the controller, which shows the progress.
        // mapDisplay: a map display that holds the event's map, which the writer sets up for drawing pages.
        // settings: what to write, and where.
        // appearance: the course appearance.
        public CourseSvg(EventDB eventDB, SymbolDB symbolDB, Controller controller, MapDisplay mapDisplay,
                         CourseSvgSettings settings, CourseAppearance appearance)
        {
            this.eventDB = eventDB;
            this.symbolDB = symbolDB;
            this.controller = controller;
            this.mapDisplay = mapDisplay;
            this.settings = settings;
            this.appearance = appearance;

            // Draw as PDF export draws: the map at full intensity, as for printing, in the chosen color model.
            mapDisplay.MapIntensity = 1.0F;
            mapDisplay.AntiAlias = false;
            mapDisplay.Printing = true;
            mapDisplay.ColorModel = settings.ColorModel;
            if (settings.DontPrintBaseMap)
                mapDisplay.SetMapFile(MapType.None, null);
        }

        // The extension of the files written.
        private string Extension
        {
            get { return settings.Compress ? ".svgz" : ".svg"; }
        }

        // The files that writing would overwrite.
        public List<string> OverwrittenFiles()
        {
            return (from filePage in GetPagesToCreate()
                    where File.Exists(filePage.First)
                    select filePage.First).ToList();
        }

        // Write the files, showing the progress if the settings ask for it.
        public void CreateSvgs()
        {
            List<Pair<string, CoursePage>> pages = GetPagesToCreate();
            totalPages = pages.Count;

            if (settings.ShowProgressDialog)
                controller.ShowProgressDialog(true);

            try {
                currentPage = 0;
                foreach (Pair<string, CoursePage> filePage in pages) {
                    if (settings.ShowProgressDialog) {
                        if (controller.UpdateProgressDialog(string.Format(MiscText.CreatingFile, Path.GetFileName(filePage.First)), (double) currentPage / (double) totalPages))
                            throw new Exception(MiscText.CancelledByUser);
                    }

                    WritePage(filePage.First, filePage.Second);
                    currentPage += 1;
                }
            }
            finally {
                if (settings.ShowProgressDialog)
                    controller.EndProgressDialog();
            }
        }

        // Get each file to write, with the page it holds: a file for each course part, and when a course part needs
        // several pages, a file for each page, named with the page's number after the part's name (A-3).
        private List<Pair<string, CoursePage>> GetPagesToCreate()
        {
            List<Pair<string, CoursePage>> pages = new List<Pair<string, CoursePage>>();
            List<Pair<string, IEnumerable<CourseDesignator>>> files = CourseOutputPages.GetFilesToCreate(
                eventDB, CoursePdfSettings.PdfFileCreation.FilePerCoursePart, settings.CourseIds, settings.VariationChoicesPerCourse,
                settings.PrintMapExchangesOnOneMap, settings.filePrefix, settings.outputDirectory, Extension);

            CoursePageLayout pageLayout = new CoursePageLayout(eventDB, symbolDB, controller, appearance, settings.CropLargePrintArea);
            foreach (Pair<string, IEnumerable<CourseDesignator>> file in files) {
                List<CoursePage> filePages = pageLayout.LayoutPages(file.Second);
                for (int index = 0; index < filePages.Count; ++index) {
                    string fileName = filePages.Count == 1 ? file.First : NumberedFileName(file.First, index + 1);
                    pages.Add(new Pair<string, CoursePage>(fileName, filePages[index]));
                }
            }

            return pages;
        }

        // The name of the file of one page of several, with the page's number before the extension.
        // fileName: the name of the file the pages would share.
        // pageNumber: the page's number, from 1.
        private static string NumberedFileName(string fileName, int pageNumber)
        {
            string extension = Path.GetExtension(fileName);
            return fileName.Substring(0, fileName.Length - extension.Length) + "-" + pageNumber.ToString(System.Globalization.CultureInfo.InvariantCulture) + extension;
        }

        // Write one page as a file: the map's parts and the course's layers, each a part of the document, and the page
        // that shows them through its print area.
        // fileName: the file to write.
        // page: the page.
        private void WritePage(string fileName, CoursePage page)
        {
            // Lay the course out in its purple and put it into the map display.
            CourseLayout layout = CourseOutputPages.CreateCourseLayout(eventDB, symbolDB, mapDisplay, appearance, page.courseDesignator, settings.RenderControlDescriptions);
            mapDisplay.SetCourse(layout);
            mapDisplay.SetPrintArea(null);

            IColorConverter colorConverter = settings.ColorModel == ColorModel.CMYK ? (IColorConverter) SwopColorConverter.Instance : (IColorConverter) DefaultColorConverter.Instance;
            SvgBitmapOptions bitmapOptions = new SvgBitmapOptions(settings.BitmapHandling, settings.JpegQuality,
                                                                  settings.BitmapHandling == SvgBitmapHandling.Linked ? BitmapStoreBeside(fileName) : null);

            using (Svg_GraphicsTarget target = new Svg_GraphicsTarget(colorConverter, settings.ColorModel == ColorModel.CMYK, bitmapOptions,
                                                                      Services.TextMetricsProvider, Services.FontSubsetProvider)) {
                object mapKey = new object(), courseKey = new object();
                bool hasMap = mapDisplay.MapType != MapType.None;
                string mapLabel = hasMap ? Path.GetFileName(mapDisplay.FileName) : null;
                CoordinateMapper coordinateMapper = hasMap ? mapDisplay.CoordinateMapper : null;
                MapGeoreference georeference = coordinateMapper == null ? null : coordinateMapper.GetMapGeoreference();
                List<object> parts = new List<object>();

                if (hasMap)
                    AddPart(target, parts, mapKey, mapLabel, georeference, MapDisplayLayer.MapBelowCourse, page.mapRectangle);
                AddPart(target, parts, courseKey, page.description, null, MapDisplayLayer.CourseLower, page.mapRectangle);
                if (mapDisplay.HasLayersAboveCourse) {
                    AddPart(target, parts, mapKey, mapLabel, null, MapDisplayLayer.MapAboveCourse, page.mapRectangle);
                    AddPart(target, parts, courseKey, page.description, null, MapDisplayLayer.CourseUpper, page.mapRectangle);
                }

                using (Matrix mapToPage = MapToPage(page))
                    target.AddPage(PaperSize(page), page.mapRectangle, mapToPage, parts.ToArray());

                WriteDocument(fileName, target.FinishDocument());
            }
        }

        // Draw one layer of the page as a part of the document, holding one map.
        // target: the SVG target.
        // parts: the parts of the page so far, which the part is added to.
        // mapKey: the key of the map the layer draws, the same in each of the map's parts.
        // label: what the map is called.
        // georeference: where the map lies in the real world, recorded with it, or null.
        // layer: the layer to draw.
        // mapArea: the area of the map the page shows.
        private void AddPart(Svg_GraphicsTarget target, List<object> parts, object mapKey, string label, MapGeoreference georeference,
                             MapDisplayLayer layer, RectangleF mapArea)
        {
            object partKey = new object();
            target.BeginPart(partKey);
            target.BeginMap(mapKey, label);
            if (georeference != null)
                target.SetGeoreference(mapKey, georeference);
            mapDisplay.Draw(target, layer, mapArea, MinResolution, null);
            target.EndMap();
            target.EndPart();
            parts.Add(partKey);
        }

        // The size of a page's paper in millimetres, turned for a landscape page (F-4).
        // page: the page.
        private static SizeF PaperSize(CoursePage page)
        {
            SizeF inches = new SizeF(page.paperSize.SizeInInches.Width, page.paperSize.SizeInInches.Height);
            if (page.landscape)
                inches = new SizeF(inches.Height, inches.Width);
            return new SizeF(inches.Width * MillimetresPerInch, inches.Height * MillimetresPerInch);
        }

        // The transform from map coordinates to millimetres on a page's paper, with the y axis pointing down: PDF
        // export's transform from map coordinates to hundredths of an inch, scaled to millimetres.
        // page: the page.
        private static Matrix MapToPage(CoursePage page)
        {
            Matrix transform = Geometry.CreateInvertedRectangleTransform(page.mapRectangle, page.printRectangle);
            transform.Scale(MillimetresPerHundredthOfInch, MillimetresPerHundredthOfInch, MatrixOrder.Append);
            return transform;
        }

        // A store that writes each bitmap a document links to as a file beside the document, named after the document
        // and numbered, and gives the file's name as the link (B-4).
        // fileName: the document's file.
        private static Func<byte[], GraphicsBitmapFormat, string> BitmapStoreBeside(string fileName)
        {
            string directory = Path.GetDirectoryName(fileName);
            string baseName = Path.GetFileNameWithoutExtension(fileName);
            int count = 0;
            return (data, format) => {
                ++count;
                string name = baseName + "-" + count.ToString(System.Globalization.CultureInfo.InvariantCulture) + (format == GraphicsBitmapFormat.JPEG ? ".jpg" : ".png");
                File.WriteAllBytes(Path.Combine(directory, name), data);
                return Uri.EscapeDataString(name);
            };
        }

        // Write a document as a file, in UTF-8 without a byte order mark, and gzip-compressed when the settings ask.
        // fileName: the file.
        // document: the document's text.
        private void WriteDocument(string fileName, string document)
        {
            byte[] bytes = new UTF8Encoding(false).GetBytes(document);
            using (FileStream file = new FileStream(fileName, FileMode.Create, FileAccess.Write)) {
                if (settings.Compress) {
                    using (GZipStream compressed = new GZipStream(file, CompressionLevel.Optimal))
                        compressed.Write(bytes, 0, bytes.Length);
                }
                else {
                    file.Write(bytes, 0, bytes.Length);
                }
            }
        }
    }
}
