using AquaModelLibrary.NNStructs.Structures;
using Reloaded.Memory.Streams;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Text;
using static Vector3Integer.Vector3Int;

namespace AquaModelLibrary
{
    //Massive thanks to Agrajag for this, really would not be as well supported as it is without his work. Some parts were borrowed more directly than others, particularly materials wise
    //Naming based primarily on variable namings in Sonic 06 via an exported text file leftover from development.
    //Some changes may be made for clarity
    public class NNObject
    {
        public XNJHeader header = new XNJHeader();
        public MysteryObject mysteryObject = new MysteryObject();
        public bool usesMysterObject = false;
        public List<string> texList = new List<string>();
        public AquaNode nodes = null; //Technically not the same, but it's close enough we can store everything PSU really has here
        public List<NN_NODE> NNNodes = new List<NN_NODE>(); 
        public List<AquaObject.VTXL> vtxlList = new List<AquaObject.VTXL>();
        public List<NNS_MeshSetInfo> meshInfoList = new List<NNS_MeshSetInfo>();
        public List<List<NNS_MeshSet>> meshListList = new List<List<NNS_MeshSet>>();
        public List<XNJMaterial> materials = new List<XNJMaterial>();
        public List<List<List<ushort>>> stripListList = new List<List<List<ushort>>>();
        public Dictionary<int, int> animNodeMap = null;
        public List<byte[]> nodeMetaData = new List<byte[]>();


        public const string NXIF = "NXIF";
        public const string NXTL = "NXTL"; //NX Texture List
        public const string NXEF = "NXEF"; //NX Effect List
        public const string NXNN = "NXNN"; //NX Node Names
        public const string NXOB = "NXOB"; //NX Object

        public BufferedStreamReader streamReader = null;
        public int offset = 0;

        //XNJ is incredibly similar to XNO, but not quite the same
        public void ReadPSUXNJ(string filePath)
        {
            var fileData = File.ReadAllBytes(filePath);
            fileData = GetNXOB(fileData);
            streamReader = new BufferedStreamReader(new MemoryStream(fileData), 8192);

            var magic = Encoding.ASCII.GetString(streamReader.ReadBytes(0, 4));
            offset = GetNNOffset();

            streamReader.Seek(8, SeekOrigin.Begin);
            var headerOffset = streamReader.Read<int>();
            var version = streamReader.Read<int>(); //Always 3?
            int vertTotalLen = streamReader.Read<int>();
            int vertDataOffset = streamReader.Read<int>();

            streamReader.Seek(offset + headerOffset, SeekOrigin.Begin);
            header = streamReader.Read<XNJHeader>();
            if (streamReader.Position() < vertDataOffset && streamReader.Position() + 3 < fileData.Length)
            {
                usesMysterObject = true;
                mysteryObject = streamReader.Read<MysteryObject>();
            }

            var texPath = Path.ChangeExtension(filePath, ".xnt");
            if (File.Exists(texPath))
            {
                texList = NNTextureList.ReadNNT(texPath);
            }
            ReadBones(Path.ChangeExtension(filePath, ".xna"));
            ReadVerts();
            ReadMaterials();
            ReadFaceData();
            ReadMeshData();
        }

        public static byte[] GetNXOB(byte[] bytes)
        {
            var magic = Encoding.UTF8.GetString(bytes, 0, 4);
            int headerOff = 0;
            if (magic == "NXOB" && magic == "NYOB" && magic == "NXR\0" && magic == "NYR\0")
            {
                return bytes;
            }
            while(magic != "NXOB" && magic != "NYOB" && magic != "NXR\0" && magic != "NYR\0")
            {
                switch (magic)
                {
                    case "YNJ\0":
                    case "XNJ\0":
                        headerOff += BitConverter.ToInt32(bytes, headerOff + 4);
                        break;
                    case "NYR\0":
                    case "NXR\0":
                    default:
                        headerOff += BitConverter.ToInt32(bytes, headerOff + 4) + 8;
                        break;
                }

                magic = Encoding.UTF8.GetString(bytes, headerOff, 4);
            }

            byte[] newArray = new byte[bytes.Length - headerOff];
            Array.Copy(bytes, headerOff, newArray, 0, newArray.Length);
            return newArray;
        }

