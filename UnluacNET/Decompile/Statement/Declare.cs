using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnluacNET
{
    public class Declare : Statement
    {
        private readonly List<Declaration> m_decls;

        public override void Print(Output output)
        {
            output.Print("local ");
            output.Print(m_decls[0].Name);

            for (int i = 1; i < m_decls.Count; i++)
            {
                output.Print(", ");
                output.Print(m_decls[i].Name);
            }
        }

        public Declare(List<Declaration> decls)
        {
            m_decls = decls;
        }
    }
}
