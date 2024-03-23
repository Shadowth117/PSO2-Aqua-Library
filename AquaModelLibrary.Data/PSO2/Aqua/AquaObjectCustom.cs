using AquaModelLibrary.Data.PSO2.Aqua.AquaObjectData;
using AquaModelLibrary.Data.PSO2.Aqua.AquaObjectData.Intermediary;
using AquaModelLibrary.Data.PSO2.Aqua.Presets;
using AquaModelLibrary.Data.PSO2.Aqua.Presets.Shader;
using AquaModelLibrary.Helpers.MathHelpers;
using System.Numerics;

namespace AquaModelLibrary.Data.PSO2.Aqua
{
    //This partial is intended to house methods relating to building custom PSO2 models
    public unsafe partial class AquaObject : AquaCommon
    {
        #region MeshSplitting
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
                            newTris.format0xC31 = true;
                            newTris.triIdCount = tempFaces.Count;
                            newTris.faceGroups.Add(tempFaces.Count);
                        }
                        else
                        {
                            newTris = new StripData(tempFaces.ToArray());
                            newTris.format0xC31 = false;
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

                if(localMeshNames.Count > 0 && meshNames.Count == meshList.Count)
                {
                    localMeshNames.RemoveAt(0);
                    meshNames.AddRange(localMeshNames);
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

        public void SplitMeshTempData(int meshId, List<List<int>> facesToClone)
        {
            if (facesToClone.Count > 0)
            {
                List<List<int>> boneIdList = new List<List<int>>();
                List<string> localMeshNames = new List<string>();
                List<VTXL> localVtxlList = new List<VTXL>();
                List<GenericTriangles> localTrisList = new List<GenericTriangles>();

                for (int f = 0; f < facesToClone.Count; f++)
                {
                    var referenceVTXL = vtxlList[meshId];
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
                            Vector3 face = tempTris[meshId].triList[facesToClone[f][i]];
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

                            tempFaceMatIds.Add(tempTris[meshId].matIdList[facesToClone[f][i]]);
                            tempFaces.Add(face);
                        }

                        //Remap Ids based based on the indices of the values in faceVertIds and add to outModel
                        for (int i = 0; i < tempFaces.Count; i++)
                        {
                            tempFaces[i] = new Vector3(faceVertDict[tempFaces[i].X], faceVertDict[tempFaces[i].Y], faceVertDict[tempFaces[i].Z]);
                        }

                        //Assign new tempTris
                        var newTris = new GenericTriangles(tempFaces, tempFaceMatIds);
                        newTris.baseMeshDummyId = tempTris[meshId].baseMeshDummyId;
                        newTris.baseMeshNodeId = tempTris[meshId].baseMeshNodeId;
                        newTris.name = tempTris[meshId].name;
                        localTrisList.Add(newTris);

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
                        if (meshNames.Count > meshId)
                        {
                            localMeshNames.Add(meshNames[meshId]);
                        }
                        localVtxlList.Add(vtxl);
                    }
                }

                if (localMeshNames.Count > 0 && meshNames.Count == tempTris.Count)
                {
                    localMeshNames.RemoveAt(0);
                    meshNames.AddRange(localMeshNames);
                }

                //Assign first split back to original slot, assign subsequent splits to end of the list
                for (int i = 0; i < localTrisList.Count; i++)
                {
                    if (i == 0)
                    {
                        tempTris[meshId] = localTrisList[i];
                        continue;
                    }
                    tempTris.Add(localTrisList[i]);
                }

                //If we're doing an NGS model, we can leave the vertices alone since we can recycle vertices for strips

                for (int i = 0; i < localVtxlList.Count; i++)
                {
                    //We don't reassign the original set of vertices in case another mesh is using it, even if it's unlikely.
                    if (i == 0)
                    {
                        vtxlList[meshId] = (localVtxlList[i]);
                        continue;
                    }
                    vtxlList.Add(localVtxlList[i]);
                }
            }
        }

        public void SplitByBoneCount(int meshId, int boneLimit, bool forceVtxlSplit = false)
        {
            List<List<int>> faceLists = new List<List<int>>();

            var mesh = meshList[meshId];
            var tris = strips[mesh.psetIndex].GetTriangles();
            bool[] usedFaceIds = new bool[tris.Count];
            int startFace = 0;

            while (true)
            {
                List<int> objBones = new List<int>();
                List<int> faceArray = new List<int>();

                //Find start point for this iteration
                for (int i = 0; i < usedFaceIds.Length; i++)
                {
                    if (!usedFaceIds[i])
                    {
                        startFace = i;
                        break;
                    }
                }

                List<int> edgeVertCandidates = new List<int>();
                List<int> usedVerts = new List<int>();
                for (int f = startFace; f < usedFaceIds.Length; f++)
                {
                    if (usedFaceIds[f] != true)
                    {
                        //Used to watch how many bones are being used so it can be understood where to split the meshes.
                        //These also help to track the bones being used so the edge verts list can be generated. Not sure how important that is, but the game stores it.
                        List<int> faceBones = new List<int>();
                        int newBones = 0;

                        //Get vert ids of the face
                        float[] faceIds = tris[f].AsArray();

                        //Get bones in the face
                        foreach (float v in faceIds)
                        {
                            bool vertCheck = false;
                            for (int b = 0; b < vtxlList[mesh.vsetIndex].vertWeightIndices[(int)v].Length; b++)
                            {
                                int id = vtxlList[mesh.vsetIndex].vertWeightIndices[(int)v][b];
                                if (!faceBones.Contains(id))
                                {
                                    faceBones.Add(id);
                                }

                                //If it's not in here, it's a candidate to be an edgeVert
                                if (!objBones.Contains(id))
                                {
                                    vertCheck = true;
                                }
                            }

                            //Add edge vert candidates 
                            if (vertCheck && !edgeVertCandidates.Contains((int)v))
                            {
                                edgeVertCandidates.Add((int)v);
                            }
                        }

                        //Check for bones in the face that weren't in the obj bone set yet
                        foreach (int fb in faceBones)
                        {
                            if (!objBones.Contains(fb))
                            {
                                newBones++;
                            }
                        }

                        //If enough space or no new, add to the object amount as well
                        if (newBones + objBones.Count <= boneLimit)
                        {
                            //Add new bones if there's room
                            foreach (int fb in faceBones)
                            {
                                if (!objBones.Contains(fb))
                                {
                                    objBones.Add(fb);
                                }
                            }
                            //Track used verts so we can figure out edge verts later
                            foreach (float id in faceIds)
                            {
                                if (!usedVerts.Contains((int)id))
                                {
                                    usedVerts.Add((int)id);
                                }
                            }
                            usedFaceIds[f] = true;
                            faceArray.Add(f);
                        }
                    }
                }
                //Determine what should be added to the final edge vert list
                foreach (int vert in edgeVertCandidates)
                {
                    if (!vtxlList[mesh.vsetIndex].edgeVerts.Contains((ushort)vert) && usedVerts.Contains(vert))
                    {
                        vtxlList[mesh.vsetIndex].edgeVerts.Add((ushort)vert);
                    }
                }
                faceLists.Add(faceArray);


                bool breakFromLoop = true;
                for (int i = 0; i < usedFaceIds.Length; i++)
                {
                    if (usedFaceIds[i] == false)
                    {
                        breakFromLoop = false;
                    }
                }
                if (breakFromLoop)
                {
                    break;
                }
            }

            //Split the meshes
            SplitMesh(meshId, faceLists, forceVtxlSplit);
        }

