using System.Collections.Generic;
using System.Numerics;

namespace AquaModelLibrary.Extra.Ninja.BillyHatcher.LNDH
{
    public struct ARCLNDHeader
    {
        public int nextDataOffset;
        public int extraModelCount;
        public int extraModelOffsetsOffset;
        public int mpbFileOffset;    //Often 0

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
        /// <summary>
        /// The animated model data should only be defined for the Block model.
        /// </summary>
        public int animatedModelSetCount;
        public int animatedModelSetOffset;

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

    /// <summary>
    /// The data here may allow for substituing all vertex data, but faces and collision would remain the same so this wouldn't be super useful. Maybe UV data would.
    /// In retail, only the first vert color set is used
    /// Should 
    /// </summary>
    public class ARCLNDAltVertColorInfo
    {
        public ushort vertPositionUnk;
        public ushort vertPositionCount;
        public int vertPositionOffset;
        public ushort vertNormalUnk;
        public ushort vertNormalCount;
        public int vertNormalOffset;

        public ushort vertColorUnk;
        public ushort vertColorCount;
        public int vertColorOffset;
        public ushort vertColor2Unk;
        public ushort vertColor2Count;
        public int vertColor2Offset;

        public ushort uv1Unk;
        public ushort uv1Count;
        public int uv1Offset;
        public ushort uv2Unk;
        public ushort uv2Count;
        public int uv2Offset;

        public List<Vector3> PositionData = new List<Vector3>();
        public List<Vector3> NormalData = new List<Vector3>();
        public List<byte[]> vertColors = new List<byte[]>();
        public List<byte[]> vertColor2s = new List<byte[]>();
        public List<short[]> UV1Data = new List<short[]>();
        public List<short[]> UV2Data = new List<short[]>();
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

    public class ARCLNDLandEntryRef
    {
        public int unkInt;
        public int offset;

        public ARCLNDLandEntry entry = null;
    }

    public class ARCLNDLandEntry
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


        public byte[] GetVertDataBytes(int offset, out List<int> offsets)
        {
            offsets = new List<int>();
            List<byte> outBytes = new List<byte>();
            outBytes.AddValue((ushort)1);
            outBytes.AddValue((ushort)PositionData.Count);
            offsets.Add(outBytes.Count + offset);
            outBytes.ReserveInt("PositionOffset");
            outBytes.AddValue((ushort)3);
            outBytes.AddValue((ushort)NormalData.Count);
            offsets.Add(outBytes.Count + offset);
            outBytes.ReserveInt("NormalOffset");
            outBytes.AddValue((ushort)2);
            outBytes.AddValue((ushort)VertColorData.Count);
            offsets.Add(outBytes.Count + offset);
            outBytes.ReserveInt("VertColorDataOffset");
            outBytes.AddValue((ushort)2);
            outBytes.AddValue((ushort)VertColor2Data.Count);
            offsets.Add(outBytes.Count + offset);
            outBytes.ReserveInt("VertColor2DataOffset");
            outBytes.AddValue((ushort)1);
            outBytes.AddValue((ushort)UV1Data.Count);
            offsets.Add(outBytes.Count + offset);
            outBytes.ReserveInt("UV1DataOffset");
            outBytes.AddValue((ushort)1);
            outBytes.AddValue((ushort)UV2Data.Count);
            offsets.Add(outBytes.Count + offset);
            outBytes.ReserveInt("UV2DataOffset");

            outBytes.FillInt("PositionOffset", outBytes.Count + offset);
            for (int i = 0; i < PositionData.Count; i++)
            {
                var pos = PositionData[i];
                outBytes.AddValue(pos.X);
                outBytes.AddValue(pos.Y);
                outBytes.AddValue(pos.Z);
            }
            outBytes.FillInt("NormalOffset", outBytes.Count + offset);
            for (int i = 0; i < NormalData.Count; i++)
            {
                var nrm = NormalData[i];
                outBytes.AddValue(nrm.X);
                outBytes.AddValue(nrm.Y);
                outBytes.AddValue(nrm.Z);
            }
            outBytes.FillInt("VertColorDataOffset", outBytes.Count + offset);
            for (int i = 0; i < VertColorData.Count; i++)
            {
                outBytes.AddRange(VertColorData[i]);
            }
            outBytes.FillInt("VertColor2DataOffset", outBytes.Count + offset);
            for (int i = 0; i < VertColor2Data.Count; i++)
            {
                outBytes.AddRange(VertColor2Data[i]);
            }
            outBytes.FillInt("UV1DataOffset", outBytes.Count + offset);
            for (int i = 0; i < UV1Data.Count; i++)
            {
                var uv1 = UV1Data[i];
                outBytes.AddValue(uv1[0]);
                outBytes.AddValue(uv1[1]);
            }
            outBytes.FillInt("UV2DataOffset", outBytes.Count + offset);
            for (int i = 0; i < UV2Data.Count; i++)
            {
                var uv2 = UV2Data[i];
                outBytes.AddValue(uv2[0]);
                outBytes.AddValue(uv2[1]);
            }

            return outBytes.ToArray();
        }
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
        /// <summary>
        /// This isn't always used, for unknown reasons
        /// </summary>
        public Vector3 Position;
        public short sht0;
        public short sht1;
        public short BAMS0;
        public short BAMS1;
        public short BAMS2;
        public short BAMS3;
        /// <summary>
        /// This isn't always used, for unknown reasons
        /// </summary>
        public Vector3 scale; 

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
        public int int_04;   //int_04 or int_0c is probably a vertex set. If so, it may be important to test since vertex sets cap out at either short.Max or ushort.Max
        public int lndEntry;
        public int int_0C;
        public int faceDataId;
    }

    /// <summary>
    /// When placed, these should align to 0x20
    /// </summary>
    public struct ARCLNDAnimatedMeshRefSet
    {
        public int modelOffset;
        public int motionOffset;
        public int MPLAnimId;
    }
}
