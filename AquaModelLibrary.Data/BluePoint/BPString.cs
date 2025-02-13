using AquaModelLibrary.Helpers.Readers;
using System.Text;

namespace AquaModelLibrary.Data.BluePoint
{
    public class BPString
    {
        public CLength lengthLength;
        public CLength length;

        public string str;

        public BPString() { }
        public BPString(BufferedStreamReaderBE<MemoryStream> sr)
        {
            lengthLength = new CLength(sr);
            length = new CLength(sr);
            str = Encoding.UTF8.GetString(sr.ReadBytes(sr.Position, length.GetTrueLength()));
            sr.Seek(length.GetTrueLength(), SeekOrigin.Current);
        }
    }
}
