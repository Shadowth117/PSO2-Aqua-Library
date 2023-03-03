using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnluacNET
{
    public class UpvalueTarget : Target
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

        public UpvalueTarget(string name)
        {
            m_name = name;
        }
    }
}
