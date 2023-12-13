using AquaModelLibrary.Extra.Ninja.BillyHatcher.LNDH;
using Reloaded.Memory.Streams;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text;
using static AquaModelLibrary.Extra.Ninja.BillyHatcher.ARC;

namespace AquaModelLibrary.Extra.Ninja.BillyHatcher
{
    public class LND
    {
        public byte[] gvmBytes = null;
        public List<uint> pof0Offsets = new List<uint>();
        /// <summary>
        /// Model texture references. All model textures map to this and so use its order ids. The GVM textures are then referenced by the texture names here.
        /// Essentially, order here doesn't need to match the GVM order. Howevever all models must go off the order in this list.
        /// </summary>
        public List<string> texnames = new List<string>();
        /// <summary>
        /// The names of 'files' within the lnd. These will always be the main model followed by non animated supplemental models and finally the land definition and the mpl, if it exists.
        /// </summary>
        public List<string> fileNames = new List<string>();

        public bool isArcLND = false;

        //ARCLND Data
        public ARCHeader arcHeader;
        public List<ARCFileRef> arcLndModelRefs = new List<ARCFileRef>();
        public List<ARCLNDStaticMeshData> arcLndModels = new List<ARCLNDStaticMeshData>();
        public List<ARCLNDAnimatedMeshData> arcLndAnimatedMeshDataList = new List<ARCLNDAnimatedMeshData>();
        /// <summary>
        ///Animated models. All models here can have 2 animations. An 'animation' that only contains vertex color data for night and a more typical animation.
        ///The former is not always there in retail, however technically the other animation doesn't need to be there either.
        ///These models typically have a transform associated with them, unlike the normal models.
        /// </summary>
        public List<ARCLNDAnimatedMeshRefSet> arcLndAnimatedModelRefs = new List<ARCLNDAnimatedMeshRefSet>();
        public ARCLNDLand arcLand = null;
        public MPL arcMPL = null;

        public class ARCLNDStaticMeshData
        {
            public string name = null;
            public ARCLNDModel model = null;
        }

        public class ARCLNDAnimatedMeshData
        {
            public ARCLNDModel model = null;
            public Motion motion = null;
            public int MPLAnimId = -1;
            public MPLMotionStart mplMotion = null;
        }

        public class ARCLNDLand
        {
            public ARCLNDHeader arcLndHeader;
            public List<int> arcExtraModeloffsets = new List<int>();
            public ARCLNDRefTableHead arcRefTable;
            public List<ARCLNDRefEntry> arcRefTableEntries = new List<ARCLNDRefEntry>();

            public byte[] GetBytes(int offset, int extraModelCount, List<string> texNames, out List<int> offsets)
            {
                offsets = new List<int>();
                List<byte> outBytes = new List<byte>();
                offsets.Add(outBytes.Count + offset);
                outBytes.ReserveInt("MainModelOffset");
                outBytes.AddValue(extraModelCount);
                offsets.Add(outBytes.Count + offset);
                outBytes.ReserveInt("ExtraModelOffsetsOffset");
                offsets.Add(outBytes.Count + offset);
                outBytes.ReserveInt("MPBOffset");

                offsets.Add(outBytes.Count + offset);
                outBytes.ReserveInt("TexListOffset");
                offsets.Add(outBytes.Count + offset);
                outBytes.ReserveInt("GVMOffset");

                if (extraModelCount > 0)
                {
                    outBytes.FillInt("ExtraModelOffsetsOffset", outBytes.Count);
                    for (int i = 0; i < extraModelCount; i++)
                    {
                        offsets.Add(outBytes.Count + offset);
                        outBytes.ReserveInt($"ExtraModel{i}Offset");
                    }
                }

                if (texNames.Count > 0)
                {
                    outBytes.FillInt("TexListOffset", outBytes.Count);
                    offsets.Add(outBytes.Count + offset);
                    outBytes.ReserveInt("TexListReferencesOffset");
                    outBytes.AddValue(texNames.Count);
                    outBytes.FillInt("TexListReferencesOffset", outBytes.Count);
                    for (int i = 0; i < texNames.Count; i++)
                    {
                        offsets.Add(outBytes.Count + offset);
                        outBytes.ReserveInt($"TexRef{i}");
                        outBytes.AddValue((int)0);
                        outBytes.AddValue((int)0);
                    }
                    for (int i = 0; i < texNames.Count; i++)
                    {
                        outBytes.FillInt($"TexRef{i}", outBytes.Count);
                        outBytes.AddRange(Encoding.UTF8.GetBytes(texNames[i]));
                        outBytes.Add(0);
                    }
                }

                return outBytes.ToArray();
            }
        }

        //LND Data
        public NinjaHeader nHeader;
        public LNDHeader header;
        public LNDHeader2 header2;
        public LNDNodeIdSet nodeIdSet;
        public List<LandEntry> nodes = new List<LandEntry>();
        /// <summary>
        /// List of ids of meshInfos that have actual models
        /// </summary>
        public List<ushort> modelNodeIds = new List<ushort>();
        public List<uint> objectOffsets = new List<uint>();
        public List<LNDMeshInfo> meshInfo = new List<LNDMeshInfo>();
        public LNDTexDataEntryHead texDataEntryHead;
        public List<LNDTexDataEntry> texDataEntries = new List<LNDTexDataEntry>();
        public List<int> motionDataOffsets = new List<int>();
        public List<LNDMotionDataHead> motionDataHeadList = new List<LNDMotionDataHead>();
        public List<LNDMotionDataHead2> motionDataHead2List = new List<LNDMotionDataHead2>();
        public List<List<LNDMotionData>> motionDataList = new List<List<LNDMotionData>>();
        public LND() { }

        public LND(string filePath) {
            using (var stream = new MemoryStream(File.ReadAllBytes(filePath)))
            using (var sr = new BufferedStreamReader(stream, 8192))
            {
                Read(sr);
            }
        }

        public LND(byte[] fileBytes)
        {
            using (var stream = new MemoryStream(fileBytes))
            using (var sr = new BufferedStreamReader(stream, 8192))
            {
                Read(sr);
            }
        }

        public LND(BufferedStreamReader sr)
        {
            Read(sr);
        }

        public void Read(BufferedStreamReader sr)
        {
            BigEndianHelper._active = true;
            var magicTest = sr.ReadBytes(0, 3);

            if (magicTest[0] == 0x4C && magicTest[1] == 0x4E && magicTest[2] == 0x44)
            {
                ReadLND(sr);
                //This'll be POF0 or GVM
                if (sr.Peek<int>() == 0x30464F50)
                {
                    var pof0Header = sr.Read<NinjaHeader>();
                    var pofRaw = sr.ReadBytes(sr.Position() - 0x8, pof0Header.fileSize + 0x8);
                    pof0Offsets = POF0.GetPof0Offsets(pofRaw);
                    sr.Seek(pof0Header.fileSize, System.IO.SeekOrigin.Current);
                }
                if (sr.Peek<int>() == 0x484D5647)
                {
                    gvmBytes = GVMUtil.ReadGVMBytes(sr);
                }
            }
            else
            {
                isArcLND = true;
                //This is based more around the .arc format
                ReadARCLND(sr);
                gvmBytes = GVMUtil.ReadGVMBytes(sr, true);
            }
        }

