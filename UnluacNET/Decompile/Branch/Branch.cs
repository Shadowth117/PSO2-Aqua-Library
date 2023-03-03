using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnluacNET
{
    public abstract class Branch
    {
        private int m_setTarget = -1;

        public int Line { get; private set; }
        
        public int Begin { get; set; }
        public int End { get; set; } // Might be modified to undo redirect

        public bool IsSet { get; set; }
        public bool IsCompareSet { get; set; }
        public bool IsTest { get; set; }

        public int SetTarget
        {
            get { return m_setTarget; }
            set { m_setTarget = value; }
        }

        public abstract Branch Invert();
        public abstract int GetRegister();
        public abstract Expression AsExpression(Registers registers);
        public abstract void UseExpression(Expression expression);

        public Branch(int line, int begin, int end)
        {
            Line = line;
            Begin = begin;
            End = end;
        }
    }
}
