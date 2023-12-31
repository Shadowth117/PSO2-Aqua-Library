using AquaModelLibrary.Data.PSO2.Aqua.AquaObjectData;
using AquaModelLibrary.Data.PSO2.Aqua.AquaObjectData.Intermediary;
using System.Numerics;

namespace AquaModelLibrary.Data.PSO2.Aqua
{
    public unsafe partial class AquaObject : AquaCommon
    {
        //Uses mesh id unlike the temp data variation so that it can work with retail models
        public void SplitMesh(int meshId, List<List<int>> facesToClone, bool forceVtxlSplit = false)
        {
            if (facesToClone.Count > 0)
            {
                var mesh = meshList[meshId];
                var meshFaces = strips[mesh.psetIndex].GetTriangles();
                var referenceVTXL = vtxlList[mesh.vsetIndex];
                List<VTXL> localVtxlList = new List<VTXL>();
                List<string> localMeshNames = new List<string>();
                List<StripData> localFaceList = new List<StripData>();
                List<PSET> localPsetList = new List<PSET>();
                for (int f = 0; f < facesToClone.Count; f++)
                {
                    Dictionary<float, int> faceVertDict = new Dictionary<float, int>();
                    List<int> faceVertIds = new List<int>();
                    List<int> boneIds = new List<int>();
                    List<ushort> tempFaces = new List<ushort>();

                    int vertIndex = 0;
                    if (facesToClone[f].Count > 0)
                    {
                        //Get vert ids
                        for (int i = 0; i < facesToClone[f].Count; i++)
                        {
                            Vector3 face = meshFaces[facesToClone[f][i]];
                            if (!faceVertIds.Contains((int)face.X))
                            {
                                faceVertDict.Add(face.X, vertIndex++);
                                faceVertIds.Add((int)face.X);

                                if (referenceVTXL.vertWeightIndices.Count > 0)
                                {
                                    for (int bi = 0; bi < referenceVTXL.vertWeightIndices[(int)face.X].Length; bi++)
                                    {
                                        if (!boneIds.Contains(referenceVTXL.vertWeightIndices[(int)face.X][bi]))
                                        {
                                            boneIds.Add(referenceVTXL.vertWeightIndices[(int)face.X][bi]);
                                        }
                                    }
                                }
                            }
                            if (!faceVertIds.Contains((int)face.Y))
                            {
                                faceVertDict.Add(face.Y, vertIndex++);
                                faceVertIds.Add((int)face.Y);
                                if (referenceVTXL.vertWeightIndices.Count > 0)
                                {
                                    for (int bi = 0; bi < referenceVTXL.vertWeightIndices[(int)face.Y].Length; bi++)
                                    {
                                        if (!boneIds.Contains(referenceVTXL.vertWeightIndices[(int)face.Y][bi]))
                                        {
                                            boneIds.Add(referenceVTXL.vertWeightIndices[(int)face.Y][bi]);
                                        }
                                    }
                                }
                            }
                            if (!faceVertIds.Contains((int)face.Z))
                            {
                                faceVertDict.Add(face.Z, vertIndex++);
                                faceVertIds.Add((int)face.Z);
                                if (referenceVTXL.vertWeightIndices.Count > 0)
                                {
                                    for (int bi = 0; bi < referenceVTXL.vertWeightIndices[(int)face.Z].Length; bi++)
                                    {
                                        if (!boneIds.Contains(referenceVTXL.vertWeightIndices[(int)face.Z][bi]))
                                        {
                                            boneIds.Add(referenceVTXL.vertWeightIndices[(int)face.Z][bi]);
                                        }
                                    }
                                }
                            }

                            tempFaces.Add((ushort)face.X);
                            tempFaces.Add((ushort)face.Y);
                            tempFaces.Add((ushort)face.Z);
                        }

                        //Remap Ids based based on the indices of the values in faceVertIds and add to outModel
                        for (int i = 0; i < tempFaces.Count; i++)
                        {
                            tempFaces[i] = (ushort)faceVertDict[tempFaces[i]];
                        }

                        //Assign new stripdata
                        StripData newTris;
                        if (objc.type >= 0xC31)
                        {
                            newTris = new StripData();
                            newTris.triStrips = tempFaces;
                            newTris.format0xC33 = true;
                            newTris.triIdCount = tempFaces.Count;
                            newTris.faceGroups.Add(tempFaces.Count);
                        }
                        else
                        {
                            newTris = new StripData(tempFaces.ToArray());
                            newTris.format0xC33 = false;
                        }
                        localFaceList.Add(newTris);

                        //PSET
                        var pset = new PSET();
                        if (objc.type >= 0xC31)
                        {
                            pset.tag = 0x1000;
                        }
                        else
                        {
                            pset.tag = 0x2100;
                        }
                        pset.faceGroupCount = 0x1;
                        pset.psetFaceCount = tempFaces.Count;
                        localPsetList.Add(pset);

                        //Copy vertex data based on faceVertIds ordering
                        VTXL vtxl = new VTXL();
                        for (int i = 0; i < faceVertIds.Count; i++)
                        {
                            VTXL.AppendVertex(referenceVTXL, vtxl, faceVertIds[i]);
                        }

                        //Add things that aren't linked to the vertex ids
                        if (referenceVTXL.bonePalette != null)
                        {
                            vtxl.bonePalette.AddRange(referenceVTXL.bonePalette);
                        }
                        if (meshNames.Count > meshId)
                        {
                            localMeshNames.Add(meshNames[meshId] + $"_{f}");
                        }
                        OptimizeBonePalette(vtxl, new List<uint>(), IsNGS);
                        localVtxlList.Add(vtxl);
                    }
                }

                //Assign first split back to original slot, assign subsequent splits to end of the list
                for (int i = 0; i < localFaceList.Count; i++)
                {
                    if (i == 0)
                    {
                        strips[mesh.psetIndex] = localFaceList[i];
                        continue;
                    }
                    var newMesh = mesh;
                    newMesh.psetIndex = strips.Count;
                    if (objc.type < 0xC32 || forceVtxlSplit)
                    {
                        newMesh.vsetIndex = vsetList.Count + i - 1;
                    }
                    strips.Add(localFaceList[i]);
                    psetList.Add(localPsetList[i]);
                    meshList.Add(newMesh);
                }

                //If we're doing an NGS model, we can leave the vertices alone since we can recycle vertices for strips
                if (objc.type < 0xC32 || forceVtxlSplit)
                {
                    var vset0 = vsetList[mesh.vsetIndex];
                    vset0.vtxlCount = localVtxlList[0].vertPositions.Count;
                    vsetList[mesh.vsetIndex] = vset0;
                    vtxlList[mesh.vsetIndex] = localVtxlList[0];

                    for (int i = 0; i < localVtxlList.Count; i++)
                    {
                        //We don't reassign the original set of vertices in case another mesh is using it, even if it's unlikely.
                        if (i == 0)
                        {
                            continue;
                        }
                        vtxlList.Add(localVtxlList[i]);
                        vtxeList.Add(vtxeList[mesh.vsetIndex]);
                        vsetList.Add(vset0);
                    }
                }

                //We want to copy the mesh data aside from the vertex set reference and pset data
                for (int i = 0; i < localFaceList.Count; i++)
                {
                    if (i == 0)
                    {
                        continue;
                    }
                    var meshInstance = mesh;
                    if (objc.type < 0xC32)
                    {
                        meshInstance.vsetIndex = meshList.Count;
                    }
                    meshInstance.psetIndex = meshList.Count;
                    meshList.Add(meshInstance);
                }

                //Update stripStartCounts for psets
                var totalStripsShorts = 0;
                for (int i = 0; i < psetList.Count; i++)
                {
                    var pset = psetList[i];
                    pset.stripStartCount = totalStripsShorts;
                    psetList[i] = pset;
                    totalStripsShorts += psetList[i].psetFaceCount;
                }
            }


        }