        public void ReadARCLND(BufferedStreamReader sr)
        {
            //Generic ARC header
            arcHeader = new ARCHeader();
            arcHeader.fileSize = sr.ReadBE<int>();
            arcHeader.pof0Offset = sr.ReadBE<int>();
            arcHeader.pof0OffsetsSize = sr.ReadBE<int>();
            arcHeader.fileCount = sr.ReadBE<int>();

            arcHeader.unkCount = sr.ReadBE<int>();
            arcHeader.magic = sr.ReadBE<int>();
            arcHeader.unkInt0 = sr.ReadBE<int>();
            arcHeader.unkInt1 = sr.ReadBE<int>();

            //Get model references
            sr.Seek(0x20 + arcHeader.pof0Offset, System.IO.SeekOrigin.Begin);
            pof0Offsets = POF0.GetRawPOF0Offsets(sr.ReadBytes(sr.Position(), arcHeader.pof0OffsetsSize));
            sr.Seek(arcHeader.pof0OffsetsSize, System.IO.SeekOrigin.Current);
            for (int i = 0; i < arcHeader.fileCount; i++)
            {
                ARCFileRef modelRef = new ARCFileRef();
                modelRef.modelOffset = sr.ReadBE<int>();
                modelRef.relativeNameOffset = sr.ReadBE<int>();
                arcLndModelRefs.Add(modelRef);
            }

            //Get model names
            var nameStart = sr.Position();
            foreach (var modelRef in arcLndModelRefs)
            {
                sr.Seek(nameStart + modelRef.relativeNameOffset, System.IO.SeekOrigin.Begin);
                fileNames.Add(AquaMethods.AquaGeneralMethods.ReadCString(sr));
            }

            for (int mdl = 0; mdl < arcHeader.fileCount; mdl++)
            {
                sr.Seek(0x20 + arcLndModelRefs[mdl].modelOffset, System.IO.SeekOrigin.Begin);
                var fileName = fileNames[mdl];

                //In retail, lnds have Block (Always main level data), models named 'Sphere' that sometimes have trailing numbers, land, and mpl here.
                //2nd model is usually the day skybox, 3rd model is the night skybox
                switch (fileName)
                {
                    case "land":
                        arcLand = ReadARCLand(sr);
                        break;
                    case "mpl":
                        arcMPL = new MPL(sr);
                        break;
                    default:
                        arcLndModels.Add(new ARCLNDStaticMeshData() { name = fileName, model = ReadArcLndModel(sr) });
                        break;
                }
            }

            //Assign MPL Motions
            if (arcMPL != null)
            {
                for (int i = 0; i < arcLndAnimatedModelRefs.Count; i++)
                {
                    var id = arcLndAnimatedModelRefs[i].MPLAnimId;
                    if (arcMPL.motionDict.ContainsKey(id))
                    {
                        arcLndAnimatedMeshDataList[i].mplMotion = arcMPL.motionDict[id];
                    }
                }
            }

            sr.Seek(0x20 + arcLand.arcLndHeader.GVMOffset, System.IO.SeekOrigin.Begin);
        }

        private ARCLNDLand ReadARCLand(BufferedStreamReader sr)
        {
            ARCLNDLand arcLand = new ARCLNDLand();
            arcLand.arcLndHeader = new ARCLNDHeader();
            //Core ARCLND header
            arcLand.arcLndHeader.nextDataOffset = sr.ReadBE<int>();
            arcLand.arcLndHeader.extraModelCount = sr.ReadBE<int>();
            arcLand.arcLndHeader.extraModelOffsetsOffset = sr.ReadBE<int>();
            arcLand.arcLndHeader.mpbFileOffset = sr.ReadBE<int>(); //We'll read this separately since this gets another reference.

            arcLand.arcLndHeader.texRefTableOffset = sr.ReadBE<int>();
            arcLand.arcLndHeader.GVMOffset = sr.ReadBE<int>();

            if (arcLand.arcLndHeader.extraModelOffsetsOffset != 0)
            {
                sr.Seek(0x20 + arcLand.arcLndHeader.extraModelOffsetsOffset, System.IO.SeekOrigin.Begin);
                for (int i = 0; i < arcLand.arcLndHeader.extraModelCount; i++)
                {
                    arcLand.arcExtraModeloffsets.Add(sr.ReadBE<int>());
                }
            }

            //Read texture reference table
            sr.Seek(0x20 + arcLand.arcLndHeader.texRefTableOffset, System.IO.SeekOrigin.Begin);
            arcLand.arcRefTable = new ARCLNDRefTableHead();
            arcLand.arcRefTable.entryOffset = sr.ReadBE<int>();
            arcLand.arcRefTable.entryCount = sr.ReadBE<int>();

            sr.Seek(0x20 + arcLand.arcRefTable.entryOffset, System.IO.SeekOrigin.Begin);
            for (int i = 0; i < arcLand.arcRefTable.entryCount; i++)
            {
                ARCLNDRefEntry refEntry = new ARCLNDRefEntry();
                refEntry.textOffset = sr.ReadBE<int>();
                refEntry.unkInt0 = sr.ReadBE<int>();
                refEntry.unkInt1 = sr.ReadBE<int>();
                arcLand.arcRefTableEntries.Add(refEntry);
            }
            foreach (ARCLNDRefEntry entry in arcLand.arcRefTableEntries)
            {
                sr.Seek(entry.textOffset + 0x20, System.IO.SeekOrigin.Begin);
                texnames.Add(AquaMethods.AquaGeneralMethods.ReadCString(sr));
            }

            return arcLand;
        }

