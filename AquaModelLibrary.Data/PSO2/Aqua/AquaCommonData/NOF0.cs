namespace AquaModelLibrary.Data.PSO2.Aqua.AquaCommonData
{
    public struct NOF0
    {
        public int magic;
        public int NOF0Size; //Size of NOF0 data
        public int NOF0EntryCount; //Number of entries in NOF0 data
        public int NOF0DataSizeStart;
        public List<int> relAddresses;
        public List<int> paddingToAlign;
    }
}
