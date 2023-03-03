using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnluacNET
{
    public class AssignNode : Branch
    {
        private Expression m_expression;

        public override Expression AsExpression(Registers registers)
        {
            return m_expression;
        }

        public override int GetRegister()
        {
            throw new InvalidOperationException();
        }

        public override Branch Invert()
        {
            throw new InvalidOperationException();
        }

        public override void UseExpression(Expression expression)
        {
            m_expression = expression;
        }

        public AssignNode(int line, int begin, int end)
            : base(line, begin, end)
        {

        }
    }
}
