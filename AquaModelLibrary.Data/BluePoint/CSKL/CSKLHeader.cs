using AquaModelLibrary.Helpers.Readers;

namespace AquaModelLibrary.Data.BluePoint.CSKL
{
    public class CSKLHeader
    {
        public int magic;
        /// <summary>
        /// Hash probably generated at export, probably unused
        /// </summary>
        public uint hash; 
        /// <summary>
        /// Same as the first one, again, seems unused
        /// </summary>
        public uint hash2; 
        public int fileSize;

        public ushort boneCount;
        public ushort boneCount2; //Same as first
        public ushort morphTargetCount;
        public byte partitionRecordCount;
        /// <summary>
        /// Related to bonecount, seemingly unused?
        /// </summary>
        public byte bt_17;
        /// <summary>
        /// Total number of bone partition entries.
        /// Game seems to only need a correct count for these between the partition records
        /// </summary>
        public ushort partitionEntryCount;
        /// <summary>
        /// 0x40 * The amount of parents of bones from different partition records.
        /// The game uses a table of blocks for handling these since there's bone limits per partitionRecord and allocates based on this.
        /// </summary>
        public ushort interPartitionParentBlocksSize; 
        /// <summary>
        /// Always observed 0
        /// </summary>
        public int int_1C;

        public int transformListOffset; //Counts from beginning of file
        /// <summary>
        /// Unused?
        /// </summary>
        public int transformEndOffset;
        public int inverseBoneMatricesOffset;
        public int partitionRecordOffset;

        public int partitionEntryOffset; 
        public int animOpCodeEntryOffset;
        public int int_48;
        public int boneMetadataOffset; //Data including parent ids, child ids, sibling ids, etc.

        //SOTC
        public int boneNamesOffset;

        public CSKLHeader() { }

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
                    hash = sr.Read<uint>();
                    hash2 = sr.Read<uint>();
                    fileSize = sr.Read<int>();

                    boneCount = sr.Read<ushort>();
                    boneCount2 = sr.Read<ushort>();
                    morphTargetCount = sr.Read<ushort>();
                    partitionRecordCount = sr.Read<byte>();
                    bt_17 = sr.Read<byte>();
                    partitionEntryCount = sr.Read<ushort>();
                    interPartitionParentBlocksSize = sr.Read<ushort>();
                    int_1C = sr.Read<int>();

                    transformListOffset = sr.Read<int>();
                    transformEndOffset = sr.Read<int>();
                    inverseBoneMatricesOffset = sr.Read<int>();
                    partitionRecordOffset = sr.Read<int>();

                    partitionEntryOffset = sr.Read<int>();
                    animOpCodeEntryOffset = sr.Read<int>();
                    int_48 = sr.Read<int>();
                    boneMetadataOffset = sr.Read<int>();
                    break;
            }
        }
    }
}
