using System;
using System.IO;
using System.Windows.Forms;

namespace PurplePen
{
    public partial class PublishCoursesDialog : OkCancelDialog
    {
        public Id<Course>[] SelectedCourses => courseSelector.SelectedCourses;
        public string DataExchangeFolderPath => publishTargetFolderTextBox.Text;
        public bool UseFileDirectory => fileDirectory.Checked;
        public bool UseMapDirectory => mapDirectory.Checked;

        public PublishCoursesDialog(EventDB eventDB)
        {
            InitializeComponent();
            courseSelector.EventDB = eventDB;
            publishTargetFolderTextBox.Text = MiscText.DefaultPublishPath;
            tableLayoutPanel.Height -= publishTargetFolderGroupBox.Height;
            this.MinimumSize = new System.Drawing.Size(this.Width, this.Height - publishTargetFolderGroupBox.Height);
            this.Height -= publishTargetFolderGroupBox.Height;
        }

        private void selectDataExchangeFolderButton_Click(object sender, EventArgs e)
        {
            folderBrowserDialog.SelectedPath = publishTargetFolderTextBox.Text;
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                publishTargetFolderTextBox.Text = folderBrowserDialog.SelectedPath;
        }

        private void targetFolderTextBox_TextChanged(object sender, EventArgs e)
        {
            bool isRooted = Path.IsPathRooted(publishTargetFolderTextBox.Text);
            fileDirectory.Enabled = !isRooted;
            mapDirectory.Enabled = !isRooted;
        }

        private void advancedSettingsButton_Click(object sender, EventArgs e)
        {
            advancedSettingsButton.Visible = false;
            publishTargetFolderGroupBox.Visible = true;
            tableLayoutPanel.Height += publishTargetFolderGroupBox.Height;
            this.Height += publishTargetFolderGroupBox.Height;
        }
    }
}
