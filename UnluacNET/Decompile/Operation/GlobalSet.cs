using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnluacNET
{
    public class GlobalSet : Operation
    {
        private string m_global;
        private Expression m_value;

        public override Statement Process(Registers r, Block block)
        {
            return new Assignment(new GlobalTarget(m_global), m_value);
        }

        public GlobalSet(int line, string global, Expression value)
            : base(line)
        {
            m_global = global;
            m_value = value;
        }
    }
}
