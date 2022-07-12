using System.Collections.Generic;

namespace AquaModelLibrary.Native.Fbx.Interfaces
{
    public interface IFbxExporter
    {
        void ExportToFile(AquaObject aqo, AquaNode aqn, List<AquaMotion> aqmList, string destinationFilePath, List<string> aqmNameList, bool includeMetadata);
    }
}
