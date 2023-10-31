using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AquaModelLibrary.Extra.Ninja.BillyHatcher.LND
{
    //ARCLND
    public struct ARCLNDHeader
    {
        public int mainDataOffset;
        public int extraFileCount;
        public int extraFileOffsetsOffset;
        public int unkFileOffset;    //Often 0

        public int texRefTableOffset;
        public int GVMOffset;
    }

    public struct ARCLNDRefTableHead
    {
        public int entryOffset;
        public int entryCount;
    }

    public struct ARCLNDRefEntry
    {
        public int textOffset;
        public int unkInt0;
        public int unkInt1;
    }
}
