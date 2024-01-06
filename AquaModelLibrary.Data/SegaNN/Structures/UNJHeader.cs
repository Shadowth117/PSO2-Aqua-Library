using System.Numerics;

namespace  AquaModelLibrary.Data.NNStructs.Structures
{
    public struct UNJHeader
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
}
