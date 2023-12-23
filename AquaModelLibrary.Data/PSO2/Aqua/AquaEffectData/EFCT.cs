using AquaModelLibrary.Data.PSO2.Aqua.SetLengthStrings;
using System.Numerics;

namespace AquaModelLibrary.Data.PSO2.Aqua.AquaEffectData
{
    public class EFCTObject : AnimObject
    {
        public EFCT efct;
        public List<EMITObject> emits = new List<EMITObject>();
    }

    //In VTBF, the pointer int16 is the number of nodes in the file. Ie, the single EFCT + the EMIT count
    public unsafe struct EFCT
    {
        public Vector3 unkVec3_0; //0x10, Type 0x4A, 0x1 //Translation?
        public int reserve0;
        public Vector3 unkVec3_1; //0x11, Type 0x4A, 0x1 //Rotation?
        public int reserve1;
        public Vector3 unkVec3_2; //Scale?
        public int reserve2;

        public float float_30;
        public int curvOffset;
        public int curvCount;
        public int reserve3;

        public float startFrame; //0x1, Type 0x8 //Just an educated guess based on sister formats for now.
        public float endFrame; //0x2, Type 0x8 //This becomes a float in NIFL variations rather than an int as it is in VTBF
        public int int_48; //0x3, Type 0x8
        public fixed byte color[4]; //0x42, Type 0xC

        public int int_50;
        public int boolInt_54;    //0x4, Type 0x1
        public int boolInt_58;    //0x0, Type 0x1
        public int boolInt_5C;    //0x7, Type 0x1

        public float float_60; //0x91, Type 0x8
        public float float_64; //0x92, Type 0x8

        public PSO2Stringx30 soundName; //0x90, Type 0x2
        public int emitOffset;
        public int emitCount;
    }

}
