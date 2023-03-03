using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnluacNET
{
    public class GlobalExpression : Expression
    {
        private readonly string m_name;
        private readonly int m_index;

        public override int ConstantIndex
        {
            get { return m_index; }
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

        public GlobalExpression(string name, int index)
            : base(PRECEDENCE_ATOMIC)
        {
            m_name = name;
            m_index = index;
        }
    }
}
