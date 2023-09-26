using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoulsFormats.Formats.Morpheme.NSA
{
    public class NSAVec3
    {
        public short X;
        public short Y;
        public short Z;

        public NSAVec3() { }

        public NSAVec3(BinaryReaderEx br)
        {
            X = br.ReadInt16();
            Y = br.ReadInt16();
            Z = br.ReadInt16();
        }
    }
}
