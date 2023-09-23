using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoulsFormats.Formats.Morpheme.NSA
{
    public class IndexList
    {
        public short count;
        public List<short> indices = new List<short>();

        public IndexList() { }

        public IndexList(BinaryReaderEx br)
        {
            count = br.ReadInt16();
            for (int i = 0; i < indices.Count; i++)
            {
                indices[i] = br.ReadInt16();
            }
        }
    }
}
