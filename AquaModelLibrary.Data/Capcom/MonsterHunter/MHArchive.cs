using AquaModelLibrary.Helpers.Readers;

namespace AquaModelLibrary.Data.Capcom.MonsterHunter
{
    public class MHArchive
    {
        public List<byte[]> files = new List<byte[]>();

        public MHArchive() { }
        public MHArchive(byte[] archive)
        {
            Read(archive);
        }

        public MHArchive(BufferedStreamReaderBE<MemoryStream> sr)
        {
            Read(sr);
        }

        private void Read(byte[] file)
        {
            using (MemoryStream stream = new MemoryStream(file))
            using (BufferedStreamReaderBE<MemoryStream> sr = new BufferedStreamReaderBE<MemoryStream>(stream))
            {
                Read(sr);
            }
        }
        private void Read(BufferedStreamReaderBE<MemoryStream> sr)
        {
            int fileCount = sr.ReadBE<int>();
            for (int i = 0; i < fileCount; i++)
            {
                var fileOffset = sr.ReadBE<int>();
                var fileSize = sr.ReadBE<int>();
                files.Add(sr.ReadBytes(fileOffset, fileSize));
            }
        }
    }
}
