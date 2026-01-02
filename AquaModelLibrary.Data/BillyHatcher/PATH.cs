using AquaModelLibrary.Data.Ninja;
using AquaModelLibrary.Helpers.Extensions;
using AquaModelLibrary.Helpers.Readers;
using System.Numerics;

namespace AquaModelLibrary.Data.BillyHatcher
{
    /// <summary>
    /// .pth files. Similar idea to .mc2 files with bounding boxes linked to data
    /// </summary>
    public class PATH
    {
        //These are path ids that shouuld never be calced in the base game. It's hardcoded there so we'll hard code it here, unless forced otherwise
        //Known builds of the game only have 1 race to a path, but just in case we'll do these as lists
        public static List<int> blueRacePaths = new List<int>() { 8 };
        public static List<int> redRacePaths = new List<int>() { 1 };
        public static List<int> purpleRacePaths = new List<int>() { 3 };
        public static List<int> yellowRacePaths = new List<int>() { 7 };

        public static bool IsRacePath(string fileName, int pathSplineId)
        {
            switch (fileName.ToLower())
            {
                case "path_blue.pth":
                    return blueRacePaths.Contains(pathSplineId);
                case "path_red.pth":
                    return redRacePaths.Contains(pathSplineId);
                case "path_purple.pth":
                    return purpleRacePaths.Contains(pathSplineId);
                case "path_yellow.pth":
                    return yellowRacePaths.Contains(pathSplineId);
            }

            return false;
        }

        public static int maxDepth = 4;
        /// <summary>
        /// If all pathSegments in a sector are this count or lower, the sector should not subdivide further
        /// </summary>
        public static Dictionary<int, int> maxDepthVertCounts = new Dictionary<int, int>()
        {
            { 0, 0},
            { 1, 6},
            { 2, 7},
            { 3, 7},
            { 4, 65535},
        };

        public NinjaHeader header;
        public PATHHeader pathHeader;
        /// <summary>
        /// List of PathInfos, main path definitions.
        /// Paths that use normals are stored first regardless of id
        /// </summary>
        public List<PathInfo> pathInfoList = new List<PathInfo>();
        public List<PathSector> pathSectors = new List<PathSector>();
        public PathSector rootSector = null;
        public Dictionary<int, List<PathSegment>> pathSegmentDict = new Dictionary<int, List<PathSegment>>();
        public PATH() { }
        public PATH(string filePath)
        {
            using (MemoryStream stream = new MemoryStream(File.ReadAllBytes(filePath)))
            using (var streamReader = new BufferedStreamReaderBE<MemoryStream>(stream))
            {
                Read(streamReader);
            }
        }

        public PATH(byte[] file)
        {
            using (MemoryStream stream = new MemoryStream(file))
            using (var streamReader = new BufferedStreamReaderBE<MemoryStream>(stream))
            {
                Read(streamReader);
            }
        }

        public PATH(BufferedStreamReaderBE<MemoryStream> sr)
        {
            Read(sr);
        }

