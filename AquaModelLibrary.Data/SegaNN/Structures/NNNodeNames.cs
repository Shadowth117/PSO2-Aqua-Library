using AquaModelLibrary.Helpers.Readers;
using System.Text;

namespace  AquaModelLibrary.Data.NNStructs.Structures
{
    public static class NNNodeNames
    {
        //Technically these have ids associated with them, but I don't know that it actually makes use of those
        public static List<string> ReadNNA(string filePath)
        {
            ReadFile(filePath, out var idList, out var nameList);
            return nameList;
        }

        public static Dictionary<int, string> ReadNNADict(string filePath)
        {
            ReadFile(filePath, out var idList, out var nameList);
            Dictionary<int, string> dict = new Dictionary<int, string>();
            for (int i = 0; i < idList.Count; i++)
            {
                dict.Add(idList[i], nameList[i]);
            }

            return dict;
        }

        public static void ReadFile(string filePath, out List<int> idList, out List<string> nameList)
        {
            var rawBytes = File.ReadAllBytes(filePath);
            ReadFile(rawBytes, out idList, out nameList);
        }
        public static List<string> ReadNNA(byte[] bytes)
        {
            ReadFile(bytes, out var idList, out var nameList);
            return nameList;
        }

        public static void ReadFile(byte[] bytes, out List<int> idList, out List<string> nameList)
        {
            idList = new List<int>();
            nameList = new List<string>();

            BufferedStreamReaderBE<MemoryStream> sr = new BufferedStreamReaderBE<MemoryStream>(new MemoryStream(bytes));
            int offset = 0;
            List<int> nameOffsetList = new List<int>();
            offset = ReadBytes(idList, nameList, nameOffsetList, sr, offset);
        }

        private static int ReadBytes(List<int> idList, List<string> nameList, List<int> nameOffsetList, BufferedStreamReaderBE<MemoryStream> sr, int offset)
        {
            var magic = Encoding.ASCII.GetString(sr.ReadBytes(0, 4));
            sr.Seek(4, SeekOrigin.Begin);
            var len = sr.Read<int>();
            if (magic != "NXNN")
            {
                offset = len;
                sr.Seek(len, SeekOrigin.Begin);
                sr.Seek(8, SeekOrigin.Current);
            }

            //Find out if we're reading a big endian file. Before this is consistently little endian
            bool isBE = sr.Peek<int>() > 0xFFFF;

            int headerOffset = sr.ReadBE<int>(isBE);
            sr.Seek(offset + headerOffset, SeekOrigin.Begin);
            int unkValue = sr.ReadBE<int>(isBE);
            int nameCount = sr.ReadBE<int>(isBE);
            int offsetTableOffset = sr.ReadBE<int>(isBE);

            sr.Seek(offset + offsetTableOffset, SeekOrigin.Begin);
            for (int i = 0; i < nameCount; i++)
            {
                idList.Add(sr.ReadBE<int>(isBE));
                nameOffsetList.Add(sr.ReadBE<int>(isBE));
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
