using System;
using NUnit.Framework;
using PurplePen;
using PurplePen.ViewModels;

namespace PurplePenViewModels.Tests
{
    /// <summary>
    /// Tests for CreatePdfCoursesDialogViewModel's Settings, which assembles a
    /// CoursePdfSettings from the dialog's fields and decomposes one into them.
    /// </summary>
    [TestFixture]
    public class CreatePdfCoursesDialogViewModelTests
    {
        // The color models the dialog's color-model combo lists, in the order of its items.
        private static readonly ColorModel[] colorModelComboItems = { ColorModel.RGB, ColorModel.CMYK };

        /// <summary>
        /// Settings with each color model select that model's item of the color-model
        /// combo, and Settings gives the model back unchanged.
        /// </summary>
        /// <param name="colorModel">The color model of the settings the dialog opens with.</param>
        [Test]
        public void Settings_SelectsAndKeepsColorModel([Values] ColorModel colorModel)
        {
            CreatePdfCoursesDialogViewModel vm = new CreatePdfCoursesDialogViewModel {
                Settings = new CoursePdfSettings { ColorModel = colorModel },
            };

            Assert.That(vm.ColorModelIndex, Is.EqualTo(Array.IndexOf(colorModelComboItems, colorModel)));
            Assert.That(vm.Settings.ColorModel, Is.EqualTo(colorModel));
        }
    }
}
