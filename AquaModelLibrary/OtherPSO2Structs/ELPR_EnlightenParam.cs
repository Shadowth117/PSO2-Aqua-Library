using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace AquaModelLibrary.OtherStructs
{
    public class ELPR_EnlightenParam
    {
        public List<ElprPiece> elprList = new List<ElprPiece>();

        public class ElprPiece
        {
            public string name0x18; //Read as C string from 0x18 byte array, stripping from the null character at the end
            public ushort usht_18;
            public ushort usht_1A;
            public int int_1C;

            public Vector3 vec3_20;
            public int int_2C;
            public Vector3 vec3_30;
            public int int_3C;
        }

    }
}
