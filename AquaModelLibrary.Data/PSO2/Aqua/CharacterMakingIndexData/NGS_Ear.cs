namespace AquaModelLibrary.Data.PSO2.Aqua.CharacterMakingIndexData
{
    public class NGS_EarObject : BaseCMXObject
    {
        public NGS_Ear ngsEar;

        public string dataString = null;
        public string texString1 = null;
        public string texString2 = null;
        public string texString3 = null;

        public string texString4 = null;
        public string texString5 = null;
    }

    public struct NGS_Ear
    {
        public int id;
        public int dataStringPtr;
        public int texString1Ptr;
        public int texString2Ptr;

        public int texString3Ptr;
        public int texString4Ptr;
        public int texString5Ptr;
        public int unkInt0;

        public int unkInt1;
        public int unkInt2;
        public int unkInt3;
        public int unkInt4;
    }
}
