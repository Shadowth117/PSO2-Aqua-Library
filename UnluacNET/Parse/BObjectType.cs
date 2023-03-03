using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace UnluacNET
{
    public abstract class BObjectType<T> : BObject
        where T : BObject
    {
        public abstract T Parse(Stream stream, BHeader header);

        public BList<T> ParseList(Stream stream, BHeader header)
        {
            var length = header.Integer.Parse(stream, header);
            var values = new List<T>();

            length.Iterate(() => {
                values.Add(Parse(stream, header));
            });

            return new BList<T>(length, values);
        }
    }
}
