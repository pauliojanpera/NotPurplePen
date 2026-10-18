// PublishCoursesDialog.axaml.cs
//
// Code-behind for the Publish Courses dialog. Everything that can be bound is
// bound to PublishCoursesDialogViewModel; what is left here is:
//   1. Selection state on the CourseSelector (not bindable), restored from
//      the ViewModel on Opened and read back into it on OK.
//   2. The "Select folder..." button, which needs the window's StorageProvider
//      to show the platform folder picker.
//
// Migrated from WinForms PurplePen/PublishCoursesDialog.cs.

using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using PurplePen.ViewModels;

namespace AvPurplePen.Views
{
    /// <summary>
    /// Dialog for publishing courses as images and PDFs. The caller must set
    /// DataContext to a <see cref="PublishCoursesDialogViewModel"/> (with
    /// EventDB and PublishTargetFolder populated) before showing.
    /// </summary>
    public partial class PublishCoursesDialog : Window
    {
        public PublishCoursesDialog()
        {
            InitializeComponent();
            Opened += OnOpened;
        }

        /// <summary>
        /// Restores a selection the ViewModel already carries, so that a
        /// dialog shown again after an error or an overwrite prompt comes
        /// back with the courses the user picked. An empty selection leaves
        /// the CourseSelector's own initial state alone.
        /// </summary>
        private void OnOpened(object? sender, EventArgs e)
        {
            if (DataContext is PublishCoursesDialogViewModel vm && vm.SelectedCourses.Length > 0) {
                courseSelector.SelectedCourses = vm.SelectedCourses;
            }
        }

        /// <summary>
        /// Opens the platform folder picker and writes the chosen folder back
        /// into the ViewModel's PublishTargetFolder.
        /// </summary>
        private async void SelectDataExchangeFolderButton_Click(object? sender, RoutedEventArgs e)
        {
            if (DataContext is not PublishCoursesDialogViewModel vm)
                return;

            IStorageProvider storage = StorageProvider;

            FolderPickerOpenOptions options = new FolderPickerOpenOptions {
                Title = UIText.PublishCoursesDialog_publishTargetFolderGroupBox_Text,
                AllowMultiple = false,
            };

            if (!string.IsNullOrEmpty(vm.PublishTargetFolder)) {
                options.SuggestedStartLocation = await storage.TryGetFolderFromPathAsync(vm.PublishTargetFolder);
            }

            IReadOnlyList<IStorageFolder> folders = await storage.OpenFolderPickerAsync(options);
            if (folders.Count > 0) {
                vm.PublishTargetFolder = folders[0].Path.LocalPath;
            }
        }

        /// <summary>
        /// Pulls the CourseSelector's selection state into the ViewModel and
        /// closes with OK.
        /// </summary>
        private void OkButton_Click(object? sender, RoutedEventArgs e)
        {
            if (DataContext is PublishCoursesDialogViewModel vm) {
                vm.SelectedCourses = courseSelector.SelectedCourses;
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
