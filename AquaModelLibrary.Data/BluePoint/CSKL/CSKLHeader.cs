using AquaModelLibrary.Helpers.Readers;

namespace AquaModelLibrary.Data.BluePoint.CSKL
{
    public class CSKLHeader
    {
        public int magic;
        public int crc;
        public int crc2; //Always the same as first crc??
        public int fileSize;

        public ushort boneCount;
        public ushort boneCount2; //Same as first
        public ushort secondaryBoneCount;
        public ushort flags;
        public ushort usht_18; //Variant or some count maybe?
        public ushort usht_1A; //Some kind of count
        public int int_1C;

        public int transformListOffset; //Counts from beginning of file
        public int size1;
        public int inverseBoneMatricesOffset;
        public int int_3C;

        public int int_40; //int_40-int_48 might always be 0
        public int int_44;
        public int int_48;
        public int boneMetadataOffset; //Data including parent ids, child ids, sibling ids, etc.

        public ushort boneCount3; //Same numbers again
        public ushort boneCount4;
        public ushort secondaryBoneCount2;
        public ushort secondaryBoneCount3;
        public int int_58;
        public int int_5C;

        //SOTC
        public int boneNamesOffset;

        public CSKLHeader()
        {

        }

        public CSKLHeader(BufferedStreamReaderBE<MemoryStream> sr, int csklVersion)
        {
            magic = sr.Read<int>();
            switch (csklVersion)
            {
                case 0x9:
                    var footerOffset = sr.Read<int>();
                    var unkSize0 = sr.Read<int>();
                    var unkSize1 = sr.Read<int>();

                    boneCount = sr.Read<ushort>();
                    var unkCount0 = sr.Read<ushort>();
                    var boneDepth = sr.Read<ushort>();
                    var unkUsht0 = sr.Read<ushort>();
                    transformListOffset = sr.Read<int>() + 0x18;
                    var unknownMetaData0Offset = sr.Read<int>() + 0x1C;

                    var unknownMetaData1Offset = sr.Read<int>() + 0x20;
                    var unknownMetaData2Offset = sr.Read<int>() + 0x24;
                    var unknownMetaData3Offset = sr.Read<int>() + 0x28;
                    var unknownMetaData4Offset = sr.Read<int>() + 0x2C;

                    boneNamesOffset = sr.Read<int>() + 0x30;
                    var unkInt0 = sr.Read<int>();
                    var unkInt1 = sr.Read<int>();
                    var unkInt2 = sr.Read<int>();
                    break;
                case 0x19:
                    crc = sr.Read<int>();
                    crc2 = sr.Read<int>();
                    fileSize = sr.Read<int>();

                    boneCount = sr.Read<ushort>();
                    boneCount2 = sr.Read<ushort>();
                    secondaryBoneCount = sr.Read<ushort>();
                    flags = sr.Read<ushort>();
                    usht_18 = sr.Read<ushort>();
                    usht_1A = sr.Read<ushort>();
                    int_1C = sr.Read<int>();

                    transformListOffset = sr.Read<int>();
                    size1 = sr.Read<int>();
                    inverseBoneMatricesOffset = sr.Read<int>();
                    int_3C = sr.Read<int>();

                    int_40 = sr.Read<int>();
                    int_44 = sr.Read<int>();
                    int_48 = sr.Read<int>();
                    boneMetadataOffset = sr.Read<int>();

                    boneCount3 = sr.Read<ushort>();
                    boneCount4 = sr.Read<ushort>();
                    secondaryBoneCount2 = sr.Read<ushort>();
                    secondaryBoneCount3 = sr.Read<ushort>();
                    int_58 = sr.Read<int>();
                    int_5C = sr.Read<int>();
                    int_5C = sr.Read<int>();
                    break;
            }
        }
    }
}
