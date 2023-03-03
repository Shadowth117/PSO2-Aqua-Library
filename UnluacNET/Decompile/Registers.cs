using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnluacNET
{
    public class Registers
    {
        public int NumRegisters { get; private set; }
        public int Length { get; private set; }

        private readonly Declaration[,] m_decls;
        private readonly Function m_func;
        private readonly int[,] m_updated;
        private readonly Expression[,] m_values;

        private bool[] m_startedLines;

        public bool IsAssignable(int register, int line)
        {
            return IsLocal(register, line) &&
                !GetDeclaration(register, line).ForLoop;
        }

        public bool IsLocal(int register, int line)
        {
            if (register < 0)
                return false;

            return GetDeclaration(register, line) != null;
        }

        public bool IsNewLocal(int register, int line)
        {
            var decl = GetDeclaration(register, line);

            return decl != null && decl.Begin == line && !decl.ForLoop;
        }

        public Declaration GetDeclaration(int register, int line)
        {
            return m_decls[register, line];
        }

        public Expression GetExpression(int register, int line)
        {
            if (IsLocal(register, line - 1))
                return new LocalVariable(GetDeclaration(register, line - 1));
            else
                return GetValue(register, line);
        }

        public Expression GetKExpression(int register, int line)
        {
            if ((register & 0x100) != 0)
                return m_func.GetConstantExpression(register & 0xFF);
            else
                return GetExpression(register, line);
        }

        public List<Declaration> GetNewLocals(int line)
        {
            var locals = new List<Declaration>(NumRegisters);

            for (int register = 0; register < NumRegisters; register++)
            {
                if (IsNewLocal(register, line))
                    locals.Add(GetDeclaration(register, line));
            }

            return locals;
        }

        public Target GetTarget(int register, int line)
        {
            if (!IsLocal(register, line))
                throw new InvalidOperationException("No declaration exists in register" + register + " at line " + line);

            return new VariableTarget(GetDeclaration(register, line));
        }

        public int GetUpdated(int register, int line)
        {
            return m_updated[register, line];
        }

        public Expression GetValue(int register, int line)
        {
            return m_values[register, line - 1];
        }

        private void NewDeclaration(Declaration decl, int register, int begin, int end)
        {
            for (int line = begin; line <= end; line++)
                m_decls[register, line] = decl;
        }

        public void SetValue(int register, int line, Expression expression)
        {
            m_values[register, line] = expression;
            m_updated[register, line] = line;
        }

        public void SetInternalLoopVariable(int register, int begin, int end)
        {
            var decl = GetDeclaration(register, begin);

            if (decl == null)
            {
                decl = new Declaration("_FOR_", begin, end);
                decl.Register = register;

                NewDeclaration(decl, register, begin, end);
            }

            decl.ForLoop = true;
        }

        public void SetExplicitLoopVariable(int register, int begin, int end)
        {
            var decl = GetDeclaration(register, begin);

            if (decl == null)
            {
                decl = new Declaration("_FORV_" +register + "_", begin, end);
                decl.Register = register;

                NewDeclaration(decl, register, begin, end);
            }

            decl.ForLoopExplicit = true;
        }

        public void StartLine(int line)
        {
            m_startedLines[line] = true;

            for (int register = 0; register < NumRegisters; register++)
            {
                m_values[register, line] = m_values[register, line - 1];
                m_updated[register, line] = m_updated[register, line - 1];
            }
        }

        public Registers(int registers, int length, Declaration[] declList, Function func)
        {
            NumRegisters = registers;
            Length = length;

            m_decls = new Declaration[registers, length + 1];

            for (int i = 0; i < declList.Length; i++)
            {
                var decl = declList[i];

                int register = 0;

                while (m_decls[register, decl.Begin] != null)
                    register++;

                decl.Register = register;

                for (int line = decl.Begin; line <= decl.End; line++)
                    m_decls[register, line] = decl;
            }

            m_values = new Expression[registers, length + 1];

            for (int register = 0; register < registers; register++)
                m_values[register, 0] = Expression.NIL;

            m_updated = new int[registers, length + 1];
            m_startedLines = new bool[length + 1];

            m_func = func;
        }
    }
}
