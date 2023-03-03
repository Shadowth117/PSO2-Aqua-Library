using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnluacNET
{
    public class Return : Statement
    {
        private Expression[] values;

        public override void Print(Output output)
        {
            output.Print("do ");
            PrintTail(output);
            output.Print(" end");
        }

        public override void PrintTail(Output output)
        {
            output.Print("return");

            if (values.Length > 0)
            {
                output.Print(" ");

                var rtns = new List<Expression>(values.Length);

                foreach (var value in values)
                    rtns.Add(value);

                Expression.PrintSequence(output, rtns, false, true);
            }
        }

        public Return()
        {
            values = new Expression[0];
        }

        public Return(Expression value)
        {
            values = new Expression[1] {
                value
            };
        }

        public Return(Expression[] values)
        {
            this.values = values;
        }
    }
}
