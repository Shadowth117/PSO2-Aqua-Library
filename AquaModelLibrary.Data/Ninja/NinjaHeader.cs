namespace AquaModelLibrary.Data.Ninja
{

    //Always little endian
    public struct NinjaHeader
    {
        public int magic;
        public int fileSize;
    }
}
