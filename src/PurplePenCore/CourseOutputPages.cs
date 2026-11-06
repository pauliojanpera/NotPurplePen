using System;
using System.Collections.Generic;
using System.IO;

namespace PurplePen
{
    using PurplePen.MapModel;

    // What the exports that write courses page by page share, PDF export and SVG export: which files to write and which
    // course designators go in each, and the course that a page draws, in its own purple.
    public static class CourseOutputPages
    {
        // Get the files to create, each with the course designators whose pages go in it.
        // eventDB: the event.
        // fileCreation: whether all pages go in one file, a file per course, or a file per course part.
        // courseIds: the courses to write.
        // variationChoicesPerCourse: the variations to write of each course that has them.
        // printMapExchangesOnOneMap: whether the parts of a course with map exchanges go on one map.
        // filePrefix: the prefix of the files' names, or null or empty for none.
        // outputDirectory: the directory to write the files in.
        // extension: the files' extension, with its period.
        public static List<Pair<string, IEnumerable<CourseDesignator>>> GetFilesToCreate(EventDB eventDB, CoursePdfSettings.PdfFileCreation fileCreation,
                                                                                         Id<Course>[] courseIds, Dictionary<Id<Course>, VariationChoices> variationChoicesPerCourse,
                                                                                         bool printMapExchangesOnOneMap, string filePrefix, string outputDirectory, string extension)
        {
            List<Pair<string, IEnumerable<CourseDesignator>>> fileList = new List<Pair<string, IEnumerable<CourseDesignator>>>();

            switch (fileCreation) {
                case CoursePdfSettings.PdfFileCreation.SingleFile:
                    // All pages go into a single file.
                    fileList.Add(new Pair<string, IEnumerable<CourseDesignator>>(CreateOutputFileName(eventDB, null, filePrefix, outputDirectory, extension),
                                 QueryEvent.EnumerateCourseDesignators(eventDB, courseIds, variationChoicesPerCourse, !printMapExchangesOnOneMap)));
                    break;

                case CoursePdfSettings.PdfFileCreation.FilePerCourse:
                    // Create a file for each course.
                    foreach (Id<Course> courseId in courseIds) {
                        fileList.Add(new Pair<string, IEnumerable<CourseDesignator>>(CreateOutputFileName(eventDB, new CourseDesignator(courseId), filePrefix, outputDirectory, extension),
                                     QueryEvent.EnumerateCourseDesignators(eventDB, new Id<Course>[1] { courseId }, variationChoicesPerCourse, !printMapExchangesOnOneMap)));
                    }
                    break;

                case CoursePdfSettings.PdfFileCreation.FilePerCoursePart:
                    // Create a file for each course part or variation (or both)
                    foreach (CourseDesignator designator in
                             QueryEvent.EnumerateCourseDesignators(eventDB, courseIds, variationChoicesPerCourse, !printMapExchangesOnOneMap)) {
                        fileList.Add(new Pair<string, IEnumerable<CourseDesignator>>(CreateOutputFileName(eventDB, designator, filePrefix, outputDirectory, extension), new[] { designator }));
                    }

                    break;
            }

            return fileList;
        }

        // Get the full output file name. Uses the name of the course, removes bad characters, checks for duplication of
        // the map file name, and puts it in the output directory.
        // eventDB: the event.
        // courseDesignator: the course or course part the file holds, or null for a file of every course.
        // filePrefix: the prefix of the file's name, or null or empty for none.
        // outputDirectory: the directory to write the file in.
        // extension: the file's extension, with its period.
        public static string CreateOutputFileName(EventDB eventDB, CourseDesignator courseDesignator, string filePrefix, string outputDirectory, string extension)
        {
            string basename = QueryEvent.CreateOutputFileName(eventDB, courseDesignator, filePrefix, "", extension);

            return Path.GetFullPath(Path.Combine(outputDirectory, basename));
        }

        // Create the layout of the course a page draws: the course as it prints, in the purple the map display's map
        // and the course appearance call for, with or without its control descriptions.
        // eventDB: the event.
        // symbolDB: the symbols of the control descriptions.
        // mapDisplay: the map display that holds the map, whose colors the purple is chosen among.
        // appearance: the course appearance.
        // courseDesignator: the course or course part the page draws.
        // renderDescriptions: whether to draw the control descriptions the course places on the map.
        public static CourseLayout CreateCourseLayout(EventDB eventDB, SymbolDB symbolDB, MapDisplay mapDisplay, CourseAppearance appearance,
                                                      CourseDesignator courseDesignator, bool renderDescriptions)
        {
            // Get the course view for the course we are printing.
            CourseView courseView = CourseView.CreatePrintingCourseView(eventDB, courseDesignator);

            // Get the correct purple color to print the course in.
            short ocadId;
            float purpleC, purpleM, purpleY, purpleK;
            bool purpleOverprint;
            FindPurple.GetPurpleColor(mapDisplay, appearance, out ocadId, out purpleC, out purpleM, out purpleY, out purpleK, out purpleOverprint);

            // Create a course layout from the view.
            CourseLayout layout = new CourseLayout();
            layout.SetLayerColor(CourseLayer.Descriptions, NormalCourseAppearance.blackColorOcadId, NormalCourseAppearance.blackColorName, NormalCourseAppearance.blackColorC, NormalCourseAppearance.blackColorM, NormalCourseAppearance.blackColorY, NormalCourseAppearance.blackColorK, false);
            layout.SetLayerColor(CourseLayer.MainCourse, ocadId, NormalCourseAppearance.courseColorName, purpleC, purpleM, purpleY, purpleK, purpleOverprint);
            layout.SetLowerLayerColor(CourseLayer.MainCourse, NormalCourseAppearance.lowerPurpleOcadId, NormalCourseAppearance.lowerPurpleColorName, purpleC, purpleM, purpleY, purpleK, purpleOverprint);

            CourseFormatterOptions formatterOptions = new CourseFormatterOptions();
            formatterOptions.showDescriptions = renderDescriptions;
            CourseFormatter.FormatCourseToLayout(symbolDB, courseView, appearance, layout, CourseLayer.MainCourse, formatterOptions);

            return layout;
        }
    }
}
