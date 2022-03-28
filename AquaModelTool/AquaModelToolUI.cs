using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AquaModelLibrary;
using AquaModelLibrary.Nova;
using Microsoft.WindowsAPICodePack.Dialogs;
using zamboni;
using static AquaModelLibrary.AquaCommon;

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
#if !DEBUG
            debugToolStripMenuItem.Visible = false;        
#endif
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
                    Filter = "PSO2 VTBF Model (*.aqp)|*.aqp|PSO2 VTBF Terrain (*.trp)|*.trp|PSO2 NIFL Model (*.aqp)|*.aqp|PSO2 NIFL Terrain (*.trp)|*.trp"
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

            }
            else if (effectExtensions.Contains(ext))
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
            if (currentFile != null)
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
                }
                else if (effectExtensions.Contains(ext))
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
                        aquaUI.aqua.aquaBones.Clear();
                        aquaUI.aqua.tpnFiles.Clear();
                        aquaUI.aqua.ReadModel(file, true);
#if DEBUG
                        var test = aquaUI.aqua.aquaModels[0].models[0];
                        /*
                        for (int i = 0; i < test.tstaList.Count; i++)
                        {
                            string tex = test.texfList[i].texName.GetString();
                            string tex2 = tex.Replace(".dds", "_d.dds");
                            var texf = test.texfList[i];
                            var tsta = test.tstaList[i];
                            texf.texName = PSO2String.GeneratePSO2String(tex2);

                            tsta.texName = PSO2String.GeneratePSO2String(tex2);
                            test.texfList[i] = texf;
                            test.tstaList[i] = tsta;
                        }*/
                        //aquaUI.aqua.aquaModels[0].models[0].splitVSETPerMesh();
                        /*for(int i = 0; i < test.tstaList.Count; i++)
                        {
                            Console.WriteLine(i + " " + test.tstaList[i].texName.GetString());
                        }
                        */

                        //Spirefier 
                        /*
                        for(int j = 0; j < test.vtxlList[0].vertPositions.Count; j++)
                        {
                            var vec3 = test.vtxlList[0].vertPositions[j];
                            if (vec3.Y > 0.85)
                            {
                                vec3.Y *= 10000;
                                test.vtxlList[0].vertPositions[j] = vec3;
                            }
                        }*/

                        //test.objc.bounds = AquaObjectMethods.GenerateBounding(test.vtxlList);
