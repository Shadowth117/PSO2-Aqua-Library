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
        public BPString(BufferedStreamReaderBE<MemoryStream> sr, BPEra newEra)
        {
            lengthLength = new CLength(sr, newEra);
            //In SOTC, this is probably either always one byte or a VLQ
            length = new CLength(sr, BPEra.DemonsSouls);
            str = Encoding.UTF8.GetString(sr.ReadBytes(sr.Position, length.GetTrueLength()));
            sr.Seek(length.GetTrueLength(), SeekOrigin.Current);
        }
    }
}
