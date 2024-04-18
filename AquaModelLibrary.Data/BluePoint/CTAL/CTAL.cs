using AquaModelLibrary.Helpers.Readers;
using System.Text;

namespace AquaModelLibrary.Data.BluePoint.CTAL
{
    /// <summary>
    /// BluePoint Texture Atlas
    /// </summary>
    public class CTAL
    {
        public string ctxrReference;
        public List<CTALEntry> entries = new List<CTALEntry>();
        public CFooter footer;

        public CTAL() { }

        public CTAL(byte[] file)
        {
            file = CompressionHandler.CheckCompression(file);
            using (MemoryStream ms = new MemoryStream(file))
            using (BufferedStreamReaderBE<MemoryStream> sr = new BufferedStreamReaderBE<MemoryStream>(ms))
            {
                Read(sr);
            }
        }

        private void Read(BufferedStreamReaderBE<MemoryStream> sr)
        {
            byte strLength = sr.ReadBE<byte>();
            ctxrReference = Encoding.UTF8.GetString(sr.ReadBytes(sr.Position, strLength));
            sr.Seek(strLength, SeekOrigin.Current);
            var entryCount = sr.ReadBE<int>();
            
            for(int i = 0; i < entryCount; i++)
            {
                entries.Add(sr.Read<CTALEntry>());
            }
            footer = sr.Read<CFooter>();
        }
    }
}
