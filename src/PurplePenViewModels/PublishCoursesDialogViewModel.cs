// PublishCoursesDialogViewModel.cs
//
// ViewModel for the Publish Courses dialog. The dialog wraps no settings
// class: the caller seeds it, shows it, and reads the chosen values back
// individually (see AGENTS.md, "ViewModels Without a Settings Class").

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.IO;

namespace PurplePen.ViewModels
{
    /// <summary>
    /// ViewModel for the Publish Courses dialog.
    /// Usage: the caller sets <see cref="EventDB"/> and <see cref="PublishTargetFolder"/>,
    /// shows the dialog, and on OK reads <see cref="SelectedCourses"/>,
    /// <see cref="VariationChoicesPerCourse"/>, <see cref="PublishTargetFolder"/>,
    /// <see cref="UseFileDirectory"/> and <see cref="UseMapDirectory"/>.
    /// </summary>
    public partial class PublishCoursesDialogViewModel : ViewModelBase
    {
        // ===== Inputs (set by caller before showing) =====

        /// <summary>The event database used to populate the course list.</summary>
        [ObservableProperty]
        private EventDB? eventDB;

        // ===== UI state — bound directly to dialog controls =====

        /// <summary>
        /// The folder the courses are published into. A rooted path is used as
        /// it stands; a relative one is resolved against the folder that
        /// <see cref="UseFileDirectory"/> or <see cref="UseMapDirectory"/>
        /// names, which is why those two only apply to a relative path.
        /// </summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsRelativePath))]
        private string publishTargetFolder = "";

        /// <summary>Resolve a relative target folder against the Purple Pen file's folder.</summary>
        [ObservableProperty]
        private bool useFileDirectory;

        /// <summary>Resolve a relative target folder against the map file's folder.</summary>
        [ObservableProperty]
        private bool useMapDirectory;

        /// <summary>
        /// Whether the target folder group is shown. It starts hidden, so that
        /// publishing is a course list and an OK button; the Advanced button
        /// reveals the group and then removes itself.
        /// </summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsAdvancedButtonVisible))]
        private bool showAdvanced;

        /// <summary>Whether the target folder is relative, so the two folder choices apply.</summary>
        public bool IsRelativePath => !Path.IsPathRooted(PublishTargetFolder);

        /// <summary>Whether the Advanced button is still there to be pressed.</summary>
        public bool IsAdvancedButtonVisible => !ShowAdvanced;

        /// <summary>Courses selected by the user (set by code-behind on Open/OK).</summary>
        public Id<Course>[] SelectedCourses { get; set; } = Array.Empty<Id<Course>>();

        /// <summary>Per-course variation choices (set by code-behind on Open/OK).</summary>
        public Dictionary<Id<Course>, VariationChoices> VariationChoicesPerCourse { get; set; } =
            new Dictionary<Id<Course>, VariationChoices>();

        /// <summary>Reveals the target folder group.</summary>
        [RelayCommand]
        private void RevealAdvancedSettings()
        {
            ShowAdvanced = true;
        }
    }
}