        public static void SplitMeshTempData(AquaObject model, int modelId, List<List<int>> facesToClone)
        {
            if (facesToClone.Count > 0)
            {
                List<List<int>> boneIdList = new List<List<int>>();
                List<string> splitMeshNames = new List<string>();
                List<VTXL> splitVtxlList = new List<VTXL>();
                List<GenericTriangles> splitTrisList = new List<GenericTriangles>();

                for (int f = 0; f < facesToClone.Count; f++)
                {
                    var referenceVTXL = model.vtxlList[modelId];
                    Dictionary<float, int> faceVertDict = new Dictionary<float, int>();
                    List<int> faceVertIds = new List<int>();
                    List<int> tempFaceMatIds = new List<int>();
                    List<int> boneIds = new List<int>();
                    List<Vector3> tempFaces = new List<Vector3>();

                    int vertIndex = 0;
                    if (facesToClone[f].Count > 0)
                    {
                        //Get vert ids
                        for (int i = 0; i < facesToClone[f].Count; i++)
                        {
                            Vector3 face = model.tempTris[modelId].triList[facesToClone[f][i]];
                            if (!faceVertIds.Contains((int)face.X))
                            {
                                faceVertDict.Add(face.X, vertIndex++);
                                faceVertIds.Add((int)face.X);

                                if (referenceVTXL.vertWeightIndices.Count > 0)
                                {
                                    for (int bi = 0; bi < referenceVTXL.vertWeightIndices[(int)face.X].Length; bi++)
                                    {
                                        if (!boneIds.Contains(referenceVTXL.vertWeightIndices[(int)face.X][bi]))
                                        {
                                            boneIds.Add(referenceVTXL.vertWeightIndices[(int)face.X][bi]);
                                        }
                                    }
                                }
                            }
                            if (!faceVertIds.Contains((int)face.Y))
                            {
                                faceVertDict.Add(face.Y, vertIndex++);
                                faceVertIds.Add((int)face.Y);
                                if (referenceVTXL.vertWeightIndices.Count > 0)
                                {
                                    for (int bi = 0; bi < referenceVTXL.vertWeightIndices[(int)face.Y].Length; bi++)
                                    {
                                        if (!boneIds.Contains(referenceVTXL.vertWeightIndices[(int)face.Y][bi]))
                                        {
                                            boneIds.Add(referenceVTXL.vertWeightIndices[(int)face.Y][bi]);
                                        }
                                    }
                                }
                            }
                            if (!faceVertIds.Contains((int)face.Z))
                            {
                                faceVertDict.Add(face.Z, vertIndex++);
                                faceVertIds.Add((int)face.Z);
                                if (referenceVTXL.vertWeightIndices.Count > 0)
                                {
                                    for (int bi = 0; bi < referenceVTXL.vertWeightIndices[(int)face.Z].Length; bi++)
                                    {
                                        if (!boneIds.Contains(referenceVTXL.vertWeightIndices[(int)face.Z][bi]))
                                        {
                                            boneIds.Add(referenceVTXL.vertWeightIndices[(int)face.Z][bi]);
                                        }
                                    }
                                }
                            }

                            tempFaceMatIds.Add(model.tempTris[modelId].matIdList[facesToClone[f][i]]);
                            tempFaces.Add(face);
                        }

                        //Remap Ids based based on the indices of the values in faceVertIds and add to outModel
                        for (int i = 0; i < tempFaces.Count; i++)
                        {
                            tempFaces[i] = new Vector3(faceVertDict[tempFaces[i].X], faceVertDict[tempFaces[i].Y], faceVertDict[tempFaces[i].Z]);
                        }

                        //Assign new tempTris
                        var newTris = new GenericTriangles(tempFaces, tempFaceMatIds);
                        newTris.baseMeshDummyId = model.tempTris[modelId].baseMeshDummyId;
                        newTris.baseMeshNodeId = model.tempTris[modelId].baseMeshNodeId;
                        newTris.name = model.tempTris[modelId].name;
                        splitTrisList.Add(newTris);

                        //Copy vertex data based on faceVertIds ordering
                        VTXL vtxl = new VTXL();
                        for (int i = 0; i < faceVertIds.Count; i++)
                        {
                            VTXL.AppendVertex(referenceVTXL, vtxl, faceVertIds[i]);
                        }

                        //Add things that aren't linked to the vertex ids
                        if (referenceVTXL.bonePalette != null)
                        {
                            vtxl.bonePalette.AddRange(referenceVTXL.bonePalette);
                        }
                        boneIdList.Add(boneIds);
                        if (model.meshNames.Count > modelId)
                        {
                            splitMeshNames.Add(model.meshNames[modelId]);
                        }
                        splitVtxlList.Add(vtxl);
                    }
                }
                model.meshNames = splitMeshNames;
                model.vtxlList = splitVtxlList;
                model.tempTris = splitTrisList;
            }
        }

