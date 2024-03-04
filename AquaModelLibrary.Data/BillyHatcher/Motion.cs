using AquaModelLibrary.Data.Ninja;
using AquaModelLibrary.Helpers.Extensions;
using AquaModelLibrary.Helpers.Readers;

namespace AquaModelLibrary.Data.BillyHatcher
{
    /// <summary>
    /// Billy Hatcher specific variation of the Ninja Motions
    /// </summary>

    public class Motion
    {
        public MotionStart motStart;
        public MotionRef motRef;
        public MotionHeader motHeader;

        /// <summary>
        /// RGBA Order
        /// </summary>
        public List<List<byte[]>> colorAnimations = new List<List<byte[]>>();

        public struct MotionStart
        {
            public int int_00;
            public int offset;
        }

        public struct MotionRef
        {
            public int int_00;
            public int offset;
        }

        public struct MotionHeader
        {
            public ushort usht_00;
            public ushort usht_02;
            public int int_04;
            public ushort usht_08;
            public ushort usht_0A;
            public int int_0C;

            public AnimFlags animationType;
            public ushort count;
            public int dataOffset;
            public AnimFlags animationTypeAgain; //Maybe?
            public ushort usht_1A;
            public int int_1C;

            public ushort usht_20;
            public ushort usht_22;
            public int int_24;
            public ushort usht_28;
            public ushort usht_2A;
            public int int_2C;
        }

        public Motion() { }

        public Motion(BufferedStreamReaderBE<MemoryStream> sr, int offset = 0)
        {
            motStart = new MotionStart();
            motStart.int_00 = sr.ReadBE<int>();
            motStart.offset = sr.ReadBE<int>();
            sr.Seek(motStart.offset + offset, SeekOrigin.Begin);
            motRef = new MotionRef();
            motRef.int_00 = sr.ReadBE<int>();
            motRef.offset = sr.ReadBE<int>();
            sr.Seek(motRef.offset + offset, SeekOrigin.Begin);
            motHeader = new MotionHeader();
            motHeader.usht_00 = sr.ReadBE<ushort>();
            motHeader.usht_02 = sr.ReadBE<ushort>();
            motHeader.int_04 = sr.ReadBE<int>();
            motHeader.usht_08 = sr.ReadBE<ushort>();
            motHeader.usht_0A = sr.ReadBE<ushort>();
            motHeader.int_0C = sr.ReadBE<int>();

            //For .lnd, you will commonly see motions with 1 frame of color for every vertex
            motHeader.animationType = sr.Read<AnimFlags>();
            motHeader.count = sr.ReadBE<ushort>();
            motHeader.dataOffset = sr.ReadBE<int>();
            motHeader.animationTypeAgain = sr.Read<AnimFlags>();
            motHeader.usht_1A = sr.ReadBE<ushort>();
            motHeader.int_1C = sr.ReadBE<int>();

            motHeader.usht_20 = sr.ReadBE<ushort>();
            motHeader.usht_22 = sr.ReadBE<ushort>();
            motHeader.int_24 = sr.ReadBE<int>();
            motHeader.usht_28 = sr.ReadBE<ushort>();
            motHeader.usht_2A = sr.ReadBE<ushort>();
            motHeader.int_2C = sr.ReadBE<int>();

            //If this is used outside of LND this needs fixing, but otherwise, this works for now.
            sr.Seek(motHeader.dataOffset + offset, SeekOrigin.Begin);
            List<byte[]> colorAnimation = new List<byte[]>();
            for (int i = 0; i < motHeader.count; i++)
            {
                colorAnimation.Add(new byte[] { sr.Read<byte>(), sr.Read<byte>(), sr.Read<byte>(), sr.Read<byte>() });
            }
            colorAnimations.Add(colorAnimation);
        }

        public byte[] GetBytes(int offset, out List<int> offsets)
        {
            offsets = new List<int>();
            List<byte> outBytes = new List<byte>();

            outBytes.AddValue(1);
            offsets.Add(outBytes.Count + offset);
            outBytes.AddValue(offset + 0x8);

            outBytes.AddValue(0);
            offsets.Add(outBytes.Count + offset);
            outBytes.AddValue(offset + 0x10);

            outBytes.AddValue(motHeader.usht_00);
            outBytes.AddValue(motHeader.usht_02);
            outBytes.AddValue(motHeader.int_04);
            outBytes.AddValue(motHeader.usht_08);
            outBytes.AddValue(motHeader.usht_0A);
            outBytes.AddValue(motHeader.int_0C);

            var animType = BitConverter.GetBytes((short)motHeader.animationType);
            outBytes.Add(animType[0]);
            outBytes.Add(animType[1]);
            outBytes.AddValue(motHeader.count);
            offsets.Add(outBytes.Count + offset);
            outBytes.AddValue(outBytes.Count + offset + 0x1C);
            var animType2 = BitConverter.GetBytes((short)motHeader.animationTypeAgain);
            outBytes.Add(animType2[0]);
            outBytes.Add(animType2[1]);
            outBytes.AddValue(motHeader.usht_1A);
            outBytes.AddValue(motHeader.int_1C);

            outBytes.AddValue(motHeader.usht_20);
            outBytes.AddValue(motHeader.usht_22);
            outBytes.AddValue(motHeader.int_24);
            outBytes.AddValue(motHeader.usht_28);
            outBytes.AddValue(motHeader.usht_2A);
            outBytes.AddValue(motHeader.int_2C);

            for (int i = 0; i < colorAnimations[0].Count; i++)
            {
                outBytes.AddRange(colorAnimations[0][i]);
            }

            return outBytes.ToArray();
        }
    }
}
