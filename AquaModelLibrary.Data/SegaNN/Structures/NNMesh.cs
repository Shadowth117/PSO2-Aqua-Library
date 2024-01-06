using System.Numerics;

namespace  AquaModelLibrary.Data.NNStructs.Structures
{
    public struct NNS_MeshSetInfo
    {
        public byte unkByte1;
        public byte unkByte2;
        public ushort unkShort;
        public int directMeshCount;
        public int directMeshOffset;
        public int indexedMeshCount;
        public int indexedMeshOffset;
    }

    public struct NNS_MeshSet
    {
        public Vector3 center;
        public float radius;
        public int baseNode;
        public int matrixId; //Unused?
        public int materialId;
        public int vtxlId;
        public int faceSetId;
        public int shaderId;
    }
}