        public void Read(BufferedStreamReaderBE<MemoryStream> sr)
        {
            sr._BEReadActive = true;
            header = sr.Read<NinjaHeader>();
            pathHeader = new PATHHeader();
            pathHeader.pathInfoCount = sr.ReadBE<int>();
            pathHeader.pathInfoOffset = sr.ReadBE<int>();
            pathHeader.pathCount = sr.ReadBE<int>();
            pathHeader.pathOffset = sr.ReadBE<int>();
            pathHeader.rawPathCount = sr.ReadBE<int>();
            pathHeader.rawPathOffset = sr.ReadBE<int>();

            sr.Seek(pathHeader.pathInfoOffset + 0x8, SeekOrigin.Begin);
            for (int i = 0; i < pathHeader.pathInfoCount; i++)
            {
                PathInfo pathInfo = new PathInfo();
                pathInfo.doesNotUseNormals = sr.Read<byte>();
                pathInfo.isObjectPath = sr.Read<byte>();
                pathInfo.lengthsCount = sr.ReadBE<ushort>();
                pathInfo.id = sr.ReadBE<int>();
                pathInfo.isLiquidCurrent = sr.Read<byte>();
                pathInfo.unkFlag0 = sr.Read<byte>();
                pathInfo.unkFlag1 = sr.Read<byte>();
                pathInfo.unkFlag2 = sr.Read<byte>();
                pathInfo.totalLength = sr.ReadBE<float>();
                pathInfo.definitionOffset = sr.ReadBE<int>();
                pathInfo.lengthsOffset = sr.ReadBE<int>();
                pathInfoList.Add(pathInfo);

                if (pathInfo.definitionOffset != 0)
                {
                    var bookmark = sr.Position;
                    sr.Seek(pathInfo.definitionOffset + 0x8, SeekOrigin.Begin);
                    VertDefinition vertDef = new VertDefinition();
                    vertDef.unkByte0 = sr.Read<byte>();
                    vertDef.unkByte1 = sr.Read<byte>();
                    vertDef.vertCount = sr.ReadBE<ushort>();
                    vertDef.vertPositionsOffset = sr.ReadBE<int>();
                    if (pathInfo.doesNotUseNormals == 0)
                    {
                        vertDef.vertNormalsOffset = sr.ReadBE<int>();
                    }
                    pathInfo.vertDef = vertDef;

                    sr.Seek(vertDef.vertPositionsOffset + 0x8, SeekOrigin.Begin);
                    List<Vector3> positions = new List<Vector3>();
                    for (int j = 0; j < vertDef.vertCount; j++)
                    {
                        positions.Add(sr.ReadBEV3());
                    }
                    vertDef.vertPositions = positions;
                    if (pathInfo.doesNotUseNormals == 0)
                    {
                        sr.Seek(vertDef.vertNormalsOffset + 0x8, SeekOrigin.Begin);
                        List<Vector3> normals = new List<Vector3>();
                        for (int j = 0; j < vertDef.vertCount; j++)
                        {
                            normals.Add(sr.ReadBEV3());
                        }
                        vertDef.vertNormals = normals;
                    }

                    sr.Seek(bookmark, SeekOrigin.Begin);
                }

                if (pathInfo.lengthsOffset != 0)
                {
                    var bookmark = sr.Position;
                    sr.Seek(pathInfo.lengthsOffset + 0x8, SeekOrigin.Begin);
                    List<float> lengths = new List<float>();
                    for (int j = 0; j < pathInfo.lengthsCount; j++)
                    {
                        lengths.Add(sr.ReadBE<float>());
                    }
                    pathInfo.lengthsList = lengths;
                    sr.Seek(bookmark, SeekOrigin.Begin);
                }

            }

            sr.Seek(pathHeader.pathOffset + 0x8, SeekOrigin.Begin);
            for (int i = 0; i < pathHeader.pathCount; i++)
            {
                PathSector pathDef = new PathSector();
                pathDef.isFinalSubdivision = sr.ReadBE<ushort>();
                pathDef.rawPathCount = sr.ReadBE<ushort>();
                pathDef.rawPathOffset = sr.ReadBE<int>();
                pathDef.xzMin = sr.ReadBEV2();
                pathDef.xzMax = sr.ReadBEV2();
                pathDef.childId0 = sr.ReadBE<ushort>();
                pathDef.childId1 = sr.ReadBE<ushort>();
                pathDef.childId2 = sr.ReadBE<ushort>();
                pathDef.childId3 = sr.ReadBE<ushort>();
                pathSectors.Add(pathDef);
            }

            sr.Seek(pathHeader.rawPathOffset + 0x8, SeekOrigin.Begin);
            while (sr.Position < pathHeader.rawPathCount + pathHeader.rawPathOffset + 0x8)
            {
                List<PathSegment> pathSegs = new List<PathSegment>();
                var relativeOffset = (int)(sr.Position - 0x8 - pathHeader.rawPathOffset);
                var defCount = sr.ReadBE<ushort>();
                for (int j = 0; j < defCount; j++)
                {
                    var def = new PathSegment();

                    def.vertSet = sr.ReadBE<ushort>();
                    def.startVert = sr.ReadBE<ushort>();
                    def.endVert = sr.ReadBE<ushort>();

                    def.pathInfo = pathInfoList[def.vertSet];
                    pathSegs.Add(def);
                }
                pathSegmentDict.Add((int)(relativeOffset), pathSegs);
            }

            foreach (var sector in pathSectors)
            {
                if (sector.childId0 != 0)
                {
                    sector.childSector0 = pathSectors[sector.childId0];
                }
                if (sector.childId1 != 0)
                {
                    sector.childSector1 = pathSectors[sector.childId1];
                }
                if (sector.childId2 != 0)
                {
                    sector.childSector2 = pathSectors[sector.childId2];
                }
                if (sector.childId3 != 0)
                {
                    sector.childSector3 = pathSectors[sector.childId3];
                }

                if(sector.rawPathCount != 0)
                {
                    sector.pathSegments = pathSegmentDict[sector.rawPathOffset];
                }
            }
            rootSector = pathSectors[0];

            IterateSector(rootSector, 0);
        }

