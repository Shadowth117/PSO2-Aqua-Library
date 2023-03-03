using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnluacNET
{
    public sealed class Opcode
    {
        // TODO: Optimize method
        public static string CodePointToString(Op opcode, LInstruction code)
        {
            var name = opcode.GetType().Name;

            switch (opcode)
            {
            // A
            case Op.CLOSE:
            case Op.LOADKX:
                    return String.Format("{0} {1}",
                        name, code.A);

            // A_B
            case Op.MOVE:
            case Op.LOADNIL:
            case Op.GETUPVAL:
            case Op.SETUPVAL:
            case Op.UNM:
            case Op.NOT:
            case Op.LEN:
            case Op.RETURN:
            case Op.VARARG:
                    return String.Format("{0} {1} {2}",
                        name, code.A, code.B);

            // A_C
            case Op.TEST:
            case Op.TFORLOOP:
            case Op.TFORCALL:
                    return String.Format("{0} {1} {2}",
                        name, code.A, code.C);
            
            // A_B_C
            case Op.LOADBOOL:
            case Op.GETTABLE:
            case Op.SETTABLE:
            case Op.NEWTABLE:
            case Op.SELF:
            case Op.ADD:
            case Op.SUB:
            case Op.MUL:
            case Op.DIV:
            case Op.MOD:
            case Op.POW:
            case Op.CONCAT:
            case Op.EQ:
            case Op.LT:
            case Op.LE:
            case Op.TESTSET:
            case Op.CALL:
            case Op.TAILCALL:
            case Op.SETLIST:
            case Op.GETTABUP:
            case Op.SETTABUP:
                    return String.Format("{0} {1} {2} {3}",
                        name,
                        code.A,
                        code.B,
                        code.C);

            // A_Bx
            case Op.LOADK:
            case Op.GETGLOBAL:
            case Op.SETGLOBAL:
            case Op.CLOSURE:
                    return String.Format("{0} {1} {2}",
                        name, code.A, code.Bx);

            // A_sBx
            case Op.FORLOOP:
            case Op.FORPREP:
                    return String.Format("{0} {1} {2}",
                        name, code.A, code.sBx);

            // Ax
            case Op.EXTRAARG:
                return String.Format("{0} <Ax>", name);

            // sBx
            case Op.JMP:
                return String.Format("{0} {1}",
                    name, code.sBx);

            default:
                return name;
            }
        }
    }

    public enum Op
    {
        /*------------------------------------------------------------------------
        name           args    description
        --------------------------------------------------------------------------*/
        MOVE,     /*   A B     R(A) := R(B)                                       */
        LOADK,    /*   A Bx    R(A) := Kst(Bx)                                    */
        LOADBOOL, /*   A B C   R(A) := (Bool)B; if (C) pc++                       */
        LOADNIL,  /*   A B     R(A) := ... := R(B) := nil                         */
        GETUPVAL, /*   A B     R(A) := UpValue[B]                                 */

        GETGLOBAL,/*   A Bx    R(A) := Gbl[Kst(Bx)]                               */
        GETTABLE, /*   A B C   R(A) := R(B)[RK(C)]                                */

        SETGLOBAL,/*   A Bx    Gbl[Kst(Bx)] := R(A)                               */
        SETUPVAL, /*   A B     UpValue[B] := R(A)                                 */
        SETTABLE, /*   A B C   R(A)[RK(B)] := RK(C)                               */

        NEWTABLE, /*   A B C   R(A) := {} (size = B,C)                            */

        SELF,     /*   A B C   R(A+1) := R(B); R(A) := R(B)[RK(C)]                */

        ADD,      /*   A B C   R(A) := RK(B) + RK(C)                              */
        SUB,      /*   A B C   R(A) := RK(B) - RK(C)                              */
        MUL,      /*   A B C   R(A) := RK(B) * RK(C)                              */
        DIV,      /*   A B C   R(A) := RK(B) / RK(C)                              */
        MOD,      /*   A B C   R(A) := RK(B) % RK(C)                              */
        POW,      /*   A B C   R(A) := RK(B) ^ RK(C)                              */
        UNM,      /*   A B     R(A) := -R(B)                                      */
        NOT,      /*   A B     R(A) := not R(B)                                   */
        LEN,      /*   A B     R(A) := length of R(B)                             */

        CONCAT,   /*   A B C   R(A) := R(B).. ... ..R(C)                          */

        JMP,      /*   sBx     pc+=sBx (different in 5.2)                         */

        EQ,       /*   A B C   if ((RK(B) == RK(C)) ~= A) then pc++               */
        LT,       /*   A B C   if ((RK(B) <  RK(C)) ~= A) then pc++               */
        LE,       /*   A B C   if ((RK(B) <= RK(C)) ~= A) then pc++               */

        TEST,     /*   A C     if not (R(A) <=> C) then pc++                      */
        TESTSET,  /*   A B C   if (R(B) <=> C) then R(A) := R(B) else pc++        */

        CALL,     /*   A B C   R(A), ... ,R(A+C-2) := R(A)(R(A+1), ... ,R(A+B-1)) */
        TAILCALL, /*   A B C   return R(A)(R(A+1), ... ,R(A+B-1))                 */
        RETURN,   /*   A B     return R(A), ... ,R(A+B-2)      (see note)         */

        FORLOOP,  /*   A sBx   R(A)+=R(A+2);
                       if R(A) <?= R(A+1) then { pc+=sBx; R(A+3)=R(A) }           */
        FORPREP,  /*   A sBx   R(A)-=R(A+2); pc+=sBx                              */

        TFORLOOP, /*   A C     R(A+3), ... ,R(A+3+C) := R(A)(R(A+1), R(A+2)); 
                          if R(A+3) ~= nil then { pc++; R(A+2)=R(A+3); }          */
        SETLIST,  /*   A B C   R(A)[(C-1)*FPF+i] := R(A+i), 1 <= i <= B           */

        CLOSE,    /*   A       close all variables in the stack up to (>=) R(A)   */
        CLOSURE,  /*   A Bx    R(A) := closure(KPROTO[Bx], R(A), ... ,R(A+n))     */

        VARARG,   /*   A B     R(A), R(A+1), ..., R(A+B-1) = vararg               */

        // Lua 5.2 Opcodes
        LOADKX,
        GETTABUP,
        SETTABUP,
        TFORCALL,
        EXTRAARG
    }

    /*===========================================================================
      Notes:
      (*) In OP_CALL, if (B == 0) then B = top. C is the number of returns - 1,
          and can be 0: OP_CALL then sets `top' to last_result+1, so
          next open instruction (OP_CALL, OP_RETURN, OP_SETLIST) may use `top'.

      (*) In OP_VARARG, if (B == 0) then use actual number of varargs and
          set top (like in OP_CALL with C == 0).

      (*) In OP_RETURN, if (B == 0) then return up to `top'

      (*) In OP_SETLIST, if (B == 0) then B = `top';
          if (C == 0) then next `instruction' is real C

      (*) For comparisons, A specifies what condition the test should accept
          (true or false).

      (*) All `skips' (pc++) assume that next instruction is a jump
    ===========================================================================*/
}
