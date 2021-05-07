using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static AquaModelLibrary.AquaCommon;

namespace AquaModelLibrary
{
    //PSO2 Implementation of Glitter particle effect files. Shares striking similarity to Project Diva variations.
    //May be entirely different than the NIFL variation 
    //Seemingly, the file seems to be an efct header followed by an emit nodes, their animations and particle nodes with their animations.
    //There should be at least one EFCT, one EMIT, and one PTCL per file while they must all have ANIMs, null or not.
    public unsafe class AquaEffect
    {
        public NIFL nifl;
        public REL0 rel0;
        public EFCTObject efct;
        public NOF0 nof0;
        public NEND nend;

        public class AnimObject
        {
            public List<CURVObject> curvs;
        }

        public class EFCTObject : AnimObject
        {
            public EFCT efct;
            public List<EMITObject> emits;
        }

        public class EMITObject : AnimObject
        {
            public EMIT emit;
            public List<PTCLObject> ptcls;
        }

        public class PTCLObject : AnimObject
        {
            public PTCL ptcl;
            public PTCLStrings strings;
        }

        public class CURVObject
        {
            public CURV curv;
            public List<KEYS> keys;
            public List<int> times;
        }

        //In VTBF, the pointer int16 is the number of nodes in the file. Ie, the single EFCT + the EMIT count
        public struct EFCT
        {
            public Vector3 unkVec3_0; //0x10, Type 0x4A, 0x1 //Translation?
            public int reserve0;
            public Vector3 unkVec3_1; //0x11, Type 0x4A, 0x1 //Rotation?
            public int reserve1;
            public Vector3 unkVec3_2; //Scale?
            public int reserve2;

            public float unkFloat0;
            public int curvOffset;
            public int curvCount;
            public int reserve3;

            public float startFrame; //0x1, Type 0x8 //Just an educated guess based on sister formats for now.
            public float endFrame; //0x2, Type 0x8 //This becomes a float in NIFL variations rather than an int as it is in VTBF
            public int unkInt1; //0x3, Type 0x8
            public fixed byte color[4]; //0x42, Type 0xC
            
            public int unkInt2;
            public int boolInt0;    //0x4, Type 0x1
            public int boolInt1;    //0x0, Type 0x1
            public int boolInt2;    //0x7, Type 0x1
            
            public float unkFloat1; //0x91, Type 0x8
            public float unkFloat2; //0x92, Type 0x8

            public PSO2Stringx30 soundName; //0x90, Type 0x2
            public int emmitOffset;
            public int emitCount;
        }

        //Seems fairly variable, various items missing or not seemingly randomly in VTBF.
        //In VTBF, the pointer int16 is the number of nodes at and below this. Ie, the single EMIT + the PTCL count
        //In NIFL, these are laid out sequentially based on the count within the EFCT structure
        public struct EMIT
        {
            public Vector3 unkVec3_0; //0x10, Type 0x4A, 0x1 //Translation?
            public int reserve0;
            public Vector3 unkVec3_1; //0x11, Type 0x4A, 0x1 //Rotation?
            public int reserve1;
            public Vector3 unkVec3_2; //0x13, Type 0x4A, Count 0x1 //Scale?
            public int reserve2;

            public float unkFloat0;
            public int curvOffset;
            public int curvCount;
            public int reserve3;

            public Vector3 unkVec3_3;  //0x12, Type 0x4A, Count 0x1
            public int reserve4;
            public Vector3 unkVec3_4;  //0x14, Type 0x4A, Count 0x1
            public int reserve5;
            public Vector3 unkVec3_5;  //0x15, Type 0x4A, Count 0x1
            public int reserve6;

            public float startFrame; //0x1, Type 0x8 //Just an educated guess based on sister formats for now.
            public float endFrame; //0x2, Type 0x8 //This becomes a float in NIFL variations rather than an int as it is in VTBF
            public int unkInt1;     //0x20, Type 0x8
            public float unkFloat1;  //0x21, Type 0x8

            public int unkInt2; //0x43, Type 0x4 //This became a full int in NIFL
            public int unkInt3; //0x5, Type 0x9
            public int unkInt4; 
            public float unkFloat2;

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

            public Vector3 unkVec3_6;  
            public int reserve7;
            public Vector3 unkVec3_7;           //This is maybe:  //0x19, Type 0x4A, Count 0x1 
            public int reserve8;

            public int int_E0;
            public int field_E4;
            public int field_E8;
            public int field_EC;

            public int ptclOffset;
            public int ptclCount;
            public int field_F8;
            public int field_FC;

            //****The below exist in VTBF, but seemingly aren't in NIFL...

            //0x86, Type 0x8
            //0x32, Type 0x4
            //0x31, Type 0xA
            //0x3B, Type 0xA
            //0x3C, Type 0xA
            //0x36, Type 0x9
            //0x6, Type 0x9
        }

        public struct PTCL
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

            public float field_120; //0x54, Type 0xA //Maybe??
            public float field_124; //0x48, Type 0xA
            public float field_128; //0x49, Type 0xA
            public float float_12C; //0x4A, Type 0xA

            public float float_130; //0x88, Type 0xA
            public float float_134;
            public float float_138;
            public float float_13C;

            public int ptclStringsOffset;
            public int curveOffset;
            public int curveCount;
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
            PSO2Stringx30 assetName;
            PSO2Stringx30 subDirectory;
            PSO2String diffuseTex;
            PSO2String opacityTex;
        }

        //Seems to have varying pointer data, but no actual data
        public struct ANIM
        {

        }

        public struct CURV
        {
            public int type; //0x70, Type 0x4 //0x71, Type 0x4 //71 possibly combined with this? Never observed filled so unsure.
            public float startFrame; //0x74, Type 0xA //Probably start frame. It's this or float_10;
            public int int_0C; //0x73, Type 0x6
            public float float_10; //0x77, Type 0xA
            
            public int int_14; //0x76, Type 0x9 
            public float endFrame; //0x75, Type 0xA
            public int keysOffset;
            public int keysCount;

            public int timeOffset;
            public int timeCount;
        }

        //Array, but no control types
        public struct KEYS
        {
            public int type; //0x72, Type 0x4
            public float time; //0x78, Type 0xA
            public float value; //0x79, Type 0xA
            public float inParam; //0x7A, Type 0xA

            public float outParam; //0x7B, Type 0xA
            public int field_0x18;
            public int field_0x1C;
        }

    }
}
