using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnluacNET
{
    public class TrueNode : Branch
    {
        public int Register { get; private set; }
        public bool Inverted { get; private set; }

        public override Expression AsExpression(Registers registers)
        {
            return new ConstantExpression(new Constant(Inverted ? LBoolean.LTRUE : LBoolean.LFALSE), -1);
        }

        public override int GetRegister()
        {
            return Register;
        }

        public override Branch Invert()
        {
            return new TrueNode(Register, !Inverted, Line, End, Begin);
        }

        public override void UseExpression(Expression expression)
        {
            // Do nothing
            return;
        }

        public override string ToString()
        {
            return String.Format("TrueNode[register={0};inverted={1};line={2};begin={3};end={4}]",
                Register, Inverted, Line, Begin, End);
        }

        public TrueNode(int register, bool inverted, int line, int begin, int end)
            : base(line, begin, end)
        {
            Register = register;
            Inverted = inverted;
            SetTarget = register;
            
            //???
            //IsTest = true;
        }
    }
}
