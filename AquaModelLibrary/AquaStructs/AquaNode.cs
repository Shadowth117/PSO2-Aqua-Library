using System;
using System.Collections.Generic;
using System.Linq;
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
            public int const0;
            public int const0_2;
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
            public int animatedFlag; //Should generally be 1
            public int parentId;
            public int unkNode; //Always observed -1
            public int nextSibling;
            public int firstChild;
            public int const0_2;
            public int const0_3;
            public float pos_x;
            public float pos_y;
            public float pos_z;
            public int const0_4;
            public float eulRot_x;
            public float eulRot_y;
            public float eulRot_z;
            public int const0_5;
            public float scale_x;
            public float scale_y;
            public float scale_z;
            public int const0_6;
            // 4x4 Matrix
            public float m11; public float m12; public float m13; public float m14;
            public float m21; public float m22; public float m23; public float m24;
            public float m31; public float m32; public float m33; public float m34;
            public float m41; public float m42; public float m43; public float m44;
            public fixed char boneName[0x20];
        }

        //A stripped down variant of NODE used for effect nodes. 
        public struct NOD0
        {
            public ushort boneShort1;
            public ushort boneShort2;
            public int animatedFlag; //Should generally be 0
            public int parentId;
            public int const_0_2;
            public float pos_x;
            public float pos_y;
            public float pos_z;
            public int const0_4;
            public float eulRot_x;
            public float eulRot_y;
            public float eulRot_z;
            public int const0_5;
            public fixed char boneName[0x20];
        }
    }
}
