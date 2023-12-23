namespace AquaModelLibrary.Data.PSO2.Aqua.CharacterMakingIndexData
{
    public class VTBF_COLObject : BaseCMXObject
    {
        public VTBF_COL vtbfCol = null;
        public string utf8Name = null;
        public string utf16Name = null;
    }

    //Character color data ranges as PixelFormat.Format32bppRgb. 21 x 6 pixels. Every 3 columns is one area, but only the middle column seems used ingame.
    //Left and right columns seem like they may have been related to either ingame shading, as the deuman one does not go nearly as dark as the others, but it's hard to say.
    public class VTBF_COL
    {
        public int id;
        public byte[] utf8String = null;
        public byte[] utf16String = null;
        public byte[] colorData = null;   //Should be 0x1F8 bytes. 
    }
}
