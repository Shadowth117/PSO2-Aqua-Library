using System.Collections.Generic;
using System.Numerics;
using static AquaModelLibrary.Extra.Ninja.BillyHatcher.LND;
using static AquaModelLibrary.Extra.Ninja.NinjaConstants;

namespace AquaModelLibrary.Extra.Ninja.BillyHatcher.LNDH
{

    public class ARCLNDModel
    {
        public static int counter = 0;

        public bool isAnimModel = false;
        public ARCLNDMainDataHeader arcMainDataHeader;
        public ARCLNDMainOffsetTable arcMainOffsetTable;
        public List<ARCLNDLandEntryRef> arcLandEntryList = new List<ARCLNDLandEntryRef>();
        public List<ARCLNDVertDataRef> arcVertDataRefList = new List<ARCLNDVertDataRef>();
        public List<ARCLNDVertDataSet> arcVertDataSetList = new List<ARCLNDVertDataSet>();
        public List<ARCLNDFaceDataRef> arcFaceDataRefList = new List<ARCLNDFaceDataRef>();
        public List<ARCLNDFaceDataHead> arcFaceDataList = new List<ARCLNDFaceDataHead>();
        public List<ARCLNDNodeBounding> arcBoundingList = new List<ARCLNDNodeBounding>();
        public List<ARCLNDMeshDataRef> arcMeshDataRefList = new List<ARCLNDMeshDataRef>();
        public List<List<ARCLNDMeshData>> arcMeshDataList = new List<List<ARCLNDMeshData>>();
        public ARCLNDAltVertColorRef arcAltVertRef;
        public List<ARCLNDAltVertColorMainRef> arcAltVertRefs = new List<ARCLNDAltVertColorMainRef>();
        public List<ARCLNDVertDataSet> arcAltVertColorList = new List<ARCLNDVertDataSet>();

