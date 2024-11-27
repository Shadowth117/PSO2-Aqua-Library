using System.Numerics;

namespace AquaModelLibrary.Data.BillyHatcher.Collision
{
    public struct CSDYFace
    {
        public ushort index0;
        public ushort index1;
        public ushort index2;
        public ushort index3;
        public int int_08;
        public Vector3 faceNormal;
        public CollisionBounds faceBounds;
    }
}
