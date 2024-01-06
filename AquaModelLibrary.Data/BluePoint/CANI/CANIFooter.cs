using AquaModelLibrary.Helpers.Readers;
using System.Numerics;

namespace AquaModelLibrary.Data.BluePoint.CANI
{
    public class CANIFooter
    {
        public CANIFooterHead footerHead;
        public int[] footerHeadPadding = null;
        public CANIFooterBody footerBody;

        public struct CANIFooterHead
        {
            public ushort usht_00;
            public ushort footerHeadSize;
            public int int_04;
            public int footerHeadSizeWithPadding;
        }

        public struct CANIFooterBody
        {
            public byte flag0;
            public byte flag1;
            public byte flag2;
            public byte flag3;

            public int bodySectionSize; //Always 0xC, which since this is already aligned from the footer head, checks out if this is size related.
            public int padding_08;
            public int padding_0C;

            public Quaternion quat; //May be something else entirely, but always observed as 0, 0, 0, 1. This is the default state of a quaternion angle.

            public int finalInt0; //These were always observed 0. No obvious reason they're there
            public int finalInt1;
            public int finalInt2;
            public int finalInt3;
        }

        public CANIFooter()
        {
        }

        public CANIFooter(BufferedStreamReaderBE<MemoryStream> sr)
        {
            footerHead = sr.Read<CANIFooterHead>();
            footerHeadPadding = new int[(footerHead.footerHeadSizeWithPadding - 0xC) / 4];
            for (int i = 0; i < footerHeadPadding.Length; i++)
            {
                footerHeadPadding[i] = sr.Read<int>();
            }
            footerBody = sr.Read<CANIFooterBody>();
        }
    }
}
