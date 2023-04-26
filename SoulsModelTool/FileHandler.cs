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
    public static class FileHandler
    {
        public static AquaUtil aqua = new AquaUtil();

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

                FbxExporter.ExportToFile(aqua.aquaModels[0].models[0], aqn, new List<AquaMotion>(), Path.ChangeExtension(file, ".fbx"), new List<string>(), new List<System.Numerics.Matrix4x4>(), false);
            }
        }
    }
}
