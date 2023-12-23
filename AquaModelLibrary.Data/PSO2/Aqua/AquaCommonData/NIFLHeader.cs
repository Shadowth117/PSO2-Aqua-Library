namespace AquaModelLibrary.Data.PSO2.Aqua.AquaCommonData
{
    public struct NIFL
    {
        public int magic;
        public int NIFLLength; //Length of NIFL after first 0x8
        public int unkInt0; //Always 1
        public int offsetAddition; //Full size of NIFL

        public int NOF0Offset; //Offset of NOF0 from NIFL header end
        public int NOF0OffsetFull; //Offset of NOF0 from NIFL header start
        public int NOF0BlockSize; //Size of NOF0 struct
        public int padding0;
    }
}
