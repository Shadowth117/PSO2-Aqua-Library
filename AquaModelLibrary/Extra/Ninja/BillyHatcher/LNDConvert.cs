using AquaModelLibrary.Extra.Ninja.BillyHatcher.LNDH;
using Reloaded.Memory.Streams;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Windows.Forms;

namespace AquaModelLibrary.Extra.Ninja.BillyHatcher
{
    public class LNDConvert
    {
        /// <summary>
        /// In case we need to optimize more, we can let the game handle this more by setting this false, like the game normally does with normalless vertices. This would optimize the file a tad and may improve the load that can be applied to the game.
        /// </summary>
        public static bool assignNormalsOnExport = true;
        /// <summary>
        /// Normally Billy matches the the position optimization with these so that night colors can be synced, but vertex colors can be optimized too to save more space, technically.
        /// If this is set, the night vertex colors will be copied from the standard vertex colors of the model.
        /// </summary>
        public static bool optimizeVertColors = false;
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

            public NGSAquaObject placementAqp = null;
            public AquaNode placementAqn = null;
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
                var boundData = modelRef.model.arcBoundingList[0];

                ModelData mdlData = new ModelData();

                //Set up bounding data transform
                var bndRot = boundData.GetRotation();
                var rot = MathExtras.EulerToQuaternionRadian(bndRot);
                var mat = MathExtras.Compose(boundData.Position, rot, boundData.Scale);
                Matrix4x4.Invert(mat, out var invMat);

                mdlData.name = $"animModel-{i}-{modelRef.MPLAnimId}";
                addWeight = modelRef.mplMotion != null;
                mdlData.aqp = AddModelData(lnd, modelRef.model);
                mdlData.placementAqp = AddModelData(lnd, modelRef.model);
                if (modelRef.motion != null)
                {
                    modelRef.model.arcVertDataSetList[0].VertColorData = modelRef.motion.colorAnimations[0];
                    mdlData.nightAqp = AddModelData(lnd, modelRef.model);
                }

                //Set up transformed Animated Model. We do it this way to keep the animation clean
                mdlData.placementAqn = AquaNode.GenerateBasicAQN();
                AquaNode.NODE node = new AquaNode.NODE();
                node.boneName = new AquaCommon.PSO2String();
                node.boneName.SetString($"Animated_{i}_Root");
                node.SetInverseBindPoseMatrix(invMat);
                mdlData.placementAqn.nodeList.Add(node);

                foreach (var group in mdlData.placementAqp.tempTris)
                {
                    foreach (var vtxl in group.faceVerts)
                    {
                        for (int y = 0; y < vtxl.vertPositions.Count; y++)
                        {
                            vtxl.vertPositions[y] = Vector3.Transform(vtxl.vertPositions[y], mat);
                        }
                        for (int y = 0; y < vtxl.vertNormals.Count; y++)
                        {
                            vtxl.vertNormals[y] = Vector3.TransformNormal(vtxl.vertNormals[y], mat);
                        }
                        for (int y = 0; y < vtxl.vertWeightIndices.Count; y++)
                        {
                            vtxl.vertWeightIndices[y][0] = 1;
                        }
                    }
                }
                if (mdlData.placementAqp != null)
                {
                    mdlData.placementAqp.objc.bonePaletteOffset = 1;
                    mdlData.placementAqp.bonePalette = new List<uint> { 0, 1 };
                }

