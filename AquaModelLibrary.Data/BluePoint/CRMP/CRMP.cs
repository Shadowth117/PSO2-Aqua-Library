using AquaModelLibrary.Data.BluePoint.CRMP;
using AquaModelLibrary.Helpers.Readers;

namespace AquaModelLibrary.Data.BluePoint.CTAL
{
    /// <summary>
    /// BluePoint Reference MaP
    /// </summary>
    public class CRMP
    {
        public List<CRMPEntry> entries = new List<CRMPEntry>();
        public CFooter footer;

        public CRMP() { }

        public CRMP(byte[] file)
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
            int entryCount = sr.ReadBE<int>();

            for (int i = 0; i < entryCount; i++)
            {
                entries.Add(new CRMPEntry(sr));
            }
            footer = sr.Read<CFooter>();
        }
    }
}
