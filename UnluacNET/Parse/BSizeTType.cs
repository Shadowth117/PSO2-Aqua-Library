using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace UnluacNET
{
    public class BSizeTType : BObjectType<BSizeT>
    {
        private BIntegerType m_integerType;

        public int SizeTSize { get; private set; }

        public override BSizeT Parse(Stream stream, BHeader header)
        {
            var value = new BSizeT(m_integerType.RawParse(stream, header));

            if (header.Debug)
                Console.WriteLine("-- parsed <size_t> " + value.AsInteger());

            return value;
        }

        public BSizeTType(int sizeTSize)
        {
            SizeTSize = sizeTSize;
            m_integerType = new BIntegerType(sizeTSize);
        }
    }
}
