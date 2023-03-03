using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace UnluacNET
{
    public class LUpvalueType : BObjectType<LUpvalue>
    {
        public override LUpvalue Parse(Stream stream, BHeader header)
        {
            return new LUpvalue() {
                InStack = (stream.ReadByte() != 0),
                Index   = stream.ReadByte()
            };;
        }
    }
}