                if (modelRef.mplMotion != null)
                {
                    mdlData.aqp.objc.bonePaletteOffset = 1;
                    mdlData.aqp.bonePalette = new List<uint> { 0 };
                    if (mdlData.nightAqp != null)
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
            //Has mat name as key and the list id as the value
            Dictionary<string, int> materialDict = new Dictionary<string, int>();
            for (int i = 0; i < mdl.arcMeshDataList.Count; i++)
            {
                for (int m = 0; m < mdl.arcMeshDataList[i].Count; m++)
                {
                    var meshInfo = mdl.arcMeshDataList[i][m];
                    var faceData = mdl.arcFaceDataList[meshInfo.faceDataId];
                    var mat = mdl.arcMatEntryList[meshInfo.matEntryId];
                    var texId = mat.entry.TextureId;

                    AquaObject.GenericTriangles genMesh = new AquaObject.GenericTriangles();
                    Dictionary<string, int> vertTracker = new Dictionary<string, int>();
                    genMesh.name = $"Mesh_{i}_{m}";
                    aqp.meshNames.Add(genMesh.name);
                    genMesh.triList = new List<Vector3>();

                    //Create material
                    var matName = "Mat";
                    foreach (var flag in Enum.GetValues(typeof(ARCLNDRenderFlags)))
                    {
                        if ((mat.entry.RenderFlags & (ARCLNDRenderFlags)flag) > 0)
                        {
                            matName += $"#{flag}";
                        }
                    }
                    foreach (var flag in Enum.GetValues(typeof(ARCLNDTextureFlags)))
                    {
                        if ((mat.entry.textureFlags & (ARCLNDTextureFlags)flag) > 0)
                        {
                            matName += $"#{flag}";
                        }
                    }
                    matName += $"#S_{mat.entry.sourceAlpha}";
                    matName += $"#D_{mat.entry.destinationAlpha}";
                    int matId;
                    var tex = texId < 0 ? lnd.texnames[0] : lnd.texnames[texId];
                    if (materialDict.ContainsKey(matName + $"#{tex}"))
                    {
                        matId = materialDict[matName + $"#{tex}"];
                    }
                    else
                    {
                        var genMat = new AquaObject.GenericMaterial();
                        genMat.matName = matName;
                        genMat.diffuseRGBA = new Vector4(1, 1, 1, 1);
                        genMat.texNames = new List<string>() { $"{tex}.png" };
                        materialDict.Add(matName + $"#{tex}", aqp.tempMats.Count);
                        matId = aqp.tempMats.Count;
                        aqp.tempMats.Add(genMat);
                    }

                    AddMeshData(mdl, faceData, genMesh, vertTracker, matId);

                    aqp.tempTris.Add(genMesh);
                }
            }

            for(int i = 0; i < aqp.tempMats.Count; i++)
            {
                var mat = aqp.tempMats[i];
                mat.matName += $"{i:D3}";
            }

            return aqp;
        }

        private static void AddMeshData(ARCLNDModel mdl, ARCLNDFaceDataHead faceData, AquaObject.GenericTriangles genMesh, Dictionary<string, int> vertTracker, int texId)
        {
            genMesh.triList = new List<Vector3>();
            int f = 0;
            if (faceData.triIndicesList0.Count > 0)
            {
                AddFromARCPolyData(mdl, genMesh, faceData, faceData.triIndicesList0, faceData.triIndicesListStarts0, vertTracker, faceData.flags, texId, ref f, 0);
            }
            if (faceData.triIndicesList1.Count > 0)
            {
                AddFromARCPolyData(mdl, genMesh, faceData, faceData.triIndicesList1, faceData.triIndicesListStarts1, vertTracker, faceData.flags, texId, ref f, 1);
            }
        }

