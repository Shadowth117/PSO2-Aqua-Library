using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoulsFormats.Formats.Morpheme.NSA
{
    public class NSAVec3
    {
        public short x;
        public short y;
        public short z;

        public NSAVec3() { }

        public NSAVec3(BinaryReaderEx br)
        {
            x = br.ReadInt16();
            y = br.ReadInt16();
            z = br.ReadInt16();
        }
    }
}
