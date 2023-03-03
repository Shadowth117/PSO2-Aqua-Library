using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnluacNET
{
    public class LLocal : BObject
    {
        public LString Name { get; private set; }
        
        public int Start { get; private set; }
        public int End { get; private set; }

        /* Used by the decompiler for annotation. */
        internal bool ForLoop { get; set; }

        public override string ToString()
        {
            return Name.DeRef();
        }

        public LLocal(LString name, BInteger start, BInteger end)
        {
            Name = name;

            Start = start.AsInteger();
            End = end.AsInteger();
        }
    }
}
