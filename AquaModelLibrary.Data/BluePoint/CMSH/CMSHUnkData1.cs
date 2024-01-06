using AquaModelLibrary.Helpers.Readers;

namespace AquaModelLibrary.Data.BluePoint.CMSH
{
    public class CMSHUnkData1
    {
        public int size;
        public List<byte> buffer = new List<byte>();

        public CMSHUnkData1()
        {

        }
        public CMSHUnkData1(BufferedStreamReaderBE<MemoryStream> sr)
        {
            size = sr.Read<int>();
            buffer.AddRange(sr.ReadBytes(sr.Position, size));
            sr.Seek(size, System.IO.SeekOrigin.Current);
        }
    }
}
