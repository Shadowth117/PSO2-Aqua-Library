using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnluacNET
{
    public class Break : Block
    {
        public int Target { get; private set; }

        public override bool Breakable
        {
            get { return false; }
        }

        public override bool IsContainer
        {
            get { return false; }
        }

        public override bool IsUnprotected
        {
            //Actually, it is unprotected, but not really a block
            get { return false; }
        }

        public override void AddStatement(Statement statement)
        {
            throw new InvalidOperationException();
        }

        public override int GetLoopback()
        {
            throw new InvalidOperationException();
        }

        public override void Print(Output output)
        {
            output.Print("do return end");
        }

        public override void PrintTail(Output output)
        {
            output.Print("break");
        }

        public Break(LFunction function, int line, int target)
            : base(function, line, line)
        {
            Target = target;
        }
    }
}
