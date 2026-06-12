using Doomer.Options;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace Doomer
{
    public partial class BatchCreatorForm : Form
    {
        private readonly GZDoomSettings _gzdoomSettings;

        public BatchCreatorForm()
        {
            InitializeComponent();

            var config = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json")
                .Build();

            _gzdoomSettings = config.GetSection("GZDoom").Get<GZDoomSettings>()!;

            ApplyDarkTheme();
        }

        private void ApplyDarkTheme()
        {
            BackColor = Color.FromArgb(18, 18, 18);
            ForeColor = Color.FromArgb(220, 220, 220);

            foreach (Control ctrl in Controls)
            {
                switch (ctrl)
                {
                    case TextBox tb:
                        tb.BackColor = Color.FromArgb(38, 38, 38);
                        tb.ForeColor = Color.FromArgb(220, 220, 220);
                        tb.BorderStyle = BorderStyle.FixedSingle;
                        break;
                    case Label lbl:
                        lbl.ForeColor = Color.FromArgb(170, 170, 170);
                        break;
                    case Button btn:
                        btn.FlatStyle = FlatStyle.Flat;
                        btn.BackColor = Color.FromArgb(160, 15, 15);
                        btn.ForeColor = Color.White;
                        btn.FlatAppearance.BorderColor = Color.FromArgb(200, 25, 25);
                        btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(200, 25, 25);
                        btn.Cursor = Cursors.Hand;
                        break;
                }
            }

            txtFileName.PlaceholderText = "ej: my-doom-mod";
            txtIWad.PlaceholderText = "ej: doom2.wad";
            txtWad.PlaceholderText = "ej: brutal-doom";
            txtPlugins.PlaceholderText = "ej: smoothed (opcional)";
        }

        private void BtnCreate_Click(object sender, EventArgs e)
        {
            var iwad = txtIWad.Text.Trim();
            var wadFile = txtWad.Text.Trim();
            var plugins = txtPlugins.Text.Trim();
            var fileName = txtFileName.Text.Trim();

            if (string.IsNullOrWhiteSpace(iwad) || string.IsNullOrWhiteSpace(wadFile) || string.IsNullOrWhiteSpace(fileName))
            {
                MessageBox.Show("Insert the IWAD, WAD and the bath file.");
                return;
            }

            var command = $"{_gzdoomSettings.Location} -iwad \"{iwad}\" -file \"{wadFile}.wad\"";

            if (!string.IsNullOrWhiteSpace(plugins))
            {
                command += $" \"{plugins}/{plugins}\"";
            }

            var path = Path.Combine($"{_gzdoomSettings.Batchs.Location}\\{fileName}.bat");

            try
            {
                File.WriteAllText(path, command);
                MessageBox.Show("Batch creado exitosamente.", "Batch creation", MessageBoxButtons.OK, MessageBoxIcon.Information);
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al guardar el batch: " + ex.Message, "Batch creation", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
