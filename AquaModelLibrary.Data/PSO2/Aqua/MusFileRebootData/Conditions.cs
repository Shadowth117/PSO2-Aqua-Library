using AquaModelLibrary.Helpers.Readers;

namespace AquaModelLibrary.Data.PSO2.Aqua.MusFileRebootData
{
    public class Conditions
    {
        public ConditionsStruct conditionstr;
        public string description = null;
        public string str_14 = null;
        public string str_18 = null;
        public string str_20 = null;
        public string str_28 = null;

        public Conditions()
        {

        }

        public Conditions(BufferedStreamReaderBE<MemoryStream> sr, int offset)
        {
            conditionstr = sr.Read<ConditionsStruct>();

            var bookmark = sr.Position;
            if (conditionstr.descriptionStrOffset != 0x10 && conditionstr.descriptionStrOffset != 0)
            {
                sr.Seek(offset + conditionstr.descriptionStrOffset, SeekOrigin.Begin);
                description = sr.ReadCString();
            }
            if (conditionstr.str_14 != 0x10 && conditionstr.str_14 != 0)
            {
                sr.Seek(offset + conditionstr.str_14, SeekOrigin.Begin);
                str_14 = sr.ReadCString();
            }
            if (conditionstr.str_18 != 0x10 && conditionstr.str_18 != 0)
            {
                sr.Seek(offset + conditionstr.str_18, SeekOrigin.Begin);
                str_18 = sr.ReadCString();
            }
            if (conditionstr.str_20 != 0x10 && conditionstr.str_20 != 0)
            {
                sr.Seek(offset + conditionstr.str_20, SeekOrigin.Begin);
                str_20 = sr.ReadCString();
            }
            if (conditionstr.str_28 != 0x10 && conditionstr.str_28 != 0)
            {
                sr.Seek(offset + conditionstr.str_28, SeekOrigin.Begin);
                str_28 = sr.ReadCString();
            }

            sr.Seek(bookmark, SeekOrigin.Begin);
        }
    }

    public struct ConditionsStruct
    {
        public int int_00;
        public int descriptionStrOffset;
        public int valueType;
        public float valueOne;

        public float valueTwo;
        public int str_14;
        public int str_18;
        public int int_1C;

        public int str_20;
        public byte bt_24;
        public byte bt_25;
        public byte bt_26;
        public byte bt_27;
        public int str_28;
        public int int_2C;
    }
}
