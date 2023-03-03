using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnluacNET
{
    public class ClosureExpression : Expression
    {
        private readonly LFunction m_function;
        private int m_upvalueLine;
        private Declaration[] m_declList;

        public override int ConstantIndex
        {
            get { return -1; }
        }

        public override int ClosureUpvalueLine
        {
            get { return m_upvalueLine; }
        }

        public override bool IsClosure
        {
            get { return true; }
        }

        public override bool IsUpvalueOf(int register)
        {
            foreach (var upvalue in m_function.UpValues)
            {
                if (upvalue.InStack && upvalue.Index == register)
                    return true;
            }

            return false;
        }

        public override void Print(Output output)
        {
            var d = new Decompiler(m_function);
            output.Print("function");

            PrintMain(output, d, true);
        }

        public override void PrintClosure(Output output, Target name)
        {
            var d = new Decompiler(m_function);
            output.Print("function ");

            if (m_function.NumParams >= 1 && d.DeclList[0].Name.Equals("self") &&
                name is TableTarget)
            {
                name.PrintMethod(output);
                PrintMain(output, d, false);
            }
            else
            {
                name.Print(output);
                PrintMain(output, d, true);
            }
        }

        private void PrintMain(Output output, Decompiler d, bool includeFirst)
        {
            output.Print("(");

            var start = includeFirst ? 0 : 1;

            if (m_function.NumParams > start)
            {
                new VariableTarget(d.DeclList[start]).Print(output);

                for (int i = start + 1; i < m_function.NumParams; i++)
                {
                    output.Print(", ");
                    new VariableTarget(d.DeclList[i]).Print(output);
                }
            }

            if ((m_function.VarArg & 1) == 1)
                output.Print((m_function.NumParams > start) ? ", ..." : "...");

            output.Print(")");
            output.PrintLine();

            output.IncreaseIndent();

            d.Decompile();
            d.Print(output);

            output.DecreaseIndent();
            output.Print("end");
            //output.PrintLine(); //This is an extra space for formatting
        }

        public ClosureExpression(LFunction function, Declaration[] declList, int upvalueLine)
            : base(PRECEDENCE_ATOMIC)
        {
            m_function = function;
            m_upvalueLine = upvalueLine;
            m_declList = declList;
        }
    }
}