        private ARCLNDModel ReadArcLndModel(BufferedStreamReader sr, bool isAnimModel = false)
        {
            ARCLNDModel arcModel = new ARCLNDModel();
            arcModel.isAnimModel = isAnimModel;

            //Model stuff
            if (!isAnimModel)
            {
                arcModel.arcMainDataHeader = new ARCLNDMainDataHeader();
                arcModel.arcMainDataHeader.mainOffsetTableOffset = sr.ReadBE<int>();
                arcModel.arcMainDataHeader.altVertexColorOffset = sr.ReadBE<int>();
                arcModel.arcMainDataHeader.animatedModelSetCount = sr.ReadBE<int>();
                arcModel.arcMainDataHeader.animatedModelSetOffset = sr.ReadBE<int>();

                arcModel.arcMainDataHeader.unkInt_10 = sr.ReadBE<int>();
                arcModel.arcMainDataHeader.unkInt_14 = sr.ReadBE<int>();
                arcModel.arcMainDataHeader.unkInt_18 = sr.ReadBE<int>();
                arcModel.arcMainDataHeader.unkInt_1C = sr.ReadBE<int>();
            }

            arcModel.arcMainOffsetTable = new ARCLNDMainOffsetTable();
            arcModel.arcMainOffsetTable.landEntryCount = sr.ReadBE<int>();
            arcModel.arcMainOffsetTable.landEntryOffset = sr.ReadBE<int>();
            arcModel.arcMainOffsetTable.vertDataCount = sr.ReadBE<int>();
            arcModel.arcMainOffsetTable.vertDataOffset = sr.ReadBE<int>();

            arcModel.arcMainOffsetTable.faceSetsCount = sr.ReadBE<int>();
            arcModel.arcMainOffsetTable.faceSetsOffset = sr.ReadBE<int>();
            arcModel.arcMainOffsetTable.nodeBoundingCount = sr.ReadBE<int>();
            arcModel.arcMainOffsetTable.nodeBoundingOffset = sr.ReadBE<int>();

            arcModel.arcMainOffsetTable.unkCount = sr.ReadBE<int>(); //Always 1 in retail?
            arcModel.arcMainOffsetTable.meshDataCount = sr.ReadBE<int>();
            arcModel.arcMainOffsetTable.meshDataOffset = sr.ReadBE<int>();

            if (arcModel.arcMainDataHeader.animatedModelSetOffset != 0)
            {
                sr.Seek(0x20 + arcModel.arcMainDataHeader.animatedModelSetOffset, System.IO.SeekOrigin.Begin);
                for (int i = 0; i < arcModel.arcMainDataHeader.animatedModelSetCount; i++)
                {
                    ARCLNDAnimatedMeshRefSet set = new ARCLNDAnimatedMeshRefSet();
                    set.modelOffset = sr.ReadBE<int>();
                    set.motionOffset = sr.ReadBE<int>();
                    set.MPLAnimId = sr.ReadBE<int>();
                    arcLndAnimatedModelRefs.Add(set);
                }
                foreach (var set in arcLndAnimatedModelRefs)
                {
                    ARCLNDAnimatedMeshData meshData = new ARCLNDAnimatedMeshData();
                    meshData.MPLAnimId = set.MPLAnimId;
                    sr.Seek(0x20 + set.modelOffset, System.IO.SeekOrigin.Begin);
                    meshData.model = ReadArcLndModel(sr, true);
                    if (set.motionOffset != 0)
                    {
                        sr.Seek(0x20 + set.motionOffset, System.IO.SeekOrigin.Begin);
                        meshData.motion = new Motion(sr, 0x20);
                    }
                    arcLndAnimatedMeshDataList.Add(meshData);
                }
            }

            //Alt Vertex Colors
            if (arcModel.arcMainDataHeader.altVertexColorOffset > 0)
            {
                sr.Seek(0x20 + arcModel.arcMainDataHeader.altVertexColorOffset, System.IO.SeekOrigin.Begin);
                arcModel.arcAltVertRef = new ARCLNDAltVertColorRef();
                arcModel.arcAltVertRef.count = sr.ReadBE<int>();
                arcModel.arcAltVertRef.offset = sr.ReadBE<int>();
                sr.Seek(0x20 + arcModel.arcAltVertRef.offset, System.IO.SeekOrigin.Begin);
                for (int i = 0; i < arcModel.arcAltVertRef.count; i++)
                {
                    ARCLNDAltVertColorMainRef altVert = new ARCLNDAltVertColorMainRef();
                    altVert.id = sr.ReadBE<int>();
                    altVert.offset = sr.ReadBE<int>();
                    arcModel.arcAltVertRefs.Add(altVert);

                    var bookmark = sr.Position();
                    sr.Seek(0x20 + altVert.offset, System.IO.SeekOrigin.Begin);
                    ARCLNDVertDataSet arcVertDataSet = new ARCLNDVertDataSet();
                    ReadVertDataSet(sr, arcVertDataSet);
                    arcModel.arcAltVertColorList.Add(arcVertDataSet);

                    sr.Seek(bookmark, System.IO.SeekOrigin.Begin);
                }
            }

            //Land entries
            sr.Seek(0x20 + arcModel.arcMainOffsetTable.landEntryOffset, System.IO.SeekOrigin.Begin);
            for (int i = 0; i < arcModel.arcMainOffsetTable.landEntryCount; i++)
            {
                ARCLNDMaterialEntryRef lndRef = new ARCLNDMaterialEntryRef();
                lndRef.extraDataEnabled = sr.ReadBE<int>();
                lndRef.offset = sr.ReadBE<int>();
                arcModel.arcMatEntryList.Add(lndRef);
            }

            foreach (var matRef in arcModel.arcMatEntryList)
            {
                if (matRef.offset != 0)
                {
                    sr.Seek(0x20 + matRef.offset, System.IO.SeekOrigin.Begin);
                    ARCLNDMaterialEntry matEntry = new ARCLNDMaterialEntry();
                    matEntry.RenderFlags = (ARCLNDRenderFlags)sr.ReadBE<int>();
                    matEntry.diffuseColor = sr.ReadBE<int>();
                    matEntry.specularColor = sr.ReadBE<int>();
                    matEntry.unkBool = sr.ReadBE<int>();

                    matEntry.sourceAlpha = (AlphaInstruction)sr.ReadBE<int>();
                    matEntry.destinationAlpha = (AlphaInstruction)sr.ReadBE<int>();
                    matEntry.unkInt6 = sr.ReadBE<int>();
                    matEntry.unkFlags1 = sr.ReadBE<int>();

                    if (matRef.extraDataEnabled > 0)
                    {
                        matEntry.textureFlags = (ARCLNDTextureFlags)sr.ReadBE<ushort>();
                        matEntry.ushort0 = sr.ReadBE<ushort>();
                        matEntry.TextureId = sr.ReadBE<int>();
                    }
                    matRef.entry = matEntry;
                }
            }

            //Vertex data. Should only be one reference offset, but technically there could be more
            sr.Seek(0x20 + arcModel.arcMainOffsetTable.vertDataOffset, System.IO.SeekOrigin.Begin);
            for (int i = 0; i < arcModel.arcMainOffsetTable.vertDataCount; i++)
            {
                ARCLNDVertDataRef vertRef = new ARCLNDVertDataRef();
                vertRef.unkInt = sr.ReadBE<int>();
                vertRef.offset = sr.ReadBE<int>();
                arcModel.arcVertDataRefList.Add(vertRef);
            }
            foreach (var vertRef in arcModel.arcVertDataRefList)
            {
                sr.Seek(0x20 + vertRef.offset, System.IO.SeekOrigin.Begin);
                ARCLNDVertDataSet arcVertDataSet = new ARCLNDVertDataSet();
                ReadVertDataSet(sr, arcVertDataSet);
                arcModel.arcVertDataSetList.Add(arcVertDataSet);
            }

            //Read triangle data
            sr.Seek(0x20 + arcModel.arcMainOffsetTable.faceSetsOffset, System.IO.SeekOrigin.Begin);
            for (int i = 0; i < arcModel.arcMainOffsetTable.faceSetsCount; i++)
            {
                ARCLNDFaceDataRef faceRef = new ARCLNDFaceDataRef();
                faceRef.unkInt = sr.ReadBE<int>();
                faceRef.offset = sr.ReadBE<int>();
                arcModel.arcFaceDataRefList.Add(faceRef);
            }
            foreach (var faceRef in arcModel.arcFaceDataRefList)
            {
                sr.Seek(0x20 + faceRef.offset, System.IO.SeekOrigin.Begin);
                ARCLNDFaceDataHead faceDataHead = new ARCLNDFaceDataHead();
                faceDataHead.flags = sr.ReadBE<ArcLndVertType>();
                faceDataHead.faceDataOffset0 = sr.ReadBE<int>();
                faceDataHead.bufferSize0 = sr.ReadBE<int>();
                faceDataHead.faceDataOffset1 = sr.ReadBE<int>();
                faceDataHead.bufferSize1 = sr.ReadBE<int>();
                arcModel.arcFaceDataList.Add(faceDataHead);
            }

            //TriIndices
            foreach (var faceDataHead in arcModel.arcFaceDataList)
            {
                ReadArcLndTris(sr, faceDataHead.flags, faceDataHead.faceDataOffset0, faceDataHead.bufferSize0, out faceDataHead.triIndicesList0, out faceDataHead.triIndicesListStarts0);
                ReadArcLndTris(sr, faceDataHead.flags, faceDataHead.faceDataOffset1, faceDataHead.bufferSize1, out faceDataHead.triIndicesList1, out faceDataHead.triIndicesListStarts1);

                //Generate averaged face normals - The game seemingly does this at runtime so we need these to be able to replicate how the game handles normals;
                for(int i = 0; i < faceDataHead.triIndicesList0.Count; i++)
                {
                    for(int j = 0; j < faceDataHead.triIndicesList0[i].Count - 2; j++)
                    {
                        int id0 = (j & 1) > 0 ? faceDataHead.triIndicesList0[i][j + 2][0] : faceDataHead.triIndicesList0[i][j][0];
                        int id1 = faceDataHead.triIndicesList0[i][j + 1][0];
                        int id2 = (j & 1) > 0 ? faceDataHead.triIndicesList0[i][j][0] : faceDataHead.triIndicesList0[i][j + 2][0];
                        Vector3 n = Vector3.Normalize(Vector3.Cross(arcModel.arcVertDataSetList[0].PositionData[id2] - arcModel.arcVertDataSetList[0].PositionData[id0],
                            arcModel.arcVertDataSetList[0].PositionData[id1] - arcModel.arcVertDataSetList[0].PositionData[id0]));
                        arcModel.arcVertDataSetList[0].SetFaceNormals(id0, id1, id2, n);
                    }
                }
                for (int i = 0; i < faceDataHead.triIndicesList1.Count; i++)
                {
                    for (int j = 0; j < faceDataHead.triIndicesList1[i].Count - 2; j++)
                    {
                        int id0 = ((j + 1) & 1) > 0 ? faceDataHead.triIndicesList1[i][j + 2][0] : faceDataHead.triIndicesList1[i][j][0];
                        int id1 = faceDataHead.triIndicesList1[i][j + 1][0];
                        int id2 = ((j + 1) & 1) > 0 ? faceDataHead.triIndicesList1[i][j][0] : faceDataHead.triIndicesList1[i][j + 2][0];
                        Vector3 n = Vector3.Normalize(Vector3.Cross(arcModel.arcVertDataSetList[0].PositionData[id2] - arcModel.arcVertDataSetList[0].PositionData[id0],
                            arcModel.arcVertDataSetList[0].PositionData[id1] - arcModel.arcVertDataSetList[0].PositionData[id0]));
                        arcModel.arcVertDataSetList[0].SetFaceNormals(id0, id1, id2, n);
                    }
                }
                arcModel.arcVertDataSetList[0].NoramlizeFaceNormals();
            }

            //Node bounding
            sr.Seek(0x20 + arcModel.arcMainOffsetTable.nodeBoundingOffset, System.IO.SeekOrigin.Begin);
            for (int i = 0; i < arcModel.arcMainOffsetTable.nodeBoundingCount; i++)
            {
                ARCLNDNodeBounding bounding = new ARCLNDNodeBounding();
                bounding.unkFlt_00 = sr.ReadBE<float>();
                bounding.usht_04 = sr.ReadBE<ushort>();
                bounding.usht_06 = sr.ReadBE<ushort>();
                bounding.usht_08 = sr.ReadBE<ushort>();
                bounding.index = sr.ReadBE<ushort>();
                bounding.Position = sr.ReadBEV3();
                bounding.BAMSX = sr.ReadBE<int>();
                bounding.BAMSY = sr.ReadBE<int>();
                bounding.BAMSZ = sr.ReadBE<int>();
                bounding.Scale = sr.ReadBEV3();
                bounding.center = sr.ReadBEV3();
                bounding.radius = sr.ReadBE<float>();
                arcModel.arcBoundingList.Add(bounding);
            }

            //Mesh data
            sr.Seek(0x20 + arcModel.arcMainOffsetTable.meshDataOffset, System.IO.SeekOrigin.Begin);
            for (int i = 0; i < arcModel.arcMainOffsetTable.meshDataCount; i++)
            {
                ARCLNDMeshDataRef meshDataRef = new ARCLNDMeshDataRef();
                meshDataRef.unkEnum = sr.ReadBE<int>();
                meshDataRef.count = sr.ReadBE<int>();
                meshDataRef.offset = sr.ReadBE<int>();
                arcModel.arcMeshDataRefList.Add(meshDataRef);
            }
            foreach (var datMeshaRef in arcModel.arcMeshDataRefList)
            {
                sr.Seek(0x20 + datMeshaRef.offset, System.IO.SeekOrigin.Begin);
                List<ARCLNDMeshData> meshDataList = new List<ARCLNDMeshData>();
                for (int i = 0; i < datMeshaRef.count; i++)
                {
                    ARCLNDMeshData meshData = new ARCLNDMeshData();
                    meshData.BoundingDataId = sr.ReadBE<int>();
                    meshData.int_04 = sr.ReadBE<int>();
                    meshData.matEntryId = sr.ReadBE<int>();
                    meshData.int_0C = sr.ReadBE<int>();
                    meshData.faceDataId = sr.ReadBE<int>();
                    meshDataList.Add(meshData);
                }
                arcModel.arcMeshDataList.Add(meshDataList);
            }

            return arcModel;
        }

