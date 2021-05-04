using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AquaModelLibrary.AquaCommon;

namespace AquaModelLibrary
{
    public class FLTDPhysics
    {
        public NIFL nifl;
        public REL0 rel0;

        public NOF0 nof0;
        public NEND nend;

        public struct fltdHeader
        {
            public byte version;         //Apparently there are a number of these and they work differently. This makes things painful
            public byte unkStruct0Count;
            public byte unkStruct1Count;
            public byte unkByte1;

            public int unkStruct0Offset;
            public int unkStruct1Offset;
        }

        //Sub of fltdHeader
        public struct unkStruct0
        {
            public byte index;
            public byte unkByte1; //Usually 0x1
            public byte unkByte2;
            public byte unkByte3;
            public int namePointerPointer; 
            public int unkPointer1;
        }

        //Sub of fltdHeader
        public struct unkStruct1
        {
            public byte unkByte0;
            public byte unkByte1;
            public byte unkByte2;
            public byte unkByte3;

            public byte unkByte4;
            public byte unkByte5;
            public byte unkByte6;
            public byte unkByte7;

            public int unkPointer0;
            public int unkStruct2Pointer;
            public int unkStruct3Pointer;
        }


        //Sub of unkStruct1
        public struct unkStruct2
        {
            public float unkFloat0;
            public float unkFloat1;
            public int unkInt0;
            public byte unkByte0;
            public byte unkStruct4Count;
            public byte unkStruct5Count;
            public byte unkByte3;

            public int unkStruct4Pointer;
            public int unkStruct5Pointer;
            public int unkStruct6Pointer;
            public int unkStruct7Pointer;

            public int unkStruct8Pointer;
        }

        //Sub of unkStruct1
        public struct unkStruct3
        {
            public int unkInt0;
            public float unkFloat0;
            public int unkPointer0;
        }

        //Sub of unkStruct2
        public struct unkStruct4
        {
            public byte index;
            public byte unkByte1;
            public byte unkByte2;
            public byte unkByte3;

            public float unkFloat0;
            public float unkFloat1;
            public int unkInt0;
            public int unkInt1;
        }

        //Sub of unkStruct2
        public struct unkStruct5
        {
            public byte unkByte0;
            public byte unkByte1;
            public byte index;
            public byte unkByte3;

            public int unkInt0;
            public int unkInt1;
            public int unkInt2;
            public float unkFloat0;
        }

    }
}
