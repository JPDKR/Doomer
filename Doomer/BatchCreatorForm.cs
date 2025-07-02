using Doomer.Options;
using Microsoft.Extensions.Configuration;

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

            // Armar el comando
            var command = $"{_gzdoomSettings.Location} -iwad \"{iwad}\" -file \"{wadFile}.wad\"";

            if (!string.IsNullOrWhiteSpace(plugins))
            {
                command += $" \"{plugins}/{plugins}\"";
            }

            // Guardar el archivo .bat
            var path = Path.Combine($"{_gzdoomSettings.Batchs.Location}\\{fileName}.bat");

            try
            {
                File.WriteAllText(path, command);
                MessageBox.Show("Batch creation", "Batch created succesfully.", MessageBoxButtons.OK, MessageBoxIcon.Information);
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Batch creation", "Error al guardar el batch: " + ex.Message, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}
