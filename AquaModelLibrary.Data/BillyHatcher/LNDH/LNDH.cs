using System.Numerics;

namespace AquaModelLibrary.Data.BillyHatcher.LNDH
{
    //LND header
    public struct LNDHeader
    {
        public int lndHeader2Offset;
        public ushort nodeCount;
        public ushort motionDataCount;
        public int lndMeshInfoOffset;
        public int motionDataOffset;

        public int lndTexNameListOffset;
    }

    public struct LNDMotionDataHead
    {
        public int lndMotionDataHead2Offset;
        public int frameAboveFinalFrame;
        public ushort keyType;
        public ushort dataType;
    }

    public struct LNDMotionDataHead2
    {

        public int dataOffset;
        public int unkInt;
        /// <summary>
        /// Add 1 for the true count
        /// </summary>
        public int dataCount;
    }

    public class LNDMotionData
    {
        public int frame;
        public Vector3 vec3Data;
        public ushort[] ushtData;
    }

    public struct LNDTexDataEntryHead
    {
        public int offset;
        public ushort count;
        public ushort texCount;
    }

    public struct LNDTexDataEntry
    {
        public int offset;
        public int unk0;
        public int unk1;
    }

    public struct LNDHeader2
    {
        public ushort nodeCount;
        public ushort usht02;
        public int nodesOffset;
        public int int08;
        public ushort usht0C;
        public ushort usht0E;

        public int LNDNodeIdSetOffset;
    }

    public class LNDMeshInfo
    {
        public int flags;
        public int lndMeshInfo2Offset;
        public int int08;
        public int int0C;

        public int int10;
        public int int14;
        public int int18;
        public int int1C;

        public Vector3 Scale;
        public int unkOffset0;
        public int unkOffset1;
        public int unkData;

        //SubObjs
        public LNDMeshInfo2 lndMeshInfo2 = null;
    }

    public class LNDMeshInfo2
    {
        public int layoutsOffset;
        public int unkOffset0;
        public int polyInfo0Offset; //Strips start with 0x98
        public int polyInfo1Offset; //Strips start with 0x90

        public ushort polyInfo0Count;
        public ushort polyInfo1Count;
        public Vector2 minBounding;
        public Vector2 maxBounding;

        //SubObjs
        public List<PolyInfo> polyInfo0List = new List<PolyInfo>();
        public List<PolyInfo> polyInfo1List = new List<PolyInfo>();
        public List<LNDVertLayout> layouts = new List<LNDVertLayout>();
        public VertData vertData = null;
    }

    public class PolyInfo
    {
        public int materialOffset;
        public ushort unkCount;
        public ushort materialDataCount;
        public int polyDataOffset;
        public int polyDataBufferSize;

        //SubObjs
        public List<MaterialInfo> matInfo = new List<MaterialInfo>();
        public List<List<List<int>>> triIndicesList = new List<List<List<int>>>();
        public List<List<List<int>>> triIndicesListStarts = new List<List<List<int>>>();

        //Helper
        public Dictionary<int, int> vertIndexMapping = new Dictionary<int, int>();

        public MaterialInfo? GetMaterialInfoByType(int type)
        {
            foreach (var info in matInfo)
            {
                if (info.matInfoType == type)
                {
                    return info;
                }
            }

            return null;
        }
    }

    public struct MaterialInfo
    {
        public int matInfoType;
        public byte matData0;
        public byte matData1;
        public byte matData2;
        public byte matData3;
    }

    public struct LNDVertLayout
    {
        public byte vertType;
        public byte dataType;
        public ushort vertCount;
        public int unkCount;
        public int vertDataOffset;
        public int vertDataBufferSize;
    }

    public class VertData
    {
        public List<Vector3> vertPositions = new List<Vector3>();  //1, position data
        public List<byte[]> vert2Data = new List<byte[]>();        //2, 3 bytes
        public List<ushort> vertColorData = new List<ushort>();      //3, possibly color data
        public List<short[]> vertUVData = new List<short[]>();     //5, probably uv data?
    }

    public struct LNDNodeIdSet
    {
        public ushort nodeCount;
        public ushort usht02;
        public int nodeIdsOffset;
    }

    public struct LandEntry
    {
        public ContentFlag flag;
        public ushort objectIndex;
        public ushort motionIndex;

        public Vector2 minBounding;
        public Vector2 maxBounding;
        public Vector3 unkVec3; //0 If flag is 0
        public int int24;
        public int int28;
        public int int2C;

        public Vector3 Scale;
        public int int3C;
    }

    public enum ContentFlag : int
    {
        Normal = 0,
        Motion = 1,
        Unknown = 2,
    }
}
