using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnluacNET
{
    public class VariableTarget : Target
    {
        public Declaration Declaration { get; private set; }

        public override bool IsLocal
        {
            get { return true; }
        }

        public override bool IsDeclaration(Declaration decl)
        {
            return Declaration == decl;
        }

        public override bool Equals(object obj)
        {
            if (obj is VariableTarget)
                return Declaration == ((VariableTarget)obj).Declaration;
            else
                return false;
        }

        public override int GetIndex()
        {
            return Declaration.Register;
        }

        public override void Print(Output output)
        {
            output.Print(Declaration.Name);
        }

        public override void PrintMethod(Output output)
        {
            throw new InvalidOperationException();
        }

        public VariableTarget(Declaration decl)
        {
            Declaration = decl;
        }
    }
}
