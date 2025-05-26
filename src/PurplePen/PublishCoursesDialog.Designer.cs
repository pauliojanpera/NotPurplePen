namespace PurplePen
{
    partial class PublishCoursesDialog
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PublishCoursesDialog));
            this.publishTargetFolderGroupBox = new System.Windows.Forms.GroupBox();
            this.publishTargetFolderLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.publishTargetFolderTextBox = new System.Windows.Forms.TextBox();
            this.selectDataExchangeFolderButton = new System.Windows.Forms.Button();
            this.fileDirectory = new System.Windows.Forms.RadioButton();
            this.mapDirectory = new System.Windows.Forms.RadioButton();
            this.courseSelector = new PurplePen.CourseSelector();
            this.coursesGroupBox = new System.Windows.Forms.GroupBox();
            this.folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            this.tableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.advancedSettingsButton = new System.Windows.Forms.Button();
            this.publishTargetFolderGroupBox.SuspendLayout();
            this.publishTargetFolderLayoutPanel.SuspendLayout();
            this.panel1.SuspendLayout();
            this.coursesGroupBox.SuspendLayout();
            this.tableLayoutPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // okButton
            // 
            resources.ApplyResources(this.okButton, "okButton");
            // 
            // cancelButton
            // 
            resources.ApplyResources(this.cancelButton, "cancelButton");
            // 
            // publishTargetFolderGroupBox
            // 
            resources.ApplyResources(this.publishTargetFolderGroupBox, "publishTargetFolderGroupBox");
            this.publishTargetFolderGroupBox.Controls.Add(this.publishTargetFolderLayoutPanel);
            this.publishTargetFolderGroupBox.Name = "publishTargetFolderGroupBox";
            this.publishTargetFolderGroupBox.TabStop = false;
            // 
            // publishTargetFolderLayoutPanel
            // 
            resources.ApplyResources(this.publishTargetFolderLayoutPanel, "publishTargetFolderLayoutPanel");
            this.publishTargetFolderLayoutPanel.Controls.Add(this.panel1, 0, 0);
            this.publishTargetFolderLayoutPanel.Controls.Add(this.fileDirectory, 0, 1);
            this.publishTargetFolderLayoutPanel.Controls.Add(this.mapDirectory, 0, 2);
            this.publishTargetFolderLayoutPanel.Name = "publishTargetFolderLayoutPanel";
            // 
            // panel1
            // 
            resources.ApplyResources(this.panel1, "panel1");
            this.panel1.Controls.Add(this.publishTargetFolderTextBox);
            this.panel1.Controls.Add(this.selectDataExchangeFolderButton);
            this.panel1.Name = "panel1";
            // 
            // publishTargetFolderTextBox
            // 
            resources.ApplyResources(this.publishTargetFolderTextBox, "publishTargetFolderTextBox");
            this.publishTargetFolderTextBox.Name = "publishTargetFolderTextBox";
            this.publishTargetFolderTextBox.TextChanged += new System.EventHandler(this.targetFolderTextBox_TextChanged);
            // 
            // selectDataExchangeFolderButton
            // 
            resources.ApplyResources(this.selectDataExchangeFolderButton, "selectDataExchangeFolderButton");
            this.selectDataExchangeFolderButton.Name = "selectDataExchangeFolderButton";
            this.selectDataExchangeFolderButton.UseVisualStyleBackColor = true;
            this.selectDataExchangeFolderButton.Click += new System.EventHandler(this.selectDataExchangeFolderButton_Click);
            // 
            // fileDirectory
            // 
            resources.ApplyResources(this.fileDirectory, "fileDirectory");
            this.fileDirectory.Checked = true;
            this.fileDirectory.Name = "fileDirectory";
            this.fileDirectory.TabStop = true;
            this.fileDirectory.UseVisualStyleBackColor = true;
            // 
            // mapDirectory
            // 
            resources.ApplyResources(this.mapDirectory, "mapDirectory");
            this.mapDirectory.Name = "mapDirectory";
            this.mapDirectory.UseVisualStyleBackColor = true;
            // 
            // courseSelector
            // 
            resources.ApplyResources(this.courseSelector, "courseSelector");
            this.courseSelector.Filter = null;
            this.courseSelector.Name = "courseSelector";
            this.courseSelector.ShowAllControls = true;
            this.courseSelector.ShowCourseParts = false;
            this.courseSelector.ShowVariationChooser = true;
            // 
            // coursesGroupBox
            // 
            resources.ApplyResources(this.coursesGroupBox, "coursesGroupBox");
            this.coursesGroupBox.Controls.Add(this.courseSelector);
            this.coursesGroupBox.Name = "coursesGroupBox";
            this.coursesGroupBox.TabStop = false;
            // 
            // tableLayoutPanel
            // 
            resources.ApplyResources(this.tableLayoutPanel, "tableLayoutPanel");
            this.tableLayoutPanel.Controls.Add(this.publishTargetFolderGroupBox, 0, 1);
            this.tableLayoutPanel.Controls.Add(this.coursesGroupBox, 0, 0);
            this.tableLayoutPanel.Name = "tableLayoutPanel";
            // 
            // advancedSettingsButton
            // 
            resources.ApplyResources(this.advancedSettingsButton, "advancedSettingsButton");
            this.advancedSettingsButton.Name = "advancedSettingsButton";
            this.advancedSettingsButton.UseVisualStyleBackColor = true;
            this.advancedSettingsButton.Click += new System.EventHandler(this.advancedSettingsButton_Click);
            // 
            // PublishCoursesDialog
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.Controls.Add(this.tableLayoutPanel);
            this.Controls.Add(this.advancedSettingsButton);
            this.Name = "PublishCoursesDialog";
            this.Controls.SetChildIndex(this.okButton, 0);
            this.Controls.SetChildIndex(this.cancelButton, 0);
            this.Controls.SetChildIndex(this.advancedSettingsButton, 0);
            this.Controls.SetChildIndex(this.tableLayoutPanel, 0);
            this.publishTargetFolderGroupBox.ResumeLayout(false);
            this.publishTargetFolderLayoutPanel.ResumeLayout(false);
            this.publishTargetFolderLayoutPanel.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.coursesGroupBox.ResumeLayout(false);
            this.tableLayoutPanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.GroupBox publishTargetFolderGroupBox;
        private System.Windows.Forms.TableLayoutPanel publishTargetFolderLayoutPanel;
        private System.Windows.Forms.RadioButton fileDirectory;
        private System.Windows.Forms.RadioButton mapDirectory;
        private CourseSelector courseSelector;
        private System.Windows.Forms.GroupBox coursesGroupBox;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button selectDataExchangeFolderButton;
        private System.Windows.Forms.TextBox publishTargetFolderTextBox;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel;
        private System.Windows.Forms.Button advancedSettingsButton;
    }
}