        //Returns an aqp ready for the ConvertToNGSPSO2Mesh method
        public AquaObject ConvertToBasicAquaobject(out AquaNode aqn)
        {
            aqn = nodes;
            AquaObject aqp = new NGSAquaObject();
            aqp.bonePalette = new List<uint>();
            for (int i = 0; i < aqn.nodeList.Count; i++)
            {
                aqp.bonePalette.Add((uint)i);
            }

            //Convert material data
            for (int i = 0; i < materials.Count; i++)
            {
                var curMat = materials[i];
                AquaObject.GenericMaterial genMat = new AquaObject.GenericMaterial();
                genMat.diffuseRGBA = new Vector4(1, 1, 1, 1);
                genMat.matName = $"PSUMaterial_{i}";
                genMat.texNames = new List<string>();
                for (int t = 0; t < curMat.textureList.Count; t++)
                {
                    string name = $"tex_{t}";
                    if (texList.Count - 1 > t)
                    {
                        name = texList[curMat.textureList[t].textureId];
                    }
                    genMat.texNames.Add(Path.ChangeExtension(name, ".dds"));
                }
                aqp.tempMats.Add(genMat);
            }

            //Create mesh data
            for (int i = 0; i < meshListList.Count; i++)
            {
                for (int j = 0; j < meshListList[i].Count; j++)
                {
                    aqp.meshNames.Add($"meshgroup_{i}_{j}");
                    var group = meshListList[i][j].vtxlId;
                    var matId = meshListList[i][j].materialId;
                    AquaObject.GenericTriangles genMesh = new AquaObject.GenericTriangles();
                    genMesh.vertCount = vtxlList[group].vertPositions.Count;

                    //Generate strips
                    var mesh = meshListList[i][j];
                    var meshStripSet = stripListList[mesh.faceSetId];

                    for (int s = 0; s < meshStripSet.Count; s++)
                    {
                        bool flip = false;
                        int f = 0;
                        var strips = meshStripSet[s];

                        while (f < strips.Count - 2)
                        {
                            if (strips[f + 2] == ushort.MaxValue)
                            {
                                f += 3;
                            }
                            else
                            {
                                var face = new Vector3(strips[f], strips[f + 1], strips[f + 2]);

                                if (flip)
                                {
                                    var temp = face.X;
                                    face.X = face.Y;
                                    face.Y = temp;
                                }
                                flip = !flip;

                                f += 1;
                                if (face.X == face.Y || face.Y == face.Z || face.X == face.Z)
                                {
                                    continue;
                                }

                                AquaObject.VTXL vtxl = new AquaObject.VTXL();
                                var prevListCount = vtxlList[group].vertPositions.Count;
                                if(prevListCount < face.X || prevListCount < face.Y || prevListCount < face.Z)
                                {
                                    throw new Exception();
                                }
                                AquaObjectMethods.appendVertex(vtxlList[group], vtxl, (int)face.X);
                                AquaObjectMethods.appendVertex(vtxlList[group], vtxl, (int)face.Y);
                                AquaObjectMethods.appendVertex(vtxlList[group], vtxl, (int)face.Z);
                                vtxl.rawVertId.Add((int)face.X);
                                vtxl.rawVertId.Add((int)face.Y);
                                vtxl.rawVertId.Add((int)face.Z);
                                vtxl.rawFaceId.Add(genMesh.triList.Count);
                                vtxl.rawFaceId.Add(genMesh.triList.Count);
                                vtxl.rawFaceId.Add(genMesh.triList.Count);
                                genMesh.faceVerts.Add(vtxl);
                                genMesh.triList.Add(face);
                                genMesh.matIdList.Add(matId);
                            }
                        }
                    }

                    if (genMesh.faceVerts.Count != 0)
                    {
                        aqp.tempTris.Add(genMesh);
                    }
                }
            }

            return aqp;
        }

        public void ReadMeshData()
        {
            for (int i = 0; i < header.drawCount; i++)
            {
                streamReader.Seek(offset + header.drawOffset + i * 0x14, SeekOrigin.Begin);
                var info = streamReader.Read<NNS_MeshSetInfo>();
                meshInfoList.Add(info);

                var meshSetList = new List<NNS_MeshSet>();
                streamReader.Seek(offset + info.directMeshOffset, SeekOrigin.Begin);
                for (int m = 0; m < info.directMeshCount; m++)
                {
                    var mesh = streamReader.Read<NNS_MeshSet>();
                    meshSetList.Add(mesh);
                }
                meshListList.Add(meshSetList);
            }
        }

