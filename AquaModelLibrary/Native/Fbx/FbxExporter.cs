using System.Collections.Generic;
using System.Numerics;

namespace AquaModelLibrary.Native.Fbx
{
    public static class FbxExporter
    {
        public static void ExportToFile(AquaObject aqo, AquaNode aqn, List<AquaMotion> aqmList, string destinationFilePath, List<string> aqmNameList, List<Matrix4x4> instanceTransforms, bool includeMetadata) =>
            Native.FbxExporter.ExportToFile(aqo, aqn, aqmList, destinationFilePath, aqmNameList, instanceTransforms, includeMetadata);
    }
}
