using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnluacNET
{
    public class WhileBlock : Block
    {
        private readonly Branch m_branch;
        private readonly int m_loopback;
        private readonly Registers m_registers;
        private readonly List<Statement> m_statements;

        public override int ScopeEnd
        {
            get { return End - 2; }
        }

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

        public override void AddStatement(Statement statement)
        {
            m_statements.Add(statement);
        }

        public override int GetLoopback()
        {
            return m_loopback;
        }

        public override void Print(Output output)
        {
            output.Print("while ");
            
            m_branch.AsExpression(m_registers).Print(output);

            output.Print(" do");
            output.PrintLine();

            output.IncreaseIndent();

            Statement.PrintSequence(output, m_statements);
            
            output.DecreaseIndent();

            output.Print("end");
        }

        public WhileBlock(LFunction function, Branch branch, int loopback, Registers registers)
            : base(function, branch.Begin, branch.End)
        {
            m_branch = branch;
            m_loopback = loopback;
            m_registers = registers;

            m_statements = new List<Statement>(branch.End - branch.Begin + 1);
        }
    }
}
