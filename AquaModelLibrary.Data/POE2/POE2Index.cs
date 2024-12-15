using AquaModelLibrary.Helpers.Readers;

namespace AquaModelLibrary.Data.POE2
{
    public class POE2Index
    {
        public List<int> fileSizes = new List<int>();
        public List<string> fileNames = new List<string>();
        public List<FileHeader> fileHeaderList = new List<FileHeader>();
        public List<TextBlockInfo> textBlockInfoList = new List<TextBlockInfo>();
        public byte[] textBlob = null;
        public POE2Index() { }
        public POE2Index(byte[] file)
        {
            using (MemoryStream stream = new MemoryStream(file))
            using (var streamReader = new BufferedStreamReaderBE<MemoryStream>(stream))
            {
                Read(streamReader);
            }
        }

        public POE2Index(BufferedStreamReaderBE<MemoryStream> sr)
        {
            Read(sr);
        }

        public void Read(BufferedStreamReaderBE<MemoryStream> sr)
        {
            var fileCount = sr.ReadBE<int>();
            for(int i = 0; i < fileCount; i++)
            {
                var len = sr.ReadBE<int>();
                fileNames.Add(sr.ReadCStringSeek(len));
                fileSizes.Add(sr.ReadBE<int>());
            }
            var fileHeaderCount = sr.ReadBE<int>();
            for(int i = 0; i < fileHeaderCount; i++)
            {
                fileHeaderList.Add(new FileHeader(sr));
            }
            var textBlockCount = sr.ReadBE<int>();
            for (int i = 0; i < textBlockCount; i++)
            {
                textBlockInfoList.Add(new TextBlockInfo(sr));
            }
            var textBlobStart = sr.Position;

            //Yes, to be clear this is a compressed blob within the already compressed index. Maybe at some point the index wasn't oodled?
            textBlob = POE2ArchiveUtility.DecompressArchive(sr.ReadBytesSeek((int)(sr.BaseStream.Length - textBlobStart)))[0].file;
        }

        public class FileHeader
        {
            public ulong hash;
            public int archiveIndexId;
            public int archiveOffset;
            public int fileSize;
            public FileHeader() { }
            public FileHeader(byte[] file)
            {
                using (MemoryStream stream = new MemoryStream(file))
                using (var streamReader = new BufferedStreamReaderBE<MemoryStream>(stream))
                {
                    Read(streamReader);
                }
            }

            public FileHeader(BufferedStreamReaderBE<MemoryStream> sr)
            {
                Read(sr);
            }

            public void Read(BufferedStreamReaderBE<MemoryStream> sr)
            {
                hash = sr.ReadBE<ulong>();
                archiveIndexId = sr.ReadBE<int>();
                archiveOffset = sr.ReadBE<int>();
                fileSize = sr.ReadBE<int>();
            }

        }

        public class TextBlockInfo
        {
            public ulong hash;
            public int textBlockOffset;
            public int innerOffset;
            public int sectionSize;
            public TextBlockInfo() { }
            public TextBlockInfo(byte[] file)
            {
                using (MemoryStream stream = new MemoryStream(file))
                using (var streamReader = new BufferedStreamReaderBE<MemoryStream>(stream))
                {
                    Read(streamReader);
                }
            }

            public TextBlockInfo(BufferedStreamReaderBE<MemoryStream> sr)
            {
                Read(sr);
            }

            public void Read(BufferedStreamReaderBE<MemoryStream> sr)
            {
                hash = sr.ReadBE<ulong>();
                textBlockOffset = sr.ReadBE<int>();
                innerOffset = sr.ReadBE<int>();
                sectionSize = sr.ReadBE<int>();
            }

        }
    }
}
