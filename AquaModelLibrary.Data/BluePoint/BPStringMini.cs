using AquaModelLibrary.Helpers.Extensions;
using AquaModelLibrary.Helpers.Readers;
using System.Text;

namespace AquaModelLibrary.Data.BluePoint
{
    public class BPString
    {
        public CLength lengthLength;
        public CLength length;

        private string _str;
        public string str 
        { 
            get { return _str; } 
            set { 
                _str = value; 
                length = new CLength(value.Length); 
                lengthLength = new CLength(length.GetTrueLength() + _str.Length); 
            } 
        }

        public BPString() { }
        public BPString(string newStr)
        {
            str = newStr;
        }
        public BPString(BufferedStreamReaderBE<MemoryStream> sr, BPEra era)
        {
            lengthLength = new CLength(sr, era);
            //In SOTC, this is probably either always one byte or a VLQ
            length = new CLength(sr, BPEra.DemonsSouls);
            _str = Encoding.ASCII.GetString(sr.ReadBytes(sr.Position, length.GetTrueLength()));
            sr.Seek(length.GetTrueLength(), SeekOrigin.Current);
        }

        public byte[] GetBytes(BPEra era)
        {
            List<byte> outBytes = new List<byte>();

            var strBytes = Encoding.ASCII.GetBytes(str);
            outBytes.AddValue(lengthLength.GetBytes(era));
            outBytes.AddValue(length.GetBytes(era));
            outBytes.AddValue(strBytes);
            return outBytes.ToArray();
        }
    }
}
