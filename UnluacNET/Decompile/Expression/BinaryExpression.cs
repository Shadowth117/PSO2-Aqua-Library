using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnluacNET
{
    public class BinaryExpression : Expression
    {
        private readonly string m_op;
        private readonly Expression m_left;
        private readonly Expression m_right;
        private readonly int m_associativity;

        protected bool LeftGroup
        {
            get
            {
                return (Precedence > m_left.Precedence) ||
                    (Precedence == m_left.Precedence && m_associativity == ASSOCIATIVITY_RIGHT);
            }
        }

        protected bool RightGroup
        {
            get
            {
                return (Precedence > m_right.Precedence) ||
                    (Precedence == m_right.Precedence && m_associativity == ASSOCIATIVITY_LEFT);
            }
        }

        public override int ConstantIndex
        {
            get
            {
                return Math.Max(m_left.ConstantIndex, m_right.ConstantIndex);
            }
        }

        public override bool BeginsWithParen
        {
            get { return LeftGroup || m_left.BeginsWithParen; }
        }

        public override void Print(Output output)
        {
            var leftGroup = LeftGroup;
            var rightGroup = RightGroup;

            if (leftGroup)
                output.Print("(");

            m_left.Print(output);

            if (leftGroup)
                output.Print(")");

            output.Print(" ");
            output.Print(m_op);
            output.Print(" ");

            if (rightGroup)
                output.Print("(");

            m_right.Print(output);

            if (rightGroup)
                output.Print(")");
        }

        public BinaryExpression(String op, Expression left, Expression right, int precedence, int associativity)
            : base(precedence)
        {
            m_op = op;
            m_left = left;
            m_right = right;
            m_associativity = associativity;
        }
    }
}
