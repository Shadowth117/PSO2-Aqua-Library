using AquaModelLibrary.AquaMethods;
using AquaModelLibrary.NNStructs.Structures;
using Reloaded.Memory.Streams;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Text;

namespace AquaModelLibrary.NNStructs
{
    //UNJ file, found in Phantasy Star Portable, Phantasy Star Portable 2, and Phantasy Star Portable 2 Infinity
    //Credit to Kion and Agrajag for playing around with this format
    public class UNJObject
    {
        //Vert data formats
        public const int UINT8 = 1;
        public const int INT8 = 1;
        public const int INT16 = 2;
        public const int FLOAT = 3;

        public BufferedStreamReader streamReader = null;
        public int offset = 0;
        public UNJHeader header;
        public AquaNode nodes; //Technically not the same, but it's close enough we can store everything PSU really has here
        public Dictionary<int, int> animNodeMap = null;
        public List<byte[]> nodeMetaData = new List<byte[]>();

        public List<AquaObject.VTXL> vtxlList = new List<AquaObject.VTXL>();
        public List<UNJMaterial> materials = new List<UNJMaterial>();
        public List<string> texList = new List<string>();
        public List<UNJDrawGroup> drawGroups = new List<UNJDrawGroup>();
        public List<List<UNJDirectDrawMesh>> drawMeshes = new List<List<UNJDirectDrawMesh>>();

        public void ReadUNJ(string filePath)
        {
            streamReader = new BufferedStreamReader(new MemoryStream(File.ReadAllBytes(filePath)), 8192);

            var magic = Encoding.ASCII.GetString(streamReader.ReadBytes(0, 4));
            if(magic == "UNJ\0")
            {
                streamReader.Seek(4, SeekOrigin.Begin);
                var len = streamReader.Read<int>();
                offset = len;
                streamReader.Seek(len, SeekOrigin.Begin);
            }
            streamReader.Seek(4, SeekOrigin.Current);
            var nnLen = streamReader.Read<int>();
            var headerOffset = streamReader.Read<int>();
            var version = streamReader.Read<int>(); //Always 3?

            streamReader.Seek(offset + headerOffset, SeekOrigin.Begin);
            header = streamReader.Read<UNJHeader>();

            var texPath = Path.ChangeExtension(filePath, ".unt");
            if (File.Exists(texPath))
            {
                texList = NNTextureList.ReadNNT(texPath);
            }
            ReadBones(Path.ChangeExtension(filePath, ".una"));
            ReadVerts();
            ReadMaterials();
            ReadDrawGroups();
        }

