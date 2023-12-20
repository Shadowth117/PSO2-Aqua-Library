using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AquaModelLibrary.Nova.Structures
{
    public class ipnbStruct
    {
        public int magic;
        public int len;
        public int int_08;
        public int paddedLen;

        public List<short> shortList = new List<short>(); //List is ((len - 0x10) / 2) long.
    }
}
