using System;
using System.IO;
using System.Windows.Forms;

namespace PurplePen
{
    public partial class PublishCoursesDialog : OkCancelDialog
    {
        public Id<Course>[] SelectedCourses => courseSelector.SelectedCourses;
        public string DataExchangeFolderPath => dataExchangeFolderTextBox.Text;

        public PublishCoursesDialog(EventDB eventDB)
        {
            InitializeComponent();
            courseSelector.EventDB = eventDB;
            dataExchangeFolderTextBox.Text = MiscText.DefaultPublishPath;
            tableLayoutPanel.Height -= dataExchangeFolderGroupBox.Height;
            this.Height -= dataExchangeFolderGroupBox.Height;
        }

        private void selectDataExchangeFolderButton_Click(object sender, EventArgs e)
        {
            folderBrowserDialog.SelectedPath = dataExchangeFolderTextBox.Text;
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                dataExchangeFolderTextBox.Text = folderBrowserDialog.SelectedPath;
        }

        private void targetFolderTextBox_TextChanged(object sender, EventArgs e)
        {
            bool isRooted = Path.IsPathRooted(dataExchangeFolderTextBox.Text);
            coursesDirectory.Enabled = !isRooted;
            mapDirectory.Enabled = !isRooted;
        }

        private void advancedSettingsButton_Click(object sender, EventArgs e)
        {
            advancedSettingsButton.Visible = false;
            dataExchangeFolderGroupBox.Visible = true;
            tableLayoutPanel.Height += dataExchangeFolderGroupBox.Height;
            this.Height += dataExchangeFolderGroupBox.Height;
        }
    }
}
