using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoulsFormats.Formats.Morpheme.MorphemeBundle
{
    public class LookupTable
    {
        public int elemCount;
        public int stringSize;
        public long idxOffset;
        public long localOffsetOffset;
        public long stringsOffset;
        public List<int> idxList = new List<int>(); 
        public List<int> localOffsets = new List<int>();
        public List<byte> strings = new List<byte>();
    }
}
