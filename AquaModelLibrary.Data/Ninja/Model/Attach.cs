using AquaModelLibrary.Data.PSO2.Aqua;
using AquaModelLibrary.Data.PSO2.Aqua.AquaObjectData;
using System.Numerics;

namespace AquaModelLibrary.Data.Ninja.Model
{
    public interface Attach
    {
        public bool HasWeights();
        public void GetVertexData(int nodeId, VTXL vtxl, Matrix4x4 transform);
        public void GetFaceData(int nodeId, VTXL vtxl, AquaObject aqo);
        public void Write(List<byte> outBytes, List<int> POF0Offsets);
    }
}
