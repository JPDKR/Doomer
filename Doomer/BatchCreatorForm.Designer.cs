namespace Doomer
{
    partial class BatchCreatorForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            lblFileName = new Label();
            lblSourcePort = new Label();
            lblIWad = new Label();
            lblWad = new Label();
            lblPlugins = new Label();
            lblExtraFiles = new Label();
            txtExtraFiles = new TextBox();
            txtFileName = new TextBox();
            btnFromArchive = new Button();
            lblFromArchive = new Label();
            lblArchiveSeparator = new Label();
            cmbSourcePort = new ComboBox();
            txtIWad = new TextBox();
            btnBrowseIWad = new Button();
            txtWad = new TextBox();
            btnBrowseWad = new Button();
            txtPlugins = new TextBox();
            lblImage = new Label();
            txtImage = new TextBox();
            btnBrowseImage = new Button();
            btnCreate = new Button();
            SuspendLayout();
            //
            // btnFromArchive
            //
            btnFromArchive.Location = new Point(8, 14);
            btnFromArchive.Name = "btnFromArchive";
            btnFromArchive.Size = new Size(150, 30);
            btnFromArchive.TabIndex = 17;
            btnFromArchive.Text = "From archive...";
            btnFromArchive.Click += BtnFromArchive_Click;
            //
            // lblFromArchive
            //
            lblFromArchive.AutoSize = false;
            lblFromArchive.Location = new Point(166, 14);
            lblFromArchive.Name = "lblFromArchive";
            lblFromArchive.Size = new Size(320, 30);
            lblFromArchive.TabIndex = 18;
            lblFromArchive.Text = "Fill in the form from a .zip, .rar or .7z";
            lblFromArchive.TextAlign = ContentAlignment.MiddleLeft;
            //
            // lblArchiveSeparator
            //
            lblArchiveSeparator.AutoSize = false;
            lblArchiveSeparator.BackColor = Color.FromArgb(60, 60, 60);
            lblArchiveSeparator.Location = new Point(8, 58);
            lblArchiveSeparator.Name = "lblArchiveSeparator";
            lblArchiveSeparator.Size = new Size(478, 1);
            lblArchiveSeparator.TabIndex = 19;
            //
            // lblFileName
            //
            lblFileName.AutoSize = false;
            lblFileName.Location = new Point(8, 78);
            lblFileName.Name = "lblFileName";
            lblFileName.Size = new Size(90, 27);
            lblFileName.TabIndex = 0;
            lblFileName.Text = "File Name";
            lblFileName.TextAlign = ContentAlignment.MiddleRight;
            //
            // lblSourcePort
            //
            lblSourcePort.AutoSize = false;
            lblSourcePort.Location = new Point(8, 114);
            lblSourcePort.Name = "lblSourcePort";
            lblSourcePort.Size = new Size(90, 27);
            lblSourcePort.TabIndex = 1;
            lblSourcePort.Text = "Source Port";
            lblSourcePort.TextAlign = ContentAlignment.MiddleRight;
            //
            // lblIWad
            //
            lblIWad.AutoSize = false;
            lblIWad.Location = new Point(8, 150);
            lblIWad.Name = "lblIWad";
            lblIWad.Size = new Size(90, 27);
            lblIWad.TabIndex = 2;
            lblIWad.Text = "IWAD";
            lblIWad.TextAlign = ContentAlignment.MiddleRight;
            //
            // lblWad
            //
            lblWad.AutoSize = false;
            lblWad.Location = new Point(8, 186);
            lblWad.Name = "lblWad";
            lblWad.Size = new Size(90, 27);
            lblWad.TabIndex = 3;
            lblWad.Text = "WAD";
            lblWad.TextAlign = ContentAlignment.MiddleRight;
            //
            // lblPlugins
            //
            lblPlugins.AutoSize = false;
            lblPlugins.Location = new Point(8, 294);
            lblPlugins.Name = "lblPlugins";
            lblPlugins.Size = new Size(90, 27);
            lblPlugins.TabIndex = 4;
            lblPlugins.Text = "Plugins";
            lblPlugins.TextAlign = ContentAlignment.MiddleRight;
            //
            // txtFileName
            //
            txtFileName.Location = new Point(106, 78);
            txtFileName.Name = "txtFileName";
            txtFileName.Size = new Size(380, 27);
            txtFileName.TabIndex = 5;
            //
            // cmbSourcePort
            //
            cmbSourcePort.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbSourcePort.Location = new Point(106, 114);
            cmbSourcePort.Name = "cmbSourcePort";
            cmbSourcePort.Size = new Size(180, 28);
            cmbSourcePort.TabIndex = 6;
            cmbSourcePort.SelectedIndexChanged += CmbSourcePort_SelectedIndexChanged;
            //
            // txtIWad
            //
            txtIWad.Location = new Point(106, 150);
            txtIWad.Name = "txtIWad";
            txtIWad.Size = new Size(296, 27);
            txtIWad.TabIndex = 7;
            //
            // btnBrowseIWad
            //
            btnBrowseIWad.Location = new Point(408, 150);
            btnBrowseIWad.Name = "btnBrowseIWad";
            btnBrowseIWad.Size = new Size(78, 27);
            btnBrowseIWad.TabIndex = 8;
            btnBrowseIWad.Text = "Browse...";
            btnBrowseIWad.Click += BtnBrowseIWad_Click;
            //
            // txtWad
            //
            txtWad.Location = new Point(106, 186);
            txtWad.Name = "txtWad";
            txtWad.Size = new Size(296, 27);
            txtWad.TabIndex = 9;
            //
            // btnBrowseWad
            //
            btnBrowseWad.Location = new Point(408, 186);
            btnBrowseWad.Name = "btnBrowseWad";
            btnBrowseWad.Size = new Size(78, 27);
            btnBrowseWad.TabIndex = 10;
            btnBrowseWad.Text = "Browse...";
            btnBrowseWad.Click += BtnBrowseWad_Click;
            //
            // lblExtraFiles
            //
            lblExtraFiles.AutoSize = false;
            lblExtraFiles.Location = new Point(8, 222);
            lblExtraFiles.Name = "lblExtraFiles";
            lblExtraFiles.Size = new Size(90, 27);
            lblExtraFiles.TabIndex = 20;
            lblExtraFiles.Text = "Extra files";
            lblExtraFiles.TextAlign = ContentAlignment.MiddleRight;
            //
            // txtExtraFiles
            //
            txtExtraFiles.AcceptsReturn = true;
            txtExtraFiles.Location = new Point(106, 222);
            txtExtraFiles.Multiline = true;
            txtExtraFiles.Name = "txtExtraFiles";
            txtExtraFiles.PlaceholderText = "Optional, one per line (e.g. .deh patches)";
            txtExtraFiles.ScrollBars = ScrollBars.Both;
            txtExtraFiles.Size = new Size(380, 63);
            txtExtraFiles.TabIndex = 11;
            txtExtraFiles.WordWrap = false;
            //
            // txtPlugins
            //
            txtPlugins.Location = new Point(106, 294);
            txtPlugins.Name = "txtPlugins";
            txtPlugins.Size = new Size(380, 27);
            txtPlugins.TabIndex = 12;
            //
            // lblImage
            //
            lblImage.AutoSize = false;
            lblImage.Location = new Point(8, 330);
            lblImage.Name = "lblImage";
            lblImage.Size = new Size(90, 27);
            lblImage.TabIndex = 13;
            lblImage.Text = "Image";
            lblImage.TextAlign = ContentAlignment.MiddleRight;
            //
            // txtImage
            //
            txtImage.Location = new Point(106, 330);
            txtImage.Name = "txtImage";
            txtImage.Size = new Size(296, 27);
            txtImage.TabIndex = 14;
            txtImage.KeyDown += TxtImage_KeyDown;
            //
            // btnBrowseImage
            //
            btnBrowseImage.Location = new Point(408, 330);
            btnBrowseImage.Name = "btnBrowseImage";
            btnBrowseImage.Size = new Size(78, 27);
            btnBrowseImage.TabIndex = 15;
            btnBrowseImage.Text = "Browse...";
            btnBrowseImage.Click += BtnBrowseImage_Click;
            //
            // btnCreate
            //
            btnCreate.Location = new Point(246, 374);
            btnCreate.Name = "btnCreate";
            btnCreate.Size = new Size(100, 30);
            btnCreate.TabIndex = 16;
            btnCreate.Text = "Create";
            btnCreate.UseVisualStyleBackColor = true;
            btnCreate.Click += BtnCreate_Click;
            //
            // BatchCreatorForm
            //
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(502, 420);
            Controls.Add(btnCreate);
            Controls.Add(btnBrowseImage);
            Controls.Add(txtImage);
            Controls.Add(lblImage);
            Controls.Add(txtPlugins);
            Controls.Add(txtExtraFiles);
            Controls.Add(lblExtraFiles);
            Controls.Add(btnBrowseWad);
            Controls.Add(txtWad);
            Controls.Add(btnBrowseIWad);
            Controls.Add(txtIWad);
            Controls.Add(cmbSourcePort);
            Controls.Add(btnFromArchive);
            Controls.Add(lblFromArchive);
            Controls.Add(lblArchiveSeparator);
            Controls.Add(txtFileName);
            Controls.Add(lblPlugins);
            Controls.Add(lblWad);
            Controls.Add(lblIWad);
            Controls.Add(lblSourcePort);
            Controls.Add(lblFileName);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "BatchCreatorForm";
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Add Batch File";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lblFileName;
        private Label lblSourcePort;
        private Label lblIWad;
        private Label lblWad;
        private Label lblPlugins;
        private Label lblExtraFiles;
        private TextBox txtExtraFiles;
        private TextBox txtFileName;
        private Button btnFromArchive;
        private Label lblFromArchive;
        private Label lblArchiveSeparator;
        private ComboBox cmbSourcePort;
        private TextBox txtIWad;
        private Button btnBrowseIWad;
        private TextBox txtWad;
        private Button btnBrowseWad;
        private TextBox txtPlugins;
        private Label lblImage;
        private TextBox txtImage;
        private Button btnBrowseImage;
        private Button btnCreate;
    }
}
