namespace AquaModelLibrary.BluePoint.CMSH
{
    public class CMSHMatReference
    {
        public byte[] texNameHash = new byte[0x18]; //Hash for material name? 
        public byte matNameLength;
        public string matName;
        public int startingFaceIndex = -1;
        public int endingFaceIndex = -1; //start of next mesh? Idk

        //SOTC stuff
        public byte unkByte;
        public int startingVertexIndex = -1;
        public int vertexIndicesUsed = -1;
        public int startingFaceVertIndex = -1;
        public int faceVertIndicesUsed = -1;
    }
}
