using AquaModelLibrary.Data.DataTypes.SetLengthStrings;
using AquaModelLibrary.Helpers.PSO2;

namespace AquaModelLibrary.Data.PSO2.Aqua.CharacterMakingIndexData
{
    public class ACCEObject : BaseCMXObject
    {
        public ACCE acce;
        public ACCE_B acceB;
        public ACCE_Feb8_22 acceFeb8_22;
        public ACCE2A acce2a;
        public float flt_54;
        public ACCE2B acce2b;
        public List<ACCE_12Object> acce12List = new List<ACCE_12Object>();
        public ACCEV2 accev2;
        public int effectNamePtr;
        public float flt_90;

        public string dataString = null;
        public string nodeAttach1 = null;
        public string nodeAttach2 = null;

        public string nodeAttach3 = null;
        public string nodeAttach4 = null;
        public string nodeAttach5 = null;
        public string nodeAttach6 = null;

        public string nodeAttach7 = null;
        public string nodeAttach8 = null;

        //Feb 8 2022 strings
        public string nodeAttach9 = null;
        public string nodeAttach10 = null;

        public string nodeAttach11 = null;
        public string nodeAttach12 = null;
        public string nodeAttach13 = null;
        public string nodeAttach14 = null;

        public string nodeAttach15 = null;

        //Ver2 strings
        public string nodeAttach16 = null;
        public string nodeAttach17 = null;
        public string nodeAttach18 = null;

        public string effectName = null;

        public ACCEObject() { }

        public ACCEObject(List<Dictionary<int, object>> acceRaw)
        {
            acce.id = (int)acceRaw[0][0xFF];

            ACCE_12Object acc12A = new ACCE_12Object();
            ACCE_12Object acc12B = new ACCE_12Object();
            ACCE_12Object acc12C = new ACCE_12Object();
            acc12A.unkShort1 = VTBFMethods.GetObject<short>(acceRaw[0], 0xC7);
            acc12B.unkShort1 = VTBFMethods.GetObject<short>(acceRaw[0], 0xD7);
            acce12List.Add(acc12A);
            acce12List.Add(acc12B);
            acce12List.Add(acc12C);

            dataString = new PSO2String(VTBFMethods.GetObject<byte[]>(acceRaw[0], 0xF0)).GetString();
            nodeAttach1 = new PSO2String(VTBFMethods.GetObject<byte[]>(acceRaw[0], 0xF1)).GetString();
            nodeAttach2 = new PSO2String(VTBFMethods.GetObject<byte[]>(acceRaw[0], 0xF2)).GetString();
            nodeAttach3 = new PSO2String(VTBFMethods.GetObject<byte[]>(acceRaw[0], 0xF3)).GetString();
            nodeAttach4 = new PSO2String(VTBFMethods.GetObject<byte[]>(acceRaw[0], 0xF4)).GetString();
            nodeAttach5 = new PSO2String(VTBFMethods.GetObject<byte[]>(acceRaw[0], 0xF5)).GetString();
            nodeAttach6 = new PSO2String(VTBFMethods.GetObject<byte[]>(acceRaw[0], 0xF6)).GetString();
            nodeAttach7 = new PSO2String(VTBFMethods.GetObject<byte[]>(acceRaw[0], 0xF7)).GetString();
        }
    }

    public struct ACCE
    {
        public int id;
        public int dataStringPtr; //Name of the aqp, aqn, fltd, etc.
        public int nodeAttach1Ptr; //These pointers are to bone names the accessory uses when attaching by default in PSO2 Classic seemingly
        public int nodeAttach2Ptr;

        public int nodeAttach3Ptr;
        public int nodeAttach4Ptr;
        public int nodeAttach5Ptr;
        public int nodeAttach6Ptr;

        public int nodeAttach7Ptr;
        public int nodeAttach8Ptr;
    }

    public struct ACCE_Feb8_22
    {
        public int acceString9Ptr;
        public int acceString10Ptr;

        public int acceString11Ptr;
        public int acceString12Ptr;
        public int acceString13Ptr;
        public int acceString14Ptr;

        public int acceString15Ptr;
    }

    public struct ACCE_B
    {
        public int unkInt0;           //Often 0x1
        public int unkInt1;           //Often 0x64

        public int unkInt2;
        public int unkInt3;
        public int unkInt4;
        public int unkInt5;

        public int unkInt6;
    }

    public struct ACCE2A
    {
        public float unkFloatNeg1;
        public float unkFloat0;
        public float unkFloat1;

        public float unkFloat2;
        public float unkFloat3;
        public float unkFloat4;
        public float unkFloat5;
    }
    public struct ACCE2B
    {
        public int unkInt8;
        public int unkInt9;
        public int unkInt10;
    }

    public struct ACCEV2
    {
        public int acceString16Ptr;
        public int acceString17Ptr;
        public int acceString18Ptr;
    }

    public class ACCE_12Object
    {
        public float unkFloat0;
        public float unkFloat1;
        public int unkInt0;
        public int unkInt1;

        public int unkIntFeb822_0;

        public short unkShort0;
        public short unkShort1; //0xD7, 0x6 on 1st, 0xC7, 0x6
        public short unkShort2;
        public short unkShort3;

        public int unkIntFeb822_1;
    }
}
