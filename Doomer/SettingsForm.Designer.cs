namespace Doomer
{
    partial class SettingsForm
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
            grpGZDoom = new GroupBox();
            lblGZDoomExe = new Label();
            txtGZDoomLocation = new TextBox();
            btnBrowseExe = new Button();
            lblGZDoomPlugins = new Label();
            txtGZDoomPlugins = new TextBox();
            grpBatchs = new GroupBox();
            lblBatchsLocation = new Label();
            txtBatchsLocation = new TextBox();
            btnBrowseBatchs = new Button();
            lblBatchsExtension = new Label();
            txtBatchsExtension = new TextBox();
            grpImages = new GroupBox();
            lblImagesLocation = new Label();
            txtImagesLocation = new TextBox();
            btnBrowseImages = new Button();
            lblImagesExtension = new Label();
            txtImagesExtension = new TextBox();
            grpIcons = new GroupBox();
            lblIconsWidth = new Label();
            nudWidth = new NumericUpDown();
            lblIconsHeight = new Label();
            nudHeight = new NumericUpDown();
            lblIconsPadding = new Label();
            nudPadding = new NumericUpDown();
            btnCancel = new Button();
            btnSave = new Button();
            grpGZDoom.SuspendLayout();
            grpBatchs.SuspendLayout();
            grpImages.SuspendLayout();
            grpIcons.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)nudWidth).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudHeight).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudPadding).BeginInit();
            SuspendLayout();
            //
            // grpGZDoom
            //
            grpGZDoom.Controls.Add(lblGZDoomExe);
            grpGZDoom.Controls.Add(txtGZDoomLocation);
            grpGZDoom.Controls.Add(btnBrowseExe);
            grpGZDoom.Controls.Add(lblGZDoomPlugins);
            grpGZDoom.Controls.Add(txtGZDoomPlugins);
            grpGZDoom.Location = new Point(8, 8);
            grpGZDoom.Name = "grpGZDoom";
            grpGZDoom.Size = new Size(524, 100);
            grpGZDoom.TabIndex = 0;
            grpGZDoom.TabStop = false;
            grpGZDoom.Text = "GZDoom";
            //
            // lblGZDoomExe
            //
            lblGZDoomExe.AutoSize = false;
            lblGZDoomExe.Location = new Point(8, 22);
            lblGZDoomExe.Name = "lblGZDoomExe";
            lblGZDoomExe.Size = new Size(85, 27);
            lblGZDoomExe.Text = "Executable";
            lblGZDoomExe.TextAlign = ContentAlignment.MiddleRight;
            //
            // txtGZDoomLocation
            //
            txtGZDoomLocation.Location = new Point(98, 22);
            txtGZDoomLocation.Name = "txtGZDoomLocation";
            txtGZDoomLocation.Size = new Size(308, 27);
            txtGZDoomLocation.TabIndex = 0;
            //
            // btnBrowseExe
            //
            btnBrowseExe.Location = new Point(412, 22);
            btnBrowseExe.Name = "btnBrowseExe";
            btnBrowseExe.Size = new Size(90, 27);
            btnBrowseExe.TabIndex = 1;
            btnBrowseExe.Text = "Browse...";
            btnBrowseExe.Click += BtnBrowseExe_Click;
            //
            // lblGZDoomPlugins
            //
            lblGZDoomPlugins.AutoSize = false;
            lblGZDoomPlugins.Location = new Point(8, 58);
            lblGZDoomPlugins.Name = "lblGZDoomPlugins";
            lblGZDoomPlugins.Size = new Size(85, 27);
            lblGZDoomPlugins.Text = "Plugins Folder";
            lblGZDoomPlugins.TextAlign = ContentAlignment.MiddleRight;
            //
            // txtGZDoomPlugins
            //
            txtGZDoomPlugins.Location = new Point(98, 58);
            txtGZDoomPlugins.Name = "txtGZDoomPlugins";
            txtGZDoomPlugins.Size = new Size(308, 27);
            txtGZDoomPlugins.TabIndex = 2;
            //
            // grpBatchs
            //
            grpBatchs.Controls.Add(lblBatchsLocation);
            grpBatchs.Controls.Add(txtBatchsLocation);
            grpBatchs.Controls.Add(btnBrowseBatchs);
            grpBatchs.Controls.Add(lblBatchsExtension);
            grpBatchs.Controls.Add(txtBatchsExtension);
            grpBatchs.Location = new Point(8, 116);
            grpBatchs.Name = "grpBatchs";
            grpBatchs.Size = new Size(524, 100);
            grpBatchs.TabIndex = 1;
            grpBatchs.TabStop = false;
            grpBatchs.Text = "Batch Files";
            //
            // lblBatchsLocation
            //
            lblBatchsLocation.AutoSize = false;
            lblBatchsLocation.Location = new Point(8, 22);
            lblBatchsLocation.Name = "lblBatchsLocation";
            lblBatchsLocation.Size = new Size(85, 27);
            lblBatchsLocation.Text = "Directory";
            lblBatchsLocation.TextAlign = ContentAlignment.MiddleRight;
            //
            // txtBatchsLocation
            //
            txtBatchsLocation.Location = new Point(98, 22);
            txtBatchsLocation.Name = "txtBatchsLocation";
            txtBatchsLocation.Size = new Size(308, 27);
            txtBatchsLocation.TabIndex = 3;
            //
            // btnBrowseBatchs
            //
            btnBrowseBatchs.Location = new Point(412, 22);
            btnBrowseBatchs.Name = "btnBrowseBatchs";
            btnBrowseBatchs.Size = new Size(90, 27);
            btnBrowseBatchs.TabIndex = 4;
            btnBrowseBatchs.Text = "Browse...";
            btnBrowseBatchs.Click += BtnBrowseBatchs_Click;
            //
            // lblBatchsExtension
            //
            lblBatchsExtension.AutoSize = false;
            lblBatchsExtension.Location = new Point(8, 58);
            lblBatchsExtension.Name = "lblBatchsExtension";
            lblBatchsExtension.Size = new Size(85, 27);
            lblBatchsExtension.Text = "Extension";
            lblBatchsExtension.TextAlign = ContentAlignment.MiddleRight;
            //
            // txtBatchsExtension
            //
            txtBatchsExtension.Location = new Point(98, 58);
            txtBatchsExtension.Name = "txtBatchsExtension";
            txtBatchsExtension.Size = new Size(100, 27);
            txtBatchsExtension.TabIndex = 5;
            //
            // grpImages
            //
            grpImages.Controls.Add(lblImagesLocation);
            grpImages.Controls.Add(txtImagesLocation);
            grpImages.Controls.Add(btnBrowseImages);
            grpImages.Controls.Add(lblImagesExtension);
            grpImages.Controls.Add(txtImagesExtension);
            grpImages.Location = new Point(8, 224);
            grpImages.Name = "grpImages";
            grpImages.Size = new Size(524, 100);
            grpImages.TabIndex = 2;
            grpImages.TabStop = false;
            grpImages.Text = "Images";
            //
            // lblImagesLocation
            //
            lblImagesLocation.AutoSize = false;
            lblImagesLocation.Location = new Point(8, 22);
            lblImagesLocation.Name = "lblImagesLocation";
            lblImagesLocation.Size = new Size(85, 27);
            lblImagesLocation.Text = "Directory";
            lblImagesLocation.TextAlign = ContentAlignment.MiddleRight;
            //
            // txtImagesLocation
            //
            txtImagesLocation.Location = new Point(98, 22);
            txtImagesLocation.Name = "txtImagesLocation";
            txtImagesLocation.Size = new Size(308, 27);
            txtImagesLocation.TabIndex = 6;
            //
            // btnBrowseImages
            //
            btnBrowseImages.Location = new Point(412, 22);
            btnBrowseImages.Name = "btnBrowseImages";
            btnBrowseImages.Size = new Size(90, 27);
            btnBrowseImages.TabIndex = 7;
            btnBrowseImages.Text = "Browse...";
            btnBrowseImages.Click += BtnBrowseImages_Click;
            //
            // lblImagesExtension
            //
            lblImagesExtension.AutoSize = false;
            lblImagesExtension.Location = new Point(8, 58);
            lblImagesExtension.Name = "lblImagesExtension";
            lblImagesExtension.Size = new Size(85, 27);
            lblImagesExtension.Text = "Extension";
            lblImagesExtension.TextAlign = ContentAlignment.MiddleRight;
            //
            // txtImagesExtension
            //
            txtImagesExtension.Location = new Point(98, 58);
            txtImagesExtension.Name = "txtImagesExtension";
            txtImagesExtension.Size = new Size(100, 27);
            txtImagesExtension.TabIndex = 8;
            //
            // grpIcons
            //
            grpIcons.Controls.Add(lblIconsWidth);
            grpIcons.Controls.Add(nudWidth);
            grpIcons.Controls.Add(lblIconsHeight);
            grpIcons.Controls.Add(nudHeight);
            grpIcons.Controls.Add(lblIconsPadding);
            grpIcons.Controls.Add(nudPadding);
            grpIcons.Location = new Point(8, 332);
            grpIcons.Name = "grpIcons";
            grpIcons.Size = new Size(524, 66);
            grpIcons.TabIndex = 3;
            grpIcons.TabStop = false;
            grpIcons.Text = "Icons";
            //
            // lblIconsWidth
            //
            lblIconsWidth.AutoSize = false;
            lblIconsWidth.Location = new Point(8, 24);
            lblIconsWidth.Name = "lblIconsWidth";
            lblIconsWidth.Size = new Size(48, 27);
            lblIconsWidth.Text = "Width";
            lblIconsWidth.TextAlign = ContentAlignment.MiddleRight;
            //
            // nudWidth
            //
            nudWidth.Location = new Point(60, 24);
            nudWidth.Maximum = new decimal(new int[] { 500, 0, 0, 0 });
            nudWidth.Minimum = new decimal(new int[] { 32, 0, 0, 0 });
            nudWidth.Name = "nudWidth";
            nudWidth.Size = new Size(70, 27);
            nudWidth.TabIndex = 9;
            nudWidth.Value = new decimal(new int[] { 120, 0, 0, 0 });
            //
            // lblIconsHeight
            //
            lblIconsHeight.AutoSize = false;
            lblIconsHeight.Location = new Point(152, 24);
            lblIconsHeight.Name = "lblIconsHeight";
            lblIconsHeight.Size = new Size(50, 27);
            lblIconsHeight.Text = "Height";
            lblIconsHeight.TextAlign = ContentAlignment.MiddleRight;
            //
            // nudHeight
            //
            nudHeight.Location = new Point(206, 24);
            nudHeight.Maximum = new decimal(new int[] { 500, 0, 0, 0 });
            nudHeight.Minimum = new decimal(new int[] { 32, 0, 0, 0 });
            nudHeight.Name = "nudHeight";
            nudHeight.Size = new Size(70, 27);
            nudHeight.TabIndex = 10;
            nudHeight.Value = new decimal(new int[] { 100, 0, 0, 0 });
            //
            // lblIconsPadding
            //
            lblIconsPadding.AutoSize = false;
            lblIconsPadding.Location = new Point(294, 24);
            lblIconsPadding.Name = "lblIconsPadding";
            lblIconsPadding.Size = new Size(55, 27);
            lblIconsPadding.Text = "Padding";
            lblIconsPadding.TextAlign = ContentAlignment.MiddleRight;
            //
            // nudPadding
            //
            nudPadding.Location = new Point(353, 24);
            nudPadding.Maximum = new decimal(new int[] { 50, 0, 0, 0 });
            nudPadding.Name = "nudPadding";
            nudPadding.Size = new Size(70, 27);
            nudPadding.TabIndex = 11;
            nudPadding.Value = new decimal(new int[] { 5, 0, 0, 0 });
            //
            // btnCancel
            //
            btnCancel.Location = new Point(346, 412);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(90, 30);
            btnCancel.TabIndex = 12;
            btnCancel.Text = "Cancel";
            btnCancel.Click += BtnCancel_Click;
            //
            // btnSave
            //
            btnSave.Location = new Point(442, 412);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(90, 30);
            btnSave.TabIndex = 13;
            btnSave.Text = "Save";
            btnSave.Click += BtnSave_Click;
            //
            // SettingsForm
            //
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(540, 454);
            Controls.Add(grpGZDoom);
            Controls.Add(grpBatchs);
            Controls.Add(grpImages);
            Controls.Add(grpIcons);
            Controls.Add(btnCancel);
            Controls.Add(btnSave);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "SettingsForm";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Settings";
            grpGZDoom.ResumeLayout(false);
            grpBatchs.ResumeLayout(false);
            grpImages.ResumeLayout(false);
            grpIcons.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)nudWidth).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudHeight).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudPadding).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private GroupBox grpGZDoom;
        private Label lblGZDoomExe;
        private TextBox txtGZDoomLocation;
        private Button btnBrowseExe;
        private Label lblGZDoomPlugins;
        private TextBox txtGZDoomPlugins;
        private GroupBox grpBatchs;
        private Label lblBatchsLocation;
        private TextBox txtBatchsLocation;
        private Button btnBrowseBatchs;
        private Label lblBatchsExtension;
        private TextBox txtBatchsExtension;
        private GroupBox grpImages;
        private Label lblImagesLocation;
        private TextBox txtImagesLocation;
        private Button btnBrowseImages;
        private Label lblImagesExtension;
        private TextBox txtImagesExtension;
        private GroupBox grpIcons;
        private Label lblIconsWidth;
        private NumericUpDown nudWidth;
        private Label lblIconsHeight;
        private NumericUpDown nudHeight;
        private Label lblIconsPadding;
        private NumericUpDown nudPadding;
        private Button btnCancel;
        private Button btnSave;
    }
}
