using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Doomer
{
    public partial class BatchCreatorForm : Form
    {
        private readonly string gzdoomLocation;
        private readonly string batchsLocation;
        private readonly string pluginsLocation;

        public BatchCreatorForm()
        {
            InitializeComponent();

            var config = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json")
                .Build();

            gzdoomLocation = config["GZDoom:Location"]!;
            batchsLocation = config["GZDoom:Batchs:Location"]!;
            pluginsLocation= config["GZDoom:Plugins"]!;
        }

        private void btnCreate_Click(object sender, EventArgs e)
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
            var command = $"{gzdoomLocation} -iwad \"{iwad}\" -file \"{wadFile}\"";

            if (!string.IsNullOrWhiteSpace(plugins))
            {
                command += $" \"{plugins}/{plugins}\"";
            }

            // Guardar el archivo .bat
            var path = Path.Combine($"{batchsLocation}\\{fileName}.bat");

            try
            {
                File.WriteAllText(path, command);
                MessageBox.Show("Batch creado correctamente.");
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al guardar el batch: " + ex.Message);
            }
        }
    }
}
