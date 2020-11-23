using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AquaModelTool
{
    public partial class AquaModelTool : Form
    {
        public AquaUICommon aquaUI = new AquaUICommon();
        public string currentFile;
        public AquaModelTool()
        {
            InitializeComponent();
            this.DragEnter += new DragEventHandler(AquaUI_DragEnter);
            this.DragDrop += new DragEventHandler(AquaUI_DragDrop);
        }

        private void quitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AquaUIOpenFile();
        }
        private void AquaUI_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Copy;
        }

        private void AquaUI_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            AquaUIOpenFile(files[0]);
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Model saving
            SaveFileDialog saveFileDialog = new SaveFileDialog()
            {
                Title = "Save character file",
                Filter = "PSO2 Model (*.aqp)|*.aqp|PSO2 Terrain (*.trp)|*.trp"
            };
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                aquaUI.setAllTransparent(((ModelEditor)filePanel.Controls[0]).GetAllTransparentChecked());
                aquaUI.toVTBF(saveFileDialog.FileName);
                currentFile = saveFileDialog.FileName;
                AquaUIOpenFile(saveFileDialog.FileName);
                this.Text = "Aqua Model Tool - " + Path.GetFileName(currentFile);
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Model saving
            aquaUI.setAllTransparent(((ModelEditor)filePanel.Controls[0]).GetAllTransparentChecked());
            aquaUI.toVTBF(currentFile);
            AquaUIOpenFile(currentFile);
            this.Text = "Aqua Model Tool - " + Path.GetFileName(currentFile);
        }
        
        public void AquaUIOpenFile(string str = null)
        {
            string file = aquaUI.openFile(str);
            if (file != null)
            {
                UserControl control;
                currentFile = file;
                this.Text = "Aqua Model Tool - " + Path.GetFileName(currentFile);

                filePanel.Controls.Clear();
                switch (Path.GetExtension(file))
                {
                    case ".aqp":
                    case ".aqo":
                    case ".trp":
                    case ".tro":
                        control = new ModelEditor(aquaUI.aqua.aquaModels[0]);
                        break;
                    default:
                        MessageBox.Show("Invalid File");
                        return;
                }
                filePanel.Controls.Add(control);
                control.Dock = DockStyle.Fill;
                control.BringToFront();
            }
        }
    }
}