        private static void AddFromARCPolyData(ARCLNDModel mdl, AquaObject.GenericTriangles genMesh, ARCLNDFaceDataHead faceData, List<List<List<int>>> triIndicesList, List<List<List<int>>> triIndicesListStarts, Dictionary<string, int> vertTracker, ArcLndVertType flags, int texId, ref int f, int listFlip)
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
                            x = AddARCVert(mdl, faceData, faceVtxl, strip[i], flags, genMesh, vertTracker);
                            y = AddARCVert(mdl, faceData, faceVtxl, strip[i + 1], flags, genMesh, vertTracker);
                            z = AddARCVert(mdl, faceData, faceVtxl, strip[i + 2], flags, genMesh, vertTracker);
                        }
                        else
                        {
                            x = AddARCVert(mdl, faceData, faceVtxl, strip[i + 2], flags, genMesh, vertTracker);
                            y = AddARCVert(mdl, faceData, faceVtxl, strip[i + 1], flags, genMesh, vertTracker);
                            z = AddARCVert(mdl, faceData, faceVtxl, strip[i], flags, genMesh, vertTracker);
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

                        var x = AddARCVert(mdl, faceData, faceVtxl, strip[i + 2], flags, genMesh, vertTracker);
                        var y = AddARCVert(mdl, faceData, faceVtxl, strip[i + 1], flags, genMesh, vertTracker);
                        var z = AddARCVert(mdl, faceData, faceVtxl, strip[i], flags, genMesh, vertTracker);
                        genMesh.triList.Add(new Vector3(x, y, z));
                        faceVtxl.rawVertId.Add(x);
                        faceVtxl.rawVertId.Add(y);
                        faceVtxl.rawVertId.Add(z);
                        genMesh.matIdList.Add(texId);

                        genMesh.faceVerts.Add(faceVtxl);
                    }
                }
                else
                {
                    throw new System.Exception();
                }
            }
        }
        public static int AddARCVert(ARCLNDModel mdl, ARCLNDFaceDataHead faceData, AquaObject.VTXL vtxl, List<int> faceIds, ArcLndVertType flags, AquaObject.GenericTriangles genMesh, Dictionary<string, int> vertTracker)
        {
            string vertId = "";
            int i = 0;
            if ((flags & ArcLndVertType.Position) > 0)
            {
                vtxl.vertPositions.Add(mdl.arcVertDataSetList[0].PositionData[faceIds[i]]);
                vertId += ((int)ArcLndVertType.Position).ToString() + faceIds[i++];
            }
            if ((flags & ArcLndVertType.Normal) > 0)
            {
                vtxl.vertNormals.Add(mdl.arcVertDataSetList[0].NormalData[faceIds[i]]);
                vertId += ((int)ArcLndVertType.Normal).ToString() + faceIds[i++];
            } else
            {
                vtxl.vertNormals.Add(mdl.arcVertDataSetList[0].faceNormalDict[faceIds[0]]);
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

            if (addWeight)
            {
                vtxl.vertWeightIndices.Add(new int[] { 0, 0, 0, 0 });
                vtxl.vertWeights.Add(new Vector4(1, 0, 0, 0));
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
                        vtxl.vertPositions.Add(mesh.vertData.vertPositions[faceIds[vertIndexMapping[i]]]);
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

        public static LND ConvertToLND(string folderPath)
        {
            Dictionary<string, ModelData> tempModelData = new Dictionary<string, ModelData>();
            List<ModelData> modelData = new List<ModelData>();
            var files = Directory.GetFiles(folderPath);
            byte[] gvmBytes = null;

            foreach (var file in files)
            {
                if (Path.GetExtension(file) == ".gvm")
                {
                    gvmBytes = File.ReadAllBytes(file);
                }
                else if (Path.GetExtension(file) == ".fbx")
                {
                    var aqp = ModelImporter.AssimpAquaConvertFull(file, 1, true, true, out var aqn, false);
                    NGSAquaObject nightAqp = null;
                    AquaMotion aqm = null;
                    AquaNode placementAqn = null;

                    var name = Path.GetFileNameWithoutExtension(file);
                    var splitName = name.Split('+');

                    //Check our 'parameter' for special use
                    if (splitName.Length > 1)
                    {
                        switch (splitName[1].ToLower())
                        {
                            case "animation":
                                //aqm = ModelImporter.AssimpAQMConvertNoNameSingle(file, false, true, 1);
                                break;
                            case "night":
                                nightAqp = (NGSAquaObject)aqp;
                                aqp = null;
                                break;
                            case "transform":
                                placementAqn = aqn;
                                break;
                        }
                    }

                    //Retrieve or create ModelData
                    ModelData mdl;
                    if (!tempModelData.ContainsKey(splitName[0].ToLower()))
                    {
                        mdl = new ModelData();
                        mdl.name = splitName[0];
                        tempModelData.Add(splitName[0].ToLower(), mdl);
                    }
                    else
                    {
                        mdl = tempModelData[splitName[0].ToLower()];
                    }

                    //Assign data
                    if (aqp != null)
                    {
                        mdl.aqp = (NGSAquaObject)aqp;
                    }
                    if (placementAqn != null)
                    {
                        mdl.placementAqn = placementAqn;
                    }
                    if (aqm != null)
                    {
                        mdl.aqm = aqm;
                    }
                    if (nightAqp != null)
                    {
                        mdl.nightAqp = nightAqp;
                    }
                }
            }

            if (!tempModelData.ContainsKey("block"))
            {
                throw new Exception("Error, main terrain model, block, is missing!");
            }
            modelData.Add(tempModelData["block"]);
            foreach (var pair in tempModelData)
            {
                if (pair.Key != "block" && pair.Value.aqp != null)
                {
                    modelData.Add(pair.Value);
                }
            }
            tempModelData.Clear();

            return ConvertToLND(modelData, gvmBytes);
        }

        public static LND ConvertToLND(List<ModelData> modelData, byte[] gvmBytes)
        {
            LND lnd = new LND();
            lnd.gvmBytes = gvmBytes;
            lnd.isArcLND = true;
            List<ModelData> animModels = new List<ModelData>();

            //Get texture lists;
            using (Stream stream = new MemoryStream(gvmBytes))
            using (var streamReader = new BufferedStreamReader(stream, 8192))
            {
                lnd.texnames = GVMUtil.ReadGVMFileNames(streamReader);
            }

            //Get model data
            foreach (var mdl in modelData)
            {
                if (mdl.aqm == null)
                {
                    var model = AquaToLND(lnd, mdl, false, lnd.texnames);
                    if (mdl.name.ToLower() != "block")
                    {
                        model.arcAltVertColorList.Clear();
                    }
                    lnd.arcLndModels.Add(new LND.ARCLNDStaticMeshData() { name = mdl.name, model = model });
                    lnd.fileNames.Add(mdl.name);
                }
                else
                {
                    /*
                    var animModel = new LND.ARCLNDAnimatedMeshData() { model = AquaToLND(lnd, mdl, true, lnd.texnames) };
                    //animModel.motion = GetAnimatedNightColors(mdl.nightAqp);
                    //animModel.mplMotion = GetMPLMotion(mdl.aqm);
                    lnd.arcLndAnimatedMeshDataList.Add(animModel);
                    */
                }
            }
            lnd.fileNames.Add("land");
            if (lnd.arcMPL != null)
            {
                lnd.fileNames.Add("mpl");
            }
            lnd.arcLand = new LND.ARCLNDLand();

            return lnd;
        }

        public static ARCLNDModel AquaToLND(LND lnd, ModelData mdl, bool isAnimatedModel, List<string> texNames)
        {
            //Unfortunately normals disable vert colors, but they can be auto calculated by the game.
            assignNormalsOnExport = false;
            ARCLNDModel lndMdl = new ARCLNDModel();

            //While the original format has an array of arrays of these, used to separate rendering mesh types. However this doesn't seem to matter for performance? 
            List<ARCLNDMeshData> meshData = new List<ARCLNDMeshData>();
            var vertSet = new ARCLNDVertDataSet();

            lndMdl.arcVertDataSetList.Add(vertSet);
            ushort boundCount = 1;
            List<int> badMeshIndices = new List<int>();
            for(int msh = 0; msh < mdl.aqp.meshList.Count; msh++)
            {
                var mesh = mdl.aqp.meshList[msh];
                //For reassigning vertex ids in faces after they've been combined.
                //Billy stores separate indices per vertex data type so we may as well optimize them where possible.
                Dictionary<int, int> vertIndexRemapper = new Dictionary<int, int>();
                Dictionary<int, int> normalIndexRemapper = new Dictionary<int, int>();
                Dictionary<int, int> colorIndexRemapper = new Dictionary<int, int>();
                Dictionary<int, int> uvIndexRemapper = new Dictionary<int, int>();

                var mate = mdl.aqp.mateList[mesh.mateIndex];
                var matName = mdl.aqp.matUnicodeNames[mesh.mateIndex];
                var tset = mdl.aqp.tsetList[mesh.tsetIndex];
                var strips = mdl.aqp.strips[mesh.psetIndex];
                if(AquaObject.stripData.RemoveDegenerateFaces(strips.triStrips.ToArray()).Count == 0)
                {
                    badMeshIndices.Add(msh);
                    continue;
                }
                strips.toStrips(strips.triStrips.ToArray());
                var vtxl = mdl.aqp.vtxlList[mesh.vsetIndex];

                //Vertices
                for (int v = 0; v < vtxl.vertPositions.Count; v++)
                {
                    var vertData = vtxl.vertPositions[v];
                    Vector3 normalData;
                    if (vtxl.vertNormals.Count > v)
                    {
                        normalData = vtxl.vertNormals[v];
                    }
                    else
                    {
                        normalData = new Vector3(0, 1, 0);
                    }
                    byte[] colorData;
                    if (vtxl.vertColors.Count > v)
                    {
                        colorData = new byte[] { vtxl.vertColors[v][2], vtxl.vertColors[v][1], vtxl.vertColors[v][0], vtxl.vertColors[v][3] };
                    }
                    else
                    {
                        colorData = new byte[] { 0xFF, 0xFF, 0xFF, 0xFF };
                    }
                    short[] uvData = new short[2];
                    if (vtxl.uv1List.Count > v)
                    {
                        uvData = new short[] { (short)(vtxl.uv1List[v].X * 255), (short)(vtxl.uv1List[v].Y * 255) };
                    }

                    //Combine and remap repeated vertices as needed.
                    //Assimp splits things by material by necessity and so we need to recombine them.
                    bool foundDuplicateVert = false;
                    bool foundDuplicateNormal = false;
                    bool foundDuplicateColor = false;
                    bool foundDuplicateUv = false;
                    for (int vt = 0; vt < vertSet.PositionData.Count; vt++)
                    {
                        if (foundDuplicateVert == false && vertSet.PositionData.Count > vt && vertData == vertSet.PositionData[vt])
                        {
                            foundDuplicateVert = true;
                            vertIndexRemapper.Add(v, vt);
                            if (!optimizeVertColors)
                            {
                                foundDuplicateColor = true;
                                colorIndexRemapper.Add(v, vt);
                            }
                        }
                        if (assignNormalsOnExport && foundDuplicateNormal == false && vertSet.NormalData.Count > vt && normalData == vertSet.NormalData[vt])
                        {
                            foundDuplicateNormal = true;
                            normalIndexRemapper.Add(v, vt);
                        }
                        if (optimizeVertColors && foundDuplicateColor == false && vertSet.VertColorData.Count > vt && colorData[0] == vertSet.VertColorData[vt][0] && colorData[1] == vertSet.VertColorData[vt][1] && colorData[2] == vertSet.VertColorData[vt][2] && colorData[3] == vertSet.VertColorData[vt][3])
                        {
                            foundDuplicateColor = true;
                            colorIndexRemapper.Add(v, vt);
                        }
                        if (foundDuplicateUv == false && vertSet.UV1Data.Count > vt && uvData[0] == vertSet.UV1Data[vt][0] && uvData[1] == vertSet.UV1Data[vt][1])
                        {
                            foundDuplicateUv = true;
                            uvIndexRemapper.Add(v, vt);
                        }
                    }
                    if (!foundDuplicateVert)
                    {
                        vertIndexRemapper.Add(v, vertSet.PositionData.Count);
                        vertSet.PositionData.Add(vertData);
                    }
                    if (assignNormalsOnExport && !foundDuplicateNormal)
                    {
                        normalIndexRemapper.Add(v, vertSet.NormalData.Count);
                        vertSet.NormalData.Add(normalData);
                    }
                    if (!foundDuplicateColor)
                    {
                        colorIndexRemapper.Add(v, vertSet.VertColorData.Count);
                        vertSet.VertColorData.Add(colorData);
                    }
                    if (!foundDuplicateUv)
                    {
                        uvIndexRemapper.Add(v, vertSet.UV1Data.Count);
                        vertSet.UV1Data.Add(uvData);
                    }
                }

                //Strips
                var faceData = new ARCLNDFaceDataHead();
                faceData.flags = ArcLndVertType.Position | ArcLndVertType.VertColor | ArcLndVertType.UV1;
                if(assignNormalsOnExport)
                {
                    faceData.flags |= ArcLndVertType.Normal;
                }
                bool startNewStrip = true;
                var currentList = faceData.triIndicesList0;
                var currentStartList = faceData.triIndicesListStarts0;
                for (int f = 0; f < strips.triStrips.Count - 2; f++)
                {
                    if (strips.triStrips[f] == strips.triStrips[f + 1] || strips.triStrips[f] == strips.triStrips[f + 2]
                        || strips.triStrips[f + 1] == strips.triStrips[f + 2])
                    {
                        //Check the strip has data before adding the count.
                        if (startNewStrip == true || currentList[currentList.Count - 1].Count == 0)
                        {
                            continue;
                        }

                        //Since the strip has ended, add the count and set the flag
                        startNewStrip = true;
                        currentStartList[currentStartList.Count - 1][0].Add(currentList[currentList.Count - 1].Count);
                        continue;
                    }

                    //Start the strip, add the count on exit
                    if (startNewStrip)
                    {
                        //Swap lists if needed to preserve face normals
                        if ((f & 1) > 0)
                        {
                            currentList = faceData.triIndicesList0;
                            currentStartList = faceData.triIndicesListStarts0;
                        }
                        else
                        {
                            currentList = faceData.triIndicesList1;
                            currentStartList = faceData.triIndicesListStarts1;
                        }
                        startNewStrip = false;
                        currentStartList.Add(new List<List<int>>() { new List<int>() { 0x98 } });
                        currentList.Add(new List<List<int>>());
                        AssignFaceVertIds(faceData.flags, vertIndexRemapper, normalIndexRemapper, colorIndexRemapper, uvIndexRemapper, strips, currentList, f);
                        AssignFaceVertIds(faceData.flags, vertIndexRemapper, normalIndexRemapper, colorIndexRemapper, uvIndexRemapper, strips, currentList, f + 1);
                        AssignFaceVertIds(faceData.flags, vertIndexRemapper, normalIndexRemapper, colorIndexRemapper, uvIndexRemapper, strips, currentList, f + 2);
                    }
                    else
                    {
                        AssignFaceVertIds(faceData.flags, vertIndexRemapper, normalIndexRemapper, colorIndexRemapper, uvIndexRemapper, strips, currentList, f + 2);
                    }
                }
                //Set the final count of the facedata
                if (currentStartList[currentStartList.Count - 1][0].Count == 1)
                {
                    currentStartList[currentStartList.Count - 1][0].Add(currentList[currentList.Count - 1].Count);
                }
                lndMdl.arcFaceDataList.Add(faceData);

                //Bounding
                ARCLNDNodeBounding bnd = new ARCLNDNodeBounding();
                if (isAnimatedModel)
                {
                    var node = mdl.aqn.nodeList[mdl.placementAqn.nodeList.Count - 1];
                    Matrix4x4.Decompose(node.GetInverseBindPoseMatrixInverted(), out var scale, out var rot, out var pos);
                    bnd.Position = pos;
                    bnd.Scale = scale;
                    bnd.SetRotation(MathExtras.QuaternionToEuler(rot));
                    MathExtras.CalculateBoundingSphere(vtxl.vertPositions, out var center, out var rad);
                    bnd.center = center;
                    bnd.radius = rad;
                }
                else
                {
                    MathExtras.CalculateBoundingSphere(vtxl.vertPositions, out var center, out var rad);
                    bnd.center = center;
                    bnd.radius = rad;
                }
                if (boundCount != mdl.aqp.meshList.Count)
                {
                    bnd.index = boundCount;
                }
                boundCount++;
                lndMdl.arcBoundingList.Add(bnd);

                //Material
                ARCLNDMaterialEntry matEntry = new ARCLNDMaterialEntry();
                var texName = Path.GetFileNameWithoutExtension(mdl.aqp.texfList[tset.tstaTexIDs[0]].texName.GetString());
                matEntry.TextureId = texNames.IndexOf(texName);
                ARCLNDMaterialEntryRef matRef = new ARCLNDMaterialEntryRef();
                matRef.extraDataEnabled = 1;

                var flagsSplit = matName.Split('#');
                matEntry.RenderFlags = 0;
                matEntry.textureFlags = 0;
                if (flagsSplit.Length > 1)
                {
                    for (int f = 1; f < flagsSplit.Length; f++)
                    {
                        switch (flagsSplit[f].ToLower())
                        {
                            case "s_zero":
                                matEntry.sourceAlpha = AlphaInstruction.Zero;
                                break;
                            case "s_one":
                                matEntry.sourceAlpha = AlphaInstruction.One;
                                break;
                            case "s_othercolor":
                                matEntry.sourceAlpha = AlphaInstruction.OtherColor;
                                break;
                            case "s_inverseothercolor":
                                matEntry.sourceAlpha = AlphaInstruction.InverseOtherColor;
                                break;
                            case "s_sourcealpha":
                                matEntry.sourceAlpha = AlphaInstruction.SourceAlpha;
                                break;
                            case "s_inversesourcealpha":
                                matEntry.sourceAlpha = AlphaInstruction.InverseSourceAlpha;
                                break;
                            case "s_destinationalpha":
                                matEntry.sourceAlpha = AlphaInstruction.DestinationAlpha;
                                break;
                            case "s_inversedestinationalpha":
                                matEntry.sourceAlpha = AlphaInstruction.InverseDestinationAlpha;
                                break;
                            case "d_zero":
                                matEntry.destinationAlpha = AlphaInstruction.Zero;
                                break;
                            case "d_one":
                                matEntry.destinationAlpha = AlphaInstruction.One;
                                break;
                            case "d_othercolor":
                                matEntry.destinationAlpha = AlphaInstruction.OtherColor;
                                break;
                            case "d_inverseothercolor":
                                matEntry.destinationAlpha = AlphaInstruction.InverseOtherColor;
                                break;
                            case "d_sourcealpha":
                                matEntry.destinationAlpha = AlphaInstruction.SourceAlpha;
                                break;
                            case "d_inversesourcealpha":
                                matEntry.destinationAlpha = AlphaInstruction.InverseSourceAlpha;
                                break;
                            case "d_destinationalpha":
                                matEntry.destinationAlpha = AlphaInstruction.DestinationAlpha;
                                break;
                            case "d_inversedestinationalpha":
                                matEntry.destinationAlpha = AlphaInstruction.InverseDestinationAlpha;
                                break;
                            case "enablelighting":
                                matEntry.RenderFlags |= ARCLNDRenderFlags.EnableLighting;
                                break;
                            case "rfunknown0x2":
                                matEntry.RenderFlags |= ARCLNDRenderFlags.RFUnknown0x2;
                                break;
                            case "twosided":
                                matEntry.RenderFlags |= ARCLNDRenderFlags.TwoSided;
                                break;
                            case "rfunknown0x8":
                                matEntry.RenderFlags |= ARCLNDRenderFlags.RFUnknown0x8;
                                break;
                            case "renderorderthing":
                                matEntry.RenderFlags |= ARCLNDRenderFlags.renderOrderThing;
                                break;
                            case "renderorderthing2":
                                matEntry.RenderFlags |= ARCLNDRenderFlags.renderOrderThing2;
                                break;
                            case "rfunknown0x40":
                                matEntry.RenderFlags |= ARCLNDRenderFlags.RFUnknown0x40;
                                break;
                            case "rfunknown0x80":
                                matEntry.RenderFlags |= ARCLNDRenderFlags.RFUnknown0x80;
                                break;
                            case "tfunknownx0x1":
                                matEntry.textureFlags |= ARCLNDTextureFlags.TFUnknownX0x1;
                                break;
                            case "tilex":
                                matEntry.textureFlags |= ARCLNDTextureFlags.TileX;
                                break;
                            case "mirroredtilex":
                                matEntry.textureFlags |= ARCLNDTextureFlags.MirroredTileX;
                                break;
                            case "tfunknowny0x8":
                                matEntry.textureFlags |= ARCLNDTextureFlags.TFUnknownY0x8;
                                break;
                            case "tiley":
                                matEntry.textureFlags |= ARCLNDTextureFlags.TileY;
                                break;
                            case "mirroredtiley":
                                matEntry.textureFlags |= ARCLNDTextureFlags.MirroredTileY;
                                break;
                            case "tfunknown0x40":
                                matEntry.textureFlags |= ARCLNDTextureFlags.TFUnknown0x40;
                                break;
                            case "tfunknown0x80":
                                matEntry.textureFlags |= ARCLNDTextureFlags.TFUnknown0x80;
                                break;
                        }
                    }
                }
                //Default if nothing
                if(matEntry.RenderFlags == 0)
                {
                    matEntry.RenderFlags = (ARCLNDRenderFlags)0x3;
                }
                if(matEntry.textureFlags == 0)
                {
                    matEntry.textureFlags = ARCLNDTextureFlags.TileX | ARCLNDTextureFlags.TileY;
                }

                //Optimize out same materials
                int matId = -1;
                for(int i = 0; i < lndMdl.arcMatEntryList.Count; i++)
                {
                    var entry = lndMdl.arcMatEntryList[i].entry;
                    if(entry.unkBool == matEntry.unkBool && entry.unkFlags1 == matEntry.unkFlags1 && entry.textureFlags == matEntry.textureFlags && entry.RenderFlags == matEntry.RenderFlags && entry.destinationAlpha == matEntry.destinationAlpha
                        && entry.diffuseColor == matEntry.diffuseColor && entry.sourceAlpha == matEntry.sourceAlpha && entry.specularColor == matEntry.specularColor && entry.TextureId == matEntry.TextureId && entry.unkInt6 == matEntry.unkInt6
                        && entry.ushort0 == matEntry.ushort0)
                    {
                        matId = i;
                        break;
                    }
                }
                if(matId == -1)
                {
                    matRef.entry = matEntry;
                    matId = lndMdl.arcMatEntryList.Count;
                    lndMdl.arcMatEntryList.Add(matRef);
                }

                //Mesh
                ARCLNDMeshData arcMesh = new ARCLNDMeshData();
                arcMesh.matEntryId = matId;
                arcMesh.BoundingDataId = lndMdl.arcBoundingList.Count - 1;
                arcMesh.faceDataId = lndMdl.arcFaceDataList.Count - 1;
                meshData.Add(arcMesh);
            }
            lndMdl.arcMeshDataList.Add(meshData);
            lndMdl.arcMeshDataRefList.Add(new ARCLNDMeshDataRef() { count = meshData.Count, unkEnum = 1 });

            //If we're optimizing vert colors, we need to dummy out the alt vert color set since we can no longer make a normal night vert color set 
            lndMdl.arcAltVertColorList = new List<ARCLNDVertDataSet>();
            lndMdl.arcAltVertColorList.Add(new ARCLNDVertDataSet());
            if (optimizeVertColors || mdl.nightAqp == null)
            {
                lndMdl.arcAltVertColorList[0].VertColor = lndMdl.arcVertDataSetList[0].VertColor;
            }
            else
            {
                //Recycle here, it's already been assigned and we don't really want to add more to the old one
                vertSet = new ARCLNDVertDataSet();
                for (int msh = 0; msh < mdl.nightAqp.meshList.Count; msh++)
                {
                    if(badMeshIndices.Contains(msh))
                    {
                        continue;
                    }
                    var mesh = mdl.aqp.meshList[msh];
                    //For reassigning vertex ids in faces after they've been combined.
                    //Billy stores separate indices per vertex data type so we may as well optimize them where possible.
                    Dictionary<int, int> vertIndexRemapper = new Dictionary<int, int>();
                    Dictionary<int, int> colorIndexRemapper = new Dictionary<int, int>();
                    var vtxl = mdl.nightAqp.vtxlList[mesh.vsetIndex];

                    //Vertices
                    List<Vector3> posList = new List<Vector3>();
                    for (int v = 0; v < vtxl.vertPositions.Count; v++)
                    {
                        var vertData = vtxl.vertPositions[v];
                        byte[] colorData;

                        if (vtxl.vertColors.Count > v)
                        {
                            colorData = new byte[] { vtxl.vertColors[v][2], vtxl.vertColors[v][1], vtxl.vertColors[v][0], vtxl.vertColors[v][3] };
                        }
                        else
                        {
                            colorData = new byte[] { 0xFF, 0xFF, 0xFF, 0xFF };
                        }

                        //Combine and remap repeated vertices as needed.
                        //Assimp splits things by material by necessity and so we need to recombine them.
                        bool foundDuplicateVert = false;
                        bool foundDuplicateColor = false;
                        for (int vt = 0; vt < vertSet.PositionData.Count; vt++)
                        {
                            if (foundDuplicateVert == false && vertSet.PositionData.Count > vt && vertData == vertSet.PositionData[vt])
                            {
                                foundDuplicateVert = true;
                                vertIndexRemapper.Add(v, vt);
                                if (!optimizeVertColors)
                                {
                                    foundDuplicateColor = true;
                                    colorIndexRemapper.Add(v, vt);
                                }
                            }
                            if (foundDuplicateColor == false && vertSet.VertColorData.Count > vt && colorData[0] == vertSet.VertColorData[vt][0] && colorData[1] == vertSet.VertColorData[vt][1] && colorData[2] == vertSet.VertColorData[vt][2] && colorData[3] == vertSet.VertColorData[vt][3])
                            {
                                foundDuplicateColor = true;
                                colorIndexRemapper.Add(v, vt);
                            }
                        }
                        if (!foundDuplicateVert)
                        {
                            vertIndexRemapper.Add(v, vertSet.PositionData.Count);
                            vertSet.PositionData.Add(vertData);
                            posList.Add(vertData);
                        }
                        if (!foundDuplicateColor)
                        {
                            colorIndexRemapper.Add(v, vertSet.VertColorData.Count);
                            lndMdl.arcAltVertColorList[0].VertColorData.Add(colorData);
                        }
                    }

                }
            }

            return lndMdl;
        }

        private static void AssignFaceVertIds(ArcLndVertType flags, Dictionary<int, int> vertIndexRemapper, Dictionary<int, int> normalIndexRemapper, Dictionary<int, int> colorIndexRemapper, Dictionary<int, int> uvIndexRemapper, AquaObject.stripData strips, List<List<List<int>>> currentList, int f)
        {
            var vert = new List<int>();
            if ((flags & ArcLndVertType.Position) > 0)
            {
                vert.Add(vertIndexRemapper[strips.triStrips[f]]);
            }
            if ((flags & ArcLndVertType.Normal) > 0)
            {
                vert.Add(normalIndexRemapper[strips.triStrips[f]]);
            }
            if ((flags & ArcLndVertType.VertColor) > 0)
            {
                vert.Add(colorIndexRemapper[strips.triStrips[f]]);
            }
            if ((flags & ArcLndVertType.UV1) > 0)
            {
                vert.Add(uvIndexRemapper[strips.triStrips[f]]);
            }

            currentList[currentList.Count - 1].Add(vert);
        }
    }

}
