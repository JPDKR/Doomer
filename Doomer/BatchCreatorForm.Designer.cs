namespace Doomer
{
    partial class BatchCreatorForm
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
            lblFileName.AutoSize = true;
            lblFileName.Location = new Point(12, 15);
            lblFileName.Name = "lblFileName";
            lblFileName.Size = new Size(76, 20);
            lblFileName.TabIndex = 0;
            lblFileName.Text = "File Name";
            // 
            // lblIWad
            // 
            lblIWad.AutoSize = true;
            lblIWad.Location = new Point(41, 48);
            lblIWad.Name = "lblIWad";
            lblIWad.Size = new Size(47, 20);
            lblIWad.TabIndex = 1;
            lblIWad.Text = "IWAD";
            // 
            // lblWad
            // 
            lblWad.AutoSize = true;
            lblWad.Location = new Point(45, 81);
            lblWad.Name = "lblWad";
            lblWad.Size = new Size(43, 20);
            lblWad.TabIndex = 2;
            lblWad.Text = "WAD";
            // 
            // lblPlugins
            // 
            lblPlugins.AutoSize = true;
            lblPlugins.Location = new Point(32, 114);
            lblPlugins.Name = "lblPlugins";
            lblPlugins.Size = new Size(56, 20);
            lblPlugins.TabIndex = 3;
            lblPlugins.Text = "Plugins";
            // 
            // txtFileName
            // 
            txtFileName.Location = new Point(94, 12);
            txtFileName.Name = "txtFileName";
            txtFileName.Size = new Size(322, 27);
            txtFileName.TabIndex = 4;
            // 
            // txtIWad
            // 
            txtIWad.Location = new Point(94, 45);
            txtIWad.Name = "txtIWad";
            txtIWad.Size = new Size(322, 27);
            txtIWad.TabIndex = 5;
            // 
            // txtWad
            // 
            txtWad.Location = new Point(94, 78);
            txtWad.Name = "txtWad";
            txtWad.Size = new Size(322, 27);
            txtWad.TabIndex = 6;
            // 
            // txtPlugins
            // 
            txtPlugins.Location = new Point(94, 111);
            txtPlugins.Name = "txtPlugins";
            txtPlugins.Size = new Size(322, 27);
            txtPlugins.TabIndex = 7;
            // 
            // btnCreate
            // 
            btnCreate.Location = new Point(322, 159);
            btnCreate.Name = "btnCreate";
            btnCreate.Size = new Size(94, 29);
            btnCreate.TabIndex = 8;
            btnCreate.Text = "Create";
            btnCreate.UseVisualStyleBackColor = true;
            btnCreate.Click += btnCreate_Click;
            // 
            // BatchCreatorForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(428, 200);
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