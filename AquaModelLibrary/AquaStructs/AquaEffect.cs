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

        public NOF0 nof0;
        public NEND nend;

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
            //0x86, Type 0x8
            //0x1, Type 0x8
            //0x2, Type 0x8
            //0x20, Type 0x8
            //0x21, Type 0x8
            //0x43, Type 0x4
            //0x10, Type 0x4A, Count 0x1
            //0x11, Type 0x4A, Count 0x1
            //0x12, Type 0x4A, Count 0x1
            //0x13, Type 0x4A, Count 0x1
            //0x14, Type 0x4A, Count 0x1
            //0x15, Type 0x4A, Count 0x1
            //0x33, Type 0x4
            //0x5, Type 0x9
            //0x32, Type 0x4
            //0x31, Type 0xA
            //0x35, Type 0xA
            //0x37, Type 0xA
            //0x3B, Type 0xA
            //0x3C, Type 0xA
            //0x36, Type 0x9
            //0x19, Type 0x4A, Count 0x1
            //0x6, Type 0x9
        }

        public struct PTCL
        {
            //0x2, Type 0xA
            //0x41, Type 0x4
            //0x98, Type 0x4
            //0x4F, Type 0x4
            //0x43, Type 0x4
            //0x5, Type 0x9
            //0x6, Type 0x9
            //0x4E, Type 0xA
            //0x40, Type 0x2
            //0x19, Type 0x4A, 0x1
            //0x1A, Type 0x4A, 0x1
            //0x11, Type 0x4A, 0x1
            //0x12, Type 0x4A, 0x1
            //0x14, Type 0x4A, 0x1
            //0x15, Type 0x4A, 0x1
            //0x44, Type 0x4A, 0x1
            //0x45, Type 0x4A, 0x1
            //0x46, Type 0xA
            //0x47, Type 0xA
            //0x50, Type 0x4A, 0x1
            //0x51, Type 0x4A, 0x1
            //0x5C, Type 0x4A, 0x1
            //0x56, Type 0xA
            //0x57, Type 0xA
            //0x55, Type 0x6
            //0x5D, Type 0x1
            //0x34, Type 0x2
            //0x42, Type 0xC
            //0x4B, Type 0x4
            //0x4C, Type 0x5
            //0x4D, Type 0x5
            //0x48, Type 0xA
            //0x49, Type 0xA
            //0x4A, Type 0xA
            //0x88, Type 0xA
            //0x63, Type 0x2 //Diffuse
            //0x65, Type 0x2 //Opacity? Same as above generally
            //0x61, Type 0x4
            //0x62, Type 0x4
            //0x5E, Type 0x4
            //0x5F, Type 0x4
            //0x64, Type 0x4
            //0x68, Type 0x4
            //0x67, Type 0x4
            //0x66, Type 0x4
        }

        //Seems to have varying pointer data, but no actual data
        public struct ANIM
        {

        }

        public struct CURV
        {
            //0x70, Type 0x4
            //0x71, Type 0x4
            //0x76, Type 0x9
            //0x77, Type 0xA
            //0x73, Type 0x6
            //0x74, Type 0xA
            //0x75, Type 0xA
        }

        //Array, but no control types
        public struct KEYS
        { 
            //0x72, Type 0x4
            //0x78, Type 0xA
            //0x7A, Type 0xA
            //0x7B, Type 0xA
            //0x79, Type 0xA
        }

    }
}
