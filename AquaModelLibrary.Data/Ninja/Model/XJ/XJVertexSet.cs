using AquaModelLibrary.Helpers.Extensions;
using AquaModelLibrary.Helpers.Readers;
using System.Numerics;

namespace AquaModelLibrary.Data.Ninja.Model.XJ
{
    public class XJVertexSet
    {
        public List<Vector3> Positions = new List<Vector3>();
        public List<Vector3> Normals = new List<Vector3>();
        public List<byte[]> Colors = new List<byte[]>();
        public List<Vector2> UVs = new List<Vector2>();

        public ushort VertexType;
        public ushort Ushort_02;
        public uint VertexSize;

        public XJVertexSet() { }

        public XJVertexSet(BufferedStreamReaderBE<MemoryStream> sr, bool be = true, int offset = 0)
        {
            VertexType = sr.ReadBE<ushort>();
            Ushort_02 = sr.ReadBE<ushort>();
            var vertListOffset = sr.ReadBE<int>();
            VertexSize = sr.ReadBE<uint>();
            var vertCount = sr.ReadBE<int>();

            bool hasUV = (VertexType & 0x1) > 0;
            bool hasNormal = (VertexType & 0x2) > 0;
            bool hasColor = (VertexType & 0x4) > 0;
            bool hasUnk0 = (VertexType & 0x8) > 0;
            bool hasUnk1 = (VertexType & 0x10) > 0;
            bool hasUnk2 = (VertexType & 0x20) > 0;
            bool hasUnk3 = (VertexType & 0x40) > 0;
            bool hasUnk4 = (VertexType & 0x80) > 0;

            ushort calcedVertSize = 0xC;
            if (hasNormal)
                calcedVertSize += 0xC;
            if (hasColor)
                calcedVertSize += 0x4;
            if (hasUV)
                calcedVertSize += 0x8;

            if (VertexSize != calcedVertSize)
            {
                throw new Exception($"Vertsize {VertexSize} is not equal to Calculated Vertsize {calcedVertSize}");
            }

            sr.Seek(vertListOffset + offset, SeekOrigin.Begin);
            for (int i = 0; i < vertCount; i++)
            {
                Positions.Add(sr.ReadBEV3());

                if (hasNormal)
                {
                    Normals.Add(sr.ReadBEV3());
                }
                if (hasColor)
                {
                    Colors.Add(sr.Read4Bytes());
                }
                if (hasUV)
                {
                    UVs.Add(sr.ReadBEV2());
                }
            }
        }

        public void Write(List<byte> outBytes, List<int> POF0Offsets, int offset = 0)
        {
            bool hasUV = (VertexType & 0x1) > 0;
            bool hasNormal = (VertexType & 0x2) > 0;
            bool hasColor = (VertexType & 0x4) > 0;
            bool hasUnk0 = (VertexType & 0x8) > 0;
            bool hasUnk1 = (VertexType & 0x10) > 0;
            bool hasUnk2 = (VertexType & 0x20) > 0;
            bool hasUnk3 = (VertexType & 0x40) > 0;
            bool hasUnk4 = (VertexType & 0x80) > 0;

            for (int i = 0; i < Positions.Count; i++)
            {
                outBytes.AddValue(Positions[i]);
                if (hasNormal)
                    outBytes.AddValue(Normals[i]);
                if (hasColor)
                    outBytes.AddRange(Colors[i]);
                if (hasUV)
                    outBytes.AddValue(UVs[i]);
            }
            outBytes.AlignWriter(0x20);

            POF0Offsets.Add(outBytes.Count + offset + 4);
            outBytes.AddValue(VertexType);
            outBytes.AddValue(Ushort_02);
            outBytes.AddValue(offset);
            outBytes.AddValue(VertexSize);
            outBytes.AddValue(Positions.Count);

            outBytes.AlignWriter(0x20);
        }
    }
}
