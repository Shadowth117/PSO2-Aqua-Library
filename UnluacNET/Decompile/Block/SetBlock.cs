using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnluacNET
{
    public class SetBlock : Block
    {
        private Assignment m_assign;
        private bool m_empty;
        private bool m_finalize;
        private Registers m_r;

        public int Target { get; private set; }
        public Branch Branch { get; private set; }

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
            if (!m_finalize && statement is Assignment)
                m_assign = statement as Assignment;
            else if (statement is BooleanIndicator)
                m_finalize = true;
        }

        public override int GetLoopback()
        {
            throw new InvalidOperationException();
        }

        public Expression GetValue()
        {
            return Branch.AsExpression(m_r);
        }

        public override void Print(Output output)
        {
            if (m_assign != null)
            {
                var target = m_assign.GetFirstTarget();

                if (target != null)
                {
                    new Assignment(target, GetValue()).Print(output);
                }
                else
                {
                    output.Print("-- unhandled set block");
                    output.PrintLine();
                }
            }
        }

        public override Operation Process(Decompiler d)
        {
            if (m_empty)
            {
                var expr = m_r.GetExpression(Branch.SetTarget, End);
                Branch.UseExpression(expr);

                return new RegisterSet(End - 1, Branch.SetTarget, Branch.AsExpression(m_r));
            }
            else if (m_assign != null)
            {
                Branch.UseExpression(m_assign.GetFirstValue());

                var target = m_assign.GetFirstTarget();
                var value = GetValue();

                return new LambdaOperation(End - 1, (r, block) => {
                    return new Assignment(target, value);
                });
            }
            else
            {
                return new LambdaOperation(End - 1, (r, block) => {
                    Expression expr = null;

                    var register = 0;

                    for (; register < r.NumRegisters; register++)
                    {
                        if (r.GetUpdated(register, Branch.End - 1) == Branch.End - 1)
                        {
                            expr = r.GetValue(register, Branch.End);
                            break;
                        }
                    }

                    if (d.Code.Op(Branch.End - 2) == Op.LOADBOOL &&
                        d.Code.C(Branch.End - 2) != 0)
                    {
                        var target = d.Code.A(Branch.End - 2);

                        if (d.Code.Op(Branch.End - 3) == Op.JMP &&
                            d.Code.sBx(Branch.End - 3) == 2)
                        {
                            expr = r.GetValue(target, Branch.End - 2);
                        }
                        else
                        {
                            expr = r.GetValue(target, Branch.Begin);
                        }

                        Branch.UseExpression(expr);

                        if (r.IsLocal(target, Branch.End - 1))
                            return new Assignment(r.GetTarget(target, Branch.End - 1), Branch.AsExpression(r));

                        r.SetValue(target, Branch.End - 1, Branch.AsExpression(r));
                    }
                    else if (expr != null && Target >= 0)
                    {
                        Branch.UseExpression(expr);

                        if (r.IsLocal(Target, Branch.End - 1))
                            return new Assignment(r.GetTarget(Target, Branch.End - 1), Branch.AsExpression(r));

                        r.SetValue(Target, Branch.End - 1, Branch.AsExpression(r));
                    }
                    else
                    {
                        Console.WriteLine("-- fail " + (Branch.End - 1));
                        Console.WriteLine(expr);
                        Console.WriteLine(Target);
                    }

                    return null;
                });
            }
        }

        public void UseAssignment(Assignment assignment)
        {
            m_assign = assignment;
            Branch.UseExpression(assignment.GetFirstValue());
        }

        public SetBlock(LFunction function, Branch branch, int target, int line, int begin, int end, bool empty, Registers r)
            : base(function, begin, end)
        {
            m_empty = empty;

            if (begin == end)
                Begin -= 1;

            Target = target;
            Branch = branch;

            m_r = r;
        }
    }
}
