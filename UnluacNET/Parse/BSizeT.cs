using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnluacNET
{
    public class BSizeT : BInteger
    {
        public BSizeT(BInteger b) : base(b) { }
        public BSizeT(int n) : base(n) { }
        public BSizeT(long n) : base(n) { }
    }
}
