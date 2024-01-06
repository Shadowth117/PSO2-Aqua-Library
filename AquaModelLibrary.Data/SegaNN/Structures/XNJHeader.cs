using System.Numerics;

namespace  AquaModelLibrary.Data.NNStructs.Structures
{
    public struct XNJHeader
    {
        public Vector3 center;
        public float radius;

        public int matCount;
        public int matOffset;
        public int vertGroupCount;
        public int vertGroupOffset;

        public int faceGroupCount;
        public int faceGroupOffset;
        public int boneCount;
        public int boneTreeDepth;

        public int boneOffset;
        public int int_34;
        public int drawCount;
        public int drawOffset;
    }

    public struct MysteryObject
    {
        public int unk0;
        public int unk1;
        public Vector3 vec3;
    }
}
