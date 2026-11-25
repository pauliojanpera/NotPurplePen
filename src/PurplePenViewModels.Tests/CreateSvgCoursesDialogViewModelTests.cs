// CreateSvgCoursesDialogViewModelTests.cs
//
// Tests that the Create SVG Files dialog offers PDF export's choice of which
// pages go in each file (L-7), the choice of drawing area patterns as
// separate shapes (S-7), and the choice of compressing the files (S-10).

using NUnit.Framework;
using PurplePen;
using PurplePen.ViewModels;

namespace PurplePenViewModels.Tests
{
    /// <summary>
    /// Tests the choices of the Create SVG Files dialog's view model. The file choice's index numbers the choices as
    /// <see cref="CoursePdfSettings.PdfFileCreation"/> does, as the Create PDF Files dialog's does; it, the choice of
    /// drawing area patterns as separate shapes and the choice of compressing the files go through the settings both ways.
    /// </summary>
    [TestFixture]
    public class CreateSvgCoursesDialogViewModelTests
    {
        /// <summary>
        /// The dialog starts with the settings' default, a file for each course, as PDF export's does.
        /// </summary>
        [Test]
        public void TheFileChoiceStartsAtAFileForEachCourse()
        {
            CreateSvgCoursesDialogViewModel viewModel = new CreateSvgCoursesDialogViewModel { Settings = new CourseSvgSettings() };
            Assert.That(viewModel.FileFormatIndex, Is.EqualTo(1));
            Assert.That(viewModel.Settings.FileCreation, Is.EqualTo(CoursePdfSettings.PdfFileCreation.FilePerCourse));
        }

        /// <summary>
        /// Each file choice of the settings seeds the dialog's index, and the index chosen goes back into the settings.
        /// </summary>
        /// <param name="fileCreation">The file choice.</param>
        /// <param name="index">The index of the choice in the dialog's list.</param>
        [TestCase(CoursePdfSettings.PdfFileCreation.SingleFile, 0)]
        [TestCase(CoursePdfSettings.PdfFileCreation.FilePerCourse, 1)]
        [TestCase(CoursePdfSettings.PdfFileCreation.FilePerCoursePart, 2)]
        public void TheFileChoiceGoesThroughTheSettings(CoursePdfSettings.PdfFileCreation fileCreation, int index)
        {
            CreateSvgCoursesDialogViewModel seeded = new CreateSvgCoursesDialogViewModel {
                Settings = new CourseSvgSettings { FileCreation = fileCreation },
            };
            Assert.That(seeded.FileFormatIndex, Is.EqualTo(index));

            CreateSvgCoursesDialogViewModel chosen = new CreateSvgCoursesDialogViewModel { Settings = new CourseSvgSettings() };
            chosen.FileFormatIndex = index;
            Assert.That(chosen.Settings.FileCreation, Is.EqualTo(fileCreation));
        }

        /// <summary>
        /// The dialog starts with the settings' default: area patterns written as pattern fills, not as separate shapes.
        /// </summary>
        [Test]
        public void PatternsStartAsPatternFills()
        {
            CreateSvgCoursesDialogViewModel viewModel = new CreateSvgCoursesDialogViewModel { Settings = new CourseSvgSettings() };
            Assert.That(viewModel.DrawAreaPatternsAndDashesAsSeparateShapes, Is.False);
            Assert.That(viewModel.Settings.DrawAreaPatternsAndDashesAsSeparateShapes, Is.False);
        }

        /// <summary>
        /// The choice of drawing area patterns and dashed lines as separate shapes in the settings seeds the dialog's
        /// check box, and the box's state goes back into the settings.
        /// </summary>
        /// <param name="drawAreaPatternsAndDashesAsSeparateShapes">Whether area patterns and dashed lines are drawn as separate shapes.</param>
        [TestCase(false)]
        [TestCase(true)]
        public void TheChoiceOfDrawingAreaPatternsAndDashesAsSeparateShapesGoesThroughTheSettings(bool drawAreaPatternsAndDashesAsSeparateShapes)
        {
            CreateSvgCoursesDialogViewModel seeded = new CreateSvgCoursesDialogViewModel {
                Settings = new CourseSvgSettings { DrawAreaPatternsAndDashesAsSeparateShapes = drawAreaPatternsAndDashesAsSeparateShapes },
            };
            Assert.That(seeded.DrawAreaPatternsAndDashesAsSeparateShapes, Is.EqualTo(drawAreaPatternsAndDashesAsSeparateShapes));

            CreateSvgCoursesDialogViewModel chosen = new CreateSvgCoursesDialogViewModel {
                Settings = new CourseSvgSettings { DrawAreaPatternsAndDashesAsSeparateShapes = !drawAreaPatternsAndDashesAsSeparateShapes },
            };
            chosen.DrawAreaPatternsAndDashesAsSeparateShapes = drawAreaPatternsAndDashesAsSeparateShapes;
            Assert.That(chosen.Settings.DrawAreaPatternsAndDashesAsSeparateShapes, Is.EqualTo(drawAreaPatternsAndDashesAsSeparateShapes));
        }

        /// <summary>
        /// The dialog starts with the settings' default: the files written plain, not compressed.
        /// </summary>
        [Test]
        public void FilesStartUncompressed()
        {
            CreateSvgCoursesDialogViewModel viewModel = new CreateSvgCoursesDialogViewModel { Settings = new CourseSvgSettings() };
            Assert.That(viewModel.Compress, Is.False);
            Assert.That(viewModel.Settings.Compress, Is.False);
        }

        /// <summary>
        /// The choice of compressing the files, as .svgz, in the settings seeds the dialog's check box, and the box's
        /// state goes back into the settings.
        /// </summary>
        /// <param name="compress">Whether the files are compressed.</param>
        [TestCase(false)]
        [TestCase(true)]
        public void TheChoiceOfCompressingTheFilesGoesThroughTheSettings(bool compress)
        {
            CreateSvgCoursesDialogViewModel seeded = new CreateSvgCoursesDialogViewModel { Settings = new CourseSvgSettings { Compress = compress } };
            Assert.That(seeded.Compress, Is.EqualTo(compress));

            CreateSvgCoursesDialogViewModel chosen = new CreateSvgCoursesDialogViewModel { Settings = new CourseSvgSettings { Compress = !compress } };
            chosen.Compress = compress;
            Assert.That(chosen.Settings.Compress, Is.EqualTo(compress));
        }
    }
}
