using AquaModelLibrary.Helpers.Readers;
using System.Text;

namespace  AquaModelLibrary.Data.NNStructs.Structures
{
    public class NNTextureList
    {
        public struct NNTextureListEntry
        {
            public int int_00;
            public int namePtr;
            public short sht_08;
            public short sht_0A;
            public int int_0C;

            public int int_10;
        }

        public static List<string> ReadNNT(string filePath)
        {
            return ReadFile(File.ReadAllBytes(filePath));
        }
        public static List<string> ReadNNT(byte[] file)
        {
            return ReadFile(file);
        }

        private static List<string> ReadFile(byte[] file)
        {
            List<NNTextureListEntry> entries = new List<NNTextureListEntry>();
            List<int> nameOffsetList = new List<int>();
            var nameList = new List<string>();
            BufferedStreamReaderBE<MemoryStream> sr = new BufferedStreamReaderBE<MemoryStream>(new MemoryStream(file));
            int offset = 0;
            offset = ReadBytes(entries, nameOffsetList, nameList, sr, offset);

            return nameList;
        }

        private static int ReadBytes(List<NNTextureListEntry> entries, List<int> nameOffsetList, List<string> nameList, BufferedStreamReaderBE<MemoryStream> sr, int offset)
        {
            var magic = Encoding.ASCII.GetString(sr.ReadBytes(0, 4));
            sr.Seek(4, SeekOrigin.Begin);
            var len = sr.Read<int>();
            if(magic != "NXTL")
            {
                offset = len;
                sr.Seek(len, SeekOrigin.Begin);
                sr.Seek(8, SeekOrigin.Current);
            }

            //Find out if we're reading a big endian file. Before this is consistently little endian
            bool isBE = sr.Peek<int>() > 0xFFFF;

            int headerOffset = sr.ReadBE<int>(isBE);
            sr.Seek(offset + headerOffset, SeekOrigin.Begin);
            int nameCount = sr.ReadBE<int>(isBE);
            int offsetTableOffset = sr.ReadBE<int>(isBE);

            sr.Seek(offset + offsetTableOffset, SeekOrigin.Begin);
            for (int i = 0; i < nameCount; i++)
            {
                NNTextureListEntry entry;
                if (isBE)
                {
                    entry = new NNTextureListEntry();
                    entry.int_00 = sr.ReadBE<int>(isBE);
                    entry.namePtr = sr.ReadBE<int>(isBE);
                    entry.sht_08 = sr.ReadBE<short>(isBE);
                    entry.sht_0A = sr.ReadBE<short>(isBE);
                    entry.int_0C = sr.ReadBE<int>(isBE);
                    entry.int_10 = sr.ReadBE<int>(isBE);
                }
                else
                {
                    entry = sr.Read<NNTextureListEntry>();
                }
                nameOffsetList.Add(entry.namePtr);
                entries.Add(entry);
            }
            foreach (var nameOffset in nameOffsetList)
            {
                sr.Seek(offset + nameOffset, SeekOrigin.Begin);
                nameList.Add(sr.ReadCString());
            }

            return offset;
        }
    }
}
