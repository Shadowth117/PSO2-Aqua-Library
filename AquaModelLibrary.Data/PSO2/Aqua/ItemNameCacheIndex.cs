using AquaModelLibrary.Data.PSO2.Aqua.ItemNameCacheData;
using AquaModelLibrary.Helpers.Readers;
using System.Text;

namespace AquaModelLibrary.Data.PSO2.Aqua
{
    public class ItemNameCacheIndex : AquaCommon
    {
        StringBuilder output = null;
        public override string[] GetEnvelopeTypes()
        {
            return new string[] {
            "inca"
            };
        }

        public ItemNameCacheIndex() { }

        public ItemNameCacheIndex(byte[] file) : base(file) { }

        public ItemNameCacheIndex(BufferedStreamReaderBE<MemoryStream> sr) : base(sr) { }

        public override void ReadNIFLFile(BufferedStreamReaderBE<MemoryStream> sr, int offset)
        {
            sr.Seek(sr.Read<int>() + offset, SeekOrigin.Begin);

            output = new StringBuilder();
            while (true)
            {
                int category = sr.Read<int>(); //Category?
                int id = sr.Read<int>(); //id
                if (category + id == 0)
                {
                    break;
                }
                int strPointer = sr.Read<int>();
                long bookmark = sr.Position;
                sr.Seek(strPointer + offset, SeekOrigin.Begin);
                output.AppendLine($"{category.ToString("X")} {id.ToString("X")} " + sr.ReadUTF16String(true, (int)sr.BaseStream.Length));

                sr.Seek(bookmark, SeekOrigin.Begin);
            }
        }

        public class DatItemEntry
        {
            public ushort itemCategory;
            public ushort itemSubCategory;
            public ushort itemId3;
            public ushort itemId4;
            public int reserve0;
            public int reserve1;

            public string name;
        }

        public class DatItemEntry2
        {
            public int unkInt;
        }

        public class DatItemEntry3
        {
            public int unkInt;
            public int unkInt2;
        }

        /// <summary>
        /// Should work in NGS client item name cache dats. Doesn't seem to have true item ids, just information game needs to display items in inventory. They seemed linked to true item ids elsewhere.
        /// </summary>
        public static byte[] ParseDat(byte[] datFileFull)
        {
            var cacheSize = BitConverter.ToInt32(datFileFull, 4);
            byte[] datFileMain = new byte[cacheSize];
            Array.Copy(datFileFull, 8, datFileMain, 0, cacheSize);
            DecryptItemNameCacheNaDat(datFileMain);
            
            List<DatItemEntry> entries = new List<DatItemEntry>();
            List<DatItemEntry2> entries2 = new List<DatItemEntry2>();
            List<DatItemEntry3> entries3 = new List<DatItemEntry3>();
            using (var ms = new MemoryStream(datFileMain))
            using (var sr = new BufferedStreamReaderBE<MemoryStream>(ms))
            {
                var entryCount = sr.Read<int>();
                var entry2Count = sr.Read<int>();
                var entry3Count = sr.Read<int>();
                var reserve = sr.Read<int>();
                for(int i = 0; i < entryCount; i++)
                {
                    entries.Add(new DatItemEntry() { itemCategory = sr.Read<ushort>(), itemSubCategory = sr.Read<ushort>(), itemId3 = sr.Read<ushort>(), 
                        itemId4 = sr.Read<ushort>(), reserve0 = sr.Read<int>(), reserve1 = sr.Read<int>(), });
                }
                for(int i = 0; i < entry2Count; i++)
                {
                    entries2.Add(new DatItemEntry2() { unkInt = sr.Read<int>() });
                }
                for(int i = 0; i < entry3Count; i++)
                {
                    entries3.Add(new DatItemEntry3() { unkInt = sr.Read<int>(), unkInt2 = sr.Read<int>() });
                }
                for(int i = 0; i < entryCount; i++)
                {
                    entries[i].name = sr.ReadUTF16String();
                    sr.Seek((entries[i].name.Length + 1) * 2, SeekOrigin.Current);
                }
            }

            return datFileMain;
        }

        public static void DecryptItemNameCacheNaDat(byte[] datFile)
        {
            byte[] keyTable = (byte[])INCConstants.INCKey.Clone();
            int iterator2 = 0;
            int resultantSalt = 0;

            for (int i = 0; i < datFile.Length; i++)
            {
                iterator2 = (byte)(iterator2 + 1);
                byte salt = keyTable[iterator2];
                resultantSalt = (byte)(salt + resultantSalt);
                byte resultantSalt2 = keyTable[resultantSalt];

                keyTable[iterator2] = resultantSalt2;
                keyTable[resultantSalt] = salt;

                byte resultByte = (byte)(salt + resultantSalt2);
                datFile[i] ^= keyTable[resultByte];
            }
        }
    }
}
