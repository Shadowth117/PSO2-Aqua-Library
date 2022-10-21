using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace AquaModelLibrary.NNStructs.Structures
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
