using System.Numerics;

namespace AquaModelLibrary.Data.BillyHatcher
{
    public unsafe struct SetLightParam
    {
        /// <summary>
        /// 0x80 of these, always
        /// </summary>
        public LightParam[] lightParams;
    }
    public unsafe struct LightParam
    {
        public ushort usht0;
        public ushort usht1;
        public Vector3 lightDirection;

        public fixed byte color0[4]; //4 byte RGBA color
        public fixed byte color1[4]; //4 byte RGBA color
        public float unkRange0; //Range of some sort
        public float flt1C;

        public int int20;
        public float unkRange1; //Smaller than the first range
        public int int28;
        public int int2C;

        public int int30;
        public int int34;
        public int int38;
        public int int3C;
    }
}