        private int GetPathSegmentListSize()
        {
            int size = 0;
            foreach(var pathSegment in pathSegmentDict)
            {
                size += 2 + pathSegment.Value.Count * 6; 
            }

            return size;
        }

        public byte[] GetBytes()
        {
            List<int> offsets = new List<int>();
            ByteListExtension.AddAsBigEndian = true;
            List<byte> outBytes = new List<byte>();

            outBytes.AddValue((int)pathInfoList.Count);
            offsets.Add(outBytes.Count);
            outBytes.ReserveInt("PathInfoListOffset");
            outBytes.AddValue((int)pathSectors.Count);
            offsets.Add(outBytes.Count);
            outBytes.ReserveInt("SectorsOffset");
            outBytes.AddValue(GetPathSegmentListSize());
            offsets.Add(outBytes.Count);
            outBytes.ReserveInt("PathSegmentOffset");

            outBytes.FillInt("PathInfoListOffset", outBytes.Count);
            for(int i = 0; i < pathInfoList.Count; i++)
            {
                var pathInfo = pathInfoList[i];
                byte doesNotUseVertNormals = pathInfo.vertDef.vertNormals.Count > 0 ? (byte)0 : (byte)1;
                outBytes.Add(doesNotUseVertNormals);
                outBytes.Add(pathInfo.isObjectPath);
                outBytes.AddValue((ushort)pathInfo.lengthsList.Count);
                outBytes.AddValue(pathInfo.id);
                outBytes.Add((byte)pathInfo.isLiquidCurrent);
                outBytes.Add(pathInfo.unkFlag0);
                outBytes.Add(pathInfo.unkFlag1);
                outBytes.Add(pathInfo.unkFlag2);
                outBytes.AddValue(pathInfo.totalLength);
                offsets.Add(outBytes.Count);
                outBytes.ReserveInt($"PathInfoDefs{i}Offset");
                offsets.Add(outBytes.Count);
                outBytes.ReserveInt($"PathInfoLengths{i}Offset");
            }

            for (int i = 0; i < pathInfoList.Count; i++)
            {
                outBytes.FillInt($"PathInfoDefs{i}Offset", outBytes.Count);
                var pathInfo = pathInfoList[i];
                var def = pathInfo.vertDef;
                var lenList = pathInfo.lengthsList;
                byte doesNotUseVertNormals = pathInfo.vertDef.vertNormals.Count > 0 ? (byte)0 : (byte)1;

                outBytes.Add(def.unkByte0);
                outBytes.Add(def.unkByte1);
                outBytes.AddValue((ushort)def.vertPositions.Count);
                offsets.Add(outBytes.Count);
                outBytes.ReserveInt($"VertDefsPos{i}Offset");
                if(doesNotUseVertNormals == 0)
                {
                    offsets.Add(outBytes.Count);
                    outBytes.ReserveInt($"VertDefsNrm{i}Offset");
                }

                outBytes.FillInt($"VertDefsPos{i}Offset", outBytes.Count);
                for(int j = 0; j < def.vertPositions.Count; j++)
                {
                    outBytes.AddValue(def.vertPositions[j]);
                }
                if (doesNotUseVertNormals == 0)
                {
                    outBytes.FillInt($"VertDefsNrm{i}Offset", outBytes.Count);
                    for (int j = 0; j < def.vertNormals.Count; j++)
                    {
                        outBytes.AddValue(def.vertNormals[j]);
                    }
                }

                outBytes.FillInt($"PathInfoLengths{i}Offset", outBytes.Count);
                for(int j = 0; j < lenList.Count; j++)
                {
                    outBytes.AddValue(lenList[j]);
                }
            }

            outBytes.FillInt("SectorsOffset", outBytes.Count);

            //In case for whatever reason we altered the rawPath here, we track with the original id, but use this counter to track the rawOffset sequentially, as the original tools did
            int rawPathCounter = 0;
            for (int i = 0; i < pathSectors.Count; i++)
            {
                PathSector pathDef = pathSectors[i];

                outBytes.AddValue(pathDef.isFinalSubdivision);
                outBytes.AddValue((ushort)(pathDef.rawPathCount > 0 ? pathSegmentDict[pathDef.rawPathOffset].Count : 0));
                outBytes.AddValue((int)(pathDef.rawPathCount > 0 ? rawPathCounter : 0));
                outBytes.AddValue(pathDef.xzMin);
                outBytes.AddValue(pathDef.xzMax);
                outBytes.AddValue(pathDef.childId0);
                outBytes.AddValue(pathDef.childId1);
                outBytes.AddValue(pathDef.childId2);
                outBytes.AddValue(pathDef.childId3);

                if(pathDef.rawPathCount > 0)
                {
                    rawPathCounter += 2 + pathSegmentDict[pathDef.rawPathOffset].Count * 6;
                }
            }

            outBytes.FillInt("PathSegmentOffset", outBytes.Count);
            var rawPathDefKeys = pathSegmentDict.Keys.ToList();
            rawPathDefKeys.Sort();
            for(int i = 0; i < rawPathDefKeys.Count; i++)
            {
                var rawPath = pathSegmentDict[rawPathDefKeys[i]];
                outBytes.AddValue((ushort)rawPath.Count);
                for(int j = 0; j < rawPath.Count; j++)
                {
                    var def = rawPath[j];
                    outBytes.AddValue(def.vertSet);
                    outBytes.AddValue(def.startVert);
                    outBytes.AddValue(def.endVert);
                }
            }
            outBytes.AlignWriter(0x4, 0);

            //Ninja header
            List<byte> ninjaBytes = new List<byte>() { 0x50, 0x41, 0x54, 0x48 };
            ninjaBytes.AddRange(BitConverter.GetBytes(outBytes.Count));

            outBytes.InsertRange(0, ninjaBytes);
            outBytes.AddRange(POF0.GeneratePOF0(offsets));

            ByteListExtension.Reset();
            return outBytes.ToArray();
        }