#endif
                        control = new ModelEditor(aquaUI.aqua.aquaModels[0]);
                        if (aquaUI.aqua.aquaModels[0].models[0].nifl.magic != 0)
                        {
                            isNIFL = true;
                        }
                        else
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
                Filter = "All Files|*",
                Multiselect = true
            };
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                foreach(string file in openFileDialog.FileNames)
                {
                    AquaModelLibrary.AquaUtil.AnalyzeVTBF(file);
                }
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
                DumpTextFiles(openFileDialog.FileNames);
            }
        }

        private void DumpTextFiles(string[] fileNames)
        {
            foreach (var fileName in fileNames)
            {
                aquaUI.aqua.LoadPSO2Text(fileName);

                StringBuilder output = new StringBuilder();
                output.AppendLine(Path.GetFileName(fileName) + " was created: " + File.GetCreationTime(fileName).ToString());
                output.AppendLine("Filesize is: " + new FileInfo(fileName).Length.ToString() + " bytes");
                output.AppendLine();
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

        private void convertTxtToPSO2TextToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog()
            {
                Title = "Select a .txt file (Must follow parsed pso2 .text formatting)",
                Filter = "txt (*.txt) Files|*.txt",
                Multiselect = true
            };
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                ConvertTxtFiles(openFileDialog.FileNames);
            }
        }

        private void ConvertTxtFiles(string[] fileNames)
        {
            foreach (var fileName in fileNames)
            {
                    AquaUtil.ConvertPSO2Text(fileName.Split('.')[0] + ".text", fileName);
            }
        }
        private void parsePSO2TextFolderSelectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CommonOpenFileDialog goodFolderDialog = new CommonOpenFileDialog()
            {
                IsFolderPicker = true,
                Title = "Select pso2 .text folder",
            };
            if (goodFolderDialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                DumpTextFiles(Directory.GetFiles(goodFolderDialog.FileName, "*.text"));
            }
        }
        private void convertTxtToPSO2TextFolderSelectToolStripMenuItem_Click(object sender, EventArgs e)
        {

            CommonOpenFileDialog goodFolderDialog = new CommonOpenFileDialog()
            {
                IsFolderPicker = true,
                Title = "Select .txt folder",
            };
            if (goodFolderDialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                ConvertTxtFiles(Directory.GetFiles(goodFolderDialog.FileName, "*.txt"));
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
#if DEBUG
                for(int i = 0; i < aquaUI.aqua.aquaBones[0].nodeList.Count; i++)
                {
                    var bone = aquaUI.aqua.aquaBones[0].nodeList[i];
                    Console.WriteLine($"{bone.boneName.GetString()} {bone.boneShort1.ToString("X")} {bone.boneShort2.ToString("X")}  {bone.eulRot.X.ToString()} {bone.eulRot.Y.ToString()} {bone.eulRot.Z.ToString()} ");
                    Console.WriteLine((bone.parentId == -1) + "");
                }
#endif
            }
        }

        private void updateClassicPlayerAnimToNGSAnimToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog()
            {
                Title = "Select NGS PSO2 Bones",
                Filter = "PSO2 Bones (*.aqn)|*.aqn"
            };
            if (openFileDialog.ShowDialog() == DialogResult.OK)
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
                if (openFileDialog.ShowDialog() == DialogResult.OK)
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
                bool useSubPath = true;
                string subPath = "";
                string fname = openFileDialog.FileName;
                string outFolder = null;
                if (useSubPath == true)
                {
                    subPath = Path.GetFileNameWithoutExtension(openFileDialog.FileName) + "\\";
                    var info = Directory.CreateDirectory(Path.GetDirectoryName(openFileDialog.FileName) + "\\" + subPath);
                    fname = info.FullName + Path.GetFileName(openFileDialog.FileName);
                    outFolder = info.FullName;
                }

                var rel = new PSONRelConvert(File.ReadAllBytes(openFileDialog.FileName), openFileDialog.FileName, 0.1f, outFolder);
                var aqua = new AquaUtil();
                var set = new AquaUtil.ModelSet();
                set.models.Add(rel.aqObj);
                aqua.aquaModels.Add(set);
                aqua.ConvertToClassicPSO2Mesh(false, false, false, false, false, false, false);

                fname = fname.Replace(".rel", ".trp");
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
            if (exportDialog.ShowDialog() == DialogResult.OK)
            {
                //aquaUI.aqua.ExportToGLTF(exportDialog.FileName);
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
                //ModelExporter.getGLTF(openFileDialog.FileName);
            }
        }

        private void GetTexSheetData(Dictionary<string, List<string>> shaderCombinationsTexSheet, Dictionary<string, List<string>> shaderModelFilesTexSheet, string file)
        {
            var model = aquaUI.aqua.aquaModels[0].models[0];

            //Go through all meshes in each model
            foreach (var mesh in model.meshList)
            {
                var shad = model.shadList[mesh.shadIndex];
                string key = shad.pixelShader.GetString() + " " + shad.vertexShader.GetString();
                var textures = AquaObjectMethods.GetTexListNames(model, mesh.tsetIndex);

                if (textures.Count == 0 || textures == null)
                {
                    continue;
                }

                string combination = "";
                foreach (var tex in textures)
                {
                    if (tex.Contains("_d.dds") || tex.Contains("_d_") || tex.Contains("_diffuse")) //diffuse
                    {
                        combination += "d";
                    }
                    else if (tex.Contains("_s.dds") || tex.Contains("_s_") || tex.Contains("_multi")) //specular composite
                    {
                        combination += "s";
                    }
                    else if (tex.Contains("_m.dds") || tex.Contains("_m_")) //mask
                    {
                        combination += "m";
                    }
                    else if (tex.Contains("_n.dds") || tex.Contains("_n_") || tex.Contains("_normal")) //normal
                    {
                        combination += "n";
                    }
                    else if (tex.Contains("_t.dds") || tex.Contains("_t_"))
                    {
                        combination += "t";
                    }
                    else if (tex.Contains("_k.dds") || tex.Contains("_k_")) //glow thing
                    {
                        combination += "k";
                    }
                    else if (tex.Contains("_p.dds") || tex.Contains("_p_")) //another glow thing
                    {
                        combination += "p";
                    }
                    else if (tex.Contains("_a.dds") || tex.Contains("_a_"))
                    {
                        combination += "a";
                    }
                    else if (tex.Contains("_b.dds") || tex.Contains("_b_"))
                    {
                        combination += "b";
                    }
                    else if (tex.Contains("_c.dds") || tex.Contains("_c_"))
                    {
                        combination += "c";
                    }
                    else if (tex.Contains("_e.dds") || tex.Contains("_e_") || tex.Contains("_env.dds")) //environment map
                    {
                        combination += "e";
                    }
                    else if (tex.Contains("_f.dds") || tex.Contains("_f_")) //feather map? Mostly for rappies
                    {
                        combination += "f";
                    }
                    else if (tex.Contains("_r.dds") || tex.Contains("_r_"))
                    {
                        combination += "r";
                    }
                    else if (tex.Contains("_decal"))
                    {
                        combination += "decal";
                    }
                    else if (tex.Contains("_noise"))
                    {
                        combination += "noise";
                    }
                    else if (tex.Contains("_subnormal_01"))
                    {
                        combination += "subnormal_01";
                    }
                    else if (tex.Contains("_subnormal_02"))
                    {
                        combination += "subnormal_02";
                    }
                    else if (tex.Contains("_subnormal_03"))
                    {
                        combination += "subnormal_03";
                    }
                    else if (tex.Contains("_mask"))
                    {
                        combination += "mask";
                    }
                    else //Add the full name if we absolutely cannot figure this out from these
                    {
                        combination += tex;
                    }
                    combination += " ";
                }

                //Add them to the list
                if (!shaderCombinationsTexSheet.ContainsKey(key))
                {
                    shaderCombinationsTexSheet[key] = new List<string>() { combination };
                    shaderModelFilesTexSheet[key] = new List<string>() { Path.GetFileName(file) };
                }
                else
                {
                    shaderCombinationsTexSheet[key].Add(combination);
                    shaderModelFilesTexSheet[key].Add(Path.GetFileName(file));
                }
            }
            model = null;
        }

        private void batchParsePSO2SetToTextToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CommonOpenFileDialog goodFolderDialog = new CommonOpenFileDialog()
            {
                IsFolderPicker = true,
                Title = "Select a folder containing pso2 .sets",
            };
            if (goodFolderDialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                List<string> files = new List<string>();
                string[] extensions = new string[] { "*.set"};
                foreach (string s in extensions)
                {
                    files.AddRange(Directory.GetFiles(goodFolderDialog.FileName, s));
                }

                //Go through models we gathered
                foreach (string file in files)
                {
                    aquaUI.aqua.ReadSet(file);
                }

                //Gather from .set files. This is subject to change because I'm really just checking things for now.
                StringBuilder allSetOutput = new StringBuilder();
                StringBuilder objSetOutput = new StringBuilder();
                for (int i = 0; i < aquaUI.aqua.aquaSets.Count; i++)
                {
                    StringBuilder setString = new StringBuilder();

                    var set = aquaUI.aqua.aquaSets[i];
                    setString.AppendLine(set.fileName);

                    //Strings
                    foreach (var entityString in set.entityStrings)
                    {
                        for (int sub = 0; sub < entityString.subStrings.Count; sub++)
                        {
                            var subStr = entityString.subStrings[sub];
                            setString.Append(subStr);
                            if (sub != (entityString.subStrings.Count - 1))
                            {
                                setString.Append(",");
                            }
                        }
                        setString.AppendLine();
                    }

                    //Objects
                    foreach (var obj in set.setEntities)
                    {
                        StringBuilder objString = new StringBuilder();
                        objString.AppendLine(obj.entity_variant_string0.GetString());
                        objString.AppendLine(obj.entity_variant_string1);
                        objString.AppendLine(obj.entity_variant_stringJP);
                        foreach (var variable in obj.variables)
                        {
                            objString.AppendLine(variable.Key + " - " + variable.Value.ToString());
                        }
                        setString.Append(objString);

                        if (obj.variables.ContainsKey("object_name"))
                        {
                            objSetOutput.AppendLine(set.fileName);
                            objSetOutput.Append(objString);
                        }
                    }

                    allSetOutput.Append(setString);
                    allSetOutput.AppendLine();
                }

                File.WriteAllText(goodFolderDialog.FileName + "\\" + "allSetOutput.txt", allSetOutput.ToString());
                File.WriteAllText(goodFolderDialog.FileName + "\\" + "objects.txt", objSetOutput.ToString());

                aquaUI.aqua.aquaSets.Clear();
            }
        }

        private void checkAllShaderExtrasToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CommonOpenFileDialog goodFolderDialog = new CommonOpenFileDialog()
            {
                IsFolderPicker = true,
                Title = "Select a folder containing pso2 models/ice files (PRM has no shader and will not be read). This shit can take a longass time",
            };
            if (goodFolderDialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                Dictionary<string, List<string>> shaderCombinationsTexSheet = new Dictionary<string, List<string>>();
                Dictionary<string, List<string>> shaderModelFilesTexSheet = new Dictionary<string, List<string>>();
                Dictionary<string, List<string>> shaderUnk0 = new Dictionary<string, List<string>>();
                Dictionary<string, List<string>> shaderCombinations = new Dictionary<string, List<string>>();
                Dictionary<string, List<string>> shaderModelFiles = new Dictionary<string, List<string>>();
                Dictionary<string, List<string>> shaderDetails = new Dictionary<string, List<string>>();
                Dictionary<string, List<string>> shaderExtras = new Dictionary<string, List<string>>();
                List<string> files = new List<string>();
                string[] extensions = new string[] { ".aqp", ".aqo", ".trp", ".tro" };
                files.AddRange(Directory.GetFiles(goodFolderDialog.FileName, "*", SearchOption.AllDirectories));

                //Go through models we gathered
                foreach (string file in files)
                {
                    if(extensions.Contains(Path.GetExtension(file)))
                    {
                        try
                        {
                            aquaUI.aqua.ReadModel(file);
                        }
                        catch
                        {
                            Console.WriteLine("Could not read file: " + file);
                            continue;
                        }

                        ParseModelShaderInfo(shaderUnk0, shaderCombinations, shaderModelFiles, shaderDetails, shaderExtras, file);
                        GetTexSheetData(shaderCombinationsTexSheet, shaderModelFilesTexSheet, file);
                    } else 
                    {
                        var fileBytes = File.ReadAllBytes(file);
                        if(fileBytes.Length > 0)
                        {
                            var magic = BitConverter.ToInt32(fileBytes, 0);
                            if (magic == 0x454349)
                            {
                                var strm = new MemoryStream(fileBytes);
                                IceFile fVarIce;
                                try
                                {
                                    fVarIce = IceFile.LoadIceFile(strm);

                                    List<byte[]> iceFiles = (new List<byte[]>(fVarIce.groupOneFiles));
                                    iceFiles.AddRange(fVarIce.groupTwoFiles);

                                    //Loop through files to get what we need
                                    foreach (byte[] iceFileBytes in iceFiles)
                                    {
                                        var name = IceFile.getFileName(iceFileBytes).ToLower();
                                        var nameExtension = Path.GetExtension(name);
                                        if (extensions.Contains(nameExtension))
                                        {
                                            try
                                            {
                                                aquaUI.aqua.ReadModel(iceFileBytes);
                                                ParseModelShaderInfo(shaderUnk0, shaderCombinations, shaderModelFiles, shaderDetails, shaderExtras, name);
                                                GetTexSheetData(shaderCombinationsTexSheet, shaderModelFilesTexSheet, name);
                                            }
                                            catch
                                            {
                                                Console.WriteLine("Could not read file: " + name + " in " + file);
                                                continue;
                                            }
                                        }
                                    }

                                    fVarIce = null;
                                }
                                catch
                                {
                                }
                                strm.Dispose();
                            }
                        }

                        fileBytes = null;
                    }
                    aquaUI.aqua.aquaModels.Clear();
                }

                //Sort the list so we don't get a mess
                var keys = shaderCombinations.Keys.ToList();
                keys.Sort();
                
                StringBuilder simpleOutput = new StringBuilder();
                StringBuilder advancedOutput = new StringBuilder();
                StringBuilder detailDictOutput = new StringBuilder();
                StringBuilder extraDictOutput = new StringBuilder();
                StringBuilder unk0DictOutput = new StringBuilder();

                detailDictOutput.Append("public static Dictionary<string, SHADDetail> NGSShaderDetailPresets = new Dictionary<string, SHADDetail>(){\n");
                extraDictOutput.Append("public static Dictionary<string, List<SHADExtraEntry>> NGSShaderExtraPresets = new Dictionary<string, List<SHADExtraEntry>>(){\n");
                unk0DictOutput.Append("public static Dictionary<string, int> ShaderUnk0Values = new Dictionary<string, int>(){\n");
                foreach (var key in keys)
                {
                    simpleOutput.Append("\n" + key + "\n" + shaderCombinations[key][0]);
                    detailDictOutput.Append(shaderDetails[key][0]);
                    extraDictOutput.Append(shaderExtras[key][0]);
                    unk0DictOutput.Append(shaderUnk0[key][0]);
                    advancedOutput.Append("\n" + key + "\n" + shaderCombinations[key][0] + "," + shaderModelFiles[key][0]);
                    for (int i = 1; i < shaderCombinations[key].Count; i++)
                    {
                        advancedOutput.AppendLine("," + shaderCombinations[key][i] + "," + shaderModelFiles[key][i] + "," + shaderUnk0[key][i]);
                        advancedOutput.AppendLine();
                    }
                    advancedOutput.AppendLine();
                }
                detailDictOutput.Append("};\n");
                extraDictOutput.Append("};\n");
                unk0DictOutput.Append("};\n");

                detailDictOutput.Append(extraDictOutput);
                detailDictOutput.Append(unk0DictOutput);
                File.WriteAllText(goodFolderDialog.FileName + "\\" + "simpleNGSOutput.csv", simpleOutput.ToString());
                File.WriteAllText(goodFolderDialog.FileName + "\\" + "detailedNGSOutput.csv", advancedOutput.ToString());
                File.WriteAllText(goodFolderDialog.FileName + "\\" + "presetDictionary.cs", detailDictOutput.ToString());

                //Sort the tex sheet list so we don't get a mess
                var keysTexSheet = shaderCombinationsTexSheet.Keys.ToList();
                keysTexSheet.Sort();

                StringBuilder simpleOutputTexSheet = new StringBuilder();
                StringBuilder advancedOutputTexSheet = new StringBuilder();
                foreach (var key in keysTexSheet)
                {
                    simpleOutputTexSheet.AppendLine(key + "," + shaderCombinationsTexSheet[key][0]);

                    advancedOutputTexSheet.AppendLine(key + "," + shaderCombinationsTexSheet[key][0] + "," + shaderModelFilesTexSheet[key][0]);
                    for (int i = 1; i < shaderCombinationsTexSheet[key].Count; i++)
                    {
                        advancedOutputTexSheet.AppendLine("," + shaderCombinationsTexSheet[key][i] + "," + shaderModelFilesTexSheet[key][i]);
                    }
                    advancedOutputTexSheet.AppendLine();
                }

                File.WriteAllText(goodFolderDialog.FileName + "\\" + "simpleOutputTexSheets.csv", simpleOutputTexSheet.ToString());
                File.WriteAllText(goodFolderDialog.FileName + "\\" + "detailedOutputTexSheets.csv", advancedOutputTexSheet.ToString());
            }

            aquaUI.aqua.aquaModels.Clear();
        }

        private void ParseModelShaderInfo(Dictionary<string, List<string>> shaderUnk0, Dictionary<string, List<string>> shaderCombinations, Dictionary<string, List<string>> shaderModelFiles, Dictionary<string, List<string>> shaderDetails, Dictionary<string, List<string>> shaderExtras, string file)
        {
            string filestring = file;
            //Add them to the list
            if (filestring.Contains(":"))
            {
                filestring = Path.GetFileName(filestring);
            }
            var model = aquaUI.aqua.aquaModels[0].models[0];

            //Go through all meshes in each model
            foreach (var shad in model.shadList)
            {
                string key = shad.pixelShader.GetString() + " " + shad.vertexShader.GetString();
                string shad0Line = "{" + $"\"{key}\", " + shad.unk0.ToString() + " },\n";
                if (shad.unk0 != 0)
                {
                    Debug.WriteLine(key + " " + shad.unk0);
                }


                if (shad is NGSAquaObject.NGSSHAD && (((NGSAquaObject.NGSSHAD)shad).shadDetailOffset != 0 || ((NGSAquaObject.NGSSHAD)shad).shadExtraOffset != 0))
                {
                    NGSAquaObject.NGSSHAD ngsShad = (NGSAquaObject.NGSSHAD)shad;

                    string data = "";
                    string detData = "";
                    string extData = "";
                    if (ngsShad.shadDetailOffset != 0)
                    {
                        data = $"Detail : \n unk0:{ngsShad.shadDetail.unk0} Extra Count:{ngsShad.shadDetail.shadExtraCount} unk1:{ngsShad.shadDetail.unk1} unkCount0:{ngsShad.shadDetail.unkCount0}\n" +
                            $" unk2:{ngsShad.shadDetail.unk2} unkCount1:{ngsShad.shadDetail.unkCount1} unk3:{ngsShad.shadDetail.unk3} unk4:{ngsShad.shadDetail.unk4}\n";
                        detData = "{" + $"\"{key}\", CreateDetail({ngsShad.shadDetail.unk0}, {ngsShad.shadDetail.shadExtraCount}, {ngsShad.shadDetail.unk1}, " +
                            $"{ngsShad.shadDetail.unkCount0}, {ngsShad.shadDetail.unk2}, {ngsShad.shadDetail.unkCount1}, {ngsShad.shadDetail.unk3}, " +
                            $"{ngsShad.shadDetail.unk4})" + "},\n";
                    }
                    if (ngsShad.shadExtraOffset != 0)
                    {
                        data += "Extra :\n";
                        extData = "{" + $"\"{key}\", new List<SHADExtraEntry>()" + "{";
                        foreach (var extra in ngsShad.shadExtra)
                        {
                            data += $"{extra.entryString.GetString()} {extra.entryFlag0} {extra.entryFlag1} {extra.entryFlag2}\n" +
                                $"{extra.entryFloats.X} {extra.entryFloats.Y} {extra.entryFloats.Z} {extra.entryFloats.W}\n";
                            extData += " CreateExtra(" + $"{extra.entryFlag0}, \"{extra.entryString.GetString()}\"," +
                                $" {extra.entryFlag1}, {extra.entryFlag2}, new Vector4({extra.entryFloats.X}f, {extra.entryFloats.Y}f, {extra.entryFloats.Z}f, " +
                                $"{extra.entryFloats.W}f)),";
                        }
                        extData += "}},\n";
                    }

                    if (!shaderCombinations.ContainsKey(key))
                    {
                        shaderUnk0[key] = new List<string>() { shad0Line };
                        shaderCombinations[key] = new List<string>() { data };
                        shaderModelFiles[key] = new List<string>() { filestring };
                        shaderDetails[key] = new List<string>() { detData };
                        shaderExtras[key] = new List<string>() { extData };
                    }
                    else
                    {
                        shaderUnk0[key].Add(shad0Line);
                        shaderCombinations[key].Add(data);
                        shaderModelFiles[key].Add(filestring);
                        shaderDetails[key].Add(detData);
                        shaderExtras[key].Add(extData);
                    }
                }
                else if(shad.unk0 != 0)
                {
                    if (!shaderCombinations.ContainsKey(key))
                    {
                        shaderUnk0[key] = new List<string>() { shad0Line };
                        shaderCombinations[key] = new List<string>() { "" };
                        shaderModelFiles[key] = new List<string>() { filestring };
                        shaderDetails[key] = new List<string>() { "" };
                        shaderExtras[key] = new List<string>() { "" };
                    }
                    else
                    {
                        shaderUnk0[key].Add(shad0Line);
                        shaderCombinations[key].Add("");
                        shaderModelFiles[key].Add(filestring);
                        shaderDetails[key].Add("");
                        shaderExtras[key].Add("");
                    }
                }
                else
                {
                    continue;
                }
            }


            model = null;
        }

        private void computeTangentSpaceTestToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AquaObjectMethods.ComputeTangentSpace(aquaUI.aqua.aquaModels[0].models[0], false, true);
        }

        private void cloneBoneTransformsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            aquaUI.aqua.aquaBones.Clear();
            OpenFileDialog openFileDialog = new OpenFileDialog()
            {
                Title = "Select PSO2 Bones",
                Filter = "PSO2 Bones (*.aqn, *.trn)|*.aqn;*.trn"
            };
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                OpenFileDialog openFileDialog2 = new OpenFileDialog()
                {
                    Title = "Select PSO2 Bones",
                    Filter = "PSO2 Bones (*.aqn, *.trn)|*.aqn;*.trn"
                };
                if (openFileDialog2.ShowDialog() == DialogResult.OK)
                {
                    aquaUI.aqua.ReadBones(openFileDialog.FileName);
                    aquaUI.aqua.ReadBones(openFileDialog2.FileName);

                    var bone1 = aquaUI.aqua.aquaBones[0];
                    var bone2 = aquaUI.aqua.aquaBones[1];
                    for (int i = 0; i < bone1.nodeList.Count; i++)
                    {
                        var bone = bone1.nodeList[i];
                        //bone.firstChild = bone2.nodeList[i].firstChild;
                        bone.eulRot = bone2.nodeList[i].eulRot;
                        /*
                        bone.nextSibling = bone2.nodeList[i].nextSibling;
                        bone.ngsSibling = bone2.nodeList[i].ngsSibling;
                        bone.pos = bone2.nodeList[i].pos;
                        bone.scale = bone2.nodeList[i].scale;
                        bone.m1 = bone2.nodeList[i].m1;
                        bone.m2 = bone2.nodeList[i].m2;
                        bone.m3 = bone2.nodeList[i].m3;
                        bone.m4 = bone2.nodeList[i].m4;*/
                        bone1.nodeList[i] = bone;
                    }

                    AquaUtil.WriteBones(openFileDialog.FileName + "_out", bone1);
                }
            }
        }

        private void legacyAqp2objObjExportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (aquaUI.aqua.aquaModels.Count > 0)
            {
                var exportDialog = new SaveFileDialog()
                {
                    Title = "Export obj file for basic editing",
                    Filter = "Object model (*.obj)|*.obj"
                };
                if (exportDialog.ShowDialog() == DialogResult.OK)
                {
                    LegacyObj.LegacyObjIO.ExportObj(exportDialog.FileName, aquaUI.aqua.aquaModels[0].models[0]);
                }
            }
        }

        private void legacyAqp2objObjImportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Import obj geometry to current file. Make sure to remove LOD models.
            if(aquaUI.aqua.aquaModels.Count > 0)
            {
                OpenFileDialog openFileDialog = new OpenFileDialog()
                {
                    Title = "Select PSO2 .obj",
                    Filter = "PSO2 .obj (*.obj)|*.obj"
                };
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    var newObj = LegacyObj.LegacyObjIO.ImportObj(openFileDialog.FileName, aquaUI.aqua.aquaModels[0].models[0]);
                    aquaUI.aqua.aquaModels[0].models.Clear();
                    aquaUI.aqua.aquaModels[0].models.Add(newObj);
                    ((ModelEditor)filePanel.Controls[0]).PopulateModelDropdown();
                }

            }
        }

        private void testVTXEToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var model = aquaUI.aqua.aquaModels[0].models[0];
            for(int i = 0; i < model.vtxlList.Count; i++)
            {
                model.vtxeList[i] = AquaObjectMethods.ConstructClassicVTXE(model.vtxlList[i], out int vertSize);
            }
        }

        private void exportModelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string ext = Path.GetExtension(currentFile);
            //Model saving
            if (modelExtensions.Contains(ext))
            {
                using (var ctx = new Assimp.AssimpContext())
                {
                    var formats = ctx.GetSupportedExportFormats();
                    List<(string ext,string desc)> filterKeys = new List<(string ext,string desc)>();
                    foreach(var format in formats)
                    {
                        filterKeys.Add((format.FileExtension,format.Description));
                    }
                    filterKeys.Sort();
                
                    SaveFileDialog saveFileDialog;
                    saveFileDialog = new SaveFileDialog()
                    {
                        Title = "Export model file",
                        Filter = ""
                    };
                    string tempFilter = "";
                    foreach(var fileExt in filterKeys)
                    {
                        tempFilter += $"{fileExt.desc} (*.{fileExt.ext})|*.{fileExt.ext}|";
                    }
                    tempFilter = tempFilter.Remove(tempFilter.Length - 1, 1);
                    saveFileDialog.Filter = tempFilter;
                    saveFileDialog.FileName = "";

                    //Get bone ext
                    string boneExt = "";
                    switch(ext)
                    {
                        case ".aqo":
                        case ".aqp":
                            boneExt = ".aqn";
                            break;
                        case ".tro":
                        case ".trp":
                            boneExt = ".trn";
                            break;
                        default:
                            break;
                    }
                    var bonePath = currentFile.Replace(ext,boneExt);
                    if (!File.Exists(bonePath))
                    {
                        OpenFileDialog openFileDialog = new OpenFileDialog() 
                        { 
                            Title = "Select PSO2 bones",
                            Filter = "PSO2 Bones (*.aqn,*.trn)|*.aqn;*.trn"
                        };
                        if(openFileDialog.ShowDialog() == DialogResult.OK)
                        {
                            bonePath = openFileDialog.FileName;
                        } else
                        {
                            MessageBox.Show("Must be able to read bones to export!");
                            return;
                        }
                    }
                    aquaUI.aqua.aquaBones.Clear();
                    aquaUI.aqua.ReadBones(bonePath);

                    if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        var id = saveFileDialog.FilterIndex - 1;
                        var scene = ModelExporter.AssimpExport(saveFileDialog.FileName, aquaUI.aqua.aquaModels[0].models[0], aquaUI.aqua.aquaBones[0]);
                        Assimp.ExportFormatDescription exportFormat = null;
                        for(int i = 0; i < formats.Length; i++)
                        {
                            if(formats[i].Description == filterKeys[id].desc && formats[i].FileExtension == filterKeys[id].ext)
                            {
                                exportFormat = formats[i];
                                break;
                            }
                        }
                        if(exportFormat  == null)
                        {
                            return;
                        }

                        try
                        {
                            ctx.ExportFile(scene, saveFileDialog.FileName, exportFormat.FormatId, Assimp.PostProcessSteps.FlipUVs);
                            
                            //Dae fix because Assimp 4 and 5.X can't seem to properly get a root node.
                            if (Path.GetExtension(saveFileDialog.FileName) == ".dae")
                            {
                                string replacementLine = $"<skeleton>(0)#" + aquaUI.aqua.aquaBones[0].nodeList[0].boneName.GetString() + "</skeleton>";

                                var dae = File.ReadAllLines(saveFileDialog.FileName);
                                for (int i = 0; i < dae.Length; i++)
                                {
                                    if (dae[i].Contains("<skeleton>"))
                                    {
                                        dae[i] = replacementLine;
                                    }
                                }
                                File.WriteAllLines(saveFileDialog.FileName, dae);
                            }
                        }
                        catch (Win32Exception w)
                        {
                            MessageBox.Show($"Exception encountered: {w.Message}");
                        }

                    }
                }

            }
        }

        private void dumpNOF0ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog()
            {
                Title = "Select PSO2 NIFL file",
                Filter = "PSO2 NIFL File (*)|*"
            };
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                AquaUtil.DumpNOF0(openFileDialog.FileName);
            }
        }

        private void readBTIToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog()
            {
                Title = "Select PSO2 NIFL file",
                Filter = "PSO2 NIFL File (*.bti)|*.bti"
            };
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                aquaUI.aqua.ReadBTI(openFileDialog.FileName);
            }
        }

        private void prmEffectModelExportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog()
            {
                Title = "Select PSO2 prm file",
                Filter = "PSO2 Effect Model File (*.prm)|*.prm",
                Multiselect = true
            };
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                //Read prms
                foreach(var file in openFileDialog.FileNames)
                {
                    aquaUI.aqua.LoadPRM(file);
                }

                //Set up export
                using (var ctx = new Assimp.AssimpContext())
                {
                    var formats = ctx.GetSupportedExportFormats();
                    List<(string ext, string desc)> filterKeys = new List<(string ext, string desc)>();
                    foreach (var format in formats)
                    {
                        filterKeys.Add((format.FileExtension, format.Description));
                    }
                    filterKeys.Sort();

                    SaveFileDialog saveFileDialog;
                    saveFileDialog = new SaveFileDialog()
                    {
                        Title = "Export model file",
                        Filter = ""
                    };
                    string tempFilter = "";
                    foreach (var fileExt in filterKeys)
                    {
                        tempFilter += $"{fileExt.desc} (*.{fileExt.ext})|*.{fileExt.ext}|";
                    }
                    tempFilter = tempFilter.Remove(tempFilter.Length - 1, 1);
                    saveFileDialog.Filter = tempFilter;
                    saveFileDialog.FileName = Path.GetFileName(Path.ChangeExtension(openFileDialog.FileName, "." + filterKeys[0].ext));

                    if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        var id = saveFileDialog.FilterIndex - 1;

                        Assimp.ExportFormatDescription exportFormat = null;
                        for (int i = 0; i < formats.Length; i++)
                        {
                            if (formats[i].Description == filterKeys[id].desc && formats[i].FileExtension == filterKeys[id].ext)
                            {
                                exportFormat = formats[i];
                                break;
                            }
                        }
                        if (exportFormat == null)
                        {
                            return;
                        }

                        //Iterate through each selected model and use the selected type.
                        var finalExtension = Path.GetExtension(saveFileDialog.FileName);
                        for(int i = 0; i < aquaUI.aqua.prmModels.Count; i++)
                        {
                            string finalName;
                            if(i == 0)
                            {
                                finalName = saveFileDialog.FileName;
                            } else
                            {
                                finalName = Path.ChangeExtension(openFileDialog.FileNames[i], finalExtension);
                            }

                            var scene = ModelExporter.AssimpPRMExport(finalName, aquaUI.aqua.prmModels[i]);

                            try
                            {
                                ctx.ExportFile(scene, finalName, exportFormat.FormatId, Assimp.PostProcessSteps.FlipUVs);
                            }
                            catch (Win32Exception w)
                            {
                                MessageBox.Show($"Exception encountered: {w.Message}");
                            }
                        }


                    }
                }
                aquaUI.aqua.prmModels.Clear();
            }
        }

        private void prmEffectFromModelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog()
            {
                Title = "Select Model file",
                Filter = "Assimp Model Files (*.*)|*.*"
            };
            List<string> filters = new List<string>();
            using (var ctx = new Assimp.AssimpContext())
            {
                foreach (var format in ctx.GetSupportedExportFormats())
                {
                    if (!filters.Contains(format.FileExtension))
                    {
                        filters.Add(format.FileExtension);
                    }
                }
            }
            filters.Sort();

            StringBuilder filterString = new StringBuilder("Assimp Model Files(");
            StringBuilder filterStringTypes = new StringBuilder("|");
            StringBuilder filterStringSections = new StringBuilder();
            foreach (var filter in filters)
            {
                filterString.Append($"*.{filter},");
                filterStringTypes.Append($"*.{filter};");
                filterStringSections.Append($"|{filter} Files ({filter})|*.{filter}");
            }

            //Get rid of comma, add parenthesis 
            filterString.Remove(filterString.Length - 1, 1);
            filterString.Append(")");

            //Get rid of unneeded semicolon
            filterStringTypes.Remove(filterStringTypes.Length - 1, 1);
            filterString.Append(filterStringTypes);

            //Add final section
            filterString.Append(filterStringSections);

            openFileDialog.Filter = filterString.ToString();

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                ModelImporter.AssimpPRMConvert(openFileDialog.FileName, Path.ChangeExtension(openFileDialog.FileName, ".prm"));
            }
        }

        private void readMagIndicesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog()
            {
                Title = "Select PSO2 MGX file",
                Filter = "PSO2 MGX File (*.mgx)|*.mgx"
            };
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                List<int> magIds = AquaMiscMethods.ReadMGX(openFileDialog.FileName);
            }
        }

        private void convertAnimationToAQMToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog()
            {
                Title = "Select Model file",
                Filter = "Assimp Model Files (*.*)|*.*"
            };
            List<string> filters = new List<string>();
            using (var ctx = new Assimp.AssimpContext())
            {
                foreach (var format in ctx.GetSupportedExportFormats())
                {
                    if (!filters.Contains(format.FileExtension))
                    {
                        filters.Add(format.FileExtension);
                    }
                }
            }
            filters.Sort();

            StringBuilder filterString = new StringBuilder("Assimp Model Files(");
            StringBuilder filterStringTypes = new StringBuilder("|");
            StringBuilder filterStringSections = new StringBuilder();
            foreach (var filter in filters)
            {
                filterString.Append($"*.{filter},");
                filterStringTypes.Append($"*.{filter};");
                filterStringSections.Append($"|{filter} Files ({filter})|*.{filter}");
            }

            //Get rid of comma, add parenthesis 
            filterString.Remove(filterString.Length - 1, 1);
            filterString.Append(")");

            //Get rid of unneeded semicolon
            filterStringTypes.Remove(filterStringTypes.Length - 1, 1);
            filterString.Append(filterStringTypes);

            //Add final section
            filterString.Append(filterStringSections);

            openFileDialog.Filter = filterString.ToString();
            openFileDialog.Multiselect = true;

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                //Handle maxscript scale differences from meters vs max's imperial feet based units
                float scaleFactor = 1;
                /*if(MessageBox.Show("Are the model(s) Maxscript model exports?", "Maxscript Model(s)?", MessageBoxButtons.YesNo) == DialogResult.No)
                {
                    scaleFactor = 0.3048f;
                }*/

                foreach(var file in openFileDialog.FileNames)
                {
                    ModelImporter.AssimpAQMConvert(file, false, true, scaleFactor);
                }
            }
        }

        private void readCMOToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog()
            {
                Title = "Select PSO2 CMO file",
                Filter = "PSO2 MGX File (*.cmo)|*.cmo"
            };
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                var cmo = AquaUtil.LoadCMO(openFileDialog.FileName);
            }
        }

        private void legacyAqp2objBatchExportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog()
            {
                Title = "Select PSO2 model file",
                Filter = "PSO2 Model Files (*.aqp, *.aqo, *.trp, *.tro)|*.aqp;*.aqo;*.trp;*.tro",
                Multiselect = true
            };
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                //Read models
                AquaUtil aqua = new AquaUtil(); //We want to leave the currently loaded model alone.
                foreach (var file in openFileDialog.FileNames)
                {
                    aqua.aquaModels.Clear();
                    aqua.ReadModel(file);
                    LegacyObj.LegacyObjIO.ExportObj(file + ".obj", aqua.aquaModels[0].models[0]);
                }
            }
        }

        private void dumpFigEffectTypesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog()
            {
                Title = "Select PSO2 FIG file",
                Filter = "PSO2 FIG Files (*.fig)|*.fig",
                Multiselect = true
            };
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                //Read figs
                StringBuilder sb = new StringBuilder();
                List<int> ints = new List<int>();
                foreach (var file in openFileDialog.FileNames)
                {
                    sb.Append(AquaUtil.CheckFigEffectMaps(file, ints));
                }
                ints.Sort();
                sb.AppendLine("All types:");
                foreach(var num in ints)
                {
                    sb.AppendLine(num.ToString() + " " + num.ToString("X"));
                }
                File.WriteAllText(Path.GetDirectoryName(openFileDialog.FileNames[0]) + "\\" + "figEffectTypes.txt", sb.ToString());
            }
        }

        private void spirefierToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(aquaUI.aqua.aquaModels.Count == 0)
            {
                return;
            }
            decimal value = 0;

            if(AquaUICommon.ShowInputDialog(ref value) == DialogResult.OK)
            {
                //Spirefier
                for(int i = 0; i < aquaUI.aqua.aquaModels[0].models.Count; i++)
                {
                    var model = aquaUI.aqua.aquaModels[0].models[i];
                    for (int j = 0; j < model.vtxlList[0].vertPositions.Count; j++)
                    {
                        var vec3 = model.vtxlList[0].vertPositions[j];
                        if (vec3.Y > (float)value)
                        {
                            vec3.Y *= 10000;
                            model.vtxlList[0].vertPositions[j] = vec3;
                        }
                    }

                    model.objc.bounds = AquaObjectMethods.GenerateBounding(model.vtxlList);
                }
            }
        }

        private void readCMXToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CommonOpenFileDialog goodFolderDialog = new CommonOpenFileDialog()
            {
                IsFolderPicker = true,
                Title = "Select pso2_bin",
            };
            if (goodFolderDialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                var pso2_binDir = goodFolderDialog.FileName;

                aquaUI.aqua.pso2_binDir = pso2_binDir;
                var aquaCMX = new CharacterMakingIndex();

                aquaCMX = CharacterMakingIndexMethods.ExtractCMX(pso2_binDir, aquaCMX);
            }
        }

        private void readFIGToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog()
            {
                Title = "Select PSO2 FIG file",
                Filter = "PSO2 FIG Files (*.fig)|*.fig",
                Multiselect = true
            };
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                //Read figs
                foreach (var file in openFileDialog.FileNames)
                {
                    aquaUI.aqua.ReadFig(file);
                }
            }
        }

        private void dumpFigShapesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog()
            {
                Title = "Select PSO2 FIG file",
                Filter = "PSO2 FIG Files (*.fig)|*.fig",
                Multiselect = true
            };
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                //Read figs
                StringBuilder sb = new StringBuilder();
                List<int> uniqueShapes = new List<int>();
                foreach (var file in openFileDialog.FileNames)
                {
                    aquaUI.aqua.aquaFigures.Clear();
                    aquaUI.aqua.ReadFig(file);

                    sb.AppendLine(Path.GetFileName(file));
                    var fig = aquaUI.aqua.aquaFigures[0];
                    if (fig.stateStructs != null)
                    {
                        foreach (var state in fig.stateStructs)
                        {
                            sb.AppendLine();
                            sb.AppendLine(state.text);
                            if (state.collision != null)
                            {
                                if (state.collision.colliders != null)
                                {
                                    foreach (var col in state.collision.colliders)
                                    {
                                        int shape = col.colStruct.shape;
                                        sb.AppendLine(shape + " " + col.name + " " + col.text1);
                                        if (!uniqueShapes.Contains(shape))
                                        {
                                            uniqueShapes.Add(shape);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                uniqueShapes.Sort();
                sb.AppendLine();
                sb.AppendLine("Unique Shapes");
                foreach (int shape in uniqueShapes)
                {
                    sb.AppendLine(shape + "");
                }
                File.WriteAllText(Path.GetDirectoryName(openFileDialog.FileNames[0]) + "\\" + "figShapes.txt", sb.ToString());
            }
        }

        private void readRebootLacToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog()
            {
                Title = "Select PSO2 finger LAC file",
                Filter = "PSO2 finger LAC Files (*.lac)|*.lac",
                Multiselect = true
            };
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                //Read LACs
                foreach (var file in openFileDialog.FileNames)
                {
                    AquaMiscMethods.ReadRebootLAC(file);
                }
            }
        }

        private void readLacToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog()
            {
                Title = "Select PSO2 LAC file",
                Filter = "PSO2 LAC Files (*.lac)|*.lac",
                Multiselect = true
            };
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                //Read LACs
                foreach (var file in openFileDialog.FileNames)
                {
                    AquaMiscMethods.ReadLAC(file);
                }
            }
        }

        private void readCMXFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CommonOpenFileDialog goodFolderDialog = new CommonOpenFileDialog()
            {
                Title = "Select CMX",
            };
            if (goodFolderDialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                var aquaCMX = new CharacterMakingIndex();

                aquaCMX = CharacterMakingIndexMethods.ReadCMX(goodFolderDialog.FileName, aquaCMX);
            }
        }

        private void proportionAQMAnalyzerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CommonOpenFileDialog goodFolderDialog = new CommonOpenFileDialog()
            {
                Title = "Select proportion AQM",
            };
            if (goodFolderDialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                StringBuilder outStr = new StringBuilder();
                StringBuilder endStr = new StringBuilder();
                Dictionary<int, List<int>> timeSorted = new Dictionary<int, List<int>>();
                aquaUI.aqua.aquaMotions.Clear();
                aquaUI.aqua.ReadMotion(goodFolderDialog.FileName);

                //Go through keyframes for every node and note each bone that uses a specific frame
                foreach(var keySet in aquaUI.aqua.aquaMotions[0].anims[0].motionKeys)
                {
                    foreach(var data in keySet.keyData)
                    {
                        foreach(var time in data.frameTimings)
                        {
                            var trueTime = time / 0x10;
                            if(!timeSorted.ContainsKey(trueTime))
                            {
                                timeSorted[trueTime] = new List<int>();
                            }
                            if(!timeSorted[trueTime].Contains(keySet.mseg.nodeId))
                            {
                                timeSorted[trueTime].Add(keySet.mseg.nodeId);
                            }
                        }
                    }
                }

                var timeSortedKeys = timeSorted.Keys.ToList();
                timeSortedKeys.Sort();
                foreach(var key in timeSortedKeys)
                {
                    timeSorted[key].Sort();
                    outStr.AppendLine("Frame Time: " + key);
                    foreach(var node in timeSorted[key])
                    {
                        outStr.AppendLine($"  {node} - {aquaUI.aqua.aquaMotions[0].anims[0].motionKeys[node].mseg.nodeName.GetString()}");
                    }
                    endStr.AppendLine(key + "");
                    outStr.AppendLine();
                }
                outStr.Append(endStr);
                File.WriteAllText(goodFolderDialog.FileName + "_times.txt", outStr.ToString());
            }
        }

        private void proportionAQMTesterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CommonOpenFileDialog goodFolderDialog = new CommonOpenFileDialog()
            {
                Title = "Select proportion AQM",
            };
            if (goodFolderDialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                int finalFrame = 1;
                //Get framecount
                aquaUI.aqua.ReadMotion(goodFolderDialog.FileName);
                finalFrame = aquaUI.aqua.aquaMotions[0].anims[0].moHeader.endFrame;
                aquaUI.aqua.aquaMotions.Clear();

                //Go through the motion, make edits to all keys at a specific frame time, save a copy, reset, and repeat with an incrmented frametime until the final frame
                for(int i = 0; i <= finalFrame; i++)
                {
                    aquaUI.aqua.ReadMotion(goodFolderDialog.FileName);

                    foreach (var keySet in aquaUI.aqua.aquaMotions[0].anims[0].motionKeys)
                    {
                        foreach (var data in keySet.keyData)
                        {
                            int frameIndex = -1;
                            for(int j = 0; j < data.frameTimings.Count; j++)
                            {
                                if(data.frameTimings[j] / 0x10 == i)
                                {
                                    frameIndex = j;
                                }
                            }

                            if(frameIndex != -1)
                            {
                                data.vector4Keys[frameIndex] = new System.Numerics.Vector4(5, 5, 5, 0);
                            }
                        }
                    }

                    aquaUI.aqua.WriteNIFLMotion(goodFolderDialog.FileName.Replace(".aqm", $"_{i}.aqm"));
                    aquaUI.aqua.aquaMotions.Clear();
                }



            }
        }

        private void importModelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var ctx = new Assimp.AssimpContext())
            {
                var formats = ctx.GetSupportedImportFormats().ToList();
                formats.Sort();

                OpenFileDialog openFileDialog;
                openFileDialog = new OpenFileDialog()
                {
                    Title = "Import model file",
                    Filter = ""
                };
                string tempFilter = "All supported formats|";
                string tempFilter2 = "";
                foreach(var str in formats)
                {
                    tempFilter += $"*{str};";
                    tempFilter2 += $"|(*{str})|*{str}";
                }
                openFileDialog.Filter = tempFilter + tempFilter2;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    aquaUI.aqua.aquaModels.Clear();
                    AquaUtil.ModelSet modelSet = new AquaUtil.ModelSet();
                    modelSet.models.Add(ModelImporter.AssimpAquaConvertFull(openFileDialog.FileName, 1, false, true));
                    aquaUI.aqua.aquaModels.Add(modelSet);
                    var outStr = Path.ChangeExtension(openFileDialog.FileName, ".aqp");
                    aquaUI.aqua.WriteNGSNIFLModel(outStr, outStr);
                    aquaUI.aqua.aquaModels.Clear();
                    AquaUIOpenFile(outStr);
                }
            }
        }

        private void pSOXVMDumpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog()
            {
                Title = "Select PSO xvm file(s)",
                Filter = "PSO xvm Files (*.xvm)|*.xvm",
                Multiselect = true
            };
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                //Read Xvms
                foreach (var file in openFileDialog.FileNames)
                {
                    PSOXVMConvert.ExtractXVM(file);
                }
            }
        }

        private void pSOXVRConvertToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog()
            {
                Title = "Select PSO xvr file(s)",
                Filter = "PSO xvr Files (*.xvr)|*.xvr|All Files (*.*)|*",
                Multiselect = true
            };
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                //Read Xvrs
                foreach (var file in openFileDialog.FileNames)
                {
                    PSOXVMConvert.ConvertLooseXVR(file);
                }
            }
        }

        private void importAXSToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog()
            {
                Title = "Select PS Nova axs file(s)",
                Filter = "PS Nova axs Files (*.axs,*.aif)|*.axs;*.aif|All Files (*.*)|*",
                Multiselect = true
            };
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                //System.Diagnostics.Debug.Listeners.Add(new System.Diagnostics.TextWriterTraceListener("C:\\axsout.txt"));
                //Read Xvrs
                List<string> failedFiles = new List<string>();
                foreach (var file in openFileDialog.FileNames)
                {
                    try
                    {
                        aquaUI.aqua.aquaModels.Clear();
                        AquaUtil.ModelSet set = new AquaUtil.ModelSet();
                        set.models.Add(AXSMethods.ReadAXS(file, out AquaNode aqn));
                        if (set.models[0] != null && set.models[0].vtxlList.Count > 0)
                        {
                            aquaUI.aqua.aquaModels.Add(set);
                            aquaUI.aqua.ConvertToNGSPSO2Mesh(false, false, false, true, false, false);

                            var outName = Path.ChangeExtension(file, ".aqp");
                            aquaUI.aqua.WriteNGSNIFLModel(outName, outName);
                            AquaUtil.WriteBones(Path.ChangeExtension(outName, ".aqn"), aqn);
                        }
                    }
                    catch (Exception exc)
                    {
                        failedFiles.Add(file);
                        failedFiles.Add(exc.Message);
                    }
                }

