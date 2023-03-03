using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnluacNET
{
    public class Upvalues
    {
        private readonly LUpvalue[] m_upvalues;

        public string GetName(int idx)
        {
            if (idx < m_upvalues.Length && m_upvalues[idx].Name != null)
                return m_upvalues[idx].Name;

            return String.Format("_UPVALUE{0}_", idx);
        }

        public UpvalueExpression GetExpression(int index)
        {
            return new UpvalueExpression(GetName(index));
        }

        public Upvalues(LUpvalue[] upvalues)
        {
            m_upvalues = upvalues;
        }
    }
}