        public void ReadFaceData()
        {
            for (int i = 0; i < header.faceGroupCount; i++)
            {
                List<List<ushort>> stripList = new List<List<ushort>>();
                streamReader.Seek(offset + header.faceGroupOffset + i * 8, SeekOrigin.Begin);
                int indexTopCount = streamReader.Read<int>();
                int subHeaderOffset = streamReader.Read<int>();
                if (indexTopCount > 1)
                {
                    Debug.WriteLine("Multiple subheaders!");
                }

                streamReader.Seek(offset + subHeaderOffset, SeekOrigin.Begin);
                var indexList = streamReader.Read<NNIndexList>();

                var stripLenList = new List<ushort>();
                streamReader.Seek(offset + indexList.lengthOffset, SeekOrigin.Begin);
                for(int lenEntry = 0; lenEntry < indexList.stripCount; lenEntry++)
                {
                    stripLenList.Add(streamReader.Read<ushort>());
                }

                streamReader.Seek(offset + indexList.indexOffset, SeekOrigin.Begin);
                for (int lenEntry = 0; lenEntry < indexList.stripCount; lenEntry++)
                {
                    var currentLen = stripLenList[lenEntry];
                    var strip = new List<ushort>();
                    for (int ind = 0; ind < currentLen; ind++)
                    {
                        strip.Add(streamReader.Read<ushort>());
                    }
                    stripList.Add(strip);
                }

                stripListList.Add(stripList);
            }
        }

        public int GetNNOffset()
        {
            bool keepChecking = true;
            bool modelFound = false;
            int nxOffset = 0;

            do
            {
                var type = streamReader.Read<uint>();
                switch (type)
                {
                    //NXIF
                    case 0x4649584E:
                        keepChecking = false;
                        streamReader.Seek(0x1C, SeekOrigin.Current);
                        nxOffset += 0x20;
                        break;
                    //NXR or NXOB
                    case 0x52584E:
                        keepChecking = false;
                        nxOffset -= (int)streamReader.Position() - nxOffset - 0x4;
                        modelFound = true;
                        break;
                    case 0x424F584E:
                        keepChecking = false;
                        nxOffset -= (int)streamReader.Position() - nxOffset - 0x4;
                        modelFound = true;
                        break;
                    //NXTL
                    case 0x4C54584E:
                        keepChecking = false;
                        nxOffset -= (int)streamReader.Position() - nxOffset - 0x4;
                        break;
                    //NXNN
                    case 0x4E4E584E:
                        keepChecking = false;
                        nxOffset -= (int)streamReader.Position() - nxOffset - 0x4;
                        break;
                    //NXEF
                    case 0x4645584E:
                        keepChecking = false;
                        nxOffset -= (int)streamReader.Position() - nxOffset - 0x4;
                        break;
                }
            } while (keepChecking == true);

            if (modelFound == false)
            {
                keepChecking = true;
                do
                {
                    var type = streamReader.Read<uint>();
                    switch (type)
                    {
                        //NXR or NXOB
                        case 0x52584E:
                            keepChecking = false;
                            break;

                        case 0x424F584E:
                            keepChecking = false;
                            break;

                    }
                } while (keepChecking == true);
            }
            streamReader.Seek(0x4, SeekOrigin.Current);

            var Obj = streamReader.Read<uint>() + nxOffset;
            Debug.WriteLine($"{Obj:X}");

            int key = 0;
            int boneOff = 0x10;

            //Since file offsets after the Obj offset seem to be absolute for some reason in some files, calc the "key".
            streamReader.Seek(0x8, SeekOrigin.Current);
            var test0 = streamReader.Read<ushort>();
            var test1 = streamReader.Read<ushort>();

            while (test1 != 0xFFFF) //Technically doesn't account for all possible scenarios, but should account for all used scenarios
            {
                streamReader.Seek(0xC, SeekOrigin.Current);
                test0 = streamReader.Read<ushort>();
                test1 = streamReader.Read<ushort>();
                boneOff += 0x10;
            }
            streamReader.Seek(Obj + 0x30, SeekOrigin.Begin);
            int boneOffUnfixed = streamReader.Read<int>();
            key = boneOffUnfixed - boneOff - nxOffset;

            return key;
        }

