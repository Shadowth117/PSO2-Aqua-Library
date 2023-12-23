namespace AquaModelLibrary.Data.PSO2.Aqua.AquaCommonData
{
    public struct VTBF
    {
        public int magicVTBF;
        public int size; //VTBF Size?
        public int magicAQGF; //AQGF, presumably AQua Game File
        public short unkShort0;
        public short unkShort1;
        public int magicVTC0; //special tag preceeding true tags in VTBF format. Always followed by a size int indicating the lenghh of the true tag and its data
        public int vtc0Size;
        public int magicROOT;
        public short unkTagShort;
        public short tagDataCount;
        public short tagDataSet0;
        public short tagDataSet0Length;
        public byte[] ROOTString; //String in ROOT of length tagDataSet0Length. In final, seemingly always says "hnd2aqg ver.1.61 Build: Feb 28 2012 18:46:06". Alphas note earlier dates.
    }
}