        /// <summary>
        /// Reconstructs globalBonePalette and optimizes local VTXL bonePalettes.
        /// </summary>>
        public void OptimizeBonepalettes()
        {
            List<uint> globalBonePalette = new List<uint>();
            for (int i = 0; i < vtxlList.Count; i++)
            {
                if (objc.type < 0xC32)
                {
                    globalBonePalette = new List<uint>();
                }
                OptimizeBonePalette(vtxlList[i], globalBonePalette, IsNGS);
            }
        }

        /// <summary>
        /// Reconstructs globalBonePalette and optimizes local VTXL bonePalette. StoreInVTXL should be true for classic models, objc.type < 0xC32, only
        /// </summary>>
        public static void OptimizeBonePalette(VTXL vtxl, List<uint> globalBonePalette, bool storeInVtxl)
        {
            List<int> newBonePalette = new List<int>();

            //Loop through weight indices and gather them as they're stored
            for (int v = 0; v < vtxl.vertWeightIndices.Count; v++)
            {
                List<int> usedIds = new List<int>();
                for (int vi = 0; vi < vtxl.vertWeightIndices[v].Length; vi++)
                {
                    //Repeat ids shouldn't exist and should be 0ed. Usually a duplicate implies that the original was 0 anyways.
                    if (usedIds.Contains(vtxl.vertWeightIndices[v][vi]))
                    {
                        vtxl.vertWeightIndices[v][vi] = 0;
                    }
                    else
                    {
                        usedIds.Add(vtxl.vertWeightIndices[v][vi]);
                        if (!globalBonePalette.Contains((uint)vtxl.vertWeightIndices[v][vi]))
                        {
                            globalBonePalette.Add((uint)vtxl.vertWeightIndices[v][vi]);
                            if (storeInVtxl)
                            {
                                newBonePalette.Add(vtxl.bonePalette[vtxl.vertWeightIndices[v][vi]]);
                            }
                            vtxl.vertWeightIndices[v][vi] = (globalBonePalette.Count - 1);
                        }
                        else
                        {
                            vtxl.vertWeightIndices[v][vi] = globalBonePalette.IndexOf((uint)vtxl.vertWeightIndices[v][vi]);
                        }
                    }
                }
            }

            if (storeInVtxl)
            {
                vtxl.bonePalette = newBonePalette.ConvertAll(x => (ushort)x);
            }
        }


