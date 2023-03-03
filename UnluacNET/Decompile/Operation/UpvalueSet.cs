using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnluacNET
{
    public class UpvalueSet : Operation
    {
        private UpvalueTarget m_target;
        private Expression m_value;

        public override Statement Process(Registers r, Block block)
        {
            return new Assignment(m_target, m_value);
        }

        public UpvalueSet(int line, string upvalue, Expression value)
            : base(line)
        {
            m_target = new UpvalueTarget(upvalue);
            m_value = value;
        }
    }
}
