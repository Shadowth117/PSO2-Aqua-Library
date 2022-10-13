namespace AquaModelLibrary.BluePoint.CMSH
{
    public class CMSHMatReference
    {
        public byte[] texNameHash = new byte[0x18]; //Hash for material name? 
        public byte matNameLength;
        public string matName;
        public int startingFaceIndex;
        public int endingFaceIndex; //start of next mesh? Idk
    }
}