        /// <summary>
        /// Temp material, vtxlList data or tempTri vertex data, and temptris are expected to be populated prior to this process. This should ALWAYS be run before any write attempts.
        /// High count faces is a hack for data storage. NGS at this time does NOT support face counts greater than 65536!
        /// </summary>
        /// <param name="rebootModel">Determines if model is set up for export as a reboot or classic pso2 model</param>
        /// <param name="useUnrms">Determines if unrms are generated and stored. Unrms are seemingly related to vertices which are split during export.</param>
        /// <param name="baHack">Used for models like basewear where some dummy ids are more meaningful</param>
        /// <param name="useBiTangent">Whether to generate tangent and bitangent data. </param>
        /// <param name="zeroBounds">Hack to set bounding to 0</param>
        /// <param name="useRigid">Whether the model should be written as a rigid model or not</param>
        /// <param name="splitVerts">Whether to allow the program to split meshes as needed or to leave and possibly break them.</param>
        /// <param name="useHighCountFaces">Mainly for using this as an intermediary for other games. This allows more than 65536 faces in a mesh</param>
        /// <param name="condenseMaterials">Whether to optimize material data or not.</param>
        public void ConvertToPSO2Model(bool rebootModel, bool useUnrms, bool baHack, bool useBiTangent, bool zeroBounds, bool useRigid, bool splitVerts = true, bool useHighCountFaces = false, bool condenseMaterials = true)
        {
            objc = new OBJC();
            objc.type = rebootModel ? 0xC33 : 0xC2A;
            int totalStripsShorts = 0;
            int totalVerts = 0;

            //Assemble vtxlList
            if (vtxlList == null || vtxlList.Count == 0)
            {
                VTXLFromFaceVerts();
            }
            //Fix weights
            if (useRigid == false)
            {
                foreach (var vtxl in vtxlList)
                {
                    vtxl.processToPSO2Weights(true);
                }
            }
            else
            {
                bonePalette.Clear();
                for (int v = 0; v < vtxlList.Count; v++)
                {
                    vtxlList[v].bonePalette.Clear();
                    vtxlList[v].vertWeights.Clear();
                    vtxlList[v].vertWeightsNGS.Clear();
                    vtxlList[v].vertWeightIndices.Clear();
                }
            }

            //Reindex materials if needed
            for (int mesh = 0; mesh < tempTris.Count; mesh++)
            {
                var tempMesh = tempTris[mesh];
                if (tempMesh.matIdDict.Count > 0)
                {
                    for (int face = 0; face < tempMesh.matIdList.Count; face++)
                    {
                        tempMesh.matIdList[face] = tempMesh.matIdDict[tempMesh.matIdList[face]];
                    }
                }
            }

            if (splitVerts == true)
            {
                SplitMeshByMaterialTempData();
            }
            if (useRigid == false)
            {
                if(!rebootModel)
                {
                    BatchSplitByBoneCount(16);
                }
                OptimizeBonePalettes();
            }
            if (splitVerts)
            {
                CalcUNRMs(applyNormalAveraging, useUnrms);
            }

            //Set up materials and related data
            if (mateList.Count == 0)
            {
                for (int mat = 0; mat < tempMats.Count; mat++)
                {
                    GenerateMaterial(tempMats[mat], true);
                }
            }

            //Find and condense materials with same name. These would have been split due to render/shader differences
            List<MATE> newMateList = new List<MATE>();
            Dictionary<int, int> newMatReferences = new Dictionary<int, int>();
            if (condenseMaterials)
            {
                Dictionary<string, List<int>> matDict = new Dictionary<string, List<int>>();
                var oldMatArr = mateList.ToArray();
                for (int mat = 0; mat < mateList.Count; mat++)
                {
                    var matName = mateList[mat].matName.GetString();
                    if (matDict.ContainsKey(matName))
                    {
                        matDict[matName].Add(mat);
                    }
                    else
                    {
                        matDict.Add(matName, new List<int>() { mat });
                    }
                }
                foreach (var pair in matDict)
                {
                    newMateList.Add(oldMatArr[pair.Value[0]]);
                    foreach (var val in pair.Value)
                    {
                        newMatReferences.Add(val, newMateList.Count - 1);
                    }
                }
                mateList = newMateList;
            }

            //Set up PSETs and strips, and other per mesh data
            for (int i = 0; i < tempTris.Count; i++)
            {
                //strips
                var newStrips = new StripData();
                newStrips.format0xC33 = IsNGS;
                if(newStrips.format0xC33)
                {
                    newStrips.triStrips = tempTris[i].toUshortArray().ToList();
                    //Hack for specific situations. Should be removed if PSO2 ever clearly and officially adds support for this
                    if (useHighCountFaces && newStrips.triStrips.Count > 65536)
                    {
                        newStrips.largeTriSet = tempTris[i].triList;
                    }
                    newStrips.triIdCount = newStrips.triStrips.Count;
                    newStrips.faceGroups.Add(newStrips.triStrips.Count);
                } else
                {
                    newStrips.toStrips(tempTris[i].toUshortArray());
                }
                strips.Add(newStrips);

                //PSET
                var pset = new PSET();
                pset.tag = 0x1000;
                pset.faceGroupCount = 0x1;
                pset.psetFaceCount = newStrips.triIdCount;
                if(newStrips.format0xC33)
                {
                    pset.stripStartCount = totalStripsShorts;
                }
                psetList.Add(pset);
                totalStripsShorts += newStrips.triIdCount;   //Update this *after* setting the strip start count so that we don't direct to bad data.

                //MESH
                var mesh = new MESH();
                mesh.flags = 0x17; //No idea what this really does. Seems to vary a lot, but also not matter a lot.
                mesh.unkShort0 = 0x0;
                mesh.unkByte0 = 0x80;
                mesh.unkByte1 = 0x64;
                mesh.unkShort1 = 0;
                mesh.mateIndex = tempTris[i].matIdList[0];
                mesh.rendIndex = mesh.mateIndex;
                mesh.shadIndex = mesh.mateIndex;
                mesh.tsetIndex = mesh.mateIndex;
                mesh.baseMeshNodeId = tempTris[i].baseMeshNodeId;
                mesh.vsetIndex = i;
                mesh.psetIndex = i;
                if (baHack)
                {
                    mesh.baseMeshDummyId = 0;
                }
                else
                {
                    mesh.baseMeshDummyId = tempTris[i].baseMeshDummyId;
                }
                mesh.unkInt0 = 0;
                mesh.reserve0 = 0;

                meshList.Add(mesh);
            }

            if (condenseMaterials)
            {
                //Finalize material assignment fixes
                for (int msh = 0; msh < meshList.Count; msh++)
                {
                    var mesh = meshList[msh];
                    mesh.mateIndex = newMatReferences[mesh.mateIndex];
                    meshList[msh] = mesh;
                }
            }

            //Generate VTXEs and VSETs
            int largestVertSize = 0;
            int vertCounter = 0;
            for (int i = 0; i < vtxlList.Count; i++)
            {
                totalVerts += vtxlList[i].vertPositions.Count;
                VTXE vtxe = VTXE.ConstructFromVTXL(vtxlList[i], out int size);
                vtxeList.Add(vtxe);

                //Track this for objc
                if (size > largestVertSize)
                {
                    largestVertSize = size;
                }

                VSET vset = new VSET();
                vset.vtxlCount = vtxlList[i].vertPositions.Count;

                if(IsNGS)
                {
                    vset.vtxeCount = vtxeList.Count - 1;
                    vset.vtxlStartVert = vertCounter;
                    vertCounter += vset.vtxlCount;
                    vtxlList[i].bonePalette.Sort();
                } else
                {
                    vset.vertDataSize = size;
                    vset.vtxeCount = vtxe.vertDataTypes.Count;
                }
                if (useRigid == true)
                {
                    vset.bonePaletteCount = 0;
                }
                else
                {
                    if(IsNGS)
                    {
                        vset.bonePaletteCount = -1; //Needs more research. This maybe works as a catch all for now?
                                                    //This value seems similar to the largest index in the used indices + 1 and then made negative for NGS models
                    }
                    else
                    {
                        vset.bonePaletteCount = vtxlList[i].bonePalette.Count;
                    }
                }
                vset.edgeVertsCount = vtxlList[i].edgeVerts.Count;
                vsetList.Add(vset);
            }

            //Set sizes based on VTXE results
            if(IsNGS)
            {
                for (int i = 0; i < vsetList.Count; i++)
                {
                    var vset = vsetList[i];
                    vset.vertDataSize = largestVertSize;
                    vsetList[i] = vset;
                }
            }

            //Finish OBJC
            objc.largetsVtxl = largestVertSize;
            objc.totalStripFaces = totalStripsShorts;
            objc.totalVTXLCount = totalVerts;
            objc.unkStructCount = vtxlList.Count;
            objc.vsetCount = vsetList.Count;
            objc.psetCount = psetList.Count;
            objc.meshCount = meshList.Count;
            objc.mateCount = mateList.Count;
            objc.rendCount = rendList.Count;
            objc.shadCount = shadList.Count;
            objc.tstaCount = tstaList.Count;
            objc.tsetCount = tsetList.Count;
            objc.texfCount = texfList.Count;
            objc.vtxeCount = vtxeList.Count;

            if(IsNGS)
            {
                objc.unkMeshValue = 0x30053; //Taken from pl_rbd_100000
                objc.size = 0xF0;
                objc.fBlock0 = -1;
                objc.fBlock1 = -1;
                objc.fBlock2 = -1;
                objc.fBlock3 = -1;
                objc.globalStrip3LengthCount = 1;
                objc.unkCount3 = 1;
                objc.bonePaletteOffset = 1;
            } else {
                objc.size = 0xA4;
                objc.unkMeshValue = 0x17;
            }
            if (!zeroBounds)
            {
                objc.bounds = GenerateBounding(vtxlList);
            }

            if (useBiTangent)
            {
                ComputeTangentSpace(false, true);
            }
        }

    }
}
