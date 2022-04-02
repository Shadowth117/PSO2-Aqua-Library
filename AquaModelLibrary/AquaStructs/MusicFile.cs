namespace AquaModelLibrary.AquaStructs
{
    public class MusicFile : AquaCommon
    {
        public int musHeaderOffset;

        public class MusHeader
        {
            public MusHeaderStruct mus;
            public string musString1C = null;
            public string musString28 = null;
        }

        public struct MusHeaderStruct
        {
            public int subStrOffset;
            public byte bt_4;
            public byte bt_5;
            public byte bt_6;
            public byte bt_7;
            public int int_08;
            public int int_0C;

            public byte bt_10;
            public byte bt_11;
            public byte bt_12;
            public byte bt_13;
            public int offset_14;
            public byte bt_18;
            public byte bt_19;
            public byte bt_1A;
            public byte bt_1B;
            public int str_1C;

            public byte bt_20;
            public byte bt_21;
            public byte bt_22;
            public byte bt_23;
            public int offset_24;
            public int str_28;
        }
    }
}
