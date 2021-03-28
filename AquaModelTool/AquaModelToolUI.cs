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
        public bool isNIFL = false;
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
                Filter = "PSO2 VTBF Model (*.aqp)|*.aqp|PSO2 VTBF Terrain (*.trp)|*.trp|PSO2 NIFL Model (*.aqp)|*.aqp|PSO2 NIFL Terrain (*.trp)|*.trp"
            };
            switch (Path.GetExtension(currentFile))
            {
                case ".aqp":
                case ".aqo":
                    saveFileDialog.FilterIndex = 1;
                    break;
                case ".trp":
                case ".tro":
                    saveFileDialog.FilterIndex = 2;
                    break;
                default:
                    saveFileDialog.FilterIndex = 1;
                    return;
            }
            if (isNIFL)
            {
                saveFileDialog.FilterIndex += 2;
            }
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                aquaUI.setAllTransparent(((ModelEditor)filePanel.Controls[0]).GetAllTransparentChecked());
                switch (saveFileDialog.FilterIndex)
                {
                    case 1:
                    case 2:
                        aquaUI.toVTBFModel(saveFileDialog.FileName);
                        break;
                    case 3:
                    case 4:
                        aquaUI.toNIFLModel(saveFileDialog.FileName);
                        break;
                }
                currentFile = saveFileDialog.FileName;
                AquaUIOpenFile(saveFileDialog.FileName);
                this.Text = "Aqua Model Tool - " + Path.GetFileName(currentFile);
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(currentFile != null)
            {
                //Model saving
                aquaUI.setAllTransparent(((ModelEditor)filePanel.Controls[0]).GetAllTransparentChecked());
                switch (isNIFL)
                {
                    case true:
                        aquaUI.toNIFLModel(currentFile);
                        break;
                    case false:
                        aquaUI.toVTBFModel(currentFile);
                        break;
                }
                AquaUIOpenFile(currentFile);
                this.Text = "Aqua Model Tool - " + Path.GetFileName(currentFile);
            }
        }
        
        public void AquaUIOpenFile(string str = null)
        {
            string file = aquaUI.confirmFile(str);
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
                        aquaUI.aqua.aquaModels.Clear();
                        aquaUI.aqua.aquaMotions.Clear();
                        aquaUI.aqua.ReadModel(file); 
                        //aquaUI.aqua.aquaModels[0].models[0].splitVSETPerMesh();
#if DEBUG
                        var test = aquaUI.aqua.aquaModels[0].models[0];
                        test = aquaUI.aqua.aquaModels[0].models[0];
#endif
                        control = new ModelEditor(aquaUI.aqua.aquaModels[0]);
                        if(aquaUI.aqua.aquaModels[0].models[0].nifl.magic != 0)
                        {
                            isNIFL = true;
                        } else
                        {
                            isNIFL = false;
                        }
                        setModelOptions(true);
                        break;
                    case ".aqm":
                    case ".aqv":
                    case ".aqc":
                    case ".aqw":
                    case ".trm":
                    case ".trv":
                    case ".trw":
                        aquaUI.aqua.aquaModels.Clear();
                        aquaUI.aqua.aquaMotions.Clear();
                        aquaUI.aqua.ReadMotion(file);
                        control = new AnimationEditor(aquaUI.aqua.aquaMotions[0]);
                        if (aquaUI.aqua.aquaMotions[0].anims[0].nifl.magic != 0)
                        {
                            isNIFL = true;
                        }
                        else
                        {
                            isNIFL = false;
                        }
                        setModelOptions(false);
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

        private void setModelOptions(bool setting)
        {
            averageNormalsOnSharedPositionVerticesToolStripMenuItem.Enabled = setting;
        }

        private void averageNormalsOnSharedPositionVerticesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            aquaUI.averageNormals();
            MessageBox.Show("Normal averaging complete!");
        }

        private void parseVTBFToTextToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog()
            {
                Title = "Select a VTBF PSO2 file",
                Filter = "All Files|*"
            };
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                AquaModelLibrary.AquaUtil.AnalyzeVTBF(openFileDialog.FileName);
            }
            
        }

        private void readCMXToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog()
            {
                Title = "Select a cmx file",
                Filter = "Character Making IndeX (*.cmx) Files|*.cmx"
            };
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                aquaUI.aqua.ReadCMX(openFileDialog.FileName);
            }
        }
    }
}
