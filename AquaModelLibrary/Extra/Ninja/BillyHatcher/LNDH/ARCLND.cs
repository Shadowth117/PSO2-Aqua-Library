using System;
using System.Collections.Generic;
using System.Linq;
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
        public List<ARCLNDMaterialEntryRef> arcMatEntryList = new List<ARCLNDMaterialEntryRef>();
        public List<ARCLNDVertDataRef> arcVertDataRefList = new List<ARCLNDVertDataRef>();
        public List<ARCLNDVertDataSet> arcVertDataSetList = new List<ARCLNDVertDataSet>();
        public List<ARCLNDFaceDataRef> arcFaceDataRefList = new List<ARCLNDFaceDataRef>();
        public List<ARCLNDFaceDataHead> arcFaceDataList = new List<ARCLNDFaceDataHead>();
        public List<ARCLNDNodeBounding> arcBoundingList = new List<ARCLNDNodeBounding>();
        /// <summary>
        /// Mesh array 0 seems to be opaque, 1 and 2 appear to be alpha testing. No notable performance changes from putting these all into the same list, nor visual differences. Likely a debug or vestigial thing.
        /// </summary>
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
                if (arcAltVertColorList.Count > 0)
                {
                    offsets.Add(outBytes.Count + offset);
                }
                outBytes.ReserveInt("AltVertColorOffset");
                outBytes.AddValue(arcLndAnimatedMeshDataList.Count);
                if (arcLndAnimatedMeshDataList.Count > 0)
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
            outBytes.AddValue(arcMatEntryList.Count);
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
            for (int i = 0; i < arcMatEntryList.Count; i++)
            {
                var landRef = arcMatEntryList[i];

                //This value seems like it's sometimes 0 for the first land entry in the model, but is 1 otherwise. For some models, it is always 1.
                outBytes.AddValue(landRef.extraDataEnabled);
                offsets.Add(outBytes.Count + offset);
                outBytes.ReserveInt($"LandEntry{i}");
            }
            for (int i = 0; i < arcMatEntryList.Count; i++)
            {
                outBytes.FillInt($"LandEntry{i}", outBytes.Count + offset);
                var landRef = arcMatEntryList[i];
                outBytes.AddValue((int)landRef.entry.RenderFlags);
                outBytes.AddValue((int)landRef.entry.diffuseColor);
                outBytes.AddValue((int)landRef.entry.specularColor);
                outBytes.AddValue((int)landRef.entry.unkBool);
                outBytes.AddValue((int)landRef.entry.sourceAlpha);
                outBytes.AddValue((int)landRef.entry.destinationAlpha);
                outBytes.AddValue((int)landRef.entry.unkInt6);
                outBytes.AddValue((int)landRef.entry.unkFlags1);

                if (landRef.extraDataEnabled > 0)
                {
                    outBytes.AddValue((ushort)landRef.entry.textureFlags);
                    outBytes.AddValue((ushort)landRef.entry.ushort0);
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
                    outBytes.AddValue(meshData.BoundingDataId);
                    outBytes.AddValue(meshData.int_04);
                    outBytes.AddValue(meshData.matEntryId);
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

    public class ARCLNDMaterialEntryRef
    {
        public int extraDataEnabled;
        public int offset;

        public ARCLNDMaterialEntry entry = null;
    }

    /// <summary>
    /// It's not super clear what a lot of the unknown stuff does which thankfully means that defaulting them to what we know are valid valeus for the game will probably be enough.
    /// </summary>
    public class ARCLNDMaterialEntry
    {
        /// <summary>
        /// 3 is one of the more common values for this.
        /// </summary>
        public ARCLNDRenderFlags RenderFlags = (ARCLNDRenderFlags)0x3;
        /// <summary>
        /// Always pure white, 0xFFFFFFFF in retail. Color is overriden by vertex colors.
        /// </summary>
        public int diffuseColor = -1;
        /// <summary>
        /// Unclear if this is used
        /// </summary>
        public int specularColor = 0;
        public int unkBool = 0;

        public AlphaInstruction sourceAlpha = AlphaInstruction.SourceAlpha;
        public AlphaInstruction destinationAlpha = AlphaInstruction.DestinationAlpha;
        /// <summary>
        /// Always 3 in retail
        /// </summary>
        public int unkInt6 = 0x3;
        public int unkFlags1 = 0x0;

        /// <summary>
        /// Ushort flags? Really not clear...
        /// </summary>
        public ARCLNDTextureFlags textureFlags = ARCLNDTextureFlags.TileX | ARCLNDTextureFlags.TileY;
        /// <summary>
        /// Either 0 or 0x100
        /// </summary>
        public ushort ushort0 = 0x100;
        public int TextureId = -1;
    }

    [Flags]
    public enum ARCLNDRenderFlags
    {
        None = 0x0,
        EnableLighting = 0x1,
        RFUnknown0x2 = 0x2,
        TwoSided = 0x4,
        RFUnknown0x8 = 0x8,
        /// <summary>
        /// This breaks things a bunch. Not sure how it works, but seems like it messes with render order or something.
        /// </summary>
        renderOrderThing = 0x10,
        renderOrderThing2 = 0x20,
        RFUnknown0x40 = 0x40,
        RFUnknown0x80 = 0x80,
    }

    [Flags]
    public enum ARCLNDTextureFlags : ushort
    {
        None = 0x0,
        TFUnknownX0x1 = 0x1,
        TileX = 0x2,
        MirroredTileX = 0x4,
        TFUnknownY0x8 = 0x8,
        TileY = 0x10,
        MirroredTileY = 0x20,
        TFUnknown0x40 = 0x40,
        TFUnknown0x80 = 0x80,
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
        /// Note that normals and vert colors cannot coexist!
        /// </summary>
        public List<Vector3> NormalData = new List<Vector3>();
        /// <summary>
        /// RGBA order
        /// </summary>
        public List<byte[]> VertColorData = new List<byte[]>();
        public List<byte[]> VertColor2Data = new List<byte[]>();
        public List<short[]> UV1Data = new List<short[]>();
        public List<short[]> UV2Data = new List<short[]>();

        //Helper data - Auto created
        public Dictionary<int, Vector3> faceNormalDict = new Dictionary<int, Vector3>();

        public void SetFaceNormals(int id0, int id1, int id2, Vector3 nrm)
        {
            SetFaceNormal(id0, nrm);
            SetFaceNormal(id1, nrm);
            SetFaceNormal(id2, nrm);
        }

        private void SetFaceNormal(int id, Vector3 nrm)
        {
            if (faceNormalDict.ContainsKey(id))
            {
                faceNormalDict[id] += nrm;
            }
            else
            {
                faceNormalDict[id] = nrm;
            }
        }

        public void NoramlizeFaceNormals()
        {
            var keys = faceNormalDict.Keys.ToArray();
            foreach (var key in keys)
            {
                faceNormalDict[key] = Vector3.Normalize(faceNormalDict[key]);
            }
        }

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

    /// <summary>
    /// Unlike many models of this era, data0 and data1 are not opaque and translucent. Instead, they were used to separate flipped and unflipped faces due to how sega used tristripping here.
    /// </summary>
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
        /// <summary>
        /// Some kind of epsilon or maybe a struct magic? Really unsure. Always the same.
        /// </summary>
        public float unkFlt_00 = 8.82818E-44f;
        /// <summary>
        /// Always 0
        /// </summary>
        public ushort usht_04;
        /// <summary>
        /// Always 65535
        /// </summary>
        public ushort usht_06 = ushort.MaxValue;
        /// <summary>
        /// Always 65535
        /// </summary>
        public ushort usht_08 = ushort.MaxValue;
        /// <summary>
        /// 1 based index for bounding data. Final bounding data entry will ALWAYS have 0xFFFF as the index, even if it's the only one.
        /// </summary>
        public ushort index = ushort.MaxValue;
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
        public int BoundingDataId;
        public int int_04;
        public int matEntryId;
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
