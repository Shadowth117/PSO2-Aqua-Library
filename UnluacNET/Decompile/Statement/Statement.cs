using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnluacNET
{
    public abstract class Statement
    {
        public string Comment { get; set; }

        public virtual bool BeginsWithParen
        {
            get { return false; }
        }

        public abstract void Print(Output output);

        public static void PrintSequence(Output output, List<Statement> statements)
        {
            var count = statements.Count;

            for (int i = 0; i < count; i++)
            {
                bool last = (i + 1 == count);

                var statement = statements[i];
                var next = last ? null : statements[i + 1];

                if (last)
                    statement.PrintTail(output);
                else
                    statement.Print(output);

                if (next != null && statement is FunctionCallStatement && next.BeginsWithParen)
                    output.Print(";");
                
                if (!(statement is IfThenElseBlock))
                    output.PrintLine();
            }
        }

        public virtual void PrintTail(Output output)
        {
            Print(output);
        }
    }
}
