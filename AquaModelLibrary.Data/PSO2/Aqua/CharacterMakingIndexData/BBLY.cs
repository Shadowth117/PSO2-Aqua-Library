namespace AquaModelLibrary.Data.PSO2.Aqua.CharacterMakingIndexData
{
    //BBLY, BDP1
    public class BBLYObject : BaseCMXObject
    {
        public BBLY bbly;
        public string texString1 = null;
        public string texString2 = null;
        public string texString3 = null;

        public string texString4 = null;
        public string texString5 = null;
    }

    public struct BBLY
    {
        public int id; //0xFF, 0x8
        public int texString1Ptr;
        public int texString2Ptr;
        public int texString3Ptr;

        public int texString4Ptr;
        public int texString5Ptr;
        public int unkInt0;       //This value forward may just be junk or new functionality in this version. Old BBLY only had the id and texture strings. Seems to correlate to BODY stuff though.
        public int unkInt1;

        public int unkInt2;
        public int unkInt3;
        public float unkFloat0;
        public float unkFloat1;

        public int unkInt4;
        public int unkInt5;
        public float unkFloat2;
        public float unkFloat3;

        public float unkFloat4;
        public float unkFloat5;
    }
}
