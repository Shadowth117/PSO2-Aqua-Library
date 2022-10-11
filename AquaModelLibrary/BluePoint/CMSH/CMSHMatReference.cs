namespace AquaModelLibrary.BluePoint.CMSH
{
    public class CMSHMatReference
    {
        public byte[] texNameHash = new byte[0x18]; //Hash for material name? 
        public byte matNameLength;
        public string matName;
        public int unkInt0;
        public int unkVertexAddress; //start of next mesh? Idk
    }
}
