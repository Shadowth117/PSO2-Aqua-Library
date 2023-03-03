using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnluacNET
{
    public class ForBlock : Block
    {
        private readonly int m_register;
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

        public override int ScopeEnd
        {
            get { return End - 2; }
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
            output.Print("for ");
            m_r.GetTarget(m_register + 3, Begin - 1).Print(output);
            output.Print(" = ");
            m_r.GetValue(m_register, Begin - 1).Print(output);
            output.Print(", ");
            m_r.GetValue(m_register + 1, Begin - 1).Print(output);

            var step = m_r.GetValue(m_register + 2, Begin - 1);

            if (!step.IsInteger || step.AsInteger() != 1)
            {
                output.Print(", ");
                step.Print(output);
            }

            output.Print(" do");
            output.PrintLine();

            output.IncreaseIndent();

            Statement.PrintSequence(output, m_statements);

            output.DecreaseIndent();

            output.Print("end");
        }

        public ForBlock(LFunction function, int begin, int end, int register, Registers r)
            : base(function, begin, end)
        {
            m_register = register;
            m_r = r;

            m_statements = new List<Statement>(end - begin + 1);
        }
    }
}
