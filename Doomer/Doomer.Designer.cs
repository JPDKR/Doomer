namespace Doomer
{
    partial class Doomer
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                components?.Dispose();
                _wadButtonFont?.Dispose();
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
            sortToolStripMenuItem = new ToolStripMenuItem();
            sortByNameToolStripMenuItem = new ToolStripMenuItem();
            sortByLastPlayedToolStripMenuItem = new ToolStripMenuItem();
            favoritesOnlyToolStripMenuItem = new ToolStripMenuItem();
            settingsToolStripMenuItem = new ToolStripMenuItem();
            txtSearch = new ToolStripTextBox();
            statusStrip1 = new StatusStrip();
            lblStatus = new ToolStripStatusLabel();
            loadingPanel = new TableLayoutPanel();
            lblLoading = new Label();
            progressLoading = new ProgressBar();
            lblEmpty = new Label();
            lblVersion = new ToolStripStatusLabel();
            menuStrip1.SuspendLayout();
            statusStrip1.SuspendLayout();
            loadingPanel.SuspendLayout();
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
            menuStrip1.Items.AddRange(new ToolStripItem[] { addBatchFileToolStripMenuItem, refreshListToolStripMenuItem, sortToolStripMenuItem, favoritesOnlyToolStripMenuItem, settingsToolStripMenuItem, txtSearch });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.ShowItemToolTips = true;
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
            // sortToolStripMenuItem
            //
            sortToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { sortByNameToolStripMenuItem, sortByLastPlayedToolStripMenuItem });
            sortToolStripMenuItem.Name = "sortToolStripMenuItem";
            sortToolStripMenuItem.Size = new Size(44, 23);
            sortToolStripMenuItem.Text = "Sort";
            //
            // sortByNameToolStripMenuItem
            //
            sortByNameToolStripMenuItem.Name = "sortByNameToolStripMenuItem";
            sortByNameToolStripMenuItem.Size = new Size(180, 24);
            sortByNameToolStripMenuItem.Text = "By name";
            sortByNameToolStripMenuItem.Click += SortByNameToolStripMenuItem_Click;
            //
            // sortByLastPlayedToolStripMenuItem
            //
            sortByLastPlayedToolStripMenuItem.Name = "sortByLastPlayedToolStripMenuItem";
            sortByLastPlayedToolStripMenuItem.Size = new Size(180, 24);
            sortByLastPlayedToolStripMenuItem.Text = "By last played";
            sortByLastPlayedToolStripMenuItem.Click += SortByLastPlayedToolStripMenuItem_Click;
            //
            // favoritesOnlyToolStripMenuItem
            //
            favoritesOnlyToolStripMenuItem.Name = "favoritesOnlyToolStripMenuItem";
            favoritesOnlyToolStripMenuItem.Size = new Size(84, 23);
            favoritesOnlyToolStripMenuItem.Text = "☆ Favorites";
            favoritesOnlyToolStripMenuItem.Click += FavoritesOnlyToolStripMenuItem_Click;
            //
            // settingsToolStripMenuItem
            //
            settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
            settingsToolStripMenuItem.AccessibleName = "Settings";
            settingsToolStripMenuItem.Font = new Font("Segoe MDL2 Assets", 11F);
            settingsToolStripMenuItem.Size = new Size(32, 23);
            settingsToolStripMenuItem.Text = "\uE713"; // Gear glyph
            settingsToolStripMenuItem.ToolTipText = "Settings";
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
            statusStrip1.Items.AddRange(new ToolStripItem[] { lblStatus, lblVersion });
            statusStrip1.Location = new Point(0, 434);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new Size(908, 22);
            statusStrip1.TabIndex = 2;
            //
            // lblStatus
            //
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(0, 17);
            lblStatus.Spring = true;
            lblStatus.Text = "";
            lblStatus.TextAlign = ContentAlignment.MiddleLeft;
            //
            // lblVersion
            //
            lblVersion.Name = "lblVersion";
            lblVersion.Size = new Size(0, 17);
            lblVersion.Text = "";
            //
            // loadingPanel
            //
            loadingPanel.ColumnCount = 1;
            loadingPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            loadingPanel.Controls.Add(lblLoading, 0, 1);
            loadingPanel.Controls.Add(progressLoading, 0, 2);
            loadingPanel.Dock = DockStyle.Fill;
            loadingPanel.Location = new Point(0, 27);
            loadingPanel.Name = "loadingPanel";
            loadingPanel.RowCount = 4;
            loadingPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            loadingPanel.RowStyles.Add(new RowStyle());
            loadingPanel.RowStyles.Add(new RowStyle());
            loadingPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            loadingPanel.Size = new Size(908, 407);
            loadingPanel.TabIndex = 3;
            loadingPanel.Visible = false;
            //
            // lblLoading
            //
            lblLoading.Anchor = AnchorStyles.None;
            lblLoading.AutoSize = true;
            lblLoading.Font = new Font("Segoe UI", 11F);
            lblLoading.Margin = new Padding(3, 0, 3, 8);
            lblLoading.Name = "lblLoading";
            lblLoading.Text = "Loading WADs...";
            //
            // progressLoading
            //
            progressLoading.Anchor = AnchorStyles.None;
            progressLoading.MarqueeAnimationSpeed = 30;
            progressLoading.Name = "progressLoading";
            progressLoading.Size = new Size(220, 6);
            progressLoading.Style = ProgressBarStyle.Marquee;
            //
            // lblEmpty
            //
            lblEmpty.Dock = DockStyle.Fill;
            lblEmpty.Font = new Font("Segoe UI", 11F);
            lblEmpty.Name = "lblEmpty";
            lblEmpty.Padding = new Padding(20);
            lblEmpty.TextAlign = ContentAlignment.MiddleCenter;
            lblEmpty.Visible = false;
            //
            // Doomer
            //
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(908, 456);
            MinimumSize = new Size(600, 350);
            Controls.Add(loadingPanel);
            Controls.Add(lblEmpty);
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
            loadingPanel.ResumeLayout(false);
            loadingPanel.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private FlowLayoutPanel flowLayoutPanel1;
        private MenuStrip menuStrip1;
        private ToolStripMenuItem addBatchFileToolStripMenuItem;
        private ToolStripMenuItem refreshListToolStripMenuItem;
        private ToolStripMenuItem sortToolStripMenuItem;
        private ToolStripMenuItem sortByNameToolStripMenuItem;
        private ToolStripMenuItem sortByLastPlayedToolStripMenuItem;
        private ToolStripMenuItem favoritesOnlyToolStripMenuItem;
        private ToolStripMenuItem settingsToolStripMenuItem;
        private ToolStripTextBox txtSearch;
        private StatusStrip statusStrip1;
        private ToolStripStatusLabel lblStatus;
        private TableLayoutPanel loadingPanel;
        private Label lblLoading;
        private ProgressBar progressLoading;
        private Label lblEmpty;
        private ToolStripStatusLabel lblVersion;
    }
}
