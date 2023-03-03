using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnluacNET
{
    public class TableSet : Operation
    {
        private Expression m_table;
        private Expression m_index;
        private Expression m_value;

        private bool m_isTable;
        private int m_timestamp;

        public override Statement Process(Registers r, Block block)
        {
            // .isTableLiteral() is sufficient when there is debugging info
            // TODO: Fix the commented out section screwing up tables
            if(m_table.IsTableLiteral /*&& (m_value.IsMultiple || m_table.IsNewEntryAllowed)*/)
            {
                m_table.AddEntry(new TableLiteral.Entry(m_index, m_value, !m_isTable, m_timestamp));
                return null;
            }
            else
            {
                return new Assignment(new TableTarget(m_table, m_index), m_value);
            }
        }

        public TableSet(int line,
            Expression table,
            Expression index,
            Expression value,
            bool isTable,
            int timestamp) : base(line)
        {
            m_table = table;
            m_index = index;
            m_value = value;

            m_isTable = isTable;
            m_timestamp = timestamp;
        }
    }
}
