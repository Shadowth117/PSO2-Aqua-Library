using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace AquaModelLibrary
{
    public class LHIObjectDetailLayout : AquaCommon
    {
        public LHIHeader header;
        public IDFloats idFloats;
        public List<DetailInfo> detailInfoList = new List<DetailInfo>();
        public List<string> objNames = new List<string>();
        public List<List<Matrix4x4>> matrixListList = new List<List<Matrix4x4>>();

        public struct LHIHeader
        {
            public int idFloatPointer;
            public int int_4;
            public int int_8;
            public int int_C;

            public int int_10;
            public int int_14;
            public int int_18;
            public int int_1C;

            public int int_20;
            public int int_24;
            public int detailInfoPointer;
            public int int_2C;
        }

        public struct IDFloats
        {
            public float idFloat_0;  //Odd float, seems to reference area ids, but not the area of the file?
            public float idFloat_4;  //Same as above?
            public float unkFloat_8; 
            public float field_C;
        }

        public struct DetailInfo
        {
            public int objNamePointer;
            public int int_4;
            public int matrixArrayPointer;
            public int int_C;

            public int matrixArrayCount;
            public Vector3 unkVec3_0;
            public Vector3 unkVec3_1;
            public float float_2C;
        }
    }
}
