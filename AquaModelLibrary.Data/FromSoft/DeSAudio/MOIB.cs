namespace AquaModelLibrary.Data.FromSoft.DeSAudio
{
    public class MOIB
    {
        public int magic;
        public short sht_04;
        public short sht_06;
        public int entryCount;
        public int reserve0;

        public List<MOIBEntry> moibEntries = new List<MOIBEntry>();

        public struct MOIBEntry
        {
            public int id;
            public byte bt_04;
            public byte bt_05;
            public byte bt_06;
            public byte bt_07;
            public byte bt_08;
            public byte bt_09;
            public ushort shortPadding;
            public int int_0C;

            public int int_10;
            public byte bt_14;
            public byte bt_15;
            public byte bt_16;
            public byte bt_17;
            public int reserve0;
        }
    }
}