        public void ReadMaterials()
        {
            for (int i = 0; i < header.matCount; i++)
            {
                XNJMaterial mat = new XNJMaterial();
                //Initial group
                streamReader.Seek(offset + header.matOffset + (i * 8), SeekOrigin.Begin);
                mat.unknownTopLevelInt = streamReader.Read<int>();
                int matGroupOffset = streamReader.Read<int>();

                //Subgroup
                streamReader.Seek(offset + matGroupOffset, SeekOrigin.Begin);
                mat.unknownID = streamReader.Read<int>();
                mat.unknownInt1 = streamReader.Read<int>();

                int lightingDataAddr = streamReader.Read<int>();
                int secondSubAddrUncalculated = streamReader.Read<int>();
                int secondSubAddr = secondSubAddrUncalculated;
                int thirdSubAddr = streamReader.Read<int>();

                mat.unknownInt2 = streamReader.Read<int>();
                mat.unknownInt3 = streamReader.Read<int>();
                mat.unknownInt4 = streamReader.Read<int>();

                //Lighting
                streamReader.Seek(offset + lightingDataAddr, SeekOrigin.Begin);
                mat.colorCorrection = streamReader.Read<Matrix4x4>();
                mat.specularIntensity = streamReader.Read<float>();
                mat.unknownSubInt1 = streamReader.Read<int>();
                mat.unknownSubInt2 = streamReader.Read<int>();
                mat.unknownSubInt3 = streamReader.Read<int>();

                //Second subgroup
                if (secondSubAddrUncalculated + offset != 0)
                {
                    streamReader.Seek(offset + secondSubAddr, SeekOrigin.Begin);
                    mat.materialD3DRenderFlagsUnparsed = streamReader.ReadBytes(streamReader.Position(), 0x40);
                    mat.renderData = new RenderValues(mat.materialD3DRenderFlagsUnparsed);
                }

                //Textures
                mat.textureList = new BindingList<TextureEntry>();
                int textureCount = mat.unknownTopLevelInt & 0xF;

                for (int j = 0; j < textureCount; j++)
                {
                    streamReader.Seek(thirdSubAddr + j * 0x30, SeekOrigin.Begin);
                    TextureEntry tex = new TextureEntry();
                    tex.texturingMode = streamReader.Read<byte>();
                    tex.uvMappingMode = streamReader.Read<byte>();
                    tex.textureMappingMode = streamReader.Read<byte>();
                    tex.flag4 = streamReader.Read<byte>();
                    tex.textureId = streamReader.Read<int>();
                    tex.translateU = streamReader.Read<float>();
                    tex.translateV = streamReader.Read<float>();
                    tex.unknownFloat1 = streamReader.Read<float>();
                    tex.unknownFloat2 = streamReader.Read<float>();
                    tex.minifyIndex = streamReader.Read<short>();
                    tex.magnifyIndex = streamReader.Read<short>();
                    tex.unknownInt2 = streamReader.Read<uint>();
                    tex.unknownInt3 = streamReader.Read<uint>();
                    tex.unknownInt4 = streamReader.Read<uint>();
                    tex.unknownInt5 = streamReader.Read<uint>();
                    tex.unknownInt6 = streamReader.Read<uint>();
                    mat.textureList.Add(tex);
                }

                materials.Add(mat);
            }
        }

        public void ReadVerts()
        {
            List<NNVertexSet> vsets = new List<NNVertexSet>();
            for (int i = 0; i < header.vertGroupCount; i++)
            {
                streamReader.Seek(offset + header.vertGroupOffset + i * 8, SeekOrigin.Begin);
                int unk1 = streamReader.Read<int>();
                int vsetOffset = streamReader.Read<int>();

                streamReader.Seek(offset + vsetOffset, SeekOrigin.Begin);
                var vset = streamReader.Read<NNVertexSet>();
                vsets.Add(vset);

                AquaObject.VTXL vtxl = ReadXNJVTXL(vset);

                vtxlList.Add(vtxl);
            }
        }

