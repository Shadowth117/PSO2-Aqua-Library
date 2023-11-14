using AquaModelLibrary.Extra.Ninja.BillyHatcher.LNDH;
using Reloaded.Memory.Streams;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using static AquaModelLibrary.Extra.Ninja.BillyHatcher.LND;

namespace AquaModelLibrary.Extra.Ninja.BillyHatcher
{
    public class LNDConvert
    {
        public static bool addWeight = false;
        public class ModelData
        {
            public string name = "";
            public NGSAquaObject aqp = new NGSAquaObject();
            /// <summary>
            /// FBX doesn't support multiple vert color sets. Yeah I hate doing this.
            /// </summary>
            public NGSAquaObject nightAqp = null;
            public AquaNode aqn = AquaNode.GenerateBasicAQN();
            public AquaMotion aqm = null;
        }

        public static List<ModelData> ConvertLND(byte[] file)
        {
            using (Stream stream = new MemoryStream(file))
            using (var streamReader = new BufferedStreamReader(stream, 8192))
            {
                return LNDToAqua(new LND(streamReader));
            }
        }

        public static List<ModelData> LNDToAqua(LND lnd)
        {
            if (lnd.isArcLND)
            {
                return ARCLNDToAqua(lnd);
            }
            else
            {
                return LNDHToAqua(lnd);
            }
        }

        public static List<ModelData> ARCLNDToAqua(LND lnd)
        {
            List<ModelData> mdlList = new List<ModelData>();

            //Standard ARCLND models
            foreach (var modelSet in lnd.arcLndModels)
            {
                ModelData mdlData = new ModelData();
                mdlData.name = modelSet.name;
                mdlData.aqp = AddModelData(lnd, modelSet.model);
                if (modelSet.model.arcAltVertColorList?.Count > 0 && modelSet.model.arcAltVertColorList[0].VertColorData.Count > 0)
                {
                    modelSet.model.arcVertDataSetList[0].VertColorData = modelSet.model.arcAltVertColorList[0].VertColorData;
                    mdlData.nightAqp = AddModelData(lnd, modelSet.model);
                }
                mdlList.Add(mdlData);
            }

            //Animated ARCLND models
            for (int i = 0; i < lnd.arcLndAnimatedMeshDataList.Count; i++)
            {
                var modelRef = lnd.arcLndAnimatedMeshDataList[i];
                ModelData mdlData = new ModelData();
                mdlData.name = $"Animated_{i}_{modelRef.MPLAnimId}";
                addWeight = modelRef.mplMotion != null;
                mdlData.aqp = AddModelData(lnd, modelRef.model);
                if (modelRef.motion != null)
                {
                    modelRef.model.arcVertDataSetList[0].VertColorData = modelRef.motion.colorAnimations[0];
                    mdlData.nightAqp = AddModelData(lnd, modelRef.model);
                }
                if (modelRef.mplMotion != null)
                {
                    mdlData.aqp.objc.bonePaletteOffset = 1;
                    mdlData.aqp.bonePalette = new List<uint> { 0 };
                    if(mdlData.nightAqp != null)
                    {
                        mdlData.nightAqp.objc.bonePaletteOffset = 1;
                        mdlData.nightAqp.bonePalette = new List<uint> { 0 };
                    }
                    mdlData.aqm = new AquaMotion();
                    mdlData.aqm.moHeader = new AquaMotion.MOHeader();
                    mdlData.aqm.moHeader.endFrame = modelRef.mplMotion.motionRef.motionInfo0.motionInfo1.motCount - 1;

                    mdlData.aqm.motionKeys = new List<AquaMotion.KeyData>();

                    AquaMotion.KeyData keyData = new AquaMotion.KeyData();
                    keyData.keyData = new List<AquaMotion.MKEY>();
                    AquaMotion.MKEY mkey = new AquaMotion.MKEY();
                    mkey.keyType = 2;

                    for (int key = 0; key < modelRef.mplMotion.motionRef.motionInfo0.motionInfo1.motionData.Count; key++)
                    {
                        mkey.frameTimings.Add((uint)modelRef.mplMotion.motionRef.motionInfo0.motionInfo1.motionData[key].frame * 0x10);
                        switch (modelRef.mplMotion.motionRef.motionInfo0.motionLayout)
                        {
                            case MPL.MPLMotionLayout.Quaternion:
                                mkey.vector4Keys.Add(modelRef.mplMotion.motionRef.motionInfo0.motionInfo1.motionData[key].quatFrame.ToVec4());
                                break;
                            case MPL.MPLMotionLayout.ShortBAMSEuler:
                            case MPL.MPLMotionLayout.ShortBAMSEulerAndExtra:
                                mkey.vector4Keys.Add(MathExtras.EulerToQuaternion(modelRef.mplMotion.motionRef.motionInfo0.motionInfo1.motionData[key].BAMSToDegShorts()).ToVec4());
                                break;
                            case MPL.MPLMotionLayout.IntBAMSEuler:
                            case MPL.MPLMotionLayout.IntBAMSEuler2:
                                mkey.vector4Keys.Add(MathExtras.EulerToQuaternion(modelRef.mplMotion.motionRef.motionInfo0.motionInfo1.motionData[key].BAMSToDegInts()).ToVec4());
                                break;
                        }
                    }
                    keyData.keyData.Add(mkey);
                    mdlData.aqm.motionKeys.Add(keyData);
                }
                addWeight = false;
                mdlList.Add(mdlData);
            }

            return mdlList;
        }

