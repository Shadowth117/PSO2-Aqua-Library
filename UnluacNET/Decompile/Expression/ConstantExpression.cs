using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnluacNET
{
    public class ConstantExpression : Expression
    {
        private readonly Constant m_constant;
        private readonly int m_index;

        public override int ConstantIndex
        {
            get { return m_index; }
        }

        public override bool IsConstant
        {
            get { return true; }
        }

        public override bool IsBoolean
        {
            get { return m_constant.IsBoolean; }
        }

        public override bool IsBrief
        {
            get { return !m_constant.IsString || m_constant.AsName().Length <= 10; }
        }

        public override bool IsIdentifier
        {
            get { return m_constant.IsIdentifier; }
        }

        public override bool IsInteger
        {
            get { return m_constant.IsInteger; }
        }

        public override bool IsString
        {
            get { return m_constant.IsString; }
        }

        public override bool IsNil
        {
            get { return m_constant.IsNil; }
        }

        public override int AsInteger()
        {
            return m_constant.AsInteger();
        }

        public override string AsName()
        {
            return m_constant.AsName();
        }

        public override void Print(Output output)
        {
            m_constant.Print(output);
        }

        public ConstantExpression(Constant constant, int index)
            : base(PRECEDENCE_ATOMIC)
        {
            m_constant = constant;
            m_index = index;
        }
    }
}
