// CreateSvgCoursesDialog.axaml.cs
//
// Code-behind for the Create SVG Files dialog, as the Create PDF Files dialog's.
// Almost everything is data-bound to the CreateSvgCoursesDialogViewModel — see the
// ViewModel for the property layout. This file only handles:
//   1. Selection state on the CourseSelector (not bindable). Pulled from VM
//      on Opened, pushed back on OK.
//   2. The "Select folder..." button, which needs the parent window's
//      StorageProvider to show the platform folder picker.

using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using PurplePen.ViewModels;

namespace AvPurplePen.Views
{
    /// <summary>
    /// Dialog for creating SVG files of one or more courses, one file to a page.
    /// The caller must set DataContext to a <see cref="CreateSvgCoursesDialogViewModel"/>
    /// (with EventDB, ShowMergeParts, EnableChangeCropping, and Settings
    /// populated) before showing.
    /// </summary>
    public partial class CreateSvgCoursesDialog : Window
    {
        public CreateSvgCoursesDialog()
        {
            InitializeComponent();
            Opened += OnOpened;
        }

        /// <summary>
        /// Push the ViewModel's initial selection state into the CourseSelector.
        /// Done in Opened (not the constructor) because DataContext is set by
        /// the caller after construction.
        /// </summary>
        private void OnOpened(object? sender, EventArgs e)
        {
            if (DataContext is not CreateSvgCoursesDialogViewModel vm)
                return;

            courseSelector.SelectedCourseDesignators = vm.SelectedCourseDesignators;
            courseSelector.VariationChoicesPerCourse = vm.VariationChoicesPerCourse;
        }

        /// <summary>
        /// Opens the platform folder picker and writes the selected folder
        /// back into the ViewModel's OutputDirectory.
        /// </summary>
        private async void SelectOtherDirectoryButton_Click(object? sender, RoutedEventArgs e)
        {
            if (DataContext is not CreateSvgCoursesDialogViewModel vm)
                return;

            IStorageProvider storage = StorageProvider;

            FolderPickerOpenOptions options = new FolderPickerOpenOptions {
                Title = UIText.CreatePdfCourses_folderBrowserDialog_Description,
                AllowMultiple = false,
            };

            if (!string.IsNullOrEmpty(vm.OutputDirectory)) {
                options.SuggestedStartLocation = await storage.TryGetFolderFromPathAsync(vm.OutputDirectory);
            }

            IReadOnlyList<IStorageFolder> folders = await storage.OpenFolderPickerAsync(options);
            if (folders.Count > 0) {
                vm.OutputDirectory = folders[0].Path.LocalPath;
            }
        }

        /// <summary>
        /// Pulls the CourseSelector's selection state back into the ViewModel
        /// and closes with OK.
        /// </summary>
        private void OkButton_Click(object? sender, RoutedEventArgs e)
        {
            if (DataContext is CreateSvgCoursesDialogViewModel vm) {
                vm.SelectedCourseDesignators = courseSelector.SelectedCourseDesignators;
                vm.VariationChoicesPerCourse = courseSelector.VariationChoicesPerCourse;
            }
            Close(true);
        }

        /// <summary>Cancels and closes the dialog.</summary>
        private void CancelButton_Click(object? sender, RoutedEventArgs e)
        {
            Close(false);
        }
    }
}
