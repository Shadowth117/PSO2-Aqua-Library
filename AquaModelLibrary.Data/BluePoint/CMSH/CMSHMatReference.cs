using System.Numerics;

namespace AquaModelLibrary.Data.BluePoint.CMSH
{
    public class CMSHMatReference
    {
        public Vector3 minBounding;
        public Vector3 maxBounding;
        public byte matNameLength;
        public string matName = null;
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
