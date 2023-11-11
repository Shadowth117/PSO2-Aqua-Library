using Reloaded.Memory.Streams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AquaModelLibrary.Extra.Ninja.BillyHatcher
{
    /// <summary>
    /// Billy Hatcher specific variation of the Ninja Motions
    /// </summary>

    [Flags]
	public enum AnimFlags : ushort
    {
        Position = 0x1,
        Rotation = 0x2,
        Scale = 0x4,
        Vector = 0x8,
        Vertex = 0x10,
        Normal = 0x20,
        Target = 0x40,
        Roll = 0x80,
        Angle = 0x100,
        Color = 0x200,
        Intensity = 0x400,
        Spot = 0x800,
        Point = 0x1000,
        Quaternion = 0x2000
    }

    public class Motion
    {
        public MotionStart motStart;
        public MotionRef motRef;
        public MotionHeader motHeader;

        /// <summary>
        /// RGBA Order
        /// </summary>
        public List<List<byte[]>> colorAnimation = new List<List<byte[]>>();

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

        public Motion(BufferedStreamReader sr, int offset = 0)
        {
            motStart = new MotionStart();
            motStart.int_00 = sr.ReadBE<int>();
            motStart.offset = sr.ReadBE<int>();
            sr.Seek(motStart.offset + offset, System.IO.SeekOrigin.Begin);
            motRef = new MotionRef();
            motRef.int_00 = sr.ReadBE<int>();
            motRef.offset = sr.ReadBE<int>();
            sr.Seek(motRef.offset + offset, System.IO.SeekOrigin.Begin);
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


        }

        public byte[] GetBytes(int offset, out List<int> offsets)
        {
            offsets = new List<int>();
            List<byte> outBytes = new List<byte>();

            outBytes.AddValue(0);
            offsets.Add(outBytes.Count + offset);
            outBytes.AddValue(offset + 0x8);
            outBytes.AddValue(0);
            offsets.Add(outBytes.Count + offset);
            outBytes.AddValue(offset + 0x10);
            //outBytes.Add

            return outBytes.ToArray();
        }
    }
}