        public byte[] GetBytes(int offset, List<ARCLNDAnimatedMeshData> arcLndAnimatedMeshDataList, out List<int> offsets)
        {
            offsets = new List<int>();
            List<byte> outBytes = new List<byte>();

            if (isAnimModel == false)
            {
                //Main offset table offset
                offsets.Add(outBytes.Count + offset);
                outBytes.AddValue(offset + 0x20);
                if(arcAltVertColorList.Count > 0)
                {
                    offsets.Add(outBytes.Count + offset);
                }
                outBytes.ReserveInt("AltVertColorOffset");
                outBytes.AddValue(arcLndAnimatedMeshDataList.Count);
                if(arcLndAnimatedMeshDataList.Count > 0)
                {
                    offsets.Add(outBytes.Count + offset);
                }
                outBytes.ReserveInt("AnimatedMeshOffsetsOffset");

                outBytes.AddValue((int)0);
                outBytes.AddValue((int)0);
                outBytes.AddValue((int)0);
                outBytes.AddValue((int)0);
            }

            //Main Offset Table
            outBytes.AddValue(arcLandEntryList.Count);
            offsets.Add(outBytes.Count + offset);
            outBytes.ReserveInt("LandEntryOffset");
            outBytes.AddValue(arcVertDataSetList.Count);
            offsets.Add(outBytes.Count + offset);
            outBytes.ReserveInt("VertDataOffset");

            outBytes.AddValue(arcFaceDataList.Count);
            offsets.Add(outBytes.Count + offset);
            outBytes.ReserveInt("FaceDataoffset");
            outBytes.AddValue(arcBoundingList.Count);
            offsets.Add(outBytes.Count + offset);
            outBytes.ReserveInt("BoundingOffset");

            outBytes.AddValue(1);
            outBytes.AddValue(arcMeshDataList.Count);
            offsets.Add(outBytes.Count + offset);
            outBytes.ReserveInt("MeshOffset");

            //Land Entries
            outBytes.FillInt($"LandEntryOffset", outBytes.Count + offset);
            for (int i = 0; i < arcLandEntryList.Count; i++)
            {
                var landRef = arcLandEntryList[i];

                //This value seems like it's sometimes 0 for the first land entry in the model, but is 1 otherwise. For some models, it is always 1.
                outBytes.AddValue(landRef.unkInt);
                offsets.Add(outBytes.Count + offset);
                outBytes.ReserveInt($"LandEntry{i}");
            }
            for (int i = 0; i < arcLandEntryList.Count; i++)
            {
                outBytes.FillInt($"LandEntry{i}", outBytes.Count + offset);
                var landRef = arcLandEntryList[i];
                outBytes.AddValue((int)landRef.entry.unkInt0);
                outBytes.AddValue((int)landRef.entry.unkInt1);
                outBytes.AddValue((int)landRef.entry.unkInt2);
                outBytes.AddValue((int)landRef.entry.unkInt3);
                outBytes.AddValue((int)landRef.entry.unkInt4);
                outBytes.AddValue((int)landRef.entry.unkInt5);
                outBytes.AddValue((int)landRef.entry.unkInt6);
                outBytes.AddValue((int)landRef.entry.unkInt7);

                if (landRef.unkInt > 0)
                {
                    outBytes.AddValue((ushort)landRef.entry.ushort0);
                    outBytes.AddValue((ushort)landRef.entry.ushort1);
                    outBytes.AddValue((int)landRef.entry.TextureId);
                }
            }

            //Vert data
            outBytes.FillInt($"VertDataOffset", outBytes.Count + offset);
            for (int i = 0; i < arcVertDataSetList.Count; i++)
            {
                outBytes.AddValue((int)0); //This is either i or just 0 every time. Probably the latter based on faces. Either way, retail has no example of how this should look.
                offsets.Add(outBytes.Count + offset);
                outBytes.ReserveInt($"VertData{i}");
            }
            for (int i = 0; i < arcVertDataSetList.Count; i++)
            {
                var vertInfo = arcVertDataSetList[i];
                outBytes.FillInt($"VertData{i}", outBytes.Count + offset);
                outBytes.AddRange(vertInfo.GetVertDataBytes(outBytes.Count + offset, out var verDataOffsets));
                offsets.AddRange(verDataOffsets);
            }

            //Face data
            outBytes.FillInt($"FaceDataoffset", outBytes.Count + offset);
            for (int i = 0; i < arcFaceDataList.Count; i++)
            {
                outBytes.AddValue((int)0);
                offsets.Add(outBytes.Count + offset);
                outBytes.ReserveInt($"FaceData{i}");
            }
            for (int i = 0; i < arcFaceDataList.Count; i++)
            {
                var faceData = arcFaceDataList[i];
                outBytes.FillInt($"FaceData{i}", outBytes.Count + offset);
                outBytes.AddValue((uint)faceData.flags);
                if (faceData.triIndicesList0.Count > 0)
                {
                    offsets.Add(outBytes.Count + offset);
                }
                outBytes.ReserveInt($"FaceDataOffset0{i}");
                outBytes.ReserveInt($"FaceDataBufferSize0{i}");
                if (faceData.triIndicesList1.Count > 0)
                {
                    offsets.Add(outBytes.Count + offset);
                }
                outBytes.ReserveInt($"FaceDataOffset1{i}");
                outBytes.ReserveInt($"FaceDataBufferSize1{i}");
            }
            outBytes.AlignWrite(0x20);

            for (int i = 0; i < arcFaceDataList.Count; i++)
            {
                var faceData = arcFaceDataList[i];
                //Write tridata
                if (faceData.triIndicesList0.Count > 0)
                {
                    outBytes.FillInt($"FaceDataOffset0{i}", outBytes.Count + offset);
                    var size = outBytes.Count;
                    for (int j = 0; j < faceData.triIndicesList0.Count; j++)
                    {
                        outBytes.Add((byte)faceData.triIndicesListStarts0[j][0][0]);
                        outBytes.AddValue((ushort)faceData.triIndicesListStarts0[j][0][1]);
                        var set = faceData.triIndicesList0[j];
                        for (int k = 0; k < set.Count; k++)
                        {
                            for (int l = 0; l < set[k].Count; l++)
                            {
                                outBytes.AddValue((ushort)set[k][l]);
                            }
                        }
                    }
                    outBytes.AlignWrite(0x20);
                    size = outBytes.Count - size;
                    outBytes.FillInt($"FaceDataBufferSize0{i}", size);
                }

                if (faceData.triIndicesList1.Count > 0)
                {
                    outBytes.FillInt($"FaceDataOffset1{i}", outBytes.Count + offset);
                    var size = outBytes.Count;
                    for (int j = 0; j < faceData.triIndicesList1.Count; j++)
                    {
                        outBytes.Add((byte)faceData.triIndicesListStarts1[j][0][0]);
                        outBytes.AddValue((ushort)faceData.triIndicesListStarts1[j][0][1]);
                        var set = faceData.triIndicesList1[j];
                        for (int k = 0; k < set.Count; k++)
                        {
                            for (int l = 0; l < set[k].Count; l++)
                            {
                                outBytes.AddValue((ushort)set[k][l]);
                            }
                        }
                    }
                    outBytes.AlignWrite(0x20);
                    size = outBytes.Count - size;
                    outBytes.FillInt($"FaceDataBufferSize1{i}", size);
                }
            }
            //Bounding data
            outBytes.FillInt($"BoundingOffset", outBytes.Count + offset);
            for (int i = 0; i < arcBoundingList.Count; i++)
            {
                var bounds = arcBoundingList[i];
                outBytes.AddValue(bounds.unkFlt_00);
                outBytes.AddValue(bounds.usht_04);
                outBytes.AddValue(bounds.usht_06);
                outBytes.AddValue(bounds.usht_08);
                outBytes.AddValue(bounds.index);
                outBytes.AddValue(bounds.Position.X);
                outBytes.AddValue(bounds.Position.Y);
                outBytes.AddValue(bounds.Position.Z);
                outBytes.AddValue(bounds.BAMSX);
                outBytes.AddValue(bounds.BAMSY);
                outBytes.AddValue(bounds.BAMSZ);
                outBytes.AddValue(bounds.Scale.X);
                outBytes.AddValue(bounds.Scale.Y);
                outBytes.AddValue(bounds.Scale.Z);
                outBytes.AddValue(bounds.center.X);
                outBytes.AddValue(bounds.center.Y);
                outBytes.AddValue(bounds.center.Z);
                outBytes.AddValue(bounds.radius);
            }

            //Mesh data
            outBytes.FillInt($"MeshOffset", outBytes.Count + offset);
            for (int i = 0; i < arcMeshDataRefList.Count; i++)
            {
                var meshRef = arcMeshDataRefList[i];
                outBytes.AddValue(meshRef.unkEnum);
                outBytes.AddValue(arcMeshDataList[i].Count);
                offsets.Add(outBytes.Count + offset);
                outBytes.ReserveInt($"MeshGroup{i}Offset");
            }
            for (int i = 0; i < arcMeshDataList.Count; i++)
            {
                outBytes.FillInt($"MeshGroup{i}Offset", outBytes.Count + offset);
                foreach (var meshData in arcMeshDataList[i])
                {
                    outBytes.AddValue(meshData.BoundingData);
                    outBytes.AddValue(meshData.int_04);
                    outBytes.AddValue(meshData.lndEntry);
                    outBytes.AddValue(meshData.int_0C);
                    outBytes.AddValue(meshData.faceDataId);
                }
            }
            outBytes.AlignWrite(0x20);

            //Alt Vert data
            if (arcAltVertColorList.Count > 0)
            {
                outBytes.FillInt($"AltVertColorOffset", outBytes.Count + offset);
                outBytes.AddValue(arcAltVertColorList.Count);
                offsets.Add(outBytes.Count + offset);
                outBytes.ReserveInt("AltVertColorRefOffset");

                outBytes.FillInt("AltVertColorRefOffset", outBytes.Count + offset);
                for (int i = 0; i < arcAltVertColorList.Count; i++)
                {
                    outBytes.AddValue(i);
                    offsets.Add(outBytes.Count + offset);
                    outBytes.ReserveInt($"AltVerts{i}");
                }
                for (int i = 0; i < arcAltVertColorList.Count; i++)
                {
                    outBytes.FillInt($"AltVerts{i}", outBytes.Count + offset);
                    outBytes.AddRange(arcAltVertColorList[i].GetVertDataBytes(outBytes.Count + offset, out var verDataOffsets));
                    offsets.AddRange(verDataOffsets);
                }
            }

            //Animated Models
            if (arcLndAnimatedMeshDataList.Count > 0)
            {
                outBytes.FillInt($"AnimatedMeshOffsetsOffset", outBytes.Count + offset);
                for (int i = 0; i < arcLndAnimatedMeshDataList.Count; i++)
                {
                    offsets.Add(outBytes.Count + offset);
                    outBytes.ReserveInt($"AnimatedModel{i}");
                    offsets.Add(outBytes.Count + offset);
                    outBytes.ReserveInt($"AnimatedMotion{i}");
                    outBytes.AddValue(arcLndAnimatedMeshDataList[i].MPLAnimId);
                }
                outBytes.AlignWrite(0x20);
                for (int i = 0; i < arcLndAnimatedMeshDataList.Count; i++)
                {
                    outBytes.FillInt($"AnimatedModel{i}", outBytes.Count + offset);
                    outBytes.AddRange(arcLndAnimatedMeshDataList[i].model.GetBytes(outBytes.Count + offset, new List<ARCLNDAnimatedMeshData>(), out var animModelOffsets));
                    offsets.AddRange(animModelOffsets);
                    outBytes.FillInt($"AnimatedMotion{i}", outBytes.Count + offset);
                    outBytes.AddRange(arcLndAnimatedMeshDataList[i].motion.GetBytes(outBytes.Count + offset, out var animOffsets));
                    offsets.AddRange(animOffsets);
                    outBytes.AlignWrite(0x20);
                }
            }

            return outBytes.ToArray();
        }

    }

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
        /// <summary>
        /// Normals seem like they might be laid out in some kind of crazy normal, tangent, binormal order or something, like a normal specific tristripping, 
        /// but most models also don't use them at all, and are probably using some kind of face normal averaging. Custom models should experiment with that. 
        /// </summary>
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

