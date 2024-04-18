using System.Numerics;

namespace AquaModelLibrary.Data.BluePoint.CTAL
{
    public struct CTALEntry
    {
        public int hash;
        public Vector2 upperLeftPoint;
        public Vector2 lowerLeftPoint;
        public Vector2 lowerRightPoint;
        public Vector2 upperRightPoint;
        public int unk0;
    }
}
