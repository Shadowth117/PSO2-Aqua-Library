using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnluacNET
{
    public class BList<T> : BObject
        where T : BObject
    {
        private readonly List<T> m_values;
        
        public BInteger Length { get; private set; }

        public T this[int index]
        {
            get { return m_values[index]; }
        }

        public T[] AsArray()
        {
            return m_values.AsParallel().ToArray();
        }

        public BList(BInteger length, List<T> values)
        {
            Length = length;
            m_values = values;
        }
    }
}