            if (PositionData.Count > 0)
            {
                offsets.Add(outBytes.Count + offset);
            }
            outBytes.ReserveInt("PositionOffset");
            outBytes.AddValue((ushort)3);
            outBytes.AddValue((ushort)NormalData.Count);
            if (NormalData.Count > 0)
            {
                offsets.Add(outBytes.Count + offset);
            }
            outBytes.ReserveInt("NormalOffset");
            outBytes.AddValue((ushort)2);
            outBytes.AddValue((ushort)VertColorData.Count);
            if (VertColorData.Count > 0)
            {
                offsets.Add(outBytes.Count + offset);
            }
            outBytes.ReserveInt("VertColorDataOffset");
            outBytes.AddValue((ushort)2);
            outBytes.AddValue((ushort)VertColor2Data.Count);
            if (VertColor2Data.Count > 0)
            {
                offsets.Add(outBytes.Count + offset);
            }
            outBytes.ReserveInt("VertColor2DataOffset");
            outBytes.AddValue((ushort)1);
            outBytes.AddValue((ushort)UV1Data.Count);
            if (UV1Data.Count > 0)
            {
                offsets.Add(outBytes.Count + offset);
            }
            outBytes.ReserveInt("UV1DataOffset");
            outBytes.AddValue((ushort)1);
            outBytes.AddValue((ushort)UV2Data.Count);
            if (UV2Data.Count > 0)
            {
                offsets.Add(outBytes.Count + offset);
            }
            outBytes.ReserveInt("UV2DataOffset");