        private static void ReadVertDataSet(BufferedStreamReader sr, ARCLNDVertDataSet arcVertDataSet)
        {
            for (int j = 0; j < 6; j++)
            {
                ARCLNDVertData vertInfo = new ARCLNDVertData();
                vertInfo.type = sr.ReadBE<ushort>();
                vertInfo.count = sr.ReadBE<ushort>();
                vertInfo.offset = sr.ReadBE<int>();
                switch (j)
                {
                    case 0:
                        arcVertDataSet.Position = vertInfo;
                        break;
                    case 1:
                        arcVertDataSet.Normal = vertInfo;
                        break;
                    case 2:
                        arcVertDataSet.VertColor = vertInfo;
                        break;
                    case 3:
                        arcVertDataSet.VertColor2 = vertInfo;
                        break;
                    case 4:
                        arcVertDataSet.UV1 = vertInfo;
                        break;
                    case 5:
                        arcVertDataSet.UV2 = vertInfo;
                        break;
                }
            }
            sr.Seek(0x20 + arcVertDataSet.Position.offset, System.IO.SeekOrigin.Begin);
            for (int j = 0; j < arcVertDataSet.Position.count; j++)
            {
                arcVertDataSet.PositionData.Add(sr.ReadBEV3());
            }
            sr.Seek(0x20 + arcVertDataSet.Normal.offset, System.IO.SeekOrigin.Begin);
            for (int j = 0; j < arcVertDataSet.Normal.count; j++)
            {
                arcVertDataSet.NormalData.Add(sr.ReadBEV3());
            }
            sr.Seek(0x20 + arcVertDataSet.VertColor.offset, System.IO.SeekOrigin.Begin);
            for (int j = 0; j < arcVertDataSet.VertColor.count; j++)
            {
                arcVertDataSet.VertColorData.Add(sr.ReadBytes(sr.Position(), 4));
                sr.Seek(4, System.IO.SeekOrigin.Current);
            }
            sr.Seek(0x20 + arcVertDataSet.VertColor2.offset, System.IO.SeekOrigin.Begin);
            for (int j = 0; j < arcVertDataSet.VertColor2.count; j++)
            {
                arcVertDataSet.VertColor2Data.Add(sr.ReadBytes(sr.Position(), 4));
                sr.Seek(4, System.IO.SeekOrigin.Current);
            }
            sr.Seek(0x20 + arcVertDataSet.UV1.offset, System.IO.SeekOrigin.Begin);
            for (int j = 0; j < arcVertDataSet.UV1.count; j++)
            {
                arcVertDataSet.UV1Data.Add(new short[] { sr.ReadBE<short>(), sr.ReadBE<short>() });
            }
            sr.Seek(0x20 + arcVertDataSet.UV2.offset, System.IO.SeekOrigin.Begin);
            for (int j = 0; j < arcVertDataSet.UV2.count; j++)
            {
                arcVertDataSet.UV2Data.Add(new short[] { sr.ReadBE<short>(), sr.ReadBE<short>() });
            }
        }

