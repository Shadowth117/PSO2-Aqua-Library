using Reloaded.Memory.Streams;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace AquaModelLibrary.Extra.Ninja.BillyHatcher
{
    public class MC2
    {
        public static int maxDepth = 6;

        public NinjaHeader njHeader;
        public MC2Header header;
        public List<Vector3> vertPositions = new List<Vector3>();
        public List<MC2FaceData> faceData = new List<MC2FaceData>();
        public List<MC2Sector> sectors = new List<MC2Sector>(); //While depth is 6 or less, considering root depth 0, subdivide until 64 faces or less. At depth 6, take what's left. Retail has no higher than 231 for any particular mc2.
        public MC2Sector rootSector = null;
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
            header.unkBoundingRange = sr.ReadBEV2();
            header.greatestBoundingRange = sr.ReadBEV2();
            header.ushort0 = sr.ReadBE<ushort>();
            header.ushort1 = sr.ReadBE<ushort>();
            header.maxDepth = sr.ReadBE<ushort>();
            header.ushort3 = sr.ReadBE<ushort>();
            header.unkOffset = sr.ReadBE<int>();
            header.unkCount = sr.ReadBE<int>();

            this.header = header;
            sr.Seek(header.vertPositionsOffset + 0x8, System.IO.SeekOrigin.Begin);
            Vector3 rootBoxMinExtents = new Vector3();
            Vector3 rootBoxMaxExtents = new Vector3();
            for (int i = 0; i < header.vertPositionsCount; i++)
            {
                var vert = sr.ReadBEV3();
                vertPositions.Add(vert);

                if(i == 0)
                {
                    rootBoxMaxExtents = vert;
                    rootBoxMinExtents = vert;
                    continue;
                }

                //Min extents
                if (rootBoxMinExtents.X > vert.X)
                {
                    rootBoxMinExtents.X = vert.X;
                }
                if (rootBoxMinExtents.Y > vert.Y)
                {
                    rootBoxMinExtents.Y = vert.Y;
                }
                if (rootBoxMinExtents.Z > vert.Z)
                {
                    rootBoxMinExtents.Z = vert.Z;
                }

                //Max extents
                if (rootBoxMaxExtents.X < vert.X)
                {
                    rootBoxMaxExtents.X = vert.X;
                }
                if (rootBoxMaxExtents.Y < vert.Y)
                {
                    rootBoxMaxExtents.Y = vert.Y;
                }
                if (rootBoxMaxExtents.Z < vert.Z)
                {
                    rootBoxMaxExtents.Z = vert.Z;
                }
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
                mc2FaceData.flagSet0 = (FlagSet0)sr.ReadBE<byte>();
                mc2FaceData.flagSet1 = (FlagSet1)sr.ReadBE<byte>();

                mc2FaceData.faceNormal = sr.ReadBEV3();
                mc2FaceData.faceCenter = sr.ReadBEV3();
                mc2FaceData.unkFloat = sr.ReadBE<float>();

                mc2FaceData.center = (vertPositions[mc2FaceData.vert0] + vertPositions[mc2FaceData.vert1] + vertPositions[mc2FaceData.vert2]) / 3;
                mc2FaceData.vert0Value = vertPositions[mc2FaceData.vert0];
                mc2FaceData.vert1Value = vertPositions[mc2FaceData.vert1];
                mc2FaceData.vert2Value = vertPositions[mc2FaceData.vert2];

                faceData.Add(mc2FaceData);
            }

            sr.Seek(header.sectorDataOffset + 0x8, System.IO.SeekOrigin.Begin);
            for (int i = 0; i < header.sectorDataCount; i++)
            {
                sectors.Add(new MC2Sector(sr));
            }

            foreach (var sector in sectors)
            {
                if(sector.childId0 != 0)
                {
                    sector.childSector0 = sectors[sector.childId0];
                }
                if (sector.childId1 != 0)
                {
                    sector.childSector1 = sectors[sector.childId1];
                }
                if (sector.childId2 != 0)
                {
                    sector.childSector2 = sectors[sector.childId2];
                }
                if (sector.childId3 != 0)
                {
                    sector.childSector3 = sectors[sector.childId3];
                }

                foreach (var index in sector.stripData)
                {
                    sector.faceData.Add(faceData[index]);
                }
            }
            rootSector = sectors[0];

            IterateSector(rootSector, 0);

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

        public static void IterateSector(MC2Sector sector, int depth)
        {
            sector.depth = depth;

            if(sector.faceData.Count > 0)
            {
                sector.faceAverage = new Vector3();
                for (int i = 0; i < sector.faceData.Count; i++)
                {
                    sector.faceAverage += sector.faceData[i].center;
                }
                sector.faceAverage /= sector.faceData.Count;
            }

            if(sector.childSector0 != null)
            {
                IterateSector(sector.childSector0, depth + 1);
            }
            if (sector.childSector1 != null)
            {
                IterateSector(sector.childSector1, depth + 1);
            }
            if (sector.childSector2 != null)
            {
                IterateSector(sector.childSector2, depth + 1);
            }
            if (sector.childSector3 != null)
            {
                IterateSector(sector.childSector3, depth + 1);
            }
        }

        public MC2Sector SubdivideSector(Vector2 XRange, Vector2 ZRange, List<ushort> faceIndices, int depth)
        {
            MC2Sector sector = new MC2Sector();
            sector.XRange = XRange;
            sector.ZRange = ZRange;

            //Extra
            sector.depth = depth;

            List<ushort> newFaceIndices = new List<ushort>();
            List<MC2.MC2FaceData> newFaces = new List<MC2.MC2FaceData>();
            foreach(var faceId in faceIndices)
            {
                var face = faceData[faceId];

                var vertX = vertPositions[face.vert0];
                var vertY = vertPositions[face.vert1];
                var vertZ = vertPositions[face.vert2];

                //Check if this vertex is within the bounds of the bounding box. If it is, we include all faces it's used in
                if ((IsInBounds(vertX.X, XRange) && IsInBounds(vertX.Z, ZRange)) || (IsInBounds(vertY.X, XRange) && IsInBounds(vertY.Z, ZRange)) || (IsInBounds(vertZ.X, XRange) && IsInBounds(vertZ.Z, ZRange)))
                {
                    newFaceIndices.Add(faceId);
                    newFaces.Add(face);
                }
            }

            //We continue until we've reached the maximum depth or we hit under or equal to 64
            if (depth >= maxDepth || newFaceIndices.Count <= 64 || newFaceIndices.Count == 0)
            {
                sector.stripData = newFaceIndices;
                sector.indexCount = (ushort)newFaceIndices.Count;
                sector.usesOffset = 1;
                foreach(var face in newFaceIndices)
                {
                    sector.faceData.Add(faceData[face]);
                }
            } else
            {
                var XHalfRange = Math.Abs(XRange.Y - XRange.X) / 2;
                var ZHalfRange = Math.Abs(ZRange.Y - ZRange.X) / 2;

                var leftRange = new Vector2(XRange.X, XRange.Y - XHalfRange);
                var rightRange = new Vector2(XRange.X + XHalfRange, XRange.Y);
                var upRange = new Vector2(ZRange.X + ZHalfRange, ZRange.Y);
                var downRange = new Vector2(ZRange.X, ZRange.Y - ZHalfRange);

                var child0 = SubdivideSector(leftRange, downRange, newFaceIndices, depth + 1);
                sector.childSector0 = child0;
                sectors.Add(child0);
                sector.childId0 = (short)sectors.Count;

                var child1 = SubdivideSector(leftRange, upRange, newFaceIndices, depth + 1);
                sector.childSector1 = child1;
                sectors.Add(child1);
                sector.childId1 = (short)sectors.Count;

                var child2 = SubdivideSector(rightRange, downRange, newFaceIndices, depth + 1);
                sector.childSector2 = child2;
                sectors.Add(child2);
                sector.childId2 = (short)sectors.Count;

                var child3 = SubdivideSector(rightRange, upRange, newFaceIndices, depth + 1);
                sector.childSector3 = child3;
                sectors.Add(child3);
                sector.childId3 = (short)sectors.Count;
            }
            
            return sector;
        }

        public bool IsInBounds(float value, Vector2 range)
        {
            return value >= range.X && value <= range.Y;
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
            outBytes.AddValue(header.unkBoundingRange.X);
            outBytes.AddValue(header.unkBoundingRange.Y);
            outBytes.AddValue(header.greatestBoundingRange.X);
            outBytes.AddValue(header.greatestBoundingRange.Y);
            outBytes.AddValue(header.ushort0);
            outBytes.AddValue(header.ushort1);
            outBytes.AddValue(header.maxDepth);
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
                outBytes.Add((byte)faceData[i].flagSet0);
                outBytes.Add((byte)faceData[i].flagSet1);
                outBytes.AddValue(faceData[i].faceNormal.X);
                outBytes.AddValue(faceData[i].faceNormal.Y);
                outBytes.AddValue(faceData[i].faceNormal.Z);
                outBytes.AddValue(faceData[i].faceCenter.X);
                outBytes.AddValue(faceData[i].faceCenter.Y);
                outBytes.AddValue(faceData[i].faceCenter.Z);
                outBytes.AddValue(faceData[i].unkFloat);
            }

            //Sectors
            outBytes.FillInt("SectorDataOffset", outBytes.Count);
            for (int i = 0; i < sectors.Count; i++)
            {
                outBytes.AddValue(sectors[i].usesOffset);
                outBytes.AddValue(sectors[i].indexCount);
                if (sectors[i].usesOffset > 0)
                {
                    offsets.Add(outBytes.Count);
                }
                outBytes.ReserveInt($"SectorOffset{i}");
                outBytes.AddValue(sectors[i].XRange.X);
                outBytes.AddValue(sectors[i].XRange.Y);
                outBytes.AddValue(sectors[i].ZRange.X);
                outBytes.AddValue(sectors[i].ZRange.Y);
                outBytes.AddValue(sectors[i].childId0);
                outBytes.AddValue(sectors[i].childId1);
                outBytes.AddValue(sectors[i].childId2);
                outBytes.AddValue(sectors[i].childId3);
            }

            //Stripdata
            for (int i = 0; i < sectors.Count; i++)
            {
                if (sectors[i].usesOffset > 0)
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
            //These ranges are in every mc2, but don't seem to be used in the retail game.
            public Vector2 unkBoundingRange;
            public Vector2 greatestBoundingRange;
            public ushort ushort0;
            public ushort ushort1;
            /// <summary>
            /// Almost always 0x6
            /// </summary>
            public ushort maxDepth;
            /// <summary>
            /// Always 0x14
            /// </summary>
            public ushort ushort3;

            //These fields don't always exist and only a few of the retail files use the offset data. 
            public int unkOffset;
            public int unkCount;
        }

        [Flags]
        public enum FlagSet0 : byte
        {
            None = 0x0,
            Lava = 0x1,
            Unk0_0x2 = 0x2,
            Slide = 0x4,
            Unk0_0x8 = 0x8,
            Unk0_0x10 = 0x10,
            /// <summary>
            /// This one is a little weird. When Billy grabs an egg, the egg no longer collides with faces marked with this.
            /// The camera will also follow the egg while it is in the 'held' state, even if it falls thorugh the floor, until Billy backs off from it.
            /// Billy will fall through the ground too while he is 'attached' to the egg, such as when he rolls with the egg, attempts to bounce, or slam with it.
            /// </summary>
            NoBillyAndEggCollision = 0x20,
            Unk0_0x40 = 0x40,
            /// <summary>
            /// Billy just dies upon touching this. Can look awkward if DefaultGround is on.
            /// </summary>
            Death = 0x80,
        }

        [Flags]
        public enum FlagSet1 : byte
        {
            None = 0,
            /// <summary>
            /// Default ground collision. For normal Billy Hatchering about.
            /// </summary>
            DefaultGround = 0x1,
            Unk1_0x2 = 0x2,
            /// <summary>
            /// Billy just instantly dies a water death on touching this.
            /// </summary>
            Drown = 0x4,
            /// <summary>
            /// Billy just instantly dies a sand death on touching this.
            /// </summary>
            Quicksand = 0x8,
            Unk1_0x10 = 0x10,
            Unk1_0x20 = 0x20,
            Unk1_0x40 = 0x40,
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
            public FlagSet0 flagSet0;
            public FlagSet1 flagSet1;

            public Vector3 faceNormal;
           
            //These are blank for collision that's actually used.
            public Vector3 faceCenter;
            public float unkFloat;

            //Extra
            public Vector3 center;
            public Vector3 vert0Value;
            public Vector3 vert1Value;
            public Vector3 vert2Value;
        }

        /// <summary>
        /// An MC2Sector is a quadtree box within the MC2.
        /// 
        /// This is a representation of the four boxes that can be subdivided within this one.
        /// Boxes are always separated horizontally so Y/Vertical values remain constant.
        ///   
        /// ---------------------------------------
        /// |                  |                  |
        /// |                  |                  |
        /// |                  |                  |
        /// |    ChildBox1     |    ChildBox3     |
        /// |                  |                  |
        /// |                  |                  |
        /// |                  |                  |
        /// | ------------------------------------| 
        /// |                  |                  |
        /// |                  |                  |
        /// |                  |                  |
        /// |     ChildBox0    |    ChildBox2     |
        /// |                  |                  |
        /// |                  |                  |
        /// |                  |                  |
        /// ---------------------------------------
        /// </summary>
        public class MC2Sector
        {
            public ushort usesOffset;
            public ushort indexCount;
            public int indexOffset;
            public Vector2 XRange;
            public Vector2 ZRange;

            //If usesOffset is 0, indices are here
            public short childId0;
            public short childId1;
            public short childId2;
            public short childId3;
            public MC2Sector childSector0;
            public MC2Sector childSector1;
            public MC2Sector childSector2;
            public MC2Sector childSector3;
            public List<MC2FaceData> faceData = new List<MC2FaceData>();
            public int depth = -1;
            public Vector3? faceAverage = null;

            /// <summary>
            /// References the faceData ids, NOT vertex ids!
            /// </summary>
            public List<ushort> stripData = new List<ushort>();

            public MC2Sector() { }

            public MC2Sector(BufferedStreamReader sr)
            {
                usesOffset = sr.ReadBE<ushort>();
                indexCount = sr.ReadBE<ushort>();
                indexOffset = sr.ReadBE<int>();
                XRange = sr.ReadBEV2();
                ZRange = sr.ReadBEV2();
                childId0 = sr.ReadBE<short>();
                childId1 = sr.ReadBE<short>();
                childId2 = sr.ReadBE<short>();
                childId3 = sr.ReadBE<short>();

                if (usesOffset != 0)
                {
                    var bookmark = sr.Position();
                    sr.Seek(indexOffset + 8, System.IO.SeekOrigin.Begin);
                    for (int i = 0; i < indexCount; i++)
                    {
                        stripData.Add(sr.ReadBE<ushort>());
                    }
                    sr.Seek(bookmark, System.IO.SeekOrigin.Begin);
                }
            }
        }

    }
}
