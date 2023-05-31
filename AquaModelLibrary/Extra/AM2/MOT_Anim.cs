using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AquaModelLibrary.Extra.AM2
{
    public class MOT_Anim
    {
        public struct motionHeader
        {
            public int int_00;
            public int motionCount;
            public int motionOffsets;
            public int int_0C;

            public int motionNameOffsets;
        }

        public struct motionDataHeader
        {
            public int int_00;
            public int int_04;
            public int offset0ByteCount;
            public int reserve0;

            public int offset0;
            public int reserve1;
            public int offset1HalfFloatCount;
            public int reserve2;

            public int offset1;
            public int reserve3;
            public int reserve4;
            public int reserve5;
        }
    }
}
