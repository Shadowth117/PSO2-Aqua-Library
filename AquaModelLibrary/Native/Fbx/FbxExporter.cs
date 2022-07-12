using System.Collections.Generic;

namespace AquaModelLibrary.Native.Fbx
{
    public static class FbxExporter
    {
        public static void ExportToFile(AquaObject aqo, AquaNode aqn, List<AquaMotion> aqmList, string destinationFilePath, List<string> aqmNameList, bool includeMetadata) =>
            Native.FbxExporter.ExportToFile(aqo, aqn, aqmList, destinationFilePath, aqmNameList, includeMetadata);
    }
}
