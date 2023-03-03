using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnluacNET
{
    public class LBoolean : LObject
    {
        public static readonly LBoolean LTRUE = new LBoolean(true);
        public static readonly LBoolean LFALSE = new LBoolean(false);

        private readonly bool m_value;

        public override bool Equals(object obj)
        {
            return (this == obj);
        }

        public override string ToString()
        {
            return m_value.ToString();
        }

        private LBoolean(bool value)
        {
            m_value = value;
        }
    }
}
