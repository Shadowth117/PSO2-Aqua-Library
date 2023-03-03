using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnluacNET
{
    public class DoEndBlock : Block
    {
        private readonly List<Statement> m_statements;

        public override bool Breakable
        {
            get { return false; }
        }

        public override bool IsContainer
        {
            get { return true; }
        }

        public override bool IsUnprotected
        {
            get { return false; }
        }

        public override void AddStatement(Statement statement)
        {
            m_statements.Add(statement);
        }

        public override int GetLoopback()
        {
            throw new InvalidOperationException();
        }

        public override void Print(Output output)
        {
            output.PrintLine("do");
            output.IncreaseIndent();

            Statement.PrintSequence(output, m_statements);

            output.DecreaseIndent();
            output.Print("end");
        }

        public DoEndBlock(LFunction function, int begin, int end)
            : base(function, begin, end)
        {
            m_statements = new List<Statement>(end - begin + 1);
        }
    }
}