        public void SplitByBoneCountTempData(int modelId, int boneLimit)
        {
            List<List<int>> faceLists = new List<List<int>>();

            bool[] usedFaceIds = new bool[tempTris[modelId].triList.Count];
            int startFace = 0;

            while (true)
            {
                List<int> objBones = new List<int>();
                List<int> faceArray = new List<int>();

                //Find start point for this iteration
                for (int i = 0; i < usedFaceIds.Length; i++)
                {
                    if (!usedFaceIds[i])
                    {
                        startFace = i;
                        break;
                    }
                }

                List<int> edgeVertCandidates = new List<int>();
                List<int> usedVerts = new List<int>();
                for (int f = startFace; f < usedFaceIds.Length; f++)
                {
                    if (usedFaceIds[f] != true)
                    {
                        //Used to watch how many bones are being used so it can be understood where to split the meshes.
                        //These also help to track the bones being used so the edge verts list can be generated. Not sure how important that is, but the game stores it.
                        List<int> faceBones = new List<int>();
                        int newBones = 0;

                        //Get vert ids of the face
                        float[] faceIds = tempTris[modelId].triList[f].AsArray();

                        //Get bones in the face
                        foreach (float v in faceIds)
                        {
                            bool vertCheck = false;
                            for (int b = 0; b < vtxlList[modelId].vertWeightIndices[(int)v].Length; b++)
                            {
                                int id = vtxlList[modelId].vertWeightIndices[(int)v][b];
                                if (!faceBones.Contains(id))
                                {
                                    faceBones.Add(id);
                                }

                                //If it's not in here, it's a candidate to be an edgeVert
                                if (!objBones.Contains(id))
                                {
                                    vertCheck = true;
                                }
                            }

                            //Add edge vert candidates 
                            if (vertCheck && !edgeVertCandidates.Contains((int)v))
                            {
                                edgeVertCandidates.Add((int)v);
                            }
                        }

                        //Check for bones in the face that weren't in the obj bone set yet
                        foreach (int fb in faceBones)
                        {
                            if (!objBones.Contains(fb))
                            {
                                newBones++;
                            }
                        }

                        //If enough space or no new, add to the object amount as well
                        if (newBones + objBones.Count <= boneLimit)
                        {
                            //Add new bones if there's room
                            foreach (int fb in faceBones)
                            {
                                if (!objBones.Contains(fb))
                                {
                                    objBones.Add(fb);
                                }
                            }
                            //Track used verts so we can figure out edge verts later
                            foreach (float id in faceIds)
                            {
                                if (!usedVerts.Contains((int)id))
                                {
                                    usedVerts.Add((int)id);
                                }
                            }
                            usedFaceIds[f] = true;
                            faceArray.Add(f);
                        }
                    }
                }
                //Determine what should be added to the final edge vert list
                foreach (int vert in edgeVertCandidates)
                {
                    if (!vtxlList[modelId].edgeVerts.Contains((ushort)vert) && usedVerts.Contains(vert))
                    {
                        vtxlList[modelId].edgeVerts.Add((ushort)vert);
                    }
                }
                faceLists.Add(faceArray);


                bool breakFromLoop = true;
                for (int i = 0; i < usedFaceIds.Length; i++)
                {
                    if (usedFaceIds[i] == false)
                    {
                        breakFromLoop = false;
                    }
                }
                if (breakFromLoop)
                {
                    break;
                }
            }

            //Split the meshes
            SplitMeshTempData(modelId, faceLists);
        }

        public void BatchSplitByBoneCount(int boneLimit, bool forceVtxlSplit = false)
        {
            //Set up a temporary bone palette to be used in case a mesh needs it, such as for NGS meshes
            List<ushort> bonePalette = new List<ushort>();
            for (int b = 0; b < bonePalette.Count; b++)
            {
                bonePalette.Add((ushort)bonePalette[b]);
            }

            int startingMeshCount = meshList.Count;
            for (int i = 0; i < startingMeshCount; i++)
            {
                var mesh = meshList[i];
                //Make sure there's a bonepalette to pull from
                if (vtxlList[mesh.vsetIndex].bonePalette?.Count == 0)
                {
                    vtxlList[mesh.vsetIndex].bonePalette = bonePalette;
                }
                if (!IsNGS)
                {
                    OptimizeBonePalette(vtxlList[mesh.vsetIndex], new List<uint>(), IsNGS);
                }
                //Pass to splitting function if beyond the limit, otherwise pass untouched
                if (vtxlList[mesh.vsetIndex].bonePalette.Count > boneLimit)
                {
                    SplitByBoneCount(i, boneLimit, forceVtxlSplit);
                }
            }

            //Get rid of this since we don't understand it and it's probably going to confuse the game if it's not processed along with these.
            strips2.Clear();
            strips3.Clear();
            pset2List.Clear();
            mesh2List.Clear();
            strips3Lengths.Clear();
        }

        public void BatchSplitByBoneCountTempData(int boneLimit)
        {
            for (int i = 0; i < vtxlList.Count; i++)
            {
                if (!IsNGS)
                {
                    OptimizeBonePalette(vtxlList[i], new List<uint>(), IsNGS);
                }
                //Pass to splitting function if beyond the limit, otherwise pass untouched
                if (vtxlList[i].bonePalette.Count > boneLimit)
                {
                    SplitByBoneCountTempData(i, boneLimit);
                }
            }
        }

        //Seemingly not necessary on pso2 data since this split is already a requirement
        //Therefore, we only operate on temp data
        public void SplitMeshByMaterialTempData()
        {
            for (int i = 0; i < tempTris.Count; i++)
            {
                SplitMeshTempData(i, GetAllMaterialFaceGroups(i));
            }
        }

        public List<List<int>> GetAllMaterialFaceGroups(int meshId)
        {
            List<List<int>> matGroups = new List<List<int>>();
            int matCount = 0;
            if (tempMats.Count > 0)
            {
                matCount = tempMats.Count;
            }
            else
            {
                matCount = mateList.Count;
            }
            for (int i = 0; i < matCount; i++)
            {
                matGroups.Add(new List<int>());
            }

            for (int i = 0; i < tempTris[meshId].matIdList.Count; i++)
            {
                //create list of face groups based on material data. Assume material ids are preprocessed to fit what will be used for the final AquaObject.
                matGroups[tempTris[meshId].matIdList[i]].Add(i);
            }

            return matGroups;
        }
        #endregion

