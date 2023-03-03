using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnluacNET
{
    public class Assignment : Statement
    {
        private readonly List<Target> m_targets     = new List<Target>(5);
        private readonly List<Expression> m_values  = new List<Expression>(5);

        private bool m_allNil       = true;
        private bool m_declare      = false;
        private int m_declareStart  = 0;

        public void AddFirst(Target target, Expression value)
        {
            m_targets.Insert(0, target);
            m_values.Insert(0, value);

            m_allNil = m_allNil && value.IsNil;
        }

        public void AddLast(Target target, Expression value)
        {
            if (m_targets.Contains(target))
            {
                var index = m_targets.IndexOf(target);
                value = m_values[index];
                
                m_targets.RemoveAt(index);
                m_values.RemoveAt(index);
            }

            m_targets.Add(target);
            m_values.Add(value);

            m_allNil = m_allNil && value.IsNil;
        }

        public bool AssignListEquals(List<Declaration> decls)
        {
            if (decls.Count != m_targets.Count)
                return false;

            foreach (var target in m_targets)
            {
                var found = false;

                foreach (var decl in decls)
                {
                    if (target.IsDeclaration(decl))
                    {
                        found = true;
                        break;
                    }
                }

                if (!found)
                    return false;
            }

            return true;
        }

        public bool AssignsTarget(Declaration decl)
        {
            foreach (var target in m_targets)
                if (target.IsDeclaration(decl))
                    return true;

            return false;
        }

        public void Declare(int declareStart)
        {
            m_declare = true;
            m_declareStart = declareStart;
        }

        public int GetArity()
        {
            return m_targets.Count;
        }

        public Target GetFirstTarget()
        {
            return m_targets[0];
        }

        public Expression GetFirstValue()
        {
            return m_values[0];
        }

        public override void Print(Output output)
        {
            if (m_targets.Count > 0)
            {
                if (m_declare)
                    output.Print("local ");

                var functionSugar = false;

                var value = m_values[0];
                var target = m_targets[0];

                if (m_targets.Count == 1 && m_values.Count == 1)
                {
                    if (value.IsClosure && target.IsFunctionName)
                    {
                        //This check only works in Lua version 0x51
                        if (!m_declare || m_declareStart >= value.ClosureUpvalueLine)
                            functionSugar = true;
                        if (target.IsLocal && value.IsUpvalueOf(target.GetIndex()))
                            functionSugar = true;
                    }
                }

                if (!functionSugar)
                {
                    target.Print(output);

                    for (int i = 1; i < m_targets.Count; i++)
                    {
                        output.Print(", ");
                        m_targets[i].Print(output);
                    }

                    if (!m_declare || !m_allNil)
                    {
                        output.Print(" = ");
                        Expression.PrintSequence(output, m_values, false, false);
                    }
                }
                else
                {
                    value.PrintClosure(output, target);
                }

                if (Comment != null)
                {
                    output.Print(" -- ");
                    output.Print(Comment);
                }
            }
        }

        public Assignment() { }

        public Assignment(Target target, Expression value)
        {
            m_targets.Add(target);
            m_values.Add(value);

            m_allNil = m_allNil && value.IsNil;
        }
    }
}
