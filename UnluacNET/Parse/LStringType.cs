using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using UnluacNET.IO;

namespace UnluacNET
{
    public class LStringType : BObjectType<LString>
    {
        public override LString Parse(Stream stream, BHeader header)
        {
            var sizeT = header.SizeT.Parse(stream, header);

            var sb = new StringBuilder();

            sizeT.Iterate(() => {
                sb.Append(stream.ReadChar());
            });

            var str = sb.ToString();

            if (header.Debug)
                Console.WriteLine("-- parsed <string> \"" + str + "\"");

            return new LString(sizeT, str);
        }
    }
}
