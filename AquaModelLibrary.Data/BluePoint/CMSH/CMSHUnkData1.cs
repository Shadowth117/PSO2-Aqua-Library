using AquaModelLibrary.Helpers.Extensions;
using AquaModelLibrary.Helpers.Readers;

namespace AquaModelLibrary.Data.BluePoint.CMSH
{
    public class CMSHUnkData1
    {
        public long position;

        public int size;
        public List<byte> buffer = new List<byte>();

        public CMSHUnkData1()
        {

        }
        public CMSHUnkData1(BufferedStreamReaderBE<MemoryStream> sr)
        {
            position = sr.Position;

            size = sr.Read<int>();
            buffer.AddRange(sr.ReadBytes(sr.Position, size));
            sr.Seek(size, System.IO.SeekOrigin.Current);
        }

        public byte[] GetBytes()
        {
            List<byte> outBytes = new List<byte>();

            outBytes.AddValue(buffer.Count);
            outBytes.AddRange(buffer);

            return outBytes.ToArray();
        }
    }
}
