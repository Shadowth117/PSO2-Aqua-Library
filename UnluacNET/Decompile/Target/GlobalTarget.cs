using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnluacNET
{
    public class GlobalTarget : Target
    {
        private readonly string m_name;

        public override void Print(Output output)
        {
            output.Print(m_name);
        }

        public override void PrintMethod(Output output)
        {
            throw new InvalidOperationException();
        }

        public GlobalTarget(string name)
        {
            m_name = name;
        }
    }
}
