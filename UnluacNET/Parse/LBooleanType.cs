using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace UnluacNET
{
    public class LBooleanType : BObjectType<LBoolean>
    {
        public override LBoolean Parse(Stream stream, BHeader header)
        {
            var value = stream.ReadByte();

            if ((value & 0xFFFFFFFE) != 0)
            {
                throw new InvalidOperationException();
            }
            else
            {
                var boolean = (value == 0) ? LBoolean.LFALSE : LBoolean.LTRUE;

                if (header.Debug)
                    Console.WriteLine("-- parsed <boolean> " + boolean);

                return boolean;
            }
        }
    }
}
