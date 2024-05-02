using AquaModelLibrary.Helpers.Readers;

namespace AquaModelLibrary.Data.PSO2.MiscPSO2Structs.CRBPData
{
    public class StringBundle
    {
        public List<int> strIdList = new List<int>();
        public List<string> strList = new List<string>();

        public StringBundle() { }

        public StringBundle(byte[] file)
        {
            Read(file);
        }

        public StringBundle(BufferedStreamReaderBE<MemoryStream> sr)
        {
            Read(sr);
        }

        public void Read(byte[] file)
        {
            using (MemoryStream ms = new MemoryStream(file))
            using (BufferedStreamReaderBE<MemoryStream> sr = new BufferedStreamReaderBE<MemoryStream>(ms))
            {
                Read(sr);
            }
        }

        public void Read(BufferedStreamReaderBE<MemoryStream> sr)
        {
            var startPos = sr.Position;
            var stringBundleSize = sr.Read<int>();
            var stringCount = sr.Read<int>();

            List<int> relativeAddresses = new List<int>();
            for (int i = 0; i < stringCount; i++)
            {
                relativeAddresses.Add(sr.Read<int>());
            }

            for (int i = 0; i < stringCount; i++)
            {
                strIdList.Add(sr.Read<int>());
            }

            foreach (var address in relativeAddresses)
            {
                sr.Seek(startPos + address, SeekOrigin.Begin);
                strList.Add(sr.ReadCString());
            }
        }

    }
}