        public AquaObject.VTXL ReadXNJVTXL(NNVertexSet vset)
        {
            AquaObject.VTXL vtxl = new AquaObject.VTXL();

            //Get BonePalette
            streamReader.Seek(offset + vset.bonePaletteOffset, SeekOrigin.Begin);
            List<ushort> bonePalette = new List<ushort>();
            int[] vertIndices = new int[4];
            for (int j = 0; j < vset.bonePaletteCount; j++)
            {
                ushort index = (ushort)animNodeMap[(ushort)streamReader.Read<uint>()];
                vertIndices[j] = index;
                bonePalette.Add(index);
            }
            vtxl.bonePalette = bonePalette;

            //Calc uv count
            int uvCount = (vset.vertFormat & (int)(XNJVertexFlags.HasUvs | XNJVertexFlags.HasVector4)) >> 8;

            //Read the actual vertices
            streamReader.Seek(offset + vset.vtxlOffset, SeekOrigin.Begin);

            for (int v = 0; v < vset.vtxlCount; v++) //I think we want to overread?? Maybe?
            {
                var calcedSize = 0;
                if ((vset.vertFormat & (int)XNJVertexFlags.HasXyz) > 0)
                {
                    calcedSize += 0xC;
                    vtxl.vertPositions.Add(streamReader.Read<Vector3>());
                }
                if ((vset.vertFormat & (int)XNJVertexFlags.HasWeights) > 0)
                {
                    calcedSize += 0xC;
                    var weights = streamReader.Read<Vector3>();
                    vtxl.vertWeights.Add(new Vector4(weights, 1f - weights.X - weights.Y - weights.Z));
                    vtxl.vertWeightIndices.Add(vertIndices);
                }
                if ((vset.vertFormat & (int)XNJVertexFlags.HasNormals) > 0)
                {
                    calcedSize += 0xC;
                    vtxl.vertNormals.Add(streamReader.Read<Vector3>());
                }
                if ((vset.vertFormat & (int)XNJVertexFlags.HasColors) > 0)
                {
                    calcedSize += 0x4;
                    vtxl.vertColors.Add(streamReader.ReadBytes(streamReader.Position(), 4));
                    streamReader.Seek(4, SeekOrigin.Current);
                }
                if ((vset.vertFormat & (int)XNJVertexFlags.HasAlpha) > 0)
                {
                    calcedSize += 0x4;
                    vtxl.vertColor2s.Add(streamReader.ReadBytes(streamReader.Position(), 4));
                    streamReader.Seek(4, SeekOrigin.Current);
                }
                for (int uv = 0; uv < uvCount; uv++)
                {
                    switch (uv)
                    {
                        case 0:
                            calcedSize += 0x8;
                            vtxl.uv1List.Add(streamReader.Read<Vector2>());
                            break;
                        case 1:
                            calcedSize += 0x8;
                            vtxl.uv2List.Add(streamReader.Read<Vector2>());
                            break;
                        case 2:
                            calcedSize += 0x8;
                            vtxl.uv3List.Add(streamReader.Read<Vector2>());
                            break;
                        case 3:
                            calcedSize += 0x8;
                            vtxl.uv4List.Add(streamReader.Read<Vector2>());
                            break;
                    }
                }

                if (calcedSize != vset.vertLen)
                {
                    throw new Exception($"Calced size is {calcedSize:X} while vertLen {vset.vertLen}");
                }
            }

            return vtxl;
        }

