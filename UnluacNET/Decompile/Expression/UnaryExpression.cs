using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnluacNET
{
    public class UnaryExpression : Expression
    {
        private readonly string m_op;
        private readonly Expression m_expression;

        public override int ConstantIndex
        {
            get { return m_expression.ConstantIndex; }
        }

        public override void Print(Output output)
        {
            var precedence = (Precedence > m_expression.Precedence);
            
            output.Print(m_op);

            if (precedence)
                output.Print("(");

            m_expression.Print(output);

            if (precedence)
                output.Print(")");
        }

        public UnaryExpression(string op, Expression expression, int precedence)
            : base(precedence)
        {
            m_op = op;
            m_expression = expression;
        }
    }
}
