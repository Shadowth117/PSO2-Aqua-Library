using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnluacNET
{
    public abstract class Version
    {
        public static readonly Version LUA51 = new Version51();
        public static readonly Version LUA52 = new Version52();

        protected int m_versionNumber;

        public abstract bool HasHeaderTail { get; }

        public abstract int OuterBlockScopeAdjustment { get; }
        
        public abstract Op TForTarget { get; }
        
        public abstract bool UsesInlineUpvalueDeclaritions { get; }
        public abstract bool UsesOldLoadNilEncoding { get; }

        public abstract LFunctionType GetLFunctionType();
        public abstract bool IsBreakableLoopEnd(Op op);

        public OpcodeMap GetOpcodeMap()
        {
            return new OpcodeMap(m_versionNumber);
        }

        protected Version(int versionNumber)
        {
            m_versionNumber = versionNumber;
        }
    }

    public sealed class Version51 : Version
    {
        public override bool HasHeaderTail
        {
            get { return false; }
        }

        public override int OuterBlockScopeAdjustment
        {
            get { return -1; }
        }

        public override Op TForTarget
        {
            get { return Op.TFORLOOP; }
        }

        public override bool UsesInlineUpvalueDeclaritions
        {
            get { return true; }
        }

        public override bool UsesOldLoadNilEncoding
        {
            get { return true; }
        }

        public override LFunctionType GetLFunctionType()
        {
            return LFunctionType.TYPE51;
        }

        public override bool IsBreakableLoopEnd(Op op)
        {
            return (op == Op.JMP || op == Op.FORLOOP);
        }

        public Version51() : base(0x51) { }
    }

    public sealed class Version52 : Version
    {
        public override bool HasHeaderTail
        {
            get { return true; }
        }

        public override int OuterBlockScopeAdjustment
        {
            get { return 0; }
        }

        public override Op TForTarget
        {
            get { return Op.TFORCALL; }
        }

        public override bool UsesInlineUpvalueDeclaritions
        {
            get { return false; }
        }

        public override bool UsesOldLoadNilEncoding
        {
            get { return false; }
        }

        public override LFunctionType GetLFunctionType()
        {
            return LFunctionType.TYPE52;
        }

        public override bool IsBreakableLoopEnd(Op op)
        {
            return (op == Op.JMP || op == Op.FORLOOP || op == Op.TFORLOOP);
        }

        public Version52() : base(0x52) { }
    }
}
