using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnluacNET
{
    public class RegisterSet : Operation
    {
        public int Register { get; private set; }
        public Expression Value { get; private set; }

        public override Statement Process(Registers r, Block block)
        {
            r.SetValue(Register, Line, Value);

            if (r.IsAssignable(Register, Line))
                return new Assignment(r.GetTarget(Register, Line), Value);
            else
                return null;
        }

        public RegisterSet(int line, int register, Expression value)
            : base(line)
        {
            Register = register;
            Value = value;
        }
    }
}
