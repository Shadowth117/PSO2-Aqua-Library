using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnluacNET
{
    public abstract class Block : Statement, IComparable<Block>
    {
        protected LFunction Function { get; private set; }

        public int Begin { get; set; }
        public int End { get; set; }

        public bool LoopRedirectAdjustment { get; set; }

        public virtual int ScopeEnd
        {
            get { return End - 1; }
        }

        public abstract bool Breakable { get; }
        public abstract bool IsContainer { get; }
        public abstract bool IsUnprotected { get; }
        
        public abstract void AddStatement(Statement statement);
        public abstract int GetLoopback();

        public virtual int CompareTo(Block block)
        {
            if (Begin < block.Begin)
            {
                return -1;
            }
            else if (Begin == block.Begin)
            {
                if (End < block.End)
                {
                    return 1;
                }
                else if (End == block.End)
                {
                    if (IsContainer && !block.IsContainer)
                    {
                        return -1;
                    }
                    else if (!IsContainer && block.IsContainer)
                    {
                        return 1;
                    }
                    else
                    {
                        return 0;
                    }
                }
                else
                {
                    return -1;
                }
            }
            else
            {
                return 1;
            }
        }

        public virtual bool Contains(Block block)
        {
            return (Begin <= block.Begin) && (End >= block.End);
        }

        public virtual bool Contains(int line)
        {
            return (Begin <= line) && (line < End);
        }

        public virtual Operation Process(Decompiler d)
        {
            var statement = this;

            return new LambdaOperation(End - 1, (r, block) => {
                return statement;
            });
        }

        public Block(LFunction function, int begin, int end)
        {
            Function = function;
            
            Begin = begin;
            End = end;
        }
    }
}
