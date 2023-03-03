using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnluacNET
{
    public class NotBranch : Branch
    {
        private readonly Branch m_branch;

        public override Expression AsExpression(Registers registers)
        {
            return Expression.MakeNOT(m_branch.AsExpression(registers));
        }

        public override int GetRegister()
        {
            return m_branch.GetRegister();
        }

        public override Branch Invert()
        {
            return m_branch;
        }

        public override void UseExpression(Expression expression)
        {
            // Do nothing
            return;
        }

        public NotBranch(Branch branch)
            : base(branch.Line, branch.Begin, branch.End)
        {
            m_branch = branch;
        }
    }
}
