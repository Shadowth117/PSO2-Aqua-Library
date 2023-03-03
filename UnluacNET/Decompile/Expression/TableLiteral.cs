using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnluacNET
{
    public class TableLiteral : Expression
    {
        public sealed class Entry : IComparable<Entry>
        {
            public Expression Key { get; private set; }
            public Expression Value { get; private set; }

            public bool IsList { get; private set; }
            public int Timestamp { get; private set; }

            public int CompareTo(Entry e)
            {
                return Timestamp.CompareTo(e.Timestamp);
            }

            public Entry(Expression key, Expression value, bool isList, int timestamp)
            {
                Key = key;
                Value = value;
                IsList = isList;
                Timestamp = timestamp;
            }
        }

        private List<Entry> m_entries;

        private bool m_isObject = true;
        private bool m_isList   = true;
        private int m_listLength  = 1;
        private int m_capacity;

        private void PrintEntry(int index, Output output)
        {
            var entry = m_entries[index];
            
            var key = entry.Key;
            var value = entry.Value;

            var isList = entry.IsList;
            var multiple = (index + 1 >= m_entries.Count) || value.IsMultiple;

            if (isList && key.IsInteger && m_listLength == key.AsInteger())
            {
                if (multiple)
                    value.PrintMultiple(output);
                else
                    value.Print(output);

                m_listLength++;
            }
            else if (m_isObject && key.IsIdentifier)
            {
                output.Print(key.AsName());
                output.Print(" = ");

                value.Print(output);
            }
            else
            {
                output.Print("[");
                key.Print(output);
                output.Print("] = ");
                value.Print(output);
            }
        }

        public override int ConstantIndex
        {
            get
            {
                var index = -1;

                foreach (var entry in m_entries)
                {
                    index = Math.Max(entry.Key.ConstantIndex, index);
                    index = Math.Max(entry.Value.ConstantIndex, index);
                }

                return index;
            }
        }

        public override bool IsBrief
        {
            get { return false; }
        }

        public override bool IsNewEntryAllowed
        {
            get { return m_entries.Count < m_capacity; }
        }

        public override bool IsTableLiteral
        {
            get { return true; }
        }

        public override void AddEntry(Entry entry)
        {
            m_entries.Add(entry);

            m_isObject  = m_isObject && (entry.IsList || entry.Key.IsIdentifier);
            m_isList    = m_isList && entry.IsList;
        }

        public override void Print(Output output)
        {
            m_entries.Sort();
            m_listLength = 1;

            if (m_entries.Count == 0)
            {
                output.Print("{}");
            }
            else
            {
                var lineBreak = (m_isList && m_entries.Count > 5) ||
                                (m_isObject && m_entries.Count > 2) ||
                                (!m_isObject);

                if (!lineBreak)
                {
                    foreach (var entry in m_entries)
                    {
                        var value = entry.Value;

                        if (!value.IsBrief)
                        {
                            lineBreak = true;
                            break;
                        }
                    }
                }

                output.Print("{");

                if (lineBreak)
                {
                    output.PrintLine();
                    output.IncreaseIndent();
                }

                PrintEntry(0, output);

                if (!m_entries[0].Value.IsMultiple)
                {
                    for (int i = 1; i < m_entries.Count; i++)
                    {
                        output.Print(",");

                        if (lineBreak)
                            output.PrintLine();
                        else
                            output.Print(" ");

                        PrintEntry(i, output);

                        if (m_entries[i].Value.IsMultiple)
                            break;
                    }
                }

                if (lineBreak)
                {
                    output.PrintLine();
                    output.DecreaseIndent();
                }

                output.Print("}");
            }
        }

        public TableLiteral(int arraySize, int hashSize)
            : base(PRECEDENCE_ATOMIC)
        {
            m_capacity = arraySize + hashSize;
            m_entries = new List<Entry>(m_capacity);
        }
    }
}