        //Returns an aqp ready for the ConvertToNGSPSO2Mesh method
        public AquaObject ConvertToBasicAquaobject(out AquaNode aqn)
        {
            List<List<int>> globalVtxlMapping = new List<List<int>>();
            List<AquaObject.VTXL> newVtxlList = new List<AquaObject.VTXL>();
            aqn = nodes;
            AquaObject aqp = new NGSAquaObject();
            aqp.bonePalette = new List<uint>();
            for(int i = 0; i < aqn.nodeList.Count; i++)
            {
                aqp.bonePalette.Add((uint)i);
            }

            //Optimize vert data
            //Loop through vertices and check if their data matches. If it does, set map to the existing dat in the new vertex list. If not, append and set new mapping accordingly
            //This is done due to massive redundancy from UNJ vertices being listed in tristrip draw order already. 
            for(int i = 0; i < vtxlList.Count; i++)
            {
                var curVtxl = vtxlList[i];
                AquaObject.VTXL newVtxl = new AquaObject.VTXL();
                List<int> vtxlMapping = new List<int>();

                for(int v = 0; v < curVtxl.vertPositions.Count; v++)
                {
                    int map = -1;
                    for (int j = 0; j < newVtxl.vertPositions.Count; j++)
                    {
                        if (AquaObjectMethods.IsSameVertex(curVtxl, v, newVtxl, j))
                        {
                            map = j;
                            break;
                        }
                    }

                    if (map == -1)
                    {
                        map = newVtxl.vertPositions.Count;
                        AquaObjectMethods.appendVertex(curVtxl, newVtxl, v);
                    }

                    vtxlMapping.Add(map);
                }

                newVtxlList.Add(newVtxl);
                globalVtxlMapping.Add(vtxlMapping);
            }

            //Convert material data
            for(int i = 0; i < materials.Count; i++)
            {
                var curMat = materials[i];
                AquaObject.GenericMaterial genMat = new AquaObject.GenericMaterial();
                genMat.diffuseRGBA = curMat.diffuseColor;
                genMat.matName = $"PSPMaterial_{i}";
                genMat.texNames = new List<string>();
                for(int t = 0; t < curMat.diffuseTexIds.Count; t++)
                {
                    string name = $"tex_{t}";
                    if(texList.Count - 1 > t)
                    {
                        name = texList[curMat.diffuseTexIds[t]];
                    }
                    genMat.texNames.Add(Path.ChangeExtension(name, ".dds"));
                }
                aqp.tempMats.Add(genMat);
            }

            //Create mesh data
            for(int i = 0; i < drawGroups.Count; i++)
            {
                for(int j = 0; j < drawMeshes[i].Count; j++)
                {
                    var group = drawMeshes[i][j].groupId;
                    var matId = drawMeshes[i][j].matId;
                    AquaObject.GenericTriangles genMesh = new AquaObject.GenericTriangles();
                    genMesh.vertCount = newVtxlList[group].vertPositions.Count;

                    //Generate strips
                    //UNJ vertices are stored in strip order as far as I know. Therefore, we generate the strips here
                    bool flip = true;
                    for(int v = 0; v < vtxlList[group].vertPositions.Count - 2; v++)
                    {
                        flip = !flip;

                        int a, b, c;
                        if(!flip)
                        {
                            a = globalVtxlMapping[group][v];
                            b = globalVtxlMapping[group][v + 1];
                            c = globalVtxlMapping[group][v + 2];
                        } else
                        {
                            b = globalVtxlMapping[group][v];
                            a = globalVtxlMapping[group][v + 1];
                            c = globalVtxlMapping[group][v + 2];
                        }

                        //Avoid degenerate faces
                        if(a == b || b == c || a == c)
                        {
                            continue;
                        }
                        genMesh.matIdList.Add(matId);
                        genMesh.triList.Add(new Vector3(a, b, c));
                    }
                    aqp.vtxlList.Add(newVtxlList[group].Clone());
                    aqp.tempTris.Add(genMesh);
                }
            }

            return aqp;
        }

        public void ReadDrawGroups()
        {
            drawMeshes = new List<List<UNJDirectDrawMesh>>();
            streamReader.Seek(offset + header.drawOffset, SeekOrigin.Begin);
            for (int i = 0; i < header.drawCount; i++)
            {
                drawGroups.Add(streamReader.Read<UNJDrawGroup>());
                var bookmark = streamReader.Position();

                drawMeshes.Add(new List<UNJDirectDrawMesh>());
                streamReader.Seek(offset + drawGroups[i].directDrawOffset, SeekOrigin.Begin);
                for (int j = 0; j < drawGroups[i].directDrawCount; j++)
                {
                    drawMeshes[i].Add(streamReader.Read<UNJDirectDrawMesh>());
                }
                streamReader.Seek(bookmark, SeekOrigin.Begin);
            }
        }

        public void ReadMaterials()
        {
            List<UNJMaterialHeader> matHeaders = new List<UNJMaterialHeader>();
            streamReader.Seek(offset + header.matOffset, SeekOrigin.Begin);
            for (int i = 0; i < header.matCount; i++)
            {
                matHeaders.Add(streamReader.Read<UNJMaterialHeader>());
            }
            for (int i = 0; i < header.matCount; i++)
            {
                streamReader.Seek(offset + matHeaders[i].materialOffset, SeekOrigin.Begin);
                materials.Add(ReadMaterial(matHeaders[i]));
            }
        }

