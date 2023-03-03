using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnluacNET
{
    public class UpvalueExpression : Expression
    {
        private readonly string m_name;

        public override int ConstantIndex
        {
            get { return -1; }
        }

        public override bool IsBrief
        {
            get { return true; }
        }

        public override bool IsDotChain
        {
            get { return true; }
        }

        public override void Print(Output output)
        {
            output.Print(m_name);
        }

        public UpvalueExpression(string name)
            : base(PRECEDENCE_ATOMIC)
        {
            m_name = name;
        }
    }
}