        public void ReadBones(string filePath = null)
        {
            List<string> boneNames = null;
            if (filePath != null)
            {
                if (File.Exists(filePath))
                {
                    boneNames = NNNodeNames.ReadNNA(filePath);
                }
            }
            nodes = new AquaNode();
            Dictionary<int, AquaNode.NODE> nodeDict = new Dictionary<int, AquaNode.NODE>();
            Dictionary<int, int> nodeIntDict = new Dictionary<int, int>();
            NNNodes = new List<NN_NODE>();

            streamReader.Seek(offset + header.boneOffset, SeekOrigin.Begin);
            for (int i = 0; i < header.boneCount; i++)
            {
                var nnNode = streamReader.Read<NN_NODE>();
                AquaNode.NODE node = new AquaNode.NODE();
                if (boneNames?.Count > i)
                {
                    node.boneName.SetString(boneNames[i]);
                }
                else
                {
                    node.boneName.SetString($"Node_{i}");
                }

                node.boneShort1 = BitConverter.ToUInt16(BitConverter.GetBytes(nnNode.type), 2);
                node.boneShort2 = BitConverter.ToUInt16(BitConverter.GetBytes(nnNode.type), 0);
                int mapId = nnNode.weightUsed;
                if (mapId >= 0)
                {
                    nodeDict.Add(mapId, node);
                    nodeIntDict.Add(mapId, i);
                }
                node.parentId = nnNode.NODE_PARENT;
                node.firstChild = nnNode.NODE_CHILD;
                node.nextSibling = nnNode.NODE_SIBLING;
                node.pos = nnNode.NODE_TRN;
                node.eulRot = new Vector3(nnNode.NODE_ROT.X * 360.0f / 65536.0f, nnNode.NODE_ROT.Y * 360.0f / 65536.0f, nnNode.NODE_ROT.Z * 360.0f / 65536.0f);
                node.scale = nnNode.NODE_SCL;
                node.m1 = nnNode.m1;
                node.m2 = nnNode.m2;
                node.m3 = nnNode.m3;
                node.m4 = nnNode.m4;

                List<byte> nodeBytes = new List<byte>();
                nodeBytes.AddRange(Reloaded.Memory.Struct.GetBytes(nnNode.NODE_CENTER));
                nodeBytes.AddRange(BitConverter.GetBytes(nnNode.NODE_RADIUS));
                nodeBytes.AddRange(BitConverter.GetBytes(nnNode.NODE_BBXSIZE));
                nodeBytes.AddRange(BitConverter.GetBytes(nnNode.NODE_BBYSIZE));
                nodeBytes.AddRange(BitConverter.GetBytes(nnNode.NODE_BBZSIZE));

                nodes.nodeList.Add(node);
                NNNodes.Add(nnNode);
            }
            animNodeMap = nodeIntDict;
        }

        /*
        public enum NODE_TYPE : uint
        {
            NND_NODETYPE_ROTATE_TYPE_XZY,
            NND_NODETYPE_UNIT_TRANSLATION,
            NND_NODETYPE_UNIT_ROTATION,
            NND_NODETYPE_UNIT_SCALING,
            NND_NODETYPE_UNIT_INIT_MATRIX,
            NND_NODETYPE_UNIT33_INIT_MATRIX,
            NND_NODETYPE_ORTHO33_INIT_MATRIX
        }*/

        public struct NN_NODE
        {
            public uint type;
            public short weightUsed;
            public short NODE_PARENT;
            public short NODE_CHILD;
            public short NODE_SIBLING;

            public Vector3 NODE_TRN;
            public Vec3Int NODE_ROT;
            public Vector3 NODE_SCL;
            // 4x4 Matrix NODE_INVINIT_MTX     
            public Vector4 m1;
            public Vector4 m2;
            public Vector4 m3;
            public Vector4 m4;

            //Next variables are usually left 0
            public Vector3 NODE_CENTER;
            public float NODE_RADIUS;

            public int NODE_USER;
            public int NODE_BBXSIZE;
            public int NODE_BBYSIZE;
            public int NODE_BBZSIZE;
        }
        //NNS Xbox vertex data. Also used for PC and PS3? In text data, type name is appended by vertex data type abbreviations of those that are used.
        /*
         * P - Position
         * W - Vertex Weights
         * 4I - 4 Vertex Weight Indices
         * N - Vertex Normal
         * C - Vertex Color
         * T - Texture Coordinates (UVs)
         * A - Vertex Tangent**
         * B - Vertex Binormal**
        
         **Binormals and tangents for each face are calculated and each face's values for a particular vertex are summed and averaged for the result before being normalized
         Though vertex position is used, due to the nature of the normalization applied during the process, resizing is unneeded.
         */
        public class NNS_VTXTYPE_XB
        {
            //While vertex data is technically stored in sequence (Position->Weights->Indices->Normals etc.), we'll store them here in separate lists.
            public List<Vector3> POS_F32_LIST = new List<Vector3>();
            public List<Vector3> WEIGHT3_F32_LIST = new List<Vector3>();
            public List<Vector4> MTXIDX4_I8_LIST = new List<Vector4>();
            public List<Vector3> NRM_F32_LIST = new List<Vector3>();
            public List<byte[]> RGBA8888_LIST = new List<byte[]>();
            public List<Vector2> ST0_F32_UV_LIST = new List<Vector2>(); //UV Coords
            public List<Vector3> TAN_F32_LIST = new List<Vector3>();
            public List<Vector3> BNRM_F32_LIST = new List<Vector3>();
        }
    }
}
