using AquaModelLibrary.Helpers.PSO2;
using System.Numerics;

namespace AquaModelLibrary.Data.PSO2.Aqua.AquaEffectData
{
    public class EMITObject : AnimObject
    {
        public EMIT emit;
        public List<PTCLObject> ptcls = new List<PTCLObject>();

        public EMITObject() { }

        public EMITObject(List<Dictionary<int, object>> emitRaw)
        {
            emit = new EMIT();

            emit.unkVec3_00 = VTBFMethods.GetObject<Vector3>(emitRaw[0], 0x10);
            emit.unkVec3_10 = VTBFMethods.GetObject<Vector3>(emitRaw[0], 0x11);
            emit.unkVec3_20 = VTBFMethods.GetObject<Vector3>(emitRaw[0], 0x13);
            emit.unkVec3_40 = VTBFMethods.GetObject<Vector3>(emitRaw[0], 0x12);
            emit.unkVec3_50 = VTBFMethods.GetObject<Vector3>(emitRaw[0], 0x14);
            emit.unkVec3_60 = VTBFMethods.GetObject<Vector3>(emitRaw[0], 0x15);

            emit.startFrame = VTBFMethods.GetObject<int>(emitRaw[0], 0x1);
            emit.endFrame = VTBFMethods.GetObject<int>(emitRaw[0], 0x2);
            emit.int_78 = VTBFMethods.GetObject<int>(emitRaw[0], 0x20);
            emit.float_7C = VTBFMethods.GetObject<int>(emitRaw[0], 0x21);

            emit.int_80 = VTBFMethods.GetObject<byte>(emitRaw[0], 0x43);
            emit.int_84 = VTBFMethods.GetObject<int>(emitRaw[0], 0x5);
            emit.int_88 = VTBFMethods.GetObject<byte>(emitRaw[0], 0x32);
            emit.float_8C = VTBFMethods.GetObject<float>(emitRaw[0], 0x37);

            emit.float_90 = VTBFMethods.GetObject<float>(emitRaw[0], 0x35);
            emit.int_94 = VTBFMethods.GetObject<byte>(emitRaw[0], 0x33);
            emit.float_98 = VTBFMethods.GetObject<float>(emitRaw[0], 0x37);

            emit.int_B8 = -1;
            emit.int_BC = -1;

            emit.unkVec3_D0 = VTBFMethods.GetObject<Vector3>(emitRaw[0], 0x19);
            emit.int_E0 = VTBFMethods.GetObject<int>(emitRaw[0], 0x86);
        }
    }
    //Seems fairly variable, various items missing or not seemingly randomly in VTBF.
    //In VTBF, the pointer int16 is the number of nodes at and below this. Ie, the single EMIT + the PTCL count
    //In NIFL, these are laid out sequentially based on the count within the EFCT structure
    public struct EMIT
    {
        public Vector3 unkVec3_00; //0x10, Type 0x4A, 0x1 //Translation?
        public int reserve0;
        public Vector3 unkVec3_10; //0x11, Type 0x4A, 0x1 //Rotation?
        public int reserve1;
        public Vector3 unkVec3_20; //0x13, Type 0x4A, Count 0x1 //Scale?
        public int reserve2;

        public float float_30;
        public int curvOffset;
        public int curvCount;
        public int reserve3;

        public Vector3 unkVec3_40;  //0x12, Type 0x4A, Count 0x1
        public int reserve4;
        public Vector3 unkVec3_50;  //0x14, Type 0x4A, Count 0x1
        public int reserve5;
        public Vector3 unkVec3_60;  //0x15, Type 0x4A, Count 0x1
        public int reserve6;

        public float startFrame; //0x1, Type 0x8 //Just an educated guess based on sister formats for now.
        public float endFrame; //0x2, Type 0x8 //This becomes a float in NIFL variations rather than an int as it is in VTBF
        public int int_78;     //0x20, Type 0x8
        public float float_7C;  //0x21, Type 0x8

        public int int_80; //0x43, Type 0x4 //This became a full int in NIFL
        public int int_84; //0x5, Type 0x9
        public int int_88;             //0x32, Type 0x4
        public float float_8C;            //0x31, Type 0xA

        public float float_90;            //0x35, Type 0xA
        public int int_94;                //0x33, Type 0x4
        public float float_98;            //0x37, Type 0xA
        public short short_9C;
        public short short_9E;

        public short short_A0;
        public short short_A2;
        public short short_A4;
        public short short_A6;
        public int int_A8;
        public int field_AC;

        public int field_B0;
        public int field_B4;
        public int int_B8; //usually -1;
        public int int_BC;

        public Vector3 unkVec3_C0;
        public int reserve7;
        public Vector3 unkVec3_D0;           //This is maybe:  //0x19, Type 0x4A, Count 0x1 
        public int reserve8;

        public int int_E0;            //0x86, Type 0x8
        public int field_E4;
        public int field_E8;
        public int field_EC;

        public int ptclOffset;
        public int ptclCount;
        public int field_F8;
        public int field_FC;

        //****The below exist in VTBF, but seemingly aren't in NIFL...
        //0x3B, Type 0xA
        //0x3C, Type 0xA
        //0x36, Type 0x9
        //0x6, Type 0x9
    }
}
