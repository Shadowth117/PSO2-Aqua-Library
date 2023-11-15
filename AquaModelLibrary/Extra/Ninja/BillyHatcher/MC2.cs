using Reloaded.Memory.Streams;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace AquaModelLibrary.Extra.Ninja.BillyHatcher
{
    public class MC2
    {
        public NinjaHeader njHeader;
        public MC2Header header;
        public List<Vector3> vertPositions = new List<Vector3>();
        public List<MC2FaceData> faceData = new List<MC2FaceData>();
        public List<MC2Sector> sectors = new List<MC2Sector>();
        public List<MC2UnkData> unkDataList = new List<MC2UnkData>();

        //Offsets are off by 8 due tot eh ninja header
        public MC2() { }

        public MC2(BufferedStreamReader sr)
        {
            njHeader = sr.Read<NinjaHeader>();
            BigEndianHelper._active = sr.Peek<int>() > sr.PeekBigEndianPrimitiveInt32();

            MC2Header header = new MC2Header();
            header.vertPositionsOffset = sr.ReadBE<int>();
            header.vertPositionsCount = sr.ReadBE<int>();
            header.faceData = sr.ReadBE<int>();
            header.faceCount = sr.ReadBE<int>();
            header.sectorDataOffset = sr.ReadBE<int>();
            header.sectorDataCount = sr.ReadBE<int>();
            header.minBounding = sr.ReadBEV2();
            header.maxBounding = sr.ReadBEV2();
            header.ushort0 = sr.ReadBE<ushort>();
            header.ushort1 = sr.ReadBE<ushort>();
            header.ushort2 = sr.ReadBE<ushort>();
            header.ushort3 = sr.ReadBE<ushort>();
            header.unkOffset = sr.ReadBE<int>();
            header.unkCount = sr.ReadBE<int>();

            this.header = header;
            sr.Seek(header.vertPositionsOffset + 0x8, System.IO.SeekOrigin.Begin);
            for (int i = 0; i < header.vertPositionsCount; i++)
            {
                vertPositions.Add(sr.ReadBEV3());
            }

            sr.Seek(header.faceData + 0x8, System.IO.SeekOrigin.Begin);
            for (int i = 0; i < header.faceCount; i++)
            {
                var mc2FaceData = new MC2FaceData();
                mc2FaceData.vert0 = sr.ReadBE<ushort>();
                mc2FaceData.vert1 = sr.ReadBE<ushort>();
                mc2FaceData.vert2 = sr.ReadBE<ushort>();
                mc2FaceData.usht3 = sr.ReadBE<ushort>();

                mc2FaceData.bt_4 = sr.ReadBE<byte>();
                mc2FaceData.bt_5 = sr.ReadBE<byte>();
                mc2FaceData.flagSet2 = sr.ReadBE<byte>();
                mc2FaceData.flagSet3 = sr.ReadBE<byte>();

                mc2FaceData.faceNormal = sr.ReadBEV3();
                header.minBounding = sr.ReadBEV2();
                header.maxBounding = sr.ReadBEV2();

                faceData.Add(mc2FaceData);
            }

            sr.Seek(header.sectorDataOffset + 0x8, System.IO.SeekOrigin.Begin);
            for (int i = 0; i < header.sectorDataCount; i++)
            {
                sectors.Add(new MC2Sector(sr));
            }

            bool doUnkData = header.unkCount > 0 && header.unkCount < 0xFFFF;
            if (doUnkData)
            {
                sr.Seek(header.unkOffset + 0x8, System.IO.SeekOrigin.Begin);
                for (int i = 0; i < header.unkCount; i++)
                {
                    var unk = new MC2UnkData();
                    unk.usht0 = sr.ReadBE<ushort>();
                    unk.usht1 = sr.ReadBE<ushort>();
                    unk.usht2 = sr.ReadBE<ushort>();
                    unk.usht3 = sr.ReadBE<ushort>();
                    unkDataList.Add(unk);
                }
            }
        }

        public byte[] GetBytes()
        {
            List<int> offsets = new List<int>();
            ByteListExtension.AddAsBigEndian = true;
            List<byte> outBytes = new List<byte>();
            bool doUnkData = header.unkCount > 0 && header.unkCount < 0xFFFF;

            offsets.Add(outBytes.Count);
            outBytes.ReserveInt("VertsOffset");
            outBytes.AddValue(vertPositions.Count);
            offsets.Add(outBytes.Count);
            outBytes.ReserveInt("FaceDataOffset");
            outBytes.AddValue(faceData.Count);
            offsets.Add(outBytes.Count);
            outBytes.ReserveInt("SectorDataOffset");
            outBytes.AddValue(sectors.Count);
            outBytes.AddValue(header.minBounding.X);
            outBytes.AddValue(header.minBounding.Y);
            outBytes.AddValue(header.maxBounding.X);
            outBytes.AddValue(header.maxBounding.Y);
            outBytes.AddValue(header.ushort0);
            outBytes.AddValue(header.ushort1);
            outBytes.AddValue(header.ushort2);
            outBytes.AddValue(header.ushort3);

            if (doUnkData)
            {
                offsets.Add(outBytes.Count);
                outBytes.ReserveInt("UnkData");
                outBytes.AddValue(header.unkCount);
            }
            else
            {
                outBytes.AddValue((int)0);
                outBytes.AddValue((int)0);
            }

            //Verts
            outBytes.FillInt("VertsOffset", outBytes.Count);
            for (int i = 0; i < vertPositions.Count; i++)
            {
                outBytes.AddValue(vertPositions[i].X);
                outBytes.AddValue(vertPositions[i].Y);
                outBytes.AddValue(vertPositions[i].Z);
            }

            //FaceData
            outBytes.FillInt("FaceDataOffset", outBytes.Count);
            for (int i = 0; i < faceData.Count; i++)
            {
                outBytes.AddValue(faceData[i].vert0);
                outBytes.AddValue(faceData[i].vert1);
                outBytes.AddValue(faceData[i].vert2);
                outBytes.AddValue(faceData[i].usht3);
                outBytes.Add(faceData[i].bt_4);
                outBytes.Add(faceData[i].bt_5);
                outBytes.Add(faceData[i].flagSet2);
                outBytes.Add(faceData[i].flagSet3);
                outBytes.AddValue(faceData[i].faceNormal.X);
                outBytes.AddValue(faceData[i].faceNormal.Y);
                outBytes.AddValue(faceData[i].faceNormal.Z);
                outBytes.AddValue(faceData[i].minBounding.X);
                outBytes.AddValue(faceData[i].minBounding.Y);
                outBytes.AddValue(faceData[i].maxBounding.X);
                outBytes.AddValue(faceData[i].maxBounding.Y);
            }

            //Sectors
            outBytes.FillInt("SectorDataOffset", outBytes.Count);
            for (int i = 0; i < sectors.Count; i++)
            {
                outBytes.AddValue(sectors[i].mc2Sector.usesOffset);
                outBytes.AddValue(sectors[i].mc2Sector.indexCount);
                if (sectors[i].mc2Sector.usesOffset > 0)
                {
                    offsets.Add(outBytes.Count);
                }
                outBytes.ReserveInt($"SectorOffset{i}");
                outBytes.AddValue(sectors[i].mc2Sector.minBounding.X);
                outBytes.AddValue(sectors[i].mc2Sector.minBounding.Y);
                outBytes.AddValue(sectors[i].mc2Sector.maxBounding.X);
                outBytes.AddValue(sectors[i].mc2Sector.maxBounding.Y);
                outBytes.AddValue(sectors[i].mc2Sector.index0);
                outBytes.AddValue(sectors[i].mc2Sector.index1);
                outBytes.AddValue(sectors[i].mc2Sector.index2);
                outBytes.AddValue(sectors[i].mc2Sector.index3);
            }

            //Stripdata
            for (int i = 0; i < sectors.Count; i++)
            {
                if (sectors[i].mc2Sector.usesOffset > 0)
                {
                    outBytes.FillInt($"SectorOffset{i}", outBytes.Count);
                    foreach (var index in sectors[i].stripData)
                    {
                        outBytes.AddValue(index);
                    }
                }
            }

            //UnkData
            if (doUnkData)
            {
                outBytes.FillInt($"UnkData", outBytes.Count);
                for (int i = 0; i < unkDataList.Count; i++)
                {
                    var unk = unkDataList[i];
                    outBytes.AddValue(unk.usht0);
                    outBytes.AddValue(unk.usht1);
                    outBytes.AddValue(unk.usht2);
                    outBytes.AddValue(unk.usht3);
                }
            }

            //Ninja header
            List<byte> ninjaBytes = new List<byte>() { 0x43, 0x53, 0x4E, 0x49 };
            ninjaBytes.AddRange(BitConverter.GetBytes(outBytes.Count));

            outBytes.InsertRange(0, ninjaBytes);
            outBytes.AddRange(POF0.GeneratePOF0(offsets));

            ByteListExtension.Reset();
            return outBytes.ToArray();
        }

        /// <summary>
        /// Only in a few files. Might just be used for the devs for debugging.
        /// </summary>
        public struct MC2UnkData
        {
            public ushort usht0;
            public ushort usht1;
            public ushort usht2;
            public ushort usht3;
        }

        public struct MC2Header
        {
            public int vertPositionsOffset;
            public int vertPositionsCount;
            public int faceData;
            public int faceCount;

            public int sectorDataOffset;
            public int sectorDataCount;
            public Vector2 minBounding;
            public Vector2 maxBounding;
            public ushort ushort0;
            public ushort ushort1;
            /// <summary>
            /// Almost always 0x6
            /// </summary>
            public ushort ushort2;
            /// <summary>
            /// Always 0x14
            /// </summary>
            public ushort ushort3;

            //These fields don't always exist and only a few of the retail files use the offset data. 
            public int unkOffset;
            public int unkCount;
        }

        public enum FlagSet0 : byte
        {
            None = 0x0,
            Lava = 0x1,
            Unk0x2 = 0x2,
            Slide = 0x4,
            Unk0x8 = 0x8,
            Unk0x10 = 0x10,
            /// <summary>
            /// This one is a little weird. When Billy grabs an egg, the egg no longer collides with faces marked with this.
            /// The camera will also follow the egg while it is in the 'held' state, even if it falls thorugh the floor, until Billy backs off from it.
            /// Billy will fall through the ground too while he is 'attached' to the egg, such as when he rolls with the egg, attempts to bounce, or slam with it.
            /// </summary>
            NoBillyAndEggCollision = 0x20,
            Unk0x40 = 0x40,
            /// <summary>
            /// Billy just dies upon touching this. Can look awkward if DefaultGround is on.
            /// </summary>
            Death = 0x80,
        }

        public enum FlagSet1 : byte
        {
            None = 0,
            /// <summary>
            /// Default ground collision. For normal Billy Hatchering about.
            /// </summary>
            DefaultGround = 0x1,
            Unk0x2 = 0x2,
            /// <summary>
            /// Billy just instantly dies a water death on touching this.
            /// </summary>
            Drown = 0x4,
            /// <summary>
            /// Billy just instantly dies a sand death on touching this.
            /// </summary>
            Quicksand = 0x8,
            Unk0x10 = 0x10,
            Unk0x20 = 0x20,
            Unk0x40 = 0x40,
            /// <summary>
            /// Ground gives use snow sound effects.
            /// </summary>
            Snow = 0x80, 
        }

        public class MC2FaceData
        {
            public ushort vert0;
            public ushort vert1;
            public ushort vert2;
            public ushort usht3;

            public byte bt_4; //4 and 5 might be flags, but don't seem to directly affect Billy or the egg.
            public byte bt_5;
            public byte flagSet2;
            public byte flagSet3;

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
