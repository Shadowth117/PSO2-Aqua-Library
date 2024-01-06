using AquaModelLibrary.Data.PSO2.Aqua;
using AquaModelLibrary.Data.PSO2.Aqua.AquaCommonData;
using Reloaded.Memory.Streams;

namespace AquaModelLibrary.Data.Zero
{
    public class EnemyZoneDrops : AquaCommon
    {
        public List<int> itemIds = new List<int>();
        public List<int> rates = new List<int>();
        public int itemCount;

        //These are ints that are placed after the header stuff. In PSO2, this is junk so we'll assume it is here, but just in case we'll fill it anyways
        public int junk0;
        public int junk1;

        public EnemyZoneDrops() { }

        public EnemyZoneDrops(byte[] file)
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
            int idPointer = streamReader.Read<int>();
            int ratePointer = streamReader.Read<int>();
            itemCount = (ratePointer - idPointer) / 4;
            //Read item ids
            streamReader.Seek(offset + idPointer, SeekOrigin.Begin);
            for (int i = 0; i < itemCount; i++)
            {
                itemIds.Add(streamReader.Read<int>());
            }

            //Read item rates. Count is the same and so should correlate to the same id
            streamReader.Seek(offset + ratePointer, SeekOrigin.Begin);
            for (int i = 0; i < itemCount; i++)
            {
                rates.Add(streamReader.Read<ushort>());
            }
        }
    }
}
