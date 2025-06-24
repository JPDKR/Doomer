namespace Doomer
{
    partial class Doomer
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Doomer));
            flowLayoutPanel1 = new FlowLayoutPanel();
            menuStrip1 = new MenuStrip();
            addBatchFileToolStripMenuItem = new ToolStripMenuItem();
            refreshListToolStripMenuItem = new ToolStripMenuItem();
            menuStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.AutoScroll = true;
            flowLayoutPanel1.BackColor = Color.Transparent;
            flowLayoutPanel1.Dock = DockStyle.Fill;
            flowLayoutPanel1.Location = new Point(0, 28);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Size = new Size(1038, 580);
            flowLayoutPanel1.TabIndex = 0;
            // 
            // menuStrip1
            // 
            menuStrip1.ImageScalingSize = new Size(20, 20);
            menuStrip1.Items.AddRange(new ToolStripItem[] { addBatchFileToolStripMenuItem, refreshListToolStripMenuItem });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new Size(1038, 28);
            menuStrip1.TabIndex = 1;
            menuStrip1.Text = "menuStrip1";
            // 
            // addBatchFileToolStripMenuItem
            // 
            addBatchFileToolStripMenuItem.Name = "addBatchFileToolStripMenuItem";
            addBatchFileToolStripMenuItem.Size = new Size(119, 24);
            addBatchFileToolStripMenuItem.Text = "Add Batch File";
            addBatchFileToolStripMenuItem.Click += addBatchFileToolStripMenuItem_Click;
            // 
            // refreshListToolStripMenuItem
            // 
            refreshListToolStripMenuItem.Name = "refreshListToolStripMenuItem";
            refreshListToolStripMenuItem.Size = new Size(98, 24);
            refreshListToolStripMenuItem.Text = "Refresh List";
            refreshListToolStripMenuItem.Click += refreshListToolStripMenuItem_Click;
            // 
            // Doomer
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
            ClientSize = new Size(1038, 608);
            Controls.Add(flowLayoutPanel1);
            Controls.Add(menuStrip1);
            Icon = (Icon)resources.GetObject("$this.Icon");
            MainMenuStrip = menuStrip1;
            Name = "Doomer";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Doomer";
            Load += MainForm_Load;
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private FlowLayoutPanel flowLayoutPanel1;
        private MenuStrip menuStrip1;
        private ToolStripMenuItem addBatchFileToolStripMenuItem;
        private ToolStripMenuItem refreshListToolStripMenuItem;
    }
}
