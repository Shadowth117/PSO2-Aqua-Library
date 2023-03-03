using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnluacNET
{
    public class CompareBlock : Block
    {
        public int Target { get; set; }
        public Branch Branch { get; set; }

        public override bool Breakable
        {
            get { return false; }
        }

        public override bool IsContainer
        {
            get { return false; }
        }

        public override bool IsUnprotected
        {
            get { return false; }
        }

        public override void AddStatement(Statement statement)
        {
            // Do nothing
            return;
        }

        public override int GetLoopback()
        {
            throw new InvalidOperationException();
        }

        public override void Print(Output output)
        {
            output.Print("-- unhandled compare assign");
        }

        public override Operation Process(Decompiler d)
        {
            return new LambdaOperation(End - 1, (r, block) => {
                return new RegisterSet(End - 1, Target, Branch.AsExpression(r)).Process(r, block);
            });
        }

        public CompareBlock(LFunction function, int begin, int end, int target, Branch branch)
            : base(function, begin, end)
        {
            Target = target;
            Branch = branch;
        }
    }
}
