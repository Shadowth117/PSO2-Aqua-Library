using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnluacNET
{
    public abstract class LNumber : LObject
    {
        public static LNumber MakeInteger(int number)
        {
            return new LIntNumber(number);
        }

        public override bool Equals(object obj)
        {
            if (obj is LNumber)
                return Value == ((LNumber)obj).Value;

            return false;
        }

        // TODO: problem solution for this issue (???)
        public abstract double Value { get; }
    }

    public class LFloatNumber : LNumber
    {
        public float Number { get; private set; }

        public override double Value
        {
            get { return Number; }
        }

        public override bool Equals(object obj)
        {
            if (obj is LFloatNumber)
                return Number == ((LFloatNumber)obj).Number;

            return base.Equals(obj);
        }

        public override string ToString()
        {
            if (Number == (float)Math.Round(Number))
                return ((int)Number).ToString();
            else
                return Number.ToString();
        }

        public LFloatNumber(float number)
        {
            Number = number;
        }
    }

    public class LDoubleNumber : LNumber
    {
        public double Number { get; private set; }

        public override double Value
        {
            get { return Number; }
        }

        public override bool Equals(object obj)
        {
            if (obj is LDoubleNumber)
                return Number == ((LDoubleNumber)obj).Number;

            return base.Equals(obj);
        }

        public override string ToString()
        {
            if (Number == Math.Round(Number))
                return ((long)Number).ToString();
            else
                return Number.ToString();
        }

        public LDoubleNumber(double number)
        {
            Number = number;
        }
    }

    public class LIntNumber : LNumber
    {
        public int Number { get; private set; }

        public override double Value
        {
            get { return Number; }
        }

        public override bool Equals(object obj)
        {
            if (obj is LIntNumber)
                return Number == ((LIntNumber)obj).Number;
            
            return base.Equals(obj);
        }

        public override string ToString()
        {
            return Number.ToString();
        }

        public LIntNumber(int number)
        {
            Number = number;
        }
    }

    public class LLongNumber : LNumber
    {
        public long Number { get; private set; }

        public override double Value
        {
            get { return Number; }
        }

        public override bool Equals(object obj)
        {
            if (obj is LLongNumber)
                return Number == ((LLongNumber)obj).Number;

            return base.Equals(obj);
        }

        public override string ToString()
        {
            return Number.ToString();
        }

        public LLongNumber(long number)
        {
            Number = number;
        }
    }
}
