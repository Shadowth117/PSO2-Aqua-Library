namespace AquaModelLibrary.Data.PSO2.Aqua.CharacterMakingIndexData
{
    public class NIFL_COLObject : BaseCMXObject
    {
        public NIFL_COL niflCol;
        public string textString = null;
    }

    //Character color data ranges as PixelFormat.Format32bppRgb. 7 x 6 pixels. Same main data as old VTBF_COL, just without the vestigial ranges.
    public unsafe struct NIFL_COL
    {
        public int id;
        public int textStringPtr;
        public fixed byte colorData[0xA8];
    }
}
