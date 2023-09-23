using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SoulsFormats.Formats.Morpheme.NSA
{
    public class DequantizationInfo
    {
        public byte[] init; //3 bytes
        public byte[] factorIdx; //3 bytes

        public DequantizationInfo() { }

        public DequantizationInfo(BinaryReaderEx br)
        {
            init = br.ReadBytes(3);
            factorIdx = br.ReadBytes(3);
        }
    }
}
