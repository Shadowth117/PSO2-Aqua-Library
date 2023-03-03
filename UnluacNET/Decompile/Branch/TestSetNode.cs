using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnluacNET
{
    public class TestSetNode : TestNode
    {
        public override Expression AsExpression(Registers registers)
        {
            return registers.GetExpression(Test, Line);
        }

        public override int GetRegister()
        {
            return SetTarget;
        }

        public override Branch Invert()
        {
            return new TestSetNode(SetTarget, Test, !Inverted, Line, End, Begin);
        }

        public override string ToString()
        {
            return String.Format("TestSetNode[target={0};test={1};inverted={2};line={3};begin={4};end={5}]",
                Test, Inverted, Line, Begin, End);
        }

        public TestSetNode(int target, int test, bool inverted, int line, int begin, int end)
            : base(test, inverted, line, begin, end)
        {
            SetTarget = target;
        }
    }
}
