namespace AquaModelLibrary.Data.PSO2.Aqua.AquaCommonData
{
    public struct REL0
    {
        public int magic;
        public int REL0Size; //REL0 is the container of general data in NIFL format
        public int REL0DataStart; //Always 0x10 for models, skeletons, and anims. Matters most for other filetypes where the REL structure is used more directly.
        public int version;
    }
}
