using AquaModelLibrary.Helpers.Readers;

namespace AquaModelLibrary.Data.PSO2.Aqua.MusFileRebootData.Composition
{
    public class CompositionCondition
    {
        public string conditionString = null;
        public CompositionConditionStruct compositionConditionStruct;

        public CompositionCondition()
        {

        }

        public CompositionCondition(BufferedStreamReaderBE<MemoryStream> sr, int offset)
        {
            compositionConditionStruct = sr.Read<CompositionConditionStruct>();

            var bookmark = sr.Position;

            if (compositionConditionStruct.conditionStringOffset != 0x10 && compositionConditionStruct.conditionStringOffset != 0)
            {
                sr.Seek(offset + compositionConditionStruct.conditionStringOffset, SeekOrigin.Begin);
                conditionString = sr.ReadCString();
            }
            sr.Seek(bookmark, SeekOrigin.Begin);
        }
    }

    public struct CompositionConditionStruct
    {
        public int conditionStringOffset;
        public byte bt_4;
        public byte bt_5;
        public byte bt_6;
        public byte bt_7;
        public int int_08;
        public int int_0C;
        public int int_10;
    }
}
