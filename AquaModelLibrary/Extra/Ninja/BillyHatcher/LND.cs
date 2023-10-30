using Reloaded.Memory.Streams;
using System.Collections.Generic;
using System.Numerics;
using Vector3Integer;

namespace AquaModelLibrary.Extra.Ninja
{
    public class LND
    {
        public byte[] gvmBytes = null;
        public List<uint> pof0Offsets = new List<uint>();

        //AltLND data
        NinjaHeader nHeader;
        LNDHeader header;
        LNDHeader2 header2;
        LNDNodeIdSet nodeIdSet;
        public List<LandEntry> nodes = new List<LandEntry>();
        public List<ushort> nodeIds = new List<ushort>();
        public List<uint> objectOffsets = new List<uint>();
        public List<LNDMeshInfo> meshInfo = new List<LNDMeshInfo>();
        public List<LNDMeshInfo2> meshInfo2 = new List<LNDMeshInfo2>();
        public List<LNDMeshInfo3> meshInfo3 = new List<LNDMeshInfo3>();
        public List<PolyInfo> polyInfoList = new List<PolyInfo>();
        public List<List<List<List<int>>>> polygons = new List<List<List<List<int>>>>();
        public List<List<MaterialInfo>> matInfoListList = new List<List<MaterialInfo>>();
        public LNDTexDataEntryHead texDataEntryHead;
        public List<LNDTexDataEntry> texDataEntries = new List<LNDTexDataEntry>();
        public List<string> texnames = new List<string>();
        public List<int> motionDataOffsets = new List<int>();
        public LND() { }

        public LND(BufferedStreamReader sr)
        {
            BigEndianHelper._active = true;
            var magicTest = sr.ReadBytes(0, 3);

            if (magicTest[0] == 0x4C && magicTest[1] == 0x4E && magicTest[2] == 0x44)
            {
                ReadAltLND(sr);
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
                //This is based more around the .arc format
                ReadLND(sr);
                gvmBytes = GVMUtil.ReadGVMBytes(sr);
            }
        }

        public void ReadLND(BufferedStreamReader sr)
        {
            sr.Seek(0x34, System.IO.SeekOrigin.Begin);
            sr.Seek(sr.ReadBE<int>() + 0x20, System.IO.SeekOrigin.Begin);
        }

