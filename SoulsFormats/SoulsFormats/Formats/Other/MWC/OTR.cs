using System.Drawing;
using System.Numerics;

namespace SoulsFormats.Formats.Other.MWC
{
    public class OTR
    {
        public struct OTRHeader
        {
            public int int00;
            public int boundingBoxCount;
            public int faceCount; 
            public int vertCount;
        }

        public struct BoundingBox
        {
            public Vector3 minBounding;
            public Vector3 maxBounding;
            public int int18;
            public int int1C;

            public int int20;
            public int int24;
            public int int28;
            public int int2C;

            public int int30;
            public int int34;
            public int int38;
            public int unkCount1;

            public int unkOffset1;
            public int unkOffset2;
            public int unkOffset3;
            public int unkOffset4;

            public byte unkBt0;
            public byte unkBt1;
            public byte unkBt2;
            public byte unkBt3;
            public int unkOffset5;
            public int unk58;
            public int unk5C;

            public int unk60;
        }

        public struct Face
        {
            public int vertIndex0;
            public int vertIndex1;
            public int vertIndex2;
            public Vector3 normal;
            public Vector3 min;
            public Vector3 max;

            public int unkInt0;
            public int unkInt1;
            public int unkInt2;
            public float unkFlt;
        }

        public struct Vertex
        {
            public Vector3 translation;
            public int unkInt;
            public Color color;
        }
    }
}
