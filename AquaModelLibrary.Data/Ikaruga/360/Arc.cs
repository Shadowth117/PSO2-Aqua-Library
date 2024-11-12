using AquaModelLibrary.Helpers.Readers;

namespace AquaModelLibrary.Data.Ikaruga._360
{
    public class Arc
    {
        public List<FileEntry> files = new List<FileEntry>();
        public Arc() { }
        public Arc(byte[] data)
        {
            Read(new BufferedStreamReaderBE<MemoryStream>(new MemoryStream(data)));
        }

        public Arc(BufferedStreamReaderBE<MemoryStream> sr)
        {
            Read(sr);
        }

        public void Read(BufferedStreamReaderBE<MemoryStream> sr)
        {
            var magic = sr.ReadBE<int>();
            sr._BEReadActive = sr.Peek<uint>() > sr.PeekBigEndianUInt32();
            var count = sr.ReadBE<int>();
            var filenameBufferSize = sr.ReadBE<int>();
            var dataOffset = sr.ReadBE<int>();

            for(int i = 0; i < count; i++)
            {
                var entry = new FileEntry();
                entry.info0 = sr.ReadBE<ushort>();
                entry.info1 = sr.ReadBE<ushort>();
                entry.filenameOffset = sr.ReadBE<int>();
                entry.dataOffset = sr.ReadBE<int>();
                entry.dataSize = sr.ReadBE<int>();

                entry.filename = sr.ReadCStringValidOffset(entry.filenameOffset + count * 0x10 + 0x10, 0);
                entry.data = sr.ReadBytes(entry.dataOffset + dataOffset, entry.dataSize);

                files.Add(entry);
            }
        }

        public class FileEntry
        {
            public ushort info0;
            public ushort info1;
            public int filenameOffset;
            public int dataOffset;
            public int dataSize;

            public string filename = "";
            public byte[] data = null;

            public string GetExtension()
            {
                if(data.Length > 4)
                {
                    if (data[0] == 0x89 && data[1] == 0x50 && data[2] == 0x4E && data[3] == 0x47)
                    {
                        return ".png";
                    }
                    else if (data[0] == 0x44 && data[1] == 0x44 && data[2] == 0x53 && data[3] == 0x20)
                    {
                        return ".dds";
                    } else if (data[0] == 0x44 && data[1] == 0x4E && data[2] == 0x42 && data[3] == 0x57)
                    {
                        return ".xwb";
                    }    

                }

                if(info0 == 0 && info1 == 0x10)
                {
                    return ".ngx";
                }
                if(info0 == 0 && info1 == 0x20)
                {
                    return ".sprx";
                }

                return ".bin";
            }
        }
    }
}
