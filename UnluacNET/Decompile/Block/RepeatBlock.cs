using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnluacNET
{
    public class RepeatBlock : Block
    {
        private readonly Branch m_branch;
        private readonly Registers m_r;
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
            output.Print("repeat");
            output.PrintLine();

            output.IncreaseIndent();

            Statement.PrintSequence(output, m_statements);

            output.DecreaseIndent();

            output.Print("until ");
            m_branch.AsExpression(m_r).Print(output);
        }

        public RepeatBlock(LFunction function, Branch branch, Registers r)
            : base(function, branch.End, branch.Begin)
        {
            m_branch = branch;
            m_r = r;

            m_statements = new List<Statement>(branch.Begin - branch.End + 1);
        }
    }
}
