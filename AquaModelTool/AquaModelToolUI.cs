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
                        aquaUI.aqua.ReadModel(file);

#if DEBUG
                        //aquaUI.aqua.aquaModels[0].models[0].splitVSETPerMesh();
                        var test = aquaUI.aqua.aquaModels[0].models[0];
                        test = aquaUI.aqua.aquaModels[0].models[0];
                        for(int i = 0; i < test.tstaList.Count; i++)
                        {
                            Console.WriteLine(i + " " + test.tstaList[i].texName.GetString());
                        }
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
                aqua.ConvertToClassicPSO2Mesh(false, false, false, false, false, false);

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

        private void getShadTexSheetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CommonOpenFileDialog goodFolderDialog = new CommonOpenFileDialog()
            {
                IsFolderPicker = true,
                Title = "Select a folder containing pso2 models (PRM has no shader and will not be read)",
            };
            if (goodFolderDialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                Dictionary<string, List<string>> shaderCombinations = new Dictionary<string, List<string>>();
                Dictionary<string, List<string>> shaderModelFiles = new Dictionary<string, List<string>>();
                List<string> files = new List<string>();
                string[] extensions = new string[] { "*.aqp", "*.aqo", "*.trp", "*.tro" };
                foreach (string s in extensions)
                {
                    files.AddRange(Directory.GetFiles(goodFolderDialog.FileName, s));
                }

                //Go through models we gathered
                foreach (string file in files)
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
                        if (!shaderCombinations.ContainsKey(key))
                        {
                            shaderCombinations[key] = new List<string>() { combination };
                            shaderModelFiles[key] = new List<string>() { Path.GetFileName(file) };
                        }
                        else
                        {
                            shaderCombinations[key].Add(combination);
                            shaderModelFiles[key].Add(Path.GetFileName(file));
                        }
                    }
                    model = null;
                    aquaUI.aqua.aquaModels.Clear();
                }

                //Sort the list so we don't get a mess
                var keys = shaderCombinations.Keys.ToList();
                keys.Sort();

                StringBuilder simpleOutput = new StringBuilder();
                StringBuilder advancedOutput = new StringBuilder();
                foreach (var key in keys)
                {
                    simpleOutput.AppendLine(key + "," + shaderCombinations[key][0]);

                    advancedOutput.AppendLine(key + "," + shaderCombinations[key][0] + "," + shaderModelFiles[key][0]);
                    for (int i = 1; i < shaderCombinations[key].Count; i++)
                    {
                        advancedOutput.AppendLine("," + shaderCombinations[key][i] + "," + shaderModelFiles[key][i]);
                    }
                    advancedOutput.AppendLine();
                }

                File.WriteAllText(goodFolderDialog.FileName + "\\" + "simpleOutput.csv", simpleOutput.ToString());
                File.WriteAllText(goodFolderDialog.FileName + "\\" + "detailedOutput.csv", advancedOutput.ToString());
            }
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
                Title = "Select a folder containing pso2 models (PRM has no shader and will not be read)",
            };
            if (goodFolderDialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                Dictionary<string, List<string>> shaderCombinations = new Dictionary<string, List<string>>();
                Dictionary<string, List<string>> shaderModelFiles = new Dictionary<string, List<string>>();
                Dictionary<string, List<string>> shaderDetails = new Dictionary<string, List<string>>();
                Dictionary<string, List<string>> shaderExtras = new Dictionary<string, List<string>>();
                List<string> files = new List<string>();
                string[] extensions = new string[] { "*.aqp", "*.aqo", "*.trp", "*.tro" };
                foreach (string s in extensions)
                {
                    files.AddRange(Directory.GetFiles(goodFolderDialog.FileName, s));
                }

                //Go through models we gathered
                foreach (string file in files)
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

                    var model = aquaUI.aqua.aquaModels[0].models[0];

                    //Go through all meshes in each model
                    if(model.objc.type > 0xC2A)
                    {
                        foreach (var shad in model.shadList)
                        {
                            if(shad is NGSAquaObject.NGSSHAD && (((NGSAquaObject.NGSSHAD)shad).shadDetailOffset != 0 || ((NGSAquaObject.NGSSHAD)shad).shadExtraOffset != 0))
                            {
                                NGSAquaObject.NGSSHAD ngsShad = (NGSAquaObject.NGSSHAD)shad;
                                string key = ngsShad.pixelShader.GetString() + " " + ngsShad.vertexShader.GetString();

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
                                if(ngsShad.shadExtraOffset != 0)
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

                                //Add them to the list
                                if (!shaderCombinations.ContainsKey(key))
                                {
                                    shaderCombinations[key] = new List<string>() { data };
                                    shaderModelFiles[key] = new List<string>() { Path.GetFileName(file) };
                                    shaderDetails[key] = new List<string>() { detData };
                                    shaderExtras[key] = new List<string>() { extData };
                                }
                                else
                                {
                                    shaderCombinations[key].Add(data);
                                    shaderModelFiles[key].Add(Path.GetFileName(file));
                                    shaderDetails[key].Add(detData);
                                    shaderExtras[key].Add(extData);
                                }
                            } else
                            {
                                continue;
                            }
                        }

                    }
                    model = null;
                    aquaUI.aqua.aquaModels.Clear();
                }

                //Sort the list so we don't get a mess
                var keys = shaderCombinations.Keys.ToList();
                keys.Sort();

                StringBuilder simpleOutput = new StringBuilder();
                StringBuilder advancedOutput = new StringBuilder();
                StringBuilder detailDictOutput = new StringBuilder();
                StringBuilder extraDictOutput = new StringBuilder();

                detailDictOutput.Append("public static Dictionary<string, SHADDetail> NGSShaderDetailPresets = new Dictionary<string, SHADDetail>(){\n");
                extraDictOutput.Append("public static Dictionary<string, List<SHADExtraEntry>> NGSShaderExtraPresets = new Dictionary<string, List<SHADExtraEntry>>(){\n");

                foreach (var key in keys)
                {
                    simpleOutput.Append("\n" + key + "\n" + shaderCombinations[key][0]);
                    detailDictOutput.Append(shaderDetails[key][0]);
                    extraDictOutput.Append(shaderExtras[key][0]);

                    advancedOutput.Append("\n" + key + "\n" + shaderCombinations[key][0] + "," + shaderModelFiles[key][0]);
                    for (int i = 1; i < shaderCombinations[key].Count; i++)
                    {
                        advancedOutput.AppendLine("," + shaderCombinations[key][i] + "," + shaderModelFiles[key][i]);
                        advancedOutput.AppendLine();
                    }
                    advancedOutput.AppendLine();
                }
                detailDictOutput.Append("};\n");
                extraDictOutput.Append("};");

                detailDictOutput.Append(extraDictOutput);
                File.WriteAllText(goodFolderDialog.FileName + "\\" + "simpleNGSOutput.csv", simpleOutput.ToString());
                File.WriteAllText(goodFolderDialog.FileName + "\\" + "detailedNGSOutput.csv", advancedOutput.ToString());
                File.WriteAllText(goodFolderDialog.FileName + "\\" + "presetDictionary.cs", detailDictOutput.ToString());
            }

            aquaUI.aqua.aquaModels.Clear();
        }

        private void parsePSO2TextFolderSelectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CommonOpenFileDialog goodFolderDialog = new CommonOpenFileDialog()
            {
                IsFolderPicker = true,
                Title = "Select pso2_bin",
            };
            if (goodFolderDialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                DumpTextFiles(Directory.GetFiles(goodFolderDialog.FileName, "*.text"));
            }
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
    }
}