        private static void ReadArcLndTris(BufferedStreamReader sr, ArcLndVertType flags, int offset, int bufferSize, out List<List<List<int>>> triIndicesList, out List<List<List<int>>> triIndicesListStarts)
        {
            sr.Seek(offset + 0x20, System.IO.SeekOrigin.Begin);
            triIndicesList = new List<List<List<int>>>();
            triIndicesListStarts = new List<List<List<int>>>();
            while (sr.Position() < bufferSize + offset + 20)
            {
                var type = sr.Read<byte>();
                var count = sr.ReadBE<ushort>();

                if (type != 0x98 && type != 0x90 && type != 0)
                {
                    var pos = sr.Position();
                    throw new System.Exception();
                }
                if (type == 0)
                {
                    var pos = sr.Position();
                    break;
                }
                List<List<int>> triIndices = new List<List<int>>();
                List<List<int>> triIndicesStarts = new List<List<int>>();
                var starts = new List<int>
                {
                    type,
                    count
                };
                triIndicesStarts.Add(starts);
                for (int i = 0; i < count; i++)
                {
                    List<int> triIndex = new List<int>();
                    if ((flags & ArcLndVertType.Position) > 0)
                    {
                        triIndex.Add(sr.ReadBE<ushort>());
                    }
                    if ((flags & ArcLndVertType.Normal) > 0)
                    {
                        triIndex.Add(sr.ReadBE<ushort>());
                    }
                    if ((flags & ArcLndVertType.VertColor) > 0)
                    {
                        triIndex.Add(sr.ReadBE<ushort>());
                    }
                    if ((flags & ArcLndVertType.VertColor2) > 0)
                    {
                        triIndex.Add(sr.ReadBE<ushort>());
                    }
                    if ((flags & ArcLndVertType.UV1) > 0)
                    {
                        triIndex.Add(sr.ReadBE<ushort>());
                    }
                    if ((flags & ArcLndVertType.UV2) > 0)
                    {
                        triIndex.Add(sr.ReadBE<ushort>());
                    }
                    triIndices.Add(triIndex);
                }
                triIndicesList.Add(triIndices);
                triIndicesListStarts.Add(triIndicesStarts);
            }
        }

