using System.Collections.Generic;
using System.Numerics;

namespace AquaModelLibrary
{
    public class FLTDPhysics : AquaCommon
    {
        public FltdHeader header = null;
        public UnkStructNGS ngsStruct = null;
        public List<MainPhysicsNode> mainNodes = new List<MainPhysicsNode>();
        public List<unkStruct1> subStructs = new List<unkStruct1>();

        public class FltdHeader
        {
            public byte version;         //Apparently there are a number of these and they work differently. This makes things painful
            public byte mainPhysicsNodeCount;
            public byte unkStruct1Count;
            public byte unkByte1;

            public int mainPhysicsNodeOffset;
            public int unkStruct1Offset;

            public int int_18;
            public int unkStructNGSOffset;
        }

        public class UnkStructNGS
        {
            public unkStructNGS ngsStruct;
            //public unkEndStruct
            //public unkEndStruct1
        }

        public struct unkStructNGS
        {
            public int int_00;
            public int unkEndStructPtr; //Struct only observed pointing to null areas at end of file
            public int int_08;
            public int unkEndStructPtr1; //Struct only observed pointing to the 0xFFFFFFFF at 0x10 of most NGS files.
        }

        //Sub of fltdHeader
        public class MainPhysicsNode
        {
            public byte bt_00; //Not the index, but often counts up like that.
            public byte unkByte1; //Usually 0x1
            public byte mainSubNodeCount;
            public byte unkByte3;
            public int namePointerPointer;
            public int mainSubNode;

            public int int_0C;
            public int int_10;

            //
            public string name;
            public List<MainSubNode> subNodes = new List<MainSubNode>();
        }

        public class MainSubNode
        {
            public byte unkByte0;
            public byte unkByte1;
            public byte unkByte2;
            public byte unkByte3;

            public float flt_04;
            public float flt_08;
            public float flt_0C;
            public float flt_10;

            public float flt_14;
            public float flt_18;
            public float flt_1C;
            public float flt_20;

            public float flt_24;
            public float flt_28;
            public float flt_2C;

            public Vector3 vec3_30;
            public float flt_3C;

            public float flt_40;
            public int nodePtr;
            public int ptr_48;
            public int ptr_4C;
            
            public int unkNodePtrPtr;
            public byte bt_54;
            public byte bt_55;
            public byte bt_56;
            public byte bt_57;
            public int ptr_58;

            //
            public string nodeName;
            public string ptr48String;
            public string ptr4CString;
            public string nodeName2;
            public string ptr58String;
        }

        //Sub of fltdHeader
        public class unkStruct1
        {
            public byte unkByte0;
            public byte unkByte1;
            public byte unkByte2;
            public byte unkByte3;

            public byte byteListCount;
            public byte unkByte5;
            public byte unkByte6;
            public byte unkByte7;

            public int byteListOffset;
            public int unkStruct2Pointer;
            public int unkStruct1_2Pointer;
            public int unkConst01;

            public int unkPointer4;

            //
            public List<byte> byteList = new List<byte>(); //Seems to be padded with 0s, but dentoed by the byteList count. byteList count includes padding.
            public unkStruct2 unkStr2 = null;
            public unkStruct1_2 unkStr1_2 = null;

        }

        public class unkStruct1_2
        {
            public int int_00;
            public int flt_04;
            public int ptr_08;
        }

        //Sub of unkStruct1
        public class unkStruct2
        {
            public float unkFloat0;
            public float unkFloat1;
            public float unkFloat2;
            public byte unkByte0;
            public byte unkByte1;
            public byte unkByte2;
            public byte unkByte3;

            public byte unkByte4;
            public byte unkStruct4Count;
            public byte unkStruct4_2Count;
            public byte unkStruct5Count;
            public int ptr_14;
            public int int_18;
            public int int_1C;

            public int unkStruct4Pointer;
            public int unkStruct4_2Pointer;
            public int unkStruct5Pointer;
            public int unkStruct7Pointer;

            public int unkStruct8Pointer;
            public int int_34;
            public int unkStruct3Pointer;

            //
            public List<unkStruct4> unkStr4List = new List<unkStruct4>();
            
            public List<unkStruct4> unkStr4_2List = new List<unkStruct4>();
            public List<float> floatList = new List<float>(); //Seems to be at least 8 and count downwards. Might be pairs of 8? 2 * unkStruct4_2 count of floats?

            public List<unkStruct5> unkStr5List = new List<unkStruct5>();
            public unkStruct3 unkStr3 = null;
        }

        //Sub of unkStruct1
        public class unkStruct3
        {
            public int ptr_00;
            public int ptr_04;
            public int int_08;
            public int ptr_0C;

            public List<unkStruct3_1> unkStr3_1List = new List<unkStruct3_1>();
            public List<unkStruct3_2> unkStr3_2List = new List<unkStruct3_2>();
        }

        public class unkStruct3_1
        {
            public float flt_00;
            public float flt_04;
            public float flt_08;
            public float flt_0C;

            public float flt_10;
            public int int_14;
            public int int_18;
            public int ptr_1C;
        }

        public class unkStruct3_2
        {
            public float flt_00;
            public float flt_04;
            public int int_08;
            public int int_0C;

            public int ptr_10;
        }

        //Sub of unkStruct2
        public class unkStruct4
        {
            public byte index;
            public byte unkByte1;
            public byte unkByte2;
            public byte unkByte3;

            public int int_04;
            public int int_08;
            public int int_0C;
            public float flt_10;
            public int int_14;
        }

        //Sub of unkStruct2
        public class unkStruct5
        {
            public byte unkByte0;
            public byte unkByte1;
            public byte index;
            public byte unkByte3;

            public int int_04;
            public int int_08;
            public int int_0C;
            public float flt_10;
        }

    }
}