        public UNJMaterial ReadMaterial(UNJMaterialHeader matHeader)
        {
            UNJMaterial mat = new UNJMaterial();
            mat.disablingFlags = streamReader.Read<short>();
            mat.unkFlags = streamReader.Read<short>();
            mat.int_04 = streamReader.Read<int>();
            mat.int_08 = streamReader.Read<int>();

            for(int i = 0; i < matHeader.diffuseTexCount; i++)
            {
                byte[] rawMapSettings = streamReader.ReadBytes(streamReader.Position(), 4);
                streamReader.Seek(4, SeekOrigin.Current);
                mat.diffuseMapBytesRaw.Add(rawMapSettings);
                if(rawMapSettings[1] == 1)
                {
                    mat.diffuseMapTypes.Add("env");
                } else
                {
                    mat.diffuseMapTypes.Add("uv");
                }
            }

            for(int i = 0; i < 6; i++)
            {
                byte[] colorProp = streamReader.ReadBytes(streamReader.Position(), 4);
                streamReader.Seek(4, SeekOrigin.Current);

                switch(colorProp[3])
                {
                    case 0x54:
                        mat.emissiveColor.X = (float)colorProp[0] / 255;
                        mat.emissiveColor.Y = (float)colorProp[1] / 255;
                        mat.emissiveColor.Z = (float)colorProp[2] / 255;
                        break;
                    case 0x55:
                        mat.ambientColor.X = (float)colorProp[0] / 255;
                        mat.ambientColor.Y = (float)colorProp[1] / 255;
                        mat.ambientColor.Z = (float)colorProp[2] / 255;
                        break;
                    case 0x56:
                        mat.diffuseColor.X = (float)colorProp[0] / 255;
                        mat.diffuseColor.Y = (float)colorProp[1] / 255;
                        mat.diffuseColor.Z = (float)colorProp[2] / 255;
                        break;
                    case 0x57:
                        mat.specularColor.X = (float)colorProp[0] / 255;
                        mat.specularColor.Y = (float)colorProp[1] / 255;
                        mat.specularColor.Z = (float)colorProp[2] / 255;
                        break;
                    case 0x58:
                        mat.emissiveColor.W = (float)colorProp[0] / 255;
                        mat.ambientColor.W = (float)colorProp[0] / 255;
                        mat.diffuseColor.W = (float)colorProp[0] / 255;
                        break;
                    case 0x59:
                        //mat.specularValue = BitConverter.ToSingle(colorProp, 0);
                        break;
                }
            }

            mat.unknRange = streamReader.ReadBytes(streamReader.Position(), 0x18);
            streamReader.Seek(0x18, SeekOrigin.Current);

            for (int i = 0; i < matHeader.diffuseTexCount; i++)
            {
                UNJTextureProperties props = new UNJTextureProperties();
                byte[] rawProp = streamReader.ReadBytes(streamReader.Position(), 4);
                streamReader.Seek(4, SeekOrigin.Current);

                while(rawProp[3] != 0x0B)
                {
                    switch (rawProp[3])
                    {
                        case 0x21:
                            props.disableLighting = rawProp[0] == 1;
                            break;
                        case 0x22:
                            props.alphaTestEnabled = rawProp[0] == 1;
                            break;
                        case 0x23:
                            props.zTestEnabled = rawProp[0] == 1;
                            break;
                        case 0x24:
                            props.stencilTestEnabled = rawProp[0] == 1;
                            break;
                        case 0x27:
                            props.colorTestEnabled = rawProp[0] == 1;
                            break;
                        case 0x28:
                            props.logicOpEnabled = rawProp[0] == 1;
                            break;
                        case 0x48:
                            props.scaleU = BitConverter.ToSingle(rawProp, 0);
                            break;
                        case 0x49:
                            props.scaleV = BitConverter.ToSingle(rawProp, 0);
                            break;
                        case 0x4A:
                            props.offsetU = BitConverter.ToSingle(rawProp, 0);
                            break;
                        case 0x4B:
                            props.offsetV = BitConverter.ToSingle(rawProp, 0);
                            break;
                        case 0x5E:
                            props.diffuseEnabled = rawProp[0];
                            break;
                        case 0xC7:
                            props.clampU = (rawProp[0] & 1) != 0;
                            props.clampV = (rawProp[1] & 1) != 0;
                            break;
                        case 0xC9:
                            props.textureFunction = (rawProp[0] & 7);
                            props.textureFunctionUsesAlpha = rawProp[1] == 1;
                            break;
                        case 0xDB:
                            props.alphaFunction = rawProp[0];
                            props.alphaRef = rawProp[1];
                            break;
                        case 0xDE:
                            props.zTestFunction = rawProp[0];
                            break;
                        case 0xDF:
                            props.blendMode = rawProp[1];
                            props.blendFactorA = rawProp[0];
                            props.blendFactorB = rawProp[0];
                            break;
                        case 0xE0:
                            props.blendFixedA = (rawProp[2] << 16) | (rawProp[1] << 8) | rawProp[0];
                            break;
                        case 0xE1:
                            props.blendFixedB = (rawProp[2] << 16) | (rawProp[1] << 8) | rawProp[0];
                            break;
                        case 0xE6:
                            props.logicOp = rawProp[0];
                            break;
                        case 0xE7:
                            props.zWriteDisabled = rawProp[0] > 0 ? true : false;
                            break;
                        case 0xE8:
                            props.maskRGB = (rawProp[2] << 16) | (rawProp[1] << 8) | rawProp[0];
                            break;
                        case 0xE9:
                            props.maskAlpha = (rawProp[2] << 16) | (rawProp[1] << 8) | rawProp[0];
                            break;
                        case 0x0B:
                            break;
                        default:
                            Trace.WriteLine($"Unknown material property {rawProp[3]}");
                            break;
                    }
                    rawProp = streamReader.ReadBytes(streamReader.Position(), 4);
                    streamReader.Seek(4, SeekOrigin.Current);
                }

                mat.propertyList.Add(props);
            }

            for(int i = 0; i < matHeader.diffuseTexCount; i++)
            {
                mat.diffuseTexIds.Add(streamReader.Read<int>());
            }

            return mat;
        }

