using AquaModelLibrary.Helpers.Readers;

namespace AquaModelLibrary.Data.PSO2.Aqua.MusFileRebootData
{
    public class MusHeader
    {
        public MusHeaderStruct mus;

        public MusHeader()
        {
        }

        public MusHeader(BufferedStreamReaderBE<MemoryStream> sr)
        {
            mus = sr.Read<MusHeaderStruct>();
        }
    }

    public struct MusHeaderStruct
    {
        public int compositionOffset;
        public byte bt_4;
        public byte compositionCount;
        public byte bt_6;
        public byte bt_7;
        public int int_08;
        public int int_0C;

        public byte bt_10;
        public byte bt_11;
        public byte bt_12;
        public byte transitionDataCount;
        public int transitionDataOffset;
        public byte bt_18;
        public byte bt_19;
        public byte bt_1A;
        public byte bt_1B;
        public int string_1COffset;

        public byte conditionDataCount;
        public byte bt_21;
        public byte bt_22;
        public byte bt_23;
        public int conditionDataOffset;
        public int customVariableStringOffset;
    }
}
