using AquaModelLibrary.Helpers.Readers;
using AquaModelLibrary.Helpers.Writers;

namespace AquaModelLibrary.Data.BluePoint.CSKL
{
    //Offsets in this class are relative to the pointer's own offset
    public class CSKLNames
    {
        public class CSKLNameList
        {
            public List<int> relativeOffsets = new List<int>();
            public List<string> names = new List<string>();

            //Helper
            public List<long> absoluteOffsets = new List<long>();

            public CSKLNameList()
            {

            }
            public CSKLNameList(BufferedStreamReaderBE<MemoryStream> sr, int count = 0)
            {
                if(count == 0)
                {
                    count = sr.Read<int>();
                }
                for (int i = 0; i < count; i++)
                {
                    int relativeOffset = sr.Read<int>();
                    long absoluteOffset = relativeOffset + sr.Position - 0x4;

                    var bookmark = sr.Position;
                    sr.Seek(absoluteOffset, System.IO.SeekOrigin.Begin);
                    string name = sr.ReadCString(0x500);
                    sr.Seek(bookmark, System.IO.SeekOrigin.Begin);

                    relativeOffsets.Add(relativeOffset);
                    absoluteOffsets.Add(absoluteOffset);
                    names.Add(name);
                }
            }
        }

        public int crc; 
        public int unk0;
        public int morphTargetNameCount;
        public int skeletonNameOffsetListOffset;

        public int morphTargetNameOffsetListOffset;
        public int morphTargetFlagOffsetListOffset;
        public CSKLNameList skeletonNames = null;
        public CSKLNameList morphTargetNames = null;

        public List<byte> morphTargetFlags = new List<byte>();

        //Helper
        public long skeletonNameOffsetListAbsoluteOffset;

        public long morphTargetNameOffsetListAbsoluteOffset;
        public long morphTargetParamOffsetListAbsoluteOffset;


        public CSKLNames()
        {

        }
        public CSKLNames(BufferedStreamReaderBE<MemoryStream> sr, int csklVersion, int boneCount = 0)
        {
            crc = sr.Read<int>();
            unk0 = sr.Read<int>();
            morphTargetNameCount = sr.Read<int>(); //Not used in 0x9 version

            switch (csklVersion)
            {
                case 0x9:
                    skeletonNames = new CSKLNameList(sr, boneCount);
                    break;
                case 0x19:
                    var start = sr.Position;
                    var tempPos = sr.Position;
                    skeletonNameOffsetListOffset = sr.Read<int>();
                    skeletonNameOffsetListAbsoluteOffset = skeletonNameOffsetListOffset + tempPos;

                    tempPos = sr.Position;
                    morphTargetNameOffsetListOffset = sr.Read<int>();
                    morphTargetNameOffsetListAbsoluteOffset = morphTargetNameOffsetListOffset + tempPos;

                    tempPos = sr.Position;
                    morphTargetFlagOffsetListOffset = sr.Read<int>();
                    morphTargetParamOffsetListAbsoluteOffset = morphTargetFlagOffsetListOffset + tempPos;

                    //Primary Name List
                    skeletonNames = new CSKLNameList(sr);
                    if (morphTargetNameCount > 0 && morphTargetNameOffsetListOffset > 0)
                    {
                        sr.Seek(morphTargetNameOffsetListAbsoluteOffset, System.IO.SeekOrigin.Begin);
                        morphTargetNames = new CSKLNameList(sr);
                        sr.Seek(morphTargetParamOffsetListAbsoluteOffset, System.IO.SeekOrigin.Begin);
                        int flagCount = sr.Read<int>();
                        for (int i = 0; i < flagCount; i++)
                        {
                            morphTargetFlags.Add(sr.Read<byte>());
                        }
                    }

                    //Conditionally seek to end
                    if (morphTargetNames != null && morphTargetNames.names.Count > 0)
                    {
                        sr.Seek(morphTargetNames.absoluteOffsets[morphTargetNames.absoluteOffsets.Count - 1], System.IO.SeekOrigin.Begin);
                    }
                    else
                    {
                        sr.Seek(skeletonNames.absoluteOffsets[skeletonNames.absoluteOffsets.Count - 1], System.IO.SeekOrigin.Begin);
                    }
                    sr.ReadCStringSeek(0x500);
                    break;
            }
        }

