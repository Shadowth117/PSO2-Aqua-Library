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
        public List<DetailInfoObject> detailInfoList = new List<DetailInfoObject>();

        public struct LHIHeader
        {
            public int idFloatPointer;
            public int int_4;
            public int int_8;
            public int int_C;

            public int objectTypeCount;
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

        public class DetailInfoObject
        {
            public DetailInfo diStruct;

            public string objName;
            public List<Matrix4x4> matrices = new List<Matrix4x4>(); //Matrices are world space
        }

        public struct DetailInfo
        {
            public int objNamePointer;
            public int int_4;
            public int matrixArrayPointer;
            public int int_C;

            public int matrixArrayCount;
            public Vector3 BoundingMin;
            public Vector3 BoundingMax;
            public float float_2C;
        }
    }
}