        /// <summary>
        /// This seems to be mainly for older LND archives. They have an actual LND magic unlike the more common type
        /// </summary>
        public void ReadLND(BufferedStreamReader sr)
        {
            nHeader = sr.Read<NinjaHeader>();
            header = new LNDHeader();
            header.lndHeader2Offset = sr.ReadBE<int>();
            header.nodeCount = sr.ReadBE<ushort>();
            header.motionDataCount = sr.ReadBE<ushort>();
            header.lndMeshInfoOffset = sr.ReadBE<int>();
            header.motionDataOffset = sr.ReadBE<int>();
            header.lndTexNameListOffset = sr.ReadBE<int>();

            //Motion data
            sr.Seek(header.motionDataOffset + 0x8, System.IO.SeekOrigin.Begin);
            for (int i = 0; i < header.motionDataCount; i++)
            {
                motionDataOffsets.Add(sr.ReadBE<int>());
            }
            foreach (var offset in motionDataOffsets)
            {
                sr.Seek(offset + 0x8, System.IO.SeekOrigin.Begin);
                LNDMotionDataHead head = new LNDMotionDataHead();
                head.lndMotionDataHead2Offset = sr.ReadBE<int>();
                head.frameAboveFinalFrame = sr.ReadBE<int>();
                head.keyType = sr.ReadBE<ushort>();
                head.dataType = sr.ReadBE<ushort>();
                motionDataHeadList.Add(head);
            }
            foreach (var motionHead in motionDataHeadList)
            {
                sr.Seek(motionHead.lndMotionDataHead2Offset + 0x8, System.IO.SeekOrigin.Begin);
                LNDMotionDataHead2 head = new LNDMotionDataHead2();
                head.dataOffset = sr.ReadBE<int>();
                head.unkInt = sr.ReadBE<int>();
                head.dataCount = sr.ReadBE<int>();
                motionDataHead2List.Add(head);
            }

            //TODO
            /*
            foreach (var motionHead in motionDataHead2List)
            {
                sr.Seek(motionHead.dataOffset + 0x8, System.IO.SeekOrigin.Begin);
                var motionData = new List<LNDMotionData>();
                for(int i = 0; i < motionHead.dataCount; i++)
                {
                    LNDMotionData data = new LNDMotionData();
                    data.frame = sr.ReadBE<int>();
                    switch()
                    {

                    }
                }
                motionDataList.Add(motionData);
            }*/

            //Tex name list
            if (header.lndTexNameListOffset > 0)
            {
                sr.Seek(header.lndTexNameListOffset + 0x8, System.IO.SeekOrigin.Begin);
                texDataEntryHead = new LNDTexDataEntryHead();
                texDataEntryHead.offset = sr.ReadBE<int>();
                texDataEntryHead.count = sr.ReadBE<ushort>();
                texDataEntryHead.texCount = sr.ReadBE<ushort>();

                sr.Seek(texDataEntryHead.offset + 0x8, System.IO.SeekOrigin.Begin);
                for (int i = 0; i < texDataEntryHead.texCount; i++)
                {
                    LNDTexDataEntry entry = new LNDTexDataEntry();
                    entry.offset = sr.ReadBE<int>();
                    entry.unk0 = sr.ReadBE<int>();
                    entry.unk1 = sr.ReadBE<int>();
                    texDataEntries.Add(entry);
                }
                foreach (LNDTexDataEntry entry in texDataEntries)
                {
                    sr.Seek(entry.offset + 0x8, System.IO.SeekOrigin.Begin);
                    texnames.Add(AquaMethods.AquaGeneralMethods.ReadCString(sr));
                }
            }

            //Node data
            sr.Seek(header.lndHeader2Offset + 0x8, System.IO.SeekOrigin.Begin);
            header2 = new LNDHeader2();
            header2.nodeCount = sr.ReadBE<ushort>();
            header2.usht02 = sr.ReadBE<ushort>();
            header2.nodesOffset = sr.ReadBE<int>();
            header2.int08 = sr.ReadBE<int>();
            header2.usht0C = sr.ReadBE<ushort>();
            header2.usht0E = sr.ReadBE<ushort>();
            header2.LNDNodeIdSetOffset = sr.ReadBE<int>();

            sr.Seek(header2.nodesOffset + 0x8, System.IO.SeekOrigin.Begin);
            for (int i = 0; i < header2.nodeCount; i++)
            {
                var node = new LandEntry();
                node.flag = (ContentFlag)sr.ReadBE<int>();
                node.objectIndex = sr.ReadBE<ushort>();
                node.motionIndex = sr.ReadBE<ushort>();
                node.minBounding = sr.ReadBEV2();
                node.maxBounding = sr.ReadBEV2();
                node.unkVec3 = sr.ReadBEV3();
                node.int24 = sr.ReadBE<int>();
                node.int28 = sr.ReadBE<int>();
                node.int2C = sr.ReadBE<int>();
                node.Scale = sr.ReadBEV3();
                node.int3C = sr.ReadBE<int>();
                nodes.Add(node);
            }

            sr.Seek(header2.LNDNodeIdSetOffset + 0x8, System.IO.SeekOrigin.Begin);
            nodeIdSet = new LNDNodeIdSet();
            nodeIdSet.nodeCount = sr.ReadBE<ushort>();
            nodeIdSet.usht02 = sr.ReadBE<ushort>();
            nodeIdSet.nodeIdsOffset = sr.ReadBE<int>();
            sr.Seek(nodeIdSet.nodeIdsOffset + 0x8, System.IO.SeekOrigin.Begin);
            for (int i = 0; i < nodeIdSet.nodeCount; i++)
            {
                modelNodeIds.Add(sr.ReadBE<ushort>());
            }

            //Mesh data
            sr.Seek(header.lndMeshInfoOffset + 0x8, System.IO.SeekOrigin.Begin);
            for (int i = 0; i < header.nodeCount; i++)
            {
                objectOffsets.Add(sr.ReadBE<uint>());
            }

            foreach (var offset in objectOffsets)
            {
                sr.Seek(offset + 0x8, System.IO.SeekOrigin.Begin);
                LNDMeshInfo lndMeshInfo = new LNDMeshInfo();
                lndMeshInfo.flags = sr.ReadBE<int>();
                lndMeshInfo.lndMeshInfo2Offset = sr.ReadBE<int>();
                lndMeshInfo.int08 = sr.ReadBE<int>();
                lndMeshInfo.int0C = sr.ReadBE<int>();

                lndMeshInfo.int10 = sr.ReadBE<int>();
                lndMeshInfo.int14 = sr.ReadBE<int>();
                lndMeshInfo.int18 = sr.ReadBE<int>();
                lndMeshInfo.int1C = sr.ReadBE<int>();

                lndMeshInfo.Scale = sr.ReadBEV3();
                lndMeshInfo.unkOffset0 = sr.ReadBE<int>();
                lndMeshInfo.unkOffset1 = sr.ReadBE<int>();
                lndMeshInfo.unkData = sr.ReadBE<int>();

                if (lndMeshInfo.lndMeshInfo2Offset > 0)
                {
                    sr.Seek(lndMeshInfo.lndMeshInfo2Offset + 0x8, System.IO.SeekOrigin.Begin);
                    LNDMeshInfo2 lndMeshInfo2 = new LNDMeshInfo2();
                    lndMeshInfo2.layoutsOffset = sr.ReadBE<int>();
                    lndMeshInfo2.unkOffset0 = sr.ReadBE<int>();
                    lndMeshInfo2.polyInfo0Offset = sr.ReadBE<int>();
                    lndMeshInfo2.polyInfo1Offset = sr.ReadBE<int>();

                    lndMeshInfo2.polyInfo0Count = sr.ReadBE<ushort>();
                    lndMeshInfo2.polyInfo1Count = sr.ReadBE<ushort>();
                    lndMeshInfo2.minBounding = sr.ReadBEV2();
                    lndMeshInfo2.maxBounding = sr.ReadBEV2();
                    lndMeshInfo.lndMeshInfo2 = lndMeshInfo2;

                    sr.Seek(lndMeshInfo2.layoutsOffset + 0x8, System.IO.SeekOrigin.Begin);
                    List<LNDVertLayout> layouts = new List<LNDVertLayout>();
                    var vertTypeCount = 0;
                    while (true) //not sure wtf defines the count here
                    {
                        LNDVertLayout lyt = new LNDVertLayout();
                        lyt.vertType = sr.Read<byte>();
                        if (lyt.vertType == 0xFF)
                        {
                            break;
                        }
                        vertTypeCount++;
                        lyt.dataType = sr.Read<byte>();
                        lyt.vertCount = sr.ReadBE<ushort>();
                        lyt.unkCount = sr.ReadBE<int>();
                        lyt.vertDataOffset = sr.ReadBE<int>();
                        lyt.vertDataBufferSize = sr.ReadBE<int>();
                        layouts.Add(lyt);
                    }
                    lndMeshInfo2.layouts = layouts;

                    //Vertex data
                    if (layouts.Count > 0)
                    {
                        lndMeshInfo2.vertData = new VertData();
                    }
                    foreach (var lyt in layouts)
                    {
                        sr.Seek(lyt.vertDataOffset + 0x8, System.IO.SeekOrigin.Begin);
                        for (int i = 0; i < lyt.vertCount; i++)
                        {
                            switch (lyt.vertType)
                            {
                                case 0x1:
                                    lndMeshInfo2.vertData.vertPositions.Add(sr.ReadBEV3());
                                    break;
                                case 0x2:
                                    lndMeshInfo2.vertData.vert2Data.Add(new byte[] { sr.Read<byte>(), sr.Read<byte>(), sr.Read<byte>() });
                                    break;
                                case 0x3:
                                    lndMeshInfo2.vertData.vertColorData.Add(sr.ReadBE<ushort>());
                                    break;
                                case 0x5:
                                    lndMeshInfo2.vertData.vertUVData.Add(new short[] { sr.ReadBE<short>(), sr.ReadBE<short>() });
                                    break;
                                default:
                                    throw new System.Exception($"Unk Vert type: {lyt.vertType:X} Data type: {lyt.dataType:X}");
                            }
                        }
                    }

                    //Polygon data
                    if (lndMeshInfo2.polyInfo0Offset != 0)
                    {
                        byte[] indexSizes = null;
                        sr.Seek(lndMeshInfo2.polyInfo0Offset + 0x8, System.IO.SeekOrigin.Begin);
                        for (int i = 0; i < lndMeshInfo2.polyInfo0Count; i++)
                        {
                            lndMeshInfo2.polyInfo0List.Add(ReadPolyInfo(sr, lndMeshInfo2, vertTypeCount, ref indexSizes));
                        }
                    }
                    if (lndMeshInfo2.polyInfo1Offset != 0)
                    {
                        byte[] indexSizes = null;
                        sr.Seek(lndMeshInfo2.polyInfo1Offset + 0x8, System.IO.SeekOrigin.Begin);
                        for (int i = 0; i < lndMeshInfo2.polyInfo1Count; i++)
                        {
                            lndMeshInfo2.polyInfo1List.Add(ReadPolyInfo(sr, lndMeshInfo2, vertTypeCount, ref indexSizes));
                        }
                    }
                }

                meshInfo.Add(lndMeshInfo);
            }

            //Seek for other data
            sr.Seek(nHeader.fileSize + 0x8, System.IO.SeekOrigin.Begin);
        }

