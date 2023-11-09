using System.Collections.Generic;
using System.Numerics;

namespace AquaModelLibrary.Extra.Ninja.BillyHatcher.LNDH
{
    //ARCLND
    public struct ARCLNDHeader
    {
        public int mainDataOffset;
        public int extraFileCount;
        public int extraFileOffsetsOffset;
        public int motionFileOffset;    //Often 0

        public int texRefTableOffset;
        public int GVMOffset;
    }

    public struct ARCLNDRefTableHead
    {
        public int entryOffset;
        public int entryCount;
    }

    public struct ARCLNDRefEntry
    {
        public int textOffset;
        public int unkInt0;
        public int unkInt1;
    }

    public struct ARCLNDMainDataHeader
    {
        public int mainOffsetTableOffset;
        public int altVertexColorOffset;
        public int unkCount;
        public int unkOffset1;

        public int unkInt_10;
        public int unkInt_14;
        public int unkInt_18;
        public int unkInt_1C;
    }

    public struct ARCLNDAltVertColorRef
    {
        public int count;
        public int offset;
    }

    public struct ARCLNDAltVertColorMainRef
    {
        public int id;
        public int offset;
    }

    public class ARCLNDAltVertColorInfo
    {
        public ushort usht00;
        public ushort usht02;
        public ushort usht04;
        public ushort usht06;
        public ushort usht08;
        public ushort usht0A;
        public ushort usht0C;
        public ushort usht0E;

        public ushort usht10;
        /// <summary>
        /// Should match file's usual vert count
        /// </summary>
        public ushort vertColorCount;
        public int vertColorOffset;
        public ushort usht18;
        public ushort usht1A;
        public ushort usht1C;
        public ushort usht1E;

        public ushort usht20;
        public ushort usht22;
        public ushort usht24;
        public ushort usht26;
        public ushort usht28;
        public ushort usht2A;
        public ushort usht2C;
        public ushort usht2E;

        public List<byte[]> vertColors = new List<byte[]>();
    }

    //Similar to NN's main branching point struct
    public struct ARCLNDMainOffsetTable
    {
        public int landEntryCount;
        public int landEntryOffset;
        public int vertDataCount;
        public int vertDataOffset;

        public int faceSetsCount;
        public int faceSetsOffset;
        public int nodeBoundingCount;
        public int nodeBoundingOffset;

        public int unkCount;
        public int meshDataCount;
        public int meshDataOffset;
    }

    public struct ARCLNDLandEntryRef
    {
        public int unkInt;
        public int offset;
    }

    public struct ARCLNDLandEntry
    {
        public int unkInt0;
        public int unkInt1;
        public int unkInt2;
        public int unkInt3;

        public int unkInt4;
        public int unkInt5;
        public int unkInt6;
        public int unkInt7;

        public ushort ushort0;
        public ushort ushort1;
        public int TextureId;
    }

    public struct ARCLNDVertDataRef
    {
        public int unkInt;
        public int offset;
    }

    public class ARCLNDVertDataSet
    {
        public ARCLNDVertData Position;
        public ARCLNDVertData Normal;
        public ARCLNDVertData VertColor;
        public ARCLNDVertData VertColor2;
        public ARCLNDVertData UV1; 
        public ARCLNDVertData UV2; 

        public List<Vector3> PositionData = new List<Vector3>();
        public List<Vector3> NormalData = new List<Vector3>();
        /// <summary>
        /// RGBA order
        /// </summary>
        public List<byte[]> VertColorData = new List<byte[]>();
        public List<byte[]> VertColor2Data = new List<byte[]>();
        public List<short[]> UV1Data = new List<short[]>();
        public List<short[]> UV2Data = new List<short[]>();
    }

    public struct ARCLNDVertData
    {
        public ushort type;
        public ushort count;
        public int offset;
    }

    public struct ARCLNDFaceDataRef
    {
        public int unkInt;
        public int offset;
    }

    public enum ArcLndVertType : int
    {
        Position = 0x1,
        Normal = 0x2,
        VertColor = 0x4,
        VertColor2 = 0x8,
        UV1 = 0x10,
        UV2 = 0x20,
    }

    public class ARCLNDFaceDataHead
    {
        public ArcLndVertType flags;
        public int faceDataOffset0;
        public int bufferSize0;
        public int faceDataOffset1;
        public int bufferSize1;

        public List<List<List<int>>> triIndicesList0 = new List<List<List<int>>>();
        public List<List<List<int>>> triIndicesListStarts0 = new List<List<List<int>>>();
        public List<List<List<int>>> triIndicesList1 = new List<List<List<int>>>();
        public List<List<List<int>>> triIndicesListStarts1 = new List<List<List<int>>>();
    }

    public struct ARCLNDNodeBounding
    {
        public float unkFlt_00;
        public ushort usht_04;
        public ushort usht_06;
        public ushort usht_08;
        public ushort usht_0A;
        public int int_0C;

        public int int_10;
        public int int_14;
        public int int_18;
        public int int_1C;

        public int int_20;
        public int int_24;
        public int int_28;
        public int int_2C;

        public Vector2 minBounding;
        public Vector2 maxBounding;
    }

    public struct ARCLNDMeshDataRef
    {
        public int id;
        public int count;
        public int offset;
    }

    public struct ARCLNDMeshData
    {
        public int BoundingData;
        public int int_04;
        public int lndEntry;
        public int int_0C;
        public int faceDataId;
    }
}
