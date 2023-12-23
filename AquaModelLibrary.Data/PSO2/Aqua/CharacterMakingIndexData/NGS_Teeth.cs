namespace AquaModelLibrary.Data.PSO2.Aqua.CharacterMakingIndexData
{
    public class NGS_TeethObject : BaseCMXObject
    {
        public NGS_Teeth ngsTeeth;

        public string dataString = null;
        public string texString1 = null;
        public string texString2 = null;
        public string texString3 = null;

        public string texString4 = null;
    }

    public struct NGS_Teeth
    {
        public int id;
        public int dataStringPtr;
        public int texString1Ptr;
        public int texString2Ptr;

        public int texString3Ptr;
        public int texString4Ptr;
    }
}
