using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnluacNET
{
    public class TableReference : Expression
    {
        private readonly Expression m_table;
        private readonly Expression m_index;

        public override int ConstantIndex
        {
            get { return Math.Max(m_table.ConstantIndex, m_index.ConstantIndex); }
        }

        public override bool IsDotChain
        {
            get { return m_index.IsIdentifier && m_table.IsDotChain; }
        }

        public override bool IsMemberAccess
        {
            get { return m_index.IsIdentifier; }
        }

        public override string GetField()
        {
            return m_index.AsName();
        }

        public override Expression GetTable()
        {
            return m_table;
        }

        public override void Print(Output output)
        {
            m_table.Print(output);

            if (m_index.IsIdentifier)
            {
                output.Print(".");
                output.Print(m_index.AsName());
            }
            else
            {
                output.Print("[");
                m_index.Print(output);
                output.Print("]");
            }
        }

        public TableReference(Expression table, Expression index)
            : base(PRECEDENCE_ATOMIC)
        {
            m_table = table;
            m_index = index;
        }
    }
}