        private static void IterateSector(PathSector sector, int depth)
        {
            sector.depth = depth;

            if (sector.childSector0 != null)
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

        public struct PATHHeader
        {
            public int pathInfoCount;
            public int pathInfoOffset;
            public int pathCount;
            public int pathOffset;
            public int rawPathCount;
            public int rawPathOffset;
        }

        public class PathInfo
        {
            public byte doesNotUseNormals;
            /// <summary>
            /// Used for Sand Ruin tornadoes, and the title screen chick/crow. Pirates Island has what seem to be unused ones for sharks or something
            /// </summary>
            public byte isObjectPath;
            public ushort lengthsCount;
            public int id;
            /// <summary>
            /// Only used for the Forest Village river in retail.
            /// </summary>
            public byte isLiquidCurrent;
            public byte unkFlag0;
            public byte unkFlag1;
            public byte unkFlag2;
            public float totalLength;
            public int definitionOffset;
            public int lengthsOffset;

            public VertDefinition vertDef = null;
            /// <summary>
            /// Lengths between each point
            /// </summary>
            public List<float> lengthsList = new List<float>();
        }

        public class VertDefinition
        {
            public byte unkByte0;
            public byte unkByte1;
            public ushort vertCount;
            public int vertPositionsOffset;
            public int vertNormalsOffset;

            public List<Vector3> vertPositions = new List<Vector3>();
            public List<Vector3> vertNormals = new List<Vector3>();
        }

        public class PathSegment
        {
            public ushort vertSet;
            public ushort startVert;
            public ushort endVert;

            public PathInfo pathInfo = null;
        }

        /// <summary>
        /// A Path Sector is a quadtree box within the Path.
        /// 
        /// This is a representation of the four boxes that can be subdivided within this one.
        /// Boxes are always separated horizontally so Y/Vertical values remain constant.
        /// 
        /// Subdivision appears to go until depth 4 OR, 3 or less path vertices are in the sector for all paths in a sector (so each path would be 3 or less vertices).
        /// If they exist, the vertices connecting in to the path segment from either end are included in the segment as well.
        /// If a path exits and reenters the sector, I think all vertices get included?
        /// 
        /// Naturally, paths should probably have enough vertices close enough together that they don't skip across sectors though with how long rails take to go across in this, hopefully
        /// a level designer wouldn't do that anyways.
        /// 
        /// Certain path splines are NOT included in this, namely Object path splines and race mission animal path splines.
        /// While the object path splines are marked, race animal path splines are not and appear hardcoded.
        /// Pirates Island, path_blue.pth, lists race as 8.
        /// Dino Mountain, path_red.pth, lists race as 1.
        /// Blizzard Castle, path_purple.pth, lists race as 3.
        /// Sand Ruin, path_yellow.pth, lists race as 7.
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

        public class PathSector
        {
            /// <summary>
            /// If true, sector does not subdivide further
            /// </summary>
            public ushort isFinalSubdivision;
            /// <summary>
            /// The count is in the underlying object, but we need this to to function
            /// </summary>
            public ushort rawPathCount;
            public int rawPathOffset;

            /// <summary>
            /// The x and z minimum corner, as oppposed to the MC2's x range.
            /// </summary>
            public Vector2 xzMin;
            /// <summary>
            /// The x and Z maximum corner, as opposed to the MC2's z range.
            /// </summary>
            public Vector2 xzMax;

            public ushort childId0;
            public ushort childId1;
            public ushort childId2;
            public ushort childId3;
            public PathSector? childSector0;
            public PathSector? childSector1;
            public PathSector? childSector2;
            public PathSector? childSector3;
            
            public int depth = -1;
            public List<PathSegment> pathSegments = null;
        }

        public PathSector SubdivideSector(string pathFileName, Vector2 XRange, Vector2 ZRange, List<PathSegment> pathSegments, int depth)
        {
            PathSector sector = new PathSector();
            pathSectors.Add(sector);
            sector.xzMax = new Vector2(XRange.Y, ZRange.Y);
            sector.xzMin = new Vector2(XRange.X, ZRange.X);

            //Extra
            sector.depth = depth;

            List<PathSegment> newSegments = new List<PathSegment>();
            List<bool> newSegmentUnderOrAtMinimumContainedCount = new List<bool>();
            if(depth == 0)
            {
                //Initialize pathSegments from all paths if we're at the root
                for(int i = 0; i < pathInfoList.Count; i++)
                {
                    if(pathInfoList[i].isObjectPath > 0 || IsRacePath(pathFileName, i))
                    {
                        continue;
                    }
                    PathSegment segment = new PathSegment();
                    segment.vertSet = (ushort)i;
                    segment.pathInfo = pathInfoList[i];
                    segment.startVert = 0;
                    segment.endVert = (ushort)(pathInfoList[i].vertDef.vertPositions.Count - 1);
                    newSegmentUnderOrAtMinimumContainedCount.Add((segment.endVert + 1) <= maxDepthVertCounts[depth]);
                    newSegments.Add(segment);
                }
            } else
            {
                //Loop through each PathSegment to find out what's in range, add the first and last ones outside the range if needed.
                //If segments go out, then back in, include all sections between. It's possible it could split into multiple segments, but we don't know if the game will like that
                //If a segment is not contained within the sector, don't add it again
                for(int j = 0; j < pathSegments.Count; j++)
                {
                    var segment = pathSegments[j];
                    int? newStartVert = null;
                    int? newEndVert = null;
                    for(int i = segment.startVert; i <= segment.endVert; i++)
                    {
                        var vertPos = segment.pathInfo.vertDef.vertPositions[i];
                        //Check if this vertex is within the bounds of the bounding box. If it is, we include it
                        //We do not break early if after an inbounds vertex we find an out of bounds vertex in case a later one is inbounds. We want one segment per path currently
                        if (vertPos.X <= XRange.Y && XRange.X <= vertPos.X && vertPos.Z >= ZRange.X && ZRange.Y >= vertPos.Z)
                        {
                            if(newStartVert == null)
                            {
                                newStartVert = i;
                                newEndVert = i;
                            }
                            if(i > newEndVert)
                            {
                                newEndVert = i;
                            }
                        }
                    }
                    var newSegment = new PathSegment();
                    newSegment.vertSet = segment.vertSet;
                    newSegment.pathInfo = segment.pathInfo;
                    if (newStartVert != null)
                    {
                        newSegmentUnderOrAtMinimumContainedCount.Add((newEndVert - newStartVert) <= maxDepthVertCounts[depth]);
                        if (newStartVert != 0)
                        {
                            newStartVert -= 1;
                        }
                        if (newEndVert != newSegment.pathInfo.vertDef.vertPositions.Count - 1)
                        {
                            newEndVert += 1;
                        }
                        newSegment.startVert = (ushort)newStartVert;
                        newSegment.endVert = (ushort)newEndVert;
                        newSegments.Add(newSegment);
                    }
                }
            }

            //We continue until we've reached the maximum depth, there's no new segments, or we're under or equal to the minimum count
            if (depth >= maxDepth || !newSegmentUnderOrAtMinimumContainedCount.Contains(false) || (newSegments.Count == 0 && depth != 0) )
            {
                sector.isFinalSubdivision = 1;

                sector.rawPathCount = (ushort)newSegments.Count;
                if(newSegments.Count > 0)
                {
                    sector.rawPathOffset = pathSectors.Count;
                    pathSegmentDict.Add(pathSectors.Count, newSegments);
                    sector.pathSegments = newSegments;
                }
            }
            else
            {
                var XDistance = Math.Abs(XRange.Y - XRange.X);
                var ZDistance = Math.Abs(ZRange.Y - ZRange.X);

                var XHalfRange = XDistance / 2;
                var ZHalfRange = ZDistance / 2;

                var leftRange = new Vector2(XRange.X, XRange.Y - XHalfRange);
                var rightRange = new Vector2(XRange.X + XHalfRange, XRange.Y);
                var upRange = new Vector2(ZRange.X + ZHalfRange, ZRange.Y);
                var downRange = new Vector2(ZRange.X, ZRange.Y - ZHalfRange);

                var child0 = SubdivideSector(pathFileName, leftRange, downRange, ClonePathSegmentList(newSegments), depth + 1);
                sector.childSector0 = child0;
                sector.childId0 = (ushort)pathSectors.IndexOf(child0);

                var child1 = SubdivideSector(pathFileName, leftRange, upRange, ClonePathSegmentList(newSegments), depth + 1);
                sector.childSector1 = child1;
                sector.childId1 = (ushort)pathSectors.IndexOf(child1);

                var child2 = SubdivideSector(pathFileName, rightRange, downRange, ClonePathSegmentList(newSegments), depth + 1);
                sector.childSector2 = child2;
                sector.childId2 = (ushort)pathSectors.IndexOf(child2);

                var child3 = SubdivideSector(pathFileName, rightRange, upRange, ClonePathSegmentList(newSegments), depth + 1);
                sector.childSector3 = child3;
                sector.childId3 = (ushort)pathSectors.IndexOf(child3);
            }
            
            return sector;
        }

        private List<PathSegment> ClonePathSegmentList(List<PathSegment> pathSegments)
        {
            List<PathSegment> newSegments = new();
            foreach(var segment in pathSegments)
            {
                PathSegment newSegment = new();
                newSegment.vertSet = segment.vertSet;
                newSegment.startVert = segment.startVert;
                newSegment.endVert = segment.endVert;
                if(newSegment.endVert == 0)
                {
                    var a = 0;
                }
                newSegment.pathInfo = segment.pathInfo; //We don't need to clone this since we want it to be the original object. We aren't changing it.
                newSegments.Add(newSegment);
            }

            return newSegments;
        }
    }
}
