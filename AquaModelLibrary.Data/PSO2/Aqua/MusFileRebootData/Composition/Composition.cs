using AquaModelLibrary.Helpers.Readers;

namespace AquaModelLibrary.Data.PSO2.Aqua.MusFileRebootData.Composition
{
    //Normal, transition, battle, etc.
    public class Composition
    {
        public List<Part> parts = new List<Part>();
        public List<CompositionCondition> compositionConditions = new List<CompositionCondition>();
        public CompositionStruct compositionStruct;

        public Composition() { }

        public Composition(BufferedStreamReaderBE<MemoryStream> sr, int offset)
        {
            compositionStruct = sr.Read<CompositionStruct>();

            var bookmark = sr.Position;

            sr.Seek(offset + compositionStruct.partOffset, SeekOrigin.Begin);
            for (int i = 0; i < compositionStruct.partCount; i++)
            {
                parts.Add(new Part(sr, offset));
            }

            sr.Seek(offset + compositionStruct.compositionConditionOffset, SeekOrigin.Begin);
            for (int i = 0; i < compositionStruct.compositionConditionCount; i++)
            {
                compositionConditions.Add(new CompositionCondition(sr, offset));
            }
            sr.Seek(bookmark, SeekOrigin.Begin);
        }
    }

    public struct CompositionStruct
    {
        public int partOffset;
        public int compositionConditionOffset;
        public byte partCount;
        public byte bt_9;
        public byte compositionConditionCount;
        public byte bt_B;
    }
}
