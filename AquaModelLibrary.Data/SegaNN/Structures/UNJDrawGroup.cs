using System.Numerics;

namespace  AquaModelLibrary.Data.NNStructs.Structures
{
    public struct UNJDrawGroup
    {
        public byte bt_00;
        public byte bt_01;
        public short sht_02;
        public int directDrawCount;
        public int directDrawOffset;
        public int indexedDrawCount;
        public int indexedDrawOffset;
    }

    public struct UNJDirectDrawMesh
    {
        public Vector3 center;
        public float radius;
        public int topLevelBone;
        public int matrixId;
        public int matId;
        public int groupId;
        public int shaderId;
    }
}
