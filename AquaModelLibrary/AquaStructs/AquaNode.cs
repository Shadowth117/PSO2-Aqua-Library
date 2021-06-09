using System.Collections.Generic;
using System.Numerics;

namespace AquaModelLibrary
{
    //Though the NIFL format is used for storage, VTBF format tag references for data will be commented where appropriate. Some offset/reserve related things are NIFL only, however.

    public unsafe class AquaNode : AquaCommon
    {
        public NDTR ndtr;
        public List<NODE> nodeList = new List<NODE>();
        public List<NODO> nodoList = new List<NODO>();
        public AquaNode()
        {
        }

        public struct NDTR
        {
            public int boneCount;    //0x1, type 0x8
            public int boneAddress; 
            public int unknownCount; //0x2, type 0x8
            public int unknownAddress;

            public int effCount;     //0xFA, type 0x8
            public int effAddress;
            public int const0_3;
            public int const0_4;
        }

        //Technically, the structs in this are all in the one big NODE struct, but it seemed more logical to separate it out a bit more.
        public struct NODE
        {
            public ushort boneShort1; //0x3, type 0x9 //First 2 bytes of this.
            public ushort boneShort2; //Latter 2 bytes of above. Bones with 0x400 in this value are not exported in animations.
            public int animatedFlag;  //0xB, type 0x9 //Should generally be 1. I assume this is what it is based on PSU's bone format Never really used normally?
            public int parentId;      //0x4, type 0x8
            public int unkNode;       //0xF, type 0x8 //Always observed -1

            public int firstChild;    //0x5, type 0x8
            public int nextSibling;   //0x6, type 0x8
            public int const0_2;      //0xC, type 0x9? Ordering and nebulous usage makes this unclear. Observed only 0.
            public int ngsSibling;    //Counts up in the case of an NGS bone sibling for player models.

            public Vector3 pos;       //0x7, type 0x4A, 0x1
            public int const0_4;
            public Vector3 eulRot;    //0x8, type 0x4A, 0x1
            public int const0_5;
            public Vector3 scale;     //0x9, type 0x4A, 0x1
            public int const0_6;
            // 4x4 Matrix             //0xA, type 0xCA, 0xA, 0x3
            public Vector4 m1;        
            public Vector4 m2;
            public Vector4 m3;
            public Vector4 m4;
            public PSO2String boneName; //0xD, type 0x2
        }

        //A stripped down variant of NODE used for effect nodes. 
        public struct NODO
        {
            public ushort boneShort1;  //0x3, type 0x9 //First 2 bytes of this.
            public ushort boneShort2;  //Latter 2 bytes of above.
            public int animatedFlag;   //0xB, type 0x9 //Should generally be 1. I assume this is what it is based on PSU's bone format
            public int parentId;       //0x4, type 0x8
            public int const_0_2;      //0xC, type 0x9?
            public Vector3 pos;        //0x7, type 0x4A, 0x1
            public int const0_4;
            public Vector3 eulRot;     //0x8, type 0x4A, 0x1
            public int const0_5;
            public PSO2String boneName; //0xD, type 0x2
        }
    }
}
