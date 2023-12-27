using AquaModelLibrary.Helpers.Readers;

namespace AquaModelLibrary.Data.PSO2.Aqua.MusFileRebootData.Composition
{
    //Switchable
    public class Part
    {
        public List<Movement> movements = new List<Movement>();
        public string conditionString;
        public PartStruct partStruct;

        public Part()
        {

        }

        public Part(BufferedStreamReaderBE<MemoryStream> sr, int offset)
        {
            partStruct = sr.Read<PartStruct>();

            var bookmark = sr.Position;

            sr.Seek(offset + partStruct.movementOffset, SeekOrigin.Begin);
            for (int i = 0; i < partStruct.movementCount; i++)
            {
                movements.Add(new Movement(sr, offset));
            }

            if (partStruct.conditionStringOffset != 0x10 && partStruct.conditionStringOffset != 0)
            {
                sr.Seek(offset + partStruct.conditionStringOffset, SeekOrigin.Begin);
                conditionString = sr.ReadCString();
            }
            sr.Seek(bookmark, SeekOrigin.Begin);
        }
    }

    public struct PartStruct
    {
        public int movementOffset;
        public byte movementCount;
        public byte bt_5;
        public byte bt_6;
        public byte bt_7;

        public int conditionStringOffset;
        public int int_0C;

        public int int_10;
        public int int_14;
        public int int_18;
        public int int_1C;

        public int int_20;
        public int int_24;
        public int int_28;
    }
}
