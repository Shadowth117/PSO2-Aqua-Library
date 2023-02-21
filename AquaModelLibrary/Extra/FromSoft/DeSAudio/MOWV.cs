using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AquaModelLibrary.Extra.FromSoft.DeSAudio
{
    public class MOWV
    {
        public int magic;
        public short sht_04;
        public short sht_06;
        public int fileSize;
        public int entryCount; //Actual entries seem grouped into pairs, but I think it counts the 3rd row of 0x10 in the file as one too?

        public string fileName; //Max length of 0xC? 
        public int bt_1C;
        public int bt_1D;
        public int bt_1E;
        public int bt_1F;

        public int int_20;
        public int int_24;
        public int int_28;
        public int reserve0;

        public List<MOWVEntry> entries = new List<MOWVEntry>();

        public struct MOWVEntry
        {
            public byte bt_0;
            public byte bt_1;
            public byte bt_2;
            public byte bt_3;
            public int int_4;
            public int sndFileSize;
            public int reserve0;

            public int reserve1;
            public short sht_14;
            public short sht_16;
            public int reserve2;
            public short sht_1C;
            public short sht_1E;
        }
    }
}
