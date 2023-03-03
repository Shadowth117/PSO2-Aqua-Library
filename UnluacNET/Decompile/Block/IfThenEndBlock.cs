using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnluacNET
{
    public class IfThenEndBlock : Block
    {
        private readonly Branch m_branch;
        private readonly Stack<Branch> m_stack;
        private readonly Registers m_r;
        private readonly List<Statement> m_statements;

        public override bool Breakable
        {
            get { return false; }
        }

        public override bool IsContainer
        {
            get { return true; }
        }

        public override bool IsUnprotected
        {
            get { return false; }
        }

        public override void AddStatement(Statement statement)
        {
            m_statements.Add(statement);
        }

        public override int GetLoopback()
        {
            throw new InvalidOperationException();
        }

        public override void Print(Output output)
        {
            output.Print("if ");

            m_branch.AsExpression(m_r).Print(output);

            output.Print(" then");
            output.PrintLine();

            output.IncreaseIndent();

            Statement.PrintSequence(output, m_statements);

            output.DecreaseIndent();
            output.Print("end");
        }

        public override Operation Process(Decompiler d)
        {
            if (m_statements.Count == 1)
            {
                var statement = m_statements[0];

                if (statement is Assignment)
                {
                    var assignment = statement as Assignment;

                    if (assignment.GetArity() == 1)
                    {
                        if (m_branch is TestNode)
                        {
                            var node = m_branch as TestNode;
                            var decl = m_r.GetDeclaration(node.Test, node.Line);

                            if (assignment.GetFirstTarget().IsDeclaration(decl))
                            {
                                var left = new LocalVariable(decl);
                                var right = assignment.GetFirstValue();

                                var expr = (node.Inverted)
                                    ? Expression.MakeOR(left, right)
                                    : Expression.MakeAND(left, right);

                                return new LambdaOperation(End - 1, (r, block) => {
                                    return new Assignment(assignment.GetFirstTarget(), expr);
                                });
                            }
                        }
                    }
                }
            }
            else if (m_statements.Count == 0 && m_stack != null)
            {
                var test = m_branch.GetRegister();

                if (test < 0)
                {
                    for (int reg = 0; reg < m_r.NumRegisters; reg++)
                    {
                        if (m_r.GetUpdated(reg, m_branch.End - 1) >= m_branch.Begin)
                        {
                            if (test >= 0)
                            {
                                test = -1;
                                break;
                            }

                            test = reg;
                        }
                    }
                }

                if (test >= 0)
                {
                    if (m_r.GetUpdated(test, m_branch.End - 1) >= m_branch.Begin)
                    {
                        var right = m_r.GetValue(test, m_branch.End);
                        var setb = d.PopSetCondition(m_stack, m_stack.Peek().End, test);

                        setb.UseExpression(right);

                        var testReg = test;

                        return new LambdaOperation(End - 1, (r, block) => {
                            r.SetValue(testReg, m_branch.End - 1, setb.AsExpression(r));
                            return null;
                        });
                    }
                }
            }

            return base.Process(d);
        }

        public IfThenEndBlock(LFunction function, Branch branch, Registers r)
            : this(function, branch, null, r)
        {

        }

        public IfThenEndBlock(LFunction function, Branch branch, Stack<Branch> stack, Registers r)
            : base(function,
            branch.Begin == branch.End ? branch.Begin - 1 : branch.Begin,
            branch.Begin == branch.End ? branch.Begin - 1 : branch.End)
        {
            m_branch = branch;
            m_stack = stack;
            m_r = r;

            m_statements = new List<Statement>(branch.End - branch.Begin + 1);
        }
    }
}
