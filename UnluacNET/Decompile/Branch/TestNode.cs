using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnluacNET
{
    public class TestNode : Branch
    {
        public int Test { get; private set; }
        public bool Inverted { get; private set; }

        public override Expression AsExpression(Registers registers)
        {
            if (Inverted)
                return new NotBranch(Invert()).AsExpression(registers);

            return registers.GetExpression(Test, Line);
        }

        public override int GetRegister()
        {
            return Test;
        }

        public override Branch Invert()
        {
            return new TestNode(Test, !Inverted, Line, End, Begin);
        }

        public override void UseExpression(Expression expression)
        {
            // Do nothing
            return;
        }

        public override string ToString()
        {
            return String.Format("TestNode[test={0};inverted={1};line={2};begin={3};end={4}]",
                Test, Inverted, Line, Begin, End);
        }

        public TestNode(int test, bool inverted, int line, int begin, int end)
            : base(line, begin, end)
        {
            Test = test;
            Inverted = inverted;

            IsTest = true;
        }
    }
}
