using Reloaded.Memory.Streams;
using System.Collections.Generic;
using AquaModelLibrary.Extra.Ninja.BillyHatcher.LNDH;
using static AquaModelLibrary.Extra.Ninja.BillyHatcher.ARC;
using System.Diagnostics;

namespace AquaModelLibrary.Extra.Ninja.BillyHatcher
{
    public class LND
    {
        public byte[] gvmBytes = null;
        public List<uint> pof0Offsets = new List<uint>();

        public bool isArcLND = false;

        //ARCLND Data
        public ARCHeader arcHeader;
        public ARCLNDHeader arcLndHeader;
        public List<int> extraFileOffsets = new List<int>();
        public ARCLNDRefTableHead refTable;
        public List<ARCLNDRefEntry> refTableEntries = new List<ARCLNDRefEntry>();
        public ARCLNDMainDataHeader mainDataHeader;

        //LND Data
        public NinjaHeader nHeader;
        public LNDHeader header;
        public LNDHeader2 header2;
        public LNDNodeIdSet nodeIdSet;
        public List<LandEntry> nodes = new List<LandEntry>();
        public List<ushort> nodeIds = new List<ushort>();
        public List<uint> objectOffsets = new List<uint>();
        public List<LNDMeshInfo> meshInfo = new List<LNDMeshInfo>();
        public LNDTexDataEntryHead texDataEntryHead;
        public List<LNDTexDataEntry> texDataEntries = new List<LNDTexDataEntry>();
        public List<string> texnames = new List<string>();
        public List<int> motionDataOffsets = new List<int>();
        public List<LNDMotionDataHead> motionDataHeadList = new List<LNDMotionDataHead>();
        public List<LNDMotionDataHead2> motionDataHead2List = new List<LNDMotionDataHead2>();
        public List<List<LNDMotionData>> motionDataList = new List<List<LNDMotionData>>();
        public LND() { }

