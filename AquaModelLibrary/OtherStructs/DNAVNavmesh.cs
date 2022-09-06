using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace AquaModelLibrary.OtherStructs
{
    public class DNAVNavmesh
    {
        public struct navHeader
        {
            public int magic;
            public int int_04;
            public int int_08;
            public int int_0C;

            public int int_10;
            public int padding_14;
            public int padding_18;
            public int padding_1C;

            //Bounds? Looks identical among sn_5000 dnavs
            public Vector3 minBounds;
            public Vector3 maxBounds;
            public Vector3 minBounds2;
            public float flt_44;
            public float flt_48;
            public int int_4C;

            public int int_50;
            public Vector3 vec3_54;

            public Vector3 vec3_60;
            public ushort usht_6C;
            public ushort count_6E;

            public ushort usht_70;
            public ushort usht_72;
            public int count_74;
            public int int_78;
            public int int_7C;

            public int int_80;
        }

        public struct VANDHeader
        {
            public int magic;
            public int int_04;
            public int int_08;
            public int int_0C;

            public int int_10;
            public int int_14;
            public int int_18;
            public int vertPosCount;
        }
    }
}
