using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoulsFormats.Formats.Morpheme.NSA
{
    public class RotationSample
    {
        public NSAVec3 sample;

        public RotationSample() { }

        public RotationSample(BinaryReaderEx br) 
        {
            sample = new NSAVec3(br);
        }

    }
}