        private static NGSAquaObject AddModelData(LND lnd, ARCLNDModel mdl)
        {
            NGSAquaObject aqp = new NGSAquaObject();
            CreateMaterials(lnd, aqp);
            bool createdDefaultMat = false;
            for (int i = 0; i < mdl.arcMeshDataList.Count; i++)
            {
                for (int m = 0; m < mdl.arcMeshDataList[i].Count; m++)
                {
                    var meshInfo = mdl.arcMeshDataList[i][m];
                    var faceData = mdl.arcFaceDataList[meshInfo.faceDataId];
                    var lndEntry = mdl.arcLandEntryList[meshInfo.lndEntry];
                    var texId = lndEntry.entry.TextureId;

                    AquaObject.GenericTriangles genMesh = new AquaObject.GenericTriangles();
                    Dictionary<string, int> vertTracker = new Dictionary<string, int>();
                    genMesh.name = $"Mesh_{i}_{m}";
                    genMesh.triList = new List<Vector3>();

                    //Create material for textureless meshes
                    if (lndEntry.entry.TextureId == -1)
                    {
                        if (!createdDefaultMat)
                        {
                            CreateDefaultMaterial(aqp);
                            createdDefaultMat = true;
                        }
                        texId = aqp.tempMats.Count - 1;
                    }
                    AddMeshData(mdl, faceData, genMesh, vertTracker, texId);

                    aqp.tempTris.Add(genMesh);
                }
            }

            return aqp;
        }

        private static void AddMeshData(ARCLNDModel mdl, ARCLNDFaceDataHead faceData, AquaObject.GenericTriangles genMesh, Dictionary<string, int> vertTracker, int texId)
        {
            genMesh.triList = new List<Vector3>();
            int f = 0;
            if (faceData.triIndicesList0.Count > 0)
            {
                AddFromARCPolyData(mdl, genMesh, faceData.triIndicesList0, faceData.triIndicesListStarts0, vertTracker, faceData.flags, texId, ref f, 0);
            }
            if (faceData.triIndicesList1.Count > 0)
            {
                AddFromARCPolyData(mdl, genMesh, faceData.triIndicesList1, faceData.triIndicesListStarts1, vertTracker, faceData.flags, texId, ref f, 1);
            }
        }

