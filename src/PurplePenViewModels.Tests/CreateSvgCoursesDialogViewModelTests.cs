// CreateSvgCoursesDialogViewModelTests.cs
//
// Tests that the Create SVG Files dialog offers PDF export's choice of which
// pages go in each file (L-7).

using NUnit.Framework;
using PurplePen;
using PurplePen.ViewModels;

namespace PurplePenViewModels.Tests
{
    /// <summary>
    /// Tests the file choice of the Create SVG Files dialog's view model: its index numbers the choices as
    /// <see cref="CoursePdfSettings.PdfFileCreation"/> does, as the Create PDF Files dialog's does, and it goes through
    /// the settings both ways.
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
    }
}
