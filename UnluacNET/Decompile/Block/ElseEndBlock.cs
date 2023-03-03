using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnluacNET
{
    public class ElseEndBlock : Block
    {
        private readonly List<Statement> m_statements;

        public IfThenElseBlock Partner { get; set; }

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

        public override int CompareTo(Block block)
        {
            if (block == Partner)
                return 1;

            var result = base.CompareTo(block);

            //if (result == 0 && block is ElseEndBlock)
            //    Console.WriteLine("HEY HEY HEY");

            return result;
        }

        public override int GetLoopback()
        {
            throw new InvalidOperationException();
        }

        public override void Print(Output output)
        {
            output.Print("else");

            if (m_statements.Count == 1 && m_statements[0] is IfThenEndBlock)
            {    
                m_statements[0].Print(output);
            }
            else if (m_statements.Count == 2 && m_statements[0] is IfThenElseBlock && m_statements[1] is ElseEndBlock)
            {
                m_statements[0].Print(output);
                m_statements[1].Print(output);
            }
            else
            {
                output.PrintLine();

                output.IncreaseIndent();

                Statement.PrintSequence(output, m_statements);

                output.DecreaseIndent();
                
                output.Print("end");
            }
        }

        public ElseEndBlock(LFunction function, int begin, int end)
            : base(function, begin, end)
        {
            m_statements = new List<Statement>(end - begin + 1);
        }
    }
}
