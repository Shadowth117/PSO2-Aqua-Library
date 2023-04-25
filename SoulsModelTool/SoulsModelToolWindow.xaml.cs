using AquaModelLibrary.Extra;
using AquaModelLibrary.Native.Fbx;
using AquaModelLibrary;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Windows;
using static AquaModelLibrary.Utility.AquaUtilData;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Reflection;
using System.IO;
using Path = System.IO.Path;

namespace SoulsModelTool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class SoulsModelToolWindow : Window
    {
        public AquaUtil aqua = new AquaUtil();
        public SoulsModelToolWindow(List<string> paths, List<SoulsActionModifiers> modifiers)
        {
            InitializeComponent();
            foreach (var modifier in modifiers)
            {
                switch(modifier)
                {
                    case SoulsActionModifiers.mirror:
                        mirrorCB.IsChecked = true;
                        break;
                    case SoulsActionModifiers.useMetadata:
                        useMetaDataCB.IsChecked = true;
                        break;
                    case SoulsActionModifiers.meshNameIsMatName:
                        matNamesToMeshCB.IsChecked = true;
                        break;
                    case SoulsActionModifiers.transformMesh:
                        transformMeshCB.IsChecked = true;
                        break;
                }
            }
        }

        public List<SoulsActionModifiers> GetModifiers()
        {
            List<SoulsActionModifiers> modifiers = new List<SoulsActionModifiers>();

            if(useMetaDataCB.IsChecked == true)
            {
                modifiers.Add(SoulsActionModifiers.useMetadata);
            }
            if (mirrorCB.IsChecked == true)
            {
                modifiers.Add(SoulsActionModifiers.mirror);
            }
            if (matNamesToMeshCB.IsChecked == true)
            {
                modifiers.Add(SoulsActionModifiers.meshNameIsMatName);
            }
            if (transformMeshCB.IsChecked == true)
            {
                modifiers.Add(SoulsActionModifiers.transformMesh);
            }

            return modifiers;
        }

        private void ConvertModelToFBX(object sender, RoutedEventArgs e)
        {
            foreach (var mod in GetModifiers())
            {
                switch (mod)
                {
                    case SoulsActionModifiers.useMetadata:
                        SoulsConvert.useMetaData = true;
                        break;
                    case SoulsActionModifiers.mirror:
                        SoulsConvert.mirrorMesh = true;
                        break;
                    case SoulsActionModifiers.meshNameIsMatName:
                        SoulsConvert.applyMaterialNamesToMesh = true;
                        break;
                    case SoulsActionModifiers.transformMesh:
                        SoulsConvert.transformMesh = true;
                        break;
                }
            }
            OpenFileDialog openFileDialog = new OpenFileDialog()
            {
                Title = "Select From Software flver, MDL4, TPF, or BND file(s)",
                Filter = "From Software flver, MDL4, or BND Files (*.flver, *.flv, *.mdl, *.*bnd, *.dcx, *.tpf)|*.flver;*.flv;*.mdl;*.*bnd;*.dcx;*.tpf|All Files (*.*)|*",
                Multiselect = true
            };
            if (openFileDialog.ShowDialog() == true)
            {


                foreach (var file in openFileDialog.FileNames)
                {
                    SoulsConvert.ConvertFile(file);
                }
            }
        }
        private void ConvertCMSHToFBX(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new()
            {
                Title = "Select BluePoint cmsh file(s)",
                Filter = "BluePoint cmsh Files (*.cmsh, *.cmdl)|*.cmsh;*.cmdl|All Files (*.*)|*",
                Multiselect = true
            };
            if (openFileDialog.ShowDialog() == true)
            {
                foreach (var file in openFileDialog.FileNames)
                {
                    aqua.aquaModels.Clear();
                    ModelSet set = new ModelSet();
                    set.models.Add(BluePointConvert.ReadCMDL(file, out AquaNode aqn));
                    if (set.models[0] != null && set.models[0].vtxlList.Count > 0)
                    {
                        aqua.aquaModels.Add(set);
                        aqua.ConvertToNGSPSO2Mesh(false, false, false, true, false, false, false, true);
                        set.models[0].ConvertToLegacyTypes();
                        set.models[0].CreateTrueVertWeights();

                        FbxExporter.ExportToFile(aqua.aquaModels[0].models[0], aqn, new List<AquaMotion>(), Path.ChangeExtension(file, ".fbx"), new List<string>(), new List<Matrix4x4>(), false);
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
            foreach (var mod in GetModifiers())
            {
                switch (mod)
                {
                    case SoulsActionModifiers.useMetadata:
                        SoulsConvert.useMetaData = true;
                        break;
                    case SoulsActionModifiers.mirror:
                        SoulsConvert.mirrorMesh = true;
                        break;
                    case SoulsActionModifiers.meshNameIsMatName:
                        SoulsConvert.applyMaterialNamesToMesh = true;
                        break;
                    case SoulsActionModifiers.transformMesh:
                        SoulsConvert.transformMesh = true;
                        break;
                }
            }
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
    }
}
