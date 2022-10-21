using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AquaModelLibrary.NNStructs.Structures
{
    public struct NNIndexList
    {
        public int flags; //In most cases, it'll just be tristrip ushorts
        public int indexCount;
        public int stripCount;
        public int lengthOffset;

        public int indexOffset;
        public int int_14;
        public int int_18;
        public int int_1C;
    }
}
