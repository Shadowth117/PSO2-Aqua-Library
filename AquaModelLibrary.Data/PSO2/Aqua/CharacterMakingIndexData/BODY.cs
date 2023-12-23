using static AquaModelLibrary.CharacterMakingIndex;

namespace AquaModelLibrary.Data.PSO2.Aqua.CharacterMakingIndexData
{
    public class BODYObject : BaseCMXObject
    {
        public BODY body;
        public BODYMaskColorMapping bodyMaskColorMapping;
        public BODY2 body2;
        public BODY40Cap body40cap;
        public BODY2023_1 body2023_1;
        public BODYVer2 bodyVer2;
        public string dataString = null;
        public string texString1 = null;
        public string texString2 = null;
        public string texString3 = null;
        public string texString4 = null;
        public string texString5 = null;
        public string texString6 = null;
        public string nodeString0 = null;
        public string nodeString1 = null;
        public string nodeString2 = null;
    }

    public struct BODY
    {
        public int id; //0xFF, 0x8
        public int dataStringPtr; //Name of the aqp, aqn, fltd, etc.
        public int texString1Ptr;
        public int texString2Ptr;

        public int texString3Ptr;
        public int texString4Ptr;
        public int texString5Ptr;
        public int texString6Ptr;

        public int int_20;
    }

    public struct BODYMaskColorMapping //Body struct section addition added with Ritem
    {
        public CharColorMapping redIndex;
        public CharColorMapping greenIndex;
        public CharColorMapping blueIndex;
        public CharColorMapping alphaIndex;
    }

    public struct BODY2
    {
        public int int_24_0x9_0x9;   //0x9, 0x9
        public int int_28;
        public int int_2C;           //One byte of this is a set of bitflags for parts to hide if it's outer wear. They follow the order of basewear ids.

        public int costumeSoundId;   //0xA, 0x8
        public int headId;  //0xD, 0x8 //Contains the id for a linked head piece, such as madoka's hair or bask repca's helmet. If they exist, this will be both the head part, 
        public int int_38;         //Usually -1
        public int int_3C;         //Usually -1

        public int linkedInnerId;         //Usually -1
        public int int_44;         //Usually -1
        public float legLength;   //0x8, 0xA
        public float float_4C_0xB;   //0xB, 0xA

        public float float_50;
        public float float_54;
        public float float_58;
        public float float_5C;

        public float float_60;
        public int int_64;
    }

    public struct BODY40Cap //Added 2/9 update
    {
        public float float_78;
        public float float_7C;
    }

    public struct BODY2023_1
    {
        public int nodeStrPtr_0;
        public int nodeStrPtr_1;
    }

    public struct BODYVer2
    {
        public int nodeStrPtr_2;
        public float flt_8C;
        public float flt_90;
        public float flt_94;
        public float flt_98;
    }
}
