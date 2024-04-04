using AquaModelLibrary.Helpers.Readers;

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
                    long absoluteOffset = relativeOffset + sr.Position;

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

        public int crc; //?
        public int unk0;
        public int secondaryNameCount;
        public int primaryNameOffsetListOffset;

        public int secondaryNameOffsetListOffset;
        public int secondaryParamOffsetListOffset;
        public CSKLNameList primaryNames = null;
        public CSKLNameList secondaryNames = null;

        public List<int> secondaryParams = new List<int>();

        //Helper
        public int primaryNameOffsetListAbsoluteOffset;

        public long secondaryNameOffsetListAbsoluteOffset;
        public long secondaryParamOffsetListAbsoluteOffset;


        public CSKLNames()
        {

        }
        public CSKLNames(BufferedStreamReaderBE<MemoryStream> sr, int csklVersion, int boneCount = 0)
        {
            crc = sr.Read<int>();
            unk0 = sr.Read<int>();
            var unk1 = sr.Read<int>();

            switch (csklVersion)
            {
                case 0x9:
                    primaryNames = new CSKLNameList(sr, boneCount);
                    break;
                case 0x19:
                    secondaryNameCount = sr.Read<int>();
                    primaryNameOffsetListOffset = sr.Read<int>();

                    var secondaryNameOffsetListOffsetPosition = sr.Position;
                    var secondaryParamOffsetListOffsetPosition = secondaryNameOffsetListOffsetPosition + 4;
                    secondaryNameOffsetListOffset = sr.Read<int>();
                    secondaryParamOffsetListOffset = sr.Read<int>();

                    secondaryNameOffsetListAbsoluteOffset = secondaryNameOffsetListOffsetPosition + secondaryNameOffsetListOffset;
                    secondaryParamOffsetListAbsoluteOffset = secondaryParamOffsetListOffsetPosition + secondaryParamOffsetListOffset;
                    //Primary Name List
                    primaryNames = new CSKLNameList(sr);
                    if (secondaryNameCount > 0 && secondaryNameOffsetListOffset > 0)
                    {
                        sr.Seek(secondaryNameOffsetListAbsoluteOffset, System.IO.SeekOrigin.Begin);
                        secondaryNames = new CSKLNameList(sr);
                        sr.Seek(secondaryParamOffsetListAbsoluteOffset, System.IO.SeekOrigin.Begin);
                        int paramCount = sr.Read<int>();
                        for (int i = 0; i < paramCount; i++)
                        {
                            secondaryParams.Add(sr.Read<int>());
                        }
                    }

                    //Conditionally seek to end
                    if (secondaryNames != null && secondaryNames.names.Count > 0)
                    {
                        sr.Seek(secondaryNames.absoluteOffsets[secondaryNames.absoluteOffsets.Count - 1], System.IO.SeekOrigin.Begin);
                    }
                    else
                    {
                        sr.Seek(primaryNames.absoluteOffsets[primaryNames.absoluteOffsets.Count - 1], System.IO.SeekOrigin.Begin);
                    }
                    sr.ReadCStringSeek(0x500);
                    break;
            }
        }
    }


}