        private static void AddFromARCPolyData(ARCLNDModel mdl, AquaObject.GenericTriangles genMesh, List<List<List<int>>> triIndicesList, List<List<List<int>>> triIndicesListStarts, Dictionary<string, int> vertTracker, ArcLndVertType flags, int texId, ref int f, int listFlip)
        {
            for (int s = 0; s < triIndicesList.Count; s++)
            {
                var strip = triIndicesList[s];
                var stripStart = triIndicesListStarts[s];
                if (stripStart[0][0] == 0x98)
                {
                    for (int i = 0; i < strip.Count - 2; i++)
                    {
                        AquaObject.VTXL faceVtxl = new AquaObject.VTXL();
                        faceVtxl.rawFaceId.Add(f);
                        faceVtxl.rawFaceId.Add(f);
                        faceVtxl.rawFaceId.Add(f++);

                        int x, y, z;
                        if (((i + listFlip) & 1) > 0)
                        {
                            x = AddARCVert(mdl, faceVtxl, strip[i], flags, genMesh, vertTracker);
                            y = AddARCVert(mdl, faceVtxl, strip[i + 1], flags, genMesh, vertTracker);
                            z = AddARCVert(mdl, faceVtxl, strip[i + 2], flags, genMesh, vertTracker);
                        }
                        else
                        {
                            x = AddARCVert(mdl, faceVtxl, strip[i + 2], flags, genMesh, vertTracker);
                            y = AddARCVert(mdl, faceVtxl, strip[i + 1], flags, genMesh, vertTracker);
                            z = AddARCVert(mdl, faceVtxl, strip[i], flags, genMesh, vertTracker);
                        }
                        genMesh.matIdList.Add(texId);
                        genMesh.triList.Add(new Vector3(x, y, z));
                        faceVtxl.rawVertId.Add(x);
                        faceVtxl.rawVertId.Add(y);
                        faceVtxl.rawVertId.Add(z);

                        genMesh.faceVerts.Add(faceVtxl);
                    }
                }
                else if (stripStart[0][0] == 0x90)
                {
                    for (int i = 0; i < strip.Count - 2; i += 3)
                    {
                        AquaObject.VTXL faceVtxl = new AquaObject.VTXL();
                        faceVtxl.rawFaceId.Add(f);
                        faceVtxl.rawFaceId.Add(f);
                        faceVtxl.rawFaceId.Add(f++);

                        var x = AddARCVert(mdl, faceVtxl, strip[i + 2], flags, genMesh, vertTracker);
                        var y = AddARCVert(mdl, faceVtxl, strip[i + 1], flags, genMesh, vertTracker);
                        var z = AddARCVert(mdl, faceVtxl, strip[i], flags, genMesh, vertTracker);
                        genMesh.triList.Add(new Vector3(x, y, z));
                        faceVtxl.rawVertId.Add(x);
                        faceVtxl.rawVertId.Add(y);
                        faceVtxl.rawVertId.Add(z);

                        genMesh.faceVerts.Add(faceVtxl);
                    }
                }
                else
                {
                    throw new System.Exception();
                }
            }
        }
        public static int AddARCVert(ARCLNDModel mdl, AquaObject.VTXL vtxl, List<int> faceIds, ArcLndVertType flags, AquaObject.GenericTriangles genMesh, Dictionary<string, int> vertTracker)
        {
            string vertId = "";
            int i = 0;
            if ((flags & ArcLndVertType.Position) > 0)
            {
                vtxl.vertPositions.Add(mdl.arcVertDataSetList[0].PositionData[faceIds[i]] / 10);
                vertId += ((int)ArcLndVertType.Position).ToString() + faceIds[i++];
            }
            if ((flags & ArcLndVertType.Normal) > 0)
            {
                vtxl.vertNormals.Add(mdl.arcVertDataSetList[0].NormalData[faceIds[i]]);
                vertId += ((int)ArcLndVertType.Normal).ToString() + faceIds[i++];
            }
            if ((flags & ArcLndVertType.VertColor) > 0)
            {
                var billyColor = (byte[])mdl.arcVertDataSetList[0].VertColorData[faceIds[i]].Clone();
                var temp = billyColor[0];
                billyColor[0] = billyColor[2];
                billyColor[2] = temp;
                vtxl.vertColors.Add(billyColor);
                vertId += ((int)ArcLndVertType.VertColor).ToString() + faceIds[i++];
            }
            if ((flags & ArcLndVertType.VertColor2) > 0)
            {
                var billyColor = (byte[])mdl.arcVertDataSetList[0].VertColor2Data[faceIds[i]].Clone();
                var temp = billyColor[0];
                billyColor[0] = billyColor[2];
                billyColor[2] = temp;
                vtxl.vertColor2s.Add(billyColor);
                vertId += ((int)ArcLndVertType.VertColor2).ToString() + faceIds[i++];
            }
            if ((flags & ArcLndVertType.UV1) > 0)
            {
                var billyUv = mdl.arcVertDataSetList[0].UV1Data[faceIds[i]];
                vtxl.uv1List.Add(new Vector2((float)(billyUv[0] / 255.0), (float)(billyUv[1] / 255.0)));
                vertId += ((int)ArcLndVertType.UV1).ToString() + faceIds[i++];
            }
            if ((flags & ArcLndVertType.UV2) > 0)
            {
                var billyUv = mdl.arcVertDataSetList[0].UV2Data[faceIds[i]];
                vtxl.uv1List.Add(new Vector2((float)(billyUv[0] / 255.0), (float)(billyUv[1] / 255.0)));
                vertId += ((int)ArcLndVertType.UV2).ToString() + faceIds[i++];
            }
            
            if(addWeight)
            {
                vtxl.vertWeightIndices.Add(new int[] { 0,0,0,0 } );
                vtxl.vertWeights.Add(new Vector4(1,0,0,0));
            }

            if (vertTracker.ContainsKey(vertId))
            {
                return vertTracker[vertId];
            }
            else
            {
                vertTracker.Add(vertId, genMesh.vertCount);
                return genMesh.vertCount++;
            }
        }

