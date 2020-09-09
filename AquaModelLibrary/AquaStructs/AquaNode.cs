using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace AquaModelLibrary.AquaStructs
{
    public unsafe class AquaNode
    {
        public AquaCommon.NIFL nifl;
        public AquaCommon.REL0 rel0;
        public NDTR ndtr;
        public List<NODE> nodeList= new List<NODE>();
        public List<NOD0> nod0List = new List<NOD0>();
        public AquaCommon.NOF0 nof0;
        public AquaCommon.NEND nend;

        public struct NDTR
        {
            public int boneCount;
            public int boneAddress;
            public int unknownCount;
            public int unknownAddress;
            public int effCount;
            public int effAddress;
            public int const0_3;
            public int const0_4;
        }

        //Technically, the structs in this are all in the one big NODE struct, but it seemed more logical to separate it out a bit more.
        public struct NODE
        {
            public ushort boneShort1;
            public ushort boneShort2;
            public int animatedFlag; //Should generally be 1. I assume this is what it is based on PSU's bone format
            public int parentId;
            public int unkNode; //Always observed -1
            public int firstChild;
            public int nextSibling;
            public int const0_2;
            public int const0_3;
            public Vector3 pos;
            public int const0_4;
            public Vector3 eulRot;
            public int const0_5;
            public Vector3 scale;
            public int const0_6;
            // 4x4 Matrix
            public Vector4 m1;
            public Vector4 m2;
            public Vector4 m3;
            public Vector4 m4;
            public fixed char boneName[0x20];
        }

        //A stripped down variant of NODE used for effect nodes. 
        public struct NOD0
        {
            public ushort boneShort1;
            public ushort boneShort2;
            public int animatedFlag; //Should generally be 1. I assume this is what it is based on PSU's bone format
            public int parentId;
            public int const_0_2;
            public Vector3 pos;
            public int const0_4;
            public Vector3 eulRot;
            public int const0_5;
            public fixed char boneName[0x20];
        }
    }
}
