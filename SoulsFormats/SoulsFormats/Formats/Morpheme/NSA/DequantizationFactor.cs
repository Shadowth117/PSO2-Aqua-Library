using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace SoulsFormats.Formats.Morpheme.NSA
{
    public class DequantizationFactor
    {
        public Vector3 min; 
        public Vector3 scaledExtent; 

        public DequantizationFactor() { }

        public DequantizationFactor(BinaryReaderEx br)
        {
            min = br.ReadVector3();
            scaledExtent = br.ReadVector3();
        }
    }
}