        private static PolyInfo ReadPolyInfo(BufferedStreamReader sr, LNDMeshInfo2 lndMeshInfo2, int vertTypeCount, ref byte[] indexSizes)
        {
            PolyInfo polyInfo = new PolyInfo();
            polyInfo.materialOffset = sr.ReadBE<int>();
            polyInfo.unkCount = sr.ReadBE<ushort>();
            polyInfo.materialDataCount = sr.ReadBE<ushort>();
            polyInfo.polyDataOffset = sr.ReadBE<int>();
            polyInfo.polyDataBufferSize = sr.ReadBE<int>();
            var bookmark = sr.Position();
            ReadPolyData(sr, lndMeshInfo2, vertTypeCount, polyInfo, ref indexSizes);
            sr.Seek(bookmark, System.IO.SeekOrigin.Begin);
            return polyInfo;
        }

        private static void ReadPolyData(BufferedStreamReader sr, LNDMeshInfo2 lndMeshInfo2, int vertTypeCount, PolyInfo polyInfo, ref byte[] indexSizes)
        {
            //Material data
            if (polyInfo != null)
            {
                sr.Seek(polyInfo.materialOffset + 0x8, System.IO.SeekOrigin.Begin);
                List<MaterialInfo> matInfoList = new List<MaterialInfo>();
                for (int i = 0; i < polyInfo.materialDataCount; i++)
                {
                    MaterialInfo matInfo = new MaterialInfo();
                    matInfo.matInfoType = sr.ReadBE<int>();
                    matInfo.matData0 = sr.Read<byte>();
                    matInfo.matData1 = sr.Read<byte>();
                    matInfo.matData2 = sr.Read<byte>();
                    matInfo.matData3 = sr.Read<byte>();
                    matInfoList.Add(matInfo);

                    switch (matInfo.matInfoType)
                    {
                        case 0x09000000:
                            var mapping = new byte[8];
                            mapping[0] = (byte)(matInfo.matData3 % 0x10);
                            mapping[1] = (byte)(matInfo.matData3 / 0x10);
                            mapping[2] = (byte)(matInfo.matData2 % 0x10);
                            mapping[3] = (byte)(matInfo.matData2 / 0x10);
                            mapping[4] = (byte)(matInfo.matData1 % 0x10);
                            mapping[5] = (byte)(matInfo.matData1 / 0x10);
                            mapping[6] = (byte)(matInfo.matData0 % 0x10);
                            mapping[7] = (byte)(matInfo.matData0 / 0x10);

                            var tempDict = new Dictionary<int, int>();
                            for (int j = 0; j < mapping.Length; j++)
                            {
                                //It's likely only 8 maps to another vertex index, but just in case
                                if (mapping[j] != 0 && !tempDict.ContainsKey(mapping[j]))
                                {
                                    tempDict.Add(mapping[j], j);
                                    polyInfo.vertIndexMapping.Add(j, j);
                                }
                                else if (mapping[j] == 0) //0 only maps to itself
                                {
                                    polyInfo.vertIndexMapping.Add(j, j);
                                }
                                else
                                {
                                    polyInfo.vertIndexMapping.Add(j, tempDict[mapping[j]]);
                                }
                            }
                            break;
                        //Get the sizes of individual indices for the triangle definitions
                        case 0x01000000:
                            indexSizes = new byte[8];
                            indexSizes[0] = (byte)(matInfo.matData3 % 0x10);
                            indexSizes[1] = (byte)(matInfo.matData3 / 0x10);
                            indexSizes[2] = (byte)(matInfo.matData2 % 0x10);
                            indexSizes[3] = (byte)(matInfo.matData2 / 0x10);
                            indexSizes[4] = (byte)(matInfo.matData1 % 0x10);
                            indexSizes[5] = (byte)(matInfo.matData1 / 0x10);
                            indexSizes[6] = (byte)(matInfo.matData0 % 0x10);
                            indexSizes[7] = (byte)(matInfo.matData0 / 0x10);
                            break;
                    }
                }
                polyInfo.matInfo = matInfoList;
            }

            //Polygons
            if (polyInfo != null && polyInfo.polyDataOffset != 0)
            {
                sr.Seek(polyInfo.polyDataOffset + 0x8, System.IO.SeekOrigin.Begin);
                List<List<List<int>>> triIndicesList = new List<List<List<int>>>();
                List<List<List<int>>> triIndicesListStarts = new List<List<List<int>>>();
                while (sr.Position() < polyInfo.polyDataBufferSize + polyInfo.polyDataOffset + 8)
                {
                    var type = sr.Read<byte>();
                    var count = sr.ReadBE<ushort>();

                    if (type == 0)
                    {
                        break;
                    }
                    List<List<int>> triIndices = new List<List<int>>();
                    List<List<int>> triIndicesStarts = new List<List<int>>();
                    var starts = new List<int>
                    {
                        type,
                        count
                    };
                    triIndicesStarts.Add(starts);
                    for (int i = 0; i < count; i++)
                    {
                        List<int> triIndex = new List<int>();
                        for (int j = 0; j < vertTypeCount; j++)
                        {
                            var lyt = lndMeshInfo2.layouts[j];
                            if (indexSizes?.Length > j)
                            {
                                switch (indexSizes[j])
                                {
                                    case 0: //Skip
                                        break;
                                    case 0x2:
                                    case 0x8:
                                        triIndex.Add(sr.Read<byte>());
                                        break;
                                    case 0xC:
                                        triIndex.Add(sr.ReadBE<ushort>());
                                        break;
                                    default:
                                        throw new System.Exception();
                                }
                            }
                            else //Fallback for if for some godforsaken reason this doesn't exist
                            {
                                if (lyt.vertCount > 0xFF)
                                {
                                    triIndex.Add(sr.ReadBE<ushort>());
                                }
                                else
                                {
                                    triIndex.Add(sr.Read<byte>());
                                }
                            }
                        }
                        triIndices.Add(triIndex);
                    }
                    triIndicesList.Add(triIndices);
                    triIndicesListStarts.Add(triIndicesStarts);
                }
                polyInfo.triIndicesList = triIndicesList;
                polyInfo.triIndicesListStarts = triIndicesListStarts;
            }
        }

