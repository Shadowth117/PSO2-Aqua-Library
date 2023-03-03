using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnluacNET
{
    public class FunctionCallStatement : Statement
    {
        private FunctionCall m_call;

        public override bool BeginsWithParen
        {
            get { return m_call.BeginsWithParen; }
        }

        public override void Print(Output output)
        {
            m_call.Print(output);
        }

        public FunctionCallStatement(FunctionCall call)
        {
            m_call = call;
        }
    }
}