        public byte[] GetBytes()
        {
            ByteListWriter outBytes = new();
            int skeletonNamesCount = skeletonNames.names.Count;
            int morphCount = morphTargetNames.names.Count;

            int headerSize = 0x18;
            int skeletonNamesListOffset = 0x0C; 
            int skeletonNamesListSize = 4 + skeletonNamesCount * 4;
            int pad1Size = (4 - (skeletonNamesListSize % 4)) % 4;
            int morphListOffset = headerSize + skeletonNamesListSize + pad1Size;
            int morphTargetsListSize = morphCount > 0 ? 4 + morphCount * 4 : 0;
            int pad2Size = 0;
            int flagsOffset = morphListOffset + morphTargetsListSize + pad2Size;
            int flagsSize = morphCount > 0 ? 4 + morphCount : (morphCount > 0 ? 4 : 0);
            int pad3Size = -1;

            outBytes.AddValue(crc);
            outBytes.AddValue(unk0);
            outBytes.AddValue(morphCount);
            outBytes.AddValue(skeletonNamesListOffset);
            outBytes.AddValue(morphListOffset - 0x10); 
            outBytes.AddValue(flagsOffset - 0x14);     

            long skeletonNamesListStart = outBytes.Count; 
            int stringDataStart = headerSize + skeletonNamesListSize + pad1Size + morphTargetsListSize + pad2Size + flagsSize;
            if (pad3Size >= 0)
            {
                stringDataStart = headerSize + skeletonNamesListSize + pad1Size + morphTargetsListSize + pad2Size + flagsSize + pad3Size;
            }
            else
            {
                int prePad3 = headerSize + skeletonNamesListSize + pad1Size + morphTargetsListSize + pad2Size + flagsSize;
                stringDataStart = (prePad3 + 0xF) & ~0xF;
            }

            outBytes.AddValue(skeletonNamesCount);
            long stringCursor = stringDataStart;
            List<int> skeletonOffsets = new List<int>();
            List<byte[]> strings = new List<byte[]>();
            for (int i = 0; i < skeletonNamesCount; i++)
            {
                string name = skeletonNames.names[i];
                byte[] str = System.Text.Encoding.UTF8.GetBytes(name);
                skeletonOffsets.Add((int)(stringCursor - (skeletonNamesListStart + 4 + i * 4)));
                strings.Add(str);
                stringCursor += str.Length + 1;
            }
            foreach (var off in skeletonOffsets)
            {
                outBytes.AddValue(off);
            }
            for (int i = 0; i < pad1Size; i++) 
            {
                outBytes.Add((byte)0);
            };

            if (morphTargetsListSize > 0)
            {
                long secondaryListStart = outBytes.Count;
                outBytes.AddValue(morphCount);
                for (int i = 0; i < morphCount; i++)
                {
                    string name = morphTargetNames.names[i];
                    byte[] str = System.Text.Encoding.UTF8.GetBytes(name);
                    int off = (int)(stringCursor - (secondaryListStart + 4 + i * 4));
                    outBytes.AddValue(off);
                    strings.Add(str);
                    stringCursor += str.Length + 1;
                }
            }

            //Morph params, one byte per entry.
            if(morphCount > 0 )
            {
                outBytes.AddValue(morphCount);
                for (int i = 0; i < morphCount; i++)
                {
                    outBytes.Add(morphTargetFlags[i]);
                }
            }

            int current = outBytes.Count;
            while (current < stringDataStart)
            {
                outBytes.AddValue((byte)0);
                current++;
            }

            //Strings
            foreach (var str in strings)
            {
                outBytes.AddValue(str);
                outBytes.Add((byte)0);
            }

            return outBytes.ToArray();
        }
    }


}
