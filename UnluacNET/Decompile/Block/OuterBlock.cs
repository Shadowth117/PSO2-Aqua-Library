using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnluacNET
{
    public class OuterBlock : Block
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

        public override int ScopeEnd
        {
            get { return (End - 1) + Function.Header.Version.OuterBlockScopeAdjustment; }
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
            /* extra return statement */
            var last = m_statements.Count - 1;

            if (last < 0 || !(m_statements[last] is Return))
                throw new InvalidOperationException(m_statements[last].ToString());

            // this doesn't seem like appropriate behavior???
            m_statements.RemoveAt(last);

            Statement.PrintSequence(output, m_statements);
        }

        public OuterBlock(LFunction function, int length)
            : base(function, 0, length + 1)
        {
            m_statements = new List<Statement>(length);
        }
    }
}
