using System.Collections.Generic;
using System.Numerics;

namespace AquaModelLibrary.Native.Fbx.Interfaces
{
    public interface IFbxExporter
    {
        void ExportToFile(AquaObject aqo, AquaNode aqn, List<AquaMotion> aqmList, string destinationFilePath, List<string> aqmNameList, List<Matrix4x4> instanceTransforms, bool includeMetadata);
    }
}
