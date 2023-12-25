using AquaModelLibrary.Helpers.PSO2;

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

        public NDTR(List<Dictionary<int, object>> ndtrRaw)
        {
            boneCount = (int)ndtrRaw[0][0x01];
            unknownCount = (int)ndtrRaw[0][0x02];
            effCount = (int)ndtrRaw[0][0xFA];

            //Unused in VTBF
            boneAddress = 0;
            unknownAddress = 0;
            effAddress = 0;
            const0_3 = 0;
            const0_4 = 0;
        }

        public byte[] GetBytesVTBF()
        {
            List<byte> outBytes = new List<byte>();

            VTBFMethods.AddBytes(outBytes, 0x1, 0x8, BitConverter.GetBytes(boneCount));
            VTBFMethods.AddBytes(outBytes, 0x2, 0x8, BitConverter.GetBytes(unknownCount));
            VTBFMethods.AddBytes(outBytes, 0xFA, 0x8, BitConverter.GetBytes(effCount));

            VTBFMethods.WriteTagHeader(outBytes, "NDTR", 0x2, 0x3);

            return outBytes.ToArray();
        }
    }
}
