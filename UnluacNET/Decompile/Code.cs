using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnluacNET
{
    public enum OpMode
    {
        iABC,
        iABx,
        iAsBx
    }

    public enum OpArgMask
    {
        /// <summary>
        /// Argument is not used.
        /// </summary>
        OpArgN,

        /// <summary>
        /// Argument is used.
        /// </summary>
        OpArgU,

        /// <summary>
        /// Argument is a register or a jump offset.
        /// </summary>
        OpArgR,

        /// <summary>
        /// Argument is a constant or register/constant.
        /// </summary>
        OpArgK
    }

    public sealed class LInstruction
    {
        /*
        ** Size and position of opcode arguments
        */
        private const int SIZE_C      = 9;
        private const int SIZE_B      = 9;
        private const int SIZE_Bx     = (SIZE_C + SIZE_B);
        private const int SIZE_A      = 8;

        private const int SIZE_OP     = 6;

        private const int POS_OP      = 0;
        private const int POS_A       = (POS_OP + SIZE_OP);
        private const int POS_C       = (POS_A + SIZE_A);
        private const int POS_B       = (POS_C + SIZE_C);
        private const int POS_Bx      = POS_C;

        /*
        ** Limits for opcode arguments
        ** (signed) int used to manipulate most arguments
        */
        private const int MAXARG_Bx   = (1 << SIZE_Bx) - 1;
        private const int MAXARG_sBx  = (MAXARG_Bx >> 1);

        private const int MAXARG_A    = (1 << SIZE_A) - 1;
        private const int MAXARG_B    = (1 << SIZE_B) - 1;
        private const int MAXARG_C    = (1 << SIZE_C) - 1;

        private static int MASK1(int n, int p)
        {
            return (~(~0 << n)) << p;
        }

        private static int MASK0(int n, int p)
        {
            return ~MASK1(n, p);
        }

        /*
         ** Macros to operate RK indices
         */

        /* this bit 1 means constant (0 means register) */
        private const int BITRK       = (1 << (SIZE_B - 1));
        private const int MAXINDEXRK  = (BITRK - 1);

        /* test whether value is a constant */
        private static bool ISK(int x)
        {
            return ((x & BITRK) == 1);
        }

        /* gets the index of the constant */
        private static int INDEXK(int r)
        {
            return (r & ~BITRK);
        }

        private static readonly int MASK_GETOPCODE  = MASK1(SIZE_OP, 0);
        private static readonly int MASK_SETOPCODE  = MASK1(SIZE_OP, POS_OP);

        private static readonly int MASK_GETA       = MASK1(SIZE_A, 0);
        private static readonly int MASK_SETA       = MASK1(SIZE_A, POS_A);

        private static readonly int MASK_GETB       = MASK1(SIZE_B, 0);
        private static readonly int MASK_SETB       = MASK1(SIZE_B, POS_B);

        private static readonly int MASK_GETC       = MASK1(SIZE_C, 0);
        private static readonly int MASK_SETC       = MASK1(SIZE_C, POS_C);

        private static readonly int MASK_GETBx      = MASK1(SIZE_Bx, 0);
        private static readonly int MASK_SETBx      = MASK1(SIZE_Bx, POS_Bx);

        private int m_value;

        public static implicit operator LInstruction(int value)
        {
            return new LInstruction(value);
        }

        public static implicit operator int(LInstruction value)
        {
            return value.m_value;
        }

        public static LInstruction CreateABC(Op op, int a, int b, int c)
        {
            return new LInstruction(op, a, b, c);
        }

        public static LInstruction CreateABx(Op op, int a, int bx)
        {
            return new LInstruction(op, a, bx);
        }

        public static int GetOpCode(int codePoint)
        {
            return (codePoint >> POS_OP) & MASK_GETOPCODE;
        }

        public static int GetArgA(int codePoint)
        {
            return (codePoint >> POS_A) & MASK_GETA;
        }

        public static int GetArgB(int codePoint)
        {
            return (codePoint >> POS_B) & MASK_GETB;
        }

        public static int GetArgC(int codePoint)
        {
            return (codePoint >> POS_C) & MASK_GETC;
        }

        public static int GetArgBx(int codePoint)
        {
            return (codePoint >> POS_Bx) & MASK_GETBx;
        }

        public static int GetArgsBX(int codePoint)
        {
            return GetArgBx(codePoint) - MAXARG_sBx;
        }

        public Op Op
        {
            get { return (Op)GetOpCode(m_value); }
            set
            {
                m_value = ((m_value & ~MASK_SETOPCODE) |
                    (((int)value << POS_OP) & MASK_SETOPCODE));
            }
        }

        public int A
        {
            get { return GetArgA(m_value); }
            set
            {
                m_value = ((m_value & ~MASK_SETA) |
                    ((value << POS_A) & MASK_SETA));
            }
        }

        public int B
        {
            get { return GetArgB(m_value); }
            set
            {
                m_value = ((m_value & ~MASK_SETB) |
                    ((value << POS_B) & MASK_SETB));
            }
        }

        public int C
        {
            get { return GetArgC(m_value); }
            set
            {
                m_value = ((m_value & ~MASK_SETC) |
                    ((value << POS_C) & MASK_SETC));
            }
        }

        public int Bx
        {
            get { return GetArgBx(m_value); }
            set
            {
                m_value = ((m_value & ~MASK_SETBx) |
                    ((value << POS_Bx) & MASK_SETBx));
            }
        }

        public int sBx
        {
            get { return GetArgsBX(m_value); }
            set
            {
                Bx = value + MAXARG_sBx;
            }
        }

        public LInstruction(int value)
        {
            m_value = value;
        }

        public LInstruction(Op op, int a, int bx)
        {
            m_value = (((int)op << POS_OP) | (a << POS_A) | (bx << POS_Bx));
        }

        public LInstruction(Op op, int a, int b, int c)
        {
            m_value = (((int)op << POS_OP) | (a << POS_A) | (b << POS_B) | (c << POS_C));
        }
    }

    public class Code
    {
        private readonly OpcodeMap map;
        private readonly int[] code;

        /*
        ** Size and position of opcode arguments
        */
        private static readonly int SIZE_C      = 9;
        private static readonly int SIZE_B      = 9;
        private static readonly int SIZE_Bx     = (SIZE_C + SIZE_B);
        private static readonly int SIZE_A      = 8;

        private static readonly int SIZE_OP     = 6;

        private static readonly int POS_OP      = 0;
        private static readonly int POS_A       = (POS_OP + SIZE_OP);
        private static readonly int POS_C       = (POS_A + SIZE_A);
        private static readonly int POS_B       = (POS_C + SIZE_C);
        private static readonly int POS_Bx      = POS_C;

        /*
        ** Limits for opcode arguments
        ** (signed) int used to manipulate most arguments
        */
        private static readonly int MAXARG_Bx   = (1 << SIZE_Bx) - 1;
        private static readonly int MAXARG_sBx  = (MAXARG_Bx >> 1);

        private static readonly int MAXARG_A    = (1 << SIZE_A) - 1;
        private static readonly int MAXARG_B    = (1 << SIZE_B) - 1;
        private static readonly int MAXARG_C    = (1 << SIZE_C) - 1;
        
        private static int MASK1(int n, int p)
        {
            return (~(~0 << n)) << p;
        }

        private static int MASK0(int n, int p)
        {
            return ~MASK1(n, p);
        }

        /*
         ** Macros to operate RK indices
         */

        /* this bit 1 means constant (0 means register) */
        private static readonly int BITRK       = (1 << (SIZE_B - 1));
        private static readonly int MAXINDEXRK  = (BITRK - 1);

        /* test whether value is a constant */
        private static bool ISK(int x)
        {
            return ((x & BITRK) == 1);
        }

        /* gets the index of the constant */
        private static int INDEXK(int r)
        {
            return (r & ~BITRK);
        }

        private static readonly int MASK_OPCODE = MASK1(SIZE_OP, 0);
        private static readonly int MASK_A      = MASK1(SIZE_A, 0);
        private static readonly int MASK_B      = MASK1(SIZE_B, 0);
        private static readonly int MASK_C      = MASK1(SIZE_C, 0);
        private static readonly int MASK_Bx     = MASK1(SIZE_Bx, 0);

        //----------------------------------------------------\\

        public static int GetOpCode(int codePoint)
        {
            return (codePoint >> POS_OP) & MASK_OPCODE;
        }

        public static int GetArgA(int codePoint)
        {
            return (codePoint >> POS_A) & MASK_A;
        }

        public static int GetArgB(int codePoint)
        {
            return (codePoint >> POS_B) & MASK_B;
        }

        public static int GetArgC(int codePoint)
        {
            return (codePoint >> POS_C) & MASK_C;
        }

        public static int GetArgBx(int codePoint)
        {
            return (codePoint >> POS_Bx) & MASK_Bx;
        }

        public static int GetArgsBX(int codePoint)
        {
            return GetArgBx(codePoint) - MAXARG_sBx;
        }

        public Op Op(int line)
        {
            return map.GetOp(GetOpCode(CodePoint(line)));
        }

        public int A(int line)
        {
            return GetArgA(CodePoint(line));
        }

        public int C(int line)
        {
            return GetArgC(CodePoint(line));
        }

        public int B(int line)
        {
            return GetArgB(CodePoint(line));
        }

        public int Bx(int line)
        {
            return GetArgBx(CodePoint(line));
        }

        public int sBx(int line)
        {
            return GetArgsBX(CodePoint(line));
        }

        public OpMode OpMode(int line)
        {
            return map.GetOpMode((int)Op(line));
        }

        public OpArgMask BMode(int line)
        {
            return map.GetBMode((int)Op(line));
        }

        public OpArgMask CMode(int line)
        {
            return map.GetCMode((int)Op(line));
        }

        public bool TestA(int line)
        {
            return map.TestAMode((int)Op(line));
        }

        public bool TestT(int line)
        {
            return map.TestTMode((int)Op(line));
        }

        public int CodePoint(int line)
        {
            return code[line - 1];
        }

        public Code(LFunction function)
        {
            code = function.Code;
            map  = function.Header.Version.GetOpcodeMap();
        }   
    }
}
