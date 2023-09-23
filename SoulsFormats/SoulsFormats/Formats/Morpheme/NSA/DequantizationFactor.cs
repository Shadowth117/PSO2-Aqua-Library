using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoulsFormats.Formats.Morpheme.NSA
{
    public class DequantizationFactor
    {
        public float[] min; //3 floats
        public float[] scaledExtent; //3 floats

        public DequantizationFactor() { }

        public DequantizationFactor(BinaryReaderEx br)
        {
            min = br.ReadSingles(3);
            scaledExtent = br.ReadSingles(3);
        }
    }
}
