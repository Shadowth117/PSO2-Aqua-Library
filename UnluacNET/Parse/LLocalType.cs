using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace UnluacNET
{
    public class LLocalType : BObjectType<LLocal>
    {
        public override LLocal Parse(Stream stream, BHeader header)
        {
            var name = header.String.Parse(stream, header);

            var start = header.Integer.Parse(stream, header);
            var end = header.Integer.Parse(stream, header);

            if (header.Debug)
            {
                Console.WriteLine("-- parsing local, name: {0} from {1} to {2}",
                    name, start.AsInteger(), end.AsInteger());
            }

            return new LLocal(name, start, end);
        }
    }
}
