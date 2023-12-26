using AquaModelLibrary.Data.DataTypes.SetLengthStrings;
using AquaModelLibrary.Helpers.PSO2;

namespace AquaModelLibrary.Data.PSO2.Aqua.CharacterMakingIndexData
{
    public class HAIRObject : BaseCMXObject
    {
        public HAIR hair;
        public string dataString = null;
        public string texString1 = null;
        public string texString2 = null;

        public string texString3 = null;
        public string texString4 = null;
        public string texString5 = null;
        public string texString6 = null;

        public string texString7 = null;

        public HAIRObject() { }

        public HAIRObject(List<Dictionary<int, object>> hairRaw)
        {
            hair.id = (int)hairRaw[0][0xFF];

            dataString = PSO2String.GeneratePSO2String(VTBFMethods.GetObject<byte[]>(hairRaw[0], 0xA0)).GetString();
            texString1 = PSO2String.GeneratePSO2String(VTBFMethods.GetObject<byte[]>(hairRaw[0], 0xA1)).GetString();
            texString2 = PSO2String.GeneratePSO2String(VTBFMethods.GetObject<byte[]>(hairRaw[0], 0xA2)).GetString();
            texString3 = PSO2String.GeneratePSO2String(VTBFMethods.GetObject<byte[]>(hairRaw[0], 0xA3)).GetString();
            texString4 = PSO2String.GeneratePSO2String(VTBFMethods.GetObject<byte[]>(hairRaw[0], 0xA4)).GetString();
            texString5 = PSO2String.GeneratePSO2String(VTBFMethods.GetObject<byte[]>(hairRaw[0], 0xA5)).GetString();
            //A8, A9, and AA don't seem to be stored in the NIFL 
        }
    }

    public struct HAIR
    {
        public int id;
        public int dataStringPtr; //Name of the aqp, aqn, fltd, etc.
        public int texString1Ptr;
        public int texString2Ptr;

        public int texString3Ptr;
        public int texString4Ptr;
        public int texString5Ptr;
        public int texString6Ptr;

        public int texString7Ptr;
        public int unkIntB1;      //B0, 0x9
        public int unkInt1;
        public int unkInt2;

        public float unkFloat0;
        public float unkFloat1;
        public float unkFloat2;
        public float unkFloat3;

        public int unkInt3;
        public int unkInt4;
        public int unkInt5;
        public int unkInt6;

        public int unkInt7;
        public int unkInt8;
        public float unkFloat4;
        public float unkFloat5;

        public float unkFloat6;
        public int unkInt9;
        public int unkInt10;
        public int unkInt11;

        public int unkInt12;
        public int unkInt13;
        public int unkInt14;
        public float unkFloat7;

        public float unkFloat8;
        public float unkFloat9;
        public int unkInt15;
        public int unkInt16;

        public int unkInt17;
        public int unkInt18;
        public int unkInt19;
        public int unkInt20;

        public short unkShortB1; //0xB1, 0x6
        public short unkShortB2; //0xB2, 0x6
        public short unkShortB3; //0xB3, 0x6
        public short unkShort0;
    }
}
