using AquaModelLibrary.Helpers.Readers;

namespace AquaModelLibrary.Data.BluePoint.CMDL
{
    public class CMDL_CMATMaterialMap
    {
        public ushort usht0;
        public int int0;
        public BPString cmshMaterialName = null;

        public int int1;
        public BPString cmatPath = null;
        public CMDL_CMATMaterialMap() { }

        public CMDL_CMATMaterialMap(BufferedStreamReaderBE<MemoryStream> sr)
        {
            usht0 = sr.Read<ushort>();
            int0 = sr.Read<int>();
            cmshMaterialName = new BPString(sr);

            int1 = sr.Read<int>();
            cmatPath = new BPString(sr);
        }
    }
}
