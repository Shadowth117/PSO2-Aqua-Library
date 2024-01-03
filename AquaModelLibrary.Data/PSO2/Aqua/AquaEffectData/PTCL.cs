using AquaModelLibrary.Data.DataTypes.SetLengthStrings;
using AquaModelLibrary.Helpers.PSO2;
using System.Numerics;

namespace AquaModelLibrary.Data.PSO2.Aqua.AquaEffectData
{
    public unsafe class PTCLObject : AnimObject
    {
        public PTCL ptcl;
        public PTCLStrings strings;

        public PTCLObject() { }
        public PTCLObject(List<Dictionary<int, object>> ptclRaw)
        {
            ptcl = new PTCL();

            ptcl.size = VTBFMethods.GetObject<Vector3>(ptclRaw[0], 0x19);
            ptcl.sizeRandom = VTBFMethods.GetObject<Vector3>(ptclRaw[0], 0x1A);
            ptcl.rotation = VTBFMethods.GetObject<Vector3>(ptclRaw[0], 0x11);
            ptcl.rotationRandom = VTBFMethods.GetObject<Vector3>(ptclRaw[0], 0x12);
            ptcl.rotationAdd = VTBFMethods.GetObject<Vector3>(ptclRaw[0], 0x14);
            ptcl.rotationAddRandom = VTBFMethods.GetObject<Vector3>(ptclRaw[0], 0x15);
            ptcl.direction = VTBFMethods.GetObject<Vector3>(ptclRaw[0], 0x44);
            ptcl.directionRandom = VTBFMethods.GetObject<Vector3>(ptclRaw[0], 0x45);
            ptcl.gravitationalAccel = VTBFMethods.GetObject<Vector3>(ptclRaw[0], 0x50);
            ptcl.externalAccel = VTBFMethods.GetObject<Vector3>(ptclRaw[0], 0x51);
            ptcl.externalAccelRandom = VTBFMethods.GetObject<Vector3>(ptclRaw[0], 0x5C);

            ptcl.float_B0 = VTBFMethods.GetObject<float>(ptclRaw[0], 0x56);
            ptcl.float_B4 = VTBFMethods.GetObject<float>(ptclRaw[0], 0x57);
            ptcl.float_B8 = VTBFMethods.GetObject<float>(ptclRaw[0], 0x52);
            ptcl.float_BC = VTBFMethods.GetObject<float>(ptclRaw[0], 0x53);

            ptcl.int_C0 = VTBFMethods.GetObject<int>(ptclRaw[0], 0x5);
            ptcl.float_C4 = VTBFMethods.GetObject<float>(ptclRaw[0], 0x2);
            ptcl.byte_C8 = VTBFMethods.GetObject<byte>(ptclRaw[0], 0x41);
            ptcl.byte_C9 = VTBFMethods.GetObject<byte>(ptclRaw[0], 0x98);
            ptcl.byte_CA = VTBFMethods.GetObject<byte>(ptclRaw[0], 0x43);
            ptcl.byte_CB = VTBFMethods.GetObject<byte>(ptclRaw[0], 0x4F);
            ptcl.float_CC = VTBFMethods.GetObject<float>(ptclRaw[0], 0x4E);

            ptcl.speed = VTBFMethods.GetObject<float>(ptclRaw[0], 0x46);
            ptcl.speedRandom = VTBFMethods.GetObject<float>(ptclRaw[0], 0x47);

            ptcl.float_E0 = 1.0f;

            var color = VTBFMethods.GetObject<byte[]>(ptclRaw[0], 0x42);
            for (int i = 0; i < 0x4; i++)
            {
                if (i < color.Length)
                {
                    ptcl.color[i] = color[i];
                }
                else
                {
                    ptcl.color[i] = 0;
                }
            }

            ptcl.int_F0 = VTBFMethods.GetObject<byte>(ptclRaw[0], 0x4B);
            ptcl.int_F4 = VTBFMethods.GetObject<short>(ptclRaw[0], 0x4C);
            ptcl.int_F8 = VTBFMethods.GetObject<short>(ptclRaw[0], 0x4D);
            ptcl.byte_FC = VTBFMethods.GetObject<byte>(ptclRaw[0], 0x61);
            ptcl.byte_FD = VTBFMethods.GetObject<byte>(ptclRaw[0], 0x62);
            ptcl.byte_FE = VTBFMethods.GetObject<byte>(ptclRaw[0], 0x67);
            ptcl.byte_FF = VTBFMethods.GetObject<byte>(ptclRaw[0], 0x5D);

            ptcl.int_100 = VTBFMethods.GetObject<byte>(ptclRaw[0], 0x64);
            ptcl.int_104 = VTBFMethods.GetObject<byte>(ptclRaw[0], 0x66);
            ptcl.int_108 = VTBFMethods.GetObject<byte>(ptclRaw[0], 0x68);
            ptcl.short_10C = VTBFMethods.GetObject<byte>(ptclRaw[0], 0x5E);
            ptcl.short_10E = VTBFMethods.GetObject<byte>(ptclRaw[0], 0x5F);

            ptcl.field_110 = VTBFMethods.GetObject<int>(ptclRaw[0], 0x6);
            ptcl.field_114 = VTBFMethods.GetObject<short>(ptclRaw[0], 0x55);

            ptcl.float_120 = VTBFMethods.GetObject<float>(ptclRaw[0], 0x54);
            ptcl.float_124 = VTBFMethods.GetObject<float>(ptclRaw[0], 0x48);
            ptcl.float_128 = VTBFMethods.GetObject<float>(ptclRaw[0], 0x49);
            ptcl.float_12C = VTBFMethods.GetObject<float>(ptclRaw[0], 0x4A);

            ptcl.float_130 = VTBFMethods.GetObject<float>(ptclRaw[0], 0x88);

            strings = new PTCLStrings();
            strings.assetName.SetBytes(VTBFMethods.GetObject<byte[]>(ptclRaw[0], 0x34));
            strings.subDirectory.SetBytes(VTBFMethods.GetObject<byte[]>(ptclRaw[0], 0x40));
            strings.diffuseTex.SetBytes(VTBFMethods.GetObject<byte[]>(ptclRaw[0], 0x63));
            strings.opacityTex.SetBytes(VTBFMethods.GetObject<byte[]>(ptclRaw[0], 0x65));
            //strings.unkString //May be unused in vtbf?
        }
    }
    public unsafe struct PTCL
    {
        public Vector3 size; //0x19, Type 0x4A, 0x1
        public int reserve0;
        public Vector3 sizeRandom; //0x1A, Type 0x4A, 0x1
        public int reserve1;
        public Vector3 rotation; //0x11, Type 0x4A, 0x1
        public int reserve2;
        public Vector3 rotationRandom; //0x12, Type 0x4A, 0x1
        public int reserve3;
        public Vector3 rotationAdd; //0x14, Type 0x4A, 0x1
        public int reserve4;
        public Vector3 rotationAddRandom; //0x15, Type 0x4A, 0x1
        public int reserve5;
        public Vector3 direction; //0x44, Type 0x4A, 0x1
        public int reserve6;
        public Vector3 directionRandom; //0x45, Type 0x4A, 0x1
        public int reserve7;
        public Vector3 gravitationalAccel; //0x50, Type 0x4A, 0x1
        public int reserve8;
        public Vector3 externalAccel; //0x51, Type 0x4A, 0x1
        public int reserve9;
        public Vector3 externalAccelRandom; //0x5C, Type 0x4A, 0x1
        public int reserve10;

