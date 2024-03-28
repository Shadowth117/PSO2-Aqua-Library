using AquaModelLibrary.Helpers.Readers;
using System.Text;

namespace AquaModelLibrary.Data.BluePoint
{
    public class BPString
    {
        public int lengthLength;
        public int length;

        public byte unkByte0_0;
        public byte unkByte1_0;

        public string str;

        public BPString() { }
        public BPString(BufferedStreamReaderBE<MemoryStream> sr)
        {
            lengthLength = sr.Read<byte>();
            var lengthPeek = sr.Peek<byte>();
            if (lengthPeek == 0 || !((lengthPeek == lengthLength - 1)  || (lengthPeek == lengthLength - 2)) || lengthLength >= 0x80)
            {
                unkByte0_0 = sr.Read<byte>();
            }
            length = sr.Read<byte>();

            if (length >= 0x80)
            {
                unkByte1_0 = sr.Read<byte>();
            }
            str = Encoding.UTF8.GetString(sr.ReadBytes(sr.Position, length));
            sr.Seek(length, SeekOrigin.Current);
        }
    }
}
