using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnluacNET
{
    public class AlwaysLoop : Block
    {
        private readonly List<Statement> m_statements;

        public override bool Breakable
        {
            get { return true; }
        }

        public override bool IsContainer
        {
            get { return true; }
        }

        public override bool IsUnprotected
        {
            get { return true; }
        }

        public override int ScopeEnd
        {
            get { return End - 2; }
        }

        public override int GetLoopback()
        {
            return Begin;
        }

        public override void AddStatement(Statement statement)
        {
            m_statements.Add(statement);
        }

        public override void Print(Output output)
        {
            output.PrintLine("while true do");
            output.IncreaseIndent();

            Statement.PrintSequence(output, m_statements);

            output.DecreaseIndent();
            output.Print("end");
        }

        public AlwaysLoop(LFunction function, int begin, int end)
            : base(function, begin, end)
        {
            m_statements = new List<Statement>();
        }
    }
}
