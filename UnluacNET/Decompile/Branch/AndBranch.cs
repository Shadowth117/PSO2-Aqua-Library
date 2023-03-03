using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnluacNET
{
    public class AndBranch : Branch
    {
        private readonly Branch m_left;
        private readonly Branch m_right;

        public override Expression AsExpression(Registers registers)
        {
            return Expression.MakeAND(m_left.AsExpression(registers), m_right.AsExpression(registers));
        }

        public override int GetRegister()
        {
            var rLeft = m_left.GetRegister();
            var rRight = m_right.GetRegister();

            return (rLeft == rRight) ? rLeft : -1;
        }

        public override Branch Invert()
        {
            return new OrBranch(m_left.Invert(), m_right.Invert());
        }

        public override void UseExpression(Expression expression)
        {
            m_left.UseExpression(expression);
            m_right.UseExpression(expression);
        }

        public AndBranch(Branch left, Branch right)
            : base(right.Line, right.Begin, right.End)
        {
            m_left = left;
            m_right = right;
        }
    }
}
