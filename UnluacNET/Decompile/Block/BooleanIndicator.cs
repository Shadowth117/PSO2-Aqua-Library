using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnluacNET
{
    public class BooleanIndicator : Block
    {
        public override void AddStatement(Statement statement)
        {
            // Do nothing
            return;
        }

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
            get { return false; }
        }

        public override int GetLoopback()
        {
            throw new InvalidOperationException();
        }

        public override void Print(Output output)
        {
            output.Print("-- unhandled boolean indicator");
        }

        public BooleanIndicator(LFunction function, int line)
            : base(function, line, line)
        {
        }
    }
}
