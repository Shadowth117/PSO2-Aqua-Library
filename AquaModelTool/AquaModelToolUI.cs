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
using AquaModelLibrary;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace AquaModelTool
{
    public partial class AquaModelTool : Form
    {
        public AquaUICommon aquaUI = new AquaUICommon();
        public List<string> modelExtensions = new List<string>() { ".aqp", ".aqo", ".trp", ".tro" };
        public List<string> effectExtensions = new List<string>() { ".aqe" };
        public List<string> motionExtensions = new List<string>() { ".aqm", ".aqv", ".aqc", ".aqw", ".trm", ".trv", ".trw" };
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
            string ext = Path.GetExtension(currentFile);
            SaveFileDialog saveFileDialog;
            //Model saving
            if (modelExtensions.Contains(ext))
            {
                saveFileDialog = new SaveFileDialog()
                {
                    Title = "Save model file",
                    Filter = "PSO2 VTBF Model (*.aqp)|*.aqp|PSO2 VTBF Terrain (*.trp)|*.trp|PSO2 Classic NIFL Model (*.aqp)|*.aqp|PSO2 Classic NIFL Terrain (*.trp)|*.trp"
                };
                switch (ext)
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
            //Anim Saving
            else if (motionExtensions.Contains(ext))
            {
                saveFileDialog = new SaveFileDialog()
                {
                    Title = "Save model file",
                    Filter = $"PSO2 VTBF Motion (*{ext})|*{ext}|PSO2 NIFL Motion (*{ext})|*{ext}"
                };
                if (isNIFL)
                {
                    saveFileDialog.FilterIndex += 1;
                }
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    switch (saveFileDialog.FilterIndex)
                    {
                        case 1:
                            aquaUI.aqua.WriteVTBFMotion(saveFileDialog.FileName);
                            break;
                        case 2:
                            aquaUI.aqua.WriteNIFLMotion(saveFileDialog.FileName);
                            break;
                    }
                    currentFile = saveFileDialog.FileName;
                    AquaUIOpenFile(saveFileDialog.FileName);
                    this.Text = "Aqua Model Tool - " + Path.GetFileName(currentFile);
                }

            } else if (effectExtensions.Contains(ext))
            {
                saveFileDialog = new SaveFileDialog()
                {
                    Title = "Save model file",
                    Filter = $"PSO2 Classic NIFL Effect (*{ext})|*{ext}"
                };
                /*
                if (isNIFL)
                {
                    saveFileDialog.FilterIndex += 1;
                }*/
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    switch (saveFileDialog.FilterIndex)
                    {
                        case 1:
                            aquaUI.aqua.WriteClassicNIFLEffect(saveFileDialog.FileName);
                            break;
                    }
                    currentFile = saveFileDialog.FileName;
                    AquaUIOpenFile(saveFileDialog.FileName);
                    this.Text = "Aqua Model Tool - " + Path.GetFileName(currentFile);
                }
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(currentFile != null)
            {
                string ext = Path.GetExtension(currentFile);

                //Model saving
                if (modelExtensions.Contains(ext))
                {
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
                else if (motionExtensions.Contains(ext))
                {
                    switch (isNIFL)
                    {
                        case true:
                            aquaUI.aqua.WriteNIFLMotion(currentFile);
                            break;
                        case false:
                            aquaUI.aqua.WriteVTBFMotion(currentFile);
                            break;
                    }
                    AquaUIOpenFile(currentFile);
                    this.Text = "Aqua Model Tool - " + Path.GetFileName(currentFile);
                } else if(effectExtensions.Contains(ext))
                {
                    aquaUI.aqua.WriteClassicNIFLEffect(currentFile);
                    AquaUIOpenFile(currentFile);
                    this.Text = "Aqua Model Tool - " + Path.GetFileName(currentFile);
                }
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
                        aquaUI.aqua.aquaEffect.Clear();
                        aquaUI.aqua.ReadModel(file);

#if DEBUG
                        //aquaUI.aqua.aquaModels[0].models[0].splitVSETPerMesh();
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
                        this.Size = new Size(400, 319);
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
                        aquaUI.aqua.aquaEffect.Clear();
                        aquaUI.aqua.ReadMotion(file);
#if DEBUG
                        var test2 = aquaUI.aqua.aquaMotions[0].anims[0];
                        test2 = aquaUI.aqua.aquaMotions[0].anims[0];
#endif
                        this.Size = new Size(400, 319);
                        control = SetMotion();
                        break;
                    case ".aqe":
                        aquaUI.aqua.aquaModels.Clear();
                        aquaUI.aqua.aquaMotions.Clear();
                        aquaUI.aqua.aquaEffect.Clear();
                        aquaUI.aqua.ReadEffect(file);
#if DEBUG
                        var test3 = aquaUI.aqua.aquaEffect[0];
                        test3 = aquaUI.aqua.aquaEffect[0];
#endif
                        if (aquaUI.aqua.aquaEffect[0].nifl.magic != 0)
                        {
                            isNIFL = true;
                        }
                        else
                        {
                            isNIFL = false;
                        }
                        control = new EffectEditor(aquaUI.aqua.aquaEffect[0]);
                        this.Size = new Size(800, 660);
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

        private UserControl SetMotion()
        {
            UserControl control = new AnimationEditor(aquaUI.aqua.aquaMotions[0]);
            if (aquaUI.aqua.aquaMotions[0].anims[0].nifl.magic != 0)
            {
                isNIFL = true;
            }
            else
            {
                isNIFL = false;
            }
            setModelOptions(false);
            return control;
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

        private void parsePSO2TextToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog()
            {
                Title = "Select a pso2 .text file",
                Filter = "PSO2 Text (*.text) Files|*.text",
                Multiselect = true
            };
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                foreach(var fileName in openFileDialog.FileNames)
                {
                    aquaUI.aqua.LoadPSO2Text(fileName);

                    StringBuilder output = new StringBuilder();

                    for (int i = 0; i < aquaUI.aqua.aquaText.text.Count; i++)
                    {
                        output.AppendLine(aquaUI.aqua.aquaText.categoryNames[i]);

                        for (int j = 0; j < aquaUI.aqua.aquaText.text[i].Count; j++)
                        {
                            output.AppendLine($"Group {j}");

                            for (int k = 0; k < aquaUI.aqua.aquaText.text[i][j].Count; k++)
                            {
                                var pair = aquaUI.aqua.aquaText.text[i][j][k];
                                output.AppendLine($"{pair.name} - {pair.str}");
                            }
                            output.AppendLine();
                        }
                        output.AppendLine();
                    }

                    File.WriteAllText(fileName + ".txt", output.ToString());
                }

            }
        }

        private void readBonesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog()
            {
                Title = "Select PSO2 Bones",
                Filter = "PSO2 Bones (*.aqn, *.trn)|*.aqn;*.trn"
            };
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                aquaUI.aqua.ReadBones(openFileDialog.FileName);
            }
        }

        private void updateClassicPlayerAnimToNGSAnimToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog()
            {
                Title = "Select NGS PSO2 Bones",
                Filter = "PSO2 Bones (*.aqn)|*.aqn"
            };
            if(openFileDialog.ShowDialog() == DialogResult.OK)
            {
                aquaUI.aqua.aquaBones.Clear();
                aquaUI.aqua.ReadBones(openFileDialog.FileName);
                if (aquaUI.aqua.aquaBones[0].nodeList.Count < 171)
                {
                    aquaUI.aqua.aquaBones.Clear();
                    MessageBox.Show("Not an NGS PSO2 .aqn");
                    return;
                }
                var data = new AquaModelLibrary.NGSAnimUpdater();
                data.GetDefaultTransformsFromBones(aquaUI.aqua.aquaBones[0]);

                openFileDialog = new OpenFileDialog()
                {
                    Title = "Select Classic PSO2 Player Animation",
                    Filter = "PSO2 Player Animation (*.aqm)|*.aqm",
                    FileName = ""
                };
                if(openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    aquaUI.aqua.aquaBones.Clear();
                    aquaUI.aqua.aquaMotions.Clear();
                    aquaUI.aqua.ReadMotion(openFileDialog.FileName);
                    data.UpdateToNGSPlayerMotion(aquaUI.aqua.aquaMotions[0].anims[0]);

                    currentFile = openFileDialog.FileName;
                    this.Text = "Aqua Model Tool - " + Path.GetFileName(currentFile);

                    filePanel.Controls.Clear();
                    var control = SetMotion();
                    filePanel.Controls.Add(control);
                    control.Dock = DockStyle.Fill;
                    control.BringToFront();
                }
            }
        }

        private void generateCharacterFileSheetToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            CommonOpenFileDialog goodFolderDialog = new CommonOpenFileDialog()
            {
                IsFolderPicker = true,
                Title = "Select pso2_bin",
            };
            if (goodFolderDialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                goodFolderDialog.Title = "Select output directory";
                var pso2_binDir = goodFolderDialog.FileName;

                if (goodFolderDialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    var outfolder = goodFolderDialog.FileName;

                    aquaUI.aqua.pso2_binDir = pso2_binDir;
                    aquaUI.aqua.GenerateCharacterFileList(pso2_binDir, outfolder);
                }
            }

        }

        private void pSOnrelTotrpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog()
            {
                Title = "Select PSO1 n.rel map file",
                Filter = "PSO1 Map (*n.rel)|*n.rel"
            };
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                var rel = new PSONRelConvert(File.ReadAllBytes(openFileDialog.FileName));
                var aqua = new AquaUtil();
                var set = new AquaUtil.ModelSet();
                set.models.Add(rel.aqObj);
                aqua.aquaModels.Add(set);
                aqua.ConvertToClassicPSO2Mesh(false, false, false, false, false, false);

                var fname = openFileDialog.FileName.Replace(".rel", ".trp");
                aqua.WriteClassicNIFLModel(fname, fname);
            }
        }

        private void exportToGLTFToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var exportDialog = new SaveFileDialog()
            {
                Title = "Export model file",
                Filter = "GLB model (*.glb)|*.glb"
            };
            if(exportDialog.ShowDialog() == DialogResult.OK)
            {
                aquaUI.aqua.ExportToGLTF(exportDialog.FileName);
            }
        }

        private void importFromGLTFToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog()
            {
                Title = "Select gltf/glb model file",
                Filter = "GLTF model (*.glb, *.gltf)|*.glb;*.gltf"
            };
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                ModelExporter.getGLTF(openFileDialog.FileName);
            }
        }

    }
}
