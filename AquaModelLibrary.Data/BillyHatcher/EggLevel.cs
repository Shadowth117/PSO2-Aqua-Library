using AquaModelLibrary.Data.BillyHatcher.ARCData;
using AquaModelLibrary.Helpers.Readers;

namespace AquaModelLibrary.Data.BillyHatcher
{
    /// <summary>
    /// egglevel.bin
    /// Slightly misleading since the POF0 offsets bleed into the actual data if not looking carefully. Last 4 bytes represent the 3 pointers and padding.
    /// </summary>
    public class EggLevel
    {
        public ARCHeader arcHeader;
        public EggLevelHeader header;
        public List<EggData0> eggData0List = new List<EggData0>();
        public List<GrowthData> growthDataList = new List<GrowthData>();
        public List<byte> eggData2List = new List<byte>();

        public enum Fruit : byte
        {
            None = 0x0,
            Apple = 0x1,
            Banana = 0x2,
            Cherry = 0x4,
            Melon = 0x8,
            Pineapple = 0x10,
            Strawberry = 0x20,
            WaterMelon = 0x40,
        }

        public struct EggLevelHeader
        {
            public int data0Count;
            public int data0Offset;
            public int growthDataCount;
            public int growthDataOffset;
            public int data2Count;
            public int data2Offset;
        }

        public struct EggData0
        {
            public ushort usht0;
            public ushort usht1;
        }

        public struct GrowthData
        {
            public byte eggId;
            public byte bt_01; //Always 0
            public byte bt_02;
            /// <summary>
            /// Something to do with amount of fruits the egg can consume. Should be at least as high as the final stage's count. All retail eggs match this with final stage count.
            /// Setting this to 1 and the stage count to 1 will allow an egg that can instantly hatch.
            /// </summary>
            public byte fruitsAllowedThing;

            /// <summary>
            /// Still counts its consumed fruits as 0, which makes putting this above 1 awkward.
            /// </summary>
            public byte startingStage;
            /// <summary>
            /// Amount of bytes of stage fruit counts used. Amount to progress to final stage always reliant on final entry.
            /// Maxes out at 9!
            /// </summary>
            public byte stageCount;
            public Fruit fruitPreferences;
            public byte stageFruitCount0;

            public byte stageFruitCount1;
            public byte stageFruitCount2;
            public byte stageFruitCount3;
            public byte stageFruitCount4;

            public byte stageFruitCount5;
            public byte stageFruitCount6;
            public byte stageFruitCount7;
            public byte stageFruitCount8;
        }

        public EggLevel() { }
        public EggLevel(BufferedStreamReaderBE<MemoryStream> sr)
        {
            //Generic ARC header
            arcHeader = new ARCHeader();
            arcHeader.fileSize = sr.ReadBE<int>();
            arcHeader.pof0Offset = sr.ReadBE<int>();
            arcHeader.pof0OffsetsSize = sr.ReadBE<int>();
            arcHeader.group1FileCount = sr.ReadBE<int>();

            arcHeader.group2FileCount = sr.ReadBE<int>();
            arcHeader.magic = sr.ReadBE<int>();
            arcHeader.unkInt0 = sr.ReadBE<int>();
            arcHeader.unkInt1 = sr.ReadBE<int>();

            header = new EggLevelHeader();
            header.data0Count = sr.ReadBE<int>();
            header.data0Offset = sr.ReadBE<int>();
            header.growthDataCount = sr.ReadBE<int>();
            header.growthDataOffset = sr.ReadBE<int>();
            header.data2Count = sr.ReadBE<int>();
            header.data2Offset = sr.ReadBE<int>();

            sr.Seek(0x20 + header.data0Offset, SeekOrigin.Begin);
            for (int i = 0; i < header.data0Count; i++)
            {
                EggData0 eggData = new EggData0();
                eggData.usht0 = sr.Read<ushort>();
                eggData.usht1 = sr.Read<ushort>();
                eggData0List.Add(eggData);
            }

            sr.Seek(0x20 + header.growthDataOffset, SeekOrigin.Begin);
            for (int i = 0; i < header.data0Count; i++)
            {
                GrowthData growthData = new GrowthData();
                growthData.eggId = sr.Read<byte>();
                growthData.bt_01 = sr.Read<byte>();
                growthData.bt_02 = sr.Read<byte>();
                growthData.fruitsAllowedThing = sr.Read<byte>();

                growthData.startingStage = sr.Read<byte>();
                growthData.stageCount = sr.Read<byte>();
                growthData.fruitPreferences = sr.Read<Fruit>();
                growthData.stageFruitCount0 = sr.Read<byte>();

                growthData.stageFruitCount1 = sr.Read<byte>();
                growthData.stageFruitCount2 = sr.Read<byte>();
                growthData.stageFruitCount3 = sr.Read<byte>();
                growthData.stageFruitCount4 = sr.Read<byte>();

                growthData.stageFruitCount5 = sr.Read<byte>();
                growthData.stageFruitCount6 = sr.Read<byte>();
                growthData.stageFruitCount7 = sr.Read<byte>();
                growthData.stageFruitCount8 = sr.Read<byte>();
                growthDataList.Add(growthData);
            }

            eggData2List.AddRange(sr.ReadBytes(header.data2Offset, header.data2Count));
        }
    }
}
