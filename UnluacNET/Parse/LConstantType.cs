using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace UnluacNET
{
    public class LConstantType : BObjectType<LObject>
    {
        private static readonly string[] m_constantTypes = {
            "<nil>",
            "<boolean>",
            null, // no type?
            "<number>",
            "<string>"
        };

        public override LObject Parse(Stream stream, BHeader header)
        {
            var type = stream.ReadByte();

            if (header.Debug)
            {
                if (type < m_constantTypes.Length)
                {
                    var cType = m_constantTypes[type];

                    Console.WriteLine("-- parsing <constant>, type is {0}",
                        (type != 2) ? cType : "illegal " + type);
                }
            }

            switch (type)
            {
            case 0:
                return LNil.NIL;
            case 1:
                return header.Bool.Parse(stream, header);
            case 3:
                return header.Number.Parse(stream, header);
            case 4:
                return header.String.Parse(stream, header);
            default:
                throw new InvalidOperationException();
            }
        }
    }
}