        /// <summary>
        /// This seems to be mainly for older LND archives. They have an actual LND magic unlike the more common type
        /// </summary>
        public void ReadAltLND(BufferedStreamReader sr)
        {
            nHeader = sr.Read<NinjaHeader>();
            header = new LNDHeader();
            header.lndHeader2Offset = sr.ReadBE<int>();
            header.nodeCount = sr.ReadBE<ushort>();
            header.motionDataCount = sr.ReadBE<ushort>();
            header.lndMeshInfoOffset = sr.ReadBE<int>();
            header.motionDataOffset = sr.ReadBE<int>();
            header.lndTexNameListOffset = sr.ReadBE<int>();

            //Tex name list
            sr.Seek(header.lndTexNameListOffset + 0x8, System.IO.SeekOrigin.Begin);
            texDataEntryHead = new LNDTexDataEntryHead();
            texDataEntryHead.offset = sr.ReadBE<int>();
            texDataEntryHead.count = sr.ReadBE<int>();
            for(int i = 0; i < texDataEntryHead.count; i++)
            {
                LNDTexDataEntry entry = new LNDTexDataEntry();
                entry.offset = sr.ReadBE<int>();
                entry.unk0 = sr.ReadBE<int>();
                entry.unk1 = sr.ReadBE<int>();
                texDataEntries.Add(entry);
            }
            foreach(LNDTexDataEntry entry in texDataEntries)
            {
                sr.Seek(entry.offset + 0x8, System.IO.SeekOrigin.Begin);
                texnames.Add(AquaMethods.AquaGeneralMethods.ReadCString(sr));
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
            nodeIdSet.nodeCount = sr.Read<ushort>();
            nodeIdSet.usht02 = sr.Read<ushort>();
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

                sr.Seek(lndMeshInfo.lndMeshInfo2Offset + 0x8, System.IO.SeekOrigin.Begin);
                LNDMeshInfo2 lndMeshInfo2 = new LNDMeshInfo2();
                lndMeshInfo2.lndMeshInfo3Offset = sr.ReadBE<int>();
                lndMeshInfo2.unkOffset0 = sr.ReadBE<int>();
                lndMeshInfo2.polyInfoOffset = sr.ReadBE<int>();
                lndMeshInfo2.unkOffset1 = sr.ReadBE<int>();

                lndMeshInfo2.extraVertDataCount = sr.ReadBE<ushort>();
                lndMeshInfo2.usht12 = sr.ReadBE<ushort>();
                lndMeshInfo2.flt14 = sr.ReadBE<float>();
                lndMeshInfo2.Position = sr.ReadBEV3();
                meshInfo2.Add(lndMeshInfo2);

                sr.Seek(lndMeshInfo2.lndMeshInfo3Offset + 0x8, System.IO.SeekOrigin.Begin);
                LNDMeshInfo3 lndMeshInfo3 = new LNDMeshInfo3();
                for(int i = 0; i < lndMeshInfo2.extraVertDataCount + 1; i++)
                {
                    LNDVertLayout lyt = new LNDVertLayout();
                    lyt.vertType = sr.Read<byte>();
                    lyt.dataType = sr.Read<byte>();
                    lyt.vertCount = sr.ReadBE<ushort>();
                    lyt.unkCount = sr.ReadBE<int>();
                    lyt.vertDataOffset = sr.ReadBE<int>();
                    lyt.vertDataBufferSize = sr.ReadBE<int>();
                    lndMeshInfo3.layouts.Add(lyt);
                }
                meshInfo3.Add(lndMeshInfo3);

                //Vertex data
                foreach(var lyt in lndMeshInfo3.layouts)
                {
                    sr.Seek(lyt.vertDataOffset + 0x8, System.IO.SeekOrigin.Begin);
                    for(int i = 0; i < lyt.vertCount; i++)
                    {
                        switch(lyt.vertType)
                        {
                            case 0x1:
                                break;
                            case 0x2:
                                break;
                            case 0x3:
                                break;
                            case 0x5:
                                break;
                            default:
                                throw new System.Exception($"Unk Vert type: {lyt.vertType:X} Data type: {lyt.dataType:X}");
                        }
                    }
                }

                //Polygon data
                sr.Seek(lndMeshInfo2.polyInfoOffset + 0x8, System.IO.SeekOrigin.Begin);
                PolyInfo polyInfo = new PolyInfo();
                polyInfo.materialOffset = sr.ReadBE<int>();
                polyInfo.materialDataCount = sr.ReadBE<int>();
                polyInfo.polyDataOffset = sr.ReadBE<int>();
                polyInfo.polyDataBufferSize = sr.ReadBE<int>();
                polyInfoList.Add(polyInfo);

                //Material data
                sr.Seek(polyInfo.materialOffset + 0x8, System.IO.SeekOrigin.Begin);
                List<MaterialInfo> matInfoList = new List<MaterialInfo>();
                for(int i = 0; i < polyInfo.materialDataCount; i++)
                {
                    MaterialInfo matInfo = new MaterialInfo();
                    matInfo.matInfoType = sr.ReadBE<int>();
                    matInfo.matData0 = sr.Read<byte>();
                    matInfo.matData1 = sr.Read<byte>();
                    matInfo.matData2 = sr.Read<byte>();
                    matInfo.matData3 = sr.Read<byte>();
                    matInfoList.Add(matInfo);
                }
                matInfoListList.Add(matInfoList);

                //Polygons
                sr.Seek(polyInfo.polyDataOffset + 0x8, System.IO.SeekOrigin.Begin);
                List<List<List<int>>> triIndicesList = new List<List<List<int>>>();
                while(sr.Position() < polyInfo.polyDataBufferSize + polyInfo.polyDataOffset + 8)
                {
                    var type = sr.Read<byte>();
                    var count = sr.ReadBE<ushort>();
                    if(type == 0)
                    {
                        break;
                    }
                    List<List<int>> triIndices = new List<List<int>>();
                    for (int i = 0; i < count; i++)
                    {
                        List<int> triIndex = new List<int>();
                        for(int j = 0; j < lndMeshInfo2.extraVertDataCount + 1; j++)
                        {
                            triIndex.Add(sr.Read<byte>());
                        }
                        triIndices.Add(triIndex);
                    }
                    triIndicesList.Add(triIndices);
                }
                polygons.Add(triIndicesList);

                meshInfo.Add(lndMeshInfo);
            }

            //Seek for other data
            sr.Seek(nHeader.fileSize + 0x8, System.IO.SeekOrigin.Begin);
        }

        //Alt LND header
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

        public struct LNDMotinDataHead2
        {

            public int dataOffset;
            public int unkInt;
            /// <summary>
            /// Entry count - 1
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
            public int count;
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

        public struct LNDMeshInfo
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
        }

        public struct LNDMeshInfo2
        {
            public int lndMeshInfo3Offset;
            public int unkOffset0;
            public int polyInfoOffset;
            public int unkOffset1;

            public ushort extraVertDataCount;
            public ushort usht12;
            public float flt14;
            public Vector3 Position;
        }

        public struct PolyInfo
        {
            public int materialOffset;
            public int materialDataCount;
            public int polyDataOffset;
            public int polyDataBufferSize;
        }

        public struct MaterialInfo
        {
            public int matInfoType;
            public byte matData0;
            public byte matData1;
            public byte matData2;
            public byte matData3;
        }

        public class LNDMeshInfo3
        {
            public List<LNDVertLayout> layouts = new List<LNDVertLayout>();
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
            public List<short[]> vert2Data = new List<short[]>();      //2, unknown data
            public List<short> vertColorData = new List<short>();      //3, possibly color data
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

            public Vector3 Position;
            public float flt14;
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
}
