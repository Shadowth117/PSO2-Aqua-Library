using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnluacNET
{
    public class LFunction : BObject
    {
        public BHeader Header { get; set; }
        public int[] Code { get; set; }
        public LLocal[] Locals { get; set; }
        public LObject[] Constants { get; set; }
        public LUpvalue[] UpValues { get; set; }
        public LFunction[] Functions { get; set; }
        
        public int MaxStackSize { get; set; }
        public int NumUpValues { get; set; }
        public int NumParams { get; set; }
        public int VarArg { get; set; }

        public LFunction(BHeader header, int[] code, LLocal[] locals,
            LObject[] constants, LUpvalue[] upvalues, LFunction[] functions,
            int maximumStackSize, int numUpValues, int numParams, int vararg)
        {
            Header          = header;
            Code            = code;
            Locals          = locals;
            Constants       = constants;
            UpValues        = upvalues;
            Functions       = functions;
            MaxStackSize    = maximumStackSize;
            NumUpValues     = numUpValues;
            NumParams       = numParams;
            VarArg          = vararg;
        }
    }
}
