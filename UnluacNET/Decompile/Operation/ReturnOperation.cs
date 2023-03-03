using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnluacNET
{
    public class ReturnOperation : Operation
    {
        private Expression[] m_values;

        public override Statement Process(Registers r, Block block)
        {
            return new Return(m_values);
        }

        public ReturnOperation(int line, Expression value)
            : base(line)
        {
            m_values = new Expression[1] {
                value
            };
        }

        public ReturnOperation(int line, Expression[] values)
            : base(line)
        {
            m_values = values;
        }
    }
}
