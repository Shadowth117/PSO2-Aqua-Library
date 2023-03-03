using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnluacNET
{
    public class TableTarget : Target
    {
        private readonly Expression m_table;
        private readonly Expression m_index;

        public override bool IsFunctionName
        {
            get
            {
                if (!m_index.IsIdentifier)
                    return false;
                if (!m_table.IsDotChain)
                    return false;

                return true;
            }
        }

        public override void Print(Output output)
        {
            new TableReference(m_table, m_index).Print(output);
        }

        public override void PrintMethod(Output output)
        {
            m_table.Print(output);
            output.Print(":");
            output.Print(m_index.AsName());
        }

        public TableTarget(Expression table, Expression index)
        {
            m_table = table;
            m_index = index;
        }
    }
}
