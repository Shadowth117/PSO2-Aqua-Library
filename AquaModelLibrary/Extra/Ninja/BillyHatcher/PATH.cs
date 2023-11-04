﻿using Reloaded.Memory.Streams;
using System.Collections.Generic;
using System.Numerics;

namespace AquaModelLibrary.Extra.Ninja.BillyHatcher
{
    /// <summary>
    /// .pth files. Similar idae to .mc2 files with bounding boxes linked to data
    /// </summary>
    public class PATH
    {
        public NinjaHeader header;
        public PATHHeader pathHeader;
        public List<List<Vector3>> pointListList = new List<List<Vector3>>();
        public List<List<Vector3>> normalListList = new List<List<Vector3>>();
        public List<List<float>> lengthsListList = new List<List<float>>();
        public List<PathInfo> pathInfoList = new List<PathInfo>();
        public List<VertDefinition> vertDefinitions = new List<VertDefinition>();
        public List<PathDefinition> pathDefinitions = new List<PathDefinition>();
        public List<RawPathDefinition> rawPathDefinitions = new List<RawPathDefinition>();

        public PATH() { }

        public PATH(BufferedStreamReader sr)
        {
            BigEndianHelper._active = true;
            header = sr.Read<NinjaHeader>();
            pathHeader = new PATHHeader();
            pathHeader.pathInfoCount = sr.ReadBE<int>();
            pathHeader.pathInfoOffset = sr.ReadBE<int>();
            pathHeader.pathCount = sr.ReadBE<int>();
            pathHeader.pathOffset = sr.ReadBE<int>();
            pathHeader.rawPathCount = sr.ReadBE<int>();
            pathHeader.rawPathOffset = sr.ReadBE<int>();

            sr.Seek(pathHeader.pathInfoOffset + 0x8, System.IO.SeekOrigin.Begin);
            for (int i = 0; i < pathHeader.pathInfoCount; i++)
            {
                PathInfo pathInfo = new PathInfo();
                pathInfo.unkByte0 = sr.Read<byte>();
                pathInfo.unkByte1 = sr.Read<byte>();
                pathInfo.lengthsCount = sr.ReadBE<ushort>();
                pathInfo.id = sr.ReadBE<int>();
                pathInfo.unkInt = sr.ReadBE<int>();
                pathInfo.totalLength = sr.ReadBE<float>();
                pathInfo.definitionOffset = sr.ReadBE<int>();
                pathInfo.lengthsOffset = sr.ReadBE<int>();
                pathInfoList.Add(pathInfo);

                var bookmark = sr.Position();
                sr.Seek(pathInfo.definitionOffset + 0x8, System.IO.SeekOrigin.Begin);
                VertDefinition vertDef = new VertDefinition();
                vertDef.unkByte0 = sr.Read<byte>();
                vertDef.unkByte1 = sr.Read<byte>();
                vertDef.vertCount = sr.ReadBE<ushort>();
                vertDef.vertPositionsOffset = sr.ReadBE<int>();
                vertDef.vertNormalsOffset = sr.ReadBE<int>();
                vertDefinitions.Add(vertDef);

                sr.Seek(vertDef.vertPositionsOffset + 0x8, System.IO.SeekOrigin.Begin);
                List<Vector3> positions = new List<Vector3>();
                for (int j = 0; j < vertDef.vertCount; j++)
                {
                    positions.Add(sr.ReadBEV3());
                }
                pointListList.Add(positions);

                sr.Seek(vertDef.vertNormalsOffset + 0x8, System.IO.SeekOrigin.Begin);
                List<Vector3> normals = new List<Vector3>();
                for (int j = 0; j < vertDef.vertCount; j++)
                {
                    normals.Add(sr.ReadBEV3());
                }
                normalListList.Add(normals);

                sr.Seek(pathInfo.lengthsOffset + 0x8, System.IO.SeekOrigin.Begin);
                List<float> lengths = new List<float>();
                for (int j = 0; j < pathInfo.lengthsCount; j++)
                {
                    lengths.Add(sr.ReadBE<float>());
                }
                lengthsListList.Add(lengths);

                sr.Seek(bookmark, System.IO.SeekOrigin.Begin);
            }

            sr.Seek(pathHeader.pathOffset + 0x8, System.IO.SeekOrigin.Begin);
            for (int i = 0; i < pathHeader.pathCount; i++)
            {
                PathDefinition pathDef = new PathDefinition();
                pathDef.unkShort0 = sr.ReadBE<ushort>();
                pathDef.usesRawPathOffset = sr.ReadBE<ushort>();
                pathDef.rawPathOffset = sr.ReadBE<int>();
                pathDef.minBounding = sr.ReadBEV2();
                pathDef.maxBounding = sr.ReadBEV2();
                pathDef.index0 = sr.ReadBE<ushort>();
                pathDef.index1 = sr.ReadBE<ushort>();
                pathDef.index2 = sr.ReadBE<ushort>();
                pathDef.index3 = sr.ReadBE<ushort>();
                pathDefinitions.Add(pathDef);
            }

            sr.Seek(pathHeader.rawPathOffset + 0x8, System.IO.SeekOrigin.Begin);
            for (int i = 0; i < pathHeader.rawPathCount / 8; i++)
            {
                RawPathDefinition pathDef = new RawPathDefinition();
                pathDef.unkShort0 = sr.ReadBE<ushort>();
                pathDef.vertSet = sr.ReadBE<ushort>();
                pathDef.startVert = sr.ReadBE<ushort>();
                pathDef.endVert = sr.ReadBE<ushort>();
                rawPathDefinitions.Add(pathDef);
            }
        }

        public struct PATHHeader
        {
            public int pathInfoCount;
            public int pathInfoOffset;
            public int pathCount;
            public int pathOffset;
            public int rawPathCount;
            public int rawPathOffset;
        }

        public struct PathInfo
        {
            public byte unkByte0;
            public byte unkByte1;
            public ushort lengthsCount;
            public int id;
            public int unkInt;
            public float totalLength;
            public int definitionOffset;
            public int lengthsOffset;
        }

        public struct VertDefinition
        {
            public byte unkByte0;
            public byte unkByte1;
            public ushort vertCount;
            public int vertPositionsOffset;
            public int vertNormalsOffset;
        }

        public struct PathDefinition
        {
            public ushort unkShort0;
            public ushort usesRawPathOffset;
            public int rawPathOffset; // Divide by 0x8 for index
            public Vector2 minBounding;
            public Vector2 maxBounding;

            //If usesOffset is 0, indices are here
            public ushort index0;
            public ushort index1;
            public ushort index2;
            public ushort index3;
        }

        public struct RawPathDefinition
        {
            public ushort unkShort0;
            public ushort vertSet;
            public ushort startVert;
            public ushort endVert;
        }
    }
}
