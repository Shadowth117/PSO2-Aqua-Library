using AquaModelLibrary;
using AquaModelLibrary.Extra;
using AquaModelLibrary.Native.Fbx;
using Microsoft.VisualBasic;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AquaModelLibrary.Utility.AquaUtilData;

namespace SoulsModelTool
{
    public enum SoulsActionModifiers
    {
        mirror = 0,
        useMetadata = 1,
        meshNameIsMatName = 2,
        transformMesh = 3,
    }

    public static class FileHandler
    {
        public static AquaUtil aqua = new AquaUtil();
        public static void OpenAndConvertFlver(List<SoulsActionModifiers> modifiers)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog()
            {
                Title = "Select From Software flver, MDL4, TPF, BND or BluePoint CMDL or CMSH file(s)",
                Filter = "From Software flver, MDL4, or BND Files (*.flver, *.flv, *.mdl, *.*bnd, *.dcx, *.tpf, *.cmsh, *.cmdl)|*.flver;*.flv;*.mdl;*.*bnd;*.dcx;*.tpf;*.cmsh;*.cmdl|All Files (*.*)|*",
                Multiselect = true
            };
            if (openFileDialog.ShowDialog() == true)
            {
                ConvertFlver(openFileDialog.FileNames, modifiers);
            }
        }

        public static void ConvertFlver(string[] filenames, List<SoulsActionModifiers> modifiers)
        {
            foreach (var mod in modifiers)
            {
                switch(mod)
                {
                    case SoulsActionModifiers.mirror:
                        SoulsConvert.mirrorMesh = true;
                        break;
                    case SoulsActionModifiers.useMetadata:
                        SoulsConvert.useMetaData = true;
                        break;
                    case SoulsActionModifiers.meshNameIsMatName:
                        SoulsConvert.applyMaterialNamesToMesh = true;
                        break;
                    case SoulsActionModifiers.transformMesh:
                        SoulsConvert.transformMesh = true;
                        break;
                    default:
                        break;
                }
            }
            foreach (var file in filenames)
            {
                string ext = Path.GetExtension(file);
                if(ext == ".cmsh" || ext == ".cmdl")
                {
                    ConvertBluepointModel(file);
                } else
                {
                    SoulsConvert.ConvertFile(file);
                }
            }
        }

        public static void ConvertBluepointModel(string file)
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

                FbxExporter.ExportToFile(aqua.aquaModels[0].models[0], aqn, new List<AquaMotion>(), Path.ChangeExtension(file, ".fbx"), new List<string>(), false);
            }
        }
    }
}