        #region BoneMethods
        /// <summary>
        /// Increments all bones a value, such as for if additional nodes were inserted before the existing hierarchy
        /// </summary>
        public void IncrementBones(byte value)
        {
            if(bonePalette?.Count > 0)
            {
                for (int i = 0; i < bonePalette.Count; i++)
                {
                    bonePalette[i] = bonePalette[i] + value;
                }
            }
            foreach(var vtxl in vtxlList)
            {
                for (int i = 0; i < vtxl.bonePalette.Count; i++)
                {
                    vtxl.bonePalette[i] = (ushort)(vtxl.bonePalette[i] + value);
                }
            }
        }

        /// <summary>
        /// Reconstructs globalBonePalette and optimizes local VTXL bonePalettes. Only intended for use during custom model creation!
        /// </summary>>
        public void OptimizeBonePalettes()
        {
            //Consider it NGS style globalBonePalettes if we have anything assigned to the globalBonePalette
            if (bonePalette.Count > 0)
            {
                bonePalette = OptimizeBonePaletteGroup(vtxlList, bonePalette);
            } else
            {
                foreach(var vtxl in vtxlList)
                {
                    var newBonePalette = OptimizeBonePaletteGroup(new List<VTXL> { vtxl }, vtxl.bonePalette.ConvertAll(bone => (uint)bone));
                    vtxl.bonePalette = newBonePalette.ConvertAll(bone => (ushort)bone);
                }
            }
        }