#if DEBUG
                File.WriteAllLines("C:\\failedFiiles.txt", failedFiles);
#endif
                System.Diagnostics.Debug.Unindent();
                System.Diagnostics.Debug.Flush();
            }
        }

        private void importAXSBoxTestToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AXSMethods.boxTest();
        }

        private void importAAIToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog()
            {
                Title = "Select PS Nova aai file(s)",
                Filter = "PS Nova aai Files (*.aai)|*.aai|All Files (*.*)|*",
                Multiselect = true
            };
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                foreach (var file in openFileDialog.FileNames)
                {
                    AAIMethods.ReadAAI(file);
                }
            }
        }

        private void convertPSNovaaxsaifToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog()
            {
                Title = "Select PS Nova axs/aif file(s)",
                Filter = "PS Nova model and texture Files (*.axs,*.aif)|*.axs;*.aif|PS Nova model files (*.axs)|*.axs|PS Nova Texture files (*.aif)|*.aif|All Files (*.*)|*",
                Multiselect = true
            };
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                List<string> failedFiles = new List<string>();
                foreach (var file in openFileDialog.FileNames)
                {
                    try
                    {
                        aquaUI.aqua.aquaModels.Clear();
                        AquaUtil.ModelSet set = new AquaUtil.ModelSet();
                        set.models.Add(AXSMethods.ReadAXS(file, out AquaNode aqn));
                        if (set.models[0] != null && set.models[0].vtxlList.Count > 0)
                        {
                            aquaUI.aqua.aquaModels.Add(set);
                            aquaUI.aqua.ConvertToNGSPSO2Mesh(false, false, false, true, false, false);

                            var outName = Path.ChangeExtension(file, ".aqp");
                            aquaUI.aqua.WriteNGSNIFLModel(outName, outName);
                            AquaUtil.WriteBones(Path.ChangeExtension(outName, ".aqn"), aqn);
                        }
                    }
                    catch 
                    {
                    }
                }
            }
        }

        private void proportionAQMJankTestToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CommonOpenFileDialog goodFolderDialog = new CommonOpenFileDialog()
            {
                Title = "Select proportion AQM ice",
            };
            if (goodFolderDialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                aquaUI.aqua.aquaMotions.Clear();
                var strm = new MemoryStream(File.ReadAllBytes(goodFolderDialog.FileName));
                var fVarIce = IceFile.LoadIceFile(strm);
                strm.Dispose();

                int frameToHit = 6;
                int tfmType = 3;
                int tfmType2 = 2;
                var vec4 = new System.Numerics.Vector4(5, 5, 5, 0);
                var vec4_2 = new System.Numerics.Vector4(0.707f, 0, -0.707f, 0);

                //Loop through files to get what we need
                for (int i = 0; i < fVarIce.groupTwoFiles.Length; i++)
                {
                    List<byte> file;
                    var name = IceFile.getFileName(fVarIce.groupTwoFiles[i]).ToLower();
                    switch (name)
                    {
                        case "pl_cmakemot_b_fc.aqm":
                            file = AdjustNormalKeysMotion(fVarIce, frameToHit, i, name, tfmType, vec4);
                            //file = AdjustNormalKeysMotion(fVarIce, frameToHit, i, name, tfmType2, vec4_2);
                            break;
                        case "pl_cmakemot_b_fh.aqm":
                            file = AdjustNormalKeysMotion(fVarIce, frameToHit, i, name, tfmType, vec4);
                            //file = AdjustNormalKeysMotion(fVarIce, frameToHit, i, name, tfmType2, vec4_2);
                            break;
                        case "pl_cmakemot_b_fh_hand.aqm":
                            file = AdjustNormalKeysMotion(fVarIce, frameToHit, i, name, tfmType, vec4);
                            //file = AdjustNormalKeysMotion(fVarIce, frameToHit, i, name, tfmType2, vec4_2);
                            break;
                        case "pl_cmakemot_b_fh_rb.aqm":
                            file = AdjustNormalKeysMotion(fVarIce, frameToHit, i, name, tfmType, vec4);
                            //file = AdjustNormalKeysMotion(fVarIce, frameToHit, i, name, tfmType2, vec4_2);
                            break;
                        case "pl_cmakemot_b_fh_rb_oldface.aqm":
                            file = AdjustNormalKeysMotion(fVarIce, frameToHit, i, name, tfmType, vec4);
                            //file = AdjustNormalKeysMotion(fVarIce, frameToHit, i, name, tfmType2, vec4_2);
                            break;
                        default:
                            break;
                    }
                    aquaUI.aqua.aquaMotions.Clear();
                }

                byte[] rawData = new IceV4File((new IceHeaderStructures.IceArchiveHeader()).GetBytes(), fVarIce.groupOneFiles, fVarIce.groupTwoFiles).getRawData(false, false);
                File.WriteAllBytes(goodFolderDialog.FileName + $"_{frameToHit}.ice", rawData);

                rawData = null;
                fVarIce = null;
            }
        }

        private List<byte> AdjustNormalKeysMotion(IceFile fVarIce, int frameToHit, int i, string name, int tfmType, System.Numerics.Vector4 vec4, int node = -1)
        {
            List<byte> file;
            aquaUI.aqua.ReadMotion(fVarIce.groupTwoFiles[i]);
            SetNormalKeysToValue(frameToHit, tfmType, vec4, node);
            file = aquaUI.aqua.GetNiflMotionBytes(name);
            file.InsertRange(0, (new IceHeaderStructures.IceFileHeader(name, (uint)file.Count)).GetBytes());
            fVarIce.groupTwoFiles[i] = file.ToArray();
            return file;
        }

        private void SetNormalKeysToValue(int frame, int keyType, System.Numerics.Vector4 value, int node = -1)
        {
            //Go through the motion, make edits to all keys at a specific frame time, save a copy, reset, and repeat with an incrmented frametime until the final frame

            foreach (var keySet in aquaUI.aqua.aquaMotions[0].anims[0].motionKeys)
            {
                if(node == -1 || keySet.mseg.nodeId == node)
                {
                    foreach (var data in keySet.keyData)
                    {
                        if (data.keyType == keyType)
                        {
                            int frameIndex = -1;
                            for (int j = 0; j < data.frameTimings.Count; j++)
                            {
                                if (data.frameTimings[j] / 0x10 == frame)
                                {
                                    frameIndex = j;
                                }
                            }

                            if (frameIndex != -1)
                            {
                                data.vector4Keys[frameIndex] = value;
                            }
                        }
                    }
                }
            }
        }

        //Unused data???
        private void proportionAQMFaceTestToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CommonOpenFileDialog goodFolderDialog = new CommonOpenFileDialog()
            {
                Title = "Select face proportion AQM ice",
            };
            if (goodFolderDialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                aquaUI.aqua.aquaMotions.Clear();
                var strm = new MemoryStream(File.ReadAllBytes(goodFolderDialog.FileName));
                var fVarIce = IceFile.LoadIceFile(strm);
                strm.Dispose();

                int frameToHit = 62;
                int tfmType = 3;
                int tfmType2 = 2;
                var vec4 = new System.Numerics.Vector4(5, 5, 5, 0);
                var vec4_2 = new System.Numerics.Vector4(0.707f, 0, -0.707f, 0);

                //Loop through files to get what we need
                for (int i = 0; i < fVarIce.groupTwoFiles.Length; i++)
                {
                    List<byte> file;
                    var name = IceFile.getFileName(fVarIce.groupTwoFiles[i]).ToLower();
                    switch (name)
                    {
                        case "pl_cmakemot_f_fd.aqm":
                            file = AdjustNormalKeysMotion(fVarIce, frameToHit, i, name, tfmType, vec4);
                            //file = AdjustNormalKeysMotion(fVarIce, frameToHit, i, name, tfmType2, vec4_2);
                            break;
                        case "pl_cmakemot_f_fh.aqm":
                            file = AdjustNormalKeysMotion(fVarIce, frameToHit, i, name, tfmType, vec4);
                            //file = AdjustNormalKeysMotion(fVarIce, frameToHit, i, name, tfmType2, vec4_2);
                            break;
                        case "pl_cmakemot_f_fn.aqm":
                            file = AdjustNormalKeysMotion(fVarIce, frameToHit, i, name, tfmType, vec4);
                            //file = AdjustNormalKeysMotion(fVarIce, frameToHit, i, name, tfmType2, vec4_2);
                            break;
                        default:
                            break;
                    }
                    aquaUI.aqua.aquaMotions.Clear();
                }

                byte[] rawData = new IceV4File((new IceHeaderStructures.IceArchiveHeader()).GetBytes(), fVarIce.groupOneFiles, fVarIce.groupTwoFiles).getRawData(false, false);
                File.WriteAllBytes(goodFolderDialog.FileName + $"_{frameToHit}.ice", rawData);

                rawData = null;
                fVarIce = null;
            }
        }

        private void proportionAQMNGSFaceTestToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CommonOpenFileDialog goodFolderDialog = new CommonOpenFileDialog()
            {
                Title = "Select NGS Face proportion AQM ice",
            };
            if (goodFolderDialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                aquaUI.aqua.aquaMotions.Clear();
                var strm = new MemoryStream(File.ReadAllBytes(goodFolderDialog.FileName));
                var fVarIce = IceFile.LoadIceFile(strm);
                strm.Dispose();
                
                int frameToHit = 158;
                int tfmType = 3;
                int tfmType2 = 2;
                int tfmType3 = 1;
                var vec4 = new System.Numerics.Vector4(5, 5, 5, 0);
                var vec4_2 = new System.Numerics.Vector4(0.707f, 0, -0.707f, 0);
                var vec4_3 = new System.Numerics.Vector4(5, 5, 5, 0);
                var node = -1;

                //Loop through files to get what we need
                for (int i = 0; i < fVarIce.groupTwoFiles.Length; i++)
                {
                    List<byte> file;
                    var name = IceFile.getFileName(fVarIce.groupTwoFiles[i]).ToLower();
                    if(name.Contains(".aqm"))
                    {
                        file = AdjustNormalKeysMotion(fVarIce, frameToHit, i, name, tfmType3, vec4_3, node); //pos
                        file = AdjustNormalKeysMotion(fVarIce, frameToHit, i, name, tfmType, vec4, node); //scale
                        file = AdjustNormalKeysMotion(fVarIce, frameToHit, i, name, tfmType2, vec4_2, node); //rot
                    }
                    aquaUI.aqua.aquaMotions.Clear();
                }

                byte[] rawData = new IceV4File((new IceHeaderStructures.IceArchiveHeader()).GetBytes(), fVarIce.groupOneFiles, fVarIce.groupTwoFiles).getRawData(false, false);
                File.WriteAllBytes(goodFolderDialog.FileName + $"_{frameToHit}.ice", rawData);

                rawData = null;
                fVarIce = null;
            }
        }

        private void testPointToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //General vertex transformation theory:
            //1. Subtract bone's translation from vertex translation
            //2. Assemble matrix from rotation and scale key data (we add translation again later)
            //3. Vector3.Transform() the point to the matrix
            //4. Readd bone translation

            Matrix4x4 mat = Matrix4x4.Identity;
            mat *= Matrix4x4.CreateScale(0.5f, 1, 1);

            var rotation = Matrix4x4.CreateFromQuaternion(new Quaternion(0.707107f, 0, 0, 0.707107f));

            mat *= rotation;

            //mat *= Matrix4x4.CreateTranslation(5, 5, 5);

            Vector3 vec3 = new Vector3(1, 1, 1);
            var vec3Transformed = Vector3.Transform(vec3, mat);
            var a = 0;
        }
    }
}

