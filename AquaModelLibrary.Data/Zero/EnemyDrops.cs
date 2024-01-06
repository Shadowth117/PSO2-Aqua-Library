using AquaModelLibrary.Data.PSO2.Aqua;
using AquaModelLibrary.Data.PSO2.Aqua.AquaCommonData;
using Reloaded.Memory.Streams;

namespace AquaModelLibrary.Data.Zero
{
    public struct EnemyDropSet
    {
        public int item0Id;
        public int item1Id;
        public int item2Id;
        public int item3Id;
        public int item4Id;
        public ushort item0Rate;
        public ushort item1Rate;
        public ushort item2Rate;
        public ushort item3Rate;
        public ushort item4Rate;
        public ushort padding;
    }

    public struct EnemyData
    {
        public int id;
        public int item0Id;
        public int item1Id;
        public int item2Id;
        public int item3Id;
        public ushort u16_14;
        public ushort u16_16;
        public ushort u16_18;
        public ushort u16_1A;
        public ushort u16_1C;
        public ushort u16_1E;
        public ushort u16_20;
        public ushort u16_22;
    }

    public class EnemyDrops : AquaCommon
    {
        public List<EnemyDropSet> enemyDropSets = new List<EnemyDropSet>();
        public List<EnemyData> enemyData = new List<EnemyData>();

        public EnemyDrops() { }

        public EnemyDrops(byte[] file)
        {
            Read(file);
        }

        public void Read(byte[] file)
        {
            using (MemoryStream stream = new MemoryStream(file))
            using (var streamReader = new BufferedStreamReader<MemoryStream>(stream))
            {
                Read(streamReader);
            }
        }

        public void Read(BufferedStreamReader<MemoryStream> streamReader)
        {
            int offset = 0x20;
            nifl = streamReader.Read<NIFL>();
            rel0 = streamReader.Read<REL0>();

            streamReader.Seek(offset + rel0.REL0DataStart, SeekOrigin.Begin);
            ushort dropSetCount = streamReader.Read<ushort>();
            ushort enemyDropDataCount = streamReader.Read<ushort>();
            int dropSetPointer = streamReader.Read<int>();
            int enemyDropDataPointer = streamReader.Read<int>();

            //Read drop sets
            streamReader.Seek(offset + dropSetPointer, SeekOrigin.Begin);
            for (int i = 0; i < dropSetCount; i++)
            {
                enemyDropSets.Add(streamReader.Read<EnemyDropSet>());
            }

            //Read enemy drop data
            streamReader.Seek(offset + enemyDropDataPointer, SeekOrigin.Begin);
            for (int i = 0; i < enemyDropDataCount; i++)
            {
                enemyData.Add(streamReader.Read<EnemyData>());
            }
        }
    }
}