        public static List<ModelData> LNDHToAqua(LND lnd)
        {
            List<ModelData> mdlList = new List<ModelData>();
            ModelData modelData = new ModelData();
            modelData.aqp = new NGSAquaObject();
            modelData.aqn = AquaNode.GenerateBasicAQN();
            modelData.name = "LNDH";
            //Materials
            CreateMaterials(lnd, modelData.aqp);

            //Get models by node instance
            foreach (var nodeId in lnd.modelNodeIds)
            {
                var node = lnd.nodes[nodeId];
                modelData.aqp.meshNames.Add($"Instance {nodeId}");

                AddMesh(lnd, modelData.aqp, node);
            }
            mdlList.Add(modelData);

            //Get unreferenced models
            for (int i = 0; i < lnd.nodes.Count; i++)
            {
                if (!lnd.modelNodeIds.Contains((ushort)i) && lnd.meshInfo[lnd.nodes[i].objectIndex].lndMeshInfo2Offset != 0)
                {
                    var node = lnd.nodes[i];
                    modelData.aqp.meshNames.Add($"Model {i}");

                    AddMesh(lnd, modelData.aqp, node);
                }
            }

            return mdlList;
        }

        private static void CreateMaterials(LND lnd, NGSAquaObject aqp)
        {
            for (int i = 0; i < lnd.texnames.Count; i++)
            {
                var tex = lnd.texnames[i];
                var mat = new AquaObject.GenericMaterial();
                mat.matName = $"{tex}_{i}";
                mat.texNames = new List<string>() { $"{tex}.png" };
                aqp.tempMats.Add(mat);
            }
            if (lnd.texnames.Count == 0)
            {
                CreateDefaultMaterial(aqp);
            }
        }

        private static void CreateDefaultMaterial(NGSAquaObject aqp)
        {
            var mat = new AquaObject.GenericMaterial();
            mat.matName = $"tex_0";
            mat.texNames = new List<string>() { $"tex0.png" };
            aqp.tempMats.Add(mat);
        }

