using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using UnluacNET.IO;

namespace UnluacNET
{
    public class BIntegerType : BObjectType<BInteger>
    {
        public int IntSize { get; private set; }

        public override BInteger Parse(Stream stream, BHeader header)
        {
            var value = RawParse(stream, header);

            if (header.Debug)
                Console.WriteLine("-- parsed <integer> " + value.AsInteger());

            return value;
        }

        protected internal BInteger RawParse(Stream stream, BHeader header)
        {
            // HACK HACK HACK
            var bigEndian = header.BigEndian;

            BInteger value = null;

            switch (IntSize)
            {
            case 0:
                value = new BInteger(0);
                break;
            case 1:
                value = new BInteger(stream.ReadByte());
                break;
            case 2:
                value = new BInteger(stream.ReadInt16(bigEndian));
                break;
            case 4:
                value = new BInteger(stream.ReadInt32(bigEndian));
                break;
            case 8:
                value = new BInteger(stream.ReadInt64(bigEndian));
                break;
            default:
                throw new InvalidOperationException("Bad IntSize, cannot parse data");
            //default:
            //    {
            //        var bytes = new byte[IntSize];
            //
            //        var start = 0;
            //        var delta = 1;
            //
            //        for (int i = start; i >= 0 && i < IntSize; i += delta)
            //            bytes[i] = (byte)stream.ReadByte();
            //
            //        value = new BInteger(BitConverter.ToInt64(bytes, 0));
            //    } break;
            }

            return value;
        }

        public BIntegerType(int intSize)
        {
            IntSize = intSize;
        }
    }
}
