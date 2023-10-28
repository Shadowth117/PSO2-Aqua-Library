using Reloaded.Memory.Streams;
using System.Collections.Generic;
using System.Numerics;

namespace AquaModelLibrary.Extra.Ninja
{
    public class MC2
    {
        public NinjaHeader njHeader;
        public MC2Header header;
        public List<Vector3> vertPositions = new List<Vector3>();
        public List<MC2FaceData> faceData = new List<MC2FaceData>();
        public List<MC2Sector> sectors = new List<MC2Sector>();

        //Offsets are off by 8 due tot eh ninja header
        public MC2() { }

        public MC2(BufferedStreamReader sr)
        {
            njHeader = sr.Read<NinjaHeader>();
            BigEndianHelper._active = sr.Peek<int>() > sr.PeekBigEndianPrimitiveInt32();

            MC2Header header = new MC2Header();
            header.vertPositionsOffset = sr.ReadBE<int>();
            header.vertPositionsCount = sr.ReadBE<int>();
            header.unkOffset = sr.ReadBE<int>();
            header.unkCount = sr.ReadBE<int>();
            header.stripDataOffset = sr.ReadBE<int>();
            header.stripDataCount = sr.ReadBE<int>();
            header.minBounding = sr.ReadBEV2();
            header.maxBounding = sr.ReadBEV2();
            header.ushort0 = sr.Read<ushort>();
            header.ushort1 = sr.Read<ushort>();
            header.ushort2 = sr.Read<ushort>();
            header.ushort3 = sr.Read<ushort>();

            sr.Seek(header.vertPositionsOffset + 0x8, System.IO.SeekOrigin.Begin);
            for (int i = 0; i < header.vertPositionsCount; i++)
            {
                vertPositions.Add(sr.ReadBEV3());
            }

            sr.Seek(header.unkOffset + 0x8, System.IO.SeekOrigin.Begin);
            for (int i = 0; i < header.unkCount; i++)
            {
                var mc2FaceData = new MC2FaceData();
                mc2FaceData.vert0 = sr.ReadBE<ushort>();
                mc2FaceData.vert1 = sr.ReadBE<ushort>();
                mc2FaceData.vert2 = sr.ReadBE<ushort>();
                mc2FaceData.usht3 = sr.ReadBE<ushort>();

                mc2FaceData.bt0 = sr.ReadBE<byte>();
                mc2FaceData.bt1 = sr.ReadBE<byte>();
                mc2FaceData.bt2 = sr.ReadBE<byte>();
                mc2FaceData.bt3 = sr.ReadBE<byte>();

                mc2FaceData.faceNormal = sr.ReadBEV3();
                header.minBounding = sr.ReadBEV2();
                header.maxBounding = sr.ReadBEV2();

                faceData.Add(mc2FaceData);
            }

            sr.Seek(header.stripDataOffset + 0x8, System.IO.SeekOrigin.Begin);
            for (int i = 0; i < header.stripDataCount; i++)
            {
                sectors.Add(new MC2Sector(sr));
            }

            int highest = 0;
            foreach(var strip in sectors)
            {
                foreach(var id in strip.stripData)
                {
                    if(id > highest)
                    {
                        highest = id;
                    }
                }
            }
        }

        public struct MC2Header
        {
            public int vertPositionsOffset;
            public int vertPositionsCount;
            public int unkOffset;
            public int unkCount;

            public int stripDataOffset;
            public int stripDataCount;
            public Vector2 minBounding;
            public Vector2 maxBounding;
            public ushort ushort0;
            public ushort ushort1;
            public ushort ushort2;
            public ushort ushort3;
        }

        public struct MC2FaceData
        {
            public ushort vert0;
            public ushort vert1;
            public ushort vert2;
            public ushort usht3;

            public byte bt0;
            public byte bt1;
            public byte bt2;
            public byte bt3;

            public Vector3 faceNormal;
            public Vector2 minBounding;
            public Vector2 maxBounding;
        }

        public class MC2Sector
        {
            public MC2SectorInfo mc2Sector;
            /// <summary>
            /// References the faceData ids, NOT vertex ids!
            /// </summary>
            public List<short> stripData = new List<short>();

            public MC2Sector() { }

            public MC2Sector(BufferedStreamReader sr)
            {
                mc2Sector = new MC2SectorInfo();
                mc2Sector.usesOffset = sr.ReadBE<ushort>();
                mc2Sector.indexCount = sr.ReadBE<ushort>();
                mc2Sector.indexOffset = sr.ReadBE<int>();
                mc2Sector.minBounding = sr.ReadBEV2();
                mc2Sector.maxBounding = sr.ReadBEV2();
                mc2Sector.index0 = sr.ReadBE<short>();
                mc2Sector.index1 = sr.ReadBE<short>();
                mc2Sector.index2 = sr.ReadBE<short>();
                mc2Sector.index3 = sr.ReadBE<short>();

                if (mc2Sector.usesOffset == 0)
                {
                    stripData.Add(mc2Sector.index0);
                    stripData.Add(mc2Sector.index1);
                    stripData.Add(mc2Sector.index2);
                    stripData.Add(mc2Sector.index3);
                }
                else
                {
                    var bookmark = sr.Position();
                    sr.Seek(mc2Sector.indexOffset + 8, System.IO.SeekOrigin.Begin);
                    for (int i = 0; i < mc2Sector.indexCount; i++)
                    {
                        stripData.Add(sr.ReadBE<short>());
                    }
                    sr.Seek(bookmark, System.IO.SeekOrigin.Begin);
                }
            }
        }

        public struct MC2SectorInfo
        {
            public ushort usesOffset;
            public ushort indexCount;
            public int indexOffset;
            public Vector2 minBounding;
            public Vector2 maxBounding;

            //If usesOffset is 0, indices are here
            public short index0;
            public short index1;
            public short index2;
            public short index3;
        }
    }
}
