using AquaModelLibrary.Helpers.Readers;

namespace AquaModelLibrary.Data.BluePoint.CMDL
{
    public class CMDL_CMSHReference
    {
        public ushort usht0;
        public int int0;
        public BPString cmshPath = null;
        public int int1;
        public CVariableTrail trail0 = null;
        public byte bt0;
        public ushort usht1;

        public CMDL_CMSHReference() { }

        public CMDL_CMSHReference(BufferedStreamReaderBE<MemoryStream> sr)
        {
            usht0 = sr.Read<ushort>();
            int0 = sr.Read<int>();
            cmshPath = new BPString(sr);
            int1 = sr.Read<int>();
            trail0 = new CVariableTrail(sr);
            bt0 = sr.Read<byte>();
            usht1 = sr.Read<ushort>();
        }
    }
}
