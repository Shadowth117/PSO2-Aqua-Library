using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AquaModelLibrary.Nova.Structures
{
    public class XgmiStruct
    {
        public int magic;
        public int len;
        public int int_08;
        public int paddingLen;

        public int int_10;
        public int int_14;
        public int int_18;
        public int int_1C;

        public int int_20;
        public int int_24;
        public ushort width;
        public ushort height;
        public int int_2C;

        public int int_30;
        public int int_34;
        public int int_38;
        public int int_3C;

        public long md5_1;
        public long md5_2;

        public int int_50;
        public int int_54;
        public int int_58;
        public int int_5C;

        public int int_60;
        public int int_64;
        public int int_68;
        public int int_6C;
    }
}