        public void ReadVerts()
        {
            streamReader.Seek(offset + header.vertGroupOffset, SeekOrigin.Begin);
            var vertGroupHeads = new List<UNJVertGroupHeader>();
            var vertGroupInfos = new List<UNJVertGroupInfo>();
            var vertGroupCounts = new List<int>();
            var bonePalettes = new List<List<int>>();

            for(int i = 0; i < header.vertGroupCount; i++)
            {
                var vertGroupHead = streamReader.Read<UNJVertGroupHeader>();
                vertGroupHeads.Add(vertGroupHead);
                var bookmark = streamReader.Position();

                streamReader.Seek(offset + vertGroupHead.vtxlOffset, SeekOrigin.Begin);
                var info = streamReader.Read<UNJVertGroupInfo>();
                if (info.vertScale == 0)
                {
                    info.vertScale = 1;
                }
                vertGroupInfos.Add(info);

                streamReader.Seek(offset + info.vertCountOffset, SeekOrigin.Begin);
                vertGroupCounts.Add(streamReader.Read<int>());

                streamReader.Seek(offset + info.bonePaletteOffset, SeekOrigin.Begin);
                bonePalettes.Add(new List<int>());
                for(int j = 0; j < info.bonePaletteCount; j++)
                {
                    bonePalettes[i].Add(streamReader.Read<int>());
                }

                vtxlList.Add(ReadUNJVTXL(bonePalettes[i], info, vertGroupHead, vertGroupCounts[i]));

                streamReader.Seek(bookmark, SeekOrigin.Begin);
            }

        }