        public float float_B0; //0x56, Type 0xA
        public float float_B4; //0x57, Type 0xA
        public float float_B8; //0x52, Type 0xA
        public float float_BC; //0x53, Type 0xA

        public int int_C0; //0x5, Type 0x9
        public float float_C4; //0x2, Type 0xA
        public byte byte_C8; //0x41, Type 0x4
        public byte byte_C9; //0x98, Type 0x4
        public byte byte_CA; //0x43, Type 0x4
        public byte byte_CB; //0x4F, Type 0x4
        public float float_CC; //0x4E, Type 0xA  

        public float speed; //0x46, Type 0xA
        public float speedRandom; //0x47, Type 0xA
        public float field_D8;
        public float field_DC;

        public float float_E0;
        public float field_E4;
        public float field_E8;
        public fixed byte color[4]; //0x42, Type 0xC

        public int int_F0; //0x4B, Type 0x4
        public int int_F4; //0x4C, Type 0x5
        public int int_F8; //0x4D, Type 0x5
        public byte byte_FC; //0x61, Type 0x4
        public byte byte_FD; //0x62, Type 0x4
        public byte byte_FE; //0x67, Type 0x4
        public byte byte_FF; //0x5D, Type 0x1

        public int int_100; //0x64, Type 0x4 //int in NIFL
        public int int_104; //0x66, Type 0x4 //int in NIFL
        public int int_108; //0x68, Type 0x4 //int in NIFL
        public short short_10C; //0x5E, Type 0x4 //short in NIFL?
        public short short_10E; //0x5F, Type 0x4 //short in NIFL?

        public int field_110; //0x6, Type 0x9
        public int field_114; //0x55, Type 0x6 // int in NIFL?
        public int field_118;
        public int field_11C;

        public float float_120; //0x54, Type 0xA //Maybe??
        public float float_124; //0x48, Type 0xA
        public float float_128; //0x49, Type 0xA
        public float float_12C; //0x4A, Type 0xA

        public float float_130; //0x88, Type 0xA
        public float float_134;
        public float float_138;
        public float float_13C;

        public int ptclStringsOffset;
        public int curvOffset;
        public int curvCount;
        public int field_14C;

        //***** Used differently in NIFL:
        //**
        //PTCL Strings are in a substruct, stored by order.
        //PSO2Stringx30 //0x34, Type 0x2 //Model name
        //PSO2Stringx30 //0x40, Type 0x2 //Vestigial? Subdirectory reference in VTBF
        //PSO2String //0x63, Type 0x2 //Diffuse
        //PSO2String //0x65, Type 0x2 //Opacity? Same as above generally
        //**

    }

    public struct PTCLStrings
    {
        public PSO2String assetName;
        public PSO2String subDirectory; //Can be aqm
        public PSO2String diffuseTex; //Can be aqv
        public PSO2String opacityTex;
        public PSO2String unkString;
    }
}
