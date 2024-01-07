
using AquaModelLibrary.Data.PSO2.Aqua;
using System.Numerics;

namespace AquaModelLibrary.Core.General
{
    public class FbxExporterNative
    {
        public static AquaModelLibrary.Objects.Processing.Fbx.FbxExporterCore FbxExporter = new Objects.Processing.Fbx.FbxExporterCore();
        public static void ExportToFile(AquaObject aqo, AquaNode aqn, List<AquaMotion> aqmList, string destinationFilePath, List<string> aqmNameList, List<Matrix4x4> instanceTransforms, bool includeMetadata) =>
            FbxExporter.ExportToFile(aqo, aqn, aqmList, destinationFilePath, aqmNameList, instanceTransforms, includeMetadata);
        public static void ExportToFileSets(List<AquaObject> aqoList, List<AquaNode> aqnList, List<string> modelNames, string destinationFilePath, List<List<Matrix4x4>> instanceTransformsList, bool includeMetadata) =>
            FbxExporter.ExportToFileSets(aqoList, aqnList, modelNames, destinationFilePath, instanceTransformsList, includeMetadata);
    }
}
