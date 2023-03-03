using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace UnluacNET
{
    public class Output
    {
        private TextWriter m_writer;

        private int m_indentationLevel = 0;
        private int m_position = 0;

        public int IndentationLevel
        {
            get { return m_indentationLevel; }
            set { m_indentationLevel = value; }
        }

        public int Position
        {
            get { return m_position; }
        }

        public void IncreaseIndent()
        {
            m_indentationLevel += 2;
        }

        public void DecreaseIndent()
        {
            m_indentationLevel -= 2;
        }

        private void Start()
        {
            if (m_position == 0)
            {
                for (int i = m_indentationLevel; i != 0; i--)
                {
                    m_writer.Write(" ");
                    m_position++;
                }
            }
        }

        public void Print(string str)
        {
            Start();
            m_writer.Write(str);
            m_position += str.Length;
        }

        public void PrintLine()
        {
            Start();
            m_writer.WriteLine();
            m_position = 0;
        }

        public void PrintLine(string str)
        {
            Start();
            m_writer.WriteLine(str);
            m_position = 0;
        }

        public Output() : this(Console.Out) { }
        public Output(TextWriter writer)
        {
            m_writer = writer;
        }
    }
}
