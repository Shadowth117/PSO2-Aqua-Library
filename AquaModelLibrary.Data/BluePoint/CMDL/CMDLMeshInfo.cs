using AquaModelLibrary.Helpers.Readers;

namespace AquaModelLibrary.Data.BluePoint.CMDL
{
    public class CMDLMeshInfo
    {
        public uint magic = uint.MaxValue;
        public byte bt00;
        public float flt00;
        public short sht00;

        public CMDLMeshInfo() { }
        public CMDLMeshInfo(BufferedStreamReaderBE<MemoryStream> sr, uint check)
        {
            magic = check;
            bt00 = sr.ReadBE<byte>();
            flt00 = sr.ReadBE<float>();
            sht00 = sr.ReadBE<short>();
        }
    }
}