        public LND(BufferedStreamReader sr)
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
            } else {
                isArcLND = true;
                //This is based more around the .arc format
                ReadARCLND(sr);
                gvmBytes = GVMUtil.ReadGVMBytes(sr);
            }
        }

        public void ReadARCLND(BufferedStreamReader sr)
        {
            //Generic ARC header
            arcHeader = new ARCHeader();
            arcHeader.fileSize = sr.ReadBE<int>();
            arcHeader.pof0Offset = sr.ReadBE<int>();
            arcHeader.pof0OffsetsSize = sr.ReadBE<int>();
            arcHeader.nameCount = sr.ReadBE<int>();

            arcHeader.unkCount = sr.ReadBE<int>();
            arcHeader.magic = sr.ReadBE<int>();
            arcHeader.unkInt0 = sr.ReadBE<int>();
            arcHeader.unkInt1 = sr.ReadBE<int>();

            //Core ARCLND header
            arcLndHeader.mainDataOffset = sr.ReadBE<int>();
            arcLndHeader.extraFileCount = sr.ReadBE<int>();
            arcLndHeader.extraFileOffsetsOffset = sr.ReadBE<int>();
            arcLndHeader.motionFileOffset = sr.ReadBE<int>();

            arcLndHeader.texRefTableOffset = sr.ReadBE<int>();
            arcLndHeader.GVMOffset = sr.ReadBE<int>();

            sr.Seek(0x20 + arcLndHeader.extraFileOffsetsOffset, System.IO.SeekOrigin.Begin);
            for(int i = 0; i < arcLndHeader.extraFileCount; i++)
            {
                extraFileOffsets.Add(sr.ReadBE<int>());
            }

            foreach (var offset in extraFileOffsets)
            {
                sr.Seek(0x20 + offset, System.IO.SeekOrigin.Begin);
                //TODO
            }

            //Model
            sr.Seek(0x20 + arcLndHeader.mainDataOffset, System.IO.SeekOrigin.Begin);
            mainDataHeader = new ARCLNDMainDataHeader();
            mainDataHeader.mainOffsetTableOffset = sr.ReadBE<int>();
            mainDataHeader.unkOffset0 = sr.ReadBE<int>();
            mainDataHeader.unkCount = sr.ReadBE<int>();
            mainDataHeader.unkOffset1 = sr.ReadBE<int>();

            mainDataHeader.unkInt_10 = sr.ReadBE<int>();
            mainDataHeader.unkInt_14 = sr.ReadBE<int>();
            mainDataHeader.unkInt_18 = sr.ReadBE<int>();
            mainDataHeader.unkInt_1C = sr.ReadBE<int>();




            //Read texture reference table
            sr.Seek(0x20 + arcLndHeader.texRefTableOffset, System.IO.SeekOrigin.Begin);
            refTable = new ARCLNDRefTableHead();
            refTable.entryOffset = sr.ReadBE<int>();
            refTable.entryCount = sr.ReadBE<int>();

            sr.Seek(0x20 + refTable.entryOffset, System.IO.SeekOrigin.Begin);
            for(int i = 0; i < refTable.entryCount; i++)
            {
                ARCLNDRefEntry refEntry = new ARCLNDRefEntry();
                refEntry.textOffset = sr.ReadBE<int>();
                refEntry.unkInt0 = sr.ReadBE<int>();
                refEntry.unkInt1 = sr.ReadBE<int>();
                refTableEntries.Add(refEntry);
            }

            //Read motions
            if(arcLndHeader.motionFileOffset > 0)
            {
                sr.Seek(0x20 + arcLndHeader.motionFileOffset, System.IO.SeekOrigin.Begin);
                //TODO
            }

            sr.Seek(0x20 + arcLndHeader.GVMOffset, System.IO.SeekOrigin.Begin);
            sr.Seek(sr.ReadBE<int>() + 0x20, System.IO.SeekOrigin.Begin);
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
            for(int i = 0; i < header.motionDataCount; i++)
            {
                motionDataOffsets.Add(sr.ReadBE<int>());
            }
            foreach(var offset in motionDataOffsets)
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
            if(header.lndTexNameListOffset > 0)
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
            for(int i = 0; i < header2.nodeCount; i++)
            {
                var node = new LandEntry();
                node.flag = (ContentFlag)sr.ReadBE<int>();
                node.objectIndex = sr.ReadBE<ushort>();
                node.motionIndex = sr.ReadBE<ushort>();
                node.Position = sr.ReadBEV3();
                node.flt14 = sr.ReadBE<float>();
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
            for(int i = 0; i < nodeIdSet.nodeCount; i++)
            {
                nodeIds.Add(sr.ReadBE<ushort>());
            }

            //Mesh data
            sr.Seek(header.lndMeshInfoOffset + 0x8, System.IO.SeekOrigin.Begin);
            for(int i = 0; i < header.nodeCount; i++)
            {
                objectOffsets.Add(sr.ReadBE<uint>());
            }

            foreach(var offset in objectOffsets)
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

                if(lndMeshInfo.lndMeshInfo2Offset > 0)
                {
                    sr.Seek(lndMeshInfo.lndMeshInfo2Offset + 0x8, System.IO.SeekOrigin.Begin);
                    LNDMeshInfo2 lndMeshInfo2 = new LNDMeshInfo2();
                    lndMeshInfo2.layoutsOffset = sr.ReadBE<int>();
                    lndMeshInfo2.unkOffset0 = sr.ReadBE<int>();
                    lndMeshInfo2.polyInfo0Offset = sr.ReadBE<int>();
                    lndMeshInfo2.polyInfo1Offset = sr.ReadBE<int>();

                    lndMeshInfo2.extraVertDataCount = sr.ReadBE<ushort>();
                    lndMeshInfo2.usht12 = sr.ReadBE<ushort>();
                    lndMeshInfo2.flt14 = sr.ReadBE<float>();
                    lndMeshInfo2.Position = sr.ReadBEV3();
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
                                    lndMeshInfo2.vertData.vertColorData.Add(sr.ReadBE<short>());
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
                        sr.Seek(lndMeshInfo2.polyInfo0Offset + 0x8, System.IO.SeekOrigin.Begin);
                        PolyInfo polyInfo = new PolyInfo();
                        polyInfo.materialOffset = sr.ReadBE<int>();
                        polyInfo.unkCount = sr.ReadBE<ushort>();
                        polyInfo.materialDataCount = sr.ReadBE<ushort>();
                        polyInfo.polyDataOffset = sr.ReadBE<int>();
                        polyInfo.polyDataBufferSize = sr.ReadBE<int>();
                        lndMeshInfo2.polyInfo0 = polyInfo;
                        ReadPolyData(sr, lndMeshInfo2, vertTypeCount, polyInfo);
                    }
                    if (lndMeshInfo2.polyInfo1Offset != 0)
                    {
                        sr.Seek(lndMeshInfo2.polyInfo1Offset + 0x8, System.IO.SeekOrigin.Begin);
                        PolyInfo polyInfo = new PolyInfo();
                        polyInfo.materialOffset = sr.ReadBE<int>();
                        polyInfo.unkCount = sr.ReadBE<ushort>();
                        polyInfo.materialDataCount = sr.ReadBE<ushort>();
                        polyInfo.polyDataOffset = sr.ReadBE<int>();
                        polyInfo.polyDataBufferSize = sr.ReadBE<int>();
                        lndMeshInfo2.polyInfo1 = polyInfo;
                        ReadPolyData(sr, lndMeshInfo2, vertTypeCount, polyInfo);
                    }

                    meshInfo.Add(lndMeshInfo);
                }
            }

            //Seek for other data
            sr.Seek(nHeader.fileSize + 0x8, System.IO.SeekOrigin.Begin);
        }

        private static void ReadPolyData(BufferedStreamReader sr, LNDMeshInfo2 lndMeshInfo2, int vertTypeCount, PolyInfo polyInfo)
        {
            //Material data
            byte[] indexSizes = null;
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

                    switch(matInfo.matInfoType)
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
                            for(int j = 0; j < mapping.Length; j++)
                            {
                                //It's likely only 8 maps to another vertex index, but just in case
                                if (mapping[j] != 0 && !tempDict.ContainsKey(mapping[j]))
                                {
                                    tempDict.Add(mapping[j], j);
                                    polyInfo.vertIndexMapping.Add(j, j);
                                } else if (mapping[j] == 0) //0 only maps to itself
                                {
                                    polyInfo.vertIndexMapping.Add(j, j);
                                } else
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
                    var starts = new List<int>();
                    starts.Add(type);
                    starts.Add(count);
                    triIndicesStarts.Add(starts);
                    for (int i = 0; i < count; i++)
                    {
                        List<int> triIndex = new List<int>();
                        for (int j = 0; j < vertTypeCount; j++)
                        {
                            var lyt = lndMeshInfo2.layouts[j];
                            if(indexSizes?.Length > j )
                            {
                                switch(indexSizes[j])
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
                            } else //Fallback for if for some godforsaken reason this doesn't exist
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
    }
}