        public AquaObject.VTXL ReadUNJVTXL(List<int> bonePalette, UNJVertGroupInfo info, UNJVertGroupHeader vertHeader, int vertCount)
        {
            AquaObject.VTXL vtxl = new AquaObject.VTXL();
            int boneCount = bonePalette.Count;
            int uvFormat = (info.vertFormat & 0x3);
            int colorFormat = (info.vertFormat >> 2) & 0x7;
            int normalFormat = (info.vertFormat >> 5) & 0x3;
            int positionFormat = (info.vertFormat >> 7) & 0x3;
            int weightFormat = (info.vertFormat >> 9) & 0x3;

            streamReader.Seek(offset + info.vertListOffset, SeekOrigin.Begin);
            var startPos = streamReader.Position();
            for (int i = 0; i < vertCount; i++)
            {
                //Weight reading
                Vector4 weights = new Vector4();
                int[] weightIndices = new int[4];
                for (int j = 0; j < boneCount; j++)
                {
                    weightIndices[j] = animNodeMap[bonePalette[j]];
                    switch (weightFormat)
                    {
                        case UINT8:
                            switch (j)
                            {
                                case 0:
                                    weights.X = ((float)streamReader.Read<sbyte>()) / 0x7F;
                                    break;
                                case 1:
                                    weights.Y = ((float)streamReader.Read<sbyte>()) / 0x7F;
                                    break;
                                case 2:
                                    weights.Z = ((float)streamReader.Read<sbyte>()) / 0x7F;
                                    break;
                                case 3:
                                    weights.W = ((float)streamReader.Read<sbyte>()) / 0x7F;
                                    break;
                            }
                            break;
                        case INT16:
                            switch (j)
                            {
                                case 0:
                                    weights.X = ((float)streamReader.Read<short>()) / 0x7FFF;
                                    break;
                                case 1:
                                    weights.Y = ((float)streamReader.Read<short>()) / 0x7FFF;
                                    break;
                                case 2:
                                    weights.Z = ((float)streamReader.Read<short>()) / 0x7FFF;
                                    break;
                                case 3:
                                    weights.W = ((float)streamReader.Read<short>()) / 0x7FFF;
                                    break;
                            }
                            break;
                        case FLOAT:
                            switch (j)
                            {
                                case 0:
                                    weights.X = streamReader.Read<float>();
                                    break;
                                case 1:
                                    weights.Y = streamReader.Read<float>();
                                    break;
                                case 2:
                                    weights.Z = streamReader.Read<float>();
                                    break;
                                case 3:
                                    weights.W = streamReader.Read<float>();
                                    break;
                            }
                            break;
                        default:
                            weights.X = 1;
                            break;
                    }
                }
                vtxl.vertWeights.Add(weights);
                vtxl.vertWeightIndices.Add(weightIndices);

                //UVs
                for(int j = 0; j < vertHeader.uvCount; j++)
                {
                    List<Vector2> uvList;
                    switch(j)
                    {
                        case 0:
                            uvList = vtxl.uv1List;
                            break;
                        case 1:
                            uvList = vtxl.uv2List;
                            break;
                        case 2:
                            uvList = vtxl.uv3List;
                            break;
                        case 3:
                            uvList = vtxl.uv4List;
                            break;
                        default:
                            throw new System.Exception($"Unexpected uv count: {vertHeader.uvCount}");
                    }
                    switch (uvFormat)
                    {
                        case INT8:
                            uvList.Add(new Vector2(((float)streamReader.Read<sbyte>()) / 0x7F, ((float)streamReader.Read<sbyte>()) / 0x7F));
                            break;
                        case INT16:
                            AquaGeneralMethods.AlignReader(streamReader, 0x2);
                            uvList.Add(new Vector2(((float)streamReader.Read<short>()) / 0x7FFF, ((float)streamReader.Read<short>()) / 0x7FFF));
                            break;
                        case FLOAT:
                            AquaGeneralMethods.AlignReader(streamReader, 0x4);
                            uvList.Add(new Vector2(streamReader.Read<float>(), streamReader.Read<float>()));
                            break;
                    }
                }

                //Vert Colors
                if(colorFormat != 0)
                {
                    byte[] color = new byte[4];
                    AquaGeneralMethods.AlignReader(streamReader, 0x2);
                    byte byte1 = streamReader.Read<byte>();
                    byte byte2 = streamReader.Read<byte>();
                    color[2] = (byte)(((byte1 >> 0) & 0xf) / 0x0f);
                    color[1] = (byte)(((byte1 >> 4) & 0xf) / 0x0f);
                    color[0] = (byte)(((byte2 >> 0) & 0xf) / 0x0f);
                    color[3] = (byte)(((byte2 >> 4) & 0xf) / 0x0f);
                    
                    vtxl.vertColors.Add(color);
                }

                //Normals
                switch (normalFormat)
                {
                    case INT8:
                        vtxl.vertNormals.Add(new Vector3(((float)streamReader.Read<sbyte>()) / 0x7F, ((float)streamReader.Read<sbyte>()) / 0x7F, ((float)streamReader.Read<sbyte>()) / 0x7F));
                        break;
                    case INT16:
                        AquaGeneralMethods.AlignReader(streamReader, 0x2);
                        vtxl.vertNormals.Add(new Vector3(((float)streamReader.Read<short>()) / 0x7FFF, ((float)streamReader.Read<short>()) / 0x7FFF, ((float)streamReader.Read<short>()) / 0x7FFF));
                        break;
                    case FLOAT:
                        AquaGeneralMethods.AlignReader(streamReader, 0x4);
                        vtxl.vertNormals.Add(new Vector3(streamReader.Read<float>(), streamReader.Read<float>(), streamReader.Read<float>()));
                        break;
                    default:
                        break;
                }

                //Position
                switch (positionFormat)
                {
                    case INT8:
                        vtxl.vertPositions.Add(new Vector3(((float)streamReader.Read<sbyte>()) / 0x7F, ((float)streamReader.Read<sbyte>()) / 0x7F, ((float)streamReader.Read<sbyte>()) / 0x7F) * info.vertScale);
                        break;
                    case INT16:
                        AquaGeneralMethods.AlignReader(streamReader, 0x2);
                        vtxl.vertPositions.Add(new Vector3(((float)streamReader.Read<short>()) / 0x7FFF, ((float)streamReader.Read<short>()) / 0x7FFF, ((float)streamReader.Read<short>()) / 0x7FFF) * info.vertScale);
                        break;
                    case FLOAT:
                        AquaGeneralMethods.AlignReader(streamReader, 0x4);
                        vtxl.vertPositions.Add(new Vector3(streamReader.Read<float>(), streamReader.Read<float>(), streamReader.Read<float>()) * info.vertScale);
                        break;
                }

                //Stride check
                if(i == 0 && streamReader.Position() - (offset + info.vertListOffset) != info.vertSize)
                {
                    throw new System.Exception($"Phission Mailed, better luck next time\n Start Position: {startPos.ToString("X")} End Position: {streamReader.Position().ToString("X")} ");
                }
            }

            return vtxl;
        }

