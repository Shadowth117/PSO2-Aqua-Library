using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnluacNET
{
    public class Declaration
    {
        public String Name { get; private set; }
        
        public int Begin { get; private set; }
        public int End { get; private set; }

        public int Register { get; set; }

        //Whether this is an invisible for-loop book-keeping variable.
        internal bool ForLoop { get; set; }

        //Whether this is an explicit for-loop declared variable.
        internal bool ForLoopExplicit { get; set; }

        public Declaration(LLocal local)
        {
            Name = local.ToString();
            Begin = local.Start;
            End = local.End;
        }

        public Declaration(String name, int begin, int end)
        {
            Name = name;
            Begin = begin;
            End = end;
        }
    }
}
