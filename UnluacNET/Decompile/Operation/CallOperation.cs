using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnluacNET
{
    public class CallOperation : Operation
    {
        private FunctionCall m_call;

        public override Statement Process(Registers r, Block block)
        {
            return new FunctionCallStatement(m_call);
        }

        public CallOperation(int line, FunctionCall call) : base(line)
        {
            m_call = call;
        }
    }
}
