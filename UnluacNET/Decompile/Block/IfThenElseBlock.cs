using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnluacNET
{
    public class IfThenElseBlock : Block
    {
        private readonly Branch m_branch;
        private readonly int m_loopback;
        private readonly Registers m_r;
        private readonly List<Statement> m_statements;
        private readonly bool m_emptyElse;

        public ElseEndBlock Partner { get; set; }

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
            get { return true; }
        }

        public override int ScopeEnd
        {
            get { return End - 2; }
        }

        public override void AddStatement(Statement statement)
        {
            m_statements.Add(statement);
        }

        public override int CompareTo(Block block)
        {
            if (block == Partner)
                return -1;

            return base.CompareTo(block);
        }

        public override int GetLoopback()
        {
            return m_loopback;
        }

        public override void Print(Output output)
        {
            output.Print("if ");

            m_branch.AsExpression(m_r).Print(output);

            output.Print(" then");
            output.PrintLine();

            output.IncreaseIndent();

            //Handle the case where the "then" is empty in if-then-else.
            //The jump over the else block is falsely detected as a break.
            if (m_statements.Count == 1 && m_statements[0] is Break)
            {
                var b = m_statements[0] as Break;

                if (b.Target == m_loopback)
                {
                    output.DecreaseIndent();
                    return;
                }
            }

            Statement.PrintSequence(output, m_statements);

            output.DecreaseIndent();

            if (m_emptyElse)
            {
                output.PrintLine("else");
                output.PrintLine("end");
            }
        }

        public IfThenElseBlock(LFunction function, Branch branch, int loopback, bool emptyElse, Registers r)
            : base(function, branch.Begin, branch.End)
        {
            m_branch = branch;
            m_loopback = loopback;
            m_emptyElse = emptyElse;
            m_r = r;

            m_statements = new List<Statement>(branch.End - branch.Begin + 1);
        }
    }
}
