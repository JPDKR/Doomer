namespace Doomer
{
    partial class Doomer
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Doomer));
            flowLayoutPanel1 = new FlowLayoutPanel();
            menuStrip1 = new MenuStrip();
            addBatchFileToolStripMenuItem = new ToolStripMenuItem();
            refreshListToolStripMenuItem = new ToolStripMenuItem();
            settingsToolStripMenuItem = new ToolStripMenuItem();
            txtSearch = new ToolStripTextBox();
            statusStrip1 = new StatusStrip();
            lblStatus = new ToolStripStatusLabel();
            menuStrip1.SuspendLayout();
            statusStrip1.SuspendLayout();
            SuspendLayout();
            //
            // flowLayoutPanel1
            //
            flowLayoutPanel1.AutoScroll = true;
            flowLayoutPanel1.Dock = DockStyle.Fill;
            flowLayoutPanel1.Location = new Point(0, 27);
            flowLayoutPanel1.Margin = new Padding(3, 2, 3, 2);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Padding = new Padding(5);
            flowLayoutPanel1.Size = new Size(908, 407);
            flowLayoutPanel1.TabIndex = 0;
            //
            // menuStrip1
            //
            menuStrip1.ImageScalingSize = new Size(20, 20);
            menuStrip1.Items.AddRange(new ToolStripItem[] { addBatchFileToolStripMenuItem, refreshListToolStripMenuItem, settingsToolStripMenuItem, txtSearch });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Padding = new Padding(5, 2, 0, 2);
            menuStrip1.Size = new Size(908, 27);
            menuStrip1.TabIndex = 1;
            menuStrip1.Text = "menuStrip1";
            //
            // addBatchFileToolStripMenuItem
            //
            addBatchFileToolStripMenuItem.Name = "addBatchFileToolStripMenuItem";
            addBatchFileToolStripMenuItem.Size = new Size(95, 23);
            addBatchFileToolStripMenuItem.Text = "Add Batch File";
            addBatchFileToolStripMenuItem.Click += AddBatchFileToolStripMenuItem_Click;
            //
            // refreshListToolStripMenuItem
            //
            refreshListToolStripMenuItem.Name = "refreshListToolStripMenuItem";
            refreshListToolStripMenuItem.Size = new Size(79, 23);
            refreshListToolStripMenuItem.Text = "Refresh List";
            refreshListToolStripMenuItem.Click += RefreshListToolStripMenuItem_Click;
            //
            // settingsToolStripMenuItem
            //
            settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
            settingsToolStripMenuItem.Size = new Size(61, 23);
            settingsToolStripMenuItem.Text = "Settings";
            settingsToolStripMenuItem.Click += SettingsToolStripMenuItem_Click;
            //
            // txtSearch
            //
            txtSearch.Name = "txtSearch";
            txtSearch.Size = new Size(200, 23);
            txtSearch.TextChanged += SearchWad;
            //
            // statusStrip1
            //
            statusStrip1.Items.AddRange(new ToolStripItem[] { lblStatus });
            statusStrip1.Location = new Point(0, 434);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new Size(908, 22);
            statusStrip1.TabIndex = 2;
            //
            // lblStatus
            //
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(0, 17);
            lblStatus.Text = "";
            //
            // Doomer
            //
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(908, 456);
            MinimumSize = new Size(600, 350);
            Controls.Add(flowLayoutPanel1);
            Controls.Add(statusStrip1);
            Controls.Add(menuStrip1);
            Icon = (Icon)resources.GetObject("$this.Icon");
            MainMenuStrip = menuStrip1;
            Margin = new Padding(3, 2, 3, 2);
            Name = "Doomer";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Doomer";
            Load += MainForm_Load;
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private FlowLayoutPanel flowLayoutPanel1;
        private MenuStrip menuStrip1;
        private ToolStripMenuItem addBatchFileToolStripMenuItem;
        private ToolStripMenuItem refreshListToolStripMenuItem;
        private ToolStripMenuItem settingsToolStripMenuItem;
        private ToolStripTextBox txtSearch;
        private StatusStrip statusStrip1;
        private ToolStripStatusLabel lblStatus;
    }
}
