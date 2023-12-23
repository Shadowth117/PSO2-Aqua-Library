namespace AquaModelLibrary.Data.PSO2.Aqua.CharacterMakingIndexData
{
    public class FaceTextureObject : BaseCMXObject
    {
        public FaceTextures ngsFace;
        public string texString1 = null;
        public string texString2 = null;
        public string texString3 = null;

        public string texString4 = null;
    }

    public struct FaceTextures
    {
        public int id;
        public int texString1Ptr;
        public int texString2Ptr;
        public int texString3Ptr;

        public int texString4Ptr;
    }
}
