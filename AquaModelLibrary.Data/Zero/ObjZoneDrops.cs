using AquaModelLibrary.Data.PSO2.Aqua;
using AquaModelLibrary.Data.PSO2.Aqua.AquaCommonData;
using Reloaded.Memory.Streams;

namespace AquaModelLibrary.Data.Zero
{
    public class ObjZoneDrops : AquaCommon
    {
        public List<int> itemIds = new List<int>();
        public List<int> rates = new List<int>();
        public List<int> unk0 = new List<int>();
        public List<int> unk1 = new List<int>();
        public int itemCount;
        public int unk0Count;
        public int unk1Count;

        //These are ints that are placed after the header stuff. In PSO2, this is junk so we'll assume it is here, but just in case we'll fill it anyways
        public int junk0;
        public int junk1;

        public ObjZoneDrops() { }

        public ObjZoneDrops(byte[] file)
        {
            Read(file);
        }

        public void Read(byte[] file)
        {
            using (MemoryStream stream = new MemoryStream(file))
            using (var streamReader = new BufferedStreamReader<MemoryStream>(stream))
            {
                int offset = 0x20;
                nifl = streamReader.Read<NIFL>();
                rel0 = streamReader.Read<REL0>();

                streamReader.Seek(offset + rel0.REL0DataStart, SeekOrigin.Begin);
                int idPointer = streamReader.Read<int>();
                int ratePointer = streamReader.Read<int>();
                int unkPointer0 = streamReader.Read<int>();
                int unkPointer1 = streamReader.Read<int>();

                itemCount = (ratePointer - idPointer) / 4;
                unk0Count = (unkPointer0 - ratePointer) / 2;
                unk1Count = (unkPointer1 - unkPointer0) / 2;
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

                //Read unk0
                streamReader.Seek(offset + unkPointer0, SeekOrigin.Begin);
                for (int i = 0; i < unk0Count; i++)
                {
                    unk0.Add(streamReader.Read<ushort>());
                }

                //Read unk1
                streamReader.Seek(offset + unkPointer1, SeekOrigin.Begin);
                for (int i = 0; i < unk1Count; i++)
                {
                    unk1.Add(streamReader.Read<ushort>());
                }
            }
        }
    }
}
