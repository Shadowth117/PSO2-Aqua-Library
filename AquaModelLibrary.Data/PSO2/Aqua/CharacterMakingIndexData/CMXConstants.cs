namespace AquaModelLibrary.Data.PSO2.Aqua.CharacterMakingIndexData
{
    public static class CMXConstants
    {
        //REL0 REL0DataStart values. Since we can expect these shouldn't normally go down, we can compare against these and check if we're greater than or equal to these
        public static int oct_21TableAddressInt = 0x2318B4; //Used for checking the version of the cmx in order to maintain legacy support
        public static int dec14_21TableAddressInt = 0x26B66C; //Ritem Update cmx. Some structs were reordered for this update.
        public static int feb8_22TableAddressInt = 0x2DAFD0; //Lv 40 update cmx
        public static int jun7_22TableAddressInt = 0x2F6C44; //Kvaris update cmx
        public static int aug17_22TableAddressInt = 0x307D6C; //August 17th 2022 update cmx
        public static int oct4_22TableAddressInt = 0x00320b1C; //October 5th 2022 update cmx
        public static int jan25_23TableAddressInt = 0x0034689C; //January 25th 2023 update cmx
        public static int ver2TableAddressInt = 0x0039B5EC; //Version 2, June 6th 2023 update cmx
        public static int oct2_24TableAddressInt = 0x0040fb90; //October 2nd, 2024 update cmx
        public static int dec4_24TableAddressInt = 0x00457354; //December 12th, 2024 update cmx
        public static int feb5_25TableAddressInt = 0x0046f1d0; //February 5th, 2025 update cmx
        public static int dec2_25TableAddressInt = 0x004b6370; //December 2nd, 2025 update dmx
    }
}
