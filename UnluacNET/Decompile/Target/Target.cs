using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnluacNET
{
    public abstract class Target
    {
        public virtual bool IsFunctionName
        {
            get { return true; }
        }

        public virtual bool IsLocal
        {
            get { return false; }
        }

        public virtual int GetIndex()
        {
            throw new InvalidOperationException();
        }

        public virtual bool IsDeclaration(Declaration decl)
        {
            return false;
        }

        public abstract void Print(Output output);
        public abstract void PrintMethod(Output output);
    }
}
