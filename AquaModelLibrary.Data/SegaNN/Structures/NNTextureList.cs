using AquaModelLibrary.AquaMethods;
using Reloaded.Memory.Streams;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AquaModelLibrary.NNStructs.Structures
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
            return ReadFile(filePath);
        }

        private static List<string> ReadFile(string filePath)
        {
            List<NNTextureListEntry> entries = new List<NNTextureListEntry>();
            List<int> nameOffsetList = new List<int>();
            var nameList = new List<string>();
            BufferedStreamReader sr = new BufferedStreamReader(new MemoryStream(File.ReadAllBytes(filePath)), 8192);
            int offset = 0;
            offset = ReadBytes(entries, nameOffsetList, nameList, sr, offset);

            return nameList;
        }

        private static int ReadBytes(List<NNTextureListEntry> entries, List<int> nameOffsetList, List<string> nameList, BufferedStreamReader sr, int offset)
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
                nameList.Add(AquaGeneralMethods.ReadCString(sr));
            }

            return offset;
        }
    }
}
