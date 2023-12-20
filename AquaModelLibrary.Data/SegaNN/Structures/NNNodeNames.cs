using AquaModelLibrary.AquaMethods;
using Reloaded.Memory.Streams;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AquaModelLibrary.NNStructs.Structures
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
            for(int i = 0; i < idList.Count; i++)
            {
                dict.Add(idList[i], nameList[i]);
            }

            return dict;
        }

        private static void ReadFile(string filePath, out List<int> idList, out List<string> nameList)
        {
            idList = new List<int>();
            List<int> nameOffsetList = new List<int>();
            nameList = new List<string>();
            BufferedStreamReader sr = new BufferedStreamReader(new MemoryStream(File.ReadAllBytes(filePath)), 8192);
            int offset = 0;
            offset = ReadBytes(idList, nameList, nameOffsetList, sr, offset);
        }

        private static int ReadBytes(List<int> idList, List<string> nameList, List<int> nameOffsetList, BufferedStreamReader sr, int offset)
        {
            var magic = Encoding.ASCII.GetString(sr.ReadBytes(0, 4));
            sr.Seek(4, SeekOrigin.Begin);
            var len = sr.Read<int>();
            offset = len;
            sr.Seek(len, SeekOrigin.Begin);
            
            sr.Seek(8, SeekOrigin.Current);
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
                nameList.Add(AquaGeneralMethods.ReadCString(sr));
            }

            return offset;
        }
    }
}
