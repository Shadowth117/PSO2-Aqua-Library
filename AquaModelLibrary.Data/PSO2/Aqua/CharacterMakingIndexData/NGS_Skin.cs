namespace AquaModelLibrary.Data.PSO2.Aqua.CharacterMakingIndexData
{
    public class NGS_SKINObject : BaseCMXObject
    {
        public NGS_Skin ngsSkin;
        public Skin_12_4_24 skin12_4_24;
        public string texString1 = null;
        public string texString2 = null;
        public string texString3 = null;

        public string texString4 = null;
        public string texString5 = null;
        public string texString6 = null;
        public string texString7 = null;
        public string texString8 = null;
        public string texString9 = null;
        public string texString10 = null;

        public string texString11 = null;
        public string texString12 = null;
        public string texString13 = null;
        public string texString14 = null;
    }

    public struct NGS_Skin
    {
        public int id;
        public int texString1Ptr;
        public int texString2Ptr;
        public int texString3Ptr;

        public int texString4Ptr;
        public int texString5Ptr;
        public int texString6Ptr;
        public int texString7Ptr;

        public int texString8Ptr;
        public int texString9Ptr;
        public int texString10Ptr;
        public byte unkByte0;
        public byte unkByte1;
        public byte unkByte2;
        public byte unkByte3;
    }

    public struct Skin_12_4_24
    {
        public int texString11Ptr;
        public int texString12Ptr;
        public int texString13Ptr;
        public int texString14Ptr;
    }
}
