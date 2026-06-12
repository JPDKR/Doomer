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
            lblIWad = new Label();
            lblWad = new Label();
            lblPlugins = new Label();
            txtFileName = new TextBox();
            txtIWad = new TextBox();
            txtWad = new TextBox();
            txtPlugins = new TextBox();
            btnCreate = new Button();
            SuspendLayout();
            //
            // lblFileName
            //
            lblFileName.AutoSize = false;
            lblFileName.Location = new Point(8, 16);
            lblFileName.Name = "lblFileName";
            lblFileName.Size = new Size(80, 27);
            lblFileName.TabIndex = 0;
            lblFileName.Text = "File Name";
            lblFileName.TextAlign = ContentAlignment.MiddleRight;
            //
            // lblIWad
            //
            lblIWad.AutoSize = false;
            lblIWad.Location = new Point(8, 52);
            lblIWad.Name = "lblIWad";
            lblIWad.Size = new Size(80, 27);
            lblIWad.TabIndex = 1;
            lblIWad.Text = "IWAD";
            lblIWad.TextAlign = ContentAlignment.MiddleRight;
            //
            // lblWad
            //
            lblWad.AutoSize = false;
            lblWad.Location = new Point(8, 88);
            lblWad.Name = "lblWad";
            lblWad.Size = new Size(80, 27);
            lblWad.TabIndex = 2;
            lblWad.Text = "WAD";
            lblWad.TextAlign = ContentAlignment.MiddleRight;
            //
            // lblPlugins
            //
            lblPlugins.AutoSize = false;
            lblPlugins.Location = new Point(8, 124);
            lblPlugins.Name = "lblPlugins";
            lblPlugins.Size = new Size(80, 27);
            lblPlugins.TabIndex = 3;
            lblPlugins.Text = "Plugins";
            lblPlugins.TextAlign = ContentAlignment.MiddleRight;
            //
            // txtFileName
            //
            txtFileName.Location = new Point(96, 16);
            txtFileName.Name = "txtFileName";
            txtFileName.Size = new Size(340, 27);
            txtFileName.TabIndex = 4;
            //
            // txtIWad
            //
            txtIWad.Location = new Point(96, 52);
            txtIWad.Name = "txtIWad";
            txtIWad.Size = new Size(340, 27);
            txtIWad.TabIndex = 5;
            //
            // txtWad
            //
            txtWad.Location = new Point(96, 88);
            txtWad.Name = "txtWad";
            txtWad.Size = new Size(340, 27);
            txtWad.TabIndex = 6;
            //
            // txtPlugins
            //
            txtPlugins.Location = new Point(96, 124);
            txtPlugins.Name = "txtPlugins";
            txtPlugins.Size = new Size(340, 27);
            txtPlugins.TabIndex = 7;
            //
            // btnCreate
            //
            btnCreate.Location = new Point(242, 168);
            btnCreate.Name = "btnCreate";
            btnCreate.Size = new Size(100, 30);
            btnCreate.TabIndex = 8;
            btnCreate.Text = "Create";
            btnCreate.UseVisualStyleBackColor = true;
            btnCreate.Click += BtnCreate_Click;
            //
            // BatchCreatorForm
            //
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(452, 214);
            Controls.Add(btnCreate);
            Controls.Add(txtPlugins);
            Controls.Add(txtWad);
            Controls.Add(txtIWad);
            Controls.Add(txtFileName);
            Controls.Add(lblPlugins);
            Controls.Add(lblWad);
            Controls.Add(lblIWad);
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
        private Label lblIWad;
        private Label lblWad;
        private Label lblPlugins;
        private TextBox txtFileName;
        private TextBox txtIWad;
        private TextBox txtWad;
        private TextBox txtPlugins;
        private Button btnCreate;
    }
}
