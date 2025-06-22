
using AquaModelLibrary.Data.PSO2.Aqua;
using System.Numerics;

namespace AquaModelLibrary.Core.General
{
    public class FbxExporterNative
    {
        public static AquaModelLibrary.Objects.Processing.Fbx.FbxExporterCore FbxExporter = new Objects.Processing.Fbx.FbxExporterCore();
        public static void ExportToFile(AquaObject aqo, AquaNode aqn, List<AquaMotion> aqmList, string destinationFilePath, List<string> aqmNameList, List<Matrix4x4> instanceTransforms, bool includeMetadata, int coordSystem = 0, bool excludeTangentBinormal = true)
        {
            var cloneAqo = aqo.Clone();
            if(excludeTangentBinormal)
            {
                foreach (var vtxl in cloneAqo.vtxlList)
                {
                    vtxl.vertTangentList.Clear();
                    vtxl.vertBinormalList.Clear();
                }
            }
            FbxExporter.ExportToFile(cloneAqo, aqn, aqmList, destinationFilePath, aqmNameList, instanceTransforms, includeMetadata, coordSystem);
        }
        public static void ExportToFileSets(List<AquaObject> aqoList, List<AquaNode> aqnList, List<string> modelNames, string destinationFilePath, List<List<Matrix4x4>> instanceTransformsList, bool includeMetadata, int coordSystem = 0, bool excludeTangentBinormal = true)
        {
            List<AquaObject> cloneAqoList = aqoList.Select(aqo => aqo.Clone()).ToList();
            if(excludeTangentBinormal)
            {
                foreach(var aqo in cloneAqoList)
                {
                    foreach(var vtxl in aqo.vtxlList)
                    {
                        vtxl.vertTangentList.Clear();
                        vtxl.vertBinormalList.Clear();
                    }
                }
            }
            FbxExporter.ExportToFileSets(cloneAqoList, aqnList, modelNames, destinationFilePath, instanceTransformsList, includeMetadata, coordSystem);
        }
           
    }
}
