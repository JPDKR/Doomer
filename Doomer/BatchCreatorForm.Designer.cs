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
            lstExtraFiles = new ListBox();
            btnAddExtraFile = new Button();
            btnRemoveExtraFile = new Button();
            txtFileName = new TextBox();
            btnFromArchive = new Button();
            lblFromArchive = new Label();
            lblArchiveSeparator = new Label();
            cmbSourcePort = new ComboBox();
            cmbIWad = new ComboBox();
            btnBrowseIWad = new Button();
            txtWad = new TextBox();
            btnBrowseWad = new Button();
            lstPlugins = new ListBox();
            btnAddPlugin = new Button();
            btnRemovePlugin = new Button();
            txtPluginsUnsupported = new TextBox();
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
            // cmbIWad
            //
            cmbIWad.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            cmbIWad.AutoCompleteSource = AutoCompleteSource.ListItems;
            cmbIWad.Location = new Point(106, 150);
            cmbIWad.Name = "cmbIWad";
            cmbIWad.Size = new Size(296, 28);
            cmbIWad.TabIndex = 7;
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
            // lstExtraFiles
            //
            lstExtraFiles.AllowDrop = true;
            lstExtraFiles.HorizontalScrollbar = true;
            lstExtraFiles.IntegralHeight = false;
            lstExtraFiles.Location = new Point(106, 222);
            lstExtraFiles.Name = "lstExtraFiles";
            lstExtraFiles.SelectionMode = SelectionMode.MultiExtended;
            lstExtraFiles.Size = new Size(296, 63);
            lstExtraFiles.TabIndex = 11;
            lstExtraFiles.SelectedIndexChanged += LstExtraFiles_SelectedIndexChanged;
            lstExtraFiles.KeyDown += LstExtraFiles_KeyDown;
            lstExtraFiles.DragEnter += LstExtraFiles_DragEnter;
            lstExtraFiles.DragDrop += LstExtraFiles_DragDrop;
            //
            // btnAddExtraFile
            //
            btnAddExtraFile.Location = new Point(408, 222);
            btnAddExtraFile.Name = "btnAddExtraFile";
            btnAddExtraFile.Size = new Size(78, 27);
            btnAddExtraFile.TabIndex = 21;
            btnAddExtraFile.Text = "Add...";
            btnAddExtraFile.Click += BtnAddExtraFile_Click;
            //
            // btnRemoveExtraFile
            //
            btnRemoveExtraFile.Enabled = false;
            btnRemoveExtraFile.Location = new Point(408, 255);
            btnRemoveExtraFile.Name = "btnRemoveExtraFile";
            btnRemoveExtraFile.Size = new Size(78, 27);
            btnRemoveExtraFile.TabIndex = 22;
            btnRemoveExtraFile.Text = "Remove";
            btnRemoveExtraFile.Click += BtnRemoveExtraFile_Click;
            //
            // lstPlugins
            //
            lstPlugins.HorizontalScrollbar = true;
            lstPlugins.IntegralHeight = false;
            lstPlugins.Location = new Point(106, 294);
            lstPlugins.Name = "lstPlugins";
            lstPlugins.SelectionMode = SelectionMode.MultiExtended;
            lstPlugins.Size = new Size(296, 63);
            lstPlugins.TabIndex = 12;
            lstPlugins.SelectedIndexChanged += LstPlugins_SelectedIndexChanged;
            lstPlugins.KeyDown += LstPlugins_KeyDown;
            //
            // btnAddPlugin
            //
            btnAddPlugin.Location = new Point(408, 294);
            btnAddPlugin.Name = "btnAddPlugin";
            btnAddPlugin.Size = new Size(78, 27);
            btnAddPlugin.TabIndex = 24;
            btnAddPlugin.Text = "Add...";
            btnAddPlugin.Click += BtnAddPlugin_Click;
            //
            // btnRemovePlugin
            //
            btnRemovePlugin.Enabled = false;
            btnRemovePlugin.Location = new Point(408, 327);
            btnRemovePlugin.Name = "btnRemovePlugin";
            btnRemovePlugin.Size = new Size(78, 27);
            btnRemovePlugin.TabIndex = 25;
            btnRemovePlugin.Text = "Remove";
            btnRemovePlugin.Click += BtnRemovePlugin_Click;
            //
            // txtPluginsUnsupported
            //
            txtPluginsUnsupported.Enabled = false;
            txtPluginsUnsupported.Location = new Point(106, 294);
            txtPluginsUnsupported.Name = "txtPluginsUnsupported";
            txtPluginsUnsupported.Size = new Size(380, 27);
            txtPluginsUnsupported.TabIndex = 23;
            txtPluginsUnsupported.Visible = false;
            //
            // lblImage
            //
            lblImage.AutoSize = false;
            lblImage.Location = new Point(8, 366);
            lblImage.Name = "lblImage";
            lblImage.Size = new Size(90, 27);
            lblImage.TabIndex = 13;
            lblImage.Text = "Image";
            lblImage.TextAlign = ContentAlignment.MiddleRight;
            //
            // txtImage
            //
            txtImage.Location = new Point(106, 366);
            txtImage.Name = "txtImage";
            txtImage.Size = new Size(296, 27);
            txtImage.TabIndex = 14;
            txtImage.KeyDown += TxtImage_KeyDown;
            //
            // btnBrowseImage
            //
            btnBrowseImage.Location = new Point(408, 366);
            btnBrowseImage.Name = "btnBrowseImage";
            btnBrowseImage.Size = new Size(78, 27);
            btnBrowseImage.TabIndex = 15;
            btnBrowseImage.Text = "Browse...";
            btnBrowseImage.Click += BtnBrowseImage_Click;
            //
            // btnCreate
            //
            btnCreate.Location = new Point(246, 410);
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
            ClientSize = new Size(502, 456);
            Controls.Add(btnCreate);
            Controls.Add(btnBrowseImage);
            Controls.Add(txtImage);
            Controls.Add(lblImage);
            Controls.Add(btnRemovePlugin);
            Controls.Add(btnAddPlugin);
            Controls.Add(lstPlugins);
            Controls.Add(txtPluginsUnsupported);
            Controls.Add(btnRemoveExtraFile);
            Controls.Add(btnAddExtraFile);
            Controls.Add(lstExtraFiles);
            Controls.Add(lblExtraFiles);
            Controls.Add(btnBrowseWad);
            Controls.Add(txtWad);
            Controls.Add(btnBrowseIWad);
            Controls.Add(cmbIWad);
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
        private ListBox lstExtraFiles;
        private Button btnAddExtraFile;
        private Button btnRemoveExtraFile;
        private TextBox txtFileName;
        private Button btnFromArchive;
        private Label lblFromArchive;
        private Label lblArchiveSeparator;
        private ComboBox cmbSourcePort;
        private ComboBox cmbIWad;
        private Button btnBrowseIWad;
        private TextBox txtWad;
        private Button btnBrowseWad;
        private ListBox lstPlugins;
        private Button btnAddPlugin;
        private Button btnRemovePlugin;
        private TextBox txtPluginsUnsupported;
        private Label lblImage;
        private TextBox txtImage;
        private Button btnBrowseImage;
        private Button btnCreate;
    }
}
