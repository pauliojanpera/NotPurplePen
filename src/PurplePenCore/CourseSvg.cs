using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace PurplePen
{
    using PurplePen.Graphics2D;
    using PurplePen.MapModel;

    // Writes courses as SVG files (A-1 to A-6): all pages in one file, a file for each course or a file for each course
    // part, as PDF export writes them (L-7). Each page is laid out as PDF export lays it out, and drawn through the SVG
    // target, which keeps the structure of what is drawn: the map in two parts, below and above the course's lower
    // layer, and the course in its two layers between and over them (L-2, L-3). A page that needs a map without a lower
    // purple layer, or no map, has fewer parts. A file draws each map once, for all the pages that show it, and each
    // page shows its own print area of the parts, through a view named after its course part (L-4, L-6).
    public class CourseSvg
    {
        // The smallest distance worth drawing, in millimetres of map: the step the target writes coordinates in (S-1, A-4).
        private const float MinResolution = 0.001F;

        // Millimetres in a hundredth of an inch, which page layouts are measured in.
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

            // Draw as PDF export draws: the map at full intensity, as for printing, in the chosen color model; but
            // anti-aliased, with a bitmap map scaled smoothly, and with the regular area patterns and the hatchings as
            // pattern brushes, which the map display draws for the SVG target, whose brushes are vectors, where PDF
            // export draws each element and each line; unless the settings ask for area patterns as separate shapes,
            // drawn element by element as PDF export draws them (S-7). Printing alone turns anti-aliasing off, which
            // the PDF target ignores and the SVG target writes as crisp edges (D-9).
            mapDisplay.MapIntensity = 1.0F;
            mapDisplay.AntiAlias = true;
            mapDisplay.Printing = true;
            mapDisplay.AntiAliasWhenPrinting = true;
            mapDisplay.AreaPatternsElementByElement = settings.DrawAreaPatternsAsSeparateShapes;
            mapDisplay.ColorModel = settings.ColorModel;
            mapDisplay.RecordsCourseObjects = true;
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
            return (from file in GetFilesToCreate()
                    where File.Exists(file.First)
                    select file.First).ToList();
        }

        // Write the files, showing the progress if the settings ask for it.
        public void CreateSvgs()
        {
            List<Pair<string, List<CoursePage>>> files = GetFilesToCreate();
            totalPages = files.Sum(file => file.Second.Count);

            if (settings.ShowProgressDialog)
                controller.ShowProgressDialog(true);

            try {
                currentPage = 0;
                foreach (Pair<string, List<CoursePage>> file in files) {
                    if (settings.ShowProgressDialog) {
                        if (controller.UpdateProgressDialog(string.Format(MiscText.CreatingFile, Path.GetFileName(file.First)), (double) currentPage / (double) totalPages))
                            throw new Exception(MiscText.CancelledByUser);
                    }

                    if (file.Second.Count > 0)
                        WriteFile(file.First, file.Second);
                    currentPage += file.Second.Count;
                }
            }
            finally {
                if (settings.ShowProgressDialog)
                    controller.EndProgressDialog();
            }
        }

        // Get each file to write, with the pages it holds, as PDF export lays them out: all pages in one file, a file for
        // each course or a file for each course part, as the settings choose, with every page laid out for the courses
        // or course parts that go in the file (L-7).
        private List<Pair<string, List<CoursePage>>> GetFilesToCreate()
        {
            List<Pair<string, IEnumerable<CourseDesignator>>> files = CourseOutputPages.GetFilesToCreate(
                eventDB, settings.FileCreation, settings.CourseIds, settings.VariationChoicesPerCourse,
                settings.PrintMapExchangesOnOneMap, settings.filePrefix, settings.outputDirectory, Extension);

            CoursePageLayout pageLayout = new CoursePageLayout(eventDB, symbolDB, controller, appearance, settings.CropLargePrintArea);
            return (from file in files
                    select new Pair<string, List<CoursePage>>(file.First, pageLayout.LayoutPages(file.Second))).ToList();
        }

        // Write a file of pages. Each map is drawn once, as parts of the document, for the area of all the pages that
        // show it (L-4): the base map's parts over the map rectangles of every page, and each course part's layers over
        // those of its own pages. The parts are drawn in the order the pages first show them, from the bottom up within
        // a page: the map below the course, the course part's lower layer, the map above the course and the part's
        // upper layer for the first page, then the layers of each further course part. Then each page is added, showing
        // the base map's parts and its own course part's through its print area, and named after its course part (L-3,
        // L-6).
        // fileName: the file to write.
        // pages: the pages, in the order they print.
        private void WriteFile(string fileName, List<CoursePage> pages)
        {
            mapDisplay.SetPrintArea(null);

            IColorConverter colorConverter = settings.ColorModel == ColorModel.CMYK ? (IColorConverter) SwopColorConverter.Instance : (IColorConverter) DefaultColorConverter.Instance;
            SvgBitmapOptions bitmapOptions = new SvgBitmapOptions(settings.BitmapHandling, settings.JpegQuality,
                                                                  settings.BitmapHandling == SvgBitmapHandling.Linked ? BitmapStoreBeside(fileName) : null);

            using (Svg_GraphicsTarget target = new Svg_GraphicsTarget(colorConverter, settings.ColorModel == ColorModel.CMYK, bitmapOptions,
                                                                      Services.TextMetricsProvider, Services.FontSubsetProvider)) {
                object mapKey = new object();
                bool hasMap = mapDisplay.MapType != MapType.None;
                string mapLabel = hasMap ? Path.GetFileName(mapDisplay.FileName) : null;
                CoordinateMapper coordinateMapper = hasMap ? mapDisplay.CoordinateMapper : null;
                MapGeoreference georeference = coordinateMapper == null ? null : coordinateMapper.GetMapGeoreference();
                RectangleF mapArea = UnionOfMapRectangles(pages);
                object mapBelowCourse = null, mapAboveCourse = null;

                // The course parts, each with its pages and the parts its layers are drawn in, in the order of their
                // first pages.
                List<CoursePartPages> courseParts = new List<CoursePartPages>();
                foreach (CoursePage page in pages) {
                    CoursePartPages coursePart = courseParts.FirstOrDefault(candidate => candidate.Designator.Equals(page.courseDesignator));
                    if (coursePart == null) {
                        coursePart = new CoursePartPages(page.courseDesignator);
                        courseParts.Add(coursePart);
                    }
                    coursePart.Pages.Add(page);
                }

                foreach (CoursePartPages coursePart in courseParts) {
                    // Lay the course part out in its purple and put it into the map display.
                    CourseLayout layout = CourseOutputPages.CreateCourseLayout(eventDB, symbolDB, mapDisplay, appearance, coursePart.Designator, settings.RenderControlDescriptions);
                    mapDisplay.SetCourse(layout);

                    CoursePage firstPage = coursePart.Pages[0];
                    RectangleF courseArea = UnionOfMapRectangles(coursePart.Pages);
                    if (hasMap && mapBelowCourse == null)
                        mapBelowCourse = DrawPart(target, mapKey, mapLabel, georeference, MapDisplayLayer.MapBelowCourse, mapArea);
                    coursePart.Lower = DrawPart(target, coursePart.Key, firstPage.description, null, MapDisplayLayer.CourseLower, courseArea);
                    if (mapDisplay.HasLayersAboveCourse) {
                        if (mapAboveCourse == null)
                            mapAboveCourse = DrawPart(target, mapKey, mapLabel, null, MapDisplayLayer.MapAboveCourse, mapArea);
                        coursePart.Upper = DrawPart(target, coursePart.Key, firstPage.description, null, MapDisplayLayer.CourseUpper, courseArea);
                    }

                    DescribeCourse(target, coursePart.Key, firstPage);
                }

                foreach (CoursePage page in pages) {
                    CoursePartPages coursePart = courseParts.First(candidate => candidate.Designator.Equals(page.courseDesignator));
                    object[] parts = new object[] { mapBelowCourse, coursePart.Lower, mapAboveCourse, coursePart.Upper }.Where(part => part != null).ToArray();
                    using (Matrix mapToPage = MapToPage(page))
                        target.AddPage(PaperSize(page), page.mapRectangle, mapToPage, parts, page.description);
                }

                WriteDocument(fileName, target.FinishDocument());
            }
        }

        // The pages of one course part in a file, with the key of the map its course drawing is, and the parts its two
        // layers are drawn in.
        private sealed class CoursePartPages
        {
            // The course part.
            public readonly CourseDesignator Designator;

            // The pages that show it, in the order they print.
            public readonly List<CoursePage> Pages = new List<CoursePage>();

            // The key of the map that its course drawing is, the same in both its parts.
            public readonly object Key = new object();

            // The keys of the parts its lower and upper layers are drawn in; the upper is null when the course is drawn
            // in one layer.
            public object Lower, Upper;

            // Create the pages of a course part, with none yet.
            // designator: the course part.
            public CoursePartPages(CourseDesignator designator)
            {
                Designator = designator;
            }
        }

        // The union of the map rectangles of pages: the area of the map they show between them.
        // pages: the pages, at least one.
        private static RectangleF UnionOfMapRectangles(List<CoursePage> pages)
        {
            RectangleF union = pages[0].mapRectangle;
            foreach (CoursePage page in pages)
                union = RectangleF.Union(union, page.mapRectangle);
            return union;
        }

        // Draw one layer as a part of the document, holding one map, and return the part's key.
        // target: the SVG target.
        // mapKey: the key of the map the layer draws, the same in each of the map's parts.
        // label: what the map is called.
        // georeference: where the map lies in the real world, recorded with it, or null.
        // layer: the layer to draw.
        // mapArea: the area of the map to draw: that of all the pages that show the part.
        private object DrawPart(Svg_GraphicsTarget target, object mapKey, string label, MapGeoreference georeference,
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
            return partKey;
        }

        // Describe, with the course's map, the course a page draws (CD-1), and each object of the course's map by the
        // course object that added it: its kind and the course's controls it belongs to (CD-2, CD-3).
        // target: the SVG target.
        // courseKey: the key of the course's map.
        // page: the page.
        private void DescribeCourse(Svg_GraphicsTarget target, object courseKey, CoursePage page)
        {
            CourseView courseView = CourseView.CreatePrintingCourseView(eventDB, page.courseDesignator);
            CourseRecord course = new CourseRecord {
                Id = page.courseDesignator.IsAllControls ? null : page.courseDesignator.CourseId.id.ToString(CultureInfo.InvariantCulture),
                Name = courseView.CourseName,
                Part = page.courseDesignator.AllParts ? 0 : page.courseDesignator.Part + 1,
                Variation = page.courseDesignator.IsVariation ? page.courseDesignator.VariationInfo.CodeString : null,
                PrintScale = courseView.PrintScale,
            };
            Dictionary<Id<CourseControl>, int> placeOfCourseControl = new Dictionary<Id<CourseControl>, int>();
            Dictionary<Id<ControlPoint>, int> placeOfControl = new Dictionary<Id<ControlPoint>, int>();
            foreach (CourseView.ControlView controlView in courseView.ControlViews) {
                ControlPoint control = eventDB.GetControl(controlView.controlId);
                course.Controls.Add(new CourseControlRecord {
                    Id = controlView.controlId.id.ToString(CultureInfo.InvariantCulture), Code = control.code,
                    Kind = ControlPoint.KindText(control.kind), Position = control.location,
                });
                if (!placeOfControl.ContainsKey(controlView.controlId))
                    placeOfControl.Add(controlView.controlId, course.Controls.Count);
                if (controlView.courseControlIds != null) {
                    foreach (Id<CourseControl> courseControlId in controlView.courseControlIds) {
                        if (!placeOfCourseControl.ContainsKey(courseControlId))
                            placeOfCourseControl.Add(courseControlId, course.Controls.Count);
                    }
                }
            }
            target.DescribeCourse(courseKey, course);

            Dictionary<object, CourseObjectRecord> objects = new Dictionary<object, CourseObjectRecord>(ReferenceEqualityComparer.Instance);
            Dictionary<CourseObj, CourseObjectRecord> recordOfCourseObject = new Dictionary<CourseObj, CourseObjectRecord>(ReferenceEqualityComparer.Instance);
            foreach (KeyValuePair<PurplePen.MapModel.Symbol, CourseObj> pair in mapDisplay.CourseObjectsOfSymbols) {
                CourseObjectRecord record;
                if (!recordOfCourseObject.TryGetValue(pair.Value, out record)) {
                    record = DescribeObject(pair.Value, placeOfCourseControl, placeOfControl);
                    recordOfCourseObject.Add(pair.Value, record);
                }
                objects[pair.Key] = record;
            }
            target.DescribeObjects(courseKey, objects);
        }

        // What a course object draws: its kind and the places of the course's controls it belongs to, its control, or
        // a leg's two. An object of a page of all controls belongs to no course control, and to its control point
        // instead.
        // courseObject: the course object.
        // placeOfCourseControl: the place, from 1, of each course control in the course's list of controls.
        // placeOfControl: the first place, from 1, of each control point in the course's list of controls.
        private static CourseObjectRecord DescribeObject(CourseObj courseObject, Dictionary<Id<CourseControl>, int> placeOfCourseControl,
                                                         Dictionary<Id<ControlPoint>, int> placeOfControl)
        {
            List<int> controls = new List<int>();
            int place;
            if (courseObject.courseControlId.IsNotNone && placeOfCourseControl.TryGetValue(courseObject.courseControlId, out place))
                controls.Add(place);
            else if (courseObject.courseControlId.IsNone && courseObject.controlId.IsNotNone && placeOfControl.TryGetValue(courseObject.controlId, out place))
                controls.Add(place);
            LineCourseObj line = courseObject as LineCourseObj;
            if (line != null && line.courseControlId2.IsNotNone && placeOfCourseControl.TryGetValue(line.courseControlId2, out place))
                controls.Add(place);
            return new CourseObjectRecord { Kind = KindOf(courseObject), Controls = controls.ToArray() };
        }

        // The kind of a course object, as the document names it: from its class, such as control-number for a
        // ControlNumberCourseObj, with a few named apart.
        // courseObject: the course object.
        private static string KindOf(CourseObj courseObject)
        {
            string name = courseObject.GetType().Name;
            if (name.EndsWith("CourseObj", StringComparison.Ordinal))
                name = name.Substring(0, name.Length - "CourseObj".Length);
            switch (name) {
                case "OOB": return "out-of-bounds";
                case "RegMark": return "registration-mark";
                case "TempConstruction": return "construction";
                case "TopologyLeg": return "leg";
                case "LineSpecial": return "line";
                case "RectSpecial": return "rectangle";
                case "BasicText": return "text";
            }
            StringBuilder kind = new StringBuilder(name.Length + 4);
            for (int index = 0; index < name.Length; ++index) {
                if (char.IsUpper(name[index]) && index > 0)
                    kind.Append('-');
                kind.Append(char.ToLowerInvariant(name[index]));
            }
            return kind.ToString();
        }

        // The size of a page's paper in millimetres, turned for a landscape page (F-4). Paper sizes are whole hundredths
        // of an inch, which are converted as such, so that A5, 583 by 827 hundredths, is 148.082 by 210.058 millimetres
        // and not those values with the error of inches held in single precision.
        // page: the page.
        private static SizeF PaperSize(CoursePage page)
        {
            double width = Math.Round(page.paperSize.SizeInInches.Width * 100.0) * 254 / 1000;
            double height = Math.Round(page.paperSize.SizeInInches.Height * 100.0) * 254 / 1000;
            return page.landscape ? new SizeF((float)height, (float)width) : new SizeF((float)width, (float)height);
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
            File.WriteAllBytes(fileName, settings.Compress ? Compress(bytes) : bytes);
        }

        // Bytes as one gzip member that is the same on every platform (S-10, F-2): GZipStream writes no file name and the
        // time as 0, but names in the header's last byte the operating system it runs on, which is named unknown instead.
        // bytes: the bytes.
        public static byte[] Compress(byte[] bytes)
        {
            const int operatingSystemOffset = 9;
            const byte unknownOperatingSystem = 255;
            using (MemoryStream member = new MemoryStream()) {
                using (GZipStream compressor = new GZipStream(member, CompressionLevel.Optimal, true))
                    compressor.Write(bytes, 0, bytes.Length);
                byte[] compressed = member.ToArray();
                compressed[operatingSystemOffset] = unknownOperatingSystem;
                return compressed;
            }
        }
    }
}