        public void ReadBones(string filePath = null)
        {
            List<string> boneNames = null;
            if(filePath != null)
            {
                if(File.Exists(filePath))
                {
                    boneNames = NNNodeNames.ReadNNA(filePath);
                }
            }
            nodes = new AquaNode();
            Dictionary<int, AquaNode.NODE> nodeDict = new Dictionary<int, AquaNode.NODE>();
            Dictionary<int, int> nodeIntDict = new Dictionary<int, int>();

            streamReader.Seek(offset + header.boneOffset, SeekOrigin.Begin);
            for(int i = 0; i < header.boneCount; i++)
            {
                var node = new AquaNode.NODE();
                if(boneNames.Count > i)
                {
                    node.boneName.SetString(boneNames[i]);
                } else
                {
                    node.boneName.SetString($"({i}) Node_{i}");
                }
                node.boneShort1 = streamReader.Read<ushort>();
                node.boneShort2 = streamReader.Read<ushort>();
                var mapId = streamReader.Read<short>();
                node.parentId = streamReader.Read<short>();
                node.firstChild = streamReader.Read<short>();
                node.nextSibling = streamReader.Read<short>();
                node.pos = streamReader.Read<Vector3>();
                Vector3 eulRot = new Vector3();
                eulRot.X = streamReader.Read<int>() / 0x8000 * 180;
                eulRot.Y = streamReader.Read<int>() / 0x8000 * 180;
                eulRot.Z = streamReader.Read<int>() / 0x8000 * 180;
                node.eulRot = eulRot;
                node.scale = streamReader.Read<Vector3>();
                node.m1 = streamReader.Read<Vector4>();
                node.m2 = streamReader.Read<Vector4>();
                node.m3 = streamReader.Read<Vector4>();
                node.m4 = streamReader.Read<Vector4>();
                nodeMetaData.Add(streamReader.ReadBytes(streamReader.Position(), 0x20));
                streamReader.Seek(0x20, SeekOrigin.Current);

                if (mapId >= 0)
                {
                    nodeDict.Add(mapId, node);
                    nodeIntDict.Add(mapId, i);
                }
                nodes.nodeList.Add(node);
            }
            animNodeMap = nodeIntDict;
        }


    }
}
