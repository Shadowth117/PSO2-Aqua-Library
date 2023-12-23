namespace AquaModelLibrary.Data.PSO2.Aqua.AquaNodeData
{
    /// <summary>
    /// Nodetree
    /// </summary>
    public struct NDTR
    {
        public int boneCount;    //0x1, type 0x8
        public int boneAddress;
        public int unknownCount; //0x2, type 0x8
        public int unknownAddress;

        public int effCount;     //0xFA, type 0x8
        public int effAddress;
        public int const0_3;
        public int const0_4;
    }
}
