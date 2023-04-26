using AquaModelLibrary.Extra;
using AquaModelLibrary;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Reflection;
using System.IO;
using Path = System.IO.Path;
using Newtonsoft.Json;

namespace SoulsModelTool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class SoulsModelToolWindow : Window
    {
        public string settingsPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\";
        public string settingsFile = "SoulsSettings.json";
        public SMTSetting smtSetting = new SMTSetting();
        public AquaUtil aqua = new AquaUtil();
        JsonSerializerSettings jss = new JsonSerializerSettings() { Formatting = Formatting.Indented };
        public SoulsModelToolWindow(List<string> paths, SMTSetting _smtSetting)
        {
            smtSetting = _smtSetting;
            InitializeComponent();
            useMetaDataCB.IsChecked = smtSetting.useMetaData;
            mirrorCB.IsChecked = smtSetting.mirrorMesh;
            matNamesToMeshCB.IsChecked = smtSetting.applyMaterialNamesToMesh;
            transformMeshCB.IsChecked = smtSetting.transformMesh;
        }

        private void ConvertModelToFBX(object sender, RoutedEventArgs e)
        {
            SoulsConvert.useMetaData = smtSetting.useMetaData;
            SoulsConvert.mirrorMesh = smtSetting.mirrorMesh;
            SoulsConvert.applyMaterialNamesToMesh = smtSetting.applyMaterialNamesToMesh;
            SoulsConvert.transformMesh = smtSetting.transformMesh;

            OpenFileDialog openFileDialog = new OpenFileDialog()
            {
                Title = "Select From Software flver, MDL4, TPF, BND or BluePoint CMDL or CMSH file(s)",
                Filter = "From Software flver, MDL4, or BND Files (*.flver, *.flv, *.mdl, *.*bnd, *.dcx, *.tpf, *.cmsh, *.cmdl)|*.flver;*.flv;*.mdl;*.*bnd;*.dcx;*.tpf;*.cmsh;*.cmdl|All Files (*.*)|*",
                Multiselect = true
            };
            if (openFileDialog.ShowDialog() == true)
            {


                foreach (var file in openFileDialog.FileNames)
                {
                    string ext = Path.GetExtension(file);
                    if (ext == ".cmsh" || ext == ".cmdl")
                    {
                        FileHandler.ConvertBluepointModel(file);
                    }
                    else
                    {
                        SoulsConvert.ConvertFile(file);
                    }
                }
            }
        }
        
        private void GenerateMCPMCG(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog goodFolderDialog = new()
            {
                IsFolderPicker = true,
                Multiselect = true,
                Title = "Select Demon's Souls m**_**_**_** folders for connected areas",
            };
            if (goodFolderDialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                AquaModelLibrary.Extra.FromSoft.SoulsMapMetadataGenerator.Generate(goodFolderDialog.FileNames.ToList(), out var mcCombo);
            }
        }

        private void ConvertFBXToDeSModel(object sender, RoutedEventArgs e)
        {
            SoulsConvert.useMetaData = smtSetting.useMetaData;
            SoulsConvert.mirrorMesh = smtSetting.mirrorMesh;
            SoulsConvert.applyMaterialNamesToMesh = smtSetting.applyMaterialNamesToMesh;
            SoulsConvert.transformMesh = smtSetting.transformMesh;

            using (var ctx = new Assimp.AssimpContext())
            {
                var formats = ctx.GetSupportedImportFormats().ToList();
                formats.Sort();

                OpenFileDialog openFileDialog = new()
                {
                    Title = "Import model file, fbx recommended (output .aqp and .aqn will write to import directory)",
                    Filter = ""
                };
                string tempFilter = "(*.fbx,*.dae,*.glb,*.gltf,*.pmx,*.smd)|*.fbx;*.dae;*.glb;*.gltf;*.pmx;*.smd";
                string tempFilter2 = "";
                openFileDialog.Filter = tempFilter + tempFilter2;

                if (openFileDialog.ShowDialog() == true)
                {
                    if (!File.Exists(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "DeSMtdLayoutData.bin")))
                    {
                        MessageBox.Show("No DeSMtdLayoutData.bin detected! Please select a PS3 Demon's Souls game folder!");
                        var browseDialog = new CommonOpenFileDialog()
                        {
                            Title = "Open PS3 Demon's Souls root folder",
                            IsFolderPicker = true,
                        };

                        if (browseDialog.ShowDialog() == CommonFileDialogResult.Ok)
                        {
                            SoulsConvert.GetDeSLayoutMTDInfo(browseDialog.FileName);
                        }
                        else
                        {
                            MessageBox.Show("You MUST have an DeSMtdLayoutData.bin file to proceed!");
                            return;
                        }
                    }

                    AquaUtil aqua = new AquaUtil();
                    var ext = Path.GetExtension(openFileDialog.FileName);
                    var outStr = openFileDialog.FileName.Replace(ext, "_out.flver");
                    SoulsConvert.ConvertModelToFlverAndWrite(openFileDialog.FileName, outStr, 1, true, true, SoulsConvert.SoulsGame.DemonsSouls);
                }
            }
        }

        private void smtSettingSet(object sender, RoutedEventArgs e)
        {
            smtSetting.useMetaData = (bool)useMetaDataCB.IsChecked;
            smtSetting.mirrorMesh = (bool)mirrorCB.IsChecked;
            smtSetting.applyMaterialNamesToMesh = (bool)matNamesToMeshCB.IsChecked;
            smtSetting.transformMesh = (bool)transformMeshCB.IsChecked;
            string smtSettingText = JsonConvert.SerializeObject(smtSetting, jss);
            File.WriteAllText(settingsPath + settingsFile, smtSettingText);
        }
    }
}
