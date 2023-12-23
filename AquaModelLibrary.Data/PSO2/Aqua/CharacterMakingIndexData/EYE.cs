namespace AquaModelLibrary.Data.PSO2.Aqua.CharacterMakingIndexData
{
    public class EYEObject : BaseCMXObject
    {
        public EYE eye;
        public string texString1 = null;
        public string texString2 = null;
        public string texString3 = null;

        public string texString4 = null;
        public string texString5 = null;
    }

    public struct EYE
    {
        public int id;
        public int texString1Ptr;
        public int texString2Ptr;
        public int texString3Ptr;

        public int texString4Ptr;
        public int texString5Ptr;
        public float unkFloat0;
        public float unkFloat1;
    }
}
