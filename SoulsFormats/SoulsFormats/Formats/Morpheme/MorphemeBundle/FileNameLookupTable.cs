using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoulsFormats.Formats.Morpheme.MorphemeBundle
{
    public class FileNameLookupTable : MorphemeBundle_Base
    {
        public ulong animTableOffset;
        public ulong formatTableOffset;
        public ulong sourceTableOffset;
        public ulong tagTableOffset;
        public ulong hashOffset;
        public LookupTable animlist;
        public LookupTable animFormat;
        public LookupTable sourceXmdList;
        public LookupTable tagList;
        public List<int> hashList;

        public override ulong CalculateBundleSize()
        {
            throw new NotImplementedException();
        }
    }
}