        private static void AddMesh(LND lnd, NGSAquaObject aqp, LandEntry node)
        {
            var mesh = lnd.meshInfo[node.objectIndex];
            var mesh2 = mesh.lndMeshInfo2;

            AquaObject.GenericTriangles genMesh = new AquaObject.GenericTriangles();
            Dictionary<string, int> vertTracker = new Dictionary<string, int>();
            genMesh.triList = new List<Vector3>();
            int f = 0;
            float[] matColorData = new float[] { 1, 1, 1, 1 };
            foreach (var polyInfo in mesh2.polyInfo0List)
            {
                if (polyInfo.vertIndexMapping.Count == 0)
                {
                    polyInfo.vertIndexMapping = mesh2.polyInfo0List[0].vertIndexMapping;
                    var matColorInfo = polyInfo.GetMaterialInfoByType(0x05000000);
                    if (matColorInfo.HasValue)
                    {
                        matColorData = new float[] { ((float)matColorInfo.Value.matData0 + 1) / 256f, ((float)matColorInfo.Value.matData1 + 1) / 256f, ((float)matColorInfo.Value.matData2 + 1) / 256f, ((float)matColorInfo.Value.matData3 + 1) / 256f };
                    }
                }
                AddFromPolyData(mesh2, genMesh, polyInfo, ref f, matColorData, vertTracker);
            }
            matColorData = new float[] { 1, 1, 1, 1 };
            foreach (var polyInfo in mesh2.polyInfo1List)
            {
                if (polyInfo.vertIndexMapping.Count == 0)
                {
                    polyInfo.vertIndexMapping = mesh2.polyInfo1List[0].vertIndexMapping;
                    var matColorInfo = polyInfo.GetMaterialInfoByType(0x05000000);
                    if (matColorInfo.HasValue)
                    {
                        matColorData = new float[] { ((float)matColorInfo.Value.matData0 + 1) / 256f, ((float)matColorInfo.Value.matData1 + 1) / 256f, ((float)matColorInfo.Value.matData2 + 1) / 256f, ((float)matColorInfo.Value.matData3 + 1) / 256f };
                    }
                }
                AddFromPolyData(mesh2, genMesh, polyInfo, ref f, matColorData, vertTracker);
            }
            aqp.tempTris.Add(genMesh);
        }

        private static void AddFromPolyData(LNDMeshInfo2 mesh2, AquaObject.GenericTriangles genMesh, PolyInfo polyInfo, ref int f, float[] matColorData, Dictionary<string, int> vertTracker)
        {
            int matId = 0;
            var matIdInfoCheck = polyInfo.GetMaterialInfoByType(0x08000000);
            if (matIdInfoCheck.HasValue)
            {
                matId = matIdInfoCheck.Value.matData3;
            }

            var triIndicesList = polyInfo.triIndicesList;
            for (int s = 0; s < triIndicesList.Count; s++)
            {
                var strip = triIndicesList[s];
                var stripStart = polyInfo.triIndicesListStarts[s];
                if (stripStart[0][0] == 0x98)
                {
                    for (int i = 0; i < strip.Count - 2; i++)
                    {
                        AquaObject.VTXL faceVtxl = new AquaObject.VTXL();
                        faceVtxl.rawFaceId.Add(f);
                        faceVtxl.rawFaceId.Add(f);
                        faceVtxl.rawFaceId.Add(f++);

                        int x, y, z;
                        if ((i & 1) > 0)
                        {
                            x = AddVert(mesh2, faceVtxl, strip[i], polyInfo.vertIndexMapping, genMesh, matColorData, vertTracker);
                            y = AddVert(mesh2, faceVtxl, strip[i + 1], polyInfo.vertIndexMapping, genMesh, matColorData, vertTracker);
                            z = AddVert(mesh2, faceVtxl, strip[i + 2], polyInfo.vertIndexMapping, genMesh, matColorData, vertTracker);
                        }
                        else
                        {
                            x = AddVert(mesh2, faceVtxl, strip[i + 2], polyInfo.vertIndexMapping, genMesh, matColorData, vertTracker);
                            y = AddVert(mesh2, faceVtxl, strip[i + 1], polyInfo.vertIndexMapping, genMesh, matColorData, vertTracker);
                            z = AddVert(mesh2, faceVtxl, strip[i], polyInfo.vertIndexMapping, genMesh, matColorData, vertTracker);
                        }
                        genMesh.triList.Add(new Vector3(x, y, z));
                        faceVtxl.rawVertId.Add(x);
                        faceVtxl.rawVertId.Add(y);
                        faceVtxl.rawVertId.Add(z);

                        genMesh.matIdList.Add(matId);
                        genMesh.faceVerts.Add(faceVtxl);
                    }
                }
                else if (stripStart[0][0] == 0x90)
                {
                    for (int i = 0; i < strip.Count - 2; i += 3)
                    {
                        AquaObject.VTXL faceVtxl = new AquaObject.VTXL();
                        faceVtxl.rawFaceId.Add(f);
                        faceVtxl.rawFaceId.Add(f);
                        faceVtxl.rawFaceId.Add(f++);

                        var x = AddVert(mesh2, faceVtxl, strip[i + 2], polyInfo.vertIndexMapping, genMesh, matColorData, vertTracker);
                        var y = AddVert(mesh2, faceVtxl, strip[i + 1], polyInfo.vertIndexMapping, genMesh, matColorData, vertTracker);
                        var z = AddVert(mesh2, faceVtxl, strip[i], polyInfo.vertIndexMapping, genMesh, matColorData, vertTracker);
                        genMesh.triList.Add(new Vector3(x, y, z));
                        faceVtxl.rawVertId.Add(x);
                        faceVtxl.rawVertId.Add(y);
                        faceVtxl.rawVertId.Add(z);

                        genMesh.matIdList.Add(matId);
                        genMesh.faceVerts.Add(faceVtxl);
                    }
                }
            }
        }

