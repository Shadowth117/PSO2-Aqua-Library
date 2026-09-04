using AquaModelLibrary.Helpers.Writers;
using AquaModelLibrary.Helpers.Readers;
using System.Text;

namespace AquaModelLibrary.Data.BluePoint
{
    public class BPStringMini
    {
        public CLength length;

        private string _str;
        public string str 
        { 
            get { return _str; } 
            set { 
                _str = value; 
                length = new CLength(value.Length); 
            } 
        }

        public BPStringMini() { }
        public BPStringMini(string newStr)
        {
            str = newStr;
        }
        public BPStringMini(BufferedStreamReaderBE<MemoryStream> sr, BPEra era)
        {
            //In SOTC, this is probably either always one byte or a VLQ
            length = new CLength(sr, BPEra.DemonsSouls);
            _str = Encoding.ASCII.GetString(sr.ReadBytes(sr.Position, length.GetTrueLength()));
            sr.Seek(length.GetTrueLength(), SeekOrigin.Current);
        }

        public byte[] GetBytes(BPEra era)
        {
            var outBytes = new ByteListWriter();

            var strBytes = Encoding.ASCII.GetBytes(str);
            outBytes.AddValue(length.GetBytes(era));
            outBytes.AddValue(strBytes);
            return outBytes.ToArray();
        }
    }
}
