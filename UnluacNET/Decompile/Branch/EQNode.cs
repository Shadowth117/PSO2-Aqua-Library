using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnluacNET
{
    public class EQNode : Branch
    {
        private readonly int m_left;
        private readonly int m_right;
        private readonly bool m_invert;

        public override Branch Invert()
        {
            return new EQNode(m_left, m_right, !m_invert, Line, End, Begin);
        }

        public override int GetRegister()
        {
            return -1;
        }

        public override Expression AsExpression(Registers registers)
        {
            var transpose = false;
            var op = (m_invert) ? "~=" : "==";

            return new BinaryExpression(op,
                registers.GetKExpression(!transpose ? m_left : m_right, Line),
                registers.GetKExpression(!transpose ? m_right : m_left, Line),
                Expression.PRECEDENCE_COMPARE,
                Expression.ASSOCIATIVITY_LEFT);
        }

        public override void UseExpression(Expression expression)
        {
            /* Do nothing */
            return;
        }

        public EQNode(int left, int right, bool invert, int line, int begin, int end)
            : base(line, begin, end)
        {
            m_left = left;
            m_right = right;
            m_invert = invert;
        }
    }
}