            if (PositionData.Count > 0)
            {
                outBytes.FillInt("PositionOffset", outBytes.Count + offset);
                for (int i = 0; i < PositionData.Count; i++)
                {
                    var pos = PositionData[i];
                    outBytes.AddValue(pos.X);
                    outBytes.AddValue(pos.Y);
                    outBytes.AddValue(pos.Z);
                }
            }
            if (NormalData.Count > 0)
            {
                outBytes.FillInt("NormalOffset", outBytes.Count + offset);
                for (int i = 0; i < NormalData.Count; i++)
                {
                    var nrm = NormalData[i];
                    outBytes.AddValue(nrm.X);
                    outBytes.AddValue(nrm.Y);
                    outBytes.AddValue(nrm.Z);
                }
            }
            if (VertColorData.Count > 0)
            {
                outBytes.FillInt("VertColorDataOffset", outBytes.Count + offset);
                for (int i = 0; i < VertColorData.Count; i++)
                {
                    outBytes.AddRange(VertColorData[i]);
                }
            }
            if (VertColor2Data.Count > 0)
            {
                outBytes.FillInt("VertColor2DataOffset", outBytes.Count + offset);
                for (int i = 0; i < VertColor2Data.Count; i++)
                {
                    outBytes.AddRange(VertColor2Data[i]);
                }
            }
            if (UV1Data.Count > 0)
            {
                outBytes.FillInt("UV1DataOffset", outBytes.Count + offset);
                for (int i = 0; i < UV1Data.Count; i++)
                {
                    var uv1 = UV1Data[i];
                    outBytes.AddValue(uv1[0]);
                    outBytes.AddValue(uv1[1]);
                }
            }
            if (UV2Data.Count > 0)
            {
                outBytes.FillInt("UV2DataOffset", outBytes.Count + offset);
                for (int i = 0; i < UV2Data.Count; i++)
                {
                    var uv2 = UV2Data[i];
                    outBytes.AddValue(uv2[0]);
                    outBytes.AddValue(uv2[1]);
                }
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

    public class ARCLNDNodeBounding
    {
        public float unkFlt_00;
        /// <summary>
        /// Always 0
        /// </summary>
        public ushort usht_04;
        /// <summary>
        /// Always 65535
        /// </summary>
        public ushort usht_06;
        /// <summary>
        /// Always 65535
        /// </summary>
        public ushort usht_08;
        /// <summary>
        /// 1 based index for bounding data. Final bounding data entry will ALWAYS have 0xFFFF as the index, even if it's the only one.
        /// </summary>
        public ushort index;
        /// <summary>
        /// This isn't always used, for unknown reasons
        /// </summary>
        public Vector3 Position;
        /// <summary>
        /// X Rotation in Binary Angle Measurement System
        /// </summary>
        public int BAMSX;
        /// <summary>
        /// Y Rotation in Binary Angle Measurement System
        /// </summary>
        public int BAMSY;
        /// <summary>
        /// Z Rotation in Binary Angle Measurement System
        /// </summary>
        public int BAMSZ;
        /// <summary>
        /// This isn't always used, for unknown reasons
        /// </summary>
        public Vector3 Scale;

        /// <summary>
        /// Bounding sphere data
        /// </summary>
        public Vector3 center;
        public float radius;

        public Vector3 GetRotation()
        {
            return new Vector3((float)(BAMSX * FromBAMSvalue), (float)(BAMSY * FromBAMSvalue), (float)(BAMSZ * FromBAMSvalue));
        }

        public void SetRotation(Vector3 radianEulerRot)
        {
            BAMSX = (short)(radianEulerRot.X * ToBAMSValue);
            BAMSY = (short)(radianEulerRot.Y * ToBAMSValue);
            BAMSZ = (short)(radianEulerRot.Z * ToBAMSValue);
        }
    }

    public class ARCLNDMeshDataRef
    {
        public int unkEnum;
        public int count;
        public int offset;
    }

    public class ARCLNDMeshData
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
    public class ARCLNDAnimatedMeshRefSet
    {
        public int modelOffset;
        public int motionOffset;
        public int MPLAnimId;
    }
}