        //Returns the newly constructed bone palette after altering the vtxl(s) to fit it
        private static List<uint> OptimizeBonePaletteGroup(List<VTXL> vtxlList, List<uint> originalBonePalette)
        {
            List<uint> globalBonePalette = new List<uint>();
            Dictionary<int, int> globalBonePaletteMap = new Dictionary<int, int>();

            //Gather bones that are actually used
            foreach (var vtxl in vtxlList)
            {
                //Loop through weight indices and gather them
                for (int v = 0; v < vtxl.vertWeightIndices.Count; v++)
                {
                    for (int vi = 0; vi < vtxl.vertWeightIndices[v].Length; vi++)
                    {
                        var originalBoneId = originalBonePalette[vtxl.vertWeightIndices[v][vi]];
                        if (!globalBonePalette.Contains((uint)originalBoneId))
                        {
                            globalBonePalette.Add((uint)originalBoneId);
                        }
                    }
                }
            }
            globalBonePalette.Sort();

            //Create map for the new bone list back to the old one
            for (int i = 0; i < originalBonePalette.Count; i++)
            {
                uint boneId = originalBonePalette[i];
                if (globalBonePalette.Contains(boneId))
                {
                    globalBonePaletteMap.Add(i, globalBonePalette.IndexOf(boneId));
                }
            }

            //Reassign indices
            foreach (var vtxl in vtxlList)
            {
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
                            vtxl.vertWeightIndices[v][vi] = globalBonePaletteMap[vtxl.vertWeightIndices[v][vi]];
                        }
                    }
                }
            }

            return globalBonePalette;
        }

        /// <summary>
        /// Reconstructs globalBonePalette and optimizes local VTXL bonePalette. newStyleBonePalette should be false for classic models, objc.type < 0xC32, only
        /// </summary>>
        public static void OptimizeBonePalette(VTXL vtxl, List<uint> globalBonePalette, bool newStyleBonePalette)
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
                            if (!newStyleBonePalette)
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

            if (!newStyleBonePalette)
            {
                vtxl.bonePalette = newBonePalette.ConvertAll(x => (ushort)x);
            }
        }
        #endregion

        #region TangentSpaceComputation
        //Adapted from this: https://forums.cgsociety.org/t/finding-bi-normals-tangents/975005/8 
        //Binormals and tangents for each face are calculated and each face's values for a particular vertex are summed and averaged for the result before being normalized
        //Though vertex position is used, due to the nature of the normalization applied during the process, resizing is unneeded.
        //This function expects that binormals and tangents have both either not been populated or both have been populated in particular vertex sets. The game always does both or neither.
        //Normals will also be generated if non existent because it needs those too
        public void ComputeTangentSpace(bool useFaceNormals, bool flipUv = false)
        {
            List<List<Vector3>> faces = new List<List<Vector3>>();
            Vector3[][] vertBinormalArrays = new Vector3[vtxlList.Count][];
            Vector3[][] vertTangentArrays = new Vector3[vtxlList.Count][];
            Vector3[][] vertFaceNormalArrays = new Vector3[vtxlList.Count][];
            int uvSign = 1;
            if (flipUv == true)
            {
                uvSign = -1;
            }

            //Get faces depending on model state
            if (strips.Count > 0)
            {
                foreach (var tris in strips)
                {
                    faces.Add(tris.GetTriangles(true));
                }
            }
            else
            {
                foreach (var tris in tempTris)
                {
                    faces.Add(tris.triList);
                }
            }

            //Clear before calcing
            for (int i = 0; i < vtxlList.Count; i++)
            {
                vtxlList[i].vertTangentList.Clear();
                vtxlList[i].vertTangentListNGS.Clear();
                vtxlList[i].vertBinormalList.Clear();
                vtxlList[i].vertBinormalListNGS.Clear();
            }

            //Loop through faces and sum the calculated data for each vertice's faces
            for (int meshIndex = 0; meshIndex < faces.Count; meshIndex++)
            {
                int vsetIndex;
                int psetIndex;
                //Unlike older aqo variants, NGS models can have a different vsetIndex than their
                if ((objc.type >= 0xc31) && meshList.Count > 0)
                {
                    vsetIndex = meshList[meshIndex].vsetIndex;
                    psetIndex = meshList[meshIndex].psetIndex;
                }
                else
                {
                    vsetIndex = meshIndex;
                    psetIndex = meshIndex;
                }

                //Check if it's null or not since we don't want to overwrite what's there. NGS meshes can share vertex sets
                if (vertBinormalArrays[vsetIndex] == null)
                {
                    vertBinormalArrays[vsetIndex] = new Vector3[vtxlList[vsetIndex].vertPositions.Count];
                    vertTangentArrays[vsetIndex] = new Vector3[vtxlList[vsetIndex].vertPositions.Count];
                    vertFaceNormalArrays[vsetIndex] = new Vector3[vtxlList[vsetIndex].vertPositions.Count];
                }

                foreach (var face in faces[psetIndex])
                {
                    List<int> faceIndices = new List<int>()
                        {
                            (int)face.X,
                            (int)face.Y,
                            (int)face.Z
                        };

                    List<Vector3> verts = new List<Vector3>()
                        {
                          vtxlList[vsetIndex].vertPositions[(int)face.X],
                          vtxlList[vsetIndex].vertPositions[(int)face.Y],
                          vtxlList[vsetIndex].vertPositions[(int)face.Z]
                        };
                    List<Vector2> uvs;
                    if (vtxlList[vsetIndex].uv1List.Count > 0)
                    {
                        uvs = new List<Vector2>()
                        {
                          new Vector2(vtxlList[vsetIndex].uv1List[(int)face.X].X, uvSign * vtxlList[vsetIndex].uv1List[(int)face.X].Y),
                          new Vector2(vtxlList[vsetIndex].uv1List[(int)face.Y].X, uvSign * vtxlList[vsetIndex].uv1List[(int)face.Y].Y),
                          new Vector2(vtxlList[vsetIndex].uv1List[(int)face.Z].X, uvSign * vtxlList[vsetIndex].uv1List[(int)face.Z].Y)
                        };
                    }
                    else
                    {
                        uvs = new List<Vector2>()
                        {
                            new Vector2(0, 0),
                            new Vector2(0, 0),
                            new Vector2(0, 0)
                        };
                    }

                    Vector3 dV1 = verts[0] - verts[1];
                    Vector3 dV2 = verts[0] - verts[2];

                    Vector2 dUV1 = uvs[0] - uvs[1];
                    Vector2 dUV2 = uvs[0] - uvs[2];

                    float area = dUV1.X * dUV2.Y - dUV1.Y * dUV2.X;
                    int sign = 1;
                    if (area < 0)
                    {
                        sign = -1;
                    }

                    Vector3 tangent = new Vector3(0, 0, 1);
                    tangent.X = dV1.X * dUV2.Y - dUV1.Y * dV2.X;
                    tangent.Y = dV1.Y * dUV2.Y - dUV1.Y * dV2.Y;
                    tangent.Z = dV1.Z * dUV2.Y - dUV1.Y * dV2.Z;
                    tangent = Vector3.Normalize(tangent) * sign;

                    //Calculate face normal
                    Vector3 u = verts[1] - verts[0];
                    Vector3 v = verts[2] - verts[0];

                    Vector3 normal = Vector3.Normalize(Vector3.Cross(u, v));
                    Vector3 binormal = Vector3.Normalize(Vector3.Cross(normal, tangent)) * sign;

                    //Create or add vectors to the collections
                    for (int i = 0; i < 3; i++)
                    {
                        vertBinormalArrays[vsetIndex][faceIndices[i]] += binormal;
                        vertTangentArrays[vsetIndex][faceIndices[i]] += tangent;
                        vertFaceNormalArrays[vsetIndex][faceIndices[i]] += normal;
                    }
                }
            }

            //Loop through vsets and verts and assign these first so that verts that aren't linked get assigned.
            //Then, we can use UNRMs calculations to fix verts that were split in exporting
            bool[] vertNormalsCheck = new bool[vtxlList.Count];
            for (int i = 0; i < vertNormalsCheck.Length; i++)
            {
                vertNormalsCheck[i] = true;
            }

            for (int vsetIndex = 0; vsetIndex < vertBinormalArrays.Length; vsetIndex++)
            {
                int vertCount = vtxlList[vsetIndex].vertPositions.Count;

                if (vtxlList[vsetIndex].vertBinormalList == null || vtxlList[vsetIndex].vertBinormalList.Count != vertCount)
                {
                    vtxlList[vsetIndex].vertBinormalList = new List<Vector3>(new Vector3[vertCount]);
                    vtxlList[vsetIndex].vertTangentList = new List<Vector3>(new Vector3[vertCount]);
                }
                //Add normals if they aren't there or if we want to regenerate them
                if (vtxlList[vsetIndex].vertNormals == null || vtxlList[vsetIndex].vertNormals.Count == 0)
                {
                    vertNormalsCheck[vsetIndex] = false;
                    vtxlList[vsetIndex].vertNormals = new List<Vector3>(new Vector3[vertCount]);
                }
                for (int vert = 0; vert < vertBinormalArrays[vsetIndex].Length; vert++)
                {
                    vtxlList[vsetIndex].vertBinormalList[vert] = Vector3.Normalize(vertBinormalArrays[vsetIndex][vert]);
                    vtxlList[vsetIndex].vertTangentList[vert] = Vector3.Normalize(vertTangentArrays[vsetIndex][vert]);

                    if (vertNormalsCheck[vsetIndex] == false || useFaceNormals == true)
                    {
                        vtxlList[vsetIndex].vertNormals[vert] = Vector3.Normalize(vertFaceNormalArrays[vsetIndex][vert]);
                    }
                }
            }

            //Hack since for now we don't convert back from legacy types properly
            int biggest = 0;
            for (int i = 0; i < vsetList.Count; i++)
            {
                int vertSize;
                var vset = vsetList[i];
                if (objc.type >= 0xC32)
                {
                    vtxeList[vsetList[i].vtxeCount] = VTXE.ConstructFromVTXL(vtxlList[i], out vertSize);
                }
                else
                {
                    vtxeList[i] = VTXE.ConstructFromVTXL(vtxlList[i], out vertSize);
                    vset.vtxeCount = vtxeList[i].vertDataTypes.Count;
                }
                vset.vertDataSize = vertSize;
                vsetList[i] = vset;
                if (vertSize > biggest)
                {
                    biggest = vertSize;
                }
            }
            objc.largetsVtxl = biggest;
        }

        //Calculates tangent space using tempMesh data
        public static void ComputeTangentSpaceTempMesh(AquaObject model, bool useFaceNormals)
        {
            for (int mesh = 0; mesh < model.tempTris.Count; mesh++)
            {
                var vertBinormalArray = new Vector3[model.vtxlList[mesh].vertPositions.Count];
                var vertTangentArray = new Vector3[model.vtxlList[mesh].vertPositions.Count];
                var vertFaceNormalArray = new Vector3[model.vtxlList[mesh].vertPositions.Count];
                var faces = model.tempTris[mesh].triList;

                foreach (var face in faces)
                {
                    List<int> faceIndices = new List<int>()
                        {
                            (int)face.X,
                            (int)face.Y,
                            (int)face.Z
                        };

                    List<Vector3> verts = new List<Vector3>()
                        {
                          model.vtxlList[mesh].vertPositions[(int)face.X],
                          model.vtxlList[mesh].vertPositions[(int)face.Y],
                          model.vtxlList[mesh].vertPositions[(int)face.Z]
                        };
                    //We expect model UVs to be flipped when brought into .NET. But we have to flip them back for these calculations...
                    List<Vector2> uvs = new List<Vector2>()
                        {
                          new Vector2(model.vtxlList[mesh].uv1List[(int)face.X].X, -model.vtxlList[mesh].uv1List[(int)face.X].Y),
                          new Vector2(model.vtxlList[mesh].uv1List[(int)face.Y].X, -model.vtxlList[mesh].uv1List[(int)face.Y].Y),
                          new Vector2(model.vtxlList[mesh].uv1List[(int)face.Z].X, -model.vtxlList[mesh].uv1List[(int)face.Z].Y)
                        };

                    Vector3 dV1 = verts[0] - verts[1];
                    Vector3 dV2 = verts[0] - verts[2];

                    Vector2 dUV1 = uvs[0] - uvs[1];
                    Vector2 dUV2 = uvs[0] - uvs[2];

                    float area = dUV1.X * dUV2.Y - dUV1.Y * dUV2.X;
                    int sign = 1;
                    if (area < 0)
                    {
                        sign = -1;
                    }

                    Vector3 tangent = new Vector3(0, 0, 1);
                    tangent.X = dV1.X * dUV2.Y - dUV1.Y * dV2.X;
                    tangent.Y = dV1.Y * dUV2.Y - dUV1.Y * dV2.Y;
                    tangent.Z = dV1.Z * dUV2.Y - dUV1.Y * dV2.Z;
                    tangent = Vector3.Normalize(tangent) * sign;

                    //Calculate face normal
                    Vector3 u = verts[1] - verts[0];
                    Vector3 v = verts[2] - verts[0];

                    Vector3 normal = Vector3.Normalize(Vector3.Cross(u, v));
                    Vector3 binormal = Vector3.Normalize(Vector3.Cross(normal, tangent)) * sign;

                    //Create or add vectors to the collections
                    for (int i = 0; i < 3; i++)
                    {
                        vertBinormalArray[faceIndices[i]] += binormal;
                        vertTangentArray[faceIndices[i]] += tangent;
                        vertFaceNormalArray[faceIndices[i]] += normal;
                    }
                }

                //Normalize and assign values
                for (int i = 0; i < vertBinormalArray.Length; i++)
                {
                    vertBinormalArray[i] = Vector3.Normalize(vertBinormalArray[i]);
                    vertTangentArray[i] = Vector3.Normalize(vertTangentArray[i]);

                    model.vtxlList[mesh].vertBinormalList.Add(vertBinormalArray[i]);
                    model.vtxlList[mesh].vertTangentList.Add(vertTangentArray[i]);

                    if (useFaceNormals)
                    {
                        vertFaceNormalArray[i] = Vector3.Normalize(vertFaceNormalArray[i]);

                        model.vtxlList[mesh].vertNormals.Add(vertFaceNormalArray[i]);
                    }
                }
            }
        }

        //Requires a fully assigned model
        public static void ComputeTangentSpaceOld(AquaObject model, bool useFaceNormals)
        {
            for (int mesh = 0; mesh < model.vsetList.Count; mesh++)
            {
                //Verify that there are vertices and with valid UVs
                int vsetIndex = model.meshList[mesh].vsetIndex;
                int psetIndex = model.meshList[mesh].psetIndex;

                model.vtxlList[vsetIndex].vertTangentList.Clear();
                model.vtxlList[vsetIndex].vertTangentListNGS.Clear();
                model.vtxlList[vsetIndex].vertBinormalList.Clear();
                model.vtxlList[vsetIndex].vertBinormalListNGS.Clear();
                if (model.vtxlList[vsetIndex].uv1List.Count > 0 && model.vtxlList[vsetIndex].vertPositions.Count > 0)
                {
                    Vector3[] vertBinormalArray = new Vector3[model.vtxlList[vsetIndex].vertPositions.Count];
                    Vector3[] vertTangentArray = new Vector3[model.vtxlList[vsetIndex].vertPositions.Count];
                    Vector3[] vertFaceNormalArray = new Vector3[model.vtxlList[vsetIndex].vertPositions.Count];
                    List<Vector3> faces = model.strips[psetIndex].GetTriangles(true);

                    for (int f = 0; f < faces.Count; f++)
                    {
                        Vector3 face = faces[f];
                        List<int> faceIndices = new List<int>()
                        {
                            (int)face.X,
                            (int)face.Y,
                            (int)face.Z
                        };

                        List<Vector3> verts = new List<Vector3>()
                        {
                          model.vtxlList[vsetIndex].vertPositions[(int)face.X],
                          model.vtxlList[vsetIndex].vertPositions[(int)face.Y],
                          model.vtxlList[vsetIndex].vertPositions[(int)face.Z]
                        };
                        List<Vector2> uvs = new List<Vector2>()
                        {
                          new Vector2(model.vtxlList[vsetIndex].uv1List[(int)face.X].X, -model.vtxlList[vsetIndex].uv1List[(int)face.X].Y),
                          new Vector2(model.vtxlList[vsetIndex].uv1List[(int)face.Y].X, -model.vtxlList[vsetIndex].uv1List[(int)face.Y].Y),
                          new Vector2(model.vtxlList[vsetIndex].uv1List[(int)face.Z].X, -model.vtxlList[vsetIndex].uv1List[(int)face.Z].Y)
                        };

                        Vector3 dV1 = verts[0] - verts[1];
                        Vector3 dV2 = verts[0] - verts[2];
                        Vector2 dUV1 = uvs[0] - uvs[1];
                        Vector2 dUV2 = uvs[0] - uvs[2];

                        float area = dUV1.X * dUV2.Y - dUV1.Y * dUV2.X;
                        int sign;
                        if (area < 0)
                        {
                            sign = -1;
                        }
                        else
                        {
                            sign = 1;
                        }

                        Vector3 tangent = new Vector3();
                        tangent.X = dV1.X * dUV2.Y - dUV1.Y * dV2.X;
                        tangent.Y = dV1.Y * dUV2.Y - dUV1.Y * dV2.Y;
                        tangent.Z = dV1.Z * dUV2.Y - dUV1.Y * dV2.Z;
                        tangent = Vector3.Normalize(tangent) * sign;

                        //Calculate face normal
                        Vector3 u = verts[1] - verts[0];
                        Vector3 v = verts[2] - verts[0];

                        Vector3 normal = Vector3.Normalize(Vector3.Cross(u, v));
                        Vector3 binormal = Vector3.Normalize(Vector3.Cross(normal, tangent)) * sign;

                        //Create or add vectors to the collections
                        for (int i = 0; i < 3; i++)
                        {
                            vertBinormalArray[faceIndices[i]] += binormal;
                            vertTangentArray[faceIndices[i]] += tangent;
                            vertFaceNormalArray[faceIndices[i]] += normal;
                        }
                    }

                    //Average out the values for use ingame
                    for (int vert = 0; vert < model.vtxlList[vsetIndex].vertPositions.Count; vert++)
                    {
                        vertBinormalArray[vert] = Vector3.Normalize(vertBinormalArray[vert]);
                        vertTangentArray[vert] = Vector3.Normalize(vertTangentArray[vert]);
                        vertFaceNormalArray[vert] = Vector3.Normalize(vertFaceNormalArray[vert]);
                    }

                    //Set these back in the model
                    if (useFaceNormals)
                    {
                        model.vtxlList[vsetIndex].vertNormals = vertFaceNormalArray.ToList();
                    }
                    model.vtxlList[vsetIndex].vertBinormalList = vertBinormalArray.ToList();
                    model.vtxlList[vsetIndex].vertTangentList = vertTangentArray.ToList();
                }
            }
        }
        #endregion

        #region MaterialBuilding
        //Used to generate materials like the various player materials in which the actual texture names are not used.
        public static void GenerateSpecialMaterialParameters(GenericMaterial mat)
        {
            List<string> texNames = new List<string>();
            switch (mat.specialType)
            {
                case "ey":
                case "eye":
                    if (mat.shaderNames == null)
                    {
                        mat.shaderNames = new List<string>() { "0106p", "0106" };
                    }
                    texNames.Add("pl_eye_diffuse.dds");
                    texNames.Add("pl_eye_multi.dds");
                    texNames.Add("pl_eye_env.dds");
                    texNames.Add("pl_leye_env.dds");
                    break;
                case "hr":
                case "hair":
                    //Fun fact, hair just seemingly doesn't note the other extra stuff it uses and the game figures it out at runtime. 
                    //If the texlist seems too short, that's why.
                    if (mat.shaderNames == null)
                    {
                        mat.shaderNames = new List<string>() { "0103p", "0103" };
                    }
                    texNames.Add("pl_hair_diffuse.dds");
                    texNames.Add("pl_hair_multi.dds");
                    texNames.Add("pl_hair_normal.dds");
                    break;
                case "fc":
                case "face":
                    if (mat.shaderNames == null)
                    {
                        mat.shaderNames = new List<string>() { "0101p", "0101" };
                    }
                    texNames.Add("pl_face_diffuse.dds");
                    texNames.Add("pl_face_multi.dds");
                    texNames.Add("pl_face_normal.dds");
                    break;
                case "pl":
                case "player":
                case "bd":
                case "ba":
                case "ou":
                    if (mat.shaderNames == null)
                    {
                        mat.shaderNames = new List<string>() { "0100p", "0100" };
                    }
                    texNames.Add("pl_body_diffuse.dds");
                    texNames.Add("pl_body_multi.dds");
                    texNames.Add("pl_body_decal.dds");
                    texNames.Add("pl_body_normal.dds");
                    break;
                case "rbd":
                case "reboot_bd":
                case "reboot_pl":
                case "reboot_player":
                case "reboot_ba":
                    if (mat.shaderNames == null)
                    {
                        mat.shaderNames = new List<string>() { "1102p", "1102" };
                    }
                    texNames.Add("pl_body_base_diffuse.dds");
                    texNames.Add("pl_body_base_multi.dds");
                    texNames.Add("pl_body_base_normal.dds");
                    texNames.Add("pl_body_base_mask.dds");
                    texNames.Add("pl_body_base_subnormal_01.dds");
                    texNames.Add("pl_body_base_subnormal_02.dds");
                    texNames.Add("pl_body_base_subnormal_03.dds");
                    break;
                case "rbd_d":
                case "reboot_bd_d":
                case "reboot_pl_d":
                case "reboot_player_d":
                case "reboot_ba_decal":
                    if (mat.shaderNames == null)
                    {
                        mat.shaderNames = new List<string>() { "1102p", "1102" };
                    }
                    texNames.Add("pl_body_base_diffuse.dds");
                    texNames.Add("pl_body_base_multi.dds");
                    texNames.Add("pl_body_base_normal.dds");
                    texNames.Add("pl_body_base_mask.dds");
                    texNames.Add("pl_body_base_subnormal_01.dds");
                    texNames.Add("pl_body_base_subnormal_02.dds");
                    texNames.Add("pl_body_base_subnormal_03.dds");
                    texNames.Add("pl_body_decal.dds");
                    break;
                case "rhr":
                case "reboot_hair":
                    if (mat.shaderNames == null)
                    {
                        mat.shaderNames = new List<string>() { "1103p", "1103" };
                    }
                    texNames.Add("pl_hair_diffuse.dds");
                    texNames.Add("pl_hair_multi.dds");
                    texNames.Add("pl_hair_normal.dds");
                    texNames.Add("pl_hair_alpha.dds");
                    texNames.Add("pl_hair_noise.dds");
                    break;
                case "rbd_sk":
                case "rbd_skin":
                case "reboot_bd_skin":
                case "reboot_pl_skin":
                case "reboot_player_skin":
                case "reboot_ba_skin":
                case "reboot_ou_skin":
                    if (mat.shaderNames == null)
                    {
                        mat.shaderNames = new List<string>() { "1102p", "1102" };
                    }
                    texNames.Add("pl_body_skin_diffuse.dds");
                    texNames.Add("pl_body_skin_multi.dds");
                    texNames.Add("pl_body_skin_normal.dds");
                    texNames.Add("pl_body_skin_mask01.dds");
                    texNames.Add("pl_body_skin_subnormal_01.dds");
                    texNames.Add("pl_body_skin_subnormal_02.dds");
                    texNames.Add("pl_body_skin_subnormal_03.dds");
                    break;
                case "rbd_ou":
                case "reboot_ou":
                case "reboot_outer":
                    texNames.Add("pl_body_outer_diffuse.dds");
                    texNames.Add("pl_body_outer_multi.dds");
                    texNames.Add("pl_body_outer_normal.dds");
                    texNames.Add("pl_body_outer_mask.dds");
                    texNames.Add("pl_body_outer_subnormal_01.dds");
                    texNames.Add("pl_body_outer_subnormal_02.dds");
                    texNames.Add("pl_body_outer_subnormal_03.dds");
                    break;
                case "rbd_ou_d":
                case "rbd_ou_decal":
                case "reboot_outer_decal":
                    texNames.Add("pl_body_outer_diffuse.dds");
                    texNames.Add("pl_body_outer_multi.dds");
                    texNames.Add("pl_body_outer_normal.dds");
                    texNames.Add("pl_body_outer_mask.dds");
                    texNames.Add("pl_body_outer_subnormal_01.dds");
                    texNames.Add("pl_body_outer_subnormal_02.dds");
                    texNames.Add("pl_body_outer_subnormal_03.dds");
                    texNames.Add("pl_body_decal.dds");
                    break;
                default:
                    break;
            }
            mat.texNames = texNames;
        }


        //Used to generate standard materials in which the texture names are used. 
        public void GenerateMaterial(GenericMaterial mat, bool ngsMat = false)
        {
            if (mat.specialType != null)
            {
                GenerateSpecialMaterialParameters(mat);
            }
            if (mat.shaderNames == null || mat.shaderNames.Count < 2)
            {
                mat.shaderNames = AQOConstants.DefaultShaderNames;
            }
            int texArrayStartIndex = tstaList.Count;
            List<int> tempTexIds = new List<int>();
            TSET tset = new TSET();
            MATE mate = new MATE();
            SHAD shad = new SHAD();
            REND rend = new REND();
            shad.isNGS = ngsMat;

            //Set up textures
            var shaderKey = $"{mat.shaderNames[0]} {mat.shaderNames[1]}";
            if (PSO2ShaderTexSetPresets.shaderTexSet.ContainsKey(shaderKey))
            {
                Dictionary<string, int> setTracker = new Dictionary<string, int>();
                var set = PSO2ShaderTexSetPresets.shaderTexSet[shaderKey];
                var info = PSO2ShaderTexInfoPresets.tstaTexSet[shaderKey];
                string firstTexName; //Base tex string in the case we need to generate the others
                if (mat.texNames?.Count > 0)
                {
                    firstTexName = mat.texNames[0];
                    if (firstTexName.Contains("_"))
                    {
                        int _index = -1;
                        for (int i = firstTexName.Length - 1; i > 0; i--)
                        {
                            if (firstTexName[i] == '_')
                            {
                                _index = i;
                                break;
                            }
                        }
                        if (_index == 0)
                        {
                            firstTexName = "";
                        }
                        else if (_index != -1)
                        {
                            firstTexName = firstTexName.Substring(0, _index);
                        }
                    }
                }
                else
                {
                    firstTexName = "tex";
                }
                for (int i = 0; i < set.Count; i++)
                {
                    var tex = set[i];
                    string curTexStr = "";
                    if (setTracker.ContainsKey(tex))
                    {
                        int curNum = setTracker[tex] = setTracker[tex] + 1;
                        curTexStr = tex + curNum.ToString();
                    }
                    else
                    {
                        curTexStr = tex + "0";
                        setTracker.Add(tex, 0);
                    }

                    TEXF texf = new TEXF();
                    TSTA tsta = new TSTA();
                    if (info.ContainsKey(curTexStr))
                    {
                        tsta = info[curTexStr];
                    }
                    else
                    {
                        if (TSTATypePresets.tstaTypeDict.ContainsKey(curTexStr))
                        {
                            tsta = TSTATypePresets.tstaTypeDict[curTexStr];
                        }
                        else
                        {
                            tsta = TSTATypePresets.defaultPreset;
                        }
                    }
                    string texName;
                    if (mat.texNames?.Count > i)
                    {
                        tsta.texName.SetString(mat.texNames[i]);
                        texf.texName.SetString(mat.texNames[i]);
                        texName = mat.texNames[i];
                    }
                    else
                    {
                        texName = firstTexName + "_" + tex + ".dds";
                        tsta.texName.SetString(texName);
                        texf.texName.SetString(texName);
                    }

                    //Make sure the texf list only has unique entries
                    int texId = -1;
                    bool isUniqueTexname = true;
                    for (int t = 0; t < texfList.Count; t++)
                    {
                        var tempTex = texfList[t];
                        if (tempTex.texName == texf.texName)
                        {
                            isUniqueTexname = false;
                        }
                    }
                    if (isUniqueTexname)
                    {
                        texfList.Add(texf);
                        texFUnicodeNames.Add(texName);
                    }

                    //Cull full duplicates of tsta
                    bool isUniqueTsta = true;
                    for (int t = 0; t < tstaList.Count; t++)
                    {
                        var tempTex = tstaList[t];
                        if (tempTex == tsta)
                        {
                            texId = t;
                            isUniqueTsta = false;
                        }
                    }
                    if (isUniqueTsta)
                    {
                        tstaList.Add(tsta);
                    }
                    tempTexIds.Add(texId == -1 ? texfList.Count - 1 : texId);
                }
            }
            else
            {
                if (mat.texNames != null)
                {
                    for (int i = 0; i < mat.texNames.Count; i++)
                    {
                        bool foundCopy = false;
                        for (int texIndex = 0; texIndex < texfList.Count; texIndex++)
                        {
                            if (mat.texNames[i].Equals(texfList[texIndex].texName.GetString()))
                            {
                                tempTexIds.Add(texIndex);
                                foundCopy = true;
                                break;
                            }
                        }

                        if (foundCopy == true)
                        {
                            continue;
                        }

                        TEXF texf = new TEXF();
                        TSTA tsta = new TSTA();

                        texf.texName.SetString(mat.texNames[i]);

                        tsta.tag = 0x16; //Reexamine this one when possible, these actually vary a bit in 0xC33 variants.
                                         //NGS does some funny things with the tex usage order int
                        if (mat.texNames[i].Contains("subnormal_"))
                        {
                            tsta.texUsageOrder = 0xA;
                        }
                        else
                        {
                            tsta.texUsageOrder = texArrayStartIndex + i;
                        }
                        if (mat.texUVSets == null)
                        {
                            tsta.modelUVSet = 0;
                        }
                        else
                        {
                            tsta.modelUVSet = mat.texUVSets[i];
                        }
                        var unkVector0 = new Vector3();
                        if (ngsMat == true)
                        {
                            unkVector0.Z = 1;
                        }

                        tsta.unkVector0 = unkVector0;
                        tsta.unkFloat2 = 0;
                        tsta.unkFloat3 = 0;
                        tsta.unkFloat4 = 0;
                        tsta.unkInt3 = 1;
                        tsta.unkInt4 = 1;
                        tsta.unkInt5 = 1;
                        tsta.unkFloat0 = 0f;
                        tsta.unkFloat1 = 0f;
                        tsta.texName = texf.texName;

                        texfList.Add(texf);
                        texFUnicodeNames.Add(mat.texNames[i]);
                        tstaList.Add(tsta);
                        tempTexIds.Add(texfList.Count - 1);
                    }
                }
            }

            //Set up texture set.
            tset.tstaTexIDs = tempTexIds;
            tset.texCount = tempTexIds.Count;

            //Set up material
            mate.diffuseRGBA = mat.diffuseRGBA;
            mate.unkRGBA0 = mat.unkRGBA0;
            mate._sRGBA = mat._sRGBA;
            mate.unkRGBA1 = mat.unkRGBA1;
            mate.reserve0 = mat.reserve0;
            mate.unkFloat0 = mat.unkFloat0;
            mate.unkFloat1 = mat.unkFloat1;
            mate.unkInt0 = mat.unkInt0;
            mate.unkInt1 = mat.unkInt1;
            if (mat.blendType == null || mat.blendType == "")
            {
                mat.blendType = "blendalpha";
            }
            mate.alphaType.SetString(mat.blendType);
            mate.matName.SetString(mat.matName);

            //Set up SHAD
            string key = mat.shaderNames[0] + " " + mat.shaderNames[1];
            shad.pixelShader.SetString(mat.shaderNames[0]);
            shad.vertexShader.SetString(mat.shaderNames[1]);

            //Only in NGS shaders, but in theory could come up in others. Otherwise 0. No idea what this is.
            if (NGSShaderUnk0ValuesPresets.ShaderUnk0Values.TryGetValue(key, out var unk0Val))
            {
                shad.unk0 = unk0Val;
            }

            if (ngsMat)
            {
                var ngsShad = shad;
                if (NGSShaderDetailPresets.NGSShaderDetail.TryGetValue(key, out var detailVal))
                {
                    ngsShad.shadDetail = detailVal;
                    ngsShad.shadDetailOffset = 1;
                }
                if (NGSShaderExtraPresets.NGSShaderExtra.TryGetValue(key, out var extraVal))
                {
                    ngsShad.shadExtra = extraVal;
                    ngsShad.shadExtraOffset = 1;
                }
            }

            //Set up REND
            rend.tag = 0x1FF;
            rend.unk0 = 3;
            rend.twosided = mat.twoSided;

            rend.sourceAlpha = 5;
            rend.destinationAlpha = 6;
            rend.unk3 = 1;
            rend.unk4 = 0;

            rend.unk5 = 5;
            rend.unk6 = 6;
            rend.unk7 = 1;
            //rend.unk8 = 1;

            rend.unk9 = 5;
            rend.alphaCutoff = mat.alphaCutoff;
            rend.unk11 = 1;
            rend.unk12 = 4;
            rend.unk13 = 1;

            switch (mat.blendType)
            {
                case "add":
                    rend.unk0 = 1;
                    rend.int_0C = 1;
                    rend.unk8 = 1;
                    rend.destinationAlpha = 2;
                    break;
                case "opaque":
                    rend.int_0C = 0;
                    rend.unk8 = 0;
                    break;
                case "hollow":
                    rend.int_0C = 0;
                    rend.unk8 = 1;
                    rend.twosided = 2;
                    break;
                case "blendalpha":
                default:
                    rend.int_0C = 1;
                    rend.unk8 = 1;
                    break;
            }

            tsetList.Add(tset);
            mateList.Add(mate);
            shadList.Add(shad);
            rendList.Add(rend);
            matUnicodeNames.Add(mat.matName);
        }

        public static string RemoveBlenderAndAssimpStringArtifacts(string name)
        {
            if (name.Length < 5)
            {
                return name;
            }
            if (name[name.Length - 4] == '.')
            {
                return name.Substring(0, name.Length - 4);
            }
            if (name[name.Length - 1] == ')')
            {
                if (name.Contains('('))
                {
                    int index = name.Length - 1;
                    for (int i = name.Length - 1; i > 0; i--)
                    {
                        if (name[i] == '(')
                        {
                            index = i;
                            break;
                        }
                    }
                    return name.Substring(0, index);
                }
            }
            return name;
        }

        public static void GetMaterialNameData(ref string name, ref List<string> shaderNames, out string alphaType, out string playerFlag, out int twoSided, out int alphaCutoff)
        {
            name = RemoveBlenderAndAssimpStringArtifacts(name);
            shaderNames = new List<string>();
            alphaType = null;
            alphaCutoff = 0;
            playerFlag = null;
            twoSided = 0;
            if (name.Contains('|'))
            {
                return;
            }

            //Get shader names
            string[] nameArr = name.Split(')');
            if (nameArr.Length > 1)
            {
                string shaderText = nameArr[0].Split('(')[1];
                var shaderSet = shaderText.Split(',');
                if (shaderSet.Length >= 2)
                {
                    shaderNames.Add(shaderSet[0]);
                    shaderNames.Add(shaderSet[1]);
                    name = nameArr[1];
                }
            }

            //Get alpha type
            nameArr = name.Split('}');
            if (nameArr.Length > 1)
            {
                alphaType = nameArr[0].Split('{')[1];
                name = nameArr[1];
            }

            //Get player flags
            nameArr = name.Split(']');
            if (nameArr.Length > 1)
            {
                playerFlag = nameArr[0].Split('[')[1];
                name = nameArr[1];
            }

            //Get two-sided and alphaCutoff
            nameArr = name.Split('@');
            name = nameArr[0];
            if (nameArr.Length > 1)
            {
                for (int i = 1; i < nameArr.Length; i++)
                {
                    switch (i)
                    {
                        case 1:
                            twoSided = Int32.Parse(nameArr[i]);
                            break;
                        case 2:
                            alphaCutoff = Int32.Parse(nameArr[i]);
                            break;
                    }
                }
            }
        }
        #endregion

        #region ModelBuilding
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
        /// <param name="forceClassicBonePalette">Force the model to use the classic bone palette</param>
        public void ConvertToPSO2Model(bool rebootModel, bool useUnrms, bool baHack, bool useBiTangent, bool zeroBounds, bool useRigid, bool splitVerts = true, bool useHighCountFaces = false, bool condenseMaterials = true, bool forceClassicBonePalette = false)
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
                    vtxl.ProcessToPSO2Weights(rebootModel && !forceClassicBonePalette);
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
                if (!rebootModel)
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
                newStrips.format0xC31 = IsNGS;
                if (newStrips.format0xC31)
                {
                    newStrips.triStrips = tempTris[i].toUshortArray().ToList();
                    //Hack for specific situations. Should be removed if PSO2 ever clearly and officially adds support for this
                    if (useHighCountFaces && newStrips.triStrips.Count > 65536)
                    {
                        newStrips.largeTriSet = tempTris[i].triList;
                    }
                    newStrips.triIdCount = newStrips.triStrips.Count;
                    newStrips.faceGroups.Add(newStrips.triStrips.Count);
                }
                else
                {
                    newStrips.toStrips(tempTris[i].toUshortArray());
                }
                strips.Add(newStrips);

                //PSET
                var pset = new PSET();
                pset.tag = 0x1000;
                pset.faceGroupCount = 0x1;
                pset.psetFaceCount = newStrips.triIdCount;
                if (newStrips.format0xC31)
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

                if (IsNGS)
                {
                    vset.vtxeCount = vtxeList.Count - 1;
                    vset.vtxlStartVert = vertCounter;
                    vertCounter += vset.vtxlCount;
                }
                else
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
                    if (IsNGS)
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
            if (IsNGS)
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

            if (IsNGS)
            {
                objc.unkMeshValue = 0x30053; //Taken from pl_rbd_100000
                objc.size = 0xF0;
                objc.fBlock0 = -1;
                objc.fBlock1 = -1;
                objc.fBlock2 = -1;
                objc.fBlock3 = -1;
                objc.globalStrip3LengthCount = 1;
                objc.unkCount3 = 1;
                objc.bonePaletteOffset = forceClassicBonePalette ? 0 : 1;
            }
            else
            {
                objc.size = 0xA4;
                objc.unkMeshValue = 0x17;
            }
            if (!zeroBounds)
            {
                objc.bounds = new BoundingVolume(vtxlList);
            }

            foreach(var vtxl in vtxlList)
            {
                for(int n = 0; n < vtxl.vertNormals.Count; n++)
                {
                    vtxl.vertNormals[n] = Vector3.Normalize(vtxl.vertNormals[n]);
                }
            }

            if (useBiTangent)
            {
                ComputeTangentSpace(false, true);
            }
        }
        #endregion
    }
}
