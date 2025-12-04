using System.ComponentModel;
using System.Drawing;

namespace UpdateBuilder
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            txtInputDirectory = new TextBox();
            txtDestinationFolder = new TextBox();
            btnProcess = new Button();
            btnBrowseInput = new Button();
            btnBrowseDestination = new Button();
            overallProgressBar = new ProgressBar();
            label1 = new Label();
            label2 = new Label();
            label4 = new Label();
            txtProductName = new TextBox();
            label6 = new Label();
            txtCurrentVersion = new TextBox();
            txtNewVersion = new TextBox();
            lblNewVersion = new Label();
            chkAutoIncrement = new CheckBox();
            label5 = new Label();
            cmbCompressionLevel = new ComboBox();
            label7 = new Label();
            btnCancel = new Button();
            lblCurrentFile = new Label();
            SuspendLayout();
            //
            // txtInputDirectory
            //
            txtInputDirectory.Location = new Point(112, 12);
            txtInputDirectory.Name = "txtInputDirectory";
            txtInputDirectory.Size = new Size(418, 23);
            txtInputDirectory.TabIndex = 0;
            //
            // txtDestinationFolder
            //
            txtDestinationFolder.Location = new Point(112, 41);
            txtDestinationFolder.Name = "txtDestinationFolder";
            txtDestinationFolder.Size = new Size(418, 23);
            txtDestinationFolder.TabIndex = 1;
            //
            // btnProcess
            //
            btnProcess.Location = new Point(536, 99);
            btnProcess.Name = "btnProcess";
            btnProcess.Size = new Size(75, 52);
            btnProcess.TabIndex = 2;
            btnProcess.Text = "Start";
            btnProcess.UseVisualStyleBackColor = true;
            btnProcess.Click += BtnProcess_Click;
            //
            // btnBrowseInput
            //
            btnBrowseInput.Location = new Point(536, 12);
            btnBrowseInput.Name = "btnBrowseInput";
            btnBrowseInput.Size = new Size(75, 23);
            btnBrowseInput.TabIndex = 3;
            btnBrowseInput.Text = "Select";
            btnBrowseInput.UseVisualStyleBackColor = true;
            btnBrowseInput.Click += BtnBrowseInput_Click;
            //
            // btnBrowseDestination
            //
            btnBrowseDestination.Location = new Point(536, 41);
            btnBrowseDestination.Name = "btnBrowseDestination";
            btnBrowseDestination.Size = new Size(75, 23);
            btnBrowseDestination.TabIndex = 4;
            btnBrowseDestination.Text = "Select";
            btnBrowseDestination.UseVisualStyleBackColor = true;
            btnBrowseDestination.Click += BtnBrowseDestination_Click;
            //
            // overallProgressBar
            //
            overallProgressBar.Location = new Point(112, 157);
            overallProgressBar.Name = "overallProgressBar";
            overallProgressBar.Size = new Size(499, 23);
            overallProgressBar.TabIndex = 6;
            //
            // label1
            //
            label1.AutoSize = true;
            label1.Location = new Point(12, 15);
            label1.Name = "label1";
            label1.Size = new Size(71, 15);
            label1.TabIndex = 7;
            label1.Text = "Input Folder";
            //
            // label2
            //
            label2.AutoSize = true;
            label2.Location = new Point(12, 45);
            label2.Name = "label2";
            label2.Size = new Size(81, 15);
            label2.TabIndex = 8;
            label2.Text = "Output Folder";
            //
            // label4
            //
            label4.AutoSize = true;
            label4.Location = new Point(12, 160);
            label4.Name = "label4";
            label4.Size = new Size(54, 15);
            label4.TabIndex = 10;
            label4.Text = "Progress";
            //
            // txtProductName
            //
            txtProductName.Location = new Point(112, 128);
            txtProductName.Name = "txtProductName";
            txtProductName.Size = new Size(418, 23);
            txtProductName.TabIndex = 12;
            //
            // label6
            //
            label6.AutoSize = true;
            label6.Location = new Point(12, 131);
            label6.Name = "label6";
            label6.Size = new Size(84, 15);
            label6.TabIndex = 14;
            label6.Text = "Product Name";
            //
            // txtCurrentVersion
            //
            txtCurrentVersion.Location = new Point(112, 70);
            txtCurrentVersion.Name = "txtCurrentVersion";
            txtCurrentVersion.ReadOnly = true;
            txtCurrentVersion.Size = new Size(100, 23);
            txtCurrentVersion.TabIndex = 11;
            //
            // txtNewVersion
            //
            txtNewVersion.Location = new Point(296, 70);
            txtNewVersion.Name = "txtNewVersion";
            txtNewVersion.Size = new Size(100, 23);
            txtNewVersion.TabIndex = 15;
            //
            // lblNewVersion
            //
            lblNewVersion.AutoSize = true;
            lblNewVersion.Location = new Point(218, 73);
            lblNewVersion.Name = "lblNewVersion";
            lblNewVersion.Size = new Size(72, 15);
            lblNewVersion.TabIndex = 16;
            lblNewVersion.Text = "New Version";
            //
            // chkAutoIncrement
            //
            chkAutoIncrement.AutoSize = true;
            chkAutoIncrement.CheckAlign = ContentAlignment.MiddleRight;
            chkAutoIncrement.Location = new Point(402, 72);
            chkAutoIncrement.Name = "chkAutoIncrement";
            chkAutoIncrement.Size = new Size(128, 19);
            chkAutoIncrement.TabIndex = 17;
            chkAutoIncrement.Text = "Auto Increment?";
            chkAutoIncrement.TextAlign = ContentAlignment.MiddleCenter;
            chkAutoIncrement.UseVisualStyleBackColor = true;
            chkAutoIncrement.CheckedChanged += ChkAutoIncrement_CheckedChanged;
            //
            // label5
            //
            label5.AutoSize = true;
            label5.Location = new Point(12, 73);
            label5.Name = "label5";
            label5.Size = new Size(88, 15);
            label5.TabIndex = 18;
            label5.Text = "Current Version";
            //
            // cmbCompressionLevel
            //
            cmbCompressionLevel.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbCompressionLevel.FormattingEnabled = true;
            cmbCompressionLevel.Location = new Point(112, 99);
            cmbCompressionLevel.Name = "cmbCompressionLevel";
            cmbCompressionLevel.Size = new Size(150, 23);
            cmbCompressionLevel.TabIndex = 19;
            //
            // label7
            //
            label7.AutoSize = true;
            label7.Location = new Point(12, 102);
            label7.Name = "label7";
            label7.Size = new Size(78, 15);
            label7.TabIndex = 20;
            label7.Text = "Compression";
            //
            // btnCancel
            //
            btnCancel.Enabled = false;
            btnCancel.Location = new Point(268, 99);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(75, 23);
            btnCancel.TabIndex = 21;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += BtnCancel_Click;
            //
            // lblCurrentFile
            //
            lblCurrentFile.Location = new Point(12, 186);
            lblCurrentFile.Name = "lblCurrentFile";
            lblCurrentFile.Size = new Size(599, 40);
            lblCurrentFile.TabIndex = 22;
            lblCurrentFile.Text = "";
            //
            // MainForm
            //
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(624, 230);
            Controls.Add(lblCurrentFile);
            Controls.Add(btnCancel);
            Controls.Add(label7);
            Controls.Add(cmbCompressionLevel);
            Controls.Add(label5);
            Controls.Add(label4);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(overallProgressBar);
            Controls.Add(btnBrowseDestination);
            Controls.Add(btnBrowseInput);
            Controls.Add(btnProcess);
            Controls.Add(txtDestinationFolder);
            Controls.Add(txtInputDirectory);
            Controls.Add(chkAutoIncrement);
            Controls.Add(lblNewVersion);
            Controls.Add(txtNewVersion);
            Controls.Add(txtCurrentVersion);
            Controls.Add(label6);
            Controls.Add(txtProductName);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Name = "MainForm";
            ShowIcon = false;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Update Builder";
            Load += MainForm_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox txtInputDirectory;
        private TextBox txtDestinationFolder;
        private Button btnProcess;
        private Button btnBrowseInput;
        private Button btnBrowseDestination;
        private ProgressBar overallProgressBar;
        private Label label1;
        private Label label2;
        private Label label4;
        private TextBox txtProductName;
        private Label label6;
        private TextBox txtCurrentVersion;
        private TextBox txtNewVersion;
        private Label lblNewVersion;
        private CheckBox chkAutoIncrement;
        private Label label5;
        private ComboBox cmbCompressionLevel;
        private Label label7;
        private Button btnCancel;
        private Label lblCurrentFile;
    }
}