        public static int AddVert(LNDMeshInfo2 mesh, AquaObject.VTXL vtxl, List<int> faceIds, Dictionary<int, int> vertIndexMapping, AquaObject.GenericTriangles genMesh, float[] matColorData, Dictionary<string, int> vertTracker)
        {
            string vertId = "";
            for (int i = 0; i < mesh.layouts.Count; i++)
            {
                var lyt = mesh.layouts[i];
                vertId += lyt.vertType.ToString() + faceIds[vertIndexMapping[i]];
                switch (lyt.vertType)
                {
                    case 0x1:
                        vtxl.vertPositions.Add(mesh.vertData.vertPositions[faceIds[vertIndexMapping[i]]] / 10);
                        break;
                    case 0x2:
                        //mesh.vertData.vert2Data.Add(new byte[] { sr.Read<byte>(), sr.Read<byte>(), sr.Read<byte>() });
                        break;
                    case 0x3:
                        var color = mesh.vertData.vertColorData[faceIds[vertIndexMapping[i]]];
                        int r = color >> 12;
                        int g = (color >> 8) & 0xF;
                        int b = (color >> 4) & 0xF;
                        int a = color & 0xF;
                        a = a | (a << 4);
                        r = r | (r << 4);
                        g = g | (g << 4);
                        b = b | (b << 4);

                        //Apply material coloring since it's per polyinfo. Bit excessive. But also not sure it's worthwhile
                        /*
                        b = (int)(((((float)b + 1) / 256f) * matColorData[0]) * 256 - 1);
                        g = (int)(((((float)g + 1) / 256f) * matColorData[1]) * 256 - 1);
                        r = (int)(((((float)r + 1) / 256f) * matColorData[2]) * 256 - 1);
                        a = (int)(((((float)a + 1) / 256f) * matColorData[3]) * 256 - 1);
                        */

                        vtxl.vertColors.Add(new byte[] { (byte)b, (byte)g, (byte)r, (byte)a });
                        break;
                    case 0x5:
                        var uvRaw = mesh.vertData.vertUVData[faceIds[vertIndexMapping[i]]];
                        var U = (float)(uvRaw[0] / 255.0);
                        var V = (float)(uvRaw[1] / 255.0);
                        vtxl.uv1List.Add(new Vector2(U, V));
                        break;
                    default:
                        throw new System.Exception($"Unk Vert type: {lyt.vertType:X} Data type: {lyt.dataType:X}");
                }
            }

            if (vertTracker.ContainsKey(vertId))
            {
                return vertTracker[vertId];
            }
            else
            {
                vertTracker.Add(vertId, genMesh.vertCount);
                return genMesh.vertCount++;
            }
        }
    }
}