        /// <summary>
        /// For all variants, a .gvm with its contents should be stored as a byte array. Texture names should also be set outside it, though they should mirror the texnames in the GVM itself.
        /// For ARCLND, filenames should be populated, vertex data should be populated, strip data should be populated as well as LNDEntry data with an appropriate Mesh entry linking these. 
        /// </summary>
        public byte[] GetBytes()
        {
            if (isArcLND)
            {
                return GetBytesARCLND();
            }
            return GetBytesLNDH();
        }

        /// <summary>
        /// Not looking to do this right now since it's not really used much in final.
        /// </summary>
        private byte[] GetBytesLNDH()
        {
            return null;
        }

        private byte[] GetBytesARCLND()
        {
            ByteListExtension.AddAsBigEndian = true;
            List<byte> outBytes = new List<byte>();
            List<int> offsets = new List<int>();
            List<uint> fileOffsets = new List<uint>();
            uint lndOffset = 0;
            uint mplOffset = 0;

            //Write lnd
            outBytes.AddRange(arcLand.GetBytes(0, arcLndModels.Count - 1, texnames, out var lndOffsets));
            offsets.AddRange(lndOffsets);
            outBytes.AlignWrite(0x20);

            //Write GVM
            outBytes.FillInt("GVMOffset", outBytes.Count);
            outBytes.AddRange(gvmBytes);
            outBytes.AlignWrite(0x20);

            //Write models
            for (int i = 0; i < arcLndModels.Count; i++)
            {
                var modelSet = arcLndModels[i];
                List<ARCLNDAnimatedMeshData> animData = new List<ARCLNDAnimatedMeshData>();
                if (i == 0)
                {
                    animData = arcLndAnimatedMeshDataList;
                    outBytes.FillInt("MainModelOffset", outBytes.Count);
                }
                else
                {
                    outBytes.FillInt($"ExtraModel{i - 1}Offset", outBytes.Count);
                }
                fileOffsets.Add((uint)outBytes.Count);
                outBytes.AddRange(modelSet.model.GetBytes(outBytes.Count, animData, out var modelOffsets));
                outBytes.AlignWrite(0x20);
                offsets.AddRange(modelOffsets);
            }

            //Write mpl
            if (arcMPL != null)
            {
                outBytes.FillInt("MPBOffset", outBytes.Count);
                mplOffset = (uint)outBytes.Count;
                outBytes.AddRange(arcMPL.GetBytes(outBytes.Count, out var mplOffsets));
                outBytes.AlignWrite(0x4);
                offsets.AddRange(mplOffsets);
            }

            //Add offsets to list
            fileOffsets.Add(lndOffset);
            if (mplOffset != 0)
            {
                fileOffsets.Add(mplOffset);
            }

            //Write headerless POF0
            int pof0Offset = outBytes.Count;
            offsets.Sort();
            outBytes.AddRange(POF0.GenerateRawPOF0(offsets));
            int pof0End = outBytes.Count;
            int pof0Size = pof0End - pof0Offset;

            //Write ARC POF trailing data
            int relativeOffset = 0;
            for (int i = 0; i < fileOffsets.Count; i++)
            {
                var name = fileNames[i];
                var offset = fileOffsets[i];
                outBytes.AddValue(offset);
                outBytes.AddValue(relativeOffset);
                relativeOffset += name.Length + 1;
            }
            foreach (var name in fileNames)
            {
                outBytes.AddRange(Encoding.ASCII.GetBytes(name));
                outBytes.Add(0);
            }

            //ARC Header (insert at the end to make less messy)
            List<byte> arcHead = new List<byte>();
            arcHead.AddValue(outBytes.Count + 0x20);
            arcHead.AddValue(pof0Offset);
            arcHead.AddValue(pof0Size);
            arcHead.AddValue(fileNames.Count);

            arcHead.AddValue((int)0);
            arcHead.Add(0x30);
            arcHead.Add(0x31);
            arcHead.Add(0x30);
            arcHead.Add(0x30);
            arcHead.AddValue((int)0);
            arcHead.AddValue((int)0);
            outBytes.InsertRange(0, arcHead);

            ByteListExtension.Reset();
            return outBytes.ToArray();
        }
    }
}
